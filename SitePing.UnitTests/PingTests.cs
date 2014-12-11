using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Matrix.SitePing.Domain;

namespace SitePing.UnitTests
{
    [TestFixture]
    public class PingTests
    {
        public PingTests()
        {

        }

        [Test]
        public void TestSitePing()
        {
            IConfig config = new TestConfig();
            ILog log = new Log();
            log.EchoToConsole = true;

            SiteChecker chk = new SiteChecker(config, log);
            chk.Start();
        }
    }
}
