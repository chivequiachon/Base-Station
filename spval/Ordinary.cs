using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRITE.Utility
{
    namespace Validation
    {
        public class Ordinary : ChecksumModule
        {
            private static Ordinary instance = null;
            private static object objectLock = new object();
                        
            public static ChecksumModule GetInstance()
            {
                if (instance == null)
                {
                    lock (objectLock)
                    {
                        instance = new Ordinary();
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

            private Ordinary()
            {
            }

            public byte CalculateChecksum(byte[] packet, int lenght)
            {
                return 0;
            }

            public void CalculateChecksum(byte[] packet, int length, out byte[] checksum)
            {
                if (packet == null || length < 1)
                {
                    checksum = null;
                    return;
                }

                int sum = 0;

                for (int i = 0; i < length; i++)
                    sum += packet[i];

                checksum = new byte[2];

                checksum[0] = (byte)(sum >> 8);
                checksum[1] = (byte)sum;
            }
        }
    }
}
