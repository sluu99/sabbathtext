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
            {
                IncomingMessagePrimaryTokenKey,
                "ZMTsZ9Vu8YBIAP5ZEkiEgt3L2yOh/4Tp1Jtrcwpul0B9Tywo4Nr8UhXIH3+ppidpoLJDblbAIT0e8wbGXZPNPcwQj2JRFvn+350LqWfQ1N2ctDqH/OGNGUjpCvdZJ9MSIaNxc+owAjSNJGEQdv8HJiQxxwb6k4qCtbMMST3qrUw6ixt+WUtMdrUQO/ZI+LzZS+YEjVQCipFpGo5ukUlrOpom1NiOWb1MabiLzZ+fggPto7TiZ3AzzzaRFOuDYxsm1vyxxCg8mNGzRPyX0RQiuYBWIb6OEb/Z1DmMXOtLfQuzNCIy/JhAmcfcV/ejQX/hw6h294X6KsbFwf8bcijAeQ=="
            },
            {
                IncomingMessageSecondaryTokenKey,
                "QNAnX+wp3wSKqwU1zvrSOX5dQ+3O6lmIknn2lqyJat+Y8iXYoxCjvv6ceENkj9cxTF8/WarJK627M2k88rgmgy2ZeDgggiIKw1cm6VHSEWnhBQANZZtKTlWhT2qW6vlOHCGO0C8WWBFOavXEs0N95ovhDPJ+YYNRsE/R8D42JRMnD83b9Rm1xLtrk0npoT7Vof9gKX+LLcglAiU92m3MF2zzeDpHOdzr1ZIoxk6rYI82YUcLCDvc6DnwnXkPM/NBU/XpWvgHS2e11K1obyD1kcuD4q0iTFlzPFAdjKrO+vjnZOI8aTHjnSz+j/such8UUDS9lhR/360hAy8Q8N7RUw=="
            },
            {
                GoogleClientIdKey,
                "BGgBBQCn5SlZ5VId6L3ZTDB6De9PvflXnbGzyai2ehlE5DpM+HL9QlVtFlFC3B05eEzSDUKoi78/eaqswhkl3kYbXYcakeysSuGLKbO7VTSBvgSE9fOnzVYyl9T83ZwQo5li63dQgDpNmO1E02u8WkZE54U2JJNY4e4NEeDNm8ZDCNqO0YeojqrdimNKbhccv6R8DQMHIDrKvfkuKBU9+PJxFyZ+3SaLKjDFp1E3hO3PCOAO4iyXUgopYmrogXcbF7misyK35Pxs7FefZcM1gE3u3ZiLzEYKC4wu0irS2DLFlY4l/YOmFL/oqgNUegN5A65antu5NtSI8T7VZP8oNA=="
            },
            {
                GoogleClientSecretKey,
                "Lja13w6QD/+qWj9jUJ56FA/MYUT872lin9cf07dCQFaLO2Pfc63LrGX6tH7Wcm2fUvV21CaNN2WRdsdqZr7nOs/J5G94SQ5G5xXkP8xITvI9w3fxO8lC+oioGvT1tbVDPPdqb8FPOkmet1XwK34mhJdVrYLS96xtvooKTuCNDmfw2Wvu1zt7f4TBmUoT+XSywO1mmVx4F/tBmGrN84BClpnaXN88YuKIhifbQdGsgui6ygTb3lRlYMGbhWF0v01GMdUPlIpicyqbZa8iyQ89cCpzKLtldyuoLz0Rsk9UkvAsgQ9b6eqXfjIygYHiBJT1GxSjvYoYUPRgCBv7D41xBw=="
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
