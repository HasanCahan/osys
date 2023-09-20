using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VB = Microsoft.VisualBasic;

namespace OlcuYonetimSistemi
{
    public class Information
    {
        public static bool IsNumeric(object Expression)
        {
            return VB.Information.IsNumeric(Expression);
        }
        public static bool IsDouble(object Expression)
        {
            double mydouble;
            return Double.TryParse(Convert.ToString(Expression ?? String.Empty).Trim(),
                System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite
                , Helper.trCulture, out mydouble);
        }
        public static bool IsFindCharInNumeric(string Text)
        {
            foreach (char chr in Text)
            {
                if (Char.IsNumber(chr))
                    continue;
                return true;
            }
            return false;
        }
    }

    public static class Helper
    {
        public readonly static System.Globalization.CultureInfo enCulture = new System.Globalization.CultureInfo("en-US");
        public readonly static System.Globalization.CultureInfo trCulture = new System.Globalization.CultureInfo("tr-TR");

        public static Nullable<double> ConvertDouble(object Expression)
        {
            double mydouble;
            if (Double.TryParse(Convert.ToString(Expression ?? String.Empty).Trim(),
                System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite
                , Helper.trCulture, out mydouble)) return mydouble;
            else return null;
        }
        /*
        [Description("N >= from and N <= to")]
        public static bool Between(this double number, double from, double to)
        {
            return (number >= from && number <= to);
        }*/

        [Description("N >= from and N <= to")]
        public static bool Between<T>(this T value, T from, T to) where T : IComparable<T>
        {
            return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
        }

        #region "NotifyJS"
        public enum NotifyJSType
        {
            Info,
            Danger,
            Success
        }
        public static string GetNotifyJS(NotifyJSType type, string message, bool onjQueryReady)
        {
            string js = String.Format(@"$.bootstrapGrowl({0}, {{ type: '{1}', align: 'center', width: 'auto', offset: {{from: 'top', amount: 25}} }});"
                , HttpUtility.JavaScriptStringEncode(message, true)
                , type.ToString().ToLower(enCulture));
            if (onjQueryReady) js = String.Format(@"$(function() {{{1}{0}{1}}});", js, Environment.NewLine);
            return js;
        }
        #endregion "NotifyJS"
    }

    public class DbHelper
    {
        #region "SQL Filter"
        [Serializable]
        public enum SQLFilterCompareOperator : int
        {
            NotEqual = 0,       //<> (Not Equal To)
            Equal = 1,          //= (Equals)
            Greater = 2,        //> (Greater Than)
            GreaterEqual = 3,   //>= (Greater Than or Equal To)
            Less = 4,           //< (Less Than)
            LessEqual = 5,      //<= (Less Than or Equal To)
            In = 6,
            NotIn = 7,
            IsNull = 8,
            IsNotNull = 9,
            Between = 10,
            NotBetween = 11,
            Like
        }

        [Serializable]
        public enum SQLFilterConcatOperator : int
        {
            None = 0,
            And = 1,
            Or = 2
        }

        [Serializable]
        public abstract class SQLFilterBase
        {
            public SQLFilterConcatOperator ConcatOperator { get; set; }
        }

        [Serializable]
        public class SQLFilterItem : SQLFilterBase
        {
            public string FieldName { get; set; }
            public SQLFilterCompareOperator CompareOperator { get; set; }
            public object[] Values { get; set; }

            public SQLFilterItem()
            {
                this.ConcatOperator = SQLFilterConcatOperator.And;
                this.CompareOperator = SQLFilterCompareOperator.Equal;
                this.Values = new object[] { };
            }
        }

        [Serializable]
        public class SQLFilterGroup : SQLFilterBase
        {
            public SQLFilterGroup()
            {
                this.Items = new List<SQLFilterBase>();
            }
            public List<SQLFilterBase> Items { get; set; }

            public SQLFilterItem AddItem(SQLFilterConcatOperator concatOperator, string fieldName, SQLFilterCompareOperator compareOperator, params object[] values)
            {
                SQLFilterItem item = new SQLFilterItem()
                {
                    ConcatOperator = concatOperator,
                    FieldName = fieldName,
                    CompareOperator = compareOperator,
                    Values = values
                };
                this.Items.Add(item);
                return item;
            }
        }

