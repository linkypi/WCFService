using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Reflection;
using System.Data.OleDb;
using Model;

namespace SQLHelper
{
    /// <summary>
    /// 该类封装了操作Sql Server数据库的基本方法
    /// </summary>
    public  class SqlHelper
    {
        private static string dbConnecString;

        public static string MasterDbConnecString
        {
            get { return ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString; }
        }

        public static string DBConnectionString
        {
            get
            {  
                return dbConnecString;
            }
            set
            {
                dbConnecString = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag">true =Master 主库  false = Slave 附库 </param>
        public static void ConfigConnecStr(bool flag)
        {
            if (flag)
            {
                dbConnecString = MasterDbConnecString;
            }
            else
            {
 
            }
        }

        public static ReturnModel ExecCmdByTranc(SqlTransaction trans, string cmdText, string strTableName)
        {
            try
            {
                //DBConnector 手动写的创建数据库联接的类
                SqlConnection conn = new SqlConnection(DBConnectionString);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(cmdText, conn, trans);
               
               
                cmd.CommandText = cmdText;
                cmd.CommandType = CommandType.Text;
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable(strTableName);
                
                da.Fill(dt);

                //trans.Commit();
                return new ReturnModel() { Succeed = true, SearchResult = dt };
            }
            catch (SqlException sqle)
            {
                trans.Rollback();
                return new ReturnModel() { Succeed = false, Message = sqle.Message };
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
            finally
            {
                //conn.Close();
            }
        }

        public static ReturnModel GetDataTable(string strSQL, string strTableName)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = null;
           // SqlTransaction trans = null;

            try
            {
                //DBConnector 手动写的创建数据库联接的类
                conn = new SqlConnection(DBConnectionString);
                conn.Open();
                //trans = conn.BeginTransaction();
                cmd.CommandText = strSQL;
                cmd.Connection = conn;
                //SqlParameter retParam = new SqlParameter("@ret ", SqlDbType.Int);
                //retParam.Direction = ParameterDirection.Output;
                //cmd.Parameters.Add(retParam);, ReturnValue = cmd.Parameters["@ret"].Value
               
                cmd.CommandType = CommandType.Text;
                da.SelectCommand = cmd;
                dt.TableName = strTableName;
                da.Fill(dt);

                return new ReturnModel() { Succeed = true, SearchResult = dt };
            }
            catch (SqlException sqle)
            {
                return new ReturnModel() { Succeed = false, Message = sqle.Message };
            }
            catch (Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="strTableName"></param>
        /// <param name="inParms">输入参数</param>
        /// <param name="outParms">输出參數</param>
        /// <returns>SearchResult=表  ReturnValue=輸出參數的键值对 Dictionary《string,object》 </returns>
        public static ReturnModel GetDataTableByProc(string procName, string strTableName, List<SqlParameter> inParms, List<SqlParameter> outParms)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = null;
            // SqlTransaction trans = null;

            try
            {
                //DBConnector 手动写的创建数据库联接的类
                conn = new SqlConnection(DBConnectionString);
                conn.Open();
                cmd.CommandText = procName;
                cmd.Connection = conn;
              
        
                if (inParms != null)
                {
                    foreach (var item in inParms)
                    {
                        item.Direction = ParameterDirection.Input;
                    }
                    cmd.Parameters.AddRange(inParms.ToArray());
                }
                if (outParms != null)
                {
                    foreach (var item in outParms)
                    {
                        item.Direction = ParameterDirection.Output;
                    }
                    cmd.Parameters.AddRange(outParms.ToArray());
                }
               // ReturnValue = cmd.Parameters["@ret"].Value;
                cmd.CommandType = CommandType.StoredProcedure;
                da.SelectCommand = cmd;
                dt.TableName = strTableName;
                da.Fill(dt);

                //返回輸出
                Dictionary<string, object> retDics = new Dictionary<string, object>();
                if (outParms != null)
                {
                    foreach (var item in outParms)
                    {
                        if (item.Direction == ParameterDirection.Output)
                        {
                            retDics.Add(item.ParameterName, cmd.Parameters[item.ParameterName].Value);
                        }
                    }
                }

                return new ReturnModel() { Succeed = true, SearchResult = dt,ReturnValue=retDics};
            }
            catch (SqlException sqle)
            {
                return new ReturnModel() { Succeed = false, Message = sqle.Message };
            }
            catch (Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
            finally
            {
                conn.Close();
            }
        }


        /// <summary>
        /// 实体插入
        /// </summary>
        /// <param name="sqlStr">插入实体字符串</param>
        /// <param name="paramsDict">要插入的参数字典，即多个参数的键值对（参数名和参数所对应的值）</param>
        /// <returns>运行结果：-2：参数列表为空</returns>
        public static ReturnModel InsertDataByString(string sqlStr, Dictionary<string, object> paramsDict)
        {
            return ExecuteNonQuery(sqlStr, CommandType.Text, paramsDict);
        }

        /// <summary>
        /// 实体插入，返回受影响的行数
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paramsDict">要插入的参数字典，即多个参数的键值对（参数名和参数所对应的值）</param>
        /// <returns>运行结果：-2：参数列表为空</returns>
        public static ReturnModel InsertDataByProc(string procName, Dictionary<string, object> paramsDict)
        {
            return ExecuteNonQuery(procName, CommandType.StoredProcedure, paramsDict);
        }

        /// <summary>
        /// 执行没有返回查询结果的SQL语句，执行成功返回受影响的行数 
        /// </summary>
        /// <param name="cmdText">查询字符串</param>
        /// <param name="paramsDict">参数字典列表</param>
        /// <returns>返回受影响的行数 </returns>
        public static ReturnModel ExecuteNonQuery(string cmdText, Dictionary<string, object> paramsDict)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, paramsDict);
        }

        /// <summary>
        /// 查询单条数据 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paramsValue">查询条件,存储过程参数名--实际要传入的参数值 字典</param>
        /// <param name="returnValueBinding">返回结果里面要对应的存储过程参数, 实体字段名-存储过程返回字段 字典</param>
        /// <returns>要查询的单条数据</returns>
        public static ReturnModel GetDataModelByProc<T>(string procName, Dictionary<string, object> paramsValue, Dictionary<string, string> returnValueBinding) where T : class, new()
        {
           return GetList<T>(procName, CommandType.StoredProcedure, paramsValue, returnValueBinding);
            
        }

        /// <summary>
        /// 查询单条数据 
        /// </summary>
        /// <typeparam name="T">实体名称</typeparam>
        /// <param name="cmdText">查询字符串</param>
        /// <param name="paramsValue">查询条件,存储过程参数名--实际要传入的参数值 字典</param>
        /// <param name="returnValueBinding">返回结果里面要对应的存储过程参数, 实体字段名-存储过程返回字段 字典</param>
        /// <returns>要查询的单条数据</returns>
        public static ReturnModel GetDataModelByString<T>(string cmdText, Dictionary<string, object> paramsValue, Dictionary<string, string> returnValueBinding) where T : class, new()
        {
            return GetList<T>(cmdText, CommandType.Text, paramsValue, returnValueBinding);
           
        }

        /// <summary>
        /// 查询数据列表 
        /// </summary>
        /// <typeparam name="T">实体名称</typeparam>
        /// <param name="cmdText">查询字符串</param>
        /// <param name="paramsValue">查询条件,存储过程参数名-实际要传入的参数值 字典</param>
        /// <param name="returnValue">返回结果里面实体字段名，实体字段名-存储过程返回字段  字典</param>
        /// <returns>要查询的列表数据</returns>
        public static ReturnModel GetDataListByString<T>(string cmdText, Dictionary<string, object> paramsValue, Dictionary<string, string> returnValueBinding) where T : class, new()
        {
            return GetList<T>(cmdText, CommandType.Text, paramsValue, returnValueBinding);
        
        }

        /// <summary>
        /// 查询数据列表 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paramsValue">查询条件,存储过程参数名-实际要传入的参数值 字典</param>
        /// <param name="returnValue">返回结果里面要对应的存储过程参数, 实体字段名-存储过程返回字段 字典</param>
        /// <returns>要查询的列表数据</returns>
        public static ReturnModel GetDataListByProc<T>(string procName, Dictionary<string, object> paramsValue, Dictionary<string, string> returnValueBinding) where T : class, new()
        {
            return GetList<T>(procName, CommandType.StoredProcedure, paramsValue, returnValueBinding);
        }


        /// <summary>
        /// 数据更新 返回受影响的行数
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paramsDict">要更新的数据, 存储过程参数名<->实际要传入的参数值 字典</param>
        /// <returns>受影响的行数</returns>
        public static ReturnModel UpdateData(string procName, Dictionary<string, object> paramsDict)
        {
            return ExecuteNonQuery(procName, CommandType.StoredProcedure,paramsDict);
        }

        /// <summary>
        /// 查询返回第一行第一列的查询结果
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="paramsDict"></param>
        /// <returns>返回第一行第一列的结果   </returns>
        public static ReturnModel ExecuteScalarByProc(string procName, Dictionary<string, object> paramsDict)
        {
            try
            {
                SqlConnection dbConnection = new SqlConnection(DBConnectionString);
                SqlCommand dbCommand = new SqlCommand(procName, dbConnection);
                dbCommand.CommandType = CommandType.StoredProcedure;

                if (paramsDict != null && paramsDict.Count > 0)
                {
                    foreach (KeyValuePair<string, object> kv in paramsDict)
                    {
                        dbCommand.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }
                dbCommand.CommandTimeout = 30;
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                object returnObj = dbCommand.ExecuteScalar();
                dbConnection.Close();


                return new ReturnModel() { SearchResult = returnObj, Succeed = true };
            }
            catch (System.Exception ex)
            {
                return new ReturnModel() { Message = ex.Message, Succeed = false };
            }
        }

        /// <summary>
        /// 执行一条查询语句，返回第一行第一列的结果
        /// </summary>
        /// <param name="cmdText">查询字符串</param>
        /// <param name="paramsDict">参数字典</param>
        /// <returns>返回第一行第一列的结果  否则返回null</returns>
        public static ReturnModel ExecuteScalarByString(string cmdText, Dictionary<string, object> paramsDict)
        {
            try
            {
                SqlConnection dbConnection = new SqlConnection(DBConnectionString);
                SqlCommand dbCommand = new SqlCommand(cmdText, dbConnection);
                dbCommand.CommandType = CommandType.Text;

                if (paramsDict != null && paramsDict.Count > 0)
                {
                    foreach (KeyValuePair<string, object> kv in paramsDict)
                    {
                        dbCommand.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }
                dbCommand.CommandTimeout = 30;
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                object returnObj = dbCommand.ExecuteScalar();
                dbConnection.Close();

                return new ReturnModel() { SearchResult = returnObj, Succeed = true };
            }
            catch (System.Exception ex)
            {
                return new ReturnModel() { Message=ex.Message, Succeed = false };
            }
        }


        /// <summary>
        /// 在一个事务环境中执行一条没有返回查询结果的sql命令
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(sqlTrans, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">一个事务对象</param>
        /// <param name="commandType">命令类型 (存储过程、sql语句等.)</param>
        /// <param name="commandText">存储过程或者sql语句</param>
        /// <param name="commandParameters">查询参数的数组</param>
        /// <returns>执行s成功返回影响的行数 否则返回-1</returns>
        public static ReturnModel ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, Dictionary<string, object> paramsDict)
        {
            try
            {  
                SqlCommand cmd = new SqlCommand(cmdText, trans.Connection, trans);
                if (trans.Connection.State != ConnectionState.Open)
                {
                    trans.Connection.Open();
                }

                //if (trans != null)
                //{
                //    cmd.Transaction = trans;
                //}

                cmd.CommandType = cmdType;
                if (paramsDict != null && paramsDict.Count > 0)
                {
                    foreach (KeyValuePair<string, object> kv in paramsDict)
                    {
                        cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }
          
                int result = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return new ReturnModel() { SearchResult=result, Succeed = true };
            }
            catch (Exception ex)
            {
                return new ReturnModel() { Message=ex.Message,Succeed=false};
            }
          
        }

        /// <summary>
        /// 执行一条查询语句，返回第一行第一列的结果
        /// </summary>
        /// <remarks>
        /// 比如:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">一个事务对象</param>
        /// <param name="commandType">命令类型 (存储过程、sql语句等.)</param>
        /// <param name="commandText">存储过程的名字或者sql语句</param>
        /// <param name="commandParameters">查询参数的数组</param>
        /// <returns>查询结果的对象，需要进行类型转化，使用 Convert.To{Type}</returns>
        public static ReturnModel ExecuteScalar(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                object result = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return new ReturnModel() { Succeed = true, SearchResult = result };
            }
            catch (Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
           
        }


        //**************************************  待用  ******************************************//
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        // 命令参数的缓存，使用了线程同步的Hashtable
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 将查询参数添加到缓存中，以提高效率
        /// </summary>
        /// <param name="cacheKey">参数数组的键</param>
        /// <param name="cmdParms">要缓存的参数数组</param>
        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// 得到缓存中的参数数组
        /// 该数组为克隆数组，不会影响到缓存中的数据
        /// </summary>
        /// <param name="cacheKey">要查找的参数数组的键</param>
        /// <returns>在缓存中的参数数组的克隆对象</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            try
            {
                SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];
                if (cachedParms == null) return null;
                SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];
                // 复制缓存中的数据
                for (int i = 0; i < cachedParms.Length; i++)
                {
                    clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
                }
                return clonedParms;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }


          #region 私有方法

        /// <summary>
        /// 执行没有查询的操作，并返回受影响的行数
        /// </summary>
        /// <param name="sqlStr">插入语句或者存储过程名称</param>
        /// <param name="cmdType">CommandType</param>
        /// <param name="paramsDict">传入的数据字典</param>
        /// <returns>执行成功返回受影响的行数  否则返回-1</returns>
        private static ReturnModel ExecuteNonQuery(string sqlStr, CommandType cmdType, Dictionary<string, object> paramsDict)
        {
            try
            {
                SqlConnection dbConnection = new SqlConnection(DBConnectionString);
                SqlCommand dbCommand = new SqlCommand(sqlStr, dbConnection);
                dbCommand.CommandType = cmdType;

                //if (paramsDict == null || paramsDict.Count <= 0)
                //    return -2;
                if (paramsDict != null)
                {
                    foreach (KeyValuePair<string, object> kv in paramsDict)
                    {
                        //OleDbType dbType = CSharpType2SqlType(kv.Value);
                        dbCommand.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }
                //dbCommand.Parameters.Add(new SqlParameter("@return", OleDbType.Int));
                //dbCommand.Parameters["@return"].Direction = ParameterDirection.ReturnValue;
                dbCommand.CommandTimeout = 60;
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                int result = dbCommand.ExecuteNonQuery();
                //Convert.ToInt32(dbCommand.Parameters["@return"].Value);
                dbConnection.Close();

                return new ReturnModel() {Succeed=true,SearchResult=result };
            }
            catch (System.Exception ex)
            {
                return new ReturnModel() {Succeed=false,Message=ex.Message };
            }
        }

        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <typeparam name="T">数据实体类</typeparam>
        /// <param name="cmdText">查询字符串</param>
        /// <param name="cmdType">CommandType</param>
        /// <param name="paramsValue">实际要传入的参数值 字典</param>
        /// <param name="returnValueBinding">返回结果里面实体字段名  即Reader中的字段  字典</param>
        /// <returns>存在则返回数据列表   否则返回null</returns>
        private static ReturnModel GetList<T>(string cmdText, CommandType cmdType, Dictionary<string, object> paramsValue, Dictionary<string, string> returnValueBinding) where T : class, new()
        {
            try
            {
                SqlConnection dbConnection = new SqlConnection(DBConnectionString);
                SqlCommand dbCommand = new SqlCommand(cmdText, dbConnection);
                dbCommand.CommandType = cmdType;
                List<T> list = null;
                if (paramsValue != null)
                {
                    foreach (KeyValuePair<string, object> kv in paramsValue)
                    {
                        //OleDbType dbType = CSharpType2SqlType(kv.Value);
                        dbCommand.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }
                dbCommand.CommandTimeout = 5 * 60;
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();
                using (SqlDataReader dataReader = dbCommand.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        try
                        {
                            if (list == null)
                                list = new List<T>();

                            T t = new T();

                            Type type = typeof(T);
                            PropertyInfo[] pi = type.GetProperties();
                            foreach (var item in pi)
                            {
                                Type proType = item.PropertyType;
                                if (!returnValueBinding.ContainsKey(item.Name))
                                {
                                    continue;
                                }
                                string sqlReturnValue = returnValueBinding[item.Name];
                                if (dataReader[sqlReturnValue] != DBNull.Value)
                                        item.SetValue(t, Convert.ChangeType(dataReader[sqlReturnValue], proType), null);
                                //这里的IF也可以改用SWITCH来判断。
                                /*
                                if (proType == typeof(String))
                                {
                                    item.SetValue(t, dataReader[sqlReturnValue], null);
                                }
                                else if (proType == typeof(Int32))
                                {
                                    item.SetValue(t, item == null ? 0 : Convert.ToInt32(dataReader[sqlReturnValue]), null);
                                }
                                else if (proType == typeof(Nullable<int>))// int?
                                {
                                    //所有Nulable<T>的类型以此类推，如double?类型
                                    item.SetValue(t, dataReader[sqlReturnValue] == null ? null : (int?)Convert.ToInt32(dataReader[sqlReturnValue]), null);
                                }
                                else
                                {
                                    //继续用if或者switch/case添加更多的可能。以应对更复杂的自定义类型
                                }
                                */
                            }

                            list.Add(t);
                        }
                        catch (System.Exception ex)
                        {
                            return new ReturnModel() { Succeed = false, Message = ex.Message };
                        }
                    }
                }
                dbConnection.Close();
                return new ReturnModel() { Succeed = true, SearchResult = list };
            }
            catch (System.Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
          
        }

        /// <summary>
        /// 预处理Command对象的参数
        /// </summary>
        /// <param name="cmd">要处理的Command对象</param>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="trans">事务对象</param>
        /// <param name="cmdType">命令类型（存储过程、sql语句等)</param>
        /// <param name="cmdText">存储过程的名字或者sql语句</param>
        /// <param name="cmdParms">要被Command对象使用的参数数组</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandText = cmdText;


                if (trans != null)
                {
                    cmd.Transaction = trans;
                }

                cmd.CommandType = cmdType;

                if (cmdParms != null)
                {
                    cmd.Parameters.AddRange(cmdParms);
                }
            }
            catch (Exception ex)
            {   
                return ;
            }
        
        }

        /// <summary>
        /// c#数据类型转换为sql数据类型
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private static OleDbType CSharpType2SqlType(object param)
        { 
            OleDbType dbType = OleDbType.Variant;//默认为Object
            try
            {
                string paramType = param.GetType().Name;
                switch (paramType)
                {
                    case "Int16":
                        dbType = OleDbType.SmallInt;
                        break;
                    case "Int32":
                        dbType = OleDbType.Integer;
                        break;
                    case "String":
                        dbType = OleDbType.LongVarWChar;
                        break;
                    case "DateTime":
                        dbType = OleDbType.DBDate;
                        break;
                    case "Single":
                        dbType = OleDbType.Single;
                        break;
                    case "Double":
                        dbType = OleDbType.Double;
                        break;
                    case "Decimal":
                        dbType = OleDbType.Decimal;
                        break;
                    case "Byte":
                        dbType = OleDbType.TinyInt;
                        break;
                    case "Guid":
                        dbType = OleDbType.Guid;
                        break;
                }
                return dbType;
            }
            catch (Exception ex)
            {
                return dbType;
            }
        }

        /// <summary>
        /// sql数据类型转换为c#数据类型
        /// </summary>
        /// <param name="sqlType">sql数据类型</param>
        /// <returns>c#数据类型</returns>
        private static Type SqlTyp2CSharpType(OleDbType sqlType)
        {
            try
            {
                switch (sqlType)
                {
                    case OleDbType.BigInt:
                        return typeof(Int64);
                    case OleDbType.Binary:
                        return typeof(Array);
                    case OleDbType.Boolean:
                        return typeof(Boolean);
                    case OleDbType.Char:
                        return typeof(String);
                    case OleDbType.Date:
                        return typeof(DateTime);
                    case OleDbType.Decimal:
                        return typeof(Decimal);
                    case OleDbType.Double:
                        return typeof(Double);
                    case OleDbType.Integer:
                        return typeof(Int32);
                    case OleDbType.Currency:
                        return typeof(Decimal);
                    case OleDbType.LongVarChar:
                        return typeof(String);
                    case OleDbType.LongVarWChar:
                        return typeof(String);
                    case OleDbType.Numeric:
                        return typeof(Decimal);
                    case OleDbType.Single:
                        return typeof(Single);
                    case OleDbType.DBDate:
                        return typeof(DateTime);
                    case OleDbType.SmallInt:
                        return typeof(Int16);
                    case OleDbType.Guid:
                        return typeof(Guid);
                    case OleDbType.IDispatch:
                        return typeof(Object);
                    case OleDbType.IUnknown:
                        return typeof(Object);
                    case OleDbType.TinyInt:
                        return typeof(Byte);
                    case OleDbType.PropVariant:
                        return typeof(Object);
                    case OleDbType.UnsignedBigInt:
                        return typeof(UInt64);
                    case OleDbType.WChar:
                        return typeof(String);
                    case OleDbType.VarChar:
                        return typeof(String);
                    case OleDbType.Variant:
                        return typeof(Object);
                    case OleDbType.VarWChar:
                        return typeof(String);
                    default:
                        return null;
                }
              }
             catch(Exception ex)
            {
                return null;
            }
        }

       #endregion


    }
}

