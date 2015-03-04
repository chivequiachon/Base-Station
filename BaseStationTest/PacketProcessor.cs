using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStationTest
{
    public abstract class PacketProcessor
    {
        private PacketProcessor next;

        public PacketProcessor()
        {
            this.next = null;
        }

        public void SetNext(PacketProcessor next)
        {
            this.next = next;
        }

        public void Process(Object o)
        {
            if (o != null)
            {
                if (Execute(o))
                {
                    if (next != null)
                        next.Process(this);
                }
            }
        }

        abstract protected bool Execute(Object o);
    }

}
