namespace YSS_Warranty_Serial
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource3 = new Microsoft.Reporting.WinForms.ReportDataSource();
            this.YSS_WARRANTY_SERIALBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ySS_DBLINKDataSet = new YSS_Warranty_Serial.YSS_DBLINKDataSet();
            this.buttonSearchPD = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.reportViewer1 = new Microsoft.Reporting.WinForms.ReportViewer();
            this.comboBoxPD = new System.Windows.Forms.ComboBox();
            this.ySSWARRANTYSERIALBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ySS_WARRANTY_SERIALTableAdapter = new YSS_Warranty_Serial.YSS_DBLINKDataSetTableAdapters.YSS_WARRANTY_SERIALTableAdapter();
            this.ySSDBLINKDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnopen = new System.Windows.Forms.Button();
            this.btnclose = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnsend = new System.Windows.Forms.Button();
            this.btnreceive = new System.Windows.Forms.Button();
            this.txtreceive = new System.Windows.Forms.TextBox();
            this.cboport = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.TxtItem = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textIPServer = new System.Windows.Forms.TextBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnsendSer = new System.Windows.Forms.Button();
            this.btnsnedStart = new System.Windows.Forms.Button();
            this.txtBlock = new System.Windows.Forms.TextBox();
            this.txtSerial = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.YSS_WARRANTY_SERIALBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySS_DBLINKDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySSWARRANTYSERIALBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySSDBLINKDataSetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // YSS_WARRANTY_SERIALBindingSource
            // 
            this.YSS_WARRANTY_SERIALBindingSource.DataMember = "YSS_WARRANTY_SERIAL";
            this.YSS_WARRANTY_SERIALBindingSource.DataSource = this.ySS_DBLINKDataSet;
            // 
            // ySS_DBLINKDataSet
            // 
            this.ySS_DBLINKDataSet.DataSetName = "YSS_DBLINKDataSet";
            this.ySS_DBLINKDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // buttonSearchPD
            // 
            this.buttonSearchPD.Location = new System.Drawing.Point(289, 33);
            this.buttonSearchPD.Name = "buttonSearchPD";
            this.buttonSearchPD.Size = new System.Drawing.Size(75, 23);
            this.buttonSearchPD.TabIndex = 0;
            this.buttonSearchPD.Text = "SEARCH";
            this.buttonSearchPD.UseVisualStyleBackColor = true;
            this.buttonSearchPD.Click += new System.EventHandler(this.buttonSearchPD_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "PRODUCTION :";
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(451, 35);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(93, 23);
            this.buttonClear.TabIndex = 5;
            this.buttonClear.Text = "CLEAR DATA";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(370, 34);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 8;
            this.buttonStart.Text = "START";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(2, 91);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(627, 222);
            this.dataGridView1.TabIndex = 9;
            // 
            // reportViewer1
            // 
            reportDataSource3.Name = "DataSet1";
            reportDataSource3.Value = this.YSS_WARRANTY_SERIALBindingSource;
            this.reportViewer1.LocalReport.DataSources.Add(reportDataSource3);
            this.reportViewer1.LocalReport.ReportEmbeddedResource = "YSS_Warranty_Serial.ReportWarranty.rdlc";
            this.reportViewer1.Location = new System.Drawing.Point(2, 319);
            this.reportViewer1.Name = "reportViewer1";
            this.reportViewer1.Size = new System.Drawing.Size(885, 341);
            this.reportViewer1.TabIndex = 10;
            // 
            // comboBoxPD
            // 
            this.comboBoxPD.FormattingEnabled = true;
            this.comboBoxPD.Location = new System.Drawing.Point(103, 33);
            this.comboBoxPD.Name = "comboBoxPD";
            this.comboBoxPD.Size = new System.Drawing.Size(180, 21);
            this.comboBoxPD.TabIndex = 11;
            // 
            // ySSWARRANTYSERIALBindingSource
            // 
            this.ySSWARRANTYSERIALBindingSource.DataMember = "YSS_WARRANTY_SERIAL";
            this.ySSWARRANTYSERIALBindingSource.DataSource = this.ySS_DBLINKDataSet;
            // 
            // ySS_WARRANTY_SERIALTableAdapter
            // 
            this.ySS_WARRANTY_SERIALTableAdapter.ClearBeforeFill = true;
            // 
            // ySSDBLINKDataSetBindingSource
            // 
            this.ySSDBLINKDataSetBindingSource.DataSource = this.ySS_DBLINKDataSet;
            this.ySSDBLINKDataSetBindingSource.Position = 0;
            // 
            // btnopen
            // 
            this.btnopen.Location = new System.Drawing.Point(1015, 54);
            this.btnopen.Name = "btnopen";
            this.btnopen.Size = new System.Drawing.Size(75, 23);
            this.btnopen.TabIndex = 12;
            this.btnopen.Text = "CONNECT";
            this.btnopen.UseVisualStyleBackColor = true;
            this.btnopen.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnclose
            // 
            this.btnclose.Location = new System.Drawing.Point(1096, 52);
            this.btnclose.Name = "btnclose";
            this.btnclose.Size = new System.Drawing.Size(75, 23);
            this.btnclose.TabIndex = 14;
            this.btnclose.Text = "CLOSE";
            this.btnclose.UseVisualStyleBackColor = true;
            this.btnclose.Click += new System.EventHandler(this.btnclose_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(684, 108);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(115, 21);
            this.txtMessage.TabIndex = 15;
            // 
            // btnsend
            // 
            this.btnsend.Location = new System.Drawing.Point(1076, 108);
            this.btnsend.Name = "btnsend";
            this.btnsend.Size = new System.Drawing.Size(96, 23);
            this.btnsend.TabIndex = 16;
            this.btnsend.Text = "SEND PRG";
            this.btnsend.UseVisualStyleBackColor = true;
            this.btnsend.Click += new System.EventHandler(this.btnsend_Click);
            // 
            // btnreceive
            // 
            this.btnreceive.Location = new System.Drawing.Point(680, 197);
            this.btnreceive.Name = "btnreceive";
            this.btnreceive.Size = new System.Drawing.Size(75, 23);
            this.btnreceive.TabIndex = 18;
            this.btnreceive.Text = "RECEIVE";
            this.btnreceive.UseVisualStyleBackColor = true;
            this.btnreceive.Click += new System.EventHandler(this.btnreceive_Click);
            // 
            // txtreceive
            // 
            this.txtreceive.Location = new System.Drawing.Point(684, 139);
            this.txtreceive.Multiline = true;
            this.txtreceive.Name = "txtreceive";
            this.txtreceive.Size = new System.Drawing.Size(386, 52);
            this.txtreceive.TabIndex = 17;
            // 
            // cboport
            // 
            this.cboport.FormattingEnabled = true;
            this.cboport.Location = new System.Drawing.Point(875, 56);
            this.cboport.Name = "cboport";
            this.cboport.Size = new System.Drawing.Size(121, 21);
            this.cboport.TabIndex = 19;
            this.cboport.Text = "50002";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(826, 62);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "PORT :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(636, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "send";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(635, 142);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "receive";
            // 
            // TxtItem
            // 
            this.TxtItem.Enabled = false;
            this.TxtItem.Location = new System.Drawing.Point(103, 62);
            this.TxtItem.Name = "TxtItem";
            this.TxtItem.Size = new System.Drawing.Size(526, 20);
            this.TxtItem.TabIndex = 23;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(58, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "ITEM :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(636, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "IP ADDRESS :";
            // 
            // textIPServer
            // 
            this.textIPServer.Location = new System.Drawing.Point(720, 59);
            this.textIPServer.Name = "textIPServer";
            this.textIPServer.Size = new System.Drawing.Size(100, 20);
            this.textIPServer.TabIndex = 26;
            this.textIPServer.Text = "192.168.1.234";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.labelStatus.Location = new System.Drawing.Point(634, 26);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(121, 20);
            this.labelStatus.TabIndex = 27;
            this.labelStatus.Text = "NOT CONNECT";
            // 
            // btnsendSer
            // 
            this.btnsendSer.Location = new System.Drawing.Point(1076, 137);
            this.btnsendSer.Name = "btnsendSer";
            this.btnsendSer.Size = new System.Drawing.Size(96, 23);
            this.btnsendSer.TabIndex = 28;
            this.btnsendSer.Text = "SEND SERIAL";
            this.btnsendSer.UseVisualStyleBackColor = true;
            this.btnsendSer.Click += new System.EventHandler(this.btnsendSer_Click);
            // 
            // btnsnedStart
            // 
            this.btnsnedStart.Location = new System.Drawing.Point(1076, 166);
            this.btnsnedStart.Name = "btnsnedStart";
            this.btnsnedStart.Size = new System.Drawing.Size(96, 23);
            this.btnsnedStart.TabIndex = 29;
            this.btnsnedStart.Text = "SEND START";
            this.btnsnedStart.UseVisualStyleBackColor = true;
            this.btnsnedStart.Click += new System.EventHandler(this.btnsnedStart_Click);
            // 
            // txtBlock
            // 
            this.txtBlock.Location = new System.Drawing.Point(829, 108);
            this.txtBlock.Name = "txtBlock";
            this.txtBlock.Size = new System.Drawing.Size(100, 20);
            this.txtBlock.TabIndex = 30;
            // 
            // txtSerial
            // 
            this.txtSerial.Location = new System.Drawing.Point(956, 108);
            this.txtSerial.Name = "txtSerial";
            this.txtSerial.Size = new System.Drawing.Size(100, 20);
            this.txtSerial.TabIndex = 31;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(684, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Program";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(826, 89);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Block";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(953, 89);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(33, 13);
            this.label9.TabIndex = 34;
            this.label9.Text = "Serial";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1186, 672);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSerial);
            this.Controls.Add(this.txtBlock);
            this.Controls.Add(this.btnsnedStart);
            this.Controls.Add(this.btnsendSer);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.textIPServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.TxtItem);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboport);
            this.Controls.Add(this.btnreceive);
            this.Controls.Add(this.txtreceive);
            this.Controls.Add(this.btnsend);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnclose);
            this.Controls.Add(this.btnopen);
            this.Controls.Add(this.comboBoxPD);
            this.Controls.Add(this.reportViewer1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonSearchPD);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.YSS_WARRANTY_SERIALBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySS_DBLINKDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySSWARRANTYSERIALBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ySSDBLINKDataSetBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSearchPD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.DataGridView dataGridView1;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewer1;
        private YSS_DBLINKDataSet ySS_DBLINKDataSet;
        private System.Windows.Forms.BindingSource ySSWARRANTYSERIALBindingSource;
        private YSS_DBLINKDataSetTableAdapters.YSS_WARRANTY_SERIALTableAdapter ySS_WARRANTY_SERIALTableAdapter;
        private System.Windows.Forms.BindingSource ySSDBLINKDataSetBindingSource;
        private System.Windows.Forms.ComboBox comboBoxPD;
        private System.Windows.Forms.BindingSource YSS_WARRANTY_SERIALBindingSource;
        private System.Windows.Forms.Button btnopen;
        private System.Windows.Forms.Button btnclose;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnsend;
        private System.Windows.Forms.Button btnreceive;
        private System.Windows.Forms.TextBox txtreceive;
        private System.Windows.Forms.ComboBox cboport;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.TextBox TxtItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textIPServer;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btnsendSer;
        private System.Windows.Forms.Button btnsnedStart;
        private System.Windows.Forms.TextBox txtBlock;
        private System.Windows.Forms.TextBox txtSerial;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}

