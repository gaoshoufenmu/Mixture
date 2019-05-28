using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace foundation.dal
{
    public class SqlHelper
    {


        #region execute ado.net command with transaction

        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。  默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection)
        {
            object result = null;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    result = cmd.ExecuteScalar();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }
        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。  默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            object result = null;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    PrepareCommand(cmd, sqlParams);
                    result = cmd.ExecuteScalar();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }
        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。  默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            object result = null;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    PrepareCommand(cmd, dictParams);
                    result = cmd.ExecuteScalar();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }

        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数   默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection)
        {
            int affectNum = -1;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    affectNum = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数   默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            int affectNum = -1;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    PrepareCommand(cmd, sqlParams);
                    affectNum = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数   默认带事务
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            int affectNum = -1;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    trans = conn.BeginTransaction();
                    cmd.Transaction = trans as SqlTransaction;
                    PrepareCommand(cmd, dictParams);
                    affectNum = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }

        #endregion

        #region execute ado.net command with or without transaction

        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteScalar(sqlString, strConnection);
            }
            object result = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }
        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection, SqlParameter[] sqlParams, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteScalar(sqlString, strConnection, sqlParams);
            }
            object result = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    PrepareCommand(cmd, sqlParams);
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }
        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, string strConnection, IDictionary<string, object> dictParams, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteScalar(sqlString, strConnection, dictParams);
            }
            object result = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    PrepareCommand(cmd, dictParams);
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteNonQuery(sqlString, strConnection);
            }
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    affectNum = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection, SqlParameter[] sqlParams, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteNonQuery(sqlString, strConnection, sqlParams);
            }
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    PrepareCommand(cmd, sqlParams);
                    affectNum = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlString, string strConnection, IDictionary<string, object> dictParams, bool isUseTransction)
        {
            if (isUseTransction)
            {
                return ExecuteNonQuery(sqlString, strConnection, dictParams);
            }
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    conn.Open();
                    PrepareCommand(cmd, dictParams);
                    affectNum = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return affectNum;
        }

        #endregion

        #region collections

        /// <summary>
        /// 生成一个DataReader读取器 (不确定读取器对应的连接什么时候关闭 慎用)
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string sqlString, string strConnection)
        {
            IDataReader reader = null;
            SqlConnection conn = new SqlConnection(strConnection);
            SqlCommand cmd = new SqlCommand(sqlString, conn);
            PrepareCommand(cmd, Constants.CmdTimeout);
            conn.Open();
            reader = cmd.ExecuteReader();
            return reader;
        }
        /// <summary>
        /// 生成一个DataReader读取器 (不确定读取器对应的连接什么时候关闭 慎用)
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            IDataReader reader = null;
            SqlConnection conn = new SqlConnection(strConnection);
            SqlCommand cmd = new SqlCommand(sqlString, conn);
            PrepareCommand(cmd, Constants.CmdTimeout);
            conn.Open();
            PrepareCommand(cmd, sqlParams);
            reader = cmd.ExecuteReader();
            return reader;
        }
        /// <summary>
        /// 生成一个DataReader读取器 (不确定读取器对应的连接什么时候关闭 慎用)
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            IDataReader reader = null;
            SqlConnection conn = new SqlConnection(strConnection);
            SqlCommand cmd = new SqlCommand(sqlString, conn);
            PrepareCommand(cmd, Constants.CmdTimeout);
            conn.Open();
            PrepareCommand(cmd, dictParams);
            reader = cmd.ExecuteReader();
            return reader;
        }

        /// <summary>
        /// 获取一个DataSet数据集
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static DataSet QueryForDataSet(string sqlString, string strConnection)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(strConnection))
            {
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                PrepareCommand(cmd, Constants.CmdTimeout);
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            return ds;
        }
        /// <summary>
        /// 获取一个DataSet数据集
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static DataSet QueryForDataSet(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            if (sqlParams == null || sqlParams.Length == 0)
            {
                return QueryForDataSet(sqlString, strConnection);
            }
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(strConnection))
            {
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                PrepareCommand(cmd, Constants.CmdTimeout);
                PrepareCommand(cmd, sqlParams);
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            return ds;
        }
        /// <summary>
        /// 获取一个DataSet数据集
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <returns></returns>
        public static DataSet QueryForDataSet(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            if (dictParams == null || dictParams.Count == 0)
            {
                return QueryForDataSet(sqlString, strConnection);
            }
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(strConnection))
            {
                SqlCommand cmd = new SqlCommand(sqlString, conn);
                PrepareCommand(cmd, Constants.CmdTimeout);
                PrepareCommand(cmd, dictParams);
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            return ds;
        }

        /// <summary>
        /// 获取一个DataTable对象
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static DataTable QueryForDataTable(string sqlString, string strConnection)
        {
            DataTable dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;
        }
        /// <summary>
        /// 获取一个DataTable对象
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static DataTable QueryForDataTable(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            if (sqlParams == null || sqlParams.Length == 0)
            {
                return QueryForDataTable(sqlString, strConnection);
            }
            DataTable dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection, sqlParams);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }

            return dt;
        }
        /// <summary>
        /// 获取一个DataTable对象
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <returns></returns>
        public static DataTable QueryForDataTable(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            if (dictParams == null || dictParams.Count == 0)
            {
                return QueryForDataTable(sqlString, strConnection);
            }
            DataTable dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection, dictParams);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }

            return dt;
        }

        /// <summary>
        /// 获取DataTable对象集合
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <returns></returns>
        public static DataTable[] QueryForDataTables(string sqlString, string strConnection)
        {
            DataTable[] dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = new DataTable[ds.Tables.Count];
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    dt[i] = ds.Tables[i];
                }
            }
            return dt;
        }
        /// <summary>
        /// 获取DataTable对象集合
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <returns></returns>
        public static DataTable[] QueryForDataTables(string sqlString, string strConnection, SqlParameter[] sqlParams)
        {
            if (sqlParams == null || sqlParams.Length == 0)
            {
                return QueryForDataTables(sqlString, strConnection);
            }
            DataTable[] dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection, sqlParams);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = new DataTable[ds.Tables.Count];
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    dt[i] = ds.Tables[i];
                }
            }
            return dt;
        }
        /// <summary>
        /// 获取DataTable对象集合
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <returns></returns>
        public static DataTable[] QueryForDataTables(string sqlString, string strConnection, IDictionary<string, object> dictParams)
        {
            if (dictParams == null || dictParams.Count == 0)
            {
                return QueryForDataTables(sqlString, strConnection);
            }
            DataTable[] dt = null;
            DataSet ds = QueryForDataSet(sqlString, strConnection, dictParams);
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = new DataTable[ds.Tables.Count];
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    dt[i] = ds.Tables[i];
                }
            }
            return dt;
        }

        #endregion

        #region prepare parameter

        /// <summary>
        /// 准备SqlParameter参数
        /// </summary>
        /// <param name="paraNames">参数名数组</param>
        /// <param name="paraValues">参数值数组</param>
        /// <returns></returns>
        public static SqlParameter[] PrepareParameters(string[] paraNames, object[] paraValues)
        {
            if (paraNames == null)
            {
                return null;
            }
            SqlParameter[] sqlParas = new SqlParameter[paraNames.Length];
            for (int i = 0; i < paraNames.Length; i++)
            {
                sqlParas[i] = new SqlParameter(paraNames[i], paraValues[i]);
            }
            return sqlParas;
        }
        /// <summary>
        /// 准备SqlParameter参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraValue">参数值</param>
        /// <returns></returns>
        public static SqlParameter PrepareParameter(string paraName, object paraValue)
        {
            SqlParameter para = new SqlParameter(paraName, paraValue);
            return para;
        }
        /// <summary>
        /// 准备SqlParameter参数
        /// </summary>
        /// <param name="dictParams">参数字典 不可以为空</param>
        /// <returns></returns>
        public static SqlParameter[] PrepareParameters(IDictionary<string, object> dictParams)
        {
            if (dictParams == null)
            {
                return null;
            }
            SqlParameter[] sqlParas = new SqlParameter[dictParams.Count];
            int counter = 0;
            foreach (KeyValuePair<string, object> kv in dictParams)
            {
                sqlParas[counter] = new SqlParameter(kv.Key, kv.Value);
                counter++;
            }
            return sqlParas;
        }

        /// <summary>
        /// 为DbCommand对象设置参数
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="parameter">SqlParameter对象 可以为null</param>
        public static void PrepareCommand(SqlCommand cmd, SqlParameter parameter)
        {
            if (parameter != null)
            {
                cmd.Parameters.Add(parameter);
            }
        }
        /// <summary>
        /// 为DbCommand对象设置参数
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="sqlParams">SqlParameter数组 可以为null</param>
        public static void PrepareCommand(SqlCommand cmd, SqlParameter[] sqlParams)
        {
            if (sqlParams != null && sqlParams.Length > 0)
            {
                cmd.Parameters.AddRange(sqlParams);
            }
        }
        /// <summary>
        /// 为DbCommand对象设置参数
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="dictParams">参数字典 可以为null</param>
        public static void PrepareCommand(SqlCommand cmd, IDictionary<string, object> dictParams)
        {
            if (dictParams == null || dictParams.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, object> kv in dictParams)
            {
                SqlParameter param = new SqlParameter(kv.Key, kv.Value);
                cmd.Parameters.Add(param);
            }
        }
        /// <summary>
        /// 为DbCommand对象设置参数
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="paraNames"参数名数组></param>
        /// <param name="paraValues">参数值数组</param>
        public static void PrepareCommand(SqlCommand cmd, string[] paraNames, object[] paraValues)
        {
            SqlParameter[] sqlParas = PrepareParameters(paraNames, paraValues);
            if (sqlParas == null)
            {
                return;
            }
            cmd.Parameters.AddRange(sqlParas);
        }
        /// <summary>
        /// 为DbCommand对象设置参数
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="paraName">参数名</param>
        /// <param name="paraValue">参数值</param>
        public static void PrepareCommand(SqlCommand cmd, string paraName, object paraValue)
        {
            SqlParameter sqlPara = PrepareParameter(paraName, paraValue);
            cmd.Parameters.Add(sqlPara);
        }
        /// <summary>
        /// 设置在终止执行命令的尝试并生成错误之前的等待时间
        /// </summary>
        /// <param name="cmd">SqlCommand对象</param>
        /// <param name="commandTimeout">在终止执行命令的尝试并生成错误之前的等待时间，通常写在配置文件中</param>
        public static void PrepareCommand(SqlCommand cmd, int commandTimeout)
        {
            cmd.CommandTimeout = commandTimeout;
        }

        #endregion

        #region execute store procedure

        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object SPExecuteScalar(string spName, string strConnection, bool isUseTransction)
        {
            object result = null;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    result = cmd.ExecuteScalar();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }

        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object SPExecuteScalar(string spName, string strConnection, SqlParameter[] sqlParams, bool isUseTransction)
        {
            object result = null;
            IDbTransaction trans = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    PrepareCommand(cmd, sqlParams);
                    result = cmd.ExecuteScalar();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }

        /// <summary>
        ///  执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static object SPExecuteScalar(string spName, string strConnection, IDictionary<string, object> dictParams, bool isUseTransction)
        {
            IDbTransaction trans = null;
            object result = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    PrepareCommand(cmd, dictParams);
                    result = cmd.ExecuteScalar();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return result;
        }

        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int SPExecuteNonQuery(string spName, string strConnection, bool isUseTransction)
        {
            IDbTransaction trans = null;
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    affectNum = cmd.ExecuteNonQuery();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="sqlParams">SQL参数数组</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int SPExecuteNonQuery(string spName, string strConnection, SqlParameter[] sqlParams, bool isUseTransction)
        {
            IDbTransaction trans = null;
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    PrepareCommand(cmd, sqlParams);
                    affectNum = cmd.ExecuteNonQuery();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }
        /// <summary>
        /// 对连接执行SQL语句并返回受影响的行数 
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="strConnection">数据库连接字符串</param>
        /// <param name="dictParams">SQL参数字典</param>
        /// <param name="isUseTransction">是否启用事务</param>
        /// <returns></returns>
        public static int SPExecuteNonQuery(string spName, string strConnection, IDictionary<string, object> dictParams, bool isUseTransction)
        {
            IDbTransaction trans = null;
            int affectNum = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(spName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (isUseTransction)
                    {
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans as SqlTransaction;
                    }
                    PrepareCommand(cmd, Constants.CmdTimeout);
                    PrepareCommand(cmd, dictParams);
                    affectNum = cmd.ExecuteNonQuery();
                    if (isUseTransction)
                    {
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(trans, ex);
            }
            return affectNum;
        }

        #endregion

        #region batch insert

        /// <summary>
        /// 执行批量插入
        /// </summary>
        /// <param name="strConnection">sql连接字符串</param>
        /// <param name="tableName">表名称</param>
        /// <param name="dt">组装好的要批量导入的datatable</param>
        /// <returns></returns>
        public static bool BatchInsert(string strConnection, string tableName, DataTable dt)
        {
            bool flag = false;
            try
            {
                //using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(strConnection))
                    {
                        conn.Open();
                        using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                        {
                            //服务器上目标表的名称
                            sbc.DestinationTableName = tableName;
                            sbc.BatchSize = 500000;//默认一次导入500000条记录
                            sbc.BulkCopyTimeout = 300;
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                //列映射定义数据源中的列和目标表中的列之间的关系
                                sbc.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                            }
                            sbc.WriteToServer(dt);
                            flag = true;
                            //scope.Complete();//有效的事务
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return flag;
        }

        /// <summary>
        /// 执行批量插入
        /// </summary>
        /// <param name="strConnection">sql连接字符串</param>
        /// <param name="tableName">表名称</param>
        /// <param name="batchSize">一次导入记录数</param>
        /// <param name="timeout">超时之前操作完成所允许的秒数</param>
        /// <param name="dt">组装好的要批量导入的datatable</param>
        /// <returns></returns>
        public static bool BatchInsert(string strConnection, string tableName, int batchSize, int timeout, DataTable dt)
        {
            bool flag = false;
            try
            {
                //using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(strConnection))
                    {
                        conn.Open();
                        using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                        {
                            //服务器上目标表的名称
                            sbc.DestinationTableName = tableName;
                            sbc.BatchSize = batchSize;//默认一次导入num条记录
                            sbc.BulkCopyTimeout = timeout;
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                //列映射定义数据源中的列和目标表中的列之间的关系
                                sbc.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                            }
                            sbc.WriteToServer(dt);
                            flag = true;
                            //scope.Complete();//有效的事务
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return flag;
        }

        /// <summary>
        /// 执行批量插入
        /// </summary>
        /// <param name="strConnection">sql连接字符串</param>
        /// <param name="tableName">表名称</param>
        /// <param name="batchSize">一次导入记录数</param>
        /// <param name="timeout">超时之前操作完成所允许的秒数</param>
        /// <param name="rdr">datareader，必须保证是没有关闭的读取器</param>
        /// <returns></returns>
        public static bool BatchInsert(string strConnection, string tableName, int batchSize, int timeout, IDataReader rdr)
        {
            bool flag = false;
            try
            {
                //using (System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(strConnection))
                    {
                        conn.Open();
                        using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                        {
                            //服务器上目标表的名称
                            sbc.DestinationTableName = tableName;
                            sbc.BatchSize = batchSize;//默认一次导入num条记录
                            sbc.BulkCopyTimeout = timeout;
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                //列映射定义数据源中的列和目标表中的列之间的关系,自增主键没有关系，写不写都无所谓
                                sbc.ColumnMappings.Add(rdr.GetName(i), rdr.GetName(i));
                            }
                            sbc.WriteToServer(rdr);
                            rdr.Close();
                            flag = true;
                            //scope.Complete();//有效的事务
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return flag;
        }

        #endregion

        #region exception handling

        /// <summary>
        /// 异常处理 记录日志或者直接抛出
        /// </summary>
        /// <param name="ex"></param>
        private static void HandleException(Exception ex)
        {
            throw ex;
        }

        /// <summary>
        /// 异常处理  记录日志或者直接抛出
        /// </summary>
        /// <param name="trans">事务</param>
        /// <param name="ex"></param>
        private static void HandleException(IDbTransaction trans, Exception ex)
        {
            if (trans != null)
            {
                trans.Rollback();
            }
            throw ex;
        }

        /// <summary>
        /// 异常处理 记录日志或者直接抛出
        /// </summary>
        /// <param name="sqlString">出现异常的sql语句</param>
        /// <param name="sqlConn">数据库连接字符串</param>
        /// <param name="ex"></param>
        private static void HandleException(string sqlString, string sqlConn, Exception ex)
        {
            Exception innerEx = new Exception(string.Format(" 数据库连接字符串：{0}，执行SQL语句：{1}  时发生异常，异常信息：{2}", sqlConn, sqlString, ex.Message), ex);
            throw innerEx;
        }

        /// <summary>
        ///  异常处理 记录日志或者直接抛出
        /// </summary>
        /// <param name="sqlString">出现异常的sql语句</param>
        /// <param name="sqlConn">数据库连接字符串</param>
        /// <param name="trans">事务</param>
        /// <param name="ex"></param>
        private static void HandleException(string sqlString, string sqlConn, IDbTransaction trans, Exception ex)
        {
            if (trans != null)
            {
                trans.Rollback();
            }
            Exception innerEx = new Exception(string.Format(" 数据库连接字符串：{0}，执行SQL语句：{1}  时发生异常，异常信息：{2}", sqlConn, sqlString, ex.Message), ex);
            throw innerEx;
        }

        #endregion
    }

    public class Constants
    {
        private const int DefaultCmdTimeout = 60;

        public static int CmdTimeout
        {
            get
            {
                int res = 0;
                if (int.TryParse(ConfigurationManager.AppSettings["CmdTimeout"], out res))
                    return res;
                return DefaultCmdTimeout;
            }
        }
    }
}
