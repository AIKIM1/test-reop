/*************************************************************************************
 Created Date : 
      Creator : 이홍주
   Decription : PALLET, OUTBOX단위 포장 일괄해체
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.15  DEVELOPER : Initial Created.
  2023.11.16  이홍주    : SI               NFF 증설
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_356 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        
        Util _util = new Util();
        //private static string PACKING = "PACKING,";
        //private static string PACKED = "PACKED,";
        //private static string SHIPPING = "SHIPPING,";
        //private string _searchStat = string.Empty;
        private bool bInit = true;


        //테이블 저장용
        private DataTable dsGet = new DataTable();
        //테이블 저장용 2
        private DataSet dsGet2 = new DataSet();


        #region CheckBox
        C1.WPF.DataGrid.DataGridRowHeaderPresenter prePallet = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAllPallet = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preBox = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllBox = new CheckBox()
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

        public FCS002_356()
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
            _combo.SetCombo(cboLinePallet, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCM,MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
            _combo.SetCombo(cboLinePallet2, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCM,MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");
            _combo.SetCombo(cboLineBox, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCM,MCC,MCR,MCS", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Today;
            dtpDateTo.SelectedDateTime = DateTime.Today;

            initInputPalletId();
        }

        private void initInputPalletId()
        {
            txtPalletID.Text = "";
            txtPalletID.Focus();
            txtPalletID.SelectAll();
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        /// <summary>
        /// 하위 컨트롤 초기화
        /// </summary>
        private void SetPalletDetailClear()
        {
            //txtOutBox.Text = string.Empty;
            Util.gridClear(dgPallet);
            Util.gridClear(dgPalletBox);

        }

        private void SetBoxDetailClear()
        {
            //txtOutBox.Text = string.Empty;
            Util.gridClear(dgOutBox);
            Util.gridClear(dgInBox);

        }

        #endregion

        #region Events

        #region 텍스트 박스 포커스 : text_GotFocus()
        /// <summary>
        /// 텍스트 박스 포커스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Pallet CheckAll : dgPallet_LoadedColumnHeaderPresenter
        /// <summary>
        ///  PALLET CheckAll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void  dgPallet_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            prePallet.Content = chkAllPallet;
                            if (e.Column.HeaderPresenter != null)
                            {
                                e.Column.HeaderPresenter.Content = preBox;
                            }
                            chkAllPallet.Checked -= new RoutedEventHandler(checkAllPallet_Checked);
                            chkAllPallet.Unchecked -= new RoutedEventHandler(checkAllPallet_UnChecked);
                            chkAllPallet.Checked += new RoutedEventHandler(checkAllPallet_Checked);
                            chkAllPallet.Unchecked += new RoutedEventHandler(checkAllPallet_UnChecked);
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


        #endregion

        #region OUTBOX CheckAll : dgOutBox_LoadedColumnHeaderPresenter 
        /// <summary>
        ///  OUTBOX CheckAll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOutBox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            preBox.Content = chkAllBox;
                            if (e.Column.HeaderPresenter != null)
                            {
                                e.Column.HeaderPresenter.Content = preBox;
                            }
                            chkAllBox.Checked -= new RoutedEventHandler(checkAllBox_Checked);
                            chkAllBox.Unchecked -= new RoutedEventHandler(checkAllBox_UnChecked);
                            chkAllBox.Checked += new RoutedEventHandler(checkAllBox_Checked);
                            chkAllBox.Unchecked += new RoutedEventHandler(checkAllBox_UnChecked);
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


        #endregion

        #region  체크박스 선택 이벤트 : checkAllPallet_Checked(), checkAllPallet_UnChecked()

        /// <summary>
        /// PALLET 전체 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkAllPallet_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAllPallet.IsChecked)
            {
                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgPallet.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgPallet.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        /// <summary>
        /// 전체 선택 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkAllPallet_UnChecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllPallet.IsChecked)
            {
                for (int i = 0; i < dgPallet.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPallet.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        #endregion

        #region  체크박스 선택 이벤트 : checkAllBox_Checked(), checkAllBox_UnChecked()

        /// <summary>
        /// OUTBOX 전체 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void checkAllBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAllBox.IsChecked)
            {
                for (int i = 0; i < dgOutBox.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutBox.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgOutBox.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgOutBox.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutBox.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        // <summary>
        // 전체 선택 해제
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void checkAllBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllBox.IsChecked)
            {
                for (int i = 0; i < dgOutBox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutBox.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        #endregion

        #region 조회 : btnSearchPallet_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchPallet_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            SetPalletDetailClear();
            GetPalletList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 포장 해체 이력 조회 : btnSearch_Hist_Click
        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            Search_Hist();
        }
        #endregion

        #region 팔레트/박스 ID 엑셀 붙여넣기 : txtPalletID_PreviewKeyDown

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
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && (CanInputBoxid(sPasteStrings[i].Trim())) == false)
                        {
                            break;
                        }
                        Search(sPasteStrings[i].Trim());
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }
        #endregion

        #region Outbox 조회(사용안함) : btnSearchOutBox_Click
        private void btnSearchOutBox_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            SetBoxDetailClear();
            GetOutBoxList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 콤보박스 이벤트 : cboLinePallet_SelectedValueChanged()
        /// <summary>
        /// 라인 콤보박스로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboLinePallet_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetPalletDetailClear();
           
                InitControl();
                // 라인 선택 시 자동 조회 처리  2023.11.15 필요성이 없어서 막음. NFF증설 이홍주
                //if (cboLine != null && (cboLine.SelectedIndex > 0 && cboLine.Items.Count > cboLine.SelectedIndex))
                //{
                //    if (cboLine.SelectedValue.GetString() != "SELECT")
                //    {
                //        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                //    }
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 콤보박스 이벤트 : cboLinePallet_SelectedValueChanged()
        /// <summary>
        /// 라인 콤보박스로 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboLineBox_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetBoxDetailClear();
                InitControl();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 포장중, 포장완료, 출고요청 이벤트 : chkSearch_Checked(), chkSearch_Unchecked()  사용안함.
        /// <summary>
        /// 체크박스 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void chkSearch_Checked(object sender, RoutedEventArgs e)
        //{
        //    CheckBox chk = sender as CheckBox;
        //    switch (chk.Name)
        //    {
        //        case "chkPacking":
        //            _searchStat += PACKING;
        //            break;
        //        case "chkPacked":
        //            _searchStat += PACKED;
        //            break;
        //        case "chkShipping":
        //            _searchStat += SHIPPING;
        //            break;
        //        default:
        //            break;
        //    }
        //    if (!bInit)
        //        btnSearch_Click(null, null);
        //    // bInit = false;
        //}
        /// <summary>
        /// 체크박스 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            //CheckBox chk = sender as CheckBox;
            //switch (chk.Name)
            //{
            //    case "chkPacking":
            //        if (_searchStat.Contains(PACKING))
            //            _searchStat = _searchStat.Replace(PACKING, "");
            //        break;
            //    case "chkPacked":
            //        if (_searchStat.Contains(PACKED))
            //            _searchStat = _searchStat.Replace(PACKED, "");
            //        break;
            //    case "chkShipping":
            //        //_rcvStat += SHIPPING;
            //        if (_searchStat.Contains(SHIPPING))
            //            _searchStat = _searchStat.Replace(SHIPPING, "");
            //        break;
            //    default:
            //        break;
            //}
            //if (!bInit)
            //    btnSearch_Click(null, null);
            //  bInit = false;
        }



        #endregion

        #region 작업 Pallet 스프레드 이벤트
        /// <summary>
        /// Pallet ID 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void dgPalletChoice_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    RadioButton rb = sender as RadioButton;

        //    if (rb.DataContext == null)
        //        return;


        //    if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
        //    {
        //        int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
        //        DataRow dtRow = (rb.DataContext as DataRowView).Row;

        //        for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
        //        {
        //            if (idx == i)   // Mode = OneWay 이므로 Set 처리.
        //                DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
        //            else
        //                DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
        //        }

        //        //row 색 바꾸기
        //        dgPallet.SelectedIndex = idx;
        //        SetDetailClear();
        //        GetCompleteOutbox();
        //    }
        //}
        #endregion
    
        #region OutboxID KeyDown
        private void txtOutbox_KeyDown(object sender, KeyEventArgs e)
        {

            btnSearchOutBox_Click(null, null);
            //if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtOutbox.Text))
            //{
            //    try
            //    {
            //        DataTable inTable = new DataTable();
            //        inTable.Columns.Add("EQSGID", typeof(string));
            //        inTable.Columns.Add("PALLETID", typeof(string));
            //        inTable.Columns.Add("BOXID", typeof(string));
            //        inTable.Columns.Add("LANGID", typeof(string));

            //        DataRow newRow = inTable.NewRow();

            //        newRow["PALLETID"] = txtPalletID.Text.ToString().Trim();
            //        newRow["LANGID"] = LoginInfo.LANGID;

            //        inTable.Rows.Add(newRow);

            //        new ClientProxy().ExecuteService("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "RQSTDT", "RSLTDT", inTable, (result, ex) =>
            //        {
            //            if (ex != null)
            //            {
            //                Util.MessageException(ex);
            //                return;
            //            }
            //            if (!result.Columns.Contains("CHK"))
            //                result = _util.gridCheckColumnAdd(result, "CHK");
            //            ////_dtSource = result;
            //            ////Util.GridSetData(dgSource, _dtSource, FrameOperation);
            //            ////_dtTarget.Clear();
            //            ////dgTarget.ItemsSource = null;
            //            ////if (dgSource.Rows.Count > 0)
            //            ////{
            //            ////    DataGridAggregate.SetAggregateFunctions(dgSource.Columns["OUTBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            //            ////}
            //            txtOutbox.Clear();
            //            txtOutbox.Focus();
            //        });
            //    }
            //    catch (Exception ex)
            //    {

            //        Util.MessageException(ex);
            //    }
            //}
        }
        #endregion

        #region PalletId KeyDown
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.Key == Key.Enter)
            {
                Search(txtPalletID.Text.Trim());
            }
        }
        #endregion

        #region PALLET 포장해체 btnUnPackOutBox_Click 
        private void btnUnPackOutBox_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 포장 해체 대상 초기화 : btnClear_Click
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU3440", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear();
                }
            });
        }
        #endregion

        #region OUTBOX 포장해체 btnUnPackPallet_Click 
        private void btnUnPackPallet_Click(object sender, RoutedEventArgs e)
        {
            //	SFU4491		포장 해체하시겠습니까?
            Util.MessageConfirm("SFU4491", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (dgPalletBox.GetRowCount() <= 0)
                    {
                        //SFU1645	선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgPallet.ItemsSource);

                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE");
                    inDataTable.Columns.Add("LANGID");
                    inDataTable.Columns.Add("USERID");                    
                    inDataTable.Columns.Add("INBOX_DEL_FLAG");

                    DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                    inBoxTable.Columns.Add("PLTID");
                    inBoxTable.Columns.Add("OUTBOXID");
                    inBoxTable.Columns.Add("INBOXID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = "UI";
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["INBOX_DEL_FLAG"] = (bool)chkUnpackInBox.IsChecked ? "Y" : "N";
                    inDataTable.Rows.Add(newRow);


                    DataTable dtPalletBox = DataTableConverter.Convert(dgPalletBox.ItemsSource);


                    foreach (DataRow dr in dtPalletBox.Rows)
                    {
                        newRow = inBoxTable.NewRow();
                        newRow["PLTID"] = dr["PLTID"];
                        newRow["OUTBOXID"] = dr["OUTBOXID"];
                        newRow["INBOXID"] = dr["INBOXID"];
                        inBoxTable.Rows.Add(newRow);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_BOX_MB", "INDATA,INBOX", "", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            
                            Clear();
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
        #endregion

        #region dgPallet_MouseUp
        private void dgPallet_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (dgPallet.CurrentRow == null || dgPallet.SelectedIndex == -1)
            //{
            //    return;
            //}
            //else if (e.ChangedButton.ToString().Equals("Left") && dgPallet.CurrentColumn.Name == "CHK")
            //{
            //    string sPalletid = string.Empty;
            //    try
            //    {
            //        // Rows Count가 0보다 클 경우에만 이벤트 발생하도록
            //        if (dgPallet.GetRowCount() > 0)
            //        {
            //            sPalletid = Util.NVC(dgPallet.GetCell(dgPallet.CurrentRow.Index, dgPallet.Columns["PALLETID"].Index).Value);

            //            if (Util.NVC(dgPallet.GetCell(dgPallet.CurrentRow.Index, dgPallet.Columns["CHK"].Index).Value) == "1")
            //            {
            //                DataTableConverter.SetValue(dgPallet.Rows[dgPallet.CurrentRow.Index].DataItem, "CHK", false);
            //            }
            //            else
            //            {
            //                DataTableConverter.SetValue(dgPallet.Rows[dgPallet.CurrentRow.Index].DataItem, "CHK", true);
            //            }


            //            if (Util.NVC(dgPallet.GetCell(dgPallet.CurrentRow.Index, dgPallet.Columns["CHK"].Index).Value) == "1")
            //            {
            //                int trayRows = 0;
            //                trayRows = dgPalletBox.GetRowCount();

            //                // 외뢰한 시간을 체크하여 5분전에 의뢰한 Pallet ID는 재의뢰할 수 없도록 함.
            //                // 서용수님 요청사항.
            //                //if (Check2QAREQ(sPalletid) == "NG")
            //                //{
            //                //    Util.MessageValidation("SFU1301"); //"5분 전에 의뢰된 PALLET ID 입니다."
            //                //    DataTableConverter.SetValue(dgPallet.Rows[dgPallet.CurrentRow.Index].DataItem, "CHK", false);
            //                //    return;
            //                //}

            //                //전송 정보 중복 여부 체크
            //                if (trayRows != 0)
            //                {
            //                    for (int i = 0; i < trayRows; i++)
            //                    {
            //                        if (Util.NVC(dgPalletBox.GetCell(i, dgPalletBox.Columns["PALLETID"].Index).Value) == sPalletid)
            //                        {
            //                            Util.MessageValidation("SFU1776"); //"이미 전송 정보에 해당 Pallet ID가 존재합니다."
            //                            return;
            //                        }
            //                    }
            //                }

            //                //OutBox 정보 조회 함수 호출
            //                SelectBoxInfo(sPalletid);
            //            }
            //            else
            //            {
            //                int m = 0;
            //                for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
            //                {
            //                    if (sPalletid == Util.NVC(dgPalletBox.GetCell(i, dgPalletBox.Columns["PALLETID"].Index).Value))
            //                    {
            //                        m++;
            //                    }

            //                }
            //                if (m != 0)
            //                {
            //                    DataTableConverter.SetValue(dgPallet.Rows[dgPallet.CurrentRow.Index].DataItem, "CHK", true);
            //                    Util.AlertInfo("SFU2015"); //"해당 PALLETID를 전송정보 시트에서 삭제해 주세요."
            //                    return;
            //                }
            //            }

            //            //sprTray.SetViewportTopRow(0, sprTray.ActiveSheet.RowCount - 1);
            //            // 스캔된 마지막 셀이 바로 보이도록 스프레드 스크롤 하단으로 이동
            //            if (dgPalletBox.GetRowCount() > 0)
            //                dgPalletBox.ScrollIntoView(dgPalletBox.GetRowCount() - 1, 0);


            //            int iPallet_Cnt = 0;
            //            int iCell_Cnt = 0;

            //            for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
            //            {
            //                iPallet_Cnt++;
            //                iCell_Cnt += Util.NVC_Int(dgPalletBox.GetCell(i, dgPalletBox.Columns["QTY"].Index).Value);
            //            }

            //            //txtSelPalletQty.Value = iPallet_Cnt;
            //            //txtSelCellQty.Value = iCell_Cnt;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //        return;
            //    }
            //    finally
            //    {
            //        dgPallet.CurrentRow = null;
            //    }
            //}
        }
        #endregion

        #region dgOutBox_MouseUp
        private void dgOutBox_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region btnDelCell_Click : Cell 삭제
        private void btnDelCell_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region btnDelOutBox_Click : OutBox 삭제

        private void btnDelOutBox_Click(object sender, RoutedEventArgs e)
        {
          
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

            DataTable dt = ((DataView)dgPalletBox.ItemsSource).Table;
            dt.Rows[iRow].Delete();
            
            //string sInboxId = Util.NVC(dgPalletBox.GetCell(iRow, dgPalletBox.Columns["INBOXID"].Index).Value);
            //삭제시 Pallet 조회 시트상 해당 PalletID 체크박스 해제
            /*
            for (int i = 0; i < dgPalletBox.GetRowCount(); i++)
            {
                if (sInboxId == Util.NVC(dgPallet.GetCell(i, dgPalletBox.Columns["INBOXID"].Index).Value))
                {
                    DataTableConverter.SetValue(dgPallet.Rows[i].DataItem, "CHK", false);
                }
            }
            */
            // 선택된 행 삭제
            //dgPalletBox.IsReadOnly = false;
            //dgPalletBox.RemoveRow(iRow);
            //dgPalletBox.IsReadOnly = true;         
        }
        #endregion

        #region btnDelPallet_Click : Pallet 삭제
        private void btnDelPallet_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            
            string sPalletId = Util.NVC(dgPallet.GetCell(iRow, dgPallet.Columns["BOXID"].Index).Value);

            DataTable dtBox = ((DataView)dgPalletBox.ItemsSource).Table;
            DataRow[] drRows = dtBox.Select("PLTID = '" + sPalletId + "'");
            foreach (DataRow dr in drRows)
                dtBox.Rows.Remove(dr);

            DataTable dt = ((DataView)dgPallet.ItemsSource).Table;
            dt.Rows[iRow].Delete();
        }

        #endregion

        #region 포장 해체 이력 조회 시작기간 지정 : dtpDateFrom_SelectedDataTimeChanged
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region 포장 해체 이력 조회 종료기간 지정 : dtpDateTo_SelectedDataTimeChanged
        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #endregion

        #region [Method]

        #region 포장 대상 입력 가능 체크 : CanInputBoxid

        private bool CanInputBoxid(string boxid)
        {
            
            if (string.IsNullOrWhiteSpace(boxid))
            {
                //SFU1411	PALLETID를 입력해주세요
                Util.MessageValidation("SFU1411", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        initInputPalletId();
                    }
                });
                return false;
            }

            if (string.IsNullOrWhiteSpace((string)cboLineBox.SelectedValue))
            {
                //SFU1223	라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            return true;
        }
        #endregion

        #region 포장 해체 대상 추가 : Search
        private void Search(string palletId)
        {
            if (CanInputBoxid(palletId) == false)
                return;

            DataTable dtPallet = DataTableConverter.Convert(dgPallet.ItemsSource);

            // 중복데이터 입력시 미입력
            if (dtPallet.Rows.Count > 0
                && dtPallet.Select("BOXID = '" + palletId + "'").ToList().Count > 0)
            {
                txtPalletID.Text = string.Empty;
                initInputPalletId();
                return;
            }

            DataTable dtPalletBox = DataTableConverter.Convert(dgPalletBox.ItemsSource);

            // 중복데이터 입력시 미입력
            if (dtPalletBox.Rows.Count > 0
                && dtPalletBox.Select("OUTBOXID = '" + palletId + "' OR INBOXID = '" + (palletId.Substring(0, 1).Equals("C") ? palletId.Substring(0, palletId.Length - 1) : palletId) + "'").ToList().Count > 0)
            {
                txtPalletID.Text = string.Empty;
                initInputPalletId();
                return;
            }

            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("EQSGID");
            inDataTable.Columns.Add("BOXID");
            inDataTable.Columns.Add("USERID");

            DataRow newRow = inDataTable.NewRow();
            newRow["SRCTYPE"] = "UI";
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQSGID"] = (string)cboLineBox.SelectedValue;
            newRow["BOXID"] = palletId;
            newRow["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(newRow);

            loadingIndicator.Visibility = Visibility.Visible;
            txtPalletID.Text = string.Empty;

            new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_FOR_UNPACK_MB", "INDATA", "OUTDATA_PLT,OUTDATA_BOX", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtOutDataPallet = bizResult.Tables["OUTDATA_PLT"];
                    DataTable dtOutDataBox = bizResult.Tables["OUTDATA_BOX"];

                    if (dtOutDataPallet.Rows.Count > 0)
                    ///반품 PALLET이거나 포장상태가 PALLET 가 포장완료(PACKED) 이외의 경우 해체 가능
                    {
                        if (!(dtOutDataPallet.Rows[0]["RETURN_YN"].ToString() == "Y" || dtOutDataPallet.Rows[0]["PACKSTAT"].ToString() == "0"))
                        {
                            //포장 완료된 팔레트입니다. 수정 불가합니다.
                            Util.MessageValidation("SFU4296");
                            return;
                        }
                            
                    }

                    ///


                    dtOutDataPallet.Merge(dtPallet);
                    dtOutDataBox.Merge(dtPalletBox);

                    DataTable dtOutDataPallet1 = dtOutDataPallet.DefaultView.ToTable(true);
                    DataTable dtOutDataBox1 = dtOutDataBox.DefaultView.ToTable(true);


                    Util.GridSetData(dgPallet, dtOutDataPallet1, FrameOperation, true);

                    if (dgPallet.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["OUTBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["SUBLOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }

                    Util.GridSetData(dgPalletBox, dtOutDataBox1, FrameOperation, true);

                    if (dtOutDataBox.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgPalletBox.Columns["INBOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgPalletBox.Columns["SUBLOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    initInputPalletId();
                }

            }, indataSet);
        }
        #endregion

        #region Pallet 조회 : GetPalletList()
        /// <summary>
        /// Pallet 생성 조회
        /// </summary>
        /// <param name="idx"></param>
        private void GetPalletList(int idx = -1)
        {
            try
            {
            //    DataTable RQSTDT = new DataTable("INDATA");
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("EQSGID", typeof(string));
            //    RQSTDT.Columns.Add("BOXID", typeof(string));
            //    //RQSTDT.Columns.Add("PKG_LOTID");
            //    RQSTDT.Columns.Add("BOXSTAT_LIST", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);

            //    if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
            //        dr["BOXID"] = txtPalletID.Text;
            //    else
            //    {
            //        //dr["PKG_LOTID"] = null;
            //        dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
            //    }
            //    RQSTDT.Rows.Add(dr);

            //    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_TESLA_MB", "INDATA", "OUTDATA", RQSTDT);

            //    if (!RSLTDT.Columns.Contains("CHK"))
            //        RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

            //    Util.GridSetData(dgPallet, RSLTDT, FrameOperation, true);
            //    if (idx != -1)
            //    {
            //        DataTableConverter.SetValue(dgPallet.Rows[idx].DataItem, "CHK", true);
            //        dgPallet.SelectedIndex = idx;
            //        dgPallet.ScrollIntoView(idx, 0);
            //    }
            //    else
            //    {
            //        dgPallet.SelectedIndex = -1;
            //    }

            //    GetCompleteOutbox();

            //    if (RSLTDT.Rows.Count > 0)
            //    {
            //        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            //        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
            //    }
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

        #region OutBox 조회 : GetOutBoxList()
        private void GetOutBoxList(int idx = -1)
        {
            try
            {
                //    DataTable RQSTDT = new DataTable("INDATA");
                //    RQSTDT.Columns.Add("LANGID", typeof(string));
                //    RQSTDT.Columns.Add("AREAID", typeof(string));
                //    RQSTDT.Columns.Add("EQSGID", typeof(string));
                //    RQSTDT.Columns.Add("BOXID", typeof(string));
                //    //RQSTDT.Columns.Add("PKG_LOTID");
                //    RQSTDT.Columns.Add("BOXSTAT_LIST", typeof(string));

                //    DataRow dr = RQSTDT.NewRow();
                //    dr["LANGID"] = LoginInfo.LANGID;
                //    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //    dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);

                //    if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                //        dr["BOXID"] = txtPalletID.Text;
                //    else
                //    {
                //        //dr["PKG_LOTID"] = null;
                //        dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                //    }
                //    RQSTDT.Rows.Add(dr);

                //    DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_TESLA_MB", "INDATA", "OUTDATA", RQSTDT);

                //    if (!RSLTDT.Columns.Contains("CHK"))
                //        RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                //    Util.GridSetData(dgPallet, RSLTDT, FrameOperation, true);
                //    if (idx != -1)
                //    {
                //        DataTableConverter.SetValue(dgPallet.Rows[idx].DataItem, "CHK", true);
                //        dgPallet.SelectedIndex = idx;
                //        dgPallet.ScrollIntoView(idx, 0);
                //    }
                //    else
                //    {
                //        dgPallet.SelectedIndex = -1;
                //    }

                //    GetCompleteOutbox();

                //    if (RSLTDT.Rows.Count > 0)
                //    {
                //        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //    }
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

        #region  OUTBOX 조회 : GetCompleteOutbox() : 사용안함
        /// <summary>
        ///  OUTBOX 조회
        /// </summary>
        //private void GetCompleteOutbox()
        //{
        //    try
        //    {
        //        int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

        //        if (idxPallet < 0)
        //            return;

        //        DataTable inTable = new DataTable();
        //        inTable.Columns.Add("EQSGID", typeof(string));
        //        inTable.Columns.Add("PALLETID", typeof(string));
        //        inTable.Columns.Add("BOXID", typeof(string));
        //        inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

        //        DataRow newRow = inTable.NewRow();

        //        newRow["EQSGID"] = cboLinePallet.SelectedValue.ToString();
        //        newRow["PALLETID"] = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);
        //        newRow["MULTI_SHIPTO_FLAG"] = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["MULTI_SHIPTO_FLAG"].Index).Value);
        //        inTable.Rows.Add(newRow);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);
        //        Util.GridSetData(dgPalletBox, dtResult, FrameOperation, false);
        //        if (dtResult != null && dtResult.Rows.Count > 0)
        //            dgPalletBox.CurrentCell = dgPalletBox.GetCell(0, 1);

        //        string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
        //        if (dgPalletBox.Rows.Count > 0)
        //        {
        //            DataGridAggregate.SetAggregateFunctions(dgPalletBox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
        //        }

        //        _util.SetDataGridMergeExtensionCol(dgPalletBox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #region 완성 OUTBOX 조회 : GetCompleteOutbox() 사용안함.
        /// <summary>
        /// 완성 OUTBOX CELL OCV 체크
        /// </summary>
        //private bool GetCompleteOutboxOcvCheck(string BoxID)
        //{

        //    int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

        //    DataTable inTable = new DataTable();
        //    inTable.Columns.Add("BOXID", typeof(string));
        //    inTable.Columns.Add("LANGID", typeof(string));
        //    inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

        //    DataRow newRow = inTable.NewRow();
        //    newRow["BOXID"] = BoxID;
        //    newRow["LANGID"] = LoginInfo.LANGID;
        //    newRow["MULTI_SHIPTO_FLAG"] = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["MULTI_SHIPTO_FLAG"].Index).Value);
        //    inTable.Rows.Add(newRow);

        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET_MB", "INDATA", "OUTDATA", inTable);

        //    if (dtResult != null && dtResult.Rows.Count > 0)
        //    {
        //        for (int i = 0; i < dtResult.Rows.Count; i++)
        //        {
        //            if (!dtResult.Rows[i]["OCV_SPEC_RESULT"].ToString().Equals("OK"))
        //            {
        //                //OCV SPEC이 맞지 않아 포장이 불가능합니다.
        //                Util.MessageValidation("SFU8227");
        //                return false;
        //            }

        //            /*C20210906-000208 로 변경
        //            //C20210305-000498 로 수정. INBOXID 7자리 : 테슬라 재활용 인박스, 8자리 : 테슬라 일회용 인박스. 혼입금지
        //            int iInboxID_i_len = dtResult.Rows[i]["INBOXID"].ToString().Trim().Length;  //i번째 INBOXID 길이
        //            string sOutBoxID_i = dtResult.Rows[i]["OUTBOXID"].ToString().Trim();

        //            for (int inx = 0; inx < dgOutbox.GetRowCount(); inx++)
        //            {
        //                int iInboxID_dg_len = (Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[inx].DataItem, "INBOXID"))).Trim().Length;  //dgOutbox 그리드에 있는 INBOXID 길이
        //                if (iInboxID_i_len != iInboxID_dg_len)
        //                {
        //                    Util.MessageValidation("SFU3776", sOutBoxID_i);  //유형이 다른 인박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
        //                    return false;
        //                }
        //            }
        //            */

        //            //C20210906-000208
        //            string sTypeFlag_i = dtResult.Rows[i]["TYPE_FLAG"].ToString().Trim();
        //            string sOutBoxID_i = dtResult.Rows[i]["OUTBOXID"].ToString().Trim();

        //            for (int inx = 0; inx < dgBox.GetRowCount(); inx++)
        //            {
        //                string sTypeFlag_dg = (Util.NVC(DataTableConverter.GetValue(dgBox.Rows[inx].DataItem, "TYPE_FLAG"))).Trim();
        //                if (sTypeFlag_i != sTypeFlag_dg)
        //                {
        //                    Util.MessageValidation("SFU3806", sOutBoxID_i);  //유형이 다른 박스는 하나의 팔레트에 혼입할 수 없습니다. [%1]
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        #endregion

        #region 완성 OutBox 재발행 :  PrintLabel()

        /// <summary>
        /// 완성 OutBox 재발행
        /// </summary>
        /// <param name="zpl"></param>
        /// <param name="drPrtInfo"></param>
        /// <returns></returns>
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







        #endregion

        #region SelectBoxInfo : PalletID로 OutBox 추가

        private void SelectBoxInfo(string sPalletID)
        {

            //if (cboArea.SelectedValue.ToString() == "SELECT" || cboArea.SelectedValue.ToString() == "")
            //{
            //    // 동을 선택하세요
            //    Util.MessageInfo("SFU1499");
            //    return;
            //}
            //DA_PRD_SEL_OQC_PLT_CP
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                ///dr["AREAID"] = sAREAID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OQC_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dgPalletBox.GetRowCount() == 0)
                {
                    ////dgPalletBox.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgPalletBox, dtResult, FrameOperation, true);
                    dsGet = dtResult;
                }
                else
                {
                    dsGet = dtResult;

                    if (dtResult.Rows.Count > 0)
                    {
                        //전송정보 로우 수 체크(테이블 결합 루프용)
                        DataTable DT = DataTableConverter.Convert(dgPalletBox.ItemsSource);

                        DataRow drGet = dsGet.Rows[0];
                        DataRow newDr = DT.NewRow();
                        foreach (DataColumn col in dsGet.Columns)
                        {
                            newDr[col.ColumnName] = drGet[col.ColumnName];
                        }
                        DT.Rows.Add(newDr);
                        ////dgPalletBox.ItemsSource = DataTableConverter.Convert(DT);
                        Util.GridSetData(dgPalletBox, DT, FrameOperation, true);
                    }

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }




        #endregion

        #region 포장 대상 입력 초기화 : Clear
        private void Clear()
        {
            txtPalletID.Text = string.Empty;
            Util.gridClear(dgPallet);
            Util.gridClear(dgPalletBox);
            initInputPalletId();
        }
        #endregion

        #region 포장 해체 이력 조회 : Search_Hist
        private void Search_Hist()
        {
            if (string.IsNullOrWhiteSpace(Util.NVC(cboLinePallet2.SelectedValue))
                || Util.NVC(cboLinePallet2.SelectedValue) == "SELECT")
            {
                //SFU1223	라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return;
            }

            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("FROM_DATE");
            inDataTable.Columns.Add("TO_DATE");
            inDataTable.Columns.Add("AREAID");
            inDataTable.Columns.Add("BOXID");
            inDataTable.Columns.Add("PKG_LOTID");
            inDataTable.Columns.Add("LANGID");

            DataRow newRow = inDataTable.NewRow();
            newRow["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
            newRow["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["BOXID"] = string.IsNullOrWhiteSpace(txtPalletID_Hist.Text) ? null : txtPalletID_Hist.Text;
            newRow["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtAssyLotID.Text) ? null : txtAssyLotID.Text;
            newRow["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(newRow);

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService("BR_PRD_GET_UNPACK_HIST_MB", "INDATA", "OUTDATA", inDataTable, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    //if (!bizResult.Columns.Contains("CHK"))
                    //    bizResult.Columns.Add("CHK");

                    Util.GridSetData(dgHist, bizResult, FrameOperation, true);

                    if (dgHist.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgHist.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgHist.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
        #endregion

        #endregion





    }
}
