using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

namespace VSPhone
{
    public class Audio
    {
        public Audio(int port)
        {
	        audio_manage_init(port);
	        audio_device_init();
	        audio_udp_init();
	        audio_thread_init();
        }

        AUDIO_MANAGE audio_manage = new AUDIO_MANAGE();
        public const int AUDIO__PACK_BUFF_SIZE		=	2 * 1024;
        public const int AUDIO_PLAY_BUFF_SIZE		=    64*1024;  		//音频播放缓冲区大小
        public const int AUDIO_SEND_BUFF_SIZE		=    64*1024;  		//音频发送缓冲区大小

        public string playPath;
        public string projName;
        int audio_manage_init(int port)
        {
	        int i;
	        audio_manage.udp_port = port;
	        audio_manage.pack_buff = new byte[AUDIO__PACK_BUFF_SIZE];
	        audio_manage.send_buff = new byte[AUDIO_SEND_BUFF_SIZE];

            for (i = 0; i < AUDIO_MANAGE.AUDIO_CHANEL_COUNT; i++)
	        {
                audio_manage.audio_chanel[i] = new AUDIO_MANAGE.AUDIO_CHANEL();
		        audio_manage.audio_chanel[i].play_buff = new byte[AUDIO_PLAY_BUFF_SIZE];
		        if(i == 0)
		        {
			        audio_manage.audio_chanel[i].type =AUDIO_MANAGE.AUDIO_CHANEL.AUDIO_CHANEL_TYPE.AUDIO_NET_STREAM;				//通道0 作为网络音频通道
			        audio_manage.audio_chanel[i].priority = AUDIO_MANAGE.AUDIO_CHANEL.AUDIO_PLAY_PRIORITY.PRIORITY_NET_STREAM;
		        }
		        else if(i == 1)
		        {
                    audio_manage.audio_chanel[i].type = AUDIO_MANAGE.AUDIO_CHANEL.AUDIO_CHANEL_TYPE.AUDIO_LOCAL_STREAM;			//通道1 作为本地音频通道
		        }
		        else
		        {
                    audio_manage.audio_chanel[i].type = AUDIO_MANAGE.AUDIO_CHANEL.AUDIO_CHANEL_TYPE.AUDIO_SUPERPOSITION_STREAM;    //通道2 作为叠加音频通道
		        }
		        audio_manage.audio_chanel[i].file_name = null;
	        }	
            
	        return 0;
        }
        int audio_device_init()		//音频设备初始化
        {
            return 0;
        }
        int audio_udp_init()
        {
	        //T_BOOL ret = AK_FALSE;	
            try
            {
                audio_manage.udp_trans = new UdpClient(new IPEndPoint(new IPAddress(LocalCfg.IP), audio_manage.udp_port));
            }
            catch
            {
                Console.WriteLine("new UdpClient audio");
                return -1;
            }
	        return 0;
        }
        int audio_thread_init()
        {
	        //创建发送、接收和播放线程
            audio_manage.recv_thread = new Thread(thread_audio_recv);
            audio_manage.recv_thread.Start();
            audio_manage.play_thread = new Thread(thread_audio_play);
            audio_manage.play_thread.Start();
            audio_manage.send_thread = new Thread(thread_audio_send);
            audio_manage.send_thread.Start();
            
	        /*
	        pStackAddr = Fwl_Malloc(AUDIO_TASK_STACK_SIZE);
	        memset(pStackAddr, 0, AUDIO_TASK_STACK_SIZE);
	        audio_manage.send_thread = AK_Create_Task((T_VOID*)thread_audio_send, 
										        "AUDIO_SEND", 
										        0, 
										        AK_NULL, 
										        pStackAddr, 
										        AUDIO_TASK_STACK_SIZE, 
										        TASK_PRIO_AUDIO_SEND, 
										        AUDIO_TASK_TIME_SLICE, 
										        AK_PREEMPT, 
										        AK_START);
	        if (audio_manage.send_thread  < 0)
	        {
		        printf("audio_recv_task create failed %ld!\r\n", audio_manage.send_thread);
		        audio_manage.send_thread = AK_INVALID_TASK;
		        pStackAddr = Fwl_Free(pStackAddr);
		        return -1;
	        }
	        else
	        {
		        printf("audio_recv_task create ok, task id=%ld\r\n",audio_manage.send_thread);
	        }	
	        pStackAddr = Fwl_Malloc(AUDIO_TASK_STACK_SIZE);
	        memset(pStackAddr, 0, AUDIO_TASK_STACK_SIZE);
	        audio_manage.play_thread = AK_Create_Task((T_VOID*)thread_audio_play, 
										        "AUDIO_PLAY", 
										        0, 
										        AK_NULL, 
										        pStackAddr, 
										        AUDIO_TASK_STACK_SIZE, 
										        TASK_PRIO_AUDIO_PLAY, 
										        AUDIO_TASK_TIME_SLICE, 
										        AK_PREEMPT, 
										        AK_START);
	        if (audio_manage.play_thread  < 0)
	        {
		        printf("audio_play_task create failed %ld!\r\n", audio_manage.play_thread);
		        audio_manage.play_thread = AK_INVALID_TASK;
		        pStackAddr = Fwl_Free(pStackAddr);
		        return -1;
	        }
	        else
	        {
		        printf("audio_play_task create ok, task id=%ld\r\n",audio_manage.play_thread);
	        }	*/
	        return 0;
        }

