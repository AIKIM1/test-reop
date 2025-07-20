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
  2021.07.15  김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
 
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
    public partial class COM001_081 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        String _PRODID = "";

        public COM001_081()
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

            if (FrameOperation.AUTHORITY.Equals("W"))
            {
                dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
            }
            else
            {
                dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
            }
            //여기까지 사용자 권한별로 버튼 숨기기

            // CWA전극만 QMSHOLD 추가 [2019-09-18]
            if (_Util.IsCommonCodeUse("QMS_HOLD_VISIBLE", LoginInfo.CFG_AREA_ID) == true)
                dgLotList.Columns["QMSHOLD"].Visibility = Visibility.Visible;
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;
            if (cboProcess.Items.Count > 0) cboProcess.SelectedIndex = 0;

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

                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["ROUTE_TO"] = sRouteID;
                            dr["FLOW_TO"] = sFlowID;
                            dr["PROCID_TO"] = sProcId;
                            dr["USERID"] = LoginInfo.USERID;

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
                                if (chkRow[i]["WIPSTAT"].ToString().Equals("PROC"))
                                {
                                    Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                                    return;
                                }

                                DataRow drLot = dtLot.NewRow();
                                drLot["LOTID"] = chkRow[i]["LOTID"].ToString();
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
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")), 
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROD_VER_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "LOTTYPE")));
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
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), 
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTTYPE")));

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),"", null, null);

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

        private void GetDetailLot(string sProcId, string sProdId, string sProdVerCode, string sLotType)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUM_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("RACK_FLAG", typeof(string));            // 대여RACK 제외 추가 [2018-08-24]
                dtRqst.Columns.Add("ROLL_SEQ_FLAG1", typeof(string));   // 롤프레스 압연 전 구분 [2018-08-24]
                dtRqst.Columns.Add("ROLL_SEQ_FLAG2", typeof(string));   // 롤프레스 압연 후 구분 [2018-08-24]
                dtRqst.Columns.Add("LOTTYPE", typeof(string));   

                DataRow dr = dtRqst.NewRow();

                dr["SUM_DATE"] = Util.GetCondition(dtpDate);
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() ==  "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING + "A")) ? Process.ROLL_PRESSING :  sProcId;
                dr["PRODID"] = sProdId == "" ? null : sProdId;
                dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);

                if ((bool)chkProdVerCode.IsChecked)
                    dr["PROD_VER_CODE"] = sProdVerCode;

                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                dr["RACK_FLAG"] = chkExceptRack.IsChecked == true ? "Y" : null;
                dr["ROLL_SEQ_FLAG1"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING)) ? "Y" : null;
                dr["ROLL_SEQ_FLAG2"] = (chkRollPressSeq.IsChecked == true && string.Equals(sProcId, Process.ROLL_PRESSING + "A")) ? "Y"  : null;

                if (string.IsNullOrWhiteSpace(sLotType))
                {
                    dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
                }
                else
                {
                    dr["LOTTYPE"] = sLotType;
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_IN_AREA_SNAP_DETAIL", "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgLotList, searchResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                string sBizName = string.Empty;

                // 조립 QMS HOLD 정보 추가 요청에 따른 분기.
                if (IsQMSHoldQtyAddArea())
                {
                    sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_SNAP_A" : "DA_PRD_SEL_STOCK_IN_AREA_SNAP_A";

                    dgSummary.Columns["QMS_HOLD_LOT_CNT"].Visibility = Visibility.Visible;
                    dgSummary.Columns["QMS_HOLD_LOT_QTY"].Visibility = Visibility.Visible;
                    dgSummary.Columns["QMS_HOLD_LOT_QTY2"].Visibility = Visibility.Visible;

                    //dgSummary.Columns["HOLD_LOT_CNT"].Header = new List<string>() { "MES HOLD", "LOT수" };
                    //dgSummary.Columns["HOLD_LOT_QTY"].Header = new List<string>() { "MES HOLD", "재공(Roll)" };
                    //dgSummary.Columns["HOLD_LOT_QTY2"].Header = new List<string>() { "MES HOLD", "재공(Lane)" };
                }
                else
                {
                    sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_SNAP" : "DA_PRD_SEL_STOCK_IN_AREA_SNAP";

                    dgSummary.Columns["QMS_HOLD_LOT_CNT"].Visibility = Visibility.Collapsed;
                    dgSummary.Columns["QMS_HOLD_LOT_QTY"].Visibility = Visibility.Collapsed;
                    dgSummary.Columns["QMS_HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;

                    //dgSummary.Columns["HOLD_LOT_CNT"].Header = new List<string>() { "HOLD", "LOT수" };
                    //dgSummary.Columns["HOLD_LOT_QTY"].Header = new List<string>() { "HOLD", "재공(Roll)" };
                    //dgSummary.Columns["HOLD_LOT_QTY2"].Header = new List<string>() { "HOLD", "재공(Lane)" };
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUM_DATE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("MOVE_FLAG", typeof(string));
                dtRqst.Columns.Add("SHIP_FLAG", typeof(string));
                dtRqst.Columns.Add("RACK_FLAG", typeof(string));            // 대여RACK 제외 추가 [2018-08-24]
                dtRqst.Columns.Add("ROLL_SEQ_FLAG", typeof(string));    // 롤프레스 압연 전/후 구분 [2018-08-24]
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["SUM_DATE"] = Util.GetCondition(dtpDate);
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["MOVE_FLAG"] = chkMovingFlag.IsChecked == true ? "Y" : null;
                dr["SHIP_FLAG"] = chkShipFlag.IsChecked == true ? "Y" : null;
                dr["RACK_FLAG"] = chkExceptRack.IsChecked == true ? "Y" : null;
                dr["ROLL_SEQ_FLAG"] = chkRollPressSeq.IsChecked == true ? "Y" : null;
                dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();

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

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["QMS_HOLD_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVING_SHOP_LOT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

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
                    }
                    else
                    {
                        tbElecType.Visibility = Visibility.Collapsed;
                        cboElecType.Visibility = Visibility.Collapsed;
                        dgSummary.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;
                        btnRoute.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
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
    }

}
