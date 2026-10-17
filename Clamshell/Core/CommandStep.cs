using System.Runtime.Serialization;

namespace ASCOM.Clamshell.Core
{
    /// <summary>One HTTP GET request that is part of a dome action.</summary>
    [DataContract]
    public class CommandStep
    {
        /// <summary>
        /// URL template, for example "{scheme}://{host}/x?ch=1".
        /// Supported placeholders: {scheme} and {host}.
        /// </summary>
        [DataMember] public string UrlTemplate = "";

        /// <summary>Pause after this step in sequential mode, in milliseconds (0..60000).</summary>
        [DataMember] public int DelayAfterMs;

        /// <summary>Creates an empty step.</summary>
        public CommandStep()
        {
        }

        /// <summary>Creates a step.</summary>
        /// <param name="urlTemplate">URL template with optional {scheme} and {host} placeholders.</param>
        /// <param name="delayAfterMs">Pause after this step in sequential mode, in milliseconds.</param>
        public CommandStep(string urlTemplate, int delayAfterMs)
        {
            UrlTemplate = urlTemplate;
            DelayAfterMs = delayAfterMs;
        }
    }
}
