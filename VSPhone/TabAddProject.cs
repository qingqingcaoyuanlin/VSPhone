using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using System.Xml.Linq;
//using System.Xml;
using MySql.Data.MySqlClient;

namespace VSPhone
{
    public partial class TabAddProject : Form
    {
        public TabAddProject()
        {
            InitializeComponent();
            InitSetting();
        }
        public void InitSetting()
        {            
            object[]comText = new object[]{"IS","OS","MiniOS","GU"};
            comboBox1.Items.AddRange(comText);
            comboBox1.SelectedIndex = 0;
            DataTable dt = new DataTable();
            dt = DataBase.queryTable(DataBase.DeviceTable);
            dataGridView1.DataSource = dt;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string[] num;
            if (textBox1.Text != null && textBox2.Text != null && textBox3.Text != null)
            {
                try
                {
                    String[] number = textBox2.Text.Split(' ');
                    byte[] temp = new byte[10];
                    if (number.Length == 10)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            temp[i] = (byte)Convert.ToByte(number[i], 16);
                            Console.WriteLine(temp[i]);

                        }                       
                    }
                    else
                    {
                        Console.WriteLine(number.Length);
                        MessageBox.Show("头长度错误，应为10");
                        return;
                    }

                }
                catch
                {
                    MessageBox.Show("头错误");
                    return;
                }
                switch(comboBox1.Text)
                {
                    case "IS":
                        try
                        {
                            num = textBox3.Text.Split(new char[] { '-' });
                            byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_INDOORPHONE, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };
                        }
                        catch
                        {
                            MessageBox.Show("输入格式错误");
                            return;
                        }
                        
                        break;
                    case "OS":                       
                        try
                        {
                            num = textBox1.Text.Split(new char[] { '-' });
                            if (num.Length == 3)
                            {
                                byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_DOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), 0, Convert.ToByte(num[2]), 1 };                               
                            }
                            else
                            {
                                MessageBox.Show("输入格式错误");
                                return;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("输入格式错误");
                            return;
                        }
                        break;
                    case "MiniOS":
                        try
                        {
                            num = textBox1.Text.Split(new char[] { '-' });
                            if (num.Length == 4)
                            {
                                byte[] callId = new byte[] { (byte)VsProtocol.DevType.DEV_SECONDOORSTATION, Convert.ToByte(num[0]), Convert.ToByte(num[1]), Convert.ToByte(num[2]), Convert.ToByte(num[3]), 1 };      
                            }
                            else
                            {
                                MessageBox.Show("输入格式错误");
                                return;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("输入格式错误");
                            return;
                        }
                        break;
                    case "GU":          //管理机暂时未完成
                        try
                        {

                        }
                        catch
                        {
                            MessageBox.Show("输入格式错误");
                            return;
                        }
                        break;
                    default:
                        return;

                }
                
                string cmd = "insert into device values (null,'" + textBox1.Text + "','" + textBox2.Text + "','" + comboBox1.Text + "','" + textBox3.Text + "')";
                if (DataBase.ExcuteCMD(cmd))
                {
                    MessageBox.Show("添加成功");
                }
                else 
                {
                    MessageBox.Show("添加失败");
                }
                /*
                string fname = Directory.GetCurrentDirectory() + "\\projects.xml";                
                FileInfo finfo = new FileInfo(fname);
                if (!finfo.Exists)
                {
                    finfo.Create();                    
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.CreateXmlDeclaration("1.0", "UTF-8", "");
                    xdoc.LoadXml("projects.xml");
                    XmlNode xn = xdoc.SelectSingleNode("Projects");
                    XmlElement xe = xdoc.CreateElement("Project");
                    xe.SetAttribute("Vendor",textBox1.Text);
                    xn.AppendChild(xe);
                    xe.SetAttribute("Header",textBox2.Text);
                    xn.AppendChild(xe);
                    XmlElement objElment = xdoc.CreateElement(comboBox1.Text);
                    objElment.InnerText = textBox3.Text;
                    xn.AppendChild(objElment);
                    xdoc.Save("projects.xml");
                    
                }
                else
                {
                   
                    XElement xe = XElement.Load("projects.xml");
                    IEnumerable<XElement> elements1 = from element in xe.Elements("Project")
                                                      where element.Attribute("Vendor").Value == textBox1.Text && element.Attribute("Header").Value == textBox2.Text
                                                      select element;

                    if (elements1.Count() <= 0)         //新建
                    {
                        XElement project = new XElement("Project",
                        new XAttribute("Vendor", textBox1.Text), new XAttribute("Header", textBox2.Text),
                        new XElement(comboBox1.Text, textBox3.Text));
                        xe.Add(project);
                        xe.Save("projects.xml");
                    }
                    else if(elements1.Count() > 1)
                    {
                        MessageBox.Show("项目重复，请检查projects.xml文件");
                    }
                    else if (elements1.Count() == 1)    //增加
                    {
                        XElement xel = elements1.First();
                        IEnumerable<XNode> xNodes = from node in xel.DescendantNodes()
                                                    select node;
                        string str = null;
                        foreach (XNode node in xNodes)
                        {                             
                            if((node as XElement).Name.ToString() == comboBox1.Text)
                            {
                                str += "、" + textBox3.Text;
                                
                            }
                        }
                        if(str != null)         //有节点
                        {                            
                            xel.ReplaceNodes(new XElement(comboBox1.Text, str));
                        }
                        else
                        {
                            xel.ReplaceNodes(new XElement(comboBox1.Text, textBox3.Text));
                            //xel.node
                            
                        }
                        xe.Save("projects.xml");
                        
                    }
                    
                }
                */
            }
        }
    }
}
