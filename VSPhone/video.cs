using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;

namespace VSPhone
{
    public class Video
    {
        public Video(int port)
        {
	        video_manage_init(port);
	        //video_device_init();
	        video_udp_init();
	        video_thread_init();
            jpeg_head_init();
        }
        public VIDEO_MANAGE video_manage = new VIDEO_MANAGE();
        byte[] jpegHead;
        byte[][] jpeg_head_table = new byte[VIDEO_MANAGE.JPEG_HEAD_COUNT][];
        int video_manage_init(int port)
        {
	        int i;
            video_manage.recv_buff_addr = new byte[VIDEO_MANAGE.VIDEO_RECV_BUFF_COUNT][];
            video_manage.recv_buff = new VIDEO_RECV_BUFF[VIDEO_MANAGE.VIDEO_RECV_BUFF_COUNT];
	        for(i=0; i<VIDEO_MANAGE.VIDEO_RECV_BUFF_COUNT; i++)
	        {
		        video_manage.recv_buff_addr[i] = new byte[VIDEO_MANAGE.VIDEO_FRAME_SIZE + VIDEO_MANAGE.JPEG_HEAD_LEN];	//申请接收缓冲区空间
		        video_manage.recv_buff[i] = new VIDEO_RECV_BUFF();
                video_manage.recv_buff[i].buff = video_manage.recv_buff_addr[i];	//预留兼容801平台填充的jpeg头部空间
		        video_manage.recv_buff[i].idel_flag = 1;
		        video_manage.recv_buff[i].ready_flag = 0;
		        video_manage.recv_buff[i].len = 0;
	        }

	        //video_manage.pack_buff = Fwl_Malloc(VIDEO__PACK_BUFF_SIZE);
	        //video_manage.send_buff = Fwl_Malloc(VIDEO_FRAME_SIZE*2);	   //申请发送缓冲区空间
	        //video_manage.video_quality_adjust = AK_NULL;

	        video_manage.udp_port = port;
	        video_manage.width = 640;
	        video_manage.height = 480;
	        video_manage.quality = 100;
	        return 0;
        }

        int video_udp_init()
        {
            try
            {
                video_manage.udp_trans = new UdpClient(new IPEndPoint(new IPAddress(LocalCfg.IP),video_manage.udp_port));
            }
	        catch
            {
                Console.WriteLine("video_udp_init error");
                return -1;
            }

	        return 0;
        }

