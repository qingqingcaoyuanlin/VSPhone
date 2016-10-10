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


        public enum
        {
            IS,
            MiniOS,
            OS,
            GU
        }
        public struct DevTable 
        { 
            string 
        }
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
    }
}
