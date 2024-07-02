using System;
using System.Security.Cryptography;
using System.Text;
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

        public static Guid GenerateGuid(string @string)
        {
            using MD5 md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.Default.GetBytes(@string)));
        }
    }
}