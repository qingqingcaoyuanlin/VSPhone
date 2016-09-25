using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
namespace VSPhone
{
    class Project
    {
        public Project()
        {
            ADevice = new Device();
            ProjectDevices = new DeviceList();

        }
        public string Vendor;
        public string Header;

        public class Device
        {
            public string DeviceType;      //设备类型
            public string DeviceNum;       //设备的编号
            public Device(string _DeviceType, string _DeviceNum)
            {
                DeviceType = _DeviceType;
                DeviceNum = _DeviceNum;
            }
            public Device()
            { }
        }
        public Device ADevice;
        public class DeviceList : List<Device>
        {
            public DeviceList()
            {
                Devices = new List<Device>();
            }
            public List<Device> Devices;//{get;set;}
            public void DeviceListPrint()
            {
                foreach (Device dev in Devices)
                {
                    Console.WriteLine(dev.DeviceType + "+" + dev.DeviceNum);
                }
            }
            public void AddToDeviceList(string DeviceType, string[] DeviceNum)
            {
                try
                {
                    foreach (string num in DeviceNum)
                    {
                        if (num != null)
                        {
                            Device dev = new Device(DeviceType, num);
                            Devices.Add(dev);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("AddToDeviceList Ex");
                }
            }
        }
        public DeviceList ProjectDevices;

        public void ProjectPrint()
        {
            //Console.WriteLine("ProjectPrint");
            //Console.WriteLine(Vendor);
            // Console.WriteLine(Header);
            foreach (Device dev in ProjectDevices.Devices)
            {
                //     Console.WriteLine(dev.DeviceNum + "+"+ dev.DeviceType);
            }
            //Console.WriteLine("-------------");
        }

       
    }
    class ReadXML
    {
        List<Project> projects = new List<Project>();
        public ReadXML()
        {

            string fname = Directory.GetCurrentDirectory() + "\\projects.xml";
            FileInfo finfo = new FileInfo(fname);
            if (!finfo.Exists)
            {
                Console.WriteLine("file not exit");
            }
            else
            {
                XElement xe = XElement.Load("projects.xml");
                IEnumerable<XElement> elements = from PInfo in xe.Elements("Project")
                                                 where PInfo.Attribute("Vendor").Value != null && PInfo.Attribute("Header").Value != null
                                                 select PInfo;

                foreach (XElement element in elements)
                {
                    Project pro = new Project();
                    pro = ReadXeToProject(element);
                    if (pro != null)
                    {
                        projects.Add(pro);
                    }

                }
                foreach (Project my in projects)
                {
                    Console.WriteLine(my.Vendor + my.Header);
                    my.ProjectDevices.DeviceListPrint();
                    Console.WriteLine("+++++");

                }

            }
        }
        public List<Project.Device> ElementToProject(XElement xe, string devType)
        {
            string str;
            string[] strs;
            Project pro = new Project();
            //Console.WriteLine(devType);

            if (xe.Element(devType).Value != null)
            {
                str = xe.Element(devType).Value;
                strs = str.Split(new char[] { '、' });
                foreach (string my in strs)
                {
                    //Console.WriteLine(my + "、");                   
                }
                pro.ProjectDevices.AddToDeviceList(devType, strs);
                return pro.ProjectDevices.Devices;
            }
            return null;
        }
        public Project ReadXeToProject(XElement xe)
        {

            Project project = new Project();
            project.Header = xe.Attribute("Header").Value;
            project.Vendor = xe.Attribute("Vendor").Value;

            try
            {
                project.ProjectDevices.Devices.AddRange(ElementToProject(xe, "IS"));
                project.ProjectDevices.Devices.AddRange(ElementToProject(xe, "OS"));

                project.ProjectDevices.Devices.AddRange(ElementToProject(xe, "MiniOS"));
                project.ProjectDevices.Devices.AddRange(ElementToProject(xe, "GU"));
                // project.ProjectDevices.AddRange(project.ProjectDevices);
                project.ProjectPrint();
                return project;
            }
            catch (Exception e)
            {
                Console.WriteLine("ex--");
                MessageBox.Show(e.ToString());
                return null;
            }


        }
    }
}
