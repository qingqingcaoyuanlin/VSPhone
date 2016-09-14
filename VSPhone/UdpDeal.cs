using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
//using System.Timers;

namespace VSPhone
{
    public class UdpDeal
    {
        const int  UDP_BUFF_SIZE			=		1500;	//UDP包大小
        const int UDP_SEND_BUFF_COUNT		=	100;		//UDP发送缓冲区大小
        const int UDP_TIMER_TICK			=		100;    //单位ms

        public delegate void Func(int arg);
        MULTI_UDP_MODULE multi_udp = new MULTI_UDP_MODULE();
        public class  MULTI_UDP_MODULE
        {
	        public int port;
	        public Timer vTimer;
            public UdpClient udp_trans;
	        public object send_sem = new object();			
	        public object udplock = new object();			
	        public Thread send_task;				//发送线程
	        public Thread recv_task;				//接收线程
	        public MULTI_UDP_BUFF[] send_buff = null; 	//UDP发送缓冲
            public RecvDealFun recv_deal_fun;
	        public delegate void RecvDealFun(byte[] from_ip,int source_port,byte[] buff,int len);    //接收处理回调函数
        }
        public class MULTI_UDP_BUFF
        {
            public int  isValid; 					//是否有效
            public int  timeout;					//超时标志
            public int SendNum; 					//当前发送次数
            public int MaxSendNum;  			//发送次数
            public int TimeMap;					//发送间隔 ms	
            public int DelayTime;  				//最开始存储首次发送延时，第一次发送之后更新为TimeMap的值 ms
            public int Time_Count;				//延时计数 ms	
            public int DestPort;				//端口号
            public byte[] DestIP;					//目标IP地址
            public byte[] Buff = new byte[UDP_BUFF_SIZE];		//缓冲区  
            public int nlength;					//数据长度
            public string  Magic;	//魔数  可以用来标识某个或某种类型的包,方便对包进行删除、编辑，相比记录包的ID的方式，这种方式更加灵活
	        public Func Func;			//每次发送udp后都会调用,另外udp包发送完成后还会执行一次，可用于处理包应答超时的任务。注意不要执行耗时长的任务。
        }

