using System;
using System.Threading;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Simulation sender: performs no network I/O at all. It only logs the request it would have sent
    /// and reports success after a short delay.
    /// </summary>
    public class SimulatedPulseSender : IPulseSender
    {
        private const int SimulatedDelayMs = 100;
        private const string LogCategory = "Simulated";

        private readonly Action<string, string> _log;

        /// <summary>Creates a simulated sender that writes its log lines through <paramref name="log"/>.</summary>
        /// <param name="log">Log callback: (category, text).</param>
        public SimulatedPulseSender(Action<string, string> log)
        {
            _log = log;
        }

        /// <summary>Logs the request and returns success after about 100 ms. No network access.</summary>
        public SendResult Send(string url, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex)
        {
            string masked = string.IsNullOrEmpty(url) ? "" : LogMask.MaskUrl(url);
            string line = "SIMULATED GET " + masked;
            if (!string.IsNullOrEmpty(password))
            {
                line = LogMask.MaskText(line, password);
            }
            Log(line);

            Thread.Sleep(SimulatedDelayMs);
            return new SendResult { Success = true, StatusCode = 200, Error = "", BodySnippet = "" };
        }

        private void Log(string text)
        {
            if (_log == null)
            {
                return;
            }
            try
            {
                _log(LogCategory, text);
            }
            catch (Exception)
            {
                // logging must never break a simulated pulse
            }
        }
    }
}
