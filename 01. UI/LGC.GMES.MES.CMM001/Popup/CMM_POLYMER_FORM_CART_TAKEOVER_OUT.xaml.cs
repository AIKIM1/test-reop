/*************************************************************************************
 Created Date : 2018.03.02
      Creator : 정문교
   Decription : 대차 인계
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_TAKEOVER_OUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_TAKEOVER_OUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private DataRow _selectRow;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_TAKEOVER_OUT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetCombo();
                SetDataGridColumnVisibility();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {

        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = tmps[4] as string;
            _selectRow = tmps[5] as DataRow;

            cboArea.IsEnabled = false;
            //cboTakeOverUser.IsEnabled = false;

            SetGridCartList();
          
            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
            }

        }
        private void SetCombo()
        {
            SetAreaCombo();
            SetMoveProcessCombo();
            SetMoveLineCombo();

            if (string.IsNullOrWhiteSpace(_eqptID))
            {
                // 설비가 없는경우 포장 작업자
                SetBoxingWorkerCombo();
            }
            else
            {
                SetInspectorCombo();
            }
        }

        private void SetDataGridColumnVisibility()
        {
            if (dgCart.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")).Equals("G"))
                {
                    // 양품
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    // 불량
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region [공장 변경]
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveProcessCombo();
            SetMoveLineCombo();

        }
        #endregion

        #region [공정 변경]
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveLineCombo();

        }
        #endregion

        #region [공장이동 체크]
        private void chkMoveArea_Checked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = true;
            //cboTakeOverUser.IsEnabled = true;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        private void chkMoveArea_Unchecked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = false;
            //cboTakeOverUser.IsEnabled = false;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }
        #endregion


        #region [이동]
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove())
                return;

            // 이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartMove();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void SetAreaCombo()
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cboArea.SelectedValuePath;
            string displayMemberText = cboArea.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboArea, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
        }

        private void SetMoveProcessCombo()
        {
            try
            {
                if (dgCart.Rows.Count == 0)
                    return;

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ROUTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue ?? null;
                newRow["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "ROUTID"));
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_MOVE_PROC_TAKEOVER_PC", "RQSTDT", "RSLTDT", inTable);

                //DataRow drSelect = dtResult.NewRow();
                //drSelect[cboProcess.SelectedValuePath] = "SELECT";
                //drSelect[cboProcess.DisplayMemberPath] = "- SELECT -";
                //dtResult.Rows.InsertAt(drSelect, 0);

                //cboProcess.ItemsSource = null;
                //cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                //cboProcess.SelectedIndex = 0;

                //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;

                cboProcess.ItemsSource = null;
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                int index = 0;

                if (cboProcess.Items.Count > 1 || cboProcess.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboProcess.SelectedValuePath] = "SELECT";
                    dr[cboProcess.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboProcess.ItemsSource = null;
                    cboProcess.ItemsSource = dt.Copy().AsDataView();
                }

                cboProcess.SelectedIndex = index;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMoveLineCombo()
        {
            try
            {
                string EqsgCode = string.Empty;

                if ((bool)chkMoveArea.IsChecked)
                {
                    EqsgCode = cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString();
                }
                else
                {
                    EqsgCode = LoginInfo.CFG_AREA_ID;
                }

                const string bizRuleName = "DA_PRD_SEL_CART_MOVE_EQSG_PC";
                string[] arrColumn = { "LANGID", "SHOPID", "PROCID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, Util.NVC(cboProcess.SelectedValue), EqsgCode };
                string selectedValueText = cboLine.SelectedValuePath;
                string displayMemberText = cboLine.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboLine, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, null);

                if (cboLine.Items.Count > 1 || cboLine.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboLine.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboLine.SelectedValuePath] = "SELECT";
                    dr[cboLine.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboLine.ItemsSource = null;
                    cboLine.ItemsSource = dt.Copy().AsDataView();

                    int index = 0;
                    cboLine.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetInspectorCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, _eqptID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetBoxingWorkerCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_BOXING_WORKER_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLotList()
        {
            try
            {
                string bizRuleName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "CTNR_ID"));
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT_CART_LOAD", "INDATA", "OUTDATA", inTable);

                dtResult.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                Util.GridSetData(dgAssyLot, dtResult, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 대차 이동
        /// </summary>
        private void CartMove()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_ROUTID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("FROM_EQSGID", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["TO_PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["TO_ROUTID"] = dt.Rows[cboProcess.SelectedIndex]["ROUTID_TO"].ToString();
                newRow["TO_FLOWID"] = dt.Rows[cboProcess.SelectedIndex]["FLOWID_TO"].ToString();
                newRow["TO_EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["MOVE_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["FROM_EQSGID"] = Util.NVC(_selectRow["EQSGID"]);
                newRow["FROM_PROCID"] = Util.NVC(_selectRow["PROCID"]);
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        private bool ChkCartStatus()
        {
            bool IsInspProc = false;

            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CMCDTYPE"] = "FORM_INSP_PROCID";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow[] dr = dtResult.Select("CBO_CODE = '" + _procID + "'");

                    if (dr.Length > 0)
                    {
                        IsInspProc = true;
                    }
                }

                return IsInspProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsInspProc;
            }
        }
        #endregion

        #region [Func]

        private bool ValidationMove()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            //if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
            //{
            //     대차 발행후 이동 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4406");
            //    return false;
            //}

            //if (ChkCartStatus())
            //{
            //    if (Util.NVC_Int(dr[0]["PROC_COUNT"]) != 0)
            //    {
            //        // 생산중인 대차는 이동할 수 없습니다.
            //        Util.MessageValidation("SFU4376");
            //        return false;
            //    }
            //}
            //else
            //{
            //    if (Util.NVC_Int(dr[0]["NO_PRINT_COUNT"]) != 0)
            //    {
            //        // Inbox 태그를 전부 발행해야 이동 처리가 가능 합니다.
            //        Util.MessageValidation("SFU4368");
            //        return false;
            //    }
            //}

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 공정을 선택하세요.
                Util.MessageValidation("SFU4362");
                return false;
            }

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 라인을 선택하세요.
                Util.MessageValidation("SFU4363");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 공정을 선택하세요.
                Util.MessageValidation("SFU4362");
                return false;
            }

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 라인을 선택하세요.
                Util.MessageValidation("SFU4363");
                return false;
            }

            if (cboTakeOverUser.SelectedValue == null || cboTakeOverUser.SelectedValue.ToString().Equals("SELECT"))
            {
                // 인계자를 선택하세요.
                Util.MessageValidation("SFU4290");
                return false;
            }

            return true;
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






    }
}