using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VSPhone
{
    public partial class AutoCall : UserControl
    {
        public AutoCall()
        {
            InitializeComponent();
            AutoCall_Load();

        }
        public void AutoCall_Load()
        {
            tabPage1.Text = "老化";
            tabPage1.Name = "Ageing";
            CallTabPage callTabPage = new CallTabPage();
            tabPage1.Controls.Add(callTabPage);
            //tabControl1.TabPages.Add(tabPage1);
            tabPage1.Show();

        }

    }
}
