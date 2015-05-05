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
                "Khue9nEU29oceupWUmt6cuDlZwB1dEbkqD4PfQxdDzWwP8wSjQ9Nh+fKBIny8STGJlTANCG4l+Nr4kNKpZLJ+RceNL+EIX6o60/qbDg3Srbza6lNZcWXdy71SN76mboY/rlBq++VHDljlrwW8K6frvUdZTwDoS59wLaSJ35AC5/+BZFuGLGLOhSuHFDYPhB0KDz01VGM3laX+cuAEOg9CPW0cHihg7CfxrAht1iU+xdvVX5r3K65ulALlvCMDrZGLjOXmAdgNbSEfD1/yBOwI0/cQkAFJ9AvHfoskvcwylxgeYO0J+DakdWKSKo7YEbFoT5IqK/M0pT4YquH+r026w=="
            },
            {
                TwilioTokenKey,
                "ACpQcXcDybxFOE/5HtZTis3tVI+w/ZCRBTqwAcaX4oYpgNPpvP2hey44Rk9PEiU+FcPSKXVEbvRyoLUgEG3wdFIJA86LLzDoL0eYgozrt8yuy9vm/IiRoEPA6oKOc3dX17DWH7uekJNo6SwMsFlkbw25Pv6gONpN2G6akp0AASlkABzxZzUE6X7hEkfiU7UzVEf8wekSdGoXJ+H6DHlDoW6N0zOXvTuYlneigm5h/hzmhKO/6FU5HzSLFda+nbivxx7DGNQlV4/CtmCu19SrleVwC3DFoAcEzDwp9dCS+0+2WRHBF94VxUodSSPjllmiH5Eqau+804WwU//82wLqFg=="
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
