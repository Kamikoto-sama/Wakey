using System.Text;

namespace Proxy.Utils;

public static class StringExtensions
{
    public static string Replace(this string value, string oldValue, string newValue)
    {
        return new StringBuilder(value).Replace(oldValue, newValue).ToString();
    }
}