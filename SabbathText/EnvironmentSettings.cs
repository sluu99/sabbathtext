namespace SabbathText
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.V1;

    /// <summary>
    /// Environment settings
    /// </summary>
    public class EnvironmentSettings
    {
        /// <summary>
        /// Key for KeyValueStoreConnectionString
        /// </summary>
        protected const string KeyValueStoreConnectionStringKey = "KeyValueStoreConnectionString";

        /// <summary>
        /// Key for ServicePhoneNumber
        /// </summary>
        protected const string ServicePhoneNumberKey = "ServicePhoneNumber";

        /// <summary>
        /// Key for <see cref="TwilioAccount"/>
        /// </summary>
        protected const string TwilioAccountKey = "TwilioAccount";

        /// <summary>
        /// Key for <see cref="TwilioToken"/>
        /// </summary>
        protected const string TwilioTokenKey = "TwilioToken";

        /// <summary>
        /// Key for <see cref="IncomingMessagePrimaryToken"/>
        /// </summary>
        protected const string IncomingMessagePrimaryTokenKey = "IncomingMessagePrimaryToken";

        /// <summary>
        /// Key for <see cref="IncomingMessageSecondaryToken"/>
        /// </summary>
        protected const string IncomingMessageSecondaryTokenKey = "IncomingMessageSecondaryToken";

        /// <summary>
        /// Key for <see cref="GoogleClientId"/>
        /// </summary>
        protected const string GoogleClientIdKey = "GoogleClientId";

        /// <summary>
        /// Key for <see cref="GoogleClientSecret"/>
        /// </summary>
        protected const string GoogleClientSecretKey = "GoogleClientSecret";

        private static readonly Dictionary<string, string> Secrets = new Dictionary<string, string>
        {
            {
                KeyValueStoreConnectionStringKey,
                "u9fLA4Bv1eeshQFgbwoFti8PmtU7+qIAPGJPrBAvjS3bfL7Dcb5QA+ibD0yHxMFU5WeJlWb2JPE2OLXBEbsKVCz+dNAsti5qDcQtEE4Fhca9BXCNJwrhLA7mQ9zNW5HM3z38QCHN2gt4rn/e0NunCgK258F6M/FLe86OkH64jmYOnM6KgSS/4Yiq24aux/uxVg9ktDdI9EVSYhevnygYlDJZb+vdE1Budl2HfC/D0dU+icEJXuzAznzyxQzM+K6aTm02LUcVf6BwpA4/YM7d/tIO2p2eVKoK75OTPiKYuMbCBeUeTtw1nd7pq774eaGCl5b683cWZBwzYUHIQxX5wg=="
            },
            {
                ServicePhoneNumberKey,
                "dtmHSvLVOZ0gdB3ALVqQ6He57uMbLffBinqitZQKD/Kuvf8IqiyGe11YzWP5rEs3cSSXL/daiBsGH1sM0kHeiCtlNwlYTkBO1fap/nA7gwqlXepvOjubIddDrMC8FdLun06HT6ssqEWuXSQFgOZG+Zc5/cvx/F5N7KB1/en2QHRYlWYsIYdcH+X36m9BsBpsXchuG3E0+3ZS9rGS0SuoUXyzzGeNAgYQahrrRKUNwM+Q5cIBjItxsShnfAT8KktgJqKWkK+13Q/J2w37bvZoJp1PUGWvMJlX+HaRWom95SrtGqQLhUytBpwkSY+5OxOX6NTxbi9OzU/Irr7SZqVDXw=="
            },
            {
                TwilioAccountKey,
                "w17R/L49KZBKPo1tDsmRDdAo7MKV3TpOVUkcSLw9DyahUEjyBUMAcZr8WtK+F/HfkC/2Yirp7k5GmAR7KIHdFogK3XVi3vmZ1v9H9oNMvD/aeDbz2DNkYzwKMfk1WbQmWMQp+t2HUBEGSffWoIngO+08bRkI1Ol8LBSpngLmMCdCTOHxzIv+YzfNO6oIt0IumovLBU/Le9BDVtOaBZFHI/8w6UZXrQL4fk23VwIjwccv74Sk1GNwF8XO/E0khJGTYPCT8QNu9ttTZLZbdFe4Q1aKaKf0KP3AGyAxnhjiMt8wcmnZqEk6kYnJk+mHi+Ho5mfgfKI4+9mMTsd6T1Gk3g=="
            },
            {
                TwilioTokenKey,
                "oiKWYEcpnv6+rIlkYK5ekyNyK4mSsx6Bd2Z3n08YlI5v4tygu6L82LHzPLag1mPinpyHAL1aiDFutMSK4WRynvJi+4SSEEwmRWE5JuM6/+IQdegX26WHwUtYX3sgclIWNyZm+POKDbN5HAg7eBSgUEDg00v1yJA0YThjZMFrX2wXji7akXEA2gb7toy/vlJoB7N0/GSJG0Z9IZU7R2kezlUv/p5y039Fg9T1kLHG0If4BG7Dtz0arKf898BE/8dS8ko44/AqtODweyFj+SoYm1cNoqBLIht8oGf2dQDpe256u+PhPfQ0YYxzT9L72viDpWi6zbBsxVQDd77s9F5L5Q=="
            },
            {
                IncomingMessagePrimaryTokenKey,
                "m4bsSyyOZa0vjaZuRyd8iB/ic/RAhD09dfszeDj1RVYuxNCRv8oIGy8FjFHOi1nuCrXJtNtAruj6/+4MG3nlIm1vjBOSlRC1YPIAeioGuMqq026oYZDlD5TNtAQJlw8VJYOwyqhPYoJSXWVQACHUf8l3c+umhXrXXZiqsSP9hYxlZo7FmS6+iJ+HhJCgpnDPuK0GWVbV4NhEcYPFg3AhiPl3x/BYx+TZpDRB3EZ7FdMaq4EDCDFiOsBOlNDS5g2afMvC73IjHKbIONWNqRm3/QKhj37pLSUwd7LBst2BBpXMea4qJ0+J9XsJ3IKP/t5Xs61CeHjALO+Vjto3DeUgwQ=="
            },
            {
                IncomingMessageSecondaryTokenKey,
                "ERPcmCDZfbZfOpNP4Ct3uxv5X+RPqSy0HJRhk77XC74zRknMnQ4R0b6BvMRHRJ0wVCDLIlKvhUOmOl6Cq8vaInTu2OfGBaUgfSiVJvkB+cxAX/n4PtglpGbhIpNTvUOrixxaEmSA914xvEVC2tSUg4L45gSO6FPuZ0QZ8GHnHahMeNCiZvDxCLTQeUJvSQ6QsflMUj2hejIn+x/t9pzTxoIT0/iVZUuR2suLWthrTaby9bGkSPZtBd8ZjmMs0TZKVPfMvZw71IIkM6wSGX/qjVJUTLcsueKuGZEegVZ4NDR0RBA49nNdVSLskVpgwelHtAcU0aLTXPOaxKRdSTtUGw=="
            },
            {
                GoogleClientIdKey,
                "Jd81Vq020nWE9b89Sv53jzUy2QOfeKoqcwbyGfo6JBETl8bgejH8d7fxRgayZrQ+FSvpyFeeyaLTVI0V5ChbLNoVPGrd3RukZM0588MNLPiYWzXqv97O2SKKiRFTETXJLJVS1ZsnmecF3lxJ/EJ4gDEEHcjTk1FnzcElfZIz4qp81tVdcceUMhK7bIY/55F8/VG9+AkYETlalHoJ+0cBiEDmgx3eyC13Wp8GNVOGNFmRVB5YnIL3BEOzlOw8QyeVtD694IkiiU2bj2gmxp+xJQ8ndhihoOZDl1p47Ho/24zC5q0HlpvRF83dzfhpKqEyqt133njOUzY7/M/I5tmjig=="
            },
            {
                GoogleClientSecretKey,
                "gQ92+tfjIz3iKa9RdomBrvsWyh63XYVLnNeNTk0jwtnscvEPGaKBoDZo3GZ5BwKzq+t/sMbdLXL3b+7GuYpEBDFQI9iMwlK0qnZ0RTEW/wK6NCbGpowgao1LC9flj08tk3vwRdsiKgZXYBUPqXIAaA1ZpweaLUCY2HwOfZZDqON+cYlH7A7L3PLk9TSSWRiom3jwi5EI5Q5+3Ewx9yhFfz6D7peSj/dtHzXLzh30Ld1IvBQNDeph6iQzMlRQ7psgUqvLyC70tfEleUC7tkXwXLIDc2xs3BVzwUoeN+j4GJiB6+l0H3ehQ35K39f29xZ7ZZKSU/pPca1Bck5+vE+f6g=="
            },
        };

        private Dictionary<string, string> secrets;

        /// <summary>
        /// Make the constructor private
        /// </summary>
        /// <param name="secrets">The environment secrets.</param>
        protected EnvironmentSettings(Dictionary<string, string> secrets)
        {
            this.secrets = secrets;
        }

        /// <summary>
        /// Gets the key value store connection string
        /// </summary>
        public virtual string KeyValueStoreConnectionString
        {
            get
            {
                return this.secrets[KeyValueStoreConnectionStringKey];
            }
        }

        /// <summary>
        /// Gets the phone number used by this service
        /// </summary>
        public virtual string ServicePhoneNumber
        {
            get
            {
                return this.secrets[ServicePhoneNumberKey];
            }
        }

        /// <summary>
        /// Gets the <c>Twilio</c> account
        /// </summary>
        public virtual string TwilioAccount
        {
            get
            {
                return this.secrets[TwilioAccountKey];
            }
        }

        /// <summary>
        /// Gets the <c>Twilio</c> token
        /// </summary>
        public virtual string TwilioToken
        {
            get
            {
                return this.secrets[TwilioTokenKey];
            }
        }

        /// <summary>
        /// Gets the primary token for incoming message
        /// </summary>
        public virtual string IncomingMessagePrimaryToken
        {
            get
            {
                return this.secrets[IncomingMessagePrimaryTokenKey];
            }
        }

        /// <summary>
        /// Gets the secondary token for incoming message
        /// </summary>
        public virtual string IncomingMessageSecondaryToken
        {
            get
            {
                return this.secrets[IncomingMessageSecondaryTokenKey];
            }
        }

        /// <summary>
        /// Gets the life span for an authentication key
        /// </summary>
        public virtual TimeSpan AuthKeyLifeSpan
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        /// <summary>
        /// Gets the message client type
        /// </summary>
        public virtual MessageClientType MessageClientType
        {
            get { return MessageClientType.Twilio; }
        }

        /// <summary>
        /// Gets a value indicating whether workers will add the console trace listener
        /// </summary>
        public virtual bool WorkersUseConsoleTrace
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the configuration for the account store.
        /// </summary>
        public virtual KeyValueStoreConfiguration AccountStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "accounts",
                };
            }
        }

        /// <summary>
        /// Gets the message store configuration.
        /// </summary>
        public virtual KeyValueStoreConfiguration MessageStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "messages",
                };
            }
        }

        /// <summary>
        /// Gets the tracker store configuration.
        /// </summary>
        public virtual KeyValueStoreConfiguration TrackerStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "trackers",
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint store
        /// </summary>
        public virtual KeyValueStoreConfiguration CheckpointStoreConfiguration
        {
            get
            {
                return new KeyValueStoreConfiguration
                {
                    Type = KeyValueStoreType.AzureTable,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureTableName = "checkpoints",
                };
            }
        }

        /// <summary>
        /// Gets the configuration for the checkpoint queue
        /// </summary>
        public virtual QueueStoreConfiguration CheckpointQueueConfiguration
        {
            get
            {
                return new QueueStoreConfiguration
                {
                    Type = QueueStoreType.AzureStorageQueue,
                    ConnectionString = this.KeyValueStoreConnectionString,
                    AzureQueueName = "checkpointqueue",
                };
            }
        }

        /// <summary>
        /// Gets the default operation timeout
        /// </summary>
        public virtual TimeSpan OperationTimeout
        {
            get { return TimeSpan.FromSeconds(30); }
        }

        /// <summary>
        /// Gets the timeout before the compensation agent picks up the checkpoint.
        /// This should be higher than the operation timeout
        /// </summary>
        public virtual TimeSpan CheckpointVisibilityTimeout
        {
            get { return TimeSpan.FromSeconds(60); }
        }

        /// <summary>
        /// Gets the amount of time gap between two Sabbath text messages.
        /// We cannot send more than one Sabbath message within this time span.
        /// </summary>
        public TimeSpan SabbathTextGap
        {
            get { return TimeSpan.FromDays(5); }
        }
        
        /// <summary>
        /// Gets the amount of time to way before trying again,
        /// if the checkpoint worker did not find any message.
        /// </summary>
        public TimeSpan CheckpointWorkerIdleDelay
        {
            get { return TimeSpan.FromSeconds(3); }
        }

        /// <summary>
        /// Gets the number of messages to keep with the account entity before archiving.
        /// </summary>
        public int RecentMessageThreshold
        {
            get { return 200; }
        }

        /// <summary>
        /// Gets the delay between account partition for the runner
        /// </summary>
        public virtual TimeSpan RunnerPartitionDelay
        {
            get { return TimeSpan.FromSeconds(1); }
        }

        /// <summary>
        /// Gets a value indicating whether the web app uses Development authentication.
        /// This should only be used for the development environment.
        /// </summary>
        public virtual bool UseDevelopmentAuthentication
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the web app uses Google authentication
        /// </summary>
        public virtual bool UseGoogleAuthentication
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the Google client ID.
        /// </summary>
        public string GoogleClientId
        {
            get { return this.secrets[GoogleClientIdKey]; }
        }

        /// <summary>
        /// Gets the Google client secret.
        /// </summary>
        public string GoogleClientSecret
        {
            get { return this.secrets[GoogleClientSecretKey]; }
        }

        /// <summary>
        /// Gets a list of the admin accounts
        /// </summary>
        public virtual IEnumerable<string> AdminAccounts
        {
            get
            {
                return new string[]
                {
                    "sluu99@gmail.com",
                };
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="EnvironmentSettings"/> based on the environment name.
        /// </summary>
        /// <param name="environmentName">
        /// The environment name. If null is provided, the <c>SabbathTextEnvironment</c>
        /// environment variable will be used.
        /// </param>
        /// <returns>The environment settings.</returns>
        public static EnvironmentSettings Create(string environmentName = null)
        {
            if (environmentName == null)
            {
                environmentName = Environment.GetEnvironmentVariable("SabbathTextEnvironment");
            }

            switch (environmentName)
            {
                case "Production":
                    return new EnvironmentSettings(DecryptSecrets());
                case "Staging":
                    return new StagingEnvironmentSettings();
            }

            return new DevEnvironmentSettings();
        }

        private static Dictionary<string, string> DecryptSecrets()
        {
            SecretProvider provider = new SecretProvider(Environment.GetEnvironmentVariable("CertificateThumbprint"));
            return Secrets.ToDictionary(
                kv => kv.Key,
                kv => provider.Decrypt(kv.Value));
        }
    }
}
