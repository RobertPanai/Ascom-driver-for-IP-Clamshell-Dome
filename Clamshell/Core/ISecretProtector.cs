namespace ASCOM.Clamshell.Core
{
    /// <summary>Protects secrets (the password) before they are stored.</summary>
    public interface ISecretProtector
    {
        /// <summary>Protects a plain text secret. Returns "" for an empty input.</summary>
        /// <param name="plain">Plain text secret.</param>
        /// <returns>Protected text that can be stored.</returns>
        string Protect(string plain);

        /// <summary>Reverses <see cref="Protect"/>.</summary>
        /// <param name="protectedText">Text produced by Protect.</param>
        /// <returns>The plain text, or "" on failure.</returns>
        string Unprotect(string protectedText);
    }
}
