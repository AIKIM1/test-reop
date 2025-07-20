/*************************************************************************************
 Created Date : 2017.12.13
      Creator : 
   Decription : 자주검사 (파우치 전용)
--------------------------------------------------------------------------------------
 [Change History] 2018.01.11 Degasing은 자주검사 형태가 틀림,  탭추가

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_FORM_QUALITY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_FORM_QUALITY : C1Window, IWorkArea
    {
        #region Declaration
        public UcPolymerFormShift UcPolymerFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _lineID = string.Empty;        // 라인코드
        private string _wipSeq = string.Empty;
        private string _actDttm = string.Empty;
        int _maxColumn = 0;
        DataTable _clctItem;

        private bool _load = true;
        private bool _TimeChange = false;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }

        public string Update_YN { get; set; }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public CMM_FORM_QUALITY()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();
                SetGridColumn();
                _load = false;
            }
        }

        private void InitializeUserControls()
        {
            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
        }

        private void SetControl()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = Util.NVC(tmps[0]);
            _lineID = Util.NVC(tmps[2]);
            _eqptID = Util.NVC(tmps[3]);
            _wipSeq = Util.NVC(tmps[6]);

            // SET COMMON
            txtEquipment.Text = Util.NVC(tmps[4]);
            txtLotID.Text = Util.NVC(tmps[5]);

            if (Update_YN == null || Update_YN.Equals("N"))
            {
                txtProcess.Text = Util.NVC(tmps[1]);
            }
            else
            {
                SelectProcessName();
            }

            // 자주검사 주기 콤보 조회
            SetQualityTime();
            cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

            if (Update_YN == "Y")
            {
                // 검사 주기에 현재 일자 + 시간으로 구성됨
                _actDttm = Util.NVC(tmps[7]) + " " + Util.NVC(tmps[8]) + ":00";

                if (cboQualityTime.Items.Count > 0 && !string.IsNullOrEmpty(tmps[8].GetString()))
                {
                    DataTable dtQualityTime = DataTableConverter.Convert(cboQualityTime.ItemsSource);
                    var query = (from t in dtQualityTime.AsEnumerable()
                                 where t.Field<string>("CBO_NAME") == tmps[8].GetString()
                                 select new
                                 {
                                     code = t.Field<string>("CBO_CODE")
                                 }).FirstOrDefault();
                    if (query != null)
                        cboQualityTime.SelectedValue = query.code;
                }
            }

            // 자주검사 그룹 콤보 조회
            SetQualityGroup();
            cboQualityGroup.SelectedValueChanged += cboQualityGroup_SelectedValueChanged;

            if (Update_YN == "Y")
            {
                if (cboQualityGroup.Items.Count > 0 && !string.IsNullOrEmpty(tmps[9].GetString()))
                {
                    DataTable dtQualityGroup = DataTableConverter.Convert(cboQualityGroup.ItemsSource);
                    var query = (from t in dtQualityGroup.AsEnumerable()
                                 where t.Field<string>("CBO_CODE") == tmps[9].GetString()
                                 select new
                                 {
                                     code = t.Field<string>("CBO_CODE")
                                 }).FirstOrDefault();
                    if (query != null)
                        cboQualityGroup.SelectedValue = query.code;
                }
            }

            // 자주검사 항목 조회
            if (_procID.Equals(Process.DEGASING))
            {
                GetGQualityInfoDeg();
            }
            else
            {
                GetGQualityInfo();
            }

            // 작업자, 작업조
            UcPolymerFormShift.TextShift.Tag = ShiftID;
            UcPolymerFormShift.TextShift.Text = ShiftName;
            UcPolymerFormShift.TextWorker.Tag = WorkerID;
            UcPolymerFormShift.TextWorker.Text = WorkerName;
            UcPolymerFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
            if (UcPolymerFormShift != null)
            {
                UcPolymerFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }

        private void SetControlVisibility()
        {
            if (_procID.Equals(Process.DEGASING))
            {
                tiQuality.Visibility = Visibility.Collapsed;

                // 검사그룹, CLCT_COUNT
                tbQualityGroup.Visibility = Visibility.Collapsed;
                cboQualityGroup.Visibility = Visibility.Collapsed;
                tbQualityClctCnt.Visibility = Visibility.Collapsed;
                cboQualityClctCnt.Visibility = Visibility.Collapsed;
            }
            else
            {
                tiQualityDeg.Visibility = Visibility.Collapsed;
            }
        }

        private void SetGridColumn()
        {
            tiQuality.Header = txtProcess.Text;
            tiQualityDeg.Header = txtProcess.Text;
        }

        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateQualitySave())
                return;

            // 검사 결과를 저장 하시겠습니까?
            Util.MessageConfirm("SFU2811", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (((System.Windows.FrameworkElement)tabQuality.SelectedItem).Name.Equals("tiQuality"))
                    {
                        QualitySave();
                    }
                    else
                    {
                        QualitySaveDeg();
                    }
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [자주검사 : dgQuality_LoadedCellPresenter, dgQuality_UnloadedCellPresenter]
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2);
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

                    double LSL = 0;
                    double USL = 0;
                    double InputValue = 0;
                    bool InputCheck = true;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL")), out LSL))
                        LSL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL")), out USL))
                        USL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)), out InputValue))
                        InputValue = 0;

                    // Spec에 자주검사등록제외
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SELF_INSP_REG_EXCL_FLAG")).Equals("Y"))
                        InputCheck = false;

                    if (int.TryParse(ColName, out ColCnt))
                    {
                        if (ColCnt > MaxCount)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                            
                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                        n.IsEnabled = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;

                                        if (LSL != 0 || USL != 0)
                                        {
                                            if (InputValue < LSL || InputValue > USL)
                                            {
                                                n.Foreground = new SolidColorBrush(Colors.Red);
                                                n.FontWeight = FontWeights.Bold;
                                            }
                                        }

                                        if (InputCheck)
                                        {
                                            n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                            n.IsEnabled = true;
                                        }
                                        else
                                        {
                                            n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                            n.IsEnabled = false;
                                        }

                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                        if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                        if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                            {
                                if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                {
                                    C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else if (panel.Children[cnt].GetType().Name == "TextBox")
                                {
                                    TextBox n = panel.Children[cnt] as TextBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                                else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                {
                                    ComboBox n = panel.Children[cnt] as ComboBox;
                                    n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                    n.IsEnabled = true;
                                }
                            }
                        }
                    }
                }

            }));

        }

        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));

                        if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                        {
                            if (e.Cell.Presenter == null || e.Cell.Presenter.Content == null) return;

                            if (e.Cell.Presenter.Content.GetType() != typeof(StackPanel)) return;

                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                if (panel.Children[cnt].Visibility == Visibility.Visible)
                                {
                                    if (panel.Children[cnt].GetType().Name == "C1NumericBox")
                                    {
                                        C1NumericBox n = panel.Children[cnt] as C1NumericBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "TextBox")
                                    {
                                        TextBox n = panel.Children[cnt] as TextBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                    else if (panel.Children[cnt].GetType().Name == "ComboBox")
                                    {
                                        ComboBox n = panel.Children[cnt] as ComboBox;
                                        n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                        n.IsEnabled = true;
                                    }
                                }
                            }
                        }                            
                    }
                }
            }));
        }

        #endregion

        #region [Degasing 자주검사 : dgQuality_LoadedCellPresenter, dgQuality_UnloadedCellPresenter]
        private void dgQualityDeg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.IndexOf("CLCTVAL") >= 0)
                {
                    double LSL = 0;
                    double USL = 0;
                    double InputValue = 0;
                    bool InputCheck = true;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL")), out LSL))
                        LSL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL")), out USL))
                        USL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)), out InputValue))
                        InputValue = 0;

                    // Spec에 자주검사등록제외
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SELF_INSP_REG_EXCL_FLAG")).Equals("Y"))
                        InputCheck = false;

                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].GetType().Name == "TextBox")
                        {
                            TextBox n = panel.Children[cnt] as TextBox;

                            if (LSL != 0 || USL != 0)
                            {
                                if (InputValue < LSL || InputValue > USL)
                                {
                                    n.Foreground = new SolidColorBrush(Colors.Red);
                                    n.FontWeight = FontWeights.Bold;
                                }
                            }

                            if (InputCheck)
                            {
                                n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                n.IsEnabled = true;
                            }
                            else
                            {
                                n.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                                n.IsEnabled = false;
                            }
                        }
                    }
                }

            }));

        }

        private void dgQualityDeg_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
            //        {
            //            e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
            //        }
            //    }
            //}));
        }

        #endregion

        #region [검사 주기, 검사그룹 변경: cboQualityTime_SelectedValueChanged, cboQualityGroup_SelectedValueChanged]
        private void cboQualityTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            _TimeChange = true;

            if (((System.Windows.FrameworkElement)tabQuality.SelectedItem).Name.Equals("tiQuality"))
            {
                GetGQualityInfo();
            }
            else
            {
                GetGQualityInfoDeg();
            }

            _TimeChange = false;
        }

        private void cboQualityGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (((System.Windows.FrameworkElement)tabQuality.SelectedItem).Name.Equals("tiQuality"))
            {
                GetGQualityInfo();
            }
            else
            {
                GetGQualityInfoDeg();
            }
        }

        private void cboQualityClctCnt_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dgQuality?.Columns == null || dgQuality.Columns.Count < 1) return;

                if (Util.NVC(cboQualityClctCnt.SelectedValue).Equals(""))
                {
                    int startcol = dgQuality.Columns["ACTDTTM"].Index;
                    int forCount = 0;

                    for (int col = startcol + 1; col < dgQuality.Columns.Count; col++)
                    {
                        forCount++;

                        if (forCount > _maxColumn)
                            dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                        else
                            dgQuality.Columns[col].Visibility = Visibility.Visible;
                    }

                    dgQuality.AlternatingRowBackground = null;
                }
                else
                {
                    int startcol = dgQuality.Columns["ACTDTTM"].Index;

                    for (int col = startcol + 1; col < dgQuality.Columns.Count; col++)  // CLCTVAL01
                    {
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    }

                    if (dgQuality.Columns.Contains("CLCTVAL" + Util.NVC(cboQualityClctCnt.SelectedValue)))
                        dgQuality.Columns["CLCTVAL" + Util.NVC(cboQualityClctCnt.SelectedValue)].Visibility = Visibility.Visible;
                    
                    dgQuality.AlternatingRowBackground = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                txtProcess.Text = CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0]["PROCNAME"].ToString() : string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 검사 시간
        /// </summary>
        private void SetQualityTime()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_TIME_CBO", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityTime = new DataTable();
                dtQualityTime.Columns.Add(cboQualityTime.DisplayMemberPath, typeof(string));
                dtQualityTime.Columns.Add(cboQualityTime.SelectedValuePath, typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dtResult.Rows[0]["START_HHMMDD"].ToString()))
                    {
                        DateTime dtStart = Convert.ToDateTime(dtResult.Rows[0]["NOW_YMD"] + " " + dtResult.Rows[0]["START_HHMMDD"]);
                        DateTime dtEnd = Convert.ToDateTime(dtResult.Rows[0]["NXT_YMD"] + " " + dtResult.Rows[0]["START_HHMMDD"]);
                        DateTime dtCombo = dtStart;

                        int itvl = Util.NVC_Int(dtResult.Rows[0]["CLCT_ITVL"].ToString());

                        for (int loop = 0; loop < 999; loop++)
                        {
                            DataRow dr = dtQualityTime.NewRow();
                            dr[cboQualityTime.DisplayMemberPath] = dtCombo.ToString("HH:mm");
                            dr[cboQualityTime.SelectedValuePath] = dtCombo.ToString("yyyy-MM-dd HH:mm:ss");
                            dtQualityTime.Rows.Add(dr);

                            dtCombo = dtCombo.AddHours(itvl);

                            if (dtEnd <= dtCombo)
                                break;

                        }
                    }
                }

                DataRow drSelect = dtQualityTime.NewRow();
                drSelect[cboQualityTime.DisplayMemberPath] = "-SELECT-";
                drSelect[cboQualityTime.SelectedValuePath] = "SELECT";
                dtQualityTime.Rows.InsertAt(drSelect, 0);

                cboQualityTime.DisplayMemberPath = cboQualityTime.DisplayMemberPath;
                cboQualityTime.SelectedValuePath = cboQualityTime.SelectedValuePath;
                cboQualityTime.ItemsSource = dtQualityTime.Copy().AsDataView();
                cboQualityTime.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetQualityGroup()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_GRP_CBO_CMM", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityGroup = new DataTable();
                dtQualityGroup.Columns.Add(cboQualityGroup.DisplayMemberPath, typeof(string));
                dtQualityGroup.Columns.Add(cboQualityGroup.SelectedValuePath, typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtQualityGroup = dtResult.Copy();
                }

                DataRow drSelect = dtQualityGroup.NewRow();
                drSelect[cboQualityGroup.DisplayMemberPath] = "-ALL-";
                drSelect[cboQualityGroup.SelectedValuePath] = "";
                dtQualityGroup.Rows.InsertAt(drSelect, 0);

                cboQualityGroup.DisplayMemberPath = cboQualityGroup.DisplayMemberPath;
                cboQualityGroup.SelectedValuePath = cboQualityGroup.SelectedValuePath;
                cboQualityGroup.ItemsSource = dtQualityGroup.Copy().AsDataView();
                cboQualityGroup.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetQualityClctCount(int iMax)
        {
            try
            {
                DataTable dtClctCount = new DataTable();
                dtClctCount.Columns.Add(cboQualityClctCnt.DisplayMemberPath, typeof(string));
                dtClctCount.Columns.Add(cboQualityClctCnt.SelectedValuePath, typeof(string));

                for (int i = 0; i < iMax; i++)
                {
                    DataRow newRow = dtClctCount.NewRow();
                    newRow[cboQualityClctCnt.DisplayMemberPath] = (i + 1).ToString();
                    newRow[cboQualityClctCnt.SelectedValuePath] = (i + 1).ToString("00");

                    dtClctCount.Rows.Add(newRow);
                }

                DataRow drSelect = dtClctCount.NewRow();
                drSelect[cboQualityClctCnt.DisplayMemberPath] = "-ALL-";
                drSelect[cboQualityClctCnt.SelectedValuePath] = "";
                dtClctCount.Rows.InsertAt(drSelect, 0);

                cboQualityClctCnt.DisplayMemberPath = cboQualityClctCnt.DisplayMemberPath;
                cboQualityClctCnt.SelectedValuePath = cboQualityClctCnt.SelectedValuePath;
                cboQualityClctCnt.ItemsSource = dtClctCount.Copy().AsDataView();
                cboQualityClctCnt.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetGQualityInfo()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQuality);
                return;
            }

            try
            {
                string sBizName = "DA_QCA_SEL_SELF_INSP_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["CLCTITEM_CLSS4"] = Util.NVC(cboQualityGroup.SelectedValue).Equals("") ? null: Util.NVC(cboQualityGroup.SelectedValue);
                newRow["LOTID"] = txtLotID.Text;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTDTTM"] = Update_YN.Equals("Y") ? _actDttm : cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();
                newRow["EQPTID"] = Util.NVC(_eqptID);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQuality, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                int startcol = dgQuality.Columns["ACTDTTM"].Index;
                int forCount = 0;

                for (int col = startcol + 1; col < dgQuality.Columns.Count; col++)
                {
                    forCount++;

                    if (forCount > _maxColumn)
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                    else
                        dgQuality.Columns[col].Visibility = Visibility.Visible;
                }

                dgQuality.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME4", "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQuality, sColumnName, DataGridMergeMode.VERTICAL);


                // 차수 콤보 설정
                cboQualityClctCnt.SelectedValueChanged -= cboQualityClctCnt_SelectedValueChanged;
                SetQualityClctCount(_maxColumn);
                cboQualityClctCnt.SelectedValueChanged += cboQualityClctCnt_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetGQualityInfoDeg()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQualityDeg);
                return;
            }

            try
            {
                #region DEG 자주거검사 등록시 검사코드  저장시 매핑, 해더 컨트롤용
                _clctItem = new DataTable();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["CLCTITEM_CLSS4"] = Util.NVC(cboQualityGroup.SelectedValue).Equals("") ? null : Util.NVC(cboQualityGroup.SelectedValue);
                inTable.Rows.Add(newRow);

                _clctItem = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_DEG_CLCTITEM", "RQSTDT", "RSLTDT", inTable);
                if (_clctItem == null || _clctItem.Rows.Count == 0)
                    return;

                #endregion

                #region DEG 자주거검사 등록시 LIST
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));

                inTable.Rows[0]["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows[0]["LOTID"] = txtLotID.Text;
                inTable.Rows[0]["WIPSEQ"] = _wipSeq;
                inTable.Rows[0]["ACTDTTM"] = Update_YN.Equals("Y") ? _actDttm : cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_DEG_LIST", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQualityDeg, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                #endregion

                #region 그리드 header Setting

                DataTable dt = _clctItem.DefaultView.ToTable(true, "CLCTITEM_CLSS_NAME3");

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = Util.NVC_Int(_clctItem.Rows[0]["COLUMN_COUNNT"]);

                int startcol = dgQualityDeg.Columns["ACTDTTM"].Index;
                int forCount = 0;

                for (int col = startcol + 1; col < dgQualityDeg.Columns.Count; col++)
                {
                    forCount++;

                    if (forCount > _maxColumn)
                    {
                        dgQualityDeg.Columns[col].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgQualityDeg.Columns[col].Visibility = Visibility.Visible;
                        dgQualityDeg.Columns[col].Header = dt.Rows[forCount-1]["CLCTITEM_CLSS_NAME3"].ToString();
                        dgQualityDeg.Columns[col].Tag = dt.Rows[forCount - 1]["CLCTITEM_CLSS_NAME3"].ToString();
                    }
                }

                dgQualityDeg.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME4", "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQualityDeg, sColumnName, DataGridMergeMode.VERTICAL);

                #endregion

                // 차수 콤보 설정
                cboQualityClctCnt.SelectedValueChanged -= cboQualityClctCnt_SelectedValueChanged;
                SetQualityClctCount(_maxColumn);
                cboQualityClctCnt.SelectedValueChanged += cboQualityClctCnt_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void QualitySave()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));

                for (int col = 0; col < _maxColumn; col++)
                {
                    colName = "CLCTVAL" + (col + 1).ToString("00");
                    inTable.Columns.Add(colName, typeof(string));
                }
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = txtLotID.Text;
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;
                    newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = Update_YN.Equals("Y") ? _actDttm : cboQualityTime.SelectedValue.ToString();   // cboQualityTime.SelectedValue.ToString();

                    int colValue = 0;

                    for (int col = dgQuality.Columns.Count - _maxColumn; col < dgQuality.Columns.Count; col++)
                    {
                        colValue++;

                        if (colValue > _maxColumn)
                            break;

                        colName = "CLCTVAL" + colValue.ToString("00");

                        //newRow[colName] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, colName));
                        DataRowView drvTmp = (dRow.DataItem as DataRowView);
                        newRow[colName] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals("-")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";// DataTableConverter.GetValue(row.DataItem, "CLCTVAL01");
                    }
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (!Update_YN.Equals("Y"))
                        {
                            Util.MessageInfo("SFU1270");      //저장되었습니다.
                        }

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void QualitySaveDeg()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));
                inTable.Columns.Add("CLCTVAL01", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgQualityDeg.Rows)
                {
                    if (!dRow.Type.Equals(DataGridRowType.Item))
                        continue;

                    int colValue = 0;

                    for (int col = dgQualityDeg.Columns.Count - 20; col < dgQualityDeg.Columns.Count; col++)
                    {
                        colValue++;
                        colName = "CLCTVAL" + colValue.ToString("00");

                        if (colValue > _maxColumn)
                            break;

                        DataRow[] dr = _clctItem.Select("CLCTITEM_CLSS_NAME4 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME4")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME1 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME1")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME2 = '" + Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CLCTITEM_CLSS_NAME2")) + "' And "
                                                      + "CLCTITEM_CLSS_NAME3 = '" + dgQualityDeg.Columns[col].Tag.ToString() + "'" );

                        if (dr.Length > 0)
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            newRow["LOTID"] = txtLotID.Text;
                            newRow["WIPSEQ"] = _wipSeq;
                            newRow["EQPTID"] = _eqptID;
                            newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                            newRow["USERID"] = LoginInfo.USERID;
                            //newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                            newRow["CLCTITEM"] = Util.NVC(dr[0]["INSP_CLCTITEM"]);
                            newRow["ACTDTTM"] = Update_YN.Equals("Y") ? _actDttm : cboQualityTime.SelectedValue.ToString();   // cboQualityTime.SelectedValue.ToString();

                            DataRowView drvTmp = (dRow.DataItem as DataRowView);
                            newRow["CLCTVAL01"] = (!drvTmp[colName].Equals(DBNull.Value) && !drvTmp[colName].Equals(" - ")) ? drvTmp[colName].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
                            inTable.Rows.Add(newRow);
                        }
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (!Update_YN.Equals("Y"))
                        {
                            Util.MessageInfo("SFU1270");      //저장되었습니다.
                        }

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateQualitySave()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                // 검사 주기를 선택 하세요.
                Util.MessageValidation("SFU4024");
                return false;
            }

            ////if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            ////{
            ////    // 작업조를 입력해 주세요.
            ////    Util.MessageValidation("SFU1845");
            ////    return false;
            ////}

            ////if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            ////{
            ////    // 작업자를 입력 해 주세요.
            ////    Util.MessageValidation("SFU1843");
            ////    return false;
            ////}

            return true;
        }
        #endregion

        #region [Func]

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcPolymerFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcPolymerFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcPolymerFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcPolymerFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcPolymerFormShift.TextShiftEndTime.Text))
                                {
                                    UcPolymerFormShift.TextShiftDateTime.Text = UcPolymerFormShift.TextShiftStartTime.Text + " ~ " + UcPolymerFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcPolymerFormShift.TextWorker.Text = string.Empty;
                                    UcPolymerFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcPolymerFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcPolymerFormShift.TextShift.Tag = string.Empty;
                                    UcPolymerFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcPolymerFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcPolymerFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcPolymerFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = _lineID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (System.Windows.Controls.Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
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

        #endregion

        private void CLCTVAL_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > ++rIdx)
                    {
                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;

                        if (p?.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > ++rIdx)
                    {
                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (grid.GetRowCount() - 1 > rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }

                        if (p?.Content == null) return;
                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.GetRowCount() + grid.TopRows.Count > --rIdx)
                    {
                        if (rIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (rIdx > 0)
                        {
                            grid.ScrollIntoView(rIdx - 1, cIdx);
                        }

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.Columns.Count > ++cIdx)
                    {
                        if (cIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (grid.Columns.Count - 1 > cIdx)
                        {
                            grid.ScrollIntoView(rIdx, cIdx+1);
                        }

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    int rIdx = 0;
                    int cIdx = 0;
                    C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        C1NumericBox n = sender as C1NumericBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "TextBox")
                    {
                        if (InputMethod.Current != null)
                            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                        TextBox n = sender as TextBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox n = sender as ComboBox;
                        StackPanel panel = n.Parent as StackPanel;
                        DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;
                        rIdx = p.Cell.Row.Index;
                        cIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (grid.Columns.Count > --cIdx)
                    {
                        if (cIdx < 0)
                        {
                            e.Handled = true;
                            return;
                        }

                        DataGridCellPresenter p = grid.GetCell(rIdx, cIdx).Presenter;
                        if (cIdx > 0)
                        {
                            grid.ScrollIntoView(rIdx, cIdx - 1);
                        }

                        if (p == null || p.Content == null) return;
                        if (p.Content.GetType() != typeof(StackPanel)) return;

                        StackPanel panel = p.Content as StackPanel;

                        for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                        {
                            if (panel.Children[cnt].Visibility == Visibility.Visible)
                                panel.Children[cnt].Focus();
                        }
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        private void CLCTVAL_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;

                if (sender.GetType().Name == "TextBox")
                {
                    TextBox n = sender as TextBox;
                    n.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CLCTVAL_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_TimeChange) return;

                if (sender.GetType().Name == "TextBox")
                {
                    TextBox n = sender as TextBox;
                    StackPanel panel = n.Parent as StackPanel;
                    DataGridCellPresenter p = panel.Parent as DataGridCellPresenter;

                    double LSL = 0;
                    double USL = 0;
                    double MIN = 0;
                    double MAX = 0;
                    double InputValue = 0;

                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(p.Row.DataItem, "LSL")), out LSL))
                        LSL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(p.Row.DataItem, "USL")), out USL))
                        USL = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(p.Row.DataItem, "MIN_VALUE")), out MIN))
                        MIN = 0;
                    if (!double.TryParse(Util.NVC(DataTableConverter.GetValue(p.Row.DataItem, "MAX_VALUE")), out MAX))
                        MAX = 0;
                    if (!double.TryParse(n.Text, out InputValue))
                        InputValue = 0;

                    // 하한, 상한 체크
                    if (LSL != 0 || USL != 0)
                    {
                        if (InputValue < LSL || InputValue > USL)
                        {
                            n.Foreground = new SolidColorBrush(Colors.Red);
                            n.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            n.Foreground = new SolidColorBrush(Colors.Black);
                            n.FontWeight = FontWeights.Normal;
                        }
                    }

                    // Min, Max 체크
                    if (MIN != 0 || MAX != 0)
                    {
                        if (InputValue < MIN)
                        {
                            // 입력값이 기준치의 최소값 보다 작습니다
                            Util.MessageConfirm("SFU1804", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    n.Focus();
                                }
                            });
                        }
                        if (InputValue > MAX)
                        {
                            // 입력값이 기준치의 최대값 보다 큽니다
                            Util.MessageConfirm("SFU1803", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    n.Focus();
                                }
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void cbVal_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo != null && cbo.IsVisible)
                cbo.SelectedIndex = 0;
        }

    }
}
