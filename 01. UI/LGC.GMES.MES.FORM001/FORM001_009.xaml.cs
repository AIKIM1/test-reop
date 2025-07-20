/*************************************************************************************
 Created Date : 2017.08.10
      Creator : 
   Decription : 원형 재튜빙
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Globalization;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_009 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UcFormCommand UcFormCommand { get; set; }
        public UcFormSearch UcFormSearch { get; set; }
        public UcFormShift UcFormShift { get; set; }
        public C1ComboBox ComboEquipment { get; set; }

        private string _processCode;
        private string _processName;
        private bool _isLoaded = false;

        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }
        public string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value;
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        public FORM001_009()
        {
            InitializeComponent();
        }

        #region Initialize
        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SetEventInUserControls();

            string equipmentCode = string.Empty;

            if (ComboEquipment?.SelectedValue != null)
            {
                equipmentCode = ComboEquipment.SelectedValue.GetString();
            }

        }

        private void InitializeUserControls()
        {
            UcFormCommand = grdCommand.Children[0] as UcFormCommand;
            UcFormSearch = grdSearch.Children[0] as UcFormSearch;
            UcFormShift = grdShift.Children[0] as UcFormShift;

            if (UcFormCommand != null)
            {
                UcFormCommand.ProcessCode = _processCode;
                UcFormCommand.SetButtonVisibility();
            }

            if (UcFormSearch != null)
            {
                UcFormSearch.ProcessCode = _processCode;
                UcFormSearch.SetCheckBoxVisibility();

                ComboEquipment = UcFormSearch.ComboEquipment;
            }

        }

        private void InitializeUserControlsGrid()
        {
        }

        private void SetEventInUserControls()
        {
            if (UcFormCommand != null)
            {
                UcFormCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                ////UcFormCommand.ButtonInspection.Click += ButtonInspection_Click;
                //UcFormCommand.ButtonStart.Click += ButtonStart_Click;
                //UcFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcFormCommand.ButtonCompletion.Click += ButtonCompletion_Click;
            }

            if (UcFormSearch != null)
            {
                UcFormSearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcFormSearch.ButtonSearch.Click += ButtonSearch_Click;
                //UcFormSearch.CheckRun.Checked += CheckWait_Checked;
                //UcFormSearch.CheckEqpEnd.Checked += CheckWait_Checked;
                //UcFormSearch.CheckRun.Unchecked += CheckWait_Checked;
                //UcFormSearch.CheckEqpEnd.Unchecked += CheckWait_Checked;

                // 조회 버튼을 실적 대상 옆에 보여 달라고 요청해서 안보이게 함
                UcFormSearch.ButtonSearch.Visibility = Visibility.Collapsed;
            }

            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.CircularReTubing;

            ApplyPermissions();
            Initialize();

            if (_isLoaded == false)
            {
                SetWorkOrder();
                SetRetube();

                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                {
                    ////ButtonSearch_Click(UcFormSearch.ButtonSearch, null);
                    GetEqptWrkInfo();
                }
            }
            _isLoaded = true;
            this.Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UcFormProductionPallet.DispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ButtonExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            C1DropDownButton btn = sender as C1DropDownButton;
            if (btn != null) btn.IsDropDownOpen = false;
        }
 
        private void ButtonCompletion_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            dgRetube.EndEditRow(true);

            if (!ValidationCompletion())
                return;

            object[] param = new object[] { txtLotID.Text };

            string strMsgid = string.Empty;
            if(rdoReTube.IsChecked == true)
            {
                strMsgid = "SFU4047";   //조립 LOT [%1]에 대해 재튜빙 작업을 완료하시겠습니까?
            }
            else
            {
                strMsgid = "SFU5021";   //ERP 실적 전송되지 않는 작업입니다. 조립 LOT [%1]에 대해 재튜빙 작업을 완료하시겠습니까?
            }

            Util.MessageConfirm(strMsgid, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 재튜빙 작업 사유
                    CompletionProcess();
                }
            }, param);

        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                ////ClearControls();
                UcFormShift.ClearShiftControl();

                // 설비 선택 시 자동 조회 처리
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if (ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        ////this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcFormSearch.ButtonSearch, null)));
                        GetEqptWrkInfo();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            SetWorkOrder();
            ////SetRetube();
            SetAssyLot();
            GetEqptWrkInfo();
            SetBeforeAssyLot();
        }

        private void txtLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetAssyLot();
                SetBeforeAssyLot();
            }
        }

        #region [Wo 계획 년월]
        private void dtpWOYM_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetWorkOrder();
        }
        #endregion

        #region [WO 선택하기]
        private void dgWOChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dtWO = DataTableConverter.Convert(dgWO.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dtWO.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dtWO.Rows[idx]["CHK"] = 1;
                dtWO.AcceptChanges();

                Util.GridSetData(dgWO, dtWO, null, true);
                ////dgWO.ItemsSource = DataTableConverter.Convert(dtWO);

                //row 색 바꾸기
                dgWO.SelectedIndex = idx;
            }

        }
        #endregion

        #region [튜빙 작업사유 입력 - dgRetube_CommittedEdit]
        private void dgRetube_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (!e.Cell.Row.Type.Equals(DataGridRowType.Item))
                return;

            if (dgRetube.Rows.Count == 0)
                return;

            int resnQtySum = 0;

            foreach (var column in dgRetube.Columns)
            {
                if (column.GetCellValue(dgRetube.Rows[0]) == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(column.GetCellValue(dgRetube.Rows[0]).ToString()))
                    resnQtySum += int.Parse(column.GetCellValue(dgRetube.Rows[0]).ToString());
            }

            txtGoodQty.Value = resnQtySum;

        }

        #endregion

        #region 작업조
        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);
            grdMain.Children.Add(popupShiftUser);
            popupShiftUser.BringToFront();
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #endregion

        #region Mehod

        #region [WO 정보 조회]
        private void SetWorkOrder()
        {
            try
            {
                ShowLoadingIndicator();

                dtpWOYM.SelectedDataTimeChanged -= dtpWOYM_SelectedDataTimeChanged;

                DateTime day = dtpWOYM.SelectedDateTime.AddMonths(1);
                day = Convert.ToDateTime(day.ToString("yyyy-MM") + "-01");
                day = day.AddDays(-1);

                string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_WITH_FP_BY_LINE_FO";

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WORKORDER_LIST_BY_LINE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                if ((bool)chkWOYM.IsChecked)
                {
                    newRow["STDT"] = dtpWOYM.SelectedDateTime.ToString("yyyyMM") + "01";
                    newRow["EDDT"] = day.ToString("yyyyMMdd");
                }
                else
                {
                    //계획월 체크가 없으면 전체 WO 조회
                    newRow["STDT"] = "19000101";
                    newRow["EDDT"] = "30001231";
                }
                newRow["PROC_EQPT_FLAG"] = "LINE";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgWO, dtResult, null, true);

                dtpWOYM.SelectedDataTimeChanged += dtpWOYM_SelectedDataTimeChanged;

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [재튜빙 작업사유 정보 조회]
        private void SetRetube()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "FORM_RETUBE_RESN_CODE";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                Util.gridClear(dgRetube);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dtbind = new DataTable();

                    // 칼럼 생성
                    for (int row = 0; row < dtResult.Rows.Count; row++)
                        dtbind.Columns.Add(dtResult.Rows[row]["CBO_CODE"].ToString(), typeof(Int16));

                    // Data 생성
                    DataRow inputRow = dtbind.NewRow();

                    for (int row = 0; row < dtResult.Rows.Count; row++)
                        inputRow[row] = 0;

                    dtbind.Rows.Add(inputRow);

                    Util.GridSetData(dgRetube, dtbind, null);

                    // Grid Settung 
                    foreach (var column in dgRetube.Columns)
                    {
                        column.Header = dtResult.Rows[column.Index]["CBO_NAME"].ToString();
                        (column as C1.WPF.DataGrid.DataGridNumericColumn).ShowButtons = false;
                    }

                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [조립 Lot 정보 조회]
        private void SetAssyLot()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLotID.Text))
                    return;

                DateTime day = dtpWOYM.SelectedDateTime.AddMonths(1);
                day = Convert.ToDateTime(day.ToString("yyyy-MM") + "-01");
                day = day.AddDays(-1);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));
                //inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["ASSY_LOTID"] = txtLotID.Text;
                newRow["USERID"] = LoginInfo.USERID;
                if ((bool)chkWOYM.IsChecked)
                {
                    newRow["STDT"] = dtpWOYM.SelectedDateTime.ToString("yyyyMM") + "01";
                    newRow["EDDT"] = day.ToString("yyyyMMdd");
                }
                else
                {
                    //계획월 체크가 없으면 전체 WO 조회
                    newRow["STDT"] = "19000101";
                    newRow["EDDT"] = "30001231";
                }
                //newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_WO_RT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtLotID.Text = dtResult.Rows[0]["ASSY_LOTID"].ToString();

                    // WO 선택
                    DataTable dtWO = DataTableConverter.Convert(dgWO.ItemsSource);

                    // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                    dtWO.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);

                    DataRow[] drWO = dtWO.Select("WOID ='" + dtResult.Rows[0]["WOID"].ToString() + "'");
                    int Index = 0;

                    if (drWO.Length > 0)
                    {
                        Index = dtWO.Rows.IndexOf(drWO[0]);
                        dtWO.Rows[Index]["CHK"] = 1;
                    }
                    dtWO.AcceptChanges();

                    Util.GridSetData(dgWO, dtWO, null, true);
                    if (drWO.Length > 0)
                    {
                        dgWO.SelectedIndex = Index;
                    }

                    if (drWO.Length > 0 && Index > 0)
                    {
                        dgWO.ScrollIntoView(Index, 1);
                    }
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 작업완료
        private void CompletionProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_END_PROD_LOT_RT";

                int _rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgWO, "CHK");

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("OUTPUTQTY", typeof(Decimal));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("RE_TUBE_FLAG", typeof(string));

                DataTable inResn = inDataSet.Tables.Add("INRESN");
                inResn.Columns.Add("RE_TUBE_RSN_CODE", typeof(string));
                inResn.Columns.Add("WRK_QTY", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue.GetString());
                newRow["ASSY_LOTID"] = txtLotID.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["OUTPUTQTY"] = txtGoodQty.Value;
                newRow["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[_rowIndex].DataItem, "WO_DETL_ID"));
                newRow["SHIFT"] = UcFormShift.TextShift.Tag.ToString();
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag.ToString();
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;
                if(rdoReTube.IsChecked == true)
                {
                    newRow["RE_TUBE_FLAG"] = "T";
                }
                else
                {
                    newRow["RE_TUBE_FLAG"] = "D";
                }

                //if (string.IsNullOrWhiteSpace(txtBeforeAssyLot.Text) == false)
                //{
                //    newRow["WIPNOTE"] = txtBeforeAssyLot.Text;
                //}
                if (string.IsNullOrWhiteSpace(txtCellPrintContents.Text) == false)
                {
                    newRow["WIPNOTE"] = txtCellPrintContents.Text;
                }
                //if (string.IsNullOrWhiteSpace(txtBeforeAssyLot.Text) == false && string.IsNullOrWhiteSpace(txtCellPrintContents.Text) == false)
                //{
                //    newRow["WIPNOTE"] = txtBeforeAssyLot.Text + " / " + txtCellPrintContents.Text;
                //}

                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridColumn col in dgRetube.Columns)
                {
                    if (Util.NVC_Int(col.GetCellValue(dgRetube.Rows[0]).ToString()) != 0)
                    {
                        newRow = inResn.NewRow();
                        newRow["RE_TUBE_RSN_CODE"] = Util.NVC(col.Name);
                        newRow["WRK_QTY"] = Util.NVC_Int(col.GetCellValue(dgRetube.Rows[0]).ToString());
                        inResn.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        ClearControls();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
                {
                    try
                    {
                        const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                        DataTable inTable = new DataTable("RQSTDT");
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("SHOPID", typeof(string));
                        inTable.Columns.Add("AREAID", typeof(string));
                        inTable.Columns.Add("EQSGID", typeof(string));
                        inTable.Columns.Add("PROCID", typeof(string));

                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                        newRow["PROCID"] = _processCode;

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                if (UcFormShift != null)
                                {
                                    if (result.Rows.Count > 0)
                                    {
                                        if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                        {
                                            UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                        }
                                        else
                                        {
                                            UcFormShift.TextShiftStartTime.Text = string.Empty;
                                        }

                                        if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                        {
                                            UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                        }
                                        else
                                        {
                                            UcFormShift.TextShiftEndTime.Text = string.Empty;
                                        }

                                        if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                        {
                                            UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                        }
                                        else
                                        {
                                            UcFormShift.TextShiftDateTime.Text = string.Empty;
                                        }

                                        if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                        {
                                            UcFormShift.TextWorker.Text = string.Empty;
                                            UcFormShift.TextWorker.Tag = string.Empty;
                                        }
                                        else
                                        {
                                            UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                            UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                        }

                                        if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                        {
                                            UcFormShift.TextShift.Tag = string.Empty;
                                            UcFormShift.TextShift.Text = string.Empty;
                                        }
                                        else
                                        {
                                            UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                            UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                        }
                                    }
                                    else
                                    {
                                        UcFormShift.ClearShiftControl();
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                #endregion

        private bool ClearControls()
        {
            try
            {
                txtLotID.Text = string.Empty;
                txtGoodQty.Value = 0;

                //txtBeforeAssyLot.Text = string.Empty;
                txtCellPrintContents.Text = string.Empty;
                //Util.gridClear(dgWO);
                //Util.gridClear(dgRetube);
                rdoReTube.IsChecked = true;
                Util.gridClear(dgBeforeAssyLot);
                SetGridClear();

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetGridClear()
        {
            foreach (var column in dgRetube.Columns)
            {
                column.SetCellValue(dgRetube.Rows[0], 0);
            }

        }

        private bool ValidationSearch()
        {

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return false;
            }

            ////if (string.IsNullOrWhiteSpace(txtLotID.Text))
            ////{
            ////    // 조립 Lot 정보가 없습니다.
            ////    Util.MessageValidation("SFU4001");
            ////    return false;
            ////}

            return true;
        }

        private bool ValidationCompletion()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            int _rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgWO, "CHK");

            if (_rowIndex < 0)
            {
                // 작업지시를 선택하세요.
                Util.MessageValidation("SFU1443");
                return false;
            }

            if ((txtGoodQty.Value.ToString().Equals("NaN") || txtGoodQty.Value == 0))
            {
                // 양품량을 입력해주세요.
                Util.MessageValidation("SFU2921");
                return false;
            }

            //if (string.IsNullOrWhiteSpace(txtBeforeAssyLot.Text))
            //{
            //    // 변경전 조립 LOT 정보가 없습니다.
            //    Util.MessageValidation("SFU4521");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(txtCellPrintContents.Text))
            {
                // 셀 인쇄내용 정보가 없습니다.
                Util.MessageValidation("SFU4522");
                return false;
            }

            return true;
        }

        private void SetComboBox()
        {
            SetEquipmentCombo(ComboEquipment);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _processCode, null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            //////////////////// 설비가 N건인 경우 Select 추가
            if (ComboEquipment.Items.Count > 1 || ComboEquipment.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int Index = 0;

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                    if (drIndex.Length > 0)
                    {
                        Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                }

                cbo.SelectedIndex = Index;
            }

        }

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
            var ucFormCommand = grdCommand.Children[0] as UcFormCommand;
            if (ucFormCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    ucFormCommand.ButtonInspection,
                    ucFormCommand.ButtonStart,
                    ucFormCommand.ButtonCancel,
                    ucFormCommand.ButtonCompletion
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        #endregion

        private void chkWOYM_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkWOYM.IsChecked)
            {
                dtpWOYM.IsEnabled = true;
            }
            else
            {
                dtpWOYM.IsEnabled = false;
            }
        }

        private void chkWOYM_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkWOYM.IsChecked)
            {
                dtpWOYM.IsEnabled = true;
            } else
            {
                dtpWOYM.IsEnabled = false;
            }
        }

        private void SetBeforeAssyLot()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLotID.Text))
                    return;

                Util.gridClear(dgBeforeAssyLot);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtLotID.Text;
                newRow["DEL_FLAG"] = "N";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RE_TUBE_LOT_GNRT_HIST", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgBeforeAssyLot, dtResult, null, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
