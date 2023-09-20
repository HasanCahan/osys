using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Security;

namespace OlcuYonetimSistemi.Controllers.EDW
{
    public class User
    {
        [Serializable]
        public class ListFilter
        {
            public string UserName { get; set; }
            public string EMail { get; set; }
            public string RoleName { get; set; }
            public DateTime? BeginDate { get; set; }
            public DateTime? EndDate { get; set; }

            public string IlId { get; set; }
        }

        public static DataSet ListUser(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListUser(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListUser(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);

            if (filter != null)
            {
                if (filter.UserName != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "UserName", DbHelper.SQLFilterCompareOperator.Like, filter.UserName);
                if (filter.EMail != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EMail", DbHelper.SQLFilterCompareOperator.Like, filter.EMail);
                if (filter.BeginDate != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "BeginDate", DbHelper.SQLFilterCompareOperator.GreaterEqual, filter.BeginDate);
                if (filter.EndDate != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EndDate", DbHelper.SQLFilterCompareOperator.LessEqual, filter.EndDate);
                if (filter.RoleName != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "RoleName", DbHelper.SQLFilterCompareOperator.Like, filter.RoleName);
                if (filter.IlId != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "IlId", DbHelper.SQLFilterCompareOperator.Like, filter.IlId);

                whereClause = fb.ToString();
            }

            string baseSQL = @"SELECT DISTINCT T.UserId,
SUBSTRING((SELECT ',' + ST1.RoleName  
FROM (SELECT M.UserId as UserId,
U.UserName,
C.CityName,
M.Email as EMail,
M.CreateDate as BeginDate,
M.Password as Password,R.RoleId,
ISNULL(R.RoleName,'') as RoleName 
FROM aspnet_Membership M
LEFT JOIN aspnet_Users U ON M.UserId = U.UserId
LEFT JOIN aspnet_UsersInRoles UR ON UR.UserId = m.UserId
LEFT JOIN tCity C ON U.IlId =C.CityId
LEFT JOIN aspnet_Roles R ON R.RoleId = UR.RoleId) ST1
            WHERE ST1.UserId = T.UserId
            ORDER BY ST1.UserId
            For XML PATH ('')
        ), 2, 1000) [RoleName],T.EMail,T.BeginDate,T.Password,T.UserName,0 RoleId,T.CityName as IlAdi,T.IlId
FROM (SELECT M.UserId as UserId,
U.UserName,
U.IlId,
M.Email as EMail,
M.CreateDate as BeginDate,
M.Password as Password,R.RoleId, 
C.CityName,
ISNULL(R.RoleName,'') as RoleName 
FROM aspnet_Membership M
LEFT JOIN aspnet_Users U ON M.UserId = U.UserId
LEFT JOIN aspnet_UsersInRoles UR ON UR.UserId = m.UserId
LEFT JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
LEFT JOIN tCity C ON U.IlId =C.CityId) T";

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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY BeginDate DESC) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
               (String.IsNullOrEmpty(whereClause) ? String.Empty : " WHERE " + whereClause));

            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(String.Format(@"SELECT * FROM (
{0}
) AS tPmumWithRowNumber
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

        public static Models.User GetUser(Guid UserId)
        {
            Models.User retval = new Models.User();
            List<string> roleNameList = new List<string>();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT M.UserId as UserId,U.UserName,U.IlId,M.Email as EMail,M.CreateDate as BeginDate,M.Password as Password,R.RoleId,ISNULL(R.RoleName,'') as RoleName FROM aspnet_Membership M
LEFT JOIN aspnet_Users U ON M.UserId = U.UserId
LEFT JOIN aspnet_UsersInRoles UR ON UR.UserId = m.UserId
LEFT JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
WHERE U.UserId = @UserId"))
            {
                db.AddInParameter(cmd, "UserId", DbType.Guid, UserId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        retval.UserId = reader.GetValue<Guid>("UserId");
                        retval.UserName = reader.GetValue<string>("UserName");
                        retval.CreateDate = reader.GetValue<DateTime>("BeginDate");
                        retval.Password = reader.GetValue<string>("Password");
                        retval.EMail = reader.GetValue<string>("EMail");
                        retval.RoleId = reader.GetValue<Guid>("RoleId");
                        roleNameList.Add(reader.GetValue<string>("RoleName"));
                        retval.IlId = reader.GetValue<Int32>("IlId");
                    }
                    retval.RoleName = String.Join(",", roleNameList);
                }
            }
            return retval;
        }
        public static Models.User GetUser(string UserName)
        {
            Models.User retval = new Models.User();
            List<string> roleNameList = new List<string>();

            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(@"SELECT M.UserId as UserId,U.UserName,U.IlId,M.Email as EMail,M.CreateDate as BeginDate,M.Password as Password,R.RoleId,ISNULL(R.RoleName,'') as RoleName FROM aspnet_Membership M
LEFT JOIN aspnet_Users U ON M.UserId = U.UserId
LEFT JOIN aspnet_UsersInRoles UR ON UR.UserId = m.UserId
LEFT JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
WHERE U.UserName = @UserName"))
            {
                db.AddInParameter(cmd, "UserName", DbType.String, UserName);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        retval.UserId = reader.GetValue<Guid>("UserId");
                        retval.UserName = reader.GetValue<string>("UserName");
                        retval.CreateDate = reader.GetValue<DateTime>("BeginDate");
                        retval.Password = reader.GetValue<string>("Password");
                        retval.EMail = reader.GetValue<string>("EMail");
                        retval.RoleId = reader.GetValue<Guid>("RoleId");
                        roleNameList.Add(reader.GetValue<string>("RoleName"));
                        retval.IlId = reader.GetValue<Int32>("IlId");
                    }
                    retval.RoleName = String.Join(",", roleNameList);
                }
            }
            return retval;
        }
        public static DbHelper.DbResponse DeleteUser(string userName)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    bool resp = Membership.DeleteUser(userName, true);
                    if (resp)
                    {
                        scope.Complete();
                        retval.StatusCode = DbHelper.DbResponseStatus.OK;
                    }
                    else
                    {
                        retval.StatusCode = DbHelper.DbResponseStatus.NotFound;
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

        public static bool UpdateDomainUsers(string userName)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();

                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("UPDATE aspnet_Membership SET PASSWORD = @PASSWORD WHERE UserId = @UserId"))
                {
                    MembershipUser user = Membership.GetUser(userName);
                    db.AddInParameter(cmd, "@PASSWORD", DbType.String, "DOMAIN");
                    db.AddInParameter(cmd, "UserId", DbType.Guid, user.ProviderUserKey);

                    int authParam = db.ExecuteNonQuery(cmd);
                    if (authParam > 0)
                    {
                        cmd.Connection.Close();
                        return true;
                    }
                    else
                    {
                        cmd.Connection.Close();
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool AddToRole(Guid userGuidId, Guid roleGuidId)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand("INSERT INTO ASPNET_USERSINROLES (UserId,RoleId) VALUES (@UserId,@RoleId)"))
                    {
                        db.AddInParameter(cmd, "@UserId", DbType.Guid, userGuidId);
                        db.AddInParameter(cmd, "@RoleId", DbType.Guid, roleGuidId);

                        int authParam = db.ExecuteNonQuery(cmd);
                        if (authParam > 0)
                        {
                            scope.Complete();
                            cmd.Connection.Close();
                            return true;
                        }
                        else
                        {
                            scope.Dispose();
                            cmd.Connection.Close();
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }


        public static DbHelper.DbResponse<Models.User> SaveUser(Models.User item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest);
            MembershipUser user;

            const string updateSql = @"UPDATE dbo.aspnet_Users
   SET IlId =@IlId
 WHERE UserName=@UserName";
            try
            {
                string sql = string.Empty;
                bool createParam = false;

                Database db = DatabaseFactory.CreateDatabase();

                if (item.Status > 0)
                {
                    user = Membership.GetUser(item.UserName);
                    user.Email = item.EMail;
                    Membership.UpdateUser(user);
                    createParam = true;

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(updateSql))
                    {
                        db.AddInParameter(cmd, "UserName", DbType.String, user.UserName);
                        db.AddInParameter(cmd, "IlId", DbType.Int32, item.IlId);
                        var res = db.ExecuteNonQuery(cmd);
                        if (res > 0)
                        {
                            return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.NotModified);
                    }


                }
                else
                {
                    user = Membership.CreateUser(item.UserName.Trim(), item.Password.Trim(), item.EMail.Trim());

                    createParam = true;

                    using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(updateSql))
                    {
                        db.AddInParameter(cmd, "UserName", DbType.String, user.UserName);
                        db.AddInParameter(cmd, "IlId", DbType.Int32, item.IlId);
                        var res = db.ExecuteNonQuery(cmd);
                        if (res > 0)
                        {
                            return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.NotModified);
                    }
                }

                if (createParam)
                {
                    return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.OK);
                }
                else
                {
                    return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest, ex.Message);
            }
        }

        public static DbHelper.DbResponse<Models.User> SaveUserRole(Models.User item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest);
            List<string> RoleList = item.RoleName.ToLowerInvariant().Split(',').ToList();
            try
            {
                Roles.AddUserToRoles(item.UserName, RoleList.ToArray());
                return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.OK);
            }
            catch (Exception e)
            {
                return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest,e.Message);
            }
        }

        public static DbHelper.DbResponse<Models.User> RemoveUserRole(Models.User item)
        {
            if (item == null) return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest);
            List<string> RoleList = item.RoleName.ToLowerInvariant().Split(',').ToList();
            try
            {
                Roles.RemoveUserFromRoles(item.UserName, RoleList.ToArray());
                return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.OK);
            }
            catch (Exception e)
            {
                return new DbHelper.DbResponse<Models.User>(DbHelper.DbResponseStatus.BadRequest,e.Message);
            }
        }

    }
}