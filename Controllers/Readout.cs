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
    public class Readout
    {
        [Serializable]
        public class ListFilter
        {
            //public Nullable<int> City { get; set; }
            //public Nullable<int> Town { get; set; }
            //public Nullable<int> MeteredArea { get; set; }
            public Nullable<int> Equipment { get; set; }
            public string EquipmentType { get; set; }
            //public Nullable<int> ReadSource { get; set; }
            public Nullable<DateTime> BeginDate { get; set; }
            public Nullable<DateTime> EndDate { get; set; }
            public string Description { get; set; }
        }

        public static DataSet ListReadout(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListReadout(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListReadout(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                //if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                //if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                //if (filter.MeteredArea.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "MeteredAreaId", DbHelper.SQLFilterCompareOperator.Like, filter.MeteredArea);
                if (filter.Equipment.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentId", DbHelper.SQLFilterCompareOperator.Equal, filter.Equipment);
                if (filter.EquipmentType != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EquipmentType", DbHelper.SQLFilterCompareOperator.Equal, filter.EquipmentType);
                //if (filter.ReadSource != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "ReadSource", DbHelper.SQLFilterCompareOperator.Equal, filter.ReadSource);
                if (filter.BeginDate.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "ReadBeginDate", DbHelper.SQLFilterCompareOperator.GreaterEqual, filter.BeginDate);
                if (filter.EndDate.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "ReadEndDate", DbHelper.SQLFilterCompareOperator.LessEqual, filter.EndDate);
                if (filter.Description != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Description", DbHelper.SQLFilterCompareOperator.Like, filter.Description);

                whereClause = fb.ToString();
            }

            string baseSQL = @"SELECT ReadoutId
      ,tReadout.EquipmentId
	  ,tEquipment.EquipmentName
	  ,tEquipment.EquipmentRefText
	  ,tEquipment.EquipmentType
	  ,CASE tEquipment.EquipmentType
	  WHEN 'A' THEN 'Analizör'
	  WHEN 'B' THEN 'Analizör (ION)'
	  WHEN 'O' THEN 'OSOS'
	  WHEN 'H' THEN 'Hesaplama'
	  ELSE tEquipment.EquipmentType END AS EquipmentTypeName
      ,ReadSource
	  ,CASE ReadSource WHEN 'I' THEN 'A (Input)' WHEN 'O' THEN 'B (Output)' ELSE ReadSource END AS ReadSourceName
      ,ReadBeginDate
      ,ReadEndDate
      ,kWh
      ,tReadout.[Description]
      ,Case IsTemporary
	  WHEN 0 THEN 'Hayır'
      WHEN 1 THEN 'Evet'	  	  
	  ELSE '' end as IsTemporary
  FROM tReadout
  LEFT JOIN tEquipment ON tReadout.EquipmentId=tEquipment.EquipmentId";

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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY ReadEndDate DESC, ReadBeginDate DESC, EquipmentName) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS tReadoutWithRowNumber
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

        public static Models.tReadout GetReadout(Int32 readoutId)
        {
            Models.tReadout retval = new Models.tReadout();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT ReadoutId
      ,tReadout.EquipmentId
	  ,tEquipment.EquipmentName
	  ,tEquipment.EquipmentRefText
	  ,tEquipment.EquipmentType
	  ,CASE tEquipment.EquipmentType
	  WHEN 'A' THEN 'Analizör'
	  WHEN 'B' THEN 'Analizör (ION)'
	  WHEN 'O' THEN 'OSOS'
	  WHEN 'H' THEN 'Hesaplama'
	  ELSE tEquipment.EquipmentType END AS EquipmentTypeName
      ,ReadSource
	  ,CASE ReadSource WHEN 'I' THEN 'A (Input)' WHEN 'O' THEN 'B (Output)' ELSE ReadSource END AS ReadSourceName
      ,ReadBeginDate
      ,ReadEndDate
      ,kWh
      ,tReadout.[Description]
      ,Case IsTemporary
	  WHEN 0 THEN 'Hayır'
      WHEN 1 THEN 'Evet'	  	  
	  ELSE '' end as IsTemporary
      ,IsTemporary AS IsTemporaryDesc
  FROM tReadout
  LEFT JOIN tEquipment ON tReadout.EquipmentId=tEquipment.EquipmentId
WHERE ReadoutId=@ReadoutId"))
            {
                db.AddInParameter(cmd, "ReadoutId", DbType.Int32, readoutId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.ReadoutId = reader.GetValue<int>("ReadoutId");
                        retval.EquipmentId = reader.GetValue<int>("EquipmentId");
                        retval.EquipmentName = reader.GetValue<string>("EquipmentName");
                        retval.ReadSource = reader.GetValue<string>("ReadSource");
                        retval.ReadBeginDate = reader.GetValue<DateTime>("ReadBeginDate");
                        retval.ReadEndDate = reader.GetValue<DateTime>("ReadEndDate");
                        retval.kWh = reader.GetValue<double>("kWh");
                        retval.Description = reader.GetValue<string>("Description");
                        retval.IsTemporary = reader.GetValue<bool>("IsTemporaryDesc");
                    }
                }
            }

            return retval;
        }

        public static DbHelper.DbResponse<Models.tReadout> SaveReadout(Models.tReadout item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.BadRequest);

            string sql = string.Empty;
            bool EditMode = item.ReadoutId > 0;
            if (EditMode)
            {
                sql = @"UPDATE tReadout SET EquipmentId=@EquipmentId, ReadSource=@ReadSource, ReadBeginDate=@ReadBeginDate, ReadEndDate=@ReadEndDate, kWh=@kWh, [Description]=@Description , IsTemporary = @IsTemporary 
WHERE ReadoutId=@ReadoutId";
            }
            else
            {
                sql = @"INSERT INTO tReadout (EquipmentId, ReadSource, ReadBeginDate, ReadEndDate, kWh, [Description],IsTemporary)
VALUES (@EquipmentId, @ReadSource, @ReadBeginDate, @ReadEndDate, @kWh, @Description,@IsTemporary)
SET @ReadoutId=SCOPE_IDENTITY()";
            }

            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, item.EquipmentId);
                    db.AddInParameter(cmd, "ReadSource", DbType.String, item.ReadSource);
                    db.AddInParameter(cmd, "ReadBeginDate", DbType.DateTime, item.ReadBeginDate);
                    db.AddInParameter(cmd, "ReadEndDate", DbType.DateTime, item.ReadEndDate);
                    db.AddInParameter(cmd, "kWh", DbType.Double, item.kWh);
                    db.AddInParameter(cmd, "Description", DbType.String, item.Description);
                    db.AddInParameter(cmd, "IsTemporary", DbType.Boolean, item.IsTemporary);

                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "ReadoutId", DbType.Int32, item.ReadoutId);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tReadout"))
                            {
                                db.AddInParameter(cmdlog, "ReadoutId", DbType.Int32, item.ReadoutId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "U");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddOutParameter(cmd, "ReadoutId", DbType.Int32, 0);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            item.ReadoutId = (Int32)cmd.Parameters["@ReadoutId"].Value;
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tReadout"))
                            {
                                db.AddInParameter(cmdlog, "ReadoutId", DbType.Int32, item.ReadoutId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "I");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tReadout>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static int CheckConflict(int equipmentId, DateTime beginDate, DateTime endDate)
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
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT ReadoutId FROM tReadout WHERE EquipmentId=@EquipmentId AND (
(@EndDate>ReadBeginDate AND @EndDate<=ReadEndDate) OR
(@BeginDate>=ReadBeginDate AND @BeginDate<ReadEndDate) OR
(@BeginDate>=ReadBeginDate AND @EndDate<=ReadEndDate) OR
(@BeginDate<ReadBeginDate AND @EndDate>ReadEndDate))"))
                {
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                    db.AddInParameter(cmd, "BeginDate", DbType.DateTime, beginDate);
                    db.AddInParameter(cmd, "EndDate", DbType.DateTime, endDate);
                    return Convert.ToInt32(db.ExecuteScalar(cmd) ?? -1);
                }
            }
        }

        public static DbHelper.DbResponse DeleteReadout(Int32 readoutId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tReadout"))
                    {
                        db.AddInParameter(cmdlog, "ReadoutId", DbType.Int32, readoutId);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "ReadSource", DbType.String, null);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tReadout WHERE ReadoutId=@ReadoutId"))
                    {
                        db.AddInParameter(cmd, "ReadoutId", DbType.Int32, readoutId);
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

        public static DbHelper.DbResponse DeleteReadoutByEquipment(Int32 equipmentId, string readSource)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tReadout"))
                    {
                        db.AddInParameter(cmdlog, "ReadoutId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, equipmentId);
                        db.AddInParameter(cmdlog, "ReadSource", DbType.String, readSource);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tReadout WHERE EquipmentId=@EquipmentId AND (@ReadSource IS NULL OR ReadSource=@ReadSource)"))
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
    }
}