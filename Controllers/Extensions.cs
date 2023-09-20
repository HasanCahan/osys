using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Collections;
using System.Globalization;
using System.Data.OleDb;

namespace OlcuYonetimSistemi
{
    public static class Extensions
    {

        public static List<T> ToList<T>(this DataTable dt, Dictionary<string, string> objectMap)
        {
            if (dt == null)
                return new List<T>();
            return GetObjects<T>(DataTableToListDict(dt, objectMap));
        }

        public static List<T> ToList<T>(this DataSet ds, Dictionary<string, string> objectMap)
        {
            if (ds == null)
                return new List<T>();
            var tables = ds.Tables;
            if (tables == null || tables.Count == 0)
                return new List<T>();
            return GetObjects<T>(DataTableToListDict(tables[0], objectMap));
        }

        public static List<Dictionary<string, object>> DataTableToListDict(this DataTable pDt, Dictionary<string, string> objectMap)
        {
            try
            {
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in pDt.Rows)
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    foreach (DataColumn col in pDt.Columns)
                    {
                        var mapped = objectMap.GetMappedProperty(col.ColumnName);
                        if (row.ContainsKey(mapped))
                            continue;
                        row.Add(mapped, dr[col]);
                    }
                    rows.Add(row);
                }
                return rows;
            }
            catch (Exception)
            {
                throw;
            }
        }
        static string GetMappedProperty(this Dictionary<string, string> objectMap, string objectKey)
        {
            if (objectMap == null)
                return objectKey;
            if (string.IsNullOrEmpty(objectKey))
                return objectKey;
            var key = objectMap.FirstOrDefault(s => s.Value != null && s.Value.Equals(objectKey, StringComparison.CurrentCultureIgnoreCase));
            if (!string.IsNullOrEmpty(key.Key))
                return key.Key;
            return objectKey;
        }
        public static List<T> GetObjects<T>(this List<Dictionary<string, object>> pDicts)
        {
            try
            {
                List<T> objs = (List<T>)Activator.CreateInstance(typeof(List<T>));
                var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (Dictionary<string, object> dict in pDicts)
                {
                    T obj = (T)Activator.CreateInstance(typeof(T));
                    dict.Remove("OrderRank");

                    foreach (KeyValuePair<string, object> kvp in dict)
                    {
                        if (kvp.Value != DBNull.Value)
                        {
                            var prop = props.FirstOrDefault(s => s.Name.Equals(kvp.Key, StringComparison.CurrentCultureIgnoreCase));
                            if (prop == null)
                                continue;/// veritabanında baska tablodan join edilen columnlar veya baska varyasyonlar icin modelde karsılıgı yoksa es geçsin.
                            prop.SetValue(obj, kvp.Value.TryParse(prop.PropertyType), null);
                        }
                    }
                    objs.Add(obj);
                }
                return objs;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static object TryParse(this Object obj, Type destination)
        {
            var convertable = obj as IConvertible;
            if (convertable == null)
                return obj;
            if (obj == null)
                return null;
            if (obj.GetType() == destination)
                return obj;
            if (destination == typeof(Guid))
                return new Guid(obj.ToString());
            Object result;
            TryChangeType<IConvertible>(convertable, out result, destination);
            return result;
        }
        public static bool TryChangeType<T>(this T input, out Object output, Type destinationType) where T : IConvertible
        {
            bool result = false;
            Type type = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
            try
            {

                output = Convert.ChangeType(input, type);
                result = true;
            }
            catch (Exception e)
            {
                output = Instance(type);
            }
            return result;
        }
        public static Object Instance(this Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }
        public static T GetEnumFromInt<T>(this IDataReader reader, string fieldName) where T : struct
        {
            try
            {
                var value = reader.GetValue<Int32>(fieldName);
                return (T)Enum.ToObject(typeof(T), value);
            }
            catch
            {
                return default(T);
            }
        }

        public static T GetValue<T>(this IDataReader reader, string fieldName)
        {
            return GetValue<T>(reader, fieldName, null);
        }
        public static T GetValue<T>(this System.Data.Common.DbDataReader reader, string fieldName)
        {
            return GetValue<T>(reader, fieldName, null);
        }
        public static Object GetValue(this IDataReader reader, string fieldName, string format, Type inpT)
        {
            bool isNullable = inpT.IsGenericType && inpT.GetGenericTypeDefinition() == typeof(Nullable<>);
            Type realType = isNullable ? Nullable.GetUnderlyingType(inpT) : inpT;

            object value = reader[fieldName] == DBNull.Value ? (isNullable || !realType.IsValueType ? null : Activator.CreateInstance(realType)) : reader[fieldName];
            if (value != null && String.IsNullOrWhiteSpace(format) == false)
            {
                if (realType == typeof(DateTime))
                {
                    DateTime dtOut = default(DateTime);

                    if (DateTime.TryParseExact((value ?? String.Empty).ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtOut))
                    {
                        value = dtOut;
                    }

                }
            }

            if (isNullable)
            {
                if (value == null)
                {
                    return Activator.CreateInstance(inpT, null);
                }
                else return Activator.CreateInstance(inpT, Convert.ChangeType(value, realType));
            }
            else return Convert.ChangeType(value, realType);
        }
        public static T GetValue<T>(this IDataReader reader, string fieldName, string format)
        {
            return (T)GetValue(reader, fieldName, format, typeof(T));
        }
        internal static string ToDateTimeParse(object value, string format)
        {
            DateTime dtOut = default(DateTime);

            if (DateTime.TryParseExact((value ?? String.Empty).ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dtOut))
            {
                value = dtOut;
            }
            return value.ToString();

        }
        public static string ToIntString(this Enum e)
        {
            return Convert.ToInt32(e).ToString();
        }

        public static bool IsIn(this string text, bool ignoreCase, params string[] items)
        {
            if (items == null || items.Length == 0) return String.IsNullOrEmpty(text);
            if (String.IsNullOrEmpty(text)) return false;

            StringComparer comparer = StringComparer.Create(new CultureInfo("tr-TR"), ignoreCase);
            return items.Contains(text, comparer);
        }
    }


}
