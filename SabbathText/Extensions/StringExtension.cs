﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// String extensions
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// This regex only allows alpha numeric and white spaces
    /// </summary>
    private static readonly Regex AlphaNumericSpaceRegex = new Regex(@"[a-zA-Z0-9\s]+");

    /// <summary>
    /// Repeats a string N times
    /// </summary>
    /// <param name="str">The string</param>
    /// <param name="n">Number of times to repeat</param>
    /// <returns>A new string</returns>
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

    /// <summary>
    /// Hash a string using SHA256
    /// </summary>
    /// <param name="str">The string</param>
    /// <returns>The hex representation of the hash</returns>
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

    /// <summary>
    /// Extract alpha numeric and white spaces from a string
    /// </summary>
    /// <param name="str">The string</param>
    /// <returns>A new string which contains only alpha numeric and white spaces</returns>
    public static string ExtractAlphaNumericSpace(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        MatchCollection matches = AlphaNumericSpaceRegex.Matches(str);

        StringBuilder sb = new StringBuilder();

        foreach (Match match in matches)
        {
            sb.Append(match.Value);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Invariant string format
    /// </summary>
    /// <param name="str">The string format</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>A formatted string</returns>
    public static string InvariantFormat(this string str, params object[] parameters)
    {
        return string.Format(CultureInfo.InvariantCulture, str, parameters);
    }

    /// <summary>
    /// Extracts a US phone number from a string
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    /// <returns>A string in the form of a US phone number</returns>
    public static string ExtractUSPhoneNumber(this string phoneNumber)
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
