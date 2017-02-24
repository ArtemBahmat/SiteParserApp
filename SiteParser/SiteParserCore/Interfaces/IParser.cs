using HtmlAgilityPack;
using SiteParserCore.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiteParserCore.Interfaces
{
    public interface IParser
    {
        string BaseUrl { get; }
        bool IsAlive { get; set; }
        bool ParseExternalLinks { get; set; }
        int MaxNestingLevel { get; }
        Site Site { get; set; }
        int TotalUrlsCount { get; }
        int ParsedUrlsCount { get; }
        HashSet<string> UrlNames { get; set; }
        ConcurrentQueue<Url> UrlsQueue { get; set; }
        ConcurrentBag<Url> ReadyUrls { get; set; }
        ConcurrentBag<Resource> ReadyResources { get; set; }
        HashSet<Resource> Resources { get; }
        List<Task> Tasks { get; set; }
        HtmlWeb Web { get; set; }
        int MaxThreadsCount { get; set; }  

        void Parse();
        void Initialize(string url, int maxThreadsCount, int nestingLevel, bool parseAllLinks, Site site);
        void Reset();
    }
}