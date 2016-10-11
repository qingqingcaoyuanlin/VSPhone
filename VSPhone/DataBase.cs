using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
namespace VSPhone
{
    class DataBase
    {
        public const string DBName = "vsphone";
        public const string DeviceTable = "devicelist";



        public struct DevTable
        {
            string ProjectNum;      //项目编号
            string Header;          //头
            string DeviceType;      //设备类型
            string DeviceNum;       //设备号码
        };
        public struct RecordTable
        {
            DevTable devTbl;
            string CalledTime;      //开始呼叫的时间
            int SettingTime;        //设置呼叫的时间（1-120S）
            int duration;           //呼叫持续的时间（1-120S）
        };

        public void CreatDB(string dbName)
        { 
            string connstr = "server=localhost;user id=root;pwd=1234;port=3306;database="+dbName;       
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;                    
            try 
            {
                string comstr = @"create table if not exists " + dbName;
                MySqlCommand sqlcom = new MySqlCommand(comstr, sqlconn);
                sqlconn.Open();
                sqlcom.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("DB not Exit");
                MessageBox.Show("创建数据库失败");
            }
            finally
            {
                
            }
            sqlconn.Close();        
        }

        public void CreatDBTable(string tableName)
        { 
            
        }
        public void CreatDBDeviceTable(string tableName)
        {
            string connstr = "server=localhost;user id=root;pwd=1234;port=3306;database=" + DBName+"." + tableName;
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;
            try
            {
                string comstr = @"create table if not exists " + DBName + "." + tableName + @" (
	                                        项目编号		TEXT NOT NULL,
                                        	头		        TEXT NOT NULL,
                                            设备类型	    TEXT NOT NULL,
	                                        设备编号		TEXT NOT NULL 
                                        )ENGINE=MyISAM  DEFAULT CHARSET=gbk AUTO_INCREMENT=4"; 
                MySqlCommand sqlcom = new MySqlCommand(comstr, sqlconn);
                sqlconn.Open();
                sqlcom.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("DB not Exit");
                MessageBox.Show("创建表失败");
            }
            finally
            {

            }
            sqlconn.Close();     
        }
        public void CreatDBRecordTable(string tableName)
        {
            string connstr = "server=localhost;user id=root;pwd=1234;port=3306;database=" + DBName + "." + tableName;
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;
            try
            {
                string comstr = @"create table if not exists " + DBName + "." + tableName + @" (
	                                        项目编号		TEXT NOT NULL,
                                        	头		        TEXT NOT NULL,
                                            设备类型	    TEXT NOT NULL,
	                                        设备编号		TEXT NOT NULL,
                                            时间            Date,
                                            设置时长		TEXT NOT NULL,
                                            呼叫时长		TEXT NOT NULL,
                                            是否成功		bool
                                        )ENGINE=MyISAM  DEFAULT CHARSET=gbk AUTO_INCREMENT=4";
                MySqlCommand sqlcom = new MySqlCommand(comstr, sqlconn);
                sqlconn.Open();
                sqlcom.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("DB not Exit");
                MessageBox.Show("创建表失败");
            }
            finally
            {

            }
            sqlconn.Close();
        }

        public void CreadDBNTable(string dbName,string tableName)
        {
            string connstr = "server=localhost;user id=root;pwd=1234;port=3306;database=" + dbName + "." + tableName;
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;
            try
            {
                string comstr = @"create table if not exists " + dbName;
                MySqlCommand sqlcom = new MySqlCommand(comstr, sqlconn);
                sqlconn.Open();
                sqlcom.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("DB not Exit");
                MessageBox.Show("创建数据库和表失败");
            }
            finally
            {

            }
            sqlconn.Close();     
        }
        public void DeviceTableInsert()
        { 
        
        }
        public void DeviceTableDel()
        { 
        
        }

        public void RecordTableInsert()
        { 
        
        
        }

        private static string connectionString = ConfigurationManager.ConnectionStrings["mysqlconn"].ConnectionString;
        /// <summary>  
        /// 执行查询语句，返回DataSet  
        /// </summary>  
        /// <param name="SQLString">查询语句</param>  
        /// <returns>DataSet</returns>  
        public static DataSet Query(string SQLString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                    command.Fill(ds);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }
        /// <summary>  
        /// 执行SQL语句，返回影响的记录数  
        /// </summary>  
        /// <param name="SQLString">SQL语句</param>  
        /// <returns>影响的记录数</returns>  
        public static int ExecuteSql(string SQLString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        /// <summary>  
        /// 执行SQL语句，返回影响的记录数  
        /// </summary>  
        /// <param name="SQLString">SQL语句</param>  
        /// <returns>影响的记录数</returns>  
        public static int ExecuteSql(string[] arrSql)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                try
                {
                    connection.Open();
                    MySqlCommand cmdEncoding = new MySqlCommand(SET_ENCODING, connection);
                    cmdEncoding.ExecuteNonQuery();
                    int rows = 0;
                    foreach (string strN in arrSql)
                    {
                        using (MySqlCommand cmd = new MySqlCommand(strN, connection))
                        {
                            rows += cmd.ExecuteNonQuery();
                        }
                    }
                    return rows;
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    connection.Close();
                    throw e;
                }
                finally
                {
                    connection.Close();
                }
            }
        }  
  

    }
}
