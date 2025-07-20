/*************************************************************************************
 Created Date : 
      Creator : 
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
    public partial class COM001_006_RELEASE_CELL : C1Window, IWorkArea
    {
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

        public COM001_006_RELEASE_CELL()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, System.EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tempParameters = C1WindowExtension.GetParameters(this);

            if (tempParameters != null && tempParameters.Length >= 4)
            {
                _holdTrgtCode = tempParameters[0].ToString();
                _maxHoldCell = Double.Parse(tempParameters[1].ToString());
                _cellDivideCnt = Double.Parse(tempParameters[2].ToString());

                InitControl();

                if (tempParameters[3] != null)
                {
                    DataTable dtInfo = (DataTable)tempParameters[3];
                    if (dtInfo != null)
                    {
                        Util.GridSetData(dgHold, dtInfo, FrameOperation);
                        chkAll.IsChecked = true;
                    }
                }
            }

            xTextMaxCnt.Text = "Max Cell : " + _maxHoldCell.ToString();
        }

        private void InitControl()
        {
            DataTable dtInfo = new DataTable();

            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                dtInfo.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, dtInfo, FrameOperation);
        }

        private void btnDownLoad_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();
                
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Release_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";
                    sheet[1, 0].Value = "Please input cell ID from here";
                    sheet[2, 0].Value = "Sample Cell ID G86AI051019494";

                    sheet[0, 0].Style = styel;
                    sheet.Columns[0].Width = 1500;

                    c1XLBook1.Save(od.FileName);
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUpLoad_Click(object sender, System.Windows.RoutedEventArgs e)
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

                        DataTable dtExcl = new DataTable();
                        dtExcl.Columns.Add("SUBLOTID", typeof(string));

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

        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
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

                if (string.IsNullOrEmpty(txtNote.Text))
                {
                    Util.MessageValidation("SFU4301");  //Hold 해제 사유를 입력하세요.
                    return;
                }
                
                // SFU4046 HOLD 해제 하시겠습니까?
                Util.MessageConfirm("SFU4046", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
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

        private void Save()
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHold.ItemsSource);

                CMM_BOX_HOLD_CELL_PROGRESSBAR popupProgressing = new CMM_BOX_HOLD_CELL_PROGRESSBAR();
                popupProgressing.FrameOperation = FrameOperation;

                if (popupProgressing != null)
                {
                    object[] parameters = new object[10];
                    parameters[0] = dtInfo;
                    parameters[1] = _maxHoldCell;
                    parameters[2] = _cellDivideCnt;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    parameters[5] = txtNote.Text;
                    parameters[6] = _holdTrgtCode;
                    parameters[7] = "N";    //PACK_HOLD_FLAG
                    parameters[8] = "ZZS_CELL_HOLD";    //HOLD_TYPE_CODE
                    parameters[9] = "RELEASE";

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
    }
}