        [Serializable]
        public class SQLFilterBuilder
        {
            private SQLFilterGroup m_Filter = null;
            private List<SqlParameter> m_Params = null;
            private string m_Prefix = String.Empty;

            public SQLFilterBuilder(SQLFilterGroup filter)
            {
                m_Filter = filter;
                m_Params = new List<SqlParameter>();
                m_Prefix = "P" + Guid.NewGuid().ToString("D").Split('-')[1] + "$";
            }

            public override string ToString()
            {
                m_Params.Clear();
                StringBuilder sb = new StringBuilder();
                SQLFilterToString(m_Filter, 0, ref sb, ref m_Params);
                return sb.ToString();
            }

            private static string ConcatOperatorResolver(SQLFilterConcatOperator opr)
            {
                switch (opr)
                {
                    case SQLFilterConcatOperator.And:
                        return " AND ";
                    case SQLFilterConcatOperator.Or:
                        return " OR ";
                    default:
                        return String.Empty;
                }
            }

            private static string CompareOperatorResolver(SQLFilterCompareOperator opr)
            {
                switch (opr)
                {
                    case SQLFilterCompareOperator.Equal:
                        return "=";
                    case SQLFilterCompareOperator.NotEqual:
                        return "<>";
                    case SQLFilterCompareOperator.Greater:
                        return ">";
                    case SQLFilterCompareOperator.GreaterEqual:
                        return ">=";
                    case SQLFilterCompareOperator.Less:
                        return "<";
                    case SQLFilterCompareOperator.LessEqual:
                        return "<=";
                    case SQLFilterCompareOperator.IsNull:
                        return " IS NULL ";
                    case SQLFilterCompareOperator.IsNotNull:
                        return " IS NOT NULL";
                    case SQLFilterCompareOperator.In:
                        return " IN";
                    case SQLFilterCompareOperator.NotIn:
                        return " NOT IN ";
                    case SQLFilterCompareOperator.Between:
                        return " BETWEEN ";
                    case SQLFilterCompareOperator.NotBetween:
                        return " NOT BETWEEN ";
                    case SQLFilterCompareOperator.Like:
                        return " LIKE ";
                    default:
                        return String.Empty;
                }
            }

            private string CreateParameter(object value)
            {
                string prmName = this.m_Prefix + (this.m_Params.Count + 1).ToString();
                this.m_Params.Add(new SqlParameter(prmName, value));
                return "@" + prmName;
            }

