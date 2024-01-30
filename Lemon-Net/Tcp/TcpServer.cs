using Lemon_Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon_Net.Tcp
{
    public class TcpServer:IDisposable
    {
        #region
        private TcpListener m_listener;
        private Thread m_connectThread;
        private bool m_isExit=false;
        private List<System.Net.Sockets.TcpClient> m_clientList=null;
        private object syncRoot=new object();
        #endregion
        #region
        public event EventHandler<TcpDataEvent> DataReceived;
        public event EventHandler<TcpConnectEvent> ConnectEvent;
        #endregion
        public TcpServer() {
            m_clientList=new List<System.Net.Sockets.TcpClient> ();
        }

        public void Start(int port)
        {
            m_listener=TcpListener.Create(port);
            m_listener.Start();
            m_connectThread = new Thread(AcceptThreadProc);
            m_connectThread.IsBackground = true;
            m_connectThread.Start();
            m_isExit = false;
            m_clientList.Clear();
        }


        private void AcceptThreadProc()
        {
            while (!m_isExit) {
                if(m_listener!=null)
                {
                    var client= m_listener.AcceptTcpClient();
                    lock (m_clientList)
                    {
                        m_clientList.Add(client);
                        this.ConnectEvent?.Invoke(this, new TcpConnectEvent(true, client.Client.RemoteEndPoint));
                    }
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{client.Client.RemoteEndPoint.ToString()} Connected.....");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveThreadProc),client);
                }
                Thread.Sleep(10);
            }
        }

        private void ReceiveThreadProc(object state)
        {
            var client=state as System.Net.Sockets.TcpClient;
            bool isPackBegin = false;
            byte[] packbuffer = null;
            long index = 0;
            long packlen = 0;
            while(!m_isExit)
            {
                try
                {
                    if (!m_clientList.Contains(client)) break;
                    var canRead= client.Client.Poll(1000, SelectMode.SelectRead);
                    if (client.Connected && canRead && client.Available>0)
                    {
                        var stream = client.GetStream();
                        if (client.Available > 4+8)
                        {
                            byte[] buffer= new byte[12];
                            int len= stream.Read(buffer, 0, buffer.Length);//read pack header
                            if(len<12)
                                stream.Read(buffer, len, 12-len);
                            packlen = BitConverter.ToInt64(buffer, 4);
                            isPackBegin = true;
                            packbuffer = new byte[packlen];
                            Array.Copy(buffer, packbuffer, 12);
                            index = 12;
                        }
                        if(isPackBegin)
                        {
                            int len= stream.Read(packbuffer, 12, (int)(packlen - index));
                            index += len;
                            if(index>=packlen)
                            {
                                Pack pack=new Pack(packbuffer);
                                this.DataReceived?.Invoke(this, new TcpDataEvent(pack, client.Client.RemoteEndPoint));
                                isPackBegin= false;
                                index = 0;
                                packlen = 0;
                                Array.Clear(packbuffer,0,packbuffer.Length);
                            }
                        }
                    }
                    else if(canRead)
                    {
                        RemoveClient(client);
                        break;
                    }
                }
                catch {
                    RemoveClient(client);
                    break;
                }                             
                Thread.Sleep(10);
            }
        }
        public void SendMessage(string msg)
        {
            SendPack(Pack.BuildPack(msg));
        }
        public void SendPack(Pack pack,System.Net.Sockets.TcpClient client)
        {
            if(!m_isExit && m_listener != null && m_clientList!=null && m_clientList.Count()>0)
            {
                if(m_clientList.Contains(client))
                {
                    var canWrite =client.Client.Poll(1000, SelectMode.SelectWrite);
                    if (canWrite && client.Connected)
                    {
                        var strem = client.GetStream();
                        var buffer = pack.ToBytes();
                        strem.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        RemoveClient(client);
                    }
                }
            }
        }

        public void SendPack(Pack pack)
        {
            foreach(var client in m_clientList)
            {
                SendPack(pack, client);
            }
        }

        public void Stop()
        {
            m_isExit = true;
            if (m_listener != null)
            {
                m_listener.Stop();
                m_listener = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void RemoveClient(System.Net.Sockets.TcpClient client)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{client.Client.RemoteEndPoint.ToString()} disconnected.....");
            if (client != null)
            {
                this.ConnectEvent?.Invoke(this, new TcpConnectEvent(false,client.Client.RemoteEndPoint));
                client.Close();
                m_clientList.Remove(client);
                client = null;
            }
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] m_clientList.Cout()={m_clientList.Count}");
        }
    }
}