        int video_thread_init()     //初始化信号量,创建发送和接收线程
        {

            video_manage.recv_thread = new Thread(thread_video_recv);
            video_manage.recv_thread.Start();

/*
        #if VIDEO_SEND_SUPPORT
	        video_manage.send_sem = AK_Create_Semaphore(0,AK_PRIORITY);
	        pStackAddr = Fwl_Malloc(VIDEO_TASK_STACK_SIZE);
	        memset(pStackAddr, 0, VIDEO_TASK_STACK_SIZE);
	        video_manage.send_thread = AK_Create_Task((T_VOID*)thread_video_send, 
										        "VIDEO_SEND", 
										        1, 
										        AK_NULL, 
										        pStackAddr, 
										        VIDEO_TASK_STACK_SIZE, 
										        TASK_PRIO_VIDEO_SEND, 
										        VIDEO_TASK_TIME_SLICE, 
										        AK_PREEMPT, 
										        AK_START);
	        if (video_manage.send_thread  < 0)
	        {
		        printf("video_send_task create failed %ld!\r\n", video_manage.send_thread );
		        video_manage.send_thread = AK_INVALID_TASK;
		        pStackAddr = Fwl_Free(pStackAddr);
		        return -1;
	        }
	        else
	        {
		        printf("video_send_task create ok, task id=%ld\r\n",video_manage.send_thread );
	        }	
        #endif	
 */
	        return 0;
        }
        void thread_video_recv()
        {
	        //int recv_len;
            int PackOK;
            //int frames;
            int next_buff_no;
            //int video_len;
            int j;

            VideoDataHead head;
	        Console.WriteLine("thread_video_recv...\r\n");
	        byte[] recv_buff;
            IPEndPoint recvEP = new IPEndPoint(IPAddress.Broadcast,video_manage.udp_port);
	        while(true)
	        {
                lock(video_manage.recv_sem)
                {
                    Monitor.Wait(video_manage.recv_sem);
                }
		        Console.WriteLine("thread_video_recv run...\r\n");

		        while(true)
		        {
			        //video_manage.recv_flag = true;
			        if(video_manage.recv_flag == false)
			        {
                        Console.WriteLine("thread_video_recv exit...");
				        break;
			        }
			        //recv_len = 1500;
                    recv_buff = video_manage.udp_trans.Receive(ref recvEP);
                    head = new VideoDataHead(recv_buff);
			        if(video_manage.video_recv_filter != null)
			        {
                        if (video_manage.video_recv_filter(recvEP.Address.GetAddressBytes()) == false)
				        {
                            Console.WriteLine("thread_video_recv video_recv_filter...");
                            continue;
				        }
			        }
			
			        /*
			        //printf("Video_Frameno=%d\n",head->Frameno);
                    Console.WriteLine("Video_CurrPackage="+ head.CurrPackage);
                    Console.WriteLine("Video_Framelen="+ head.Framelen);
                    Console.WriteLine("Video_TotalPackage="+ head.TotalPackage);
                    Console.WriteLine("Video_PackLen="+ head.PackLen);
                    Console.WriteLine("Video_Datalen="+head.Datalen);
			        */
			        
			        if(video_manage.recv_frameno != head.Frameno)		//新的一帧
			        {
				        video_manage.recv_frameno = head.Frameno;
				        video_manage.recv_total_pack = head.TotalPackage;
				        video_manage.recv_pack_count = 0;
				        video_manage.recv_total_frame++;
				        video_manage.recv_buff[video_manage.recv_buff_no].buff = video_manage.recv_buff_addr[video_manage.recv_buff_no];
				        video_manage.recv_buff[video_manage.recv_buff_no].len = 0;
				        video_manage.recv_buff[video_manage.recv_buff_no].ready_flag = 0;
			        }
			        Array.Copy(recv_buff,head.GetLength(),video_manage.recv_buff[video_manage.recv_buff_no].buff,(head.CurrPackage-1)*head.PackLen,head.Datalen);
	                video_manage.recv_pack_count++;
	                video_manage.recv_buff[video_manage.recv_buff_no].len += head.Datalen;
	                video_manage.pack_exit[head.CurrPackage-1] = 1;
	                if(video_manage.recv_pack_count == head.TotalPackage)  //接收满一帧
        	        {
        		        PackOK = 1;
	                    for(j = 0; j < head.TotalPackage; j++) //判断有无丢包
	                    {
	                        if(video_manage.pack_exit[j] == 0)   
	                        {
	                            PackOK = 0;
	                            Console.WriteLine("framo = "+head.Frameno +"pack = "+ j +" 丢包...");
	                        }
	                    }
	                    if(PackOK == 0)    
	                    {
	            	        video_manage.recv_lost_frame++;
	                        continue;
	                    }
	                    if(video_manage.recv_buff[video_manage.recv_buff_no].len != head.Framelen) //判断帧大小是否一致
	                    {
	            	        //Console.WriteLine("MyVideo_length=%x, Video_Framelen=%x\n",video_manage.recv_buff[video_manage.recv_buff_no].len,head->Framelen);
					        video_manage.recv_lost_frame++;
                	        continue;
	                    }
	            
	                    //frames++;         //统计视频接收帧率
				        /*if(frames>50)
				        {
					        a = get_tick_count();
					        video_manage.recv_fps = frames*1000/(a-b);
					        b = a;
					        frames=0;
				        }*/
				
				        //兼容801平台，填充jpeg头部
                        
				        if(check_jpeg_head(video_manage.recv_buff[video_manage.recv_buff_no].buff) == false)	
        		        {
        			        //printf("fill...%d\r\n",head->DQT_Value);
        			        int video_len = fill_jpeg_head(video_manage.recv_buff_addr[video_manage.recv_buff_no],
        					        video_manage.recv_buff[video_manage.recv_buff_no].len,head.DQT_Value);
        			        if(video_len == -1)
        			        {
        				        Console.WriteLine("fill_jpeg_head erro");
        				        continue;
        			        }
        			        video_manage.recv_buff[video_manage.recv_buff_no].len = video_len;
        			        video_manage.recv_buff[video_manage.recv_buff_no].buff = video_manage.recv_buff_addr[video_manage.recv_buff_no];
        		        }
                        
                        /*
				        if(video_manage.shot_info.start_shot_dest)
				        {
					        save_shot_pic(video_manage.recv_buff[video_manage.recv_buff_no].buff, video_manage.recv_buff[video_manage.recv_buff_no].len);
					        video_manage.shot_info.start_shot_dest = 0;
				        }*/
				
				        //printf("get frame:%ld\r\n",head->Frameno);
	                    //切换接收缓冲区
	                    next_buff_no = video_manage.recv_buff_no + 1;
	                    if(next_buff_no >= VIDEO_MANAGE.VIDEO_RECV_BUFF_COUNT)  
	                    {
	            	        next_buff_no = 0;
	                    }
	                    if(video_manage.recv_buff[next_buff_no].idel_flag > 0)
	                    {
	            	        if(video_manage.video_recv_callback != null)
	            	        {
	            		        video_manage.recv_buff[video_manage.recv_buff_no].ready_flag = 1;
	            		        video_manage.recv_buff[video_manage.recv_buff_no].idel_flag = 0;
	            		        video_manage.video_recv_callback(video_manage.recv_buff[video_manage.recv_buff_no]);
	            	        }	
	            	        video_manage.recv_buff_no = next_buff_no;
	                    }
	                    else
	                    {
	            	        //printf("!\r\n");
	            	        video_manage.recv_buff[video_manage.recv_buff_no].buff = video_manage.recv_buff_addr[video_manage.recv_buff_no];
	            	        video_manage.recv_buff[video_manage.recv_buff_no].ready_flag = 0;
	            	        video_manage.recv_buff[video_manage.recv_buff_no].len = 0;
	                    }
        	        }
		        }
	        }
        }
        void reset_video_recv_buff()		//重置视频接收缓冲区状态
        {
	        int i;
            for (i = 0; i < VIDEO_MANAGE.VIDEO_RECV_BUFF_COUNT; i++)
	        {
		        //video_manage.recv_buff[i].buff = video_manage.recv_buff_addr[i] + JPEG_HEAD_LEN;
		        video_manage.recv_buff[i].idel_flag = 1;
		        video_manage.recv_buff[i].ready_flag = 0;
		        video_manage.recv_buff[i].len = 0;
	        }
        }
        public void start_video_recv()
        {
	        Console.WriteLine("toggle_video_recv...\r\n");
	        video_manage.recv_flag = true;
	        reset_video_recv_buff();
	        //enable_video_info_printf(5000);
	        //AK_Release_Semaphore(video_manage.recv_sem);   //唤醒接收线程
            lock (video_manage.recv_sem)
            {
                Monitor.Pulse(video_manage.recv_sem);
            }
            
        }

