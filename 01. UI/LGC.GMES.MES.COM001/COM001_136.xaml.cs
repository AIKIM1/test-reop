/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.12.16  정문교 : 조회 조건에 생산 구분 조건 추가
                       조회에 생산 구분 칼럼 추가
  2020.03.30  정문교 : 조회 그리드에 위치구분, 위치, 상세위치구분, 상세위치 칼럼 추가
  2020.10.20  김동일 : C20200812-000329 조립동 QMS hold 정보 추가 요청 건
  2022.06.15  정재홍 : C20220329-000581 - 미검사 체크 박스 추가, 집계항목에 미검사 컬럼 ON/OFF, 집계항목에서 선택한 제품만 상세에 조회, 상세 상태선택시 Sorting 기능
  2022.07.21  최도훈 : Line 호출부분 조립, 활성화 시스템에 따라 분기 
  2023.02.15  조영대 : 활성화에서 오픈시 재공(Roll) => 재공(Cells) 로 변경, 재공(Lane) 숨김처리
  2023.05.15  박성진 : E20230504-001196 생산구분 콤보박스 선택에 따른 조회조건 변경
  2023.11.03  윤지해 : E20231014-001517 라인 콤보박스 활성화 외 MultiSelectionBox로 변경
  2024.09.11  안유수 E20240822-001327 대상목록 Grid 조회 DA 변경 DA_PRD_SEL_FN_SFC_WIP -> DA_PRD_SEL_FN_SFC_WIP_FOR_TRGT_LIST 재공 조회 중복 열 오류 수정건
  2025.04.15  조범모 MI_LS_OSS_0066   대상목록 Grid 더블클릭시 상세목록의 LOT 수와 일치하지 않음 수정 (조회조건의 생산구분이 ALL 일 때 발생)
  2025.06.09  박세리 MES_IT_E_664 조립일 때 상세 데이터 그리드 선택(CHK) 체크박스 컬럼 안보이도록 처리
  2025.07.02  박세리 OC9_OSS_0044 라우트 변경 시 WIP_TYPE_CODE 변수 BR에 안넘어가는 현상 조치, wndRoute_Closed() 함수
  2025.07.17  이민형 : HD_OSS_0452 재공조회 -> 경로변경 에러, 소형화 소스 반영
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_136 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        String _PRODID = "";

        DataTable dtLotAll;

        public COM001_136()
        {
            InitializeComponent();
            InitCombo();
            SetElec();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRoute);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //if (FrameOperation.AUTHORITY.Equals("W"))
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
            //}
            ////여기까지 사용자 권한별로 버튼 숨기기

            //// CWA전극만 QMSHOLD 추가 [2019-09-18]
            //if (_Util.IsCommonCodeUse("QMS_HOLD_VISIBLE", LoginInfo.CFG_AREA_ID) == true)
            //    dgLotList.Columns["QMSHOLD"].Visibility = Visibility.Visible;

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                (dgSummary.Columns["WAIT_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["WAIT_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["QLTY_NO_INSP_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["QLTY_NO_INSP_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["PROC_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["PROC_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["END_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["END_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["HOLD_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["HOLD_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["BIZWF_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["BIZWF_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_AREA_OUT_OUTSIDE_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_AREA_OUT_OUTSIDE_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_AREA_OUT_CONVEYOR_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_AREA_OUT_CONVEYOR_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_AREA_IN_OUTSIDE_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_AREA_IN_OUTSIDE_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_AREA_IN_CONVEYOR_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_AREA_IN_CONVEYOR_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_AREA_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_AREA_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_SHOP_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_SHOP_QTY2"].Visibility = Visibility.Collapsed;
                (dgSummary.Columns["MOVE_SHIP_QTY"].Header as List<string>)[1] = "WIP_CELLS";
                dgSummary.Columns["MOVE_SHIP_QTY2"].Visibility = Visibility.Collapsed;
            }

            Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            // 2023.11.03 윤지해 CSRID E20231014-001517 라인 셋팅
            cboEquipmentSegment.ApplyTemplate();
            cboEquipmentSegmentMulti.ApplyTemplate();

            CommonCombo _combo = new CommonCombo();
            
            //동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                cboEquipmentSegment.Visibility = Visibility.Visible;
                cboEquipmentSegmentMulti.Visibility = Visibility.Collapsed;
                SetEquipmentSegmentCombo(cboEquipmentSegment);
            }
            else
            {
                cboEquipmentSegment.Visibility = Visibility.Collapsed;
                cboEquipmentSegmentMulti.Visibility = Visibility.Visible;
                SetEquipmentSegmentCombo(cboEquipmentSegmentMulti);
            }
             
            SetProcessCombo(cboProcess);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            

            // 2023.11.03 윤지해 CSRID E20231014-001517 주석처리
            /*
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };


            // 활성화 시스템 일 때,
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_FORM");
            }
            // 조립 시스템 일 때,
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);
            }


            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");
            
            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;
            if (cboProcess.Items.Count > 0) cboProcess.SelectedIndex = 0;
            */

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            _combo.SetCombo(cboWipStat, CommonCombo.ComboStatus.ALL);

            DataTable dtWipStat = DataTableConverter.Convert(cboWipStat.ItemsSource);

            DataRow dr = dtWipStat.NewRow();
            dr["CBO_NAME"] = "HOLD:HOLD";
            dr["CBO_CODE"] = "HOLD";
            dtWipStat.Rows.Add(dr);

            cboWipStat.ItemsSource = dtWipStat.Copy().AsDataView();

            //cboWipStat.SelectedValue = "WAIT";

            //생산구분
            string[] sFilter2 = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;
        }

        #endregion

        #region Event
        private void chkProdVerCode_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
        }

        private void chkProdVerCode_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStock();

            dtLotAll = null;
        }

        private void btnRoute_Click(object sender, RoutedEventArgs e)
        {
            if (_PRODID.Equals(""))
            {
                Util.MessageValidation("SFU1895");  //제품을 선택하세요.
                return;
            }

            COM001_018_ROUTE wndPopup = new COM001_018_ROUTE();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _PRODID;
                //Parameters[1] = org_set["TIME"].ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndRoute_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }


        private void wndRoute_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_018_ROUTE window = sender as COM001_018_ROUTE;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    string sRouteID = window.ROUTID;
                    string sFlowID = window.FLOWID;
                    string sProcId = window.PROCID;
                    string sWipTypeCode = window.WIP_TYPE;

                    //변경하시겠습니까?
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    //        "변경하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU2875", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {

                            DataSet inData = new DataSet();
                            DataTable dtRqst = inData.Tables.Add("INDATA");

                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("ROUTE_TO", typeof(string));
                            dtRqst.Columns.Add("FLOW_TO", typeof(string));
                            dtRqst.Columns.Add("PROCID_TO", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("WIP_TYPE_CODE", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["ROUTE_TO"] = sRouteID;
                            dr["FLOW_TO"] = sFlowID;
                            dr["PROCID_TO"] = sProcId;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["WIP_TYPE_CODE"] = sWipTypeCode.Equals("SELECT") ? "" : sWipTypeCode;
                            dtRqst.Rows.Add(dr);


                            DataTable dtLot = inData.Tables.Add("INLOT");

                            dtLot.Columns.Add("LOTID", typeof(string));


                            DataRow[] chkRow = Util.gridGetChecked(ref dgLotList, "CHK");

                            if (chkRow.Length == 0)
                            {
                                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                                return;
                            }

                            for (int i = 0; i < chkRow.Length; i++)
                            {
                                if (chkRow[i]["D_WIPSTAT"].ToString().Equals("PROC"))
                                {
                                    Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                                    return;
                                }

                                DataRow drLot = dtLot.NewRow();
                                drLot["LOTID"] = chkRow[i]["D_LOTID"].ToString();
                                dtLot.Rows.Add(drLot);
                            }
                                    

                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CHANGE_ROUTE", "INDATA,INLOT", null, inData);

                            Util.AlertInfo("SFU1166");  //변경되었습니다.
                            cboWipStat_SelectedItemChanged(null, null);
                        }
                    });

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboWipStat_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgSummary.GetRowCount() > 0 && dgSummary.CurrentRow != null)
            {
                // 주석처리 2022.06.16
                //GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")),
                //             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")));
                ////////////////////////////////////////////////////////////////////////////////////////////////

                if (dtLotAll == null) return;

                Util.gridClear(dgLotList);

                if (Util.GetCondition(cboWipStat) == "")
                {
                    Util.GridSetData(dgLotList, dtLotAll, FrameOperation, true);
                }
                else
                {
                    if (dtLotAll.Select("D_WIPSTAT = '" + Util.GetCondition(cboWipStat) + "'").Length > 0)
                    {
                        DataTable dtLot = dtLotAll.Select("D_WIPSTAT ='" + Util.GetCondition(cboWipStat) + "'").CopyToDataTable();
                        Util.GridSetData(dgLotList, dtLot, FrameOperation, true);

                    }
                    else
                    {
                        Util.gridClear(dgLotList);
                        return;
                    }
                }
            }
        }


        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    string sPILOT_FLAG = "";
                    switch (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODUCTION_TYPE")))
                    {
                        case "A":
                            sPILOT_FLAG = "A";
                            break;
                        case "P":
                            sPILOT_FLAG = "N";
                            break;
                        case "X":
                            sPILOT_FLAG = "Y";
                            break;
                        case "L":
                            sPILOT_FLAG = "L";
                            break;
                        default:
                            sPILOT_FLAG = "A";
                            break;
                    }

                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 sPILOT_FLAG);
                                // Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")),
                                // Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTTYPE")));

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),"");

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        //private void GetDetailLot(string sProcId, string sProdId, string sProdVerCode, string sLotType)
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.Columns.Add("SUM_DATE", typeof(string));
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("AREAID", typeof(string));
        //        dtRqst.Columns.Add("EQSGID", typeof(string));
        //        dtRqst.Columns.Add("PROCID", typeof(string));
        //        dtRqst.Columns.Add("PRODID", typeof(string));
        //        dtRqst.Columns.Add("WIPSTAT", typeof(string));
        //        dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
        //        dtRqst.Columns.Add("PRJT_NAME", typeof(string));
        //        dtRqst.Columns.Add("RACK_FLAG", typeof(string));            // 대여RACK 제외 추가 [2018-08-24]
        //        dtRqst.Columns.Add("ROLL_SEQ_FLAG1", typeof(string));   // 롤프레스 압연 전 구분 [2018-08-24]
        //        dtRqst.Columns.Add("ROLL_SEQ_FLAG2", typeof(string));   // 롤프레스 압연 후 구분 [2018-08-24]
        //        dtRqst.Columns.Add("LOTTYPE", typeof(string));

        //        DataRow dr = dtRqst.NewRow();

        //        dr["SUM_DATE"] = Util.GetCondition(dtpDate);
        //        dr["LANGID"] = LoginInfo.LANGID;

        //        dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
        //        if (dr["AREAID"].Equals("")) return;

        //        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
        //        dr["PROCID"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING + "A")) ? Process.ROLL_PRESSING : sProcId;
        //        dr["PRODID"] = sProdId == "" ? null : sProdId;
        //        dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);

        //        if ((bool)chkProdVerCode.IsChecked)
        //            dr["PROD_VER_CODE"] = sProdVerCode;

        //        dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

        //        dr["RACK_FLAG"] = null;// chkExceptRack.IsChecked == true ? "Y" : null;
        //        dr["ROLL_SEQ_FLAG1"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING)) ? "Y" : null;
        //        dr["ROLL_SEQ_FLAG2"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING + "A")) ? "Y" : null;

        //        if (string.IsNullOrWhiteSpace(sLotType))
        //        {
        //            dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
        //        }
        //        else
        //        {
        //            dr["LOTTYPE"] = sLotType;
        //        }

        //        dtRqst.Rows.Add(dr);

        //        ShowLoadingIndicator();

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_IN_AREA_SNAP_DETAIL", "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
        //        {
        //            HiddenLoadingIndicator();

        //            if (searchException != null)
        //            {
        //                Util.MessageException(searchException);
        //                return;
        //            }

        //            Util.GridSetData(dgLotList, searchResult, FrameOperation, true);
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}

        #region 2023.11.03 윤지해 E20231014-001517 활성화 외 라인 멀티 선택 가능하도록 변경
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                SetEquipmentSegmentCombo(cboEquipmentSegment);
            }
            else
            {
                SetEquipmentSegmentCombo(cboEquipmentSegmentMulti);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboProcess);
        }

        private void cboEquipmentSegmentMulti_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cboProcess.ItemsSource = null;
                cboProcess.Items.Clear();

                string tmpLine = string.Empty;

                tmpLine = SelectEquipmentSegmentMulti();

                if (string.IsNullOrEmpty(tmpLine)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = tmpLine;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_WITH_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (CommonVerify.HasTableRow(dtResult))
                {
                    cboProcess.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                }

                if(dtResult.Rows.Count == 1 || cboProcess.SelectedIndex < 0)
                {
                    cboProcess.SelectedIndex = 0;
                }
                else if (dtResult.Rows.Count == 2)
                {
                    cboProcess.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Method
        //private void GetStock()
        //{
        //    try
        //    {
        //        string sBizName = string.Empty;

        //        // 조립 QMS HOLD 정보 추가 요청에 따른 분기.
        //        if (IsQMSHoldQtyAddArea())
        //        {
        //            sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_SNAP_A" : "DA_PRD_SEL_STOCK_IN_AREA_SNAP_A";

        //            dgSummary.Columns["QMS_HOLD_LOT_CNT"].Visibility = Visibility.Visible;
        //            dgSummary.Columns["QMS_HOLD_LOT_QTY"].Visibility = Visibility.Visible;
        //            dgSummary.Columns["QMS_HOLD_LOT_QTY2"].Visibility = Visibility.Visible;

        //            //dgSummary.Columns["HOLD_LOT_CNT"].Header = new List<string>() { "MES HOLD", "LOT수" };
        //            //dgSummary.Columns["HOLD_LOT_QTY"].Header = new List<string>() { "MES HOLD", "재공(Roll)" };
        //            //dgSummary.Columns["HOLD_LOT_QTY2"].Header = new List<string>() { "MES HOLD", "재공(Lane)" };
        //        }
        //        else
        //        {
        //            sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_SNAP" : "DA_PRD_SEL_STOCK_IN_AREA_SNAP";

        //            dgSummary.Columns["QMS_HOLD_LOT_CNT"].Visibility = Visibility.Collapsed;
        //            dgSummary.Columns["QMS_HOLD_LOT_QTY"].Visibility = Visibility.Collapsed;
        //            dgSummary.Columns["QMS_HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;

        //            //dgSummary.Columns["HOLD_LOT_CNT"].Header = new List<string>() { "HOLD", "LOT수" };
        //            //dgSummary.Columns["HOLD_LOT_QTY"].Header = new List<string>() { "HOLD", "재공(Roll)" };
        //            //dgSummary.Columns["HOLD_LOT_QTY2"].Header = new List<string>() { "HOLD", "재공(Lane)" };
        //        }

        //        DataTable dtRqst = new DataTable();
        //        dtRqst.Columns.Add("SUM_DATE", typeof(string));
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("AREAID", typeof(string));
        //        dtRqst.Columns.Add("EQSGID", typeof(string));
        //        dtRqst.Columns.Add("PROCID", typeof(string));
        //        dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
        //        dtRqst.Columns.Add("PRODID", typeof(string));
        //        dtRqst.Columns.Add("MODLID", typeof(string));
        //        dtRqst.Columns.Add("PRJT_NAME", typeof(string));
        //        dtRqst.Columns.Add("MOVE_FLAG", typeof(string));
        //        dtRqst.Columns.Add("SHIP_FLAG", typeof(string));
        //        dtRqst.Columns.Add("RACK_FLAG", typeof(string));            // 대여RACK 제외 추가 [2018-08-24]
        //        dtRqst.Columns.Add("ROLL_SEQ_FLAG", typeof(string));    // 롤프레스 압연 전/후 구분 [2018-08-24]
        //        dtRqst.Columns.Add("LOTTYPE", typeof(string));

        //        DataRow dr = dtRqst.NewRow();

        //        dr["SUM_DATE"] = Util.GetCondition(dtpDate);
        //        dr["LANGID"] = LoginInfo.LANGID;

        //        dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
        //        if (dr["AREAID"].Equals("")) return;

        //        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
        //        dr["PROCID"] = Util.GetCondition(cboProcess);
        //        dr["PRODID"] = Util.GetCondition(txtProdId);
        //        dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
        //        dr["MODLID"] = Util.GetCondition(txtModlId);
        //        dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
        //        dr["MOVE_FLAG"] = chkMovingFlag.IsChecked == true ? "Y" : null;
        //        dr["SHIP_FLAG"] = chkShipFlag.IsChecked == true ? "Y" : null;
        //        dr["RACK_FLAG"] = null; //chkExceptRack.IsChecked == true ? "Y" : null;
        //        dr["ROLL_SEQ_FLAG"] = chkRollPressSeq.IsChecked == true ? "Y" : null;
        //        dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();

        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

        //        Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
        //        Util.gridClear(dgLotList);

        //        if (dtRslt.Rows.Count > 0)
        //        {
        //            string[] sColumnName = new string[] { "PROCNAME", "MODLID" };
        //            _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

        //            dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
        //            dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MODLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRDT_CLSS_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //    finally
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //    }
        //}

        /// <summary>
        /// 재공현황 합계조회
        /// </summary>
        private void GetStock()
        {
            try
            {
                string sBizName = string.Empty;
                sBizName = "DA_PRD_SEL_FN_SFC_WIP_FOR_TRGT_LIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("P_LANGID", typeof(string));
                dtRqst.Columns.Add("P_WIP_TYPE", typeof(string)); //WIP, WIP_SNAP
                dtRqst.Columns.Add("P_WIP_DETL_TYPE", typeof(string)); //SUM, LOT, SCM
                dtRqst.Columns.Add("P_UI_TYPE", typeof(string)); //ELTR, CELL, PACK, WH, ELTR_WH, FORM, ELTR_WH_SHIP, SCM_ORIG, SCM
                dtRqst.Columns.Add("P_SUM_DATE", typeof(string));
                dtRqst.Columns.Add("P_AREAID", typeof(string));
                dtRqst.Columns.Add("P_WH_ID", typeof(string));
                dtRqst.Columns.Add("P_RACK_ID", typeof(string));
                dtRqst.Columns.Add("P_EQSGID", typeof(string));
                dtRqst.Columns.Add("P_PROCID", typeof(string));
                dtRqst.Columns.Add("P_PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("P_FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("P_PRODID", typeof(string));
                dtRqst.Columns.Add("P_MODLID", typeof(string));
                dtRqst.Columns.Add("P_PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("P_LOTID", typeof(string));
                dtRqst.Columns.Add("P_PKG_LOTID", typeof(string));
                dtRqst.Columns.Add("P_GRP_MKT_TYPE_CODE_FLAG", typeof(string)); //시장유형 구분 집계 (Y : 시장유형 구분, N : 시장유형 미구분)
                dtRqst.Columns.Add("P_GRP_PROD_VER_CODE_FLAG", typeof(string)); //전극버전 구분 집계 (Y : 전극버전 구분, N : 전극버전 미구분)
                dtRqst.Columns.Add("P_GRP_RP_SEQ_FLAG", typeof(string)); //R/P 공정 구분  (Y : R/P 공정 구분 1차E3000/2차이상E3000A, N : R/P 공정 미구분)
                dtRqst.Columns.Add("P_SUM_NO_INSP_FLAG", typeof(string)); //미검사 구분 집계 (Y : 미검사 구분, N : 미검사 미구분)
                dtRqst.Columns.Add("P_SUM_2ND_OCV_FLAG", typeof(string)); //2nd OCV 구분 집계 (Y : 2nd OCV 구분, N : 2nd OCV 미구분)
                dtRqst.Columns.Add("P_SUM_QLTY_HOLD_FLAG", typeof(string)); //품질 Hold 재공 Hold 포함 집계 (Y : 전체홀드에 품질 Hold 포함, N : 전체홀드에 품질 Hold 미포함)
                dtRqst.Columns.Add("P_CHK_RP_N_SEQ_FLAG", typeof(string)); //R/P N차 여부 (A : 전체, Y : 2차 이상, N : 1차)
                dtRqst.Columns.Add("P_CHK_RENTAL_RACK_FLAG", typeof(string)); //대여 Rack (A : 전체, Y : 대여 Rack, N : RACK 없거나 대여 Rack 제외)
                dtRqst.Columns.Add("P_CHK_MOVING_FLAG", typeof(string)); //이동중/출하 재고 Moving 만 조회 (A : 전체, Y : WIPSTAT = MOVING, N : WIPSTAT <> MOVING)
                dtRqst.Columns.Add("P_CHK_RETURN_FLAG", typeof(string)); //반품 (A : 전체, Y : 반품, N : 반품제외)
                dtRqst.Columns.Add("P_CHK_VLD_DATE_OVER_FLAG", typeof(string)); //유효기간 (A : 전체, Y : 유효기간 초과, N : 유효기간 초과 제외)
                dtRqst.Columns.Add("P_CHK_BOX_FLAG", typeof(string)); //포장 (A : 전체, Y : BOX 만, N : BOX 제외)
                dtRqst.Columns.Add("P_CHK_PROC_FLAG", typeof(string)); //WIP상태 PROC (A : 전체, Y : WIPSTAT = PROC, N : WIPSTAT <> PROC)
                dtRqst.Columns.Add("P_CHK_NO_INSP_FLAG", typeof(string)); //미검사 (A : 전체, Y : 미검사만, N : 미검사 제외)
                dtRqst.Columns.Add("P_CHK_BIZWF_FLAG", typeof(string)); //WIP상태 BIZWF (A : 전체, Y : WIPSTAT = BIZWF, N : WIPSTAT <> BIZWF)
                dtRqst.Columns.Add("P_CHK_HOLD_FLAG", typeof(string)); //보류 (A : 전체, Y : 보류, N : 보류 제외)
                dtRqst.Columns.Add("P_CHK_PILOT_FLAG", typeof(string)); //시생산 (A : 전체, Y : 시생산, N : 양산)

                DataRow dr = dtRqst.NewRow();

                dr["P_LANGID"] = LoginInfo.LANGID;
                dr["P_WIP_TYPE"]= rdoWIP.IsChecked == true ? "WIP" : "WIP_SNAP";
                dr["P_WIP_DETL_TYPE"] = "SUM";
                dr["P_SUM_DATE"] = Util.GetCondition(dtpDate);
                dr["P_AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["P_AREAID"].Equals("")) return;
                dr["P_UI_TYPE"] = dr["P_AREAID"].ToString().Substring(0, 1) == "A" ? "CELL" : "ELTR";
                dr["P_WH_ID"] = null;
                dr["P_RACK_ID"] = null;
                // 2023.11.03 윤지해 CSRID E20231014-001517 라인 멀티 처리
                //dr["P_EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["P_EQSGID"] = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") ? Util.GetCondition(cboEquipmentSegment) : cboEquipmentSegmentMulti.SelectedItemsToString;
                dr["P_PROCID"] = Util.GetCondition(cboProcess);
                dr["P_PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType); 
                dr["P_FORM_WRK_TYPE_CODE"] = null;
                dr["P_PRODID"] = Util.GetCondition(txtProdId);
                dr["P_MODLID"] = Util.GetCondition(txtModlId);
                dr["P_PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["P_LOTID"] = null;
                dr["P_PKG_LOTID"] = null;
                dr["P_GRP_MKT_TYPE_CODE_FLAG"] = null;
                dr["P_GRP_PROD_VER_CODE_FLAG"] = null;
                dr["P_GRP_RP_SEQ_FLAG"] = null;
                dr["P_SUM_NO_INSP_FLAG"] = chkQmsSum.IsChecked == true ? "Y" : "N"; 
                dr["P_SUM_2ND_OCV_FLAG"] = null;
                dr["P_SUM_QLTY_HOLD_FLAG"] = chkQltyHold.IsChecked == true ? "Y" : "N";
                dr["P_CHK_RP_N_SEQ_FLAG"] = "A";
                dr["P_CHK_RENTAL_RACK_FLAG"] = "A";
                dr["P_CHK_MOVING_FLAG"] = "A";
                dr["P_CHK_RETURN_FLAG"] = "A";
                dr["P_CHK_VLD_DATE_OVER_FLAG"] = "A";
                dr["P_CHK_BOX_FLAG"] = "A";
                dr["P_CHK_PROC_FLAG"] = "A";
                dr["P_CHK_NO_INSP_FLAG"] = "A";
                dr["P_CHK_BIZWF_FLAG"] = "A";
                dr["P_CHK_HOLD_FLAG"] = "A";
                switch (cboProductDiv.SelectedIndex)
                {
                    case 0:
                        dr["P_CHK_PILOT_FLAG"] = "A";
                        break;
                    case 1:
                        dr["P_CHK_PILOT_FLAG"] = "N";
                        break;
                    case 2:
                        dr["P_CHK_PILOT_FLAG"] = "Y";
                        break;
                    case 3:
                        dr["P_CHK_PILOT_FLAG"] = "L";
                        break;
                    default:
                        dr["P_CHK_PILOT_FLAG"] = "A";
                        break;
                }


                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                //dr["P_PROCID"] = Util.GetCondition(cboProcess);
                //dr["P_PRODID"] = Util.GetCondition(txtProdId);
                //dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                //dr["MODLID"] = Util.GetCondition(txtModlId);
                //dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                //dr["MOVE_FLAG"] = chkMovingFlag.IsChecked == true ? "Y" : null;
                //dr["SHIP_FLAG"] = chkShipFlag.IsChecked == true ? "Y" : null;
                //dr["RACK_FLAG"] = chkQltyHold.IsChecked == true ? "Y" : null;
                //dr["ROLL_SEQ_FLAG"] = chkRollPressSeq.IsChecked == true ? "Y" : null;
                //dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);

                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROCNAME", "MODLID" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MODLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODNAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRDT_CLSS_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MKT_TYPE_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MKT_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODUCTION_TYPE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QLTY_NO_INSP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QLTY_NO_INSP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QLTY_NO_INSP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["BIZWF_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["BIZWF_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["BIZWF_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_OUTSIDE_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_OUTSIDE_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_OUTSIDE_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_CONVEYOR_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_CONVEYOR_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_OUT_CONVEYOR_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_OUTSIDE_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_OUTSIDE_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_OUTSIDE_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_CONVEYOR_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_CONVEYOR_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_IN_CONVEYOR_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_AREA_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHOP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHOP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHOP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHIP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHIP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_SHIP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 재공현황 상세조회
        /// </summary>
        /// <param name="sProcId"></param>
        /// <param name="sProdId"></param>
        private void GetDetailLot(string sProcId, string sProdId, string sPILOT_FLAG = null)
        {
            try
            {
                string sBizName = string.Empty;
                sBizName = "DA_PRD_SEL_FN_SFC_WIP";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("P_LANGID", typeof(string));
                dtRqst.Columns.Add("P_WIP_TYPE", typeof(string)); //WIP, WIP_SNAP
                dtRqst.Columns.Add("P_WIP_DETL_TYPE", typeof(string)); //SUM, LOT, SCM
                dtRqst.Columns.Add("P_UI_TYPE", typeof(string)); //ELTR, CELL, PACK, WH, ELTR_WH, FORM, ELTR_WH_SHIP, SCM_ORIG, SCM
                dtRqst.Columns.Add("P_SUM_DATE", typeof(string));
                dtRqst.Columns.Add("P_AREAID", typeof(string));
                dtRqst.Columns.Add("P_WH_ID", typeof(string));
                dtRqst.Columns.Add("P_RACK_ID", typeof(string));
                dtRqst.Columns.Add("P_EQSGID", typeof(string));
                dtRqst.Columns.Add("P_PROCID", typeof(string));
                dtRqst.Columns.Add("P_PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("P_FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("P_PRODID", typeof(string));
                dtRqst.Columns.Add("P_MODLID", typeof(string));
                dtRqst.Columns.Add("P_PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("P_LOTID", typeof(string));
                dtRqst.Columns.Add("P_PKG_LOTID", typeof(string));
                dtRqst.Columns.Add("P_GRP_MKT_TYPE_CODE_FLAG", typeof(string)); //시장유형 구분 집계 (Y : 시장유형 구분, N : 시장유형 미구분)
                dtRqst.Columns.Add("P_GRP_PROD_VER_CODE_FLAG", typeof(string)); //전극버전 구분 집계 (Y : 전극버전 구분, N : 전극버전 미구분)
                dtRqst.Columns.Add("P_GRP_RP_SEQ_FLAG", typeof(string)); //R/P 공정 구분  (Y : R/P 공정 구분 1차E3000/2차이상E3000A, N : R/P 공정 미구분)
                dtRqst.Columns.Add("P_SUM_NO_INSP_FLAG", typeof(string)); //미검사 구분 집계 (Y : 미검사 구분, N : 미검사 미구분)
                dtRqst.Columns.Add("P_SUM_2ND_OCV_FLAG", typeof(string)); //2nd OCV 구분 집계 (Y : 2nd OCV 구분, N : 2nd OCV 미구분)
                dtRqst.Columns.Add("P_SUM_QLTY_HOLD_FLAG", typeof(string)); //품질 Hold 재공 Hold 포함 집계 (Y : 전체홀드에 품질 Hold 포함, N : 전체홀드에 품질 Hold 미포함)
                dtRqst.Columns.Add("P_CHK_RP_N_SEQ_FLAG", typeof(string)); //R/P N차 여부 (A : 전체, Y : 2차 이상, N : 1차)
                dtRqst.Columns.Add("P_CHK_RENTAL_RACK_FLAG", typeof(string)); //대여 Rack (A : 전체, Y : 대여 Rack, N : RACK 없거나 대여 Rack 제외)
                dtRqst.Columns.Add("P_CHK_MOVING_FLAG", typeof(string)); //이동중/출하 재고 Moving 만 조회 (A : 전체, Y : WIPSTAT = MOVING, N : WIPSTAT <> MOVING)
                dtRqst.Columns.Add("P_CHK_RETURN_FLAG", typeof(string)); //반품 (A : 전체, Y : 반품, N : 반품제외)
                dtRqst.Columns.Add("P_CHK_VLD_DATE_OVER_FLAG", typeof(string)); //유효기간 (A : 전체, Y : 유효기간 초과, N : 유효기간 초과 제외)
                dtRqst.Columns.Add("P_CHK_BOX_FLAG", typeof(string)); //포장 (A : 전체, Y : BOX 만, N : BOX 제외)
                dtRqst.Columns.Add("P_CHK_PROC_FLAG", typeof(string)); //WIP상태 PROC (A : 전체, Y : WIPSTAT = PROC, N : WIPSTAT <> PROC)
                dtRqst.Columns.Add("P_CHK_NO_INSP_FLAG", typeof(string)); //미검사 (A : 전체, Y : 미검사만, N : 미검사 제외)
                dtRqst.Columns.Add("P_CHK_BIZWF_FLAG", typeof(string)); //WIP상태 BIZWF (A : 전체, Y : WIPSTAT = BIZWF, N : WIPSTAT <> BIZWF)
                dtRqst.Columns.Add("P_CHK_HOLD_FLAG", typeof(string)); //보류 (A : 전체, Y : 보류, N : 보류 제외)
                dtRqst.Columns.Add("P_CHK_PILOT_FLAG", typeof(string)); //시생산 (A : 전체, Y : 시생산, N : 양산)
                
                DataRow dr = dtRqst.NewRow();

                dr["P_LANGID"] = LoginInfo.LANGID;
                dr["P_WIP_TYPE"] = rdoWIP.IsChecked == true ? "WIP" : "WIP_SNAP";
                dr["P_WIP_DETL_TYPE"] = "LOT";
                dr["P_SUM_DATE"] = Util.GetCondition(dtpDate);
                dr["P_AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["P_AREAID"].Equals("")) return;
                dr["P_UI_TYPE"] = GetUitype(dr["P_AREAID"].ToString()); //== "A" ? "CELL" : "ELTR";
                dr["P_WH_ID"] = null;
                dr["P_RACK_ID"] = null;
                // 2023.11.03 윤지해 CSRID E20231014-001517 라인 멀티 처리
                //dr["P_EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["P_EQSGID"] = LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") ? Util.GetCondition(cboEquipmentSegment) : cboEquipmentSegmentMulti.SelectedItemsToString;
                //dr["P_PROCID"] = Util.GetCondition(cboProcess);
                dr["P_PROCID"] = sProcId == "" ? null : sProcId;
                dr["P_PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["P_FORM_WRK_TYPE_CODE"] = null;
                dr["P_PRODID"] = sProdId == "" ? null : sProdId;
                dr["P_MODLID"] = Util.GetCondition(txtModlId);
                dr["P_PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["P_LOTID"] = null;
                dr["P_PKG_LOTID"] = null;
                dr["P_GRP_MKT_TYPE_CODE_FLAG"] = null;
                dr["P_GRP_PROD_VER_CODE_FLAG"] = null;
                dr["P_GRP_RP_SEQ_FLAG"] = null;
                dr["P_SUM_NO_INSP_FLAG"] = chkQmsSum.IsChecked == true ? "Y" : "N"; ;
                dr["P_SUM_2ND_OCV_FLAG"] = null;
                dr["P_SUM_QLTY_HOLD_FLAG"] = chkQltyHold.IsChecked == true ? "Y" : "N";
                dr["P_CHK_RP_N_SEQ_FLAG"] = "A";
                dr["P_CHK_RENTAL_RACK_FLAG"] = "A";
                dr["P_CHK_MOVING_FLAG"] = "A";
                dr["P_CHK_RETURN_FLAG"] = "A";
                dr["P_CHK_VLD_DATE_OVER_FLAG"] = "A";
                dr["P_CHK_BOX_FLAG"] = "A";
                dr["P_CHK_PROC_FLAG"] = "A";
                dr["P_CHK_NO_INSP_FLAG"] = "A";
                dr["P_CHK_BIZWF_FLAG"] = "A";
                dr["P_CHK_HOLD_FLAG"] = "A";

                if (string.IsNullOrEmpty(sPILOT_FLAG))
                {
                    switch (cboProductDiv.SelectedIndex)
                    {
                        case 0:
                            sPILOT_FLAG = "A";
                            break;
                        case 1:
                            sPILOT_FLAG = "N";
                            break;
                        case 2:
                            sPILOT_FLAG = "Y";
                            break;
                        case 3:
                            sPILOT_FLAG = "L";
                            break;
                        default:
                            sPILOT_FLAG = "A";
                            break;
                    }
                }
                dr["P_CHK_PILOT_FLAG"] = sPILOT_FLAG;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                   
                    Util.GridSetData(dgLotList, searchResult, FrameOperation, true);

                    // 상태 콤보박스 조회 테이블
                    dtLotAll = (dgLotList.ItemsSource as DataView).Table;
                    

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetElec()
        {

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                dtRslt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["S01"].ToString().Equals("E"))
                    {
                        tbElecType.Visibility = Visibility.Visible;
                        cboElecType.Visibility = Visibility.Visible;
                        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Visible;
                        btnRoute.Visibility = Visibility.Visible;
                        dgLotList.Columns["CHK"].Visibility = Visibility.Visible; //2025.06.09 전극 일때 선택 컬럼 보이게 처리
                    }
                    else
                    {
                        tbElecType.Visibility = Visibility.Collapsed;
                        cboElecType.Visibility = Visibility.Collapsed;
                        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;
                        btnRoute.Visibility = Visibility.Collapsed;
                        dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed; //2025.06.09 MES_IT_E_664 조립일 때 상세 데이터 그리드 선택(CHK) 체크박스 컬럼 안보이도록 처리
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 2023.11.03 윤지해 E20231014-001517 활성화 외 라인 멀티 선택 가능하도록 변경
        // 2023.11.03 윤지해 CSRID E20231014-001517 라인 콤보박스
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            try
            {
                cbo.ItemsSource = null;
                cbo.Items.Clear();

                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count == 1 || cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
                else if (dtResult.Rows.Count == 2)
                {
                    cbo.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSegmentCombo(MultiSelectionBox cbo)
        {
            try
            {
                cbo.ItemsSource = null;

                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FORM_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                cboEquipmentSegmentMulti.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboEquipmentSegmentMulti.Check(i);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        // 2023.11.03 윤지해 CSRID E20231014-001517 공정 콤보박스
        private void SetProcessCombo(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO_WITH_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count == 1 || cboProcess.SelectedIndex < 0)
                {
                    cboProcess.SelectedIndex = 0;
                }
                else if (dtResult.Rows.Count == 2)
                {
                    cboProcess.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 2023.11.03 윤지해 CSRID E20231014-001517 라인 콤보박스 선택 추가
        private string SelectEquipmentSegmentMulti()
        {
            string sEqsgID = string.Empty;
            for (int i = 0; i < cboEquipmentSegmentMulti.SelectedItems.Count; i++)
            {
                if (i != cboEquipmentSegmentMulti.SelectedItems.Count - 1)
                {
                    sEqsgID += cboEquipmentSegmentMulti.SelectedItems[i] + ",";
                }
                else
                {
                    sEqsgID += cboEquipmentSegmentMulti.SelectedItems[i];
                }
            }

            return sEqsgID;
        }
        #endregion

        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Visible;
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

        private bool IsQMSHoldQtyAddArea()
        {

            try
            {
                bool bRet = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "QMS_HOLD_QTY_INFO_USE_AREA";
                dr["CMCODE"] = Util.NVC(cboArea.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt?.Rows?.Count > 0)
                    bRet = true;                

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public static string GetUitype(string AreaID)
        {
            // MES 2.0 - LoginInfo.CFG_SYSTEM_TYPE_CODE 사용으로 변경 (이상권 책임 요청)
            
            // UI TYPE : ELTR, CELL, PACK, WH, ELTR_WH, FORM, ELTR_WH_SHIP, SCM_ORIG, SCM 
            string sRet = "";
            //switch (AreaID.ToString().Substring(0, 1))
            //{
            //    case "A":
            //    case "M":
            //    case "S":
            //        sRet = "CELL";
            //        break;
            //    case "E":
            //        sRet = "ELTR";
            //        break;
            //    case "P":
            //        sRet = "PACK";
            //        break;
            //}
            switch (LoginInfo.CFG_SYSTEM_TYPE_CODE)
            {
                case "A":
                    sRet = "CELL";
                    break;
                case "E":
                    sRet = "ELTR";
                    break;
                case "P":
                    sRet = "PACK";
                    break;
            }
            return sRet;
        }

        private void chkQmsSum_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["QLTY_NO_INSP_CNT"].Visibility = Visibility.Visible;
            dgSummary.Columns["QLTY_NO_INSP_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["QLTY_NO_INSP_QTY2"].Visibility = Visibility.Visible;

        }

        private void chkQmsSum_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["QLTY_NO_INSP_CNT"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["QLTY_NO_INSP_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["QLTY_NO_INSP_QTY2"].Visibility = Visibility.Collapsed;
        }
    }

}
