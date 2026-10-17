using System;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Raised when a dome command cannot be executed or a configuration is invalid.
    /// The message is meant to be shown to the user and never contains secrets.
    /// </summary>
    public class DomeCommandException : Exception
    {
        /// <summary>Creates an exception with a user-facing message.</summary>
        /// <param name="message">Message to show to the user.</param>
        public DomeCommandException(string message) : base(message)
        {
        }

        /// <summary>Creates an exception with a user-facing message and an inner exception.</summary>
        /// <param name="message">Message to show to the user.</param>
        /// <param name="inner">The underlying exception.</param>
        public DomeCommandException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
