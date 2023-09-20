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
    public class Fider
    {
        [Serializable]
        public class ListFilter
        {
            public string Name { get; set; }
            public int? TransformerCenterId { get; set; }
            public int? IlId { get; set; }
        }
        public static DataSet ListFider(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListFider(startRowIndex, maximumRows, filter, ref tmp);
        }
        public static DataSet ListFider(Int32 startRowIndex, Int32 maximumRows, string whereClause, ref Int32 totalRows, DbHelper.SQLFilterBuilder fb)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"
SELECT fider.ID as Id
      ,fider.NAME as Name
      ,fider.TRANSFORMERCENTERID as TransformerCenterId  
	  ,tc.NAME as TransformerCenterName
      ,tc.CITYID as IlId
  FROM NEW_FIDER as fider
  LEFT join NEW_TRANSFORMER_CENTER tc on tc.ID=fider.TRANSFORMERCENTERID";
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
        public static DataSet ListFider(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Name)) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Name", DbHelper.SQLFilterCompareOperator.Like, "%" + filter.Name + "%");
                if (filter.TransformerCenterId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerCenterId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerCenterId);
                if (filter.IlId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "IlId", DbHelper.SQLFilterCompareOperator.Equal, filter.IlId);
                whereClause = fb.ToString();
            }
            return ListFider(startRowIndex, maximumRows, whereClause, ref totalRows, fb);
        }
     
        public static EdwFider GetFider(Int32 id)
        {
            EdwFider retval = new EdwFider();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(
@"
SELECT fider.ID as Id
      ,fider.NAME as Name
      ,fider.TRANSFORMERCENTERID as TransformerCenterId 
	  ,tc.Name as TransformerCenterName
  FROM NEW_FIDER as fider
  LEFT join NEW_TRANSFORMER_CENTER tc on tc.ID=fider.TRANSFORMERCENTERID
  where  fider.Id=@Id"))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.Name = reader.GetValue<string>("Name");
                        retval.TransformerCenterId = reader.GetValue<Int32>("TransformerCenterId");
                        retval.TransformerCenterName = reader.GetValue<string>("TransformerCenterName"); 
                    }
                }
            }
            return retval;
        }
        public static Boolean Validate(EdwFider item)
        {
            var result = TryFind(item);
            if (result == null)
                return true;
            return false;
        }
        static EdwFider Get(ListFilter filter, EdwFider item)
        {
            int tmp = 0;
            var ds = ListFider(1, 5, filter, ref tmp);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;

            foreach (DataRow row in rows)
            {
                EdwFider result = new EdwFider();
                Read(row, result);
                if (item.Id == 0)
                    return result;
                if (item.Id != result.Id)
                    return result;
            }
            //var row = rows[0];        
            return null;
        }
        public static EdwFider TryFind(EdwFider item)
        {
            ListFilter filter = new ListFilter();
            EdwFider result = null;
            Boolean hasCriteria = false;
            if (result != null)
                return result;
            if (hasCriteria)
                result = Get(filter, item);
            return result;

        }
        static void Read(DataRow row, EdwFider item)
        {
            item.Id = row.Field<Int32>("Id");
            item.Name = row.Field<string>("Name");
            item.TransformerCenterId = row.Field<Int32>("TransformerCenterId");
            item.TransformerCenterName = row.Field<string>("TransformerCenterName"); 
        }
        public static DbHelper.DbResponse<EdwFider> SaveFider(EdwFider item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwFider>(DbHelper.DbResponseStatus.BadRequest);
            if (!Validate(item))
            {
                return new DbHelper.DbResponse<EdwFider>(DbHelper.DbResponseStatus.Conflict, "Tekrar edilmemesi gereken alan ihlal edildi: " + item.ToString());
            }
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            Database db = DatabaseFactory.CreateDatabase();
            if (item.Id > 0)
            {
                sql = @"UPDATE dbo.tEdw_Fider
   SET Name =@Name
      ,TransformerCenterId = @TransformerCenterId
      ,IsActive = @IsActive
      ,Comment=@Comment
      ,LastActivityUserId=@LastActivityUserId
 WHERE Id=@Id";
            }
            else
            {
                sql = @"INSERT INTO tEdw_Fider
           (Name
           ,TransformerCenterId
           ,IsActive
           ,Comment
           ,LastActivityUserId)
     VALUES
           (@Name
           ,@TransformerCenterId
           ,@IsActive
           ,@Comment
           ,@LastActivityUserId) SET @Identity=SCOPE_IDENTITY()";
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
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    db.AddInParameter(cmd, "Comment", DbType.String, item.Comment);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    var res = db.ExecuteNonQuery(cmd);
                    if (res > 0)
                    {
                        if (item.Id == 0)
                            item.Id = (Int32)cmd.Parameters["@Identity"].Value;
                        scope.Complete();
                        return new DbHelper.DbResponse<EdwFider>(DbHelper.DbResponseStatus.OK, null, item);
                    }
                    else return new DbHelper.DbResponse<EdwFider>(DbHelper.DbResponseStatus.NotModified);
                }
            }
        }

        public static DbHelper.DbResponse DeleteFider(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var transCenter = GetFider(id);
            if (transCenter == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            transCenter.IsActive = false;
            var result = SaveFider(transCenter);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}