        public int app_udp_init(int port)
        {
	        multi_udp.port = port;
	        multi_udp.recv_deal_fun = null;
	        //初始化发送缓冲区
	        multi_udp.send_buff = new MULTI_UDP_BUFF[UDP_SEND_BUFF_COUNT];

            for (int i = 0; i < UDP_SEND_BUFF_COUNT; i++)
            {
                multi_udp.send_buff[i] = new MULTI_UDP_BUFF();
            }
            VsProtocol.Get_MulticastIP(VsProtocol.MulticastIPType.INDOOR_MULICASTIP, LocalCfg.Addr, 0, LocalCfg.IP_Mulicast);
	        //创建UDP，绑定指定端口
            try
            {
                IPEndPoint ipEp = new IPEndPoint(new IPAddress(LocalCfg.IP),multi_udp.port);
                multi_udp.udp_trans = new UdpClient(ipEp);
                multi_udp.udp_trans.JoinMulticastGroup(new IPAddress(LocalCfg.IP_Mulicast), new IPAddress(LocalCfg.IP));
            }
	        catch(Exception e)
            {
                Console.WriteLine("new UdpClient fail :"+e.ToString());
            }
	
	        //创建发送和接收线程
            multi_udp.recv_task = new Thread(thread_multi_udp_recv);
            multi_udp.recv_task.Start();
            multi_udp.send_task = new Thread(thread_multi_udp_send);
            multi_udp.send_task.Start();
	        //初始化定时器
            multi_udp.vTimer = new Timer(udp_pack_time_update); 
            multi_udp.vTimer.Change(0,10);
	        
	        return 0;
        }
        public int creat_multi_udp_pack2(byte[]ip, int dest_port, byte[]buff, int len,
										int sendtime, int inittime, int delaytime,
										string magic, Func Func)
        {
	        int i;

            lock(multi_udp.udplock)
            {
	            if((i = get_free_buff()) < 0)
	            {
		            Console.WriteLine("multi udp buff is full!");
		            //release_nest_lock(multi_udp.lock );
		            return i;
	            }
	            if(magic != null)
	            {
                    multi_udp.send_buff[i].Magic = magic;
	            }

	            multi_udp.send_buff[i].DestIP = ip;
	            multi_udp.send_buff[i].DestPort = dest_port;
	            multi_udp.send_buff[i].MaxSendNum = sendtime;
	            multi_udp.send_buff[i].SendNum = 0;
	            multi_udp.send_buff[i].DelayTime = inittime;
	            multi_udp.send_buff[i].TimeMap = delaytime;
	            multi_udp.send_buff[i].Time_Count = 0;
	            multi_udp.send_buff[i].timeout = 0;
	            ArrayDeal.memcpy(multi_udp.send_buff[i].Buff, buff, len);
	            multi_udp.send_buff[i].nlength = len;
	            multi_udp.send_buff[i].Func = Func;
	            multi_udp.send_buff[i].isValid = 1;
	        }
	        return i;
        }
        void thread_multi_udp_send()
        {
	        int i;
	        Console.WriteLine("thread_multi_udp_send...");
	        while(true)
	        {
		        //AK_Obtain_Semaphore(multi_udp.send_sem, AK_SUSPEND);
		        //AK_Reset_Semaphore(multi_udp.send_sem, 0);
                lock (multi_udp.send_sem)
                {
                    Monitor.Wait(multi_udp.send_sem);
                }
                
		        //printf("thread_multi_udp_send: run...\n");
		        for(i=0; i<UDP_SEND_BUFF_COUNT; i++)
		        {
			        lock(multi_udp.udplock)	//加锁
                    {
			            if(multi_udp.send_buff[i].isValid > 0 && multi_udp.send_buff[i].timeout > 0)
			            {
				            multi_udp.send_buff[i].SendNum ++;
				            multi_udp.send_buff[i].Time_Count = 0;
				            multi_udp.send_buff[i].timeout = 0;
				            if(multi_udp.send_buff[i].SendNum > multi_udp.send_buff[i].MaxSendNum)
				            {
					            multi_udp.send_buff[i].isValid = 0;
				            }
				            else
				            {
                                multi_udp.udp_trans.Send(multi_udp.send_buff[i].Buff,multi_udp.send_buff[i].nlength,new IPEndPoint(new IPAddress(multi_udp.send_buff[i].DestIP),multi_udp.send_buff[i].DestPort));
					            //Fwl_Net_Conn_Sendto(multi_udp.udp_trans->pNetConn,multi_udp.send_buff[i].Buff, multi_udp.send_buff[i].nlength,multi_udp.send_buff[i].DestIP,multi_udp.send_buff[i].DestPort,NETCONN_COPY);
					            if(multi_udp.send_buff[i].SendNum == 1)
					            {
						            multi_udp.send_buff[i].DelayTime = multi_udp.send_buff[i].TimeMap;
					            }
				            }		
				            if(multi_udp.send_buff[i].Func != null)
				            {
					            multi_udp.send_buff[i].Func(multi_udp.send_buff[i].SendNum);
				            }
			            }
			            //release_nest_lock(multi_udp.lock );	//解锁
                    }
		        }
	        }	
        }
        void thread_multi_udp_recv()
        {
	        int recv_len;
	        byte[] recv_buff;
            IPEndPoint ipEd = new IPEndPoint(new IPAddress(new byte[4]),0);

	        Console.WriteLine("thread_multi_udp_recv...\n");
	        while(true)
	        {
		        //Console.WriteLine("thread_multi_udp_recv...\n");
                recv_buff = multi_udp.udp_trans.Receive(ref ipEd);
                recv_len = recv_buff.Length;
		        //Console.WriteLine("recv ip:%s,port =%d,len=%d\r\n",ip_str,multi_udp.udp_trans->pNetConn->info.RemotePort,recv_len);
		        if(multi_udp.recv_deal_fun != null)
		        {
			        multi_udp.recv_deal_fun(ipEd.Address.GetAddressBytes(),ipEd.Port,recv_buff, recv_len);
		        }
	        }

        }

        void udp_pack_time_update(object sta)		//更新UDP包的延时时间计数
        {
	        int i;
	        bool flag = false;
	        lock(multi_udp.udplock)			//加锁
            {
	            for(i=0; i<UDP_SEND_BUFF_COUNT; i++)
	            {
		            if(multi_udp.send_buff[i].isValid > 0 && multi_udp.send_buff[i].timeout == 0)
		            {
			            multi_udp.send_buff[i].Time_Count += UDP_TIMER_TICK;
			            if(multi_udp.send_buff[i].Time_Count > multi_udp.send_buff[i].DelayTime)
			            {
				            multi_udp.send_buff[i].timeout = 1;
				            flag = true;
			            }
		            }
	            }
	            
	        //release_nest_lock(multi_udp.lock );	
            }
            if (flag == true) 	//唤醒发送线程
            {
                //printf("Wake up send thread...\r\n");
                //AK_Release_Semaphore(multi_udp.send_sem);
                lock(multi_udp.send_sem)
                {
                    Monitor.Pulse(multi_udp.send_sem);
                }
            }
        }
        public void set_multi_udp_recv_fun(MULTI_UDP_MODULE.RecvDealFun fun)
        {
	        multi_udp.recv_deal_fun = fun;
        }
        int get_free_buff()
        {
	        int i;
	        lock(multi_udp.udplock)//加锁
            { 
	            for(i=0; i<UDP_SEND_BUFF_COUNT; i++)
	            {
		            if(multi_udp.send_buff[i].isValid == 0)
		            {
			            return i;
		            }
	            }
            }
	        return -1;
        }

        string ip_to_string(byte[] ip,string str)
        {
            int i;
            str = "";
            for(i=0;i<4;i++)
            {
               str += ip[i].ToString();
            }
            return str;
        }



    }
}
