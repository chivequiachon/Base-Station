using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStationTest
{
    public class StatusObserver
    {
        private Model m;

        public StatusObserver(Model m)
        {
            this.m = m;
        }

        // table operations
        public void AddToTable(String id, int timeRemaining)
        {
            if (!m.WasHalted)
                m.Observer.AddToTable(id, timeRemaining);
        }

        public void UpdateTable(String id, int timeRemaining)
        {
            if (!m.WasHalted)
                m.Observer.UpdateTable(id, timeRemaining);
        }

        public void RemoveFromTable(String id)
        {
            if (!m.WasHalted)
                m.Observer.RemoveFromTable(id);
        }

        // Connection Status
        public void PacketProcessing()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Processing packet...");
        }

        public void PacketProcessingStopped()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Packet processing stopped.");
        }

        public void HouseholdDisconnection(String householdID)
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Disconnecting " + householdID);
        }

        public void TimedDisconnection(String householdID)
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus(householdID + " was disconnected.");
        }

        public void PacketProcessingSuccess()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Packet processing successful.");
        }

        public void ServerStarted()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Server started.");
        }

        public void SentToBS()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConnectionStatus("Sending to base station");
        }

        // Console
        public void PacketValidation()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Validator", "Validating packet...");
        }

        public void ChecksumComparison()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Validator", "Comparing checksum...");
        }

        public void ValidationSuccess()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Validator", "Validation success.");
        }

        public void ValidationFailed()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Validator", "Validation failed.");
        }

        public void PacketExtraction()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Extracting message from validated packet.");
        }

        public void HouseholdIDExtracted(String householdID)
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Household ID: " + householdID);
        }

        public void DataExtracted(String data)
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Data: " + data);
        }

        public void OperationExtracted(int operation)
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Operation: " + operation);
        }

        public void ExtractionSuccess()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Extraction success.");
        }

        public void ExtractionFailed()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Extractor", "Extraction failed.");
        }

        public void PacketLogging()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Logger", "Logging packet...");
        }

        public void ConnectionStopped()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Logger", "Attempted connection stopped.");
        }

        public void TimerSet()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Logger", "Socket timer set.");
        }

        public void LoggingSuccess()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Logger", "Packet logging successful.");
        }

        public void LoggingFailed()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Logger", "Packet logging failed.");
        }

        public void PacketFiltering()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Filter", "Filtering packet...");
        }

        public void FiltrationSuccess()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Filter", "Fitration success.");
        }

        public void FiltrationFailed()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Filter", "Fitration failed.");
        }

        public void PacketReceived()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("Receiver", "Received a packet from base station.");
        }

        public void SystemPaused()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("System", "System paused.");
        }

        public void SystemResumed()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("System", "System resumed.");
        }

        public void InitSuccessful()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("System", "Initialization successful");
        }

        public void SystemStarting()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("System", "Starting....");
        }

        public void ShutdownChecked()
        {
            if (!m.WasHalted)
                m.Observer.AppendToConsole("System", "Checking shutdown flags...");
        }
    }
}
