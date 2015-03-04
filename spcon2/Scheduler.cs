using System;
using System.Collections.Generic;
using System.Threading;
using SPRITE.Utility.Concurrency;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public class Scheduler
        {
            private ConcurrentThreadPool pool;
            private Queue<IMethodRequest> requests;
            private String threadName;
            private object schedulerLock;

            public Scheduler(String threadName)
            {
                this.pool = ConcurrentThreadPool.GetInstance(0);
                this.threadName = threadName;
                this.schedulerLock = new object();
                this.requests = new Queue<IMethodRequest>();
            }

            public String ThreadName
            {
                get { return this.threadName; }
            }

            public void Run()
            {
                this.pool.AddTask(
                    new CustomizedTask(
                        threadName,
                        () =>
                        {
                            if (requests.Count != 0)
                            {
                                IMethodRequest request = null;
                                lock (schedulerLock)
                                {
                                    request = requests.Dequeue();
                                }

                                if (request.IsRunnable())
                                    request.Run();
                            }
                        },
                        true,
                        false
                    )
                );
            }

            public bool Enqueue(IMethodRequest request)
            {
                if (request != null)
                {
                    requests.Enqueue(request);
                    return true;
                }

                return false;
            }
        }
    }
}
