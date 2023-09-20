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

    public class TransformerCenter
    {
        [Serializable]
        public class ListFilter
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Nullable<int> City { get; set; }
            public Nullable<int> Town { get; set; }
            public Nullable<int> EdwNumber { get; set; }
        }

        public static DataSet ListTransformerCenter(Int32 startRowIndex, Int32 maximumRows, ListFilter filter)
        {
            Int32 tmp = Int32.MinValue;
            return ListTransformerCenter(startRowIndex, maximumRows, filter, ref tmp);
        }

        public static DataSet ListTransformerCenter(Int32 startRowIndex, Int32 maximumRows, String whereClause, ref Int32 totalRows, DbHelper.SQLFilterBuilder fb)
        {
            DataSet retval = null;
            Database db = DatabaseFactory.CreateDatabase();
            string baseSQL = @"SELECT tc.Id
      ,tc.Name
      ,tc.TownId
      ,tci.CityId
      ,tc.EdwNumber
      ,tc.IsActive
	  ,tci.CityName
	  ,tt.TownName
  FROM dbo.tEdw_TransformerCenter as tc
  left join tTown tt on tt.TownId=tc.TownId
  left join tCity tci on tci.CityId=tt.CityId
  where tc.IsActive=1";
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

            string fetchSQL = String.Format("SELECT tBase.*, ROW_NUMBER() OVER(ORDER BY CityName, TownName, Name) As OrderRank FROM ({0}) AS tBase{1}", baseSQL,
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

        public static DataSet ListTransformerCenter(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.Name != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Name", DbHelper.SQLFilterCompareOperator.Like, filter.Name);
                if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                if (filter.EdwNumber.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "EdwNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.EdwNumber.Value);
                whereClause = fb.ToString();
            }
            return ListTransformerCenter(startRowIndex, maximumRows, whereClause, ref totalRows, fb);
        }
        public static DataSet ListTransformerCenter2(Int32 startRowIndex, Int32 maximumRows, ListFilter filter, ref Int32 totalRows)
        {

            string whereClause = null;
            DbHelper.SQLFilterGroup fg = new DbHelper.SQLFilterGroup();
            DbHelper.SQLFilterBuilder fb = new DbHelper.SQLFilterBuilder(fg);
            if (filter != null)
            {
                if (filter.Name != null) fg.AddItem(DbHelper.SQLFilterConcatOperator.And, "Name", DbHelper.SQLFilterCompareOperator.Equal, filter.Name);
                if (filter.City.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "CityId", DbHelper.SQLFilterCompareOperator.Equal, filter.City.Value);
                if (filter.Town.HasValue) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "TownId", DbHelper.SQLFilterCompareOperator.Equal, filter.Town.Value);
                if (filter.EdwNumber.HasValue && filter.EdwNumber != 0) fg.AddItem(DbHelper.SQLFilterConcatOperator.Or, "EdwNumber", DbHelper.SQLFilterCompareOperator.Equal, filter.EdwNumber.Value);
                whereClause = fb.ToString();
            }
            return ListTransformerCenter(startRowIndex, maximumRows, whereClause, ref totalRows, fb);
        }
        public static EdwTransformerCenter GetTransformerCenter(Int32 id)
        {
            EdwTransformerCenter retval = new EdwTransformerCenter();
            Database db = DatabaseFactory.CreateDatabase();
            using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(
@"
SELECT tc.Id
      ,tc.Name
      ,tc.TownId
      ,tc.EdwNumber
      ,tc.IsActive
	  ,tci.CityName
      ,tci.CityId
	  ,tt.TownName
  FROM dbo.tEdw_TransformerCenter as tc
  left join tTown tt on tt.TownId=tc.TownId
  left join tCity tci on tci.CityId=tt.CityId
  where tc.IsActive=1
  and Id=@Id"))
            {
                db.AddInParameter(cmd, "Id", DbType.Int32, id);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        retval.Id = reader.GetValue<Int32>("Id");
                        retval.EdwNumber = reader.GetValue<Int32>("EdwNumber");
                        retval.TownId = reader.GetValue<Int32?>("TownId");
                        retval.CityId = reader.GetValue<Int32>("CityId");
                        retval.Name = reader.GetValue<string>("Name");
                        retval.TownName = reader.GetValue<String>("TownName");
                        retval.CityName = reader.GetValue<string>("CityName");
                    }
                }
            }
            return retval;
        }


        public static DbHelper.DbResponse<EdwTransformerCenter> SaveTransformerCenter(EdwTransformerCenter item)
        {
            if (item == null) return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.BadRequest);
            if (!Validate(item))
            {
                return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.Conflict, "Tekrar edilmemesi gereken alan ihlal edildi: " + item.ToString());
            }
            item.LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            string sql = string.Empty;
            bool EditMode = item.Id > 0;
            if (EditMode)
            {
                sql = @"
UPDATE tEdw_TransformerCenter
   SET Name = @Name
      ,CityId=@CityId
      ,TownId = @TownId
      ,EdwNumber = @EdwNumber
      ,IsActive = @IsActive
      ,LastActivityUserId=@LastActivityUserId
 WHERE Id=@Id
";
            }
            else
            {
                sql = @"INSERT INTO tEdw_TransformerCenter
           (Name
           ,CityId
           ,TownId
           ,EdwNumber
           ,IsActive
           ,LastActivityUserId)
     VALUES
           (@Name
           ,@CityId
           ,@TownId
           ,@EdwNumber
           ,@IsActive
           ,@LastActivityUserId) SET @Identity=SCOPE_IDENTITY()";
            }

            Database db = DatabaseFactory.CreateDatabase();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                using (SqlCommand cmd = (SqlCommand)db.GetSqlStringCommand(sql))
                {
                    db.AddInParameter(cmd, "Name", DbType.String, item.Name);
                    db.AddInParameter(cmd, "TownId", DbType.Int32, item.TownId);
                    db.AddInParameter(cmd, "CityId", DbType.Int32, item.CityId);
                    db.AddInParameter(cmd, "EdwNumber", DbType.Int32, item.EdwNumber);
                    db.AddInParameter(cmd, "IsActive", DbType.Boolean, item.IsActive);
                    db.AddInParameter(cmd, "LastActivityUserId", DbType.Guid, item.LastActivityUserId);
                    if (!EditMode)
                    {
                        db.AddOutParameter(cmd, "Identity", DbType.Int32, 0);
                        var res = db.ExecuteNonQuery(cmd);
                        if (res > 0)
                        {
                            item.Id = (Int32)cmd.Parameters["@Identity"].Value;
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.NotFound);
                    }
                    else
                    {
                        db.AddInParameter(cmd, "Id", DbType.Int32, item.Id);
                        var res = db.ExecuteNonQuery(cmd);
                        if (res > 0)
                        {
                            scope.Complete();
                            return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.OK, null, item);
                        }
                        else return new DbHelper.DbResponse<EdwTransformerCenter>(DbHelper.DbResponseStatus.NotModified);
                    }
                }
            }
        }

        public static DbHelper.DbResponse DeleteTransformerCenter(Int32 id)
        {
            DbHelper.DbResponse retval = new DbHelper.DbResponse(DbHelper.DbResponseStatus.Unknown);
            var transCenter = GetTransformerCenter(id);
            if (transCenter == null)
                return new DbHelper.DbResponse(DbHelper.DbResponseStatus.Error, "İlişkili Kayıt Bulunamadı.");
            transCenter.IsActive = false;
            var result = SaveTransformerCenter(transCenter);
            return new DbHelper.DbResponse(result.StatusCode, result.StatusDescription);
        }


        public static Boolean Validate(EdwTransformerCenter item)
        {
            var result = TryFind(item);
            if (result == null)
                return true;
            return false;
        }
        static EdwTransformerCenter Get(ListFilter filter, EdwTransformerCenter item)
        {
            int tmp = 0;
            var ds = ListTransformerCenter2(1, 3, filter, ref tmp);
            if (ds.Tables == null || ds.Tables.Count == 0)
                return null;
            var rows = ds.Tables[0].Rows;
            if (rows == null || rows.Count == 0)
                return null;

            foreach (DataRow row in rows)
            {
                EdwTransformerCenter result = new EdwTransformerCenter();
                Read(row, result);
                if (item.Id == 0)
                    return result;
                if (item.Id != result.Id)
                    return result;
            }
            return null;
        }
        public static EdwTransformerCenter TryFind(EdwTransformerCenter item)
        {
            ListFilter filter = new ListFilter();
            EdwTransformerCenter result = null;
            if (item.EdwNumber != null && item.EdwNumber != 0)
            {
                filter.EdwNumber = item.EdwNumber;

            }
            filter.Name = item.Name;
            result = Get(filter, item);
            return result;
        }
        static void Read(DataRow row, EdwTransformerCenter item)
        {
            item.Id = row.Field<Int32>("Id");
            item.EdwNumber = row.Field<Int32?>("EdwNumber");
            item.TownId = row.Field<Int32?>("TownId");
            item.CityId = row.Field<Int32?>("CityId");
            item.Name = row.Field<string>("Name");
            item.TownName = row.Field<String>("TownName");
            item.CityName = row.Field<string>("CityName");
        }
    }
}