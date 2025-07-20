using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LGC.GMES.MES.VIS001
{
    public partial class dlgLog : Form
    {
        public dlgLog()
        {
            try
            {
                InitializeComponent();
                this.Load += new System.EventHandler(this.dlgLog_Load);
            }
            catch (Exception ex)
            {
                MainWindows.SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void dlgLog_Load(object sender, EventArgs e)
        {
            this.Load -= new System.EventHandler(this.dlgLog_Load);
            this.CenterToScreen();
            try
            {
                this.Text = "Vision Slitter : " + System.DateTime.Now.ToString("yyyy.MM") + " DATA 로그";
                DataLogview();

            }
            catch (Exception ex)
            {
                SysLogView();
                MainWindows.SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void DataLogview()
        {

            try
            {
                if (Directory.Exists(Common.dataDir) == false)
                {
                    Directory.CreateDirectory(Common.dataDir);
                }

                FileStream fs = new FileStream(Common.dataDir + System.DateTime.Now.ToString("yyyyMM") + "_DATA.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                fs.Close();

                rtxtLog.Clear();
                rtxtLog.LoadFile(Common.dataDir + System.DateTime.Now.ToString("yyyyMM") + "_DATA.log", RichTextBoxStreamType.PlainText);
            }
            catch (Exception ex)
            {
                MainWindows.SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void SysLogView()
        {

            try
            {
                if (Directory.Exists(Common.sysDir) == false)
                {
                    Directory.CreateDirectory(Common.sysDir);
                }

                FileStream fs = new FileStream(Common.sysDir + System.DateTime.Now.ToString("yyyyMM") + "_SYS.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                fs.Close();

                rtxtLog.Clear();
                rtxtLog.LoadFile(Common.sysDir + System.DateTime.Now.ToString("yyyyMM") + "_SYS.log", RichTextBoxStreamType.PlainText);
            }
            catch (Exception ex)
            {
                MainWindows.SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void btnSystemLogView_Click(object sender, EventArgs e)
        {
            SysLogView();
        }

        private void btnDataLogView_Click(object sender, EventArgs e)
        {
            DataLogview();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
