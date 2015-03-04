using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPRITE.Utility.Validation;
using SPRITE.Utility.Concurrency.Containers;

namespace BaseStationTest
{
    public partial class ActiveNetworkLogs
    {
        private class MR_Set : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;

            private String encryptedID;
            private String id;
            private int timeRemaining;

            private StatusObserver so;

            public MR_Set(ActiveNetworkLogs thisMap, String encryptedID, String id, int timeRemaining, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.encryptedID = encryptedID;
                this.id = id;
                this.timeRemaining = timeRemaining;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return true;
            }

            public void Run()
            {
                EncryptedConnectionInfo eci = new EncryptedConnectionInfo();
                eci.EncryptedID = encryptedID;
                eci.TimeRemaining = timeRemaining;

                thisMap.container[id] = eci;
                so.AddToTable(id, timeRemaining);
            }
        }

        private class MR_UpdateElement : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;

            private String id;
            private int timeRemaining;

            private StatusObserver so;

            public MR_UpdateElement(ActiveNetworkLogs thisMap, String id, int timeRemaining, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.id = id;
                this.timeRemaining = timeRemaining;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                thisMap.container[id].TimeRemaining = timeRemaining;
                so.UpdateTable(id, timeRemaining);
            }
        }

        private class MR_UpdateShutdownCheckAck : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String id;

            public MR_UpdateShutdownCheckAck(ActiveNetworkLogs thisMap, String id)
            {
                this.thisMap = thisMap;
                this.id = id;
            }

            public bool IsRunnable()
            {
                EncryptedConnectionInfo eci;
                if (thisMap.container.Count > 0)
                    if (thisMap.container.TryGetValue(id, out eci))
                        return true;

                return false;
            }

            public void Run()
            {
                thisMap.container[id].ShutdownCheckAck = !thisMap.container[id].ShutdownCheckAck;
                Console.WriteLine("[" + id + "] Admin shutdown checked.");
            }
        }

        private class MR_UpdateStoreAck : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String id;

            public MR_UpdateStoreAck(ActiveNetworkLogs thisMap, String id)
            {
                this.thisMap = thisMap;
                this.id = id;
            }

            public bool IsRunnable()
            {
                EncryptedConnectionInfo eci;
                if (thisMap.container.Count > 0)
                    if (thisMap.container.TryGetValue(id, out eci))
                        return true;

                return false;
            }

            public void Run()
            {
                switch (thisMap.container[id].CashStoreUpdateAck)
                {
                    case 0:
                        Console.WriteLine("[" + id + "] Cash Store value has been retrieved.");
                        thisMap.container[id].CashStoreUpdateAck = 1;
                        break;
                    case 1:
                        Console.WriteLine("[" + id + "] Cash Store value reset.");
                        thisMap.container[id].CashStoreUpdateAck = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        private class MR_Remove : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String id;
            private StatusObserver so;

            public MR_Remove(ActiveNetworkLogs thisMap, String id, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.id = id;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                thisMap.container.Remove(id);
                so.RemoveFromTable(id);
            }
        }

        private class MR_Get : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String key;

            public MR_Get(ActiveNetworkLogs thisMap, String key)
            {
                this.thisMap = thisMap;
                this.key = key;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count != 0;
            }

            public void Run()
            {
                thisMap.future.Ready(thisMap.container[key]);
            }
        }

        private class MR_Update : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private StatusObserver so;

            public MR_Update(ActiveNetworkLogs thisMap, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                String key = null;
                for (int index = 0; index < thisMap.container.Count; index++)
                {
                    var item = thisMap.container.ElementAt(index);

                    if (item.Value.TimeRemaining <= 0)
                    {
                        key = thisMap.container[item.Key].EncryptedID;
                        thisMap.container.Remove(item.Key);
                        so.TimedDisconnection(item.Key);
                        so.RemoveFromTable(item.Key);
                        break;
                    }
                    else
                    {
                        thisMap.container[item.Key].TimeRemaining--;
                        so.UpdateTable(item.Key, thisMap.container[item.Key].TimeRemaining);
                    }
                }

                thisMap.future.Ready(key);
            }
        }

        private class MR_CheckElementStatus : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private StatusObserver so;

            public MR_CheckElementStatus(ActiveNetworkLogs thisMap, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                String key = null;
                for (int index = 0; index < thisMap.container.Count; index++)
                {
                    var item = thisMap.container.ElementAt(index);

                    if (item.Value.TimeRemaining <= 0)
                    {
                        key = thisMap.container[item.Key].EncryptedID;
                        thisMap.container.Remove(item.Key);
                        so.TimedDisconnection(item.Key);
                        so.RemoveFromTable(item.Key);
                        break;
                    }
                }

                thisMap.future.ReadyDisconnectedElement(key);
            }
        }

        private class MR_TimeDecrement : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String realID;
            private StatusObserver so;

            public MR_TimeDecrement(ActiveNetworkLogs thisMap, String realID, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.realID = realID;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                if (thisMap.container[realID].TimeRemaining > 0)
                {
                    thisMap.container[realID].TimeRemaining--;
                    so.UpdateTable(realID, thisMap.container[realID].TimeRemaining);
                }
            }
        }

        private class MR_TimeReset : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private String realID;
            private int resetVal;
            private StatusObserver so;

            public MR_TimeReset(ActiveNetworkLogs thisMap, String realID, int resetVal, StatusObserver so)
            {
                this.thisMap = thisMap;
                this.realID = realID;
                this.resetVal = resetVal;
                this.so = so;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                thisMap.container[realID].TimeRemaining = resetVal;
                so.UpdateTable(realID, thisMap.container[realID].TimeRemaining);
            }
        }

        private class MR_UpdateShutdown : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private ChecksumModule checksum;

            public MR_UpdateShutdown(ActiveNetworkLogs thisMap, ChecksumModule checksum)
            {
                this.thisMap = thisMap;
                this.checksum = checksum;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                List<byte[]> shutdownUpdatePackets = new List<byte[]>();
                byte[] header = Encoding.ASCII.GetBytes("4p?2");

                for (int index = 0; index < thisMap.container.Count; index++)
                {
                    var item = thisMap.container.ElementAt(index);

                    if (!thisMap.container[item.Key].ShutdownCheckAck)
                    {
                        byte[] id = Encoding.ASCII.GetBytes(thisMap.container[item.Key].EncryptedID);
                        //byte[] normalizedMessage = Extractor.Normalize(thisMap.container[item.Key].EncryptedID, '0');
                        shutdownUpdatePackets.Add(PacketCreator.FormReadPacket(header, id, 7, checksum));
                        thisMap.container[item.Key].ShutdownCheckAck = true;
                    }
                    else
                    {
                        // maybe disconnected
                    }
                }

                thisMap.future.ReadyShutdownCheckPackets(shutdownUpdatePackets);
            }
        }

        private class MR_UpdateShutdownSingle : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private ChecksumModule checksum;
            private String key;

            public MR_UpdateShutdownSingle(ActiveNetworkLogs thisMap, String key, ChecksumModule checksum)
            {
                this.thisMap = thisMap;
                this.checksum = checksum;
                this.key = key;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                EncryptedConnectionInfo eci;
                if (thisMap.container.TryGetValue(key, out eci))
                {
                    byte[] shutdownPacket;
                    byte[] header = Encoding.ASCII.GetBytes("4p?2");
                    byte[] id = Encoding.ASCII.GetBytes(thisMap.container[key].EncryptedID);

                    shutdownPacket = PacketCreator.FormReadPacket(header, id, 7, checksum);
                    
                    thisMap.container[key].ShutdownCheckAck = true;
                    thisMap.future.ReadySingleShutdownCheckPacket(shutdownPacket);
                }
            }
        }

        private class MR_UpdateStore : IMethodRequest
        {
            private ActiveNetworkLogs thisMap;
            private ChecksumModule checksum;

            public MR_UpdateStore(ActiveNetworkLogs thisMap, ChecksumModule checksum)
            {
                this.thisMap = thisMap;
                this.checksum = checksum;
            }

            public bool IsRunnable()
            {
                return thisMap.container.Count > 0;
            }

            public void Run()
            {
                List<byte[]> storeCheckPackets = new List<byte[]>();
                byte[] header = Encoding.ASCII.GetBytes("4p?2");

                for (int index = 0; index < thisMap.container.Count; index++)
                {
                    var item = thisMap.container.ElementAt(index);
                    byte[] id = Encoding.ASCII.GetBytes(thisMap.container[item.Key].EncryptedID);

                    switch (thisMap.container[item.Key].CashStoreUpdateAck)
                    {
                        case 0:
                            storeCheckPackets.Add(PacketCreator.FormReadPacket(header, id, 8, checksum));
                            break;
                        case 1:
                            storeCheckPackets.Add(PacketCreator.FormReadPacket(header, id, 9, checksum));
                            break;
                        default:
                            // maybe disconnected
                            break;
                    }
                }

                thisMap.future.ReadyStoreUpdatePackets(storeCheckPackets);
            }
        }
    }
}