using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtension
{
    static readonly Regex StripPunctuationRegex = new Regex(@"[^:,\.!\?]+");

    public static string Repeat(this string str, int n)
    {
        if (n <= 1)
        {
            return str;
        }

        StringBuilder sb = new StringBuilder(str.Length * n);

        for (int i = 0; i < n; i++)
        {
            sb.Append(str);
        }

        return sb.ToString();
    }

    public static string Sha256(this string str)
    {
        if (str == null)
        {
            return null;
        }

        byte[] data = Encoding.UTF8.GetBytes(str);

        using (SHA256Managed sha = new SHA256Managed())
        {
            byte[] hashBytes = sha.ComputeHash(data);

            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }

    public static string StripPunctuation(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        MatchCollection matches = StripPunctuationRegex.Matches(str);

        StringBuilder sb = new StringBuilder();

        foreach (Match match in matches)
        {
            sb.Append(match.Value);
        }

        return sb.ToString();
    }

    public static string GetParameters(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        str = str.Trim();
        int index = str.IndexOf(' ');

        if (index == -1)
        {
            return null;
        }

        return str.Substring(index + 1);
    }
}
