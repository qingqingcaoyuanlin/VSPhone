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
        private DataTable dt = new DataTable();
        private MySqlDataAdapter adapter;
        private MySqlCommand cmd = new MySqlCommand();
        private MySqlConnection connect = new MySqlConnection();
        private String strCmd;
        private void InitSetting()
        {
            //dt = DataBase.queryTable(DataBase.DeviceTable);
            connect = DataBase.GetMySqlConnection();
            strCmd = "select * from " + DataBase.DeviceTable;
            adapter = new MySqlDataAdapter(strCmd, connect);
            adapter.Fill(dt);
            dataGridView1.DataSource = dt;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            
        }

        private void button1_Click(object sender, EventArgs e)  //增加
        {

        }

        private void button2_Click(object sender, EventArgs e)  //删除
        {
            int row = dataGridView1.CurrentRow.Index;
            DataRow dr = dt.Rows[row];
            dr.Delete();
            MySqlCommandBuilder mscb = new MySqlCommandBuilder(adapter);
            adapter.Update(dt);

        }

        private void button3_Click(object sender, EventArgs e)  //刷新
        {
            MySqlCommandBuilder mscb = new MySqlCommandBuilder(adapter);
            adapter.Update(dt);
        }

        private void button4_Click(object sender, EventArgs e)  //清空
        {

        }
    }
}
