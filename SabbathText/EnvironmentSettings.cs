namespace SabbathText
{
    using System;

    /// <summary>
    /// Environment settings
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>
        /// Gets the key value store connection string
        /// </summary>
        public virtual string KeyValueStoreConnectionString
        {
            get
            {
                return Environment.GetEnvironmentVariable("SabbathTextKeyValueStoreConnectionString");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current environment is using in-memory key value store 
        /// </summary>
        public virtual bool UseInMemoryKeyValueStore
        {
            get
            {
                return false;
            }
        }
    }
}
