namespace GatewayForm
{
    partial class Tcp_pop
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TcpTimeout_tx = new System.Windows.Forms.TextBox();
            this.Tcp_len_tx = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.static_btn = new System.Windows.Forms.RadioButton();
            this.dhcp_btn = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.TcpIP_tx = new System.Windows.Forms.TextBox();
            this.TcpPort_tx = new System.Windows.Forms.TextBox();
            this.gateway_tx = new System.Windows.Forms.TextBox();
            this.Subnet_tx = new System.Windows.Forms.TextBox();
            this.close_btn = new System.Windows.Forms.Button();
            this.update_btn = new System.Windows.Forms.Button();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.TcpTimeout_tx);
            this.groupBox4.Controls.Add(this.Tcp_len_tx);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Location = new System.Drawing.Point(12, 220);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(241, 100);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Packet Properties";
            this.groupBox4.Enter += new System.EventHandler(this.groupBox4_Enter);
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
            // TcpTimeout_tx
            // 
            this.TcpTimeout_tx.Location = new System.Drawing.Point(83, 55);
            this.TcpTimeout_tx.Name = "TcpTimeout_tx";
            this.TcpTimeout_tx.Size = new System.Drawing.Size(109, 20);
            this.TcpTimeout_tx.TabIndex = 1;
            this.TcpTimeout_tx.Text = "50";
            // 
            // Tcp_len_tx
            // 
            this.Tcp_len_tx.Location = new System.Drawing.Point(84, 20);
            this.Tcp_len_tx.Name = "Tcp_len_tx";
            this.Tcp_len_tx.Size = new System.Drawing.Size(108, 20);
            this.Tcp_len_tx.TabIndex = 1;
            this.Tcp_len_tx.Text = "1024";
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
            this.groupBox3.Controls.Add(this.static_btn);
            this.groupBox3.Controls.Add(this.dhcp_btn);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.TcpIP_tx);
            this.groupBox3.Controls.Add(this.TcpPort_tx);
            this.groupBox3.Controls.Add(this.gateway_tx);
            this.groupBox3.Controls.Add(this.Subnet_tx);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(241, 202);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Connection Properties";
            // 
            // static_btn
            // 
            this.static_btn.AutoSize = true;
            this.static_btn.Location = new System.Drawing.Point(123, 95);
            this.static_btn.Name = "static_btn";
            this.static_btn.Size = new System.Drawing.Size(52, 17);
            this.static_btn.TabIndex = 2;
            this.static_btn.TabStop = true;
            this.static_btn.Text = "Static";
            this.static_btn.UseVisualStyleBackColor = true;
            this.static_btn.CheckedChanged += new System.EventHandler(this.static_btn_CheckedChanged);
            // 
            // dhcp_btn
            // 
            this.dhcp_btn.AutoSize = true;
            this.dhcp_btn.Location = new System.Drawing.Point(9, 95);
            this.dhcp_btn.Name = "dhcp_btn";
            this.dhcp_btn.Size = new System.Drawing.Size(55, 17);
            this.dhcp_btn.TabIndex = 2;
            this.dhcp_btn.TabStop = true;
            this.dhcp_btn.Text = "DHCP";
            this.dhcp_btn.UseVisualStyleBackColor = true;
            this.dhcp_btn.CheckedChanged += new System.EventHandler(this.dhcp_btn_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 163);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Gateway";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Net Mask";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 26);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Address";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 66);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(26, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Port";
            // 
            // TcpIP_tx
            // 
            this.TcpIP_tx.Location = new System.Drawing.Point(76, 23);
            this.TcpIP_tx.Name = "TcpIP_tx";
            this.TcpIP_tx.Size = new System.Drawing.Size(124, 20);
            this.TcpIP_tx.TabIndex = 1;
            this.TcpIP_tx.Text = "192.168.1.118";
            // 
            // TcpPort_tx
            // 
            this.TcpPort_tx.Location = new System.Drawing.Point(75, 59);
            this.TcpPort_tx.Name = "TcpPort_tx";
            this.TcpPort_tx.Size = new System.Drawing.Size(124, 20);
            this.TcpPort_tx.TabIndex = 1;
            this.TcpPort_tx.Text = "5000";
            // 
            // gateway_tx
            // 
            this.gateway_tx.Enabled = false;
            this.gateway_tx.Location = new System.Drawing.Point(76, 160);
            this.gateway_tx.Name = "gateway_tx";
            this.gateway_tx.Size = new System.Drawing.Size(123, 20);
            this.gateway_tx.TabIndex = 1;
            this.gateway_tx.Text = "192.168.1.1";
            // 
            // Subnet_tx
            // 
            this.Subnet_tx.Enabled = false;
            this.Subnet_tx.Location = new System.Drawing.Point(76, 126);
            this.Subnet_tx.Name = "Subnet_tx";
            this.Subnet_tx.Size = new System.Drawing.Size(124, 20);
            this.Subnet_tx.TabIndex = 1;
            this.Subnet_tx.Text = "255.255.255.0";
            // 
            // close_btn
            // 
            this.close_btn.Location = new System.Drawing.Point(191, 326);
            this.close_btn.Name = "close_btn";
            this.close_btn.Size = new System.Drawing.Size(62, 23);
            this.close_btn.TabIndex = 10;
            this.close_btn.Text = "Cancel";
            this.close_btn.UseVisualStyleBackColor = true;
            this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
            // 
            // update_btn
            // 
            this.update_btn.Location = new System.Drawing.Point(95, 326);
            this.update_btn.Name = "update_btn";
            this.update_btn.Size = new System.Drawing.Size(75, 23);
            this.update_btn.TabIndex = 9;
            this.update_btn.Text = "OK";
            this.update_btn.UseVisualStyleBackColor = true;
            this.update_btn.Click += new System.EventHandler(this.update_btn_Click);
            // 
            // Tcp_pop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 360);
            this.Controls.Add(this.close_btn);
            this.Controls.Add(this.update_btn);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Name = "Tcp_pop";
            this.Text = "Tcp_pop";
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TcpTimeout_tx;
        private System.Windows.Forms.TextBox Tcp_len_tx;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox TcpIP_tx;
        private System.Windows.Forms.TextBox TcpPort_tx;
        private System.Windows.Forms.TextBox gateway_tx;
        private System.Windows.Forms.TextBox Subnet_tx;
        private System.Windows.Forms.RadioButton static_btn;
        private System.Windows.Forms.RadioButton dhcp_btn;
        private System.Windows.Forms.Button close_btn;
        private System.Windows.Forms.Button update_btn;
    }
}