        void thread_audio_recv()
        {
	        int free_space;
	        int recv_size=0;
    
	        byte[] recv_buff;
	        AudioPack head;
	        //Console.WriteLine("thread_audio_recv...\r\n");
            IPEndPoint recvEP = new IPEndPoint(IPAddress.Broadcast,audio_manage.udp_port);
	        while(true)
	        {
		        lock(audio_manage.recv_sem)
                {
                    Monitor.Wait(audio_manage.recv_sem);
                }
		        //Console.WriteLine("thread_audio_recv run...\r\n");
		        while(true)
		        {
			        recv_buff = audio_manage.udp_trans.Receive(ref recvEP);
			        //Console.WriteLine("thread_audio_recv");
			        if(audio_manage.audio_recv_filter != null)
			        {
				        if(audio_manage.audio_recv_filter(recvEP.Address.GetAddressBytes()) == false)
				        {
					        continue;
				        }
			        }
                    head = new AudioPack(recv_buff);
			        lock(audio_manage.recv_lock)
                    {
			            if(audio_manage.recv_flag == false)
			            {
				            break;
			            }
			            free_space = AUDIO_PLAY_BUFF_SIZE - audio_manage.audio_chanel[0].w_pos;
			
			            recv_size+=head.Datalen;    //统计音频数据接收速率
			           /* if(recv_size>=40000)
			            {
				            a = get_tick_count();
				            audio_manage.recv_bps = (recv_size*1000)/(a-b);
				            recv_size = 0;
				            b = a;
			            }*/

                        //Console.WriteLine("data len" + head.Datalen);
			            if(head.Datalen > free_space) 
                        { 
                            Array.Copy(head.audioData,0,audio_manage.audio_chanel[0].play_buff , audio_manage.audio_chanel[0].w_pos, free_space);
                            Array.Copy(head.audioData,free_space,audio_manage.audio_chanel[0].play_buff ,0, head.Datalen - free_space);
	 
				            audio_manage.audio_chanel[0].w_pos = head.Datalen - free_space;
				            if(audio_manage.audio_chanel[0].w_pos >= audio_manage.audio_chanel[0].r_pos)  //缓冲区溢出
				            {
					            Console.WriteLine("audio recv thread: buff overflow! "+audio_manage.audio_chanel[0].w_pos+audio_manage.audio_chanel[0].r_pos);
				            }
                        }
                        else
                        {
                            Array.Copy(head.audioData,0,audio_manage.audio_chanel[0].play_buff , audio_manage.audio_chanel[0].w_pos, head.Datalen);

	                        audio_manage.audio_chanel[0].w_pos += head.Datalen;
	                        if(audio_manage.audio_chanel[0].w_pos >= AUDIO_PLAY_BUFF_SIZE)
                            {
                                audio_manage.audio_chanel[0].w_pos = 0;
                            }
	                    }   
	                    if(audio_manage.audio_chanel[0].w_pos >= audio_manage.audio_chanel[0].r_pos)    //计算缓冲区中数据的长度
	                    {
	        	            audio_manage.audio_chanel[0].len = audio_manage.audio_chanel[0].w_pos-audio_manage.audio_chanel[0].r_pos;
	                    }
	                    else
	                    {
	        	            audio_manage.audio_chanel[0].len = AUDIO_PLAY_BUFF_SIZE + audio_manage.audio_chanel[0].w_pos - audio_manage.audio_chanel[0].r_pos;
	                    }
	                    //printf("Audio net recv:w_pos=%ld,r_pos=%ld,len=%ld\r\n",audio_manage.audio_chanel[0].w_pos,audio_manage.audio_chanel[0].r_pos,audio_manage.audio_chanel[0].len);
                    }
		        }
	        }
        }
        void thread_audio_send()
        {
	        int i;
	        //int read_num;
	        //T_U32 destip;
	       // T_U8 temp[20];
	        int data_len;
	        //T_U16 *p_liner;
	        //T_U8 *pbuf;
	        //AudioDataHead *head;
	        //int send_size=0;
	        //int a=0,b=0;
	        byte[] ip = new byte[4];
	        byte[] addr = new byte[6];
	
	        Console.WriteLine("thread_audio_send...");
	        //head = (AudioDataHead *)audio_manage.pack_buff;
            AudioPack head; 
            FileStream playFile = null;
	        while(true)
	        {
                lock(audio_manage.send_sem)
                {
                    Monitor.Wait(audio_manage.send_sem);
                }
                Console.WriteLine("thread_audio_send...");
                try
                {
                    if (playPath != null && playPath != "")
                    {
                        playFile = new FileStream(playPath, FileMode.Open);
                    }
                    else
                    {
                        playFile = null;
                    }
                }
                catch
                {
                    playFile = null;
                }
                if(playFile == null) continue;
		        Console.WriteLine("thread_audio_send file ok...");
		        while(true)
		        {
			        lock(audio_manage.send_lock)
                    {
			            if(audio_manage.send_flag == false)
			            {
				            //AK_Release_Semaphore(audio_manage.send_lock);
				            break;
			            }
		                //if(audio_manage.send_len >= 128)
		                {
		    	            //打包
		    	            //data_len = (audio_manage.send_len - (audio_manage.send_len % 128))/2; 
                            data_len = (int)(playFile.Length - playFile.Position);
		    	            if(data_len > 256)
		    	            {
                                data_len = 256;
		    	            }
                           

                            head = new AudioPack();
			                //memcpy(head->PublicHead,audio_manage.public_head, 10);  //公共包头
			                //memcpy(head->SourceAddr,audio_manage.source_addr, 6);//源地址
                            head.SourceAddr = LocalCfg.Addr;
			                head.Type= 0;
			                head.Fomat= 0;
			                head.Frameno = (UInt16)audio_manage.send_frame_no;
			                head.Framelen = (UInt32)data_len;     				//帧数据长度
			                head.TotalPackage = 1; 		  			//总包数
			                head.CurrPackage = 1;  		  			//当前包
			                head.Datalen = (UInt16)data_len;      				//数据长度
			                head.PackLen = (UInt16)data_len;
			                head.audioData = new byte[data_len];
                            playFile.Read(head.audioData,0,data_len);
                            if (data_len == 0)
                            {
                                playFile.Position = 0;
                            }
			                //转成alaw
                            /*
			                p_liner = (T_U16 *)(audio_manage.send_buff+audio_manage.send_rpos);
			                for(i=0; i<data_len; i++)    				
			                {
			    	            audio_manage.pack_buff[sizeof(AudioDataHead)+i] = linear2alaw(p_liner[i]);
			                }*/
				
				            audio_manage.send_rpos += data_len*2;
				            audio_manage.send_len -= data_len*2;
		                    audio_manage.send_frame_no++;
		                    audio_manage.send_frame_total++;
		        
		                    /*send_size += data_len;    		//统计发送速率
				            if(send_size >= 40000)
				            {
					            a = get_tick_count();
					            audio_manage.send_bps = (send_size*1000)/(a-b);
					            send_size = 0;
					            b = a;
				            }*/
		                    //printf("audio frame%d: len = %ld\r\n",audio_manage.send_frame_no,head->Datalen = data_len);
				            //AK_Release_Semaphore(audio_manage.send_lock);
				            //发送
				            for(i = 0; i < Remoter.REMOTEMAX; i++)
		                    {
		                        if(Remoter.Remoter_get_audio_flag(i) == 1)
			                    {
			        	            Remoter.Remoter_get_ip(i,ip);
			        	            Remoter.Remoter_get_addr(i,addr);
			        	            head.DestAddr=addr;	//目的地址
                                    audio_manage.udp_trans.Send(head.GetByte(), head.GetLength(), new IPEndPoint(new IPAddress(ip), audio_manage.udp_port));
                                   // Console.WriteLine("thread_audio_send...");
                                }
		                    }
                            Thread.Sleep(30);
		                 }
                        /*
		                 else
		                 {
		    	            //AK_Release_Semaphore(audio_manage.send_lock);
		    	            Thread.Sleep(5);
		                 }	*/		     
                    }
		        }
                if (playFile != null)
                {
                    playFile.Close();
                }
	        }
        }
        void thread_audio_play()
        {
	        int free_space;
	        int ret;
	        int data_len = 0;
	        byte[] play_buff;
            string wavNamePath;
            string CurTime;
	        Console.WriteLine("thread_audio_play...");
            System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
            
	        while(true)
	        {
                lock(audio_manage.play_sem)
                {
                    Monitor.Wait(audio_manage.play_sem);
                }
                data_len = 0;
                CurTime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                //CurTime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "-"+ DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                wavNamePath = Directory.GetCurrentDirectory() + "\\" + projName + "\\" + projName + "-"+ CurTime + ".wav";
                FileStream fs = new FileStream(wavNamePath, FileMode.Create);
                sp.Stream = new MemoryStream();
		        Console.WriteLine("thread_audio_play run...");
		        while(true)
		        {
			        lock(audio_manage.play_lock)
                    {
			            if(audio_manage.play_flag == false)
			            {
				            Console.WriteLine("play break");
				            break;
			            }

				        //printf("obtain_play_data len %d\r\n",obtain_len);
                        play_buff = obtain_play_data();
                        if (play_buff != null)
                        {
                            
                            
                            byte[] wavHead = WavAlawHead.GetHeadBytes(data_len);
                            fs.Position = 0;
                            fs.Write(wavHead, 0, wavHead.Length);
                            fs.Position = data_len + wavHead.Length;
                            fs.Write(play_buff, 0, play_buff.Length);
                            fs.Flush();
                            data_len += play_buff.Length;


                            
                            wavHead = WavAlawHead.GetHeadBytes(play_buff.Length);
                            sp.Stream.Write(wavHead, 0, wavHead.Length);
                            sp.Stream.Write(play_buff, 0, play_buff.Length);
                            //sp.Play();

                            
                            
                            //play_size += data_len;    		//统计播放速率
                            /*if(play_size>=40000)
                            {
                                a = get_tick_count();
                                audio_manage.play_bps = (play_size*500)/(a-b);
                                play_size = 0;
                                b = a;
                            }*/
                        }
                        else if (sp.Stream.Position == sp.Stream.Length)
                        {
                            if (audio_manage.audio_chanel[0].enable == 0)
                            {
                                if (is_waveout_play_done())	 //判断是否播放完成,播放完成关闭spk
                                {
                                    audio_manage.play_flag = false;
                                    //set_spk_mute(1);
                                    //stop_aec_process();
                                }
                                else
                                {
                                    Thread.Sleep(25);
                                }
                            }
                            else
                            {
                                Thread.Sleep(25);
                            }
                        }
                        else
                        {
                            Thread.Sleep(25);
                        }
			            
                    }
		        }
                fs.Close();
                sp.Stream.Close();
	        }
           
        }
        byte[] obtain_play_data()
        {
            lock (audio_manage.recv_lock)
            {
                if(play_buff_is_empty())
                {
                    return null;
                }
                else
                {
                    byte[] data = null;
                    if(audio_manage.audio_chanel[0].w_pos > audio_manage.audio_chanel[0].r_pos)
                    {

                        data = new byte[audio_manage.audio_chanel[0].w_pos - audio_manage.audio_chanel[0].r_pos];
                        Array.Copy(audio_manage.audio_chanel[0].play_buff, audio_manage.audio_chanel[0].r_pos, data,0,data.Length);
                    }
                    else if (audio_manage.audio_chanel[0].r_pos != audio_manage.audio_chanel[0].w_pos)
                    {
                        data = new byte[audio_manage.audio_chanel[0].play_buff.Length - audio_manage.audio_chanel[0].r_pos + audio_manage.audio_chanel[0].w_pos];
                        Array.Copy(audio_manage.audio_chanel[0].play_buff, audio_manage.audio_chanel[0].r_pos, data, 0, audio_manage.audio_chanel[0].play_buff.Length - audio_manage.audio_chanel[0].r_pos);
                        Array.Copy(audio_manage.audio_chanel[0].play_buff, 0, data, audio_manage.audio_chanel[0].play_buff.Length - audio_manage.audio_chanel[0].r_pos, audio_manage.audio_chanel[0].w_pos);
                    }
                    audio_manage.audio_chanel[0].r_pos = audio_manage.audio_chanel[0].w_pos;
                    return data;
                }
            }
        }

