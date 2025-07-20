/*************************************************************************************
 Created Date : 2018.09.19
      Creator : 
   Decription : 자동차 활성화 후공정 - 폐기 관리
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_503 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private CheckBoxHeaderType _HeaderTypeCell;
        private CheckBoxHeaderType _HeaderTypeHistory;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public FORM001_503()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapLot"))
            {
                cboResnCodeLot.SelectedIndex = 0;
                txtUserNameLot.Text = string.Empty;
                txtUserNameLot.Tag = string.Empty;
                txtResnNoteLot.Text = string.Empty;

                Util.gridClear(dgDefectDetail);
            }
            else if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapSubLot"))
            {
                cboResnCodeSubLot.SelectedIndex = 0;
                txtUserNameSubLot.Text = string.Empty;
                txtUserNameSubLot.Tag = string.Empty;
                txtResnNoteSubLot.Text = string.Empty;
                txtScanQty.Value = 0;
            }

        }
        private void SetControl()
        {
            _HeaderTypeCell = CheckBoxHeaderType.Zero;
            _HeaderTypeHistory = CheckBoxHeaderType.Zero;
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //폐기사유코드 
            String[] sFilter = { "SCRAP_LOT" };
            _combo.SetCombo(cboResnCodeLot, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "ACTIVITIREASON");
            _combo.SetCombo(cboResnCodeSubLot, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "ACTIVITIREASON");

            // 폐기이력조회
            //동
            C1ComboBox[] cboAreaHisChild = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboAreaHis, CommonCombo.ComboStatus.NONE, cbChild: cboAreaHisChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentHisParent = { cboAreaHis };
            C1ComboBox[] cboEquipmentSegmentHisChild = { cboProcessHis };
            _combo.SetCombo(cboEquipmentSegmentHis, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbChild: cboEquipmentSegmentHisChild, cbParent: cboEquipmentSegmentHisParent);

            //공정
            C1ComboBox[] cboProcessLotParent = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboProcessHis, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cboProcessLotParent);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearchLot);
            listAuth.Add(btnScrapLot);
            listAuth.Add(btnSaveSubLot);
            listAuth.Add(btnSearchHis);
            listAuth.Add(btnCancelHis);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        #region [폐기 등록(Lot 단위)]

        private void txtDefectLot_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
                        txtDefectLot.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtDefectLot.Text))
                            txtDefectLot_KeyDown(txtDefectLot, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtDefectLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    string sLotID = txtDefectLot.Text.Trim();

                    DataTable dtLot = DataTableConverter.Convert(dgDefectLot.ItemsSource);


                    if (dtLot.Rows.Count > 0)
                    {
                        DataRow[] drDup = dtLot.Select("LOTID = '" + sLotID + "'");

                        if (drDup.Length > 0)
                        {
                            // 불량LOT이 존재합니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU5042"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtDefectLot.Focus();
                                    txtDefectLot.Text = string.Empty;
                                }
                            });

                            txtDefectLot.Text = string.Empty;
                            return;
                        }
                    }

                    // LOT 정보 조회
                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("LANGID");
                    RQSTDT.Columns.Add("LOTID");

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sLotID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DEFECTLOT_AUTO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            if (dtLot.Rows.Count > 0)
                            {
                                // 중복 체크
                                DataRow[] drList = dtLot.Select("LOTID = '" + dtResult.Rows[0]["LOTID"] + "'");

                                if (drList.Length > 0)
                                {
                                    // 불량LOT이 존재합니다.
                                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU5042"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtDefectLot.Focus();
                                            txtDefectLot.Text = string.Empty;
                                        }
                                    });

                                    txtDefectLot.Text = string.Empty;
                                    return;
                                }

                                // Row 추가
                                dtResult.Merge(dtLot);
                            }

                            Util.GridSetData(dgDefectLot, dtResult, null, false);

                            // Cell Count(중복제거)
                            DataTable dt = dtResult.DefaultView.ToTable(true, new string[] { "LOTID", "WIPQTY", "INPUTQTY" });
                            decimal WipQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));
                            decimal InputQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("INPUTQTY"));

                            DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = WipQty.ToString("###,###") } });
                            DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["INPUTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = InputQty.ToString("###,###") } });
                        }
                    }

                    txtDefectLot.Text = string.Empty;
                    txtDefectLot.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtDefectLot.Text = string.Empty;
                        txtDefectLot.Focus();
                    }
                });
            }
            finally
            {
            }
        }

        /// <summary>
        /// 초기화
        /// <summary>
        private void btnClearLot_Click(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            Util.gridClear(dgDefectLot);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearchLot_Click(object sender, RoutedEventArgs e)
        {
            SearchLotProcess();
        }

        private void dgDefectLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgDefectLot.SelectedIndex = idx;

                        // Cell 정보 조회
                        SearchCellProcess(Util.NVC(drv.Row["LOTID"].ToString()));

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserNameLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnUserLot_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        /// <summary>
        /// 폐기등록
        /// </summary>
        private void btnScrapLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveScrapLot())
                return;

            // 폐기하시겠습니까?
            Util.MessageConfirm("SFU4191", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveScrapLotProcess();
                }
            });
        }

        #endregion

        #region [폐기 등록(Cell 단위)]

        private void txtDefectSubLot_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
                        txtDefectSubLot.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtDefectSubLot.Text))
                            txtDefectSubLot_KeyDown(txtDefectSubLot, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtDefectSubLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    string sSubLotID = txtDefectSubLot.Text.Trim();

                    DataTable dtSubLot = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);

                    if (dtSubLot.Rows.Count > 0)
                    {
                        DataRow[] drDup = dtSubLot.Select("SUBLOTID = '" + sSubLotID + "'");

                        if (drDup.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtDefectSubLot.Focus();
                                    txtDefectSubLot.Text = string.Empty;
                                }
                            });

                            txtDefectSubLot.Text = string.Empty;
                            return;
                        }
                    }

                    // Sublot 정보 조회
                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("LANGID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("OPERATION");

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SUBLOTID"] = sSubLotID;
                    dr["OPERATION"] = "S";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_AUTO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            if (dtSubLot.Rows.Count > 0)
                            {
                                // 중복 체크
                                DataRow[] drList = dtSubLot.Select("SUBLOTID = '" + dtResult.Rows[0]["SUBLOTID"] + "'");

                                if (drList.Length > 0)
                                {
                                    // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtDefectSubLot.Focus();
                                            txtDefectSubLot.Text = string.Empty;
                                        }
                                    });

                                    txtDefectSubLot.Text = string.Empty;
                                    return;
                                }

                                // Row 추가
                                dtResult.Merge(dtSubLot);
                            }

                            Util.GridSetData(dgDefectSubLot, dtResult, null, true);
                            txtScanQty.Value = dtResult.Rows.Count;
                        }
                    }

                    txtDefectSubLot.Text = string.Empty;
                    txtDefectSubLot.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtDefectSubLot.Text = string.Empty;
                        txtDefectSubLot.Focus();
                    }
                });
            }
            finally
            {
            }
        }

        private void tbCheckHeaderAllDefect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectSubLot;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCell)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCell)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCell = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCell = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void txtUserNameSubLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnUserSubLot_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        /// <summary>
        /// 엑셀 파일 오픈
        /// </summary>
        private void btnOpenExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {
                txtScanQty.Value = 0;

                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        // DataTable dtResult = exl.GetSheetData(str[0]);

                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable inSubLot = new DataTable();
                        inSubLot.Columns.Add("CHK", typeof(Boolean));
                        inSubLot.Columns.Add("SUBLOTID", typeof(string));

                        DataRow newRow = null;

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = inSubLot.NewRow();
                            newRow["CHK"] = 0;
                            newRow["SUBLOTID"] = sheet.GetCell(rowInx, 0).Text;
                            inSubLot.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgDefectSubLot, inSubLot, null, true);
                        txtScanQty.Value = inSubLot.Rows.Count;

                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDeleteSubLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteSubLot())
                return;

            DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);
            dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgDefectSubLot, dt, null, true);
            txtScanQty.Value = dt.Rows.Count;
        }

        /// <summary>
        /// 폐기등록
        /// </summary>
        private void btnSaveSubLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveSubLot())
                return;

            // 폐기하시겠습니까?
            Util.MessageConfirm("SFU4191", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveScrapSubLotProcess();
                }
            });
        }

        #endregion

        #region [폐기 이력 조회]
        private void txtHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchHistoryProcess(false, tb);
            }
        }

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryProcess(true);
        }

        private void dgHistory_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("CHK"))
            {
                if (!Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "UPDATE_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

            //CELL 정보 조회 전 Grid 초기화 선행작업.
            Util.gridClear(dgHistoryCell);

            // Cell 정보 조회
            DataTable dtAct = DataTableConverter.Convert(dgHistory.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LOTID")) + "'").CopyToDataTable().DefaultView.ToTable(true, "ACTID");
            DataTable dtMin = DataTableConverter.Convert(dgHistory.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LOTID")) + "'").CopyToDataTable();

            var MinMax = from dt in dtMin.AsEnumerable()
                         group dt by dt.Field<string>("LOTID") into grp
                         select new
                         {
                             Min = grp.Min(T => T.Field<string>("ACTDTTM_MIN")),
                             Max = grp.Max(T => T.Field<string>("ACTDTTM"))
                         };

            string ActID = string.Empty;

            foreach (DataRow dr in dtAct.Rows)
            {
                ActID += dr["ACTID"] + ",";
            }

            foreach (var data in MinMax)
            {
                SearchCellHistoryProcess(data.Min, data.Max, Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LOTID"))
                                       , ActID.Substring(0, ActID.Length - 1)
                                       , Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DPMS_RCV_ISS_NO")));
                break;
            }

            DataRow[] drE = DataTableConverter.Convert(dgHistory.ItemsSource).Select("CHK = 1");

            if (drE.Length == 0)
            {
                Util.gridClear(dgHistoryCell);
            }
        }

        private void tbCheckHeaderAllHistory_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgHistory;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeHistory)
                {
                    case CheckBoxHeaderType.Zero:
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "UPDATE_YN")).Equals("Y"))
                        {
                            DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        }
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeHistory)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeHistory = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeHistory = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);

        }

        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("DPMS_RCV_ISS_NO2"))
                    {
                        if (!e.Cell.Column.Width.Equals(0))
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(0);
                    }

                    if (e.Cell.Column.Name.Equals("CELLQTY"))
                    {
                        if (Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CELLQTY").GetString()).Equals(-1))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                }
            }));

        }

        private void txtUserNameHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnUserHis_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        /// <summary>
        /// 폐기취소
        /// </summary>
        private void btnCancelHis_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelScrap())
                return;

            // 취소하시겠습니까?
            Util.MessageConfirm("SFU4616", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelScrapProcess();
                }
            });
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 폐기 Lot 조회
        /// </summary>
        private void SearchLotProcess()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDefectLot.Text))
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량LOT"));
                    return;
                }

                Util.gridClear(dgDefectLot);
                Util.gridClear(dgDefectDetail);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID_LIKE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID_LIKE"] = txtDefectLot.Text;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECTLOT_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectLot, bizResult, null, false);

                        // Cell Count(중복제거)
                        DataTable dt = bizResult.DefaultView.ToTable(true, new string[] { "LOTID", "WIPQTY", "INPUTQTY" });
                        decimal WipQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));
                        decimal InputQty = dt.AsEnumerable().Sum(r => r.Field<decimal>("INPUTQTY"));

                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = WipQty.ToString("###,###") } });
                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["INPUTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = InputQty.ToString("###,###") } });
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량 Cell 조회(Lot단위)
        /// </summary>
        private void SearchCellProcess(string DefectLot)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DefectLot;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_DEFECT_LOT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectDetail, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 폐기 등록(LOT단위)
        /// </summary>
        private void SaveScrapLotProcess()
        {
            try
            {
                // DATA SET 
                DataRow[] dr = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");
                if (dr.Length == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("RESNCODE", typeof(string));
                inTable.Columns.Add("RESNNOTE", typeof(string));
                inTable.Columns.Add("PRCS_TYPE_FLAG", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("CELL_QTY", typeof(decimal));

                DataTable inCell = inDataSet.Tables.Add("INCELL");
                inCell.Columns.Add("CELLID", typeof(string));
                inCell.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = dr[0]["AREAID"].ToString();
                newRow["ACT_USERID"] = txtUserNameLot.Tag.ToString();
                newRow["RESNCODE"] = cboResnCodeLot.SelectedValue.ToString();
                newRow["RESNNOTE"] = txtResnNoteLot.Text;
                newRow["PRCS_TYPE_FLAG"] = "L";
                inTable.Rows.Add(newRow);

                foreach (DataRow drins in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = drins["LOTID"].ToString();
                    newRow["CELL_QTY"] = Util.NVC_Decimal(drins["WIPQTY"]);
                    inLot.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_SCRAP_SUBLOT_AUTO", "INDATA,INLOT,INCELL", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        InitializeUserControls();

                        DataTable dt = DataTableConverter.Convert(dgDefectLot.ItemsSource);
                        dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
                        dt.AcceptChanges();
                        Util.GridSetData(dgDefectLot, dt, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 폐기 등록(Cell단위)
        /// </summary>
        private void SaveScrapSubLotProcess()
        {
            try
            {
                // DATA SET 
                DataTable dtScrap = DataTableConverter.Convert(dgDefectSubLot.ItemsSource).Select("CHK = 1").CopyToDataTable();
                if (dtScrap == null || dtScrap.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("RESNCODE", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("RESNNOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["RESNCODE"] = cboResnCodeSubLot.SelectedValue.ToString();
                newRow["ACT_USERID"] = txtUserNameSubLot.Tag.ToString();
                newRow["RESNNOTE"] = txtResnNoteSubLot.Text;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // XML string 500건당 1개 생성
                int rowCount = 0;
                string sXML = string.Empty;

                for (int row = 0; row < dtScrap.Rows.Count; row++)
                {
                    if (row == 0 || row % 500 == 0)
                    {
                        sXML = "<root>";
                    }

                    sXML += "<DT><L>" + dtScrap.Rows[row]["SUBLOTID"] + "</L></DT>";

                    if ((row + 1) % 500 == 0 || row + 1 == dtScrap.Rows.Count)
                    {
                        sXML += "</root>";

                        newRow = inSubLot.NewRow();
                        newRow["SUBLOTID"] = sXML;
                        inSubLot.Rows.Add(newRow);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SCRAP_SUBLOT_AUTO", "INDATA,INSUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        InitializeUserControls();
                        Util.GridSetData(dgDefectSubLot, bizResult.Tables["OUTDATA"], null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 폐기 이력 조회
        /// </summary>
        private void SearchHistoryProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtDefectLotHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량LOT"));
                            return;
                        }
                        else if (tb.Name.Equals("txtProdidHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량명"));
                            return;
                        }
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DEFECT_LOT", typeof(string));
                inTable.Columns.Add("RESNNAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);

                if (!string.IsNullOrWhiteSpace(txtDefectLotHis.Text))
                {
                    newRow["DEFECT_LOT"] = txtDefectLotHis.Text;
                }
                if (!string.IsNullOrWhiteSpace(txtProdidHis.Text))
                {
                    newRow["PRODID"] = txtProdidHis.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtResnNameHis.Text))
                {
                    newRow["RESNNAME"] = txtResnNameHis.Text;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHis);
                    if (newRow["EQSGID"].Equals(""))
                    {
                        Util.MessageValidation("SFU1223");
                        return;
                    }

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessHis.SelectedValue.ToString()) ? null : cboProcessHis.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_SCRAP_LIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, null, false);

                        //// Cell Count(중복제거)
                        //DataTable dt = bizResult.DefaultView.ToTable(true, "LOTID", "WIPQTY");
                        //decimal CellCount = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));

                        //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = CellCount.ToString("###,###") } });
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 폐기 취소
        /// </summary>
        private void CancelScrapProcess()
        {
            try
            {
                DataRow[] drScrap = DataTableConverter.Convert(dgHistory.ItemsSource).Select("CHK = 1");

                // DATA SET 
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ACT_USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inScrp = inDataSet.Tables.Add("INSCRP");
                inScrp.Columns.Add("DPMS_RCV_ISS_NO", typeof(string));

                ///////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = drScrap[0]["AREAID"].ToString();
                newRow["ACT_USERID"] = txtUserNameHis.Tag.ToString();
                newRow["NOTE"] = txtResnNoteHis.Text;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                foreach (DataRow drins in drScrap)
                {
                    newRow = inScrp.NewRow();
                    newRow["DPMS_RCV_ISS_NO"] = drins["DPMS_RCV_ISS_NO"].ToString();
                    inScrp.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SCRAP_SUBLOT_AUTO", "INDATA,INSCRP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        SearchHistoryProcess(true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 폐기 이력 Cell 조회
        /// </summary>
        private void SearchCellHistoryProcess(string FromDT, string ToDT, string DefectLot, string ActID, string DPMS_No)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_ACTDTTM", typeof(string));
                inTable.Columns.Add("TO_ACTDTTM", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("RESNGRID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("DPMS_RCV_ISS_NO", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_ACTDTTM"] = FromDT;
                newRow["TO_ACTDTTM"] = ToDT;
                newRow["LOTID"] = DefectLot;
                newRow["RESNGRID"] = "DEFECT_EQPOUT_AUTO,DEFECT_SURFACE_AUTO";
                newRow["ACTID"] = ActID;
                newRow["DPMS_RCV_ISS_NO"] = DPMS_No;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryCell, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool ValidationSaveScrapLot()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectLot, "CHK");

            if (rowIndex < 0 || dgDefectDetail.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (cboResnCodeLot.SelectedValue.ToString().Equals("SELECT"))
            {
                // % 1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("폐기사유"));
                return false;
            }

            if (txtUserNameLot.Tag == null || string.IsNullOrWhiteSpace(txtUserNameLot.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationDeleteSubLot()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectSubLot, "CHK", true);

            if (rowIndex < 0 || dgDefectSubLot.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationSaveSubLot()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectSubLot, "CHK", true);

            if (rowIndex < 0 || dgDefectSubLot.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            // 중복 CELL 체크
            DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource).Select("CHK = 1").CopyToDataTable();
            var dupSublot = dt.AsEnumerable().GroupBy(x => x["SUBLOTID"]).Where(x => x.Count() > 1);

            if (dupSublot.Count() > 0)
            {
                foreach (var data in dupSublot)
                {
                    // 중복 데이터가 존재 합니다. % 1
                    Util.MessageValidation("SFU2051", data.Key);
                    return false;
                }
            }

            if (cboResnCodeSubLot.SelectedValue.ToString().Equals("SELECT"))
            {
                // % 1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("폐기사유"));
                return false;
            }

            if (txtUserNameSubLot.Tag == null || string.IsNullOrWhiteSpace(txtUserNameSubLot.Tag.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationCancelScrap()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgHistory, "CHK");

            if (rowIndex < 0 || dgHistory.GetRowCount() == 0)
            {
                // 취소할 LOT을 선택하세요.
                Util.MessageValidation("SFU1938");
                return false;
            }

            if (txtUserNameHis.Tag == null || string.IsNullOrWhiteSpace(txtUserNameHis.Tag.ToString()))
            {
                // 의뢰자를 선택하세요
                Util.MessageValidation("SFU4151");
                return false;
            }

            return true;
        }


        #endregion

        #region [팝업]
        private void PopupUser()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];

            if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapLot"))
                Parameters[0] = txtUserNameLot.Text;
            else if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapSubLot"))
                Parameters[0] = txtUserNameSubLot.Text;
            else
                Parameters[0] = txtUserNameHis.Text;

            C1WindowExtension.SetParameters(wndPerson, Parameters);

            wndPerson.Closed += new EventHandler(popupUser_Closed);

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

        private void popupUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapLot"))
                {
                    txtUserNameLot.Text = popup.USERNAME;
                    txtUserNameLot.Tag = popup.USERID;
                }
                else if (((System.Windows.FrameworkElement)tabScrap.SelectedItem).Name.Equals("ctbScrapSubLot"))
                {
                    txtUserNameSubLot.Text = popup.USERNAME;
                    txtUserNameSubLot.Tag = popup.USERID;
                }
                else
                {
                    txtUserNameHis.Text = popup.USERNAME;
                    txtUserNameHis.Tag = popup.USERID;
                }
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        #endregion

        #region [Func]

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

        #endregion

    }
}
