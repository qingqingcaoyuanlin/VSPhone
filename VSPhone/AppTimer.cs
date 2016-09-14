using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VSPhone
{
    static public class AppTimer
    {
        public class AppTimerMember
        {
            public int   id;			  			  		//唯一标识
            public string name;      						//定时器名称
            public int   isValid;  	  			  		//是否有效
            public int   isStart;  	  			  		//是否开始计时
            public int   timeout;							//超时标志
            public int counter; 		  			  		//定时的时长
            public int oldcounter; 	  			  		//设置时长	
            public int life;     	  			  		//生存周期
            public int argc;								//参数长度
            public object argv;		  			  		//参数
            public func func;
        }
        public delegate void func(int arg1, object arg2);
        static public object timer_lock = new object();
        const int APP_TIMER_COUNT = 30;		//定时器个数

        static private AppTimerMember curren_deal_timer;
        static private AppTimerMember[] app_timer_list = new AppTimerMember[APP_TIMER_COUNT];
        static private object timer_sync_semaphore = new object();
        static Thread app_timer_task;
        static Timer hTimer;
        static public int app_timer_init()
        {
	        Console.WriteLine("app timer init...");
	
	        //创建定时器列表
	        for(int i=0; i<APP_TIMER_COUNT; i++)
	        {
                app_timer_list[i] = new AppTimerMember();
		        app_timer_list[i].id = i;
		        app_timer_list[i].isValid = 0;
	        }
	        curren_deal_timer = null;

	        //初始化信号量
	        //timer_sync_semaphore = AK_Create_Semaphore(0,AK_PRIORITY);
	
	        //创建定时器处理线程
            app_timer_task = new Thread(thread_app_timer_deal);
            app_timer_task.Start();
	        //初始化定时器
            hTimer = new Timer(app_timer_update); 
            hTimer.Change(0,50);
	        
	        return 0;
        }

        static public AppTimerMember register_timer(string name, func func, int argc, object argv, int counter, int life)  //counter = DelayTime/TIMER_TICK
        {
            int i;
            lock (timer_lock)	//加锁
            {
                for (i = 0; i < APP_TIMER_COUNT; i++)
                {
                    if (curren_deal_timer != null && curren_deal_timer.id == i)
                    {
                        continue;
                    }
                    if (app_timer_list[i].isValid == 0)
                    {
                        if (name != null)
                        {
                            app_timer_list[i].name = name;
                        }
                        else
                        {
                            app_timer_list[i].name = null;
                        }

                        if (argv != null)
                        {
                           app_timer_list[i].argv = argv;
                        }
                        else
                        {
                            app_timer_list[i].argv = null;
                        }
                        app_timer_list[i].timeout = 0;
                        app_timer_list[i].counter = 0;
                        app_timer_list[i].oldcounter = counter;
                        app_timer_list[i].func = func;
                        app_timer_list[i].life = life;
                        app_timer_list[i].isStart = 0;
                        app_timer_list[i].isValid = 1;
                        Console.WriteLine("Register_Timer:"+ app_timer_list[i].id);
                        return app_timer_list[i];
                    }
                }
            }
            Console.WriteLine("app timer is full...");
            return null;
        }
        static public int start_timer(AppTimerMember timer)
        {
            lock (timer_lock)
            {
                if (timer == null || timer.isValid == 0)
                {
                    Console.WriteLine("Start_Timer:timer无效");
                    //release_nest_lock(timer_lock);
                    return -1;
                }
                timer.isStart = 1;
            }
            Console.WriteLine("Start Timer"+ timer.id);
            return 0;
        }
        static public int get_timer_life(AppTimerMember timer)
        {
            int life;
            lock (timer_lock)
            {
                if (timer == null || timer.isValid == 0)
                {
                    Console.WriteLine("get_timer_life:timer无效\r\n");
                    //release_nest_lock(timer_lock);
                    return -1;
                }
                life = timer.life;
                //release_nest_lock(timer_lock);
            }
            return life;
        }
        static public AppTimerMember get_current_deal_timer()		
        {
            AppTimerMember timer;
            lock (timer_lock)
            {
                timer = curren_deal_timer;
            }
	        //release_nest_lock(timer_lock);
	        return timer;
        }
        static public int stop_timer(AppTimerMember timer)
        {
            lock (timer_lock)
            {
                if (timer == null || timer.isValid == 0)
                {
                    Console.WriteLine("Stop_timer:timer无效");
                    //release_nest_lock(timer_lock);
                    return -1;
                }
                timer.isStart = 0;
                //release_nest_lock(timer_lock);
            }
            Console.WriteLine("Stop Timer"+timer.id);
            return 0;
        }
        static public int reset_timer_count(AppTimerMember timer)
        {
            lock (timer_lock)
            {
                if (timer == null || timer.isValid == 0)
                {
                    Console.WriteLine("set_timer_count:timer无效");
                    //release_nest_lock(timer_lock);
                    return -1;
                }
                timer.counter = 0;
                timer.timeout = 0;
                //release_nest_lock(timer_lock);
            }
            return 0;
        }
        static public int destroy_timer(AppTimerMember timer)
        {
            if (timer == null)
            {
                Console.WriteLine("Destroy_Timer:timer无效\r\n");
                return -1;
            }
            lock (timer_lock)
            {
                if (timer.isValid > 0)
                {
                    timer.isValid = 0;
                    timer.name ="";
                    timer.argv = null;
                }
            }
            Console.WriteLine("Destroy timer"+ timer.id);
            return 0;
        }
        static public AppTimerMember search_timer_by_func(func func)
        {
	        int i;
            lock (timer_lock)
            {
                for (i = 0; i < APP_TIMER_COUNT; i++)
                {
                    if (app_timer_list[i].isValid > 0 && app_timer_list[i].func == func)
                    {
                        return app_timer_list[i];
                    }
                }
            }
	        return null;
        }
        static  void thread_app_timer_deal()
        {
	        int i;
	        Console.WriteLine("thread_app_timer_deal...\r\n");
	        while(true)
	        {
		        //AK_Obtain_Semaphore(timer_sync_semaphore, AK_SUSPEND);	//等待信号量
		        //AK_Reset_Semaphore(timer_sync_semaphore, 0);
                lock (timer_sync_semaphore)
                {
                    Monitor.Wait(timer_sync_semaphore);

                }
		        //printf("timer deal thread run...\r\n");
		        for(i=0; i<APP_TIMER_COUNT; i++)
		        {
			        lock(timer_lock)
                    {
			            if(app_timer_list[i].isValid > 0 && app_timer_list[i].timeout > 0 && app_timer_list[i].isStart > 0) 
			            {
				            curren_deal_timer = app_timer_list[i];
				            app_timer_list[i].counter = 0;
				            app_timer_list[i].timeout = 0;
				            if(app_timer_list[i].life > 0)
				            {
					            app_timer_list[i].life--;
					            if(app_timer_list[i].func != null)
					            {
						            app_timer_list[i].func(app_timer_list[i].argc, app_timer_list[i].argv);
					            }
					            if(app_timer_list[i].life == 0)
					            {
						            app_timer_list[i].isValid = 0;
						            Console.WriteLine("timer"+app_timer_list[i].id+" exit!\n");
					            }
				            }
				            else
				            {
					            if(app_timer_list[i].func != null)
					            {
						            app_timer_list[i].func(app_timer_list[i].argc, app_timer_list[i].argv);
					            }
				            }
			            }
			        //release_nest_lock(timer_lock);
                    }
		        }
	        }
        }

        static void app_timer_update(object sta)
        {
	
	        int i;
	        bool flag = false;
	        lock(timer_lock)
            {
	            for(i=0; i<APP_TIMER_COUNT; i++)
	            {
		            if(app_timer_list[i].isValid > 0 && app_timer_list[i].isStart > 0 && app_timer_list[i].timeout == 0)
		            {
			            app_timer_list[i].counter += 50;
			            if(app_timer_list[i].counter >= app_timer_list[i].oldcounter)
			            {
				            app_timer_list[i].timeout = 1;
				            flag = true;
			            }
		            }
	            }
	            if(flag == true)
	            {
		            //printf("Wake up timer deal thread...\r\n");
		            //AK_Release_Semaphore(timer_sync_semaphore);
                    lock(timer_sync_semaphore)
                    {
                        Monitor.Pulse(timer_sync_semaphore);
                    }
	            }
	            //release_nest_lock(timer_lock);
            }
        }

    }
}
