using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClamshellCore.Tests
{
    /// <summary>One request seen by the mock dome.</summary>
    public sealed class RecordedRequest
    {
        public string RawUrl;
        public string Method;
        public bool HasAuthorization;
        public string Authorization;
        public DateTime TimestampUtc;
    }

    /// <summary>
    /// HttpListener on 127.0.0.1 with a random free port. Records every request and answers with
    /// configurable status, body, delay, redirect and optional Basic auth. Loopback only.
    /// </summary>
    public sealed class MockDomeServer : IDisposable
    {
        private readonly object _gate = new object();
        private readonly List<RecordedRequest> _requests = new List<RecordedRequest>();
        private readonly HttpListener _listener;
        private bool _disposed;

        /// <summary>HTTP status to answer with (ignored when RedirectLocation is set).</summary>
        public int StatusCode = 200;
        /// <summary>Response body.</summary>
        public string Body = "OK";
        /// <summary>Artificial delay before answering.</summary>
        public int DelayMs;
        /// <summary>When set, the mock answers 302 with this Location header.</summary>
        public string RedirectLocation;
        /// <summary>When true, requests without the exact Basic header get 401.</summary>
        public bool RequireBasic;
        public string BasicUser = "";
        public string BasicPassword = "";

        public MockDomeServer()
        {
            int port = FreePort();
            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
            _listener.Start();

            var thread = new Thread(AcceptLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        public int Port { get; private set; }

        public string BaseUrl
        {
            get { return "http://127.0.0.1:" + Port; }
        }

        /// <summary>Host value as used in DomeConfig.Host, e.g. "127.0.0.1:54321".</summary>
        public string HostPort
        {
            get { return "127.0.0.1:" + Port; }
        }

        public List<RecordedRequest> Requests
        {
            get
            {
                lock (_gate)
                {
                    return new List<RecordedRequest>(_requests);
                }
            }
        }

        public int RequestCount
        {
            get
            {
                lock (_gate)
                {
                    return _requests.Count;
                }
            }
        }

        /// <summary>Expected Authorization header value for Basic auth.</summary>
        public static string BasicHeader(string user, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + password));
        }

        /// <summary>Finds a free TCP port on loopback (the probe listener is stopped again).</summary>
        public static int FreePort()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            return port;
        }

        private void AcceptLoop()
        {
            while (true)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = _listener.GetContext();
                }
                catch (Exception)
                {
                    // listener closed by Dispose
                    return;
                }
                ThreadPool.QueueUserWorkItem(state => Handle((HttpListenerContext)state), ctx);
            }
        }

        private void Handle(HttpListenerContext ctx)
        {
            try
            {
                string auth = ctx.Request.Headers["Authorization"];
                lock (_gate)
                {
                    var rec = new RecordedRequest();
                    rec.RawUrl = ctx.Request.RawUrl;
                    rec.Method = ctx.Request.HttpMethod;
                    rec.HasAuthorization = auth != null;
                    rec.Authorization = auth;
                    rec.TimestampUtc = DateTime.UtcNow;
                    _requests.Add(rec);
                }

                if (DelayMs > 0)
                {
                    Thread.Sleep(DelayMs);
                }

                int status = StatusCode;
                string body = Body ?? "";

                if (RequireBasic && auth != BasicHeader(BasicUser, BasicPassword))
                {
                    status = 401;
                    body = "unauthorized";
                    ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=\"mock\"");
                }
                else if (RedirectLocation != null)
                {
                    status = 302;
                    ctx.Response.AddHeader("Location", RedirectLocation);
                }

                byte[] bytes = Encoding.UTF8.GetBytes(body);
                ctx.Response.StatusCode = status;
                ctx.Response.ContentLength64 = bytes.Length;
                ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                ctx.Response.OutputStream.Close();
            }
            catch (Exception)
            {
                // client went away (e.g. it timed out); nothing to do
            }
            finally
            {
                try
                {
                    ctx.Response.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            try
            {
                _listener.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
