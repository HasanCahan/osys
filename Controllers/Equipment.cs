using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;

namespace OlcuYonetimSistemi.Controllers
{
    public class Equipment
    {
        [Serializable]
        public class ListFilter
        {
            public Nullable<Int32> EquipmentId { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string RefText { get; set; }
            public Nullable<Int32> CBSRef { get; set; }
        }

        public static DataSet ListEquipment(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListEquipment(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListEquipment(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.EquipmentId.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentId", DbHelper.SQLFilterCompareOperator.Equal, filter.EquipmentId.Value);
                if (filter.Type != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentType", DbHelper.SQLFilterCompareOperator.Equal, filter.Type);
                if (filter.Name != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentName", DbHelper.SQLFilterCompareOperator.Like, filter.Name);
                if (filter.RefText != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentRef", DbHelper.SQLFilterCompareOperator.Equal, filter.RefText);
                if (filter.CBSRef.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CBSId", DbHelper.SQLFilterCompareOperator.Equal, filter.CBSRef.Value);

                whereClause = fb.ToString();
            }

            string baseSQL = @"SELECT EquipmentId
		  ,EquipmentType
		  ,CASE EquipmentType
		  WHEN 'A' THEN 'Analizör'
		  WHEN 'B' THEN 'Analizör (ION)'
		  WHEN 'O' THEN 'OSOS'
		  WHEN 'H' THEN 'Hesaplama'
		  ELSE EquipmentType
		  END AS EquipmentTypeName
		  ,EquipmentRefNo
		  ,EquipmentRefText
		  ,COALESCE(EquipmentRefText, CAST(EquipmentRefNo AS varchar))  AS EquipmentRef
		  ,EquipmentName
		  ,CBSId
		  ,Bidirectional
	  FROM tEquipment";

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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY EquipmentType, EquipmentName) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS tEquipmentWithRowNumber
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

        public static Models.tEquipment GetEquipment(Int32 equipmentId)
        {
            Models.tEquipment retval = new Models.tEquipment();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("SELECT * FROM tEquipment WHERE EquipmentId=@EquipmentId"))
            {
                db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.EquipmentId = reader.GetValue<Int32>("EquipmentId");
                        retval.EquipmentType = reader.GetValue<string>("EquipmentType");
                        retval.EquipmentName = reader.GetValue<string>("EquipmentName");
                        retval.EquipmentRefNo = reader.GetValue<long?>("EquipmentRefNo");
                        retval.EquipmentRefText = reader.GetValue<string>("EquipmentRefText");
                        retval.CBSId = reader.GetValue<int?>("CBSId");
                        retval.Bidirectional = reader.GetValue<bool>("Bidirectional");
                    }
                }
            }

            return retval;
        }

        public static DbHelper.DbResponse<Models.tEquipment> SaveEquipment(Models.tEquipment item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.BadRequest);

            string sql = string.Empty;
            bool EditMode = item.EquipmentId > 0;

            Database db = DatabaseFactory.CreateDatabase();
            #region "Kayıt çakışması kontrolleri"
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("SELECT 0"))
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM tEquipment WHERE EquipmentName=@EquipmentName AND (@EquipmentId IS NULL OR EquipmentId<>@EquipmentId)";
                    db.AddInParameter(cmd, "EquipmentName", DbType.String, item.EquipmentName);
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, (EditMode ? item.EquipmentId : new Nullable<int>()));
                    if (Convert.ToInt32(db.ExecuteScalar(cmd)) > 0)
                    {
                        return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.Conflict, "Aynı isimle farklı bir ekipman mevcut", item);
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT COUNT(*) FROM tEquipment WHERE EquipmentType=@EquipmentType AND @EquipmentRefText IS NOT NULL AND EquipmentRefText = @EquipmentRefText AND (@EquipmentId IS NULL OR EquipmentId<>@EquipmentId)";
                        db.AddInParameter(cmd, "EquipmentType", DbType.String, item.EquipmentType);
                        db.AddInParameter(cmd, "EquipmentRefText", DbType.String, string.IsNullOrEmpty(item.EquipmentRefText) ? null : item.EquipmentRefText);
                        db.AddInParameter(cmd, "EquipmentId", DbType.Int32, (EditMode ? item.EquipmentId : (object)null));
                        if (Convert.ToInt32(db.ExecuteScalar(cmd)) > 0)
                        {
                            return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.Conflict, "Aynı referans ile farklı bir ekipman mevcut", item);
                        }
                    }
                    cmd.Parameters.Clear();
                }
            }
            #endregion "Kayıt çakışması kontrolleri"

            if (EditMode)
            {
                sql = "UPDATE tEquipment SET EquipmentType = @EquipmentType, EquipmentRefNo=@EquipmentRefNo, EquipmentRefText=@EquipmentRefText, EquipmentName = @EquipmentName, CBSId = @CBSId, Bidirectional = @Bidirectional WHERE EquipmentId=@EquipmentId";
            }
            else
            {
                sql = @"INSERT INTO tEquipment (EquipmentType, EquipmentRefNo, EquipmentRefText, EquipmentName, CBSId, Bidirectional)
                VALUES (@EquipmentType, @EquipmentRefNo, @EquipmentRefText, @EquipmentName, @CBSId, @Bidirectional);
                SET @EquipmentId=SCOPE_IDENTITY()";
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "EquipmentType", DbType.String, item.EquipmentType);
                    db.AddInParameter(cmd, "EquipmentRefNo", DbType.Int64, item.EquipmentRefNo);
                    db.AddInParameter(cmd, "EquipmentRefText", DbType.String, item.EquipmentRefText);
                    db.AddInParameter(cmd, "EquipmentName", DbType.String, item.EquipmentName);
                    db.AddInParameter(cmd, "CBSId", DbType.Int32, item.CBSId);
                    db.AddInParameter(cmd, "Bidirectional", DbType.Boolean, item.Bidirectional);
                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "EquipmentId", DbType.Int32, item.EquipmentId);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tEquipment"))
                            {
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, item.EquipmentId);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "U");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddOutParameter(cmd, "EquipmentId", DbType.Int32, 0);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            item.EquipmentId = (Int32)cmd.Parameters["@EquipmentId"].Value;
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tEquipment"))
                            {
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, item.EquipmentId);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "I");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tEquipment>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteEquipment(Int32 equipmentId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tEquipment"))
                    {
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, equipmentId);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tEquipment WHERE EquipmentId=@EquipmentId"))
                    {
                        db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
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