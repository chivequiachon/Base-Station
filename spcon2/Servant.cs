using System;
using System.Collections.Generic;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public class QueueServant<T>
        {
            private Queue<T> activeQueue = new Queue<T>();

            public QueueServant() { }

            public void Enqueue(T el) { activeQueue.Enqueue(el); }
            public T Dequeue() { return activeQueue.Dequeue(); }
            public Boolean IsEmpty() { return activeQueue.Count == 0; }
            public int GetSize() { return activeQueue.Count; }
        }
    }
}
