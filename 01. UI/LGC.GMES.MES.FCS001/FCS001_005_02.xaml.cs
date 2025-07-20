/*************************************************************************************
 Created Date : 2020.11.16
      Creator : Dooly
   Decription : Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.13  DEVELOPER : Initial Created.
  2022.07.15  조영대 : 목록 체크박스 선택 및 해제시 같은 BOX ID 동일 처리.
  2022.08.16  이정미 : 강제출고 알람 정보 수정
  2022.08.31  이정미 : Aging 공정만 Sample 출고 가능하도록 변경  
  2022.08.30  김진섭 : TRAYID 엑셀파일로 업로드하는 기능 추가
  2022.09.13  이정미 : Aging 공정 대기 상태인 경우 Sample 출고 불가
  2022.09.16  김진섭 : 엑셀 업로드 버튼 위치 변경
  2022.10.17  조영대 : Aging 공정 대기 상태인 경우 Sample 출고 불가
  2022.10.18  강동희 : Rack간 이동/이동해제 기능 추가
  2022.12.14  조영대 : 인덱스 오류 수정
  2023.01.05  조영대 : Stocker 이동해제 버튼 숨김 처리
  2023.02.13  주훈종 : [E20230213-000118] 특별관리등록 기존 200개 제한에서 1000개 처리로 변경, 200개씩 나눠서 비즈룰 호출하는 방식으로 변경
  2023.02.20  조영대 : 재공정보현황(차기공정지연) 호출 추가 - 선택 컬럼 구분 인수 추가
                       데이터 조회 백그라운드 처리로 더블클릭 후 창 오픈시간 단축(오픈 후 인디케이터 처리)
  2023.04.06  권혜정 : ROUTE 변경 시, 생산라인/MODEL 자동 선택 및 비활성화
  2023.04.07  조영대 : Sorting 및 스크롤시 컬럼너비 확장으로 인한 현상 수정
  2023.04.26  홍석원 : Hold 제외 기능 추가
  2023 05 15  임근영 : [E20230513-001852] 컬럼너비 고정 
  2023.05.15  이해령 : 합계 조건으로 조회시에도 Aging 공정중이라면 Stocker 이동 가능하도록 수정
  2023.06.08  이지은 : Aging 대기상태일때 강제출고 되지 않도록 수정(WA2동 GMES 역전개 오류사항 대응)
  2023.06.09  최도훈 : UI버튼 누른 사용자 TB_SFC_UI_EVENT_HIST에 이력 남기도록 수정 (강제출고, Sample 출고/출고해제, Stocker 이동/이동해제)
  2023.06.20  이지은 : 재공정보현황(Lot별) 종료 후 대기상테일때에도 강제출고 되지 않도록 수정(WA2동 GMES 역전개 오류사항 대응)
  2023.08.07  최도훈 : Y등급 정렬 순서 수정(생산팀 요청)
  2023.08.15  손동혁 : 재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일, 상 / 하층레인 추가
  2023.09.07  최도훈 : Aging 공정이 아닌 경우 출고버튼 3개(강제출고, Sample 출고, Sample 출고해제) 비활성화
  2023.09.11  손동혁 : Sample 출고 일때 보라색 표현 처리 안되어서 수정
  2023.10.12  김수용 : 보정dOCV 미수신 Row 색상처리, 보정dOCV 항목 재전송(btnDocvReTrans_Click) 기능추가
  2023.10.17  배준호 : 강제출고 시 UI PROCID와 Tray PROCID 확인(시점 차이로 발생하는 오류대응)
  2023.10.31  임근영 : CT Sample 출고/해제 버튼 추가.
  2023.10.31  이의철 : 강제출고 취소 버튼 추가
  2023.11.17  조영대 : 재공정보조회(Lot별) LOT 8자리, 10자리구분 수정 연관
  2023.11.27  이의철 : RACK DEEP AND FRONT 구분 추가
  2023.11.30  이의철 : 로딩시 GetList() 위치 맨끝으로 이동
  2023.12.10  손동혁 : ESNA 특화 샘플출고 , 강제출고 시 비정상 랙 인터락 팝업창 추가
  2023.01.08  이지은 : Route 변경시 반송상태인 Tray들은 변경하지 못하도록 Validation 추가
  2024.01.17  이지은 : RTD물류동에서 샘플출고 취소시 반송 예약이 걸려있으면 취소하지 못하도록 Validation 추가 
  2024.04.22  조영대 : CDC Cell Hold 존재시 배경 표시
  2024.05.03  임정훈 : 특별관리등록 버튼 특별관리로 변경 및 클릭 시 팝업창 메시지 변경
  2024.09.10  복현수 : IRS Lock 여부 (IRS 미검사 보류) 조회조건 추가, 버튼 표시 조건 변경 (동별공통코드 : FORM_IRS_DFCT_EM_JUDGE_MGMT, 공통코드 : E - ATTR4 값이 존재할 때만 표시)
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Configuration;
using System.IO;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_005_02 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _sOPER = string.Empty;
        private string _sOPER_NAME = string.Empty;
        private string _sLINE_ID = string.Empty;
        private string _sLINE_NAME = string.Empty;
        private string _sROUTE_ID = string.Empty;
        private string _sROUTE_NAME = string.Empty;
        private string _sMODEL_ID = string.Empty;
        private string _sMODEL_NAME = string.Empty;
        private string _sROUTE_TYPE_DG = string.Empty;
        private string _sROUTE_TYPE_DG_NAME = string.Empty;
        private string _sStatus = string.Empty;
        private string _sStatusName = string.Empty;
        private string _sSPECIAL_YN = string.Empty;
        private string _sSpecialName = string.Empty;
        private string _sLOT_ID = string.Empty;
        private string _sAgingEqpID = string.Empty;
        private string _sAgingEqpName = string.Empty; //추가 02.17 
        private string _sOpPlanTime = string.Empty;
        private string _sLotType = string.Empty;
        private string _sLotTypeName = string.Empty;
        private string _sNextOpType = string.Empty; // 23.02.16

        private string _sExceptHold = string.Empty; // 23.04.19

        private string _sIRS_Lock = string.Empty; // 24.08.23

        private string _sOverTime = string.Empty;    ///2023.08.15 
        private string _sFloorLane = string.Empty;    ///2023.08.15 

        private bool checkEvent = true;
        private bool isNoSampleOut = false;
        private bool isNoForceOut = false;
        private string _sProcGrCode = string.Empty;
        bool bUseFlag = false; //20221018_Rack간 이동/이동해제 기능 추가

        //CT추가
        private string ctRetval = string.Empty;
        private string CtSampleInTray = string.Empty;
        private string _ctTrayID = string.Empty;
        private string ctIn_YN = string.Empty;
        private string _ctExTray = string.Empty;

        private string _dayGrLotId = string.Empty;

        //강제출고 취소 버튼 추가
        private bool bFCS001_005_02_FORCEOUT_CANCEL = false;

        //RACK DEEP AND FRONT 구분 추가
        private bool bFCS001_005_02_DEEP_FRONT = false;

        //CDC Cell Hold 존재시 배경 표시 사용 여부
        private bool bFCS001_005_02_CDC_HOLD_BACKGROUND_USE = false;

        bool cUseFlag = false; //샘플 출고 및 강제 출고 시 랙 상태가 비정상이면 해당 기능 인터락 추가

        public FCS001_005_02()
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

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;

            _sOPER = tmps[0] as string;
            _sOPER_NAME = tmps[1] as string;
            _sLINE_ID = tmps[2] as string;
            _sLINE_NAME = tmps[3] as string;
            _sROUTE_ID = tmps[4] as string;
            _sROUTE_NAME = tmps[5] as string;
            _sMODEL_ID = tmps[6] as string;
            _sMODEL_NAME = tmps[7] as string;
            _sStatus = tmps[8] as string;
            _sStatusName = tmps[9] as string;
            _sROUTE_TYPE_DG = tmps[10] as string;
            _sROUTE_TYPE_DG_NAME = tmps[11] as string;
            _sLOT_ID = tmps[12] as string;
            _sSPECIAL_YN = tmps[13] as string;
            _sAgingEqpID = tmps[14] as string; //추가 02-17
            if (tmps.Length > 15) //추가 02-17
            {
                _sAgingEqpName = tmps[15] as string;
                _sOpPlanTime = tmps[16] as string;
            }
            _sLotType = tmps[17] as string;
            _sLotTypeName = tmps[18] as string;
            if (tmps.Length > 19) //23.02.16
            {
                _sNextOpType = tmps[19] as string;
            }

            if (tmps.Length > 20) //23.02.16
            {
                _sExceptHold = tmps[20] as string;
            }

            if (tmps.Length > 22) // 2023.08.15
            {
                _sOverTime = tmps[21] as string;  /// 2023.08.15
                _sFloorLane = tmps[22] as string; /// 2023.08.15
            }

            if (tmps.Length > 23)
            {
                _dayGrLotId = tmps[23] as string;
            }

            InitText();

            

            dgTrayList.Columns["G_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["O_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["R_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["Q_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["I_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["L_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["X_GRD_QTY"].Visibility = Visibility.Collapsed;

            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_UnChecked;

            //RACK DEEP AND FRONT 구분 추가
            bFCS001_005_02_DEEP_FRONT = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_005_02_DEEP_FRONT");
            if(bFCS001_005_02_DEEP_FRONT.Equals(false))
            {
                dgTrayList.Columns["DBLRCHSTK_DEEP_RACK_FLAG"].Visibility = Visibility.Collapsed;
            }

            //강제출고 취소 버튼 추가
            bFCS001_005_02_FORCEOUT_CANCEL = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_005_02_FORCEOUT_CANCEL");

            // 20221018_Rack간 이동/이동해제 기능 추가 START
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_005_02"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                btnRackMove.Visibility = Visibility.Visible;
                btnRackRel.Visibility = Visibility.Visible;
            }
            else
            {
                btnRackMove.Visibility = Visibility.Collapsed;
                btnRackRel.Visibility = Visibility.Collapsed;
            }

            cUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "CSTID_ABNORM_RACK_INTEROCK"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            //CDC Cell Hold 존재시 배경 표시 사용 여부
            bFCS001_005_02_CDC_HOLD_BACKGROUND_USE = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_005_02_CDC_HOLD_BACKGROUND_USE");
            if (bFCS001_005_02_CDC_HOLD_BACKGROUND_USE)
            {
                rowBottomArea.Height = new GridLength(1, GridUnitType.Auto);
                btnBackground0.Visibility = Visibility.Visible;
            }
            else
            {
                rowBottomArea.Height = new GridLength(0, GridUnitType.Pixel);
                btnBackground0.Visibility = Visibility.Collapsed;
            }

            //IRS 미적용 사이트일 경우 조회조건 체크박스, 색인 숨김처리
            DataTable dtTemp = _Util.GetAreaCommonCodeUse("FORM_IRS_DFCT_EM_JUDGE_MGMT", "E");

            if (dtTemp != null && dtTemp.Rows.Count > 0)
            {
                if (string.IsNullOrEmpty(dtTemp.Rows[0]["ATTR4"].ToString()))
                {
                    chkIRS_Lock.Visibility = Visibility.Collapsed;
                    btn5.Visibility = Visibility.Collapsed;
                }
            }

            // 2023.01.05 : Stocker 이동해제 버튼 숨김 처리
            btnRackRel.Visibility = Visibility.Collapsed;

            // CT 검사기 공정 사용 유무 추가. 동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.                                                                                    
            if (!_Util.IsAreaCommonCodeUse("FORM_CT_SMPL_YN", "USE_YN")) 
            {
                btnCtSampleOUT.Visibility = Visibility.Hidden;
                btnCtSampleRel.Visibility = Visibility.Hidden;
            }

            _sProcGrCode = GetProcessGrpCode(_sOPER);

            if (_sProcGrCode.Equals("7"))
            {
                btnRackMove.IsEnabled = true;
                btnRackRel.IsEnabled = true;
            }
            else
            {
                btnRackMove.IsEnabled = false;
                btnRackRel.IsEnabled = false;
            }

            // 2023.09.07 : Aging 공정이 아닌 경우 출고버튼 3개(강제출고, Sample 출고, Sample 출고해제) 비활성화
            if (_sProcGrCode.Equals("3") ||        // Normal Temp.
                _sProcGrCode.Equals("4") ||        // High Temp.
                _sProcGrCode.Equals("7") ||        // Shipping
                _sProcGrCode.Equals("9"))          // Pre
            {
                btnForceOut.IsEnabled = true;
                btnSampleOut.IsEnabled = true;
                btnSampleRel.IsEnabled = true;
                
                //강제출고 취소 버튼 추가
                if (bFCS001_005_02_FORCEOUT_CANCEL.Equals(true))
                {
                    btnForceOutCancel.IsEnabled = true;
                }
            }
            else
            {
                btnForceOut.IsEnabled = false;
                btnSampleOut.IsEnabled = false;
                btnSampleRel.IsEnabled = false;
                btnCtSampleOUT.IsEnabled = false;  //CT 버튼 추가
                btnCtSampleRel.IsEnabled = false;

                //강제출고 취소 버튼 추가
                if (bFCS001_005_02_FORCEOUT_CANCEL.Equals(true))
                {
                    btnForceOutCancel.IsEnabled = false;
                }
            }

            //CT 출고 버튼 추가 
            if (_sProcGrCode.Equals("3") ||        // Normal Temp.
                _sProcGrCode.Equals("7") ||        // Shipping
                _sProcGrCode.Equals("9"))          // Pre
            {
                btnCtSampleOUT.IsEnabled = true;
                btnCtSampleRel.IsEnabled = true;
            }
            else
            {
                btnCtSampleOUT.IsEnabled = false;  
                btnCtSampleRel.IsEnabled = false;
            }

            // 20221018_Rack간 이동/이동해제 기능 추가 END

            if (_sExceptHold == "Y")
            {
                chkHold.IsChecked = true;
            }
            else
            {
                chkHold.IsChecked = false;
            }

            string[] sColumnName = new string[] { "EQP_ID" };
            _Util.SetDataGridMergeExtensionCol(dgTrayList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            //강제출고 취소 버튼 추가
            if(bFCS001_005_02_FORCEOUT_CANCEL.Equals(true))
            {
                this.btnForceOutCancel.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnForceOutCancel.Visibility = Visibility.Collapsed;
            }

            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Index == 0) return;
                    if (e.Cell.Row.DataItem == null) return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;

                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DUMMY_FLAG")).ToString().Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE")).ToString().Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE")).ToString().Equals("P"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    // if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).ToString().Equals("SAMPLE")) ||
                    //  (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).ToString().Equals("SAMPLE_SHIP")))

                    if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).ToString().Equals("SAMPLE"))
                    || (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).Equals(ObjectDic.Instance.GetObjectName("SAMPLE_SHIP"))))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.DarkViolet);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DOCV_RCV_FLAG")).ToString().Equals("N"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    if (bFCS001_005_02_CDC_HOLD_BACKGROUND_USE && e.Cell.Column.Index > dgTrayList.Columns["EQP_ID"].Index &&
                        Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CDC_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = Brushes.SkyBlue;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "IRS_LOCK")).ToString().Equals("Y"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Magenta);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    //Column Width Set
                    if (e.Cell.Column.Name.Equals("A_GRD_QTY") || e.Cell.Column.Name.Equals("B_GRD_QTY") || e.Cell.Column.Name.Equals("C_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("D_GRD_QTY") || e.Cell.Column.Name.Equals("E_GRD_QTY") || e.Cell.Column.Name.Equals("F_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("G_GRD_QTY") || e.Cell.Column.Name.Equals("H_GRD_QTY") || e.Cell.Column.Name.Equals("I_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("J_GRD_QTY") || e.Cell.Column.Name.Equals("K_GRD_QTY") || e.Cell.Column.Name.Equals("L_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("M_GRD_QTY") || e.Cell.Column.Name.Equals("N_GRD_QTY") || e.Cell.Column.Name.Equals("O_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("P_GRD_QTY") || e.Cell.Column.Name.Equals("Q_GRD_QTY") || e.Cell.Column.Name.Equals("R_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("S_GRD_QTY") || e.Cell.Column.Name.Equals("T_GRD_QTY") || e.Cell.Column.Name.Equals("U_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("V_GRD_QTY") || e.Cell.Column.Name.Equals("W_GRD_QTY") || e.Cell.Column.Name.Equals("X_GRD_QTY") ||
                        e.Cell.Column.Name.Equals("Y_GRD_QTY") || e.Cell.Column.Name.Equals("Z_GRD_QTY") || e.Cell.Column.Name.Equals("WIP_QTY") ||
                        e.Cell.Column.Name.Equals("INPUT_SUBLOT_QTY"))
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(35);
                    }
                    //Column Size  넓어지는 현상 임시조치
                    string LangID = LoginInfo.LANGID;

                    if (LangID.Equals("ko-KR") || LangID.Equals("zh-CN"))
                    {
                        if (e.Cell.Column.Name.Equals("WIPDTTM_ED") || e.Cell.Column.Name.Equals("WIPDTTM_ST") || e.Cell.Column.Name.Equals("OP_PLAN_TIME") || e.Cell.Column.Name.Equals("ED_PLAN_TIME") ||
                            e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("OP_NAME") || e.Cell.Column.Name.Equals("PROCID") || e.Cell.Column.Name.Equals("NEXT_OP_NAME"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                        }

                        if (e.Cell.Column.Name.Equals("PROD_LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                        }

                        if (e.Cell.Column.Name.Equals("ROUTID") || e.Cell.Column.Name.Equals("OUT_TYPE"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(75);
                        }

                        if (e.Cell.Column.Name.Equals("DUMMY_FLAG") || e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("LOT_TYPE"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(85);
                        }

                        else
                        {

                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                        }
                    }
                    else
                    {
                        if (e.Cell.Column.Name.Equals("WIPDTTM_ED") || e.Cell.Column.Name.Equals("WIPDTTM_ST") || e.Cell.Column.Name.Equals("OP_PLAN_TIME") || e.Cell.Column.Name.Equals("ED_PLAN_TIME") ||
                           e.Cell.Column.Name.Equals("LOT_TYPE") || e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("DUMMY_FLAG") ||
                           e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("OP_NAME") || e.Cell.Column.Name.Equals("PROCID") || e.Cell.Column.Name.Equals("NEXT_OP_NAME"))

                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                        }

                        if (e.Cell.Column.Name.Equals("PROD_LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                        }

                        if (e.Cell.Column.Name.Equals("ROUTID"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(120);
                        }

                        else
                        {

                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                        }

                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
            string sTrayList = string.Empty;


            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    sTrayList += DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID") + "|";
                }
            }

            if (sTrayList.Length == 0)
            {
                //Tray를 선택해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0081"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    { }
                });
                return;
            }

            Util.MessageConfirm("SFU9219", (result) =>  //특별관리 등록 시 단 적재 된 Tray도 같이 등록 됩니다. 
            {
                if (result == MessageBoxResult.OK)
                {
                    LGC.GMES.MES.FCS001.FCS001_021_SPECIAL_MANAGEMENT wndPopup = new LGC.GMES.MES.FCS001.FCS001_021_SPECIAL_MANAGEMENT();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[2];

                        Parameters[0] = sTrayList.Substring(0, sTrayList.Length - 1);
                        Parameters[1] = "Y";

                        C1WindowExtension.SetParameters(wndPopup, Parameters);
                        wndPopup.Closed += new EventHandler(OnCloseRegDefectLane);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                else { return; }
            });
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            LGC.GMES.MES.FCS001.FCS001_021_SPECIAL_MANAGEMENT window = sender as LGC.GMES.MES.FCS001.FCS001_021_SPECIAL_MANAGEMENT;
            //if (window.DialogResult == MessageBoxResult.OK)
            if (window.sResultReturn == "Y") //E20230213-000118
                GetList();
        }

        private void btnRoute_Click(object sender, RoutedEventArgs e)
        {
            int iChkCnt = 0;
            string sLotID = string.Empty;
            string sCstID = string.Empty;
            string[] sLineReturn = new string[3];
            string[] sMhsTrfReturn = new string[2];

            try
            {
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        iChkCnt++;
                    }
                }

                if (iChkCnt == 0)
                {
                    //선택된 데이터가 없습니다.
                    Util.MessageInfo("FM_ME_0165");
                    return;
                }

                //Check 된 Tray와 같은 Rack에 들어있는 Tray를\r\n모두Check 하여 Route 변경을 진행하시겠습니까?
                Util.MessageConfirm("FM_ME_0027", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
                        for (int i = 0; i < dgTrayList.Rows.Count; i++)
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                                 Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                            {
                                alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                            }
                        }

                        for (int i = 0; i < dgTrayList.Rows.Count; i++)
                        {
                            if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                            {
                                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                            }
                        }

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("BF_ROUTE_ID", typeof(string));
                        dtRqst.Columns.Add("MDF_ID", typeof(string));

                        for (int i = 0; i < dgTrayList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                if (!((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3")) ||
                                      (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("4")) ||
                                      (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7")) ||
                                      (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9"))))
                                {
                                    //Aging 공정이 아닌 Tray가 있습니다.
                                    Util.MessageInfo("FM_ME_0015");
                                    return;
                                }

                                DataRow dr = dtRqst.NewRow();
                                dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).ToString();
                                dr["BF_ROUTE_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "ROUTID")).ToString();
                                dr["MDF_ID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);

                                sLotID += "," + Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID")).ToString();
                                sCstID += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).ToString() + "," ;
                            }
                        }

                        sLineReturn = GetLineAndModel(sLotID);
                        sMhsTrfReturn = GetMhsTrfCmd(sCstID);

                        if (sMhsTrfReturn[0].ToString().Equals("N"))
                        {
                            //반송중인 Tray가 있습니다.[%1]
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("59078", sMhsTrfReturn[1].ToString().Trim()), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                            return;
                        }

                        if (sLineReturn[0] == "1")
                        {
                            FCS001_005_ROUTE_CHG wndConfirm = new FCS001_005_ROUTE_CHG();
                            wndConfirm.FrameOperation = FrameOperation;

                            if (wndConfirm != null)
                            {
                                wndConfirm.TrayList = dtRqst;
                                wndConfirm.LineModel = sLineReturn;

                                grdMain.Children.Add(wndConfirm);
                                wndConfirm.BringToFront();
                            }
                        }
                        else
                        {
                            // Line과 Model이 동일하지 않습니다.
                            Util.MessageInfo("FM_ME_0480");
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnConveyorOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));

                DataRow drIn = dtInData.NewRow();
                drIn["USERID"] = LoginInfo.USERID;
                dtInData.Rows.Add(drIn);

                DataTable dtInLot = ds.Tables.Add("INLOT");
                dtInLot.Columns.Add("CSTID", typeof(string));
                dtInLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = dtInLot.NewRow();
                        dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID"));
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtInLot.Rows.Add(dr);
                    }
                }

                if (dtInLot.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //선택한 데이터를 삭제하시겠습니까?
                Util.MessageConfirm("FM_ME_0167", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_DELETE_MULTI", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //삭제되었습니다.
                                Util.MessageInfo("SFU1273");
                                GetList();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnForceOut_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //ESNA 1동 요청 트레이가 샘플출고 및 강제출고 시 비정상 상태인 렉이면 인터락
                if (cUseFlag)
                {
                    if (CheckAbnormRack())
                    {
                        return;
                    }

                }


                //RACK DEEP AND FRONT 구분 추가
                if (bFCS001_005_02_DEEP_FRONT.Equals(true))
                {
                    SetForceOut_All();
                }
                else
                {
                    isNoForceOut = false;

                    DataSet ds = new DataSet();
                    DataTable dtInData = ds.Tables.Add("INDATA");
                    dtInData.Columns.Add("USERID", typeof(string));
                    dtInData.Columns.Add("AREAID", typeof(string));
                    dtInData.Columns.Add("MENUID", typeof(string));
                    dtInData.Columns.Add("USER_IP", typeof(string));
                    dtInData.Columns.Add("PC_NAME", typeof(string));


                    DataRow drIn = dtInData.NewRow();
                    drIn["USERID"] = LoginInfo.USERID;
                    drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drIn["MENUID"] = LoginInfo.CFG_MENUID;
                    drIn["USER_IP"] = LoginInfo.USER_IP;
                    drIn["PC_NAME"] = LoginInfo.PC_NAME;

                    dtInData.Rows.Add(drIn);

                    DataTable dtInLot = ds.Tables.Add("INLOT");
                    dtInLot.Columns.Add("LOTID", typeof(string));
                    dtInLot.Columns.Add("PROCID", typeof(string));

                    string sMessage = "";
                    for (int i = 0; i < dgTrayList.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            DataRow dr = dtInLot.NewRow();
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                            dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROCID"));
                            dtInLot.Rows.Add(dr);

                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME"))))
                            {
                                //출고예정시간이 없는 경우 강제 출고가 불가합니다. 강제출고요청을 하시겠습니까?
                                sMessage = "FM_ME_0441";

                                if ((txtStatus.Text.Equals("WAIT") || _sStatus.Equals("E")) &&
                                    (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3") ||
                                     Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("4") ||
                                     Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7") ||
                                     Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9")))
                                {
                                    isNoForceOut = true;
                                }
                            }
                            else if (!(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")))) && (Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")).ToString()) - DateTime.Now).TotalSeconds > 0)
                            {
                                // 출고예정시간 미달인 경우 강제 출고가 불가합니다. 강제출고요청을 하시겠습니까?
                                sMessage = "FM_ME_0442";
                            }

                            else
                            {
                                // 강제출고요청을 하시겠습니까?
                                sMessage = "FM_ME_0094";
                            }
                        }
                    }
                    if (dtInLot.Rows.Count == 0)
                    {
                        //선택된 데이터가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            { }
                        });
                        return;
                    }

                    //2023.06.08 Aging 대기상태(WAIT)이고 출고예정시간(OP_PLAN_TIME)이 없으면 강제출고 하지 못하도록 변경
                    if (isNoForceOut)
                    {
                        //Lot이 착공 상태가 아니므로 강제출고 할 수 없습니다.
                        Util.Alert("FM_ME_0492");
                        return;
                    }

                    //RACK DEEP AND FRONT 구분 추가
                    //강제 출고시 Deep and Front 표시 및 시간 업데이트 - Front 

                    Util.MessageConfirm(sMessage, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_MULTI", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    //강제출고 요청을 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0092");
                                    GetList();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }
                            }, ds);

                        }
                    });
                }

                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CheckAbnormRack()
        {
            bool result = false;
            DataSet ds = new DataSet();
            DataTable dtInData = ds.Tables.Add("INDATA");
            dtInData.Columns.Add("CSTID", typeof(string));


            DataRow drIn = dtInData.NewRow();
            drIn["CSTID"] = LoginInfo.USERID;

            dtInData.Rows.Add(drIn);

            DataTable dtInCst = ds.Tables.Add("INCST");
            dtInCst.Columns.Add("CSTID", typeof(string));

            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                {

                    DataRow dr = dtInCst.NewRow();
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID"));
                    dtInCst.Rows.Add(dr);

                }
            }

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TROUBLE_RACK_STAT", "RQSTDT", "RSLTDT", dtInCst);
            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
            {
                Util.Alert("SFU9051", dtRslt.Rows[0]["CSTID"].ToString());  //해당 %1의 랙의 상태는 비정상 상태이므로 해당 작업을 할 수 없습니다.
                result = true;
                return result;
            }

            return result;

        }

        private void SetForceOut_All()
        {
            int nFour = 0;
            int nTree = 0;
            int nTwo = 0;
            int nOne = 0;
            int nDeepFront = 0;
            int ncount = 0;
            string sMessage = "";

            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("4"))
                    {

                        nFour++;
                    }

                    else if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("3"))
                    {
                        nTree++;
                    }

                    else if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("2"))
                    {
                        nTwo++;
                    }

                    else if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("1"))
                    {
                        nOne++;
                    }
                    else
                    {
                        nDeepFront++;
                    }



                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME"))))
                    {
                        //출고예정시간이 없는 경우 강제 출고가 불가합니다. 강제출고요청을 하시겠습니까?
                        sMessage = "FM_ME_0441";

                        if ((txtStatus.Text.Equals("WAIT") || _sStatus.Equals("E")) &&
                            (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3") ||
                                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("4") ||
                                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7") ||
                                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9")))
                        {
                            isNoForceOut = true;
                        }
                    }
                    else if (!(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")))) && (Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")).ToString()) - DateTime.Now).TotalSeconds > 0)
                    {
                        // 출고예정시간 미달인 경우 강제 출고가 불가합니다. 강제출고요청을 하시겠습니까?
                        sMessage = "FM_ME_0442";
                    }

                    else
                    {
                        // 강제출고요청을 하시겠습니까?
                        sMessage = "FM_ME_0094";
                    }

                    ncount++;
                }
            }

            if (ncount == 0)
            {
                //선택된 데이터가 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    { }
                });
                return;
            }

            //2023.06.08 Aging 대기상태(WAIT)이고 출고예정시간(OP_PLAN_TIME)이 없으면 강제출고 하지 못하도록 변경
            if (isNoForceOut)
            {
                //Lot이 착공 상태가 아니므로 강제출고 할 수 없습니다.
                Util.Alert("FM_ME_0492");
                return;
            }

            Util.MessageConfirm(sMessage, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    
                    if (nFour > 0)
                    {
                        SetForceOut_No("4");
                    }
                    if (nTree > 0)
                    {
                        SetForceOut_No("3");
                    }
                    if (nTwo > 0)
                    {
                        SetForceOut_No("2");
                    }
                    if (nOne > 0)
                    {
                        SetForceOut_No("1");
                    }
                    if (nFour > 0)
                    {
                        SetForceOut_DeepFront();
                    }



                    GetList();
                }
            });
            
        }

        private void SetForceOut_No(String no)
        {
            try
            {
                isNoForceOut = false;

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));
 

                DataRow drIn = dtInData.NewRow();
                drIn["USERID"] = LoginInfo.USERID;
                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                drIn["MENUID"] = LoginInfo.CFG_MENUID;
                drIn["USER_IP"] = LoginInfo.USER_IP;
                drIn["PC_NAME"] = LoginInfo.PC_NAME;

                dtInData.Rows.Add(drIn);

                DataTable dtInLot = ds.Tables.Add("INLOT");
                dtInLot.Columns.Add("LOTID", typeof(string));
                dtInLot.Columns.Add("PROCID", typeof(string));
                                
                string sMessage = "";
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals(no))
                        {

                            DataRow dr = dtInLot.NewRow();
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                            dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROCID"));
                            dtInLot.Rows.Add(dr);
                            
                        }                        
                    } 
                }
                if (dtInLot.Rows.Count == 0)
                {
                    return;
                }

                //RACK DEEP AND FRONT 구분 추가
                //강제 출고시 Deep and Front 표시 및 시간 업데이트 - Front                 
                new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_MULTI", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //강제출고 요청을 완료하였습니다.
                        //Util.MessageInfo("FM_ME_0092");                                                              
                        //GetList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);

                    
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetForceOut_DeepFront()
        {
            try
            {
                isNoForceOut = false;

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));

                DataRow drIn = dtInData.NewRow();
                drIn["USERID"] = LoginInfo.USERID;
                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                drIn["MENUID"] = LoginInfo.CFG_MENUID;
                drIn["USER_IP"] = LoginInfo.USER_IP;
                drIn["PC_NAME"] = LoginInfo.PC_NAME;

                dtInData.Rows.Add(drIn);

                DataTable dtInLot = ds.Tables.Add("INLOT");
                dtInLot.Columns.Add("LOTID", typeof(string));
                dtInLot.Columns.Add("PROCID", typeof(string));

                string sMessage = "";
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("4") 
                            || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("3")
                            || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("2")
                            || Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DBLRCHSTK_DEEP_FRONT")).Equals("1"))
                        {
                            DataRow dr = dtInLot.NewRow();
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                            dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROCID"));
                            dtInLot.Rows.Add(dr);                            
                        }
                    }
                }
                if (dtInLot.Rows.Count == 0)
                {
                    return;
                }
                              

               
                new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_MULTI", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //강제출고 요청을 완료하였습니다.
                        //Util.MessageInfo("FM_ME_0092");
                        //GetList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);

                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnForceOutCancel_Click(object sender, RoutedEventArgs e)
        {
            //강제출고 취소 버튼 추가
            try
            {
                isNoForceOut = false;

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));


                DataRow drIn = dtInData.NewRow();
                drIn["USERID"] = LoginInfo.USERID;
                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                drIn["MENUID"] = LoginInfo.CFG_MENUID;
                drIn["USER_IP"] = LoginInfo.USER_IP;
                drIn["PC_NAME"] = LoginInfo.PC_NAME;

                dtInData.Rows.Add(drIn);

                DataTable dtInLot = ds.Tables.Add("INLOT");
                dtInLot.Columns.Add("LOTID", typeof(string));
                dtInLot.Columns.Add("PROCID", typeof(string));

                string sMessage = "";
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = dtInLot.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROCID"));
                        dtInLot.Rows.Add(dr);
                    }
                }
                if (dtInLot.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                // 강제출고 취소를 하시겠습니까?
                sMessage = "FM_ME_0461";

                Util.MessageConfirm(sMessage, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_CANCEL_MULTI", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //강제출고 취소를 완료하였습니다.
                                Util.MessageInfo("FM_ME_0462");
                                GetList();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSampleOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cUseFlag)
                {
                    //ESNA 1동 요청 트레이가 샘플출고 및 강제출고 시 비정상 상태인 렉이면 인터락
                    if (CheckAbnormRack())
                    {
                        return;
                    }

                }

                isNoSampleOut = false;

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));

                //DataRow drIn = dtInData.NewRow();
                //drIn["USERID"] = LoginInfo.USERID;


                //DataTable dtInLot = ds.Tables.Add("INLOT");
                //dtInLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        drIn["MENUID"] = LoginInfo.CFG_MENUID;
                        drIn["USER_IP"] = LoginInfo.USER_IP;
                        drIn["PC_NAME"] = LoginInfo.PC_NAME;
                        dtInData.Rows.Add(drIn);

                        if ((txtStatus.Text.Equals("WAIT") || _sStatus.Equals("E")) &&
                            (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9")))
                        {
                            isNoSampleOut = true;
                        }
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                if (isNoSampleOut)
                {
                    //Aging 공정 대기 상태인 Tray는 Sample 출고가 불가합니다. 
                    Util.Alert("FM_ME_0445");
                    return;
                }

                CtSampleOut_YN(); //CT 샘플출고 여부 확인 

                if (ctRetval == "CT")
                {   //CT SAMPLE 추출중인 TRAY가 포함되어 있습니다.[TRAYID : %1] 
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0556", _ctExTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }
                if (ctRetval != "CT")
                {
                    //Sample 출고 하시겠습니까?
                    Util.MessageConfirm("FM_ME_0323", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_OUT", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                                    {
                                    //Sample 출고 지시를 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0065");
                                        GetList();
                                    }

                                    if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "-1")
                                    {
                                    //Sample 출고는 Aging 공정에서만 가능합니다. 
                                    Util.Alert("FM_ME_0444");
                                    }

                                    if (bizResult.Tables["OUT_SAMPLE_PORT"].Rows.Count > 0)
                                    {
                                    //POPUP
                                    FCS001_005_03 wndRunStart = new FCS001_005_03();
                                        wndRunStart.FrameOperation = FrameOperation;

                                        if (wndRunStart != null)
                                        {
                                            object[] Parameters = new object[1];
                                            Parameters[0] = result;

                                            C1WindowExtension.SetParameters(wndRunStart, Parameters);
                                        }
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
                            }, ds);

                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSampleRel_Click(object sender, RoutedEventArgs e)
        {
            string sCstID = string.Empty;
            string[] sMhsTrfReturn = new string[2];

            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));

                //DataRow drIn = dtInData.NewRow();
                //drIn["USERID"] = LoginInfo.USERID;

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        drIn["MENUID"] = LoginInfo.CFG_MENUID;
                        drIn["USER_IP"] = LoginInfo.USER_IP;
                        drIn["PC_NAME"] = LoginInfo.PC_NAME;
                        dtInData.Rows.Add(drIn);

                        sCstID += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).ToString() + ",";
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //2024.01.15 [RTD물류동] 반송중(TB_MHS_TRF_CMD 테이블에 데이터 조회)인 CSTID는 샘플 취소 못하도록 수정
                sMhsTrfReturn = GetMhsTrfCmd(sCstID);

                if (sMhsTrfReturn[0].ToString().Equals("N"))
                {
                    //반송중인 샘플링 트레이를 취소할 수 없습니다..[%CSTID]
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0604", sMhsTrfReturn[1].ToString().Trim()), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: false);
                    return;
                }

                CtSampleOut_YN(); //CT 출고 여부 확인
                if(ctRetval == "CT")
                {   //CT SAMPLE 추출중인 Tray가 포함되어 있습니다. [Trayid: %1]
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0556", _ctExTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                if (ctRetval == "SAMPLE")
                {
                    //Sample 해제하시겠습니까?
                    Util.MessageConfirm("FM_ME_0324", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_IN", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //Sample 해제를 완료하였습니다.
                                Util.MessageInfo("FM_ME_0067");
                                GetList();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtTrayID.Text.Trim() == string.Empty)
                    return;

                CheckTray(txtTrayID.Text.Trim());
                txtTrayID.SelectAll();
                txtTrayID.Focus();
            }
        }

        private void txtTrayID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string _ValueToFind = string.Empty;
                    bool bFlag = false;

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        for (int j = 0; j < dgTrayList.Rows.Count; j++)
                        {
                            if (sPasteStrings[i] == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CSTID")))
                            {
                                DataTableConverter.SetValue(dgTrayList.Rows[j].DataItem, "CHK", true);
                                bFlag = true;
                            }
                        }
                    }

                    if (!bFlag)
                    {
                        //해당 Tray ID가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0260"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTrayID.SelectAll();
                                txtTrayID.Focus();
                            }
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["G_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["O_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["R_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["Q_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["I_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["L_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["X_GRD_QTY"].Visibility = Visibility.Visible;

            dgTrayList.TopRowHeaderMerge();
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["G_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["O_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["R_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["Q_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["I_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["L_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["X_GRD_QTY"].Visibility = Visibility.Collapsed;
        }

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayList == null || dgTrayList.CurrentRow == null || !dgTrayList.Columns.Contains("CSTID") || !dgTrayList.Columns.Contains("ROUTID"))
                    return;

                FCS001_021 wndTRAY = new FCS001_021();
                wndTRAY.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);

                //if (dgTrayList.CurrentColumn.Name.Equals("CSTID"))
                //{
                //    FCS001_021 wndTRAY = new FCS001_021();
                //    wndTRAY.FrameOperation = FrameOperation;

                //    object[] Parameters = new object[10];
                //    Parameters[0] = DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID").ToString(); // TRAY ID
                //    Parameters[1] = DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID").ToString(); // LOTID

                //    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                //}

                if (dgTrayList.CurrentColumn.Name.Equals("ROUTID"))
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("TRAY_NO", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["TRAY_NO"] = DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID").ToString();
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LINE_MODEL_ROUTE_FROM_TRAY", "RQSTDT", "RSLTDT", dtRqst);

                    //Route 관리
                    //Util.OpenRouteForm(dtRslt.Rows[0]["LINE_ID"].ToString(), dtRslt.Rows[0]["MODEL_ID"].ToString(), dtRslt.Rows[0]["ROUTE_ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotID.Text.Trim() == string.Empty)
                        return;

                    if (chkCellLot.IsChecked == true)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                        dtRqst.Columns.Add("PROCID", typeof(string));
                        dtRqst.Columns.Add("EQSGID", typeof(string));
                        dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("ROUTE_TYPE_DG", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["PROD_LOTID"] = Util.GetCondition(txtLotID);
                        dr["PROCID"] = (string.IsNullOrEmpty(_sOPER) ? null : _sOPER);
                        dr["EQSGID"] = (string.IsNullOrEmpty(_sLINE_ID) ? null : _sLINE_ID);
                        dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sMODEL_ID) ? null : _sMODEL_ID);
                        dr["ROUTID"] = (string.IsNullOrEmpty(_sROUTE_ID) ? null : _sROUTE_ID);
                        dr["ROUTE_TYPE_DG"] = (string.IsNullOrEmpty(_sROUTE_TYPE_DG) ? null : _sROUTE_TYPE_DG);
                        dtRqst.Rows.Add(dr);

                        ShowLoadingIndicator();
                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_BY_CELL_LOT", "RQSTDT", "RSLTDT", dtRqst);

                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgTrayList.Rows.Count; j++)
                            {
                                if (SearchResult.Rows[i]["CSTID"].ToString() == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CSTID")))
                                {
                                    DataTableConverter.SetValue(dgTrayList.Rows[j].DataItem, "CHK", true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dgTrayList.Rows.Count; i++)
                        {
                            if (txtLotID.Text.Trim() == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROD_LOTID")))
                            {
                                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
            }
        }

        private void chkAll_UnChecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", false);
            }
        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            if (e.Row.Index + 1 - dgTrayList.TopRows.Count > 0 && e.Row.Index != dgTrayList.Rows.Count - 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = (e.Row.Index + 1 - dgTrayList.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")).Equals(string.Empty))
                return;

            if (checkEvent.Equals(true))
            {
                checkEvent = false;
                SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")), true);
                checkEvent = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")).Equals(string.Empty))
                return;

            if (checkEvent.Equals(true))
            {
                checkEvent = false;
                SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")), false);
                checkEvent = true;
            }
        }

        private void dgTrayList_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;

            // 기본 체크값이 없어 처리함
            if (dtResult != null)
            {
                dtResult.Columns.Add("CHK", typeof(bool));
                dtResult.Select().ToList<DataRow>().ForEach(row => row["CHK"] = false);
            }
        }

        private void dgTrayList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            gdConditionArea.IsEnabled = true;
        }

        private void chkHold_Checked(object sender, RoutedEventArgs e)
        {
            _sExceptHold = "Y";
        }

        private void chkHold_Unchecked(object sender, RoutedEventArgs e)
        {
            _sExceptHold = "N";
        }

        private void chkIRS_Lock_Checked(object sender, RoutedEventArgs e)
        {
            _sIRS_Lock = "Y";
        }

        private void chkIRS_Lock_Unchecked(object sender, RoutedEventArgs e)
        {
            _sIRS_Lock = null;
        }

        #endregion

        #region Method

        private void CheckTray(string sTray)
        {
            bool bFlag = false;
            try
            {
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (sTray == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                        bFlag = true;
                    }
                }

                if (!bFlag)
                {
                    //해당 Tray ID가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0260"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayID.SelectAll();
                            txtTrayID.Focus();
                        }
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetList()
        {
            try
            {
                dgTrayList.ClearRows();

                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROD_LOTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("ROUTID", typeof(string));
                dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));
                dt.Columns.Add("MDLLOT_ID", typeof(string));
                dt.Columns.Add("SPECIAL_YN", typeof(string));
                dt.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                dt.Columns.Add("AGING_EQP_ID", typeof(string));
                dt.Columns.Add("OP_PLAN_TIME", typeof(string));
                dt.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dt.Columns.Add("ABNORM_FLAG", typeof(string));
                dt.Columns.Add("LOTTYPE", typeof(string));
                dt.Columns.Add("NEXT_OP_TYPE", typeof(string));
                dt.Columns.Add("EXCEPT_HOLD", typeof(string));
                dt.Columns.Add("OVERTIME", typeof(string));
                dt.Columns.Add("FLOOR_LANE", typeof(string));
                dt.Columns.Add("DAY_GR_LOTID", typeof(string));

                //RACK DEEP AND FRONT 구분 추가
                if (bFCS001_005_02_DEEP_FRONT.Equals(true))
                {
                    dt.Columns.Add("DEEPFRONT_FLAG", typeof(string));
                }
                else
                {
                    dt.Columns.Add("DEEPFRONT_N_FLAG", typeof(string));
                }

                dt.Columns.Add("IRS_LOCK", typeof(string));
                
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROD_LOTID"] = (string.IsNullOrEmpty(_sLOT_ID) ? null : "%" + _sLOT_ID + "%");
                dr["PROCID"] = (string.IsNullOrEmpty(_sOPER) ? null : _sOPER);
                dr["EQSGID"] = (string.IsNullOrEmpty(_sLINE_ID) ? null : _sLINE_ID);
                dr["ROUTID"] = (string.IsNullOrEmpty(_sROUTE_ID) ? null : _sROUTE_ID);
                dr["TRAY_OP_STATUS_CD"] = (string.IsNullOrEmpty(_sStatus) ? null : _sStatus);
                dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sMODEL_ID) ? null : _sMODEL_ID);
                dr["SPECIAL_YN"] = (string.IsNullOrEmpty(_sSPECIAL_YN) ? null : _sSPECIAL_YN);
                dr["ROUTE_TYPE_DG"] = (string.IsNullOrEmpty(_sROUTE_TYPE_DG) ? null : _sROUTE_TYPE_DG);
                dr["AGING_EQP_ID"] = (string.IsNullOrEmpty(_sAgingEqpID) ? null : _sAgingEqpID);
                dr["OP_PLAN_TIME"] = (string.IsNullOrEmpty(_sOpPlanTime) ? null : _sOpPlanTime);
                dr["LOTTYPE"] = (string.IsNullOrEmpty(_sLotType) ? null : _sLotType);
                dr["NEXT_OP_TYPE"] = (string.IsNullOrEmpty(_sNextOpType) ? null : _sNextOpType);

                //dr["EXCEPT_HOLD"] = chkHold.IsChecked == true ? "Y" : "N";
                dr["EXCEPT_HOLD"] = (string.IsNullOrEmpty(_sExceptHold) ? null : _sExceptHold);

                ///재공정보현황(공정별) 화면에서 파라미터 전달받는 NA 특화 경과일, 상 / 하층레인 추가 1
                dr["OVERTIME"] = (string.IsNullOrEmpty(_sOverTime) ? null : _sOverTime);
                dr["FLOOR_LANE"] = (string.IsNullOrEmpty(_sFloorLane) ? null : _sFloorLane);
                //if (_sStatus == "A")
                //{
                //}
                //else if (_sStatus == "S")
                //{
                //    dr["WIPSTAT"] = "PROC";
                //}
                //else if (_sStatus == "E")
                //{
                //    dr["WIPSTAT"] = "WAIT";
                //}
                //else if (_sStatus == "P")
                //{
                //    dr["ISS_RSV_FLAG"] = "Y";
                //}
                //else if (_sStatus == "T")
                //{
                //    dr["ABNORM_FLAG"] = "Y";
                //}
                dr["DAY_GR_LOTID"] = string.IsNullOrEmpty(_dayGrLotId) ? null : _dayGrLotId;

                //RACK DEEP AND FRONT 구분 추가
                if (bFCS001_005_02_DEEP_FRONT.Equals(true))
                {
                    dr["DEEPFRONT_FLAG"] = "Y";
                }
                else
                {
                    dr["DEEPFRONT_N_FLAG"] = "Y";
                }

                dr["IRS_LOCK"] = (string.IsNullOrEmpty(_sIRS_Lock) ? null : _sIRS_Lock);

                dt.Rows.Add(dr);

                gdConditionArea.IsEnabled = false;

                // Background 처리 완료시 dgTrayList_ExecuteDataCompleted 이벤트 호출
                dgTrayList.ExecuteService("BR_GET_TRAY_LIST", "INDATA", "OUTDATA", dt, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetBoxIdCheck(string boxId, bool isCheck)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).Equals(boxId))
                {
                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", isCheck);
                }
            }
        }

        private void InitText()
        {
            txtLine.Text = _sLINE_NAME;
            txtModel.Text = _sMODEL_NAME;
            txtOp.Text = _sOPER_NAME;
            txtRoute.Text = _sROUTE_NAME;
            txtSpecialYN.Text = _sSPECIAL_YN;
            txtStatus.Text = _sStatusName;
            txtAgingEqpName.Text = _sAgingEqpID;
            txtLotID.Text = _sLOT_ID;
            txtLotType.Text = _sLotTypeName;
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

        #endregion

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
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
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("CSTID", typeof(string));
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CST ID;
                            if (sheet.GetCell(rowInx, 0) == null)
                                continue;

                            string CSTID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            DataRow dataRow = dataTable.NewRow();
                            dataRow["CSTID"] = CSTID;
                            dataTable.Rows.Add(dataRow);
                        }

                        if (dataTable.Rows.Count > 0)
                            dataTable = dataTable.DefaultView.ToTable(true);

                        foreach (DataRow dr in dataTable.Rows)
                        {
                            string cstid = dr["CSTID"].ToString();
                            for (int i = 0; i < dgTrayList.Rows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).Equals(cstid))
                                {
                                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                                    continue;
                                }
                            }
                        }

                        Util.Alert("FM_ME_0239");  //처리가 완료되었습니다.

                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 20221018_Rack간 이동/이동해제 기능 추가 START
        private void btnRackMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));

                if (!(_sStatus.Equals("S") || _sStatus.Equals("A")))
                {
                    //Aging 공정 진행중인 Tray가 아닙니다.
                    Util.Alert("FM_ME_0452");
                    return;
                }

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        drIn["MENUID"] = LoginInfo.CFG_MENUID;
                        drIn["USER_IP"] = LoginInfo.USER_IP;
                        drIn["PC_NAME"] = LoginInfo.PC_NAME;
                        dtInData.Rows.Add(drIn);

                        if (!Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7"))
                        {
                            //Aging 호기간 이동은 출하 Aging 공정에서만 가능합니다.
                            Util.Alert("FM_ME_0455");
                            return;
                        }

                        if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")).Equals(""))
                        {
                            //Aging 공정 진행중인 Tray가 아닙니다.
                            Util.Alert("FM_ME_0452");
                            return;
                        }
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //Aging 호기간 이동 하시겠습니까?
                Util.MessageConfirm("FM_ME_0453", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_AGING_UNITS_MOVE", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                                {
                                    //Aging 호기간 이동 지시를 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0454");
                                    GetList();
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "-1")
                                {
                                    //Aging 호기간 이동은 출하 Aging 공정에서만 가능합니다.
                                    Util.Alert("FM_ME_0455");
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
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 20221018_Rack간 이동/이동해제 기능 추가 END

        // 20221018_Rack간 이동/이동해제 기능 추가 START
        private void btnRackRel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("MENUID", typeof(string));
                dtInData.Columns.Add("USER_IP", typeof(string));
                dtInData.Columns.Add("PC_NAME", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        drIn["MENUID"] = LoginInfo.CFG_MENUID;
                        drIn["USER_IP"] = LoginInfo.USER_IP;
                        drIn["PC_NAME"] = LoginInfo.PC_NAME;
                        dtInData.Rows.Add(drIn);
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //Aging 호기간 이동 해제하시겠습니까?
                Util.MessageConfirm("FM_ME_0456", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_IN", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //Aging 호기간 이동 해제를 완료하였습니다.
                                Util.MessageInfo("FM_ME_0457");
                                GetList();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 20221018_Rack간 이동/이동해제 기능 추가 END

        // 20221018_Rack간 이동/이동해제 기능 추가 START
        private string GetProcessGrpCode(string sProcID)
        {
            string sProcessID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = sProcID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_PROCID_F", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sProcessID = dtRslt.Rows[0]["PROC_GR_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sProcessID;
        }
        // 20221018_Rack간 이동/이동해제 기능 추가 END

        #region 20230404 라우트변경 Line & Model 비활성화 추가
        private string[] GetLineAndModel(string sLotID)
        {
            string[] sLineReturn = new string[3];
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTIDLIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTIDLIST"] = sLotID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LIST_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLineReturn[0] = dtRslt.Rows.Count.ToString();
                    sLineReturn[1] = dtRslt.Rows[0]["EQSGID"].ToString();
                    sLineReturn[2] = dtRslt.Rows[0]["MDLLOT_ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sLineReturn;
        }
        #endregion

        #region 20231226 라우트변경 전 반송 상태인 CST ID 검색
        private string[] GetMhsTrfCmd(string sCstID)
        {
            string[] _sReturn = new string[2];

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; ;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTID"] = sCstID.Substring(0, sCstID.Length - 1);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_TRAY_TRF_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                _sReturn[0] = dtRslt.Rows[0]["RETURN"].ToString();

                if (dtRslt.Rows[0]["RETURN"].ToString().Equals("N"))
                {
                    _sReturn[1] = dtRslt.Rows[0]["ERR_CSTID"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return _sReturn;
        }
        #endregion

        # region 20231006 suyong.kim 보정dOCV 항목 재전송
        private void btnDocvReTrans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("LOTID", typeof(string));
                dtInData.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "DOCV_RCV_FLAG")).Equals("N"))
                        //{
                        DataRow drIn = dtInData.NewRow();
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        drIn["USERID"] = LoginInfo.USERID;
                        dtInData.Rows.Add(drIn);
                        //}
                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                //보정dOCV 항목 재전송 요청 하시겠습니까?
                Util.MessageConfirm("FM_ME_0473", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_DOCV_TRNF_INFO_MANUAL_MULTI", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //보정dOCV 항목 재전송 요청 하였습니다.
                                Util.MessageInfo(ObjectDic.Instance.GetObjectName("UC_0043"));
                                GetList();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private void btnCtSampleOUT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ctTrayID = string.Empty;

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        _ctTrayID += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID")) + ",";
                        inDataTable.Rows.Add(dr);

                        /*    
                        if ((txtStatus.Text.Equals("WAIT") || _sStatus.Equals("E")) &&
                            (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9")))
                        {
                            isNoSampleOut = true;
                        }
                        */
                    }
                }
                _ctTrayID = _ctTrayID.TrimEnd(',');
                if (inDataTable.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                CtSampleOut_YN(); //CT출고 가능여부 확인
                if (ctRetval == "CT")
                {
                    //이미 CT SAMPLE 추출중인 Tray가 포함되어 있습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0556", _ctExTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            return;
                        }

                    });
                }
                else if (ctRetval == "SAMPLE")
                {
                    //SAMPLE 추출중인 Tray가 포함되어 있습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0555", _ctExTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            return;
                        }

                    });
                }

                else if (ctRetval == "FORCE")
                {

                    //강제 출고 중인 Tray가 포함되어 있습니다. [Trayid: %1]
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0600", _ctExTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            return;
                        }

                    });                    
                }
           
                else if (ctRetval == "OK")
                {
                    Util.MessageConfirm("FM_ME_0487", (result) =>  //[Tray ID : {0}]를 CTC Sample 출고 하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_CTC_SAMPLE_OUT", "INDATA", "OUTDATA", inDataTable);

                                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0559");  //CT Sample 출고 지시를 완료하였습니다.
                                    GetList();
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0066");
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                    }, _ctTrayID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnCtSampleRel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ctTrayID = string.Empty;

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "INDATA";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        _ctTrayID += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID")) + ",";
                        inDataTable.Rows.Add(dr);

                    }
                }
                _ctTrayID = _ctTrayID.TrimEnd(',');

                if (inDataTable.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                CtSampleIn_YN(); //CT샘플 해제 가능 여부 확인.
                if (ctIn_YN == "N")
                {
                    //CT 추출 되지않은 TRAY가 포함되어 있습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0557", CtSampleInTray), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            return;
                        }
                        else return;
                    });
                }

                if (ctIn_YN != "N")
                {
                    Util.MessageConfirm("FM_ME_0488", (result) =>  //[Tray ID : {0}]를 CTC Sample 해제하시겠습니까?
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            return;
                        }
                        else
                        {
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_CTC_SAMPLE_IN", "INDATA", "OUTDATA", inDataTable);

                                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0558");  //CT Sample 해제를 완료하였습니다.
                                    GetList();
                                }
                                else
                                {
                                    Util.Alert("ME_0068");  //Sample 해제에 실패하였습니다.
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }

                    }, _ctTrayID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string CtSampleOut_YN() //CT 샘플출고 가능 여부 확인.
        {
            try
            {
                _ctExTray = string.Empty; //trayid
                ctRetval = "OK"; 

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID");

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtRqst.Rows.Add(dr);
                    }
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CT_SAMPLE_YN", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {   //이미 CT SAMPLE 출고중인 TRAY 중복의뢰 막기 
                    Decimal Aging_CT_Count = dtRslt.Select("AGING_ISS_PRIORITY_NO = '7'AND CT_TYPE_CODE = 'Y'").Count(); 
                    if (Aging_CT_Count > 0)
                    {
                        ctRetval = "CT";
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            if (dtRslt.Rows[i]["AGING_ISS_PRIORITY_NO"].ToString().Equals("7") && dtRslt.Rows[i]["CT_TYPE_CODE"].ToString().Equals("Y"))
                            {
                                _ctExTray += dtRslt.Rows[i]["CSTID"].ToString() + ','; //CT SAMPLE 출고중인 TRAY

                            }
                        }
                        _ctExTray = _ctExTray.TrimEnd(',');

                        return ctRetval;
                    }
                    //SAMPLE 출고중인 TRAY CT 샘플출고 막기 
                    Decimal Aging_SAMPLE_Count = dtRslt.Select("AGING_ISS_PRIORITY_NO = '7'AND [CT_TYPE_CODE] IS NULL").Count(); 
                    if (Aging_SAMPLE_Count > 0)
                    {
                        ctRetval = "SAMPLE";
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            if (dtRslt.Rows[i]["AGING_ISS_PRIORITY_NO"].ToString().Equals("7") && !dtRslt.Rows[i]["CT_TYPE_CODE"].ToString().Equals("Y"))
                            {
                                _ctExTray += dtRslt.Rows[i]["CSTID"].ToString() + ","; //SAMPLE 출고중인 TRAY 

                            }
                        }
                        _ctExTray = _ctExTray.TrimEnd(',');

                        return ctRetval;
                    }
                    //강제 출고중인 TRAY CT 샘플출고 막기 
                    Decimal Aging_Force_Count = dtRslt.Select("AGING_ISS_PRIORITY_NO = '9'").Count();
                    if (Aging_SAMPLE_Count > 0)
                    {
                        ctRetval = "FORCE";
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            if (dtRslt.Rows[i]["AGING_ISS_PRIORITY_NO"].ToString().Equals("9"))
                            {
                                _ctExTray += dtRslt.Rows[i]["CSTID"].ToString() + ","; //강제출고중인 TRAY

                            }
                        }
                        _ctExTray = _ctExTray.TrimEnd(',');

                        return ctRetval;
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return ctRetval;
        }

        private string CtSampleIn_YN() //CT 샘플 출고 해제 가능여부 확인.
        {
            try
            {
                ctIn_YN = string.Empty;
                CtSampleInTray = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID");

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CT_SAMPLE_YN", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Decimal Aging_CT_Count = dtRslt.Select("AGING_ISS_PRIORITY_NO <> '7'").Count(); //CT샘플 출고중이 아닌 TRAY COUNT
                    Decimal Aging_Smple_Count = dtRslt.Select("AGING_ISS_PRIORITY_NO = '7'AND [CT_TYPE_CODE] IS NULL").Count(); //SAMPLE출고 중인 TRAY COUNT 
                    if (Aging_CT_Count + Aging_Smple_Count > 0)
                    {
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            if (!dtRslt.Rows[i]["AGING_ISS_PRIORITY_NO"].ToString().Equals("7") || !dtRslt.Rows[i]["CT_TYPE_CODE"].ToString().Equals("Y")) //CT 샘플출고중이 아닌 TRAY.
                            {
                                CtSampleInTray += dtRslt.Rows[i]["CSTID"].ToString() + ",";

                            }
                        }
                        CtSampleInTray = CtSampleInTray.TrimEnd(',');
                        ctIn_YN = "N";  //CT SAMPLE 출고중이 아닌 TRAY 포함됐을시

                        return ctIn_YN;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return ctIn_YN;
        }

        
    }
}

