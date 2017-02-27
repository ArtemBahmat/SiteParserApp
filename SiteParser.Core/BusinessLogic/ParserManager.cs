using SiteParser.Core.Helpers;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.Utils;
using System.Collections.Generic;
using System.Threading;

namespace SiteParser.Core.BusinessLogic
{
    public class ParserManager : IParserManager
    {
        private const int ThreadsCount = 20;
        private const int MaxNestingLevel = 5;

        public ParserManager(IRepository<Site> siteRepo, IRepository<Url> urlRepo, IRepository<Resource> resRepo, IParser parser)
        {
            SitesRepository = siteRepo;
            UrlsRepository = urlRepo;
            ResourceRepository = resRepo;
            ParserInstance = parser;
        }

        public bool IsAlive
        {
            get
            {
                return ParserInstance.IsAlive;
            }
            set
            {
                ParserInstance.IsAlive = value;
            }
        }
        public IParser ParserInstance { get; }
        private IRepository<Resource> ResourceRepository { get; }
        private IRepository<Url> UrlsRepository { get; }
        private IRepository<Site> SitesRepository { get; }

        public void Execute(string url, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            Initialize(url, maxThreadsCount, nestingLevel, parseExternal);

            if (ParserInstance?.Site == null) return;
            ResourceRepository.Delete(ParserInstance.Site.Id);   

            Thread additionalThread = new Thread(ParserInstance.Parse);
            additionalThread.Start();

            bool isAlive;
            do
            {
                Thread.Sleep(1000);
                isAlive = ParserInstance.IsAlive;

                IEnumerable<Url> urls = GetUrlsToSave();
                UrlsRepository.AddRange(urls); 

                IEnumerable<Resource> resources = GetResourcesToSave();     
                ResourceRepository.AddRange(resources);                    

            } while (isAlive);
        }

        private void Initialize(string urlName, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            urlName = UrlHelper.GetCorrectUrl(urlName);
            if (string.IsNullOrEmpty(urlName)) return;
            Site site = SitesRepository.Get(urlName) ?? CreateSite(urlName);
            ParserInstance.Initialize(urlName, maxThreadsCount, nestingLevel, parseExternal, site);
        }

        private Site CreateSite(string url)
        {
            Site site = new Site() { Name = url, Threads = ThreadsCount, MaxNestingLevel = MaxNestingLevel, ToParseExternalLinks = false };
            SitesRepository.Add(site); 
            return site;
        }

        private IEnumerable<Url> GetUrlsToSave()
        {
            IEnumerable<Url> result = null;

            if (ParserInstance != null)
            {
                result = ParserInstance.ReadyUrls.TryTakeAll();
            }

            return result;
        }

        private IEnumerable<Resource> GetResourcesToSave()
        {
            IEnumerable<Resource> result = null;

            if (ParserInstance != null)
            {
                result = ParserInstance.ReadyResources.TryTakeAll();
            }

            return result;
        }
    }
}
