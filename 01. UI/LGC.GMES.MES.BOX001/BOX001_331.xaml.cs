/*************************************************************************************
 Created Date : 2024.05.08
      Creator : 이제섭
   Decription : Cell 포장 - QA 출하검사 의뢰 (Tray) (ESMI 1동 활성화 Tray 단위 포장 대응)
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.08  DEVELOPER : Initial Created.
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
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_331 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _combo_f = new CommonCombo_Form();

        //테이블 저장용
        private DataTable dsGet = new DataTable();
        //테이블 저장용 2
        private DataSet dsGet2 = new DataSet();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        string sAREAID2 = string.Empty;
        string sSHOPID2 = string.Empty;
        string sAREAID3 = string.Empty;
        string sSHOPID3 = string.Empty;

        string sCYCL_TYPE_CODE = string.Empty;

        // 조회한 수량 저장하기 위한 변수
        private int isQty = 0;
        // 출하 예정일
        string shipdt = "";
        string Shipdate_Schedule = "";
        // 출하 예정일2 (QA 이력조회 태그 재발행 메일 재전송 에 사용)
        string shipdt2 = "";
        string Shipdate_Schedule2 = "";

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        public BOX001_331()
        {
            InitializeComponent();
            Loaded += BOX001_331_Loaded;
        }

        private void BOX001_331_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_331_Loaded;

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

            //출고상태 Combo Set.
            string[] sFilter4 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };
            _combo.SetCombo(cboShipStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");

            //타입 Combo Set.
            string[] sFilter5 = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            dtpShipDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            //검사의뢰이력조회
            dtpDateFrom2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            dtpDateTo2.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // Area 셋팅
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            //타입 Combo Set.
            string[] sFilterTYPE = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype2, CommonCombo.ComboStatus.ALL, sFilter: sFilterTYPE, sCase: "COMMCODE");


            //판졍결과 Combo Set.
            //string[] sFilterJUDGE = { "JUDGE_VALUE" };
            //_combo.SetCombo(cboJudge_Value, CommonCombo.ComboStatus.ALL, sFilter: sFilterJUDGE, sCase: "COMMCODE");

            // 판정결과에 Cancel 조건 추가
            setComboboxJudgeValue(cboJudge_Value);

            dtpShipDate2.Text = DateTime.Now.ToString("yyyy-MM-dd");

        }

        #endregion

        // 판정결과에 Cancel 조건 추가
        private void setComboboxJudgeValue(C1ComboBox cboJudgeValue)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "JUDGE_VALUE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow drCancel = dtResult.NewRow();
                drCancel["CBO_NAME"] = "C : Cancel";
                drCancel["CBO_CODE"] = "C";

                dtResult.Rows.Add(drCancel);

                cboJudgeValue.DisplayMemberPath = "CBO_NAME";
                cboJudgeValue.SelectedValuePath = "CBO_CODE";
                
                DataRow drAll = dtResult.NewRow();
                drAll["CBO_NAME"] = "-ALL-";
                drAll["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drAll, 0);

                cboJudgeValue.ItemsSource = dtResult.Copy().AsDataView();

                cboJudgeValue.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
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
        /// 
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

                            for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                            {
                                string sPalletid2 = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                                // 같은 Pallet에 포장된 Tray는 동시에 선택될수 있도록
                                if (sPalletid == sPalletid2)
                                {
                                    DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "CHK", true);

                                }

                            }
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

            // 삭제 버튼 클릭한 Row의 Tray와 동일한 Pallet에 구성된 Tray를 모두 삭제 할 수 있도록 처리
            int iRows = DataTableConverter.Convert(dgTray.ItemsSource).Select("PALLETID = '" + sPalletid + "'").Count();
            int iCnt = 0;
            int[] DeleteRowIndex = new int[iRows];

            // 삭제 대상 Tray와 같은 Pallet에 포장된 Tray 존재 시, 추가로 행 삭제.
            for (int j = 0; j < dgTray.GetRowCount(); j++)
            {
                if (Util.NVC(dgTray.GetCell(j, dgTray.Columns["PALLETID"].Index).Value) == sPalletid)
                {
                    DeleteRowIndex[iCnt] = j;

                    iCnt++;
                }
            }

            dgTray.IsReadOnly = false;
            dgTray.RemoveRows(DeleteRowIndex);
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
                string sPallet = string.Empty;

                // Pallet이 아닌 경우,
                if (!sPalletId.StartsWith("P"))
                {
                    sPallet = GetPalletId(sPalletId);

                    // Tray로 Pallet 를 조회하지 못한 경우 (Tray 포장 안됨 or 정보이상)
                    if (string.IsNullOrWhiteSpace(sPallet))
                    {
                        // SFU1177 포장정보가 없습니다.
                        Util.MessageValidation("SFU1177");
                        return false;
                    }
                    else
                    {
                        // 정상적인 Pallet이면 변수에 바인딩
                        sPalletId = sPallet;
                    }
                }


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
                ScanPalletId(txtPalletID.Text.Trim());
            }
            //sprTray.SetViewportTopRow(0, sprTray.ActiveSheet.RowCount - 1);

        }


        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();

                    if (sPasteString.IndexOf(txtPalletID2.Text) == -1)
                    {
                        sPasteString = txtPalletID2.Text;
                    }

                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 200)
                    {
                        //"한번에 200개 이하의 PALLET만 의뢰 가능합니다."
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU10001"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                        return;
                    }

                    string palletList = "";

                    for (int idxPallet = 0; idxPallet < sPasteStrings.Length ; idxPallet++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[idxPallet]))
                        {
                            // Pallet Lot
                            string lsScanID = sPasteStrings[idxPallet].ToUpper().Substring(0, 1);

                            if (lsScanID == "P")
                            {
                                bool checkDuplicate = false;
                                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                                {
                                    if (sPasteStrings[idxPallet].ToUpper().Trim() == (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["PALLETID"].Index).Value)))
                                    {
                                        checkDuplicate = true;
                                        break;
                                    }
                                }

                                if (checkDuplicate)
                                {
                                    if (sPasteStrings.Length == 1)
                                    {
                                        //이미 조회된 Pallet ID 입니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3165"), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                                        return;
                                    }
                                } 
                                else
                                {
                                    palletList += sPasteStrings[idxPallet] + ",";
                                }
                            }
                            else
                            {
                                if (sPasteStrings.Length == 1)
                                {
                                    //스캔된 ID는 PalletID / TrayID 가 아닙니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1688"), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                                    return;
                                }
                            }

                        }
                    }

                    if (palletList != "")
                    {
                        selectQAHistInfo_BY_PALLETID(palletList);
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
                //라디오 버튼 클릭과 동시에 조건 별로 조회
                //selectQAHistInfo(dtFrom2.Value, dtTo2.Value, cboLine2.SelectedValue.ToString(), cboModel.SelectedValue.ToString());
                GetQAReqHis();


                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
                        dgQAReqHis.Rows[i].Visibility = Visibility.Visible;
                    }

                }

            }
            else if (rdoUseN.IsChecked == true)
            {
                btnSave.IsEnabled = false;
                GetQAReqHis();

                for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
                {
                    if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["DEL_FLAG"].Index).Value) == "D")
                    {
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
                //"전송정보 없음 검사의뢰PALLET을 체크해 주세요."
                Util.MessageInfo("SFU2015");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageInfo("SFU1843");
                return;
            }

            if (dgTray.GetRowCount() > 200)    
            {
                //"한번에 200개 이하의 PALLET만 의뢰 가능합니다."
                Util.MessageInfo("SFU10001");
                return;
            }

            shipdt = dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd");
            QAMailSend();
        }

        private void btnQAMail2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageInfo("SFU1843");
                return;
            }

            string abnormalPalletID = "";
            string inspPalletID = "";
            string dupPalletID = "";
            string holdPalletID = "";
            int inspCount = 0;

            for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
            {
                if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                {
                    inspCount++;

                    string palletID = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["PALLETID"].Index).Value);

                    if (!CheckInspPalletID(palletID))
                    {
                        abnormalPalletID += palletID + ", ";
                    }

                    if (inspPalletID.IndexOf(palletID) > -1 )
                    {
                        dupPalletID += palletID + ", ";
                    }

                    // NCR Release 되지 않은 Pallet도 QA출하검사의뢰가 가능 Hold Check 기능 삭제
                    /*
                    if (CheckHoldPalletID(palletID))
                    {
                        holdPalletID += palletID + ", ";
                    }
                    */

                    inspPalletID += palletID + ", ";
                }
            }

            if (inspCount == 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (inspCount > 200)
            {
                //"한번에 200개 이하의 PALLET만 의뢰 가능합니다."
                Util.MessageInfo("SFU10001");
                return;
            }

            if (dupPalletID != "")
            {
                dupPalletID = dupPalletID.Substring(0, dupPalletID.Length - 2);
                // SFU2051 중복 데이터가 존재 합니다. %1
                Util.MessageValidation("SFU2051", new object[] { "("+ dupPalletID + ")" });
                return;
            }

            // NCR Release 되지 않은 Pallet도 QA출하검사의뢰가 가능 Hold Check 기능 삭제
            /*
            if (holdPalletID != "")
            {
                if (holdPalletID.Length > 2)
                {
                    // Hold Pallet List 끝 ', ' 제거 처리
                    holdPalletID = holdPalletID.Substring(0, holdPalletID.Length - 2);
                }
                // 101534 - LOT [%1]는 NCR HOLD되어 있습니다.
                Util.MessageValidation("101534", new object[] { holdPalletID });
                return;
            }
            */

            if (abnormalPalletID != "")
            {
                if (abnormalPalletID.Length > 2)
                {
                    // Pallet List 끝 ', ' 제거 처리
                    abnormalPalletID = abnormalPalletID.Substring(0, abnormalPalletID.Length - 2);
                }
                //SFU3163 이미 검사 의뢰 되었거나 PALLET의 정보를 찾을 수 없습니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3163") + "\n (" + abnormalPalletID + ")", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
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
            string shipdate = "";

            for (int i = 0; i < dgQAReqHis.GetRowCount(); i++)
            {
                if (Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["CHK"].Index).Value) == "1")
                {
                    shipdate = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["SHIPDATE_SCHEDULE"].Index).Value); 
                    iTotalQty = iTotalQty + Util.NVC_Int(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["QTY"].Index).Value);

                    DataRow inLot = inLotTable.NewRow();
                    inLot["BOXID"] = Util.NVC(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["PALLETID"].Index).Value);
                    inLot["TOTAL_QTY"] = Util.NVC_Int(dgQAReqHis.GetCell(i, dgQAReqHis.Columns["QTY"].Index).Value);
                    inLotTable.Rows.Add(inLot);

                }
            }

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["SHIPDATE"] = Convert.ToDateTime(shipdate).ToString("yyyy-MM-dd");
            inData["USERID"] = txtWorker.Tag as string;
            inData["TOTAL_QTY"] = iTotalQty;
            inData["NOTE"] = "";
            inDataTable.Rows.Add(inData);

            //SFU4140	검사의뢰하시겠습니까?
            Util.MessageConfirm("SFU4140", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    new ClientProxy().ExecuteService_Multi("BR_SET_REQUEST_OQC_BX", "INDATA,INLOT", null, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            // 정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");

                            GetQAReqHis();
                        }
                    }, indataSet);
                }
            });
        }

        #endregion

        #region Mehod


        /// <summary>
        /// Pallet 정보 조회
        /// </summary>
        private void GetPalletInfo()
        {
            try
            {

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
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpTimeFrom.DateTime.Value.ToString("HH:mm:00");  //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpTimeTo.DateTime.Value.ToString("HH:mm:00"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["MDLLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                dr["BOX_RCV_ISS_STAT_CODE"] = sPackStat;
                dr["PACK_WRK_TYPE_CODE"] = sLotType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_OQC_PLT_TRAY_BX", "RQSTDT", "RSLTDT", RQSTDT);
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

                string[] sColumnName = new string[] { "PALLETID" };

                _util.SetDataGridMergeExtensionCol(dgPalletInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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

                // 권혜정 추가(2022.11.23)
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                
                // 작업일/요청일 조회 기준 추가 처리 (2023.11.22)
                RQSTDT.Columns.Add("SEARCHTYPE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpTimeFrom2.DateTime.Value.ToString("HH:mm:00"); ;  //Convert.ToDateTime(dtpDateFrom2.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                dr["TODTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpTimeTo2.DateTime.Value.ToString("HH:mm:00"); // Convert.ToDateTime(dtpDateTo2.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                dr["AREAID"] = sAREAID2;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment2.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment2.SelectedValue);
                dr["MODELID"] = sModel;
                dr["JUDG_VALUE"] = Util.NVC(cboJudge_Value.SelectedValue) == "" ? null : Util.NVC(cboJudge_Value.SelectedValue);
                dr["PACK_WRK_TYPE_CODE"] = Util.NVC(cboLottype2.SelectedValue) == "" ? null : Util.NVC(cboLottype2.SelectedValue);

                // 권혜정 추가(2022.11.23)
                dr["PALLETID"] = txtPalletID2.Text.Trim() == "" ? null : txtPalletID2.Text.Trim();
                
                // 작업일/요청일 조회 기준 추가 처리 (2023.11.22)
                if (rdoWork.IsChecked == true)
                {
                    dr["SEARCHTYPE"] = "WORK";
                } 
                else
                {
                    dr["SEARCHTYPE"] = "REQ";
                }
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_QAREQ_HIS_CP_BX", "RQSTDT", "RSLTDT", RQSTDT);

                string judgChk = Util.NVC(cboJudge_Value.SelectedValue) == "" ? null : Util.NVC(cboJudge_Value.SelectedValue);

                if (judgChk != null) dtResult = GetJudgQAReqHis(dtResult, judgChk);

                Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);

                string[] sColumnName = new string[] { "PALLETID" };
                _util.SetDataGridMergeExtensionCol(dgQAReqHis, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                txtPalletQty2.Value = 0;
                txtCellQty2.Value = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_OQC_PLT_TRAY_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dgTray.GetRowCount() == 0)
                {
                    ////dgTray.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgTray, dtResult, FrameOperation, true);
                    dsGet = dtResult;
                }
                else
                {
                    dsGet = dtResult;

                    if (dtResult.Rows.Count > 0)
                    {
                        //전송정보 로우 수 체크(테이블 결합 루프용)
                        DataTable DT = DataTableConverter.Convert(dgTray.ItemsSource);

                        for (int i = 0; i < dsGet.Rows.Count; i++)
                        {
                            DataRow drGet = dsGet.Rows[i];
                            DataRow newDr = DT.NewRow();
                            foreach (DataColumn col in dsGet.Columns)
                            {
                                newDr[col.ColumnName] = drGet[col.ColumnName];
                            }
                            DT.Rows.Add(newDr);
                        }

                        ////dgTray.ItemsSource = DataTableConverter.Convert(DT);
                        Util.GridSetData(dgTray, DT, FrameOperation, true);
                    }

                }

                string[] sColumnName = new string[] { "PALLETID" };

                _util.SetDataGridMergeExtensionCol(dgTray, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private bool CheckInspPalletID(string sPalletID)
        {
            bool chkFlag = false;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_OQC_PLT_REQ_CHK_CP_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    chkFlag = true;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

            return chkFlag;
        }

        private bool CheckHoldPalletID(string palletID)
        {
            bool chkFlag = false;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_OQC_PLT_HOLD_CHK_CP_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    chkFlag = true;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

            return chkFlag;
        }


        private void QAMailSend()
        {
            try
            {
                MailSend_Only();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void wndQAMailSend_Closed(object sender, EventArgs e)
        {
            try
            {
                Report_Multi_Cell wndPopup = sender as Report_Multi_Cell;

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

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REQUEST_OQC_BX", "INDATA,INLOT", null, indataSet);

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
                    string sUserID = Util.NVC(txtWorker.Tag);// LoginInfo.USERID;//Util.NVC(cboUser2.SelectedValue);
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
                    Util.MessageInfo("SFU1843");
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

                    if (inLotTable.Select("BOXID = '" + Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value) + "'").Count() == 0)
                    {
                        DataRow inLot = inLotTable.NewRow();
                        inLot["BOXID"] = Util.NVC(dgTray.GetCell(i, dgTray.Columns["PALLETID"].Index).Value);
                        inLot["TOTAL_QTY"] = 0; //Biz 내에서 사용안함. INDATA-TOTAL_QTY만 봄.

                        //inLot["TOTAL_QTY"] = Util.NVC_Int(dgTray.GetCell(i, dgTray.Columns["QTY"].Index).Value);
                        inLotTable.Rows.Add(inLot);
                    }

                }

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["SHIPDATE"] = Convert.ToDateTime(shipdate).ToString("yyyy-MM-dd");
                inData["USERID"] = txtWorker.Tag as string;
                inData["TOTAL_QTY"] = iTotalQty;
                inData["NOTE"] = "";
                inDataTable.Rows.Add(inData);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REQUEST_OQC_BX", "INDATA,INLOT", null, indataSet);

                // 정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                // 전송정보 시트 초기화
                isQty = 0;

                dgTray.ItemsSource = null;

                chkLine.IsChecked = false;
                //재조회
                GetPalletInfo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
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
                            Util.MessageInfo("SFU1477"); //"다른 모델이 포함되어 있습니다. 한번에 한가지 모델만 전송 할 수 있습니다."
                            return;
                        }
                        if (lineId[0].Substring(0, 1) != lineId[i].Substring(0, 1))
                        {
                            if (chkLine.IsChecked == false && lineId[0].Substring(0, 1) != "2")
                            {
                                Util.MessageInfo("SFU1479"); //"다른 생산라인 모델이 섞여있습니다.같은 생산라인 모델 만 전송 할 수 있습니다."
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

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REQUEST_OQC_BX", "INDATA,INLOT", null, indataSet);

                    // 정상 처리 되었습니다.
                    Util.MessageInfo("SFU1275");

                    GetQAReqHis();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_QAREQ_HIS_CP_BX", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    if (dgQAReqHis.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);
                    }
                    else
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            //전송정보 로우 수 체크(테이블 결합 루프용)
                            DataTable DT = DataTableConverter.Convert(dgQAReqHis.ItemsSource);

                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                DataRow drGet = dtResult.Rows[i];
                                DataRow newDr = DT.NewRow();
                                foreach (DataColumn col in dtResult.Columns)
                                {
                                    newDr[col.ColumnName] = drGet[col.ColumnName];
                                }

                                DT.Rows.Add(newDr);
                            }

                            Util.GridSetData(dgQAReqHis, DT, FrameOperation, true);
                        }
                    }

                    string[] sColumnName = new string[] { "PALLETID" };
                    _util.SetDataGridMergeExtensionCol(dgQAReqHis, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

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
                        Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);
                    }
                    else
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            //전송정보 로우 수 체크(테이블 결합 루프용)
                            DataTable DT = DataTableConverter.Convert(dgQAReqHis.ItemsSource);

                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                DataRow drGet = dtResult.Rows[i];
                                DataRow newDr = DT.NewRow();
                                foreach (DataColumn col in dtResult.Columns)
                                {
                                    newDr[col.ColumnName] = drGet[col.ColumnName];
                                }

                                DT.Rows.Add(newDr);
                            }

                            Util.GridSetData(dgQAReqHis, DT, FrameOperation, true);
                        }

                        string[] sColumnName = new string[] { "PALLETID" };
                        _util.SetDataGridMergeExtensionCol(dgQAReqHis, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                        /*
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
                            Util.GridSetData(dgQAReqHis, DT, FrameOperation, true);
                        }
                        */
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
            _combo_f.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //모델 Combo Set.
            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
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
            _combo_f.SetCombo(cboEquipmentSegment2, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE");//, sCase: "EQSGID_PACK");

            //의뢰인2 Combo Set.
            // String[] sFilterUser = { sSHOPID2, sAREAID2, Process.CELL_BOXING };
            // _combo.SetCombo(cboUser2, CommonCombo.ComboStatus.SELECT, sFilter: sFilterUser, sCase: "PROC_USER");
        }

        private void cboEquipmentSegment2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //모델 Combo Set.
            C1ComboBox[] cboParent = { cboEquipmentSegment2 };
            _combo.SetCombo(cboModelLot2, CommonCombo.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
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
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && ScanPalletId(sPasteStrings[i].Trim()) == false)
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

                        new ClientProxy().ExecuteService_Multi("BR_SET_REQUEST_OQC_CANCEL_BX", "INDATA", null, (bizResult, bizException) =>
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

        /// <summary>
        /// 검사의뢰 이력조회 - 검사결과 조건조회
        /// </summary>
        private DataTable GetJudgQAReqHis(DataTable indataTable, string judgValue)
        {
            DataTable result = indataTable.Clone();
            List<string> getList = new List<string>();

            if (judgValue == "C")
            {
                GetPalletTable(indataTable, getList, judgValue);
                getList = getList.Distinct().ToList();

                foreach (string list in getList) //DataRow 재배열
                {
                    DataView resultView = indataTable.AsEnumerable().Where(r => r.Field<string>("PALLETID") == list).CopyToDataTable().DefaultView; //Pallet ID 필터 조회
                    resultView.Sort = "REQ_DTTM DESC"; // 최신 판정 결과 조회를 위한 내림차순 조회
                    DataRow[] selectResultRow = resultView.ToTable().Select();

                    DataTable selectTable = selectResultRow.CopyToDataTable(); //해당 조회 rows to table 저장

                    int cancelCount = 0;

                    foreach (DataRow row in selectTable.Rows)
                    {
                        string value = row["JUDG_CODE"].ToString();

                        cancelCount = value == "C" ? 1 : 0;

                        if (cancelCount == 0)
                        {
                            selectTable.Rows.Clear();
                            break;
                        }
                    }

                    result.Merge(selectTable);

                }

            }
            else
            {
                GetPalletTable(indataTable, getList, judgValue);
                getList = getList.Distinct().ToList();

                foreach (string list in getList) //DataRow 재배열
                {
                    DataView resultView = indataTable.AsEnumerable().Where(r => r.Field<string>("PALLETID") == list).CopyToDataTable().DefaultView; //Pallet ID 필터 조회
                    resultView.Sort = "REQ_DTTM DESC"; // 최신 판정 결과 조회를 위한 내림차순 조회
                    DataRow[] selectResultRow = resultView.ToTable().Select();

                    DataTable selectTable = selectResultRow.CopyToDataTable(); //해당 조회 rows to table 저장

                    int cancelCount = 0;

                    foreach (DataRow row in selectTable.Rows)
                    {
                        string value = row["JUDG_CODE"].ToString();

                        if (value != "C" || value == judgValue)
                        {
                            string chkValue = value;
                            break;
                        }
                        if (value == "C") cancelCount++;
                    }

                    DataRow nthRow = selectTable.Rows[cancelCount];
                    string nthValue = nthRow["JUDG_CODE"].ToString();
                    if (nthValue != judgValue) selectTable.Rows.Clear();

                    result.Merge(selectTable);

                }
            }

            DataView resultTbView = result.DefaultView;

            resultTbView.Sort = "PACKDTTM ASC, REQ_DTTM ASC";
            result = resultTbView.ToTable();

            return result;

        }

        private void GetPalletTable(DataTable indataTable, List<string> getList, string value)
        {
            indataTable.DefaultView.RowFilter = $"JUDG_CODE = '{value}'";
            DataTable selectResult = indataTable.DefaultView.ToTable();

            foreach (DataRow rows in selectResult.Rows) //해당Pallet_ID 추출
            {
                object[] arrCrOjArray = selectResult.Select().Select(x => x["PALLETID"]).ToArray();
                string[] arrCrStArray = arrCrOjArray.Cast<string>().ToArray();
                getList.AddRange(arrCrStArray);
            }

        }
        /// <summary>
        /// Tray로 팔레트 조회
        /// </summary>
        /// <param name="TrayId"></param>
        /// <returns></returns>
        private string GetPalletId(string TrayId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SCAN_VALUE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["SCAN_VALUE"] = TrayId;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_GET_PACKING_TRAY_INFO_BX", "INDATA", "OUTCST", dt);

            if (dtResult.Rows.Count != 0 && !string.IsNullOrWhiteSpace(dtResult.Rows[0]["BOXID"].ToString()))
            {
                return dtResult.Rows[0]["BOXID"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        //private void dgTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{
        //    if (sender == null)
        //    {
        //        return;
        //    }

        //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //    dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (e.Cell.Presenter == null)
        //        {
        //            return;
        //        }

        //        //Grid Data Binding 이용한 Background 색 변경
        //        if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
        //        {
        //            if (Convert.ToString(e.Cell.Column.Name) != "CHK")
        //            {
        //                string value = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OQC_TRGT_FLAG"));

        //                if (!string.IsNullOrEmpty(value))
        //                {
        //                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");//System.Windows.Media.ColorConverter.ConvertFromString("#FFD0DA");//System.Windows.Media.ColorConverter.ConvertFromString("#FFD0DA");

        //                    if (value.Equals("Y"))
        //                    {
        //                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
        //                    }
        //                    else
        //                    {
        //                        e.Cell.Presenter.Background = null;
        //                    }
        //                }
        //            }
        //        }
        //    }));
        //}
    }
}

