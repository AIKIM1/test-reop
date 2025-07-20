/*************************************************************************************
 Created Date : 2020.11.17
      Creator : DEVELOPER
   Decription : Tray 예상시간
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.17  DEVELOPER : Initial Created.
 2022.07.04 KDH : 검색조건(조회기간) 추가

**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

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
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_011 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        Dictionary<int, int> noEnd = new Dictionary<int, int>();
        Dictionary<int, int> noStart = new Dictionary<int, int>();

        public FCS002_011()
        {
            InitializeComponent();
            InitCombo();

            //20220704_검색조건(조회기간) 추가 START
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpFromTime.DateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now.AddDays(1);
            dtpToTime.DateTime = DateTime.Now.AddDays(1);

            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
            dtpToDate.IsEnabled = false;
            dtpToTime.IsEnabled = false;
            //20220704_검색조건(조회기간) 추가 END
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ROUTE", cbParent: cboRouteParent);
        }
        #endregion

        #region Method
        private void SetGridHeaderMulti(DataTable dt, C1DataGrid dg)
        {
            for (int rowidx = 0; rowidx < dt.Rows.Count; rowidx++)
            {
                C1.WPF.DataGrid.DataGridTemplateColumn dc = new C1.WPF.DataGrid.DataGridTemplateColumn();

                string sTopHeader = dt.Rows[rowidx]["PROCNAME"].ToString();

                //DataGridTemplateColumn.Header 처리를 위한 GRID
                Grid g = new Grid();
                g.Width = double.NaN;

                //Row 생성(두줄)
                for (int i = 0; i < 2; i++)
                {
                    RowDefinition r = new RowDefinition();
                    g.RowDefinitions.Add(r);
                }
                //Row Count만큼 컬럼 생성
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ColumnDefinition c = new ColumnDefinition();
                    g.ColumnDefinitions.Add(c);
                }
                //DataGridTemplateColumn.CellTemplate의 DataTemplate 처리를 위한 GRID
                Grid g2 = new Grid();
                g2.Width = double.NaN;
                g2.HorizontalAlignment = HorizontalAlignment.Center;

                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(Grid));

                //첫번째 Row Header 생성
                TextBlock tbh = new TextBlock();
                tbh.Text = sTopHeader;

                Grid.SetRow(tbh, 0);
                Grid.SetColumn(tbh, 0);
                g.Children.Add(tbh);

                tbh.TextAlignment = TextAlignment.Center;
                tbh.VerticalAlignment = VerticalAlignment.Center;

                g.HorizontalAlignment = HorizontalAlignment.Center;

                //두번째 Row의 Header 처리

                //DataGridTemplateColumn.Header 처리
                string header = dt.Rows[rowidx]["END_TIME"].ToString() + " (min)";
                TextBlock tb = new TextBlock();
                tb.Text = header;
                tb.TextAlignment = TextAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.FontSize = 11;

                Grid.SetRow(tb, 1);
                Grid.SetColumn(tb, 0);

                g.Children.Add(tb);
                //  g.VerticalAlignment = VerticalAlignment.Center;
                //DataGridTemplateColumn.CellTemplate의 DataTemplate 처리
                FrameworkElementFactory columnDefinitionFactory = new FrameworkElementFactory(typeof(ColumnDefinition));
                factory.AppendChild(columnDefinitionFactory);

                factory.SetValue(Grid.WidthProperty, double.NaN);

                FrameworkElementFactory factorytb = new FrameworkElementFactory(typeof(TextBlock));
                factorytb.SetValue(Grid.ColumnProperty, rowidx);
                factorytb.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                factorytb.SetValue(TextBlock.PaddingProperty, new Thickness(5, 1, 5, 1));

                //DataBinding 처리
                Binding b = new Binding(dt.Rows[rowidx]["PROCID"].ToString());
                b.Path = new PropertyPath(dt.Rows[rowidx]["PROCID"].ToString());
                b.Mode = BindingMode.TwoWay;
                factorytb.SetBinding(TextBlock.TextProperty, b);
                factory.AppendChild(factorytb);

                DataTemplate template = new DataTemplate();
                template.VisualTree = factory;
                dc.CellTemplate = template;
                template.Seal();

                dc.Header = g;
                dc.HorizontalAlignment = HorizontalAlignment.Center;
                dc.VerticalAlignment = VerticalAlignment.Center;

                dg.Columns.Add(dc);
            }
        }

        private void GetList()
        {
            try
            {
                dgTrayFlow.ItemsSource = null;
                dgTrayFlow.Columns.Clear();
                noEnd.Clear();
                noStart.Clear();
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string)); //ROUTE_ID

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = Util.GetCondition(cboRoute); //ROUTE_ID

                dtRqst.Rows.Add(dr);

                DataTable dtHeader = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_FLOW_TIME_HEADER_MB", "RQSTDT", "RSLTDT", dtRqst);

                DataTable dtRqst1 = new DataTable();
                dtRqst1.TableName = "RQSTDT";
                dtRqst1.Columns.Add("AREAID", typeof(string));
                dtRqst1.Columns.Add("EQSGID", typeof(string)); //LINE_ID
                dtRqst1.Columns.Add("ROUTID", typeof(string)); //ROUTE_ID
                dtRqst1.Columns.Add("CSTID", typeof(string)); //TRAY_ID

                DataRow dr1 = dtRqst1.NewRow();
                dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr1["EQSGID"] = Util.GetCondition(cboLine); //LINE_ID
                dr1["ROUTID"] = Util.GetCondition(cboRoute); //ROUTE_ID
                if (!string.IsNullOrEmpty(txtTrayId.Text)) dr1["CSTID"] = txtTrayId.Text; //TRAY_ID

                dtRqst1.Rows.Add(dr1);

                DataTable dtTray = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_FLOW_TIME_MB", "RQSTDT", "RSLTDT", dtRqst1);

                DataTable dtCellInfo = new DataTable();
                //Column 생성 Start
                SetGridHeader(dtHeader, dgTrayFlow); //single Row
               
                DataColumn dcCellInfo = new DataColumn("MDLLOT_ID");
                dtCellInfo.Columns.Add(dcCellInfo);
                dcCellInfo = new DataColumn("MODEL_NAME");
                dtCellInfo.Columns.Add(dcCellInfo);
                dcCellInfo = new DataColumn("PROD_LOTID");
                dtCellInfo.Columns.Add(dcCellInfo);
                dcCellInfo = new DataColumn("CSTID");
                dtCellInfo.Columns.Add(dcCellInfo);

              //  SetGridHeaderMulti(dtHeader, dgTrayFlow);
                for (int j = 0; j < dtHeader.Rows.Count; j++)
                {
                    DataColumn dcApd = new DataColumn(dtHeader.Rows[j]["PROCID"].ToString());

                    dtCellInfo.Columns.Add(dcApd);
                }
                //Column 생성 End


                int iRow = -1;
                int iCol = -1;
                int iActRow = 0;
                int iActCol = 0;
                DateTime dtCal = new DateTime();


                foreach (DataRow drTray in dtTray.Rows)
                {
                    iRow = -1;
                    iCol = -1;

                    for (int i = 0; i < dtCellInfo.Rows.Count; i++)
                    {
                        if (Util.NVC(dtCellInfo.Rows[i]["CSTID"]).Equals(drTray["CSTID"].ToString()))
                        {
                            iRow = i;
                        }
                    }
                    if (iRow == -1)
                    {
                        DataRow newRow = dtCellInfo.NewRow();
                        newRow["MDLLOT_ID"] = drTray["MDLLOT_ID"].ToString();
                        newRow["MODEL_NAME"] = drTray["MODEL_NAME"].ToString();
                        newRow["PROD_LOTID"] = drTray["PROD_LOTID"].ToString();
                        newRow["CSTID"] = drTray["CSTID"].ToString();
                        dtCellInfo.Rows.Add(newRow);
                        iActRow = dtCellInfo.Rows.Count - 1;
                    }
                    else
                    {
                        iActRow = iRow;
                    }
                    for (int idx = 0; idx < dtCellInfo.Columns.Count; idx++)
                    {
                        if (dtCellInfo.Columns[idx].ColumnName.Equals(drTray["PROCID"].ToString()))
                        {
                            iActCol = idx;
                            break;
                        }
                    }
                    DataTable dtSource = new DataTable();

                    if (!string.IsNullOrEmpty(drTray["WIPDTTM_ST"].ToString()) && string.IsNullOrEmpty(drTray["WIPDTTM_ED"].ToString()))
                    {
                        dtCal = Convert.ToDateTime(drTray["WIPDTTM_ST"].ToString());

                        dtCal = dtCal.AddMinutes(Convert.ToInt32(dtHeader.Rows[iActCol - 4]["END_TIME"].ToString()));

                        dtCellInfo.Rows[iActRow][drTray["PROCID"].ToString()] = dtCal;

                        if (!noEnd.ContainsKey(iActRow + 2))
                        {
                            noEnd.Add(iActRow + 2, iActCol);
                        }
                        //  dgTrayFlow[iActRow,iActCol].Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        dtCellInfo.Rows[iActRow][iActCol] = drTray["WIPDTTM_ED"];
                    }

                }
                //빈칸 처리
                for (int i = 0; i < dtCellInfo.Rows.Count; i++)
                {
                    int startidx = -1;
                    for (int j = dtCellInfo.Columns.Count - 1; j > 0; j--)
                    {
                        if (string.IsNullOrEmpty(dtCellInfo.Rows[i][j].ToString()) && !string.IsNullOrEmpty(dtCellInfo.Rows[i][j - 1].ToString()))
                        {
                            startidx = j;
                            break;
                        }
                    }
                    if (startidx <= 4) continue; //값이 다 있는 Case
                    for (int j = startidx; j < dtCellInfo.Columns.Count; j++)
                    {
                        dtCal = Convert.ToDateTime(dtCellInfo.Rows[i][j - 1].ToString());
                        dtCal = dtCal.AddMinutes(Convert.ToInt32(dtHeader.Rows[j - 4]["END_TIME"].ToString()));
                        dtCellInfo.Rows[i][j] = dtCal;
                        if (!noStart.ContainsKey(i + 2)) noStart.Add(i + 2, j);
                    }
                    //fpsTrayFlow.ActiveSheet.Cells[i, j].BackColor = Color.Gray;
                }

                //20220704_검색조건(조회기간) 추가 START
                if (chkDateTime.IsChecked.Equals(true))
                {
                    DateTime dtSTDate = new DateTime();
                    DateTime dtEDDate = new DateTime();
                    DateTime dtCellDate = new DateTime();

                    dtSTDate = Convert.ToDateTime(dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00"));
                    dtEDDate = Convert.ToDateTime(dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00"));

                    DataTable dtTemp = new DataTable();
                    dtTemp.TableName = "RQSTDT";
                    for (int idx = 0; idx < dtCellInfo.Columns.Count; idx++)
                    {
                        dtTemp.Columns.Add(Util.NVC(dtCellInfo.Columns[idx].ColumnName), typeof(string));
                    }
                    dtTemp.Columns.Add("TYPE", typeof(string));

                    foreach (DataRow drCellInfo in dtCellInfo.Rows)
                    {
                        bool bType = false;
                        DataRow newRow = dtTemp.NewRow();
                        for (int idx = 0; idx < dtCellInfo.Columns.Count; idx++)
                        {
                            if (!Util.NVC(dtCellInfo.Columns[idx].ColumnName).Equals("TYPE"))
                            {
                                if (!Util.NVC(dtCellInfo.Columns[idx].ColumnName).Equals("MDLLOT_ID") & !Util.NVC(dtCellInfo.Columns[idx].ColumnName).Equals("MODEL_NAME")
                                    & !Util.NVC(dtCellInfo.Columns[idx].ColumnName).Equals("PROD_LOTID") & !Util.NVC(dtCellInfo.Columns[idx].ColumnName).Equals("CSTID"))
                                {
                                    if (!string.IsNullOrEmpty(Util.NVC(drCellInfo[Util.NVC(dtCellInfo.Columns[idx].ColumnName)])))
                                    {
                                        string sVaule = Util.NVC(drCellInfo[Util.NVC(dtCellInfo.Columns[idx].ColumnName)]);
                                        //dtCellDate = DateTime.ParseExact(sVaule, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                                        dtCellDate = DateTime.Parse(sVaule);
                                        newRow[Util.NVC(dtCellInfo.Columns[idx].ColumnName)] = dtCellDate.ToString("yyyy-MM-dd HH:mm:ss");

                                        if (dtCellDate >= dtSTDate && dtCellDate <= dtEDDate)
                                        {
                                            bType = true;
                                            newRow[Util.NVC(dtCellInfo.Columns[idx].ColumnName)] = dtCellDate.ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                    }
                                }
                                else
                                {
                                    newRow[Util.NVC(dtCellInfo.Columns[idx].ColumnName)] = drCellInfo[Util.NVC(dtCellInfo.Columns[idx].ColumnName)].ToString();
                                }
                            }
                        }

                        if (bType)
                        {
                            newRow["TYPE"] = "Y";
                        }

                        dtTemp.Rows.Add(newRow);
                    }

                    // 조회기간에 속한 Tray Count 구하기 (각 공정별)
                    if (dtTemp.Rows.Count > 0)
                    {
                        DataRow newRow = dtTemp.NewRow();
                        for (int ColCnt = 0; ColCnt < dtTemp.Columns.Count; ColCnt++)
                        {
                            if (!Util.NVC(dtTemp.Columns[ColCnt].ColumnName).Equals("MDLLOT_ID") & !Util.NVC(dtTemp.Columns[ColCnt].ColumnName).Equals("MODEL_NAME")
                                & !Util.NVC(dtTemp.Columns[ColCnt].ColumnName).Equals("PROD_LOTID") & !Util.NVC(dtTemp.Columns[ColCnt].ColumnName).Equals("CSTID")
                                & !Util.NVC(dtTemp.Columns[ColCnt].ColumnName).Equals("TYPE"))
                            {
                                string sColName = Util.NVC(dtCellInfo.Columns[ColCnt].ColumnName);
                                int iSum = 0;

                                foreach (DataRow drTempInfo in dtTemp.Rows)
                                {
                                    if (!string.IsNullOrEmpty(Util.NVC(drTempInfo[Util.NVC(dtTemp.Columns[ColCnt].ColumnName)])))
                                    {
                                        string sVaule = Util.NVC(drTempInfo[Util.NVC(dtTemp.Columns[ColCnt].ColumnName)]);
                                        dtCellDate = DateTime.Parse(sVaule);

                                        if (dtCellDate >= dtSTDate && dtCellDate <= dtEDDate)
                                        {
                                            iSum++;
                                        }
                                    }
                                }

                                newRow[Util.NVC(dtCellInfo.Columns[ColCnt].ColumnName)] = iSum.ToString();
                            }
                        }
                        newRow["TYPE"] = "Y";
                        dtTemp.Rows.Add(newRow);

                        dtCellInfo = dtTemp.Select("TYPE = 'Y' ").CopyToDataTable();
                    }
                    else
                    {
                        dtCellInfo = null;
                    }
                }
                //20220704_검색조건(조회기간) 추가 END

                //dgTrayFlow.ItemsSource = DataTableConverter.Convert(dtCellInfo);
                Util.GridSetData(dgTrayFlow, dtCellInfo, this.FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #region Header 생성
        private void SetGridHeader(DataTable dt, C1DataGrid dg)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "MODEL_ID",
                Binding = new Binding() { Path = new PropertyPath("MDLLOT_ID"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "MODEL_NAME",
                Binding = new Binding() { Path = new PropertyPath("MODEL_NAME"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "ASSEMBLY_LOT_ID",
                Binding = new Binding() { Path = new PropertyPath("PROD_LOTID"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TRAY_ID",
                Binding = new Binding() { Path = new PropertyPath("CSTID"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string sPath = dt.Rows[i]["PROCID"].ToString();
                dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                {
                    Binding = new Binding() { Path = new PropertyPath(sPath), Mode = BindingMode.OneWay },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                });
                dg.Columns[i + 4].Header = dt.Rows[i]["PROCNAME"].ToString() + " ("
                    + dt.Rows[i]["END_TIME"].ToString() + " min)";
            }
        }
        #endregion

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        private void dgTrayFlow_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //20220704_검색조건(조회기간) 추가 START
                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////
                //20220704_검색조건(조회기간) 추가 END

                if (!string.IsNullOrEmpty(e.Cell.Column.Name) && e.Cell.Column.Name.Equals("CSTID")) e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                if (noEnd == null && noStart == null) return;

                //공정 종료시간 계산되었을 경우
                if (noEnd != null && noEnd.ContainsKey(e.Cell.Row.Index))
                {
                    if (e.Cell.Column.Index >= noEnd[e.Cell.Row.Index]) e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                }
                if (noStart != null && noStart.ContainsKey(e.Cell.Row.Index))
                {
                    if (e.Cell.Column.Index >= noStart[e.Cell.Row.Index]) e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                }

                //20220704_검색조건(조회기간) 추가 START
                if (chkDateTime.IsChecked.Equals(true))
                {
                    DateTime dtSTDate = Convert.ToDateTime(dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00"));
                    DateTime dtEDDate = Convert.ToDateTime(dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00"));

                    if (e.Cell.Row.Index < dgTrayFlow.Rows.Count - 1)
                    {
                        if (!e.Cell.Column.Name.Equals("MDLLOT_ID") & !e.Cell.Column.Name.Equals("MODEL_NAME") & !e.Cell.Column.Name.Equals("PROD_LOTID") & !e.Cell.Column.Name.Equals("CSTID"))
                        {
                            if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("")))
                            {
                                DateTime dtCellDate = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString());

                                if (dtCellDate >= dtSTDate && dtCellDate <= dtEDDate)
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                }
                //20220704_검색조건(조회기간) 추가 END

            }));

        }

        #endregion

        private void dgTrayFlow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;


            if (!string.IsNullOrEmpty(cell.Value.ToString()) && datagrid.CurrentColumn.Name == "CSTID")
            {
                FCS002_021 wndTRAY = new FCS002_021();
                wndTRAY.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayFlow.CurrentRow.DataItem, "CSTID")); //Tray ID
                                                                                                                //  Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayFlow.CurrentRow.DataItem, "LOTID")); //Tray No
                this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
            }
        }

        //20220704_검색조건(조회기간) 추가 START
        private void dgTrayFlow_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        //20220704_검색조건(조회기간) 추가 END

        //20220704_검색조건(조회기간) 추가 START
        private void chkDateTime_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
            dtpToDate.IsEnabled = true;
            dtpToTime.IsEnabled = true;
        }
        //20220704_검색조건(조회기간) 추가 END

        //20220704_검색조건(조회기간) 추가 START
        private void chkDateTime_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
            dtpToDate.IsEnabled = false;
            dtpToTime.IsEnabled = false;
        }
        //20220704_검색조건(조회기간) 추가 END

    }
}