        public void stop_video_recv()
        {
            Console.WriteLine("stop_video_recv...\r\n");
	        video_manage.recv_flag = false;
	        //disabel_video_info_printf();
        }

        int jpeg_head_init()			   //jpeg头部初始化
        {

	        //byte[] head = new byte[VIDEO_MANAGE.JPEG_HEAD_LEN];
            //ResourceManager resMan = new ResourceManager("Resources1",System.Reflection.Assembly.GetExecutingAssembly());
            //System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
	        string[] dqt_table = new string[]
	        {
		        "HEADER_DQT25.bin",
		        "HEADER_DQT30.bin",
		        "HEADER_DQT35.bin",
		        "HEADER_DQT40.bin",
		        "HEADER_DQT45.bin",
		        "HEADER_DQT50.bin",
		        "HEADER_DQT60.bin",
		        "HEADER_DQT70.bin",
		        "HEADER_DQT80.bin"
	        };
	        //读取jpeg头部
            jpegHead = Resource1.VIDEO_HEADER;
            //Stream ms = resMan.GetStream("VIDEO_HEADER.BIN");
            //jpegHead = new byte[ms.Length];
            //ms.Read(jpegHead,0,(int)ms.Length);
            //ms.Close();
            jpeg_head_table[0] = Resource1.HEADER_DQT25;
            jpeg_head_table[1] = Resource1.HEADER_DQT30;
            jpeg_head_table[2] = Resource1.HEADER_DQT35;
            jpeg_head_table[3] = Resource1.HEADER_DQT40;
            jpeg_head_table[4] = Resource1.HEADER_DQT45;
            jpeg_head_table[5] = Resource1.HEADER_DQT50;
            jpeg_head_table[6] = Resource1.HEADER_DQT60;
            jpeg_head_table[7] = Resource1.HEADER_DQT70;
            jpeg_head_table[8] = Resource1.HEADER_DQT80;
            /*
            for (int i = 0; i < VIDEO_MANAGE.JPEG_HEAD_COUNT; i++)
	        {
                //ms = resMan.GetStream(dqt_table[i]);
                jpeg_head_table[i] = new byte[ms.Length];
                //ms.Read(jpeg_head_table[i], 0, (int)ms.Length);
                //ms.Close();
	        }*/
	        return 0;
        }

