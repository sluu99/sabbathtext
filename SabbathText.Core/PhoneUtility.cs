using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public static class PhoneUtility
    {
        public static string ExtractUSPhoneNumber(string phoneNumber)
        {
            string digits = ExtractDigits(phoneNumber);
            
            if (!IsValidUSPhoneNumber(digits))
            {
                return null;
            }

            return NormalizePhoneNumber(digits);
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length == 11)
            {
                return "+" + phoneNumber;
            }

            return "+1" + phoneNumber;
        }

        private static bool IsValidUSPhoneNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            str = str.Trim();

            if (!(str.Length == 10 || str.Length == 11))
            {
                return false;
            }

            if (str.Length == 11 && str[0] != '1')
            {
                return false;
            }

            return true;
        }

        private static string ExtractDigits(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            MatchCollection matches = Regex.Matches(str, @"\d+");

            StringBuilder sb = new StringBuilder();

            foreach (Match match in matches)
            {
                sb.Append(match.Value);
            }

            return sb.ToString();
        }
    }
}
