namespace GatewayForm
{
    partial class setup_connect
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
            this.label2 = new System.Windows.Forms.Label();
            this.address_IP_tx = new System.Windows.Forms.TextBox();
            this.port_IP_tx = new System.Windows.Forms.TextBox();
            this.ok_btn = new System.Windows.Forms.Button();
            this.Cancel_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP/Hostname:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Port:";
            // 
            // address_IP_tx
            // 
            this.address_IP_tx.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.address_IP_tx.Location = new System.Drawing.Point(128, 17);
            this.address_IP_tx.Name = "address_IP_tx";
            this.address_IP_tx.Size = new System.Drawing.Size(150, 25);
            this.address_IP_tx.TabIndex = 1;
            this.address_IP_tx.Text = "192.168.1.100";
            // 
            // port_IP_tx
            // 
            this.port_IP_tx.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.port_IP_tx.Location = new System.Drawing.Point(128, 57);
            this.port_IP_tx.Name = "port_IP_tx";
            this.port_IP_tx.Size = new System.Drawing.Size(150, 25);
            this.port_IP_tx.TabIndex = 1;
            this.port_IP_tx.Text = "5000";
            // 
            // ok_btn
            // 
            this.ok_btn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ok_btn.Location = new System.Drawing.Point(129, 117);
            this.ok_btn.Name = "ok_btn";
            this.ok_btn.Size = new System.Drawing.Size(75, 23);
            this.ok_btn.TabIndex = 2;
            this.ok_btn.Text = "OK";
            this.ok_btn.UseVisualStyleBackColor = true;
            this.ok_btn.Click += new System.EventHandler(this.ok_btn_Click);
            // 
            // Cancel_btn
            // 
            this.Cancel_btn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Cancel_btn.Location = new System.Drawing.Point(231, 117);
            this.Cancel_btn.Name = "Cancel_btn";
            this.Cancel_btn.Size = new System.Drawing.Size(75, 23);
            this.Cancel_btn.TabIndex = 2;
            this.Cancel_btn.Text = "Cancel";
            this.Cancel_btn.UseVisualStyleBackColor = true;
            this.Cancel_btn.Click += new System.EventHandler(this.Cancel_btn_Click);
            // 
            // setup_connect
            // 
            this.AcceptButton = this.ok_btn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.Cancel_btn;
            this.ClientSize = new System.Drawing.Size(318, 152);
            this.Controls.Add(this.Cancel_btn);
            this.Controls.Add(this.ok_btn);
            this.Controls.Add(this.port_IP_tx);
            this.Controls.Add(this.address_IP_tx);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "setup_connect";
            this.Text = "Setup Connection";
            this.Activated += new System.EventHandler(this.setup_connect_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox address_IP_tx;
        private System.Windows.Forms.TextBox port_IP_tx;
        private System.Windows.Forms.Button ok_btn;
        private System.Windows.Forms.Button Cancel_btn;
    }
}