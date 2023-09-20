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
    public class StatusHistory
    {
        [Serializable]
        public class ListFilter
        {
            public Nullable<int> StatusId { get; set; }
            public Nullable<int> TransformerCenterId { get; set; }
            public Nullable<int> TransformerId { get; set; }
            public Nullable<int> OsosNumber { get; set; }
            public Nullable<int> EdwConsumptionTypeId { get; set; }
            public Nullable<int> BaraConsumptionTypeId { get; set; }
            public Nullable<int> FiderConsumptionTypeId { get; set; }
            public Nullable<int> ExceptId { get; set; }
            public DateTime? CreationTime { get; set; }
            public Nullable<Boolean> HasEnded { get; set; }
        }
        const string BaseSql = @"SELECT sh.Id
           ,sh.CreationTime
      ,sh.StatusId
      ,sh.CityId
      ,sh.TownId
      ,sh.PmumId
      ,sh.EndDate
      ,sh.ReceivedEnergyId
      ,sh.DeliveredEnergyId
      ,sh.TransformerCenterName
      ,sh.StatusDescription
      ,sh.TransformerId
      ,sh.TransformerCenterId
      ,sh.OsosNumber
      ,sh.LastActivityUserId
      ,sh.TransformerCenterEdwNumber
	  ,sh.TransformerName	 
	  ,sh.EdwConsumptionTypeId
      ,sh.BaraConsumptionTypeId
      ,sh.FiderConsumptionTypeId
      ,sh.Comment
	  ,ct.CityName
	  ,tt.TownName
  FROM dbo.tEdw_StatusHistory as sh
  left join tCity ct on ct.CityId=sh.Id
  left join tTown tt on sh.TownId=tt.TownId  
  where   IsActive=1";
        const string baseSqlGrouping = @"select  sh.Id
      ,sh.CreationTime
      ,sh.StatusId
      ,sh.CityId
      ,sh.TownId
      ,sh.PmumId
      ,sh.EndDate
      ,sh.ReceivedEnergyId
      ,sh.DeliveredEnergyId
      ,sh.TransformerCenterName
      ,sh.StatusDescription
      ,sh.TransformerId
      ,sh.TransformerCenterId
      ,sh.OsosNumber
      ,sh.LastActivityUserId
      ,sh.TransformerCenterEdwNumber
	  ,sh.TransformerName
	  ,sh.EdwConsumptionTypeId
      ,sh.BaraConsumptionTypeId
      ,sh.FiderConsumptionTypeId
      ,sh.Comment
	  ,ct.CityName 
	  ,tt.TownName 
	    from (select Id
      ,CreationTime
      ,StatusId
      ,CityId
      ,TownId
      ,PmumId
      ,EndDate
      ,ReceivedEnergyId
      ,DeliveredEnergyId
      ,TransformerCenterName
      ,StatusDescription
      ,TransformerId
      ,TransformerCenterId
      ,OsosNumber
      ,LastActivityUserId
      ,TransformerCenterEdwNumber
	  ,TransformerName 
	  ,EdwConsumptionTypeId
      ,BaraConsumptionTypeId
      ,FiderConsumptionTypeId
      ,Comment
	  ,ROW_NUMBER() OVER 
	  (PARTITION BY TransformerId, TransformerCenterId,EdwConsumptionTypeId,BaraConsumptionTypeId,FiderConsumptionTypeId order by Id Desc) as Row	   
  FROM dbo.tEdw_StatusHistory   where IsActive=1 ) sh
  left join tCity ct on ct.CityId=sh.Id
  left join tTown tt on sh.TownId=tt.TownId
    where sh.Row=1
   ";
        const string singleSql = @"SELECT sh.Id
           ,sh.CreationTime
      ,sh.StatusId
      ,sh.CityId
      ,sh.TownId
      ,sh.PmumId
      ,sh.EndDate
      ,sh.ReceivedEnergyId
      ,sh.DeliveredEnergyId
      ,sh.TransformerCenterName
      ,sh.StatusDescription
      ,sh.TransformerId
      ,sh.TransformerCenterId
      ,sh.OsosNumber
      ,sh.LastActivityUserId
      ,sh.TransformerCenterEdwNumber
	  ,sh.TransformerName	 
	  ,sh.EdwConsumptionTypeId
      ,sh.BaraConsumptionTypeId
      ,sh.FiderConsumptionTypeId
      ,sh.Comment
	  ,ct.CityName
	  ,tt.TownName
  FROM dbo.tEdw_StatusHistory as sh
  left join tCity ct on ct.CityId=sh.Id
  left join tTown tt on sh.TownId=tt.TownId
  where sh.Id=@Id AND IsActive=1";
        const string updateSql = @"
UPDATE dbo.tEdw_StatusHistory
SET CreationTime = @CreationTime,
    StatusId = @StatusId,
    OsosNumber = @OsosNumber,
    TransformerId = @TransformerId,
    LastActivityUserId = @LastActivityUserId,
    EdwConsumptionTypeId = @EdwConsumptionTypeId,
    EdwConsumptionTypeDescription = @EdwConsumptionTypeDescription,
    BaraConsumptionTypeId = @BaraConsumptionTypeId,
    BaraConsumptionTypeDescription = @BaraConsumptionTypeDescription,
    FiderConsumptionTypeId = @FiderConsumptionTypeId,
    FiderConsumptionTypeDescription = @FiderConsumptionTypeDescription,
    Comment=@Comment,
    IsActive = @IsActive
 WHERE Id=@Id
";
        const string insertSql = @"INSERT INTO tEdw_StatusHistory
           (CreationTime
           ,StatusId
           ,TransformerId
           ,OsosNumber
           ,LastActivityUserId
		   ,EdwConsumptionTypeId
           ,BaraConsumptionTypeId
           ,FiderConsumptionTypeId
		   ,EdwConsumptionTypeDescription
           ,BaraConsumptionTypeDescription
           ,FiderConsumptionTypeDescription
           ,Comment
           ,IsActive)
     VALUES
           (@CreationTime
           ,@StatusId
           ,@TransformerId
           ,@OsosNumber
           ,@LastActivityUserId
		   ,@EdwConsumptionTypeId
           ,@BaraConsumptionTypeId
           ,@FiderConsumptionTypeId
		   ,@EdwConsumptionTypeDescription
           ,@BaraConsumptionTypeDescription
           ,@FiderConsumptionTypeDescription
           ,@Comment
           ,@IsActive)";
        public static DataSet ListStatusHistory(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListStatusHistory(startRowIndex, maximumRows, filter, ref tmp);
        }
      
        public static Boolean Exists(EdwStatusHistory item)
        {
            var result = TryFind(item);
            if (result == null)
                return false;
            return true;
        }

        public static EdwStatusHistory TryFind(EdwStatusHistory item)
        {
            ListFilter filter = new ListFilter();
            filter.TransformerId = item.TransformerId;
            filter.EdwConsumptionTypeId = item.EdwConsumptionTypeId;
            filter.BaraConsumptionTypeId = item.BaraConsumptionTypeId;
            filter.FiderConsumptionTypeId = item.FiderConsumptionTypeId;
            filter.StatusId = item.StatusId;
            filter.ExceptId = item.Id;
            filter.HasEnded = false;
            var ds = ListStatusHistory(1, 1, filter);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;
            var row = rows[0];
            EdwStatusHistory result = new EdwStatusHistory();
            Read(row, result);
            return result;
        }
        public static DataSet ListStatusHistory(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.StatusId != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "StatusId", DbHelper.SQLFilterCompareOperator.Equal, filter.StatusId);
                if (filter.TransformerCenterId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerCenterId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerCenterId.Value);
                if (filter.TransformerId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerId.Value);
                if (filter.OsosNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "OsosNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.OsosNumber.Value);
                if (filter.EdwConsumptionTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EdwConsumptionTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.EdwConsumptionTypeId.Value);
                if (filter.BaraConsumptionTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "BaraConsumptionTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.BaraConsumptionTypeId.Value);
                if (filter.FiderConsumptionTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "FiderConsumptionTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.FiderConsumptionTypeId.Value);
                //if (filter.CreationTime.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CAST(CreationTime AS Date)", DbHelper.SQLFilterCompareOperator.Like, filter.CreationTime.Value.Date);
                if (filter.ExceptId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Id", DbHelper.SQLFilterCompareOperator.NotEqual, filter.ExceptId.Value);
                if (filter.HasEnded.HasValue)
                    fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EndDate", DbHelper.SQLFilterCompareOperator.IsNull);
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
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY CreationTime Desc ,StatusId ASC,TransformerCenterName asc, TransformerName asc) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS statusHistoryWithRowNumber
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
        static void Read(DataRow row, EdwStatusHistory item)
        {
            item.Id = row.Field<Int32>("Id");
            item.CityId = row.Field<Int32?>("CityId");
            item.CreationTime = row.Field<DateTime>("CreationTime");
            item.EndDate = row.Field<DateTime?>("EndDate");
            item.DeliveredEnergyId = row.Field<Int32?>("DeliveredEnergyId");
            item.OsosNumber = row.Field<Int32?>("OsosNumber");
            item.PmumId = row.Field<Int32>("PmumId");
            item.ReceivedEnergyId = row.Field<Int32?>("ReceivedEnergyId");
            item.StatusDescription = row.Field<string>("StatusDescription");
            item.StatusId = row.Field<Int32>("StatusId");
            item.TownId = row.Field<Int32?>("TownId");
            item.TransformerCenterId = row.Field<Int32>("TransformerCenterId");
            item.TransformerCenterName = row.Field<string>("TransformerCenterName");
            item.TransformerId = row.Field<Int32>("TransformerId");
            //item.TransformerTypeDescription = row.Field<string>("TransformerTypeDescription");
            //item.TransformerTypeId = row.Field<Int32?>("TransformerTypeId");
            item.TransformerCenterEdwNumber = row.Field<Int32?>("TransformerCenterEdwNumber");
            item.TransformerName = row.Field<string>("TransformerName");
            item.EdwConsumptionTypeId = row.Field<int?>("EdwConsumptionTypeId");
            item.BaraConsumptionTypeId = row.Field<int?>("BaraConsumptionTypeId");
            item.FiderConsumptionTypeId = row.Field<int?>("FiderConsumptionTypeId");
            item.Comment = row.Field<string>("Comment");
        }
        static void Read(IDataReader reader, EdwStatusHistory item, out Boolean readSuccess)
        {
            if (reader.Read())
            {
                readSuccess = true;
                item.Id = reader.GetValue<Int32>("Id");
                item.CityId = reader.GetValue<Int32>("CityId");
                item.CreationTime = reader.GetValue<DateTime>("CreationTime");
                item.EndDate = reader.GetValue<DateTime?>("EndDate");
                item.DeliveredEnergyId = reader.GetValue<Int32>("DeliveredEnergyId");
                item.OsosNumber = reader.GetValue<Int32?>("OsosNumber");
                item.PmumId = reader.GetValue<Int32>("PmumId");
                item.ReceivedEnergyId = reader.GetValue<Int32>("ReceivedEnergyId");
                item.StatusDescription = reader.GetValue<string>("StatusDescription");
                item.StatusId = reader.GetValue<Int32>("StatusId");
                item.TownId = reader.GetValue<Int32>("TownId");
                item.TransformerCenterId = reader.GetValue<Int32>("TransformerCenterId");
                item.TransformerCenterName = reader.GetValue<string>("TransformerCenterName");
                item.TransformerId = reader.GetValue<Int32>("TransformerId");
                //item.TransformerTypeDescription = reader.GetValue<string>("TransformerTypeDescription");
                //item.TransformerTypeId = reader.GetValue<Int32?>("TransformerTypeId");
                item.TransformerCenterEdwNumber = reader.GetValue<Int32?>("TransformerCenterEdwNumber");
                item.TransformerName = reader.GetValue<string>("TransformerName");
                item.EdwConsumptionTypeId = reader.GetValue<int?>("EdwConsumptionTypeId");
                item.BaraConsumptionTypeId = reader.GetValue<int?>("BaraConsumptionTypeId");
                item.FiderConsumptionTypeId = reader.GetValue<int?>("FiderConsumptionTypeId");
                item.Comment = reader.GetValue<string>("Comment");
            }
            else
                readSuccess = false;
        }
        public static EdwStatusHistory GetStatusHistory(Int32 id)
        {
            EdwStatusHistory retval = new EdwStatusHistory();
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
        public static DbHelper.DbResponse<EdwStatusHistory> SaveStatusHistory(EdwStatusHistory item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.BadRequest);
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            var exists = Exists(item);
            if (exists)
                return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.NotModified, "Bu Statütü ilgili Trafo-Trafo Merkezi ile zaten ilişkili.");
            if(item.StatusId==5)//Gömülü santral ise tüketim verileri nullanacak
            {
                item.BaraConsumptionTypeId = null;
                item.EdwConsumptionTypeId = null;
                item.FiderConsumptionTypeId = null;
            }
            string sql = string.Empty;
            bool EditMode = item.Id > 0;
            if (EditMode)
            {
                sql = updateSql;
            }
            else
            {
                sql = insertSql;
            }
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "CreationTime", DbType.DateTime, item.CreationTime);
                    db.AddInParameter(cmd, "StatusId", DbType.Int32, item.StatusId);
                    db.AddInParameter(cmd, "TransformerId", DbType.Int32, item.TransformerId);
                    db.AddInParameter(cmd, "TransformerCenterId", DbType.Int32, item.TransformerCenterId);
                    db.AddInParameter(cmd, "OsosNumber", DbType.Int32, item.OsosNumber);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    db.AddInParameter(cmd, "EdwConsumptionTypeId", DbType.Int32, item.EdwConsumptionTypeId);
                    db.AddInParameter(cmd, "BaraConsumptionTypeId", DbType.Int32, item.BaraConsumptionTypeId);
                    db.AddInParameter(cmd, "FiderConsumptionTypeId", DbType.Int32, item.FiderConsumptionTypeId);
                    db.AddInParameter(cmd, "EdwConsumptionTypeDescription", DbType.String, item.EdwConsumptionTypeDescription);
                    db.AddInParameter(cmd, "BaraConsumptionTypeDescription", DbType.String, item.BaraConsumptionTypeDescription);
                    db.AddInParameter(cmd, "FiderConsumptionTypeDescription", DbType.String, item.FiderConsumptionTypeDescription);
                    db.AddInParameter(cmd, "Comment", DbType.String, item.Comment);

                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    if (!EditMode)
                    {
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwStatusHistory>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteStatusHistory(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var stH = GetStatusHistory(id);
            if (stH == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            stH.IsActive = false;
            var result = SaveStatusHistory(stH);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}