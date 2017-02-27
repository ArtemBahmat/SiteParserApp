using SiteParser.Core.Interfaces;
using System.Collections.Generic;
using System;
using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.DAL.Repositories;

namespace SiteParser.Core.BusinessLogic
{
    public abstract class TreeBuilder : ITreeBuilder
    {
        private IRepository<Site> _siteRepository;
        private IRepository<Url> _urlRepository;

        public string BaseUrl { get; set; }
        public int NestingLevel { get; set; }
        public int SiteId { get; set; }
        public bool GetExternal { get; set; }
        public List<Url> Urls { get; set; }

        public abstract bool BuildTree();
    }
}
