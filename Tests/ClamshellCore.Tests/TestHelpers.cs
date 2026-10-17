using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ASCOM.Clamshell.Core;

namespace ClamshellCore.Tests
{
    /// <summary>One request as seen by FakeSender.</summary>
    public sealed class SentCall
    {
        public string Url;
        public AuthMode Auth;
        public string Username;
        public string Password;
        public int TimeoutMs;
        public string ExpectedRegex;
        public DateTime StartedUtc;
        public DateTime EndedUtc;
    }

    /// <summary>Recording IPulseSender with no network. Thread-safe.</summary>
    public sealed class FakeSender : IPulseSender
    {
        private readonly object _gate = new object();
        private readonly List<SentCall> _calls = new List<SentCall>();

        /// <summary>Time each fake request takes.</summary>
        public int DelayMs;

        /// <summary>Optional responder by URL; null means every request succeeds.</summary>
        public Func<string, SendResult> Responder;

        public SendResult Send(string url, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex)
        {
            var call = new SentCall();
            call.Url = url;
            call.Auth = auth;
            call.Username = username;
            call.Password = password;
            call.TimeoutMs = timeoutMs;
            call.ExpectedRegex = expectedRegex;
            call.StartedUtc = DateTime.UtcNow;
            lock (_gate)
            {
                _calls.Add(call);
            }

            if (DelayMs > 0)
            {
                Thread.Sleep(DelayMs);
            }
            call.EndedUtc = DateTime.UtcNow;

            Func<string, SendResult> responder = Responder;
            return responder != null ? responder(url) : Ok();
        }

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _calls.Count;
                }
            }
        }

        public List<SentCall> Calls
        {
            get
            {
                lock (_gate)
                {
                    return new List<SentCall>(_calls);
                }
            }
        }

        public static SendResult Ok()
        {
            var r = new SendResult();
            r.Success = true;
            r.StatusCode = 200;
            r.BodySnippet = "OK";
            return r;
        }

        public static SendResult Fail(string error)
        {
            var r = new SendResult();
            r.Success = false;
            r.StatusCode = 500;
            r.Error = error;
            return r;
        }
    }

    /// <summary>Collects every log line (category + text) in memory. Thread-safe.</summary>
    public sealed class LogCollector
    {
        private readonly object _gate = new object();
        private readonly List<string> _lines = new List<string>();

        public void Write(string category, string text)
        {
            lock (_gate)
            {
                _lines.Add(category + ": " + text);
            }
        }

        /// <summary>All log lines joined by newlines.</summary>
        public string All
        {
            get
            {
                lock (_gate)
                {
                    return string.Join("\n", _lines);
                }
            }
        }
    }

    /// <summary>State store whose Load throws; used to check the controller constructor never throws.</summary>
    public sealed class FailingStateStore : IStateStore
    {
        public string Load()
        {
            throw new InvalidOperationException("store unavailable (test)");
        }

        public void Save(string value)
        {
            throw new InvalidOperationException("store unavailable (test)");
        }
    }

    /// <summary>Shared builders for test configs and actions.</summary>
    public static class Helpers
    {
        public const string TestPassword = "S3cr3t-Test-Pw-42";

        /// <summary>Builds an action from explicit URL templates, all with the same DelayAfterMs.</summary>
        public static DomeAction ActionOf(bool parallel, int delayAfterMs, params string[] urlTemplates)
        {
            var action = new DomeAction();
            action.Parallel = parallel;
            foreach (string template in urlTemplates)
            {
                var step = new CommandStep();
                step.UrlTemplate = template;
                step.DelayAfterMs = delayAfterMs;
                action.Steps.Add(step);
            }
            return action;
        }

        /// <summary>Valid config pointing at the given loopback host:port. Open = relays 1,2,3; Close = relay 4.</summary>
        public static DomeConfig MakeConfig(string hostPort)
        {
            var c = new DomeConfig();
            c.Host = hostPort;
            c.UseHttps = false;
            c.Auth = AuthMode.Basic;
            c.Username = "alice";
            c.OpenTravelSeconds = 10;
            c.CloseTravelSeconds = 10;
            c.RequestTimeoutMs = 5000;
            c.AssumedInitialState = ShutterPhase.Closed;
            c.OpenAction = ActionOf(false, 0,
                "{scheme}://{host}/test/pulse?ch=1",
                "{scheme}://{host}/test/pulse?ch=2",
                "{scheme}://{host}/test/pulse?ch=3");
            c.CloseAction = ActionOf(false, 0, "{scheme}://{host}/test/pulse?ch=4");
            return c;
        }

        /// <summary>The URL the controller should produce for relay N on the fake host.</summary>
        public static string LocalUrl(string hostPort, int relay)
        {
            return "http://" + hostPort + "/test/pulse?ch=" + relay;
        }

        public static string TempPath()
        {
            return Path.Combine(Path.GetTempPath(), "clamshell-test-" + Guid.NewGuid().ToString("N") + ".json");
        }

        public static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    /// <summary>Controller under test with a fake clock, a recording sender and in-memory state.</summary>
    public sealed class ControllerFixture
    {
        public const string FakeHostPort = "127.0.0.1:8080";

        public DateTime Clock = new DateTime(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);
        public readonly DomeConfig Config = Helpers.MakeConfig(FakeHostPort);
        public readonly FakeSender Sender = new FakeSender();
        public readonly NullSecretProtector Protector = new NullSecretProtector();
        public readonly MemoryStateStore Store = new MemoryStateStore();
        public readonly LogCollector Log = new LogCollector();
        public readonly DomeController Controller;

        public ControllerFixture()
        {
            Config.SetPassword(Helpers.TestPassword, Protector);
            Controller = Build(Store);
        }

        /// <summary>Creates a new controller on the same config/sender/clock (e.g. to simulate a restart).</summary>
        public DomeController Build(IStateStore store)
        {
            return new DomeController(() => Config, Sender, Protector, store, Log.Write, () => Clock);
        }
    }
}
