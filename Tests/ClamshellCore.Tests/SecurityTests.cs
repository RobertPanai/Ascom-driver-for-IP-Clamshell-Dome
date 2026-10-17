using System;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    public sealed class SecurityTests
    {
        [Test]
        public void LogMask_MaskUrl_StripsUserInfo()
        {
            string masked = LogMask.MaskUrl("http://alice:Hunter-Pw-1@127.0.0.1:8080/test/pulse?ch=1");
            Assert.DoesNotContain("Hunter-Pw-1", masked, "password stripped");
            Assert.DoesNotContain("alice:", masked, "user:pass stripped");
            Assert.Contains("127.0.0.1:8080/test/pulse?ch=1", masked, "rest of url kept");
        }

        [Test]
        public void LogMask_MaskUrl_MasksSecretQueryValues()
        {
            string masked = LogMask.MaskUrl("http://127.0.0.1/x?ch=1&pass=Abc123&token=Tok999&pwd=Pwd888&secret=Sec777&key=Key666");
            Assert.DoesNotContain("Abc123", masked, "pass value");
            Assert.DoesNotContain("Tok999", masked, "token value");
            Assert.DoesNotContain("Pwd888", masked, "pwd value");
            Assert.DoesNotContain("Sec777", masked, "secret value");
            Assert.DoesNotContain("Key666", masked, "key value");
            Assert.Contains("ch=1", masked, "non-secret query kept");
            Assert.Contains("***", masked, "masked marker present");
        }

        [Test]
        public void LogMask_MaskUrl_LeavesPlainUrlUnchanged()
        {
            string url = "http://127.0.0.1:8080/test/pulse?ch=1";
            Assert.AreEqual(url, LogMask.MaskUrl(url), "nothing to mask");
        }

        [Test]
        public void LogMask_MaskText_ReplacesSecret()
        {
            string text = LogMask.MaskText("auth=Hunter-Pw-1 done", "Hunter-Pw-1");
            Assert.DoesNotContain("Hunter-Pw-1", text, "secret removed");
            Assert.Contains("***", text, "masked marker present");
        }

        [Test]
        public void LogMask_MaskText_EmptySecret_LeavesTextAlone()
        {
            Assert.AreEqual("hello", LogMask.MaskText("hello", ""), "empty secret must not mangle text");
        }

        [Test]
        public void Safety_TestModeIsOn_UnderHarness()
        {
            Assert.AreEqual("CLAMSHELL_TEST_MODE", SafetyGuard.TestModeEnvVar, "env var name");
            Assert.IsTrue(SafetyGuard.IsTestMode, "harness runs with CLAMSHELL_TEST_MODE=1");
        }

        [Test]
        public void Safety_LoopbackHosts_AreRecognised()
        {
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("127.0.0.1"), "127.0.0.1");
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("127.0.0.1:8080"), "127.0.0.1 with port");
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("127.255.0.9"), "whole 127/8 is loopback");
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("localhost"), "localhost");
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("localhost:8080"), "localhost with port");
            Assert.IsTrue(SafetyGuard.IsLoopbackHost("::1"), "IPv6 loopback");
        }

        [Test]
        public void Safety_NonLoopbackHosts_AreNotLoopback()
        {
            // TEST-NET-1 address: only used to check the guard refuses it. Never contacted.
            Assert.IsFalse(SafetyGuard.IsLoopbackHost("192.0.2.1"), "TEST-NET is not loopback");
            Assert.IsFalse(SafetyGuard.IsLoopbackHost("192.0.2.1:8080"), "TEST-NET with port");
            Assert.IsFalse(SafetyGuard.IsLoopbackHost("198.51.100.7"), "TEST-NET-2 is not loopback");
            Assert.IsFalse(SafetyGuard.IsLoopbackHost("203.0.113.9"), "TEST-NET-3 is not loopback");
            Assert.IsFalse(SafetyGuard.IsLoopbackHost("example.invalid"), "hostname");
        }

        [Test]
        public void Safety_EnsureAllowed_Loopback_DoesNotThrow()
        {
            // EnsureAllowed does no I/O; the port is never contacted.
            SafetyGuard.EnsureAllowed(new Uri("http://127.0.0.1:1/test/pulse?ch=1"));
        }

        [Test]
        public void Safety_EnsureAllowed_NonLoopback_IsRefusedInTestMode()
        {
            Assert.Throws<DomeCommandException>(() => SafetyGuard.EnsureAllowed(new Uri("http://192.0.2.1/test/pulse?ch=1")));
        }

        [Test]
        public void Safety_NullProtector_RoundTripAndFailureHandling()
        {
            var p = new NullSecretProtector();
            string protectedText = p.Protect("abc");
            Assert.IsTrue(protectedText.StartsWith("test1:", StringComparison.Ordinal), "test prefix");
            Assert.DoesNotContain("abc", protectedText, "not plain text");
            Assert.AreEqual("abc", p.Unprotect(protectedText), "round trip");
            Assert.AreEqual("", p.Protect(""), "empty stays empty");
            Assert.AreEqual("", p.Unprotect("garbage!!"), "bad input gives empty, no throw");
        }
    }
}
