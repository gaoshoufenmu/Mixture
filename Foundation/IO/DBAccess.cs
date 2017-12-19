using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Dapper;
namespace Foundation.IO
{
    /// <summary>
    /// 数据库访问
    /// </summary>
    public class DBAccess
    {
        private string[] conn_strs;
        public DBAccess(params string[] conn_strs)
        {
            this.conn_strs = conn_strs;
        }

        #region dapper
        public IEnumerable<T> SelectMany_d<T>(string query, int conn_idx = 0)
        {
            using (var conn = new SqlConnection(conn_strs[conn_idx]))
                return conn.Query<T>(query);
        }

        public T Select_d<T>(string query, int conn_idx = 0)
        {
            using (var conn = new SqlConnection(conn_strs[conn_idx]))
                return conn.QueryFirst<T>(query);
        }

        public void Execute_d(string sql, int conn_idx = 0)
        {
            using (var conn = new SqlConnection(conn_strs[conn_idx]))
                conn.Execute(sql);
        }
        #endregion

        #region ado.net
        public IEnumerable<T> SelectMany_a<T>(string query, int conn_idx = 0)
        {
            using (var conn = new SqlConnection(conn_strs[conn_idx]))
            {
                using (var sda = new SqlDataAdapter(query, conn))
                {
                    var ds = new DataSet();
                    sda.Fill(ds);
                    if(ds.Tables[0].Rows.Count > 0)
                    {
                        var list = new List<T>();
                        foreach(DataRow dr in ds.Tables[0].Rows)
                        {
                            list.Add(IL.Proxy.CreateEntity<T>(dr));
                        }
                        return list;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
