using System;
using System.Threading;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public class Promise<T>
        {
            private Future<T> f;
            private ManualResetEvent manualEvent = new ManualResetEvent(false);

            public Promise() { this.f = new Future<T>(); }
            public Promise(T mh) { this.f = new Future<T>(mh); }

            public Promise(Promise<T> p)
            {
                this.f = p.GetFuture();
            }

            public Future<T> GetFuture() { return f; }

            public Promise<T> Ready(Promise<T> p)
            {
                this.f = p.GetFuture();
                return this;
            }

            public Promise<T> Ready(T mh)
            {
                this.f.SetValue(mh);
                manualEvent.Set(); // notify that data is ready
                return this;
            }

            public T Result()
            {
                manualEvent.WaitOne();
                return f.GetValue();
            }
        }
    }
}
