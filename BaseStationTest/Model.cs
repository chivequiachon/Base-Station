using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;
using SPRITE.Utility.Types;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SPRITE.Utility.Validation;
using SPRITE.Utility.Concurrency;
using SPRITE.Utility.Communication;
using SPRITE.Utility.Concurrency.Containers;
using System.Timers;

namespace BaseStationTest
{
    public class Model
    {
        private ConcurrentThreadPool t;
        private ActiveQueue<EncryptedPacket> receivedPackets;
        private ActiveQueue<FilteredPacket> filteredPackets;
        private ConcurrentQueue<byte[]> responseQueue;
        
        private System.Collections.Concurrent.ConcurrentQueue<byte[]> packetsToWCS;
        private System.Collections.Concurrent.ConcurrentDictionary<String, EncryptedConnectionInfo> connectionMap;

        private ChecksumModule primaryChecksum;
        private ChecksumModule alternateChecksum;

        private AsyncClient dbServerClient;
        private StatusObserver so;
        private BreakdownTimer timer;

        // packet processors
        private Receiver receiver;

        // observer
        private View v;
        private int state;

        // Model halt
        private bool halt;
        private bool[] haltThreadSet;

        #region accessors
        public StatusObserver StatusObserver
        {
            get { return this.so; }
        }

        public View Observer
        {
            get { return this.v; }
        }

        public int State
        {
            get { return this.state; }
        }

        public bool WasHalted
        {
            get { return this.halt; }
        }
        #endregion

        public void AddObserver(View v)
        {
            this.v = v;
        }

