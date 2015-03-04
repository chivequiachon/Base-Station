using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPRITE.Utility.Concurrency.Containers;
using SPRITE.Utility.Validation;

namespace BaseStationTest
{
    public class EncryptedConnectionInfo
    {
        private String encryptedID;
        private int timeRemaining;
        private bool shutdownCheckAck;
        private int cashStoreUpdateAck;

        public EncryptedConnectionInfo()
        {
            this.encryptedID = null;
            this.timeRemaining = 0;
            this.shutdownCheckAck = false;
            this.cashStoreUpdateAck = 0;
        }

        public String EncryptedID
        {
            get { return this.encryptedID; }
            set { this.encryptedID = value; }
        }

        public int TimeRemaining
        {
            get { return this.timeRemaining; }
            set { this.timeRemaining = value; }
        }

        public bool ShutdownCheckAck
        {
            get { return this.shutdownCheckAck; }
            set { this.shutdownCheckAck = value; }
        }

        public int CashStoreUpdateAck
        {
            get { return this.cashStoreUpdateAck; }
            set { this.cashStoreUpdateAck = value; }
        }
    };

    public partial class ActiveNetworkLogs
    {
        // real id, Encrypted Connection Information
        private Dictionary<String, EncryptedConnectionInfo> container;
        private Scheduler scheduler;
        private ManualResetEvent blocker;
        private ManualResetEvent blocker2;
        private ManualResetEvent blocker3;
        private ManualResetEvent blocker4;
        private ManualResetEvent blocker5;
        private ManualResetEvent blocker6;
        private FutureValue future;

        private int iterator;

        public ActiveNetworkLogs(String threadName)
        {
            container = new Dictionary<String, EncryptedConnectionInfo>();
            future = new FutureValue(this);
            blocker = new ManualResetEvent(false);
            blocker2 = new ManualResetEvent(false);
            blocker3 = new ManualResetEvent(false);
            blocker4 = new ManualResetEvent(false);
            blocker5 = new ManualResetEvent(false);
            blocker6 = new ManualResetEvent(false);
            scheduler = new Scheduler(threadName);

            iterator = 0;
        }

        public void Run()
        {
            scheduler.Run();
        }

        public bool SetElement(String encryptedID, String id, int timeRemaining, StatusObserver so)
        {
            if (encryptedID != null && id != null)
            {
                /// enqueue to activation list
                IMethodRequest setRequest = new MR_Set(this, encryptedID, id, timeRemaining, so);
                scheduler.Enqueue(setRequest);
                return true;
            }

            return false;
        }

        public bool UpdateElement(String id, int timeRemaining, StatusObserver so)
        {
            if (id != null)
            {
                /// enqueue to activation list
                IMethodRequest updateRequest = new MR_UpdateElement(this, id, timeRemaining, so);
                scheduler.Enqueue(updateRequest);
                return true;
            }

            return false;
        }

        public bool UpdateShutdownCheckAck(String id/*, StatusObserver so*/)
        {
            if (id != null)
            {
                /// enqueue to activation list
                IMethodRequest updateShAckRequest = new MR_UpdateShutdownCheckAck(this, id/*, so*/);
                scheduler.Enqueue(updateShAckRequest);
                return true;
            }

            return false;
        }

        public bool UpdateStoreAck(String id/*, StatusObserver so*/)
        {
            if (id != null)
            {
                /// enqueue to activation list
                IMethodRequest updateShAckRequest = new MR_UpdateStoreAck(this, id);
                scheduler.Enqueue(updateShAckRequest);
                return true;
            }

            return false;
        }

        public bool RemoveElement(String key, StatusObserver so)
        {
            if (key != null)
            {
                /// enqueue to activation list
                IMethodRequest removeRequest = new MR_Remove(this, key, so);
                scheduler.Enqueue(removeRequest);
                return true;
            }

            return false;
        }

        public void DecrementTime(String realID, StatusObserver so)
        {
            if (realID != null && so != null)
            { 
                /// enqueue to activation list
                IMethodRequest timeDecrementRequest = new MR_TimeDecrement(this, realID, so);
                scheduler.Enqueue(timeDecrementRequest);
            }
        }

        public void DecrementTimeIterator(StatusObserver so)
        {
            if (container.Count != 0 && so != null)
            {
                /// enqueue to activation list
                var pair = container.ElementAt(iterator);
                IMethodRequest timeDecrementRequest = new MR_TimeDecrement(this, pair.Key, so);
                scheduler.Enqueue(timeDecrementRequest);
            }
        }

        public void UpdateElementIterator(int timeRemaining, StatusObserver so)
        {
            if (so != null)
            {
                /// enqueue to activation list
                var pair = container.ElementAt(iterator);
                IMethodRequest updateRequest = new MR_UpdateElement(this, pair.Key, timeRemaining, so);
                scheduler.Enqueue(updateRequest);
            }
        }

        public String GetNextEncryptedID()
        {
            if (container.Count == 0) return null;

            var pair = container.ElementAt(iterator);
            
            iterator++;

            if (container.Count <= iterator)
                iterator = 0;
            
            return pair.Value.EncryptedID;
        }

        public String GetCurrentEncryptedID()
        {
            if (container.Count == 0 || iterator >= container.Count) return null;

            var pair = container.ElementAt(iterator);
            return pair.Value.EncryptedID;
        }

        public int GetSize()
        {
            return container.Count;
        }

        public int GetTimeRemainingWithDefault(String key, int defaultValue)
        {
            EncryptedConnectionInfo eci;
            return container.TryGetValue(key, out eci) ? eci.TimeRemaining : defaultValue;
        }

        public String GetEncryptedIDWithDefault(String key, String defaultValue)
        {
            EncryptedConnectionInfo eci;
            return container.TryGetValue(key, out eci) ? eci.EncryptedID : defaultValue;
        }
   }
}
