namespace GatewayForm
{
    partial class SearchForm_Data
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
            this.btn_searchTag = new System.Windows.Forms.Button();
            this.btn_searchdate = new System.Windows.Forms.Button();
            this.txt_search_tag = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_search_date = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // btn_searchTag
            // 
            this.btn_searchTag.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_searchTag.Location = new System.Drawing.Point(270, 18);
            this.btn_searchTag.Name = "btn_searchTag";
            this.btn_searchTag.Size = new System.Drawing.Size(75, 23);
            this.btn_searchTag.TabIndex = 0;
            this.btn_searchTag.Text = "Search";
            this.btn_searchTag.UseVisualStyleBackColor = true;
            this.btn_searchTag.Click += new System.EventHandler(this.btn_searchTag_Click);
            // 
            // btn_searchdate
            // 
            this.btn_searchdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_searchdate.Location = new System.Drawing.Point(270, 50);
            this.btn_searchdate.Name = "btn_searchdate";
            this.btn_searchdate.Size = new System.Drawing.Size(75, 23);
            this.btn_searchdate.TabIndex = 1;
            this.btn_searchdate.Text = "Search";
            this.btn_searchdate.UseVisualStyleBackColor = true;
            this.btn_searchdate.Click += new System.EventHandler(this.btn_searchdate_Click);
            // 
            // txt_search_tag
            // 
            this.txt_search_tag.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_search_tag.Location = new System.Drawing.Point(64, 19);
            this.txt_search_tag.Name = "txt_search_tag";
            this.txt_search_tag.Size = new System.Drawing.Size(200, 22);
            this.txt_search_tag.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Tag";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(18, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Date";
            // 
            // txt_search_date
            // 
            this.txt_search_date.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_search_date.Location = new System.Drawing.Point(64, 50);
            this.txt_search_date.Name = "txt_search_date";
            this.txt_search_date.Size = new System.Drawing.Size(200, 21);
            this.txt_search_date.TabIndex = 6;
            this.txt_search_date.ValueChanged += new System.EventHandler(this.txt_search_date_ValueChanged);
            // 
            // SearchForm_Data
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 89);
            this.Controls.Add(this.txt_search_date);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_search_tag);
            this.Controls.Add(this.btn_searchdate);
            this.Controls.Add(this.btn_searchTag);
            this.Name = "SearchForm_Data";
            this.Text = "Search";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_searchTag;
        private System.Windows.Forms.Button btn_searchdate;
        private System.Windows.Forms.TextBox txt_search_tag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker txt_search_date;
    }
}