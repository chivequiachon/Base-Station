using System;

namespace SPRITE.Utility.Concurrency
{
    namespace Containers
    {
        public class Future<T>
        {
            private T val;

            public Future() { }
            public Future(T val)
            {
                this.val = val;
            }

            public T GetValue() { return val; }
            public void SetValue(T val) { this.val = val; }
        }
    }
    
}
