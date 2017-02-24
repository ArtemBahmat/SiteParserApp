using SiteParserCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteParserCore.Repository
{
    public class SitesRepository : Repository2<Site>
    {
        public override void AddRange(IEnumerable<Site> sites)
        {
            base.AddRange(sites);
        }

        public override List<Site> GetRange(IEnumerable<Site> sites)
        {
            return base.GetRange(sites);
        }

        public override void Add(Site site)
        {
            base.Add(site);
        }

        public override Site Get(string name)
        {
            return base.Get(name);
        }
    }
}
