using System;

namespace SPRITE.Utility
{
    namespace Types
    {
        public class EncryptedPacket
        {
            private byte[] packet;
            int size;

            public EncryptedPacket(byte[] packet)
            {
                if (packet != null)
                {
                    this.size = packet.Length;
                    this.packet = packet;
                }
            }

            public bool IsEmpty()
            {
                if (this.packet != null)
                {
                    return this.packet.Length == 0;
                }

                return false;
            }

            public byte[] Packet
            {
                get
                {
                    if (this.packet != null)
                        return this.packet;
                    else
                        return null;
                }

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
