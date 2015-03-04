using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Validation;

namespace BaseStationTest
{
    public class PacketCreator
    {
        public static byte[] FormReadPacketWCS(byte[] header, byte[] id, int requestType)
        {
            if (header == null || id == null)
                return null;
            
            if (id.Length != 4)
                return null;

            byte[] tmp_packet = new byte[12];

            int pos = 0;
            for (uint i = 0; i < header.Length; i++, pos++)
                tmp_packet[pos] = header[i];

            for (uint i = 0; i < id.Length; i++, pos++)
                tmp_packet[pos] = id[i];

            tmp_packet[8] = (byte)requestType;

            byte[] checksum;
            ChecksumModule c = Ordinary.GetInstance();
            c.CalculateChecksum(tmp_packet, 9, out checksum);

            tmp_packet[9] = checksum[0];
            tmp_packet[10] = checksum[1];
            tmp_packet[11] = (byte)'\0';

            return tmp_packet;
        }

        public static byte[] FormWritePacketWCS(byte[] header, byte[] id, byte[] message, int requestType)
        {
            if (header == null || id == null || message == null)
                return null;

            if (id.Length != 4 || message.Length > 4)
                return null;

            byte[] tmp_packet = new byte[16];

            int pos = 0;
            for (uint i = 0; i < header.Length; i++, pos++)
                tmp_packet[pos] = header[i];

            for (uint i = 0; i < id.Length; i++, pos++)
                tmp_packet[pos] = id[i];

            tmp_packet[8] = (byte)requestType;
            pos++;

            for (uint i = 0; i < message.Length; i++, pos++)
                tmp_packet[pos] = message[i];

            int size = message.Length;

            if (message.Length < 4)
            {
                int pos2 = header.Length + id.Length + message.Length + 1;
                for (; pos2 < 13; pos2++) tmp_packet[pos2] = (byte)'_';
            }

            ChecksumModule c = Ordinary.GetInstance();
            byte[] checksum;
            c.CalculateChecksum(tmp_packet, 13, out checksum);

            tmp_packet[13] = checksum[0];
            tmp_packet[14] = checksum[1];
            tmp_packet[15] = (byte)'\0';

            return tmp_packet;
        }

        public static byte[] FormReadPacket(byte[] header, byte[] id, int requestType, ChecksumModule c)
        {
            if (id.Length != 4)
                return null;

            byte[] tmp_packet = new byte[11];

            int pos = 0;
            for (uint i = 0; i < header.Length; i++, pos++)
                tmp_packet[pos] = header[i];

            for (uint i = 0; i < id.Length; i++, pos++)
                tmp_packet[pos] = id[i];

            tmp_packet[8] = (byte)requestType;
            tmp_packet[9] = c.CalculateChecksum(tmp_packet, 9);
            tmp_packet[10] = (byte)'\0';

            return tmp_packet;
        }

        public static byte[] FormWritePacket(byte[] header, byte[] id, byte[] message, int requestType, ChecksumModule c)
        {
            if (id.Length != 4 || message.Length > 4)
                return null;

            byte[] tmp_packet = new byte[15];

            int pos = 0;
            for (uint i = 0; i < header.Length; i++, pos++)
                tmp_packet[pos] = header[i];

            for (uint i = 0; i < id.Length; i++, pos++)
                tmp_packet[pos] = id[i];

            tmp_packet[8] = (byte)requestType;
            pos++;

            for (uint i = 0; i < message.Length; i++, pos++)
                tmp_packet[pos] = message[i];

            if (message.Length < 4)
            {
                int pos2 = header.Length + id.Length + message.Length + 1;
                for(; pos2 < 13; pos2++) tmp_packet[pos] = (byte)'_';
            }

            tmp_packet[13] = c.CalculateChecksum(tmp_packet, 13);
            tmp_packet[14] = (byte)'\0';

            return tmp_packet;
        }
    }
}
