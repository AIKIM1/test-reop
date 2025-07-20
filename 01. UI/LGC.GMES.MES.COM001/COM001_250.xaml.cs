/*************************************************************************************
 Created Date : 2018.08.24
      Creator : 
   Decription : 출하계획 
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.24  DEVELOPER : Initial Created.
  2018.12.03 손우석 3852216 GMES 월 출하 계획 화면 개선 요청 件 [요청번호]C20181123_52216
  2018.12.06 손우석 3852216 GMES 월 출하 계획 화면 개선 요청 件 [요청번호]C20181123_52216
  2019.12.19 손우석 SM  메시지 다국어 처리
  2021.05.12 김동일 : C20210415-000326 출고처 컬럼 추가.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
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
using C1.WPF.DataGrid.Summaries;
using System.Windows.Data;
//using Microsoft.VisualBasic;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_250 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtNote = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_250()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            dtpDateTo.SelectedDateTime = System.DateTime.Now.AddDays(31);
            switch(LoginInfo.CFG_AREA_ID)
            {
                case "A1":
                case "A2":
                    txtAletPlan.Visibility = Visibility.Visible;
                    txtAletPlan.Text = "비상연락망:물류(안재현 선임), 생산관리(이재범 책임)";
                    break;
                default:
                    txtAletPlan.Visibility = Visibility.Collapsed;
                    break;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;
            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            // 제품유형
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "SHIP_PLAN_TYPE_CELL", "SHIP_PLAN_TYPE_PACK" };
            String[] sFilter2 = { "TRANSP_MODE" };
            String[] sFilter3 = { "MKT_TYPE_CODE" };

            string[] SHIP_PLAN;
            switch (LoginInfo.CFG_AREA_ID)
            {
                case "A1":  
                case "A2":
                    SHIP_PLAN = sFilter1[0].Split(',');
                    break;
                default:  //PACK
                    SHIP_PLAN = sFilter1[1].Split(',');
                    break;
            }
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "AREA_CP");
            _combo.SetCombo(cboShipType, CommonCombo.ComboStatus.ALL, sFilter: SHIP_PLAN, sCase: "COMMCODE");
            _combo.SetCombo(cboTransport, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
            _combo.SetCombo(cboMKT_TYPE, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboArea.Items.Count > 0) cboArea.SelectedIndex = 0;

            //2018.12.06
            _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL);
        }
        #endregion

        #region Event
        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetGridDate();
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.Rows.Count <= 3)
                return;

            if (dgList.CurrentRow == null)
                return;

            if (string.Equals(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "TRANSP_MODE"), "Total"))
                return;

            if (!dgList.CurrentColumn.Name.Contains("SHIP_QTY_"))
                return;            

            COM001.COM001_250_REMARK NotePopup = new COM001.COM001_250_REMARK();
            NotePopup.FrameOperation = FrameOperation;

            if (NotePopup != null)
            {
                object[] Parameters = new object[6];
                string _ValueToDate = string.Empty;
                string _Year = string.Empty;
                string _Month = string.Empty;
                string _Day = string.Empty;

                string[] splitColHeader = dgList.CurrentColumn.ActualGroupHeader.ToString().Split(',');
                string[] splitYearMonth = splitColHeader[0].Split('.');
                _Year = splitYearMonth[0].Trim().Replace("[#]", "").Trim();
                _Month = string.Concat('0', splitYearMonth[1].Trim());
                _Day = string.Concat('0', splitColHeader[2].Trim());

                _ValueToDate = _Year + _Month.Substring(_Month.Length - 2) + _Day.Substring(_Day.Length - 2);

                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRODID"));
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "SHIPTO_CODE"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "SHIPTO_CODE_NAME"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "TRANSP_MODE"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "TRANSP_MODE_NAME"));
                Parameters[5] = _ValueToDate;

                C1WindowExtension.SetParameters(NotePopup, Parameters);

                NotePopup.Closed += new EventHandler(NotePopup_Closed);
                NotePopup.ShowModal();
                NotePopup.CenterOnScreen();
            }
        }
        private void NotePopup_Closed(object sender, EventArgs e)
        {
            COM001_250_REMARK Window = sender as COM001_250_REMARK;
            if (Window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                string _Day = string.Empty;
                string _colName = string.Empty;
                string _dayofWeek = string.Empty;
                string _date = string.Empty;
                string ValueToFind = string.Empty;
                int iYear;
                int iMon;
                int iDay;
                DateTime DateValue;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Top && e.Cell.Column.Name.Contains("SHIP_QTY_"))
                    {
                        string[] splitColHeader = e.Cell.Column.ActualGroupHeader.ToString().Split(',');
                        string[] splitYearMonth = splitColHeader[0].Split('.');

                        iYear = int.Parse(splitYearMonth[0].Trim().Replace("[#]", ""));
                        iMon = int.Parse(splitYearMonth[1].Trim().Replace("[#]", "").Replace("(Today)", ""));
                        iDay = int.Parse(splitColHeader[2].Trim().Replace("[#]", ""));
                        DateValue = new DateTime(iYear, iMon, iDay);

                        _dayofWeek = Convert.ToString((int)DateValue.DayOfWeek);

                        if (e.Cell.Presenter.Content.GetType() == typeof(C1.WPF.DataGrid.DataGridColumnHeaderPresenter))
                        {
                            System.Windows.Controls.ContentControl cc = (e.Cell.Presenter.Content as System.Windows.Controls.ContentControl);
                            switch (_dayofWeek.Trim())
                            {
                                case "6": //토
                                    cc.Foreground = new SolidColorBrush(Colors.Blue);
                                    break;
                                case "0": //일
                                    cc.Foreground = new SolidColorBrush(Colors.Red);
                                    break;
                                default:
                                    cc.Foreground = new SolidColorBrush(Colors.Black);
                                    break;
                            }
                        }
                    }
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        string sDataType = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRANSP_MODE"));
                        if (sDataType.Equals("Total"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                        else
                        {
                            //일자별 비고항목 표시
                            int Startcol = dgList.Columns["SHIP_QTY"].Index;
                            for (int i = Startcol - 1; i < dgList.Columns.Count; i++)
                            {
                                if (e.Cell.Column.Name.Contains("SHIP_QTY_"))
                                {
                                    string[] splitDay = e.Cell.Column.Name.Split('_');
                                    _Day = splitDay[2];

                                    DataRow[] dr = dtNote.Select("SHOPID         = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "SHOPID")) + "'         AND " +
                                                                 "SHIP_PLAN_TYPE = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "SHIP_PLAN_TYPE")) + "' AND " +
                                                                 "PRODID         = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "PRODID")) + "'         AND " +
                                                                 "SHIPTO_CODE    = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "SHIPTO_CODE")) + "'    AND " +
                                                                 "TRANSP_MODE    = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Cell.Row.Index].DataItem, "TRANSP_MODE")) + "'");

                                    if (dr.Length > 0)
                                    {
                                        foreach (DataRow row in dr)
                                        {
                                            _colName = "SHIP_NOTE_" + _Day;
                                            ValueToFind = Util.NVC(row[_colName]);

                                            if (ValueToFind.Equals("Y"))
                                            {
                                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                                            }                                                
                                            else
                                            {
                                                e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtPrjName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void txtModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void txtShipToName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 62)
                {
                    Util.Alert("SFU5033", new object[] { "2" });   // 기간은 {}달 이내 입니다.
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(60);
                    //SetGridDate();
                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                try
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        SetcboPrj(_ValueFrom, _ValueTo);
                    }));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 62)
                {
                    Util.Alert("SFU5033", new object[] { "2" });  // 기간은 {}달 이내 입니다.
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-60);
                    //SetGridDate();
                    return;
                }

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                try
                {
                    this.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        SetcboPrj(_ValueFrom, _ValueTo);
                    }));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region Mehod

        private void SetcboPrj(string sFromDate, string sToDate)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_DATE"] = sFromDate;
                dr["TO_DATE"] = sToDate;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_SHIP_PLAN_PRJT_NAME", "RQSTDT", "RSLTDT", RQSTDT);

                cboProject.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProject.Check(i);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void SetGridDate()
        {
            System.DateTime dtFrom = dtpDateFrom.SelectedDateTime;
            System.DateTime dtTo = dtpDateTo.SelectedDateTime;

            int i = 0;
            for (i = 1; i <= dtTo.Subtract(dtFrom).Days + 1; i++)
            {
                string dayColumnName = string.Empty;

                if (i < 10)
                {
                    dayColumnName = "SHIP_QTY_0" + i.ToString();
                }
                else
                {
                    dayColumnName = "SHIP_QTY_" + i.ToString();
                }

                List<string> sHeader = new List<string>() { dtFrom.AddDays(i - 1).Year.ToString() + "." + dtFrom.AddDays(i - 1).Month.ToString(), dtFrom.AddDays(i - 1).ToString("ddd"), dtFrom.AddDays(i - 1).Day.ToString() };

                if (DateTime.Now.ToString("yyyyMMdd") == dtFrom.AddDays(i - 1).Year.ToString() + dtFrom.AddDays(i - 1).Month.ToString("00") + dtFrom.AddDays(i - 1).ToString("dd"))
                {
                    List<string> sHeader_Today = new List<string>() { dtFrom.AddDays(i - 1).Year.ToString() + "." + dtFrom.AddDays(i - 1).Month.ToString(), dtFrom.AddDays(i - 1).ToString("ddd") + " (Today)", dtFrom.AddDays(i - 1).ToString("dd") };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
            }
        }

        private void SearchData()
        {
            try
            {
                if (Util.NVC(cboProject.SelectedItemsToString) == "")
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    //2019.12.16
                    //Util.MessageValidation("SFU4964");  //프로젝트를 선택하세요
                    Util.MessageValidation("SFU3478");  //프로젝트를 선택하세요
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                for (int i = dgList.Columns.Count; i-- > 13;) //Grid Head 삭제
                    dgList.Columns.RemoveAt(i);

                //Grid Head 재생성
                SetGridDate();

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                string sTemp = Util.NVC(cboArea.SelectedValue);
                string sSHOPID = string.Empty;
                if (sTemp == "")
                {
                    sSHOPID = "";
                }
                else
                {
                    string[] sArry = sTemp.Split('^');
                    sSHOPID = sArry[1];
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FROM_DATE", typeof(string));
                IndataTable.Columns.Add("TO_DATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("SHIP_PLAN_TYPE", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("SHIPTO_CODE_NAME", typeof(string));
                IndataTable.Columns.Add("TRANSP_MODE", typeof(string));
                IndataTable.Columns.Add("SUM_FLAG", typeof(string));
                IndataTable.Columns.Add("EXCEPT_FLAG", typeof(string));
                IndataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                //2018.12.03
                IndataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FROM_DATE"] = _ValueFrom;
                Indata["TO_DATE"] = _ValueTo;
                Indata["SHOPID"] = sSHOPID;
                Indata["SHIP_PLAN_TYPE"] = Util.GetCondition(cboShipType);
                Indata["PRJT_NAME"] = cboProject.SelectedItemsToString;
                Indata["PRODID"] = Util.NVC(txtProdID.Text.Trim());
                Indata["MODLID"] = Util.NVC(txtModel.Text.Trim());
                Indata["SHIPTO_CODE_NAME"] = Util.NVC(txtShipToName.Text.Trim());
                Indata["TRANSP_MODE"] = Util.GetCondition(cboTransport); 
                Indata["SUM_FLAG"] = chkGTotal.IsChecked == true ? "Y" : "N";
                Indata["EXCEPT_FLAG"] = chkSTotal.IsChecked == true ? "Y" : "N";
                Indata["MKT_TYPE_CODE"] = Util.GetCondition(cboMKT_TYPE);
                //2018.12.03
                Indata["PRDT_CLSS_CODE"] = Util.GetCondition(cboPrdtClass);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_SHIP_PLAN", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgList, result, FrameOperation, true);

                    // ESWA 만 출고처 컬럼 View 처리.
                    if (LoginInfo.SYSID.Equals("GMES-A-WA") ||
                        LoginInfo.SYSID.Equals("GMES-A-W3"))
                    {
                        if (dgList.Columns.Contains("FROM_AREAID"))
                            dgList.Columns["FROM_AREAID"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (dgList.Columns.Contains("FROM_AREAID"))
                            dgList.Columns["FROM_AREAID"].Visibility = Visibility.Collapsed;
                    }


                    // I/F 일자
                    DataRow drow;
                    var sCond = new List<string>();

                    //DataRow[] CellRows = result.Select("SHIP_PLAN_TYPE = 'SSP'", "INSDTTM DESC");
                    DataRow[] CellRows = result.Select("SHIP_PLAN_TYPE = 'SSP'", "UPDDTTM DESC");
                    if (CellRows.Length > 0)
                    {
                        drow = CellRows.First();
                        //sCond.Add("Cell :" + Util.NVC(drow["INSDTTM"]));
                        sCond.Add("Cell :" + Util.NVC(drow["UPDDTTM"]));
                    }

                    //DataRow[] PackRows = result.Select("SHIP_PLAN_TYPE = 'PACK'", "INSDTTM DESC");
                    DataRow[] PackRows = result.Select("SHIP_PLAN_TYPE = 'PACK'", "UPDDTTM DESC");
                    if (PackRows.Length > 0)
                    {
                        drow = PackRows.First();
                        //sCond.Add("Pack : " + Util.NVC(drow["INSDTTM"]));
                        sCond.Add("Pack : " + Util.NVC(drow["UPDDTTM"]));
                    }
                    txtIFTime.Text = string.Join(" ,", sCond);

                    if (result.Rows.Count > 0)
                        dtNote = result.Select().CopyToDataTable();

                    for (int i = 0; i < dgList.GetRowCount(); i++)
                    {
                        Scrolling_Into_Today_Col();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Scrolling_Into_Today_Col()
        {
            string today_Col_Name = "SHIP_QTY_" + DateTime.Now.ToString("dd");

            if (dgList.Columns[today_Col_Name] != null)
            {
                if (DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateFrom.SelectedDateTime) ||
                    DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateTo.SelectedDateTime))
                {
                    dgList.ScrollIntoView(2, dgList.Columns[today_Col_Name].Index);
                }
            }
        }
        #endregion
    }
}
