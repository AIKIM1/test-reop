using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGC.GMES.MES.VIS001
{
    public partial class dlgConfig : Form
    {
        public dlgConfig()
        {
            InitializeComponent();
            this.Load += new System.EventHandler(this.dlgConfig_Load);
        }

        private void dlgConfig_Load(object sender, EventArgs e)
        {
            this.Load -= new System.EventHandler(this.dlgConfig_Load);
            Initialize();
        }

        private void Initialize()
        {
            this.CenterToScreen();

            this.txtShopCode.Text = LGC.GMES.MES.VIS001.Config.ShopCode;
            this.txtAreaCode.Text = LGC.GMES.MES.VIS001.Config.AreaCode;

            this.txtBizActorIP.Text = LGC.GMES.MES.VIS001.Config.BizActorIP;
            this.txtBizActorServiceIndex.Text = LGC.GMES.MES.VIS001.Config.BizActorServiceIndex;
            this.txtBizActorPort.Text = LGC.GMES.MES.VIS001.Config.BizActorPort;

            this.chkFTPUsed.Checked = LGC.GMES.MES.VIS001.Config.FTPUsed;
            this.txtFTP.Text = LGC.GMES.MES.VIS001.Config.FTP;
            this.txtFTPID.Text = LGC.GMES.MES.VIS001.Config.FTPID;
            this.txtFTPPassword.Text = LGC.GMES.MES.VIS001.Config.FTPPassword;

            this.txtExtension.Text = LGC.GMES.MES.VIS001.Config.Extension;
            this.txtDataFileFolder.Text = LGC.GMES.MES.VIS001.Config.DataFileFolder;

            this.cboSeparationType.Items.Add("Comma");
            this.cboSeparationType.Items.Add("Cemicolon");
            this.cboSeparationType.Items.Add("Blank");
            this.cboSeparationType.Items.Add("Tab");

            this.cboSeparationType.SelectedIndex =  LGC.GMES.MES.VIS001.Config.SeparationType;

            this.chkDataBackupFolder.Checked = LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed;
            this.txtDataBackupFolder.Text = LGC.GMES.MES.VIS001.Config.DataBackupFolder;

            this.chkImageBackupFolder.Checked = LGC.GMES.MES.VIS001.Config.ImageBackupFolderUsed;
            this.txtImageBackupFolder.Text = LGC.GMES.MES.VIS001.Config.ImageBackupFolder;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            LGC.GMES.MES.VIS001.Config.ShopCode = this.txtShopCode.Text;
            LGC.GMES.MES.VIS001.Config.AreaCode = this.txtAreaCode.Text;

            LGC.GMES.MES.VIS001.Config.BizActorIP = this.txtBizActorIP.Text;
            LGC.GMES.MES.VIS001.Config.BizActorServiceIndex = this.txtBizActorServiceIndex.Text;
            LGC.GMES.MES.VIS001.Config.BizActorPort = this.txtBizActorPort.Text;

            LGC.GMES.MES.VIS001.Config.FTPUsed = this.chkFTPUsed.Checked;
            LGC.GMES.MES.VIS001.Config.FTP = this.txtFTP.Text;
            LGC.GMES.MES.VIS001.Config.FTPID = this.txtFTPID.Text;
            LGC.GMES.MES.VIS001.Config.FTPPassword = this.txtFTPPassword.Text;

            LGC.GMES.MES.VIS001.Config.Extension = this.txtExtension.Text;
            LGC.GMES.MES.VIS001.Config.DataFileFolder = this.txtDataFileFolder.Text;

            LGC.GMES.MES.VIS001.Config.SeparationType = this.cboSeparationType.SelectedIndex;

            LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed = this.chkDataBackupFolder.Checked;
            LGC.GMES.MES.VIS001.Config.DataBackupFolder = this.txtDataBackupFolder.Text;

            LGC.GMES.MES.VIS001.Config.ImageBackupFolderUsed = this.chkImageBackupFolder.Checked;
            LGC.GMES.MES.VIS001.Config.ImageBackupFolder = this.txtImageBackupFolder.Text;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
