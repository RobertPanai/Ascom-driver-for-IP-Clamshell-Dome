using System;
using System.Globalization;
using System.Net.Sockets;

namespace ASCOM.Clamshell.Core
{
    /// <summary>Checks whether the relay board accepts TCP connections. Sends no HTTP request.</summary>
    public static class Reachability
    {
        /// <summary>
        /// Opens a plain TCP connection to the host and closes it again.
        /// The host may contain a port (name:8080, or [IPv6]:8080). The default port is 80, or 443 when useHttps is true.
        /// </summary>
        /// <param name="host">Host name or IP address, optionally with :port.</param>
        /// <param name="useHttps">Selects the default port (443 instead of 80).</param>
        /// <param name="timeoutMs">Maximum time to wait for the connection.</param>
        /// <param name="error">Set to a user-facing message when the result is false.</param>
        /// <returns>True when the TCP connection succeeded.</returns>
        public static bool TryTcpConnect(string host, bool useHttps, int timeoutMs, out string error)
        {
            error = "";
            string hostOnly;
            int port;
            string parseError;
            if (!TryParseHostPort(host, useHttps ? 443 : 80, out hostOnly, out port, out parseError))
            {
                error = parseError;
                return false;
            }

            string bracketed = hostOnly.IndexOf(':') >= 0 ? "[" + hostOnly + "]" : hostOnly;
            try
            {
                Uri target = new Uri((useHttps ? "https" : "http") + "://" + bracketed + ":" + port.ToString(CultureInfo.InvariantCulture) + "/");
                SafetyGuard.EnsureAllowed(target);
            }
            catch (DomeCommandException ex)
            {
                error = ex.Message;
                return false;
            }
            catch (UriFormatException)
            {
                error = "Invalid host name: " + hostOnly;
                return false;
            }

            TcpClient client = new TcpClient();
            try
            {
                IAsyncResult ar = client.BeginConnect(hostOnly, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(Math.Max(1, timeoutMs)))
                {
                    error = "Connection to " + hostOnly + ":" + port.ToString(CultureInfo.InvariantCulture) + " timed out after " + timeoutMs.ToString(CultureInfo.InvariantCulture) + " ms.";
                    return false;
                }

                client.EndConnect(ar);
                return client.Connected;
            }
            catch (Exception ex)
            {
                error = "Cannot reach " + hostOnly + ":" + port.ToString(CultureInfo.InvariantCulture) + ": " + ex.Message;
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        private static bool TryParseHostPort(string input, int defaultPort, out string host, out int port, out string error)
        {
            host = null;
            port = defaultPort;
            error = null;

            string h = (input ?? "").Trim();
            if (h.Length == 0)
            {
                error = "Host is empty.";
                return false;
            }

            if (h.IndexOfAny(new[] { '/', '\\', '@', '?', '#', ' ', '\t' }) >= 0 || h.IndexOf("://", StringComparison.Ordinal) >= 0)
            {
                error = "Host must be an IP address or host name, optionally with a port, without a scheme or path.";
                return false;
            }

            string hostPart;
            string portPart = null;
            if (h[0] == '[')
            {
                int close = h.IndexOf(']');
                if (close < 2)
                {
                    error = "Invalid IPv6 host: missing ']'.";
                    return false;
                }

                hostPart = h.Substring(1, close - 1);
                string rest = h.Substring(close + 1);
                if (rest.Length > 0)
                {
                    if (rest[0] != ':')
                    {
                        error = "Invalid IPv6 host.";
                        return false;
                    }

                    portPart = rest.Substring(1);
                }
            }
            else
            {
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
                    int idx = h.IndexOf(':');
                    hostPart = h.Substring(0, idx);
                    portPart = h.Substring(idx + 1);
                }
                else
                {
                    // No port, or a bare IPv6 address (more than one colon).
                    hostPart = h;
                }
            }

            if (string.IsNullOrEmpty(hostPart))
            {
                error = "Host is empty.";
                return false;
            }

            if (portPart != null)
            {
                int parsed;
                if (!int.TryParse(portPart, NumberStyles.None, CultureInfo.InvariantCulture, out parsed) || parsed < 1 || parsed > 65535)
                {
                    error = "Invalid port: " + portPart;
                    return false;
                }

                port = parsed;
            }

            host = hostPart;
            return true;
        }
    }
}
