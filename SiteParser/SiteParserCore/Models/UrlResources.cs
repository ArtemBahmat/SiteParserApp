using System.Collections.Generic;

namespace SiteParserCore.Models
{
    public class UrlResources
    {
        public Url Url { get; set; }
        public IEnumerable<Resource> Resources { get; set; }
        public UrlResources(Url url, IEnumerable<Resource> resources)
        {
            Url = url;
            Resources = resources;
        }
    }
}
