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
        public const string DeviceList = "devicelist";

        public void CreatDB(string dbName)
        { 
            string connstr = "server=localhost;user id=root;pwd=1234;port=3306;database="+dbName;       
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;                    
            try 
            {
                //string comstr = "create database if not exists vsphone";
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
        public void CreatDBDeviceList(string tableName)
        { 
        
        }
    }
}
