/*************************************************************************************
 Created Date : 2018.03.02
      Creator : 정문교
   Decription : 대차 인수
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
    /// CMM_POLYMER_FORM_CART_TAKEOVER_IN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_TAKEOVER_IN : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private DataRow[] _cartList;
        private DataTable _inboxList;

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

        public CMM_POLYMER_FORM_CART_TAKEOVER_IN()
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
            _cartList = tmps[4] as DataRow[];

            txtProcess.Text = tmps[1] as string;

            // 대차 정보 조회
            SetGridCartList();
          
            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgCart.Rows[0].DataItem, "CHK", true);

                SetGridAssyLotList(0);
                SetGridProductInboxList(0);
            }

        }
        private void SetCombo()
        {
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
                    tbInboxTitle.Text = ObjectDic.Instance.GetObjectName("INBOX");
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("InBox ID");

                    // 양품
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Visible;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Collapsed;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Visible;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbInboxTitle.Text = ObjectDic.Instance.GetObjectName("불량그룹LOT");
                    dgProductionInbox.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("불량그룹LOT");

                    // 불량
                    dgAssyLot.Columns["INBOX_QTY"].Visibility = Visibility.Collapsed;
                    dgAssyLot.Columns["DEFECT_GROUP_LOTQTY"].Visibility = Visibility.Visible;

                    dgProductionInbox.Columns["VISL_INSP_USERNAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["INBOX_TYPE_NAME"].Visibility = Visibility.Collapsed;
                    dgProductionInbox.Columns["RESNGRNAME"].Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region 대차 Check
        private void dgCartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        //row 색 바꾸기
                        dgCart.SelectedIndex = idx;

                        SetGridAssyLotList(idx);
                        SetGridProductInboxList(idx);

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 조립Lot Check
        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                }
                else
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                }

                dt.AcceptChanges();
                Util.GridSetData(dgAssyLot, dt, null, true);

                // 완성 Inbox 조회
                GetGridProductInboxList(dt);
            }

        }
        #endregion

        #region [대차인수]
        private void btnTakeOver_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTakeOver())
                return;

            // 인수 하시겠습니까?
            Util.MessageConfirm("SFU4273", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartTakeOver();
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
                string CartList = string.Empty;
                foreach(DataRow dr in _cartList)
                {
                    CartList += dr["CTNR_ID"] + ",";
                }

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
                newRow["CTNR_ID"] = CartList.Substring(0, CartList.Length - 1);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_MULTI_PC", "INDATA", "OUTDATA", inTable);
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
        private void SetGridAssyLotList(int row)
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
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[row].DataItem, "CTNR_ID"));
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
        /// 조립LOT 완성 Inbox List
        /// </summary>
        private void SetGridProductInboxList(int row)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _eqptID;
                newRow["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[row].DataItem, "CTNR_ID"));
                inTable.Rows.Add(newRow);

                _inboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_CART_LOAD", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionInbox, _inboxList, FrameOperation, true);

                if (_inboxList != null && _inboxList.Rows.Count > 0)
                    dgProductionInbox.CurrentCell = dgProductionInbox.GetCell(0, 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 인수
        /// </summary>
        private void CartTakeOver()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("RCPT_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("MOVE_ORD_ID", typeof(string));
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["RCPT_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
                foreach (DataRow row in dt.Rows)
                {
                    newRow = inCNTR.NewRow();
                    newRow["MOVE_ORD_ID"] = row["MOVE_ORD_ID"];
                    newRow["CTNR_ID"] = row["CTNR_ID"];
                    inCNTR.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
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

        #endregion

        #region [Func]

        private void GetGridProductInboxList(DataTable dt)
        {
            DataTable dtinbox = _inboxList.Copy();
            DataRow[] drDel = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = 0");

            foreach (DataRow rowdel in drDel)
            {
                dtinbox.Select("ASSY_LOTID = '" + Util.NVC(rowdel["ASSY_LOTID"]) + "'").ToList<DataRow>().ForEach(row => row.Delete());
            }
            dtinbox.AcceptChanges();

            Util.GridSetData(dgProductionInbox, dtinbox, FrameOperation, true);
        }

        private bool ValidationTakeOver()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);

            if (dt.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            foreach (DataRow row in dt.Rows)
            {
                if (Util.NVC_Int(row["CELL_QTY"]) == 0)
                {
                    // 대차에 Inbox 정보가 없습니다.
                    Util.MessageValidation("SFU4375");
                    return false;
                }
            }

            if (cboTakeOverUser.SelectedValue == null || cboTakeOverUser.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업자 정보를 입력하세요.
                Util.MessageValidation("SFU4201");
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