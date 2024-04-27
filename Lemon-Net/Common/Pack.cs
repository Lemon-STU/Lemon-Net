using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.Common
{
    public struct Pack
    {
        public Int32 PackID { get; set; }  
        public Int64 PackLength { get; private set; }
        public Int32 PackFlag { get; set; }
        public byte[] PackData { get; set; }
         
        public byte[] ToBytes()
        {
            var idBytes = BitConverter.GetBytes(this.PackID);
            if (this.PackData != null)
                this.PackLength = this.PackData.Length + 16;
            else
                this.PackLength = 16;
            var lengthBytes = BitConverter.GetBytes(this.PackLength);
            var flagBytes=BitConverter.GetBytes(this.PackFlag);
            var buffer = new byte[16 + this.PackData.Length];
            Array.Copy(idBytes, 0, buffer, 0, 4);
            Array.Copy(lengthBytes, 0, buffer, 4, lengthBytes.Length);
            Array.Copy(flagBytes,0,buffer,12, flagBytes.Length);
            Array.Copy(this.PackData, 0, buffer, 16, this.PackData.Length);
            return buffer;
        }

        public Pack(byte[] buffer)
        {
            PackID = 0;
            PackLength = 0;
            PackFlag = 0;
            PackData = new byte[0];
            if(buffer.Length >= 16) {
                PackID= BitConverter.ToInt32(buffer,0);
                PackLength=BitConverter.ToInt64(buffer,4);
                PackFlag=BitConverter.ToInt32(buffer,12);
                PackData = new byte[PackLength-16];
                if(PackData.Length>0)
                    Array.Copy(buffer,16, PackData, 0, this.PackData.Length);
            }
        }

        public static Pack BuildPack(int id,int flag,string payload)
        {
            Pack pack = new Pack();
            pack.PackID = id;
            pack.PackData = Encoding.UTF8.GetBytes(payload);
            pack.PackFlag = flag;
            pack.PackLength = pack.PackData.Length + 16;
            return pack;
        }
        public static Pack BuildPack(int id,string payload)
        {
            return BuildPack(id, 0, payload);
        }
        public static Pack BuildPack(string payload)
        {
            return BuildPack(0,payload);
        }

        public static Pack BuildPack(int id, int flag, byte[] payload)
        {
            Pack pack = new Pack();
            pack.PackID = id;
            pack.PackData = payload;
            pack.PackFlag = flag;
            pack.PackLength = pack.PackData.Length + 16;
            return pack;
        }
        public static Pack BuildPack(int id, byte[] payload)
        {
            return BuildPack(id,0,payload);
        }
        public static Pack BuildPack(byte[] payload)
        {
            return BuildPack(0,payload);
        }

        public bool IsValidPack()
        {
            return PackLength == this.PackData.Length + 16;
        }
    }
}
