using System;
using System.Security.Cryptography;
using System.Text;

public static class StringExtension
{
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
}
