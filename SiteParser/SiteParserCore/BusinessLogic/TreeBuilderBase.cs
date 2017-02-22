using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using System.Collections.Generic;
using System;

namespace SiteParserCore.BusinessLogic
{
    public abstract class TreeBuilderBase : ITreeBuilder
    {
        private IRepository _dBRepository;

        public Url Url { get; set; }
        public string BaseUrl { get; set; }
        public int NestingLevel { get; set; }
        public int SiteId { get; set; }
        public bool GetExternal { get; set; }
        public List<Url> Urls { get; set; }

        public TreeBuilderBase(string baseUrl, int nestingLevel, bool getExternal)
        {
            _dBRepository = new Repository();
            BaseUrl = baseUrl;
            NestingLevel = nestingLevel;
            GetExternal = getExternal;
            Url = new Url()
            {
                Name = BaseUrl,
                State = State.IsAwaiting,
                IsExternal = false,
                NestingLevel = 0,
                SiteId = SiteId
            };
        }

        public void Initialize()
        {
            Site site = _dBRepository.GetSite(BaseUrl);

            if (site != null)
            {
                SiteId = site.Id;
                Urls = _dBRepository.GetUrls(SiteId, GetExternal, NestingLevel);
            }
        }

        public abstract bool BuildTree();
    }
}
