/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 기간별 Pallet 확정 이력 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.07.21  이제섭     : UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2023.07.13  안유수    E20230404-000532 UNCODE 수량관리하는 PLANT 분기 처리




 
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
    public partial class BOX001_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        //테이블 저장용
        private DataTable dsGet = new DataTable();

        private DataTable CommDT = null;

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        //string sPalletIDs = string.Empty;
        private string _emptyLot = string.Empty;

        //UNCODE 사용여부 변수
        private bool bUseUnCode = false;

        // 조회한 수량 저장하기 위한 변수
        private int isQty = 0;
        // 출하 예정일
        string shipdt = "";
        string Shipdate_Schedule = "";

        private bool bTop_ProdID_Use_Flag = false;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public BOX001_015()
        {
            InitializeComponent();
            Loaded += BOX001_015_Loaded;
        }

        private void BOX001_015_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_015_Loaded;

            initSet();

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnShipDate_Chg);
            listAuth.Add(btnPack_Outgo_Chg);
            listAuth.Add(btnGetFCSData);
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

            _combo.SetCombo(cboScanType, CommonCombo.ComboStatus.NONE, sFilter: new string[] { "CP_SCANID_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");

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

            // UNCODE 사용 Plant 조회하여 변수처리
            CommDT = UseCommoncodePlant();
            bUseUnCode = CommonVerify.HasTableRow(CommDT);

            // UNCODE 사용 Plant는 UN_CODE 컬럼 Visible 처리
            if (bUseUnCode)
            {
                dgPalletInfo.Columns["UN_CODE"].Visibility = Visibility.Visible;
            }

            // 고정식 RFID 사용 Plant는 RFID 컬럼 Visible 처리
            if (UseCommoncodePlant2())
            {
                dgPalletInfo.Columns["TAG_ID"].Visibility = Visibility.Visible;
            }
            if (CommonVerify.HasTableRow(CommDT))
            {
                if (Util.NVC(CommDT.Rows[0]["ATTRIBUTE1"]) == "Y")
                {
                    btnUnCode_Chg.Visibility = Visibility.Visible;
                }
            }

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
                BOX001_015_INFO_CHANGE popUp = new BOX001_015_INFO_CHANGE();
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
            BOX001.BOX001_015_INFO_CHANGE wndPopup = sender as BOX001.BOX001_015_INFO_CHANGE;
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
                BOX001_015_INFO_CHANGE popUp = new BOX001_015_INFO_CHANGE();
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
            BOX001.BOX001_015_INFO_CHANGE wndPopup = sender as BOX001.BOX001_015_INFO_CHANGE;
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
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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

                    if (chkValue == "0")
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
                if (_util.GetDataGridCheckValue(dgPalletInfo, "CHK", lsCount))
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

                    // string lsScanID = txtScanID.Text.Substring(0, 1);
                    // Pallet Lot
                    //if (optAuto.IsChecked == true)
                    //{
                    //    if (lsScanID == "P")
                    //    {
                    //        // ScanID에 의한 PALLET 작업이력 조회 함수 호출
                    //        SelectScanPalletInfo(txtScanID.Text.Trim());
                    //    }
                    //    else if (lsScanID == "T")
                    //    {
                    //        // Tray 정보 조회 함수 호출
                    //        string resultPalletID = SelectScanTrayInfo(txtScanID.Text.Trim());

                    //        if (resultPalletID != "")
                    //        {
                    //            // PALLET 작업이력 조회 함수 호출
                    //            SelectScanPalletInfo(resultPalletID);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Util.AlertInfo("SFU1688"); //스캔된 ID는 PalletID / TrayID 가 아닙니다.
                    //        return;
                    //    }
                    //}
                    //// 매거진 Lot
                    //else
                    //{
                    //    // ScanID에 의한 PALLET 작업이력 조회 함수 호출
                    //    SelectScanPalletInfo(txtScanID.Text.Trim());
                    //}
                }
            }
        }
        private void txtScanID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    loadingIndicator.Visibility = Visibility.Visible;
                    ClearALL();
                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");//, sCase: "EQSGID_PACK");

            //프로젝트 Combo Set.
            _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID, Util.NVC(cboEquipmentSegment.SelectedValue) }, sCase: "PROJECT_CP");
            //팔레트바코드 표시 여부
            isVisibleBCD(sAREAID);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedValue != null)
            {
                //프로젝트 Combo Set.
                _combo.SetCombo(cboProject, CommonCombo.ComboStatus.ALL, sFilter: new string[] { sAREAID, Util.NVC(cboEquipmentSegment.SelectedValue) }, sCase: "PROJECT_CP");

                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
                String[] sFilter1 = { Process.CELL_BOXING };
                _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT");
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
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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
                int iSelCnt = 0;
                int iSelRow = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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

                // 조립 LotID / 수량 저장을 위한 DataTable
                DataTable dtAssyLot = new DataTable();

                // Tray _ MagazineID / 수량 저장을 위한 DataTable
                DataTable dtBox = new DataTable();

                // Palelt ID
                string palletID = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PALLETID"].Index).Value);
                string boxstat = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["BOXSTAT"].Index).Value);
                string sPackWrkType = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["LOT_TYPE"].Index).Value);
                string sAreaid = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["AREAID"].Index).Value);

                if (boxstat == "DELETED")
                {
                    //SFU3399 발행정보가 없습니다
                    Util.MessageValidation("SFU3399");
                    return;
                }

                // 수작업 여부 확인 : MAG 컬럼이 Y면 수작업임
                if (sPackWrkType == "UI")
                {
                    // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                    dtAssyLot = SearchAssyLot(palletID);
                }
                else
                {
                    //  Tray / Magazine 정보 조회 함수 호출
                    dtBox = SelectTagInformation(palletID, sPackWrkType);
                    if (dtBox == null)
                    {
                        return;
                    }
                    else
                    {
                        // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                        dtAssyLot = SearchAssyLot(palletID);
                    }
                }

                #region 완제품 ID 존재 여부 체크
                // TOP_PRODID 조회.
                ChkUseTopProdID();

                string sTopProdID = "";
                
                if (bTop_ProdID_Use_Flag)
                {
                    sTopProdID = GetTopProdID(palletID);

                    if (sTopProdID.Equals(""))
                    {
                        // [%1]에 완제품 정보(TOP_PRODID)가 없습니다.
                        Util.MessageValidation("SFU5208", palletID);
                        return;
                    }
                }
                #endregion

                //Pallet Tag 정보Set
                DataTable dtPalletHisCard = setPalletTag(LoginInfo.USERID, palletID, iSelRow, dtBox, dtAssyLot, sTopProdID);

                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = bUseUnCode ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = getPalletTagCount(sAreaid);
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

                string sBoxStat = Util.NVC(cboPackStatus.SelectedValue);
                if (sBoxStat == "" || sBoxStat == "SELECT") sBoxStat = null;

                string sPackStat = Util.NVC(cboShipStatus.SelectedValue);
                if (sPackStat == "" || sPackStat == "SELECT") sPackStat = null;

                string sLotType = Util.NVC(cboLottype.SelectedValue);
                if (sLotType == "" || sLotType == "SELECT") sLotType = null;
                //{
                //    Util.AlertInfo("타입을 선택하세요.");
                //    return;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                // RQSTDT.Columns.Add("PALLETID", typeof(string)); -- 조회대상이 아님(스캔시 사용함)
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeFrom.Value; //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeTo.Value;  //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEqpt.SelectedValue) == "" ? null : Util.NVC(cboEqpt.SelectedValue);
                dr["AREAID"] = sAREAID;
                dr["PRODID"] = Util.NVC(cboProject.SelectedValue) == "" ? null : Util.NVC(cboProject.SelectedValue);
                // dr["PALLETID"] = txtPalletID.Text.Trim() == "" ? null : txtPalletID.Text.Trim();
                dr["BOXSTAT"] = sBoxStat;
                dr["BOX_RCV_ISS_STAT_CODE"] = sPackStat;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;

                RQSTDT.Rows.Add(dr);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_PALLET_INFO_FOR_PERIOD", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
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

                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("TAGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = sAREAID;              
                string value = string.IsNullOrWhiteSpace(lot_ID) ? txtScanID.Text : lot_ID;

                switch (Util.NVC(cboScanType.SelectedValue))
                {
                    case "PLT":
                        dr["PALLETID"] = getPalletBCD(value);  // 팔레트바코드id -> boxid
                        break;
                    case "MGZ":
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

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
                    //   dgTray.ItemsSource = DataTableConverter.Convert(dtResult);
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
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("TAGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = sAREAID;              
                string value = string.IsNullOrWhiteSpace(palletID) ? txtScanID.Text : palletID;

                switch (Util.NVC(cboScanType.SelectedValue))
                {
                    case "PLT":
                        dr["PALLETID"] = getPalletBCD(value);  // 팔레트바코드ID -> BOXID
                        break;
                    case "MGZ":
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
                new ClientProxy().ExecuteService("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
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
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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

                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "OUTER_BOXID", null, "PALLETID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "BOXID", null, "BOXID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "TAG_ID", null, "TAG_ID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "SUBLOTID", null, "CELLID", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "BOXSEQ", null, "BOXSEQ", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                    LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, "BOX_PSTN_NO", null, "CELL위치", false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);

                    for (int i = 0; i < dsResult.Tables["OUTDATA_HEADER"].Rows.Count; i++)
                    {
                        if (Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]).IndexOf("VALUE") > -1)
                        {
                            string sBinding = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["TBL_COL_NAME"]);
                            string sHeadName = Util.NVC(dsResult.Tables["OUTDATA_HEADER"].Rows[i]["MEASR_ITEM_NAME"]);

                            LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dgFCSData, sBinding, null, sHeadName, false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
                        }
                    }

                    dgFCSData.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA_VALUE"]);

                    new LGC.GMES.MES.Common.ExcelExporter().Export(dgFCSData);

                }, indataSet);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_CELL_FCS_MEARS_CP", "INDATA", "OUTDATA_HEADER,OUTDATA_VALUE", indataSet);   

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

        private DataTable setPalletTag(string sUserName, string sPalletID, int iSelRow, DataTable dtBOX, DataTable dtASSYLOT, string sTopProdID)
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
                if (bTop_ProdID_Use_Flag)
                    dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID;// Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PRODID"].Index).Value);
                else
                    dr["PRODID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PRODID"].Index).Value);
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

                //if (sShipToName == "HLGP")
                //{
                //    if (sProjectName == "" || sProjectName == "N/A")
                //    {
                //        dr["MODEL"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["MODELID"].Index).Value);          
                //    }
                //    else
                //    {
                //        dr["MODEL"] = sProjectName;                                                                                            
                //    }
                //}
                //else
                //{
                //    dr["MODEL"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["MODELID"].Index).Value);             
                //}
                dr["MODEL"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["MODELID"].Index).Value) + " (" + sProjectName + ")";
                if (bTop_ProdID_Use_Flag)
                    dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID; //Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PRODID"].Index).Value);
                else
                    dr["PRODID"] = Util.NVC(dgPalletInfo.GetCell(iSelRow, dgPalletInfo.Columns["PRODID"].Index).Value);
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "RQSTDT", "RSLTDT", RQSTDT);

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }

        }

        /// <summary>
        /// UNCODE 필수입력 Plant 조회
        /// </summary>
        /// <returns></returns>
        private DataTable UseCommoncodePlant()
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

            return dtRslt;
        }
        /// <summary>
        /// 고정식 RFID 사용 Plant 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodePlant2()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLT_RFID_SHOP";
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

        private void btnGetFCSData_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            int nSuccess = 0;
            int nFail = 0;
            for (int i = 0; i < dgCell.GetRowCount(); i++)
            {
                //if (Util.NVC(dgCell.GetCell(i, dgCell.Columns["FCS_YN"].Index).Value) == "N")
                {
                    try
                    {
                        //DataTable RQSTDT = new DataTable();
                        //RQSTDT.Columns.Add("CELLID", typeof(string));
                        //RQSTDT.Columns.Add("COND_SKIP", typeof(string));
                        //RQSTDT.Columns.Add("BCRCHECK_YN", typeof(string));

                        //DataRow dr = RQSTDT.NewRow();
                        //dr["CELLID"] = Util.NVC(dgCell.GetCell(i, dgCell.Columns["CELLID"].Index).Value);
                        //dr["COND_SKIP"] = "N";
                        //dr["BCRCHECK_YN"] = "N";
                        //RQSTDT.Rows.Add(dr);

                        //DataTable dtRslt = null;
                        //if (sAREAID.Equals("A1"))
                        //{
                        //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_v01", "INDATA", "OUTDATA", RQSTDT);
                        //}
                        //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                        //{
                        //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_v01", "INDATA", "OUTDATA", RQSTDT);
                        //}

                        DataSet indataSet = new DataSet();
                        DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                        RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                        RQSTDT.Columns.Add("SHOPID", typeof(string));
                        RQSTDT.Columns.Add("AREAID", typeof(string));
                        RQSTDT.Columns.Add("EQSGID", typeof(string));
                        RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                        RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("USERID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["SUBLOTID"] = Util.NVC(dgCell.GetCell(i, dgCell.Columns["CELLID"].Index).Value);
                        dr["SHOPID"] = string.Empty;
                        dr["AREAID"] = string.Empty;
                        dr["EQSGID"] = string.Empty;
                        dr["MDLLOT_ID"] = string.Empty;
                        dr["SUBLOT_CHK_SKIP_FLAG"] = "Y";
                        dr["INSP_SKIP_FLAG"] = "Y";
                        dr["2D_BCR_SKIP_FLAG"] = "Y";
                        dr["USERID"] = LoginInfo.USERID;
                        RQSTDT.Rows.Add(dr);

                        // ClientProxy2007
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);

                        if (dsResult.Tables[0].Rows.Count > 0)
                            nSuccess++;

                        System.Windows.Forms.Application.DoEvents();

                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        nFail++;
                    }
                }
            }

            loadingIndicator.Visibility = Visibility.Collapsed;

            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
        }

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
                //string sPalletIDs = string.Empty;
                //int iSelCnt = 0;
                //for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                //{
                //    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
                //    {
                //        iSelCnt = iSelCnt + 1;
                //        if (iSelCnt == 1)
                //        {
                //            sPalletIDs = sPalletIDs + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                //        }
                //        else
                //        {
                //            sPalletIDs = sPalletIDs + "," + Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                //        }
                //    }
                //}

                //if (iSelCnt == 0)
                //{
                //    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                //    return;
                //}

                BOX001_015_RCV_LOT popUp = new BOX001_015_RCV_LOT();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    //sFROM_DTTM = tmps[0] as string;
                    //sTO_DTTM = tmps[1] as string;
                    //sAREAID = tmps[2] as string;
                    //sEQSGID = tmps[3] as string;
                    //sProject_name = tmps[4] as string;
                    //sEQPTID = tmps[5] as string;
                    //sPACK_WRK_TYPE_CODE = tmps[6] as string;

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
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
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
                BOX001_015_INFO_CHANGE popUp = new BOX001_015_INFO_CHANGE();
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
        //[CSR ID:3897395] GMES Hold Lot/Cell Classification | [요청번호]C20190116_97395 | [서비스번호]3897395 수정자: 강호운 수정일:2019.03.14
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

        private string GetTopProdID(string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("STR_ID", typeof(string));
                RQSTDT.Columns.Add("GBN_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TYPE_CODE"] = "B";
                dr["STR_ID"] = sPalletID;
                dr["GBN_ID"] = "A";
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TOP_PRODID", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["TOP_PRODID"]).ToString();
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

        private void ChkUseTopProdID()
        {
            try
            {
                bTop_ProdID_Use_Flag = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TOP_PRODID_USE", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["TOP_PRODID_USE_FLAG"]).Equals("Y"))
                {
                    bTop_ProdID_Use_Flag = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 파레트 바코드 표시 설정
            if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                dgTray.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                dgTray.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }

        // 팔레트바코드ID -> BoxID
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void btnUnCode_Chg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BOX001_015_INFO_CHANGE popUp = new BOX001_015_INFO_CHANGE();
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
