using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VSPhone
{
    static public class Output
    {
        static public Object outObject;
        static public void MessaggeOutput(string msg)
        {
            if(outObject != null)
            {
                RichTextBox outTextBox = outObject as RichTextBox;
                outTextBox.Invoke(new MethodInvoker(delegate { outTextBox.Text = msg+"\r\n" + outTextBox.Text; }), null);
                
            }
        }
    }
}
