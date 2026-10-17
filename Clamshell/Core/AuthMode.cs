namespace ASCOM.Clamshell.Core
{
    /// <summary>HTTP authentication used for the dome commands.</summary>
    public enum AuthMode
    {
        /// <summary>No Authorization header is sent.</summary>
        None = 0,
        /// <summary>HTTP Basic authentication (Authorization header, never in the URL).</summary>
        Basic = 1
    }
}
