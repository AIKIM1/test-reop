/*************************************************************************************
 Created Date : 2018.03.26
      Creator : 정문교
   Decription : Inbox 정보 변경
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
    /// CMM_POLYMER_FORM_CART_INPUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_INBOX_INFO_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;          // 공정코드
        private string _procName = string.Empty;        // 공정명
        private string _eqptID = string.Empty;          // 설비코드

        private DataRow _inboxList;
        private string _assyLotID = string.Empty;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        public bool ProcessCall { get; set; }
          //활성화 대차 생성/삭제/복원/INBOX 변경 화면에서 호출
        public string Ctnr_Create { get; set; }

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

        public CMM_POLYMER_FORM_INBOX_INFO_CHANGE()
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
                //활성화 대차 생성/삭제/복원/INBOX 변경 화면에서 호출
                if (Ctnr_Create == "Y")
                {
                    SetControl_Ctnr();
                    cboTakeOverUser.Visibility = Visibility.Collapsed;
                    txtReqUserCreate.Visibility = Visibility.Visible;
                    btnReqUserCreate.Visibility = Visibility.Visible;
                }
                else
                {
                    SetControl();
                    cboTakeOverUser.Visibility = Visibility.Visible;
                    txtReqUserCreate.Visibility = Visibility.Collapsed;
                    btnReqUserCreate.Visibility = Visibility.Collapsed;
                }
                
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
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            _inboxList = tmps[3] as DataRow;
        }

        private void SetControl()
        {
            txtInboxID.Text = Util.NVC(_inboxList["LOTID"]);

            txtBeforeAssyLot.Text = Util.NVC(_inboxList["ASSY_LOTID"]);
            txtBeforeGrade.Text = Util.NVC(_inboxList["CAPA_GRD_CODE"]);
            txtBeforeQty.Value = double.Parse(Util.NVC(_inboxList["CELL_QTY"]));

            if (txtBeforeAssyLot.Text.Length >= 8)
            {
                txtAssyLotID1.Text = txtBeforeAssyLot.Text.Substring(0, 1);
                txtAssyLotID2.Text = txtBeforeAssyLot.Text.Substring(1, 1);
                txtAssyLotID3.Text = txtBeforeAssyLot.Text.Substring(2, 1);
                txtAssyLotID45.Text = txtBeforeAssyLot.Text.Substring(3, 2);
                txtAssyLotID67.Text = txtBeforeAssyLot.Text.Substring(5, 2);
                txtAssyLotID8.Text = txtBeforeAssyLot.Text.Substring(7, 1);
            }

            //cboCapaGrade.SelectedValue = Util.NVC(_inboxList["CAPA_GRD_CODE"]);
            txtNextQty.Value = double.Parse(Util.NVC(_inboxList["CELL_QTY"]));

            // 버튼 활성, 비활성
            SetControlVisibility();
            SetControlEnabled("AssyLot");

            rdoAssyLot.Checked += rdoAssyLot_Checked;
            rdoGrade.Checked += rdoGrade_Checked;
            rdoQty.Checked += rdoQty_Checked;

            txtAssyLotID45.Focus();
        }

        //활성화 대차 생성/삭제/복원/INBOX 변경 화면에서 호출
        private void SetControl_Ctnr()
        {
            txtInboxID.Text = Util.NVC(_inboxList["INBOX_ID"]);

            txtBeforeAssyLot.Text = Util.NVC(_inboxList["LOTID_RT"]);
            txtBeforeGrade.Text = Util.NVC(_inboxList["CAPA_GRD_CODE"]);
            txtBeforeQty.Value = double.Parse(Util.NVC(_inboxList["WIPQTY"]));

            if (txtBeforeAssyLot.Text.Length >= 8)
            {
                txtAssyLotID1.Text = txtBeforeAssyLot.Text.Substring(0, 1);
                txtAssyLotID2.Text = txtBeforeAssyLot.Text.Substring(1, 1);
                txtAssyLotID3.Text = txtBeforeAssyLot.Text.Substring(2, 1);
                txtAssyLotID45.Text = txtBeforeAssyLot.Text.Substring(3, 2);
                txtAssyLotID67.Text = txtBeforeAssyLot.Text.Substring(5, 2);
                txtAssyLotID8.Text = txtBeforeAssyLot.Text.Substring(7, 1);
            }

            //cboCapaGrade.SelectedValue = Util.NVC(_inboxList["CAPA_GRD_CODE"]);
            txtNextQty.Value = double.Parse(Util.NVC(_inboxList["WIPQTY"]));

            // 버튼 활성, 비활성
            SetControlVisibility();
            SetControlEnabled("AssyLot");

            rdoAssyLot.Checked += rdoAssyLot_Checked;
            rdoGrade.Checked += rdoGrade_Checked;
            rdoQty.Checked += rdoQty_Checked;

            txtAssyLotID45.Focus();
        }

        private void SetControlVisibility()
        {
            if (ProcessCall)
            {
                // 공정에서 사용시 수량 안보이게
                rdoQty.Visibility = Visibility.Collapsed;
                spQty1.Visibility = Visibility.Collapsed;
                spQty2.Visibility = Visibility.Collapsed;
                spQty3.Visibility = Visibility.Collapsed;
            }
        }

        private void SetControlEnabled(string RadioButtonValue)
        {
            if (RadioButtonValue.Equals("AssyLot"))
            {
                txtAssyLotID45.IsEnabled = true;
                txtAssyLotID67.IsEnabled = true;
                cboCapaGrade.IsEnabled = false;
                txtNextQty.IsEnabled = false;
            }
            else if (RadioButtonValue.Equals("Grade"))
            {
                txtAssyLotID45.IsEnabled = false;
                txtAssyLotID67.IsEnabled = false;
                cboCapaGrade.IsEnabled = true;
                txtNextQty.IsEnabled = false;
            }
            else
            {
                txtAssyLotID45.IsEnabled = false;
                txtAssyLotID67.IsEnabled = false;
                cboCapaGrade.IsEnabled = false;
                txtNextQty.IsEnabled = true;
            }

            // 불량인 경우 일자 변경 불가
            if (Util.NVC(_inboxList["QLTY_TYPE_CODE"]).Equals("N"))
            {
                txtAssyLotID67.IsEnabled = false;
            }

        }

        private void SetCombo()
        {
            // 변경사유
            CommonCombo _combo = new CommonCombo();

            //String[] sFilterReason = { "MODIFY_WIPQTY" };
            //_combo.SetCombo(cboChangeReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilterReason, sCase: "ACTIVITIREASON");

            // 용량구분
            string[] sFilterCapa = { LoginInfo.CFG_AREA_ID, Util.NVC(_inboxList["EQSGID"]), "G" };
            _combo.SetCombo(cboCapaGrade, CommonCombo.ComboStatus.SELECT, sCase: "FORM_GRADE_TYPE_CODE_LINE", sFilter: sFilterCapa);

            cboCapaGrade.SelectedValue = Util.NVC(_inboxList["CAPA_GRD_CODE"]);

            // 작업자
            if (string.IsNullOrWhiteSpace(_eqptID))
            {
                // 설비가 없는경우 포장 작업자
                SetBoxingWorkerCombo();
            }
            {
                SetInspectorCombo();
            }
        }

        #endregion

        #region AssyLot, Grade, Qty RadioButton Click
        private void rdoAssyLot_Checked(object sender, RoutedEventArgs e)
        {
            SetControlEnabled("AssyLot");
            txtAssyLotID45.Focus();
        }
        private void rdoGrade_Checked(object sender, RoutedEventArgs e)
        {
            SetControlEnabled("Grade");
        }
        private void rdoQty_Checked(object sender, RoutedEventArgs e)
        {
            SetControlEnabled("Qty");
        }
        #endregion


        #region 조립LOT 일,주별 입력
        private void txtAssyLotID45_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAssyLotID45.SelectAll();
        }

        private void txtAssyLotID67_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAssyLotID67.SelectAll();
        }

        private void txtAssyLotID45_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Tab || e.Key == Key.Enter)
            {
                txtAssyLotID67.Focus();
            }
        }

        private void txtAssyLotID67_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                 (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                  e.Key == Key.Back ||
                  e.Key == Key.Tab ||
                  e.Key == Key.Enter ||
                 (e.Key >= Key.Left && e.Key <= Key.Down)))
            {
                e.Handled = false;
                if (e.Key == Key.Left)
                {
                    txtAssyLotID45.Focus();
                }
                else if (e.Key == Key.Right)
                {
                    txtAssyLotID8.Focus();
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion

        #region [Inbox 정보 수정]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChange())
                return;

            // 수정 하시겠습니까?
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeProcess();
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

        private bool ChkAssyLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _assyLotID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_ASSY_LOT_INFO_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// Inbox 정보 변경
        /// </summary>
        private void ChangeProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("COLNAME", typeof(string));
                inTable.Columns.Add("VALUE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                string colname = string.Empty;
                string value = string.Empty;

                if ((bool)rdoAssyLot.IsChecked)
                {
                    colname = "LOTID_RT";
                    value = _assyLotID.ToUpper();
                }
                else if ((bool)rdoGrade.IsChecked)
                {
                    colname = "CAPA_GRD_CODE";
                    value = Util.NVC(cboCapaGrade.SelectedValue);
                }

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtInboxID.Text;
                newRow["COLNAME"] = colname;
                newRow["VALUE"] = value;
                newRow["USERID"] = LoginInfo.USERID;
                if(Ctnr_Create == "Y")
                {
                    newRow["ACT_USERID"] = txtReqUserCreate.Tag;
                }
                else
                {
                    newRow["ACT_USERID"] = Util.NVC(cboTakeOverUser.SelectedValue);
                }
               
                newRow["NOTE"] = txtNote.Text;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_LOT", "INDATA", null, inTable, (result, searchException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

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

        private bool ValidationChange()
        {
            if ((bool)rdoAssyLot.IsChecked)
            {
                _assyLotID = txtAssyLotID1.Text.Trim() +
                             txtAssyLotID2.Text.Trim() +
                             txtAssyLotID3.Text.Trim() +
                             txtAssyLotID45.Text.Trim() +
                             txtAssyLotID67.Text.Trim() +
                             txtAssyLotID8.Text.Trim();

                if (_assyLotID.Length != 8)
                {
                    // 조립LOT 정보는 8자리 입니다.
                    Util.MessageValidation("SFU4228");
                    return false;
                }

                if (!ChkAssyLot())
                {
                    // 조립 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4001");
                    return false;
                }
            }
            else if ((bool)rdoGrade.IsChecked)
            {
                if (cboCapaGrade.SelectedValue == null || cboCapaGrade.SelectedValue.GetString().Equals("SELECT"))
                {
                    // 설비의 Inbox 유형을 설정 하세요.
                    Util.MessageValidation("SFU4354");
                    return false;
                }
            }
            else
            {
                if (txtNextQty.Value == 0)
                {
                    // 수량은 0보다 커야 합니다.
                    Util.MessageValidation("SFU1683");
                    return false;
                }
            }
            if(Ctnr_Create == "Y")
            {
                if (string.IsNullOrWhiteSpace(txtReqUserCreate.Text) || txtReqUserCreate.Tag == null)
                {
                    // 작업자 정보를 입력하세요.
                    Util.MessageValidation("SFU4201");
                    return false;
                }
            }
            else
            {
                if (cboTakeOverUser.SelectedValue == null || cboTakeOverUser.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 작업자 정보를 입력하세요.
                    Util.MessageValidation("SFU4201");
                    return false;
                }
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

        #region [작업자]
        private void txtReqUserCreate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        private void btnReqUserCreate_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;
            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtReqUserCreate.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        grdMain.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }


            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtReqUserCreate.Text = wndPerson.USERNAME;
                txtReqUserCreate.Tag = wndPerson.USERID;

            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }
        #endregion
    }
}