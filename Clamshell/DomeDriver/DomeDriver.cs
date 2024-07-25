// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Dome driver for Clamshell
//
// Description:	 <To be completed by driver developer>
//
// Implements:	ASCOM Dome interface version: <To be completed by driver developer>
// Author:		Robert Panai - robertpanai96@gmail.com
//

using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.LocalServer;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.IO;

namespace ASCOM.Clamshell.Dome
{
    //
    // This code is mostly a presentation layer for the functionality in the DomeHardware class. You should not need to change the contents of this file very much, if at all.
    // Most customisation will be in the DomeHardware class, which is shared by all instances of the driver, and which must handle all aspects of communicating with your device.
    //
    // Your driver's DeviceID is ASCOM.Clamshell.Dome
    //
    // The COM Guid attribute sets the CLSID for ASCOM.Clamshell.Dome
    // The COM ClassInterface/None attribute prevents an empty interface called _Clamshell from being created and used as the [default] interface
    //

    /// <summary>
    /// ASCOM Dome Driver for Clamshell.
    /// </summary>
    [ComVisible(true)]
    [Guid("cf76f964-4c58-4c5a-a3ed-cb680cd24e13")]
    [ProgId("ASCOM.Clamshell.Dome")]
    [ServedClassName("ASCOM Dome Driver for Clamshell")] // Driver description that appears in the Chooser, customise as required
    [ClassInterface(ClassInterfaceType.None)]
    public class Dome : ReferenceCountedObjectBase, IDomeV2, IDisposable
    {
        internal static string DriverProgId; // ASCOM DeviceID (COM ProgID) for this driver, the value is retrieved from the ServedClassName attribute in the class initialiser.
        internal static string DriverDescription; // The value is retrieved from the ServedClassName attribute in the class initialiser.

        // connectedState holds the connection state from this driver instance's perspective, as opposed to the local server's perspective, which may be different because of other client connections.
        internal bool connectedState; // The connected state from this driver's perspective)
        internal TraceLogger tl; // Trace logger object to hold diagnostic information just for this instance of the driver, as opposed to the local server's log, which includes activity from all driver instances.
        private bool disposedValue;

        #region Initialisation and Dispose

