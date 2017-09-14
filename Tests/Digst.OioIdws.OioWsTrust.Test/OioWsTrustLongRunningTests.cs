﻿using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;
using Digst.OioIdws.Test.Common;
using Digst.OioIdws.Wsc.OioWsTrust;
using Fiddler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digst.OioIdws.OioWsTrust.Test
{
    /// <summary>
    /// Some tests in this test suite is not working and are ignored. Hence, it has not been testet that NemLog-in STS rejects the RST SOAP message if the <wsu:Expires>2015-11-04T11:59:13Z</wsu:Expires> time has been exceeded.
    /// One could argue that it is not up to OIOIDWS.Net to test that NemLog-in STS behaves correctly.
    /// https in combination with Fiddler makes WCF create an extra HttpWebRequest inside a existing HttpWebRequest. The existing HttpWebRequest has the correctly timeout value of 1 day due to debug property being set to true in app.config. However, the extra internally created HttpWebRequest has the default value of 100 seconds ... and this makes the test suite fail.
    /// The problem starts in the line proxyAddress = chain.Enumerator.Current; line number 898 in System.Net.ServerPointManager. Current is null when Fiddler is not running and the consequence is that no extra internally HttpWebRequest is created.
    /// Switching to http is not an option as NemLog-in STS only accepts https.
    /// </summary>
    [TestClass]
    public class OioWsTrustLongRunningTests
    {
        private SessionStateHandler _fiddlerApplicationOnBeforeRequest;
        private SessionStateHandler _fiddlerApplicationOnBeforeResponse;
        private const string StsHostName = "securetokenservice.test-nemlog-in.dk";
        
        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // Start proxy server (to simulate man in the middle attacks)
            if (!FiddlerApplication.IsStarted())
            {
                FiddlerApplication.Startup(8877, true, true, false);
            }
        }

        [ClassCleanup]
        public static void TearDown()
        {
            // Shut down proxy server
            FiddlerApplication.Shutdown();
        }

        [TestCleanup]
        public void CleanupAfterEachTest()
        {
            // Unregister event handlers after each test so tests do not interfere with each other.
            FiddlerApplication.BeforeRequest -= _fiddlerApplicationOnBeforeRequest;
            FiddlerApplication.BeforeResponse -= _fiddlerApplicationOnBeforeResponse;
        }

        [TestMethod]
        [TestCategory(Constants.IntegrationTestLongRunning)]
        [Ignore]
        public void OioWsTrustRequestExpiredTest()
        {
            // Arrange
            IStsTokenService stsTokenService = new StsTokenService(TokenServiceConfigurationFactory.CreateConfiguration());

            _fiddlerApplicationOnBeforeRequest = delegate (Session oS)
            {
                // Only act on requests to WSP
                if (StsHostName != oS.hostname)
                    return;

                Thread.Sleep(610000); // Wait 10 minutes seconds. 5 minutes token time + 5 minutes clockscrew + 10 seconds extra to be sure that token is expired
            };
            FiddlerApplication.BeforeRequest += _fiddlerApplicationOnBeforeRequest;

            // Act
            try
            {
                stsTokenService.GetToken();
                Assert.IsTrue(false, "Expected exception was not thrown!!!");
            }
            catch (MessageSecurityException mse)
            {
                // Assert
                var fe = mse.InnerException as FaultException;
                Assert.IsNotNull(fe, "Expected inner fault exception");
                Assert.AreEqual("An error occurred when verifying security for the message.", fe.Message);
            }
        }

        [TestMethod]
        [TestCategory(Constants.IntegrationTestLongRunning)]
        [Ignore]
        public void OioWsTrustResponseExpiredTest()
        {
            // Arrange
            IStsTokenService stsTokenService = new StsTokenService(TokenServiceConfigurationFactory.CreateConfiguration());

            _fiddlerApplicationOnBeforeRequest = delegate (Session oS)
            {
                // Only act on requests to WSP
                if (StsHostName != oS.hostname)
                    return;

                // it not set then Thread.Sleep is ignored on the response.
                oS.bBufferResponse = true;
            };
            FiddlerApplication.BeforeRequest += _fiddlerApplicationOnBeforeRequest;

            _fiddlerApplicationOnBeforeResponse = delegate (Session oS)
            {
                // Only act on requests to WSP
                if (StsHostName != oS.hostname)
                    return;

                Thread.Sleep(610000); // Wait 10 minutes seconds. 5 minutes token time + 5 minutes clockscrew + 10 seconds extra to be sure that the response is expired
            };
            FiddlerApplication.BeforeResponse += _fiddlerApplicationOnBeforeResponse;

            // Act
            try
            {
                stsTokenService.GetToken();
                Assert.IsTrue(false, "Expected exception was not thrown!!!");
            }
            catch (MessageSecurityException mse)
            {
                // Assert
                Assert.IsTrue(mse.Message.StartsWith("The security timestamp is stale because its expiration time"));
            }
        }

        #region IStsTokenService tests

        [TestMethod]
        [TestCategory(Constants.IntegrationTestLongRunning)]
        public void OioWsTrustTokenServiceCacheGivesDifferentTokenTest()
        {
            // Arrange
            IStsTokenService stsTokenService = new StsTokenServiceCache(TokenServiceConfigurationFactory.CreateConfiguration());
            var securityToken = stsTokenService.GetToken();
            Thread.Sleep(230000); // Sleep 4 minutes - 10 seconds ... 4 minutes due to default clock skew of 1 minut

            // Act 1
            var securityToken2 = stsTokenService.GetToken();

            // Assert 1
            Assert.AreEqual(securityToken, securityToken2, "Expected that tokens was the same");

            // Act 2
            Thread.Sleep(20000); // Sleep 20 seconds more and token should be expired.
            var securityToken3 = stsTokenService.GetToken();

            // Assert 2
            Assert.AreNotEqual(securityToken, securityToken3, "Expected that tokens was Not the same");
        }

        #endregion
    }
}
