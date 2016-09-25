using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml;

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
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null && textBox2.Text != null && textBox3.Text != null)
            {
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
                
            }
        }
    }
}
