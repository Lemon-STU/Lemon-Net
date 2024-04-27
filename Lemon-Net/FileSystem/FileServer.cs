using Lemon_Net.Common;
using Lemon_Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon_Net.FileSystem
{
    public class FileServer
    {
        private TcpServer m_TcpServer = null;
        private int m_Port = 0;
        //private
        public FileServer() { 
            
        }

        public void Setup(int port)
        {
            m_Port = port;
            m_TcpServer=new TcpServer();
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

        private void ReceiveFile(Pack pack)
        {
            if(pack.PackFlag == FileCommand.CommandFileBegin) {
                string filestr = Encoding.UTF8.GetString(pack.PackData);
                string[] args = filestr.Split('#');

            }
            else if(pack.PackFlag==FileCommand.CommandFilePart)
            {

            }
            else if(pack.PackFlag == FileCommand.CommandFileEnd)
            {

            }
        }
    }
}
