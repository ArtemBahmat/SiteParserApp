using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using StructureMap;
using System.Collections.Generic;
using System.Threading;

namespace SiteParserCore.BusinessLogic
{
    public class ParserManager : IParserManager
    {
        const int THREADS_COUNT = 20;
        const int MAX_NESTING_LEVEL = 5;

        public IContainer container = new Container(x =>
        {
            x.For<IParser>().Use<Parser>();
            x.For<IRepository>().Use<Repository>();
        });

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
        public IParser ParserInstance { get; set; }
        private IRepository DbRepository { get; set; }

        public void Execute(string url, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            Initialize(url, maxThreadsCount, nestingLevel, parseExternal);

            if (ParserInstance?.Site != null)
            {
                DbRepository.DeleteResources(ParserInstance.Site.Id);

                Thread AdditionalThread = new Thread(ParserInstance.Parse);
                AdditionalThread.Start();

                bool isAlive = false;
                do
                {
                    Thread.Sleep(1000);
                    isAlive = ParserInstance.IsAlive;

                    IEnumerable<Url> urls = GetUrlsToSave();
                    DbRepository.SaveUrls(urls);

                    IEnumerable<Resource> resources = GetResourcesToSave();     
                    DbRepository.SaveResources(resources);                    

                } while (isAlive);
            }
        }

        private void Initialize(string url, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            url = UrlHelper.GetCorrectUrl(url);

            if (!string.IsNullOrEmpty(url))
            {
                DbRepository = container.GetInstance<IRepository>();
                Site site = DbRepository.GetSite(url);

                if (site == null)
                {
                    site = CreateSite(url);
                }

                if (ParserInstance == null)
                {
                    ParserInstance = container.GetInstance<IParser>();
                }

                ParserInstance.Initialize(url, maxThreadsCount, nestingLevel, parseExternal, site);
            }
        }

        private Site CreateSite(string url)
        {
            Site site = new Site() { Name = url, Threads = THREADS_COUNT, MaxNestingLevel = MAX_NESTING_LEVEL, ToParseExternalLinks = false };
            DbRepository.SaveSite(site);
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
