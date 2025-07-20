/*************************************************************************************
 Created Date : 2018.03.02
      Creator : 정문교
   Decription : 대차 인계 취소
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
    /// CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private string _moveordID = string.Empty;     // 이동번호

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

        public CMM_POLYMER_FORM_CART_TAKEOVER_CANCEL()
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
            _moveordID = tmps[4] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = tmps[5] as string;

            SetGridCartList();
          
            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
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
            if (dgCart != null && dgCart.Rows.Count > 0)
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

        #region [인계취소]
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel())
                return;

            // 인계취소 하시겠습니까?
            Util.MessageConfirm("SFU2932", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartTakeOverCancel();
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
        /// 대차 인계 취소
        /// </summary>
        private void CartTakeOverCancel()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("MOVE_ORD_ID", typeof(string));
                inTable.Columns.Add("RCPT_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["MOVE_ORD_ID"] = _moveordID;
                newRow["RCPT_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //ds.GetXml();

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_SEND_CTNR", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
 
        #endregion

        #region [Func]

        private bool ValidationCancel()
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