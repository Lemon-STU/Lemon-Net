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
        /// <summary>
        /// pack header length=sizeof(PackID)+sizeof(PackLength)+sizeof(PackFlag)
        /// </summary>
        public const int PackHeaderLength = sizeof(Int32)+sizeof(Int64)+sizeof(Int32);
        /// <summary>
        /// pack index
        /// </summary>
        public Int32 PackID { get; set; }  
        /// <summary>
        /// total pack length include pack header and pack data
        /// </summary>
        public Int64 PackLength { get; private set; }
        /// <summary>
        /// other flag to identify the pack function
        /// </summary>
        public Int32 PackFlag { get; set; }
        /// <summary>
        /// pack data
        /// </summary>
        public byte[] PackData { get; set; }
        
        /// <summary>
        /// pack data length
        /// </summary>
        public Int64 PackDataLength { get
            {
                return PackData.Length;
            } }

        /// <summary>
        /// convert pack to bytes
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// pack buffer to pack
        /// </summary>
        /// <param name="buffer"></param>
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

        /// <summary>
        /// build pack with id,flag,payload
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(int id,int flag,string payload)
        {
            Pack pack = new Pack();
            pack.PackID = id;
            pack.PackData = Encoding.UTF8.GetBytes(payload);
            pack.PackFlag = flag;
            pack.PackLength = pack.PackData.Length + 16;
            return pack;
        }
        /// <summary>
        /// build pack
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(int id,string payload)
        {
            return BuildPack(id, 0, payload);
        }
        /// <summary>
        /// build pack
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(string payload)
        {
            return BuildPack(0,payload);
        }

        /// <summary>
        /// build pack
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(int id, int flag, byte[] payload)
        {
            Pack pack = new Pack();
            pack.PackID = id;
            pack.PackData = payload;
            pack.PackFlag = flag;
            pack.PackLength = pack.PackData.Length + 16;
            return pack;
        }
        /// <summary>
        /// build pack
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(int id, byte[] payload)
        {
            return BuildPack(id,0,payload);
        }

        /// <summary>
        /// build pack
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static Pack BuildPack(byte[] payload)
        {
            return BuildPack(0,payload);
        }

        /// <summary>
        /// valide if the pack is correct
        /// </summary>
        /// <returns></returns>
        public bool IsValidPack()
        {
            return PackLength == this.PackData.Length + 16;
        }

        public override string ToString()
        {
            if (this.PackData == null)
                return "";
            return Encoding.UTF8.GetString(this.PackData);
        }
    }
}
