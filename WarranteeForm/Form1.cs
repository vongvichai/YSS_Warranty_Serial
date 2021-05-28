using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using SimpleTcp;

using System.IO;

using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rectangle = System.Drawing.Rectangle;

namespace WarranteeForm
{
    public partial class Form1 : Form
    {
        SimpleTcpClient client;
        string connString = Properties.Settings.Default.ConnectionString;
        string department_id = Properties.Settings.Default.Department;
        int markedIndex = -1;
        enum State
        {
            StandBy,
            Initial,
            Programmed,
            Blocked,
            OneOrTwoSide,
            Marked,
            Printed
        }
        const string WXNG = "WX,NG";
        const string WXOK = "WX,OK";
        State markingState;
        public Form1()
        {
            InitializeComponent();
        }
        #region Functions
        /*
         * 1. Get Departments
         * 2. Get Production Orders
         * 3. Get Production Order
         * 4. Show Production Order Detail
         * 4.1 Item Details and Image
         * 4.2 Get/Show Marking Data
         * 5. Select Unmarked Serial Numbers
         * 6. Connect Marking Machine
         * 7. Send Marking Data
         * 8. Set Mark Finished
         * 9. Insert Marked At 
         * 10. Print Warranty Card
         * 11. InsertPrintedAt
         * 12. Update Warranty Card
         * * */
        private void GetDepartments(string departments)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = conn;
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = $@"
                    Select DEPARTMENTID, DESCRIPTION From YSS_DEPARTMENT
                    Where DEPARTMENTID IN ({departments})";
                SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd);

