/*************************************************************************************
 Created Date : 2020.10.23
      Creator : 
   Decription : 공 Tray 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.23  DEVELOPER : Initial Created.
  2021.04.21 KDH : 위치정보 그룹화 대응
  2022.05.25 이정미 : EQP_LOC_GRP_CD 동별 공통코드로 변경
  2022.07-07 조영대 : 공 Tray 수동배출 팝업 신규 추가
  2022.07.21 최도훈 : 공 Tray 수동배출 팝업 ESWA RTD용으로 신규 추가

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_026 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sOnOff;
        private string _sTrayType;
        private string _sLocGrp;
        private string _sBldgCd;
        private string _sActYN = "N";

        private readonly Util _util = new Util();

        public string ON_OFF
        {
            set { this._sOnOff = value; }
        }

        public string TRAY_TYPE
        {
            set { this._sTrayType = value; }
        }

        public string LOC_GRP
        {
            set { this._sLocGrp = value; }
        }

        public string BLDG_CD
        {
            set { this._sBldgCd = value; }
        }

        public string ACTYN
        {
            set { this._sActYN = value; }
            get { return this._sActYN; }
        }

        public FCS002_026()
        {
            InitializeComponent();
            InitCombo();
            initDefault();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                _sOnOff = Util.NVC(parameters[0]);
                _sTrayType = Util.NVC(parameters[1]);
                _sLocGrp = Util.NVC(parameters[2]);
                _sBldgCd = Util.NVC(parameters[3]);
                _sActYN = Util.NVC(parameters[4]);
                InitCombo();
                initDefault();
                GetList();
            }
            //  SetEvent();
        }

        private void initDefault()
        {
            rdoOn.Checked -= rdoOn_Checked;
            rdoOff.Checked -= rdoOff_Checked;
            if (!string.IsNullOrEmpty(_sOnOff))
            {
                if (_sOnOff.Equals("ON"))
                    rdoOn.IsChecked = true;
                else if (_sOnOff.Equals("OFF"))
                    rdoOff.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(_sTrayType))
                cboTrayType.SelectedValue = _sTrayType;
            if (!string.IsNullOrEmpty(_sLocGrp))
                cboLoc.SelectedValue = _sLocGrp;

            rdoOn.Checked += rdoOn_Checked;
            rdoOff.Checked += rdoOff_Checked;
            rdoOn_Checked(null, null);
            rdoOnScrap_Checked(null, null);
        }

        private void InitCombo()
        {
            //조회 Tab
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            _combo.SetCombo(cboTrayType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "TRAYTYPE");
            string[] sFilter1 = { "COMBO_TRAY_STATUS", "1", null, null, null, null };
            _combo.SetCombo(cboState, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN_WITH_OPTION", sFilter: sFilter1);
            //2021.04.21 위치정보 그룹화 대응 START
            //_combo.SetCombo(cboLoc, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ETRAY_LOC");
            string[] sFilter = { "EQP_LOC_GRP_CD" };
            //combo.SetCombo(cboLoc, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilter);
            //2021.04.21 위치정보 그룹화 대응 END
            _combo.SetCombo(cboLoc, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter);

            //폐기 Tray 조회 Tab
            _combo.SetCombo(cboTrayTypeScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "TRAYTYPE");

            //2021.04.21 위치정보 그룹화 대응 START
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ETRAY_LOC");
            string[] sFilterSrp = { "EQP_LOC_GRP_CD" };
            //_combo.SetCombo(cboLocScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilterSrp);
            //2021.04.21 위치정보 그룹화 대응 END
            _combo.SetCombo(cboLocScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilterSrp);
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                string sBiz = "";
                if ((bool)rdoOn.IsChecked)
                {
                    sBiz = "DA_SEL_ONLINE_EMPTY_TRAY_MB";
                }
                else
                {
                    sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY_MB";
                }
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType);

                if (!string.IsNullOrEmpty(Util.GetCondition(cboState, bAllNull: true)))
                {
                    if (Util.GetCondition(cboState).Equals("ONCLEAN"))
                    {
                        dr["CST_CLEAN_FLAG"] = "Y";
                    }
                    else if (Util.GetCondition(cboState).Equals("NORMAL"))
                    {
                        dr["CST_CLEAN_FLAG"] = "N";
                    }
                }

                if (!string.IsNullOrEmpty(txtTrayID.Text))
                {
                    dr["CSTID"] = txtTrayID.Text;
                }

                if (Util.GetCondition(cboState).Equals("ONCLEAN"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "I";
                }
                else if (Util.GetCondition(cboState).Equals("NORMAL"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "I";
                }
                else if (Util.GetCondition(cboState).Equals("DISUSE"))
                {
                    dr["CST_MNGT_STAT_CODE"] = "S";
                }

                string s = Util.GetCondition(cboLoc, bAllNull: true);
                if (string.IsNullOrEmpty(s)) { }

                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                }
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["CHK"] = false;
                }
                Util.GridSetData(dgEmptyTray, dtRslt, this.FrameOperation,true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListScrap()
        {
            string sBiz = "";
            if ((bool)rdoOnScrap.IsChecked)
            {
                sBiz = "DA_SEL_ONLINE_EMPTY_TRAY_MB";
            }
            else
            {
                sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY_MB";
            }
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("TRAY_TYPE_CD", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("BLDG_CD", typeof(string));
            inDataTable.Columns.Add("CST_MNGT_STAT_CODE", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["TRAY_TYPE_CD"] = Util.GetCondition(cboTrayTypeScrap, bAllNull: true);
            string s = Util.GetCondition(cboLocScrap, bAllNull: true);
            if (string.IsNullOrEmpty(s)) { }
            if (!string.IsNullOrEmpty(txtTrayIDScrap.Text)) dr["CSTID"] = txtTrayIDScrap.Text;
            dr["CST_MNGT_STAT_CODE"] = "S"; //폐기
            inDataTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
            if (dtRslt.Rows.Count > 0)
            {
                dtRslt.Columns.Add("CHK", typeof(bool));
            }
            //dgScrapTray.ItemsSource = DataTableConverter.Convert(dtRslt);
            Util.GridSetData(dgScrapTray, dtRslt, this.FrameOperation);
        }

        private void GetList(string sTrayList)
        {
            try
            {
                string sBiz = "";
                if ((bool)rdoOn.IsChecked)
                {
                    sBiz = "DA_SEL_ONLINE_EMPTY_TRAY_MB";
                }
                else
                {
                    sBiz = "DA_SEL_OFFLINE_EMPTY_TRAY_MB";
                }
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["CSTID"] = sTrayList;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                }
                dgEmptyTray.ItemsSource = DataTableConverter.Convert(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrayEnd(string sTray, int iRow)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("TRAY_ID", typeof(string));
                inDataTable.Columns.Add("MDF_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["TRAY_ID"] = sTray;
                dr["MDF_ID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_TRAY_END", "INDATA", "OUTDATA", inDataTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]

        #region [조회 Tab]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void rdoOn_Checked(object sender, RoutedEventArgs e)
        {
            //btnManualOut.IsEnabled = true;
            /*   if (cboLoc != null)
               {
                   CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
                   _combo.SetCombo(cboLoc, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CONV_MAIN");
               }*/
        }

        private void rdoOff_Checked(object sender, RoutedEventArgs e)
        {
            //btnManualOut.IsEnabled = false;
            /*if (sender != null)
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
                _combo.SetCombo(cboLoc, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "PORT_MAIN");
            }*/
        }

        private void btnStatusChange_Click(object sender, RoutedEventArgs e)
        {

            FCS002_026_STATUS_CHANGE wndPopup = new FCS002_026_STATUS_CHANGE();
            wndPopup.FrameOperation = FrameOperation;

            string sTrayList = string.Empty;

            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS002_026_STATUS_CHANGE FCS002_026_status_change = new FCS002_026_STATUS_CHANGE();
            FCS002_026_status_change.FrameOperation = FrameOperation;
            if (FCS002_026_status_change != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(FCS002_026_status_change, Parameters);

                FCS002_026_status_change.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => FCS002_026_status_change.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_026_STATUS_CHANGE window = sender as FCS002_026_STATUS_CHANGE;
            GetList();
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOut_Click(object sender, RoutedEventArgs e)
        {
            if (IsCnvrLocationOutLocUse())
            {
                FCS002_026_MANUAL_OUT_LOC manualOutLoc = new FCS002_026_MANUAL_OUT_LOC();
                manualOutLoc.FrameOperation = FrameOperation;

                if (manualOutLoc != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = sTrayList;
                    Parameters[1] = string.Empty;

                    C1WindowExtension.SetParameters(manualOutLoc, Parameters);

                    manualOutLoc.Closed += new EventHandler(ManualOutLoc_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOutLoc.ShowModal()));
                }
            }
            else if (IsBothPortIdUse())
            {
                FCS002_026_MANUAL_OUT_PORT manualOutPort = new FCS002_026_MANUAL_OUT_PORT();
                manualOutPort.FrameOperation = FrameOperation;

                if (manualOutPort != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = sTrayList;
                    Parameters[1] = string.Empty;

                    C1WindowExtension.SetParameters(manualOutPort, Parameters);

                    manualOutPort.Closed += new EventHandler(ManualOutPort_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOutPort.ShowModal()));
                }
            }
            else
            {
                FCS002_026_MANUAL_OUT manualOut = new FCS002_026_MANUAL_OUT();
                manualOut.FrameOperation = FrameOperation;

                if (manualOut != null)
                {
                    string sTrayList = string.Empty;
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                        {
                            sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                        }
                    }

                    if (string.IsNullOrEmpty(sTrayList))
                    {
                        Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                        return;
                    }

                    object[] Parameters = new object[2];
                    Parameters[0] = sTrayList;
                    Parameters[1] = string.Empty;

                    C1WindowExtension.SetParameters(manualOut, Parameters);

                    manualOut.Closed += new EventHandler(ManualOut_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => manualOut.ShowModal()));
                }
            }            
        }

        private void ManualOut_Closed(object sender, EventArgs e)
        {
            FCS002_026_MANUAL_OUT window = sender as FCS002_026_MANUAL_OUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutLoc_Closed(object sender, EventArgs e)
        {
            FCS002_026_MANUAL_OUT_LOC window = sender as FCS002_026_MANUAL_OUT_LOC;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void ManualOutPort_Closed(object sender, EventArgs e)
        {
            FCS002_026_MANUAL_OUT_PORT window = sender as FCS002_026_MANUAL_OUT_PORT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnManualOff_Click(object sender, RoutedEventArgs e)
        {
            string sTrayList = string.Empty;
            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList += Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")) + ",";
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            FCS002_026_MANUAL_OFF FCS002_026_manual_off = new FCS002_026_MANUAL_OFF();
            FCS002_026_manual_off.FrameOperation = FrameOperation;
            if (FCS002_026_manual_off != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;
                C1WindowExtension.SetParameters(FCS002_026_manual_off, Parameters);

                FCS002_026_manual_off.Closed += new EventHandler(FCS002_026_manual_off_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => FCS002_026_manual_off.ShowModal()));
            }
        }

        private void FCS002_026_manual_off_Closed(object sender, EventArgs e)
        {
            FCS002_026_MANUAL_OFF window = sender as FCS002_026_MANUAL_OFF;
            GetList();
            this.grdMain.Children.Remove(window);
        }

        private void btnWorkEnd_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0236", (result) => //진행중인 공정을 종료하시겠습니까?
            {
                if (result != MessageBoxResult.OK)
                {
                    for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                    {
                        if (_util.GetDataGridCheckValue(dgEmptyTray, "CHK", i))
                        {
                            SetTrayEnd(Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "TRAY_ID")), i);
                        }
                    }
                    Util.MessageInfo("FM_ME_0111");  //공정종료를 완료하였습니다.
                    GetList();
                }
            });
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < numCnt.Value && i < dgEmptyTray.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", true);
            }
        }

        private void dgEmptyTray_ButtonClicked(object sender, RoutedEventArgs e)
        {
            //   if(dgEmptyTray.GetValue(e.Rows.DateItem,"RACK_YN"))
        }

        /* private void dgEmptyTray_BeganEdit(object sender, DataGridBeganEditEventArgs e)
         {
             try
             {
                 if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "LOAD_REP_CSTID"))))
                 {
                     //동일한 설비 체크하기
                     bool bCheck = (bool)DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "CHK");
                     string sEqpID = string.Empty;
                     sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[e.Row.Index].DataItem, "LOAD_REP_CSTID"));

                     if (!string.IsNullOrEmpty(sEqpID)) {

                         for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
                         {
                             if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID))
                             {
                                 DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", !bCheck);
                             }
                         }
                     }
                 }
             }
             catch (Exception ex)
             {
                 Util.MessageException(ex);
             }
         }*/
        /// <summary>
        /// Excel 업로드
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            GetList(GetExcelData());

        }

        private string GetExcelData()
        {
            string sColData = string.Empty;

            Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    sColData = LoadExcel(stream, (int)0);
                }
            }
            return sColData;
        }

        private string LoadExcel(Stream excelFileStream, int sheetNo)
        {
            string sColData = string.Empty;
            try
            {
                string sSeparator = "|";
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                }

                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null)
                    {
                        sColData += sheet.GetCell(rowInx, 0).Text + sSeparator;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sColData;
        }
        // 전체 선택, 해제
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgEmptyTray);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgEmptyTray);

        }

        private void dgEmptyTray_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //동일한 설비 체크하기
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEmptyTray.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                bool bCheck = !(bool)DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "CHK");
                string sEqpID = string.Empty;
                sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[cell.Row.Index].DataItem, "LOAD_REP_CSTID"));

                if (string.IsNullOrEmpty(sEqpID)) return;
                for (int i = 0; i < this.dgEmptyTray.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID)
                        && i != cell.Row.Index)
                    {
                        DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", bCheck);
                    }
                }
            }
        }

        private void dgEmptyTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgEmptyTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private bool IsCnvrLocationOutLocUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CNVR_LOCATION_OUT_LOC_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private bool IsBothPortIdUse()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "BOTH_PORT_ID_USE";
                dr["COM_CODE"] = "USE_YN";
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Rows[0]["ATTR1"] != null &&
                    dtResult.Rows[0]["ATTR1"].ToString().Equals("Y"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && txtTrayID.Text.Length == 10)
                {
                    GetList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBCRTray_KeyDown(object sender, KeyEventArgs e)
        {
            string sTrayID = string.Empty;
            string sEqpID = string.Empty;
            sTrayID = Util.NVC(txtBCRTray.Text);

            if (string.IsNullOrEmpty(sTrayID)) return;
            for (int i = 0; i < this.dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID")).Equals(sTrayID))
                {
                    DataTableConverter.SetValue(dgEmptyTray.Rows[i].DataItem, "CHK", true);

                    sEqpID = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "LOAD_REP_CSTID"));




                }
            }

            for (int j = 0; j < this.dgEmptyTray.GetRowCount(); j++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[j].DataItem, "LOAD_REP_CSTID")).Equals(sEqpID)
                    )
                {
                    DataTableConverter.SetValue(dgEmptyTray.Rows[j].DataItem, "CHK", true);
                }
            }

        }

        private void btnCMPOUT_Click(object sender, RoutedEventArgs e)
        {
            string sTrayList = string.Empty;
            int iTrayCnt = 0;
            for (int i = 0; i < dgEmptyTray.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    sTrayList = Util.NVC(DataTableConverter.GetValue(dgEmptyTray.Rows[i].DataItem, "CSTID"));
                    iTrayCnt++;
                }
            }

            if (string.IsNullOrEmpty(sTrayList))
            {
                Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                return;
            }

            else if (iTrayCnt > 1)
            {
                Util.MessageInfo("FM_ME_0526");  //Tray를 하나만 선택해주세요.
                return;
            }

            FCS002_026_CMP_OUT CMPOut = new FCS002_026_CMP_OUT();
            CMPOut.FrameOperation = FrameOperation;

            if (CMPOut != null)
            {


                object[] Parameters = new object[1];
                Parameters[0] = sTrayList;

                C1WindowExtension.SetParameters(CMPOut, Parameters);

                CMPOut.Closed += new EventHandler(FCS002_026_CMPOut_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => CMPOut.ShowModal()));
            }

        }

        private void FCS002_026_CMPOut_Closed(object sender, EventArgs e)
        {
            FCS002_026_CMP_OUT window = sender as FCS002_026_CMP_OUT;
            GetList();
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [폐기 Tray 조회 Tab]
        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            GetListScrap();
        }

        private void rdoOnScrap_Checked(object sender, RoutedEventArgs e)
        {
            /*  if (cboTrayLocScrap == null)
                  return;
              if (sender != null)
              {
                  CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
                  _combo.SetCombo(cboTrayLocScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CONV_MAIN");
              }*/
        }

        private void rdoOffScrap_Checked(object sender, RoutedEventArgs e)
        {
            /*if (cboTrayLocScrap == null)
                return;
            if (sender != null)
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
                _combo.SetCombo(cboTrayLocScrap, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "PORT_MAIN");
            }*/
        }

        //전체선택 , 전체선택 해제
        private void chkHeaderAllScrap_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgScrapTray);
        }

        private void chkHeaderAllScrap_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgScrapTray);
        }

        private void dgScrapTray_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgScrapTray.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #endregion
    }
}


