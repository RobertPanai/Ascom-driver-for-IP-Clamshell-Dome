namespace ASCOM.Clamshell.Core
{
    /// <summary>Persists the controller state as one opaque string.</summary>
    public interface IStateStore
    {
        /// <summary>Returns the last saved value, or null/empty when nothing was saved.</summary>
        string Load();

        /// <summary>Saves the opaque state value.</summary>
        /// <param name="value">Value produced by the controller.</param>
        void Save(string value);
    }
}
