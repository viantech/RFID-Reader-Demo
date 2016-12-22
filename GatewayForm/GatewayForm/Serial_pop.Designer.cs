namespace GatewayForm
{
    partial class Serial_pop
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
            this.close_btn = new System.Windows.Forms.Button();
            this.update_btn = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Ztimeout_tx = new System.Windows.Forms.TextBox();
            this.Zmax_len_tx = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.portname_tx = new System.Windows.Forms.TextBox();
            this.baudrate_tx = new System.Windows.Forms.TextBox();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // close_btn
            // 
            this.close_btn.Location = new System.Drawing.Point(201, 327);
            this.close_btn.Name = "close_btn";
            this.close_btn.Size = new System.Drawing.Size(62, 23);
            this.close_btn.TabIndex = 12;
            this.close_btn.Text = "Cancel";
            this.close_btn.UseVisualStyleBackColor = true;
            this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
            // 
            // update_btn
            // 
            this.update_btn.Location = new System.Drawing.Point(105, 327);
            this.update_btn.Name = "update_btn";
            this.update_btn.Size = new System.Drawing.Size(75, 23);
            this.update_btn.TabIndex = 11;
            this.update_btn.Text = "OK";
            this.update_btn.UseVisualStyleBackColor = true;
            this.update_btn.Click += new System.EventHandler(this.update_btn_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.Ztimeout_tx);
            this.groupBox4.Controls.Add(this.Zmax_len_tx);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Location = new System.Drawing.Point(22, 221);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(241, 100);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Packet Properties";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(203, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "ms";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(203, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "bytes";
            // 
            // Ztimeout_tx
            // 
            this.Ztimeout_tx.Location = new System.Drawing.Point(83, 55);
            this.Ztimeout_tx.Name = "Ztimeout_tx";
            this.Ztimeout_tx.Size = new System.Drawing.Size(109, 20);
            this.Ztimeout_tx.TabIndex = 1;
            this.Ztimeout_tx.Text = "50";
            // 
            // Zmax_len_tx
            // 
            this.Zmax_len_tx.Location = new System.Drawing.Point(84, 20);
            this.Zmax_len_tx.Name = "Zmax_len_tx";
            this.Zmax_len_tx.Size = new System.Drawing.Size(108, 20);
            this.Zmax_len_tx.TabIndex = 1;
            this.Zmax_len_tx.Text = "1024";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Timeout";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Max length";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.portname_tx);
            this.groupBox3.Controls.Add(this.baudrate_tx);
            this.groupBox3.Location = new System.Drawing.Point(22, 13);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(241, 202);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Connection Properties";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 22);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Port Name";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 58);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Baudrate";
            // 
            // portname_tx
            // 
            this.portname_tx.Location = new System.Drawing.Point(83, 19);
            this.portname_tx.Name = "portname_tx";
            this.portname_tx.Size = new System.Drawing.Size(124, 20);
            this.portname_tx.TabIndex = 1;
            this.portname_tx.Text = "/dev/ttyO1";
            // 
            // baudrate_tx
            // 
            this.baudrate_tx.Location = new System.Drawing.Point(83, 55);
            this.baudrate_tx.Name = "baudrate_tx";
            this.baudrate_tx.Size = new System.Drawing.Size(123, 20);
            this.baudrate_tx.TabIndex = 1;
            this.baudrate_tx.Text = "115200";
            // 
            // Serial_pop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(280, 362);
            this.Controls.Add(this.close_btn);
            this.Controls.Add(this.update_btn);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.MaximizeBox = false;
            this.Name = "Serial_pop";
            this.Text = "Serial Port";
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button close_btn;
        private System.Windows.Forms.Button update_btn;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Ztimeout_tx;
        private System.Windows.Forms.TextBox Zmax_len_tx;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox portname_tx;
        private System.Windows.Forms.TextBox baudrate_tx;
    }
}