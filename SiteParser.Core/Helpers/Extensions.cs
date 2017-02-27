using HtmlAgilityPack;
using System.Text;

namespace SiteParser.Core.Helpers
{
    public static class Extensions
    {
        public static int GetHtmlSize(this HtmlDocument doc)
        {
            return Encoding.UTF8.GetByteCount(doc.DocumentNode.OuterHtml)
                 + Encoding.UTF8.GetByteCount(doc.DocumentNode.InnerText);
        }
    }
}
