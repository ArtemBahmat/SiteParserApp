using System.Data.Entity;

namespace SiteParserCore.Models
{
    class ParserContext : DbContext
    {
        public ParserContext() : base("SpDbConnection") { }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Url> Urls { get; set; }
        public DbSet<Resource> Resources { get; set; }
    }
}
