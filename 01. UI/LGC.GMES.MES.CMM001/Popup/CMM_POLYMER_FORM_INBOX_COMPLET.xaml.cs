/*************************************************************************************
 Created Date : 2018.03.01
      Creator : 정문교
   Decription : INBOX 생성 (작업유형이 일반 재작업인경우)
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
    /// CMM_POLYMER_FORM_INBOX_COMPLET.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_INBOX_COMPLET : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;             // 공정코드
        private string _procName = string.Empty;           // 공정명
        private string _eqptID = string.Empty;             // 설비코드
        private string _lineID = string.Empty;             // 라인정보
        private string _createInboxType = string.Empty;    // 생산 LOT의 첫 Inbox 생성 FIRST, 등급별 생성 GRADE, 수량으로 생성 QTY
        private DataRow drProdLot;
        private int _inboxMaxQty = 0;
        private string _assyLotID = string.Empty;
        private string _modTypeCode;                       // 월,일 수정(M), 일 수정(D)
        
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public string InspectorCode { get; set; }

        public string ShiptoID { get; set; }

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

        public CMM_POLYMER_FORM_INBOX_COMPLET()
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
                SetCombo();
                SetControl();
                SetDataGrade();
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
            _lineID = tmps[4] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtCartID.Text = tmps[5] as string;
            _createInboxType = tmps[6] as string;

            drProdLot = tmps[7] as DataRow;

            if(tmps.Length >= 9)
            {
                txtAommType.Text = Util.NVC(tmps[8]);
            }
        }

        private void SetCombo()
        {
            CommonCombo combo = new CommonCombo();

            // Mix Lot Type
            string[] sFilterMixType = { "MIX_LOT_TYPE_CODE" };
            combo.SetCombo(cboMixLotType, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilterMixType);

            // Inbox type
            string[] sFilterInboxType = { LoginInfo.CFG_AREA_ID, _procID };
            combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.NONE, sFilter: sFilterInboxType);
            SetInboxType();

            // 등급별수량입력여부
            string[] sFilterUse = { "IUSE" };
            combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilterUse);
            cboUseYN.SelectedValueChanged += cboUseYN_SelectedValueChanged;
        }

        private void SetControl()
        {
            // Inbox Type Combo는 설비의 Inbox Type로 설정
            cboInboxType.IsEnabled = false;

            // 생산Lot의 Inbox가 처음생성시는 입력 방식 선택(수량, Grade별 수량)
            if (!_createInboxType.Equals("FIRST"))
                cboUseYN.IsEnabled = false;

            if (_createInboxType.Equals("FIRST"))
            {
                cboUseYN.SelectedValue = "Y";
            }
            else if (_createInboxType.Equals("GRADE"))
            {
                txtInboxQty.IsEnabled = false;
                cboUseYN.SelectedValue = "Y";
            }
            else if (_createInboxType.Equals("QTY"))
            {
                dgInboxGrade.IsEnabled = false;
                cboUseYN.SelectedValue = "N";
            }

            SetControlEnabled(false);
            SetAssyLot(Util.NVC(drProdLot["ASSY_LOTID"]));
            GeInboxMaxQty();

            //if (_modTypeCode.Equals("M") || _modTypeCode.Equals("D") || _modTypeCode.Equals("B"))
            if (!string.IsNullOrWhiteSpace(_modTypeCode))
                txtAssyLotID.Focus();
        }

        private void SetControlEnabled(bool Enabled)
        {
            cboMixLotType.IsEnabled = Enabled;
        }

        private void SetAssyLot(string AssyLot)
        {
            // 생산LOT의 조립LOT 텍스트 박스에 바인딩
            if (AssyLot.Length >= 8)
            {
                txtAssyLotID1.Text = AssyLot.Substring(0, 1);
                txtAssyLotID2.Text = AssyLot.Substring(1, 1);
                txtAssyLotID3.Text = AssyLot.Substring(2, 1);
                txtAssyLotID4.Text = AssyLot.Substring(3, 1);
                txtAssyLotID5.Text = AssyLot.Substring(4, 1);
                txtAssyLotID6.Text = AssyLot.Substring(5, 1);
                txtAssyLotID7.Text = AssyLot.Substring(6, 1);
                txtAssyLotID8.Text = AssyLot.Substring(7, 1);

                if (AssyLot.Substring(4, 3).Equals("000") || AssyLot.Substring(5, 2).Equals("00"))
                {
                    // 불량 조립 LOT인 경우만 수정
                    if (AssyLot.Substring(4, 3).Equals("000"))
                    {
                        // 월,일변경
                        _modTypeCode = "M";
                        txtAssyLotID5.IsEnabled = true;
                        txtAssyLotID6.IsEnabled = true;
                        txtAssyLotID7.IsEnabled = true;

                        txtAssyLotID.Text = AssyLot.Substring(4, 3);
                        txtAssyLotID.IsEnabled = true;
                        txtAssyLotID.MaxLength = 3;
                    }
                    else
                    {
                        // 일변경
                        _modTypeCode = "D";
                        txtAssyLotID6.IsEnabled = true;
                        txtAssyLotID7.IsEnabled = true;

                        txtAssyLotID.Text = AssyLot.Substring(5, 2);
                        txtAssyLotID.IsEnabled = true;
                        txtAssyLotID.MaxLength = 2;
                    }
                }

                // 포장(Cell포장,물류반품)인 경우 조립LOT 5자리 수정가능
                if (Util.NVC(drProdLot["FORM_WRK_TYPE_CODE"]).Equals("FORM_WORK_RT") &&  (_procID.Equals(Process.CELL_BOXING) || _procID.Equals(Process.CELL_BOXING_RETURN)))
                {
                    _modTypeCode = "B";
                    txtAssyLotID1.IsEnabled = true;
                    txtAssyLotID2.IsEnabled = true;
                    txtAssyLotID3.IsEnabled = true;
                    txtAssyLotID4.IsEnabled = true;
                    txtAssyLotID5.IsEnabled = true;
                    txtAssyLotID6.IsEnabled = true;
                    txtAssyLotID7.IsEnabled = true;
                    txtAssyLotID8.IsEnabled = true;

                    txtAssyLotID.Text = AssyLot;
                    txtAssyLotID.IsEnabled = true;
                    txtAssyLotID.MaxLength = 8;
                }
            }
        }

        private void SetDataGrade()
        {
            // 등급 조회
            SetInboxGrade();
        }

        #endregion

        #region [조립LOTMix chkLotMix_Checked, chkLotMix_Unchecked]
        private void chkLotMix_Checked(object sender, RoutedEventArgs e)
        {
            SetControlEnabled(true);
        }

        private void chkLotMix_Unchecked(object sender, RoutedEventArgs e)
        {
            SetControlEnabled(false);
        }
        #endregion

        #region 조립LOT 일,주별 입력
        private void txtAssyLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtAssyLotID.SelectAll();
        }

        private void txtAssyLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_modTypeCode.Equals("D"))
            {
                // 일자 변경인 경우 숫자값 체크
                if (((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                     (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                      e.Key == Key.Back ||
                      e.Key == Key.Tab ||
                      e.Key == Key.Enter ||
                     (e.Key >= Key.Left && e.Key <= Key.Down)))
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void txtAssyLotID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_modTypeCode)) return;

            if (_modTypeCode.Equals("D"))
            {
                txtAssyLotID6.Text = string.Empty;
                txtAssyLotID7.Text = string.Empty;

                switch (txtAssyLotID.Text.Length)
                {
                    case 1:
                        txtAssyLotID6.Text = txtAssyLotID.Text.ToUpper();
                        break;
                    case 2:
                        txtAssyLotID6.Text = txtAssyLotID.Text.Substring(0,1).ToUpper();
                        txtAssyLotID7.Text = txtAssyLotID.Text.Substring(1,1).ToUpper();
                        break;
                }
            }
            else if (_modTypeCode.Equals("M"))
            {
                txtAssyLotID5.Text = string.Empty;
                txtAssyLotID6.Text = string.Empty;
                txtAssyLotID7.Text = string.Empty;

                switch (txtAssyLotID.Text.Length)
                {
                    case 1:
                        txtAssyLotID5.Text = txtAssyLotID.Text.ToUpper();
                        break;
                    case 2:
                        txtAssyLotID5.Text = txtAssyLotID.Text.Substring(0, 1).ToUpper();
                        txtAssyLotID6.Text = txtAssyLotID.Text.Substring(1, 1).ToUpper();
                        break;
                    case 3:
                        txtAssyLotID5.Text = txtAssyLotID.Text.Substring(0, 1).ToUpper();
                        txtAssyLotID6.Text = txtAssyLotID.Text.Substring(1, 1).ToUpper();
                        txtAssyLotID7.Text = txtAssyLotID.Text.Substring(2, 1).ToUpper();
                        break;
                }
            }
            else if (_modTypeCode.Equals("B"))
            {
                txtAssyLotID1.Text = string.Empty;
                txtAssyLotID2.Text = string.Empty;
                txtAssyLotID3.Text = string.Empty;
                txtAssyLotID4.Text = string.Empty;
                txtAssyLotID5.Text = string.Empty;
                txtAssyLotID6.Text = string.Empty;
                txtAssyLotID7.Text = string.Empty;
                txtAssyLotID8.Text = string.Empty;

                if (txtAssyLotID.Text.Length > 0)
                    txtAssyLotID1.Text = txtAssyLotID.Text.Substring(0, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 1)
                    txtAssyLotID2.Text = txtAssyLotID.Text.Substring(1, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 2)
                    txtAssyLotID3.Text = txtAssyLotID.Text.Substring(2, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 3)
                    txtAssyLotID4.Text = txtAssyLotID.Text.Substring(3, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 4)
                    txtAssyLotID5.Text = txtAssyLotID.Text.Substring(4, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 5)
                    txtAssyLotID6.Text = txtAssyLotID.Text.Substring(5, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 6)
                    txtAssyLotID7.Text = txtAssyLotID.Text.Substring(6, 1).ToUpper();
                if (txtAssyLotID.Text.Length > 7)
                    txtAssyLotID8.Text = txtAssyLotID.Text.Substring(7, 1).ToUpper();
            }

        }
        #endregion

        #region [등급별 수량 입력 여부 cboUseYN_SelectedValueChanged]
        private void cboUseYN_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboUseYN.SelectedValue == null || cboUseYN.SelectedValue.Equals("Y"))
            {
                txtInboxQty.IsEnabled = false;
                dgInboxGrade.IsEnabled = true;
            }
            else
            {
                txtInboxQty.IsEnabled = true;
                dgInboxGrade.IsEnabled = false;
            }
        }
        #endregion

        #region [등급별 Cell수량 IsReadOnly 바탕색 처리]: dgInboxGrade_LoadedCellPresenter
        private void dgInboxGrade_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.Equals("INBOX_QTY"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = _inboxMaxQty;
                    (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                }

            }));

        }
        #endregion

        #region [Inbox 생성]
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate())
                return;

            // %1 (을)를 생성 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4584", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateProcess();
                }
            }, parameters);
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
        /// 설비 InBox 유형 조회
        /// </summary>
        private void SetInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Inbox Max 생성수 조회
        /// </summary>
        private void GeInboxMaxQty()
        {
            try
            {
                ShowLoadingIndicator();

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string)); ;

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "INBOX_MAX_PRT_QTY";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null || dtResult.Rows.Count > 0)
                {
                    if (!int.TryParse(dtResult.Rows[0]["ATTRIBUTE1"].ToString(), out _inboxMaxQty)) _inboxMaxQty = 0;
                }

                txtInboxQty.Maximum = _inboxMaxQty;
                txtInboxQty.Minimum = 0;
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
        /// 등급 조회
        /// </summary>
        private void SetInboxGrade()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _lineID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                dtResult.Columns.Add("INBOX_QTY", typeof(int));
                dtResult.AsEnumerable().ToList<DataRow>().ForEach(r => r["INBOX_QTY"] = 0);
                dtResult.AcceptChanges();

                Util.GridSetData(dgInboxGrade, dtResult, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Inbox 생성
        /// </summary>
        private void CreateProcess()
        {
            try
            {
                ShowLoadingIndicator();

                #region ### DataSet
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
                inTable.Columns.Add("MOD_FLAG", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("MIX_LOT_FLAG", typeof(string));
                inTable.Columns.Add("MIX_LOT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("LOAD_QTY", typeof(decimal));
                inTable.Columns.Add("INBOX_QTY", typeof(decimal));
                inTable.Columns.Add("MOD_TYPE_CODE", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inBox.Columns.Add("CELL_QTY", typeof(string));
                inBox.Columns.Add("INBOX_QTY", typeof(string));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                if (!String.IsNullOrEmpty(txtAommType.Text))
                {
                    inBox.Columns.Add("AOMM_GRD_CODE", typeof(string));
                }
                #endregion

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _eqptID;
                newRow["PROD_LOTID"] = Util.NVC(drProdLot["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["CTNR_ID"] = txtCartID.Text;
                newRow["VISL_INSP_USERID"] = InspectorCode;
                newRow["PROCID"] = _procID;
                newRow["MIX_LOT_FLAG"] = (bool)chkLotMix.IsChecked ? "Y" : "N";
                if ((bool)chkLotMix.IsChecked)
                {
                    newRow["MIX_LOT_TYPE_CODE"] = cboMixLotType.SelectedValue == null ? null : cboMixLotType.SelectedValue.ToString();
                }
                newRow["ASSY_LOTID"] = _assyLotID;
                newRow["MOD_TYPE_CODE"] = _modTypeCode;
                newRow["SHIPTO_ID"] = ShiptoID;
                inTable.Rows.Add(newRow);

                // Inbox 정보
                DataTable dtInboxType = DataTableConverter.Convert(cboInboxType.ItemsSource);

                int TotalInboxQty = 0;

                if (cboUseYN.SelectedValue == null || cboUseYN.SelectedValue.Equals("Y"))
                {
                    // 등급별 Inbox 생성
                    foreach (DataGridRow dRow in dgInboxGrade.Rows)
                    {
                        int InboxQty = Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY"));
                        if (InboxQty != 0)
                        {
                            for (int i = 0; i < InboxQty; i++)
                            {
                                TotalInboxQty++;

                                DataRow dr = inBox.NewRow();
                                dr["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CBO_CODE"));
                                dr["CELL_QTY"] = Util.NVC_Int(dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"]);
                                dr["INBOX_QTY"] = 1;
                                dr["INBOX_TYPE_CODE"] = Util.NVC(dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"]);
                                if (!String.IsNullOrEmpty(txtAommType.Text))
                                {
                                    dr["AOMM_GRD_CODE"] = txtAommType.Text;
                                }
                                inBox.Rows.Add(dr);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < txtInboxQty.Value.GetInt(); i++)
                    {
                        TotalInboxQty++;

                        DataRow dr = inBox.NewRow();
                        dr["CELL_QTY"] = Util.NVC_Int(dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"]);
                        dr["INBOX_QTY"] = 1;
                        dr["INBOX_TYPE_CODE"] = Util.NVC(dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"]);
                        if (!String.IsNullOrEmpty(txtAommType.Text))
                        {
                            dr["AOMM_GRD_CODE"] = txtAommType.Text;
                        }
                        inBox.Rows.Add(dr);
                    }
                }

                inTable.Rows[0]["LOAD_QTY"] = Util.NVC_Int(dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"]);
                inTable.Rows[0]["INBOX_QTY"] = TotalInboxQty;
                inTable.AcceptChanges();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_INBOX", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        //////Util.AlertInfo("정상 처리 되었습니다.");
                        ////Util.MessageInfo("SFU1889");

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

        private bool ValidationCreate()
        {
            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.GetString().Equals(""))
            {
                // 설비의 Inbox 유형을 설정 하세요.
                Util.MessageValidation("SFU4354");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_modTypeCode))
            {
                // 입력 조립 LOT 대문자로 변경
                _assyLotID = txtAssyLotID1.Text.Trim() +
                             txtAssyLotID2.Text.Trim() +
                             txtAssyLotID3.Text.Trim() +
                             txtAssyLotID4.Text.Trim() +
                             txtAssyLotID5.Text.Trim() +
                             txtAssyLotID6.Text.Trim() +
                             txtAssyLotID7.Text.Trim() +
                             txtAssyLotID8.Text.Trim();

                if (_assyLotID.Length != 8)
                {
                    // 조립LOT 정보는 8자리 입니다.
                    Util.MessageValidation("SFU4228");
                    return false;
                }

                // 불량 조립LOT인 경우만 체크
                if (_modTypeCode.Equals("D"))
                {
                    if (_assyLotID.Substring(5,2).Equals("00"))
                    {
                        // 조립LOT을 변경하세요.
                        Util.MessageValidation("SFU4908");
                        return false;
                    }
                }
                else if (_modTypeCode.Equals("M"))
                {
                    if (_assyLotID.Substring(4, 3).Equals("000"))
                    {
                        // 조립LOT을 변경하세요.
                        Util.MessageValidation("SFU4908");
                        return false;
                    }

                    // 월 체크 ABCDEFGHIJKL
                    string monthValue = "ABCDEFGHIJKL";
                    if (monthValue.IndexOf(_assyLotID.Substring(4,1)) < 0)
                    {
                        // 조립LOT의 월 입력은 ABCDEFGHIJKL 중 하나를 입력하세요.
                        Util.MessageValidation("SFU4907");
                        return false;
                    }
                }
                else
                {
                    // 포장공정이고 반품 재작업 (조립LOT 5자리 수정)
                    // 월 체크 ABCDEFGHIJKL
                    string monthValue = "ABCDEFGHIJKL";
                    if (monthValue.IndexOf(_assyLotID.Substring(4,1)) < 0)
                    {
                        // 조립LOT의 월 입력은 ABCDEFGHIJKL 중 하나를 입력하세요.
                        Util.MessageValidation("SFU4907");
                        return false;
                    }
                }

                int Day = 0;
                if (!int.TryParse(_assyLotID.Substring(5, 2), out Day))
                {
                    // 조립LOT 6,7 번쨰는 숫자만 입력가능합니다.
                    Util.MessageValidation("SFU4910");
                    return false;
                }

                if ((bool)chkLotMix.IsChecked)
                {
                    // Mix Lot은 40이상
                    if (Day < 40)
                    {
                        // 조립Lot Mix의 일자는 40 이상 입니다.
                        Util.MessageValidation("SFU4895");
                        return false;
                    }
                }
                else
                {
                    // 31까지만
                    if (Day > 31)
                    {
                        // 조립Lot 일자는 31 이하 입니다.
                        Util.MessageValidation("SFU4896");
                        return false;
                    }
                }
            }
            else
            {
                _assyLotID = Util.NVC(drProdLot["ASSY_LOTID"]);
            }

            if (cboUseYN.SelectedValue == null || cboUseYN.SelectedValue.Equals("Y"))
            {
                int CreateCount = DataTableConverter.Convert(dgInboxGrade.ItemsSource).AsEnumerable().Count(r => r.Field<int>("INBOX_QTY") > 0);

                if (CreateCount == 0)
                {
                    // 수량을 입력하세요.
                    Util.MessageValidation("SFU1684");
                    return false;
                }
            }
            else
            {
                if (txtInboxQty.Value == 0)
                {
                    // 수량을 입력하세요.
                    Util.MessageValidation("SFU1684");
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

    }
}