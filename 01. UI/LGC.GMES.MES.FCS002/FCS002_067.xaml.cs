/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 이력 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.20 손우석 CSR ID 5991 GMES 설비 Loss 현황의 일자별 loss 현황 운영 설비 상태 조회항목 구분 요청 [요청번호] C20191119-000176
  2020.09.25 김동일 C20200908-000102 : TAB WELD LOSS 상세 정보 더블 클릭 시 타 M/C T,U 정보 조회 팝업 추가.
  2020.12.08 안기백 동일 화면 활성화 추가
  2022.05.26 이정미 : 설비 COMBOBOX 수정 - 데이터 없을 시 초기화 
  2022.06.27 강동희 : Ultium Cell 기준으로(MAIN & MACHIN) 수정
  2022.07.07 이정미 : 2022.05.06 기준으로 RollBack
  2022.07.12 이정미 : 조회 시,EQPTID 대표 설비로 조회 조건 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_067 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS002_067()
        {
            InitializeComponent();
            InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            #region MyRegion
            ////동,라인,공정,설비 셋팅
            //CommonCombo_Form _combo = new CommonCombo_Form();

            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            //C1ComboBox[] cboAreaDailyChild = { cboEquipmentSegmentDaily };
            //_combo.SetCombo(cboAreaDaily, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaDailyChild);

            //if (string.IsNullOrWhiteSpace(LoginInfo.CFG_AREA_ID) || LoginInfo.CFG_AREA_ID.Length < 1 || !LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P"))
            //{
            //    bPack = false;

            //    //라인
            //    C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //    C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //    _combo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //    C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
            //    C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily, cboEquipmentDaily };
            //    _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);


            //    //공정
            //    C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            //    C1ComboBox[] cbProcessChild = { cboEquipment };
            //    _combo.SetCombo(cboProcess, CommonCombo_Form.ComboStatus.SELECT, sCase: "PROCESS", cbChild: cbProcessChild, cbParent: cbProcessParent);

            //    C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
            //    C1ComboBox[] cbProcessDailyChild = { cboEquipmentDaily };
            //    string strProcessDailyCase = string.Empty;

            //    if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
            //    {
            //        strProcessDailyCase = "cboProcessPack";
            //    }
            //    else
            //    {
            //        strProcessDailyCase = "PROCESS";
            //    }

            //    _combo.SetCombo(cboProcessDaily, CommonCombo_Form.ComboStatus.ALL, sCase: "PROCESS", cbChild: cbProcessDailyChild, cbParent: cbProcessDailyParent);

            //    //설비
            //    C1ComboBox[] cbEquipmentParent = { cboEquipmentSegment, cboProcess };
            //    _combo.SetCombo(cboEquipment, CommonCombo_Form.ComboStatus.ALL, sCase: "EQUIPMENT", cbParent: cbEquipmentParent);

            //    C1ComboBox[] cbEquipmentDailyParent = { cboEquipmentSegmentDaily, cboProcessDaily };
            //    _combo.SetCombo(cboEquipmentDaily, CommonCombo_Form.ComboStatus.ALL, sCase: "EQUIPMENT", cbParent: cbEquipmentDailyParent);

            //    cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            //    cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            //    SetTroubleUnitColumnDisplay();
            //}
            //else
            //{
            //    bPack = true;

            //    //라인
            //    C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //    C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            //    _combo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //    C1ComboBox[] cboEquipmentSegmentDailyParent = { cboAreaDaily };
            //    C1ComboBox[] cboEquipmentSegmentDailyChild = { cboProcessDaily };
            //    _combo.SetCombo(cboEquipmentSegmentDaily, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboEquipmentSegmentDailyChild, cbParent: cboEquipmentSegmentDailyParent);

            //    //공정
            //    C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            //    string strProcessCase = string.Empty;
            //    if (LoginInfo.CFG_AREA_ID.StartsWith("P"))
            //    {
            //        strProcessCase = "cboProcessPack";
            //    }
            //    else
            //    {
            //        strProcessCase = "PROCESS";
            //    }

            //    _combo.SetCombo(cboProcess, CommonCombo_Form.ComboStatus.ALL, cbParent: cbProcessParent);

            //    C1ComboBox[] cbProcessDailyParent = { cboEquipmentSegmentDaily };
            //    _combo.SetCombo(cboProcessDaily, CommonCombo_Form.ComboStatus.ALL, sCase: strProcessCase, cbParent: cbProcessDailyParent);

            //    SetEquipment(cboEquipment, cboProcess);
            //    SetEquipment(cboEquipmentDaily, cboProcessDaily);

            //    cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            //    cboEquipmentSegmentDaily.SelectedItemChanged += cboEquipmentSegmentDaily_SelectedItemChanged;
            //    cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            //    cboProcessDaily.SelectedItemChanged += cboProcessDaily_SelectedItemChanged;
            //} 
            #endregion

            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboLaneChild = { cboEqp };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", cbChild: cboLaneChild);

            string[] sFilterEqpType = { "DEG,EOL" };  //설정된 설비만 Loss 모니터링 가능(DEGAS, EOL)
            C1ComboBox[] cboEqpKindChild = { cboEqp};
            _combo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType, cbChild: cboEqpKindChild);
      
            C1ComboBox[] cboEqpParent = { cboLane, cboEqpKind };
            //C1ComboBox[] cboEqpChild = { cboLossDetl, cboLatest, cboCauEqp };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "LOSSEQP", cbParent: cboEqpParent);

            //--------------------------------------------------
            C1ComboBox[] cboLaneDailyChild = { cboEqpDaily };
            _combo.SetCombo(cboLaneDaily, CommonCombo_Form.ComboStatus.NONE, sCase: "LANE", cbChild: cboLaneDailyChild);

            //string[] sFilterEqpType = { "DEG,EOL" };  //설정된 설비만 Loss 모니터링 가능(DEGAS, EOL)
            C1ComboBox[] cboEqpKindDailyChild = { cboEqpDaily };
            _combo.SetCombo(cboEqpKindDaily, CommonCombo_Form.ComboStatus.NONE, sCase: "EQUIPMENTGROUP", sFilter: sFilterEqpType, cbChild: cboEqpKindDailyChild);

            C1ComboBox[] cboEqpDailyParent = { cboLaneDaily, cboEqpKindDaily };
            //C1ComboBox[] cboEqpChild = { cboLossDetl, cboLatest, cboCauEqp };
            _combo.SetCombo(cboEqpDaily, CommonCombo_Form.ComboStatus.NONE, sCase: "LOSSEQP", cbParent: cboEqpDailyParent);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetLossList();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchDaily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("WRK_DATE", typeof(string));
                dtRqst.Columns.Add("EIOSTAT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEqpDaily.GetStringValue("MAIN_EQPTID"); //Util.GetCondition(cboEqpDaily);
                dr["WRK_DATE"] = Util.GetCondition(ldpDateFrom_Daily);
                //dr["EIOSTAT"] = cboEquipmentState.SelectedValue; 
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_RAW_DAILY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgLossDailyList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgLossList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLossList.CurrentRow != null && dgLossList.CurrentColumn.Name.Equals("LOSSCNT"))
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("LOSS_CODE", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOSS_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "LOSS_CODE")); ;
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgLossList.CurrentRow.DataItem, "EQPTID")); ;
                    dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                    dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);
                    dtRqst.Rows.Add(dr);

                    ShowLoadingIndicator();
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgLossDetail, dtResult, FrameOperation, true);
                    }
                    else
                    {
                        dgLossDetail.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgLossList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("LOSSCNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));

        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEqp.Items.Count == 0)
            {
                cboEqp.Text = string.Empty;
            }
        }
        #endregion Event

        #region Mehod
        private void GetLossList()
        {
            try
            {
                //Show Loading indicator
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_WRK_DATE", typeof(string));
                dtRqst.Columns.Add("TO_WRK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboEqp.GetStringValue("MAIN_EQPTID");//Util.GetCondition(cboEqp);
                dr["FROM_WRK_DATE"] = Util.GetCondition(ldpDateFrom);
                dr["TO_WRK_DATE"] = Util.GetCondition(ldpDateTo);
                dtRqst.Rows.Add(dr);
                 
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_SUMMARY", "INDATA", "OUTDATA", dtRqst);
                dgLossList.ItemsSource = DataTableConverter.Convert(dtRslt);
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

        #endregion  Mehod
    }
}
