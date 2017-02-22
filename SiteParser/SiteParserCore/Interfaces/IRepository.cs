using SiteParserCore.Models;
using System.Collections.Generic;

namespace SiteParserCore.Interfaces
{
    public interface IRepository
    {
        void SaveUrls(IEnumerable<Url> urls);
        void SaveResources(IEnumerable<Resource> resources);
        void SaveSite(Site site);
        void DeleteResources(int siteId);
        List<Url> GetUrls(int siteId, bool getExternal, int? nestingLevel=null);
        List<Site> GetSites();
        Site GetSite(string url);
    }
}