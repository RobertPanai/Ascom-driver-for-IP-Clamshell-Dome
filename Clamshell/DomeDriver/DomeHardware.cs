// ASCOM Dome hardware class for the Clamshell IP relay dome.
//
// Description:	Device logic shared by all driver instances: configuration stored in the ASCOM Profile,
//				the shutter state machine (DomeController) and the ASCOM dome interface.
// Implements:	ASCOM Dome interface version: 2
// Author:		Robert Panai - robertpanai96@gmail.com
//

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Astrometry.NOVAS;
using ASCOM.Clamshell.Core;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ASCOM.Clamshell.Dome
{
    /// <summary>
    /// ASCOM Dome hardware class for the Clamshell IP relay dome.
    /// </summary>
    [HardwareClass()] // Flags this class as a hardware class that the local server disposes when it exits.
    internal static class DomeHardware
    {
        // ASCOM Profile keys (DeviceType "Dome")
        private const string ConfigProfileName = "Config";
        private const string StateProfileName = "State";
        internal const string traceStateProfileName = "Trace Level";
        internal const string traceStateDefault = "true";

        // Legacy keys, read only to migrate a profile written by the previous version.
        private const string LegacyComPortName = "COM Port";
        private const string LegacyComPortDefault = "COM1";
        private const string LegacyUsernameName = "Username";
        private const string LegacyPasswordName = "Password";

        // Returned by Profile.GetValue when a key does not exist.
        private const string AbsentMarker = "\u0001<absent>";

        private const string DriverName = "Clamshell IP Relay Dome";
        private const string DriverSummary = "Clamshell shutter driven by an IP relay board.";
        private const string DriverDetails = "The relay board gives no position feedback: the shutter state is estimated from the configured travel times.";

        private static string DriverProgId = ""; // ASCOM DeviceID (COM ProgID) for this driver, the value is set by the driver's class initialiser.
        private static string DriverDescription = ""; // The value is set by the driver's class initialiser.
        private static bool connectedState; // Local server's connected state
        private static int connectionCount; // Number of ASCOM clients currently connected
        private static bool runOnce = false; // Flag to enable "one-off" activities only to run once.
        private static readonly object controllerSync = new object();

        internal static Util utilities; // ASCOM Utilities object for use as required
        internal static AstroUtils astroUtilities; // ASCOM AstroUtils object for use as required
        internal static TraceLogger tl; // Local server's trace logger object for diagnostic log with information that you specify

        internal static DomeConfig Config = new DomeConfig(); // Current configuration, replaced by the setup dialog on OK
        internal static ISecretProtector Protector = new DpapiSecretProtector(); // Protects the stored password (DPAPI, current user)
        internal static DomeController Controller; // Created lazily, re-created after the configuration changes

        private static readonly System.Action<string, string> ControllerLog = (identifier, message) => LogMessage(identifier, message);

        /// <summary>
        /// Initializes a new instance of the device Hardware class.
        /// </summary>
        static DomeHardware()
        {
            try
            {
                // Create the hardware trace logger in the static initialiser.
                // All other initialisation should go in the InitialiseHardware method.
                tl = new TraceLogger("", "Clamshell.Hardware");

                // DriverProgId has to be set here because it is used by ReadProfile.
                DriverProgId = Dome.DriverProgId;

                // ReadProfile has to go here before anything is written to the log because it loads the TraceLogger enable / disable state.
                ReadProfile();

                LogMessage("DomeHardware", "Static initialiser completed.");
            }
            catch (Exception ex)
            {
                try { LogMessage("DomeHardware", $"Initialisation exception: {ex}"); } catch { }
                MessageBox.Show($"{ex.Message}", "Exception creating ASCOM.Clamshell.Dome", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Initialises the hardware class. Called every time a new ASCOM client loads the driver.
        /// </summary>
        internal static void InitialiseHardware()
        {
            LogMessage("InitialiseHardware", "Start.");

            // Make sure that "one off" activities are only undertaken once
            if (runOnce == false)
            {
                LogMessage("InitialiseHardware", "Starting one-off initialisation.");

                DriverDescription = Dome.DriverDescription; // Get this device's Chooser description

                LogMessage("InitialiseHardware", $"ProgID: {DriverProgId}, Description: {DriverDescription}");

                connectedState = false; // Initialise connected to false
                utilities = new Util(); // Initialise ASCOM Utilities object
                astroUtilities = new AstroUtils(); // Initialise ASCOM Astronomy Utilities object

                LogMessage("InitialiseHardware", "Completed basic initialisation");
                LogMessage("InitialiseHardware", "One-off initialisation complete.");
                runOnce = true; // Set the flag to ensure that this code is not run again
            }
        }

        // PUBLIC COM INTERFACE IDomeV2 IMPLEMENTATION

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialogue form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public static void SetupDialog()
        {
            // The configuration cannot change while a client is connected: the controller may be mid-command.
            if (IsConnected)
            {
                MessageBox.Show(Strings.Get("AlreadyConnected"), Strings.Get("msg.error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SetupDialogForm form = new SetupDialogForm(tl, Config.Clone(), Protector))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Result != null)
                {
                    Config = form.Result;
                    WriteProfile(); // Persist the configuration to the ASCOM Profile store

                    // The controller is re-created with the new configuration on next use.
                    lock (controllerSync)
                    {
                        Controller = null;
                    }
                }
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public static ArrayList SupportedActions
        {
            get
            {
                ArrayList actions = new ArrayList { "SetAssumedState", "GetStateInfo" };
                LogMessage("SupportedActions Get", $"Returning {actions.Count} actions.");
                return actions;
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="actionName">
        /// <c>SetAssumedState</c> (parameter <c>open</c> or <c>closed</c>) records the shutter state when it is known by other means;
        /// <c>GetStateInfo</c> returns a one-line summary of the controller state.
        /// </param>
        /// <param name="actionParameters">Action parameters, or an empty string if none are required.</param>
        /// <returns>A string response.</returns>
        public static string Action(string actionName, string actionParameters)
        {
            string name = (actionName ?? "").Trim();
            string parameters = (actionParameters ?? "").Trim();
            LogMessage("Action", $"Action {name}, parameters {parameters}");

            if (string.Equals(name, "SetAssumedState", StringComparison.OrdinalIgnoreCase))
            {
                ShutterPhase phase;
                if (string.Equals(parameters, "open", StringComparison.OrdinalIgnoreCase))
                    phase = ShutterPhase.Open;
                else if (string.Equals(parameters, "closed", StringComparison.OrdinalIgnoreCase))
                    phase = ShutterPhase.Closed;
                else
                    throw new InvalidValueException("SetAssumedState", parameters, "open or closed");

                GetController().SetAssumedState(phase);
                return GetController().GetStateInfo();
            }

            if (string.Equals(name, "GetStateInfo", StringComparison.OrdinalIgnoreCase))
                return GetController().GetStateInfo();

            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response. Not supported by this driver.
        /// </summary>
        public static void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            throw new MethodNotImplementedException($"CommandBlind - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response. Not supported by this driver.
        /// </summary>
        public static bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            throw new MethodNotImplementedException($"CommandBool - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response. Not supported by this driver.
        /// </summary>
        public static string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            throw new MethodNotImplementedException($"CommandString - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Deterministically release both managed and unmanaged resources that are used by this class.
        /// </summary>
        /// <remarks>
        /// Do not call this method from the Dispose method in the driver class: this hardware class is decorated with
        /// the <see cref="HardwareClassAttribute"/> attribute and the local server calls this method when it shuts down.
        /// </remarks>
        public static void Dispose()
        {
            try { LogMessage("Dispose", "Disposing of assets and closing down."); } catch { }

            try
            {
                // Clean up the trace logger and utility objects
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
            }
            catch { }

            try
            {
                utilities.Dispose();
                utilities = null;
            }
            catch { }

            try
            {
                astroUtilities.Dispose();
                astroUtilities = null;
            }
            catch { }
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// Connecting checks the configuration and that the relay host is reachable over TCP. No command is sent.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public static bool Connected
        {
            get
            {
                LogMessage("Connected", $"Get {IsConnected}");
                return IsConnected;
            }
            set
            {
                LogMessage("Connected", $"Set {value}");

                // Several ASCOM clients can share this hardware class: it stays connected until the last one disconnects.
                if (!value)
                {
                    if (connectionCount > 0)
                        connectionCount--;

                    if (connectionCount == 0)
                    {
                        LogMessage("Connected Set", "Disconnected.");
                        connectedState = false;
                    }
                    else
                    {
                        LogMessage("Connected Set", $"Client disconnected, {connectionCount} still connected.");
                    }
                    return;
                }

                if (connectionCount > 0)
                {
                    connectionCount++;
                    LogMessage("Connected Set", $"Already connected, now {connectionCount} clients.");
                    return;
                }

                {
                    List<string> problems;
                    if (!Config.Validate(out problems))
                        throw new DriverException("Dome is not configured: " + string.Join("; ", problems));

                    if (!Config.SimulationMode)
                    {
                        string error;
                        if (!Reachability.TryTcpConnect(Config.Host, Config.UseHttps, Config.RequestTimeoutMs, out error))
                        {
                            LogMessage("Connected Set", $"Cannot reach host {Config.Host}: {error}");
                            throw new DriverException($"cannot reach host {Config.Host}: {error}");
                        }
                    }

                    LogMessage("Connected Set", Config.SimulationMode ? "Connected (simulation mode)." : $"Connected, host {Config.Host} is reachable.");
                    connectionCount = 1;
                    connectedState = true;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and model number. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public static string Description
        {
            get
            {
                string description = DriverSummary + " " + DriverDetails;
                LogMessage("Description Get", description);
                return description;
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public static string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = $"{DriverName} ASCOM Dome driver. Version: {version.Major}.{version.Minor}. {DriverSummary} {DriverDetails}";
                LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public static string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = $"{version.Major}.{version.Minor}";
                LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        /// <summary>
        /// The interface version number that this device supports.
        /// </summary>
        public static short InterfaceVersion
        {
            get
            {
                LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes
        /// </summary>
        public static string Name
        {
            get
            {
                LogMessage("Name Get", DriverName);
                return DriverName;
            }
        }

        #endregion

        #region IDome Implementation

        /// <summary>
        /// Immediately stops any and all movement of the dome.
        /// </summary>
        /// <remarks>
        /// Not supported: each relay command is a single pulse that cannot be interrupted, so the call only logs.
        /// </remarks>
        internal static void AbortSlew()
        {
            LogMessage("AbortSlew", "Not supported by the relay board (each command is a single pulse). Nothing was sent.");
        }

        /// <summary>
        /// The altitude (degrees, horizon zero and increasing positive to 90 zenith) of the part of the sky that the observer wishes to observe.
        /// </summary>
        internal static double Altitude
        {
            get
            {
                LogMessage("Altitude Get", "Not implemented");
                throw new PropertyNotImplementedException("Altitude", false);
            }
        }

        /// <summary>
        /// <see langword="true" /> when the dome is in the home position. Raises an error if not supported.
        /// </summary>
        internal static bool AtHome
        {
            get
            {
                LogMessage("AtHome Get", "Not implemented");
                throw new PropertyNotImplementedException("AtHome", false);
            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome is in the programmed park position.
        /// </summary>
        internal static bool AtPark
        {
            get
            {
                LogMessage("AtPark Get", "Not implemented");
                throw new PropertyNotImplementedException("AtPark", false);
            }
        }

        /// <summary>
        /// The dome azimuth (degrees, North zero and increasing clockwise, i.e., 90 East, 180 South, 270 West). North is true north and not magnetic north.
        /// </summary>
        internal static double Azimuth
        {
            get
            {
                LogMessage("Azimuth Get", "Not implemented");
                throw new PropertyNotImplementedException("Azimuth", false);
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver can perform a search for home position.
        /// </summary>
        internal static bool CanFindHome
        {
            get
            {
                LogMessage("CanFindHome Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of parking the dome.
        /// </summary>
        internal static bool CanPark
        {
            get
            {
                LogMessage("CanPark Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of setting dome altitude.
        /// </summary>
        internal static bool CanSetAltitude
        {
            get
            {
                LogMessage("CanSetAltitude Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of rotating the dome. Must be <see langword="false" /> for a
        /// roll-off roof or clamshell.
        /// </summary>
        internal static bool CanSetAzimuth
        {
            get
            {
                LogMessage("CanSetAzimuth Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver can set the dome park position.
        /// </summary>
        internal static bool CanSetPark
        {
            get
            {
                LogMessage("CanSetPark Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of opening and closing the shutter or roof mechanism.
        /// </summary>
        internal static bool CanSetShutter
        {
            get
            {
                LogMessage("CanSetShutter Get", true.ToString());
                return true;
            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome hardware supports slaving to a telescope.
        /// </summary>
        internal static bool CanSlave
        {
            get
            {
                LogMessage("CanSlave Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of synchronizing the dome azimuth position
        /// using the <see cref="SyncToAzimuth" /> method.
        /// </summary>
        internal static bool CanSyncAzimuth
        {
            get
            {
                LogMessage("CanSyncAzimuth Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// Close the shutter or otherwise shield the telescope from the sky.
        /// </summary>
        internal static void CloseShutter()
        {
            LogMessage("CloseShutter", "Closing the shutter.");
            Execute("CloseShutter", () => GetController().Close());
        }

        /// <summary>
        /// Start operation to search for the dome home position.
        /// </summary>
        internal static void FindHome()
        {
            LogMessage("FindHome", "Not implemented");
            throw new MethodNotImplementedException("FindHome");
        }

        /// <summary>
        /// Open shutter or otherwise expose telescope to the sky.
        /// </summary>
        internal static void OpenShutter()
        {
            LogMessage("OpenShutter", "Opening the shutter.");
            Execute("OpenShutter", () => GetController().Open());
        }

        /// <summary>
        /// Rotate dome in azimuth to park position.
        /// </summary>
        internal static void Park()
        {
            LogMessage("Park", "Not implemented");
            throw new MethodNotImplementedException("Park");
        }

        /// <summary>
        /// Set the current azimuth position of dome to the park position.
        /// </summary>
        internal static void SetPark()
        {
            LogMessage("SetPark", "Not implemented");
            throw new MethodNotImplementedException("SetPark");
        }

        /// <summary>
        /// Gets the status of the dome shutter or roof structure. The state is estimated from the travel times.
        /// </summary>
        internal static ShutterState ShutterStatus
        {
            get
            {
                ShutterState state = ToShutterState(GetController().Phase);
                LogMessage("ShutterStatus", state.ToString());
                return state;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the dome is slaved to the telescope in its hardware, else <see langword="false"/>.
        /// </summary>
        internal static bool Slaved
        {
            get
            {
                LogMessage("Slaved Get", false.ToString());
                return false;
            }
            set
            {
                LogMessage("Slaved Set", "not implemented");
                throw new PropertyNotImplementedException("Slaved", true);
            }
        }

        /// <summary>
        /// Ensure that the requested viewing altitude is available for observing.
        /// </summary>
        /// <param name="Altitude">
        /// The desired viewing altitude (degrees, horizon zero and increasing positive to 90 degrees at the zenith)
        /// </param>
        internal static void SlewToAltitude(double Altitude)
        {
            LogMessage("SlewToAltitude", "Not implemented");
            throw new MethodNotImplementedException("SlewToAltitude");
        }

        /// <summary>
        /// Ensure that the requested viewing azimuth is available for observing.
        /// </summary>
        /// <param name="Azimuth">
        /// Desired viewing azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        internal static void SlewToAzimuth(double Azimuth)
        {
            LogMessage("SlewToAzimuth", "Not implemented");
            throw new MethodNotImplementedException("SlewToAzimuth");
        }

        /// <summary>
        /// <see langword="true" /> if any part of the dome is currently moving or a move command has been issued,
        /// but the dome has not yet started to move. <see langword="false" /> if all dome components are stationary
        /// and no move command has been issued.
        /// </summary>
        internal static bool Slewing
        {
            get
            {
                LogMessage("Slewing Get", false.ToString());
                return false;
            }
        }

        /// <summary>
        /// Synchronize the current position of the dome to the given azimuth.
        /// </summary>
        /// <param name="Azimuth">
        /// Target azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        internal static void SyncToAzimuth(double Azimuth)
        {
            LogMessage("SyncToAzimuth", "Not implemented");
            throw new MethodNotImplementedException("SyncToAzimuth");
        }

        #endregion

        #region Private properties and methods

        /// <summary>
        /// Returns true if the driver is connected (see <see cref="Connected"/>).
        /// </summary>
        private static bool IsConnected
        {
            get
            {
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private static void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new NotConnectedException(message);
            }
        }

        /// <summary>
        /// Returns the controller, creating it on first use with the current configuration.
        /// </summary>
        private static DomeController GetController()
        {
            lock (controllerSync)
            {
                if (Controller == null)
                {
                    IPulseSender sender = Config.SimulationMode
                        ? (IPulseSender)new SimulatedPulseSender(ControllerLog)
                        : new HttpPulseSender(ControllerLog);

                    Controller = new DomeController(() => Config, sender, Protector, new ProfileStateStore(), ControllerLog, () => DateTime.UtcNow);
                }

                return Controller;
            }
        }

        /// <summary>
        /// Runs a controller command and converts its failures into ASCOM exceptions for the client.
        /// </summary>
        private static void Execute(string identifier, System.Action command)
        {
            try
            {
                command();
            }
            catch (DomeCommandException ex)
            {
                LogMessage(identifier, $"Command failed: {ex.Message}");
                throw new DriverException(ex.Message, ex);
            }
        }

        private static ShutterState ToShutterState(ShutterPhase phase)
        {
            switch (phase)
            {
                case ShutterPhase.Open:
                    return ShutterState.shutterOpen;
                case ShutterPhase.Opening:
                    return ShutterState.shutterOpening;
                case ShutterPhase.Closing:
                    return ShutterState.shutterClosing;
                case ShutterPhase.Error:
                    return ShutterState.shutterError;
                default:
                    return ShutterState.shutterClosed;
            }
        }

        /// <summary>
        /// Reads the configuration from the ASCOM Profile store. Migrates the legacy format when the "Config" key is absent.
        /// Invalid stored data never throws: the configuration falls back to an empty one and the problem is logged.
        /// </summary>
        internal static void ReadProfile()
        {
            bool migrated = false;
            try
            {
                using (Profile driverProfile = new Profile())
                {
                    driverProfile.DeviceType = "Dome";

                    string json = driverProfile.GetValue(DriverProgId, ConfigProfileName, string.Empty, AbsentMarker);
                    if (json != AbsentMarker)
                    {
                        Config = ParseConfig(json);
                    }
                    else
                    {
                        Config = MigrateLegacyProfile(driverProfile);
                        migrated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Config = new DomeConfig();
                LogMessage("ReadProfile", $"Cannot read the ASCOM Profile, using an empty configuration: {ex.Message}");
            }

            if (tl != null)
                tl.Enabled = Config.TraceEnabled;

            if (migrated)
            {
                try
                {
                    WriteProfile(); // writes the Config key first
                    ClearLegacyPassword(); // the plain text password must not stay in the profile
                    LogMessage("ReadProfile", "Migrated the legacy profile to the new configuration format.");
                }
                catch (Exception ex)
                {
                    LogMessage("ReadProfile", $"Legacy profile migration could not be saved: {ex.Message}");
                }
            }
        }

        private static DomeConfig ParseConfig(string json)
        {
            try
            {
                return ConfigSerializer.FromJson(json) ?? new DomeConfig();
            }
            catch (Exception ex)
            {
                LogMessage("ReadProfile", $"Stored configuration is not valid, starting with an empty configuration: {ex.Message}");
                return new DomeConfig();
            }
        }

        private static DomeConfig MigrateLegacyProfile(Profile driverProfile)
        {
            // The legacy "COM Port" key held the IP address; its default "COM1" means "not set".
            string host = (driverProfile.GetValue(DriverProgId, LegacyComPortName, string.Empty, string.Empty) ?? "").Trim();
            if (string.Equals(host, LegacyComPortDefault, StringComparison.OrdinalIgnoreCase))
                host = "";

            string username = (driverProfile.GetValue(DriverProgId, LegacyUsernameName, string.Empty, string.Empty) ?? "").Trim();
            string password = driverProfile.GetValue(DriverProgId, LegacyPasswordName, string.Empty, string.Empty) ?? "";

            DomeConfig config = new DomeConfig();
            config.Host = host;
            config.Username = username;
            config.TraceEnabled = ReadTraceState(driverProfile);

            // The actions are left empty on purpose: the user enters them in the setup dialog.
            if (password.Length > 0)
            {
                try
                {
                    config.SetPassword(password, Protector);
                }
                catch (Exception ex)
                {
                    LogMessage("ReadProfile", $"The legacy password could not be protected and was not migrated: {ex.Message}");
                }
            }

            return config;
        }

        private static bool ReadTraceState(Profile driverProfile)
        {
            string value = driverProfile.GetValue(DriverProgId, traceStateProfileName, string.Empty, traceStateDefault);
            bool result;
            return bool.TryParse(value, out result) ? result : true;
        }

        private static void ClearLegacyPassword()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                driverProfile.WriteValue(DriverProgId, LegacyPasswordName, string.Empty);
            }
        }

        /// <summary>
        /// Write the configuration to the ASCOM Profile store and align the trace logger with it.
        /// </summary>
        internal static void WriteProfile()
        {
            string json = ConfigSerializer.ToJson(Config);

            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                driverProfile.WriteValue(DriverProgId, ConfigProfileName, json);
                driverProfile.WriteValue(DriverProgId, traceStateProfileName, Config.TraceEnabled.ToString());
            }

            if (tl != null)
                tl.Enabled = Config.TraceEnabled;
        }

        /// <summary>
        /// Persists the controller state string in the ASCOM Profile (key "State").
        /// </summary>
        private sealed class ProfileStateStore : IStateStore
        {
            public string Load()
            {
                try
                {
                    using (Profile driverProfile = new Profile())
                    {
                        driverProfile.DeviceType = "Dome";
                        return driverProfile.GetValue(DriverProgId, StateProfileName, string.Empty, string.Empty) ?? "";
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("ProfileStateStore", $"Cannot read the saved shutter state: {ex.Message}");
                    return "";
                }
            }

            public void Save(string value)
            {
                try
                {
                    using (Profile driverProfile = new Profile())
                    {
                        driverProfile.DeviceType = "Dome";
                        driverProfile.WriteValue(DriverProgId, StateProfileName, value ?? "");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("ProfileStateStore", $"Cannot save the shutter state: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Log helper function that takes identifier and message strings
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        internal static void LogMessage(string identifier, string message)
        {
            tl.LogMessageCrLf(identifier, message);
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            LogMessage(identifier, msg);
        }

        #endregion
    }
}
