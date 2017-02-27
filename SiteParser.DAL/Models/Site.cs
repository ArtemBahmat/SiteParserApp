using System.Collections.Generic;

namespace SiteParser.DAL.Models
{
    public class Site : Entity
    {
        public new int Id { get; set; }
        public new string Name { get; set; }
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