        /// <summary>
        /// Initializes a new instance of the <see cref="Clamshell"/> class. Must be public to successfully register for COM.
        /// </summary>
        public Dome()
        {
            try
            {
                // Pull the ProgID from the ProgID class attribute.
                Attribute attr = Attribute.GetCustomAttribute(this.GetType(), typeof(ProgIdAttribute));
                DriverProgId = ((ProgIdAttribute)attr).Value ?? "PROGID NOT SET!";  // Get the driver ProgIDfrom the ProgID attribute.

                // Pull the display name from the ServedClassName class attribute.
                attr = Attribute.GetCustomAttribute(this.GetType(), typeof(ServedClassNameAttribute));
                DriverDescription = ((ServedClassNameAttribute)attr).DisplayName ?? "DISPLAY NAME NOT SET!";  // Get the driver description that displays in the ASCOM Chooser from the ServedClassName attribute.

                // LOGGING CONFIGURATION
                // By default all driver logging will appear in Hardware log file
                // If you would like each instance of the driver to have its own log file as well, uncomment the lines below

                tl = new TraceLogger("", "Clamshell.Driver"); // Remove the leading ASCOM. from the ProgId because this will be added back by TraceLogger.
                SetTraceState();

                // Initialise the hardware if required
                DomeHardware.InitialiseHardware();

                LogMessage("Dome", "Starting driver initialisation");
                LogMessage("Dome", $"ProgID: {DriverProgId}, Description: {DriverDescription}");

                connectedState = false; // Initialise connected to false


                LogMessage("Dome", "Completed initialisation");
            }
            catch (Exception ex)
            {
                LogMessage("Dome", $"Initialisation exception: {ex}");
                MessageBox.Show($"{ex.Message}", "Exception creating ASCOM.Clamshell.Dome", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Class destructor called automatically by the .NET runtime when the object is finalised in order to release resources that are NOT managed by the .NET runtime.
        /// </summary>
        /// <remarks>See the Dispose(bool disposing) remarks for further information.</remarks>
        ~Dome()
        {
            // Please do not change this code.
            // The Dispose(false) method is called here just to release unmanaged resources. Managed resources will be dealt with automatically by the .NET runtime.

            Dispose(false);
        }

        /// <summary>
        /// Deterministically dispose of any managed and unmanaged resources used in this instance of the driver.
        /// </summary>
        /// <remarks>
        /// Do not dispose of items in this method, put clean-up code in the 'Dispose(bool disposing)' method instead.
        /// </remarks>
        public void Dispose()
        {
            // Please do not change the code in this method.

            // Release resources now.
            Dispose(disposing: true);

            // Do not add GC.SuppressFinalize(this); here because it breaks the ReferenceCountedObjectBase COM connection counting mechanic
        }

        /// <summary>
        /// Dispose of large or scarce resources created or used within this driver file
        /// </summary>
        /// <remarks>
        /// The purpose of this method is to enable you to release finite system resources back to the operating system as soon as possible, so that other applications work as effectively as possible.
        ///
        /// NOTES
        /// 1) Do not call the DomeHardware.Dispose() method from this method. Any resources used in the static DomeHardware class itself, 
        ///    which is shared between all instances of the driver, should be released in the DomeHardware.Dispose() method as usual. 
        ///    The DomeHardware.Dispose() method will be called automatically by the local server just before it shuts down.
        /// 2) You do not need to release every .NET resource you use in your driver because the .NET runtime is very effective at reclaiming these resources. 
        /// 3) Strong candidates for release here are:
        ///     a) Objects that have a large memory footprint (> 1Mb) such as images
        ///     b) Objects that consume finite OS resources such as file handles, synchronisation object handles, memory allocations requested directly from the operating system (NativeMemory methods) etc.
        /// 4) Please ensure that you do not return exceptions from this method
        /// 5) Be aware that Dispose() can be called more than once:
        ///     a) By the client application
        ///     b) Automatically, by the .NET runtime during finalisation
        /// 6) Because of 5) above, you should make sure that your code is tolerant of multiple calls.    
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        // Dispose of managed objects here

                        // Clean up the trace logger object
                        if (!(tl is null))
                        {
                            tl.Enabled = false;
                            tl.Dispose();
                            tl = null;
                        }
                    }
                    catch (Exception)
                    {
                        // Any exception is not re-thrown because Microsoft's best practice says not to return exceptions from the Dispose method. 
                    }
                }

                try
                {
                    // Dispose of unmanaged objects, if any, here (OS handles etc.)
                }
                catch (Exception)
                {
                    // Any exception is not re-thrown because Microsoft's best practice says not to return exceptions from the Dispose method. 
                }

                // Flag that Dispose() has already run and disposed of all resources
                disposedValue = true;
            }
        }

        #endregion

