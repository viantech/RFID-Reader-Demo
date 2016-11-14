namespace GatewayForm
{
    partial class Zigbee_pop
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ZigbeeIP_tx = new System.Windows.Forms.TextBox();
            this.ZigbeePort_tx = new System.Windows.Forms.TextBox();
            this.PanID_tx = new System.Windows.Forms.TextBox();
            this.EPID_tx = new System.Windows.Forms.TextBox();
            this.ZigbeeChannel_tx = new System.Windows.Forms.TextBox();
            this.DeviceID_tx = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Ztimeout_tx = new System.Windows.Forms.TextBox();
            this.Zmax_len_tx = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.update_btn = new System.Windows.Forms.Button();
            this.close_btn = new System.Windows.Forms.Button();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.ZigbeeIP_tx);
            this.groupBox3.Controls.Add(this.ZigbeePort_tx);
            this.groupBox3.Controls.Add(this.PanID_tx);
            this.groupBox3.Controls.Add(this.EPID_tx);
            this.groupBox3.Controls.Add(this.ZigbeeChannel_tx);
            this.groupBox3.Controls.Add(this.DeviceID_tx);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(241, 202);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Connection Properties";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 156);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "EPID";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 127);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Channel";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Device ID";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 19);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Address";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 46);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(26, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Port";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pan ID";
            // 
            // ZigbeeIP_tx
            // 
            this.ZigbeeIP_tx.Location = new System.Drawing.Point(68, 19);
            this.ZigbeeIP_tx.Name = "ZigbeeIP_tx";
            this.ZigbeeIP_tx.Size = new System.Drawing.Size(124, 20);
            this.ZigbeeIP_tx.TabIndex = 1;
            this.ZigbeeIP_tx.Text = "192.168.0.105";
            // 
            // ZigbeePort_tx
            // 
            this.ZigbeePort_tx.Location = new System.Drawing.Point(69, 46);
            this.ZigbeePort_tx.Name = "ZigbeePort_tx";
            this.ZigbeePort_tx.Size = new System.Drawing.Size(123, 20);
            this.ZigbeePort_tx.TabIndex = 1;
            this.ZigbeePort_tx.Text = "4096";
            // 
            // PanID_tx
            // 
            this.PanID_tx.Location = new System.Drawing.Point(69, 72);
            this.PanID_tx.Name = "PanID_tx";
            this.PanID_tx.Size = new System.Drawing.Size(123, 20);
            this.PanID_tx.TabIndex = 1;
            this.PanID_tx.Text = "C525";
            // 
            // EPID_tx
            // 
            this.EPID_tx.Location = new System.Drawing.Point(68, 156);
            this.EPID_tx.Name = "EPID_tx";
            this.EPID_tx.Size = new System.Drawing.Size(124, 20);
            this.EPID_tx.TabIndex = 1;
            this.EPID_tx.Text = "57934F06ADD5745A";
            // 
            // ZigbeeChannel_tx
            // 
            this.ZigbeeChannel_tx.Location = new System.Drawing.Point(69, 127);
            this.ZigbeeChannel_tx.Name = "ZigbeeChannel_tx";
            this.ZigbeeChannel_tx.Size = new System.Drawing.Size(123, 20);
            this.ZigbeeChannel_tx.TabIndex = 1;
            this.ZigbeeChannel_tx.Text = "25";
            // 
            // DeviceID_tx
            // 
            this.DeviceID_tx.Location = new System.Drawing.Point(68, 101);
            this.DeviceID_tx.Name = "DeviceID_tx";
            this.DeviceID_tx.Size = new System.Drawing.Size(124, 20);
            this.DeviceID_tx.TabIndex = 1;
            this.DeviceID_tx.Text = "C9DE";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.Ztimeout_tx);
            this.groupBox4.Controls.Add(this.Zmax_len_tx);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Location = new System.Drawing.Point(12, 220);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(241, 100);
            this.groupBox4.TabIndex = 5;
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
            this.Zmax_len_tx.Text = "72";
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
            // update_btn
            // 
            this.update_btn.Location = new System.Drawing.Point(96, 326);
            this.update_btn.Name = "update_btn";
            this.update_btn.Size = new System.Drawing.Size(75, 23);
            this.update_btn.TabIndex = 7;
            this.update_btn.Text = "OK";
            this.update_btn.UseVisualStyleBackColor = true;
            this.update_btn.Click += new System.EventHandler(this.update_btn_Click);
            // 
            // close_btn
            // 
            this.close_btn.Location = new System.Drawing.Point(191, 326);
            this.close_btn.Name = "close_btn";
            this.close_btn.Size = new System.Drawing.Size(62, 23);
            this.close_btn.TabIndex = 8;
            this.close_btn.Text = "Cancel";
            this.close_btn.UseVisualStyleBackColor = true;
            this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
            // 
            // Zigbee_pop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 359);
            this.Controls.Add(this.close_btn);
            this.Controls.Add(this.update_btn);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Name = "Zigbee_pop";
            this.Text = "Zigbee Setting Form";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private  System.Windows.Forms.GroupBox groupBox3;
        private  System.Windows.Forms.Label label2;
        private  System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PanID_tx;
        private System.Windows.Forms.TextBox DeviceID_tx;
        private  System.Windows.Forms.GroupBox groupBox4;
        private  System.Windows.Forms.Label label6;
        private  System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Ztimeout_tx;
        private System.Windows.Forms.TextBox Zmax_len_tx;
        private  System.Windows.Forms.Label label5;
        private  System.Windows.Forms.Label label3;
        private  System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox EPID_tx;
        private System.Windows.Forms.TextBox ZigbeeChannel_tx;
        private  System.Windows.Forms.Label label8;
        private  System.Windows.Forms.Label label10;
        private  System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox ZigbeeIP_tx;
        private System.Windows.Forms.TextBox ZigbeePort_tx;
        private  System.Windows.Forms.Button update_btn;
        private  System.Windows.Forms.Button close_btn;
    }
}