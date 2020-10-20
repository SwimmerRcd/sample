using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json;
using static System.Console;

namespace Utils
{
    public static class OracleHelper
    {
        public static string DataTableToJson(DataTable table)
        {
            string JsonString = string.Empty;
            JsonString = JsonConvert.SerializeObject(table);
            return JsonString;
        }

        public static List<T> DataTableToList<T>(DataTable table)
        {
            string json = DataTableToJson(table);
            List<T> list = JsonConvert.DeserializeObject<List<T>>(json);
            return list;
        }

        /// <summary>
        /// 输入一个select语句,返回一个DataTable.
        /// </summary>
        /// <param name="querySql">select语句</param>
        /// <param name="cmdParms">select语句中的参数</param>
        /// <param name="conn">数据库Connection对象</param>
        /// <returns>结果集DataTable</returns>
        public static DataTable ReadTable(string querySql, OracleParameter[] cmdParams, OracleConnection conn)
        {
            DataTable dt = new DataTable();
            DbDataReader reader = null;
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(querySql, cmdParams, cmd, CommandType.Text, conn, null);
            reader = cmd.ExecuteReader();
            //组织DT表结构
            int fieldc = reader.FieldCount;
            for (int i = 0; i < fieldc; i++)
            {
                DataColumn dc = new DataColumn(reader.GetName(i), reader.GetFieldType(i));
                dt.Columns.Add(dc);
            }
            //将数据行逐条加入DT
            while (reader.Read())
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < fieldc; i++)
                {
                    dr[i] = reader[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 输入一个select语句,返回一个DataTable.
        /// </summary>
        /// <param name="querySql">select语句</param>
        /// <param name="cmdParms">select语句中的参数</param>
        /// <param name="conn">数据库Connection对象</param>
        /// <returns>结果集DataTable</returns>
        public static IList<T> ReadTableToList<T>(string querySql, OracleParameter[] cmdParams, OracleConnection conn)
        {
            DataTable dt = ReadTable(querySql, cmdParams, conn);
            return ConvertTo<T>(dt);
        }

        /// <summary>
        /// 将结果集DataTable在控制台上打印出来
        /// </summary>
        /// <param name="dt">结果集DataTable</param>
        public static void PrintDataTable(DataTable dt)
        {
            foreach (DataRow vDr in dt.Rows)
            {
                string rec = vDr[0].ToString();
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    rec = rec + ", " + vDr[i];
                }
                rec = rec + ";";
                WriteLine(rec);
                rec = "";
            }
            ReadLine();
        }

        #region 将DataTable转化为List
        public static IList<T> ConvertTo<T>(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            List<DataRow> rows = new List<DataRow>();

            foreach (DataRow row in table.Rows)
            {
                rows.Add(row);
            }

            return ConvertTo<T>(rows);
        }

        public static IList<T> ConvertTo<T>(IList<DataRow> rows)
        {
            IList<T> list = null;

            if (rows != null)
            {
                list = new List<T>();

                foreach (DataRow row in rows)
                {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }

        public static T CreateItem<T>(DataRow row)
        {
            T obj = default(T);
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
                    try
                    {
                        object value = row[column.ColumnName];
                        prop.SetValue(obj, value, null);
                    }
                    catch
                    {  //You can log something here     
                       //throw;    
                    }
                }
            }

            return obj;
        }
        #endregion 将DataTable转化为List

        /// <summary>
        /// 执行增删改的sql
        /// </summary>
        /// <param name="zsgSql">update/insert/delete语句</param>
        /// <param name="cmdParms">sql语句中的变量</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务（可选：
        ///                                  传入非空则事务由传入方控制提交; 
        ///                                  为空则自建事务直接提交</param>
        /// <param name="commit">事务是否提交（可选：默认执行后就提交</param>                                
        /// <returns></returns>
        public static int ExecuteSql(string zsgSql, OracleParameter[] cmdParams, OracleConnection conn, OracleTransaction transaction = null, bool commit = true)
        {
            OracleTransaction trans = transaction ?? conn.BeginTransaction();
            using (OracleCommand cmd = new OracleCommand())
            {
                try
                {
                    PrepareCommand(zsgSql, cmdParams, cmd, CommandType.Text, conn, trans);
                    int affectedCounts = cmd.ExecuteNonQuery();
                    if (transaction == null || commit)
                        trans.Commit();
                    cmd.Parameters.Clear();
                    return affectedCounts;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    if (transaction == null)
                        trans.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行存储过程Procedure
        /// </summary>
        /// <param name="procName">Procedure名</param>
        /// <param name="cmdParams">Procedure的参数列表</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="transaction">事务（可选：
        ///                                  传入非空则事务由传入方控制提交; 
        ///                                  为空则自建事务直接提交</param>
        /// <param name="commit">事务是否提交（可选：默认执行后就提交</param>  
        /// <returns>0 成功； 其他 失败</returns>
        public static int ExecuteProc(string procName, OracleParameter[] cmdParams, OracleConnection conn, OracleTransaction transaction = null, bool commit = true)
        {
            OracleTransaction trans = transaction ?? conn.BeginTransaction();
            using (OracleCommand cmd = new OracleCommand())
            {
                try
                {
                    PrepareCommand(procName, cmdParams, cmd, CommandType.StoredProcedure, conn, trans);
                    int affectedCounts = cmd.ExecuteNonQuery();
                    if (transaction == null || commit)
                        trans.Commit();
                    cmd.Parameters.Clear();
                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    cmd.Dispose();
                    if (transaction == null)
                        trans.Dispose();
                }
            }
        }

        /// <summary>
        /// 打开一个数据库连接并返回
        /// </summary>
        /// <returns>打开的数据库连接</returns>
        public static OracleConnection OpenConn()
        {
            OracleConnection conn = new OracleConnection(); //"Data Source = (DESCRIPTION =\n(ADDRESS_LIST =\n(ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.57.9)(PORT = 1521))\n)\n(CONNECT_DATA =\n(SERVER = DEDICATED)\n(SERVICE_NAME = mstr9)\n)\n); User Id = db_config; Password = lmis; "
            conn.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=lmis_local)));Persist Security Info=True;User ID=lmis;Password=lmis6tsl;"; //
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 将sql语句绑定到指定连接的指定事务上
        /// </summary>
        /// <param name="cmdText">数据库命令sql语句</param>
        /// <param name="cmdParms">sql语句参数</param>
        /// <param name="cmd">数据库命令对象</param>
        /// <param name="cmdType">数据库命令类型</param>
        /// <param name="conn">数据库connection对象</param>
        /// <param name="trans">数据库事务对象</param>
        private static void PrepareCommand(string cmdText, OracleParameter[] cmdParms, OracleCommand cmd, CommandType cmdType, OracleConnection conn, OracleTransaction trans)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.BindByName = true;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                cmd.Parameters.AddRange(cmdParms);
            }
        }

