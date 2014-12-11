using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Matrix.SitePing.Domain;

namespace SitePing
{
    class Program
    {
        static void Main(string[] args)
        {
            SiteChecker chk = new SiteChecker(Config.Instance, new Log());
            chk.Start();

            while (chk.Active) { }
        }
    }
}
