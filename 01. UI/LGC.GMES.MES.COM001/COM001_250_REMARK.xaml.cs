/*************************************************************************************
 Created Date : 2018.08.24
      Creator : 
   Decription : 출하계획 비고입력
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.24  DEVELOPER : Initial Created.


**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_250_REMARK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sPRODID = string.Empty;
        private string sSHIPTOCODE = string.Empty;
        private string sSHIPTONAME = string.Empty;
        private string sTRANSPORT = string.Empty;
        private string sTRANSPORTNAME = string.Empty;
        private string sREMARK = string.Empty;
        private string sSHIPDATE = string.Empty;

        public COM001_250_REMARK()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            sPRODID = Util.NVC(tmps[0]);
            sSHIPTOCODE = Util.NVC(tmps[1]);
            sSHIPTONAME = Util.NVC(tmps[2]);
            sTRANSPORT = Util.NVC(tmps[3]);
            sTRANSPORTNAME = Util.NVC(tmps[4]);
            sSHIPDATE = Util.NVC(tmps[5]);

            txtProdID.Text = sPRODID;
            txtShipToName.Text = sSHIPTONAME;
            txtTransport.Text = sTRANSPORTNAME;

            txtRemark.Text = getNote();
            txtDate.Text = sSHIPDATE.Substring(0, 4) + "-" + sSHIPDATE.Substring(4, 2) + "-" + sSHIPDATE.Substring(6, 2);
        }
        #endregion

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private string getNote()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("SHIPTO_CODE", typeof(string));
                IndataTable.Columns.Add("SHIP_DATE", typeof(string));
                IndataTable.Columns.Add("TRANSP_MODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = sPRODID;
                Indata["SHIPTO_CODE"] = sSHIPTOCODE;
                Indata["SHIP_DATE"] = sSHIPDATE;
                Indata["TRANSP_MODE"] = sTRANSPORT;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_SHIP_PLAN_NOTE", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return Util.NVC(dtMain.Rows[0][0]);

            }
            catch (Exception ex) { }

            return "";
        }

        private void SaveData()
        {
            try
            {
                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("PRODID", typeof(string));
                        IndataTable.Columns.Add("SHIPTO_CODE", typeof(string));
                        IndataTable.Columns.Add("TRANSP_MODE", typeof(string));
                        IndataTable.Columns.Add("NOTE", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("SHIP_DATE", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["PRODID"] = sPRODID;
                        Indata["SHIPTO_CODE"] = sSHIPTOCODE;
                        Indata["TRANSP_MODE"] = sTRANSPORT;
                        Indata["NOTE"] = Util.NVC(txtRemark.Text);
                        Indata["USERID"] = LoginInfo.USERID;
                        Indata["SHIP_DATE"] = sSHIPDATE;
                        IndataTable.Rows.Add(Indata);

                        if (IndataTable.Rows.Count !=0 )
                        {
                            new ClientProxy().ExecuteService("DA_PRD_UPD_TB_SFC_FP_SHIP_PLAN", "INDATA", null, IndataTable, (result, ex) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                    return;
                                }

                                Util.AlertInfo("SFU1275");  //정상처리되었습니다.
                            });
                        }
                        else
                        {
                            Util.Alert("SFU1278");  //처리 할 항목이 없습니다.
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
            
        }
        #endregion
    }
}
