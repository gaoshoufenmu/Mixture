using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace foundation.dal
{
    class QueryHelper
    {
        /// <summary>
        /// 反射：指定控制绑定和由反射执行的成员和类型搜索方法的标志
        /// </summary>
        private static BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
        public static T Query<T>(IDataReader reader) where T : class, new()
        {
            IList<T> list = QueryForList<T>(reader);
            return list.FirstOrDefault();
        }
        public static IList<T> QueryForList<T>(IDataReader reader) where T : class, new()
        {
            Type type = typeof(T);
            var list = new List<T>();
            var properties = GetProperties(type);
            while (reader.Read())
            {
                var t = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string colName = reader.GetName(i).ToLower();
                    if (!properties.ContainsKey(colName))
                        continue;

                    object value = reader[i];
                    SetValue(properties, colName, t, value);
                }
                list.Add(t);
            }
            reader.Close();
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K">data type of key column</typeparam>
        /// <typeparam name="V">entity type of database record</typeparam>
        /// <param name="key">key column value</param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IDictionary<K, V> QueryForDictionary<K, V>(string key, IDataReader reader) where V : class, new()
        {
            Type type = typeof(V);
            var dict = GetProperties(type);
            IDictionary<K, V> dic = new Dictionary<K, V>();
            string keyD = key.Trim().ToLower();
            while (reader.Read())
            {
                K k = default(K);
                V v = new V();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string colName = reader.GetName(i).ToLower();
                    if (dict.ContainsKey(colName))
                    {
                        object val = reader[i];
                        if (keyD == colName)
                            k = (K)val;

                        SetValue(dict, colName, v, val);
                    }
                }
                dic.Add(k, v);
            }
            reader.Close();
            return dic;
        }
        public static IList<T> QueryForList<T>(string sql, string conn, SqlParameter[] paramArr) where T : class, new()
        {
            IList<T> list = new List<T>();
            Type type = typeof(T);
            var dict = GetProperties(type);

            using (var sqlConn = new SqlConnection(conn))
            {
                var cmd = new SqlCommand(sql, sqlConn);
                SqlHelper.PrepareCommand(cmd, Constants.CmdTimeout);
                SqlHelper.PrepareCommand(cmd, paramArr);
                sqlConn.Open();
                var reader = cmd.ExecuteReader();

                return QueryForList<T>(reader);
            }
        }
        public static T Query<T>(string sql, string conn, SqlParameter[] paramArr) where T : class, new()
        {
            return QueryForList<T>(sql, conn, paramArr).FirstOrDefault();
        }
        public static IDictionary<K, V> QueryForDictionary<K, V>(string key, string sql, string conn, SqlParameter[] paramArr) where V : class, new()
        {
            var dict = GetProperties(typeof(V));
            var dic = new Dictionary<K, V>();

            using (var sqlConn = new SqlConnection(conn))
            {
                var cmd = new SqlCommand(sql, sqlConn);
                SqlHelper.PrepareCommand(cmd, Constants.CmdTimeout);
                SqlHelper.PrepareCommand(cmd, paramArr);
                sqlConn.Open();
                var reader = cmd.ExecuteReader();

                return QueryForDictionary<K, V>(key, reader);
            }
        }

        public static T Query<T>(string sql, string conn, string[] paramNames, object[] paramVals) where T : class, new()
        {
            var sqlParams = SqlHelper.PrepareParameters(paramNames, paramVals);
            return Query<T>(sql, conn, sqlParams);
        }
        public static IList<T> QueryForList<T>(string sql, string conn, string[] paramNames, object[] paramVals) where T : class, new()
        {
            var sqlParams = SqlHelper.PrepareParameters(paramNames, paramVals);
            return QueryForList<T>(sql, conn, sqlParams);
        }
        public static IDictionary<K, V> QueryForDictionary<K, V>(string key, string sql, string conn, string[] paramNames, object[] paramVals) where V : class, new()
        {
            return QueryForDictionary<K, V>(key, sql, conn, SqlHelper.PrepareParameters(paramNames, paramVals));
        }
        public static T Query<T>(DataTable dt) where T : class, new()
        {
            return QueryForList<T>(dt).FirstOrDefault();
        }
        public static IList<T> QueryForList<T>(DataTable dt) where T : class, new()
        {
            var list = new List<T>();
            if (dt == null || dt.Rows.Count == 0)
                return list;

            var dict = GetProperties(typeof(T));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var t = new T();
                DataRow dr = dt.Rows[i];
                foreach (DataColumn dc in dt.Columns)
                {
                    string colName = dc.ColumnName;
                    if (dict.ContainsKey(colName))
                    {
                        object val = dr[colName];
                        SetValue(dict, colName, t, val);
                    }
                }
                list.Add(t);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K">data type of key column</typeparam>
        /// <typeparam name="V">entity type of database record</typeparam>
        /// <param name="key">key column value</param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IDictionary<K, V> QueryForDictionary<K, V>(string key, DataTable dt) where V : class, new()
        {
            var dic = new Dictionary<K, V>();
            if (dt == null || dt.Rows.Count == 0)
                return dic;

            string keyD = key.Trim().ToLower();
            var dict = GetProperties(typeof(V));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                K k = default(K);
                V v = new V();

                DataRow dr = dt.Rows[i];
                foreach (DataColumn dc in dt.Columns)
                {
                    string colName = dc.ColumnName;
                    if (dict.ContainsKey(colName))
                    {
                        object val = dr[colName];
                        SetValue(dict, colName, v, val);

                        if (keyD == colName)
                            k = (K)val;
                    }
                }
                dic.Add(k, v);
            }
            return dic;
        }
        #region Auxiliary methods
        private static IDictionary<string, PropertyInfo> GetProperties(Type type)
        {
            IDictionary<string, PropertyInfo> dict = new Dictionary<string, PropertyInfo>();
            var properties = type.GetProperties(bf);
            foreach (var p in properties)
            {
                if (string.IsNullOrEmpty(p.Name))
                    continue;

                if (!dict.ContainsKey(p.Name.ToLower()))
                    dict.Add(p.Name.ToLower(), p);
            }
            return dict;
        }
        private static void SetValue(IDictionary<string, PropertyInfo> dict, string colName, object obj, object val)
        {
            if (val == null || val == DBNull.Value)
                return;

            if (dict.ContainsKey(colName))
            {
                try
                {
                    dict[colName].SetValue(obj, val, null);
                }
                catch
                {
                    try
                    {
                        var realVal = Convert.ChangeType(val, dict[colName].PropertyType);
                        dict[colName].SetValue(obj, realVal, null);
                    }
                    catch
                    { }
                }
            }
        }
        #endregion
    }
}
