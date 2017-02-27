using System.Data.Entity;

namespace SiteParser.DAL.Models
{
    public class ParserContext : DbContext
    {
        public ParserContext() : base("SpDbConnection") { }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Url> Urls { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ImportUrl> ImportUrls { get; set; }
    }
}
