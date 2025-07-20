using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MultiSelectionBox : Control
    {
        public delegate void DropDownClosedHandler(object sender);
             
        public event DropDownClosedHandler DropDownClosed;

        public event EventHandler SelectionChanged;

        public static readonly DependencyProperty MultiSelectionBoxSourceProperty = DependencyProperty.Register("MultiSelectionBoxSource", typeof(IEnumerable), typeof(MultiSelectionBox), new PropertyMetadata(null));
        public IEnumerable MultiSelectionBoxSource
        {
            get
            {
                return this.GetValue(MultiSelectionBoxSourceProperty) as IEnumerable;
            }
            set
            {
                this.SetValue(MultiSelectionBoxSourceProperty, value);
            }
        }

        public static DependencyProperty isAllUsedProperty = DependencyProperty.Register("isAllUsed", typeof(bool), typeof(MultiSelectionBox), new PropertyMetadata(true, null));

        public bool isAllUsed
        {
            get
            {
                return (bool)this.GetValue(isAllUsedProperty);
            }
            set
            {
                this.SetValue(isAllUsedProperty, value);
            }
        }

        private ComboBox cbo;
        private ListBox lbTooltip; 
        private TextBlock nonSelectionString;
        private ContentPresenter selectedContent;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiSelectionBox), new PropertyMetadata(null, ItemsSourcePropertyChangedCallback));
        public IEnumerable ItemsSource
        {
            get
            {
                return this.GetValue(ItemsSourceProperty) as IEnumerable;
            }
            set
            {
                this.SetValue(ItemsSourceProperty, value);
            }
        }
        private static void ItemsSourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectionBox multiSelectionBox = d as MultiSelectionBox;
            if (e.NewValue == null)
            {
                multiSelectionBox.MultiSelectionBoxSource = new List<MultiSelectionBoxItem>();
            }
            else
            {
                IEnumerable source = e.NewValue as IEnumerable;
                List<MultiSelectionBoxItem> multiSelectionBoxSource = new List<MultiSelectionBoxItem>((from object s in source
                                                                                                        select new MultiSelectionBoxItem() { Item = s }).Cast<MultiSelectionBoxItem>());

                if (multiSelectionBox.isAllUsed == true)
                {
                    multiSelectionBoxSource.Insert(0, new MultiSelectionBoxItem() { IsChecked = false, IsAll = true, Item = "All" });
                }

                    foreach (MultiSelectionBoxItem msbitem in multiSelectionBoxSource)
                {
                    msbitem.SetBinding(MultiSelectionBoxItem.WidthProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("ActualWidth"), Source = multiSelectionBox });
                    msbitem.PropertyChanged += multiSelectionBox.MultiSelectionBoxItem_PropertyChanged;

                    if (!msbitem.IsAll)
                        msbitem.SetBinding(MultiSelectionBoxItem.ItemTemplateProperty, new System.Windows.Data.Binding() { Path = new PropertyPath("DataTemplate"), Source = multiSelectionBox });
                }
                multiSelectionBox.MultiSelectionBoxSource = multiSelectionBoxSource;
            }

            if (multiSelectionBox.SelectionChanged != null)
                multiSelectionBox.SelectionChanged(multiSelectionBox, null);
        }
        private bool isAllProcessing = false;
        private bool isIndivisualProcessing = false;
        private bool isCurrpted = false;
        public void MultiSelectionBoxItem_PropertyChanged(object sender, PropertyChangedEventArgs arg)
        {
            MultiSelectionBoxItem multiSelectionBoxItem = sender as MultiSelectionBoxItem;

            if (multiSelectionBoxItem.IsAll)
            {
                if (!isIndivisualProcessing)
                {
                    isAllProcessing = true;
                    foreach (MultiSelectionBoxItem item in this.MultiSelectionBoxSource)
                    {
                        if (!item.IsAll)
                            item.IsChecked = multiSelectionBoxItem.IsChecked;
                    }
                    isAllProcessing = false;

                    if (!cbo.IsDropDownOpen)
                    {
                        if (SelectionChanged != null)
                            SelectionChanged(this, null);
                    }
                }
            }
            else
            {
                if (!isAllProcessing)
                {
                    isIndivisualProcessing = true;
                    if (!multiSelectionBoxItem.IsChecked)
                    {
                        this.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().First().IsChecked = false;
                    }
                    else
                    {
                        if (SelectedItems.Count == this.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().Count() -1)
                        {
                            this.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().First().IsChecked = true;
                        }
                    }
                    isIndivisualProcessing = false;

                    if (!cbo.IsDropDownOpen)
                    {
                        if (SelectionChanged != null)
                            SelectionChanged(this, null);
                    }
                }
            }

            if (cbo.IsDropDownOpen)
                isCurrpted = true;
        }

        public IList<object> SelectedItems
        {
            get
            {
                if (MultiSelectionBoxSource == null)
                    return new List<object>();
                else
                    return (from MultiSelectionBoxItem i in MultiSelectionBoxSource where i.IsChecked && !i.IsAll select evaluateItem(i)).ToList<object>();
            }
        }

        public string SelectedItemsToString
        {
            get
            {
                if (MultiSelectionBoxSource == null)
                    return string.Empty;
                else
                    return string.Join(",", SelectedItems.ToArray());
            }
        }

        private object evaluateItem(MultiSelectionBoxItem i)
        {
            if (i.Item is DataRowView)
            {
                return (SelectedValuePath != null && (i.Item as DataRowView).Row.Table.Columns.Contains(SelectedValuePath)) ? (i.Item as DataRowView)[SelectedValuePath] : i.Item;
            }
            else
            {
                return (SelectedValuePath != null && i.Item.GetType().GetProperty(SelectedValuePath) != null) ? i.Item.GetType().GetProperty(SelectedValuePath).GetValue(i.Item, null) : i.Item;
            }
        }

        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(MultiSelectionBox), new PropertyMetadata(null));
        public string SelectedValuePath
        {
            get
            {
                return this.GetValue(SelectedValuePathProperty) as string;
            }
            set
            {
                this.SetValue(SelectedValuePathProperty, value);
            }
        }

        private bool isExternalDataTemplate = false;
        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register("DisplayMemberPath",
                                                                                                          typeof(string),
                                                                                                          typeof(MultiSelectionBox),
                                                                                                          new PropertyMetadata(
                                                                                                              null,
                                                                                                              (d, arg) =>
                                                                                                              {
                                                                                                                  MultiSelectionBox multiSelectionBox = d as MultiSelectionBox;
                                                                                                                  if (!multiSelectionBox.isExternalDataTemplate)
                                                                                                                  {
                                                                                                                      string dataTemplateXaml = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                                                                                                                            <Grid>
                                                                                                                                                                <Rectangle Fill=""#01010101"" Margin=""0,-3""/>
                                                                                                                                                                <TextBlock Text=""{Binding" + (arg.NewValue == null ? string.Empty : " " + arg.NewValue) + @"}"" />
                                                                                                                                                            </Grid>
                                                                                                                                                        </DataTemplate>";
                                                                                                                      multiSelectionBox.DataTemplate = (DataTemplate)XamlReader.Load(XmlReader.Create(new StringReader(dataTemplateXaml)));

                                                                                                                      multiSelectionBox.isExternalDataTemplate = false;
                                                                                                                  }
                                                                                                              }));
        public string DisplayMemberPath
        {
            get
            {
                return this.GetValue(DisplayMemberPathProperty) as string;
            }
            set
            {
                this.SetValue(DisplayMemberPathProperty, value);
            }
        }

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register("DataTemplate",
                                                                                                     typeof(DataTemplate),
                                                                                                     typeof(MultiSelectionBox),
                                                                                                     new PropertyMetadata(
                                                                                                         XamlReader.Load(XmlReader.Create(new StringReader(@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" >
                                                                                                                                <Grid>
                                                                                                                                    <Rectangle Fill=""#01010101"" Margin=""0,-3""/>
                                                                                                                                    <TextBlock Text=""{Binding}"" />
                                                                                                                                </Grid>
                                                                                                                            </DataTemplate>"))),
                                                                                                        (d, arg) =>
                                                                                                        {
                                                                                                            MultiSelectionBox multiSelectionBox = d as MultiSelectionBox;
                                                                                                            multiSelectionBox.isExternalDataTemplate = true;
                                                                                                        }));
        public DataTemplate DataTemplate
        {
            get
            {
                return this.GetValue(DataTemplateProperty) as DataTemplate;
            }
            set
            {
                this.SetValue(DataTemplateProperty, value);
            }
        }

        static MultiSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectionBox), new FrameworkPropertyMetadata(typeof(MultiSelectionBox)));
        }

        public MultiSelectionBox()
        {
            this.SelectionChanged += MultiSelectionBox_SelectionChanged;
        }

        void MultiSelectionBox_SelectionChanged(object sender, EventArgs e)
        {
            lbTooltip.ItemsSource = (from MultiSelectionBoxItem i in MultiSelectionBoxSource where !i.IsAll && i.IsChecked select i.Item);
            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                nonSelectionString.Opacity = 1;
                nonSelectionString.Margin = new Thickness(15, 0, 0, 0);
                selectedContent.Opacity = 0;
                selectedContent.Content = null;
            }
            else
            {
                nonSelectionString.Opacity = 0;
                nonSelectionString.Margin = new Thickness(15, 0, 0, 0);
                selectedContent.Opacity = 1;
                IEnumerable<object> selected = (from MultiSelectionBoxItem i in MultiSelectionBoxSource where i.IsChecked select i);
                MultiSelectionBoxItem first = selected.First() as MultiSelectionBoxItem;
                selectedContent.Content = first.Item;
                selectedContent.Margin = new Thickness(15, 0, 18, 0);
                if (first.IsAll)
                {
                    selectedContent.SetBinding(ContentPresenter.ContentTemplateProperty, "");
                }
                else
                {
                    selectedContent.SetBinding(ContentPresenter.ContentTemplateProperty, new System.Windows.Data.Binding("DataTemplate") { Source = this });
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            cbo = base.GetTemplateChild("cbo") as ComboBox;
            lbTooltip = base.GetTemplateChild("lbTooltip") as ListBox;
            nonSelectionString = GetTemplateChild("nonSelectionString") as TextBlock;
            selectedContent = GetTemplateChild("selectedContent") as ContentPresenter;

            cbo.SelectionChanged += cbo_SelectionChanged;
            cbo.DropDownClosed += cbo_DropDownClosed;
            cbo.DropDownOpened += cbo_DropDownOpened;
        }

        private void cbo_DropDownOpened(object sender, EventArgs e)
        {
            cbo.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Size maxSize = new Size(0, 0);
                    foreach (object obj in cbo.Items)
                    {
                        if (obj is MultiSelectionBoxItem)
                        {
                            MultiSelectionBoxItem item = obj as MultiSelectionBoxItem;
                            Size itemSize = item.MeasureActualSize();
                            maxSize = new Size(maxSize.Width > itemSize.Width ? maxSize.Width : itemSize.Width,
                                               maxSize.Height > itemSize.Height ? maxSize.Height : itemSize.Height);
                        }
                    }

                    foreach (object obj in cbo.Items)
                    {
                        if (obj is MultiSelectionBoxItem)
                        {
                            MultiSelectionBoxItem item = obj as MultiSelectionBoxItem;
                            item.Width = maxSize.Width;
                            item.Height = maxSize.Height;
                        }
                    }
                }
            ));
        }

        private void cbo_DropDownClosed(object sender, EventArgs e)
        {
            if (isCurrpted)
            {
                if (SelectionChanged != null)
                    SelectionChanged(this, null);
            }
            
            if(DropDownClosed != null)
            {
                DropDownClosed(this);
            }

            isCurrpted = false;
        }

        private void cbo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbo.Items.Count > -1)
                cbo.SelectedIndex = -1;
        }

        public void Check(int index)
        {
            MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(index + 1).IsChecked = true;
        }

        public void Uncheck(int index)
        {
            MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(index + 1).IsChecked = false;
        }

        public void CheckAll()
        {
            if (isAllUsed == true)
            {
                MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked = true;
            }
        }

        public void UncheckAll()
        {
            isIndivisualProcessing = true;
            isAllProcessing = true;
            foreach (MultiSelectionBoxItem item in MultiSelectionBoxSource)
            {
                item.IsChecked = true;
            }
            isAllProcessing = false;
            isIndivisualProcessing = false;

            MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked = false;
        }

        public void Check(object value)
        {
            foreach (MultiSelectionBoxItem item in MultiSelectionBoxSource)
            {
                if (!item.IsAll)
                {
                    if (evaluateItem(item).Equals(value))
                    {
                        item.IsChecked = true;
                    }
                }
            }
        }

        public void Uncheck(object value)
        {
            foreach (MultiSelectionBoxItem item in MultiSelectionBoxSource)
            {
                if (!item.IsAll)
                {
                    if (evaluateItem(item).Equals(value))
                    {
                        item.IsChecked = false;
                    }
                }
            }
        }

		public void SetDropDownOpen(bool dropDownOpen)
		{
			cbo.IsDropDownOpen = dropDownOpen;
		}


    }
}
