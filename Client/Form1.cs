using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleTcp;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SimpleTcpClient client;

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            //client.Connect(txtHost.Text, Convert.ToInt32(txtPort.Text));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new SimpleTcpClient("127.0.0.1:9000");
            client.Events.Connected += Connected;
            client.Events.Disconnected += Disconnected; ;
            client.Events.DataReceived += DataReceived; ;
            //
            client.Connect();
            client.Send("Hello world!");
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                txtStatus.Text += Encoding.UTF8.GetString(e.Data);
            });
        }

        private void Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                txtStatus.Text += $"Disconnected: [{e.IpPort}]";
            });
        }

        private void Connected(object sender, ClientConnectedEventArgs e)
        {
            txtStatus.Invoke((MethodInvoker)delegate ()
            {
                txtStatus.Text += $"Connected: [{e.IpPort}]";
            });
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //client.WriteLineAndGetReply(txtMessage.Text, TimeSpan.FromSeconds(3));
            client.Send(txtMessage.Text + Environment.NewLine);
        }
    }
}
