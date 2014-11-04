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
}
