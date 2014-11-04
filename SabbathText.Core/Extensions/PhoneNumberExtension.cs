
public static class PhoneNumberExtension
{
    public static string Mask(this string str, int show)
    {
        if (str == null || str.Length <= show)
        {
            return str;
        }

        return "*".Repeat(str.Length - show) + str.Substring(str.Length - show, show);
    }
}
