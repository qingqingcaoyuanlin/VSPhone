using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class LocalCfg
    {
        static public byte[] IP = new byte[4];

        static public byte[] Addr = new byte[6];

        static public byte[] Addr_Guard = new byte[6];

        static public byte[] IP_Guard = new byte[4];

        static public byte[] Addr_GuardUnit = new byte[6] { (byte)VsProtocol.DevType.DEV_GUARDUNIT, 0xff, 0xff, 0xff, 0xff, 0xff};

        static public byte[] IP_GuardUnit = new byte[4];

        static public byte[] IP_Mulicast = new byte[4];

    }
}
