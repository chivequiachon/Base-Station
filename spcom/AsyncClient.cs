using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;

namespace SPRITE.Utility
{
    namespace Communication
    {
        public class AsyncClient
        {
            private int port;
            private string host;

            private IPEndPoint remoteEP;
            private Queue<byte[]> receivedBytes;
            private Socket tcpClient;

            private AutoResetEvent sendLock;

            private object blocker1;
            private object blocker2;
            private object blocker3;
            private object blocker4;

            private class ReceiveRequirements
            {
                public AsyncCallback callback { get; set; }
                public ReceiveRequirements(AsyncCallback callback) { this.callback = callback; }
            }

            public AsyncClient(string host, int port)
            {
                this.port = port;
                this.host = host;

                IPHostEntry ipHostInfo = Dns.Resolve(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];

                this.remoteEP = new IPEndPoint(ipAddress, port);
                this.tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.receivedBytes = new Queue<byte[]>();

                this.blocker1 = new object();
                this.blocker2 = new object();
                this.blocker3 = new object();
                this.blocker4 = new object();

                this.sendLock = new AutoResetEvent(false);
            }

            public string Host
            {
                get { return this.host; }
            }

            public int Port
            {
                get { return this.port; }
            }

            public bool Connect(int timeout)
            {
                lock (blocker1)
                {
                    try
                    {
                        ManualResetEvent connectionLock = new ManualResetEvent(false);
                        Boolean errorFlag = false;
                        tcpClient.BeginConnect(this.remoteEP,
                            new AsyncCallback(
                                (IAsyncResult ar) =>
                                {
                                    try
                                    {
                                        Socket s = (Socket)ar.AsyncState;

                                        s.EndConnect(ar);
                                        connectionLock.Set();
                                    }
                                    catch (Exception e)
                                    {
                                        errorFlag = true;
                                        Console.WriteLine(e.ToString());
                                    }
                                }
                            ),
                            tcpClient
                        );

                        if (errorFlag) return false;

                        if (timeout > 0)
                        {
                            return connectionLock.WaitOne(timeout);
                        }
                        else
                        {
                            connectionLock.WaitOne();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            public bool Send(byte[] data, int timeout)
            {
                lock (blocker2)
                {
                    try
                    {
                        byte[] byteData = data;

                        tcpClient.BeginSend(byteData, 0, byteData.Length, 0,
                            new AsyncCallback(
                                (IAsyncResult ar) =>
                                {
                                    try
                                    {
                                        Socket s = (Socket)ar.AsyncState;

                                        s.EndSend(ar);
                                        sendLock.Set();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.ToString());
                                    }
                                }
                            ),
                            tcpClient
                        );

                        if (timeout > 0)
                        {
                            return sendLock.WaitOne(timeout);
                        }
                        else
                        {
                            sendLock.WaitOne();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            public bool Receive(int bufferSize, byte delimiter, int timeout)
            {
                lock (blocker3)
                {
                    try
                    {
                        byte[] buffer = new byte[bufferSize];
                        ReceiveRequirements rr = null;
                        MemoryStream ms = new MemoryStream();
                        ManualResetEvent receiveLock = new ManualResetEvent(false);

                        AsyncCallback ReceiveCallback = new AsyncCallback(
                            (IAsyncResult ar) =>
                            {
                                try
                                {
                                    ReceiveRequirements req = (ReceiveRequirements)ar.AsyncState;

                                    Socket client = tcpClient;

                                    int bytesRead = client.EndReceive(ar);

                                    if (bytesRead > 0)
                                    {
                                        ms.Write(buffer, 0, buffer.Length);

                                        byte[] tempResponse = ms.ToArray();
                                        if (Array.IndexOf(tempResponse, delimiter) != -1)
                                        {
                                            receivedBytes.Enqueue(tempResponse);
                                            receiveLock.Set();
                                        }
                                        else
                                        {
                                            client.BeginReceive(buffer, 0, bufferSize, 0, req.callback, req);
                                        }
                                    }
                                    else
                                    {
                                        receivedBytes.Enqueue(ms.ToArray());
                                        receiveLock.Set();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        );

                        rr = new ReceiveRequirements(ReceiveCallback);
                        tcpClient.BeginReceive(buffer, 0, bufferSize, 0, ReceiveCallback, rr);

                        if (timeout > 0)
                        {
                            return receiveLock.WaitOne(timeout);
                        }
                        else
                        {
                            receiveLock.WaitOne();
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            public bool Disconnect(int timeout)
            {
                try
                {
                    ManualResetEvent disconnectLock = new ManualResetEvent(false);

                    tcpClient.Shutdown(SocketShutdown.Both);
                    tcpClient.BeginDisconnect(true,
                        new AsyncCallback(
                            (IAsyncResult ar) =>
                            {
                                Socket client = (Socket)ar.AsyncState;
                                client.EndDisconnect(ar);
                            }
                        ),
                        tcpClient
                    );

                    if (timeout > 0)
                    {
                        return disconnectLock.WaitOne(timeout);
                    }
                    else
                    {
                        disconnectLock.WaitOne();
                        return true;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }

            public byte[] DequeueMessage()
            {
                lock (blocker4)
                {
                    return receivedBytes.Count > 0 ? receivedBytes.Dequeue() : null;
                }
            }
        }
    }
}
