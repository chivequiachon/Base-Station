using System;

namespace SPRITE.Utility
{
    namespace Validation
    {
        public interface ChecksumModule
        {
            byte CalculateChecksum(byte[] packet, int length);
            void CalculateChecksum(byte[] packet, int length, out byte[] checksum);
        }
    }
}
