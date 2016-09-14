using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static class UdpApp
    {
        public delegate void StatFun(byte[] buff, byte[] FromIP, int DestPort, int recv_len);
        static bool udp_filter(byte[] addr)
        {
            return true;
        }
        static StatFun[] stat_fun;
        static Talkback talkback;
        static public void UdpAppInit(Talkback talkback)
        {
            UdpApp.talkback = talkback;
            stat_fun = new StatFun[]
            {
                talkback.STA_NORMAL_STANDBY_Deal,
                talkback.STA_WAIT_CALL_REPLY_Deal,
                talkback.STA_WAIT_PICK_Deal,
                talkback.STA_WAIT_PICK_ASK_Deal,
                talkback.STA_WAIT_PICK_REPLY_Deal,
                talkback.STA_WAIT_MONITOR_REPLY_Deal,
                talkback.STA_MONITOR_STAT_Deal,
                talkback.STA_MONITOR_CALL_Deal,
                talkback.STA_WAIT_HANDUP_REPLY_Deal,
                talkback.STA_CALLING_Deal,
                talkback.STA_INDOOR_PAGING_Deal,
                talkback.STA_WAIT_RADIO_REPLY_Deal,
                talkback.STA_WAIT_TRANS_REPLY_Deal,
            };
        }
        static public void udp_deal(byte[] ip, int source_port, byte[] buff, int len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            if (ArrayDeal.vs_strstr(Form1.PublicHead, pack.PublicHead, 10) == 0 )//&&
            //    ArrayDeal.vs_strstr(onvif_manage.PublicHead, pack.PublicHead, 10) == 0)
	        {
		        //printf("public head no match\r\n");
		        return;
	        }

            if (udp_filter(pack.DestAddr) == false)
	        {
		        //vs_printf2("udp_filter: ",addr, 0, 6);
		        return;
	        }
            switch (pack.FunCode)  //第28位为功能码，区分是哪个功能模块的指令
            {
                case 0x01:   	//生产测试指令
                    break;
                case 0x02:   	//系统配置指令     
                    break;
                case 0x03:   	//呼叫对讲相关指令
        	       // if(Call_Record_Common_Deal(buff, ip, source_port, len) == AK_SUCCESS)
        	       // {
        		   //     break;
        	       // }
                    stat_fun[(int)talkback.get_talkback_state()](buff, ip, source_port, len);		
                    break;
                case 0x04:   	//门禁功能相关指令           
                   //DoorFunc_Deal(ip, source_port, buff, len);           
                   break;
                case 0x05:
        	        //AlarmUdpDeal(ip, source_port, buff, len); 
        	        break;
                case 0x06:   	//信息交互功能相关指令        
        	        //Information_Deal(ip, source_port, buff, len);
                    break;
                case 0x07:   	//管理应用功能相关指令
                    //Manage_Common_Deal(ip, source_port, buff, len);
                    break;
                case 0x08:   	//电梯联动相关指令
                    break;
                case 0x10:  	//户内相关功能指令
                    /*
        	        if(Indoor_Func_Deal(ip, source_port, buff, len) == -1)
        	        {
            	        MiniOS_Common_Deal(ip, source_port, buff, len);
                    }*/
                    break;
                case 0x0a:	 	//网络摄像头监控相关指令
			        //Onvif_ComPort_Deal(buff, ip, source_port, len);
                    break;
                case 0x20:            
                    //File_Transfer_Common_Deal(buff, ip, source_port, len);
                    break;
                default:
                    break;    
            }
        }
    }
}
