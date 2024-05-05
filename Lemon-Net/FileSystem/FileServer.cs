using Lemon_Net.Common;
using Lemon_Net.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon_Net.FileSystem
{
    public class FileServer
    {
        private TcpServer m_TcpServer = null;
        private int m_Port = 0;
        private string m_RootDir;


        public event EventHandler<FileDataEvent> OnFileDataEvent;

        private Queue<Pack> cachedPacks=new Queue<Pack>();
        private Thread m_writeThread = null;
        private FileStream fs = null;
        private int TotalParts = 0;
        private string filePath = "";
        private bool m_isExit = false;
        private bool m_isWriteOver = true;
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
            m_isExit = false;
            if (string.IsNullOrEmpty(rootdir))
                m_RootDir = Environment.CurrentDirectory;
            else
                m_RootDir=rootdir;
            if(!Directory.Exists(m_RootDir))
                Directory.CreateDirectory(m_RootDir);

            m_TcpServer.Start(m_Port);
            m_TcpServer.DataReceived += M_TcpServer_DataReceived;

            m_writeThread = new Thread(WriteThreadProc);
            m_writeThread.IsBackground = true;
           // m_writeThread.Start();
        }

        private void WriteThreadProc()
        {
            while(!m_isExit)
            {
                if (cachedPacks.Count > 0)
                {
                    var pack = cachedPacks.Dequeue();
                    ReceiveFile(pack);
                }
                Thread.Sleep(10);
            }
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
                    //cachedPacks.Enqueue(e.Pack);  
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


        private bool isFileBegin = false;
        private void ReceiveFile(Pack pack)
        {
            try
            {
                if (pack.PackFlag == FileCommand.CommandFileBegin && m_isWriteOver &&!isFileBegin)
                {
                    isFileBegin = true;
                    m_isWriteOver = false;
                    string filestr = Encoding.UTF8.GetString(pack.PackData);
                    string[] args = filestr.Split('#');
                    if (args.Length == 2)
                    {
                        string strTotalParts = args[0];
                        TotalParts = int.Parse(strTotalParts);
                        string fileName = Path.GetFileName(args[1]);
                        if (fs == null)
                        {
                            filePath = $"{m_RootDir}\\{fileName}";
                            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            AppendLog($"{Path.GetFileName(filePath)} Write begin...");
                        }
                    }
                }
                else if (pack.PackFlag == FileCommand.CommandFilePart && isFileBegin)
                {
                    if (fs != null)
                    {
                        fs.Write(pack.PackData, 0, pack.PackData.Length);
                        AppendLog($"{Path.GetFileName(filePath)}: {pack.PackID}/{TotalParts}");
                    }
                }
                else if (pack.PackFlag == FileCommand.CommandFileEnd && isFileBegin)
                {
                    m_TcpServer.SendMessage("FileEndAck");
                    if (fs != null)
                    {
                        fs.Flush(true);
                        fs.Close();
                        TotalParts = 0;
                        fs = null;
                        OnFileDataEvent?.Invoke(this, new FileDataEvent(true, filePath));
                        filePath = "";
                    }
                    m_isWriteOver = true;
                    isFileBegin = false;
                    AppendLog($"{Path.GetFileName(filePath)} is Write over!");
                }
            }
            catch (Exception e)
            {
                if (fs != null)
                {
                    fs.Close();
                }
                TotalParts = 0;
                fs = null;
                filePath = "";
                m_isWriteOver = true;
                OnFileDataEvent?.Invoke(this, new FileDataEvent(false, e.Message));
                AppendLog(e.Message);
            }        
        }

        private void AppendLog(string msg)
        {
            string file = $"{Environment.CurrentDirectory}\\recelog.txt";
            File.AppendAllText(file, $"[{DateTime.Now.ToString("HH:mm::ss,fff")}]{msg}\n");
            File.AppendAllText(file, $"{msg}\n");
        }
    }
}
