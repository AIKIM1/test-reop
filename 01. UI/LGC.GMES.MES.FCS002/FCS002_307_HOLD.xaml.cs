/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리 - HOLD 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2020.08.26 김동일    C20200430-000047    LOT HOLD 처리 시 보류수량 Validation 추가
 2021.07.29 이상훈    C20210729-000334    LOT HOLD 보류수량 체크 로직 변경
 2021.08.17 송교진    C20210630-000408    BOX HOLD 처리 로직 추가
 2021.11.18 안인효    C20211111-000007    LOT Hold  등록시 포장 보류 옵션처리 추가
 2021.12.17 김광오    C20210615-000524    【ESNB 】 Cell hold 등록 에라내용 개선 
 2022.01.14 김광오    C20220113-000346    ESNB PI] 포장HOLD가진해중인LOT ID HOLD관리시，최대수량 이 잘못됐다고 표시
                                         비고 : 1. [ C20210615-000524    【ESNB 】 Cell hold 등록 에라내용 개선 ] 배포 후 오류 확인되어 관련 내용 수정
                                                2. HOLD BOX - TOTAL_QTY 컬럼 표시 포맷 변경
 2023.03.13  LEEHJ   SI                   소형활성화 MES 복사
 2023.11.07  최경아  SI                   소형활성화 BZ로 수정
 2023.11.30  최경아  SI                   LOT,CELL HOLD시 위치 VALIDATION 추가, 대량CELL HOLD 처리 기능 추가.
 2024.02.14  이홍주  SI                   LOT HOLD 등록시 최대수량(입력가능)칼럼 숨김처리, 5100 미판정 창고 추가
 2024.08.23  이홍주  E20240819-000268     Hold_code,Unhold_Code 추가 및 선택/조회기능 추가, Cell Hold시 500개 단위 누락건 수정
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
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using System.Collections;
using LGC.GMES.MES.Common.Mvvm;
using System.Collections.ObjectModel;
using System.Text;
using System.Reflection;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_307_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _holdTrgtCode = string.Empty;
        string skip_sublot = string.Empty;
        int sDiv = 100;
        string holdGrID = string.Empty;
        int dgHoldRow = 0;
        DataTable PHoldCellT = new DataTable();
        DataTable NotHoldCell = new DataTable();
        Util _Util = new Util();

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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region [C20210615-000524] Added by kimgwango on 2021.12.16
        public HoldViewModel Vm { get; }
        #endregion

        public FCS002_307_HOLD()
        {
            Vm = new HoldViewModel();
            DataContext = Vm;

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
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "CBO_AREA_ACTIVITIREASON", sFilter: new string[] { LoginInfo.CFG_AREA_ID, "HOLD_LOT" });

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            GetAreaDefectCode();

            _holdTrgtCode = tmps[0].ToString();

            Vm.Items = new ObservableCollectionHold<HoldModel>(dgHold,_holdTrgtCode);
            
            xProgress.Maximum = 100;
            xProgress.Minimum = 0;

            if (_holdTrgtCode == "LOT")
            {
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;

                dgHold.Columns["WH_5100"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5000"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PKG_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["CURR_LOC"].Visibility = Visibility.Collapsed;
                dgHold.Columns["VAL_FLAG"].Visibility = Visibility.Collapsed; //임시
                
            }
            else if (_holdTrgtCode == "BOX")
            {
                Header = "Pallet HOLD 등록";
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                chkInputHold.Visibility = Visibility.Collapsed;
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_310"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5100"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5000"].Visibility = Visibility.Collapsed;
                dgHold.Columns["VAL_FLAG"].Visibility = Visibility.Collapsed; //임시
                dgHold.Columns["PKG_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["CURR_LOC"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_310"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5100"].Visibility = Visibility.Collapsed;
                dgHold.Columns["WH_5000"].Visibility = Visibility.Collapsed;
                dgHold.Columns["VAL_FLAG"].Visibility = Visibility.Collapsed;

                NotHoldCell.Columns.Add("SUBLOTID", typeof(string));
                NotHoldCell.Columns.Add("RESULT_CODE", typeof(string));
                NotHoldCell.Columns.Add("HOLDID", typeof(string));
            }

            //E20240219 - 000132 : 포장 보류 여부는 자동차만 사용
            //if (_Util.IsCommonCodeUseAttr("BOXING_INPUT_HOLD_USE", LoginInfo.CFG_SHOP_ID, "Y") == true)
            //{
            //    chkInputHold.IsChecked = true;
            //}

            //// PACK_HOLD_CHECK_YN 사용하는 경우 : 포장보류여부 체크 후 해당 체크박스 비활성화
            //if (_Util.IsAreaCommonCodeUse("PACK_HOLD_CHECK_YN", "HOLD_YN") == true)
            //{
            //    chkInputHold.IsChecked = true;
            //    chkInputHold.IsEnabled = false;
            //    chkInputHold.Opacity = 0.4;
            //}
            //else if (_Util.IsAreaCommonCodeUse("PACK_HOLD_CHECK_YN", "HOLD_YN") == false && _Util.IsCommonCodeUseAttr("BOXING_INPUT_HOLD_USE", LoginInfo.CFG_SHOP_ID) == false)
            //{
            //    // PACK_HOLD_CHECK_YN 사용하지 않고 BOXKING_INPUT_HOLD_USE 사용하지 않는 경우 : 포장보류여부 체크 해제 후 해당 체크박스 비활성화
            //    chkInputHold.IsChecked = false;
            //    chkInputHold.IsEnabled = false;
            //    chkInputHold.Opacity = 0.4;
            //}
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
            #region [C20210615-000524] Commented by kimgwango on 2021.12.18
            //if ((bool)chkAll.IsChecked)
            //{
            //    for (int i = 0; i < dgHold.GetRowCount(); i++)
            //    {
            //        // 기존 저장자료는 제외
            //        if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
            //            DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", true);
            //    }
            //}
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.18
            foreach (var item in Vm.Items)
                item.CHK = true;
            #endregion
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.18
            //if (!(bool)chkAll.IsChecked)
            //{
            //    for (int i = 0; i < dgHold.GetRowCount(); i++)
            //    {
            //        DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
            //    }
            //}
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.18
            foreach (var item in Vm.Items)
                item.CHK = false;
            #endregion
        }
        #endregion

        #region Validation
        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
        }

        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

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
              
                #region C20210615 - 000524    【ESNB 】 Cell hold 등록 에라내용 개선 Added by kimgwango on 2021.01.14
                try
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        HoldModel item = (HoldModel)e.Cell.Row.DataItem;
                        item.HOLD_REG_MAX_QTY = (decimal)iQty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                dataGrid.UpdateLayout();
            }
            else if(Util.NVC(e?.Cell?.Column?.Name) == "BOXID")
            {

                dataGrid.EndEdit();
                dataGrid.EndEditRow(true);

                string sBoxID = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["BOXID"].Index)?.Value);

                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                {
                    if (i == e.Cell.Row.Index || dataGrid.GetCell(i, dataGrid.Columns["BOXID"].Index).Value==null)
                        continue;
                    if (dataGrid.GetCell(i, dataGrid.Columns["BOXID"].Index).Value.Equals(sBoxID))
                    {
                        //이미 조회된 Pallet ID 입니다.
                        Util.MessageValidation("SFU3165");
                        return;
                    }
                }

                DataTable dtRslt = GetPalletInfo(sBoxID);

                if (dtRslt.Rows.Count<1)
                {
                    //입력한 Pallet가 존재하지 않습니다. 
                    Util.MessageValidation("SFU3394");

                    #region C20210615 - 000524    【ESNB 】 Cell hold 등록 에라내용 개선 Added by kimgwango on 2021.01.14
                    if (e.Cell.Row.DataItem != null)
                    {
                        HoldModel item = (HoldModel)e.Cell.Row.DataItem;

                        item.PRODID = string.Empty;
                        item.PACK_EQSGID = string.Empty;
                        item.MDLLOT_ID = string.Empty;
                        item.PRJT_NAME = string.Empty;
                        item.TOTAL_QTY = 0;
                        item.BOXTYPE = string.Empty;
                    }
                    #endregion
                    return;
                }
                
                #region C20210615 - 000524    【ESNB 】 Cell hold 등록 에라내용 개선 Added by kimgwango on 2021.01.14
                try
                {
                    if (e.Cell.Row.DataItem != null)
                    {
                        HoldModel item = (HoldModel)e.Cell.Row.DataItem;

                        item.PRODID = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                        item.PACK_EQSGID = Util.NVC(dtRslt.Rows[0]["PACK_EQSGID"]);
                        item.MDLLOT_ID = Util.NVC(dtRslt.Rows[0]["MDLLOT_ID"]);
                        item.PRJT_NAME = Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]);

                        decimal total_qty = 0;
                        decimal.TryParse(Util.NVC(dtRslt.Rows[0]["TOTAL_QTY"]), out total_qty);
                        item.TOTAL_QTY = total_qty;

                        item.BOXTYPE = Util.NVC(dtRslt.Rows[0]["BOXTYPE"]);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                dataGrid.UpdateLayout();
                //tbTotCount.Text = dataGrid.GetRowCount().ToString();

            }

            if (_holdTrgtCode == "LOT")
                Hold_Validation(Vm.Items);
            else if (_holdTrgtCode == "SUBLOT")
            {
                //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
                DataTable dtInfo = new DataTable();

                dtInfo.Columns.Add("CHK", typeof(int));
                dtInfo.Columns.Add("STRT_SUBLOTID", typeof(string));
                dtInfo.Columns.Add("HOLD_REG_QTY", typeof(int));


                foreach (HoldModel item in Vm.Items)
                {

                    DataRow dr = dtInfo.NewRow();
                    dr["CHK"] = true;
                    dr["STRT_SUBLOTID"] = item.STRT_SUBLOTID;
                    dr["HOLD_REG_QTY"] = 1;
                    dtInfo.Rows.Add(dr);

                }
                    ///Hold_Validation_Sublot(dtInfo);  
                    ///2024.08.22 아래 METHOD로 변경 확인필요
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
                od.FileName = "Hold_LIST_Upload_Sample.xlsx";

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

                    else if (_holdTrgtCode == "BOX")
                    {
                        sheet[0, 0].Value = "BOXID";
                        sheet[1, 0].Value = "XXX4DZ9576";
                        sheet[2, 0].Value = "XDY9FA2556";
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

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
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
                ObservableCollectionHold<HoldModel> _items = new ObservableCollectionHold<HoldModel>();
                
                loadingIndicator.Visibility = Visibility.Visible;


                #region Comment by kimgwango on 2021.12.17
                //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                //dtInfo.Clear();
                #endregion

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
                            #region LOT

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

                                #region [C20210615-000524] Commented by kimgwango on 2021.12.18
                                //DataRow dr = dtInfo.NewRow();
                                //dr["CHK"] = true;
                                //dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();

                                //double dTmp = 0;
                                //string sTmp = Util.NVC(sheet.GetCell(rowInx, 1)?.Text);
                                //if (double.TryParse(sTmp, out dTmp))
                                //{
                                //    dr["HOLD_REG_QTY"] = dTmp;
                                //}

                                //dtInfo.Rows.Add(dr);
                                #endregion

                                #region [C20210615-000524] Added by kimgwango on 2021.12.16
                                HoldModel holdmodel = new HoldModel();
                                holdmodel.CHK = true;
                                holdmodel.ASSY_LOTID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();

                                decimal dTmp = 0;
                                string sTmp = Util.NVC(sheet.GetCell(rowInx, 1)?.Text);
                                if (decimal.TryParse(sTmp, out dTmp))
                                {
                                    holdmodel.HOLD_REG_QTY = dTmp;
                                }

                                _items.Add(holdmodel);
                                #endregion
                            }

                            #region [C20210615-000524] Commented by kimgwango on 2021.12.18
                            //if (dtInfo.Rows.Count > 0)
                            //    dtInfo = dtInfo.DefaultView.ToTable(true);

                            //GetAssyHoldLotInfo(dtInfo);
                            #endregion

                            #region [C20210615-000524] Added by kimgwango on 2021.12.18
                            DataTable dtLot = new DataTable();

                            if (_items.Count > 0)
                                dtLot = GetAssyHoldLotInfo(_items);
                            #endregion

                            if (dtLot.Rows.Count < 1)
                            {
                                //입력한 LOT ID 가 없습니다.
                                Util.MessageValidation("SFU1813");
                                return;
                            }

                            GridSetData<HoldModel>(dgHold, dtLot, FrameOperation);
                            ///tbTotCount.Text = dgHold.GetRowCount().ToString();

                            Hold_Validation(_items);

                            btnAdd.IsEnabled = false;
                            #endregion
                        }
                        else if (_holdTrgtCode == "SUBLOT")
                        {
                            xProgress.Visibility = Visibility.Visible;
                            xTextBlock.Visibility = Visibility.Visible;
                            xTextStatus.Visibility = Visibility.Visible;
                            //tbTot.Visibility = Visibility.Hidden;
                            //tbTotCount.Visibility = Visibility.Hidden;

                            #region SUBLOT
                            if (sheet.GetCell(0, 0).Text != "CELLID")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }
                            
                            ShowLoadingIndicator();

                            DataTable dtInfo = new DataTable();
                            dtInfo.Columns.Add("CHK", typeof(int));
                            dtInfo.Columns.Add("STRT_SUBLOTID", typeof(string));
                            dtInfo.Columns.Add("HOLD_REG_QTY", typeof(int));

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // LOTID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                #region [C20210615-000524] Commented by kimgwango on 2021.12.18
                                DataRow dr = dtInfo.NewRow();
                                dr["CHK"] = true;
                                //dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text;
                                dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                dr["HOLD_REG_QTY"] = 1;
                                dtInfo.Rows.Add(dr);
                                #endregion

                                #region [C20210615-000524] Added by kimgwango on 2021.12.16
                                HoldModel holdModel = new HoldModel();
                                holdModel.CHK = true;
                                holdModel.STRT_SUBLOTID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                holdModel.HOLD_REG_QTY = 1;
                                _items.Add(holdModel);
                                #endregion

                                double div = Convert.ToDouble(rowInx) / Convert.ToDouble(sheet.Rows.Count);
                                int iPgrs = Convert.ToInt32((25) * div);
                                xProgress.Maximum = 100;
                                xProgress.Minimum = 0;
                                UpdateProgressBar(iPgrs, 100, "Data Uploading...");

                            }

                 

                            #region
                            GridSetData<HoldModel>(dgHold, _items, FrameOperation);
                         
                            #endregion

                            

                            Hold_Validation_Sublot(dtInfo);
                            
                            btnAdd.IsEnabled = false;

                            #endregion
                        }
                        // _holdTrgtCode == "BOX" 일때 
                        else if (_holdTrgtCode == "BOX")
                        {
                            #region BOX
                            if (sheet.GetCell(0, 0).Text != "BOXID")
                            {
                                //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                                Util.MessageValidation("SFU4424");
                                return;
                            }

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // BOXID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                #region [C20210615-000524] Added by kimgwango on 2021.12.16
                                HoldModel holdModel = new HoldModel();
                                holdModel.CHK = true;
                                holdModel.BOXID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                holdModel.BOXTYPE = "PLT";
                                _items.Add(holdModel);
                                #endregion

                            }

                            #region [C20210615-000524] Commented by kimgwango on 2021.12.21
                            //DataTable dtBox = new DataTable();

                            //dtBox = GetPalletInfo_EXCEL(dtInfo);

                            //if (dtBox.Rows.Count < 1)
                            //{
                            //    //입력한 Pallet가 존재하지 않습니다. 
                            //    Util.MessageValidation("SFU3394");
                            //    return;
                            //}

                            //Util.GridSetData(dgHold, dtBox, FrameOperation);
                            //tbTotCount.Text = dgHold.GetRowCount().ToString();
                            #endregion

                            #region [C20210615-000524] Added by kimgwango on 2021.12.21
                            DataTable dtBox = new DataTable();
                            dtBox = GetPalletInfo_EXCEL(_items);

                            if (dtBox.Rows.Count < 1)
                            {
                                //입력한 Pallet가 존재하지 않습니다. 
                                Util.MessageValidation("SFU3394");
                                return;
                            }

                            GridSetData<HoldModel>(dgHold, dtBox, FrameOperation);
                            //tbTotCount.Text = dgHold.GetRowCount().ToString();
                            #endregion

                            #endregion
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
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// HOLD 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.16
            //DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            ////dt.Columns["ASSY_LOTID"].AllowDBNull = true;
            //DataRow dr = dt.NewRow();
            //if (_holdTrgtCode == "SUBLOT")
            //    dr["HOLD_REG_QTY"] = 1;

            //dt.Rows.Add(dr);

            //Util.GridSetData(dgHold, dt, FrameOperation);
            //dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
            #endregion


            #region [C20210615-000524] Added by kimgwango on 2021.12.16
            var item = new HoldModel();
            //item.DataGrid = dgHold;
            //item.HOLD_TRGT_CODE = _holdTrgtCode;
            item.HOLD_REG_QTY = 1;

            Vm.Items.Add(item);

            dgHold.ScrollIntoView(Vm.Items.Count - 1, 0);
            #endregion
        }

        /// <summary>
        /// HOLD 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.16
            //DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);

            //List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            //if (drList.Count > 0)
            //{
            //    foreach (DataRow dr in drList)
            //    {
            //        dt.Rows.Remove(dr);
            //    }
            //    Util.GridSetData(dgHold, dt, FrameOperation);
            //    chkAll.IsChecked = false;
            //}
            //tbTotCount.Text = dgHold.GetRowCount().ToString();

            //tbTotCount.Text = dgHold.GetRowCount().ToString();
            #endregion


            #region [C20210615-000524] Added by kimgwango on 2021.12.16 
            var query = Vm.Items.Where(x => x.CHK == true).ToList();
            foreach (var item in query)
            {
                Vm.Items.Remove(item);
            }

            ///tbTotCount.Text = dgHold.GetRowCount().ToString();
            #endregion
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
            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            if (Vm.Items.Count() < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }
            #endregion

            /////
           
            /////
            
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

            string sRet = cboHoldType.SelectedValue.ToString();

            if (string.IsNullOrEmpty(sRet) || (sRet.ToUpper() == "SELECT"))
            {
                //사유는 필수입니다
                Util.MessageValidation("SFU1593");
                return;
            }
            
            #region [C20210615-000524] Added by kimgwango on 2121.12.16
            foreach (HoldModel item in Vm.Items)
            {
                StringBuilder err = new StringBuilder();

                switch (_holdTrgtCode)
                {
                    case "BOX":
                        err.Append(item["BOXID"]);
                        break;
                    case "LOT":
                        err.Append(item["ASSY_LOTID"]);
                        err.Append(item["HOLD_REG_QTY"]);
                        break;
                    case "SUBLOT":
                        err.Append(item["STRT_SUBLOTID"]);
                        break;
                }

                if (!string.IsNullOrEmpty(err.ToString()))
                {
                    Util.MessageValidation("SFU8216");
                    try
                    {
                        dgHold.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

                        // Filter 적용 시 Exception 발생
                        int rowIdx = dgHold.Rows.IndexOf(item);
                        dgHold.ScrollIntoView(rowIdx, 0);
                    }
                    catch(Exception ex)
                    {
                    }

                    return;
                }
            }
            #endregion

            //SFU1345	HOLD 하시겠습니까?
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    holdGrID = string.Empty;

                    if (_holdTrgtCode != "LOT")
                    {
                        hold_check(Vm.Items);
                    }
                    else
                    {
                        holdGrID = string.Empty;
                        Save(Vm.Items);
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
                inDataTable.Columns.Add("HOLD_CODE", typeof(string));

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
                newRow["HOLD_CODE"] = Util.GetCondition(cboHoldType, "SFU1593");//사유는 필수입니다

                if (newRow["HOLD_CODE"].Equals(""))
                {
                    return;
                }

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                string bizName = "BR_PRD_REG_ASSY_HOLD";  

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
                
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,INHOLD", null, (result, exception) =>
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

        #region CELL HOLD 시 저장 전  HOLD 가능 여부 CELL만 체크되었는지 확인.
        private async void hold_check(ObservableCollection<HoldModel> items)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
                    PHoldCellT.Clear();
                    PHoldCellT.Columns.Add("HOLD_POSS_FLAG", typeof(string));
                    PHoldCellT.Columns.Add("STRT_SUBLOTID", typeof(string));
                    PHoldCellT.Columns.Add("HOLD_REG_QTY", typeof(int));
                    string NHoldCell = string.Empty;
                    string PHoldCell = string.Empty;
                    int NHoldCellCount = 0;
                    int chkResultCount = 0;

                    ShowLoadingIndicator();

                    for (int i = 0; i < items.Count; i++)
                    {
                        // CELL 등록 시 Hold 불가한 대상은 Hold 할 수 없도록
                        if (items[i].CHK.Equals(true) && items[i].HOLD_POSS_FLAG.Equals(false))
                        {
                            NHoldCell += items[i]["STRT_SUBLOTID"] + "  ";
                            NHoldCellCount += 1;
                        }
                        else if (items[i].CHK.Equals(true) && items[i].HOLD_POSS_FLAG.Equals(true))
                        {
                            PHoldCell += items[i].STRT_SUBLOTID + "  ";
                            DataRow newRow = PHoldCellT.NewRow();
                            newRow["STRT_SUBLOTID"] = items[i].STRT_SUBLOTID;
                            newRow["HOLD_POSS_FLAG"] = items[i].HOLD_POSS_FLAG;
                            newRow["HOLD_REG_QTY"] = 1;
                            PHoldCellT.Rows.Add(newRow);
                        }
                        if (items[i].CHK.Equals(true)) chkResultCount += 1;
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

                                NotHoldCell.Clear();
                                int iRowCnt = PHoldCellT.Rows.Count;
                                int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / sDiv)));
                                int iQuot = iQuotient;

                                if (iRowCnt % sDiv != 0)
                                    iQuot += 1;

                                if (iRowCnt <= sDiv)
                                    iQuotient = 0;

                                int idxEnd = 0;

                                if (iRowCnt <= sDiv)
                                    idxEnd = iRowCnt;

                                xProgress.Maximum = iRowCnt;
                                xProgress.Minimum = 0;
                                UpdateProgressBar(0, iRowCnt, " HOLDING START...");

                                for (int idx = 0; idx < iQuot; idx++)
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

                        NotHoldCell.Clear();
                        int iRowCnt = PHoldCellT.Rows.Count;
                        int iQuotient = Convert.ToInt32(System.Math.Truncate(Convert.ToDecimal(iRowCnt / sDiv)));
                        int iQuot = iQuotient;

                        if (iRowCnt% sDiv != 0)
                            iQuot += 1;

                        if (iRowCnt <= sDiv)
                            iQuotient = 0;

                        int idxEnd = 0;

                        if (iRowCnt <= sDiv)
                            idxEnd = iRowCnt;

                        xProgress.Maximum = iRowCnt;
                        xProgress.Minimum = 0;
                        UpdateProgressBar(0, iRowCnt, " HOLDING START...");

                        for (int idx = 0; idx < iQuot; idx++)
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
                        inDataTable.Columns.Add("HOLD_CODE", typeof(string));

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
                        newRow["HOLD_CODE"] = Util.GetCondition(cboHoldType, "SFU1593");//사유는 필수입니다
                        if (newRow["HOLD_CODE"].Equals(""))
                        {
                            return;
                        }
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
                FCS002_307_NOTHOLD popupProgress = new FCS002_307_NOTHOLD();
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
            FCS002_307_NOTHOLD popup = sender as FCS002_307_NOTHOLD;

            this.grdMain.Children.Remove(popup);
        }
        #endregion

        private void Save(ObservableCollectionHold<HoldModel> items)
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
                inDataTable.Columns.Add("HOLD_CODE", typeof(string));

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
                newRow["HOLD_CODE"] = Util.GetCondition(cboHoldType, "SFU1593");//사유는 필수입니다

                if (newRow["HOLD_CODE"].Equals(""))
                {
                    return;
                }

                inDataTable.Rows.Add(newRow);
                newRow = null;
                
                string bizName = "BR_SET_ASSY_HOLD_BX";
                
                for (int row = 0; row < items.Count; row++)
                {

                    if (_holdTrgtCode != "LOT")
                    {
                        // CELL 등록 시 Hold 불가한 대상은 Hold 할 수 없도록
                        if (items[row].HOLD_POSS_FLAG.ToString().ToUpper().Equals("FALSE"))
                            continue;
                    }

                    newRow = inHoldTable.NewRow();
                    newRow["ASSY_LOTID"] = items[row].ASSY_LOTID;
                    newRow["STRT_SUBLOTID"] = items[row].STRT_SUBLOTID;
                    newRow["END_SUBLOTID"] = items[row].END_SUBLOTID;
                    newRow["HOLD_REG_QTY"] = items[row].HOLD_REG_QTY;
                    inHoldTable.Rows.Add(newRow);
                }


                if (inHoldTable.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    this.DialogResult = MessageBoxResult.Cancel;
                    this.Close();
                }


                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,INHOLD", null, (result, exception) =>
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
                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        txtUser.IsReadOnly = true;      //값이 들어간 이후 비활성화

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

        #region [ Event ] - Button
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        #endregion

        #region [ Event ]
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
        #endregion

        #region [ Util - DA ] - Orignal
        private DataTable GetPalletInfo(string sBoxID)
        {
            try
            {
                DataTable dtResult = new DataTable();

                if (sBoxID.Trim().Equals(""))
                {
                    dtResult.Clear();
                    return dtResult;
                }
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("BOXTYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["BOXID"] = sBoxID;
                dr["BOXTYPE"] = "PLT";
                inTable.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", inTable);

                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetPalletInfo_EXCEL(DataTable dtExcel)
        {
            try
            {
                DataTable dtResult = new DataTable();
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", dtExcel);

                dtResult.Columns.Add("CHK");

                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        dtResult.Rows[i]["CHK"] = true;
                    }
                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
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
                //포장 Hold 관리 - Lot Hold 등록 시 보류수량 정보 조회 
                //WIP PROCID = 'A9000' PACKING 존재,WIP_TYPE_CODE('PROD').  2023.03.14
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_LOT_HOLD_REG_QTY_INFO", "INDATA", "OUTDATA", inTable);
                //2023.06.01
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_LOT_HOLD_REG_QTY_INFO_MB", "INDATA", "OUTDATA", inTable);

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
                //포장 Hold 관리 - Lot Hold 등록 시 보류수량 정보 조회:WIP정보 조회 2023.06.01
                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST", "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST_MB", "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
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
        #endregion

        #region [ Util - DA ]
        private DataTable GetAssyHoldLotInfo(ObservableCollection<HoldModel> items)
        {
            DataSet dsResult = new DataSet();
            DataTable dtResult = new DataTable();

            try
            {
                if (items.Count < 1) return dtResult;

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

                for (int i = 0; i < items.Count; i++)
                {
                    DataRow dr = inLotTable.NewRow();
                    dr["ASSY_LOTID"] = items[i].ASSY_LOTID;
                    dr["HOLD_REG_QTY"] = items[i].HOLD_REG_QTY;

                    inLotTable.Rows.Add(dr);
                }


                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_ASSY_LOT_HOLD_REG_QTY_LIST_BX", "INDATA, IN_LOT", "OUTDATA", inDataSet); //BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    dtResult = dsResult.Tables["OUTDATA"];
                    return dtResult;
                }
                                
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return dtResult;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Hold_Validation(ObservableCollection<HoldModel> items)
        {
            try
            {

                if (items.Count() < 1)
                {
                    //FM_ME_240	처리할 데이터가 없습니다.
                    Util.MessageValidation("FM_ME_0240");
                    return;
                }
                
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inDataSet = new DataSet();
                DataTable rqstDataTable = inDataSet.Tables.Add("RQSTDT");

                string bizruleName = string.Empty;

                if (_holdTrgtCode == "LOT")
                {
                    //bizruleName = "DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_LOT_VALD_MB";
                    bizruleName = "BR_GET_ASSY_HOLD_LOT_VALD_MB";

                    rqstDataTable.Columns.Add("LOTID", typeof(string));
                    rqstDataTable.Columns.Add("LANGID", typeof(string));

                    for (int i = 0; i < items.Count; i++)
                    {

                        if (Util.NVC(items[i].VAL_FLAG).ToUpper().Equals("TRUE"))
                            continue;

                        if (string.IsNullOrEmpty(items[i].ASSY_LOTID)) 
                            continue;

                        if (Util.NVC(items[i].ASSY_LOTID.ToString()) == "")
                            continue;

                         DataRow rqstDr = rqstDataTable.NewRow();

                        rqstDr["LOTID"] = Util.NVC(items[i].ASSY_LOTID.ToString());
                        rqstDr["LANGID"] = LoginInfo.LANGID;
                     

                        rqstDataTable.Rows.Add(rqstDr);
                    }

                    if (rqstDataTable.Rows.Count == 0)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }
                    //DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizruleName, "RQSTDT", "RSLTDT", rqstDataTable);
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizruleName, "INDATA", "OUTDATA", rqstDataTable);
                    if (_holdTrgtCode == "LOT")
                    {
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            for (int i = 0; i < dgHold.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "ASSY_LOTID").ToString() == dr["PKG_LOTID"].ToString())
                                {
                                    
                                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_310", Convert.ToDecimal(dr["WH_310"]));
                                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_5100", Convert.ToDecimal(dr["WH_5100"]));
                                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "WH_5000", Convert.ToDecimal(dr["WH_5000"]));
                                    DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "VAL_FLAG", true);
                                }
                            }
                        }
                    }
                    Hold_Validation_Backgorund();

                }
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
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                            int idxEnd = (((idx + 1) * iDiv) - 1);

                            if (idx == iQuotient)
                                idxEnd = iRowCnt - 1; //2024.07.30 x <= idxEnd 조건을 만족할려면 -1 해야 함

                            if (iRowCnt <= iDiv)
                                idxEnd = iRowCnt - 1; //2024.07.30 x <= idxEnd 조건을 만족할려면 -1 해야 함
                            //2024.07.30 HOLD Cell Excel import 시 500*idx번째 Cell 누락 오류 조치
                            for (int x = idxStart; x <= idxEnd; x++)
                            {
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
                            }
                            iSeq++;

                            double div = Convert.ToDouble(idx) / Convert.ToDouble(iQuotient);

                            if (iQuotient == 0)
                                div = 1;

                            int iPgrs = 25 + Convert.ToInt32((25) * div);
                            UpdateProgressBar(iPgrs, 100, "Data Query...");

                            HiddenLoadingIndicator();
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

                    new ClientProxy().ExecuteService_Multi("BR_GET_ASSY_HOLD_SUBLOT_VALD_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
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

                                if (!dtResult.Columns.Contains("ASSY_LOTID"))
                                    dtResult.Columns.Add("ASSY_LOTID", typeof(string));

                                if (!dtResult.Columns.Contains("END_SUBLOTID"))
                                    dtResult.Columns.Add("END_SUBLOTID", typeof(string));

                                if (!dtResult.Columns.Contains("HOLD_REG_QTY"))
                                    dtResult.Columns.Add("HOLD_REG_QTY", typeof(int));
                                
                                GridSetData<HoldModel>(dgHold, dtResult, FrameOperation);
                            }
                            UpdateProgressBar(100, 100, "Completed.");
                            
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

        //private void btnRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    this.ClearValidation();
        //    DataGridRowAdd(dgHold, Convert.ToInt32(txtRowCntInsertCell.Text));



        //}

        //private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();

        //        ////

        //        ////

        //        foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
        //        {
        //            dt.Columns.Add(Convert.ToString(col.Name));
        //        }

        //        if (dg.ItemsSource != null)
        //        {
        //            for (int i = 0; i < iRowcount; i++)
        //            {
        //                dt = DataTableConverter.Convert(dg.ItemsSource);
        //                DataRow dr2 = dt.NewRow();
        //                dt.Rows.Add(dr2);
        //                dg.BeginEdit();
        //                dg.ItemsSource = DataTableConverter.Convert(dt);
        //                dg.EndEdit();
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < iRowcount; i++)
        //            {
        //                DataRow dr = dt.NewRow();
        //                dt.Rows.Add(dr);
        //                dg.BeginEdit();
        //                dg.ItemsSource = DataTableConverter.Convert(dt);
        //                dg.EndEdit();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

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

                new ClientProxy().ExecuteService_Multi("BR_GET_ASSY_HOLD_SUBLOT_VALD_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
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

        private DataTable GetPalletInfo_EXCEL(ObservableCollection<HoldModel> items)
        {
            DataTable dtResult = new DataTable();

            try
            {
                if (items.Count < 1) return dtResult;

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("BOXTYPE", typeof(string));
                
                for (int i = 0; i < items.Count; i++)
                {
                    DataRow dr = inDataTable.NewRow();
                    dr["BOXID"] = items[i].BOXID;
                    dr["BOXTYPE"] = items[i].BOXTYPE;

                    inDataTable.Rows.Add(dr);
                }

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", inDataTable);

                dtResult.Columns.Add("CHK", typeof(bool));

                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        dtResult.Rows[i]["CHK"] = true;
                    }

                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtResult;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #region CWA Level Hold 코드 선택
        private void chkHoldLevel_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (cboHoldType.Items.Count > 1)
                cboHoldType.SelectedIndex = 0;

            if (checkBox.IsChecked == true)
            {
                GetAreaDefectCode();
                cboHoldType.IsEnabled = false;
            }
            else
            {
                Util.gridClear(dgHoldGroup1);
                Util.gridClear(dgHoldGroup2);
                cboHoldType.IsEnabled = true;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.DataContext == null)
                return;

            if ((bool)radioButton.IsChecked)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = (C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent;
                if (cellPresenter != null)
                {
                    C1.WPF.DataGrid.C1DataGrid dataGrid = cellPresenter.DataGrid;
                    int rowIdx = cellPresenter.Row.Index;
                    //dataGrid.SelectedIndex = rowIdx;

                    if (string.Equals(radioButton.GroupName, "radHoldGroup1"))
                    {
                        if (cboHoldType.Items.Count > 1)
                            cboHoldType.SelectedIndex = 0;

                        GetAreaDefectDetailCode(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "DFCT_CODE")));
                    }
                    else if (string.Equals(radioButton.GroupName, "radHoldGroup2"))
                    {
                        cboHoldType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "RESNCODE"));
                    }
                }
            }
        }

        private void GetAreaDefectCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("AREATYPE", typeof(string));


                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["ACTID"] = "HOLD_LOT";
                dataRow["AREATYPE"] = GetAreaType();

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup1, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }



        private void GetAreaDefectDetailCode(string sDefectCode)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("DFCT_CODE", typeof(string));
                dt.Columns.Add("AREATYPE", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["ACTID"] = "HOLD_LOT";
                dataRow["DFCT_CODE"] = sDefectCode;
                dataRow["AREATYPE"] = GetAreaType(); 

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_DETL_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup2, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool IsCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private String setExpectTime(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["ATTRIBUTE2"].ToString()))
                        return row["ATTRIBUTE1"].ToString();
                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["ATTRIBUTE3"].ToString()) && string.Equals("", row["ATTRIBUTE2"].ToString()))
                        return row["ATTRIBUTE1"].ToString();
            }
            catch (Exception ex) { }

            return "";
        }
        #endregion

        #endregion

        #region [ UIHelper ]
        public void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.20 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            ObservableCollectionHold<T> _items = (dataGrid.ItemsSource as ObservableCollectionHold<T>);
            _items.Clear();

            DataTableHoldHelper.CopyToObservableCollectionFromTable(dt, _items);
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        public void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, ObservableCollection<T> oc, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.20 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            ObservableCollectionHold<T> _items = (dataGrid.ItemsSource as ObservableCollectionHold<T>);
            _items.Clear();

            if (oc.Count > 0)
            {
                foreach (var item in oc)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (oc.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(oc.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && oc.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        private void DataGridLoaded(object sender, RoutedEventArgs args)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
            double sumHeight = dataGrid.ActualHeight;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            dataGrid.UpdateLayout();
            dataGrid.Measure(new Size(sumWidth, sumHeight));

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                if (dgc.ActualWidth > 0)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        }
        #endregion
    }

    #region [ Model ] - HoldModel
    public class HoldViewModel:BindableBase
    {
        private ObservableCollectionHold<HoldModel> _items;
        public ObservableCollectionHold<HoldModel> Items
        {

            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public C1DelegateCommand SelectAllCommand { get; set; }

        public HoldViewModel()
        {
            Items = new ObservableCollectionHold<HoldModel>();

            SelectAllCommand = new C1DelegateCommand(new Action<object>(p =>
            {
                CheckBox chk = (CheckBox)p;

                // [C20210615-000524] Added by kimgwango on 2021.12.17 --<
                foreach (var item in Items)
                    item.CHK = (bool)chk.IsChecked;
                // [C20210615-000524] Added by kimgwango on 2021.12.17 >--
            }));
        }

        public HoldViewModel(C1DataGrid dataGrid, string holdTrgtCode)
        {
            Items = new ObservableCollectionHold<HoldModel> (dataGrid, holdTrgtCode);
        }
    }

    public class ObservableCollectionHold<T> : ObservableCollection<T>
    {
        private C1DataGrid _dataGrid;
        public C1DataGrid DataGrid
        {
            get { return _dataGrid; }
            set { _dataGrid = value; }
        }

        private string _hold_trgt_code;
        public string HOLD_TRGT_CODE
        {
            get { return _hold_trgt_code; }
            set { _hold_trgt_code = value; }
        }

        private bool _chk;
        public bool CHK
        {
            get { return _chk; }
            set { _chk = value; }
        }

        private string _pkg_lotid;

        public string PKG_LOTID
        {
            get { return _pkg_lotid; }
            set { _pkg_lotid = value; }
        }

        private string _curr_loc;

        public string CURR_LOC
        {
            get { return _curr_loc; }
            set {  _curr_loc = value; }
        }
        public new void Add(T item)
        {
            PropertyInfo p;

            #region Essential Properties
            // find the property for the column
            p = item.GetType().GetProperty("DataGrid");

            // if exists, set the value
            if (p != null)
            {
                p.SetValue(item, this.DataGrid, null);
            }

            // find the property for the column
            p = item.GetType().GetProperty("HOLD_TRGT_CODE");

            // if exists, set the value
            if (p != null && string.IsNullOrEmpty(p.GetValue(item)?.ToString()))
            {
                p.SetValue(item, this.HOLD_TRGT_CODE, null);
            }
            #endregion

            // find the property for the column
            p = item.GetType().GetProperty("CHK");

            // if exists, set the value
            if (p != null && this.CHK && !(bool)p.GetValue(item))
            {
                //p.SetValue(item, this.CHK, null);
                p.SetValue(item, true, null);
            }

            base.Add(item);
        }

        public ObservableCollectionHold() : base()
        {
        }

        public ObservableCollectionHold(string holdTrgtCode)
        {
            HOLD_TRGT_CODE = holdTrgtCode;
        }

        public ObservableCollectionHold(C1DataGrid dataGrid, string holdTrgtCode)
        {
            DataGrid = dataGrid;
            HOLD_TRGT_CODE = holdTrgtCode;
        }
    }

    public class HoldModel : BindableBase, IDataErrorInfo
    {
        #region properties for common
        private C1DataGrid _dataGrid;
        public C1DataGrid DataGrid
        {
            get { return _dataGrid; }
            set { _dataGrid = value; }
        }

        private bool _chk;
        public bool CHK
        {
            get { return _chk; }
            set { SetProperty(ref _chk, value); }
        }

        private string _hold_trgt_code;
        public string HOLD_TRGT_CODE
        {
            get { return _hold_trgt_code; }
            set { SetProperty(ref _hold_trgt_code, value); }
        }
        #endregion

        #region properties fo DB
        private string _assy_lotid;
        public string ASSY_LOTID
        {
            get { return _assy_lotid; }
            set { SetProperty(ref _assy_lotid, value); }
        }

        private decimal _hold_reg_qty;
        public decimal HOLD_REG_QTY
        {
            get { return _hold_reg_qty; }
            set { SetProperty(ref _hold_reg_qty, value); }
        }

        private decimal _hold_reg_max_qty;
        public decimal HOLD_REG_MAX_QTY
        {
            get { return _hold_reg_max_qty; }
            set { SetProperty(ref _hold_reg_max_qty, value); }
        }

        private string _strt_sublotid;
        public string STRT_SUBLOTID
        {
            get { return _strt_sublotid; }
            set { SetProperty(ref _strt_sublotid, value); }
        }

        private string _end_sublotid;
        public string END_SUBLOTID
        {
            get { return _end_sublotid; }
            set { SetProperty(ref _end_sublotid, value); }
        }

        private string _boxid;
        public string BOXID
        {
            get { return _boxid; }
            set { SetProperty(ref _boxid, value); }
        }

        private string _prodid;
        public string PRODID
        {
            get { return _prodid; }
            set { SetProperty(ref _prodid, value); }
        }

        private string _pack_eqsgid;
        public string PACK_EQSGID
        {
            get { return _pack_eqsgid; }
            set { SetProperty(ref _pack_eqsgid, value); }
        }

        private string _mdllot_id;
        public string MDLLOT_ID
        {
            get { return _mdllot_id; }
            set { SetProperty(ref _mdllot_id, value); }
        }

        private string _prjt_name;
        public string PRJT_NAME
        {
            get { return _prjt_name; }
            set { SetProperty(ref _prjt_name, value); }
        }

        private decimal _total_qty;
        public decimal TOTAL_QTY
        {
            get { return _total_qty; }
            set { SetProperty(ref _total_qty, value); }
        }

        private string _boxtype;

        public string BOXTYPE
        {
            get { return _boxtype; }
            set { SetProperty(ref _boxtype, value); }
        }

        private bool _val_flag;
        public bool VAL_FLAG
        {
            get { return _val_flag; }
            set { SetProperty(ref _val_flag, value); }
        }

        private decimal _wh_310;
        public decimal WH_310
        {
            get { return _wh_310; }
            set { SetProperty(ref _wh_310, value); }
        }

        private decimal _wh_5100;
        public decimal WH_5100
        {
            get { return _wh_5100; }
            set { SetProperty(ref _wh_5100, value); }
        }

        private decimal _wh_5000;
        public decimal WH_5000
        {
            get { return _wh_5000; }
            set { SetProperty(ref _wh_5000, value); }
        }

        private bool _hold_poss_flag;
        public bool HOLD_POSS_FLAG
        {
            get { return _hold_poss_flag; }
            set { SetProperty(ref _hold_poss_flag, value); }
        }

        private string _pkg_lotid;

        public string PKG_LOTID
        {
            get { return _pkg_lotid; }
            set { SetProperty(ref _pkg_lotid, value); }
        }

        private string _curr_loc;

        public string CURR_LOC
        {
            get { return _curr_loc; }
            set { SetProperty(ref _curr_loc, value); }
        }
        #endregion

        #region IDataErrorInfo
        public string Error
        {
            get
            {
                // perform item-level validation: Price must be > Cost
                //if (Price <= Cost)
                //{
                //    return string.Format("Price must be > Cost ({0:c2})", Cost);
                //}

                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                StringBuilder error = new StringBuilder();
                StringBuilder errorRow = new StringBuilder();
                string rowName = string.Empty;

                if (DataGrid == null) return string.Empty;

                switch (HOLD_TRGT_CODE)
                {
                    case "LOT":
                        #region LOT
                        try
                        {
                            switch (columnName)
                            {
                                case "ASSY_LOTID":
                                    if (string.IsNullOrWhiteSpace(ASSY_LOTID))
                                    {
                                        //SFU4351		미입력된 항목이 존재합니다.	
                                        error.AppendLine(MessageHelper.MessageValidation("SFU4351"));
                                    }
                                    else if (ASSY_LOTID.Length < 8)
                                    {
                                        // 조립 LOT ID를 8자리 이상 입력해주세요.
                                        error.AppendLine(MessageHelper.MessageValidation("100841", ASSY_LOTID));
                                    }
                                    else if (IsLotIDDuplecate(DataGrid, this))
                                    {
                                        // 입력한 LOT [%1]은 LOT ID가 이미 존재합니다.
                                        error.AppendLine((MessageHelper.MessageValidation("1103", ASSY_LOTID)));
                                    }
                                    else if (!IsLotIDExist(this))
                                    {
                                        // 대상 LOT[% 1] 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1026", ASSY_LOTID));
                                    }
                                    else if (IsLotIDDuplecate(this))
                                    {
                                        // LOT [%1]는 HOLD되어 있습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1029", ASSY_LOTID));
                                    }
                                    break;
                                case "HOLD_REG_QTY":
                                    if (HOLD_REG_QTY < 1)
                                    {
                                        //수량은 0보다 커야 합니다.
                                        error.AppendLine(MessageHelper.MessageValidation("SFU1683"));
                                    }

                                    rowName = "ROWERROR";
                                    if (HOLD_REG_MAX_QTY > 0 && HOLD_REG_QTY > HOLD_REG_MAX_QTY)
                                    {
                                        //C20210729-000334 type 변경 처리
                                        //DataRow[] drList = dtInfo.Select("HOLD_REG_QTY+0 > HOLD_REG_MAX_QTY+0 AND HOLD_REG_MAX_QTY > 0");

                                        // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
                                        errorRow.AppendLine(MessageHelper.MessageValidation("SFU3759", ASSY_LOTID));
                                    }
                                    break;
                                case "HOLD_REG_MAX_QTY":
                                    rowName = "ROWERROR";
                                    if (HOLD_REG_MAX_QTY > 0 && HOLD_REG_QTY > HOLD_REG_MAX_QTY)
                                    {
                                        //C20210729-000334 type 변경 처리
                                        //DataRow[] drList = dtInfo.Select("HOLD_REG_QTY+0 > HOLD_REG_MAX_QTY+0 AND HOLD_REG_MAX_QTY > 0");

                                        // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
                                        errorRow.AppendLine(MessageHelper.MessageValidation("SFU3759", ASSY_LOTID));
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        #endregion
                        break;
                    case "BOX":
                        #region BOX
                        try
                        {
                            switch (columnName)
                            {
                                case "BOXID":
                                    DataTable dt = GetHoldPalletStatus(this);
                                    string holdNote = string.Empty;

                                    if (string.IsNullOrWhiteSpace(BOXID))
                                    {
                                        //SFU4351		미입력된 항목이 존재합니다.	
                                        error.AppendLine(MessageHelper.MessageValidation("SFU4351"));
                                    }
                                    else if (IsBoxIDDuplecate(DataGrid, this))
                                    {
                                        //이미 조회된 Pallet ID 입니다.
                                        error.AppendLine((MessageHelper.MessageValidation("SFU3165")));
                                    }
                                    else if (dt == null || dt.Rows.Count == 0)
                                    {
                                        // BOX [%1]의 포장 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1067", BOXID));
                                    }
                                    else if(dt.Rows[0]["BOXSTAT"].ToString() != "PACKED")
                                    { 
                                        // BOX [%1]은 포장이 완료되지 않았습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1134", BOXID));
                                    }
                                    else if (!string.IsNullOrEmpty(dt.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString()) &&
                                        dt.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString() != "CANCEL_SHIP")
                                    {
                                        // BOX [%1]은 이미 출하 요청되었습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("70031", BOXID));
                                    }
                                    else if (dt.Rows[0]["ISS_HOLD_FLAG"].ToString() == "Y")
                                    {
                                        // BOX [%1] 는 [%2] 사유로 포장 Hold 상태 입니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1182", BOXID, dt.Rows[0]["PACK_NOTE"]));
                                    }
                                    else if (IsPalletHold(this, out holdNote))
                                    {
                                        // BOX [%1] 는 [%2] 사유로 포장 Hold 상태 입니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1182", BOXID, holdNote));
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        #endregion
                        break;
                    case "SUBLOT":
                        #region SUBLOT
                        try
                        {
                            switch (columnName)
                            {
                                case "STRT_SUBLOTID":
                                    if (string.IsNullOrWhiteSpace(STRT_SUBLOTID))
                                    {
                                        //SFU1209		Cell 정보가 없습니다.	
                                        error.AppendLine(MessageHelper.MessageValidation("SFU1209"));
                                    }
                                    else if (STRT_SUBLOTID.Length < 10)
                                    {
                                        // CELL ID[% 1] 자리수를 확인하세요
                                        error.AppendLine(MessageHelper.MessageValidation("100243", STRT_SUBLOTID));
                                    }
                                    else if (IsCellIDDuplecate(DataGrid, this))
                                    {
                                        // Cell ID [%1] 는 중복입니다.
                                        error.AppendLine((MessageHelper.MessageValidation("1128", STRT_SUBLOTID)));
                                    }
                                    else if (!LoginInfo.CFG_SHOP_ID.Equals("G182") && !LoginInfo.CFG_AREA_ID.Equals("S5") && !IsLotIDExist(this))
                                    {
                                        // 대상 LOT[% 1] 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1026", STRT_SUBLOTID));
                                    }
                                    //else if (IsLotIDDuplecate(this))
                                    //{
                                    //    // LOT [%1]는 HOLD되어 있습니다.
                                    //    error.AppendLine(MessageHelper.MessageValidation("1029", STRT_SUBLOTID));
                                    //}
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        #endregion
                        break;

                    default:
                        break;

                }

                #region Column Error
                if (error.Length >= 2)
                    error = error.Replace("\r", "", error.Length - 2, 1).Replace("\n", "", error.Length - 1, 1);

                try
                {
                    int idx = DataGrid.Rows.IndexOf(this);
                    if (idx >= 0)
                    {
                        foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                        {
                            if (item.ColumnNames.Contains(columnName))
                            {
                                DataGrid.Rows[idx].Errors.Remove(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(error.ToString()))
                        {
                            string colHeader = DataGrid.Columns[columnName].Header.ToString();
                            error.Insert(0, string.Format("[{0}]:",colHeader));
                            DataGrid.Rows[idx].Errors.Add(new DataGridRowError(error.ToString(), new string[] { columnName }));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                #region Row Error
                if (errorRow.Length >= 2)
                    errorRow = errorRow.Replace("\r", "", errorRow.Length - 2, 1).Replace("\n", "", errorRow.Length - 1, 1);

                try
                {
                    int idx = DataGrid.Rows.IndexOf(this);
                    if (idx >= 0)
                    {
                        foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                        {
                            if (item.ColumnNames.Contains(rowName))
                            {
                                DataGrid.Rows[idx].Errors.Remove(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(errorRow.ToString()))
                        {
                            string colHeader = DataGrid.Columns["HOLD_REG_QTY"].Header.ToString();
                            errorRow.Insert(0, string.Format("[{0}]:", colHeader));
                            DataGrid.Rows[idx].Errors.Add(new DataGridRowError(errorRow.ToString(), new string[] { rowName }));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                return error.Append(errorRow).ToString();
            }
        }
        #endregion

        #region [ Util ]
        
        #region BOX ID 유효성 체크
        public bool IsBoxIDDuplecate(C1DataGrid grid, HoldModel item)
        {
            bool result = false;

            ObservableCollectionHold<HoldModel> items = (ObservableCollectionHold<HoldModel>)grid.ItemsSource;
            if (items == null) return false;
            if (string.IsNullOrEmpty(item.BOXID)) return false;

            foreach (var _item in items)
            {
                if (_item == item)
                    continue;

                if (_item.BOXID == item.BOXID)
                    return true;
            }

            return result;
        }
        
        public DataTable GetHoldPalletStatus(HoldModel item)
        {
            DataTable result = null;

            #region
            // INDATA
            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("BOXID", typeof(string));
            //inDataTable.Columns.Add("BOXTYPE", typeof(string));
            //inDataTable.Columns.Add("LANGID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["BOXID"] = item.BOXID;
            inDataTable.Rows.Add(newRow);
            #endregion

            try
            {
                string bizName = "BR_PRD_CHK_BOXING_FN_IS_PALLET_STATUS";
                DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return result;
        }

        public bool IsPalletHold(HoldModel item, out string holdNote)
        {
            bool result = false;
            holdNote = string.Empty;

            #region
            // INDATA
            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("PALLETID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["PALLETID"] = item.BOXID;
            inDataTable.Rows.Add(newRow);
            #endregion

            try
            {
                string bizName = "BR_PRD_CHK_BOXING_FN_IS_BOXID_DUPLICATE";
                DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                if (dt != null && dt.Rows.Count > 0)
                {
                    holdNote = dt.Rows[0]["HOLD_NOTE"].ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return result;
        }
        #endregion

        #region LOT ID 유효성 체크
        public bool IsLotIDExist(HoldModel item)
        {
            bool result = false;

            #region
            // INDATA
            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));
            inDataTable.Columns.Add("STRT_SUBLOTID", typeof(string));
            inDataTable.Columns.Add("END_SUBLOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["HOLD_TRGT_CODE"] = item.HOLD_TRGT_CODE;
            newRow["ASSY_LOTID"] = item.ASSY_LOTID;
            newRow["STRT_SUBLOTID"] = item.STRT_SUBLOTID;
            newRow["END_SUBLOTID"] = item.END_SUBLOTID;

            inDataTable.Rows.Add(newRow);
            #endregion

            try
            {
                //string bizName = "BR_PRD_CHK_BOXING_FN_IS_LOTID_EXIST";

                string bizName = "BR_PRD_CHK_BOXING_FN_IS_LOTID_EXIST_MB"; //2023.06.19 소형활성화 분리 필요

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0] == DBNull.Value ? false : Convert.ToBoolean(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return result;
        }

        public bool IsLotIDDuplecate(HoldModel item)
        {
            bool result = false;

            #region
            // INDATA
            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));
            inDataTable.Columns.Add("STRT_SUBLOTID", typeof(string));
            inDataTable.Columns.Add("END_SUBLOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["HOLD_TRGT_CODE"] = item.HOLD_TRGT_CODE;
            newRow["ASSY_LOTID"] = item.ASSY_LOTID;
            newRow["STRT_SUBLOTID"] = item.STRT_SUBLOTID;
            newRow["END_SUBLOTID"] = item.END_SUBLOTID;

            inDataTable.Rows.Add(newRow);
            #endregion

            try
            {
                string bizName = "BR_PRD_CHK_BOXING_FN_IS_LOTID_DUPLICATE";
                DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inDataTable);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0][0] == DBNull.Value ? false : Convert.ToBoolean(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return result;
        }

        public bool IsLotIDDuplecate(C1DataGrid grid, HoldModel item)
        {
            bool result = false;

            ObservableCollectionHold<HoldModel> items = (ObservableCollectionHold<HoldModel>)grid.ItemsSource;
            if (items == null) return false;
            if (string.IsNullOrEmpty(item.ASSY_LOTID)) return false;

            foreach (var _item in items)
            {
                if (_item == item)
                    continue;

                if (_item.ASSY_LOTID == item.ASSY_LOTID)


                    return true;
            }

            return result;
        }
        #endregion
              
        #region CELL ID 유효성 체크
        public bool IsCellIDDuplecate(C1DataGrid grid, HoldModel item)
        {
            bool result = false;

            ObservableCollectionHold<HoldModel> items = (ObservableCollectionHold<HoldModel>)grid.ItemsSource;
            if (items == null) return false;
            if (string.IsNullOrEmpty(item.STRT_SUBLOTID)) return false;

            foreach (var _item in items)
            {
                if (_item == item)
                    continue;

                if (_item.STRT_SUBLOTID == item.STRT_SUBLOTID)
                    return true;
            }

            return result;
        }
        #endregion
        
        #endregion
    }

    public class DataTableHoldHelper : DataTableHelper
    {
        public static void CopyToObservableCollectionFromTable<T>(DataTable tbl, ObservableCollectionHold<T> desc) where T : new()
        {
            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                desc.Add(CreateItemFromRow<T>(r));
            }
        }
    }

    #endregion

    #region [ Common ]
    public class MessageHelper
    {
        public static void ResultMessage(object result, IFrameOperation iFO)
        {
            int resultCount = 0;

            if (result == null) return;

            if(result is ICollection)
                resultCount = (result as ICollection).Count;
            else if (result is DataTable)
                resultCount = (result as DataTable).Rows.Count;

            if (resultCount == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(resultCount + ObjectDic.Instance.GetObjectName("건"));
            }
        }

        public static string MessageValidation(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            return message;

        }

        /// <summary>
        /// Validation MessageBox 호출 메소드
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="parameters"></param>
        public static string MessageValidation(string messageId, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i]?.ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }

            return message;
        }
}

    public class GridSetDataHelper
    {
        public static void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.20 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            ObservableCollection<T> _items = (dataGrid.ItemsSource as ObservableCollection<T>);
            _items.Clear();

            List<T> _list = DataTableHelper.CreateListFromTable<T>(dt);
            if (_list.Count > 0)
            {
                foreach (var item in _list)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        public static void GridSetData<T>(C1.WPF.DataGrid.C1DataGrid dataGrid, ObservableCollection<T> oc, IFrameOperation iFO, bool isAutoWidth = false) where T : new()
        {
            #region [C20210615-000524] Commented by kimgwango on 2021.12.20 
            //gridClear(dataGrid);

            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            ObservableCollection<T> _items = (dataGrid.ItemsSource as ObservableCollection<T>);
            _items.Clear();

            if (oc.Count > 0)
            {
                foreach (var item in oc)
                    _items.Add(item);
            }
            #endregion

            dataGrid.FilterBy(new DataGridColumnValue<DataGridFilterState>[0]);

            if (oc.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(oc.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && oc.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    { 
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        private static void DataGridLoaded(object sender, RoutedEventArgs args)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
            double sumHeight = dataGrid.ActualHeight;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            dataGrid.UpdateLayout();
            dataGrid.Measure(new Size(sumWidth, sumHeight));

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                if (dgc.ActualWidth > 0)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        }
    }

    public class DataTableHelper
    {
        // function that creates a list of an object from the given data table
        public static List<T> CreateListFromTable<T>(DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }

        // function that creates an object from the given data row
        public static T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        public static void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }
    }
    #endregion
}
