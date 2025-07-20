/*************************************************************************************
 Created Date : 2017.06.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - Cell ID 관리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.15  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007_CELL_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_CELL_LIST : C1Window, IWorkArea
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
        private Brush[,] originColorMap = null;


        private bool _ShowSlotNo = false;
        private bool _OnlyView = false;

        BizDataSet _Biz = new BizDataSet();
        //PKG_TRAY_25 winTray25 = null;
        UserControl winTray = null;

        // 주액 USL, LSL 기준정보
        private string _EL_WEIGHT_LSL = string.Empty;
        private string _EL_WEIGHT_USL = string.Empty;
        private string _EL_AFTER_WEIGHT_LSL = string.Empty;
        private string _EL_AFTER_WEIGHT_USL = string.Empty;
        private string _EL_BEFORE_WEIGHT_LSL = string.Empty;
        private string _EL_BEFORE_WEIGHT_USL = string.Empty;


        ASSY003_007_CELLID_RULE wndCellIDRule = null;

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

        public ASSY003_007_CELL_LIST()
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

            //rdoAuto.IsChecked = true;
            cboTrayLocation.IsEnabled = false;

            // 주액 관려 여부에 따른 컨트롤 Hidden 처리
            if (_ELUseYN)
            {
                //rdoManual.IsEnabled = true;
                tblEl.Visibility = Visibility.Visible;
                txtEl.Visibility = Visibility.Visible;
                //tblElMinMax.Visibility = Visibility.Visible;
                tblBeforeWeight.Visibility = Visibility.Visible;
                txtBeforeWeight.Visibility = Visibility.Visible;
                //tblBeforeWeightMinMax.Visibility = Visibility.Visible;
                tblAfterWeight.Visibility = Visibility.Visible;
                txtAfterWeight.Visibility = Visibility.Visible;
                //tblAfterWeightMinMax.Visibility = Visibility.Visible;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Visible;
                txtPosition.Visibility = Visibility.Visible;
                tblJudge.Visibility = Visibility.Visible;
                txtJudge.Visibility = Visibility.Visible;

                btnCheckElJudge.Visibility = Visibility.Visible;
            }
            else
            {
                //rdoManual.IsEnabled = false;
                tblEl.Visibility = Visibility.Collapsed;
                txtEl.Visibility = Visibility.Collapsed;
                tblElMinMax.Visibility = Visibility.Collapsed;
                tblBeforeWeight.Visibility = Visibility.Collapsed;
                txtBeforeWeight.Visibility = Visibility.Collapsed;
                tblBeforeWeightMinMax.Visibility = Visibility.Collapsed;
                tblAfterWeight.Visibility = Visibility.Collapsed;
                txtAfterWeight.Visibility = Visibility.Collapsed;
                tblAfterWeightMinMax.Visibility = Visibility.Collapsed;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Collapsed;
                txtPosition.Visibility = Visibility.Collapsed;
                tblJudge.Visibility = Visibility.Collapsed;
                txtJudge.Visibility = Visibility.Collapsed;

                btnCheckElJudge.Visibility = Visibility.Collapsed;
            }


            // View Mode 인 경우 모두 Disable 처리.
            if (_OnlyView)
            {
                txtCellId.IsReadOnly = true;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                dgDupList.IsReadOnly = true;

                //rdoManual.IsEnabled = true;

                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;

                btnCheckElJudge.Visibility = Visibility.Collapsed;
                btnOutRangeDelAll.Visibility = Visibility.Collapsed;

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
            grdOutRangeList.Visibility = Visibility.Collapsed;

            // Slot No. 표시 처리
            //_ShowSlotNo = true;
            //if (chkViewSlotNo != null && chkViewSlotNo.IsChecked.HasValue)
            //    chkViewSlotNo.IsChecked = true; 

            chkViewSlotNo.Visibility = Visibility.Visible;


            ApplyPermissions();

            // 기본 Display 설정            
            if (rdoCellID != null && rdoCellID.IsChecked != null)
            {
                rdoCellID.Checked -= rdoCellID_Checked;
                rdoCellID.IsChecked = true;
                rdoCellID.Checked += rdoCellID_Checked;
            }

            // 주액 DATA 관리 여부
            GetElDataMngtFlag();

            InitializeControls();

            SetTrayWindow();
            //setTryLoction();
            SetBasicInfo();

            ChangeMode("ALL");  // 컨트롤 모두 View 처리...
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayWindow(bChgNexRow: false);

            GetTrayInfo();

            GetOutRangeCellList();
        }

        private void rdoAuto_Checked(object sender, RoutedEventArgs e)
        {
            txtCellId.IsReadOnly = false;
            //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
            txtEl.IsReadOnly = true;
            txtBeforeWeight.IsReadOnly = true;
            txtAfterWeight.IsReadOnly = true;
            txtHeader.IsReadOnly = true;
            txtPosition.IsReadOnly = true;
            txtJudge.IsReadOnly = true;
            cboTrayLocation.IsEnabled = true;
        }

        private void rdoManual_Checked(object sender, RoutedEventArgs e)
        {
            txtCellId.IsReadOnly = true;
            txtEl.IsReadOnly = false;
            txtBeforeWeight.IsReadOnly = true;
            txtAfterWeight.IsReadOnly = false;
            txtHeader.IsReadOnly = false;
            txtPosition.IsReadOnly = false;
            txtJudge.IsReadOnly = true;
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

            if (e.Key == Key.Enter) // && rdoAuto.IsChecked.HasValue && (bool)rdoAuto.IsChecked)
            {
                if (!CanCellScan())
                    return;


                if (SaveCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
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

                        //if (winTray25 == null)
                        //    return;

                        //C1DataGrid dgTray = winTray25.GetTrayGrdInfo();

                        if (winTray == null)
                            return;

                        C1DataGrid dgTray = null;

                        if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                            dgTray = (winTray as PKG_TRAY_MOBILE).GetTrayGrdInfo();

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
                                                    newRow["SUBLOTID"] = sTmpCellID + iRow.ToString("0000");
                                                    newRow["CSTSLOT"] = iLocation.ToString();

                                                    inSublotTable.Rows.Add(newRow);

                                                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S", "IN_EQP,IN_CST,IN_EL", null, indataSet);

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
                        if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                        {
                            bool bShowSlotNo = chkViewSlotNo.IsChecked.HasValue && (bool)chkViewSlotNo.IsChecked ? true : false;
                            (winTray as PKG_TRAY_MOBILE).ShowHideAllColumns(bShowSlotNo);
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
                txtPosition.Text = "";
                txtJudge.Text = "";

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
                    txtPosition.Text = dtResult.Columns.Contains("EL_PSTN") ? Util.NVC(dtResult.Rows[0]["EL_PSTN"]) : "";
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
                    if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    {
                        int iLoc = 0;

                        int.TryParse(sLoc, out iLoc);

                        if (cboTrayLocation.Items.Count >= iLoc)
                        {
                            if (iLoc == 0)
                                cboTrayLocation.SelectedValue = 1;
                            else
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

        //private bool SaveCell()
        //{
        //    try
        //    {
        //        ShowLoadingIndicator();

        //        DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

        //        DataTable inTable = indataSet.Tables["IN_EQP"];

        //        DataRow newRow = inTable.NewRow();
        //        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        //        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
        //        newRow["EQPTID"] = _EqptID;
        //        newRow["PROD_LOTID"] = _LotID;
        //        newRow["OUT_LOTID"] = _OutLotID;
        //        newRow["CSTID"] = _TrayID;
        //        newRow["USERID"] = LoginInfo.USERID;

        //        inTable.Rows.Add(newRow);
        //        newRow = null;

        //        DataTable inSublotTable = indataSet.Tables["IN_CST"];
        //        newRow = inSublotTable.NewRow();
        //        newRow["SUBLOTID"] = txtCellId.Text.Trim();
        //        newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

        //        inSublotTable.Rows.Add(newRow);

        //        if (_ELUseYN)
        //        {
        //            DataTable inElTable = indataSet.Tables["IN_EL"];
        //            newRow = inElTable.NewRow();
        //            newRow["SUBLOTID"] = txtCellId.Text.Trim();
        //            newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
        //            newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
        //            newRow["EL_WEIGHT"] = txtEl.Text.Trim();
        //            newRow["EL_PSTN"] = txtPosition.Text.Trim();
        //            newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();

        //            inElTable.Rows.Add(newRow);
        //        }

        //        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S", "IN_EQP,IN_CST,IN_EL", null, indataSet);

        //        HiddenLoadingIndicator();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //        return false;
        //    }
        //}

        private bool SaveCell()
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

                if (_ELUseYN)
                {
                    DataTable inElTable = indataSet.Tables["IN_EL"];
                    newRow = inElTable.NewRow();
                    newRow["SUBLOTID"] = txtCellId.Text.Trim();
                    newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                    newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                    newRow["EL_WEIGHT"] = txtEl.Text.Trim();
                    newRow["EL_PSTN"] = txtPosition.Text.Trim();
                    newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();

                    inElTable.Rows.Add(newRow);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S_UI", "IN_EQP,IN_CST,IN_EL", "OUTDATA", indataSet);

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

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_VALID_SUBLOTID_S", "INDATA,IN_SUBLOT", "OUTDATA", indataSet);

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

        private void GetElDataMngtFlag()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EL_DATA_MNGT_FLAG_CL", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("EL_DATA_USE_YN") && Util.NVC(dtRslt.Rows[0]["EL_DATA_USE_YN"]).Equals("Y"))
                    {
                        _ELUseYN = true;
                    }
                    else
                    {
                        _ELUseYN = false;
                    }
                }
                else
                {
                    _ELUseYN = false;
                }
            }
            catch (Exception ex)
            {
                _ELUseYN = false;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetOutRangeCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TrayID.Substring(0, 4);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_OF_RANGE_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutRangeList.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Collapsed;
                        }

                        SetOriginColorMap();
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

        private void GetElJudgeInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["PROCID"] = Process.PACKAGING;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EL_JUDGE_WEIGHT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("CLCTITEM") && searchResult.Columns.Contains("MIN_MAX_VALUE"))
                        {
                            tblElMinMax.Visibility = Visibility.Visible;
                            tblBeforeWeightMinMax.Visibility = Visibility.Visible;
                            tblAfterWeightMinMax.Visibility = Visibility.Visible;

                            foreach (DataRow dtRow in searchResult.Rows)
                            {
                                if (dtRow != null)
                                {
                                    if (Util.NVC(dtRow["CLCTITEM"]).Equals("FCLCT7723")) // 주액량
                                    {
                                        tblElMinMax.Text = "Min~Max (" + Util.NVC(dtRow["MIN_MAX_VALUE"]) + ")";

                                        if (searchResult.Columns.Contains("CLCTITEM_LSL_VALUE"))
                                            _EL_WEIGHT_LSL = Util.NVC(dtRow["CLCTITEM_LSL_VALUE"]);

                                        if (searchResult.Columns.Contains("CLCTITEM_USL_VALUE"))
                                            _EL_WEIGHT_USL = Util.NVC(dtRow["CLCTITEM_USL_VALUE"]);
                                    }
                                    else if (Util.NVC(dtRow["CLCTITEM"]).Equals("FCLCT7722")) // 주액후
                                    {
                                        tblAfterWeightMinMax.Text = "Min~Max (" + Util.NVC(dtRow["MIN_MAX_VALUE"]) + ")";

                                        if (searchResult.Columns.Contains("CLCTITEM_LSL_VALUE"))
                                            _EL_AFTER_WEIGHT_LSL = Util.NVC(dtRow["CLCTITEM_LSL_VALUE"]);

                                        if (searchResult.Columns.Contains("CLCTITEM_USL_VALUE"))
                                            _EL_AFTER_WEIGHT_USL = Util.NVC(dtRow["CLCTITEM_USL_VALUE"]);
                                    }
                                    else if (Util.NVC(dtRow["CLCTITEM"]).Equals("FCLCT7721")) // 주액전
                                    {
                                        tblBeforeWeightMinMax.Text = "Min~Max (" + Util.NVC(dtRow["MIN_MAX_VALUE"]) + ")";

                                        if (searchResult.Columns.Contains("CLCTITEM_LSL_VALUE"))
                                            _EL_BEFORE_WEIGHT_LSL = Util.NVC(dtRow["CLCTITEM_LSL_VALUE"]);

                                        if (searchResult.Columns.Contains("CLCTITEM_USL_VALUE"))
                                            _EL_BEFORE_WEIGHT_USL = Util.NVC(dtRow["CLCTITEM_USL_VALUE"]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            tblElMinMax.Visibility = Visibility.Collapsed;
                            tblBeforeWeightMinMax.Visibility = Visibility.Collapsed;
                            tblAfterWeightMinMax.Visibility = Visibility.Collapsed;

                            tblElMinMax.Text = "Min~Max ()";
                            tblBeforeWeightMinMax.Text = "Min~Max ()";
                            tblAfterWeightMinMax.Text = "Min~Max ()";

                            Util.MessageValidation("SFU4285"); // 해당 제품의 주액 Spec 기준정보가 등록되지 않았습니다.
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
            if (_ELUseYN)
            {
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
                if (txtPosition.Text.Trim().Equals("") || txtPosition.Text.Trim().Length > 1)
                {
                    Util.MessageValidation("SFU4450"); // 해더 정보는 1자리로 입력하세요.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanCellScan()
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

        private bool CanDeleteOutRange()
        {
            bool bRet = false;

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
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
            listAuth.Add(btnCheckElJudge);
            listAuth.Add(btnOutRangeDelAll);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetBasicInfo()
        {
            txtLotId.Text = _LotID;
            txtTrayId.Text = _TrayID;

            GetTrayInfo();

            GetOutRangeCellList();

            // 주액 MIN MAX 관리 안하기로 하여 주석..
            //GetElJudgeInfo();
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
                //    // 패키지 Tray View 화면 1개로 구현하여 아래 주석 후 공통 사용 처리
                //    //winTray = new PKG_TRAY_25(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                //    ////IWorkArea winWorkOrder = obj as IWorkArea;
                //    //(winTray as PKG_TRAY_25).FrameOperation = FrameOperation;

                //    //(winTray as PKG_TRAY_25)._Parent = this;
                //    //gTrayLayout.Children.Add(winTray);

                //    ////winTray25 = new PKG_TRAY_25(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                //    //////IWorkArea winWorkOrder = obj as IWorkArea;
                //    ////winTray25.FrameOperation = FrameOperation;

                //    ////winTray25._Parent = this;
                //    ////gTrayLayout.Children.Add(winTray25);
                //    //break;
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
                winTray = new PKG_TRAY_MOBILE(_LotID, _WipSeq, _TrayID, _TrayQty, _OutLotID, _EqptID);
                (winTray as PKG_TRAY_MOBILE).FrameOperation = FrameOperation;
                (winTray as PKG_TRAY_MOBILE)._Parent = this;
                gTrayLayout.Children.Add(winTray);



                //        break;
                //    default:
                //        break;
                //}
            }
        }

        private void SetOriginColorMap()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (winTray == null) return;

                C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                originColorMap = new Brush[dgdCell.Rows.Count, dgdCell.Columns.Count];

                for (int i = 0; i < dgdCell.Rows.Count; i++)
                    for (int j = 0; j < dgdCell.Columns.Count; j++)
                        if (dgdCell[i, j].Presenter != null)
                            originColorMap[i, j] = dgdCell[i, j].Presenter.Background;
            })
            );
        }

        private void SearchTrayWindow(bool bLoad = false, bool bSameLoc = false, bool bChgNexRow = true)
        {
            if (winTray == null)
                return;

            if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                (winTray as PKG_TRAY_MOBILE).SetCellInfo(bLoad, bSameLoc, bChgNexRow);

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
                    grdOutRangeList.Visibility = Visibility.Collapsed;

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
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;

                btnSave.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnOutRangeDelAll.IsEnabled = false;

                btnCheckElJudge.IsEnabled = false;
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
                txtPosition.Text = "";
                txtJudge.Text = "";
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

                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    {
                        (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
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

                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    {
                        (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoCellID_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    {
                        // Display Set.
                        (winTray as PKG_TRAY_MOBILE).SetTrayDisplayList(new string[] { "CELLID" });

                        // 조회
                        (winTray as PKG_TRAY_MOBILE).SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                        GetTrayInfo();


                        //rdoManual.IsEnabled = false;
                        //tblEl.Visibility = Visibility.Collapsed;
                        //txtEl.Visibility = Visibility.Collapsed;
                        //tblBeforeWeight.Visibility = Visibility.Collapsed;
                        //txtBeforeWeight.Visibility = Visibility.Collapsed;
                        //tblAfterWeight.Visibility = Visibility.Collapsed;
                        //txtAfterWeight.Visibility = Visibility.Collapsed;
                        //tblHeader.Visibility = Visibility.Collapsed;
                        //txtHeader.Visibility = Visibility.Collapsed;                        
                        //tblPosition.Visibility = Visibility.Collapsed;
                        //txtPosition.Visibility = Visibility.Collapsed;
                        //tblJudge.Visibility = Visibility.Collapsed;
                        //txtJudge.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoELWeight_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    {
                        // Display Set.
                        (winTray as PKG_TRAY_MOBILE).SetTrayDisplayList(new string[] { "EL_WEIGHT" });

                        // 조회
                        (winTray as PKG_TRAY_MOBILE).SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                        GetTrayInfo();


                        //rdoManual.IsEnabled = true;
                        //tblEl.Visibility = Visibility.Visible;
                        //txtEl.Visibility = Visibility.Visible;
                        //tblBeforeWeight.Visibility = Visibility.Visible;
                        //txtBeforeWeight.Visibility = Visibility.Visible;
                        //tblAfterWeight.Visibility = Visibility.Visible;
                        //txtAfterWeight.Visibility = Visibility.Visible;
                        //tblHeader.Visibility = Visibility.Visible;
                        //txtHeader.Visibility = Visibility.Visible;
                        //tblPosition.Visibility = Visibility.Visible;
                        //txtPosition.Visibility = Visibility.Visible;
                        //tblJudge.Visibility = Visibility.Visible;
                        //txtJudge.Visibility = Visibility.Visible;
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
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
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
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = true;
            }
            else
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;
            }


            if (!(winTray as PKG_TRAY_MOBILE).bExistLayOutInfo)
            {
                SetLayoutNone();
            }
        }

        private void tbCellID_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    txtCellId.IsReadOnly = false;
                    txtEl.IsReadOnly = false;
                    txtBeforeWeight.IsReadOnly = false;
                    txtAfterWeight.IsReadOnly = false;
                    txtHeader.IsReadOnly = false;
                    txtPosition.IsReadOnly = false;
                    txtJudge.IsReadOnly = true;
                    cboTrayLocation.IsEnabled = true;

                    btnSave.IsEnabled = true;
                    btnDelete.IsEnabled = true;

                    btnCheckElJudge.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtEl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtEl.Text, false, -1))
                {
                    txtEl.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBeforeWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtBeforeWeight.Text, false, -1))
                {
                    txtBeforeWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtAfterWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtAfterWeight.Text, false, -1))
                {
                    txtAfterWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHeader_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtHeader.Text, false, -1))
                {
                    txtHeader.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkDouble(string str, bool bUseMin, double dMinValue)
        {
            try
            {
                bool bRet = false;

                if (str.Trim().Equals(""))
                    return bRet;

                if (str.Trim().Equals("-"))
                    return true;

                double value;
                if (!double.TryParse(str, out value))
                {
                    //숫자필드에 부적절한 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2914");
                    return bRet;
                }
                if (bUseMin && value < dMinValue)
                {
                    //숫자필드에 허용되지 않는 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2915");
                    return bRet;
                }

                bRet = true;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void txtPosition_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtJudge_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void btnOutRangeDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                GetTrayInfo();
                                GetOutRangeCellList();
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

        private void btnOutRangeDelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        for (int i = 0; i < dgOutRangeList.Rows.Count; i++)
                        {
                            DeleteCell(Util.NVC(DataTableConverter.GetValue(dgOutRangeList.Rows[i].DataItem, "SUBLOTID")));
                        }

                        GetTrayInfo();
                        GetOutRangeCellList();

                        Util.MessageInfo("SFU1273");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCheckElJudge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EL_SPEC_CHK_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.PACKAGING;

                //if (Util.NVC(_EL_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_USL).Length > 0 )
                //    newRow["EL_SPEC_CHK_FLAG"] = "Y";
                //else
                newRow["EL_SPEC_CHK_FLAG"] = "N";

                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PKG_CELL_CALC_EL_SPEC", "IN_EQP", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void txtCellId_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCellId == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCellId, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbCellIDRuleTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    wndCellIDRule = new ASSY003_007_CELLID_RULE();

                    wndCellIDRule.FrameOperation = FrameOperation;

                    if (wndCellIDRule != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = _LotID;

                        C1WindowExtension.SetParameters(wndCellIDRule, Parameters);

                        wndCellIDRule.Closed += new EventHandler(wndCellIDRule_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndCellIDRule.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndCellIDRule_Closed(object sender, EventArgs e)
        {
            ASSY003_007_CELLID_RULE window = sender as ASSY003_007_CELLID_RULE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            wndCellIDRule = null;
        }

        private void C1Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Loaded -= C1Window_Loaded;
                this.Initialized -= C1Window_Initialized;
                this.Unloaded -= C1Window_Unloaded;
                chkViewSlotNo.Checked -= chkViewSlotNo_Checked;
                chkViewSlotNo.Unchecked -= chkViewSlotNo_Unchecked;
                btnCheckElJudge.Click -= btnCheckElJudge_Click;
                btnSearch.Click -= btnSearch_Click;
                btnClose.Click -= btnClose_Click;
                rdoCellID.Checked -= rdoCellID_Checked;
                rdoELWeight.Checked -= rdoELWeight_Checked;
                txtCellId.KeyUp -= txtCellId_KeyUp;
                txtCellId.GotFocus -= txtCellId_GotFocus;
                txtEl.KeyUp -= txtEl_KeyUp;
                txtBeforeWeight.KeyUp -= txtBeforeWeight_KeyUp;
                txtAfterWeight.KeyUp -= txtAfterWeight_KeyUp;
                txtHeader.KeyUp -= txtHeader_KeyUp;
                txtPosition.KeyUp -= txtPosition_KeyUp;
                txtJudge.KeyUp -= txtJudge_KeyUp;
                btnSave.Click -= btnSave_Click;
                btnDelete.Click -= btnDelete_Click;
                btnOutRangeDelAll.Click -= btnOutRangeDelAll_Click;

                GC.Collect();
                //GC.WaitForPendingFinalizers();

                //System.Diagnostics.Debug.WriteLine("After Cell List Unloaded : {0} Bytes.", GC.GetTotalMemory(false));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbxCellId_TextChanged(object sender, TextChangedEventArgs e)
        {
            //    if (winTray == null) return;

            //    string inputText = (sender as TextBox).Text;

            //    C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");

            //    string[] colNameArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            //    for (int i = 0; i < dgdCell.Rows.Count; i++)
            //    {
            //        for (int j = 2; j < dgdCell.Columns.Count; j++)
            //        {
            //            for (int k = 0; k < colNameArr.Length; k++)
            //            {
            //                if (dgdCell.Columns[j].Name == colNameArr[k] && dgdCell[i, j].Text == dgdCell[i, j + 3].Text)
            //                {
            //                    //if (dgdCell[i, j].Presenter != null)
            //                    if (inputText.Trim().Length > 0 && dgdCell[i, j].Text.Contains(inputText))
            //                        dgdCell[i, j].Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#77009DFF"));
            //                    else
            //                        dgdCell[i, j].Presenter.Background = originColorMap[i, j];
            //                    break;
            //                }
            //            }
            //        }
            //    }
        }

        private void tbxCellId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                string[] colNameArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                ClearControl();

                for (int i = 0; i < dgdCell.Rows.Count; i++)
                {
                    for (int j = 2; j < dgdCell.Columns.Count; j++)
                    {
                        if (dgdCell[i, j].Presenter != null)
                        {
                            dgdCell[i, j].Presenter.Background = originColorMap[i, j];
                            dgdCell[i, j].Presenter.IsSelected = false;
                            for (int k = 0; k < colNameArr.Length; k++)
                            {
                                if (tbxCellId.Text == dgdCell[i, j].Text &&
                                    dgdCell.Columns[j].Name == colNameArr[k] && dgdCell[i, j].Text == dgdCell[i, j + 3].Text)
                                {
                                    dgdCell.CurrentCell = dgdCell[i, j];
                                    dgdCell[i, j].Presenter.IsSelected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ClearControl()
        {
            txtDefaultWeight.Text = "";
            txtCellId.Text = "";
            txtEl.Text = "";
            txtBeforeWeight.Text = "";
            txtAfterWeight.Text = "";
        }
    }
}