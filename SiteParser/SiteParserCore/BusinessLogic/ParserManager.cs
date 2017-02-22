using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
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
        public IRepository DBRepository { get; set; } 

        public void Execute(string url, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            Initialize(url, maxThreadsCount, nestingLevel, parseExternal);
            bool isAlive = false;

            if (ParserInstance != null && ParserInstance.Site != null)
            {
                DBRepository.DeleteResources(ParserInstance.Site.Id);

                Thread AdditionalThread = new Thread(ParserInstance.Parse);
                AdditionalThread.Start();

                do
                {
                    Thread.Sleep(1000);
                    isAlive = ParserInstance.IsAlive;

                    List<Url> urls = GetUrlsToSave();
                    DBRepository.SaveUrls(urls);

                    List<Resource> resources = GetResourcesToSave();
                    DBRepository.SaveResources(resources);

                } while (isAlive);
            }
        }

        private void Initialize(string url, int maxThreadsCount, int nestingLevel, bool parseExternal)
        {
            url = UrlHelper.GetCorrectUrl(url);

            if (!string.IsNullOrEmpty(url))
            {
                DBRepository = container.GetInstance<IRepository>();
                Site site = DBRepository.GetSite(url);

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
            DBRepository.SaveSite(site);
            return site;
        }

        private List<Url> GetUrlsToSave()
        {
            List<Url> result = null;

            lock (Parser._locker)
            {
                if (ParserInstance != null)
                {
                    result = new List<Url>();

                    IEnumerable<Url> notProcessedUrls = ParserInstance.Urls.Where(url => url.State == State.IsParsed).ToList();
                    IEnumerable<Url> resultUrls = ParserInstance.Urls.Intersect(notProcessedUrls);

                    foreach (var url in resultUrls)
                    {
                        url.State = State.IsSaved;
                    }
                                        
                    result.AddRange(resultUrls);
                }

                return result;
            }
        }

        private List<Resource> GetResourcesToSave()
        {
            List<Resource> result = null;

            lock (Parser._locker)
            {
                if (ParserInstance != null)
                {
                    result = new List<Resource>();

                    IEnumerable<Resource> notProcessedRes = ParserInstance.Resources.Where(res => res.State == State.IsAwaiting).ToList();
                    IEnumerable<Resource> resultRes = ParserInstance.Resources.Intersect(notProcessedRes);

                    foreach (var res in resultRes)
                    {
                        res.State = State.IsSaved;
                    }
                    result.AddRange(resultRes);
                }

                return result;
            }
        }
    }
}
