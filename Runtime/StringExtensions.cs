using System.Threading;

namespace BlockadeLabsSDK
{
    internal static class StringExtensions
    {
        public static string ToTitleCase(this string @string)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(@string);
        }
    }
}