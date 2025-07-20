/*************************************************************************************
 Created Date : 2018.04.13
      Creator : 정문교
   Decription : 불량LOT LOSS 처리
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_DEFECT_LOSS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_DEFECT_LOSS : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _AreaID = string.Empty;
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _cartId = string.Empty;        // 대차ID
        private DataTable _defectLot;

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

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

        public CMM_POLYMER_FORM_CART_DEFECT_LOSS()
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
            _eqptID = tmps[2] as string;
            _cartId = tmps[3] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = tmps[3] as string;

            DataRow defectLot = tmps[4] as DataRow;

            _defectLot = defectLot.Table.Clone();
            _defectLot.ImportRow(defectLot);
        }

        private void SetControl()
        {
            // Grid
            Util.GridSetData(dgDefectGroup, _defectLot, null, true);

            // Area ID 조회
            GetAreaID();

            // 전기일
            dtpDate.SelectedDateTime = GetComSelCalDate();
        }
        private void SetCombo()
        {
            SetLossCode();
        }

        #endregion

        #region [작업자]
        private void txtWorkUser_GotFocus(object sender, RoutedEventArgs e)
        {
            dgWorkUserSelect.Visibility = Visibility.Collapsed;
        }

        private void txtWorkUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                SelectWorkUser();
            }
        }

        private void dgWorkUser_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtWorkUserID.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtWorkUserNM.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());

            dgWorkUserSelect.Visibility = Visibility.Collapsed;
            txtResnNote.Visibility = Visibility.Visible;

        }
        #endregion

        #region [LOSS처리]
        private void btnLoss_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLoss())
                return;

            // Loss처리 하시겠습니까?
            Util.MessageConfirm("SFU4918", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LossProcess();
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

        private void SetLossCode()
        {
            const string bizRuleName = "DA_PRD_SEL_ACTIVITIREASON_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID, "LOSS_LOT" };
            string selectedValueText = cboLossCode.SelectedValuePath;
            string displayMemberText = cboLossCode.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboLossCode, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        /// <summary>
        /// 작업자 조회
        /// </summary>
        private void SelectWorkUser()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERNAME", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERNAME"] = txtWorkUser.Text;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", inTable);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    // 사용자 정보가 없습니다.
                    Util.MessageValidation("SFU1592");
                }
                else if (dtResult.Rows.Count == 1)
                {
                    txtWorkUserID.Text = dtResult.Rows[0]["USERID"].ToString();
                    txtWorkUserNM.Text = dtResult.Rows[0]["USERNAME"].ToString();
                }
                else
                {
                    dgWorkUserSelect.Visibility = Visibility.Visible;
                    txtResnNote.Visibility = Visibility.Collapsed;

                    Util.GridSetData(dgWorkUserSelect, dtResult, null);
                    this.Focusable = true;
                    this.Focus();
                    this.Focusable = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAreaID()
        {
            try
            {
                if (_defectLot == null || _defectLot.Rows.Count == 0)
                {
                    _AreaID = string.Empty;
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _defectLot.Rows[0]["LOTID"].ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_AREAID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _AreaID = Util.NVC(dtResult.Rows[0]["AREAID"]);
                else
                    _AreaID = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = _AreaID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Loss 처리
        /// </summary>
        private void LossProcess()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("POSTDATE", typeof(string));

                DataTable inRESN = inDataSet.Tables.Add("INRESN");
                inRESN.Columns.Add("LOTID", typeof(string));
                inRESN.Columns.Add("WIPSEQ", typeof(string));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(string));
                inRESN.Columns.Add("RESNNOTE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CTNR_ID"] = _cartId;
                newRow["ACT_USERID"] = txtWorkUserID.Text;
                newRow["POSTDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                inTable.Rows.Add(newRow);

                newRow = inRESN.NewRow();
                newRow["LOTID"] = Util.NVC(_defectLot.Rows[0]["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_defectLot.Rows[0]["WIPSEQ"]);
                newRow["ACTID"] = "LOSS_LOT";
                newRow["RESNCODE"] = Util.NVC(cboLossCode.SelectedValue);
                newRow["RESNQTY"] = txtLossQty.Value;
                newRow["RESNNOTE"] = txtResnNote.Text;
                inRESN.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOSS_LOT_FOR_DFCT_CTNR", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

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
        private bool ValidationLoss()
        {
            if (cboLossCode.SelectedValue == null || cboLossCode.SelectedValue.ToString().Equals("SELECT"))
            {
                // 사유를 선택하세요.
                Util.MessageValidation("SFU1593");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWorkUserID.Text))
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4595");
                return false;
            }

            if (txtLossQty.Value == 0)
            {
                // Loss량 이 입력되지 않았습니다.
                Util.MessageValidation("SFU1357");
                return false;
            }

            double cellQty = 0;
            if (!double.TryParse(Util.NVC(_defectLot.Rows[0]["CELL_QTY"]), out cellQty))
            {
                cellQty = 0;
            }

            if (cellQty < txtLossQty.Value)
            {
                // 수량이 이전 수량보다 많이 입력 되었습니다.
                Util.MessageValidation("SFU3107");
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

        private void btnCartCellRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string ctnrID = txtCartID.Text.ToString();

                if (string.IsNullOrEmpty(ctnrID))
                {
                    DataTable dtSource = DataTableConverter.Convert(dgDefectGroup.ItemsSource);

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = Util.NVC(dtSource.Rows[0]["LOTID"]);
               
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CTNR_BY_LOTID", "INDATA", "OUTDATA", inTable);

                    if(dtResult == null || dtResult.Rows.Count < 1)
                    {
                        Util.Alert("SFU4365");
                        return;
                    }

                    ctnrID = Util.NVC(dtResult.Rows[0]["CTNR_ID"]);
                }
                //int loss_qty = Int32.Parse(Util.NVC(dr[0]["WIPQTY"]));

                CMM_ASSY_LOSS_CELL_INPUT popupoutput = new CMM_ASSY_LOSS_CELL_INPUT();

                //popupoutput.isInputQty = true;

                popupoutput.FrameOperation = this.FrameOperation;
                object[] parameters = new object[1];
                parameters[0] = ctnrID;
                //parameters[1] = loss_qty;
                C1WindowExtension.SetParameters(popupoutput, parameters);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupoutput);
                        popupoutput.BringToFront();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}