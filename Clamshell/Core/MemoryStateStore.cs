namespace ASCOM.Clamshell.Core
{
    /// <summary>In memory state store, used for tests and when no persistent store is available.</summary>
    public class MemoryStateStore : IStateStore
    {
        private readonly object _sync = new object();
        private string _value = "";

        /// <summary>Returns the last saved value, or an empty string.</summary>
        public string Load()
        {
            lock (_sync)
            {
                return _value;
            }
        }

        /// <summary>Stores the value in memory.</summary>
        public void Save(string value)
        {
            lock (_sync)
            {
                _value = value ?? "";
            }
        }
    }
}
