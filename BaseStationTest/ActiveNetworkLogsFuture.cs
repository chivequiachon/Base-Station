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
        public class FutureValue
        {
            private ActiveNetworkLogs thisMap;
            private EncryptedConnectionInfo promisedValue;
            private String toBeErased;
            private String disconnectedElement;
            private List<byte[]> shutdownCheckPackets;
            private List<byte[]> storeUpdatePackets;

            private byte[] shutdownCheckPacket;

            private bool isReady;
            private bool isReadyKey;
            private bool isReadyShutdownCheckPackets;
            private bool isReadySingleShutdownCheckPacket;
            private bool isReadyStoreUpdatePackets;
            private bool isReadyDisconnectedElement;

            public FutureValue(ActiveNetworkLogs thisMap)
            {
                this.thisMap = thisMap;
                this.promisedValue = null;
                
                this.isReady = false;
                this.isReadyKey = false;
                this.isReadyShutdownCheckPackets = false;
                this.isReadyStoreUpdatePackets = false;
                this.isReadyDisconnectedElement = false;
                this.isReadySingleShutdownCheckPacket = false;

                this.toBeErased = null;
                this.disconnectedElement = null;
                this.shutdownCheckPacket = null;
            }

            public bool IsReady
            {
                set { this.isReady = value; }
                get { return this.isReady; }
            }

            public bool IsReadyKey
            {
                set { this.isReadyKey = value; }
                get { return this.isReadyKey; }
            }

            public bool IsReadyShutdownCheckPackets
            {
                set { this.isReadyShutdownCheckPackets = value; }
                get { return this.isReadyShutdownCheckPackets; }
            }

            public bool IsReadySingleShutdownCheckPacket
            {
                set { this.isReadySingleShutdownCheckPacket = value; }
                get { return this.isReadySingleShutdownCheckPacket; }
            }

            public bool IsReadyStoreUpdatePackets
            {
                set { this.isReadyStoreUpdatePackets = value; }
                get { return this.isReadyStoreUpdatePackets; }
            }

            public bool IsReadyDisconnectedElement
            {
                set { this.isReadyDisconnectedElement = value; }
                get { return this.isReadyDisconnectedElement; }
            }

            public void Ready(EncryptedConnectionInfo element)
            {
                this.promisedValue = element;
                this.isReady = true;
                this.thisMap.blocker.Set();
            }

            public void Ready(String key)
            {
                this.toBeErased = key;
                this.isReadyKey = true;
                thisMap.blocker2.Set();
            }

            public void ReadyShutdownCheckPackets(List<byte[]> shutdownPackets)
            {
                this.shutdownCheckPackets = shutdownPackets;
                this.isReadyShutdownCheckPackets = true;
                this.thisMap.blocker3.Set();
            }

            public void ReadyStoreUpdatePackets(List<byte[]> storeUpdatePackets)
            {
                this.storeUpdatePackets = storeUpdatePackets;
                this.isReadyStoreUpdatePackets = true;
                this.thisMap.blocker4.Set();
            }

            public void ReadyDisconnectedElement(String key)
            {
                this.disconnectedElement = key;
                this.isReadyDisconnectedElement = true;
                this.thisMap.blocker5.Set();
            }

            public void ReadySingleShutdownCheckPacket(byte[] shutdownPacket)
            {
                this.shutdownCheckPacket = shutdownPacket;
                this.isReadyShutdownCheckPackets = true;
                //this.thisMap.blocker6.Set();
            }

            public EncryptedConnectionInfo GetPromisedValue(int timeout, EncryptedConnectionInfo defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker.WaitOne(timeout);

                    if (flag)
                    {
                        isReady = false;
                        return this.promisedValue;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker.WaitOne();
                isReady = false;
                return this.promisedValue;
            }

            public String GetPromisedDisconnectedElement(int timeout, String defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker5.WaitOne(timeout);

                    if (flag)
                    {
                        isReadyDisconnectedElement = false;
                        return this.disconnectedElement;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker5.WaitOne();
                isReadyDisconnectedElement = false;
                return this.disconnectedElement;        
            }

            public String GetPromisedKey(int timeout, String defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker2.WaitOne(timeout);

                    if (flag)
                    {
                        isReadyKey = false;
                        return this.toBeErased;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker2.WaitOne();
                isReadyKey = false;
                return this.toBeErased;
            }

            public List<byte[]> GetPromisedShutdownCheckPackets(int timeout, List<byte[]> defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker3.WaitOne(timeout);

                    if (flag)
                    {
                        isReadyShutdownCheckPackets = false;
                        return this.shutdownCheckPackets;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker3.WaitOne();
                isReadyShutdownCheckPackets = false;
                return this.shutdownCheckPackets;
            }

            public List<byte[]> GetPromisedStoreUpdatePackets(int timeout, List<byte[]> defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker4.WaitOne(timeout);

                    if (flag)
                    {
                        isReadyShutdownCheckPackets = false;
                        return this.shutdownCheckPackets;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker4.WaitOne();
                isReadyStoreUpdatePackets = false;
                return this.storeUpdatePackets;
            }
            
            public byte[] GetPromisedSingleShutdownCheckPacket(int timeout, byte[] defValue)
            {
                if (timeout > 0)
                {
                    bool flag = thisMap.blocker6.WaitOne(timeout);

                    if (flag)
                    {
                        isReadyShutdownCheckPackets = false;
                        return this.shutdownCheckPacket;
                    }
                    else
                    {
                        return defValue;
                    }
                }

                thisMap.blocker6.WaitOne();
                isReadyShutdownCheckPackets = false;
                return this.shutdownCheckPacket;
            }
        }

        public FutureValue GetDisconnectedElement(StatusObserver so)
        {
            /// enqueue to activation list
            if (future.IsReadyDisconnectedElement)
            {
                future.IsReadyDisconnectedElement = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker5.Reset();

                IMethodRequest disconnectElementRequest = new MR_CheckElementStatus(this, so);
                scheduler.Enqueue(disconnectElementRequest);

                return future;
            }

            return null;        
        }

        public FutureValue GetElement(String key)
        {
            /// enqueue to activation list
            if (future.IsReady)
            {
                future.IsReady = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker.Reset();

                IMethodRequest getRequest = new MR_Get(this, key);
                scheduler.Enqueue(getRequest);

                return future;
            }

            return null;
        }

        public FutureValue UpdateElements(StatusObserver so)
        {
            /// enqueue to activation list
            if (future.IsReadyKey)
            {
                future.IsReadyKey = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker2.Reset();

                IMethodRequest updateRequest = new MR_Update(this, so);
                scheduler.Enqueue(updateRequest);

                return future;
            }

            return null;
        }


        public FutureValue UpdateShutdown(ChecksumModule cm)
        {
            /// enqueue to activation list
            if (future.IsReadyShutdownCheckPackets)
            {
                future.IsReadyShutdownCheckPackets = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker3.Reset();

                IMethodRequest updateRequest = new MR_UpdateShutdown(this, cm);
                scheduler.Enqueue(updateRequest);

                return future;
            }

            return null;
        }

        public FutureValue UpdateShutdownSingle(String id, ChecksumModule cm)
        {
            /// enqueue to activation list
            if (future.IsReadySingleShutdownCheckPacket)
            {
                future.IsReadySingleShutdownCheckPacket = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker6.Reset();

                IMethodRequest updateRequest = new MR_UpdateShutdownSingle(this, id, cm);
                scheduler.Enqueue(updateRequest);

                return future;
            }

            return null;
        }

        public FutureValue UpdateStore(ChecksumModule cm)
        {
            /// enqueue to activation list
            if (future.IsReadyShutdownCheckPackets)
            {
                future.IsReadyStoreUpdatePackets = false;
                return future;
            }

            if (container.Count != 0)
            {
                blocker4.Reset();

                IMethodRequest updateStoreRequest = new MR_UpdateStore(this, cm);
                scheduler.Enqueue(updateStoreRequest);

                return future;
            }

            return null;
        }
    }
}
