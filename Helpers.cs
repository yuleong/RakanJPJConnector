using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace rakan_eservices
{
    public static class Helpers
    {
        //public static string replace(this string str, string pattern, string replace)
        //{
        //    try
        //    {
        //        int s = 0;
        //        int e = 0;
        //        System.Text.StringBuilder result = new System.Text.StringBuilder();

        //        while ((e = str.IndexOf(pattern, s)) >= 0)
        //        {
        //            result.Append(str.Substring(s, e));
        //            result.Append(replace);
        //            s = e + pattern.Length;
        //        }
        //        result.Append(str.Substring(s));
        //        return result.ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        return "";
        //    }
        //}
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }
        public static String HtmlEncode(this String s)
        {
            //return HttpUtility.HtmlEncode(s);
            //return System.Security.SecurityElement.Escape(s);
            //return System.Net.WebUtility.HtmlEncode(s);
            return HttpUtility.UrlEncode(s);
        }
        public static String EscapeHtml(this String s)
        {
            return s.Replace(" ", "%20").Replace("&", "%26").Replace(",", "%2C").Replace("'", "%27").Replace("\"", "%22");
            //return System.Xml.XmlConvert.EncodeName(s);
        }
        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(System.DayOfWeek))
                {
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                    property.SetValue(item, day, null);
                }
                else
                {
                    if (row[property.Name] == DBNull.Value)
                        property.SetValue(item, null, null);
                    else
                    {
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            //nullable
                            object convertedValue = null;
                            try
                            {
                                convertedValue = System.Convert.ChangeType(row[property.Name], Nullable.GetUnderlyingType(property.PropertyType));
                            }
                            catch (Exception ex)
                            {
                            }
                            property.SetValue(item, convertedValue, null);
                        }
                        else
                            property.SetValue(item, row[property.Name], null);
                    }
                }
            }
            return item;
        }
    }
}
