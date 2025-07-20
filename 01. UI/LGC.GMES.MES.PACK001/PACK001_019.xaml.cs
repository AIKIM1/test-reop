/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.05.14  손우석  입고현황 추가
                      입고 완료 가용수량 : CELL 출고된 정보 중 입고하고 투입 대기인 상태 CELL 수
                      입고 완료 HOLD : CELL 출고된 정보 중 입고하고 투입 대기인 상태 중 HOLD된 CELL 수(QMS LOT HOLD 포함)
                      입고 대기 가용수량 : CELL 출고된 정보 중 입고하지 않은 CELL 수
                      입고 대기 HOLD : CELL 출고된 정보 중 입고하지 않은 CELL 중 HOLD 수(CELL 포장에서 출하 HOLD한 CELL, QMS LOT HOLD)
  2018.07.27          입고할 Pallet 조회시 OCV유무 체크에 대한 기능 수정
  2018.09.14  손우석  3788562 GMES 셀 입고 시 요청번호 C20180910_88562
  2019.04.01  손우석  조회 기간에 시간 추가 및 +1일하기
  2019.06.07  손우석  이력 조회 기간 일주일 기본 설정
  2010.11.06  강호운  CELL 정보 동간이동 기능 추가
  2019.11.07  손우석  4102929 장기재고 검사 누락 셀 팩라인 투입 인터락 기능 구현 요청 건 [요청번호]C20191011_02929
  2019.12.27  염규범  출고 ID, PALLET ID  추가 Valdation
  2020.01.17  염규범  폴란드의 경우, DB가 분리되어 있어서, 입고 잘못되는 Issus Validation Logic 추가
  2020.02.04  손우석  CSR ID 25713 'Cell 입고 대기 전체 조회' UI 신규 생성 요청 건 [요청번호] C20200128-000438
  2020.02.13  손우석  CSR ID 25713 'Cell 입고 대기 전체 조회' UI 신규 생성 요청 건 [요청번호] C20200128-000438
  2020.10.08  염규범  INDATE 추가의건 SRCTYPE
  2020.10.22  염규범  PACK CELL 입고 Transaction 분산 처리
  2021.08.25  김용준  GMES 시스템의 타 법인셀 입고를 위한 입고대기 현황 기능 개선 건 [요청번호] C20210720-000232
  2022.05.13  정용석  DCIR 검사 존재여부 추가
  2023.06.22  정유진  PLLT_BCD_ID OUTDATA 추가, BCD ID로 조회가능하도록 비즈 변경 - RTD
  2024.06.26  김민석  CELL 입고 화면 입고 시 GBT 중복 여부 FLAG, 입고 가능여부 FLAG N인 경우 입고되지 않도록 수정 및 기존 INDEX가 숫자로 하드 코딩 된 부분 수정
                      , 각 FLAG VALIDATION 시 CHK된 항목만 볼 수 있도록 수정, 취소 및 선택 취소 버튼 선택 시 GRID 공란일 경우 오류 메시지 추가
                      , 입고가능FLAG Y일 경우 자동 CHECK되는 부분 주석 처리 및 처음 조회 시에만 CHECK되도록 수정
                      , 장기재고 VALIDATION 시 CHK된 항목만 VALIDATION되도록 수정 [요청번호] E20240617-000090
  2024.07.01  김민석  입고대기현황 탭 동 콤보박스 'SELECT'으로 조회 시 에러 메시지 추가
                      , PALLET ID 입력하여 ENTER KEYDOWN 조회 시 동 조건 무조건 포함되도록 삼항식 제거
                      , 조회기간 VALIDATION 추가 [요청번호] E20240701-000177
  2025.04.16  윤주일  MI1_OSS_0075 / CELL 입고 화면 내 입/출고일자 기본 날짜를 from-to 동일하고 조회 시점 일자로 설정
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_019 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_019()
        {
            InitializeComponent();

            if (LoginInfo.CFG_AREA_ID == "P7" || LoginInfo.CFG_AREA_ID == "P8" || LoginInfo.CFG_AREA_ID == "PA")
            {
                btnTagetMove.Visibility = Visibility.Visible;
            }
            else
            {
                btnTagetMove.Visibility = Visibility.Collapsed;
            }

            // 공통 코드 DIFFUSION_SITE 적용 
            DataTable dtsiteValid = new DataTable();
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["CMCODE"] = "RISK_RANGE_HOLD";
            RQSTDT.Rows.Add(dr);

            dtsiteValid = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DIFFUSION_SITE", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtsiteValid != null && dtsiteValid.Rows[0]["CHECK_YN"].ToString() == "Y")
            {
                colPKG_RISK_HOLD.Visibility = Visibility.Visible;
            }
            else
            {
                colPKG_RISK_HOLD.Visibility = Visibility.Collapsed;
            }


            //#region Tray물류여부 체크
            DataTable dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "CELL_TRAY_LOGIS");
            if (null != dtDiffusionSite && dtDiffusionSite.Rows.Count > 0)
            {
                if (Util.NVC(dtDiffusionSite.Rows[0]["ATTRIBUTE2"]).Contains(LoginInfo.CFG_AREA_ID))
                {
                    colTAG_ID1.Visibility = Visibility.Visible;
                    colTAG_ID2.Visibility = Visibility.Visible;
                    colTAG_ID3.Visibility = Visibility.Visible;
                }
                else
                {
                    colTAG_ID1.Visibility = Visibility.Collapsed;
                    colTAG_ID2.Visibility = Visibility.Collapsed;
                    colTAG_ID3.Visibility = Visibility.Collapsed;
                }
            }
            //#endregion //Tray물류여부 체크


        }
        #endregion

        #region Initialize
        //2020.02.04
        private void InitCombo()
        {
            setComboBox();
        }
        #endregion

        #region Event

        #region Event - UserControl

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CELL_PLT_BCD_USE_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtReturn.Rows.Count > 0)
                {
                    colBCDID.Visibility = Visibility.Visible;
                    colBCDID2.Visibility = Visibility.Visible;
                    colBCDID3.Visibility = Visibility.Visible;
                    colBCDID4.Visibility = Visibility.Visible;
                }

                else
                {
                    colBCDID.Visibility = Visibility.Collapsed;
                    colBCDID2.Visibility = Visibility.Collapsed;
                    colBCDID3.Visibility = Visibility.Collapsed;
                    colBCDID4.Visibility = Visibility.Collapsed;
                }

                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm);
                listAuth.Add(btnPalletInfoChangePopUpOpen);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                DateTime DateNow = DateTime.Now;
                DateTime firstOfThisMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                //2019.06.07
                //DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddMonths(1);
                DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddDays(7);
                //DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                //dtpDateFrom.SelectedDateTime = firstOfThisMonth; //20250418
                //2019.06.07
                //dtpDateTo.SelectedDateTime = lastOfThisMonth;
                //dtpDateTo.SelectedDateTime = firstOfNextMonth; //20250418

                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;

                dtpWaitSearchDateFrom.IsNullInitValue = true;
                dtpWaitSearchDateTo.IsNullInitValue = true;

                dtpWaitSearchDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpWaitSearchDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                //dtpWaitSearchDateFrom.SelectedDateTime = firstOfThisMonth; //2025.04.16
                //2019.06.07
                //dtpWaitSearchDateTo.SelectedDateTime = lastOfThisMonth;
                //dtpWaitSearchDateTo.SelectedDateTime = firstOfNextMonth; //2025.04.16

                //2018.05.14
                dtpCell_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpCell_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                //2020.02.04
                //setComboBox();
                InitCombo();

                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbWaitSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbRcvCellAllListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbCellInfoCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        #endregion

        #region Event - Button

        private void btnPalletInfoChangePopUpOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_019_RECEIVEPRODUCT_CHANGE popup = new PACK001_019_RECEIVEPRODUCT_CHANGE();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    string sPalletId = txtPalletID.Text;

                    C1WindowExtension.SetParameter(popup, sPalletId);

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            // Declarations
            int FCSDataCheck = 0;

            try
            {

                if (this.dgTargetList.Rows.Count <= 0)
                {
                    ms.AlertWarning("SFU1796"); //입고 대상이 없습니다. PALLETID를 입력 하세요.
                    return;
                }

                int row = (from C1.WPF.DataGrid.DataGridRow rows in dgTargetList.Rows
                           where rows.DataItem != null
                                 // && rows.Visibility == Visibility.Visible
                                 && rows.Type == DataGridRowType.Item
                                 && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                           select rows).Count();

                if (row <= 0)
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                if (this.ChkRcvable())
                {
                    ms.AlertWarning("SFU3722");     // 입고 가능한 상태가 아닙니다.
                    return;
                }

                // 입력 체크
                if (!this.ChkInputData())
                {
                    return;
                }

                // 장기매매 검사 누락 셀 팩라인 투입 인터락 기능
                if (ChkReceive())
                {
                    Util.MessageInfo("SFU5139");    // 장기재고 Cell입니다.
                    return;
                }

                if (this.ChkLotId_Exist())
                {
                    ms.AlertWarning("SFU8502");     // 입고된 LOT 혹은 BOXID 안에서 중복된 LOTID가 있습니다.
                    return;
                }

                if (this.ChkGBTId_Exist())
                {
                    ms.AlertWarning("SFU8578");     // 입고된 LOT 중 중복된 GBT ID가 존재합니다.
                    return;
                }

                if (this.ChkRiskRange_Exist())
                {
                    ms.AlertWarning("SFU8579");  // Package LOT Risk range hold 제품이 포함되어 있어 등록 불가 합니다.
                    return;
                }


                // FCS Data Check (OCV 데이터 유무)
                if (this.ChkOCV_Exist())
                {
                    if ((bool)chkFCS.IsChecked)
                    {
                        ms.AlertWarning("SFU3447");     // OCV DATA가 없는 CELL이 존재합니다.
                        return;
                    }
                    else
                    {
                        FCSDataCheck++;
                    }
                }

                // FCS Data Check (DCIR 데이터 유무)
                if (this.ChkDCIR_Exist())
                {
                    if ((bool)chkDCIR.IsChecked)
                    {
                        ms.AlertWarning("SFU3447");     // OCV DATA가 없는 CELL이 존재합니다.
                        return;
                    }
                    else
                    {
                        FCSDataCheck++;
                    }
                }

                // 위의 두가지 체크 함수 결과가 하나라도 true이면 확인후에 입고처리
                if (FCSDataCheck > 0)
                {
                    // OCV DATA가 없는 CELL이 존재합니다. 입고하시겠습니까?"
                    Util.MessageConfirm("SFU1405", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            checkPopUpOpen();
                        }
                    });
                }
                else    // 그렇지 않으면 그냥 입고처리
                {
                    checkPopUpOpen();   // 선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
                }

                #region 주석처리
                //if (dgTargetList.Rows.Count > 0)
                //{
                //    int row = (from C1.WPF.DataGrid.DataGridRow rows in dgTargetList.Rows
                //               where rows.DataItem != null
                //                     //&& rows.Visibility == Visibility.Visible
                //                     && rows.Type == DataGridRowType.Item
                //                     && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                //               select rows).Count();

                //    if (row > 0)
                //    {
                //        if (chkInputData()) //입력 체크
                //        {
                //            //2019.11.07  4102929 장기재고 검사 누락 셀 팩라인 투입 인터락 기능 구현 요청 건 [요청번호]C20191011_02929
                //            if (!chkReceive())
                //            {
                //                if (ChkOCV_Exist())
                //                {
                //                    if ((bool)chkFCS.IsChecked)
                //                    {
                //                        ms.AlertWarning("SFU3447");//SFU3447   OCV DATA가 없는 CELL이 존재합니다.
                //                        return;
                //                    }
                //                    else
                //                    {
                //                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1405"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //                        //OCV DATA가 없는 CELL이 존재합니다. 입고하시겠습니까?"
                //                        {
                //                            if (result == MessageBoxResult.OK)
                //                            {
                //                                checkPopUpOpen();//선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
                //                            }
                //                        }
                //                        );
                //                    }
                //                }
                //                else
                //                {
                //                    checkPopUpOpen();//선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
                //                }
                //            }
                //            //2019.11.07
                //            else
                //            {
                //                //장기재고
                //                ms.AlertWarning("SFU5139");//
                //                //return;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Util.MessageInfo("SFU1636");
                //    }
                //}
                //else
                //{
                //    ms.AlertWarning("SFU1796"); //입고 대상이 없습니다. PALLETID를 입력 하세요.
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTargetList.Rows.Count <= 0)
            {
                ms.AlertWarning("SFU2093"); //취소 대상이 없습니다. PALLET ID를 입력해주세요.
                return;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //전체 취소 하시겠습니까?
            {
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                if (result == MessageBoxResult.OK)
                {
                    //for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)
                    //{
                    //    dgTagetList.RemoveRow(i);
                    //}

                    Util.gridClear(dgTargetList);

                    clearInput();
                }
            }
            );
        }

        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                if (util.GetDataGridCheckCnt(dgTargetList, "CHK") > 0)
                {
                    DataTable dtTempTagetList = DataTableConverter.Convert(dgTargetList.ItemsSource);

                    for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                            Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                        {

                            dtTempTagetList.Rows[i].Delete();
                            dtTempTagetList.AcceptChanges();
                        }
                    }
                    dgTargetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtTempTagetList.Rows.Count));
                    if (!(dtTempTagetList.Rows.Count > 0))
                    {
                        Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");
                        dgTargetList.ItemsSource = null;
                        clearInput();
                    }

                    #region 주석처리
                    /*
                    int iTotalRow = dgTagetList.Rows.Count;
                    for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")) == "True" ||
                            Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")) == "1")
                        {
                            dgTagetList.BeginEdit();
                            dgTagetList.IsReadOnly = false;
                            dgTagetList.RemoveRow(i);
                            dgTagetList.IsReadOnly = true;
                            dgTagetList.EndEdit();
                            iTotalRow--;
                        }
                    }

                    DataTableConverter.Convert(dgTagetList.ItemsSource).AcceptChanges();
                    int Test = dgTagetList.Rows.Count;

                    if (!(iTotalRow > 0))
                    {
                        dgTagetList.ItemsSource = null;
                        clearInput();
                    }

                    */
                    #endregion
                }
                else
                {
                    ms.AlertWarning("SFU2093"); //취소 대상이 없습니다. PALLET ID를 입력해주세요.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dtDateCompare()) return;

                getWareHousingData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnWaitExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgWaitSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnWaitSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboWaitSearchAREAID.SelectedValue.ToString() == "SELECT")
            {
                ms.AlertWarning("SFU1499");//동을 선택하세요
            }

            //조회 기간 validation 추가 - KIM MIN SEOK
            if (!WaitSearchDateValidation()) return;

            getInputWaitPalletInfo(null);
        }

        //2018.05.14
        private void btnCell_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getCell_InputState();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnCell_Excel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotCellList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //2018.05.14
        private void btnPalletInfo_Click(object sender, RoutedEventArgs e)
        {
            popUpOpenPalletInfo(null, null);
        }

        //2010.11.06  강호운 CELL 정보 동간이동 기능 추가
        private void btnTagetMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_019_POPUP popup = new PACK001_019_POPUP();
                popup.FrameOperation = this.FrameOperation;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        //2020.02.04
        private void btnSearchAll_Click(object sender, RoutedEventArgs e)
        {
            getRcvCellALL();
        }

        private void btnExcelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRcvList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcelAll1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Event - Button

        #region Event - TextBox

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    dgTargetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                    dgTargetList.LoadedCellPresenter += dgTagetListCellPresenter;
                    //ChkGbtInPallet();
                    // 2019.12.27 염규범
                    // 김정균 책임님 요청의 건
                    // 입고 Validation
                    // getTagetPalletInfo();
                    getTagetPalletInfoNew();

                    //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtWaitSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtWaitSearch.Text.Length > 0)
                    {
                        getInputWaitPalletInfo(txtWaitSearch.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Event - TextBox

        #region Event - ComboBox

        private void cboSearchProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                txtSearchProduct.Text = e.NewValue.ToString();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), txtTagetPRODID.Text);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.12.12
        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            try
            {
                if (cboTagetRoute.SelectedItem == null)
                {
                    sTagetArea = "";
                }
                else
                {
                    sTagetArea = Convert.ToString(DataTableConverter.GetValue(cboTagetRoute.SelectedItem, "AREAID"));
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion  Event - ComboBox

        #region Event - DataGrid

        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "PALLETID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTagetListCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                    return;
                if (dgTargetList == null)
                    return;

                if (e.Cell.Column.Name == "OCV_FLAG" || e.Cell.Column.Name == "DCIR_FLAG")
                {
                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else if (e.Cell.Column.Name == "LOT_OVERLAP")
                {
                    if (e.Cell.Text == "Y")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else if (e.Cell.Column.Name == "GBT_DUP_FLAG")
                {
                    if (e.Cell.Text == "Y")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else if (e.Cell.Column.Name == "RISK_RANGE_FLAG")
                {
                    if (e.Cell.Text == "Y")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }


                if (e.Cell.Column.Name == "RECEIVABLE_FLAG")
                {
                    C1.WPF.DataGrid.DataGridCell chkCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["CHK"].Index);
                    C1.WPF.DataGrid.DataGridCell issIdCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["RCV_ISS_ID"].Index);
                    C1.WPF.DataGrid.DataGridCell plltIdCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["PALLETID"].Index);


                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        DataTableConverter.SetValue(dgTargetList.Rows[e.Cell.Row.Index].DataItem, "CHK", 0);

                        if (!(dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter.IsEnabled = false;
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        //DataTableConverter.SetValue(dgTargetList.Rows[e.Cell.Row.Index].DataItem, "CHK", 1);
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        }
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        }
                        //if (!(dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter == null))
                        //{
                        //    dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter.IsEnabled = true;

                        //}
                    }
                }
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWaitSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "PALLETID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWaitSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWaitSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgWaitSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgWaitSearchResultList.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Ocv_Button_Click(object sender, RoutedEventArgs e)
        {
            getOcvData(sender);
        }

        //2018.12.12
        private void dgTagetList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgTargetList.CurrentRow == null || dgTargetList.SelectedIndex == -1)
            {
                return;
            }

            string sColName = dgTargetList.CurrentColumn.Name;
            string strChkValue = "";
            string strOkValue = "";

            if (!sColName.Contains("CHK"))
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgTargetList.CurrentRow.Index;
                    int indexColumn = dgTargetList.CurrentColumn.Index;

                    C1.WPF.DataGrid.DataGridCell ableYNCell = dgTargetList.GetCell(dgTargetList.CurrentRow.Index, dgTargetList.Columns["RECEIVABLE_FLAG"].Index);


                    strChkValue = Util.NVC(dgTargetList.GetCell(indexRow, indexColumn).Value);
                    strOkValue = Util.NVC(dgTargetList.GetCell(indexRow, ableYNCell.Column.Index).Value);

                    if (string.IsNullOrEmpty(strChkValue) || strChkValue.Equals(""))
                        return;
                    if (!strOkValue.Equals("N"))
                    {
                        if (strChkValue == "0" || strChkValue == "False")
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[dgTargetList.CurrentRow.Index].DataItem, sColName, true);
                        }
                        else if (strChkValue == "1" || strChkValue == "True")
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[dgTargetList.CurrentRow.Index].DataItem, sColName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.02.04
        private void dgRcvList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name == "RCV_ISS_ID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (e.Cell.Column.Name == "MOVE_PERIOD")
                    {
                        SetCellColor(dataGrid, e);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRcvList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRcvList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("RCV_ISS_ID"))
                    {
                        string sRCVISSID = Util.NVC(DataTableConverter.GetValue(dgRcvList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));

                        getRcvCellALL_DETAIL(sRCVISSID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion  Event - DataGrid

        #endregion Event

        #region Mehod
        /// <summary>
        /// 입력 Validation
        /// </summary>
        /// <returns>true:정상 , false: 입력/선택 오류</returns>
        private bool ChkInputData()
        {
            bool bReturn = true;

            try
            {
                //if (cboTagetAREAID.SelectedIndex < 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동을선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    bReturn = false;
                //    cboTagetRoute.Focus();
                //    return bReturn;
                //}

                //if (cboTagetEQSGID.SelectedIndex < 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    bReturn = false;
                //    cboTagetRoute.Focus();
                //    return bReturn;
                //}

                if (cboTagetRoute.SelectedIndex < 0)
                {
                    ms.AlertWarning("SFU1455"); //경로를 선택 하세요
                    bReturn = false;
                    cboTagetRoute.Focus();
                    return bReturn;
                }

                if (cboTagetModel.SelectedIndex < 0)
                {
                    ms.AlertWarning("SFU1619"); //생산예정모델을 선택 하세요.
                    bReturn = false;
                    cboTagetModel.Focus();
                    return bReturn;
                }

                bReturn = ChkGbtInPallet();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        private void checkPopUpOpen()
        {
            #region 선택확인 팝업
            PACK001_019_RECEIVEPRODUCT_SELECTCHECK popup = new PACK001_019_RECEIVEPRODUCT_SELECTCHECK();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                DataTable dtData = new DataTable();
                dtData.Columns.Add("MODELNAME", typeof(string));
                dtData.Columns.Add("PRODUCTNAME", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("ROUTENAME", typeof(string));
                dtData.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = null;

                newRow = dtData.NewRow();
                newRow["MODELNAME"] = cboTagetModel.Text;
                newRow["PRODUCTNAME"] = txtTagetPRODNAME.Text;
                newRow["PRODID"] = txtTagetPRODID.Text;
                newRow["ROUTENAME"] = cboTagetRoute.Text;
                newRow["LOTTYPE"] = "";//cboTagetLotType.Text;
                dtData.Rows.Add(newRow);

                //========================================================================
                object[] Parameters = new object[2];
                Parameters[0] = dtData;
                Parameters[1] = GetSaveWarehousing_DataSet();
                C1WindowExtension.SetParameters(popup, Parameters);
                //========================================================================
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
            #endregion
        }

        void popup_Closed(object sender, EventArgs e)
        {
            PACK001_019_RECEIVEPRODUCT_SELECTCHECK popup = sender as PACK001_019_RECEIVEPRODUCT_SELECTCHECK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //if (ChkGbtInPallet())
                //{
                setWarehousingUnitPallet(); //입고처리
                //}
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region [입고 - 변경전 로직 ]
        /*
        private void setWarehousing()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTagetList.ItemsSource);
                //RCV_ISS_ID groupby 추출
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        drRCV_ISS = dtRCV_ISS.NewRow();
                        drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "RCV_ISS_ID"));
                        drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "PALLETID"));
                        dtRCV_ISS.Rows.Add(drRCV_ISS);
                    }
                }

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtRCV_ISS);
                loadingIndicator.Visibility = Visibility.Visible;


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_PRODUCT_PACK", "INDATA,RCV_ISS", "OUTDATA", (dsResult, dataException) =>
                  {
                      try
                      {
                          loadingIndicator.Visibility = Visibility.Collapsed;
                          if (dataException != null)
                          {
                              Util.MessageException(dataException);
                              return;
                          }
                          else
                          {
                              if (dsResult != null && dsResult.Tables.Count > 0)
                              {

                                  if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                  {
                                      if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                      {
                                          ms.AlertInfo("SFU1412"); //PALLET을입고하였습니다

                                          Util.gridClear(dgTagetList);

                                          Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");

                                          clearInput();

                                          getWareHousingData();//조회
                                      }

                                  }
                              }
                              return;
                          }
                      }
                      catch (Exception ex)
                      {
                          throw ex;
                      }

                  }, dsIndata);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }
        */
        #endregion

        private void setWarehousingUnitPallet()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                var serWareHousingList = dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true);

                DataTable dtPallet = serWareHousingList.CopyToDataTable();

                loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                for (int i = 0; i < dtPallet.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["PALLETID"])) || string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"])))
                    {
                        Util.MessageInfo("SFU3256");
                        return;
                    }

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("SRCTYPE", typeof(string));
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("MODLID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    INDATA.Columns.Add("EQSGID", typeof(string));
                    INDATA.Columns.Add("ROUTID", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                    DataRow drINDATA = null;
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);

                    INDATA.Rows.Add(drINDATA);

                    DataTable RCV_ISS = new DataTable();
                    RCV_ISS.TableName = "RCV_ISS";
                    RCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                    RCV_ISS.Columns.Add("PALLETID", typeof(string));

                    DataRow drRCV_ISS = null;
                    drRCV_ISS = RCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);
                    drRCV_ISS["PALLETID"] = Util.NVC(dtPallet.Rows[i]["PALLETID"]);
                    RCV_ISS.Rows.Add(drRCV_ISS);

                    DataSet dsIndata = new DataSet();
                    dsIndata.Tables.Add(INDATA);
                    dsIndata.Tables.Add(RCV_ISS);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_PACK", "INDATA,RCV_ISS", "OUTDATA", dsIndata);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        dt.AcceptChanges();

                        foreach (DataRow drDel in dt.Rows)
                        {
                            if (drDel["PALLETID"].ToString() == dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].GetString())
                            {
                                drDel.Delete();
                                break;
                            }
                        }

                        dt.AcceptChanges();

                        Util.GridSetData(dgTargetList, dt, FrameOperation);

                    }

                }

                ms.AlertInfo("SFU1412");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                loadingIndicator.Visibility = Visibility.Collapsed;

            }
        }

        private Boolean ChkGbtInPallet()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                object[] arrPallet = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["PALLETID"]).ToArray();
                object[] arrRcv = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["RCV_ISS_ID"]).ToArray();

                string[] arrPalletStr = arrPallet.Cast<string>().ToArray();
                string[] arrRcvStr = arrRcv.Cast<string>().ToArray();

                string strSeparator = ",";

                string strPallet = string.Join(strSeparator, arrPalletStr);
                string strRcv = string.Join(strSeparator, arrRcvStr);


                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["RCV_ISS_ID"] = strRcv;
                dr["PALLETID"] = strPallet;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_GBT", "INDATA", null, INDATA);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return true;


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //Util.MessageException(ex);
                //Util.MessageInfo(ex.Message.ToString());

                Util.MessageInfo(ex.Data["CODE"].ToString());

                return false;
            }
        }

        private DataSet GetSaveWarehousing_DataSet()
        {
            DataSet dsIndata = new DataSet();
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //RCV_ISS_ID groupby 추출
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTargetList.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "RCV_ISS_ID"));
                    drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "PALLETID"));
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }


                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtRCV_ISS);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dsIndata;
        }

        private void getWareHousingData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = null;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedValue) == "" ? null : Util.NVC(cboProductModel.SelectedValue); //null;//cboSearchModel.SelectedValue;
                dr["PRODID"] = null;//cboSearchProduct.SelectedValue;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["PALLETID"] = null;//txtSearchPallet.Text;
                dr["LOTID"] = null;//txtSearchLot.Text;
                dr["EQSGID"] = Util.NVC(cboSearchEQSGID.SelectedValue) == "" ? null : Util.NVC(cboSearchEQSGID.SelectedValue);
                dr["AREAID"] = Util.NVC(cboSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboSearchAREAID.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", "RQSTDT", "RSLTDT", RQSTDT);


                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getTagetPalletInfoNew()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("OCV_FLAG", typeof(string));
                RQSTDT.Columns.Add("DCIR_FLAG", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["PALLETID"] = txtPalletID.Text;
                dr["OCV_FLAG"] = ((bool)chkFCS.IsChecked) ? "Y" : null;
                dr["DCIR_FLAG"] = ((bool)this.chkDCIR.IsChecked) ? "Y" : null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RECEIVE_PRODUCT_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (chkPalletInput(dtResult))
                {
                    txtTagetPRODID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                    txtTagetPRODNAME.Text = Util.NVC(dtResult.Rows[0]["PRODNAME"]);
                    sTagetArea = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                    setComboBox_Model_schd(txtTagetPRODID.Text);
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtResult.Rows[i]["RECEIVABLE_FLAG"].Equals("Y"))
                        {
                            dtResult.Rows[i]["CHK"] = "True";
                        }
                    }
                    DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                    dtBefore.Merge(dtResult);

                    dgTargetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));
                }
                txtPalletID.Text = "";
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.05.14
        private void getCell_InputState()
        {
            try
            {
                //2019.04.01
                string strStrTime = "";
                string strEndTime = "";

                switch (LoginInfo.CFG_SHOP_ID)
                {
                    case "A040":
                        strStrTime = " 06:00:00";
                        strEndTime = " 05:59:59";
                        break;

                    case "G451":
                        strStrTime = " 07:00:00";
                        strEndTime = " 06:59:59";
                        break;

                    case "G382":
                        strStrTime = " 06:00:00";
                        strEndTime = " 05:59:59";
                        break;

                    case "G481":
                        strStrTime = " 07:00:00";
                        strEndTime = " 06:59:59";
                        break;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_ST", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_ED", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAREACell.SelectedValue);
                //2019.04.01
                //dr["ISS_DTTM_ST"] = dtpCell_DateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                //dr["ISS_DTTM_ED"] = dtpCell_DateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["ISS_DTTM_ST"] = dtpCell_DateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + strStrTime;
                dr["ISS_DTTM_ED"] = dtpCell_DateTo.SelectedDateTime.ToString("yyyy-MM-dd") + strEndTime;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgLotCellList, dtResult, FrameOperation, true);
                    //Util.SetTextBlockText_DataGridRowCount(tbCell_LotListCount, Util.NVC(dgLotCellList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.05.14
        private bool chkPalletInput(DataTable dtResult)
        {
            bool bResult = true;
            try
            {
                if (!(dtResult.Rows.Count > 0))
                {
                    ms.AlertWarning("SFU1888"); //정보없는ID입니다
                    bResult = false;
                    return bResult;
                }
                if (Util.NVC(dtResult.Rows[0]["RCV_ISS_STAT_CODE"]) == "END_RECEIVE")
                {
                    ms.AlertWarning("SFU1800"); //입고완료된ID입니다
                    bResult = false;
                    return bResult;
                }

                #region 입력된이전값과 비교
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                bool bCheck = false;
                string sCheckPalletId = string.Empty;
                if (dtBefore.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow[] drTemp = dtBefore.Select("PALLETID = '" + Util.NVC(dtResult.Rows[i]["PALLETID"]) + "'");
                        if (drTemp.Length > 0)
                        {
                            bCheck = true;
                            sCheckPalletId = Util.NVC(dtResult.Rows[i]["PALLETID"]);
                            break;
                        }
                    }
                }
                if (bCheck)
                {
                    ms.AlertWarning("SFU1410", sCheckPalletId); //PALLETID는중복입력할수없습니다.\r\n({0})
                    bResult = false;
                    return bResult;
                }
                #endregion

                if (txtTagetPRODID.Text != "")
                {
                    if (txtTagetPRODID.Text != Util.NVC(dtResult.Rows[0]["PRODID"]))
                    {
                        ms.AlertWarning("SFU1481"); //다른제품ID를입고할수없습니다.
                        bResult = false;
                        return bResult;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                #region 입고
                //작업자 선택 동
                //C1ComboBox[] cboAreaChild = { cboTagetEQSGID };
                //_combo.SetCombo(cboTagetAREAID, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sCase: "AREA");

                ////작업자 선택 라인
                //C1ComboBox[] cboEquipmentSegmentParent = { cboTagetAREAID };
                //_combo.SetCombo(cboTagetEQSGID, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

                //작업자 조회 동
                /* cboAreaAll
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");
                */

                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                    _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.NONE, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "cboAreaAll");
                }
                else
                {
                    C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                    _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");
                }

                //작업자 조회 라인
                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                C1ComboBox[] cboSearchEQSGIDChild = { cboProductModel };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.SELECT, cbParent: cboSearchEQSGIDParent, cbChild: cboSearchEQSGIDChild, sCase: "EQUIPMENTSEGMENT");

                //모델
                C1ComboBox[] cboProductModelParent = { cboSearchAREAID, cboSearchEQSGID };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");
                #endregion

                #region 입고대기 현황
                //동
                _combo.SetCombo(cboWaitSearchAREAID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "AREA_AREATYPE");
                #endregion

                //2018.05.14
                #region Cell 투입 재고 현황
                //동
                _combo.SetCombo(cboAREACell, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "AREA_AREATYPE");
                #endregion
                //2018.05.14

                //2020.02.04
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                SetFlag();

                #region OLD
                /*
                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = null; //LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;
                C1ComboBox cboAreaByAreaType = new C1ComboBox();
                cboAreaByAreaType.SelectedValue = null;
                C1ComboBox cboEquipmentSegment = new C1ComboBox();
                cboEquipmentSegment.SelectedValue = null;
                C1ComboBox cboProductType = new C1ComboBox();
                cboProductType.SelectedValue = "CELL";

                //입고대상입력 모델
                C1ComboBox[] cboTagetProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                C1ComboBox[] cboTagetProductModelChild = { cboTagetProduct , cboTagetRoute };
                _combo.SetCombo(cboTagetModel, CommonCombo.ComboStatus.NONE, cbChild: cboTagetProductModelChild, cbParent: cboTagetProductModelParent , sCase: "PRODUCTMODEL");

                //입고대상입력 제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboTagetModel, cboAREA_TYPE_CODE, cboProductType };
                C1ComboBox[] cboTagetProductChild = { cboTagetRoute };
                _combo.SetCombo(cboTagetProduct, CommonCombo.ComboStatus.ALL, cbChild: cboTagetProductChild, cbParent: cboProductParent, sCase: "PRODUCT");

                //입고대상입력 LOTTYPE
                _combo.SetCombo(cboTagetLotType, CommonCombo.ComboStatus.NONE, null, sCase: "LOTTYPE");

                //입고대상입력 route
                C1ComboBox[] cboTagetRouteParent = { cboTagetModel, cboTagetProduct };
                _combo.SetCombo(cboTagetRoute, CommonCombo.ComboStatus.NONE, cbParent: cboTagetRouteParent, sCase: "ROUTEBYMODLID");

                //입고대상 Line
                string[] sTagetEqsgFilter = { LoginInfo.CFG_SHOP_ID, null, Area_Type.PACK };
                _combo.SetCombo(cboTagetEQSGID, CommonCombo.ComboStatus.NONE, null, sFilter: sTagetEqsgFilter ,sCase: "EQUIPMENTSEGMENT_AREATYPE");


                //조회 Line
                string[] sFilter = { LoginInfo.CFG_SHOP_ID, null, Area_Type.PACK };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.NONE, null, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_AREATYPE");
                */


                /*
                //조회 모델
                C1ComboBox[] cboSearchModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                _combo.SetCombo(cboSearchModel, CommonCombo.ComboStatus.ALL, cbParent: cboSearchModelParent, sCase: "PRODUCTMODEL");

                //조회 제품
                C1ComboBox[] cboSearchProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboSearchModel, cboAREA_TYPE_CODE, cboProductType };
                C1ComboBox[] cboSearchProductChild = { cboSearchRoute };
                _combo.SetCombo(cboSearchProduct, CommonCombo.ComboStatus.ALL, cbChild: cboSearchProductChild, cbParent: cboSearchProductParent, sCase: "PRODUCT");

                //조회 route
                C1ComboBox[] cboSearchRouteParent = { cboSearchModel, cboSearchProduct };
                _combo.SetCombo(cboSearchRoute, CommonCombo.ComboStatus.NONE, cbParent: cboSearchRouteParent, sCase: "ROUTEBYMODLID");
                */

                //"EQUIPMENTSEGMENT_AREATYPE");

                /*

                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProcess, cboProductModel };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);

                //공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);

                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;
                //모델
                C1ComboBox[] cboProductModelChild = { cboProductType, cboProduct };
                C1ComboBox[] cboProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent);

                //제품 CLASS TYPE : CELL CMA BMA
                C1ComboBox[] cboProductTypeChild = { cboProduct };
                C1ComboBox[] cboProductTypeParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                _combo.SetCombo(cboProductType, CommonCombo.ComboStatus.ALL, cbChild: cboProductTypeChild, cbParent: cboProductTypeParent);

                //제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboProductType };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, null, cbParent: cboProductParent);
                */
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Model_schd(string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = sMTRLID;

                // 2020-01-17 - 염규범S
                // 폴란드의 경우 Itransit 재공같은경우, AEARID가 없어서 DB가 분리 되어있는 상황에서, 타동에서 타동 CELL 입고시 ISSUSE 발생
                // 해당 내용에 대해서, Login된 동으로 입고 처리 가능하도록, Login 위치로 처리
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID == "" ? null : LoginInfo.CFG_AREA_ID;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    //PILOT2 호기 대응 으로 CELL 이 구분없이 투입되도록 하기위해 ALL 추가
                    //[TB_SFC_RCV_SUBLOT_INFO] PROD_SCHD_MODL NULL로 업데이트 하기위함.
                    if (dtResult.Rows.Count > 0)
                    {
                        DataRow dr = dtResult.NewRow();
                        dr["CBO_NAME"] = "-ALL-";
                        dr["CBO_CODE"] = null;
                        dtResult.Rows.Add(dr);
                    }


                    cboTagetModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetModel.SelectedIndex = 0;
                        cboTagetModel_SelectedValueChanged(null, null);
                    }
                    else
                    {
                        cboTagetModel_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_Route_schd(string sMODLID, string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MODLID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MODLID"] = sMODLID == "" ? null : sMODLID;
                drIndata["MTRLID"] = sMTRLID;
                // 2020-01-17 - 염규범S
                // 폴란드의 경우 Itransit 재공같은경우, AEARID가 없어서 DB가 분리 되어있는 상황에서, 타동에서 타동 CELL 입고시 ISSUSE 발생
                // 해당 내용에 대해서, Login된 동으로 입고 처리 가능하도록, Login 위치로 처리
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID == "" ? null : LoginInfo.CFG_AREA_ID;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetRoute.SelectedIndex = 0;
                    }
                    else
                    {
                        Util.MessageInfo("라우터 정보가 존재하지 않습니다.");
                    }
                    //else
                    //{
                    //    cboTagetRoute_SelectionChanged(sender, null);
                    //}

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ChkOCV_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("OCV_FLAG = 'N' AND CHK = 'True'").Length > 0) //OCV존재 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool ChkDCIR_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("DCIR_FLAG = 'N' AND CHK = 'True'").Length > 0) // DCIR 존재 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool ChkLotId_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("LOT_OVERLAP = 'Y' AND CHK = 'True'").Length > 0) // LOT 중복 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool ChkGBTId_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("GBT_DUP_FLAG = 'Y' AND CHK = 'True'").Length > 0) // GBT 중복 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private bool ChkRiskRange_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("RISK_RANGE_FLAG = 'Y' AND CHK = 'True'").Length > 0) // Risk Range 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }
        private bool ChkRcvable()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("RECEIVABLE_FLAG = 'N' AND CHK = 'True'").Length > 0) // 입고 가능 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }


        private void popUpOpenPalletInfo(string sRcvIssId, string sPalletId)
        {
            try
            {
                PACK001_019_RECEIVEPRODUCT_PALLETINFO popup = new PACK001_019_RECEIVEPRODUCT_PALLETINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sRcvIssId;
                    Parameters[1] = sPalletId;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void clearInput()
        {
            try
            {
                txtTagetPRODID.Text = string.Empty;
                txtTagetPRODNAME.Text = string.Empty;

                cboTagetModel.ItemsSource = null;
                cboTagetModel.SelectedValue = null;

                cboTagetRoute.ItemsSource = null;
                cboTagetRoute.SelectedValue = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getInputWaitPalletInfo(string sRCV_ISS)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("TO_AREA_NULL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sRCV_ISS;
                dr["FROMDATE"] = sRCV_ISS != null ? null : dtpWaitSearchDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = sRCV_ISS != null ? null : dtpWaitSearchDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["AREAID"] = Util.NVC(cboWaitSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboWaitSearchAREAID.SelectedValue);//AREAID는 무조건 있어야하므로 삼항식 제거 - KIM MIN SEOK
                                                                                                                                      //sRCV_ISS != null ? null :(Util.NVC(cboWaitSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboWaitSearchAREAID.SelectedValue))
                dr["TO_AREA_NULL"] = (bool)chkNotToArea.IsChecked == true ? "Y" : "N";

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWaitSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                //Util.SetTextBlockText_DataGridRowCount(tbWaitSearchListCount, Util.NVC(dtResult.Rows.Count));

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("BR_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    //dgWaitSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgWaitSearchResultList, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbWaitSearchListCount, Util.NVC(dgWaitSearchResultList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getOcvData(object sender)
        {
            try
            {
                Button btn = sender as Button;
                int iRow = -1;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                        iRow = row.Index;
                    }
                }


                DataRowView drv = row.DataItem as DataRowView;

                string selectPallet = drv["PALLETID"].ToString();
                string sOCV_FLAG = drv["OCV_FLAG"].ToString();
                if (sOCV_FLAG == "Y")
                {
                    return;
                }


                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PALLETID"] = selectPallet;
                INDATA.Rows.Add(drINDATA);

                //DataSet dsIndata = new DataSet();
                //dsIndata.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_CHK_RECEIVE_PRODUCT", "INDATA", "OUTDATA", INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_CHK_RECEIVE_PRODUCT", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {

                        if (result.Rows.Count > 0)
                        {
                            string sOcv_Flag = Util.NVC(result.Rows[0]["OCV_FLAG"]);



                            DataTableConverter.SetValue(dgTargetList.Rows[iRow].DataItem, "OCV_FLAG", sOcv_Flag);
                            C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCell(iRow, dgTargetList.Columns["OCV_FLAG"].Index);
                            if (sOcv_Flag == "Y")
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                cell.Presenter.FontWeight = FontWeights.Bold;
                            }


                        }
                        return;
                    }
                });

                //new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_RECEIVE_PRODUCT", "INDATA", "OUTDATA", (dsResult, dataException) =>
                //{
                //    try
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        if (dataException != null)
                //        {
                //            Util.AlertByBiz("BR_PRD_CHK_RECEIVE_PRODUCT", dataException.Message, dataException.ToString());
                //            return;
                //        }
                //        else
                //        {
                //            if (dsResult != null && dsResult.Tables.Count > 0)
                //            {

                //                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                //                {
                //                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                //                    {
                //                        string sOcv_Flag = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["OCV_FLAG"]);

                //                        DataTableConverter.SetValue(dgTagetList.Rows[iRow].DataItem, "OCV_FLAG", sOcv_Flag);
                //                    }

                //                }
                //            }
                //            return;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        throw ex;
                //    }

                //}, dsIndata);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
            }

        }

        //2019.11.07
        private bool ChkReceive()
        {
            bool bReturn = false;

            try
            {
                //2024.06.26 CHK된 PALLET만 조회하기 위해 수정 - KIM MIN SEOK
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true)).CopyToDataTable();
                //RCV_ISS_ID groupby 추출
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());

                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();

                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["PROCID"] = "P1000";
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["PRODID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["PRODID"]);
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    drINDATA["PALLETID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["PALLETID"]);

                    INDATA.Rows.Add(drINDATA);
                }

                //loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_PACK", "INDATA", "OUTDATA", INDATA);

                if (dtReturn != null)
                {
                    int StandDay = int.Parse(dtReturn.Rows[0]["STAND_TIME"].ToString());
                    int OverDay = int.Parse(dtReturn.Rows[0]["OVER_TIIME"].ToString());

                    if (StandDay > 0)
                    {
                        if (OverDay >= StandDay)
                        {
                            bReturn = true;
                        }
                        else
                        {
                            bReturn = false;
                        }
                    }
                    else
                    {
                        bReturn = false;
                    }
                }

                return bReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.12.12
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                if (Util.NVC(dr["RECEIVABLE_FLAG"]).Equals("Y"))
                {
                    dr["CHK"] = true;
                }
            }
            dgTargetList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList);
        }

        //2020.02.04
        private void getRcvCellALL()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //2020.02.13
                RQSTDT.Columns.Add("AREA_TYPE", typeof(string));
                RQSTDT.Columns.Add("TO_AREA_NULL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);

                //2020.02.13
                dr["AREA_TYPE"] = Util.NVC(cboAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaType.SelectedValue);
                dr["TO_AREA_NULL"] = (bool)chkNotToArea2.IsChecked == true ? "Y" : "N";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_ALL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgRcvList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbRcvCellAllListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getRcvCellALL_DETAIL(string sRCV_ISS_ID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_ALL_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCellInfo, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbCellInfoCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFlag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                //2020.02.13
                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "ALL";
                dr["CBO_CODE"] = "";
                dt.Rows.Add(dr);

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "Country";
                //2020.02.13
                //dr_["CBO_CODE"] = "IN";
                dr_["CBO_CODE"] = "Country";
                dt.Rows.Add(dr_);

                DataRow dr_1 = dt.NewRow();
                dr_1["CBO_NAME"] = "Foreign";
                //2020.02.13
                //dr_1["CBO_CODE"] = "OUT";
                dr_1["CBO_CODE"] = "Foreign";
                dt.Rows.Add(dr_1);

                dt.AcceptChanges();

                cboAreaType.ItemsSource = DataTableConverter.Convert(dt);
                cboAreaType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetCellColor(C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Row.DataItem != null)
            {
                if (dataGrid.Name.Equals("dgRcvList"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                //국내Site   3일 이하 : 녹색
                                //           3일 초과 7일 이하 : 노란색
                                //           7일 초과 : 빨간색
                                // 해외Site 30일 이하 : 녹색
                                //          30일 초과 60일 이하 : 노란색
                                //          60일 초과 : 빨간색

                                //2020.02.13
                                //if (cboAreaType.SelectedValue.ToString() == "IN")
                                //{
                                //    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                //    if (nDiff <= 3)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //    else if (nDiff >3 && nDiff <= 7)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                //    }
                                //    else
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //}
                                //else
                                //{
                                //    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                //    if (nDiff <= 30)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //    else if (nDiff > 30 && nDiff <= 60)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                //    }
                                //    else
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //}

                                string str_AREA_TYPE = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AREA_TYPE"));

                                if (str_AREA_TYPE == "Country")
                                {
                                    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                    if (nDiff <= 3)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                    else if (nDiff > 3 && nDiff <= 7)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                }
                                else
                                {
                                    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                    if (nDiff <= 30)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                    else if (nDiff > 30 && nDiff <= 60)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                }



                            }
                        }
                    }
                }
            }
        }

        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+7);
                    //조회기간은 7일을 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3567");
                    return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion Mehod

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void dgTargetList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                string sPasteString = Clipboard.GetText();

                if (!string.IsNullOrWhiteSpace(sPasteString))
                {
                    Util.MessageInfo("SFU3180");
                    return;
                }
            }
        }

        private bool WaitSearchDateValidation()
        {
            TimeSpan timeSpan = dtpWaitSearchDateTo.SelectedDateTime.Date - dtpWaitSearchDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                Util.MessageValidation("SFU3569");//조회 시작일자는 종료일자를 초과 할 수 없습니다.
                return false;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");//조회기간은 30일을 초과 할 수 없습니다.
                return false;
            }

            return true;
        }


        #region [BizCall]        

        private DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }
        #endregion
    }
}