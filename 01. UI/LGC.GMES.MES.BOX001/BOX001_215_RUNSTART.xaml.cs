/*************************************************************************************
 Created Date : 2017.12.07
      Creator : 
   Decription : 파활성화 후공정 파우치 : 작업시작
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_215_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        //private string _procID = string.Empty;              // 공정코드
        //private string _eqptID = string.Empty;              // 설비코드
        //private string _divisionCode = string.Empty;        // Degas Tray, Pallet 구분자

        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _processName = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _divisionCode = string.Empty;
        private string _userID = string.Empty;

        private bool _load = true;

        public string ProdLotId { get; set; }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public BOX001_215_RUNSTART()
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
                object[] parameters = C1WindowExtension.GetParameters(this);
                _processCode = parameters[0] as string;
                _processName = parameters[1] as string;
                _equipmentCode = parameters[2] as string;
                _equipmentName = parameters[3] as string;
                _divisionCode = parameters[4] as string;
                _equipmentSegmentCode = parameters[5] as string;
                _userID = parameters[6] as string;

                if (_processName != null) txtProcess.Text = _processName;
                if (_equipmentName != null) txtEquipment.Text = _equipmentName;

                InitializeUserControls();
                SetControl();
                
                _load = false;
            }

            txtStartTray.Focus();
        }

        private void InitializeUserControls()
        {
            Util.gridClear(dgLot);
         //   txtAssyLotID.Text = string.Empty;
       //     cboLottype.IsEnabled = true;
        }
        private void SetControl()
        {
            // 작업구분
            CommonCombo combo = new CommonCombo();
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _equipmentSegmentCode };
            //combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE_LINE", sFilter: sFilter);
            //combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "MKT_TYPE_CODE", sFilter: sFilter);

            // Lot type
            SetLotType();
            cboLottype.SelectedValueChanged += cboLottype_SelectedValueChanged;

        }


        #endregion

        #region [시작 Tray KeyDown시 조립 Lot 정보 조회]
        private void txtStartTray_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 시작 Tray, Pallet로 조립 Lot 검색
                CheckPallet();
            }
        }
        #endregion

        #region [dgLot 색상 dgLot_LoadedCellPresenter]
        private void dgLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("FORM_WRK_TYPE_NAME"))
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
            }));
        }
        #endregion

        #region [LotType 변경 cboLottype_SelectedValueChanged]
        private void cboLottype_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLot.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTTYPE", cboLottype.SelectedValue ?? cboLottype.SelectedValue.ToString());
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTYNAME", cboLottype.Text);
            }

        }
        #endregion

        #region [작업구분 변경 cboFormWorkType_SelectedValueChanged]
        private void cboFormWorkType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLot.Rows.Count > 0)
            {
                string[] FromWorkTypeSplit = cboFormWorkType.Text.Split(':');
                if (FromWorkTypeSplit.Length > 1)
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "FORM_WRK_TYPE_NAME", FromWorkTypeSplit[1]);
            }

        }
        #endregion

        #region AssyLot 팝업
        private void txtAssyLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AssyLotPopUp();
            }
        }

        private void btnAssyLot_Click(object sender, RoutedEventArgs e)
        {
            AssyLotPopUp();
        }

        private void AssyLotPopUp()
        {
            if (txtAssyLotID.Text.Length < 4)
            {
                // Lot ID는 4자리 이상 넣어 주세요.
                Util.MessageValidation("SFU3450");
                return;
            }

            BOX001_215_ASSYLOT popupAssyLot = new BOX001_215_ASSYLOT { FrameOperation = this.FrameOperation };

            C1WindowExtension.SetParameters(popupAssyLot, null);
            popupAssyLot.AssyLot = txtAssyLotID.Text;
            popupAssyLot.PolymerYN = "Y";

            popupAssyLot.Closed += new EventHandler(popupAssyLot_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupAssyLot);
                    popupAssyLot.BringToFront();
                    break;
                }
            }
        }

        private void popupAssyLot_Closed(object sender, EventArgs e)
        {
            Util.gridClear(dgLot);

            BOX001_215_ASSYLOT popup = sender as BOX001_215_ASSYLOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                txtAssyLotID.Text = popup.AssyLot;
                GetAssyLotInfo();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

            txtAssyLotID.Focus();
        }
        #endregion

        #region Tray, Pallet RadioButton Click
        private void rdoInbox_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작INBOX");
            dgLot.Columns["CSTID"].Header = Util.NVC(ObjectDic.Instance.GetObjectName("INBOXID"));
            txtStartTray.Focus();
            if (!string.IsNullOrWhiteSpace(txtStartTray.Text))
                CheckPallet();
        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작대차");
            if(dgLot!=null)
                dgLot.Columns["CSTID"].Header = Util.NVC(ObjectDic.Instance.GetObjectName("대차ID"));
            txtStartTray.Focus();
            if (!string.IsNullOrWhiteSpace(txtStartTray.Text))
                CheckPallet();
        }
        #endregion

        #region [작업시작]
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //if (_processCode.Equals(Process.PolymerDSF) || _processCode.Equals(Process.PolymerTaping))
                    //    StartRunProcessDSF();
                    //else
                        StartRunProcess();

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

        #region User Method

        #region [BizCall]
        /// <summary>
        /// Lot Type
        /// </summary>
        private void SetLotType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTYPE_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr[cboLottype.DisplayMemberPath.ToString()] = "-SELECT-";
                dr[cboLottype.SelectedValuePath.ToString()] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboLottype.DisplayMemberPath = cboLottype.DisplayMemberPath.ToString();
                cboLottype.SelectedValuePath = cboLottype.SelectedValuePath.ToString();
                cboLottype.ItemsSource = dtResult.Copy().AsDataView();
                cboLottype.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 시작 Pallet 체크
        /// </summary>
        private void CheckPallet()
        {
            try
            {
                ShowLoadingIndicator();

                // Clear
                InitializeUserControls();

                string bizName;

                if (rdoInbox.IsChecked != null && (bool)rdoInbox.IsChecked)
                    bizName = "BR_PRD_CHK_INPUT_LOT_INBOX";
                else
                    bizName = "BR_PRD_CHK_INPUT_LOT_CTNR";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["USERID"] = _userID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtStartTray.Text;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                        else
                        {
                            if (bizResult.Tables["OUTDATA"].Rows.Count > 0)
                            {
                               // txtAssyLotID.Text = bizResult.Tables["OUTDATA"].Rows[0]["ASSY_LOTID"].ToString();

                                //if (bizResult.Tables["OUTDATA"].Rows[0]["LOTTYPE"].ToString().Equals("N"))
                                //{
                                //    cboLottype.IsEnabled = false;
                                //}

                                // 조립 Lot 정보 조회 --> 자동조회안함
                                GetAssyLotInfo();
                            }
                        }

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
                Util.gridClear(dgLot);
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// Lot 정보 조회
        /// </summary>
        private void GetAssyLotInfo()
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(txtAssyLotID.Text))
                //{
                //    // 조립 Lot 정보가 없습니다.
                //    Util.MessageValidation("SFU4001");
                //    return;
                //}

                string bizName = string.Empty;

                if (rdoInbox.IsChecked != null && (bool)rdoInbox.IsChecked)
                    bizName = "DA_PRD_SEL_INPUT_INBOX_ASSY_LOT_INFO_NJ";
                else
                    bizName = "DA_PRD_SEL_INPUT_CTNR_ASSY_LOT_INFO_NJ";

                ShowLoadingIndicator();

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["PROCID"] = _processCode;
                newRow["CSTID"] = string.IsNullOrEmpty(txtStartTray.Text)?null:txtStartTray.Text;
                newRow["ASSY_LOTID"] = string.IsNullOrEmpty(txtAssyLotID.Text)?null: txtAssyLotID.Text;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult.Columns.Add("CHK");

                Util.GridSetData(dgLot, dtResult, FrameOperation, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    // 조회된 작업구분으로 Combo Setting
                    //if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString()))
                    //    cboFormWorkType.SelectedIndex = 0;
                    //else
                    //    cboFormWorkType.SelectedValue = dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();

                    //// 조회된 Lot 유형으로 Combo Setting
                    //if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["LOTTYPE"].ToString()))
                    //    cboLottype.SelectedIndex = 0;
                    //else
                    //    cboLottype.SelectedValue = dtResult.Rows[0]["LOTTYPE"].ToString();

                    //txtAssyLotID.Text = dtResult.Rows[0]["LOTID"].ToString();

                    //if (dtResult.Rows[0]["HOLD_CNT"].ToString() != "0")
                    //{
                    //    // 조립LOT[%1]는 HOLD 되어 있습니다. \r\n출하는 불가능하나 공정진행은 가능합니다.
                    //    Util.MessageValidation("SFU4139", dtResult.Rows[0]["LOTID"].ToString());
                    //}
                }
                else
                {
                    // 조립 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4001");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// 작업 시작
        /// </summary>
        //private void StartRunProcessDSF()
        //{
        //    try
        //    {
        //        ShowLoadingIndicator();

        //        string bizName = string.Empty;

        //        bizName = "BR_PRD_REG_START_PROD_LOT_DSF";

        //        DataSet inDataSet = new DataSet();

        //        DataTable inTable = inDataSet.Tables.Add("IN_EQP");
        //        inTable.Columns.Add("SRCTYPE", typeof(string));
        //        inTable.Columns.Add("IFMODE", typeof(string));
        //        inTable.Columns.Add("EQPTID", typeof(string));
        //        inTable.Columns.Add("USERID", typeof(string));

        //        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
        //        inInput.Columns.Add("CSTID", typeof(string));
        //        inInput.Columns.Add("CELLID", typeof(string));

        //        // INDATA SET
        //        DataRow newRow = inTable.NewRow();
        //        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        //        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
        //        newRow["EQPTID"] = Util.NVC(_equipmentCode);
        //        newRow["USERID"] = LoginInfo.USERID;
        //        inTable.Rows.Add(newRow);

        //        newRow = inInput.NewRow();
        //        newRow["CSTID"] = txtStartTray.Text;
        //        inInput.Rows.Add(newRow);

        //        new ClientProxy().ExecuteService_Multi(bizName, "IN_EQP,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
        //        {
        //            try
        //            {
        //                HiddenLoadingIndicator();

        //                if (bizException != null)
        //                {
        //                    Util.MessageException(bizException);
        //                }

        //                if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
        //                {
        //                    ProdLotId = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();
        //                }

        //                ////Util.AlertInfo("정상 처리 되었습니다.");
        //                //Util.MessageInfo("SFU1889");

        //                this.DialogResult = MessageBoxResult.OK;
        //            }
        //            catch (Exception ex)
        //            {
        //                HiddenLoadingIndicator();
        //                Util.MessageException(ex);
        //            }
        //        }, inDataSet);
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}

        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                int idxLot = _Util.GetDataGridCheckFirstRowIndex(dgLot, "CHK");

                if (idxLot < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string bizName = string.Empty;

                //if (_processCode.Equals(Process.PolymerDSF) || _processCode.Equals(Process.PolymerTaping))
                //    bizName = "BR_PRD_REG_START_PROD_LOT_DSF";
                //else
                    bizName = "BR_PRD_REG_START_PROD_NJ";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_SUPPLIERID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));             // (*)활성화 작업 유형
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("CTNR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_equipmentCode);
                newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "LOTID"));
            //    if (rdoInbox.IsChecked != null && (bool)rdoInbox.IsChecked)
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "CSTID"));
                //newRow["INPUT_LOTID"] = string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CSTID"))) ? "NA" : Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CSTID"));
             //   else
            //        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "CTNR_ID"));
                newRow["INPUT_TYPE"] = rdoInbox.IsChecked != null && (bool)rdoInbox.IsChecked ? "P" : "C";
                newRow["USERID"] = _userID;
                newRow["FORM_WRK_TYPE_CODE"] = null; //미사용
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "PRODID"));
                newRow["EXP_DOM_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "MKT_TYPE_CODE"));
                newRow["LOTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[idxLot].DataItem, "LOTTYPE"));
                newRow["CTNR_TYPE_CODE"] = "CART";

                //if (string.Equals(_divisionCode, "Tray"))
                //{
                //    newRow["CTNR_TYPE_CODE"] = "T";
                //}
                //else
                //{
                //    newRow["CTNR_TYPE_CODE"] = "P";
                //}

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizName, "RQSTDT", "OUTDATA", inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (result != null && result.Rows.Count > 0)
                        {
                            ProdLotId = result.Rows[0]["LOTID"].ToString();
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

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateStartRun()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            int idxLot = _Util.GetDataGridCheckFirstRowIndex(dgLot, "CHK");

            if (idxLot < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            // 작업구분 / LOT유형 사용 안함

            //if (cboFormWorkType.SelectedValue == null || cboFormWorkType.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // 작업 구분을 선택 하세요.
            //    Util.MessageValidation("SFU4002");
            //    return false;
            //}

            //if (cboLottype.SelectedValue == null || cboLottype.SelectedValue.GetString().Equals("SELECT"))
            //{
            //    // LOT 유형을 선택하세요.
            //    Util.MessageValidation("SFU4068");
            //    return false;
            //}

            //if (cboLottype.IsEnabled == true && cboLottype.SelectedValue.ToString().Equals("N"))
            //{
            //    // 반품 Pallet인 경우만 Lot 유형을 반품으로 선택할 수 있습니다.
            //    Util.MessageValidation("SFU4293");
            //    return false;
            //}

            return true;
        }
        #endregion

        #region [Func]
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

        #endregion

        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString) || string.IsNullOrEmpty((rb.DataContext as DataRowView).Row["CHK"].ToString())))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgLot.SelectedIndex = idx;
            }
        }
    }
}
