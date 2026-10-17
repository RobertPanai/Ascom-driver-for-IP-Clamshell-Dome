using System.Text.RegularExpressions;

namespace ASCOM.Clamshell.Core
{
    /// <summary>Removes secrets from text before it is logged.</summary>
    public static class LogMask
    {
        private static readonly Regex UserInfoRegex =
            new Regex(@"^([A-Za-z][A-Za-z0-9+.\-]*://)[^/?#]*@", RegexOptions.CultureInvariant);

        private static readonly Regex SecretKeyRegex =
            new Regex("(pass|pwd|token|key|secret)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// Removes "user:pass@" from the URL and replaces the values of query parameters whose
        /// name contains pass, pwd, token, key or secret with ***.
        /// </summary>
        /// <param name="url">URL to mask.</param>
        /// <returns>The masked URL ("" for null).</returns>
        public static string MaskUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }

            string result = UserInfoRegex.Replace(url, "$1");

            int q = result.IndexOf('?');
            if (q < 0)
            {
                return result;
            }

            string prefix = result.Substring(0, q + 1);
            string rest = result.Substring(q + 1);
            string fragment = "";
            int hash = rest.IndexOf('#');
            if (hash >= 0)
            {
                fragment = rest.Substring(hash);
                rest = rest.Substring(0, hash);
            }

            string[] pairs = rest.Split('&');
            for (int i = 0; i < pairs.Length; i++)
            {
                int eq = pairs[i].IndexOf('=');
                if (eq > 0 && SecretKeyRegex.IsMatch(pairs[i].Substring(0, eq)))
                {
                    pairs[i] = pairs[i].Substring(0, eq + 1) + "***";
                }
            }

            return prefix + string.Join("&", pairs) + fragment;
        }

        /// <summary>Replaces every occurrence of a non-empty secret with ***.</summary>
        /// <param name="text">Text to mask.</param>
        /// <param name="secret">Secret to hide.</param>
        /// <returns>The masked text ("" for null).</returns>
        public static string MaskText(string text, string secret)
        {
            if (text == null)
            {
                return "";
            }

            if (string.IsNullOrEmpty(secret))
            {
                return text;
            }

            return text.Replace(secret, "***");
        }
    }
}
