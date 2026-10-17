namespace ASCOM.Clamshell.Core
{
    /// <summary>Outcome of a single pulse request sent to the relay board.</summary>
    public class SendResult
    {
        /// <summary>True only when the board answered with a 2xx status and the optional response check passed.</summary>
        public bool Success;

        /// <summary>HTTP status code, 0 when no response was received.</summary>
        public int StatusCode;

        /// <summary>Human readable error text, empty on success. Never contains the password.</summary>
        public string Error = "";

        /// <summary>First characters of the response body (at most 200), for diagnostics.</summary>
        public string BodySnippet = "";
    }
}
