using System;

namespace BaseStationTest
{
    public class EncryptedConnectionInfo
    {
        private String encryptedID;
        private int timeRemaining;
        private bool wasRegistered;
        private int waterEquivalent;
        private int balance;

        public EncryptedConnectionInfo()
        {
            this.encryptedID = null;
            this.timeRemaining = 0;
            this.wasRegistered = false;
            this.waterEquivalent = 0;
        }

        public EncryptedConnectionInfo(String encryptedID, int balance, int timeRemaining)
        {
            this.encryptedID = encryptedID;
            this.timeRemaining = timeRemaining;
            this.wasRegistered = false;
            this.balance = balance;
            this.waterEquivalent = 0;
        }

        public EncryptedConnectionInfo(String encryptedID, int balance, int waterEquivalent, int timeRemaining)
        {
            this.encryptedID = encryptedID;
            this.timeRemaining = timeRemaining;
            this.wasRegistered = false;
            this.balance = balance;
            this.waterEquivalent = waterEquivalent;
        }

        public int Balance
        {
            get { return this.balance; }
            set { this.balance = value; }
        }

        public bool WasRegistered
        {
            get { return this.wasRegistered; }
            set { this.wasRegistered = value; }
        }

        public int WaterEquivalent
        {
            get { return this.waterEquivalent; }
            set { this.waterEquivalent = value; }
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
    };
}