        public void toggle_audio_recv()
        {
	        lock(audio_manage.recv_lock)
            {
	            if(audio_manage.recv_flag == false)
	            {
		            audio_manage.recv_flag = true;
		            audio_manage.audio_chanel[0].len = 0;
		            audio_manage.audio_chanel[0].r_pos = 0;
		            audio_manage.audio_chanel[0].w_pos = 0;
		            audio_manage.audio_chanel[0].loop = 1;
		            audio_manage.audio_chanel[0].enable = 1;
		            //唤醒接收线程
                    lock(audio_manage.recv_sem)
                    {
                        Monitor.Pulse(audio_manage.recv_sem);
                    }
		            //toggle_aec_process(AEC_WORKING);
	            }
            }
	        toggle_audio_play();
        }
        void toggle_audio_play()
        {
            lock (audio_manage.play_lock)
            {
                if (audio_manage.play_flag == false)
                {
                    audio_manage.play_flag = true;
                    lock(audio_manage.play_sem)   //唤醒播放线程
                    {
                        Monitor.Pulse(audio_manage.play_sem);
                    }
                   // set_spk_mute(0);
                }
            }
        }
        public void toggle_audio_send()
        {
           
            lock (audio_manage.send_lock)
            {
                if (audio_manage.send_flag == false)
                {
                    audio_manage.send_frame_no = 0;
                    audio_manage.send_rpos = 0;
                    audio_manage.send_wpos = 0;
                    audio_manage.send_len = 0;
                    audio_manage.send_flag = true;
                    //enable_audio_info_printf(5000);
                    //toggle_aec_process(AEC_WORKING);
                    lock (audio_manage.send_sem)  //唤醒发送线程
                    {
                        Monitor.Pulse(audio_manage.send_sem);
                    }
                }
            }
        }
        public void stop_audio_send()
        {
            lock (audio_manage.send_lock)
            {
                if (audio_manage.send_flag == true)
                {
                    audio_manage.send_flag = false;
                    //disabel_audio_info_printf();
                }
            }
        }

