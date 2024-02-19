using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Lemon_Net.Common;
using System.Net.NetworkInformation;

namespace Lemon_Net.Udp
{
    public class UdpClient
    {
        public event EventHandler<UdpDataEvent> DataReceived;
        private byte[] buff = new byte[65536];


        private Socket serverSocket;

        private int localActualPort;

        private List<IPAddress> localAdapterAddresses = new List<IPAddress>();

        private bool isExit;

        private bool isInitialized = false;

        private static object syncRoot=new object();
        private static UdpClient _Instance=null;
        public static UdpClient Instance
        {
            get
            {
                if( _Instance == null )
                {
                    lock(syncRoot)
                    {
                        if(_Instance == null)
                        {
                            _Instance = new UdpClient();
                        }
                    }
                }
                return _Instance;
            }
        }
        public void Close()
        {
            this.isExit = true;
            isInitialized = false;
            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
                this.serverSocket = null;
            }
        }

       
        public bool Listen(int port)
        {
            bool result;
            try
            {
                this.isExit = false;
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                this.serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
                this.serverSocket.Bind(localEP);
                this.localActualPort = ((IPEndPoint)this.serverSocket.LocalEndPoint).Port;
                EndPoint state = new IPEndPoint(IPAddress.Any, 0);
                this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref state, new AsyncCallback(this.Receive), state);
                this.localAdapterAddresses = this.GetAllAdaptersIPAddress();
                result = true;
                isInitialized = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

       
        protected void OnDataReceived(UdpDataEvent e)
        {
            EventHandler<UdpDataEvent> dataReceived = this.DataReceived;
            if (dataReceived == null)
            {
                return;
            }
            dataReceived(this, e);
        }

        public void Send(byte[] buffer, string ip, int port)
        {
            if (isInitialized)
            {
                this.serverSocket.SendTo(buffer, new IPEndPoint(IPAddress.Parse(ip), port));
            }
            else
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                socket.SendTo(buff, buff.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse(ip), port));
            }
        }

        
        public void Send(string data, string ip, int port)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            this.Send(bytes, ip, port);
        }

       
        public void SendBroadCast(byte[] buffer, int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
            foreach (IPAddress address in this.localAdapterAddresses)
            {
                System.Net.Sockets.UdpClient udpClient = new System.Net.Sockets.UdpClient(new IPEndPoint(address, this.localActualPort));
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                udpClient.Send(buffer, buffer.Length, endPoint);
                udpClient.Close();
            }
        }

        private List<IPAddress> GetAllAdaptersIPAddress()
        {
            List<IPAddress> list = new List<IPAddress>();
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!networkInterface.Description.Contains("Loopback") && networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            list.Add(unicastIPAddressInformation.Address);
                        }
                    }
                }
            }
            return list;
        }
        public void SendBroadCast(string data, int port)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            this.SendBroadCast(bytes, port);
        }

        private void Receive(IAsyncResult ar)
        {
            if (this.isExit)
            {
                return;
            }
            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                if (this.serverSocket != null)
                {
                    int num = this.serverSocket.EndReceiveFrom(ar, ref endPoint);
                    if (num > 0)
                    {
                        byte[] array = new byte[num];
                        Array.Copy(this.buff, 0, array, 0, array.Length);
                        this.OnDataReceived(new UdpDataEvent(endPoint, array));
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (this.serverSocket != null)
                {
                    this.serverSocket.BeginReceiveFrom(this.buff, 0, this.buff.Length, SocketFlags.None, ref endPoint, new AsyncCallback(this.Receive), endPoint);
                }
            }
        }        
    }
}
