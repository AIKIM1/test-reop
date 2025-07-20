/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2023.05.11  이병윤 : E20230425-000509_Shipping To and label type selection improved
  2024.03.28  오수현 : E20240122-000144 출하처에 따라 Packing List 라벨(A4용지) 인쇄 수량을 MMD에서 관리 함




**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.UserControls;
using System.Linq;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_223 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 


        Util _util = new Util();
        DataTable _dtInboxQueue = new DataTable();
        private static string CREATED = "CREATED,";
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private string _searchStat = string.Empty;
        private bool bInit = true;
        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        #region CheckBox
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre3 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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
        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        CheckBox chkAll3 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion


        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        string _sPGM_ID = "BOX001_223";

        public BOX001_223()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            bInit = false;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            //_combo.SetCombo(cboShipTo, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, null, null }, sCase: "SHIPTO_CP");


            //_combo.SetCombo(cboExpDom_DETL, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            txtBoxQty.Value = 100;
            _dtInboxQueue.Columns.Add("BOXID");
            /* 공용 작업조 컨트롤 초기화 */
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker_Main = ucBoxShift.TextWorker;
            txtShift_Main = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
            ucBoxShift.FrameOperation = this.FrameOperation;

            if (LoginInfo.CFG_AREA_ID == "M7")
            {
                //C20220330-000139 : Hide the check box of N5’s Gogoro shipp’g
                chkGogoro.Visibility = Visibility.Collapsed;
                chkGogoro.IsChecked = false;
            }
            else
            {
                chkGogoro.Visibility = Visibility.Visible;
            }

            cboShipTo.IsEnabled = false;
            btnChangeShipTo.IsEnabled = false;

        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }




        #endregion

        #region Events
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void dgOutbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
        private void dgOutbox2_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre2.Content = chkAll2;
                            e.Column.HeaderPresenter.Content = pre2;
                            chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                            chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
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
        private void dgInPallet_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre3.Content = chkAll3;
                            e.Column.HeaderPresenter.Content = pre3;
                            chkAll3.Checked -= new RoutedEventHandler(checkAll3_Checked);
                            chkAll3.Unchecked -= new RoutedEventHandler(checkAll3_Unchecked);
                            chkAll3.Checked += new RoutedEventHandler(checkAll3_Checked);
                            chkAll3.Unchecked += new RoutedEventHandler(checkAll3_Unchecked);
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

        #region  체크박스 선택 이벤트
        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;


            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgPalletList.SelectedIndex = idx;

                GetDetailInfo();
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        void checkAll2_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgOutbox2.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutbox2.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox2.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgOutbox2.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutbox2.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgOutbox2.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutbox2.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        void checkAll3_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll3.IsChecked)
            {
                for (int i = 0; i < dgInPallet.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgInPallet.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgInPallet.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll3_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll3.IsChecked)
            {
                for (int i = 0; i < dgInPallet.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInPallet.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    _searchStat += CREATED;
                    break;
                case "chkPacking":
                    _searchStat += PACKING;
                    break;
                case "chkPacked":
                    _searchStat += PACKED;
                    break;
                case "chkShipping":
                    _searchStat += SHIPPING;
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            // bInit = false;
        }
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    if (_searchStat.Contains(CREATED))
                        _searchStat = _searchStat.Replace(CREATED, "");
                    break;
                case "chkPacking":
                    if (_searchStat.Contains(PACKING))
                        _searchStat = _searchStat.Replace(PACKING, "");
                    break;
                case "chkPacked":
                    if (_searchStat.Contains(PACKED))
                        _searchStat = _searchStat.Replace(PACKED, "");
                    break;
                case "chkShipping":
                    //_rcvStat += SHIPPING;
                    if (_searchStat.Contains(SHIPPING))
                        _searchStat = _searchStat.Replace(SHIPPING, "");
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            //  bInit = false;
        }
        #endregion

        #region 인팔레트 이벤트
        /// <summary>
        /// [InBox라벨발행]버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        private void popUpFirstPalletTag_Closed(object sender, EventArgs e)
        {
            Report_1st_Boxing popup = sender as Report_1st_Boxing;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                GetPalletList();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// [작업시작]버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>   
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            BOX001_204_RUNSTART popup = new BOX001_204_RUNSTART();
            popup.FrameOperation = this.FrameOperation;
            if (popup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_AREA_ID;//cboArea.SelectedValue;
                Parameters[1] = txtWorker_Main.Tag;
                Parameters[2] = txtShift_Main.Tag;
                // E20230425-000509 : 작업시작 공통으로 사용. 부모창 호출 추가.
                Parameters[3] = _sPGM_ID;

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puRun_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }
        }

        private void puRun_Closed(object sender, EventArgs e)
        {
            BOX001_204_RUNSTART popup = sender as BOX001_204_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                cboArea.SelectedValue = popup.AREAID;
                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", popup.PALLETID, true);
                GetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// [작업취소]버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
            Util.MessageConfirm("SFU1168", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("BOXID");
                        dt.Columns.Add("USERID");
                        DataRow dr = dt.NewRow();
                        dr["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        dr["USERID"] = txtWorker_Main.Tag;
                        dt.Rows.Add(dr);
                        new ClientProxy().ExecuteService("BR_PRD_DEL_2ND_PLT_NJ", "INDATA", null, dt, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            GetPalletList();
                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        /// <summary>
        /// [실적확정]버튼 클릭 이벤트
        /// BIZ : BR_PRD_REG_END_INPALLET_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKING")
                {
                    //SFU2048		확정할 수 없는 상태입니다.	
                    Util.MessageValidation("SFU2048");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                 && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4414	출고중 팔레트는 실적확정 불가합니다. 	
                    Util.MessageValidation("SFU4414");
                    return;
                }

                if (Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["INPUT_QTY"].Index).Value)
                    != Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value))
                {
                    //SFU4417	투입수량과 포장수량이 일치하지 않습니다.	
                    Util.MessageValidation("SFU4417");
                    return;

                }

                // SFU1716 실적확정 하시겠습니까? 
                Util.MessageConfirm("SFU1716", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_2ND_PLT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                //   TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TagPrint()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
            int nPrintQty = GetMmdPackingCondition_PrintQty(idxPallet);  // E20240122-000144

            Report_2nd_Boxing popup = new Report_2nd_Boxing();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletId;
                //Parameters[1] = string.Empty;
                Parameters[2] = nPrintQty; // E20240122-000144
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(confirmPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        #region MMD > 포장/출하 조건(소형) 에서 출하처/제품별 인쇄 수량 조회
        private int GetMmdPackingCondition_PrintQty(int idxPallet)
        {
            int nPrintQty = 1; // Default

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("SHIPTO_ID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                //dt.Columns.Add("PROJECT", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SHOPID"]    = LoginInfo.CFG_SHOP_ID;
                dr["SHIPTO_ID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value);
                dr["PRODID"]    = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);
                dr["USE_FLAG"]  = "Y";
                dr["LANGID"]    = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PACK_COND_NJ", "INDATA", "OUTDATA", dt);

                if (dtResult.Rows.Count != 0)
                {
                    string sPrintQty = dtResult.Rows[0]["PRT_QTY"].ToString(); // 인쇄 수량
                    if (!String.IsNullOrEmpty(sPrintQty))
                    {
                        nPrintQty = Convert.ToInt32(sPrintQty);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return nPrintQty;
        }
        #endregion


        private void GogoroTagPrint()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            Report_Gogoro_Tag popup = new Report_Gogoro_Tag();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private DataSet GetPalletDataSet()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_PALLET_NJ", "INDATA", "OUTPALLET,OUTBOX,OUTLOT", inDataSet);
                return resultDs;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }




        private void confirmPopup_Closed(object sender, EventArgs e)
        {
            Report_2nd_Boxing popup = sender as Report_2nd_Boxing;
            string sPalletId = popup.PALLET_ID;
            int idx = 0;

            GetPalletList();
            for (int i = 0; i < dgPalletList.Rows.Count; i++)
            {
                if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sPalletId))
                {
                    idx = i;
                    break;
                }
            }
            GetPalletList(idx);
            dgPalletList.ScrollIntoView(idx, 0);
        }

        /// <summary>
        /// [조회]버튼 클릭 이벤트
        /// BIZ : BR_PRD_GET_INPALLET_LIST_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (cboArea.SelectedValue.Equals("SELECT"))
            //{
            //    Util.MessageValidation("SFU1499");
            //    return;
            //}
            loadingIndicator.Visibility = Visibility.Visible;
            SetDetailClear();
            GetPalletList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 인박스 이벤트

        /// <summary>
        ///  박스 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxId1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (chkOddPackYN.IsChecked == true)
                {

                    DataRow dr = _dtInboxQueue.NewRow();
                    dr["BOXID"] = txtBoxId1.Text.Trim();
                    _dtInboxQueue.Rows.Add(dr);
                    txtBoxId1.Clear();
                    txtBoxId2.Clear();
                    //chkOddPackYN.IsChecked = false; 20180620 홀수포장 체크 유지
                    txtBoxId1.Focus();
                    RegOutBox();
                }
                else
                {
                    txtBoxId2.Focus();
                }
            }
        }
        private void txtBoxId2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataRow dr = _dtInboxQueue.NewRow();
                dr["BOXID"] = txtBoxId1.Text.Trim();
                _dtInboxQueue.Rows.Add(dr);

                dr = _dtInboxQueue.NewRow();
                dr["BOXID"] = txtBoxId2.Text.Trim();
                _dtInboxQueue.Rows.Add(dr);
                txtBoxId1.Clear();
                txtBoxId2.Clear();
                txtBoxId1.Focus();
                RegOutBox();
            }
        }
        /// <summary>
        /// 셀등록 버튼 클릭시 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        /// <summary>
        /// [삭제]버튼 클릭시 이벤트
        /// BIZ : BR_PRD_DEL_INBOX_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int idxOutbox = 0;
            List<int> idxBoxList = new List<int>();
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                        if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                        {
                            //SFU1843	작업자를 입력 해 주세요.
                            Util.MessageValidation("SFU1843");
                            return;
                        }
                        if (btn.Name.Equals("btnBoxDelete"))
                        {

                            idxOutbox = _util.GetDataGridCheckFirstRowIndex(dgOutbox, "CHK");
                            idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox, "CHK");

                        }
                        else if (btn.Name.Equals("btnBoxDelete3"))
                        {
                            idxOutbox = _util.GetDataGridCheckFirstRowIndex(dgOutbox2, "CHK");
                            idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox2, "CHK");
                        }

                        if (idxOutbox < 0)
                        {
                            //SFU1645 선택된 작업대상이 없습니다.
                            Util.MessageValidation("SFU1645");
                            return;
                        }


                        if (idxBoxList.Count <= 0)
                        {
                            //SFU1645 선택된 작업대상이 없습니다.
                            Util.MessageValidation("SFU1645");
                            return;
                        }

                        if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                        {
                            //SFU1235	이미 확정 되었습니다.
                            Util.MessageValidation("SFU1235");
                            return;
                        }

                        string sPalletId = Util.NVC(dgPalletList.GetCell(dgPalletList.SelectedIndex, dgPalletList.Columns["BOXID"].Index).Value);
                        //int idxPallet = dgPalletList.SelectedIndex;

                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataRow newRow = inPalletTable.NewRow();
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = txtWorker_Main.Tag;

                        inPalletTable.Rows.Add(newRow);
                        if (btn.Name.Equals("btnBoxDelete"))
                        {

                            foreach (int idxBox in idxBoxList)
                            {
                                string sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTER_BOXID"].Index).Value);
                                //var query = (from t in inBoxTable.AsEnumerable()
                                //             where t.Field<string>("BOXID") == sBoxId
                                //             select t.Field<string>("BOXID")).ToList();
                                //if (query.Any())
                                //    continue;
                                newRow = inBoxTable.NewRow();
                                newRow["BOXID"] = sBoxId;
                                inBoxTable.Rows.Add(newRow);
                            }
                        }
                        else
                        {
                            foreach (int idxBox in idxBoxList)
                            {
                                string sBoxId = Util.NVC(dgOutbox2.GetCell(idxBox, dgOutbox2.Columns["OUTER_BOXID"].Index).Value);
                                //var query = (from t in inBoxTable.AsEnumerable()
                                //             where t.Field<string>("BOXID") == sBoxId
                                //             select t.Field<string>("BOXID")).ToList();
                                //if (query.Any())
                                //    continue;
                                newRow = inBoxTable.NewRow();
                                newRow["BOXID"] = sBoxId;
                                inBoxTable.Rows.Add(newRow);
                            }
                        }
                        new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_OUTBOX_NJ", "INPALLET,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                int idx = 0;
                                GetPalletList();
                                for (int i = 0; i < dgPalletList.Rows.Count; i++)
                                {
                                    if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sPalletId))
                                    {
                                        idx = i;
                                        break;
                                    }
                                }
                                GetPalletList(idx);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
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
            });
        }
        #endregion

        #endregion

        #region [Biz]

        /// <summary>
        /// 작업 팔레트 리스트
        /// BIZ : BR_PRD_GET_SHIP_PALLET_LIST_NJ
        /// </summary>
        private void GetPalletList(int idx = -1)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("PKG_LOTID");
                RQSTDT.Columns.Add("BOXSTAT_LIST");
                // RQSTDT.Columns.Add("RCV_ISS_STAT_CODE");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                    dr["BOXID"] = txtPalletID.Text;
                else
                {
                    dr["PKG_LOTID"] = null;//string.IsNullOrWhiteSpace(txtAssyLotID.Text) ? null : txtAssyLotID.Text;
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                    // dr["RCV_ISS_STAT_CODE"] = string.IsNullOrEmpty(_rcvStat) ? null : string.Empty;
                }

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgPalletList, RSLTDT, FrameOperation, true);
                if (idx != -1)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[idx].DataItem, "CHK", true);
                    dgPalletList.SelectedIndex = idx;
                    dgPalletList.ScrollIntoView(idx, 0);

                }
                else
                {
                    dgPalletList.SelectedIndex = -1;
                }

                GetDetailInfo();

                if (dgPalletList.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        #endregion

        #region [Method]
        /// <summary>
        /// 상세 
        /// </summary>
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            //if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            //{
                            //    txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            //}
                            //else
                            //{
                            //    txtShiftStartTime.Text = string.Empty;
                            //}

                            //if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            //{
                            //    txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            //}
                            //else
                            //{
                            //    txtShiftEndTime.Text = string.Empty;
                            //}

                            //if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            //{
                            //    txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            //}
                            //else
                            //{
                            //    txtShiftDateTime.Text = string.Empty;
                            //}

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker_Main.Text = string.Empty;
                                txtWorker_Main.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker_Main.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker_Main.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift_Main.Tag = string.Empty;
                                txtShift_Main.Text = string.Empty;
                            }
                            else
                            {
                                txtShift_Main.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift_Main.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker_Main.Text = string.Empty;
                            txtWorker_Main.Tag = string.Empty;
                            txtShift_Main.Text = string.Empty;
                            txtShift_Main.Tag = string.Empty;
                            //txtShiftStartTime.Text = string.Empty;
                            //txtShiftEndTime.Text = string.Empty;
                            //txtShiftDateTime.Text = string.Empty;
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
                });
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
        private void setShipToCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }

        #endregion
        private DataSet getPalletDataSet(string sinPallet)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = sinPallet; //Util.NVC(dgInPallet.GetCell(dgInPallet.SelectedIndex, dgInPallet.Columns["BOXID"].Index).Value);
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_INPALLET_NJ", "INDATA", "OUTDATA,OUTBOX", inDataSet);
                return resultDs;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }

        private void btnGogoroPrint_Click(object sender, RoutedEventArgs e)
        {
            GogoroTagPrint();
        }
        private void PrintinPalletTag(string sPalletID)
        {
            Report_1st_Boxing popup = new Report_1st_Boxing();
            popup.FrameOperation = this.FrameOperation;
            popup.bRemainder = true;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletID;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(popUpFirstPalletTag_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void txtInPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                //SFU1987	투입처리 하시겠습니까?        
                Util.MessageConfirm("SFU1987", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        string sProdId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("LANGID");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("SHFT_ID");

                        DataTable inCtnrTable = indataSet.Tables.Add("INPALLET");
                        inCtnrTable.Columns.Add("BOXID");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["SHFT_ID"] = txtShift_Main.Tag;
                        inDataTable.Rows.Add(newRow);

                        newRow = inCtnrTable.NewRow();
                        newRow["BOXID"] = txtInPalletID.Text;
                        inCtnrTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_INS_INPALLET_FOR_2ND_PLT_NJ", "INDATA,INPALLET", "OUTINPALLET", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //SFU1973	투입완료되었습니다.
                                Util.MessageInfo("SFU1973");

                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                    else
                        loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void btnChangeShipTo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }
                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("BOXID", typeof(string));
                IndataTable.Columns.Add("SHIPTO_ID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["BOXID"] = sPalletId;
                Indata["SHIPTO_ID"] = cboShipTo.SelectedValue;
                Indata["USERID"] = txtWorker_Main.Tag;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("BR_PRD_UPD_SHIP_PALLET_NJ", "INDATA", null, IndataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        int idx = 0;
                        GetPalletList();
                        for (int i = 0; i < dgPalletList.Rows.Count; i++)
                        {
                            if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sPalletId))
                            {
                                idx = i;
                                break;
                            }
                        }
                        GetPalletList(idx);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                List<int> idxBoxList = new List<int>();
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;

                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }
                if (btn.Name.Equals("btnRePrint"))
                    idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox, "CHK");
                else if (btn.Name.Equals("btnRePrint2"))
                    idxBoxList = _util.GetDataGridCheckRowIndex(dgOutbox2, "CHK");


                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                Util.MessageConfirm("SFU2059", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                        DataSet ds = new DataSet();
                        DataTable dtIndata = ds.Tables.Add("INDATA");
                        dtIndata.Columns.Add("LANGID");
                        dtIndata.Columns.Add("USERID");
                        dtIndata.Columns.Add("PGM_ID");    //라벨 이력 저장용
                        dtIndata.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                        DataRow dr = null;
                        dr = dtIndata.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["USERID"] = txtWorker_Main.Tag;
                        dr["PGM_ID"] = _sPGM_ID;
                        dr["BZRULE_ID"] = sBizRule;
                        dtIndata.Rows.Add(dr);

                        DataTable dtInbox = ds.Tables.Add("INBOX");
                        dtInbox.Columns.Add("BOXID");

                        if (btn.Name.Equals("btnRePrint"))
                        {
                            foreach (int idxBox in idxBoxList)
                            {
                                dr = dtInbox.NewRow();
                                string sBoxId = Util.NVC(dgOutbox.GetCell(idxBox, dgOutbox.Columns["OUTER_BOXID"].Index).Value);
                                dr["BOXID"] = sBoxId;
                                dtInbox.Rows.Add(dr);
                            }
                        }
                        else
                        {
                            foreach (int idxBox in idxBoxList)
                            {
                                dr = dtInbox.NewRow();
                                string sBoxId = Util.NVC(dgOutbox2.GetCell(idxBox, dgOutbox2.Columns["OUTER_BOXID"].Index).Value);
                                dr["BOXID"] = sBoxId;
                                dtInbox.Rows.Add(dr);
                            }
                        }
                        DataTable dtInPrint = ds.Tables.Add("INPRINT");
                        dtInPrint.Columns.Add("PRMK");
                        dtInPrint.Columns.Add("RESO");
                        dtInPrint.Columns.Add("PRCN");
                        dtInPrint.Columns.Add("MARH");
                        dtInPrint.Columns.Add("MARV");
                        dtInPrint.Columns.Add("DARK");
                        dr = dtInPrint.NewRow();
                        dr["PRMK"] = _sPrt;
                        dr["RESO"] = _sRes;
                        dr["PRCN"] = _sCopy;
                        dr["MARH"] = _sXpos;
                        dr["MARV"] = _sYpos;
                        dr["DARK"] = _sDark;
                        dtInPrint.Rows.Add(dr);

                        //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);

                        if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            DataTable dtResult = dsResult.Tables["OUTDATA"];
                            string zplCode = string.Empty;
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                            }
                            PrintLabel(zplCode, _drPrtInfo);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (e.Key == Key.Enter)
            {
                //if (_dtInboxQueue.Rows.Count < 6)
                //{
                //    DataRow dr = _dtInboxQueue.NewRow();
                //    dr["BOXID"] = txtPalletId.Text.Trim();
                //    _dtInboxQueue.Rows.Add(dr);
                //    if (!_dtInboxQueue.Columns.Contains("CHK"))
                //    {
                //        _dtInboxQueue.Columns.Add("CHK");
                //    }
                //    Util.GridSetData(dgInboxQueue, _dtInboxQueue, FrameOperation);
                //    txtPalletId.Clear();
                //}
                //if (_dtInboxQueue.Rows.Count == 6)
                //{

                DataRow dr = _dtInboxQueue.NewRow();

                dr["BOXID"] = txtPalletId.Text.Trim();
                _dtInboxQueue.Rows.Add(dr);

                Util.GridSetData(dgInboxQueue, _dtInboxQueue, FrameOperation);

                RegMCSOutBox();
                txtPalletId.Clear();
                txtPalletId.Focus();

               // }
            }
        }

        private bool ValidationRegOutBox()
        {
            int row = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (row < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }

        private void RegMCSOutBox()
        {
            try
            {
                if (!ValidationRegOutBox())
                {
                    _dtInboxQueue.Clear();
                    return;
                }

                string zplCode = string.Empty;
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;

                int row = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                string sBoxId = Util.NVC(dgPalletList.GetCell(row, dgPalletList.Columns["BOXID"].Index).Value); // 2차 팔레트id

                string sBizRule = "BR_PRD_REG_OUTBOX_BY_1ST_PLT_NJ";

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("SHFT_ID");
                inPalletTable.Columns.Add("USERID");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("SRCTYPE");
                inPalletTable.Columns.Add("EQPTID");
                //inPalletTable.Columns.Add("INBOX_QTY");
                inPalletTable.Columns.Add("PACKTYPE");
                inPalletTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inPalletTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable FSTPLTTable = indataSet.Tables.Add("FST_PLT");
                FSTPLTTable.Columns.Add("FST_PLT_ID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inPalletTable.NewRow();
                newRow["BOXID"] = sBoxId;
                newRow["SHFT_ID"] = txtShift_Main.Tag;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = ""; // 미정
                //newRow["INBOX_QTY"] = _dtInboxQueue.Rows.Count;
                newRow["PACKTYPE"] = "MCC";  // 원각형 2차 포장
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;
                inPalletTable.Rows.Add(newRow);

           
                newRow = FSTPLTTable.NewRow();
                newRow["FST_PLT_ID"] = txtPalletId.Text.Trim();
                FSTPLTTable.Rows.Add(newRow);

                _dtInboxQueue.Clear();
                //dgInboxQueue.ItemsSource = null;

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                inPrintTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_BY_1ST_PLT_NJ", "INPALLET,FST_PLT,INPRINT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi(sBizRule, "INPALLET,FST_PLT,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {

                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            _dtInboxQueue.Clear();
                            dgInboxQueue.ItemsSource = DataTableConverter.Convert(_dtInboxQueue);
                            return;
                        }
                        _dtInboxQueue.Clear();
                        dgInboxQueue.ItemsSource = DataTableConverter.Convert(_dtInboxQueue);

                        for (int j = 0; j < bizResult.Tables["OUTDATA"].Rows.Count; j++)
                        {
                            zplCode = bizResult.Tables["OUTDATA"].Rows[j]["ZPLCODE"].GetString();
                            if (zplCode.Split(',')[0].Equals("1"))
                            {
                                ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                //throw new Exception(zplCode.Split(',')[1]);
                                return;
                            }
                            else
                            {
                                zplCode = zplCode.Substring(2);
                                if (chkPrintYN.IsChecked == true)
                                    PrintLabel(zplCode, _drPrtInfo);
                            }
                        }
                        //GetPalletDetailInfo();
                        //txtInBoxId.Focus();
                        GetPalletList();
                        int idxPallet = 0;
                        for (int i = 0; i < dgPalletList.Rows.Count; i++)
                        {
                            if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sBoxId))
                            {
                                idxPallet = i;
                            }
                        }

                        GetPalletList(idxPallet);

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void RegOutBox()
        {
            try
            {
                if (!ValidationRegOutBox())
                {
                    _dtInboxQueue.Clear();
                    return;
                }

                string zplCode = string.Empty;
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    return;

                int row = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                string sBoxId = Util.NVC(dgPalletList.GetCell(row, dgPalletList.Columns["BOXID"].Index).Value); // 2차 팔레트id

                string sBizRule = "BR_PRD_REG_OUTBOX_NJ";

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("SHFT_ID");
                inPalletTable.Columns.Add("USERID");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("SRCTYPE");
                inPalletTable.Columns.Add("EQPTID");
                inPalletTable.Columns.Add("INBOX_QTY");
                inPalletTable.Columns.Add("PACKTYPE");
                inPalletTable.Columns.Add("SOC");
                inPalletTable.Columns.Add("GOGORO_FLAG");
                inPalletTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inPalletTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inPalletTable.NewRow();
                newRow["BOXID"] = sBoxId;
                newRow["SHFT_ID"] = txtShift_Main.Tag;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SRCTYPE"] = "UI";
                newRow["EQPTID"] = ""; // 미정
                newRow["INBOX_QTY"] = _dtInboxQueue.Rows.Count;
                newRow["PACKTYPE"] = "MCC";  // 원각형 2차 포장
                newRow["SOC"] = txtSOC.Value <= 0 ? null : txtSOC.Value.ToString();
                newRow["GOGORO_FLAG"] = (bool)chkGogoro.IsChecked ? "Y" : "N";
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inPalletTable.Rows.Add(newRow);

                for (int i = 0; i < _dtInboxQueue.Rows.Count; i++)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = _dtInboxQueue.Rows[i]["BOXID"].ToString();
                    inBoxTable.Rows.Add(newRow);
                }
                _dtInboxQueue.Clear();
                dgInboxQueue.ItemsSource = null;
                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                inPrintTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_NJ", "INPALLET,INBOX,INPRINT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi(sBizRule, "INPALLET,INBOX,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {

                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            _dtInboxQueue.Clear();
                            dgInboxQueue.ItemsSource = DataTableConverter.Convert(_dtInboxQueue);
                            return;
                        }
                        _dtInboxQueue.Clear();
                        dgInboxQueue.ItemsSource = DataTableConverter.Convert(_dtInboxQueue);

                        for (int j = 0; j < bizResult.Tables["OUTDATA"].Rows.Count; j++)
                        {
                            zplCode = bizResult.Tables["OUTDATA"].Rows[j]["ZPLCODE"].GetString();
                            if (zplCode.Split(',')[0].Equals("1"))
                            {
                                ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                //throw new Exception(zplCode.Split(',')[1]);
                                return;
                            }
                            else
                            {
                                zplCode = zplCode.Substring(2);
                                if (chkPrintYN.IsChecked == true)
                                    PrintLabel(zplCode, _drPrtInfo);
                            }
                        }
                        //GetPalletDetailInfo();
                        txtPalletId.Focus();
                        GetPalletList();
                        int idxPallet = 0;
                        for (int i = 0; i < dgPalletList.Rows.Count; i++)
                        {
                            if (Util.NVC(dgPalletList.GetCell(i, dgPalletList.Columns["BOXID"].Index).Value).Equals(sBoxId))
                            {
                                idxPallet = i;
                            }
                        }

                        GetPalletList(idxPallet);

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void btnRegInbox_Click(object sender, RoutedEventArgs e)
        {
            if (_dtInboxQueue.Rows.Count > 0)
            {
                RegMCSOutBox();
                txtPalletId.Clear();
                txtPalletId.Focus();
            }
        }

        private void btnBoxDelete2_Click(object sender, RoutedEventArgs e)
        {
            List<string> ids = new List<string>();
            List<DataRow> delRows = new List<DataRow>();
            for (int i = 0; i < dgInboxQueue.Rows.Count; i++)
            {
                if (Util.NVC(dgInboxQueue.GetCell(i, dgInboxQueue.Columns["CHK"].Index).Value).SafeToBoolean() == true)
                    ids.Add(Util.NVC(dgInboxQueue.GetCell(i, dgInboxQueue.Columns["BOXID"].Index).Value));
            }

            foreach (string id in ids)
            {
                foreach (DataRow dr in _dtInboxQueue.Rows)
                {
                    if (dr["BOXID"].Equals(id))
                        delRows.Add(dr);
                }
                foreach (DataRow dr in delRows)
                {
                    dr.Delete();
                }
            }
            dgInboxQueue.ItemsSource = DataTableConverter.Convert(_dtInboxQueue);
            Util.GridSetData(dgInboxQueue, _dtInboxQueue, FrameOperation);

            dgInboxQueue.Refresh(false, false);
        }

        private void chkOddPackYN_Checked(object sender, RoutedEventArgs e)
        {
            txtBoxId2.IsEnabled = false;
        }

        private void chkOddPackYN_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoxId2.IsEnabled = true;
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPalletList();
            }
        }

        private void dgPalletList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblCreated.Tag))
                    {
                        e.Cell.Presenter.Background = lblCreated.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacking.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacking.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacked.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacked.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void chkPrintYN2_Checked(object sender, RoutedEventArgs e)
        {
            chkPrintYN.IsChecked = true;
        }

        private void chkPrintYN2_Unchecked(object sender, RoutedEventArgs e)
        {
            chkPrintYN.IsChecked = false;
        }

        private void chkPrintYN_Checked(object sender, RoutedEventArgs e)
        {
            if (chkPrintYN2 != null)
                chkPrintYN2.IsChecked = true;
        }

        private void chkPrintYN_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkPrintYN2 != null)
                chkPrintYN2.IsChecked = false;
        }

        private void lblCreated_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCreated.IsChecked == true)
                chkCreated.IsChecked = false;
            else
                chkCreated.IsChecked = true;
        }
        private void lblPacking_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacking.IsChecked == true)
                chkPacking.IsChecked = false;
            else
                chkPacking.IsChecked = true;
        }
        private void lblPacked_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacked.IsChecked == true)
                chkPacked.IsChecked = false;
            else
                chkPacked.IsChecked = true;
        }

        private void datePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!bInit)
                btnSearch_Click(null, null);
        }

        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4413		포장 완료된 팔레트만 출고 가능합니다.	
                    Util.MessageValidation("SFU4413");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                    && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4416	이미 출고된 팔레트 입니다.
                    Util.MessageValidation("SFU4416");
                    return;
                }

                //SFU2802	포장출고를 하시겠습니까?
                Util.MessageConfirm("SFU2802", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancelShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4417   실적 확정된 팔레트만 출고 취소 가능합니다.
                    Util.MessageValidation("SFU4417");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) != "SHIPPING")
                {
                    //SFU3717		출고중 상태인 팔레트만 출고취소 가능합니다.	
                    Util.MessageValidation("SFU3717");
                    return;
                }

                //	SFU2805		포장출고를 취소하시겠습니까?	
                Util.MessageConfirm("SFU2805", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                Util.MessageInfo("SFU3431");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4262		실적 확정후 작업 가능합니다.	
                    Util.MessageValidation("SFU4262");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4415	출고중 팔레트는 실적취소 불가합니다. 	
                    Util.MessageValidation("SFU4415");
                    return;
                }

                //		SFU4263	실적 취소 하시겠습니까?	
                Util.MessageConfirm("SFU4263", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_END_2ND_PLT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void lblShipping_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkShipping.IsChecked == true)
                chkShipping.IsChecked = false;
            else
                chkShipping.IsChecked = true;
        }

        private void GetDetailInfo()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                    return;

                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                string sProdID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);

                setShipToCombo(cboShipTo, CommonCombo.ComboStatus.SELECT, sProdID);
                cboShipTo.SelectedValue = string.IsNullOrEmpty(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value)) ? "SELECT" : Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value);

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("BOXID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = sPalletId;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_2ND_PALLET_DETAIL_NJ", "INDATA", "OUTINBOX,OUTINPALLET,OUTSUMMARY,OUTINPUTBOX,OUTBOX", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTBOX"] != null)
                        {
                            DataTable dtInfo = bizResult.Tables["OUTBOX"];
                            if (!dtInfo.Columns.Contains("CHK"))
                                dtInfo.Columns.Add("CHK");
                            //Util.GridSetData(dgOutbox, dtInfo, FrameOperation, true);

                            //_dtInboxQueue.Clear();

                            string[] sColumnName = new string[] { "CHK", "OUTER_BOXID3", "BOXSEQ", "OUTER_BOXID", "TOTAL_QTY" };
                            if (sProdID.Substring(0, 3).Equals("MCS") || sProdID.Substring(0, 3).Equals("MFS") || sProdID.Substring(0, 3).Equals("MAS"))
                            {
                                TabOutboxReg1.Visibility = Visibility.Collapsed;
                                TabOutboxReg2.Visibility = Visibility.Visible;
                                Util.GridSetData(dgOutbox2, dtInfo, FrameOperation);
                                Util.GridSetData(dgInboxQueue, _dtInboxQueue, FrameOperation);
                                if (dgOutbox2.Rows.Count > 0)
                                {
                                    DataGridAggregate.SetAggregateFunctions(dgOutbox2.Columns["TOTL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                }
                                _util.SetDataGridMergeExtensionCol(dgOutbox2, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            }
                            else
                            {
                                TabOutboxReg2.Visibility = Visibility.Collapsed;
                                TabOutboxReg1.Visibility = Visibility.Visible;
                                Util.GridSetData(dgOutbox, dtInfo, FrameOperation);
                                if (dgOutbox.Rows.Count > 0)
                                {
                                    DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["TOTL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                }
                                _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            }

                            //if (dgOutbox.Rows.Count > 0)
                            //{
                            //    DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            //    //DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["OUTER_BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                            //}

                        }
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTINPALLET"] != null)
                        {
                            DataTable dtInfo = bizResult.Tables["OUTINPALLET"];
                            if (!dtInfo.Columns.Contains("CHK"))
                                dtInfo.Columns.Add("CHK");

                            Util.GridSetData(dgInPallet, dtInfo, FrameOperation);

                            if (dgInPallet.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["TOTAL_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["REST_TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInPallet.Columns["REST_BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
                        }
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTSUMMARY"] != null)
                        {
                            Util.GridSetData(dgSummary, bizResult.Tables["OUTSUMMARY"], FrameOperation);

                            if (dgSummary.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PKG_LOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                                DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["LOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
                        }

                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTINPUTBOX"] != null)
                        {
                            Util.GridSetData(dgInputBox, bizResult.Tables["OUTINPUTBOX"], FrameOperation);

                            if (dgInputBox.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInputBox.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                                DataGridAggregate.SetAggregateFunctions(dgInputBox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInPallet, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }

                //SFU1862	잔량처리 하시겠습니까?
                Util.MessageConfirm("SFU1862", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("LANGID");
                        inDataTable.Columns.Add("BOXID");
                        inDataTable.Columns.Add("USERID");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        inDataTable.Rows.Add(newRow);

                        DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                        inPalletTable.Columns.Add("BOXID");

                        foreach (int idxBox in idxBoxList)
                        {
                            string sBoxId = Util.NVC(dgInPallet.GetCell(idxBox, dgInPallet.Columns["BOXID"].Index).Value);

                            newRow = inPalletTable.NewRow();
                            newRow["BOXID"] = sBoxId;
                            inPalletTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REM_INPALLET_FOR_2ND_PLT_NJ", "INDATA,INPALLET", "OUTINPALLET", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTINPALLET"] != null)
                                {
                                    GetPalletList();

                                    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                    GetDetailInfo();
                                }
                                //SFU1275	정상처리되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        private void SetDetailClear()
        {
            txtInPalletID.Text = string.Empty;
            Util.gridClear(dgInPallet);
            Util.gridClear(dgOutbox);
            Util.gridClear(dgOutbox2);
            Util.gridClear(dgSummary);
            Util.gridClear(dgInputBox);
        }
    }

}