        public void stop_audio_recv()
        {
            lock (audio_manage.recv_lock)
            {
                audio_manage.recv_flag = false;
                audio_manage.audio_chanel[0].enable = 0;
                //AK_Release_Semaphore(audio_manage.recv_lock);
            }
        }

        public bool play_buff_is_empty()
        {
            if(audio_manage.audio_chanel[0].w_pos != audio_manage.audio_chanel[0].r_pos)
            {
                return false;
            }
            return true;
        }
        bool is_waveout_play_done()
        {
	        int freeSpace;
	        if(play_buff_is_empty() == false)				//检测AEC播放缓冲区是否为空
	        {
		        return false;
	        }
            /*
	        if(aec_model != AEC_WORKING)
	        {
		        WaveOut_GetStatus(&freeSpace, WAVEOUT_SPACE_SIZE);	//检测驱动播放缓冲区剩余容量
		        if(freeSpace < 10240)
		        {
			        return false;
		        }
	        }	*/
	        return true;
        }
        public class AUDIO_MANAGE
        {
            public const int  AUDIO_CHANEL_COUNT 	=	3;
	        public byte[] public_head = new byte[10];
	        public byte[] source_addr= new byte[6];
	        public int udp_port;
	        public UdpClient udp_trans;
	        //T_AUDIO_DECODE_OUT output;
	        //T_pVOID	input;  
	
