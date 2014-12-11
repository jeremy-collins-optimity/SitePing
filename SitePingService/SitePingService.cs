using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Matrix.SitePing.Domain;

namespace SitePingService
{
    public partial class SitePingService : ServiceBase
    {
        private SiteChecker mSiteChecker;

        public string ServiceLogName
        {
            get { return String.Format("{0}_Log", this.ServiceName); }
        }
	
        public SitePingService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ILog log = new Log();
            log.EchoToConsole = false;
            
            mSiteChecker = new SiteChecker(Config.Instance, log);
            mSiteChecker.CheckFailed += new EventHandler<SiteEventArgs>(mSiteChecker_CheckFailed);
            mSiteChecker.Start();
        }

        void mSiteChecker_CheckFailed(object sender, SiteEventArgs e)
        {
            string msg = String.Format("SITE DOWN - {0} - {1}", e.WebSite.Name, e.WebSite.Url);
            EventLog el = GetEventLog();
            el.WriteEntry(msg, EventLogEntryType.Error);
        }

        private EventLog GetEventLog()
        {
            EventLog log = new EventLog();
            if (!EventLog.SourceExists(this.ServiceName))
            {
                EventLog.CreateEventSource(this.ServiceName, this.ServiceLogName);
            }

            log.Source = this.ServiceName;
            return log;
        }

        protected override void OnStop()
        {
            mSiteChecker.Stop();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
