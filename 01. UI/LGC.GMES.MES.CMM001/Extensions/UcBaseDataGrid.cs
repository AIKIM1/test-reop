/*************************************************************************************
 Created Date : 2022.08.08
      Creator : 
   Decription : C1DataGrid Extension
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.08  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Excel;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;


namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseDataGrid : C1DataGrid, IWorkArea, IControlValidation
    {
        #region Internal Class

        public class ExecuteDataEventArgs : EventArgs
        {
            public ExecuteDataEventArgs(object inData, object resultData, object arguments, Exception ex)
            {
                InData = inData;
                ResultData = resultData;
                Arguments = arguments;
                Exception = ex;
            }

            public object InData { get; set; }
            public object ResultData { get; set; }
            public object Arguments { get; set; }
            public Exception Exception { get; set; }
        }

        public class UserConfigInformation
        {
            public UserConfigInformation(C1.WPF.DataGrid.DataGridColumn column)
            {
                ColumnName = column.Name;
                DisplayIndex = column.DisplayIndex;
                Visibility = column.Visibility;
                //Width = column.Width.Value;
                //UnitType = column.Width.Type;
                Width = column.ActualWidth;
                UnitType = DataGridUnitType.Pixel;
            }

            public UserConfigInformation(string columnName, int displayIndex, Visibility visibility, double width, DataGridUnitType unitType)
            {
                ColumnName = columnName;
                DisplayIndex = displayIndex;
                Visibility = visibility;
                Width = width;
                UnitType = unitType;
            }

            public string ColumnName { get; set; }
            public int DisplayIndex { get; set; }
            public System.Windows.Visibility Visibility { get; set; }
            public double Width { get; set; }
            public DataGridUnitType UnitType { get; set; }

        }

        public class DataGridAggregateText : DataGridAggregateWithFormat
        {
            private string textColumn = string.Empty;

            public DataGridAggregateText(string columnText)
            {
                textColumn = columnText;
            }

            public override object Compute(DataGridRowCollection rows, C1.WPF.DataGrid.DataGridColumn column, bool recursive)
            {
                return ObjectDic.Instance.GetObjectName(textColumn);
            }

        }

        public class DataGridAggregateRatio : DataGridAggregateWithFormat
        {
            private C1.WPF.DataGrid.DataGridColumn totalColumn = null;
            private C1.WPF.DataGrid.DataGridColumn partColumn = null;

            private List<C1.WPF.DataGrid.DataGridColumn> totalColumns = null;
            private List<C1.WPF.DataGrid.DataGridColumn> partColumns = null;

            public DataGridAggregateRatio(C1.WPF.DataGrid.DataGridColumn columnTotal, C1.WPF.DataGrid.DataGridColumn columnPart)
            {
                totalColumn = columnTotal;
                partColumn = columnPart;
            }

            public DataGridAggregateRatio(List<C1.WPF.DataGrid.DataGridColumn> columnTotals, List<C1.WPF.DataGrid.DataGridColumn> columnParts)
            {
                totalColumns = columnTotals;
                partColumns = columnParts;
            }

            public override object Compute(DataGridRowCollection rows, C1.WPF.DataGrid.DataGridColumn column, bool recursive)
            {
                double totalValue = 0.0;
                double partValue = 0.0;
                double returnValue = 0.0;

                foreach (C1.WPF.DataGrid.DataGridRow item in rows.AsEnumerable((C1.WPF.DataGrid.DataGridRow gridRow) => gridRow.Type == DataGridRowType.Item, (DataGridGroupRow gridRow) => recursive))
                {
                    try
                    {
                        if (totalColumns != null || partColumns != null)
                        {
                            foreach (C1.WPF.DataGrid.DataGridColumn col in totalColumns)
                            {
                                double valueTotal = Convert.ToDouble(col.GetCellValue(item));
                                if (!double.IsNaN(valueTotal) && !double.IsInfinity(valueTotal)) totalValue += valueTotal;
                            }

                            foreach (C1.WPF.DataGrid.DataGridColumn col in partColumns)
                            {
                                double valuePart = Convert.ToDouble(col.GetCellValue(item));
                                if (!double.IsNaN(valuePart) && !double.IsInfinity(valuePart)) partValue += valuePart;
                            }
                        }
                        else
                        {
                            double valueTotal = Convert.ToDouble(totalColumn.GetCellValue(item));
                            if (!double.IsNaN(valueTotal) && !double.IsInfinity(valueTotal)) totalValue += valueTotal;

                            double valuePart = Convert.ToDouble(partColumn.GetCellValue(item));
                            if (!double.IsNaN(valuePart) && !double.IsInfinity(valuePart)) partValue += valuePart;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

                if (totalValue == 0) return 0;

                returnValue = partValue / totalValue * 100;

                if (column is C1.WPF.DataGrid.DataGridTextColumn)
                {
                    C1.WPF.DataGrid.DataGridTextColumn textColumn = column as C1.WPF.DataGrid.DataGridTextColumn;
                    if (!string.IsNullOrEmpty(textColumn.Format) && textColumn.Format.Substring(0, 1) == "P")
                    {
                        returnValue = returnValue / 100;
                    }
                }

                return returnValue;
            }

        }

        public class DataGridAggregateEven : DataGridAggregateWithFormat
        {
            public DataGridAggregateEven()
            {
            }

            public override object Compute(DataGridRowCollection rows, C1.WPF.DataGrid.DataGridColumn column, bool recursive)
            {
                double totalValue = 0.0;
                int count = 0;
                double returnValue = 0.0;

                foreach (C1.WPF.DataGrid.DataGridRow item in rows.AsEnumerable((C1.WPF.DataGrid.DataGridRow gridRow) => gridRow.Type == DataGridRowType.Item, (DataGridGroupRow gridRow) => recursive))
                {
                    try
                    {
                        double valueItem = Convert.ToDouble(column.GetCellValue(item));
                        if (!double.IsNaN(valueItem) && !double.IsInfinity(valueItem) && valueItem != 0)
                        {
                            totalValue += valueItem;
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }

                if (totalValue == 0 || count == 0) return 0;

                returnValue = totalValue / count;

                return returnValue;
            }

        }

        #endregion

        #region Internal Extension

        #endregion

        #region EventHandler
        public event RowIndexChangedEventHandler RowIndexChanged;
        /// <summary>
        /// 로우 변경시 발생하는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="beforeRow">이전 로우</param>
        /// <param name="currentRow">현재 로우</param>
        public delegate void RowIndexChangedEventHandler(object sender, int beforeRow, int currentRow);

        public event ExecuteDataDoWorkEventHandler ExecuteDataDoWork;
        /// <summary>
        /// Background 작업 내부 처리 마지막 단계(다중 Thread 내부, 외부 컨트롤 참조 안됨)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">ExecuteDataEventArgs</param>
        public delegate void ExecuteDataDoWorkEventHandler(object sender, ExecuteDataEventArgs e);

        public event ExecuteDataModifyEventHandler ExecuteDataModify;
        /// <summary>
        /// Biz 에서 가져온 데이터를 중간 수정 후 단계(다중 Thread 외부(종료 후), 외부 컨트롤 사용 가능)
        /// e.DataSource 데이터를 변경 처리 하면 이후 데이터그리드에 반영됨.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">ExecuteDataEventArgs</param>
        public delegate void ExecuteDataModifyEventHandler(object sender, ExecuteDataEventArgs e);

        public event ExecuteCustomBindingEventHandler ExecuteCustomBinding;
        /// <summary>
        /// 데이터그리드에 사용자가 직접 데이터 바인딩 할 경우 이벤트 사용
        /// 이 이벤트가 있으면 자동 바인딩 하지 않음
        /// </summary>
        /// <param name="sender">send</param>
        /// <param name="e">ExecuteDataEventArgs</param>
        public delegate void ExecuteCustomBindingEventHandler(object sender, ExecuteDataEventArgs e);

        public event ExecuteDataCompletedEventHandler ExecuteDataCompleted;
        /// <summary>
        /// 데이터그리드 바인딩 후 후처리 단계
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">ExecuteDataEventArgs</param>
        public delegate void ExecuteDataCompletedEventHandler(object sender, ExecuteDataEventArgs e);

        public event ClipboardPastedEventHandler ClipboardPasted;
        public delegate void ClipboardPastedEventHandler(object sender, DataObjectPastingEventArgs e);

        public event ExcelExportModifyEventHandler ExcelExportModify;
        public delegate void ExcelExportModifyEventHandler(object sender, C1XLBook book);

        public event CheckAllChangingEventHandler CheckAllChanging;
        public delegate void CheckAllChangingEventHandler(object sender, int row, bool isCheck, RoutedEventArgs e);

        public event CheckAllChangeEventHandler CheckAllChanged;
        public delegate void CheckAllChangeEventHandler(object sender, bool isCheck, RoutedEventArgs e);

        public event ColumnHeaderResetEventHandler ColumnHeaderReset;
        public delegate void ColumnHeaderResetEventHandler(object sender);

        #endregion


        #region Property
        private bool isUserConfigUse = true;
        [Category("GMES"), DefaultValue(true), Description("Column 사용자 설정 사용유무")]
        public bool IsUserConfigUse
        {
            get { return isUserConfigUse; }
            set { isUserConfigUse = value; }
        }

        [Category("GMES"), DefaultValue(true), Description("Column 사용자 설정 현재 적용되었는지 유무")]
        public bool IsUserConfigUsing
        {
            get
            {
                if (userConfigInfoColumnSet != null && userConfigInfoColumnSet.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool isColumnHeaderWrap = false;
        [Category("GMES"), DefaultValue(false), Description("Column Header Text Warp 유무")]
        public bool IsColumnHeaderWrap
        {
            get { return isColumnHeaderWrap; }
            set { isColumnHeaderWrap = value; }
        }

        private bool isCheckAllColumnUse = false;
        [Category("GMES"), DefaultValue(false), Description("CHK 컬럼 전체선택 사용 유무(CHK 컬럼)")]
        public bool IsCheckAllColumnUse
        {
            get { return isCheckAllColumnUse; }
            set
            {
                try
                {
                    isCheckAllColumnUse = value;

                    if (DesignerProperties.GetIsInDesignMode(this)) return;

                    if (isCheckAllColumnUse)
                    {
                        if (chkAllPresenter == null)
                        {
                            chkAllPresenter = new DataGridRowHeaderPresenter();
                            chkAllPresenter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
                            chkAllPresenter.MouseOverBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
                        }

                        if (chkDataGridAll == null)
                        {
                            chkDataGridAll = new CheckBox();
                            chkDataGridAll.IsChecked = false;
                            chkDataGridAll.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
                            chkDataGridAll.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                            chkDataGridAll.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            chkDataGridAll.Margin = new System.Windows.Thickness(4, 0, 0, 0);
                        }

                        chkDataGridAll.IsChecked = false;

                        chkDataGridAll.Checked += ChkDataGridAll_Checked;
                        chkDataGridAll.Unchecked += ChkDataGridAll_Unchecked;

                        if (this.Columns.Contains("CHK")) this.Columns["CHK"].Header = "";
                    }
                    else
                    {
                        if (this.Columns.Contains("CHK")) this.Columns["CHK"].Header = ObjectDic.Instance.GetObjectName("선택");
                    }
                    this.Refresh();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private bool isSumCellsUse = true;
        [Category("GMES"), DefaultValue(true), Description("다중선택시 합계 보임 유무")]
        public bool IsSumCellsUse
        {
            get { return isSumCellsUse; }
            set { isSumCellsUse = value; }
        }

        private bool isCellContextUse = false;
        [Category("GMES"), DefaultValue(false), Description("셀 팝업 메뉴 사용 유무")]
        public bool IsCellContextUse
        {
            get { return isCellContextUse; }
            set { isCellContextUse = value; }
        }

        private bool isRowCountView = true;
        [Category("GMES"), DefaultValue(true), Description("행수 보임 유무")]
        public bool IsRowCountView
        {
            get { return isRowCountView; }
            set
            {
                isRowCountView = value;

                if (DesignerProperties.GetIsInDesignMode(this)) return;

                Grid gdFirst = this.FindChild<Grid>("");
                if (gdFirst == null) return;

                DependencyObject gdRowCount = System.Windows.LogicalTreeHelper.FindLogicalNode(gdFirst, "GridRowCount");
                if (gdRowCount != null && gdRowCount is Grid)
                {
                    Grid gd = gdRowCount as Grid;
                    if (gd != null)
                    {
                        if (isRowCountView)
                        {
                            gd.Visibility = Visibility.Visible;
                            SetRowCount();
                        }
                        else
                        {
                            gd.Visibility = Visibility.Collapsed;
                        }
                    }
                }

            }
        }

        [Category("GMES"), Description("사용자 설정 제외 컬럼 설정")]
        public List<string> UserConfigExceptColumns { get; set; }

        private bool isCommonFormat = false;
        [Category("GMES"), DefaultValue(false), Description("공통 Format 적용 유무")]
        public bool IsCommonFormat
        {
            get { return isCommonFormat; }
            set
            {
                isCommonFormat = value;
            }
        }

        private bool isSummaryRowApply = false;
        [Category("GMES"), DefaultValue(false), Description("합계 Row 사용 유무")]
        public bool IsSummaryRowApply
        {
            get { return isSummaryRowApply; }
            set
            {
                isSummaryRowApply = value;

                if (isSummaryRowApply) MakeSummaryRow();
            }
        }

        [Category("GMES"), Description("Group Summary 컬럼 지정")]
        public List<string> SummaryGroupColumns { get; set; }

        private bool isMouseWheelApply = true;
        [Category("GMES"), DefaultValue(true), Description("마우스 휠 사용 유무")]
        public bool IsMouseWheelApply
        {
            get { return isMouseWheelApply; }
            set
            {
                isMouseWheelApply = value;
            }
        }
        #endregion


        #region [Declaration & Constructor]
        public IFrameOperation FrameOperation { get; set; }
        public List<DataGridCellsRange> MergedRangeList = null;

        private System.IFormatProvider numberFormat = new System.Globalization.CultureInfo("en-US", true);

        private List<UserConfigInformation> originalConfigInfos = null;
        private Dictionary<string, List<UserConfigInformation>> userConfigInfoColumnSet = null;
        private Dictionary<string, bool> userConfigInfoRowCountSet = null;
        private MenuItem mnuUserConfigList = null;
        private string currentConfigSet = "";
        private bool isDefaultConfigSet = false;

        private Dictionary<System.Drawing.Point, CellBorderLineInfo> cellsBorderLineInfo = null;
        private List<DataGridCellsRange> rangesBorderInfo = null;
        private Dictionary<System.Drawing.Point, CellBorderLineInfo> rangesBorderLineInfo = null;

        private Dictionary<System.Drawing.Point, CellAlertInfo> cellsAlertInfo = null;

        private Dictionary<string, string> cellToolTips = null;
        private ToolTip selectionToolTip = null;

        private BackgroundWorker bgWorker = null;

        private DataView dvDefault = null;

        private ControlsLibrary.LoadingIndicator loadingIndicator = null;

        private System.Windows.Media.Animation.DoubleAnimation moveAni;

        private DataGridRowHeaderPresenter chkAllPresenter = null;
        private CheckBox chkDataGridAll = null;

        private Dictionary<string, string> columnFormat = new Dictionary<string, string>();

        private Dictionary<int, string> rowValidationList = null;
        private Dictionary<System.Drawing.Point, string> cellValidationList = null;
        private System.Windows.Style baseCellStyle = null;
        private System.Windows.Style normalStyle = null;
        private System.Windows.Style columnHeaderStyle = null;

        private ContextMenu columnHeaderMenu = null;
        private ContextMenu cellMenu = null;
        private ContextMenu originalMenu = null;

        private string lastArgumentDeclare = string.Empty;
        private string lastArgumentXaml = string.Empty;

        private string topParentName = string.Empty;

        private string loadingText = "Loading.....";
        private string applyingText = "Applying.....";

        private System.Windows.Media.SolidColorBrush summaryRowBackgroundColor = null;

        private bool isAutoWidth = true;
        private int currentRow = -1;


        public UcBaseDataGrid()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        private void InitializeControls()
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                SummaryGroupColumns = new List<string>();
                UserConfigExceptColumns = new List<string>();

                DataObject.AddPastingHandler(this, OnPaste);

                FrameOperation = FindWorkArea();

                Style = Application.Current.TryFindResource(typeof(C1DataGrid)) as Style;

                if (!Application.Current.Resources.Contains("UcBaseDataGridCellPresenterStyle"))
                {
                    ResourceDictionary resourceDic = new ResourceDictionary();
                    resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                    Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                }

                this.Loaded += UcBaseDataGrid_Loaded;

                this.LoadedColumnHeaderPresenter += UcBaseDataGrid_LoadedColumnHeaderPresenter;
                this.LoadedRowHeaderPresenter += UcBaseDataGrid_LoadedRowHeaderPresenter;
                //this.LoadedRowPresenter += UcBaseDataGrid_LoadedRowPresenter;
                this.LoadedCellPresenter += UcBaseDataGrid_LoadedCellPresenter;
                this.UnloadedCellPresenter += UcBaseDataGrid_UnloadedCellPresenter;
                this.MergingCells += UcBaseDataGrid_MergingCells;
                this.CurrentCellChanged += UcBaseDataGrid_CurrentCellChanged;
                this.SelectionChanged += UcBaseDataGrid_SelectionChanged;
                this.SelectionDragCompleted += UcBaseDataGrid_SelectionDragCompleted;
                this.FilterChanged += UcBaseDataGrid_FilterChanged;
                this.GroupChanging += UcBaseDataGrid_GroupChanging;
                this.GroupChanged += UcBaseDataGrid_GroupChanged;
                this.ColumnResized += UcBaseDataGrid_ColumnResized;

                var dpd = DependencyPropertyDescriptor.FromProperty(ItemsSourceProperty, typeof(UcBaseDataGrid));
                if (dpd != null)
                {
                    dpd.AddValueChanged(this, ItemsSourceTableChanged);
                }

                loadingText = ObjectDic.Instance.GetObjectName("LOADING");
                applyingText = ObjectDic.Instance.GetObjectName("APPLYING");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            topParentName = this.FindPageName();
            if (!string.IsNullOrEmpty(topParentName))
            {
                if (isUserConfigUse)
                {
                    SetOriginalColumns();

                    if (!string.IsNullOrEmpty(topParentName)) GetUserConfig();
                }
                this.Loaded -= UcBaseDataGrid_Loaded;
            }

            TopRowHeaderMerge();

            if (isSummaryRowApply) MakeSummaryRow();

            this.Refresh();
        }

        #endregion


        #region Override

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (!isMouseWheelApply)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseWheel(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (isSumCellsUse && selectionToolTip != null)
            {
                if (selectionToolTip.IsOpen) selectionToolTip.IsOpen = false;
                this.ToolTip = null;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.Cursor == Cursors.Wait || this.Cursor == Cursors.No || this.Cursor == Cursors.None)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);

            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ClipboardPasted?.Invoke(this, null);
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (this.Cursor == Cursors.Wait || this.Cursor == Cursors.No || this.Cursor == Cursors.None)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyUp(e);

            if (e.Key == Key.LeftCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                ShowSumToolTip();
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (this.Cursor == Cursors.Wait || this.Cursor == Cursors.No || this.Cursor == Cursors.None)
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            try
            {
                if (originalMenu == null) originalMenu = ContextMenu;

                if (!isCellContextUse && Keyboard.Modifiers == ModifierKeys.Control) isCellContextUse = true;

                if (string.IsNullOrEmpty(topParentName)) topParentName = this.FindPageName();

                if (!string.IsNullOrEmpty(topParentName))
                {
                    if (columnHeaderMenu == null && isUserConfigUse)
                    {
                        columnHeaderMenu = new ContextMenu();

                        MenuItem mnuUserConfig = new MenuItem();
                        mnuUserConfig.Name = "mnuUserConfig";
                        mnuUserConfig.Header = ObjectDic.Instance.GetObjectName("USER_CONFIG");
                        System.Windows.Controls.Image imgUserConfig = new System.Windows.Controls.Image();
                        imgUserConfig.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/icon_login_bizconfig.png", UriKind.Relative));
                        imgUserConfig.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfig.Icon = imgUserConfig;
                        mnuUserConfig.Click += MnuUserConfig_Click;
                        columnHeaderMenu.Items.Add(mnuUserConfig);

                        Separator separator = new Separator();
                        separator.Name = "separator";
                        columnHeaderMenu.Items.Add(separator);

                        mnuUserConfigList = new MenuItem();
                        mnuUserConfigList.Name = "mnuUserConfigList";
                        mnuUserConfigList.Header = ObjectDic.Instance.GetObjectName("USER_CONFIG_LIST");
                        System.Windows.Controls.Image imgUserConfigList = new System.Windows.Controls.Image();
                        imgUserConfigList.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                        imgUserConfigList.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfigList.Icon = imgUserConfigList;
                        columnHeaderMenu.Items.Add(mnuUserConfigList);

                        Separator separator2 = new Separator();
                        separator2.Name = "separator2";
                        columnHeaderMenu.Items.Add(separator2);

                        MenuItem mnuUserConfigSave = new MenuItem();
                        mnuUserConfigSave.Name = "mnuUserConfigSave";
                        mnuUserConfigSave.Header = ObjectDic.Instance.GetObjectName("USER_CONFIG_SAVE");
                        System.Windows.Controls.Image imgUserConfigSave = new System.Windows.Controls.Image();
                        imgUserConfigSave.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/i_savebtn.png", UriKind.Relative));
                        imgUserConfigSave.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfigSave.Icon = imgUserConfigSave;
                        mnuUserConfigSave.Click += MnuUserConfigSave_Click;
                        columnHeaderMenu.Items.Add(mnuUserConfigSave);

                        MenuItem mnuUserConfigDelete = new MenuItem();
                        mnuUserConfigDelete.Name = "mnuUserConfigDelete";
                        mnuUserConfigDelete.Header = ObjectDic.Instance.GetObjectName("USER_CONFIG_DELETE");
                        System.Windows.Controls.Image imgUserConfigDelete = new System.Windows.Controls.Image();
                        imgUserConfigDelete.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/ico-delete.png", UriKind.Relative));
                        imgUserConfigDelete.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfigDelete.Icon = imgUserConfigDelete;
                        mnuUserConfigDelete.Click += MnuUserConfigDelete_Click;
                        columnHeaderMenu.Items.Add(mnuUserConfigDelete);

                        MenuItem mnuUserConfigReset = new MenuItem();
                        mnuUserConfigReset.Name = "mnuUserConfigReset";
                        mnuUserConfigReset.Header = ObjectDic.Instance.GetObjectName("USER_CONFIG_RESET");
                        System.Windows.Controls.Image imgUserConfigReset = new System.Windows.Controls.Image();
                        imgUserConfigReset.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_table_refresh.png", UriKind.Relative));
                        imgUserConfigReset.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfigReset.Icon = imgUserConfigReset;
                        mnuUserConfigReset.Click += MnuUserConfigReset_Click;
                        columnHeaderMenu.Items.Add(mnuUserConfigReset);
                    }

                    if (cellMenu == null)
                    {
                        cellMenu = new ContextMenu();

                        MenuItem mnuCopyAllWithHeader = new MenuItem();
                        mnuCopyAllWithHeader.Name = "mnuCopyAllWithHeader";
                        mnuCopyAllWithHeader.Header = ObjectDic.Instance.GetObjectName("CopyAllWithHeader");
                        System.Windows.Controls.Image imgCopyAllWithHeader = new System.Windows.Controls.Image();
                        imgCopyAllWithHeader.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                        imgCopyAllWithHeader.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuCopyAllWithHeader.Icon = imgCopyAllWithHeader;
                        mnuCopyAllWithHeader.Click += MnuCopyAllWithHeader_Click;
                        cellMenu.Items.Add(mnuCopyAllWithHeader);

                        MenuItem mnuCopyWithHeader = new MenuItem();
                        mnuCopyWithHeader.Name = "mnuCopyWithHeader";
                        mnuCopyWithHeader.Header = ObjectDic.Instance.GetObjectName("COPYWITHHEADER");
                        System.Windows.Controls.Image imgCopyWithHeader = new System.Windows.Controls.Image();
                        imgCopyWithHeader.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                        imgCopyWithHeader.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuCopyWithHeader.Icon = imgCopyWithHeader;
                        mnuCopyWithHeader.Click += MnuCopyWithHeader_Click;
                        cellMenu.Items.Add(mnuCopyWithHeader);

                        Separator separator = new Separator();
                        separator.Name = "separator";
                        cellMenu.Items.Add(separator);

                        MenuItem mnuExcelSave = new MenuItem();
                        mnuExcelSave.Name = "mnuExcelSave";
                        mnuExcelSave.Header = ObjectDic.Instance.GetObjectName("EXCELSAVE");
                        System.Windows.Controls.Image imgExcelSave = new System.Windows.Controls.Image();
                        imgExcelSave.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_table_excel.png", UriKind.Relative));
                        imgExcelSave.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuExcelSave.Icon = imgExcelSave;
                        mnuExcelSave.Click += MnuExcelSave_Click;
                        cellMenu.Items.Add(mnuExcelSave);

                        MenuItem mnuExcelSaveAndOpen = new MenuItem();
                        mnuExcelSaveAndOpen.Name = "mnuExcelSaveAndOpen";
                        mnuExcelSaveAndOpen.Header = ObjectDic.Instance.GetObjectName("EXCELSAVEANDOPEN");
                        System.Windows.Controls.Image imgExcelSaveAndOpen = new System.Windows.Controls.Image();
                        imgExcelSaveAndOpen.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_table_excel.png", UriKind.Relative));
                        imgExcelSaveAndOpen.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuExcelSaveAndOpen.Icon = imgExcelSaveAndOpen;
                        mnuExcelSaveAndOpen.Click += MnuExcelSaveAndOpen_Click;
                        cellMenu.Items.Add(mnuExcelSaveAndOpen);

                        if (CheckAuth("MESDEV"))
                        {
                            Separator separator2 = new Separator();
                            separator2.Name = "separator2";
                            cellMenu.Items.Add(separator2);

                            MenuItem mnuCopyDeclare = new MenuItem();
                            mnuCopyDeclare.Name = "mnuDeclare";
                            mnuCopyDeclare.Header = "Copy Declaration";
                            System.Windows.Controls.Image imgCopyDeclare = new System.Windows.Controls.Image();
                            imgCopyDeclare.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                            imgCopyDeclare.Stretch = System.Windows.Media.Stretch.Fill;
                            mnuCopyDeclare.Icon = imgCopyDeclare;
                            mnuCopyDeclare.Click += MnuDeclare_Click;
                            cellMenu.Items.Add(mnuCopyDeclare);

                            MenuItem mnuCopyXaml = new MenuItem();
                            mnuCopyXaml.Name = "mnuCopyXaml";
                            mnuCopyXaml.Header = "Copy Xaml";
                            System.Windows.Controls.Image imgCopyXaml = new System.Windows.Controls.Image();
                            imgCopyXaml.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                            imgCopyXaml.Stretch = System.Windows.Media.Stretch.Fill;
                            mnuCopyXaml.Icon = imgCopyXaml;
                            mnuCopyXaml.Click += MnuCopyXaml_Click;
                            cellMenu.Items.Add(mnuCopyXaml);
                        }
                    }

                    if (!string.IsNullOrEmpty(lastArgumentDeclare) && !string.IsNullOrEmpty(lastArgumentXaml))
                    {
                        foreach (FrameworkElement item in cellMenu.Items)
                        {
                            if (item.Name == "separator2" || item.Name == "mnuDeclare" || item.Name == "mnuCopyXaml")
                            {
                                item.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    else
                    {
                        foreach (FrameworkElement item in cellMenu.Items)
                        {
                            if (item.Name == "separator2" || item.Name == "mnuDeclare" || item.Name == "mnuCopyXaml")
                            {
                                item.Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    if (IsUserConfigUse)
                    {
                        mnuUserConfigList.Items.Clear();

                        if (userConfigInfoColumnSet == null)
                        {
                            userConfigInfoColumnSet = new Dictionary<string, List<UserConfigInformation>>();
                            userConfigInfoRowCountSet = new Dictionary<string, bool>();
                        }

                        foreach (KeyValuePair<string, List<UserConfigInformation>> userConfig in userConfigInfoColumnSet)
                        {
                            MenuItem subUserItem = new MenuItem();
                            subUserItem.Name = this.Name + "_" + userConfig.Key.Replace(" ", "_");
                            subUserItem.Tag = userConfig.Key;
                            subUserItem.Header = userConfig.Key;
                            if (mnuUserConfigList.Items.Count == 0) subUserItem.FontWeight = FontWeights.Bold;

                            System.Windows.Controls.Image imgUser = new System.Windows.Controls.Image();
                            imgUser.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                            imgUser.Stretch = System.Windows.Media.Stretch.Fill;
                            subUserItem.Icon = imgUser;

                            if (userConfig.Key.Equals(currentConfigSet)) subUserItem.IsChecked = true;

                            subUserItem.Click += SubUserConfig_Click;

                            mnuUserConfigList.Items.Add(subUserItem);
                        }

                        Separator itemSep = columnHeaderMenu.FindItem<Separator>("separator2");
                        MenuItem itemDel = columnHeaderMenu.FindItem<MenuItem>("mnuUserConfigDelete");
                        if (mnuUserConfigList == null || mnuUserConfigList.Items.Count == 0)
                        {
                            mnuUserConfigList.Visibility = itemSep.Visibility = itemDel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            mnuUserConfigList.Visibility = itemSep.Visibility = itemDel.Visibility = System.Windows.Visibility.Visible;
                            if (string.IsNullOrEmpty(currentConfigSet)) itemDel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }

                    C1.WPF.DataGrid.DataGridCell cell = GetCellFromPoint(e.GetPosition(null));
                    object clickObject = e.Device.Target;

                    if (clickObject != null && (cell == null || cell.Row.DataItem == null))
                    {
                        switch (clickObject.GetType().Name)
                        {
                            case "Border":
                                Border border = clickObject as Border;
                                if (border.TemplatedParent is C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRowPresenter ||
                                    border.TemplatedParent is C1.WPF.DataGrid.DataGridSelectableRowPresenter)
                                {
                                    if (!ContextMenu.Equals(originalMenu)) ContextMenu = cellMenu;
                                }
                                else
                                {
                                    if (columnHeaderMenu != null && !ContextMenu.Equals(columnHeaderMenu)) ContextMenu = columnHeaderMenu;
                                }
                                break;
                            case "TextBlock":
                            case "Rectangle":
                                if (columnHeaderMenu != null && !ContextMenu.Equals(columnHeaderMenu)) ContextMenu = columnHeaderMenu;
                                break;
                            default:
                                if (isCellContextUse)
                                {
                                    if (!ContextMenu.Equals(cellMenu)) ContextMenu = cellMenu;
                                }
                                else
                                {
                                    if (!ContextMenu.Equals(originalMenu)) ContextMenu = originalMenu;
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (isCellContextUse)
                        {
                            if (!ContextMenu.Equals(cellMenu)) ContextMenu = cellMenu;
                        }
                        else
                        {
                            if (!ContextMenu.Equals(originalMenu)) ContextMenu = originalMenu;
                        }
                    }
                }
                else
                {
                    if (ContextMenu != null && !ContextMenu.Equals(originalMenu)) ContextMenu = originalMenu;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            base.OnToolTipOpening(e);

            e.Handled = true;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            ClipboardPasted?.Invoke(this, e);
        }

        private void UcBaseDataGrid_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell == null || e.Cell.Row == null) return;

                if (!e.Cell.Row.Index.Equals(currentRow))
                {
                    RowIndexChanged?.Invoke(this, currentRow, e.Cell.Row.Index);
                    currentRow = e.Cell.Row.Index;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (isSumCellsUse && selectionToolTip != null)
            {
                if (selectionToolTip.IsOpen) selectionToolTip.IsOpen = false;
                this.ToolTip = null;
            }
        }

        private void UcBaseDataGrid_SelectionDragCompleted(object sender, DataGridSelectionDragEventArgs e)
        {
            ShowSumToolTip();
        }

        private void UcBaseDataGrid_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                if (isCheckAllColumnUse &&
                    chkAllPresenter != null && chkDataGridAll != null &&
                    !string.IsNullOrEmpty(e.Column.Name) && e.Column.Name.Equals("CHK"))
                {
                    chkAllPresenter.Content = chkDataGridAll;
                    e.Column.HeaderPresenter.Content = chkAllPresenter;
                    e.Column.HeaderPresenter.Margin = new Thickness(6, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                if (e.Row.HeaderPresenter == null) return;

                if (e.Row.Type != DataGridRowType.Item)
                {
                    if (e.Row.HeaderPresenter != null) e.Row.HeaderPresenter.Content = null;
                    return;
                }

                e.Row.HeaderPresenter.Content = null;

                if (rowValidationList != null && rowValidationList.ContainsKey(e.Row.Index))
                {
                    System.Windows.Controls.Image validImage = new System.Windows.Controls.Image();
                    validImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/icon_message.png", UriKind.Relative));
                    validImage.Stretch = System.Windows.Media.Stretch.Fill;
                    validImage.Width = 15;
                    validImage.Height = 14;
                    validImage.ToolTip = rowValidationList[e.Row.Index].IsNvc() ? null : rowValidationList[e.Row.Index].Nvc();
                    e.Row.HeaderPresenter.Content = validImage;
                }
                else
                {
                    if (e.Row.Type == DataGridRowType.Item)
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = (e.Row.Index + 1 - this.TopRows.Count).ToString();
                        tb.VerticalAlignment = VerticalAlignment.Center;
                        tb.HorizontalAlignment = HorizontalAlignment.Center;
                        e.Row.HeaderPresenter.Content = tb;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            if (e.Row.Presenter == null) return;

            if (e.Row.Type == DataGridRowType.Group)
            {
            }
        }

        private void UcBaseDataGrid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                if (e.Cell.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Top)
                {
                    #region Column Header Wrapping
                    if (isColumnHeaderWrap)
                    {
                        if (this.TopRows.Count > 0 && e.Cell.Row.Index.Equals(this.TopRows.Count - 1))
                        {
                            if (columnHeaderStyle == null)
                            {
                                columnHeaderStyle = Application.Current.Resources["UcBaseDataGridColumnHeaderPresenterStyle"] as Style;
                            }
                            DataGridColumnHeaderPresenter colPre = e.Cell.Presenter.Content as DataGridColumnHeaderPresenter;
                            colPre.Style = columnHeaderStyle;
                            TextBlock tb = colPre.Content as TextBlock;
                            if (tb != null)
                            {
                                tb.TextTrimming = TextTrimming.None;
                                tb.TextWrapping = TextWrapping.Wrap;
                                tb.TextAlignment = TextAlignment.Center;
                                tb.Margin = new Thickness(2, 6, 2, 6);

                                e.Cell.Row.Height = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                            }
                        }
                    }
                    #endregion
                }
                else if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (normalStyle == null || baseCellStyle == null)
                    {
                        normalStyle = e.Cell.Presenter.Style as Style;
                        baseCellStyle = Application.Current.Resources["UcBaseDataGridCellPresenterStyle"] as Style;
                    }

                    if (cellValidationList != null && cellValidationList.Count > 0)
                    {
                        System.Drawing.Point cellPoint = new System.Drawing.Point(e.Cell.Row.Index, e.Cell.Column.Index);
                        if (cellValidationList.ContainsKey(cellPoint))
                        {
                            e.Cell.Presenter.Style = baseCellStyle;
                            if (string.IsNullOrEmpty(cellValidationList[cellPoint]))
                            {
                                e.Cell.Presenter.ToolTip = null;
                            }
                            else
                            {
                                e.Cell.Presenter.ToolTip = cellValidationList[cellPoint].IsNvc() ? null : cellValidationList[cellPoint];
                            }

                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                object validGrid = e.Cell.Presenter.Template.FindName("ValidationGrid", e.Cell.Presenter);
                                if (validGrid != null)
                                {
                                    Grid grid = validGrid as Grid;
                                    if (grid != null && grid.Children.Count > 0)
                                    {
                                        if (e.Cell.Row.Height.Value < 27)
                                        {
                                            double cellHeight = GetCellHeight(e.Cell);
                                            grid.Height = cellHeight;
                                            grid.VerticalAlignment = VerticalAlignment.Top;

                                            System.Windows.Controls.Image validImage = grid.Children[0] as System.Windows.Controls.Image;
                                            validImage.Width = cellHeight / 1.7;
                                            validImage.Height = cellHeight / 1.8;
                                            validImage.VerticalAlignment = VerticalAlignment.Center;
                                        }
                                        grid.Visibility = Visibility.Visible;
                                    }
                                }
                            }));
                        }
                        else
                        {
                            e.Cell.Presenter.Style = normalStyle;
                            e.Cell.Presenter.ToolTip = null;
                        }
                    }
                    else if (cellToolTips != null && cellToolTips.Count > 0)
                    {
                        string cellKey = e.Cell.Row.Index.ToString() + "," + e.Cell.Column.Index.ToString();
                        if (cellToolTips.ContainsKey(cellKey))
                        {
                            e.Cell.Presenter.ToolTip = cellToolTips[cellKey].IsNvc() ? null : cellToolTips[cellKey];
                        }
                        else
                        {
                            e.Cell.Presenter.ToolTip = null;
                        }
                    }

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (e.Cell.Presenter != null)
                            {
                                double cellHeight = GetCellHeight(e.Cell);
                                if (cellHeight == 0) cellHeight = this.RowHeight.Value;

                                if (cellHeight < e.Cell.Presenter.MinHeight)
                                {
                                    Font cellFont = new Font(e.Cell.Presenter.FontFamily.Source, (float)e.Cell.Presenter.FontSize);
                                    double fontHeight = cellFont.Height - e.Cell.Presenter.FontFamily.LineSpacing;
                                    double heightDelta = (e.Cell.Presenter.MinHeight - cellHeight) / 2;

                                    if (cellHeight > fontHeight)
                                    {
                                        if (e.Cell.Presenter.Style.Equals(baseCellStyle))
                                        {
                                            heightDelta += 6 - ((cellHeight - fontHeight) * 0.6);
                                        }
                                        else
                                        {
                                            heightDelta -= (cellHeight - fontHeight) / 2;
                                        }
                                    }
                                    else
                                    {
                                        if (e.Cell.Presenter.Style.Equals(baseCellStyle)) heightDelta += 8;
                                    }

                                    e.Cell.Presenter.Padding = new Thickness(e.Cell.Presenter.Padding.Left, e.Cell.Presenter.Padding.Top - heightDelta,
                                                                             e.Cell.Presenter.Padding.Right, e.Cell.Presenter.Padding.Bottom);
                                }

                                if (GMES.Version.Equals("1.0") && DataGridExtension.GetIsAlternatingRow(this) == true)
                                {
                                    if (e.Cell.Row.Index >= e.Cell.DataGrid.TopRows.Count)
                                    {
                                        e.Cell.Presenter.Background = (e.Cell.Row.Index % 2) == (e.Cell.DataGrid.TopRows.Count % 2) ? e.Cell.DataGrid.RowBackground : e.Cell.DataGrid.AlternatingRowBackground;
                                    }
                                }
                            }
                        }
                        catch { }
                    }));
                }
                else if (e.Cell.Row.Type == DataGridRowType.Group)
                {
                    if (IsSummaryRowApply)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;

                        C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRow dggwsr = e.Cell.Row as C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRow;
                        C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRowPresenter dggwsrp = dggwsr.Presenter as C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRowPresenter;

                        int colorIndex = SummaryGroupColumns.IndexOf(dggwsr.Column.Name);

                        double leftMargin = (SummaryGroupColumns.Count + 1) * 20 + 10;
                        Border borderUb = dggwsrp.FindChild<Border>("UserBackground");
                        if (borderUb != null)
                        {
                            borderUb.Background = GetSummaryLevelColor(colorIndex);
                            borderUb.Margin = new Thickness(leftMargin, 0, 0, 0);

                            Border borderBg = dggwsrp.FindChild<Border>("BackgroundElement");
                            if (borderBg != null && borderBg.Margin.Left == 0)
                            {
                                borderBg.Margin = new Thickness(leftMargin, 0, 0, 0);
                            }
                        }

                        foreach (DataGridCellPresenter groupCell in dggwsrp.CellsPanel.Children)
                        {
                            if (groupCell.Column == null) continue;

                            groupCell.FontWeight = FontWeights.Bold;

                            if (SummaryGroupColumns.Count == 0)
                            {
                                TextBlock tbSum = groupCell.FindChild<TextBlock>("");
                                if (tbSum != null)
                                {
                                    if (dggwsr.Column.Name.Equals("AUTO_SUMMARY_GROUP_COLUMN"))
                                    {
                                        if (string.IsNullOrEmpty(tbSum.Text.Trim()))
                                        {
                                            tbSum.Text = ObjectDic.Instance.GetObjectName("합계");
                                            groupCell.FontWeight = FontWeights.Bold;
                                            groupCell.HorizontalContentAlignment = HorizontalAlignment.Center;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (SummaryGroupColumns.Contains(groupCell.Column.Name))
                                {
                                    TextBlock tbSum = groupCell.FindChild<TextBlock>("");
                                    if (tbSum != null)
                                    {
                                        if (dggwsr.Column.Name.Equals("AUTO_SUMMARY_GROUP_COLUMN"))
                                        {
                                            tbSum.Text = ObjectDic.Instance.GetObjectName("총계");

                                            groupCell.FontWeight = FontWeights.Bold;

                                            MergeCells(e.Cell.Row.Index, this.Columns[SummaryGroupColumns[0]].Index,
                                                       e.Cell.Row.Index, this.Columns[SummaryGroupColumns[SummaryGroupColumns.Count - 1]].Index);
                                        }
                                        else
                                        {
                                            if (dggwsr.Column.Name.Equals(groupCell.Column.Name))
                                            {
                                                string previewText = GetPreviewRowValue(e.Cell.Row.Index, groupCell.Column.Name);
                                                //tbSum.Text = previewText + " - " + ObjectDic.Instance.GetObjectName("합계");
                                                tbSum.Text = ObjectDic.Instance.GetObjectName("합계");

                                                if (SummaryGroupColumns.Last().Equals(groupCell.Column.Name))
                                                {
                                                    groupCell.FontWeight = FontWeights.Bold;
                                                    groupCell.HorizontalAlignment = HorizontalAlignment.Right;
                                                }

                                                if (SummaryGroupColumns.IndexOf(groupCell.Column.Name) < SummaryGroupColumns.Count - 1)
                                                {
                                                    groupCell.FontWeight = FontWeights.Bold;
                                                    MergeCells(e.Cell.Row.Index, this.Columns[groupCell.Column.Name].Index,
                                                           e.Cell.Row.Index, this.Columns[SummaryGroupColumns[SummaryGroupColumns.Count - 1]].Index);
                                                }
                                            }
                                            else
                                            {
                                                // 빈공간 배경 흰색
                                                int groupIndex = SummaryGroupColumns.IndexOf(groupCell.Column.Name);
                                                int currentIndex = SummaryGroupColumns.IndexOf(dggwsr.Column.Name);
                                                if (currentIndex > groupIndex)
                                                {
                                                    if (SummaryGroupColumns.Contains(e.Cell.Column.Name))
                                                    {
                                                        groupCell.Background = System.Windows.Media.Brushes.White;
                                                    }
                                                }

                                                // 빈공간 이름 표시
                                                int beforeRowIndex = e.Cell.Row.Index - 1;
                                                while (beforeRowIndex > 0)
                                                {
                                                    string beforeValue = Util.NVC(this.GetValue(beforeRowIndex, groupCell.Column.Name));
                                                    if (!string.IsNullOrEmpty(beforeValue))
                                                    {
                                                        tbSum.Text = beforeValue;
                                                        groupCell.FontWeight = FontWeights.Normal;
                                                        break;
                                                    }
                                                    beforeRowIndex--;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Border Ling Setting
                SettingBorderLine(e.Cell.Presenter);

                // Cell Alert Setting
                SettingAlert(e.Cell.Presenter);

                #region 공통 Format 적용
                if (isCommonFormat)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item || e.Cell.Row.Type == DataGridRowType.Bottom)
                    {
                        if (!string.IsNullOrEmpty(e.Cell.Text))
                        {
                            if (e.Cell.Column.Name.EndsWith("DESC") ||
                                e.Cell.Column.Name.EndsWith("NOTE") ||
                                e.Cell.Column.Name.EndsWith("REMARK") ||
                                e.Cell.Column.Name.EndsWith("NAME") ||
                                e.Cell.Column.Name.EndsWith("UPDUSER") ||
                                e.Cell.Column.Name.EndsWith("INSUSER"))
                            {
                                e.Cell.Column.HorizontalAlignment = HorizontalAlignment.Left;

                                if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                {
                                    columnFormat.Add(e.Cell.Column.Name, e.Cell.Column.HorizontalAlignment.ToString());
                                }
                                return;
                            }

                            if (e.Cell.Column.Name.EndsWith("_YN") ||
                                e.Cell.Column.Name.EndsWith("_CODE") ||
                                e.Cell.Column.Name.EndsWith("_FLAG") ||
                                e.Cell.Column.Name.EndsWith("_ID"))
                            {
                                e.Cell.Column.HorizontalAlignment = HorizontalAlignment.Center;

                                if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                {
                                    columnFormat.Add(e.Cell.Column.Name, e.Cell.Column.HorizontalAlignment.ToString());
                                }
                                return;
                            }

                            double resultNumber;
                            DateTime resultDateTime;
                            bool isPercent = false;
                            string valueText = Util.NVC(e.Cell.Text);
                            if (Util.NVC(e.Cell.Text).Contains("%"))
                            {
                                isPercent = true;
                                valueText = valueText.Replace("%", "");
                            }


                            if (double.TryParse(valueText, NumberStyles.Any, numberFormat, out resultNumber))
                            {
                                switch (e.Cell.Column.DependencyObjectType.Name)
                                {
                                    case "DataGridTextColumn":
                                        {
                                            C1.WPF.DataGrid.DataGridTextColumn nuColumn = e.Cell.Column as C1.WPF.DataGrid.DataGridTextColumn;
                                            if ((nuColumn.Format != null && nuColumn.Format.Equals(string.Empty)) || columnFormat.ContainsKey(e.Cell.Column.Name))
                                            {
                                                if (isPercent)
                                                {

                                                }

                                                string formatString = "#,##0";
                                                int pointLen = 0;
                                                if (valueText.Contains("."))
                                                {
                                                    pointLen = valueText.Length - valueText.IndexOf(".") - 1;
                                                    formatString += "." + new string('0', pointLen);
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                            }
                                            nuColumn.HorizontalAlignment = HorizontalAlignment.Right;
                                        }
                                        break;
                                    case "DataGridComboBoxColumn":
                                        {
                                            C1.WPF.DataGrid.DataGridComboBoxColumn nuColumn = e.Cell.Column as C1.WPF.DataGrid.DataGridComboBoxColumn;
                                            if ((nuColumn.Format != null && nuColumn.Format.Equals(string.Empty)) || columnFormat.ContainsKey(e.Cell.Column.Name))
                                            {
                                                if (isPercent)
                                                {

                                                }

                                                string formatString = "#,##0";
                                                int pointLen = 0;
                                                if (valueText.Contains("."))
                                                {
                                                    pointLen = valueText.Length - valueText.IndexOf(".") - 1;
                                                    formatString += "." + new string('0', pointLen);
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                            }
                                            nuColumn.HorizontalAlignment = HorizontalAlignment.Right;
                                        }
                                        break;
                                    case "DataGridNumericColumn":
                                        {
                                            C1.WPF.DataGrid.DataGridNumericColumn nuColumn = e.Cell.Column as C1.WPF.DataGrid.DataGridNumericColumn;
                                            if ((nuColumn.Format != null && nuColumn.Format.Equals(string.Empty)) || columnFormat.ContainsKey(e.Cell.Column.Name))
                                            {
                                                string value = Util.NVC(e.Cell.Value);
                                                string formatString = "#,##0";
                                                int pointLen = 0;
                                                if (value.Contains("."))
                                                {
                                                    pointLen = value.Length - value.IndexOf(".") - 1;
                                                    formatString += "." + new string('0', pointLen);
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                                    {
                                                        columnFormat.Add(e.Cell.Column.Name, formatString);
                                                        nuColumn.Format = formatString;
                                                    }
                                                    else
                                                    {
                                                        int saveLen = columnFormat[e.Cell.Column.Name].Length - columnFormat[e.Cell.Column.Name].IndexOf(".") - 1;
                                                        if (pointLen > saveLen)
                                                        {
                                                            columnFormat[e.Cell.Column.Name] = formatString;
                                                            nuColumn.Format = formatString;
                                                        }
                                                    }
                                                }
                                            }
                                            if (e.Cell != null && e.Cell.Presenter != null)
                                            {
                                                e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
                                                e.Cell.Presenter.Padding = new Thickness(6, 0, 6, 0);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (DateTime.TryParse(Util.NVC(e.Cell.Value), out resultDateTime))
                            {
                                switch (e.Cell.Column.DependencyObjectType.Name)
                                {
                                    case "DataGridTextColumn":
                                        System.Windows.Controls.TextBlock tb = e.Cell.Presenter.Content as System.Windows.Controls.TextBlock;
                                        if (tb.Text.Length == 10 && tb.Text.Replace("-", "").Length == 8)
                                        {
                                            tb.Text = resultDateTime.ToString("yyyy-MM-dd");
                                        }
                                        else
                                        {
                                            tb.Text = resultDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        }

                                        tb.HorizontalAlignment = HorizontalAlignment.Center;
                                        if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                        {
                                            columnFormat.Add(e.Cell.Column.Name, "yyyy-MM-dd HH:mm:ss");
                                        }
                                        break;
                                    case "DataGridDateTimeColumn":
                                        C1.WPF.DataGrid.DataGridDateTimeColumn dtColumn = e.Cell.Column as C1.WPF.DataGrid.DataGridDateTimeColumn;
                                        if (dtColumn.CustomDateFormat != null && !dtColumn.CustomDateFormat.Equals("yyyy-MM-dd HH:mm:ss"))
                                        {
                                            dtColumn.CustomDateFormat = "yyyy-MM-dd HH:mm:ss";
                                            if (!columnFormat.ContainsKey(e.Cell.Column.Name))
                                            {
                                                columnFormat.Add(e.Cell.Column.Name, "yyyy-MM-dd HH:mm:ss");
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region 특수 처리
                try
                {
                    // -99999 일때 값 보이지 않기
                    double specialNumber;
                    string specialText = Util.NVC(e.Cell.Text).Replace(".", "");
                    if (double.TryParse(specialText, NumberStyles.Any, numberFormat, out specialNumber))
                    {
                        if (specialNumber == -99999)
                        {
                            this.SetValue(e.Cell.Row.Index, e.Cell.Column.Name, null);
                        }
                    }
                }
                catch
                {
                    // 오류 시 스킵
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                if (e.Cell.Presenter == null) return;
                if (e.Cell.Row.Type == DataGridRowType.Top)
                {
                }
                else if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                }
                else if (e.Cell.Row.Type == DataGridRowType.Group)
                {
                    if (IsSummaryRowApply)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (MergedRangeList == null) return;

                foreach (DataGridCellsRange range in MergedRangeList)
                {
                    if (range.TopLeftCell.Row.Index < 0 || range.TopLeftCell.Column.Index < 0 ||
                        range.BottomRightCell.Row.Index < 0 || range.BottomRightCell.Column.Index < 0) continue;

                    e.Merge(range);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseDataGrid_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            if (isRowCountView) SetRowCount();
        }

        private void UcBaseDataGrid_GroupChanging(object sender, DataGridGroupChangingEventArgs e)
        {
            if (!IsGroupingRowsAllowed || !IsSummaryRowApply) return;

            if (Application.Current.Resources.Contains("UcBaseDataGridGroupRowPresenterStyle"))
            {
                Style groupStyle = Application.Current.Resources["UcBaseDataGridGroupRowPresenterStyle"] as Style;
                if (!this.GroupRowStyle.Equals(groupStyle))
                {
                    this.GroupRowStyle = groupStyle;
                }

                DataGridRowsPanel dgrp = this.FindChild<DataGridRowsPanel>("Body");
                if (dgrp != null)
                {
                    double leftMargin = ((SummaryGroupColumns.Count + 1) * 20 + 10) * -1;
                    dgrp.Margin = new Thickness(leftMargin, 0, 0, 0);
                }

            }
        }

        private void UcBaseDataGrid_GroupChanged(object sender, DataGridGroupChangedEventArgs e)
        {
            if (!IsGroupingRowsAllowed || !IsSummaryRowApply) return;

            if (this.ItemsSource == null || this.GetRowCount() == 0) return;

            summaryRowBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);

            this.GroupRowPosition = DataGridGroupRowPosition.BelowData;
        }

        private void UcBaseDataGrid_ColumnResized(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if ((cellsBorderLineInfo != null && cellsBorderLineInfo.Count > 0) || (rangesBorderInfo != null && rangesBorderInfo.Count > 0)) this.Refresh();
        }
        #endregion


        #region [Public Method]

        #region LoadingIndicator
        public void LoadingIndicatorStart(string message)
        {
            if (loadingIndicator == null)
            {
                loadingIndicator = this.FindChild<LoadingIndicator>("LoadingBackground");
            }

            if (loadingIndicator != null)
            {
                TextBlock tb = loadingIndicator.FindChild<TextBlock>("tb");
                if (tb != null)
                {
                    tb.Text = message;
                    tb.InvalidateVisual();
                }

                if (loadingIndicator.Visibility != Visibility.Visible)
                {
                    loadingIndicator.SetCurrentValue(OpacityProperty, 1.0d);
                    loadingIndicator.Visibility = Visibility.Visible;
                }
            }
        }

        public void LoadingIndicatorStop()
        {
            if (loadingIndicator == null) return;

            if (loadingIndicator.Visibility == Visibility.Visible) loadingIndicator.Visibility = Visibility.Collapsed;

            TextBlock tb = loadingIndicator.FindChild<TextBlock>("tb");
            if (tb != null) tb.Text = loadingText;
        }
        #endregion

        #region Edit Rows
        public void AddRowData(int rowCount)
        {
            try
            {
                DataTable dt = null;

                if (this.ItemsSource != null)
                {
                    for (int i = 0; i < rowCount; i++)
                    {
                        dt = this.GetDataTable(false);
                        DataRow newRow = dt.NewRow();
                        dt.Rows.Add(newRow);
                    }
                }
                else
                {
                    dt = new DataTable();

                    foreach (C1.WPF.DataGrid.DataGridColumn col in this.Columns)
                    {
                        if (!dt.Columns.Contains(col.Name)) dt.Columns.Add(col.Name);
                    }

                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow newRow = dt.NewRow();
                        dt.Rows.Add(newRow);
                    }
                    this.ItemsSource = DataTableConverter.Convert(dt);
                }

                if (dt != null) dt.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void DeleteRowData(int rowIndex)
        {
            try
            {
                if (this.ItemsSource == null) return;

                DataTable dtDelete = this.GetDataTable(false);
                if (rowIndex >= dtDelete.Rows.Count) return;

                dtDelete.Rows.RemoveAt(rowIndex);
                dtDelete.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Data Execute
        /// <summary>
        /// 백그라운드 처리로 데이터 바인딩까지 처리하는 메소드. 
        /// (데이터 바인딩, Exception, 인디케이터 포함). 
        /// 데이터 확인이 필요할 경우 ExecuteDataCompleted 이벤트 반환 인수에서 확인 가능
        /// </summary>
        /// <param name="bizRuleID"></param>
        /// <param name="inData"></param>
        /// <param name="outData"></param>
        /// <param name="inDataTable"></param>
        /// <param name="autoWidth"></param>
        /// <param name="indicatorView"></param>
        /// <param name="argument"></param>
        public void ExecuteService(string bizRuleID, string inData, string outData, DataTable inDataTable, bool autoWidth = true, bool indicatorView = true, object argument = null, string bindTableName = null)
        {
            ExecuteServiceRun(bizRuleID, inData, outData, inDataTable, autoWidth, indicatorView, argument, bindTableName);
        }

        /// <summary>
        /// 백그라운드 처리로 데이터 바인딩까지 처리하는 메소드. 
        /// (데이터 바인딩, Exception, 인디케이터 포함). 
        /// 데이터 확인이 필요할 경우 ExecuteDataCompleted 이벤트 반환 인수에서 확인 가능
        /// </summary>
        /// <param name="bizRuleID"></param>
        /// <param name="inData"></param>
        /// <param name="outData"></param>
        /// <param name="inDataSet"></param>
        /// <param name="autoWidth"></param>
        /// <param name="indicatorView"></param>
        /// <param name="argument"></param>
        public void ExecuteService(string bizRuleID, string inData, string outData, System.Data.DataSet inDataSet, bool autoWidth = true, bool indicatorView = true, object argument = null, string bindTableName = null)
        {
            ExecuteServiceRun(bizRuleID, inData, outData, inDataSet, autoWidth, indicatorView, argument, bindTableName);
        }

        private void ExecuteServiceRun(string bizRuleID, string inData, string outData, object inDataSource, bool autoWidth, bool indicatorView, object argument, string bindTableName)
        {
            try
            {
                FrameOperation = FindWorkArea();

                isAutoWidth = autoWidth;

                if (string.IsNullOrEmpty(bizRuleID) || string.IsNullOrEmpty(inData) || string.IsNullOrEmpty(outData) || inDataSource == null) return;

                if (inDataSource is DataTable)
                {
                    if ((inDataSource as DataTable).Rows.Count == 0) return;
                }
                else if (inDataSource is DataSet)
                {
                    if ((inDataSource as DataSet).Tables.Count == 0) return;
                }
                else
                {
                    return;
                }

                if (indicatorView && loadingIndicator == null)
                {
                    loadingIndicator = this.FindChild<LoadingIndicator>("LoadingBackground");
                }

                if (moveAni == null)
                {
                    moveAni = new System.Windows.Media.Animation.DoubleAnimation();
                    moveAni.Completed += MoveAni_Completed;
                    moveAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                    moveAni.AutoReverse = false;
                    moveAni.From = 1d;
                    moveAni.To = 0d;
                }

                if (bgWorker == null)
                {
                    bgWorker = new BackgroundWorker();
                    bgWorker.DoWork += BgWorker_DoWork;
                    bgWorker.ProgressChanged += BgWorker_ProgressChanged;
                    bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
                    bgWorker.WorkerReportsProgress = true;
                }
                else
                {
                    if (bgWorker.IsBusy) return;
                }

                object[] arguments = new object[6];
                arguments[0] = bizRuleID;
                arguments[1] = inData;
                arguments[2] = outData;
                arguments[3] = inDataSource;
                arguments[4] = argument;
                arguments[5] = bindTableName;

                if (loadingIndicator != null && indicatorView)
                {
                    loadingIndicator.SetCurrentValue(OpacityProperty, 1.0d);
                    loadingIndicator.Visibility = Visibility.Visible;
                }

                currentRow = -1;

                bgWorker.RunWorkerAsync(arguments);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetDataFiler(string filter)
        {
            if (this.ItemsSource == null) return;

            if (this.ItemsSource is DataView)
            {
                DataView dv = this.ItemsSource as DataView;
                dv.RowFilter = filter;
            }
        }

        public void SetDataFilerClear()
        {
            if (this.ItemsSource == null) return;

            if (this.ItemsSource is DataView)
            {
                DataView dv = this.ItemsSource as DataView;
                dv.RowFilter = "";
            }
        }
        #endregion

        #region DataGridComboBox 설정
        /// <summary>
        /// Column Combo 데이터 설정
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="dtCombo">DataTable</param>
        /// <param name="isSelectFirst">첫번째 선택 여부</param>
        public void SetDataGridComboBoxColumn(string comboColumnName, DataTable dtCombo, string filter = "", bool isInBlank = true, bool isInCode = true)
        {
            try
            {
                if (dtCombo == null) return;

                DataTable dtComboTable = dtCombo.Copy();

                C1.WPF.DataGrid.DataGridColumn col = this.Columns[comboColumnName];
                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dtComboTable != null && dtComboTable.Columns.Count >= 2)
                {
                    if (cboColumn.SelectedValuePath == null)
                    {
                        cboColumn.SelectedValuePath = dtComboTable.Columns[0].ColumnName;
                    }

                    if (cboColumn.DisplayMemberPath == null)
                    {
                        cboColumn.DisplayMemberPath = dtComboTable.Columns[1].ColumnName;
                    }
                }

                if (isInCode && dtComboTable != null)
                {
                    dtComboTable.AsEnumerable().ToList<DataRow>()
                        .ForEach(x => x[cboColumn.DisplayMemberPath] = "[" + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.SelectedValuePath]) + "] " + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.DisplayMemberPath]));
                }

                if (isInBlank && dtComboTable != null)
                {
                    DataRow newRow = dtComboTable.NewRow();
                    newRow[cboColumn.SelectedValuePath] = null;
                    newRow[cboColumn.DisplayMemberPath] = string.Empty;
                    dtComboTable.Rows.InsertAt(newRow, 0);
                    dtComboTable.AcceptChanges();
                }

                cboColumn.ItemsSource = dtComboTable.AsDataView();

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = cboColumn.ItemsSource as DataView;
                    dvResult.RowFilter = filter;
                }

                dtComboTable.Dispose();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetDataGridComboBoxColumn(string comboColumnName, List<string> lstCombo, bool isInBlank = false)
        {
            try
            {
                if (lstCombo == null || lstCombo.Count == 0) return;

                C1.WPF.DataGrid.DataGridColumn col = this.Columns[comboColumnName];

                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (isInBlank)
                {
                    lstCombo.Insert(0, string.Empty);
                }

                cboColumn.ItemsSource = lstCombo;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetDataGridComboBoxFilter(string comboColumnName, string filter)
        {
            try
            {
                C1.WPF.DataGrid.DataGridColumn col = this.Columns[comboColumnName];
                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (cboColumn.ItemsSource == null) return;

                DataView dvResult = cboColumn.ItemsSource as DataView;
                dvResult.RowFilter = filter;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Column Visible for Common Code
        /// <summary>
        /// 사용자 설정이 적용된 컬럼 Visible
        /// 기준정보에 등록된 내역을 비교하여 Visible 된다.
        /// </summary>
        /// <param name="columnName">대상 컬럼</param>
        /// <param name="commonGroupCode">그룹코드</param>
        /// <param name="commonCode">코드</param>        
        /// <param name="commonColumn">비교대상 컬럼</param>
        /// <param name="CommonValue">비교 값</param>
        /// <param name="visible">보임 여부</param>
        public void SetColumnVisibleForCommonCode(string columnName, string commonGroupCode, string commonCode, CommonCodeColumn commonColumn = CommonCodeColumn.CMCODE, string CommonValue = "[Default]", bool visible = true)
        {
            try
            {
                if (this.Columns.Contains(columnName))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("CMCODE", typeof(string));
                    RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                    DataRow drNew = RQSTDT.NewRow();
                    drNew["LANGID"] = LoginInfo.LANGID;
                    drNew["CMCDTYPE"] = commonGroupCode;
                    drNew["CMCODE"] = commonCode;
                    drNew["CMCDIUSE"] = "Y";
                    RQSTDT.Rows.Add(drNew);

                    string bizRuleName = "DA_BAS_SEL_COMMONCODE_USE";

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (CommonValue.Equals("[Default]"))
                        {
                            if (visible)
                            {
                                SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                            }
                            else
                            {
                                SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                            }
                        }
                        else
                        {
                            if (dtResult.Rows[0][commonColumn.ToString()].Equals(CommonValue))
                            {
                                if (visible)
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                                }
                                else
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                                }
                            }
                            else
                            {
                                if (!visible)
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                                }
                                else
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!visible)
                        {
                            SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                        }
                        else
                        {
                            SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용자 설정이 적용된 컬럼 Visible
        /// 기준정보에 등록된 내역을 비교하여 Visible 된다.
        /// </summary>
        /// <param name="columnName">대상 컬럼</param>
        /// <param name="commonGroupCode">그룹코드</param>
        /// <param name="commonCode">코드</param>        
        /// <param name="commonColumn">비교대상 컬럼</param>
        /// <param name="CommonValue">비교 값</param>
        /// <param name="visible">보임 여부</param>
        public void SetColumnVisibleForAreaCommonCode(string columnName, string commonGroupCode, string commonCode, CommonCodeColumn commonColumn = CommonCodeColumn.CMCODE, string CommonValue = "[Default]", bool visible = true)
        {
            try
            {
                if (this.Columns.Contains(columnName))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                    RQSTDT.Columns.Add("COM_CODE", typeof(string));
                    RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                    DataRow drNew = RQSTDT.NewRow();
                    drNew["LANGID"] = LoginInfo.LANGID;
                    drNew["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drNew["COM_TYPE_CODE"] = commonGroupCode;
                    drNew["COM_CODE"] = commonCode;
                    drNew["USE_FLAG"] = "Y";
                    RQSTDT.Rows.Add(drNew);

                    string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (CommonValue.Equals("[Default]"))
                        {
                            if (visible)
                            {
                                SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                            }
                            else
                            {
                                SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                            }
                        }
                        else
                        {
                            if (dtResult.Rows[0][commonColumn.ToString().Replace("IBUTE", "")].Equals(CommonValue))
                            {
                                if (visible)
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                                }
                                else
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                                }
                            }
                            else
                            {
                                if (!visible)
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                                }
                                else
                                {
                                    SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!visible)
                        {
                            SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Visible);
                        }
                        else
                        {
                            SetColumnVisible(this.Columns[columnName].Index, System.Windows.Visibility.Collapsed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용자 설정이 적용된 컬럼 Visible
        /// 사용자 설정에서 숨긴 처리된 내역은 Visible 되지 않는다.
        /// </summary>
        /// <param name="columnName">컬럼명</param>
        /// <param name="visibility">Visibility</param>
        public void SetColumnVisible(string columnName, System.Windows.Visibility visibility)
        {
            try
            {
                if (this.Columns.Contains(columnName))
                {
                    SetColumnVisible(this.Columns[columnName].Index, visibility);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 사용자 설정이 적용된 컬럼 Visible
        /// 사용자 설정에서 숨긴 처리된 내역은 Visible 되지 않는다.
        /// </summary>
        /// <param name="columnIndex">컬럼 Index</param>
        /// <param name="visibility">Visibility</param>
        public void SetColumnVisible(int columnIndex, System.Windows.Visibility visibility)
        {
            try
            {
                if (visibility == Visibility.Visible && userConfigInfoColumnSet != null && !string.IsNullOrEmpty(currentConfigSet) &&
                    userConfigInfoColumnSet[currentConfigSet] != null && userConfigInfoColumnSet[currentConfigSet].Count > 0)
                {
                    UserConfigInformation ucf = userConfigInfoColumnSet[currentConfigSet].Find(f => f.ColumnName.Equals(this.Columns[columnIndex].Name));

                    if (ucf != null && ucf.Visibility != Visibility.Visible) return;
                }

                this.Columns[columnIndex].Visibility = visibility;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region MergeCell
        /// <summary>
        /// Celll 을 Merge 한다.
        /// </summary>
        /// <param name="row1">좌측상단 Row</param>
        /// <param name="column1">좌측상단 Column</param>
        /// <param name="row2">우측하단 Row</param>
        /// <param name="column2">우측하단 Column</param>
        public void MergeCells(int row1, int column1, int row2, int column2, CellBorderLineInfo cellBorder = null)
        {
            try
            {
                C1.WPF.DataGrid.DataGridCell leftTop = this.GetCell(row1, column1);
                C1.WPF.DataGrid.DataGridCell rightBottom = this.GetCell(row2, column2);

                MergeCells(leftTop, rightBottom, cellBorder);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Celll 을 Merge 한다.
        /// </summary>
        /// <param name="leftTop">좌측상단</param>
        /// <param name="rightBottom">우측하단</param>
        public void MergeCells(C1.WPF.DataGrid.DataGridCell leftTop, C1.WPF.DataGrid.DataGridCell rightBottom, CellBorderLineInfo cellBorder = null)
        {
            try
            {
                if (leftTop == null || rightBottom == null) return;

                if (MergedRangeList == null)
                {
                    MergedRangeList = new List<DataGridCellsRange>();
                }

                if (leftTop.Row.Index > rightBottom.Row.Index || leftTop.Column.Index > rightBottom.Column.Index) return;

                DataGridCellsRange range = new DataGridCellsRange(leftTop, rightBottom);
                if (range.IsSingleCell()) return;

                MergeCells(range, cellBorder);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Celll 을 Merge 한다.
        /// </summary>
        /// <param name="range">range</param>
        public void MergeCells(DataGridCellsRange range, CellBorderLineInfo cellBorder = null)
        {
            try
            {
                if (MergedRangeList == null)
                {
                    MergedRangeList = new List<DataGridCellsRange>();
                }

                if (range.IsSingleCell()) return;
                if (range.TopLeftCell.Row.Index < 0 || range.TopLeftCell.Column.Index < 0 ||
                    range.BottomRightCell.Row.Index < 0 || range.BottomRightCell.Column.Index < 0)
                {
                    return;
                }

                bool isDuplicate = false;
                foreach (DataGridCellsRange mergedRange in MergedRangeList)
                {
                    if (mergedRange.Intersects(range))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (range.TopLeftCell.Row.Index < 0 || range.TopLeftCell.Column.Index < 0 ||
                    range.BottomRightCell.Row.Index < 0 || range.BottomRightCell.Column.Index < 0) return;

                if (!isDuplicate)
                {
                    MergedRangeList.Add(range);
                    if (cellBorder != null) SetBorderLine(range.TopLeftCell.Row.Index, range.TopLeftCell.Column.Index, cellBorder);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearMergeCell()
        {
            try
            {
                if (MergedRangeList == null) return;

                MergedRangeList.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void MergeCellsRemove(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                DataGridCellsRange range = new DataGridCellsRange(cell, cell);
                foreach (DataGridCellsRange mergedRange in MergedRangeList)
                {
                    if (mergedRange.Intersects(range))
                    {
                        MergedRangeList.Remove(mergedRange);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool IsMergedCell(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                if (MergedRangeList == null) return false;

                DataGridCellsRange range = new DataGridCellsRange(cell, cell);
                foreach (DataGridCellsRange mergedRange in MergedRangeList)
                {
                    if (mergedRange.Intersects(range)) return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        /// <summary>
        /// TopRow 로 설정된 컬럼 헤더를 자동 머지 한다.
        /// </summary>
        public void TopRowHeaderMerge()
        {
            try
            {
                if (this.TopRows.Count == 0) return;

                ClearMergeCell();

                // Horizontal
                for (int row = 0; row < this.TopRows.Count; row++)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in this.Columns.OrderBy(o => o.DisplayIndex))
                    {
                        DataGridCellsRange dgcr = FindCellRectangleHorizontal(row, dgc.DisplayIndex);
                        if (dgcr != null && !dgcr.IsSingleCell())
                        {
                            this.MergeCells(dgcr.TopLeftCell, dgcr.BottomRightCell);
                        }
                    }
                }

                //Vertical
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in this.Columns.OrderBy(o => o.DisplayIndex))
                {
                    for (int row = 0; row < this.TopRows.Count; row++)
                    {
                        DataGridCellsRange dgcr = FindCellRectangleVertical(row, dgc.DisplayIndex);
                        if (dgcr != null && !dgcr.IsSingleCell())
                        {
                            this.MergeCells(dgcr.TopLeftCell, dgcr.BottomRightCell);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Excel Export

        public void ExcelExport(bool isOpen)
        {
            try
            {
                string excelFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                SaveFileDialog saveDialog = new SaveFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    saveDialog.InitialDirectory = @"\\Client\C$\Users";
                }
                saveDialog.Filter = "Excel Files (.xlsx)|*.xlsx";
                saveDialog.FileName = excelFileName;

                if (saveDialog.ShowDialog() == true)
                {
                    ExcelExport(saveDialog.FileName, isOpen);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ExcelExport(string excelFileName, bool isOpen = false)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                this.Save(ms, new ExcelSaveOptions
                {
                    FileFormat = ExcelFileFormat.Xlsx,
                    KeepColumnWidths = true,
                    KeepRowHeights = true
                });
                ms.Seek(0L, SeekOrigin.Begin);

                C1XLBook c1XLBook = new C1XLBook();
                c1XLBook.Load(ms);

                c1XLBook.Sheets[0].Name = "Export";

                XLSheet sheet = c1XLBook.Sheets["Export"];

                XLStyle headerStyle = new XLStyle(c1XLBook);
                headerStyle.AlignHorz = XLAlignHorzEnum.Center;
                headerStyle.AlignVert = XLAlignVertEnum.Center;
                headerStyle.BackColor = System.Windows.Media.Colors.Gainsboro;
                headerStyle.Font = new XLFont("Arial", 10f, true, false);
                headerStyle.SetBorderStyle(XLLineStyleEnum.Thin);

                XLCellRangeCollection rangeMerge = sheet.MergedCells;

                List<int> list = new List<int>();
                foreach (C1.WPF.DataGrid.DataGridRow row in this.Rows)
                {
                    if (row.Visibility == Visibility.Collapsed)
                    {
                        list.Add(row.Index + ((this.TopRows.Count == 0) ? 1 : 0));
                    }
                }

                foreach (int item in list.OrderByDescending((int i) => i))
                {
                    if (item < c1XLBook.Sheets[0].Rows.Count)
                    {
                        c1XLBook.Sheets[0].Rows.RemoveAt(item);
                    }
                }

                for (int j = 0; j < c1XLBook.Sheets[0].Rows.Count; j++)
                {
                    for (int k = 0; k < c1XLBook.Sheets[0].Columns.Count; k++)
                    {
                        if (j == 0)
                        {
                            c1XLBook.Sheets[0].GetCell(j, k).SetValue(c1XLBook.Sheets[0].GetCell(j, k).Text, c1XLBook.Sheets[0].GetCell(j, k).Style);
                        }

                        if (c1XLBook.Sheets[0].GetCell(j, k).Style != null)
                        {
                            c1XLBook.Sheets[0].GetCell(j, k).Style.Font = new XLFont("Arial", 10f);
                        }
                    }
                }

                #region Header 설정

                int rowIndex = 0, colIndex = 0, headerRow = 0;

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in this.Columns)
                {
                    if (dgc.Name == null) continue;
                    if (dgc.Visibility != Visibility.Visible) continue;
                    if (originalConfigInfos != null && originalConfigInfos.Find(f => f.ColumnName.Equals(dgc.Name)) == null) continue;

                    if (dgc.Header is List<string>)
                    {
                        int mergeStart = 0, mergeEnd = 0;
                        List<string> headers = dgc.Header as List<string>;
                        for (int inx = 0; inx < headers.Count; inx++)
                        {
                            sheet[rowIndex + inx, colIndex].Value = headers[inx];
                            sheet[rowIndex + inx, colIndex].Style = headerStyle;

                            if (inx > 0 && !headers[inx - 1].Equals(headers[inx]))
                            {
                                if (mergeStart < mergeEnd)
                                {
                                    XLCellRange range = new XLCellRange(mergeStart, mergeEnd, colIndex, colIndex);
                                    rangeMerge.Add(range);
                                }
                                mergeStart = mergeEnd = inx;
                            }
                            if (inx > 0 && headers[inx - 1].Equals(headers[inx])) mergeEnd = inx;
                            if (inx > headerRow) headerRow = inx;
                        }

                        if (mergeStart < mergeEnd)
                        {
                            XLCellRange range = new XLCellRange(mergeStart, mergeEnd, colIndex, colIndex);
                            rangeMerge.Add(range);
                        }
                    }
                    else
                    {
                        sheet[rowIndex, colIndex].Value = dgc.Header.ToString();
                        sheet[rowIndex, colIndex].Style = headerStyle;
                    }
                    colIndex++;
                }

                for (int row = 0; row < headerRow; row++)
                {
                    int mergeStart = 0, mergeEnd = 0;
                    for (int col = 0; col < colIndex; col++)
                    {
                        if (col > 0 && !sheet[row, col - 1].Value.Equals(sheet[row, col].Value))
                        {
                            if (mergeStart < mergeEnd)
                            {
                                XLCellRange range = new XLCellRange(row, row, mergeStart, mergeEnd);
                                rangeMerge.Add(range);
                            }
                            mergeStart = mergeEnd = col;
                        }
                        if (col > 0 && sheet[row, col - 1].Value.Equals(sheet[row, col].Value)) mergeEnd = col;
                    }

                    if (mergeStart < mergeEnd)
                    {
                        XLCellRange range = new XLCellRange(row, row, mergeStart, mergeEnd);
                        rangeMerge.Add(range);
                    }
                }
                #endregion

                MemoryStream editedms = new MemoryStream();
                if (this.Resources.Contains("ExportRemove"))
                {
                    List<int> list2 = this.Resources["ExportRemove"] as List<int>;
                    for (int num = list2.Count; num > 0; num--)
                    {
                        c1XLBook.Sheets[0].Columns.RemoveAt(list2[num - 1]);
                    }
                }

                AutoSizeColumns(c1XLBook.Sheets[0]);

                c1XLBook.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
                editedms.Seek(0L, SeekOrigin.Begin);

                uploadTempStream(excelFileName, editedms, delegate (object sender, UploadCompletedEventArgs arg)
                {
                    ms.Close();
                    editedms.Close();
                    if (!arg.Success)
                    {
                    }
                }, isOpen);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void uploadTempStream(string filekey, Stream stream, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler, bool bOpen = false)
        {
            int blockSize = 1024 * 1024 * 100;
            int readedTotal = 0;
            int partNumber = 1;
            int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

            try
            {
                while (stream.Length > readedTotal)
                {
                    byte[] buffer = new byte[blockSize];
                    int readed = stream.Read(buffer, 0, blockSize);
                    readedTotal += readed;

                    FileInfo tempFile = new FileInfo(filekey);

                    using (FileStream fs = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        fs.Write(buffer, 0, readed);
                        fs.Flush();
                        fs.Close();
                    }
                    partNumber++;
                }

                if (bOpen) System.Diagnostics.Process.Start(filekey);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AutoSizeColumns(XLSheet sheet)
        {
            try
            {
                /*No Graphics instance available because there's no Paint event*/
                /*Create a Graphics object using a handle to current window instead*/
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    /*Traverse rows and columns*/
                    for (int c = 0; c < sheet.Columns.Count; c++)
                    {
                        int colWidth = -1;
                        int startRow = 0;
                        if (this.TopRows != null && this.TopRows.Count > 0) startRow = this.TopRows.Count - 1;

                        for (int r = startRow; r < (sheet.Rows.Count > 10 ? 10 : sheet.Rows.Count); r++)
                        {
                            /*Get cell value*/
                            object value = sheet[r, c].Value;
                            if (value != null)
                            {
                                string text = value.ToString();

                                /*Get Style for this cell*/
                                XLStyle s = sheet[r, c].Style;
                                if (s != null && s.Format.Length > 0 && value is IFormattable)
                                {
                                    string fmt = XLStyle.FormatXLToDotNet(s.Format);
                                    /*get formatted text*/
                                    text = ((IFormattable)value).ToString(fmt, CultureInfo.CurrentCulture);
                                }

                                XLFont dFont = sheet.Book.DefaultFont;

                                System.Drawing.FontFamily fFamily = new System.Drawing.FontFamily(dFont.FontName);
                                Font font = new Font(fFamily, dFont.FontSize);

                                /*Get size of drawn string according to its Font*/
                                System.Drawing.Size sz = System.Drawing.Size.Ceiling(g.MeasureString(text + "XX", font));

                                if (sz.Width > colWidth)
                                    colWidth = sz.Width;
                            }
                        }
                        /*Set columns width*/
                        if (colWidth > -1)
                            sheet.Columns[c].Width = C1XLBook.PixelsToTwips(colWidth);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Validation 설정

        public void ClearValidation()
        {
            try
            {
                if (rowValidationList == null)
                {
                    rowValidationList = new Dictionary<int, string>();
                }

                foreach (KeyValuePair<int, string> item in rowValidationList)
                {
                    if (item.Key < this.Rows.Count) this.Rows[item.Key].Refresh();
                }

                rowValidationList.Clear();

                if (cellValidationList == null)
                {
                    cellValidationList = new Dictionary<System.Drawing.Point, string>();
                }

                List<int> refreshRows = new List<int>();
                foreach (KeyValuePair<System.Drawing.Point, string> item in cellValidationList)
                {
                    C1.WPF.DataGrid.DataGridCell cell = this.GetCell(item.Key.X, item.Key.Y);
                    if (cell != null) refreshRows.Add(cell.Row.Index);
                }

                cellValidationList.Clear();

                foreach (int index in refreshRows)
                {
                    this.Rows[index].Refresh();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void RemoveCellValidation(int rowIndex, string colName)
        {
            if (!this.Columns.Contains(colName)) return;

            RemoveCellValidation(rowIndex, this.Columns[colName].Index);
        }

        public void RemoveCellValidation(int rowIndex, int colIndex)
        {
            try
            {
                if (cellValidationList == null)
                {
                    cellValidationList = new Dictionary<System.Drawing.Point, string>();
                    return;
                }

                if (cellValidationList.ContainsKey(new System.Drawing.Point(rowIndex, colIndex)))
                {
                    cellValidationList.Remove(new System.Drawing.Point(rowIndex, colIndex));
                    this.Rows[rowIndex].Refresh();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetCellValidation(int rowIndex, string colName, string messageID, params object[] parameters)
        {
            if (!this.Columns.Contains(colName)) return;

            SetCellValidation(rowIndex, this.Columns[colName].Index, messageID, parameters);
        }

        public void SetCellValidation(int rowIndex, int colIndex, string messageID, params object[] parameters)
        {
            SetCellValidation(rowIndex, colIndex, MessageDic.Instance.GetMessage(messageID, parameters));
        }

        public void SetCellValidation(int rowIndex, string colName, string message, bool showToolTip = false)
        {
            if (!this.Columns.Contains(colName)) return;

            SetCellValidation(rowIndex, this.Columns[colName].Index, message, showToolTip);
        }

        public void SetCellValidation(int rowIndex, int colIndex, string message, bool showToolTip = false)
        {
            try
            {
                if (this.ItemsSource == null) return;

                if (cellValidationList == null)
                {
                    cellValidationList = new Dictionary<System.Drawing.Point, string>();
                }

                if (!cellValidationList.ContainsKey(new System.Drawing.Point(rowIndex, colIndex)))
                {
                    string convertMessage = MessageDic.Instance.GetMessage(message).Replace("[#]", "").Trim();

                    cellValidationList.Add(new System.Drawing.Point(rowIndex, colIndex), convertMessage);
                    if (rowIndex < this.Rows.Count) this.Rows[rowIndex].Refresh();

                    this.ShowToolTip(convertMessage, PlacementMode.Mouse, showToolTip);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void RemoveRowValidation(int rowIndex)
        {
            try
            {
                if (this.ItemsSource == null) return;

                if (rowValidationList == null)
                {
                    rowValidationList = new Dictionary<int, string>();
                    return;
                }

                if (rowValidationList.Count == 0) return;

                Dictionary<int, string> newValidationList = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> item in rowValidationList)
                {
                    if (item.Key < rowIndex)
                    {
                        newValidationList.Add(item.Key, item.Value);
                    }
                    else if (item.Key > rowIndex)
                    {
                        newValidationList.Add(item.Key - 1, item.Value);
                    }
                }
                rowValidationList = newValidationList;

                this.Rows[rowIndex].Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetRowValidation(int rowIndex, string messageID, params object[] parameters)
        {
            SetRowValidation(rowIndex, MessageDic.Instance.GetMessage(messageID, parameters));
        }

        public void SetRowValidation(int rowIndex, string message)
        {
            try
            {
                if (this.ItemsSource == null) return;

                if (rowValidationList == null)
                {
                    rowValidationList = new Dictionary<int, string>();
                }

                if (!rowValidationList.ContainsKey(rowIndex) && this.Rows.Count > rowIndex)
                {
                    string convertMessage = MessageDic.Instance.GetMessage(message).Replace("[#]", "").Trim();

                    rowValidationList.Add(rowIndex, convertMessage);
                    this.Rows[rowIndex].Refresh();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool IsValidation()
        {
            try
            {
                if (rowValidationList != null && rowValidationList.Count > 0) return true;
                if (cellValidationList != null && cellValidationList.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        public bool IsValidation(int row)
        {
            try
            {
                if (rowValidationList != null && rowValidationList.Count > 0)
                {
                    if (rowValidationList.ContainsKey(row)) return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }

        public bool IsValidation(int row, int col)
        {
            try
            {
                if (cellValidationList != null && cellValidationList.Count > 0)
                {
                    if (cellValidationList.ContainsKey(new System.Drawing.Point(row, col))) return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }
        #endregion

        #region Check/UnCheck All
        public void CheckAll()
        {
            try
            {
                chkDataGridAll.IsChecked = true;
                ChkDataGridAll_Checked(chkDataGridAll, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void UnCheckAll()
        {
            try
            {
                chkDataGridAll.IsChecked = false;
                ChkDataGridAll_Unchecked(chkDataGridAll, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 사용자 설정
        /// <summary>
        /// 현재 컬럼 설정을 초기 설정으로 지정
        /// </summary>
        public void SetOriginalColumns()
        {
            if (isUserConfigUse)
            {
                if (originalConfigInfos == null || originalConfigInfos.Count == 0)
                {
                    originalConfigInfos = new List<UserConfigInformation>();
                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in this.Columns)
                    {
                        UserConfigInformation uci = new UserConfigInformation(dgc);
                        if (uci.ColumnName == null) continue;

                        originalConfigInfos.Add(new UserConfigInformation(dgc));
                    }
                }
            }
        }
        #endregion 사용자 설정

        #region SummaryRow Value Control
        /// <summary>
        /// Presenter 내부에서 호출  필요
        /// </summary>
        /// <param name="summaryRowIndex"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string GetSummaryRowValue(int summaryRowIndex, string columnName)
        {
            if (this.BottomRows.Count == 0) return null;

            int rowIndex = this.Rows.Count - this.BottomRows.Count + summaryRowIndex;
            if (rowIndex >= this.Rows.Count) return null;

            C1.WPF.DataGrid.DataGridCell cell = this.GetCell(rowIndex, this.Columns[columnName].Index);
            if (cell.Presenter == null) return null;

            StackPanel sp = cell.Presenter.Content as StackPanel;
            if (sp == null) return null;

            ContentPresenter cp = sp.FindChild<ContentPresenter>("");
            if (cp == null) return null;
            if (!string.IsNullOrEmpty(Util.NVC(cp.Content))) return Util.NVC(cp.Content);

            TextBlock tb = cp.FindChild<TextBlock>("");
            if (tb == null) return null;

            return tb.Text;
        }

        /// <summary>
        /// Presenter 내부에서 호출 필요
        /// </summary>
        /// <param name="summaryRowIndex"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public void SetSummaryRowValue(int summaryRowIndex, string columnName, string value)
        {
            if (this.BottomRows.Count == 0) return;

            int rowIndex = this.Rows.Count - this.BottomRows.Count + summaryRowIndex;
            if (rowIndex >= this.Rows.Count) return;

            C1.WPF.DataGrid.DataGridCell cell = this.GetCell(rowIndex, this.Columns[columnName].Index);
            if (cell.Presenter == null) return;

            StackPanel sp = cell.Presenter.Content as StackPanel;
            if (sp == null) return;

            ContentPresenter cp = sp.FindChild<ContentPresenter>("");
            if (cp == null) return;

            TextBlock tb = cp.FindChild<TextBlock>("");
            if (tb == null) return;

            tb.Text = value;
        }
        #endregion

        #region Cell Work
        public void ClearCellToolTip()
        {
            if (cellToolTips == null) cellToolTips = new Dictionary<string, string>();

            cellToolTips.Clear();
        }

        public void ClearCellToolTip(int rowIndex, int colIndex)
        {
            if (cellToolTips == null) cellToolTips = new Dictionary<string, string>();

            string cellKey = rowIndex.ToString() + "," + colIndex.ToString();
            if (cellToolTips.ContainsKey(cellKey)) cellToolTips.Remove(cellKey);
        }

        /// <summary>
        /// Cell ToolTip 설정
        /// (LoadedCellPresenter 내부 사용금지)
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="colName"></param>
        /// <param name="toolTipText"></param>
        public void SetCellToolTip(int rowIndex, string colName, string toolTipText)
        {
            if (!this.Columns.Contains(colName)) return;

            SetCellToolTip(rowIndex, this.Columns[colName].Index, toolTipText);
        }

        /// <summary>
        /// Cell ToolTip 설정
        /// (LoadedCellPresenter 내부 사용금지)
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        /// <param name="toolTipText"></param>
        public void SetCellToolTip(int rowIndex, int colIndex, string toolTipText)
        {
            if (cellToolTips == null) cellToolTips = new Dictionary<string, string>();

            string cellKey = rowIndex.ToString() + "," + colIndex.ToString();
            if (!cellToolTips.ContainsKey(cellKey)) cellToolTips.Add(cellKey, toolTipText);
        }

        public double GetCellHeight(C1.WPF.DataGrid.DataGridCell cell)
        {
            try
            {
                double returnHeight = 0;

                returnHeight = cell.Row.ActualHeight;

                if (MergedRangeList != null && MergedRangeList.Count > 0)
                {
                    DataGridCellsRange range = new DataGridCellsRange(cell, cell);
                    foreach (DataGridCellsRange mergedRange in MergedRangeList)
                    {
                        if (mergedRange.Intersects(range))
                        {
                            returnHeight = 0;
                            for (int row = mergedRange.TopLeftCell.Row.Index; row <= mergedRange.BottomRightCell.Row.Index; row++)
                            {
                                returnHeight += this.Rows[row].ActualHeight;
                            }
                            break;
                        }
                    }
                }
                return returnHeight;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return double.NaN;
        }

        #region Setting Alert
        public void ClearCellAlert()
        {
            try
            {
                if (cellsAlertInfo == null) cellsAlertInfo = new Dictionary<System.Drawing.Point, CellAlertInfo>();
                cellsBorderLineInfo.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearCellAlert(int row, int col)
        {
            try
            {
                System.Drawing.Point position = new System.Drawing.Point(row, col);
                if (cellsAlertInfo == null) cellsAlertInfo = new Dictionary<System.Drawing.Point, CellAlertInfo>();
                if (cellsAlertInfo.ContainsKey(position)) cellsAlertInfo.Remove(position);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 해당 Cell 의 배경색에 깜빡임 효과를 준다.
        /// </summary>
        /// <param name="row">행</param>
        /// <param name="col">열</param>
        /// <param name="background">배경색</param>
        /// <param name="speedMillisecond">한번 깜빡이 시간(밀리초) - Default : 1,000</param>
        /// <param name="repeatSecond">반복하는 시간(초) - Default 0 : 무한</param>
        public void SetCellAlert(int row, int col, System.Windows.Media.Brush background = null, int speedMillisecond = 1000, int repeatSecond = 0)
        {
            try
            {
                if (row < 0 || col < 0) return;

                if (cellsAlertInfo == null) cellsAlertInfo = new Dictionary<System.Drawing.Point, CellAlertInfo>();

                System.Drawing.Point position = new System.Drawing.Point(row, col);
                if (cellsAlertInfo.ContainsKey(position))
                {
                    cellsAlertInfo[position].Background = background;
                    cellsAlertInfo[position].Repeat = repeatSecond.Equals(0) ? RepeatBehavior.Forever : new RepeatBehavior(TimeSpan.FromSeconds(repeatSecond));
                    cellsAlertInfo[position].SpeedMillisecond = speedMillisecond;
                }
                else
                {
                    CellAlertInfo cellAlertInfo = new CellAlertInfo();
                    cellAlertInfo.Background = background;
                    cellAlertInfo.Repeat = repeatSecond.Equals(0) ? RepeatBehavior.Forever : new RepeatBehavior(TimeSpan.FromSeconds(repeatSecond));
                    cellAlertInfo.SpeedMillisecond = speedMillisecond;
                    cellsAlertInfo.Add(position, cellAlertInfo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Setting Alert

        #region Setting Border
        public void ClearBorderLine()
        {
            try
            {
                if (cellsBorderLineInfo == null) cellsBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();
                cellsBorderLineInfo.Clear();

                if (rangesBorderInfo == null) rangesBorderInfo = new List<DataGridCellsRange>();
                rangesBorderInfo.Clear();

                if (rangesBorderLineInfo == null) rangesBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();
                rangesBorderLineInfo.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearBorderLine(int row, int col)
        {
            try
            {
                System.Drawing.Point position = new System.Drawing.Point(row, col);
                if (cellsBorderLineInfo == null) cellsBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();
                if (cellsBorderLineInfo.ContainsKey(position)) cellsBorderLineInfo.Remove(position);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearBorderLine(DataGridCellsRange range)
        {
            try
            {
                if (rangesBorderInfo == null) rangesBorderInfo = new List<DataGridCellsRange>();
                if (rangesBorderInfo.Contains(range))
                {
                    for (int row = range.TopLeftCell.Row.Index; row <= range.BottomRightCell.Row.Index; row++)
                    {
                        for (int col = range.TopLeftCell.Column.Index; col <= range.BottomRightCell.Column.Index; col++)
                        {
                            if (row == range.TopLeftCell.Row.Index || col == range.TopLeftCell.Column.Index ||
                                row == range.BottomRightCell.Row.Index || col == range.BottomRightCell.Column.Index)
                            {
                                System.Drawing.Point position = new System.Drawing.Point(row, col);
                                if (rangesBorderLineInfo.ContainsKey(position)) rangesBorderLineInfo.Remove(position);
                            }
                        }
                    }
                    rangesBorderInfo.Remove(range);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell 의 테두리를 그린다. (Sortting, Filtering 지원 안함)
        /// </summary>
        /// <param name="row">Row Index</param>
        /// <param name="col">Column index</param>
        /// <param name="cellBorderLineInfo">테두리 설정 정보</param>
        /// <param name="isReplace">True 일때 전체 적용, False 일때 테두리 값이 있는 변만 적용(null 인 변은 제외)</param>
        public void SetBorderLine(int row, int col, CellBorderLineInfo cellBorderLineInfo, bool isReplace = true, bool isAlert = false)
        {
            try
            {
                if (row < 0 || col < 0 || cellBorderLineInfo == null) return;

                if (cellsBorderLineInfo == null) cellsBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();

                System.Drawing.Point position = new System.Drawing.Point(row, col);
                if (cellsBorderLineInfo.ContainsKey(position))
                {
                    if (isReplace)
                    {
                        cellsBorderLineInfo[position].LeftBorderLineInfo = cellBorderLineInfo.LeftBorderLineInfo;
                        cellsBorderLineInfo[position].TopBorderLineInfo = cellBorderLineInfo.TopBorderLineInfo;
                        cellsBorderLineInfo[position].RightBorderLineInfo = cellBorderLineInfo.RightBorderLineInfo;
                        cellsBorderLineInfo[position].BottomBorderLineInfo = cellBorderLineInfo.BottomBorderLineInfo;
                    }
                    else
                    {
                        if (cellBorderLineInfo.LeftBorderLineInfo != null) cellsBorderLineInfo[position].LeftBorderLineInfo = cellBorderLineInfo.LeftBorderLineInfo;
                        if (cellBorderLineInfo.TopBorderLineInfo != null) cellsBorderLineInfo[position].TopBorderLineInfo = cellBorderLineInfo.TopBorderLineInfo;
                        if (cellBorderLineInfo.RightBorderLineInfo != null) cellsBorderLineInfo[position].RightBorderLineInfo = cellBorderLineInfo.RightBorderLineInfo;
                        if (cellBorderLineInfo.BottomBorderLineInfo != null) cellsBorderLineInfo[position].BottomBorderLineInfo = cellBorderLineInfo.BottomBorderLineInfo;
                    }
                }
                else
                {
                    cellsBorderLineInfo.Add(position, cellBorderLineInfo);
                }

                if (isAlert)
                {
                    DoubleAnimation opacityAni = new DoubleAnimation
                    {
                        From = 1.5,
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(1),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    Random rand = new Random(DateTime.Now.Millisecond);
                    int stratTime = rand.Next(0, 1000);
                    opacityAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                    cellsBorderLineInfo[position].LeftBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                    cellsBorderLineInfo[position].TopBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                    cellsBorderLineInfo[position].RightBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                    cellsBorderLineInfo[position].BottomBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell Range 의 테두리를 그린다. (Sortting, Filtering 지원 안함)
        /// </summary>
        /// <param name="range">Cell 블럭 범위</param>
        /// <param name="cellBorderLineInfo">테두리 설정 정보</param>
        /// <param name="isReplace">True 일때 전체 적용, False 일때 테두리 값이 있는 변만 적용(null 인 변은 제외)</param>
        public void SetBorderLine(DataGridCellsRange range, CellBorderLineInfo cellBorderLineInfo, bool isReplace = true, bool isAlert = false)
        {
            try
            {
                if (range == null || cellBorderLineInfo == null) return;

                if (rangesBorderInfo == null) rangesBorderInfo = new List<DataGridCellsRange>();

                if (cellsBorderLineInfo == null) cellsBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();
                if (rangesBorderLineInfo == null) rangesBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();

                if (!rangesBorderInfo.Contains(range)) rangesBorderInfo.Add(range);

                for (int row = range.TopLeftCell.Row.Index; row <= range.BottomRightCell.Row.Index; row++)
                {
                    for (int col = range.TopLeftCell.Column.Index; col <= range.BottomRightCell.Column.Index; col++)
                    {
                        if (row == range.TopLeftCell.Row.Index || col == range.TopLeftCell.Column.Index ||
                            row == range.BottomRightCell.Row.Index || col == range.BottomRightCell.Column.Index)
                        {
                            CellBorderLineInfo rangeBorderLineInfo = new CellBorderLineInfo();
                            if (row == range.TopLeftCell.Row.Index && col == range.TopLeftCell.Column.Index)
                            {
                                rangeBorderLineInfo.LeftBorderLineInfo = cellBorderLineInfo.LeftBorderLineInfo;
                                rangeBorderLineInfo.TopBorderLineInfo = cellBorderLineInfo.TopBorderLineInfo;
                            }

                            if (row == range.TopLeftCell.Row.Index &&
                                col != range.TopLeftCell.Column.Index && col != range.BottomRightCell.Column.Index)
                            {
                                rangeBorderLineInfo.TopBorderLineInfo = cellBorderLineInfo.TopBorderLineInfo;
                            }

                            if (row == range.TopLeftCell.Row.Index && col == range.BottomRightCell.Column.Index)
                            {
                                rangeBorderLineInfo.TopBorderLineInfo = cellBorderLineInfo.TopBorderLineInfo;
                                rangeBorderLineInfo.RightBorderLineInfo = cellBorderLineInfo.RightBorderLineInfo;
                            }

                            if (col == range.BottomRightCell.Column.Index &&
                                row != range.TopLeftCell.Row.Index && row != range.BottomRightCell.Row.Index)
                            {
                                rangeBorderLineInfo.RightBorderLineInfo = cellBorderLineInfo.RightBorderLineInfo;
                            }

                            if (row == range.BottomRightCell.Row.Index && col == range.BottomRightCell.Column.Index)
                            {
                                rangeBorderLineInfo.RightBorderLineInfo = cellBorderLineInfo.RightBorderLineInfo;
                                rangeBorderLineInfo.BottomBorderLineInfo = cellBorderLineInfo.BottomBorderLineInfo;
                            }

                            if (row == range.BottomRightCell.Row.Index &&
                                col != range.TopLeftCell.Column.Index && col != range.BottomRightCell.Column.Index)
                            {
                                rangeBorderLineInfo.BottomBorderLineInfo = cellBorderLineInfo.BottomBorderLineInfo;
                            }

                            if (row == range.BottomRightCell.Row.Index && col == range.TopLeftCell.Column.Index)
                            {
                                rangeBorderLineInfo.LeftBorderLineInfo = cellBorderLineInfo.LeftBorderLineInfo;
                                rangeBorderLineInfo.BottomBorderLineInfo = cellBorderLineInfo.BottomBorderLineInfo;
                            }

                            if (col == range.TopLeftCell.Column.Index &&
                                row != range.TopLeftCell.Row.Index && row != range.BottomRightCell.Row.Index)
                            {
                                rangeBorderLineInfo.LeftBorderLineInfo = cellBorderLineInfo.LeftBorderLineInfo;
                            }


                            System.Drawing.Point position = new System.Drawing.Point(row, col);
                            if (rangesBorderLineInfo.ContainsKey(position))
                            {
                                if (isReplace)
                                {
                                    rangesBorderLineInfo[position].LeftBorderLineInfo = rangeBorderLineInfo.LeftBorderLineInfo;
                                    rangesBorderLineInfo[position].TopBorderLineInfo = rangeBorderLineInfo.TopBorderLineInfo;
                                    rangesBorderLineInfo[position].RightBorderLineInfo = rangeBorderLineInfo.RightBorderLineInfo;
                                    rangesBorderLineInfo[position].BottomBorderLineInfo = rangeBorderLineInfo.BottomBorderLineInfo;
                                }
                                else
                                {
                                    if (rangeBorderLineInfo.LeftBorderLineInfo != null) rangesBorderLineInfo[position].LeftBorderLineInfo = rangeBorderLineInfo.LeftBorderLineInfo;
                                    if (rangeBorderLineInfo.TopBorderLineInfo != null) rangesBorderLineInfo[position].TopBorderLineInfo = rangeBorderLineInfo.TopBorderLineInfo;
                                    if (rangeBorderLineInfo.RightBorderLineInfo != null) rangesBorderLineInfo[position].RightBorderLineInfo = rangeBorderLineInfo.RightBorderLineInfo;
                                    if (rangeBorderLineInfo.BottomBorderLineInfo != null) rangesBorderLineInfo[position].BottomBorderLineInfo = rangeBorderLineInfo.BottomBorderLineInfo;
                                }
                            }
                            else
                            {
                                rangesBorderLineInfo.Add(position, rangeBorderLineInfo);
                            }

                            if (isAlert)
                            {
                                DoubleAnimation opacityAni = new DoubleAnimation();
                                opacityAni.From = 1.5;
                                opacityAni.To = 0.3;
                                opacityAni.Duration = TimeSpan.FromSeconds(1);
                                opacityAni.AutoReverse = true;
                                opacityAni.RepeatBehavior = RepeatBehavior.Forever;

                                Random rand = new Random(DateTime.Now.Millisecond);
                                int stratTime = rand.Next(0, 1000);
                                opacityAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                                cellsBorderLineInfo[position].LeftBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                                cellsBorderLineInfo[position].TopBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                                cellsBorderLineInfo[position].RightBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                                cellsBorderLineInfo[position].BottomBorderLineInfo.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Setting Border

        #endregion Cell Work

        #endregion [Public Method]


        #region [Private Method]

        private IFrameOperation FindWorkArea()
        {
            DependencyObject dObj = this;
            while (true)
            {
                dObj = System.Windows.Media.VisualTreeHelper.GetParent(dObj);

                if (dObj == null) break;
                if (dObj is IWorkArea && (dObj as IWorkArea).FrameOperation != null) break;
            }

            if (dObj is IWorkArea)
            {
                return (dObj as IWorkArea).FrameOperation;
            }
            else
            {
                return null;
            }
        }

        private void ShowSumToolTip()
        {
            try
            {
                if (this.Selection.SelectedCells.Count < 2) return;

                if (isSumCellsUse && this.SelectionMode.Equals(C1.WPF.DataGrid.DataGridSelectionMode.MultiRange))
                {
                    List<C1.WPF.DataGrid.DataGridCell> sumCells = new List<C1.WPF.DataGrid.DataGridCell>();
                    foreach (C1.WPF.DataGrid.DataGridCell cell in this.Selection.SelectedCells)
                    {
                        if (cell.Row.Type == DataGridRowType.Group) continue;

                        if (!sumCells.Contains(cell)) sumCells.Add(cell);
                    }

                    foreach (C1.WPF.DataGrid.DataGridCellsRange range in this.Selection.SelectedRanges)
                    {
                        for (int row = range.TopLeftCell.Row.Index; row <= range.BottomRightCell.Row.Index; row++)
                        {
                            for (int col = range.TopLeftCell.Column.Index; col <= range.BottomRightCell.Column.Index; col++)
                            {
                                C1.WPF.DataGrid.DataGridCell cell = this.GetCell(row, col);
                                if (cell.Row.Type == DataGridRowType.Group) continue;

                                if (!sumCells.Contains(cell)) sumCells.Add(cell);
                            }
                        }
                    }


                    double total = 0;
                    int count = 0;
                    foreach (C1.WPF.DataGrid.DataGridCell cell in sumCells)
                    {
                        double result = 0.0;
                        string value = value = Util.NVC(cell.Text);

                        if (Util.NVC(cell.Text).Length > 1 && Util.NVC(cell.Text).Substring(Util.NVC(cell.Text).Length - 1, 1).Equals("%"))
                        {
                            value = Util.NVC(cell.Text).Replace("%", "");
                        }

                        if (double.TryParse(value, NumberStyles.Any, numberFormat, out result))
                        {
                            count++;
                            total += result;
                        }
                    }

                    if (count > 0)
                    {
                        string toolTipMessage = "";
                        if (total.ToString().IndexOf(".") > -1)
                        {
                            toolTipMessage = ObjectDic.Instance.GetObjectName("합계") + " : " + total.ToString("#,##0.0#########");
                        }
                        else
                        {
                            toolTipMessage = ObjectDic.Instance.GetObjectName("합계") + " : " + total.ToString("#,##0");
                        }
                        ShowToolTip(toolTipMessage, PlacementMode.MousePoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowToolTip(string message, PlacementMode mode, bool isShowToolTip = true)
        {
            if (selectionToolTip == null)
            {
                selectionToolTip = new ToolTip();
                selectionToolTip.PlacementTarget = this;
            }

            selectionToolTip.Placement = mode;
            selectionToolTip.Content = message;

            this.ToolTip = selectionToolTip.IsNvc() ? null : selectionToolTip;

            if (isShowToolTip) selectionToolTip.IsOpen = true;
        }

        private string GetConfString(StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return null;

            string returnString = string.Empty;

            if (sb.Length > 4000)
            {
                returnString = sb.ToString().Substring(0, 4000);
                sb.Remove(0, 4000);
            }
            else
            {
                returnString = sb.ToString();
                sb.Remove(0, sb.Length);
            }

            return returnString;
        }

        private void GetUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                currentConfigSet = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "SELECT";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_CONFIG_DATAGRID";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = this.Name;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    userConfigInfoColumnSet = new Dictionary<string, List<UserConfigInformation>>();
                    userConfigInfoRowCountSet = new Dictionary<string, bool>();

                    foreach (DataRow drConf in dtResult.Rows)
                    {
                        if (string.IsNullOrEmpty(currentConfigSet)) currentConfigSet = Util.NVC(drConf["CONF_KEY3"]);

                        List<UserConfigInformation> configInfos = new List<UserConfigInformation>();

                        string confString = Util.NVC(drConf["USER_CONF01"]) + Util.NVC(drConf["USER_CONF02"]) + Util.NVC(drConf["USER_CONF03"]) + Util.NVC(drConf["USER_CONF04"]) + Util.NVC(drConf["USER_CONF05"]) +
                                            Util.NVC(drConf["USER_CONF06"]) + Util.NVC(drConf["USER_CONF07"]) + Util.NVC(drConf["USER_CONF08"]) + Util.NVC(drConf["USER_CONF09"]);

                        List<string> confColumn = confString.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        foreach (string confInfoString in confColumn)
                        {
                            List<string> confSub = confInfoString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                            if (confSub.Count == 5 && this.Columns.Contains(confSub[0]))
                            {
                                configInfos.Add(new UserConfigInformation(confSub[0], Convert.ToInt16(confSub[1]),
                                    (Visibility)Visibility.Parse(typeof(Visibility), confSub[2]), Convert.ToDouble(confSub[3]),
                                    (DataGridUnitType)DataGridUnitType.Parse(typeof(DataGridUnitType), confSub[4])));
                            }
                        }

                        if (configInfos.Count > 0)
                        {
                            userConfigInfoColumnSet.Add(Util.NVC(drConf["CONF_KEY3"]), configInfos);

                            string[] options = Util.NVC(drConf["USER_CONF10"]).Split(new string[1] { "," }, StringSplitOptions.None);
                            if (options.Length > 1) userConfigInfoRowCountSet.Add(Util.NVC(drConf["CONF_KEY3"]), options[1].Equals("True"));
                        }
                    }

                    isDefaultConfigSet = true;

                    RefreshUserConfig();
                }
                else
                {
                    userConfigInfoColumnSet = null;
                    userConfigInfoRowCountSet = null;

                    if (isRowCountView) IsRowCountView = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SaveUserConfig(string configSetName, bool isDefault)
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                currentConfigSet = configSetName;

                if (userConfigInfoColumnSet == null)
                {
                    userConfigInfoColumnSet = new Dictionary<string, List<UserConfigInformation>>();
                    userConfigInfoRowCountSet = new Dictionary<string, bool>();
                }

                List<UserConfigInformation> userAddConfigs = new List<UserConfigInformation>();
                foreach (C1.WPF.DataGrid.DataGridColumn column in this.Columns)
                {
                    userAddConfigs.Add(new UserConfigInformation(column));
                }

                Dictionary<string, List<UserConfigInformation>> sortConfig = new Dictionary<string, List<UserConfigInformation>>();
                if (isDefault)
                {
                    sortConfig.Add(currentConfigSet, userAddConfigs);
                    foreach (KeyValuePair<string, List<UserConfigInformation>> userConfig in userConfigInfoColumnSet)
                    {
                        if (userConfig.Key.Equals(currentConfigSet)) continue;
                        sortConfig.Add(userConfig.Key, userConfig.Value);
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, List<UserConfigInformation>> userConfig in userConfigInfoColumnSet)
                    {
                        if (userConfig.Key.Equals(currentConfigSet)) continue;
                        sortConfig.Add(userConfig.Key, userConfig.Value);
                    }
                    sortConfig.Add(currentConfigSet, userAddConfigs);
                }
                userConfigInfoColumnSet = sortConfig;

                if (userConfigInfoRowCountSet.ContainsKey(currentConfigSet))
                {
                    userConfigInfoRowCountSet[currentConfigSet] = IsRowCountView;
                }
                else
                {
                    userConfigInfoRowCountSet.Add(currentConfigSet, isRowCountView);
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                dtRqst.Columns.Add("USER_CONF01", typeof(string));
                dtRqst.Columns.Add("USER_CONF02", typeof(string));
                dtRqst.Columns.Add("USER_CONF03", typeof(string));
                dtRqst.Columns.Add("USER_CONF04", typeof(string));
                dtRqst.Columns.Add("USER_CONF05", typeof(string));
                dtRqst.Columns.Add("USER_CONF06", typeof(string));
                dtRqst.Columns.Add("USER_CONF07", typeof(string));
                dtRqst.Columns.Add("USER_CONF08", typeof(string));
                dtRqst.Columns.Add("USER_CONF09", typeof(string));
                dtRqst.Columns.Add("USER_CONF10", typeof(string));

                StringBuilder saveUserConf = new StringBuilder();
                foreach (UserConfigInformation ucf in userConfigInfoColumnSet[currentConfigSet])
                {
                    saveUserConf.Append(ucf.ColumnName + ",");
                    saveUserConf.Append(ucf.DisplayIndex.ToString() + ",");
                    saveUserConf.Append(ucf.Visibility.ToString() + ",");
                    saveUserConf.Append(ucf.Width.ToString() + ",");
                    saveUserConf.Append(ucf.UnitType.ToString() + "|");
                }


                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = "SAVE";
                drNew["USERID"] = LoginInfo.USERID;
                drNew["CONF_TYPE"] = "USER_CONFIG_DATAGRID";
                drNew["CONF_KEY1"] = topParentName;
                drNew["CONF_KEY2"] = this.Name;
                drNew["CONF_KEY3"] = currentConfigSet;
                drNew["USER_CONF01"] = GetConfString(saveUserConf);
                drNew["USER_CONF02"] = GetConfString(saveUserConf);
                drNew["USER_CONF03"] = GetConfString(saveUserConf);
                drNew["USER_CONF04"] = GetConfString(saveUserConf);
                drNew["USER_CONF05"] = GetConfString(saveUserConf);
                drNew["USER_CONF06"] = GetConfString(saveUserConf);
                drNew["USER_CONF07"] = GetConfString(saveUserConf);
                drNew["USER_CONF08"] = GetConfString(saveUserConf);
                drNew["USER_CONF09"] = GetConfString(saveUserConf);
                drNew["USER_CONF10"] = (isDefault ? "DEFAULT" : "NONE") + "," + isRowCountView.ToString();
                dtRqst.Rows.Add(drNew);

                //string xml = dtRqst.GetXaml();
                //Clipboard.SetText(xml);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null)
                {
                    Util.MessageInfo("SFU3532", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "DELETE";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_CONFIG_DATAGRID";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = this.Name;
                dr["CONF_KEY3"] = currentConfigSet;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count == 1 && Util.NVC(dtResult.Rows[0]["USERID"]).Equals("OK"))
                {
                    userConfigInfoColumnSet.Remove(currentConfigSet);
                    userConfigInfoRowCountSet.Remove(currentConfigSet);

                    if (userConfigInfoColumnSet.Count == 0)
                    {
                        if (originalConfigInfos != null && originalConfigInfos.Count > 0)
                        {
                            foreach (UserConfigInformation userConfig in originalConfigInfos)
                            {
                                if (this.Columns.Contains(userConfig.ColumnName))
                                {
                                    this.Columns[userConfig.ColumnName].Visibility = userConfig.Visibility;
                                    this.Columns[userConfig.ColumnName].Width = new C1.WPF.DataGrid.DataGridLength(userConfig.Width, userConfig.UnitType);
                                    this.Columns[userConfig.ColumnName].DisplayIndex = userConfig.DisplayIndex;
                                }
                            }
                        }
                        currentConfigSet = string.Empty;
                    }
                    else
                    {
                        currentConfigSet = userConfigInfoColumnSet.FirstOrDefault().Key;

                        foreach (MenuItem item in mnuUserConfigList.Items)
                        {
                            if (currentConfigSet.Equals(Util.NVC(item.Tag)))
                            {
                                item.IsChecked = true;
                            }
                            else
                            {
                                item.IsChecked = false;
                            }
                        }

                        RefreshUserConfig();
                    }

                    if (userConfigInfoRowCountSet.ContainsKey(currentConfigSet))
                    {
                        IsRowCountView = userConfigInfoRowCountSet[currentConfigSet];
                    }
                    else
                    {
                        IsRowCountView = true;
                    }

                    TopRowHeaderMerge();

                    Util.MessageInfo("SFU3544", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ResetUserConfig()
        {
            try
            {
                if (originalConfigInfos != null && originalConfigInfos.Count > 0)
                {
                    foreach (UserConfigInformation userConfig in originalConfigInfos)
                    {
                        if (this.Columns.Contains(userConfig.ColumnName))
                        {
                            this.Columns[userConfig.ColumnName].Width = new C1.WPF.DataGrid.DataGridLength(userConfig.Width, userConfig.UnitType);
                            this.Columns[userConfig.ColumnName].DisplayIndex = userConfig.DisplayIndex;
                            this.Columns[userConfig.ColumnName].Visibility = userConfig.Visibility;
                        }
                    }
                }


                currentConfigSet = string.Empty;

                foreach (MenuItem item in mnuUserConfigList.Items)
                {
                    item.IsChecked = false;
                }

                TopRowHeaderMerge();

                ColumnHeaderReset?.Invoke(this);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RefreshUserConfig()
        {
            if (!userConfigInfoColumnSet.ContainsKey(currentConfigSet)) return;

            List<UserConfigInformation> newColumns = new List<UserConfigInformation>();
            foreach (UserConfigInformation ucf in originalConfigInfos)
            {
                UserConfigInformation findUcf = userConfigInfoColumnSet[currentConfigSet].Find(f => f.ColumnName.Equals(ucf.ColumnName));
                if (findUcf == null)
                {
                    newColumns.Add(ucf);
                }
            }

            int newColumnCount = 0;
            int displayIndex = 0;
            foreach (UserConfigInformation ucf in userConfigInfoColumnSet[currentConfigSet].OrderBy(o => o.DisplayIndex))
            {
                UserConfigInformation findUcf = originalConfigInfos.Find(f => f.ColumnName.Equals(ucf.ColumnName));
                if (findUcf == null) continue;

                UserConfigInformation existNewUcf = newColumns.Find(f => f.DisplayIndex == ucf.DisplayIndex);
                if (existNewUcf != null) newColumnCount++;

                if (this.Columns.Contains(ucf.ColumnName))
                {
                    if (UserConfigExceptColumns == null || !UserConfigExceptColumns.Contains(ucf.ColumnName))
                    {
                        this.Columns[ucf.ColumnName].Visibility = ucf.Visibility;
                    }
                    this.Columns[ucf.ColumnName].DisplayIndex = displayIndex + newColumnCount;
                    this.Columns[ucf.ColumnName].Width = new C1.WPF.DataGrid.DataGridLength(ucf.Width, ucf.Width == 0 ? DataGridUnitType.Auto : ucf.UnitType);
                }

                displayIndex++;
            }

            if (userConfigInfoRowCountSet.ContainsKey(currentConfigSet))
            {
                IsRowCountView = userConfigInfoRowCountSet[currentConfigSet];
            }
            this.Refresh();
        }

        private string Clipboard_SetText(DataGridSelectedItemsCollection<C1.WPF.DataGrid.DataGridCell> selectedCells, bool bheader)
        {
            try
            {
                string sClipText = string.Empty;

                if (selectedCells.Count > 0)
                {
                    int nRowIndex = 0;

                    if (bheader)
                    {
                        nRowIndex = selectedCells[0].Row.Index;

                        for (int i = 0; i < selectedCells.Count; i++)
                        {
                            if (nRowIndex == selectedCells[i].Row.Index)
                            {
                                string sColumn = selectedCells[i].Column.GetColumnText();
                                string[] sList = sColumn.Split(new string[] { ", " }, StringSplitOptions.None);

                                bool bMerge = true;

                                foreach (string str in sList)
                                    if (str != sList[0])
                                        bMerge = false;

                                sClipText += bMerge ? sList[0] : sColumn;

                                if (selectedCells.Count - 1 == i)
                                    sClipText += "\r\n";
                                else
                                    sClipText += "\t";
                            }
                            else
                            {
                                sClipText = sClipText.Substring(0, sClipText.Length - 1);
                                sClipText += "\r\n";
                                break;
                            }

                            nRowIndex = selectedCells[i].Row.Index;
                        }
                    }

                    nRowIndex = selectedCells[0].Row.Index;

                    for (int i = 0; i < selectedCells.Count; i++)
                    {
                        if (nRowIndex == selectedCells[i].Row.Index)
                        {
                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += selectedCells[i].Text;

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += "\t";
                        }
                        else
                        {
                            sClipText = sClipText.Substring(0, sClipText.Length - 1);
                            sClipText += "\r\n";

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += selectedCells[i].Text;

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += "\t";
                        }

                        nRowIndex = selectedCells[i].Row.Index;
                    }

                    sClipText = sClipText.Substring(0, sClipText.LastIndexOf('\t'));
                }

                return sClipText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SetRowCount()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBlock tb = this.FindChild<TextBlock>("RowCountText");
                if (tb != null)
                {
                    int rowCount = this.Rows.Count - this.TopRows.Count - this.BottomRows.Count;
                    tb.Text = " " + ObjectDic.Instance.GetObjectName("ROW_CNT") + " : " + rowCount.ToString("#,##0") + " ";
                }
            }));
        }

        private DataGridCellsRange FindCellRectangleHorizontal(int rowIndex, int colIndex)
        {
            try
            {
                string compSource = string.Empty;
                string compBeforeSource = string.Empty;

                C1.WPF.DataGrid.DataGridColumn dgcSource = Columns.FirstOrDefault(x => x.DisplayIndex.Equals(colIndex));

                if (dgcSource.Header is string)
                {
                    compSource = Util.NVC(dgcSource.Header);
                }
                else if (dgcSource.Header is List<string>)
                {
                    List<string> sourceHeaders = dgcSource.Header as List<string>;
                    compSource = Util.NVC(sourceHeaders[rowIndex]);
                    if (rowIndex > 0) compBeforeSource = Util.NVC(sourceHeaders[rowIndex - 1]);
                }
                else
                {
                    return null;
                }

                int rowDisplay = rowIndex;
                int colDisplay = colIndex;
                int rowFind = rowIndex;
                int colFind = colIndex;

                for (int row = rowDisplay; row < this.TopRows.Count; row++)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn dgcTarget in this.Columns.OrderBy(o => o.DisplayIndex))
                    {
                        if (dgcTarget.DisplayIndex < colDisplay) continue;

                        string compTarget = string.Empty;
                        string compBeforeTarget = string.Empty;

                        if (dgcTarget.Header is string)
                        {
                            compTarget = Util.NVC(dgcTarget.Header);
                        }
                        else if (dgcTarget.Header is List<string>)
                        {
                            List<string> targetHeaders = dgcTarget.Header as List<string>;
                            compTarget = Util.NVC(targetHeaders[row]);
                            if (row > 0) compBeforeTarget = Util.NVC(targetHeaders[row - 1]);
                        }
                        else
                        {
                            continue;
                        }

                        if (compSource.Equals(compTarget))
                        {
                            if (string.IsNullOrEmpty(compBeforeSource) || compBeforeSource.Equals(compBeforeTarget))
                            {
                                if (dgcTarget.DisplayIndex > colFind) colFind = dgcTarget.DisplayIndex;
                                if (dgcTarget.Index >= colFind && row > rowFind) rowFind = row;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                C1.WPF.DataGrid.DataGridCell leftTop = this.GetCell(rowIndex, dgcSource.Index);
                C1.WPF.DataGrid.DataGridColumn dgcLast = Columns.FirstOrDefault(x => x.DisplayIndex.Equals(colFind));
                C1.WPF.DataGrid.DataGridCell rightBottom = this.GetCell(rowFind, dgcLast.Index);
                if (leftTop != null && rightBottom != null)
                {
                    if (leftTop.Column.DisplayIndex > rightBottom.Column.DisplayIndex)
                    {
                        leftTop = this.GetCell(rowIndex, dgcLast.Index);
                        rightBottom = this.GetCell(rowFind, colIndex);
                    }
                    return new DataGridCellsRange(leftTop, rightBottom);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private DataGridCellsRange FindCellRectangleVertical(int rowIndex, int colIndex)
        {
            try
            {
                int rowDisplay = rowIndex;
                int colDisplay = colIndex;

                int rowFind = rowIndex;
                int colFind = colIndex;

                string compSource = string.Empty;
                string compNextSource = string.Empty;

                C1.WPF.DataGrid.DataGridColumn dgcSource = Columns.FirstOrDefault(x => x.DisplayIndex.Equals(colIndex));

                if (dgcSource.Header is string)
                {
                    compSource = Util.NVC(dgcSource.Header);
                    if (this.TopRows.Count > 0) rowFind = this.TopRows.Count - 1;
                }
                else if (dgcSource.Header is List<string>)
                {
                    List<string> sourceHeaders = dgcSource.Header as List<string>;
                    compSource = Util.NVC(sourceHeaders[rowIndex]);
                    for (int r = rowIndex; r < sourceHeaders.Count; r++)
                    {
                        compNextSource = Util.NVC(sourceHeaders[r]);
                        if (compSource.Equals(compNextSource)) rowFind = r;

                    }
                }
                else
                {
                    return null;
                }

                C1.WPF.DataGrid.DataGridCell leftTop = this.GetCell(rowIndex, dgcSource.Index);
                C1.WPF.DataGrid.DataGridColumn dgcLast = Columns.FirstOrDefault(x => x.DisplayIndex.Equals(colFind));
                C1.WPF.DataGrid.DataGridCell rightBottom = this.GetCell(rowFind, dgcLast.Index);
                if (leftTop != null && rightBottom != null)
                {
                    if (leftTop.Column.DisplayIndex > rightBottom.Column.DisplayIndex)
                    {
                        leftTop = this.GetCell(rowIndex, dgcLast.Index);
                        rightBottom = this.GetCell(rowFind, colIndex);
                    }
                    return new DataGridCellsRange(leftTop, rightBottom);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private SolidColorBrush GetSummaryLevelColor(int level)
        {
            byte A = 255;
            if (level >= 0 && level <= 5) A = Convert.ToByte(255 - ((level + 1) * (160 / SummaryGroupColumns.Count + 1)));
            byte R = summaryRowBackgroundColor.Color.R;
            byte G = summaryRowBackgroundColor.Color.G;
            byte B = summaryRowBackgroundColor.Color.B;

            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(A, R, G, B));
        }

        private string GetPreviewRowValue(int row, string columnName)
        {
            int beforeRowIndex = row - 1;
            while (beforeRowIndex > 0)
            {
                string beforeValue = Util.NVC(this.GetValue(beforeRowIndex, columnName));
                if (!string.IsNullOrEmpty(beforeValue)) return beforeValue;
                beforeRowIndex--;
            }
            return string.Empty;
        }

        private void MakeSummaryRow()
        {
            C1DataGrid c1DataGrid = this as C1DataGrid;
            C1GroupingWithSummariesBehavior gwsb = C1GroupingWithSummariesBehavior.GetGroupingWithSummariesBehavior(c1DataGrid);
            if (gwsb == null)
            {
                C1GroupingWithSummariesBehavior groupingWithSummariesBehavior = new C1GroupingWithSummariesBehavior();
                C1GroupingWithSummariesBehavior.SetGroupingWithSummariesBehavior(c1DataGrid, groupingWithSummariesBehavior);
            }

            foreach (C1.WPF.DataGrid.DataGridColumn column in this.Columns)
            {
                if (column is C1.WPF.DataGrid.DataGridNumericColumn)
                {
                    if (column.Name.Equals("AUTO_SUMMARY_GROUP_COLUMN")) continue;

                    DataGridAggregatesCollection dgac = DataGridAggregate.GetAggregateFunctions(column);
                    if (dgac != null) continue;

                    switch (AggregateExtension.GetAggregateColumnType(column))
                    {
                        case AggregateColumnType.AVG:
                            DataGridAggregate.SetAggregateFunctions(column, new DataGridAggregatesCollection { new DataGridAggregateAvg() });
                            break;
                        case AggregateColumnType.RATIO:
                            C1.WPF.DataGrid.DataGridColumn totalColumn = null;
                            C1.WPF.DataGrid.DataGridColumn partColumn = null;
                            foreach (C1.WPF.DataGrid.DataGridColumn columnRatio in this.Columns)
                            {
                                if (AggregateExtension.GetAggregateColumnType(columnRatio) == AggregateColumnType.RATIO_TOTAL) totalColumn = columnRatio;
                                if (AggregateExtension.GetAggregateColumnType(columnRatio) == AggregateColumnType.RATIO_PART) partColumn = columnRatio;
                            }
                            DataGridAggregate.SetAggregateFunctions(column, new DataGridAggregatesCollection { new DataGridAggregateRatio(totalColumn, partColumn) });
                            break;
                        default:
                            DataGridAggregate.SetAggregateFunctions(column, new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            break;
                    }
                }
                else if (column is C1.WPF.DataGrid.DataGridTextColumn)
                {
                    if (column.Name.Equals("AUTO_SUMMARY_GROUP_COLUMN")) continue;

                    DataGridAggregatesCollection dgac = DataGridAggregate.GetAggregateFunctions(column);
                    if (dgac != null) continue;

                    switch (AggregateExtension.GetAggregateColumnType(column))
                    {
                        case AggregateColumnType.SUM_TEXT:
                            DataGridAggregate.SetAggregateFunctions(column, new DataGridAggregatesCollection { new DataGridAggregateText(ObjectDic.Instance.GetObjectName("합계")) });
                            break;
                    }
                }
            }
        }

        private bool CheckAuth(string authList)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = authList;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT, noLogInputData: true, nologOutputData: true);
                if (dtResult == null || dtResult.Rows?.Count <= 0) return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SettingBorderLine(DataGridCellPresenter cellPresenter)
        {
            try
            {
                if (cellsBorderLineInfo == null && rangesBorderLineInfo == null) return;

                if (normalStyle != null) cellPresenter.Style = normalStyle;

                if (cellsBorderLineInfo == null) cellsBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();
                if (rangesBorderLineInfo == null) rangesBorderLineInfo = new Dictionary<System.Drawing.Point, CellBorderLineInfo>();

                System.Drawing.Point position = new System.Drawing.Point(cellPresenter.Cell.Row.Index, cellPresenter.Cell.Column.Index);
                if (!cellsBorderLineInfo.ContainsKey(position) && !rangesBorderLineInfo.ContainsKey(position)) return;

                if (baseCellStyle == null) baseCellStyle = Application.Current.Resources["UcBaseDataGridCellPresenterStyle"] as Style;
                if (baseCellStyle != null) cellPresenter.Style = baseCellStyle;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    double thickRatio = 1;
                    double actualHeight = cellPresenter.Row.ActualHeight;
                    bool isMergeCell = IsMergedCell(this.GetCell(position.X, position.Y));

                    if (isMergeCell) actualHeight = cellPresenter.ActualHeight;

                    if (cellsBorderLineInfo.ContainsKey(position) && cellsBorderLineInfo[position].LeftBorderLineInfo != null &&
                        cellsBorderLineInfo[position].LeftBorderLineInfo.BorderBrush != null)
                    {
                        object leftBorder = cellPresenter.Template.FindName("LeftBorderLine", cellPresenter);
                        if (leftBorder != null)
                        {
                            Line border = leftBorder as Line;
                            border.Stroke = cellsBorderLineInfo[position].LeftBorderLineInfo.BorderBrush;
                            border.StrokeThickness = cellsBorderLineInfo[position].LeftBorderLineInfo.BorderThickness * thickRatio;
                            switch (cellsBorderLineInfo[position].LeftBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = border.StrokeThickness / 2;
                            border.Y1 = 0;
                            border.X2 = border.StrokeThickness / 2;
                            border.Y2 = actualHeight;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (rangesBorderLineInfo.ContainsKey(position) && rangesBorderLineInfo[position].LeftBorderLineInfo != null &&
                        rangesBorderLineInfo[position].LeftBorderLineInfo.BorderBrush != null)
                    {
                        object leftBorder = cellPresenter.Template.FindName("LeftBorderLine", cellPresenter);
                        if (leftBorder != null)
                        {
                            Line border = leftBorder as Line;
                            border.Stroke = rangesBorderLineInfo[position].LeftBorderLineInfo.BorderBrush;
                            border.StrokeThickness = rangesBorderLineInfo[position].LeftBorderLineInfo.BorderThickness * thickRatio;
                            switch (rangesBorderLineInfo[position].LeftBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = border.StrokeThickness / 2;
                            border.Y1 = 0;
                            border.X2 = border.StrokeThickness / 2;
                            border.Y2 = actualHeight;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (cellsBorderLineInfo.ContainsKey(position) && cellsBorderLineInfo[position].TopBorderLineInfo != null &&
                        cellsBorderLineInfo[position].TopBorderLineInfo.BorderBrush != null)
                    {
                        object topBorder = cellPresenter.Template.FindName("TopBorderLine", cellPresenter);
                        if (topBorder != null)
                        {
                            Line border = topBorder as Line;
                            border.Stroke = cellsBorderLineInfo[position].TopBorderLineInfo.BorderBrush;
                            border.StrokeThickness = cellsBorderLineInfo[position].TopBorderLineInfo.BorderThickness * thickRatio;
                            switch (cellsBorderLineInfo[position].TopBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = 0;
                            border.Y1 = border.StrokeThickness / 2;
                            border.X2 = cellPresenter.ActualWidth;
                            border.Y2 = border.StrokeThickness / 2;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (rangesBorderLineInfo.ContainsKey(position) && rangesBorderLineInfo[position].TopBorderLineInfo != null &&
                        rangesBorderLineInfo[position].TopBorderLineInfo.BorderBrush != null)
                    {
                        object topBorder = cellPresenter.Template.FindName("TopBorderLine", cellPresenter);
                        if (topBorder != null)
                        {
                            Line border = topBorder as Line;
                            border.Stroke = rangesBorderLineInfo[position].TopBorderLineInfo.BorderBrush;
                            border.StrokeThickness = rangesBorderLineInfo[position].TopBorderLineInfo.BorderThickness * thickRatio;
                            switch (rangesBorderLineInfo[position].TopBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = 0;
                            border.Y1 = border.StrokeThickness / 2;
                            border.X2 = cellPresenter.ActualWidth;
                            border.Y2 = border.StrokeThickness / 2;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (cellsBorderLineInfo.ContainsKey(position) && cellsBorderLineInfo[position].RightBorderLineInfo != null &&
                        cellsBorderLineInfo[position].RightBorderLineInfo.BorderBrush != null)
                    {
                        object rightBorder = cellPresenter.Template.FindName("RightBorderLine", cellPresenter);
                        if (rightBorder != null)
                        {
                            Line border = rightBorder as Line;
                            border.Stroke = cellsBorderLineInfo[position].RightBorderLineInfo.BorderBrush;
                            border.StrokeThickness = cellsBorderLineInfo[position].RightBorderLineInfo.BorderThickness * thickRatio;
                            switch (cellsBorderLineInfo[position].RightBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = cellPresenter.ActualWidth - (border.StrokeThickness / 2);
                            border.Y1 = 0;
                            border.X2 = cellPresenter.ActualWidth - (border.StrokeThickness / 2);
                            border.Y2 = actualHeight;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (rangesBorderLineInfo.ContainsKey(position) && rangesBorderLineInfo[position].RightBorderLineInfo != null &&
                        rangesBorderLineInfo[position].RightBorderLineInfo.BorderBrush != null)
                    {
                        object rightBorder = cellPresenter.Template.FindName("RightBorderLine", cellPresenter);
                        if (rightBorder != null)
                        {
                            Line border = rightBorder as Line;
                            border.Stroke = rangesBorderLineInfo[position].RightBorderLineInfo.BorderBrush;
                            border.StrokeThickness = rangesBorderLineInfo[position].RightBorderLineInfo.BorderThickness * thickRatio;
                            switch (rangesBorderLineInfo[position].RightBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = cellPresenter.ActualWidth - (border.StrokeThickness / 2);
                            border.Y1 = 0;
                            border.X2 = cellPresenter.ActualWidth - (border.StrokeThickness / 2);
                            border.Y2 = actualHeight;
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (cellsBorderLineInfo.ContainsKey(position) && cellsBorderLineInfo[position].BottomBorderLineInfo != null &&
                        cellsBorderLineInfo[position].BottomBorderLineInfo.BorderBrush != null)
                    {
                        object bottomBorder = cellPresenter.Template.FindName("BottomBorderLine", cellPresenter);
                        if (bottomBorder != null)
                        {
                            Line border = bottomBorder as Line;
                            border.Stroke = cellsBorderLineInfo[position].BottomBorderLineInfo.BorderBrush;
                            border.StrokeThickness = cellsBorderLineInfo[position].BottomBorderLineInfo.BorderThickness * thickRatio;
                            switch (cellsBorderLineInfo[position].BottomBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 5, border.StrokeThickness * 2 };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 5, border.StrokeThickness * 2, border.StrokeThickness * 2, border.StrokeThickness * 2 };
                                    break;
                            }
                            border.X1 = 0;
                            border.Y1 = actualHeight - (border.StrokeThickness / 2);
                            border.X2 = cellPresenter.ActualWidth;
                            border.Y2 = actualHeight - (border.StrokeThickness / 2);
                            border.Visibility = Visibility.Visible;
                        }
                    }

                    if (rangesBorderLineInfo.ContainsKey(position) && rangesBorderLineInfo[position].BottomBorderLineInfo != null &&
                        rangesBorderLineInfo[position].BottomBorderLineInfo.BorderBrush != null)
                    {
                        object bottomBorder = cellPresenter.Template.FindName("BottomBorderLine", cellPresenter);
                        if (bottomBorder != null)
                        {
                            Line border = bottomBorder as Line;
                            border.Stroke = rangesBorderLineInfo[position].BottomBorderLineInfo.BorderBrush;
                            border.StrokeThickness = rangesBorderLineInfo[position].BottomBorderLineInfo.BorderThickness * thickRatio;
                            switch (rangesBorderLineInfo[position].BottomBorderLineInfo.IsDot)
                            {
                                case BorderLineStyle.Line:
                                    border.StrokeDashArray = null;
                                    break;
                                case BorderLineStyle.SmallDot:
                                    border.StrokeDashArray = new DoubleCollection { 1, 1 };
                                    break;
                                case BorderLineStyle.Dot:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.Dash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness };
                                    break;
                                case BorderLineStyle.SinglePointDash:
                                    border.StrokeDashArray = new DoubleCollection { border.StrokeThickness * 2, border.StrokeThickness, border.StrokeThickness, border.StrokeThickness };
                                    break;
                            }
                            border.X1 = 0;
                            border.Y1 = actualHeight - (border.StrokeThickness / 2);
                            border.X2 = cellPresenter.ActualWidth;
                            border.Y2 = actualHeight - (border.StrokeThickness / 2);
                            border.Visibility = Visibility.Visible;
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SettingAlert(DataGridCellPresenter cellPresenter)
        {
            try
            {
                if (cellsAlertInfo == null) return;

                if (normalStyle != null) cellPresenter.Style = normalStyle;

                if (cellsAlertInfo == null) cellsAlertInfo = new Dictionary<System.Drawing.Point, CellAlertInfo>();

                System.Drawing.Point position = new System.Drawing.Point(cellPresenter.Cell.Row.Index, cellPresenter.Cell.Column.Index);
                if (!cellsAlertInfo.ContainsKey(position)) return;

                if (baseCellStyle == null) baseCellStyle = Application.Current.Resources["UcBaseDataGridCellPresenterStyle"] as Style;
                if (baseCellStyle != null) cellPresenter.Style = baseCellStyle;

                if (cellsAlertInfo.ContainsKey(position))
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        object cellBackgroundGrid = cellPresenter.Template.FindName("CellBackgroundGrid", cellPresenter);
                        if (cellBackgroundGrid != null)
                        {
                            Grid grid = cellBackgroundGrid as Grid;
                            if (grid != null && !grid.Background.HasAnimatedProperties)
                            {
                                System.Windows.Media.Brush aniBrush = new SolidColorBrush((cellPresenter.Background as SolidColorBrush).Color);
                                if (cellsAlertInfo[position].Background != null)
                                {
                                    aniBrush = new SolidColorBrush((cellsAlertInfo[position].Background as SolidColorBrush).Color);
                                }

                                DoubleAnimation opacityAni = new DoubleAnimation
                                {
                                    From = 1.5,
                                    To = 0.3,
                                    Duration = TimeSpan.FromMilliseconds(cellsAlertInfo[position].SpeedMillisecond),
                                    AutoReverse = true,
                                    RepeatBehavior = cellsAlertInfo[position].Repeat
                                };

                                Random rand = new Random(DateTime.Now.Millisecond);
                                int stratTime = rand.Next(0, cellsAlertInfo[position].SpeedMillisecond);
                                opacityAni.BeginTime = TimeSpan.FromMilliseconds(stratTime);

                                aniBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAni);

                                grid.Background = aniBrush;
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region [Event]

        #region 팝업 메뉴 관련
        private void MnuUserConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (originalConfigInfos == null || originalConfigInfos.Count == 0) return;

                CMM_COLUMN_VISIBLE popColumnVisible = new CMM_COLUMN_VISIBLE();

                if (popColumnVisible != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = this;
                    Parameters[1] = originalConfigInfos;
                    Parameters[2] = isRowCountView;

                    ControlsLibrary.C1WindowExtension.SetParameters(popColumnVisible, Parameters);

                    popColumnVisible.Closed += new EventHandler(popColumnVisible_Closed);
                    popColumnVisible.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SubUserConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem mnuConfig = sender as MenuItem;
                foreach (MenuItem item in mnuUserConfigList.Items)
                {
                    if (Util.NVC(mnuConfig.Header).Equals(Util.NVC(item.Header)))
                    {
                        item.IsChecked = true;
                    }
                    else
                    {
                        item.IsChecked = false;
                    }
                }

                if (userConfigInfoColumnSet != null && userConfigInfoColumnSet.Count > 0)
                {
                    isDefaultConfigSet = userConfigInfoColumnSet.First().Key.Equals(mnuConfig.Header);
                }
                else
                {
                    isDefaultConfigSet = false;
                }

                if (userConfigInfoColumnSet.ContainsKey(Util.NVC(mnuConfig.Tag)))
                {
                    currentConfigSet = Util.NVC(mnuConfig.Tag);

                    RefreshUserConfig();

                    TopRowHeaderMerge();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuUserConfigSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_USER_CONF_INPUT_TEXT popInputText = new CMM_USER_CONF_INPUT_TEXT();

                popInputText.FrameOperation = FrameOperation;
                object[] param = new object[4];
                param[0] = ObjectDic.Instance.GetObjectName("USER_CONFIG");
                param[1] = ObjectDic.Instance.GetObjectName("설정 이름");
                param[2] = currentConfigSet;
                if (userConfigInfoColumnSet == null || userConfigInfoColumnSet.Count == 0)
                {
                    param[3] = true;
                }
                else
                {
                    param[3] = isDefaultConfigSet;
                }

                C1WindowExtension.SetParameters(popInputText, param);

                popInputText.Closed += new EventHandler(popInputText_Closed);
                popInputText.ShowModal();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void MnuUserConfigDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(currentConfigSet))
                {
                    Util.MessageValidation("SFU1636");
                    return;
                }

                if (isDefaultConfigSet)
                {
                    Util.MessageConfirm("SFU4332", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteUserConfig();
                        }
                    }, currentConfigSet);
                }
                else
                {
                    DeleteUserConfig();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuUserConfigReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ResetUserConfig();
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuCopyAllWithHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                string str = string.Empty;

                ContextMenu itemContext = menuItem?.Parent as ContextMenu;
                C1DataGrid dg = itemContext?.PlacementTarget as C1DataGrid;

                if (dg != null)
                {
                    //Header Text 적출
                    if (dg.TopRows.Count == 0 && dg.HeadersVisibility != C1.WPF.DataGrid.DataGridHeadersVisibility.None) //Header가 Single Row이고 HeaderVisibility가 None이 아닌 경우 별도의 Header Text 적출 로직 적용
                    {
                        for (int i = 0; i < dg.Columns.Count; i++)
                            if (dg.Columns[i].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                                str += dg.Columns[i].ActualGroupHeader?.ToString() + (i.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)

                        str += "\r\n"; //Row가 바뀌므로 줄 바꿈
                    }
                    else
                    {
                        for (int i = 0; i < dg.TopRows.Count; i++) //Header 적출
                        {
                            for (int j = 0; j < dg.Columns.Count; j++)
                                if (dg.Columns[j].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                                    str += dg.GetCell(i, j)?.Value + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)

                            str += "\r\n"; //Row가 바뀌므로 줄 바꿈
                        }
                    }

                    //본문 내용 적출
                    for (int i = dg.TopRows.Count; i < dg.Rows.Count; i++)
                    {
                        for (int j = 0; j < dg.Columns.Count; j++)
                        {
                            if (dg.Columns[j].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                            {
                                if (dg.GetCell(i, j).Value != null && dg.GetCell(i, j).Value.ToString().IndexOf("\n") > -1)
                                {
                                    // 한 셀에 여러라인이 있을때 처리
                                    str += ("\"" + dg.GetCell(i, j).Value?.ToString() + "\"").Replace("\r\n", " ") + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)
                                }
                                else
                                {
                                    str += dg.GetCell(i, j).Value?.ToString().Replace("\r\n", " ") + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)
                                }
                            }
                        }

                        if (!i.Equals(dg.Rows.Count - 1)) //마지막 Row가 아닐 경우
                            str += "\r\n"; //줄 바꿈
                    }

                    Clipboard.SetText(str.Replace("System.Data.DataRowView", string.Empty)); //ClipBoard에 적출 내용 복사
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuCopyWithHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem itemmenu = sender as MenuItem;

                if (itemmenu != null)
                {
                    ContextMenu itemcontext = itemmenu.Parent as ContextMenu;

                    if (itemcontext != null)
                    {
                        C1DataGrid dg = itemcontext.PlacementTarget as C1DataGrid;

                        if (dg != null)
                            System.Windows.Clipboard.SetText(Clipboard_SetText(dg.Selection.SelectedCells, true));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuExcelSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ExcelExport(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuExcelSaveAndOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ExcelExport(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuDeclare_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lastArgumentDeclare);
        }

        private void MnuCopyXaml_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lastArgumentXaml);
        }

        private void ItemsSourceTableChanged(object sender, EventArgs e)
        {
            if (isRowCountView)
            {
                if (this.ItemsSource != null && this.ItemsSource is DataView)
                {
                    dvDefault = this.ItemsSource as DataView;
                    dvDefault.ListChanged -= Dv_ListChanged;
                    dvDefault.ListChanged += Dv_ListChanged;
                }

                SetRowCount();
            }
        }

        private void Dv_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (isRowCountView) SetRowCount();
        }

        #endregion

        #region Header Check All
        private void ChkDataGridAll_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                for (int idx = 0; idx < this.Rows.Count; idx++)
                {
                    C1.WPF.DataGrid.DataGridRow row = this.Rows[idx];
                    if (row != null)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        CheckAllChanging?.Invoke(this, idx, true, e);
                    }
                }

                CheckAllChanged?.Invoke(this, true, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChkDataGridAll_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                for (int idx = 0; idx < this.Rows.Count; idx++)
                {
                    C1.WPF.DataGrid.DataGridRow row = this.Rows[idx];
                    if (row != null)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        CheckAllChanging?.Invoke(this, idx, false, e);
                    }
                }

                CheckAllChanged?.Invoke(this, false, e);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void MoveAni_Completed(object sender, EventArgs e)
        {
            if (loadingIndicator == null) return;

            if (loadingIndicator.Visibility == Visibility.Visible) loadingIndicator.Visibility = Visibility.Collapsed;

            TextBlock tb = loadingIndicator.FindChild<TextBlock>("tb");
            if (tb != null) tb.Text = loadingText;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] results = new object[4];

            try
            {
                object[] arguments = e.Argument as object[];

                string bizRuleID = arguments[0] as string;
                string inData = arguments[1] as string;
                string outData = arguments[2] as string;

                results[0] = arguments[3]; // inDataTable
                results[1] = arguments[4]; // argument
                results[2] = arguments[5];

                if (arguments[3] is DataTable)
                {
                    DataTable inDataTable = arguments[3] as DataTable;
                    if (outData.IndexOf(",") < 0)
                    {
                        lastArgumentDeclare = "----- " + bizRuleID + " -----\r\n";
                        lastArgumentDeclare += inDataTable.GetDeclare();
                        lastArgumentXaml = "----- " + bizRuleID + " -----\r\n";
                        lastArgumentXaml += inDataTable.GetXaml();

                        results[3] = new ClientProxy().ExecuteServiceSync(bizRuleID, inData, outData, inDataTable);
                    }
                    else
                    {
                        DataSet inDataSet = new DataSet();
                        inDataSet.Tables.Add(inDataTable.Copy());

                        lastArgumentDeclare = "----- " + bizRuleID + " -----\r\n";
                        lastArgumentDeclare += inDataSet.GetDeclare();
                        lastArgumentXaml = "----- " + bizRuleID + " -----\r\n";
                        lastArgumentXaml += inDataSet.GetXml();

                        results[3] = new ClientProxy().ExecuteServiceSync_Multi(bizRuleID, inData, outData, inDataSet);
                    }
                }
                else if (arguments[3] is DataSet)
                {
                    DataSet inDataSet = arguments[3] as DataSet;

                    lastArgumentDeclare = "----- " + bizRuleID + " -----\r\n";
                    lastArgumentDeclare += inDataSet.GetDeclare();
                    lastArgumentXaml = "----- " + bizRuleID + " -----\r\n";
                    lastArgumentXaml += inDataSet.GetXml();

                    results[3] = new ClientProxy().ExecuteServiceSync_Multi(bizRuleID, inData, outData, inDataSet);
                }

                e.Result = results;

                bgWorker.ReportProgress(100);

                if (ExecuteDataDoWork != null)
                {
                    ExecuteDataDoWork?.Invoke(this, new ExecuteDataEventArgs(arguments[3], results[3], arguments, null));
                }

                Thread.Sleep(100); // LoadingIndicator 메세지 갱신시간.
            }
            catch (Exception ex)
            {
                results[3] = ex;
                e.Result = results;
            }
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (loadingIndicator == null) return;

                TextBlock tb = loadingIndicator.FindChild<TextBlock>("tb");
                if (e.ProgressPercentage == 0)
                {
                    if (tb != null) tb.Text = loadingText;
                }
                else if (e.ProgressPercentage == 100)
                {
                    if (tb != null) tb.Text = applyingText;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null) return;

            object[] argumentsWork = e.Result as object[];
            object inData = argumentsWork[0];
            object arguments = argumentsWork[1];
            string bindTableName = Util.NVC(argumentsWork[2]);
            object dataSource = argumentsWork[3];

            if (dataSource is Exception)
            {
                if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible) loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException((Exception)dataSource);

                ExecuteDataCompleted?.Invoke(this, new ExecuteDataEventArgs(inData, null, arguments, (Exception)dataSource));
                return;
            }

            try
            {
                if (dataSource == null)
                {
                    ExecuteDataCompleted?.Invoke(this, new ExecuteDataEventArgs(inData, dataSource, arguments, null));
                    return;
                }

                if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
                {
                    loadingIndicator.BeginAnimation(OpacityProperty, moveAni);
                }

                if (isCheckAllColumnUse)
                {
                    if (dataSource is DataTable)
                    {
                        DataTable dtResult = dataSource as DataTable;
                        if (dtResult != null && !dtResult.Columns.Contains("CHK"))
                        {
                            dtResult.Columns.Add("CHK", typeof(bool));
                            dtResult.AsEnumerable().ToList<DataRow>().ForEach(f => f["CHK"] = false);
                        }
                    }
                    else if (dataSource is DataSet)
                    {
                        DataSet dsResult = dataSource as DataSet;
                        if (dsResult.Tables.Count > 0)
                        {
                            if (bindTableName.Equals(string.Empty))
                            {
                                DataTable dtResult = dsResult.Tables[0];
                                if (dtResult != null && !dtResult.Columns.Contains("CHK"))
                                {
                                    dtResult.Columns.Add("CHK", typeof(bool));
                                    dtResult.AsEnumerable().ToList<DataRow>().ForEach(f => f["CHK"] = false);
                                }
                            }
                            else
                            {
                                if (dsResult.Tables.Contains(bindTableName))
                                {
                                    DataTable dtResult = dsResult.Tables[bindTableName];
                                    if (dtResult != null && !dtResult.Columns.Contains("CHK"))
                                    {
                                        dtResult.Columns.Add("CHK", typeof(bool));
                                        dtResult.AsEnumerable().ToList<DataRow>().ForEach(f => f["CHK"] = false);
                                    }
                                }
                            }
                        }
                    }
                }

                if (ExecuteDataModify != null)
                {
                    ExecuteDataModify?.Invoke(this, new ExecuteDataEventArgs(inData, dataSource, arguments, null));
                }

                if (ExecuteCustomBinding != null)
                {
                    ExecuteCustomBinding?.Invoke(this, new ExecuteDataEventArgs(inData, dataSource, arguments, null));
                }
                else
                {
                    if (dataSource is DataTable)
                    {
                        DataTable dtResult = (DataTable)dataSource;

                        if (isUserConfigUse && userConfigInfoColumnSet != null && userConfigInfoColumnSet.Count > 0)
                        {
                            this.SetItemsSource(dtResult, FrameOperation, false);

                            RefreshUserConfig();
                        }
                        else
                        {
                            this.SetItemsSource(dtResult, FrameOperation, isAutoWidth);
                        }
                    }
                    else if (dataSource is DataSet)
                    {
                        DataSet dsResult = (DataSet)dataSource;

                        if (dsResult.Tables.Count > 0)
                        {
                            DataTable bindTable = dsResult.Tables[0];
                            if (!string.IsNullOrEmpty(bindTableName) && dsResult.Tables.Contains(bindTableName))
                            {
                                bindTable = dsResult.Tables[bindTableName];
                            }

                            if (isUserConfigUse && userConfigInfoColumnSet != null && userConfigInfoColumnSet.Count > 0)
                            {
                                this.SetItemsSource(bindTable, FrameOperation, false);
                            }
                            else
                            {
                                this.SetItemsSource(bindTable, FrameOperation, isAutoWidth);
                            }
                        }
                    }
                }

                if (isUserConfigUse && IsUserConfigUsing)
                {
                    RefreshUserConfig();
                }

                if (isSummaryRowApply)
                {
                    MakeSummaryRow();

                    this.AlternatingRowBackground = System.Windows.Media.Brushes.White;

                    DataTable dtResult = this.GetDataTable(false);

                    if (!dtResult.Columns.Contains("AUTO_SUMMARY_GROUP_COLUMN"))
                    {
                        dtResult.Columns.Add("AUTO_SUMMARY_GROUP_COLUMN");

                        this.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                        {
                            Name = "AUTO_SUMMARY_GROUP_COLUMN",
                            Header = "AUTO_SUMMARY_GROUP_COLUMN",
                            Binding = new Binding()
                            {
                                Path = new PropertyPath("AUTO_SUMMARY_GROUP_COLUMN"),
                                Mode = BindingMode.TwoWay
                            },
                            IsReadOnly = true,
                            Visibility = Visibility.Collapsed
                        });

                        if (!UserConfigExceptColumns.Contains("AUTO_SUMMARY_GROUP_COLUMN")) UserConfigExceptColumns.Add("AUTO_SUMMARY_GROUP_COLUMN");
                    }

                    Dictionary<C1.WPF.DataGrid.DataGridColumn, DataGridSortDirection> summaryColumns = new Dictionary<C1.WPF.DataGrid.DataGridColumn, DataGridSortDirection>();
                    summaryColumns.Add(this.Columns["AUTO_SUMMARY_GROUP_COLUMN"], DataGridSortDirection.None);

                    for (int index = 1; index <= SummaryGroupColumns.Count; index++)
                    {
                        C1.WPF.DataGrid.DataGridColumn groupColumn = this.Columns[SummaryGroupColumns[index - 1]];
                        summaryColumns.Add(groupColumn, DataGridSortDirection.None);

                        DataGridAggregate.SetAggregateFunctions(groupColumn, new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = " " } });
                    }
                    this.GroupBy(summaryColumns.ToArray());

                    C1.WPF.DataGrid.DataGridCell leftTop = null;
                    C1.WPF.DataGrid.DataGridCell rightBottom = null;
                    foreach (string groupColumnName in SummaryGroupColumns)
                    {
                        leftTop = rightBottom = null;
                        for (int rowIndex = 0; rowIndex < this.Rows.Count; rowIndex++)
                        {
                            if (this.Rows[rowIndex].Type != DataGridRowType.Item &&
                                this.Rows[rowIndex].Type != DataGridRowType.Group) continue;

                            C1.WPF.DataGrid.DataGridCell cell = this.GetCell(rowIndex, this.Columns[groupColumnName].Index);

                            if (leftTop == null)
                            {
                                if (this.Rows[rowIndex].Type == DataGridRowType.Item) leftTop = rightBottom = cell;
                                continue;
                            }

                            if (this.Rows[rowIndex].Type == DataGridRowType.Group)
                            {
                                C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRow dggwsr = this.Rows[rowIndex] as C1.WPF.DataGrid.Summaries.DataGridGroupWithSummaryRow;
                                if (dggwsr.Column.Name == groupColumnName)
                                {
                                    MergeCells(leftTop, rightBottom);
                                    leftTop = rightBottom = null;
                                    continue;
                                }
                            }

                            rightBottom = cell;
                        }
                    }
                }

                ExecuteDataCompleted?.Invoke(this, new ExecuteDataEventArgs(inData, dataSource, arguments, null));
            }
            catch (Exception ex)
            {
                if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible) loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);

                ExecuteDataCompleted?.Invoke(this, new ExecuteDataEventArgs(inData, dataSource, arguments, ex));
            }
        }

        private void popColumnVisible_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM_COLUMN_VISIBLE popColumnVisible = sender as CMM_COLUMN_VISIBLE;
                if (popColumnVisible.DialogResult != MessageBoxResult.OK) return;

                IsRowCountView = popColumnVisible.IsRowCountView;

                if (userConfigInfoRowCountSet != null && userConfigInfoRowCountSet.ContainsKey(currentConfigSet))
                {
                    userConfigInfoRowCountSet[currentConfigSet] = IsRowCountView;
                }

                TopRowHeaderMerge();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popInputText_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM_USER_CONF_INPUT_TEXT popInputText = sender as CMM_USER_CONF_INPUT_TEXT;
                if (string.IsNullOrEmpty(popInputText.ReturnText)) return;
                if (popInputText.DialogResult != MessageBoxResult.OK) return;

                isDefaultConfigSet = popInputText.ReturnCheck;

                SaveUserConfig(popInputText.ReturnText, isDefaultConfigSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}