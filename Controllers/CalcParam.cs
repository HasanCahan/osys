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
    public class CalcParam
    {
        public static Models.tCalcParam GetCalcParamByEquipment(Int32 equipmentId, Nullable<int> year, string source)
        {
            Models.tCalcParam retval = new Models.tCalcParam();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT CalcParamId
,tEquipment.EquipmentId
,tEquipment.EquipmentName
,tEquipment.EquipmentRefText
,ReadSource
,CASE ReadSource WHEN 'I' THEN 'A (Input)' WHEN 'O' THEN 'B (Output)' ELSE ReadSource END AS ReadSourceName
,kVA, K1, K2, K3, K4, K5, K6, K7, K8, K9, K10, K11, K12
,DataPeriod
FROM tEquipment
LEFT JOIN tCalcParam ON tEquipment.EquipmentId=tCalcParam.EquipmentId
AND (@DataPeriod IS NULL OR DataPeriod = @DataPeriod)
AND (@ReadSource IS NULL OR ReadSource = @ReadSource)
WHERE tEquipment.EquipmentId=@EquipmentId
ORDER BY CalcParamId DESC"))
            {
                db.AddInParameter(cmd, "EquipmentId", DbType.Int32, equipmentId);
                db.AddInParameter(cmd, "DataPeriod", DbType.Int32, year);
                db.AddInParameter(cmd, "ReadSource", DbType.String, source);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.CalcParamId = reader.GetValue<int>("CalcParamId");
                        retval.EquipmentId = reader.GetValue<int>("EquipmentId");
                        retval.EquipmentName = reader.GetValue<string>("EquipmentName");
                        retval.ReadSource = reader.GetValue<string>("ReadSource");
                        retval.ReadSourceName = reader.GetValue<string>("ReadSourceName");
                        retval.kVA = reader.GetValue<Nullable<int>>("kVA");
                        retval.K1 = reader.GetValue<Nullable<double>>("K1");
                        retval.K2 = reader.GetValue<Nullable<double>>("K2");
                        retval.K3 = reader.GetValue<Nullable<double>>("K3");
                        retval.K4 = reader.GetValue<Nullable<double>>("K4");
                        retval.K5 = reader.GetValue<Nullable<double>>("K5");
                        retval.K6 = reader.GetValue<Nullable<double>>("K6");
                        retval.K7 = reader.GetValue<Nullable<double>>("K7");
                        retval.K8 = reader.GetValue<Nullable<double>>("K8");
                        retval.K9 = reader.GetValue<Nullable<double>>("K9");
                        retval.K10 = reader.GetValue<Nullable<double>>("K10");
                        retval.K11 = reader.GetValue<Nullable<double>>("K11");
                        retval.K12 = reader.GetValue<Nullable<double>>("K12");
                        retval.DataPeriod = reader.GetValue<int>("DataPeriod");
                    }
                }
            }

            return retval;
        }

        public static DbHelper.DbResponse<Models.tCalcParam> SaveCalcParam(Models.tCalcParam item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.BadRequest);

            string sql = string.Empty;
            bool EditMode = item.CalcParamId > 0;
            if (EditMode)
            {
                sql = @"UPDATE tCalcParam SET EquipmentId=@EquipmentId, [ReadSource]=@ReadSource, kVA=@kVA, K1=@K1, K2=@K2, K3=@K3, K4=@K4, K5=@K5, K6=@K6, K7=@K7, K8=@K8, K9=@K9, K10=@K10, K11=@K11, K12=@K12, DataPeriod=@DataPeriod
WHERE CalcParamId=@CalcParamId";
            }
            else
            {
                sql = @"INSERT INTO tCalcParam (EquipmentId, [ReadSource], kVA, K1, K2, K3, K4, K5, K6, K7, K8, K9, K10, K11, K12, DataPeriod)
VALUES (@EquipmentId, @ReadSource, @kVA, @K1, @K2, @K3, @K4, @K5, @K6, @K7, @K8, @K9, @K10, @K11, @K12, @DataPeriod)
SET @CalcParamId=SCOPE_IDENTITY()";
            }

            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "EquipmentId", DbType.Int32, item.EquipmentId);
                    db.AddInParameter(cmd, "ReadSource", DbType.String, item.ReadSource);
                    db.AddInParameter(cmd, "kVA", DbType.Double, item.kVA);
                    db.AddInParameter(cmd, "K1", DbType.Double, item.K1);
                    db.AddInParameter(cmd, "K2", DbType.Double, item.K2);
                    db.AddInParameter(cmd, "K3", DbType.Double, item.K3);
                    db.AddInParameter(cmd, "K4", DbType.Double, item.K4);
                    db.AddInParameter(cmd, "K5", DbType.Double, item.K5);
                    db.AddInParameter(cmd, "K6", DbType.Double, item.K6);
                    db.AddInParameter(cmd, "K7", DbType.Double, item.K7);
                    db.AddInParameter(cmd, "K8", DbType.Double, item.K8);
                    db.AddInParameter(cmd, "K9", DbType.Double, item.K9);
                    db.AddInParameter(cmd, "K10", DbType.Double, item.K10);
                    db.AddInParameter(cmd, "K11", DbType.Double, item.K11);
                    db.AddInParameter(cmd, "K12", DbType.Double, item.K12);
                    db.AddInParameter(cmd, "DataPeriod", DbType.Int32, item.DataPeriod.GetValueOrDefault(DateTime.Today.Year));

                    if (EditMode)
                    {
                        db.AddInParameter(cmd, "CalcParamId", DbType.Int32, item.CalcParamId);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tCalcParam"))
                            {
                                db.AddInParameter(cmdlog, "CalcParamId", DbType.Int32, item.CalcParamId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "U");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddOutParameter(cmd, "CalcParamId", DbType.Int32, 0);
                        if (db.ExecuteNonQuery(cmd) > 0)
                        {
                            item.CalcParamId = (Int32)cmd.Parameters["@CalcParamId"].Value;
                            using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tCalcParam"))
                            {
                                db.AddInParameter(cmdlog, "CalcParamId", DbType.Int32, item.CalcParamId);
                                db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                                db.AddInParameter(cmdlog, "LastActivity", DbType.String, "I");
                                db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                                if (db.ExecuteNonQuery(cmdlog) > 0)
                                {
                                    scope.Complete();
                                    return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.OK, null, item);
                                }
                                else return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.Error, "İşlem kaydı günlüğe yazılamadığından gerçekleştirilemiyor.");
                            }
                        }
                        else return new DbHelper.DbResponse<Models.tCalcParam>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteCalcParam(Int32 calcParamId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tCalcParam"))
                    {
                        db.AddInParameter(cmdlog, "CalcParamId", DbType.Int32, calcParamId);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tCalcParam WHERE CalcParamId=@CalcParamId"))
                    {
                        db.AddInParameter(cmd, "CalcParamId", DbType.Int32, calcParamId);
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

        public static DbHelper.DbResponse DeleteCalcParamByEquipment(Int32 equipmentId)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    using (SqlCommand cmdlog = (SqlCommand)db.GetStoredProcCommand("splog_tCalcParam"))
                    {
                        db.AddInParameter(cmdlog, "CalcParamId", DbType.Int32, null);
                        db.AddInParameter(cmdlog, "EquipmentId", DbType.Int32, equipmentId);
                        db.AddInParameter(cmdlog, "LastActivity", DbType.String, "D");
                        db.AddInParameter(cmdlog, "LastActivityUserId", DbType.Guid, CustomMembership.GetActiveUserId());
                        db.ExecuteNonQuery(cmdlog);
                    }

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("DELETE FROM tCalcParam WHERE EquipmentId=@EquipmentId"))
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