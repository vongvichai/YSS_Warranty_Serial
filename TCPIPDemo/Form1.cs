using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using SimpleTcp;

namespace TCPIPDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SimpleTcpServer server;

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new SimpleTcpServer("127.0.0.1:9000");
            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;
            //
            server.Start();
            //
            server.Send("[ClientIP:Port]", "Hello world!");
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            lbxStatus.Invoke((MethodInvoker)delegate ()
            {
                var text = Encoding.UTF8.GetString(e.Data);
                lbxStatus.Items.Add (text);
                if (text.Contains("WX"))
                    server.Send(e.IpPort,"WX,OK\r\n");
                else
                    server.Send(e.IpPort,string.Format("You said: {0}{1}", text, Environment.NewLine));
            });
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            lbxStatus.Invoke((MethodInvoker)delegate ()
            {
                lbxStatus.Items.Add($"Disconnected: {e.IpPort}");
            });
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            lbxStatus.Invoke((MethodInvoker)delegate ()
            {
                lbxStatus.Items.Add($"Connected: {e.IpPort}");
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            lbxStatus.Items.Add("Server starting...");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (server.IsListening)
                server.Stop();
        }
    }
}
