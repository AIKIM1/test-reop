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
  2023.02.21     이제섭 : 모델 Lot 콤보박스 라인 미선택시에도 조회되도록 변경
  2023.05.23     최경아 : LotType 조회정보 추가 
  2023.05.30     최경아 : 장기재고 항목 추가
  2023.05.31     조영대 : Pallet Barcode ID 컬럼 추가
  2023.07.20     김동훈 : TOP_PRODID 제품ID 추가
  2023.09.25     박나연 : 조회 필터 라인, 모델 복수선택 가능하도록 수정. 활성화 재고 조회 시 미검사/검사완료 재고로 조회 가능하도록 추가. HOLD 재고 클릭 시, HOLD 세부내역 표시 체크박스 추가하여 해당 체크박스 체크 시 HOLD 세부내역 조회되도록 수정.
  2023.10.20     임근영 : UNCODE 유효기간 컬럼 추가 
  2023.11.24     최경아 : 모든 재고 조회 시, 'HOLD 세부 내역 표기' 체크박스 표기되도록 수정.
  2024.02.19     박나연 : 대표 PJT 입력 시, 해당 대표 PJT에 해당하는 모델만 조회되도록 수정
  2024.05.16     박나연 : 포장 타입 선택 시, 활성화 기준(공통코드 FORM_PACKING_WRK_TYPE_CODE)로 변경
  2024.07.10     Zahran : E20240703-001283 Add copy and drag function to stand-by lot info grid (GDC)
  2024.09.04     최경아 : E20240813-000343 최종보류여부사유 컬럼 추가 및 'HOLD 세부 내역 표기' 체크박스 표기시 최종보류여부사유 컬럼 조회 되도록 수정.
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
using System.Linq;
using C1.WPF.DataGrid.Summaries; //20220120_합계출력

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_304 : System.Windows.Controls.UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();

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
        double _SHIP_WAIT_SPQ;
        double _SHIP_WAIT_SCQ;
        double _PACK_WAIT_SPQ;
        double _PACK_WAIT_SCQ;

        string judgeValueY = string.Empty;
        string ncrOk = string.Empty;
        Style styleGreen;
        Style styleGray;

        public BOX001_304()
        {
            InitializeComponent();
            Loaded += BOX001_304_Loaded;
        }

        private void BOX001_304_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_304_Loaded;

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
            string[] sFilter5 = { "FORM_PACKING_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            //포장 Lottype 세팅
            _combo.SetCombo(boxLottype, CommonCombo.ComboStatus.ALL, sCase: "LOTTYPE");

            // UNCODE 사용 Plant는 UN_CODE 컬럼 Visible 처리
            if (UseCommoncodePlant())
            {
                dgPalletInfo.Columns["UN_CODE"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["VLD_PERIOD"].Visibility = Visibility.Visible;    //유효기간 추가. 
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

            dgPalletInfo.SetColumnVisibleForCommonCode("PLLT_BCD_ID", "CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
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

            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                sEqsgid = null;
            }
            Geqsgid = sEqsgid;

            string sMDLLot = SelectEquipment(cboModelLot);
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

            string sBLotType = Util.NVC(boxLottype.SelectedValue);
            if (sBLotType == "" || sBLotType == "SELECT")
            {
                sBLotType = null;
            }
            // 출고대기 LOT Summary 조회
            GetSummaryInfo(sAREAID, sEqsgid, sMDLLot, sLotType, sBLotType);

            dgPalletInfo.ItemsSource = null;

            chkHoldDetail.Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["MES_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["CELL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["PALLET_HOLD_NOTE"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["MES_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["QMS_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["CELL_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
          //  dgPalletInfo.Columns["PALLET_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
            dgPalletInfo.Columns["QMS_FINL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
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

            SetLine_CP2(sAREAID);

        }

        private void txtModelLot_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetModelLot2();
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            SetModelLot2();
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

                    if (sColName == "CHK_TOTAL" || sColName == "CHK_PROC" || sColName == "CHK_EXP" || sColName == "CHK_HOLD" || sColName == "CHK_INSP" || sColName == "CHK_NO_INSP" || sColName == "CHK_OWMS" || sColName == "CHK_SHIP_WAIT")
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
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_INSP", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_NO_INSP", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_EXP", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_HOLD", false);
                            DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_SHIP_WAIT", false);
                       //     DataTableConverter.SetValue(dgSummary.Rows[i].DataItem, "CHK_PACK_WAIT", false);
                        }

                        if (sChkValue == "0")
                        {
                            DataTableConverter.SetValue(dgSummary.Rows[dgSummary.CurrentRow.Index].DataItem, sColName, true);
                            GetMMDShipToPackCondAuto(sEqsgid, sProdid);
                            GetQMSBlockBas();
                            GetPalletInfo(sAreaid, sEqsgid, sLotType, sProdid, sColName, sBLotType, sViewAll, sSchEqsgid, sSchLotId, sSchPackType, sSchLottype);
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
                //2024.07.10 Zahran Commented out the line below to enable copy feature using Ctrl+C features
                //dgSummary.CurrentRow = null;
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
                // GMES 2.0에서는 PW랏 사용하지 않아서 로직 제거
                //// 포장 대기 Lot인 경우
                //if (sPalletid.StartsWith("PW"))
                //{
                //    loadingIndicator.Visibility = Visibility.Visible;
                //    string[] sParam = { sPalletid };
                //    // 활성화 Lot 관리
                //    this.FrameOperation.OpenMenu("SFU010705220", true, sParam);
                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                    loadingIndicator.Visibility = Visibility.Visible;
                    string[] sParam = { sAREAID, sSHOPID, sPalletid };
                    // 기간별 Pallet 확정 이력 정보 조회
                    this.FrameOperation.OpenMenu("SFU010736080", true, sParam);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                //}
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
                    int ProcNPQ = 0; // 활성화 미검사 재고
                    int ProcNCQ = 0;
                    int ProcIPQ = 0; // 활성화 검사완료 재고
                    int ProcICQ = 0;
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
                        ProcIPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_IPQ"])));
                        ProcICQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_ICQ"])));
                        ProcNPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_NPQ"])));
                        ProcNCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PROC_NCQ"])));
                        OwmsOPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["OWMS_OPQ"])));
                        OwmsOCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["OWMS_OCQ"])));
                        ExpEPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["EXP_EPQ"])));
                        ExpECQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["EXP_ECQ"])));
                        HoldHPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HOLD_HPQ"])));
                        HoldHCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["HOLD_HCQ"])));
                        ShipWaitSPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP_WAIT_SPQ"])));
                        ShipWaitSCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["SHIP_WAIT_SCQ"])));
                        //PackWaitSPQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PACK_WAIT_SPQ"])));
                        //PackWaitSCQ += Convert.ToInt32(Util.NVC(Convert.ToString(dtResult.Rows[iRow]["PACK_WAIT_SCQ"])));
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgSummary.ItemsSource);
                    dtInfo.Columns.Add("VIEW_ALL", typeof(string));
                    dtInfo.Columns.Add("SCH_EQSGID", typeof(string));
                    dtInfo.Columns.Add("SCH_MDLLOT_ID", typeof(string));
                    dtInfo.Columns.Add("SCH_PACK_WRK_TYPE_CODE", typeof(string));
                    dtInfo.Columns.Add("SCH_LOTTYPE", typeof(string));
                    DataRow drr = dtInfo.NewRow();
                    drr["TOTAL_QTY"] = TotalTPQ + "(" + TotalTCQ + ")";
                    drr["PROC_QTY"] = ProcPPQ + "(" + ProcPCQ + ")";
                    drr["INSP_QTY"] = ProcIPQ + "(" + ProcICQ + ")";
                    drr["NO_INSP_QTY"] = ProcNPQ + "(" + ProcNCQ + ")";
                    drr["OWMS_QTY"] = OwmsOPQ + "(" + OwmsOCQ + ")";
                    drr["EXP_QTY"] = ExpEPQ + "(" + ExpECQ + ")";
                    drr["HOLD_QTY"] = HoldHPQ + "(" + HoldHCQ + ")";
                    drr["SHIP_WAIT_QTY"] = ShipWaitSPQ + "(" + ShipWaitSCQ + ")";
                  //  drr["PACK_WAIT_QTY"] = PackWaitSPQ + "(" + PackWaitSCQ + ")";
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
                //RQSTDT.Columns.Add("VIEW_PROC", typeof(string));
                //RQSTDT.Columns.Add("VIEW_INSP", typeof(string));
                //RQSTDT.Columns.Add("VIEW_NO_INSP", typeof(string));
                //RQSTDT.Columns.Add("VIEW_EXP", typeof(string));
                //RQSTDT.Columns.Add("VIEW_HOLD", typeof(string));
                //RQSTDT.Columns.Add("VIEW_OWMS", typeof(string));
                //RQSTDT.Columns.Add("VIEW_SHIP_WAIT", typeof(string));
                //RQSTDT.Columns.Add("VIEW_PACK_WAIT", typeof(string));
                RQSTDT.Columns.Add("VIEW_ALL", typeof(string));
                RQSTDT.Columns.Add("VIEW_CHEK", typeof(string));
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
                //dr["VIEW_PROC"] = sChkType == "CHK_PROC" ? sChkType : null;
                //dr["VIEW_INSP"] = sChkType == "CHK_INSP" ? sChkType : null;
                //dr["VIEW_NO_INSP"] = sChkType == "CHK_NO_INSP" ? sChkType : null;
                //dr["VIEW_EXP"] = sChkType == "CHK_EXP" ? sChkType : null;
                //dr["VIEW_HOLD"] = sChkType == "CHK_HOLD" ? sChkType : null;
                //dr["VIEW_OWMS"] = sChkType == "CHK_OWMS" ? sChkType : null;
                //dr["VIEW_SHIP_WAIT"] = sChkType == "CHK_SHIP_WAIT" ? sChkType : null;
                //dr["VIEW_PACK_WAIT"] = sChkType == "CHK_PACK_WAIT" ? sChkType : null;
                dr["VIEW_ALL"] = sViewAll;
                dr["VIEW_CHEK"] = sChkType;
                dr["SCH_EQSGID"] = sSchEqsgid;
                dr["SCH_MDLLOT_ID"] = sSchLotId;
                dr["SCH_PACK_WRK_TYPE_CODE"] = sSchPackType;
                dr["SCH_LOTTYPE"] = sSchLottype;
                RQSTDT.Rows.Add(dr);

                //if (sChkType == "CHK_HOLD")
                //{
                chkHoldDetail.Visibility = Visibility.Visible;

                if (chkHoldDetail.IsChecked == true)
                {
                    dgPalletInfo.Columns["MES_HOLD_NOTE"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["CELL_HOLD_NOTE"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["PALLET_HOLD_NOTE"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["MES_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["QMS_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["CELL_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                  //  dgPalletInfo.Columns["PALLET_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                    dgPalletInfo.Columns["QMS_FINL_HOLD_NOTE"].Visibility = Visibility.Visible;

                }
                //}
                else
                {
                    //chkHoldDetail.Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["MES_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["CELL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["PALLET_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["MES_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["QMS_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["CELL_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                 //   dgPalletInfo.Columns["PALLET_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                    dgPalletInfo.Columns["QMS_FINL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("BR_SET_STOCK_LIST_BX", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    SetFifo4Grid(dtResult);

                    //Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);// Util.AlertInfo(ex.Message);
                return;
            }
        }


        /// <summary>
        /// 선입선출 컬럼 추가..
        /// </summary>
        /// <param name="dtResult"></param>
        private void SetFifo4Grid(DataTable dtResult)
        {

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

                        ////출하검사 컬럼 헤더 스타일 변경
                        //if (Util.NVC(Convert.ToString(dtResult.Rows[0]["OQC_INSP_JUDG_FLAG"]), "N") == "Y")
                        //{
                        //    dgPalletInfo.Columns["OQC_INSP_YN"].HeaderStyle = styleGreen;
                        //}
                        //else
                        //{
                        //    dgPalletInfo.Columns["OQC_INSP_YN"].HeaderStyle = styleGray;
                        //}

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

        private void chkHoldDetail_Click(object sender, RoutedEventArgs e)
        {
            if (chkHoldDetail.IsChecked == true)
            {
                dgPalletInfo.Columns["MES_HOLD_NOTE"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["CELL_HOLD_NOTE"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["PALLET_HOLD_NOTE"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["MES_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["QMS_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["CELL_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
               // dgPalletInfo.Columns["PALLET_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Visible;
                dgPalletInfo.Columns["QMS_FINL_HOLD_NOTE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgPalletInfo.Columns["MES_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["QMS_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["CELL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["PALLET_HOLD_NOTE"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["MES_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["QMS_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["CELL_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
              //  dgPalletInfo.Columns["PALLET_UNHOLD_CHARGE_USERNAME"].Visibility = Visibility.Collapsed;
                dgPalletInfo.Columns["QMS_FINL_HOLD_NOTE"].Visibility = Visibility.Collapsed;
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
                    else if (e.Cell.Column.Name == "EXP_INSP_RESULT_TOTAL")
                    {
                        string pass = Util.NVC(ObjectDic.Instance.GetObjectName("미검사"), "미검사");
                        string ng = Util.NVC(ObjectDic.Instance.GetObjectName("불합격"), "불합격");
                        if (dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text == pass || dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text == ng)  // 조회된 값이 '불합격','미검사' 경우 글자색 변경 (빨강)
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
                                    presenter.Content = _TOTAL_TPQ + " (" + _TOTAL_TCQ + ")";
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

        private void SetModelLot2()
        {
            try
            {
                cboModelLot.ItemsSource = null;

                string str = string.Empty;
                for (int i = 0; i < cboEquipmentSegment.SelectedItems.Count; i++)
                {
                    if (i != cboEquipmentSegment.SelectedItems.Count - 1)
                    {
                        str += cboEquipmentSegment.SelectedItems[i] + ",";
                    }
                    else
                    {
                        str += cboEquipmentSegment.SelectedItems[i];
                    }
                }

                if (string.IsNullOrEmpty(str)) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = str;
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

        private void SetLine_CP2(string sAREAID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";
                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetComboBox(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_MDLLOT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, sAREAID };
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
