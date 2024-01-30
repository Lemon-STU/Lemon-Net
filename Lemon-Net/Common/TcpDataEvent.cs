using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public class TcpDataEvent:EventArgs
    {
        public byte[] Data { get;}
        public Pack Pack { get;}
        public string Message {  get;}
        public EndPoint IPEndPoint { get;}
        public TcpDataEvent(Pack pack,EndPoint iPEndPoint) { 
            this.Pack = pack;
            this.Data = pack.PackData;
            this.Message = Encoding.UTF8.GetString(this.Data);
            this.IPEndPoint = iPEndPoint;
        }
    }

    public class TcpConnectEvent:EventArgs
    {
        public bool ConnectStatus {  get;}
        public EndPoint IPEndPoint { get; }
        public TcpConnectEvent(bool connectStatus, EndPoint iPEndPoint)
        {
            ConnectStatus = connectStatus;
            this.IPEndPoint = iPEndPoint;
        }
    }
}
