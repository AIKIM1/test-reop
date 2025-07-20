/*************************************************************************************
 Created Date : 2021.02.09
      Creator : 주건태
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

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
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// </summary>
    public partial class COM001_006_HOLD_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        private string _holdTrgtCode = string.Empty;
        private double _maxHoldCell = 0;
        private double _cellDivideCnt = 0;

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

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preCellHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllCellHold = new CheckBox()
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

        public COM001_006_HOLD_CELL()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tempParameters = C1WindowExtension.GetParameters(this);

            if (tempParameters != null && tempParameters.Length >= 3)
            {
                _holdTrgtCode = tempParameters[0].ToString();
                _maxHoldCell = Double.Parse(tempParameters[1].ToString());
                _cellDivideCnt = Double.Parse(tempParameters[2].ToString());
            }

            xTextMaxCnt.Text = "Max Cell : " + _maxHoldCell.ToString();

            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboAreaCellHold, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                dt.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, dt, FrameOperation);
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
        #endregion

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

        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Column.Name == "HOLD_REG_QTY")
            {
                string hold_req_qty = Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["HOLD_REG_QTY"].Index).Value);
                int iHold_req_qty;

                if (!string.IsNullOrWhiteSpace(hold_req_qty) && !int.TryParse(hold_req_qty, out iHold_req_qty))
                {
                    Util.MessageInfo("SFU3435");    //숫자만 입력해주세요
                }
            }
        }

        #region Hold 리스트 추가/제거
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
                od.FileName = "Hold_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    if (_holdTrgtCode == "SUBLOT")
                    {
                        sheet[0, 0].Value = "CELLID";
                        sheet[1, 0].Value = "Please input cell ID from here";
                        sheet[2, 0].Value = "Sample Cell ID G86AI051019494";

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

                        if (sheet.GetCell(0, 0).Text != "CELLID")
                        {
                            Util.MessageValidation("SFU4424");  //형식에 맞는 EXCEL파일을 선택해 주세요.
                            return;
                        }

                        if (sheet.Rows.Count - 1 > Convert.ToInt32(_maxHoldCell))   //헤더는 데이터 개수에서 제외
                        {
                            Util.MessageValidation("SFU8217", _maxHoldCell); //최대 [%1]까지 등록 가능 합니다.
                            return;
                        }

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // LOTID 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null)
                            {
                                return;
                            }

                            DataRow dr = dtInfo.NewRow();
                            dr["CHK"] = true;
                            dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dtInfo.Rows.Add(dr);
                        }

                        if (dtInfo.Rows.Count > 0)
                        {
                            dtInfo = dtInfo.DefaultView.ToTable(true);
                        }

                        Util.GridSetData(dgHold, dtInfo, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            Util.GridSetData(dgHold, dt, FrameOperation);
            dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

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
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3552");  //저장 할 DATA가 없습니다.
                return;
            }

            if (dtInfo.Rows.Count > _maxHoldCell)
            {
                Util.MessageValidation("SFU8217", _maxHoldCell); //최대 [%1]까지 등록 가능 합니다.
                return;
            }

            if (dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("STRT_SUBLOTID")))).ToList().Count > 0)
            {	
                Util.MessageValidation("SFU4351");  //미입력된 항목이 존재합니다.
                return;
            }

            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
            }

            if (string.IsNullOrEmpty(txtUser.Text))
            {
                Util.MessageValidation("SFU4350");  //해제 예정 담당자를 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                Util.MessageValidation("SFU4300");  //Hold 사유를 입력하세요.
                return;
            }

            try
            {
                CMM_BOX_HOLD_CELL_PROGRESSBAR popupProgressing = new CMM_BOX_HOLD_CELL_PROGRESSBAR();
                popupProgressing.FrameOperation = FrameOperation;

                if (popupProgressing != null)
                {
                    object[] parameters = new object[10];
                    parameters[0] = dtInfo;
                    parameters[1] = _maxHoldCell;
                    parameters[2] = _cellDivideCnt;
                    parameters[3] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                    parameters[4] = txtUser.Tag;
                    parameters[5] = txtNote.Text;
                    parameters[6] = _holdTrgtCode;
                    parameters[7] = "N";    //PACK_HOLD_FLAG
                    parameters[8] = "ZZS_CELL_HOLD";    //HOLD_TYPE_CODE
                    parameters[9] = "HOLD";

                    C1WindowExtension.SetParameters(popupProgressing, parameters);
                    popupProgressing.Closed += new EventHandler(popupProgressing_Closed);
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(popupProgressing);
                            popupProgressing.BringToFront();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupProgressing_Closed(object sender, EventArgs e)
        {
            CMM_BOX_HOLD_CELL_PROGRESSBAR window = sender as CMM_BOX_HOLD_CELL_PROGRESSBAR;
            if (window.DialogResult == MessageBoxResult.OK)
            {
               // Search();
            }
            this.grdMain.Children.Remove(window);

            //btnClose_Click(null, null);
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
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

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dtpSchdDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.

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

        private void btnSearchCell_Click(object sender, RoutedEventArgs e)
        {
            SearchCell();
        }

        private void SearchCell()
        {
            try
            {
                TimeSpan timeSpan = dtpDateToCellHold.SelectedDateTime.Date - dtpDateFromCellHold.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    Util.MessageValidation("SFU3569");  //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    return;
                }

                if (timeSpan.Days > 7)
                {
                    Util.MessageValidation("SFU3567");  //조회기간은 7일을 초과 할 수 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(txtCellIDCellHold.Text) && string.IsNullOrEmpty(txtLotIDCellHold.Text) && string.IsNullOrEmpty(txtProdIDCellHold.Text))
                {
                    Util.MessageValidation("SFU5018");  //조회조건이 하나라도 있어야 합니다.
                    return;
                }

                    DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("FROM_DTTM");
                RQSTDT.Columns.Add("TO_DTTM");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("PROD_LOTID");
                RQSTDT.Columns.Add("PRODID");
                RQSTDT.Columns.Add("AREAID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCellIDCellHold.Text))
                {
                    dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellIDCellHold.Text) ? null : txtCellIDCellHold.Text;
                }
                else
                {

                    dr["FROM_DTTM"] = dtpDateFromCellHold.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_DTTM"] = dtpDateToCellHold.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["PROD_LOTID"] = string.IsNullOrEmpty(txtLotIDCellHold.Text) ? null : txtLotIDCellHold.Text;
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdIDCellHold.Text) ? null : txtProdIDCellHold.Text;
                    dr["AREAID"] = (string)cboAreaCellHold.SelectedValue == "" ? null : (string)cboAreaCellHold.SelectedValue;
                }

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_BASIC", "INDATA", "OUTDATA", RQSTDT);

                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult = _Util.gridCheckColumnAdd(dtResult, "CHK");
                }

                if(dtResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1905");  //조회된 Data가 없습니다.
                    return;
                }

                Util.GridSetData(dgCell, dtResult, FrameOperation);
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

        private void dgCell_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            preCellHold.Content = chkAllCellHold;
                            e.Column.HeaderPresenter.Content = preCellHold;
                            chkAllCellHold.Checked -= new RoutedEventHandler(checkAllCellHold_Checked);
                            chkAllCellHold.Unchecked -= new RoutedEventHandler(checkAllCellHold_Unchecked);
                            chkAllCellHold.Checked += new RoutedEventHandler(checkAllCellHold_Checked);
                            chkAllCellHold.Unchecked += new RoutedEventHandler(checkAllCellHold_Unchecked);
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

        void checkAllCellHold_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAllCellHold.IsChecked)
            {
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void checkAllCellHold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllCellHold.IsChecked)
            {
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void btnCellHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgCell.ItemsSource);

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();
                if (drList.Count <= 0)
                {
                    Util.MessageValidation("SFU3538");    //선택된 데이터가 없습니다
                    return;
                }

                DataTable dtHold = new DataTable();
                dtHold.Columns.Add("CHK");
                dtHold.Columns.Add("STRT_SUBLOTID");

                for (int inx = 0; inx < drList.Count; inx++)
                {
                    DataRow dr = dtHold.NewRow();
                    dr["CHK"] = drList[inx]["CHK"];
                    dr["STRT_SUBLOTID"] = drList[inx]["SUBLOTID"];

                    dtHold.Rows.Add(dr);
                }

                Util.GridSetData(dgHold, dtHold, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }


    }
}
