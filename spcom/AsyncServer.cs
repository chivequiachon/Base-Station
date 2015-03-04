using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace SPRITE.Utility
{
    namespace Communication
    {
        public class AsyncServer
        {
            private Queue<byte[]> receivedBytes;

            private IPEndPoint localEndPoint;
            private Socket listener;
            
            private object blocker1;
            private object blocker2;
            private object blocker3;

            private class ReceiveRequirements
            {
                private AsyncCallback callback;

                public ReceiveRequirements(AsyncCallback callback)
                {
                    this.callback = callback;
                }

                public AsyncCallback Callback
                {
                    get { return this.callback; }
                }
            }

            public AsyncServer(string host, int port)
            {
                IPHostEntry ipHostInfo = Dns.Resolve(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                
                this.localEndPoint = new IPEndPoint(ipAddress, port);
                this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.receivedBytes = new Queue<byte[]>();
                
                this.blocker1 = new object();
                this.blocker2 = new object();
                this.blocker3 = new object();
            }

            public bool Bind()
            {
                lock (blocker1)
                {
                    try
                    {
                        listener.Bind(localEndPoint);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            public bool Listen(int backlog)
            {
                lock (blocker1)
                {
                    try
                    {
                        listener.Listen(backlog);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return false;
                    }
                }
            }

            public Socket Accept(int timeout)
            {
                lock (blocker1)
                {
                    Socket handler = null;
                    try
                    {
                        ManualResetEvent acceptLock = new ManualResetEvent(false);

                        listener.BeginAccept(new AsyncCallback(
                                (IAsyncResult ar) =>
                                {
                                    Socket tmp_socket = (Socket)ar.AsyncState;
                                    handler = tmp_socket.EndAccept(ar);
                                    acceptLock.Set();
                                }
                            ),
                            listener
                        );

                        if (timeout > 0)
                        {
                            acceptLock.WaitOne(timeout);
                        }
                        else
                        {
                            acceptLock.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    return handler;
                }
            }

            public bool Send(Socket handler, byte[] data, int timeout)
            {
                lock (blocker2)
                {
                    try
                    {
                        ManualResetEvent sendLock = new ManualResetEvent(false);
                        AsyncCallback sendCallback = new AsyncCallback(
                            (IAsyncResult ar) =>
                            {
                                try
                                {
                                    Socket tmp_handler = (Socket)ar.AsyncState;
                                    handler.EndSend(ar);
                                    sendLock.Set();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        );

                        byte[] byteData = data;

                        handler.BeginSend(byteData, 0, byteData.Length, 0, sendCallback, handler);

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

            public bool Receive(Socket handler, int bufferSize, byte delimeter, int timeout)
            {
                lock (blocker2)
                {
                    try
                    {
                        byte[] buffer = new byte[bufferSize];

                        ReceiveRequirements req;
                        MemoryStream ms = new MemoryStream();
                        ManualResetEvent receiveLock = new ManualResetEvent(false);

                        AsyncCallback receiveCallback = new AsyncCallback(
                            (IAsyncResult ar) =>
                            {
                                try
                                {
                                    ReceiveRequirements rr = (ReceiveRequirements)ar.AsyncState;
                                    AsyncCallback callback = rr.Callback;

                                    //String content = String.Empty;
                                    byte[] content = null;

                                    int bytesRead = handler.EndReceive(ar);

                                    if (bytesRead > 0)
                                    {
                                        ms.Write(buffer, 0, buffer.Length);
    
                                        content = ms.ToArray();

                                        if (Array.IndexOf(content, delimeter) != -1)
                                        {
                                            receivedBytes.Enqueue(content);
                                            receiveLock.Set();
                                        }
                                        else
                                        {
                                            handler.BeginReceive(buffer, 0, bufferSize, 0, callback, handler);
                                        }
                                    }
                                    else
                                    {
                                        handler.BeginReceive(buffer, 0, bufferSize, 0, callback, handler);
                                        receiveLock.Set();
                                    }

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }
                        );

                        req = new ReceiveRequirements(receiveCallback);
                        handler.BeginReceive(buffer, 0, bufferSize, 0, receiveCallback, req);

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

            public byte[] DequeueMessage()
            {
                lock (blocker3)
                {
                    return receivedBytes.Count != 0 ? receivedBytes.Dequeue() : null;
                }
            }
        }
    }
}
