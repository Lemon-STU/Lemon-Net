using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public struct Pack
    {
        public Int32 PackID;  
        public Int64 PackLength;
        public byte[] PackData;


        public byte[] ToBytes()
        {
            var idBytes = BitConverter.GetBytes(this.PackID);
            if (this.PackData != null)
                this.PackLength = this.PackData.Length + 12;
            else
                this.PackLength = 12;
            var lengthBytes = BitConverter.GetBytes(this.PackLength);
            var buffer = new byte[idBytes.Length + lengthBytes.Length + this.PackData.Length];
            Array.Copy(idBytes, 0, buffer, 0, idBytes.Length);
            Array.Copy(lengthBytes, 0, buffer, idBytes.Length, lengthBytes.Length);
            Array.Copy(this.PackData, 0, buffer, idBytes.Length + lengthBytes.Length, this.PackData.Length);
            return buffer;
        }

        public Pack(byte[] buffer)
        {
            PackID = 0;
            PackLength = 0;
            PackData = new byte[0];
            var idlen= sizeof(Int32);
            var lengthlen= sizeof(Int64);
            if(buffer.Length >= (idlen+lengthlen)) {
                PackID= BitConverter.ToInt32(buffer,0);
                PackLength=BitConverter.ToInt64(buffer,idlen);
                PackData = new byte[PackLength-12];
                if(PackData.Length>0)
                    Array.Copy(buffer,idlen+lengthlen, PackData, 0, this.PackData.Length);
            }
        }


        public static Pack BuildPack(int id,string payload)
        {
            Pack pack = new Pack();
            pack.PackID = id;
            pack.PackData=Encoding.UTF8.GetBytes(payload);
            pack.PackLength =pack.PackData.Length+12;
            return pack;
        }
        public static Pack BuildPack(string payload)
        {
            Pack pack = new Pack();
            pack.PackID =0;
            pack.PackData = Encoding.UTF8.GetBytes(payload);
            pack.PackLength = pack.PackData.Length + 12;
            return pack;
        }
    }


    
    


}
