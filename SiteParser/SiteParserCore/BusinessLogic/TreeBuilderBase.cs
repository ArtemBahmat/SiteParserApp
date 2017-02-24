using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using System.Collections.Generic;
using System;

namespace SiteParserCore.BusinessLogic
{
    public abstract class TreeBuilderBase : ITreeBuilder
    {
        private readonly IRepository _dBRepository;

        protected Url Url { get; private set; }
        protected string BaseUrl { get; }
        private int NestingLevel { get; set; }
        private int SiteId { get; set; }
        private bool GetExternal { get; set; }
        protected List<Url> Urls { get; private set; }

        protected TreeBuilderBase(string baseUrl, int nestingLevel, bool getExternal)
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
