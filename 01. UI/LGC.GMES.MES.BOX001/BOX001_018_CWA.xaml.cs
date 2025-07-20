/*************************************************************************************
 Created Date : 2020.10.29
      Creator : 오화백
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  DATE            CSR NO            DEVELOPER            DESCRIPTION
  2020.10.29  DEVELOPER : Initial Created(폴란드 전용으로 신규로 메뉴생성).
  2022.01.11   C20210810-000547       김광오        Additonal column in Search stand-by pallet for shipment (ESWA)
  2022.03.17   C20220125-000010       김광오        [ESWA] GMES Cell hold 관리 효율 향상을 위한 Hold 재고 정보 추가 요청 검토 필요
  2022.04.25   C20220413-000427       김광오        [생산PI] 한계불량률 관리 항목 신규 추가(용량/DC)를 위한 GMES/MMD 시스템 개선
  2022.07.21   C20220324-000014       윤지해        [제조PI팀] OCV2 경과 일자 기준 변경으로 인한 장기 재고 관리 용이화 개선
  2022.02.15   C20221020-000003       김광섭        GMES 시스템의 "포장출고 대기 Lot 조회"기능 추가
  2023.07.06   E20230614-000940       박성진        상세조회화면 제품ID SPLIT 추가(변경번호 58074 주석처리)
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
using LGC.GMES.MES.Common.Mvvm;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_018_CWA : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();


        private DataTable _dtInfo = null;

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        string Geqsgid = string.Empty;
        string GMoDelLot = string.Empty;

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

        double _INSP_WAIT_IWPQ;
        double _INSP_WAIT_IWCQ;

        double _INSP_NG_INPQ;
        double _INSP_NG_INCQ;

        private int reqCount = 0;
        private int reqTotalCount = 0;
        private bool reqError = false;

        public BOX001_018_CWA_Model Vm { get; }

        public BOX001_018_CWA()
        {
            Vm = new BOX001_018_CWA_Model();
            DataContext = Vm;

            InitializeComponent();
            Loaded += BOX001_018_CWA_Loaded;

            #region Row Number 표시
            dgPalletInfo.RowHeaderWidth = 24; // default : 24

            dgPalletInfo.LoadedRowHeaderPresenter += (s, e) =>
            {
                try
                {
                    int topRows = ((C1DataGrid)s).Rows.TopRows.Count;
                    int bottomRows = ((C1DataGrid)s).Rows.BottomRows.Count;
                    int rows = ((C1DataGrid)s).Rows.Count;

                    if (e.Row.Index > topRows - 1 && e.Row.Index < rows - bottomRows)
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = (e.Row.Index - topRows + 1).ToString();
                        tb.TextAlignment = TextAlignment.Right;
                        tb.Margin = new Thickness(0, 0, 4, 0);

                        e.Row.HeaderPresenter.VerticalContentAlignment = VerticalAlignment.Center;
                        e.Row.HeaderPresenter.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        e.Row.HeaderPresenter.Content = tb;
                    }

                    int beforeRowInx = e.Row.Index - topRows;
                    int currRowInx = e.Row.Index - topRows + 1;

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        //if(beforeRowInx > 0 && (int)Math.Log10(beforeRowInx) != (int)Math.Log10(currRowInx) && ((int)Math.Log10(currRowInx) - 1) > 0)
                        //{
                        //    dgPalletInfo.RowHeaderWidth = 24 + ((int)Math.Log10(currRowInx) - 1) * 7;
                        //}

                        try
                        {
                            if (currRowInx >= 1 && ((int)Math.Log10(currRowInx) - 1) > 0)
                            {
                                dgPalletInfo.RowHeaderWidth = 24 + ((int)Math.Log10(currRowInx) - 1) * 7;
                            }
                            else
                            {
                                dgPalletInfo.RowHeaderWidth = 24;
                            }
                        }
                        catch
                        { }
                    }));
                }
                catch (Exception ex)
                {
                }
            };
            #endregion
        }

        private void BOX001_018_CWA_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_018_CWA_Loaded;

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

            // 라인 초기화
            cboEquipmentSegment.ApplyTemplate();

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
            if (cboEquipmentSegment.SelectedItems.Count == 0)
            {
                Util.MessageValidation("SFU1223"); //라인을 선택하세요
                return;
            }

            string sEqsgid = SelectEquipment(cboEquipmentSegment);
            //string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);


            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                sEqsgid = null;
            }
            Geqsgid = sEqsgid;


            string sMDLLot = SelectEquipment(cboModelLot);
            //string sMDLLot = Util.NVC(cboModelLot.SelectedValue);
            if (sMDLLot == "" || sMDLLot == "SELECT")
            {
                sMDLLot = null;
            }
            GMoDelLot = sMDLLot;

            string sLotType = Util.NVC(cboLottype.SelectedValue);
            if (sLotType == "" || sLotType == "SELECT")
            {
                sLotType = null;
            }
            // 출고대기 LOT Summary 조회
            GetSummaryInfo(sAREAID, sEqsgid, sMDLLot, sLotType);

            dgPalletInfo.ItemsSource = null;
        }

        private string SelectEquipment(MultiSelectionBox MultiSB)
        {
            string sEqptID = string.Empty;
            bool bModelLot = true;

            //
           if (MultiSB == cboModelLot && !string.IsNullOrEmpty(this.txtModelLot.Text.Trim()))
            {
                bModelLot = false;
            }

            int iCnt = DataTableConverter.Convert(MultiSB.ItemsSource).AsEnumerable().ToList().Count;
            if (iCnt == MultiSB.SelectedItems.Count && bModelLot)
            {
                return "SELECT";// "ALL";
            }

            for (int i = 0; i < MultiSB.SelectedItems.Count; i++)
            {
                if (i < MultiSB.SelectedItems.Count - 1)
                {
                    sEqptID += Convert.ToString(MultiSB.SelectedItems[i]) + ",";
                }
                else
                {
                    sEqptID += Convert.ToString(MultiSB.SelectedItems[i]);
                }
            }

            return sEqptID;
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

            //String[] sFilter = { sAREAID };    // Area
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");
            //SetLine_CP(sAREAID);
            SetLine_CP2(sAREAID);
            isVisibleBCD(sAREAID);
        }

        private void txtModelLot_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetModelLot2();
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            SetModelLot2();
            //SetModelLot();
            //_combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboEquipmentSegment });
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

                    #region C20220125-000010 [ESWA] GMES Cell hold 관리 효율 향상을 위한 Hold 재고 정보 추가 요청 검토 필요 by kimgwago on 2022.03.22
                    if (sColName == "CHK_HOLD")
                    {
                        Vm.HoldVisibility = Visibility.Visible;
                    }
                    else
                    {
                        Vm.HoldVisibility = Visibility.Hidden;
                    }
                    #endregion

                    if (sColName == "CHK_TOTAL" || sColName == "CHK_PROC" || sColName == "CHK_OWMS" || sColName == "CHK_EXP" || sColName == "CHK_HOLD" || sColName == "CHK_INSP_WAIT" || sColName == "CHK_INSP_NG")
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
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_INSP_WAIT", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_INSP_NG", false);
                        }

                         if (sChkValue != "1")
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[dgSummary.CurrentRow.Index].DataItem, sColName, true);
                            if (string.IsNullOrEmpty(sAreaid))
                            {
                                if (string.IsNullOrEmpty(GMoDelLot))
                                {
                                    GetPalletInfo(sAREAID, Geqsgid, null, null, sColName);
                                }
                                else
                                {
                                    for(int i = dgSummary.Rows.TopRows.Count; i < dgSummary.Rows.Count - 1; i++)
                                    {
                                        sProdid += Util.NVC(dgSummary.GetCell(i, dgSummary.Columns["PRODID"].Index).Value) + ",";
                                    }
                                    GetPalletInfo(sAREAID, Geqsgid, null, sProdid, sColName);
                                }


                            }
                            else
                            {
                                GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sColName);
                            }

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
                string[] sParam = { sAREAID, sSHOPID, sPalletid };
                // 기간별 Pallet 확정 이력 정보 조회
                this.FrameOperation.OpenMenu("SFU010060100", true, sParam);
                loadingIndicator.Visibility = Visibility.Collapsed;

            }
            else if (datagrid.CurrentColumn.Name == "PACKDTTM")
            {
                BOX001_018_BOX_HIST_CWA popUp = new BOX001_018_BOX_HIST_CWA();
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
                BOX001_018_PACK_NOTE_CWA popUp = new BOX001_018_PACK_NOTE_CWA();
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
            BOX001.BOX001_018_PACK_NOTE_CWA wndPopup = sender as BOX001.BOX001_018_PACK_NOTE_CWA;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //재조회
                string sAreaid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["AREAID"].Index).Value);
                string sEqsgid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["EQSGID"].Index).Value);
                string sLotType = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                string sProdid = Util.NVC(dgSummary.GetCell(iCURRENT_ROW, dgSummary.Columns["PRODID"].Index).Value);

                if (string.IsNullOrEmpty(sAreaid))
                {
                    if (string.IsNullOrEmpty(GMoDelLot))
                    {
                        GetPalletInfo(sAREAID, Geqsgid, null, null, sCOLNAME);
                    }
                    else
                    {
                        for (int i = dgSummary.Rows.TopRows.Count; i < dgSummary.Rows.Count - 1; i++)
                        {
                            sProdid += Util.NVC(dgSummary.GetCell(i, dgSummary.Columns["PRODID"].Index).Value) + ",";
                        }
                        GetPalletInfo(sAREAID, Geqsgid, null, sProdid, sCOLNAME);
                    }
                }
                else
                {
                    GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sCOLNAME);
                }


            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndBoxHist_Closed(object sender, EventArgs e)
        {
            BOX001_018_BOX_HIST_CWA window = sender as BOX001_018_BOX_HIST_CWA;

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
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_SUMMARY_CP_CWA", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
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

                    _INSP_WAIT_IWPQ = 0;
                    _INSP_WAIT_IWCQ = 0;

                    _INSP_NG_INPQ = 0;
                    _INSP_NG_INCQ = 0;

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

                            _INSP_WAIT_IWPQ = Convert.ToDouble(dtResult.Rows[i]["INSP_WAIT_IWPQ"].ToString()) + _INSP_WAIT_IWPQ;
                            _INSP_WAIT_IWCQ = Convert.ToDouble(dtResult.Rows[i]["INSP_WAIT_IWCQ"].ToString()) + _INSP_WAIT_IWCQ;

                            _INSP_NG_INPQ = Convert.ToDouble(dtResult.Rows[i]["INSP_NG_INPQ"].ToString()) + _INSP_NG_INPQ;
                            _INSP_NG_INCQ = Convert.ToDouble(dtResult.Rows[i]["INSP_NG_INCQ"].ToString()) + _INSP_NG_INCQ;
                        }

                        DataRow drTotal = dtResult.NewRow();

                        drTotal["EQSGNAME"] = ObjectDic.Instance.GetObjectName("합계");
                        drTotal["TOTAL_QTY"] = _TOTAL_TPQ + " (" + _TOTAL_TCQ + ")";
                        drTotal["PROC_QTY"] = _PROC_PPQ + " (" + _PROC_PCQ + ")";
                        drTotal["EXP_QTY"] = _EXP_EPQ + " (" + _EXP_ECQ + ")";
                        drTotal["HOLD_QTY"] = _HOLD_HPQ + " (" + _HOLD_HCQ + ")";
                        drTotal["OWMS_QTY"] = _OWMS_OPQ + " (" + _OWMS_OCQ + ")";
                        drTotal["INSP_WAIT_QTY"] = _INSP_WAIT_IWPQ + " (" + _INSP_WAIT_IWCQ + ")";
                        drTotal["INSP_NG_QTY"] = _INSP_NG_INPQ + " (" + _INSP_NG_INCQ + ")";

                        dtResult.Rows.InsertAt(drTotal, dtResult.Rows.Count);
                    }

                    Util.GridSetData(dgSummary, dtResult, FrameOperation, true);

                    //for (int col = dgSummary.Columns["TOTAL_QTY"].Index; col < dgSummary.Columns["CHK_HOLD"].Index; col++)
                    //{
                    //    if(dgSummary.Columns[col].Name.ToString() == "TOTAL_QTY" || dgSummary.Columns[col].Name.ToString() == "PROC_QTY" || dgSummary.Columns[col].Name.ToString() == "OWMS_QTY" || dgSummary.Columns[col].Name.ToString() == "EXP_QTY" || dgSummary.Columns[col].Name.ToString() == "HOLD_QTY")
                    //    {
                    //        DataGridAggregate.SetAggregateFunctions(dgSummary.Columns[dgSummary.Columns[col].Name], new DataGridAggregatesCollection {
                    //                        new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});
                    //    }

                    //}

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
                RQSTDT.Columns.Add("VIEW_INSP_WAIT", typeof(string));
                RQSTDT.Columns.Add("VIEW_INSP_NG", typeof(string));

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
                dr["VIEW_INSP_WAIT"] = sChkType == "CHK_INSP_WAIT" ? sChkType : null;
                dr["VIEW_INSP_NG"] = sChkType == "CHK_INSP_NG" ? sChkType : null;
                RQSTDT.Rows.Add(dr);

                string bizRule = string.Empty;
                #region C20220324-000014 [제조PI팀]OCV2 경과 일자 기준 변경으로 인한 장기 재고 관리 용이화 개선 by 윤지해(jihaeyoon) on 2022.07.21
                //if (sChkType == "CHK_TOTAL")
                //{
                //    bizRule = "DA_PRD_SEL_STOCK_LIST_CP";
                //}
                //else
                //{
                //    bizRule = "DA_PRD_SEL_STOCK_LIST_CP_CWA";
                //}
                bizRule = "DA_PRD_SEL_STOCK_LIST_CP_CWA";
                #endregion

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        SetFifo4Grid(dtResult);
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    #region C20210810-000547 Additonal column in Search stand by pallet for shipment (ESWA) - Commented by kimgwango on 2021.01.05
                    ////dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                    //Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                    #endregion

                    #region C20220125-000010 [ESWA] GMES Cell hold 관리 효율 향상을 위한 Hold 재고 정보 추가 요청 검토 필요 - Added by kimgwango on 2022.03.21
                    if (Vm.HoldVisibility != Visibility.Visible || dtResult == null || dtResult.Rows.Count == 0)
                    {
                        SetFifo4Grid(dtResult);
                        return;
                    }

                    List<string> selectedValues = dtResult.AsEnumerable().Select(r => r.Field<string>("BOXID").ToString()).ToList();
                               
                    string palletList = string.Empty;

                    //if (selectedValues != null && selectedValues.Count > 0)
                    //{
                    //    palletList = "'";
                    //    palletList = palletList + String.Join("','", selectedValues);
                    //    palletList = palletList + "'";
                    //}

                    int sndQty = 20;
                    reqCount = 0;
                    reqTotalCount = 0;
                    reqError = false;

                    if(selectedValues.Count / 20 > 100)
                    {
                        sndQty = selectedValues.Count / 100;
                    }

                    for (int i = 0; i < selectedValues.Count; i += sndQty)
                    {
                        palletList = "'";
                        palletList = palletList + String.Join("','", selectedValues.Skip(i).Take(sndQty));
                        palletList = palletList + "'";

                        RQSTDT = new DataTable();
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("BOXID", typeof(string));

                        dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["BOXID"] = palletList;
                        RQSTDT.Rows.Add(dr);

                        bizRule = "DA_PRD_SEL_STOCK_HOLD_NOTE_LIST_CP_CWA";

                        if (reqTotalCount == 0) loadingIndicator.Visibility = Visibility.Visible;
                        reqTotalCount++;
                        new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", RQSTDT, (dtResult2, ex2) =>
                        {
                            if (ex2 != null)
                            {
                                if (reqTotalCount == ++reqCount)
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    SetFifo4Grid(dtResult);
                                }

                                if (!reqError)
                                {
                                    reqError = true;
                                    Util.MessageException(ex2); //Util.AlertInfo(ex.Message);
                                }
                                return;
                            }

                            if (dtResult2 == null || dtResult2.Rows.Count == 0)
                            {
                                ++reqCount;
                                return;
                            }

                            dtResult.PrimaryKey = new DataColumn[] { dtResult.Columns["BOXID"] };
                            dtResult2.PrimaryKey = new DataColumn[] { dtResult2.Columns["BOXID"] };

                            dtResult.Merge(dtResult2);

                            if (reqTotalCount == ++reqCount)
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                SetFifo4Grid(dtResult);
                            }
                        });
                    }
                    #endregion
                });

                //DataTable dtLine = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                //loadingIndicator.Visibility = Visibility.Visible;

                //if (string.IsNullOrEmpty(sEqsgid) && sChkType != "CHK_TOTAL")
                //{
                //    for(int i=0; i< dtLine.Rows.Count; i++)
                //    {
                //        RQSTDT.Rows[0]["EQSGID"] = dtLine.Rows[i]["CBO_CODE"].ToString();

                //        //DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRule, "RQSTDT", "RSLTDT", RQSTDT);
                //        new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //        {
                //           if (ex != null)
                //            {
                //                Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                //                return;
                //            }

                //            if (_dtInfo == null)
                //            {
                //                _dtInfo = dtResult.Copy();
                //            }
                //            else
                //            {
                //                _dtInfo.PrimaryKey = new DataColumn[] { _dtInfo.Columns["BOXID"] };
                //                _dtInfo.Merge(dtResult, true);
                //            }
                //        });

                //   }
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //    Util.GridSetData(dgPalletInfo, _dtInfo, FrameOperation, true);
                //    _dtInfo = null;
                //    //loadingIndicator.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        if (ex != null)
                //        {
                //            Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                //        return;
                //        }
                //    ////dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                //    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                //    });

                //}
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
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {


                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
                //if (e.Cell.Row.Type == DataGridRowType.Bottom)
                //{
                //    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                //        if (panel == null && panel.Children == null && panel.Children.Count < 1) return;

                //        ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                //        if (e.Cell.Column.Index >= dg.Columns["TOTAL_QTY"].Index && e.Cell.Column.Index <= dg.Columns["HOLD_QTY"].Index)
                //        {

                //           if (dg.GetRowCount() > 0)
                //            {
                //               if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                //                {
                //                    if (e.Cell.Column.Name == "TOTAL_QTY")
                //                    {
                //                        presenter.Content =  _TOTAL_TPQ + " (" + _TOTAL_TCQ + ")";
                //                    }
                //                    if (e.Cell.Column.Name == "PROC_QTY")
                //                    {
                //                        presenter.Content = _PROC_PPQ + " (" + _PROC_PCQ + ")";
                //                    }
                //                    if (e.Cell.Column.Name == "EXP_QTY")
                //                    {
                //                        presenter.Content = _EXP_EPQ + " (" + _EXP_ECQ + ")";
                //                    }
                //                    if (e.Cell.Column.Name == "HOLD_QTY")
                //                    {
                //                        presenter.Content = _HOLD_HPQ + " (" + _HOLD_HCQ + ")";
                //                    }
                //                    if (e.Cell.Column.Name == "OWMS_QTY")
                //                    {
                //                        presenter.Content = _OWMS_OPQ + " (" + _OWMS_OCQ + ")";
                //                    }
                //                }
                //            }

                //        }
                //    }

            }));
        }

        private void dgSummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }


        //private void SetModelLot()
        //{
        //    DataTable RQSTDT = new DataTable();
        //    RQSTDT.TableName = "RQSTDT";
        //    RQSTDT.Columns.Add("LANGID", typeof(string));
        //    RQSTDT.Columns.Add("EQSGID", typeof(string));
        //    RQSTDT.Columns.Add("AREAID", typeof(string));

        //    DataRow dr = RQSTDT.NewRow();
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue?.ToString()) ? null : cboEquipmentSegment.SelectedValue?.ToString();
        //    dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? LoginInfo.CFG_AREA_ID.ToString() : sAREAID.ToString();
        //    RQSTDT.Rows.Add(dr);

        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_CBO", "RQSTDT", "RSLTDT", RQSTDT);


        //    cboModelLot.DisplayMemberPath = "CBO_NAME";
        //    cboModelLot.SelectedValuePath = "CBO_CODE";

        //    DataRow drIns = dtResult.NewRow();
        //    drIns["CBO_NAME"] = "-ALL-";
        //    drIns["CBO_CODE"] = "";
        //    dtResult.Rows.InsertAt(drIns, 0);

        //    cboModelLot.ItemsSource = dtResult.Copy().AsDataView();

        //    cboModelLot.SelectedIndex = 0;

        //}

        private void SetModelLot2()
        {
            try
            {
                cboModelLot.ItemsSource = null;

                string sEqsgid = SelectEquipment(cboEquipmentSegment);
                //string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);

                if (sEqsgid == "" || sEqsgid == "SELECT")
                {
                    sEqsgid = null;
                    // 라인선택 필수 메시지 필요??
                    //return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = sEqsgid;
                dr["AREAID"] = string.IsNullOrEmpty(sAREAID) ? LoginInfo.CFG_AREA_ID.ToString() : sAREAID.ToString();
                dr["MDLLOT_ID"] = this.txtModelLot.Text.Trim();
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MDLLOT_MULTI_PJT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboModelLot.DisplayMemberPath = "CBO_NAME";
                cboModelLot.SelectedValuePath = "CBO_CODE";

                cboModelLot.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
            }
            catch (Exception ex)
            {
            }

        }

        //private void SetLine_CP(string sAREAID)
        //{
        //    try
        //    {

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));
        //        RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = sAREAID;
        //        dr["EXCEPT_GROUP"] = "VD";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

        //        cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
        //        cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

        //        DataRow drIns = dtResult.NewRow();
        //        drIns["CBO_NAME"] = "-ALL-";
        //        drIns["CBO_CODE"] = "";
        //        dtResult.Rows.InsertAt(drIns, 0);

        //        cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();
        //        if (dtResult.Rows.Count > 1)
        //        {
        //            if (!LoginInfo.CFG_EQSG_ID.Equals(""))
        //            {
        //                cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
        //                if (cboEquipmentSegment.SelectedIndex < 0)
        //                {
        //                    cboEquipmentSegment.SelectedIndex = 0;
        //                }
        //            }
        //            else
        //            {
        //                cboEquipmentSegment.SelectedIndex = 0;
        //            }
        //        }
        //        else
        //        {
        //            cboEquipmentSegment.SelectedIndex = 0;
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //}

        private void SetLine_CP2(string sAREAID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID;
                dr["EXCEPT_GROUP"] = "VD";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CP", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #region [ UTIL ] - Grid
        private void SetFifo4Grid(DataTable dtResult)
        {
            #region C20210810-000547 Additonal column in Search stand by pallet for shipment (ESWA) - Added by kimgwango on 2021.01.05
            try
            {
                // FIFO 추가
                // 생산유효일(PROD_VALID_DATE) VS 장기재고검사유효일(EXP_INSP_VALID_DATE) 중 오래된 날짜를 기준으로 SORT
                if (dtResult == null)
                {
                    Util.gridClear(dgPalletInfo);
                    return;
                }
                if (dtResult.Rows.Count == 0)
                {
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                    return;
                } 

                DataTable dtResult2 = dtResult.Select().OrderBy(r =>
                {
                    bool blnDt1 = false;
                    bool blnDt2 = false;
                    DateTime dt1;
                    DateTime dt2;

                    string strDt1 = string.Empty;
                    string strDt2 = string.Empty;

                    strDt1 = r["PROD_VALID_DATE"]?.ToString();
                    strDt2 = r["EXP_INSP_VALID_DATE"]?.ToString();

                    if (strDt1.Length >= 10) strDt1 = strDt1.Substring(0, 10);
                    if (strDt2.Length >= 10) strDt2 = strDt2.Substring(0, 10);

                    blnDt1 = DateTime.TryParse(strDt1, out dt1);
                    blnDt2 = DateTime.TryParse(strDt2, out dt2);

                    if (blnDt1 && blnDt2)
                    {
                        if (DateTime.Compare(dt1, dt2) < 0)
                            return dt1;
                        else
                            return dt2;
                    }
                    else if (blnDt1)
                        return dt1;
                    else if (blnDt2)
                        return dt2;
                    else
                        return DateTime.MaxValue;
                }).CopyToDataTable<DataRow>();

                dtResult2.Columns.Add(new DataColumn("FIFO", typeof(decimal)));
                for (int i = 0; i < dtResult2.Rows.Count; i++)
                {
                    DataRow r = dtResult2.Rows[i];
                    r["FIFO"] = i + 1;
                }

                Util.GridSetData(dgPalletInfo, dtResult2, FrameOperation, true);
            }
            catch (Exception ex2)
            {
                Util.MessageException(ex2);
            }
            #endregion
        }
        #endregion

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            if (dgPalletInfo.Columns.Contains("PLLT_BCD_ID"))
            {
                // 팔레트 바코드 표시 설정
                if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
                {
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    public class BOX001_018_CWA_Model : BindableBase
    {
        private Visibility _holdVisibility;
        public Visibility HoldVisibility
        {
            get { return _holdVisibility; }
            set { SetProperty(ref _holdVisibility, value); }
        }

        public BOX001_018_CWA_Model()
        {
            HoldVisibility = Visibility.Collapsed;
        }
    }
}
