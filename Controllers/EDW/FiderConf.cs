using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using OlcuYonetimSistemi.Models.Edw;
using System.Data.Common;
using System.Text;

namespace OlcuYonetimSistemi.Controllers.EDW
{
    public class FiderConf
    {

        const string singleSql = @"SELECT [FiderId]
      ,[Month]
      ,[Year]
  FROM [dbo].[tEdw_FiderConf] where FiderId IN ({0}) and Month=@Month and Year=@Year";
        public static DataSet ListFiderConf()
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"SELECT [FiderId]
      ,[Month]
      ,[Year]
  FROM [dbo].[tEdw_FiderConf]";
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY Id) As OrderRank FROM ({0}) AS tBase", baseSQL);
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(fetchSQL))
            {
                retval = db.ExecuteDataSet(cmd);
            }
            return retval;
        }
        public static DataSet GetFiderConf(int month, int year, params Int32[] fiderId)
        {
            Database db = DatabaseFactory.CreateDatabase();
            StringBuilder sb = new StringBuilder();
            foreach(var itm in fiderId)
            {
                sb.Append(sb.Length > 0 ? "," : "");
                sb.Append(itm);
            }
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(string.Format(singleSql,sb.ToString())))
            {
                db.AddInParameter(cmd, "Month", DbType.Int32, month);
                db.AddInParameter(cmd, "Year", DbType.Int32, year);
                return db.ExecuteDataSet(cmd);
            }
        }
        public static DbHelper.DbResponse<EdwFiderConf> SaveFiderConf(EdwFiderConf item, Database db, DbTransaction transaction)
        {
            if (item == null) return new DbHelper.DbResponse<EdwFiderConf>(DbHelper.DbResponseStatus.BadRequest);
            string sql = string.Empty;
            sql = @"INSERT INTO tEdw_FiderConf
           ([FiderId]
           ,Month
           ,Year)
     VALUES
           (@FiderId
           ,@Month
           ,@Year)";
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
            {
                db.AddInParameter(cmd, "FiderId", DbType.String, item.FiderId);
                db.AddInParameter(cmd, "Month", DbType.Int32, item.Month);
                db.AddInParameter(cmd, "Year", DbType.Int32, item.Year);
                if (db.ExecuteNonQuery(cmd, transaction) > 0)
                {
                    return new DbHelper.DbResponse<EdwFiderConf>(DbHelper.DbResponseStatus.OK, null, item);
                }
                else return new DbHelper.DbResponse<EdwFiderConf>(DbHelper.DbResponseStatus.NotModified);
            }
        }


    }
}