using System;
using System.Diagnostics;
using System.Text;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    /// <summary>HttpPulseSender against the loopback mock, reachability, simulation, and controller end-to-end.</summary>
    public sealed class SenderTests
    {
        private const string Path1 = "/test/pulse?ch=1";

        private static DomeController HttpController(DomeConfig cfg, LogCollector log, NullSecretProtector protector)
        {
            return new DomeController(() => cfg, new HttpPulseSender(log.Write), protector, new MemoryStateStore(), log.Write, () => DateTime.UtcNow);
        }

        [Test]
        public void Send_2xx_IsSuccess_AndHitsExactPath()
        {
            using (var mock = new MockDomeServer())
            {
                mock.StatusCode = 200;
                mock.Body = "OK";
                var log = new LogCollector();
                SendResult r = new HttpPulseSender(log.Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 2000, "");

                Assert.IsTrue(r.Success, "success, error was: " + r.Error);
                Assert.AreEqual(200, r.StatusCode, "status");
                Assert.AreEqual("OK", r.BodySnippet, "body snippet");
                Assert.AreEqual(1, mock.RequestCount, "one request");
                Assert.AreEqual(Path1, mock.Requests[0].RawUrl, "path and query sent as given");
                Assert.AreEqual("GET", mock.Requests[0].Method, "HTTP GET only");
            }
        }

        [Test]
        public void Send_500_IsFailure_WithStatusCode()
        {
            using (var mock = new MockDomeServer())
            {
                mock.StatusCode = 500;
                mock.Body = "boom";
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 2000, "");
                Assert.IsFalse(r.Success, "500 is failure");
                Assert.AreEqual(500, r.StatusCode, "status kept");
            }
        }

        [Test]
        public void Send_RegexMatch_IsSuccess()
        {
            using (var mock = new MockDomeServer())
            {
                mock.Body = "DONE";
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 2000, "^DONE$");
                Assert.IsTrue(r.Success, "body matches regex, error was: " + r.Error);
            }
        }

        [Test]
        public void Send_RegexMismatch_IsFailure()
        {
            using (var mock = new MockDomeServer())
            {
                mock.Body = "BUSY";
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 2000, "^DONE$");
                Assert.IsFalse(r.Success, "body does not match regex");
                Assert.IsNotEmpty(r.Error, "error text set");
            }
        }

        [Test]
        public void Send_BodySnippet_IsCappedAt200()
        {
            using (var mock = new MockDomeServer())
            {
                mock.Body = new string('x', 5000);
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 4000, "");
                Assert.IsTrue(r.Success, "2xx is success");
                Assert.IsTrue(r.BodySnippet != null && r.BodySnippet.Length <= 200, "snippet <= 200 chars, was " + (r.BodySnippet == null ? -1 : r.BodySnippet.Length));
            }
        }

        [Test]
        public void Send_Timeout_FailsWithinTimeout()
        {
            using (var mock = new MockDomeServer())
            {
                mock.DelayMs = 2000;
                var sw = Stopwatch.StartNew();
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 500, "");
                sw.Stop();

                Assert.IsFalse(r.Success, "timed out is failure");
                Assert.IsNotEmpty(r.Error, "timeout error text");
                Assert.IsTrue(sw.ElapsedMilliseconds < 1900, "returned at the timeout, not after the 2 s delay: " + sw.ElapsedMilliseconds + " ms");
            }
        }

        [Test]
        public void Send_Redirect_IsNotFollowed()
        {
            using (var mock = new MockDomeServer())
            {
                mock.RedirectLocation = "http://192.0.2.1/elsewhere";
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "", "", 2000, "");
                Assert.IsFalse(r.Success, "302 is not a success");
                Assert.AreEqual(302, r.StatusCode, "redirect returned, not followed");
                Assert.AreEqual(1, mock.RequestCount, "no follow-up request");
            }
        }

        [Test]
        public void Send_BasicAuth_CorrectPassword_SendsHeader()
        {
            using (var mock = new MockDomeServer())
            {
                mock.RequireBasic = true;
                mock.BasicUser = "alice";
                mock.BasicPassword = Helpers.TestPassword;
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.Basic, "alice", Helpers.TestPassword, 2000, "");

                Assert.IsTrue(r.Success, "accepted with Basic auth, error was: " + r.Error);
                Assert.IsTrue(mock.Requests[0].HasAuthorization, "Authorization header present");
                Assert.AreEqual(MockDomeServer.BasicHeader("alice", Helpers.TestPassword), mock.Requests[0].Authorization, "Basic header value");
                Assert.DoesNotContain(Helpers.TestPassword, mock.Requests[0].RawUrl, "password never in URL");
            }
        }

        [Test]
        public void Send_BasicAuth_WrongPassword_Is401Failure()
        {
            using (var mock = new MockDomeServer())
            {
                mock.RequireBasic = true;
                mock.BasicUser = "alice";
                mock.BasicPassword = Helpers.TestPassword;
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.Basic, "alice", "wrong-pw", 2000, "");
                Assert.IsFalse(r.Success, "wrong credentials rejected");
                Assert.AreEqual(401, r.StatusCode, "401 returned");
            }
        }

        [Test]
        public void Send_AuthNone_SendsNoAuthorizationHeader()
        {
            using (var mock = new MockDomeServer())
            {
                SendResult r = new HttpPulseSender(new LogCollector().Write).Send(mock.BaseUrl + Path1, AuthMode.None, "alice", Helpers.TestPassword, 2000, "");
                Assert.IsTrue(r.Success, "no auth needed, error was: " + r.Error);
                Assert.IsFalse(mock.Requests[0].HasAuthorization, "no Authorization header for AuthMode.None");
            }
        }

        [Test]
        public void Send_Logs_NeverContainPasswordOrCredentials()
        {
            using (var mock = new MockDomeServer())
            {
                mock.RequireBasic = true;
                mock.BasicUser = "alice";
                mock.BasicPassword = Helpers.TestPassword;
                var log = new LogCollector();
                var sender = new HttpPulseSender(log.Write);
                sender.Send(mock.BaseUrl + Path1, AuthMode.Basic, "alice", Helpers.TestPassword, 2000, "");
                sender.Send(mock.BaseUrl + Path1, AuthMode.Basic, "alice", "wrong-pw", 2000, "");

                string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("alice:" + Helpers.TestPassword));
                Assert.DoesNotContain(Helpers.TestPassword, log.All, "password not logged");
                Assert.DoesNotContain(b64, log.All, "Basic credentials not logged");
            }
        }

        [Test]
        public void Send_NonLoopbackTarget_IsRefusedBeforeAnyIO()
        {
            // TEST-NET-1 address, only used to check the guard refuses it. Nothing is contacted.
            var sw = Stopwatch.StartNew();
            SendResult r = new HttpPulseSender(new LogCollector().Write).Send("http://192.0.2.1/test/pulse?ch=1", AuthMode.None, "", "", 500, "");
            sw.Stop();
            Assert.IsFalse(r.Success, "non-loopback refused");
            Assert.IsNotEmpty(r.Error, "refusal reason given");
            Assert.IsTrue(sw.ElapsedMilliseconds < 400, "refused without waiting for network, took " + sw.ElapsedMilliseconds + " ms");
        }

        [Test]
        public void Simulated_ReturnsSuccess_AndLogsWithoutNetwork()
        {
            var log = new LogCollector();
            SendResult r = new SimulatedPulseSender(log.Write).Send("http://127.0.0.1:1/test/pulse?ch=1", AuthMode.None, "", "", 500, "");
            Assert.IsTrue(r.Success, "simulated success");
            Assert.Contains("SIMULATED GET", log.All, "simulation logged");
        }

        [Test]
        public void Reachability_OpenPort_Succeeds_WithoutHttpRequest()
        {
            using (var mock = new MockDomeServer())
            {
                string error;
                bool ok = Reachability.TryTcpConnect(mock.HostPort, false, 1000, out error);
                Assert.IsTrue(ok, "tcp reachable, error: " + error);
                Assert.AreEqual(0, mock.RequestCount, "connect check sends no HTTP request");
            }
        }

        [Test]
        public void Reachability_ClosedPort_Fails()
        {
            int port = MockDomeServer.FreePort();
            string error;
            bool ok = Reachability.TryTcpConnect("127.0.0.1:" + port, false, 1000, out error);
            Assert.IsFalse(ok, "nothing listening");
            Assert.IsNotEmpty(error, "error text given");
        }

        // ---- controller end-to-end over real HTTP to the loopback mock ----

        [Test]
        public void Controller_BasicAuth_CredentialsReachTheDome_AndStayOutOfLogs()
        {
            using (var mock = new MockDomeServer())
            {
                mock.RequireBasic = true;
                mock.BasicUser = "alice";
                mock.BasicPassword = Helpers.TestPassword;

                var protector = new NullSecretProtector();
                var log = new LogCollector();
                DomeConfig cfg = Helpers.MakeConfig(mock.HostPort);
                cfg.OpenAction = Helpers.ActionOf(false, 0, "{scheme}://{host}" + Path1);
                cfg.SetPassword(Helpers.TestPassword, protector);

                DomeController c = HttpController(cfg, log, protector);
                c.Open();

                Assert.AreEqual(ShutterPhase.Opening, c.Phase, "accepted by dome");
                Assert.AreEqual(1, mock.RequestCount, "one pulse");
                Assert.AreEqual(MockDomeServer.BasicHeader("alice", Helpers.TestPassword), mock.Requests[0].Authorization, "credentials sent");
                Assert.DoesNotContain(Helpers.TestPassword, log.All, "password not logged");
            }
        }

        [Test]
        public void Controller_AuthNone_SendsNoHeader()
        {
            using (var mock = new MockDomeServer())
            {
                var protector = new NullSecretProtector();
                DomeConfig cfg = Helpers.MakeConfig(mock.HostPort);
                cfg.Auth = AuthMode.None;
                cfg.OpenAction = Helpers.ActionOf(false, 0, "{scheme}://{host}" + Path1);

                DomeController c = HttpController(cfg, new LogCollector(), protector);
                c.Open();
                Assert.AreEqual(ShutterPhase.Opening, c.Phase, "opened");
                Assert.IsFalse(mock.Requests[0].HasAuthorization, "no Authorization header");
            }
        }

        [Test]
        public void Controller_SlowDome_TimesOutAndEntersError()
        {
            using (var mock = new MockDomeServer())
            {
                mock.DelayMs = 2000;
                DomeConfig cfg = Helpers.MakeConfig(mock.HostPort);
                cfg.Auth = AuthMode.None;
                cfg.RequestTimeoutMs = 500;
                cfg.OpenAction = Helpers.ActionOf(false, 0, "{scheme}://{host}" + Path1);

                var protector = new NullSecretProtector();
                DomeController c = HttpController(cfg, new LogCollector(), protector);

                var sw = Stopwatch.StartNew();
                Assert.Throws<DomeCommandException>(() => c.Open());
                sw.Stop();

                Assert.IsTrue(sw.ElapsedMilliseconds < 1900, "gave up at the timeout, took " + sw.ElapsedMilliseconds + " ms");
                Assert.AreEqual(ShutterPhase.Error, c.Phase, "error after timeout");
                Assert.IsNotEmpty(c.LastError, "error text kept");
            }
        }

        [Test]
        public void Controller_HttpFailure_StopsSequence_AndRegexIsEnforced()
        {
            using (var mock = new MockDomeServer())
            {
                var protector = new NullSecretProtector();

                // HTTP 500 on step 1: the other two steps must not be sent.
                DomeConfig cfg = Helpers.MakeConfig(mock.HostPort);
                cfg.Auth = AuthMode.None;
                mock.StatusCode = 500;
                DomeController c = HttpController(cfg, new LogCollector(), protector);
                Assert.Throws<DomeCommandException>(() => c.Open());
                Assert.AreEqual(1, mock.RequestCount, "remaining steps not sent after failure");
                Assert.AreEqual(ShutterPhase.Error, c.Phase, "error phase");

                // Regex mismatch: body is not what the config expects.
                mock.StatusCode = 200;
                mock.Body = "BUSY";
                cfg.ExpectedResponseRegex = "^DONE$";
                DomeController c2 = HttpController(cfg, new LogCollector(), protector);
                Assert.Throws<DomeCommandException>(() => c2.Open());
                Assert.AreEqual(ShutterPhase.Error, c2.Phase, "regex mismatch is an error");

                // Matching body: accepted.
                mock.Body = "DONE";
                DomeController c3 = HttpController(cfg, new LogCollector(), protector);
                c3.Open();
                Assert.AreEqual(ShutterPhase.Opening, c3.Phase, "regex match accepted");
            }
        }
    }
}
