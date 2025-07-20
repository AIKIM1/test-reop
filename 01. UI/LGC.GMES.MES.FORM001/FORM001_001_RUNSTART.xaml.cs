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
    public partial class FORM001_001_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        DataTable _prodList;
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

        public FORM001_001_RUNSTART()
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

            tbProd.Visibility = Visibility.Collapsed;
            cboProd.Visibility = Visibility.Collapsed;
            txtProd.Visibility = Visibility.Collapsed;

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

        #region Tray, Pallet RadioButton Click
        private void rdoTray_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작 Tray");
            txtStartTray.Focus();
        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (tbStartTray == null)
                return;

            tbStartTray.Text = ObjectDic.Instance.GetObjectName("시작 Pallet");
            txtStartTray.Focus();
        }
        #endregion

        #region 제품 ID 콤보 
        private void cboProd_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLot.Rows.Count > 0 && cboProd.SelectedIndex >= 0)
            {
                DataTable dt = DataTableConverter.Convert(cboProd.ItemsSource);
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", dt.Rows[cboProd.SelectedIndex]["PRJT_NAME"].ToString());
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", dt.Rows[cboProd.SelectedIndex]["PRODID"].ToString());
            }

        }
        #endregion

        #region AutoCompleteTextBox KeyDown
        private void txtProd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataRow[] dr = _prodList.Select("PRODID = '" + txtProd.Text + "'");

                if (dr.Length > 0)
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", dr[0]["PRJT_NAME"].ToString());
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", dr[0]["PRODID"].ToString());
                }
                else
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", "");
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", "");
                }

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_GD", "INDATA", "OUTDATA", inTable);

                txtAssyLotID.Text = string.Empty;

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtAssyLotID.Text = dtResult.Rows[0]["ASSY_LOTID"].ToString();
                    return true;
                }
                else
                {
                    Util.gridClear(dgLot);
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
                if (string.IsNullOrWhiteSpace(txtAssyLotID.Text) || txtAssyLotID.Text.Length < 8)
                {
                    // 조립 Lot 정보가 없거나 Lot의 자릿수가 틀립니다.
                    Util.MessageValidation("SFU4028");
                    return;
                }

                _job = string.Empty;
                string bizName = (bool)rdoTray.IsChecked ? "DA_PRD_SEL_INPUT_TRAY_ASSY_LOT_INFO_GD" : "DA_PRD_SEL_INPUT_PALLET_ASSY_LOT_INFO_GD";

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
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["LOTID"] = txtAssyLotID.Text;
                newRow["CSTID"] = txtStartTray.Text;
                if ((bool)rdoTray.IsChecked)
                {
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                // 조립 Lot 정보가 없는경우 FCS 정보 검색
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    if ((bool)rdoTray.IsChecked)
                    {
                        ////newRow = inTable.NewRow();
                        ////newRow["LANGID"] = LoginInfo.LANGID; ;
                        ////newRow["PROCID"] = _procID;
                        ////newRow["EQPTID"] = _eqptID;
                        ////newRow["LOTID"] = txtAssyLotID.Text;
                        ////newRow["CSTID"] = txtStartTray.Text;
                        ////inTable.Rows.Add(newRow);

                        dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_TRAY_LOT_INFO_GD", "INDATA", "OUTDATA", inTable);
                    }
                    else
                    {
                        // 조립 Lot 정보가 없습니다. => Pallet인 경우는 없으면 오류 메세지 처리후 Return
                        Util.MessageValidation("SFU4001");
                        return;
                    }
                }
                else
                {
                    _job = "1st";

                    if (dtResult.Rows[0]["HOLD_CNT"].ToString() != "0")
                    {
                        // 조립LOT[%1]는 HOLD 되어 있습니다. \r\n출하는 불가능하나 공정진행은 가능합니다.
                        Util.MessageValidation("SFU4139", dtResult.Rows[0]["LOTID"].ToString());
                    }
                }

                Util.GridSetData(dgLot, dtResult, null, true);

                cboLottype.IsEnabled = true;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString()))
                        cboLottype.SelectedIndex = 0;
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
                }
                else
                {
                    // 조립 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4001");
                    return;
                }

                #region 제품 콤보 생성
                if (string.IsNullOrWhiteSpace(_job))
                {
                    // 제품 콤보 생성 : 조립 Lot 정보가 없다  FCS의 조립 Lot을 8자리로 검색
                    DataTable inTableAssy = new DataTable();
                    inTableAssy.Columns.Add("ASSY_LOTID", typeof(string));

                    DataRow newRowAssy = inTableAssy.NewRow();
                    newRowAssy["ASSY_LOTID"] = txtAssyLotID.Text.Substring(0, 8);
                    inTableAssy.Rows.Add(newRowAssy);

                    DataTable dtResultAssy = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_PROD_FO", "INDATA", "OUTDATA", inTableAssy);

                    if (dtResultAssy != null && dtResultAssy.Rows.Count > 0)
                    {
                        _job = "2st";
                        cboProd.SelectedValueChanged -= cboProd_SelectedValueChanged;

                        cboProd.ItemsSource = null;
                        cboProd.ItemsSource = dtResultAssy.Copy().AsDataView();
                        cboProd.SelectedIndex = 0;

                        // 첫번째 값을 그리드에 넣어 준다
                        DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", dtResultAssy.Rows[0]["PRJT_NAME"].ToString());
                        DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", dtResultAssy.Rows[0]["PRODID"].ToString());
                        cboProd.SelectedValueChanged += cboProd_SelectedValueChanged;
                    }

                }
                #endregion

                #region 제품 AutoCompleteTextBox 생성
                if (string.IsNullOrWhiteSpace(_job))
                {
                    // 제품 AutoCompleteTextBox 생성 : CLSS3_CODE= 'MCC' 제품 정보 조회
                    DataTable inTableProd = new DataTable();
                    inTableProd.Columns.Add("CLSS3_CODE", typeof(string));

                    DataRow newRowProd = inTableProd.NewRow();
                    newRowProd["CLSS3_CODE"] = "MCC";
                    inTableProd.Rows.Add(newRowProd);

                    _prodList = new DataTable();
                    _prodList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_FO", "INDATA", "OUTDATA", inTableProd);

                    if (_prodList != null && _prodList.Rows.Count > 0)
                    {
                        _job = "3st";
                        txtProd.Text = _prodList.Rows[0]["PRODID"].ToString().Substring(0, 3);

                        foreach (DataRow r in _prodList.Rows)
                        {
                            string displayString = r["PRODID"].ToString(); //표시 텍스트
                            string keywordString;

                            keywordString = displayString;
                            txtProd.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString));
                        }
                    }

                }
                #endregion

                if (string.Equals(_job, "1st"))
                {
                    tbProd.Visibility = Visibility.Collapsed;
                    cboProd.Visibility = Visibility.Collapsed;
                    txtProd.Visibility = Visibility.Collapsed;
                }
                else if (string.Equals(_job, "2st"))
                {
                    tbProd.Visibility = Visibility.Visible;
                    cboProd.Visibility = Visibility.Visible;
                    txtProd.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbProd.Visibility = Visibility.Visible;
                    cboProd.Visibility = Visibility.Collapsed;
                    txtProd.Visibility = Visibility.Visible;
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

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_GD";

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
                inTable.Columns.Add("PRODID", typeof(string));                         // (*OPT)조립 LOT 생성시 필요
                inTable.Columns.Add("LOTTYPE", typeof(string));                        // (*OPT)조립 LOT 생성시 필요

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["ASSY_LOTID"] = txtAssyLotID.Text;
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "CSTID"));
                newRow["INPUT_TYPE"] = (bool)rdoTray.IsChecked ? "T" : "P";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"));
                newRow["LOTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTTYPE"));

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

            if (_prodList != null && _prodList.Rows.Count > 0)
            {
                DataRow[] dr = _prodList.Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID")) + "'");
                if (dr.Length == 0)
                {
                    // 제품 정보가 없습니다.
                    Util.MessageValidation("SFU4029");
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"))))
                {
                    // 제품 정보가 없습니다.
                    Util.MessageValidation("SFU4029");
                    return false;
                }

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
