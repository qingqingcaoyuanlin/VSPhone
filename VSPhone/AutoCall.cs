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
    public partial class AutoCall : Form
    {
        public AutoCall()
        {
            InitializeComponent();
            AutoCall_Load();

        }
        public void AutoCall_Load()
        {
            tabPage1.Text = "声音测试";
            tabPage1.Name = "VoiceTest";
            CallTabPage callTabPage = new CallTabPage();
            callTabPage.TopLevel = false;
            tabPage1.Controls.Add(callTabPage);
            //tabControl1.TabPages.Add(tabPage1);
            callTabPage.Show();

            TabPage tabPage2 = new TabPage();
            tabPage2.Text = "老化测试";
            tabPage2.Name = "AgeingTest";
            tabControl1.Controls.Add(tabPage2);
            AgeingCall ageCall = new AgeingCall();
            ageCall.TopLevel = false;
            tabPage2.Controls.Add(ageCall);
            ageCall.Show();

            TabPage tabPage3 = new TabPage();
            tabPage3.Text = "添加项目";
            tabPage3.Name = "AddProject";
            tabControl1.Controls.Add(tabPage3);
            TabAddProject addProject = new TabAddProject();
            addProject.TopLevel = false;
            tabPage3.Controls.Add(addProject);
            addProject.Show();
        }
        

    }
}
