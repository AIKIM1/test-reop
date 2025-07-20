/*************************************************************************************
 Created Date : 2021.04.11
      Creator : 이제섭
   Decription : Cell 반품 확정
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.11  DEVELOPER : Initial Created.
  2023.03.20  박나연 : 반품 현황 CELL 정보 조회 시, 반품 이후 취출/추가된 CELL에 대해서 취출/추가 조회되는 '비고' 컬럼 추가
  2023.07.28  홍석원 : 반제품 전환 대응 TOP_PRODID 컬럼 추가.
  2023.11.15  이제섭 : ESNJ RMA 반품 기능 추가 (특정창고(300R/301R)로 입고 시, Cell 스캔 없이 수량으로 입고처리)
  2023.12.07  박수미 : CELL반품(총 PALLET/출고 수량), 반품현황 탭 (총 PALLET/입고/출고 수량) 추가
  2024.02.19  최윤호 : 반품확인 라디오버튼 에서 체크 박스로, 다중 선택 및 반품 사유 필드 추가.
  2024.02.26  Adira  : Add line combobox at Cell Return and Return status Tab
  2024.03.05  최윤호 : Adira(2024.02.19)버전 Merge + 엑셀 다운로드 기능 추가[E20231025-001049]
  2024.03.06  홍석원 : 반품 확정 시 Multi 선택하여 일괄 반품 가능하도록 기능 개선
  2024.08.06  최석준 : 사외반품 RSO NO 조건 조회 및 표시 기능 추가  (2025년 적용예정, 수정 시 연락부탁드립니다)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_310 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        public bool bScanflag = false;
        public bool bLoadflag = false;
        public bool bScanflag1 = false;
        public bool bLoadflag1 = false;
        public string pREMARK = string.Empty;
        public string pUSER = string.Empty;
        public string pRCV_ISS_ID = string.Empty;
        public string sSAVE_SEQNO = string.Empty;
        public int iOld_Index = 0;
        public int iNow_Index = 0;
        public int iBack = 0;

        private string sRETURN_TYPE_CODE = string.Empty;
        private string sRETURN_TYPE_CODE_HIST = string.Empty;


        #region Declaration & Constructor 
        public BOX001_310()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnTempStorage);
            listAuth.Add(btnConfrim);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            // Sample 입고 탭 숨김여부
            if (UseCommoncodeArea())
            {
                tabReturnSmpl.Visibility = Visibility.Visible;
            }
            else
            {
                tabReturnSmpl.Visibility = Visibility.Collapsed;
            }

            // 사외반품번호 조회 숨김여부
            if (GetOcopRtnPsgArea())
            {
                txtOCOPRTNNO.Visibility = Visibility.Visible;
                txtOCOPRSONO.Visibility = Visibility.Visible;
                txtOCOPRTNNO2.Visibility = Visibility.Visible;
                txtOCOPRSONO2.Visibility = Visibility.Visible;
                dgRetrunCellList.Columns["OCOP_RSO_NO"].Visibility = Visibility.Visible;
                dgRetrunCellList2.Columns["OCOP_RSO_NO"].Visibility = Visibility.Visible;
            }
            else
            {
                txtOCOPRTNNO.Visibility = Visibility.Collapsed;
                txtOCOPRSONO.Visibility = Visibility.Collapsed;
                txtOCOPRTNNO2.Visibility = Visibility.Collapsed;
                txtOCOPRSONO2.Visibility = Visibility.Collapsed;
                dgRetrunCellList.Columns["OCOP_RSO_NO"].Visibility = Visibility.Collapsed;
                dgRetrunCellList2.Columns["OCOP_RSO_NO"].Visibility = Visibility.Collapsed;
            }

            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            dtpDateFrom2.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo2.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            CommonCombo combo = new CommonCombo();

            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            combo.SetCombo(cboArea1, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            C1ComboBox[] cboToChild = { cboTransLoc };
            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, cbChild: cboToChild, sCase: "AREA_CP");

            //combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT);
            C1ComboBox[] cboCompParent = { cboArea2 };
            combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            //2024-02-26 Adira: Add Line Equipment
            //Cell반품 탭
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID }, sCase: "LINE_FCS" );
            cboEquipmentSegment.SelectedIndex = 0;

            //반품현황 탭
            combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID2 }, sCase: "LINE_FCS");
            cboEquipmentSegment2.SelectedIndex = 0;


            if (LoginInfo.CFG_SHOP_ID == "G184")
            {
                // 반품유형 콤보
                cboReturnType.IsEnabled = true;
                cboReturnType.Visibility = Visibility.Visible;
                txtReturnType.Visibility = Visibility.Visible;

                cboReturnTypeHist.IsEnabled = true;
                cboReturnTypeHist.Visibility = Visibility.Visible;
                txtReturnTypeHist.Visibility = Visibility.Visible;

                SetReturnTypeCombo();
            }
            else
            {
                cboReturnType.IsEnabled = false;
                cboReturnType.Visibility = Visibility.Collapsed;
                txtReturnType.Visibility = Visibility.Collapsed;

                cboReturnTypeHist.IsEnabled = false;
                cboReturnTypeHist.Visibility = Visibility.Collapsed;
                txtReturnTypeHist.Visibility = Visibility.Collapsed;
            }


        }
        #endregion

        #region Event

        private void SearchCell_ReturnList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["RCV_ISS_ID"] = null;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgRetrunCellList);
                //    dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgRetrunCellList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            int idx = 0;

            if (sender == null)
                return;


            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if (bScanflag == true && iBack == 1)
            {
                //if (iOld_Index == iNow_Index)
                iBack = 0;
                return;
            }

            if (bLoadflag == true && iBack == 1)
            {
                //if (iOld_Index == iNow_Index)
                iBack = 0;
                return;
            }


            if (bScanflag == true || bLoadflag == true)
            {
                //스캔한 데이터를 초기화 하시겠습니까?
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3133"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3133", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                            //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                            //if (dt != null)
                            //{
                            //    for (int i = 0; i < dt.Rows.Count; i++)
                            //    {
                            //        DataRow row = dt.Rows[i];

                            //        if (idx == i)
                            //            dt.Rows[i]["CHK"] = true;
                            //        else
                            //            dt.Rows[i]["CHK"] = false;
                            //    }

                            //    dgRetrunCellList.BeginEdit();
                            //    dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                            //    dgRetrunCellList.EndEdit();
                            //}

                            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                            for (int i = 0; i < dg.GetRowCount(); i++)
                            {
                                if (idx == i)
                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                                else
                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                            }

                            //row 색 바꾸기
                            dgRetrunCellList.SelectedIndex = idx;

                            Util.gridClear(dgReturnBoxList);
                            Util.gridClear(dgReturnLotList);
                            Util.gridClear(dgCellInfo);

                            Return_Box_List(idx);

                            bScanflag = false;
                            bLoadflag = false;

                            txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                            iNow_Index = idx;
                            iOld_Index = idx;
                            iBack = 0;
                        }
                    }
                    else
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            //int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                            DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                            if (dt != null)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    DataRow row = dt.Rows[i];

                                    if (iOld_Index == i)
                                        dt.Rows[i]["CHK"] = true;
                                    else
                                        dt.Rows[i]["CHK"] = false;
                                }

                                dgRetrunCellList.BeginEdit();
                                // dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                                Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                                dgRetrunCellList.EndEdit();

                                iBack = 1;
                            }

                            //row 색 바꾸기
                            //dgRetrunCellList.SelectedIndex = iIndex;

                            //Util.gridClear(dgReturnBoxList);
                            //Util.gridClear(dgReturnLotList);
                            //Util.gridClear(dgCellInfo);

                            //Return_Box_List(idx);

                            //bScanflag = false;

                            //txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
                        }
                    }

                });
            }
            else
            {
                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    iNow_Index = idx;

                    DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];

                            if (idx == i)
                                dt.Rows[i]["CHK"] = true;
                            else
                                dt.Rows[i]["CHK"] = false;
                        }

                        dgRetrunCellList.BeginEdit();
                        //  dgRetrunCellList.ItemsSource = DataTableConverter.Convert(dt);
                        Util.GridSetData(dgRetrunCellList, dt, FrameOperation, true);
                        dgRetrunCellList.EndEdit();
                    }

                    //row 색 바꾸기
                    dgRetrunCellList.SelectedIndex = idx;

                    Util.gridClear(dgReturnBoxList);
                    Util.gridClear(dgReturnLotList);
                    Util.gridClear(dgCellInfo);

                    Return_Box_List(idx);

                    txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                    iOld_Index = iNow_Index;
                    iBack = 0;
                }
            }
        }

        private void Return_Box_List(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                // Multi 선택하는 경우 조회된 List를 추가하기 위해 Grid 초기화 삭제 처리
                //Util.gridClear(dgReturnBoxList);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }

                // 기존 조회된 Grid에 추가 조회된 List 추가 처리
                DataTable dtBoxInfo = DataTableConverter.Convert(dgReturnBoxList.ItemsSource);
                dtBoxInfo.Merge(BOXList);
                Util.GridSetData(dgReturnBoxList, dtBoxInfo, FrameOperation, true);
                //Util.GridSetData(dgReturnBoxList, BOXList, FrameOperation, true);


                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT1.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr1["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                RQSTDT1.Rows.Add(dr1);

                DataTable LOTList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LOT_LIST", "RQSTDT", "RSLTDT", RQSTDT1);
                
                // Multi 선택하는 경우 조회된 List를 추가하기 위해 Grid 초기화 삭제 처리
                //Util.gridClear(dgReturnLotList);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }

                // 기존 조회된 Grid에 추가 조회된 List 추가 처리
                DataTable dtLotInfo = DataTableConverter.Convert(dgReturnLotList.ItemsSource);
                dtLotInfo.Merge(LOTList);
                Util.GridSetData(dgReturnLotList, dtLotInfo, FrameOperation, true);
                //Util.GridSetData(dgReturnLotList, LOTList, FrameOperation, true);                

                if (DataTableConverter.GetValue(dgRetrunCellList.Rows[idx].DataItem, "CELL_INFO").ToString() == "Y")
                {
                    // 반품 CELL 리스트 조회
                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT2.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["RCV_ISS_ID"] = sRCV_ISS_ID;
                    dr2["BOX_RCV_ISS_STAT_CODE"] = "SHIPPING";

                    RQSTDT2.Rows.Add(dr2);

                    DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                    // Multi 선택하는 경우 조회된 List를 추가하기 위해 Grid 초기화 삭제 처리
                    //Util.gridClear(dgCellInfo);

                    if (CELLList.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                        return;
                    }

                    // 기존 조회된 Grid에 추가 조회된 List 추가 처리
                    DataTable dtCellInfo = DataTableConverter.Convert(dgCellInfo.ItemsSource);
                    dtCellInfo.Merge(CELLList);
                    Util.GridSetData(dgCellInfo, dtCellInfo, FrameOperation, true);
                    //Util.GridSetData(dgCellInfo, CELLList, FrameOperation, true);

                    txtCell_ID.IsReadOnly = true;
                    txtCell_ID.Text = "";

                    txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
                }
                else
                {
                    txtCell_ID.IsReadOnly = false;
                    txtCell_ID.Text = "";
                    txtCell_ID.Focus();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void btnConfrim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                // Multi 선택한 경우 Cell 수량 비교 시 전체 수량을 합하여 비교할 수 있도록 수정
                //double dReturnQty = Convert.ToDouble(drChk[0]["ISS_QTY"].ToString());
                double dReturnQty = 0;

                for (int i = 0; i < drChk.Length; i++)
                {
                    dReturnQty += Convert.ToDouble(drChk[i]["ISS_QTY"].ToString());
                }

                int iRowCnt = dgCellInfo.GetRowCount();

                // 2023.11.15 이제섭 반품 유형 추가 
                if (sRETURN_TYPE_CODE != "RMA" && dReturnQty != iRowCnt)
                {
                    Util.MessageValidation("SFU3134");  //반품수량과 Cell 수량이 일치하지 않습니다.
                    return;
                }

                DataTable returnlist = new DataTable();
                returnlist.Columns.Add("RCV_ISS_ID", typeof(string));
                returnlist.Columns.Add("BOXID", typeof(string));
                returnlist.Columns.Add("PRODID", typeof(string));
                returnlist.Columns.Add("PRJT_NAME", typeof(string));
                returnlist.Columns.Add("ISS_QTY", typeof(string));

                for (int row = 0; row < dgReturnLotList.Rows.Count; row++)
                {
                    DataRow dr = returnlist.NewRow();
                    dr["RCV_ISS_ID"] = Util.NVC(dgReturnLotList.GetCell(row, dgReturnLotList.Columns["RCV_ISS_ID"].Index).Value);
                    dr["BOXID"] = Util.NVC(dgReturnLotList.GetCell(row, dgReturnLotList.Columns["BOXID"].Index).Value);
                    dr["PRODID"] = Util.NVC(dgReturnLotList.GetCell(row, dgReturnLotList.Columns["PRODID"].Index).Value);
                    dr["PRJT_NAME"] = Util.NVC(dgReturnLotList.GetCell(row, dgReturnLotList.Columns["PROJECTNAME"].Index).Value);
                    dr["ISS_QTY"] = Util.NVC(dgReturnLotList.GetCell(row, dgReturnLotList.Columns["RETURN_QTY"].Index).Value);
                    returnlist.Rows.Add(dr);
                }

                BOX001_310_CONFIRM wndConfirm = new BOX001_310_CONFIRM(iRowCnt);
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = returnlist;
                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_310_CONFIRM window = sender as BOX001_310_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                System.Windows.Forms.Application.DoEvents();
                // 2023.11.15 이제섭 ESNJ ESS RMA 반품 추가
                if (sRETURN_TYPE_CODE == "RMA")
                {
                    Comfirm_ReturnCell_RMA(window.sNOTE);
                }
                else
                {
                    Comfirm_ReturnCell(window.sNOTE);
                }

            }
            grdMain.Children.Remove(window);
        }

        private void result_wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_310_RESULT window = sender as BOX001_310_RESULT;
            grdMain.Children.Remove(window);

            Init_Form();
        }

        private void Comfirm_ReturnCell(string sNote)
        {
            // 최종 처리결과 전송을 위해 try 외부에서 처리결과 처리
            DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");

           // DataTable dtChk = DataTableConverter.Convert(dgRetrunCellList.ItemsSource);
           // DataRow[] drChk = null;
          //  drChk = dtChk.Select("CHK" + " = 'True'");

            DataTable dtReturnResult = new DataTable();
            dtReturnResult.Columns.Add("RCV_ISS_ID", typeof(string));
            dtReturnResult.Columns.Add("STATUS", typeof(string));
            dtReturnResult.Columns.Add("NOTE", typeof(string));

            // 선택된 반품ID의 결과값을 초기값 NotProcessed 으로 설정
            foreach (DataRow dr in drChk)
            {
                DataRow drRcvIssIs = dtReturnResult.NewRow();

                drRcvIssIs["RCV_ISS_ID"] = dr["RCV_ISS_ID"].ToString();
                drRcvIssIs["STATUS"] = "NotProcessed";
                drRcvIssIs["NOTE"] = "";

                dtReturnResult.Rows.Add(drRcvIssIs);
            }

            int returnIdx = 0;

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                
                if (drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }
                
                foreach (DataRow dr in drChk)
                {
                    string rcvIssID = dr["RCV_ISS_ID"].ToString();

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("TOTAL_QTY", typeof(int));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("RCV_NOTE", typeof(string));

                    DataTable inPallet = indataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("BOXID", typeof(string));
                    inPallet.Columns.Add("RCV_ISS_ID", typeof(string));

                    DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                    inLot.Columns.Add("SUBLOTID", typeof(string));

                    // 반품번호에 맞는 Cell List 조회
                    int cellCount = 0;

                    for(int i = 0; i < dgCellInfo.Rows.Count; i++)
                    {
                        if (rcvIssID == DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "RCV_ISS_ID").ToString())
                        {
                            cellCount++;

                            DataRow drSublot = inLot.NewRow();
                            drSublot["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();

                            indataSet.Tables["INSUBLOT"].Rows.Add(drSublot);
                        }
                    }

                    DataRow row = inData.NewRow();
                    row["AREAID"] = sAREAID;
                    row["TOTAL_QTY"] = cellCount;
                    row["USERID"] = txtWorker.Tag as string;
                    row["RCV_NOTE"] = sNote;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    String palletID = "";
                    for (int i = 0; i < dgReturnLotList.Rows.Count; i++)
                    {
                        if (rcvIssID == DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "RCV_ISS_ID").ToString())
                        {
                            palletID = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "BOXID").ToString();
                            break;
                        }
                    }

                    DataRow row2 = inPallet.NewRow();
                    row2["BOXID"] = palletID;
                    row2["RCV_ISS_ID"] = rcvIssID;

                    indataSet.Tables["INPALLET"].Rows.Add(row2);

                    // 테스트 후 주석 해제
                    //System.Windows.Forms.Application.DoEvents();
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_RETURN_SHIP_CELL_BX", "INDATA,INPALLET,INSUBLOT", null, indataSet);

                    // 처리결과값을 Complete으로 변경
                    dtReturnResult.Rows[returnIdx]["STATUS"] = "Complete";

                    returnIdx++;
                }
            }
            catch (Exception ex)
            {
                // Exception 발생 시 결과값을 Fail, 비고에 Exception 메시지 설정
                dtReturnResult.Rows[returnIdx]["STATUS"] = "Fail";
                dtReturnResult.Rows[returnIdx]["NOTE"] = ex.Message.ToString();
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //처리결과 Popup으로 보여주기
                ShowResultPopup(dtReturnResult);
            }
        }

        /*
        // Comfrim_ReturnCell 함수 백업
        private void Comfrim_ReturnCell_OLD(string sNote)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TOTAL_QTY", typeof(int));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("RCV_NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = sAREAID;
                row["TOTAL_QTY"] = dgCellInfo.GetRowCount();
                row["USERID"] = txtWorker.Tag as string;
                row["RCV_NOTE"] = sNote;

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inPallet = indataSet.Tables.Add("INPALLET");
                inPallet.Columns.Add("BOXID", typeof(string));
                inPallet.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow row2 = inPallet.NewRow();
                row2["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "BOXID").ToString();
                row2["RCV_ISS_ID"] = sRCV_ISS_ID;

                indataSet.Tables["INPALLET"].Rows.Add(row2);

                DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                {
                    DataRow row4 = inLot.NewRow();
                    row4["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();

                    indataSet.Tables["INSUBLOT"].Rows.Add(row4);
                }

                System.Windows.Forms.Application.DoEvents();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_RETURN_SHIP_CELL_BX", "INDATA,INPALLET,INSUBLOT", null, indataSet);

                Util.MessageInfo("SFU1275");
                Init_Form();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        */

        private void ShowResultPopup(DataTable dtReturnResult)
        {
            BOX001_310_RESULT popUp = new BOX001_310_RESULT();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = dtReturnResult;

                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(result_wndConfirm_Closed);

                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }
        /// <summary>
        /// RMA 반품 처리 (ESNJ ESS Only)
        /// 특정 창고로 입고 시, Cell 리딩 없이 수량으로만 반품 처리 (이 후 폐기처리함.)
        /// </summary>
        /// <param name="sNote"></param>
        private void Comfirm_ReturnCell_RMA(string sNote)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                int iExceptCnt = 0;
                string sExceptMsg = string.Empty;

                // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                foreach (DataRow dr in drChk)
                {
                    string sRCV_ISS_ID = string.Empty;

                    sRCV_ISS_ID = dr["RCV_ISS_ID"].ToString();

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("RCV_ISS_ID", typeof(string));
                    inData.Columns.Add("AREAID", typeof(string));
                    inData.Columns.Add("RCV_QTY", typeof(string));
                    inData.Columns.Add("PROCID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("RCV_NOTE", typeof(string));
                    inData.Columns.Add("RETURN_TYPE_CODE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["RCV_ISS_ID"] = sRCV_ISS_ID;
                    row["AREAID"] = sAREAID;
                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        row["RCV_QTY"] = dr["ISS_QTY"].ToString();
                    }
                    else
                    {
                        row["RCV_QTY"] = ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'").Length;
                    }
                    row["PROCID"] = Process.CELL_BOXING;
                    row["USERID"] = txtWorker.Tag as string;
                    row["RCV_NOTE"] = sNote;
                    row["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                    indataSet.Tables["INDATA"].Rows.Add(row);


                    DataTable inPallet = indataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("BOXID", typeof(string));
                    inPallet.Columns.Add("RCV_QTY", typeof(string));


                    DataRow row2 = inPallet.NewRow();

                    row2["BOXID"] = ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'")[0]["BOXID"].ToString();
                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        row2["RCV_QTY"] = dr["ISS_QTY"].ToString();
                    }
                    else
                    {
                        row2["RCV_QTY"] = ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'").Length;
                    }

                    indataSet.Tables["INPALLET"].Rows.Add(row2);
                    //}


                    DataTable inBox = indataSet.Tables.Add("INBOX");
                    inBox.Columns.Add("BOXID", typeof(string));
                    inBox.Columns.Add("LOTID", typeof(string));

                    foreach (DataRow drReturnLot in ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'"))
                    {
                        DataRow row3 = inBox.NewRow();
                        row3["BOXID"] = drReturnLot["BOXID"].ToString();
                        row3["LOTID"] = drReturnLot["LOTID"].ToString();

                        indataSet.Tables["INBOX"].Rows.Add(row3);
                    }


                    DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                    inLot.Columns.Add("BOXID", typeof(string));
                    inLot.Columns.Add("SUBLOTID", typeof(string));

                    if (sRETURN_TYPE_CODE != "RMA")
                    {
                        foreach (DataRow drCellInfo in ((DataView)dgCellInfo.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'"))
                        {
                            DataRow row4 = inLot.NewRow();
                            row4["BOXID"] = ((DataView)dgReturnLotList.ItemsSource).ToTable().Select("RCV_ISS_ID = '" + sRCV_ISS_ID + "'")[0]["BOXID"].ToString();
                            row4["SUBLOTID"] = drCellInfo["SUBLOTID"].ToString();

                            indataSet.Tables["INSUBLOT"].Rows.Add(row4);
                        }
                    }


                    System.Windows.Forms.Application.DoEvents();
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_RETURN_SHIP_CELL_RMA_NJ_BX", "INDATA,INPALLET,INBOX,INSUBLOT", null, indataSet);

                    }
                    catch (Exception ex)
                    {
                        // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                        // 로직 처리 중 Exception 발생 시 한번에 Alert 보여주기 위하여 추가
                        iExceptCnt++;

                        if (!sExceptMsg.Equals("") || sExceptMsg != string.Empty)
                            sExceptMsg += Environment.NewLine;

                        sExceptMsg += sRCV_ISS_ID + " : " + MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));

                        //Util.MessageException(ex);

                    }
                }

                if (iExceptCnt > 0)
                {
                    Util.Alert(sExceptMsg);
                }
                else
                {
                    Util.MessageInfo("SFU1275");
                }

                Init_Form();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Scan_CellID()
        {
            try
            {
                string sCell_ID = string.Empty;
                string sRCV_ISS_ID = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk == null || drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtCell_ID.Text.ToString() == "")
                {
                    //입력한 CELL ID 가 없습니다. >> CELL ID를 입력 하세요.
                    Util.MessageValidation("SFU1319");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1319"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dgReturnLotList.GetRowCount() == 0)
                {
                    //반품 LOT 정보가 없습니다.
                    Util.MessageValidation("SFU1195");
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1195"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                double dReturnQty = Convert.ToDouble(drChk[0]["ISS_QTY"].ToString());

                int iRowCnt = dgCellInfo.GetRowCount();

                if (dReturnQty <= iRowCnt)
                {
                    //반품 수량을 넘었습니다.
                    Util.MessageValidation("SFU1551");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1551"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();
                sCell_ID = txtCell_ID.Text.ToString();

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("SRCTYPE", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["SUBLOTID"] = sCell_ID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOXID"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString();

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_RETURN", "INDATA", "OUTDATA", RQSTDT);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_CHK_SUBLOT_INFO_FOR_RETURN_BX", "INDATA", "OUTDATA", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count == 0)
                {
                    //스캔한 CELL ID의 정보가 없습니다.
                    Util.MessageValidation("SFU1689");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1689"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString() == sCell_ID)
                    {
                        //중복 스캔되었습니다.
                        Util.MessageValidation("SFU1914");
                        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1914"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Cell Scan 시 반제품 전환이 되지 않은 Cell의 경우 CELL의 PRODID가 완제품 ProdID로 조회되기 때문에 반제품 완제품 모두 비교
                if (DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "PRODID").ToString() != dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString()
                    && DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "TOP_PRODID").ToString() != dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString())
                {
                    //반품 제품과 다른 제품 입니다. >> 다른 제품을 선택하셨습니다.
                    Util.MessageValidation("SFU1480");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1480"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }


                bool bLotCheck = false;
                for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString() == " ")
                    {
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "QTY", 1);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "RETURN_QTY", 1);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "LOTID", dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString());
                        bLotCheck = true;
                        break;
                    }

                    if (DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString() == dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString())
                    {
                        int iQty = 0;

                        iQty = Convert.ToInt32(DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "QTY").ToString()) + 1;

                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "QTY", iQty);
                        DataTableConverter.SetValue(dgReturnLotList.Rows[i].DataItem, "RETURN_QTY", iQty);
                        bLotCheck = true;
                        break;
                    }
                }

                if (bLotCheck == false)
                {
                    dgReturnLotList.IsReadOnly = false;
                    dgReturnLotList.BeginNewRow();
                    dgReturnLotList.EndNewRow(true);
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "BOXID", DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "LOTID", dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "PROJECTNAME", (DataTableConverter.GetValue(dgReturnLotList.Rows[0].DataItem, "PROJECTNAME").ToString()));
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "RETURN_QTY", 1);
                    DataTableConverter.SetValue(dgReturnLotList.CurrentRow.DataItem, "QTY", 1);
                    dgReturnLotList.IsReadOnly = true;
                }


                if (dgCellInfo.GetRowCount() == 0)
                {
                    //dgCellInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA"]);
                    Util.GridSetData(dgCellInfo, dsRslt.Tables["OUTDATA"], FrameOperation, true);
                }
                else
                {
                    dgCellInfo.IsReadOnly = false;
                    dgCellInfo.BeginNewRow();
                    dgCellInfo.EndNewRow(true);
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "RCV_ISS_ID", dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "SUBLOTID", dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "TOP_PRODID", dsRslt.Tables["OUTDATA"].Rows[0]["TOP_PRODID"].ToString());
                    DataTableConverter.SetValue(dgCellInfo.CurrentRow.DataItem, "NOTE", dsRslt.Tables["OUTDATA"].Rows[0]["NOTE"].ToString());
                    dgCellInfo.IsReadOnly = true;
                }

                bScanflag = true;
                txtCell_ID.Focus();
                txtCell_ID.SelectAll();

                //dgRetrunCellList.IsEnabled = false;

                txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtCell_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Scan_CellID();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                pRCV_ISS_ID = null;

                BOX001_017_LOAD wndConfirm = new BOX001_017_LOAD();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    //object[] Parameters = new object[2];
                    //Parameters[0] = sRCV_ISS_ID;
                    //Parameters[1] = dtTempInfo;

                    //C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Load);
                    // 팝업 화면 숨겨지는 문제 수정.
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Load(object sender, EventArgs e)
        {
            BOX001_017_LOAD window = sender as BOX001_017_LOAD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                sSAVE_SEQNO = window.pSave_Seqno;
                Search_LoadList(sSAVE_SEQNO);

                bLoadflag = true;
            }
            grdMain.Children.Remove(window);
        }

        private void Search_LoadList(string sSave_no)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {   //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet ds = new DataSet();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SAVE_SEQNO", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SAVE_SEQNO"] = sSave_no;
                dr["BOXID"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "BOXID").ToString();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;

                RQSTDT.Rows.Add(dr);

                ds.Tables.Add(RQSTDT);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LOAD_CELL", "RQSTDT", "RSLTDT", RQSTDT);
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TMP_SAVE_FOR_RETURN", "INDATA", "OUTDATA_LOT,OUTDATA_SUBLOT", ds);

                Util.gridClear(dgCellInfo);
                // dgCellInfo.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA_SUBLOT"]);
                Util.GridSetData(dgCellInfo, dsRslt.Tables["OUTDATA_SUBLOT"], FrameOperation, true);

                Util.gridClear(dgReturnLotList);
                //   dgReturnLotList.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTDATA_LOT"]);
                Util.GridSetData(dgReturnLotList, dsRslt.Tables["OUTDATA_LOT"], FrameOperation, true);

                txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void btnTempStorage_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgRetrunCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                pREMARK = null;
                pUSER = null;

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                if (dgCellInfo.GetRowCount() == 0)
                {   //Cell 정보가 없습니다.
                    Util.MessageValidation("SFU1209");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1209"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataTable dtTempInfo = new DataTable();
                dtTempInfo.TableName = "RQSTDT";
                dtTempInfo.Columns.Add("RCV_ISS_ID", typeof(String));
                dtTempInfo.Columns.Add("BOXID", typeof(String));
                dtTempInfo.Columns.Add("LOTID", typeof(String));
                dtTempInfo.Columns.Add("PRODID", typeof(String));
                dtTempInfo.Columns.Add("RETURN_QTY", typeof(String));
                dtTempInfo.Columns.Add("QTY", typeof(String));
                //dtTempInfo.Columns.Add("SCAN_QTY", typeof(String));

                for (int i = 0; i < dgReturnLotList.GetRowCount(); i++)
                {
                    DataRow dr = dtTempInfo.NewRow();
                    dr["RCV_ISS_ID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "RCV_ISS_ID").ToString();
                    dr["BOXID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "BOXID").ToString();
                    dr["LOTID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "LOTID").ToString();
                    dr["PRODID"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "PRODID").ToString();
                    dr["RETURN_QTY"] = DataTableConverter.GetValue(dgReturnBoxList.Rows[0].DataItem, "RETURN_QTY").ToString();
                    dr["QTY"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "QTY").ToString();
                    //dr["SCAN_QTY"] = DataTableConverter.GetValue(dgReturnLotList.Rows[i].DataItem, "SCAN_QTY").ToString();

                    dtTempInfo.Rows.Add(dr);
                }

                BOX001_017_TEMP_CELL wndConfirm = new BOX001_017_TEMP_CELL();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sRCV_ISS_ID;
                    Parameters[1] = dtTempInfo;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Temp);
                    // 팝업 화면 숨겨지는 문제 수정
                    // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }

                bScanflag = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Temp(object sender, EventArgs e)
        {
            BOX001_017_TEMP_CELL window = sender as BOX001_017_TEMP_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                pREMARK = window.pRemark;
                //pUSER = window.pUser;
                TempStorage(pREMARK);
            }
            grdMain.Children.Remove(window);
        }

        private void TempStorage(string sRemark)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");

                string sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("NOTE", typeof(string));
                inData.Columns.Add("RCV_ISS_ID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["PACK_TMP_TYPE_CODE"] = "RETURN_CELL";
                row["USERID"] = txtWorker.Tag as string;
                row["NOTE"] = sRemark;
                row["RCV_ISS_ID"] = sRCV_ISS_ID;
                row["PRODID"] = DataTableConverter.GetValue(dgCellInfo.Rows[0].DataItem, "PRODID").ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgCellInfo.GetRowCount(); i++)
                {
                    DataRow row2 = inLot.NewRow();
                    row2["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo.Rows[i].DataItem, "SUBLOTID").ToString();

                    indataSet.Tables["INSUBLOT"].Rows.Add(row2);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TMP_PACK_CELL", "INDATA,INSUBLOT", null, indataSet);
                //정상 처리 되었습니다.
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageInfo("SFU1275");
                Init_Form();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void dgRetrunCellList2_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            CheckBox rb = sender as CheckBox;

            if (rb.DataContext == null)
                return;

            //if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            //{
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }

                //    dgRetrunCellList2.BeginEdit();
                //    dgRetrunCellList2.ItemsSource = DataTableConverter.Convert(dt);
                //    dgRetrunCellList2.EndEdit();
                //}

                //C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                //for (int i = 0; i < dg.GetRowCount(); i++)
                //{
                //    if (idx == i)
                //        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                //    else
                //        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                //}

                //row 색 바꾸기
                dgRetrunCellList2.SelectedIndex = idx;

                Util.gridClear(dgReturnBoxList2);
                Util.gridClear(dgReturnLotList2);
                Util.gridClear(dgCellInfo2);

                Return_Hist(idx);

                txtCount2.Text = "Count : " + Convert.ToString(dgCellInfo2.GetRowCount());

            //}
        }

        private void Return_Hist(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;
                //sRCV_ISS_ID = DataTableConverter.GetValue(dgRetrunCellList2.Rows[idx].DataItem, "RCV_ISS_ID").ToString();
                List<string> lstInputData = new List<string>();  //체크된   RCV_ISS_ID 담을 변수 

                DataTable dt = DataTableConverter.Convert(dgRetrunCellList2.ItemsSource);
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["CHK"].ToString().Equals("True"))
                            lstInputData.Add(dt.Rows[i]["RCV_ISS_ID"].ToString());
                    }
                }
                sRCV_ISS_ID = string.Join(",", lstInputData);   //최종 input 값  : aaaa,bbb,ccc,1111

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgReturnBoxList2);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }
                //  dgReturnBoxList2.ItemsSource = DataTableConverter.Convert(BOXList);
                Util.GridSetData(dgReturnBoxList2, BOXList, FrameOperation, true);

                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT1.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr1["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";

                RQSTDT1.Rows.Add(dr1);

                string sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST";

                if (sRETURN_TYPE_CODE_HIST == "RMA")
                {
                    sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST_RMA";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_RETURN_LOT_HIST_LIST";
                }

                DataTable LOTList = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT1);

                Util.gridClear(dgReturnLotList2);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }
                //    dgReturnLotList2.ItemsSource = DataTableConverter.Convert(LOTList);
                Util.GridSetData(dgReturnLotList2, LOTList, FrameOperation, true);

                // 반품 CELL 리스트 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT2.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT2.Columns.Add("LANGID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr2["BOX_RCV_ISS_STAT_CODE"] = "END_RECEIVE";
                dr2["LANGID"] = LoginInfo.LANGID;

                RQSTDT2.Rows.Add(dr2);

                DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_SEL_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                Util.gridClear(dgCellInfo2);

                if (CELLList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                    return;
                }
                //   dgCellInfo2.ItemsSource = DataTableConverter.Convert(CELLList);
                Util.GridSetData(dgCellInfo2, CELLList, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchList();
                //SearchList(txtRCVISSID.Text);

                txtRCVISSID.Text = string.Empty;

                /*
            Util.gridClear(dgRetrunCellList);
            Util.gridClear(dgReturnBoxList);
            Util.gridClear(dgReturnLotList);
            Util.gridClear(dgCellInfo);

            string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
            string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(String));
            RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
            RQSTDT.Columns.Add("LANGID", typeof(String));
            RQSTDT.Columns.Add("FROM_DATE", typeof(String));
            RQSTDT.Columns.Add("TO_DATE", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = sAREAID;
            dr["RCV_ISS_ID"] = String.IsNullOrEmpty(txtRCVISSID.Text) ? null : txtRCVISSID.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["FROM_DATE"] = sStart_date;
            dr["TO_DATE"] = sEnd_date;

            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

            //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
            Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);
            */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SearchList();
                    //SearchList(txtRCVISSID.Text);

                    txtRCVISSID.Text = string.Empty;
                    /*
                Util.gridClear(dgRetrunCellList);
                Util.gridClear(dgReturnBoxList);
                Util.gridClear(dgReturnLotList);
                Util.gridClear(dgCellInfo);

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["RCV_ISS_ID"] = String.IsNullOrEmpty(txtRCVISSID.Text) ? null : txtRCVISSID.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                txtRCVISSID.Text = string.Empty;
                */
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        private void txtRCVISSID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    txtRCVISSID.Text = Clipboard.GetText();

                    SearchList();
                    //SearchList(Clipboard.GetText());

                    txtRCVISSID.Text = string.Empty;

                    /*
                    Util.gridClear(dgRetrunCellList);
                    Util.gridClear(dgReturnBoxList);
                    Util.gridClear(dgReturnLotList);
                    Util.gridClear(dgCellInfo);

                    string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                    string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                    string sPasteString = Clipboard.GetText();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["AREAID"] = sAREAID;
                    dr["RCV_ISS_ID"] = String.IsNullOrEmpty(sPasteString) ? null : sPasteString;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;

                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                    //dgRetrunCellList.ItemsSource = DataTableConverter.Convert(SearchResult);
                    Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                    txtRCVISSID.Text = string.Empty;
                    */
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        #endregion

        private void SearchList()
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    if (cboReturnType.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3640");
                        return;

                    }
                }

                txtTotalPalletQty1.Value = 0;
                txtTotalIssQty1.Value = 0;

                Util.gridClear(dgRetrunCellList);
                Util.gridClear(dgReturnBoxList);
                Util.gridClear(dgReturnLotList);
                Util.gridClear(dgCellInfo);

                sRETURN_TYPE_CODE = string.Empty;

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sPasteString = Clipboard.GetText();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("OCOP_RSO_NO", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
				RQSTDT.Columns.Add("EQSGID", typeof(String)); //2024.02.26 Adira: Add Equipment Line

				if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                    if (Util.NVC(cboReturnType.SelectedValue) == "RMA")
                    {
                        RQSTDT.Columns.Add("IN", typeof(string));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("NOTIN", typeof(string));
                    }
                }

                string sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;

                if (string.IsNullOrEmpty(txtOCOPRSONO.Text) && string.IsNullOrEmpty(txtRCVISSID.Text))
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }
                else
                {
                    dr["OCOP_RSO_NO"] = string.IsNullOrEmpty(txtOCOPRSONO.Text) ? null : Util.NVC(txtOCOPRSONO.Text); // 2024.07.05 이현승
                    dr["RCV_ISS_ID"] = string.IsNullOrEmpty(txtRCVISSID.Text) ? null : Util.NVC(txtRCVISSID.Text);
                }
                //dr["RCV_ISS_ID"] = string.IsNullOrEmpty(RcvIssId) ? null : RcvIssId;
                dr["LANGID"] = LoginInfo.LANGID;
				dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
																					    

				if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    sRETURN_TYPE_CODE = Util.NVC(cboReturnType.SelectedValue);
                    dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;

                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        dr["IN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_CELL_LIST_RMA";
                    }
                    else
                    {
                        dr["NOTIN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";
                    }
                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgRetrunCellList, SearchResult, FrameOperation, true);


                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgRetrunCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgRetrunCellList.Columns["PALLET_QTY"], dac);

                //2023-11-29 총 PALLET, 입고, 반품 수 조회
                int SumPallet = 0;
                int SumIss = 0;

                for (int lsCount = 0; lsCount < dgRetrunCellList.GetRowCount(); lsCount++)
                {
                    SumPallet = SumPallet + Util.NVC_Int(dgRetrunCellList.GetCell(lsCount, dgRetrunCellList.Columns["PALLET_QTY"].Index).Value);
                    SumIss = SumIss + Util.NVC_Int(dgRetrunCellList.GetCell(lsCount, dgRetrunCellList.Columns["ISS_QTY"].Index).Value);
                }

                txtTotalPalletQty1.Value = SumPallet;
                txtTotalIssQty1.Value = SumIss;


                txtRCVISSID.Text = string.Empty;

                if (SearchResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2951");  //조회결과가 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #region Mehod


        #endregion

        private void dgRetrunCellList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //Checked = "dgRetrunCellList_Choice_Checked"

            dgRetrunCellList.Dispatcher.BeginInvoke(new Action(() =>
            {
                //C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                        e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null)
                {
                    RadioButton rdo = e.Cell.Presenter.Content as RadioButton;
                    if (rdo != null)
                    {
                        rdo.Checked -= dgRetrunCellList_Choice_Checked;
                        rdo.Checked += dgRetrunCellList_Choice_Checked;
                    }
                }
            }));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Init_Form();
                }
            });
        }

        private void Init_Form()
        {
            //dgRetrunCellList.IsEnabled = true;
            bScanflag = false;
            bLoadflag = false;

            Util.gridClear(dgRetrunCellList);
            Util.gridClear(dgReturnBoxList);
            Util.gridClear(dgReturnLotList);
            Util.gridClear(dgCellInfo);

            //SearchCell_ReturnList();

            txtCount1.Text = "Count : 0";
            txtCount2.Text = "Count : 0";

            txtCell_ID.Text = null;
            txtCell_ID.Focus();
        }

        private void Init_Form2()
        {
            bScanflag = false;
            bLoadflag = false;

            Util.gridClear(dgSampleCellList);
            Util.gridClear(dgSampleBoxList);
            Util.gridClear(dgSampleLotList);
            Util.gridClear(dgCellInfo1);

            txtCount.Text = "Count : 0";

            txtCell_ID.Text = null;
            txtCell_ID.Focus();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchHistList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SearchHistList()
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    if (cboReturnTypeHist.SelectedValue.Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3640");
                        return;
                    }
                }
                txtTotalPalletQty2.Value = 0;
                txtTotalRcvQty2.Value = 0;
                txtTotalIssQty2.Value = 0;

                Util.gridClear(dgRetrunCellList2);
                Util.gridClear(dgReturnBoxList2);
                Util.gridClear(dgReturnLotList2);
                Util.gridClear(dgCellInfo2);

                sRETURN_TYPE_CODE_HIST = string.Empty;

                string sStart_date = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                string sEnd_date = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo2.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);
                string sLoc = string.Empty;

                if (cboTransLoc.SelectedIndex < 0 || cboTransLoc.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sLoc = null;
                }
                else
                {
                    sLoc = cboTransLoc.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_SLOC_ID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
				RQSTDT.Columns.Add("EQSGID", typeof(String)); //2024.02.26 Adira - Add Line information
                RQSTDT.Columns.Add("OCOP_RSO_NO", typeof(String)); //2024.07.05 이현승 - 사외반품번호 추가

                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));

                    if (Util.NVC(cboReturnTypeHist.SelectedValue) == "RMA")
                    {
                        RQSTDT.Columns.Add("IN", typeof(string));
                    }
                    else
                    {
                        RQSTDT.Columns.Add("NOTIN", typeof(string));
                    }
                }

                string sBizName = "DA_PRD_SEL_RETURN_HIST";

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_AREAID"] = sAREAID2;
                dr["FROM_SLOC_ID"] = sLoc;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment2, bAllNull: true);

                if (string.IsNullOrEmpty(txtOCOPRSONO2.Text))
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }
                else
                {
                    dr["OCOP_RSO_NO"] = string.IsNullOrEmpty(txtOCOPRSONO2.Text) ? null : Util.NVC(txtOCOPRSONO2.Text);
                }

                if (LoginInfo.CFG_SHOP_ID == "G184")
                {
                    sRETURN_TYPE_CODE_HIST = Util.NVC(cboReturnTypeHist.SelectedValue);
                    dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE_HIST;

                    if (sRETURN_TYPE_CODE_HIST == "RMA")
                    {
                        dr["IN"] = "Y";
                        sBizName = "DA_PRD_SEL_RETURN_HIST_RMA";
                    }
                    else
                    {
                        dr["NOTIN"] = "Y";
                        sBizName = "DA_SEL_RETURN_SHIP_HIST_BX";
                    }
                }

                RQSTDT.Rows.Add(dr);

                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_RETURN_SHIP_HIST_BX", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgRetrunCellList2, SearchResult, FrameOperation, true);

                //2023-11-29 총 PALLET, 입고, 반품 수 조회
                int SumPallet = 0;
                int SumRcv = 0;
                int SumIss = 0;

                for (int lsCount = 0; lsCount < dgRetrunCellList2.GetRowCount(); lsCount++)
                {
                    SumPallet = SumPallet + Util.NVC_Int(dgRetrunCellList2.GetCell(lsCount, dgRetrunCellList2.Columns["PALLET_QTY"].Index).Value);
                    SumRcv = SumRcv + Util.NVC_Int(dgRetrunCellList2.GetCell(lsCount, dgRetrunCellList2.Columns["RCV_QTY"].Index).Value);
                    SumIss = SumIss + Util.NVC_Int(dgRetrunCellList2.GetCell(lsCount, dgRetrunCellList2.Columns["ISS_QTY"].Index).Value);
                }

                txtTotalPalletQty2.Value = SumPallet;
                txtTotalRcvQty2.Value = SumRcv;
                txtTotalIssQty2.Value = SumIss;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private string sAREAID = string.Empty;
        private string sSHOPID = string.Empty;

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

			//2024-02-26 Adira: Add Line information
			CommonCombo combo = new CommonCombo();
			combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID }, sCase: "LINE_FCS");
            cboEquipmentSegment.SelectedIndex = 0;

        }

        private string sAREAID1 = string.Empty;
        private string sSHOPID1 = string.Empty;

        private void cboArea1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID1 = "";
                sSHOPID1 = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID1 = sArry[0];
                sSHOPID1 = sArry[1];
            }
        }

        private string sAREAID2 = string.Empty;
        private string sSHOPID2 = string.Empty;

        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea2.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID2 = "";
                sSHOPID2 = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID2 = sArry[0];
                sSHOPID2 = sArry[1];
            }

			//2024-02-26 Adira: Add Line information
			CommonCombo combo = new CommonCombo();
			combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID2 }, sCase: "LINE_FCS");
            cboEquipmentSegment2.SelectedIndex = 0;
		}



		private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRetrunCellList.ItemsSource == null || dgRetrunCellList.Rows.Count == dgRetrunCellList.BottomRows.Count)
                {
                    // 조회 결과가 없습니다.
                    Util.MessageValidation("SFU2816");
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgRetrunCellList, "CHK");
                if (drChk.Length <= 0)
                {   //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1651"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("SUBLOTID", typeof(string));

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            DataRow dataRow = dataTable.NewRow();
                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                dataRow["SUBLOTID"] = cell.Text;
                            }

                            dataTable.Rows.Add(dataRow);
                        }

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            txtCell_ID.Text = dataTable.Rows[i]["SUBLOTID"].ToString();

                            Scan_CellID();

                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            GetSampleList();
        }

        private void GetSampleList()
        {
            try
            {
                Util.gridClear(dgSampleCellList);
                Util.gridClear(dgSampleBoxList);
                Util.gridClear(dgSampleLotList);
                Util.gridClear(dgCellInfo1);

                string sStart_date = dtpDateFrom1.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo1.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd");  //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["RCV_ISS_ID"] = string.IsNullOrWhiteSpace(txtRCVISSID.Text) ? null : txtRCVISSID.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SMPL_SHIP_LIST_BX", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgSampleCellList, SearchResult, FrameOperation, true);

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgSampleCellList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgSampleCellList.Columns["PALLET_QTY"], dac);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgSampleCellList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            int idx = 0;

            if (sender == null)
                return;


            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if (bScanflag1 == true && iBack == 1)
            {
                iBack = 0;
                return;
            }

            if (bLoadflag1 == true && iBack == 1)
            {
                iBack = 0;
                return;
            }


            if (bScanflag1 == true || bLoadflag1 == true)
            {
                //스캔한 데이터를 초기화 하시겠습니까?
                Util.MessageConfirm("SFU3133", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;


                            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                            for (int i = 0; i < dg.GetRowCount(); i++)
                            {
                                if (idx == i)
                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                                else
                                    DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                            }

                            //row 색 바꾸기
                            dgRetrunCellList.SelectedIndex = idx;

                            Util.gridClear(dgSampleBoxList);
                            Util.gridClear(dgSampleLotList);
                            Util.gridClear(dgCellInfo1);

                            Sample_Box_List(idx);

                            bScanflag1 = false;
                            bLoadflag1 = false;

                            txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo1.GetRowCount());

                            iNow_Index = idx;
                            iOld_Index = idx;
                            iBack = 0;
                        }
                    }
                    else
                    {

                        if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                        {

                            DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                            if (dt != null)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    DataRow row = dt.Rows[i];

                                    if (iOld_Index == i)
                                        dt.Rows[i]["CHK"] = true;
                                    else
                                        dt.Rows[i]["CHK"] = false;
                                }

                                dgSampleCellList.BeginEdit();
                                Util.GridSetData(dgSampleCellList, dt, FrameOperation, true);
                                dgSampleCellList.EndEdit();

                                iBack = 1;
                            }

                        }
                    }

                });
            }
            else
            {
                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    iNow_Index = idx;

                    DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow row = dt.Rows[i];

                            if (idx == i)
                                dt.Rows[i]["CHK"] = true;
                            else
                                dt.Rows[i]["CHK"] = false;
                        }

                        dgSampleCellList.BeginEdit();
                        Util.GridSetData(dgSampleCellList, dt, FrameOperation, true);
                        dgSampleCellList.EndEdit();
                    }

                    //row 색 바꾸기
                    dgSampleCellList.SelectedIndex = idx;

                    Util.gridClear(dgSampleBoxList);
                    Util.gridClear(dgSampleLotList);
                    Util.gridClear(dgCellInfo1);

                    Sample_Box_List(idx);

                    txtCount.Text = "Count : " + Convert.ToString(dgCellInfo1.GetRowCount());

                    iOld_Index = iNow_Index;
                    iBack = 0;
                }
            }
        }

        private void btnConfirm2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSampleCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgSampleCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1645");    //선택된 작업대상이 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                double dReturnQty = Convert.ToDouble(drChk[0]["ISS_QTY"].ToString());

                int iRowCnt = dgCellInfo1.GetRowCount();

                if (dReturnQty != iRowCnt)
                {
                    Util.MessageValidation("SFU3134");  //반품수량과 Cell 수량이 일치하지 않습니다.
                    return;
                }

                DataTable returnlist = new DataTable();
                returnlist.Columns.Add("RCV_ISS_ID", typeof(string));
                returnlist.Columns.Add("BOXID", typeof(string));
                returnlist.Columns.Add("PRODID", typeof(string));
                returnlist.Columns.Add("PRJT_NAME", typeof(string));
                returnlist.Columns.Add("ISS_QTY", typeof(string));

                for (int row = 0; row < dgSampleLotList.Rows.Count; row++)
                {
                    DataRow dr = returnlist.NewRow();
                    dr["RCV_ISS_ID"] = Util.NVC(dgSampleLotList.GetCell(row, dgSampleLotList.Columns["RCV_ISS_ID"].Index).Value);
                    dr["BOXID"] = Util.NVC(dgSampleLotList.GetCell(row, dgSampleLotList.Columns["BOXID"].Index).Value);
                    dr["PRODID"] = Util.NVC(dgSampleLotList.GetCell(row, dgSampleLotList.Columns["PRODID"].Index).Value);
                    dr["PRJT_NAME"] = Util.NVC(dgSampleLotList.GetCell(row, dgSampleLotList.Columns["PROJECTNAME"].Index).Value);
                    dr["ISS_QTY"] = Util.NVC(dgSampleLotList.GetCell(row, dgSampleLotList.Columns["RETURN_QTY"].Index).Value);
                    returnlist.Rows.Add(dr);
                }

                BOX001_310_CONFIRM wndConfirm = new BOX001_310_CONFIRM(iRowCnt);
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = returnlist;
                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm2_Closed);

                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void wndConfirm2_Closed(object sender, EventArgs e)
        {
            BOX001_310_CONFIRM window = sender as BOX001_310_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                System.Windows.Forms.Application.DoEvents();
                Comfirm_SampleCell(window.sNOTE);
            }
            grdMain.Children.Remove(window);
        }

        private void Comfirm_SampleCell(string sNote)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataRow[] drChk = Util.gridGetChecked(ref dgSampleCellList, "CHK");
                if (drChk.Length <= 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = drChk[0]["RCV_ISS_ID"].ToString();

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TOTAL_QTY", typeof(int));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("RCV_NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = sAREAID;
                row["TOTAL_QTY"] = dgCellInfo1.GetRowCount();
                row["USERID"] = txtWorker.Tag as string;
                row["RCV_NOTE"] = sNote;

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inPallet = indataSet.Tables.Add("INPALLET");
                inPallet.Columns.Add("BOXID", typeof(string));
                inPallet.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow row2 = inPallet.NewRow();
                row2["BOXID"] = DataTableConverter.GetValue(dgSampleLotList.Rows[0].DataItem, "BOXID").ToString();
                row2["RCV_ISS_ID"] = sRCV_ISS_ID;

                indataSet.Tables["INPALLET"].Rows.Add(row2);

                DataTable inLot = indataSet.Tables.Add("INSUBLOT");
                inLot.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgCellInfo1.GetRowCount(); i++)
                {
                    DataRow row4 = inLot.NewRow();
                    row4["SUBLOTID"] = DataTableConverter.GetValue(dgCellInfo1.Rows[i].DataItem, "SUBLOTID").ToString();

                    indataSet.Tables["INSUBLOT"].Rows.Add(row4);
                }

                System.Windows.Forms.Application.DoEvents();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SMPL_SHIP_CELL_BX", "INDATA,INPALLET,INSUBLOT", null, indataSet);

                Util.MessageInfo("SFU1275");
                Init_Form2();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Sample_Box_List(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgSampleCellList.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                // 반품 BOX 리스트 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["BOX_RCV_ISS_STAT_CODE"] = "SMPL_SHIP"; // 미판정 반품

                RQSTDT.Rows.Add(dr);

                DataTable BOXList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_BOX_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgSampleBoxList);

                if (BOXList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1547");  //반품 BOX 리스트 조회 항목이 없습니다.
                    return;
                }

                Util.GridSetData(dgSampleBoxList, BOXList, FrameOperation, true);


                // 반품 LOT 리스트 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT1.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr1["BOX_RCV_ISS_STAT_CODE"] = "SMPL_SHIP"; // 미판정 반품

                RQSTDT1.Rows.Add(dr1);

                DataTable LOTList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LOT_LIST", "RQSTDT", "RSLTDT", RQSTDT1);

                Util.gridClear(dgSampleLotList);

                if (LOTList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1549");  //반품 LOT 리스트 조회 항목이 없습니다.
                    return;
                }

                Util.GridSetData(dgSampleLotList, LOTList, FrameOperation, true);

                if (DataTableConverter.GetValue(dgSampleCellList.Rows[idx].DataItem, "CELL_INFO").ToString() == "Y")
                {
                    // 반품 CELL 리스트 조회
                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT2.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["RCV_ISS_ID"] = sRCV_ISS_ID;
                    dr2["BOX_RCV_ISS_STAT_CODE"] = "SMPL_SHIP"; // 미판정 반품

                    RQSTDT2.Rows.Add(dr2);

                    DataTable CELLList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                    Util.gridClear(dgCellInfo1);

                    if (CELLList.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU1548");  //반품 CELL 리스트 조회 항목이 없습니다.
                        return;
                    }
                    Util.GridSetData(dgCellInfo1, CELLList, FrameOperation, true);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 자동 OQC 샘플 선정 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodeArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "OQC_SMPL_TRGT_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

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

        /// <summary>
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

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

        private void SetReturnTypeCombo()
        {
            try
            {
                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string)); ;

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "RETURN_TYPE_CODE";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "SELECT";
                dr["CBO_NAME"] = "- SELECT -";
                dtResult.Rows.InsertAt(dr, 0);

                cboReturnType.DisplayMemberPath = "CBO_NAME";
                cboReturnType.SelectedValuePath = "CBO_CODE";
                cboReturnType.ItemsSource = dtResult.Copy().AsDataView();
                cboReturnType.SelectedIndex = 0;

                cboReturnTypeHist.DisplayMemberPath = "CBO_NAME";
                cboReturnTypeHist.SelectedValuePath = "CBO_CODE";
                cboReturnTypeHist.ItemsSource = dtResult.Copy().AsDataView();
                cboReturnTypeHist.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = $"Cell_ReturnList_{ DateTime.Now.ToString("yyyyMMdd_ss") }.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    c1XLBook1.Sheets.Add(); // 시트한개더 추가(이거 찾느라 고생함 ㅜ.ㅜ)

                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    //sheet1 / cell 반품리스트 시작.
                    DataTable dt0Info = DataTableConverter.Convert(dgRetrunCellList2.ItemsSource);  //cell 반품확정

                    DataTable dt0 = dt0Info.Select("CHK = 'True'").CopyToDataTable();

                    XLSheet sheet0 = c1XLBook1.Sheets[0];
                    sheet0.Name = "returned cell sheet";

                    sheet0[0, 0].Style = styel;
                    sheet0[0, 1].Style = styel;
                    sheet0[0, 2].Style = styel;
                    sheet0[0, 3].Style = styel;
                    sheet0[0, 4].Style = styel;
                    sheet0[0, 5].Style = styel;
                    sheet0[0, 6].Style = styel;
                    sheet0[0, 7].Style = styel;

                    sheet0.Columns[0].Width = 3000;
                    sheet0.Columns[1].Width = 2200;
                    sheet0.Columns[2].Width = 2200;
                    sheet0.Columns[3].Width = 2200;
                    sheet0.Columns[4].Width = 2200;
                    sheet0.Columns[5].Width = 2200;
                    sheet0.Columns[6].Width = 2200;
                    sheet0.Columns[7].Width = 3000;

                    sheet0[0, 0].Value = "Return No.";
                    sheet0[0, 1].Value = "Return Status";
                    sheet0[0, 2].Value = "From W/H Name";
                    sheet0[0, 3].Value = "TO  W/H Name";
                    sheet0[0, 4].Value = "Box Qty";
                    sheet0[0, 5].Value = "Receiv'g QTY";
                    sheet0[0, 6].Value = "Return QTY";
                    sheet0[0, 7].Value = "Remark";

                    for (int i = 0; i < dt0.Rows.Count; i++)    //체크된 것만.
                    {
                        if (dt0.Rows[i]["CHK"].ToString().Equals("True"))
                        {
                            sheet0[i + 1, 0].Value = dt0.Rows[i]["RCV_ISS_ID"].ToString();
                            sheet0[i + 1, 1].Value = dt0.Rows[i]["RCV_ISS_STAT_CODE_DESC"].ToString();
                            sheet0[i + 1, 2].Value = dt0.Rows[i]["FROM_SLOC_ID_DESC"].ToString();
                            sheet0[i + 1, 3].Value = dt0.Rows[i]["TO_SLOC_ID_DESC"].ToString();
                            sheet0[i + 1, 4].Value = dt0.Rows[i]["PALLET_QTY"].ToString();
                            sheet0[i + 1, 5].Value = dt0.Rows[i]["RCV_QTY"].ToString();
                            sheet0[i + 1, 6].Value = dt0.Rows[i]["ISS_QTY"].ToString();
                            sheet0[i + 1, 7].Value = dt0.Rows[i]["RCV_NOTE"].ToString();
                        }
                    }

                    // sheet2 - cell정보 시작
                    DataTable dt1 = DataTableConverter.Convert(dgCellInfo2.ItemsSource);
                    XLSheet sheet1 = c1XLBook1.Sheets[1];
                    sheet1.Name = "CELL Info sample sheet";

                    sheet1[0, 0].Style = styel;
                    sheet1[0, 1].Style = styel;
                    sheet1[0, 2].Style = styel;
                    sheet1[0, 3].Style = styel;

                    sheet1.Columns[0].Width = 2200;
                    sheet1.Columns[1].Width = 2200;
                    sheet1.Columns[2].Width = 2200;
                    sheet1.Columns[3].Width = 3000;

                    sheet1[0, 0].Value = "Cell ID";
                    sheet1[0, 1].Value = "Item No.";
                    sheet1[0, 2].Value = "Return Reason";
                    sheet1[0, 3].Value = "Return No.";

                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        sheet1[i + 1, 0].Value = dt1.Rows[i]["SUBLOTID"].ToString();
                        sheet1[i + 1, 1].Value = dt1.Rows[i]["TOP_PRODID"].ToString();
                        sheet1[i + 1, 2].Value = dt1.Rows[i]["RTN_RSN_NOTE"].ToString();
                        sheet1[i + 1, 3].Value = dt1.Rows[i]["RCV_ISS_ID"].ToString();
                    }

                    //최종 다운로드
                    c1XLBook1.Save(od.FileName);
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRetrunCellList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgRetrunCellList.CurrentRow == null || dgRetrunCellList.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgRetrunCellList.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgRetrunCellList.GetCell(dgRetrunCellList.CurrentRow.Index, dgRetrunCellList.Columns["CHK"].Index).Value);
                    if (chkValue == "0")
                    {
                        if (bScanflag == true && iBack == 1)
                        {
                            iBack = 0;
                            return;
                        }

                        int idx = 0;

                        // RMA 반품 선택 후 Cell 스캔을 진행한 경우 다른 반품ID를 선택했을때 초기화 확인
                        if (bScanflag == true || bLoadflag == true)
                        {
                            //스캔한 데이터를 초기화 하시겠습니까?
                            Util.MessageConfirm("SFU3133", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    if (chkValue == "0")
                                    {
                                        // 리스트 전체 체크박스 체크 해제
                                        for (int i = 0; i < dgRetrunCellList.Rows.Count; i++)
                                        {
                                            DataTableConverter.SetValue(dgRetrunCellList.Rows[i].DataItem, "CHK", false);
                                        }

                                        // 선택된 Row의 체크박스 체크 처리
                                        DataTableConverter.SetValue(dgRetrunCellList.Rows[dgRetrunCellList.SelectedIndex].DataItem, "CHK", true);

                                        // 조회 Grid 초기화
                                        Util.gridClear(dgReturnBoxList);
                                        Util.gridClear(dgReturnLotList);
                                        Util.gridClear(dgCellInfo);

                                        // 선택된 반품 조회
                                        Return_Box_List(dgRetrunCellList.SelectedIndex);

                                        bScanflag = false;
                                        bLoadflag = false;

                                        txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());

                                        iNow_Index = idx;
                                        iOld_Index = idx;
                                        iBack = 0;
                                    }
                                }
                            });
                        }
                        else 
                        {
                            string selectedCellInfo = DataTableConverter.GetValue(dgRetrunCellList.Rows[dgRetrunCellList.CurrentRow.Index].DataItem, "CELL_INFO").ToString();

                            for (int i = 0; i < dgRetrunCellList.GetRowCount(); i++)
                            {
                                string chk = DataTableConverter.GetValue(dgRetrunCellList.Rows[i].DataItem, "CHK").ToString();

                                if (chk == "1")
                                {
                                    string cellInfo = DataTableConverter.GetValue(dgRetrunCellList.Rows[i].DataItem, "CELL_INFO").ToString();

                                    if (selectedCellInfo == "N")
                                    {
                                        if (cellInfo == "N")
                                        {
                                            // 기존 선택된 반품이 RMA반품이고 현재 선택한 반품도 RMA인 경우
                                            Util.MessageValidation("SFU4394"); // RMA 반품은 1건만 선택할 수 있습니다. 
                                            return;
                                        } 
                                        else
                                        {
                                            // 기존 선택된 반품이 RMA반품이고 현재 선택한 반품은 일반 반품 경우
                                            Util.MessageValidation("SFU4395"); // 일반 반품과 RMA반품은 함께 선택할 수 없습니다.
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (cellInfo == "N")
                                        {
                                            // 기존 선택된 반품이 일반 반품이고 현재 선택한 반품은 RMA 반품 경우
                                            Util.MessageValidation("SFU4395"); // 일반 반품과 RMA반품은 함께 선택할 수 없습니다.
                                            return;
                                        }
                                    }
                                }
                            }

                            DataTableConverter.SetValue(dgRetrunCellList.Rows[dgRetrunCellList.CurrentRow.Index].DataItem, "CHK", true);

                            Return_Box_List(dgRetrunCellList.CurrentRow.Index);
                        }
                    }
                    else
                    {
                        // 체크박스 해제 시
                        string selectedRcvIssID = DataTableConverter.GetValue(dgRetrunCellList.Rows[dgRetrunCellList.CurrentRow.Index].DataItem, "RCV_ISS_ID").ToString();

                        DataTableConverter.SetValue(dgRetrunCellList.Rows[dgRetrunCellList.CurrentRow.Index].DataItem, "CHK", false);
                        DataTable dtBoxInfo = DataTableConverter.Convert(dgReturnBoxList.ItemsSource);
                        DataTable dtLotInfo = DataTableConverter.Convert(dgReturnLotList.ItemsSource);
                        DataTable dtCellInfo = DataTableConverter.Convert(dgCellInfo.ItemsSource);

                        for (int i = dtBoxInfo.Rows.Count; i > 0; i--)
                        {
                            if (dtBoxInfo.Rows[i - 1]["RCV_ISS_ID"].ToString() == selectedRcvIssID)
                            {
                                dtBoxInfo.Rows[i - 1].Delete();
                            }
                        }

                        for (int i = dtLotInfo.Rows.Count; i > 0; i--)
                        {
                            if (dtLotInfo.Rows[i - 1]["RCV_ISS_ID"].ToString() == selectedRcvIssID)
                            {
                                dtLotInfo.Rows[i - 1].Delete();
                            }
                        }

                        for (int i = dtCellInfo.Rows.Count; i > 0; i--)
                        {
                            if (dtCellInfo.Rows[i - 1]["RCV_ISS_ID"].ToString() == selectedRcvIssID)
                            {
                                dtCellInfo.Rows[i - 1].Delete();
                            }
                        }
                        Util.GridSetData(dgReturnBoxList, dtBoxInfo, FrameOperation, true);
                        Util.GridSetData(dgReturnLotList, dtLotInfo, FrameOperation, true);
                        Util.GridSetData(dgCellInfo, dtCellInfo, FrameOperation, true);

                        txtCount1.Text = "Count : " + Convert.ToString(dgCellInfo.GetRowCount());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgRetrunCellList.CurrentRow = null;
            }
        }

        private void txtOCOPRSONO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SearchList();
                    txtOCOPRSONO.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void txtOCOPRSONO_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    txtOCOPRSONO.Text = Clipboard.GetText();

                    SearchList();
                    txtOCOPRSONO.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void txtOCOPRSONO2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    SearchHistList();
                    txtOCOPRSONO2.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void txtOCOPRSONO2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    txtOCOPRSONO2.Text = Clipboard.GetText();

                    SearchHistList();
                    txtOCOPRSONO2.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

    }
}
