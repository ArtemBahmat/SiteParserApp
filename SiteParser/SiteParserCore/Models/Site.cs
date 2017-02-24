using SiteParserCore.Repository;
using System.Collections.Generic;

namespace SiteParserCore.Models
{
    public class Site : Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Threads { get; set; }
        public int MaxNestingLevel { get; set; }
        public bool ToParseExternalLinks { get; set; }

        private IEnumerable<Url> Urles { get; set; }
        public Site()
        {
            Urles = new List<Url>();
        }
    }
}
