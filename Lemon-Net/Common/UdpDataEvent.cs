using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public class UdpDataEvent:EventArgs
    {
        public UdpDataEvent(EndPoint remoteEP, byte[] recBytes)
        {
            this.IP = ((IPEndPoint)remoteEP).Address.ToString();
            this.RemoteEP = remoteEP;
            this.RecBytes = recBytes;
            this.RecString = Encoding.UTF8.GetString(recBytes);
        }

        public string IP { get; private set; }

        public byte[] RecBytes { get; private set; }

        public string RecString { get; private set; }

        public EndPoint RemoteEP { get; private set; }
    }
}
