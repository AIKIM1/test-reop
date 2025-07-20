/*************************************************************************************
 Created Date : 2017.07.28
      Creator : 
   Decription : 특성검사 작업시작
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
    public partial class FORM001_002_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        DataTable _worTypeCode;
        DataTable _lotType;
        DataTable _assyLot;

        private bool _load = true;
        private string _offgrd_LotTypeCode;
        private string _offgrd_PrdtSffx;

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

        public FORM001_002_RUNSTART()
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

            txtStartPallet.Focus();
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

            _worTypeCode = DataTableConverter.Convert(cboFormWorkType.ItemsSource);

            // 작업 업체
            string[] sFilterSupplier = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboWorkSupplier, CommonCombo.ComboStatus.NONE, sFilter: sFilterSupplier);

            DataTable dtSupplier = DataTableConverter.Convert(cboWorkSupplier.ItemsSource);

            int rowindex = dtSupplier.AsEnumerable().Select(row => row.Field<string>("DFLT_WRK_SUPPLIER_FLAG") == "Y").ToList().FindIndex(col => col);

            if (rowindex >= 0)
                cboWorkSupplier.SelectedIndex = rowindex;

            SetGrideOffGrade(false);

        }
        #endregion

        #region [시작 Pallet KeyDown시 조립 Lot 정보 조회]
        private void txtStartPallet_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtStartPallet_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 조립 Lot 정보 검색
                GetAssyLotInfo();
            }
        }
        #endregion

        #region [등외품 Check, UnChkeck]
        private void chkNonrated_Checked(object sender, RoutedEventArgs e)
        {
            SetGrideOffGrade(true);
        }

        private void chkNonrated_Unchecked(object sender, RoutedEventArgs e)
        {
            SetGrideOffGrade(false);
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
                else
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "FORM_WRK_TYPE_NAME","");
            }

        }
        #endregion

        #region [등외품 lot, 제품 조회 팝업]
        private void btnSOffGradeProd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOffGrade())
                return;

            FORM001_OFFGRD_LOT_TYPE popupOffGrade = new FORM001_OFFGRD_LOT_TYPE();
            popupOffGrade.FrameOperation = this.FrameOperation;

            //object[] parameters = new object[2];
            //parameters[0] = _procID;
            //parameters[1] = _eqptID;

            C1WindowExtension.SetParameters(popupOffGrade, null);

            popupOffGrade.Closed += new EventHandler(popupOffGrade_Closed);

            ////popupTagPrint.Show();
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupOffGrade);
                    popupOffGrade.BringToFront();
                    break;
                }
            }

        }

        private void popupOffGrade_Closed(object sender, EventArgs e)
        {
            FORM001_OFFGRD_LOT_TYPE popup = sender as FORM001_OFFGRD_LOT_TYPE;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _offgrd_LotTypeCode = popup.Offgrd_LotTypeCode;
                _offgrd_PrdtSffx = popup.Offgrd_PrdtSffx;

                if (_assyLot.Rows[0]["ASSY_LOTID"].ToString().Length < 7 || String.IsNullOrWhiteSpace(_offgrd_PrdtSffx) || _offgrd_PrdtSffx.Length < 1)
                {
                    _offgrd_LotTypeCode = string.Empty;
                    _offgrd_PrdtSffx = string.Empty;

                    // 조립 Lot 정보가 없거나 Lot의 자릿수가 틀립니다.
                    Util.MessageValidation("SFU4028");
                    return;
                }
                else
                {
                    // 전에 등외작업 했던 경우 - 이전 제품 ID만..
                    string[] ProdSplit = _assyLot.Rows[0]["PRODID"].ToString().Split('-');

                    //등외품 접미어로 등외품 제품 코드 검색
                    string offGradeProdID = string.Empty;
                    offGradeProdID = GetOffGradeProdID(ProdSplit[0], _offgrd_PrdtSffx);

                    //현재 등외품 제품코드 화면에 참조로만 보야주고 사용하는 곳 없음.
                    //등외 제품코드 없으면 기존 제품코드에 _offgrd_PrdtSffx 붙여서 보여주고 작업 가능하도록.
                    //향후 등외 제품코드 필요하게 되면 등외 제품코드 없으면 작업 못하도록 수정 필요.
                    if (String.IsNullOrEmpty(offGradeProdID))
                    {
                        //_offgrd_LotTypeCode = string.Empty;
                        //_offgrd_PrdtSffx = string.Empty;

                        //return;

                        txtOffGradeProdID.Text = ProdSplit[0] + "-" + _offgrd_PrdtSffx;
                    }
                    else
                    {
                        txtOffGradeProdID.Text = offGradeProdID;
                    }

                    txtOffGradeAssyLotID.Text = txtOffGradeAssyLotID.Text.Replace(_assyLot.Rows[0]["ASSY_LOTID"].ToString().Substring(5, 2), _offgrd_LotTypeCode);
                    ////txtOffGradeProdID.Text = _assyLot.Rows[0]["PRODID"].ToString() + "-" + _offgrd_PrdtSffx;
                    //txtOffGradeProdID.Text = ProdSplit[0] + "-" + _offgrd_PrdtSffx;
                }

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        private string GetOffGradeProdID(string pProdID, string pOffgrdPrdtSffx)
        {
            string offGradeProdID = string.Empty;

            try
            {
                if (String.IsNullOrEmpty(pProdID))
                {
                    Util.MessageValidation("100940");
                    return offGradeProdID;
                }

                if (String.IsNullOrEmpty(pOffgrdPrdtSffx))
                {
                    Util.MessageValidation("100939");
                    return offGradeProdID;
                }

                string sBizName = "BR_PRD_SEL_OFFGRD_PRDT";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("OFFGRD_PRDTSFFX", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PRODID"] = pProdID;
                newRow["OFFGRD_PRDTSFFX"] = pOffgrdPrdtSffx;
                inTable.Rows.Add(newRow);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    offGradeProdID = dtResult.Rows[0]["PRODID"].ToString();
                }
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return offGradeProdID;
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
        /// 작업구분 콤보 데이터
        /// </summary>
        private void SetComboFormWrkType()
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

                _worTypeCode = new DataTable();
                _worTypeCode = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Lot 정보 조회
        /// </summary>
        private void GetAssyLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgLot);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("ASSYLOT_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("NONRATED", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["PALLETID"] = txtStartPallet.Text;
                newRow["ASSYLOT_ID"] = txtAssyLotID.Text;
                newRow["PROCID"] = _procID;
                newRow["NONRATED"] = (bool)chkNonrated.IsChecked ? "Y" : "N";
                inTable.Rows.Add(newRow);

                _assyLot = new DataTable();
                _assyLot = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_PALLET_ASSY_LOT_INFO_CM", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgLot, _assyLot, null, true);

                if (_assyLot != null && _assyLot.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(_assyLot.Rows[0]["FORM_WRK_TYPE_CODE"].ToString()))
                        cboFormWorkType.SelectedIndex = 0;
                    else
                        cboFormWorkType.SelectedValue = _assyLot.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();

                    txtAssyLotID.Text = _assyLot.Rows[0]["ASSY_LOTID"].ToString();

                    if (_assyLot.Rows[0]["HOLD_CNT"].ToString() != "0")
                    {
                        // 조립LOT[%1]는 HOLD 되어 있습니다. \r\n출하는 불가능하나 공정진행은 가능합니다.
                        Util.MessageValidation("SFU4139", txtAssyLotID.Text);
                    }

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

        private void GetLotType()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTTYPE"] = "O";
                RQSTDT.Rows.Add(dr);

                _lotType = new DataTable();
                _lotType = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTYPE_FO", "RQSTDT", "RSLTDT", RQSTDT);

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
        private void StartRunProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_CM";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_SUPPLIERID", typeof(string));
                inTable.Columns.Add("OFFGRADE_FLAG", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["ASSY_LOTID"] = (bool)chkNonrated.IsChecked ? txtOffGradeAssyLotID.Text : Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID"));
                newRow["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PALLETID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                newRow["OFFGRADE_FLAG"] = (bool)chkNonrated.IsChecked ? "Y" : "N";
                newRow["PRODID"] = (bool)chkNonrated.IsChecked ? txtOffGradeProdID.Text : Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"));
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();

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
        private bool ValidateOffGrade()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 조립 Lot 정보 조회후 등외품 작업을 하세요.
                Util.MessageValidation("SFU4026");
                return false;
            }

            return true;
        }

        private bool ValidateStartRun()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4002");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID"))))
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            if ((bool)chkNonrated.IsChecked)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID")) != txtAssyLotID.Text)
                {
                    // 입력한 조립 Lot 정보와 조회된 조립 Lot 정보가 틀립니다. 입력 조립 Lot에서 Enter를 누르세요.
                    Util.MessageValidation("SFU4030");
                    return false;
                }
            }

            if (cboFormWorkType.SelectedValue == null || cboFormWorkType.SelectedValue.ToString().Equals("SELECT"))
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

            if ((bool)chkNonrated.IsChecked)
            {
                // 등외품 작업시 체크
                if (string.IsNullOrWhiteSpace(_offgrd_LotTypeCode) || string.IsNullOrWhiteSpace(_offgrd_PrdtSffx))
                {
                    // 등외 제품을 선택 하세요.
                    Util.MessageValidation("SFU4121");
                    return false;
                }

                //if (string.IsNullOrWhiteSpace(txtOffGradeProdID.Text))
                //{
                //    // 등외 제품을 선택 하세요.
                //    Util.MessageValidation("1002");
                //    return false;
                //}
            }

            return true;
        }
        #endregion

        #region [Func]

        private void SetGrideOffGrade(bool isChk)
        {
            try
            {
                _offgrd_LotTypeCode = string.Empty;
                _offgrd_PrdtSffx = string.Empty;

                if (isChk)
                {
                    // 등외품 작업일 경우
                    if (dgLot.Rows.Count <= 0)
                    {
                        chkNonrated.IsChecked = false;

                        // 조립 Lot 정보 조회후 등외품 작업을 하세요.
                        Util.MessageValidation("SFU4026");
                        return;
                    }

                    // 등외품 작업으로 변경
                    SetFormWrkType("FORM_WORK_OG");

                    //// 등외 LOT TYPE 갖고 오기
                    //if (_lotType == null || _lotType.Rows.Count == 0)
                    //    GetLotType();

                    btnSOffGradeProd.IsEnabled = true;
                    tbOffGradeAssyLotID.Foreground = new SolidColorBrush(Colors.Black);
                    tbOffGradeProdID.Foreground = new SolidColorBrush(Colors.Black);

                    txtOffGradeAssyLotID.Text = _assyLot.Rows[0]["ASSY_LOTID"].ToString();
                    txtOffGradeProdID.Text = _assyLot.Rows[0]["PRODID"].ToString();
                }
                else
                {
                    btnSOffGradeProd.IsEnabled = false;
                    tbOffGradeAssyLotID.Foreground = new SolidColorBrush(Colors.Gray);
                    tbOffGradeProdID.Foreground = new SolidColorBrush(Colors.Gray);

                    txtOffGradeAssyLotID.Text = string.Empty;
                    txtOffGradeProdID.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 작업 구분 Setting
        /// </summary>
        /// <param name="wrkTypeCode"></param>
        private void SetFormWrkType(string wrkTypeCode)
        {
            try
            {
                // 조회된 wrkTypeCode만 콤보에 조회
                ////DataTable dt = new DataTable();
                ////dt = _worTypeCode.Copy();
                ////dt.Select("CBO_CODE <> '" + wrkTypeCode + "'").ToList<DataRow>().ForEach(row => row.Delete());
                ////dt.AcceptChanges();

                ////cboFormWorkType.Text = null;
                ////cboFormWorkType.ItemsSource = dt.Copy().AsDataView();

                if (cboFormWorkType.Items.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(wrkTypeCode))
                        cboFormWorkType.SelectedValue = wrkTypeCode;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

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

        #endregion






    }
}
