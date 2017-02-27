using System.Collections.Generic;

namespace SiteParser.DAL.Models
{
    public class UrlResources
    {
        private Url Url { get; set; }
        private IEnumerable<Resource> Resources { get; set; }
        public UrlResources(Url url, IEnumerable<Resource> resources)
        {
            Url = url;
            Resources = resources;
        }
    }
}
