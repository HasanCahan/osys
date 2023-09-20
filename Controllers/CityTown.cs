using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Controllers
{
    public class CityTown
    {
        public static DataTable ListCity()
        {
            DataTable retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            DataSet ds = db.ExecuteDataSet(CommandType.Text, "SELECT CityId, CityName FROM tCity ORDER BY CityName");
            if (ds != null && ds.Tables.Count > 0) retval = ds.Tables[0];
            return retval;
        }

        public static DataTable ListTown(Nullable<int> cityId)
        {
            DataTable retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            using (DbCommand cmd = db.GetSqlStringCommand(@"SELECT tCity.CityId, CityName, TownId, TownName FROM tTown LEFT JOIN tCity ON tTown.CityId=tCity.CityId WHERE (@CityId IS NULL OR tCity.CityId=@CityId) ORDER BY CityName, TownName"))
            {
                db.AddInParameter(cmd, "CityId", DbType.Int32, cityId);
                DataSet ds = db.ExecuteDataSet(cmd);
                if (ds != null && ds.Tables.Count > 0) retval = ds.Tables[0];
            }
            return retval;
        }
    }
}