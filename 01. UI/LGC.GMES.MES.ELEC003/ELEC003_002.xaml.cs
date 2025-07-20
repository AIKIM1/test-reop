/*************************************************************************************
 Created Date : 2020.12.02
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 투입요청서
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.02  조영대 : Initial Created
  2021.09.28  김지은 : 투입 단위 중량 기준 추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System.ComponentModel;

namespace LGC.GMES.MES.ELEC003
{
    public partial class ELEC003_002 : UserControl, IWorkArea
    {
        #region Declaration


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string selectedVersionCheckFlag = string.Empty;
        private bool HopperMtrlFlag = false;

        private string saveMatrialId = string.Empty;
        private string saveHopperId = string.Empty;

        private enum CheckBoxHeaderType
        {
            Zero,
            One
        }

        private CheckBoxHeaderType _HeaderType = CheckBoxHeaderType.Zero;

        #endregion

        #region Initialize
        public ELEC003_002()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox();

            InitializeControls();

            object[] parameters = this.FrameOperation.Parameters;

            if (parameters != null && parameters.Length > 0)
            {
                popMonitoring.ProcessCode = Util.NVC(parameters[0]);
            }
        }

        private void SetComboBox()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            CommonCombo combo = new CommonCombo();

            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "ProcessCWA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);

            cboEquipment.SelectedIndex = 0;
        }

        private void InitializeControls()
        {
            SetDate();

            popMonitoring.FrameOperation = FrameOperation;

            btnRequest.Content = "▶▶▶ " + Util.NVC(btnRequest.Content);

            grdMain.RowDefinitions[0].Height = new GridLength(0);
            grdMain.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
        }


        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            grdMain.RowDefinitions[0].Height = new GridLength(0);
            grdMain.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
        }

        private void popMonitoring_MinusButtonClick(object sender, string equipmentSegment, string process, string equipment)
        {
            cboEquipmentSegment.SelectedValue = equipmentSegment;
            cboProcess.SelectedValue = process;

            grdMain.RowDefinitions[1].Height = new GridLength(0);
            grdMain.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgWorkOrder.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgInputMaterial.Columns)
            {
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            HopperMtrlFlag = false;
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty && Util.NVC(cboProcess.SelectedValue) != string.Empty)
            {
                if (Util.NVC(cboProcess.SelectedValue).Equals(Process.MIXING) || Util.NVC(cboProcess.SelectedValue).Equals(Process.SRS_MIXING) || Util.NVC(cboProcess.SelectedValue).Equals(Process.PRE_MIXING))
                {
                    IsHopperMtrlChk();
                }
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            if (e.NewValue != null && e.OldValue != null && !e.NewValue.Equals(e.OldValue))
            {
                Refresh();
            }
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                int rowIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

                if (selectedVersionCheckFlag.Equals("Y"))
                {
                    DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
                    if (selectWO != null)
                    {
                        string Version = Util.NVC(selectWO["PROD_VER_CODE"]);

                        if (Version.Equals(string.Empty))
                        {
                            Util.gridClear(dgInputMaterial);
                            Util.MessageValidation("SFU5036");
                            return;
                        }
                    }
                }

                dgWorkOrder.SelectedIndex = rowIndex;

                SearchInputMaterial();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgInputMaterial;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void dgInputMaterial_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bool isCheck = Convert.ToBoolean(DataTableConverter.GetValue(dgInputMaterial.CurrentRow.DataItem, "CHK"));

                if (isCheck == true)
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;    // Editing 불가능
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputMaterial_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                switch (e.Column.Name)
                {
                    case "MTRL_QTY":
                        {
                            bool isCheck = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));

                            if (isCheck == true)
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                e.Cancel = true;    // Editing 불가능
                            }
                            break;
                        }
                    case "MTRL_BAG_QTY":
                        {
                            bool isCheck = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));
                            bool isBagCnt = (DataTableConverter.GetValue(e.Row.DataItem, "MTRL_BAG_QTY_CHK_FLAG").ToString().Equals("Y")) ? true : false;

                            if (isCheck && isBagCnt)
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                e.Cancel = true;    // Editing 불가능
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputMaterial_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (!dgInputMaterial.GetValue(e.Cell.Row.Index, "MTRL_BAG_QTY_CHK_FLAG").ToString().Equals("Y"))
                    return;

                if (!e.Cell.Column.Name.Equals("MTRL_QTY") && !e.Cell.Column.Name.Equals("MTRL_BAG_QTY"))
                    return;

                decimal dConvertVal = GetMtrlInputUnitWeight(dgInputMaterial.GetValue(e.Cell.Row.Index, "MTRLID").ToString().Trim());
                if (dConvertVal == 0)
                {
                    Util.MessageValidation("SFU8406");  //자재 투입 단위 중량을 확인 해 주세요.
                    dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_QTY", string.Empty);
                    dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_BAG_QTY", string.Empty);
                    return;
                }
                switch (e.Cell.Column.Name)
                {
                    case "MTRL_QTY":
                        {
                            decimal dInputVal = Convert.ToDecimal(e.Cell.Value);
                            decimal dOutputVal = dInputVal / dConvertVal;
                            if (dOutputVal != (int)dOutputVal)
                            {
                                //투입요청수량은 정수만 입력 하세요.
                                Util.MessageValidation("SFU1977");
                                dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_QTY", string.Empty);
                                dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_BAG_QTY", string.Empty);
                                return;
                            }
                            dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_BAG_QTY", dOutputVal);
                            break;
                        }
                    case "MTRL_BAG_QTY":
                        {
                            decimal dInputVal = Convert.ToDecimal(e.Cell.Value);
                            decimal dOutputVal = dInputVal * dConvertVal;
                            dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_QTY", dOutputVal);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_QTY", string.Empty);
                dgInputMaterial.SetValue(e.Cell.Row.Index, "MTRL_BAG_QTY", string.Empty);
            }
        }

        private void dgInputMaterial_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgInputMaterial.ItemsSource == null || dgInputMaterial.Rows.Count == 0)
                    return;

                if (dgInputMaterial.CurrentRow == null || dgInputMaterial.CurrentRow.DataItem == null)
                    return;

                if (grdMain.RowDefinitions[0].Height.Equals(new GridLength(0))) return;

                DataRow selectWO = dgWorkOrder.GetCheckedDataRow("CHK").FirstOrDefault();
                if (selectWO == null)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                dgInputMaterial.EndEditRow(true);

                CheckBox chkCurrent = sender as CheckBox;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)chkCurrent.Parent).Row.Index;

                dgInputMaterial.SelectedIndex = rowIndex;

                DataRow selectMtl = dgInputMaterial.GetDataRow(rowIndex);

                C1.WPF.DataGrid.DataGridCell gridCell = dgInputMaterial.GetCell(rowIndex, dgInputMaterial.Columns["HOPPER_ID"].Index);
                if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                {
                    C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;

                    if (combo != null)
                    {
                        if (chkCurrent.IsChecked.Equals(true))
                        {

                            if (selectMtl != null)
                            {
                                SetGridCboItem(combo, Util.NVC(cboEquipment.SelectedValue), Util.NVC(selectMtl["MTRLID"]));
                            }

                            combo.Visibility = Visibility.Visible;
                            combo.SelectedIndex = 0;
                        }
                        else
                        {
                            combo.Visibility = Visibility.Hidden;

                            dgInputMaterial.SetValue(rowIndex, "MTRL_QTY", string.Empty);
                            dgInputMaterial.SetValue(rowIndex, "MTRL_BAG_QTY", string.Empty);
                            dgInputMaterial.SetValue(rowIndex, "HOPPER_ID", string.Empty);
                            dgInputMaterial.SetValue(rowIndex, "MTRL_BAG_QTY_CHK_FLAG", string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputMaterialCombo_SelectedIndexChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox combo = sender as C1ComboBox;
            if (combo.SelectedItem != null)
            {
                DataRowView drv = combo.SelectedItem as DataRowView;
                string sBagQtyChkFlag = string.IsNullOrEmpty(drv.Row["MTRL_BAG_QTY_CHK_FLAG"].ToString()) ? "N" : drv.Row["MTRL_BAG_QTY_CHK_FLAG"].ToString();
                dgInputMaterial.SetValue(((DataGridCellPresenter)combo.Parent).Cell.Row.Index, "MTRL_BAG_QTY_CHK_FLAG", sBagQtyChkFlag);
            }
        }

        private void dgInputMaterial_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                TextBlock header = e.OriginalSource as TextBlock;
                if (header.Text.Equals(ObjectDic.Instance.GetObjectName("선택")))
                {
                    C1DataGrid dg = dgInputMaterial;
                    if (dg?.ItemsSource == null) return;

                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        switch (_HeaderType)
                        {
                            case CheckBoxHeaderType.Zero:
                                DataTableConverter.SetValue(row.DataItem, "CHK", true);
                                break;
                            case CheckBoxHeaderType.One:
                                DataTableConverter.SetValue(row.DataItem, "CHK", false);
                                break;
                        }
                    }

                    switch (_HeaderType)
                    {
                        case CheckBoxHeaderType.Zero:
                            _HeaderType = CheckBoxHeaderType.One;
                            break;
                        case CheckBoxHeaderType.One:
                            _HeaderType = CheckBoxHeaderType.Zero;
                            break;
                    }

                    dg.EndEdit();
                    dg.EndEditRow(true);
                }
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                int rowIndex = dgInputMaterial.SelectedIndex;

                DataView tempView = dgInputMaterial.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();

                if (rowIndex != 0)
                {
                    tempTable.Rows[rowIndex - 1]["RowSequenceNo"] = rowIndex;
                    tempTable.Rows[rowIndex]["RowSequenceNo"] = rowIndex - 1;

                    Util.GridSetData(dgInputMaterial, tempTable.Select("", "RowSequenceNo").CopyToDataTable(), FrameOperation, true);

                    Util.gridSetFocusRow(ref dgInputMaterial, --rowIndex);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = dgInputMaterial.SelectedIndex;

                DataView tempView = dgInputMaterial.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();

                if (rowIndex != tempTable.Rows.Count - 1)
                {
                    tempTable.Rows[rowIndex + 1]["RowSequenceNo"] = rowIndex;
                    tempTable.Rows[rowIndex]["RowSequenceNo"] = rowIndex + 1;

                    Util.GridSetData(dgInputMaterial, tempTable.Select("", "RowSequenceNo").CopyToDataTable(), FrameOperation, true);

                    Util.gridSetFocusRow(ref dgInputMaterial, ++rowIndex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            SaveInputRequest();
        }

        private void popMonitoring_HopperDoubleClick(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId)
        {
            saveMatrialId = materialId;
            saveHopperId = hopperId;

            cboEquipment.SelectedIndex = 0;

            cboEquipmentSegment.SelectedValue = equipmentSegment;
            cboProcess.SelectedValue = process;
            cboEquipment.SelectedValue = equipment;

            grdMain.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            grdMain.RowDefinitions[1].Height = new GridLength(0);

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgWorkOrder.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgInputMaterial.Columns)
            {
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            }
        }

        private void ucHopperList_HopperDoubleClick(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId)
        {
            int findIndex = Util.gridFindDataRow(ref dgInputMaterial, "MTRLID", materialId, true);
            if (findIndex > -1)
            {
                DataTableConverter.SetValue(dgInputMaterial.Rows[findIndex].DataItem, "CHK", true);
                DataTableConverter.SetValue(dgInputMaterial.Rows[findIndex].DataItem, "HOPPER_ID", hopperId);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime > dtpDateTo.SelectedDateTime)
            {
                dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime;
            }

            SearchWorkOrder();
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateTo.SelectedDateTime < dtpDateFrom.SelectedDateTime)
            {
                dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            }

            SearchWorkOrder();
        }

        // 2021.11.15 한종현. 기존 투입요청서의 Proc로 WO 조회하는 기능 추가
        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            SearchWorkOrder();
        }

        // 2021.11.15 한종현. 기존 투입요청서의 Proc로 WO 조회하는 기능 추가
        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchWorkOrder();
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnRequest
            };

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

        private void GetVersionCheckFlag()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQUIPID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQUIPID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_VER_CHK_FLAG", "INDATA", "OUTDATA", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    selectedVersionCheckFlag = dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private decimal GetMtrlInputUnitWeight(string sMtrlId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["MTRLID"] = sMtrlId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MTRL_INPUT_UNIT_WEIGHT", "INDATA", "OUTDATA", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    decimal dReturn = 0;
                    decimal.TryParse(dt.Rows[0]["WEIGHT"].ToString(), out dReturn);
                    return dReturn;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private void IsHopperMtrlChk()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.Equals(dtResult.Rows[0]["MIXER_HOPPER_MTRL_USE_FLAG"], "Y"))
                    {
                        HopperMtrlFlag = true;
                        return;
                    }
                }
            }
            catch (Exception ex) { }

            HopperMtrlFlag = false;
        }

        private void SetDate()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    dtpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

                    return;
                }
                dtpDateFrom.Text = result.Rows[0][0].ToString();
                dtpDateTo.Text = result.Rows[0][0].ToString();

                dtpDateFrom.SelectedDateTime = Convert.ToDateTime(result.Rows[0][0].ToString());
                dtpDateTo.SelectedDateTime = Convert.ToDateTime(result.Rows[0][0].ToString());
            }
            );
        }

        private void SetGridCboItem(C1ComboBox combo, string sEQPTID, string sMTRLID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                Indata["MTRLID"] = sMTRLID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(HopperMtrlFlag == true ? "DA_BAS_SEL_HOPPER_TANK_MTRL_SET_CBO_CNB" : "DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count == 0) { return; }
                combo.ItemsSource = DataTableConverter.Convert(dtMain);

                if (combo != null && combo.Items.Count == 1)
                {
                    DataTable distinctTable = ((DataView)dgInputMaterial.ItemsSource).Table.DefaultView.ToTable(true, "HOPPER_ID");
                    foreach (DataRow row in distinctTable.Rows)
                        if (string.Equals(row["HOPPER_ID"], dtMain.Rows[0][0]))
                            return;

                    combo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ClearControl()
        {
            Util.gridClear(dgWorkOrder);
            Util.gridClear(dgInputMaterial);

            this.txtRemark.Clear();
            this.txtWorker.Clear();
            this.txtWorker.Tag = string.Empty;

            ucHopperList.ClearHopperData();
        }

        private void Refresh()
        {
            SearchHopperInEqpt();

            SearchWorkOrder();

            GetVersionCheckFlag();
        }

        private void SearchHopperInEqpt()
        {
            try
            {
                if (Util.IsNVC(cboEquipmentSegment.SelectedValue) ||
                   Util.IsNVC(cboProcess.SelectedValue) ||
                   Util.IsNVC(cboEquipment.SelectedValue)) return;

                ShowLoadingIndicator();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_HOPPER_MONITER_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ucHopperList.SetHopperData(result.AsEnumerable().ToArray());

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SearchWorkOrder()
        {
            try
            {
                if (Util.IsNVC(cboEquipmentSegment.SelectedValue) ||
                    Util.IsNVC(cboProcess.SelectedValue) ||
                    Util.IsNVC(cboEquipment.SelectedValue)) return;

                ShowLoadingIndicator();

                // 2021.11.15 한종현.기존 투입요청서의 Proc로 WO 조회하는 기능 추가
                bool isProc = false;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                Util.gridClear(dgWorkOrder);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);

                // 2021.11.15 한종현.기존 투입요청서의 Proc로 WO 조회하는 기능 추가
                if (isProc == false)
                    Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                Indata["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                IndataTable.Rows.Add(Indata);

                // 2021.11.15 한종현.기존 투입요청서의 Proc로 WO 조회하는 기능 추가
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_WORKORDER_DRB", "RQSTDT", "RSLTDT", IndataTable);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync(isProc ? "DA_PRD_SEL_MIXMTRL_PROC_WORKORDER_DRB" : "DA_PRD_SEL_MIXMTRL_WORKORDER_DRB", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgWorkOrder, dtMain, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SearchInputMaterial()
        {
            try
            {
                //Util.gridClear(dgInputMaterial);
                dgInputMaterial.ClearRows();

                DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
                if (selectWO == null)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                IndataTable.Columns.Add("ALL_FLAG", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WOID"] = Util.NVC(selectWO["WOID"]);
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = Util.NVC(selectWO["PRODID"]);
                Indata["PROD_VER_CODE"] = Util.NVC(selectWO["PROD_VER_CODE"]);

                if (Util.NVC(selectWO["PRODID"]).Length > 5 &&
                    Util.NVC(selectWO["PRODID"]).Substring(3, 2).Equals("CA"))
                {
                    Indata["ALL_FLAG"] = "Y";
                }
                else
                {
                    Indata["ALL_FLAG"] = null;
                }

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCHORDID_MATERIAL_LIST_CWA", "INDATA", "OUTDATA", IndataTable, RowSequenceNo: true);
                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgInputMaterial, dtMain, FrameOperation, true);

                dgInputMaterial.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!saveMatrialId.Equals(string.Empty) && !saveHopperId.Equals(string.Empty))
                        {
                            int findIndex = Util.gridFindDataRow(ref dgInputMaterial, "MTRLID", saveMatrialId, true);
                            if (findIndex > -1)
                            {
                                DataTableConverter.SetValue(dgInputMaterial.Rows[findIndex].DataItem, "CHK", true);
                                DataTableConverter.SetValue(dgInputMaterial.Rows[findIndex].DataItem, "HOPPER_ID", saveHopperId);
                            }

                            saveMatrialId = saveHopperId = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }));


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtWorker.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = wndPerson.USERNAME;
                txtWorker.Tag = wndPerson.USERID;
                txtPersonId.Text = wndPerson.USERID;
            }
        }

        private bool CheckSaveValidation()
        {
            if (dgInputMaterial.Rows.Count < 1 || dgInputMaterial.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1833");  //자재정보가 없습니다.
                return false;
            }

            DataRow[] selectRows = Util.gridGetChecked(ref dgInputMaterial, "CHK");
            if (selectRows == null || selectRows.Length.Equals(0))
            {
                Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                return false;
            }

            foreach (DataRow drRow in selectRows)
            {
                if (Util.NVC(drRow["MTRL_QTY"]).Equals(string.Empty) || Util.NVC(drRow["MTRL_QTY"]).Equals("0"))
                {
                    //투입요청수량을 입력 하세요.
                    Util.MessageValidation("SFU1978");
                    return false;
                }

                if (Util.NVC_Decimal(drRow["MTRL_QTY"]) < 0)
                {
                    //투입요청수량은 정수만 입력 하세요.
                    Util.MessageValidation("SFU1977");
                    return false;
                }

                if (drRow["HOPPER_ID"].Equals(string.Empty))
                {
                    Util.MessageValidation("SFU2035");  //호퍼를 선택하세요.
                    return false;
                }

            }

            if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace((string)txtWorker.Tag))
            {
                // 요청자를 선택해 주세요.
                Util.MessageValidation("SFU3467");
                return false;
            }

            DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();
            if (selectWO == null)
            {
                Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                return false;
            }

            return true;
        }

        private void SaveInputRequest()
        {
            try
            {
                dgInputMaterial.EndEdit();

                if (!CheckSaveValidation()) return;


                DataRow selectWO = Util.gridGetChecked(ref dgWorkOrder, "CHK").First();

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                inDataTable.Columns.Add("CMC_BINDER_FLAG", typeof(string));

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inDataRow["WOID"] = Util.NVC(selectWO["WOID"]);
                inDataRow["NOTE"] = Util.NVC(txtRemark.Text.ToString());
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["REQ_USERID"] = (string)txtWorker.Tag;
                inDataRow["BTCH_ORD_ID"] = Util.NVC(selectWO["BTCH_ORD_ID"]);
                inDataRow["CMC_BINDER_FLAG"] = "N";
                inDataTable.Rows.Add(inDataRow);

                DataTable InDetailTable = inDataSet.Tables.Add("INDTLDATA");
                InDetailTable.Columns.Add("MTRLID", typeof(string));
                InDetailTable.Columns.Add("MTRL_QTY", typeof(string));
                InDetailTable.Columns.Add("MTRL_BAG_QTY", typeof(string));
                InDetailTable.Columns.Add("HOPPER_ID", typeof(string));
                InDetailTable.Columns.Add("CHK_FLAG", typeof(string));


                //투입요청 하시겠습니까?
                Util.MessageConfirm("SFU1974", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataRow[] selectRows = Util.gridGetChecked(ref dgInputMaterial, "CHK");
                        foreach (DataRow drRow in selectRows)
                        {
                            InDetailTable.Rows.Clear();
                            InDetailTable.ImportRow(drRow);
                            InDetailTable.AcceptChanges();

                            DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", "INDATA,INDTLDATA", null, inDataSet);
                        }

                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                        this.txtRemark.Clear();
                        this.txtWorker.Clear();
                        this.txtWorker.Tag = string.Empty;

                        SearchInputMaterial();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion
    }
}
