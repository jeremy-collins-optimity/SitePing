using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matrix.SitePing.Domain;

namespace SitePing.UnitTests
{
    public class TestSite : IWebSiteElement
    {
        #region IWebSiteElement Members

        private string mName;
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        private string mUrl;
        public string Url
        {
            get { return mUrl; }
            set { mUrl = value; }
        }

        private bool mEnabled;
        public bool Enabled
        {
            get { return mEnabled; }
            set { mEnabled = value; }
        }	        

        public string[] EmailNotificationList
        {
            get { return new string[] { "jeremy.collins@matrixknowledge.com" }; }
        }

        #endregion

        public TestSite(string name, string url, bool enabled)
        {
            this.Name = name;
            this.Url = url;
            this.Enabled = enabled;
        }
    }
}
