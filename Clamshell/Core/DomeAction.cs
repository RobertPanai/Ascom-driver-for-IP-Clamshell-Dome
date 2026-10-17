using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASCOM.Clamshell.Core
{
    /// <summary>A dome command (open or close): a list of HTTP GET steps.</summary>
    [DataContract]
    public class DomeAction
    {
        /// <summary>Steps of the action, executed in order unless <see cref="Parallel"/> is set.</summary>
        [DataMember] public List<CommandStep> Steps = new List<CommandStep>();

        /// <summary>When true, all steps are sent at the same time.</summary>
        [DataMember] public bool Parallel;

        /// <summary>True when the action has no steps (it is not usable yet).</summary>
        public bool IsEmpty
        {
            get { return Steps == null || Steps.Count == 0; }
        }

        /// <summary>
        /// Quick builder used by the wizard. Creates one step per non-empty relay.
        /// Each step URL is "{scheme}://{host}" plus the path template, where "{relay}"
        /// is replaced by the escaped relay number.
        /// </summary>
        /// <param name="pathTemplate">Path template containing "{relay}". A leading '/' is added when missing.</param>
        /// <param name="relaysCsv">Relay identifiers separated by comma, semicolon or space.</param>
        /// <param name="parallel">Value of <see cref="Parallel"/> for the new action.</param>
        /// <param name="delayBetweenMs">Delay copied into every step (applied after each step except the last).</param>
        /// <returns>The new action, with no steps when no relay was given.</returns>
        public static DomeAction FromTemplate(string pathTemplate, string relaysCsv, bool parallel, int delayBetweenMs)
        {
            string path = (pathTemplate ?? "").Trim();
            if (path.Length == 0 || path[0] != '/')
            {
                path = "/" + path;
            }

            DomeAction action = new DomeAction();
            action.Parallel = parallel;
            if (string.IsNullOrEmpty(relaysCsv))
            {
                return action;
            }

            string[] parts = relaysCsv.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string relay = part.Trim();
                if (relay.Length == 0)
                {
                    continue;
                }

                string url = "{scheme}://{host}" + path.Replace("{relay}", Uri.EscapeDataString(relay));
                action.Steps.Add(new CommandStep(url, delayBetweenMs));
            }

            return action;
        }
    }
}
