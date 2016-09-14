using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Management;
using System.Threading;

namespace VSPhone
{
    public partial class Form1 : Form
    {


        public static byte[] PublicHead;

        public Form1()
        {
            InitializeComponent();

            AppTimer.app_timer_init();
            Remoter.Remoter_init();
            LocalCfg.Addr = new byte[]{(byte)VsProtocol.DevType.DEV_INDOORPHONE,1,1,8,8,1};
            VsProtocol.Change_IDToIP(LocalCfg.Addr,LocalCfg.IP);
            if("10.2.200.7" != GetCurrentIP())
            {
                if (SetIP("10.2.200.7"))
                {
                    Console.WriteLine("设置成功");
                }
            }
            /*
            talkback = new Talkback();
            talkback.talk_back_init();
            talkback.udpDeal.app_udp_init(8300);
            talkback.udpDeal.set_multi_udp_recv_fun(UdpApp.udp_deal);
            talkback.videoDeal.video_manage.video_recv_callback = videoCallback;
            UdpApp.UdpAppInit(talkback);
            label1.Text = new IPAddress(LocalCfg.IP).ToString();
            Output.outObject = richTextBox1;
            InitSetting();
            */

        }
        Talkback talkback;

        private string GetCurrentIP()
        {
            IPAddress CurIP = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            Console.Write(CurIP.ToString());
            return CurIP.ToString();
        }
        private bool SetIP(string ip)
        {
            ManagementBaseObject myInMBO = null;
            ManagementBaseObject myOutMBO = null;
            ManagementClass myMClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection myMOCollection = myMClass.GetInstances();
            string[]ipList = {ip};
            string[]ipMask = {"255.0.0.0"};
            try
            {
                foreach (ManagementObject mo in myMOCollection)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        myInMBO = mo.GetMethodParameters("EnableStatic");
                        myInMBO["IPAddress"] = new string[] {ip};
                        myInMBO["SubnetMask"] = new string[] { "255.0.0.0"};
                        myOutMBO = mo.InvokeMethod("EnableStatic", myInMBO, null);
                        Thread.Sleep(2000);
                        Console.WriteLine("SetIP");
                        break;
                    }

                }

