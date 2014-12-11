using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matrix.SitePing.Domain;

namespace SitePing.UnitTests
{
    public class TestConfig : IConfig
    {
        #region IConfig Members

        public bool CheckOnStartup
        {
            get { return true; }
        }

        public int CheckFrequency
        {
            get { return 10; }
        }

        public bool ZeroBytesIsFailure
        {
            get { return true; }
        }

        public TimeSpan NotificationStartTime
        {
            get { return TimeSpan.Parse("08:00"); }
        }

        public TimeSpan NotificationEndTime
        {
            get { return TimeSpan.Parse("18:00"); }
        }

        public int RepeatMessageInterval
        {
            get { return 30; }
        }

        public bool SendStartupMessage
        {
            get { return true; }
        }

        public string[] StartupRecipientAddresses
        {
            get
            {
                return new string[] { "jeremy.collins@matrixknowledge.com" };
            }
        }

        public IEnumerable<IWebSiteElement> WebSites
        {
            get 
            {
                IList<IWebSiteElement> list = new List<IWebSiteElement>();

                list.Add(new TestSite("NOMS", "https://pmu.hub.uk.com", true));
                list.Add(new TestSite("Charter", "https://charter.hub.uk.com", true));
                list.Add(new TestSite("St Andrews", "https://standrews.hub.uk.com", false));
                list.Add(new TestSite("Unused domain", "https://baddomain.hub.uk.com", true));

                return list;
            }
        }

        #endregion
    }
}
