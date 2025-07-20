/*************************************************************************************
 Created Date : 2023.10.24
      Creator : 주훈
   Decription : 생산 실적 레포트(소형-월별)
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.10  DEVELOPER : Initial Created.
  2023.10.24  주훈        LGC.GMES.MES.FCS002.FCS002_211 초기 복사
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Linq;


namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_211_MONTH : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        private DataTable _dtHeader;

        public class ResultElement
        {
            public CheckBox chkBox = null;
            public string Title = string.Empty;
            public Control Control;
            public bool Visibility = true;
            public int SpaceInCharge = 1;
        }
        
        System.ComponentModel.BackgroundWorker bgWorker = null;

        Util _Util = new Util();

        #endregion

        #region [Initialize]
        public FCS002_211_MONTH()
        {
            InitializeComponent();

            bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting            
            InitCombo();

            //Control Setting
            InitControl();

            InitSpread();

            //chkAll.Checked += chkAll_Checked;
            //chkAll.Unchecked += chkAll_Unchecked;

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelChild = { cboRoute };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);
        }

        private void InitControl()
        {
            dtpMonth.SelectedDateTime = DateTime.Now;

            _dtHeader = new DataTable();
        }

        private void InitSpread()
        {
            Util.gridClear(dgProdResult); //Grid clear

            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgProdResult.TopRows.Add(HR);
            }

            // FIX
            FixedMultiHeader("주|주", "JOB_WEEK", false, 100);
            FixedMultiHeader("일자|일자", "JOB_DATE", false, 100);

            string[] sColumnName = new string[] { "JOB_WEEK" };
            _Util.SetDataGridMergeExtensionCol(dgProdResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            switch (LoginInfo.CFG_AREA_ID)
            {
                case "MC":      // OC2 Mobile Assy Bldg#2
                default:
                    InitSpread_MC();
                    break;
            }
        }

        private void InitSpread_MC()
        {
            FixedMultiHeader("Washing 투입|Washing 투입", "CNT_FF1A01", false, 100);    // 투입 수량: LowCurrent Inspection #01
            FixedMultiHeader("AGING_1|AGING_1", "CNT_FF3101", false, 100);              // 투입 수량: Norm Temp Aging #01
            FixedMultiHeader("CHARGE|CHARGE", "CNT_FF1101", false, 100);                // 투입 수량: Charge #01
            FixedMultiHeader("AGING_2|AGING_2", "CNT_FF3102", false, 100);              // 투입 수량: Norm Temp Aging #02
            FixedMultiHeader("DISCHARGE|DISCHARGE", "CNT_FF1201", false, 100);          // 투입 수량: DisCharge #01

            FixedMultiHeader("HIGH_AGING|HIGH_AGING", "CNT_FF4101", false, 100);        // 투입 수량: High Temp Aging #01
            FixedMultiHeader("AGING_3|AGING_3", "CNT_FF3103", false, 100);              // 투입 수량: Norm Temp Aging #03
            FixedMultiHeader("PRIVT_OCV|PRIVT_OCV", "CNT_FF8101", false, 100);          // 투입 수량: PRIVT OCV #01
            FixedMultiHeader("AGING_4|AGING_4", "CNT_FF3104", false, 100);              // 투입 수량: Norm Temp Aging #04

            FixedMultiHeader("SHIP_CHARGE|SHIP_CHARGE", "CNT_SHIPCHARGE", false, 100);  // 투입 수량: EOL 전 Charge
            FixedMultiHeader("SHIP_AGING|SHIP_AGING", "CNT_SHIPAGING", false, 100);     // 투입 수량: EOL 전 Aging
            FixedMultiHeader("EOL|EOL", "CNT_FF5101", false, 100);                      // 투입 수량: EOL #01

            FixedMultiHeader("GRADE|A", "OTHER_A", false, 100);                     // OTHER 등급 수량: A
            FixedMultiHeader("GRADE|B", "OTHER_B", false, 100);                     // OTHER 등급 수량: B
            FixedMultiHeader("GRADE|C", "OTHER_C", false, 100);                     // OTHER 등급 수량: C
            FixedMultiHeader("GRADE|D", "OTHER_D", false, 100);                     // OTHER 등급 수량: D
            FixedMultiHeader("GRADE|E", "OTHER_E", false, 100);                     // OTHER 등급 수량: E
            FixedMultiHeader("GRADE|F", "OTHER_F", false, 100);                     // OTHER 등급 수량: F
            FixedMultiHeader("GRADE|G", "OTHER_G", false, 100);                     // OTHER 등급 수량: G
            FixedMultiHeader("GRADE|H", "OTHER_H", false, 100);                     // OTHER 등급 수량: H
            FixedMultiHeader("GRADE|I", "OTHER_I", false, 100);                     // OTHER 등급 수량: I
            FixedMultiHeader("GRADE|J", "OTHER_J", false, 100);                     // OTHER 등급 수량: J
            FixedMultiHeader("GRADE|K", "OTHER_K", false, 100);                     // OTHER 등급 수량: K
            FixedMultiHeader("GRADE|L", "OTHER_L", false, 100);                     // OTHER 등급 수량: L
            FixedMultiHeader("GRADE|M", "OTHER_M", false, 100);                     // OTHER 등급 수량: M
            FixedMultiHeader("GRADE|N", "OTHER_N", false, 100);                     // OTHER 등급 수량: N
            FixedMultiHeader("GRADE|O", "OTHER_O", false, 100);                     // OTHER 등급 수량: O
            FixedMultiHeader("GRADE|P", "OTHER_P", false, 100);                     // OTHER 등급 수량: P
            FixedMultiHeader("GRADE|Q", "OTHER_Q", false, 100);                     // OTHER 등급 수량: Q
            FixedMultiHeader("GRADE|R", "OTHER_R", false, 100);                     // OTHER 등급 수량: R
            FixedMultiHeader("GRADE|S", "OTHER_S", false, 100);                     // OTHER 등급 수량: S
            FixedMultiHeader("GRADE|T", "OTHER_T", false, 100);                     // OTHER 등급 수량: T
            FixedMultiHeader("GRADE|U", "OTHER_U", false, 100);                     // OTHER 등급 수량: U
            FixedMultiHeader("GRADE|V", "OTHER_V", false, 100);                     // OTHER 등급 수량: V
            FixedMultiHeader("GRADE|W", "OTHER_W", false, 100);                     // OTHER 등급 수량: W
            FixedMultiHeader("GRADE|X", "OTHER_X", false, 100);                     // OTHER 등급 수량: X
            FixedMultiHeader("GRADE|Y", "OTHER_Y", false, 100);                     // OTHER 등급 수량: Y
            FixedMultiHeader("GRADE|Z", "OTHER_Z", false, 100);                     // OTHER 등급 수량: Z
        }

        #endregion

        #region [Method]

        private void FixedMultiHeader(string sName, string sBindName, bool bPercent, int iWidth = 75)
        {
            bool bReadOnly = true;
            bool bEditable = false;
            bool bVisible = true;

            string[] sColName = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColName.ToList();

            var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth, bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent);
            dgProdResult.Columns.Add(column_TEXT);
        }

        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header
                                                                         , List<string> Multi_Header
                                                                         , string sName
                                                                         , string sBinding
                                                                         , int iWidth
                                                                         , bool bReadOnly = false
                                                                         , bool bEditable = true
                                                                         , bool bVisible = true
                                                                         , bool bPercent = false
                                                                         , HorizontalAlignment HorizonAlign = HorizontalAlignment.Center
                                                                         , VerticalAlignment VerticalAlign = VerticalAlignment.Center
                                                        )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = HorizonAlign;
            Col.VerticalAlignment = VerticalAlign;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;
            else
                Col.Header = Multi_Header;

            //임시 테이블에 헤더값 저장
            _dtHeader.Columns.Add(sBinding, typeof(string));

            return Col;
        }

        private object GetList(object arg)
        {
            try
            {
                object[] argument = (object[])arg;

                DateTime dMonth = (DateTime)argument[0];
                string EQSGID = argument[1] == null ? null : argument[1].ToString();
                string MDLLOT_ID = argument[2] == null ? null : argument[2].ToString();
                string ROUTEID = argument[3] == null ? null : argument[3].ToString();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MONTH", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTEID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MONTH"] = dMonth.ToString("yyyyMM");
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["ROUTEID"] = ROUTEID;
                dtRqst.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_ALL_GRADE_SUM_MB", "RQSTDT", "RSLTDT", dtRqst);

                SetBottomRow(dtRslt);

                DataSet rtnDataSet = new DataSet();
                DataTable dtResult = dtRslt.DefaultView.ToTable();

                rtnDataSet.Tables.Add(dtResult);

                return rtnDataSet;
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }
        
        /// <summary>
        /// 하단 Summary Row
        /// </summary>
        /// <param name="dt"></param>
        private void SetBottomRow(DataTable dt)
        {
            int iSum = 0;
            int iRow = 0;
            int iCol = 0;
            int iStaCol = 0;
            int iPrvWeek = -1;
            int iMaxRow = 0;
            DataRow dr = dt.NewRow();

            //일자 - "소계"
            iMaxRow = dt.Rows.Count;
            for (iRow = 0; iRow < iMaxRow; iRow++)
            {
                if (iPrvWeek != Util.NVC_Int(dt.Rows[iRow]["JOB_WEEK"]))
                {
                    if (Util.NVC_Int(dr["JOB_WEEK"]) != 0)
                        dt.Rows.Add(dr);

                    dr = dt.NewRow();
                    dr["JOB_MONTH"] = Util.NVC_Int(dt.Rows[iRow]["JOB_MONTH"]);
                    dr["JOB_WEEK"] = Util.NVC_Int(dt.Rows[iRow]["JOB_WEEK"]);
                    dr["JOB_DATE"] = ObjectDic.Instance.GetObjectName("소계");

                    iPrvWeek = Util.NVC_Int(dt.Rows[iRow]["JOB_WEEK"]);
                }

                iStaCol = dt.Columns.IndexOf("CALDATE") + 1;
                for (iCol = iStaCol; iCol < dt.Columns.Count; iCol++)
                {
                    dr[iCol] = Util.NVC_Int(dr[iCol]) +  Util.NVC_Int(dt.Rows[iRow][iCol]);
                }
            }

            if (Util.NVC_Int(dr["JOB_WEEK"]) != 0)
                dt.Rows.Add(dr);

            dt.DefaultView.Sort = "JOB_WEEK, JOB_DATE";

            //주 - "합계"
            dr = dt.NewRow();
            dr["JOB_MONTH"] = Util.NVC_Int(dt.Rows[iRow]["JOB_MONTH"]);
            dr["JOB_WEEK"] = ObjectDic.Instance.GetObjectName("합계");

            for (iRow = 0; iRow < dt.Rows.Count; iRow++)
            {
                if (String.IsNullOrEmpty(dt.Rows[iRow]["CALDATE"].ToString()))
                    continue;

                iStaCol = dt.Columns.IndexOf("CALDATE") + 1;
                for (iCol = iStaCol; iCol < dt.Columns.Count; iCol++)
                {
                    dr[iCol] = Util.NVC_Int(dr[iCol]) + Util.NVC_Int(dt.Rows[iRow][iCol]);
                }
            }

            dt.Rows.Add(dr);
        }

        private static List<ResultElement> CheckBoxList(DataTable dt)
        {
            List<ResultElement> lst = new List<ResultElement>();
            var procList = dt.AsEnumerable().OrderBy(o => o.Field<string>("ATTR1")).ToList();
            int idx = 1;
            foreach (var Item in procList)
            {
                DataRow[] drcominfo = dt.AsEnumerable().Where(f => f.Field<string>("ATTR1").Equals(idx.ToString())).ToArray();

                idx++;

                foreach (DataRow dr in drcominfo)
                {
                    lst.Add(new ResultElement
                    {
                        Title = Util.NVC(dr["COM_CODE_NAME"]),
                        Control = new CheckBox()
                        {
                            Name = "chk" + Util.NVC(dr["COM_CODE"]),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            Content = ObjectDic.Instance.GetObjectName(Util.NVC(dr["COM_CODE_NAME"]))
                        }
                    });
                }
            }
            return lst;
        }
 
        private void SetResult(List<ResultElement> elementList, Grid grid)
        {
            int elementCol = 0;
            grid.SetValue(Grid.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            int colIndex = 0;

            foreach (ResultElement re in elementList)
            {
                if (re.Control != null)
                {
                    re.chkBox = re.Control as CheckBox;
                    re.chkBox.Style = Application.Current.Resources["SearchCondition_CheckBoxStyle"] as Style;
                    re.chkBox.Margin = new Thickness(10, 0, 5, 0);
                    re.chkBox.IsChecked = true;
                    elementCol++;
                    re.chkBox.SetValue(Grid.ColumnProperty, elementCol);
                    re.chkBox.Checked += chk_Checked;
                    re.chkBox.Unchecked += chk_UnChecked;
                    Area.Children.Add(re.chkBox);
                }
                colIndex += re.SpaceInCharge;
            }
            //Search_Status();
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

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            btnSearch.IsEnabled = false;

            object[] argument = new object[5];
            argument[0] = dtpMonth.SelectedDateTime;
            argument[1] = Util.GetCondition(cboLine, bAllNull: true);
            argument[2] = Util.GetCondition(cboModel, bAllNull: true);
            argument[3] = Util.GetCondition(cboRoute, bAllNull: true);

            xProgress.Percent = 0;
            xProgress.ProgressText = string.Empty;
            xProgress.Visibility = Visibility.Visible;

            if (!bgWorker.IsBusy)
            {
                bgWorker.RunWorkerAsync(argument);
            }
        }

        private void chk_Checked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            if (chk.Name == "chkAll")
            {

            }
            else if (chk.Name == "chkSELECTOR")
            {
                for (int i = dgProdResult.Columns["SELECTOR_IN"].Index; i <= dgProdResult.Columns["SELECTOR_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkSELECTORRE")
            {
                for (int i = dgProdResult.Columns["SELECTOR_REWORK_IN"].Index; i <= dgProdResult.Columns["SELECTOR_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_IN"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Visible;
                }
            }
        }

        private void chk_UnChecked(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            if (chk.Name == "chkAll")
            {

            }
            else if (chk.Name == "chkSELECTOR")
            {
                for (int i = dgProdResult.Columns["SELECTOR_IN"].Index; i <= dgProdResult.Columns["SELECTOR_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkSELECTORRE")
            {
                for (int i = dgProdResult.Columns["SELECTOR_REWORK_IN"].Index; i <= dgProdResult.Columns["SELECTOR_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkEOL")
            {
                for (int i = dgProdResult.Columns["EOL_IN"].Index; i <= dgProdResult.Columns["EOL_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkEOLRE")
            {
                for (int i = dgProdResult.Columns["EOL_REWORK_IN"].Index; i <= dgProdResult.Columns["EOL_REWORK_LOSS"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
            else if (chk.Name == "chkINSP_REQ")
            {
                for (int i = dgProdResult.Columns["INSP_REQ_LQC"].Index; i <= dgProdResult.Columns["INSP_REQ_PQC"].Index; i++)
                {
                    dgProdResult.Columns[i].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = true;
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < Area.Children.Count; idx++)
            {
                CheckBox chk = Area.Children[idx] as CheckBox;
                chk.IsChecked = false;
            }
        }
        
        private void dgProdResult_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index != dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgProdResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "JOB_DATE")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("소계"))))
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                }

                if (e.Cell.Row.Index == dg.Rows.Count - 1)  // 합계
                {
                    //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));                    
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                }
            }));
        }

        private void BgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bgWorker.ReportProgress(30);
            e.Result = GetList(e.Argument);
            bgWorker.ReportProgress(60);
        }

        private void BgWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            xProgress.Percent = e.ProgressPercentage;
            xProgress.ProgressText = e.UserState == null ? "" : e.UserState.ToString();
        }

        private void BgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result is Exception)
                {
                    Util.MessageException((Exception)e.Result);
                }
                else if (e.Result != null && e.Result is DataSet)
                {
                    DataSet dsData = (DataSet)e.Result;

                    if (dsData != null)
                    {
                        if (dsData.Tables.Contains("RSLTDT")) dgProdResult.ItemsSource = DataTableConverter.Convert(dsData.Tables["RSLTDT"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            xProgress.Visibility = Visibility.Collapsed;
            xProgress.Percent = 0;

            btnSearch.IsEnabled = true;
        }

        #endregion

    }
}
