using System;
using System.Security.Cryptography;
using System.Text;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Protects secrets with Windows DPAPI (current user scope).
    /// The output is "dpapi1:" followed by base64 of the encrypted bytes.
    /// </summary>
    public class DpapiSecretProtector : ISecretProtector
    {
        private const string Prefix = "dpapi1:";
        private readonly byte[] _entropy;

        /// <summary>Creates a protector without additional entropy.</summary>
        public DpapiSecretProtector() : this(null)
        {
        }

        /// <summary>Creates a protector with optional additional entropy.</summary>
        /// <param name="entropy">Optional entropy bytes; the same value is needed to unprotect.</param>
        public DpapiSecretProtector(byte[] entropy)
        {
            _entropy = entropy == null ? null : (byte[])entropy.Clone();
        }

        /// <inheritdoc/>
        public string Protect(string plain)
        {
            if (string.IsNullOrEmpty(plain))
            {
                return "";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(plain);
            try
            {
                byte[] encrypted = ProtectedData.Protect(bytes, _entropy, DataProtectionScope.CurrentUser);
                return Prefix + Convert.ToBase64String(encrypted);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        /// <inheritdoc/>
        public string Unprotect(string protectedText)
        {
            if (string.IsNullOrEmpty(protectedText) || !protectedText.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return "";
            }

            try
            {
                byte[] encrypted = Convert.FromBase64String(protectedText.Substring(Prefix.Length));
                byte[] bytes = ProtectedData.Unprotect(encrypted, _entropy, DataProtectionScope.CurrentUser);
                try
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                finally
                {
                    Array.Clear(bytes, 0, bytes.Length);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
