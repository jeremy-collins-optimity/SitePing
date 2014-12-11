using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;
using System.Net.Mail;
using System.Diagnostics;

namespace Matrix.SitePing.Domain
{
    public class SiteChecker
    {
        private ILog mLog;
        private IConfig mConfig;
        private IEnumerable<IWebSiteElement> mSites;
        private System.Timers.Timer mTimer;
        private DateTime mNextExecutionTime;
        private IDictionary<string, DateTime> mFailedSites;

        public bool Active { get; private set; }

        /// <summary>
        /// A failed check will always send an email and write to the log file. Supply an event handler
        /// if you wish to perform other actions when a site is down.
        /// </summary>
        public event EventHandler<SiteEventArgs> CheckFailed;

        public SiteChecker(IConfig config, ILog log)
        {
            this.mConfig = config;
            this.mLog = log;
            this.mSites = this.mConfig.WebSites;
            
            mNextExecutionTime = DateTime.Now;
            mFailedSites = new Dictionary<string, DateTime>();

            SendStartupMessage();
        }        

        public void Start()
        {
            this.Active = true;
            if ((this.mConfig.CheckOnStartup) && (InActiveTimeWindow()))
            {
                CheckSites();
            }
            InitTimer();
        }

        public void Stop()
        {
            this.Active = false;
            this.mTimer.Enabled = false;
        }

        private void InitTimer()
        {
            mTimer = new System.Timers.Timer();
            mTimer.Interval = 60 * 1000;
            mTimer.Elapsed += new System.Timers.ElapsedEventHandler(mTimer_Elapsed);
            mTimer.Enabled = true;
        }

        private void mTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if ((DateTime.Now >= this.mNextExecutionTime) && (InActiveTimeWindow()))
            {
                CheckSites();
            }
        }

        private bool InActiveTimeWindow()
        {
            TimeSpan ts = DateTime.Now.TimeOfDay;
            return (ts >= this.mConfig.NotificationStartTime) && (ts <= this.mConfig.NotificationEndTime);
        }

        private bool CheckSites()
        {
            bool success = true;
            foreach (IWebSiteElement el in this.mSites)
            {
                if (el.Enabled)
                {
                    success = CheckSite(el) && success;
                }
            }
            
            mNextExecutionTime = DateTime.Now.AddMinutes(this.mConfig.CheckFrequency);
            mLog.AddLogEntry(String.Format("Next test at {0:dd/MM/yyyy HH:mm}", mNextExecutionTime));
            mLog.SeparatorLine();

            return success;
        }

        private bool CheckSite(IWebSiteElement el)
        {
            bool success = false;

            mLog.AddLogEntry(String.Format("Testing {0} ({1})...", el.Name, el.Url));
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(el.Url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                byte[] buffer = new byte[8192];
                //StringBuilder sb = new StringBuilder();

                int count = 0;
                int totalBytes = 0;

                do
                {
                    count = responseStream.Read(buffer, 0, buffer.Length);
                    totalBytes += count;

                    //if (count > 0)
                    //{
                    //    sb.Append(Encoding.ASCII.GetString(buffer, 0, count));
                    //}
                }                
                while (count > 0);

                if ((totalBytes == 0) && (this.mConfig.ZeroBytesIsFailure))
                {
                    throw new ApplicationException("Zero bytes returned from site");
                }                

                HandleIsOnline(el, totalBytes);

                success = true;
            }
            catch (Exception ex)
            {
                HandleFailure(el, ex);             
            }

            mLog.NewLine();
            
            return success;            
        }

        private void HandleIsOnline(IWebSiteElement el, int bytesReceived)
        {
            mLog.AddLogEntry(String.Format("Site OK - {0} bytes received.", bytesReceived));

            if (mFailedSites.ContainsKey(el.Name))
            {
                mFailedSites.Remove(el.Name);
                string subject = String.Format("Site back up - {0}", el.Name);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("The website \"{0}\" at {1} was previously unreachable - it is now back online ({2} bytes received).",
                    el.Name, el.Url, bytesReceived);
                SendEmail(el.EmailNotificationList, subject, sb.ToString(), MailPriority.High);
            }
        }

        private void HandleFailure(IWebSiteElement el, Exception ex)
        {
            bool sendFailure = true;

            // if we've already sent a failure message, we can specify an interval before a repeat failure message is sent
            if (this.mFailedSites.ContainsKey(el.Name))
            {
                if (DateTime.Now > this.mFailedSites[el.Name].AddMinutes(this.mConfig.RepeatMessageInterval))
                {
                    this.mFailedSites.Remove(el.Name);
                }
                else
                {
                    mLog.AddLogEntry(String.Format("Site {0} still down but notification skipped due to repeat message interval", el.Name));
                    sendFailure = false;
                }
            }

            if (sendFailure)
            {
                mLog.LogError(ex, el.Url);
                SendEmail(el, ex);

                if (this.CheckFailed != null)
                {
                    SiteEventArgs se = new SiteEventArgs(el);
                    this.CheckFailed(this, se);
                }

                // record when this failure message was generated
                this.mFailedSites.Add(el.Name, DateTime.Now);
            }
        }

        private void SendEmail(IWebSiteElement el, Exception ex)
        {
            string subject;
            StringBuilder sb = new StringBuilder();
            
            if (ex is WebException)
            {
                subject = String.Format("SITE DOWN - {0}", el.Name);
                sb.AppendFormat("The web site at {0} is not responding - please investigate immediately.\n\n", el.Url);
            }
            else
            {
                subject = String.Format("Site check error - {0}", el.Name);
                sb.AppendFormat("An error occurred when checking the website at {0} - please investigate immediately", el.Url);
            }

            sb.AppendLine();
            sb.AppendFormat("Message: {0}\n\n", ex.Message);

            SendEmail(el.EmailNotificationList, subject, sb.ToString(), MailPriority.High);
        }

        private void SendEmail(string[] recipients, string subject, string body, MailPriority priority)
        {
            MailMessage msg = new MailMessage();
            for (int i = 0; i < recipients.Length; i++)
            {
                msg.To.Add(recipients[i]);
            }
            msg.Priority = priority;
            msg.Subject = subject;
            msg.Body = body;

            SmtpClient smtp = new SmtpClient();
            smtp.Send(msg);
        }

        private void SendStartupMessage()
        {
            if (this.mConfig.SendStartupMessage)
            {
                string subject = String.Format("{0} started successfully", Process.GetCurrentProcess().ProcessName);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("For information - the process \"{0}\" has started on host {1}.",
                    Process.GetCurrentProcess().ProcessName, Environment.MachineName);
                sb.AppendLine();
                sb.AppendLine("If you do not wish to recieve these messages, alter the corresponding values in app.config.");

                SendEmail(this.mConfig.StartupRecipientAddresses, subject, sb.ToString(), MailPriority.Normal);
            }
        }
    }
}
