/*************************************************************************************
 Created Date : 2021.12.22
      Creator : 신광희
   Decription : CNB2동 증설 - 대기 Pancake, 대기반제품 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.12.22  신광희 : Initial Created.
  2022.04.04  김태균 : 투입 대상 생산 Lot 미 선택 시 알람 처리
  2022.05.03  배현우 : 투입위치에 따른 반제품 조회 수정 및 WOID를 이용하여 대기 반제품 조회
  2022.06.02  배현우 : 대기반제품 투입시 장비ID가 라인ID로 들어가는 오류 수정
  2023.06.21  배현우 : Winding 대기 반제품 조회시 inputClassCode 코드 매핑 오류 수정
  2024.02.22  백광영 : 설비 재작업 모드 -> 대기 반제품 조회 공정 AC003 적용
  2024.07.22  백광영 : 설비 재작업 모드 제거
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace LGC.GMES.MES.ASSY006
{
    /// <summary>
    /// ASSY006_OUTLOT_MERGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY006_WAIT_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _prodWorkOrderId = string.Empty;
        private string _equipmentMountPositionId = string.Empty;
        private string _sVal001 = string.Empty;
        private bool _isWaitUseAuthority = false;

        private string _inputLotID = string.Empty;

        private string _maxPeviewProcessEndDay = string.Empty;
        private DateTime _dtMinValid;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();


        private string _equipmentGroupCode = string.Empty;

        public bool IsUpdated { get; set; }

        public string InputLotID
        {
            get { return _inputLotID; }
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

        public ASSY006_WAIT_LOT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControl();
            SetControlVisibility();

            // 선입선출 기준일 조회.
            GetProcMtrlInputRule();

            // 조회
            if (_processCode == Process.WINDING || _processCode == Process.ZZS)
            {
                GetWaitPancake();
            }
            else if (_processCode == Process.ASSEMBLY)
            {
                CheckInline(_equipmentSegmentCode);
                GetWaitHalfProductList();
            }
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _equipmentSegmentCode = Util.NVC(tmps[0]);
            _processCode = Util.NVC(tmps[1]);
            _equipmentCode = Util.NVC(tmps[2]);
            _ProdLotID = Util.NVC(tmps[3]);
            _equipmentMountPositionId = Util.NVC(tmps[4]);
            _isWaitUseAuthority = (bool)tmps[5];
            _prodWorkOrderId = Util.NVC(tmps[6]);
            _sVal001 = Util.NVC(tmps[7]);

            txtEquipment.Text = _equipmentCode;
            txtProdLotID.Text = _ProdLotID;
            //txtMountPstnID.Text = _equipmentMountPositionId;
            txtMountPstnID.Text = _sVal001;

            if (_isWaitUseAuthority == false)
                btnInput.Visibility = Visibility.Collapsed;

            if(string.Equals(_processCode, Process.WINDING) || string.Equals(_processCode, Process.ZZS))
            {
                Header = ObjectDic.Instance.GetObjectName("대기PANCAKE");
                dgWaitPancake.Columns["VALID_DATE"].Visibility = Visibility.Collapsed;
            }
            else if(string.Equals(_processCode, Process.ASSEMBLY))
            {
                Header = ObjectDic.Instance.GetObjectName("대기반제품");

                dgWaitHalfProduct.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                dgWaitHalfProduct.Columns["LOTID"].Visibility = Visibility.Visible;
                //tbWaitLotId.Text = ObjectDic.Instance.GetObjectName("LOT ID");
                //btnInHalfProductInPutQty.Visibility = Visibility.Visible;
            }
        }
        
        private void SetControlVisibility()
        {
            tbPancake.Visibility = Visibility.Collapsed;
            tbWaitHalfProduct.Visibility = Visibility.Collapsed;

            if(string.Equals(_processCode, Process.WINDING) || string.Equals(_processCode, Process.ZZS))
            {
                tbPancake.Visibility = Visibility.Visible;
            }
            else if(string.Equals(_processCode, Process.ASSEMBLY))
            {
                tbWaitHalfProduct.Visibility = Visibility.Visible;
            }
        }

        private void dgWaitPancake_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                // 대기 1개만 선택.
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null && dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null && checkBox.IsChecked.HasValue && !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWaitPancake_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_maxPeviewProcessEndDay).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay;
                        int.TryParse(_maxPeviewProcessEndDay, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_dtMinValid.AddDays(iDay) >= dtValid)
                                {
                                    var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                                    if (convertFromString != null)
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitPancake_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgWaitHalfProduct_OnCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            DataGridCurrentCellChanged(sender, e);
        }

        private void DataGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            if (dg != null)
                            {
                                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                         checkBox.IsChecked.HasValue &&
                                                         !(bool)checkBox.IsChecked))
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                if (chk != null) chk.IsChecked = false;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                        dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                        box.IsChecked.HasValue &&
                                                        (bool)box.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;
                                    }
                                }
                            }
                            break;
                    }
                    if (dg?.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInput()) return;

            Util.MessageConfirm("SFU1248", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if(string.Equals(_processCode, Process.WINDING))
                    {
                        InputWaitPancake();
                    }
                    else if(string.Equals(_processCode, Process.ASSEMBLY))
                    {
                        WaitHalfProductInput();
                    }
                    else if (string.Equals(_processCode, Process.ZZS))
                    {
                        InputWaitPancakeZZS();
                    }
                }
            });

            //this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetProcMtrlInputRule()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MTRL_INPUT_RULE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _maxPeviewProcessEndDay = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                //HideParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetInputMtrlClssCode()
        {
            try
            {
                string sInputMtrlClssCode = "";

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _equipmentCode;
                newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void CheckInline(string equipmentSegmentCode)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LDR_FLAG";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["EQSGID"] = equipmentSegmentCode;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (searchResult.Rows.Count > 0)
            {
                dgWaitHalfProduct.Columns["INLINE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgWaitHalfProduct.Columns["INLINE"].Visibility = Visibility.Collapsed;
            }
        }

        private void GetWaitPancake()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.Equals(_processCode, Process.WINDING))
                    bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_WN_BY_LV3_CODE";
                else if (string.Equals(_processCode, Process.ZZS))
                    bizRuleName = "DA_PRD_SEL_WAIT_LOT_LIST_ZZS";

                string inputClassCode;
                if (string.IsNullOrEmpty(txtMountPstnID.Text))
                {
                    inputClassCode = string.Empty;
                }
                else
                {
                    string[] postionStrings = _equipmentMountPositionId.Split('_');
               
                    inputClassCode = postionStrings.Length > 0 ? postionStrings[0] : string.Empty;

                    if (inputClassCode.Equals("NEXT"))
                    {
                        inputClassCode = postionStrings.Length > 1  ?postionStrings[1] : string.Empty; //임시적으로 투입위치 [0]문자열이 NEXT일때 [1] 문자열을 이용하도록 수정 2022-05-03
                    }
                }


                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                if (string.Equals(_processCode, Process.WINDING))
                    inTable = _bizDataSet.GetDA_PRD_SEL_READY_LOT_LM();
                else if (string.Equals(_processCode, Process.ZZS))
                    inTable = _bizDataSet.GetDA_PRD_SEL_READY_LOT_ZZS();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["IN_LOTID"] = null;
                
                if (string.Equals(_processCode, Process.WINDING))
                {
                    newRow["EQSGID"] = _equipmentSegmentCode;
                    newRow["WOID"] = _prodWorkOrderId;
                    newRow["INPUT_MTRL_CLSS_CODE"] = inputClassCode; // string.Empty;
                }
                else if (string.Equals(_processCode, Process.ZZS))
                {
                    newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId;
                }

                inTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (CommonVerify.HasTableRow(searchResult) && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _dtMinValid);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetWaitHalfProductList()
        {
            try
            {
                Util.gridClear(dgWaitHalfProduct);
                string bizRuleName = "DA_PRD_SEL_WAIT_HALFPROD_AS_WO";

                DataTable indataTable = new DataTable();
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));
                indataTable.Columns.Add("INPUT_LOTID", typeof(string));
                indataTable.Columns.Add("WOID", typeof(string));

                DataRow dr = indataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["PROCID"] = _processCode;
                dr["INPUT_LOTID"] = null;
                dr["WOID"] = _prodWorkOrderId;
                indataTable.Rows.Add(dr);


                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgWaitHalfProduct.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputWaitPancake()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_WN";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId;
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;

                        GetWaitPancake();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void InputWaitPancakeZZS()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_PRD_REG_START_INPUT_IN_LOT_ZZS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_LM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId;
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        IsUpdated = true;

                        GetWaitPancake();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void WaitHalfProductInput()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                // 추후 분리작업이 필요할 수 있음

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgWaitHalfProduct, "CHK", true);

                DataSet inDataSet = new DataSet();
                string bizRuleName = string.Empty;
                string outData = null;

                bizRuleName = "BR_PRD_REG_INPUT_LOT_AS";

                DataTable inDataTable = inDataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = _ProdLotID;
                dr["EQPT_LOTID"] = null;
                inDataTable.Rows.Add(dr);

                DataTable ininput = inDataSet.Tables.Add("IN_INPUT");
                ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                ininput.Columns.Add("PRODID", typeof(string));
                ininput.Columns.Add("WINDING_RUNCARD_ID", typeof(string));
                ininput.Columns.Add("INPUT_QTY", typeof(decimal));

                DataRow newRow = ininput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _equipmentMountPositionId;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "PRODID"));
                newRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "LOTID"));
                newRow["INPUT_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgWaitHalfProduct.Rows[rowIndex].DataItem, "WIPQTY2"));

                ininput.Rows.Add(newRow);
                outData = "OUT_EQP";

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", outData, (bizResult, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    IsUpdated = true;
                    GetWaitHalfProductList();
                    Util.MessageInfo("SFU1275");

                }, inDataSet);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInput);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region [Validation]
        private bool ValidationInput()
        {
            if(string.Equals(_processCode, Process.WINDING))
            {
                if (_util.GetDataGridCheckCnt(dgWaitPancake, "CHK") < 1)
                {
                    // "선택된 항목이 없습니다."
                    Util.MessageValidation("SFU1651");
                    return false;
                }
            }
            else if(string.Equals(_processCode, Process.ASSEMBLY))
            {
                if (_util.GetDataGridCheckCnt(dgWaitHalfProduct, "CHK") < 1)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }
            }

            //if (string.IsNullOrWhiteSpace(_inputLotID))
            //{
            //    // 선택된 항목이 없습니다.
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (string.IsNullOrEmpty(_ProdLotID))
            {
                // 선택된 LOT 이 존재하지 않습니다.
                Util.MessageValidation("SFU1137");
                return false;
            }

            return true;
        }


        #endregion

        #endregion


    }
}
