using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    public class Talkback
    {

        public const int DOORSTATION_MAX = 10;		//门口机最大数量
        public const int PERIMETERGATE_MAX = 10;		//围墙机最大数量

        private const int VIDEO_FORMAT = 0x00;    //支持的视频格式
        public const int RING_TIME      =   30;        //振铃时间30s
        public const int CALL_TIME      =   30;        //通话时间
        public const int CALLTRANS_TIME =   25;        //响铃5s后进行呼叫转移
        public const int CCTV_AMOUNT   =    1;         //摄像头数
        private const int AUDIO_FORMAT   =   0x00;      //音频格式

        public const int PUBLIC_HEAD_LEN	=		10;
        private const int COMMON_PORT		=	    8300;   //通信端口
        private const int AUDIO_PORT		=			8302;	//音频端口
        private const int VIDEO_PORT		=			8303;	//视频端口
        private const int FILE_PORT = 8304;	//文件流端口

        private const int HAND_TIMEOUT		=		1000;   //握手应答超时
        private const int HAND_INTERVAL = 3000;   //握手间隔

        public const int RING_TIME_MAX = 30;   //响铃超时时间  s
        public const int TALK_TIME_MAX		=		120; //通话超时时间  s
        public const int MONITOR_TIME_MAX	=		30;   //监视超时时间	s
        public const int MONITOR_CALL_TIME_MAX	=	120;  //监视主动对讲超时时间	s
        public const int INDOOR_RADIO_TIME_MAX	=	30;	  //户内广播超时时间  s

        public RecentCall recentCall = new RecentCall();    //最近通话模块
        private TalkbackManage talkback_manage = new TalkbackManage();
        public UdpDeal udpDeal = new UdpDeal();
        public Audio audioDeal;
        public Video videoDeal;
        public int talk_back_init()
        {
	        Console.WriteLine("talk_back_init...\r\n");
            VsProtocol.UDP_Encrypt_init();
	        set_talkback_state(STA.STA_NORMAL_STANDBY);
            talkback_manage.hand_sendtimer = AppTimer.register_timer(null, T_send_hand_ask, 0, null, HAND_INTERVAL, 0);
            talkback_manage.hand_replytimer = AppTimer.register_timer(null, T_hand_timeout, 0, null, HAND_TIMEOUT, 0);
            talkback_manage.countdown_timer = AppTimer.register_timer(null, T_countdown_deal, 0, null, 800, 0);
	        //talkback_msg_init();
            audioDeal = new Audio(8302);
            videoDeal = new Video(8303);
            talkback_manage.toggle_audio_recv = audioDeal.toggle_audio_recv;
            talkback_manage.toggle_audio_send = audioDeal.toggle_audio_send;
            talkback_manage.stop_audio_recv = audioDeal.stop_audio_recv;
            talkback_manage.stop_audio_send = audioDeal.stop_audio_send;
            talkback_manage.toggle_video_recv = videoDeal.start_video_recv;
            talkback_manage.stop_video_recv = videoDeal.stop_video_recv;
	        return 0;
        }
 
        public int call_out(byte[] Addr, int CallTransferflag, byte[] CallTrans_Addr)
        {
            int Time_Count;
            Console.WriteLine("call_out:" + Addr);
            switch ((VsProtocol.DevType)Addr[0])
            {
                case VsProtocol.DevType.DEV_INDOORPHONE:   //室内机
                    {
                        if (ArrayDeal.vs_strstr(Addr, LocalCfg.Addr, 5) == 1)
                        {
                            VsProtocol.Change_IDToIP(Addr, ConnectStat.Currentalk_IP);
                            ConnectStat.Currentalk_Type = ConnectStat.CurrentalkType.INDOORCALL; //户内通
                            ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
                        }
                        else
                        {
                            if (VsProtocol.Get_MulticastIP(VsProtocol.MulticastIPType.INDOOR_MULICASTIP, Addr, 0, ConnectStat.Currentalk_IP) == 0)
                            {
                                Console.WriteLine("Call Out Addr erro...");
                                return 0;
                            }
                            ConnectStat.Currentalk_Type = ConnectStat.CurrentalkType.HOUSETOHOUSE; //户户通                    
                            ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
                            recentCall.update_recent_call(Addr);
                        }
                        Time_Count = 1000;
                    }
                    break;
                case VsProtocol.DevType.DEV_GUARD:   //管理机
                    {

                        if (ArrayDeal.vs_strstr(Addr, LocalCfg.Addr_Guard, 6) == 1)  //群呼
                        {
                            ArrayDeal.memcpy(ConnectStat.Currentalk_IP, LocalCfg.IP_Guard, 4);
                        }
                        else   //单呼
                        {
                            VsProtocol.Change_IDToIP(Addr, ConnectStat.Currentalk_IP);
                        }
                        ConnectStat.Currentalk_Type = ConnectStat.CurrentalkType.MANAGETOINDOOR; //分机呼叫总管理机
                        Time_Count = 600;
                        ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
                    }
                    break;
                case VsProtocol.DevType.DEV_GUARDUNIT:  //单元管理机
                    {
                        if (ArrayDeal.vs_strstr(Addr, LocalCfg.Addr_GuardUnit, 6) == 1)  //群呼
                        {
                            ArrayDeal.memcpy(ConnectStat.Currentalk_IP, LocalCfg.IP_GuardUnit, 4);
                        }
                        else   //单呼
                        {
                            VsProtocol.Change_IDToIP(Addr, ConnectStat.Currentalk_IP);
                        }
                        ConnectStat.Currentalk_Type = ConnectStat.CurrentalkType.MANAGETOINDOOR; //分机呼叫管理机
                        Time_Count = 600;
                        ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
                    }
                    break;
                default:
                    Console.WriteLine("call out: wrong number!");
                    return 0;
            }
            talkback_manage.Clear();
            ConnectStat.Busy_Flag = 0;
            ConnectStat.Currentalk_Transflag = CallTransferflag;
            if (CallTrans_Addr != null)
            {
                ArrayDeal.memcpy(ConnectStat.CallTrans_Addr, CallTrans_Addr, 6);
            }
            ConnectStat.CallType = ConnectStat.TalkType.CALL_OUT;
            Send_CallAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 0, CallTransferflag, ConnectStat.Currentalk_Type, CallTrans_Addr);
            AppTimer.start_timer(AppTimer.register_timer(null, T_SendCallAsk, 0, null, Time_Count, 3));
            set_talkback_state(STA.STA_WAIT_CALL_REPLY);
            return 1;
        }
        public int build_monitor(byte[] Addr)	 //建立监视
        {
            byte[] IP = new byte[4];
            if(VsProtocol.Change_IDToIP(Addr, IP) == 0)
            {
    	        Console.WriteLine("build_monitor: change ip erro\r\n");
    	        return -1;
            }
            talkback_manage.Clear();
            ArrayDeal.memcpy(ConnectStat.Currentalk_Addr,Addr,6);
            ArrayDeal.memcpy(ConnectStat.Currentalk_IP, IP, 4);
            ConnectStat.Busy_Flag = 0;
            ConnectStat.CallType =ConnectStat.TalkType.CALL_MONITOR;
            ConnectStat.Currentalk_Type =ConnectStat.CurrentalkType.MONITORING;
            set_talkback_state(STA.STA_WAIT_MONITOR_REPLY);
            Send_MonitorAsk(Addr, IP, COMMON_PORT);  		//发送监视请求
            AppTimer.start_timer(AppTimer.register_timer(null, T_SendMonitorAsk, 0, null, 500, 3));
            return 0;
        }
        public void pick()
        {
	        Console.WriteLine("pick...\r\n");
            Output.MessaggeOutput("摘机");
	        switch(get_talkback_state())
	        {
		        case STA.STA_WAIT_PICK:
			        stopring(0);
                    Send_PickAsk(ConnectStat.Currentalk_Addr,ConnectStat.Currentalk_IP, COMMON_PORT);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_SendPickAsk, 0, null, 500, 3));
                    set_talkback_state(STA.STA_WAIT_PICK_REPLY);
			        break;
                case STA.STA_MONITOR_STAT:   		//监视时主动摘机进行主动对讲
                    Send_PickAsk(ConnectStat.Currentalk_Addr,ConnectStat.Currentalk_IP, COMMON_PORT);
                    AppTimer.start_timer(AppTimer.register_timer(null, T_SendPickAsk, 0, null, 500, 3));
                    break;
		        default:
			        break;
	        }
        }
        public void ringing(int flag)   // 0-无视频  1-有视频
        {
            if (talkback_manage.ring_flag == 0)
            {
                talkback_manage.ring_flag = 1;
                talkback_manage.ring_count = 0;
                AppTimer.start_timer(talkback_manage.countdown_timer);
                if (talkback_manage.start_ring != null)
                {
                    talkback_manage.start_ring();
                }
                if (flag > 0)
                {
                    if (talkback_manage.toggle_video_recv != null)
                    {
                        talkback_manage.toggle_video_recv();
                    }
                }
            }
        }
        public void stopring(int flag)    //0 - 不关视频  1-关视频
        {
            if (talkback_manage.ring_flag == 1)
            {
                AppTimer.stop_timer(talkback_manage.countdown_timer);
                if (talkback_manage.stop_ring != null)
                {
                    talkback_manage.stop_ring();
                }
                if (flag > 0)
                {
                    if (talkback_manage.stop_video_recv != null)
                    {
                        talkback_manage.stop_video_recv();
                    }
                }
                talkback_manage.ring_flag = 0;
            }
        }
        public void handup(int type, int flag)  //flag=0 正常挂机    flag = 1 过渡挂机
        {
            //TALKBACK_MSG msg;
            //TALKBACK_INFO info;
            //printf("handup...\r\n");
            switch (get_talkback_state())
            {
                case STA.STA_MONITOR_STAT:   				//监视状态
                    Stop_Hand();
                    stop_monotor();
                    break;
                case STA.STA_WAIT_MONITOR_REPLY:  			//等待监视应答状态
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendMonitorAsk));
                    break;
                case STA.STA_WAIT_CALL_REPLY:     			//等待呼叫应答状态
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallAsk));
                    stopring(1);
                    break;
                case STA.STA_WAIT_RADIO_REPLY:    			//等待户内广播应答状态
                   // AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendRaidoAsk));
                   // stop_indoor_radio();
                    break;
                case STA.STA_INDOOR_PAGING:   				//户内广播状态
                    /*
                    {
                        if (ConnectStat.CallType == CALL_IN) //不能挂断户内广播，只能由发起方挂断
                        {
                            return;
                        }
                        else
                        {
                            stop_indoor_radio();
                        }
                    }
                     * */
                    break;
                case STA.STA_WAIT_PICK_ASK:   				//等待摘机指令状态
                    Stop_Hand();
                    stopring(1);
                    break;
                case STA.STA_WAIT_PICK:      				//等待用户摘机状态
                    Stop_Hand();
                    stopring(1);
                    break;
                case STA.STA_WAIT_PICK_REPLY:  				//等待摘机应答
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendPickAsk));
                    stopring(1);
                    Stop_Hand();
                    break;
                case STA.STA_MONITOR_CALL: 					//监视主动对讲
                    Stop_Hand();
                    stop_monotor_call();
                    break;
                case STA.STA_CALLING:     //通话中
                    Stop_Hand();
                    stop_talking();
                    break;
                case STA.STA_WAIT_TRANS_REPLY:
                    //AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallTransferAsk));
                    break;
                case STA.STA_ONVIF_MONITOR:
                    //onvif_handup_deal(type, flag);
                    return;
                default:
                    return;
            }

            if (flag > 0)
            {
                Send_HandUpAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 1);
                set_talkback_state(STA.STA_NORMAL_STANDBY);
            }
            else
            {
                Send_HandUpAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 0);
                AppTimer.start_timer(AppTimer.register_timer(null, T_SendHandUpAsk, 0, null, 500, 3));
                set_talkback_state(STA.STA_WAIT_HANDUP_REPLY);
            }
            //msg.cmd = MSG_HAND_UP;
            //msg.arg[0] = type;
            //get_talkback_info(&info);	//收集本次通话信息
            //memcpy(msg.arg + 1, &info, sizeof(TALKBACK_INFO));
            //broadcast_talkback_msg(&msg);
        }

        void Start_Hand()  //开始握手，发送心跳包
        {
            Refresh_Hand();
            AppTimer.start_timer(talkback_manage.hand_sendtimer);
        }
        void Stop_Hand() //停止握手
        {
            AppTimer.stop_timer(talkback_manage.hand_sendtimer);
            AppTimer.stop_timer(talkback_manage.hand_replytimer);
        }
        void Refresh_Hand() //刷新握手
        {
            talkback_manage.hand_count = 0;
            AppTimer.reset_timer_count(talkback_manage.hand_sendtimer); //刷新发送握手的定时器
        }
        void Hand_Ask_Deal(byte[] Addr, byte[] FromIP, int DestPort) //握手请求处理
        {
            Refresh_Hand();						   //刷新发送握手的定时器
            Send_HandReply(Addr, FromIP, DestPort); //返回握手应答
        }
        void Hand_Reply_Deal(byte[] Addr, byte[] FromIP) //握手应答处理
        {
            AppTimer.stop_timer(talkback_manage.hand_replytimer);//停止握手应答超时定时器
            Refresh_Hand();
        }


        void Send_CallAsk(byte[] Addr, byte[] DestIP, int DestPort, int Resendflag, int CallTransferflag, ConnectStat.CurrentalkType CallType, byte[] CallTrans_Addr)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, 0x0301);
            //AK_Obtain_Semaphore(pack_lock, AK_SUSPEND);
            Console.WriteLine("Send_CallAsk....");
            //数据
            byte[] send_b = new byte[15];
            send_b[0] = VIDEO_FORMAT;        		//视频格式
            send_b[1] = (byte)(VIDEO_PORT & 0xFF);    //视频端口(高8位)
            send_b[2] = (byte)(VIDEO_PORT >> 8 );       //视频端口(低8位)
            send_b[3] = (byte)Resendflag;      		//重发标志
            send_b[4] = (byte)CallTransferflag;	    //呼叫转移标志
            send_b[5] = 0;
            send_b[6] = RING_TIME;        		//振铃时间
            send_b[7] = (byte)CallType;                //呼叫类型 
            send_b[8] = CCTV_AMOUNT;              //摄像头数量
            if (CallTrans_Addr != null)
            {
                Array.Copy(send_b, 9, CallTrans_Addr, 0, 6);
            }
            pack.data = send_b;
            pack.Datalength = 15;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
            //AK_Release_Semaphore(pack_lock);
        }
        void Send_HandAsk(byte[] Addr, byte[] DestIP, int DestPort)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, 0x0351);
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_Busy(byte[] Addr, byte[] DestIP, int DestPort)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_BUSY);
            //数据
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_HandReply(byte[] Addr, byte[] DestIP, int DestPort)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_HANDREPLY);
            //数据
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_HandUpReply(byte[] Addr, byte[] DestIP, int DestPort)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_HANDUPREPLY);
            //数据
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_PickAsk(byte[] Addr, byte[] DestIP, int DestPort)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_PICK);
            //数据
            byte[] send_b = new byte[7];
            send_b[0] = VIDEO_FORMAT;                 //视频格式
            send_b[1] = (byte)(VIDEO_PORT >> 8);     //视频端口(高8位)
            send_b[2] = (byte)(VIDEO_PORT&0xff);        //视频端口(低8位)
            send_b[3] = AUDIO_FORMAT;              //音频格式
            send_b[4] = (byte)(AUDIO_PORT >> 8);     //音频端口(高8位)
            send_b[5] = (byte)(AUDIO_PORT & 0xff);        //音频端口(低8位)
            send_b[6] = CALL_TIME;                 //通话时间
            pack.Datalength = 7;
            pack.data = send_b;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_PickReply(byte[] Addr, byte[] DestIP, int DestPort)
        {

            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_PICKREPLY);
            //数据
            byte[] send_b = new byte[7];
            send_b[0]=VIDEO_FORMAT;                 //视频格式
            send_b[1]=(byte)(VIDEO_PORT>>8);      //视频端口(高8位)
            send_b[2]=(byte)(VIDEO_PORT&0xff);         //视频端口(低8位)
            send_b[3]=AUDIO_FORMAT;               //音频格式
            send_b[4]=(byte)(AUDIO_PORT>>8);      //音频端口(高8位)
            send_b[5]=(byte)(AUDIO_PORT &0xff);         //音频端口(低8位)
            send_b[6]=CALL_TIME;                  //通话时间
            pack.Datalength = 7;
            pack.data = send_b;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_HandUpAsk(byte[] Addr, byte[] DestIP, int DestPort, int handupflag)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_HANDUP);
            //数据
            byte[] send_b = new byte[1];
            send_b[0] = (byte)handupflag;     //挂机原因
            pack.Datalength = 1;
            pack.data = send_b;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_CallReply(byte[] Addr, byte[] DestIP, int DestPort, int Videoflag)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_CALLREPLY);
            //数据
            byte[] send_b = new byte[7];
            send_b[0] = (byte)Videoflag;                     	//是否要求发送视频
            send_b[1] = VIDEO_FORMAT;                 //视频格式
            send_b[2] = (byte)Local.Video_Resolution;       //视频分辨率  
            send_b[3] = (byte)(VIDEO_PORT >> 8);       	//视频端口(高8位)
            send_b[4] = (byte)(VIDEO_PORT&0xff);          	//视频端口(低8位)
            send_b[5] = RING_TIME;                   	// 振铃时间
            send_b[6] = CCTV_AMOUNT;                 	//摄像头数量
            pack.Datalength = 7;
            pack.data = send_b;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }
        void Send_MonitorAsk(byte[] Addr, byte[] DestIP, int DestPort) //监视应答指令
        {

            VsProtocol.Pack pack = new VsProtocol.Pack(Addr, (int)VsProtocol.COM.COM_MONITORASK);
            //数据
            byte[] send_b = new byte[4];
            send_b[0] = (byte)Local.Video_Resolution;          //视频类型
            send_b[1] = 30;                 //监视时间
            send_b[2] = 1;           //CCTV编号
            send_b[3] = 1;          //CCTV数量
            pack.Datalength = 4;
            pack.data = send_b;
            udpDeal.creat_multi_udp_pack2(DestIP, DestPort, pack.GetByte(), pack.GetLength(), 1, 1, 500, null, null);
        }

        void T_send_hand_ask(int argc, object argv)  //握手发送定时器
        {
            Send_HandAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT);
            AppTimer.reset_timer_count(talkback_manage.hand_sendtimer); //刷新发送握手的定时器
            AppTimer.start_timer(talkback_manage.hand_replytimer);
        }
        void T_hand_timeout(int argc, object argv) //握手超时定时器
        {
            talkback_manage.hand_count++;
            Console.WriteLine("hand count...%d\r\n", talkback_manage.hand_count);
            if (talkback_manage.hand_count >= 3)   //超过三次没收到握手应答表明对方不在线
            {
                Console.WriteLine("hand timeout...\r\n");
                //send_handup_msg(TYPE_DISCONNECTED);
            }
        }
        void T_countdown_deal(int argc, object argv)  //倒计时
        {
            //TALKBACK_MSG msg;
            //int time_max;
            //int time;
            //int count_down;
            switch (get_talkback_state())
            {
                case STA.STA_WAIT_PICK:
                case STA.STA_WAIT_PICK_ASK:
                    talkback_manage.ring_count++;
                    Console.WriteLine("ring_count:"+ talkback_manage.ring_count);
                    Output.MessaggeOutput("倒计时 ：" + talkback_manage.ring_count);
                    //time = talkback_manage.ring_count;
                    //time_max = RING_TIME_MAX;
                    //msg.cmd = MSG_RING_COUNT;
                    //count_down = RING_TIME_MAX - talkback_manage.ring_count;
                    //memcpy(msg.arg, &count_down, sizeof(count_down));
                    if (talkback_manage.ring_count == 5)
                    {
                        //call_transfer();
                    }
                    break;
                case STA.STA_CALLING:
                    if (ConnectStat.NewCallFlag > 0)
                    {
                        talkback_manage.new_call_ring_count++;
                    }
                    talkback_manage.talk_count++;
                    Output.MessaggeOutput("倒计时 ：" + talkback_manage.talk_count);
                    //Console.WriteLine("talk_count:%d\r\n", talkback_manage.talk_count);
                    //time = talkback_manage.talk_count;
                    //time_max = TALK_TIME_MAX;
                    //msg.cmd = MSG_TALK_COUNT;
                    //count_down = TALK_TIME_MAX - talkback_manage.talk_count;
                    //memcpy(msg.arg, &count_down, sizeof(count_down));
                    break;
                case STA.STA_MONITOR_STAT:
                    talkback_manage.monitor_count++;
                    Console.WriteLine("monitor_count:"+talkback_manage.monitor_count);
                    Output.MessaggeOutput("倒计时 ：" + talkback_manage.monitor_count);
                    //time = talkback_manage.monitor_count;
                    //time_max = MONITOR_TIME_MAX;
                    //msg.cmd = MSG_MONITOR_COUNT;
                    //count_down = MONITOR_TIME_MAX - talkback_manage.monitor_count;
                    //memcpy(msg.arg, &count_down, sizeof(count_down));
                    break;
                case STA.STA_MONITOR_CALL:
                    if (ConnectStat.NewCallFlag > 0)
                    {
                        talkback_manage.new_call_ring_count++;
                    }
                    talkback_manage.monitor_count++;
                    Console.WriteLine("monitor_call_count:"+talkback_manage.monitor_count);
                    Output.MessaggeOutput("倒计时 ：" + talkback_manage.monitor_count);
                    //time = talkback_manage.monitor_count;
                    //time_max = MONITOR_CALL_TIME_MAX;
                    //msg.cmd = MSG_MONITOR_COUNT;
                    //count_down = MONITOR_CALL_TIME_MAX - talkback_manage.monitor_count;
                    //memcpy(msg.arg, &count_down, sizeof(count_down));
                    break;
                case STA.STA_INDOOR_PAGING:
                    talkback_manage.radio_count++;
                    Console.WriteLine("radio_count:"+ talkback_manage.radio_count);
                    Output.MessaggeOutput("倒计时 ：" + talkback_manage.radio_count);
                    //time = talkback_manage.radio_count;
                    //time_max = INDOOR_RADIO_TIME_MAX;
                    //msg.cmd = MSG_RADIO_COUNT;
                    //count_down = INDOOR_RADIO_TIME_MAX - talkback_manage.radio_count;
                    //memcpy(msg.arg, &count_down, sizeof(count_down));
                    break;
                default:
                    return;
            }

            //if (time >= time_max)
            //{
                //Console.WriteLine("timeout...\r\n");
                //send_handup_msg(TYPE_TIMEOUT);
            //}
            //else
            //{
                //broadcast_talkback_msg(&msg);
            //}
        }
        void T_SendCallAsk(int argc, object argv)
        {
            int life = AppTimer.get_timer_life(AppTimer.get_current_deal_timer());
            if (life > 0)
            {
                Send_CallAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 0, ConnectStat.Currentalk_Transflag, ConnectStat.Currentalk_Type, ConnectStat.CallTrans_Addr);
            }
            else
            {
                if (Remoter.Remoter_get_count() > 0)
                {
                    Start_Hand();
                    set_talkback_state(STA.STA_WAIT_PICK_ASK);
                }
                else
                {
                    if (ConnectStat.Currentalk_Addr[0] == (byte)VsProtocol.DevType.DEV_GUARDUNIT)
                    {
                        call_out(LocalCfg.Addr_Guard, 0, null);      //转呼到总管理机
                    }
                    else
                    {
                        if (ConnectStat.Busy_Flag > 0)
                        {
                            send_handup_msg(HANDUP_TYPE.TYPE_BUSY);		//正忙
                            set_talkback_state(STA.STA_NORMAL_STANDBY);
                        }
                        else
                        {
                            send_handup_msg(HANDUP_TYPE.TYPE_NO_ANSWER);	//无应答
                            set_talkback_state(STA.STA_NORMAL_STANDBY);
                            if(AgeingCall.callFunc.CheckStateAgeing())
                            {
                                Console.WriteLine("测试无应答");
                                DataBase.recordStruct.CallSucceed = false;
                                DataBase.recordStruct.TimeEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                string cmd = "insert into record values (null,"+DataBase.CreateRecordString() + ")";
                                Console.WriteLine(cmd);
                                DataBase.ExcuteCMD(cmd);
                            }
                            

                        }
                    }
                }
            }
        }
        void T_SendPickAsk(int argc, object argv)
        {
            int life = AppTimer.get_timer_life(AppTimer.get_current_deal_timer());
            if (life > 0)
            {
                Send_PickAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT);
            }
            else
            {
                Console.WriteLine("wait pick reply timeout...\r\n");
                //send_handup_msg(TYPE_NO_ANSWER);	//无应答
            }
        }
        void T_SendHandUpAsk(int argc, object argv)
        {
            int life = AppTimer.get_timer_life(AppTimer.get_current_deal_timer());
            if (life > 0)
            {
                Send_HandUpAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 0);
            }
            else
            {
                Remoter.Remoter_Clear();
                set_talkback_state(STA.STA_NORMAL_STANDBY);
            }
        }
        void T_SendMonitorAsk(int argc, object argv)
        {
            int life = AppTimer.get_timer_life(AppTimer.get_current_deal_timer());
            if (life > 0)
            {
                Send_MonitorAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT);
            }
            else
            {
                if (ConnectStat.Busy_Flag > 0)
                {
                    send_handup_msg(HANDUP_TYPE.TYPE_BUSY);	//线路正忙
                }
                else
                {
                    send_handup_msg(HANDUP_TYPE.TYPE_NO_ANSWER);	 //无应答
                }
            }
        }

        void HandUp_Ask_Deal(byte[] Addr,byte[] buff, byte[] FromIP, int DestPort)
        {
	        //TALKBACK_MSG msg;
	        //TALKBACK_INFO info;
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
	        switch(get_talkback_state())
	        {
		        case STA.STA_WAIT_CALL_REPLY:
			        AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallAsk));   //停止发包
			        stopring(1);
			        break;
		        case STA.STA_WAIT_PICK:
			        Stop_Hand();
			        stopring(1);
			        break;
		        case STA.STA_WAIT_PICK_ASK:
			        Stop_Hand();
			        stopring(1);
			        break;
		        case STA.STA_WAIT_PICK_REPLY:
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendPickAsk)); 
			        Stop_Hand();
			        stopring(1);
			        break;
		        case STA.STA_MONITOR_STAT:
			        Stop_Hand();
			        stop_monotor();
			        break;
		        case STA.STA_MONITOR_CALL:
			        Stop_Hand();
			        stop_monotor_call();
			        break;
		        case STA.STA_CALLING:
			        Stop_Hand();
			        stop_talking();
			        break;
		        case STA.STA_INDOOR_PAGING:
			        Stop_Hand();
			        //stop_indoor_radio();
			        break;
		        case STA.STA_WAIT_HANDUP_REPLY:
			        break;
		        case STA.STA_WAIT_TRANS_REPLY:
                    //AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallTransferAsk));
			        break;
		        default:
			        return;
	        }
            if (pack.Datalength == 0x0) //正常挂机需回应答
            {
		        Send_HandUpReply(Addr, FromIP, DestPort);
	        }	

	        //msg.cmd = MSG_HAND_UP;
	        //msg.arg[0] = TYPE_NORMAL;
	        //get_talkback_info(&info);	//收集本次通话信息
	        //memcpy(msg.arg+1, &info,sizeof(TALKBACK_INFO));
	        //broadcast_talkback_msg(&msg);
	
	        if(ConnectStat.NewCallFlag > 0)
	        {
		        //Change_To_NewCall_Ring();
	        }
	        else
	        {
		        Remoter.Remoter_Clear();
		        set_talkback_state(STA.STA_NORMAL_STANDBY);
	        }	
        }
        void Call_Reply_Deal(byte[] Addr, byte[] buff, byte[] FromIP, int DestPort)
        {
            //TALKBACK_MSG msg;
            if (Remoter.Remoter_Search(FromIP) != -1)  //已经在列表中
            {
                return;
            }
            if (Remoter.Remoter_get_count() >= Remoter.REMOTEMAX)
            {
                Send_Busy(Addr, FromIP, DestPort); //返回忙
                return;
            }
            if (Remoter.Remoter_get_count() == 0)
            {
                ringing(0);
                //msg.cmd = MSG_BACK_RING;
                //broadcast_talkback_msg(&msg);
            }
            Remoter.Remoter_Add(Addr, FromIP, 0, 0);
        }
        void Pick_Ask_Deal(byte[] Addr, byte[] buff, byte[] FromIP, int DestPort)
        {
	        int i;
	        byte[] ip = new byte[4];
            byte[] addr = new byte[6];
	        //TALKBACK_MSG msg;
	        switch(get_talkback_state())
	        {
		        case STA.STA_WAIT_CALL_REPLY:
			        AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallAsk));
			        break;
		        case STA.STA_WAIT_PICK_ASK:
			        break;
		        default:
			        break;
	        }
	        stopring(0);
	        Send_PickReply(Addr, FromIP, DestPort); //发送摘机应答指令
            for (i = 0; i < Remoter.REMOTEMAX; i++)
	        {
                if (Remoter.Remoter_get_ip(i, ip) != -1)
		        {
			        if(ArrayDeal.vs_strstr(ip, FromIP, 4) > 0)
			        {
				        ArrayDeal.memcpy(ConnectStat.Currentalk_IP, FromIP, 4);
				        ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
			        }
			        else
			        {
				        Remoter.Remoter_get_addr(i, addr);
				        Send_HandUpAsk(addr, ip, COMMON_PORT, 1);
			        }
		        }
	        }
            Remoter.Remoter_Set_AudioFlag(FromIP, 1);
	        talking();
	        set_talkback_state(STA.STA_CALLING);
	        if(ConnectStat.Currentalk_Type ==ConnectStat.CurrentalkType.MANAGETOINDOOR)  //如果是分机呼叫管理机
            {
               // Send_VideoAsk(Addr, FromIP, DestPort, 1); //请求发送视频
		        if(talkback_manage.toggle_video_recv != null)
		        {
			        //talkback_manage.toggle_video_recv();
		        }
            }
	        //msg.cmd = MSG_TALKING;
	        //memcpy(msg.arg, Addr, 6);
	        //broadcast_talkback_msg(&msg);
        }
        void handup(HANDUP_TYPE type, int flag)  //flag=0 正常挂机    flag = 1 过渡挂机
        {
            //TALKBACK_MSG msg;
            //TALKBACK_INFO info;
            Console.WriteLine("handup...\r\n");
            switch (get_talkback_state())
            {
                case STA.STA_MONITOR_STAT:   				//监视状态
                    Stop_Hand();
                    stop_monotor();
                    break;
                case STA.STA_WAIT_MONITOR_REPLY:  			//等待监视应答状态
                    //AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendMonitorAsk));
                    break;
                case STA.STA_WAIT_CALL_REPLY:     			//等待呼叫应答状态
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallAsk));
                    stopring(1);
                    break;
                case STA.STA_WAIT_RADIO_REPLY:    			//等待户内广播应答状态
                   // AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendRaidoAsk));
                    //stop_indoor_radio();
                    break;
                case STA.STA_INDOOR_PAGING:   				//户内广播状态
                    {
                        if (ConnectStat.CallType ==  ConnectStat.TalkType.CALL_IN) //不能挂断户内广播，只能由发起方挂断
                        {
                            return;
                        }
                        else
                        {
                            //stop_indoor_radio();
                        }
                    }
                    break;
                case STA.STA_WAIT_PICK_ASK:   				//等待摘机指令状态
                    Stop_Hand();
                    stopring(1);
                    break;
                case STA.STA_WAIT_PICK:      				//等待用户摘机状态
                    Stop_Hand();
                    stopring(1);
                    break;
                case STA.STA_WAIT_PICK_REPLY:  				//等待摘机应答
                    AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendPickAsk));
                    stopring(1);
                    Stop_Hand();
                    break;
                case STA.STA_MONITOR_CALL: 					//监视主动对讲
                    Stop_Hand();
                    stop_monotor_call();
                    break;
                case STA.STA_CALLING:     //通话中
                    Stop_Hand();
                    stop_talking();
                    break;
                case STA.STA_WAIT_TRANS_REPLY:
                    //AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendCallTransferAsk));
                    break;
                case STA.STA_ONVIF_MONITOR:
                    //onvif_handup_deal(type, flag);
                    return;
                default:
                    return;
            }

            if (flag > 0)
            {
                Send_HandUpAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 1);
                set_talkback_state(STA.STA_NORMAL_STANDBY);
            }
            else
            {
                Send_HandUpAsk(ConnectStat.Currentalk_Addr, ConnectStat.Currentalk_IP, COMMON_PORT, 0);
                AppTimer.start_timer(AppTimer.register_timer(null, T_SendHandUpAsk, 0, null, 500, 3));
                set_talkback_state(STA.STA_WAIT_HANDUP_REPLY);
            }
            //msg.cmd = MSG_HAND_UP;
            //msg.arg[0] = type;
            //get_talkback_info(&info);	//收集本次通话信息
            //memcpy(msg.arg + 1, &info, sizeof(TALKBACK_INFO));
            //broadcast_talkback_msg(&msg);
        }
        void Call_In_Deal(byte[] Addr,byte[]buff, byte[] FromIP, int DestPort)   //呼入处理
        {
	        //TALKBACK_MSG msg;
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
	        Console.WriteLine("Call_In_Deal...\r\n");
	        talkback_manage.Clear();
	        ArrayDeal.memcpy(ConnectStat.Currentalk_IP, FromIP, 4);
	        ArrayDeal.memcpy(ConnectStat.Currentalk_Addr, Addr, 6);
	        ConnectStat.CallType =ConnectStat.TalkType.CALL_IN;
            ConnectStat.Currentalk_Type = (ConnectStat.CurrentalkType)pack.data[7]; //呼叫类型
	        ConnectStat.Currentalk_Transflag = buff[4]; //是否呼叫转移
            Remoter.Remoter_Add(Addr, FromIP, 0, 0);
            Console.WriteLine("Call_Type:"+ConnectStat.Currentalk_Type);
	        if(ConnectStat.Currentalk_Type ==ConnectStat.CurrentalkType.HOUSETOHOUSE)
	        {
		        //update_recent_call(Addr);
	        }
	        if(check_call_video_flag(ConnectStat.Currentalk_Type) > 0) //门口机呼叫室内机
	        {
	  	        Send_CallReply(Addr, FromIP, DestPort, 1); 	//返回呼叫应答，要求发送视频
		        ringing(1);	 //响铃  	 		
	        } 
	        else
	        {
	            Send_CallReply(Addr, FromIP, DestPort, 0); //返回呼叫应答，不要求发送视频
	            ringing(0);   //响铃
	        }
	        Start_Hand(); 							 //开始握手
	        //msg.cmd = MSG_RINGING;
	        //memcpy(msg.arg, Addr);
	        //broadcast_talkback_msg(&msg);			 //广播消息
	        set_talkback_state(STA.STA_WAIT_PICK);	 //进入等待摘机的状态
            Output.MessaggeOutput("呼入");
        }
        void Pick_Reply_Deal(byte[] Addr, byte[] buff, byte[] FromIP, int DestPort)   //摘机应答处理
        {
            //TALKBACK_MSG msg;
            switch (get_talkback_state())
            {
                case STA.STA_WAIT_PICK_REPLY:
                    talking();
                    set_talkback_state(STA.STA_CALLING);
                    break;
                case STA.STA_MONITOR_STAT:
                    monitor_calling();
                    set_talkback_state(STA.STA_MONITOR_CALL);
                    break;
            }
            AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendPickAsk));
            Remoter.Remoter_Set_AudioFlag(FromIP, 1);
            //msg.cmd = MSG_TALKING;
            //memcpy(msg.arg, Addr, 6);
            //broadcast_talkback_msg(&msg);
        }
        
        void Monitor_Reply_Deal(byte[] Addr, byte[] buff, byte[] FromIP, int DestPort)   //监视应答处理
        {
            //TALKBACK_MSG msg;
            AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendMonitorAsk));
            Remoter.Remoter_Add(Addr, FromIP, 0, 0);
            monitoring();
            set_talkback_state(STA.STA_MONITOR_STAT);
            //msg.cmd = MSG_MONOTORING;
            //memcpy(msg.arg, Addr, 6);
            //broadcast_talkback_msg(&msg);
        }
        void monitoring()	  //监视中
        {
	        talkback_manage.monitor_count = 0;
            AppTimer.start_timer(talkback_manage.countdown_timer);
	        if(talkback_manage.toggle_audio_recv != null)
	        {
		        talkback_manage.toggle_audio_recv();
	        }
            if (talkback_manage.toggle_video_recv != null)
	        {
		        talkback_manage.toggle_video_recv();
	        }
        }
        void HandUp_Reply_Deal(byte[] Addr, byte[] buff, byte[] FromIP, int DestPort)
        {
            Remoter.Remoter_Remove(FromIP);
            if (Remoter.Remoter_get_count() == 0)
            {
                AppTimer.destroy_timer(AppTimer.search_timer_by_func(T_SendHandUpAsk));
                set_talkback_state(STA.STA_NORMAL_STANDBY);
            }
        }


        void set_talkback_state(STA state)
        {
            lock (talkback_manage.state_lock)
            {
                talkback_manage.state = state;
            }
            Console.WriteLine("talkback_stat: "+ talkback_manage.state);
            //AK_Release_Semaphore(talkback_manage.state_lock);
            switch(state)
            {
                case STA.STA_CALLING:
                    Output.MessaggeOutput("通话中");
                    break;
            }
        }
        public STA get_talkback_state()
        {
	        lock(talkback_manage.state_lock)
            {
	            return talkback_manage.state;
	        }
        }
        int set_talkback_countdown(int time)
        {
            STA state;
            state = get_talkback_state();
            switch (state)
            {
                case STA.STA_WAIT_PICK:
                case STA.STA_WAIT_PICK_ASK:
                case STA.STA_WAIT_CALL_REPLY:
                    talkback_manage.ring_count = RING_TIME_MAX - time;
                    break;
                case STA.STA_MONITOR_STAT:
                    talkback_manage.monitor_count = MONITOR_TIME_MAX - time;
                    break;
                case STA.STA_MONITOR_CALL:
                    talkback_manage.monitor_count = MONITOR_CALL_TIME_MAX - time;
                    break;
                case STA.STA_CALLING:
                    talkback_manage.talk_count = TALK_TIME_MAX - time;
                    break;
                case STA.STA_WAIT_RADIO_REPLY:
                case STA.STA_INDOOR_PAGING:
                    talkback_manage.radio_count = INDOOR_RADIO_TIME_MAX - time;
                    break;
                default:
                    return -1;
            }
            return 0;
        }
        int check_call_video_flag(ConnectStat.CurrentalkType type)
        {
            switch (type)
            {
                case ConnectStat.CurrentalkType.GATETOINDOOR:
                case ConnectStat.CurrentalkType.MANAGETOINDOOR:
                case ConnectStat.CurrentalkType.MINIOSCALL:
                    return 1;
                default:
                    return 0;
            }
        }
        int call_priority_compare(int type1, int type2)  // -1:type1<type2  0: type1=type2 1:type1>type2  
        {
            int result = 1;
            Console.WriteLine("type1="+ type1+ type2);
            if(type1 < 0 || type2 < 0)
            {
                return -1;
            }
            if (type1 == type2)
            {
                return 0;
            }
            switch ((ConnectStat.CurrentalkType)type1)
            {
                case ConnectStat.CurrentalkType.INDOORCALL:
                    result = -1;
                    break;
                case ConnectStat.CurrentalkType.HOUSETOHOUSE:
                    if ((ConnectStat.CurrentalkType)type2 != ConnectStat.CurrentalkType.INDOORCALL)
                    {
                        result = -1;
                    }
                    break;
                case ConnectStat.CurrentalkType.GATETOINDOOR:
                    if ((ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.MANAGETOINDOOR || (ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.MINIOSCALL)
                    {
                        result = 0;
                    }
                    break;
                case ConnectStat.CurrentalkType.MANAGETOINDOOR:
                    if ((ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.GATETOINDOOR || (ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.MINIOSCALL)
                    {
                        result = 0;
                    }
                    break;
                case ConnectStat.CurrentalkType.MINIOSCALL:
                    if ((ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.MANAGETOINDOOR || (ConnectStat.CurrentalkType)type2 == ConnectStat.CurrentalkType.GATETOINDOOR)
                    {
                        result = 0;
                    }
                    break;
                default:
                    return -1;
            }
            return result;
        }
        int get_call_type(byte[] addr)
        {
            ConnectStat.CurrentalkType call_type;
            switch ((VsProtocol.DevType)addr[0])
            {
                case VsProtocol.DevType.DEV_INDOORPHONE:
                    if (ArrayDeal.vs_strstr(addr, LocalCfg.Addr, 5) > 0)  //户内通
                    {
                        call_type = ConnectStat.CurrentalkType.INDOORCALL;
                    }
                    else	//户户通
                    {
                        call_type = ConnectStat.CurrentalkType.HOUSETOHOUSE;
                    }
                    break;
                case VsProtocol.DevType.DEV_PERIMETERGATE:
                case VsProtocol.DevType.DEV_DOORSTATION:
                    call_type =ConnectStat.CurrentalkType.GATETOINDOOR;
                    break;
                case VsProtocol.DevType.DEV_SECONDOORSTATION:
                    call_type = ConnectStat.CurrentalkType.MINIOSCALL;
                    break;
                case VsProtocol.DevType.DEV_GUARD:
                case VsProtocol.DevType.DEV_GUARDUNIT:
                    call_type = ConnectStat.CurrentalkType.MANAGETOINDOOR;
                    break;
                default:
                    return -1;
            }
            return (int)call_type;
        }
        void send_handup_msg(HANDUP_TYPE type)
        {
            //TALKBACK_MSG msg;
            //msg.cmd = MSG_HANDUP;
            //msg.arg[0] = type;
            //send_to_talkback(&msg);
            switch (type)
            {
                case HANDUP_TYPE.TYPE_TIMEOUT:
                case HANDUP_TYPE.TYPE_NORMAL:
                    Output.MessaggeOutput("挂机");
                    break;
                case HANDUP_TYPE.TYPE_BUSY:
                    Output.MessaggeOutput("对方正忙");
                    break;
                case HANDUP_TYPE.TYPE_NO_ANSWER:
                    Output.MessaggeOutput("无应答");
                    break;
                case HANDUP_TYPE.TYPE_DISCONNECTED:
                    Output.MessaggeOutput("连接断开");
                    break;
            }
            
        }

        public void STA_NORMAL_STANDBY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_NORMAL_STANDBY_Deal..."+pack.com);
            
            switch(pack.com)
            {
                case VsProtocol.COM.COM_CALLASK:  //呼叫指令
                    Call_In_Deal(pack.SourceAddr,buff,FromIP, DestPort);
                    break;
                case VsProtocol.COM.COM_RADIOASK:  //户内广播
                    if (ArrayDeal.vs_strstr(pack.SourceAddr, LocalCfg.Addr, 5) > 0) //判断是否是本户
                    {
                        //Radio_Ask_Deal(pack.SourceAddr,FromIP,DestPort);
                    }
                    break;
                case VsProtocol.COM.COM_INTERCEPT_CALL_ASK:    //呼叫拦截的呼叫指令
                    {
                        Call_In_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                        ConnectStat.Intercept_flag = 1;
                        ConnectStat.Intercept_os_isonline = 1;
                        Array.Copy(pack.SourceAddr,0, ConnectStat.Intercept_Addr, 0, 6); 
                        ArrayDeal.memcpy(ConnectStat.Video_SourceIP, ConnectStat.Currentalk_IP, 4);
                        // Intecept_Video_Source = DEV_GUARD;
                    }
                    break;    
                default:
                    break;
            }
             

        }

        public void STA_WAIT_CALL_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_CALL_REPLY_Deal..." + pack.com);
            if (pack.com == VsProtocol.COM.COM_CALLASK || pack.com == VsProtocol.COM.COM_INTERCEPT_CALL_ASK)  //临界状态接收到呼叫指令返回忙
            {
                Send_Busy(pack.SourceAddr,FromIP,DestPort);
                return;
            }
            if(ConnectStat.Currentalk_Type ==ConnectStat.CurrentalkType.MANAGETOINDOOR)  //如果是分机呼叫管理机
            {
                if ((VsProtocol.DevType)pack.SourceAddr[0] != VsProtocol.DevType.DEV_GUARDUNIT && (VsProtocol.DevType)pack.SourceAddr[0] != VsProtocol.DevType.DEV_GUARD)
                {
                    return;
                }
            }
            else
            {
                if (ArrayDeal.vs_strstr(pack.SourceAddr, ConnectStat.Currentalk_Addr, 5) == 0) //地址
                {
                    return;
                }
            }
            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
               Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                Hand_Reply_Deal(pack.SourceAddr, FromIP);
                break;
            case VsProtocol.COM.COM_HANDUP:
                HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_CALLREPLY:
                Call_Reply_Deal(pack.SourceAddr, buff, FromIP, DestPort);  
              break;
            case VsProtocol.COM.COM_PICK:
                Pick_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);        
                break;
            case VsProtocol.COM.COM_BUSY:
                ConnectStat.Busy_Flag = 1;
                break;    
            default:
                break;
            }
        }

        public void STA_WAIT_PICK_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_PICK_Deal..."+pack.com);

            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                if(ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Reply_Deal(pack.SourceAddr, FromIP);
                }
                break;
            case VsProtocol.COM.COM_HANDUP:
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0) //地址
                {
                    HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_TIMECOUNTREPLY:   //倒计时同步应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0 || ArrayDeal.vs_strstr(LocalCfg.Addr, pack.DestAddr, 5) > 0)
                {
        	        set_talkback_countdown(pack.data[0]);
                }
                break;    
            case VsProtocol.COM.COM_INTERCEPT_CALL_ASK:   //呼叫拦截指令
            case VsProtocol.COM.COM_CALLASK:  //呼叫指令    
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4)>0)//即使之前的应答包丢包，此处具有补发作用,确保成功建立连接
    	        {
    		        if(check_call_video_flag(ConnectStat.Currentalk_Type) > 0)
    		        {
                        Send_CallReply(pack.SourceAddr, FromIP, DestPort, 1); //返回呼叫应答，要求发送视频
    		        }
    		        else
    		        {
                        Send_CallReply(pack.SourceAddr, FromIP, DestPort, 0); //返回呼叫应答，不要求发送视频
    		        }
    	        }
    	        else
    	        {
    		        if(call_priority_compare(get_call_type(pack.SourceAddr), (int)ConnectStat.Currentalk_Type) == 1)  //高优先级打断
    		        {
    			        Console.WriteLine("call interrupt...");
                        handup(HANDUP_TYPE.TYPE_INTERRUPT, 1);
    			        STA_NORMAL_STANDBY_Deal(buff, FromIP, DestPort, recv_len);
    		        }
    		        else
    		        {
                        Send_Busy(pack.SourceAddr, FromIP, DestPort);
    		        }
    	        }
                break;    
            default:
                break;
            }
           
        }

        public void STA_WAIT_PICK_ASK_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_PICK_ASK_Deal..."+pack.com);

            if (pack.com == VsProtocol.COM.COM_CALLASK || pack.com == VsProtocol.COM.COM_INTERCEPT_CALL_ASK)
            {
                Send_Busy(pack.SourceAddr,FromIP,DestPort);
                return;
            }
            if(ConnectStat.Currentalk_Type == ConnectStat.CurrentalkType.MANAGETOINDOOR)  //如果是分机呼叫管理机
            {
                if (pack.SourceAddr[0] != (byte)VsProtocol.DevType.DEV_GUARDUNIT && pack.SourceAddr[0] != (byte)VsProtocol.DevType.DEV_GUARD)
                {
                    return;
                }
            }
            else
            {
                if (ArrayDeal.vs_strstr(pack.SourceAddr, ConnectStat.Currentalk_Addr, 5) == 0) //地址
                {
                    return;
                }
            }
            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                Hand_Reply_Deal(pack.SourceAddr, FromIP);
                break;
            case VsProtocol.COM.COM_PICK:
                Pick_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);        
                break;
            case VsProtocol.COM.COM_HANDUP:
                HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_CALLTRANSFER: //呼叫转移指令
                /*if (vs_strstr(pack.SourceAddr, ConnectStat.Currentalk_Addr, 5))
                {
                    Call_Transfer_ASK_Deal(head->SourceAddr,buff, FromIP, DestPort);
                }*/
                break;
            case VsProtocol.COM.COM_TIMECOUNTREPLY:   //倒计时同步应答指令
                if(ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP,FromIP,4) > 0)
                {
			
                }
                break;       
            case VsProtocol.COM.COM_TIMECOUNTSYNC:  //倒计时同步请求指令
                break;     
            default:
                break;
            }
        }

        public void STA_WAIT_PICK_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_PICK_REPLY_Deal..." + pack.com);

            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Ask_Deal(pack.SourceAddr,FromIP,DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Reply_Deal(pack.SourceAddr, FromIP);
                }
                break;
            case VsProtocol.COM.COM_PICKREPLY: //摘机应答指令 
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
   		        {
                    Pick_Reply_Deal(pack.SourceAddr, buff, FromIP, DestPort);
   		        }
                break;
            case VsProtocol.COM.COM_HANDUP:   //挂机指令        
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0) //地址
                {
                    HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                }
                break;    
            default:
                break;
            }
        }

        public void STA_WAIT_MONITOR_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_MONITOR_REPLY_Deal..." + pack.com);

            switch (pack.com)
            {
                case VsProtocol.COM.COM_MONITORREPLY:  //监视应答指令
                  if(ArrayDeal.vs_strstr(FromIP,ConnectStat.Currentalk_IP,4) > 0)
                  {
          	        Monitor_Reply_Deal(pack.SourceAddr,buff,FromIP, DestPort);
                  }
                  break;  
                case VsProtocol.COM.COM_BUSY:
                  ConnectStat.Busy_Flag = 1;
                  break;   
            }
        }

        public void STA_MONITOR_STAT_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_MONITOR_STAT_Deal..." + pack.com);

            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                    if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {
                    Hand_Reply_Deal(pack.SourceAddr, FromIP);
                }
                break;
            case VsProtocol.COM.COM_PICKREPLY: //摘机应答指令
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0)
                {
                    Pick_Reply_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDUP:
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0)
                {
                    HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_CALLASK:  //呼叫指令
    	        handup(HANDUP_TYPE.TYPE_INTERRUPT, 1);
                Call_In_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_TIMECOUNTREPLY:   //倒计时同步应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4)>0)
                {
        	
                }
                break;          
            default:
                break;
            }
        }

        public void STA_MONITOR_CALL_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_MONITOR_CALL_Deal..." + pack.com);

            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                    if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) >0 || (ConnectStat.NewCallFlag > 0 && ArrayDeal.vs_strstr(ConnectStat.NewCall_IP, FromIP, 4)>0))
                {
                    Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4)>0 || (ConnectStat.NewCallFlag > 0 && ArrayDeal.vs_strstr(ConnectStat.NewCall_IP, FromIP, 4)>0))
                {
                    Hand_Reply_Deal(pack.SourceAddr, FromIP);
                }
                break;
            case VsProtocol.COM.COM_HANDUP:
                if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4)>0)
                {
                    HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                }
                break;
             case VsProtocol.COM.COM_CALLASK:  //呼叫指令
                //NewCall_In_Deal(head->SourceAddr,buff, FromIP, DestPort);
                break;    
            case VsProtocol.COM.COM_TIMECOUNTREPLY:   //倒计时同步应答指令
                if(ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP,FromIP,4) > 0)
                {
                }
                break;            
            default:
                break;
            }
        }

        public void STA_WAIT_HANDUP_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_HANDUP_REPLY_Deal..." + pack.com);
            switch (pack.com)
            {
            case VsProtocol.COM.COM_HANDUPREPLY:   //挂机应答指令
                 HandUp_Reply_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                break;
            case VsProtocol.COM.COM_HANDASK:
                if(ConnectStat.Currentalk_Type == ConnectStat.CurrentalkType.MANAGETOINDOOR)  //如果是分机呼叫管理机
                {
                    if (pack.SourceAddr[0] != (byte)VsProtocol.DevType.DEV_GUARDUNIT && pack.SourceAddr[0] != (byte)VsProtocol.DevType.DEV_GUARD)
                    {
                        return;
                    }
                }
                else
                {
                    if (ArrayDeal.vs_strstr(pack.SourceAddr, ConnectStat.Currentalk_Addr, 5) == 0) //地址
                    {
                        return;
                    }
                }
                HandUp_Ask_Deal(pack.SourceAddr,buff,FromIP, DestPort);
                break;
            default:
                break;
            }
        }

        public void STA_CALLING_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
           // Console.WriteLine("STA_CALLING_Deal..." + pack.com);
            switch(pack.com)
            {
                case VsProtocol.COM.COM_HANDASK:
                    if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4)>0 || (ConnectStat.NewCallFlag>0 && ArrayDeal.vs_strstr(ConnectStat.NewCall_IP, FromIP, 4)>0))
                    {
                        //Console.WriteLine("COM_HANDASK...");
                        Hand_Ask_Deal(pack.SourceAddr, FromIP, DestPort);
                    }
                    break;
                case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                    if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4)>0 || (ConnectStat.NewCallFlag>0 && ArrayDeal.vs_strstr(ConnectStat.NewCall_IP, FromIP, 4)>0))
                    {
                        Hand_Reply_Deal(pack.SourceAddr, FromIP);
                    }
                    break;
                case VsProtocol.COM.COM_HANDUP:   //挂机指令
                    {
                        if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0)
                        {
                            HandUp_Ask_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                        }
                        else if (ConnectStat.NewCallFlag > 0 && ArrayDeal.vs_strstr(ConnectStat.NewCall_IP, FromIP, 4) > 0)  //新呼叫挂机
                        {
                            //NewCall_HandUp_Deal(pack.SourceAddr, buff, FromIP, DestPort);
                        }
                    }    
                    break;
                 case VsProtocol.COM.COM_INTERCEPT_CALL_ASK:   //呼叫拦截指令
                 case VsProtocol.COM.COM_CALLASK:  //呼叫指令
                    {
                        //NewCall_In_Deal(head->SourceAddr,buff, FromIP,DestPort);
                    }
                    break;
              case VsProtocol.COM.COM_PICK:
                    {
                        if (ArrayDeal.vs_strstr(FromIP, ConnectStat.Currentalk_IP, 4) > 0)
                        {
                             Send_PickReply(pack.SourceAddr, FromIP, DestPort); //发送摘机应答指令
                        }
                    }
                    break;
              case VsProtocol.COM.COM_TIMECOUNTREPLY:   //倒计时同步应答指令
                    if (ArrayDeal.vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4) > 0)
                {

                }
                break;          
              default:
                break;
            }
        }

        public void STA_INDOOR_PAGING_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_INDOOR_PAGING_Deal..." + pack.com);
            /*switch(common)
            {
                case VsProtocol.COM.COM_HANDASK:  //握手指令
                    if(vs_strstr(ConnectStat.Currentalk_Addr, head->SourceAddr, 5))
                    {
                        Hand_Ask_Deal(head->SourceAddr,FromIP,DestPort);
                    }
                    break;
                case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                    if(vs_strstr(ConnectStat.Currentalk_Addr, head->SourceAddr, 5))
                    {
                        Hand_Reply_Deal(head->SourceAddr,FromIP);
                    }
                    break;
                case VsProtocol.COM.COM_HANDUP://挂机指令
        	        if(vs_strstr(head->SourceAddr,LocalCfg.Addr,5))
        	        {
        		        HandUp_Ask_Deal(head->SourceAddr,buff,FromIP, DestPort);
        	        }
                    break;
                case VsProtocol.COM.COM_RADIOASK:  //户内广播   
                    if(vs_strstr(FromIP,ConnectStat.Currentalk_IP,4))
        	        {
        		        Send_UDPCom(VsProtocol.COM.COM_RADIOREPLY,head->SourceAddr,FromIP, DestPort);//发送户内广播应答指令
        	        }
        	        break;
                case VsProtocol.COM.COM_CALLASK:  //呼叫指令

                    break;
                default:
                    break;
            }*/
        }

        public void STA_WAIT_RADIO_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_RADIO_REPLY_Deal..." + pack.com);
            /*switch(common)
            {
                 case VsProtocol.COM.COM_HANDASK:  //握手指令
                    if(vs_strstr(ConnectStat.Currentalk_Addr, head->SourceAddr, 5))
                    {
                        Hand_Ask_Deal(head->SourceAddr,FromIP,DestPort);
                    }
                    break;
                case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                    if(vs_strstr(ConnectStat.Currentalk_Addr, head->SourceAddr, 5))
                    {
                        Hand_Reply_Deal(head->SourceAddr,FromIP);
                    }
                    break;
                case VsProtocol.COM.COM_RADIOREPLY:  //户内广播应答指令
                 {
                    if(vs_strstr(head->SourceAddr, LocalCfg.Addr, 5))
                    {
                       Radio_Reply_Deal(head->SourceAddr,FromIP, DestPort);
                    }
                 }
                 break;
                default:
                    break;
            }*/
        }

        public void STA_WAIT_TRANS_REPLY_Deal(byte[] buff, byte[] FromIP, int DestPort, int recv_len)
        {
            VsProtocol.Pack pack = new VsProtocol.Pack(buff);
            Console.WriteLine("STA_WAIT_TRANS_REPLY_Deal..." + pack.com);
            /*
            switch(common)
            {
            case VsProtocol.COM.COM_HANDASK:  //握手指令
                if(vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4))
                {
                    Hand_Ask_Deal(head->SourceAddr,FromIP,DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDREPLY: //握手应答指令
                if(vs_strstr(ConnectStat.Currentalk_IP, FromIP, 4))
                {
                    Hand_Reply_Deal(head->SourceAddr,FromIP);
                }
                break;
            case VsProtocol.COM.COM_CALLTRANSFERREPLY:  //呼叫转移应答指令
                if(vs_strstr(ConnectStat.Currentalk_IP,FromIP,4))
                {
        	        Call_Transfer_Reply_Deal(head->SourceAddr,buff,FromIP, DestPort);
                }
                break;
            case VsProtocol.COM.COM_HANDUP:
    	        if(vs_strstr(ConnectStat.Currentalk_IP,FromIP,4))
                {
        	        HandUp_Ask_Deal(head->SourceAddr,buff,FromIP, DestPort);
                }
            default:
                break;
            }*/
        }

        void talking()
        {
	        talkback_manage.talk_count = 0;
	        AppTimer.start_timer(talkback_manage.countdown_timer);
	        if(talkback_manage.toggle_audio_recv != null)
	        {
		        talkback_manage.toggle_audio_recv();
	        }
	        if(talkback_manage.toggle_audio_send != null)
	        {
		        talkback_manage.toggle_audio_send();
	        }
        }
        void stop_talking()
        {
	        Console.WriteLine("stop_talking...\r\n");
            AppTimer.stop_timer(talkback_manage.countdown_timer);
	        if(talkback_manage.stop_audio_recv != null)
	        {
		        talkback_manage.stop_audio_recv();
	        }	
	        if(talkback_manage.stop_audio_send != null)
	        {
		        talkback_manage.stop_audio_send();
	        }	
	        if(talkback_manage.stop_video_recv != null)
	        {
		        talkback_manage.stop_video_recv();
	        }
        }
        void stop_monotor()	  //结束监视
        {
	        AppTimer.stop_timer(talkback_manage.countdown_timer);
	        if(talkback_manage.stop_audio_recv != null)
	        {
		        talkback_manage.stop_audio_recv();
	        }
            if (talkback_manage.stop_video_recv != null)
	        {
		        talkback_manage.stop_video_recv();
	        }
        }

        void monitor_calling()	  //监视主动对讲中
        {
	        talkback_manage.monitor_count = 0;
	        if(talkback_manage.toggle_audio_send != null)
	        {
		        talkback_manage.toggle_audio_send();
	        }
        }

        void stop_monotor_call()	  //结束监视主动对讲
        {
	        //printf("stop_monotor_call...\r\n");
	        AppTimer.stop_timer(talkback_manage.countdown_timer);
	        if(talkback_manage.stop_audio_recv != null)
	        {
		        talkback_manage.stop_audio_recv();
	        }
	        if(talkback_manage.stop_video_recv != null)
	        {
		        talkback_manage.stop_video_recv();
	        }
	        if(talkback_manage.stop_audio_send != null)
	        {
		        talkback_manage.stop_audio_send();
	        }	
        }

        class TalkbackManage
        {
            public STA state;							//状态
            public Object state_lock = new Object();			//状态锁
            public AppTimer.AppTimerMember countdown_timer; 	   	//通话倒计时定时器
            public AppTimer.AppTimerMember hand_sendtimer;   		//握手发送定时器
            public AppTimer.AppTimerMember hand_replytimer;  		//握手应答定时器
            public int hand_count;	      	   		//握手超时计数
            public int ring_flag;					//响铃标志
            public int pick_flag;					//摘机标志
            public int unlock_flag;					//开锁标志
            public int ring_count;	           		//响铃计时
            public int talk_count;			   		//通话计时
            public int monitor_count;			   	//监视计时
            public int radio_count;			   		//户内广播计时
            public int new_call_ring_count;	    //新呼叫响铃计时

            public byte[] doorsation_list = new byte[DOORSTATION_MAX];			//本单元门口机列表
            public byte[] perimetergate_list = new byte[PERIMETERGATE_MAX];	//围墙机列表

            public startRing start_ring;	   		//响铃
            public stopRing stop_ring;      		//停止响铃
            public toggleVideoSend toggle_video_send;  //开始发送视频
            public stopVideoSend stop_video_send;		//停止视频发送
            public toggleVideoRecv toggle_video_recv;  //开始发送视频
            public stopVideoRecv stop_video_recv;		//停止视频发送
            public toggleAudioSend toggle_audio_send;  //开始发送音频
            public stopAudioSend stop_audio_send;		//停止音频发送
            public toggleAudioRecv toggle_audio_recv;  //开始发送音频
            public stopAudioRecv stop_audio_recv;		//停止音频发送
            public updateRecentCalls update_recent_calls;	//更新最近通话
	
	        //callback function
            public delegate void startRing();	   		//响铃
            public delegate void stopRing();      		//停止响铃
            public delegate void toggleVideoSend();  //开始发送视频
            public delegate void stopVideoSend();		//停止视频发送
            public delegate void toggleVideoRecv();  //开始发送视频
            public delegate void stopVideoRecv();		//停止视频发送
            public delegate void toggleAudioSend();  //开始发送音频
            public delegate void stopAudioSend();		//停止音频发送
            public delegate void toggleAudioRecv();  //开始发送音频
            public delegate void stopAudioRecv();		//停止音频发送
            public delegate void updateRecentCalls(byte[] addr);	//更新最近通话

            public void Clear()
            {
                ring_flag = 0;
                ring_count = 0;
                pick_flag = 0;
                talk_count = 0;
                monitor_count = 0;
                radio_count = 0;
                unlock_flag = 0;
            }
        }

        public enum STA
        {
            STA_NORMAL_STANDBY       	,    	//正常待机状态
            STA_WAIT_CALL_REPLY      	,   	//等待呼叫应答状态
            STA_WAIT_PICK             	,   	//等待主动摘机
            STA_WAIT_PICK_ASK         ,    	//等待对方摘机
            STA_WAIT_PICK_REPLY      	,   	//等待摘机应答指令
            STA_WAIT_MONITOR_REPLY   	,   	//等待监视应答状态
            STA_MONITOR_STAT         	,  		//监控状态
            STA_MONITOR_CALL         	,		//监控对讲状态
            STA_WAIT_HANDUP_REPLY    	,   	//等待挂机应答指令
            STA_CALLING             	,  		//正在通话中
            STA_INDOOR_PAGING			,  		//正在进行户内广播
            STA_WAIT_RADIO_REPLY		,		//等待户内广播应答
            STA_WAIT_TRANS_REPLY		,		//等待呼叫转移应答
            STA_ONVIF_MONITOR			,		//onvif监控状态
    }
        public enum HANDUP_TYPE     //挂断类型
        {
	        TYPE_NORMAL,					//正常挂机
	        TYPE_BUSY,						//对方正忙
	        TYPE_NO_ANSWER,				//无应答
	        TYPE_TIMEOUT,  				//超时挂机
	        TYPE_DISCONNECTED,			//握手超时,连接断开
	        TYPE_INTERRUPT,				//打断
	        TYPE_TRANSFER,					//呼叫转移
	        TYPE_MON_POLL,					//监视轮询
	        TYPE_ERRO,						//出错
        }
    }
}
