using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiteParserCore.Helpers
{
    public static class Extensions
    {
        public static int GetHtmlSize(this HtmlDocument doc)
        {
           return Encoding.UTF8.GetByteCount(doc.DocumentNode.OuterHtml)
                + Encoding.UTF8.GetByteCount(doc.DocumentNode.InnerText);
        }

        public static bool IsASCII(this string value)
        {
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public static int FirstOrDefault(this IEnumerable<int> sequence, int defaultValue)
        {
            return sequence.Any() ? sequence.First() : defaultValue;
        }
    }
}
