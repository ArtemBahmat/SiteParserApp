using NLog.Fluent;
using SiteParserCore.Interfaces;
using SiteParserCore.Models;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace SiteParserCore.Repository
{
    public class Repository2<T> : IRepository2<T> where T : Entity
    {
        public virtual void AddRange(IEnumerable<T> entities)
        {
            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    db.Set<T>().AddRange(entities);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public virtual List<T> GetRange(IEnumerable<T> entities)
        {
            List<T> result = new List<T>();

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    result = db.Set<T>().ToList(); 
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return result;
        }

        public virtual void Add(T entity)
        {
            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Set<T>().Add(entity);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public virtual T Get(string name)
        {
            T result = null;

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    result = db.Set<T>().FirstOrDefault(item => item.Name == name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return result;
        }

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

        protected string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["SpDbConnection"].ConnectionString;
        }
    }
}

