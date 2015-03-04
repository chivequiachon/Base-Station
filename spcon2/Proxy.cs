using System;
using System.Collections.Generic;
using SPRITE.Utility.Concurrency;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public class QueueServantProxy<T>
        {
            private QueueServant<T> servant = new QueueServant<T>();
            private Scheduler s;

            public QueueServantProxy(String schedulerThreadID, ConcurrentThreadPool t)
            {
                s = new Scheduler(schedulerThreadID, t);
                s.Run();
            }

            public void Enqueue(T element)
            {
                MethodRequest mr_enqueue = new MR_Enqueue<T>(this.servant, element);
                s.EnqueueMethodRequest(mr_enqueue);
            }

            public Promise<T> dequeue()
            {
                Promise<T> promised_value = new Promise<T>();
                MethodRequest mr_dequeue = new MR_Dequeue<T>(this.servant, promised_value);
                s.EnqueueMethodRequest(mr_dequeue);
                return promised_value;
            }

            public int getSize() { return servant.GetSize(); }
        }
    }
}
