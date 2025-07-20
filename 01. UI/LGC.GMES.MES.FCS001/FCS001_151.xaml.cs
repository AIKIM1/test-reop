/*************************************************************************************
 Created Date : 2023.02.24
      Creator : 홍석원
   Decription : Cell List 정보조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.03.09  홍석원 : Initial Created.
  2024.02.19  조영대 : 초기화 속도 수정 및 조회 방식 500 건씩 조회 변경
**************************************************************************************/
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_151 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        
        int addRows = 0;

        Style styleFgRed;

        public FCS001_151()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            styleFgRed = new Style(typeof(DataGridColumnHeaderPresenter));
            styleFgRed.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(238, 238, 238)) });
            styleFgRed.Setters.Add(new Setter { Property = ForegroundProperty, Value = new SolidColorBrush(Colors.Red) });
            styleFgRed.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = System.Windows.HorizontalAlignment.Stretch });
            styleFgRed.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = System.Windows.HorizontalAlignment.Center });
            styleFgRed.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });

            dgCellList.Columns["SUBLOTID"].HeaderStyle = styleFgRed;

            this.Loaded -= UserControl_Loaded;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellList);
            txtCellCntSum.Clear();
            
            DataGridRowAdd(dgCellList);
            dgCellList.Columns["SUBLOTID"].IsReadOnly = false;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {

            if (LoadExcel()) GetList();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            xProgress.Percent = 0;
            xProgress.ProgressText = "0 / 0";
            xProgress.Visibility = Visibility.Visible;
            xProgress.InvalidateVisual();

            GetList();
        }
        #endregion

        #region Method
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                int rowCount = int.Parse(txtRowCnt.Text);

                if (Math.Abs(rowCount) > 0)
                {
                    addRows = rowCount;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < addRows; i++)
                    {                        
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                    }
                }

                dg.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetList()
        {
            try
            {
                List<string> subLotIdList = new List<string>();

                for (int iRow = 0; iRow < dgCellList.Rows.Count; iRow++)
                {
                    string sublot = DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID").Nvc();
                    if (string.IsNullOrEmpty(sublot)) continue;
                                        
                    if (!subLotIdList.Contains(sublot)) subLotIdList.Add(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID").Nvc());
                }

                if (subLotIdList.Count == 0) return;

                btnRefresh.IsEnabled = true;
                btnExcel.IsEnabled = true;
                btnSearch.IsEnabled = true;

                xProgress.Percent = 0;
                xProgress.ProgressText = "0 / " + subLotIdList.Count;
                xProgress.Visibility = Visibility.Visible;

                // Background 실행
                xProgress.RunWorker(subLotIdList);
                    
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;
            }
        }

        private bool LoadExcel()
        {
            DataTable dtInfo = DataTableConverter.Convert(dgCellList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (*.xls; *.xlsx)|*.xls; *.xlsx";

            if (fd.ShowDialog() == true)
            {
                xProgress.Percent = 0;
                xProgress.ProgressText = "0 / 0";
                xProgress.Visibility = Visibility.Visible;
                xProgress.InvalidateVisual();

                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null) return true;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dgCellList, dataTable, FrameOperation);
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            List<string> sublotList = e.Arguments as List<string>;
            DataTable dtMergeResult = null;

            int workCount = 0;
            int totalCount = sublotList.Count;

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("SUBLOTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SUBLOTID"] = string.Empty;
            dtRqst.Rows.Add(dr);

            int groupCount = 0;
            string sublotString = string.Empty;
            for (int inx = 0; inx < totalCount; inx++)
            {
                sublotString += sublotList[inx] + ",";
                
                groupCount++;
                workCount++;

                if (groupCount >= 500 || inx >= totalCount - 1)
                {                    
                    dr["SUBLOTID"] = sublotString;

                    string progressText = workCount.Nvc() + " / " + totalCount.Nvc();
                    e.Worker.ReportProgress(Convert.ToInt16((double)workCount / (double)totalCount * 100), progressText);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_DEFECT_INFO_LIST", "RQSTDT", "RSLTDT", dtRqst);
                    if (dtMergeResult == null)
                    {
                        dtMergeResult = dtRslt.Copy();
                    }
                    else
                    {
                        dtMergeResult.Merge(dtRslt, true, MissingSchemaAction.Ignore);
                    }

                    groupCount = 0;
                    sublotString = string.Empty;
                }
            }

            return dtMergeResult;
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                string progressText = e.Arguments as string;

                xProgress.Percent = percent;
                xProgress.ProgressText = progressText;
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                DataTable dtResult = e.Result as DataTable;
                if (dtResult != null)
                {
                    dgCellList.SetItemsSource(dtResult, FrameOperation, true);
                }

                txtCellCntSum.Text = Util.NVC(dtResult.Rows.Count);

                dgCellList.Columns["SUBLOTID"].IsReadOnly = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                xProgress.Visibility = Visibility.Collapsed;

                btnRefresh.IsEnabled = true;
                btnExcel.IsEnabled = true;
                btnSearch.IsEnabled = true;
            }
        }
    }
}
