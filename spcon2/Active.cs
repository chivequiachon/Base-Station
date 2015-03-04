using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SPRITE.Utility.Concurrency;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public interface IMethodRequest
        {
            bool IsRunnable();
            void Run();
        }

        public class ActiveQueue<T>
        {
            private Queue<T> container;
            private Scheduler scheduler;
            private ManualResetEvent blocker;
            private FutureValue future;

            public class FutureValue
            {
                private ActiveQueue<T> thisQueue;
                private T promisedValue;
                private bool isReady;

                public FutureValue(ActiveQueue<T> thisQueue)
                {
                    this.thisQueue = thisQueue;
                    this.promisedValue = default(T);
                    this.isReady = false;
                }

                public bool IsReady
                {
                    set { this.isReady = value; }
                    get { return this.isReady; }
                }

                public void Ready(T element)
                {
                    promisedValue = element;
                    this.isReady = true;
                    thisQueue.blocker.Set();
                }

                public T GetPromisedValue(int timeout, T defValue)
                {
                    if (timeout > 0)
                    {
                        bool flag = thisQueue.blocker.WaitOne(timeout);

                        if (flag)
                        {
                            isReady = false;
                            return this.promisedValue;
                        }
                        else
                        {
                            return defValue;
                        }
                    }

                    thisQueue.blocker.WaitOne();
                    isReady = false;
                    return this.promisedValue;
                }
            }

            private class MR_Enqueue : IMethodRequest
            {
                private ActiveQueue<T> thisQueue;
                private T element;

                public MR_Enqueue(ActiveQueue<T> thisQueue, T element)
                {
                    this.thisQueue = thisQueue;
                    this.element = element;
                }

                public bool IsRunnable()
                {
                    return true;
                }

                public void Run()
                {
                    thisQueue.container.Enqueue(element);
                }
            }

            private class MR_Dequeue : IMethodRequest
            {
                private ActiveQueue<T> thisQueue;

                public MR_Dequeue(ActiveQueue<T> thisQueue)
                {
                    this.thisQueue = thisQueue;
                }

                public bool IsRunnable()
                {
                    return thisQueue.container.Count != 0;
                }

                public void Run()
                {
                    thisQueue.future.Ready(thisQueue.container.Dequeue());
                }
            }

            public ActiveQueue(String threadName)
            {
                container = new Queue<T>();
                future = new FutureValue(this);
                blocker = new ManualResetEvent(false);
                scheduler = new Scheduler(threadName);
            }

            public void Run()
            {
                scheduler.Run();
            }

            public bool Enqueue(T element)
            {
                if (element != null)
                {
                    /// enqueue to activation list
                    IMethodRequest enqueueRequest = new MR_Enqueue(this, element);
                    scheduler.Enqueue(enqueueRequest);
                    return true;
                }

                return false;
            }

            public FutureValue Dequeue()
            {
                /// enqueue to activation list
                if (future.IsReady)
                {
                    future.IsReady = false;
                    return future;
                }

                if (container.Count != 0)
                {
                    blocker.Reset();

                    IMethodRequest dequeueRequest = new MR_Dequeue(this);
                    scheduler.Enqueue(dequeueRequest);

                    return future;
                }

                return null;
            }

            public int GetSize()
            {
                return container.Count;
            }
        }
    }
}
