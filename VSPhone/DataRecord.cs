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
    public partial class DataRecord : Form
    {
        public DataRecord()
        {
            InitializeComponent();
            InitSetting();
        }
        private void InitSetting()
        {
            DeviceRecord deviceRecord = new DeviceRecord();
            deviceRecord.TopLevel = false;
            tabPage1.Text = "设备记录";
            tabPage1.Name = "DeviceRecord";
            tabPage1.Controls.Add(deviceRecord);
            deviceRecord.Show();

            CallRecord callRecord = new CallRecord();
            callRecord.TopLevel = false;
            tabPage2.Text = "呼叫记录";
            tabPage2.Text = "CallRecord";
            tabPage2.Controls.Add(callRecord);
            callRecord.Show();
        }
    }
}
