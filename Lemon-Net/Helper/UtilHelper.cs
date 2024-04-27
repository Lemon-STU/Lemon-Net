using Lemon_Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon_Net.Helper
{
    internal class UtilHelper
    {
        /// <summary>
        /// read a certain number bytes from Tcp Stream,if timeout is -1,it will read without timeout
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytesToRead"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadBytesAsync(System.Net.Sockets.TcpClient client, int bytesToRead, int timeout = 1000)
        {
            if (bytesToRead <= 0) return new byte[0];
            int index = 0;
            byte[] buffer = new byte[bytesToRead];
            var stream = client.GetStream();
            int startTime = Environment.TickCount;
            while (index < bytesToRead)
            {
                if (client.Available > 0)
                {
                    int len = await stream.ReadAsync(buffer, index, bytesToRead - index);
                    index += len;
                }
                else
                    Thread.Sleep(10);

                if (timeout > 0 && (Environment.TickCount - startTime) > timeout)
                    break;
            }
            return buffer;
        }

        public static async Task<Pack> ReadPackAsync(System.Net.Sockets.TcpClient client,int timeout = 1000)
        {
            byte[] packHeader= await ReadBytesAsync(client, 16, -1);
            long packlen = BitConverter.ToInt64(packHeader,4);
            byte[] packData = await ReadBytesAsync(client, (int)(packlen - 16), timeout);
            byte[] buffer=new byte[packlen];
            Array.Copy(packHeader, buffer, packHeader.Length);
            Array.Copy(packData,0, buffer, 16,packData.Length);
            return new Pack(buffer);
        }

        public static Pack ReadPack(System.Net.Sockets.TcpClient client, int timeout = 1000)
        {
            byte[] packHeader =ReadBytes(client, 16, -1);
            long packlen = BitConverter.ToInt64(packHeader, 4);
            byte[] packData =ReadBytes(client, (int)(packlen - 16), timeout);
            byte[] buffer = new byte[packlen];
            Array.Copy(packHeader, buffer, packHeader.Length);
            Array.Copy(packData, 0, buffer, 16, packData.Length);
            return new Pack(buffer);
        }
        public static byte[] ReadBytes(System.Net.Sockets.TcpClient client, int bytesToRead, int timeout = 1000)
        {
            if (bytesToRead <= 0) return new byte[0];
            int index = 0;
            byte[] buffer = new byte[bytesToRead];
            var stream = client.GetStream();
            int startTime = Environment.TickCount;
            while (index < bytesToRead)
            {
                if (client.Available > 0)
                {
                    int len =stream.Read(buffer, index, bytesToRead - index);
                    index += len;
                }
                else
                    Thread.Sleep(10);

                if (timeout > 0 && (Environment.TickCount - startTime) > timeout)
                    break;
            }
            return buffer;
        }
    }
}
