using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Types;
using SPRITE.Utility.Validation;

namespace BaseStationTest
{
    public class Validator
    {
        public static bool Validate(ref StatusObserver so, ref EncryptedPacket unvalidatedPacket, ChecksumModule vm, out EncryptedPacket validatedPacket)
        {
            Console.WriteLine("Validator:");
            Console.WriteLine(1);
            if (unvalidatedPacket != null && vm != null)
            {
                Console.WriteLine(2);
                if (!unvalidatedPacket.IsEmpty())
                {
                    so.PacketProcessing();
                    so.PacketValidation();

                    byte[] buffer = unvalidatedPacket.Packet;

                    if (buffer != null)
                    {
                        Console.WriteLine(3 + "---");
                        Console.WriteLine(buffer[0]);
                        Console.WriteLine(buffer[1]);
                        Console.WriteLine(buffer[2]);
                        Console.WriteLine(buffer[3]);
                        if (buffer[0] == (byte)'4' && buffer[1] == (byte)'p' && buffer[2] == (byte)'?' && buffer[3] == (byte)'2')
                        {
                            so.ChecksumComparison();

                            Console.WriteLine(4);
                            if (unvalidatedPacket.Size == 12) // read Packet
                            {
                                // validate data using ordinary checksum algorithm
                                byte[] checksum;
                                vm.CalculateChecksum(buffer, 9, out checksum);

                                if (checksum != null && buffer[9] == checksum[0] && buffer[10] == checksum[1])
                                {
                                    so.ValidationSuccess();

                                    // return validated packet
                                    validatedPacket = new EncryptedPacket(buffer);
                                    return true;
                                }
                            }
                            else if (unvalidatedPacket.Size == 16) // write packet
                            {
                                // validate data using ordinary checksum algorithm
                                byte[] checksum;
                                vm.CalculateChecksum(buffer, 13, out checksum);

                                Console.WriteLine(5);
                                if (checksum != null && buffer[13] == checksum[0] && buffer[14] == checksum[1])
                                {
                                    so.ValidationSuccess();

                                    // return validated packet
                                    validatedPacket = new EncryptedPacket(buffer);
                                    return true;
                                }
                            }
                        }
                    }

                    so.ValidationFailed();
                }
            }

            validatedPacket = null;
            return false;
        }
    }
}
