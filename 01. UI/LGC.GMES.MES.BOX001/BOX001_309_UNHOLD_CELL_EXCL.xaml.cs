/*************************************************************************************
 Created Date : 2020.08.11
      Creator : INS 김동일K
   Decription : Cell Hold 해제(Excel) 기능 추가 : C20200711-000007
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.11  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_213_RELEASE_CELL_EXCL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_309_UNHOLD_CELL_EXCL : C1Window, IWorkArea
    {
        private Util _Util = new Util();

        private DataTable _DtSearchInfo = new DataTable();

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

        public BOX001_309_UNHOLD_CELL_EXCL()
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
            InitControl();
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
                    sheet[1, 0].Value = "PB23K1B264";
                    sheet[2, 0].Value = "G86AI051019494";

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

                _DtSearchInfo.Clear();

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
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }

                        DataTable dtExcl = new DataTable();
                        dtExcl.Columns.Add("SUBLOTID", typeof(string));

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // LOTID 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null)
                                return;

                            DataRow dr = dtInfo.NewRow();
                            dr["CHK"] = true;
                            dr["STRT_SUBLOTID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dtInfo.Rows.Add(dr);

                            //DataTable dtTmp = Search(sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim());

                            //if (dtTmp?.Rows?.Count > 0)
                            //{
                            //    DataRow dr = dtInfo.NewRow();
                            //    dr["CHK"] = true;
                            //    dr["HOLD_ID"] = dtTmp.Rows[0]["HOLD_ID"];
                            //    dr["HOLD_GR_ID"] = dtTmp.Rows[0]["HOLD_GR_ID"];
                            //    dr["ASSY_LOTID"] = dtTmp.Rows[0]["ASSY_LOTID"];
                            //    dr["HOLD_TRGT_CODE"] = dtTmp.Rows[0]["HOLD_TRGT_CODE"];
                            //    dr["STRT_SUBLOTID"] = dtTmp.Rows[0]["STRT_SUBLOTID"];
                            //    dr["HOLD_REG_QTY"] = dtTmp.Rows[0]["HOLD_REG_QTY"];
                            //    dtInfo.Rows.Add(dr);
                            //}
                        }

                        if (dtInfo.Rows.Count > 0)
                            dtInfo = dtInfo.DefaultView.ToTable(true);

                        //Util.GridSetData(dgHold, dtInfo, FrameOperation);

                        //SearchWithProgressBarSystem(dtInfo);

                        GetCellHoldInfo(dtInfo);
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
                if (string.IsNullOrEmpty(txtNote.Text))
                {
                    //SFU4301		Hold 해제 사유를 입력하세요.	
                    Util.MessageValidation("SFU4301");
                    return;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgHold, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
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


        private void InitControl()
        {
            _DtSearchInfo = new DataTable();

            for (int i = 0; i < dgHold.Columns.Count; i++)
            {
                _DtSearchInfo.Columns.Add(dgHold.Columns[i].Name);
            }

            Util.GridSetData(dgHold, _DtSearchInfo, FrameOperation);

            _DtSearchInfo.Clear();
        }

        private void Save()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_NOTE");
                inDataTable.Columns.Add("SHOPID");
                
                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("HOLD_ID");
                inHoldTable.Columns.Add("SUBLOTID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_NOTE"] = txtNote.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int i = dgHold.TopRows.Count; i < dgHold.Rows.Count - dgHold.BottomRows.Count - dgHold.TopRows.Count; i++)
                {
                    //if (!_Util.GetDataGridCheckValue(dgHold, "CHK", i)) continue;

                    if (Util.NVC(dgHold.GetCell(i, dgHold.Columns["CHK"].Index).Value) == "0"
                        || Util.NVC(dgHold.GetCell(i, dgHold.Columns["CHK"].Index).Value).ToUpper() == "FALSE") continue;

                        newRow = inHoldTable.NewRow();
                    newRow["HOLD_ID"] = Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "HOLD_ID"));
                    inHoldTable.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_ASSY_UNHOLD_ERP", "INDATA,INHOLD", null, (result, exception) =>
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

        private void GetCellHoldInfo(DataTable dtExcel)
        {
            try
            {
                Util.gridClear(dgHold);
                if (dtExcel == null || dtExcel.Rows.Count < 1) return;

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                
                DataTable inSubLotTable = inDataSet.Tables.Add("IN_SUBLOTID");
                inSubLotTable.Columns.Add("SUBLOTID", typeof(string));
                
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dtExcel.Rows.Count; i++)
                {
                    DataRow dr = inSubLotTable.NewRow();                    
                    dr["SUBLOTID"] = dtExcel.Rows[i]["STRT_SUBLOTID"];

                    inSubLotTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_HOLD_LIST_BY_SUBLOT", "INDATA,IN_SUBLOTID", "OUTDATA", (bizResult, bizException) =>
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

                            Util.GridSetData(dgHold, dtResult, FrameOperation, true);

                            tbTotCount.Text = dtResult.Rows.Count.ToString();
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
