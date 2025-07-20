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
    public partial class FORM001_006_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

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

        public FORM001_006_RUNSTART()
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

            // 작업 업체
            string[] sFilterSupplier = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            _combo.SetCombo(cboWorkSupplier, CommonCombo.ComboStatus.NONE, sFilter: sFilterSupplier);

            DataTable dtSupplier = DataTableConverter.Convert(cboWorkSupplier.ItemsSource);

            int rowindex = dtSupplier.AsEnumerable().Select(row => row.Field<string>("DFLT_WRK_SUPPLIER_FLAG") == "Y").ToList().FindIndex(col => col);

            if (rowindex >= 0)
                cboWorkSupplier.SelectedIndex = rowindex;

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
                // 시작 Pallet로 조립 Lot 검색
                //CheckPallet();

                // 조립 Lot 정보 검색
                GetAssyLotInfo();
            }
        }
        #endregion

        #region [조립 Lot KeyDown시 조립 Lot 정보 조회]
        private void txtAssyLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtAssyLotID_KeyDown(object sender, KeyEventArgs e)
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
            txtAssyLotID.IsEnabled = true;
        }

        private void chkNonrated_Unchecked(object sender, RoutedEventArgs e)
        {
            txtAssyLotID.IsEnabled = false;
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
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["PALLETID"] = txtStartPallet.Text;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_PALLET_ASSY_LOT_INFO_FO", "INDATA", "OUTDATA", inTable);

                txtAssyLotID.Text = String.Empty;
                Util.GridSetData(dgLot, dtResult, null, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString()))
                        cboFormWorkType.SelectedIndex = 0;
                    else
                        cboFormWorkType.SelectedValue = dtResult.Rows[0]["FORM_WRK_TYPE_CODE"].ToString();

                    txtAssyLotID.Text = dtResult.Rows[0]["ASSY_LOTID"].ToString();

                    if (dtResult.Rows[0]["HOLD_CNT"].ToString() != "0")
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

                const string bizRuleName = "BR_PRD_REG_START_PROD_LOT_OCV";

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WRK_SUPPLIERID", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));             // (*)활성화 작업 유형

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PALLETID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WRK_SUPPLIERID"] = cboWorkSupplier.SelectedValue.ToString();
                newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID"));
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
