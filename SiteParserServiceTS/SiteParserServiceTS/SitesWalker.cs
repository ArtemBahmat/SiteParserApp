using SiteParserCore.BusinessLogic;
using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using System;
using System.Collections.Generic;

namespace SiteParserServiceTS
{
    public class SitesWalker
    {
        IParserManager _parserManager;
        IRepository _dBRepository;
        List<Site> sites;

        public bool Stopped { get; set; }

        public SitesWalker()
        {
            sites = new List<Site>();
            _dBRepository = ConfigureService.container.GetInstance<IRepository>();
        }

        public void Start()
        {
            Stopped = false;

            try
            {
                sites = _dBRepository.GetSites();
                _parserManager = ConfigureService.container.GetInstance<IParserManager>();

                if (sites.Count > 0)
                {
                    foreach (Site site in sites)
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
