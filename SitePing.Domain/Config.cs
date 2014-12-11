using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matrix.Hub.Config;
using System.Configuration;

namespace Matrix.SitePing.Domain
{
    public interface IConfig
    {
        bool CheckOnStartup { get; }
        int CheckFrequency { get; }
        bool ZeroBytesIsFailure { get; }
        TimeSpan NotificationStartTime { get; }
        TimeSpan NotificationEndTime { get; }
        int RepeatMessageInterval { get; }
        bool SendStartupMessage { get; }
        string[] StartupRecipientAddresses { get; }

        IEnumerable<IWebSiteElement> WebSites { get; }
    }

    public class Config : IConfig
    {
        private static readonly object mLock = new object();

        private static Config mInstance = null;
        public static Config Instance
        {
            get
            {
                lock (mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new Config();
                    }
                    return mInstance;
                }
            }
        }

        #region Properties

        /// <summary>
        /// Gets / sets a value to determine whether site checking is performed on startup, regardless of timer wait internal
        /// </summary>
        public bool CheckOnStartup { get; private set; }

        /// <summary>
        /// If a site is responding but returns zero bytes of data, this can optionally be counted as a failure
        /// </summary>
        public bool ZeroBytesIsFailure { get; private set; }

        /// <summary>
        /// Interval period in minutes for the site checking. Minimum is one minute.
        /// </summary>
        public int CheckFrequency { get; set; }

        /// <summary>
        /// Time in minutes between repeated failure messages being generated
        /// </summary>
        public int RepeatMessageInterval { get; private set; }

        /// <summary>
        /// Gets / sets a value to incidate whether an email is sent when code first executed
        /// </summary>
        public bool SendStartupMessage { get; private set; }


        /// <summary>
        /// The earliest time of day which notification emails are sent
        /// </summary>
        public TimeSpan NotificationStartTime { get; private set; }
        /// <summary>
        /// The latest time of day which notification emails are sent
        /// </summary>
        public TimeSpan NotificationEndTime { get; private set; }

        /// <summary>
        /// If true, additional event log information is captured
        /// </summary>
        public bool LoggingVerbose { get; set; }

        public bool SendLogEmail { get; set; }

        private string[] mStartupRecipientAddresses;
        public string[] StartupRecipientAddresses
        {
            get { return mStartupRecipientAddresses; }
        }

        public IEnumerable<IWebSiteElement> WebSites
        {
            get
            {                
                WebSiteSection section = (WebSiteSection)ConfigurationManager.GetSection("WebSiteList/webSites");
                IList<IWebSiteElement> sites = new List<IWebSiteElement>();
                foreach (WebSiteElement el in section.WebSites)
                {
                    sites.Add(el);
                }
                return sites;
            }
        }

        #endregion

        private void CheckUniqueSiteNames()
        {
            List<string> siteNames = new List<string>();
            foreach (WebSiteElement el in this.WebSites)
            {
                if (siteNames.Contains(el.Name))
                {
                    throw new ApplicationException(String.Format("Configuration error: duplicate site name \"{0}\"", el.Name));
                }
                siteNames.Add(el.Name);
            }
        }

        private Config()
        {
            ConfigConverter<int> intConfig = new ConfigConverter<int>();
            this.CheckFrequency = intConfig.GetValue("CheckFrequency", 30);
            this.RepeatMessageInterval = intConfig.GetValue("RepeatMessageInterval", 60);

            ConfigConverter<bool> boolConfig = new ConfigConverter<bool>();
            this.LoggingVerbose = boolConfig.GetValue("Logging.Verbose", false);
            this.CheckOnStartup = boolConfig.GetValue("CheckOnStartUp", true);
            this.ZeroBytesIsFailure = boolConfig.GetValue("ZeroBytesIsFailure", true);
            this.SendStartupMessage = boolConfig.GetValue("Startup.SendMessage", true);
            if ((this.SendStartupMessage) && (String.IsNullOrEmpty(ConfigurationManager.AppSettings["Startup.MessageRecipients"])))
            {
                throw new ApplicationException("Startup.MessageRecipients key must be added to app.config if Startup.SendMessage is true"); 
            }
            this.mStartupRecipientAddresses = ConfigurationManager.AppSettings["Startup.MessageRecipients"].Split(';');

            this.NotificationStartTime = TimeSpan.Parse(ConfigurationManager.AppSettings["Notification.StartTime"]);
            this.NotificationEndTime = TimeSpan.Parse(ConfigurationManager.AppSettings["Notification.EndTime"]);

            CheckUniqueSiteNames();
        }
    }
}
