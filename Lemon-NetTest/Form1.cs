using Lemon_Net.Common;
using Lemon_Net.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lemon_NetTest
{
    public partial class Form1 : Form
    {
        byte[] data;

        void test(Pack pack)
        {
            this.data = pack.PackData;
        }

        TcpServer tcpServer = new TcpServer();
        public Form1()
        {
            InitializeComponent();

            Pack pack = new Pack();
            pack.PackData = Encoding.UTF8.GetBytes("He");
            pack.PackID = 0;

            var bytes = pack.ToBytes();

            //tcpServer.Start(5025);
            tcpServer.DataReceived += TcpServer_DataReceived;

           // tcpClient.Start("127.0.0.1",5025);
            
        }
        TcpClient tcpClient = new TcpClient();
        private void TcpServer_DataReceived(object sender, TcpDataEvent e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{e.Message}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //tcpClient.SendMessage("hello,world");
        
        }
    }
}