        /// <summary>
        /// 添加输入(IN)参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">类型长度（可选：默认为0）</param>
        /// <returns>配置好的Oracle参数</returns>
        public static OracleParameter AddInputParameter(string paramName, object value, OracleDbType dbType, int size = 0)
        {
            return AddParameter(paramName, value, dbType, size, ParameterDirection.Input);
        }

        /// <summary>
        /// 添加输出(OUT)参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">类型长度（可选：默认为0）</param>
        /// <returns>配置好的Oracle参数</returns>
        public static OracleParameter AddOutputParameter(string paramName, OracleDbType dbType, int size = 0)
        {
            return AddParameter(paramName, DBNull.Value, dbType, size, ParameterDirection.Output);
        }

        /// <summary>
        /// 添加输入输出(INOUT)参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="value">输入值</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">类型长度（可选：默认为0）</param>
        /// <returns></returns>
        public static OracleParameter AddInOutputParameter(string paramName, object value, OracleDbType dbType, int size = 0)
        {
            return AddParameter(paramName, value, dbType, size, ParameterDirection.InputOutput);
        }

        /// <summary>
        /// 添加返回值
        /// </summary>
        /// <param name="paramName">返回值名</param>
        /// <param name="dbType">数据类型</param>
        /// <param name="size">类型长度（可选：默认为0）</param>
        /// <returns>配置好的Oracle参数</returns>
        public static OracleParameter AddReturnValue(string retValName, OracleDbType dbType, int size = 0)
        {
            return AddParameter(retValName, DBNull.Value, dbType, size, ParameterDirection.ReturnValue);
        }

        /// <summary>
        /// 针对sql语句中的参数（数据类型有长度，比如varchar2(30)等
        ///  . 传入value(输入参数)
        ///  . 或传入变量引用(输出参数)
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="dbtype">类型</param>
        /// <param name="size">长度（可选：默认为0）</param>
        /// <param name="direction">参数方向: IN/ OUT（可选：默认为IN）</param>
        /// <returns>配置好的Oracle参数</returns>
        private static OracleParameter AddParameter(string paramName, object value, OracleDbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            OracleParameter param;
            if (size <= 0)
            {
                param = new OracleParameter(paramName, dbType);
            }
            else
            {
                param = new OracleParameter(paramName, dbType, size);
            }
            param.Value = value;
            param.Direction = direction;
            return param;
        }

    }
}

