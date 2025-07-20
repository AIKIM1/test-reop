/*************************************************************************************
 Created Date : 2016.09.21
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - Cell ID 관리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.21  INS 김동일K : Initial Created.
  2004.05.31  이병윤        E20240528-000578 NG Cell 추가, 삭제시 Caution 팝업 호출
  2024.07.12  김동일        E20240514-000618 - ESNA 법인 주액 Interlock 로직관련 Cell 별 주액 데이터 조회 및 저장 기능 추가

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007_CELL_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _TrayID = string.Empty;
        private string _TrayQty = string.Empty;
        private string _OutLotID = string.Empty;
        private bool _ELUseYN = false;

        private bool _ShowSlotNo = false;
        private bool _OnlyView = false;

        private bool _ELUseLineYN = false;

        // 주액 USL, LSL 기준정보
        private string _EL_WEIGHT_LSL = string.Empty;
        private string _EL_WEIGHT_USL = string.Empty;
        private string _EL_AFTER_WEIGHT_LSL = string.Empty;
        private string _EL_AFTER_WEIGHT_USL = string.Empty;
        private string _EL_BEFORE_WEIGHT_LSL = string.Empty;
        private string _EL_BEFORE_WEIGHT_USL = string.Empty;

        //2019-06-20 최상민 TERM LOT에 CELL 투입 기능 구분
        private string _Term_Cell = string.Empty;

        BizDataSet _Biz = new BizDataSet();
        //PKG_TRAY_25 winTray25 = null;
        UserControl winTray = null;

        // E20240528-000578 NG Cell 추가 Visible 여부
        private bool bCellNg = false;

        public bool IsShowSlotNo
        {
            get { return _ShowSlotNo; }
        }

        public bool IsOnlyViewMode
        {
            get { return _OnlyView; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_007_CELL_LIST()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            int iTrayQty = 0;
            if (!_TrayQty.Equals("") && int.TryParse(_TrayQty, out iTrayQty))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = 0; i < iTrayQty; i++)
                {
                    dt.Rows.Add((i + 1).ToString(), (i + 1).ToString());
                }

                cboTrayLocation.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboTrayLocation.SelectedIndex = 0;
            }

            rdoAuto.IsChecked = true;
            cboTrayLocation.IsEnabled = false;




            // MMD 공통코드에서 주액량 사용 PLANT인지 확인. 2019.12.19
            DataTable inTmpTable = new DataTable();
            inTmpTable.Columns.Add("SHOPID", typeof(string));

            DataRow newTmpRow = inTmpTable.NewRow();
            newTmpRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

            inTmpTable.Rows.Add(newTmpRow);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_EL_FILLING_USE", "INDATA", "OUTDATA", inTmpTable);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                _ELUseYN = true;
            }

            #region [E20240514-000618] - Cell 별 주액 DATA 로직 추가 건
            _ELUseLineYN = false;

            if (string.IsNullOrEmpty(_Term_Cell))
            {
                DataTable inTemp = new DataTable();
                inTemp.Columns.Add("LANGID", typeof(string));
                inTemp.Columns.Add("CMCDTYPE", typeof(string));
                inTemp.Columns.Add("CMCODE", typeof(string));

                DataRow tempRow = inTemp.NewRow();
                tempRow["LANGID"] = LoginInfo.LANGID;
                tempRow["CMCDTYPE"] = "PACKAGE_EL_FILLING_CHECK_LINE";
                tempRow["CMCODE"] = _LineID;

                inTemp.Rows.Add(tempRow);

                DataTable dtTempRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTemp);

                if (dtTempRslt != null && dtTempRslt.Rows.Count > 0)
                {
                    _ELUseYN = true;
                    _ELUseLineYN = true;
                }
            }                
            #endregion

            // 주액 관려 여부에 따른 컨트롤 Hidden 처리
            if (_ELUseYN)
            {
                rdoManual.IsEnabled = true;
                tblEl.Visibility = Visibility.Visible;
                txtEl.Visibility = Visibility.Visible;
                tblBeforeWeight.Visibility = Visibility.Visible;
                txtBeforeWeight.Visibility = Visibility.Visible;
                tblAfterWeight.Visibility = Visibility.Visible;
                txtAfterWeight.Visibility = Visibility.Visible;
                tblHeader.Visibility = Visibility.Visible;
                txtHeader.Visibility = Visibility.Visible;
                tblJudge.Visibility = Visibility.Visible;
                txtJudge.Visibility = Visibility.Visible;

                #region [E20240514-000618] - Cell 별 주액 DATA 로직 추가 건
                if (_ELUseLineYN)
                {
                    tblHeader.Visibility = Visibility.Collapsed;
                    txtHeader.Visibility = Visibility.Collapsed;
                    txtHeader.IsReadOnly = true;
                    tblJudge.Visibility = Visibility.Collapsed;
                    txtJudge.Visibility = Visibility.Collapsed;
                    txtJudge.IsReadOnly = true;
                }
                #endregion
            }
            else
            {
                rdoManual.IsEnabled = false;
                tblEl.Visibility = Visibility.Collapsed;
                txtEl.Visibility = Visibility.Collapsed;
                tblBeforeWeight.Visibility = Visibility.Collapsed;
                txtBeforeWeight.Visibility = Visibility.Collapsed;
                tblAfterWeight.Visibility = Visibility.Collapsed;
                txtAfterWeight.Visibility = Visibility.Collapsed;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblJudge.Visibility = Visibility.Collapsed;
                txtJudge.Visibility = Visibility.Collapsed;
            }


            // View Mode 인 경우 모두 Disable 처리.
            if (_OnlyView)
            {
                txtCellId.IsReadOnly = true;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;                
                dgDupList.IsReadOnly = true;

                rdoManual.IsEnabled = true;

                txtEl.IsReadOnly = true;                
                txtBeforeWeight.IsReadOnly = true;                
                txtAfterWeight.IsReadOnly = true;                
                txtHeader.IsReadOnly = true;
                txtJudge.IsReadOnly = true;

                this.Header = ObjectDic.Instance.GetObjectName("TRAY별CELLID관리") + " (Read Only)";
            }
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length == 8)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TrayID = Util.NVC(tmps[4]);
                _TrayQty = Util.NVC(tmps[5]);
                _OutLotID = Util.NVC(tmps[6]);                

                if (!bool.TryParse(Util.NVC(tmps[7]), out _OnlyView))
                    _OnlyView = false;
            }
            else if (tmps != null && tmps.Length == 9)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _TrayID = Util.NVC(tmps[4]);
                _TrayQty = Util.NVC(tmps[5]);
                _OutLotID = Util.NVC(tmps[6]);

                _Term_Cell = Util.NVC(tmps[8]);

                if (!bool.TryParse(Util.NVC(tmps[7]), out _OnlyView))
                    _OnlyView = false;


            }             
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _TrayID = "";
                _TrayQty = "";
                _OutLotID = "";
                _OnlyView = false;
                _Term_Cell = "";
            }

            grdDupList.Visibility = Visibility.Collapsed;

            // Slot No. 표시 처리
            _ShowSlotNo = true;
            if (chkViewSlotNo != null && chkViewSlotNo.IsChecked.HasValue)
                chkViewSlotNo.IsChecked = true;


            ApplyPermissions();
            InitializeControls();

            SetTrayWindow();
            //setTryLoction();
            SetBasicInfo();

            ChangeMode("ALL");  // 컨트롤 모두 View 처리...

            // E20240528-000578 NG Cell 추가 Visible 여부
            if (LoginInfo.SYSID.Equals("GMES-S-N5") || LoginInfo.SYSID.Equals("GMES-S-N6"))
            {
                bCellNg = true;
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayWindow(bChgNexRow:false);

            GetTrayInfo();
        }

        private void rdoAuto_Checked(object sender, RoutedEventArgs e)
        {
            txtCellId.IsReadOnly = false;
            //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
            txtEl.IsReadOnly = true;
            txtBeforeWeight.IsReadOnly = true;
            txtAfterWeight.IsReadOnly = true;
            txtHeader.IsReadOnly = true;
            cboTrayLocation.IsEnabled = true;
        }

        private void rdoManual_Checked(object sender, RoutedEventArgs e)
        {
            txtCellId.IsReadOnly = true;
            txtEl.IsReadOnly = false;
            txtBeforeWeight.IsReadOnly = true;
            txtAfterWeight.IsReadOnly = false;
            txtHeader.IsReadOnly = false;
            cboTrayLocation.IsEnabled = false;

            #region [E20240514-000618] - Cell 별 주액 DATA 로직 추가 건
            if (_ELUseLineYN)
            {
                txtHeader.IsReadOnly = true;
                //txtJudge.IsReadOnly = true;
            }
            #endregion
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            if (_ELUseYN)
            {
                if (_ELUseLineYN)
                {
                    if (!CanCellModify_EL())
                        return;

                    if (SaveCell_EL())
                    {
                        SearchTrayWindow();

                        GetTrayInfo();
                        return;
                    }
                }
                else
                {
                    if (!CanCellModify_CNJESS())
                        return;

                    if (SaveCell_CNJESS())
                    {
                        SearchTrayWindow();

                        GetTrayInfo();
                        return;
                    }
                }
            }
            else
            {
                if (!CanCellModify())
                    return;

                if (SaveCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                    return;
                }
            }

            //if (!CanCellModify())
            //    return;

            //if (SaveCell())
            //{
            //    SearchTrayWindow();

            //    GetTrayInfo();
            //}


        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            // E20240528-000578 : N5,6동 소형조립인경우 삭제팝업 호출 분기처리
            if (bCellNg == true)
            {
                CheckDeleteCell();
            }
            else
            {
                if (!CanDelete())
                    return;

                if (DeleteCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtCellId_KeyUp(object sender, KeyEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            // 권한 없으면 Skip.
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                return;            

            if (e.Key == Key.Enter && rdoAuto.IsChecked.HasValue && (bool)rdoAuto.IsChecked)
            {
                if (_ELUseYN)
                {
                    var trayLoc = cboTrayLocation.SelectedValue;
                    GetCellInfo(txtCellId.Text, 0, 0);
                    cboTrayLocation.SelectedValue = trayLoc;
                    //if (!CanCellModify_CNJESS())
                    //    return;

                    //if (SaveCell_CNJESS())
                    //{
                    //    SearchTrayWindow();

                    //    GetTrayInfo();
                    //}
                }
                else
                {
                    if (!CanCellModify())
                        return;

                    if (SaveCell())
                    {
                        SearchTrayWindow();

                        GetTrayInfo();
                    }
                }

                //if (!CanCellModify())
                //    return;

                //if (SaveCell())
                //{
                //    SearchTrayWindow();

                //    GetTrayInfo();
                //}
            }
        }

        private void btnDupDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDelete())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgDupList == null || dgDupList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                SearchTrayWindow(false, true);

                                GetTrayInfo();

                                GetDupLocList(Util.NVC(dtRow["CSTSLOT"]));
                            }
                        }
                    }
                });                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        // 자동차동..
                        if (!LoginInfo.CFG_AREA_ID.StartsWith("A"))
                            return;

                        // ReadOnly
                        if (_OnlyView) return;

                        ShowLoadingIndicator();

                        //if (winTray25 == null)
                        //    return;

                        //C1DataGrid dgTray = winTray25.GetTrayGrdInfo();

                        if (winTray == null)
                            return;

                        C1DataGrid dgTray = null;

                        if (winTray.GetType() == typeof(PKG_TRAY_25))
                            dgTray = (winTray as PKG_TRAY_25).GetTrayGrdInfo();
                        else if (winTray.GetType() == typeof(PKG_TRAY))
                            dgTray = (winTray as PKG_TRAY).GetTrayGrdInfo();

                        if (dgTray == null)
                            return;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (txtLotId.Text.Length == 10)
                            {
                                string sTmpCellID = txtLotId.Text.Substring(3, 5) + txtLotId.Text.Substring(9, 1);
                                int iRow = 0;
                                int iLocation = 0;
                                int iColCnt = 0;    // 컬럼 수

                                // 해당 LOT의 MAX SEQ 조회.
                                DataTable inTmpTable = new DataTable();
                                inTmpTable.Columns.Add("LOTID", typeof(string));
                                inTmpTable.Columns.Add("OUT_LOTID", typeof(string));
                                inTmpTable.Columns.Add("TRAYID", typeof(string));
                                inTmpTable.Columns.Add("CELLID", typeof(string));

                                DataRow newTmpRow = inTmpTable.NewRow();
                                newTmpRow["LOTID"] = _LotID;
                                //newTmpRow["OUT_LOTID"] = _OutLotID;
                                //newTmpRow["TRAYID"] = _TrayID;
                                //newTmpRow["CELLID"] = "";

                                inTmpTable.Rows.Add(newTmpRow);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CELL_SEQ_IN_TRAY", "INDATA", "OUTDATA", inTmpTable);

                                if (dtRslt != null && dtRslt.Rows.Count > 0)
                                    iRow = Util.NVC(dtRslt.Rows[0]["MAXSEQ"]).Equals("") ? 0 : Convert.ToInt32(Util.NVC(dtRslt.Rows[0]["MAXSEQ"]));

                                for (int i = 0; i < dgTray.Columns.Count; i++)
                                {
                                    if (!dgTray.Columns[i].Name.Equals("NO") &&
                                        !dgTray.Columns[i].Name.EndsWith("_JUDGE") &&
                                        !dgTray.Columns[i].Name.EndsWith("_LOC"))
                                    {
                                        // 여러 컬럼인 경우 계산.
                                        iLocation = dgTray.Rows.Count * iColCnt;
                                        iColCnt = iColCnt + 1;

                                        for (int j = 0; j < dgTray.Rows.Count; j++)
                                        {
                                            iLocation = iLocation + 1;
                                            iRow = iRow + 1;

                                            string sTmpCell = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name));
                                            string sTmpLoc = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_LOC"));

                                            if ((sTmpCell.Equals("") || sTmpCell.Equals(sTmpLoc)) &&
                                                !Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_JUDGE")).Equals("EMPT_SLOT"))
                                            {
                                                try
                                                {
                                                    DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL();

                                                    DataTable inTable = indataSet.Tables["IN_EQP"];

                                                    DataRow newRow = inTable.NewRow();
                                                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                                    newRow["EQPTID"] = _EqptID;
                                                    newRow["PROD_LOTID"] = _LotID;
                                                    newRow["OUT_LOTID"] = _OutLotID;
                                                    newRow["CSTID"] = _TrayID;
                                                    newRow["USERID"] = LoginInfo.USERID;

                                                    inTable.Rows.Add(newRow);
                                                    newRow = null;

                                                    DataTable inSublotTable = indataSet.Tables["IN_CST"];
                                                    newRow = inSublotTable.NewRow();
                                                    newRow["SUBLOTID"] = sTmpCellID + iRow.ToString("0000");
                                                    newRow["CSTSLOT"] = iLocation.ToString();

                                                    inSublotTable.Rows.Add(newRow);

                                                    if (_Term_Cell.Equals("TERM_CELL"))
                                                    {
                                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_TERM", "IN_EQP,IN_CST", null, indataSet);
                                                    }
                                                    else
                                                    {
                                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);                                                        
                                                    }

                                                    

                                                    System.Threading.Thread.Sleep(300);
                                                }
                                                catch (Exception ex)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }

                                SearchTrayWindow();
                                GetTrayInfo();
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }));
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Released &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        if (winTray != null && winTray.GetType() == typeof(PKG_TRAY))
                        {
                            bool bShowSlotNo = chkViewSlotNo.IsChecked.HasValue && (bool)chkViewSlotNo.IsChecked ? true : false;
                            (winTray as PKG_TRAY).ShowHideAllColumns(bShowSlotNo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }));
        }
        #endregion

        #region Mehod

        #region[BizRule]
        public void GetCellInfo(string sCellID, int iRow, int iCol, string sLoc = "")
        {
            try
            {
                ShowLoadingIndicator();

                txtCellId.Text = sCellID;

                // 주액 정보
                txtEl.Text = "";
                txtBeforeWeight.Text = "";
                txtAfterWeight.Text = "";
                txtHeader.Text = "";

                string biz = _ELUseYN ? "DA_PRD_SEL_CELL_INFO_CNJ" : "DA_PRD_SEL_CELL_INFO";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));
                inTable.Columns.Add("CELLID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;
                newRow["CELLID"] = sCellID;

                if (_ELUseLineYN)
                {
                    biz = "BR_PRD_SEL_CELL_INFO_EL";

                    newRow["EQSGID"] = _LineID;
                    newRow["EQPTID"] = _EqptID;
                }

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(biz, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtEl.Text = dtResult.Columns.Contains("EL_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_WEIGHT"]) : "";
                    txtBeforeWeight.Text = dtResult.Columns.Contains("EL_PRE_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_PRE_WEIGHT"]) : "";
                    txtAfterWeight.Text = dtResult.Columns.Contains("EL_AFTER_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_AFTER_WEIGHT"]) : "";

                    //cboTrayLocation.SelectedValue = dtResult.Columns.Contains("EL_PSTN") ? Util.NVC(dtResult.Rows[0]["EL_PSTN"]) : "";
                    //txtHeader.Text = dtResult.Columns.Contains("HEADER") ? Util.NVC(dtResult.Rows[0]["HEADER"]) : "";
                    //cboTrayLocation.SelectedValue = dtResult.Columns.Contains("LOCATION") ? Util.NVC(dtResult.Rows[0]["LOCATION"]) : "";
                    txtHeader.Text = dtResult.Columns.Contains("EL_PSTN") ? Util.NVC(dtResult.Rows[0]["EL_PSTN"]) : "";

                    txtJudge.Text = dtResult.Columns.Contains("EL_JUDG_VALUE") ? Util.NVC(dtResult.Rows[0]["EL_JUDG_VALUE"]) : "";
                    int iLoc = 0;
                    int.TryParse(Util.NVC(dtResult.Rows[0]["LOCATION"]), out iLoc);

                    //if(cboTrayLocation.Items.Contains(iLoc))
                    //    cboTrayLocation.SelectedValue = iLoc;
                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        cboTrayLocation.SelectedIndex = iLoc - 1;

                        //if (winTray25 == null)
                        //    return;

                        //winTray25.SetNexPos(iLoc - 1);
                    }


                    // 해당 위치에 Cell 정보 조회 후 중복건인경우 중복 List View 처리.
                    CheckLocationDup();
                }
                else
                {
                    if (winTray.GetType() == typeof(PKG_TRAY))
                    {
                        int iLoc = 0;

                        int.TryParse(sLoc, out iLoc);

                        if (cboTrayLocation.Items.Count >= iLoc)
                        {
                            cboTrayLocation.SelectedValue = iLoc;

                            //if (winTray25 == null)
                            //    return;

                            //winTray25.SetNexPos(iLoc);
                        }
                    }
                    else
                    {
                        int iMaxRow = 25;
                                            
                        int iLoc = 0;
                        iLoc = iRow + (iCol > 1 ? iMaxRow : 0);

                        if (cboTrayLocation.Items.Count >= iLoc)
                        {
                            cboTrayLocation.SelectedIndex = iLoc;

                            //if (winTray25 == null)
                            //    return;

                            //winTray25.SetNexPos(iLoc);
                        }
                    }

                    grdDupList.Visibility = Visibility.Collapsed;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private bool ChkTrayStatWait()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SaveCell()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

                inSublotTable.Rows.Add(newRow);

                if (_Term_Cell.Equals("TERM_CELL"))
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_TERM", "IN_EQP,IN_CST", null, indataSet);
                }else
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);
                }
                

                HiddenLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool DeleteCell(string sDelCell = "")
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_SUBLOT_CL();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["CSTID"] = _TrayID;
                newRow["SUBLOTID"] = sDelCell.Equals("") ? txtCellId.Text.Trim() : sDelCell;

                inSublotTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);

                if (_Term_Cell.Equals("TERM_CELL"))
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL_TERM", "INDATA,IN_CST", null, indataSet);
                }
                else
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);
                }

                HiddenLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetSubLotValid(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_CHK_VALID_SUBLOTID();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_SUBLOT"];
                newRow = inSublotTable.NewRow();                
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = Convert.ToInt32(cboTrayLocation.SelectedValue.ToString());

                inSublotTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_VALID_SUBLOTID", "INDATA,IN_SUBLOT", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                    }
                    else
                    {
                        sRet = "NG";

                        if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("SC"))
                        {
                            sMsg = "SFU3049";  // 특수문자가 포함되어 있습니다.
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NR"))
                        {
                            sMsg = "SFU3050";    // 읽을 수 없습니다
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("DL"))
                        {
                            sMsg = "SFU3051";    // CELL ID 자리수가 잘못 되었습니다
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("ID"))
                        {
                            sMsg = "SFU3052";  // CELL ID 가 이미 존재 합니다.
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("PD"))
                        {
                            sMsg = "SFU3053";   // 동일한 위치에 이미 CELL ID가 존재 합니다.
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NI"))
                        {
                            sMsg = "SFU3054";  // 주액 정보가 없습니다.
                        }
                        else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("CE"))
                        {
                            sMsg = "SFU3749";  // 셀ID조합 구성이 올바르지 않습니다.
                        }
                        else
                        {
                            sMsg = "";
                        }

                    }                    
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
        }

        private DataTable GetCellLocCount(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_SEL_TRAY_LOCATION_CNT();
                
                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;
                
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_LOCATION_CNT", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                return dtRslt;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetDupLocList(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_SEL_DUP_TRAY_LOCATION_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DUP_TRAY_LOCATION_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgDupList.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            txtCellCnt.Text = Double.Parse(Util.NVC(searchResult.Rows[0]["CELLQTY"])).ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanCellModify()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            if (txtCellId.Text.Trim().Length != 10)
            {
                //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
                Util.MessageValidation("SFU1318");
                return bRet;
            }

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // Cell ID 특수문자 등 체크.            
            string sRet = string.Empty;
            string sMsg = string.Empty;

            GetSubLotValid(out sRet, out sMsg);

            if (sRet.Equals("NG"))
            {
                //Util.Alert(sMsg);
                Util.MessageValidation(sMsg);
                return bRet;
            }
            else if(sRet.Equals("EXCEPTION"))
                return bRet;

            // 해당 Location 존재 여부 체크.            
            DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            {
                //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
                Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanDelete()
        {
            bool bRet = false;
            
            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // CELL 확정 여부 체크.

            // 해당 Location 존재 여부 체크.            
            DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            {
                //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 없습니다.", cboTrayLocation.SelectedValue.ToString());
                Util.MessageValidation("SFU2031", cboTrayLocation.SelectedValue.ToString());
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region[Func]

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDelete);


            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetBasicInfo()
        {
            txtLotId.Text = _LotID;
            txtTrayId.Text = _TrayID;

            GetTrayInfo();
        }

        private void SetTrayWindow()
        {

            if (gTrayLayout.Children.Count == 0)
            {
                //switch (_TrayQty)
                //{
                //    case "20":
                //    case "25":
                //    case "50":
                //        // 패키지 Tray View 화면 1개로 구현하여 아래 주석 후 공통 사용 처리
                //        //winTray = new PKG_TRAY_25(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                //        ////IWorkArea winWorkOrder = obj as IWorkArea;
                //        //(winTray as PKG_TRAY_25).FrameOperation = FrameOperation;

                //        //(winTray as PKG_TRAY_25)._Parent = this;
                //        //gTrayLayout.Children.Add(winTray);

                //        ////winTray25 = new PKG_TRAY_25(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                //        //////IWorkArea winWorkOrder = obj as IWorkArea;
                //        ////winTray25.FrameOperation = FrameOperation;

                //        ////winTray25._Parent = this;
                //        ////gTrayLayout.Children.Add(winTray25);
                //        //break;
                //    case "43":
                //    case "64":
                //    case "66":
                //    case "81":
                //    case "88":
                //    case "108":
                //    case "110":
                //    case "128":
                //    case "132":
                //    case "151":
                        winTray = new PKG_TRAY(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                        (winTray as PKG_TRAY).FrameOperation = FrameOperation;
                        (winTray as PKG_TRAY)._Parent = this;
                        gTrayLayout.Children.Add(winTray);
                //        break;
                //    default:
                //        break;
                //}
            }
        }

        private void SearchTrayWindow(bool bLoad = false, bool bSameLoc = false, bool bChgNexRow = true)
        {
            if (winTray == null)
                return;

            if (winTray.GetType() == typeof(PKG_TRAY_25))
                (winTray as PKG_TRAY_25).SetCellInfo(bLoad, bSameLoc);
            else if (winTray.GetType() == typeof(PKG_TRAY))
                (winTray as PKG_TRAY).SetCellInfo(bLoad, bSameLoc, bChgNexRow);
            
            //if (winTray25 == null)
            //    return;

            //winTray25.SetCellInfo(bLoad, bSameLoc);
        }

        private void setTryLoction()
        {
            try
            {
                int iRowCnt = 30;
                int iColCnt = 5;

                for (int i = 0; i < iColCnt; i++)
                {
                    ColumnDefinition gridCol1 = new ColumnDefinition();
                    gridCol1.Width = new GridLength(100);

                    gTrayLayout.ColumnDefinitions.Add(gridCol1);

                }

                for (int i = 0; i < iRowCnt + 1; i++)
                {
                    RowDefinition gridRow1 = new RowDefinition();
                    gridRow1.Height = new GridLength(10);
                    gTrayLayout.RowDefinitions.Add(gridRow1);

                }

                for (int iRow = 0; iRow < iRowCnt / 2; iRow++)
                {
                    for (int iCol = 0; iCol < iColCnt; iCol++)
                    {
                        Label _lable = new Label();
                        _lable.Content = iRow.ToString() + iCol.ToString();
                        _lable.FontSize = 10;
                        _lable.HorizontalContentAlignment = HorizontalAlignment.Center;
                        _lable.VerticalContentAlignment = VerticalAlignment.Center;
                        _lable.Margin = new Thickness(0, 0, 0, 0);
                        _lable.Padding = new Thickness(0, 0, 0, 0);
                        _lable.BorderThickness = new Thickness(1, 1, 1, 1);
                        _lable.BorderBrush = new SolidColorBrush(Colors.Gray);
                        _lable.Width = 100;
                        _lable.Height = 20;

                        if ((iCol + iRow) % 3 == 0)
                        {
                            _lable.Background = new SolidColorBrush(Colors.Red);
                        }

                        //_lable.Background = new SolidColorBrush(Colors.Red);
                        Grid.SetColumn(_lable, iCol);

                        if (iCol % 2 == 0)
                        {
                            Grid.SetRow(_lable, iRow * 2);
                        }
                        else
                        {
                            Grid.SetRow(_lable, iRow * 2 + 1);
                        }
                        Grid.SetRowSpan(_lable, 2);

                        gTrayLayout.Children.Add(_lable);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //commMessage.Show(ex.Message);
            }
        }
        
        private void CheckLocationDup()
        {
            try
            {
                if (cboTrayLocation == null || cboTrayLocation.SelectedValue == null)
                    return;

                DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

                if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0") || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("1"))
                {
                    grdDupList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdDupList.Visibility = Visibility.Visible;

                    GetDupLocList(cboTrayLocation.SelectedValue.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        public void DeleteBtnCall()
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                if (!CanDelete())
                    return;

                if (DeleteCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetLayoutNone()
        {
            try
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;

                btnSave.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearInfo()
        {
            try
            {
                txtCellId.Text = "";

                // 주액 정보
                txtEl.Text = "";
                txtBeforeWeight.Text = "";
                txtAfterWeight.Text = "";
                txtHeader.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void chkViewSlotNo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)                    
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;
                    
                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY))
                    {
                        (winTray as PKG_TRAY).ShowSlotNoColumns(bShowSlotNo);
                    }
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkViewSlotNo_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;

                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY))
                    {
                        (winTray as PKG_TRAY).ShowSlotNoColumns(bShowSlotNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeMode(string sMode)
        {
            if (sMode.Equals("ALL"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = false;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                cboTrayLocation.IsEnabled = false;                
            }
            else if (sMode.Equals("AUTO"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                cboTrayLocation.IsEnabled = true;
            }
            else
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                cboTrayLocation.IsEnabled = false;
            }


            if (!(winTray as PKG_TRAY).bExistLayOutInfo)
            {
                SetLayoutNone();
            }

            #region [E20240514-000618] - Cell 별 주액 DATA 로직 추가 건
            if (_ELUseLineYN)
            {
                txtHeader.IsReadOnly = true;
                //txtJudge.IsReadOnly = true;
            }
            #endregion
        }

        private void txtCellId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    // ReadOnly
                    if (_OnlyView)
                    {
                        txtCellId.Text = string.Empty;
                        return;
                    }

                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) {
                        txtCellId.Text = string.Empty;
                        return;
                    }

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 25)
                    {
                        txtCellId.Text = string.Empty;
                        Util.MessageValidation("SFU5015", "25");   //한번에 최대 %1개 까지 처리 가능합니다.
                        return;
                    }

                    for (int inx = 0; inx < sPasteStrings.Length; inx++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[inx]))
                        {
                            break;
                        }

                        txtCellId.Text = sPasteStrings[inx];

                        if (_ELUseYN)
                        {
                            var trayLoc = cboTrayLocation.SelectedValue;
                            GetCellInfo(txtCellId.Text, 0, 0);
                            cboTrayLocation.SelectedValue = trayLoc;
                            //if (!CanCellModify_CNJESS())
                            //    return;

                            //if (SaveCell_CNJESS())
                            //{
                            //    SearchTrayWindow();

                            //    GetTrayInfo();
                            //}
                        }
                        else
                        {
                            if (!CanCellModify())
                                return;

                            if (SaveCell())
                            {
                                SearchTrayWindow();

                                GetTrayInfo();
                            }

                            txtCellId.Text = string.Empty;
                        }

                        //if (!CanCellModify())
                        //    return;

                        //if (SaveCell())
                        //{
                        //    SearchTrayWindow();

                        //    GetTrayInfo();
                        //}

                        //txtCellId.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    GetTrayInfo();
                }

                e.Handled = true;
            }
        }


        // 2019.11.22   C20190813_63226     ASSY003_007.xaml.cs 에서 복사해옴.
        private bool SaveCell_CNJESS()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

                inSublotTable.Rows.Add(newRow);

                DataTable inElTable = indataSet.Tables["IN_EL"];
                newRow = inElTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                newRow["EL_WEIGHT"] = txtEl.Text.Trim();

                int iHeader = 0;
                int.TryParse(Util.NVC(txtHeader.Text.Trim()), out iHeader);

                //2020-07-16 최상민 
                //CNJ ESS의 EL 주액 HEARDER 정보가 설비에서 1~4값으로 올라오며 4보다 큰 값이 올라오면 4로 저장(설비에서 투입이 안된 CELL 등록시 작동됨)
                //CNJ PI 이문철 주관, CNJ PI팀장 협의됨
                if (iHeader > 4)
                {
                    newRow["EL_PSTN"] = "4";
                }
                else
                {
                    newRow["EL_PSTN"] = txtHeader.Text.Trim();
                }
                

                newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();

                inElTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_UI", "IN_EQP,IN_CST,IN_EL", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sRet = string.Empty;
                    string sMsg = string.Empty;

                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                    }
                    else
                    {
                        sRet = "NG";

                        if (dsRslt.Tables["OUTDATA"].Columns.Contains("NG_MSG"))
                            sMsg = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG"]).Trim();
                        else
                            sMsg = "";

                        //if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("SC"))
                        //{
                        //    sMsg = "SFU3049";  // 특수문자가 포함되어 있습니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NR"))
                        //{
                        //    sMsg = "SFU3050";    // 읽을 수 없습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("DL"))
                        //{
                        //    sMsg = "SFU3051";    // CELL ID 자리수가 잘못 되었습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("ID"))
                        //{
                        //    sMsg = "SFU3052";  // CELL ID 가 이미 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("PD"))
                        //{
                        //    sMsg = "SFU3053";   // 동일한 위치에 이미 CELL ID가 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NI"))
                        //{
                        //    sMsg = "SFU3054";  // 주액 정보가 없습니다.
                        //}
                        //else
                        //{
                        //    sMsg = "";
                        //} 

                    }

                    if (sRet.Equals("NG"))
                    {
                        //Util.Alert(sMsg);
                        Util.MessageValidation(sMsg);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CanCellModify_CNJESS()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            //if (txtCellId.Text.Trim().Length != 10)
            //{
            //    //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
            //    Util.MessageValidation("SFU1318");
            //    return bRet;
            //}

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // 주액 저장시 신규 Cell ID 인지 수정 Cell ID 인지 구분 불가로 해당 validation 저장 Biz에서 처리 하도록 변경.
            //// Cell ID 특수문자 등 체크.            
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetSubLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    //Util.Alert(sMsg);
            //    Util.MessageValidation(sMsg);
            //    return bRet;
            //}
            //else if (sRet.Equals("EXCEPTION"))
            //    return bRet;

            //// 해당 Location 존재 여부 체크.            
            //DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            //if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            //{
            //    //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
            //    Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
            //    return bRet;
            //}


            // 주액 정보 Min ~ Max 값 확인
            double dTmpLSL, dTmpUSL, dTmpInput;

            if (double.TryParse(_EL_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_WEIGHT_USL, out dTmpUSL))
            {
                if (double.TryParse(txtEl.Text, out dTmpInput))
                {
                    if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                    {
                        Util.MessageValidation("SFU4280"); // 입력한 주액 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                        return bRet;
                    }
                }
            }

            if (double.TryParse(_EL_BEFORE_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_BEFORE_WEIGHT_USL, out dTmpUSL))
            {
                if (double.TryParse(txtBeforeWeight.Text, out dTmpInput))
                {
                    if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                    {
                        Util.MessageValidation("SFU4281"); // 입력한 주액전 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                        return bRet;
                    }
                }
            }

            if (double.TryParse(_EL_AFTER_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_AFTER_WEIGHT_USL, out dTmpUSL))
            {
                if (double.TryParse(txtAfterWeight.Text, out dTmpInput))
                {
                    if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                    {
                        Util.MessageValidation("SFU4282"); // 입력한 주액후 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                        return bRet;
                    }
                }
            }


            // 주액량 정보입력 여부 체크.
            if (txtEl.Text.Trim().Equals("") || (double.TryParse(txtEl.Text, out dTmpInput) && dTmpInput <= 0))
            {
                Util.MessageValidation("SFU4451"); // 주액량 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (txtBeforeWeight.Text.Trim().Equals("") || (double.TryParse(txtBeforeWeight.Text, out dTmpInput) && dTmpInput <= 0))
            {
                Util.MessageValidation("SFU4452"); // 주액전 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (txtAfterWeight.Text.Trim().Equals("") || (double.TryParse(txtAfterWeight.Text, out dTmpInput) && dTmpInput <= 0))
            {
                Util.MessageValidation("SFU4453"); // 주액후 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }


            //Header 길이 체크.
            if (txtHeader.Text.Trim().Equals("") || txtHeader.Text.Trim().Length > 1)
            {
                Util.MessageValidation("SFU4450"); // 해더 정보는 1자리로 입력하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void CheckDeleteCell()
        {
            try
            {
                string cellId = txtCellId.Text.Trim();
                ASSY001_007_CELL_DEL wndConfirm = new ASSY001_007_CELL_DEL();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {

                    object[] Parameters = new object[1];

                    Parameters[0] = cellId;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Cell_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    //grdSub.Children.Add(wndConfirm);
                    //wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void wndConfirm_Cell_Closed(object sender, EventArgs e)
        {
            ASSY001_007_CELL_DEL window = sender as ASSY001_007_CELL_DEL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!CanDelete())
                    return;

                if (DeleteCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
            grdSub.Children.Remove(window);
        }

        #region [E20240514-000618] - Cell 별 주액 DATA 로직 추가 건
        private bool CanCellModify_EL()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }
            
            double dTmpInput;
            
            // 주액량 정보입력 여부 체크.
            if (!double.TryParse(txtEl.Text, out dTmpInput) || dTmpInput <= 0)
            {
                Util.MessageValidation("SFU4451"); // 주액량 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (!double.TryParse(txtBeforeWeight.Text, out dTmpInput) || dTmpInput <= 0)
            {
                Util.MessageValidation("SFU4452"); // 주액전 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (!double.TryParse(txtAfterWeight.Text, out dTmpInput) || dTmpInput <= 0)
            {
                Util.MessageValidation("SFU4453"); // 주액후 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }
            
            bRet = true;
            return bRet;
        }

        private bool SaveCell_EL()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                #region Create DataSet
                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inSublotTable = indataSet.Tables.Add("IN_CST");
                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("CSTSLOT", typeof(string));

                DataTable inElTable = indataSet.Tables.Add("IN_EL");
                inElTable.Columns.Add("SUBLOTID", typeof(string));
                inElTable.Columns.Add("EL_PRE_WEIGHT", typeof(string));
                inElTable.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
                inElTable.Columns.Add("EL_WEIGHT", typeof(string));
                //inElTable.Columns.Add("EL_PSTN", typeof(string));
                //inElTable.Columns.Add("EL_JUDG_VALUE", typeof(string));
                inElTable.Columns.Add("VALUE001", typeof(string));
                inElTable.Columns.Add("VALUE002", typeof(string));
                inElTable.Columns.Add("VALUE003", typeof(string));
                inElTable.Columns.Add("VALUE004", typeof(string));
                inElTable.Columns.Add("VALUE005", typeof(string));
                inElTable.Columns.Add("VALUE006", typeof(string));
                inElTable.Columns.Add("VALUE007", typeof(string));
                inElTable.Columns.Add("VALUE008", typeof(string));
                inElTable.Columns.Add("VALUE009", typeof(string));
                inElTable.Columns.Add("VALUE010", typeof(string));
                inElTable.Columns.Add("VALUE011", typeof(string));
                inElTable.Columns.Add("VALUE012", typeof(string));
                inElTable.Columns.Add("VALUE013", typeof(string));
                inElTable.Columns.Add("VALUE014", typeof(string));
                inElTable.Columns.Add("VALUE015", typeof(string));
                inElTable.Columns.Add("VALUE016", typeof(string));
                inElTable.Columns.Add("VALUE017", typeof(string));
                inElTable.Columns.Add("VALUE018", typeof(string));
                inElTable.Columns.Add("VALUE019", typeof(string));
                inElTable.Columns.Add("VALUE020", typeof(string));
                #endregion

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

                inSublotTable.Rows.Add(newRow);
                newRow = null;

                newRow = inElTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                newRow["EL_WEIGHT"] = txtEl.Text.Trim();
                
                inElTable.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_UI_EL", "IN_EQP,IN_CST,IN_EL", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sRet = string.Empty;
                    string sMsg = string.Empty;

                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                    }
                    else
                    {
                        sRet = "NG";

                        if (dsRslt.Tables["OUTDATA"].Columns.Contains("NG_MSG"))
                            sMsg = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG"]).Trim();
                        else
                            sMsg = "";
                    }

                    if (sRet.Equals("NG"))
                    {
                        //Util.Alert(sMsg);
                        Util.MessageValidation(sMsg);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion
    }
}
