using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabbathText;

namespace AccountMigrator
{
    public class MigrationEnvironmentSettings : DevEnvironmentSettings
    {
        public override string KeyValueStoreConnectionString
        {
            get
            {
                return "";
            }
        }
    }
}
