/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.11.13  이대근 [CSR ID:3781337] GMES update to Formation Data save, and Printing Pallet Labels | [요청번호]C20180902_81337





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        //테이블 저장용
        private DataTable dsGet = new DataTable();
        //테이블 저장용 2
        private DataSet dsGet2 = new DataSet();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sAREAID2 = string.Empty;
        string sSHOPID2 = string.Empty;

        // 조회한 수량 저장하기 위한 변수
        private int isQty = 0;
        // 출하 예정일
        string shipdt = "";
        string Shipdate_Schedule = "";
        // 출하 예정일2 (QA 이력조회 태그 재발행 메일 재전송 에 사용)
        string shipdt2 = "";
        string Shipdate_Schedule2 = "";

        public BOX001_010()
        {
            InitializeComponent();
            Loaded += BOX001_010_Loaded;
        }

        private void BOX001_010_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_010_Loaded;
            
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
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            //정보전송조회
            dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Area 셋팅
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            //포장상태 Combo Set.
            //string[] sFilter3 = { "BOXSTAT" };
            //_combo.SetCombo(cboPackStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            //출고상태 Combo Set.
            string[] sFilter4 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };
            _combo.SetCombo(cboShipStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");

            //타입 Combo Set.
            string[] sFilter5 = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            dtpShipDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            //dtpShipDate.Visibility = Visibility.Hidden;
            //lblShipDate.Visibility = Visibility.Hidden;

            //검사의뢰이력조회
            dtpDateFrom2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo2.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Area 셋팅
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            //타입 Combo Set.
            string[] sFilterTYPE = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype2, CommonCombo.ComboStatus.ALL, sFilter: sFilterTYPE, sCase: "COMMCODE");


            //판졍결과 Combo Set.
            string[] sFilterJUDGE = { "JUDGE_VALUE" };
            _combo.SetCombo(cboJudge_Value, CommonCombo.ComboStatus.ALL, sFilter: sFilterJUDGE, sCase: "COMMCODE");



            dtpShipDate2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            //dtpShipDate2.Visibility = Visibility.Hidden;
            //lblShipDate2.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Event


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
        }

        /// <summary>
        /// 검사의뢰 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQASearch_Click(object sender, RoutedEventArgs e)
        {
            GetQAReqHis();
        }

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (dgPalletInfo.CurrentRow == null || dgPalletInfo.SelectedIndex == -1)
            {
                return;
            }
            else if (e.ChangedButton.ToString().Equals("Left") && dgPalletInfo.CurrentColumn.Name == "CHK")
            {
                string sPalletid = string.Empty;
                try
                {
                    // Rows Count가 0보다 클 경우에만 이벤트 발생하도록
                    if (dgPalletInfo.GetRowCount() > 0)
                    {
                        sPalletid = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["PALLETID"].Index).Value);

                        if (Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);
                        }


                        if (Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value) == "1")
                        {
                            int trayRows = 0;
                            trayRows = dgTray.GetRowCount();

                            // 외뢰한 시간을 체크하여 5분전에 의뢰한 Pallet ID는 재의뢰할 수 없도록 함.
                            // 서용수님 요청사항.
                            if (Check2QAREQ(sPalletid) == "NG")
                            {
                                Util.MessageValidation("SFU1301"); //"5분 전에 의뢰된 PALLET ID 입니다."
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                                return;
                            }

                            //전송 정보 중복 여부 체크
                            if (trayRows != 0)
                            {
                                for (int i = 0; i < trayRows; i++)
                                {
                                    if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value) == sPalletid)
                                    {
                                        Util.MessageValidation("SFU1776"); //"이미 전송 정보에 해당 Pallet ID가 존재합니다."
                                        return;
                                    }
                                }
                            }

                            //Tray 정보 조회 함수 호출
                            SelectTrayInfo(sPalletid);

                            dtpShipDate.Text = Util.NVC(dsGet.Rows[0]["SHIPDATE_SCHEDULE"]);
                            // 출하 예정일 확정을 위한 코드
                            //string shipdate = "";
                            //if (dsGet.Rows.Count > 0)
                            //{
                            //    Shipdate_Schedule = Util.NVC(dsGet.Rows[0]["SHIPDATE_SCHEDULE"]);
                            //}
                            //else
                            //{
                            //    Shipdate_Schedule = "";
                            //}


                            //for (int i = 0; i < dsGet.Rows.Count; i++)
                            //{
                            //    shipdate = dsGet.Rows[i]["SHIPDATE_SCHEDULE"].ToString();
                            //    if (shipdate != Shipdate_Schedule)
                            //    {
                            //        Shipdate_Schedule = "";
                            //        i = dsGet.Rows.Count;
                            //        //dtpShipDate.Visibility = Visibility.Visible;
                            //        //lblShipDate.Visibility = Visibility.Visible;

                            //    }
                            //}

                            //Lot 타입 설비 5개 이상 전송항목에 들어가는지 체크하기 위한 변수
                            int Lot_Type_Check = 0;
                            //전송정보 시트 로우수 다시 체크
                            int secTrayRows = dgTray.GetRowCount();
                            //Lot 타입 설비 5개 이상 전송항목에 들어가는지 체크하기 위한 루프 문(sheet 체크)
                            for (int i = 0; i < secTrayRows; i++)
                            {
                                if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ)   // "T"
                                {
                                    Lot_Type_Check++;
                                }
                            }

                            //if (chkPalletQty.IsChecked == false && Lot_Type_Check > 5)
                            //{
                            //    int n = secTrayRows - Lot_Type_Check + 5;
                            //    for (int i = n; i < secTrayRows; i++)
                            //    {
                            //        //PALLET ID 1개 클릭시 여러개의 PALLETID가 전송될 경우 중복값 삭제
                            //        //GridUtil.RemoveRow(sprTray, n);

                            //        // 선택된 행 삭제
                            //        dgTray.IsReadOnly = false;
                            //        dgTray.RemoveRow(n);
                            //        dgTray.IsReadOnly = true;

                            //    }
                            //    //PALLET ID 1개 클릭시 여러개의 PALLETID가 전송될 경우 중복값 삭제
                            //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");

                            //    DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                            //    return;
                            //}
                        }
                        else
                        {
                            int m = 0;
                            for (int i = 0; i < dgTray.GetRowCount(); i++)
                            {
                                if (sPalletid == Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value))
                                {
                                    m++;
                                }

                            }
                            if (m != 0)
                            {
                                DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);
                                Util.AlertInfo("SFU2015"); //"해당 PALLETID를 전송정보 시트에서 삭제해 주세요."
                                return;
                            }
                        }

                        //sprTray.SetViewportTopRow(0, sprTray.ActiveSheet.RowCount - 1);
                        // 스캔된 마지막 셀이 바로 보이도록 스프레드 스크롤 하단으로 이동
                        if (dgTray.GetRowCount() > 0)
                            dgTray.ScrollIntoView(dgTray.GetRowCount() - 1, 0);


                        int iPallet_Cnt = 0;
                        int iCell_Cnt = 0;

                        for (int i = 0; i < dgTray.GetRowCount(); i++)
                        {
                            iPallet_Cnt++;
                            iCell_Cnt += Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                        }

                        txtSelPalletQty.Value = iPallet_Cnt;
                        txtSelCellQty.Value = iCell_Cnt;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    dgPalletInfo.CurrentRow = null;
                }
            }

        }

        /// <summary>
        /// 전송정보 삭제버튼 클릭시 동작하는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            string sPalletid = Util.NVC(dgTray.GetCell(iRow, dgTray.Columns["PALLETID"].Index).Value);
            //삭제시 Pallet 조회 시트상 해당 PalletID 체크박스 해제
            for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
            {
                if (sPalletid == Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value))
                {
                    DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                }
            }
            // 선택된 행 삭제
            dgTray.IsReadOnly = false;
            dgTray.RemoveRow(iRow);
            dgTray.IsReadOnly = true;

            //string strQty = GridUtil.GetValue(sprTray, e.Row, "QTY").ToString();
            //if (dgTray.GetRowCount() <= 0)
            //{
            //    // 행이 존재하지 않으니 객체 초기화해서 차후 에러 방지함
            //    isDataTable = new DataTable();
            //}
            //else
            //{
            //    // 행 삭제되었으니 아래 로직 다시 수행함
            //    // 스프레드의 Row만큼 반복하면서 '번호' 입력
            //    for (int lsCount = 0; lsCount < dgTray.GetRowCount(); lsCount++)
            //    {
            //        sprTray.ActiveSheet.Cells[lsCount, 0].Text = ConvertUtil.ToString(lsCount + 1);
            //    }
            //}

            //isCellQty = isCellQty - ConvertUtil.ToInt32(strQty);

            //if (dgTray.GetRowCount() == 0)
            //{
            //    dtpShipDate.Visibility = Visibility.Hidden;
            //    lblShipDate.Visibility = Visibility.Hidden;
            //}

            int iPallet_Cnt = 0;
            int iCell_Cnt = 0;

            for (int i = 0; i < dgTray.GetRowCount(); i++)
            {
                iPallet_Cnt++;
                iCell_Cnt += Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
            }

            txtSelPalletQty.Value = iPallet_Cnt;
            txtSelCellQty.Value = iCell_Cnt;
        }


        private bool ScanPalletId(string sPalletId)
        {
            if (!string.IsNullOrEmpty(sPalletId))
            {
                // Pallet Lot
                string lsScanID = sPalletId.Substring(0, 1);

                // Pallet이 아니라면, 아래 로직 수행하지 않음
                if (lsScanID != "P")
                {
                    //스캔된 ID는 PalletID  가 아닙니다. >> 스캔된 ID는 PalletID / TrayID 가 아닙니다.
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1688"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                   Util.MessageInfo("SFU1688", (result) =>
                   {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletID.Focus();
                            txtPalletID.SelectAll();
                        }
                    });
                    return false;
                }
                else
                {
                    int trayRows = 0;
                    trayRows = dgTray.GetRowCount();
                    int eCnt = 0;

                    //전송 정보 중복 여부 체크
                    if (trayRows != 0)
                    {
                        for (int i = 0; i < trayRows; i++)
                        {
                            if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value) == sPalletId)
                            {
                                eCnt++;
                            }
                        }

                        if (eCnt != 0)
                        {
                            //이미 전송 정보에 해당 Pallet ID가 존재합니다.
                           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1776"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                           Util.MessageInfo("SFU1776", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID.Focus();
                                    txtPalletID.SelectAll();
                                }
                            });
                            return false;
                        }
                        // 외뢰한 시간을 체크하여 5분전에 의뢰한 Pallet ID는 재의뢰할 수 없도록 함.
                        // 서용수님 요청사항.
                        if (Check2QAREQ(sPalletId) == "NG")
                        {
                            //5분 전에 의뢰된 PALLET ID 입니다.
                     //       LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1301"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                     Util.MessageConfirm("SFU1301", (result) =>
                     {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID.Focus();
                                    txtPalletID.SelectAll();
                                }
                            });
                            return false;
                        }

                    }

                    SelectTrayInfo(sPalletId);

                    if (dsGet.Rows.Count == 0)
                    {
                        //이미 검사 의뢰 되었거나 PALLET의 정보를 찾을 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3163"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtPalletID.Focus();
                                txtPalletID.SelectAll();
                            }
                        });
                        return false;
                    }

                    // 출하 예정일 확정을 위한 코드

                    dtpShipDate.Text = Util.NVC(dsGet.Rows[0]["SHIPDATE_SCHEDULE"]);

                    //Lot 타입 설비 5개 이상 전송항목에 들어가는지 체크하기 위한 변수
                    int Lot_Type_Check = 0;
                    //전송정보 시트 로우수 다시 체크
                    int secTrayRows = dgTray.GetRowCount();
                    //Lot 타입 설비 5개 이상 전송항목에 들어가는지 체크하기 위한 루프 문(sheet 체크)
                    for (int i = 0; i < secTrayRows; i++)
                    {
                        if (Util.NVC(dgTray.GetCell(i, dgTray.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ)   // "T"
                        {
                            Lot_Type_Check++;

                        }
                    }

                    txtPalletID.Text = "";
                }

            }

            if (dgTray.GetRowCount() > 0)
                dgTray.ScrollIntoView(dgTray.GetRowCount() - 1, 0);

            int iPallet_Cnt = 0;
            int iCell_Cnt = 0;

            for (int i = 0; i < dgTray.GetRowCount(); i++)
            {
                iPallet_Cnt++;
                iCell_Cnt += Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
            }

            txtSelPalletQty.Value = iPallet_Cnt;
            txtSelCellQty.Value = iCell_Cnt;

            return true;
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScanPalletId(getPalletBCD(txtPalletID.Text.Trim()));  // 팔레트바코드id -> boxid
            }
            //sprTray.SetViewportTopRow(0, sprTray.ActiveSheet.RowCount - 1);

        }


        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtPalletID2.Text))
                    {
                        // Pallet Lot
                        string lsScanID = txtPalletID2.Text.ToUpper().Substring(0, 1);

                        // Pallet이 아니라면, 아래 로직 수행하지 않음
                        if (lsScanID != "P")
                        {
                            //스캔된 ID는 PalletID  가 아닙니다. >> 스캔된 ID는 PalletID / TrayID 가 아닙니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1688"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID2.Focus();
                                    txtPalletID2.SelectAll();
                                }
                            });
                            return;

                        }
                        else
                        {
                            bool bIN = false;
                            for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                            {
                                if (txtPalletID2.Text.ToUpper().Trim() == (Util.NVC(dgQAReqHis.GetCell(i,dgQAReqHis.Columns["PALLETID"].Index).Value)))
                                {
                                    bIN = true;
                                    //sprQASelect.SetViewportTopRow(0, i);
                                    dgQAReqHis.ScrollIntoView(i, 0);
                                    //이미 조회된 Pallet ID 입니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtPalletID2.Focus();
                                            txtPalletID2.SelectAll();
                                        }
                                    });
                                    return;
                                }

                            }
                            if (!bIN)
                            {
                                selectQAHistInfo_BY_PALLETID(getPalletBCD(txtPalletID2.Text.Trim()));  // 팔레트바코드ID -> boxid
                            }
                        }
                    }
                    txtPalletID2.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }


        //private void chkPalletQty_Click(object sender, RoutedEventArgs e)
        //{
        //    if (chkPalletQty.IsChecked == true)
        //    {
        //        Util.AlertInfo("체크할 경우 PALLET 수량 5개 체크에서 제외시킵니다. 주의하세요!!!");
        //    }
        //}

        private void chkLine_Click(object sender, RoutedEventArgs e)
        {
            if (chkLine.IsChecked == true)
            {
                Util.AlertInfo("SFU1929"); //"체크할 경우 라인 혼입이 될 수 있습니다. 주의하세요!!!"
            }
        }

        private void btnShtInit_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    isQty = 0;

                    dgTray.ItemsSource = null;
                    //dtpShipDate.Visibility = Visibility.Hidden;
                    //lblShipDate.Visibility = Visibility.Hidden;

                    for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", false);
                    }

                    int iPallet_Cnt = 0;
                    int iCell_Cnt = 0;

                    for (int i = 0; i < dgTray.GetRowCount(); i++)
                    {
                        iPallet_Cnt++;
                        iCell_Cnt += Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                    }

                    txtSelPalletQty.Value = iPallet_Cnt;
                    txtSelCellQty.Value = iCell_Cnt;
                }

            });

           
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgQAReqHis_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (dgQAReqHis.CurrentRow == null || dgQAReqHis.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgQAReqHis.CurrentColumn.Name == "CHK")
                {
                    if (Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                    {
                        DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "CHK", true);
                    }


                    if (rdoUseY.IsChecked == false)
                    {
                        //출하예정일 일치여부 체크용 변수
                        int shipCnt = 0;
                        //체크박스 체크 수 체크
                        int checkCnt = 0;
                        //출하예정일 비교를 위한 변수
                        string copy_shipdate = Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value);
                        string qaId1 = Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["OQC_INSP_REQ_ID"].Index).Value);

                        //체크박스 체크여부 체크
                        if (Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                            {
                                string qaId2 = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["OQC_INSP_REQ_ID"].Index).Value);
                                // 같은 의뢰ID를 가진 PALLETID는 동시에 체크되어 푸른 색으로 표시
                                if (qaId1 == qaId2)
                                {
                                    //sprQASelect_Sheet1.Rows[i].ForeColor = Color.Blue;
                                    //sprQASelect_Sheet1.Rows[i].Font = new Font("", 9, FontStyle.Bold);
                                    DataTableConverter.SetValue(dgQAReqHis.Rows[i].DataItem, "SEND_FLAG", "Y");
                                    DataTableConverter.SetValue(dgQAReqHis.Rows[i].DataItem, "CHK", true);

                                }

                            }
                            //sprQASelect_Sheet1.Rows[e.Row].ForeColor = Color.Blue;
                            //sprQASelect_Sheet1.Rows[e.Row].Font = new Font("", 9, FontStyle.Bold);
                            DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "SEND_FLAG", "Y");

                            //string sTmpShipDate = Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value);

                            //try
                            //{
                            //    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                            //    {
                            //        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                            //        {
                            //            checkCnt++;
                            //            if (sTmpShipDate == Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value))
                            //            {
                            //                shipCnt++;
                            //            }
                            //            else
                            //            {
                            //                if (DateTime.Parse(copy_shipdate) < DateTime.Parse(Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value)))
                            //                {
                            //                    copy_shipdate = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value);
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    Util.AlertInfo(ex.Message);
                            //    return;
                            //}

                            //if (shipCnt != checkCnt)
                            //{

                            //    Shipdate_Schedule2 = copy_shipdate;
                            //}
                            //else
                            //{
                            //    Shipdate_Schedule2 = sTmpShipDate;
                            //}

                        }
                        else
                        {
                            //sprQASelect_Sheet1.Rows[e.Row].ForeColor = Color.Black;
                            //sprQASelect_Sheet1.Rows[e.Row].Font = new Font("", 9, FontStyle.Regular);
                            DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "SEND_FLAG", "N");

                        }
                    }
                    else
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(dgQAReqHis.CurrentRow.Index, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {

                            //sprQASelect_Sheet1.Rows[e.Row].ForeColor = Color.OrangeRed;
                            //sprQASelect_Sheet1.Rows[e.Row].Font = new Font("", 9, FontStyle.Strikeout);
                            DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "DEL_FLAG", "D");
                        }
                        else
                        {
                            //sprQASelect_Sheet1.Rows[e.Row].ForeColor = Color.Black;
                            //sprQASelect_Sheet1.Rows[e.Row].Font = new Font("", 9, FontStyle.Regular);
                            DataTableConverter.SetValue(dgQAReqHis.Rows[dgQAReqHis.CurrentRow.Index].DataItem, "DEL_FLAG", "A");
                        }
                    }
                    /* 2012.02.13 Add By JP Kim ********************************************************************************/
                    /* 선택된 Pallet 수량 및 Cell 수량 표시 위함.                                                              */
                    int iPallet_Cnt = 0;
                    int iCell_Cnt = 0;

                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            iPallet_Cnt++;
                            iCell_Cnt += Util.NVC_Int(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["QTY"].Index).Value);
                        }
                    }

                    txtPalletQty2.Value = iPallet_Cnt;
                    txtCellQty2.Value = iCell_Cnt;
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
            finally
            {
                dgQAReqHis.CurrentRow = null;
            }

        }

        private void rdoUseY_Click(object sender, RoutedEventArgs e)
        {
            if (rdoUseY.IsChecked == true)
            {
                btnSave.IsEnabled = true;
                btnRePrint.IsEnabled = false;
                btnReMail.IsEnabled = false;
                //라디오 버튼 클릭과 동시에 조건 별로 조회
                //selectQAHistInfo(dtFrom2.Value, dtTo2.Value, cboLine2.SelectedValue.ToString(), cboModel.SelectedValue.ToString());
                GetQAReqHis();


                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
                        //sprQASelect_Sheet1.Rows[i].Visible = true;
                        dgQAReqHis.Rows[i].Visibility = Visibility.Visible;
                        //sprQASelect_Sheet1.Rows[i].ForeColor = Color.OrangeRed;
                        //sprQASelect_Sheet1.Rows[i].Font = new Font("", 9, FontStyle.Strikeout);
                    }

                }

            }
            else if (rdoUseN.IsChecked == true)
            {
                btnSave.IsEnabled = false;
                btnRePrint.IsEnabled = true;
                btnReMail.IsEnabled = true;
                //selectQAHistInfo(dtFrom2.Value, dtTo2.Value, cboLine2.SelectedValue.ToString(), cboModel.SelectedValue.ToString());
                GetQAReqHis();

                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
                        //sprQASelect_Sheet1.Rows[i].Visible = false;
                        dgQAReqHis.Rows[i].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// QA 검사의뢰 메일 송부
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQAMail_Click(object sender, RoutedEventArgs e)
        {

            if (dgTray.GetRowCount() <= 0)
            {
                Util.AlertInfo("SFU2015"); //"전송정보 없음 검사의뢰PALLET을 체크해 주세요."
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }


            shipdt = dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd");
            QAMailSend();
        }

        #endregion

        /// <summary>
        /// Tag 재발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            int t = 0; // 설비 구성 PALLETID는 5개 까지만 전송하도록 설비구성 PALLETID 개수 체크용

            ////string sUserID = Util.NVC(cboUser2.SelectedValue);
            ////if (sUserID == string.Empty || sUserID == "SELECT")
            ////{
            ////    Util.AlertInfo("SFU1760"); //"의뢰인을 선택해 주세요."
            ////    return;
            ////}

            // 데이터 존재여부 확인
            if (dgQAReqHis.GetRowCount() > 0)
            {

                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ && Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                    {
                        t++;
                    }
                }

                int Value_Cnt = t / 5;
                int Remainder_Cnt = t % 5;
                int Sheet_Cnt = Value_Cnt + (Remainder_Cnt == 0 ? 0 : 1);

                //pallet 수량 체크박스가 해제되어있고
                //if (chkPalletQty.IsChecked == false && t > 5)
                //{
                //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                //    return;
                //}
                //else if (t > 0)

                if (t > 0)
                {

                    if (Shipdate_Schedule2 == "" || isQty != 0)
                    {
                        Shipdate_Schedule2 = Convert.ToDateTime(dtpShipDate2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                    }

                    //출하예정일이 {%1}이면 확인버튼을 눌러 주세요.
                    object[] parameters = new object[1];
                    parameters[0] = Shipdate_Schedule2.Substring(0, 10);

                    Util.MessageConfirm("SFU3252", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            QAReTagPrint(t, Sheet_Cnt);
                        }
                        else
                        {
                            isQty++;
                            //dtpShipDate2.Visibility = Visibility.Visible;
                            //lblShipDate2.Visibility = Visibility.Visible;
                            return;
                        }
                    }, parameters);
                    
                    //string sMsg = "출하예정일이 " + Shipdate_Schedule2.Substring(0, 10) + " 이면 확인버튼을 눌러 주세요.";
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //{
                    //    if (result == MessageBoxResult.OK)
                    //    {
                    //        QAReTagPrint(t, Sheet_Cnt);
                    //    }
                    //    else
                    //    {
                    //        isQty++;
                    //        //dtpShipDate2.Visibility = Visibility.Visible;
                    //        //lblShipDate2.Visibility = Visibility.Visible;
                    //        return;
                    //    }
                    //});
                }
                else
                {
                    int checkedCnt = 0;
                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i,dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            checkedCnt++;
                        }
                    }

                    if (checkedCnt == 0)
                    {
                        Util.AlertInfo("SFU1942"); //"태그에 전송할 정보가 없습니다."
                        return;
                    }
                    else
                    {
                        Util.AlertInfo("SFU1943"); //"태그에 전송할 정보가 없습니다. 선택 Pallet의 LotType이 설비구성인지 확인하세요."
                        return;
                    }
                }

                
            }
        }

        /// <summary>
        /// 메일 재전송
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReMail_Click(object sender, RoutedEventArgs e)
        {
            int t = 0; // 설비 구성 PALLETID는 5개 까지만 전송하도록 설비구성 PALLETID 개수 체크용

            string sUserID = Util.NVC(txtWorker.Tag); // LoginInfo.USERID;//Util.NVC(cboUser2.SelectedValue);
            if (sUserID == string.Empty || sUserID == "SELECT")
            {
                Util.AlertInfo("SFU1760"); //"의뢰인을 선택해 주세요."
                return;
            }

            try
            {
                // 데이터 존재여부 확인
                if (dgQAReqHis.GetRowCount() > 0)
                {

                    {
                        t = 0; // 전송여부 A인 항목 개수 체크
                        for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                        {
                            if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                            {
                                t++;
                            }
                        }
                        //전송항목 존재여부 체크
                        if (t <= 0)
                        {
                            Util.AlertInfo("SFU2015"); //"전송정보 없음 검사의뢰PALLET을 체크해 주세요."
                            return;
                        }

                        //if (Shipdate_Schedule2 == "")// || isQty != 0)
                        //{
                        //    Shipdate_Schedule2 = Convert.ToDateTime(dtpShipDate2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                        //}
                        //else
                        //{
                        //    dtpShipDate2.Text = Shipdate_Schedule2;
                        //}


                        shipdt2 = dtpShipDate2.SelectedDateTime.ToString("yyyy-MM-dd");

                        object[] parameters = new object[1];
                        parameters[0] = Shipdate_Schedule2.Substring(0, 10);

                        //출하예정일이 {%1}이면 확인 후 전송하시겠습니까?
                        Util.MessageConfirm("SFU3253", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReMailSend(t);
                            }
                        }, parameters);

                        //string sMsg = "출하예정일이 " + Shipdate_Schedule2.Substring(0, 10) + " 이면 확인 후 전송하시겠습니까?";
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //{
                        //    if (result == MessageBoxResult.OK)
                        //    {
                        //        ReMailSend(t);
                        //    }
                        //    //else
                        //    //{
                        //    //    isQty++;
                        //    //    //dtpShipDate2.Visibility = Visibility.Visible;
                        //    //    //lblShipDate2.Visibility = Visibility.Visible;
                        //    //    return;
                        //    //}
                        //});
                    }


                    //for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    //{
                    //    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ  && Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                    //    {
                    //        t++;
                    //    }
                    //}

                    //int Value_Cnt = t / 5;
                    //int Remainder_Cnt = t % 5;
                    //int Sheet_Cnt = Value_Cnt + (Remainder_Cnt == 0 ? 0 : 1);

                    ////pallet 수량 체크박스가 해제되어있고
                    ////if (chkPalletQty.IsChecked == false && t > 5)
                    ////{
                    ////    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                    ////    return;
                    ////}
                    ////else if (t > 0)

                    //if (t > 0)
                    //{
                    //    if (Shipdate_Schedule2 == "")// || isQty != 0)
                    //    {
                    //        Shipdate_Schedule2 = Convert.ToDateTime(dtpShipDate2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                    //    }
                    //    else if (Shipdate_Schedule2 != "")
                    //    {
                    //        dtpShipDate2.Text = Shipdate_Schedule;
                    //    }

                    //    shipdt2 = Shipdate_Schedule2;

                    //    string sMsg = "출하예정일이 " + Shipdate_Schedule2.Substring(0, 10) + " 이면 확인 후 전송하시겠습니까?";
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //    {
                    //        if (result == MessageBoxResult.OK)
                    //        {
                    //            QAReMailSend(t, Sheet_Cnt);
                    //        }
                    //        else
                    //        {
                    //            isQty++;
                    //            //dtpShipDate2.Visibility = Visibility.Visible;
                    //            //lblShipDate2.Visibility = Visibility.Visible;
                    //            return;
                    //        }
                    //    });
                    //}
                    //else
                    //{
                    //    t = 0; // 전송여부 A인 항목 개수 체크
                    //    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    //    {
                    //        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                    //        {
                    //            t++;
                    //        }
                    //    }
                    //    //전송항목 존재여부 체크
                    //    if (t <= 0)
                    //    {
                    //        Util.AlertInfo("SFU2015"); //"전송정보 없음 검사의뢰PALLET을 체크해 주세요."
                    //        return;
                    //    }

                    //    if (Shipdate_Schedule2 == "")// || isQty != 0)
                    //    {
                    //        Shipdate_Schedule2 = Convert.ToDateTime(dtpShipDate2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                    //    }
                    //    else
                    //    {
                    //        dtpShipDate2.Text = Shipdate_Schedule2;
                    //    }


                    //    shipdt2 = Shipdate_Schedule2;

                    //    string sMsg = "출하예정일이 " + Shipdate_Schedule2.Substring(0, 10) + " 이면 확인 후 전송하시겠습니까?";
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //    {
                    //        if (result == MessageBoxResult.OK)
                    //        {
                    //            ReMailSend(t);
                    //        }
                    //        else
                    //        {
                    //            isQty++;
                    //            //dtpShipDate2.Visibility = Visibility.Visible;
                    //            //lblShipDate2.Visibility = Visibility.Visible;
                    //            return;
                    //        }
                    //    });
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }   
        }
    
        #region Mehod


        /// <summary>
        /// Pallet 정보 조회
        /// </summary>
        private void GetPalletInfo()
        {
            try
            {
                //string sBoxStat = Util.NVC(cboPackStatus.SelectedValue);
                //if (sBoxStat == "" || sBoxStat == "SELECT") sBoxStat = null;

                string sPackStat = Util.NVC(cboShipStatus.SelectedValue);
                if (sPackStat == "" || sPackStat == "SELECT") sPackStat = null;

                string sLotType = Util.NVC(cboLottype.SelectedValue);
                if (sLotType == "" || sLotType == "SELECT")
                {
                    // CSR 적용으로 ALL 추가함 -- (2016/11/02)
                    //Util.AlertInfo("타입을 선택하세요.");
                    //return;
                    sLotType = null;
                }

                if (cboArea.SelectedValue.ToString() == "SELECT" || cboArea.SelectedValue.ToString() == "")
                {
                    // 동을 선택하세요
                    Util.MessageInfo("SFU1499");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
               // RQSTDT.Columns.Add("PALLETID", typeof(string)); -- 조회대상이 아님(스캔시 사용함)
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";  //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                //dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue) == "" ? null : Util.NVC(cboEquipment.SelectedValue);
                // dr["PALLETID"] = txtPalletID.Text.Trim() == "" ? null : txtPalletID.Text.Trim();
                //dr["BOXSTAT"] = sBoxStat;
                dr["BOX_RCV_ISS_STAT_CODE"] = sPackStat;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                if (!string.IsNullOrWhiteSpace(cboEqpt.SelectedValue.ToString())) dr["EQPT_ID"] = cboEqpt.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPalletInfo, dtResult, FrameOperation,true);

                int lSumPallet = 0;
                int lSumCell = 0;

                for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
                {
                    lSumPallet = lSumPallet + 1;

                    lSumCell = lSumCell + Util.NVC_Int(dgPalletInfo.GetCell(lsCount, dgPalletInfo.Columns["QTY"].Index).Value);
                }

                txtPalletQty.Value = lSumPallet;
                txtCellQty.Value = lSumCell;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 검사의뢰 이력조회
        /// </summary>
        private void GetQAReqHis()
        {

            string sModel = Util.NVC(cboModelLot2.SelectedValue);
            if (sModel == "" || sModel == "SELECT") sModel = null;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROMDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TODTTM", typeof(DateTime));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));
                RQSTDT.Columns.Add("JUDG_VALUE", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_ID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00"; ;  //Convert.ToDateTime(dtpDateFrom2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TODTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; // Convert.ToDateTime(dtpDateTo2.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = sAREAID2;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment2.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment2.SelectedValue);
                dr["MODELID"] = sModel;
                dr["JUDG_VALUE"] = Util.NVC(cboJudge_Value.SelectedValue) == "" ? null : Util.NVC(cboJudge_Value.SelectedValue);
                dr["PACK_WRK_TYPE_CODE"] = Util.NVC(cboLottype2.SelectedValue) == "" ? null : Util.NVC(cboLottype2.SelectedValue);
                dr["EQPT_ID"] = Util.NVC(cboEqpt2.SelectedValue) == "" ? null : Util.NVC(cboEqpt2.SelectedValue);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QAREQ_HIS_CP", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgQAReqHis, dtResult, FrameOperation,true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PalletID"></param>
        /// <returns></returns>
        private string Check2QAREQ(string sPalletID)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QAREQ_CHECK_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["JUDGE"]);
                }
                else
                {
                    return "GOOD";
                }
            }
            catch (Exception ex)
            {
                return "NG";
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }


        /// <summary>
        /// PalletID로 Tray 정보 조회하는 함수
        /// </summary>
        /// <param name="palletID">Pallet 스프레드에서 선택한 셀의 LotID</param>
        private void SelectTrayInfo(string sPalletID)
        {

            if (cboArea.SelectedValue.ToString() == "SELECT" || cboArea.SelectedValue.ToString() == "")
            {
                // 동을 선택하세요
                Util.MessageInfo("SFU1499");
                return;
            }
            //DA_PRD_SEL_OQC_PLT_CP
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dgTray.GetRowCount() == 0)
                {
                    ////dgTray.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgTray, dtResult, FrameOperation,true);
                    dsGet = dtResult;
                }
                else
                {
                    dsGet = dtResult;
                    
                    if (dtResult.Rows.Count > 0)
                    {
                        //전송정보 로우 수 체크(테이블 결합 루프용)
                        DataTable DT = DataTableConverter.Convert(dgTray.ItemsSource);

                        DataRow drGet = dsGet.Rows[0];
                        DataRow newDr = DT.NewRow();
                        foreach (DataColumn col in dsGet.Columns)
                        {
                            newDr[col.ColumnName] = drGet[col.ColumnName];
                        }
                        DT.Rows.Add(newDr);
                        ////dgTray.ItemsSource = DataTableConverter.Convert(DT);
                        Util.GridSetData(dgTray, DT, FrameOperation,true);
                    }

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }


        private void QAMailSend()
        {
            string sUserID = string.Empty;

            try
            {

                //string shipdate = shipdt;
                //string modelid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["MODELID"].Index).Value);
                //string Lineid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["LINEID"].Index).Value);

                //sUserID = LoginInfo.USERID;//Util.NVC(cboUser.Text);
                //// paleltID 받을 배열 생성
                //string[] palletID = null;

                //int k = 0; //설비구성 PALLETID가 아닐 경우 For문이 추가로 돌도록 설정
                //int t = 0; // 설비 구성 PALLETID는 5개 까지만 전송하도록 설비구성 PALLETID 개수 체크용

                //// 호기명 받기 및 중복검사
                //object[] Line = null;
                //object[] LineName = null;

                //// Model명 받기 및 중복 검사
                //object[] Model = null;

                //// 검사요청일
                //string Req_Date = "";

                // 태그 Sheet 생성
                //frmPopupQATestReq palletTag = new frmPopupQATestReq(this, 1);

                // 출하 의뢰서 발행을 하지 않는다. 

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("SHOPID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHOP_OCV2_QA_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                    int AgingTime = Util.StringToInt(SearchResult.Rows[0]["AGINGTIME"].ToString());

                if (SearchResult.Rows[0]["RESULT"].ToString().Equals("Y"))
                {
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.Columns.Add("PALLETID", typeof(string));

                    for (int i = 0; i < dgTray.GetRowCount(); i++)
                    {
                        DataRow dr1 = RQSTDT1.NewRow();

                        dr1["PALLETID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);

                        RQSTDT1.Rows.Add(dr1);
                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OCV2_LOT_INFO_FOR_PERIOD", "RQSTDT", "RSLTDT", RQSTDT1);
                        if (Result.Rows.Count > 0)
                        {
                            DateTime Ocv2date = Util.StringToDateTime(Result.Rows[0]["OCV_DTTM"].ToString());

                            Ocv2date = Ocv2date.AddDays(AgingTime);
                            // DateTime ShipDate = Util.StringToDateTime(dgTray.GetCell(i, dgTray.Columns["SHIPDATE_SCHEDULE"].Index).Value.ToString());
                            // DateTime ShipDate = DateTime.Now;
                            DateTime ShipDate = Util.StringToDateTime(dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd"));
                            if (ShipDate < Ocv2date)
                            {

                                //SFU3729 팔레트[%1]의 OCV2 Aging시간[%2]이 출하예정일[%3]을 경과하였습니다.
                                string message = MessageDic.Instance.GetMessage("SFU3729");
                                object[] parameters = { dr1["PALLETID"], Ocv2date.ToString(), ShipDate.ToString() };

                                message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                                if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
                                {
                                    for (int j = 0; j < parameters.Length; j++)
                                    {
                                        message = message.Replace("%" + (j + 1), parameters[j].ToString());
                                    }
                                }
                                System.Windows.Forms.MessageBox.Show(message, "", System.Windows.Forms.MessageBoxButtons.OK);

                                //SFU3728 팔레트[%1]는 [%2]이 후 QA검사 의뢰 가능합니다.	
                                //string message = MessageDic.Instance.GetMessage("SFU3728");
                                //object[] parameters = {dr1["PALLETID"], Ocv2date.ToString()};

                                //message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                                //if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
                                //{
                                //    for (int j = 0; j < parameters.Length; j++)
                                //    {
                                //        message = message.Replace("%" + (j+ 1), parameters[j].ToString());
                                //    }
                                //}
                                //System.Windows.Forms.MessageBox.Show(message, "", System.Windows.Forms.MessageBoxButtons.OK);
                                return;
                            }
                            else
                            {
                                MailSend_Only();
                            }
                        }
                        else
                        {
                            //SFU3726
                            Util.MessageValidation("SFU3726"); //
                            return;
                        }

                    }
                    //DataTable RQSTDT1 = new DataTable();
                    //RQSTDT1.Columns.Add("PALLETID", typeof(string));

                    //DataRow dr1 = RQSTDT1.NewRow();
                    //dr1["PALLETID"] = _sPalletID;

                    //RQSTDT1.Rows.Add(dr1);

                    //DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OCV2_LOT_INFO_FOR_PALLET", "RQSTDT", "RSLTDT", RQSTDT1);

                    //ShipDate_Schedule = Util.StringToDateTime(Result.Rows[0]["OCV_DTTM"].ToString());

                    //ShipDate_Schedule = Util.StringToDateTime(ShipDate_Schedule.AddDays(AgingTime).ToString("yyyy-MM-dd"));


                }
                else
                {
                    MailSend_Only();
                }




              

                //for (int i = 0; i < dgTray.GetRowCount(); i++)
                //{
                //    if (dgTray.GetCell(i + k, dgTray.Columns["LOT_TYPE"].Index).Value.ToString() == Pack_Wrk_Type.EQ)   // "T"
                //    {
                //        t++;
                //    }
                //}

                //if (chkPalletQty.IsChecked == false && t > 5)
                //{
                //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                //    return;
                //}
                //else if (t <= 0)

                //if (t <= 0)
                //{

                //    // 해당 Model 의 Pallet 는 TAG 는 출력하지 않습니다. 출하검사 의뢰 메일만 송부하시겠습니까?
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 Model 의 Pallet 는 TAG 는 출력하지 않습니다. 출하검사 의뢰 이력만 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            MailSend_Only();
                //        }
                //        else
                //        {
                //            return;
                //        }
                //    });

                //}
                //else
                //{

                //    //palletID 배열 생성
                //    palletID = new string[t];
                //    //Line 배열 생성
                //    Line = new object[t];
                //    LineName = new object[t];
                //    //Model 배열 생성
                //    Model = new object[t];

                //    int Value_Cnt = t / 5;
                //    int Remainder_Cnt = t % 5;
                //    int Sheet_Cnt = Value_Cnt + (Remainder_Cnt == 0 ? 0 : 1);

                //    // Tag 발행용 DataTable 생성
                //    DataTable dtQAReqTag = new DataTable();
                //    dtQAReqTag = CreateDT_QAReqTag(dtQAReqTag, Sheet_Cnt);

                //    if (Shipdate_Schedule != "" || Shipdate_Schedule != null)
                //    {

                //        Shipdate_Schedule = Shipdate_Schedule.Substring(0, 4) + "년 "
                //                      + Shipdate_Schedule.Substring(5, 2) + "월 "
                //                      + Shipdate_Schedule.Substring(8, 2) + "일";
                //    }

                //    for (int i = 0; i < t; i++)
                //    {
                //        //태그에 설비구성인 PALLETID만 받도록 처리
                //        if (dgTray.GetCell(i + k, dgTray.Columns["LOT_TYPE"].Index).Value.ToString() == Pack_Wrk_Type.EQ)   // "T"
                //        {
                //            palletID[i] = dgTray.GetCell(i + k, dgTray.Columns["PALLETID"].Index).Value.ToString(); //PALLETID 받기
                //            Line[i] = dgTray.GetCell(i + k, dgTray.Columns["LINEID"].Index).Value.ToString(); //Line 받기
                //            LineName[i] = dgTray.GetCell(i + k, dgTray.Columns["EQSGNAME"].Index).Value.ToString();
                //            Model[i] = dgTray.GetCell(i + k, dgTray.Columns["MODELID"].Index).Value.ToString().Substring(0, 3); //Model 받기

                //            // 검사요청일 셋팅
                //            if (Req_Date == "" || Req_Date == null)
                //            {
                //                Req_Date = DateTime.Now.ToString().Substring(0, 4) + "년 " + DateTime.Now.ToString().Substring(5, 2) + "월 "
                //                         + DateTime.Now.ToString().Substring(8, 2) + "일";
                //            }
                //        }
                //        else
                //        {
                //            k++;
                //            i--;
                //        }
                //    }
                    
                //    //호기명 표시 (중복 제거 존재 Line 당 1개씩만)
                //    int s = dgTray.GetRowCount();
                //    int m = 0;
                //    int g = 0;
                //    for (int i = 0; i < s; i++)
                //    {
                //        int Index = i / 5;
                //        if (i == s - 1)
                //        {
                //            for (int n = 0; n < s; n++)
                //            {
                //                if (Line[i].ToString().Substring(0, 1) != "2")  // As-is: LINE 테이블 조립-활성화-포장-PACK 공정에 동일 라인 번호 부여됨... (일단은 그냥 그대로 둠)
                //                {
                //                    if (Line[i].ToString() == Line[n].ToString())
                //                    {
                //                        m++;
                //                    }
                //                }
                //                else
                //                {
                //                    if (Line[i].ToString().Substring(0, 1) == Line[n].ToString().Substring(0, 1))
                //                    {
                //                        g++;
                //                    }
                //                }
                //            }

                //        }
                //        if (i % 5 == 0)
                //        {
                //            //palletTag.sprTag.Sheets[Index].Cells[7, 2].Value = Line[i].ToString() + "호기 ";
                //            //palletTag.sprTag.Sheets[Index].Cells[25, 2].Value = palletTag.sprTag.Sheets[Index].Cells[7, 2].Value; //태그 아래쪽 호기명
                //            dtQAReqTag.Rows[Index]["LINEID"] = LineName[i].ToString();      //Line[i].ToString() + "호기 ";
                //            dtQAReqTag.Rows[Index]["LINEID2"] = LineName[i].ToString();     //Line[i].ToString() + "호기 ";
                //        }

                //    }


                //    if (chkLine.IsChecked == false && m != s && g != s)
                //    {
                //        Util.AlertInfo("SFU1479"); //"다른 생산라인 모델이 섞여있습니다.같은 생산라인 모델 만 전송 할 수 있습니다."
                //        return;
                //    }
                //    //전체 모델명 확인
                //    object[] AllModel = new object[dgTray.GetRowCount()];
                //    for (int i = 0; i < dgTray.GetRowCount(); i++)
                //    {
                //        AllModel[i] = dgTray.GetCell(i, dgTray.Columns["MODELID"].Index).Value.ToString().Substring(0, 3); //모든Model 받기

                //    }
                //    //모델 예외 존재시 에러창 처리
                //    for (int i = 0; i < dgTray.GetRowCount(); i++)
                //    {
                //        m = 0;
                //        for (int n = 0; n < dgTray.GetRowCount(); n++)
                //        {
                //            if (AllModel[i].ToString() == AllModel[n].ToString())
                //            {
                //                m++;
                //            }
                //        }
                //        if (m != dgTray.GetRowCount())
                //        {
                //            Util.AlertInfo("SFU1477"); //"다른 모델이 포함되어 있습니다. 한번에 한가지 모델만 전송 할 수 있습니다."
                //            return;
                //        }
                //    }
                //    //모델명 받기 및 중복 검사 

                //    for (int i = 0; i < Sheet_Cnt; i++)
                //    {
                //        //palletTag.sprTag.Sheets[i].Cells[7, 6].Value = Model[0].ToString(); //모델명 태그 시트에 입력
                //        //palletTag.sprTag.Sheets[i].Cells[25, 2].Value = palletTag.sprTag_Sheet1.Cells[7, 2].Value; //태그 아래쪽 호기명
                //        dtQAReqTag.Rows[i]["MODEL"] = Model[0].ToString();
                //        dtQAReqTag.Rows[i]["MODEL2"] = Model[0].ToString();
                //    }

                //    // 조립 LotID / 수량 저장을 위한 DataTable
                //    DataTable resultAssyLotIDs = new DataTable();

                //    // Tray _ MagazineID / 수량 저장을 위한 DataTable
                //    DataTable DT = new DataTable();

                //    // 개별 Pallet 구성
                //    // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                //    for (int i = 0; i < t; i++)
                //    {
                //        //if (chkPalletQty.IsChecked == true || t <= 5)
                //        //{
                //            resultAssyLotIDs = SearchAssyLot_8digit(palletID[i]);
                //            if (resultAssyLotIDs == null)
                //            {
                //                Util.AlertInfo(palletID[i] + "에 Lot 정보가 없습니다.");
                //                return;
                //            }

                //            dtQAReqTag = setTagLotId(dtQAReqTag, resultAssyLotIDs, i);  //lot별 수량 조회 합산 태그에 입력

                //        //}

                //        //else
                //        //{
                //        //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                //        //    return;
                //        //}
                //    }


                //    //Pallet
                //    int iCol = 0;
                //    int Old_Index = 0;
                //    //태그상 palletId 입력 (tag 시트 셀이 병합되 있는 관계로 3번째 PalletID 부터 코드 조작하여 입력)
                //    for (int i = 0; i < t; i++)
                //    {
                //        int Index = i / 5;

                //        if (Index != Old_Index || i == 0)
                //            iCol = 1;
                //        //1번째 palletID 입력
                //        if (iCol == 1)
                //        {
                //            dtQAReqTag.Rows[Index]["PALLET1_1"] = palletID[i];
                //            dtQAReqTag.Rows[Index]["PALLET2_1"] = palletID[i];
                //            iCol++;
                //        }
                //        //2번째 palletID 입력
                //        else if (iCol == 2)
                //        {
                //            dtQAReqTag.Rows[Index]["PALLET1_2"] = palletID[i];
                //            dtQAReqTag.Rows[Index]["PALLET2_2"] = palletID[i];
                //            iCol++;
                //        }
                //        //3번째 palletID 입력
                //        else if (iCol == 3)
                //        {
                //            dtQAReqTag.Rows[Index]["PALLET1_3"] = palletID[i];
                //            dtQAReqTag.Rows[Index]["PALLET2_3"] = palletID[i];
                //            iCol++;
                //        }
                //        //4번째 palletID 입력
                //        else if (iCol == 4)
                //        {
                //            dtQAReqTag.Rows[Index]["PALLET1_4"] = palletID[i];
                //            dtQAReqTag.Rows[Index]["PALLET2_4"] = palletID[i];
                //            iCol++;
                //        }
                //        //5번째 palletID 입력
                //       else if (iCol == 5)
                //        {
                //            dtQAReqTag.Rows[Index]["PALLET1_5"] = palletID[i];
                //            dtQAReqTag.Rows[Index]["PALLET2_5"] = palletID[i];
                //            iCol++;
                //        }
                //        Old_Index = Index;
                //    }

                //    for (int i = 0; i < Sheet_Cnt; i++)
                //    {
                //        //태그상 출하예정일 입력
                //        //palletTag.sprTag.Sheets[i].Cells[5, 3].Value = Shipdate_Schedule;
                //        //palletTag.sprTag.Sheets[i].Cells[23, 3].Value = Shipdate_Schedule;
                //        dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE"] = Shipdate_Schedule;
                //        dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE2"] = Shipdate_Schedule;

                //        //태그상 검사요청일 입력
                //        //palletTag.sprTag.Sheets[i].Cells[6, 3].Value = Req_Date;
                //        //palletTag.sprTag.Sheets[i].Cells[24, 3].Value = Req_Date;
                //        dtQAReqTag.Rows[i]["REQ_DATE"] = Req_Date;
                //        dtQAReqTag.Rows[i]["REQ_DATE2"] = Req_Date;

                //        //담당
                //        //palletTag.sprTag.Sheets[i].Cells[3, 1].Value = sUserID;
                //        //palletTag.sprTag.Sheets[i].Cells[21, 1].Value = sUserID;
                //        dtQAReqTag.Rows[i]["USERID"] = LoginInfo.USERNAME; // sUserID;
                //        dtQAReqTag.Rows[i]["USERID2"] = LoginInfo.USERNAME; //sUserID;
                //    }

                //    //// 태그 발행 창 화면에 띄움.
                //    //object[] Parameters = new object[2];
                //    //Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                //    //Parameters[1] = dtQAReqTag;

                //    //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                //    //C1WindowExtension.SetParameters(rs, Parameters);
                //    //rs.Show();


                //    LGC.GMES.MES.BOX001.Report_Multi_Cell rs = new LGC.GMES.MES.BOX001.Report_Multi_Cell();
                //    rs.FrameOperation = this.FrameOperation;

                //    if (rs != null)
                //    {
                //        // 태그 발행 창 화면에 띄움.
                //        object[] Parameters = new object[2];
                //        Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                //        Parameters[1] = dtQAReqTag;

                //        C1WindowExtension.SetParameters(rs, Parameters);

                //        rs.Closed += new EventHandler(wndQAMailSend_Closed);
                //        this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                //    }


                //}

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }


        private void wndQAMailSend_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Multi_Cell wndPopup = sender as LGC.GMES.MES.BOX001.Report_Multi_Cell;
             
                //if (wndPopup.DialogResult == MessageBoxResult.OK)
                //{

                //string sumPalletID = "";
                string shipdate = shipdt;
                    string modelid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["MODELID"].Index).Value);
                    string Lineid = Util.NVC(dgTray.GetCell(0, dgTray.Columns["LINEID"].Index).Value);

                    // 스프레드 상 PALLETID들 ('AAA' , 'BBB', 'CCC') 형태로 통합, 변수로 지정
                    //int j = 0;
                    //for (int i = 0; i < dgTray.GetRowCount(); i++)
                    //{
                    //    string sPalletid = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                    //    if (i == 0)
                    //    {
                    //        sumPalletID = "(" + "'" + sPalletid + "'" + ", ";
                    //    }
                    //    else if (i > 0 && i < dgTray.GetRowCount())
                    //    {
                    //        sumPalletID += "'" + sPalletid + "'" + ", ";
                    //    }
                    //    else
                    //    {
                    //        sumPalletID += "'" + sPalletid + "'" + ")";
                    //    }
                    //    j = i;
                    //}
                    //if (j == 0)
                    //{
                    //    sumPalletID = sumPalletID.Replace(",", ")");
                    //}


                    // 메일 전송 안함  (현업확인 - 차요한K)
                    //DataTable RQSTDT = new DataTable();
                    //RQSTDT.Columns.Add("I_SHIP_DATE", typeof(string));
                    //RQSTDT.Columns.Add("I_PALLETID", typeof(string));
                    //RQSTDT.Columns.Add("I_MODELID", typeof(string));
                    //RQSTDT.Columns.Add("LINEID", typeof(string));


                    //DataRow dr = RQSTDT.NewRow();
                    //dr["I_SHIP_DATE"] = shipdate;
                    //dr["I_PALLETID"] = sumPalletID;
                    //dr["I_MODELID"] = modelid;
                    //dr["LINEID"] = Lineid;
                    //RQSTDT.Rows.Add(dr);

                    // DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? QAREQ_MAILING", "RQSTDT", "RSLTDT", RQSTDT);

                    //출하검사 의뢰 이력 저장
                    //출하검사 의뢰 이력 저장
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHIPDATE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                    inDataTable.Columns.Add("NOTE", typeof(string));

                    DataTable inLotTable = indataSet.Tables.Add("INLOT");
                    inLotTable.Columns.Add("BOXID", typeof(string));
                    inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                    int iTotalQty = 0;
                    for (int i = 0; i < dgTray.GetRowCount(); i++)
                    {
                        iTotalQty = iTotalQty + Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                        DataRow inLot = inLotTable.NewRow();
                        inLot["BOXID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                        inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                        inLotTable.Rows.Add(inLot);
                    }

                    DataRow inData = inDataTable.NewRow();
                    inData["LANGID"] = LoginInfo.LANGID;
                    inData["SHIPDATE"] = Convert.ToDateTime(shipdate).ToString("yyyy-MM-dd");
                    inData["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;//Util.NVC(cboUser.Text);
                    inData["TOTAL_QTY"] = iTotalQty;
                    inData["NOTE"] = null;
                    inDataTable.Rows.Add(inData);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_CP", "INDATA,INLOT", null, indataSet);

                    Util.AlertInfo("SFU1275"); //"정상처리 되었습니다."
                                                      //Util.AlertInfo("메일 전송, 이력 저장 및 출력 완료.");

                // 전송정보 시트 초기화
                isQty = 0;

                    dgTray.ItemsSource = null;
                    //dtpShipDate.Visibility = Visibility.Hidden;
                    //lblShipDate.Visibility = Visibility.Hidden;

                    //chkPalletQty.IsChecked = false;
                    chkLine.IsChecked = false;

                    GetPalletInfo();

                //}
                grdMain.Children.Remove(wndPopup);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }   
        }


        private void QAReTagPrint(int t, int Sheet_Cnt)
        {
            // 의뢰자 변수 생성
            string sUserID = "";
            int k = 0; //설비구성 PALLETID가 아닐 경우 For문이 추가로 돌도록 설정
            // paleltID 설비타입 5개 배열 생성
            string[] palletID = null;
            // 호기명 받기 및 중복검사
            object[] Line = null;
            object[] LineName = null;
            // Model명 받기 및 중복 검사
            object[] Model = null;
            // 검사요청일
            string Req_Date = "";

            try
            {
                palletID = new string[t];
                Line = new object[t];
                LineName = new object[t];
                Model = new object[t];

                // Tag 발행용 DataTable 생성
                DataTable dtQAReqTag = new DataTable();
                dtQAReqTag = CreateDT_QAReqTag(dtQAReqTag, Sheet_Cnt);

                Shipdate_Schedule2 = Shipdate_Schedule2.Substring(0, 4) + "년 "
                                              + Shipdate_Schedule2.Substring(5, 2) + "월 "
                                              + Shipdate_Schedule2.Substring(8, 2) + "일";

                for (int i = 0; i < t; i++)
                {
                    //태그에 설비구성인 PALLETID만 받도록 처리
                    if (Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ && Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                    {
                        //palletID[i] = sprQASelect_Sheet1.Cells.Get(i + k, 1).Value.ToString(); //PALLETID 받기
                        //Line[i] = sprQASelect_Sheet1.Cells.Get(i + k, 11).Value.ToString(); //Line 받기
                        //Model[i] = sprQASelect_Sheet1.Cells.Get(i + k, 10).Value.ToString(); //Model 받기
                        palletID[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["PALLETID"].Index).Value); //PALLETID 받기
                        Line[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["LINEID"].Index).Value); //Line 받기
                        LineName[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["EQSGNAME"].Index).Value);
                        Model[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["MODELID"].Index).Value).Substring(0, 3); //Model 받기

                        // 검사요청일 셋팅
                        if (Req_Date == "" || Req_Date == null)
                        {
                            Req_Date = DateTime.Now.ToString().Substring(0, 4) + "년 " + DateTime.Now.ToString().Substring(5, 2) + "월 "
                                     + DateTime.Now.ToString().Substring(8, 2) + "일";
                        }
                        if (sUserID == "")
                        {
                            sUserID = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["REQ_USERNAME"].Index).Value);
                        }
                    }
                    else
                    {
                        k++;
                        i--;
                    }
   
                }

                //호기명 표시 (중복 제거 존재 Line 당 1개씩만)
                for (int i = 0; i < t; i++)
                {
                    int m = 0;
                    int Index = i / 5;

                    if (i % 5 != 0)
                    {
                        for (int n = 0; n < t; n++)
                        {

                            if (Line[i].ToString() == Line[n].ToString())
                            {
                                m++;
                            }

                        }
                        if (m == 1)
                        {
                            //palletTag.sprTag.Sheets[Index].Cells[7, 2].Value += Line[i].ToString() + "호기 ";
                            dtQAReqTag.Rows[Index]["LINEID"] = LineName[i].ToString();      //Line[i].ToString() + "호기 ";
                        }
                    }
                    else
                    {
                        //palletTag.sprTag.Sheets[Index].Cells[7, 2].Value = Line[i].ToString() + "호기 ";
                        //palletTag.sprTag.Sheets[Index].Cells[25, 2].Value = palletTag.sprTag.Sheets[Index].Cells[7, 2].Value; //태그 아래쪽 호기명
                        dtQAReqTag.Rows[Index]["LINEID"] = LineName[i].ToString();      //Line[i].ToString() + "호기 ";
                        dtQAReqTag.Rows[Index]["LINEID2"] = LineName[i].ToString();     //Line[i].ToString() + "호기 ";

                    }
                }

                //모델명 받기 및 중복 검사 
                for (int i = 0; i < t; i++)
                {
                    int m = 0;
                    int tmp = i / 5;
                    if (i > 0 && i % 5 != 0)
                    {
                        for (int n = 0; n < t; n++)
                        {

                            if (Model[i].ToString() == Model[n].ToString())
                            {
                                m++;
                            }

                        }
                        if (m != t)
                        {
                            Util.AlertInfo("SFU2007"); //"한번에 한가지 모델만 전송 할 수있습니다."
                            return;
                        }

                    }
                    else
                    {
                        //palletTag.sprTag.Sheets[tmp].Cells[7, 6].Value = Model[0].ToString();
                        //palletTag.sprTag.Sheets[tmp].Cells[25, 6].Value = palletTag.sprTag.Sheets[tmp].Cells[7, 6].Value; // 태그 아래쪽 모델명
                        dtQAReqTag.Rows[tmp]["MODEL"] = Model[0].ToString();
                        dtQAReqTag.Rows[tmp]["MODEL2"] = Model[0].ToString();
                    }
                }

                // 조립 LotID / 수량 저장을 위한 DataTable
                DataTable resultAssyLotIDs = new DataTable();

                // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                for (int i = 0; i < t; i++)
                {
                    //if (chkPalletQty.IsChecked == true || t <= 5)
                    //{
                        resultAssyLotIDs = SearchAssyLot_8digit(palletID[i]);                       
                        if (resultAssyLotIDs == null)
                        {
                            Util.AlertInfo("SFU1726", new object[] { palletID[i] }); //%1에 Lot 정보가 없습니다.
                            return;
                        }

                        dtQAReqTag = setTagLotId(dtQAReqTag, resultAssyLotIDs, i);  //lot별 수량 조회 합산 태그에 입력
                    //}
                    //else
                    //{
                    //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                    //    return;
                    //}
                }


                //Pallet
                int iCol = 0;
                int Old_Index = 0;
                //태그상 palletId 입력 (tag 시트 셀이 병합되 있는 관계로 3번째 PalletID 부터 코드 조작하여 입력)
                for (int i = 0; i < t; i++)
                {
                    int Index = i / 5;

                    if (Index != Old_Index || i == 0)
                        iCol = 1;
                    //1번째 palletID 입력
                    if (iCol == 1)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_1"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_1"] = palletID[i];
                        iCol++;
                    }
                    //2번째 palletID 입력
                    else if (iCol == 2)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_2"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_2"] = palletID[i];
                        iCol++;
                    }
                    //3번째 palletID 입력
                    else if (iCol == 3)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_3"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_3"] = palletID[i];
                        iCol++;
                    }
                    //4번째 palletID 입력
                    else if (iCol == 4)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_4"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_4"] = palletID[i];
                        iCol++;
                    }
                    //5번째 palletID 입력
                    else if (iCol == 5)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_5"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_5"] = palletID[i];
                        iCol++;
                    }
                    Old_Index = Index;
                }

                for (int i = 0; i < Sheet_Cnt; i++)
                {
                    //태그상 출하예정일 입력
                    dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE"] = Shipdate_Schedule;
                    dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE2"] = Shipdate_Schedule;

                    //태그상 검사요청일 입력
                    dtQAReqTag.Rows[i]["REQ_DATE"] = Req_Date;
                    dtQAReqTag.Rows[i]["REQ_DATE2"] = Req_Date;

                    //담당
                    dtQAReqTag.Rows[i]["USERID"] = txtWorker.Text;//  LoginInfo.USERNAME;//sUserID;
                    dtQAReqTag.Rows[i]["USERID2"] = txtWorker.Text;// LoginInfo.USERNAME;//sUserID;
                }
                // 전송정보 시트 초기화
                isQty = 0;

                // 태그 발행 창 화면에 띄움.
                //object[] Parameters = new object[2];
                //Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                //Parameters[1] = dtQAReqTag;

                //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                //C1WindowExtension.SetParameters(rs, Parameters);
                //rs.Show();

                LGC.GMES.MES.BOX001.Report_Multi_Cell rs = new LGC.GMES.MES.BOX001.Report_Multi_Cell();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                    Parameters[1] = dtQAReqTag;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(wndQAMailSend_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }


        private void QAReMailSend(int t, int Sheet_Cnt)
        {

            // 의뢰자 변수 생성
            string sUserID = Util.NVC(txtWorker.Tag); //  LoginInfo.USERID;//Util.NVC(cboUser2.SelectedValue);
            int k = 0; //설비구성 PALLETID가 아닐 경우 For문이 추가로 돌도록 설정
            // paleltID 설비타입 5개 배열 생성
            string[] palletID = null;
            // 호기명 받기 및 중복검사
            object[] Line = null;
            object[] LineName = null;
            // Model명 받기 및 중복 검사
            object[] Model = null;
            // 검사요청일
            string Req_Date = "";

            try
            {
                palletID = new string[t];
                Line = new object[t];
                LineName = new object[t];
                Model = new object[t];

                // Tag 발행용 DataTable 생성
                DataTable dtQAReqTag = new DataTable();
                dtQAReqTag = CreateDT_QAReqTag(dtQAReqTag, Sheet_Cnt);

                Shipdate_Schedule2 = Shipdate_Schedule2.Substring(0, 4) + "년 "
                                              + Shipdate_Schedule2.Substring(5, 2) + "월 "
                                              + Shipdate_Schedule2.Substring(8, 2) + "일";

                for (int i = 0; i < t; i++)
                {
                    //태그에 설비구성인 PALLETID만 받도록 처리
                    if (Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ && Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                    {
                        palletID[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["PALLETID"].Index).Value); //PALLETID 받기
                        Line[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["LINEID"].Index).Value); //Line 받기
                        LineName[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["EQSGNAME"].Index).Value);
                        Model[i] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["MODELID"].Index).Value).Substring(0, 3); //Model 받기

                        // 검사요청일 셋팅
                        if (Req_Date == "" || Req_Date == null)
                        {
                            Req_Date = DateTime.Now.ToString().Substring(0, 4) + "년 " + DateTime.Now.ToString().Substring(5, 2) + "월 "
                                     + DateTime.Now.ToString().Substring(8, 2) + "일";
                        }
                        if (sUserID == "")
                        {
                            sUserID = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["REQ_USERID"].Index).Value);
                        }
                    }
                    else
                    {
                        k++;
                        i--;
                    }
                }

                //호기명 표시 (중복 제거 존재 Line 당 1개씩만)
                for (int i = 0; i < t; i++)
                {
                    int m = 0;
                    int g = 0;
                    int Index = i / 5;
                    if (i > 0)
                    {
                        for (int n = 0; n < t; n++)
                        {
                            if (Line[i].ToString().Substring(0, 1) == "2")
                            {
                                if (Line[i].ToString().Substring(0, 1) == Line[n].ToString().Substring(0, 1))
                                {
                                    m++;
                                }
                            }
                            else
                            {
                                if (Line[i].ToString() == Line[n].ToString())
                                {
                                    g++;
                                }
                            }

                        }
                        if (chkLine.IsChecked == false && m != t && g != t)
                        {
                            Util.AlertInfo("SFU1506"); //"동일한 라인별 모델만 전송 할 수있습니다."
                            return;
                        }
                        if (m == 1)
                        {
                            //palletTag.sprTag.Sheets[Index].Cells[7, 2].Value += Line[i].ToString() + "호기 ";
                            dtQAReqTag.Rows[Index]["LINEID"] = LineName[i].ToString();
                        }
                    }

                    if (i % 5 == 0)
                    {
                        //palletTag.sprTag.Sheets[Index].Cells[7, 2].Value = Line[i].ToString() + "호기 ";
                        //palletTag.sprTag.Sheets[Index].Cells[25, 2].Value = palletTag.sprTag.Sheets[Index].Cells[7, 2].Value; //태그 아래쪽 호기명
                        dtQAReqTag.Rows[Index]["LINEID"] = LineName[i].ToString();      //Line[i].ToString() + "호기 ";
                        dtQAReqTag.Rows[Index]["LINEID2"] = LineName[i].ToString();     //Line[i].ToString() + "호기 ";
                    }
                }

                //모델명 받기 및 중복 검사 
                for (int i = 0; i < t; i++)
                {
                    int m = 0;
                    int Index = i / 5;

                    if (i % 5 != 0)
                    {
                        for (int n = 0; n < t; n++)
                        {

                            if (Model[i].ToString() == Model[n].ToString())
                            {
                                m++;
                            }

                        }
                        if (m != t)
                        {
                            Util.AlertInfo("SFU2007"); //"한번에 한가지 모델만 전송 할 수있습니다."
                            return;
                        }

                    }
                    else
                    {
                        //palletTag.sprTag.Sheets[Index].Cells[7, 6].Value = Model[0].ToString();
                        //palletTag.sprTag.Sheets[Index].Cells[25, 6].Value = palletTag.sprTag.Sheets[Index].Cells[7, 6].Value; // 태그 아래쪽 모델명
                        dtQAReqTag.Rows[Index]["MODEL"] = Model[0].ToString();
                        dtQAReqTag.Rows[Index]["MODEL2"] = Model[0].ToString();
                    }
                }

                // 조립 LotID / 수량 저장을 위한 DataTable
                DataTable resultAssyLotIDs = new DataTable();

                // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                for (int i = 0; i < t; i++)
                {
                    //if (chkPalletQty.IsChecked == true || t <= 5)
                    //{
                        resultAssyLotIDs = SearchAssyLot_8digit(palletID[i]);
                        if (resultAssyLotIDs == null)
                        {                           
                            Util.AlertInfo("SFU1726", new object[] { palletID[i] }); //%1에 Lot 정보가 없습니다.
                            return;
                        }
                        dtQAReqTag = setTagLotId(dtQAReqTag, resultAssyLotIDs, i);  //lot별 수량 조회 합산 태그에 입력
                    //}
                    //else
                    //{
                    //    Util.AlertInfo("설비 구성 PALLETID는 5개 까지만 전송 가능합니다.");
                    //    return;
                    //}
                }

                //Pallet
                int iCol = 0;
                int Old_Index = 0;
                //태그상 palletId 입력 (tag 시트 셀이 병합되 있는 관계로 3번째 PalletID 부터 코드 조작하여 입력)
                for (int i = 0; i < t; i++)
                {
                    int Index = i / 5;

                    if (Index != Old_Index || i == 0)
                        iCol = 1;
                    //1번째 palletID 입력
                    if (iCol == 1)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_1"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_1"] = palletID[i];
                        iCol++;
                    }
                    //2번째 palletID 입력
                    else if (iCol == 2)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_2"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_2"] = palletID[i];
                        iCol++;
                    }
                    //3번째 palletID 입력
                    else if (iCol == 3)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_3"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_3"] = palletID[i];
                        iCol++;
                    }
                    //4번째 palletID 입력
                    else if (iCol == 4)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_4"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_4"] = palletID[i];
                        iCol++;
                    }
                    //5번째 palletID 입력
                    else if (iCol == 5)
                    {
                        dtQAReqTag.Rows[Index]["PALLET1_5"] = palletID[i];
                        dtQAReqTag.Rows[Index]["PALLET2_5"] = palletID[i];
                        iCol++;
                    }
                    Old_Index = Index;
                }

                for (int i = 0; i < Sheet_Cnt; i++)
                {
                    //태그상 출하예정일 입력
                    dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE"] = Shipdate_Schedule;
                    dtQAReqTag.Rows[i]["SHIPDATE_SCHEDULE2"] = Shipdate_Schedule;

                    //태그상 검사요청일 입력
                    dtQAReqTag.Rows[i]["REQ_DATE"] = Req_Date;
                    dtQAReqTag.Rows[i]["REQ_DATE2"] = Req_Date;

                    //담당
                    dtQAReqTag.Rows[i]["USERID"] = sUserID;
                    dtQAReqTag.Rows[i]["USERID2"] = sUserID;
                }
                // 전송정보 시트 초기화
                isQty = 0;

                // 태그 발행 창 화면에 띄움.
                //object[] Parameters = new object[2];
                //Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                //Parameters[1] = dtQAReqTag;

                //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
                //C1WindowExtension.SetParameters(rs, Parameters);
                //rs.Show();

                LGC.GMES.MES.BOX001.Report_Multi_Cell rs = new LGC.GMES.MES.BOX001.Report_Multi_Cell();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "QARequest_Tag"; // "PalletHis_Tag";
                    Parameters[1] = dtQAReqTag;

                    C1WindowExtension.SetParameters(rs, Parameters);

                  //  rs.Closed += new EventHandler(wndQAReMailSend_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }


        /// <summary>
        /// 메일 재전송에서 Tag발행 호출 후 닫힐때...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndQAReMailSend_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Multi wndPopup = sender as LGC.GMES.MES.BOX001.Report_Multi;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {

                    int ycnt = 0;
                    int t = 0;
                    string sUserID =Util.NVC(txtWorker.Tag);// LoginInfo.USERID;//Util.NVC(cboUser2.SelectedValue);
                    string sumPalletID = "";
                    string shipdate = shipdt2;
                    string modelid = "";
                    string lineId = "";

                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            ycnt++;
                            if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["LOT_TYPE"].Index).Value) == Pack_Wrk_Type.EQ)   // "T"
                            {
                                t++;
                                if (t == 1)
                                {
                                    modelid = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["MODELID"].Index).Value);
                                    lineId = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["LINEID"].Index).Value);
                                }
                            }
                        }
                    }

                    if (t > 0 && t <= ycnt)
                    {

                        int j = 0;
                        int k = 0;
                        for (int i = 0; i < ycnt; i++)
                        {
                            // 스프레드 상 PALLETID들 ('AAA' , 'BBB', 'CCC') 형태로 통합, 변수로 지정
                            if (Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["CHK"].Index).Value) == "1" && Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                            {

                                string sPalletid = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["PALLETID"].Index).Value);
                                if (i == 0)
                                {
                                    sumPalletID = "(" + "'" + sPalletid + "'" + ", ";
                                }
                                else if (i > 0 && i < ycnt - 1)
                                {
                                    sumPalletID += "'" + sPalletid + "'" + ", ";
                                }
                                else
                                {
                                    sumPalletID += "'" + sPalletid + "'" + ")";
                                }
                                j = i;
                            }
                            else
                            {
                                i--;
                                k++;
                            }
                        }
                        if (j == 0)
                        {
                            sumPalletID = sumPalletID.Replace(",", ")");
                        }

                        // 메일 전송 안함 ( 현업확인 - 차요한K)
                        //DataTable RQSTDT = new DataTable();
                        //RQSTDT.Columns.Add("I_SHIP_DATE", typeof(string));
                        //RQSTDT.Columns.Add("I_PALLETID", typeof(string));
                        //RQSTDT.Columns.Add("I_MODELID", typeof(string));
                        //RQSTDT.Columns.Add("LINEID", typeof(string));


                        //DataRow dr = RQSTDT.NewRow();
                        //dr["I_SHIP_DATE"] = shipdate;
                        //dr["I_PALLETID"] = sumPalletID;
                        //dr["I_MODELID"] = modelid;
                        //dr["LINEID"] = lineId;
                        //RQSTDT.Rows.Add(dr);

                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? QAREQ_MAILING", "RQSTDT", "RSLTDT", RQSTDT);


                        //출하검사 의뢰 이력 저장 -- (2016-10-21: 재전송에서는 Skip 처리하기로... 차요한과장과 협의)
                        //DataSet indataSet = new DataSet();
                        //DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        //inDataTable.Columns.Add("SHIPDATE", typeof(string));
                        //inDataTable.Columns.Add("USERID", typeof(string));
                        //inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                        //inDataTable.Columns.Add("NOTE", typeof(string));

                        //DataTable inLotTable = indataSet.Tables.Add("INLOT");
                        //inLotTable.Columns.Add("BOXID", typeof(string));
                        //inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                        //k = 0;
                        //int iTotalQty = 0;
                        //for (int i = 0; i < ycnt; i++)
                        //{
                        //    if (Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        //    {
                        //        iTotalQty = iTotalQty + Util.NVC_Int(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["QTY"].Index).Value);

                        //        DataRow inLot = inLotTable.NewRow();
                        //        inLot["BOXID"] = Util.NVC(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["PALLETID"].Index).Value);
                        //        inLot["TOTAL_QTY"] = Util.NVC_Int(dgQAReqHis.GetCell(i + k, dgQAReqHis.Columns["QTY"].Index).Value);
                        //        inLotTable.Rows.Add(inLot);
                        //    }
                        //    else
                        //    {
                        //        i--;
                        //        k++;
                        //    }
                        //}

                        //DataRow inData = inDataTable.NewRow();
                        //inData["SHIPDATE"] = shipdate;
                        //inData["USERID"] = sUserID;
                        //inData["TOTAL_QTY"] = iTotalQty;
                        //inData["NOTE"] = "";
                        //inDataTable.Rows.Add(inData);

                        //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_CP", "INDATA,INLOT", null, indataSet);

                        //Util.AlertInfo("메일 전송, 이력 저장 및 출력 완료.");
                        Util.AlertInfo("SFU1528"); //"메일 전송 및 출력 완료."
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }


        private void MailSend_Only()
        {
            try
            {

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                // 호기명 받기 및 중복검사
                object[] Line = null;

                string sumPalletID = "";
                string shipdate = shipdt;
                string[] modelid = new string[dgTray.GetRowCount()];


                // 스프레드 상 PALLETID들 ('AAA' , 'BBB', 'CCC') 형태로 통합, 변수로 지정
                int j = 0;
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    string sPalletid = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value); 
                    if (i == 0)
                    {
                        sumPalletID = "(" + "'" + sPalletid + "'" + ", ";
                    }
                    else if (i > 0 && i < dgTray.GetRowCount())
                    {
                        sumPalletID += "'" + sPalletid + "'" + ", ";
                    }
                    else
                    {
                        sumPalletID += "'" + sPalletid + "'" + ")";
                    }
                    j = i;
                }
                if (j == 0)
                {
                    sumPalletID = sumPalletID.Replace(",", ")");
                }
                //한가지 모델만 전송하도록 동작하는 코드
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    modelid[i] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["MODELID"].Index).Value);

                    if (modelid[0] != modelid[i])
                    {
                        Util.AlertInfo("SFU1477"); //"다른 모델이 포함되어 있습니다. 한번에 한가지 모델만 전송 할 수 있습니다."
                        return;
                    }
                }
                //호기명 표시 (중복 제거 존재 Line 당 1개씩만)
                int s = dgTray.GetRowCount();
                Line = new object[s];
                int g = 0;
                int m = 0;
                Line[0] = Util.NVC(dgTray.GetCell(0, dgTray.Columns["LINEID"].Index).Value);
                for (int n = 0; n < s; n++)
                {
                    Line[n] = Util.NVC(dgTray.GetCell(n, dgTray.Columns["LINEID"].Index).Value);
                    if (Line[n].ToString().Substring(0, 1) != "2")
                    {
                        if (Line[0].ToString() == Line[n].ToString())
                        {
                            m++;
                        }
                    }
                    else
                    {
                        if (Line[0].ToString().Substring(0, 1) == Line[n].ToString().Substring(0, 1))
                        {
                            g++;
                        }
                    }
                }

                //라인 혼입 체크박스가 해제되어있고, 라인이 섞여 있으면 
                if (chkLine.IsChecked == false && m != s && g != s)
                {
                    Util.AlertInfo("SFU1478"); //"다른 생산라인 모델이 섞여있습니다.같은 생산라인 PALLET만 전송 할 수 있습니다."
                    return;
                }


                // 메일 전송 기능 사용 안함...(현업확인 -- 차요한K)
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("I_SHIP_DATE", typeof(string));
                //RQSTDT.Columns.Add("I_PALLETID", typeof(string));
                //RQSTDT.Columns.Add("I_MODELID", typeof(string));
                //RQSTDT.Columns.Add("LINEID", typeof(string));


                //DataRow dr = RQSTDT.NewRow();
                //dr["I_SHIP_DATE"] = shipdate;
                //dr["I_PALLETID"] = sumPalletID;
                //dr["I_MODELID"] = modelid[0];
                //dr["LINEID"] = Line[0];
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? QAREQ_MAILING", "RQSTDT", "RSLTDT", RQSTDT);


                //출하검사 의뢰 이력 저장 

                // C20220111-000593 오광택 20220412
                DataTable dtOcvinput = new DataTable();
                dtOcvinput.TableName = "RQSTDT";
                dtOcvinput.Columns.Add("PALLETID", typeof(string));

                //DataRow drOcv = dtOcvinput.NewRow();

                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    DataRow drOcv = dtOcvinput.NewRow();
                    drOcv["PALLETID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                    dtOcvinput.Rows.Add(drOcv);
                }

                DataTable dtOcvchk = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OCV2_CHK", "RQSTDT", "RSLTDT", dtOcvinput);
                if (dtOcvchk.Rows.Count > 0)
                {
                    for (int aa = 0; aa < dtOcvchk.Rows.Count; aa++)
                    {
                        if (Convert.ToInt32(dtOcvchk.Rows[aa]["DAYCHK"].ToString()) > 1209600)
                        {
                            Util.AlertInfo("SFU8482"); // OCV2 TIME INTERVAL 14일 초과되었습니다.
                            return;
                        }
                    }
                    
                }
                



                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHIPDATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("BOXID", typeof(string));
                inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                int iTotalQty = 0;
                for (int i = 0; i < dgTray.GetRowCount(); i++)
                {
                    iTotalQty = iTotalQty + Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                    DataRow inLot = inLotTable.NewRow();
                    inLot["BOXID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                    inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                    inLotTable.Rows.Add(inLot);
                }

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["SHIPDATE"] = Convert.ToDateTime(shipdate).ToString("yyyy-MM-dd");
                inData["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID; //Util.NVC(cboUser.SelectedValue);
                inData["TOTAL_QTY"] = iTotalQty;
                inData["NOTE"] = "";
                inDataTable.Rows.Add(inData);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_CP", "INDATA,INLOT", null, indataSet);

                Util.AlertInfo("SFU1275"); //"이력저장 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다."
                //Util.AlertInfo("메일 전송 및 이력저장 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다.");


                // 전송정보 시트 초기화
                isQty = 0;

                dgTray.ItemsSource = null;
                //dtpShipDate.Visibility = Visibility.Hidden;
                //lblShipDate.Visibility = Visibility.Hidden;

                //chkPalletQty.IsChecked = false;
                chkLine.IsChecked = false;
                //재조회
                GetPalletInfo();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        private void ReMailSend(int t)
        {

            try
            {

                //BizData data = new BizData("QAREQ_MAILING", "RSLTDT");
                string sUserID = Util.NVC(txtWorker.Tag); // LoginInfo.USERID; //Util.NVC(cboUser2.SelectedValue);
                string shipdate = shipdt2;
                int checkedCnt = 0;

                try
                {
                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            checkedCnt++;
                        }
                    }

                    //modelID, lineId 셋팅
                    string[] modelId = new string[checkedCnt];
                    string[] lineId = new string[checkedCnt];
                    string[] palletID = new string[checkedCnt];
                    int modelCnt = 0;
                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            modelId[modelCnt] = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["MODELID"].Index).Value);
                            lineId[modelCnt] = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["LINEID"].Index).Value);
                            palletID[modelCnt] = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["PALLETID"].Index).Value);
                            modelCnt++;
                        }
                    }
                    //modelId 예외 포함 여부 체크 및 Alert
                    for (int i = 0; i < checkedCnt; i++)
                    {
                        if (modelId[0] != modelId[i])
                        {
                            Util.AlertInfo("SFU1477"); //"다른 모델이 포함되어 있습니다. 한번에 한가지 모델만 전송 할 수 있습니다."
                            return;
                        }
                        if (lineId[0].Substring(0, 1) != lineId[i].Substring(0, 1))
                        {
                            if (chkLine.IsChecked == false && lineId[0].Substring(0, 1) != "2")
                            {
                                Util.AlertInfo("SFU1479"); //"다른 생산라인 모델이 섞여있습니다.같은 생산라인 모델 만 전송 할 수 있습니다."
                                return;
                            }
                        }
                    }
                    
                    //출하검사 의뢰 이력 저장 
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("SHIPDATE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                    inDataTable.Columns.Add("NOTE", typeof(string));

                    DataTable inLotTable = indataSet.Tables.Add("INLOT");
                    inLotTable.Columns.Add("BOXID", typeof(string));
                    inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                    int iTotalQty = 0;
                    for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                        {
                            iTotalQty = iTotalQty + Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);

                            DataRow inLot = inLotTable.NewRow();
                            inLot["BOXID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                            inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                            inLotTable.Rows.Add(inLot);
                        }
                    }

                    DataRow inData = inDataTable.NewRow();
                    inData["LANGID"] = LoginInfo.LANGID;
                    inData["SHIPDATE"] = Convert.ToDateTime(shipdate).ToString("yyyy-MM-dd");
                    inData["USERID"] = Util.NVC(txtWorker.Tag);// LoginInfo.USERID; //Util.NVC(cboUser.SelectedValue);
                    inData["TOTAL_QTY"] = iTotalQty;
                    inData["NOTE"] = "";
                    inDataTable.Rows.Add(inData);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_CP", "INDATA,INLOT", null, indataSet);

                    Util.AlertInfo("SFU1275"); //"이력저장 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다."
                                               //Util.AlertInfo("메일 전송 및 이력저장 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다.");

                    
                    GetQAReqHis();

                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.Message);
                }

                // 메일 전송 안함 (현업확인 - 차요한K)
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("I_SHIP_DATE", typeof(string));
                //RQSTDT.Columns.Add("I_PALLETID", typeof(string));
                //RQSTDT.Columns.Add("I_MODELID", typeof(string));
                //RQSTDT.Columns.Add("LINEID", typeof(string));


                //DataRow dr = RQSTDT.NewRow();
                //dr["I_SHIP_DATE"] = shipdate;
                //dr["I_PALLETID"] = sumPalletID;
                //dr["I_MODELID"] = modelId[0];
                //dr["LINEID"] = lineId[0];
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? QAREQ_MAILING", "RQSTDT", "RSLTDT", RQSTDT);



                //출하검사 의뢰 이력 저장 -- (2016-10-21: 재전송에서는 Skip 처리하기로... 차요한과장과 협의)
                //DataSet indataSet = new DataSet();
                //DataTable inDataTable = indataSet.Tables.Add("INDATA");
                //inDataTable.Columns.Add("SHIPDATE", typeof(string));
                //inDataTable.Columns.Add("USERID", typeof(string));
                //inDataTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                //inDataTable.Columns.Add("NOTE", typeof(string));

                //DataTable inLotTable = indataSet.Tables.Add("INLOT");
                //inLotTable.Columns.Add("BOXID", typeof(string));
                //inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));

                //int m = 0; // 사용여부 A만 추려내기 위한 변수
                //int iTotalQty = 0;
                //for (int i = 0; i < t; i++)
                //{
                //    if (Util.NVC(dgQAReqHis.GetCell(i + m, dgQAReqHis.Columns["SEND_FLAG"].Index).Value) == "Y")
                //    {

                //        iTotalQty = iTotalQty + Util.NVC_Int(dgTray.GetCell(i + m, dgTray.Columns["QTY"].Index).Value);

                //        DataRow inLot = inLotTable.NewRow();
                //        inLot["BOXID"] = Util.NVC(dgTray.GetCell(i + m, dgTray.Columns["PALLETID"].Index).Value);
                //        inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i + m, dgTray.Columns["QTY"].Index).Value);
                //        inLotTable.Rows.Add(inLot);
                //    }
                //    else
                //    {
                //        i--;
                //        m++;
                //    }
                //}

                //DataRow inData = inDataTable.NewRow();
                //inData["SHIPDATE"] = shipdate;
                //inData["USERID"] = sUserID;
                //inData["TOTAL_QTY"] = iTotalQty;
                //inData["NOTE"] = "";
                //inDataTable.Rows.Add(inData);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REQUEST_OQC_CP", "INDATA,INLOT", null, indataSet);

                ////Util.AlertInfo("메일 전송 및 이력저장 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다.");
                //Util.AlertInfo("SFU1529"); //"메일 전송 완료, 설비구성 PalletID가 없으므로 TAG는 출력하지 않습니다."

                //isQty = 0;
                ////재조회
                //GetQAReqHis();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }

        }


        /// 포장 출고 ID 별 조립LotID, 해당 Lot 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        private DataTable SearchAssyLot_8digit(string sPalletID)
        {
            DataTable lsDataTable = new DataTable();

            try
            {
                // 데이터 조회
                //BizData data = new BizData("QR_GETASSYLOT_PALLETID_8DIGIT", "RSLTDT");

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_8DIGITLOT_BYPLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                lsDataTable.Columns.Add("LOTID", typeof(string));
                lsDataTable.Columns.Add("QTY", typeof(string));
                #endregion

                for (int lsCount = 0; lsCount < dtResult.Rows.Count; lsCount++)
                {
                    DataRow row = lsDataTable.NewRow();
                    row["LOTID"] = Util.NVC(dtResult.Rows[lsCount]["LOTID"]);
                    row["QTY"] = Util.NVC_Int(dtResult.Rows[lsCount]["CELLQTY"]).ToString();
                    lsDataTable.Rows.Add(row);
                }
                return lsDataTable;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// tag상 LotID 및 Cell 수량 입력 함수
        /// </summary>
        /// <param name="lotIdQty"></param>
        /// <param name="i"></param>
        private DataTable setTagLotId(DataTable dtQAReqTag, DataTable lotIdQty, int i)
        {
            string sTmpLotID = string.Empty;
            int iSumQty = 0;
            int Index = i / 5;

            if (lotIdQty == null)
            {
                Util.AlertInfo("SFU1372"); //"Lot 수량 정보가 존재하지 않습니다."
                return null;
            }
            if (i % 5 == 0)
            {
                //tag.sprTag.Sheets[Index].Cells[10, 2].Value = 0;    //QTY1_1
                //tag.sprTag.Sheets[Index].Cells[28, 2].Value = 0;    //QTY2_1
                dtQAReqTag.Rows[Index]["QTY1_1"] = 0;
                dtQAReqTag.Rows[Index]["QTY2_1"] = 0;

                for (int j = 0; j < lotIdQty.Rows.Count; j++)
                {
                    sTmpLotID += lotIdQty.Rows[j].ItemArray.GetValue(0).ToString() + " : " + lotIdQty.Rows[j].ItemArray.GetValue(1).ToString() + "\n"; //LOTNO1_1
                    iSumQty = iSumQty + int.Parse(lotIdQty.Rows[j].ItemArray.GetValue(1).ToString()); //QTY1_1
                }

                dtQAReqTag.Rows[Index]["LOTNO1_1"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY1_1"] = iSumQty;
                dtQAReqTag.Rows[Index]["LOTNO2_1"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY2_1"] = iSumQty;
            }
            else if (i % 5 == 1)
            {
                //tag.sprTag.Sheets[Index].Cells[10, 3].Value = 0;    //QTY1_2
                //tag.sprTag.Sheets[Index].Cells[28, 3].Value = 0;    //QTY2_2
                dtQAReqTag.Rows[Index]["QTY1_2"] = 0;
                dtQAReqTag.Rows[Index]["QTY2_2"] = 0;

                for (int j = 0; j < lotIdQty.Rows.Count; j++)
                {
                    sTmpLotID += lotIdQty.Rows[j].ItemArray.GetValue(0).ToString() + " : " + lotIdQty.Rows[j].ItemArray.GetValue(1).ToString() + "\n"; //LOTNO1_2
                    iSumQty = iSumQty + int.Parse(lotIdQty.Rows[j].ItemArray.GetValue(1).ToString()); //QTY1_2
                }

                dtQAReqTag.Rows[Index]["LOTNO1_2"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY1_2"] = iSumQty;
                dtQAReqTag.Rows[Index]["LOTNO2_2"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY2_2"] = iSumQty;
            }
            else if (i % 5 == 2)
            {
                //tag.sprTag.Sheets[Index].Cells[10, 6].Value = 0;    //QTY1_3
                //tag.sprTag.Sheets[Index].Cells[28, 6].Value = 0;    //QTY2_3
                dtQAReqTag.Rows[Index]["QTY1_3"] = 0;
                dtQAReqTag.Rows[Index]["QTY2_3"] = 0;

                for (int j = 0; j < lotIdQty.Rows.Count; j++)
                {
                    sTmpLotID += lotIdQty.Rows[j].ItemArray.GetValue(0).ToString() + " : " + lotIdQty.Rows[j].ItemArray.GetValue(1).ToString() + "\n"; //LOTNO1_3
                    iSumQty = iSumQty + int.Parse(lotIdQty.Rows[j].ItemArray.GetValue(1).ToString()); //QTY1_3
                }

                dtQAReqTag.Rows[Index]["LOTNO1_3"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY1_3"] = iSumQty;
                dtQAReqTag.Rows[Index]["LOTNO2_3"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY2_3"] = iSumQty;
            }
            else if (i % 5 == 3)
            {
                //tag.sprTag.Sheets[Index].Cells[10, 8].Value = 0;    //QTY1_4
                //tag.sprTag.Sheets[Index].Cells[28, 8].Value = 0;    //QTY2_4
                dtQAReqTag.Rows[Index]["QTY1_4"] = 0;
                dtQAReqTag.Rows[Index]["QTY2_4"] = 0;

                for (int j = 0; j < lotIdQty.Rows.Count; j++)
                {
                    sTmpLotID += lotIdQty.Rows[j].ItemArray.GetValue(0).ToString() + " : " + lotIdQty.Rows[j].ItemArray.GetValue(1).ToString() + "\n"; //LOTNO1_4
                    iSumQty = iSumQty + int.Parse(lotIdQty.Rows[j].ItemArray.GetValue(1).ToString()); //QTY1_4
                }

                dtQAReqTag.Rows[Index]["LOTNO1_4"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY1_4"] = iSumQty;
                dtQAReqTag.Rows[Index]["LOTNO2_4"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY2_4"] = iSumQty;
            }
            else if (i % 5 == 4)
            {
                //tag.sprTag.Sheets[Index].Cells[10, 9].Value = 0;    //QTY1_5
                //tag.sprTag.Sheets[Index].Cells[28, 9].Value = 0;    //QTY2_5
                dtQAReqTag.Rows[Index]["QTY1_5"] = 0;
                dtQAReqTag.Rows[Index]["QTY2_5"] = 0;

                for (int j = 0; j < lotIdQty.Rows.Count; j++)
                {
                    sTmpLotID += lotIdQty.Rows[j].ItemArray.GetValue(0).ToString() + " : " + lotIdQty.Rows[j].ItemArray.GetValue(1).ToString() + "\n"; //LOTNO1_5
                    iSumQty = iSumQty + int.Parse(lotIdQty.Rows[j].ItemArray.GetValue(1).ToString()); //QTY1_5
                }

                dtQAReqTag.Rows[Index]["LOTNO1_5"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY1_5"] = iSumQty;
                dtQAReqTag.Rows[Index]["LOTNO2_5"] = sTmpLotID;
                dtQAReqTag.Rows[Index]["QTY2_5"] = iSumQty;
            }
            return dtQAReqTag;
        }


        //PALLET ID로 이력조회
        private void selectQAHistInfo_BY_PALLETID(string sPalletID)
        {
            try
            {
                // BizData data = new BizData("QR_QAHIST_BY_PALLETID", "RSLTDT");

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QAREQ_HIS_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (dgQAReqHis.GetRowCount() == 0)
                    {
                        ////dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgQAReqHis, dtResult, FrameOperation,true);
                    }
                    else
                    {

                        if (dtResult.Rows.Count > 0)
                        {
                            //전송정보 로우 수 체크(테이블 결합 루프용)
                            DataTable DT = DataTableConverter.Convert(dgQAReqHis.ItemsSource);

                            DataRow drGet = dtResult.Rows[0];
                            DataRow newDr = DT.NewRow();
                            foreach (DataColumn col in dtResult.Columns)
                            {
                                newDr[col.ColumnName] = drGet[col.ColumnName];
                            }
                            DT.Rows.Add(newDr);
                            ////dgQAReqHis.ItemsSource = DataTableConverter.Convert(DT);
                            Util.GridSetData(dgQAReqHis, DT, FrameOperation,true);
                        }
                    }
                }
                else
                {
                    //의뢰되지 않은 Pallet ID 입니다. 표시 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3162"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            selectQAHistInfo_Made_PALLETID(sPalletID);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        //PALLET ID로 조회 후 이력이 없는 경우 생성
        private void selectQAHistInfo_Made_PALLETID(string sPalletID)
        {

            try
            {
                //BizData data = new BizData("QR_QA_PALLETID", "RSLTDT");

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_MADE_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (dgQAReqHis.GetRowCount() == 0)
                    {
                        ////dgQAReqHis.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgQAReqHis, dtResult, FrameOperation,true);
                    }
                    else
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            //전송정보 로우 수 체크(테이블 결합 루프용)
                            DataTable DT = DataTableConverter.Convert(dgQAReqHis.ItemsSource);

                            DataRow drGet = dtResult.Rows[0];
                            DataRow newDr = DT.NewRow();
                            foreach (DataColumn col in dtResult.Columns)
                            {
                                newDr[col.ColumnName] = drGet[col.ColumnName];
                            }
                            DT.Rows.Add(newDr);
                            ////dgQAReqHis.ItemsSource = DataTableConverter.Convert(DT);
                            Util.GridSetData(dgQAReqHis, DT, FrameOperation,true);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }

        }


        /// <summary>
        /// Tag 발행용 DataTable 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable CreateDT_QAReqTag(DataTable dt, int Sheet_Cnt)
        {
            try
            {

                dt.Columns.Add("LINEID", typeof(string));
                dt.Columns.Add("LINENAME", typeof(string));
                dt.Columns.Add("MODEL", typeof(string));
                dt.Columns.Add("SHIPDATE_SCHEDULE", typeof(string));
                dt.Columns.Add("REQ_DATE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                dt.Columns.Add("LINEID2", typeof(string));
                dt.Columns.Add("LINENAME2", typeof(string));
                dt.Columns.Add("MODEL2", typeof(string));
                dt.Columns.Add("SHIPDATE_SCHEDULE2", typeof(string));
                dt.Columns.Add("REQ_DATE2", typeof(string));
                dt.Columns.Add("USERID2", typeof(string));

                dt.Columns.Add("LOTNO1_1", typeof(string));
                dt.Columns.Add("QTY1_1", typeof(string));
                dt.Columns.Add("LOTNO1_2", typeof(string));
                dt.Columns.Add("QTY1_2", typeof(string));
                dt.Columns.Add("LOTNO1_3", typeof(string));
                dt.Columns.Add("QTY1_3", typeof(string));
                dt.Columns.Add("LOTNO1_4", typeof(string));
                dt.Columns.Add("QTY1_4", typeof(string));
                dt.Columns.Add("LOTNO1_5", typeof(string));
                dt.Columns.Add("QTY1_5", typeof(string));

                dt.Columns.Add("LOTNO2_1", typeof(string));
                dt.Columns.Add("QTY2_1", typeof(string));
                dt.Columns.Add("LOTNO2_2", typeof(string));
                dt.Columns.Add("QTY2_2", typeof(string));
                dt.Columns.Add("LOTNO2_3", typeof(string));
                dt.Columns.Add("QTY2_3", typeof(string));
                dt.Columns.Add("LOTNO2_4", typeof(string));
                dt.Columns.Add("QTY2_4", typeof(string));
                dt.Columns.Add("LOTNO2_5", typeof(string));
                dt.Columns.Add("QTY2_5", typeof(string));

                dt.Columns.Add("PALLET1_1", typeof(string));
                dt.Columns.Add("PALLET1_2", typeof(string));
                dt.Columns.Add("PALLET1_3", typeof(string));
                dt.Columns.Add("PALLET1_4", typeof(string));
                dt.Columns.Add("PALLET1_5", typeof(string));

                dt.Columns.Add("PALLET2_1", typeof(string));
                dt.Columns.Add("PALLET2_2", typeof(string));
                dt.Columns.Add("PALLET2_3", typeof(string));
                dt.Columns.Add("PALLET2_4", typeof(string));
                dt.Columns.Add("PALLET2_5", typeof(string));

                for (int i = 0; i < Sheet_Cnt; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["LINEID"] = string.Empty;
                    dr["LINENAME"] = string.Empty;
                    dr["MODEL"] = string.Empty;
                    dr["SHIPDATE_SCHEDULE"] = string.Empty;
                    dr["REQ_DATE"] = string.Empty;
                    dr["USERID"] = string.Empty;

                    dr["LINEID2"] = string.Empty;
                    dr["LINENAME2"] = string.Empty;
                    dr["MODEL2"] = string.Empty;
                    dr["SHIPDATE_SCHEDULE2"] = string.Empty;
                    dr["REQ_DATE2"] = string.Empty;
                    dr["USERID2"] = string.Empty;

                    dr["LOTNO1_1"] = string.Empty;
                    dr["QTY1_1"] = string.Empty;
                    dr["LOTNO1_2"] = string.Empty;
                    dr["QTY1_2"] = string.Empty;
                    dr["LOTNO1_3"] = string.Empty;
                    dr["QTY1_3"] = string.Empty;
                    dr["LOTNO1_4"] = string.Empty;
                    dr["QTY1_4"] = string.Empty;
                    dr["LOTNO1_5"] = string.Empty;
                    dr["QTY1_5"] = string.Empty;

                    dr["LOTNO2_1"] = string.Empty;
                    dr["QTY2_1"] = string.Empty;
                    dr["LOTNO2_2"] = string.Empty;
                    dr["QTY2_2"] = string.Empty;
                    dr["LOTNO2_3"] = string.Empty;
                    dr["QTY2_3"] = string.Empty;
                    dr["LOTNO2_4"] = string.Empty;
                    dr["QTY2_4"] = string.Empty;
                    dr["LOTNO2_5"] = string.Empty;
                    dr["QTY2_5"] = string.Empty;

                    dr["PALLET1_1"] = string.Empty;
                    dr["PALLET1_2"] = string.Empty;
                    dr["PALLET1_3"] = string.Empty;
                    dr["PALLET1_4"] = string.Empty;
                    dr["PALLET1_5"] = string.Empty;

                    dr["PALLET2_1"] = string.Empty;
                    dr["PALLET2_2"] = string.Empty;
                    dr["PALLET2_3"] = string.Empty;
                    dr["PALLET2_4"] = string.Empty;
                    dr["PALLET2_5"] = string.Empty;

                    dt.Rows.Add(dr);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
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
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP"); //, sCase: "EQSGID_PACK");

            //의뢰인 Combo Set.
            //String[] sFilter6 = { sSHOPID, sAREAID, Process.CELL_BOXING };
            // _combo.SetCombo(cboUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter6, sCase: "PROC_USER");

            // 팔레트바코드 
            isVisibleBCD(sAREAID);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.SelectedValue != null)
            {
                //모델 Combo Set.
                C1ComboBox[] cboParent = { cboEquipmentSegment };
                String[] sFilter1 = { Process.CELL_BOXING };

                _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sFilter: sFilter1, sCase: "EQUIPMENT");
                _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
            }

        }

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

            //라인
            String[] sFilter = { sAREAID2 };    // Area
            //_combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT");
            _combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE_CP");//, sCase: "EQSGID_PACK");

            //의뢰인2 Combo Set.
            // String[] sFilterUser = { sSHOPID2, sAREAID2, Process.CELL_BOXING };
            // _combo.SetCombo(cboUser2, CommonCombo.ComboStatus.SELECT, sFilter: sFilterUser, sCase: "PROC_USER");

            // 팔레트바코드 조회 
            isVisibleBCD(sAREAID2);
        }

        private void cboEquipmentSegment2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment2.SelectedValue != null)
            {
                //모델 Combo Set.
                C1ComboBox[] cboParent = { cboEquipmentSegment2 };
                String[] sFilter1 = { Process.CELL_BOXING };

                _combo.SetCombo(cboEqpt2, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sFilter: sFilter1, sCase: "EQUIPMENT");
                _combo.SetCombo(cboModelLot2, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
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

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletId(getPalletBCD(sPasteStrings[i].Trim())) == false)  // 팔레트바코드 -> boxid
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }
        
        private void btnQACancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }
                //SFU1168 작업을 취소하시겠습니까? 
                Util.MessageConfirm("SFU1168", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        if (_util.GetDataGridCheckCnt(dgQAReqHis, "CHK") < 1)
                        {
                            Util.MessageValidation("SFU1636"); //선택된 대상이 없습니다.
                            return;
                        }
                        List<DataRow> drList = DataTableConverter.Convert(dgQAReqHis.ItemsSource).Select("CHK = '1' AND INSP_PROG_CODE= 'REQUEST'").ToList();
                        if (drList.Count <= 0)
                        {
                            Util.MessageValidation("SFU1939"); //취소 할 수 있는 상태가 아닙니다.	
                            return;
                        }

                        DataTable dtInfo = drList.CopyToDataTable();
                        List<string> sIdList = dtInfo.AsEnumerable().Select(c => c.Field<string>("OQC_INSP_REQ_ID")).Distinct().ToList();

                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("OQC_INSP_REQ_ID", typeof(string));
                        dtIndata.Columns.Add("USERID", typeof(string));

                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = dtIndata.NewRow();
                            drIndata["OQC_INSP_REQ_ID"] = id;
                            drIndata["USERID"] = txtWorker.Tag as string;
                            dtIndata.Rows.Add(drIndata);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_OQC_CP", "INDATA", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            else
                            {
                                Util.MessageInfo("SFU1747");// 요청되었습니다.	
                                GetQAReqHis();
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }

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

        // 팔레트 바코드 항목 표시 여부
        private void isVisibleBCD(string sAreaID)
        {
            // 파레트 바코드 표시 설정
            if (_util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgPalletInfo.Columns.Contains("PLLT_BCD_ID"))
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                if (dgTray.Columns.Contains("PLLT_BCD_ID"))
                    dgTray.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
                if (dgQAReqHis.Columns.Contains("PLLT_BCD_ID"))
                    dgQAReqHis.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgPalletInfo.Columns.Contains("PLLT_BCD_ID"))
                    dgPalletInfo.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                if (dgTray.Columns.Contains("PLLT_BCD_ID"))
                    dgTray.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
                if (dgQAReqHis.Columns.Contains("PLLT_BCD_ID"))
                    dgQAReqHis.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
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
    }

}
