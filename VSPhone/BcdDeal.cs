using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class BcdDeal
    {
        static public int BCDToInt(int bcd) //BCD转十进制
        {
            return ((0xf & (bcd >> 4)) * 10 + (0xf & bcd));
        }
    }
}
