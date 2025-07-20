/*************************************************************************************
 Created Date : 2018.03.02
      Creator : 정문교
   Decription : 대차 활성화 이동
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
    /// CMM_POLYMER_FORM_CART_FCS_MOVE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_FCS_MOVE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private DataRow _selectRow;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _fcsMoveProcID;

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

        public CMM_POLYMER_FORM_CART_FCS_MOVE()
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
                SetParameters();
                SetControl();
                SetCombo();
                SetDataGridColumnVisibility();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {

        }
        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = tmps[4] as string;
            _selectRow = tmps[5] as DataRow;
        }

        private void SetControl()
        {
            // 대차 정보 조회
            SetGridCartList();

            if (dgCart != null && dgCart.Rows.Count > 0)
            {
                SetGridAssyLotList();
            }

            // FCS 이동 공정 조회
            SetCommCode("FCS_MOVE_TO_PROCID");
        }
        private void SetCombo()
        {
            // FCS 이동 라인
            SetMoveLineCombo();

            // 인계자
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

        #region [FCS 이동]
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove())
                return;

            // 대차를 활성화로 이동처리하시겠습니까? \r\n(활성화로 이동하면 재공종료 됩니다.)
            Util.MessageConfirm("SFU4582", (result) =>
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

        /// <summary>
        /// FCS 이동 공정 조회
        /// </summary>
        private void SetCommCode(string cmcdType)
        {
            try
            {
                _fcsMoveProcID = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = cmcdType;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _fcsMoveProcID = Util.NVC(dtResult.Rows[0]["CBO_CODE"]);
                }
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
                const string bizRuleName = "DA_PRD_SEL_CART_MOVE_EQSG_PC";
                string[] arrColumn = { "LANGID", "SHOPID", "PROCID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, _fcsMoveProcID, LoginInfo.CFG_AREA_ID };
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
        /// 대차 FCS 이동
        /// </summary>
        private void CartMove()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));
                inTable.Columns.Add("FROM_EQSGID", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("TO_AREAID", typeof(string));
                inTable.Columns.Add("TO_ROUTID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("MOVE_MTHD_CODE", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _eqptID;
                newRow["FROM_PROCID"] = Util.NVC(_selectRow["PROCID"]);
                newRow["FROM_EQSGID"] = Util.NVC(_selectRow["EQSGID"]);
                newRow["TO_PROCID"] = _fcsMoveProcID;
                newRow["TO_EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["MOVE_MTHD_CODE"] = "COMMON";
                newRow["MOVE_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_SEND_CTNR_TO_FORM", "INDATA,INCTNR", null, (bizResult, bizException) =>
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
            //    // 대차 발행후 이동 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4406");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(_fcsMoveProcID))
            {
                // 공정 정보가 없습니다.
                Util.MessageValidation("SFU1456");
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