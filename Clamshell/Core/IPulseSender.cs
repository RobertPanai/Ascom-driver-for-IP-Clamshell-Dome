namespace ASCOM.Clamshell.Core
{
    /// <summary>Sends one pulse request to the relay board.</summary>
    public interface IPulseSender
    {
        /// <summary>
        /// Sends a single pulse request and reports the outcome. Implementations must not throw.
        /// </summary>
        /// <param name="url">Fully expanded URL of the request.</param>
        /// <param name="auth">Authentication mode.</param>
        /// <param name="username">User name for Basic authentication.</param>
        /// <param name="password">Plain password (already decrypted), never logged.</param>
        /// <param name="timeoutMs">Per request timeout in milliseconds.</param>
        /// <param name="expectedRegex">Optional regex the response body must match; empty means any 2xx is accepted.</param>
        /// <returns>The outcome of the request.</returns>
        SendResult Send(string url, AuthMode auth, string username, string password, int timeoutMs, string expectedRegex);
    }
}
