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
            InitSetting();
        }
        static public List<DataBase.DevStruct> callList = new List<DataBase.DevStruct>();
        static int index = 0;
        private void InitSetting()
        {
            callList = DataBase.QueryDBDevice();            
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int CallTime = trackBar1.Value;
            textBox1.Text = CallTime.ToString();

        }
        public void Start_Auto_Call()
        {
            AppTimer.start_timer(AppTimer.register_timer(null, T_Check_AutoCall, 0, null, 1000, 0));
            index = 0;
        }
        private void button1_Click(object sender, EventArgs e)  //开始键
        {
            if(AppTimer.search_timer_by_func(T_Check_AutoCall)==null)
            {
                Start_Auto_Call();
            }
            
        }
        static public void Stop_Auto_Call()
        {
            AppTimer.stop_timer(AppTimer.search_timer_by_func(T_Check_AutoCall));
            index = 0;
        }
        private void button2_Click(object sender, EventArgs e)  //结束键
        {
            if (AppTimer.search_timer_by_func(T_Check_AutoCall) != null)
            {
                Stop_Auto_Call();                
            }
        }
        static public void Call_IndoorStation(string devNum)
        {
            try
            {
                string[] num = devNum.Split(new char[] { '-' });
                byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_INDOORPHONE, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };
                CallTabPage.talkback.call_out(callId, 0, null);
                
                
            }
            catch
            {
                MessageBox.Show("格式错误");
            }

        }
        static public void Call_GU(string devNum)
        { 
            
        
        }
        static public void Monitor_OS(string devNum)
        {
            string[] num = null;
            try
            {
                num = textBox1.Text.Split(new char[] { '-' });
                
                if (num.Length == 3)
                {
                    byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_DOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), 0, Convert.ToByte(num[2]), 1 };
                    CallTabPage.talkback.build_monitor(callId);
                }               
                else
                {
                    return;
                }


            }
            catch
            {
                MessageBox.Show("格式错误");
                return;
            }
        }

        static public void Monitor_MiniOS(string devNum)
        {
            string[] num = null;
            try
            {
                num = textBox1.Text.Split(new char[] { '-' });
                if (num.Length == 4)
                {
                    byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_SECONDOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };
                    CallTabPage.talkback.build_monitor(callId);
                }
                else
                {
                    return;
                }
            }
            catch
            {
                MessageBox.Show("格式错误");
                return;
            }
        }
        static void T_Pick(int argc, object argv)
        {
            CallTabPage.talkback.pick();
        }
        static public void AutoCall()
        {
            
            DataBase.DevStruct dev;
            if(index > callList.Count()-1)
            {
                index = 0;
            }
            dev = callList[index++];
            switch(dev.DeviceType)
            {
                case "IS":
                    Call_IndoorStation(dev.DeviceNum);
                    break;
                case "GU":
                    Call_GU(dev.DeviceNum);
                    break;
                case "MiniOS":
                    Monitor_MiniOS(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Pick, 0, null, 4000,1));
                    break;
                case "OS":
                    Monitor_OS(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Pick, 0, null, 4000, 1));
                    break;
                default:

                    break;

            }

        }
        static public void T_Check_AutoCall(int argc, object argv)
        {
            if (CallTabPage.talkback.get_talkback_state() == Talkback.STA.STA_NORMAL_STANDBY)
            {
                AutoCall();
            }
        }
        
    }
}
