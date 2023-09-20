using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace OlcuYonetimSistemi
{
    public class EnumSource
    {
        public EnumSource()
        {

        }
        public EnumSource(object val, string text)
        {
            Value = val.ToString();
            Text = text;
        }
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public static class EnumHelper
    {
        public static T ParseEnum<T>(this int val) where T : struct
        {
            try
            {
                return (T)Enum.ToObject(typeof(T), val);
            }
            catch
            {
                return default(T);
            }
        }
        public static T ParseEnum<T>(this Object val) where T : struct
        {
            if (val == null)
                return default(T);
            return ParseEnum<T>(val.ToString());
        }
        public static T ParseEnum<T>(this string val) where T : struct
        {
            try
            {
                if (string.IsNullOrEmpty(val))
                    return default(T);
                int intVal;
                var isInt = Int32.TryParse(val, out intVal);
                if (isInt)
                    return ParseEnum<T>(intVal);
                return (T)Enum.ToObject(typeof(T), val);
            }
            catch
            {
                return default(T);
            }
        }
        public static string GetDisplayName<T>(this Object val) where T : struct
        {
            if (val == null)
                return string.Empty;
            var enumVal = val.ParseEnum<T>();
            return enumVal.GetDisplayName();
        }
        public static string GetDisplayName(this Object enumVal)
        {
            if (enumVal == null)
                return string.Empty;
            var type = enumVal.GetType();
            if (!type.IsEnum)
                return enumVal.ToString();
            var memInfo = type.GetMember(enumVal.ToString());
            if (memInfo == null || memInfo.Length == 0)
                return enumVal.ToString();
            var attributes = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
            var attr = (attributes.Length > 0) ? (DisplayAttribute)attributes[0] : null;
            if (attr == null)
                return enumVal.ToString();
            return attr.Name;
        }
        public static List<EnumSource> Enumerate<T>(Boolean intValue = true) where T : struct
        {
            try
            {
                List<EnumSource> result = new List<EnumSource>();
                var vals = Enum.GetValues(typeof(T));
                foreach (var itm in vals)
                {
                    var intVal = (int)itm;
                    if (intVal == 0)
                        continue;
                    var enumSource = intValue ? new EnumSource((int)itm, itm.GetDisplayName()) : new EnumSource(itm.ToString(), itm.GetDisplayName());
                    result.Add(enumSource);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}