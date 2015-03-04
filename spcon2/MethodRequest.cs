using System;
using System.Threading;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public interface MethodRequest
        {
            void Run();
            Boolean IsRunnable();
        }

        public class MR_Enqueue<T> : MethodRequest
        {
            private QueueServant<T> qsp;
            private T element;

            public MR_Enqueue(QueueServant<T> qsp, T element)
            {
                this.qsp = qsp;
                this.element = element;
            }

            public Boolean IsRunnable() { return true; }
            public void Run()
            {
                qsp.Enqueue(element);
            }
        }

        public class MR_Dequeue<T> : MethodRequest
        {
            private QueueServant<T> qsp;
            private Promise<T> promised_value;

            public MR_Dequeue(QueueServant<T> qsp, Promise<T> promised_value)
            {
                this.qsp = qsp;
                this.promised_value = promised_value;
            }

            public Boolean IsRunnable() { return !qsp.IsEmpty(); }

            public void Run()
            {
                promised_value = promised_value.Ready(qsp.Dequeue());
            }
        }
    }
}
