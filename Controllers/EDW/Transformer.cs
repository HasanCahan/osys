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
    public class Transformer
    {
        [Serializable]
        public class ListFilter
        {
            public Nullable<Int32> PmumNumber { get; set; }
            public string Name { get; set; }
            //public int? TransformerTypeId { get; set; }
            public int? InavitasIdType { get; set; }
            public int? TransformerCenterId { get; set; }
            public int? ReceivedEnergyId { get; set; }
            public int? DeliveredEnergyId { get; set; }
            public int? OsosNumber { get; set; }
        }
        public static DataSet ListTransformer(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListTransformer(startRowIndex, maximumRows, filter, ref tmp);
        }
        public static DataSet ListTransformer(Int32 startRowIndex, Int32 maximumRows, string whereClause, ref Int32 totalRows, DbHelper.SQLFilterBuilder fb)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"
SELECT trans.Id
      ,trans.Name
      ,trans.TransformerCenterId
      ,trans.PmumNumber
      ,trans.ReceivedEnergyId
      ,trans.DeliveredEnergyId
      ,trans.IsActive
      ,trans.Comment
	  ,trans.IdType as InavitasIdType
	  ,tc.Name as TransformerCenterName
      ,OsosNumber
  FROM tEdw_Transformer as trans
  LEFT join tEdw_TransformerCenter tc on tc.Id=trans.TransformerCenterId
  where trans.IsActive=1";
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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY Id) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS transformerWithRowNumber
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
        public static DataSet ListTransformer(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.PmumNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "PmumNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.PmumNumber.Value);
                if (!string.IsNullOrEmpty(filter.Name)) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Name", DbHelper.SQLFilterCompareOperator.Like, "%" + filter.Name + "%");
                if (filter.InavitasIdType.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "InavitasIdType", DbHelper.SQLFilterCompareOperator.Equal, filter.InavitasIdType);
                if (filter.TransformerCenterId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerCenterId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerCenterId);
                if (filter.ReceivedEnergyId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "ReceivedEnergyId", DbHelper.SQLFilterCompareOperator.Equal, filter.ReceivedEnergyId);
                if (filter.DeliveredEnergyId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "DeliveredEnergyId", DbHelper.SQLFilterCompareOperator.Equal, filter.DeliveredEnergyId);
                if (filter.OsosNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "OsosNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.OsosNumber);
                whereClause = fb.ToString();
            }
            return ListTransformer(startRowIndex, maximumRows, whereClause, ref totalRows, fb);
        }
        public static DataSet ListTransformer2(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.OsosNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "OsosNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.OsosNumber);
                if (filter.PmumNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "PmumNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.PmumNumber);
                if (filter.ReceivedEnergyId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "ReceivedEnergyId", DbHelper.SQLFilterCompareOperator.Equal, filter.ReceivedEnergyId);
                if (filter.DeliveredEnergyId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "DeliveredEnergyId", DbHelper.SQLFilterCompareOperator.Equal, filter.DeliveredEnergyId);
                whereClause = fb.ToString();
            }
            return ListTransformer(startRowIndex, maximumRows, whereClause, ref totalRows, fb);
        }
        public static EdwTransformer GetTransformer(Int32 id)
        {
            EdwTransformer retval = new EdwTransformer();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(
@"
SELECT trans.Id
      ,trans.Name
      ,trans.TransformerCenterId
      ,trans.PmumNumber
      ,trans.ReceivedEnergyId
      ,trans.DeliveredEnergyId
      ,trans.IsActive
      ,trans.Comment
	  ,trans.IdType as InavitasIdType
	  ,tc.Name as TransformerCenterName
      ,OsosNumber
  FROM tEdw_Transformer as trans
  LEFT join tEdw_TransformerCenter tc on tc.Id=trans.TransformerCenterId
  where trans.IsActive=1 and trans.Id=@Id"))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.DeliveredEnergyId = reader.GetValue<Int32>("DeliveredEnergyId");
                        retval.ReceivedEnergyId = reader.GetValue<Int32>("ReceivedEnergyId");
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.Name = reader.GetValue<string>("Name");
                        retval.PmumNumber = reader.GetValue<Int32>("PmumNumber");
                        retval.TransformerCenterId = reader.GetValue<Int32>("TransformerCenterId");
                        retval.TransformerCenterName = reader.GetValue<string>("TransformerCenterName");
                        retval.Comment = reader.GetValue<string>("Comment");
                        retval.InavitasIdType = reader.GetValue<Int32?>("InavitasIdType");
                        retval.OsosNumber = reader.GetValue<int?>("OsosNumber");
                    }
                }
            }
            return retval;
        }
        public static Boolean Validate(EdwTransformer item)
        {
            var result = TryFind(item);
            if (result == null)
                return true;
            return false;
        }
        static EdwTransformer Get(ListFilter filter, EdwTransformer item)
        {
            int tmp = 0;
            var ds = ListTransformer2(1, 5, filter, ref tmp);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;

            foreach (DataRow row in rows)
            {
                EdwTransformer result = new EdwTransformer();
                Read(row, result);
                if (item.Id == 0)
                    return result;
                if (item.Id != result.Id)
                    return result;
            }
            //var row = rows[0];        
            return null;
        }
        public static EdwTransformer TryFind(EdwTransformer item)
        {
            ListFilter filter = new ListFilter();
            EdwTransformer result = null;
            Boolean hasCriteria = false;
            if (item.PmumNumber != 0)
            {
                filter.PmumNumber = item.PmumNumber;
                hasCriteria = true;
            }
            if (result != null)
                return result;
            if (filter.ReceivedEnergyId != null && filter.ReceivedEnergyId != 0)
            {
                filter.ReceivedEnergyId = item.ReceivedEnergyId;
                hasCriteria = true;
            }
            if (filter.DeliveredEnergyId != null && filter.DeliveredEnergyId != 0)
            {
                filter.DeliveredEnergyId = item.DeliveredEnergyId;
                hasCriteria = true;
            }
            if (filter.OsosNumber != null && filter.OsosNumber != 0)
            {
                filter.OsosNumber = item.OsosNumber;
                hasCriteria = true;
            }
            if (hasCriteria)
                result = Get(filter, item);
            return result;

        }
        static void Read(DataRow row, EdwTransformer item)
        {
            item.DeliveredEnergyId = row.Field<Int32>("DeliveredEnergyId");
            item.ReceivedEnergyId = row.Field<Int32>("ReceivedEnergyId");
            item.Id = row.Field<Int32>("Id");
            item.Name = row.Field<string>("Name");
            item.PmumNumber = row.Field<Int32>("PmumNumber");
            item.TransformerCenterId = row.Field<Int32>("TransformerCenterId");
            item.TransformerCenterName = row.Field<string>("TransformerCenterName");
            item.Comment = row.Field<string>("Comment");
            item.InavitasIdType = row.Field<Int32?>("InavitasIdType");
            item.OsosNumber = row.Field<int?>("OsosNumber");
        }
       
        public static DbHelper.DbResponse<EdwTransformer> SaveTransformer(EdwTransformer item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwTransformer>(DbHelper.DbResponseStatus.BadRequest);
            if (!Validate(item))
            {
                return new DbHelper.DbResponse<EdwTransformer>(DbHelper.DbResponseStatus.Conflict, "Tekrar edilmemesi gereken alan ihlal edildi: " + item.ToString());
            }
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            Database db = DatabaseFactory.CreateDatabase();
            if (item.Id > 0)
            {
                sql = @"UPDATE dbo.tEdw_Transformer
   SET Name =@Name
      ,TransformerCenterId = @TransformerCenterId
      ,PmumNumber = @PmumNumber
      ,ReceivedEnergyId = @ReceivedEnergyId
      ,DeliveredEnergyId = @DeliveredEnergyId
      ,IsActive = @IsActive
      ,Comment=@Comment
      ,LastActivityUserId=@LastActivityUserId
	  ,IdType=@InavitasIdType
      ,OsosNumber=@OsosNumber
 WHERE Id=@Id";
            }
            else
            {
                sql = @"INSERT INTO tEdw_Transformer
           (Name
           ,TransformerCenterId
           ,PmumNumber
           ,ReceivedEnergyId
           ,DeliveredEnergyId
           ,IsActive
           ,Comment
           ,LastActivityUserId
		   ,IdType
           ,OsosNumber)
     VALUES
           (@Name
           ,@TransformerCenterId
           ,@PmumNumber
           ,@ReceivedEnergyId
           ,@DeliveredEnergyId
           ,@IsActive
           ,@Comment
           ,@LastActivityUserId
		   ,@InavitasIdType
            ,@OsosNumber) SET @Identity=SCOPE_IDENTITY()";
            }
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    if (item.Id > 0)
                        db.AddInParameter(cmd, "Id", DbType.String, item.Id);
                    else
                        db.AddOutParameter(cmd, "Identity", DbType.Int32, 0);
                    db.AddInParameter(cmd, "Name", DbType.String, item.Name);
                    db.AddInParameter(cmd, "TransformerCenterId", DbType.Int32, item.TransformerCenterId);
                    db.AddInParameter(cmd, "PmumNumber", DbType.Int32, item.PmumNumber);
                    db.AddInParameter(cmd, "ReceivedEnergyId", DbType.Int32, item.ReceivedEnergyId);
                    db.AddInParameter(cmd, "DeliveredEnergyId", DbType.Int32, item.DeliveredEnergyId);
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    db.AddInParameter(cmd, "Comment", DbType.String, item.Comment);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    db.AddInParameter(cmd, "OsosNumber", DbType.Int32, item.OsosNumber);
                    db.AddInParameter(cmd, "@InavitasIdType", DbType.Int32, item.InavitasIdType);
                    var res = db.ExecuteNonQuery(cmd);
                    if (res > 0)
                    {
                        if (item.Id == 0)
                            item.Id = (Int32)cmd.Parameters["@Identity"].Value;
                        scope.Complete();
                        return new DbHelper.DbResponse<EdwTransformer>(DbHelper.DbResponseStatus.OK, null, item);
                    }
                    else return new DbHelper.DbResponse<EdwTransformer>(DbHelper.DbResponseStatus.NotModified);
                }
            }
        }

        public static DbHelper.DbResponse DeleteTransformer(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var transCenter = GetTransformer(id);
            if (transCenter == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            transCenter.IsActive = false;
            var result = SaveTransformer(transCenter);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}