        #region threadStopHelpers
        private bool HaltComplete()
        {
            bool result = true;
            foreach (bool b in haltThreadSet)
            {
                if (b == false)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        private void DeactivationReset()
        {
            for (int i = 0; i < haltThreadSet.Length; i++)
                haltThreadSet[i] = false;
        }
        #endregion

        #region initializers
        public Model()
        {
            // may force garbage collection in this section if stopped
            state = 0; // not running
            dbServerClient = null;

            haltThreadSet = new bool[6] {false, false, false, false, false, false};

            t = ConcurrentThreadPool.GetInstance(7);
            primaryChecksum = Ordinary.GetInstance();
            alternateChecksum = Fletcher.GetInstance();

            receivedPackets = new ActiveQueue<EncryptedPacket>("packetReceiver");
            filteredPackets = new ActiveQueue<FilteredPacket>("packetFilter");
            responseQueue = new ConcurrentQueue<byte[]>();
            packetsToWCS = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();
            connectionMap = new System.Collections.Concurrent.ConcurrentDictionary<String, EncryptedConnectionInfo>();
            so = new StatusObserver(this);

            timer = new BreakdownTimer(500, 1 * 60 * 1000);

            timer.DecrementEvent = (Object source, ElapsedEventArgs e) =>
            {
                // decrement time
                EncryptedConnectionInfo eci = null;
                if (connectionMap.TryGetValue(timer.GlobalKey, out eci))
                {
                    if (connectionMap[timer.GlobalKey].TimeRemaining > 0)
                    {
                        connectionMap[timer.GlobalKey].TimeRemaining = --connectionMap[timer.GlobalKey].TimeRemaining;
                        so.UpdateTable(timer.GlobalKey, connectionMap[timer.GlobalKey].TimeRemaining);
                    }
                }
            };

            // for breakdown detection
            timer.TimerEvent = (Object source, ElapsedEventArgs e) =>
            {
                // last hour + 2 == DateTime.Now.Hour
                //if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
                //{
                    timer.LastHour = DateTime.Now.Hour;

                    // send first first-time connection permission packet
                    byte[] header = { (byte)'4', (byte)'p', (byte)'?', (byte)'2' };

                    if (connectionMap.Count > 0)
                    {
                        // send in check packets for each connection existent in the dictionary
                        // and collect those ids whom did not reply to the check packets
                        Queue<KeyValuePair<String, EncryptedConnectionInfo>> disconnectedIDs =
                            new Queue<KeyValuePair<String, EncryptedConnectionInfo>>();

                        t.StopThread("ResponseBackgrounder");
                        t.StopThread("DBTThread");

                        // iterate through the dictionary
                        foreach (var item in connectionMap)
                        {
                            String str_encryptedID = item.Value.EncryptedID;
                            byte[] encryptedID = {(byte)str_encryptedID[0], (byte)str_encryptedID[1],
                                                      (byte)str_encryptedID[2], (byte)str_encryptedID[3]};
                            byte[] permissionPacket = PacketCreator.FormReadPacketWCS(header, encryptedID, 10);

                            timer.GlobalKey = item.Key;

                            receiver.Write(permissionPacket);
                            Console.WriteLine(Encoding.ASCII.GetString(permissionPacket));

                            timer.Reset();
                            timer.EnableDecrementEvent = true;
                            Boolean res = timer.WaitOne(7000);
                            timer.EnableDecrementEvent = false;

                            
                            if (!res)
                            {
                                timer.Reset();
                                disconnectedIDs.Enqueue(item);
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }

                        t.StartThread("DBTThread");
                        t.StartThread("ResponseBackgrounder");

                        // delete all disconnected wcs information
                        while (disconnectedIDs.Count > 0)
                        {
                            // dequeue from queue
                            KeyValuePair<String, EncryptedConnectionInfo> item = disconnectedIDs.Dequeue();

                            String str_encryptedID = item.Value.EncryptedID;
                            byte[] encryptedID =
                            {
                                (byte)str_encryptedID[0],
                                (byte)str_encryptedID[1],
                                (byte)str_encryptedID[2],
                                (byte)str_encryptedID[3]
                            }
                            ;
                            byte[] disconnectionPacket = PacketCreator.FormReadPacketWCS(header, encryptedID, 4);

                            //so.UpdateTable(item.Key, 10);
                            //connectionMap[item.Key].TimeRemaining = 10;
                            receivedPackets.Enqueue(new EncryptedPacket(disconnectionPacket));

                            // erase from dictionary
                            //so.RemoveFromTable(item.Key);
                        }
                    }
                //}
            };
        }

        public void InitializeComponents()
        {
            receiver = new Receiver(this.v);
        }
        #endregion

        #region MethodsForTheController
        public String[] DetectComPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Exit()
        {
            t.KillAll();

            if (dbServerClient != null)
            {
                dbServerClient.Disconnect(1000);
            }

            // loading takes place here...
        }

        public void Resume()
        {
            state = 1;
            v.UpdateButtons();
            so.SystemResumed();
            t.StartAll();
        }

        public void Invoke(byte a, byte b, byte c, byte d, String comPort, String dbHost, int dbPort)
        {
            halt = false;
            DeactivationReset();

            v.DisableButtons();

            // initialize db server client
            dbServerClient = new AsyncClient(dbHost, dbPort);

            if (dbServerClient.Connect(0))
            {
                // initialize and set packet processors
                receiver.PortName = comPort;
                receiver.BaudRate = 38400;
                receiver.Parity = Parity.None;
                receiver.DataBits = 8;
                receiver.StopBits = StopBits.One;
                receiver.ReceiveEvent += new ReceiverEvent(
                    (String packet) =>
                    {
                        timer.Set();

                        //List<byte> l = new List<byte>();

                        //foreach (byte byteEl in packet)
                        //l.Add(byteEl);

                        //l.Add((byte)'\0');

                        //Console.WriteLine("RECEIVER: " + Encoding.ASCII.GetString(l.ToArray()));
                        packet += "\0";
                        Console.WriteLine("RECEIVER: " + packet);
                        Console.WriteLine("RECEIVER CHECK: " + Encoding.GetEncoding("Latin1").GetBytes(packet)[packet.Length-2]);
                        so.PacketReceived();

                        //receivedPackets.Enqueue(new EncryptedPacket(l.ToArray()));
                        receivedPackets.Enqueue(new EncryptedPacket(Encoding.GetEncoding("Latin1").GetBytes(packet)));
                    }
                );

                if (!receiver.Open())
                {
                    v.EnableButtons();
                    return;
                }

                byte[] id = { a, b, c, d };
                byte[] header = { (byte)'4', (byte)'p', (byte)'?', (byte)'2' };
                byte[] sendBuffer = PacketCreator.FormReadPacket(header, id, 1, alternateChecksum);

                // send authentication to db server
                while (!dbServerClient.Send(sendBuffer, 0))
                    Thread.Sleep(500);

                // reconstruct dictionary
                while (true)
                {
                    if (dbServerClient.Receive(18, (byte)'\0', 0))
                    {
                        byte[] buffer2 = dbServerClient.DequeueMessage();
                        if (buffer2[0] == '^') break;

                        // store in to a string buffer
                        String info = Encoding.ASCII.GetString(buffer2).Trim('\0');

                        // find dash
                        int dashPos = info.IndexOf('-');

                        // extract data
                        String encryptedID = info.Substring(0, 4).Trim();
                        String realID = info.Substring(4, dashPos - 4).Trim();
                        String balance = info.Substring(dashPos + 1).Trim();
                        int waterEquivalent = Int32.Parse(balance) / 20;

                        // add to dictionary
                        EncryptedConnectionInfo eci = new EncryptedConnectionInfo(encryptedID, Int32.Parse(balance), waterEquivalent, 10);

                        if (connectionMap.TryAdd(realID, eci))
                        {
                            // ! write to console
                            Console.WriteLine("Encrypted ID: " + encryptedID);
                            Console.WriteLine("Real ID: " + realID);
                            Console.WriteLine("Balance: " + balance);
                            Console.WriteLine("Water Equivalent: " + eci.WaterEquivalent);

                            so.AddToTable(realID, 10);
                        }
                    }
                }

                if (!t.IsActivated) t.ActivateAll();

                Console.WriteLine("Base Station has connected successfully");

                state = 1;
                v.UpdateButtons();

                so.InitSuccessful();
                so.SystemStarting();
                so.ServerStarted();

                Run();
                return;
            }

            Console.WriteLine(123);
            v.EnableButtons();
        }

        public void Stop()
        {
            if (!receiver.Close()) return;

            v.DisableButtons();

            halt = true;
            while (!HaltComplete()) { }

            t.DeactivateAll();

            if (dbServerClient != null)
            {
                dbServerClient.Disconnect(1000);
                dbServerClient = null;
            }
            
            // loading takes place here...
            while (!t.HasNoTask) { }

            state = 0;
            v.UpdateButtons();
        }

        public void Pause()
        {
            t.PauseAll();
            so.SystemPaused();

            state = 2;
            v.UpdateButtons();
        }

        #endregion
        
        public void Run()
        {
            receivedPackets.Run();
            filteredPackets.Run();

            timer.EnableTimerEvent = true;

            // thread for wcs server
            t.AddTask(
                new CustomizedTask(
                    "ValidatorThread",
                    () =>
                    {
                        if (halt)
                        {
                            haltThreadSet[0] = true;
                            return;
                        }

                        Thread.Sleep(1000);
                        
                        ActiveQueue<EncryptedPacket>.FutureValue fv = receivedPackets.Dequeue();
                        
                        if (fv != null)
                        {
                            EncryptedPacket ep = fv.GetPromisedValue(0, null);
                            if(ep != null)
                            {
                                EncryptedPacket validateOut = null;
                                if (!Validator.Validate(ref so, ref ep, primaryChecksum, out validateOut)) return;

                                ExtractedData extractOut = null;
                                if (!Extractor.Extract(ref so, ref validateOut, out extractOut)) return;

                                if (!Logger.Log(ref connectionMap, ref receivedPackets, ref so, ref extractOut, 10)) return;

                                FilteredPacket filterOut = null;
                                if (!Filtrator.Filter(ref so, ref extractOut, ref alternateChecksum, out filterOut)) return;
                            
                                if (filterOut != null)
                                    filteredPackets.Enqueue(filterOut);
                            }
                        }
                    },
                    true,
                    false
                )
            );

            // thread for db request transmitter
            t.AddTask(
                new CustomizedTask(
                    "DBTThread",
                    () =>
                    {
                        if (halt)
                        {
                            haltThreadSet[1] = true;
                            return;
                        }

                        Thread.Sleep(1000);
                        
                        // Transmission of request to db server will be done here...
                        ActiveQueue<FilteredPacket>.FutureValue fv = filteredPackets.Dequeue();

                        if (fv != null)
                        {
                            FilteredPacket fp = fv.GetPromisedValue(0, null);

                            if (fp != null)
                            {
                                // transmit request to db server
                                so.SentToBS();

                                if (dbServerClient != null)
                                    dbServerClient.Send(fp.Packet, 0);
                            }
                        }
                    },
                    true,
                    false
                )
            );

            // thread for receiving responses from the db server
            t.AddTask(
                new CustomizedTask(
                    "DBRThread",
                    () =>
                    {
                        if (halt)
                        {
                            haltThreadSet[2] = true;
                            return;
                        }

                        if (dbServerClient != null)
                        {
                            dbServerClient.Receive(22, (byte)'\0', 0);
                        }

                        Thread.Sleep(500);
                    },
                    true,
                    false
                )
            );
            
            // thread for managing db server responses
            t.AddTask(
                new CustomizedTask(
                    "ResponseBackgrounder",
                    () =>
                    {
                        if (halt)
                        {
                            haltThreadSet[3] = true;
                            return;
                        }
                        
                        Thread.Sleep(1000);

                        // Receive messages from db server will be done here...
                        // Transmission of response from db server to serial port will be done here...
                        if (dbServerClient != null)
                        {
                            // dequeue byte array and transmit to serial port
                            byte[] byte_buffer = dbServerClient.DequeueMessage();
                            if (byte_buffer != null)
                            {
                                String buffer = Encoding.ASCII.GetString(byte_buffer).Trim('\0');

                                if (buffer != null && buffer.Length > 0)
                                {
                                    buffer = buffer.Remove(1, 8);

                                    //receiver.Write(buffer);
                                    Console.WriteLine("Response: " + buffer);

                                    if (buffer[0] == '!' || buffer[0] == '?') // shutdown update & cash store update
                                    {
                                        // disable timer
                                        //timer.EnableDecrementEvent = false;
                                        //timer.EnableTimerEvent = false;

                                        // get wcs real id
                                        String wcsID = buffer.Substring(1, 8).Trim('-');

                                        // search encrypted id in connection map using real id
                                        EncryptedConnectionInfo eci;

                                        // get encrypted id from map
                                        if (connectionMap.TryGetValue(wcsID, out eci))
                                        {
                                            String str_encryptedID = eci.EncryptedID;

                                            if (str_encryptedID != "NULL")
                                            {
                                                t.StopThread("DBTThread");

                                                // create header
                                                byte[] header = { (byte)'4', (byte)'p', (byte)'?', (byte)'2' };
                                                byte[] encryptedID = new byte[]
                                                {
                                                    (byte)str_encryptedID[0],
                                                    (byte)str_encryptedID[1],
                                                    (byte)str_encryptedID[2],
                                                    (byte)str_encryptedID[3]
                                                };

                                                switch (buffer[0])
                                                {
                                                    case '!': // update shutdown
                                                        // packet format: !<wcs real id>
                                                        // create packet to be sent to the base station
                                                        byte[] updatePacket1 =
                                                            PacketCreator.FormReadPacketWCS(header, encryptedID, 11);

                                                        //packetsToWCS.Enqueue(updatePacket1);
                                                        Console.WriteLine("update shutdown: " + Encoding.ASCII.GetString(updatePacket1));

                                                        // no need to wait for an acknowledgment
                                                        break;
                                                    case '?': // update cash store
                                                        {
                                                            // packet format: ?<wcs real id><data>

                                                            // extract data
                                                            String data = buffer.Substring(9).Trim('-').Trim('\0');

                                                            // create conversion
                                                            int balance = Int32.Parse(data);
                                                            int waterEquivalent = balance / 20;

                                                            byte[] message = Encoding.ASCII.GetBytes(waterEquivalent.ToString("D4"));
                                                            Console.WriteLine("Water Equivalent: " + waterEquivalent);

                                                            // create packet to be sent to the base station
                                                            byte[] updatePacket2 =
                                                                PacketCreator.FormWritePacketWCS(header, encryptedID, message, 12);

                                                            // needs to wait for an acknowledgment
                                                            //do
                                                            //{
                                                                // send to wcs
                                                                packetsToWCS.Enqueue(updatePacket2);
                                                                Console.WriteLine("update cash store: " + Encoding.ASCII.GetString(updatePacket2));
                                                                timer.WaitOne(200);
                                                            //}
                                                            //while(!timer.WaitOne(200));

                                                            break;
                                                        }
                                                    case '&': // a new household was registered
                                                        // packet format: &<wcs real id>
                                                        // add to connection map
                                                       /* if (connectionMap.TryAdd(wcsID, new EncryptedConnectionInfo(str_encryptedID, 10)))
                                                            so.AddToTable(wcsID, 10);*/

                                                        break;
                                                }

                                                t.StartThread("DBTThread");
                                            }
                                        }

                                        //timer.EnableTimerEvent = true;
                                    }
                                }
                            }
                        }
                    },
                    true,
                    false
                )
            );

            // scheduled thread for sending responses to port
            t.AddTask(
                new CustomizedTask(
                    "DBTThread",
                    () =>
                    {
                        if (halt)
                        {
                            haltThreadSet[4] = true;
                            return;
                        }

                        Thread.Sleep(1000);
                        
                        //so.ShutdownChecked();
                        
                        byte[] packet = null;
                        if (packetsToWCS.TryDequeue(out packet))
                        {
                            if (packet != null)
                            {
                                // send to wcs
                                receiver.Write(packet);
                                Console.WriteLine(Encoding.ASCII.GetString(packet));
                            }
                        }
                    },
                    true,
                    false
                )
            );
        }
    }
}
