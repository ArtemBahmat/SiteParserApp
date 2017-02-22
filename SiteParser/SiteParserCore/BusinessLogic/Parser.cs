using HtmlAgilityPack;
using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiteParserCore.BusinessLogic
{
    public class Parser : IParser
    {
        private int _maxThreadCount;
        private string _baseUrl;
        public static object _locker = new object();
        private TaskFactory _taskFactory = new TaskFactory();
        private UrlValidator _urlValidator;

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
        public string Domain { get; set; }
        public bool IsAlive { get; set; }
        public bool ParseExternalLinks { get; set; }
        public int MaxNestingLevel { get; private set; }
        public Site Site { get; set; }
        public int TotalUrlsCount { get; private set; }
        public int ParsedUrlsCount { get; private set; }
        public HashSet<Url> Urls { get; private set; } = new HashSet<Url>();
        public HashSet<Resource> Resources { get; private set; } = new HashSet<Resource>();
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
                    Task task = _taskFactory.StartNew(() =>
                              StartParse());
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
            Urls.Clear();
            Resources.Clear();
            Tasks.Clear();
            IsAlive = true;
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
            System.Diagnostics.Stopwatch sw;
            HashSet<Url> partialUrls = new HashSet<Url>();
            Url url = isParent ? CreateUrl(BaseUrl, State.IsInParsing, false, 0, Site.Id) : GetFirstUrlFromGlobalList();

            if (url != null)
            {
                try
                {
                    sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    HtmlDocument doc = Web.Load(url.Name);
                    sw.Stop();
                    url.ResponseTime = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
                    url.HtmlSize = doc.GetHtmlSize();
                    url.DateTimeOfLastScan = DateTime.Now;
                    url.State = State.IsParsed;

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

        private Url CreateUrl(string name, State state, bool isExternal, int nestingLevel, int siteId)
        {
            Url url = new Url()
            {
                Name = name.ToLowerInvariant(),
                State = state,
                IsExternal = isExternal,
                NestingLevel = nestingLevel,
                SiteId = siteId
            };

            AddUrlToList(url);
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
                string href;

                foreach (var link in links)
                {
                    href = link.GetAttributeValue("href", string.Empty);
                    Url url = new Url()
                    {
                        Name = href.ToLowerInvariant(),
                        State = State.IsAwaiting,
                        IsExternal = !href.Contains(Domain),
                        NestingLevel = parentUrl.NestingLevel + 1,
                        SiteId = Site.Id,
                        ParentName = parentUrl.Name
                    };

                    AddUrlToList(url);
                }
            }
        }

        private Url GetFirstUrlFromGlobalList()
        {
            lock (_locker)
            {
                Url url = Urls?.Where(u => u.State == State.IsAwaiting).FirstOrDefault();

                if (url != null)
                {
                    url.State = State.IsInParsing;
                }

                return url;
            }
        }

        private void AddUrlToList(Url url)
        {
            if (_urlValidator.IsValid(url))
            {
                lock (_locker)
                {
                    int startCount = Urls.Count;
                    Urls.Add(url);
                    int finishCount = Urls.Count;

                    if (finishCount > startCount)
                    {
                        TotalUrlsCount++;
                    }
                }
            }
        }

        private void AddResourcesToList(IEnumerable<Resource> resources)
        {
            if (resources == null) return;

            try
            {
                lock (_locker)
                {
                    Resources.UnionWith(resources);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
