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
        private static Dictionary<string, string> secrets = new Dictionary<string, string>
        {
            {
                KeyValueStoreConnectionStringKey,
                "a/siO7KIYLCZjMiA+tp05aYZdCKJArmYJFPthL9QqCKEyOYhBMo80vvzOFRxtC2CNJ2d8LACJFPN5X6+i9PXUavh/OFqDfpyYid/sNc0hgNtW7Jt1Z4wk31BwZbZUqWWr5cX0aPh4O3HpaCDa8jOcp1LGXff6VaA37zDhmYrMRU8WxmIYPfYLpuVbtDp9h1Osmmo2YYSFSQ7oRHTmtio8S9DN6+GUCZvKaLS7QyR3b8gxEBeDoh88TvgcRkpLw0YhCwsie+zRkMrcTE3fOquxe/gr7Dnz/PRASEhsYNxiO43SrKC0ydGGXbHcwKgop27LYTysnrDglMHsWYwqVeMaA=="
            },
            {
                ServicePhoneNumberKey,
                "grb9nA0yrrIqvDdRbhSEYHtUi7TdbGgkGfNquz7g8pmNLXeur14P6/EdL06+W2HQDoEQQgnpjU/gs/wXQNFD+bOaRKbIQrAeDNvq2TTl9pu0VT5gWrMlZqhjntvl3YuEZkITV7iH9LWIuq9/NBqL5S3CndmJBC1ngQMHK+zuGuGX4voImwpGxnjMMTV6t5xKxOp75ZineLB4DGxSAFu1+GXdNyW0i7zScN98OJn4bXgW3MSdQOZNX+JRKsvkHiSfRuF0PmyYSNtigQJoBR7yDSkH3EoZdXwzKqmc/HTGb16c1idxWK7W1KOQjezgCs1cKG2qxB3XVBwHuQTnQWdLVw=="
            }
        };

        /// <summary>
        /// Creates a new instance of the development environment settings.
        /// </summary>
        public DevEnvironmentSettings()
            : base(DecryptSecrets())
        {
        }

        private static Dictionary<string, string> DecryptSecrets()
        {
            string certPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "devcert.pfx");

            SecretProvider provider = new SecretProvider(certPath, password: "dev");
            return secrets.ToDictionary(
                kv => kv.Key,
                kv => provider.Decrypt(kv.Value));
        }
    }
}
