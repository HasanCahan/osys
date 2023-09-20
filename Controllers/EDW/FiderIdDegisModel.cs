using Microsoft.Practices.EnterpriseLibrary.Data;
using OlcuYonetimSistemi.Models.Edw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;

namespace OlcuYonetimSistemi.Controllers.EDW
{
    public class FiderIdDegisModel
    {
        public class FiderId {
            public int FiderEskiId { get; set; }
            public int FiderYeniId { get; set; }

            public DateTime BaslangicTarih { get; set; }
            public DateTime BitisiTarih { get; set; }
        }

        public static EdwFider GetFider(Int32 id)
        {
            EdwFider retval = new EdwFider();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(
    @"
SELECT fider.ID
      ,fider.NAME as Name
      ,fider.TRANSFORMERCENTERID as TransformerCenterId 
  FROM NEW_FIDER as fider
  where fider.ID=@Id"))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.Name = reader.GetValue<string>("Name");
                        retval.TransformerCenterId = reader.GetValue<Int32>("TransformerCenterId");
                        retval.TransformerCenterName = ""; 
                    }
                }
            }
            return retval;
        }

 

        public static int? GetFiderAcmaList(Int32 id,DateTime baslangicTarih, DateTime bitisTarih)
        {
            int? count = 0;
              
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(
    @"Select COUNT(*) as COUNT from 
    (SELECT  CONVERT(date,CONCAT(Day,'.',Month,'.',Year),104)  as dateee
  FROM tEdw_FiderAcma 
  where FiderId=@Id ) as gelen where gelen.dateee BETWEEN @beginDate and @endDate"))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);

                db.AddInParameter(cmd, "beginDate", DbType.Date, baslangicTarih.ToString("yyyy-MM-dd"));
                db.AddInParameter(cmd, "endDate", DbType.Date, bitisTarih.ToString("yyyy-MM-dd")); 

                var data = db.ExecuteReader(cmd);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        count = reader.GetValue<Int32>("COUNT");
                    }
                }
            }
            return count;
        }


        public static bool updateFiderId(FiderId fiderId)
        {
            if (fiderId == null) return false;

              
            string sql = @"
                        UPDATE [dbo].[tEdw_FiderAcma]
                           SET 
                              [FiderId] = @YeniId 
                         WHERE Id IN(
                                           Select Id from 
                                                (SELECT  CONVERT(date,CONCAT(Day,'.',Month,'.',Year),104)  as datee,Id
                                                  FROM [dbo].[tEdw_FiderAcma] 
                                                 where FiderId=@EskiId ) as gelen 
                                           where gelen.datee BETWEEN @beginDate and @endDate
                                        )";
            Database db = DatabaseFactory.CreateDatabase();
            using (var connection = db.CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                     
                        using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                        {
                            db.AddInParameter(cmd, "YeniId", DbType.Int32, fiderId.FiderYeniId); 
                            db.AddInParameter(cmd, "EskiId", DbType.Int32, fiderId.FiderEskiId);

                        db.AddInParameter(cmd, "beginDate", DbType.Date, fiderId.BaslangicTarih.ToString("yyyy-MM-dd"));
                        db.AddInParameter(cmd, "endDate", DbType.Date, fiderId.BitisiTarih.ToString("yyyy-MM-dd"));


                        int result = db.ExecuteNonQuery(cmd, transaction);
                            if (result <= 0)
                            {
                                transaction.Rollback();
                                return true;
                            }
                        
                    }
                    transaction.Commit();
                    logImport(fiderId);
                }
                catch (Exception ex)
                {

                    transaction.Rollback();
                    return false;
                }
                return true;


            }
        }


        public static DbHelper.DbResponse<bool> replaceFiderId(FiderId fiderId)
        {

            if (fiderId == null) return new DbHelper.DbResponse<bool>(DbHelper.DbResponseStatus.BadRequest,"FiderId bilgileri boş olamaz.");

            int memoryFiderId = -1;
            int result1 = 0;
            int result2 = 0;
            int result3 = 0;
            string sql = @"
                        UPDATE [dbo].[tEdw_FiderAcma]
                           SET 
                              [FiderId] = @YeniId 
                         WHERE Id IN(
                                           Select Id from 
                                                (SELECT  CONVERT(date,CONCAT(Day,'.',Month,'.',Year),104)  as datee,Id
                                                  FROM [dbo].[tEdw_FiderAcma] 
                                                 where FiderId=@EskiId ) as gelen 
                                           where gelen.datee BETWEEN @beginDate and @endDate
                                        )";
            try
            { 
                Database db = DatabaseFactory.CreateDatabase();
                using (var connection = db.CreateConnection())
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    // yeni fider id bilgileri hafızaya al 
                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(cmd, "YeniId", DbType.Int32, memoryFiderId);
                        db.AddInParameter(cmd, "EskiId", DbType.Int32, fiderId.FiderYeniId);

                        db.AddInParameter(cmd, "beginDate", DbType.Date, fiderId.BaslangicTarih.ToString("yyyy-MM-dd"));
                        db.AddInParameter(cmd, "endDate", DbType.Date, fiderId.BitisiTarih.ToString("yyyy-MM-dd"));


                        result1 = db.ExecuteNonQuery(cmd, transaction);

                    }
                    //eski fiderId yeni FiderId ye taşı
                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(cmd, "YeniId", DbType.Int32, fiderId.FiderYeniId);
                        db.AddInParameter(cmd, "EskiId", DbType.Int32, fiderId.FiderEskiId);

                        db.AddInParameter(cmd, "beginDate", DbType.Date, fiderId.BaslangicTarih.ToString("yyyy-MM-dd"));
                        db.AddInParameter(cmd, "endDate", DbType.Date, fiderId.BitisiTarih.ToString("yyyy-MM-dd"));


                        result2 = db.ExecuteNonQuery(cmd, transaction);

                    }

                    //hafızadaki fiderId yi eski fiderId ye taşı
                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                    {
                        db.AddInParameter(cmd, "YeniId", DbType.Int32, fiderId.FiderEskiId);
                        db.AddInParameter(cmd, "EskiId", DbType.Int32, memoryFiderId);

                        db.AddInParameter(cmd, "beginDate", DbType.Date, fiderId.BaslangicTarih.ToString("yyyy-MM-dd"));
                        db.AddInParameter(cmd, "endDate", DbType.Date, fiderId.BitisiTarih.ToString("yyyy-MM-dd"));


                        result3 = db.ExecuteNonQuery(cmd, transaction);


                    }

                    if (result1 <= 0 && result2 <= 0 && result3 <= 0)
                    {
                        transaction.Rollback();
                        return new DbHelper.DbResponse<bool>(DbHelper.DbResponseStatus.BadRequest,"Fİder ID değişim işlemi yapılamadı.");
                    }

                    transaction.Commit();
                    logImport(fiderId);

                    return new DbHelper.DbResponse<bool>(DbHelper.DbResponseStatus.OK);


                }
            }
            catch (Exception ex)
            {
                return new DbHelper.DbResponse<bool>(DbHelper.DbResponseStatus.BadRequest,"Fider ID değişim işlemi yapılırken beklenmedik hata oluştu. HATA:"+ ex.Message);
            }

        }


        public static void logImport(FiderId fiderId)
        {
            if (fiderId == null)
                return;
            const string insrtSql = @"INSERT INTO [dbo].[log_FiderIdDegis]
           (   [EskiFiderId]
              ,[YeniFiderId]
              ,[IslemTarihi]
              ,[BaslangicTarih]
              ,[BitisTarih]
              ,[UserName]
            )
     VALUES
           (@EskiFiderId
           ,@YeniFiderId
           ,@IslemTarihi
           ,@BaslangicTarih
           ,@BitisTarih
           ,@UserName
            )";
            Database db = DatabaseFactory.CreateDatabase();
            using (var connection = db.CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {

                    var usr = User.GetUser(HttpContext.Current.User.Identity.Name);

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(insrtSql))
                    {
                        db.AddInParameter(cmd, "EskiFiderId", DbType.Int32, fiderId.FiderEskiId);
                        db.AddInParameter(cmd, "YeniFiderId", DbType.Int32, fiderId.FiderYeniId);
                        db.AddInParameter(cmd, "IslemTarihi", DbType.DateTime, DateTime.Now);
                        db.AddInParameter(cmd, "BaslangicTarih", DbType.DateTime, fiderId.BaslangicTarih);
                        db.AddInParameter(cmd, "BitisTarih", DbType.DateTime, fiderId.BitisiTarih);
                        db.AddInParameter(cmd, "UserName", DbType.String, usr.UserName.ToString()); 
                        int result = db.ExecuteNonQuery(cmd, transaction);
                        if (result <= 0)
                        {
                            transaction.Rollback();
                            return;
                        }
                    }
                     
                        transaction.Commit();
                     connection.Close();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    connection.Close();
                }
              
            }

        }


        public static DataSet ListLog(Int32 startRowIndex, Int32 maximumRows, ref Int32 totalRows)
        {
            DataSet retval = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);

            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"
                                SELECT
                                Id as Id,
                                EskiFiderId as EskiFiderId,
                                YeniFiderId as YeniFiderId,
                                UserName as UserName,
                                CONVERT(datetime,IslemTarihi,113) as IslemTarihi,
                                CONVERT(date,BaslangicTarih,103) as BaslangicTarih,
                                CONVERT(date,BitisTarih,103) as BitisTarih

                                   FROM log_FiderIdDegis ";
            if (totalRows != Int32.MinValue)
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT COUNT(*) FROM ({0}) AS tBaseCount", baseSQL
                    )))
                {
                    cmd.Parameters.AddRange(fb.Parameters);
                    totalRows = (Int32)db.ExecuteScalar(cmd);
                    cmd.Parameters.Clear();
                }
                if (startRowIndex > totalRows) startRowIndex = totalRows;
            }

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY Id) As OrderRank FROM ({0}) AS tBase", baseSQL
                );

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS FiderWithRowNumber
WHERE OrderRank >= @StartRowIndex AND OrderRank < (@StartRowIndex + @MaximumRows)
ORDER BY OrderRank", fetchSQL)))
            {
                cmd.Parameters.AddRange(fb.Parameters);
                cmd.Parameters.Add("StartRowIndex", SqlDbType.Int).Value = startRowIndex;
                cmd.Parameters.Add("MaximumRows", SqlDbType.Int).Value = maximumRows;
                retval = db.ExecuteDataSet(cmd);
            }
            return retval;
        }

    }

}