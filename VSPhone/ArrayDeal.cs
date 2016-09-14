using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class ArrayDeal
    {
        static public int vs_strstr(byte[] str1, byte[] str2, int length)
        {
            int i;
            for (i = 0; i < length; i++)
            {
                if (str1[i] != str2[i])
                {
                    return 0;
                }
            }
            return 1;
        }
        static public int memcpy(byte[] s,byte[] d,int len)
        {
            Array.Copy(d,0,s,0,len);
            return 0;
        }

    }
}
