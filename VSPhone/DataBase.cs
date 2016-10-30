using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;
using Maticsoft.DBUtility;
namespace VSPhone
{
    public class DataBase
    {
        public const string DBName = "vsphone";
        public const string DeviceTable = "device";
        public const string RecordTable = "record";
        static public DevStruct devStruct;
        static public RecordStruct recordStruct;
       
        public struct DevStruct
        {
            public string ProjectCode;      //项目编号
            public string Header;          //头
            public string DeviceType;      //设备类型
            public string DeviceNum;       //设备号码
        }
        
        public struct RecordStruct
        {
            public DevStruct dev;
            public string TimeStart;        //开始呼叫的时间
            public int CallPeriodSet;       //设置呼叫的时间（1-30S）
            public int CallPeriod;          //实际呼叫时长
            public bool CallSucceed;        //呼叫是否成功，实际与设置的比较
            public bool Pickup;             //对方中途有人摘机
            public int TalkPeriodSet;       //设置通话的时间（1-120S）
            public int TalkPeriod;          //实际通话时长
            public bool TalkSucceed;        //通话是否成功，实际与设置的比较
            public string TimeEnd;          //结束的时间（呼叫或者通话）
        }
        static public void ClearRecordStruct()
        {
            recordStruct.dev.ProjectCode = null;
            recordStruct.dev.Header = null;
            recordStruct.dev.DeviceType = null;
            recordStruct.dev.DeviceNum = null;
            recordStruct.TimeStart = null;
            recordStruct.CallPeriodSet = 0;
            recordStruct.CallPeriod = 0;
            recordStruct.CallSucceed = false;
            recordStruct.TalkPeriodSet = 0;
            recordStruct.TalkPeriod = 0;
            recordStruct.TalkSucceed = false;
            recordStruct.TimeEnd = null;

        }
        static public string CreateRecordString()
        {
            string str;
            str = "'" + recordStruct.dev.ProjectCode + "','" + recordStruct.dev.Header + "','" + recordStruct.dev.DeviceType + "','" + recordStruct.dev.DeviceNum + "','" + recordStruct.TimeStart + "','"+ recordStruct.CallPeriodSet +"','" + recordStruct.CallPeriod + "','";
            if(recordStruct.CallSucceed)
            {
                str += "1','";
            }
            else
            {
                str += "0','";
            }
            if (recordStruct.Pickup)
            {
                str += "1','";
            }
            else
            {
                str += "0','";
            }
            str += recordStruct.TalkPeriodSet + "','" + recordStruct.TalkPeriod + "','";
            if(recordStruct.TalkSucceed)
            {
                str += "1','";
            }
            else
            {
                str += "0','";
            }
            str += recordStruct.TimeEnd + "'";
            ClearRecordStruct();
            return str;
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
        static public MySqlConnection GetMySqlConnection()
        {
            MySqlConnection msc = new MySqlConnection("server=localhost;user id=root;pwd=1234;port=3306;database=" + DataBase.DBName);
            return msc;
        }
        static public MySqlCommand GetMySqlCommand(String cmd, MySqlConnection connect)
        {
            MySqlCommand sqlCmd = new MySqlCommand(cmd, connect);
            return sqlCmd;
        }
        static public List<DevStruct> query(MySqlCommand cmd)
        {
            
            MySqlDataReader reader = cmd.ExecuteReader();
            List<DevStruct> devList = new List<DevStruct>();
            DevStruct dev;
            try
            {
                while(reader.Read())
                {
                    dev.ProjectCode = reader["ProjectCode"].ToString();
                    dev.Header = reader["Header"].ToString();
                    dev.DeviceType = reader["DeviceType"].ToString();
                    dev.DeviceNum = reader["DeviceNum"].ToString();
                    devList.Add(dev);
                    //Console.WriteLine(reader["ProjectCode"].ToString());
                    //Console.WriteLine(reader["Header"].ToString());
                    //Console.WriteLine(reader["DeviceType"].ToString());
                    //Console.WriteLine(reader["DeviceNum"].ToString());
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                reader.Close();
                
            }
            return devList;
            /*
            MySqlDataAdapter msda = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            msda.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Console.Write(dt.Rows[i]["id"].ToString());
                Console.Write(dt.Rows[i]["ProjectCode"].ToString());
                Console.Write(dt.Rows[i]["Header"].ToString());
                Console.Write(dt.Rows[i]["DeviceType"].ToString());
                Console.Write(dt.Rows[i]["DeviceNum"].ToString());
            }
             */
        }
        static public DataTable queryTable(string TableName)
        {
            MySqlConnection connect = DataBase.GetMySqlConnection();
            string cmd = "select * from " + TableName;
            MySqlDataAdapter msda = new MySqlDataAdapter(cmd, connect);
            DataTable dt = new DataTable();
            msda.Fill(dt);
            return dt;
            
        }
        
        static public bool GetExecuteCMD(MySqlCommand cmd)
        {
            bool flag = true;
            try
            { 
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                flag = false;
                throw e;
            }
            finally{}
            return flag;

        }
        static public List<DevStruct> QueryDBDevice()
        {
            MySqlConnection connect = DataBase.GetMySqlConnection();
            String sqlQuery = "select * from " + DataBase.DeviceTable;
            
            MySqlCommand cmdQuery = DataBase.GetMySqlCommand(sqlQuery, connect);
            List<DevStruct> devList = new List<DevStruct>();
            connect.Open();
            devList = DataBase.query(cmdQuery);            
            connect.Close();
            return devList;
        }
        static public bool ExcuteCMD(string cmdStr)
        {
            bool flag;
            MySqlConnection connect = DataBase.GetMySqlConnection();
            MySqlCommand cmd = DataBase.GetMySqlCommand(cmdStr, connect);
            connect.Open();
            flag = GetExecuteCMD(cmd);
            connect.Close();
            return flag;
        }
       
    }
}
