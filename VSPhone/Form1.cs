using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Media;
using System.IO;
//using Microsoft.DirectX;
//using Microsoft.DirectX.DirectSound;
//using Phone;
using MySql.Data.MySqlClient;


namespace VSPhone
{
    public partial class Form1 : Form
    {
        public static byte[] PublicHead;
        //public Phone Phone1 = new Phone();
        public AutoCall autoCall = new AutoCall();
        public Form1()
        {
            InitializeComponent();
            Form1_Load();

        }
        public void Form1_Load()
        { 
            
            
            tabPage1.Text = "声音测试";
            tabPage1.Name = "VoiceTest";
            autoCall.TopLevel = false;
            tabPage1.Controls.Add(autoCall);
            autoCall.Show();

            //ReadXML xml = new ReadXML();
            Check_MySql();
            //Check_SqlServer();
            
        }

        public const string dbName = "VSPhone";
        public bool Check_SqlServer()
        {
            Console.WriteLine("sqlserver");
            SqlConnection sqlconn = new SqlConnection();
            try
            {
                
                sqlconn.ConnectionString = "server=local; database=" + dbName;

                string strCom = "create database if not exists " + dbName;
                SqlCommand sqlCom = new SqlCommand(strCom, sqlconn);
                
                sqlconn.Open();
                sqlCom.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error"+e.ToString());
                MessageBox.Show(e.ToString());
            }
            finally
            { }
            sqlconn.Close();
            
            
            return true;
        
        }
        public bool Check_MySql()
        {
            
            
            string connstr = "server=127.0.0.1;user id=sa;pwd=1234;port=3306;database=vsphone";       
            MySqlConnection sqlconn = new MySqlConnection();
            sqlconn.ConnectionString = connstr;
            
            try 
            {
                string comstr = "create database if not exists vsphone";
                MySqlCommand sqlcom = new MySqlCommand(comstr, sqlconn);
                sqlconn.Open();
                sqlcom.ExecuteNonQuery();
                sqlconn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("DB not Exit");
                return false;
            }
            finally
            {
                sqlconn.Close();
            }
            Console.WriteLine("DB Exit");
            return true;

        }
        
    }
}
