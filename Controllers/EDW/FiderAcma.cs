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
    public class FiderAcma
    {
        [Serializable]
        public class ListFilter
        {
            public int? ExceptId { get; set; }
            public int? Day { get; set; }
            public int? Month { get; set; }
            public int? Year { get; set; }
            public int? TransformerCenterId { get; set; }
            public int? FiderId { get; set; }
            public string Cihaz { get; set; }

            public int? IlId { get; set; }

        }
        const string BaseSql = @"SELECT sh.Id
           ,sh.Count
      ,sh.Day
      ,sh.FiderId
      ,sh.IsActive
      ,sh.LastActivityUserId
      ,sh.Month
      ,sh.Year
	  ,f.NAME as FiderName
      ,f.METER_ID as Cihaz
	  ,tc.NAME as TransformerCenterName
	  ,tc.ID as TransformerCenterId
      ,tc.CITYID as IlId
  FROM dbo.tEdw_FiderAcma as sh
   left join dbo.NEW_FIDER f on f.ID=sh.FiderId
   left join dbo.NEW_TRANSFORMER_CENTER tc on tc.ID=f.TRANSFORMERCENTERID
  where   sh.IsActive=1";

        const string singleSql = @"SELECT sh.Id
           ,sh.Count
      ,sh.Day
      ,sh.FiderId
      ,sh.IsActive
      ,sh.LastActivityUserId
      ,sh.Month
      ,sh.Year
	  ,f.Name as FiderName
      ,f.METER_ID as Cihaz
	  ,tc.NAME as TransformerCenterName
	  ,tc.ID as TransformerCenterId
      ,tc.CITYID as IlId
  FROM dbo.tEdw_FiderAcma as sh
   left join dbo.NEW_FIDER f on f.ID=sh.FiderId
   left join dbo.NEW_TRANSFORMER_CENTER tc on tc.ID=f.TRANSFORMERCENTERID
  where sh.Id=@Id AND IsActive=1";
        const string updateSql = @"
UPDATE [dbo].[tEdw_FiderAcma]
   SET [FiderId] = @FiderId
      ,[Count] = @Count
      ,[LastActivityUserId] = @LastActivityUserId
      ,[IsActive] = @IsActive
      ,[Day] = @Day
      ,[Month] = @Month
      ,[Year] = @Year
 WHERE Id=@Id
";
        const string insertSql = @"INSERT INTO [dbo].[tEdw_FiderAcma]
           ([FiderId]
           ,[Count]
           ,[LastActivityUserId]
           ,[IsActive]
           ,[Day]
           ,[Month]
           ,[Year])
     VALUES
           (@FiderId
           ,@Count
           ,@LastActivityUserId
           ,@IsActive
           ,@Day
           ,@Month
           ,@Year)";
        public static DataSet ListFiderAcma(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListFiderAcma(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static Boolean Exists(EdwFiderAcma item)
        {
            var result = TryFind(item);
            if (result == null)
                return false;
            return true;
        }

        public static EdwFiderAcma TryFind(EdwFiderAcma item)
        {
            ListFilter filter = new ListFilter();
            filter.Month = item.Month;
            filter.Year = item.Year;
            filter.FiderId = item.FiderId;
            filter.Day = item.Day;
            filter.ExceptId = item.Id;
            var ds = ListFiderAcma(1, 1, filter);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;
            var row = rows[0];
            EdwFiderAcma result = new EdwFiderAcma();
            Read(row, result);
            return result;
        }
        static void Fix(ListFilter filter)
        {
            try
            {

                if (filter.FiderId.HasValue)
                {
                    GenerateNewData(filter.Year.Value, filter.Month.Value, filter.FiderId.Value);
                }
                else if (filter.TransformerCenterId.HasValue)
                {
                    Fider.ListFilter filter1 = new Fider.ListFilter();
                    filter1.TransformerCenterId = filter.TransformerCenterId;
                    var itms = Fider.ListFider(1, 1000, filter1).ToList<EdwFider>(null);
                    if (itms != null && itms.Count > 0)
                    {
                        var exists = FiderConf.GetFiderConf(filter.Month.Value, filter.Year.Value, itms.Select(s => s.Id).ToArray()).ToList<EdwFiderConf>(null);
                        if (exists != null && exists.Count > 0)
                        {
                            var ids = exists.Select(s => s.FiderId).ToList();
                            itms = itms.Where(s => !ids.Contains(s.Id)).ToList();
                        }
                        foreach (var itm in itms)
                            GenerateNewData(filter.Year.Value, filter.Month.Value, itm.Id);
                    }
                }
                else if(filter.IlId.HasValue)
                {
                    Fider.ListFilter filter1 = new Fider.ListFilter();
                    filter1.IlId = filter.IlId;
                    var itms = Fider.ListFider(1, 1000, filter1).ToList<EdwFider>(null);
                    if (itms != null && itms.Count > 0)
                    {
                        var exists = FiderConf.GetFiderConf(filter.Month.Value, filter.Year.Value, itms.Select(s => s.Id).ToArray()).ToList<EdwFiderConf>(null);
                        if (exists != null && exists.Count > 0)
                        {
                            var ids = exists.Select(s => s.FiderId).ToList(); 
                            itms = itms.Where(s => !ids.Contains(s.Id)).ToList();
                        }
                        foreach (var itm in itms)
                            GenerateNewData(filter.Year.Value, filter.Month.Value, itm.Id);
                    }

                }
            }
            catch (Exception e)
            {
            }
        }

        public static DataSet ListFiderAcma(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            var usr = User.GetUser(HttpContext.Current.User.Identity.Name);
            //if ((!filter.IlId.HasValue) || !(roles!=null && (roles.Contains("EDW")|| roles.Contains("ADMIN" ))))
            if (!filter.IlId.HasValue )
                filter.IlId = usr.IlId;
            if (filter != null)
            {
                Fix(filter);
                if (filter.Day.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Day", DbHelper.SQLFilterCompareOperator.Equal, filter.Day);
                if (filter.TransformerCenterId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerCenterId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerCenterId.Value);
                if (filter.FiderId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "FiderId", DbHelper.SQLFilterCompareOperator.Equal, filter.FiderId.Value);
                if (filter.Month.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Month", DbHelper.SQLFilterCompareOperator.Equal, filter.Month.Value);
                if (filter.Year.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Year", DbHelper.SQLFilterCompareOperator.Equal, filter.Year.Value);
                if (filter.IlId.HasValue && filter.IlId != 0) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "IlId", DbHelper.SQLFilterCompareOperator.Equal, filter.IlId.Value);
                //if (filter.CreationTime.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CAST(CreationTime AS Date)", DbHelper.SQLFilterCompareOperator.Like, filter.CreationTime.Value.Date);
                if (filter.ExceptId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Id", DbHelper.SQLFilterCompareOperator.NotEqual, filter.ExceptId.Value);
                whereClause = fb.ToString();
            }
            string baseSQL = BaseSql;
            if (totalRows != Int32.MinValue)
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT COUNT(*) FROM ({0}) AS tBaseCount{1}", baseSQL,
                    (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause)
                    )))
                {
                    cmd.Parameters.AddRange(fb.Parameters);
                    totalRows = (Int32)db.ExecuteScalar(cmd);
                    cmd.Parameters.Clear();
                }
                if (startRowIndex > totalRows) startRowIndex = totalRows;
            }
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY TransformerCenterId Asc ,FiderId ASC,Year asc, Month asc) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS FiderAcmaWithRowNumber
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
        static void Read(DataRow row, EdwFiderAcma item)
        {
            item.Id = row.Field<Int32>("Id");
            item.Count = row.Field<Int32>("Count");
            item.Day = row.Field<Int32>("Day");
            item.FiderId = row.Field<Int32>("FiderId");
            item.FiderName = row.Field<string>("FiderName");
            item.Month = row.Field<Int32>("Month");
            item.TransformerCenterName = row.Field<string>("TransformerCenterName");
            item.Year = row.Field<Int32>("Year");
        }
        static void Read(IDataReader reader, EdwFiderAcma item, out Boolean readSuccess)
        {
            if (reader.Read())
            {
                readSuccess = true;
                item.Id = reader.GetValue<Int32>("Id");
                item.Count = reader.GetValue<Int32>("Count");
                item.Day = reader.GetValue<Int32>("Day");
                item.FiderId = reader.GetValue<Int32>("FiderId");
                item.FiderName = reader.GetValue<string>("FiderName");
                item.Month = reader.GetValue<Int32>("Month");
                item.TransformerCenterName = reader.GetValue<string>("TransformerCenterName");
                item.Year = reader.GetValue<Int32>("Year");
            }
            else
                readSuccess = false;
        }
        public static Boolean Validate(EdwFiderAcma item)
        {
            var result = TryFind(item);
            if (result == null)
                return true;
            return false;
        }
        public static EdwFiderAcma GetFiderAcma(Int32 id)
        {
            EdwFiderAcma retval = new EdwFiderAcma();
            Database db = DatabaseFactory.CreateDatabase();
            Boolean succes;
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(singleSql))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    Read(reader, retval, out succes);
                }
            }
            return retval;
        }
       public static List<EdwFiderAcma> GenerateNewData(int yil, int ay, int fiderId)
        {
            var exists = FiderConf.GetFiderConf(ay, yil, fiderId).ToList<EdwFiderConf>(null);
            if (exists != null && exists.Count > 0)
                return null;
            var max = DateTime.DaysInMonth(yil, ay);
            List<EdwFiderAcma> result = new List<EdwFiderAcma>();
            EdwFider fider = Fider.GetFider(fiderId);
            var user = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            for (int i = 1; i <= max; i++)
            {
                var tmp = new EdwFiderAcma { Day = i, FiderId = fider.Id, IsActive = true, Month = ay, Year = yil, LastActivityUserId = user };
                result.Add(tmp);
            }
            Import(result);
            return result;
        }
        public static void Import(List<EdwFiderAcma> items)
        {
            if (items == null || items.Count == 0)
                return;
            const string insrtSql = @"INSERT INTO [dbo].[tEdw_FiderAcma]
           ([FiderId]
           ,[Count]
           ,[LastActivityUserId]
           ,[IsActive]
           ,[Day]
           ,[Month]
           ,[Year])
     VALUES
           (@FiderId
           ,@Count
           ,@LastActivityUserId
           ,@IsActive
           ,@Day
           ,@Month
           ,@Year)";
            Database db = DatabaseFactory.CreateDatabase();
            using (var connection = db.CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    var refItem = items.FirstOrDefault();
                    var res1 = FiderConf.SaveFiderConf(new EdwFiderConf { FiderId = refItem.FiderId, Month = refItem.Month, Year = refItem.Year }, db, transaction);
                    if (res1.StatusCode == DbHelper.DbResponseStatus.OK)
                    {

                        foreach (var itm in items)
                        {
                            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(insrtSql))
                            {
                                db.AddInParameter(cmd, "Count", DbType.Int32, itm.Count);
                                db.AddInParameter(cmd, "Day", DbType.Int32, itm.Day);
                                db.AddInParameter(cmd, "FiderId", DbType.Int32, itm.FiderId);
                                db.AddInParameter(cmd, "IsActive", DbType.Boolean, itm.IsActive);
                                db.AddInParameter(cmd, "LastActivityUserId", DbType.String, itm.LastActivityUserId.ToString());
                                db.AddInParameter(cmd, "Month", DbType.Int16, itm.Month);
                                db.AddInParameter(cmd, "Year", DbType.Int32, itm.Year);
                                int result = db.ExecuteNonQuery(cmd, transaction);
                                if (result <= 0)
                                {
                                    transaction.Rollback();
                                    return;
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }
        public static DbHelper.DbResponse<EdwFiderAcma> Update(params EdwFiderAcma[] items)
        {
            if (items == null) return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.BadRequest);

            const string sql = @"
UPDATE [dbo].[tEdw_FiderAcma]
   SET 
      [Count] = @Count
      ,[LastActivityUserId] = @LastActivityUserId
 WHERE Id=@Id";
            Database db = DatabaseFactory.CreateDatabase();
            using (var connection = db.CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    foreach (var item in items)
                    {
                        item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
                        using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                        {
                            db.AddInParameter(cmd, "Count", DbType.Int32, item.Count);
                            db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                            db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                            int result = db.ExecuteNonQuery(cmd, transaction);
                            if (result <= 0)
                            {
                                transaction.Rollback();
                                return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.Unknown);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {

                    transaction.Rollback();
                }
                return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.OK, null, items.FirstOrDefault());
            }



        }
        public static DbHelper.DbResponse<EdwFiderAcma> SaveFiderAcma(EdwFiderAcma item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.BadRequest);
            if (!Validate(item))
            {
                return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.Conflict, "Tekrar edilmemesi gereken alan ihlal edildi: " + item.ToString());
            }
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            Database db = DatabaseFactory.CreateDatabase();
            if (item.Id > 0)
            {
                sql = @"
UPDATE [dbo].[tEdw_FiderAcma]
   SET [FiderId] = @FiderId
      ,[Count] = @Coun
      ,[LastActivityUserId] = @LastActivityUserId
      ,[IsActive] = @IsActive
      ,[Day] = @Day
      ,[Month] = @Month
      ,[Year] = @Year
 WHERE Id=@Id
";
            }
            else
            {
                sql = @"INSERT INTO [dbo].[tEdw_FiderAcma]
           ([FiderId]
           ,[Count]
           ,[LastActivityUserId]
           ,[IsActive]
           ,[Day]
           ,[Month]
           ,[Year])
     VALUES
           (@FiderId
           ,@Count
           ,@LastActivityUserId
           ,@IsActive
           ,@Day
           ,@Month
           ,@Year) SET @Identity=SCOPE_IDENTITY()";
            }
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    if (item.Id > 0)
                        db.AddInParameter(cmd, "Id", DbType.String, item.Id);
                    else
                        db.AddOutParameter(cmd, "Identity", DbType.Int32, 0);
                    db.AddInParameter(cmd, "Count", DbType.Int32, item.Count);
                    db.AddInParameter(cmd, "Day", DbType.Int32, item.Day);
                    db.AddInParameter(cmd, "FiderId", DbType.Int32, item.FiderId);
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    db.AddInParameter(cmd, "Month", DbType.String, item.Month);
                    db.AddInParameter(cmd, "Year", DbType.Int32, item.Year);
                    var res = db.ExecuteNonQuery(cmd);
                    if (res > 0)
                    {
                        if (item.Id == 0)
                            item.Id = (Int32)cmd.Parameters["@Identity"].Value;
                        scope.Complete();
                        return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.OK, null, item);
                    }
                    else return new DbHelper.DbResponse<EdwFiderAcma>(DbHelper.DbResponseStatus.NotModified);
                }
            }
        }

        public static DbHelper.DbResponse DeleteFiderAcma(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var stH = GetFiderAcma(id);
            if (stH == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            stH.IsActive = false;
            var result = SaveFiderAcma(stH);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}