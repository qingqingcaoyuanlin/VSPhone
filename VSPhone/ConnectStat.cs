using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class ConnectStat
    {
        static public TalkType CallType;             					//呼入/呼出
        static public int NewCallFlag;          					//新呼叫标志
        static public byte[] Currentalk_IP = new byte[4];   						//当前通话的IP
        static public byte[] Currentalk_Mac = new byte[6];   					//当前通话的Mac
        static public byte[] Currentalk_Addr = new byte[6];
        static public CurrentalkType Currentalk_Type;      					//当前通话的类型 户户通，户内通等
        static public int Currentalk_Transflag; 					//当前通话是否呼叫转移
        static public int Busy_Flag;             					//对方忙标志
        static public byte[] CallTrans_Addr = new byte[6];    					//发起呼叫转移的地址
        static public int Intercept_flag;      						//呼叫拦截标志
        static public byte[] Intercept_Addr = new byte[6];   					//呼叫拦截地址
        static public int Intercept_os_isonline;  					//标识呼叫拦截中门口机是否在线
        static public byte[] Video_SourceIP = new byte[4];   					//视频源的IP地址
    
        static public byte[] NewCall_IP= new byte[4];       						//通话过程中新呼入的IP
        static public byte[] NewCall_Mac = new byte[6];     						//新呼入的Mac
        static public byte[] NewCall_Addr = new byte[6];    						//新呼入的地址
        static public int NewCall_Type;      						//新通话的类型
        static public int NewCall_Transflag; 						//当前通话是否呼叫转移
        static public int NewCall_Intercept_flag;  				//新呼叫拦截标志
        static public byte[] NewCall_Intercept_Addr = new byte[6]; 				//新呼叫拦截地址
        static public int NewCall_Intercept_os_isonline;  		//标识呼叫拦截中门口机是否在线
        public enum CurrentalkType
        {
            GATETOINDOOR = 0,   	//围墙机/门口机和室内分机通话
            MANAGETOINDOOR = 1,  	 	//管理机和室内分机通话
            GATETOMANAGE = 2,   	//围墙机/门口机呼叫管理机
            HOUSETOHOUSE = 3,   	//户户通
            INDOORCALL = 4,   	//户内通
            INDOORRADIO = 5,   	//户内广播
            MONITORING = 6,   	//监控状态
            MINIOSCALL = 7,   	//小门口机通话
            IPCAM_MONITORING = 8,  		//监控IP摄像头
        }

        public enum TalkType
        {
             CALL_OUT    				=0x01,	//呼出
             CALL_IN     				=0x02,	//呼入
             CALL_MONITOR     			=0x03,	//监视
        }
    }
    
}
