/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 재공현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.12.12  정문교 : 물류 SKID ID 컬럼 추가
  2019.12.16  정문교 : 조회 조건에 생산 구분 조건 추가
                       조회에 생산 구분 칼럼 추가
  2020.03.27  정문교 : 조회 그리드에 위치구분, 위치, 상세위치구분, 상세위치 칼럼 추가
  2020.04.21  정문교 : CSR[C20200413-000368] GMES 재공현황 Column위치 변경 요청 건 
                       > 재공 상세 Header Row를 Single Row로 변경
  2020.06.18  정문교   C20200610-000491 - 등급 정보 칼럼 추가 QMSHOLD
  2020.08.27  신광희   C20200814-000099 - Hole 재공 Hold 집계 요청 체크 박스 선택 추가 및 Summary 영역에 QMS Hold 재공 수량 포함 
  2020.09.02  이호섭   음영처리 이슈로 음영처리 순서 변경
  2020.09.25  신광희   C20200625-000215 - 코팅라인 컬럼 추가
  2022.03.24  서용호   C20220110-000251 - 슬라이딩 측정값 컬럼 추가
  2023.06.29  이제섭   C20220915-000045 - BLOCK RESULT (MOVE_PROC/MOVE_SHOP/SHIP_PRODUCT) 컬럼 추가 
  2024.04.19  안유수   E20240403-001305 - HOLD_NOTE 컬럼 추가
  2024.05.28  박성진   E20240524-001613 - 사용자 설정 HOLD_NOTE 컬럼 조회되도록 수정
  2024.05.29  안유수   E20240422-000021 - 장기대기재공 컬럼 추가 및 더블 클릭시 장기대기재공을 대상으로 상세 조회탭 조회 기능 추가
  2024.08.07  안유수   E20240524-001631 - 합계 컬럼 부분에서 제품ID, 공정컬럼 부분 더블 클릭 시, 조건없이 해당 동의 전체 재공 조회 기능과 [#] 항목으로 조회되는 부분 제거

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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        String _PRODID = "";

        string sSystemTypeCode = LoginInfo.CFG_SYSTEM_TYPE_CODE; //2023.06.29 이제섭 추가

        public COM001_018()
        {
            InitializeComponent();
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
        public DataTable WIPCOLORLEGEND { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
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

                if (LoginInfo.CFG_AREA_ID.Length > 0 && LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E"))
                {
                    // 전극
                    dgLotList.Columns["COATING_DT"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["COATING_DT"].Visibility = Visibility.Collapsed;
                }

                dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Collapsed;

                // CWA전극만 QMSHOLD 추가 [2019-09-18]
                //if (_Util.IsCommonCodeUse("QMS_HOLD_VISIBLE", LoginInfo.CFG_AREA_ID) == true)
                //    dgLotList.Columns["QMSHOLD"].Visibility = Visibility.Visible;

                if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED")
                {
                    dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                }

                //C20210114 - 000362
                //선투입순서 컬럼 LOTID 뒤에 정렬 요청
                //전극 자동차동만 해당
                if (LoginInfo.CFG_AREA_ID == "E6")
                {
                    //정렬 순서 변경
                    dgLotList.Columns["COATING_DT"].DisplayIndex = 2;
                    dgLotList.Columns["FIRST_ORDER"].DisplayIndex = 3;
                    //선투입순서 표기 활성화
                    dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Visible;
                }
                else
                {
                    //선투입순서 표기 비활성화
                    dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Collapsed;
                }

                if (LoginInfo.CFG_SHOP_ID == "A010" || LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    dgLotList.Columns["PRE_VLD_DATE"].Visibility = Visibility.Visible;
                    //dgLotList.Columns["VLD_DATE"].Header = "VD 후 유효기간";
                    dgLotList.Columns["VLD_DATE"].Header = ObjectDic.Instance.GetObjectName("VD 후 유효기간");
                }
                else
                {
                    dgLotList.Columns["PRE_VLD_DATE"].Visibility = Visibility.Collapsed;
                }

                InitCombo();
                SetElec();

                // 전극 등급 표시여부
                EltrGrdCodeColumnVisible();

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;

            _combo.SetCombo(cboWipStat, CommonCombo.ComboStatus.ALL);

            DataTable dtWipStat = DataTableConverter.Convert(cboWipStat.ItemsSource);

            DataRow drIns = dtWipStat.NewRow();
            drIns["CBO_NAME"] = "HOLD:HOLD";
            drIns["CBO_CODE"] = "HOLD";
            dtWipStat.Rows.Add(drIns);

            cboWipStat.ItemsSource = dtWipStat.Copy().AsDataView();

            //cboWipStat.SelectedValue = "WAIT";

            string selectedWipstat = string.Empty;

            if (LoginInfo.CFG_ETC.Rows.Count > 0)
            {
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_WIPSTAT))
                {
                    selectedWipstat = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_WIPSTAT].ToString();
                }
            }

            if (dtWipStat.Rows.Count > 0)
                cboWipStat.SelectedValue = selectedWipstat;
            else
                cboWipStat.SelectedItem = null;

            cboWipStat.SelectedItemChanged += cboWipStat_SelectedItemChanged;
            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;

            //생산구분
            string[] sFilter2 = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            // 2020.07.06 공정 Interlock - 범례 속성 공통코드로 조회
            SetWipColorLegendCombo();
        }

        #endregion

        #region Event
        private void chkProdVerCode_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

            //if (chkQMSHold?.IsChecked == true)
            //    chkQMSHold.IsChecked = false;
        }

        private void chkProdVerCode_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
        }

        private void chkCollapsed_Checked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["MODLID"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["WAIT_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PROC_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["PROC_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["END_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["END_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["CONV_MOVING_OUT_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_OUT_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["COMMON_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["SHIP_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["SHIP_QTY2"].Visibility = Visibility.Collapsed;

            dgSummary.Columns["SUM_QTY"].Visibility = Visibility.Collapsed;
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Collapsed;


        }

        private void chkCollapsed_Unchecked(object sender, RoutedEventArgs e)
        {
            dgSummary.Columns["MODLID"].Visibility = Visibility.Visible;
            dgSummary.Columns["PRODNAME"].Visibility = Visibility.Visible;
            dgSummary.Columns["WAIT_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["WAIT_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["PROC_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["PROC_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["END_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["END_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_OUT_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["CONV_MOVING_OUT_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_OUT_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["COMMON_MOVING_IN_QTY2"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_MOVING_IN_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["SHIP_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["SHIP_QTY2"].Visibility = Visibility.Visible;

            dgSummary.Columns["SUM_QTY"].Visibility = Visibility.Visible;
            dgSummary.Columns["SUM_QTY2"].Visibility = Visibility.Visible;
        }

        private void chkQMSHold_Checked(object sender, RoutedEventArgs e)
        {
            //if (chkProdVerCode?.IsChecked == true)
            //    chkProdVerCode.IsChecked = false;
        }

        private void chkQMSHold_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chkAvlDays_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Columns["AVL_DAYS"].Visibility = Visibility.Visible;
        }

        private void chkAvlDays_Unchecked(object sender, RoutedEventArgs e)
        {
            dgLotList.Columns["AVL_DAYS"].Visibility = Visibility.Collapsed;
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
                    string sWipTypeCode = window.WIP_TYPE;

                    //변경하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "변경하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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
                                        if (chkRow[i]["WIPSTAT"].ToString().Equals("PROC"))
                                        {
                                            Util.MessageValidation("SFU1917");  //진행중인 LOT이 있습니다.
                                            return;
                                        }

                                        if (!string.IsNullOrEmpty(chkRow[i]["ABNORM_FLAG"].ToString()) && chkRow[i]["ABNORM_FLAG"].ToString().Equals("Y"))
                                        {
                                            Util.MessageValidation("101255", chkRow[i]["LOTID"].ToString());  //LOT [%1]은 전수불량레인이 존재하여 이동 불가합니다.
                                            return;
                                        }

                                        DataRow drLot = dtLot.NewRow();
                                        drLot["LOTID"] = chkRow[i]["LOTID"].ToString();
                                        dtLot.Rows.Add(drLot);
                                    }

                                    try
                                    {
                                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_CHANGE_ROUTE", "INDATA,INLOT", null, inData);

                                        Util.AlertInfo("SFU1166");  //변경되었습니다.
                                        cboWipStat_SelectedItemChanged(null, null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
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
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "LOTTYPE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODUCT_LEVEL2_CODE")));
            }
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();
        }

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (dgSummary.ItemsSource == null || dgSummary.Rows.Count < 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                //link 색변경
                if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else if (e.Cell.Column.Name.Equals("LONG_TERM_WAIT_CNT"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "PRODID"))) 
                     && int.Parse(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LONG_TERM_WAIT_CNT"))) > 0)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }
                else
                {
                    if (!e.Cell.Column.Name.Equals("LONG_TERM_WAIT_CNT"))
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));

            
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgSummary.ItemsSource == null || dgSummary.Rows.Count < 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null
                    && !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"))))
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTTYPE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODUCT_LEVEL2_CODE")));

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null
                         && !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID"))))
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), "", null, null, null);

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                else if (dg.CurrentColumn.Name.Equals("LONG_TERM_WAIT_CNT") && dg.GetRowCount() > 0 && dg.CurrentRow != null
                         && !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")))
                         && !string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")))
                         && (int.Parse(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LONG_TERM_WAIT_CNT"))) > 0))
                {
                    GetDetailLotForLW(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROD_VER_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTTYPE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODUCT_LEVEL2_CODE")));

                    _PRODID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PRODID"));
                }
                
                else if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    TextBlock textBlock = e.OriginalSource as TextBlock;

                    if (textBlock != null && !string.IsNullOrEmpty(textBlock.Text))
                    {
                        DataGridCellPresenter cellPresenter = textBlock.Parent as DataGridCellPresenter;

                        if (cellPresenter != null && string.Equals(cellPresenter.Column.Name, "PRJT_NAME"))
                        {
                            LGC.GMES.MES.CMM001.CMM_ELEC_PRDT_GPLM window = new CMM_ELEC_PRDT_GPLM();
                            window.FrameOperation = FrameOperation;
                            if (window != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = DataTableConverter.GetValue(cellPresenter.Row.DataItem, "PRODID");

                                //if (string.Equals(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID"), Process.COATING)))
                                if (string.Equals(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), Process.COATING))
                                    Parameters[1] = Gplm_Process_Type.COATING;
                                else
                                    Parameters[1] = Gplm_Process_Type.RTS;

                                C1WindowExtension.SetParameters(window, Parameters);

                                this.Dispatcher.BeginInvoke(new Action(() => window.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //loadingIndicator.Visibility=Visibility.Collapsed;
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                {
                    return;
                }

                if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                {
                    return;
                }

                // 2020.07.07 공정 Interlock - 범례 표시 공정이 아닌 경우 보완 
                if (WIPCOLORLEGEND == null)
                    return;

                // 2020.07.06 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 기능 추가
                SolidColorBrush scbHoldBack = new SolidColorBrush();
                SolidColorBrush scbHoldFore = new SolidColorBrush();

                SolidColorBrush scbZVersionBack = new SolidColorBrush();
                SolidColorBrush scbZVersionFore = new SolidColorBrush();

                SolidColorBrush scbJudgeBack = new SolidColorBrush();
                SolidColorBrush scbJudgeFore = new SolidColorBrush();

                SolidColorBrush scbPastDayBack = new SolidColorBrush();
                SolidColorBrush scbPastDayFore = new SolidColorBrush();

                foreach (DataRow dr in WIPCOLORLEGEND.Rows)
                {
                    if (dr["COLOR_BACK"].ToString().IsNullOrEmpty() || dr["COLOR_FORE"].ToString().IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (dr["CODE"].ToString().Equals("Z_VER"))
                    {
                        scbZVersionBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbZVersionFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                    else if (dr["CODE"].ToString().Equals("HOLD"))
                    {
                        scbHoldBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbHoldFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                    else if (dr["CODE"].ToString().Equals("JUDGE"))
                    {
                        scbJudgeBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbJudgeFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                    else if (dr["CODE"].ToString().Equals("PAST_DAY"))
                    {
                        scbPastDayBack = new BrushConverter().ConvertFromString(dr["COLOR_BACK"].ToString()) as SolidColorBrush;
                        scbPastDayFore = new BrushConverter().ConvertFromString(dr["COLOR_FORE"].ToString()) as SolidColorBrush;
                    }
                }

                //dgLotList.Columns["VLD_DAY"].Visibility = Visibility.Visible;

                //20200902 전극 재공현황조회 음영처리 수정
                //if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                //{
                //    e.Cell.Presenter.Background = scbHoldBack;
                //    e.Cell.Presenter.Foreground = scbHoldFore;
                //}

                if (dgLotList.Columns["QMSHOLD"].Visibility == Visibility.Visible && (LoginInfo.CFG_AREA_ID == "EB" || LoginInfo.CFG_AREA_ID == "ED"))
                {
                    if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QMSHOLD")).ToString() != "PASS")
                    {
                        e.Cell.Presenter.Background = scbHoldBack;
                        e.Cell.Presenter.Foreground = scbHoldFore;
                    }
                }
                //20200902 전극 재공현황조회 음영처리 순서 수정
                else if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                {
                    e.Cell.Presenter.Background = scbHoldBack;
                    e.Cell.Presenter.Foreground = scbHoldFore;
                }
                //20210104 JUDG_FLAG -> QMSHOLD 변경 표준화 작업 처리
                //else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["JUDG_FLAG"] != null && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JUDG_FLAG")).Equals("FAIL"))
                //{
                //    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                //    e.Cell.Presenter.Background = scbJudgeBack;
                //    e.Cell.Presenter.Foreground = scbJudgeFore;
                //}
                else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PAST_DAY"] != null &&
                ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns.Contains("PAST_DAY") &&
                    Convert.ToInt32(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY")) > 30 && LoginInfo.CFG_SHOP_ID.ToString() != "A041")
                {
                    e.Cell.Presenter.Background = scbPastDayBack;
                    e.Cell.Presenter.Foreground = scbPastDayFore;
                }
                // 2020.06.23 공정 Interlock - 4M 검증 Sample 전극버전(Z로 같이 사용)의 경우 녹색으로 표시 요청
                else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["PROD_VER_CODE"] != null &&
                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Length >= 1 &&
                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_VER_CODE")).Substring(0, 1).Equals("Z"))
                {
                    e.Cell.Presenter.Background = scbZVersionBack;
                    e.Cell.Presenter.Foreground = scbZVersionFore;
                }


                if (e.Cell.Column == dgLotList.Columns["WIPHOLD"] && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")) == "Y")
                {
                    //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    e.Cell.Presenter.FontSize = 14;
                }
                else if (e.Cell.Column.Name.Equals("LOTID") && ((DataRowView)e.Cell.Row.DataItem).DataView.Table.Columns["VLD_DAY"] != null &&
                         Convert.ToInt32(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VLD_DAY")) <= 30)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Cell.Presenter.Foreground = dgLotList.Foreground;
                    e.Cell.Presenter.FontWeight = dgLotList.FontWeight;
                    e.Cell.Presenter.FontSize = dgLotList.FontSize;
                }

                //선투입순서 대상 LOT 색상표기
                if (LoginInfo.CFG_AREA_ID == "E6")
                {
                    if (e.Cell.Column.Name.Equals("FIRST_ORDER") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOT")) == "Y")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FST_ORDER_YN")) == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = dgLotList.Foreground;
                        e.Cell.Presenter.FontWeight = dgLotList.FontWeight;
                    }
                }
            }));
        }

        private void GetDetailLot(string sProcId, string sProdId, string sProdVerCode, string sLotType, string sProdLevel2Code = null)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("QMS_FLAG", typeof(string));
                dtRqst.Columns.Add("SYSTEM_TYPE_CODE", typeof(string)); // 2023.06.29 이제섭 추가

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = sProcId;
                dr["PRODID"] = sProdId == "" ? null : sProdId;
                dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);
                dr["SYSTEM_TYPE_CODE"] = sSystemTypeCode; // 2023.06.29 이제섭 추가 

                if ((bool)chkProdVerCode.IsChecked)
                    dr["PROD_VER_CODE"] = sProdVerCode;

                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                //E20240524-001613 사용자설정 항목 추가를 위해 추가
                dgLotList.Columns["HOLD_NOTE"].Visibility = Visibility.Collapsed;

                if (string.IsNullOrWhiteSpace(sLotType))
                {
                    dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
                }
                else
                {
                    dr["LOTTYPE"] = sLotType;
                }
                if (chkQMSHold?.IsChecked == true)
                {
                    dr["QMS_FLAG"] = "Y";
                }
                else
                {
                    dr["QMS_FLAG"] = "N";
                }

                dtRqst.Rows.Add(dr);

                // 2020.10.12 C20200721-000237 Foil ID 추가
                // 2021.04.19 C20210412-000407 슬리터 공정도 추가
                if (sProcId.Equals(Process.ROLL_PRESSING) || sProcId.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Collapsed;
                }

                string bizRuleName;

                dgLotList.Columns["HOLD_NOTE"].Visibility = Visibility.Collapsed;

                if (string.Equals(sProcId, Process.ASSEMBLY)) // A3000
                {
                    bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V01_MOBILE";
                    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Collapsed;

                    // 전극창고 PANCAKE QMS정보가 필요하다고 하여 BIZ 분리
                    //2022-12-29 오화백  동 :EP 추가 
                    if ((string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP")) && (string.Equals(sProcId, Process.ELEC_STORAGE) || string.IsNullOrEmpty(sProcId)))
                    {
                        if (string.IsNullOrEmpty(sProdId))
                        {
                            bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_INNER_QMS_T1";
                        }
                        else
                        {
                            bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_INNER_QMS_V02";
                        }
                    } 
                    else
                    {
                        dgLotList.Columns["HOLD_NOTE"].Visibility = Visibility.Visible;

                        if (string.IsNullOrEmpty(sProdId))
                        {
                            bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO";
                        }
                        else
                        {
                            bizRuleName = "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V01";
                        }
                    }

                    //공정 클릭시는 선투입순서 컬럼 비활성화
                    //전극 자동차 1동만 적용
                    if (LoginInfo.CFG_AREA_ID == "E6")
                    {
                        if (string.IsNullOrEmpty(sProdId))
                            dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Collapsed;
                        else 
                            dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Visible;
                    }

                    // //2023.06.29 이제섭 전극 시스템 아닐 시, 컬럼 비활성화
                    if (sSystemTypeCode != "E")
                    {
                        dgLotList.Columns["MOVE_PROC_JUDG_FLAG"].Visibility = Visibility.Visible;
                        dgLotList.Columns["MOVE_SHOP_JUDG_FLAG"].Visibility = Visibility.Visible;
                        dgLotList.Columns["SHIP_PRODUCT_JUDG_FLAG"].Visibility = Visibility.Visible;
                    }
                }

                if (LoginInfo.CFG_AREA_ID == "A5" || LoginInfo.CFG_AREA_ID == "A6" || LoginInfo.CFG_AREA_ID == "A7")
                {
                    if (sProcId.Equals(Process.NOTCHING))
                        dgLotList.Columns["RECE_DATE"].Visibility = Visibility.Visible;
                    else
                        dgLotList.Columns["RECE_DATE"].Visibility = Visibility.Collapsed;
                }


                //dtRslt = new ClientProxy().ExecuteServiceSync(string.IsNullOrEmpty(sProdId) ? "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO" : "DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V01", "INDATA", "OUTDATA", dtRqst);
                //dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                if (sProcId.Equals(Process.NOTCHING))
                {
                    if (string.IsNullOrEmpty(sProdLevel2Code) || sProdLevel2Code.Equals("PC"))
                    {
                        dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
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
                Util.MessageException(ex);
            }
        }

        private void GetDetailLotForLW(string sProcId, string sProdId, string sProdVerCode, string sLotType, string sProdLevel2Code = null)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("PROD_VER_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = sProcId;
                dr["PRODID"] = sProdId == "" ? null : sProdId;
                dr["WIPSTAT"] = Util.GetCondition(cboWipStat).Trim() == "" ? null : Util.GetCondition(cboWipStat);
                dr["SYSTEM_TYPE_CODE"] = sSystemTypeCode;

                if ((bool)chkProdVerCode.IsChecked)
                    dr["PROD_VER_CODE"] = sProdVerCode;

                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                if (string.IsNullOrWhiteSpace(sLotType))
                {
                    dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
                }
                else
                {
                    dr["LOTTYPE"] = sLotType;
                }

                dtRqst.Rows.Add(dr);

                // 2020.10.12 C20200721-000237 Foil ID 추가
                // 2021.04.19 C20210412-000407 슬리터 공정도 추가
                if (sProcId.Equals(Process.ROLL_PRESSING) || sProcId.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Collapsed;
                }

                if (string.Equals(sProcId, Process.ASSEMBLY)) // A3000
                {
                    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["LOTID_WN"].Visibility = Visibility.Collapsed;

                    //공정 클릭시는 선투입순서 컬럼 비활성화
                    //전극 자동차 1동만 적용
                    if (LoginInfo.CFG_AREA_ID == "E6")
                    {
                        if (string.IsNullOrEmpty(sProdId))
                            dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Collapsed;
                        else
                            dgLotList.Columns["FIRST_ORDER"].Visibility = Visibility.Visible;
                    }

                    // //2023.06.29 이제섭 전극 시스템 아닐 시, 컬럼 비활성화
                    if (sSystemTypeCode != "E")
                    {
                        dgLotList.Columns["MOVE_PROC_JUDG_FLAG"].Visibility = Visibility.Visible;
                        dgLotList.Columns["MOVE_SHOP_JUDG_FLAG"].Visibility = Visibility.Visible;
                        dgLotList.Columns["SHIP_PRODUCT_JUDG_FLAG"].Visibility = Visibility.Visible;
                    }
                }

                if (LoginInfo.CFG_AREA_ID == "A5" || LoginInfo.CFG_AREA_ID == "A6" || LoginInfo.CFG_AREA_ID == "A7")
                {
                    if (sProcId.Equals(Process.NOTCHING))
                        dgLotList.Columns["RECE_DATE"].Visibility = Visibility.Visible;
                    else
                        dgLotList.Columns["RECE_DATE"].Visibility = Visibility.Collapsed;
                }


                if (sProcId.Equals(Process.NOTCHING))
                {
                    if (string.IsNullOrEmpty(sProdLevel2Code) || sProdLevel2Code.Equals("PC"))
                    {
                        dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgLotList.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_IN_AREA_DETAIL_TO_V02", "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
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
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                if (Util.NVC(cboProcess.SelectedItemsToString) == "")
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                ShowLoadingIndicator();

                string bizRuleName;
                if (chkQMSHold?.IsChecked == true)
                {
                    bizRuleName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_TO_QMS_HOLD" : "DA_PRD_SEL_STOCK_IN_AREA_QMS_HOLD";
                }
                else
                {
                    bizRuleName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_TO" : "DA_PRD_SEL_STOCK_IN_AREA_TO";
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("SYSTEM_TYPE_CODE", typeof(string)); // 2023.06.29 이제섭 추가

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = cboProcess.SelectedItemsToString;
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                dr["MODLID"] = Util.GetCondition(txtModlId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["LOTTYPE"] = cboProductDiv.SelectedValue.ToString() == "" ? null : cboProductDiv.SelectedValue.ToString();
                dr["SYSTEM_TYPE_CODE"] = sSystemTypeCode;  // 2023.06.29 이제섭 추가
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgSummary, searchResult, FrameOperation, true);
                        Util.gridClear(dgLotList);

                        if (searchResult.Rows.Count > 0)
                        {
                            string[] sColumnName = new string[] { "PROCNAME", "MODLID" };
                            _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                            dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
                            dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MODLID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("Total") } });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["LONG_TERM_WAIT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

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

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_OUT_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["COMMON_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["CONV_PLANT_MOVING_IN_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

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
                        HiddenLoadingIndicator();
                    }
                });

                if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_SHOP_ID == "A010" || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    chkAvlDays.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
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

        private void SetWipColorLegendCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTiTle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow inRow = inTable.NewRow();
            inRow["LANGID"] = LoginInfo.LANGID;
            inRow["CMCDTYPE"] = "WIP_COLOR_LEGEND";
            inRow["PROCID"] = "ALL";

            inTable.Rows.Add(inRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtResult.Rows)
            {
                if (row["COLOR_BACK"].ToString().IsNullOrEmpty() || row["COLOR_FORE"].ToString().IsNullOrEmpty())
                {
                    continue;
                }

                C1ComboBoxItem cbItem = new C1ComboBoxItem
                {
                    Content = row["NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["COLOR_BACK"].ToString()) as SolidColorBrush,
                    Foreground = new BrushConverter().ConvertFromString(row["COLOR_FORE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem);
            }
            cboColor.SelectedIndex = 0;

            WIPCOLORLEGEND = dtResult;
        }

        private void EltrGrdCodeColumnVisible()
        {
            try
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
                {
                    dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProcess()
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
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProcess.Check(i);

                    //if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(LoginInfo.CFG_EQSG_ID))
                    //{
                    //    cboProcess.Check(i);
                    //    break;
                    //}
                    //else
                    //    cboProcess.Check(i);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotList);
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgLotList_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dgLotList.GroupBy(dgLotList.Columns["HOLD"]);
            dgLotList.FilterBy(dgLotList.Columns["HOLD"], new DataGridFilterState()
            {
                FilterInfo = new List<DataGridFilterInfo>(
                    new DataGridFilterInfo[1]
                    {
                        new DataGridFilterInfo()
                        {
                            FilterOperation = DataGridFilterOperation.Contains, FilterType = DataGridFilterType.Text, Value = "Y"
                        }
                    })
            });
        }

        private void CheckHold_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null)
                return;
            try
            {

                DataGridFilterState fs = null;
                DataGridFilterInfo fsFilter = null;
                List<DataGridFilterInfo> fsList = null;

                fsFilter = new DataGridFilterInfo();
                fs = new DataGridFilterState();
                fsList = new List<DataGridFilterInfo>();


                fsFilter.FilterType = DataGridFilterType.Text;
                fsFilter.Value = "Y";

                fsList.Add(fsFilter);
                fs.FilterInfo = fsList;
                dgLotList.FilterBy(dgLotList.Columns[15], fs, false); //"WIPHOLD"
                dgLotList.Reload(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckHold_Unhecked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null)
                return;
            try
            {
                DataGridFilterState fs = null;
                DataGridFilterInfo fsFilter = null;
                List<DataGridFilterInfo> fsList = null;

                fsFilter = new DataGridFilterInfo();
                fs = new DataGridFilterState();
                fsList = new List<DataGridFilterInfo>();


                fsFilter.FilterType = DataGridFilterType.Text;
                //fsFilter.Value = "N";

                fsList.Add(fsFilter);
                fs.FilterInfo = fsList;
                dgLotList.FilterBy(dgLotList.Columns[15], fs, false); //"WIPHOLD"
                dgLotList.Reload(false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboProcess();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLotList.ItemsSource == null || dgLotList.Rows.Count < 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("WIPHOLD") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                    || dg.CurrentColumn.Name.Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                {
                    ShowHoldDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
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

        private void ShowHoldDetail(string pLotid)
        {
            COM001_018_HOLD_DETL wndRunStart = new COM001_018_HOLD_DETL();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = pLotid;
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
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
    }
}
