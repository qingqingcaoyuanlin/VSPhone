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
        static public int index = 0;
        static public int CallTime = 30;       //呼叫时长
        static public int TalkTime = 120;      //通话时长
        static public CallFunc callFunc;
        private void InitSetting()
        {
            callList = DataBase.QueryDBDevice();
            callFunc = new CallFunc();
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)   //通话时间
        {
            TalkTime = trackBar2.Value;
            textBox2.Text = TalkTime.ToString();
            Console.WriteLine(TalkTime.ToString());
        }
        private void trackBar2_Scroll(object sender, EventArgs e)   //呼叫时间
        {
            CallTime = trackBar1.Value;
            textBox1.Text = CallTime.ToString();
            Console.WriteLine(CallTime.ToString());

        }
        
               
        private void button1_Click(object sender, EventArgs e)  //开始键
        {
            
            if(DataBase.QueryDBDevice().Count > 0)      //数据库记录>0
            {
                Console.WriteLine("Data Not Empty");
                callFunc.Start_Call();
            }
            else
            {
                MessageBox.Show("数据库为空，请添加设备");
            }            
        }
        
        private void button2_Click(object sender, EventArgs e)  //结束键
        {
            callFunc.Stop_Call();
        }
        public void UpdateCountDown(string str)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.label1.Text = str;
            }));
            
        }

        public void UpdateCalling(string str)
        {
            this.Invoke(new EventHandler(delegate
            {
                this.label2.Text = str;
            }));
        }     
        
    }
    public class CallFunc
    {
        public void Call_IndoorStation(string devNum)
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
        public void Call_GU(string devNum)
        {


        }
        public void Monitor_OS(string devNum)
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

        public void Monitor_MiniOS(string devNum)
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
        public void Pick()
        {
            CallTabPage.talkback.pick();
        }

        public void T_Check_AutoCall(int argc, object argv)
        {
            if (CallTabPage.talkback.get_talkback_state() == Talkback.STA.STA_NORMAL_STANDBY)
            {
                AutoCall();
            }
        }

        public void AutoCall()
        {

            DataBase.DevStruct dev;
            if (AgeingCall.index > AgeingCall.callList.Count() - 1)
            {
                AgeingCall.index = 0;
            }
            dev = AgeingCall.callList[AgeingCall.index++];
            CallTabPage.talkback.audioDeal.projName = dev.Header;
            string Calling;
            switch (dev.DeviceType)
            {
                case "IS":
                    Calling = "呼叫" + dev.DeviceType + ":" + dev.DeviceNum;
                    AgeingCall.
                    Call_IndoorStation(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Handup, 0, null, AgeingCall.CallTime * 800, 1));
                    break;
                case "GU":
                    Calling = "呼叫" + dev.DeviceType + ":" + dev.DeviceNum;
                    Call_GU(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Handup, 0, null, AgeingCall.CallTime * 800, 1));
                    break;
                case "MiniOS":
                    Calling = "监视" + dev.DeviceType + ":" + dev.DeviceNum;
                    Monitor_MiniOS(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Pick, 0, null, 4000, 1));
                    break;
                case "OS":
                    Calling = "监视" + dev.DeviceType + ":" + dev.DeviceNum;
                    Monitor_OS(dev.DeviceNum);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_Pick, 0, null, 4000, 1));
                    break;
                default:

                    break;

            }

        }
        public void Stop_Auto_Call()
        {
            AppTimer.stop_timer(AppTimer.search_timer_by_func(T_Check_AutoCall));            
        }
        public void Stop_Call()
        {
            if (AppTimer.search_timer_by_func(T_Check_AutoCall) != null)
            {
                Stop_Auto_Call();
                AgeingCall.index = 0;
            }
        }
        public void Start_Auto_Call()
        {
            AppTimer.start_timer(AppTimer.register_timer(null, T_Check_AutoCall, 0, null, 2500, 0));
            AgeingCall.index = 0;
        }
        public void Start_Call()
        { 
            if(AppTimer.search_timer_by_func(T_Check_AutoCall)==null)
            {
                Start_Auto_Call();
            }
        }
        static void T_Pick(int argc, object argv)
        {
            CallTabPage.talkback.pick();            //先摘机
            AppTimer.start_timer(AppTimer.register_timer(null, T_Handup, 0, null, AgeingCall.TalkTime * 800, 1));      //再定时挂机
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
    }
}