        // PUBLIC COM INTERFACE IDomeV2 IMPLEMENTATION

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialogue form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            try
            {
                if (connectedState) // Don't show if already connected
                {
                    MessageBox.Show("Already connected, just press OK");
                }
                else // Show dialogue
                {
                    LogMessage("SetupDialog", $"Calling SetupDialog.");
                    DomeHardware.SetupDialog();
                    LogMessage("SetupDialog", $"Completed.");
                }
            }
            catch (Exception ex)
            {
                LogMessage("SetupDialog", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public ArrayList SupportedActions
        {
            get
            {
                try
                {
                    CheckConnected($"SupportedActions");
                    ArrayList actions = DomeHardware.SupportedActions;
                    LogMessage("SupportedActions", $"Returning {actions.Count} actions.");
                    return actions;
                }
                catch (Exception ex)
                {
                    LogMessage("SupportedActions", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="ActionName">A well known name agreed by interested parties that represents the action to be carried out.</param>
        /// <param name="ActionParameters">List of required parameters or an <see cref="String.Empty">Empty String</see> if none are required.</param>
        /// <returns>A string response. The meaning of returned strings is set by the driver author.
        /// <para>Suppose filter wheels start to appear with automatic wheel changers; new actions could be <c>QueryWheels</c> and <c>SelectWheel</c>. The former returning a formatted list
        /// of wheel names and the second taking a wheel name and making the change, returning appropriate values to indicate success or failure.</para>
        /// </returns>
        public string Action(string actionName, string actionParameters)
        {
            try
            {
                CheckConnected($"Action {actionName} - {actionParameters}");
                LogMessage("", $"Calling Action: {actionName} with parameters: {actionParameters}");
                string actionResponse = DomeHardware.Action(actionName, actionParameters);
                LogMessage("Action", $"Completed.");
                return actionResponse;
            }
            catch (Exception ex)
            {
                LogMessage("Action", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        public void CommandBlind(string command, bool raw)
        {
            try
            {
                CheckConnected($"CommandBlind: {command}, Raw: {raw}");
                LogMessage("CommandBlind", $"Calling method - Command: {command}, Raw: {raw}");
                DomeHardware.CommandBlind(command, raw);
                LogMessage("CommandBlind", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("CommandBlind", $"Command: {command}, Raw: {raw} threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the interpreted boolean response received from the device.
        /// </returns>
        public bool CommandBool(string command, bool raw)
        {
            try
            {
                CheckConnected($"CommandBool: {command}, Raw: {raw}");
                LogMessage("CommandBlind", $"Calling method - Command: {command}, Raw: {raw}");
                bool commandBoolResponse = DomeHardware.CommandBool(command, raw);
                LogMessage("CommandBlind", $"Returning: {commandBoolResponse}.");
                return commandBoolResponse;
            }
            catch (Exception ex)
            {
                LogMessage("CommandBool", $"Command: {command}, Raw: {raw} threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the string response received from the device.
        /// </returns>
        public string CommandString(string command, bool raw)
        {
            try
            {
                CheckConnected($"CommandString: {command}, Raw: {raw}");
                LogMessage("CommandString", $"Calling method - Command: {command}, Raw: {raw}");
                string commandStringResponse = DomeHardware.CommandString(command, raw);
                LogMessage("CommandString", $"Returning: {commandStringResponse}.");
                return commandStringResponse;
            }
            catch (Exception ex)
            {
                LogMessage("CommandString", $"Command: {command}, Raw: {raw} threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// You can also read the property to check whether it is connected. This reports the current hardware state.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public bool Connected
        {
            get
            {
                try
                {
                    // Returns the driver's connection state rather than the local server's connected state, which could be different because there may be other client connections still active.
                    LogMessage("Connected Get", connectedState.ToString());
                    return connectedState;
                }
                catch (Exception ex)
                {
                    LogMessage("Connected Get", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
            set
            {
                try
                {
                    if (value == connectedState)
                    {
                        LogMessage("Connected Set", "Device already connected, ignoring Connected Set = true");
                        return;
                    }

                    if (value)
                    {
                        connectedState = true;
                        LogMessage("Connected Set", "Connecting to device");
                        DomeHardware.Connected = true;
                    }
                    else
                    {
                        connectedState = false;
                        LogMessage("Connected Set", "Disconnecting from device");
                        DomeHardware.Connected = false;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage("Connected Set", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and model number. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                try
                {
                    CheckConnected($"Description");
                    string description = DomeHardware.Description;
                    LogMessage("Description", description);
                    return description;
                }
                catch (Exception ex)
                {
                    LogMessage("Description", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public string DriverInfo
        {
            get
            {
                try
                {
                    // This should work regardless of whether or not the driver is Connected, hence no CheckConnected method.
                    string driverInfo = DomeHardware.DriverInfo;
                    LogMessage("DriverInfo", driverInfo);
                    return driverInfo;
                }
                catch (Exception ex)
                {
                    LogMessage("DriverInfo", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public string DriverVersion
        {
            get
            {
                try
                {
                    // This should work regardless of whether or not the driver is Connected, hence no CheckConnected method.
                    string driverVersion = DomeHardware.DriverVersion;
                    LogMessage("DriverVersion", driverVersion);
                    return driverVersion;
                }
                catch (Exception ex)
                {
                    LogMessage("DriverVersion", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// The interface version number that this device supports.
        /// </summary>
        public short InterfaceVersion
        {
            get
            {
                try
                {
                    // This should work regardless of whether or not the driver is Connected, hence no CheckConnected method.
                    short interfaceVersion = DomeHardware.InterfaceVersion;
                    LogMessage("InterfaceVersion", interfaceVersion.ToString());
                    return interfaceVersion;
                }
                catch (Exception ex)
                {
                    LogMessage("InterfaceVersion", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes
        /// </summary>
        public string Name
        {
            get
            {
                try
                {
                    // This should work regardless of whether or not the driver is Connected, hence no CheckConnected method.
                    string name = DomeHardware.Name;
                    LogMessage("Name Get", name);
                    return name;
                }
                catch (Exception ex)
                {
                    LogMessage("Name", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        #endregion

        #region IDome Implementation

        /// <summary>
        /// Immediately stops any and all movement of the dome.
        /// </summary>
        /// 
        private bool domeState = false; //set false by default
        public void AbortSlew()
        {
            try
            {
                CheckConnected("AbortSlew");
                LogMessage("AbortSlew", $"Calling method.");
                DomeHardware.AbortSlew();
                LogMessage("AbortSlew", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("AbortSlew", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// The altitude (degrees, horizon zero and increasing positive to 90 zenith) of the part of the sky that the observer wishes to observe.
        /// </summary>
        public double Altitude
        {
            get
            {
                try
                {
                    CheckConnected("Altitude");
                    double altitude = DomeHardware.Altitude;
                    LogMessage("Altitude", altitude.ToString());
                    return altitude;
                }
                catch (Exception ex)
                {
                    LogMessage("Altitude", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <para><see langword="true" /> when the dome is in the home position. Raises an error if not supported.</para>
        /// <para>
        /// This is normally used following a <see cref="FindHome" /> operation. The value is reset
        /// with any azimuth slew operation that moves the dome away from the home position.
        /// </para>
        /// <para>
        /// <see cref="AtHome" /> may optionally also become true during normal slew operations, if the
        /// dome passes through the home position and the dome controller hardware is capable of
        /// detecting that; or at the end of a slew operation if the dome comes to rest at the home
        /// position.
        /// </para>
        /// </summary>
        public bool AtHome
        {
            get
            {
                try
                {
                    CheckConnected("AtHome");
                    bool atHome = DomeHardware.AtHome;
                    LogMessage("AtHome", atHome.ToString());
                    return atHome;
                }
                catch (Exception ex)
                {
                    LogMessage("AtHome", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome is in the programmed park position.
        /// </summary>
        public bool AtPark
        {
            get
            {
                try
                {
                    CheckConnected("AtPark");
                    bool atPark = DomeHardware.AtPark;
                    LogMessage("AtPark", atPark.ToString());
                    return atPark;
                }
                catch (Exception ex)
                {
                    LogMessage("AtPark", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// The dome azimuth (degrees, North zero and increasing clockwise, i.e., 90 East, 180 South, 270 West). North is true north and not magnetic north.
        /// </summary>
        public double Azimuth
        {
            get
            {
                try
                {
                    CheckConnected("Azimuth");
                    double azimuth = DomeHardware.Azimuth;
                    LogMessage("Azimuth", azimuth.ToString());
                    return azimuth;
                }
                catch (Exception ex)
                {
                    LogMessage("Azimuth", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver can perform a search for home position.
        /// </summary>
        public bool CanFindHome
        {
            get
            {
                try
                {
                    CheckConnected("CanFindHome");
                    bool canFindHome = DomeHardware.CanFindHome;
                    LogMessage("CanFindHome", canFindHome.ToString());
                    return canFindHome;
                }
                catch (Exception ex)
                {
                    LogMessage("CanFindHome", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of parking the dome.
        /// </summary>
        public bool CanPark
        {
            get
            {
                try
                {
                    CheckConnected("CanPark");
                    bool canPark = DomeHardware.CanPark;
                    LogMessage("CanPark", canPark.ToString());
                    return canPark;
                }
                catch (Exception ex)
                {
                    LogMessage("CanPark", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of setting dome altitude.
        /// </summary>
        public bool CanSetAltitude
        {
            get
            {
                try
                {
                    CheckConnected("CanSetAltitude");
                    bool canSetAltitude = DomeHardware.CanSetAltitude;
                    LogMessage("CanSetAltitude", canSetAltitude.ToString());
                    return canSetAltitude;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSetAltitude", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if driver is capable of rotating the dome. Must be <see "langword="false" /> for a 
        /// roll-off roof or clamshell.
        /// </summary>
        public bool CanSetAzimuth
        {
            get
            {
                try
                {
                    CheckConnected("CanSetAzimuth");
                    bool canSetAzimuth = DomeHardware.CanSetAzimuth;
                    LogMessage("CanSetAzimuth", canSetAzimuth.ToString());
                    return canSetAzimuth;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSetAzimuth", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver can set the dome park position.
        /// </summary>
        public bool CanSetPark
        {
            get
            {
                try
                {
                    CheckConnected("CanSetPark");
                    bool canSetPark = DomeHardware.CanSetPark;
                    LogMessage("CanSetPark", canSetPark.ToString());
                    return canSetPark;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSetPark", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of opening and closing the shutter or roof
        /// mechanism.
        /// </summary>
        public bool CanSetShutter
        {
            get
            {
                try
                {
                    CheckConnected("CanSetShutter");
                    bool canSetShutter = DomeHardware.CanSetShutter;
                    LogMessage("CanSetShutter", canSetShutter.ToString());
                    return canSetShutter;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSetShutter", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the dome hardware supports slaving to a telescope.
        /// </summary>
        public bool CanSlave
        {
            get
            {
                try
                {
                    CheckConnected("CanSlave");
                    bool canSlave = DomeHardware.CanSlave;
                    LogMessage("CanSlave", canSlave.ToString());
                    return canSlave;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSlave", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true" /> if the driver is capable of synchronizing the dome azimuth position
        /// using the <see cref="SyncToAzimuth" /> method.
        /// </summary>
        public bool CanSyncAzimuth
        {
            get
            {
                try
                {
                    CheckConnected("CanSyncAzimuth");
                    bool canSyncAzimuth = DomeHardware.CanSyncAzimuth;
                    LogMessage("CanSyncAzimuth", canSyncAzimuth.ToString());
                    return canSyncAzimuth;
                }
                catch (Exception ex)
                {
                    LogMessage("CanSyncAzimuth", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Close the shutter or otherwise shield the telescope from the sky.
        /// </summary>
        public void CloseShutter()
        {
            // Definizione delle credenziali di autenticazione
            //string username = "ftp"; // Sostituisci con il nome utente reale
            //string password = "ftp"; // Sostituisci con la password reale
            string username = DomeHardware.Username;
            string password = DomeHardware.Password;
            string nu_port = DomeHardware.Relè_NU;
            string su_port = DomeHardware.Relè_SU;
            

            // Metodo per inviare la richiesta HTTP
            void SendRequest(string led)
            {
                string remoteUrl = $"http://{DomeHardware.comPort}/protect/leds.cgi?led={led}";
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(remoteUrl);

                // Aggiungi credenziali di autenticazione
                httpRequest.Credentials = new NetworkCredential(username, password);

                try
                {
                    using (WebResponse response = httpRequest.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseText = reader.ReadToEnd();
                            LogMessage($"SendRequest for LED {led}", $"Response received: {responseText}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"SendRequest for LED {led}", $"Threw an exception: \r\n{ex}");
                }
            }

            // Crea e avvia i thread per le richieste parallele
            Thread threadLed4 = new Thread(() => SendRequest("4"));
            Thread threadLed2 = new Thread(() => SendRequest("2"));

            threadLed4.Start();
            threadLed2.Start();

            // Attendi che entrambi i thread abbiano completato
            threadLed4.Join();
            threadLed2.Join();

            // Operazioni successive
            domeState = false;
            try
            {
                CheckConnected("CloseShutter");
                LogMessage("CloseShutter", $"Calling method.");
                DomeHardware.CloseShutter();
                LogMessage("CloseShutter", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("CloseShutter", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Start operation to search for the dome home position.
        /// </summary>
        public void FindHome()
        {
            try
            {
                CheckConnected("FindHome");
                LogMessage("FindHome", $"Calling method.");
                DomeHardware.FindHome();
                LogMessage("FindHome", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("FindHome", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Open shutter or otherwise expose telescope to the sky.
        /// </summary>
        public void OpenShutter()
        {
            // Definizione delle credenziali di autenticazione
            //string username = "ftp"; // Sostituisci con il nome utente reale
            //string password = "ftp"; // Sostituisci con la password reale
            string username = DomeHardware.Username;
            string password = DomeHardware.Password;
            string nu_port = DomeHardware.Relè_NU;
            string su_port = DomeHardware.Relè_SU;


            // Metodo per inviare la richiesta HTTP
            void SendRequest(string led)
            {
                string remoteUrl = $"http://{DomeHardware.comPort}/protect/leds.cgi?led={led}";
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(remoteUrl);

                // Aggiungi credenziali di autenticazione
                httpRequest.Credentials = new NetworkCredential(username, password);

                try
                {
                    using (WebResponse response = httpRequest.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseText = reader.ReadToEnd();
                            LogMessage($"SendRequest for LED {led}", $"Response received: {responseText}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"SendRequest for LED {led}", $"Threw an exception: \r\n{ex}");
                }
            }

            // Crea e avvia i thread per le richieste parallele
            Thread threadLed5 = new Thread(() => SendRequest("5"));
            Thread threadLed3 = new Thread(() => SendRequest("3"));

            threadLed5.Start();
            threadLed3.Start();

            // Attendi che entrambi i thread abbiano completato
            threadLed5.Join();
            threadLed3.Join();

            // Operazioni successive
            domeState = true;
            try
            {
                CheckConnected("OpenShutter");
                LogMessage("OpenShutter", $"Calling method.");
                DomeHardware.OpenShutter();
                LogMessage("OpenShutter", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("OpenShutter", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Rotate dome in azimuth to park position.
        /// </summary>
        public void Park()
        {
            try
            {
                CheckConnected("Park");
                LogMessage("Park", $"Calling method.");
                DomeHardware.Park();
                LogMessage("Park", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("Park", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Set the current azimuth position of dome to the park position.
        /// </summary>
        public void SetPark()
        {
            try
            {
                CheckConnected("SetPark");
                LogMessage("SetPark", $"Calling method.");
                DomeHardware.SetPark();
                LogMessage("SetPark", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("SetPark", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Gets the status of the dome shutter or roof structure.
        /// </summary>
        public ShutterState ShutterStatus
        {
            get
            {
                try
                {
                    CheckConnected("ShutterStatus");
                    ShutterState shutterSttaus = DomeHardware.ShutterStatus;
                    LogMessage("ShutterStatus", shutterSttaus.ToString());
                    return shutterSttaus;
                }
                catch (Exception ex)
                {
                    LogMessage("ShutterStatus", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// <see langword="true"/> if the dome is slaved to the telescope in its hardware, else <see langword="false"/>.
        /// </summary>
        public bool Slaved
        {
            get
            {
                try
                {
                    CheckConnected("Slaved Get");
                    bool slaved = DomeHardware.Slaved;
                    LogMessage("Slaved Get", slaved.ToString());
                    return slaved;
                }
                catch (Exception ex)
                {
                    LogMessage("Slaved Get", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
            set
            {
                try
                {
                    CheckConnected("Slaved Set");
                    LogMessage("Slaved Set", value.ToString());
                    DomeHardware.Slaved = value;
                }
                catch (Exception ex)
                {
                    LogMessage("Slaved Set", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Ensure that the requested viewing altitude is available for observing.
        /// </summary>
        /// <param name="altitude">
        /// The desired viewing altitude (degrees, horizon zero and increasing positive to 90 degrees at the zenith)
        /// </param>
        public void SlewToAltitude(double altitude)
        {
            try
            {
                CheckConnected("SlewToAltitude");
                LogMessage("SlewToAltitude", $"Calling method.");
                DomeHardware.SlewToAltitude(altitude);
                LogMessage("SlewToAltitude", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("SlewToAltitude", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// Ensure that the requested viewing azimuth is available for observing.
        /// The method should not block and the slew operation should complete asynchronously.
        /// </summary>
        /// <param name="azimuth">
        /// Desired viewing azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        public void SlewToAzimuth(double azimuth)
        {
            try
            {
                CheckConnected("SlewToAzimuth");
                LogMessage("SlewToAzimuth", $"Calling method.");
                DomeHardware.SlewToAzimuth(azimuth);
                LogMessage("SlewToAzimuth", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("SlewToAzimuth", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        /// <summary>
        /// <see langword="true" /> if any part of the dome is currently moving or a move command has been issued, 
        /// but the dome has not yet started to move. <see langword="false" /> if all dome components are stationary
        /// and no move command has been issued. /> 
        /// </summary>
        public bool Slewing
        {
            get
            {
                try
                {
                    CheckConnected("Slewing");
                    bool slewing = DomeHardware.Slewing;
                    LogMessage("Slewing", slewing.ToString());
                    return slewing;
                }
                catch (Exception ex)
                {
                    LogMessage("Slewing", $"Threw an exception: \r\n{ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Synchronize the current position of the dome to the given azimuth.
        /// </summary>
        /// <param name="azimuth">
        /// Target azimuth (degrees, North zero and increasing clockwise. i.e., 90 East,
        /// 180 South, 270 West)
        /// </param>
        public void SyncToAzimuth(double azimuth)
        {
            try
            {
                CheckConnected("SyncToAzimuth");
                LogMessage("SyncToAzimuth", $"Calling method.");
                DomeHardware.SyncToAzimuth(azimuth);
                LogMessage("SyncToAzimuth", $"Completed.");
            }
            catch (Exception ex)
            {
                LogMessage("SyncToAzimuth", $"Threw an exception: \r\n{ex}");
                throw;
            }
        }

        #endregion

        #region Private properties and methods
        // Useful properties and methods that can be used as required to help with driver development

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!connectedState)
            {
                throw new NotConnectedException($"{DriverDescription} ({DriverProgId}) is not connected: {message}");
            }
        }

        /// <summary>
        /// Log helper function that writes to the driver or local server loggers as required
        /// </summary>
        /// <param name="identifier">Identifier such as method name</param>
        /// <param name="message">Message to be logged.</param>
        private void LogMessage(string identifier, string message)
        {
            // This code is currently set to write messages to an individual driver log AND to the shared hardware log.

            // Write to the individual log for this specific instance (if enabled by the driver having a TraceLogger instance)
            if (tl != null)
            {
                tl.LogMessageCrLf(identifier, message); // Write to the individual driver log
            }

            // Write to the common hardware log shared by all running instances of the driver.
            DomeHardware.LogMessage(identifier, message); // Write to the local server logger
        }

        /// <summary>
        /// Read the trace state from the driver's Profile and enable / disable the trace log accordingly.
        /// </summary>
        private void SetTraceState()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Dome";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(DriverProgId, DomeHardware.traceStateProfileName, string.Empty, DomeHardware.traceStateDefault));
            }
        }

        #endregion
    }
}
