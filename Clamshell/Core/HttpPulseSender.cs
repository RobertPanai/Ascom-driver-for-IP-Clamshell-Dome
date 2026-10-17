using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Real network sender: HTTP GET only, one request per call. This is the only class in the
    /// driver that performs network I/O for pulses. Redirects are never followed, TLS errors are
    /// errors, and the password is sent only in the Authorization header, never in the URL or logs.
    /// </summary>
    public class HttpPulseSender : IPulseSender
    {
        private const int MaxBodyBytes = 4096;
        private const int MaxSnippetChars = 200;
        private const int DefaultTimeoutMs = 5000;
        private const string LogCategory = "Http";

        private static readonly HttpClient Client = CreateClient();

        private readonly Action<string, string> _log;

        /// <summary>Creates a sender that writes its log lines through <paramref name="log"/>.</summary>
        /// <param name="log">Log callback: (category, text).</param>
        public HttpPulseSender(Action<string, string> log)
        {
            _log = log;
        }

        private static HttpClient CreateClient()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            handler.UseCookies = false;
            HttpClient client = new HttpClient(handler);
            // The per request timeout is enforced with a CancellationTokenSource in SendCore.
            client.Timeout = Timeout.InfiniteTimeSpan;
            return client;
        }

        /// <summary>Sends one GET request. Never throws; failures are reported in the result.</summary>
        public SendResult Send(string url, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex)
        {
            string secret = password ?? "";
            Stopwatch clock = Stopwatch.StartNew();
            string masked = MaskedUrl(url);
            SendResult result;
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new DomeCommandException("empty URL");
                }
                Uri uri = new Uri(url, UriKind.Absolute);
                SafetyGuard.EnsureAllowed(uri);
                result = SendCore(uri, auth, username, secret, timeoutMs, expectedRegex);
            }
            catch (DomeCommandException ex)
            {
                result = Failure(0, "not sent: " + ex.Message, "");
            }
            catch (Exception ex)
            {
                result = Failure(0, "request failed: " + Describe(ex), "");
            }

            result.Error = Mask(result.Error, secret);
            result.BodySnippet = Mask(result.BodySnippet, secret);

            string line = "GET " + masked + " -> " + result.StatusCode + " in " + clock.ElapsedMilliseconds + " ms";
            if (!result.Success)
            {
                line += ": " + result.Error;
            }
            Log(Mask(line, secret));
            return result;
        }

        private static SendResult SendCore(Uri uri, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex)
        {
            int effectiveTimeout = timeoutMs > 0 ? timeoutMs : DefaultTimeoutMs;
            using (CancellationTokenSource cts = new CancellationTokenSource(effectiveTimeout))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                if (auth == AuthMode.Basic && !string.IsNullOrEmpty(username))
                {
                    byte[] raw = Encoding.UTF8.GetBytes(username + ":" + password);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(raw));
                }

                try
                {
                    return SendAndReadAsync(request, password, expectedRegex, cts.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    return Failure(0, "timeout after " + effectiveTimeout + " ms", "");
                }
            }
        }

        private static async Task<SendResult> SendAndReadAsync(HttpRequestMessage request, string password, string expectedRegex, CancellationToken token)
        {
            using (HttpResponseMessage response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
            {
                int status = (int)response.StatusCode;
                string body = Mask(await ReadBodyAsync(response.Content, token).ConfigureAwait(false), password);
                string snippet = Truncate(body, MaxSnippetChars);

                if (status >= 300 && status <= 399)
                {
                    return Failure(status, "redirect not followed (HTTP " + status + ")", snippet);
                }
                if (status < 200 || status > 299)
                {
                    return Failure(status, "HTTP " + status, snippet);
                }
                if (!string.IsNullOrEmpty(expectedRegex) && !MatchesRegex(expectedRegex, body))
                {
                    return Failure(status, "response does not match the expected pattern", snippet);
                }
                return new SendResult { Success = true, StatusCode = status, Error = "", BodySnippet = snippet };
            }
        }

        /// <summary>Reads at most <see cref="MaxBodyBytes"/> bytes (so at most 4096 characters) of the body.</summary>
        private static async Task<string> ReadBodyAsync(HttpContent content, CancellationToken token)
        {
            if (content == null)
            {
                return "";
            }
            using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                byte[] buffer = new byte[MaxBodyBytes];
                int total = 0;
                while (total < buffer.Length)
                {
                    int read = await stream.ReadAsync(buffer, total, buffer.Length - total, token).ConfigureAwait(false);
                    if (read <= 0)
                    {
                        break;
                    }
                    total += read;
                }
                return Encoding.UTF8.GetString(buffer, 0, total);
            }
        }

        private static bool MatchesRegex(string pattern, string body)
        {
            Regex regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(2));
            return regex.IsMatch(body ?? "");
        }

        private static SendResult Failure(int status, string error, string snippet)
        {
            return new SendResult
            {
                Success = false,
                StatusCode = status,
                Error = error ?? "",
                BodySnippet = snippet ?? ""
            };
        }

        private static string Describe(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }
            return ex.Message + " (" + ex.InnerException.Message + ")";
        }

        private static string MaskedUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }
            try
            {
                return LogMask.MaskUrl(url);
            }
            catch (Exception)
            {
                return "<unparseable URL>";
            }
        }

        private static string Mask(string text, string secret)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(secret))
            {
                return text ?? "";
            }
            return LogMask.MaskText(text, secret);
        }

        private static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            return text.Length <= max ? text : text.Substring(0, max);
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
                // logging must never break a pulse
            }
        }
    }
}
