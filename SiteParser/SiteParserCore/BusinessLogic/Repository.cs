using SiteParserCore.Models;
using System.Collections.Generic;
using System.Linq;
using SqlBulkTools;
using System.Data.SqlClient;
using System;
using System.Transactions;
using SiteParserCore.Helpers;
using SiteParserCore.Interfaces;
using System.Configuration;

namespace SiteParserCore.BusinessLogic
{
    public class Repository : IRepository
    {
        /////// ------------  SaveUrls with Entity Framework --------------//
        //public static void SaveUrls(List<Url> urls)
        //{
        //    try
        //    {
        //        using (ParserContext db = new ParserContext())
        //        {
        //            db.Configuration.AutoDetectChangesEnabled = false;
        //            db.Configuration.ValidateOnSaveEnabled = false;

        //            db.Urls.AddRange(urls);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //    }
        //}

        public void SaveUrls(IEnumerable<Url> urls)
        {
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

        public void SaveResources(IEnumerable<Resource> resources)
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

        public void SaveSite(Site site)           
        {
            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Sites.Add(site);
                    db.SaveChanges();
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
                       (Func<Url, bool>) (url => url.SiteId == siteId && url.NestingLevel <= nestingLevel.Value) :
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

        public Site GetSite(string url)                  
        {
            Site site = null;

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    site = db.Sites.FirstOrDefault(s => s.Name == url);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return site;
        }

        public List<Site> GetSites()                
        {
            List<Site> sites = new List<Site>();

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    sites = db.Sites.ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return sites;
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["SpDbConnection"].ConnectionString;
        }
    }
}
