using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace rakan_eservices
{
    public class Database
    {
        public delegate void OnInfoLog(string info);
        public event OnInfoLog InfoLog;

        string ConnStr = System.Configuration.ConfigurationManager.AppSettings["ConnString"];
        SqlConnection conn = default(SqlConnection);
        SqlCommand cmd = default(SqlCommand);
        SqlDataAdapter da = default(SqlDataAdapter);
        SqlTransaction trx = default(SqlTransaction);
        public Database()
        {
            conn = new SqlConnection();
            cmd = new SqlCommand();
            da = new SqlDataAdapter();
            conn = new SqlConnection(ConnStr);
            cmd.Connection = conn;
            InfoLog?.Invoke("Connection has been established.");
        }
        public void Disconnect()
        {
            conn.Close();
            conn.Dispose();
        }
        public DataSet Execute_FillDS(string query, bool enableLog = true)
        {
            DataSet ds = new DataSet();
            string SQLStr = "";
            try
            {
                if(enableLog) InfoLog?.Invoke(query);
                if (conn.State == ConnectionState.Closed) conn.Open();
                SQLStr = query;
                cmd.CommandText = SQLStr;
                da.SelectCommand = cmd;
                da.Fill(ds);

            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"Exception to get recordset. {ex.Message}");
                ds = null;
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return ds;
        }
        public string Execute_NonQuery(string query, bool IsReturnValue = false)
        {
            string result = "";
            try
            {
                InfoLog?.Invoke(query);
                if (conn.State == ConnectionState.Closed) conn.Open();
                cmd.CommandText = query;
                if (IsReturnValue == true)
                {
                    result = Convert.ToString(cmd.ExecuteNonQuery());
                    InfoLog?.Invoke($"Affected record: {result}");
                }
                else
                {
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"Exception when executing non query statement. {ex.Message}");
                result = "-1";
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return result;
        }
        public string Execute_NonQuery2(string query)
        {
            string result = "0";
            try
            {
                InfoLog?.Invoke(query);
                cmd.CommandText = query;

            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"Exception when executing non query statement2. {ex.Message}");
                result = "-1";
            }
            return result;
        }
        public int Execute_NonQueryResult(string query)
        {
            int count = 0;
            try
            {
                InfoLog?.Invoke(query);
                if (conn.State == ConnectionState.Closed) conn.Open();
                cmd.CommandText = query;
                count = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"Exception when executing non query statement. {ex.Message}");
                count = 0;
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return count;
        }
        public object Execute_Scalar(string query)
        {
            object result = null;
            try
            {
                InfoLog?.Invoke(query);

                if (conn.State == ConnectionState.Closed) conn.Open();
                cmd.CommandText = query;
                result = cmd.ExecuteScalar();
                if (result == null)
                {
                    result = "";
                }

            }
            catch (Exception ex)
            {
                InfoLog?.Invoke($"Exception executing scalar statement. {ex.Message}");
                result = "-1";
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return result;
        }
        public string SQLQ(object data)
        {
            return string.IsNullOrEmpty(data.ToString()) ? "" : "'" + data + "'";
        }
        public string SQLS(string data)
        {
            return string.IsNullOrEmpty(data) ? "" : "N'" + data.Replace("'", "''") + "'";
        }
        public string SQLN(string data)
        {
            return string.IsNullOrEmpty(data) ? data : data.Replace("'", "''");
        }
    }
}
