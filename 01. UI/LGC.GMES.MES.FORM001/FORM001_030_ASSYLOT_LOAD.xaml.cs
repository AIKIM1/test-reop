/*************************************************************************************
 Created Date : 2017.12.22
      Creator : 
   Decription : 파활성화 후공정 파우치 : 조립 LOT 적재
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_030_ASSYLOT_LOAD : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _processName = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _divisionCode = string.Empty;
        private string _ctnrId = string.Empty;
        private int _inboxMaxQty = 0;

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public string InspectorCode { get; set; }
        public string ProductLot { get; set; }
        public C1DataGrid DgAssyLot { get; set; }
        public C1DataGrid DgProductionInbox { get; set; }

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

        public FORM001_030_ASSYLOT_LOAD()
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
                _ctnrId = parameters[6] as string;

                if (_processName != null) txtProcess.Text = _processName;
                if (_equipmentName != null) txtEquipment.Text = _equipmentName;
                if (_ctnrId != null) txtCtnrID.Text = _ctnrId;

                InitializeUserControls();
                SetControl();
                SetControlVisibility();
                _load = false;
            }

            //txtStartTray.Focus();
        }

        private void InitializeUserControls()
        {
            Util.gridClear(dgLot);
            //Util.gridClear(dgInboxGrade);

            cboLottype.SelectedIndex = 0;
            cboMKTtype.SelectedIndex = 0;
            SetFormWorkType();
            cboAssyProdID.ItemsSource = null;
            cboAssyProdID.Text = string.Empty;
            cboProdID.ItemsSource = null;
            cboProdID.Text = string.Empty;

            txtAssyLotID.Text = string.Empty;
            txtAssyLotID.Focus();
        }

        private void SetControl()
        {
            // 작업구분 : SetFormWorkType => 재작업으로 셋팅
            CommonCombo combo = new CommonCombo();
            string[] sFilter = { LoginInfo.CFG_AREA_ID, _equipmentSegmentCode };
            combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE_LINE", sFilter: sFilter);
            SetFormWorkType();

            // Lot type
            SetLotType();
            cboLottype.SelectedValueChanged += cboLottype_SelectedValueChanged;

            // 시장유형
            string[] sFilterMKType = { "MKT_TYPE_CODE" };
            combo.SetCombo(cboMKTtype, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilterMKType);
            cboMKTtype.SelectedValueChanged += cboMKTtype_SelectedValueChanged;

            // Inbox type
            string[] sFilterInboxType = { LoginInfo.CFG_AREA_ID, _processCode };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilterInboxType);
            SetInboxType();

            // 등급별수량입력여부
            string[] sFilterUse = { "IUSE" };
            combo.SetCombo(cboUseYN, CommonCombo.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilterUse);
            cboUseYN.SelectedValueChanged += cboUseYN_SelectedValueChanged;

            SetGradeUseType();
            GeInboxMaxQty();
            // 등급 조회
            SetInboxGrade();
        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [조립LOTMix chkLotMix_Checked, chkLotMix_Unchecked]

        //MIX LOT 선택시 초기화

        private void chkLotMix_Checked(object sender, RoutedEventArgs e)
        {
            cboAssyProdID.IsEnabled = true;
            MixLotRt_ReSet();
        }

        private void chkLotMix_Unchecked(object sender, RoutedEventArgs e)
        {
            cboAssyProdID.IsEnabled = false;
            MixLotRt_ReSet();
        }
        #endregion

        #region [조립LOT제품 변경 cboAssyProdID_SelectedValueChanged]
        private void cboAssyProdID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetModelLotBomProd();
        }
        #endregion

        #region [조립 Lot 정보 조회]
        private void txtAssyLotID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationAssyLot())
                {
                    InitializeUserControls();
                    return;
                }
                GetAssyLotInfo(txtAssyLotID.Text);
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
                    (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = _inboxMaxQty;
                    (e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;

                    
                }

            }));

        }
        #endregion

        #region [작업구분 변경 cboFormWorkType_SelectedValueChanged]
        private void cboFormWorkType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLot.Rows.Count > 0)
            {
                if (cboFormWorkType.SelectedValue == null || Util.NVC(cboFormWorkType.SelectedValue).Equals("SELECT"))
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "FORM_WRK_TYPE_NAME",string.Empty);
                    return;
                }

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
                if (cboLottype.SelectedValue == null || Util.NVC(cboLottype.SelectedValue).Equals("SELECT"))
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTTYPE", string.Empty);
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTYNAME", string.Empty);
                    return;
                }

                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTTYPE", cboLottype.SelectedValue ?? cboLottype.SelectedValue.ToString());
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "LOTYNAME", cboLottype.Text);
            }

        }
        #endregion

        #region [제품 변경 cboProdID_SelectedValueChanged]
        private void cboProdID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (dgLot.Rows.Count > 0)
            {
                if (cboProdID.SelectedValue == null || Util.NVC(cboProdID.SelectedValue).Equals("SELECT"))
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", string.Empty);
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", string.Empty);
                    return;
                }

                DataTable dt = DataTableConverter.Convert(cboProdID.ItemsSource);

                if (dt == null || dt.Rows.Count == 0)
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", string.Empty);
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", string.Empty);
                }
                else
                {
                    string PrjName = dt.Rows[cboProdID.SelectedIndex]["PRJT_NAME"].ToString();

                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRJT_NAME", PrjName);
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "PRODID", cboProdID.SelectedValue ?? cboProdID.SelectedValue.ToString());
                }
            }
        }
        #endregion

        #region [시장유형 변경 cboMKTtype_SelectedValueChanged]  
        private void cboMKTtype_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLot.Rows.Count > 0)
            {
                if (cboMKTtype.SelectedValue == null || Util.NVC(cboMKTtype.SelectedValue).Equals("SELECT"))
                {
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "MKT_TYPE_CODE", string.Empty);
                    DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "MKT_TYPE_NAME", string.Empty);
                    return;
                }

                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "MKT_TYPE_CODE", cboMKTtype.SelectedValue ?? cboMKTtype.SelectedValue.ToString());
                DataTableConverter.SetValue(dgLot.Rows[0].DataItem, "MKT_TYPE_NAME", cboMKTtype.Text);
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

        #region [대차적재]
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            dgInboxGrade.EndEdit();
            dgInboxGrade.EndEditRow(true);

            if (!ValidateLoad())
                return;

            // 조립lot을 대차 적재 하시겠습니까?
            Util.MessageConfirm("SFU4373", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LoadProcess();
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
                newRow["EQPTID"] = _equipmentCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE_CODE"].ToString();
                }

                if (cboInboxType.SelectedValue == null)
                    cboInboxType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                newRow["EQSGID"] = _equipmentSegmentCode;
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
        /// Lot 정보 조회
        /// </summary>
        private void GetAssyLotInfo(string AssyLotID)
        {
            try
            {
                ShowLoadingIndicator();

                // Clear
                InitializeUserControls();

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["LOTID"] = AssyLotID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_ASSY_LOT_INFO_TOP_PC", "INDATA", "OUTDATA", inTable);

                txtAssyLotID.Text = AssyLotID;
                Util.GridSetData(dgLot, dtResult, null, true);

                SetdgLot(dtResult);
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
        /// 제품 콤보
        /// </summary>
        private DataTable GeProdID()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtResult = new DataTable();
                DataTable inTable = new DataTable();
                DataRow newRow;

                if ((bool)chkLotMix.IsChecked)
                {
                    if (dgLot.Rows.Count > 0 &&
                        !string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"))))
                    {
                        // 조회된 제품이 있다
                        dtResult.Columns.Add("CBO_CODE", typeof(string));
                        dtResult.Columns.Add("CBO_NAME", typeof(string));
                        dtResult.Columns.Add("PRJT_NAME", typeof(string));

                        newRow = dtResult.NewRow();
                        newRow["CBO_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"));
                        newRow["CBO_NAME"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID"));
                        newRow["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRJT_NAME"));
                        dtResult.Rows.Add(newRow);
                    }
                    else
                    {
                        // 없는경우 모델LOT으로 조회
                        inTable.Columns.Add("LOTID", typeof(string));

                        newRow = inTable.NewRow();
                        newRow["LOTID"] = txtAssyLotID.Text;
                        inTable.Rows.Add(newRow);

                        dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_MDLLOT_PC", "INDATA", "OUTDATA", inTable);
                    }
                }
                else
                {
                    inTable.Columns.Add("PROCID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));

                    newRow = inTable.NewRow();
                    newRow["PROCID"] = _processCode;
                    newRow["LOTID"] = txtAssyLotID.Text;
                    inTable.Rows.Add(newRow);

                    dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_PC", "INDATA", "OUTDATA", inTable);
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// <summary>
        /// 모델LOT 콤보 변경시 해당 제픔으로 BOM 제품 조회
        /// </summary>
        private void SetModelLotBomProd()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _processCode;
                newRow["PRODID"] = Util.NVC(cboAssyProdID.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_MDLLOT_BOM_PC", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr[cboLottype.DisplayMemberPath.ToString()] = "-SELECT-";
                dr[cboLottype.SelectedValuePath.ToString()] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboProdID.DisplayMemberPath = cboLottype.DisplayMemberPath.ToString();
                cboProdID.SelectedValuePath = cboLottype.SelectedValuePath.ToString();
                cboProdID.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count == 2)
                    cboProdID.SelectedIndex = 1;
                else
                    cboProdID.SelectedIndex = 0;
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


        private void LoadProcess()
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
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
                inTable.Columns.Add("ASSY_FLAG", typeof(string));
                inTable.Columns.Add("ASSY_PRODID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("GRD_CODE", typeof(string));
                inBox.Columns.Add("INBOX_QTY", typeof(string));
                inBox.Columns.Add("INBOX_TYPE_QTY", typeof(string));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                #endregion

                DataTable dtLot = DataTableConverter.Convert(dgLot.ItemsSource);

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["ASSY_LOTID"] = Util.NVC(dtLot.Rows[0]["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["FORM_WRK_TYPE_CODE"] = Util.NVC(dtLot.Rows[0]["FORM_WRK_TYPE_CODE"]);
                newRow["PRODID"] = Util.NVC(cboProdID.SelectedValue);
                newRow["LOTTYPE"] = Util.NVC(dtLot.Rows[0]["LOTTYPE"]);
                newRow["MKT_TYPE_CODE"] = Util.NVC(dtLot.Rows[0]["MKT_TYPE_CODE"]);
                newRow["CTNR_ID"] = _ctnrId;
                newRow["SHIFT"] = ShiftID;
                newRow["WRK_USERID"] = WorkerID;
                newRow["WRK_USER_NAME"] = WorkerName;
                newRow["VISL_INSP_USERID"] = InspectorCode;
                newRow["ASSY_FLAG"] = (bool)chkLotMix.IsChecked ? "Y" : "N";
                newRow["ASSY_PRODID"] = Util.NVC(cboAssyProdID.SelectedValue);
                inTable.Rows.Add(newRow);

                // Inbox 정보
                DataTable dtInboxType = DataTableConverter.Convert(cboInboxType.ItemsSource);

                if (cboUseYN.SelectedValue == null || cboUseYN.SelectedValue.Equals("Y"))
                {
                    foreach (DataGridRow dRow in dgInboxGrade.Rows)
                    {
                        if (Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY")) != 0)
                        {
                            newRow = inBox.NewRow();
                            newRow["GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CBO_CODE"));
                            newRow["INBOX_QTY"] = Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY"));
                            newRow["INBOX_TYPE_QTY"] = Util.NVC_Int(dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"]);
                            newRow["INBOX_TYPE_CODE"] = Util.NVC(dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"]);
                            inBox.Rows.Add(newRow);
                        }
                    }
                }
                else
                {
                    newRow = inBox.NewRow();
                    newRow["INBOX_QTY"] = txtInboxQty.Value;
                    newRow["INBOX_TYPE_QTY"] = Util.NVC_Int(dtInboxType.Rows[cboInboxType.SelectedIndex]["INBOX_LOAD_QTY"]);
                    newRow["INBOX_TYPE_CODE"] = Util.NVC(dtInboxType.Rows[cboInboxType.SelectedIndex]["CBO_CODE"]);
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_WITH_ASSY", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        ProductLot = bizResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();

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

        #region[[Validation]
        private bool ValidationAssyLot()
        {
            if (string.IsNullOrWhiteSpace(txtAssyLotID.Text))
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            if (txtAssyLotID.Text.Length < 8)
            {
                // 조립LOTID는 8자리 이상입니다.
                Util.MessageValidation("SFU4075");
                return false;
            }

            DataTable dt = DataTableConverter.Convert(DgAssyLot.ItemsSource);
            DataRow[] dr = dt.Select("ASSY_LOTID = '" + txtAssyLotID.Text + "'");

            if (dr.Length > 0)
            {
                // 중복된 조립LOT 정보가 존재합니다.
                Util.MessageValidation("SFU4226");
                return false;
            }

            return true;
        }

        private bool ValidateLoad()
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

            if (cboLottype.SelectedValue == null || cboLottype.SelectedValue.GetString().Equals("SELECT"))
            {
                // LOT 유형을 선택하세요.
                Util.MessageValidation("SFU4068");
                return false;
            }

            if ((bool)chkLotMix.IsChecked)
            {
                if (cboAssyProdID.SelectedValue == null || cboAssyProdID.SelectedValue.GetString().Equals("SELECT"))
                {
                    // 조립LOT Mix시 해당 조립LOT 제품을 선택하세요.
                    Util.MessageValidation("SFU4463");
                    return false;
                }
            }

            if (cboProdID.SelectedValue == null || cboProdID.SelectedValue.GetString().Equals("SELECT"))
            {
                // 제품을 선택하세요.
                Util.MessageValidation("SFU1895");
                return false;
            }

            if (cboMKTtype.SelectedValue == null || cboMKTtype.SelectedValue.GetString().Equals("SELECT"))
            {
                // 시장유형을 선택하세요.
                Util.MessageValidation("SFU4371");
                return false;
            }

            if (cboInboxType.SelectedValue == null || cboInboxType.SelectedValue.GetString().Equals("SELECT"))
            {
                // InBox 유형을 선택해 주세요.
                Util.MessageValidation("SFU4005");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID"))))
            {
                // 조립 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4001");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void SetGradeUseType()
        {
            string UseSetting = string.Empty;
            bool IsFirst;

            DataTable dt = DataTableConverter.Convert(DgProductionInbox.ItemsSource);

            if (dt != null && dt.Rows.Count > 0)
            {
                IsFirst = false;
                if (string.IsNullOrWhiteSpace(Util.NVC(dt.Rows[0]["CAPA_GRD_CODE"])))
                {
                    UseSetting = "N";
                }
                else
                {
                    UseSetting = "Y";
                }
            }
            else
            {
                IsFirst = true;
                UseSetting = "Y";
            }

            // 등급별 수량 입력여부
            cboUseYN.SelectedValue = UseSetting;

            if (UseSetting.Equals("Y"))
            {
                txtInboxQty.IsEnabled = false;
                dgInboxGrade.IsEnabled = true;
            }
            else
            {
                txtInboxQty.IsEnabled = true;
                dgInboxGrade.IsEnabled = false;
            }

            if (!IsFirst)
            {
                cboUseYN.IsEnabled = false;
            }

        }

        private void SetFormWorkType()
        {
            // 재작업으로 작업구분 Setting
            DataTable dtFormWorkType = DataTableConverter.Convert(cboFormWorkType.ItemsSource);

            if (dtFormWorkType.Rows.Count > 0)
            {
                DataRow[] drIndex = dtFormWorkType.Select("CBO_CODE ='FORM_WORK_RW'");
                int index = 0;

                if (drIndex.Length > 0)
                {
                    index = dtFormWorkType.Rows.IndexOf(drIndex[0]);
                    cboFormWorkType.SelectedValue = "R";
                }

                cboFormWorkType.SelectedIndex = index;
            }
        }

        private void SetProdID(C1ComboBox cb)
        {
            DataTable dtProd = GeProdID();

            DataRow drProd = dtProd.NewRow();
            drProd[cboProdID.SelectedValuePath.ToString()] = "SELECT";
            drProd[cboProdID.DisplayMemberPath.ToString()] = "- SELECT -";
            dtProd.Rows.InsertAt(drProd, 0);

            cb.ItemsSource = null;
            cb.ItemsSource = dtProd.Copy().AsDataView();

            if (dtProd.Rows.Count == 2)
                cb.SelectedIndex = 1;
            else
                cb.SelectedIndex = 0;
        }

        private void SetdgLot(DataTable dt)
        {
            // 재작업으로 셋팅
            SetFormWorkType();

            if ((bool)chkLotMix.IsChecked)
            {
                cboAssyProdID.SelectedValueChanged -= cboAssyProdID_SelectedValueChanged;
                SetProdID(cboAssyProdID);
                cboAssyProdID.SelectedValueChanged += cboAssyProdID_SelectedValueChanged;
                SetModelLotBomProd();
            }
            else
            {
                SetProdID(cboProdID);
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                // 셋팅된 작업구분
                if (cboFormWorkType.Items.Count > 0)
                {
                    dt.Rows[0]["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue ?? "";

                    string[] FromWorkTypeSplit = cboFormWorkType.Text.Split(':');
                    if (FromWorkTypeSplit.Length > 1)
                        dt.Rows[0]["FORM_WRK_TYPE_NAME"] = FromWorkTypeSplit[1];

                    dt.AcceptChanges();
                }

                //// 제품 콤보 셋팅
                //SetProdID(cboProdID);
                ////cboProdID.SelectedValue = dt.Rows[0]["PRODID"].ToString();

                // 조회된 Lot 유형으로 Combo Setting
                if (string.IsNullOrWhiteSpace(dt.Rows[0]["LOTTYPE"].ToString()))
                    cboLottype.SelectedIndex = 0;
                else
                    cboLottype.SelectedValue = dt.Rows[0]["LOTTYPE"].ToString();

                // 조회된 시장 유형으로 Combo Setting
                if (string.IsNullOrWhiteSpace(dt.Rows[0]["MKT_TYPE_CODE"].ToString()))
                    cboMKTtype.SelectedIndex = 0;
                else
                    cboMKTtype.SelectedValue = dt.Rows[0]["MKT_TYPE_CODE"].ToString();

                cboFormWorkType.IsEnabled = false;
                cboLottype.IsEnabled = false;
                //cboProdID.IsEnabled = false;
                cboMKTtype.IsEnabled = false;

                Util.GridSetData(dgLot, dt, null, true);
            }
            else
            {
                //// 제품 콤보 셋팅
                //SetProdID(cboAssyProdID);

                if (cboProdID != null && cboProdID.Items.Count > 0)
                {
                    DataRow drins = dt.NewRow();
                    drins["LOTID"] = txtAssyLotID.Text;
                    drins["LOTID_RT"] = txtAssyLotID.Text.Substring(0,8);

                    if (cboFormWorkType.Items.Count > 0)
                    {
                        drins["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue ?? "";

                        string[] FromWorkTypeSplit = cboFormWorkType.Text.Split(':');
                        if (FromWorkTypeSplit.Length > 1)
                            drins["FORM_WRK_TYPE_NAME"] = FromWorkTypeSplit[1];
                    }
                    dt.Rows.Add(drins);

                    cboFormWorkType.IsEnabled = true;
                    cboLottype.IsEnabled = true;
                    //cboProdID.IsEnabled = true;
                    cboMKTtype.IsEnabled = true;

                    Util.GridSetData(dgLot, dt, null, true);
                }
                else
                {
                    // Model LOT 코드에 해당하는 제품코드가 없습니다.
                    Util.MessageValidation("SFU4370");
                    txtAssyLotID.Focus();
                }
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

        // MIX 조립LOT 선택시 초기화 기능 추가
        private void MixLotRt_ReSet()
        {
            //조립LOT 초기화
            txtAssyLotID.Text = string.Empty;
            txtAssyLotID.Focus();
          
            //LOTTYPE 초기화 
            cboLottype.SelectedIndex = 0;
            cboLottype.IsEnabled = true;
            
            //시장유형 초기화
            cboMKTtype.SelectedIndex = 0;
            cboMKTtype.IsEnabled = true;

            //작업구분 초기화 
            cboFormWorkType.IsEnabled = true;

            //LOT 정보 초기화
            Util.gridClear(dgLot);

            //조립LOT제품 초기화
            cboAssyProdID.ItemsSource = null;
            cboAssyProdID.Text = string.Empty;

            //제품코드 초기화
            cboProdID.ItemsSource = null;
            cboProdID.Text = string.Empty;
        }



        #endregion

        #endregion







    }
}
