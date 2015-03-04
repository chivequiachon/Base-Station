using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using SPRITE.Utility.Types;

namespace BaseStationTest
{
    public class Extractor
    {
        public static String ExtractData(byte[] packet)
        {
            String str = Encoding.ASCII.GetString(packet);
            return str.Substring(9, 4);
        }

        public static String ExtractID(byte[] packet)
        {
            String str = Encoding.ASCII.GetString(packet);
            return str.Substring(4, 4);
        }

        public static int ExtractRequestType(byte[] packet)
        {
            return (int)packet[8];
        }
        
        public static String ExtractRealID(String id)
        {
            BigInteger a = (byte)id[0];
            BigInteger b = (byte)id[1];
            BigInteger c = (byte)id[2];
            BigInteger d = (byte)id[3];
            BigInteger res = (a*b*c)+d;
            return res.ToString();
        }

        public static bool Extract(ref StatusObserver so, ref EncryptedPacket input, out ExtractedData output)
        {
            if (input != null)
            {
                String householdID, data = null;
                int requestType = 0;

                if (!input.IsEmpty())
                {
                    so.PacketExtraction();

                    String encr_householdID = ExtractID(input.Packet);
                    
                    householdID = ExtractRealID(encr_householdID);
                    requestType = ExtractRequestType(input.Packet);

                    if (householdID == null || requestType == -1)
                    {
                        output = null;
                        return false;
                    }

                    // extract data
                    if (input.Size == 12) // this is a read packet
                    { 
                        // there is no data to be extracted from a read packet
                    }
                    else if (input.Size == 16) // this is a write packet
                    {
                        data = ExtractData(input.Packet);

                        // bounds checking
                        if (data == null)
                        {
                            output = null;
                            return false;
                        }
                    }

                    so.HouseholdIDExtracted(householdID);
                    so.DataExtracted(data);
                    so.OperationExtracted(requestType);

                    byte[] id = new byte[]
                    {
                        input.Packet[4],
                        input.Packet[5],
                        input.Packet[6],
                        input.Packet[7]
                    };

                    output = new ExtractedData(input.Packet, id, householdID, data, requestType);

                    so.ExtractionSuccess();
                    return true;
                }
            }

            so.ExtractionFailed();
            output = null;
	        return false;
        }
    }
}
