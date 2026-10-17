using System;
using System.Diagnostics;
using System.Threading;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    /// <summary>The status must stay readable while a command is on the wire, and commands must never overlap.</summary>
    public sealed class ConcurrencyTests
    {
        [Test]
        public void Phase_IsInstant_AndReportsOpening_WhileCommandIsBeingSent()
        {
            var f = new ControllerFixture();
            f.Sender.DelayMs = 600; // each of the 3 open steps takes 600 ms

            Exception failure = null;
            var worker = new Thread(() =>
            {
                try { f.Controller.Open(); }
                catch (Exception ex) { failure = ex; }
            });
            worker.Start();
            Thread.Sleep(150); // the command is now in flight

            var sw = Stopwatch.StartNew();
            ShutterPhase phase = f.Controller.Phase;
            string info = f.Controller.GetStateInfo();
            sw.Stop();

            Assert.IsTrue(sw.ElapsedMilliseconds < 300, "reading the state must not wait for the HTTP request, took " + sw.ElapsedMilliseconds + " ms");
            Assert.AreEqual(ShutterPhase.Opening, phase, "a command being sent is reported as Opening");
            Assert.Contains("being sent", info, "state info says the command is in flight");

            worker.Join();
            Assert.IsTrue(failure == null, "open completed without error: " + failure);
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "after success the shutter is Opening until the travel time elapses");
            Assert.AreEqual(3, f.Sender.Count, "exactly one pulse per step");
        }

        [Test]
        public void OppositeCommand_FromAnotherThread_NeverSendsAnExtraPulse()
        {
            var f = new ControllerFixture();
            f.Sender.DelayMs = 400;

            var opener = new Thread(() => { try { f.Controller.Open(); } catch (Exception) { } });
            opener.Start();
            Thread.Sleep(100); // open is in flight

            // Close arrives from a second client while Open is still being sent: it waits for the first command, then is refused.
            Assert.Throws<DomeCommandException>(() => f.Controller.Close(), "close during opening is refused");
            opener.Join();

            Assert.AreEqual(3, f.Sender.Count, "only the 3 open pulses were sent, no close pulse");
            Assert.AreEqual(ShutterPhase.Opening, f.Controller.Phase, "still opening");
        }

        [Test]
        public void TwoSimultaneousOpens_SendTheOpenPulsesOnlyOnce()
        {
            var f = new ControllerFixture();
            f.Sender.DelayMs = 300;

            var t1 = new Thread(() => { try { f.Controller.Open(); } catch (Exception) { } });
            var t2 = new Thread(() => { try { f.Controller.Open(); } catch (Exception) { } });
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.AreEqual(3, f.Sender.Count, "two clients asking to open at once produce one set of pulses");
        }
    }
}
