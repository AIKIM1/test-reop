/*************************************************************************************
 Created Date : 2021.01.07
      Creator : 이제섭
   Decription : 포장 Hold 관리 - HOLD 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.07  DEVELOPER : Initial Created.
  2023.01.22  이윤중 : 양품창고, 미판정 창고 Validation 체크 로직 추가
  2023.02.10  이윤중 : CELL Hold 등록 - Excel Upload시 갯수 제한(500 -> 제한없음) 개선
  2023.03.20  박나연 : CELL Hold 등록 - 포장보류여부 체크 추가
  2023.09.22  최경아 : CELL Hold 등록 - 대량 CELL HOLD시 분할하여 BIZ 호출, 
                                        저장시 hold possible status 값이 ture인 것만 저장. false인 것만 저장 시 return
                                        hold 중 hold 불가 cell이 있는 경우, 해당 cell 제외 나머지만 hold 후, 화면에 hold 불가 cell 팝업 노출
  2023.12.12  INS 최윤호  : 'row추가' 버튼 추가, 
                             CELL 수정후 기존 GRID 유지, 
                             해제예정담당자 입력 후 임의 수정 못하게 readonly 상태로 변경 : E20231122-000992
  2024.01.12  최경아    : CELL HOLD 시 HOLD 가능 여부 체크되지 않은 CELL을 NOT HOLD CELL POPUP 에 추가 :E20240112-001808
  2024.04.16  남형희    : E20240219-000132 ESNA 포장 보류 여부 체크시 Packing 및 Shipment 불가 / 포장 보류 여부 미체크시 Packing 가능 Shipment 불가
  2024.05.02  박나연    : 동별 공통 코드 PACK_HOLD_CHECK_YN 사용 여부에 따라 UI에서 '포장보류여부' 체크박스 다르게 표시
                          1) PACK_HOLD_CHECK_YN 사용 여부 'Y'인 경우, '포장보류여부' 체크 이후 비활성화되게 표시
                          2) PACK_HOLD_CHECK_YN 사용 여부 'N'이고, 공통 코드 BOXING_INPUT_HOLD_USE (포장보류여부 사용 공통코드) 사용 여부 'N'인 경우,
                          '포장보류여부' 체크 해제 이후 비활성화되게 표시
  2024.07.30  김수용    : HOLD Cell Excel import 시 500*idx번째 Cell 누락 오류 조치
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_309_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _holdTrgtCode = string.Empty;
        string skip_sublot = string.Empty;
        int sDiv = 100;
        string holdGrID = string.Empty;
        int dgHoldRow = 0;
        //DataTable chkResult = new DataTable();
        DataTable PHoldCellT = new DataTable();
        DataTable NotHoldCell = new DataTable();
        Util _Util = new Util();

        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        DataGridRowPresenter preHold = new DataGridRowPresenter()
        {
            Background = new SolidColorBrush(Colors.Red),
            Foreground = new SolidColorBrush(Colors.Red)
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            //Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_309_HOLD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            //// 시장유형 MKT_TYPE_CODE
            //DataTable dtMrk = dtTypeCombo("MKT_TYPE_CODE", ComboStatus.ALL);
            //(dgHold.Columns["MKT_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMrk);

            //// 보류범위 HOLD_TRGT_CODE  
            //DataTable dtHold = dtTypeCombo("HOLD_TRGT_CODE",ComboStatus.NONE);
            //(dgHold.Columns["HOLD_TRGT_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtHold);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _holdTrgtCode = tmps[0].ToString();

            DataTable dt = new DataTable();
            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                dt.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, dt, FrameOperation);

            xProgress.Visibility = Visibility.Hidden;
            xTextBlock.Visibility = Visibility.Hidden;
            xTextStatus.Visibility = Visibility.Hidden;
            xProgress.Maximum = 100;
            xProgress.Minimum = 0;

            if (_holdTrgtCode == "LOT")
            {
                dgHold.Columns["WH_5000"].Visibility = Visibility.Collapsed;
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PKG_LOTID"].Visibility = Visibility.Collapsed;
                //2023.01.21 - Validation 컬럼 추가 
                dgHold.Columns["CURR_LOC"].Visibility = Visibility.Collapsed;
                dgHold.Columns["VAL_FLAG"].Visibility = Visibility.Collapsed; //임시
                //dgHold.Columns["HOLD_POSS_FLAG"].Visibility = Visibility.Collapsed; //임시
            }
            else
            {
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;
                //2023.01.21 - Validation 컬럼 추가
                dgHold.Columns["WH_310"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5100"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5000"].Visibility = Visibility.Collapsed;
                dgHold.Columns["VAL_FLAG"].Visibility = Visibility.Collapsed; //임시
                //dgHold.Columns["HOLD_POSS_FLAG"].Visibility = Visibility.Collapsed; //임시

                NotHoldCell.Columns.Add("SUBLOTID", typeof(string));
                NotHoldCell.Columns.Add("RESULT_CODE", typeof(string));
                NotHoldCell.Columns.Add("HOLDID", typeof(string));
            }

            //E20240219 - 000132
            if (_Util.IsCommonCodeUseAttr("BOXING_INPUT_HOLD_USE", LoginInfo.CFG_SHOP_ID, "Y") == true)
            {
                chkInputHold.IsChecked = true;
            }

            // PACK_HOLD_CHECK_YN 사용하는 경우 : 포장보류여부 체크 후 해당 체크박스 비활성화
            if (_Util.IsAreaCommonCodeUse("PACK_HOLD_CHECK_YN", "HOLD_YN") == true)
            {
                chkInputHold.IsChecked = true;
                chkInputHold.IsEnabled = false;
                chkInputHold.Opacity = 0.4;
            }
            else if (_Util.IsAreaCommonCodeUse("PACK_HOLD_CHECK_YN", "HOLD_YN") == false && _Util.IsCommonCodeUseAttr("BOXING_INPUT_HOLD_USE", LoginInfo.CFG_SHOP_ID) == false)
            {
                // PACK_HOLD_CHECK_YN 사용하지 않고 BOXKING_INPUT_HOLD_USE 사용하지 않는 경우 : 포장보류여부 체크 해제 후 해당 체크박스 비활성화
                chkInputHold.IsChecked = false;
                chkInputHold.IsEnabled = false;
                chkInputHold.Opacity = 0.4;
            }
        }

        private void dgHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region Validation
        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (Util.NVC(e?.Cell?.Column?.Name) == "HOLD_REG_QTY")
            {
                string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index)?.Value);
                int iHold_req_qty = 0;

                if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                {
                    //SFU3435	숫자만 입력해주세요
                    Util.MessageInfo("SFU3435");
                }

                if (_holdTrgtCode == "LOT")
                {
                    string sMaxRegQty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_MAX_QTY"].Index)?.Value);
                    double iMaxRegQty = 0;
                    if (double.TryParse(sMaxRegQty, out iMaxRegQty))
                    {
                        if (iMaxRegQty < iHold_req_qty && iMaxRegQty > 0)
                        {
                            string sAssyLot = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["ASSY_LOTID"].Index)?.Value);
                            // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
                            Util.MessageValidation("SFU3759", sAssyLot);
                        }
                    }
                }
            }
            else if (Util.NVC(e?.Cell?.Column?.Name) == "ASSY_LOTID")
            {
                dataGrid.EndEdit();
                dataGrid.EndEditRow(true);

                double iQty = GetQtyInfo(Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["ASSY_LOTID"].Index)?.Value));

                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_QTY", iQty);
                DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_MAX_QTY", iQty);

                dataGrid.UpdateLayout();
            }
            //else if(Util.NVC(e?.Cell?.Column?.Name) == "CHK")
            //{
            //    if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_POSS_FLAG"].Index)?.Value).ToUpper().Equals("FALSE"))
            //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "CHK", false);

            //    dataGrid.Refresh();
            //}
            if (_holdTrgtCode == "LOT")
                Hold_Validation();
            else
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
                // Hold_Validation_Sublot(dtInfo);
                Hold_Validation_Sublot_Row(e);
            }
        }
        #endregion

        #region Hold 리스트 추가/제거
        /// <summary>
        /// 엑셀 업로드 양식 다운로드
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
                od.FileName = "Hold_Lot_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    if (_holdTrgtCode == "LOT")
                    {
                        sheet[0, 0].Value = "LOTID";
                        sheet[1, 0].Value = "ABCRB01L";
                        sheet[2, 0].Value = "ABCRD237";

                        sheet[0, 1].Value = "QTY";
                        sheet[1, 1].Value = "1234";
                        sheet[2, 1].Value = "12000";

                        sheet[0, 0].Style = sheet[0, 1].Style = styel;
                        sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;
                    }
                    else
                    {
                        sheet[0, 0].Value = "CELLID";
                        sheet[1, 0].Value = "PB23K1B264";
                        sheet[2, 0].Value = "G86AI051019494";

                        sheet[0, 0].Style = styel;
                        sheet.Columns[0].Width = 1500;
                    }

                    c1XLBook1.Save(od.FileName);

                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                dtInfo.Clear();

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

                        if (_holdTrgtCode == "LOT")
                        {
                            dgHold.ClearRows();

                            if (sheet.Rows.Count > 500)
                            {
                                //CELL ID 개수가 너무 많습니다.(CELLID 개수 500개 이하로 등록 권장)
                                Util.MessageValidation("MMD0118");
                                //loadingIndicator.Visibility = Visibility.Collapsed;
                                return;
                            }

                            if (sheet.GetCell(0, 0).Text != "LOTID"
                                || sheet.GetCell(0, 1).Text != "QTY")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // LOTID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                DataRow dr = dtInfo.NewRow();
                                dr["CHK"] = true;
                                dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();

                                double dTmp = 0;
                                string sTmp = Util.NVC(sheet.GetCell(rowInx, 1)?.Text);
                                if (double.TryParse(sTmp, out dTmp))
                                    dr["HOLD_REG_QTY"] = dTmp;

                                dtInfo.Rows.Add(dr);
                            }

                            if (dtInfo.Rows.Count > 0)
                                dtInfo = dtInfo.DefaultView.ToTable(true);

                            GetAssyHoldLotInfo(dtInfo);

                            btnAdd.IsEnabled = false;
                            //btnDelete.IsEnabled = false;
                        }
                        else
                        {
                            //Util.MessageConfirm("SFU33942",new Action() => 
                            //    {
                            //    }
                            //));

                            xProgress.Visibility = Visibility.Visible;
                            xTextBlock.Visibility = Visibility.Visible;
                            xTextStatus.Visibility = Visibility.Visible;

                            dgHold.ClearRows();

                            if (sheet.GetCell(0, 0).Text != "CELLID")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }

                            ShowLoadingIndicator();

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // LOTID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                DataRow dr = dtInfo.NewRow();

                                if (!dtInfo.Columns.Contains("HOLD_REG_QTY"))
                                    dtInfo.Columns.Add("HOLD_REG_QTY", typeof(int));

                                dr["CHK"] = true;
                                dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text;
                                dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                dr["HOLD_REG_QTY"] = 1;
                                dtInfo.Rows.Add(dr);

                                double div = Convert.ToDouble(rowInx) / Convert.ToDouble(sheet.Rows.Count);
                                int iPgrs = Convert.ToInt32((25) * div);
                                xProgress.Maximum = 100;
                                xProgress.Minimum = 0;
                                UpdateProgressBar(iPgrs, 100, "Data Uploading...");
                            }

                            //if (dtInfo.Rows.Count > 0)
                            //    dtInfo = dtInfo.DefaultView.ToTable(true);

                            Hold_Validation_Sublot(dtInfo);

                            btnAdd.IsEnabled = false;
                            //btnDelete.IsEnabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// HOLD 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            //dt.Columns["ASSY_LOTID"].AllowDBNull = true;
            DataRow dr = dt.NewRow();
            if (_holdTrgtCode == "SUBLOT")
            {
                if (!dt.Columns.Contains("HOLD_REG_QTY"))
                    dt.Columns.Add("HOLD_REG_QTY", typeof(int));
                dr["HOLD_REG_QTY"] = 1;
            }

            dt.Rows.Add(dr);

            Util.GridSetData(dgHold, dt, FrameOperation);
            dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        /// <summary>
        /// HOLD 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgHold, dt, FrameOperation);
                chkAll.IsChecked = false;
            }
        }
        #endregion

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 등록
        /// BIZ : BR_PRD_REG_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }

            if (_holdTrgtCode == "LOT" && dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("ASSY_LOTID"))).ToList().Count > 0)
            {
                //SFU4351		미입력된 항목이 존재합니다.	
                Util.MessageValidation("SFU4351");
                return;
            }

            if (_holdTrgtCode == "LOT" && dtInfo.Select("HOLD_REG_QTY IS NULL OR HOLD_REG_QTY < 1")?.ToList()?.Count > 0)
            {
                //수량은 0보다 커야 합니다.
                Util.MessageValidation("SFU1683");
                return;
            }

            if (_holdTrgtCode == "LOT")
            {
                DataRow[] drList = dtInfo.Select("HOLD_REG_QTY > HOLD_REG_MAX_QTY AND HOLD_REG_MAX_QTY > 0");
                if (drList?.Count() > 0)
                {
                    // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
                    Util.MessageValidation("SFU3759", Util.NVC(drList[0]["ASSY_LOTID"]));
                    return;
                }
            }

            if (_holdTrgtCode == "SUBLOT" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("STRT_SUBLOTID")))).ToList().Count > 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }

            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                // SFU1740  오늘 이후 날짜만 지정 가능합니다.
                Util.MessageValidation("SFU1740");
            }

            if (string.IsNullOrEmpty(txtUser.Text))
            {
                //SFU4350 해제 예정 담당자를 선택하세요.
                Util.MessageValidation("SFU4350");
                return;
            }

            //부서가 없는 경우 있음, id만 체크 csr : E20231122-000992)
            if (txtUser.Tag == null)
            {
                Util.MessageValidation("SFU4350");
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4300 Hold 사유를 입력하세요.
                Util.MessageValidation("SFU4300");
                return;
            }

            //SFU1345	HOLD 하시겠습니까?
            Util.MessageConfirm("SFU13451"
                //+ Environment.NewLine + "※ 양품창고에 존재하는 CELL ID(PALLET, PKG ID)에 대한 HOLD는 GQMS를 통해 추가로 HOLD처리해야합니다"
                //+ Environment.NewLine + "※ what if exist CELLID(PALLET, PKG LOID) in good product warehouse. Additional HOLD processing is required to GQMS"
                , (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        holdGrID = string.Empty;

                        if (_holdTrgtCode != "LOT")
                        {
                            hold_check();
                        }
                        else
                        {
                            holdGrID = string.Empty;
                            Save();
                        }
                    }
                });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region CELL HOLD 시 저장 전  HOLD 가능 여부 CELL만 체크되었는지 확인.
        private async void hold_check()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
                    NotHoldCell.Clear();
                    PHoldCellT.Clear();
                    PHoldCellT.Columns.Add("HOLD_POSS_FLAG", typeof(string));
                    PHoldCellT.Columns.Add("STRT_SUBLOTID", typeof(string));
                    string NHoldCell = string.Empty;
                    string PHoldCell = string.Empty;
                    int NHoldCellCount = 0;
                    int chkResultCount = 0;
                    dgHoldRow = 0;

                    ShowLoadingIndicator();
                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        // CELL 등록 시 Hold 불가한 대상은 Hold 할 수 없도록
                        if (dtInfo.Rows[row]["CHK"].ToString().ToUpper().Equals("TRUE") && dtInfo.Rows[row]["HOLD_POSS_FLAG"].ToString().ToUpper().Equals("FALSE"))
                        {
                            NHoldCell += dtInfo.Rows[row]["STRT_SUBLOTID"] + "  ";
                            NHoldCellCount += 1;
                        }
                        else if (dtInfo.Rows[row]["CHK"].ToString().ToUpper().Equals("TRUE") && dtInfo.Rows[row]["HOLD_POSS_FLAG"].ToString().ToUpper().Equals("TRUE"))
                        {
                            PHoldCell += dtInfo.Rows[row]["STRT_SUBLOTID"] + "  ";
                            DataRow newRow = PHoldCellT.NewRow();
                            newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                            newRow["HOLD_POSS_FLAG"] = dtInfo.Rows[row]["HOLD_POSS_FLAG"];
                            PHoldCellT.Rows.Add(newRow);
                        }
                        if (dtInfo.Rows[row]["CHK"].ToString().ToUpper().Equals("TRUE")) chkResultCount += 1;

                        if (dtInfo.Rows[row]["CHK"].ToString().ToUpper().Equals("FALSE") && dtInfo.Rows[row]["HOLD_POSS_FLAG"].ToString().ToUpper().Equals("FALSE"))
                        {
                            DataRow rqstDr = NotHoldCell.NewRow();

                            rqstDr["SUBLOTID"] = Util.NVC(dtInfo.Rows[row]["STRT_SUBLOTID"].ToString());
                            rqstDr["RESULT_CODE"] = "3";
                            rqstDr["HOLDID"] = "";

                            NotHoldCell.Rows.Add(rqstDr);
                        }
                    }

                    if (chkResultCount == NHoldCellCount)
                    {
                        string message = MessageDic.Instance.GetMessage("SFU4654");
                        Util.MessageInfo(message);
                        //Util.MessageInfo("SFU4654"); //HOLD 가능한 CELL이 없습니다.
                        HiddenLoadingIndicator();
                        this.DialogResult = MessageBoxResult.Cancel;
                        this.Close();
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(NHoldCell))
                    {
                        string message = MessageDic.Instance.GetMessage("SFU4653"); //HOLD 불가 Cell이 포함되어 있습니다. 진행하시겠습니까?
                        //string message = MessageDic.Instance.GetMessage("SFU4653", NHoldCell);

                        Util.MessageConfirm(message
                        , (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                holdGrID = string.Empty;
                                skip_sublot = string.Empty;
                                dgHoldRow = dtInfo.Rows.Count;

                                int iRowCnt = PHoldCellT.Rows.Count;
                                int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / sDiv)));

                                if (iRowCnt <= sDiv)
                                    iQuotient = 0;

                                int idxEnd = 0;

                                if (iRowCnt <= sDiv)
                                    idxEnd = iRowCnt;

                                xProgress.Maximum = iRowCnt;
                                xProgress.Minimum = 0;
                                UpdateProgressBar(0, iRowCnt, " HOLDING START...");

                                for (int idx = 0; idx < iQuotient + 1; idx++)
                                {
                                    idxEnd = ((idx + 1) * sDiv);
                                    if (idx == iQuotient) idxEnd = iRowCnt;

                                    SublotSave(idx, iQuotient, iRowCnt);
                                    UpdateProgressBar(idxEnd, iRowCnt, " CELL HOLDING...");
                                }
                            }
                        });
                    }
                    else
                    {
                        holdGrID = string.Empty;
                        skip_sublot = string.Empty;
                        dgHoldRow = dtInfo.Rows.Count;

                        int iRowCnt = PHoldCellT.Rows.Count;
                        int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / sDiv)));

                        if (iRowCnt <= sDiv)
                            iQuotient = 0;

                        int idxEnd = 0;

                        if (iRowCnt <= sDiv)
                            idxEnd = iRowCnt;

                        xProgress.Maximum = iRowCnt;
                        xProgress.Minimum = 0;
                        UpdateProgressBar(0, iRowCnt, " HOLDING START...");

                        for (int idx = 0; idx < iQuotient + 1; idx++)
                        {
                            idxEnd = ((idx + 1) * sDiv);
                            if (idx == iQuotient) idxEnd = iRowCnt;

                            SublotSave(idx, iQuotient, iRowCnt);
                            UpdateProgressBar(idxEnd, iRowCnt, " CELL HOLDING...");
                        }
                    }
                    HiddenLoadingIndicator();
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }),
                    System.Windows.Threading.DispatcherPriority.Input
                );
            });
        }
        #endregion

        #region Biz
        /// <summary>
        /// Hold 등록 
        /// </summary>
        private void Save()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PACK_HOLD_FLAG", typeof(string));

                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["UNHOLD_CHARGE_USERID"] = txtUser.Tag;
                newRow["HOLD_NOTE"] = txtNote.Text;
                newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PACK_HOLD_FLAG"] = chkInputHold.IsChecked == true ? "Y" : "N";
                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    if (_holdTrgtCode != "LOT")
                    {
                        // CELL 등록 시 Hold 불가한 대상은 Hold 할 수 없도록
                        if (dtInfo.Rows[row]["HOLD_POSS_FLAG"].ToString().ToUpper().Equals("FALSE"))
                            continue;
                    }

                    newRow = inHoldTable.NewRow();
                    newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];
                    newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                    newRow["END_SUBLOTID"] = dtInfo.Rows[row]["END_SUBLOTID"];
                    newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                    inHoldTable.Rows.Add(newRow);
                }

                if (inHoldTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    this.DialogResult = MessageBoxResult.Cancel;
                    this.Close();
                }

                new ClientProxy().ExecuteService_Multi("BR_SET_ASSY_HOLD_BX", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region [CELL HOLD등록]
        private async void SublotSave(int idx, int iQuotient, int iRowCnt)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                const string bizRuleName = "BR_SET_SUBLOT_HOLD_BX";
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    try
                    {

                        DataSet inDataSet = new DataSet();

                        if (!PHoldCellT.Columns.Contains("ASSY_LOTID"))
                            PHoldCellT.Columns.Add("ASSY_LOTID", typeof(string));

                        if (!PHoldCellT.Columns.Contains("END_SUBLOTID"))
                            PHoldCellT.Columns.Add("END_SUBLOTID", typeof(string));

                        if (!PHoldCellT.Columns.Contains("HOLD_REG_QTY"))
                            PHoldCellT.Columns.Add("HOLD_REG_QTY", typeof(int));

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("AREAID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                        inDataTable.Columns.Add("UNHOLD_CHARGE_USERID", typeof(string));
                        inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                        inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                        inDataTable.Columns.Add("SHOPID", typeof(string));
                        inDataTable.Columns.Add("PACK_HOLD_FLAG", typeof(string));
                        inDataTable.Columns.Add("HOLD_GR_ID", typeof(string));

                        DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                        inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                        inHoldTable.Columns.Add("STRT_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("END_SUBLOTID", typeof(string));
                        inHoldTable.Columns.Add("HOLD_REG_QTY", typeof(Int32));

                        DataTable inTable = inDataSet.Tables["INDATA"];
                        DataRow newRow = inDataTable.NewRow();
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                        newRow["UNHOLD_CHARGE_USERID"] = txtUser.Tag;
                        newRow["HOLD_NOTE"] = txtNote.Text;
                        newRow["HOLD_TRGT_CODE"] = _holdTrgtCode;
                        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        newRow["PACK_HOLD_FLAG"] = chkInputHold.IsChecked == true ? "Y" : "N";
                        newRow["HOLD_GR_ID"] = holdGrID;
                        inDataTable.Rows.Add(newRow);
                        newRow = null;

                        //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                        if (iRowCnt <= sDiv)
                            iQuotient = 0;


                        int idxStart = (idx * sDiv);
                        int idxEnd = ((idx + 1) * sDiv);

                        if (idx == iQuotient)
                            idxEnd = iRowCnt;

                        if (iRowCnt <= sDiv)
                            idxEnd = iRowCnt;

                        for (int row = idxStart; row < idxEnd; row++)
                        {
                            //for (int row = 0; row < dtInfo.Rows.Count; row++)
                            //{
                            if (_holdTrgtCode != "LOT")
                            {
                                //CELL 등록 시 Hold 불가한 대상은 Hold 할 수 없도록
                                if (PHoldCellT.Rows[row]["HOLD_POSS_FLAG"].ToString().ToUpper().Equals("FALSE"))
                                {
                                    continue;
                                }
                            }

                            newRow = inHoldTable.NewRow();
                            newRow["ASSY_LOTID"] = PHoldCellT.Rows[row]["ASSY_LOTID"];
                            newRow["STRT_SUBLOTID"] = PHoldCellT.Rows[row]["STRT_SUBLOTID"];
                            newRow["END_SUBLOTID"] = PHoldCellT.Rows[row]["END_SUBLOTID"];
                            newRow["HOLD_REG_QTY"] = PHoldCellT.Rows[row]["HOLD_REG_QTY"];
                            inHoldTable.Rows.Add(newRow);
                        }

                        if (inHoldTable.Rows.Count < 1)
                        {
                            HiddenLoadingIndicator();
                            this.DialogResult = MessageBoxResult.Cancel;
                            this.Close();
                        }

                        DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INHOLD", "OUTDATA,OUTHOLD", inDataSet);
                        holdGrID = resultDS.Tables["OUTHOLD"].Rows[0]["HOLD_GR_ID"].ToString();
                        DataTable dtResult = resultDS.Tables["OUTDATA"];

                        //HOLD안된 CELL 모음
                        foreach (DataRow drResult in dtResult.Rows)
                        {
                            DataRow rqstDr = NotHoldCell.NewRow();

                            rqstDr["SUBLOTID"] = Util.NVC(drResult["SUBLOTID"].ToString());
                            rqstDr["RESULT_CODE"] = Util.NVC(drResult["RESULT_CODE"].ToString());
                            rqstDr["HOLDID"] = Util.NVC(drResult["HOLDID"].ToString());

                            NotHoldCell.Rows.Add(rqstDr);
                        }
                        //int idxP = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(idxEnd / iRowCnt))); 


                        if (idxEnd == iRowCnt)
                        {
                            if (NotHoldCell.Rows.Count > 0)
                            {
                                //HiddenLoadingIndicator();
                                notHoldList(iRowCnt);
                            }

                            else
                                Util.MessageValidation("FM_ME_0433"); //Hold 처리 되었습니다.
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        HiddenLoadingIndicator();
                    }
                }),
                    System.Windows.Threading.DispatcherPriority.Input
                );
            });
        }
        #endregion

        #region CELL HOLD등록후 HOLD되지않은 CELL LIST POPUP
        private void notHoldList(int holdcount)
        {
            try
            {
                BOX001_309_NOTHOLD popupProgress = new BOX001_309_NOTHOLD();
                popupProgress.FrameOperation = FrameOperation;

                if (popupProgress != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = NotHoldCell;
                    Parameters[1] = holdcount;
                    Parameters[2] = dgHoldRow;

                    C1WindowExtension.SetParameters(popupProgress, Parameters);

                    popupProgress.Closed += new EventHandler(notHoldList_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupProgress.ShowModal()));
                    popupProgress.BringToFront();
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
        }

        private void notHoldList_Closed(object sender, EventArgs e)
        {
            BOX001_309_NOTHOLD popup = sender as BOX001_309_NOTHOLD;

            this.grdMain.Children.Remove(popup);
        }
        #endregion

        /// <summary>
        /// 타입으로 CommonCode 조회
        /// Biz : DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE
        /// </summary>
        /// <param name="sFilter"></param>
        /// <returns></returns>
        private DataTable dtTypeCombo(string sFilter, ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                return dtResult;
            }
            catch (Exception ex)
            {
                return null;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();

                        txtUser.IsReadOnly = true;      //값이 들어간 이후 임의수정 못 하게비황성 E20231122-000992  
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //if (e.Column.Name == "STRT_SUBLOTID" || e.Column.Name == "END_SUBLOTID")
            //{
            //    string sLotType = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);               

            //    if (string.IsNullOrWhiteSpace(sLotType))
            //    {
            //        //	SFU4349		보류범위 먼저 선택하세요.	
            //        Util.MessageValidation("SFU4349");
            //        e.Cancel = true;
            //    }
            //    else if (sLotType == "LOT")
            //        e.Cancel = true;               
            //}
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dtpSchdDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //SFU1740  오늘이후날짜만지정가능합니다.
                Util.MessageValidation("SFU1740");
                dtpSchdDate.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                GetUserWindow();

                e.Handled = true;
            }
        }

        private void dgHold_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }

        private double GetQtyInfo(string sAssyLot)
        {
            try
            {
                double dRet = 0;

                if (sAssyLot.Trim().Equals("")) return dRet;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("ASSY_LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["ASSY_LOTID"] = sAssyLot;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSY_LOT_HOLD_REG_QTY_INFO_BX", "INDATA", "OUTDATA", inTable);

                if (dtResult?.Rows?.Count > 0)
                {
                    string sTmp = Util.NVC(dtResult.Rows[0]["HOLD_REG_MAX_QTY"]);

                    double.TryParse(sTmp, out dRet);
                }

                return dRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private void GetAssyHoldLotInfo(DataTable dtExcel)
        {
            try
            {
                Util.gridClear(dgHold);
                if (dtExcel == null || dtExcel.Rows.Count < 1) return;

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));

                DataTable inLotTable = inDataSet.Tables.Add("IN_LOT");
                inLotTable.Columns.Add("ASSY_LOTID", typeof(string));
                inLotTable.Columns.Add("HOLD_REG_QTY", typeof(decimal));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dtExcel.Rows.Count; i++)
                {
                    DataRow dr = inLotTable.NewRow();
                    dr["ASSY_LOTID"] = dtExcel.Rows[i]["ASSY_LOTID"];
                    dr["HOLD_REG_QTY"] = dtExcel.Rows[i]["HOLD_REG_QTY"];

                    inLotTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi("BR_GET_ASSY_LOT_HOLD_REG_QTY_LIST_BX", "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableInDataSet(bizResult))
                        {
                            DataTable dtResult = bizResult.Tables["OUTDATA"];

                            for (int i = 0; i < dgHold.Columns.Count; i++)
                            {
                                if (!dtResult.Columns.Contains(dgHold.Columns[i].Name))
                                    dtResult.Columns.Add(dgHold.Columns[i].Name);
                            }

                            Util.GridSetData(dgHold, dtResult, FrameOperation, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        Hold_Validation();
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void Hold_Validation_Sublot(DataTable dt)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                DataTable dtBizRsltMerge = new DataTable();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SEQ", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                int iSeq = 0;
                try
                {
                    int iRowCnt = dt.Rows.Count;
                    int iDiv = 500;
                    int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / 500)));

                    if (iRowCnt <= iDiv)
                        iQuotient = 0;

                    int iRemainder = (iRowCnt % iDiv);

                    for (int idx = 0; idx < iQuotient + 1; idx++)
                    {
                        try
                        {
                            string sSublotid = string.Empty;
                            int iSublotCnt = 0;

                            int idxStart = (idx * iDiv);
                            int idxEnd = (((idx + 1) * iDiv)); //2024.07.30 HOLD Cell Excel import 시 500*idx번째 Cell 누락 오류 조치, 김수용

                            if (idx == iQuotient)
                                idxEnd = iRowCnt;

                            if (iRowCnt <= iDiv)
                                idxEnd = iRowCnt;

                            for (int x = idxStart; x < idxEnd; x++)
                            {
                                //dtTemp.ImportRow(rqstDataTable.Rows[x]);
                                sSublotid += string.Format("{0},", dt.Rows[x]["STRT_SUBLOTID"].ToString());
                                iSublotCnt++;
                            }

                            if (iSublotCnt > 0)
                            {
                                sSublotid = sSublotid.Substring(0, sSublotid.Length - 1);
                                DataRow rqstDr = dtRqst.NewRow();

                                rqstDr["SEQ"] = iSeq.ToString();
                                rqstDr["SUBLOTID"] = sSublotid;
                                dtRqst.Rows.Add(rqstDr);
                                //dtBizRsltMerge.Merge(new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_SUBLOT_VALD", "RQSTDT", "RSLTDT", dtTemp));
                            }
                            iSeq++;

                            double div = Convert.ToDouble(idx) / Convert.ToDouble(iQuotient);

                            if (iQuotient == 0)
                                div = 1;

                            int iPgrs = 25 + Convert.ToInt32((25) * div);
                            UpdateProgressBar(iPgrs, 100, "Data Query...");
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }

                    if (dtRqst.Rows.Count == 0)
                        return dtBizRsltMerge;


                    DataSet inDataSet = new DataSet();
                    inDataSet.Tables.Add(dtRqst);

                    //UpdateProgressBar(50);

                    new ClientProxy().ExecuteService_Multi("BR_GET_ASSY_HOLD_SUBLOT_VALD", "INDATA", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            UpdateProgressBar(75, 100, "Data Conversion...");

                            if (CommonVerify.HasTableInDataSet(bizResult))
                            {
                                DataTable dtResult = bizResult.Tables["OUTDATA"];

                                DataView dv = new DataView(dtResult);
                                dv.Sort = "HOLD_POSS_FLAG";
                                dtResult = dv.ToTable();

                                //DataView chkdv = new DataView(dtResult);
                                //chkdv.RowFilter = "CHK = 'True'";
                                //chkResult = chkdv.ToTable();

                                if (!dtResult.Columns.Contains("ASSY_LOTID"))
                                    dtResult.Columns.Add("ASSY_LOTID", typeof(string));

                                if (!dtResult.Columns.Contains("END_SUBLOTID"))
                                    dtResult.Columns.Add("END_SUBLOTID", typeof(string));

                                if (!dtResult.Columns.Contains("HOLD_REG_QTY"))
                                    dtResult.Columns.Add("HOLD_REG_QTY", typeof(int));

                                Util.GridSetData(dgHold, dtResult, FrameOperation);
                            }
                            UpdateProgressBar(100, 100, "Completed.");
                            HiddenLoadingIndicator();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    HiddenLoadingIndicator();
                }

                return dt;
            });

        }

        private void UpdateProgressBar(int value, int tal_v, string status)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate
                {
                    xProgress.Value = value;
                    xTextBlock.Text = string.Format("{0}/{1}", xProgress.Value, tal_v);
                    xTextStatus.Text = status;
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }

        private void Hold_Validation()
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                if (dtInfo.Rows.Count < 1)
                {
                    //FM_ME_240	처리할 데이터가 없습니다.
                    Util.MessageValidation("FM_ME_0240");
                    return;
                }

                //DataTable dtTarget = new DataTable();

                //foreach(DataRow dr in dtInfo.Select("VAL_FLAG NOT IN ( 'Y' )"))
                //{
                //    dtTarget.ImportRow(dr);
                //}

                //if (dtTarget.Rows.Count < 1)
                //    return;
                //dtInfo = dtTarget;

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inDataSet = new DataSet();
                DataTable rqstDataTable = inDataSet.Tables.Add("RQSTDT");

                string bizruleName = string.Empty;

                if (_holdTrgtCode == "LOT")
                {
                    bizruleName = "DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_LOT_VALD";

                    rqstDataTable.Columns.Add("LOTID", typeof(string));

                    foreach (DataRow dr in dtInfo.Rows)
                    {
                        if (Util.NVC(dr["VAL_FLAG"]).ToUpper().Equals("TRUE"))
                            continue;

                        if (Util.NVC(dr["ASSY_LOTID"]) == "")
                            continue;

                        DataRow rqstDr = rqstDataTable.NewRow();

                        rqstDr["LOTID"] = Util.NVC(dr["ASSY_LOTID"].ToString());
                        //rqstDr["SUBLOTID"] = dr["STRT_SUBLOTID"].ToString();

                        rqstDataTable.Rows.Add(rqstDr);
                    }

                    if (rqstDataTable.Rows.Count == 0)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    new ClientProxy().ExecuteService_Multi(bizruleName, "RQSTDT", "RSLTDT", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (CommonVerify.HasTableInDataSet(bizResult))
                            {
                                DataTable dtResult = bizResult.Tables["RSLTDT"];

                                //for (int i = 0; i < dgHold.Columns.Count; i++)
                                //{
                                //    if (!dtResult.Columns.Contains(dgHold.Columns[i].Name))
                                //        dtResult.Columns.Add(dgHold.Columns[i].Name);
                                //}

                                //Util.GridSetData(dgHold, dtResult, FrameOperation, false);
                                if (_holdTrgtCode == "LOT")
                                {
                                    foreach (DataRow dr in dtResult.Rows)
                                    {
                                        for (int i = 0; i < dgHold.GetRowCount(); i++)
                                        {
                                            if (DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "ASSY_LOTID").ToString() == dr["PKG_LOTID"].ToString())
                                            {
                                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_310", dr["WH_310"]);
                                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_5000", dr["WH_5000"]);
                                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_5100", dr["WH_5100"]);
                                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "VAL_FLAG", true);
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //    foreach (DataRow dr in dtResult.Rows)
                                //    {
                                //        for (int i = 0; i < dgHold.GetRowCount(); i++)
                                //        {
                                //            DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
                                //            if (DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "STRT_SUBLOTID").ToString() == dr["SUBLOTID"].ToString())
                                //            {
                                //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "PKG_LOTID", dr["PKG_LOTID"]);
                                //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CURR_LOC", dr["CURR_LOC"]);
                                //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "VAL_FLAG", true);
                                //            }
                                //        }
                                //    }
                                //}
                                Hold_Validation_Backgorund();
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
                    }, inDataSet);

                }
                //else
                //{
                //    //int iSublotCnt = 0;

                //    bizruleName = "DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_SUBLOT_VALD";

                //    rqstDataTable.Columns.Add("SUBLOTID", typeof(string));

                //    foreach (DataRow dr in dtInfo.Rows)
                //    {
                //        if (Util.NVC(dr["VAL_FLAG"]).ToUpper().Equals("TRUE"))
                //            continue;

                //        if (Util.NVC(dr["STRT_SUBLOTID"]) == "")
                //            continue;

                //        DataRow rqstDr = rqstDataTable.NewRow();
                //        rqstDr["SUBLOTID"] = Util.NVC(dr["STRT_SUBLOTID"].ToString()); ;
                //        rqstDataTable.Rows.Add(rqstDr);
                //        //sSublotid += string.Format("{0},", dr["STRT_SUBLOTID"].ToString());
                //        //iSublotCnt++;
                //    }

                //    if (rqstDataTable.Rows.Count == 0)
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        return;
                //    }

                //    int iRowCnt = rqstDataTable.Rows.Count;
                //    int iDiv = 500;
                //    int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / 500)));
                //    int iRemainder = (iRowCnt % iDiv);

                //    for (int idx = 0; idx < iQuotient + 1; idx++)
                //    {
                //        try
                //        {
                //            DataTable dtTemp = new DataTable();
                //            dtTemp.TableName = "RQSTDT";
                //            dtTemp.Columns.Add("SUBLOTID", typeof(string));

                //            string sSublotid = string.Empty;
                //            int iSublotCnt = 0;

                //            for (int x = (idx * iDiv); x < (((idx + 1) * iDiv) -1); x++)
                //            {
                //                //dtTemp.ImportRow(rqstDataTable.Rows[x]);
                //                sSublotid += string.Format("{0},", rqstDataTable.Rows[x]["SUBLOTID"].ToString());
                //                iSublotCnt++;
                //            }

                //            if (iSublotCnt > 0)
                //            {
                //                sSublotid = sSublotid.Substring(0, sSublotid.Length - 1);
                //                DataRow rqstDr = dtTemp.NewRow();
                //                rqstDr["SUBLOTID"] = sSublotid;
                //                dtTemp.Rows.Add(rqstDr);

                //                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizruleName, "RQSTDT", "RSLTDT", dtTemp);

                //                foreach (DataRow dr in dtRslt.Rows)
                //                {
                //                    for (int i = 0; i < dgHold.GetRowCount(); i++)
                //                    {
                //                        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
                //                        if (DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "STRT_SUBLOTID").ToString() == dr["SUBLOTID"].ToString())
                //                        {
                //                            DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "PKG_LOTID", dr["PKG_LOTID"]);
                //                            DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CURR_LOC", dr["CURR_LOC"]);
                //                            DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "VAL_FLAG", true);
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Util.MessageException(ex);
                //        }
                //    }

                //    //if(iSublotCnt > 0)
                //    //{
                //    //    sSublotid = sSublotid.Substring(0, sSublotid.Length - 1);
                //    //    DataRow rqstDr = rqstDataTable.NewRow();
                //    //    rqstDr["SUBLOTID"] = sSublotid;
                //    //    rqstDataTable.Rows.Add(rqstDr);
                //    //}
                //}
                //rqstDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                //Hold_Validation_Backgorund();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Hold_Validation_Backgorund()
        {
            if (dgHold.GetRowCount() < 1)
                return;


            try
            {
                for (int i = 0; i < dgHold.GetRowCount(); i++)
                {
                    if (_holdTrgtCode == "LOT")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "WH_310")) != "")
                        {
                            if (Convert.ToDecimal(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "WH_310").ToString()) > 0)
                            {
                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", false);
                                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                            }
                        }
                        //if (Convert.ToDecimal(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "WH_5000").ToString()) > 0
                        //    || Convert.ToDecimal(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "WH_5100").ToString()) > 0)
                        //{
                        //    //dgHold.Rows[i].Presenter.Background = new SolidColorBrush(Colors.Red);
                        //    //dgHold.Rows[i].Presenter.Foreground = new SolidColorBrush(Colors.Red);

                        //    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", false);
                        //    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                        //}
                        //else //활성화 창고
                        //{
                        //    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
                        //    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                        //}
                    }
                    //else
                    //{
                    //    if (Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CURR_LOC")).Equals("양품 창고")
                    //        || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CURR_LOC")).Equals("미판정 창고"))
                    //    {
                    //        //dgHold.Rows[i].Presenter.SetValue(dgHold.Rows[i]., preHold);
                    //        //dgHold.Rows[i].Presenter.Background = new SolidColorBrush(Colors.Red);
                    //        //dgHold.Rows[i].Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    //        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", false);
                    //        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                    //    }
                    //    else //활성화 창고
                    //    {
                    //        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
                    //        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private DataTable Hold_Validation_Background(DataTable dtInput)
        //{
        //if (dtInput.Rows.Count < 1)
        //    return dtInput;


        //try
        //{
        //    for (int i = 0; i < dtInput.Rows.Count; i++)
        //    {
        //        if (_holdTrgtCode == "LOT")
        //        {
        //            if(!string.IsNullOrEmpty(dtInput.Rows[i]["WH_310"].ToString()))
        //            {
        //                if(Convert.ToDecimal(dtInput.Rows[i]["WH_310"].ToString()) > 0)
        //                {
        //                    dtInput.Rows[i]["HOLD_POSS_FLAG"] = true;
        //                    dtInput.Rows[i]["CHK"] = true;
        //                }
        //                else
        //                {
        //                    dtInput.Rows[i]["HOLD_POSS_FLAG"] = false;
        //                    dtInput.Rows[i]["CHK"] = false;
        //                }
        //            }
        //        else
        //        {
        //            if(!string.IsNullOrEmpty(dtInput.Rows[i]["CURR_LOC"].ToString()))
        //            {
        //                }

        //            if (Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CURR_LOC")).Equals("양품 창고")
        //                || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CURR_LOC")).Equals("미판정 창고"))
        //            {
        //                //dgHold.Rows[i].Presenter.SetValue(dgHold.Rows[i]., preHold);
        //                //dgHold.Rows[i].Presenter.Background = new SolidColorBrush(Colors.Red);
        //                //dgHold.Rows[i].Presenter.Foreground = new SolidColorBrush(Colors.Red);
        //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", false);
        //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
        //            }
        //            else //활성화 창고
        //            {
        //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "HOLD_POSS_FLAG", true);
        //                DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
        //            }
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Util.MessageException(ex);
        //    return dtInput;
        //}

        //return dtInput;
        //}

        private void ShowLoadingIndicator()
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background
                ,
                new Action(delegate
                {
                    if (loadingIndicator != null)
                        loadingIndicator.Visibility = Visibility.Visible;
                })
            );
        }

        private void HiddenLoadingIndicator()
        {
            Application.Current.Dispatcher.Invoke((
                new Action(delegate
                {
                    if (loadingIndicator != null)
                        loadingIndicator.Visibility = Visibility.Collapsed;
                }))
                , System.Windows.Threading.DispatcherPriority.Background
            );
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            //dgHold.ClearRows();
            DataGridRowAdd(dgHold, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // <summary>
        /// csr : E20231122-000992 , 그리드 CELLID 편집시 해당 것만 수정 되고, 빈로우들은 유지
        /// </summary>
        /// <param name="e"></param>
        private void Hold_Validation_Sublot_Row(DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.ActualFilterMemberPath != "STRT_SUBLOTID")
                {
                    return;
                };

                //input table_1
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SEQ", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                //input table 데이타 add
                DataTable Grid_Dt = DataTableConverter.Convert(dgHold.ItemsSource);
                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = e.Cell.Value.ToString();
                dr["SEQ"] = "0";
                dtRqst.Rows.Add(dr);

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(dtRqst);

                new ClientProxy().ExecuteService_Multi("BR_GET_ASSY_HOLD_SUBLOT_VALD", "INDATA", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (e.Cell.Row.Index < 0)    //cell 편집중 +- row추가 등 이벤트 발생시 오류 발생방지.
                        {
                            return;
                        }

                        if (bizResult.Tables[0].Rows.Count > 0)
                        {
                            DataTable dtResult = bizResult.Tables["OUTDATA"];

                            // Util.GridSetData(dgHold, dtResult, FrameOperation, true);  //빈셀들은 유지될 수 있게 기존 공통 소스는 주석처리

                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "chk", dtResult.Rows[0]["chk"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "STRT_SUBLOTID", dtResult.Rows[0]["STRT_SUBLOTID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "PKG_LOTID", dtResult.Rows[0]["PKG_LOTID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "CURR_LOC", dtResult.Rows[0]["CURR_LOC"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_POSS_FLAG", dtResult.Rows[0]["HOLD_POSS_FLAG"].ToString());

                            if (!dgHold.Columns.Contains("ASSY_LOTID"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", dtResult.Rows[0]["ASSY_LOTID"].ToString());
                            }

                            if (!dgHold.Columns.Contains("HOLD_REG_QTY"))
                            {
                                if (dtResult.Rows[0]["HOLD_REG_QTY"].ToString() != "")
                                {
                                    DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_QTY", dtResult.Rows[0]["HOLD_REG_QTY"].ToString());
                                }
                            }

                            if (!dgHold.Columns.Contains("ASSY_LOTID"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", dtResult.Rows[0]["ASSY_LOTID"].ToString());
                            }

                            if (!dgHold.Columns.Contains("HOLD_REG_MAX_QTY"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_MAX_QTY", dtResult.Rows[0]["HOLD_REG_MAX_QTY"].ToString());
                            }

                            if (!dgHold.Columns.Contains("WH_310"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_310", dtResult.Rows[0]["WH_310"].ToString());
                            }

                            if (!dgHold.Columns.Contains("WH_5100"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_5100", dtResult.Rows[0]["WH_5100"].ToString());
                            }

                            if (!dgHold.Columns.Contains("WH_5000"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_5000", dtResult.Rows[0]["WH_5000"].ToString());
                            }

                            if (!dgHold.Columns.Contains("VAL_FLAG"))
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "VAL_FLAG", dtResult.Rows[0]["VAL_FLAG"].ToString());
                            }

                            // DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                            // tbTotCount.Text = dtResult.Rows.Count.ToString();
                        }
                        else
                        {  //biz return 데이타가 없을 경우.. 해당 로우 초기화
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "chk", "false");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "STRT_SUBLOTID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "PKG_LOTID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "CURR_LOC", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_POSS_FLAG", "false");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_QTY", null);
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", "");

                            //if (dgHold.Columns.Contains("HOLD_REG_MAX_QTY"))
                            //    DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_MAX_QTY", null);
                            //if (dgHold.Columns.Contains("WH_310") )
                            //    DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_310", "");
                            //if (dgHold.Columns.Contains("WH_5100"))
                            //    DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_5100", "");
                            //if (dgHold.Columns.Contains("WH_5000"))
                            //    DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "WH_5000", "");

                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "VAL_FLAG", "fasle");
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
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


    }
}
