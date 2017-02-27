using System;
using System.Data;
using System.Data.SqlClient;
using SiteParser.DAL.Models;
using SiteParser.Utils;
using SiteParser.DAL.Extensions;

namespace SiteParser.DAL.Repositories
{
    public class ImportUrlsRepository : Repository<ImportUrl>
    {
        private readonly string _tableName;

        public ImportUrlsRepository()
        {
            using (ParserContext db = new ParserContext())
            {
                _tableName = db.GetTableName<ImportUrl>();
            }
        }

        public override bool TryAdd(ImportUrl importUrl)
        {
            bool result = false;

            try
            {
                using (ParserContext db = new ParserContext())
                {
                    ImportUrl r = db.Set<ImportUrl>().AddIfNotExists(importUrl, x => x.Name == importUrl.Name);
                    if (r != null)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return result;
        }

        public override void DeleteAll()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                using (SqlCommand command = new SqlCommand("TruncateImportUrls", connection)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
