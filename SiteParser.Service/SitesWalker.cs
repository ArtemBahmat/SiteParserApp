using SiteParser.Core.Interfaces;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.DAL.Repositories;
using SiteParser.Utils;
using SiteParserService.Interfaces;
using System;
using System.Collections.Generic;

namespace SiteParserService
{
    public class SitesWalker : IWalker
    {
        private IParserManager _parserManager;
        private readonly IRepository<Site> _sitesRepository;
        private List<Site> _sites;

        private bool Stopped { get; set; }

        public SitesWalker()
        {
            _sites = new List<Site>();
            _sitesRepository = new Repository<Site>();
        }

        public void Start()
        {
            Stopped = false;

            try
            {
                _sites = _sitesRepository.GetRange(null); 
                _parserManager = ConfigureService.Container.GetInstance<IParserManager>();

                if (_sites.Count > 0)
                {
                    foreach (Site site in _sites)
                    {
                        if (!Stopped)
                        {
                            Log.Info($"START parsing site: {site.Name} with threads: {site.Threads}, max nesting level: {site.MaxNestingLevel}, parse external links: {site.ToParseExternalLinks}");
                            _parserManager.Execute(site.Name, site.Threads, site.MaxNestingLevel, site.ToParseExternalLinks);
                            Log.Info("FINISH parsing site: " + site.Name);
                        }
                    }
                }
                else
                {
                    Log.Error("No sites in db");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                Stopped = true;
            }
        }

        public void Stop()
        {
            Log.Info("Walker stopped");
            Stopped = true;
            _parserManager.IsAlive = false;
        }
    }
}
