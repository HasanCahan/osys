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
    public class Formulation
    {
        [Serializable]
        public class ListFilter
        {

            public Nullable<int> EnergyDirectionTypeId { get; set; }
            public Nullable<int> StatusTypeId { get; set; }
            public Nullable<short> MeasurementTypeId { get; set; }
            public int? ExceptId { get; set; }
            public string Sign { get; set; }
        }
        const string baseSql = @"SELECT tf.Id
      ,tf.Sign
      ,tf.LastActivityUserId
      ,tf.CreationTime
	  ,tf.MeasurementTypeId
	  ,tf.EnergyDirectionTypeId
	  ,tf.StatusId as StatusTypeId
      ,st.Name as StatusDescription
      ,tf.Comment
  FROM dbo.tEdw_Formulation as tf
  left join tEdw_Status st on st.Id=tf.StatusId";
        const string singleSql = @"SELECT tf.Id
      ,tf.Sign
      ,tf.LastActivityUserId
      ,tf.CreationTime
	  ,tf.MeasurementTypeId
	  ,tf.EnergyDirectionTypeId
	  ,tf.StatusId as StatusTypeId
      ,st.Name as StatusDescription
      ,tf.Comment
  FROM dbo.tEdw_Formulation as tf
  left join tEdw_Status st on st.Id=tf.StatusId WHERE tf.Id=@Id";
        const string updateSql = @"
UPDATE tEdw_Formulation
   SET MeasurementTypeId = @MeasurementTypeId
      ,StatusId = @StatusId
      ,EnergyDirectionTypeId = @EnergyDirectionTypeId
      ,Sign = @Sign
      ,LastActivityUserId =@LastActivityUserId
      ,Comment=@Comment
 WHERE Id=@Id";
        const string insertSql = @"
INSERT INTO tEdw_Formulation
           (MeasurementTypeId
           ,StatusId
           ,EnergyDirectionTypeId
           ,Sign
           ,LastActivityUserId
           ,Comment)
     VALUES
           (@MeasurementTypeId
           ,@StatusId
           ,@EnergyDirectionTypeId
           ,@Sign
           ,@LastActivityUserId
           ,@Comment)";




        public static Boolean Exists(EdwFormulation item)
        {
            var result = TryFind(item);
            if (result == null)
                return false;
            return true;
        }

        public static EdwFormulation TryFind(EdwFormulation item)
        {
            ListFilter filter = new ListFilter();
            //filter.Sign = item.Sign;
            filter.ExceptId = item.Id;
            filter.StatusTypeId = item.StatusTypeId;
            filter.MeasurementTypeId = item.MeasurementTypeId;
            var ds = ListFormulation(1, 1, filter);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;
            var row = rows[0];
            EdwFormulation result = new EdwFormulation();
            Read(row, result);
            return result;
        }
        static void Read(DataRow row, EdwFormulation item)
        {
            item.Id = row.Field<Int32>("Id");
            item.CreationTime = row.Field<DateTime>("CreationTime");
            item.MeasurementTypeId = row.Field<Int16>("MeasurementTypeId");
            item.StatusDescription = row.Field<string>("StatusDescription");
            item.StatusTypeId = row.Field<Int32?>("StatusTypeId");
            item.EnergyDirectionTypeId = row.Field<int?>("EnergyDirectionTypeId");
            item.Sign = row.Field<string>("Sign");
        }
        public static DataSet ListFormulation(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListFormulation(startRowIndex, maximumRows, filter, ref tmp);
        }


        public static DataSet ListFormulation(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.EnergyDirectionTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EnergyDirectionTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.EnergyDirectionTypeId.Value);
                if (filter.StatusTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "StatusTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.StatusTypeId.Value);
                if (filter.MeasurementTypeId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "MeasurementTypeId", DbHelper.SQLFilterCompareOperator.Equal, filter.MeasurementTypeId.Value);
                if (filter.ExceptId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Id", DbHelper.SQLFilterCompareOperator.NotEqual, filter.ExceptId.Value);
                if (!string.IsNullOrEmpty(filter.Sign)) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Sign", DbHelper.SQLFilterCompareOperator.Like, filter.Sign);
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
            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY CreationTime DESC, MeasurementTypeId) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS formulationWithRowNumber
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

        public static EdwFormulation GetFormulation(Int32 id)
        {
            EdwFormulation retval = new EdwFormulation();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(singleSql))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.CreationTime = reader.GetValue<DateTime>("CreationTime");
                        retval.MeasurementTypeId = reader.GetValue<Int16>("MeasurementTypeId");
                        retval.StatusDescription = reader.GetValue<string>("StatusDescription");
                        retval.StatusTypeId = reader.GetValue<Int32?>("StatusTypeId");
                        retval.EnergyDirectionTypeId = reader.GetValue<int?>("EnergyDirectionTypeId");
                        retval.Comment = reader.GetValue<string>("Comment");
                        retval.Sign = reader.GetValue<string>("Sign");
                    }
                }
            }
            return retval;
        }


        public static DbHelper.DbResponse<EdwFormulation> SaveFormulation(EdwFormulation item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.BadRequest);

            var exists = Exists(item);
            if (exists)
                return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.NotModified, "Bu Formulasyon Bilgileri Zaten Kayıtlı.");
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
                item.CreationTime = DateTime.Now;
            }
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "MeasurementTypeId", DbType.Int16, item.MeasurementTypeId);
                    db.AddInParameter(cmd, "Sign", DbType.String, item.Sign);
                    db.AddInParameter(cmd, "StatusHistoryId", DbType.Int32, item.StatusHistoryId);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.Guid, item.LastActivityUserId);
                    db.AddInParameter(cmd, "StatusId", DbType.Int32, item.StatusTypeId);
                    db.AddInParameter(cmd, "EnergyDirectionTypeId", DbType.Int32, item.EnergyDirectionTypeId);
                    db.AddInParameter(cmd, "Comment", DbType.String, item.Comment);
                    if (!EditMode)
                    {
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwFormulation>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }
        public static DbHelper.DbResponse DeleteFormulation(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tEdw_Formulation WHERE Id=@Id"))
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, id);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            scope.Complete();
                            retval.StatusCode = DbHelper.DbResponseStatus.OK;
                        }
                        else retval.StatusCode = DbHelper.DbResponseStatus.NotFound;
                    }
                }
            }
            catch (SqlException ex)
            {
                string conf = DbHelper.SqlConflictParser(ex.Message);
                if (!String.IsNullOrEmpty(conf))
                {
                    retval.StatusCode = DbHelper.DbResponseStatus.Conflict;
                    retval.StatusDescription = conf;
                }
                else throw;
            }
            catch (Exception ex)
            {
                retval.StatusCode = DbHelper.DbResponseStatus.Error;
                retval.StatusDescription = ex.Message;
            }

            return retval;
        }
    }
}
