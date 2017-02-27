using HtmlAgilityPack;
using SiteParser.Core.Helpers;
using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiteParser.Utils;

namespace SiteParser.Core.BusinessLogic
{
    public class Parser : IParser
    {
        private int _maxThreadCount;
        private string _baseUrl;
        private static readonly object Locker = new object();
        private readonly TaskFactory _taskFactory = new TaskFactory();
        private UrlValidator _urlValidator;

        private IRepository<ImportUrl> ImportUrlRepository { get; set; }
        private string Domain { get; set; }
        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            private set
            {
                _baseUrl = value;
                Domain = UrlHelper.GetDomainFromUrl(_baseUrl);
            }
        }
        public bool IsAlive { get; set; } = true;
        public bool ParseExternalLinks { get; set; }
        public int MaxNestingLevel { get; private set; }
        public Site Site { get; set; }
        public int TotalUrlsCount { get; private set; }
        public int ParsedUrlsCount { get; private set; }
        public HashSet<string> UrlNames { get; set; } = new HashSet<string>();
        public ConcurrentQueue<Url> UrlsQueue { get; set; } = new ConcurrentQueue<Url>();
        public ConcurrentBag<Url> ReadyUrls { get; set; } = new ConcurrentBag<Url>();
        public HashSet<Resource> Resources { get; } = new HashSet<Resource>();
        public ConcurrentBag<Resource> ReadyResources { get; set; } = new ConcurrentBag<Resource>();
        public List<Task> Tasks { get; set; } = new List<Task>();
        public HtmlWeb Web { get; set; } = new HtmlWeb();
        public int MaxThreadsCount
        {
            get
            {
                return _maxThreadCount;
            }
            set
            {
                _maxThreadCount = value;
                ThreadPool.SetMaxThreads(_maxThreadCount, _maxThreadCount);
                ThreadPool.SetMinThreads(_maxThreadCount, _maxThreadCount);
            }
        }

        public Parser(IRepository<ImportUrl> importUrlRepo)
        {
            ImportUrlRepository = importUrlRepo;
        }

        public void Initialize(string url, int maxThreadsCount, int nestingLevel, bool parseAllLinks, Site site)
        {
            Reset();
            BaseUrl = url;
            MaxThreadsCount = maxThreadsCount;
            MaxNestingLevel = nestingLevel;
            ParseExternalLinks = parseAllLinks;
            Site = site;
            _urlValidator = new UrlValidator(BaseUrl, ParseExternalLinks);
        }

        public void Parse()
        {
            if (ParseUrls(true))
            {
                for (int i = 0; i < MaxThreadsCount; i++)
                {
                    Task task = _taskFactory.StartNew(StartParse);
                    Tasks.Add(task);
                }
            }

            Task[] tasks = Tasks.ToArray<Task>();
            Task.WaitAll(tasks);
            IsAlive = false;
        }

        public void Reset()
        {
            TotalUrlsCount = 0;
            ParsedUrlsCount = 0;
            UrlNames.Clear();
            UrlsQueue.Clear();
            ReadyUrls.Clear();
            Resources.Clear();
            Tasks.Clear();
            IsAlive = true;
            ImportUrlRepository.DeleteAll();
        }

        private void StartParse()
        {
            while (IsAlive)
            {
                if (!ParseUrls())
                {
                    break;
                }
            }
        }

        private bool ParseUrls(bool isParent = false)
        {
            bool toContinue = true;
            Url url = GetUrl(isParent);

            if (url != null)
            {
                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    HtmlDocument doc = Web.Load(url.Name);
                    sw.Stop();
                    url.ResponseTime = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
                    url.HtmlSize = doc.GetHtmlSize();
                    url.DateTimeOfLastScan = DateTime.Now;
                    ReadyUrls.Add(url);

                    if (url.NestingLevel < MaxNestingLevel)
                    {
                        ProcessResources(doc, url);
                        ProcessLinks(doc, url);
                    }

                    ParsedUrlsCount++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }
            else
            {
                toContinue = false;
            }

            return toContinue;
        }

        private Url GetUrl(bool isParent)
        {
            Url url;

            if (isParent)
            {
                url = CreateUrl(BaseUrl, false, 0, Site.Id);
            }
            else
            {
                UrlsQueue.TryDequeue(out url);
            }

            return url;
        }

        private Url CreateUrl(string name, bool isExternal, int nestingLevel, int siteId)
        {
            Url url = new Url()
            {
                Name = name.ToLowerInvariant(),
                IsExternal = isExternal,
                NestingLevel = nestingLevel,
                SiteId = siteId
            };

            AddUrl(url);
            return url;
        }

        private void ProcessResources(HtmlDocument doc, Url parentUrl)
        {
            try
            {
                IEnumerable<Resource> images = doc.DocumentNode.Descendants("img")
                              ?.Select(e => e.GetAttributeValue("src", null))
                              .Where(s => !string.IsNullOrEmpty(s))
                              .Select(srcUrl => UrlHelper.GetAbsoluteUrl(srcUrl, parentUrl.Name))
                              .Select(x => new Resource() { Name = x, ParentName = parentUrl.Name, ParentSiteId = Site.Id, Type = ResourceType.Image, State = State.IsAwaiting });

                AddResourcesToList(images);

                IEnumerable<Resource> css = doc.DocumentNode.SelectNodes("/html/head/link[@rel='stylesheet']")
                              ?.Select(e => e.GetAttributeValue("href", null))
                              .Where(s => !string.IsNullOrEmpty(s))
                              .Select(srcUrl => UrlHelper.GetAbsoluteUrl(srcUrl, parentUrl.Name))
                              .Select(x => new Resource() { Name = x, ParentName = parentUrl.Name, ParentSiteId = Site.Id, Type = ResourceType.Css, State = State.IsAwaiting });


                AddResourcesToList(css);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void ProcessLinks(HtmlDocument doc, Url parentUrl)
        {
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");

            if (links != null)
            {
                foreach (HtmlNode link in links)
                {
                    string href = link.GetAttributeValue("href", string.Empty);
                    Url url = new Url()
                    {
                        Name = href.ToLowerInvariant(),
                        IsExternal = !href.Contains(Domain),
                        NestingLevel = parentUrl.NestingLevel + 1,
                        SiteId = Site.Id,
                        ParentName = parentUrl.Name
                    };

                    AddUrl(url);
                }
            }
        }

        private bool AddUrlName(Url url)
        {
            bool success = false;
            if (!_urlValidator.IsValid(url)) return false;

            lock (Locker)
            {
                int startCount = UrlNames.Count;
                UrlNames.Add(url.Name);
                int finishCount = UrlNames.Count;

                if (finishCount > startCount)
                {
                    TotalUrlsCount++;
                    success = true;
                }
            }
            return success;
        }

        private bool AddImportUrl(Url url)
        {
            bool success = false;

            if (_urlValidator.IsValid(url) && ImportUrlRepository.TryAdd(new ImportUrl(url.Name)))
            {
                TotalUrlsCount++;
                success = true;
            }

            return success;
        }

        private void AddUrl(Url url)
        {
            if (AddImportUrl(url))
            //if (AddUrlName(url))   // for storing urlNames in HashSet
            {
                UrlsQueue.Enqueue(url);
            }
        }

        private void AddResourcesToList(IEnumerable<Resource> resources)
        {
            if (resources == null) return;

            try
            {
                var enumerable = resources as IList<Resource> ?? resources.ToList();
                lock (Locker)
                {
                    Resources.UnionWith(enumerable);
                }
                ReadyResources.AddRange(enumerable);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
