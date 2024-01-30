using Lemon_Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon_Net.Tcp
{
    public class TcpClient : IDisposable
    {
        #region
        private System.Net.Sockets.TcpClient m_client;
        private bool m_isExit = false;
        #endregion

        #region
        public event EventHandler<TcpDataEvent> DataReceived;
        public event EventHandler<TcpConnectEvent> ConnectEvent;
        #endregion

        public TcpClient()
        {
            m_client=new System.Net.Sockets.TcpClient();
        }

        public async void Start(string ip,int port)
        {
            await m_client.ConnectAsync(ip, port);
            this.ConnectEvent?.Invoke(this, new TcpConnectEvent(true, m_client.Client.RemoteEndPoint));
            Console.WriteLine("Cnnnected to Server");
            ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveThreadProc), m_client);
        }

        private void ReceiveThreadProc(object state)
        {
            bool isPackBegin = false;
            byte[] packbuffer = null;
            long index = 0;
            long packlen = 0;
            while (!m_isExit)
            {
                try
                {
                    if (m_client == null) break;
                    var canRead = m_client.Client.Poll(1000, SelectMode.SelectRead);
                    if (m_client.Connected && canRead && m_client.Available > 0)
                    {
                        var stream = m_client.GetStream();
                        if (m_client.Available > 4 + 8)
                        {
                            byte[] buffer = new byte[12];
                            int len = stream.Read(buffer, 0, buffer.Length);//read pack header
                            if (len < 12)
                                stream.Read(buffer, len, 12 - len);
                            packlen = BitConverter.ToInt64(buffer, 4);
                            isPackBegin = true;
                            packbuffer = new byte[packlen];
                            Array.Copy(buffer, packbuffer, 12);
                            index = 12;
                        }
                        if (isPackBegin)
                        {
                            int len = stream.Read(packbuffer, 12, (int)(packlen - index));
                            index += len;
                            if (index >= packlen)
                            {
                                Pack pack = new Pack(packbuffer);
                                this.DataReceived?.Invoke(this, new TcpDataEvent(pack, m_client.Client.RemoteEndPoint));
                                isPackBegin = false;
                                index = 0;
                                packlen = 0;
                                Array.Clear(packbuffer, 0, packbuffer.Length);
                            }
                        }
                    }
                    else if (canRead)
                    {
                        Stop();
                    }
                }
                catch
                {
                    Stop();
                }
                Thread.Sleep(10);
            }
        }
        public void SendPack(Pack pack)
        {
            if (!m_isExit && m_client != null)
            {
                var canWrite = m_client.Client.Poll(1000, SelectMode.SelectWrite);
                if (canWrite && m_client.Connected)
                {
                    var strem = m_client.GetStream();
                    var buffer = pack.ToBytes();
                    strem.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void SendMessage(string msg)
        {
            SendPack(Pack.BuildPack(msg));
        }
        public void Stop()
        {
            Console.WriteLine("DisConnected form server.............");
            m_isExit = true;
            if (m_client != null)
            {
                this.ConnectEvent?.Invoke(this, new TcpConnectEvent(false, m_client.Client.RemoteEndPoint));
                m_client.Close();
                m_client = null;
            }
        }
        public void Dispose()
        {
            Stop();
        }
    }
}
