using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace VSPhone
{
    public partial class DeviceRecord : Form
    {
        public DeviceRecord()
        {
            InitializeComponent();
            InitSetting();
        }
        private DataTable dataTable = new DataTable();
//        private MySqlDataAdapter adapter;
//        private MySqlCommand cmd = new MySqlCommand();
//        private MySqlConnection connect = new MySqlConnection();
//        private String strCmd;
        private void InitSetting()
        {
            //dt = DataBase.queryTable(DataBase.DeviceTable);
            MySqlConnection connect = DataBase.GetMySqlConnection();
            string strCmd = "select * from " + DataBase.DeviceTable;
            MySqlDataAdapter adapter = new MySqlDataAdapter(strCmd, connect);
            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            
        }

        private void button1_Click(object sender, EventArgs e)  //增加
        {
            /*
            string cmd = "";
            adapter.InsertCommand = DataBase.GetMySqlCommand(cmd, connect);
            //adapter.InsertCommand = new MySqlCommand();
            adapter.InsertCommand.Parameters.Add();
             */
            /*
            string[] str = new string[dataGridView1.Rows.Count];
            int index = 0;
            
            foreach(DataGridViewRow row in dataGridView1.Rows)
            {
                if(true)
                {
                    continue;
                }
                else
                {
                    str[index] = "insert into " + DataBase.DeviceTable + " values(null,'" + row.Cells[1].Value + "," + row.Cells[2].Value + "," + row.Cells[3].Value + "," + row.Cells[4].Value + "')";
                    index++;

                }
                


            }
             */ 
       }

        private void button2_Click(object sender, EventArgs e)  //删除
        {
            MySqlConnection connect = DataBase.GetMySqlConnection();
            string strCmd = "select * from " + DataBase.DeviceTable;
            MySqlDataAdapter adapter = new MySqlDataAdapter(strCmd, connect);

            int row = dataGridView1.CurrentRow.Index;
            DataRow dr = dataTable.Rows[row];
            dr.Delete();
            MySqlCommandBuilder mscb = new MySqlCommandBuilder(adapter);
            adapter.Update(dataTable);

        }

        private void button3_Click(object sender, EventArgs e)  //修改/或者添加
        {
            MySqlConnection connect = DataBase.GetMySqlConnection();
            string strCmd = "select * from " + DataBase.DeviceTable;
            MySqlDataAdapter adapter = new MySqlDataAdapter(strCmd, connect);

            MySqlCommandBuilder mscb = new MySqlCommandBuilder(adapter);
            adapter.Update(dataTable);
        }

        private void button4_Click(object sender, EventArgs e)  //清空
        {
            string cmd = "truncate table "+ DataBase.DeviceTable;
            if(DataBase.ExcuteCMD(cmd))
            {
                MessageBox.Show("清空成功");
                dataTable.Clear();
            }
            else
            {
                MessageBox.Show("清空失败");
            }
        }
    }
}
