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
        static int CallTime = 120;
        private void InitSetting()
        {
            callList = DataBase.QueryDBDevice();            
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            CallTime = trackBar1.Value;
            textBox1.Text = CallTime.ToString();
            Console.WriteLine(CallTime.ToString());

        }

        public void Start_Auto_Call()
        {
            AppTimer.start_timer(AppTimer.register_timer(null, T_Check_AutoCall, 0, null, 2000, 0));
            index = 0;
        }
        public void AutoHandup()        //自动挂断
        {


        }
        private void button1_Click(object sender, EventArgs e)  //开始键
        {
            if(AppTimer.search_timer_by_func(T_Check_AutoCall)==null)
            {
                if(DataBase.QueryDBDevice().Count > 0)      //数据库记录>0
                {
                    Console.WriteLine("Data Not Empty");
                    Start_Auto_Call();
                }
                else
                {
                    MessageBox.Show("数据库为空，请添加设备");
                }
                
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
        
        static void T_Pick(int argc, object argv)
        {
            CallTabPage.talkback.pick();
            AppTimer.start_timer(AppTimer.register_timer(null, T_Handup, 0, null, CallTime * 800, 1));      //定时挂机
        }
        /// <summary>
        /// 定时挂机
        /// </summary>
        static void T_Handup(int argc, object argv)
        {
            if (ConnectStat.NewCallFlag > 0)
            {
                CallTabPage.talkback.handup(0, 1);
                //Change_To_NewCall_Ring();
            }
            else
            {
                CallTabPage.talkback.handup(0, 0);
            }
        }
        static public void AutoCall()
        {
            
            DataBase.DevStruct dev;
            if(index > callList.Count()-1)
            {
                index = 0;
            }
            dev = callList[index++];
            CallTabPage.talkback.audioDeal.projName = dev.Header;
            switch(dev.DeviceType)
            {
                case "IS":
                    CallFunc.Call_IndoorStation(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Handup,0, null, CallTime * 800,1));
                    break;
                case "GU":
                    CallFunc.Call_GU(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Handup, 0, null, CallTime * 800, 1));
                    break;
                case "MiniOS":
                    CallFunc.Monitor_MiniOS(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Pick, 0, null, 4000,1));
                    break;
                case "OS":
                    CallFunc.Monitor_OS(dev.DeviceNum);
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
    public static class CallFunc
    {
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
                num = devNum.Split(new char[] { '-' });

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
                num = devNum.Split(new char[] { '-' });
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
        static void Pick()
        {
            CallTabPage.talkback.pick();
        }
    }
}
