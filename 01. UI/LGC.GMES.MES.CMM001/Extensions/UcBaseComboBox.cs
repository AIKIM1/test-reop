/*************************************************************************************
 Created Date : 2022.07.14
      Creator : 
   Decription : ComboBox Extension
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.14  조영대 : Initial Created. (검색기능이 추가된 콤보박스) 
                       - AlphaNumeric 만 검색, IME 검색(한글,한문)안됨(Ctro+V 로 붙여넣기시 한글,한문 검색됨).
                       - DropDownOpen 상태에서 좌우측화살표키로 이전검색, 다음검색 지원
  2025.01.25 안민호  : (콤보박스) base.Items.Clear() 하기 전에 base.ItemsSource = null 추가
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.ComponentModel;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Collections;
using C1.WPF;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseComboBox : C1.WPF.C1ComboBox, IControlValidation
    {
        #region EventHandler

        public class CheckedChangedEventArgs : EventArgs
        {
            public CheckedChangedEventArgs(int changeIndex, DataRow changeDataRow, bool oldValue, bool newValue, Dictionary<int, bool> checkStateList)
            {
                ChangeIndex = changeIndex;
                ChangeDataRow = changeDataRow;
                OldValue = oldValue;
                NewVAlue = newValue;
                CheckStateList = checkStateList;
            }

            public int ChangeIndex { get; set; }
            public DataRow ChangeDataRow { get; set; }
            public bool OldValue { get; set; }
            public bool NewVAlue { get; set; }
            public Dictionary<int, bool> CheckStateList { get; set; }

        }

        public event CheckedChangedEventHandler CheckedChanged;

        public delegate void CheckedChangedEventHandler(object sender, CheckedChangedEventArgs e);

        #endregion

        #region Property

        private Visibility validationVisibility = Visibility.Collapsed;
        [Category("GMES"), Browsable(false), DefaultValue(Visibility.Collapsed), Description("Validation Visibility")]
        public Visibility ValidationVisibility
        {
            get
            {
                return (Visibility)GetValue(ValidationVisibilityProperty);
            }
            set
            {
                SetValue(ValidationVisibilityProperty, value);
            }
        }
        public static readonly DependencyProperty ValidationVisibilityProperty =
            DependencyProperty.Register("ValidationVisibility", typeof(Visibility), typeof(UcBaseComboBox), new PropertyMetadata(Visibility.Collapsed, ValidationVisibilityPropertyChangedCallback));

        private static void ValidationVisibilityPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UcBaseComboBox ucBaseComboBox = d as UcBaseComboBox;
            ucBaseComboBox.validationVisibility = (Visibility)e.NewValue;
            ucBaseComboBox.InvalidateVisual();
        }

        private bool isUserConfigUse = true;
        [Category("GMES"), DefaultValue(true), Description("사용자 설정 사용 유무")]
        public bool IsUserConfigUse
        {
            get { return isUserConfigUse; }
            set { isUserConfigUse = value; }
        }

        private bool isUserConfigValue = false;
        [Browsable(false)]
        public bool IsUserConfigValue
        {
            get { return isUserConfigValue; }
            set { isUserConfigValue = value; }
        }

        [Browsable(false)]
        public new IEnumerable ItemsSource
        {
            get { return base.ItemsSource; }
            set
            {
                base.ItemsSource = null;
                base.Items.Clear();

                base.ItemsSource = value;
            }
        }

        [Browsable(false)]
        public new ItemCollection Items
        {
            get
            {
                return base.Items;
            }
        }

        #region CheckBox 관련
        private bool isCheckBoxUse = false;
        [Category("GMES"), DefaultValue(false), Description("체크박스 사용 유무")]
        public bool IsCheckBoxUse
        {
            get { return isCheckBoxUse; }
            set
            {
                isCheckBoxUse = value;

                if (isCheckBoxUse) this.AutoComplete = false;

                SetCheckBoxStyle();
            }
        }

        private bool isAllUsed = true;
        [Category("GMES"), DefaultValue(true), Description("첫행 선택 및 해제시 전체 여부")]
        public bool IsAllUsed
        {
            get { return isAllUsed; }
            set { isAllUsed = value; }
        }

        private string isFirstText = "All";
        [Category("GMES"), DefaultValue("All"), Description("첫행 Text")]
        public string IsFirstText
        {
            get { return isFirstText; }
            set { isFirstText = value; }
        }

        private bool isCheckAll = false;
        [Category("GMES"), DefaultValue(false), Description("전체 선택 여부")]
        public bool IsCheckAll
        {
            get { return isCheckAll; }
        }

        private bool isUnCheckAll = true;
        [Category("GMES"), DefaultValue(true), Description("전체 비선택 여부")]
        public bool IsUnCheckAll
        {
            get { return isUnCheckAll; }
        }

        [Browsable(false), DefaultValue(null)]
        public List<int> SelectedCheckedIndex
        {
            get
            {
                try
                {
                    if (isCheckBoxUse)
                    {
                        if (isUserConfigUse && string.IsNullOrEmpty(topParentName))
                        {
                            topParentName = this.FindPageName();
                            if (!string.IsNullOrEmpty(topParentName))
                            {
                                GetUserConfig();
                            }
                        }

                        List<int> returnItems = checkStateList.Where(w => w.Value == true).Select(s => s.Key).ToList();
                        return returnItems == null || returnItems.Count == 0 ? null : returnItems;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return null;
            }
        }

        [Browsable(false), DefaultValue(null)]
        public List<C1ComboBoxItem> SelectedCheckedItem

        {
            get
            {
                try
                {
                    if (isCheckBoxUse)
                    {
                        if (this.SelectedCheckedIndex == null) return null;

                        List<C1ComboBoxItem> returnItems = new List<C1ComboBoxItem>();
                        foreach (int itemIndex in this.SelectedCheckedIndex)
                        {
                            if (IsAllUsed && itemIndex == 0) continue;

                            if (itemIndex < base.Items.Count && base.Items[itemIndex] is C1ComboBoxItem) returnItems.Add(this.Items[itemIndex] as C1ComboBoxItem);
                        }
                        return returnItems.Count == 0 ? null : returnItems;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return null;
            }
        }

        [Browsable(false), DefaultValue(null)]
        public List<string> SelectedCheckedValue
        {
            get
            {
                try
                {
                    if (isCheckBoxUse)
                    {
                        if (this.SelectedCheckedItem == null) return null;

                        List<string> returnItems = new List<string>();
                        foreach (C1ComboBoxItem item in this.SelectedCheckedItem)
                        {
                            if (item.DataContext is DataRow)
                            {
                                DataRow dataRow = item.DataContext as DataRow;
                                if (!string.IsNullOrEmpty(this.SelectedValuePath) && dataRow.Table.Columns.Contains(this.SelectedValuePath))
                                {
                                    returnItems.Add(dataRow[this.SelectedValuePath].Nvc());
                                }
                            }
                            else
                            {
                                returnItems.Add(item.Content.Nvc());
                            }
                        }
                        return returnItems.Count == 0 ? null : returnItems;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return null;
            }
        }

        [Browsable(false), DefaultValue(null)]
        public List<string> SelectedCheckedText
        {
            get
            {
                try
                {
                    if (isCheckBoxUse)
                    {
                        if (this.SelectedCheckedItem == null) return null;

                        List<string> returnItems = new List<string>();
                        foreach (C1ComboBoxItem item in this.SelectedCheckedItem)
                        {
                            if (item.DataContext is DataRow)
                            {
                                DataRow dataRow = item.DataContext as DataRow;
                                if (!string.IsNullOrEmpty(this.DisplayMemberPath) && dataRow.Table.Columns.Contains(this.DisplayMemberPath))
                                {
                                    returnItems.Add(dataRow[this.DisplayMemberPath].Nvc());
                                }
                            }
                            else
                            {
                                returnItems.Add(item.Content.Nvc());
                            }
                        }
                        return returnItems.Count == 0 ? null : returnItems;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return null;
            }
        }

        [Browsable(false), DefaultValue("")]
        public string SelectedValuesToString
        {
            get
            {
                string returnString = string.Empty;

                try
                {
                    if (isCheckBoxUse)
                    {
                        if (this.SelectedCheckedValue != null && this.SelectedCheckedValue.Count > 0) returnString = string.Join(",", this.SelectedCheckedValue);

                        return returnString;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                return returnString;
            }
        }

        [Browsable(false), DefaultValue("")]
        public string SelectedItemsToString
        {
            get { return SelectedValuesToString; }
        }
        #endregion

        #endregion


        #region Declaration & Constructor 

        private Style originalStyle = null;

        private System.Timers.Timer openTimer;
        private int openTimerCount = 0;

        private System.Timers.Timer searchTimer;
        private ToolTip searchToolTip = null;

        private System.Timers.Timer validationTimer;
        private ToolTip validationToolTip = null;

        private string searchValue = string.Empty;
        private string saveValue = string.Empty;
        private string valuePath = string.Empty;
        private string displayPath = string.Empty;

        private ContextMenu contextMenu = null;
        private ContextMenu saveContextMenu = null;
        private MenuItem mnuUserConfig = null;
        private bool isExistsUserConfig = false;

        private string topParentName = string.Empty;
        private UcBaseComboBox previewComboBox = null;

        private string userConfigValue = string.Empty;
        private DateTime orderDateTime = DateTime.MinValue;

        private Dictionary<int, CheckBox> checkBoxList = null;
        private Dictionary<int, bool> checkStateList = null;

        private bool isCheckEvent = true;

        #endregion


        #region Initialize
        public UcBaseComboBox()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }

        public void InitializeControls()
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;

                if (openTimer == null)
                {
                    openTimer = new System.Timers.Timer(200);
                    openTimer.Elapsed += OpenTimer_Elapsed;
                    openTimer.Stop();
                }

                if (searchTimer == null)
                {
                    searchTimer = new System.Timers.Timer(1000);
                    searchTimer.Elapsed += SearchTimer_Elapsed;
                    searchTimer.Stop();
                }

                if (searchToolTip == null)
                {
                    searchToolTip = new ToolTip();
                    searchToolTip.StaysOpen = true;
                    searchToolTip.PlacementTarget = this;
                    searchToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                }

                if (originalStyle == null)
                {
                    if (this.Style == null)
                    {
                        this.Style = Application.Current.Resources["C1ComboBoxStyle"] as Style;
                    }

                    originalStyle = this.Style;

                    if (!Application.Current.Resources.Contains("UcBaseComboBoxStyle"))
                    {
                        ResourceDictionary resourceDic = new ResourceDictionary();
                        resourceDic.Source = new Uri(@"/LGC.GMES.MES.CMM001;component/Extensions/UcBaseStyle.xaml", UriKind.Relative);

                        Application.Current.Resources.MergedDictionaries.Insert(Application.Current.Resources.MergedDictionaries.Count - 3, resourceDic);
                    }

                    if (Application.Current.Resources.Contains("UcBaseComboBoxStyle"))
                    {
                        this.Style = Application.Current.Resources["UcBaseComboBoxStyle"] as Style;
                        this.ApplyTemplate();
                    }

                    foreach (Setter setter in originalStyle.Setters)
                    {
                        switch (setter.Property.ToString())
                        {
                            case "Background":
                            case "Foreground":
                            case "FontSize":
                            case "FontFamily":
                                this.SetValue(setter.Property, setter.Value);
                                break;
                        }
                    }
                }

                this.Loaded += UcBaseComboBox_Loaded;
                this.Unloaded += UcBaseComboBox_Unloaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsCheckBoxUse && isAllUsed)
                {
                    isCheckAll = true;
                    isUnCheckAll = false;
                    CheckAll();
                }

                if (isUserConfigUse)
                {
                    topParentName = this.FindPageName();
                    if (!string.IsNullOrEmpty(topParentName))
                    {
                        GetUserConfig();

                        this.Loaded -= UcBaseComboBox_Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UcBaseComboBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (isUserConfigUse && isExistsUserConfig)
            {
                SaveUserConfig();
            }
        }

        #endregion

        #region Event

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            orderDateTime = DateTime.Now;

            if (isCheckBoxUse) SetCheckBoxStyle();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            try
            {
                if (isCheckBoxUse)
                {
                    if (!isCheckEvent) return;

                    isCheckEvent = false;

                    if (e.Action.Equals(NotifyCollectionChangedAction.Add) || e.Action.Equals(NotifyCollectionChangedAction.Replace))
                    {
                        if (isAllUsed && !string.IsNullOrEmpty(isFirstText) && this.Items.Count == 1 && this.Items[0] is C1ComboBoxItem)
                        {
                            C1ComboBoxItem comboItem = this.Items[0] as C1ComboBoxItem;
                            if (!comboItem.Content.Nvc().Equals(isFirstText))
                            {
                                C1ComboBoxItem allItem = new C1ComboBoxItem();
                                allItem.Content = isFirstText;
                                allItem.DataContext = null;
                                allItem.Style = Application.Current.Resources["UcBaseCheckComboBoxItemStyle"] as Style;

                                allItem.Loaded -= ComboItem_Loaded;
                                allItem.Loaded += ComboItem_Loaded;

                                this.Items.Insert(0, allItem);

                                if (checkStateList == null) checkStateList = new Dictionary<int, bool>();
                                if (!checkStateList.ContainsKey(0)) checkStateList.Add(0, true);
                            }
                            this.Text = isFirstText;
                            SetCheckedTooltipText();
                        }

                        foreach (var item in e.NewItems)
                        {
                            if (item is C1ComboBoxItem)
                            {
                                C1ComboBoxItem comboItem = item as C1ComboBoxItem;
                                if (isCheckBoxUse)
                                {
                                    comboItem.Style = Application.Current.Resources["UcBaseCheckComboBoxItemStyle"] as Style;

                                    comboItem.Loaded -= ComboItem_Loaded;
                                    comboItem.Loaded += ComboItem_Loaded;
                                }
                                else
                                {
                                    comboItem.Style = Application.Current.Resources["C1ComboBoxItemStyle"] as Style;
                                }

                                if (checkStateList == null) checkStateList = new Dictionary<int, bool>();
                                int itemIndex = base.Items.IndexOf(comboItem);
                                if (isCheckAll)
                                {
                                    if (!checkStateList.ContainsKey(itemIndex)) checkStateList.Add(itemIndex, true);
                                }
                                else
                                {
                                    if (!checkStateList.ContainsKey(itemIndex)) checkStateList.Add(itemIndex, false);
                                }
                            }
                        }
                    }

                    isCheckEvent = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ComboItem_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsCheckBoxUse) return;

                if (e.Source is C1ComboBoxItem)
                {
                    C1ComboBoxItem cbi = e.Source as C1ComboBoxItem;
                    int itemIndex = base.Items.IndexOf(cbi);
                    CheckBox comboCheckBox = cbi.FindChild<CheckBox>("ContentCheckBox");
                    if (comboCheckBox != null)
                    {
                        if (isCheckAll)
                        {
                            comboCheckBox.IsChecked = true;
                        }
                        else if (isUnCheckAll)
                        {
                            comboCheckBox.IsChecked = false;
                        }
                        else
                        {
                            if (checkStateList != null && checkStateList.ContainsKey(itemIndex))
                            {
                                comboCheckBox.IsChecked = checkStateList[itemIndex];
                            }
                        }

                        if (checkBoxList == null) checkBoxList = new Dictionary<int, CheckBox>();
                        if (!checkBoxList.ContainsKey(itemIndex)) checkBoxList.Add(itemIndex, comboCheckBox);

                        comboCheckBox.Tag = itemIndex;

                        comboCheckBox.Checked -= ComboCheckBox_Checked;
                        comboCheckBox.Checked += ComboCheckBox_Checked;

                        comboCheckBox.Unchecked -= ComboCheckBox_Unchecked;
                        comboCheckBox.Unchecked += ComboCheckBox_Unchecked;

                        cbi.Loaded -= ComboItem_Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ComboCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isCheckEvent) return;

                CheckBox checkBox = e.Source as CheckBox;

                int selectIndex = -1;
                if (checkBox.Tag != null) selectIndex = checkBox.Tag.NvcInt();

                if (selectIndex < 0) return;

                isCheckEvent = false;

                if (isAllUsed)
                {
                    if (selectIndex.Equals(0))
                    {
                        isCheckAll = true;
                        isUnCheckAll = false;

                        if (checkStateList.ContainsKey(0)) checkStateList[0] = true;

                        for (int index = 1; index < base.Items.Count; index++)
                        {
                            if (checkBoxList.ContainsKey(index)) checkBoxList[index].IsChecked = true;
                            if (checkStateList.ContainsKey(index)) checkStateList[index] = true;
                        }
                    }
                    else
                    {
                        if (checkBoxList.ContainsKey(selectIndex)) checkBoxList[selectIndex].IsChecked = true;
                        if (checkStateList.ContainsKey(selectIndex)) checkStateList[selectIndex] = true;

                        isUnCheckAll = false;
                        isCheckAll = true;
                        for (int index = 1; index < base.Items.Count; index++)
                        {
                            if (checkStateList.ContainsKey(index) && checkStateList[index] == false)
                            {
                                isCheckAll = false;
                                break;
                            }
                        }

                        if (isCheckAll)
                        {
                            if (checkBoxList.ContainsKey(0)) checkBoxList[0].IsChecked = true;
                            if (checkStateList.ContainsKey(0)) checkStateList[0] = true;
                        }
                    }
                }
                else
                {
                    isUnCheckAll = false;

                    if (checkBoxList.ContainsKey(selectIndex)) checkBoxList[selectIndex].IsChecked = true;
                    if (checkStateList.ContainsKey(selectIndex)) checkStateList[selectIndex] = true;
                }

                SetCheckBoxText();

                DataRow changeDataRow = null;
                if (checkBox.TemplatedParent is C1ComboBoxItem)
                {
                    C1ComboBoxItem cbi = checkBox.TemplatedParent as C1ComboBoxItem;
                    if (cbi != null && cbi.DataContext is DataRow) changeDataRow = cbi.DataContext as DataRow;
                }

                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(selectIndex, changeDataRow, false, true, checkStateList));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            isCheckEvent = true;
        }

        private void ComboCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!isCheckEvent) return;

                CheckBox checkBox = e.Source as CheckBox;

                int selectIndex = -1;
                if (checkBox.Tag != null) selectIndex = checkBox.Tag.NvcInt();

                if (selectIndex < 0) return;

                isCheckEvent = false;

                if (isAllUsed && selectIndex.Equals(0))
                {
                    isCheckAll = false;
                    isUnCheckAll = true;

                    if (checkStateList.ContainsKey(0)) checkStateList[0] = false;

                    for (int index = 1; index < base.Items.Count; index++)
                    {
                        if (checkBoxList.ContainsKey(index)) checkBoxList[index].IsChecked = false;
                        if (checkStateList.ContainsKey(index)) checkStateList[index] = false;
                    }
                }
                else
                {
                    isCheckAll = false;

                    if (checkBoxList.ContainsKey(selectIndex)) checkBoxList[selectIndex].IsChecked = false;
                    if (checkStateList.ContainsKey(selectIndex)) checkStateList[selectIndex] = false;

                    if (isAllUsed)
                    {
                        if (checkBoxList.ContainsKey(0)) checkBoxList[0].IsChecked = false;
                        if (checkStateList.ContainsKey(0)) checkStateList[0] = false;
                    }
                }

                SetCheckBoxText();

                DataRow changeDataRow = null;
                if (checkBox.TemplatedParent is C1ComboBoxItem)
                {
                    C1ComboBoxItem cbi = checkBox.TemplatedParent as C1ComboBoxItem;
                    if (cbi != null && cbi.DataContext is DataRow) changeDataRow = cbi.DataContext as DataRow;
                }

                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(selectIndex, changeDataRow, true, false, checkStateList));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            isCheckEvent = true;
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            if (!IsCheckBoxUse && !(AutoComplete || IsEditable || !IsEnabled))
            {
                if (e.Text.Equals("\r") || e.Text.Equals("\n")) return;

                searchValue += e.Text;
                saveValue = string.Empty;

                if (searchValue.Length > 50)
                {
                    searchValue = string.Empty;
                    searchTimer.Stop();
                }

                if (!searchValue.Equals(string.Empty) && !searchToolTip.IsOpen) searchToolTip.IsOpen = true;

                searchToolTip.Content = searchValue.ToUpper();
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (!isCheckBoxUse && !(AutoComplete || IsEditable || !IsEnabled))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        FindItem();
                        break;
                    case Key.V:
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            searchValue = Clipboard.GetText();
                            FindItem();
                        }
                        break;
                    case Key.Delete:
                    case Key.Back:
                        searchTimer.Stop();
                        SelectedIndex = 0;
                        EndSearch();
                        break;
                    case Key.Up:
                    case Key.Down:
                        break;
                    case Key.Left:
                        PreviewFind();
                        break;
                    case Key.Right:
                        NextFind();
                        break;
                    case Key.Space:
                        searchValue += " ";
                        saveValue = string.Empty;
                        break;
                    default:
                        if (searchTimer != null)
                        {
                            searchTimer.Stop();
                            valuePath = SelectedValuePath;
                            displayPath = DisplayMemberPath;
                            searchTimer.Start();
                        }
                        break;
                }
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            try
            {
                if (isUserConfigUse && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (contextMenu == null)
                    {
                        contextMenu = new ContextMenu();

                        mnuUserConfig = new MenuItem();
                        mnuUserConfig.Name = "mnuUserConfig";
                        mnuUserConfig.Header = ObjectDic.Instance.GetObjectName("LAST_STAT_SAVE");
                        System.Windows.Controls.Image imgUserConfig = new System.Windows.Controls.Image();
                        imgUserConfig.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/ico-delete.png", UriKind.Relative));
                        imgUserConfig.Stretch = System.Windows.Media.Stretch.Fill;
                        mnuUserConfig.Icon = imgUserConfig;
                        mnuUserConfig.IsChecked = isExistsUserConfig;
                        mnuUserConfig.Click += MnuUserConfig_Click;
                        contextMenu.Items.Add(mnuUserConfig);
                    }

                    if (ContextMenu != null && !ContextMenu.Equals(contextMenu)) saveContextMenu = ContextMenu;

                    ContextMenu = contextMenu;
                }
                else
                {
                    ContextMenu = saveContextMenu;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MnuUserConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mnuUserConfig.IsChecked = !mnuUserConfig.IsChecked;

                if (mnuUserConfig.IsChecked)
                {
                    SaveUserConfig();
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

        private void OpenTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                openTimerCount++;
                if (openTimerCount > 50) openTimer.Stop();

                if (previewComboBox != null && previewComboBox.IsUserConfigValue.Equals(false)) return;

                if (base.ItemsSource != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            DataTable dtCombo = this.ToDataTable();

                            string codeColumn = "CBO_CODE";
                            if (!string.IsNullOrEmpty(this.SelectedValuePath))
                            {
                                codeColumn = this.SelectedValuePath;
                            }
                            else
                            {
                                if (dtCombo.Columns.Contains("CBO_CODE"))
                                {
                                    codeColumn = "CBO_CODE";
                                }
                                else
                                {
                                    codeColumn = string.Empty;
                                }
                            }

                            if (!codeColumn.IsNvc() && dtCombo.AsEnumerable().Where(w => w.Field<string>(codeColumn).Equals(userConfigValue)).Count() > 0)
                            {
                                this.SelectedValue = userConfigValue;
                                isUserConfigValue = true;
                            }
                        }
                        catch
                        {
                            // 오류시 스킵                    
                        }
                    }));


                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            int tryInt = 0;
                            if (int.TryParse(userConfigValue, out tryInt))
                            {
                                this.SelectedIndex = tryInt;
                                isUserConfigValue = true;
                            }
                        }
                        catch
                        {
                            // 오류시 스킵                    
                        }
                    }));
                }

                openTimer.Stop();
            }
            catch
            {
                openTimer.Stop();
            }
        }

        private void SearchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FindItem();
        }

        #endregion

        #region Public Method

        #region CheckBox 관련
        public void CheckAll()
        {
            try
            {
                if (checkStateList == null || checkStateList.Count == 0) return;

                isCheckEvent = false;

                for (int index = 0; index < base.Items.Count; index++)
                {
                    if (checkStateList.ContainsKey(index)) checkStateList[index] = true;
                    if (checkBoxList != null && checkBoxList.ContainsKey(index)) checkBoxList[index].IsChecked = true;
                }

                isCheckAll = true;
                isUnCheckAll = false;

                SetCheckBoxText();

                isCheckEvent = true;
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
                if (checkStateList == null || checkStateList.Count == 0) return;

                isCheckEvent = false;

                for (int index = 0; index < base.Items.Count; index++)
                {
                    if (checkStateList.ContainsKey(index)) checkStateList[index] = false;
                    if (checkBoxList != null && checkBoxList.ContainsKey(index)) checkBoxList[index].IsChecked = false;
                }

                isCheckAll = false;
                isUnCheckAll = true;

                SetCheckBoxText();

                isCheckEvent = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Validation 관련
        public void ClearValidation()
        {
            try
            {
                if (validationToolTip == null)
                {
                    validationToolTip = new ToolTip();
                    validationToolTip.PlacementTarget = this;
                    validationToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                }

                validationTimer?.Stop();

                if (validationVisibility.Equals(Visibility.Visible))
                {
                    if (validationToolTip.IsOpen) validationToolTip.IsOpen = false;

                    this.ToolTip = null;

                    ValidationVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        public void SetValidation(string messageID, params object[] parameters)
        {
            SetValidation(MessageDic.Instance.GetMessage(messageID, parameters), true);
        }

        public void SetValidation(string message, bool showToolTip = true)
        {
            try
            {
                if (validationToolTip == null)
                {
                    validationToolTip = new ToolTip();
                    validationToolTip.PlacementTarget = this;
                    validationToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Center;
                }

                if (validationTimer == null)
                {
                    validationTimer = new System.Timers.Timer(5000);
                    validationTimer.Elapsed += validationTimer_Elapsed;
                    validationTimer.AutoReset = true;
                }
                validationTimer.Stop();

                ValidationVisibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(message))
                {
                    string convertMessage = MessageDic.Instance.GetMessage(message).Replace("[#]", "").Trim();

                    validationToolTip.Content = convertMessage;

                    this.ToolTip = validationToolTip;

                    if (showToolTip && !validationToolTip.IsOpen)
                    {
                        validationToolTip.IsOpen = true;
                        validationToolTip.HorizontalOffset = (this.ActualWidth - validationToolTip.ActualWidth) / 2 * -1 + 23;
                        validationToolTip.VerticalOffset = (this.ActualHeight / 2) + (validationToolTip.ActualHeight / 2) + 1;
                        validationTimer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void validationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                validationTimer?.Stop();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (validationToolTip.IsOpen) validationToolTip.IsOpen = false;
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

        public DataTable ToDataTable(bool isCopy = true)
        {
            if (base.ItemsSource == null) return null;

            DataTable dtCombo = null;

            if (isCopy)
            {
                dtCombo = ((DataView)base.ItemsSource).ToTable();
            }
            else
            {
                DataView dvGrid = base.ItemsSource as DataView;
                dtCombo = dvGrid.Table;
            }

            return dtCombo;
        }

        #endregion

        #region Method

        private void FindItem()
        {
            try
            {
                searchTimer.Stop();
                if (!searchValue.Trim().Equals(string.Empty))
                {
                    for (int inx = 0; inx < Items.Count; inx++)
                    {
                        System.Data.DataRowView drFind = (System.Data.DataRowView)Items[inx];
                        string compareValuePath = string.IsNullOrEmpty(valuePath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[valuePath].ToString().ToUpper();
                        string compareDisplayPath = string.IsNullOrEmpty(displayPath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[displayPath].ToString().ToUpper();
                        if (compareDisplayPath.Equals("ALL") || compareDisplayPath.Equals("SELECT")) continue;
                        if (compareValuePath.Contains(searchValue.ToUpper()) || compareDisplayPath.Contains(searchValue.ToUpper()))
                        {
                            saveValue = searchValue;
                            FindItemData(drFind);
                            break;
                        }
                    }
                    EndSearch();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void EndSearch()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                searchValue = string.Empty;

                if (searchToolTip.IsOpen) searchToolTip.IsOpen = false;
            }));
        }

        private void FindItemData(System.Data.DataRowView drFind)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedItem = drFind;
            }));
        }

        private void PreviewFind()
        {
            try
            {
                if (saveValue.Equals(string.Empty)) return;
                if (SelectedIndex - 1 < 0) return;

                for (int inx = SelectedIndex - 1; inx > 0; inx--)
                {
                    if (inx < 0) continue;

                    DataRowView drFind = (DataRowView)Items[inx];
                    string compareValuePath = string.IsNullOrEmpty(valuePath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[valuePath].ToString().ToUpper();
                    string compareDisplayPath = string.IsNullOrEmpty(displayPath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[displayPath].ToString().ToUpper();
                    if (compareDisplayPath.Equals("ALL") || compareDisplayPath.Equals("SELECT")) continue;
                    if (compareValuePath.Contains(saveValue.ToUpper()) || compareDisplayPath.Contains(saveValue.ToUpper()))
                    {
                        FindItemData(drFind);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void NextFind()
        {
            try
            {
                if (saveValue.Equals(string.Empty)) return;
                if (SelectedIndex + 1 >= Items.Count) return;

                for (int inx = SelectedIndex + 1; inx < Items.Count; inx++)
                {
                    if (inx > Items.Count - 1) continue;

                    DataRowView drFind = (DataRowView)Items[inx];
                    string compareValuePath = string.IsNullOrEmpty(valuePath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[valuePath].ToString().ToUpper();
                    string compareDisplayPath = string.IsNullOrEmpty(displayPath) ? drFind.Row[0].ToString().ToUpper() : drFind.Row[displayPath].ToString().ToUpper();
                    if (compareDisplayPath.Equals("ALL") || compareDisplayPath.Equals("SELECT")) continue;
                    if (compareValuePath.Contains(saveValue.ToUpper()) || compareDisplayPath.Contains(saveValue.ToUpper()))
                    {
                        FindItemData(drFind);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Common.MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private void GetUserConfig()
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
                dr["WRK_TYPE"] = "SELECT";
                dr["USERID"] = LoginInfo.USERID;
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_COMBOBOX";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = LoginInfo.CFG_MENUID;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    string previewComboName = string.Empty;
                    foreach (DataRow drCombo in dtResult.AsEnumerable().OrderBy(o => o.Field<string>("USER_CONF02")).ToList())
                    {
                        if (drCombo["CONF_KEY3"].Equals(this.Name) && !drCombo["USER_CONF01"].IsNvc())
                        {
                            isExistsUserConfig = true;

                            string value = drCombo["USER_CONF01"].Nvc();
                            if (isCheckBoxUse && value.Equals("USE_CHECKBOX"))
                            {
                                if (drCombo["USER_CONF03"].Equals("CHECK_ALL"))
                                {
                                    this.CheckAll();
                                }
                                else if (drCombo["USER_CONF03"].Equals("UNCHECK_ALL"))
                                {
                                    this.UnCheckAll();
                                }
                                else
                                {
                                    string values = string.Empty;
                                    for (int index = 3; index <= 9; index++)
                                    {
                                        string columnName = "USER_CONF0" + index.Nvc();
                                        if (!drCombo[columnName].IsNvc()) values += drCombo[columnName].Nvc();
                                    }

                                    Dictionary<int, bool> saveCheckStateList = new Dictionary<int, bool>();
                                    List<string> selectIndexList = values.Split(',').ToList();
                                    foreach (KeyValuePair<int, bool> item in checkStateList)
                                    {
                                        if (selectIndexList.Contains(item.Key.Nvc()))
                                        {
                                            if (saveCheckStateList.ContainsKey(item.Key))
                                            {
                                                checkStateList[item.Key] = true;
                                            }
                                            else
                                            {
                                                saveCheckStateList.Add(item.Key, true);
                                            }
                                        }
                                        else
                                        {
                                            if (saveCheckStateList.ContainsKey(item.Key))
                                            {
                                                checkStateList[item.Key] = false;
                                            }
                                            else
                                            {
                                                saveCheckStateList.Add(item.Key, false);
                                            }
                                        }
                                    }
                                    checkStateList = saveCheckStateList;
                                    isCheckAll = isUnCheckAll = false;

                                    SetCheckBoxText();
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    userConfigValue = value;

                                    object topParentControl = this.FindPageControl();
                                    if (topParentControl != null && !string.IsNullOrEmpty(previewComboName))
                                    {
                                        previewComboBox = ((DependencyObject)topParentControl).FindChild<UcBaseComboBox>(previewComboName);
                                    }

                                    Dispatcher.BeginInvoke(new Action(() => openTimer.Start()));
                                }
                            }
                        }
                        previewComboName = drCombo["CONF_KEY3"].Nvc();
                    }
                }
            }
            catch (Exception ex)
            {
                // 스킵
            }

        }

        private void SaveUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;
                if (!this.IsCheckBoxUse && this.SelectedValue.IsNvc()) return;

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

                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = "SAVE";
                drNew["USERID"] = LoginInfo.USERID;
                drNew["CONF_TYPE"] = "USER_CONFIG_BASE_COMBOBOX";
                drNew["CONF_KEY1"] = topParentName;
                drNew["CONF_KEY2"] = LoginInfo.CFG_MENUID;
                drNew["CONF_KEY3"] = this.Name;
                if (isCheckBoxUse)
                {
                    drNew["USER_CONF01"] = "USE_CHECKBOX";
                    drNew["USER_CONF02"] = orderDateTime.Ticks.ToString("00000000000000000000");
                    if (isCheckAll)
                    {
                        drNew["USER_CONF03"] = "CHECK_ALL";
                    }
                    else if (isUnCheckAll)
                    {
                        drNew["USER_CONF03"] = "UNCHECK_ALL";
                    }
                    else
                    {
                        int saveLength = 3990;
                        if (this.SelectedValuesToString.Length < saveLength * 7)
                        {
                            string selectIndexString = string.Join(",", this.SelectedCheckedIndex);
                            int lessLen = selectIndexString.Length;
                            for (int valueLength = 0; valueLength < selectIndexString.Length; valueLength += saveLength)
                            {
                                int index = valueLength / saveLength;
                                if (index + 3 > 9)
                                {
                                    drNew["USER_CONF03"] = "CHECK_ALL";
                                    break;
                                }

                                string columnName = "USER_CONF0" + (index + 3).Nvc();
                                drNew[columnName] = selectIndexString.Substring(index * saveLength, lessLen < saveLength ? lessLen : saveLength);
                                lessLen -= saveLength;
                            }
                        }
                        else
                        {
                            drNew["USER_CONF03"] = "CHECK_ALL";
                        }
                    }
                }
                else
                {
                    drNew["USER_CONF01"] = this.SelectedValue is C1ComboBoxItem ? this.SelectedIndex.Nvc() : this.SelectedValue.Nvc();
                    drNew["USER_CONF02"] = orderDateTime.Ticks.ToString("00000000000000000000");
                }
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);

                isExistsUserConfig = true;
            }
            catch (Exception ex)
            {
                // 스킵
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
                dr["CONF_TYPE"] = "USER_CONFIG_BASE_COMBOBOX";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = LoginInfo.CFG_MENUID;
                dr["CONF_KEY3"] = this.Name;
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst, noLogInputData: true, nologOutputData: true);
                if (dtResult != null && dtResult.Rows.Count == 1 && Util.NVC(dtResult.Rows[0]["USERID"]).Equals("OK"))
                {
                    isExistsUserConfig = false;
                }
            }
            catch (Exception ex)
            {
                // 스킵
            }
        }

        private void SetCheckBoxStyle()
        {
            try
            {
                if (base.Items.Count == 0) return;

                if (isCheckBoxUse)
                {
                    if (base.Items[0] is DataRowView)
                    {
                        List<C1ComboBoxItem> checkComboItems = new List<C1ComboBoxItem>();
                        foreach (DataRowView item in base.Items)
                        {
                            C1ComboBoxItem comboItem = new C1ComboBoxItem();
                            comboItem.Content = item.Row["CBO_NAME"];
                            comboItem.DataContext = item.Row;
                            checkComboItems.Add(comboItem);
                        }

                        this.Clear();
                        foreach (C1ComboBoxItem item in checkComboItems)
                        {
                            base.Items.Add(item);
                        }
                    }

                    if (checkStateList == null) checkStateList = new Dictionary<int, bool>();
                    checkStateList.Clear();
                    for (int index = 0; index < base.Items.Count; index++)
                    {
                        if (isCheckAll)
                        {
                            checkStateList.Add(index, true);
                        }
                        else
                        {
                            checkStateList.Add(index, false);
                        }
                    }
                    SetCheckedTooltipText();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCheckBoxText()
        {
            try
            {
                if (isCheckBoxUse)
                {
                    if (checkStateList == null || checkStateList.Count == 0) return;

                    if (IsCheckAll || IsUnCheckAll)
                    {
                        if (this.Items[0] is C1ComboBoxItem)
                        {
                            C1ComboBoxItem comboItem = this.Items[0] as C1ComboBoxItem;
                            this.Text = comboItem.Content.Nvc();
                        }
                    }
                    else
                    {
                        if (this.SelectedCheckedText != null && this.SelectedCheckedText.Count > 0)
                        {
                            this.Text = string.Join(",", SelectedCheckedText);
                        }
                        else
                        {
                            this.Text = ObjectDic.Instance.GetObjectName("선택");
                        }
                    }
                    SetCheckedTooltipText();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCheckedTooltipText()
        {
            try
            {
                if (!isCheckBoxUse) return;

                string toolTipString = ObjectDic.Instance.GetObjectName("선택목록");
                if (isCheckAll)
                {
                    toolTipString += " - " + ObjectDic.Instance.GetObjectName("ALL_CHK");
                }
                else if (isUnCheckAll)
                {
                    toolTipString += " - " + ObjectDic.Instance.GetObjectName("없음");
                }
                else
                {
                    if (this.SelectedCheckedText != null && this.SelectedCheckedText.Count > 0)
                    {
                        int index = 0;
                        foreach (string text in SelectedCheckedText)
                        {
                            if (index++ >= 20)
                            {
                                toolTipString += "\r\n   .....";
                                break;
                            }
                            toolTipString += "\r\n   " + text;
                        }
                    }
                    else
                    {
                        toolTipString += " - " + ObjectDic.Instance.GetObjectName("없음");
                    }
                }

                this.ToolTip = toolTipString;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