            private void SQLFilterToString(SQLFilterBase filter, int index, ref StringBuilder sb, ref List<SqlParameter> prms)
            {
                if (filter == null || (filter is SQLFilterGroup && ((filter as SQLFilterGroup).Items == null || (filter as SQLFilterGroup).Items.Count == 0))) return;

                if (filter is SQLFilterGroup)
                {
                    SQLFilterGroup group = (filter as SQLFilterGroup);
                    if (sb.Length > 0 && index > 0) sb.Append(ConcatOperatorResolver(group.ConcatOperator));
                    if (group.Items.Count > 1) sb.Append("(");
                    for (int idx = 0; idx < group.Items.Count; idx++)
                    {
                        SQLFilterToString(group.Items[idx], idx, ref  sb, ref prms);
                    }
                    if (group.Items.Count > 1) sb.Append(")");
                }
                else if (filter is SQLFilterItem)
                {
                    SQLFilterItem item = (filter as SQLFilterItem);
                    if (String.IsNullOrWhiteSpace(item.FieldName)) throw new ArgumentNullException("Boş bırakılmış 'FieldName' bulundu.");

                    if (sb.Length > 0 && index > 0) sb.Append(ConcatOperatorResolver(item.ConcatOperator));
                    switch (item.CompareOperator)
                    {
                        case SQLFilterCompareOperator.Between:
                        case SQLFilterCompareOperator.NotBetween:
                            if (item.Values == null || item.Values.Length != 2) throw new ArgumentOutOfRangeException(item.FieldName, "'Between' operatörü için 2 adet değer gereklidir.");
                            sb.Append(String.Format("{0}{1} ({2} AND {3})", item.FieldName, CompareOperatorResolver(item.CompareOperator), CreateParameter(item.Values[0]), CreateParameter(item.Values[1])));
                            break;
                        case SQLFilterCompareOperator.IsNull:
                        case SQLFilterCompareOperator.IsNotNull:
                            sb.Append(item.FieldName + CompareOperatorResolver(item.CompareOperator));
                            break;
                        case SQLFilterCompareOperator.In:
                        case SQLFilterCompareOperator.NotIn:
                            if (item.Values == null || item.Values.Length == 0) throw new ArgumentOutOfRangeException(item.FieldName, "'In' operatörü için en az 1 adet değer gereklidir.");
                            sb.Append(String.Format("{0}{1}({2})", item.FieldName, CompareOperatorResolver(item.CompareOperator),
                                String.Join(", ", item.Values.Select(x => CreateParameter(x)).ToArray())
                                ));
                            break;
                        default:
                            if (item.Values == null || item.Values.Length != 1) throw new ArgumentOutOfRangeException(item.FieldName, "Karşılaştrma operatörü için 1 adet değer gereklidir.");
                            sb.Append(String.Format("{0}{1}{2}", item.FieldName, CompareOperatorResolver(item.CompareOperator), CreateParameter(item.Values[0])));
                            break;
                    }
                }
                else throw new ArgumentException(String.Format("Eklenen '{0}' tipinde filtre tanınmıyor.", filter.GetType().UnderlyingSystemType.Name));
            }

            public SqlParameter[] Parameters { get { return m_Params.ToArray(); } }
        }
        #endregion "SQL Filter"

        public static Dictionary<string, string> AllTables = new Dictionary<string, string>() {
                    {"tEquipment","Ekipman"},
                    {"tMeteredArea","Ölçüm Sahası"},
                    {"tMeterPoint","Ölçüm Noktası"},
                    {"tReadout","Manuel Veri"},
                    {"tCalcParam","Parametre"},
                    {"tCity","Şehir"},
                    {"tTown","İlçe"}
                };

        [Serializable]
        public enum DbResponseStatus
        {
            Unknown = 0,
            OK = 200,
            //Found = 302,
            NotFound = 404,
            NotModified = 304,
            Conflict = 409,
            //Unauthorized = 401,
            BadRequest = 400,
            Error = 500
        }

        [Serializable]
        public class DbResponse<TOut>
        {
            public DbResponse()
            {
                this.StatusCode = DbResponseStatus.Unknown;
            }
            public DbResponse(DbResponseStatus status)
            {
                this.StatusCode = status;
            }
            public DbResponse(DbResponseStatus status, string description)
            {
                this.StatusCode = status;
                this.StatusDescription = description;
            }
            public DbResponse(DbResponseStatus status, string description, TOut data)
            {
                this.StatusCode = status;
                this.StatusDescription = description;
                this.Data = data;
            }

            public DbResponseStatus StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public TOut Data { get; set; }
        }

        [Serializable]
        public class DbResponse
        {
            public DbResponse()
            {
                this.StatusCode = DbResponseStatus.Unknown;
            }
            public DbResponse(DbResponseStatus status)
            {
                this.StatusCode = status;
            }
            public DbResponse(DbResponseStatus status, string description)
            {
                this.StatusCode = status;
                this.StatusDescription = description;
            }

            public DbResponseStatus StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }

        public static string SqlConflictParser(string message)
        {
            string retval = null;

            Regex rgx = new Regex(@"(?:\sconflict\s).*(?:\stable\s)(""[\[\w\.\]]*"")", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match match = rgx.Match(message);
            if (match.Success)
            {
                string tname = match.Groups[1].Value.Split('.').Last().Trim('\n', '\r', '\t', '"', ' ');
                string tvalue;
                if (!AllTables.TryGetValue(tname, out tvalue)) tvalue = tname;

                retval = String.Format("{0} içerisindeki ilişkili kayıtlar silinmeden bu işlem gerçekleştirilemez.", tvalue);
            }

            return retval;
        }
    }
}