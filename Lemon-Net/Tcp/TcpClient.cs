using Lemon_Net.Common;
using Lemon_Net.Helper;
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
            byte[] packbuffer = null;//to store the pack data
            long index = 0;//offset of buffer position
            long packlen = 0;//pack length
            int packHeaderLen = Pack.PackHeaderLength;
            while (!m_isExit)
            {
                try
                {
                    if (m_client == null) break;
                    var canRead = m_client.Client.Poll(1000, SelectMode.SelectRead);
                    if (m_client.Connected && canRead && m_client.Available > 0)//can read and avaiilabe>0 meas hava data pending
                    {
                        var stream = m_client.GetStream();
                        if (m_client.Available > packHeaderLen)
                        {
                            byte[] buffer = new byte[packHeaderLen];
                            int len = stream.Read(buffer, 0, buffer.Length);//read sendPack header
                            if (len < packHeaderLen)
                                stream.Read(buffer, len, packHeaderLen - len);
                            packlen = BitConverter.ToInt64(buffer, 4);
                            isPackBegin = true;
                            packbuffer = new byte[packlen];
                            Array.Copy(buffer, packbuffer, packHeaderLen);
                            index = packHeaderLen;
                        }
                        if (isPackBegin)
                        {
                            int len = stream.Read(packbuffer, (int)index, (int)(packlen - index));
                            index += len;
                            if (index >= packlen)//pack is over
                            {
                                Pack pack = new Pack(packbuffer);
                                this.DataReceived?.Invoke(this, new TcpDataEvent(pack, m_client.Client.RemoteEndPoint));

                                //reset the variables
                                isPackBegin = false;
                                index = 0;
                                packlen = 0;
                                Array.Clear(packbuffer, 0, packbuffer.Length);
                            }
                        }
                    }
                    else if (canRead)//otherwise if can read but no data means the connect is bad,just sotp the connection
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
        public void SendMessage(int flag,string msg)
        {
            SendPack(Pack.BuildPack(0,flag,msg));
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


        #region Synchronous Methods

        /// <summary>
        /// if bytesToRead is 0,it will return immediatelay,if -1 it will read the full pack bytes
        /// in stream,otherwise it will read unitl the return size is equals bytesToRead
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="bytesToRead"></param>
        /// <returns></returns>
        public async Task<byte[]> ReadPackAsync(string ip, int port,int bytesToRead = -1)
        {
            try
            {
                if (m_client == null)
                {
                    m_client = new System.Net.Sockets.TcpClient();
                }
                if (!m_client.Connected)
                    await m_client.ConnectAsync(ip, port);
                var stream = m_client.GetStream();
                if (bytesToRead != 0)
                {
                    byte[] readBuffer = new byte[0];
                    if (bytesToRead == -1)
                    {
                        byte[] headBuffer = await UtilHelper.ReadBytesAsync(m_client, 16, -1);
                        if (headBuffer.Length == 16)
                        {
                            bytesToRead = (int)BitConverter.ToInt64(headBuffer, 4);
                            readBuffer = new byte[bytesToRead];
                            Array.Copy(headBuffer, readBuffer, headBuffer.Length);
                        }
                        var databuffer = await UtilHelper.ReadBytesAsync(m_client, bytesToRead - 16, -1);
                        Array.Copy(databuffer, 0, readBuffer, 16, databuffer.Length);
                    }
                    else
                    {
                        readBuffer = await UtilHelper.ReadBytesAsync(m_client, bytesToRead, -1);
                    }
                    return readBuffer;
                }
            }
            catch { }
            return new byte[0];
        }
     

        /// <summary>
        /// if bytesToRead is 0,it will return immediatelay,if -1 it will read the full pack bytes
        /// in stream,otherwise it will read unitl the return size is equals bytesToRead
        /// </summary>
        /// <param name="ip">remote server IP</param>
        /// <param name="port">remote server Port</param>
        /// <param name="sendPack">data pack to Send</param>
        /// <param name="bytesToRead">bytes num to read</param>
        /// <returns></returns>
        public async Task<byte[]> SendPackAsync(string ip, int port, Pack sendPack,int bytesToRead=0)
        {          
            try
            {
                if (m_client == null)
                {
                    m_client = new System.Net.Sockets.TcpClient();
                }
                if (!m_client.Connected)
                    await m_client.ConnectAsync(ip, port);
                var stream=m_client.GetStream();
                var buffer=sendPack.ToBytes();
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
                if(bytesToRead!=0)
                {
                    byte[] readBuffer = new byte[0];
                    if (bytesToRead==-1) { 
                        byte[] headBuffer =await UtilHelper.ReadBytesAsync(m_client, 16,-1);
                        if(headBuffer.Length==16)
                        {
                            bytesToRead=(int)BitConverter.ToInt64(headBuffer,4);
                            readBuffer = new byte[bytesToRead];
                            Array.Copy(headBuffer, readBuffer, headBuffer.Length);
                        }
                        var databuffer = await UtilHelper.ReadBytesAsync(m_client, bytesToRead - 16, -1);
                        Array.Copy(databuffer, 0, readBuffer, 16, databuffer.Length);
                    }
                    else
                    {
                        readBuffer = await UtilHelper.ReadBytesAsync(m_client, bytesToRead, -1);
                    }                    
                    return readBuffer;
                }
            }
            catch{}        
            return new byte[0];
        }
        public  Task<byte[]> SendMessageAsync(string ip, int port, string msg, int bytesToRead = 0)
        {
            return SendPackAsync(ip,port,Pack.BuildPack(msg),bytesToRead);
        }


        #endregion
        public void Dispose()
        {
            Stop();
        }
    }
}
