using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Types;
using SPRITE.Utility.Validation;

namespace BaseStationTest
{
    public class Filtrator
    {
        public static bool Filter(ref StatusObserver so, ref ExtractedData ed, ref ChecksumModule vm, out FilteredPacket filteredPacket)
        {
            if(ed != null)
            {
                so.PacketFiltering();

	            String householdID = ed.HouseholdID;
	            String data = null;
	            int requestType = 0;

	            if(householdID != null)
	            {
		            // get all data from the extracted message
		            data = ed.Data;
		            requestType = ed.RequestType;

		            // check requested operation
		            switch(requestType)
		            {
			            case 1: // water usage
			            case 2: // update water balance
			            case 3: // wcs authentication
			            case 4: // retrieve water balance
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10: 
                        case 11:
                        case 12: break;
                        default:
                            filteredPacket = null;
                            return false;
		            };

                    byte[] id = ed.EncryptedID;
                    byte[] header = Encoding.ASCII.GetBytes("4p?2");

                    if (data == null)
                    {
                        filteredPacket = new FilteredPacket(PacketCreator.FormReadPacket(header, id, requestType, vm));
                    }
                    else
                    {
                        data.Trim('_');

                        char[] char_message = data.ToCharArray();
                        byte[] message = new byte[4];

                        for(int i = 0; i < 4; i++)
                            message[i] = (byte)char_message[i];

                        filteredPacket = new FilteredPacket(PacketCreator.FormWritePacket(header, id, message, requestType, vm));
                    }
                    
                    so.FiltrationSuccess();
                    return true;
	            }
            }

            so.FiltrationFailed();
            filteredPacket = null;
	        return false;
        }
    }
}
