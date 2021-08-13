using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using SimpleTcp;
using ZXing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Reporting.WinForms;
using System.Dynamic;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Diagnostics;

namespace WarranteeForm
{
    public partial class Form1 : MaterialForm
    {
        SimpleTcpClient client;
        string connString = Properties.Settings.Default.ConnectionString;
        string department_id = Properties.Settings.Default.Department;
        string departments = Properties.Settings.Default.Departments;
        string image_path = Properties.Settings.Default.ImagePath;
        string WarrantyPdfFile = "";
        const string WarrantyCardsPath = "WarrantyCards";
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
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Amber800, Primary.Amber900, Primary.Amber500, Accent.LightBlue200, TextShade.WHITE);
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
        
        private SqlConnection g_conn;
        private SqlTransaction g_trans;
        private IEnumerator<Button> Procedures;
        private IEnumerator<string> Cmds;
        
        private bool ConnectMarkingMachine()
        {
            client = new SimpleTcpClient(tbHost.Text, int.Parse(tbPort.Text));
            client.Events.Connected += Connected;
            client.Events.Disconnected += Disconnected;
            client.Events.DataReceived += DataReceived;
            try
            {
                client.Connect();
                return true;
            }
            catch (Exception ex)
            {
                client.Events.Connected -= Connected;
                client.Events.Disconnected -= Disconnected;
                client.Events.DataReceived -= DataReceived;
                MessageBox.Show(ex.Message, "ไม่สามารถติดต่อกับเครื่อง Marking ได้", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        private void GetDepartments()
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
                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);

                DataTable dt = new DataTable
                {
                    Locale = CultureInfo.InvariantCulture
                };
                da.Fill(dt);
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
                    Select distinct t1.ID, t1.PRODID, t1.ITEMID, t1.ITEMNAME, t1.QTY, t1.UNITID
                        , convert(varchar(5),t1.STARTDATE,3) + ' ' + convert(varchar,t1.STARTTIME,24) as STARTED
                        , t3.KBA_Use , ( SELECT KBANumber FROM YSS_PRODUCT_ABE WHERE t3.KBA_Use = 'YES' AND LEFT(t1.ITEMID,6) LIKE '%' + Condition + '%') as KBANumber
                        , t1.INVENTSTLYEID , t1.DEPARTMENT, t1.ITEMSEARCHNAME, t1.STARTDATE, t1.STARTTIME
                        , t4.YSSACTIVEABE
                        , t5.INVOICEADDRESSCOUNTRYREGIONID
                        , t6.region
                    From YSS_PRODTABLE t1
                        LEFT JOIN YSS_PROD_JOBCARD t2 on t1.id = t2.prod_id
                        LEFT JOIN YSS_PRODUCT_ABE_TRANS t3 ON t1.InventStlyeId = t3.DistributorCode
                        LEFT JOIN [YSS_BYOD]..[YSSProductDetailStaging] t4 on t1.ITEMID = t4.ITEMID collate Thai_CI_AS
                        LEFT JOIN [YSS_BYOD]..[CustCustomerV3Staging] t5 on t5.DEFAULTINVENTORYSTATUSID collate Thai_CI_AS = t1.InventStlyeId and t1.InventStlyeId <> '0000' 
                        LEFT JOIN [YSS_DBLINK]..[YSS_REGION_COUNTRY] t6 on t6.[alpha-3] collate Thai_CI_AS = t5.INVOICEADDRESSCOUNTRYREGIONID
                    Where t1.MARKINGCODE IS NOT NULL 
                        --AND t1.PRODSTATUS = 'Started' 
                        --AND t1.WaitEndPD = 0
                        --AND t2.ID IS NOT NULL  
                        --AND isnull(T2.ToTime,'') <> ''
                        --AND t1.DEPARTMENT = '{departmentid}'
                        AND t1.PRODID in ('PD20057350','PD20021516','PD21035816','PD20012095','PD20027383','PD20040577'
                                        ,'PD21038039','PD20048540','PD20012145','PD20012999','PD20014117')

                    ORDER BY t1.PRODID, t1.STARTDATE, t1.STARTTIME";
                SqlDataAdapter da = new SqlDataAdapter(sqlCmd);
                BindingSource bs = new BindingSource();
                DataTable dt = new DataTable
                {
                    Locale = CultureInfo.InvariantCulture
                };
                da.Fill(dt);
                bs.DataSource = dt.DefaultView;
                cbx_production_order.DisplayMember = "PRODID";
                cbx_production_order.ValueMember = "PRODID";
                cbx_production_order.DataSource = bs;
            }
        }
        private dynamic GetProductionOrder(string prod_id)
        {
            BindingSource bs = cbx_production_order.DataSource as BindingSource;
            bs.Position = bs.Find("PRODID", prod_id);
            DataRowView dr = bs.Current as DataRowView;
            dynamic order = new ExpandoObject();
            order.prodId = prod_id;
            order.distributor = dr["INVENTSTLYEID"].ToString().Trim();
            order.kbaNumber = dr["KBANUMBER"].ToString().Trim();
            order.kbaActive = dr["YSSACTIVEABE"].ToString();
            order.itemId = dr["ITEMID"].ToString().Trim();
            order.itemName = dr["ITEMNAME"].ToString().Trim();
            order.unitId = dr["UNITID"].ToString().Trim();
            order.inventStyleId = order.distributor.Contains("0000") ? "" : order.distributor;
            order.kbaNumber = order.distributor.Contains("0000") || order.kbaActive == "0" ? "" : order.kbaNumber;
            order.kbaUse = dr["KBA_USE"].ToString() == "YES" ? true : false;
            //
            order.qty = dr["QTY"].ToString();
            order.activeAbe = dr["YSSACTIVEABE"].ToString() == "1";
            order.region = dr["region"].ToString().Trim();
            return order;
        }
        private void ShowProductionInfo(dynamic order)
        {
            txItemId.Text = order.itemId;
            txItemName.Text = order.itemName;
            txUnitId.Text = order.unitId;
            txInventStyleId.Text = order.inventStyleId;
            txKBANumber.Text = order.kbaNumber;
            cbx_kba_use.Checked = order.kbaUse;
            //
            pictureBox1.LoadAsync(image_path + txItemId.Text + ".jpg");
        }
        private dynamic GetMarkingInfo(dynamic order) 
        {
            DataTable dtSerials = GetSerials(order.prodId);
            order.serialFrom = dtSerials.AsEnumerable().Min(r => r.Field<string>("MARKINGCODE"));
            order.serialTo = dtSerials.AsEnumerable().Max(r => r.Field<string>("MARKINGCODE"));
            order.marked = dtSerials.AsEnumerable().Count(r => r.Field<DateTime?>("FINISHED_AT").HasValue);

            tbx_left_serial.Text = tbx_right_serial.Text = string.Empty;
            tbx_left_serial.Tag = tbx_right_serial.Tag = string.Empty;
            if (order.marked < Int16.Parse(order.qty) )
            {
                int count = order.unitId == "PAIR" ? 2:1;
                var tbxs = new List<TextBox>() { tbx_left_serial, tbx_right_serial };
                var markSerials = GetMarkSerials(dtSerials, g_conn, g_trans, count);
                foreach (var item in markSerials.Select((value, i) => new { i, value }))
                {
                    string[] values = item.value.Split(',');
                    tbxs[item.i].Text = values[1];
                    tbxs[item.i].Tag = values[0];
                }
            }
            return order;
        }
        private void ShowMarkingInfo(dynamic order) 
        { 
            tbx_serial_from.Text = order.serialFrom;
            tbx_serial_to.Text = order.serialTo;
            txQty.Text = $"{order.marked}/{order.qty}";
            cbx_abe_active.Checked = order.activeAbe;
            tbx_region.Text = order.region;
        }
        private void GetPatternCode(string prodid, string itemId)
        {
            //itemId = "MO302-300T-66-858";
            DataTable dt = new DataTable { Locale=CultureInfo.InvariantCulture };
            var sql = $@"
               select distinct top 1 t1.ITEMID fg_item_id, t4.item_id, t4.product_series, t4.unit_id, t4.part_type, t4.pattern_code, t5.InventSizeId, t1.TXTPRODUCTION
                from [YSS_DBLINK]..YSS_VIEW_PRODUCT_SERIES t1
                    join [YSS_BYOD].[dbo].[BOMBillOfMaterialsVersionV3Staging] t2 on t2.MANUFACTUREDITEMNUMBER = t1.ITEMID
                    join [YSS_BYOD].[dbo].[BOMBillOfMaterialsLineV3Staging] t3 on t3.BOMID = t2.BOMID
                    join [YSS_DBLINK]..YSS_LASER_PATTERN t4 on t4.item_id = t3.ITEMNUMBER collate Thai_CI_AS
                        and t4.unit_id = t1.BOMUNITID collate Thai_CI_AS
                        and t4.product_series = t1.SERIES
                        and t4.TxtProduction = t1.Txtproduction
                    join [YSS_DBLINK]..YSS_PRODTABLE t5 on t5.ITEMID = t1.ITEMID collate Thai_CI_AS
                where t1.itemid = '{itemId}'
                    and t5.PRODID = '{prodid}'
                    and not (t5.InventSizeId = 'R50' and t4.part_type = 'RESERVOIR') --Except Japan Edition
                order by t4.part_type";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            if (dt.Rows.Count > 0)
            {
                dt.AsEnumerable().ToList().ForEach(r =>
                {
                    tbx_pattern_code.Text = r.Field<string>("pattern_code");
                    tbx_pattern_code.BackColor = Color.Yellow;
                });
            }
            else
            {
                ToggleButton(false);
                MessageBox.Show("ไม่พบ Pattern Code ของรหัส " + txItemId.Text, "ค้นหา Pattern Code ผิดพลาด:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private IEnumerator<Button> CreateProcedures()
        {
            var procedures = new List<Button>();
            if (tbx_pattern_code.Text != "N.A.")
                procedures.Add(btnMark);
            procedures.Add(btnPrint);
            foreach(var btn in procedures)
                yield return btn;
        }
        private DataTable GetSerials(string prod_id)
        {
            DataTable dt = new DataTable { Locale=CultureInfo.InvariantCulture };
            var sql = $@"Select t1.ID, t1.MARKINGCODE, t2.FINISHED_AT
                From YSS_MARKINGCODE t1
                    Left Join YSS_MARKING_FINISHED t2 ON t1.ID = t2.MARKINGCODE_ID
                Where PRODID = '{prod_id}'
                ORDER BY t1.MARKINGCODE";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }
        private List<string> GetMarkSerials(DataTable dt, SqlConnection conn, SqlTransaction trans, int count)
        {
            List<string> serials = new List<string>();
            EnumerableRowCollection<DataRow> rows = dt.AsEnumerable();
            var markingCodes = rows
                .Where(r=> r.Field<DateTime?>("FINISHED_AT").HasValue == false)
                .Select(r =>
                    new {
                        id = r.Field<Int64>("ID"),
                        markingCode = r.Field<string>("MARKINGCODE")
                    }).ToList();

            var cmd = g_conn.CreateCommand();
            cmd.Transaction = g_trans;
            cmd.CommandText = "select @@TRANCOUNT";
            var trancount = Convert.ToInt32(cmd.ExecuteScalar());
            if (trancount > 0)
                g_trans.Commit();
            
            g_trans = g_conn.BeginTransaction();
            cmd = g_conn.CreateCommand();
            cmd.Transaction = g_trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_getapplock";
            cmd.Parameters.Add(new SqlParameter("@LockMode", "Exclusive"));
            cmd.Parameters.Add(new SqlParameter("@Resource", "?"));
            foreach( var item in markingCodes) 
            {
                cmd.Parameters["@Resource"].Value = item.markingCode;
                if (isAppLock(item.markingCode, g_conn, g_trans) == false)
                {
                    Convert.ToInt32(cmd.ExecuteScalar());
                    serials.Add($"{item.id},{item.markingCode}");
                }
                if (serials.Count == count) break;
            };
            return serials;
        }
        private bool isAppLock(string resource, SqlConnection conn, SqlTransaction trans)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = $@"select count(*) 
                from sys.dm_tran_locks
                where resource_type = 'APPLICATION'
                    AND request_mode = 'X'
                    AND request_status = 'GRANT'
                    AND resource_description LIKE '%:\[{resource}\]:%' ESCAPE '\'";
            return (Convert.ToInt32(cmd.ExecuteScalar()) > 0);
        }
        private void releaseAppLock(string resource, SqlConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = g_trans;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_releaseapplock";
            cmd.Parameters.AddWithValue("@Resource", resource);
            cmd.ExecuteNonQuery();
        }
        private void Marking()
        {
            markingState = State.Initial;
            CollectMarkingData(txUnitId.Text);
            SendMarkingData();
            WarrantyPdfFile = $@"{tbx_left_serial.Text}{(string.IsNullOrEmpty(tbx_right_serial.Text) ? "" : "-" + tbx_right_serial.Text)}.PDF";
        }
        private void CollectMarkingData(string unitId)
        {
            string blocks = $"BLK=0001,CharacterString={tbx_left_serial.Text}";         //serial
            if (txInventStyleId.TextLength > 0) //Export
            {
                blocks += $",BLK=0002,CharacterString={txInventStyleId.Text.Trim()}";   //distributor code
                if (cbx_kba_use.Checked)
                {
                    blocks += $",BLK=0003,CharacterString={txItemId.Text}";             //item code
                    blocks += $",BLK=0004,CharacterString={txKBANumber.Text}";          //kba
                }
            }
            if (unitId == "PAIR")
            {
                blocks += $"BLK=0011,CharacterString={tbx_right_serial.Text}";      //serial
                if (txInventStyleId.TextLength > 0) //Export
                {
                    blocks += $",BLK=0012,CharacterString={txInventStyleId.Text.Trim()}";//distributor code
                    if (cbx_kba_use.Checked)
                    {
                        blocks += $",BLK=0013,CharacterString={txItemId.Text}";         //item code
                        blocks += $",BLK=0014,CharacterString={txKBANumber.Text}";      //kba
                    }
                }
            }
            Cmds = GetMarkingCmds(tbx_pattern_code.Text, blocks);
        }
        private void SendMarkingData()
        {
            if (Cmds.MoveNext())
            {
                client.Send($"{Cmds.Current}{Environment.NewLine}");
                Logger($"Request => {Cmds.Current}");
            } else {
                //1 set serials Mark Finished
                if (Cmds.Current == "WX,StartMarking")
                {
                    InsertMarkedAt();
                    releaseAppLock(tbx_left_serial.Text, g_conn);
                    if (tbx_right_serial.Text != string.Empty)
                        releaseAppLock(tbx_right_serial.Text, g_conn);
                }
                this.Invoke(() => Procedures.Current.Enabled = false);
                //Enable Print Button
                if (Procedures.MoveNext())
                    this.Invoke(() => Procedures.Current.Enabled = true);
            }
        }
        private IEnumerator<string> GetMarkingCmds(string pattern_code, string blocks)
        {
            foreach(var cmd in new List<string>
            {
                $"WX,ProgramNo={pattern_code}",
                $"WX,PRG={pattern_code},{blocks}",
//                $"WX,PRG={pattern_code},BLK=0001,MarkingEnable=1,11111111111111",
                $"WX,StartMarking"
            }) yield return cmd;
        }
        private List<WarrantyCard> GetWarrantyCards()
        {
            List<WarrantyCard> cards = new List<WarrantyCard>();
            var serials = WarrantyPdfFile.Split('.')[0].Replace('-',',');
            var qrcode = CreateQrcode(serials);
            var img = new Bitmap(qrcode);
            var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            cards.Add(new WarrantyCard
            {
                product_code = txItemId.Text,
                serial_numbers = serials,
                qr_code = ms.ToArray()
            });
            return cards; 
        }
        private Bitmap CreateQrcode(String serial)
        {
            var WarrantyUrl = Properties.Settings.Default.WarrantyUrl;
            var qrWriter = new BarcodeWriter { 
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 130,
                    Height = 130
                }
            };
            return qrWriter.Write(WarrantyUrl + serial);
        }
        private void InsertMarkedAt()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                var sql = $@"INSERT INTO YSS_MARKING_FINISHED(MARKINGCODE_ID) VALUES ({tbx_left_serial.Tag})";
                sql += tbx_right_serial.Tag.ToString() != string.Empty ? $",({tbx_right_serial.Tag})" : "";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
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
        private void PrintWarrantyCard()
        {
            Directory.CreateDirectory(WarrantyCardsPath);
            var pdfPath = $@"{WarrantyCardsPath}\{WarrantyPdfFile}";
            ReportViewer reportViewer = new ReportViewer();
            reportViewer.LocalReport.ReportPath = @"WarrantyCard.rdlc";
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("WarrantyCardsDS", GetWarrantyCards()));
            reportViewer.RefreshReport();
            byte[] pdf = reportViewer.LocalReport.Render("PDF");
            using(var fs = new FileStream(pdfPath, FileMode.OpenOrCreate,FileAccess.Write, FileShare.Write))
            {
                fs.Write(pdf, 0, pdf.Length);
                fs.Flush();
                fs.Close();
            }
        }
        private void UpdateWarrantyCard(Int32 warranty_card_id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                foreach (var item in new List<TextBox>(){ tbx_left_serial, tbx_right_serial})
                {
                    var id = item.Tag.ToString();
                    if (!string.IsNullOrEmpty(id))
                    {
                        cmd.CommandText = $@"

                            UPDATE YSS_MARKING_FINISHED
                            SET WARRANTY_CARD_ID = {warranty_card_id} 
                            WHERE MARKINGCODE_ID = {id}";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        private void Logger(string msg)
        {
            new LogWriter(msg);
            listBox1.Invoke(() => listBox1.Items.Add($"{DateTime.Now}: {msg}"));
        }
        private void ToggleButton(bool value)
        {
            btnPrint.Invoke(() => btnPrint.Enabled = value);
            btnMark.Invoke(() => btnMark.Enabled = value);
        }
        private void PrintToPrinter(string filePath)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = @"powershell.exe";
            processInfo.Arguments = @"& {Get-Content .\" + filePath + " | Out-Printer}";
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += $" [Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}]";
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, lbStatus.Width - 4, lbStatus.Height - 4);
            lbStatus.Region = new Region(path);
            ToggleButton(false);
            markingState = State.StandBy;
            //
            g_conn = new SqlConnection(connString);
            g_conn.Open();
            //
            btnDisconnect.Enabled = false;
            // production order info. always disabled
            txItemId.Enabled = txItemName.Enabled = txQty.Enabled = false;
            txUnitId.Enabled = txKBANumber.Enabled = txInventStyleId.Enabled = false;
            tbx_serial_from.Enabled = tbx_serial_to.Enabled = tbx_region.Enabled = false;
            tbx_left_serial.Enabled = tbx_right_serial.Enabled = false;
            cbx_kba_use.Enabled = false;
            cbx_abe_active.Enabled = false;
            // pattern code always disabled
            tbx_pattern_code.Enabled = false;
            //
            btnPrint.Enabled = false;
            //
            tbHost.Text = Properties.Settings.Default.Ip;
            tbPort.Text = Properties.Settings.Default.Port;
            //
            pictureBox1.LoadCompleted += PictureBox1_LoadCompleted;
            //
            btnConnect_Click(null, null);
            GetDepartments();
            cbDepartment_SelectedValueChanged(cbDepartment, null);
            cbProductionOrder_SelectedIndexChanged(cbx_production_order, null);
            cbDepartment.SelectedValueChanged += cbDepartment_SelectedValueChanged;
            cbx_production_order.SelectedIndexChanged += cbProductionOrder_SelectedIndexChanged;
            //
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool connected = ConnectMarkingMachine();
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            ToggleButton(connected);
        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            ToggleButton(false);
        }
        private void btnMark_Click(object sender, EventArgs e)
            => Marking();
        private void btnPrint_Click(object sender, EventArgs e)
        {
            var id = InsertPrintedAt();
            Procedures.Current.Enabled = false;
            PrintWarrantyCard();
            PrintToPrinter(WarrantyPdfFile);
            UpdateWarrantyCard(id);
            //
            dynamic order = new ExpandoObject();
            order.prodId = cbx_production_order.Text;
            order.qty = txQty.Text.Split('/')[1];
            order.unitId = txUnitId.Text;
            order.activeAbe = cbx_abe_active.Checked;
            order.region = tbx_region.Text;
            GetMarkingInfo(order);
            ShowMarkingInfo(order);
            if (order.marked < Int16.Parse(order.qty))
            {
                Procedures = CreateProcedures();
                Procedures.MoveNext();
                Procedures.Current.Enabled = true;
            }
            else
            {
                MessageBox.Show("Finished !!!");
            }
        }
        private void btnRefreshPd_Click(object sender, EventArgs e)
        {
            cbDepartment_SelectedValueChanged(cbDepartment, null);
        }

        private void cbDepartment_SelectedValueChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            var departmentid = cb.SelectedValue.ToString();
            GetProductionOrders(departmentid);
        }
        private void cbProductionOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prod_id = (sender as ComboBox).SelectedValue.ToString();
            var order = GetProductionOrder(prod_id);
            ShowProductionInfo(order);
            order = GetMarkingInfo(order);
            ShowMarkingInfo(order);
            if (order.marked < Int16.Parse(order.qty))
            {
                GetPatternCode(order.prodId, order.itemId);
                Procedures = CreateProcedures();
                Procedures.MoveNext();
                Procedures.Current.Enabled = true;
            } else
            {
                MessageBox.Show("Finished !!!");
            }
        }

        private void PictureBox1_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                PictureBox pic = (PictureBox)sender;
                var image = new Bitmap(pic.Width, pic.Height);
                var font = new Font("Tahoma", 16, FontStyle.Bold, GraphicsUnit.Pixel);
                var graphics = Graphics.FromImage(image);
                graphics.DrawString(e.Error.Message, font, Brushes.Red, new Point(0,pic.Height/2));
                pic.Image = image;
            }
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
                cb.Text = string.Empty;
        }
        private void DataReceived(object sender, SimpleTcp.DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            if (data.Contains(WXOK))
            {
                Logger(data);
                SendMarkingData();
            }
            else if (data.Contains(WXNG))
            {
                Logger($"Error: {data} State: {Enum.GetName(typeof(State), markingState)}");
            }
        }
        private void Disconnected(object sender, ClientDisconnectedEventArgs e)
        {
            lbStatus.Invoke(() =>
            {
                lbStatus.Text = $"Disconnected: {String.Format("{0:MMM-dd hh:mm:ss}", DateTime.Now)}";
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
            new LogWriter("Connected.");
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
