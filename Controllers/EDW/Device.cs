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
    public class Device
    {
        [Serializable]
        public class ListFilter
        {
            public string DeviceInfo { get; set; }
            public Nullable<int> City { get; set; }
            public Nullable<int> Town { get; set; }
            public Nullable<int> TransformerId { get; set; }
            public Nullable<int> TransformerCenterId { get; set; }
        }
        const string baseSql = @"SELECT ddevice.Id
      ,ddevice.TransformerCenterId
      ,ddevice.TransformerId
      ,ddevice.MeasurementTypeId
      ,ddevice.LastActivityUserId
	  ,ddevice.Description
	  ,ddevice.DeviceInfo
	  ,tC.Name as TransformerCenter
	  ,tt.Name as Transformer
  FROM tEdw_Device as ddevice
Left Join tEdw_TransformerCenter as tC on tC.Id=ddevice.TransformerCenterId
left join tEdw_Transformer as tt on tt.Id=ddevice.TransformerId
where ddevice.IsActive=1"
;
        const string singleSql = @"SELECT ddevice.Id
      ,ddevice.TransformerCenterId
      ,ddevice.TransformerId
      ,ddevice.MeasurementTypeId
      ,ddevice.LastActivityUserId
	  ,ddevice.Description
	  ,ddevice.DeviceInfo
	  ,tC.Name as TransformerCenter
	  ,tt.Name as Transformer
  FROM tEdw_Device as ddevice
Left Join tEdw_TransformerCenter as tC on tC.Id=ddevice.TransformerCenterId
left join tEdw_Transformer as tt on tt.Id=ddevice.TransformerId
where ddevice.Id=@Id
";
        const string updateSql = @"UPDATE dbo.tEdw_Device
   SET TransformerCenterId = @TransformerCenterId
      ,TransformerId = @TransformerId
      ,Description = @Description
      ,DeviceInfo = @DeviceInfo
      ,MeasurementTypeId = @MeasurementTypeId
      ,LastActivityUserId = @LastActivityUserId
      ,IsActive = @IsActive
 WHERE Id=@Id";
        const string insertSql = @"INSERT INTO tEdw_Device
           (TransformerCenterId
           ,TransformerId
           ,Description
           ,DeviceInfo
           ,MeasurementTypeId
           ,LastActivityUserId
           ,IsActive)
     VALUES
           (@TransformerCenterId
           ,@TransformerId
           ,@Description
           ,@DeviceInfo
           ,@MeasurementTypeId
           ,@LastActivityUserId
           ,@IsActive)";
        public static DataSet ListDevice(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListDevice(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListDevice(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.DeviceInfo != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "DeviceInfo", DbHelper.SQLFilterCompareOperator.Like, filter.DeviceInfo);
                if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                if (filter.TransformerId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerId.Value);
                if (filter.TransformerCenterId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TransformerCenterId", DbHelper.SQLFilterCompareOperator.Equal, filter.TransformerCenterId.Value);
                whereClause = fb.ToString();
            }
            string baseSQL = baseSql;
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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY TransformerCenter, Transformer) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS deviceWithRowNumber
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

        public static EdwDevice GetDevice(Int32 id)
        {
            EdwDevice retval = new EdwDevice();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(singleSql))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.MeasurementTypeId = reader.GetValue<Int32>("MeasurementTypeId");
                        retval.MeasurementDescription = reader.GetEnumFromInt<MeasurementType>("MeasurementTypeId").ToString();
                        retval.Description = reader.GetValue<string>("Description");
                        retval.DeviceInfo = reader.GetValue<string>("DeviceInfo");
                        retval.TransformerCenterId = reader.GetValue<Int32>("TransformerCenterId");
                        retval.TransformerId = reader.GetValue<Int32>("TransformerId");
                        retval.Transformer = reader.GetValue<string>("Transformer");
                        retval.TransformerCenter = reader.GetValue<string>("TransformerCenter");
                    }
                }
            }
            return retval;
        }


        public static DbHelper.DbResponse<EdwDevice> SaveDevice(EdwDevice item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwDevice>(DbHelper.DbResponseStatus.BadRequest);
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
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
                    db.AddInParameter(cmd, "DeviceInfo", DbType.String, item.DeviceInfo);
                    db.AddInParameter(cmd, "TransformerCenterId", DbType.Int32, item.TransformerCenterId);
                    db.AddInParameter(cmd, "Description", DbType.Int32, item.Description);
                    db.AddInParameter(cmd, "TransformerId", DbType.Int32, item.TransformerId);
                    db.AddInParameter(cmd, "MeasurementTypeId", DbType.Int32, item.MeasurementTypeId);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.String, item.LastActivityUserId.ToString());
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    if (!EditMode)
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwDevice>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwDevice>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwDevice>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwDevice>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteDevice(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var transCenter = GetDevice(id);
            if (transCenter == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            transCenter.IsActive = false;
            var result = SaveDevice(transCenter);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }
    }
}