using System;
using System.Net;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Guards against contacting hosts that are not allowed. In test mode
    /// (environment variable CLAMSHELL_TEST_MODE=1) only loopback hosts are allowed.
    /// </summary>
    public static class SafetyGuard
    {
        /// <summary>Name of the environment variable that enables test mode.</summary>
        public const string TestModeEnvVar = "CLAMSHELL_TEST_MODE";

        /// <summary>True when the test mode environment variable is set to "1".</summary>
        public static bool IsTestMode
        {
            get { return Environment.GetEnvironmentVariable(TestModeEnvVar) == "1"; }
        }

        /// <summary>
        /// True when the host is localhost, 127.0.0.0/8 or ::1. The host may contain a port
        /// and IPv6 addresses may be in brackets.
        /// </summary>
        /// <param name="host">Host, optionally with :port.</param>
        public static bool IsLoopbackHost(string host)
        {
            string h = ExtractHostOnly(host);
            if (string.IsNullOrEmpty(h))
            {
                return false;
            }

            if (string.Equals(h, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            IPAddress ip;
            if (!IPAddress.TryParse(h, out ip))
            {
                return false;
            }

            return IPAddress.IsLoopback(ip);
        }

        /// <summary>
        /// Throws when the URI must not be contacted. Only http and https are ever allowed.
        /// In test mode only loopback hosts are allowed. The message never contains the URL.
        /// </summary>
        /// <param name="uri">URI about to be contacted.</param>
        public static void EnsureAllowed(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                throw new DomeCommandException("Only http and https requests are allowed.");
            }

            if (!IsTestMode)
            {
                return;
            }

            if (!IsLoopbackHost(uri.Host))
            {
                throw new DomeCommandException("Test mode is on (" + TestModeEnvVar + "=1): refusing to contact non-loopback host '" + uri.Host + "'.");
            }
        }

        private static string ExtractHostOnly(string host)
        {
            if (host == null)
            {
                return null;
            }

            string h = host.Trim();
            if (h.StartsWith("[", StringComparison.Ordinal))
            {
                int close = h.IndexOf(']');
                return close > 1 ? h.Substring(1, close - 1) : null;
            }

            int colons = 0;
            foreach (char c in h)
            {
                if (c == ':')
                {
                    colons++;
                }
            }

            if (colons == 1)
            {
                return h.Substring(0, h.IndexOf(':'));
            }

            return h;
        }
    }
}
