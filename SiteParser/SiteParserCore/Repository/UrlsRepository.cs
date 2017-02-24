using NLog.Fluent;
using SiteParserCore.Models;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace SiteParserCore.Repository
{
    public class UrlsRepository : Repository2<Url>
    {
        public override void AddRange(IEnumerable<Url> urls)
        {
            // base.AddRange(urls);

            try
            {
                BulkOperations bulk = new BulkOperations();

                using (TransactionScope transaction = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                    {
                        bulk.Setup<Url>()
                           .ForCollection(urls)
                           .WithTable("Urls")
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
                    }
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public override void Add(Url entity)
        {
            base.Add(entity);
        }

        public override Url Get(string name)
        {
            return base.Get(name);
        }

        public List<Url> GetUrls(int siteId, bool getExternal, int? nestingLevel = null)
        {
            List<Url> result = new List<Url>();
            Func<Url, bool> nestingCondition;
            Func<Url, bool> getExternalCondition;
            Func<Url, bool> resultCondition;

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    getExternalCondition = url => !url.IsExternal;
                    nestingCondition = nestingLevel.HasValue ?
                       (Func<Url, bool>)(url => url.SiteId == siteId && url.NestingLevel <= nestingLevel.Value) :
                                         (url => url.SiteId == siteId);
                    resultCondition = getExternal ? nestingCondition : url => nestingCondition(url) && getExternalCondition(url);
                    result = db.Urls.Where(resultCondition).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return result;
        }
    }
}
