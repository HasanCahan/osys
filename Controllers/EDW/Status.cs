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
    public class Status
    {

        const string singleSql = @"SELECT EdwStatus.Id,EdwStatus.Name
FROM tEdw_Status as  EdwStatus 
WHERE EdwStatus.Id=@Id and EdwStatus.IsActive=1";
        public static DataSet ListStatus()
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"SELECT Id,Name 
FROM tEdw_Status where IsActive=1";
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY Id) As OrderRank FROM ({0}) AS tBase", baseSQL);
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(fetchSQL))
            {
                retval = db.ExecuteDataSet(cmd);
            }
            return retval;
        }
        public static EdwStatus GetStatus(Int32 id)
        {
            EdwStatus retval = new EdwStatus();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(singleSql))
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
        public static DbHelper.DbResponse<EdwStatus> SaveStatus(EdwStatus item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwStatus>(DbHelper.DbResponseStatus.BadRequest);
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            bool EditMode = item.Id > 0;
            if (EditMode)
            {
                sql = @"UPDATE tEdw_Status
   SET [Name] =@Name
      ,LastActivityUserId=@LastActivityUserId
      ,IsActive=@IsActive
 WHERE Id=@Id";
            }
            else
            {
                sql = @"INSERT INTO tEdw_Status
           ([Name]
           ,LastActivityUserId
           ,IsActive)
     VALUES
           (@Name
           ,@LastActivityUserId
           ,@IsActive)";
            }
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "Name", DbType.String, item.Name);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.Guid, item.LastActivityUserId);
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                    }
                    if (db.ExecuteNonQuery(cmd) > 0)
                    {
                        scope.Complete();
                        return new DbHelper.DbResponse<EdwStatus>(DbHelper.DbResponseStatus.OK, null, item);
                    }
                    else return new DbHelper.DbResponse<EdwStatus>(DbHelper.DbResponseStatus.NotModified);
                }
            }
        }

        public static DbHelper.DbResponse DeleteStatus(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var transCenter = GetStatus(id);
            if (transCenter == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            transCenter.IsActive = false;
            var result = SaveStatus(transCenter);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}