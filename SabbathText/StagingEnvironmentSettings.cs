namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Environment settings for the staging environment.
    /// </summary>
    public class StagingEnvironmentSettings : EnvironmentSettings
    {
        /// <summary>
        /// Creates a new instance of the staging environment settings.
        /// </summary>
        public StagingEnvironmentSettings()
            : base(null)
        {
        }
    }
}
