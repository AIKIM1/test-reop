/*************************************************************************************
 Created Date : 2020.08.11
      Creator : INS 김동일K
   Decription : Cell Hold 해제(Excel) 기능 추가 : C20200711-000007
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.11  INS 김동일K : Initial Created.
  2022.11.17  INS 임근영  : HOLD 해제사유 컬럼 추가: C20221116-000234
  2023.12.12  INS 최윤호  : ROW추가, +- 추가, CELL 수정후 기존 GRID 유지 : E20231122-000992
  2024.05.16  남형희: E20240422 - 000342 HOLD 해제 사유 Validation 추가
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
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_213_RELEASE_CELL_EXCL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_213_RELEASE_CELL_EXCL : C1Window, IWorkArea
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

        public BOX001_213_RELEASE_CELL_EXCL()
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
                //if (string.IsNullOrEmpty(txtNote.Text))
                //{
                //    //SFU4301		Hold 해제 사유를 입력하세요.	
                //    Util.MessageValidation("SFU4301");
                //    return;
                //}
                //2024.05.16  남형희: E20240422 - 000342 HOLD 해제 사유 Validation 추가
                if (string.IsNullOrEmpty(txtNote.Text) || txtNote.Text.Trim().Length <= 3)
                {
                    // HOLD 해제사유 예시	
                    Util.MessageValidation("SFU10014");
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

            xProgress.Visibility = Visibility.Collapsed;
            xTextBlock.Visibility = Visibility.Collapsed;

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
                inDataTable.Columns.Add("PACK_RELEASE_FLAG");

                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("HOLD_ID");
                inHoldTable.Columns.Add("SUBLOTID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_NOTE"] = txtNote.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PACK_RELEASE_FLAG"] = chkInputRelease.IsChecked == true ? "Y" : "N";
                inDataTable.Rows.Add(newRow);
                newRow = null;

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5")) // 소형 파우치
                {
                    string sXML = string.Empty;

                    for (int row = dgHold.TopRows.Count; row < dgHold.Rows.Count - dgHold.BottomRows.Count - dgHold.TopRows.Count; row++)
                    {
                        if (row == 0 || row % 500 == 0)
                        {
                            sXML = "<root>";
                        }

                        sXML += "<DT><L>" + Util.NVC(DataTableConverter.GetValue(dgHold.Rows[row].DataItem, "HOLD_ID")) + "</L></DT>";

                        if ((row + 1) % 500 == 0 || row + 1 == (dgHold.Rows.Count - dgHold.BottomRows.Count - dgHold.TopRows.Count))
                        {
                            sXML += "</root>";

                            newRow = inHoldTable.NewRow();
                            newRow["HOLD_ID"] = sXML;
                            inHoldTable.Rows.Add(newRow);
                        }
                    }
                }
                else // 자동차
                {
                    for (int i = dgHold.TopRows.Count; i < dgHold.Rows.Count - dgHold.BottomRows.Count - dgHold.TopRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgHold, "CHK", i)) continue;

                        newRow = inHoldTable.NewRow();
                        newRow["HOLD_ID"] = Util.NVC(DataTableConverter.GetValue(dgHold.Rows[i].DataItem, "HOLD_ID"));
                        inHoldTable.Rows.Add(newRow);
                    }
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

        //private DataTable Search(string sSubLotID)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.Columns.Add("LANGID");
        //        RQSTDT.Columns.Add("HOLD_FLAG");
        //        RQSTDT.Columns.Add("HOLD_TYPE_CODE");
        //        RQSTDT.Columns.Add("HOLD_TRGT_CODE");
        //        RQSTDT.Columns.Add("SUBLOTID");

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["HOLD_FLAG"] = "Y";
        //        dr["HOLD_TYPE_CODE"] = "SHIP_HOLD";
        //        dr["HOLD_TRGT_CODE"] = "SUBLOT";
        //        dr["SUBLOTID"] = sSubLotID;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ASSY_LOT_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
        //        //if (!dtResult.Columns.Contains("CHK"))
        //        //    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

        //        //Util.GridSetData(dgHold, dtResult, FrameOperation, true);

        //        return dtResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return null;
        //    }
        //    finally
        //    {
        //        loadingIndicator.Visibility = Visibility.Collapsed;
        //    }

        //}

        private async void SearchWithProgressBarSystem(DataTable dtExcel)
        {
            xProgress.Visibility = Visibility.Visible;
            xTextBlock.Visibility = Visibility.Visible;
            xProgress.Maximum = dtExcel.Rows.Count;
            xProgress.Value = 0;

            await System.Threading.Tasks.Task.Run(() =>
            {
                loadingIndicator.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate () { loadingIndicator.Visibility = Visibility.Visible; }));

                const string bizRuleName = "BR_PRD_GET_ASSY_HOLD_HIST";
                                
                for (var i = 0; i <= dtExcel.Rows.Count - 1; i++)
                {
                    Application.Current.Dispatcher.Invoke(new Action(delegate {
                        
                        //DataRowView drv = dgHold.Rows[i].DataItem as DataRowView;
                        //if (drv != null)
                        //{
                            DataTable inDataTable = new DataTable("RQSTDT");
                            inDataTable.Columns.Add("LANGID", typeof(string));
                            inDataTable.Columns.Add("HOLD_FLAG", typeof(string));
                            inDataTable.Columns.Add("HOLD_TYPE_CODE", typeof(string));
                            inDataTable.Columns.Add("HOLD_TRGT_CODE", typeof(string));
                            inDataTable.Columns.Add("SUBLOTID", typeof(string));
                            inDataTable.Columns.Add("SEARCH_GUBUN", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["LANGID"] = LoginInfo.LANGID;
                            dr["HOLD_FLAG"] = "Y";
                            dr["HOLD_TYPE_CODE"] = "SHIP_HOLD";
                            dr["HOLD_TRGT_CODE"] = "SUBLOT";
                            dr["SUBLOTID"] = dtExcel.Rows[i]["STRT_SUBLOTID"];
                            dr["SEARCH_GUBUN"] = "G";
                        
                            inDataTable.Rows.Add(dr);
                            
                            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                            {
                                if (bizException != null)
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    Util.MessageException(bizException);
                                    return;
                                }
                                UpdateProgressBar(i, bizResult, dtExcel.Rows.Count);
                            });
                        //}
                    }),
                        System.Windows.Threading.DispatcherPriority.Input
                    );
                }
                                
            });
            loadingIndicator.Visibility = Visibility.Collapsed;
        }


        private void UpdateProgressBar(int value, DataTable dtRslt, int iTotCnt)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(delegate {

                    if (dtRslt?.Rows?.Count > 0)
                    {
                        for (int i = 0; i < dtRslt.Rows.Count; i++)
                        {
                            DataRow dr = _DtSearchInfo.NewRow();
                            dr["CHK"] = true;
                            dr["HOLD_ID"] = dtRslt.Rows[i]["HOLD_ID"];
                            dr["HOLD_GR_ID"] = dtRslt.Rows[i]["HOLD_GR_ID"];
                            dr["ASSY_LOTID"] = dtRslt.Rows[i]["ASSY_LOTID"];
                            dr["HOLD_TRGT_CODE"] = dtRslt.Rows[i]["HOLD_TRGT_CODE"];
                            dr["STRT_SUBLOTID"] = dtRslt.Rows[i]["STRT_SUBLOTID"];
                            dr["HOLD_REG_QTY"] = dtRslt.Rows[i]["HOLD_REG_QTY"];
                            dr["HOLD_NOTE"] = dtRslt.Rows[i]["HOLD_NOTE"];
                            _DtSearchInfo.Rows.Add(dr);
                        }
                    }

                    xTextBlock.Text = xProgress.Value + "/" + iTotCnt;
                    xProgress.Value += 1;
                    if (xProgress.Value >= xProgress.Maximum)
                    {
                        xTextBlock.Visibility = Visibility.Collapsed;
                        xProgress.Visibility = Visibility.Collapsed;

                        Util.GridSetData(dgHold, _DtSearchInfo, FrameOperation, false);

                        tbTotCount.Text = _DtSearchInfo.Rows.Count.ToString();
                    }
                }),
                System.Windows.Threading.DispatcherPriority.Input
            );
        }

        private void GetCellHoldInfo(DataTable dtExcel)
        {
            try
            {
                //Util.gridClear(dgHold);     csr : E20231122-000992 그리드 데이타 유지
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


        /// <summary>
        /// csr : E20231122-000992 , 그리드 row 초기화
        /// </summary>
        /// <param name="txtRowCntInsertCell"></param>
  
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            //dgHold.ClearRows();
            int iRowcount = Convert.ToInt32(txtRowCntInsertCell.Text);

            try
            {
                for (int i = 0; i < iRowcount; i++)
                {
                    DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
                    DataRow dr = dt.NewRow();

                    dt.Rows.Add(dr);

                    Util.GridSetData(dgHold, dt, FrameOperation);
                    dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
           
        }

        /// <summary>
        /// csr : E20231122-000992 ,HOLD 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgHold.ItemsSource);
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            Util.GridSetData(dgHold, dt, FrameOperation);
            dgHold.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        /// <summary>
        /// csr : E20231122-000992, HOLD 리스트 제외
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

        // <summary>
        /// csr : E20231122-000992 , 그리드 CELLID 편집 기능 추가, 수정 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgHold_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.ActualFilterMemberPath != "STRT_SUBLOTID")
                {
                    return;
                };

                DataSet inDataSet = new DataSet();

                //input table_1
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(newRow);

                //input table_2
                DataTable inSubLotTable = inDataSet.Tables.Add("IN_SUBLOTID");
                inSubLotTable.Columns.Add("SUBLOTID", typeof(string));

                //input table 데이타 add
                DataTable Grid_Dt = DataTableConverter.Convert(dgHold.ItemsSource);
                DataRow dr = inSubLotTable.NewRow();
                dr["SUBLOTID"] = e.Cell.Value.ToString();
                inSubLotTable.Rows.Add(dr);
                

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_ASSY_HOLD_LIST_BY_SUBLOT", "INDATA,IN_SUBLOTID", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if(e.Cell.Row.Index < 0)    //cell 편집중 +- row추가 등 이벤트 발생시 오류 발생방지.
                        {
                            return;
                        }

                        if (bizResult.Tables[0].Rows.Count > 0)
                        {
                            DataTable dtResult = bizResult.Tables["OUTDATA"];

                            // Util.GridSetData(dgHold, dtResult, FrameOperation, true);

                            //e.Cell.Row.Index
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "chk", dtResult.Rows[0]["chk"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_GR_ID", dtResult.Rows[0]["HOLD_GR_ID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", dtResult.Rows[0]["ASSY_LOTID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_TRGT_CODE", dtResult.Rows[0]["HOLD_TRGT_CODE"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "STRT_SUBLOTID", dtResult.Rows[0]["STRT_SUBLOTID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_ID", dtResult.Rows[0]["HOLD_ID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_ID", dtResult.Rows[0]["HOLD_ID"].ToString());
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_NOTE", dtResult.Rows[0]["HOLD_NOTE"].ToString());

                            if (dtResult.Rows[0]["HOLD_REG_QTY"].ToString() != "")
                            {
                                DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_REG_QTY", dtResult.Rows[0]["HOLD_REG_QTY"].ToString());
                            }
                            // DataTableConverter.SetValue(dgHold.Rows[i].DataItem, "CHK", false);
                            // tbTotCount.Text = dtResult.Rows.Count.ToString();
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "chk", false);
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_GR_ID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "ASSY_LOTID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_TRGT_CODE", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "STRT_SUBLOTID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_ID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_ID", "");
                            DataTableConverter.SetValue(dgHold.Rows[e.Cell.Row.Index].DataItem, "HOLD_NOTE", "");
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        tbTotCount.Text = chk_Chkcnt().ToString();   //row 카운트
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

        // <summary>
        /// csr : E20231122-000992 , 체크된 row 카운트
        /// </summary>
        private int chk_Chkcnt()
        {
            int cnt = 0;
            try
            {
                dgHold.EndEdit();
               
                for (int i = 1; i < dgHold.Rows.Count; i++)
                {
                    object isChecked = DataTableConverter.GetValue(dgHold.Rows[i-1].DataItem, "CHK") ?? "";
                    if (isChecked.ToString() == "True")
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
            catch (Exception e)
            {
                return cnt;
            }         
        }



    }  
}
