/*************************************************************************************
 Created Date : 2017.07.21
      Creator : 
   Decription : 작업시작
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

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_003_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private string _job = string.Empty;

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

        public FORM001_003_RUNSTART()
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
                _load = false;
            }

            txtStartTray.Focus();
        }

        private void InitializeUserControls()
        {
            Util.gridClear(dgLot);
            cboLottype.IsEnabled = true;
            txtAssyLotID.Text = string.Empty;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            // 작업구분
            CommonCombo _combo = new CommonCombo();
            ////string[] sFilter = { "FORM_WRK_TYPE_CODE" };
            ////_combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
            string[] sFilter = { LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID };
            _combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE_LINE", sFilter: sFilter);

            // 작업 업체
            string[] sFilterSupplier = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboWorkSupplier, CommonCombo.ComboStatus.NONE, sFilter: sFilterSupplier);

            DataTable dtSupplier = DataTableConverter.Convert(cboWorkSupplier.ItemsSource);

            int rowindex = dtSupplier.AsEnumerable().Select(row => row.Field<string>("DFLT_WRK_SUPPLIER_FLAG") == "Y").ToList().FindIndex(col => col);

            if (rowindex >= 0)
                cboWorkSupplier.SelectedIndex = rowindex;

            // Lot type
            SetLotType();
            cboLottype.SelectedValueChanged += cboLottype_SelectedValueChanged;

            // 남경일때만 활성화
            if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                txtAssyLotID.IsEnabled = true;
                btnAssyLot.IsEnabled = true;
            }
            else
            {
                txtAssyLotID.IsEnabled = false;
                btnAssyLot.IsEnabled = false;
            }
        }
        #endregion

        #region [시작 Tray KeyDown시 조립 Lot 정보 조회]
        private void txtStartTray_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtStartTray_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 시작 Pallet로 조립 Lot 검색
                if (CheckPallet())
                {
                    // 조립 Lot 정보 검색
                    GetAssyLotInfo();
                }
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

        #region [SOC 값 변경 txtSocValue_TextChanged, txtSocValue_KeyDown]
        private void txtSocValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgLot.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "SOC_VALUE", txtSocValue.Text);
            }

        }

        private void txtSocValue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                 (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                  e.Key == Key.Back ||
                  e.Key == Key.Tab ||
                 (e.Key >= Key.Left && e.Key <= Key.Down)))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

        }
        #endregion

        #region AssyLot 팝업
        private void txtAssyLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

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

            FORM001_ASSYLOT popupAssyLot = new FORM001_ASSYLOT { FrameOperation = this.FrameOperation };

            C1WindowExtension.SetParameters(popupAssyLot, null);
            popupAssyLot.AssyLot = txtAssyLotID.Text;

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

            FORM001_ASSYLOT popup = sender as FORM001_ASSYLOT;
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
        private void rdoTray_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작 Tray");
            txtStartTray.Focus();

            // 남경인 경우만 적용
            if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                txtAssyLotID.IsEnabled = true;
                btnAssyLot.IsEnabled = true;
            }

        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작 Pallet");
            txtStartTray.Focus();

            // 남경인 경우만 적용
            if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                txtAssyLotID.IsEnabled = false;
                btnAssyLot.IsEnabled = false;
            }
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
        private bool CheckPallet()
        {
            try
            {
                ShowLoadingIndicator();
                // Clear
                InitializeUserControls();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["INPUT_LOTID"] = txtStartTray.Text;
                newRow["INPUT_TYPE"] = (bool)rdoTray.IsChecked ? "T" : "P";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_GS", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtAssyLotID.Text = dtResult.Rows[0]["ASSY_LOTID"].ToString();

                    if (dtResult.Rows[0]["LOTTYPE"].ToString().Equals("N"))
                    {
                        cboLottype.IsEnabled = false;
                    }

                    return true;
                }
                else
                {
                    //// Tray 정보가 없습니다.
                    //Util.MessageValidation("SFU4031");

                    if ((bool)rdoTray.IsChecked)
                    {
                        // Tray 정보가 없습니다.
                        Util.MessageValidation("SFU4031");
                    }
                    else
                    {
                        // Pallet 정보가 없습니다.
                        Util.MessageValidation("SFU4246");
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.gridClear(dgLot);
                Util.MessageException(ex);
                return false;
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
                if (string.IsNullOrWhiteSpace(txtAssyLotID.Text))
                {
                    // 조립 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4001");
                    return;
                }

                _job = string.Empty;
                string bizName = string.Empty;

                if ((bool)rdoTray.IsChecked)
                {
                    // Tray
                    if (string.IsNullOrWhiteSpace(txtStartTray.Text))
                    {
                        _job = "TRAY_NJ";
                        bizName = "DA_PRD_SEL_INPUT_TRAY_ASSY_LOT_INFO_NJ";
                    }
                    else
                    {
                        _job = "TRAY";
                        bizName = "DA_PRD_SEL_INPUT_TRAY_ASSY_LOT_INFO_GD";
                    }
                }
                else
                {
                    // Pallet
                    _job = "PALLET";
                    bizName = "DA_PRD_SEL_INPUT_PALLET_ASSY_LOT_INFO_GD";
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                if ((bool)rdoTray.IsChecked)
                {
                    inTable.Columns.Add("AREAID", typeof(string));
                }

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;

                if (!_job.Equals("TRAY_NJ"))
                {
                    newRow["PROCID"] = _procID;
                    newRow["EQPTID"] = _eqptID;
                    newRow["CSTID"] = txtStartTray.Text;
                }
                newRow["LOTID"] = txtAssyLotID.Text;
                if ((bool)rdoTray.IsChecked)
                {
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                }

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgLot, dtResult, null, true);

                cboLottype.IsEnabled = true;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString()))
                        cboFormWorkType.SelectedIndex = 0;
                    else
                        cboFormWorkType.SelectedValue = dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();

                    if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["LOTTYPE"].ToString()))
                    {
                        cboLottype.SelectedIndex = 0;
                    }
                    else
                    {
                        cboLottype.SelectedValue = dtResult.Rows[0]["LOTTYPE"].ToString();
                        cboLottype.IsEnabled = false;
                    }

                    txtAssyLotID.Text = dtResult.Rows[0]["LOTID"].ToString();
                    txtSocValue.Text = dtResult.Rows[0]["SOC_VALUE"].ToString();

                    if (dtResult.Rows[0]["HOLD_CNT"].ToString() != "0")
                    {
                        // 조립LOT[%1]는 HOLD 되어 있습니다. \r\n출하는 불가능하나 공정진행은 가능합니다.
                        Util.MessageValidation("SFU4139", dtResult.Rows[0]["LOTID"].ToString());
                    }
                }
                else
                {
                    txtSocValue.Text = string.Empty;

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

        #region 작업구분 콤보
        private void SetComboFormWrkType(string wrkTypeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_WRK_TYPE_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                // 조회된 wrkTypeCode만 콤보에 조회
                dtResult.Select("CBO_CODE <> '" + wrkTypeCode + "'").ToList<DataRow>().ForEach(row => row.Delete());
                dtResult.AcceptChanges();

                cboFormWorkType.DisplayMemberPath = "CBO_NAME";
                cboFormWorkType.SelectedValuePath = "CBO_CODE";
                cboFormWorkType.ItemsSource = dtResult.Copy().AsDataView();

                if (cboFormWorkType.Items.Count > 0)
                    cboFormWorkType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        /// <summary>
        /// 작업 시작
        /// </summary>
        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_GS";

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
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("SOC_VALUE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID"));
                newRow["INPUT_LOTID"] = string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CSTID"))) ? "NA" : Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CSTID"));
                newRow["INPUT_TYPE"] = (bool)rdoTray.IsChecked ? "T" : "P";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();
                newRow["LOTTYPE"] = cboLottype.SelectedValue.ToString();
                newRow["SOC_VALUE"] = txtSocValue.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "OUTDATA", inTable, (result, bizException) =>
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

            if (cboFormWorkType.SelectedValue == null || cboFormWorkType.SelectedValue.GetString().Equals("SELECT"))
            {
                // 작업 구분을 선택 하세요.
                Util.MessageValidation("SFU4002");
                return false;
            }

            if (cboWorkSupplier.SelectedValue == null)
            {
                // 작업 업체를 선택 하세요.
                Util.MessageValidation("SFU4003");
                return false;
            }

            if (cboLottype.SelectedValue == null || cboLottype.SelectedValue.GetString().Equals("SELECT"))
            {
                // LOT 유형을 선택하세요.
                Util.MessageValidation("SFU4068");
                return false;
            }

            if (cboLottype.IsEnabled == true && cboLottype.SelectedValue.ToString().Equals("N"))
            {
                // 반품 Pallet인 경우만 Lot 유형을 반품으로 선택할 수 있습니다.
                Util.MessageValidation("SFU4293");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSocValue.Text))
            {
                // SOC 정보를 입력하세요.
                Util.MessageValidation("SFU4203");
                return false;
            }

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

    }
}