        bool check_jpeg_head(byte[] buff)	//检测是否包含jpeg的头，为了兼容801平台的小门口机和管理机(发过来的视频不包含DQT表)
        {
            if (buff[0] == 0xff && buff[1] == 0xd8)
            {
                return true;
            }
            return false;
        }

        int fill_jpeg_head(byte[] buff, int len, int type)	//填充jpeg头部,返回填充完的图片大小
        {
            if (type >= VIDEO_MANAGE.JPEG_HEAD_COUNT)
            {
                return -1;
            }
            Array.Copy(buff, 0, buff,VIDEO_MANAGE.JPEG_HEAD_LEN,len);
            Array.Copy(jpegHead, 0, buff, 0, jpegHead.Length);
            Array.Copy(jpeg_head_table[type], 0, buff, VIDEO_MANAGE.JPEG_DQT_OFFSET, jpeg_head_table[type].Length);
            len += VIDEO_MANAGE.JPEG_HEAD_LEN;
            buff[len++] = 0xff;
            buff[len++] = 0xd9;
            return len;
        }
        public class VIDEO_MANAGE
        {
            public const int IDEOPACK_DATALEN 	=	1200;   	//视频按此大小分包
            public const int VIDEO_RECV_BUFF_COUNT =	2;			//接收缓冲区块数
            public const int VIDEO_PACK_MAX			=	100;			//每帧视频最大分包数
            public const int VIDEO_FRAME_SIZE		=    64*1024;   //每帧视频最大的大小
            public const int JPEG_HEAD_COUNT	=		9;			//jpeg头部数量
            public const int JPEG_HEAD_LEN		=		0x26f;		//jpeg头部长度
            public const int JPEG_DQT_OFFSET	=		0x14;		//DQT表在头部的偏移
            public const int JPEG_DQT_LEN		=		0x8a;		//DQT表长度


	        public byte[] public_head;
	        public byte[] source_addr;
	        //CAMERA_P camera;	
	        public int width;
	        public int height;

	        public byte quality;			     //视频编码质量
	        public byte VideoType;			 //视频类型  VGA QVGA
	        public int udp_port;
	        public UdpClient udp_trans;

