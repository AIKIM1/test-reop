/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
      DATE            CSR NO            DEVELOPER            DESCRIPTION
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.13                              이제섭            CNB 법인은 활성화 불량분석(Selector) 컬럼 보이고 나머지 법인은 숨김처리.
  2020.07.21                              이제섭            UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2020.10.13                                               변경집합 번호 34689, 2020-08-03일 버젼으로 롤백처리함
  2022.01.18        C20211213-000321      김광오            [오창] QA 리튬석출 검사 신규 도입에 따른 시스템 개발
  2022.04.25        C20220413-000427      김광오            [생산PI] 한계불량률 관리 항목 신규 추가(용량/DC)를 위한 GMES/MMD 시스템 개선
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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using System.Linq;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid.Summaries;



namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

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


        public BOX001_018()
        {
            InitializeComponent();
            Loaded += BOX001_018_Loaded;
        }

        private void BOX001_018_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_018_Loaded;

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

            // CNB만 활성화 불량분석(Selector) Set
            if (LoginInfo.CFG_SHOP_ID.Equals("G631") || LoginInfo.CFG_SHOP_ID.Equals("G634"))
            {
                dgPalletInfo.Columns["FORM_DFCT_INSP_RESULT"].Visibility = Visibility.Visible;
            }

            // UNCODE 사용 Plant는 UN_CODE 컬럼 Visible 처리
            if (UseCommoncodePlant())
            {
                dgPalletInfo.Columns["UN_CODE"].Visibility = Visibility.Visible;
            }
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
            // 출고대기 LOT Summary 조회
            GetSummaryInfo(sAREAID, sEqsgid, sMDLLot, sLotType);

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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");

            isVisibleBCD(sAREAID);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboEquipmentSegment });
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
                    string sProdid = "";
                    string sColName = dgSummary.CurrentColumn.Name;
                    sCOLNAME = sColName;
                    iCURRENT_ROW = dgSummary.CurrentRow.Index;

                    if (sColName == "CHK_TOTAL" || sColName == "CHK_PROC" || sColName == "CHK_OWMS" || sColName == "CHK_EXP" || sColName == "CHK_HOLD")
                    {
                        sChkValue = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns[sColName].Index).Value);
                        sAreaid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["AREAID"].Index).Value);
                        sEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["EQSGID"].Index).Value);
                        sLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                        sProdid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PRODID"].Index).Value);
                        // 초기화...
                        for (int i = dgSummary.Rows.TopRows.Count; i < dgSummary.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_TOTAL", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_OWMS", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_PROC", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_EXP", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_HOLD", false);
                        }

                        if (sChkValue == "0" )
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[dgSummary.CurrentRow.Index].DataItem, sColName, true);
                            GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sColName);
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

                loadingIndicator.Visibility = Visibility.Visible;
                string[] sParam = { sAREAID, sSHOPID, sPalletid};
                // 기간별 Pallet 확정 이력 정보 조회
                this.FrameOperation.OpenMenu("SFU010060100", true, sParam);
                loadingIndicator.Visibility = Visibility.Collapsed;

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

                GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sCOLNAME);

            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndBoxHist_Closed(object sender, EventArgs e)
        {
            BOX001_018_BOX_HIST window = sender as BOX001_018_BOX_HIST;
           
            grdMain.Children.Remove(window);

        }
        #endregion


        #region Mehod

        /// <summary>
        /// 출고대기 LOT Summary 조회
        /// </summary>
        private void GetSummaryInfo(string sAreaid, string sEqsgid, string sMDLLot, string sLotType)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAreaid;
                dr["EQSGID"] = sEqsgid;
                dr["MDLLOT_ID"] = sMDLLot;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_SUMMARY_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    _TOTAL_TPQ = 0;
                    _TOTAL_TCQ = 0;
                    _PROC_PPQ = 0;
                    _PROC_PCQ = 0;
                    _EXP_EPQ = 0;
                    _EXP_ECQ = 0;
                    _HOLD_HPQ = 0;
                    _HOLD_HCQ = 0;
                    _OWMS_OPQ = 0;
                    _OWMS_OCQ = 0;

                    if (dtResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            _TOTAL_TPQ = Convert.ToDouble(dtResult.Rows[i]["TOTAL_TPQ"].ToString()) + _TOTAL_TPQ;
                            _TOTAL_TCQ = Convert.ToDouble(dtResult.Rows[i]["TOTAL_TCQ"].ToString()) + _TOTAL_TCQ;

                            _PROC_PPQ = Convert.ToDouble(dtResult.Rows[i]["PROC_PPQ"].ToString()) + _PROC_PPQ;
                            _PROC_PCQ = Convert.ToDouble(dtResult.Rows[i]["PROC_PCQ"].ToString()) + _PROC_PCQ;

                            _EXP_EPQ = Convert.ToDouble(dtResult.Rows[i]["EXP_EPQ"].ToString()) + _EXP_EPQ;
                            _EXP_ECQ = Convert.ToDouble(dtResult.Rows[i]["EXP_ECQ"].ToString()) + _EXP_ECQ;

                            _HOLD_HPQ = Convert.ToDouble(dtResult.Rows[i]["HOLD_HPQ"].ToString()) + _HOLD_HPQ;
                            _HOLD_HCQ = Convert.ToDouble(dtResult.Rows[i]["HOLD_HCQ"].ToString()) + _HOLD_HCQ;

                            _OWMS_OPQ = Convert.ToDouble(dtResult.Rows[i]["OWMS_OPQ"].ToString()) + _OWMS_OPQ;
                            _OWMS_OCQ = Convert.ToDouble(dtResult.Rows[i]["OWMS_OCQ"].ToString()) + _OWMS_OCQ;
                        }
                    }
                     Util.GridSetData(dgSummary, dtResult, FrameOperation, true);

                    for (int col = dgSummary.Columns["TOTAL_QTY"].Index; col < dgSummary.Columns["CHK_HOLD"].Index; col++)
                    {
                        if(dgSummary.Columns[col].Name.ToString() == "TOTAL_QTY" || dgSummary.Columns[col].Name.ToString() == "PROC_QTY" || dgSummary.Columns[col].Name.ToString() == "OWMS_QTY" || dgSummary.Columns[col].Name.ToString() == "EXP_QTY" || dgSummary.Columns[col].Name.ToString() == "HOLD_QTY")
                        {
                            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns[dgSummary.Columns[col].Name], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});
                        }
                           
                    }

                    ////dgSummary.ItemsSource = DataTableConverter.Convert(dtResult);
                    //Util.GridSetData(dgSummary, dtResult, FrameOperation, true);

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
        private void GetPalletInfo(string sAreaid, string sEqsgid, string sLotType, string sProdid, string sChkType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("VIEW_PROC", typeof(string));
                RQSTDT.Columns.Add("VIEW_EXP", typeof(string));
                RQSTDT.Columns.Add("VIEW_HOLD", typeof(string));
                RQSTDT.Columns.Add("VIEW_OWMS", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAreaid;
                dr["EQSGID"] = sEqsgid;
                dr["PRODID"] = sProdid;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                dr["VIEW_PROC"] = sChkType == "CHK_PROC" ? sChkType : null;
                dr["VIEW_EXP"] = sChkType == "CHK_EXP" ? sChkType : null;
                dr["VIEW_HOLD"] = sChkType == "CHK_HOLD" ? sChkType : null;
                dr["VIEW_OWMS"] = sChkType == "CHK_OWMS" ? sChkType : null;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_LIST_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    ////dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation,true);
                });

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_LIST_CP", "RQSTDT", "RSLTDT", RQSTDT);
                //dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);// Util.AlertInfo(ex.Message);
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

        #endregion

        //private void btnNote_Click(object sender, RoutedEventArgs e)
        //{
        //    Button button = sender as Button;
        //    int row = ((C1.WPF.DataGrid.DataGridCellPresenter)button.Parent).Row.Index;

        //    string sPalletid = Util.NVC(dgPalletInfo.GetCell(row, dgPalletInfo.Columns["BOXID"].Index).Value);

        //    BOX001_018_PACK_NOTE popUp = new BOX001_018_PACK_NOTE();
        //    popUp.FrameOperation = this.FrameOperation;

        //    if (popUp != null)
        //    {
        //        object[] Parameters = new object[1];
        //        Parameters[0] = sPalletid;
        //        C1WindowExtension.SetParameters(popUp, Parameters);

        //        popUp.Closed += new EventHandler(wndPackNote_Closed);
        //        // 팝업 화면 숨겨지는 문제 수정.
        //        // this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
        //        grdMain.Children.Add(popUp);
        //        popUp.BringToFront();
        //    }
        //}

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
          //  datagrid.
           // datagrid.CurrentCell
        }

        private void dgPalletInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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

        private void dgPalletInfo_PreviewKeyDown(object sender, KeyEventArgs e)
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
                            }
                        }

                    }
                }

            }));
        }

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 파레트 바코드 표시 설정
            if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgPalletInfo.Columns.Contains("PLLT_BCD_ID"))
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgPalletInfo.Columns.Contains("PLLT_BCD_ID"))
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }

    }

}
