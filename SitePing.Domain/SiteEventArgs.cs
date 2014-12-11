using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Matrix.SitePing.Domain
{
    public class SiteEventArgs : EventArgs
    {
        private IWebSiteElement mWebSite;
        public IWebSiteElement WebSite
        {
            get { return mWebSite; }
            set { mWebSite = value; }
        }

        public SiteEventArgs()
        { }

        public SiteEventArgs(IWebSiteElement el)
        {
            this.mWebSite = el;
        }
    }
}
