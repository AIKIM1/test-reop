/*************************************************************************************
 Created Date : 2022.12.06
      Creator : KIM TAEKYUN
   Decription : CPF 온도 모니터링
--------------------------------------------------------------------------------------
 [Change History]
------------------------------------------------------------------------------
     Date     |   NAME   |                  DESCRIPTION
------------------------------------------------------------------------------
  2022.12.06  DEVELOPER : Initial Created.
  2023.09.13  주훈        Chart Mouse Event 삭제 및 최대 화면으로 변경 (기구분만 표시)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Collections;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.C1Chart;
using System.IO;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Configuration;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_212 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string sLANEID = string.Empty;
        private string sEQPID = string.Empty;
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        // Load시 Column 수
        int colCnt = 0;

        double height = 0;
        private DataTable dtRslt;
        DataTable tempGrp = new DataTable();

        DataTable _CPFGrp = new DataTable();
        
        public FCS002_212()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboLaneChild = { cboEqp };
            string[] sFilterLane = { "L" };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", cbChild: cboLaneChild, sFilter: sFilterLane);
        }
        private void InitControl()
        {
            SetWorkResetTime();
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                InitCombo();
                InitControl();

                // 기본 Column 수 저장
                colCnt = dgEqpTemp.Columns.Count;

                // 2023.09.13  Chart Mouse Event 삭제 및 최대 화면으로 변경 (기구분만 표시)
                chrCPF.MouseDoubleClick -= new System.Windows.Input.MouseButtonEventHandler(this.chart_MouseDown);

                //다른 화면에서 넘어온 경우
                object[] parameters = this.FrameOperation.Parameters;
                if (parameters != null && parameters.Length >= 1)
                {
                    sLANEID = Util.NVC(parameters[0]);
                    sEQPID = Util.NVC(parameters[1]);

                    cboLane.SelectedValue = sLANEID;
                    cboEqp.SelectedValue = sEQPID;

                    GetList();
                }

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEqp.Text = string.Empty;
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboEqpParent = { cboLane };
            string[] sFilterEqp = { "L" };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "EQPIDBYLANE", cbParent: cboEqpParent, sFilter: sFilterEqp);

        }
        private void chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            C1Chart chrt = sender as C1Chart;

            Grid grd = chrt.Parent as Grid;

            if (chrt.Tag.Equals("all"))
            {
                gr0.Visibility = Visibility.Collapsed;
                
                grd.Visibility = Visibility.Visible;

                if (grdChrtMain.ActualHeight != 0)
                    grd.Height = grdChrtMain.ActualHeight;
                else
                    grd.Height = grdDataMain.ActualHeight;

                cl1.Height = grd.Height / 5 * 4;
                chrt.Tag = "one";
            }
            else
            {
                height = grdChrtMain.ActualHeight / 3;
                gr0.Height = height;
                
                gr0.Visibility = Visibility.Visible;
                
                cl1.Height = 120;
                chrt.Tag = "all";
            }
        }

        private void dgEqpTemp_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null);
            dJobDate = dJobDate.AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private void GridInit()
        {
            // GetList에 Column Add가 있기 때문에 삭제 처리
            while (dgEqpTemp.Columns.Count > colCnt)
            {
                dgEqpTemp.Columns.RemoveAt(dgEqpTemp.Columns.Count - 1);
            }

            Util.gridClear(dgEqpTemp);

            _CPFGrp.Clear();

            chrCPF.Data.Children.Clear();

            // 2023.09.13  Chart Mouse Event 삭제 및 최대 화면으로 변경 (기구분만 표시)
            chrCPF.Tag = "all";
            chart_MouseDown(chrCPF, null);
        }


        private void GetList()
        {
            try
            {
                int iCnt = 0;

                btnSearch.IsEnabled = false;
                GridInit();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 08:30:00";
                //dr["TO_TIME"] = dtpToDate.SelectedDateTime.AddDays(1).ToString("yyyy-MM-dd") + " 08:29:59";

                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");

                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TEMP_HIST_CPF_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageInfo("FM_ME_0232"); //조회된 값이 없읍니다.
                    return;
                }
                if (dtRslt.Rows.Count > 0)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgEqpTemp.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    //동적 컬럼 생성
                    foreach (DataColumn c in dtRslt.Columns)
                    {
                        if (c.ColumnName.Contains("CPF_TEMP"))
                        {
                            dgEqpTemp.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                            {
                                Name = Util.NVC(c.ColumnName),
                                Header = c.ColumnName.ToString(),
                                Binding = new Binding()
                                {
                                    Path = new PropertyPath(Util.NVC(c.ColumnName)),
                                    Mode = BindingMode.TwoWay
                                },
                                TextWrapping = TextWrapping.Wrap,
                                IsReadOnly = true,
                                Format = "#,###.#",
                                Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto)
                            });

                            iCnt++;
                        }
                    }
                }

                //Util.GridSetData(dgEqpTemp, dtRslt, this.FrameOperation, true);
                dgEqpTemp.ItemsSource = DataTableConverter.Convert(dtRslt);

                SetChartDataTable(dtRslt, iCnt);
                SetSeries();
                ModifyGraph();
                changeYScale();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnSearch.IsEnabled = true;
                HiddenLoadingIndicator();
            }
        }

        private void SetChartDataTable(DataTable dt, int TempCnt)
        {
//            if (TempCnt > 40) TempCnt = 50;

            string[] sCPF = new string[TempCnt + 1];

            sCPF[0] = "MEAS_TIME";
            for (int i = 1; i < TempCnt + 1; i++)
            {
                sCPF[i] = "CPF_TEMP" + Convert.ToString(i).PadLeft(2, '0');
            }

            _CPFGrp = dt.DefaultView.ToTable(false, sCPF);
        }

        private void SetSeries()
        {
            for (int i = 1; i < _CPFGrp.Columns.Count; i++)
            {
                C1.WPF.C1Chart.XYDataSeries x = new XYDataSeries();
                x.Label = "CPF_TEMP" + Convert.ToString(i).PadLeft(2, '0');
                x.RenderMode = RenderMode.Default;
                x.ValueBinding = new Binding("CPF_TEMP" + Convert.ToString(i).PadLeft(2, '0'));

                chrCPF.Data.Children.Add(x);
            }

            chrCPF.Data.ItemsSource = DataTableConverter.Convert(_CPFGrp);
        }

        private void ModifyGraph()
        {
            chrCPF.View.AxisX.IsTime = true;
            chrCPF.View.AxisX.AnnoFormat = "yyyy-MM-dd HH:mm:ss";
            chrCPF.View.AxisX.AnnoAngle = -90;
            chrCPF.View.AxisY.AnnoFormat = "#0.##";
        }

        private void changeYScale()
        {
            try
            {
                //if (!string.IsNullOrEmpty(txtUpperTemp.Text) && !string.IsNullOrEmpty(txtLowerTemp.Text))
                //{
                //    chrCPF.View.AxisY.Max = Convert.ToDouble(txtUpperTemp.Text);
                //    chrCPF.View.AxisY.Min = Convert.ToDouble(txtLowerTemp.Text);
                //}

                if (!string.IsNullOrEmpty(txtLowerTemp.Text))
                {
                    chrCPF.View.AxisY.Min = Convert.ToDouble(txtLowerTemp.Text);
                }
                else
                {
                    chrCPF.View.AxisY.Min = double.NaN;
                }

                if (!string.IsNullOrEmpty(txtUpperTemp.Text))
                {
                    chrCPF.View.AxisY.Max = Convert.ToDouble(txtUpperTemp.Text);
                }
                else
                {
                    chrCPF.View.AxisY.Max = double.NaN;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                chrCPF.View.AxisY.Max = double.NaN;
                chrCPF.View.AxisY.Min = double.NaN;
            }
        }


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

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCnt = 0;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 08:30:00";
                //dr["TO_TIME"] = dtpToDate.SelectedDateTime.AddDays(1).ToString("yyyy-MM-dd") + " 08:29:59";

                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");

                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TEMP_HIST_CPF_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageInfo("FM_ME_0232"); //조회된 값이 없읍니다.

                    return;
                }
                if (dtRslt.Rows.Count > 0)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgEqpTemp.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                
                }

                for (int i = 0; i < dtRslt.Columns.Count; i++)
                {
                    string sColName = ObjectDic.Instance.GetObjectName(dtRslt.Columns[i].ColumnName);
                    if (sColName.Contains("[#]")) // 다국어 처리가 되지않은 컬럼명 처리
                        sColName = sColName.Substring(3);
                    dtRslt.Columns[i].ColumnName = sColName;
                }
              
              
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnSearch.IsEnabled = true;
                HiddenLoadingIndicator();
            }



            C1DataGrid dgCellInfo = new C1DataGrid();
            dgCellInfo.ItemsSource = DataTableConverter.Convert(dtRslt);
            Export(dgCellInfo);

        }
        public void Export(C1DataGrid dataGrid, string defaultFileName = null, bool bOpen = false)
        {
            MemoryStream ms = new MemoryStream();
            dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = ExcelFileFormat.Xlsx, KeepColumnWidths = true, KeepRowHeights = true });
            ms.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(ms);
            List<int> deleteIndex = new List<int>();
            foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
            {
                if (row.Visibility == System.Windows.Visibility.Collapsed)
                {
                    deleteIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
                }
            }

            foreach (int index in (from i in deleteIndex orderby i descending select i))
            {
                if (index < book.Sheets[0].Rows.Count)
                    book.Sheets[0].Rows.RemoveAt(index);
            }
            for (int rowinx = 0; rowinx < book.Sheets[0].Rows.Count; rowinx++)
            {
                for (int colinx = 0; colinx < book.Sheets[0].Columns.Count; colinx++)
                {
                    // 2017.03.30 이슬아
                    // DB에서 Header를 조회하여 다국어처리된 값을 업무테이블 동적으로 가져오나 
                    // 다국어테이블에는 등록이 안되서 [#]이 붙는 경우가 발생하여 replace 처리하고 싶었으나, 일단 주석처리함
                    if (rowinx == 0)
                    {
                        // book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text.Replace("[#]", string.Empty), book.Sheets[0].GetCell(rowinx, colinx).Style);
                        book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text, book.Sheets[0].GetCell(rowinx, colinx).Style);
                    }

                    if (book.Sheets[0].GetCell(rowinx, colinx).Style != null)
                        book.Sheets[0].GetCell(rowinx, colinx).Style.Font = new XLFont("Arial", 12);
                }
            }

            MemoryStream editedms = new MemoryStream();
            //===================================================================================================================
            if (dataGrid.Resources.Contains("ExportRemove"))
            {
                List<int> removecol = dataGrid.Resources["ExportRemove"] as List<int>;
                for (int idx = removecol.Count; idx > 0; idx--)
                {
                    book.Sheets[0].Columns.RemoveAt(removecol[idx - 1]);
                }
            }
            //===================================================================================================================

            //   AutoSizeColumns(book.Sheets[0]);

            book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
            editedms.Seek(0, SeekOrigin.Begin);
            string tempFilekey = defaultFileName + DateTime.Now.ToString("yyyyMMddHHmmss");

            //string tempFilekey = Guid.NewGuid().ToString("N");
            new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            {
                ms.Close();
                editedms.Close();
                if (arg.Success)
                {
                    //new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
                }
            }, bOpen);
        }

        internal class StreamUploader
        {
            internal void uploadTempStream(string filekey, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler, bool bOpen = false)
            {
                int blockSize = 1024 * 1024 * 100;
                //int blockSize = 1024 * 1024 * 3;
                int readedTotal = 0;
                int partNumber = 1;
                int uploadedNumber = 0;
                int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

                try
                {
                    SaveFileDialog od = new SaveFileDialog();
                    if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                    {
                        od.InitialDirectory = @"\\Client\C$\Users";
                    }
                    od.Filter = "Excel Files (.xlsx)|*.xlsx";
                    od.FileName = filekey + "_" + partNumber.ToString() + ".xlsx";

                    if (od.ShowDialog() == true)
                    {
                        while (stream.Length > readedTotal)
                        {
                            byte[] buffer = new byte[blockSize];
                            int readed = stream.Read(buffer, 0, blockSize);
                            readedTotal += readed;

                            //FileInfo tempFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + filekey + "_" + partNumber.ToString() + ".xlsx");
                            FileInfo tempFile = new FileInfo(od.FileName);

                            using (FileStream fs = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                fs.Write(buffer, 0, readed);
                                fs.Flush();
                                fs.Close();
                            }
                            partNumber++;
                        }
                        //  WebClient client = new WebClient();
                        // uploadCompleteHandler(client, new UploadCompletedEventArgs(true, false, null, null, null, userState));
                        // if (bOpen)
                        // System.Diagnostics.Process.Start(od.FileName);
                    }
                }
                catch { }
            }
        }
    }
}
