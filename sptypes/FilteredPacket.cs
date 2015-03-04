using System;

namespace SPRITE.Utility
{
    namespace Types
    {
        public class FilteredPacket
        {
            private byte[] packet;
            int size;

            public FilteredPacket(byte[] packet)
            {
                if (packet != null)
                {
                    this.size = packet.Length;
                    this.packet = packet;
                }
            }

            public bool IsEmpty() { return this.packet.Length == 0; }
            public byte[] Packet
            {
                get { return this.packet; }
                set
                {
                    if (this.packet == null && value != null)
                    {
                        this.packet = value;
                    }
                }
            }

            public int Size
            {
                get { return size; }
                set { this.size = value; }
            }
        }
    }
}