                if (GetCurrentIP() != ip)
                {
                    MessageBox.Show("请手动设置IP：" + ip);
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "请手动设置IP：" + ip);
                return false;
            }
            return false;
        }
        private void InitSetting()
        {
            string fname = Directory.GetCurrentDirectory() + "\\voice.txt";
            string wavname = Directory.GetCurrentDirectory() + "\\P.501 中国语音.wav";
            FileInfo finfo = new FileInfo(fname);
            FileInfo wavinfo = new FileInfo(wavname);
            if (finfo.Exists)
            {
                textBox3.AppendText(fname);
                try
                {
                    string Path = (string)textBox3.Text;
                    FileStream file = new FileStream(Path, FileMode.Open, FileAccess.Read);
                    StreamReader reader = new StreamReader(file);
                    itemObject[] item = new itemObject[5];
                    string line = reader.ReadLine();
                    int i = 0;
                    string[] Line = new string[40];
                    List<itemObject> items = new List<itemObject>();
                    string path;
                    itemObject itemTemp = new itemObject();
                    while (line != null)
                    {
                        Line = line.Split('/');
                        //item[i].Text = Line[0];
                        //item[i].Value = Line[1];
                        itemTemp = new itemObject(Line[1], Line[0]);
                        items.Add(itemTemp);
                        Console.WriteLine(itemTemp.Text);
                        Console.WriteLine(itemTemp.Value);
                        i++;
                        line = reader.ReadLine();
                        path = Directory.GetCurrentDirectory() + itemTemp.Text.ToString();
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(itemTemp.Text.ToString());
                        }
                    }
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(items.ToArray());

                    if (comboBox1.Items.Count > 0)
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                    file.Close();
                }
                catch (Exception)
                {
                    Output.MessaggeOutput("Exception");
                }

            }
            if (wavinfo.Exists)
            {
                textBox2.AppendText(wavname);
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string[] num = textBox1.Text.Split(new char[]{'-'});
                if (textBox_Header.TextLength <= 0)
                {
                    MessageBox.Show("请输入Header");
                    return;
                }
                /*
                string text = textBox_Header.Text.ToString();
                PublicHead = Encoding.UTF8.GetBytes(text);
                */
                //PublicHead = new byte[] { 0x47, 0x56, 0x53, 0x38, 0x30, 0x31, 0xce, 0xcf, 0xc8, 0x0};
                /*
                string[] numbers = textBox3.Text.Split(' ');
                for(int i=0; i<10; i++)
                {
                    String hex = numbers[i];
                    PublicHead[i] = Convert.ToByte(hex, 16);
                    Console.WriteLine(PublicHead[i]);
                }
                */
                byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_INDOORPHONE, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };
                talkback.call_out(callId, 0, null);
            }
            catch
            {
                MessageBox.Show("格式错误");
            }
            //talkback.audioDeal.playPath = textBox2.Text;
            Output.MessaggeOutput("呼叫室内机 :" + textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] num = null;
            try
            {
                num = textBox1.Text.Split(new char[] { '-' });
                if (num.Length == 3)
                {
                    byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_DOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), 0, Convert.ToByte(num[2]), 1 };
                    talkback.build_monitor(callId);
                }
                else if (num.Length == 4)
                {
                    byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_SECONDOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };
                    talkback.build_monitor(callId);
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
            //talkback.audioDeal.playPath = textBox2.Text;
            if (num.Length == 3)
            {
                Output.MessaggeOutput("监视门口机 :" + textBox1.Text);
            }
            else if (num.Length == 4)
            {
                Output.MessaggeOutput("监视小门口机 :" + textBox1.Text);
            }
            

        }

        private void button3_Click(object sender, EventArgs e)
        {
            talkback.pick();
            //talkback.audioDeal.playPath = textBox2.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
            /*
            if (textBox2.Text != null)
            {
                FileStream file = new FileStream(textBox2.Text, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                int i=0;
                int j=0;
                string line = reader.ReadLine();
                string []Line = new string[40];
                string []Head = new string[30];
                string []itemname = new string[10];
                while(line != null)
                {
                    
                    Line = line.Split('/');
                    Head[i] = Line[0];
                    itemname[i] = Line[1];                   
                    i++;
                    line = reader.ReadLine();
                }

            
            }
            */
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
         //   Device dv = new Device();
         //   dv.SetCooperativeLevel(sender as Control, CooperativeLevel.Normal);
         //   SecondaryBuffer buf = null;
            //Microsoft.DirectX.DirectSound.
            try
            {
        //        buf = new SecondaryBuffer("E:\\xx.wav", dv);
            }
            catch { }
        //    buf.Play(0,BufferPlayFlags.Looping);
        }
        public class itemObject
        { 
            public string Text;
            public string Value;

            public itemObject()
            { 
            
            }
            public itemObject(string _text, string _value)
            {
                Text = _text;
                Value = _value;
            }
            public override string ToString()
            {
                return Text;
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
           // if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFileDialog1.FileName;
            }          
            if (textBox3.Text != null)
            {
                try
                {
                    string Path = (string)textBox3.Text;
                    FileStream file = new FileStream(Path, FileMode.Open, FileAccess.Read);
                    StreamReader reader = new StreamReader(file);
                    itemObject[] item = new itemObject[5];
                    string line = reader.ReadLine();
                    int i = 0;
                    string[] Line = new string[40];
                    List<itemObject> items = new List<itemObject>();
                    /*
                
                    int j = 0;
                
                
                    string[] Head = new string[30];
                    string[] itemname = new string[10];
                    while (line != null)
                    {

                        Line = line.Split('/');
                        Head[i] = Line[0];
                        itemname[i] = Line[1];
                        Console.WriteLine(Head[i]);
                        Console.WriteLine(itemname[i]);
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add();
                        i++;
                        line = reader.ReadLine();
                    }
                    */
                    itemObject itemTemp = new itemObject();
                    while (line != null)
                    {
                        Line = line.Split('/');
                        //item[i].Text = Line[0];
                        //item[i].Value = Line[1];
                        itemTemp = new itemObject(Line[1], Line[0]);
                        items.Add(itemTemp);
                        Console.WriteLine(itemTemp.Text);
                        Console.WriteLine(itemTemp.Value);
                        i++;
                        line = reader.ReadLine();
                    }
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(items.ToArray());

                    if (comboBox1.Items.Count > 0)
                    {
                        comboBox1.SelectedIndex = 0;
                        talkback.audioDeal.projName = comboBox1.SelectedItem.ToString();
                    }
                    file.Close();
                }
                catch (Exception) 
                {
                    Output.MessaggeOutput("Exception");
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            itemObject object1 = new itemObject();
            object1 = (itemObject)comboBox1.SelectedItem;
            String head = object1.Value;           
            String[] number = head.Split(' ');
            byte []temp = new byte[10];
            if(number.Length == 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    temp[i] =(byte) Convert.ToByte(number[i],16);
                    Console.WriteLine(temp[i]);
                    
                }
                PublicHead = temp;
                textBox_Header.Text = head;
                talkback.audioDeal.projName = object1.ToString();
                
            }
            else
            {
                Console.WriteLine(number.Length);
                MessageBox.Show("头长度错误，应为10");
            }
        }

        void  videoCallback(Video.VIDEO_RECV_BUFF video_recv_buff)
        {
            pictureBox1.Invoke(new MethodInvoker(delegate{
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                MemoryStream ms = new MemoryStream();
                ms.Write(video_recv_buff.buff, 0, video_recv_buff.len);
                pictureBox1.Image = new Bitmap(ms);
                ms.Close();
                video_recv_buff.idel_flag = 1;
            }));
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            talkback.call_out(LocalCfg.Addr_GuardUnit, 0, null);
            //talkback.audioDeal.playPath = textBox2.Text;
            Output.MessaggeOutput("呼叫管理机 " );
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (ConnectStat.NewCallFlag > 0)
            {
                talkback.handup(0, 1);
                //Change_To_NewCall_Ring();
            }
            else
            {
                talkback.handup(0, 0);
            }
            Output.MessaggeOutput("挂机 ");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string wavname = textBox2.Text;
            FileInfo wavfile = new FileInfo(wavname);
            if (checkBox1.Checked)
            {
                if (wavfile.Exists)
                {
                    this.talkback.audioDeal.playPath = wavname;
                }
                else
                {
                    MessageBox.Show("语言文件不存在");
                }
            }
            else
            {
                this.talkback.audioDeal.playPath = null;
            }
        }
    }
}
