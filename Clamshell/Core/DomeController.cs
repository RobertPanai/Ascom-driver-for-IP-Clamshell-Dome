using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// State machine of the shutter. The board has no feedback, so the state is estimated: a
    /// command moves the phase to Opening/Closing and the phase settles to Open/Closed after the
    /// configured travel time. There is no stop command: a pulse is sent once per relay and never
    /// while the opposite movement is in progress.
    /// </summary>
    public class DomeController
    {
        private const int MaxErrorChars = 500;
        private const string LogCategory = "Controller";

        // _commandLock serializes commands (two clients can never fire pulses at the same time).
        // _lock only protects the in-memory state and is never held while a request is on the wire,
        // so reading the status stays instant even while a command is being sent.
        private readonly object _commandLock = new object();
        private readonly object _lock = new object();
        private readonly Func<DomeConfig> _configProvider;
        private readonly IPulseSender _sender;
        private readonly IPulseSender _simulated;
        private readonly ISecretProtector _protector;
        private readonly IStateStore _store;
        private readonly Action<string, string> _log;
        private readonly Func<DateTime> _utcNow;

        private ShutterPhase _phase = ShutterPhase.Closed;
        private DateTime _endTimeUtc = default(DateTime);
        private string _lastError = "";
        private bool _sending;
        private ShutterPhase _pendingPhase = ShutterPhase.Closed;

        /// <summary>
        /// Creates the controller and restores the persisted state. Never throws: any problem while
        /// reading the configuration or the stored state falls back to the assumed initial state.
        /// </summary>
        /// <param name="configProvider">Returns the current configuration (called on every command).</param>
        /// <param name="sender">Pulse sender. Simulation mode always uses an internal simulated sender.</param>
        /// <param name="protector">Used to decrypt the stored password.</param>
        /// <param name="store">Persistent state store.</param>
        /// <param name="log">Log callback: (category, text).</param>
        /// <param name="utcNow">UTC clock (injectable for tests).</param>
        public DomeController(Func<DomeConfig> configProvider, IPulseSender sender, ISecretProtector protector, IStateStore store, Action<string, string> log, Func<DateTime> utcNow)
        {
            _log = log ?? ((category, text) => { });
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            _configProvider = configProvider ?? (() => new DomeConfig());
            _sender = sender ?? new RefusingSender();
            _simulated = new SimulatedPulseSender(_log);
            _protector = protector;
            _store = store ?? new MemoryStateStore();

            try
            {
                Restore();
            }
            catch (Exception ex)
            {
                _phase = ShutterPhase.Closed;
                _endTimeUtc = default(DateTime);
                _lastError = "";
                Log("could not restore the saved state, assuming Closed: " + ex.Message);
            }
        }

        /// <summary>Current phase. Opening and Closing settle to Open and Closed once the travel time has elapsed.</summary>
        public ShutterPhase Phase
        {
            get
            {
                lock (_lock)
                {
                    if (_sending)
                    {
                        return _pendingPhase;
                    }
                    SettleLocked();
                    return _phase;
                }
            }
        }

        /// <summary>Last error text, empty when there is none.</summary>
        public string LastError
        {
            get
            {
                lock (_lock)
                {
                    return _lastError ?? "";
                }
            }
        }

        /// <summary>Estimated seconds left in the current movement, 0 when not moving.</summary>
        public double SecondsRemaining
        {
            get
            {
                lock (_lock)
                {
                    SettleLocked();
                    if (_phase != ShutterPhase.Opening && _phase != ShutterPhase.Closing)
                    {
                        return 0;
                    }
                    double seconds = (_endTimeUtc - Now()).TotalSeconds;
                    return seconds > 0 ? seconds : 0;
                }
            }
        }

        /// <summary>
        /// Opens the shutter: sends the open action once when Closed or Error. Does nothing when already
        /// Open or Opening. Throws when the shutter is Closing, or when the action fails (phase becomes Error).
        /// </summary>
        public void Open()
        {
            lock (_commandLock)
            {
                DomeConfig config = ReadValidConfig();
                lock (_lock)
                {
                    SettleLocked();
                    switch (_phase)
                    {
                        case ShutterPhase.Open:
                        case ShutterPhase.Opening:
                            Log("open requested but already " + _phase.ToString().ToLowerInvariant() + ", no pulse sent");
                            return;

                        case ShutterPhase.Closing:
                            Log("open refused: shutter is closing");
                            throw new DomeCommandException("Shutter is closing, wait until it has finished");
                    }
                }

                // Closed or Error
                RunTransition(config, config.OpenAction, "Open", ShutterPhase.Opening, config.OpenTravelSeconds);
            }
        }

        /// <summary>
        /// Closes the shutter: sends the close action once when Open or Error. Does nothing when already
        /// Closed or Closing. Throws when the shutter is Opening, or when the action fails (phase becomes Error).
        /// </summary>
        public void Close()
        {
            lock (_commandLock)
            {
                DomeConfig config = ReadValidConfig();
                lock (_lock)
                {
                    SettleLocked();
                    switch (_phase)
                    {
                        case ShutterPhase.Closed:
                        case ShutterPhase.Closing:
                            Log("close requested but already " + _phase.ToString().ToLowerInvariant() + ", no pulse sent");
                            return;

                        case ShutterPhase.Opening:
                            Log("close refused: shutter is opening");
                            throw new DomeCommandException("Shutter is opening, wait until it has finished");
                    }
                }

                // Open or Error
                RunTransition(config, config.CloseAction, "Close", ShutterPhase.Closing, config.CloseTravelSeconds);
            }
        }

        /// <summary>Sets the estimated state manually (Open or Closed only). Clears any error and persists.</summary>
        /// <param name="phase">Open or Closed.</param>
        public void SetAssumedState(ShutterPhase phase)
        {
            if (phase != ShutterPhase.Open && phase != ShutterPhase.Closed)
            {
                throw new ArgumentException("Assumed state must be Open or Closed", "phase");
            }

            lock (_commandLock)
            {
                lock (_lock)
                {
                    _phase = phase;
                    _endTimeUtc = default(DateTime);
                    _lastError = "";
                    Persist();
                    Log("state set manually to " + phase);
                }
            }
        }

        /// <summary>One line human readable summary, for logs and actions.</summary>
        public string GetStateInfo()
        {
            lock (_lock)
            {
                if (_sending)
                {
                    return _pendingPhase + " (command being sent)";
                }
                SettleLocked();
                switch (_phase)
                {
                    case ShutterPhase.Opening:
                    case ShutterPhase.Closing:
                        double seconds = (_endTimeUtc - Now()).TotalSeconds;
                        long rounded = seconds > 0 ? (long)Math.Ceiling(seconds) : 0;
                        return _phase + " (estimated, about " + rounded.ToString(CultureInfo.InvariantCulture) + " s left)";
                    case ShutterPhase.Error:
                        return "Error: " + (_lastError ?? "") + " (check the dome visually before retrying)";
                    default:
                        return _phase + " (estimated, no sensor feedback)";
                }
            }
        }

        /// <summary>
        /// Runs an action and moves to the target phase on success, or to Error on failure.
        /// Must be called with _commandLock held and WITHOUT _lock: the requests are sent outside the state lock.
        /// </summary>
        private void RunTransition(DomeConfig config, DomeAction action, string label, ShutterPhase target, int travelSeconds)
        {
            lock (_lock)
            {
                _sending = true;
                _pendingPhase = target;
            }

            string error = "";
            bool ok = false;
            try
            {
                ok = ExecuteAction(config, action, label, out error);
            }
            catch (Exception ex)
            {
                error = label + " failed unexpectedly: " + ex.Message;
            }

            lock (_lock)
            {
                _sending = false;
                if (ok)
                {
                    _phase = target;
                    _endTimeUtc = Now().AddSeconds(travelSeconds);
                    _lastError = "";
                    Persist();
                    Log(label + " sent, estimated travel " + travelSeconds.ToString(CultureInfo.InvariantCulture) + " s");
                    return;
                }

                _phase = ShutterPhase.Error;
                _endTimeUtc = default(DateTime);
                _lastError = OneLine(error);
                Persist();
                throw new DomeCommandException(_lastError);
            }
        }

        /// <summary>
        /// Executes all steps of an action. Sequential: in order, stop at the first failure.
        /// Parallel: all at once, wait for all, any failure is a failure.
        /// </summary>
        private bool ExecuteAction(DomeConfig config, DomeAction action, string label, out string error)
        {
            error = "";
            if (action == null || action.Steps == null || action.Steps.Count == 0)
            {
                error = label + " action is empty";
                return false;
            }

            string password = FetchPassword(config);
            List<CommandStep> steps = action.Steps;
            int total = steps.Count;

            // Expand every URL before sending anything, so a malformed step sends nothing.
            string[] urls = new string[total];
            for (int i = 0; i < total; i++)
            {
                CommandStep step = steps[i];
                if (step == null || string.IsNullOrEmpty(step.UrlTemplate))
                {
                    error = FailureText(label, i, total, "", "the step has no URL", password);
                    return false;
                }
                urls[i] = config.ExpandUrl(step.UrlTemplate);
            }

            if (action.Parallel)
            {
                return ExecuteParallel(config, label, urls, password, out error);
            }
            return ExecuteSequential(config, steps, label, urls, password, out error);
        }

        private bool ExecuteSequential(DomeConfig config, List<CommandStep> steps, string label, string[] urls, string password, out string error)
        {
            error = "";
            int total = urls.Length;
            for (int i = 0; i < total; i++)
            {
                Log(label + " step " + (i + 1) + "/" + total + ": " + SafeMaskUrl(urls[i], password), password);
                SendResult result = SafeSend(urls[i], config, password);
                if (!result.Success)
                {
                    error = FailureText(label, i, total, urls[i], result.Error, password);
                    Log(error, password);
                    return false;
                }

                int delay = steps[i].DelayAfterMs;
                if (i < total - 1 && delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
            return true;
        }

        private bool ExecuteParallel(DomeConfig config, string label, string[] urls, string password, out string error)
        {
            error = "";
            int total = urls.Length;
            Task<SendResult>[] tasks = new Task<SendResult>[total];
            for (int i = 0; i < total; i++)
            {
                Log(label + " step " + (i + 1) + "/" + total + " (parallel): " + SafeMaskUrl(urls[i], password), password);
                string url = urls[i];
                tasks[i] = Task.Run(() => SafeSend(url, config, password));
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ex)
            {
                error = FailureText(label, 0, total, "", ex.Flatten().Message, password);
                Log(error, password);
                return false;
            }

            for (int i = 0; i < total; i++)
            {
                SendResult result = tasks[i].Result;
                if (!result.Success)
                {
                    error = FailureText(label, i, total, urls[i], result.Error, password);
                    Log(error, password);
                    return false;
                }
            }
            return true;
        }

        /// <summary>Sends one request and turns any unexpected exception into a failed result.</summary>
        private SendResult SafeSend(string url, DomeConfig config, string password)
        {
            try
            {
                SendResult result = SenderFor(config).Send(url, config.Auth, config.Username, password, config.RequestTimeoutMs, config.ExpectedResponseRegex);
                return result ?? Failure("the sender returned no result");
            }
            catch (Exception ex)
            {
                return Failure(LogMask.MaskText(ex.Message ?? "", password));
            }
        }

        /// <summary>Simulation mode never touches the network, whatever sender was injected.</summary>
        private IPulseSender SenderFor(DomeConfig config)
        {
            return config.SimulationMode ? _simulated : _sender;
        }

        private string FetchPassword(DomeConfig config)
        {
            if (_protector == null)
            {
                return "";
            }
            try
            {
                return config.GetPassword(_protector) ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private DomeConfig ReadValidConfig()
        {
            DomeConfig config;
            try
            {
                config = _configProvider();
            }
            catch (Exception ex)
            {
                throw new DomeCommandException("Cannot read the configuration: " + ex.Message, ex);
            }

            if (config == null)
            {
                throw new DomeCommandException("Configuration is missing");
            }

            List<string> problems;
            if (!config.Validate(out problems))
            {
                string list = problems == null ? "" : string.Join("; ", problems);
                throw new DomeCommandException("Configuration is invalid: " + list);
            }
            return config;
        }

        private static SendResult Failure(string error)
        {
            return new SendResult { Success = false, StatusCode = 0, Error = error ?? "", BodySnippet = "" };
        }

        private static string FailureText(string label, int index, int total, string url, string reason, string secret)
        {
            string text = label + " failed at step " + (index + 1) + " of " + total;
            if (!string.IsNullOrEmpty(url))
            {
                text += " (" + SafeMaskUrl(url, secret) + ")";
            }
            text += ": " + (reason ?? "unknown error")
                  + ". The dome may have partially moved: check it visually before retrying.";
            return LogMask.MaskText(text, secret ?? "");
        }

        private static string SafeMaskUrl(string url, string secret)
        {
            try
            {
                string masked = LogMask.MaskUrl(url ?? "");
                return string.IsNullOrEmpty(secret) ? masked : LogMask.MaskText(masked, secret);
            }
            catch (Exception)
            {
                return "<unparseable URL>";
            }
        }

        /// <summary>Single line, bounded, safe to store and show.</summary>
        private static string OneLine(string text)
        {
            string line = (text ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();
            return line.Length <= MaxErrorChars ? line : line.Substring(0, MaxErrorChars);
        }

        /// <summary>Moves Opening/Closing to Open/Closed once the end time has passed. Returns true when it changed.</summary>
        private bool SettleLocked()
        {
            if (_phase != ShutterPhase.Opening && _phase != ShutterPhase.Closing)
            {
                return false;
            }
            if (Now() < _endTimeUtc)
            {
                return false;
            }

            _phase = _phase == ShutterPhase.Opening ? ShutterPhase.Open : ShutterPhase.Closed;
            _endTimeUtc = default(DateTime);
            Persist();
            return true;
        }

        private DateTime Now()
        {
            try
            {
                return _utcNow();
            }
            catch (Exception)
            {
                return DateTime.UtcNow;
            }
        }

        /// <summary>Persists "phase|endTimeUtcTicks|error". The error has '|' replaced.</summary>
        private void Persist()
        {
            try
            {
                long ticks = (_phase == ShutterPhase.Opening || _phase == ShutterPhase.Closing) ? _endTimeUtc.Ticks : 0L;
                string error = (_lastError ?? "").Replace('|', '/');
                _store.Save(_phase.ToString() + "|" + ticks.ToString(CultureInfo.InvariantCulture) + "|" + error);
            }
            catch (Exception ex)
            {
                Log("could not persist the state: " + ex.Message);
            }
        }

        private void Restore()
        {
            ShutterPhase assumed = AssumedFromConfig();
            string raw = null;
            try
            {
                raw = _store.Load();
            }
            catch (Exception ex)
            {
                Log("could not load the saved state: " + ex.Message);
            }

            ShutterPhase phase;
            long ticks;
            string error;
            if (TryParseState(raw, out phase, out ticks, out error))
            {
                _phase = phase;
                _endTimeUtc = ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : default(DateTime);
                _lastError = phase == ShutterPhase.Error ? error : "";
            }
            else
            {
                _phase = assumed;
                _endTimeUtc = default(DateTime);
                _lastError = "";
            }

            // A movement whose end time is already in the past is settled right away.
            SettleLocked();
        }

        private static bool TryParseState(string raw, out ShutterPhase phase, out long ticks, out string error)
        {
            phase = ShutterPhase.Closed;
            ticks = 0;
            error = "";
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] parts = raw.Trim().Split('|');
            if (parts.Length < 2)
            {
                return false;
            }

            ShutterPhase parsed;
            if (!Enum.TryParse<ShutterPhase>(parts[0].Trim(), false, out parsed) || !Enum.IsDefined(typeof(ShutterPhase), parsed))
            {
                return false;
            }

            long endTicks;
            if (!long.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out endTicks))
            {
                return false;
            }
            if (endTicks < 0 || endTicks > DateTime.MaxValue.Ticks)
            {
                return false;
            }

            phase = parsed;
            ticks = endTicks;
            error = parts.Length >= 3 ? string.Join("/", parts.Skip(2)) : "";
            return true;
        }

        private ShutterPhase AssumedFromConfig()
        {
            try
            {
                DomeConfig config = _configProvider();
                if (config != null && config.AssumedInitialState == ShutterPhase.Open)
                {
                    return ShutterPhase.Open;
                }
            }
            catch (Exception ex)
            {
                Log("cannot read the configuration, assuming Closed: " + ex.Message);
            }
            return ShutterPhase.Closed;
        }

        private void Log(string text)
        {
            Log(text, null);
        }

        private void Log(string text, string secret)
        {
            try
            {
                string line = text ?? "";
                if (!string.IsNullOrEmpty(secret))
                {
                    line = LogMask.MaskText(line, secret);
                }
                _log(LogCategory, line);
            }
            catch (Exception)
            {
                // logging must never break a command
            }
        }

        /// <summary>Used when no sender was supplied: it refuses every request instead of pretending to work.</summary>
        private sealed class RefusingSender : IPulseSender
        {
            public SendResult Send(string url, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex)
            {
                return new SendResult { Success = false, StatusCode = 0, Error = "no pulse sender is configured", BodySnippet = "" };
            }
        }
    }
}
