using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VSPhone
{
    public partial class AgeingCall : Form
    {
        public AgeingCall()
        {
            InitializeComponent();
        }
        public void InitSetting()
        { 
            
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int CallTime = trackBar1.Value;
            textBox1.Text = CallTime.ToString();



        }

        private void button1_Click(object sender, EventArgs e)  //开始键
        {
            AppTimer.start_timer(AppTimer.register_timer(null, ));
        }

        private void button2_Click(object sender, EventArgs e)  //结束键
        {

        }

        public void Ageing_Call_Device()
        { 
        
        }
    }
}
