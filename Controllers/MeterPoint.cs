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
    public class MeterPoint
    {
        [Serializable]
        public class ListFilter
        {
            public Nullable<int> City { get; set; }
            public Nullable<int> Town { get; set; }
            public Nullable<int> MeteredArea { get; set; }
            public Nullable<int> Equipment { get; set; }
            public string ReadSource { get; set; }
            public Nullable<DateTime> BeginDate { get; set; }
            public Nullable<DateTime> EndDate { get; set; }
            // public Boolean IsValid { get; set; }
            public Nullable<int> Valid { get; set; }
        }

        public static DataSet ListMeterPoint(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListMeterPoint(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListMeterPoint(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                // Default Olarak Bu Kaydedilecek                                
                if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                if (filter.MeteredArea.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "MeteredAreaId", DbHelper.SQLFilterCompareOperator.Equal, filter.MeteredArea);
                if (filter.Equipment.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentId", DbHelper.SQLFilterCompareOperator.Equal, filter.Equipment);
                if (filter.ReadSource != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "ReadSource", DbHelper.SQLFilterCompareOperator.Equal, filter.ReadSource);
                if (filter.BeginDate.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "BeginDate", DbHelper.SQLFilterCompareOperator.GreaterEqual, filter.BeginDate);
                if (filter.EndDate.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EndDate", DbHelper.SQLFilterCompareOperator.LessEqual, filter.EndDate);
                //if (filter.IsValid == true)
                //{
                //    fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "BeginDate", DbHelper.SQLFilterCompareOperator.LessEqual, DateTime.Now);                    
                //}
                if (filter.Valid == 1)
                {
                    fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "BeginDate", DbHelper.SQLFilterCompareOperator.LessEqual, DateTime.Now);
                }

                whereClause = fb.ToString();
            }
            string subSql = string.Empty;
            string baseSQL = @"SELECT M.MeterPointId
,tTown.CityId
,tCity.CityName
,tMeteredArea.TownId
,tTown.TownName
,M.MeteredAreaId
,tMeteredArea.AreaName
,M.EquipmentId
,tEquipment.EquipmentName
,tEquipment.EquipmentType
,ReadSource
,CASE ReadSource WHEN 'I' THEN 'A (Input)' WHEN 'O' THEN 'B (Output)' ELSE ReadSource END AS ReadSourceName
,CalcSign
,M.BeginDate
,M.EndDate
FROM tMeterPoint M
LEFT JOIN tMeteredArea ON M.MeteredAreaId=tMeteredArea.MeteredAreaId
LEFT JOIN tEquipment ON M.EquipmentId=tEquipment.EquipmentId
LEFT JOIN tTown ON tMeteredArea.TownId=tTown.TownId
LEFT JOIN tCity ON tTown.CityId=tCity.CityId ";
            if (filter.Valid == 1)
            {
                subSql = @" WHERE M.MeterPointId IN (SELECT D.MeterPointId FROM tMeterPoint D
								WHERE D.EquipmentId = M.EquipmentId
								AND D.MeteredAreaId = M.MeteredAreaId
								AND D.BeginDate < GETDATE()
								AND D.ReadSource IN('I','O')																																								
                                AND (D.EndDate > GETDATE() OR D.EndDate is null))";
								//ORDER BY D.BeginDate DESC)";
                baseSQL = baseSQL + subSql;
            }
            else if (filter.Valid == 2)
            {
                subSql = @" WHERE M.MeterPointId IN (SELECT D.MeterPointId FROM tMeterPoint D
								WHERE D.EquipmentId = M.EquipmentId
								AND D.MeteredAreaId = M.MeteredAreaId
								-- OR D.BeginDate > GETDATE() OR (D.EndDate < GETDATE())
                                AND D.ReadSource IN('I','O')								
                                AND (D.BeginDate > GETDATE() OR (D.EndDate < GETDATE())))";
								//ORDER BY D.BeginDate DESC)";
                baseSQL = baseSQL + subSql;
            }

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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY CityName, TownName, AreaName, EquipmentName) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS tMeterPointWithRowNumber
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

        public static Models.tMeterPoint GetMeterPoint(Int32 meterPointId)
        {
            Models.tMeterPoint retval = new Models.tMeterPoint();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT MeterPointId
,tTown.CityId
,tCity.CityName
,tMeteredArea.TownId
,tTown.TownName
,tMeterPoint.MeteredAreaId
,tMeteredArea.AreaName
,tMeterPoint.EquipmentId
,tEquipment.EquipmentName
,tEquipment.EquipmentType
,ReadSource
,CalcSign
,tMeterPoint.BeginDate
,tMeterPoint.EndDate
FROM tMeterPoint
LEFT JOIN tMeteredArea ON tMeterPoint.MeteredAreaId=tMeteredArea.MeteredAreaId
LEFT JOIN tEquipment ON tMeterPoint.EquipmentId=tEquipment.EquipmentId
LEFT JOIN tTown ON tMeteredArea.TownId=tTown.TownId
LEFT JOIN tCity ON tTown.CityId=tCity.CityId
WHERE MeterPointId=@MeterPointId"))
            {
                db.AddInParameter(cmd, "MeterPointId", DbType.Int32, meterPointId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.MeterPointId = reader.GetValue<int>("MeterPointId");
                        retval.CityId = reader.GetValue<Nullable<int>>("CityId");
                        retval.CityName = reader.GetValue<string>("CityName");
                        retval.TownId = reader.GetValue<Nullable<int>>("TownId");
                        retval.TownName = reader.GetValue<string>("TownName");
                        retval.MeteredAreaId = reader.GetValue<Int32>("MeteredAreaId");
                        retval.AreaName = reader.GetValue<string>("AreaName");
                        retval.EquipmentId = reader.GetValue<Int32>("EquipmentId");
                        retval.EquipmentName = reader.GetValue<string>("EquipmentName");
                        retval.EquipmentType = reader.GetValue<string>("EquipmentType");
                        retval.ReadSource = reader.GetValue<string>("ReadSource");
                        retval.CalcSign = reader.GetValue<string>("CalcSign");
                        retval.BeginDate = reader.GetValue<DateTime>("BeginDate");
                        retval.EndDate = reader.GetValue<DateTime>("EndDate");
                    }
                }
            }

            return retval;
        }

        public static DbHelper.DbResponse<Models.tMeterPoint> SaveMeterPoint(Models.tMeterPoint item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.BadRequest);

            string sql = string.Empty;
            bool EditMode = item.MeterPointId > 0;
            if (EditMode && item.IsValid != "-1")
            {
                sql = "UPDATE tMeterPoint SET MeteredAreaId=@MeteredAreaId, EquipmentId=@EquipmentId, ReadSource=@ReadSource, CalcSign=@CalcSign, BeginDate=@BeginDate, EndDate=@EndDate WHERE MeterPointId=@MeterPointId";
            }
            else
            {
                sql = @"INSERT INTO tMeterPoint (MeteredAreaId, EquipmentId, ReadSource, CalcSign,BeginDate,EndDate) VALUES (@MeteredAreaId, @EquipmentId, @ReadSource, @CalcSign,@BeginDate,@EndDate)
SET @MeterPointId=SCOPE_IDENTITY()";
            }

            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, item.MeteredAreaId);
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, item.EquipmentId);
                    db.AddInParameter(cmd, "ReadSource", DbType.String, item.ReadSource);
                    db.AddInParameter(cmd, "CalcSign", DbType.String, item.CalcSign);
                    db.AddInParameter(cmd, "BeginDate", DbType.DateTime, item.BeginDate);

                    if (item.EndDate != DateTime.MinValue) db.AddInParameter(cmd, "EndDate", DbType.DateTime, item.EndDate);
                    else db.AddInParameter(cmd, "EndDate", DbType.DateTime, null);

                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "MeterPointId", DbType.Int32, item.MeterPointId);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeterPoint"))
                            {
                                db.AddInParameter(cmdlog, "MeterPointId", DbType.Int32, item.MeterPointId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "U");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddOutParameter(cmd, "MeterPointId", DbType.Int32, 0);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            item.MeterPointId = (Int32)cmd.Parameters["@MeterPointId"].Value;
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeterPoint"))
                            {
                                db.AddInParameter(cmdlog, "MeterPointId", DbType.Int32, item.MeterPointId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "I");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tMeterPoint>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteMeterPoint(Int32 meterPointId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeterPoint"))
                    {
                        db.AddInParameter(cmdlog, "MeterPointId", DbType.Int32, meterPointId);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tMeterPoint WHERE MeterPointId=@MeterPointId"))
                    {
                        db.AddInParameter(cmd, "MeterPointId", DbType.Int32, meterPointId);
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

        public static DbHelper.DbResponse DeleteMeterPointByEquipment(Int32 equipmentId, string readSource)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeterPoint"))
                    {
                        db.AddInParameter(cmdlog, "MeterPointId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, equipmentId);
                        db.AddInParameter(cmdlog, "ReadSource", DbType.String, readSource);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tMeterPoint WHERE EquipmentId=@EquipmentId AND (@ReadSource IS NULL OR ReadSource=@ReadSource)"))
                    {
                        db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                        db.AddInParameter(cmd, "ReadSource", DbType.String, readSource);
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

        public static int CheckConflict(int meteredAreaId, int equipmentId, DateTime beginDate, DateTime endDate,string readSource)
        {
            /*
            ---------------|-----Conflict-----|---------------
                   |----A1----|
                                         |----A2----|
                                |--A3--|
                        |----------A4----------|
             */
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT Sum(MeterPointId) FROM tMeterPoint WHERE tMeterPoint.MeteredAreaId = @MeteredAreaId AND tMeterPoint.EquipmentId=@EquipmentId AND (
(@EndDate>BeginDate AND @EndDate<=EndDate) OR
(@BeginDate>=BeginDate AND @BeginDate<EndDate) OR
(@BeginDate>=BeginDate AND @EndDate<=EndDate) OR
(@BeginDate<BeginDate AND @EndDate>EndDate)) AND ReadSource = @ReadSource"))
                {
                    DateTime _endDate = (endDate != DateTime.MinValue) ? endDate : beginDate;

                    db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, meteredAreaId);
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                    db.AddInParameter(cmd, "BeginDate", DbType.DateTime, beginDate);
                    db.AddInParameter(cmd, "EndDate", DbType.DateTime, _endDate);
                    db.AddInParameter(cmd, "ReadSource", DbType.String, readSource);
                    if (db.ExecuteScalar(cmd) != DBNull.Value)
                    {
                        return Convert.ToInt32(db.ExecuteScalar(cmd) ?? -1);
                    }
                    else
                    {
                        return -1;
                    }

                }
            }
        }

        public static int CheckCtrlTerminateDate(int meterPointId, int meteredAreaId, int equipmentId,string readSource)
        {
            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT M.MeterPointId FROM tMeterPoint M
WHERE M.MeteredAreaId = @MeteredAreaId AND M.EquipmentId = @EquipmentId
AND (M.EndDate is null OR M.MeterPointId = 
(SELECT D.MeterPointId FROM tMeterPoint D 
LEFT JOIN tMeterPoint M ON M.MeterPointId = D.MeterPointId
WHERE (D.MeterPointId = @MeterPointId OR D.BeginDate > M.BeginDate)))
AND M.ReadSource = @ReadSource"))
                {
                    db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, meteredAreaId);
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                    db.AddInParameter(cmd, "MeterPointId", DbType.Int32, meterPointId);
                    db.AddInParameter(cmd, "ReadSource", DbType.String, readSource);
                    if (db.ExecuteScalar(cmd) != null)
                    {
                        return Convert.ToInt32(db.ExecuteScalar(cmd));
                    }
                    else
                    {
                        return -1;
                    }

                }
            }
        }

    }
}