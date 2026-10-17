using System;
using System.Collections.Generic;
using System.Diagnostics;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    /// <summary>Controller state machine tests, using a fake clock and a recording fake sender.</summary>
    public sealed class ControllerTests
    {
        [Test]
        public void Open_FromClosed_PulsesEachRelay_ThenReachesOpenAfterTravel()
        {
            var f = new ControllerFixture();
            Assert.AreEqual(ShutterPhase.Closed, f.Controller.Phase, "initial phase");

            f.Controller.Open();
            Assert.AreEqual(3, f.Sender.Count, "one pulse per relay");
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "phase right after open");
            Assert.AreEqual("", f.Controller.LastError, "no error");

            f.Clock = f.Clock.AddSeconds(9);
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "still travelling at 9 s");

            f.Clock = f.Clock.AddSeconds(1);
            Assert.AreEqual(ShutterPhase.Open, f.Controller.Phase, "open after 10 s travel");
        }

        [Test]
        public void Open_SendsResolvedUrlsWithAuthPasswordAndSettings()
        {
            var f = new ControllerFixture();
            f.Controller.Open();

            List<SentCall> calls = f.Sender.Calls;
            Assert.AreEqual(3, calls.Count, "calls");
            Assert.AreEqual(Helpers.LocalUrl(ControllerFixture.FakeHostPort, 1), calls[0].Url, "url 1 expanded");
            Assert.AreEqual(Helpers.LocalUrl(ControllerFixture.FakeHostPort, 3), calls[2].Url, "url 3 expanded");
            Assert.AreEqual(AuthMode.Basic, calls[0].Auth, "auth mode passed");
            Assert.AreEqual("alice", calls[0].Username, "username passed");
            Assert.AreEqual(Helpers.TestPassword, calls[0].Password, "password decrypted and passed");
            Assert.AreEqual(5000, calls[0].TimeoutMs, "timeout passed");
            Assert.AreEqual("", calls[0].ExpectedRegex, "regex passed");
        }

        [Test]
        public void Open_ConfigIsReReadOnEachCommand()
        {
            var f = new ControllerFixture();
            f.Config.OpenAction = Helpers.ActionOf(false, 0, "{scheme}://{host}/test/pulse?ch=9");
            f.Controller.Open();
            Assert.AreEqual(1, f.Sender.Count, "single step from new config");
            Assert.Contains("ch=9", f.Sender.Calls[0].Url, "new action used");
        }

        [Test]
        public void Open_WhenAlreadyOpening_SendsNothing()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Controller.Open();
            Assert.AreEqual(3, f.Sender.Count, "no second pulse while opening");
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "still opening");
        }

        [Test]
        public void Open_WhenAlreadyOpen_SendsNothing()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Clock = f.Clock.AddSeconds(10);
            Assert.AreEqual(ShutterPhase.Open, f.Controller.Phase, "precondition: open");
            f.Controller.Open();
            Assert.AreEqual(3, f.Sender.Count, "no pulse when already open");
        }

        [Test]
        public void Close_WhileOpening_IsRejected_WithNoExtraRequests()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            int before = f.Sender.Count;
            Assert.Throws<DomeCommandException>(() => f.Controller.Close());
            Assert.AreEqual(before, f.Sender.Count, "no extra request sent while opening");
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "phase unchanged");
        }

        [Test]
        public void Open_WhileClosing_IsRejected_WithNoExtraRequests()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Clock = f.Clock.AddSeconds(10);
            f.Controller.Close();
            Assert.AreEqual(4, f.Sender.Count, "3 open pulses + 1 close pulse (relay 4)");
            Assert.AreEqual(ShutterPhase.Closing, f.Controller.Phase, "closing");
            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(4, f.Sender.Count, "no pulse that could interrupt travel");
            Assert.AreEqual(ShutterPhase.Closing, f.Controller.Phase, "still closing");
        }

        [Test]
        public void Close_WhenAlreadyClosed_SendsNothing()
        {
            var f = new ControllerFixture();
            f.Controller.Close();
            Assert.AreEqual(ShutterPhase.Closed, f.Controller.Phase, "closed stays closed");
            Assert.AreEqual(0, f.Sender.Count, "no pulse when already closed");
        }

        [Test]
        public void Close_FromOpen_ThenReachesClosedAfterTravel()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Clock = f.Clock.AddSeconds(10);
            f.Controller.Close();
            Assert.AreEqual(4, f.Sender.Count, "close pulse added");
            Assert.Contains("ch=4", f.Sender.Calls[3].Url, "close action used");
            Assert.AreEqual(ShutterPhase.Closing, f.Controller.Phase, "closing");
            f.Clock = f.Clock.AddSeconds(10);
            Assert.AreEqual(ShutterPhase.Closed, f.Controller.Phase, "closed after travel");
        }

        [Test]
        public void Close_WhenAlreadyClosing_SendsNothing()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Clock = f.Clock.AddSeconds(10);
            f.Controller.Close();
            f.Controller.Close();
            Assert.AreEqual(4, f.Sender.Count, "3 open + exactly one close pulse, no second close");
            Assert.AreEqual(ShutterPhase.Closing, f.Controller.Phase, "still closing");
        }

        [Test]
        public void Sequential_StopsAtFirstFailure_AndEntersError()
        {
            var f = new ControllerFixture();
            f.Sender.Responder = url => url.EndsWith("ch=2", StringComparison.Ordinal)
                ? FakeSender.Fail("HTTP 500")
                : FakeSender.Ok();

            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(2, f.Sender.Count, "step 3 must not be sent after step 2 failed");
            Assert.AreEqual(ShutterPhase.Error, f.Controller.Phase, "error phase");
            Assert.IsNotEmpty(f.Controller.LastError, "error text kept");
            Assert.IsTrue(f.Store.Load().StartsWith("Error|", StringComparison.Ordinal), "error persisted");
        }

        [Test]
        public void Error_CanBeRetried_FromError()
        {
            var f = new ControllerFixture();
            f.Sender.Responder = url => url.EndsWith("ch=2", StringComparison.Ordinal)
                ? FakeSender.Fail("HTTP 500")
                : FakeSender.Ok();
            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(ShutterPhase.Error, f.Controller.Phase, "precondition: error");

            f.Sender.Responder = null;
            f.Controller.Open();
            Assert.AreEqual(5, f.Sender.Count, "2 failed + 3 for the retry");
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "retry succeeded");
            Assert.AreEqual("", f.Controller.LastError, "error cleared");
        }

        [Test]
        public void Parallel_FailureAnywhere_IsFailure()
        {
            var f = new ControllerFixture();
            f.Config.OpenAction = Helpers.ActionOf(true, 0,
                "{scheme}://{host}/test/pulse?ch=1",
                "{scheme}://{host}/test/pulse?ch=2",
                "{scheme}://{host}/test/pulse?ch=3");
            f.Sender.Responder = url => url.EndsWith("ch=2", StringComparison.Ordinal)
                ? FakeSender.Fail("HTTP 500")
                : FakeSender.Ok();

            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(ShutterPhase.Error, f.Controller.Phase, "error phase");
        }

        [Test]
        public void Parallel_StartsAllStepsTogether()
        {
            var f = new ControllerFixture();
            f.Config.OpenAction = Helpers.ActionOf(true, 0,
                "{scheme}://{host}/test/pulse?ch=1",
                "{scheme}://{host}/test/pulse?ch=2",
                "{scheme}://{host}/test/pulse?ch=3");
            f.Sender.DelayMs = 300;

            f.Controller.Open();
            List<SentCall> calls = f.Sender.Calls;
            Assert.AreEqual(3, calls.Count, "all steps sent");

            DateTime latestStart = calls[0].StartedUtc;
            DateTime earliestEnd = calls[0].EndedUtc;
            foreach (SentCall c in calls)
            {
                if (c.StartedUtc > latestStart)
                {
                    latestStart = c.StartedUtc;
                }
                if (c.EndedUtc < earliestEnd)
                {
                    earliestEnd = c.EndedUtc;
                }
            }
            // Every step started before the first one finished: they overlapped.
            Assert.IsTrue(latestStart < earliestEnd, "parallel steps overlapped");
        }

        [Test]
        public void Sequential_PreservesOrder_NextStepStartsAfterPreviousEnds()
        {
            var f = new ControllerFixture();
            f.Sender.DelayMs = 50;
            f.Controller.Open();

            List<SentCall> calls = f.Sender.Calls;
            Assert.AreEqual(3, calls.Count, "three calls");
            Assert.Contains("ch=1", calls[0].Url, "first");
            Assert.Contains("ch=2", calls[1].Url, "second");
            Assert.Contains("ch=3", calls[2].Url, "third");
            for (int i = 1; i < calls.Count; i++)
            {
                Assert.IsTrue(calls[i].StartedUtc >= calls[i - 1].EndedUtc, "step " + (i + 1) + " waited for step " + i);
            }
        }

        [Test]
        public void Sequential_DelayAfterStep_IsHonoured()
        {
            var f = new ControllerFixture();
            f.Config.OpenAction = Helpers.ActionOf(false, 150,
                "{scheme}://{host}/test/pulse?ch=1",
                "{scheme}://{host}/test/pulse?ch=2");

            f.Controller.Open();
            List<SentCall> calls = f.Sender.Calls;
            Assert.AreEqual(2, calls.Count, "two calls");
            double gapMs = (calls[1].StartedUtc - calls[0].StartedUtc).TotalMilliseconds;
            Assert.IsTrue(gapMs >= 140, "delay applied between steps, gap was " + gapMs + " ms");
        }

        [Test]
        public void Sequential_DelayAfterLastStep_IsNotApplied()
        {
            var f = new ControllerFixture();
            f.Config.OpenAction = Helpers.ActionOf(false, 200,
                "{scheme}://{host}/test/pulse?ch=1",
                "{scheme}://{host}/test/pulse?ch=2");

            var sw = Stopwatch.StartNew();
            f.Controller.Open();
            sw.Stop();
            // One 200 ms delay (after step 1) only; a delay after the last step would make this ~400 ms.
            Assert.IsTrue(sw.ElapsedMilliseconds < 380, "no delay after last step, took " + sw.ElapsedMilliseconds + " ms");
        }

        [Test]
        public void SecondsRemaining_CountsDownDuringTravel()
        {
            var f = new ControllerFixture();
            Assert.AreEqual(0.0, f.Controller.SecondsRemaining, "not moving");
            f.Controller.Open();
            Assert.InRange(f.Controller.SecondsRemaining, 9.9, 10.0001, "full travel right after open");
            f.Clock = f.Clock.AddSeconds(4);
            Assert.InRange(f.Controller.SecondsRemaining, 5.99, 6.01, "six seconds left after 4 s");
            f.Clock = f.Clock.AddSeconds(10);
            Assert.AreEqual(0.0, f.Controller.SecondsRemaining, "zero once settled");
        }

        [Test]
        public void Open_InvalidConfig_ThrowsWithoutStateChange()
        {
            var f = new ControllerFixture();
            f.Config.Host = "";
            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(0, f.Sender.Count, "nothing sent");
            Assert.AreEqual(ShutterPhase.Closed, f.Controller.Phase, "state unchanged");
        }

        [Test]
        public void SetAssumedState_OnlyOpenOrClosed()
        {
            var f = new ControllerFixture();
            Assert.Throws<ArgumentException>(() => f.Controller.SetAssumedState(ShutterPhase.Opening));
            Assert.Throws<ArgumentException>(() => f.Controller.SetAssumedState(ShutterPhase.Closing));
            Assert.Throws<ArgumentException>(() => f.Controller.SetAssumedState(ShutterPhase.Error));
            f.Controller.SetAssumedState(ShutterPhase.Open);
            Assert.AreEqual(ShutterPhase.Open, f.Controller.Phase, "assumed open");
        }

        [Test]
        public void SetAssumedState_ClearsError()
        {
            var f = new ControllerFixture();
            f.Sender.Responder = url => FakeSender.Fail("HTTP 500");
            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            Assert.AreEqual(ShutterPhase.Error, f.Controller.Phase, "precondition: error");

            f.Controller.SetAssumedState(ShutterPhase.Closed);
            Assert.AreEqual(ShutterPhase.Closed, f.Controller.Phase, "assumed closed");
            Assert.AreEqual("", f.Controller.LastError, "error cleared");
        }

        [Test]
        public void SetAssumedState_IsPersisted()
        {
            var f = new ControllerFixture();
            f.Controller.SetAssumedState(ShutterPhase.Open);
            Assert.IsTrue(f.Store.Load().StartsWith("Open|", StringComparison.Ordinal), "persisted as Open");
            DomeController restored = f.Build(f.Store);
            Assert.AreEqual(ShutterPhase.Open, restored.Phase, "restored after restart");
        }

        [Test]
        public void Persistence_RestoresOpening_AndSettlesAfterEndTime()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            string saved = f.Store.Load();
            Assert.IsNotNull(saved, "state saved");
            Assert.IsTrue(saved.StartsWith("Opening|", StringComparison.Ordinal), "saved as Opening");

            f.Clock = f.Clock.AddSeconds(3);
            DomeController restored = f.Build(f.Store);
            Assert.AreEqual(ShutterPhase.Opening, restored.Phase, "still travelling after restart");
            Assert.InRange(restored.SecondsRemaining, 6.9, 7.1, "remaining time restored");

            f.Clock = f.Clock.AddSeconds(8);
            Assert.AreEqual(ShutterPhase.Open, restored.Phase, "settled to Open after end time");
        }

        [Test]
        public void Persistence_RestoresClosing_SettlesToClosed()
        {
            var f = new ControllerFixture();
            long pastTicks = f.Clock.AddSeconds(-60).Ticks;
            f.Store.Save("Closing|" + pastTicks + "|");
            DomeController restored = f.Build(f.Store);
            Assert.AreEqual(ShutterPhase.Closed, restored.Phase, "closing with past end time settles to Closed");
        }

        [Test]
        public void Persistence_RestoresError_WithText()
        {
            var f = new ControllerFixture();
            f.Store.Save("Error|0|boom");
            DomeController restored = f.Build(f.Store);
            Assert.AreEqual(ShutterPhase.Error, restored.Phase, "error restored");
            Assert.Contains("boom", restored.LastError, "error text restored");
        }

        [Test]
        public void Persistence_Garbled_FallsBackToAssumedInitialState()
        {
            var f = new ControllerFixture();
            f.Store.Save("nonsense|||");
            Assert.AreEqual(ShutterPhase.Closed, f.Build(f.Store).Phase, "garbled -> assumed Closed");

            var f2 = new ControllerFixture();
            f2.Store.Save("Opening|not-a-number|");
            Assert.AreEqual(ShutterPhase.Closed, f2.Build(f2.Store).Phase, "bad ticks -> assumed Closed");
        }

        [Test]
        public void Persistence_EmptyStore_UsesAssumedInitialState()
        {
            var f = new ControllerFixture();
            f.Config.AssumedInitialState = ShutterPhase.Open;
            DomeController c = f.Build(new MemoryStateStore());
            Assert.AreEqual(ShutterPhase.Open, c.Phase, "assumed Open used");
            f.Sender.Responder = null;
            c.Open();
            Assert.AreEqual(0, f.Sender.Count, "already open: no pulse");
        }

        [Test]
        public void Constructor_NeverThrows_WhenStoreFails()
        {
            var f = new ControllerFixture();
            DomeController c = null;
            c = new DomeController(() => f.Config, f.Sender, f.Protector, new FailingStateStore(), f.Log.Write, () => f.Clock);
            Assert.IsNotNull(c, "controller constructed");
            Assert.AreEqual(ShutterPhase.Closed, c.Phase, "falls back to assumed state");
        }

        [Test]
        public void GetStateInfo_IsNonEmpty()
        {
            var f = new ControllerFixture();
            Assert.IsNotEmpty(f.Controller.GetStateInfo(), "state info");
        }

        [Test]
        public void Password_NeverAppearsInLogs_OrInStore()
        {
            var f = new ControllerFixture();
            f.Controller.Open();
            f.Clock = f.Clock.AddSeconds(10);
            f.Controller.Close();
            f.Clock = f.Clock.AddSeconds(10);

            f.Sender.Responder = url => FakeSender.Fail("HTTP 500");
            Assert.Throws<DomeCommandException>(() => f.Controller.Open());
            f.Controller.SetAssumedState(ShutterPhase.Closed);
            string info = f.Controller.GetStateInfo();

            Assert.DoesNotContain(Helpers.TestPassword, f.Log.All, "password not in logs");
            Assert.DoesNotContain(Helpers.TestPassword, info, "password not in state info");
            Assert.DoesNotContain(Helpers.TestPassword, f.Store.Load(), "password not in store");
            Assert.DoesNotContain(Helpers.TestPassword, f.Controller.LastError, "password not in error text");
        }
    }
}
