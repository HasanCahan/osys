using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using OlcuYonetimSistemi.Models.Edw;
namespace OlcuYonetimSistemi.Controllers.EDW
{
    public class TransformerType
    {
        const string ssql = @"SELECT transformerType.Id,transformerType.Name
FROM tEdw_TransformerType as  transformerType 
WHERE transformerType.Id=@Id AND transformerType.IsActive=1";
        public static DataSet ListTransformerType()
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"SELECT Id,Name 
FROM tEdw_TransformerType";
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY Id) As OrderRank FROM ({0}) AS tBase", baseSQL);
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(fetchSQL))
            {
                retval = db.ExecuteDataSet(cmd);
            }
            return retval;
        }
        public static DataTable TransformerTypeTable()
        {
            DataTable retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            DataSet ds = db.ExecuteDataSet(CommandType.Text, "SELECT Id, Name FROM tEdw_TransformerType ORDER BY Name");
            if (ds != null && ds.Tables.Count > 0) retval = ds.Tables[0];
            return retval;
        }
        public static EdwTransformerType GetTransformerType(Int32 id)
        {
            EdwTransformerType retval = new EdwTransformerType();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(ssql))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.Name = reader.GetValue<string>("Name");
                    }
                }
            }
            return retval;
        }
        public static DbHelper.DbResponse<EdwTransformerType> SaveTransformerType(EdwTransformerType item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwTransformerType>(DbHelper.DbResponseStatus.BadRequest);
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            bool EditMode = item.Id > 0;
            if (EditMode)
            {
                sql = @"UPDATE tEdw_TransformerType 
   SET [Name] =@Name ,LastActivityUserId=@LastActivityUserId
 WHERE Id=@Id";
            }
            else
            {
                sql = @"INSERT INTO tEdw_TransformerType 
           ([Name],LastActivityUserId,IsActive
           )
     VALUES
           (@Name,@LastActivityUserId,@IsActive)";
            }
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "Name", DbType.String, item.Name);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                    }
                    if (db.ExecuteNonQuery(cmd) > 0)
                    {
                        scope.Complete();
                        return new DbHelper.DbResponse<EdwTransformerType>(DbHelper.DbResponseStatus.OK, null, item);
                    }
                    else return new DbHelper.DbResponse<EdwTransformerType>(DbHelper.DbResponseStatus.NotModified);
                }
            }
        }
        public static DbHelper.DbResponse DeleteTransformerType(Int32 id)
        {
            var ttype = GetTransformerType(id);
            if (ttype == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            ttype.IsActive = false;
            var resp = SaveTransformerType(ttype);
            return new DbHelper.DbResponse(resp.StatusCode, resp.StatusDescription);
        }
    }
}