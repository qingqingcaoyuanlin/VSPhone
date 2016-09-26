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
//using Phone;

namespace VSPhone
{
    public partial class Form1 : Form
    {
        public static byte[] PublicHead;
        //public Phone Phone1 = new Phone();
        public AutoCall autoCall = new AutoCall();
        public Form1()
        {
            InitializeComponent();
            Form1_Load();

        }
        public void Form1_Load()
        { 
            /*
            tabPage1.Text = "声音测试";
            tabPage1.Name = "VoiceTest";
            tabPage1.Controls.Add(Phone1);            
            Phone1.Show();
             * */
            
            tabPage1.Text = "声音测试";
            tabPage1.Name = "VoiceTest";
            autoCall.TopLevel = false;
            tabPage1.Controls.Add(autoCall);
            autoCall.Show();

            ReadXML xml = new ReadXML();
            
            
        }
        
    }
}
