using System;

namespace SPRITE.Utility
{
    namespace Types
    {
        public class ExtractedData
        {
            private byte[] packet;
            private byte[] encryptedID;
            private String householdID;
            private String data;
            private int requestType;

            public byte[] Packet
            {
                get { return this.packet; }
                set { this.packet = value; }
            }

            public byte[] EncryptedID
            {
                get { return this.encryptedID; }
                set { this.encryptedID = value; }
            }

            public String HouseholdID
            {
                get { return this.householdID; }
                set { this.householdID = value; }
            }

            public String Data
            {
                get { return this.data; }
                set { this.data = value; }
            }

            public int RequestType
            {
                get { return this.requestType; }
                set { this.requestType = value; }
            }

            public ExtractedData()
            {
                this.householdID = null;
                this.data = null;
                this.requestType = -1;
                this.packet = null;
                this.encryptedID = null;
            }

            public ExtractedData(byte[] packet, byte[] encryptedID, String householdID, String data, int requestType)
            {
                this.householdID = householdID;
                this.data = data;
                this.requestType = requestType;
                this.packet = packet;
                this.encryptedID = encryptedID;
            }
        }
    }
}
