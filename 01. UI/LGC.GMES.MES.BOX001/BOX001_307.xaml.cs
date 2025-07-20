/*************************************************************************************
 Created Date : 2021.01.06
      Creator : 이제섭
   Decription : 기간별 Pallet 확정 이력 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.06  DEVELOPER : Initial Created.D:\#.Secure Work Folder\GMES_NEW\01. UI\LGC.GMES.MES.BOX001\BOX001_300_TRAY_DETL.xaml
  2023.02.10  조영대 : 기간별 Pallet 확정 이력 정보 조회 멀티선택 출력
  2023.05.31  조영대 : Pallet Barcode ID 컬럼 추가
  2023.07.20  김동훈 : TOP_PRODID 제품ID 추가
  2023.09.25  박수미 : TAG 재발행 시 제품 ID 완제품 코드로 맵핑
  2024.01.03  최경아 : 출하처 변경 POPUP 링크 변경
  2024.01.08  박나연 : TAG 재발행 시 수동 포장인 경우에도 내포장 ID TAG에 조회되도록 수정
  2024.07.11  이현승 : 사외반품 CELL 포함여부, 사외반품여부 컬럼 추가    *2025년 적용 예정 - 수정필요시 연락부탁드립니다.
  2024.08.01  임정훈 : UNCODE변경 버튼 추가 및 팝업 호출(E20240731-000840)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_307 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _combo_f = new CommonCombo_Form();
        //테이블 저장용
        private DataTable dsGet = new DataTable();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        //string sPalletIDs = string.Empty;
        private string _emptyLot = string.Empty;

        //UNCODE 사용여부 변수
        private bool bUseUnCode = false;
        bool bPackEqptUseFlag = false; // 2023.10.29 포장기 조건 조회 동별 코드 추가
        // 조회한 수량 저장하기 위한 변수
        private int isQty = 0;
        // 출하 예정일
        string shipdt = "";
        string Shipdate_Schedule = "";

        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center

        };

        public BOX001_307()
        {
            InitializeComponent();

            Initialize();

            Loaded += BOX001_307_Loaded;
        }

        private void BOX001_307_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_307_Loaded;

            bPackEqptUseFlag = _util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "BOX001_307_PACKEQPT_USEFLAG"); // 2023.10.29 포장기 조건 조회 동별 코드 추가

            initSet();

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnShipDate_Chg);
            listAuth.Add(btnPack_Outgo_Chg);
            listAuth.Add(btnOCVdate);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgPalletInfo.Columns["OCOP_RTN_CELL_ICL_FLAG"].Visibility = Visibility.Visible;  //PALLET 정보
                dgCell.Columns["RTN_FLAG"].Visibility = Visibility.Visible;             //CELL 정보
            }

        }

        private void initSet()
        {
            // 활성화Data 저장용 DataGrid
            dgFCSData.Visibility = Visibility.Hidden;

            //정보전송조회
            dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            teTimeFrom.Value = new TimeSpan(0, 0, 0);
            teTimeTo.Value = new TimeSpan(23, 59, 59);

            // Area 셋팅
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            //포장상태 Combo Set.
            string[] sFilter3 = { "BOXSTAT" };
            _combo.SetCombo(cboPackStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            //출고상태 Combo Set.
            string[] sFilter4 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };
            _combo.SetCombo(cboShipStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");

            //타입 Combo Set.
            string[] sFilter5 = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            _combo.SetCombo(cboScanType, CommonCombo.ComboStatus.NONE, sFilter: new string[] { "BX_SCANID_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");

            //프로젝트 Combo Set.
            _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID, Util.NVC(cboEquipmentSegment.SelectedValue) }, sCase: "PROJECT_CP");

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                string sAreaid = ary.GetValue(0).ToString() + "^" + ary.GetValue(1).ToString();
                cboArea.SelectedValue = sAreaid;
                if (cboArea.SelectedIndex < 0)
                {
                    cboArea.SelectedIndex = 0;
                }

                this.txtScanID.Text = ary.GetValue(2).ToString();
                // ScanID에 의한 PALLET 작업이력 조회 함수 호출
                SelectScanPalletInfo(txtScanID.Text.Trim());
            }

            // 2023.10.29 포장기 조건 조회 동별 코드 추가
            if (bPackEqptUseFlag == false)
            {
                cboEquipment.IsEnabled = false;
            }
            
            // UNCODE 사용여부 셋팅
            bUseUnCode = UseCommoncodePlant();
            // UNCODE 사용 Plant는 UN_CODE 컬럼 Visible 처리
            if (bUseUnCode)
            {
                dgPalletInfo.Columns["UN_CODE"].Visibility = Visibility.Visible;
                btnUnCode_Chg.Visibility = Visibility.Visible;
            }

            

            dgPalletInfo.SetColumnVisibleForCommonCode("PLLT_BCD_ID", "CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
            dgTray.Columns["PLLT_BCD_ID"].Visibility = dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility;
        }

        #endregion


        #region Event

        /// <summary>
        /// 출하예정일 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShipDate_Chg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_307_INFO_CHANGE popUp = new BOX001_307_INFO_CHANGE();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "SHIPDATE";
                    Parameters[1] = sSHOPID;
                    DataTable dtPALLET = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    Parameters[2] = dtPALLET;
                    Parameters[3] = sAREAID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndShipDate_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndShipDate_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_307_INFO_CHANGE wndPopup = sender as BOX001.BOX001_307_INFO_CHANGE;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletInfo();
            }
            grdMain.Children.Remove(wndPopup);
        }

        /// <summary>
        /// 출하처 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPack_Outgo_Chg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_307_INFO_CHANGE popUp = new BOX001_307_INFO_CHANGE();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "PACKOUT_GO";
                    Parameters[1] = sSHOPID;
                    DataTable dtPALLET = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    Parameters[2] = dtPALLET;
                    Parameters[3] = sAREAID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackOutGo_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //  this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPackOutGo_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_307_INFO_CHANGE wndPopup = sender as BOX001.BOX001_307_INFO_CHANGE;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetPalletInfo();
            }

            grdMain.Children.Remove(wndPopup);
        }
        /// <summary>
        /// 조립LOT/수량확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLotInformation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sPalletIDs = string.Empty;
                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        if (iSelCnt == 1)
                        {
                            sPalletIDs = sPalletIDs + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                        else
                        {
                            sPalletIDs = sPalletIDs + "," + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                BOX001_015_ASSY_LOT popUp = new BOX001_015_ASSY_LOT();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sSHOPID;
                    Parameters[1] = sPalletIDs;
                    Parameters[2] = sAREAID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.ShowModal();
                    popUp.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 활성화 측정 데이터 저장 버튼 클릭 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFCSExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sPalletIDs = string.Empty;
                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        if (iSelCnt == 1)
                        {
                            sPalletIDs = sPalletIDs + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                        else
                        {
                            sPalletIDs = sPalletIDs + "," + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                SaveExcel_FCSData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgPalletInfo.CurrentRow == null || dgPalletInfo.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgPalletInfo.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value);

                    //초기화
                    dgTray.ItemsSource = null;
                    dgCell.ItemsSource = null;

                    if (chkValue == "0" || chkValue.ToUpper() == "FALSE")
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);

                        string sPalletid = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        string sLotType = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["LOT_TYPE"].Index).Value);

                        SelectTrayInfo(sPalletid);
                        // 매거진 여부 확인 : N 이면 설비 / N 이 아니면 수작업
                        //if (sLotType != "UI")
                        //{
                        //    // Tray 정보 조회 함수 호출
                        //  //  SelectTrayInfo(sPalletid);
                        //}
                        //else
                        //{
                        //    // 수작업 Lot(매거진 Lot)에 해당하니 Tray 정보가 아닌 셀 정보만 존재
                        //  //  SelectCellInfo(sPalletid, null);
                        //}
                    }
                    else
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);

                    SetSelectedQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgPalletInfo.CurrentRow = null;
            }
        }

        private void SetSelectedQty()
        {
            int lSumPallet = 0;
            int lSumCell = 0;

            for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
            {
                //if (_util.GetDataGridCheckValue(dgPalletInfo, "CHK", lsCount))
                if (Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[lsCount].DataItem, "CHK")).Equals("1")
                    || Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[lsCount].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    lSumPallet = lSumPallet + 1;
                    lSumCell = lSumCell + Util.NVC_Int(dgPalletInfo.GetCell(lsCount, dgPalletInfo.Columns["QTY"].Index).Value);
                }
            }

            txtPalletQty_Search.Value = lSumPallet;
            txtCellQty_Search.Value = lSumCell;
        }

        private void dgTray_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgTray.CurrentRow == null || dgTray.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                string sPalletid = Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["PALLETID"].Index).Value);
                string sTrayid = Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["TRAYID"].Index).Value);

                SelectCellInfo(sPalletid, sTrayid);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgTray.CurrentRow = null;
            }
        }


        /// <summary>
        /// 첫글자가 "P","T"가 아니면 매거진 LOT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScanID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtScanID.Text))
                {
                    // 초기화
                    ClearALL();

                    SelectScanPalletInfo();

                }
            }
        }
        private void txtScanID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.V)
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    ClearALL();
                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 1000)
                    {
                        Util.MessageValidation("SFU4243");   //최대 1000개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Search(item) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (!string.IsNullOrEmpty(_emptyLot))
                    {
                        Util.MessageValidation("SFU3588", _emptyLot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        _emptyLot = string.Empty;
                    }

                    loadingIndicator.Visibility = Visibility.Collapsed;

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
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
            _combo_f.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");//, sCase: "EQSGID_PACK");

            //프로젝트 Combo Set.
            _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID, Util.NVC(cboEquipmentSegment.SelectedValue) }, sCase: "PROJECT_CP");
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //프로젝트 Combo Set.
            _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID, Util.NVC(cboEquipmentSegment.SelectedValue) }, sCase: "PROJECT_CP");
            // 2023.10.29 포장기 조건 조회 동별 코드 추가
            if (bPackEqptUseFlag)
            {
                if (string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue)) == false)
                {

                }
                string[] sFilter6 = { Util.NVC(cboEquipmentSegment.SelectedValue), "B1000" };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter6, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            }
        }

        private void btnBoxLabelPrt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iSelCnt = 0;
                int iSelRow = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        iSelRow = i;
                    }
                }

                if (iSelCnt != 1)
                {
                    Util.AlertInfo("SFU2004"); //하나의 Pallet만 선택해주십시오.
                    return;
                }

                // 박스총수량 저장을 위한 DataTable
                DataTable resultBoxLotByQty = new DataTable();

                // Palelt ID
                string palletID = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PALLETID"].Index).Value);
                string sPackWrkType = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["LOT_TYPE"].Index).Value);

                // 수작업 여부 확인 : MAG 컬럼이 Y면 수작업임
                if (sPackWrkType == "UI")
                {
                    // 박스번호,랏+수량 조회 함수 호출 : 결과값을 DataTable 로 리턴
                    resultBoxLotByQty = SearchBoxLotByQty(palletID);

                    if (resultBoxLotByQty.Rows.Count > 0)
                    {
                        int t = resultBoxLotByQty.Rows.Count;
                        int Value_Cnt = t / 21;
                        int Remainder_Cnt = t % 21;
                        int Sheet_Cnt = Value_Cnt + (Remainder_Cnt == 0 ? 0 : 1);

                        // Tag 발행용 DataTable 생성
                        DataTable dtBoxTag = new DataTable();
                        dtBoxTag = CreateDT_BoxTag(dtBoxTag, Sheet_Cnt);
                        int iSeqNo = 0;
                        for (int lsCount = 0; lsCount <= resultBoxLotByQty.Rows.Count - 1; lsCount++)
                        {
                            int Index = lsCount / 21;

                            iSeqNo = iSeqNo + 1;
                            if (iSeqNo > 21)
                            {
                                iSeqNo = 1;
                            }

                            string sBoxid_Col = "BOXID_PRT" + iSeqNo.ToString();
                            string sLotQty_Col = "LOT_QTY" + iSeqNo.ToString();

                            dtBoxTag.Rows[Index]["PALLETID"] = palletID;    // 동일값이라서.. 그냥 계속 넘어줘도 됨.
                            dtBoxTag.Rows[Index][sBoxid_Col] = resultBoxLotByQty.Rows[lsCount]["BOXID_PRT"].ToString();
                            dtBoxTag.Rows[Index][sLotQty_Col] = resultBoxLotByQty.Rows[lsCount]["LOT_QTY"].ToString();
                        }

                        LGC.GMES.MES.BOX001.Report_Multi_Cell rs = new LGC.GMES.MES.BOX001.Report_Multi_Cell();
                        rs.FrameOperation = this.FrameOperation;

                        if (rs != null)
                        {
                            // 태그 발행 창 화면에 띄움.
                            object[] Parameters = new object[2];
                            Parameters[0] = "Pallet_BoxTag";
                            Parameters[1] = dtBoxTag;

                            C1WindowExtension.SetParameters(rs, Parameters);

                            rs.Closed += new EventHandler(wndQAMailSend_Closed);
                            // 팝업 화면 숨겨지는 문제 수정.
                            //   this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                            grdMain.Children.Add(rs);
                            rs.BringToFront();
                        }

                    }

                }
                else
                {
                    Util.AlertInfo("SFU1448"); //개별 Pallet구성만 Box Lable 출력 가능합니다.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void wndQAMailSend_Closed(object sender, EventArgs e)
        {
            Report_Multi_Cell window = sender as Report_Multi_Cell;

            grdMain.Children.Remove(window);
        }

        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();
                if (!dgPalletInfo.IsCheckedRow("CHK"))
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataTable dtPalletHisCard = null;

                foreach (int iSelRow in dgPalletInfo.GetCheckedRowIndex("CHK"))
                {
                    // 조립 LotID / 수량 저장을 위한 DataTable
                    DataTable dtAssyLot = new DataTable();

                    // Tray _ MagazineID / 수량 저장을 위한 DataTable
                    DataTable dtBox = new DataTable();

                    // Palelt ID
                    string palletID = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PALLETID"].Index).Value);
                    string boxstat = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["BOXSTAT"].Index).Value);
                    string sPackWrkType = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["LOT_TYPE"].Index).Value);

                    if (boxstat == "DELETED")
                    {
                        //SFU3399 발행정보가 없습니다.
                        dgPalletInfo.SetRowValidation(iSelRow, MessageDic.Instance.GetMessage("SFU3399"));
                        continue;
                    }

                    //  Tray / Magazine 정보 조회 함수 호출
                    dtBox = SelectTagInformation(palletID, sPackWrkType);
                    if (dtBox == null)
                    {
                        dgPalletInfo.SetRowValidation(iSelRow, MessageDic.Instance.GetMessage("SFU3399"));
                        continue;
                    }
                    else
                    {
                        // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                        dtAssyLot = SearchAssyLot(palletID);
                    }

                    //Pallet Tag 정보Set
                    DataTable dtResult = setPalletTag(LoginInfo.USERID, palletID, iSelRow, dtBox, dtAssyLot);

                    if (dtPalletHisCard == null)
                    {
                        dtPalletHisCard = dtResult;
                    }
                    else
                    {
                        dtPalletHisCard.Merge(dtResult, true, MissingSchemaAction.Ignore);
                    }
                }

                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = bUseUnCode ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = getPalletTagCount(sAREAID);
                Parameters[3] = "Y";
                Parameters[4] = sSHOPID;

                LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                C1WindowExtension.SetParameters(rs, Parameters);
                rs.Closed += new EventHandler(Tag_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                //  this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                grdMain.Children.Add(rs);
                rs.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Tag_Closed(object sender, EventArgs e)
        {
            Report_Pallet_Hist window = sender as Report_Pallet_Hist;
            grdMain.Children.Remove(window);
        }

        #endregion


        #region Mehod


        private void ClearALL()
        {
            this.ClearValidation();
            dgPalletInfo.ItemsSource = null;
            dgTray.ItemsSource = null;
            dgCell.ItemsSource = null;
            txtPalletQty.Value = 0;
            txtCellQty.Value = 0;
            txtPalletQty_Search.Value = 0;
            txtCellQty_Search.Value = 0;
        }

        /// <summary>
        /// Pallet 정보 조회
        /// </summary>
        private void GetPalletInfo()
        {
            try
            {                
                //초기화
                ClearALL();

                loadingIndicator.Visibility = Visibility.Visible;

                string sBoxStat = Util.NVC(cboPackStatus.SelectedValue);
                if (sBoxStat == "" || sBoxStat == "SELECT") sBoxStat = null;

                string sPackStat = Util.NVC(cboShipStatus.SelectedValue);
                if (sPackStat == "" || sPackStat == "SELECT") sPackStat = null;

                string sLotType = Util.NVC(cboLottype.SelectedValue);
                if (sLotType == "" || sLotType == "SELECT") sLotType = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeFrom.Value;
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeTo.Value;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["EQPTID"] = null;
                dr["AREAID"] = sAREAID;
                dr["PRODID"] = Util.NVC(cboProject.SelectedValue) == "" ? null : Util.NVC(cboProject.SelectedValue);
                dr["BOXSTAT"] = sBoxStat;
                dr["BOX_RCV_ISS_STAT_CODE"] = sPackStat;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;

                // 2023.10.29 포장기 조건 조회 동별 코드 추가
                if (bPackEqptUseFlag)
                {
                    dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue) == "" ? null : Util.NVC(cboEquipment.SelectedValue);
                }

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_PALLET_INFO_PERIOD_BX", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);

                    int lSumPallet = 0;
                    int lSumCell = 0;

                    for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
                    {
                        lSumPallet = lSumPallet + 1;

                        lSumCell = lSumCell + Util.NVC_Int(dgPalletInfo.GetCell(lsCount, dgPalletInfo.Columns["QTY"].Index).Value);
                    }
                    txtPalletQty.Value = lSumPallet;
                    txtCellQty.Value = lSumCell;

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        bool Multi_Search(string lot_ID)
        {
            DataSet ds = new DataSet();

            try
            {
                DoEvents();
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("TAGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;           
                string value = string.IsNullOrWhiteSpace(lot_ID) ? txtScanID.Text : lot_ID;

                switch (Util.NVC(cboScanType.SelectedValue))
                {
                    case "PLT":
                        dr["PALLETID"] = ConvertBarcodeId(value);
                        break;
                    case "TAG":
                        dr["TAGID"] = value;
                        break;
                    case "BOX":
                        dr["BOXID"] = value;
                        break;
                    case "CELL":
                        dr["SUBLOTID"] = value;
                        break;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_PALLET_INFO_BY_ID_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dgPalletInfo.GetRowCount() == 0)
                {
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgPalletInfo, dtInfo, FrameOperation);
                }

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + 1;

                    lSumCell = lSumCell + Util.NVC_Int(dgPalletInfo.GetCell(lsCount, dgPalletInfo.Columns["QTY"].Index).Value);
                }

                txtPalletQty.Value = lSumPallet;
                txtCellQty.Value = lSumCell;

                if (dtResult.Rows.Count == 0)
                {
                    if (string.IsNullOrEmpty(_emptyLot))
                        _emptyLot += lot_ID;
                    else
                        _emptyLot = _emptyLot + ", " + lot_ID;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        /// <summary>
        /// PalletID로 Tray 정보 조회하는 함수
        /// </summary>
        /// <param name="palletID">Pallet 스프레드에서 선택한 셀의 LotID</param>
        private void SelectTrayInfo(string palletID)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgTray, dtResult, FrameOperation, true);
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
        /// Cell 정보 조회 함수
        /// </summary>
        /// <param name="boxID">Tray 스프레에서 선택한 셀의 TrayID</param>
        private void SelectCellInfo(string sPalletid, string sTrayid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("TRAYID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletid;
                dr["TRAYID"] = sTrayid == "" ? null : sTrayid;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_CELL_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //   dgCell.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgCell, dtResult, FrameOperation, true);
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
        /// TrayID로 Tray 정보 조회하는 함수
        /// </summary>
        /// <param name="trayID"></param>
        /// <returns></returns>
        private string SelectScanTrayInfo(string trayID)
        {
            DataSet ds = new DataSet();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("TRAYID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TRAYID"] = trayID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYTRAYID_CP", "RQSTDT", "RSLTDT", RQSTDT);
                dgTray.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["PALLETID"]);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        /// <summary>
        /// 기간별 작업이력 조회 화면에서 ScanID에 의한 PALLET/매거진 작업이력 조회 함수
        /// </summary>
        /// <param name="palletID"></param>
        private void SelectScanPalletInfo(string palletID = "")
        {
            DataSet ds = new DataSet();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("TAGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;    
                string value = string.IsNullOrWhiteSpace(palletID) ? txtScanID.Text : palletID;

                switch (Util.NVC(cboScanType.SelectedValue))
                {
                    case "PLT":
                        List<string> splitList = value.Split(';').ToList();
                        for (int inx = 0; inx < splitList.Count; inx++)
                        {
                            splitList[inx] = ConvertBarcodeId(splitList[inx]);
                        }
                        dr["PALLETID"] = string.Join(";", splitList);
                        break;
                    case "TAG":
                        dr["TAGID"] = value;
                        break;
                    case "BOX":
                        dr["BOXID"] = value;
                        break;
                    case "CELL":
                        dr["SUBLOTID"] = value;
                        break;
                }
                RQSTDT.Rows.Add(dr);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_SEL_PALLET_INFO_BY_ID_BX", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    // dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);

                    int lSumPallet = 0;
                    int lSumCell = 0;

                    for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
                    {
                        lSumPallet = lSumPallet + 1;

                        lSumCell = lSumCell + Util.NVC_Int(dgPalletInfo.GetCell(lsCount, dgPalletInfo.Columns["QTY"].Index).Value);
                    }

                    txtPalletQty.Value = lSumPallet;
                    txtCellQty.Value = lSumCell;
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void SaveExcel_FCSData() 
        {

            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("OUTER_BOXID", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));

                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        DataRow inData = inDataTable.NewRow();
                        inData["OUTER_BOXID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);   // "P6A0900083";
                        inData["LANGID"] = LoginInfo.LANGID;
                        inDataTable.Rows.Add(inData);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_CELL_FCS_MEARS_CP", "INDATA", "OUTDATA_HEADER,OUTDATA_VALUE", (dsResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    dgFCSData.Columns.Clear();

                    Util.SetGridColumnText(dgFCSData, "OUTER_BOXID", null, "PALLETID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "BOXID", null, "BOXID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "TAG_ID", null, "TAG_ID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "SUBLOTID", null, "CELLID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "BOXSEQ", null, "BOXSEQ", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    Util.SetGridColumnText(dgFCSData, "BOX_PSTN_NO", null, "CELL위치", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);

                    for (int i = 0; i < dsResult.Tables["OUTDATA_HEADER"].Rows.Count; i++)
                    {
                        //if (Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]).IndexOf("VALUE") > -1)
                        //{
                            string sBinding = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]);
                            string sHeadName = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["MEASR_ITEM_NAME"]);

                            Util.SetGridColumnText(dgFCSData, sBinding, null, sHeadName, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                        //}
                    }

                    dgFCSData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA_VALUE"]);

                    new ExcelExporter().Export(dgFCSData);

                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }


        private DataTable SearchBoxLotByQty(string sPalletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXCELL_QTY_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }



        /// <summary>
        /// Tag 발행용 DataTable 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable CreateDT_BoxTag(DataTable dt, int Sheet_Cnt)
        {
            try
            {
                dt.Columns.Add("PALLETID", typeof(string));

                dt.Columns.Add("BOXID_PRT1", typeof(string));
                dt.Columns.Add("LOT_QTY1", typeof(string));
                dt.Columns.Add("BOXID_PRT2", typeof(string));
                dt.Columns.Add("LOT_QTY2", typeof(string));
                dt.Columns.Add("BOXID_PRT3", typeof(string));
                dt.Columns.Add("LOT_QTY3", typeof(string));
                dt.Columns.Add("BOXID_PRT4", typeof(string));
                dt.Columns.Add("LOT_QTY4", typeof(string));
                dt.Columns.Add("BOXID_PRT5", typeof(string));
                dt.Columns.Add("LOT_QTY5", typeof(string));
                dt.Columns.Add("BOXID_PRT6", typeof(string));
                dt.Columns.Add("LOT_QTY6", typeof(string));
                dt.Columns.Add("BOXID_PRT7", typeof(string));
                dt.Columns.Add("LOT_QTY7", typeof(string));
                dt.Columns.Add("BOXID_PRT8", typeof(string));
                dt.Columns.Add("LOT_QTY8", typeof(string));
                dt.Columns.Add("BOXID_PRT9", typeof(string));
                dt.Columns.Add("LOT_QTY9", typeof(string));
                dt.Columns.Add("BOXID_PRT10", typeof(string));
                dt.Columns.Add("LOT_QTY10", typeof(string));
                dt.Columns.Add("BOXID_PRT11", typeof(string));
                dt.Columns.Add("LOT_QTY11", typeof(string));
                dt.Columns.Add("BOXID_PRT12", typeof(string));
                dt.Columns.Add("LOT_QTY12", typeof(string));
                dt.Columns.Add("BOXID_PRT13", typeof(string));
                dt.Columns.Add("LOT_QTY13", typeof(string));
                dt.Columns.Add("BOXID_PRT14", typeof(string));
                dt.Columns.Add("LOT_QTY14", typeof(string));
                dt.Columns.Add("BOXID_PRT15", typeof(string));
                dt.Columns.Add("LOT_QTY15", typeof(string));
                dt.Columns.Add("BOXID_PRT16", typeof(string));
                dt.Columns.Add("LOT_QTY16", typeof(string));
                dt.Columns.Add("BOXID_PRT17", typeof(string));
                dt.Columns.Add("LOT_QTY17", typeof(string));
                dt.Columns.Add("BOXID_PRT18", typeof(string));
                dt.Columns.Add("LOT_QTY18", typeof(string));
                dt.Columns.Add("BOXID_PRT19", typeof(string));
                dt.Columns.Add("LOT_QTY19", typeof(string));
                dt.Columns.Add("BOXID_PRT20", typeof(string));
                dt.Columns.Add("LOT_QTY20", typeof(string));
                dt.Columns.Add("BOXID_PRT21", typeof(string));
                dt.Columns.Add("LOT_QTY21", typeof(string));

                for (int i = 0; i < Sheet_Cnt; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["PALLETID"] = string.Empty;

                    dr["BOXID_PRT1"] = string.Empty;
                    dr["LOT_QTY1"] = string.Empty;
                    dr["BOXID_PRT2"] = string.Empty;
                    dr["LOT_QTY2"] = string.Empty;
                    dr["BOXID_PRT3"] = string.Empty;
                    dr["LOT_QTY3"] = string.Empty;
                    dr["BOXID_PRT4"] = string.Empty;
                    dr["LOT_QTY4"] = string.Empty;
                    dr["BOXID_PRT5"] = string.Empty;
                    dr["LOT_QTY5"] = string.Empty;
                    dr["BOXID_PRT6"] = string.Empty;
                    dr["LOT_QTY6"] = string.Empty;
                    dr["BOXID_PRT7"] = string.Empty;
                    dr["LOT_QTY7"] = string.Empty;
                    dr["BOXID_PRT8"] = string.Empty;
                    dr["LOT_QTY8"] = string.Empty;
                    dr["BOXID_PRT9"] = string.Empty;
                    dr["LOT_QTY9"] = string.Empty;
                    dr["BOXID_PRT10"] = string.Empty;
                    dr["LOT_QTY10"] = string.Empty;
                    dr["BOXID_PRT11"] = string.Empty;
                    dr["LOT_QTY11"] = string.Empty;
                    dr["BOXID_PRT12"] = string.Empty;
                    dr["LOT_QTY12"] = string.Empty;
                    dr["BOXID_PRT13"] = string.Empty;
                    dr["LOT_QTY13"] = string.Empty;
                    dr["BOXID_PRT14"] = string.Empty;
                    dr["LOT_QTY14"] = string.Empty;
                    dr["BOXID_PRT15"] = string.Empty;
                    dr["LOT_QTY15"] = string.Empty;
                    dr["BOXID_PRT16"] = string.Empty;
                    dr["LOT_QTY16"] = string.Empty;
                    dr["BOXID_PRT17"] = string.Empty;
                    dr["LOT_QTY17"] = string.Empty;
                    dr["BOXID_PRT18"] = string.Empty;
                    dr["LOT_QTY18"] = string.Empty;
                    dr["BOXID_PRT19"] = string.Empty;
                    dr["LOT_QTY19"] = string.Empty;
                    dr["BOXID_PRT20"] = string.Empty;
                    dr["LOT_QTY20"] = string.Empty;
                    dr["BOXID_PRT21"] = string.Empty;
                    dr["LOT_QTY21"] = string.Empty;

                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private string getPalletTagCount(string areaid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "BOX_TAG_COUNT";
            dr["CMCODE"] = areaid;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_TAG_COUNT", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                return Convert.ToString(dtResult.Rows[0]["PRINT_COUNT"]);
            }
            else
            {
                return "2";
            }


        }

        private DataTable setPalletTag(string sUserName, string sPalletID, int iSelRow, DataTable dtBOX, DataTable dtASSYLOT)
        {
            string sProjectName = string.Empty;
            //고객 모델 조회
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drMomel = RQSTDT.NewRow();
                drMomel["PRODID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PRODID"].Index).Value);
                RQSTDT.Rows.Add(drMomel);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR_FOR_PROJECTNAME", "RQSTDT", "RSLTDT", RQSTDT);
                sProjectName = Util.NVC(dtResult.Rows[0]["PROJECTNAME"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //return null;
            }


            //Lot 편차 구하기... 2014.02.20 Add By Airman
            string sLotTerm = GetLotTerm2PalletID(sPalletID);

            DataTable dtPalletHisCard = new DataTable();

            dtPalletHisCard.Columns.Add("PALLETID", typeof(string));    //4,3   PALLETID_01
            dtPalletHisCard.Columns.Add("BARCODE1", typeof(string));    //4,9   PALLETID_02
            dtPalletHisCard.Columns.Add("CONBINESEQ1", typeof(string));  //4,17  PALLETD_03

            dtPalletHisCard.Columns.Add("SHIP_LOC", typeof(string));    //5,7   출하처
            dtPalletHisCard.Columns.Add("SHIPDATE", typeof(string));    //5,14  출하예정일
            dtPalletHisCard.Columns.Add("OUTGO", typeof(string));       //6,7   출하지
            dtPalletHisCard.Columns.Add("LOTTERM", typeof(string));     //6,16  LOT편차
            dtPalletHisCard.Columns.Add("NOTE", typeof(string));        //7,7   특이사항
            dtPalletHisCard.Columns.Add("UNCODE", typeof(string));      //UNCODE

            dtPalletHisCard.Columns.Add("PACKDATE", typeof(string));    //8,7   포장작업일자
            dtPalletHisCard.Columns.Add("LINE", typeof(string));        //8,15  생산호기
            dtPalletHisCard.Columns.Add("MODEL", typeof(string));       //9,7   모델
            dtPalletHisCard.Columns.Add("PRODID", typeof(string));      //9,15  제품id
            dtPalletHisCard.Columns.Add("SHIPQTY", typeof(string));     //10,7   출하수량
            dtPalletHisCard.Columns.Add("PARTNO", typeof(string));      //10,15  PART NO
            dtPalletHisCard.Columns.Add("OUTQTY", typeof(string));      //11,7   제품수량
            dtPalletHisCard.Columns.Add("USERID", typeof(string));      //11,15  작업자
            dtPalletHisCard.Columns.Add("CONBINESEQ2", typeof(string)); //12,7   구성차수관리No
            dtPalletHisCard.Columns.Add("SKIPYN", typeof(string));      //12,15  검사조건Skip여부

            dtPalletHisCard.Columns.Add("SHIP_LOC_EN", typeof(string));
            dtPalletHisCard.Columns.Add("LINE_EN", typeof(string));

            //dtTRAY
            for (int i = 0; i < 40; i++)
            {
                dtPalletHisCard.Columns.Add("TRAY_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("T_" + i.ToString(), typeof(string));
            }

            // lot
            for (int i = 0; i < 20; i++)
            {
                dtPalletHisCard.Columns.Add("LOTID_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("L_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("BCD" + i.ToString(), typeof(string));
            }

            string sShipToID = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPTO_ID"].Index).Value);
            string sShipToName = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPTO_NAME"].Index).Value);

            DataRow dr = dtPalletHisCard.NewRow();
            dr["PALLETID"] = sPalletID;
            dr["BARCODE1"] = sPalletID;
            dr["LOTTERM"] = sLotTerm;

            if (sShipToID == string.Empty)
            {
                dr["SHIP_LOC"] = "";
                dr["SHIPDATE"] = "";
                dr["OUTGO"] = "";
                dr["NOTE"] = "";
                dr["PACKDATE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["WIPDTTM_ED"].Index).Text);
                dr["LINE"] = "";
                dr["MODEL"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["MODELID"].Index).Value);
                dr["PRODID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["TOP_PRODID"].Index).Value);
                dr["SHIPQTY"] = "";
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["REG_USERNAME"].Index).Value);
                dr["CONBINESEQ2"] = "";
                dr["CONBINESEQ1"] = "";
                dr["SKIPYN"] = "";
                dr["SHIP_LOC_EN"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPTO_NAME_EN"].Index).Value);
                dr["LINE_EN"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["EQSGNAME_EN"].Index).Value);
                dr["UNCODE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["UN_CODE"].Index).Value);
            }
            else
            {
                dr["SHIP_LOC"] = sShipToName;
                dr["SHIPDATE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPDATE_SCHEDULE"].Index).Value);
                dr["OUTGO"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPTO_NOTE"].Index).Value);
                dr["NOTE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PACK_NOTE"].Index).Value);
                dr["PACKDATE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["WIPDTTM_ED"].Index).Text);
                dr["LINE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["EQSGNAME"].Index).Value);
                dr["MODEL"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["MODELID"].Index).Value) + " (" + sProjectName + ")";
                dr["PRODID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["TOP_PRODID"].Index).Value);
                dr["SHIPQTY"] = string.Format("{0:#,###}", Util.NVC_Int(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPQTY"].Index).Value));
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC_Int(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["REG_USERNAME"].Index).Value);
                dr["CONBINESEQ2"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["COMBINESEQ"].Index).Value);
                dr["CONBINESEQ1"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["COMBINESEQ"].Index).Value);
                dr["SKIPYN"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["INSP_SKIP_FLAG"].Index).Value) == "Y" ? "SKIP" : "NO SKIP";
                dr["SHIP_LOC_EN"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["SHIPTO_NAME_EN"].Index).Value);
                dr["LINE_EN"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["EQSGNAME_EN"].Index).Value);
                dr["UNCODE"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["UN_CODE"].Index).Value);
            }

            for (int i = 0; i < dtBOX.Rows.Count && i < 40; i++)
            {
                dr["TRAY_" + i.ToString()] = Util.NVC(dtBOX.Rows[i]["TRAY_MAGAZINE"]);
                dr["T_" + i.ToString()] = Util.NVC_Int(dtBOX.Rows[i]["QTY"]);
            }

            dtPalletHisCard.Rows.Add(dr);

            for (int cnt = 0; cnt < (dtASSYLOT.Rows.Count + 1) / 20; cnt++)
            {
                DataTable dtNew = dtPalletHisCard.Copy();
                dtPalletHisCard.Merge(dtNew);
            }

            for (int i = 0; i < dtASSYLOT.Rows.Count; i++)
            {
                int cnt = i / 20;
                dtPalletHisCard.Rows[cnt]["LOTID_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtASSYLOT.Rows[i]["LOTID"]);
                dtPalletHisCard.Rows[cnt]["L_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC_Int(dtASSYLOT.Rows[i]["CELLQTY"]).ToString();
                dtPalletHisCard.Rows[cnt]["BCD" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtASSYLOT.Rows[i]["LOTID"]) + " " + Util.NVC_Int(dtASSYLOT.Rows[i]["CELLQTY"]).ToString();
            }

            //  dtPalletHisCard.Rows.Add(dr);
            return dtPalletHisCard;
        }

        /// <summary>
        /// LOT 편차 구하기
        /// </summary>
        /// <param name="sPalletID"></param>
        /// <returns></returns>
        private string GetLotTerm2PalletID(string sPalletID)
        {
            // DO_CONFIRM_CHECK
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["OUTER_BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_OUTER_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC_Int(dtResult.Rows[0]["LOTTERM"]).ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "0";
            }
        }


        /// <summary>
        /// BOXID, 해당 BOX 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SelectTagInformation(string palletID, string sPackWrkType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
                lsDataTable.Columns.Add("QTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();

                    if (sPackWrkType == "MGZ")
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TAG_ID"].ToString());
                    }
                    else
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TRAYID"].ToString());
                    }

                    row["QTY"] = Util.NVC(dtResult.Rows[i]["QTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }
        }

        /// <summary>
        /// 조립LotID, 해당 Lot 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SearchAssyLot(string palletID)
        {

            //BizData data = new BizData("QR_GETASSYLOT_PALLETID", "RSLTDT");
            try
            {
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletID;
                dr["SHOPID"] = sSHOPID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_GET_ASSY_LOT_BY_PALLET_BX", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("LOTID", typeof(string));
                lsDataTable.Columns.Add("CELLQTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();
                    row["LOTID"] = Util.NVC(dtResult.Rows[i]["LOTID"].ToString());
                    row["CELLQTY"] = Util.NVC(dtResult.Rows[i]["CELLQTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }
        #endregion

        private void dgPalletInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
 
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", true);
                }
            }

            SetSelectedQty();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                }
            }

            SetSelectedQty();
        }

        private void btnRcvLotInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                BOX001_015_RCV_LOT popUp = new BOX001_015_RCV_LOT();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    Parameters[1] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59"); // sPalletIDs;
                    Parameters[2] = sAREAID;
                    Parameters[3] = cboEquipmentSegment.SelectedValue;
                    Parameters[4] = cboProject.SelectedValue;
                    Parameters[5] = "";
                    Parameters[6] = cboLottype.SelectedValue;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.ShowModal();
                    popUp.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOCVdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sPalletIDs = string.Empty;
                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        if (iSelCnt == 1)
                        {
                            sPalletIDs = sPalletIDs + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                        else
                        {
                            sPalletIDs = sPalletIDs + "," + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        }
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                BOX001_015_OCV_DTTM popUp = new BOX001_015_OCV_DTTM();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = sPalletIDs;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.ShowModal();
                    popUp.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPack_Shipping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_307_INFO_CHANGE popUp = new BOX001_307_INFO_CHANGE();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "SHIPPING_PACK";
                    Parameters[1] = sSHOPID;
                    DataTable dtPALLET = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    Parameters[2] = dtPALLET;
                    Parameters[3] = sAREAID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndPackOutGo_Closed);
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;
            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) != "CHK")
                    {
                        string value = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_FLAG"));
                        if (!string.IsNullOrEmpty(value))
                        {
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFD0DA");
                            if (value.Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;
                            }
                        }
                    }
                }
            }));
        }

        private void dgPalletInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void btnUploadFile_Click(object sender, RoutedEventArgs e)
        {
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

                    string palletID = string.Empty;
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        XLCell cell = sheet.GetCell(rowInx, 0);
                        if (cell != null)
                        {
                            palletID += cell.Text;
                            if(rowInx != sheet.Rows.Count - 1)
                            {
                                //;를 구분자로 사용
                                palletID += ";";
                            }
                        }

                    }

                    ClearALL();

                    SelectScanPalletInfo(palletID);
                }
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

        // 활성화 사외 반품 처리 여부 사용 Area 조회
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

        // 바코드 ID ==> Pallet ID 입력 변환
        private string ConvertBarcodeId(string lotId)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["CSTID"] = lotId;
            dtRqst.Rows.Add(drRqst);

            DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", dtRqst);
            if (dtPallet != null && dtPallet.Rows.Count > 0)
            {
                return Util.NVC(dtPallet.Rows[0]["CURR_LOTID"]);
            }
            return lotId;
        }
        private void btnUnCode_Chg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_307_INFO_CHANGE popUp = new BOX001_307_INFO_CHANGE();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "UNCODE";
                    Parameters[1] = sSHOPID;
                    DataTable dtPALLET = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    Parameters[2] = dtPALLET;
                    Parameters[3] = sAREAID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndShipDate_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //   this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
