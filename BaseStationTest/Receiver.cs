using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using SPRITE.Utility.Concurrency;

namespace BaseStationTest
{
    public delegate void ReceiverEvent(String packet);

    public class Receiver
    {
        private SerialPort comport;
        private bool isReady;
        private View v;

        public event ReceiverEvent ReceiveEvent;

        public bool IsReady
        {
            get { return this.isReady; }
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Shortened and error checking removed for brevity...
            if (!comport.IsOpen) return;
            isReady = false;

            int bytes = comport.BytesToRead;

            //byte[] buffer = new byte[bytes];
            //comport.Read(buffer, 0, bytes);
            String buffer = comport.ReadLine();

            if (ReceiveEvent != null)
                ReceiveEvent(buffer);

            isReady = true;
        }

        public String PortName
        {
            get { return this.comport.PortName; }
            set { this.comport.PortName = value; }
        }

        public int BaudRate
        {
            get { return this.comport.BaudRate; }
            set { this.comport.BaudRate = value; }
        }

        public Parity Parity
        {
            get { return this.comport.Parity; }
            set { this.comport.Parity = value; }
        }

        public int DataBits
        {
            get { return this.comport.DataBits; }
            set { this.comport.DataBits = value; }
        }

        public StopBits StopBits
        {
            get { return this.comport.StopBits; }
            set { this.comport.StopBits = value; }
        }

        public Receiver(View v)
        {
            this.v = v;

            ReceiveEvent = null;
            isReady = true;
            
            comport = new SerialPort();
            comport.Encoding = Encoding.GetEncoding("Latin1");
            comport.NewLine = "E";
            comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        public bool Open()
        {
            try
            {
                comport.Open();
                return true;
            }
            catch (Exception)
            {
                v.AppendToConsole("System", "COM port usage denied.");
                return false;
            }        
        }

        public bool Close()
        {
            try
            {
                comport.Close();
                return true;
            }
            catch (Exception)
            {
                v.AppendToConsole("System", "Cannot close COM port.");
                return false;
            }
        }

        public bool IsOpen
        {
            get { return this.comport.IsOpen; }
        }

        public void Write(byte[] message)
        {
            if (!comport.IsOpen)
            {
                v.AppendToConsole("System", "COM port has not been opened.");
                return;
            }

            comport.Write(message, 0, message.Length);
        }
    }
}
