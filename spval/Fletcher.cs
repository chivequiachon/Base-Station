using System;

namespace SPRITE.Utility
{
    namespace Validation
    {
        public class Fletcher : ChecksumModule
        {
            private static Fletcher instance = null;
            private static object objectLock = new object();

            public static ChecksumModule GetInstance()
            {
                if (instance == null)
                {
                    lock (objectLock)
                    {
                        instance = new Fletcher();
                    }
                }

                return instance;
            }

            public static void DisposeInstance()
            {
                if (instance != null)
                {
                    lock (objectLock)
                    {
                        instance = null;
                    }
                }
            }

            private Fletcher()
            {
            }

            public void CalculateChecksum(byte[] packet, int lenght, out byte[] checksum)
            {
                checksum = null;
                return;
            }

            public byte CalculateChecksum(byte[] packet, int length)
            {
                if (packet == null || length < 1) return 0;

                ushort sum1 = 0xf, sum2 = 0xf;

                for (int i = 0; i < length; i++)
                {
                    sum1 += packet[i];
                    sum2 += sum1;
                }

                sum1 = (ushort)((sum1 & 0x0f) + (sum1 >> 4));
                sum1 = (ushort)((sum1 & 0x0f) + (sum1 >> 4));
                sum2 = (ushort)((sum2 & 0x0f) + (sum2 >> 4));
                sum2 = (ushort)((sum2 & 0x0f) + (sum2 >> 4));

                return (byte)((sum2 << 4) | sum1);
            }
        }
    }
}
