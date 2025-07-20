/*************************************************************************************
 Created Date : 2021.01.30
      Creator : 오화백
   Decription : LESS/CESS 투입현황
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.30    오화백K : Initial Created.    
  2021.12.29    김광오        C20210904-000039        [ESWA PI]GMES 시스템 CESS GMES 화면 구성
  2022.08.15    오화백  :  잔량을 소수점 3째자리까지, AREA_TMPR_KIND 공통코드를 통해서 온도 컬럼에 화씨로 표시할지 섭씨로 표시할지로 가능하게함
  2024.01.09    유재홍  : UI 편의성 개선을 위한 컬럼명, 컬럼위치 변경 & 탱크현황 클릭시 탱크 데이터 수집이력 화면 조회 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Text;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_080.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_080 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private bool _isLoaded = false;

        public decimal TankLevelAlarmLow { get; set; } = 1;
        public decimal TankGroupLevelAlarmLow { get; set; } = 1;

        public ASSY004_080()
        {
            InitializeComponent();

            #region C20210904-000039 [ESWA PI]GMES 시스템 CESS GMES 화면 구성 Added by kimgwango on 2022.01.12
            //TankLevelAlarmLow = GetTankLevelAlarmLow();
            //TankGroupLevelAlarmLow = GetTankGroupLevelAlarmLow();
            #endregion
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControl();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;

            #region [ C20210904-000039 ] Added by kimgwango on 2021.12.27
            // 수정사항 3 - 2) 온도, 압력, 잔량 항목에 대한 단위 표시 필요 : 각각 ℃, bar, %
            dgTank.LoadedColumnHeaderPresenter += dgTank_LoadedColumnHeaderPresenter;
            dgTank.LoadedRowPresenter += dgTank_LoadedRowPresenter;
            dgTank.UnloadedRowPresenter += dgTank_UnloadedRowPresenter;

            dgTankDataHistory.LoadedColumnHeaderPresenter += dgTankDataHistory_LoadedColumnHeaderPresenter;
            #endregion
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void InitializeControl()
        {
            InitializeCombo();
        }

        private void InitializeCombo()
        {
            // 구분 콤보박스
            SetDivisionCombo(cboDivision);
        }

        #endregion

        #region [ Event ] - Button
        /// <summary>
        /// 조회버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            ShowLoadingIndicator();

            #region #region C20210904-000039 [ESWA PI]GMES 시스템 CESS GMES 화면 구성 Added by kimgwango on 2022.01.12
            TankLevelAlarmLow = GetTankLevelAlarmLow();
            TankGroupLevelAlarmLow = GetTankGroupLevelAlarmLow();
            #endregion

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EL_TANK_TYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EL_TANK_TYPE"] = cboDivision.SelectedIndex == 0 ? null : cboDivision.SelectedValue.ToString();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_ELCTRLT_TANK_LIST_CL_L", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgTank, bizResult, null, true);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        #endregion

        #region [ Event ] - dgTank

        /// <summary>
        /// 탱크현황 리스트 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTank_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Column.Name.Equals("ELCTRLT_TANK_NAME"))
                {
                    int rowIdx = cell.Row.Index;
                    DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                    if (drv == null) return;

                    SelStatChangeHistory(DataTableConverter.GetValue(drv, "ELCTRLT_TANK_ID").GetString());
                    SelDataHistory(DataTableConverter.GetValue(drv, "ELCTRLT_TANK_ID").GetString());//2024-01-09 유재홍 
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 탱크현황 링크 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTank_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            decimal decMtrlQty = 0;
            decimal decTankCapa = 0;
            decimal decMtrlTankGrpCapa = 0;
            decimal decTankGrpCapa = 0;

            bool blnMtrlQty = false;
            bool blnTankCapa = false;
            bool blnMtrlTankGrpCapa = false;
            bool blnTankGrpCapa = false;

            bool blnMtrlQtyAlarm = false;
            bool blnMtrlTankGrpCapaAlarm = false;

            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "ELCTRLT_TANK_NAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    #region [ C20210904-000039 ] Added by kimgwango on 2021.12.27
                    // 기타 1) 탱크별 기준량 정보를 기반으로 전해액 잔량 레벨이 일정 이상이면 녹색, 일정 이하이면 붉은색 등으로 표시
                    // 기타 3) Tank 2개 중 1개가 사용을 다 하였는데, 사용하고 있는 Tank 가 50 % 미만으로 남아 있을 경우 전체 붉은색으로 표시
                    if (e.Cell.Row.DataItem == null) return;

                    DataRowView dvr = (DataRowView)e.Cell.Row.DataItem;
                    DataRow dr = dvr.Row;
                    switch (e.Cell.Column.Name)
                    {
                        case "ELCTRLT_TANK_NAME":
                            break;
                        case "EL_TANK_TYPE_NAME":
                        case "ELCTRLT_TANK_GR_NAME":
                        case "ELCTRLT_TANK_GR_CODE":
                            DataView dv = (DataView)dataGrid.ItemsSource;
                            DataTable dt = dv?.ToTable();

                            string strELCTRLT_TANK_GR_CODE = dr["ELCTRLT_TANK_GR_CODE"]?.ToString();
                            List<DataRow> lst = dt?.Select(string.Format("ELCTRLT_TANK_GR_CODE = '{0}'", strELCTRLT_TANK_GR_CODE)).ToList<DataRow>();

                            if (lst != null && lst.Count > 0)
                            {
                                foreach (DataRow dr2 in lst)
                                {
                                    decMtrlQty = 0;
                                    decTankCapa = 0;
                                    decMtrlTankGrpCapa = 0;
                                    decTankGrpCapa = 0;

                                    blnMtrlQty = decimal.TryParse(dr2["MTRLQTY"]?.ToString(), out decMtrlQty);                                // 잔량
                                    blnTankCapa = decimal.TryParse(dr2["ELCTRLT_TANK_CAPA"]?.ToString(), out decTankCapa);                    // 탱크 용량
                                    blnMtrlTankGrpCapa = decimal.TryParse(dr2["MTRL_TANK_GR_CAPA"]?.ToString(), out decMtrlTankGrpCapa);      // 잔량 그룹 총량
                                    blnTankGrpCapa = decimal.TryParse(dr2["ELCTRLT_TANK_GR_CAPA"]?.ToString(), out decTankGrpCapa);           // 탱크 그룹 총량

                                    #region [ 잔량 체크 ]
                                    if (blnMtrlQty && TankLevelAlarmLow > 0)
                                    {
                                        if (decMtrlQty < TankLevelAlarmLow)
                                        {
                                            blnMtrlQtyAlarm = true;
                                            break;
                                        }
                                    }
                                    #endregion

                                    #region [ 그룹 총량 체크 ]
                                    if (blnTankCapa && blnTankGrpCapa && blnMtrlTankGrpCapa &&
                                        decTankCapa > 0 && decTankGrpCapa > 0 &&
                                        decTankCapa != decTankGrpCapa && TankGroupLevelAlarmLow > 0)
                                    {
                                        if (decMtrlTankGrpCapa < decTankGrpCapa * decimal.Divide(TankGroupLevelAlarmLow, (decimal)100)) // Percent
                                        {
                                            blnMtrlTankGrpCapaAlarm = true;
                                            break;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            break;
                        default:
                            decMtrlQty = 0;
                            decTankCapa = 0;
                            decMtrlTankGrpCapa = 0;
                            decTankGrpCapa = 0;

                            blnMtrlQty = decimal.TryParse(dr["MTRLQTY"]?.ToString(), out decMtrlQty);                                // 잔량
                            blnTankCapa = decimal.TryParse(dr["ELCTRLT_TANK_CAPA"]?.ToString(), out decTankCapa);                    // 탱크 용량
                            blnMtrlTankGrpCapa = decimal.TryParse(dr["MTRL_TANK_GR_CAPA"]?.ToString(), out decMtrlTankGrpCapa);      // 잔량 그룹 총량
                            blnTankGrpCapa = decimal.TryParse(dr["ELCTRLT_TANK_GR_CAPA"]?.ToString(), out decTankGrpCapa);           // 탱크 그룹 총량

                            blnMtrlQtyAlarm = false;
                            blnMtrlTankGrpCapaAlarm = false;

                            #region [ 잔량 체크 ]
                            if (blnMtrlQty && TankLevelAlarmLow > 0)
                            {
                                if (decMtrlQty < TankLevelAlarmLow)
                                {
                                    blnMtrlQtyAlarm = true;
                                }
                            }
                            #endregion

                            #region [ 그룹 총량 체크 ]
                            if (blnTankCapa && blnTankGrpCapa && blnMtrlTankGrpCapa &&
                                decTankCapa > 0 && decTankGrpCapa > 0 &&
                                decTankCapa != decTankGrpCapa && TankGroupLevelAlarmLow > 0)
                            {
                                if (decMtrlTankGrpCapa < decTankGrpCapa * decimal.Divide(TankGroupLevelAlarmLow, (decimal)100)) // Percent
                                {
                                    blnMtrlTankGrpCapaAlarm = true;
                                }
                            }
                            #endregion
                            break;
                    }

                    #region Background Color
                    switch (e.Cell.Column.Name)
                    {
                        case "ELCTRLT_TANK_NAME":
                            break;
                        default:
                            if (blnMtrlQtyAlarm || blnMtrlTankGrpCapaAlarm)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            break;
                    }
                    #endregion

                    #region SetErrorInfo
                    switch (e.Cell.Column.Name)
                    {
                        case "MTRLQTY":
                            try
                            {
                                StringBuilder errorMessage = new StringBuilder();

                                // TANK 잔량이 알람 레벨에 도달했습니다. [%1]Ton / [%2]Ton
                                if (blnMtrlQtyAlarm) errorMessage.AppendLine(MessageDic.Instance.GetMessage("SFU8468", string.Format("{0:#,##0.000}", decMtrlQty), string.Format("{0:#,##0.000}", TankLevelAlarmLow)));
                                // TANK 그룹 잔량이 그룹 LOW LEVEL ALARM값보다 작습니다. [%1]Ton / [%2]Ton
                                if (blnMtrlTankGrpCapaAlarm) errorMessage.AppendLine(MessageDic.Instance.GetMessage("SFU8469", string.Format("{0:#,##0.000}", decMtrlTankGrpCapa), string.Format("{0:#,##0.000}", decTankGrpCapa * decimal.Divide(TankGroupLevelAlarmLow, (decimal)100)), TankGroupLevelAlarmLow));

                                if (errorMessage.Length >= 2)
                                    errorMessage = errorMessage.Replace("\r", "", errorMessage.Length - 2, 1).Replace("\n", "", errorMessage.Length - 1, 1);

                                SetErrorInfo((C1DataGrid)sender, (DataRowView)e?.Cell?.Row?.DataItem, "", errorMessage.ToString());
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            break;
                    }
                    #endregion
                    #endregion
                }
            }));
        }
        /// <summary>
        /// 탱크현황 링크 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTank_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        #endregion

        #region [ Event ] - dgTank, C20210904-000039 [ESWA PI]GMES 시스템 CESS GMES 화면 구성 Added by kimgwango on 2022.01.12
        private void dgTank_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name == null) return;

            dgTank.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                switch (e.Column.Name)
                {
                    // 용량
                    case "ELCTRLT_TANK_CAPA":
                        //if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("㎥"))
                        //{
                        //    e.Column.Header = string.Format("{0}({1})", e.Column.Header, "㎥");
                        //}
                        if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("Ton"))
                        {
                            e.Column.Header = string.Format("{0}({1})", e.Column.Header, "Ton");
                        }
                        break;
                    // 온도
                    case "TMPR":
                        //섭씨온도, 화씨온도 사용여부확인
                        if (ChkTmprKind() == "F")
                        {
                            if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("℉"))
                            {
                                e.Column.Header = string.Format("{0}({1})", e.Column.Header, "℉");
                            }
                        }
                        else
                        {
                            if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("℃"))
                            {
                                e.Column.Header = string.Format("{0}({1})", e.Column.Header, "℃");
                            }
                        }

                        break;
                    // 잔량
                    case "MTRLQTY":
                        if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("Ton"))
                        {
                            e.Column.Header = string.Format("{0}({1})", e.Column.Header, "Ton");
                        }
                        break;
                    // 압력
                    case "PRESS_VALUE":
                        if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("Bar"))
                        {
                            e.Column.Header = string.Format("{0}({1})", e.Column.Header, "Bar");
                        }
                        break;
                }
            }));
        }

        private void dgTank_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
        }

        private void dgTank_UnloadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (sender == null) return;
            if (e.Row == null || e.Row.Presenter == null) return;

            e.Row.Presenter.Foreground = dgTank.Foreground;
        }
        #endregion

        #region [ Event ] - dgTankStat, 탱크상태변경이력 이벤트 : dgTankStatChangeHistory_MouseLeftButtonUp(), dgTankStatChangeHistory_LoadedCellPresenter(), dgTankStatChangeHistory_UnloadedCellPresenter()

        /// <summary>
        /// 탱크현황 리스트 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTankStatChangeHistory_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Column.Name.Equals("ELCTRLT_TANK_NAME"))
                {
                    int rowIdx = cell.Row.Index;
                    DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                    if (drv == null) return;

                    SelDataHistory(DataTableConverter.GetValue(drv, "ELCTRLT_TANK_ID").GetString());

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 탱크현황 링크 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTankStatChangeHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "ELCTRLT_TANK_NAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        /// <summary>
        /// 탱크현황 링크 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTankStatChangeHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        #endregion

        #region [ Event ] - dgTankDataHistory, C20210904-000039 [ESWA PI]GMES 시스템 CESS GMES 화면 구성 Added by kimgwango on 2022.01.12
        private void dgTankDataHistory_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name == null) return;

            dgTankDataHistory.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                switch (e.Column.Name)
                {
                    // 온도
                    case "TMPR":
                        //섭씨온도, 화씨온도 사용여부확인
                        if (ChkTmprKind() == "F")
                        {
                            if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("℉"))
                                e.Column.Header = string.Format("{0}({1})", e.Column.Header, "℉");
                        }
                        else
                        {
                            if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("℃"))
                                e.Column.Header = string.Format("{0}({1})", e.Column.Header, "℃");
                        }


                        break;
                    // 잔량
                    case "MTRLQTY":
                        if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("Ton"))
                            e.Column.Header = string.Format("{0}({1})", e.Column.Header, "Ton");
                        break;
                    // 압력
                    case "PRESS_VALUE":
                        if (!(e.Column.Header == null) && !e.Column.Header.ToString().Contains("Bar"))
                        {
                            e.Column.Header = string.Format("{0}({1})", e.Column.Header, "Bar");
                        }
                        break;
                }
            }));
        }
        #endregion

        #region Method

        #region 탱크상태변경이력 조회 : SelStatChangeHistory()

        /// <summary>
        /// 탱크상태변경이력 조회 
        /// </summary>
        /// <param name="TankID"></param>
        private void SelStatChangeHistory(string TankID)
        {

            const string bizRuleName = "DA_PRD_SEL_ELCTRLT_TANK_STAT_HIST_CL_L";
            ShowLoadingIndicator();
            try
            {

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("ELCTRLT_TANK_ID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["ELCTRLT_TANK_ID"] = TankID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }
                   


                    bizResult.Columns.Add("ACTDTTM_TYPE", typeof(string));

                    int actdttm = bizResult.Columns.IndexOf("ACTDTTM");
                    int actdttm_String = bizResult.Columns.IndexOf("ACTDTTM_TYPE");
                    foreach (DataRow row in bizResult.Rows)
                    {

                        DateTime date = (DateTime)row[actdttm];
                        string dateString = date.ToString("yyyy-MM-dd HH:mm:ss");
                        row[actdttm_String] = dateString;

                    }
                    bizResult.Columns.Remove("ACTDTTM");
                    bizResult.Columns["ACTDTTM_TYPE"].ColumnName = "ACTDTTM";
                    //2024-01-09 Actdttm 날짜형식 오전/오후-> 24시간으로 변경(Datetype->String 타입으로 컬럼 변경

                    Util.GridSetData(dgTankStatChangeHistory, bizResult, null, true);
                    HiddenLoadingIndicator();

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 탱크데이터수집이력 조회 : SelDataHistory()

        /// <summary>
        /// 탱크데이터수집이력 조회 
        /// </summary>
        /// <param name="TankID"></param>
        private void SelDataHistory(string TankID)
        {

            const string bizRuleName = "DA_PRD_SEL_ELCTRLT_TANK_CLCT_HIST_CL_L";
            ShowLoadingIndicator();
            try
            {

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("ELCTRLT_TANK_ID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["ELCTRLT_TANK_ID"] = TankID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgTankDataHistory, bizResult, null, true);
                    HiddenLoadingIndicator();

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 구분 콤보박스 조회 : SetDivisionCombo()
        private static void SetDivisionCombo(C1ComboBox cbo)
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));


            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELCTRLT_TANK_TYPE_COMBO_CL_L", "RQSTDT", "RSLTDT", inTable);

            DataRow dRow = dtResult.NewRow();
            dRow["EL_TANK_TYPE_NAME"] = "-ALL-";
            dRow["EL_TANK_TYPE"] = "";
            dtResult.Rows.InsertAt(dRow, 0);
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;

        }

        #endregion

        #region 데이터그리드 초기화 :  ClearControl()

        /// <summary>
        /// 데이터 그리드 초괴화 
        /// </summary>
        private void ClearControl()
        {
            Util.gridClear(dgTank);
            Util.gridClear(dgTankStatChangeHistory);
            Util.gridClear(dgTankDataHistory);
        }
        #endregion

        #region 프로그래스 바 : ShowLoadingIndicator(),HiddenLoadingIndicator()

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }




        #endregion


        private string ChkTmprKind()
        {

            string bRet = string.Empty;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "AREA_TMPR_KIND";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                bRet = dtResult.Rows[0]["CBO_NAME"].ToString();
            }

            return bRet;
        }


        #endregion

        #region [ Util ]
        public string SetErrorInfo(C1DataGrid grid, DataRowView dv, string columnName, string errorMessage = "")
        {
            C1DataGrid DataGrid;
            DataRow dr;
            StringBuilder error = new StringBuilder();
            StringBuilder errorRow = new StringBuilder();
            string rowName = "ROWERROR";

            if (grid == null) return string.Empty;
            DataGrid = grid;

            if (dv == null && dv.Row == null) return string.Empty;
            dr = (DataRow)dv.Row;

            if (!string.IsNullOrEmpty(columnName)) error.AppendLine(errorMessage);
            else if (!string.IsNullOrEmpty(errorMessage)) errorRow.AppendLine(errorMessage);

            #region Column Error
            if (error.Length >= 2)
                error = error.Replace("\r", "", error.Length - 2, 1).Replace("\n", "", error.Length - 1, 1);

            try
            {
                int idx = DataGrid.Rows.IndexOf(dv);
                if (idx >= 0)
                {
                    foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                    {
                        if (item.ColumnNames.Contains(columnName))
                        {
                            DataGrid.Rows[idx].Errors.Remove(item);
                        }
                    }

                    if (!string.IsNullOrEmpty(error.ToString()))
                    {
                        string colHeader = DataGrid.Columns[columnName].Header.ToString();
                        error.Insert(0, string.Format("[{0}]:", colHeader));
                        DataGrid.Rows[idx].Errors.Add(new DataGridRowError(error.ToString(), new string[] { columnName }));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            #endregion

            #region Row Error
            if (errorRow.Length >= 2)
                errorRow = errorRow.Replace("\r", "", errorRow.Length - 2, 1).Replace("\n", "", errorRow.Length - 1, 1);

            try
            {
                int idx = DataGrid.Rows.IndexOf(dv);
                if (idx >= 0)
                {
                    foreach (var item in DataGrid.Rows[idx].Errors.ToList())
                    {
                        if (item.ColumnNames.Contains(rowName))
                        {
                            DataGrid.Rows[idx].Errors.Remove(item);
                        }
                    }

                    if (!string.IsNullOrEmpty(errorRow.ToString()))
                    {
                        DataGrid.Rows[idx].Errors.Add(new DataGridRowError(errorRow.ToString(), new string[] { rowName }));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            #endregion

            return error.Append(errorRow).ToString();
        }
        #endregion

        #region [ Util ] - DA
        private decimal GetTankLevelAlarmLow()
        {
            decimal result = 0;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "ELCTRLT_TANK_LEVEL_ALARM";
            dr["COM_CODE"] = "TANK_LOW_LEVEL_VALUE";

            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    decimal decAttr1 = 0;
                    string strAttr1 = dtRslt.Rows[0]["ATTR1"]?.ToString();

                    if (decimal.TryParse(strAttr1, out decAttr1))
                    {
                        result = decAttr1;
                    }
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }

            return result;
        }

        private decimal GetTankGroupLevelAlarmLow()
        {
            decimal result = 0;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "ELCTRLT_TANK_LEVEL_ALARM";
            dr["COM_CODE"] = "TANK_GROUP_LOW_LEVEL_VALUE";


            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    decimal decAttr1 = 0;
                    string strAttr1 = dtRslt.Rows[0]["ATTR1"]?.ToString();

                    if (decimal.TryParse(strAttr1, out decAttr1))
                    {
                        result = decAttr1;
                    }
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }

            return result;
        }
        #endregion
    }
}
