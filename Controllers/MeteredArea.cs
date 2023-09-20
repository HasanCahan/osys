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
    public class MeteredArea
    {
        [Serializable]
        public class ListFilter
        {
            public Nullable<int> City { get; set; }
            public Nullable<int> Town { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public static DataSet ListMeteredArea(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListMeteredArea(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListMeteredArea(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                if (filter.Name != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "AreaName", DbHelper.SQLFilterCompareOperator.Like, filter.Name);
                if (filter.Description != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Description", DbHelper.SQLFilterCompareOperator.Like, filter.Description);

                whereClause = fb.ToString();
            }

            string baseSQL = @"SELECT MeteredAreaId, tMeteredArea.TownId, tTown.TownName, tTown.CityId, tCity.CityName, AreaName, [Description]
FROM tMeteredArea
LEFT JOIN tTown ON tMeteredArea.TownId=tTown.TownId
LEFT JOIN tCity ON tTown.CityId=tCity.CityId";

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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY CityName, TownName, AreaName) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS tMeteredAreaWithRowNumber
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

        public static Models.tMeteredArea GetMeteredArea(Int32 meteredAreaId)
        {
            Models.tMeteredArea retval = new Models.tMeteredArea();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT MeteredAreaId, tTown.CityId, tMeteredArea.TownId, AreaName, [Description]
FROM tMeteredArea
LEFT JOIN tTown ON tMeteredArea.TownId=tTown.TownId
LEFT JOIN tCity ON tTown.CityId=tCity.CityId
WHERE MeteredAreaId=@MeteredAreaId"))
            {
                db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, meteredAreaId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.MeteredAreaId = reader.GetValue<Int32>("MeteredAreaId");
                        retval.CityId = reader.GetValue<Int32>("CityId");
                        retval.TownId = reader.GetValue<Int32>("TownId");
                        retval.AreaName = reader.GetValue<string>("AreaName");
                        retval.Description = reader.GetValue<string>("Description");
                    }
                }
            }

            return retval;
        }

        public static DbHelper.DbResponse<Models.tMeteredArea> SaveMeteredArea(Models.tMeteredArea item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.BadRequest);

            string sql = string.Empty;
            bool EditMode = item.MeteredAreaId > 0;
            if (EditMode)
            {
                sql = "UPDATE tMeteredArea SET TownId=@TownId, AreaName=@AreaName, [Description]=@Description WHERE MeteredAreaId=@MeteredAreaId";
            }
            else
            {
                sql = @"INSERT INTO tMeteredArea (TownId, AreaName, Description) VALUES (@TownId, @AreaName, @Description);
SET @MeteredAreaId=SCOPE_IDENTITY()";
            }

            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "TownId", DbType.Int32, item.TownId);
                    db.AddInParameter(cmd, "AreaName", DbType.String, item.AreaName);
                    db.AddInParameter(cmd, "Description", DbType.String, item.Description);

                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, item.MeteredAreaId);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeteredArea"))
                            {
                                db.AddInParameter(cmdlog, "MeteredAreaId", DbType.Int32, item.MeteredAreaId);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "U");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddOutParameter(cmd, "MeteredAreaId", DbType.Int32, 0);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            item.MeteredAreaId = (Int32)cmd.Parameters["@MeteredAreaId"].Value;
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeteredArea"))
                            {
                                db.AddInParameter(cmdlog, "MeteredAreaId", DbType.Int32, item.MeteredAreaId);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "I");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tMeteredArea>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteMeteredArea(Int32 meteredAreaId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tMeteredArea"))
                    {
                        db.AddInParameter(cmdlog, "MeteredAreaId", DbType.Int32, meteredAreaId);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tMeteredArea WHERE MeteredAreaId=@MeteredAreaId"))
                    {
                        db.AddInParameter(cmd, "MeteredAreaId", DbType.Int32, meteredAreaId);
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