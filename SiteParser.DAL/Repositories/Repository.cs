using SiteParser.DAL.Interfaces;
using SiteParser.DAL.Models;
using SiteParser.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.Extensions;
using SqlBulkTools;
using System.Transactions;
using System.Configuration;

namespace SiteParser.DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity
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

        public virtual List<T> GetRange(Func<T, bool> condition)
        {
            List<T> result = new List<T>();

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    result = condition == null ? db.Set<T>().ToList() : db.Set<T>().Where(condition).ToList();
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

        public virtual bool TryAdd(T entity)
        {
            bool success = false;
            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Set<T>().Add(entity);
                    db.SaveChanges();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return success;
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

        public virtual void Delete(int id)
        {
            try
            {
                using (ParserContext db = new ParserContext())
                {
                    db.Set<T>().Where(x => x.Id == id).Delete();   // OR ASYNC
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public virtual void DeleteAll()
        {
            throw new NotImplementedException();
        }

        protected void BulkOperation(Action<BulkOperations, SqlConnection> action)
        {
            try
            {
                BulkOperations bulk = new BulkOperations();

                using (TransactionScope transaction = new TransactionScope())
                {
                    using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                    {
                        action(bulk, connection);
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

