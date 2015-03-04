using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Types;
using SPRITE.Utility.Communication;

namespace BaseStationTest
{
    public class RequestTransmitter
    {
        public static byte[] Transmit(FilteredPacket fp, ref AsyncClient ac)
        { 
            Console.WriteLine("Transmitting " + fp.Packet);
	
	        // thread blocking section
            /*
	        while(!client->sendData(
		        reinterpret_cast<unsigned char*>(const_cast<char*>(fp->getPacket().c_str())),
		        fp->getPacket().length())) {} // watch

	        std::cout << "waiting for response..." << std::endl;
	        while(!client->receiveData(recvBuff, 16)) {} // watch

	        *response = std::string(reinterpret_cast<char*>(recvBuff));
	
	        std::cout << "response: " << recvBuff << std::endl;
             * */
            return null;
        }
    }
}