	        public bool send_flag;			//音频发送标志
	        public Thread send_thread;		//音频发送线程
	        public object send_sem = new object();		//信号量
	        public object send_lock = new object();	//锁
	        public byte[] send_buff; 			//音频发送缓冲区
	        public int send_rpos;			//发送缓冲区读指针
	        public int send_wpos;			//发送缓冲区写指针
	        public int send_len;			//发送缓冲区数据长度
	        public int send_bps;			//音频发送速率
	        public byte[] pack_buff;			//音频打包缓冲区
	        public int send_frame_no;		//音频发送帧号
	        public int send_frame_total;	//音频发送总帧数

	        public bool recv_flag;			//音频接收标志
	        public Thread recv_thread;		//音频接收线程
	        public int recv_bps;			//音频网络接收速率
	        public object recv_sem= new object();		//音频接收信号量
	        public object recv_lock= new object();	//锁
	        public int recv_frame_no;		//音频接收帧号
	        public int recv_frame_total;	//音频接收总帧数

	        public int play_state;			//音频播放状态
	        public bool play_flag;			//音频播放标志
	        public Thread play_thread;		//音频播放线程
	        public int play_bps;			//音频播放速率
	        public object play_sem= new object();		//音频播放信号量
	        public object play_lock= new object();	//锁

