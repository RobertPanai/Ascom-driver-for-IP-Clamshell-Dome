using System;
using System.Collections.Generic;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    public sealed class ConfigTests
    {
        private static bool Valid(DomeConfig c)
        {
            List<string> problems;
            return c.Validate(out problems);
        }

        [Test]
        public void FromTemplate_ExpandsOneStepPerRelay()
        {
            DomeAction a = DomeAction.FromTemplate("/test/pulse?ch={relay}", "1, 2;3", true, 250);
            Assert.AreEqual(3, a.Steps.Count, "one step per relay");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=1", a.Steps[0].UrlTemplate, "step 0");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=2", a.Steps[1].UrlTemplate, "step 1");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=3", a.Steps[2].UrlTemplate, "step 2");
            Assert.IsTrue(a.Parallel, "parallel flag kept");
            Assert.IsFalse(a.IsEmpty, "action with steps is not empty");
        }

        [Test]
        public void FromTemplate_AddsMissingLeadingSlash()
        {
            DomeAction a = DomeAction.FromTemplate("test/pulse?ch={relay}", "7", false, 0);
            Assert.AreEqual(1, a.Steps.Count, "one step");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=7", a.Steps[0].UrlTemplate, "leading slash added");
        }

        [Test]
        public void FromTemplate_SkipsEmptyAndTrimsRelays()
        {
            DomeAction a = DomeAction.FromTemplate("/test/pulse?ch={relay}", "  1 ,, ;  2  ", false, 0);
            Assert.AreEqual(2, a.Steps.Count, "empty entries skipped");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=1", a.Steps[0].UrlTemplate, "first relay trimmed");
            Assert.AreEqual("{scheme}://{host}/test/pulse?ch=2", a.Steps[1].UrlTemplate, "second relay trimmed");
        }

        [Test]
        public void FromTemplate_EmptyRelays_GivesEmptyAction()
        {
            DomeAction a = DomeAction.FromTemplate("/test/pulse?ch={relay}", "", false, 0);
            Assert.AreEqual(0, a.Steps.Count, "no steps");
            Assert.IsTrue(a.IsEmpty, "IsEmpty");
        }

        [Test]
        public void FromTemplate_EscapesRelayValue()
        {
            // Spaces, commas and semicolons separate relays, so the escaped value uses a slash instead.
            DomeAction a = DomeAction.FromTemplate("/test/pulse?ch={relay}", "a/b", false, 0);
            Assert.AreEqual(1, a.Steps.Count, "one step");
            Assert.Contains("ch=a%2Fb", a.Steps[0].UrlTemplate, "relay value escaped");
        }

        [Test]
        public void NewDomeAction_IsEmpty()
        {
            Assert.IsTrue(new DomeAction().IsEmpty, "new action has no steps");
        }

        [Test]
        public void Defaults_LeaveCommandsEmpty_AndAreSafe()
        {
            var c = new DomeConfig();
            Assert.IsTrue(c.OpenAction.IsEmpty, "no default open command");
            Assert.IsTrue(c.CloseAction.IsEmpty, "no default close command");
            Assert.AreEqual("", c.Host, "no default host");
            Assert.AreEqual("", c.PasswordProtected, "no password by default");
            Assert.AreEqual(5000, c.RequestTimeoutMs, "default timeout");
            Assert.AreEqual(AuthMode.Basic, c.Auth, "default auth");
            Assert.AreEqual(ShutterPhase.Closed, c.AssumedInitialState, "default assumed state");
            Assert.IsFalse(c.UseHttps, "https off by default");
            Assert.IsFalse(c.SimulationMode, "simulation off by default");
            Assert.IsTrue(c.TraceEnabled, "trace on by default");
        }

        [Test]
        public void Validate_DefaultConfig_IsInvalidWithProblems()
        {
            var c = new DomeConfig();
            List<string> problems;
            Assert.IsFalse(c.Validate(out problems), "default config is not usable");
            Assert.IsNotNull(problems, "problems list returned");
            Assert.IsTrue(problems.Count > 0, "problems listed");
        }

        [Test]
        public void Validate_CompleteConfig_IsValid()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            List<string> problems;
            Assert.IsTrue(c.Validate(out problems), "complete config is valid");
            Assert.AreEqual(0, problems.Count, "no problems");
        }

        [Test]
        public void Validate_EmptyHost_IsInvalid()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.Host = "";
            Assert.IsFalse(Valid(c), "empty host rejected");
        }

        [Test]
        public void Validate_EmptyOpenOrCloseAction_IsInvalid()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.OpenAction = new DomeAction();
            Assert.IsFalse(Valid(c), "empty open action rejected");

            c = Helpers.MakeConfig("127.0.0.1:8080");
            c.CloseAction = new DomeAction();
            Assert.IsFalse(Valid(c), "empty close action rejected");
        }

        [Test]
        public void Validate_RequestTimeoutRange_500To60000()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.RequestTimeoutMs = 499;
            Assert.IsFalse(Valid(c), "499 ms rejected");
            c.RequestTimeoutMs = 60001;
            Assert.IsFalse(Valid(c), "60001 ms rejected");
            c.RequestTimeoutMs = 60000;
            Assert.IsTrue(Valid(c), "60000 ms accepted");
        }

        [Test]
        public void Validate_TravelSecondsRange_1To900()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.OpenTravelSeconds = 0;
            Assert.IsFalse(Valid(c), "open travel 0 means not set, invalid");
            c.OpenTravelSeconds = 901;
            Assert.IsFalse(Valid(c), "open travel 901 rejected");
            c.OpenTravelSeconds = 1;
            Assert.IsTrue(Valid(c), "open travel 1 accepted");

            c.CloseTravelSeconds = 0;
            Assert.IsFalse(Valid(c), "close travel 0 invalid");
            c.CloseTravelSeconds = 900;
            Assert.IsTrue(Valid(c), "close travel 900 accepted");
        }

        [Test]
        public void Validate_BadRegex_IsInvalid()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.ExpectedResponseRegex = "([";
            Assert.IsFalse(Valid(c), "non-compiling regex rejected");
            c.ExpectedResponseRegex = "^OK$";
            Assert.IsTrue(Valid(c), "valid regex accepted");
        }

        [Test]
        public void Validate_SimulationMode_SkipsHostCheck()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            c.Host = "";
            c.SimulationMode = true;
            Assert.IsTrue(Valid(c), "simulation mode does not need a host");
        }

        [Test]
        public void ExpandUrl_ReplacesSchemeAndHost()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            Assert.AreEqual("http://127.0.0.1:8080/x?ch=1", c.ExpandUrl("{scheme}://{host}/x?ch=1"), "http");
            c.UseHttps = true;
            Assert.AreEqual("https://127.0.0.1:8080/x?ch=1", c.ExpandUrl("{scheme}://{host}/x?ch=1"), "https");
        }

        [Test]
        public void Clone_IsIndependentOfOriginal()
        {
            var c = Helpers.MakeConfig("127.0.0.1:8080");
            DomeConfig clone = c.Clone();
            Assert.IsNotNull(clone, "clone created");
            clone.Host = "127.0.0.2:9999";
            Assert.AreEqual("127.0.0.1:8080", c.Host, "original host unchanged");
            Assert.AreEqual("127.0.0.2:9999", clone.Host, "clone host changed");
        }

        [Test]
        public void SetPassword_StoresOnlyProtectedText()
        {
            var c = new DomeConfig();
            var p = new NullSecretProtector();
            c.SetPassword("Plain-Pw-99", p);
            Assert.IsFalse(string.IsNullOrEmpty(c.PasswordProtected), "protected value stored");
            Assert.DoesNotContain("Plain-Pw-99", c.PasswordProtected, "plain text not stored");
            Assert.AreEqual("Plain-Pw-99", c.GetPassword(p), "round trip");
        }

        [Test]
        public void SetPassword_Empty_ClearsStoredPassword()
        {
            var c = new DomeConfig();
            var p = new NullSecretProtector();
            c.SetPassword("something", p);
            c.SetPassword("", p);
            Assert.AreEqual("", c.PasswordProtected, "cleared");
            Assert.AreEqual("", c.GetPassword(p), "no password");
        }

        [Test]
        public void GetPassword_CorruptValue_ReturnsEmptyWithoutThrowing()
        {
            var c = new DomeConfig();
            c.PasswordProtected = "test1:%%%not-base64###";
            Assert.AreEqual("", c.GetPassword(new NullSecretProtector()), "corrupt value gives empty password");
        }
    }
}
