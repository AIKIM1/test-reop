namespace LGC.GMES.MES.VIS001
{
    partial class MainWindows
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindows));
            this.nifIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.MnuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuStart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRun = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSepa01 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuLog = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSepa02 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEnd = new System.Windows.Forms.ToolStripMenuItem();
            this.lstImg = new System.Windows.Forms.ImageList(this.components);
            this.MnuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // nifIcon
            // 
            this.nifIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.nifIcon.BalloonTipTitle = "Vision Slitter Checkup";
            this.nifIcon.ContextMenuStrip = this.MnuStrip;
            this.nifIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("nifIcon.Icon")));
            this.nifIcon.Text = "Vision Slitter Checkup";
            this.nifIcon.Visible = true;
            // 
            // MnuStrip
            // 
            this.MnuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStart,
            this.mnuStop,
            this.mnuRun,
            this.mnuSepa01,
            this.mnuLog,
            this.mnuConfig,
            this.mnuSepa02,
            this.mnuEnd});
            this.MnuStrip.Name = "MnuStrip";
            this.MnuStrip.Size = new System.Drawing.Size(111, 148);
            // 
            // mnuStart
            // 
            this.mnuStart.Name = "mnuStart";
            this.mnuStart.Size = new System.Drawing.Size(110, 22);
            this.mnuStart.Text = "Start";
            this.mnuStart.Click += new System.EventHandler(this.mnuStart_Click);
            // 
            // mnuStop
            // 
            this.mnuStop.Name = "mnuStop";
            this.mnuStop.Size = new System.Drawing.Size(110, 22);
            this.mnuStop.Text = "Stop";
            this.mnuStop.Click += new System.EventHandler(this.mnuStop_Click);
            // 
            // mnuRun
            // 
            this.mnuRun.Name = "mnuRun";
            this.mnuRun.Size = new System.Drawing.Size(110, 22);
            this.mnuRun.Text = "Run";
            this.mnuRun.Click += new System.EventHandler(this.mnuRun_Click);
            // 
            // mnuSepa01
            // 
            this.mnuSepa01.Name = "mnuSepa01";
            this.mnuSepa01.Size = new System.Drawing.Size(107, 6);
            // 
            // mnuLog
            // 
            this.mnuLog.Name = "mnuLog";
            this.mnuLog.Size = new System.Drawing.Size(110, 22);
            this.mnuLog.Text = "Log";
            this.mnuLog.Click += new System.EventHandler(this.mnuLog_Click);
            // 
            // mnuConfig
            // 
            this.mnuConfig.Name = "mnuConfig";
            this.mnuConfig.Size = new System.Drawing.Size(110, 22);
            this.mnuConfig.Text = "Config";
            this.mnuConfig.Click += new System.EventHandler(this.mnuConfig_Click);
            // 
            // mnuSepa02
            // 
            this.mnuSepa02.Name = "mnuSepa02";
            this.mnuSepa02.Size = new System.Drawing.Size(107, 6);
            // 
            // mnuEnd
            // 
            this.mnuEnd.Name = "mnuEnd";
            this.mnuEnd.Size = new System.Drawing.Size(110, 22);
            this.mnuEnd.Text = "End";
            this.mnuEnd.Click += new System.EventHandler(this.mnuEnd_Click);
            // 
            // lstImg
            // 
            this.lstImg.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstImg.ImageStream")));
            this.lstImg.TransparentColor = System.Drawing.Color.Transparent;
            this.lstImg.Images.SetKeyName(0, "icoVision_Start.ico");
            this.lstImg.Images.SetKeyName(1, "icoVision_Stop.ico");
            this.lstImg.Images.SetKeyName(2, "icoVision_Run.ico");
            // 
            // MainWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindows";
            this.Text = "Form1";
            this.MnuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon nifIcon;
        private System.Windows.Forms.ContextMenuStrip MnuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuStart;
        private System.Windows.Forms.ToolStripMenuItem mnuStop;
        private System.Windows.Forms.ToolStripMenuItem mnuRun;
        private System.Windows.Forms.ToolStripSeparator mnuSepa01;
        private System.Windows.Forms.ToolStripMenuItem mnuLog;
        private System.Windows.Forms.ToolStripMenuItem mnuConfig;
        private System.Windows.Forms.ToolStripSeparator mnuSepa02;
        private System.Windows.Forms.ToolStripMenuItem mnuEnd;
        private System.Windows.Forms.ImageList lstImg;
    }
}

