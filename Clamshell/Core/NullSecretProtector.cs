using System;
using System.Text;

namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// TEST ONLY. Stores the secret as base64 with prefix "test1:". This is NOT secure
    /// and must not be used by the driver.
    /// </summary>
    public class NullSecretProtector : ISecretProtector
    {
        private const string Prefix = "test1:";

        /// <inheritdoc/>
        public string Protect(string plain)
        {
            if (string.IsNullOrEmpty(plain))
            {
                return "";
            }

            return Prefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
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
                return Encoding.UTF8.GetString(Convert.FromBase64String(protectedText.Substring(Prefix.Length)));
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
