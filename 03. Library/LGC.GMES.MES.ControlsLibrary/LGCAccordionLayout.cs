using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using C1.WPF.Extended;

namespace LGC.GMES.MES.ControlsLibrary
{
    [ContentProperty("Content")]
    public class LGCAccordionLayout : ItemsControl
    {
        TextBlock txtTitle;
        private Button btnSearch;
        public event RoutedEventHandler SearchClick;

        public static readonly DependencyProperty SearchAreaProperty = DependencyProperty.Register("SearchArea", typeof(ObservableCollection<C1AccordionItem>), typeof(LGCAccordionLayout), new PropertyMetadata(null, SearchAreaPropertyChanged));
        public ObservableCollection<C1AccordionItem> SearchArea
        {
            get
            {
                return GetValue(SearchAreaProperty) as ObservableCollection<C1AccordionItem>;
            }
            set
            {
                SetValue(SearchAreaProperty, value);
            }
        }
        private static void SearchAreaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                (e.OldValue as ObservableCollection<C1AccordionItem>).CollectionChanged -= (d as LGCAccordionLayout).LGCAccordionLayout_CollectionChanged;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as ObservableCollection<C1AccordionItem>).CollectionChanged += (d as LGCAccordionLayout).LGCAccordionLayout_CollectionChanged;
                (d as LGCAccordionLayout).LGCAccordionLayout_CollectionChanged(null, null);
            }
        }
        private void LGCAccordionLayout_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MainSearchArea.Clear();
            DetailSearchArea.Clear();

            if (SearchArea.Count > 0)
            {
                SearchArea.First().IsExpanded = true;
                MainSearchArea.Add(SearchArea.First());

                for (int inx = 1; inx < SearchArea.Count; inx++)
                {
                    DetailSearchArea.Add(SearchArea[inx]);
                }
            }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(LGCAccordionLayout), new PropertyMetadata(null));
        public object Content
        {
            get
            {
                return GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }

        public static readonly DependencyProperty MainSearchAreaProperty = DependencyProperty.Register("MainSearchArea", typeof(ObservableCollection<C1AccordionItem>), typeof(LGCAccordionLayout), new PropertyMetadata(null));
        public ObservableCollection<C1AccordionItem> MainSearchArea
        {
            get
            {
                return GetValue(MainSearchAreaProperty) as ObservableCollection<C1AccordionItem>;
            }
            set
            {
                SetValue(MainSearchAreaProperty, value);
            }
        }

        public static readonly DependencyProperty DetailSearchAreaProperty = DependencyProperty.Register("DetailSearchArea", typeof(ObservableCollection<C1AccordionItem>), typeof(LGCAccordionLayout), new PropertyMetadata(null));
        public ObservableCollection<C1AccordionItem> DetailSearchArea
        {
            get
            {
                return GetValue(DetailSearchAreaProperty) as ObservableCollection<C1AccordionItem>;
            }
            set
            {
                SetValue(DetailSearchAreaProperty, value);
            }
        }

        static LGCAccordionLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LGCAccordionLayout), new FrameworkPropertyMetadata(typeof(LGCAccordionLayout)));
        }

        public LGCAccordionLayout()
        {
            MainSearchArea = new ObservableCollection<C1AccordionItem>();
            DetailSearchArea = new ObservableCollection<C1AccordionItem>();

            SearchArea = new ObservableCollection<C1AccordionItem>();
            SearchArea.Clear();
            SearchArea.CollectionChanged -= LGCAccordionLayout_CollectionChanged;
            SearchArea.CollectionChanged += LGCAccordionLayout_CollectionChanged;

            this.Loaded += LGCAccordionLayout_Loaded;
        }

        void LGCAccordionLayout_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= LGCAccordionLayout_Loaded;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            btnSearch = GetTemplateChild("btnSearch") as Button;

            btnSearch.Click += (sender, arg) =>
                {
                    if (SearchClick != null)
                        SearchClick(sender, arg);
                };

            txtTitle = GetTemplateChild("txtTitlePanel") as TextBlock;
            txtTitle.Text = string.Empty;
            if (this.Tag == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.Tag.ToString()))
            {
                txtTitle.Text = "Program Description :" + Environment.NewLine + Environment.NewLine + this.Tag.ToString();
            }

        }
    }
}
