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
        private static readonly Dictionary<string, string> Secrets = new Dictionary<string, string>
        {
            {
                KeyValueStoreConnectionStringKey,
                "fQL7xc/L93HwEKJOiGilz1fbCX0KKazxg0mEJ1z3RgTQ9pxshhzSkcFX0wA6Yzcab0/tsofxzTDBryLOhmZ79znnYein39Rh0TzTYuMpCYIa5Fkl+k2HXPg1gv9TkONCqUXkiMv2xkWjDKiUGOt6PVmLDMtqcg9fpU1DXBDzka3y0guqqMgcBAI1ZD24XUstzn0/2nkbcwToX0b5By/T05M1yTgGNiFbxb7s9ZQeL31Dgcw0QG43v1b+5NLLkFqpsoQjczT4Rg8hZXKKQSSg5Jbrv7so2e2eqUcpv+OVyNrGix6oT3Votfr+Bb0Zhw0/eRgWnz1RhpgBunEnLGEXsQ=="
            },
            {
                ServicePhoneNumberKey,
                "mYEjvyNqDbaq/0bphHwsaYQEr6x9eNQdZ42Zx9yO3/IJVGEy4ogB5vOS1F+d64qZ9Qa67RvMtg840hCGpPnW9nm9GRae5Lj/Iy1u+6IjdQab+tpH4cBgfo9fJRi2wn7hKtcUttxHuvrC8yeeq2wj5ORhSAfFVaws/Ink8xanfDqKnwNEENNdE882NMAbyM4tU1ex1VNVSB3pGWOCPw8cuxzSBew7FDPb+U65eo6BEVkbmMzLB+ni/AYKNgeUCvuAKviWDw7gxtWp4LyH8d0UTDOG7gAwq1lAqAFf3c/qxwjdR4+5lJ8/UEMWVX1ro3YSpFu1bRxqGGb0DXAEA2JG8w=="
            },
            {
                TwilioAccountKey,
                "DI73bfso1JVPMnD9Vw8efHaUJ18ANzma8/xXs8H9b2yJBFub/GS0IFTxV1dH60DEfxS9vGSMvd+SZHW8USag7BK0fKUuyvlEhk9b1SHy5ocQXDUcdD0dgBq+Od08MDNSA3f45WD5JPOEV2laT1u2k7FQEWld7igXSwH7E"
            },
            {
                TwilioTokenKey,
                "TSEh+qK5G44t+SQ99tqkjzGYZuxD19kNwCYp3iRVy+WNEHrkAKmnJrBvZtT66oPQrCoEtwDNpgZHy/Hwt9sbS+rRw2q679hNgUq+Yz87/hbyaxb1RDqyaUV2aWE1oIJv7UlDGR8h"
            },
        };

        /// <summary>
        /// Creates a new instance of the staging environment settings.
        /// </summary>
        public StagingEnvironmentSettings()
            : base(DecryptSecrets())
        {
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
