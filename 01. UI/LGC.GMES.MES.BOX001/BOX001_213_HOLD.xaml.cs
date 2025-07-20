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
 2023.03.28 김대현                        ESWA 1동 조립 GMES의 포장홀드 시 활성화 전환 동의 Hold Validation
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

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_213_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private Util _Util = new Util();
        string _holdTrgtCode = string.Empty;

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

        public BOX001_213_HOLD()
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
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _holdTrgtCode = tmps[0].ToString();

            Vm.Items = new ObservableCollectionHold<HoldModel>(dgHold,_holdTrgtCode);

            #region [C20210615-000524] Commented by kimgwango on 2021.12.19
            //DataTable dt = new DataTable();
            //for (int i = 0; i < dgHold.Columns.Count; i++)
            //{
            //    dt.Columns.Add(dgHold.Columns[i].Name);
            //}

            ////DataRow dr = dt.NewRow();
            ////dt.Rows.Add(dr);
            //Util.GridSetData(dgHold, dt, FrameOperation);
            #endregion

            if (_holdTrgtCode == "LOT")
            {
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["BOXID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PRODID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PACK_EQSGID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                dgHold.Columns["TOTAL_QTY"].Visibility = Visibility.Collapsed;
                // C20211111-000007 추가 요청
                //chkInputHold.Visibility = Visibility.Collapsed;
            }
            else if (_holdTrgtCode == "BOX")
            {
                Header = "Pallet HOLD 등록";
                dgHold.Columns["STRT_SUBLOTID"].Visibility = Visibility.Collapsed;
                chkInputHold.Visibility = Visibility.Collapsed;
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;

                // 팔레트 바코드 표시 여부
                isVisibleBCD(LoginInfo.CFG_AREA_ID);
            }
            else
            {
                dgHold.Columns["ASSY_LOTID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["HOLD_REG_MAX_QTY"].Visibility = Visibility.Collapsed;
                dgHold.Columns["BOXID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PRODID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PACK_EQSGID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                dgHold.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                dgHold.Columns["TOTAL_QTY"].Visibility = Visibility.Collapsed;
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


                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_QTY", iQty);
                #region C20210615 - 000524    【ESNB 】 Cell hold 등록 에라내용 개선 Commented by kimgwango on 2021.01.14
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_MAX_QTY", iQty);
                #endregion

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
                        item.PLLT_BCD_ID = string.Empty;
                    }
                    #endregion
                    return;
                }

                #region C20210615 - 000524    【ESNB 】 Cell hold 등록 에라내용 개선 Commented by kimgwango on 2021.01.14
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRODID", Util.NVC(dtRslt.Rows[0]["PRODID"]));
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PACK_EQSGID", Util.NVC(dtRslt.Rows[0]["PACK_EQSGID"]));
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MDLLOT_ID", Util.NVC(dtRslt.Rows[0]["MDLLOT_ID"]));
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRJT_NAME", Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]));
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "TOTAL_QTY", Util.NVC(dtRslt.Rows[0]["TOTAL_QTY"]));
                //DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "BOXTYPE", Util.NVC(dtRslt.Rows[0]["BOXTYPE"]));
                #endregion

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
                        // 팔레트 바코드
                        if (dtRslt.Columns.Contains("PLLT_BCD_ID"))
                            item.PLLT_BCD_ID = Util.NVC(dtRslt.Rows[0]["PLLT_BCD_ID"]);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                #endregion

                dataGrid.UpdateLayout();
                tbTotCount.Text = dataGrid.GetRowCount().ToString();

            }
            //if (e.Cell.Column.Name == "HOLD_TRGT_CODE"
            //      && Util.NVC(e.Cell.Value) == "LOT")
            //{
            //    DataTableConverter.SetValue(e.Cell.Row.DataItem, "STRT_SUBLOTID", string.Empty);
            //    DataTableConverter.SetValue(e.Cell.Row.DataItem, "END_SUBLOTID", string.Empty);

            //    dataGrid.Refresh();
            //}

            //else if (e.Cell.Column.Name == "STRT_SUBLOTID" || e.Cell.Column.Name == "END_SUBLOTID")
            //{
            //    //string sLotType = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);
            //    string sStart = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["STRT_SUBLOTID"].Index).Value);
            //    string sEnd = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["END_SUBLOTID"].Index).Value);

            //    int iStart;
            //    int iEnd;

            //    int cnt = 10;
            //    if (Util.NVC(e.Cell.Value).Length < cnt)
            //    {
            //        //SFU4342	[%1] 자리수 이상 입력하세요.
            //        Util.MessageInfo("SFU4342", new object[] { cnt });
            //    }

            //    //else if (!string.IsNullOrWhiteSpace(sStart) && !int.TryParse(sStart, out iStart))
            //    //{
            //    //    //SFU3435	숫자만 입력해주세요
            //    //    Util.MessageInfo("SFU3435");
            //    //}

            //    //else if(!string.IsNullOrWhiteSpace(sEnd) && !int.TryParse(sEnd, out iEnd))
            //    //{
            //    //    Util.MessageInfo("SFU3435");
            //    //}

            //    //else if (sStart.Length != 0 && sEnd.Length != 0 && sStart.Length != sEnd.Length)
            //    //{
            //    //    //SFU4341	시작값과 종료값의 자리수가 동일해야 합니다.	
            //    //    Util.MessageInfo("SFU4341");
            //    //}


            //}
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
                            tbTotCount.Text = dgHold.GetRowCount().ToString();
                            #endregion
                        }
                        else if (_holdTrgtCode == "SUBLOT")
                        {
                            #region SUBLOT
                            if (sheet.GetCell(0, 0).Text != "CELLID")
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
                                ////dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text;
                                //dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                //dr["HOLD_REG_QTY"] = 1;
                                //dtInfo.Rows.Add(dr);
                                #endregion

                                #region [C20210615-000524] Added by kimgwango on 2021.12.16
                                HoldModel holdModel = new HoldModel();
                                holdModel.CHK = true;
                                holdModel.STRT_SUBLOTID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                holdModel.HOLD_REG_QTY = 1;
                                _items.Add(holdModel);
                                #endregion
                            }

                            #region
                            //if (dtInfo.Rows.Count > 0)
                            //    dtInfo = dtInfo.DefaultView.ToTable(true);

                            //Util.GridSetData(dgHold, dtInfo, FrameOperation);
                            #endregion

                            #region
                            GridSetData(dgHold, _items, FrameOperation);
                            tbTotCount.Text = dgHold.GetRowCount().ToString();
                            #endregion;
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

                            // Pallet BCD 컬럼 활성화 상태일 경우만 getPalletBCD 호출 (boxid, pallet bcd 값 적용)
                            bool _chkPalletbcd = false;
                            if (dgHold.Columns["PLLT_BCD_ID"].Visibility == Visibility.Visible)
                                _chkPalletbcd = true;

                            for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                            {
                                // BOXID 미입력시 return;
                                if (sheet.GetCell(rowInx, 0) == null)
                                    return;

                                #region
                                //DataRow dr = dtInfo.NewRow();
                                //dr["CHK"] = true;
                                ////dr["ASSY_LOTID"] = sheet.GetCell(rowInx, 0).Text;
                                //dr["BOXID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                //dr["BOXTYPE"] = "PLT";
                                //dtInfo.Rows.Add(dr);
                                #endregion

                                #region [C20210615-000524] Added by kimgwango on 2021.12.16
                                HoldModel holdModel = new HoldModel();
                                holdModel.CHK = true;
                                // Pallet BCD 조회
                                if (_chkPalletbcd)
                                {
                                    string _boxid = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                    holdModel.BOXID = getPalletBCD(_boxid); 
                                }
                                else
                                {
                                    holdModel.BOXID = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                }

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
                            tbTotCount.Text = dgHold.GetRowCount().ToString();
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

            tbTotCount.Text = dgHold.GetRowCount().ToString();
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
            #region [C20210615-000524] Commented by kimgwango on 2021.12.20 
            //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

            //if (dtInfo.Rows.Count < 1)
            //{
            //    //SFU3552	저장 할 DATA가 없습니다.	
            //    Util.MessageValidation("SFU3552");
            //    return;
            //}

            //if (_holdTrgtCode == "LOT" && dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("ASSY_LOTID"))).ToList().Count > 0)
            //{
            //    //SFU4351		미입력된 항목이 존재합니다.	
            //    Util.MessageValidation("SFU4351");
            //    return;
            //}

            //if (_holdTrgtCode == "LOT" && dtInfo.Select("HOLD_REG_QTY IS NULL OR HOLD_REG_QTY < 1")?.ToList()?.Count > 0)
            //{
            //    //수량은 0보다 커야 합니다.
            //    Util.MessageValidation("SFU1683");
            //    return;
            //}

            //if (_holdTrgtCode == "LOT")
            //{
            //    //C20210729-000334 type 변경 처리
            //    //DataRow[] drList = dtInfo.Select("HOLD_REG_QTY+0 > HOLD_REG_MAX_QTY+0 AND HOLD_REG_MAX_QTY > 0");
            //    DataRow[] drList = dtInfo.Select("Convert(HOLD_REG_QTY, 'System.Decimal') > Convert(HOLD_REG_MAX_QTY, 'System.Decimal') AND Convert(HOLD_REG_MAX_QTY, 'System.Decimal') > 0");
            //    if (drList?.Count() > 0)
            //    {
            //        // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
            //        Util.MessageValidation("SFU3759", Util.NVC(drList[0]["ASSY_LOTID"]));
            //        return;
            //    }
            //}

            //if (_holdTrgtCode == "SUBLOT" && dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("STRT_SUBLOTID"))
            //                            )).ToList().Count > 0)
            //{
            //    //SFU1209		Cell 정보가 없습니다.	
            //    Util.MessageValidation("SFU1209");
            //    return;
            //}
            #endregion

            #region [C20210615-000524] Added by kimgwango on 2021.12.20
            if (Vm.Items.Count() < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }
            #endregion     

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

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4300 Hold 사유를 입력하세요.
                Util.MessageValidation("SFU4300");
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
                    //2023.03.28 김대현
                    //Util.MessageValidation("SFU8216");
                    if (item.PKGRULEAREA)
                    {
                        Util.MessageValidation("SFU8216");
                    }
                    else
                    {
                        Util.MessageValidation(err.ToString());
                    }
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
                    //Save();
                    Save(Vm.Items);
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

                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                //inHoldTable.Columns.Add("HOLD_TRGT_CODE");
                //inHoldTable.Columns.Add("MKT_TYPE_CODE");
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

                string bizName = "BR_PRD_REG_ASSY_HOLD";

                if (_holdTrgtCode == "SUBLOT"
                   && (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))) // 소형파우치 & HOLD_TRGT_CODE = SUBLOT
                {
                    string sXML = string.Empty;

                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        if (row == 0 || row % 150 == 0)
                        {
                            sXML = "<root>";
                        }

                        sXML += "<DT><L>" + dtInfo.Rows[row]["STRT_SUBLOTID"] + "</L></DT>";

                        if ((row + 1) % 150 == 0 || row + 1 == dtInfo.Rows.Count)
                        {
                            sXML += "</root>";

                            newRow = inHoldTable.NewRow();
                            newRow["STRT_SUBLOTID"] = sXML;
                            newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];
                            //newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                            //2020-01-09 최상민 _holdTrgtCode == "SUBLOT" 일 경우, Cell 단위 Hold시 건당 수량이 1이므로 강제로 1로 입력
                            //기존 엑셀 Upload시는 1로 입력하고 수동으로 화면에서 입력하여 Hold 등록시는 컬럼을 미사용하여 null로 진행됨
                            newRow["HOLD_REG_QTY"] = "1";
                            inHoldTable.Rows.Add(newRow);
                        }
                    }
                }
                else if (_holdTrgtCode == "BOX")
                {
                    bizName = "BR_PRD_REG_ASSY_HOLD_PALLET";
                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = dtInfo.Rows[row]["BOXID"]; 
                        inHoldTable.Rows.Add(newRow);
                    }
                }
                else // HOLD_TRGT_CODE = LOT (소형 파우치 , 자동차 공통) or 자동차
                {
                    for (int row = 0; row < dtInfo.Rows.Count; row++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = dtInfo.Rows[row]["ASSY_LOTID"];
                        //newRow["MKT_TYPE_CODE"] = dtInfo.Rows[row]["MKT_TYPE_CODE"];
                        newRow["STRT_SUBLOTID"] = dtInfo.Rows[row]["STRT_SUBLOTID"];
                        newRow["END_SUBLOTID"] = dtInfo.Rows[row]["END_SUBLOTID"];
                        newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                        inHoldTable.Rows.Add(newRow);
                    }
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

                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("ASSY_LOTID", typeof(string));
                //inHoldTable.Columns.Add("HOLD_TRGT_CODE");
                //inHoldTable.Columns.Add("MKT_TYPE_CODE");
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

                //DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                string bizName = "BR_PRD_REG_ASSY_HOLD";

                if (_holdTrgtCode == "SUBLOT"
                   && (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))) // 소형파우치 & HOLD_TRGT_CODE = SUBLOT
                {
                    string sXML = string.Empty;

                    for (int row = 0; row < items.Count; row++)
                    {
                        if (row == 0 || row % 150 == 0)
                        {
                            sXML = "<root>";
                        }

                        sXML += "<DT><L>" + items[row].STRT_SUBLOTID + "</L></DT>";

                        if ((row + 1) % 150 == 0 || row + 1 == items.Count)
                        {
                            sXML += "</root>";

                            newRow = inHoldTable.NewRow();
                            newRow["STRT_SUBLOTID"] = sXML;
                            newRow["ASSY_LOTID"] = items[row].ASSY_LOTID;
                            //newRow["HOLD_REG_QTY"] = dtInfo.Rows[row]["HOLD_REG_QTY"];
                            //2020-01-09 최상민 _holdTrgtCode == "SUBLOT" 일 경우, Cell 단위 Hold시 건당 수량이 1이므로 강제로 1로 입력
                            //기존 엑셀 Upload시는 1로 입력하고 수동으로 화면에서 입력하여 Hold 등록시는 컬럼을 미사용하여 null로 진행됨
                            newRow["HOLD_REG_QTY"] = "1";
                            inHoldTable.Rows.Add(newRow);
                        }
                    }
                }
                else if (_holdTrgtCode == "BOX")
                {
                    bizName = "BR_PRD_REG_ASSY_HOLD_PALLET";
                    for (int row = 0; row < items.Count; row++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = items[row].BOXID;  
                        inHoldTable.Rows.Add(newRow);
                    }
                }
                else // HOLD_TRGT_CODE = LOT (소형 파우치 , 자동차 공통) or 자동차
                {
                    for (int row = 0; row < items.Count; row++)
                    {
                        newRow = inHoldTable.NewRow();
                        newRow["ASSY_LOTID"] = items[row].ASSY_LOTID;
                        //newRow["MKT_TYPE_CODE"] = dtInfo.Rows[row]["MKT_TYPE_CODE"];
                        newRow["STRT_SUBLOTID"] = items[row].STRT_SUBLOTID;
                        newRow["END_SUBLOTID"] = items[row].END_SUBLOTID;
                        newRow["HOLD_REG_QTY"] = items[row].HOLD_REG_QTY;
                        inHoldTable.Rows.Add(newRow);
                    }
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_LOT_HOLD_REG_QTY_INFO", "INDATA", "OUTDATA", inTable);

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST", "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
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

                #region
                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST", "INDATA,IN_LOT", "OUTDATA", (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.MessageException(bizException);
                //            return;
                //        }

                //        if (CommonVerify.HasTableInDataSet(bizResult))
                //        {
                //            DataTable dtResult = bizResult.Tables["OUTDATA"];

                //            GridSetData<HoldModel>(dgHold, dtResult, FrameOperation);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //    }
                //}, inDataSet);
                #endregion


                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ASSY_LOT_HOLD_REG_QTY_LIST", "INDATA, IN_LOT", "OUTDATA", inDataSet);
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
                    dr["BOXID"] = items[i].BOXID.ToString();
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

        /// <summary>
        /// 팔레트 바코드 표시 여부
        /// </summary>
        /// <param name="sAreaID"></param>
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgHold.Columns.Contains("PLLT_BCD_ID"))
                    dgHold.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgHold.Columns.Contains("PLLT_BCD_ID"))
                    dgHold.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }
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
        //2023.03.28 김대현
        private string AreaName = string.Empty;

        private bool _pkgrulearea = true;
        public bool PKGRULEAREA
        {

            get { return _pkgrulearea; }
            set { SetProperty(ref _pkgrulearea, value); }
        }

        // 팔레트 바코드
        private string _palletBCD = string.Empty;

        public string PLLT_BCD_ID
        {

            get { return _palletBCD; }
            set { SetProperty(ref _palletBCD, value); }

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
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (ASSY_LOTID.Length < 8)
                                    {
                                        // 조립 LOT ID를 8자리 이상 입력해주세요.
                                        error.AppendLine(MessageHelper.MessageValidation("100841", ASSY_LOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsLotIDDuplecate(DataGrid, this))
                                    {
                                        // 입력한 LOT [%1]은 LOT ID가 이미 존재합니다.
                                        error.AppendLine((MessageHelper.MessageValidation("1103", ASSY_LOTID)));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (!IsLotIDExist(this))
                                    {
                                        // 대상 LOT[% 1] 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1026", ASSY_LOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsLotIDDuplecate(this))
                                    {
                                        // LOT [%1]는 HOLD되어 있습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1029", ASSY_LOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    //2023.03.28 김대현
                                    else if (!IsPkgOutLotIDRuleAreaCheck(this))
                                    {
                                        // [%1] %2의 포장 Hold는 활성화 MES를 사용하여 진행해야 합니다.
                                        error.AppendLine(MessageHelper.MessageValidation("SFU9017", new object[] { AreaName, ASSY_LOTID }));
                                        _pkgrulearea = false;
                                    }
                                    break;
                                case "HOLD_REG_QTY":
                                    if (HOLD_REG_QTY < 1)
                                    {
                                        //수량은 0보다 커야 합니다.
                                        error.AppendLine(MessageHelper.MessageValidation("SFU1683"));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }

                                    rowName = "ROWERROR";
                                    if (HOLD_REG_MAX_QTY > 0 && HOLD_REG_QTY > HOLD_REG_MAX_QTY)
                                    {
                                        //C20210729-000334 type 변경 처리
                                        //DataRow[] drList = dtInfo.Select("HOLD_REG_QTY+0 > HOLD_REG_MAX_QTY+0 AND HOLD_REG_MAX_QTY > 0");

                                        // LOT [%1] 의 보류수량이 입력 가능한 최대 수량을 초과 했습니다.
                                        errorRow.AppendLine(MessageHelper.MessageValidation("SFU3759", ASSY_LOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
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
                                        _pkgrulearea = true;    //2023.03.28 김대현
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
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsBoxIDDuplecate(DataGrid, this))
                                    {
                                        //이미 조회된 Pallet ID 입니다.
                                        error.AppendLine((MessageHelper.MessageValidation("SFU3165")));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (dt == null || dt.Rows.Count == 0)
                                    {
                                        // BOX [%1]의 포장 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1067", BOXID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if(dt.Rows[0]["BOXSTAT"].ToString() != "PACKED")
                                    { 
                                        // BOX [%1]은 포장이 완료되지 않았습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1134", BOXID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (!string.IsNullOrEmpty(dt.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString()) &&
                                        dt.Rows[0]["BOX_RCV_ISS_STAT_CODE"].ToString() != "CANCEL_SHIP")
                                    {
                                        // BOX [%1]은 이미 출하 요청되었습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("70031", BOXID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (dt.Rows[0]["ISS_HOLD_FLAG"].ToString() == "Y")
                                    {
                                        // BOX [%1] 는 [%2] 사유로 포장 Hold 상태 입니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1182", BOXID, dt.Rows[0]["PACK_NOTE"]));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsPalletHold(this, out holdNote))
                                    {
                                        // BOX [%1] 는 [%2] 사유로 포장 Hold 상태 입니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1182", BOXID, holdNote));
                                        _pkgrulearea = true;    //2023.03.28 김대현
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
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (STRT_SUBLOTID.Length < 10)
                                    {
                                        // CELL ID[% 1] 자리수를 확인하세요
                                        error.AppendLine(MessageHelper.MessageValidation("100243", STRT_SUBLOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsCellIDDuplecate(DataGrid, this))
                                    {
                                        // Cell ID [%1] 는 중복입니다.
                                        error.AppendLine((MessageHelper.MessageValidation("1128", STRT_SUBLOTID)));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (!LoginInfo.CFG_SHOP_ID.Equals("G182") && !LoginInfo.CFG_AREA_ID.Equals("S5") && !IsLotIDExist(this))
                                    {
                                        // 대상 LOT[% 1] 정보가 존재하지 않습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1026", STRT_SUBLOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    else if (IsLotIDDuplecate(this))
                                    {
                                        // LOT [%1]는 HOLD되어 있습니다.
                                        error.AppendLine(MessageHelper.MessageValidation("1029", STRT_SUBLOTID));
                                        _pkgrulearea = true;    //2023.03.28 김대현
                                    }
                                    //2023.03.28 김대현
                                    else if (!IsPkgOutLotIDRuleAreaCheck(this))
                                    {
                                        // [%1] %2의 포장 Hold는 활성화 MES를 사용하여 진행해야 합니다.
                                        error.AppendLine(MessageHelper.MessageValidation("SFU9017", new object[] { AreaName, STRT_SUBLOTID }));
                                        _pkgrulearea = false;
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
                string bizName = "BR_PRD_CHK_BOXING_FN_IS_LOTID_EXIST";
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

        private DataTable GetPkgOutLotIDRuleArea()
        {
            DataTable result = new DataTable();
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PKG_OUT_LOTID_RULE_AREA";
            dr["CMCODE"] = null;

            inTable.Rows.Add(dr);

            try
            {
                result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

            return result;
        }

        public bool IsPkgOutLotIDRuleAreaCheck(HoldModel item)
        {
            bool result = true;

            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));
            inDataTable.Columns.Add("STRT_SUBLOTID", typeof(string));
            inDataTable.Columns.Add("LANG_ID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["HOLD_TRGT_CODE"] = item.HOLD_TRGT_CODE;
            newRow["ASSY_LOTID"] = item.ASSY_LOTID;
            newRow["STRT_SUBLOTID"] = item.STRT_SUBLOTID;
            newRow["LANG_ID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(newRow);

            DataTable dtData = GetPkgOutLotIDRuleArea();

            if (dtData != null && dtData.Rows.Count > 0)
            {
                try
                {
                    string bizName = "DA_PRD_SEL_BOXING_FN_IS_LOTID_AREA";
                    DataTable dt = new ClientProxy().ExecuteServiceSync(bizName, "RQSTDT", "RSLTDT", inDataTable);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string AreaID = Util.NVC(dt.Rows[0]["AREAID"]);
                        if (!string.IsNullOrEmpty(AreaID))
                        {
                            DataRow[] dr = dtData.Select("CMCODE='" + AreaID + "'");

                            if (dr.Length > 0 && !AreaID.Equals(LoginInfo.CFG_AREA_ID))
                            {
                                AreaName = Util.NVC(dt.Rows[0]["AREANAME"]);
                                result = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
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
