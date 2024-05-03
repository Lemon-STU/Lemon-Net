using Lemon_Net.Common;
using Lemon_Net.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.FileSystem
{
    public class FileServer
    {
        private TcpServer m_TcpServer = null;
        private int m_Port = 0;
        private string m_RootDir;


        public event EventHandler<FileDataEvent> OnFileDataEvent;
        public FileServer() { 
            
        }
        /// <summary>
        /// setup the file server
        /// </summary>
        /// <param name="port"></param>
        /// <param name="rootdir"></param>
        public void Setup(int port,string rootdir="")
        {
            m_Port = port;
            m_TcpServer=new TcpServer();

            if (string.IsNullOrEmpty(rootdir))
                m_RootDir = Environment.CurrentDirectory;
            else
                m_RootDir=rootdir;
            if(!Directory.Exists(m_RootDir))
                Directory.CreateDirectory(m_RootDir);

            m_TcpServer.Start(m_Port);
            m_TcpServer.DataReceived += M_TcpServer_DataReceived;
        }

        private void M_TcpServer_DataReceived(object sender, Common.TcpDataEvent e)
        {
            switch(e.Pack.PackFlag)
            {
                case FileCommand.CommandFlag:
                    ReceiveCommand(e.Pack);
                    break;
                case FileCommand.CommandFileBegin:
                case FileCommand.CommandFilePart:
                case FileCommand.CommandFileEnd:
                    ReceiveFile(e.Pack);
                    break;                
            }
        }

        private void ReceiveCommand(Pack pack)
        {
            var cmdstr =Encoding.UTF8.GetString(pack.PackData);
            string[] args = cmdstr.Split('#');
            string cmd = args[0];
            string cmdArg = args[1];
            switch(cmd)
            {
                case FileCommand.ls:break;
                case FileCommand.rm:break;
                case FileCommand.rename:break;
                case FileCommand.mkdir:
                    break;
                case FileCommand.cd:break;
                case FileCommand.get:break;
            }
        }


        private FileStream fs=null;
        private int TotalParts = 0;
        private string filePath = "";
        private void ReceiveFile(Pack pack)
        {
            if(pack.PackFlag == FileCommand.CommandFileBegin) {
                string filestr = Encoding.UTF8.GetString(pack.PackData);
                string[] args = filestr.Split('#');
                if(args.Length==2)
                {
                    string strTotalParts = args[0];
                    TotalParts=int.Parse(strTotalParts);
                    string fileName =Path.GetFileName(args[1]);
                    if(fs==null)
                    {
                        filePath = $"{m_RootDir}\\{fileName}.tmp";
                        fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    }
                }
            }
            else if(pack.PackFlag==FileCommand.CommandFilePart)
            {
                if(fs!=null)
                {
                    fs.Write(pack.PackData,0,pack.PackData.Length);
                }
            }
            else if(pack.PackFlag == FileCommand.CommandFileEnd)
            {
                if(fs!=null)
                {
                    fs.Close();
                    TotalParts = 0;
                    fs = null;
                    string realFileName = filePath.Substring(0,filePath.Length-4);
                    bool isSuccessful = false;
                    try
                    {
                        if (File.Exists(realFileName))
                        {
                            File.Delete(realFileName);
                        }
                        File.Move(filePath, realFileName);
                        isSuccessful = true;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    OnFileDataEvent?.Invoke(this,new FileDataEvent(isSuccessful,filePath));
                    Console.WriteLine(realFileName);
                    filePath = "";
                }
            }
        }
    }
}
