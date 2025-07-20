/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 포장 Pallet 구성 (자동)
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.11.07 이대근 [CSR ID:3781337] GMES update to Formation Data save, and Printing Pallet Labels | [요청번호]C20180902_81337 
  2019.05.20 이제섭 CNA 6호 종이박스포장기 BOX 취출 기능 추가.
  2019.05.29 이제섭 CNA 6호 종이박스포장기 BOX 추가 기능 추가.
  2019.09.02 최상민 [CSR ID:4059554] GEMS 셀 전압 구간 Tag 내용 추가 件 |[요청번호]C20190807_59554 |[서비스번호]4059554\
  2019.12.27 이동우 추가기능 - 설비 시생산 Mode 생산
  2020.07.18 이제섭 UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2022.01.12 오화백 24인의 경우 SORTING 설비와 같이 사용하므로 색깔로 구분 기능 추가
  2022.03.28 오화백 소팅기 신규 라인 생성으로 조회시 소팅라인일 경우 라인정보와 라인명정보를 전역변수에 담는 로직 수정
  2022.12.27 안유수 C20221026-000317 ESWA 강제완료버튼 오류 수정건(CSTSTAT INDATA 컬럼 추가)
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

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

        public BOX001_005()
        {
            InitializeComponent();
            Loaded += BOX001_005_Loaded;
        }

        private void BOX001_005_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_005_Loaded;

            // 기본은 체크 해제
            //chkTrayRefresh.Checked = false;
            //chkDummy.Visible = false;

            setInit();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //OC일 경우 Cell 조회 버튼 보이도록
            if (LoginInfo.CFG_SHOP_ID == "A040")
            {
                btnCellDetail.Visibility = Visibility.Visible;
            }

            // CNA일 경우 BOX 취출, 추가 버튼 보이도록 
            if (LoginInfo.CFG_SHOP_ID == "G451")
            {
                btnUnpackBox.Visibility = Visibility.Visible;
                btnAddBox.Visibility = Visibility.Visible;
            }
            else //CNA 이외는 버튼 숨김
            {
                btnUnpackBox.Visibility = Visibility.Collapsed;
                btnAddBox.Visibility = Visibility.Collapsed;
            }

            if(GetPilotAdminArea() > 0) // 공통코드로 시생산 제품 관리하는 동에 따라 버튼 Visible
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
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT,  sCase:"AREA_CP");           
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
            if (!CanPilotProdMode()) return;

            string sMsg = string.Empty;
            bool bMode = GetPilotProdMode();

            if (bMode)
                sMsg = "SFU2875";
            else
                sMsg = "SFU2875";

            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetPilotProdMode(!bMode);
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

            //string sEqptid = Util.NVC(cboEquipment.SelectedValue);
            //if (sEqptid == string.Empty || sEqptid == "SELECT")
            //{
            //    Util.AlertInfo("설비를 선택해주십시오.");
            //    return;
            //}

            //초기화
            selPalletID = string.Empty;
            selLotData = null;
            dtPRODLOT = null;
            dtTRAY = null;
            dtASSYLOT = null;
            // 현재 선택된 라디오버튼의 Pallet Id ( Lot ID ) 가져오기
            ChekSetPalletID();

            AllClear();
            //2022-03-28 ESWA 조립3동일경우 : 소팅라인 선택시 라인정보 적용
            if (LoginInfo.CFG_AREA_ID == "A7")
            {
                //소팅설비는 여러라인으로 포장할수 있어서 조회시 라인을 정할 수 없음
               if (Util.NVC(cboEquipmentSegment.SelectedValue) == "A7AS1")
                {
                    sEQSGID = string.Empty;
                    sEQSGNAME = string.Empty;
                    dgProductLot.Columns["EQSGNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);
                    sEQSGNAME = Util.NVC(cboEquipmentSegment.Text);
                    dgProductLot.Columns["EQSGNAME"].Visibility = Visibility.Collapsed;
                }
                
            }
            else
            {
                sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);
                sEQSGNAME = Util.NVC(cboEquipmentSegment.Text);
            }
    
            //sEQPTID = Util.NVC(cboEquipment.SelectedValue);

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            //2022-03-28 오화백  - 소팅설비여부
            RQSTDT.Columns.Add("SORTING_YN", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = Process.CELL_BOXING;

            //2022-03-28 오화백 - 조립 3동일경우 소팅라인 선택시 소팅기에서 생산중인 포장정보 조회
            if (LoginInfo.CFG_AREA_ID == "A7")
            {
                 //소팅라인일 경우 : 라인아이디 없이 현재 소팅설비에서 생산중이거나 장비완료인 정보 조회
                if (Util.NVC(cboEquipmentSegment.SelectedValue) == "A7AS1")
                {
                    dr["EQPTID"] = "W1AWRPM18";
                }
                //소팅라인이 아닐 경우  해당 라인에서  소팅설비에서 생산중인 정보는 제외 
                else
                {
                    dr["EQSGID"] = sEQSGID;
                    dr["SORTING_YN"] = "N";
                }
            }
            else //ESWA 조립3동외는 기존그대로
            {
                dr["EQSGID"] = sEQSGID;
            }

            //dr["EQPTID"] = sEQPTID;


            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_CP", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(searchException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                        return;
                    }
                         
                    Util.GridSetData(dgProductLot, searchResult, FrameOperation);
                    DataTable DtTemp = null;
                    DtTemp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                    dtPRODLOT = DtTemp.Copy();

                    if (chkReload.IsChecked == true )  // 작업대상이 선택안되었다면 Pass
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
                            //AllClear();
                            //DtTemp = null;
                            //DtTemp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                            //dtPRODLOT = DtTemp.Copy();
                            //DataTable dtCurrLot = dtPRODLOT.Clone();
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
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
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
                    //    dgProductLot.BeginEdit();
                    //    dgProductLot.ItemsSource = DataTableConverter.Convert(dt);
                    //    dgProductLot.EndEdit();
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
                       //2022-03-28 오화백 - 조립 3동일경우 소팅라인일 경우  선택된 포장정보의 생산 라인정보를 셋팅한다.
                        if (LoginInfo.CFG_AREA_ID == "A7")
                        {
                            if (Util.NVC(cboEquipmentSegment.SelectedValue) == "A7AS1")
                            {
                                sEQSGID = (selLotData as DataRowView)["EQSGID"].ToString();
                                sEQSGNAME = (selLotData as DataRowView)["EQSGNAME"].ToString();
                            }
                        }
                      
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
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3167"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // 작업자를 입력해 주세요.
                    Util.MessageValidation("SFU1843");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                //string sWipStat = (selLotData as DataRowView)["WIPSTAT"].ToString();
                //if (sWipStat != "PROC")
                //{
                //    //진행중인 Lot만 취소 할 수 있습니다.
                //    Util.MessageValidation("SFU3172");
                //  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3172"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //    return;
                //}

                string sLotid = (selLotData as DataRowView)["LOTID"].ToString();
                string sProcid = (selLotData as DataRowView)["PROCID"].ToString();

                //선택된 LOT을 작업 취소하시겠습니까?
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3151"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_OUTER_PACK_BY_EQ_CP", "INDATA,INLOT", null, indataSet);

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
                    //작업 취소하고자 하는 LotID를 선택하신 후 진행하시길 바랍니다.
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3167"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU3167");
                    return;
                }

                string sWipStat = (selLotData as DataRowView)["WIPSTAT"].ToString();
                if (sWipStat != "EQPT_END")
                {
                    //진행 중인 LOT은 확정처리할 수 없습니다.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3171"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU3171");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //진행 중인 LOT은 확정처리할 수 없습니다.
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU1843");
                    return;
                }

                // Tray
                int result = 0;
                // 덮개
                int cover = 0;

                // 스프레드의 Row만큼 반복하면서, 확정되지 않은 Tray 여부 확인
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    string trayValue = Util.NVC(dgTray.GetCell(i, dgTray.Columns["CONFIRMYN"].Index).Value);
                    string coverValue = Util.NVC(dgTray.GetCell(i, dgTray.Columns["COVERYN"].Index).Value);

                    if (trayValue == "N")
                    {
                        result = result + 1;
                    }

                    if (coverValue == "True")
                    {
                        cover = cover + 1;
                    }
                }


                // 확정이 되지 않은 Tray가 있다면 BIz Rule 호출하지 않음 20100305 홍광표
                if (dgTray.GetRowCount() == 0)
                {
                    //Tray 정보가 존재하지 않습니다. 해당 실적은 확정하실 수 없습니다.
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3144"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU3144");
                    return;
                }
                else if (result > 0)
                {
                    //Tray를 먼저 확정처리해주십시오
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1250"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU1250");
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

                    //sUserID = frmPopConfirm.Show("전산의 투입 Cell 수량이 " + resultCellCount.ToString() + " EA 이며" + CurrentInfo.crlf +
                    //                                "Tray 수량은 " + spdTrayInfo_Sheet1.Rows.Count.ToString() + " EA 입니다." + CurrentInfo.crlf + CurrentInfo.crlf +
                    //                                "Cell 수량 " + resultCellCount.ToString() + " EA 로 실적 확정처리 하시겠습니까?");
                    //전산의 투입 Cell 수량이 %1EA 이며, \r\n 
                    //Tray 수량은 %2 EA 입니다. \r\n
                    // Cell 수량 %3 EA 로 실적 확정처리 하시겠습니까?
                    string sCellCount = resultCellCount.ToString();
                    string sTrCount = dgTray.Rows.Count.ToString();

                    object[] param = new object[] { CellMaxCount, sTrCount, sCellCount};

                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3168", new object[] { CellMaxCount, sTrCount, sCellCount }), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result3) =>
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

                DateTime ShipDate_Schedule = DateTime.Now;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOP_ADD_OCV2_DATE", "RQSTDT", "RSLTDT", RQSTDT);

                int AgingTime = Util.StringToInt(SearchResult.Rows[0]["AGINGTIME"].ToString());

               if (SearchResult.Rows[0]["RESULT"].ToString().Equals("Y"))
                {
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.Columns.Add("PALLETID", typeof(string));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["PALLETID"] = _sPalletID;

                    RQSTDT1.Rows.Add(dr1);

                    DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OCV2_LOT_INFO_FOR_PALLET", "RQSTDT", "RSLTDT", RQSTDT1);

                    ShipDate_Schedule = Util.StringToDateTime(Result.Rows[0]["OCV_DTTM"].ToString());

                    ShipDate_Schedule = Util.StringToDateTime(ShipDate_Schedule.AddDays(AgingTime).ToString("yyyy-MM-dd"));

                }




                // 폼 화면에 보여주는 메서드에 5개의 매개변수 전달
                BOX001_005_CONFIRM popUp = new BOX001_005_CONFIRM();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[18];
                    Parameters[0] = txtLotid.Text.Trim();
                    Parameters[1] = txtStartTime.Text.Trim();
                    Parameters[2] = txtProjectName.Text.Trim();
                    Parameters[3] = Process.CELL_BOXING;
                    Parameters[4] = txtRemark.Text.Trim();
                    Parameters[5] = (selLotData as DataRowView)["MDLLOT_ID"].ToString();
                    Parameters[6] = sEQSGID;
                    Parameters[7] = sEQSGNAME.Replace(sEQSGID, string.Empty).Replace(":", string.Empty).Trim();
                    Parameters[8] = sEQPTID;
                    Parameters[9] = txtProdid.Text.Trim();
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
                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                {
                    selCnt = selCnt + 1;
                    if (selCnt == 1)
                    {
                        sTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                        sEQSGID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["EQSGID"].Index).Value);
                        sMDLLOT_ID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["MDLLOT_ID"].Index).Value);
                        // selCellQty = Util.NVC_Decimal(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                    }       
                }
            }

            if (selCnt == 0)
            {
                //PalletID/TrayID를 선택하신 후 Tray 상세 조회를 하시길 바랍니다.
                Util.MessageValidation("SFU3142");
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3142"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }
            else if (selCnt > 1)
            {
                //Tray는 하나만 선택하십시오.
                Util.MessageValidation("SFU3145");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3145"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }
            //2022.03.28 오화백 : 작업자 정보없으면 메세지 출력 추가 
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                // 작업자를 입력해 주세요.
                Util.MessageValidation("SFU1843");
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }
            int CellMaxCount = SerarchPalletMaxQty(selPalletID);

            //string MAINFORMPATH = "LGC.GMES.MES.BOX001";
            //string MAINFORMNAME = "BOX001_005_TRAY_DETL";

            //if (CellMaxCount > 540)
            //{
            //    MAINFORMNAME = "BOX001_005_TRAY_DETL_NEW";
            //    //frmPop2.Text = "Tray : " + sTrayID;
            //        // 선택한 LotID는 해당 Popup에서 눈으로 확인은 안 되지만, 미리 지정함
            //      //  frmPop2.txtHPalletid.Text = this.sprWIPLotID_Sheet1.Cells.Get(GridUtil.GetCheckedCellFirstRow(sprWIPLotID, 0), 1).Value.ToString();
            //}
            //else
            //{
            //    MAINFORMNAME = "BOX001_005_TRAY_DETL";
            //    //frmPop3.Text = "Tray : " + sTrayID;
            //        // 선택한 LotID는 해당 Popup에서 눈으로 확인은 안 되지만, 미리 지정함
            //        //frmPop3.txtHPalletid.Text = this.sprWIPLotID_Sheet1.Cells.Get(GridUtil.GetCheckedCellFirstRow(sprWIPLotID, 0), 1).Value.ToString();
            //}

            if (CellMaxCount > 540)
            {
                BOX001_005_TRAY_DETL_NEW popUp = new BOX001_005_TRAY_DETL_NEW();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = tempPalletID;
                    Parameters[1] = sTrayID;
                    Parameters[2] = CellMaxCount.ToString();
                    Parameters[3] = sSHOPID;
                    Parameters[4] = sAREAID;
                    Parameters[5] = sEQSGID;
                    Parameters[6] = sMDLLOT_ID;
                    Parameters[7] = txtWorker.Tag.ToString();
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndTrayDetl_New_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();

                }
            }
            else
            {
                BOX001_005_TRAY_DETL popUp = new BOX001_005_TRAY_DETL();
                popUp.FrameOperation = this.FrameOperation;
                if (popUp != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = tempPalletID;
                    Parameters[1] = sTrayID;
                    Parameters[2] = CellMaxCount.ToString();
                    Parameters[3] = sSHOPID;
                    Parameters[4] = sAREAID;
                    Parameters[5] = sEQSGID;
                    Parameters[6] = sMDLLOT_ID;
                    Parameters[7] = txtWorker.Tag.ToString();
                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndTrayDetl_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }

        }

        private void wndTrayDetl_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_005_TRAY_DETL wndPopup = sender as BOX001.BOX001_005_TRAY_DETL;
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
            BOX001.BOX001_005_TRAY_DETL_NEW wndPopup = sender as BOX001.BOX001_005_TRAY_DETL_NEW;
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
                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                {
                    selCnt = selCnt + 1;
                    selRow = i;
                }
            }

            if (selCnt == 0)
            {
                //PalletID/TrayID를 선택하신 후 Tray 확정을 하시길 바랍니다.
                Util.MessageValidation("SFU3143");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3143"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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
                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
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
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(lsTrayId + Convert.ToString((char)13) + Convert.ToString((char)10)
                    // +"Total Cell Qty: " + liCellSum.ToString() + "EA" + Convert.ToString((char)13) + Convert.ToString((char)10)
                    // + "선택된 TRAY를 확정하시겠습니까?", null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None,

                    //%1\r\nTotal Cell Qty: %2\r\n선택된 Tray를 확정하시겠습니까?
                    // Popup 창 정의해서 해당 폼에 위에서 저장했던 변수와 수량 Data 메시지로 넘김
                    object[] param = new object[] { lsTrayId, liCellSum.ToString() };

                    Util.MessageConfirm("SFU3375", (result2) =>
                              {
                                  if (result2 == MessageBoxResult.OK)
                                  {
                                      for (int i = 0; i < dgTray.GetRowCount(); i++)
                                      {
                                          if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                                          {

                                              tmpTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                                              tmpCellQty = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                                              // 덮개 부분 체크 여부 확인
                                              if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["COVERYN"].Index).Value) == "1")
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
                                                  //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                                  Util.MessageException(ex);
                                                  //Util.AlertByBiz("BR_PRD_REG_CONFIRM_TRAY_CP", ex.Message, ex.ToString());
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
                    //%1\r\nTotal Cell Qty: %2\r\n선택된 Tray를 확정하시겠습니까?
                    // Popup 창 정의해서 해당 폼에 위에서 저장했던 변수와 수량 Data 메시지로 넘김
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(lsTrayId + Convert.ToString((char)13) + Convert.ToString((char)10)
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
                            if (Util.NVC(dgTray.GetCell(selRow, dgTray.Columns["COVERYN"].Index).Value) == "1")
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
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                //Util.AlertByBiz("BR_PRD_REG_CONFIRM_TRAY_CP", ex.Message, ex.ToString());
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
            BOX001.BOX001_005_CONFIRM wndPopup = sender as BOX001.BOX001_005_CONFIRM;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {

                // 초기화 함수 호출
                //AllClear();
                btnSearch_Click(null, null);

                DataTable dtPalletHisCard = wndPopup.retDT;

                if (dtPalletHisCard != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                    Parameters[1] = dtPalletHisCard;
                    //Parameters[2] = "2";
                    Parameters[2] = getPalletTagCount(sAREAID); // 2019.07.24 수정. 
                    Parameters[3] = "Y";
                    Parameters[4] = sSHOPID;

                    //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                    //C1WindowExtension.SetParameters(rs, Parameters);
                    //rs.Show();

                    LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                    C1WindowExtension.SetParameters(rs, Parameters);
                    //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }
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
                    //for (int i = 0; i < dgTray.GetRowCount(); i++)
                    //{
                    //    if (i == dgTray.CurrentRow.Index)
                    //    {
                    //        if (Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["CHK"].Index).Value) == "1")
                    //        {
                    //            DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "CHK", false);
                    //        }
                    //        else
                    //        {
                    //            DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "CHK", true);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        DataTableConverter.SetValue(dgTray.Rows[i].DataItem, "CHK", false);
                    //    }
                    //}

                    if (Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["CHK"].Index).Value) == "1")
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
                            if (Util.NVC(dgTray.GetCell(dgTray.CurrentRow.Index, dgTray.Columns["COVERYN"].Index).Value) == "1")
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
            catch(Exception ex)
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
            //dgDefect.ItemsSource = null;
            //dgQuality.ItemsSource = null;
            dgAssyLot.ItemsSource = null;


            txtLotid.Text = string.Empty;
            txtProjectName.Text = string.Empty;
            txtProdid.Text = string.Empty;
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

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
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
                btnEnd.Visibility = ((Util.NVC_Int(dt.Rows[0]["WIPQTY"]) == Util.NVC_Int(dt.Rows[0]["BOX_TOTL_CELL_QTY"])) && Util.NVC(dt.Rows[0]["WIPSTAT"]) == "PROC") ? Visibility.Visible : Visibility.Collapsed;

                if (AuthCheck())
                    btnEnd.Visibility = Visibility.Visible;

                txtLotid.Text = Util.NVC(dt.Rows[0]["LOTID"]);
                txtProjectName.Text = Util.NVC(dt.Rows[0]["PROJECTNAME"]);
                txtProdid.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                txtProdname.Text = Util.NVC(dt.Rows[0]["PRODNAME"]);
                txtStartQty.Text = (Util.NVC_Int(dt.Rows[0]["WIPQTY"]) + Util.NVC_Int(dt.Rows[0]["LOSSQTY"])).ToString();    // 투입수(생산수) : WIPQTY + LOSSQTY 
                txtGoodQty.Text = Util.NVC_Int(dt.Rows[0]["WIPQTY"]).ToString();                     
                txtStartTime.Text = Util.NVC(dt.Rows[0]["WIPDTTM_ST"]);
                txtEndTime.Text = Util.NVC(dt.Rows[0]["WIPDTTM_ED"]);
                txtRemark.Text = Util.NVC(dt.Rows[0]["WIPNOTE"]);

                TextBox _BCD1 = FindName("txtBarcodeID") as TextBox;
                if (_BCD1 != null)
                    _BCD1.Text = Util.NVC(dt.Rows[0]["PLLT_BCD_ID"]);  // 팔레트바코드
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
            RQSTDT.Columns.Add("SHOPID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["BOXID"] = sPalletID;
            dr["SHOPID"] = sSHOPID;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("BR_PRD_SEL_LOT_BY_PALLET", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                    }

                    //dgAssyLot.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgAssyLot, searchResult, FrameOperation);
                    dtASSYLOT = searchResult.Copy();


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        private void dgAssyLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0)
                return;

            string sPalletid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PALLETID"].Index).Value);
            string sLotid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["LOTID"].Index).Value);


            BOX001_005_LOT_DETL popUp = new BOX001_005_LOT_DETL();
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
                //dgTray.ItemsSource = DataTableConverter.Convert(dtResult);
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

                    BOX001_005_LOT_DETL_NEW popUp = new BOX001_005_LOT_DETL_NEW();
                    popUp.FrameOperation = this.FrameOperation;

                    if (popUp != null)
                    {
                        object[] Parameters = new object[5];
                        Parameters[0] = sPalletid;
                        Parameters[1] = resultCellCheck;
                        Parameters[2] = sSHOPID;
                        Parameters[3] = sAREAID;
                        Parameters[4] = txtWorker.Tag.ToString();
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
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINE_CP");

            // Barcode 속성 표시 여부
            isVisibleBCD(sAREAID);
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
                if ( chkReload.IsChecked != true )  // 작업대상이 선택안되었다면 Pass
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
            List<int> list = _Util.GetDataGridCheckRowIndex(dgProductLot, "CHK");
            if (list.Count <= 0)
            {
                return;
            }
            foreach (int row in list)
            {
                strPalletID = DataTableConverter.GetValue(dgProductLot.Rows[row].DataItem, "LOTID").ToString();
            }
            _sPalletID = strPalletID;
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
                           // drCurrRow[""]


                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("IFMODE", typeof(string));
                            inDataTable.Columns.Add("EQPTID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("FORM_LINEID", typeof(string));
                            inDataTable.Columns.Add("MODELID", typeof(string));

                            DataTable inLotTable = indataSet.Tables.Add("IN_PALLET");
                            inLotTable.Columns.Add("PALLETID", typeof(string));
                            inLotTable.Columns.Add("PACKING_QTY", typeof(string));
                            inLotTable.Columns.Add("CSTSTAT", typeof(string));

                            DataRow inData = inDataTable.NewRow();
                            inData["SRCTYPE"] = "UI";
                            inData["IFMODE"] = "OFF";
                            inData["EQPTID"] = drCurrRow["EQPTID"];
                            inData["USERID"] = txtWorker.Tag as string;
                            inData["FORM_LINEID"] = drCurrRow["FORM_LINEID"];
                            inData["MODELID"] = drCurrRow["MDLLOT_ID"];
                            

                            inDataTable.Rows.Add(inData);

                            DataRow inDataMag = inLotTable.NewRow();
                            inDataMag["PALLETID"] = _sPalletID;
                            inDataMag["PACKING_QTY"] = txtStartQty.Text;
                            inDataMag["CSTSTAT"] = "U";
                            inLotTable.Rows.Add(inDataMag);

                           // DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_PACKING_PALLET", "IN_EQP,IN_PALLET", null, indataSet);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_PACKING_PALLET", "IN_EQP,IN_PALLET", null, (searchResult, searchException) =>
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

        private void btnUnpackBox_Click(object sender, RoutedEventArgs e)
        {
            int selCnt = 0;
            int selRow = 0; // 단일선택시에 참조하기 위해 선언.

            for (int i = 0; i < dgTray.GetRowCount(); i++)
            {
                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                {
                    selCnt = selCnt + 1;
                    selRow = i;
                }
            }

            if (selCnt == 0)
            {
                //선택 후 작업하세요.
                Util.MessageValidation("SFU1629");
                return;
            }

            try
            {

                // 선택된 TrayID
                string lsTrayId = "";
                // 선택된 Tray에 속한 셀 수량
                int liCellSum = 0;

                // packMessage에 표시하기 위해 선택한 TrayID / Cell 수량 모두 변수에 저장
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                    {
                        string sTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);
                        int iCellQty = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                        lsTrayId = lsTrayId + sTrayID + "(Qty : " + iCellQty + "EA)" + Convert.ToString((char)13) + Convert.ToString((char)10);
                        liCellSum = liCellSum + iCellQty;
                    }
                }


                string tmpTrayID = string.Empty;

                // 다중 선택 여부 확인함 : 다중일 경우
                if (selCnt > 1)
                {
                    object[] param = new object[] { lsTrayId, liCellSum.ToString() };

                    //취출 하시겠습니까?
                    Util.MessageConfirm("SFU2968", (result2) =>
                    {
                        if (result2 == MessageBoxResult.OK)
                        {
                            for (int i = 0; i < dgTray.GetRowCount(); i++)
                            {
                                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["CHK"].Index).Value) == "1")
                                {

                                    tmpTrayID = Util.NVC(dgTray.GetCell(i, dgTray.Columns["TRAYID"].Index).Value);

                                    try
                                    {
                                        DataSet indataSet = new DataSet();
                                        DataTable inTable = indataSet.Tables.Add("INDATA");
                                        inTable.Columns.Add("USERID", typeof(string));
                                        inTable.Columns.Add("BOXID", typeof(string));
                                        inTable.Columns.Add("SRCTYPE", typeof(string));

                                        DataTable inLotTable = indataSet.Tables.Add("INBOX");
                                        inLotTable.Columns.Add("BOXID", typeof(string));

                                        DataRow newRow = inTable.NewRow();
                                        newRow["USERID"] = txtWorker.Tag as string;
                                        newRow["BOXID"] = _sPalletID;
                                        newRow["SRCTYPE"] = "UI";
                                        inTable.Rows.Add(newRow);

                                        DataRow inDataMag = inLotTable.NewRow();
                                        inDataMag["BOXID"] = tmpTrayID;
                                        inLotTable.Rows.Add(inDataMag);

                                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX_FOR_CNA_CP", "INDATA,INLOT", null, indataSet);

                                    }
                                    catch (Exception ex)
                                    {
                                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                        Util.MessageException(ex);
                                        //Util.AlertByBiz("BR_PRD_REG_CONFIRM_TRAY_CP", ex.Message, ex.ToString());
                                        return;
                                    }
                                }
                            }

                            // 재 조회                                                                                                                                    
                            getLotTrayInfo(selPalletID);
                            getAssyLot(selPalletID);
                            btnSearch_Click(null, null);

                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275");

                        }

                    }, new object[] { lsTrayId, liCellSum.ToString() });

                }
                else
                {
                    object[] param = new object[] { lsTrayId, liCellSum.ToString() };

                    //취출 하시겠습니까?
                    Util.MessageConfirm("SFU2968", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            tmpTrayID = Util.NVC(dgTray.GetCell(selRow, dgTray.Columns["TRAYID"].Index).Value);

                            try
                            {

                                DataSet indataSet = new DataSet();
                                DataTable inTable = indataSet.Tables.Add("INDATA");
                                inTable.Columns.Add("USERID", typeof(string));
                                inTable.Columns.Add("BOXID", typeof(string));
                                inTable.Columns.Add("SRCTYPE", typeof(string));

                                DataTable inLotTable = indataSet.Tables.Add("INBOX");
                                inLotTable.Columns.Add("BOXID", typeof(string));

                                DataRow newRow = inTable.NewRow();
                                newRow["USERID"] = txtWorker.Tag as string;
                                newRow["BOXID"] = _sPalletID;
                                newRow["SRCTYPE"] = "UI";
                                inTable.Rows.Add(newRow);

                                DataRow inDataMag = inLotTable.NewRow();
                                inDataMag["BOXID"] = tmpTrayID;
                                inLotTable.Rows.Add(inDataMag);

                                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX_FOR_CNA_CP", "INDATA,INLOT", null, indataSet);

                            }
                            catch (Exception ex)
                            {

                                Util.MessageException(ex);
                                return;
                            }

                            // 재 조회                                                                                                                                    
                            getLotTrayInfo(selPalletID);
                            getAssyLot(selPalletID);
                            btnSearch_Click(null, null);

                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275"); 
                        }

                    }, new object[] { lsTrayId, liCellSum.ToString() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnAddBox_Click(object sender, RoutedEventArgs e)
        {
            BOX001_005_ADD_BOX popUpBox = new BOX001_005_ADD_BOX();
            popUpBox.FrameOperation = this.FrameOperation;
            if (popUpBox != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _sPalletID;
                Parameters[1] = txtWorker.Tag.ToString();
  
                C1WindowExtension.SetParameters(popUpBox, Parameters);

                popUpBox.Closed += new EventHandler(wndAddBox_Closed);
                grdMain.Children.Add(popUpBox);
                popUpBox.BringToFront();
            }
        }

        private void wndAddBox_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_005_ADD_BOX wndPopup = sender as BOX001.BOX001_005_ADD_BOX;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(null, null);
            }
            grdMain.Children.Remove(wndPopup);
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
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
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
                //ShowLoadingIndicator();

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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["PILOT_PROD_MODE"]).Equals("Y"))
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

        private bool SetPilotProdMode(bool bMode)
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
                newRow["PILOT_PRDC_MODE"] = bMode ? "PILOT" : "";
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
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
            dr["PROCID"] = Process.CELL_BOXING;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "INDATA", "OUTDATA", inTable);

            //return Util.NVC(dtRslt.Rows[0]["CBO_CODE"]);

            // 설비 null일 때 방어로직 추가.
            return dtRslt.Rows.Count > 0 ? Util.NVC(dtRslt.Rows[0]["CBO_CODE"]) : string.Empty;
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

        // Cell 조회 버튼
        private void btnCellDetail_Click(object sender, RoutedEventArgs e)
        {
            ChekSetPalletID();

            if (selPalletID == string.Empty)
            {
                // 선택된 Pallet가 없습니다.
                Util.MessageValidation("SFU3425");
                return;
            }

            BOX001_005_CELL_DETL popUp = new BOX001_005_CELL_DETL();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = selPalletID;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null) return;

            //C1DataGrid dataGrid = sender as C1DataGrid;

            //dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null) return;

            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //       if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "W1AWRPM18"))
            //        {
                       
            //            if (e.Cell.Column.Name.Equals("LOTID"))
            //            {
            //                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
            //                if (convertFromString != null)
            //                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
            //            }
            //            else
            //            {
            //                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
            //                if (convertFromString != null)
            //                    e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
            //            }

                      
            //        }
            //        else
            //        {
            //            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
            //            if (convertFromString != null)
            //                e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
            //        }

            //    }
            //}));
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null) return;

            //C1DataGrid dataGrid = sender as C1DataGrid;
            //dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //            e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //        }
            //    }
            //}));
        }


        #region 2022-03-28 오화백 현재는 사용하지 않지만 다시 사용할수 있어서 주석처리함

        //2022-03-28 오화백  소팅설비에 설정된 활성화라인 아이디로  현재 생산중인 MES 라인 조회 
        //private DataTable SortingCurrEqsgid(string eqptid)
        //{
        //    DataTable _dtResult = null;
        //    try
        //    {
        //        DataTable inDataTable = new DataTable("INDATA");
        //        inDataTable.Columns.Add("LANGID", typeof(string));
        //        inDataTable.Columns.Add("EQPTID", typeof(string));
        //        inDataTable.Columns.Add("AREAID", typeof(string));

        //        DataRow dr = inDataTable.NewRow();

        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["EQPTID"] = eqptid;
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;

        //        inDataTable.Rows.Add(dr);


        //        _dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SORTING_CURR_LINE", "RQSTDT", "RSLTDT", inDataTable);

        //        return _dtResult;
        //    }

        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        return _dtResult;
        //    }
        //}

        //2022-03-28 오화백  소팅설비에 설정된 활성화라인 아이디의 설비 조회
        //private string CurrLine_Equipment(string eqsgid)
        //{
        //    string _eqptid = string.Empty;
        //    try
        //    {
        //        DataTable inDataTable = new DataTable("INDATA");
        //        inDataTable.Columns.Add("EQSGID", typeof(string));


        //        DataRow dr = inDataTable.NewRow();

        //        dr["EQSGID"] = eqsgid;

        //        inDataTable.Rows.Add(dr);


        //       DataTable _dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_LINE_PACK_EQUIPMENT_SORITNG", "RQSTDT", "RSLTDT", inDataTable);

        //        if(_dtResult.Rows.Count > 0)
        //        {
        //            _eqptid = _dtResult.Rows[0]["EQPTID"].ToString();
        //        }
        //        else
        //        {
        //            _eqptid = string.Empty;
        //        }


        //        return _eqptid;
        //    }

        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        return _eqptid;
        //    }
        //}
        #endregion

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgProductLot.Columns.Contains("PLLT_BCD_ID"))
                    dgProductLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                if (dgAssyLot.Columns.Contains("PLLT_BCD_ID"))
                    dgAssyLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;

                Border _BCD1 = FindName("brdBCD_1") as Border;
                Border _BCD2 = FindName("brdBCD_2") as Border;

                if (_BCD1 != null)
                    _BCD1.Visibility = Visibility.Visible;
                if (_BCD2 != null)
                    _BCD2.Visibility = Visibility.Visible;
            }
            else
            {
                if (dgProductLot.Columns.Contains("PLLT_BCD_ID"))
                    dgProductLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                if (dgAssyLot.Columns.Contains("PLLT_BCD_ID"))
                    dgAssyLot.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;

                Border _BCD1 = FindName("brdBCD_1") as Border;
                Border _BCD2 = FindName("brdBCD_2") as Border;

                if (_BCD1 != null)
                    _BCD1.Visibility = Visibility.Collapsed;
                if (_BCD2 != null)
                    _BCD2.Visibility = Visibility.Collapsed;
            }
        }
    }
}
