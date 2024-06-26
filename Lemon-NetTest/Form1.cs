﻿using Lemon_Net.Common;
using Lemon_Net.FileSystem;
using Lemon_Net.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lemon_NetTest
{
    public partial class Form1 : Form
    {
        TcpServer tcpServer = new TcpServer();

        FileServer fileServer = new FileServer();
        public Form1()
        {
            InitializeComponent();         
        }
        TcpClient tcpClient = new TcpClient();
        private void TcpServer_DataReceived(object sender, TcpDataEvent e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}]{e.Message}");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //var buffer= await tcpClient.SendMessageAsync("127.0.0.1", 5025,"Hello", int.Parse(textBox1.Text.Trim()));
            //var buffer = await tcpClient.ReadPackAsync("127.0.0.1", 5025, int.Parse(textBox1.Text.Trim()));
            //await Console.Out.WriteLineAsync($"buffer is returned:");
            //var msg= Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            //await Console.Out.WriteLineAsync(msg);
            //tcpClient.Stop();

            OpenFileDialog dlg=new OpenFileDialog();
            if(dlg.ShowDialog()==DialogResult.OK)
            {

                string ip = textBox1.Text.Trim();
                var begin = Environment.TickCount;
                FileClient.SendFile(ip, dlg.FileName, () => { 
                    var diff=Environment.TickCount- begin;
                    
                    MessageBox.Show($"FileTrans Over!Time:{diff}ms"); 
                });
                //fileClient.SendFile(dlg.FileName);
            }
            
        }
    }
}
