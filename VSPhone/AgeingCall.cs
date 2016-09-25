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
        }
    }
}
