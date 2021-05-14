using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.IO.Ports;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Configuration;
using SimpleTCP;

namespace YSS_Warranty_Serial
{
    public partial class FormMain : Form
    {

        private SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString);
        delegate void SetTextCallback(string text);
        TcpClient tcpclient = new TcpClient();
        

        public FormMain()
        {

            InitializeComponent();

            //TcpClient tcpclient = new TcpClient();
            //tcpclient.Connect("192.168.1.201", 500);
            
            

           

            //StartServer();  
        }

        SimpleTcpClient client;

        public static void CreateQrcode(String SerialCode1, String SerialCode2)
        {
            string Serialwar;

            if (SerialCode2 == "")
            {
                Serialwar = SerialCode1;
            }
            else
            {
                Serialwar = SerialCode1 + ',' + SerialCode2; 
            }
            var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", "www.yss.co.th/warranty.php?serial=" + Serialwar, 200, 200);
            WebResponse response = default(WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            WebRequest request = WebRequest.Create(url);                        
            
            response = request.GetResponse();
            remoteStream = response.GetResponseStream();
            readStream = new StreamReader(remoteStream);
            System.Drawing.Image img = System.Drawing.Image.FromStream(remoteStream);
            //img.Save("D:/QRCode/123456" + txtCode.Text + ".png");
            img.Save("D:/QRCode/" + SerialCode1 + ".png");
            response.Close();
            remoteStream.Close();
            readStream.Close();            
        }

        public static void Connect3(string host, int port)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

                MessageBox.Show("Establishing Connection to {0}",
                    host);
                s.Connect(host, port);
                MessageBox.Show("Connection established");
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                MessageBox.Show(e.ToString());
                MessageBox.Show(e.Message);

            }
            
        }		

        public static void StartServer()
        {
            // Get Host IP Address that is used to establish a connection  
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
            // If a host has multiple addresses, you will get a list of addresses  
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);


            try
            {

                // Create a Socket that will use Tcp protocol      
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method  
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                listener.Listen(10);

                //Console.WriteLine("Waiting for a connection...");
                MessageBox.Show("Waiting for a connection...");
                Socket handler = listener.Accept();

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                //Console.WriteLine("Text received : {0}", data);
                MessageBox.Show("Text received : {0}", data);

                byte[] msg = Encoding.ASCII.GetBytes(data);
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                MessageBox.Show(e.ToString());

            }

            //Console.WriteLine("\n Press any key to continue...");
            //Console.ReadKey();
        }

       

        private void FormMain_Load(object sender, EventArgs e)
        {
            
            // TODO: This line of code loads data into the 'ySS_DBLINKDataSet.YSS_WARRANTY_SERIAL' table. You can move, or remove it, as needed.
            //this.ySS_WARRANTY_SERIALTableAdapter.Fill(this.ySS_DBLINKDataSet.YSS_WARRANTY_SERIAL);

            //this.reportViewer1.RefreshReport();                                 
            string[] ports = SerialPort.GetPortNames();
            cboport.Items.AddRange(ports);
            //btnsend.Enabled = false;
            labelStatus.Text = "NOT CONNECT";
            labelStatus.BackColor = Color.FromArgb(255, 0, 0);
            //cboport.SelectedIndex = 0;
            //btnclose.Enabled = false;
            Load_Production();

            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            txtreceive.Invoke((MethodInvoker)delegate()
            {
                txtreceive.Text += e.MessageString;
            });
        }

        private void DataReceivedHandler( object sender,
                        SerialDataReceivedEventArgs e)
        {
           /* SerialPort sp = (SerialPort)sender;
            string textIn = sp.ReadExisting();
            SetText(textIn);*/
        }

        private void SetText(string text)
        {
           /* if (txtreceive.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                txtreceive.Text = txtreceive.Text + text;
            }*/

        }

        
        private void buttonSearchPD_Click(object sender, EventArgs e)
        {          
            
            try
            {
                // timer2.Enabled = false;

                

                //SqlConnection SQL_connectionStr;

                //SQL_connectionStr = new SqlConnection(strConnString);
                //SQL_connectionStr.Open();
                conn.Open();

                SqlCommand CmdSQLST = conn.CreateCommand();
                CmdSQLST.CommandText = "EXEC YSS_CAL_WARRANTY '" + comboBoxPD.Text + "','" + ConfigurationManager.ConnectionStrings["yss_department"].ConnectionString + "'";
                CmdSQLST.ExecuteNonQuery();

                SqlCommand CmdSQL = conn.CreateCommand();

                DataTable table_Not = new DataTable("YSS_WARRANTY_SERIAL");
                //table_Not.Columns.Add("ITEMID", typeof(String));
                table_Not.Columns.Add("MARKINGCODE1", typeof(String));
                table_Not.Columns.Add("MARKINGCODE2", typeof(String));
                table_Not.Columns.Add("UNIT", typeof(String));
                table_Not.Columns.Add("LS_MODEL", typeof(String));
                table_Not.Columns.Add("LS_LOCATION", typeof(String));             
                table_Not.Columns.Add("DEPARTMENT", typeof(String));
                table_Not.Columns.Add("DATE_PRINT", typeof(DateTime));



                TxtItem.Clear();

                CmdSQL.CommandText = "SELECT ITEMID, MARKINGCODE1, MARKINGCODE2, DEPARTMENT, UNIT, LS_MODEL, LS_LOCATION,NAME,DATE_PRINT FROM YSS_WARRANTY_SERIAL WHERE DATE_PRINT = '1900-01-01 00:00:00.000'  and PRODID = '" + comboBoxPD.Text + "' and DEPARTMENT = '" + ConfigurationManager.ConnectionStrings["yss_department"].ConnectionString + "'"; // and SALESID = 'SO17025611'";
                SqlDataReader ReaderSQL = CmdSQL.ExecuteReader();
                while (ReaderSQL.Read())
                {
                    if (TxtItem.Text == "")
                    {
                        TxtItem.Text = ReaderSQL.GetString(0) + "-" + ReaderSQL.GetString(7);
                    }
                    DataRow newRow = table_Not.NewRow();
                    //newRow["ITEMID"] = ReaderSQL.GetString(0);
                    newRow["MARKINGCODE1"] = ReaderSQL.GetString(1);
                    newRow["MARKINGCODE2"] = ReaderSQL.GetString(2);
                    newRow["UNIT"] = ReaderSQL.GetString(4);
                    newRow["LS_MODEL"] = ReaderSQL.GetString(5);
                    newRow["LS_LOCATION"] = ReaderSQL.GetString(6);
                    newRow["DEPARTMENT"] = ReaderSQL.GetString(3);
                    newRow["DATE_PRINT"] = ReaderSQL.GetDateTime(8); 
                 
                    table_Not.Rows.Add(newRow);
                }
                ReaderSQL.Close();
                this.dataGridView1.AutoGenerateColumns = true;
                this.dataGridView1.DataSource = table_Not;
                dataGridView1.ReadOnly = false;
                //dataGridView1.Columns["ITEMID"].ReadOnly = true;
                dataGridView1.Columns["MARKINGCODE1"].ReadOnly = true;
                dataGridView1.Columns["MARKINGCODE2"].ReadOnly = true;
                dataGridView1.Columns["UNIT"].ReadOnly = true;
                dataGridView1.Columns["LS_MODEL"].ReadOnly = true;
                dataGridView1.Columns["LS_LOCATION"].ReadOnly = true;
                dataGridView1.Columns["DEPARTMENT"].ReadOnly = true;
                dataGridView1.Columns["DATE_PRINT"].ReadOnly = true;


                

                //this.ITEMIDDataGridViewTextBoxColumn.ReadOnly(true);


                //                timer2.Enabled = true;



               


                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show(ex.Message);
                conn.Close();
                // timer2.Enabled = true;
                // DynAx.Logoff();
            }            
        }


        String imageurl = null;
        private void buttonStart_Click(object sender, EventArgs e)
        {
               Warning[] warnings;
               string[] streamids;
               string mimeType;
               string encoding;
               string extension;
               int X = 1;
               foreach (DataGridViewRow row in dataGridView1.Rows)
               {                   
                   if (row.Cells["DATE_PRINT"].Value.ToString() == "01/01/1900 00:00:00")
                   {
                       row.Cells["DATE_PRINT"].Value = DateTime.Now;
                       conn.Open();

                       SqlCommand CmdSQLST = conn.CreateCommand();
                       CmdSQLST.CommandText = "Update YSS_WARRANTY_SERIAL set DATE_PRINT = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where  PRODID = '" + comboBoxPD.Text + "' and MARKINGCODE1 = '" + row.Cells["MARKINGCODE1"].Value.ToString() + "' and DEPARTMENT = '" + ConfigurationManager.ConnectionStrings["yss_department"].ConnectionString + "'";
                       CmdSQLST.ExecuteNonQuery();

                       conn.Close();

                       imageurl = "D:\\QRcode\\" + row.Cells["MARKINGCODE1"].Value.ToString() + ".png";
                       //row.Cells["MARKINGCODE1"].Value = X;
                       CreateQrcode(row.Cells["MARKINGCODE1"].Value.ToString(), row.Cells["MARKINGCODE2"].Value.ToString());
                       FileInfo fi = new FileInfo(imageurl);
                       ReportParameter PUrl = new ReportParameter("PUrl", new Uri(imageurl).AbsoluteUri);
                       //ReportParameter serial = new ReportParameter("serial", row.Cells["MARKINGCODE1"].Value.ToString());
                       this.reportViewer1.LocalReport.EnableExternalImages = true;
                       this.reportViewer1.LocalReport.SetParameters(new ReportParameter[] { PUrl });
                        
                       //this.YSS_INVOICE_LINETableAdapter.Fill(this.YSS_TESTDataSet.YSS_INVOICE_LINE, textBox1.Text);
                       this.ySS_WARRANTY_SERIALTableAdapter.Fill(this.ySS_DBLINKDataSet.YSS_WARRANTY_SERIAL, row.Cells["MARKINGCODE1"].Value.ToString());
                       this.reportViewer1.RefreshReport();
                       //this.reportViewer1.LocalReport.SetParameters[] {PUrl});
                       break;
                   }

                   //MessageBox.Show(row.Cells["MARKINGCODE1"].Value.ToString());
                   //MessageBox.Show(row.Cells["MARKINGCODE2"].Value.ToString());
                   X++;
               }
               byte[] bytes = reportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);
               FileStream fs = new FileStream(@"D:\ReportOutput.pdf", FileMode.Create);
               fs.Write(bytes, 0, bytes.Length);
               fs.Close();

               var pi = new ProcessStartInfo("D:\\ReportOutput.pdf");
               pi.UseShellExecute = true;
               pi.Verb = "print";
               var process = System.Diagnostics.Process.Start(pi);
            /*
            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string extension;


            this.ySS_WARRANTY_SERIALTableAdapter.Fill(this.ySS_DBLINKDataSet.YSS_WARRANTY_SERIAL, row.Cells["MARKINGCODE1"].Value.ToString());
            //ap = new Auto_Print();
            //YSS_TESTDataSet

            //ap.Main(YSS_TESTDataSet, "Report_Invoice.rdlc", reportViewer1, textBox1.Text);
            imageurl = "D:\\QRcode\\" + row.Cells["MARKINGCODE1"].Value.ToString() + ".png";
            //row.Cells["MARKINGCODE1"].Value = X;
            CreateQrcode(row.Cells["MARKINGCODE1"].Value.ToString(), row.Cells["MARKINGCODE2"].Value.ToString());
            FileInfo fi = new FileInfo(imageurl);
            ReportParameter PUrl = new ReportParameter("PUrl", new Uri(imageurl).AbsoluteUri);
            //ReportParameter serial = new ReportParameter("serial", row.Cells["MARKINGCODE1"].Value.ToString());
            this.reportViewer1.LocalReport.EnableExternalImages = true;
            this.reportViewer1.LocalReport.SetParameters(new ReportParameter[] { PUrl });
            this.reportViewer1.LocalReport.SetParameters(SP);
            this.reportViewer1.RefreshReport();

            byte[] bytes = reportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);
            FileStream fs = new FileStream(@"D:\ReportOutput.pdf", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();

            var pi = new ProcessStartInfo("D:\\ReportOutput.pdf");
            pi.UseShellExecute = true;
            pi.Verb = "print";
            var process = System.Diagnostics.Process.Start(pi);
             */
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            btnopen.Enabled = false;
            btnclose.Enabled = true;
            try
            {
                serialPort1.PortName = cboport.Text;
                serialPort1.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnopen.Enabled = true;
                btnclose.Enabled = false;
            }*/

            try
            {
                //serialPort1.Close();
                
                //tcpclient.Connect("192.168.1.234", 50002);\

                btnopen.Enabled = false;
                client.Connect(textIPServer.Text, Convert.ToInt32(cboport.Text));

                //tcpclient.Connect(textIPServer.Text, 50002);
                MessageBox.Show("Connect Data");
                btnsend.Enabled = true;
                labelStatus.Text = "CONNECT";
                labelStatus.BackColor = Color.FromArgb(0, 255, 0);
            }
            catch (Exception ex)
            {
                btnsend.Enabled = false;
                labelStatus.Text = "NOT CONNECT";
                labelStatus.BackColor = Color.FromArgb(255, 0, 0);
                
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // btnopen.Enabled = true;
                // btnclose.Enabled = false;
            }
        }

        private void btnclose_Click(object sender, EventArgs e)
        {
            //tcpclient.Close();
            client.TcpClient.Close();
            btnsend.Enabled = false;
            labelStatus.Text = "NOT CONNECT";
            labelStatus.BackColor = Color.FromArgb(255, 0, 0);

           // btnopen.Enabled = true;
           // btnclose.Enabled = false;
           
        }

        private void btnsend_Click(object sender, EventArgs e)
        {           
            /*try
            {
                if(serialPort1.IsOpen)
                {
                    serialPort1.WriteLine(txtMessage.Text + Environment.NewLine);
                    txtMessage.Clear();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);                
            }*/
            /*NetworkStream serverStream = tcpclient.GetStream();
            MessageBox.Show("1");
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(txtMessage.Text + "$");
            MessageBox.Show("2");
            serverStream.Write(outStream, 0, outStream.Length);
            MessageBox.Show("3");
            serverStream.Flush();
            MessageBox.Show("4");
            */

            client.WriteLine("WX,ProgramNo=" + txtMessage.Text);
            
            //txtMessage.Text = "";
            //txtMessage.Focus();

            /*byte[] inStream = new byte[10025];
            serverStream.Read(inStream, 0, (int)tcpclient.ReceiveBufferSize);
            string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            msg(returndata);
            txtMessage.Text = "";
            txtMessage.Focus();*/
        }


        public void msg(string mesg)
        {
            txtreceive.Text = txtreceive.Text + Environment.NewLine + " >> " + mesg;
        } 

        private void btnreceive_Click(object sender, EventArgs e)
        {
            /*try
            {
                if (serialPort1.IsOpen)
                {
                    txtreceive.Text = serialPort1.ReadExisting();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
            try
            {
                NetworkStream serverStream = tcpclient.GetStream();
                MessageBox.Show("5");

                byte[] inStream = new byte[4096];

                MessageBox.Show("6");
               
                serverStream.Read(inStream, 0, (int)tcpclient.ReceiveBufferSize);
                MessageBox.Show("7");

                string returndata = System.Text.Encoding.ASCII.GetString(inStream);

                MessageBox.Show("8");

                msg(returndata);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
             
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {           
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();                   
                }            
        }

        public void Load_Production()
        {

            try
            {
                // timer2.Enabled = false;

                //SqlConnection SQL_connectionStr;

                //SQL_connectionStr = new SqlConnection(strConnString);
                //SQL_connectionStr.Open();
                conn.Open();

                SqlCommand CmdSQL = conn.CreateCommand();




                CmdSQL.CommandText = "Select PRODID from YSS_PRODTABLE where PRODSTATUS = 'Started' and  DEPARTMENT = '" + ConfigurationManager.ConnectionStrings["yss_department"].ConnectionString + "' and STARTDATE <= GETDATE()";
                SqlDataReader ReaderSQL = CmdSQL.ExecuteReader();
                while (ReaderSQL.Read())
                {
                    comboBoxPD.Items.Add(ReaderSQL.GetString(0));
                }
                ReaderSQL.Close();


                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show(ex.Message);
                conn.Close();
                // timer2.Enabled = true;
                // DynAx.Logoff();
            }

            //ComboboxItem item = new ComboboxItem();
            //item.Text = "Item text1";
            //item.Value = 12;

            // comboBoxPD.Items.Add("PD20010669");

            //comboBoxPD.SelectedIndex = 0;

            //comboBoxPD.Items.Clear();

            //comboBoxPD.Items[1] = "PD20010669";
            //comboBoxPD.Items[2] = "PD20010670";
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {

        }

        private void btnsendSer_Click(object sender, EventArgs e)
        {
            client.WriteLine("WX,PRG=" + txtMessage.Text + ",BLK=" + txtBlock.Text + ",CharacterString=" + txtSerial.Text);
        }

        private void btnsnedStart_Click(object sender, EventArgs e)
        {
            client.WriteLine("WX,StartMarking");
        }

        
       
    }
}
