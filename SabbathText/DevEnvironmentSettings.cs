namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Environment settings for the development environment.
    /// </summary>
    public class DevEnvironmentSettings : EnvironmentSettings
    {
        private static readonly Dictionary<string, string> Secrets = new Dictionary<string, string>
        {
            {
                KeyValueStoreConnectionStringKey,
                "a/siO7KIYLCZjMiA+tp05aYZdCKJArmYJFPthL9QqCKEyOYhBMo80vvzOFRxtC2CNJ2d8LACJFPN5X6+i9PXUavh/OFqDfpyYid/sNc0hgNtW7Jt1Z4wk31BwZbZUqWWr5cX0aPh4O3HpaCDa8jOcp1LGXff6VaA37zDhmYrMRU8WxmIYPfYLpuVbtDp9h1Osmmo2YYSFSQ7oRHTmtio8S9DN6+GUCZvKaLS7QyR3b8gxEBeDoh88TvgcRkpLw0YhCwsie+zRkMrcTE3fOquxe/gr7Dnz/PRASEhsYNxiO43SrKC0ydGGXbHcwKgop27LYTysnrDglMHsWYwqVeMaA=="
            },
            {
                ServicePhoneNumberKey,
                "grb9nA0yrrIqvDdRbhSEYHtUi7TdbGgkGfNquz7g8pmNLXeur14P6/EdL06+W2HQDoEQQgnpjU/gs/wXQNFD+bOaRKbIQrAeDNvq2TTl9pu0VT5gWrMlZqhjntvl3YuEZkITV7iH9LWIuq9/NBqL5S3CndmJBC1ngQMHK+zuGuGX4voImwpGxnjMMTV6t5xKxOp75ZineLB4DGxSAFu1+GXdNyW0i7zScN98OJn4bXgW3MSdQOZNX+JRKsvkHiSfRuF0PmyYSNtigQJoBR7yDSkH3EoZdXwzKqmc/HTGb16c1idxWK7W1KOQjezgCs1cKG2qxB3XVBwHuQTnQWdLVw=="
            },
            {
                IncomingMessagePrimaryTokenKey,
                "Tq2ULXZ9P/6lyvzEDmab681Zw4oLLMqFZ50bOhDIa5ujVg+/Tn+Vfa+DhWuZxdGJN3USIUyKDkLl1r9yvnQY+SPS/O9+y78M2EXKaHF8dDzTh2CiYknuQuzHRjwcOA07zjo5oDELvravR+WGNWpL7nC4r25R7AVDtHNB170iWEdMmRihkCuQwv4CvTJnlpKwoY68KD0D0MP9ooCa1LsAGyOPqjb8V4O5iRaiNbtBU64TZ2ee90M/aE/tLZEzSjRDKKdNj1jc5xQw3u3uCX3RM0LTnZlQDBd0IKMsCm4P8wIsoJ34rutYQEMK1eGtaqqEiTIuO/Rd7y20LPHabcPn6Q=="
            },
            {
                IncomingMessageSecondaryTokenKey,
                "EfzivVXsHNGobxSCfWtuS1YG3DSVVhF0fOddPBt2EGWU1PQPp268uy3Onv+74Jo3QG079UObtboTUiK1XGujR8yU8RMe74pNlwSrg095nHQPRvBBPQiOs82l5jxTsVbPuU2z1myAecSYjIlMMOzf+mrzn77GfA5+NMlmpZJVkOmv34wGhQjUfxySTZBzlOfIHo3Jhii8UswaJnLL4gCjmPTxl/D7nCMPyZQI7PXF+ntlJvedDU8jEPJgrpXeKNycFgi3t4tGOn9VAH79B1IYEbTMn7UnIN1NRP/QQX3KXY87oyQ53PoYTikr5qb4kC8u1WEtrNxsH8hkwiTV5Pe0XA=="
            },
        };

        /// <summary>
        /// Creates a new instance of the development environment settings.
        /// </summary>
        public DevEnvironmentSettings()
            : base(DecryptSecrets())
        {
        }

        /// <summary>
        /// Gets the message client type
        /// </summary>
        public override SabbathText.V1.MessageClientType MessageClientType
        {
            get
            {
                return SabbathText.V1.MessageClientType.InMemory;
            }
        }

        /// <summary>
        /// Gets a value indicating whether workers should use console trace
        /// </summary>
        public override bool WorkersUseConsoleTrace
        {
            get
            {
                return true;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether to use Google authentication
        /// </summary>
        public override bool UseGoogleAuthentication
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to use Development authentication
        /// </summary>
        public override bool UseDevelopmentAuthentication
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the frequency for the runner
        /// </summary>
        public override TimeSpan RunnerFrequency
        {
            get
            {
                return TimeSpan.FromMinutes(1);
            }
        }

        /// <summary>
        /// Gets a list of admin accounts
        /// </summary>
        public override IEnumerable<string> AdminAccounts
        {
            get
            {
                return new string[]
                {
                    "dev@dev.local",
                };
            }
        }

        private static new Dictionary<string, string> DecryptSecrets()
        {
            string certPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetDirectory(),
                "devcert.pfx");

            SecretProvider provider = new SecretProvider(certPath, password: "dev");
            return Secrets.ToDictionary(
                kv => kv.Key,
                kv => provider.Decrypt(kv.Value));
        }
    }
}
