namespace ASCOM.Clamshell.Core
{
    /// <summary>
    /// Estimated phase of the shutter. The relay board gives no feedback,
    /// so the phase is estimated from the travel times in the configuration.
    /// </summary>
    public enum ShutterPhase
    {
        /// <summary>The shutter is closed.</summary>
        Closed,
        /// <summary>Opening travel is in progress (estimated).</summary>
        Opening,
        /// <summary>The shutter is open.</summary>
        Open,
        /// <summary>Closing travel is in progress (estimated).</summary>
        Closing,
        /// <summary>A command failed; the real position is unknown and must be checked.</summary>
        Error
    }
}
