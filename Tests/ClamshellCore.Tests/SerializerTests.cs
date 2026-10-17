using System;
using System.IO;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    public sealed class SerializerTests
    {
        private static DomeConfig ConfigWithPassword(NullSecretProtector p)
        {
            DomeConfig c = Helpers.MakeConfig("127.0.0.1:8080");
            c.UseHttps = true;
            c.RequestTimeoutMs = 1234;
            c.OpenTravelSeconds = 7;
            c.CloseTravelSeconds = 8;
            c.ExpectedResponseRegex = "^OK$";
            c.AssumedInitialState = ShutterPhase.Open;
            c.SimulationMode = true;
            c.TraceEnabled = false;
            c.QuickPathTemplate = "/test/pulse?ch={relay}";
            c.QuickOpenRelays = "1,2,3";
            c.QuickCloseRelays = "4";
            c.OpenAction.Parallel = true;
            c.SetPassword(Helpers.TestPassword, p);
            return c;
        }

        [Test]
        public void Json_RoundTrip_PreservesAllFields()
        {
            var p = new NullSecretProtector();
            DomeConfig c = ConfigWithPassword(p);

            string json = ConfigSerializer.ToJson(c);
            DomeConfig back = ConfigSerializer.FromJson(json);

            Assert.IsNotNull(back, "config parsed");
            Assert.AreEqual("127.0.0.1:8080", back.Host, "host");
            Assert.IsTrue(back.UseHttps, "https");
            Assert.AreEqual("alice", back.Username, "username");
            Assert.AreEqual(AuthMode.Basic, back.Auth, "auth");
            Assert.AreEqual(1234, back.RequestTimeoutMs, "timeout");
            Assert.AreEqual(7, back.OpenTravelSeconds, "open travel");
            Assert.AreEqual(8, back.CloseTravelSeconds, "close travel");
            Assert.AreEqual("^OK$", back.ExpectedResponseRegex, "regex");
            Assert.AreEqual(ShutterPhase.Open, back.AssumedInitialState, "assumed state");
            Assert.IsTrue(back.SimulationMode, "simulation");
            Assert.IsFalse(back.TraceEnabled, "trace");
            Assert.AreEqual("/test/pulse?ch={relay}", back.QuickPathTemplate, "quick path");
            Assert.AreEqual("1,2,3", back.QuickOpenRelays, "quick open relays");
            Assert.AreEqual("4", back.QuickCloseRelays, "quick close relays");
            Assert.AreEqual(3, back.OpenAction.Steps.Count, "open steps");
            Assert.AreEqual(c.OpenAction.Steps[1].UrlTemplate, back.OpenAction.Steps[1].UrlTemplate, "open step url");
            Assert.IsTrue(back.OpenAction.Parallel, "parallel flag");
            Assert.AreEqual(1, back.CloseAction.Steps.Count, "close steps");
            Assert.AreEqual(c.PasswordProtected, back.PasswordProtected, "protected password");
            Assert.AreEqual(Helpers.TestPassword, back.GetPassword(p), "password decrypts");
        }

        [Test]
        public void Json_IsIndentedAndContainsNoPlainPassword()
        {
            var p = new NullSecretProtector();
            DomeConfig c = ConfigWithPassword(p);
            string json = ConfigSerializer.ToJson(c);
            Assert.Contains("\n", json, "indented output");
            Assert.DoesNotContain(Helpers.TestPassword, json, "plain password not in JSON");
        }

        [Test]
        public void FromJson_NullOrEmpty_GivesNewDefaults()
        {
            DomeConfig fromNull = ConfigSerializer.FromJson(null);
            Assert.IsNotNull(fromNull, "null -> new config");
            Assert.AreEqual("", fromNull.Host, "default host");
            Assert.IsTrue(fromNull.OpenAction.IsEmpty, "no default command");

            DomeConfig fromEmpty = ConfigSerializer.FromJson("");
            Assert.IsNotNull(fromEmpty, "empty -> new config");
            Assert.AreEqual("", fromEmpty.Host, "default host (empty)");
        }

        [Test]
        public void FromJson_Garbage_ThrowsDomeCommandException()
        {
            Assert.Throws<DomeCommandException>(() => ConfigSerializer.FromJson("{not json"));
        }

        [Test]
        public void Export_WithoutPassword_BlanksProtectedValue()
        {
            var p = new NullSecretProtector();
            DomeConfig c = ConfigWithPassword(p);
            string protectedText = c.PasswordProtected;
            Assert.IsFalse(string.IsNullOrEmpty(protectedText), "precondition: password stored");

            string path = Helpers.TempPath();
            try
            {
                ConfigSerializer.Export(c, path, false);
                string text = File.ReadAllText(path);
                Assert.DoesNotContain(protectedText, text, "protected password not exported");
                Assert.DoesNotContain(Helpers.TestPassword, text, "plain password not exported");

                DomeConfig imported = ConfigSerializer.Import(path);
                Assert.AreEqual("", imported.PasswordProtected, "password blank after import");
                Assert.AreEqual("127.0.0.1:8080", imported.Host, "host kept");
                Assert.AreEqual(3, imported.OpenAction.Steps.Count, "actions kept");
            }
            finally
            {
                Helpers.SafeDelete(path);
            }
        }

        [Test]
        public void Export_WithPassword_IncludesProtectedValue()
        {
            var p = new NullSecretProtector();
            DomeConfig c = ConfigWithPassword(p);
            string protectedText = c.PasswordProtected;

            string path = Helpers.TempPath();
            try
            {
                ConfigSerializer.Export(c, path, true);
                string text = File.ReadAllText(path);
                Assert.DoesNotContain(Helpers.TestPassword, text, "plain password never written");

                DomeConfig imported = ConfigSerializer.Import(path);
                Assert.AreEqual(protectedText, imported.PasswordProtected, "protected value exported");
                Assert.AreEqual(Helpers.TestPassword, imported.GetPassword(p), "password recoverable with same protector");
            }
            finally
            {
                Helpers.SafeDelete(path);
            }
        }

        [Test]
        public void Import_MissingFile_Throws()
        {
            string path = Helpers.TempPath();
            Assert.Throws<Exception>(() => ConfigSerializer.Import(path));
        }

        [Test]
        public void Import_GarbageFile_Throws()
        {
            string path = Helpers.TempPath();
            try
            {
                File.WriteAllText(path, "{{{ definitely not config");
                Assert.Throws<Exception>(() => ConfigSerializer.Import(path));
            }
            finally
            {
                Helpers.SafeDelete(path);
            }
        }
    }
}
