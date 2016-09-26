using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class VsProtocol
    {
        private const int FLOORNUMMAX = 63;     //楼层号最大值
        private const int ROOMNUMMAX = 32;     //房间号最大值
        private const int NET_FID_TOTAL = 512;  //系统保留的公共组播IP数
        private const int UNIT_HOME_FONCTION_OFFSET = 512;  //每个单元保留的公共组播地址
        private const int UNIT_OFFSET = (FLOORNUMMAX * ROOMNUMMAX + UNIT_HOME_FONCTION_OFFSET);

        static private byte[] Random_Code = new byte[8];
        static private byte[] Cipher_Text = new byte[8];
        static private byte[] FactoryNum = new byte[8]{ (byte)'G', (byte)'V', (byte)'S', (byte)'&', (byte)'A', (byte)'B', (byte)'B',0 };
        static public int Change_IDToIP(byte[] ID, byte[] IP)
        {
            DevType DevType;   //类型
            int Type;
            int Num;    //编号
            int Unit_Num; //梯口号
            int Floor_Num; //楼层号
            int Room_Num;   //房号
            int Villa_Num;  //别墅号
            IP[0] = 10;   //固定为10段
            DevType = (DevType)ID[0];
            if (DevType == DevType.DEV_INDOORPHONE)  //室内机
            {
                Unit_Num = BcdDeal.BCDToInt(ID[1]) * 10 + BcdDeal.BCDToInt(ID[2]);
                if (Unit_Num > 999)
                {
                    //printf("Unit_Num %d out of the range 0-999\n",Unit_Num);
                    return 0;
                }
                if (Unit_Num == 0)  //别墅型
                {
                    Unit_Num = 1023;  //映射到1023
                    Villa_Num = BcdDeal.BCDToInt(ID[3]) * 100 + BcdDeal.BCDToInt(ID[4]);
                    Num = BcdDeal.BCDToInt(ID[5]);
                    IP[1] = (byte)((Unit_Num >> 2) & 0xff);
                    IP[2] = (byte)(((Unit_Num & 0x03) << 6) | ((Villa_Num >> 5) & 0xff));
                    IP[3] = (byte)((Villa_Num & 0x1f) | ((Num - 1) << 5));
                }
                else
                {
                    Floor_Num = BcdDeal.BCDToInt(ID[3]);
                    if (Floor_Num < 1 || Floor_Num > 63)
                    {
                        //printf("Floor_Num %d out of the range 1-63\n",Floor_Num);
                        return 0;
                    }
                    Room_Num = BcdDeal.BCDToInt(ID[4]);
                    if (Room_Num < 1 || Room_Num > 32)
                    {
                        //printf("Room_Num %d out of the range 1-32\n",Floor_Num);
                        return 0;
                    }
                    Num = BcdDeal.BCDToInt(ID[5]);
                    if (Num < 1 || Num > 6)
                    {
                        //printf("Number %d out of the range 1-6\n",Num);
                        return 0;
                    }
                    IP[1] = (byte)((Unit_Num >> 2) & 0xff);
                    IP[2] = (byte)(((Unit_Num & 0x03) << 6) | Floor_Num);
                    //  IP[3] = ((Room_Num-1) << 3) | (Num-1);
                    IP[3] = (byte)(((Num - 1) << 5) | (Room_Num - 1));
                }

            }
            else if (DevType == DevType.DEV_SECONDOORSTATION)  //二次门口机
            {
                Unit_Num = BcdDeal.BCDToInt(ID[1]) * 10 + BcdDeal.BCDToInt(ID[2]);
                if (Unit_Num > 999)
                {
                    //printf("Unit_Num %d out of the range 0-999\n",Unit_Num);
                    return 0;
                }
                if (Unit_Num == 0)  //别墅型二次门口机
                {
                    Unit_Num = 1023;  //映射到1023
                    Villa_Num = BcdDeal.BCDToInt(ID[3]) * 100 + BcdDeal.BCDToInt(ID[4]);
                    Num = BcdDeal.BCDToInt(ID[5]) + 6;
                    IP[1] = (byte)((Unit_Num >> 2) & 0xff);
                    IP[2] = (byte)(((Unit_Num & 0x03) << 6) | ((Villa_Num >> 5) & 0xff));
                    IP[3] = (byte)((Villa_Num & 0x1f) | ((Num - 1) << 5));
                }
                else
                {
                    Floor_Num = BcdDeal.BCDToInt(ID[3]);
                    if (Floor_Num < 1 || Floor_Num > 63)
                    {
                        //printf("Floor_Num %d out of the range 1-63\n",Floor_Num);
                        return 0;
                    }
                    Room_Num = BcdDeal.BCDToInt(ID[4]);
                    if (Room_Num < 1 || Room_Num > 32)
                    {
                        //printf("Room_Num %d out of the range 1-32\n",Floor_Num);
                        return 0;
                    }
                    Num = BcdDeal.BCDToInt(ID[5]) + 6;  //室内设备号 1-8 ，最后两个IP预留给二次门口机
                    if (Num < 7 || Num > 8)
                    {
                        //printf("Number0 %d out of the range 1-2\n",Num);
                        return 0;
                    }
                    IP[1] = (byte)((Unit_Num >> 2) & 0xff);
                    IP[2] = (byte)(((Unit_Num & 0x03) << 6) | Floor_Num);
                    //IP[3] = ((Room_Num-1) << 3) | (Num-1);
                    IP[3] = (byte)(((Num - 1) << 5) | (Room_Num - 1));
                }

            }
            else if ((DevType == DevType.DEV_DOORSTATION) || (DevType == DevType.DEV_ELEVOTAR) || (DevType == DevType.DEV_CARDUNIT))  //门口机、电梯联动模块、单元刷卡头
            {
                Unit_Num = BcdDeal.BCDToInt(ID[1]) * 10 + BcdDeal.BCDToInt(ID[2]);
                if (Unit_Num > 999)
                {
                    //printf("Unit_Num %d out of the range 0-999\n",Unit_Num);
                    return 0;
                }
                Num = BcdDeal.BCDToInt(ID[3]) * 100 + BcdDeal.BCDToInt(ID[4]);
                if (Num < 1 || Num > 32)
                {
                    //printf("Number %d out of the range 0-32\n",Num);
                    return 0;
                }
                switch (DevType)
                {
                    case DevType.DEV_DOORSTATION:  //门口机
                        Type = 0;
                        break;
                    case DevType.DEV_ELEVOTAR:  //电梯联动模块
                        Type = 1;
                        Num--;
                        break;
                    case DevType.DEV_CARDUNIT:  //单元刷卡头
                        Type = 2;
                        Num--;
                        break;
                    default:
                        return 0;
                }
                IP[1] = (byte)((Unit_Num >> 2) & 0xff);
                IP[2] = (byte)(((Unit_Num & 0x03) << 6));
                //IP[3] = (Type << 5) | (Num-1);
                IP[3] = (byte)(((Num - 1) << 3) | Type);

            }
            else  //围墙机、管理机、IPC、刷卡头
            {
                Num = BcdDeal.BCDToInt(ID[1]) * 100 + BcdDeal.BCDToInt(ID[2]);
                if (Num < 1 || Num > 32)
                {
                    //printf("Number %d out of the range 1-32\n",Num);
                    return 0;
                }
                switch (DevType)
                {
                    case DevType.DEV_PC:  //PC管理机
                        Type = 0;
                        break;
                    case DevType.DEV_GUARDUNIT:
                    case DevType.DEV_GUARD:  //硬件管理机
                        Type = 1;
                        break;
                    case DevType.DEV_PERIMETERGATE:  //围墙机
                        Type = 2;
                        break;
                    case DevType.DEV_IPCAMERA:  //IP摄像头
                        Type = 3;
                        break;
                    case DevType.DEV_CARD:  //刷卡头
                        Type = 4;
                        break;
                    default:
                        return 0;
                }
                IP[1] = 0;
                IP[2] = (byte)Type;
                IP[3] = (byte)Num;
            }
            return 1;
        }
        static public bool Set_PC_IP(byte[]IP)      //设置电脑IP
        {

            return true;
        }
        static public int Get_MulticastIP(MulticastIPType Type, byte[] ID, int Num, byte[] IP)
        {
            uint FID;     //设备编号
            uint MID;   //映射ID
            uint Unit_Num;
            int Room_Num;
            int Floor_Num;
            int Villa_Num;
            switch (Type)
            {
                case MulticastIPType.NET_MULICASTIP:
                    FID = (uint)Num;
                    MID = FID;
                    break;
                case MulticastIPType.UNIT_MULICASTIP:
                    Unit_Num = (uint)(BcdDeal.BCDToInt(ID[0]) * 10 + BcdDeal.BCDToInt(ID[1]));
                    FID = (uint)Num;
                    MID = (uint)(NET_FID_TOTAL + ((Unit_Num - 1) * UNIT_OFFSET) + FID);
                    break;
                case MulticastIPType.INDOOR_MULICASTIP:   //户内组播地址
                    Unit_Num = (uint)(BcdDeal.BCDToInt(ID[1]) * 10 + BcdDeal.BCDToInt(ID[2]));
                    if (Unit_Num == 0)  //别墅机组播分配
                    {
                        Unit_Num = 1023;  //将1023梯口号分配给别墅
                        Villa_Num = BcdDeal.BCDToInt(ID[3]) * 100 + BcdDeal.BCDToInt(ID[4]);  //别墅号
                        MID = (uint)(NET_FID_TOTAL + UNIT_HOME_FONCTION_OFFSET + +((Unit_Num - 1) * UNIT_OFFSET) + Villa_Num);
                    }
                    else
                    {
                        Floor_Num = BcdDeal.BCDToInt(ID[3]);
                        if (Floor_Num < 1 || Floor_Num > 63)
                        {
                            //printf("Floor_Num %d out of the range 1-63\n",Floor_Num);
                            return 0;
                        }
                        Room_Num = BcdDeal.BCDToInt(ID[4]);
                        if (Room_Num < 1 || Room_Num > 32)
                        {
                            //printf("Room_Num %d out of the range 1-32\n",Floor_Num);
                            return 0;
                        }
                        MID = (uint)(NET_FID_TOTAL + UNIT_HOME_FONCTION_OFFSET + ((Unit_Num - 1) * UNIT_OFFSET) + (Floor_Num - 1) * ROOMNUMMAX + Room_Num);
                    }
                    break;
                default:
                    //printf("ID TO IP :无效ID号\n"); 
                    return 0;
            }
            //printf("MID = %x",(MID>>16));
            //printf("%x\n",MID);
            IP[0] = 238;   //固定为238段
            IP[1] = (byte)((MID >> 16) & 0xff);
            IP[2] = (byte)((MID >> 8) & 0xff);
            IP[3] = (byte)(MID & 0xff);
            return 1;
        }

        static void UDP_Encrypt(byte[] ClipherNum,byte[] RamdomNum)
        {
            for(int i=0;i<8;i++)
            {
                ClipherNum[i] =(byte)((RamdomNum[i]<<1) ^ FactoryNum[7-i]);
            }
        }
        static void RamDomNum_Product(byte[] RamDomNum, int len)  //随机数产生
        {

            Random ran = new Random();
            for (int i = 0; i < len; i++)
            {
                RamDomNum[i] = (byte)ran.Next(0,255);
            }
        }
        static public void UDP_Encrypt_init()
        {
	        RamDomNum_Product(Random_Code,8);//产生随机数
	        UDP_Encrypt(Cipher_Text,Random_Code);//计算密文
	        //vs_printf2("Random_Code:",Random_Code,0,8);
	        //vs_printf2("Cipher_Text:",Cipher_Text,0,8);
        }
        static public byte[] PublicHead_Temp;
        static public void Get_PulicHead_Temp(byte[]head)
        {
            PublicHead_Temp = head;
        }
        public class Pack
        {
            //public byte[] PublicHead = new byte[] { (byte)'G', (byte)'V', (byte)'S', (byte)'G', (byte)'V', (byte)'S', 0xa5, 0xa5, 0xa5, 0xa5 };  //公共头信息
            //public byte[] PublicHead = new byte[] { 0x47,0x56, 0x53, 0xCC, 0xce, 0xcf, 0xce, 0xcf, 0xc8, 0x0 };  //公共头信息
            public byte[] PublicHead;
            public byte[] DestAddr;     //目的地址
            public byte[] SourceAddr;   //源地址
            public byte[] Random_Code;  //随机码
            public byte[] Cipher_Text;  //密文
            public int FunCode;          //功能码
            public int Common;           //指令码
            public COM com
            {
                get
                {
                    return (COM)((FunCode << 8) + Common);
                }
            }
            public int Datalength;      //数据长度
            public byte[] data;
            public Pack(byte[] dstAddr,int com)
            {
                //PublicHead = Form1.PublicHead;      //此处需修改
                PublicHead = CallTabPage.PublicHead;
                SourceAddr = LocalCfg.Addr;
                DestAddr = dstAddr;
                FunCode = com >> 8;
                Common = com & 0xFF;
                Random_Code = VsProtocol.Random_Code;
                Cipher_Text = VsProtocol.Cipher_Text;
            }
           
            public byte[] GetByte()
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(PublicHead,0,10);
                ms.Write(DestAddr, 0, 6);
                ms.Write(SourceAddr, 0, 6);
                ms.Write(Random_Code, 0, 8);
                ms.Write(Cipher_Text, 0, 8);
                ms.WriteByte((byte)FunCode);
                ms.WriteByte((byte)Common);
                ms.WriteByte((byte)(Datalength & 0xff));
                ms.WriteByte((byte)(Datalength >> 8 ));
                if (data != null)
                {
                    ms.Write(data, 0, data.Length);
                }
                ms.Position = 0;
                byte[] d = new byte[ms.Length];
                ms.Read(d,0,(int)ms.Length);
                ms.Close();
                return d;
            }
            public int GetLength()
            {
                if(data != null)
                {
                    return 10 + 6 + 6 + 8 + 8 + 4 + data.Length;
                }
                else
                {
                    return 10 + 6 + 6 + 8 + 8 + 4;
                }
            }

            public Pack(byte[] buff)
            {
                DestAddr = new byte[6];                
                Array.Copy(buff, 10, DestAddr,0,6);
                SourceAddr = new byte[6];
                Array.Copy(buff, 16, SourceAddr, 0, 6);
                PublicHead = new byte[10];
                Array.Copy(buff, 0,PublicHead, 0, 10);
                FunCode = buff[38];
                Common = buff[39];
                Datalength = ((int)buff[41] << 8) + buff[40];
                if (Datalength != 0)
                {
                    data = new byte[Datalength];
                    Array.Copy(buff, 42, data, 0, Datalength);
                }
            }
        }
        public enum MulticastIPType
        {
            NET_MULICASTIP = 0x01,   //系统功能保留的公共组播地址IP 根据功能进行分配
            UNIT_MULICASTIP = 0x02,    //单元内保留的公共组播地址IP  根据功能进行分配
            INDOOR_MULICASTIP = 0x03,    //为每户分配的组播IP
        }
        public enum DevType
        {
            DEV_UNKNOW = 0x00,    //未知设备
            DEV_TEST = 0x01,  //测试板
            DEV_PC = 0x11,   //PC管理机
            DEV_GUARD = 0x12,   //管理机
            DEV_PERIMETERGATE = 0x13,   //围墙机
            DEV_CARD = 0x14,    //联网刷卡头
            DEV_IPCAMERA = 0x15,   //IP摄像头
            DEV_GUARDUNIT = 0x31,  //单元管理机
            DEV_DOORSTATION = 0x32,    //门口机
            DEV_CARDUNIT = 0x34,   //单元刷卡头
            DEV_ELEVOTAR = 0x35,   //电梯联动模块
            DEV_INDOORPHONE = 0x61,    //室内机
            DEV_SECONDOORSTATION = 0x62,   //二次门口机
        }
        public enum COM
        {
            COM_CALLASK = 0x0301,     //呼叫指令
            COM_HANDUP = 0x0302,     //挂机指令
            COM_PICK = 0x0303,     //摘机指令
            COM_MONITORASK = 0x0304,     //监视请求指令
            COM_INDOORPAGING = 0x0307,     //户内寻呼(广播)
            COM_CALLTRANSFER = 0x0308,     //呼叫转移指令
            COM_MESSAGESYNC = 0x0309,     //留言同步指令
            COM_UPDATE_RECORD = 0x0310,     //通话记录上报
            COM_CAPTUREDIMAGEUPLOAD = 0x0311,  	//抓拍图片上传指令
            COM_CALLREPLY = 0x0381,     //呼叫应答指令
            COM_HANDUPREPLY = 0x0382,     //挂机应答指令
            COM_PICKREPLY = 0x0383,     //摘机应答指令
            COM_MONITORREPLY = 0x0384,     //监视应答指令
            COM_CALLTRANSFERREPLY = 0x0388,     //呼叫转移应答指令
            COM_MESSAGESYNCREPLY = 0x0389,     //留言同步应答指令
            COM_UPDATE_RECORD_REPLY = 0x0390,     //通话记录应答
            COM_CAPTUREDIMAGEUPLOADREPLY = 0x0391,  //抓拍图像上传应答指令
            COM_BUSY = 0x0350,     //正忙
            COM_HANDASK = 0x0351,     //握手请求指令
            COM_HANDREPLY = 0x0352,     //握手应答指令
            COM_RADIOASK = 0x0353,     //户内广播请求指令
            COM_RADIOREPLY = 0x0354,     //户内广播应答指令
            COM_VIDEOASK = 0x0355,     //请求发送视频
            COM_TIMECOUNTSYNC = 0x0356,     //倒计时同步请求
            COM_TIMECOUNTREPLY = 0x0357,     //倒计时同步请求应答
            COM_INTERCEPT_STAT_ASK = 0x0358,     //呼叫拦截状态请求指令
            COM_INTERCEPT_STAT_REPLY = 0x0359,     //呼叫拦截状态应答指令
            COM_INTERCEPT_CALL_ASK = 0x035a,   	//呼叫拦截状态下的呼叫指令
            COM_INTERCEPT_CALL_REPLY = 0x035b,  	//呼叫拦截状态下的呼叫应答指令
            COM_CALL_SUSPEND_ASK = 0x035c,     //呼叫挂起请求
            COM_CALL_SUSPEND_REPLY = 0x035d,     //呼叫挂起应答
            COM_CALL_RECOVER_ASK = 0x035e,     //呼叫恢复请求
            COM_CALL_RECOVER_REPLY = 0x035f,     //呼叫恢复应答
            COM_CALL_TURN_ASK = 0x0360,     //一键转呼指令
            COM_CALL_TURN_REPLY     = 	0x0361,     //一键转呼应答指令

        }
    }
}
