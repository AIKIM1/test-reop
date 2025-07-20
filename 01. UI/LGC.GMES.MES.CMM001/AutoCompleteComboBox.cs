/*************************************************************************************
 Created Date : 2016.11.24
      Creator : Jeong Hyeon Sik
   Decription : Pack 반품 화면 (Cell 포장- Cell반품화면[BOX001_017] 수정함)
--------------------------------------------------------------------------------------
 [Change History]
  2017-01-25  srcadm01  Initial Created.
  2017-06-23  신광희    AutoComplete popup 구현
  2020-08-28  손우석    PopupFindDataColumn MMD소스버젼으로 변경
**************************************************************************************/
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Filters;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    public class AutoCompleteComboBoxItem
    {
        public AutoCompleteComboBoxItem()
        {
        }

        public object this[string name]
        {
            get
            {
                if (this.Tag != null) return this.Tag.GetValue(name);
                return null;
            }
        }

        public object Tag
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Text;
        }
    }

    public class CustomComboBoxItem
    {
        public CustomComboBoxItem()
        {
        }

        public object Tag
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Text;
        }
    }

    public class AutoCompleteComboBox : System.Windows.Controls.AutoCompleteBox, INotifyPropertyChanged
    {
        bool isUpdatingDPs = false;

        public event ItemsSourceChangedHandler ItemsSourceEvent;
        public delegate void ItemsSourceChangedHandler(object sender, EventArgs e);

        protected virtual void OnItemsSourceChanged(EventArgs e)
        {
            this.SetSelectemItemUsingSelectedValueDP();

            if (ItemsSourceEvent != null)
                ItemsSourceEvent(this, e);
        }

        public new System.Collections.IEnumerable ItemsSource
        {
            get { return base.ItemsSource; }
            set
            {
                base.ItemsSource = value;
                EventArgs e = new EventArgs();
                OnItemsSourceChanged(e);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (propertyName == "SelectedValue")
            {
                C1.WPF.DataGrid.DataGridCellPresenter presenter = this.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (presenter != null)
                {
                    presenter.Row.DataGrid.EndEdit();
                    presenter.Row.DataGrid.EndEditRow(true);
                    presenter.Row.Refresh();
                }
            }

            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region [SelectedItemBinding]

        public static readonly DependencyProperty SelectedItemBindingProperty =
            DependencyProperty.Register("SelectedItemBinding",
                                        typeof(object),
                                        typeof(AutoCompleteComboBox),
                                        new PropertyMetadata(new PropertyChangedCallback(OnSelectedItemBindingChanged))
                                        );

        public object SelectedItemBinding
        {
            get { return GetValue(SelectedItemBindingProperty); }
            set { SetValue(SelectedItemBindingProperty, value); }
        }


        private static void OnSelectedItemBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoCompleteComboBox)d).OnSelectedItemBindingChanged(e);
        }

        protected virtual void OnSelectedItemBindingChanged(DependencyPropertyChangedEventArgs e)
        {
            SetSelectemItemUsingSelectedItemBindingDP();
        }

        public void SetSelectemItemUsingSelectedItemBindingDP()
        {
            if (!this.isUpdatingDPs)
                SetValue(SelectedItemProperty, GetValue(SelectedItemBindingProperty));
        }
        #endregion

        #region [SelectedValue]



        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText",
                    typeof(object),
                    typeof(AutoCompleteComboBox),
                    new PropertyMetadata(new PropertyChangingEventHandler(OnSelectedTextChanging))
                    );

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue",
                    typeof(object),
                    typeof(AutoCompleteComboBox),
                    new PropertyMetadata(new PropertyChangedCallback(OnSelectedValueChanged))
        );

        public static void OnSelectedTextChanging(object sender, PropertyChangingEventArgs e)
        {
            //((AutoCompleteComboBox)sender).Text = e.e.NewValue.ToString();
            //((AutoCompleteComboBox)sender).OnTextChanged(new RoutedEventArgs());
        }



        public object SelectedText
        {
            get
            {

                if (this.Text.IsEmpty()) return string.Empty;

                return GetValue(SelectedTextProperty);
            }
            set { SetValue(SelectedTextProperty, value); }
        }


        public object SelectedValue
        {
            get
            {
                if (this.Text.IsEmpty()) return null;

                return GetValue(SelectedValueProperty);
            }
            set { SetValue(SelectedValueProperty, value); }
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoCompleteComboBox)d).OnSelectedValueChanged(e);
        }


        protected virtual void OnSelectedValueChanged(DependencyPropertyChangedEventArgs e)
        {
            SetSelectemItemUsingSelectedValueDP();
        }

        public void SetSelectemItemUsingSelectedValueDP()
        {
            if (!this.isUpdatingDPs)
            {
                if (this.ItemsSource != null)
                {

                    if (this.SelectedValue == null)
                    {
                        this.SelectedItem = null;
                    }


                    else if (this.SelectedItem == null)
                    {
                        object selectedValue = GetValue(SelectedValueProperty);
                        string propertyPath = this.SelectedValuePath;
                        if (selectedValue != null && !(string.IsNullOrEmpty(propertyPath)))
                        {

                            foreach (object item in this.ItemsSource)
                            {
                                PropertyInfo propertyInfo = item.GetType().GetProperty(propertyPath);
                                if (propertyInfo.GetValue(item, null).Equals(selectedValue))
                                    this.SelectedItem = item;
                            }
                        }
                    }

                }
            }
        }


        #endregion

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath",
                                        typeof(string),
                                        typeof(AutoCompleteComboBox),
                                        null
                                        );

        public string SelectedValuePath
        {
            get { return GetValue(SelectedValuePathProperty) as string; }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        public AutoCompleteComboBox()
            : base()
        {
            SetCustomFilter();
            this.DefaultStyleKey = typeof(AutoCompleteComboBox);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ToggleButton toggle = (ToggleButton)GetTemplateChild("DropDownToggle");
            if (toggle != null)
            {
                //toggle.Click -= DropDownToggle_Click;
                toggle.Click += DropDownToggle_Click;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            ListBox listBox = GetTemplateChild("Selector") as ListBox;
            if (listBox != null)
            {
                if (this.ItemsSource != null && this.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }));
                }
            }
        }

        private void DropDownToggle_Click(object sender, RoutedEventArgs e)
        {
            //this.Focus();
            FrameworkElement fe = sender as FrameworkElement;
            AutoCompleteBox acb = null;

            while (fe != null && acb == null)
            {
                fe = VisualTreeHelper.GetParent(fe) as FrameworkElement;
                acb = fe as AutoCompleteBox;
            }
            if (acb != null)
            {
                acb.IsDropDownOpen = !acb.IsDropDownOpen;
            }
        }

        protected virtual void SetCustomFilter()
        {
            this.ItemFilter = (prefix, item) =>
            {
                if (string.IsNullOrEmpty(prefix))
                    return true;

                if (this.SelectedItem != null)
                    if (this.SelectedItem.ToString() == prefix)
                        return true;

                return item.ToString().ToLower().Contains(prefix.ToLower());
            };
        }

        protected override void OnPopulated(PopulatedEventArgs e)
        {
            base.OnPopulated(e);
            ListBox listBox = GetTemplateChild("Selector") as ListBox;
            if (listBox != null)
            {
                if (this.ItemsSource != null && this.SelectedItem != null)
                {
                    listBox.SelectedItem = this.SelectedItem;
                    listBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        listBox.UpdateLayout();
                        listBox.ScrollIntoView(listBox.SelectedItem);
                        //listBox.UpdateLayout();
                    }));
                }
            }
        }

        protected override void OnDropDownClosed(RoutedPropertyChangedEventArgs<bool> e)
        {
            base.OnDropDownClosed(e);
            UpdateCustomDPs();
        }

        private void UpdateCustomDPs()
        {
            this.isUpdatingDPs = true;

            if (this.SelectedItem != null || this.Text == string.Empty)
            {
                SetValue(SelectedItemBindingProperty, GetValue(SelectedItemProperty));

                string propertyPath = this.SelectedValuePath;

                if (!string.IsNullOrEmpty(propertyPath))
                {
                    if (this.SelectedItem != null)
                    {
                        PropertyInfo propertyInfo = this.SelectedItem.GetType().GetProperty(propertyPath);

                        object propertyValue = propertyInfo.GetValue(this.SelectedItem, null);
                        this.SelectedText = this.SelectedItem.ToString();
                        this.SelectedValue = propertyValue;

                        RaisePropertyChanged("SelectedValue");
                    }
                    else
                    {
                        BindingExpression bindingExpression = this.GetBindingExpression(SelectedValueProperty);
                        if (bindingExpression != null)
                        {
                            Binding dataBinding = bindingExpression.ParentBinding;
                            object dataItem = bindingExpression.DataItem;
                            string propertyPathForSelectedValue = dataBinding.Path.Path;

                            Type propertyTypeForSelectedValue = dataItem.GetType().GetProperty(propertyPathForSelectedValue).PropertyType;
                            object defaultObj = null;
                            if (propertyTypeForSelectedValue.IsValueType)
                                defaultObj = Activator.CreateInstance(propertyTypeForSelectedValue);

                            this.SelectedValue = defaultObj;
                            RaisePropertyChanged("SelectedValue");
                        }
                    }
                }
            }
            else
            {
                if (this.GetBindingExpression(SelectedItemBindingProperty) != null)
                    SetSelectemItemUsingSelectedItemBindingDP();
                else if (this.GetBindingExpression(SelectedValueProperty) != null)
                    SetSelectemItemUsingSelectedValueDP();
            }
            this.isUpdatingDPs = false;
        }
    }

    public enum ContentDisplayMode { TextOnly, ValueOnly, TextAndValue, ValueAndText };

    public enum PopupDisplayMode { AllContents, ValueAndText, ValueOnly, TextOnly };

    public partial class PopupFindControl : ContentControl, INotifyPropertyChanged
    {
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
        }

        public delegate void ValueChangedEventHandler(object sender, EventArgs e);
        public event ValueChangedEventHandler ValueChanged;
        private bool dataColumnMode = false;

        public PopupFindControl()
        {
        }

        public PopupFindControl(bool columnMode)
        {
            this.dataColumnMode = columnMode;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty PopupDisplayModeProperty =
            DependencyProperty.Register("PopupDisplayMode", typeof(PopupDisplayMode), typeof(PopupFindControl), new UIPropertyMetadata(PopupDisplayMode.ValueAndText));
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnSelectedPathChanged)));
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnSelectedPathChanged)));
        public static readonly DependencyProperty AddMemberPathProperty =
            DependencyProperty.Register("AddMemberPath", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnSelectedPathChanged)));
        public static readonly DependencyProperty UseFlagPathProperty =
            DependencyProperty.Register("UseFlagPath", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnUseFlagPathChanged)));
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnSelectedValueChanged)));
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(object), typeof(PopupFindControl), new PropertyMetadata(new PropertyChangedCallback(OnSelectedTextChanged)));
        public static readonly DependencyProperty IsLikeModeProperty =
            DependencyProperty.Register("IsLikeMode", typeof(bool), typeof(PopupFindControl), new UIPropertyMetadata(false));

        private static void OnUseFlagPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public PopupDisplayMode PopupDisplayMode
        {
            get
            {
                return (PopupDisplayMode)GetValue(PopupDisplayModeProperty);
            }
            set { SetValue(PopupDisplayModeProperty, value); }
        }


        public object UseFlagPath
        {
            get
            {
                return GetValue(UseFlagPathProperty);
            }
            set { SetValue(UseFlagPathProperty, value); }
        }

        public object AddMemberPath
        {
            get
            {
                return GetValue(AddMemberPathProperty);
            }
            set { SetValue(AddMemberPathProperty, value); }
        }

        public object DisplayMemberPath
        {
            get
            {
                return GetValue(DisplayMemberPathProperty);
            }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public object SelectedValuePath
        {
            get
            {
                return GetValue(SelectedValuePathProperty);
            }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        private static void OnSelectedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupFindControl)d).UpdateSelectedText(e);
        }

        public void UpdateSelectedText(DependencyPropertyChangedEventArgs e)
        {
            SelectedText = e.NewValue;

            if (xText != null)
            {
                if (SelectedText != null)
                {
                    this.xText.Text = SelectedText.ToString();
                }
                else
                {
                    this.xText.Text = string.Empty;
                }
            }
            //((PopupFindControl)d).OnSelectedItemBindingChanged(e);
            //SetValue(SelectedItemProperty, GetValue(SelectedItemBindingProperty));
        }

        public void UpdateSelectedValue(DependencyPropertyChangedEventArgs e)
        {
            this.SelectedValue = e.NewValue;
        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupFindControl)d).UpdateSelectedValue(e);
        }

        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public object SelectedText
        {
            get { return GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        public bool IsLikeMode
        {
            get { return (bool)GetValue(IsLikeModeProperty); }
            set { SetValue(IsLikeModeProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private System.Collections.IEnumerable itemsSource = null;
        public event ItemsSourceChangedHandler ItemsSourceEvent;
        public delegate void ItemsSourceChangedHandler(object sender, EventArgs e);
        public object SelectedItem { get; set; }
        private Button xOK;
        private Button xClear;
        private Button xButton;
        private TextBox xText;
        private TextBox xSearch;
        private C1DataGrid xGrid;
        private System.Windows.Controls.Primitives.Popup xPopup;
        private Image xClose;
        private Thumb xThumb;
        public bool IsTextReadOnly
        {
            get
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return true;

                if (this.xText == null)
                    this.xText = GetTemplateChild("xText") as TextBox;

                return this.xText.IsReadOnly;
            }
            set
            {
                if(this.xText != null)
                    this.xText.IsReadOnly = value;
            }
        }

        public bool IsTextEnabled
        {
            get
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return true;

                return !this.xText.IsEnabled;
            }
            set
            {
                this.xText.IsEnabled = !value;
            }
        }

        public bool IsLoading
        {
            get
            {
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                    return false;
                
                return xGrid.IsLoading;
            }
            set
            {
                if(xGrid != null)
                    xGrid.IsLoading = value;
            }
        }
        protected virtual void OnItemsSourceChanged(EventArgs e)
        {
            if (xGrid != null)
            {
                xGrid.ItemsSource = this.ItemsSource;

                #region 템플릿 적용하면 검색이 안됨

                //                if (this.SelectedValuePath != null)
                //                {
                //                    C1.WPF.DataGrid.DataGridTemplateColumn column = xGrid.Columns[0] as C1.WPF.DataGrid.DataGridTemplateColumn;
                //                    string xaml = @"
                //                        <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">                                  
                //                                <TextBlock HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Text=""{Binding " + (this.SelectedValuePath.ToString()) + @"}"" />                         
                //                        </DataTemplate>
                //                    ";

                //                    column.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(xaml);
                //                }

                //                if (this.SelectedTextPath != null)
                //                {
                //                    C1.WPF.DataGrid.DataGridTemplateColumn column = xGrid.Columns[1] as C1.WPF.DataGrid.DataGridTemplateColumn;

                //                    string xaml = @"
                //                        <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">                                 
                //                                <TextBlock HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Text=""{Binding " + (this.SelectedTextPath.ToString()) + @"}"" />                            
                //                        </DataTemplate>
                //                    ";

                //                    column.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(xaml);
                //                }
                #endregion

                if (this.SelectedValuePath != null)
                {
                    C1.WPF.DataGrid.DataGridTextColumn column = xGrid.Columns[0] as C1.WPF.DataGrid.DataGridTextColumn;
                    column.Binding = new Binding(this.SelectedValuePath.ToString());
                }

                if (this.DisplayMemberPath != null)
                {
                    C1.WPF.DataGrid.DataGridTextColumn column = xGrid.Columns[1] as C1.WPF.DataGrid.DataGridTextColumn;
                    column.Binding = new Binding(this.DisplayMemberPath.ToString());
                }

                if (this.AddMemberPath != null)
                {
                    C1.WPF.DataGrid.DataGridTextColumn column = xGrid.Columns[2] as C1.WPF.DataGrid.DataGridTextColumn;
                    column.Binding = new Binding(this.AddMemberPath.ToString());
                }

                if (ItemsSourceEvent != null)
                {
                    ItemsSourceEvent(this, e);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        public System.Collections.IEnumerable ItemsSource
        {
            get { return itemsSource; }
            set
            {
                itemsSource = value;
                EventArgs e = new EventArgs();
                OnItemsSourceChanged(e);
            }
        }



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            xThumb = GetTemplateChild("xThumb") as Thumb;
            if (xThumb != null)
            {
                xThumb.DragStarted += xThumb_DragStarted;
                xThumb.DragDelta += xThumb_DragDelta;
                xThumb.DragCompleted += xThumb_DragCompleted;
            }

            this.xClose = GetTemplateChild("xClose") as Image;
            if (xClose != null)
            {
                xClose.MouseLeftButtonUp += xClose_MouseLeftButtonUp;
            }

            this.xOK = GetTemplateChild("xOK") as Button;
            if (xOK != null)
            {
                xOK.Click += xOK_Click;
            }

            this.xClear = GetTemplateChild("xClear") as Button;
            if (xClear != null)
            {
                xClear.Click += xClear_Click;
            }

            this.xButton = GetTemplateChild("xButton") as Button;
            if (xButton != null)
            {
                xButton.Click += xButton_Click;
                if (this.IsLikeMode)
                {
                    xButton.Content = "%";
                }
            }

            this.xText = GetTemplateChild("xText") as TextBox;
            if (xText != null)
            {
                xText.LostFocus += xText_LostFocus;
                xText.GotFocus += xText_GotFocus;
                xText.KeyDown += xText_KeyDown;
                xText.TextChanged += xText_TextChanged;
            }

            this.xSearch = GetTemplateChild("xSearch") as TextBox;
            if (xSearch != null)
            {
                //System.Windows.Input.InputMethod.SetIsInputMethodEnabled(xSearch, true);
                xSearch.KeyDown += xSearch_KeyDown;
                xSearch.GotFocus += xSearch_GotFocus;
                xSearch.TextChanged += xSearch_TextChanged;
            }

            this.xGrid = GetTemplateChild("xGrid") as C1DataGrid;
            if (this.xGrid != null)
            {
                if (this.PopupDisplayMode != PopupDisplayMode.AllContents)
                {
                    if (this.PopupDisplayMode == PopupDisplayMode.ValueAndText)
                    {
                        this.xGrid.Columns[2].Visibility = Visibility.Collapsed;
                    }
                    else if (this.PopupDisplayMode == PopupDisplayMode.ValueOnly)
                    {
                        this.xGrid.Columns[1].Visibility = Visibility.Collapsed;
                        this.xGrid.Columns[2].Visibility = Visibility.Collapsed;
                    }
                    else if (this.PopupDisplayMode == PopupDisplayMode.TextOnly)
                    {
                        this.xGrid.Columns[0].Visibility = Visibility.Collapsed;
                        this.xGrid.Columns[2].Visibility = Visibility.Collapsed;
                    }
                }

                xGrid.Height = xGrid.MinHeight;
                xGrid.KeyDown += xGrid_KeyDown;
                xGrid.MouseLeftButtonUp += xGrid_MouseLeftButtonUp;
                xGrid.LoadedCellPresenter += xGrid_LoadedCellPresenter;
            }

            this.xPopup = GetTemplateChild("xPopup") as System.Windows.Controls.Primitives.Popup;
            if (this.xPopup != null)
            {
                xPopup.PreviewKeyDown += delegate (object sender, KeyEventArgs args)
                {
                };
                xPopup.LayoutUpdated += delegate (object sender, EventArgs args)
                {
                };

                xPopup.Opened += delegate (object sender, EventArgs args)
                {
                    C1.WPF.DataGrid.DataGridCellPresenter dgcr = this.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    if (dgcr != null) dgcr.DataGrid.PreviewMouseWheel += bubble_PreviewMouseWheel;
                };

                xPopup.Closed += delegate (object sender, EventArgs args) {
                    C1.WPF.DataGrid.DataGridCellPresenter dgcr = this.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    if (dgcr != null) dgcr.DataGrid.PreviewMouseWheel -= bubble_PreviewMouseWheel;
                };
            }
        }

        private void xThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb t = (Thumb)sender;
            t.Cursor = null;
        }

        private void xThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            this.xGrid.Height = Double.NaN;
            double yadjust = xGrid.ActualHeight + e.VerticalChange + xSearch.ActualHeight + xOK.ActualHeight + 25;
            double xadjust = xGrid.ActualWidth + e.HorizontalChange + 10;

            if (xadjust < this.xGrid.MinWidth)
            {
                xPopup.Width = xGrid.MinWidth;
            }
            else
            {
                if ((xadjust >= 0))
                {
                    xPopup.Width = xadjust;
                }
            }

            if (yadjust < this.xGrid.MinHeight)
            {
                xPopup.Height = xGrid.MinHeight;
            }
            else
            {
                if ((yadjust >= 0))
                {
                    xPopup.Height = yadjust;
                }
            }
            //if ((xadjust >= 0) && (yadjust >= 0))
            //{
            //    xPopup.Width = xadjust;
            //    xPopup.Height = yadjust;
            //}
        }

        private void xThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb t = (Thumb)sender;
            t.Cursor = Cursors.Hand;
        }

        void xGrid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (this.UseFlagPath != null)
            {
                bool b = false;
                object flag = e.Cell.Row.DataItem.GetValue(this.UseFlagPath.ToString());

                if (flag == null || flag.ToString() == "1" || flag.ToString() == "Y" || flag.ToString().ToUpper() == "TRUE" || flag.ToString().ToUpper() == "True")
                {
                    b = true;
                }

                if (b == false)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                    //e.Cell.Presenter.ValidationState = C1.WPF.ValidationState.InvalidUnfocused;

                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
                    //e.Cell.Presenter.ValidationState = C1.WPF.ValidationState.Valid;
                }
            }
        }

        void xText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.IsLikeMode == false)
            {
                IInputElement focusedControl = Keyboard.FocusedElement;
                if ((focusedControl is C1DataGrid && ((C1DataGrid)focusedControl).Name == "xGrid") || (focusedControl is Button && ((Button)focusedControl).Name == "xButton"))
                {
                }
                else
                {
                    if (this.SelectedText != null)
                    {
                        if (this.SelectedText.SafeToString() != xText.Text)
                            this.xText.Text = this.SelectedText.SafeToString();
                    }
                }
            }
        }

        void xSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox box = sender as TextBox;
                if (box != null && box.Text.SafeToString() == "")
                {
                    C1FullTextSearchBehavior.GetFullTextSearchBehavior(xGrid).Filter = string.Empty;
                    this.xGrid.Selection.Clear();
                }
                else
                {
                    C1FullTextSearchBehavior.GetFullTextSearchBehavior(xGrid).Filter = box.Text;
                }
            }
            catch
            {

            }
        }

        void xOK_Click(object sender, RoutedEventArgs e)
        {
            SelectAction();
        }

        void xClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.xPopupCloseAndFocus();
        }

        void xPopupCloseAndFocus()
        {
            xPopup.IsOpen = false;
            xText.Focus();
        }
        void xPopupClose()
        {
            xPopup.IsOpen = false;
        }

        void xGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectAction();
        }

        void xClear_Click(object sender, RoutedEventArgs e)
        {
            if (this.xGrid.IsLoading == false)
            {
                this.xSearch.Text = string.Empty;
                this.xGrid.Selection.Clear();
                SelectAction();
            }
        }

        void SelectAction()
        {
            try
            {
                if (this.IsLikeMode == true)
                    xText.TextChanged -= xText_TextChanged;

                string value = string.Empty;
                string text = string.Empty;
                string item = string.Empty;

                if (xGrid.SelectedItem != null)
                {
                    value = xGrid.SelectedItem.GetValue(this.SelectedValuePath.ToString()).ToString();
                    text = xGrid.SelectedItem.GetValue(this.DisplayMemberPath.ToString()).ToString();
                    //item = xGrid.SelectedItem.GetValue(this.AddMemberPath.ToString()).ToString();
                    this.SelectedItem = xGrid.SelectedItem;
                }

                this.SelectedValue = value;
                this.SelectedText = text;
                this.SelectedItem = item;
                this.xText.Text = text;
                //xPopup.IsOpen = false;
                this.xPopupClose();

                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);

                C1.WPF.DataGrid.DataGridCellPresenter presenter = this.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (presenter != null)
                {
                    presenter.Row.DataGrid.EndEdit();
                    //presenter.Row.DataGrid.EndEditRow(true);
                    //presenter.Row.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (this.IsLikeMode == true) xText.TextChanged += xText_TextChanged;
            }
        }

        void xSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            this.xSearch.SelectAll();
        }

        void xGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                this.xSearch.SelectAll();
                this.xSearch.Focus();
            }
            else if (e.Key == Key.Escape)
            {

                xPopupCloseAndFocus();
            }
            else if (e.Key == Key.Enter)
            {
                SelectAction();
            }
        }

        private void xText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLikeMode == true)
            {
                this.SelectedText = xText.Text;
                this.SelectedValue = null;
            }
        }

        void xText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                xButton_Click(this.xButton, null);
                e.Handled = true;
            }
        }

        void xSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                xGrid = GetTemplateChild("xGrid") as C1DataGrid;
                if (xGrid.Rows.Count > 0)
                {
                    xGrid.SelectedIndex = 0;
                    xGrid.Focus();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                this.xPopupCloseAndFocus();
            }
        }

        [DllImport("User32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        IntPtr GetHwnd(System.Windows.Controls.Primitives.Popup popup)
        {
            System.Windows.Interop.HwndSource source = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(popup.Child);
            return source.Handle;
        }
        void xButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.xPopup != null)
            {
                if (xGrid != null && xGrid.ItemsSource == null) OnItemsSourceChanged(null);

                string text = this.xText.Text;

                xPopup.IsOpen = true;
                xPopup.UpdateLayout();

                IntPtr handle = GetHwnd(this.xPopup);
                SetFocus(handle);

                xPopup.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        xSearch.Text = text;

                        if (e == null)
                        {
                            xGrid.Focus();
                            if (xGrid.Rows.Count > 0)
                                this.xGrid.SelectedIndex = 0;
                        }
                        else
                            xSearch.Focus();
                    })
                );
            }

            C1.WPF.DataGrid.DataGridCellPresenter dgcr = this.Parent as C1.WPF.DataGrid.DataGridCellPresenter;

            UIElement parent = null;
            if (dgcr != null)
            {
                parent = dgcr.DataGrid;
            }
            else
            {
                parent = Application.Current.MainWindow as UIElement;
            }

            GeneralTransform gt = this.TransformToVisual(parent);
            Point offset = gt.Transform(new Point(0, 0));

            double rootWidth = parent.RenderSize.Width;
            double popupWidth = offset.X + xGrid.Width;

            if (rootWidth < popupWidth)
            {
                xPopup.HorizontalOffset = (popupWidth - rootWidth) * -1;
            }
            else
            {
                xPopup.HorizontalOffset = 0;
            }

            double rootHeight = parent.RenderSize.Height;
            double popupHeight = offset.Y + xGrid.Height + 70;

            if (rootHeight < popupHeight)
            {
                xPopup.VerticalOffset = (popupHeight - rootHeight) * -1;
            }
            else
            {
                xPopup.VerticalOffset = 0;
            }
            //Console.WriteLine(xPopup.HorizontalOffset.ToString() + "," + xPopup.VerticalOffset.ToString());
        }

        private void bubble_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            if (xPopup != null && xPopup.IsOpen == true)
            {
                e.Handled = true;

                //버블링 때문에 안됨
                //var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                //eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                //eventArg.Source = sender;
                //this.xGrid.RaiseEvent(eventArg); 
                //var parent = ((Control)sender).Parent as UIElement;
                //parent.RaiseEvent(eventArg); 

                this.xGrid.Viewport.ScrollToVerticalOffset(this.xGrid.Viewport.VerticalOffset - e.Delta);
            }


        }

        void xText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (xPopup != null)
            {
                xPopupClose();
                ((TextBox)sender).SelectAll();
            }
        }
    }

    public class PopupFindDataColumn : C1.WPF.DataGrid.DataGridBoundColumn
    {
        //2020-08-28
        public override object GetCellValue(C1.WPF.DataGrid.DataGridRow dgr)
        {
            if (!this.GetCellValueMode.SafeToString().Equals("SAVE"))
            {
                DataView source = ((DataView)this.ItemsSource);
                string value = ((DataRowView)dgr.DataItem)[this.Name].SafeToString();

                DataRow[] rows = source.Table.Select(this.SelectedValuePath.SafeToString() + " = '" + value + "'", string.Empty);

                if (rows.Length != 0)
                {
                    if (this.GetCellValueMode.SafeToString().Equals("EXCEL"))
                    {
                        //자동화면구성 대응
                        return String.Format("{0} : {1}", rows[0][this.SelectedValuePath.SafeToString()].SafeToString(), rows[0][this.DisplayMemberPath.SafeToString()].SafeToString());
                    }
                    else
                    {
                        return rows[0][this.DisplayMemberPath.SafeToString()].SafeToString();
                    }
                }
            }

            return base.GetCellValue(dgr);
        }

        private System.Collections.IEnumerable itemsSource = null;
        public System.Collections.IEnumerable ItemsSource
        {
            get { return itemsSource; }
            set
            {
                itemsSource = value;

                if (this.SelectedValuePath != null && this.DisplayMemberPath != null && this.ItemsSource != null)
                {
                    if (this.DataGrid.Viewport != null)
                    {
                        int firstRow = this.DataGrid.Viewport.FirstVisibleRow - 5;
                        int lastRow = this.DataGrid.Viewport.LastVisibleRow + 5;

                        if (firstRow < 0) firstRow = 0;
                        if (lastRow > this.DataGrid.Rows.Count) lastRow = this.DataGrid.Rows.Count;

                        for (int i = firstRow; i < lastRow; i++)
                        {
                            if (this.DataGrid.Rows[i].DataItem != null && this.DataGrid.Rows[i].Presenter != null)
                            {
                                DataGridCellPresenter dgcp = this.DataGrid.Rows[i].Presenter[this];

                                StackPanel panel = dgcp.Content as StackPanel;
                                if (panel != null)
                                {
                                    TextBlock displayBox = null;
                                    TextBlock valueBox = null;
                                    TextBlock delimeterBox = null;

                                    for (int j = 0; j < VisualTreeHelper.GetChildrenCount(panel); j++)
                                    {
                                        TextBlock child = VisualTreeHelper.GetChild(panel, j) as TextBlock;
                                        if (child != null)
                                        {
                                            if (child.Name == "DISPLAY_BOX") displayBox = child;
                                            if (child.Name == "VALUE_BOX") valueBox = child;
                                            if (child.Name == "DELIMITER_BOX") delimeterBox = child;
                                        }
                                    }
                                    this.InitCellContent(displayBox, valueBox, delimeterBox);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override bool IsEditable
        {
            get
            {
                return true;
            }
        }

        public static readonly DependencyProperty PopupDisplayModeProperty
            = DependencyProperty.Register("PopupDisplayMode", typeof(PopupDisplayMode), typeof(PopupFindDataColumn), new UIPropertyMetadata(PopupDisplayMode.ValueAndText));
        public static readonly DependencyProperty ContentDisplayModeProperty
            = DependencyProperty.Register("ContentDisplayMode", typeof(ContentDisplayMode), typeof(PopupFindDataColumn), new UIPropertyMetadata(ContentDisplayMode.TextOnly));


        public PopupDisplayMode PopupDisplayMode
        {
            get
            {
                return (PopupDisplayMode)GetValue(PopupDisplayModeProperty);
            }
            set { SetValue(PopupDisplayModeProperty, value); }
        }

        public ContentDisplayMode ContentDisplayMode
        {
            get
            {
                return (ContentDisplayMode)GetValue(ContentDisplayModeProperty);
            }
            set { SetValue(ContentDisplayModeProperty, value); }
        }

        //public static readonly DependencyProperty BindingValueProperty =
        //DependencyProperty.Register("BindingValue",
        //        typeof(object),
        //        typeof(PopupFindDataColumn),
        //        new PropertyMetadata(new PropertyChangedCallback(OnSelectedValueChanged))
        //        );

        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register("ValueMemberPath",
                typeof(object),
                typeof(PopupFindDataColumn),
                new PropertyMetadata(new PropertyChangedCallback(OnValueMemberPathChanged))
                );

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath",
                typeof(object),
                typeof(PopupFindDataColumn),
                new PropertyMetadata(new PropertyChangedCallback(OnDisplayMemberPathChanged))
                );

        public static readonly DependencyProperty AddMemberPathProperty =
            DependencyProperty.Register("AddMemberPath",
                typeof(object),
                typeof(PopupFindDataColumn),
                new PropertyMetadata(new PropertyChangedCallback(OnAddMemberPathChanged))
                );

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath",
                typeof(object),
                typeof(PopupFindDataColumn),
                new PropertyMetadata(new PropertyChangedCallback(OnSelectedValuePathChanged))
                );

        public static readonly DependencyProperty UseFlagMemberPathProperty =                  
            DependencyProperty.Register("UseFlagMemberPath",                         
                typeof(object),                         
                typeof(PopupFindDataColumn),                        
                new PropertyMetadata(new PropertyChangedCallback(OnUseFlagMemberPathChanged))                       
                );

        public static readonly DependencyProperty TriggerFilterProperty =
            DependencyProperty.Register("TriggerFilter",
                typeof(object),
                typeof(PopupFindDataColumn),               
                new PropertyMetadata(new PropertyChangedCallback(OnTriggerFilterChanged))                    
                );

        //2020-08-28
        public static readonly DependencyProperty MWindowSizeProperty =
            DependencyProperty.Register("MainWindowSize",
                typeof(object),
                typeof(PopupFindDataColumn),
                new PropertyMetadata(new PropertyChangedCallback(OnMainWindowSizeChanged))
                );

        //2020-08-28
        public static readonly DependencyProperty GetCellValueModeProperty =
           DependencyProperty.Register("GetCellValueMode",
               typeof(object),
               typeof(PopupFindDataColumn),
               new PropertyMetadata(new PropertyChangedCallback(OnGetCellValueModeChanged))
               );

        //public object BindingValue
        //{
        //    get { return GetValue(BindingValueProperty); }
        //    set { SetValue(BindingValueProperty, value); }
        //}

        public object ValueMemberPath
        {
            get { return GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }
        public object SelectedValuePath
        {
            get { return GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }
        public object DisplayMemberPath
        {
            get { return GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public object AddMemberPath
        {
            get { return GetValue(AddMemberPathProperty); }
            set { SetValue(AddMemberPathProperty, value); }
        }
        public object UseFlagMemberPath
        {
            get { return GetValue(UseFlagMemberPathProperty); }
            set { SetValue(UseFlagMemberPathProperty, value); }
        }
        public object TriggerFilter
        {
            get { return GetValue(TriggerFilterProperty); }
            set { SetValue(TriggerFilterProperty, value); }
        }

        //2020-08-28
        public object MainWindowSize
        {
            get { return GetValue(MWindowSizeProperty); }
            set { SetValue(MWindowSizeProperty, value); }
        }

        //2020-08-28
        public object GetCellValueMode
        {
            get { return GetValue(GetCellValueModeProperty); }
            set { SetValue(GetCellValueModeProperty, value); }
        }

        private static void OnTriggerFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnUseFlagMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnSelectedValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnValueMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnAddMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        //2020-08-28
        private static void OnMainWindowSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        //2020-08-28
        private static void OnGetCellValueModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public PopupFindDataColumn()
        {
            Initialize();
        }

        public PopupFindDataColumn(PropertyInfo property)
            : base(property)
        {
            Initialize();
        }

        protected void Initialize()
        {
            IsReadOnly = false;
        }

        public override object GetCellContentRecyclingKey(C1.WPF.DataGrid.DataGridRow dgr)
        {
            return typeof(PopupFindDataColumn);
        }

        public override FrameworkElement CreateCellContent(C1.WPF.DataGrid.DataGridRow dgr)
        {
            //2020-08-28
            //StackPanel panel = new StackPanel();
            //panel.Orientation = Orientation.Horizontal;
            Grid panel = new Grid();
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            TextBlock displayBox = new TextBlock()
            {
                Name = "DISPLAY_BOX",
                Visibility = Visibility.Visible,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };
            //2020-08-28
            TextBlock delimeterBox = new TextBlock() { Name = "DELIMITER_BOX", Text = " : ", VerticalAlignment = VerticalAlignment.Center, Visibility = Visibility.Collapsed };

            TextBlock valueBox = new TextBlock()
            {
                Name = "VALUE_BOX",
                Visibility = Visibility.Collapsed,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };

            if (this.ContentDisplayMode == ContentDisplayMode.TextOnly)
            {
                //2020-08-28
                delimeterBox.Visibility = Visibility.Collapsed;

                panel.Children.Add(displayBox);
                //2020-08-28
                panel.Children.Add(delimeterBox);
                panel.Children.Add(valueBox);

                //2020-08-28
                Grid.SetColumn(displayBox, 0);
                Grid.SetColumn(delimeterBox, 1);
                Grid.SetColumn(valueBox, 2);
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.ValueOnly)
            {
                displayBox.Visibility = Visibility.Collapsed;
                valueBox.Visibility = Visibility.Visible;
                //2020-08-28
                delimeterBox.Visibility = Visibility.Collapsed;

                panel.Children.Add(displayBox);
                //2020-08-28
                panel.Children.Add(delimeterBox);
                panel.Children.Add(valueBox);

                //2020-08-28
                Grid.SetColumn(displayBox, 0);
                Grid.SetColumn(delimeterBox, 1);
                Grid.SetColumn(valueBox, 2);
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.TextAndValue)
            {
                valueBox.Visibility = Visibility.Visible;
                //2020-08-28
                delimeterBox.Visibility = Visibility.Visible;
                panel.Children.Add(displayBox);
                //2020-08-28
                //panel.Children.Add(new TextBlock() { Name = "DELIMITER_BOX", Text = " : ", VerticalAlignment = VerticalAlignment.Center });
                panel.Children.Add(delimeterBox);
                panel.Children.Add(valueBox);

                //2020-08-28
                Grid.SetColumn(displayBox, 0);
                Grid.SetColumn(delimeterBox, 1);
                Grid.SetColumn(valueBox, 2);
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.ValueAndText)
            {
                valueBox.Visibility = Visibility.Visible;
                //2020-08-28
                delimeterBox.Visibility = Visibility.Visible;

                panel.Children.Add(valueBox);
                //2020-08-28
                //panel.Children.Add(new TextBlock() { Name = "DELIMITER_BOX", Text = " : ", VerticalAlignment = VerticalAlignment.Center });
                panel.Children.Add(delimeterBox);
                panel.Children.Add(displayBox);

                //2020-08-28
                Grid.SetColumn(displayBox, 2);
                Grid.SetColumn(delimeterBox, 1);
                Grid.SetColumn(valueBox, 0);
            }
            return panel;
        }

        public override void BindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow dgr)
        {
            //System.Data.DataView source = this.itemsSource as System.Data.DataView;
            //if(source != null)
            //{
            //    // source.RowFilter = string.Empty;
            //    this.ItemsSource = source;
            //}

            //2020-08-28
            //StackPanel panel = cellContent as StackPanel;
            Grid panel = cellContent as Grid;
            TextBlock displayBox = null;
            TextBlock valueBox = null;
            TextBlock delimeterBox = null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(panel); i++)
            {
                TextBlock child = VisualTreeHelper.GetChild(panel, i) as TextBlock;
                if (child != null)
                {
                    if (child.Name == "DISPLAY_BOX") displayBox = child;
                    if (child.Name == "VALUE_BOX") valueBox = child;
                    if (child.Name == "DELIMITER_BOX") delimeterBox = child;
                }
            }

            if (panel != null)
            {
                if (this.ContentDisplayMode == ContentDisplayMode.TextOnly)
                {
                    //2020-08-28
                    //displayBox.Visibility = Visibility.Visible;
                    //valueBox.Visibility = Visibility.Collapsed;
                    if (displayBox != null) displayBox.Visibility = Visibility.Visible;
                    if (delimeterBox != null) delimeterBox.Visibility = Visibility.Collapsed;
                    if (valueBox != null) valueBox.Visibility = Visibility.Collapsed;

                    if (displayBox != null) Grid.SetColumn(displayBox, 0);
                    if (delimeterBox != null) Grid.SetColumn(delimeterBox, 1);
                    if (valueBox != null) Grid.SetColumn(valueBox, 2);
                }
                else if (this.ContentDisplayMode == ContentDisplayMode.ValueOnly)
                {
                    //2020-08-28
                    //displayBox.Visibility = Visibility.Collapsed;
                    //valueBox.Visibility = Visibility.Visible;
                    if (displayBox != null) displayBox.Visibility = Visibility.Collapsed;
                    if (delimeterBox != null) delimeterBox.Visibility = Visibility.Collapsed;
                    if (valueBox != null) valueBox.Visibility = Visibility.Visible;

                    if (displayBox != null) Grid.SetColumn(displayBox, 0);
                    if (delimeterBox != null) Grid.SetColumn(delimeterBox, 1);
                    if (valueBox != null) Grid.SetColumn(valueBox, 2);
                }
                else if (this.ContentDisplayMode == ContentDisplayMode.TextAndValue)
                {
                    //2020-08-28
                    //valueBox.Visibility = Visibility.Visible;
                    //displayBox.Visibility = Visibility.Visible;
                    if (valueBox != null) valueBox.Visibility = Visibility.Visible;
                    if (delimeterBox != null) delimeterBox.Visibility = Visibility.Visible;
                    if (displayBox != null) displayBox.Visibility = Visibility.Visible;

                    if (displayBox != null) Grid.SetColumn(displayBox, 0);
                    if (delimeterBox != null) Grid.SetColumn(delimeterBox, 1);
                    if (valueBox != null) Grid.SetColumn(valueBox, 2);
                }
                else if (this.ContentDisplayMode == ContentDisplayMode.ValueAndText)
                {
                    //2020-08-28
                    //valueBox.Visibility = Visibility.Visible;
                    //displayBox.Visibility = Visibility.Visible;
                    if (valueBox != null) valueBox.Visibility = Visibility.Visible;
                    if (delimeterBox != null) delimeterBox.Visibility = Visibility.Visible;
                    if (displayBox != null) displayBox.Visibility = Visibility.Visible;

                    if (displayBox != null) Grid.SetColumn(displayBox, 2);
                    if (delimeterBox != null) Grid.SetColumn(delimeterBox, 1);
                    if (valueBox != null) Grid.SetColumn(valueBox, 0);
                }

                //displayBox = panel.Children[0] as TextBlock;
                //valueBox = panel.Children[1] as TextBlock;

                var binding = CopyBinding(Binding);

                displayBox.Text = string.Empty;
                //displayBox.DataContext = row.DataItem;
                //displayBox.SetBinding(TextBlock.TextProperty, binding);

                valueBox.DataContext = dgr.DataItem;
                binding.NotifyOnTargetUpdated = true;
                valueBox.SetBinding(TextBlock.TextProperty, binding);

                valueBox.TargetUpdated -= valueBox_TargetUpdated;
                valueBox.TargetUpdated += valueBox_TargetUpdated;

                if (this.SelectedValuePath != null && this.DisplayMemberPath != null && this.ItemsSource != null)
                {
                    this.InitCellContent(displayBox, valueBox, delimeterBox);
                }
            }
        }

        private void valueBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            TextBlock valueBox = e.Source as TextBlock;

            if (valueBox != null && valueBox.Text.SafeToString().Equals(string.Empty))
            {
                //2020-08-28
                //StackPanel panel = valueBox.Parent as StackPanel;
                Grid panel = valueBox.Parent as Grid;
                TextBlock displayBox = null;
                TextBlock delimeterBox = null;

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(panel); i++)
                {
                    TextBlock child = VisualTreeHelper.GetChild(panel, i) as TextBlock;
                    if (child != null)
                    {
                        if (child.Name == "DISPLAY_BOX") displayBox = child;
                        if (child.Name == "VALUE_BOX") valueBox = child;
                        if (child.Name == "DELIMITER_BOX") delimeterBox = child;
                    }
                }
                this.InitCellContent(displayBox, valueBox, delimeterBox);
            }
        }

        private void InitCellContent(TextBlock displayBox, TextBlock valueBox, TextBlock delimeterBox)
        {
            if (displayBox != null && this.Binding != null)
            {
                string initValue = string.Empty;
                string pathName = this.Binding.Path.Path;
                //bool comboBoxStyle = false;

                // 바인딩 컬럼이 팝업 데이터소스에 없는 경우 콤보박스 스타일
                //if (pathName != this.SelectedValuePath.ToString())
                //{
                //comboBoxStyle = true;
                //}

                if (valueBox.DataContext.GetValue(pathName) != null)
                {
                    initValue = valueBox.DataContext.GetValue(pathName).SafeToString();
                }

                if (initValue.Equals(string.Empty))
                {
                    displayBox.Text = string.Empty;
                }
                else
                {
                    IList list = (IList)this.ItemsSource;
                    foreach (DataRowView drv in list)
                    {
                        if (drv.DataView.Table.Columns.Contains(this.SelectedValuePath.ToString()))
                        {
                            if (drv[this.SelectedValuePath.ToString()] != null)
                            {
                                string itemValue = drv[this.SelectedValuePath.ToString()].ToString();

                                if (itemValue.Equals(initValue))
                                {
                                    object displayValue = drv[this.DisplayMemberPath.ToString()];

                                    displayBox.Text = (displayValue != null) ? displayValue.ToString() : string.Empty;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (delimeterBox != null)
                {
                    if (displayBox.Text == string.Empty && valueBox.Text == string.Empty)
                    {
                        delimeterBox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (this.ContentDisplayMode == ContentDisplayMode.TextAndValue ||
                                this.ContentDisplayMode == ContentDisplayMode.ValueAndText)
                        {
                            delimeterBox.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        public override void UnbindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow dgr)
        {
            //2020-08-28
            //var panel = (StackPanel)cellContent;
            var panel = (Grid)cellContent;

            TextBlock displayBox = null;
            TextBlock valueBox = null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(panel); i++)
            {
                TextBlock child = VisualTreeHelper.GetChild(panel, i) as TextBlock;
                if (child != null)
                {
                    if (child.Name == "DISPLAY_BOX") displayBox = child;
                    if (child.Name == "VALUE_BOX") valueBox = child;
                }
            }

            //var dbox = panel.Children[0] as TextBlock;
            //var vbox = panel.Children[1] as TextBlock;
            // box.InvalidateMeasure();
            displayBox.DataContext = null;
            valueBox.DataContext = null;
        }

        public delegate void PopupValueChangedEventHandler(string column, C1.WPF.DataGrid.DataGridRow dgr, PopupFindControl pfc);

        public event PopupValueChangedEventHandler PopupValueChanged;

        public override FrameworkElement GetCellEditingContent(C1.WPF.DataGrid.DataGridRow dgr)
        {
            DataRowView drv = dgr.DataItem as DataRowView;

            C1.WPF.DataGrid.DataGridCellPresenter presenter = dgr.Presenter[this.DataGrid.Columns[this.Name]];
            //2020-08-28
            //StackPanel panel = presenter.Content as StackPanel;
            Grid panel = presenter.Content as Grid;
            TextBlock displayBox = null; // panel.Children[0] as TextBlock;
            TextBlock valueBox = null; // panel.Children[1] as TextBlock;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(panel); i++)
            {
                TextBlock child = VisualTreeHelper.GetChild(panel, i) as TextBlock;
                if (child != null)
                {
                    if (child.Name == "DISPLAY_BOX") displayBox = child;
                    if (child.Name == "VALUE_BOX") valueBox = child;
                }
            }

            string filter = string.Empty;

            if (!this.TriggerFilter.IsNullOrEmpty())
            {
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"\{([^}]*[^/])\}");
                filter = this.TriggerFilter.ToString();

                System.Text.RegularExpressions.MatchCollection mc = reg.Matches(filter);
                for (int i = 0; i < mc.Count; i++)
                {
                    System.Text.RegularExpressions.Match m = mc[i];
                    string triggerName = m.Value.Replace("{", "").Replace("}", "");
                    string triggerValue = drv[triggerName].SafeToString();
                    filter = filter.Replace(m.Value, triggerValue);
                }

            }

            System.Data.DataView source = this.itemsSource as System.Data.DataView;
            System.Data.DataView copySource = null;
            if (source != null)
            {
                copySource = source.Table.Copy().AsDataView();
                if (filter.IsNotEmpty())
                {
                    copySource.RowFilter = filter;

                    // TriggerFilter 속성에 USE_FLAG = 'Y' 넣어 처리 가능하지만 
                    // 화면 자동구성 때문에 자동으로 사용여부를 필터링 처리 한다.
                    if (this.UseFlagMemberPath.SafeToString().IsNotEmpty() || copySource.Table.Columns.Contains("USE_FLAG"))
                    {
                        if (copySource.Table.Columns.Contains(this.UseFlagMemberPath.SafeToString()))
                            copySource.RowFilter += " AND " + this.UseFlagMemberPath.SafeToString() + " = 'Y'";
                    }
                    copySource = copySource.ToTable().AsDataView();
                }
                else
                {
                    if (this.UseFlagMemberPath.SafeToString().IsNotEmpty() || copySource.Table.Columns.Contains("USE_FLAG"))
                    {
                        if (this.UseFlagMemberPath.SafeToString().IsEmpty())
                        {
                            copySource.RowFilter = "USE_FLAG = 'Y'";
                        }
                        else
                        {
                            if (copySource.Table.Columns.Contains(this.UseFlagMemberPath.SafeToString()))
                                copySource.RowFilter = this.UseFlagMemberPath.SafeToString() + " = 'Y'";
                        }
                    }
                }
            }

            PopupFindControl control = new PopupFindControl(true);
            control.IsLikeMode = false;

            control.Margin = new Thickness(3);
            control.ItemsSource = copySource;

            control.DisplayMemberPath = this.DisplayMemberPath.ToString();
            control.SelectedValuePath = this.SelectedValuePath.ToString(); //.ValueMemberPath.ToString();
            //2020-08-28
            //control.AddMemberPath = this.AddMemberPath.ToString();

            if (UseFlagMemberPath != null)
            {
                control.UseFlagPath = this.UseFlagMemberPath.ToString();
            }

            control.PopupDisplayMode = this.PopupDisplayMode;
            if (this.PopupValueChanged != null)
            {
                control.ValueChanged += delegate (System.Object o, System.EventArgs e)
                {
                    string column = null;

                    if (this.Binding != null) column = this.Binding.Path.Path;
                    this.PopupValueChanged(column, dgr, (PopupFindControl)o);
                };
            }


            Binding binding = this.CopyBinding(this.Binding);
            //control.SetBinding(PopupFindControl.BindingTextProperty, binding);
            control.SetBinding(PopupFindControl.SelectedValueProperty, binding);

            if (this.ContentDisplayMode == ContentDisplayMode.TextOnly)
            {
                control.SelectedText = displayBox.Text;
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.ValueOnly)
            {
                control.SelectedText = valueBox.Text;
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.TextAndValue)
            {
                control.SelectedText = displayBox.Text;
            }
            else if (this.ContentDisplayMode == ContentDisplayMode.ValueAndText)
            {
                control.SelectedText = valueBox.Text;
            }

            if (SelectedValuePath != null)
            {
                //control.SetBinding(PopupFindControl.BindingValueProperty, new Binding() { Path = new PropertyPath(SelectedValuePath.ToString()), Mode = BindingMode.TwoWay });
                control.SetBinding(PopupFindControl.SelectedValueProperty, new Binding() { Path = new PropertyPath(binding.Path.Path), Mode = BindingMode.TwoWay });
            }

            //var binding = this.CopyBinding(this.Binding);
            //control.SetBinding(PopupFindControl.BindingTextProperty, binding);

            //if (SelectedValuePath != null)
            //{
            //    control.SetBinding(PopupFindControl.BindingValueProperty, new Binding() { Path = new PropertyPath(SelectedValuePath.ToString()), Mode = BindingMode.TwoWay });
            //}

            return control;
        }

        public override void EndEdit(FrameworkElement editingElement)
        {
            base.EndEdit(editingElement);

            var box = (PopupFindControl)editingElement;
            if (box != null)
            {
                //if (this.BindingValue != box.BindingValue)
                //{
                //    this.BindingValue = box.BindingValue;



                //    C1.WPF.DataGrid.DataGridCellPresenter presenter = (C1.WPF.DataGrid.DataGridCellPresenter)(box.Parent);
                //    if (presenter != null)
                //    {

                //        //presenter.Row.DataGrid.EndEdit();
                //        //presenter.Row.DataGrid.EndEditRow(true);
                //        //presenter.Row.Refresh();
                //    }

                //}
            }

        }
    }
}
