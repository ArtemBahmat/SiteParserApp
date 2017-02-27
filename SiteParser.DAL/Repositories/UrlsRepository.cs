using SiteParser.DAL.Extensions;
using SiteParser.DAL.Models;
using System.Collections.Generic;

namespace SiteParser.DAL.Repositories
{
    public class UrlsRepository : Repository<Url>
    {
        private readonly string _tableName;

        public UrlsRepository()
        {
            using (ParserContext db = new ParserContext())
            {
                _tableName = db.GetTableName<Url>();
            }
        }

        public override void AddRange(IEnumerable<Url> urls)
        {
            // base.AddRange(urls);       // if need using EF

            base.BulkOperation(
                   (bulk, connection) =>
                   {
                       bulk.Setup<Url>()
                          .ForCollection(urls)
                          .WithTable(_tableName)
                          .AddColumn(x => x.SiteId)
                          .AddColumn(x => x.Name)
                          .AddColumn(x => x.ParentName)
                          .AddColumn(x => x.NestingLevel)
                          .AddColumn(x => x.HtmlSize)
                          .AddColumn(x => x.IsExternal)
                          .AddColumn(x => x.ResponseTime)
                          .AddColumn(x => x.DateTimeOfLastScan)
                          .BulkInsertOrUpdate()
                          .SetIdentityColumn(url => url.Id)
                          .MatchTargetOn(url => url.Name)
                          .Commit(connection);
                   });
        }
    }
}
