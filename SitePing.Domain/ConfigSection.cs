using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Matrix.SitePing.Domain
{
    public class WebSiteSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public WebSiteCollection WebSites
        {
            get { return (WebSiteCollection)base[""]; }
            set { this[""] = value; }
        }
    }

    public class WebSiteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WebSiteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as WebSiteElement).Name;
        }

        public WebSiteElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as WebSiteElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new WebSiteElement this[string name]
        {
            get
            {
                WebSiteElement value = null;
                foreach (WebSiteElement qe in this)
                {
                    if (qe.Name == name)
                    {
                        value = qe;
                        break;
                    }
                }
                return value;
            }
        }
    }

    public interface IWebSiteElement
    {
        string Name { get; }
        string Url { get; }
        bool Enabled { get; }
        string[] EmailNotificationList { get; }
    }

    public class WebSiteElement : ConfigurationElement, IWebSiteElement
    {
        public WebSiteElement()
        { }

        public WebSiteElement(string name, string url, bool enabled, string emailNotifications)
        {
            this.Name = name;
            this.Url = url;
            this.Enabled = enabled;
            this.EmailNotifications = emailNotifications;
        }

        #region properties

        /// <summary>
        /// Membership Site name of web app to be updated
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public String Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Connection string to membership database for this web Site
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true)]
        public String Url
        {
            get { return (String)this["url"]; }
            set { this["url"] = value; }
        }

        /// <summary>
        /// Enables / disables automatic account updates for this web Site
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = "true", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Semicolon delimited list of email addresses for site unavailability warning
        /// </summary>
        [ConfigurationProperty("emailNotifications", DefaultValue = "helpdesk@matrixknowledge.com", IsRequired = true)]
        public string EmailNotifications
        {
            get { return (String)this["emailNotifications"]; }
            set { this["emailNotifications"] = value; }
        }

        /// <summary>
        /// Separated list of email addresses
        /// </summary>
        public string[] EmailNotificationList
        {
            get { return this.EmailNotifications.Split(';'); }
            //set { mEmailNotificationList = String.Join(";", value); }
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0} - {1}", this.Name, this.Url);
        }
    }
}
