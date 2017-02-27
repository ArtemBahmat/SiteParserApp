using SiteParser.DAL.Models;
using System.Collections.Concurrent;

namespace SiteParser.Core.Interfaces
{
    public interface IParser
    {
        bool IsAlive { get; set; }
        Site Site { get; }
        int TotalUrlsCount { get; }
        int ParsedUrlsCount { get; }
        ConcurrentBag<Url> ReadyUrls { get; }
        ConcurrentBag<Resource> ReadyResources { get; }

        void Parse();
        void Initialize(string url, int maxThreadsCount, int nestingLevel, bool parseAllLinks, Site site);
    }
}