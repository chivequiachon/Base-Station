using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStationTest
{
    public class ConcurrentQueue<T>
    {
        private Queue<T> queue = null;
        private object queueLock = new object();

        public Queue<T> Queue
        {
            get { return this.queue; }
            set { this.queue = value; }
        }

        public ConcurrentQueue()
        {
            queue = new Queue<T>();
        }

        public ConcurrentQueue(Queue<T> queue)
        {
            if (queue != null)
            {
                this.queue = queue;
            }
        }

        public ConcurrentQueue(ConcurrentQueue<T> cq)
        {
            if (cq != null && cq.queue != null)
            {
                this.queue = cq.queue;
            }
        }

        public bool Enqueue(T element)
        {
            if (queue != null)
            {
                queue.Enqueue(element);
                return true;
            }

            return false;
        }

        public T Dequeue()
        {
            if (queue != null)
            {
                return queue.Count != 0 ? queue.Dequeue() : default(T);
            }

            return default(T);
        }
    }
}