            public AUDIO_CHANEL[] audio_chanel = new AUDIO_CHANEL[AUDIO_CHANEL_COUNT];  //音频通道

            public setSpkMute set_spk_mute;
            public audioRecvFilter audio_recv_filter;
	        //call function
	        public delegate void setSpkMute(int ismute); 
	        public delegate bool audioRecvFilter(byte[]ip);

            public class AUDIO_CHANEL
            {
	            public int  enable;			//启用该通道
                public AUDIO_CHANEL_TYPE type;				//类型
	            public int  loop;				//是否循环
	            public int  playing;			//正在播放标志
	            public int  flag;				//标志
                public AUDIO_PLAY_PRIORITY priority;		//优先级
	            public string file_name;		//音频文件
	            public int file_len;		//音频文件大小
	            public byte[] play_buff;		//缓冲区
	            public int len;				//长度
	            public int r_pos;			//缓冲区读指针
	            public int w_pos;			//缓冲区写指针
	            public int load_pos;		//音频文件加载指针,当文件比较大时需要分段加载进缓冲区进行播放

                public enum AUDIO_PLAY_PRIORITY			//播放优先级
                {
	                PRIORITY_TOUCH = 10,					//按键音优先级  flag 建议使用 AUDIO_SUPERPOSITION
	                PRIORITY_PROMPT = 11,					//提示音优先级  flag 建议使用 AUDIO_SUPERPOSITION
	                PRIORITY_RING = 49,                   //响铃优先级
	                PRIORITY_ALARM = 50,					//报警音优先级
	                PRIORITY_NET_STREAM  = 100,			//网络音频流播放优先级
                }

