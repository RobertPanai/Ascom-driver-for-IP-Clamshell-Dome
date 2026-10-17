using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Complete driver configuration. Defaults leave both actions empty: the user must
    /// enter the commands in the setup wizard. Serialized with <see cref="ConfigSerializer"/>.
    /// </summary>
    [DataContract]
    public class DomeConfig
    {
        /// <summary>Configuration format version.</summary>
        [DataMember] public int Version = 1;

        /// <summary>Host name or IP address of the relay board, optionally with ":port".</summary>
        [DataMember] public string Host = "";

        /// <summary>When true, step URLs use https.</summary>
        [DataMember] public bool UseHttps;

        /// <summary>HTTP authentication mode.</summary>
        [DataMember] public AuthMode Auth = AuthMode.Basic;

        /// <summary>User name for Basic authentication.</summary>
        [DataMember] public string Username = "";

        /// <summary>Password protected with <see cref="ISecretProtector"/>. Never plain text.</summary>
        [DataMember] public string PasswordProtected = "";

        /// <summary>Command sent to open the shutter.</summary>
        [DataMember] public DomeAction OpenAction = new DomeAction();

        /// <summary>Command sent to close the shutter.</summary>
        [DataMember] public DomeAction CloseAction = new DomeAction();

        /// <summary>Remembered wizard input: path template with {relay}.</summary>
        [DataMember] public string QuickPathTemplate = "";

        /// <summary>Remembered wizard input: relays used to open.</summary>
        [DataMember] public string QuickOpenRelays = "";

        /// <summary>Remembered wizard input: relays used to close.</summary>
        [DataMember] public string QuickCloseRelays = "";

        /// <summary>Timeout of each HTTP request, in milliseconds (500..60000).</summary>
        [DataMember] public int RequestTimeoutMs = 5000;

        /// <summary>Regular expression the response body must match. Empty accepts any 2xx response.</summary>
        [DataMember] public string ExpectedResponseRegex = "";

        /// <summary>Time the shutter needs to open, in seconds (1..900). Used to estimate the state.</summary>
        [DataMember] public int OpenTravelSeconds;

        /// <summary>Time the shutter needs to close, in seconds (1..900). Used to estimate the state.</summary>
        [DataMember] public int CloseTravelSeconds;

        /// <summary>State assumed when the driver starts without saved state (Closed or Open only).</summary>
        [DataMember] public ShutterPhase AssumedInitialState = ShutterPhase.Closed;

        /// <summary>When true, no network is used at all; commands are only logged.</summary>
        [DataMember] public bool SimulationMode;

        /// <summary>When true, diagnostic lines are written to the trace log.</summary>
        [DataMember] public bool TraceEnabled = true;

        private static readonly Regex SchemeRegex = new Regex("^https?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>Returns a deep copy of this configuration.</summary>
        public DomeConfig Clone()
        {
            return new DomeConfig
            {
                Version = Version,
                Host = Host,
                UseHttps = UseHttps,
                Auth = Auth,
                Username = Username,
                PasswordProtected = PasswordProtected,
                OpenAction = CopyAction(OpenAction),
                CloseAction = CopyAction(CloseAction),
                QuickPathTemplate = QuickPathTemplate,
                QuickOpenRelays = QuickOpenRelays,
                QuickCloseRelays = QuickCloseRelays,
                RequestTimeoutMs = RequestTimeoutMs,
                ExpectedResponseRegex = ExpectedResponseRegex,
                OpenTravelSeconds = OpenTravelSeconds,
                CloseTravelSeconds = CloseTravelSeconds,
                AssumedInitialState = AssumedInitialState,
                SimulationMode = SimulationMode,
                TraceEnabled = TraceEnabled
            };
        }

        /// <summary>
        /// Checks the configuration. In simulation mode the host and authentication checks are skipped.
        /// The actions, timeouts, travel times, URLs and regular expression are always checked.
        /// </summary>
        /// <param name="problems">Receives one English message per problem (empty when valid).</param>
        /// <returns>True when the configuration is usable.</returns>
        public bool Validate(out List<string> problems)
        {
            problems = new List<string>();
            string scheme = UseHttps ? "https" : "http";
            string sampleHost = string.IsNullOrWhiteSpace(Host) ? "example.invalid" : Host.Trim();

            if (!Enum.IsDefined(typeof(AuthMode), Auth))
            {
                problems.Add("Unknown authentication mode.");
            }

            if (!SimulationMode)
            {
                ValidateHost(problems);
                if (Auth == AuthMode.Basic && string.IsNullOrWhiteSpace(Username))
                {
                    problems.Add("Username is required for Basic authentication.");
                }
            }

            ValidateAction("Open", OpenAction, scheme, sampleHost, problems);
            ValidateAction("Close", CloseAction, scheme, sampleHost, problems);

            if (RequestTimeoutMs < 500 || RequestTimeoutMs > 60000)
            {
                problems.Add("Request timeout must be between 500 and 60000 ms.");
            }

            if (OpenTravelSeconds < 1 || OpenTravelSeconds > 900)
            {
                problems.Add("Open travel time must be between 1 and 900 seconds.");
            }

            if (CloseTravelSeconds < 1 || CloseTravelSeconds > 900)
            {
                problems.Add("Close travel time must be between 1 and 900 seconds.");
            }

            if (AssumedInitialState != ShutterPhase.Closed && AssumedInitialState != ShutterPhase.Open)
            {
                problems.Add("Assumed initial state must be Closed or Open.");
            }

            if (!string.IsNullOrEmpty(ExpectedResponseRegex))
            {
                try
                {
                    new Regex(ExpectedResponseRegex);
                }
                catch (ArgumentException ex)
                {
                    problems.Add("Expected response regex is not valid: " + ex.Message);
                }
            }

            return problems.Count == 0;
        }

        /// <summary>Replaces {scheme} ("http" or "https") and {host} in the template.</summary>
        /// <param name="template">URL template.</param>
        /// <returns>The expanded URL.</returns>
        public string ExpandUrl(string template)
        {
            return Expand(template, UseHttps ? "https" : "http", (Host ?? "").Trim());
        }

        /// <summary>Protects the password and stores the result. An empty or null password clears it.</summary>
        /// <param name="plain">Plain text password.</param>
        /// <param name="p">Protector to use.</param>
        public void SetPassword(string plain, ISecretProtector p)
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            if (string.IsNullOrEmpty(plain))
            {
                PasswordProtected = "";
                return;
            }

            try
            {
                PasswordProtected = p.Protect(plain) ?? "";
            }
            catch (Exception ex)
            {
                throw new DomeCommandException("Could not protect the password: " + ex.Message, ex);
            }
        }

        /// <summary>Returns the plain text password. Returns "" when none is set or decryption fails. Never throws.</summary>
        /// <param name="p">Protector used to store the password.</param>
        public string GetPassword(ISecretProtector p)
        {
            if (p == null || string.IsNullOrEmpty(PasswordProtected))
            {
                return "";
            }

            try
            {
                return p.Unprotect(PasswordProtected) ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void ValidateHost(List<string> problems)
        {
            string h = (Host ?? "").Trim();
            if (h.Length == 0)
            {
                problems.Add("Host is missing. Enter the IP address or host name of the relay board.");
                return;
            }

            bool bad = h.IndexOfAny(new[] { '/', '\\', '@', '?', '#', ' ', '\t' }) >= 0
                       || h.IndexOf("://", StringComparison.Ordinal) >= 0;
            Uri uri;
            if (!bad && (!Uri.TryCreate("http://" + h, UriKind.Absolute, out uri) || string.IsNullOrEmpty(uri.Host)))
            {
                bad = true;
            }

            if (bad)
            {
                problems.Add("Host must be an IP address or host name, optionally with a port (for example 192.0.2.10 or name:8080), without http:// or a path.");
            }
        }

        private static void ValidateAction(string name, DomeAction action, string scheme, string host, List<string> problems)
        {
            if (action == null || action.IsEmpty)
            {
                problems.Add(name + " action has no commands. Add at least one step in the Commands page.");
                return;
            }

            for (int i = 0; i < action.Steps.Count; i++)
            {
                CommandStep step = action.Steps[i];
                string label = name + " step " + (i + 1).ToString();
                if (step == null || string.IsNullOrWhiteSpace(step.UrlTemplate))
                {
                    problems.Add(label + ": URL is empty.");
                    continue;
                }

                if (step.DelayAfterMs < 0 || step.DelayAfterMs > 60000)
                {
                    problems.Add(label + ": delay must be between 0 and 60000 ms.");
                }

                string template = step.UrlTemplate.Trim();
                string masked = LogMask.MaskUrl(template);
                string sample = Expand(template, scheme, host);
                if (sample.IndexOf('{') >= 0 || sample.IndexOf('}') >= 0)
                {
                    problems.Add(label + ": unknown placeholder in " + masked + ". Only {scheme} and {host} are supported.");
                    continue;
                }

                Uri uri;
                if (!Uri.TryCreate(sample, UriKind.Absolute, out uri))
                {
                    problems.Add(label + ": not a valid absolute URL: " + masked);
                    continue;
                }

                if (!SchemeRegex.IsMatch(uri.Scheme))
                {
                    problems.Add(label + ": scheme must be http or https: " + masked);
                    continue;
                }

                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    problems.Add(label + ": do not put credentials in the URL; use the authentication settings.");
                    continue;
                }

                if (string.IsNullOrEmpty(uri.Host))
                {
                    problems.Add(label + ": URL has no host: " + masked);
                }
            }
        }

        private static string Expand(string template, string scheme, string host)
        {
            return (template ?? "").Replace("{scheme}", scheme).Replace("{host}", host);
        }

        private static DomeAction CopyAction(DomeAction source)
        {
            DomeAction copy = new DomeAction();
            if (source == null)
            {
                return copy;
            }

            copy.Parallel = source.Parallel;
            if (source.Steps != null)
            {
                foreach (CommandStep step in source.Steps)
                {
                    copy.Steps.Add(step == null ? null : new CommandStep(step.UrlTemplate, step.DelayAfterMs));
                }
            }

            return copy;
        }
    }
}
