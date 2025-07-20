/*******************************************************************************************************************
 Created Date : 2023.10.16
      Creator : 이정미
   Decription : PKG LOT별 HOLD 현황 조회
--------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.10.16  NAME : Initial Created

********************************************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using LGC.GMES.MES.ControlsLibrary;
using System.Drawing;
using System.Text;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_132 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private string EQSGID = string.Empty;
        private string EQSGNAME = string.Empty;
        private string PRODID = string.Empty;
        private string WRK_DATE = string.Empty;
        private string HOLD_DEPT_COLOR = string.Empty;
        private string HOLD_DEPT = string.Empty;
         
        DataTable dtTemp = new DataTable();
        DataTable dtPlanTemp = new DataTable();
        DataTable dtHoldTemp = new DataTable();

        public FCS001_132()
        {
            InitializeComponent();

            InitControl();
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
            //Combo Setting
            InitCombo();

            //Spread Setting
            InitializeDataGrid();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, sCase: "PLANT");

        }

        private void InitializeDataGrid()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
       
        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.ParseExact("20231028", "yyyyMMdd", null).AddDays(-20); //임시 테스트 중
                                                                                                           // DateTime.ParseExact("20230605", "yyyyMMdd", null).AddDays(-20); //임시 테스트 중
                                                                                                           //DateTime.Now.AddDays(-20);
            dtpToDate.SelectedDateTime = DateTime.ParseExact("20231028", "yyyyMMdd", null).AddDays(10); //임시 테스트 중
                                                                                                         //DateTime.ParseExact("20230605", "yyyyMMdd", null).AddDays(10);  //임시 테스트 중
                                                                                                         //DateTime.Now.AddDays(10);
        }

        #endregion
 
        private void init_tplnsobj()
        {
            try
            {
                dgHoldList.ItemsSource = null;
                dgHoldList.Columns.Clear();
                dgHoldList.Refresh();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PKG_LOT_HOLD_MODEL_BY_LINE_UI", "RQSTDT", "RSLTDT", dtRqst);

                //DataTable 정의
                DataTable Ddt = new DataTable();
                DataRow DrowHeader = Ddt.NewRow();

                int ColumnWith = 0;

                for(int i = 0; i < 34; i ++)
                {
                    switch(i.ToString())
                    {
                        case "0":
                            ColumnWith = 200;
                            break;
                        case "1":
                            ColumnWith = 200;
                            break;
                        case "2":
                            ColumnWith = 80;
                            break;
                        default:
                            ColumnWith = 52;
                            break;
                    }

                    //Column 생성
                    SetGridHeaderSingle((i + 1).ToString(), dgHoldList, ColumnWith);
                    Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for(int i = 0; i <= dtRslt.Rows.Count; i ++)
                {
                    DataRow Drow = Ddt.NewRow();
                    Ddt.Rows.Add(Drow);
                }

                //From Date
                DateTime dtFrom = dtpFromDate.SelectedDateTime;
                string fromYear = Convert.ToChar((dtFrom.Year - 2001) % 20 + 65).ToString();
                string fromMonth = Convert.ToChar(dtFrom.Month + 64).ToString();
                int fromLastday = DateTime.DaysInMonth(dtFrom.Year, dtFrom.Month);

                if (dtFrom.Year < 2010) return;

                //To Date
                DateTime dtTo = dtpToDate.SelectedDateTime;
                string toYear = Convert.ToChar((dtTo.Year - 2001) % 20 + 65).ToString();
                string toMonth = Convert.ToChar(dtTo.Month + 64).ToString();

                if (dtTo.Year < 2010) return;

                for(int i = 0; i < dtRslt.Rows.Count; i ++)
                {
                    if(i == 0)
                    {
                        Ddt.Rows[i][0] = ObjectDic.Instance.GetObjectName("EQSGID");
                        Ddt.Rows[i][1] = ObjectDic.Instance.GetObjectName("EQSGNAME");
                        Ddt.Rows[i][2] = ObjectDic.Instance.GetObjectName("MODEL");

                        //같은 달일 때 
                        if ((dtTo.Month - dtFrom.Month) == 0)
                        {
                            int ColumnCount = 0;
                            for (int j = dtFrom.Day; j <= dtTo.Day; j++)
                            {
                                Ddt.Rows[i][ColumnCount + 3] = Util.NVC(fromYear + fromMonth + string.Format("{0:D2}", j));
                                ColumnCount++;
                            }
                        }

                        //From Month - To Month 한 달 이상 차이 날 때 EX)240131 ~ 240301
                        else if ((dtTo.Month - dtFrom.Month) > 1)
                        {
                            //중간에 낀 달
                            int midMonth = dtFrom.Month + (dtTo.Month - dtFrom.Month - 1);
                            int midMonthLastDay = DateTime.DaysInMonth(dtFrom.Year, midMonth);
                            string sMidMonth = Convert.ToChar(midMonth + 64).ToString();

                            int ColumnCount = 0;
                            for (int j = dtFrom.Day; j <= fromLastday; j++)
                            {
                                Ddt.Rows[i][ColumnCount + 3] = Util.NVC(fromYear + fromMonth + string.Format("{0:D2}", j));
                                ColumnCount++;
                            }

                            for (int k = 1; k <= midMonthLastDay; k++)
                            {
                                Ddt.Rows[i][ColumnCount+3] = Util.NVC(fromYear + sMidMonth + string.Format("{0:D2}", k));
                                ColumnCount++;
                            }

                            for (int l = 1; l <= dtTo.Day; l++)
                            {
                                Ddt.Rows[i][ColumnCount + 3] = Util.NVC(toYear + toMonth + string.Format("{0:D2}", l));
                                ColumnCount++;
                            }
                        }

                        else
                        {
                            int ColumnCount = 0;
                            for (int j = dtFrom.Day; j <= fromLastday; j++)
                            {
                                Ddt.Rows[i][ColumnCount +3] = Util.NVC(fromYear + fromMonth + string.Format("{0:D2}", j));
                                ColumnCount++;
                            }

                            for (int k = 1; k <= dtTo.Day; k++)
                            {
                                Ddt.Rows[i][ColumnCount+3] = Util.NVC(toYear + toMonth + string.Format("{0:D2}", k));
                                ColumnCount++;
                            }
                        }
                    }
                    Ddt.Rows[i + 1][0] = Util.NVC(dtRslt.Rows[i]["EQSGID"]);
                    Ddt.Rows[i + 1][1] = Util.NVC(dtRslt.Rows[i]["EQSGNAME"]);
                    Ddt.Rows[i + 1][2] = Util.NVC(dtRslt.Rows[i]["REP_MODEL"]);
                }

                Util.GridSetData(dgHoldList, Ddt, FrameOperation, false);

                dgHoldList.Columns[0].Visibility = Visibility.Collapsed;
            }
            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                IsReadOnly = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            init_tplnsobj();
            GetFormPlan();
            GetList();
            GetHoldList();
        }

        private void GetList()
        {
            try
            {
                dtTemp = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_HOLD_DEPT_MNGT_PART_UI", "RQSTDT", "RSLTDT", dtRqst);

                dtTemp = dtRslt.Copy();
            }

            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetFormPlan()
        {
            dtPlanTemp = null;

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("FROM_DATE", typeof(string));
            dtRqst.Columns.Add("TO_DATE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd");
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_EOL_PLAN_UI", "RQSTDT", "RSLTDT", dtRqst);

            dtPlanTemp = dtRslt.Copy();
        }

        private void GetHoldList()
        {
            try
            {
                dtHoldTemp = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_HOLD_STATUS_UI", "RQSTDT", "RSLTDT", dtRqst);

                dtHoldTemp = dtRslt.Copy();
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgHoldList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            System.Windows.Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);
            
            if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;

            if (cell != null)
            {
                if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
            }
            else
            {
                if (datagrid.CurrentRow.Type != DataGridRowType.Bottom) return;
            }

            int row = cell.Row.Index;
            string sColumnName = datagrid.CurrentCell.Column.Name.ToString(); 
            string sMdoel = Util.NVC(dgHoldList[cell.Row.Index, 2].Text); 
            string sDate = Util.NVC(dgHoldList[0, cell.Column.Index].Text); 
            string sYear = (Int32.Parse(sDate.Substring(0, 1), System.Globalization.NumberStyles.HexNumber) + 2011).ToString();
            string sMonth = string.Format("{0:D2}", Encoding.ASCII.GetBytes(sDate.Substring(1, 1))[0] - 64);
            string sDay = sDate.Substring(2, 2);
            string sLine = Util.NVC(dgHoldList[cell.Row.Index, 0].Text); 
            Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, sColumnName));

            string sValue = Util.NVC(dgHoldList[cell.Row.Index, cell.Column.Index].Presenter.Background.ToString());

            if (sValue.Equals("#00FFFFFF"))
            {
                //등록 화면 연계
                FCS001_132_HOLD_DETAIL_SAVE wndPopup = new FCS001_132_HOLD_DETAIL_SAVE();
                wndPopup.FrameOperation = FrameOperation;
                  
                object[] parameters = new object[6]; 
                parameters[0] = sMdoel;
                parameters[1] = sDate;
                parameters[2] = sLine;
                parameters[3] = sYear + sMonth + sDay;

                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed += new EventHandler(HoldDetailSaveClosed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }

            else
            {
                if (cell.Column.Index.Equals(1) || cell.Column.Index.Equals(2)) return;

                //조회 화면 연계
                FCS001_132_DETAIL wndPopup = new FCS001_132_DETAIL();
                wndPopup.FrameOperation = FrameOperation;

                object[] parameters = new object[5];
                parameters[0] = sYear+ '-' + sMonth + '-' +sDay;
                parameters[1] = sMdoel;
                parameters[2] = sLine;
                parameters[3] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                parameters[4] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");

                C1WindowExtension.SetParameters(wndPopup, parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void HoldDetailSaveClosed(object sender, EventArgs e)
        {
            FCS001_132_HOLD_DETAIL_SAVE window = sender as FCS001_132_HOLD_DETAIL_SAVE;
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_132_DETAIL window = sender as FCS001_132_DETAIL;
        }

        private void dgHoldList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //default
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    e.Cell.Presenter.Padding = new Thickness(0);
                    e.Cell.Presenter.Margin = new Thickness(0);
                    e.Cell.Presenter.BorderBrush = System.Windows.Media.Brushes.LightGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 1, 0);
                }

                if (e.Cell.Row.Index.Equals(0))
                {
                    e.Cell.Presenter.Height = 50;
                    e.Cell.Presenter.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#404040"));
                    e.Cell.Presenter.FontWeight = FontWeights.SemiBold;
                    e.Cell.Presenter.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EEEEEE"));
                    e.Cell.Presenter.IsEnabled = false;
                }

                #region HOLD 

                string sHoldColumnName = string.Empty;
                string sHoldSearchData = string.Empty;
                DataRow[] HoldFoundRows;

                if (e.Cell.Row.Index == 0) return;

                sHoldColumnName = Util.NVC(dgHoldList[0, e.Cell.Column.Index].Text);


                if (string.IsNullOrEmpty(sHoldColumnName)) return;

                sHoldSearchData = "EQSGID = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 0].Text) + "' AND MODEL = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 2].Text) + "' AND WORK_DATE = '" + sHoldColumnName + "' ";

                HoldFoundRows = dtHoldTemp.Select(sHoldSearchData);

                if (HoldFoundRows.Length != 0)
                {
                    DataTableConverter.SetValue(dgHoldList.Rows[e.Cell.Row.Index].DataItem, dgHoldList.Columns[e.Cell.Column.Index].Name, HoldFoundRows[0]["HOLD_ID"]);
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(120);
                }

                #endregion

                #region 데이터 

                string sColumnName = string.Empty; 
                string sSearchData = string.Empty;
                string sHoldDept = string.Empty;
                DataRow[] foundRows;

                if (e.Cell.Row.Index == 0) return;
               
                sColumnName = Util.NVC(dgHoldList[0, e.Cell.Column.Index].Text);
                

                if (string.IsNullOrEmpty(sColumnName)) return;

                sSearchData = "EQSGID = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 0].Text) + "' AND PRODID = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 2].Text) + "' AND WRK_DATE = '" + sColumnName + "' ";

                foundRows = dtTemp.Select(sSearchData);

                if(foundRows.Length != 0)
                {
                    e.Cell.Presenter.Tag = foundRows[0]["HOLD_DEPT"].ToString();
                    sHoldDept = Util.NVC(foundRows[0]["HOLD_DEPT"].ToString());
                }

                //HOLD
                if (sHoldDept.Equals("ASSY_PROD"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.DimGray);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("ASSY_TECH"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("ELECT"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("FORM_PROD"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("FORM_TECH"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LimeGreen);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("DEVELOP"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Violet);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }

                if (sHoldDept.Equals("QA"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Purple);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                #endregion

                #region EOL_Plan

                string sPlanSearch = string.Empty;
                string sWorkDate = string.Empty;
                DataRow[] foundPlanSearchRows;

                if(dtPlanTemp.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(sColumnName)) return;
                   
                    if (e.Cell.Column.Index > 3)
                    {
                        sWorkDate = Util.NVC(dgHoldList[0, e.Cell.Column.Index].Text);

                        sSearchData = "EQSGID = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 0].Text) + "' AND MODEL = '" + Util.NVC(dgHoldList[e.Cell.Row.Index, 2].Text) + "' AND WORK_DATE = '" + sWorkDate + "' ";

                        foundPlanSearchRows = dtPlanTemp.Select(sSearchData);

                        if (foundPlanSearchRows.Length != 0)
                        {
                            e.Cell.Presenter.BorderBrush = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.BorderThickness = new Thickness(3, 3, 3, 3);
                        }
                    }
                }
                #endregion
            }));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (string.IsNullOrEmpty(WRK_DATE) || string.IsNullOrEmpty(EQSGID) || string.IsNullOrEmpty(PRODID) || string.IsNullOrEmpty(HOLD_DEPT_COLOR))
                    return;

                if (HOLD_DEPT_COLOR.Equals("Transparent"))
                    return;

                string sMsg = string.Empty;

                string sYear = (Int32.Parse(WRK_DATE.Substring(0, 1), System.Globalization.NumberStyles.HexNumber) + 2011).ToString();
                string sMonth = string.Format("{0:D2}", Encoding.ASCII.GetBytes(WRK_DATE.Substring(1, 1))[0] - 64);
                string sDay = WRK_DATE.Substring(2, 2);

                switch(HOLD_DEPT_COLOR)
                {
                    case "DimGray":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("조립생산");
                        break;

                    case "Yellow":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("조립생산기술");
                        break;

                    case "Orange":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("활성화생산기술");
                        break;

                    case "LimeGreen":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("활성화생산");
                        break;

                    case "RED":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("전극");
                        break;

                    case "Violet":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("개발팀");
                        break;

                    case "Purple":
                        HOLD_DEPT = ObjectDic.Instance.GetObjectName("QA");
                        break;
                }
                sMsg = "Date : " + sYear +"-"+ sMonth + "-" + sDay + "\n Line : " + EQSGNAME + "\n Model : " + PRODID + "\n HOLD 부서 : " + HOLD_DEPT+ "\n";

                //선택된 %1정보를 삭제하시겠습니까?
                Util.MessageConfirmByWarning("10026", result =>
                 {
                     if(result == MessageBoxResult.OK)
                     {
                         try
                         {
                             DataTable dtRqst = new DataTable();
                             dtRqst.TableName = "RQSDT";
                             dtRqst.Columns.Add("WRK_DATE", typeof(string));
                             dtRqst.Columns.Add("EQSGID", typeof(string));
                             dtRqst.Columns.Add("PRODID", typeof(string));

                             DataRow dr = dtRqst.NewRow();
                             dr["WRK_DATE"] = sYear + sMonth + sDay;
                             dr["EQSGID"] = EQSGID;
                             dr["PRODID"] = PRODID;
                             dtRqst.Rows.Add(dr);

                             DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_DEL_LOT_HOLD_DEPT_MNGT_UI", "RQSTDT", "RSLTDT", dtRqst);

                             btnSearch_Click(null, null);
                         }

                         catch(Exception ex)
                         {
                             LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                         }
                     }         
                 },sMsg);
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgHoldList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            System.Windows.Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null) return;

            int rowIndex = cell.Row.Index;
            int columnIndex = cell.Column.Index;
             
            EQSGID = Util.NVC(dgHoldList[rowIndex, 0].Text);
            EQSGNAME = Util.NVC(dgHoldList[rowIndex, 1].Text);
            PRODID = Util.NVC(dgHoldList[rowIndex, 2].Text);
            WRK_DATE = Util.NVC(dgHoldList[0, columnIndex].Text);
            System.Drawing.Color a = ColorTranslator.FromHtml(Util.NVC(dgHoldList[rowIndex, columnIndex].Presenter.Background.ToString()));
            HOLD_DEPT_COLOR = a.Name;
        }
    }
}

