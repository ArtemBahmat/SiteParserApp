using NLog.Fluent;
using SiteParserCore.Models;
using SiteParserCore.Repository;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SiteParserCore.Repository
{
    public class ResourcesRepository : Repository2<Resource>
    {
        public override void AddRange(IEnumerable<Resource> resources)
        {
            try
            {
                BulkOperations bulk = new BulkOperations();

                using (TransactionScope transaction = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                    {
                        bulk.Setup<Resource>()
                           .ForCollection(resources)
                           .WithTable("Resources")
                           .AddColumn(x => x.Name)
                           .AddColumn(x => x.Type)
                           .AddColumn(x => x.ParentName)
                           .AddColumn(x => x.ParentSiteId)
                           .BulkInsert()
                           .SetIdentityColumn(res => res.Id)
                           .Commit(connection);
                    }
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public void DeleteResources(int siteId)
        {
            try
            {
                BulkOperations bulk = new BulkOperations();

                using (TransactionScope transaction = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                    {
                        bulk.Setup<Resource>()
                        .ForDeleteQuery()
                        .WithTable("Resources")
                        .Delete()
                        .Where(x => x.ParentSiteId == siteId)
                        .Commit(connection);
                    }
                    transaction.Complete();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
