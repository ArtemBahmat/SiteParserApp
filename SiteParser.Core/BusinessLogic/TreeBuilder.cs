using SiteParser.Core.Interfaces;
using System.Collections.Generic;
using SiteParser.DAL.Models;

namespace SiteParser.Core.BusinessLogic
{
    public abstract class TreeBuilder : ITreeBuilder
    {
        public string BaseUrl { protected get; set; }
        public int NestingLevel { get; set; }
        public int SiteId { protected get; set; }
        public bool GetExternal { get; set; }
        public List<Url> Urls { protected get; set; }

        public abstract bool BuildTree();
    }
}
