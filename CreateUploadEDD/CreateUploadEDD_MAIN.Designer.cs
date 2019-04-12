namespace CreateUploadEDD
{
    partial class CreateUploadEDD_MAIN
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
            this.cbTables = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbEnDARCIMP = new System.Windows.Forms.RadioButton();
            this.rbEnDAR = new System.Windows.Forms.RadioButton();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbTables
            // 
            this.cbTables.FormattingEnabled = true;
            this.cbTables.Location = new System.Drawing.Point(227, 59);
            this.cbTables.Name = "cbTables";
            this.cbTables.Size = new System.Drawing.Size(226, 24);
            this.cbTables.TabIndex = 0;
            this.cbTables.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(227, 127);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(233, 45);
            this.button1.TabIndex = 1;
            this.button1.Text = "Create Scripts";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.button2.Location = new System.Drawing.Point(30, 215);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(169, 58);
            this.button2.TabIndex = 2;
            this.button2.Text = "ReCreateDropDownList";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbEnDARCIMP);
            this.groupBox1.Controls.Add(this.rbEnDAR);
            this.groupBox1.Location = new System.Drawing.Point(13, 59);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(150, 93);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // rbEnDARCIMP
            // 
            this.rbEnDARCIMP.AutoSize = true;
            this.rbEnDARCIMP.Location = new System.Drawing.Point(17, 66);
            this.rbEnDARCIMP.Name = "rbEnDARCIMP";
            this.rbEnDARCIMP.Size = new System.Drawing.Size(115, 21);
            this.rbEnDARCIMP.TabIndex = 1;
            this.rbEnDARCIMP.TabStop = true;
            this.rbEnDARCIMP.Text = "EnDAR_CIMP";
            this.rbEnDARCIMP.UseVisualStyleBackColor = true;
            // 
            // rbEnDAR
            // 
            this.rbEnDAR.AutoSize = true;
            this.rbEnDAR.Location = new System.Drawing.Point(17, 22);
            this.rbEnDAR.Name = "rbEnDAR";
            this.rbEnDAR.Size = new System.Drawing.Size(75, 21);
            this.rbEnDAR.TabIndex = 0;
            this.rbEnDAR.TabStop = true;
            this.rbEnDAR.Text = "EnDAR";
            this.rbEnDAR.UseVisualStyleBackColor = true;
            this.rbEnDAR.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.Color.SkyBlue;
            this.textBox1.Location = new System.Drawing.Point(365, 215);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(369, 61);
            this.textBox1.TabIndex = 4;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.DoubleClick += new System.EventHandler(this.textBox1_DoubleClick);
            this.textBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseDoubleClick);
            // 
            // CreateUploadEDD_MAIN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 323);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cbTables);
            this.Name = "CreateUploadEDD_MAIN";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.CreateUploadEDD_MAIN_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbTables;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbEnDARCIMP;
        private System.Windows.Forms.RadioButton rbEnDAR;
        private System.Windows.Forms.TextBox textBox1;
    }
}

