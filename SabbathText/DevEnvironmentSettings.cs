namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Environment settings for the development environment.
    /// </summary>
    public class DevEnvironmentSettings : EnvironmentSettings
    {
        /// <summary>
        /// Creates a new instance of the development environment settings.
        /// </summary>
        public DevEnvironmentSettings()
            : base(null)
        {
        }
    }
}
