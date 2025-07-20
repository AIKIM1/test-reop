/*************************************************************************************
 Created Date : 2024.05.14
      Creator : kor21cman
   Decription : GMES Main (배포이력 공지)
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.14  조범모 : E20231011-000895 GMES Main 화면 배포이력 표기

**************************************************************************************/

using LGC.GMES.MES.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// Interaction logic for COM001_999.xaml
    /// </summary>
    public partial class COM001_MAIN : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        DataView _dvCMCD { get; set; }

        object[] parameter = null;
        private string _RELS_REQ_ID = string.Empty;
        string sURL_MESAuthority = @"http://itsm.lgensol.com/cm/service/html.ncd?view=/lgerp/am/amMain&title=%EC%A0%84%EC%82%AC%EA%B6%8C%ED%95%9C%EA%B4%80%EB%A6%AC&navi=%EA%B6%8C%ED%95%9C%EC%8B%A0%EC%B2%AD+%3E+%EC%A0%84%EC%82%AC%EA%B6%8C%ED%95%9C%EA%B4%80%EB%A6%AC&itemId=null";
        string sURL_MESInstall   = @"http://10.32.192.109/Citrix/notice/notice.html";
        #endregion

        #region Initialize
        public COM001_MAIN()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private void InitializeCombo()
        {
            SetComboYear();
            SetComboMonth();
            SetComboWeek();

            Get_COMMCODE_CBO_MULT("'BIZ_DVSN'"       // 배포 대상 시스템 그룹
                      + ", 'MES_SYSTEM_TYPE_CODE'"   // MES 시스템 그룹
                      + ", 'RELS_FLAG'"              // 배포여부
                      , null, (dt, ex) =>
                      {
                          if (ex != null)
                          {
                              ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                              return;
                          }
                          _dvCMCD = dt.DefaultView;

                          SetComboSystem();
                          btnSearch_Click(null, null);
                      });
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, EventArgs e)
        {
            parameter = C1WindowExtension.GetParameters(this);

            InitializeCombo();

            this.Loaded -= UserControl_Loaded;

            if (txtMESAuthorityCopyLink.Text.Length > 200)
                txtMESAuthorityCopyLink.TextWrapping = System.Windows.TextWrapping.Wrap;

            if (txtMESAuthorityCopyLink.Text.Length > 250)
                this.grdRoot.RowDefinitions[3].Height = new GridLength(100);
            else
                this.grdRoot.RowDefinitions[3].Height = new GridLength(80);
        }

        void REQUEST_popup_Closed(object sender, EventArgs e)
        {
            try
            {
                //btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                //CommonUtil.MessageError(ex);
            }
        }

        private void dgDeployRequestList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgDeployRequestList.GetCellFromPoint(pnt);
            if (cell != null)
            {
                if (cell.Row.Index > -1)
                {
                    if ((cell.Column.Name != ""))
                    {
                        _RELS_REQ_ID = Util.NVC(DataTableConverter.GetValue(dgDeployRequestList.Rows[cell.Row.Index].DataItem, "RELS_REQ_ID"));

                        COM001_MAIN_Popup wndPopup = new COM001_MAIN_Popup();
                        wndPopup.FrameOperation = this.FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[4];
                            Parameters[0] = _dvCMCD;
                            Parameters[1] = _RELS_REQ_ID;
                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            //wndPopup.Closed += new EventHandler(REQUEST_popup_Closed);
                            wndPopup.Closed -= REQUEST_popup_Closed;
                            wndPopup.Closed += REQUEST_popup_Closed;

                            wndPopup.ShowModal();
                            wndPopup.CenterOnScreen();

                        }
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();

            try
            {
                Logger.Instance.WriteLine("[FRAME] GMES Main Search", Logger.MESSAGE_OPERATION_START, LogCategory.FRAME);

                DataTable InputData = new DataTable();
                InputData.Columns.Add("LANGID", typeof(string));
                InputData.Columns.Add("RELS_YEAR", typeof(string));
                InputData.Columns.Add("RELS_MNTH", typeof(string));
                InputData.Columns.Add("RELS_WEEK", typeof(string));
                InputData.Columns.Add("RELS_FLAG", typeof(string));
                InputData.Columns.Add("DEL_FLAG", typeof(string));
                InputData.Columns.Add("LOGIC_TRGT_CORP", typeof(string));
                InputData.Columns.Add("LOGIC_TRGT_SYSTEM_GR", typeof(string));
                InputData.Columns.Add("SYSTEM_ID", typeof(string));
                InputData.Columns.Add("RELS_REQ_DATE", typeof(string));

                DataRow rowIndata = InputData.NewRow();
                rowIndata["LANGID"]    = LoginInfo.LANGID;
                rowIndata["SYSTEM_ID"] = LoginInfo.SYSID;   
                rowIndata["LOGIC_TRGT_SYSTEM_GR"] = this.cboSystem.SelectedValue;

                rowIndata["RELS_YEAR"] = this.cboYear.SelectedValue;
                rowIndata["RELS_MNTH"] = this.cboMonth.SelectedValue;
                rowIndata["RELS_WEEK"] = this.cboWeek.SelectedValue;

                //옵션 (개발시 테스트용)
                if (false)
                {
                    //if (LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG")
                    //rowIndata["LOGIC_TRGT_CORP"] = "NA";       // 특정법인 조회용
                    rowIndata["RELS_FLAG"] = "W";                // 배포 여부 (W: Ready, C: Complete, R: Rollback)
                    rowIndata["DEL_FLAG"]  = "N";                // 삭제 여부 (Y:삭제, N:미삭제)
                }

                InputData.Rows.Add(rowIndata);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_GMES_RELS_REQ_LIST", "RQSTDT", "RSLTDT", InputData);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgDeployRequestList, dtRslt, this.FrameOperation, false);
                }
                else
                {
                    Util.gridClear(dgDeployRequestList);
                }

                Logger.Instance.WriteLine("[FRAME] GMES Main Search", Logger.MESSAGE_OPERATION_END, LogCategory.FRAME);
            }
            catch (Exception ex) { }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgDeployRequestList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //  default 
                    //e.Cell.Presenter.FontSize = 12;

                    if (e.Cell.Column.Name == "CSR_TITL")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = System.Windows.Input.Cursors.Hand;
                    }

                }
            }));
        }

        private void dgDeployRequestList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
        #endregion

        #region Method
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

        private void Get_COMMCODE_CBO_MULT(string sCMCDTYPE, string sUSE_FLAG, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));
            IndataTable.Columns.Add("USE_FLAG", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = sCMCDTYPE;
            Indata["USE_FLAG"] = "Y";

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMCODE_CBO_MULT", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        private void btnMESInstall_Click(object sender, RoutedEventArgs e)
        {
            //openWebBrowser(sURL_MESInstall);
            System.Diagnostics.Process.Start(sURL_MESInstall);
            //openDefaultBrowser(sURL_MESInstall);
        }

        private void btnMESAuthority_Click(object sender, RoutedEventArgs e)
        {
            //openWebBrowser(sURL_MESAuthority);
            //System.Diagnostics.Process.Start(sURL_MESAuthority);
            openDefaultBrowser(sURL_MESAuthority);
        }

        private void openWebBrowser(string sURL)
        {
            COM001_MAIN_WebBrowser webBrowser = new COM001_MAIN_WebBrowser();
            webBrowser.FrameOperation = this.FrameOperation;

            if (webBrowser != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = sURL;
                C1WindowExtension.SetParameters(webBrowser, Parameters);

                webBrowser.Closed -= REQUEST_popup_Closed;
                webBrowser.Closed += REQUEST_popup_Closed;

                webBrowser.ShowModal();
                webBrowser.CenterOnScreen();
            }
        }

        private void openDefaultBrowser(string sURL)
        {
            try
            {
                using (System.Diagnostics.Process cmd = new System.Diagnostics.Process())
                {
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine($"start {sURL}");
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        private void btnMESinstallCopyLink_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                btnMESInstall_Click(null, null);
            else
                Clipboard.SetText(txtMESinstallLink.Text);
        }

        private void btnMESAuthorityCopyLink_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
                btnMESAuthority_Click(null, null);
            else
                Clipboard.SetText(sURL_MESAuthority);
        }

        #region 년, 월, 일, 날짜 관련

        #region 연도 검색조건의 콤보 리스트를 설정(현재 연도를 기본값)

        private void SetComboYear()
        {
            this.cboYear.SetDataComboItem(getYearList(), CommonCombo.ComboStatus.SELECT);

            string currentYear = DateTime.Today.Year.ToString(); 
            this.cboYear.SelectedValue = currentYear;
        }

        #endregion 연도 검색조건의 콤보 리스트를 설정(현재 연도를 기본값)

        #region 월 검색 조건의 콤보 리스트를 설정(현재 월을 기본값)

        private void SetComboMonth()
        {
            this.cboMonth.SetDataComboItem(getMonthList(), CommonCombo.ComboStatus.ALL);

            string currentMonth = string.Format("{0:0#}", DateTime.Today.Month);
            this.cboMonth.SelectedValue = currentMonth;
        }

        #endregion 월 검색 조건의 콤보 리스트를 설정(현재 월을 기본값)

        #region 주차 검색 조건의 콤보 리스트를 설정(현재 월의 주차를 기본값)

        private void SetComboWeek()
        {
            DateTime curdt = DateTime.Today;
            this.cboWeek.SetDataComboItem(getWeekList(), CommonCombo.ComboStatus.ALL);

            //string currentWeek = GetWeekofMonth(curdt)[1].ToString();
            //this.cboWeek.SelectedValue = currentWeek;

            this.cboWeek.SelectedIndex = 0;
        }

        #endregion 주차 검색 조건의 콤보 리스트를 설정(현재 월의 주차를 기본값)

        #region 연도 콤보박스에 입력될 연도리스트를 생성

        private DataTable getYearList()
        {
            DataTable rsDT = new DataTable();
            rsDT.TableName = "OUTDATA";
            rsDT.Columns.Add("CMCODE");
            rsDT.Columns.Add("CMCDNAME");

            string currentYear = DateTime.Today.Year.ToString();
            int NowYear = Convert.ToInt32(currentYear) + 5;
            int StartYear = 2024;

            for (int i = StartYear; i <= NowYear; i++)
            {
                DataRow dr = rsDT.NewRow();
                dr["CMCODE"] = Convert.ToString(i.ToString());
                dr["CMCDNAME"] = Convert.ToString(i.ToString());
                rsDT.Rows.Add(dr);
            }

            return rsDT;
        }

        #endregion 연도 콤보박스에 입력될 연도리스트를 생성

        #region 월 콤보박스에 입력될 월 리스트를 생성

        private DataTable getMonthList()
        {
            DataTable rsDT = new DataTable();
            rsDT.TableName = "OUTDATA";
            rsDT.Columns.Add("CMCODE");
            rsDT.Columns.Add("CMCDNAME");

            for (int i = 1; i <= 12; i++)
            {
                DataRow dr = rsDT.NewRow();

                if (i < 10)
                {
                    dr["CMCODE"] = "0" + Convert.ToString(i.ToString());
                }
                else
                {
                    dr["CMCODE"] = Convert.ToString(i.ToString());
                }
                dr["CMCDNAME"] = Convert.ToString(i.ToString());

                rsDT.Rows.Add(dr);
            }

            return rsDT;
        }

        #endregion 월 콤보박스에 입력될 월 리스트를 생성

        #region 월의 주차 콤보박스에 입력될 주차리스트를 생성

        private DataTable getWeekList()
        {
            DataTable rsDT = new DataTable();
            rsDT.TableName = "OUTDATA";
            rsDT.Columns.Add("CMCODE");
            rsDT.Columns.Add("CMCDNAME");

            for (int i = 1; i <= 5; i++)
            {
                DataRow dr = rsDT.NewRow();
                dr["CMCODE"] = Convert.ToString(i.ToString());
                dr["CMCDNAME"] = Convert.ToString(i.ToString());
                rsDT.Rows.Add(dr);
            }

            return rsDT;
        }

        #endregion 월의 주차 콤보박스에 입력될 주차리스트를 생성

        #region 파라미터의 날짜에 대한 월과 주차정보를 반환(배열 0번 : 월, 1번 : 주)

        private int[] GetWeekofMonth(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    dt = dt.AddDays(3);
                    break;
                case DayOfWeek.Tuesday:
                    dt = dt.AddDays(2);
                    break;
                case DayOfWeek.Wednesday:
                    dt = dt.AddDays(1);
                    break;
                case DayOfWeek.Thursday:
                    dt = dt.AddDays(0);
                    break;
                case DayOfWeek.Friday:
                    dt = dt.AddDays(-1);
                    break;
                case DayOfWeek.Saturday:
                    dt = dt.AddDays(-2);
                    break;
                case DayOfWeek.Sunday:
                    dt = dt.AddDays(-3);
                    break;
            }

            int month = dt.Month;
            int weekofmonth = 0;

            while (true)
            {
                if (dt.Month == month)
                {
                    dt = dt.AddDays(-7);
                    weekofmonth++;
                }
                else
                {
                    break;
                }
            }

            return new int[] { month, weekofmonth };
        }

        #endregion 파라미터의 날짜에 대한 월과 주차정보를 반환(배열 0번 : 월, 1번 : 주)

        #region 선택된 날짜의 요일 반환

        private string GetDayOfWeek(DateTime dt)
        {
            string sDOW = dt.DayOfWeek.ToString();

            return sDOW;
        }

        #endregion 선택된 날짜의 요일 반환

        #region 년,월, 주 정보에 해당하는 화요일 찾기


        private DataTable GET_Calendar(string sYear, string sMonth)
        {
            string sStartDate = sYear + "-" + sMonth + "-" + "01" + " 00:00:00";
            DateTime DT_StartDate = Convert.ToDateTime(sStartDate);
            DateTime DT_EndDAte = DT_StartDate.AddMonths(1).AddDays(-1);

            int eDay = Int32.Parse(DT_EndDAte.ToShortDateString().Substring(8, 2));
            string sDay = string.Empty;
            string sDate = string.Empty;
            DateTime DT_Date = new DateTime();

            DataTable DTCalendar = new DataTable();
            DTCalendar.TableName = "OUTDATA";
            DTCalendar.Columns.Add("YEAR");
            DTCalendar.Columns.Add("MONTH");
            DTCalendar.Columns.Add("DAY");
            DTCalendar.Columns.Add("WEEK");
            DTCalendar.Columns.Add("DAY_OF_WEEK");

            for (int i = 1; i <= eDay; i++)
            {
                DataRow dr = DTCalendar.NewRow();
                dr["YEAR"] = sYear;
                dr["MONTH"] = sMonth;

                if (i < 10)
                {
                    sDay = "0" + Convert.ToString(i.ToString());
                }
                else
                {
                    sDay = Convert.ToString(i.ToString());
                }
                dr["DAY"] = sDay;

                sDate = sYear + "-" + sMonth + "-" + sDay + " 00:00:00";
                DT_Date = Convert.ToDateTime(sDate);
                dr["WEEK"] = GetWeekofMonth(DT_Date)[1].ToString();

                dr["DAY_OF_WEEK"] = GetDayOfWeek(DT_Date);

                DTCalendar.Rows.Add(dr);
            }

            return DTCalendar;
        }

        #endregion 년,월, 주 정보에 해당하는 화요일 찾기

        private string ConvertToString(DateTime dt)
        {
            string strDt = string.Empty;

            strDt += dt.Year.ToString();
            strDt += dt.Month.ToString().Length == 1 ? "0" + dt.Month.ToString() : dt.Month.ToString();
            strDt += dt.Day.ToString().Length == 1 ? "0" + dt.Day.ToString() : dt.Day.ToString();

            return strDt;
        }

        #endregion 년, 월, 일, 날짜 관련

        #region 배포 법인, 배포 시스템 관련
        private void SetComboSystem()
        {
            _dvCMCD.RowFilter = "CMCDTYPE = 'MES_SYSTEM_TYPE_CODE' AND USE_FLAG = 'Y' ";

            this.cboSystem.SelectedValuePath = "CMCODE";
            this.cboSystem.DisplayMemberPath = "CMCDNAME";
            this.cboSystem.SetDataComboItem(_dvCMCD.ToTable(), CommonCombo.ComboStatus.ALL, true, ComboBoxExtension.InCodeType.Colon, LoginInfo.CFG_SYSTEM_TYPE_CODE);

            // MES_SYSTEM_TYPE_CODE : E, A, F, P
            // BIZ_DVSN : ME,MA, MF,MP, AE, AA, AF, AP 
            // SYSID : GMES-E-NJ, CFG_SHOP_ID : G183, CFG_SHOP_NAME : NJ 소형 전극, CFG_SYSTEM_TYPE_CODE : E
            //this.cboSystem.SelectedValue = LoginInfo.CFG_SYSTEM_TYPE_CODE;
        }

        #endregion 배포 법인, 배포 시스템 관련

        #endregion
    }
}
