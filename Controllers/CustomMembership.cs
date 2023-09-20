using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Security;

namespace OlcuYonetimSistemi.Controllers
{
    public class CustomMembership
    {
        public static bool IsDomainUser(string userName)
        {
            if (userName == null) throw new NullReferenceException("Kullanıcı adı boş olamaz.");
            bool retval = false;

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (SqlCommand cmd = (SqlCommand)db.GetStoredProcCommand("aspnet_Membership_GetPasswordWithFormat"))
                {
                    db.AddInParameter(cmd, "ApplicationName", System.Data.DbType.String, Membership.ApplicationName);
                    db.AddInParameter(cmd, "UserName", System.Data.DbType.String, userName);
                    db.AddInParameter(cmd, "UpdateLastLoginActivityDate", System.Data.DbType.Boolean, false);
                    db.AddInParameter(cmd, "CurrentTimeUtc", System.Data.DbType.DateTime, DateTime.UtcNow);

                    using (IDataReader reader = db.ExecuteReader(cmd))
                    {
                        if (reader.Read())
                        {
                            retval = (String.Compare(reader[0].ToString(), "DOMAIN", true, Helper.enCulture) == 0);
                        }
                        if (!reader.IsClosed) reader.Close();
                    }
                }
            }
            catch { }

            return retval;
        }

        public static bool UpdateLoginAttempt(string userName, bool loginSuccess)
        {
            if (userName == null) throw new NullReferenceException("Kullanıcı adı boş olamaz.");

            bool retval = false;

            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                using (SqlCommand cmd = (SqlCommand)db.GetStoredProcCommand("aspnet_Membership_UpdateUserInfo"))
                {
                    db.AddInParameter(cmd, "ApplicationName", System.Data.DbType.String, Membership.ApplicationName);
                    db.AddInParameter(cmd, "UserName", System.Data.DbType.String, userName);
                    db.AddInParameter(cmd, "IsPasswordCorrect", System.Data.DbType.Boolean, loginSuccess);
                    db.AddInParameter(cmd, "UpdateLastLoginActivityDate", System.Data.DbType.Boolean, true);
                    db.AddInParameter(cmd, "MaxInvalidPasswordAttempts", System.Data.DbType.Int32, Membership.MaxInvalidPasswordAttempts);
                    db.AddInParameter(cmd, "PasswordAttemptWindow", System.Data.DbType.Int32, Membership.PasswordAttemptWindow);
                    db.AddInParameter(cmd, "CurrentTimeUtc", System.Data.DbType.DateTime, DateTime.UtcNow);
                    db.AddInParameter(cmd, "LastLoginDate", System.Data.DbType.DateTime, (DateTime)cmd.Parameters["@CurrentTimeUtc"].Value);
                    db.AddInParameter(cmd, "LastActivityDate", System.Data.DbType.DateTime, (DateTime)cmd.Parameters["@CurrentTimeUtc"].Value);
                    db.AddParameter(cmd, "@RETURN_VALUE", DbType.Int32, ParameterDirection.ReturnValue, String.Empty, DataRowVersion.Default, null);

                    db.ExecuteNonQuery(cmd);
                    retval = (Convert.ToInt32(cmd.Parameters["@RETURN_VALUE"].Value ?? -1) == 0);
                }
            }
            catch { }

            return retval;
        }

        public static Guid GetActiveUserId()
        {
            Guid retval = Guid.Empty;

            if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    MembershipUser user = Membership.GetUser(username: HttpContext.Current.User.Identity.Name);
                    if (user != null)
                    {
                        retval = (Guid)user.ProviderUserKey;
                    }
                }
            }

            return retval;
        }
        
    }
}