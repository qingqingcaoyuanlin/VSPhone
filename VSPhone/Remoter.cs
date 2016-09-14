using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSPhone
{
    static public class Remoter
    {
        static object remoterManageLock = new object();
        static int count;				//远程连接数量
	    static int video_count;		//视频连接数量
	    static int audio_count;		//音频连接数量
        public const int REMOTEMAX=8;
        public class REMOTRE
        {
            public int isValid;      //是否有效
            public byte[] IP = new byte[4];        //对方IP
            public byte[] Addr = new byte[6];      //对方Addr
            public int Videoflag;    // 1 表示需要发送视频 0 表示不用
            public int Audioflag;    // 1 表示需要发送音频 0 表示不用
        }
        static public REMOTRE[] remoter = new REMOTRE[REMOTEMAX];
        static public void Remoter_init()
        {
            for (int i = 0; i < REMOTEMAX; i++)
            {
                remoter[i] = new REMOTRE();
            }
        }
        static public void Remoter_Clear()
        {
            lock (remoterManageLock)
            {
                count = 0;
                video_count = 0;
                audio_count = 0;
                for (int i = 0; i < REMOTEMAX; i++)
                {
                    remoter[i].isValid = 0;
                    Array.Clear( remoter[i].IP,0,4);
                    Array.Clear(remoter[i].Addr, 0, 6);
                    remoter[i].Videoflag = 0;
                    remoter[i].Audioflag = 0;
                }
            }
        }
        static public int Remoter_Add(byte[] Addr, byte[] IP, int Audio_Flag, int Video_Flag)
        {
	        int i;
	       lock (remoterManageLock)
           {
	            i = Remoter_Search(IP);
                if(i != -1)
                {
                    Console.WriteLine("Remoter_Add: In RemoterList\n"+i);
                    return i;
                }
        
                for(i = 0; i < REMOTEMAX; i++)
                {
                    if(remoter[i].isValid == 0)
                    {  
                        ArrayDeal.memcpy(remoter[i].Addr, Addr, 6);
                        ArrayDeal.memcpy(remoter[i].IP, IP, 4);
                        remoter[i].Audioflag = Audio_Flag;
                        remoter[i].Videoflag = Video_Flag;
                        remoter[i].isValid = 1;
                        count ++;
                        if(Audio_Flag == 1)
                        {
            	            audio_count++;
                        }
                        if(Video_Flag == 1)
                        {
            	            video_count++;
                        }
                        Console.WriteLine("Add:Remotenum =  \n"+count);
                        return i;
                    }    
                }
           }
            return -1;
        }

        static public int Remoter_Remove(byte[] IP)
        {
            int i;
            lock(remoterManageLock)
            {
                for(i = 0; i < REMOTEMAX; i++)
                {
                    if(remoter[i].isValid == 1)
                    {
                        if(ArrayDeal.vs_strstr(remoter[i].IP,IP,4) > 0) 
                        {
                            remoter[i].isValid = 0;
                            count--;
                            if(remoter[i].Audioflag > 0)
                            {
                	            audio_count--;
                            }
                            if(remoter[i].Videoflag > 0)
               	            {
                	            video_count--;
                            }
                            Console.WriteLine("Remove:i =%d, Remotenum = "+count);
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        static public int Remoter_Search(byte[] IP)
        {
            int i;
            lock(remoterManageLock)
            {
                for(i = 0; i < REMOTEMAX; i++)
                {
                    if(remoter[i].isValid == 1)
                    {
                        if(ArrayDeal.vs_strstr(remoter[i].IP,IP,4) > 0) 
                        {
                            //printf("Remoter_Search:%d\n",i);
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        static public int Remoter_Set_VideoFlag(byte[] ip, int flag)
        {
	        int i;
	        lock(remoterManageLock)
            {
	            i = Remoter_Search(ip);
	            if(i == -1)
	            {
		            return -1;
	            }
	            if(remoter[i].Videoflag != flag)
	            {
		            remoter[i].Videoflag = flag;
		            video_count ++;
	            }
	        }
	        return 0;
        }

        static public int Remoter_Set_AudioFlag(byte[] ip, int flag)
        {
	        int i;
	        lock (remoterManageLock)
            {
	            i = Remoter_Search(ip);
	            if(i == -1)
	            {
		            return -1;
	            }
	            if(remoter[i].Audioflag != flag)
	            {
		            remoter[i].Audioflag = flag;
		            audio_count ++;
	            }
	        }
	        return 0;
        }

        static public int Remoter_get_count()
        {
            lock (remoterManageLock)
            {
                return count;
            }
        }
        static public int Remoter_get_video_count()
        {
	        lock (remoterManageLock)
            {
	            return video_count;
            }
        }

        static public int Remoter_get_audio_count()
        {
	        lock (remoterManageLock)
            {
	            return audio_count;
            }
        }

        static public int Remoter_get_ip(int i, byte[] ip)
        {
	       lock (remoterManageLock)
           {
	            if(remoter[i].isValid > 0)
	            {
		            ArrayDeal.memcpy(ip,remoter[i].IP,4);
	            }
	            else
	            {
		            return -1;
	            }
	            return i;
           }
        }

        static public int Remoter_get_addr(int i, byte[] addr)
        {
	        lock (remoterManageLock)
            {
	            if(remoter[i].isValid > 0)
	            {
		            ArrayDeal.memcpy(addr,remoter[i].Addr,6);
	            }
	            else
	            {
		            return -1;
	            }
	            return i;
            }
        }

        static public int Remoter_get_audio_flag(int i)
        {
	        lock (remoterManageLock)
            {
	            if(remoter[i].isValid > 0)
	            {
		           return remoter[i].Audioflag;
	            }
	            else
	            {
		            return -1;
	            }
	        }
        }

        static public int Remoter_get_video_flag(int i)
        {
	        lock (remoterManageLock)
            {
	            if(remoter[i].isValid > 0)
	            {
		           return remoter[i].Videoflag;
	            }
	            else
	            {
		            return -1;
	            }
	        }
        }

    }
}