                public enum AUDIO_CHANEL_TYPE		//通道类型
                {
	                AUDIO_NET_STREAM,						//网络音频流
	                AUDIO_LOCAL_STREAM,					//本地音频流
	                AUDIO_SUPERPOSITION_STREAM			//叠加音频流 主要用于播放按键音和提示语，一般是较短的音频,播放优先级最高
                }
            }
	
        }
        public class AudioPack       //音频包的头信息
        {
           // public byte[] PublicHead = new byte[] { (byte)'G', (byte)'V', (byte)'S', (byte)'G', (byte)'V', (byte)'S', 0xa5, 0xa5, 0xa5, 0xa5 };  //公共头信息
          //  public byte[] PublicHead = new byte[] { 0x47, 0x56, 0x53, 0xCC, 0xce, 0xcf, 0xce, 0xcf, 0xc8, 0x0 };  //公共头信息
            public byte[] PublicHead;       //公共头
            public byte[] DestAddr;       //目的地址
            public byte[] SourceAddr;     //源地址
            public byte Type;         	  //类型
            public byte Fomat;             //音频格式
            public UInt16 Frameno;           //帧序号
            public UInt32 Framelen;          //帧数据长度
            public UInt16 TotalPackage;     //总包数
            public UInt16 CurrPackage;       //当前包数
            public UInt16 Datalen;           //实际数据长度
            public UInt16 PackLen;           //分包时的单位大小,最后一个包实际的数据长度等于或小于该单位大小   
            UInt16 Half_duplex_arg;	  //早期的半双工参数
            UInt16 Half_duplex_arg2; //早期的半双工参数
            public byte[] audioData;
            public byte[] GetByte()
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(Form1.PublicHead, 0, 10);
                ms.Write(DestAddr, 0, 6);
                ms.Write(SourceAddr, 0, 6);
                ms.WriteByte(Type);
                ms.WriteByte(Fomat);
                ms.WriteByte((byte)(Frameno & 0xff));
                ms.WriteByte((byte)(Frameno >> 8));
                ms.WriteByte((byte)(Framelen & 0xff));
                ms.WriteByte((byte)(Framelen >> 8));
                ms.WriteByte((byte)(Framelen >> 16));
                ms.WriteByte((byte)(Framelen >> 24));
                ms.WriteByte((byte)(TotalPackage & 0xff));
                ms.WriteByte((byte)(TotalPackage >> 8));
                ms.WriteByte((byte)(CurrPackage & 0xff));
                ms.WriteByte((byte)(CurrPackage >> 8));
                ms.WriteByte((byte)(Datalen & 0xff));
                ms.WriteByte((byte)(Datalen >> 8));
                ms.WriteByte((byte)(PackLen & 0xff));
                ms.WriteByte((byte)(PackLen >> 8));
                ms.WriteByte((byte)(Half_duplex_arg & 0xff));
                ms.WriteByte((byte)(Half_duplex_arg >> 8));
                ms.WriteByte((byte)(Half_duplex_arg2 & 0xff));
                ms.WriteByte((byte)(Half_duplex_arg2 >> 8));
                if (audioData != null)
                {
                    ms.Write(audioData, 0, audioData.Length);
                }
                ms.Position = 0;
                byte[] d = new byte[ms.Length];
                ms.Read(d,0,(int)ms.Length);
                ms.Close();
                return d;
            }
            public int GetLength()
            {
                if (audioData != null)
                {
                    return 10 + 6 + 6 + 20 + audioData.Length;
                }
                else
                {
                    return 10 + 6 + 6 + 20;
                }
            }
            public AudioPack()
            {

            }
            public AudioPack(byte[] buff)
            {
                DestAddr = new byte[6];
                Array.Copy(buff, 10, DestAddr,0,6);
                SourceAddr = new byte[6];
                Array.Copy(buff, 16, SourceAddr, 0, 6);
                Type = buff[22];
                Fomat = buff[23];
                Frameno = (UInt16)(((int)buff[25] << 8) + buff[24]);
                Framelen = (UInt32)(((int)buff[29] << 24) + ((int)buff[28] << 16) + ((int)buff[27] << 8) + buff[26]);
                TotalPackage = (UInt16)(((int)buff[31] << 8) + buff[30]);
                CurrPackage = (UInt16)(((int)buff[33] << 8) + buff[32]);
                Datalen = (UInt16)(((int)buff[35] << 8) + buff[34]);
                PackLen = (UInt16)(((int)buff[37] << 8) + buff[36]);
                if (Datalen != 0)
                {
                    audioData = new byte[Datalen];
                    Array.Copy(buff, 42, audioData, 0, Datalen);
                }
            }
        }

        static public class WavAlawHead
        {
            static public byte[] GetHeadBytes(int dataLen)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }, 0, 4);
                ms.Write(new byte[] { (byte)(dataLen + 50), (byte)(dataLen + 50 >> 8), (byte)(dataLen + 50 >> 16), (byte)(dataLen + 50 >> 24) }, 0, 4);
                ms.Write(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t' }, 0, 7);
                ms.Write(new byte[] { 0x20,0x12,0,0 ,0}, 0, 5);
                ms.Write(new byte[] { 0x06, 0}, 0, 2);
                ms.Write(new byte[] { 0x1, 0 }, 0, 2);
                ms.Write(new byte[] { 0x40,0x1f,0,0 }, 0, 4);
                ms.Write(new byte[] { 0x40, 0x1f,0,0 }, 0, 4);
                ms.Write(new byte[] { 0x01, 0x00 }, 0, 2);
                ms.Write(new byte[] { 0x8, 0, 0, 0 }, 0, 4);
                ms.Write(new byte[] { (byte)'f', (byte)'a', (byte)'c', (byte)'t' }, 0, 4);
                ms.Write(new byte[] { 0x04, 0, 0, 0,0,0x8f,0x00,0}, 0, 8);
                ms.Write(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' }, 0, 4);
                ms.Write(new byte[] { (byte)(dataLen), (byte)(dataLen >> 8), (byte)(dataLen >> 16), (byte)(dataLen >> 24) }, 0, 4);
                ms.Position = 0;
                byte[] data = new byte[ms.Length];
                ms.Read(data,0,data.Length);
                return data;
            }
        }
    }
}
