/*************************************************************************************
 Created Date : 2024.05.07
      Creator : LG CNS 
   Decription : Mixer 원재료 Lot 투입 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.07  DEVELOPER : [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 투입 이력 조회 Initial Created.
  2024.06.10  백상우    : [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건
  2024.07.12  이원열    : [E20240626-000936] Mixer 원재료 Lot 투입이력 조회 조건 추가 - 설비 추가
  2025.03.14  이민형    : [HD_OSS_0096] 자재코드 Combo -> SearchCombo로 변환 및 Code 기준 정렬
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_042 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo _combo = new CommonCombo();
        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_042()
        {
            InitializeComponent();     
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            SetEvent();
        }

        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;


            // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 : 검색 조건 변경 Start
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, null, cbParent: cboEquipmentSegmentChild);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent, null, sCase: "MixingProcess");

            // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 : 검색 조건 변경 End
            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            
            //Hopper 정보
            SetHopperList();

            //설비정보
            SetEquipment();
        }
        #endregion

        #region Event
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31) {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }

            // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
            if (cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223"); //라인을 선택하세요
                return;
            }
            // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End

            if (cboProcess.SelectedValue.ToString().Equals("SELECT")) {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요
                return;
            }

            if (Util.NVC(cboEquipment.SelectedItemsToString) == "")
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return;
            }

            SearchData();
        }
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                
                SetProcess();
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
                Util.gridClear(dgResult);
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End
            }
            SetEquipment();
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null )
            {
                //Hopper 정보
                SetHopperList();
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
                Util.gridClear(dgResult);
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End
            }
            SetEquipment();
        }
        #endregion

        #region [Area] - 조회조건
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.Items.Count > 0 && cboArea.SelectedValue != null && !cboArea.SelectedValue.Equals("SELECT"))
            {

                SetMtrlCalss();
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
                Util.gridClear(dgResult);
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End
            }
            SetEquipment();
        }
        #endregion

        #region [자재분류] - 조회조건
        private void cboMtrlClass_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboMtrlClass.Items.Count > 0 && cboMtrlClass.SelectedValue != null && !cboMtrlClass.SelectedValue.Equals("ALL"))
            {

                SetMtrlID();

            }
        }
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_CWA_MIXING", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                    cboEquipment.CheckAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [자재분류 정보 가져오기]
        private void SetMtrlCalss()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTGR_FOR_RMTRL_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboMtrlClass.DisplayMemberPath = "MTGRNAME";
                cboMtrlClass.SelectedValuePath = "MTGRID";

                DataRow drIns = dtResult.NewRow();
                drIns["MTGRNAME"] = "-ALL-";
                drIns["MTGRID"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboMtrlClass.ItemsSource = dtResult.Copy().AsDataView();
                cboMtrlClass.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [자재ID 정보 가져오기]
        private void SetMtrlID()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sMtrlClass = Util.GetCondition(cboMtrlClass);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MTGRID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["MTGRID"] = sMtrlClass;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RMTRL_BY_MTGR_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboMtrlID.DisplayMemberPath = "MTRLDISP2";
                cboMtrlID.SelectedValuePath = "MTRLID";

                 DataRow drIns = dtResult.NewRow();
                drIns["MTRLDISP2"] = "-ALL-";
                drIns["MTRLID"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                dtResult.DefaultView.Sort = "MTRLID";
                cboMtrlID.ItemsSource = dtResult.Copy().AsDataView();
                //cboMtrlID.ItemsSource = DataTableConverter.Convert(dtResult);                
                cboMtrlID.SelectedText = "-ALL";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Hopper ID 정보 가져오기]
        private void SetHopperList()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;


                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    popHopperID.ItemsSource = null;
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["PROCID"] = sProc;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HOPPER_FOR_PRCS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                popHopperID.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void SearchData()
        {

            try
            {
                Util.gridClear(dgResult);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End

                IndataTable.Columns.Add("CHK_MISS_MTRL_LOT", typeof(string));
                IndataTable.Columns.Add("MTRLCLASS", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("HOPPERID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("EQPTID_LIST", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);

                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 Start
                Indata["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                Indata["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                // [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건 End

                Indata["CHK_MISS_MTRL_LOT"] = chkMissedMaterialLot.IsChecked == true ? "Y" : null;
                Indata["MTRLCLASS"] = Util.GetCondition(cboMtrlClass, bAllNull: true);                

                if ((cboMtrlID.SelectedValue == null) || (string.IsNullOrEmpty(cboMtrlID.SelectedValue.ToString())))
                {
                    Indata["MTRLID"] = null;
                }
                else
                {
                    Indata["MTRLID"] = cboMtrlID.SelectedValue.ToString();
                }

                if ((popHopperID.SelectedValue == null) || (string.IsNullOrEmpty(popHopperID.SelectedValue.ToString())))
                {
                    Indata["HOPPERID"] = null;
                }
                else
                {
                    Indata["HOPPERID"] = popHopperID.SelectedValue.ToString();
                }

                Indata["LOTID"] = Util.GetCondition(txtLotId).Equals("") ? null : Util.GetCondition(txtLotId);
                Indata["EQPTID_LIST"] = cboEquipment.SelectedItemsToString;

                IndataTable.Rows.Add(Indata);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_MIXMTRL_INPUTLOT_TRACKING", "RQSTDT", "RSLTDT", IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            Util.gridClear(dgResult);
                            Util.GridSetData(dgResult, searchResult, FrameOperation, true);

                        }
                        else
                        {
                            Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                        }

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
                );

            }
            catch (Exception ex)
            {
                
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [E20240607-001447] Mixer 원재료 Lot 투입 이력 개선 요청건
        private void dgResult_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (sender == null)
                return;

            dgResult.Dispatcher.BeginInvoke(new Action(() =>
            {
                DataRowView drv = e.Row.DataItem as DataRowView;

                if (drv != null)
                {
                    if (drv["COMMENT"].GetString() == "Y")
                    {
                        e.Row.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
            }));
        }

        private void dgResult_UnloadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Row.Presenter != null)
            {
                e.Row.Presenter.Background = null;
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

    }
}
