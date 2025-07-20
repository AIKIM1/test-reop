namespace LGC.GMES.MES.VIS001
{
    partial class dlgLog
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
            this.rtxtLog = new System.Windows.Forms.RichTextBox();
            this.btnDataLogView = new System.Windows.Forms.Button();
            this.btnSystemLogView = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtxtLog
            // 
            this.rtxtLog.BackColor = System.Drawing.SystemColors.HighlightText;
            this.rtxtLog.Location = new System.Drawing.Point(12, 12);
            this.rtxtLog.Name = "rtxtLog";
            this.rtxtLog.ReadOnly = true;
            this.rtxtLog.Size = new System.Drawing.Size(979, 483);
            this.rtxtLog.TabIndex = 0;
            this.rtxtLog.Text = "";
            // 
            // btnDataLogView
            // 
            this.btnDataLogView.Location = new System.Drawing.Point(12, 501);
            this.btnDataLogView.Name = "btnDataLogView";
            this.btnDataLogView.Size = new System.Drawing.Size(127, 27);
            this.btnDataLogView.TabIndex = 1;
            this.btnDataLogView.Text = "Data Log View";
            this.btnDataLogView.UseVisualStyleBackColor = true;
            this.btnDataLogView.Click += new System.EventHandler(this.btnDataLogView_Click);
            // 
            // btnSystemLogView
            // 
            this.btnSystemLogView.Location = new System.Drawing.Point(145, 501);
            this.btnSystemLogView.Name = "btnSystemLogView";
            this.btnSystemLogView.Size = new System.Drawing.Size(127, 27);
            this.btnSystemLogView.TabIndex = 1;
            this.btnSystemLogView.Text = "System Log View";
            this.btnSystemLogView.UseVisualStyleBackColor = true;
            this.btnSystemLogView.Click += new System.EventHandler(this.btnSystemLogView_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(923, 501);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(68, 27);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // dlgLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1003, 533);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSystemLogView);
            this.Controls.Add(this.btnDataLogView);
            this.Controls.Add(this.rtxtLog);
            this.Name = "dlgLog";
            this.Text = "dlgLoc";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnDataLogView;
        private System.Windows.Forms.Button btnSystemLogView;
        private System.Windows.Forms.Button btnClose;
        public System.Windows.Forms.RichTextBox rtxtLog;
    }
}