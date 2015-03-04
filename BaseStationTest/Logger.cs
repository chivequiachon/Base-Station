using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Types;
using SPRITE.Utility.Validation;
using System.Net.Sockets;
using System.Collections.Concurrent;
using SPRITE.Utility.Concurrency.Containers;

namespace BaseStationTest
{
    public class Logger
    {   
        public Logger()
        {
        }

        public static bool Log(ref ConcurrentDictionary<String, EncryptedConnectionInfo> socketMap,
                               ref ActiveQueue<EncryptedPacket> receivedPackets,
                               ref StatusObserver so, ref ExtractedData ed, int startTime)
        {
            if (ed != null)
            {
                so.PacketLogging();

                String realID = ed.HouseholdID;
                EncryptedConnectionInfo eci = null;

                switch (ed.RequestType)
                {
                    case 6: // wcs connection status update
                        if (socketMap.TryGetValue(realID, out eci))
                        {
                            if (eci.EncryptedID == null)
                            {
                                // connection will be stopped
                                so.ConnectionStopped();
                                so.PacketProcessingStopped();
                                return false;
                            }
                            else
                            {
                                // reset timer
                                //so.TimerSet();

                                // reset time in the dictionary
                                //EncryptedConnectionInfo newEci = eci;
                                //newEci.TimeRemaining = startTime;

                                //if (!socketMap.TryUpdate(realID, newEci, eci))
                                //    return false;

                                //so.UpdateTable(realID, startTime);
                            }
                        }
                        else
                        { 
                            // updating a non-existent connection
                            so.ConnectionStopped();
                            so.PacketProcessingStopped();
                            return false;
                        }

                        // Extracted data should not go to the filter.
                        // No DB Request should be made from this point.
                        return false;

                    case 4: // disconnect wcs
                        if (socketMap.TryGetValue(realID, out eci))
                        {
                            /*
                            // remove from map
                            if (socketMap.TryRemove(realID, out eci))
                            {
                                Console.WriteLine("Removed " + realID);
                            }
                             */

                            Console.WriteLine("Logger: " + ed.HouseholdID);
                            socketMap[ed.HouseholdID].WasRegistered = false;
                            socketMap[ed.HouseholdID].TimeRemaining = 10;
                            so.UpdateTable(ed.HouseholdID, 10);
                        }
                        else
                        {
                            // Extracted data should not go to the filter.
                            // No DB Request should be made from this point.
                            return false;
                        }

						break;

                    case 1: // insert water usage
                        if (!socketMap.TryGetValue(realID, out eci))
                        {
                            // updating a non-existent connection
                            so.ConnectionStopped();
                            so.PacketProcessingStopped();

                            // Extracted data should not go to the filter.
                            // No DB Request should be made from this point.
                            return false;
                        }
                        else
                        {
                            // reset timer
                            so.TimerSet();
                            
                            EncryptedConnectionInfo newEci = eci;
                            newEci.TimeRemaining = startTime;

                            so.UpdateTable(realID, startTime);

                            if (!socketMap.TryUpdate(realID, newEci, eci))
                                return false;
                        }

                        break;
                    case 2: // update water balance
                        if (!socketMap.TryGetValue(realID, out eci))
                        {
                            // updating a non-existent connection
                            so.ConnectionStopped();
                            so.PacketProcessingStopped();

                            // Extracted data should not go to the filter.
                            // No DB Request should be made from this point.
                            return false;
                        }
                        else
                        {
                            // check if water is used
                            int tmpVolume = Int32.Parse(ed.Data);
                            int tmpBalance = tmpVolume * 20; // this is the new water equivalent

                            if (tmpBalance > 9999) tmpBalance = 9999;

                            if (socketMap[ed.HouseholdID].Balance > tmpBalance)
                            {
                                // calculate difference
                                int difference = socketMap[ed.HouseholdID].WaterEquivalent - tmpVolume;

                                // calculate equivalent water usage in liters
                                int waterUsage = difference;
                                Console.WriteLine("difference: " + difference);
                                Console.WriteLine("Water Usage: " + waterUsage);

                                // convert to byte
                                byte[] liters_byte = Encoding.ASCII.GetBytes(waterUsage.ToString("D4"));

                                // create packet
                                byte[] header = { (byte)'4', (byte)'p', (byte)'?', (byte)'2' };
                                byte[] id = ed.EncryptedID;
                                byte[] packet = PacketCreator.FormWritePacketWCS(header, id, liters_byte, 1);
                                EncryptedPacket waterUsagePacket = new EncryptedPacket(packet);

                                // enqueue to received packet list
                                receivedPackets.Enqueue(waterUsagePacket);
                            }

                            // update extracted data
                            ed.Data = tmpBalance.ToString("D4");

                            // reset timer
                            so.TimerSet();

                            if(!socketMap[realID].WasRegistered)
                            {
                                // register
                                byte[] header2 = { (byte)'4', (byte)'p', (byte)'?', (byte)'2' };
                                byte[] id2 = ed.EncryptedID;
                                byte[] packet2 = PacketCreator.FormReadPacketWCS(header2, id2, 3);
                                EncryptedPacket regPacket = new EncryptedPacket(packet2);

                                // enqueue to received packet list
                                receivedPackets.Enqueue(regPacket);
                            }

                            // update value in dictionary
                            socketMap[ed.HouseholdID].Balance = tmpBalance;
                            socketMap[ed.HouseholdID].TimeRemaining = startTime;

                            so.UpdateTable(realID, startTime);
                            Console.WriteLine("Test");
                        }

						break;
                    case 5: // retrieve water balance
                    case 8: // retrieve shutdown value
                    case 9: // retrieve store
                        if (!socketMap.TryGetValue(realID, out eci))
                        {
                            // updating a non-existent connection
                            so.ConnectionStopped();
                            so.PacketProcessingStopped();

                            // Extracted data should not go to the filter.
                            // No DB Request should be made from this point.
                            return false;
                        }
                        else
                        {
                            // reset timer
                            so.TimerSet();

                            // reset time in the dictionary
                            EncryptedConnectionInfo newEci = eci;
                            newEci.TimeRemaining = startTime;

                            if (!socketMap.TryUpdate(realID, newEci, eci))
                                return false;
                        }

						break;

                    case 3: // update connection status
                        {
                            if (!socketMap.TryGetValue(realID, out eci))
                            {
                                // connection will be stopped
                                so.ConnectionStopped();
                                so.PacketProcessingStopped();
                                return false;

                                // calculations may be inserted here for the true household id
                                //socketMap.SetElement(ed.HouseholdID, realID, startTime, so);
                                
                                // add to dictionary
                                //if (!socketMap.TryAdd(realID, new EncryptedConnectionInfo(ed.HouseholdID, startTime)))
                                //    return false;

                                //so.AddToTable(realID, startTime);
                            }
                            else
                            {
                                so.TimerSet();
                                
                                socketMap[realID].TimeRemaining = startTime;
                                so.UpdateTable(realID, startTime);
                                //socketMap.UpdateElement(realID, startTime, so);

                                if (socketMap[realID].WasRegistered == true)
                                    return false;
                                else
                                    socketMap[realID].WasRegistered = true;
                            }

                            break;
                        }

                    default:
                        so.ConnectionStopped();
                        so.PacketProcessingStopped();
                        return false;
                }

                so.LoggingSuccess();
                
                return true;
            }

            so.LoggingFailed();
            
            return false;
        }
    }
}