/*************************************************************************************
 Created Date : 2020.12.24
      Creator : 
   Decription : 포장 Pallet 구성 (자동)
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.24  DEVELOPER : Initial Created.
  2023.07.24  김동훈    : TOP_PRODID, TOP_PRODNAME 완제품 추가
  2023.10.20  박수미    : 실적확정 시 LOTID 단위수량 업데이트
  2023.11.28  홍석원    : Tag 발행 시 완제품ID로 발행되도록 수정
  2024.04.18  박나연    : PALLETID 미선택 후 실적확정 버튼 클릭 시 오류 메세지 수정
  2024.06.21  Fauzul A. : Adding new authentication user on AuthCheck
  2025.03.31  복현수    : 조회버튼 여러번 누를경우 CHK 타입변환 에러 발생하여 수정
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
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_300 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private Util _Util = new Util();
        CommonCombo_Form _combo = new CommonCombo_Form();
        //CommonCombo_Form _combo_f = new CommonCombo_Form();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        string sEQSGID = string.Empty;
        string sEQSGNAME = string.Empty;
        string sEQPTID = string.Empty;

        string selPalletID = string.Empty;
        Object selLotData = null;
        DataTable dtPRODLOT = null;
        DataTable dtTRAY = null;
        DataTable dtASSYLOT = null;
        DataRow drCurrRow = null;
        // 모니터링용 타이머
        private System.Windows.Threading.DispatcherTimer _Timer = new System.Windows.Threading.DispatcherTimer();
        private string _sPalletID = string.Empty;
        private int iInterval = 50;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            Content = "ALL",
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public BOX001_300()
        {
            InitializeComponent();
            Loaded += BOX001_300_Loaded;
        }

        private void BOX001_300_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_300_Loaded;

            // 기본은 체크 해제
            //chkTrayRefresh.Checked = false;
            //chkDummy.Visible = false;

            setInit();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (GetPilotAdminArea() > 0) // 공통코드로 시생산 제품 관리하는 동에 따라 버튼 Visible
            {
                btnExtra.Visibility = Visibility.Visible;
            }
            else
            {
                btnExtra.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize


        private void setInit()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }
            //라인,설비 셋팅
            // String[] sFilter = { LoginInfo.CFG_AREA_ID };    // Area
            _combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.SELECT, sCase: "AREA_CP");
            chkReload.IsChecked = false;
            TimerSetting();

            this.RegisterName("redBrush", redBrush);
        }

        #endregion

        #region Event
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;

            string sMode = string.Empty;

            // 시생산
            if (btn == btnPilotProdMode)
            {
                sMode = "PILOT";
            }
            // 시생산 샘플
            else if (btn == btnPilotSProdMode)
            {
                sMode = "PILOT_S";
            }
            if (!CanPilotProdMode()) return;

            string sMsg = string.Empty;
            bool bMode = GetPilotProdMode();

            if (bMode == false)
            {
                if (sMode == "PILOT")
                {
                    // 시생산 설정하시겠습니까?
                    sMsg = "SFU8515";
                }
                else if (sMode == "PILOT_S")
                {
                    // 시생산샘플 설정하시겠습니까?
                    sMsg = "SFU8517";
                }
            }
            else
            {
                if (sMode == "PILOT")
                {
                    // 시생산 해지하시겠습니까?
                    sMsg = "SFU8516";
                }
                else if (sMode == "PILOT_S")
                {
                    // 시생산샘플 해지하시겠습니까?
                    sMsg = "SFU8518";
                }
            }

            // 변경하시겠습니까?
            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetPilotProdMode(sMode, bMode);
                    GetPilotProdMode();

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _Timer.Stop();
            string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sLine == string.Empty || sLine == "SELECT")
            {
                Util.MessageValidation("MMD0047"); //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                return;
            }

            //초기화
            selPalletID = string.Empty;
            selLotData = null;
            dtPRODLOT = null;
            dtTRAY = null;
            dtASSYLOT = null;
            // 현재 선택된 라디오버튼의 Pallet Id ( Lot ID ) 가져오기
            ChekSetPalletID();

            AllClear();
            sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);
            sEQSGNAME = Util.NVC(cboEquipmentSegment.Text);

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = sEQSGID;
            dr["PROCID"] = "B1000"; //Process.CELL_BOXING; -- 2024.05.14 이제섭 ESMI 1동 Packer가 EOL ULD로 대체 되어 공정 FF5101 조회될 수 있도록 추가
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_SEL_PALLET_LIST_EQ_BX", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgProductLot, searchResult, FrameOperation);
                    DataTable DtTemp = null;
                    DtTemp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                    dtPRODLOT = DtTemp.Copy();

                    if (chkReload.IsChecked == true)  // 작업대상이 선택안되었다면 Pass
                    {
                        _Timer.Start();
                        if (_sPalletID != string.Empty)
                        {
                            int nCount = 0;
                            int idx = -1;
                            nCount = dgProductLot.GetRowCount();
                            for (int i = 0; i < nCount; i++)
                            {
                                if (Util.NVC(dgProductLot.GetCell(i, dgProductLot.Columns["LOTID"].Index).Value) == _sPalletID)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", 1);
                                    DtTemp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                                    Util.GridSetData(dgProductLot, DtTemp, FrameOperation);
                                    idx = i;
                                    i = nCount;
                                }
                            }
                            if (idx == -1)
                            {
                                _sPalletID = string.Empty;
                                AllClear();
                            }
                            else
                            {
                                if (dtPRODLOT.Rows.Count > 0)
                                {
                                    drCurrRow = dtPRODLOT.Select("LOTID ='" + _sPalletID + "'", "")[0];
                                    if (drCurrRow != null)
                                    {
                                        DataTable dtCurrLot = dtPRODLOT.Clone();
                                        dtCurrLot.ImportRow(drCurrRow);
                                        getLotDetail(dtCurrLot);
                                        getLotTrayInfo(_sPalletID);
                                        getAssyLot(_sPalletID);
                                    }
                                    else
                                    {
                                        _sPalletID = string.Empty;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        AllClear();
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked)
                {
                    selLotData = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.DataItem;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgProductLot.SelectedIndex = idx;
                    AllClear();
                    if (dtPRODLOT != null)
                    {
                        DataTable dtCurrLot = dtPRODLOT.Clone();
                        selPalletID = (selLotData as DataRowView)["LOTID"].ToString();
                        drCurrRow = dtPRODLOT.Select("LOTID ='" + selPalletID + "'", "")[0];
                        dtCurrLot.ImportRow(drCurrRow);
                        getLotDetail(dtCurrLot);
                        getLotTrayInfo(selPalletID);
                        getAssyLot(selPalletID);
                    }

                }
                // 현재 선택된 라디오버튼의 Pallet Id ( Lot ID ) 가져오기
                ChekSetPalletID();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 시작 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selLotData == null)
                {
                    // 작업 취소하고자 하는 LotID를 선택하신 후 진행하시길 바랍니다.
                    Util.MessageValidation("SFU3167");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // 작업자를 입력해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                string sLotid = (selLotData as DataRowView)["LOTID"].ToString();
                string sProcid = (selLotData as DataRowView)["PROCID"].ToString();

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // 작업 취소 함수 호출
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("PROCID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("IFMODE", typeof(string));

                            DataTable inLotTable = indataSet.Tables.Add("INLOT");
                            inLotTable.Columns.Add("LOTID", typeof(string));
                            inLotTable.Columns.Add("WIPNOTE", typeof(string));

                            DataRow inData = inDataTable.NewRow();
                            inData["SRCTYPE"] = "UI";
                            inData["PROCID"] = sProcid;
                            inData["USERID"] = txtWorker.Tag as string;
                            inData["IFMODE"] = "OFF";
                            inDataTable.Rows.Add(inData);

                            DataRow inDataMag = inLotTable.NewRow();
                            inDataMag["LOTID"] = sLotid;
                            inDataMag["WIPNOTE"] = txtRemark.Text.Trim();
                            inLotTable.Rows.Add(inDataMag);

                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CANCEL_END_PALLET_BX", "INDATA,INLOT", null, indataSet);

                        }
                        catch (Exception ex)
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);                        
                            //Util.AlertByBiz("BR_PRD_REG_CANCEL_OUTER_PACK_BY_EQ_CP", ex.Message, ex.ToString());
                            Util.MessageException(ex);
                            return;
                        }

                        // 재조회
                        btnSearch_Click(null, null);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 실적 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pallet 선택 여부 확인
                if (selLotData == null)
                {
                    //실적확정 할 LOT이 선택되지 않았습니다.
                    Util.MessageValidation("SFU1717");
                    return;
                }

                string sWipStat = (selLotData as DataRowView)["WIPSTAT"].ToString();
                if (sWipStat != "PACKED")
                {
                    //진행 중인 LOT은 확정처리할 수 없습니다.
                    Util.MessageValidation("SFU3171");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //진행 중인 LOT은 확정처리할 수 없습니다.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                
                // Pallet 을 구성하고 있는 셀 수량이 540개인지 여부 확인 및 정합성 체크 누락 여부 확인
                // PalletID 를 조회조건으로 하여 Pallet 을 구성하는 셀들의 수량 확인하는 함수 호출 : 20101013 홍광표
                int resultCellCount = SearchCellCount(selPalletID);

                // Cell 수량이 540개 아니라면 팝업창 띄움 : 셀 수량이 540개 안 되어도 확정이 가능하도록 수정 요청. 2011 05 25 홍광표S
                // PALLET별 기준수량으로 체크하도록 변경. 2012.06.13 김홍동
                int CellMaxCount = SerarchPalletMaxQty(selPalletID);

                //if (resultCellCount != 540)
                if (resultCellCount != CellMaxCount)
                {
                    //전산의 투입 Cell 수량이 %1EA 이며, \r\n 
                    //Tray 수량은 %2 EA 입니다. \r\n
                    // Cell 수량 %3 EA 로 실적 확정처리 하시겠습니까?
                    string sCellCount = resultCellCount.ToString();
                    string sTrCount = dgTray.Rows.Count.ToString();

                    object[] param = new object[] { CellMaxCount, sTrCount, sCellCount };

                    Util.MessageConfirm("SFU3168", (result3) =>
                    {
                        if (result3 == MessageBoxResult.OK)
                        {

                            // Cell 정합성 누락 확인 함수 호출.
                            Boolean resultCell = SelectCellCheck(selPalletID);

                            // 위 함수에서 false 를 리턴한 경우 함수 진행 중지
                            if (!resultCell) return;

                            PopUpConfirm();

                        }
                        else
                        {
                            return;
                        }
                    }, param);
                }
                else
                {
                    // Cell 정합성 누락 확인 함수 호출.
                    Boolean resultCell = SelectCellCheck(selPalletID);

                    // 위 함수에서 false 를 리턴한 경우 함수 진행 중지
                    if (!resultCell) return;

                    PopUpConfirm();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PopUpConfirm()
        {
            try
            {
                // 총 셀 수량
                int totalCount = 0;

                // Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
                lsDataTable.Columns.Add("QTY", typeof(string));

                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    DataRow row = lsDataTable.NewRow();
                    row["TRAY_MAGAZINE"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                    row["QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                    lsDataTable.Rows.Add(row);

                    totalCount = totalCount + Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                }
                // 2023-10-20 실적확정 시 LOTID 단위수량 업데이트
                getAssyLot(_sPalletID);

                DateTime ShipDate_Schedule = DateTime.Now;
                
                

                // 폼 화면에 보여주는 메서드에 5개의 매개변수 전달
                BOX001_300_CONFIRM popUp = new BOX001_300_CONFIRM();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[18];
                    Parameters[0] = txtLotid.Text.Trim();
                    Parameters[1] = txtStartTime.Text.Trim();
                    Parameters[2] = txtProjectName.Text.Trim();
                    //Parameters[3] = Process.CELL_BOXING;
                    Parameters[3] = (selLotData as DataRowView)["PROCID"].ToString(); // 2024-05-15 이제섭 Pallet의 공정 ID로 변경
                    Parameters[4] = txtRemark.Text.Trim();
                    Parameters[5] = (selLotData as DataRowView)["MDLLOT_ID"].ToString();
                    Parameters[6] = sEQSGID;
                    Parameters[7] = sEQSGNAME.Replace(sEQSGID, string.Empty).Replace(":", string.Empty).Trim();
                    Parameters[8] = sEQPTID;
                    //Parameters[9] = txtProdid.Text.Trim();
                    Parameters[9] = txtTopProdid.Text.Trim(); //완제품 ID
                    Parameters[10] = lsDataTable;
                    Parameters[11] = dtASSYLOT;
                    Parameters[12] = totalCount.ToString();
                    Parameters[13] = sSHOPID;
                    Parameters[14] = sAREAID;
                    Parameters[15] = txtWorker.Text;
                    Parameters[16] = txtWorker.Tag as string;
                    Parameters[17] = ShipDate_Schedule.ToString("yyyy-MM-dd");

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndConfirm_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Tray 상세
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrayDetail_Click(object sender, RoutedEventArgs e)
        {

            // 진행중인 Lot 현황, Tray 구성 스프레드 에서 하나 이상 선택해야 아래 로직 수행
            int selCnt = 0;
            string tempPalletID = selPalletID;
            string sTrayID = string.Empty;
            string sEQSGID = string.Empty;
            string sMDLLOT_ID = string.Empty;
            decimal selCellQty = 0;

            for (int i = 0; i < dgTray.GetRowCount(); i++)
            {
                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "True")
                {
                    selCnt = selCnt + 1;
                    if (selCnt == 1)
                    {
                        sTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                        sEQSGID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["EQSGID"].Index).Value);
                        sMDLLOT_ID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["MDLLOT_ID"].Index).Value);
                    }
                }
            }

            if (selCnt == 0)
            {
                //PalletID/TrayID를 선택하신 후 Tray 상세 조회를 하시길 바랍니다.
                Util.MessageValidation("SFU3142");
                return;
            }
            else if (selCnt > 1)
            {
                //Tray는 하나만 선택하십시오.
                Util.MessageValidation("SFU3145");
                return;
            }

            int CellMaxCount = SerarchPalletMaxQty(selPalletID);

            if (CellMaxCount > 540)
            {
                BOX001_300_TRAY_DETL_NEW popUp = new BOX001_300_TRAY_DETL_NEW();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = tempPalletID;
                    Parameters[1] = sTrayID;
                    Parameters[2] = CellMaxCount.ToString();
                    Parameters[3] = sSHOPID;
                    Parameters[4] = sAREAID;
                    Parameters[5] = sEQSGID;
                    Parameters[6] = sMDLLOT_ID;
                    Parameters[7] = txtWorker.Tag.ToString();
                    Parameters[8] = sEQPTID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndTrayDetl_New_Closed);
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();

                }
            }
            else
            {
                BOX001_300_TRAY_DETL popUp = new BOX001_300_TRAY_DETL();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = tempPalletID;
                    Parameters[1] = sTrayID;
                    Parameters[2] = CellMaxCount.ToString();
                    Parameters[3] = sSHOPID;
                    Parameters[4] = sAREAID;
                    Parameters[5] = sEQSGID;
                    Parameters[6] = sMDLLOT_ID;
                    Parameters[7] = txtWorker.Tag.ToString();
                    Parameters[8] = sEQPTID;
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndTrayDetl_Closed);
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndTrayDetl_Closed(object sender, EventArgs e)
        {
            BOX001_300_TRAY_DETL wndPopup = sender as BOX001_300_TRAY_DETL;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                // 초기화 함수 호출
                getLotTrayInfo(selPalletID);
                getAssyLot(selPalletID);
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndTrayDetl_New_Closed(object sender, EventArgs e)
        {
            BOX001_300_TRAY_DETL_NEW wndPopup = sender as BOX001_300_TRAY_DETL_NEW;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                // 초기화 함수 호출
                getLotTrayInfo(selPalletID);
                getAssyLot(selPalletID);
            }
            grdMain.Children.Remove(wndPopup);
        }

        /// <summary>
        /// Tray 확정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrayConfirm_Click(object sender, RoutedEventArgs e)
        {
            int selCnt = 0;
            int selRow = 0; // 단일선택시에 참조하기 위해 선언.

            for (int i = 0; i < dgTray.GetRowCount(); i++)
            {
                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "True")
                {
                    selCnt = selCnt + 1;
                    selRow = i;
                }
            }

            if (selCnt == 0)
            {
                //PalletID/TrayID를 선택하신 후 Tray 확정을 하시길 바랍니다.
                Util.MessageValidation("SFU3143");
                return;
            }


            string lsCheckYN = "";  //덮개여부

            try
            {

                // 선택된 TrayID
                string lsTrayId = "";
                // 선택된 Tray에 속한 셀 수량
                int liCellSum = 0;

                // packMessage에 표시하기 위해 선택한 TrayID / Cell 수량 모두 변수에 저장
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "True")
                    {
                        string sTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                        int iCellQty = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                        lsTrayId = lsTrayId + sTrayID + "(Qty : " + iCellQty + "EA)" + Convert.ToString((char)13) + Convert.ToString((char)10);
                        liCellSum = liCellSum + iCellQty;
                    }
                }


                string tmpTrayID = string.Empty;
                int tmpCellQty = 0;

                // 다중 선택 여부 확인함 : 다중일 경우
                if (selCnt > 1)
                {
                    //%1\r\nTotal Cell Qty: %2\r\n선택된 Tray를 확정하시겠습니까?
                    // Popup 창 정의해서 해당 폼에 위에서 저장했던 변수와 수량 Data 메시지로 넘김
                    object[] param = new object[] { lsTrayId, liCellSum.ToString() };

                    Util.MessageConfirm("SFU3375", (result2) =>
                    {
                        if (result2 == MessageBoxResult.OK)
                        {
                            for (int i = 0; i < dgTray.GetRowCount(); i++)
                            {
                                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "True")
                                {

                                    tmpTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                                    tmpCellQty = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                                    // 덮개 부분 체크 여부 확인
                                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["COVERYN"].Index).Value) == "True")
                                    {
                                        lsCheckYN = "Y";
                                    }
                                    else
                                    {
                                        lsCheckYN = "N";
                                    }

                                    try
                                    {

                                        DataTable inTable = new DataTable();
                                        inTable.Columns.Add("BOXID", typeof(string));
                                        inTable.Columns.Add("OUTER_BOXID", typeof(string));
                                        inTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                                        inTable.Columns.Add("COVER_FLAG", typeof(string));

                                        DataRow newRow = inTable.NewRow();
                                        newRow["BOXID"] = tmpTrayID;
                                        newRow["OUTER_BOXID"] = selPalletID;
                                        newRow["TOTAL_QTY"] = tmpCellQty;
                                        newRow["COVER_FLAG"] = lsCheckYN;

                                        inTable.Rows.Add(newRow);

                                        new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CONFIRM_TRAY_CP", "INDATA", null, inTable);

                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                        return;
                                    }
                                }
                            }

                            // Tray 구성 정보 조회                                                                                                                                     
                            getLotTrayInfo(selPalletID);

                            Util.MessageInfo("SFU2040"); //"확정 처리 되었습니다"

                        }

                    }, new object[] { lsTrayId, liCellSum.ToString() });

                }
                else
                {
                    //             + "Total Cell Qty: " + liCellSum.ToString() + "EA" + Convert.ToString((char)13) + Convert.ToString((char)10)
                    //             + "선택된 TRAY를 확정하시겠습니까?", null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None,
                    object[] param = new object[] { lsTrayId, liCellSum.ToString() };

                    Util.MessageConfirm("SFU3375", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            tmpTrayID = Util.NVC(dgTray.GetCell(selRow, dgTray.Columns["TRAYID"].Index).Value);
                            tmpCellQty = Util.NVC_Int(dgTray.GetCell(selRow, dgTray.Columns["QTY"].Index).Value);
                            // 덮개 부분 체크 여부 확인
                            if (Util.NVC(dgTray.GetCell(selRow, dgTray.Columns["COVERYN"].Index).Value) == "True")
                            {
                                lsCheckYN = "Y";
                            }
                            else
                            {
                                lsCheckYN = "N";
                            }

                            try
                            {

                                DataTable inTable = new DataTable();
                                inTable.Columns.Add("BOXID", typeof(string));
                                inTable.Columns.Add("OUTER_BOXID", typeof(string));
                                inTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                                inTable.Columns.Add("COVER_FLAG", typeof(string));

                                DataRow newRow = inTable.NewRow();
                                newRow["BOXID"] = tmpTrayID;
                                newRow["OUTER_BOXID"] = selPalletID;
                                newRow["TOTAL_QTY"] = tmpCellQty;
                                newRow["COVER_FLAG"] = lsCheckYN;

                                inTable.Rows.Add(newRow);

                                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CONFIRM_TRAY_CP", "INDATA", null, inTable);

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            // Tray 구성 정보 조회                                                                                                                            
                            getLotTrayInfo(selPalletID);

                            Util.MessageInfo("SFU2040"); //"확정 처리 되었습니다"
                        }

                    }, new object[] { lsTrayId, liCellSum.ToString() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_300_CONFIRM wndPopup = sender as BOX001.BOX001_300_CONFIRM;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

                // 초기화 함수 호출
                //AllClear();
                btnSearch_Click(null, null);

                DataTable dtPalletHisCard = wndPopup.retDT;

                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = getPalletTagCount(sAREAID); // 2019.07.24 수정. 
                Parameters[3] = "Y";
                Parameters[4] = sSHOPID;

                Report_Pallet_Hist rs = new Report_Pallet_Hist();
                C1WindowExtension.SetParameters(rs, Parameters);
                grdMain.Children.Add(rs);
                rs.BringToFront();
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void dgTray_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (dgTray.CurrentRow == null || dgTray.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgTray.CurrentColumn.Name == "CHK")
                {
                    if (Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["CHK"].Index).Value) == "True")
                    {
                        DataTableConverter.SetValue(dgTray.Rows[dgTray.CurrentRow.Index].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgTray.Rows[dgTray.CurrentRow.Index].DataItem, "CHK", true);
                    }


                }
                else if (e.ChangedButton.ToString().Equals("Left") && dgTray.CurrentColumn.Name == "COVERYN")
                {
                    for (int i = 0; i < dgTray.GetRowCount(); i++)
                    {
                        if (i == dgTray.CurrentRow.Index)
                        {
                            if (Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["COVERYN"].Index).Value) == "True")
                            {
                                DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "COVERYN", false);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "COVERYN", true);
                            }
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "COVERYN", false);
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
                dgTray.CurrentRow = null;
             }
        }

        #endregion

        #region Mehod

        private void AllClear()
        {
            dgTray.ItemsSource = null;
            dgAssyLot.ItemsSource = null;

            txtLotid.Text = string.Empty;
            txtProjectName.Text = string.Empty;
            txtProdid.Text = string.Empty;
            txtTopProdid.Text = string.Empty;
            txtProdname.Text = string.Empty;

            txtStartQty.Text = String.Empty;
            txtGoodQty.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtRemark.Text = string.Empty;

        }

        private bool AuthCheck()
        {
            try
            {

                // 2024.06.21 Fauzul A.
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LANGID", typeof(string));
                RQSTDT1.Columns.Add("AREAID", typeof(string));
                RQSTDT1.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT1.Columns.Add("COM_CODE", typeof(string));
                RQSTDT1.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr1["COM_TYPE_CODE"] = "FORM_UI_BTN_AUTH";
                dr1["COM_CODE"] = "AUTO_PACKING_MANUAL_CONFIRM";
                RQSTDT1.Rows.Add(dr1);

                DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT1);

                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("USERID", typeof(string));
                RQSTDT2.Columns.Add("AUTHID", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                if (dtResult1.Rows.Count > 0)
                {
                    DataRow sourceRow = dtResult1.Rows[0];
                    dr2["USERID"] = LoginInfo.USERID;
                    dr2["AUTHID"] = sourceRow["ATTR1"];
                    RQSTDT2.Rows.Add(dr2);
                }

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT2);

                if (dtResult2 == null || dtResult2.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// LOT 상세 조회
        /// </summary>
        /// <param name="dataItem"></param>
        private void getLotDetail(DataTable dt)
        {
            try
            {
                // [강제종료] 버튼
                // WIPQTY와 BOX_TOTL_CELL_QTY가 수량이 같고
                // WIPSTAT가 PROC 일때만 강제종료할 수 있음. 
                // -> 강제종료 오작동시 설비와 문제가 생길 수 있어서 사용가능할 때에만 VISIBLE 처리함
                btnEnd.Visibility = ((Util.NVC_Int(dt.Rows[0]["WIPQTY"]) == Util.NVC_Int(dt.Rows[0]["BOX_TOTL_CELL_QTY"])) && Util.NVC(dt.Rows[0]["WIPSTAT"]) == "PACKING") ? Visibility.Visible : Visibility.Collapsed;

                // -> 강제종료 오작동시 설비와 문제가 생길 수 있어서 사용가능할 때에만 VISIBLE 처리함 (권한 주석 처리)
                //  if (AuthCheck())
                //     btnEnd.Visibility = Visibility.Visible;

                txtLotid.Text = Util.NVC(dt.Rows[0]["LOTID"]);
                txtProjectName.Text = Util.NVC(dt.Rows[0]["PROJECTNAME"]);
                txtProdid.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                txtProdname.Text = Util.NVC(dt.Rows[0]["PRODNAME"]);
                txtStartQty.Text = Util.NVC_Int(dt.Rows[0]["WIPQTY"]).ToString();   
                txtGoodQty.Text = Util.NVC_Int(dt.Rows[0]["WIPQTY"]).ToString();
                txtStartTime.Text = Util.NVC(dt.Rows[0]["WIPDTTM_ST"]);
                txtEndTime.Text = Util.NVC(dt.Rows[0]["WIPDTTM_ED"]);
                txtRemark.Text = Util.NVC(dt.Rows[0]["WIPNOTE"]);

                txtTopProdid.Text = Util.NVC(dt.Rows[0]["TOP_PRODID"]);
                txtTopProdNm.Text = Util.NVC(dt.Rows[0]["TOP_PRODNAME"]);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getAssyLot(string sPalletID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("BOXID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["BOXID"] = sPalletID;
            RQSTDT.Rows.Add(dr);
            
            DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSY_LOT_BY_PALLET_BX", "RQSTDT", "RSLTDT", RQSTDT);
            Util.GridSetData(dgAssyLot, searchResult, FrameOperation);
            dtASSYLOT = searchResult.Copy();

        }

        private void dgAssyLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0)
                return;

            string sPalletid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PALLETID"].Index).Value);
            string sLotid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value);


            BOX001_300_LOT_DETL popUp = new BOX001_300_LOT_DETL();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = sPalletid;
                Parameters[1] = sLotid;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.ShowModal();
                popUp.CenterOnScreen();
            }

        }

        private void getLotTrayInfo(string sPalletID)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgTray, dtResult, FrameOperation);
                dtTRAY = dtResult.Copy();

                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgTray_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgTray.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgTray.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgTray.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgTray.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }

        /// <summary>
        /// Cell 정합성 누락 확인 함수
        /// </summary>
        /// <returns></returns>
        private Boolean SelectCellCheck(string sPalletid)
        {

            if (chkDummy.IsChecked == true)  //  dummy 이면  예외 처리
            {
                return true;
            }
            else
            {
                // PalletID 를 조회조건으로 하여 Cell 정합성 체크 여부 확인하는 함수 호출 : 20101013 홍광표
                DataTable resultCellCheck = SearchCellCheck(sPalletid);

                // 정합성 체크 정보가 누락된 셀이 있다면 아래 로직 수행
                if (resultCellCheck.Rows.Count > 0)
                {

                    BOX001_300_LOT_DETL_NEW popUp = new BOX001_300_LOT_DETL_NEW();
                    popUp.FrameOperation = this.FrameOperation;

                    if (popUp != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = sPalletid;
                        Parameters[1] = resultCellCheck;
                        Parameters[2] = sSHOPID;
                        Parameters[3] = sAREAID;
                        Parameters[4] = txtWorker.Tag.ToString();
                        Parameters[5] = sEQPTID;
                        C1WindowExtension.SetParameters(popUp, Parameters);

                        popUp.ShowModal();
                        popUp.CenterOnScreen();
                    }
                    //비동기라서.. 팝업화면 보다 먼저 리턴됨..(상관없음)
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

        /// <summary>
        /// PalletID 를 조회조건으로 하여 Pallet 을 구성하는 셀들의 수량
        /// </summary>
        /// <param name="sPalletID"></param>
        private int SearchCellCount(string sPalletID)
        {
            // 기존Biz : QR_MLB05_01_CELLCOUNT
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLT_CELLCOUNT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                // 데이터테이블에 값이 없다면 0 리턴하면서 함수 종료
                if (dtResult.Rows.Count <= 0)
                {
                    return 0;
                }
                return Util.NVC_Int(dtResult.Rows[0]["CELLQTY"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        /// <summary>
        /// Pallet ID로 CELL 기준수량을 리턴함
        /// </summary>
        /// <param name="sPalletID"></param>
        /// <returns></returns>
        private int SerarchPalletMaxQty(string sPalletID)
        {
            // 기존Biz : R_GMPALLET_JOIN_PACK_MASTER
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLT_MAXCELLCNT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                // 데이터테이블에 값이 없다면 0 리턴하면서 함수 종료
                if (dtResult.Rows.Count <= 0)
                {
                    return 0;
                }
                return Util.NVC_Int(dtResult.Rows[0]["CELLQTY"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        /// <summary>
        /// 해당 PALLETID 를 조회조건으로 정합성 체크 누락된 셀이 있는지 여부 확인하는 함수
        /// </summary>
        /// <param name="palletID"></param>
        private DataTable SearchCellCheck(string palletID)
        {

            // 기존Biz : QR_MLB05_01_CELLCHECK
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLT_CELLCHECK_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        #endregion

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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINE");

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sEqsgid = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sEqsgid == "" || sEqsgid == "SELECT")
            {
                sEqsgid = "";
            }
            _sPalletID = string.Empty;

            GetPilotProdMode();
            //String[] sFilter = { sEqsgid, Process.CELL_BOXING };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, sFilter:sFilter);
        }

        private void TimerSetting()
        {
            _Timer.Interval = new TimeSpan(0, 0, 0, iInterval);
            _Timer.Tick += new EventHandler(Timer_Tick);
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (chkReload.IsChecked != true)  // 작업대상이 선택안되었다면 Pass
                {
                    _Timer.Stop();
                    return;
                }
                if (_sPalletID == string.Empty)  // 작업대상이 선택안되었다면 Pass
                {
                    return;
                }
                btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 현재 선택된 라디오버튼의 Pallet Id ( Lot ID ) 가져오기
        private void ChekSetPalletID()
        {
            string strPalletID;
            strPalletID = string.Empty;
            //조회버튼 여러번 누를경우 CHK 타입변환 에러 발생하여 수정 2025.03.31
            //List<int> list = _Util.GetDataGridCheckRowIndex(dgProductLot, "CHK");
            //if (list.Count <= 0)
            //{
            //    return;
            //}
            //foreach (int row in list)
            //{
            //    strPalletID = DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID").ToString();
            //}

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (iRow == -1)
            {
                return;
            }
            else
            {
                strPalletID = DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID").ToString();
            }

            _sPalletID = strPalletID;
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER wndPopup = new CMM001.Popup.CMM_SHIFT_USER();
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
            CMM001.Popup.CMM_SHIFT_USER window = sender as CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //진행하시겠습니까?
                Util.MessageConfirm("SFU1170", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        try
                        {
                            if (drCurrRow == null) return;

                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                            inDataTable.Columns.Add("EQPTID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("MODELID", typeof(string));
                            inDataTable.Columns.Add("LANGID", typeof(string));

                            DataTable inLotTable = indataSet.Tables.Add("IN_PALLET");
                            inLotTable.Columns.Add("PALLETID", typeof(string));
                            inLotTable.Columns.Add("PACKING_QTY", typeof(string));

                            DataRow inData = inDataTable.NewRow();
                            inData["EQPTID"] = drCurrRow["EQPTID"];
                            inData["USERID"] = txtWorker.Tag as string;
                            inData["MODELID"] = drCurrRow["MDLLOT_ID"];
                            inData["LANGID"] = LoginInfo.LANGID;
                            inDataTable.Rows.Add(inData);

                            DataRow inDataMag = inLotTable.NewRow();
                            inDataMag["PALLETID"] = _sPalletID;
                            inDataMag["PACKING_QTY"] = txtStartQty.Text;
                            inLotTable.Rows.Add(inDataMag);

                            new ClientProxy().ExecuteService_Multi("BR_FORM_REG_END_PACKING_PALLET", "IN_EQP,IN_PALLET", null, (searchResult, searchException) =>
                            {
                                try
                                {
                                    if (searchException != null)
                                    {
                                        Util.MessageException(searchException);
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    // 재조회
                                    btnSearch_Click(null, null);
                                }
                            }, indataSet
                            );
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private bool CanPilotProdMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return bRet;
            }

            string sProcLotID = string.Empty;
            if (CheckProcWip(out sProcLotID))
            {
                Util.MessageValidation("SFU3199", sProcLotID); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        private bool CheckProcWip(out string sProcLotID)
        {
            sProcLotID = "";

            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = GetEqptID();
                dtRow["WIPSTAT"] = Wip_State.PROC;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sProcLotID = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private bool GetPilotProdMode()
        {
            try
            {
                bool bRet = false;

                if (cboEquipmentSegment == null || cboEquipmentSegment.SelectedIndex < 0 || Util.NVC(cboEquipmentSegment.SelectedValue).Trim().Equals("SELECT"))
                {
                    HidePilotProdMode();
                    return bRet;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = GetEqptID();

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR", "RQSTDT", "RSLTDT", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EQPT_OPER_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"]).Contains("PILOT"))
                    {
                        ShowPilotProdMode();
                        bRet = true;
                    }
                    else
                    {
                        HidePilotProdMode();
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SetPilotProdMode(string sMode, bool bMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = GetEqptID();
                newRow["PILOT_PRDC_MODE"] = bMode ? string.Empty : sMode;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        private string GetEqptID()
        {
            string sOutEqptID = string.Empty;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = "FF5101,B1000"; //Process.CELL_BOXING;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EOL_PACKER_V01", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                sOutEqptID = Util.NVC(dtRslt.Rows[0]["EQPTID"]);
            }
            else
            {
                sOutEqptID = string.Empty;
            }

            sEQPTID = sOutEqptID;

            return sOutEqptID;
        }

        private int GetPilotAdminArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PILOT_PROD_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
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

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void ShowPilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }
        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rect)
        {
            rect.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }

    }
}
