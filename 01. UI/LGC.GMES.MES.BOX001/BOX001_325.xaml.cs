/*************************************************************************************
 Created Date : 2020.12.28
      Creator : 이제섭
   Decription : 포장 출고 대기 Lot 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.28  DEVELOPER : Initial Created.
  2022.01.19        KDH : 합계출력
  2022.11.17     임근영 : xaml 파일에 시장 유형에 대한 필터기능 추가
  2022.12.26     이제섭 : 포장 대기 Lot인 경우 활성화 Lot 관리 화면으로 연계 처리
  2023.02.14     홍석원 : QC 검사 관련 컬럼 헤더 BG 녹색 처리 및 합격이 아닌 경우 빨간색으로 글자 처리
                          적용 컬럼 : 성능검사, 치수검사, 한계불량률 (저전압), 한계불량률 (용량), 한계불량률 (DCIR), 출하검사
  2023.02.14     홍석원 : PALLET정보 Grid 영역 확대 축소 버튼 추가, QC 검사 컬럼 MMD 설정값으로 처리
  2023.05.23     최경아 : LotType 조회정보 추가 
**************************************************************************************/

using C1.WPF;
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
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries; //20220120_합계출력

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_325 : System.Windows.Controls.UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        string sCOLNAME = string.Empty;
        int iCURRENT_ROW = -1;

        // 조회한 수량 저장하기 위한 변수
        private int isQty = 0;
        // 출하 예정일
        string shipdt = "";
        string Shipdate_Schedule = "";

        double _TOTAL_TPQ;
        double _TOTAL_TCQ;
        double _PROC_PPQ;
        double _PROC_PCQ;
        double _EXP_EPQ;
        double _EXP_ECQ;
        double _HOLD_HPQ;
        double _HOLD_HCQ;
        double _OWMS_OPQ;
        double _OWMS_OCQ;
        double _SHIP_WAIT_SPQ;
        double _SHIP_WAIT_SCQ;
        double _PACK_WAIT_SPQ;
        double _PACK_WAIT_SCQ;

        string judgeValueY = string.Empty;
        string ncrOk = string.Empty;
        Style styleGreen;
        Style styleGray;

        public BOX001_325()
        {
            InitializeComponent();
            Loaded += BOX001_325_Loaded;
        }

        private void BOX001_325_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_325_Loaded;

            initSet();
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

        private void initSet()
        {
            // Area 셋팅
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            //타입 Combo Set.
            string[] sFilter5 = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            //포장 Lottype 세팅
            _combo.SetCombo(boxLottype, CommonCombo.ComboStatus.ALL, sCase: "LOTTYPE");

            // UNCODE 사용 Plant는 UN_CODE 컬럼 Visible 처리
            if (UseCommoncodePlant())
            {
                dgPalletInfo.Columns["UN_CODE"].Visibility = Visibility.Visible;
            }

            judgeValueY = Util.NVC(getCmcdName("JUDGE_VALUE", "Y"), "합격");  // JUDGE_VALUE값이 없는 경우 기본값 '합격'
            ncrOk = judgeValueY + " (" + Util.NVC(ObjectDic.Instance.GetObjectName("NCR_OK"), "NCR 종료") + ")"; // NCR_OK 값이 없는 경우 기본값 'NCR 종료'

            styleGreen = new Style(typeof(DataGridColumnHeaderPresenter));
            styleGreen.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(169, 208, 142)) });
            styleGreen.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = System.Windows.HorizontalAlignment.Stretch });
            styleGreen.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = System.Windows.HorizontalAlignment.Center });
            styleGreen.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });

            styleGray = new Style(typeof(DataGridColumnHeaderPresenter));
            styleGray.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(238, 238, 238)) });
            styleGray.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = System.Windows.HorizontalAlignment.Stretch });
            styleGray.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = System.Windows.HorizontalAlignment.Center });
            styleGray.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
        }

        #endregion


        #region Event





        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sAREAID == "" || sAREAID == "SELECT")
            {
                Util.AlertInfo("SFU1499"); //"동을 선택하세요."
                return;
            }

            string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                sEqsgid = null;
            }

            string sMDLLot = Util.NVC(cboModelLot.SelectedValue);
            if (sMDLLot == "" || sMDLLot == "SELECT")
            {
                sMDLLot = null;
            }

            string sLotType = Util.NVC(cboLottype.SelectedValue);
            if (sLotType == "" || sLotType == "SELECT")
            {
                sLotType = null;
            }

            string sBLotType = Util.NVC(boxLottype.SelectedValue);
            if (sBLotType == "" || sBLotType == "SELECT")
            {
                sBLotType = null;
            }
            // 출고대기 LOT Summary 조회
            GetSummaryInfo(sAREAID, sEqsgid, sMDLLot, sLotType, sBLotType);

            dgPalletInfo.ItemsSource = null;
        }
        

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            _comboF.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");


        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sEqsgid = cboEquipmentSegment.SelectedValue.ToString();

            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                SetComboBox(cboModelLot);
            }
            else
            {
                _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboEquipmentSegment });
            }
        }

        private void dgSummary_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgSummary.CurrentRow == null || dgSummary.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    string sChkValue = "";
                    string sAreaid = "";
                    string sEqsgid = "";
                    string sLotType = "";
                    string sBLotType = "";
                    string sProdid = "";
                    string sViewAll = "";
                    string sSchEqsgid = "";
                    string sSchLotId = "";
                    string sSchPackType = "";
                    string sSchLottype = "";
                    string sColName = dgSummary.CurrentColumn.Name;
                    sCOLNAME = sColName;
                    iCURRENT_ROW = dgSummary.CurrentRow.Index;

                    if (sColName == "CHK_TOTAL" || sColName == "CHK_PROC" || sColName == "CHK_OWMS" || sColName == "CHK_EXP" || sColName == "CHK_HOLD" || sColName == "CHK_SHIP_WAIT" || sColName == "CHK_PACK_WAIT")
                    {
                        sChkValue = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns[sColName].Index).Value);
                        sAreaid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["AREAID"].Index).Value);
                        sEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["EQSGID"].Index).Value);
                        sLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                        sBLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["LOTTYPE"].Index).Value);
                        sProdid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PRODID"].Index).Value);
                        sViewAll = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["VIEW_ALL"].Index).Value);
                        sSchEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_EQSGID"].Index).Value);
                        sSchLotId = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_MDLLOT_ID"].Index).Value);
                        sSchPackType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_PACK_WRK_TYPE_CODE"].Index).Value);
                        sSchLottype = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_LOTTYPE"].Index).Value);

                        // 초기화...
                        for (int i = dgSummary.Rows.TopRows.Count; i < dgSummary.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_TOTAL", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_OWMS", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_PROC", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_EXP", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_HOLD", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_SHIP_WAIT", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_PACK_WAIT", false);
                        }

                        if (sChkValue == "0" )
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[dgSummary.CurrentRow.Index].DataItem, sColName, true);
                            GetMMDShipToPackCondAuto(sEqsgid, sProdid);
                            GetQMSBlockBas();
                            GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sColName, sBLotType,  sViewAll, sSchEqsgid, sSchLotId, sSchPackType, sSchLottype);
                        }
                        else
                        {
                            dgPalletInfo.ItemsSource = null;
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
                dgSummary.CurrentRow = null;
            }
        }

        private void dgPalletInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {            
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
               // datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
                return;
            }
            

            string sPalletid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["BOXID"].Index).Value);
            string sLotid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value);

            if (datagrid.CurrentColumn.Name == "BOXID")
            {
                // 포장 대기 Lot인 경우
                if (sPalletid.StartsWith("PW"))
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    string[] sParam = { sPalletid };
                    // 활성화 Lot 관리
                    this.FrameOperation.OpenMenu("SFU010705220", true, sParam);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    string[] sParam = { sAREAID, sSHOPID, sPalletid };
                    // 기간별 Pallet 확정 이력 정보 조회
                    this.FrameOperation.OpenMenu("SFU010736080", true, sParam);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
            else if (datagrid.CurrentColumn.Name == "PACKDTTM")
            {
                BOX001_018_BOX_HIST popUp = new BOX001_018_BOX_HIST();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = sPalletid;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndBoxHist_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //  this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            else if (datagrid.CurrentColumn.Name == "PACK_NOTE")
            {
                BOX001_018_PACK_NOTE popUp = new BOX001_018_PACK_NOTE();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = sPalletid;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackNote_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndPackNote_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_018_PACK_NOTE wndPopup = sender as BOX001.BOX001_018_PACK_NOTE;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //재조회
                string sAreaid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["AREAID"].Index).Value);
                string sEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["EQSGID"].Index).Value);
                string sLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                string sProdid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PRODID"].Index).Value);
                string sBLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["LOTTYPE"].Index).Value);
                string sViewAll = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["VIEW_ALL"].Index).Value);
                string sSchEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_EQSGID"].Index).Value);
                string sSchLotId = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_MDLLOT_ID"].Index).Value);
                string sSchPackType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_PACK_WRK_TYPE_CODE"].Index).Value);
                string sSchLottype = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["SCH_LOTTYPE"].Index).Value);

                GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sCOLNAME, sBLotType, sViewAll, sSchEqsgid, sSchLotId, sSchPackType, sSchLottype);

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndBoxHist_Closed(object sender, EventArgs e)
        {
            BOX001_018_BOX_HIST window = sender as BOX001_018_BOX_HIST;
           
            grdMain.Children.Remove(window);

        }

        private void dgPalletInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // MMD에 설정된 필수 판단 대상 값을 이용하여 처리하도록 수정
            /*
            try
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(169, 208, 142)) });
                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = System.Windows.HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = System.Windows.HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });

                    dgPalletInfo.Columns["PROD_INSP_RESULT"].HeaderStyle = style;
                    dgPalletInfo.Columns["MEASR_INSP_RESULT"].HeaderStyle = style;
                    dgPalletInfo.Columns["LOW_VOLT_INSP_RESULT"].HeaderStyle = style;
                    dgPalletInfo.Columns["CAPA_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = style;
                    dgPalletInfo.Columns["DCIR_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = style;
                    dgPalletInfo.Columns["OQC_INSP_YN"].HeaderStyle = style;
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            */
        }

        private void btnExpend1_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpend1.Content.Equals("↗"))
            {
                btnExpend1.Content = "↙";

                Row1.Height = new GridLength(0);
                Row2.Height = new GridLength(0);
                Row3.Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                btnExpend1.Content = "↗";

                Row1.Height = new GridLength(1, GridUnitType.Star);
                Row2.Height = new GridLength(8);
                Row3.Height = new GridLength(1, GridUnitType.Star);
            }
        }
        #endregion


        #region Mehod

        /// <summary>
        /// 출고대기 LOT Summary 조회
        /// </summary>
        private void GetSummaryInfo(string sAreaid, string sEqsgid, string sMDLLot, string sLotType, string sBLotType)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAreaid;
                dr["EQSGID"] = sEqsgid;
                dr["MDLLOT_ID"] = sMDLLot;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                dr["LOTTYPE"] = sBLotType;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_STOCK_SUMMARY_BX", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgSummary, dtResult, FrameOperation, true);

                    //20220120_합계출력 START
                    int TotalTPQ = 0;
                    int TotalTCQ = 0;
                    int ProcPPQ = 0;
                    int ProcPCQ = 0;
                    int OwmsOPQ = 0;
                    int OwmsOCQ = 0;
                    int ExpEPQ = 0;
                    int ExpECQ = 0;
                    int HoldHPQ = 0;
                    int HoldHCQ = 0;
                    int ShipWaitSPQ = 0;
                    int ShipWaitSCQ = 0;
                    int PackWaitSPQ = 0;
                    int PackWaitSCQ = 0;

                    //if (dtResult.Rows.Count > 0)
                    //{
                        for (int iRow = 0; iRow < dtResult.Rows.Count; iRow++)
                        {
                            TotalTPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["TOTAL_TPQ"])));
                            TotalTCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["TOTAL_TCQ"])));
                            ProcPPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_PPQ"])));
                            ProcPCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_PCQ"])));
                            OwmsOPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["OWMS_OPQ"])));
                            OwmsOCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["OWMS_OCQ"])));
                            ExpEPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["EXP_EPQ"])));
                            ExpECQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["EXP_ECQ"])));
                            HoldHPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HOLD_HPQ"])));
                            HoldHCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HOLD_HCQ"])));
                            ShipWaitSPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP_WAIT_SPQ"])));
                            ShipWaitSCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP_WAIT_SCQ"])));
                            PackWaitSPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PACK_WAIT_SPQ"])));
                            PackWaitSCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PACK_WAIT_SCQ"])));
                        }

                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = TotalTPQ + "(" + TotalTCQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ProcPPQ + "(" + ProcPCQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["OWMS_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = OwmsOPQ + "(" + OwmsOCQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["EXP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ExpEPQ + "(" + ExpECQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = HoldHPQ + "(" + HoldHCQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ShipWaitSPQ + "(" + ShipWaitSCQ + ")" } });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PACK_WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = PackWaitSPQ + "(" + PackWaitSCQ + ")" } });
                    //}
                    //else
                    //{
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["OWMS_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["EXP_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SHIP_WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PACK_WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //}

                    DataTable dtInfo = DataTableConverter.Convert(dgSummary.ItemsSource);
                    dtInfo.Columns.Add("VIEW_ALL", typeof(string));
                    dtInfo.Columns.Add("SCH_EQSGID", typeof(string));
                    dtInfo.Columns.Add("SCH_MDLLOT_ID", typeof(string));
                    dtInfo.Columns.Add("SCH_PACK_WRK_TYPE_CODE", typeof(string));
                    dtInfo.Columns.Add("SCH_LOTTYPE", typeof(string));
                    DataRow drr = dtInfo.NewRow();
                    drr["TOTAL_QTY"] = TotalTPQ + "(" + TotalTCQ + ")";
                    drr["PROC_QTY"] = ProcPPQ + "(" + ProcPCQ + ")";
                    drr["OWMS_QTY"] = OwmsOPQ + "(" + OwmsOCQ + ")";
                    drr["EXP_QTY"] = ExpEPQ + "(" + ExpECQ + ")";
                    drr["HOLD_QTY"] = HoldHPQ + "(" + HoldHCQ + ")";
                    drr["SHIP_WAIT_QTY"] = ShipWaitSPQ + "(" + ShipWaitSCQ + ")";
                    drr["PACK_WAIT_QTY"] = PackWaitSPQ + "(" + PackWaitSCQ + ")";
                    drr["AREAID"] = sAreaid;
                    drr["SCH_EQSGID"] = sEqsgid;
                    drr["SCH_MDLLOT_ID"] = sMDLLot;
                    drr["SCH_PACK_WRK_TYPE_CODE"] = sLotType;
                    drr["SCH_LOTTYPE"] = sBLotType;
                    drr["VIEW_ALL"] = "VIEW_ALL";
                    dtInfo.Rows.Add(drr);

                    if (dtInfo.Rows.Count > 0)
                        dtInfo = dtInfo.DefaultView.ToTable(true);

                    Util.GridSetData(dgSummary, dtInfo, FrameOperation);
                    //20220120_합계출력 END
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex); // Util.AlertInfo(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 출고대기 LOT 조회
        /// </summary>
        private void GetPalletInfo(string sAreaid, string sEqsgid, string sLotType, string sProdid, string sChkType, string sBLotType, string sViewAll, string sSchEqsgid, string sSchLotId, string sSchPackType, string sSchLottype)
        {
            try
            {
                if (sEqsgid == "") sEqsgid = null;
                if (sProdid == "") sProdid = null;
                if (sLotType == "") sLotType = null;
                if (sBLotType == "") sBLotType = null;
                if (sViewAll != "VIEW_ALL") sViewAll = null;
                if (sSchEqsgid == "") sSchEqsgid = null;
                if (sSchLotId == "") sSchLotId = null;
                if (sSchPackType == "") sSchPackType = null;
                if (sSchLottype == "") sSchLottype = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTTYPE", typeof(string));
                RQSTDT.Columns.Add("VIEW_PROC", typeof(string));
                RQSTDT.Columns.Add("VIEW_EXP", typeof(string));
                RQSTDT.Columns.Add("VIEW_HOLD", typeof(string));
                RQSTDT.Columns.Add("VIEW_OWMS", typeof(string));
                RQSTDT.Columns.Add("VIEW_SHIP_WAIT", typeof(string));
                RQSTDT.Columns.Add("VIEW_PACK_WAIT", typeof(string));
                RQSTDT.Columns.Add("VIEW_ALL", typeof(string));
                RQSTDT.Columns.Add("SCH_EQSGID", typeof(string));
                RQSTDT.Columns.Add("SCH_MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("SCH_PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SCH_LOTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAreaid;
                dr["EQSGID"] = sEqsgid;
                dr["PRODID"] = sProdid;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                dr["LOTTYPE"] = sBLotType;
                dr["VIEW_PROC"] = sChkType == "CHK_PROC" ? sChkType : null;
                dr["VIEW_EXP"] = sChkType == "CHK_EXP" ? sChkType : null;
                dr["VIEW_HOLD"] = sChkType == "CHK_HOLD" ? sChkType : null;
                dr["VIEW_OWMS"] = sChkType == "CHK_OWMS" ? sChkType : null;
                dr["VIEW_SHIP_WAIT"] = sChkType == "CHK_SHIP_WAIT" ? sChkType : null;
                dr["VIEW_PACK_WAIT"] = sChkType == "CHK_PACK_WAIT" ? sChkType : null;
                dr["VIEW_ALL"] = sViewAll;
                dr["SCH_EQSGID"] = sSchEqsgid;
                dr["SCH_MDLLOT_ID"] = sSchLotId;
                dr["SCH_PACK_WRK_TYPE_CODE"] = sSchPackType;
                dr["SCH_LOTTYPE"] = sSchLottype;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_STOCK_LIST_ESGM_BX", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation,true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);// Util.AlertInfo(ex.Message);
                return;
            }
        }

        private void GetMMDShipToPackCondAuto(string eqsgId, string prodId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = eqsgId;
                dr["SHIPTO_ID"] = "ALL";
                dr["PRODID"] = prodId;
                RQSTDT.Rows.Add(dr);

                //loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        //성능검사 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["PRDT_INSP_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["PROD_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["PROD_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //치수검사 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["DIMENSION_INSP_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["MEASR_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["MEASR_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //한계불량률(저전압) 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["LOW_VLTG_LIMIT_DFCT_RATE_JUDG_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["LOW_VOLT_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["LOW_VOLT_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //한계불량률(용량) 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["CAPA_LIMIT_DFCTRATE_INSP_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["CAPA_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["CAPA_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //한계불량률(DCIR) 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["DCIR_LIMIT_DFCTRATE_INSP_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["DCIR_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["DCIR_LIMIT_DFCTRATE_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //활성화 불량분석(selector) 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["FORM_SELECTOR_INSP_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["FORM_DFCT_INSP_RESULT"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["FORM_DFCT_INSP_RESULT"].HeaderStyle = styleGray;
                        }

                        //출하검사 컬럼 헤더 스타일 변경
                        if (Util.NVC(Convert.ToString(dtResult.Rows[0]["OQC_INSP_JUDG_FLAG"]), "N") == "Y")
                        {
                            dgPalletInfo.Columns["OQC_INSP_YN"].HeaderStyle = styleGreen;
                        }
                        else
                        {
                            dgPalletInfo.Columns["OQC_INSP_YN"].HeaderStyle = styleGray;
                        }

                        //loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// UNCODE 필수입력 Plant 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodePlant()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLT_UNCODE_SHOP";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string getCmcdName(string commonType, string cmCode)
        {
            string returnCmcdName = string.Empty;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = commonType;
            dr["CMCODE"] = cmCode;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                returnCmcdName = Util.NVC(dtRslt.Rows[0]["CMCDNAME"]);
            }

            return returnCmcdName;
        }

        #endregion

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid datagrid = (sender as C1DataGrid);
          //  datagrid.
           // datagrid.CurrentCell
        }

        private void dgPalletInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "BOXID" || e.Cell.Column.Name == "PACK_NOTE")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else if (e.Cell.Column.Name == "PROD_INSP_RESULT" 
                            || e.Cell.Column.Name == "MEASR_INSP_RESULT"
                            || e.Cell.Column.Name == "LOW_VOLT_INSP_RESULT"
                            || e.Cell.Column.Name == "CAPA_LIMIT_DFCTRATE_INSP_RESULT"
                            || e.Cell.Column.Name == "DCIR_LIMIT_DFCTRATE_INSP_RESULT"
                            || e.Cell.Column.Name == "FORM_DFCT_INSP_RESULT"
                            || e.Cell.Column.Name == "FORM_AGRADE_INSP_RESULT")
                    {
                        if (dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text != judgeValueY)  // 조회된 값이 '합격'이 아닌 경우 글자색 변경 (빨강)
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        }
                    }
                    else if (e.Cell.Column.Name == "OQC_INSP_YN")
                    {
                        string[] arrOqcInspYn = dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text.Split('\n');

                        if (Array.LastIndexOf(arrOqcInspYn, ncrOk) < 0 && Array.LastIndexOf(arrOqcInspYn, judgeValueY) < 0) // 조회된 값에 '합격 (NCR 종료)', '합격'이 없는 경우 글자색 변경 (빨강)
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgPalletInfo_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            //{
            //    datagrid.Selection.Add(datagrid.GetCell(datagrid.FrozenTopRowsCount, 0), datagrid.GetCell(datagrid.Rows.Count - 1, datagrid.Columns.Count - 2));
            //    e.Handled = true;
            //}
        }





        private void dgSummary_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                    if (panel == null && panel.Children == null && panel.Children.Count < 1) return;

                    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    if (e.Cell.Column.Index >= dg.Columns["TOTAL_QTY"].Index && e.Cell.Column.Index <= dg.Columns["HOLD_QTY"].Index)
                    {

                       if (dg.GetRowCount() > 0)
                        {
                           if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                            {
                                if (e.Cell.Column.Name == "TOTAL_QTY")
                                {
                                    presenter.Content =  _TOTAL_TPQ + " (" + _TOTAL_TCQ + ")";
                                }
                                if (e.Cell.Column.Name == "PROC_QTY")
                                {
                                    presenter.Content = _PROC_PPQ + " (" + _PROC_PCQ + ")";
                                }
                                if (e.Cell.Column.Name == "EXP_QTY")
                                {
                                    presenter.Content = _EXP_EPQ + " (" + _EXP_ECQ + ")";
                                }
                                if (e.Cell.Column.Name == "HOLD_QTY")
                                {
                                    presenter.Content = _HOLD_HPQ + " (" + _HOLD_HCQ + ")";
                                }
                                if (e.Cell.Column.Name == "OWMS_QTY")
                                {
                                    presenter.Content = _OWMS_OPQ + " (" + _OWMS_OCQ + ")";
                                }
                                if (e.Cell.Column.Name == "SHIP_WAIT_QTY")
                                {
                                    presenter.Content = _SHIP_WAIT_SPQ + " (" + _SHIP_WAIT_SCQ + ")";
                                }
                                if (e.Cell.Column.Name == "PACK_WAIT_QTY")
                                {
                                    presenter.Content = _PACK_WAIT_SPQ + " (" + _PACK_WAIT_SCQ + ")";
                                }
                            }
                        }

                    }
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VIEW_ALL")).ToString().Equals("VIEW_ALL"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Color.FromRgb(247, 233, 213)); 
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                    e.Cell.Presenter.BorderBrush = new SolidColorBrush(Color.FromRgb(247, 233, 213));
                }
            }));
        }

        private void SetComboBox(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_MDLLOT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = {LoginInfo.LANGID, sAREAID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText);
        }

        private void GetQMSBlockBas()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["INSP_MED_CLSS_CODE"] = "PQCM098";
                RQSTDT.Rows.Add(dr);

                //loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_QMS_BLOCK_FOR_SHIP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        dgPalletInfo.Columns["FORM_AGRADE_INSP_RESULT"].HeaderStyle = styleGreen;
                    }
                    else
                    {
                        dgPalletInfo.Columns["FORM_AGRADE_INSP_RESULT"].HeaderStyle = styleGray;
                    }

                    //loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return;
            }
        }

    }

}
