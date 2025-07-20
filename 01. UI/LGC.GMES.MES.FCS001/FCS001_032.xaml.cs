/************************************************************************************* 
 Created Date : 2020.10.15
      Creator : Dooly
   Decription : 차기 공정 실Tray 버퍼 수량 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  DEVELOPER : Initial Created.
  2022.05.18  이정미 : NB1동 전용으로 변경되어 전체적인 수정 
  2022.07.02  이정미 : 저장 이벤트 수정 - 최대수량 변경 가능하도록 수정
                       저장 이벤트 오류 수정 - 모든 데이터가 업데이트되는 오류 수정 
  2022.08.19  조영대 : 행번호 추가, 블럭설정
  2023.08.14  손동혁 : NA 1동 요청 트레이수량 -> 출고 , 예약 , 강제출고 , 강제예약 수량 세분화 표시
  2023.11.08  조영대 : LAIN 컬럼 위치 변경, 이력조회 탭 추가
  2023.11.17  이의철 : 저장 DA 수정
  2023.11.25  이의철 : 업데이트 시간 체크 기능 추가
  2023.12.06  이의철 : 상/하층 cboFloor 추가
  2024.02.14  권순범 : WA3동 건물코드 추가, 건물코드와 라인콤보박스 매핑
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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_032 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        bool bUseFlag = false; //2023.08.14 상세현황 기능 추가
        bool bFCS001_032_UPDDTTM_CHECK = false; //업데이트 시간 체크 기능 추가
        bool bFCS001_032_FLOOR = false; //상/하층 cboFloor 추가
        bool bFCS001_032_BLDG = false; //[WA3동] 건물코드 사용변수

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_032()
        {
            InitializeComponent();
        }

        #endregion Declaration & Constructor 


        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {            
            /// 2023.08.14
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_032"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                chkDetail.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkDetail.Visibility = Visibility.Collapsed;

            }

            bFCS001_032_UPDDTTM_CHECK = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_032_UPDDTTM_CHECK"); //업데이트 시간 체크 기능 추가

            bFCS001_032_FLOOR =  _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_032_FLOOR"); //상/하층 cboFloor 추가

            bFCS001_032_BLDG = AreaCommonCode_BLDG_CODE("FORM_BLDG_CODE"); //활성화 건물코드 사용여부

            InitCombo();
            //상/하층 cboFloor 추가
            InitControl();
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            //동
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboPlantChild = { cboEquipmentSegment };
            string[] sFilter1 = { "FORM_BLDG_CODE" };
            if (bFCS001_032_BLDG)
                ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", cbChild: cboPlantChild, sFilter: sFilter1);
            else
                ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, sCase: "PLANT", cbChild: cboPlantChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            if (bFCS001_032_BLDG)
                ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE_SHOPID_BLDG_CODE", cbParent: cboLineParent);
            else
                ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE_SHOPID", cbParent: cboLineParent);
    
            //상/하층 cboFloor 추가
            if (bFCS001_032_FLOOR.Equals(true)) 
            {
                string[] sFilter2 = { "FLOOR_CODE" };
                ComCombo.SetCombo(cboFloor, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter2);
            }
        }
        private void InitControl()
        {
            //상/하층 cboFloor 추가
            if (bFCS001_032_FLOOR.Equals(true))
            {
                //cboFloor
                this.lblFloor.Visibility = Visibility.Visible;
                this.cboFloor.Visibility = Visibility.Visible;
                //this.dgShipCutLane.Columns["FLOOR_NAME"].Visibility = Visibility.Collapsed;
            }
            else
            {
                this.lblFloor.Visibility = Visibility.Collapsed;
                this.cboFloor.Visibility = Visibility.Collapsed;
                //this.dgShipCutLane.Columns["FLOOR_NAME"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion Initialize

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgShipCutLane_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    txtSelLane.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_ID"));
                    txtFromOp.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CURR_OP"));
                    txtToOp.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NEXT_OP"));

                    GetDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_ID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CURR_PROC_GR_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NEXT_PROC_GR_CODE")));

                    GetHistory(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_ID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CURR_PROC_GR_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NEXT_PROC_GR_CODE")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgShipCutLane_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgShipCutLane_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CURR_CNT"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        private void dgShipCutLane_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                DataTable dtResult = e.ResultData as DataTable;

                dtResult.Columns.Add("FLAG", typeof(string));

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dtResult.Rows[i]["FLAG"] = "N";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgShipCutLane_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                string[] sColumnName = new string[] { "CURR_OP", "NEXT_OP", "LANE_ID", };
                _Util.SetDataGridMergeExtensionCol(dgShipCutLane, sColumnName, DataGridMergeMode.VERTICAL);

                btnSearch.IsEnabled = true;
                btnSave.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgShipCutLane.GetRowCount() == 0)
            {

                Util.Alert("SFU3552");  //저장 할 DATA가 없습니다.
                return;
            }

            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    /* DataTable RQSTDT = new DataTable();
                     RQSTDT.TableName = "RQSTDT";
                     RQSTDT.Columns.Add("PORT_ID", typeof(String));
                     RQSTDT.Columns.Add("MAX_TRF_QTY", typeof(String));
                     RQSTDT.Columns.Add("USERID", typeof(String));

                     DataTable dt = DataTableConverter.Convert(dgShipCutLane.ItemsSource);

                     for (int i = 0; i < dt.Rows.Count; i++)
                     {
                         DataRow dr = RQSTDT.NewRow();
                         dr["PORT_ID"] = dt.Rows[i]["PORT_ID"].ToString();
                         dr["MAX_TRF_QTY"] = dt.Rows[i]["MAX_TRF_QTY"].ToString();
                         dr["USERID"] = LoginInfo.USERID;

                         RQSTDT.Rows.Add(dr);
                     }*/

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("MAX_BUF_QTY", typeof(String));
                    RQSTDT.Columns.Add("USERID", typeof(String));
                    RQSTDT.Columns.Add("LANE_ID", typeof(String));
                    RQSTDT.Columns.Add("CURR_PROC_GR_CODE", typeof(String));
                    RQSTDT.Columns.Add("NEXT_PROC_GR_CODE", typeof(String));

                    DataTable dt = DataTableConverter.Convert(dgShipCutLane.ItemsSource);

                    for (int i = 0; i < dgShipCutLane.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "FLAG")).Equals("Y"))
                        {  // string[] sTagList = dgShipCutLane.GetCell(i, 0).ToString().Split('_');
                            DataRow dr = RQSTDT.NewRow();
                            dr["MAX_BUF_QTY"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "MAX_BUF_QTY")); //dgShipCutLane.GetCell(i, 4).ToString();
                                                                                                                                      //Convert.ToUInt16(dgShipCutLane.GetCell(i, 1).Text);
                            dr["USERID"] = LoginInfo.USERID;
                            dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "LANE_ID"));
                            dr["CURR_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "CURR_PROC_GR_CODE"));
                            //agList[1]; //NEXT_OP
                            dr["NEXT_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "NEXT_PROC_GR_CODE"));//sTagList[2];
                            RQSTDT.Rows.Add(dr);
                        }
                    }

                    ShowLoadingIndicator();
                    //저장 DA 수정
                    //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_UPD_LANE_AGING_OUT_CNT_NEW", "RQSTDT", "RSLTDT", RQSTDT);
                    //new ClientProxy().ExecuteService("DA_UPD_LANE_AGING_OUT_CNT_NEW", "RQSTDT", "RSLTDT", RQSTDT, (results, ex) =>
                    new ClientProxy().ExecuteService("BR_SET_FORMLGS_REAL_CST_BUF", "RQSTDT", "RSLTDT", RQSTDT, (results, ex) =>
                    {
                        if (ex != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                        GetList();

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                    HiddenLoadingIndicator();
                }
            });
        }

        private void dgLaneTryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLaneTryList.ItemsSource == null)
                    return;

                if (sender == null)
                    return;

                if (dgLaneTryList.CurrentRow != null && dgLaneTryList.CurrentColumn.Name.Equals("CSTID"))
                {
                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLaneTryList.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLaneTryList.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    this.FrameOperation.OpenMenu("SFU010710010", true, parameters);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLaneTryList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgLaneTryList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;
                
                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    //if (e.Cell.Column.Name.Equals("CSTID"))
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //}

                    //업데이트 시간 체크 기능 추가
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                        string UPDDTTM_CHECK_FLAG = string.Empty;                        

                        if (bFCS001_032_UPDDTTM_CHECK.Equals(true))
                        {
                            UPDDTTM_CHECK_FLAG = Util.NVC(dr.Row["UPDDTTM_CHECK_FLAG"]);

                            if (UPDDTTM_CHECK_FLAG.Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSlateGray);
                            }                            
                        }
                    }

                    
                }
            }));
        }
                
        private void dgLaneTryList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null) return;

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgLaneTryList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        /// 2023.08.14
        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            dgShipCutLane.Columns["NOR_SHIP"].Visibility = System.Windows.Visibility.Collapsed;
            dgShipCutLane.Columns["NOR_RESV"].Visibility = System.Windows.Visibility.Collapsed;
            dgShipCutLane.Columns["FORCE_SHIP"].Visibility = System.Windows.Visibility.Collapsed;
            dgShipCutLane.Columns["FORCE_RESV"].Visibility = System.Windows.Visibility.Collapsed;
            dgLaneTryList.Columns["DETAIL_STATUS"].Visibility = System.Windows.Visibility.Collapsed;
        }
        /// 2023.08.14
        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            dgShipCutLane.Columns["NOR_SHIP"].Visibility = System.Windows.Visibility.Visible;
            dgShipCutLane.Columns["NOR_RESV"].Visibility = System.Windows.Visibility.Visible;
            dgShipCutLane.Columns["FORCE_SHIP"].Visibility = System.Windows.Visibility.Visible;
            dgShipCutLane.Columns["FORCE_RESV"].Visibility = System.Windows.Visibility.Visible;
            dgLaneTryList.Columns["DETAIL_STATUS"].Visibility = System.Windows.Visibility.Visible;
        }

        #endregion Event


        #region Method
        private void Clear()
        {
            Util.gridClear(dgShipCutLane);
            Util.gridClear(dgLaneTryList);

            txtSelLane.Text = string.Empty;
            txtFromOp.Text = string.Empty;
            txtToOp.Text = string.Empty;
        }

        private void GetList()
        {
            try
            {
                Clear();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("COND", typeof(string));
                dtRqst.Columns.Add("FLOOR_CODE", typeof(string));
                dtRqst.Columns.Add("BLDG_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                DataTable dtRslt = new DataTable();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COND"] = Util.ConvertEmptyToNull((string)cboEquipmentSegment.SelectedValue);
                
                //상/하층 cboFloor 추가
                if (bFCS001_032_FLOOR.Equals(true))
                {
                    dr["FLOOR_CODE"] = Util.GetCondition(cboFloor, bAllNull: true);
                }

                //동번호 코드 추가
                if (bFCS001_032_BLDG)
                {
                    dr["BLDG_CODE"] = Util.GetCondition(cboArea, bAllNull: true);
                }
                
                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;
                btnSave.IsEnabled = false;

                // 백그라운드로 실행, 비즈 후 순서대로 dgShipCutLane_ExecuteDataModify, dgShipCutLane_ExecuteDataCompleted 실행
                if(LoginInfo.CFG_AREA_ID == "A3") //NA
                    dgShipCutLane.ExecuteService("DA_SEL_LANE_OUT_ABLE_CNT_ESNA", "INDATA", "OUTDATA", dtRqst, true);
                else
                    dgShipCutLane.ExecuteService("DA_SEL_LANE_OUT_ABLE_CNT", "INDATA", "OUTDATA", dtRqst, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDetail(string sLineID, string sBF_PROC, string sCR_PROC)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CURR_PROCID", typeof(string));
                dtRqst.Columns.Add("NEXT_PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = sLineID;
                dr["CURR_PROCID"] = sBF_PROC;
                dr["NEXT_PROCID"] = sCR_PROC;
                dtRqst.Rows.Add(dr);

                // 백그라운드로 실행
                dgLaneTryList.ExecuteService("DA_SEL_LANE_BUFFER_TRAY_LIST", "INDATA", "OUTDATA", dtRqst, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetHistory(string sLineID, string sBF_PROC, string sCR_PROC)
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CURR_PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("NEXT_PROC_GR_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = sLineID;
                dr["CURR_PROC_GR_CODE"] = sBF_PROC;
                dr["NEXT_PROC_GR_CODE"] = sCR_PROC;
                dtRqst.Rows.Add(dr);

                // 백그라운드로 실행
                dgShipCutLaneHist.ExecuteService("DA_SEL_LANE_OUT_ABLE_CNT_HIST", "RQSTDT", "RSLTDT", dtRqst, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool AreaCommonCode_BLDG_CODE(string sComeCodeType)
        {
            try
            {
                bool nCheck = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                    nCheck = true;

                return nCheck;
            }
            catch (Exception ex) { }

            return false;
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


        #endregion Method

      
    }
}