	        public bool  send_flag;			//发送标志
	        public bool  send_buff_ready;   //发送缓冲区准备好的标志
	        public Thread send_thread;		//发送线程
	        public object send_sem = new object();
	        public int camera_fps;			//摄像头采集帧率
	        public int send_fps;			//发送帧率
	        public int send_total_frame;	//发送总帧数
	        public byte[] pack_buff;			//打包缓冲区
	        public byte[] send_buff;			//视频缓冲区
	        public int send_len;			//视频大小
	        public int send_frameno;		//发送帧号
	        public int total_pack;			//总包数
	        public int current_pack;		//当前包号
	        public delegate int VideoQualityAdjust(int current_quality,int len);    //视频质量动态调整回调函数,默认为空，不进行调整,返回值为质量，范围为
            public VideoQualityAdjust video_quality_adjust;

	        public bool  recv_flag;			//接收标志
	        public Thread recv_thread;		//接收线程
	        public object recv_sem = new object();
	        public int recv_fps;			//接收帧率
	        public int recv_total_frame;	//接收总帧数
	        public int recv_lost_frame;		//丢帧
	        public int recv_frameno;		//本帧帧号
	        public int recv_total_pack;		//本帧包的总数
	        public int recv_pack_count;		//本帧接收包的计数
	        public int recv_frame_len;		//本帧大小
	        public int  recv_buff_no;		//正在使用的接收缓冲区编号
	        public VIDEO_RECV_BUFF[] recv_buff;	//视频接收缓冲区
	        public byte[][] recv_buff_addr;		//存储视频接收缓冲区的地址，包含兼容801平台填充的jpeg头部
            public byte[] pack_exit = new byte[VIDEO_PACK_MAX];	
	        public delegate bool VideoRecvFilter(byte[] from_ip);						 //视频接收过滤函数
	        public delegate void VideoRecvCallback(VIDEO_RECV_BUFF video_recv_buff);	//视频接收完成的回调函数	
            public VideoRecvFilter video_recv_filter;
            public VideoRecvCallback video_recv_callback;
	        //SHOT_INFO shot_info;		//抓拍信息

           
        }
        public class VIDEO_RECV_BUFF
        {
	        public byte[] buff;
	        public int len;
	        public int idel_flag;	
	        public int ready_flag;  
        }
        public class VideoDataHead       //视频包的头信息
        {
            public byte[]  PublicHead;    //公共头信息
            public byte[] DestAddr;       //目的地址
            public byte[] SourceAddr;     //源地址
            public byte DQT_Value;         //视频编码质量
            public byte VideoType;         //视频格式
            public int Frameno;           //帧序号
            public int Framelen;          //帧数据长度
            public int TotalPackage;     //总包数
            public int CurrPackage;      //当前包数
            public int Datalen;          //实际数据长度
            public int PackLen;          //分包时的单位大小,最后一个包实际的数据长度等于或小于该单位大小   

            public VideoDataHead(byte[] buff)
            {
                PublicHead = new byte[10];
                DestAddr = new byte[6];
                SourceAddr = new byte[6];
                Array.Copy(buff,0,PublicHead,0,10);
                Array.Copy(buff,10, DestAddr, 0, 6);
                Array.Copy(buff,16, SourceAddr, 0, 6);
                DQT_Value = buff[22];
                VideoType = buff[23];
                Frameno = (((int)buff[25] << 8) + buff[24]);
                Framelen = (((int)buff[29] << 24) + ((int)buff[28] << 16) + ((int)buff[27] << 8) + buff[26]);
                TotalPackage = (((int)buff[31] << 8) + buff[30]);
                CurrPackage = (((int)buff[33] << 8) + buff[32]);
                Datalen = (((int)buff[35] << 8) + buff[34]);
                PackLen = (((int)buff[37] << 8) + buff[36]);
            }

            public int GetLength()
            {
                return 38;
            }
        }
    }
}
