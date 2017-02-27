using SiteParser.DAL.Extensions;
using SiteParser.DAL.Models;
using SiteParser.DAL.Repositories;
using System.Collections.Generic;

namespace SiteParser.Core.Repository
{
    public class ResourcesRepository : Repository<Resource>
    {
        private readonly string _tableName;

        public ResourcesRepository()
        {
            using (ParserContext db = new ParserContext())
            {
                _tableName = db.GetTableName<Url>();
            }
        }

        public override void AddRange(IEnumerable<Resource> resources)
        {
            BulkOperation(
               (bulk, connection) =>
               {
                   bulk.Setup<Resource>()
                        .ForCollection(resources)
                        .WithTable(_tableName)
                        .AddColumn(x => x.Name)
                        .AddColumn(x => x.Type)
                        .AddColumn(x => x.ParentName)
                        .AddColumn(x => x.ParentSiteId)
                        .BulkInsert()
                        .SetIdentityColumn(res => res.Id)
                        .Commit(connection);
               });
        }

        public override void Delete(int siteId)          
        {
            BulkOperation(
               (bulk, connection) =>
               {
                   bulk.Setup<Resource>()
                        .ForDeleteQuery()
                        .WithTable(_tableName)
                        .Delete()
                        .Where(x => x.ParentSiteId == siteId)
                        .Commit(connection);
               });
        }
    }
}
