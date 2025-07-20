/*************************************************************************************
 Created Date : 2021.12.15
      Creator : 오화백
   Decription : FOL,STK Rework 재작업  BOX 출고
--------------------------------------------------------------------------------------
 [Change History]
 2024.07.26  이동주  E20240703-000930   FOL/STK 재작업 설비는 MAIN만 조회되도록 변경
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060_RWK_BOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_061_RWK_BOX : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private UserControl _UCParent = null;
        private Util _Util = null;
        private DataTable releasedTable = null;

        public ASSY004_061_RWK_BOX(UserControl uc)
        {
            _UCParent = uc;
            _Util = new Util();

            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        #endregion

        #region Initialize
        private void InitializeUserControls(bool txtClear = true)
        {
            Util.gridClear(dgWaitReleasedWip);
            Util.gridClear(dgReleasedWip);

            if (txtClear)
            { 
                tbEquipmentSegment.Text = string.Empty;
                txtCstID.Text = string.Empty;
            }
        }

        private void SetControl()
        {
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "cboAreaAll");
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;

            String[]  sFilter = { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "PROCESSEQUIPMENTSEGMENT");

            sFilter = new string[] { Process.RWK_LNS };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_MAIN_LEVEL"); // 2024.07.26 DJLEE FOL/STK 재작업 설비는 MAIN만 조회되도록 변경

            sFilter = new string[] { Process.PACKAGING };
            C1ComboBox[] cboReleasedLineParent = { cboArea };
            _combo.SetCombo(cboReleasedLine, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, cbParent: cboReleasedLineParent, sCase: "EQUIPMENTSEGMENT_PROC");
            cboReleasedLine.SelectedIndex = 0;
        }

        private void InitTable()
        {
            releasedTable = new DataTable();
            releasedTable.Columns.Add("CHK", typeof(bool));
            releasedTable.Columns.Add("ImageUrl", typeof(string));
            releasedTable.Columns.Add("WIPDTTM_IN", typeof(string));
            releasedTable.Columns.Add("LOTID", typeof(string));
            releasedTable.Columns.Add("CSTID", typeof(string));
            releasedTable.Columns.Add("WIPQTY", typeof(int));
            releasedTable.Columns.Add("PRODID", typeof(string));
            releasedTable.Columns.Add("PRJT_NAME", typeof(string));
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeUserControls();
            SetControl();
            InitCombo();
            InitTable();

            //Reload방지
            this.Loaded -= UserControl_Loaded;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedIndex > 0 && cboArea.Items.Count > cboArea.SelectedIndex)
            {
                if (Util.NVC((cboArea.Items[cboArea.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                {
                    GetReleasedLineCombo(cboReleasedLine);
                }
            }
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if ((sender as C1ComboBox).SelectedIndex == -1 || cboEquipment.SelectedValue.ToString() == "SELECT" || cboEquipment.SelectedIndex == 0)
            {
                InitializeUserControls();
            }
            else
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            SearchWip();
        }

        private void btnDelRow_Click(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as Button).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            int idx = _Util.GetDataGridRowIndex(dgWaitReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);

            DataTable tmpTable = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);
            tmpTable.Rows[idx]["CHK"] = false;
            Util.GridSetData(dgWaitReleasedWip, tmpTable, FrameOperation);

            // 출고 대상 삭제
            Button dg = sender as Button;
            DataRow drRow = (dg.DataContext as DataRowView).Row;

            ReleasedWipRowDelete(drRow["LOTID"].ToString());
        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRelease()) return;

            ReleasedWips();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as CheckBox).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            DataRow newRow = releasedTable.NewRow();
            newRow["CHK"] = DataTableConverter.GetValue(checkedDrv, "CHK");
            newRow["ImageUrl"] = @".\Images\icon_close.png";
            newRow["WIPDTTM_IN"] = DataTableConverter.GetValue(checkedDrv, "WIPDTTM_IN");
            newRow["LOTID"] = DataTableConverter.GetValue(checkedDrv, "LOTID");
            newRow["CSTID"] = DataTableConverter.GetValue(checkedDrv, "CSTID");
            newRow["WIPQTY"] = DataTableConverter.GetValue(checkedDrv, "WIPQTY");
            newRow["PRODID"] = DataTableConverter.GetValue(checkedDrv, "PRODID");
            newRow["PRJT_NAME"] = DataTableConverter.GetValue(checkedDrv, "PRJT_NAME");
            releasedTable.Rows.Add(newRow);
            RefreshDgReleasedWip();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DataRowView checkedDrv = ((sender as CheckBox).Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;
            //int idx = _Util.GetDataGridRowIndex(dgReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);
            //releasedTable.Rows.RemoveAt(idx);
            //RefreshDgReleasedWip();

            ReleasedWipRowDelete(DataTableConverter.GetValue(checkedDrv, "LOTID").ToString());
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedCheckBox = sender as CheckBox;
            DataRowView checkedDrv = (clickedCheckBox.Parent as DataGridCellPresenter).Cell.Row.DataItem as DataRowView;

            int row = (clickedCheckBox.Parent as DataGridCellPresenter).Cell.Row.Index;

            if (clickedCheckBox.IsChecked.HasValue)
            {
                //int idx = _Util.GetDataGridRowIndex(dgReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);

                if (clickedCheckBox.IsChecked.Value)
                {
                    //DataRow newRow = releasedTable.NewRow();
                    //newRow["CHK"] = DataTableConverter.GetValue(checkedDrv, "CHK");
                    //newRow["ImageUrl"] = @".\Images\icon_close.png";
                    //newRow["WIPDTTM_IN"] = DataTableConverter.GetValue(checkedDrv, "WIPDTTM_IN");
                    //newRow["LOTID"] = DataTableConverter.GetValue(checkedDrv, "LOTID");
                    //newRow["CSTID"] = DataTableConverter.GetValue(checkedDrv, "CSTID");
                    //newRow["WIPQTY"] = DataTableConverter.GetValue(checkedDrv, "WIPQTY");
                    //newRow["PRODID"] = DataTableConverter.GetValue(checkedDrv, "PRODID");
                    //newRow["PRJT_NAME"] = DataTableConverter.GetValue(checkedDrv, "PRJT_NAME");
                    //releasedTable.Rows.Add(newRow); 

                    // 출고 대상에 Row Add
                    ReleasedWipRowAdd(row);
                    //}
                }
                else
                {
                    //int idx = _Util.GetDataGridRowIndex(dgReleasedWip, "LOTID", DataTableConverter.GetValue(checkedDrv, "LOTID") as string);
                    //releasedTable.Rows.RemoveAt(idx);

                    ReleasedWipRowDelete(DataTableConverter.GetValue(checkedDrv, "LOTID").ToString());

                    //Project 전체 체크 해제
                    DataRow[] drChk = Util.gridGetChecked(ref dgWaitReleasedWip, "CHK", true);      //Check 기능 오류 수정, 2024-11-15, 김선영
                    //DataRow[] drChk = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource).Select("CHK = 1");
                    
                    if (drChk.Length == 0)
                    {
                        chkAllcheck.Unchecked -= chkAllcheck_Unchecked;
                        chkAllcheck.IsChecked = false;
                        chkAllcheck.Unchecked += chkAllcheck_Unchecked;
                    }
                }
                //RefreshDgReleasedWip();
            }
        }

        private void txtCstID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string cstID = txtCstID.Text;
                int idx = _Util.GetDataGridRowIndex(dgWaitReleasedWip, "CSTID", cstID);

                if (idx == -1)
                {
                    idx = _Util.GetDataGridRowIndex(dgWaitReleasedWip, "LOTID", cstID);
                }
                     
                if (idx == -1)
                {
                    //해당하는 LOT정보가 없습니다.
                    Util.MessageValidation("SFU2025", (result) =>
                    {
                        if(result == MessageBoxResult.OK)
                        {
                            txtCstID.Clear();
                            txtCstID.Focusable = true;
                            txtCstID.Focus();
                        }
                    });
                    return;
                }

                // 이미 체크된 Lot이면 Skip
                if (Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[idx].DataItem, "CHK")) != "True")       // 이미 체크된 Lot이면 Skip되도록 비교구문 수정, 2024-11-14, 김선영
                {
                    DataTable tmpTable = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);
                    tmpTable.Rows[idx]["CHK"] = true;
                    Util.GridSetData(dgWaitReleasedWip, tmpTable, FrameOperation);

                    // 출고 대상에 Row Addㄴ
                    ReleasedWipRowAdd(idx);
                }

                txtCstID.Clear();
                txtCstID.Focusable = true;
                txtCstID.Focus();
            }
        }
        #endregion

        #region Method

        #region [BizCall]

        /// <summary>
        /// 출고라인 콤보
        /// </summary>
        private void GetReleasedLineCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "PROD_GROUP" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), Process.PACKAGING, null };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SearchWip()
        {
            try
            {
                cboReleasedLine.SelectedIndex = 0;

                InitializeUserControls();

                DataTable inData = new DataTable();
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("WIPSTAT", typeof(string));
                inData.Columns.Add("WIP_TYPE_CODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inData.NewRow();
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue as string;
                newRow["WIPSTAT"] = "END";
                newRow["WIP_TYPE_CODE"] = "OUT";
                //newRow["EQPTID"] = cboEquipment.SelectedValue.ToString().Trim().Equals("-ALL-") ? null : cboEquipment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedIndex == 0 ? null : cboEquipment.SelectedValue.ToString();
                inData.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_BY_EQSGID_PROCID_FOR_RWK_ST", "INDATA", "OUTDATA", inData, (searchResult, searchException) =>
                {
                    HideLoadingIndicator();

                    try
                    {
                        Util.GridSetData(dgWaitReleasedWip, searchResult, FrameOperation);

                        txtCstID.Text = string.Empty;
                        txtCstID.Focusable = true;
                        txtCstID.Focus();

                        if (cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                            tbEquipmentSegment.Text = string.Empty;
                        else
                            tbEquipmentSegment.Text = cboEquipmentSegment.SelectedValue.ToString();

                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
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

        private void ReleasedWips()
        {
            try
            {
                ///////////////////////////////////////////////////////////////////// DataSet
                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("LOTID", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));
                inInput.Columns.Add("ACTQTY2", typeof(int));
                inInput.Columns.Add("ACTUQTY", typeof(int));
                inInput.Columns.Add("WIPNOTE", typeof(string));

                ///////////////////////////////////////////////////////////////////// 바인딩
                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQSGID"] = cboReleasedLine.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Process.PACKAGING;              // 출고공정
                inData.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgReleasedWip.ItemsSource);

                foreach (DataRow dr in dt.Rows)
                {
                    newRow = inInput.NewRow();
                    newRow["LOTID"] = dr["LOTID"].ToString();
                    newRow["ACTQTY"] = Util.NVC_Int(dr["WIPQTY"].ToString());
                    newRow["ACTQTY2"] = Util.NVC_Int(dr["WIPQTY"].ToString());
                    newRow["ACTUQTY"] = 0;
                    inInput.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DISPATCH_OUT_LOT_RWK_FOL_STK_L", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        SearchWip();
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]

        private bool ValidationSearch()
        {
            if ((cboEquipmentSegment.SelectedValue as string).Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if ((cboEquipment.SelectedValue as string).Trim().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationRelease()
        {
            if ((cboEquipmentSegment.SelectedValue as string).Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if ((cboReleasedLine.SelectedValue as string).Trim().Equals("SELECT"))
            {
                // 대상라인을 선택해 주세요.
                Util.MessageValidation("SFU3704");
                return false;
            }

            if (dgReleasedWip.Rows.Count == 0)
            {
                // 출고처리할 LOT 정보가 존재하지 않습니다.
                Util.MessageValidation("SFU2967");
                return false;
            }

            return true;
        }
        #endregion

        #region [Function]

        private void ReleasedWipRowAdd(int row)
        {
            DataTable dt = DataTableConverter.Convert(dgReleasedWip.ItemsSource);

            if (dt.Columns.Count == 0)
            {
                dt = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in dgReleasedWip.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["WIPDTTM_IN"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "WIPDTTM_IN"));
            newrow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "LOTID"));
            newrow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "CSTID"));
            newrow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "WIPQTY"));
            newrow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "PRODID"));
            newrow["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgWaitReleasedWip.Rows[row].DataItem, "PRJT_NAME"));
            dt.Rows.Add(newrow);

            Util.GridSetData(dgReleasedWip, dt, null, true);
        }

        private void ReleasedWipRowDelete(string LotID)
        {
            DataTable dt = DataTableConverter.Convert(dgReleasedWip.ItemsSource);

            dt.Select("LOTID = '" + LotID + "'").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();
            Util.GridSetData(dgReleasedWip, dt, null);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRelease);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Collapsed)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void RefreshDgReleasedWip()
        {
            Util.gridClear(dgReleasedWip);
            Util.GridSetData(dgReleasedWip, releasedTable, FrameOperation);
        }



        #endregion

        #endregion

        private void chkAllcheck_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtProject.Text.ToString()))
            {
                DataTable dtTo = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);

                if (dtTo.Rows.Count > 1)
                {
                    for (int i = 0; i < dtTo.Rows.Count; i++)
                    {
                        if (dtTo.Rows[i]["PRJT_NAME"].ToString() == this.txtProject.Text.ToString())
                        {
                            if (dtTo.Rows[i]["CHK"].ToString() != "True")      //Check 기능 오류 수정, 2024-11-15, 김선영
                                //if (dtTo.Rows[i]["CHK"].ToString() != "1")

                                {
                                DataTableConverter.SetValue(dgWaitReleasedWip.Rows[i].DataItem, "CHK", true);

                                //DataRow newRow = releasedTable.NewRow();
                                //newRow["CHK"] = dtTo.Rows[i]["CHK"];
                                //newRow["ImageUrl"] = @".\Images\icon_close.png";
                                //newRow["WIPDTTM_IN"] = dtTo.Rows[i]["WIPDTTM_IN"];
                                //newRow["LOTID"] = dtTo.Rows[i]["LOTID"];
                                //newRow["CSTID"] = dtTo.Rows[i]["CSTID"];
                                //newRow["WIPQTY"] = dtTo.Rows[i]["WIPQTY"];
                                //newRow["PRODID"] = dtTo.Rows[i]["PRODID"];
                                //newRow["PRJT_NAME"] = dtTo.Rows[i]["PRJT_NAME"];
                                //releasedTable.Rows.Add(newRow);

                                ReleasedWipRowAdd(i);
                            }
                        }
                    }

                    //RefreshDgReleasedWip();
                }
            }
        }

        private void chkAllcheck_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dtTo = DataTableConverter.Convert(dgWaitReleasedWip.ItemsSource);

            if (dtTo.Rows.Count > 1)
            {
                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["PRJT_NAME"].ToString() == this.txtProject.Text.ToString())
                    {
                        if (dtTo.Rows[i]["CHK"].ToString() == "True")       //Check 기능 오류 수정, 2024-11-15, 김선영
                        {
                            int idx = _Util.GetDataGridRowIndex(dgReleasedWip, "LOTID", dtTo.Rows[i]["LOTID"].ToString());
                            //releasedTable.Rows.RemoveAt(idx);

                            DataTableConverter.SetValue(dgWaitReleasedWip.Rows[i].DataItem, "CHK", "False");      //Check 기능 오류 수정, 2024-11-15, 김선영
                            //DataTableConverter.SetValue(dgWaitReleasedWip.Rows[i].DataItem, "CHK", 0);      

                            ReleasedWipRowDelete(dtTo.Rows[i]["LOTID"].ToString());
                        }
                    }

                    //RefreshDgReleasedWip();
                }
            }
        }

    }
}