                DataTable dt = new DataTable
                {
                    Locale = CultureInfo.InvariantCulture
                };
                sqlDataAdap.Fill(dt);
                cbDepartment.DataSource = dt;
                cbDepartment.DisplayMember = "DESCRIPTION";
                cbDepartment.ValueMember = "DEPARTMENTID";
            }
        }
        private void GetProductionOrders(string departmentid)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Connection = conn;
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = $@"
                    Select t1.ID, t1.PRODID, t1.ITEMID, t1.ITEMNAME, t1.QTY, t1.UNITID
                        , convert(varchar(5),t1.STARTDATE,3) + ' ' + convert(varchar,t1.STARTTIME,24) as STARTED
	                    , t3.KBA_Use , ( SELECT KBANumber FROM YSS_PRODUCT_ABE WHERE t3.KBA_Use = 'YES' AND LEFT(t1.ITEMID,6) LIKE '%' + Condition + '%') as KBANumber
                        , t1.INVENTSTLYEID , t1.DEPARTMENT, t1.ITEMSEARCHNAME
                    From YSS_PRODTABLE t1
                        LEFT JOIN YSS_PROD_JOBCARD t2 on t1.id = t2.prod_id
	                    LEFT JOIN YSS_PRODUCT_ABE_TRANS t3 ON t1.InventStlyeId = t3.DistributorCode
                    Where t1.MARKINGCODE IS NOT NULL 
                        AND t1.PRODSTATUS = 'Started' 
                        AND t2.ID IS NOT NULL  
                        AND isnull(T2.ToTime,'') <> ''
                        AND t1.DEPARTMENT = '{departmentid}'
                    ORDER BY t1.STARTDATE, t1.STARTTIME";
                SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd);
                BindingSource bs = new BindingSource();
                DataTable dt = new DataTable
                {
                    Locale = CultureInfo.InvariantCulture
                };
                sqlDataAdap.Fill(dt);
                bs.DataSource = dt.DefaultView;
                dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                dataGridView1.DataSource = bs;
            }                
        }
        private void GetProductionOrder()
        {
            if (dataGridView1.RowCount > 0)
            {
                markedIndex = -1;
                string prodid = dataGridView1.CurrentRow.Cells["PRODID"].Value.ToString();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand sqlCmd = new SqlCommand();
                    sqlCmd.Connection = conn;
                    sqlCmd.CommandType = CommandType.Text;
                    sqlCmd.CommandText = $@"
                        Select t1.ID, t1.MARKINGCODE, convert(varchar,t2.FINISHED_AT,20) AS FINISHED_AT, t3.RUNNUMBER
                            , convert(varchar,t3.PRINTED_AT,20) AS PRINTED_AT
                        From YSS_MARKINGCODE t1
                           Left Join YSS_MARKING_FINISHED t2 ON t1.ID = t2.MARKINGCODE_ID
                           Left Join YSS_WARRANTY_CARD t3 ON t3.ID = t2.WARRANTY_CARD_ID
                        Where PRODID = '{ prodid }'
                        ORDER BY t1.MARKINGCODE";
                    SqlDataAdapter sqlDataAdap = new SqlDataAdapter(sqlCmd);

                    DataTable dt = new DataTable();
                    sqlDataAdap.Fill(dt);
                    dataGridView2.DataSource = dt.DefaultView;
                }
                dataGridView2.ClearSelection();
            }
        }
        private void ShowItemDetails()
        {
            var dvr = dataGridView1.CurrentRow as DataGridViewRow;
            var distributor = dvr.Cells["INVENTSTLYEID"].Value.ToString();
            var kbaNumber = dvr.Cells["KBANUMBER"].Value.ToString();
            txProdId.Text = dvr.Cells["PRODID"].Value.ToString();
            txItemId.Text = dvr.Cells["ITEMID"].Value.ToString();
            txItemName.Text = dvr.Cells["ITEMNAME"].Value.ToString();
            txItemSearchName.Text = dvr.Cells["ITEMSEARCHNAME"].Value.ToString();
            txQty.Text = dvr.Cells["QTY"].Value.ToString();
            txUnitId.Text = dvr.Cells["UNITID"].Value.ToString();
            txInventStyleId.Text = distributor.Contains("0000") ? "":distributor;
            txKBANumber.Text = distributor.Contains("0000") ? "":kbaNumber;
            cbSetLeft.Text = cbSetRight.Text = "";
            cbSetLeft.Checked = cbSetRight.Checked = false;
            var rows = dataGridView2.SelectedRows;
            cbSetLeft.Enabled = false;
            cbSetRight.Enabled = txUnitId.Text.Contains("PCS");
            if (dataGridView2.SelectedRows.Count == 2)
            {
                cbSetLeft.Text = rows[1].Cells["MARKINGCODE"].Value.ToString();
                cbSetRight.Text = rows[0].Cells["MARKINGCODE"].Value.ToString();
                cbSetLeft.Checked = cbSetRight.Checked = true;
            }
            else
            {
                cbSetLeft.Text = rows[0].Cells["MARKINGCODE"].Value.ToString();
                cbSetLeft.Checked = true;
            }
        }
        private void GetMarkingData(string item_number)
        {
            // Table design
        }
        private void SelectUnmarkSerial(DataGridView dgv)
        {
            if (markedIndex == -1)
            {
                dgv.ClearSelection();
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.Cells["FINISHED_AT"].Value.ToString().Equals(String.Empty))
                    {
                        markedIndex = row.Index;
                        row.Selected = true;
                        dgv.CurrentCell = row.Cells[0];
                        if (markedIndex + 1 < dgv.Rows.Count)
                        {
                            dgv.Rows[++markedIndex].Selected = true;
                        }
                        break;
                    }
                }
            } else
            {
                if (markedIndex + 1 < dgv.Rows.Count)
                {
                    dgv.Rows[++markedIndex].Selected = true;
                    dgv.CurrentCell = dgv.Rows[markedIndex].Cells[0];
                    if (markedIndex + 1 < dgv.Rows.Count)
                    {
                        dgv.Rows[++markedIndex].Selected = true;
                    }
                } 
            }
        }
        private bool ConnectMarkingMachine()
        {
            client = new SimpleTcpClient(tbHost.Text,int.Parse(tbPort.Text));
            client.Events.Connected += Connected;
            client.Events.Disconnected += Disconnected;
            client.Events.DataReceived += DataReceived;
            try
            {
                client.Connect();
                return true;
            } catch (Exception ex)
            {
                client.Events.Connected -= Connected;
                client.Events.Disconnected -= Disconnected;
                client.Events.DataReceived -= DataReceived;
                MessageBox.Show(ex.Message, "ไม่สามารถติดต่อกับเครื่อง Marking ได้");
            }
            return false;
        }
        private void SendMarkingData()
        {
            var msg = Enum.GetName(typeof(State), markingState);
            string cmd = String.Empty;
            var programNo = txProgramNo.Text;
            string[] leftBlocks = { "000", "001", "002", "003", "004", "005", "006", "007", "008", "009" };
            string[] rightBlocks = { "010", "011", "012", "013", "014", "015", "016", "017", "018", "019" };
            var rows = dataGridView2.SelectedRows;
            //
            if (markingState.Equals(State.Initial)) //Initial State
            {
                cmd = $"WX,ProgramNo={programNo}";
                client.Send($"{cmd}{Environment.NewLine}");
            }
            else if (markingState.Equals(State.Programmed))    //Programmed
            {
                cmd = $"WX,PRG={programNo}";
                if (cbSetLeft.Checked)
                {
                    cmd += $@",BLK={leftBlocks[1]},CharacterString={cbSetLeft.Text}";
                    cmd += $@",BLK={leftBlocks[2]},CharacterString={txItemId.Text}";
                    cmd += $@",BLK={leftBlocks[3]},CharacterString={txInventStyleId.Text.Trim()}";
                    cmd += $@",BLK={leftBlocks[4]},CharacterString={txKBANumber.Text}";
                }
                if (cbSetRight.Checked)
                {
                    cmd += $",BLK={rightBlocks[1]},CharacterString={cbSetRight.Text}";
                    cmd += $@",BLK={rightBlocks[2]},CharacterString={txItemId.Text}";
                    cmd += $@",BLK={rightBlocks[3]},CharacterString={txInventStyleId.Text.Trim()}";
                    cmd += $@",BLK={rightBlocks[4]},CharacterString={txKBANumber.Text}";
                }
                client.Send($"{cmd}{Environment.NewLine}");
            }
            else if (markingState == State.Blocked)
            {
                /* *
                 * ตัวอย่าง:รูปแบบการ Mark ข้างเดียว
                 * WX,ProgNo=0001,BLK=006,MarkingEnable=0,111
                 * => 0 = Off ทุก Block
                 * => 111 = On ตั้งแต่ Block ที่ 6,7,8 
                 * */
                cmd = $"WX,PRG={programNo}";
                if (cbSetLeft.Checked && cbSetRight.Checked)
                    cmd += $",BLK=000,MarkingEnable=1,1111111111";
                else if (cbSetLeft.Checked)
                    cmd += $",BLK=000,MarkingEnable=0,11111";
                else //(cbSetRight.Checked)
                    cmd += $",BLK=011,MarkingEnable=0,11111";

                client.Send($"{cmd}{Environment.NewLine}");
            }
            else if (markingState.Equals(State.OneOrTwoSide))
            {
                cmd = "WX,StartMarking";
                client.Send($"{cmd}{Environment.NewLine}");
            }
            else if (markingState.Equals(State.Marked)) // Marked
            {
                //Print Warranty Card
                cmd = "WX,Print Warranty Card";
                btnStop_Click(null, null);
            }
            Logger($"{msg}=>{cmd}");
        }
        private void SetMarkFinished(DataGridView dgv)
        {
            foreach(DataGridViewRow row in dgv.SelectedRows)
            {
                row.Cells["FINISHED_AT"].Value = DateTime.Now;
            }
        }
        private void InsertMarkedAt(DataGridView dgv)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                var id = "";
                var sql = $@"INSERT INTO YSS_MARKING_FINISHED(MARKINGCODE_ID) VALUES ";
                foreach (DataGridViewRow row in dgv.SelectedRows) {
                    id = row.Cells["ID"].Value.ToString();
                    sql += $"({id}), ";
                }
                cmd.CommandText = sql.Remove(sql.Length - 2, 2);
                cmd.ExecuteNonQuery();
            }
        }
        private void PrintWarrantyCard()
        {
            // Nui: Implement
        }
        private Int32 InsertPrintedAt()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO YSS_WARRANTY_CARD (RUNNUMBER) VALUES (NEXT VALUE FOR dbo.WarrantyRunNumber);
                    SELECT CAST(scope_identity() AS int)";
                return (Int32)cmd.ExecuteScalar();
            }

        }
        private void Logger(string msg)
        {
            new LogWriter(msg);
            listBox1.Invoke(() => listBox1.Items.Add($"{DateTime.Now}: {msg}"));
        }
        private void ToggleButton(bool value)
        {
            btnStart.Invoke(() => btnStart.Enabled = value);
            btnStop.Invoke(() => btnStop.Enabled = value);
            btnMarking.Invoke(() => btnMarking.Enabled = value);
            btnPrint.Invoke(() => btnPrint.Enabled = value);
        }
        private void UpdateWarrantyCard(DataGridView dgv, Int32 warranty_card_id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                var id = "";
                foreach (DataGridViewRow row in dgv.SelectedRows)
                {
                    id = row.Cells["ID"].Value.ToString();
                    cmd.CommandText = $@"
                        UPDATE YSS_MARKING_FINISHED
                        SET WARRANTY_CARD_ID = {warranty_card_id} 
                        WHERE MARKINGCODE_ID = {id}";
                   cmd.ExecuteNonQuery();
                }
            }
        }
        private void SetPrintFinished(DataGridView dgv, Int32 warranty_card_id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                Int32? runnumber = null;
                DateTime? printed_at = null; 
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = $@"SELECT ID, RUNNUMBER, PRINTED_AT 
                    FROM YSS_WARRANTY_CARD 
                    WHERE ID = {warranty_card_id}";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    runnumber = Convert.ToInt32(reader.GetValue(1));
                    printed_at = reader.GetDateTime(2);
                }
                foreach (DataGridViewRow row in dgv.SelectedRows)
                {
                    row.Cells["RUNNUMBER"].Value = runnumber;
                    row.Cells["PRINTED_AT"].Value = printed_at;
                }
            }
        }
        #endregion
        private void btnConnect_Click(object sender, EventArgs e)
        {
            
            bool connected = ConnectMarkingMachine();
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            markingState = State.Initial;
            cbSetProgram.Checked = true;
            //btnStop.Enabled = !(btnStart.Enabled = dataGridView1.Enabled = false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += $" [Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}]";
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, lbStatus.Width -4, lbStatus.Height -4);
            lbStatus.Region = new Region(path);
            ToggleButton(false);
            markingState = State.StandBy;
            //
            dataGridView1.ReadOnly = true;
            dataGridView2.ReadOnly = true;
            dataGridView1.RowPostPaint += dgv_RowPostPaint;
            dataGridView2.RowPostPaint += dgv_RowPostPaint;
            var departments = department_id == String.Empty ? "'6122','6123','6124','6125','6126'" : $"'{department_id}'";
            GetDepartments(departments);
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            cbDepartment.SelectedValueChanged += CbDepartment_SelectedValueChanged;
            CbDepartment_SelectedValueChanged(cbDepartment, new EventArgs());
            btnDisconnect.Enabled = false;
            //btnStop.Enabled = false;
            txProdId.Enabled = txItemId.Enabled = txItemName.Enabled = txItemSearchName.Enabled = txQty.Enabled = false;
            txUnitId.Enabled = txProgramNo.Enabled = txKBANumber.Enabled = txInventStyleId.Enabled = false;
            cbSetProgram.Enabled = cbSetBlock.Enabled = cbSetSide.Enabled = cbSetMark.Enabled = cbSetPrintCard.Enabled = false;
            //
            cbSetProgram.CheckedChanged += StateChangedEvent;
            cbSetBlock.CheckedChanged += StateChangedEvent;
            cbSetSide.CheckedChanged += StateChangedEvent;
            cbSetRight.MouseClick += OneSideChangedEvent;
            cbSetMark.CheckedChanged += StateChangedEvent;
            cbSetPrintCard.CheckedChanged += StateChangedEvent;
            //
            btnConnect_Click(null, null);
        }

        private void dgv2_SelectionChanged(object sender, EventArgs e)
        {
            var dgv = sender as DataGridView;
            MessageBox.Show(e.ToString());
        }

        private void StateChangedEvent(object sender, EventArgs e)
        {
            if (markingState != State.StandBy)
                SendMarkingData();
        }

        private void OneSideChangedEvent(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb.Checked == false)
            {
                cb.Text = string.Empty;
                dataGridView2.Rows[markedIndex--].Selected = false;
            } else
            {
                dataGridView2.Rows[++markedIndex].Selected = true;
                cb.Text = dataGridView2.Rows[markedIndex].Cells["MARKINGCODE"].Value.ToString();
            }
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            var msg = String.Empty;
            if (data.Contains(WXOK))
            {
                switch (markingState)
                {
                    case State.Initial:
                        markingState = State.Programmed;
                        cbSetBlock.Invoke(() => cbSetBlock.Checked = true);
                        break;
                    case State.Programmed:
                        markingState = State.Blocked;
                        cbSetSide.Invoke(() => cbSetSide.Checked = true);
                        break;
                    case State.Blocked:
                        markingState = State.OneOrTwoSide;
                        cbSetMark.Invoke(() => cbSetMark.Checked = true);
                        break;
                    case State.OneOrTwoSide:
                        markingState = State.Marked;
                        cbSetPrintCard.Invoke(() => cbSetPrintCard.Checked = true);
                        break;
                    case State.Marked:
                        markingState = State.Printed;
                        break;
                    case State.Printed:
                        markingState = State.Programmed;
                        break;
                }
            }
            else if (data.Contains(WXNG))
            {
                msg = $"Error: {data} State: {Enum.GetName(typeof(State),markingState)}";
                Logger(msg);
            }
        }

        private void Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            lbStatus.Invoke(() =>
            {
                lbStatus.Text = $"Disconnected: {String.Format("{0:MMM-dd hh:mm:ss}",DateTime.Now)}";
                lbStatus.BackColor = Color.Red;
            });
            client.Events.Connected -= Connected;
            client.Events.Disconnected -= Disconnected;
            client.Events.DataReceived -= DataReceived;
            ToggleButton(false);
            new LogWriter("Disconnected.");
        }

        private void Connected(object sender, ClientConnectedEventArgs e)
        {
            lbStatus.Invoke(() =>
            {
                lbStatus.Text = $"Connected: {String.Format("{0:MMM-dd hh:mm:ss}", DateTime.Now)}";
                lbStatus.BackColor = Color.Green;
            });
            ToggleButton(true);
            new LogWriter("Connected.");
        }

        private void CbDepartment_SelectedValueChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            var departmentid = cb.SelectedValue.ToString();
            GetProductionOrders(departmentid);
            var bs = dataGridView1.DataSource as BindingSource;
            if (bs.Count > 0) bs.Position = 0;
            dataGridView1_SelectionChanged(dataGridView1, new EventArgs());
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv.SelectedRows.Count > 0)
            {
                GetProductionOrder();
                SelectUnmarkSerial(dataGridView2);
                ShowItemDetails();
            }
            else
            {
                dataGridView2.DataSource = null;
                txProdId.Text = txItemId.Text = txItemName.Text = txQty.Text = txUnitId.Text = "";
            }
        }

        private void dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dv = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, dv.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SetMarkFinished(dataGridView2);
            InsertMarkedAt(dataGridView2);
            markingState = State.StandBy;
            dataGridView2.ClearSelection();
            cbSetLeft.Text = cbSetRight.Text = string.Empty;
            cbSetProgram.Checked = cbSetBlock.Checked = cbSetSide.Checked = cbSetMark.Checked = cbSetPrintCard.Checked = false;
            if (markedIndex < dataGridView2.Rows.Count - 1)
            {
                SelectUnmarkSerial(dataGridView2);
                ShowItemDetails();
            }
            //btnStop.Enabled = !(btnStart.Enabled = dataGridView1.Enabled = true);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            var id = InsertPrintedAt();
            
            Print_Warranty();
            UpdateWarrantyCard(dataGridView2, id);
            SetPrintFinished(dataGridView2, id);

        }


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
            System.Net.WebResponse response = default(System.Net.WebResponse);
            Stream remoteStream = default(Stream);
            StreamReader readStream = default(StreamReader);
            System.Net.WebRequest request = System.Net.WebRequest.Create(url);

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

        private void Print_Warranty()
        {
            int i = 0, j=0;
            if (txUnitId.Text == "PAIR")
            {
                i = 1;
                CreateQrcode(cbSetLeft.Text, cbSetRight.Text);
            }
            else
            {
                if(cbSetRight.Text == "")
                {
                    i = 1;
                    CreateQrcode(cbSetLeft.Text, "");
                }
                else
                {
                    i = 2;
                    CreateQrcode(cbSetLeft.Text, "");
                    CreateQrcode(cbSetRight.Text, "");
                }
            }
            
            for (j = 0; j <= i;j++);
            {               

                Document doc = new Document(PageSize.A5.Rotate());

                BaseFont arial = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                iTextSharp.text.Font f_15_Bold = new iTextSharp.text.Font(arial, 15, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font f_12_Narmal = new iTextSharp.text.Font(arial, 12, iTextSharp.text.Font.NORMAL);

                Random rnd = new Random();
                int name = rnd.Next(1, 1000);
                FileStream os = new FileStream("OUTPUT.pdf", FileMode.Create);

                using (os)
                {
                    PdfWriter.GetInstance(doc, os);
                    doc.Open();

                    //Information about company
                    PdfPTable table1 = new PdfPTable(1);
                    float[] width = new float[] { 40f, 60f };

                    System.Drawing.Image pImage = System.Drawing.Image.FromFile("D:\\QRcode\\B1304148.png");
                    iTextSharp.text.Image ItextImage = iTextSharp.text.Image.GetInstance(pImage, System.Drawing.Imaging.ImageFormat.Png);
                    ItextImage.Alignment = Element.ALIGN_BOTTOM;
                    ItextImage.WidthPercentage = 10;




                    PdfPCell cel1 = new PdfPCell(new Phrase("\n " + cbSetLeft.Text, f_12_Narmal));
                    PdfPCell cel2 = new PdfPCell(new Phrase("\n " + cbSetRight.Text, f_12_Narmal));
                    PdfPCell cel3 = new PdfPCell(new Phrase("\n " + txItemId.Text, f_12_Narmal));


                    cel1.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cel2.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cel3.Border = iTextSharp.text.Rectangle.NO_BORDER;


                    cel1.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cel2.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cel3.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                    table1.WidthPercentage = 40;
                    table1.HorizontalAlignment = Element.ALIGN_LEFT;
                    table1.AddCell(cel1);
                    table1.AddCell(cel2);
                    table1.AddCell(cel3);

                    //doc.Add(ItextImage);

                    table1.SpacingAfter = 20;
                    table1.SpacingBefore = 10;


                    PdfPTable table2 = new PdfPTable(1);
                    table2.AddCell(ItextImage);
                    table2.WidthPercentage = 30;
                    table2.HorizontalAlignment = Element.ALIGN_RIGHT;

                    doc.Add(table1);
                    doc.Add(table2);


                    doc.Close();
                    //Open document 
                    //System.Diagnostics.Process.Start(@"OUTPUT.pdf");

                    var pi = new System.Diagnostics.ProcessStartInfo(@"OUTPUT.pdf");//"D:\\OUTPUT.pdf");
                    pi.UseShellExecute = true;
                    pi.Verb = "print";
                    var process = System.Diagnostics.Process.Start(pi);
                }
           }
        }

        private void btnMarking_Click(object sender, EventArgs e)
        {
            string cmd = "WX,StartMarking";
            client.Send($"{cmd}{Environment.NewLine}");
            listBox1.Items.Add(cmd);
        }

    }
    public static class ControlExtensions
    {
        public static void Invoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(action), null);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}
