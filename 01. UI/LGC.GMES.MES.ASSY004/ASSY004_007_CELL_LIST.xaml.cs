/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Packaging 공정진척 화면 - Cell ID 관리 팝업 (ASSY0001.ASSY001_007_CELL_LIST 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.
  2019.09.02  INS 김동일K : 공정진척(조립) > 패키지 생산반제품에 외관검사 판정 정보 추가 및 Cell 관리 팝업 외관검사 목록 추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_007_CELL_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_007_CELL_LIST : C1Window, IWorkArea
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

        BizDataSet _Biz = new BizDataSet();
        //PKG_TRAY_25 winTray25 = null;
        UserControl winTray = null;

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

        public ASSY004_007_CELL_LIST()
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

                this.Header = ObjectDic.Instance.GetObjectName("TRAY별CELLID관리") + " (Read Only)";
            }
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 8)
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
            }

            grdDupList.Visibility = Visibility.Collapsed;
            grdVslJudgList.Visibility = Visibility.Collapsed;

            // Slot No. 표시 처리
            _ShowSlotNo = true;
            if (chkViewSlotNo != null && chkViewSlotNo.IsChecked.HasValue)
                chkViewSlotNo.IsChecked = true;


            ApplyPermissions();
            InitializeControls();

            SetTrayWindow();
            SetBasicInfo();

            ChangeMode("ALL");  // 컨트롤 모두 View 처리...

            GetVslChkCellInfo();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayWindow(bChgNexRow: false);

            GetTrayInfo();

            GetVslChkCellInfo();
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
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            if (!CanCellModify())
                return;

            if (SaveCell())
            {
                SearchTrayWindow();

                GetTrayInfo();

                GetVslChkCellInfo();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            if (!CanDelete())
                return;

            if (DeleteCell())
            {
                SearchTrayWindow();

                GetTrayInfo();

                GetVslChkCellInfo();
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
                if (!CanCellModify())
                    return;


                if (SaveCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();

                    GetVslChkCellInfo();
                }
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
                        
                        if (winTray == null)
                            return;

                        C1DataGrid dgTray = null;

                        if (winTray.GetType() == typeof(PKG_TRAY))
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
                                    if (!Util.NVC(dgTray.Columns[i].Name).Equals("NO") &&
                                        !Util.NVC(dgTray.Columns[i].Name).EndsWith("_JUDGE") &&
                                        !Util.NVC(dgTray.Columns[i].Name).EndsWith("_LOC"))
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

                                                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);

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
                    HideLoadingIndicator();
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
                    HideLoadingIndicator();
                }
            }));
        }

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
            catch (Exception ex)
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
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
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

                        if (!CanCellModify())
                        {
                            return;
                        }

                        if (SaveCell())
                        {
                            SearchTrayWindow();
                        }

                        txtCellId.Text = string.Empty;
                    }

                    GetVslChkCellInfo();
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

                DataTable inTable = _Biz.GetDA_PRD_SEL_CELL_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;
                newRow["CELLID"] = sCellID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_INFO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtEl.Text = dtResult.Columns.Contains("EL_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_WEIGHT"]) : "";
                    txtBeforeWeight.Text = dtResult.Columns.Contains("EL_PRE_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_PRE_WEIGHT"]) : "";
                    txtAfterWeight.Text = dtResult.Columns.Contains("EL_AFTER_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_AFTER_WEIGHT"]) : "";
                    txtHeader.Text = dtResult.Columns.Contains("HEADER") ? Util.NVC(dtResult.Rows[0]["HEADER"]) : "";

                    int iLoc = 0;
                    int.TryParse(Util.NVC(dtResult.Rows[0]["LOCATION"]), out iLoc);
                    
                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        cboTrayLocation.SelectedIndex = iLoc - 1;                        
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
                        }
                    }

                    grdHiddenInfo.RowDefinitions[1].Height = new GridLength(0);

                    grdDupList.Visibility = Visibility.Collapsed;                    
                }

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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

                HideLoadingIndicator();

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
                HideLoadingIndicator();
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

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);

                HideLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);

                HideLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ChkSubLotValid()
        {
            try
            {
                bool bRet = false;

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
                        bRet = true;
                    }
                    else
                    {
                        bRet = false;

                        if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSGID"]).Trim().Equals(""))
                        {
                            if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("SC"))
                            {
                                // 특수문자가 포함되어 있습니다.
                                Util.MessageValidation("SFU3049");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NR"))
                            {
                                // 읽을 수 없습니다
                                Util.MessageValidation("SFU3050");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("DL"))
                            {
                                // CELL ID 자리수가 잘못 되었습니다
                                Util.MessageValidation("SFU3051");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("ID"))
                            {
                                // CELL ID 가 이미 존재 합니다.
                                Util.MessageValidation("SFU3052");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("PD"))
                            {
                                // 동일한 위치에 이미 CELL ID가 존재 합니다.
                                Util.MessageValidation("SFU3053");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NI"))
                            {
                                // 주액 정보가 없습니다.
                                Util.MessageValidation("SFU3054");
                            }
                            else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("CE"))
                            {
                                // 셀ID조합 구성이 올바르지 않습니다.
                                Util.MessageValidation("SFU3749");
                            }
                            else
                            {
                                Util.MessageValidation("");
                            }
                        }
                        else
                        {
                            string[] parms = (new string[2] { Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG_PARA1"]), Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG_PARA2"]) }).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                            Util.MessageValidation(Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSGID"]), parms);
                        }
                    }
                }
                else
                {
                    bRet = false;

                    // "존재하지 않습니다.";
                    Util.MessageValidation("SFU2881");
                }

                HideLoadingIndicator();

                return bRet;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
                return false;
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

                HideLoadingIndicator();

                return dtRslt;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetVslChkCellInfo()
        {
            try
            {
                Util.gridClear(dgVslJudgList);

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("TRAYID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_VISUAL_JUDG_LIST", "INDATA", "OUTDATA", inTable, (dtRslt, bzExcption) =>
                {
                    try
                    {
                        if (bzExcption != null)
                        {
                            Util.MessageException(bzExcption);
                            return;
                        }

                        if (dtRslt?.Rows?.Count > 0)
                        {
                            grdHiddenInfo.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

                            grdVslJudgList.Visibility = Visibility.Visible;
                            Util.GridSetData(dgVslJudgList, dtRslt, FrameOperation, true);
                        }
                        else
                        {
                            grdHiddenInfo.RowDefinitions[0].Height = new GridLength(0);

                            grdVslJudgList.Visibility = Visibility.Collapsed;                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
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

            // Cell ID Validation            
            if (!ChkSubLotValid())
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

        public void HideLoadingIndicator()
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

            if (winTray.GetType() == typeof(PKG_TRAY))
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
                    grdHiddenInfo.RowDefinitions[1].Height = new GridLength(0);

                    grdDupList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdHiddenInfo.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

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
        }
        #endregion

        #endregion
        
    }
}
