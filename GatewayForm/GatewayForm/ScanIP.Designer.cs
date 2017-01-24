namespace GatewayForm
{
    partial class ScanIP
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
            this.label1 = new System.Windows.Forms.Label();
            this.network_card_cbx = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ipAddressControl1 = new IPAddressControlLib.IPAddressControl();
            this.ipAddressControl2 = new IPAddressControlLib.IPAddressControl();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ip_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.host_col = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status_scanning_lb = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Network Adapter:";
            // 
            // network_card_cbx
            // 
            this.network_card_cbx.FormattingEnabled = true;
            this.network_card_cbx.Location = new System.Drawing.Point(109, 56);
            this.network_card_cbx.Name = "network_card_cbx";
            this.network_card_cbx.Size = new System.Drawing.Size(104, 21);
            this.network_card_cbx.TabIndex = 0;
            this.network_card_cbx.SelectedIndexChanged += new System.EventHandler(this.network_card_cbx_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(230, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Range:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(400, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "-->";
            // 
            // ipAddressControl1
            // 
            this.ipAddressControl1.AllowInternalTab = false;
            this.ipAddressControl1.AutoHeight = true;
            this.ipAddressControl1.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControl1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressControl1.Location = new System.Drawing.Point(280, 56);
            this.ipAddressControl1.MinimumSize = new System.Drawing.Size(99, 21);
            this.ipAddressControl1.Name = "ipAddressControl1";
            this.ipAddressControl1.ReadOnly = false;
            this.ipAddressControl1.Size = new System.Drawing.Size(117, 21);
            this.ipAddressControl1.TabIndex = 1;
            this.ipAddressControl1.Text = "192.168.1.0";
            this.ipAddressControl1.Click += new System.EventHandler(this.ipAddressControl1_Click);
            this.ipAddressControl1.Enter += new System.EventHandler(this.ipAddressControl1_Enter);
            this.ipAddressControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipAddressControl1_KeyDown);
            this.ipAddressControl1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ipAddressControl1_KeyUp);
            // 
            // ipAddressControl2
            // 
            this.ipAddressControl2.AllowInternalTab = false;
            this.ipAddressControl2.AutoHeight = true;
            this.ipAddressControl2.BackColor = System.Drawing.SystemColors.Window;
            this.ipAddressControl2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ipAddressControl2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ipAddressControl2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressControl2.Location = new System.Drawing.Point(420, 56);
            this.ipAddressControl2.MinimumSize = new System.Drawing.Size(99, 21);
            this.ipAddressControl2.Name = "ipAddressControl2";
            this.ipAddressControl2.ReadOnly = false;
            this.ipAddressControl2.Size = new System.Drawing.Size(117, 21);
            this.ipAddressControl2.TabIndex = 2;
            this.ipAddressControl2.Text = "192.168.1.255";
            this.ipAddressControl2.Click += new System.EventHandler(this.ipAddressControl2_Click);
            this.ipAddressControl2.Enter += new System.EventHandler(this.ipAddressControl2_Enter);
            this.ipAddressControl2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ipAddressControl2_KeyDown);
            this.ipAddressControl2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ipAddressControl2_KeyUp);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(25, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 30);
            this.button1.TabIndex = 13;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ip_col,
            this.host_col});
            this.dataGridView1.Location = new System.Drawing.Point(15, 110);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(575, 302);
            this.dataGridView1.TabIndex = 14;
            this.dataGridView1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
            // 
            // ip_col
            // 
            this.ip_col.HeaderText = "IP";
            this.ip_col.Name = "ip_col";
            this.ip_col.ReadOnly = true;
            this.ip_col.Width = 200;
            // 
            // host_col
            // 
            this.host_col.HeaderText = "Host Name";
            this.host_col.Name = "host_col";
            this.host_col.ReadOnly = true;
            this.host_col.Width = 300;
            // 
            // status_scanning_lb
            // 
            this.status_scanning_lb.AutoSize = true;
            this.status_scanning_lb.Location = new System.Drawing.Point(16, 440);
            this.status_scanning_lb.Name = "status_scanning_lb";
            this.status_scanning_lb.Size = new System.Drawing.Size(62, 13);
            this.status_scanning_lb.TabIndex = 11;
            this.status_scanning_lb.Text = "scan/range";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(195, 433);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(402, 22);
            this.progressBar1.TabIndex = 15;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(421, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 28);
            this.button2.TabIndex = 16;
            this.button2.Text = "Connect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ScanIP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 457);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.network_card_cbx);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.status_scanning_lb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ipAddressControl2);
            this.Controls.Add(this.ipAddressControl1);
            this.Name = "ScanIP";
            this.Text = "ScanIP";
            this.Load += new System.EventHandler(this.ScanIP_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox network_card_cbx;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private IPAddressControlLib.IPAddressControl ipAddressControl2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ip_col;
        private System.Windows.Forms.DataGridViewTextBoxColumn host_col;
        private System.Windows.Forms.Label status_scanning_lb;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button2;
    }
}