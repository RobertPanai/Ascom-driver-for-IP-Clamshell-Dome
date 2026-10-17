using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace ASCOM.Clamshell.Core
{
    /// <summary>Reads and writes <see cref="DomeConfig"/> as indented JSON.</summary>
    public static class ConfigSerializer
    {
        /// <summary>Serializes the configuration as indented UTF-8 JSON.</summary>
        /// <param name="c">Configuration to serialize.</param>
        /// <returns>The JSON text.</returns>
        public static string ToJson(DomeConfig c)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DomeConfig));
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, false, true, "  "))
                {
                    serializer.WriteObject(writer, c);
                    writer.Flush();
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Parses JSON produced by <see cref="ToJson"/>. Null or empty input gives a new default configuration.
        /// </summary>
        /// <param name="json">JSON text.</param>
        /// <returns>The configuration.</returns>
        /// <exception cref="DomeCommandException">The JSON is not a valid configuration.</exception>
        public static DomeConfig FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new DomeConfig();
            }

            DomeConfig config;
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DomeConfig));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    config = serializer.ReadObject(ms) as DomeConfig;
                }
            }
            catch (DomeCommandException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomeCommandException("Invalid configuration data: " + ex.Message, ex);
            }

            if (config == null)
            {
                return new DomeConfig();
            }

            if (config.OpenAction == null)
            {
                config.OpenAction = new DomeAction();
            }

            if (config.CloseAction == null)
            {
                config.CloseAction = new DomeAction();
            }

            if (config.OpenAction.Steps == null)
            {
                config.OpenAction.Steps = new List<CommandStep>();
            }

            if (config.CloseAction.Steps == null)
            {
                config.CloseAction.Steps = new List<CommandStep>();
            }

            return config;
        }

        /// <summary>Writes the configuration to a JSON file.</summary>
        /// <param name="c">Configuration to export.</param>
        /// <param name="path">Target file path.</param>
        /// <param name="includePassword">When false the protected password is blanked in the file.</param>
        public static void Export(DomeConfig c, string path, bool includePassword)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A file path is required.", "path");
            }

            DomeConfig toWrite = c.Clone();
            if (!includePassword)
            {
                toWrite.PasswordProtected = "";
            }

            string json = ToJson(toWrite);
            try
            {
                File.WriteAllText(path, json, new UTF8Encoding(false));
            }
            catch (IOException ex)
            {
                throw new DomeCommandException("Could not write the configuration file: " + ex.Message, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DomeCommandException("Could not write the configuration file: " + ex.Message, ex);
            }
        }

        /// <summary>Reads a configuration from a JSON file.</summary>
        /// <param name="path">File to read.</param>
        /// <returns>The configuration.</returns>
        /// <exception cref="DomeCommandException">The file is missing, unreadable or not a valid configuration.</exception>
        public static DomeConfig Import(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A file path is required.", "path");
            }

            if (!File.Exists(path))
            {
                throw new DomeCommandException("Configuration file not found: " + path);
            }

            string text;
            try
            {
                text = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (IOException ex)
            {
                throw new DomeCommandException("Could not read the configuration file: " + ex.Message, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DomeCommandException("Could not read the configuration file: " + ex.Message, ex);
            }

            return FromJson(text);
        }
    }
}
