using Lemon_Net.Common;
using Lemon_Net.Helper;
using Lemon_Net.Tcp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon_Net.FileSystem
{
    
    public class FileClient
    {
        private string m_CurrentDir;

        private int m_ServerPort;
        private string m_ServerIP;

        private TcpClient m_TcpClient;

        private static  AutoResetEvent m_AutoResetEvent;
        
        
        public FileClient() { 
            m_AutoResetEvent = new AutoResetEvent(false);
        }

        public void Setup(string ip,int port=10005)
        {
            this.m_ServerIP = ip;
            this.m_ServerPort = port;
            m_TcpClient = new TcpClient();
        }


        public void Stop()
        {
            if(m_TcpClient != null)
            {
                m_TcpClient.Stop();
            }
        }

        
        public async Task<bool> RenameFile(string beforeFileName,string afterFileName)
        {
            string beforeFilePath = $"{m_CurrentDir}/{beforeFileName}";
            var pack = await SendCommand($"{FileCommand.rename}#{beforeFilePath}:{afterFileName}");
            if (pack.PackFlag == FileCommand.CommandFileSuccessful)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> CreateDirectory(string dir)
        {
            m_CurrentDir = $"{m_CurrentDir}/{dir}";
            var pack= await SendCommand($"{FileCommand.mkdir}#{m_CurrentDir}");
            if(pack.PackFlag==FileCommand.CommandFileSuccessful)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// filename format dDriectory\n-File,split with '\n'
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public async Task<List<FileItem>> ListDirectory(string dir)
        {
            List<FileItem> list=new List<FileItem>();
            if (!string.IsNullOrEmpty(m_CurrentDir))
            {
                m_CurrentDir = $"{m_CurrentDir}/{dir}";
                var ackPack = await SendCommand($"{FileCommand.cd}#{m_CurrentDir}");
                if (ackPack.PackFlag == FileCommand.CommandFileSuccessful)
                {
                    if (ackPack.PackData != null)
                    {
                        string filestr = Encoding.UTF8.GetString(ackPack.PackData);
                        
                        var args= filestr.Split('\n');
                        foreach (var item in args)
                        {
                            if(item.StartsWith("d"))
                            {
                                list.Add(new FileItem { FileName = item.Substring(1),IsDirectory=true });
                            }
                            else if(item.StartsWith("-"))
                            {
                                list.Add(new FileItem { FileName = item.Substring(1), IsDirectory = false });
                            }
                        }
                    }
                }
            }
            return list;
        }
        public async Task<List<FileItem>> GotoDirectory(string dir)
        {
            m_CurrentDir = $"{m_CurrentDir}/{dir}";
            return await ListDirectory(m_CurrentDir);
        }

        public async Task<List<FileItem>> GotoParent()
        {
            int pos = m_CurrentDir.LastIndexOf("/");
            m_CurrentDir = m_CurrentDir.Substring(0, pos);
            return await ListDirectory(m_CurrentDir);
        }

        public async Task<bool> DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(m_CurrentDir))
            {
                string remoteFilePath = $"{m_CurrentDir}/{fileName}";
                var ackPack= await SendCommand($"{FileCommand.rm}#{remoteFilePath}");
                if(ackPack.PackFlag==FileCommand.CommandFileSuccessful)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<Pack> SendCommand(string command)
        {
            var executeResult= await m_TcpClient.SendPackAsync(this.m_ServerIP,this.m_ServerPort,Pack.BuildPack(0,FileCommand.CommandFlag,command),-1);
            m_TcpClient.Stop();
            return new Pack(executeResult.ResultBuffer);
        }

        public async void GetFile(string fileName, string targetFilePath)
        {
            string filePath = $"{m_CurrentDir}/{fileName}";
            var pack = await SendCommand($"{FileCommand.get}#{filePath}");
            if (pack.PackFlag == FileCommand.CommandFileSuccessful)
            {
                string fileinfo = Encoding.UTF8.GetString(pack.PackData);//totalParts#remoteFilePath"
                string[] args = fileinfo.Split('#');
                long totalParts = long.Parse(args[0]);
                string remoteFileName = args[1];
                int index = 0;
                using (FileStream fs = new FileStream(targetFilePath, FileMode.OpenOrCreate))
                {
                    while (index < totalParts)
                    {
                        var partBuffer = await m_TcpClient.ReadPackAsync(this.m_ServerIP, this.m_ServerPort, -1);
                        fs.Write(partBuffer, 0, partBuffer.Length);
                        index++;
                    }
                }
            }
            m_TcpClient.Stop();
        }

        /// <summary>
        /// Send File to Server
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="filePath"></param>
        /// <param name="port"></param>
        public static async void SendFile(string ip,string filePath,Action<ExecuteResult> continueWith=null, int port=10005)
        {
            ExecuteResult result = ExecuteResult.Empty;
            if(m_AutoResetEvent==null)
                m_AutoResetEvent = new AutoResetEvent(true);
            if (!m_AutoResetEvent.WaitOne(2000)) { continueWith?.Invoke(ExecuteResult.Empty); return; }
            TcpClient m_TcpClient = new TcpClient();
            string filename = Path.GetFileName(filePath);
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[FileCommand.BufferMaxLength];
                int len = 0;
                int index = 0;
                long totalParts = fs.Length / FileCommand.BufferMaxLength;
                if (fs.Length % FileCommand.BufferMaxLength != 0) { totalParts++; }
                Pack pack = Pack.BuildPack(index++, FileCommand.CommandFileBegin, $"{totalParts}#{filename}");
                result= await m_TcpClient.SendPackAsync(ip,port, pack);//send server the file info
                if (result.Fail)
                {
                    continueWith?.Invoke(result);
                    m_TcpClient.Stop();
                    m_AutoResetEvent.Set();
                    m_TcpClient = null;
                    return;
                }
                while ((len = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (len == FileCommand.BufferMaxLength)
                        pack = Pack.BuildPack(index++, FileCommand.CommandFilePart, buffer);
                    else
                    {
                        var tmpbuffer = new byte[len];
                        Array.Copy(buffer, tmpbuffer, len);
                        pack = Pack.BuildPack(index++, FileCommand.CommandFilePart, tmpbuffer);
                    }
                    result= await m_TcpClient.SendPackAsync(ip,port, pack);//send file parts info
                    if(result.Fail)
                    {
                        continueWith?.Invoke(result);
                        m_TcpClient.Stop();
                        m_AutoResetEvent.Set();
                        m_TcpClient = null;
                        return;
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                }
                pack = Pack.BuildPack(index++, FileCommand.CommandFileEnd, "FileEnd");
                result= await m_TcpClient.SendPackAsync(ip, port, pack);//send file end info
            }
            m_TcpClient.Stop();
            m_AutoResetEvent.Set();
            m_TcpClient= null;
            continueWith?.Invoke(result);
        }
    }
}
