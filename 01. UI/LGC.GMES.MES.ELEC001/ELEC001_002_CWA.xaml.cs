/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 믹서원자재 투입요청서
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.25  김태우    : DAM 믹서(E0430) 추가
   
 [미적용 사항]
     1. 화면 종료 시 확인정보 Table Update 처리 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_002_CWA : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        CommonCombo _combo = new CommonCombo();
        string BatchOrdID = string.Empty;
        string WO_ID = string.Empty;
        string ReqID = string.Empty;
        string REQ_DTTM = string.Empty;
        string _EQPTID = string.Empty;
        string _HopperID = "";
        string _MtrlID = "";
        string _ProdID = string.Empty;
        string _ProcChkEqptID = string.Empty;
        DataSet inDataSet = null;
        DataTable IndataTable = null;
        DataTable ReqPrintTag = new DataTable();
        DataTable ReqPrintTagDetl = new DataTable();
        public DataTable dtColumnLang;
        DataTable ChkKeepData = new DataTable();
        bool HopperFlag = false;
        bool InputFlag = false;
        Util _Util = new Util();
        bool DETAIL_POPUP = false;
        bool HopperMtrlFlag = false;
        int Select = 0;
        private System.Windows.Threading.DispatcherTimer _Timer = new System.Windows.Threading.DispatcherTimer();
        private SoundPlayer player;

        public ELEC001_002_CWA()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public string VersionCheckFlag = string.Empty;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            //WPF 는 컨트롤 초기화를 Loaded 이벤트에서 해야합니다
            //가끔 컨트롤이 정상 초기화 되지 않을 경우가 있습니다
            InitControls();
            InitCombo();
            ApplyPermissions();
            ReqForceAreaCheck();
            //Set_Combo_BatchCode(cboBatchCode);

            _Timer.Tick += dispatcherTimer_Tick;
            _Timer.Interval = TimeSpan.FromSeconds(10);
        }
        
        #endregion


        #region Initialize

        public string PRODID { get; set; }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDelete);
            listAuth.Add(btnRequest);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControls()
        {

            //ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
            //ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ldpDate();
            dtpDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            
        }

        private void ReqForceAreaCheck()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INPUT_REQ_FORCE_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    btnReqEnd.IsEnabled = true;
            }
            catch (Exception ex) { }
        }

        private void ldpDate()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    //Util.MessageException(Exception);
                    return;
                }
                ldpDateFrom.Text = result.Rows[0][0].ToString();
                ldpDateTo.Text = result.Rows[0][0].ToString();
            }
            );
        }


        private void InitCombo()
        {
            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment, EQUIPMENT };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment, EQUIPMENT };

            //ESNB에서도 본 메뉴를 사용 중인데 절연액 공정도 나올 수 있도록 해달라는 요청사항이 발생
            //ProcessCWA는 TOP4만 조회하여 절연액이 조회되지 않고 있음
            if (LoginInfo.CFG_AREA_ID.Equals("EC"))
            {
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "MixingProcess");
            }
            else
            {
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "ProcessCWA");
            }

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);
            _combo.SetCombo(EQUIPMENT, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);
        }

        private void Set_Combo_BatchCode(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["CMCDTYPE"] = "BTCH_LINK_CODE";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_COMMONCODE", Exception.Message, Exception.ToString());
                    return;
                }
                DataRow dRow = result.NewRow();
                dRow["CBO_NAME"] = "-SELECT-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);
                cbo.SelectedIndex = 0;
            }
            );
        }
        
        #endregion


        #region Event

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty && Util.NVC(cboProcess.SelectedValue) != string.Empty)
            {
                if (Util.NVC(cboProcess.SelectedValue).Equals(Process.MIXING) || Util.NVC(cboProcess.SelectedValue).Equals(Process.SRS_MIXING) || Util.NVC(cboProcess.SelectedValue).Equals(Process.PRE_MIXING))
                {
                    // HOPPER MTRL USE FLAG 갱신 처리 [2018-03-21]
                    IsHopperMtrlChk();
                }

                if(Util.NVC(cboProcess.SelectedValue).Equals(Process.BS) || Util.NVC(cboProcess.SelectedValue).Equals(Process.CMC))
                {
                    chkProc.IsChecked = true;
                }
                else
                {
                    chkProc.IsChecked = false;
                }
                txtDetailRemark.Clear();
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipment.SelectedValue) != string.Empty && Util.NVC(cboEquipment.SelectedValue) != "-SELECT-")
            {
                _EQPTID = Util.NVC(cboEquipment.SelectedValue.ToString().Trim());
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);

                GetVersionCheckFlag();
            }
        }

        private void btnCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            ELEC001_002_INPUT_INFO _ReqDetail = new ELEC001_002_INPUT_INFO();
            _ReqDetail.FrameOperation = FrameOperation;
            if (_ReqDetail != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _EQPTID;
                C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                _ReqDetail.Closed += new EventHandler(InputInfo_Closed);
                _ReqDetail.ShowModal();
                _ReqDetail.CenterOnScreen();
            }
        }

        private void btnRingblower_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            ELEC001_002_INPUT_CHK _InputCheck = new ELEC001_002_INPUT_CHK();
            _InputCheck.FrameOperation = FrameOperation;
            if (_InputCheck != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _EQPTID;
                C1WindowExtension.SetParameters(_InputCheck, Parameters);

                _InputCheck.Closed += new EventHandler(InputCheck_Closed);
                _InputCheck.ShowModal();
                _InputCheck.CenterOnScreen();
            }
        }

        private void dgWorkOrder_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e) //C1.WPF.DataGrid.DataGridMergingCellsEventArgs e, DataGridCellEventArgs a
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgWorkOrder.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgWorkOrder.GetRowCount(); i++)
                {
                    if (Util.NVC(dgWorkOrder.GetCell(x, dgWorkOrder.Columns["BTCH_ORD_ID"].Index).Value) == Util.NVC(dgWorkOrder.GetCell(i, dgWorkOrder.Columns["BTCH_ORD_ID"].Index).Value))
                    {
                        x1 = i;
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)0), dgWorkOrder.GetCell((int)x1, (int)0)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)2), dgWorkOrder.GetCell((int)x1, (int)2)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)3), dgWorkOrder.GetCell((int)x1, (int)3)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)4), dgWorkOrder.GetCell((int)x1, (int)4)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)5), dgWorkOrder.GetCell((int)x1, (int)5)));
                        x = x1 + 1;
                        i = x1;
                    }
                }
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)0), dgWorkOrder.GetCell((int)x1, (int)0)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)2), dgWorkOrder.GetCell((int)x1, (int)2)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)3), dgWorkOrder.GetCell((int)x1, (int)3)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)4), dgWorkOrder.GetCell((int)x1, (int)4)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)5), dgWorkOrder.GetCell((int)x1, (int)5)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count == 0)
            {
                Util.MessageValidation("SFU2016");   //해당 라인에 설비가 존재하지 않습니다.
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            if(cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }

            GetWorkOrder();//좌측상단 작업지시 리스트 조회
            GetEqptWrkInfo();//우측상단 작업자 조회
            btnSearchList.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearchList));//좌측하단 투입요청서 리스트 조회
        }

        private void btnSearchList_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipment.Items.Count == 0)
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return;
            //}
            //if (cboEquipment.Text.Equals("-SELECT-"))
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요
            //    return;
            //}

            GetRequestList();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DelRequestList();
        }

        private void btnReqEnd_Click(object sender, RoutedEventArgs e)
        {
            ReqForceEnd();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!chkRequest.IsChecked.Value)
            {
                DETAIL_POPUP = true;
            }
            if (DETAIL_POPUP)
            {
                this.PlaySound();
                return;
            }
            if (!string.IsNullOrEmpty(GetRequestCount()))
            {
                ELEC001_002_DETAIL_CWA _ReqDetail = new ELEC001_002_DETAIL_CWA();
                _ReqDetail.FrameOperation = FrameOperation;

                if (_ReqDetail != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = GetRequestCount();
                    //Parameters[1] = _EQPTID;
                    C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                    _ReqDetail.Closed += new EventHandler(Detail_Closed);
                    _ReqDetail.ShowModal();
                    _ReqDetail.CenterOnScreen();

                    DETAIL_POPUP = true;
                    this.PlaySound();
                }
            }
        }        

        private void PlaySound()
        {
            player = new System.Media.SoundPlayer(LGC.GMES.MES.ELEC001.Properties.Resources.InputBois);
            this.player.Play();
        }

        private void chkRequest_Checked(object sender, RoutedEventArgs e)
        {
            _Timer.Tick += dispatcherTimer_Tick;
            _Timer.Interval = TimeSpan.FromSeconds(10);
            _Timer.Start();
        }

        private void chkRequest_Unchecked(object sender, RoutedEventArgs e)
        {
            _Timer.Stop();
            DETAIL_POPUP = false;
            _Timer.Tick -= dispatcherTimer_Tick;
        }

        private void dgProductLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));

                if (bchk == true)
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = false;
                        }
                    }
                    /*
                    if (e.Column is C1.WPF.DataGrid.DataGridComboBoxColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "HOPPER_ID")
                        {
                            e.Cancel = false;

                        }
                    }
                    */
                }
                else
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    /*
                    if (e.Column is C1.WPF.DataGrid.DataGridComboBoxColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "HOPPER_ID")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    */
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.ItemsSource == null || dgProductLot.Rows.Count == 0)
                    return;

                if (dgProductLot.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;

                if (DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "CHK").ToString().Equals("1"))
                {
                    dgProductLot.SelectedIndex = rowIndex;

                    // HOPPER 체크 시 ITEM 갱신되도록 추가 [2018-03-23]
                    C1.WPF.DataGrid.DataGridCell gridCell = dgProductLot.GetCell(rowIndex, dgProductLot.Columns["HOPPER_ID"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;

                        if (combo != null)
                            SetGridCboItem(combo, _EQPTID, Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MTRLID")));
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkMtrl_Checked(object sender, RoutedEventArgs e)
        {
            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count == 0)
                return;

            if (dgWorkOrder.CurrentRow.DataItem == null)
                return;
            if (BatchOrdID == null || BatchOrdID == "")
                return;
            //btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")));
        }

        private void chkMtrl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count == 0)
                return;

            if (dgWorkOrder.CurrentRow.DataItem == null)
                return;
            if (BatchOrdID == null || BatchOrdID == "")
                return;
            //btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")));
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    Select = idx;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                    //row 색 바꾸기
                    dgWorkOrder.SelectedIndex = idx;
                    BatchOrdID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "BTCH_ORD_ID"));
                    WO_ID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID"));
                    _ProdID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                    _ProcChkEqptID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EQPTID"));


                    if (VersionCheckFlag.Equals("Y"))
                    {
                        string Version = Util.NVC(dtRow["PROD_VER_CODE"]);

                        if (Version.Equals(string.Empty))
                        {
                            Util.gridClear(dgProductLot);
                            Util.MessageValidation("SFU5036");
                            return;
                        }
                    }

                    GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.Rows.Count < 1 || dgProductLot.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1833");  //자재정보가 없습니다.
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace((string)txtWorker.Tag))
                {
                    // 요청자를 선택해 주세요.
                    Util.MessageValidation("SFU3467");
                    return;
                }

                dgProductLot.EndEdit();
                inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                inDataTable.Columns.Add("CMC_BINDER_FLAG", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["WOID"] = WO_ID;
                inDataRow["NOTE"] = Util.NVC(txtRemark.Text.ToString());
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["REQ_USERID"] = (string)txtWorker.Tag;
                inDataRow["BTCH_ORD_ID"] = BatchOrdID;
                inDataRow["CMC_BINDER_FLAG"] = this.chkMtrl.IsChecked == true ? "Y" : "N";
                inDataTable.Rows.Add(inDataRow);

                DataTable inMtrlid = _Mtrlid();
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }
                if (!InputFlag)
                {
                    return;
                }
                if (!HopperFlag)
                {
                    Util.MessageValidation("SFU2035");  //호퍼를 선택하세요.
                    return;
                }

                //투입요청 하시겠습니까?
                Util.MessageConfirm("SFU1974", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", "INDATA,INDTLDATA", null, (result, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", ex.Message, ex.ToString());
                                    return;
                                }
                                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                                dgProductLot.EndEdit(true);
                                dgWorkOrder.EndEdit(true);
                                btnSearchList.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearchList));
                                //Util.gridClear(dgProductLot);
                                ClearGrid();
                                this.txtRemark.Clear();
                                this.txtWorker.Clear();
                                this.txtWorker.Tag = string.Empty;
                                //dgProductLot.ItemsSource = DataTableConverter.Convert(ChkKeepData);
                            }
                            catch (Exception ErrEx)
                            {
                                Util.MessageException(ErrEx);
                            }
                        }, inDataSet);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ClearGrid()
        {
            try
            {
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {                    
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "MTRL_QTY", string.Empty);
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "HOPPER_ID", "");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }

        }

        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgRequestList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }
                dtColumnLang = new DataTable();

                dtColumnLang.Columns.Add("믹서투입요청서", typeof(string));
                dtColumnLang.Columns.Add("담당자", typeof(string));
                dtColumnLang.Columns.Add("요청서번호", typeof(string));
                dtColumnLang.Columns.Add("요청이력", typeof(string));
                dtColumnLang.Columns.Add("자재상세정보", typeof(string));
                dtColumnLang.Columns.Add("요청일자", typeof(string));
                dtColumnLang.Columns.Add("프로젝트명", typeof(string));
                dtColumnLang.Columns.Add("요청작업자", typeof(string));
                dtColumnLang.Columns.Add("요청장비", typeof(string));
                dtColumnLang.Columns.Add("특이사항", typeof(string));
                dtColumnLang.Columns.Add("자재", typeof(string));
                dtColumnLang.Columns.Add("자재규격", typeof(string));
                dtColumnLang.Columns.Add("요청중량", typeof(string));
                dtColumnLang.Columns.Add("호퍼", typeof(string));

                DataRow drCrad = null;

                drCrad = dtColumnLang.NewRow();

                drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("믹서투입요청서"),
                                                  ObjectDic.Instance.GetObjectName("담당자"),
                                                  ObjectDic.Instance.GetObjectName("요청서번호"),
                                                  ObjectDic.Instance.GetObjectName("요청이력"),
                                                  ObjectDic.Instance.GetObjectName("자재상세정보"),
                                                  ObjectDic.Instance.GetObjectName("요청일자"),
                                                  ObjectDic.Instance.GetObjectName("프로젝트명"),
                                                  ObjectDic.Instance.GetObjectName("요청작업자"),
                                                  ObjectDic.Instance.GetObjectName("요청장비"),
                                                  ObjectDic.Instance.GetObjectName("특이사항"),
                                                  ObjectDic.Instance.GetObjectName("자재"),
                                                  ObjectDic.Instance.GetObjectName("자재규격"),
                                                  ObjectDic.Instance.GetObjectName("요청중량"),
                                                  ObjectDic.Instance.GetObjectName("호퍼")
                                               };

                dtColumnLang.Rows.Add(drCrad);

                ReqPrintTagDetl = new DataTable();
                ReqPrintTagDetl = CreateReqDetlPrint(ReqPrintTagDetl);

                LGC.GMES.MES.ELEC001.Report_Multi rs = new LGC.GMES.MES.ELEC001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[4];
                    Parameters[0] = "MReqList_Print";
                    Parameters[1] = ReqPrintTag;
                    Parameters[2] = ReqPrintTagDetl;
                    Parameters[3] = dtColumnLang;

                    C1WindowExtension.SetParameters(rs, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgProductLot_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            /*
            if (dgProductLot.CurrentRow == null || dgProductLot.SelectedIndex == -1)
            {
                return;
            }
            try
            {
                if (e.Cell.Column.Name.Equals("HOPPER_ID"))
                {
                    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                    if (dg.GetRowCount() == 0) return;

                    DataTable dtTmp = DataTableConverter.Convert(dg.ItemsSource);
                    _HopperID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID"));
                    _MtrlID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "MTRLID"));

                    //같은 자재, 같은 호퍼 투입 불가능
                    for (int icnt = 0; icnt < dgProductLot.Rows.Count; icnt++)
                    {
                        if (icnt != dgProductLot.SelectedIndex) //현재 선택한 row 제외
                        {
                            if (dtTmp.Rows[icnt]["HOPPER_ID"].ToString() == _HopperID)//dtTmp.Rows[icnt]["MTRLID"].ToString() == _MtrlID && //자재상관없이 같은 호퍼는 선택 불가능
                            {
                                //같은 호퍼ID를 선택하셨습니다.
                                Util.MessageInfo("SFU2852", (result) =>
                                {
                                    if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                    {
                                        DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID", "");
                                    }
                                });
                                return;
                            }
                        }
                    }

                    if (FinalInputMtrlCheck(_HopperID, _MtrlID))
                    {
                        //이미 투입요청된 자재입니다. \r\n다시 요청하시겠습니까? -> 해당 HOPPER에 잔량이 존재합니다. 초기화하고 다시 투입하시겠습니까?
                        Util.MessageConfirm("SFU1778", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "Y");

                                // DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK", false);
                                // CANCEL시 초기화 하는 로직 제거
                                //DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID", "");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "N");
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            */
        }

        private void dgReqListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgRequestList.SelectedIndex = idx;
                GetRequestDetail(Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_ID")));
                ReqID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_ID"));

                //요청서 프린트 
                ReqPrintTag = new DataTable();
                ReqPrintTag = CreateReqPrint(ReqPrintTag);
                ReqPrintTag.Rows[0]["REQ_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_DTTM"));
                ReqPrintTag.Rows[0]["REQ_ID"] = ReqID;
                ReqPrintTag.Rows[0]["REQ_ID1"] = ReqID;
                ReqPrintTag.Rows[0]["USERNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "USERNAME"));
                ReqPrintTag.Rows[0]["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "NOTE"));
                txtDetailRemark.Text = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "NOTE"));
                ReqPrintTag.Rows[0]["EPQTID"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "EQPTNAME"));
                ReqPrintTag.Rows[0]["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "PRJT_NAME"));
            }
        }

        private void Detail_Closed(object sender, EventArgs e)
        {
            ELEC001_002_DETAIL_CWA window = sender as ELEC001_002_DETAIL_CWA;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
                _Timer.Start();
                DETAIL_POPUP = false;
                this.player.Stop();
            }
        }

        private void InputInfo_Closed(object sender, EventArgs e)
        {
            ELEC001_002_INPUT_INFO window = sender as ELEC001_002_INPUT_INFO;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
            }
        }

        private void InputCheck_Closed(object sender, EventArgs e)
        {
            ELEC001_002_INPUT_CHK window = sender as ELEC001_002_INPUT_CHK;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
            }
        }

        private bool FinalInputMtrlCheck(string HopperID, string MtrlID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                //IndataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                //IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["HOPPER_ID"] = HopperID;
                Indata["PRODID"] = _ProdID;
                //Indata["BTCH_ORD_ID"] = BatchOrdID;
                //Indata["MTRLID"] = MtrlID;
                IndataTable.Rows.Add(Indata);

                //DA_BAS_SEL_TB_SFC_MIXER_FINL_INPUT_MTRL -> BR_PRD_CHK_FINL_INPUT_MTRL
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_FINL_INPUT_MTRL", "INDATA", "OUTDATA", IndataTable);

                if (dtMain.Rows[0]["CHK_FLAG"].ToString() == "N")
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "N");
                    return false;
                }
                else
                {
                    //DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "Y");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        // (C20200724-000246) 원재료 오투입 방지를 위한 알람팝업 적용
        private bool IsFinalMaterialCheck(string HopperID, string MtrlID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["HOPPER_ID"] = HopperID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_MIXER_FINL_INPUT_MTRL", "INDATA", "RSLTDT", IndataTable);

                //투입이력 없음
                if (result.Rows.Count == 0)
                    return true;
                //동일투입자재
                foreach (DataRow row in result.Rows)
                    if (string.Equals(MtrlID, row["MTRLID"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }
        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_TERM_CANCEL_PREMIX wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_TERM_CANCEL_PREMIX();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                C1WindowExtension.SetParameters(wndPopup, null);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "HOPPER_ID"))
                                {
                                    C1ComboBox combo = e.Cell.Presenter.Content as C1ComboBox;
                                    if (combo != null)
                                    {
                                        combo.IsDropDownOpenChanged -= dgProductLot_DropDownOpenChanged;
                                        combo.IsDropDownOpenChanged += dgProductLot_DropDownOpenChanged;

                                        combo.SelectionCommitted -= dgProductLot_SelectedCommited;
                                        combo.SelectionCommitted += dgProductLot_SelectedCommited;
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void dgProductLot_DropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue == true)
            {
                C1ComboBox combo = sender as C1ComboBox;
                if (combo != null)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgProductLot.Rows[((DataGridCellPresenter)combo.Parent).Cell.Row.Index].DataItem, "CHK")) == false)
                    {
                        combo.IsDropDownOpen = false;
                        Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                        return;
                    }
                }
            }
        }

        private void dgProductLot_SelectedCommited(object sender, PropertyChangedEventArgs<object> e)
        {
            int iSelectIdx = -1;
            C1ComboBox comboBox = sender as C1ComboBox;
            if (comboBox != null)
            {
                DataGridCellPresenter cellPresenter = comboBox.Parent as DataGridCellPresenter;
                if (cellPresenter != null && cellPresenter.Cell != null && cellPresenter.Cell.Row != null)
                    iSelectIdx = cellPresenter.Cell.Row.Index;
            }

            if (iSelectIdx < 0)
                return;


            if (dgProductLot.CurrentRow == null || iSelectIdx < 0 || string.IsNullOrEmpty(Util.NVC(e.NewValue)))
                return;

            try
            {
                if (dgProductLot.GetRowCount() == 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                _HopperID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID"));
                _MtrlID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "MTRLID"));

                //같은 자재, 같은 호퍼 투입 불가능
                for (int icnt = 0; icnt < dgProductLot.Rows.Count; icnt++)
                {
                    if (icnt != iSelectIdx) //현재 선택한 row 제외
                    {
                        if (dtTmp.Rows[icnt]["HOPPER_ID"].ToString() == _HopperID)//dtTmp.Rows[icnt]["MTRLID"].ToString() == _MtrlID && //자재상관없이 같은 호퍼는 선택 불가능
                        {
                            //같은 호퍼ID를 선택하셨습니다.
                            Util.MessageInfo("SFU2852", (result) =>
                            {
                                if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID", "");
                                }
                            });
                            return;
                        }
                    }
                }
                // (C20200724-000246) 원재료 오투입 방지를 위한 알람팝업 적용 (이전호퍼 투입자재 변경의 경우)
                if (!IsFinalMaterialCheck(_HopperID, _MtrlID))
                {
                    // 투입자재를 변경하시겠습니까? 호퍼와 자재를 확인해주십시오
                    Util.MessageConfirm("SFU8228", (result) =>
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "MTRL_QTY", string.Empty);
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID", "");
                        }
                    });
                    return;
                }
                if (FinalInputMtrlCheck(_HopperID, _MtrlID))
                {
                    //이미 투입요청된 자재입니다. \r\n다시 요청하시겠습니까? -> 해당 HOPPER에 잔량이 존재합니다. 초기화하고 다시 투입하시겠습니까?
                    Util.MessageConfirm("SFU1778", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "Y");
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "N");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataView tempView = dgProductLot.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();                

                if (idx != 0)
                {
                    tempTable.Rows[idx - 1]["RowSequenceNo"] = idx;
                    tempTable.Rows[idx]["RowSequenceNo"] = idx - 1;

                    DataTable newTable = tempTable.Select("", "RowSequenceNo").CopyToDataTable();
                    Util.GridSetData(dgProductLot, newTable, FrameOperation, true);
                }                                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataView tempView = dgProductLot.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();

                if (idx != tempTable.Rows.Count - 1)
                {
                    tempTable.Rows[idx + 1]["RowSequenceNo"] = idx;
                    tempTable.Rows[idx]["RowSequenceNo"] = idx + 1;

                    DataTable newTable = tempTable.Select("", "RowSequenceNo").CopyToDataTable();
                    Util.GridSetData(dgProductLot, tempTable.Select("", "RowSequenceNo").CopyToDataTable(), FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region Mehod

        private void GetVersionCheckFlag()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQUIPID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQUIPID"] = Util.NVC(cboEquipment.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_VER_CHK_FLAG", "INDATA", "OUTDATA", IndataTable);

                if (dt.Rows.Count > 0)
                {
                    VersionCheckFlag = dt.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetWorkOrder()
        {
            try
            {
                bool isProc = false;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MES_CLOSE_FLAG", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());

                if (isProc == false) 
                    Indata["EQPTID"] = _EQPTID;

                string bizName = string.Empty;

                switch (Util.NVC(cboProcess.SelectedValue))
                {
                    case "E0400":
                    case "E0410":
                    case "E0420":
                    case "E0430":
                        bizName = "DA_PRD_SEL_MIXMTRL_WORKORDER_FP_CWA";
                        break;

                    case "E0500":
                    case "E1000":
                        bizName = "DA_PRD_SEL_MIXMTRL_WORKORDER_CWA";
                        break;                    
                }

                Indata["MES_CLOSE_FLAG"] = "N";
                Indata["STDT"] = Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd");
                Indata["EDDT"] = Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd");                
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "RSLTDT", IndataTable);

                dgWorkOrder.BeginEdit();

                Util.GridSetData(dgWorkOrder, dtMain, FrameOperation, true);
                dgWorkOrder.EndEdit();
                //dgWorkOrder.MergingCells -= dgWorkOrder_MergingCells;
                //dgWorkOrder.MergingCells += dgWorkOrder_MergingCells;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.NOTCHING;

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                dgWorkOrder.SelectedIndex = iRow;
            }
        }

        private void GetWOMaterial(string WOID)
        {
            try
            {
                this.txtRemark.Clear();
                Util.gridClear(dgProductLot);
                                
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));                
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                IndataTable.Columns.Add("ALL_FLAG", typeof(string));
                IndataTable.AcceptChanges();

                int RowIndex = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["WOID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[RowIndex].DataItem, "WOID"));
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[RowIndex].DataItem, "PRODID"));
                Indata["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[RowIndex].DataItem, "PROD_VER_CODE"));
                
                if (_ProdID.Substring(3, 2).Equals("CA"))
                {
                    Indata["ALL_FLAG"] = "Y";                    
                }
                else
                {
                    Indata["ALL_FLAG"] = null;
                }

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCHORDID_MATERIAL_LIST_CWA", "INDATA", "OUTDATA", IndataTable, RowSequenceNo:true);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                Util.GridSetData(dgProductLot, dtMain, FrameOperation, true);

                ChkKeepData = dtMain;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable _Mtrlid()
        {
            IndataTable = inDataSet.Tables.Add("INDTLDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("MTRL_QTY", typeof(string));
            IndataTable.Columns.Add("HOPPER_ID", typeof(string));
            IndataTable.Columns.Add("CHK_FLAG", typeof(string));

            dgProductLot.EndEdit();
            DataTable dtTop = DataTableConverter.Convert(dgProductLot.ItemsSource);

            foreach (DataRow _iRow in dtTop.Rows)
            {
                if (_iRow["CHK"].Equals(1))
                {
                    if (_iRow["MTRL_QTY"].Equals("") || _iRow["MTRL_QTY"].Equals("0"))
                    {
                        //투입요청수량을 입력 하세요.
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1978"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        Util.MessageInfo("SFU1978", (result) =>
                        {
                            Keyboard.Focus(dgProductLot.CurrentCell.DataGrid);
                        });
                        InputFlag = false;
                        return null;
                    }
                    if (Double.Parse(Util.NVC(_iRow["MTRL_QTY"])) < 0)
                    {
                        Util.MessageValidation("SFU1977");   //투입요청수량은 정수만 입력 하세요.
                        InputFlag = false;
                        return null;
                    }
                    else
                    {
                        InputFlag = true;
                    }
                    if (_iRow["HOPPER_ID"].Equals(""))
                    {
                        HopperFlag = false;
                        return null;
                    }
                    else
                    {
                        HopperFlag = true;
                    }

                    IndataTable.ImportRow(_iRow);
                    HopperFlag = true;
                    InputFlag = true;
                }
            }
            return IndataTable;
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sEQPTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count == 0) { return; }
                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetGridCboItem(C1ComboBox combo, string sEQPTID, string sMTRLID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                Indata["MTRLID"] = sMTRLID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(HopperMtrlFlag == true ? "DA_BAS_SEL_HOPPER_TANK_MTRL_SET_CBO" : "DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count == 0) { return; }
                combo.ItemsSource = DataTableConverter.Convert(dtMain);

                if (combo != null && combo.Items.Count == 1)
                {
                    DataTable distinctTable = ((DataView)dgProductLot.ItemsSource).Table.DefaultView.ToTable(true, "HOPPER_ID");
                    foreach(DataRow row in distinctTable.Rows)
                        if (string.Equals(row["HOPPER_ID"], dtMain.Rows[0][0]))
                            return;

                    combo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetRequestList()
        {
            try
            {
                if (cboEquipment.Items.Count < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요
                    return;
                }

                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                txtDetailRemark.Clear();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("REQ_DATE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["REQ_DATE"] = Convert.ToDateTime(dtpDate.SelectedDateTime).ToString("yyyyMMdd");     //dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQPTID"] = Util.NVC(EQUIPMENT.SelectedValue.ToString()) == "" ? null : Util.NVC(EQUIPMENT.SelectedValue.ToString());
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_LIST_CWA", "INDATA", "RSLTDT", IndataTable);
                if (dtMain == null)
                {
                    return;
                }
                // 투입요청서 완료포함 여부 (C20200317-000002)
                if (!(bool)chkCompleted.IsChecked)
                {
                    DataView view = dtMain.AsDataView();
                    view.RowFilter = "CMCODE <> 'C'";
                    DataTable dt = view.ToTable();
                    Util.GridSetData(dgRequestList, dt, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgRequestList, dtMain, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DelRequestList()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgRequestList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQ_ID"] = ReqID;
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                //삭제처리 하시겠습니까?
                Util.MessageConfirm("SFU1259", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_RMTRL_INPUT_REQ_PROC", "INDATA", null, IndataTable);
                            if (dtMain != null)
                            {
                                return;
                            }
                            GetRequestList();
                            ReqID = string.Empty;
                        }
                        catch ( Exception ex2 )
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ReqForceEnd()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgRequestList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQ_ID"] = ReqID;
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                //종료 하시겠습니까?
                Util.MessageConfirm("SFU3540", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_RMTRL_INPUT_REQ_PROC_FORCE", "INDATA", null, IndataTable);
                            if (dtMain != null)
                            {
                                GetRequestList();
                                ReqID = string.Empty;

                                return;
                            }
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable CreateReqPrint(DataTable dt)
        {
            try
            {
                dt.Columns.Add("REQ_DTTM", typeof(string));
                dt.Columns.Add("REQ_ID", typeof(string));
                dt.Columns.Add("REQ_ID1", typeof(string));
                dt.Columns.Add("USERNAME", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("EPQTID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["REQ_DTTM"] = string.Empty;
                dr["REQ_ID"] = string.Empty;
                dr["REQ_ID1"] = string.Empty;
                dr["USERNAME"] = string.Empty;
                dr["NOTE"] = string.Empty;
                dr["EPQTID"] = string.Empty;
                dr["PRJT_NAME"] = string.Empty;
                dt.Rows.Add(dr);
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetRequestDetail(string sReqID)
        {
            try
            {
                Util.gridClear(dgRequestDetailList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                //IndataTable.Columns.Add("REQ_DATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["REQ_ID"] = sReqID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                //Indata["REQ_DATE"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_DETAIL_CWA", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgRequestDetailList, dtMain, FrameOperation, true);
                //dgRequestDetailList.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable CreateReqDetlPrint(DataTable dt)
        {
            try
            {
                if (dgRequestDetailList.Rows.Count > 10)
                {
                    Util.AlertInfo("SFU1824");  //자재가 10개이상일 경우 MES팀에 연락주시기바랍니다. (투입요청서 레포트 줄 추가 해야함)
                    return null;
                }
                for (int i = 0; i < dgRequestDetailList.Rows.Count; i++)
                {
                    dt.Columns.Add("MTRLID_" + i, typeof(string));
                    dt.Columns.Add("MTRLDESC_" + i, typeof(string));
                    dt.Columns.Add("QTY_" + i, typeof(decimal));
                    dt.Columns.Add("HOPPER_" + i, typeof(string));
                }

                //DataRow dr = dt.NewRow();
                DataRow dr = null;
                dr = dt.NewRow();
                for (int i = 0; i < dgRequestDetailList.Rows.Count; i++)
                {
                    dr["MTRLID_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRLID"));
                    dr["MTRLDESC_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRLDESC"));
                    dr["QTY_" + i] = System.Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRL_QTY")), 3);
                    dr["HOPPER_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "HOPPER")) + "[" + Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "HOPPER_ID")) + "]";
                }
                dt.Rows.Add(dr);
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string GetRequestCount()
        {
            try
            {
                string _ValueToFind = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_POPUP", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    _ValueToFind = dtMain.Rows[0]["REQ_ID"].ToString();
                }
                return _ValueToFind;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        private void IsHopperMtrlChk()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.Equals(dtResult.Rows[0]["MIXER_HOPPER_MTRL_USE_FLAG"], "Y"))
                    {
                        HopperMtrlFlag = true;
                        return;
                    }
                }
            }
            catch (Exception ex) { }

            HopperMtrlFlag = false;
        }

        public T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                        break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtWorker.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = wndPerson.USERNAME;
                txtWorker.Tag = wndPerson.USERID;
            }
        }

        #endregion

        
    }
}