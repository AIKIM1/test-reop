using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MainFrame
{
    public class MegadropMenuItemEventArg : EventArgs
    {
        private List<MegadropMenuItem> _menuPath = new List<MegadropMenuItem>();
        public List<MegadropMenuItem> MenuPath
        {
            get
            {
                return _menuPath;
            }
        }

        public MegadropMenuItemEventArg()
            : base()
        {
        }
    }

    public class MegadropMenuItem : ItemsControl
    {
        internal event EventHandler<MegadropMenuItemEventArg> MenuItemMouseLeftButtonDown;

        private Grid LayoutRoot;

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(MegadropMenuItem));
        public string Header
        {
            get
            {
                return (string)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderFontFamilyProperty = DependencyProperty.Register("HeaderFontFamily", typeof(FontFamily), typeof(MegadropMenuItem), new PropertyMetadata(null));
        public FontFamily HeaderFontFamily
        {
            get
            {
                return GetValue(HeaderFontFamilyProperty) as FontFamily;
            }
            set
            {
                SetValue(HeaderFontFamilyProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register("HeaderFontSize", typeof(double), typeof(MegadropMenuItem), new PropertyMetadata(12.0));
        public double HeaderFontSize
        {
            get
            {
                return (double)GetValue(HeaderFontSizeProperty);
            }
            set
            {
                SetValue(HeaderFontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderFontWeightProperty = DependencyProperty.Register("HeaderFontWeight", typeof(FontWeight), typeof(MegadropMenuItem), new PropertyMetadata(FontWeights.Normal));
        public FontWeight HeaderFontWeight
        {
            get
            {
                return (FontWeight)GetValue(HeaderFontWeightProperty);
            }
            set
            {
                SetValue(HeaderFontWeightProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(MegadropMenuItem));
        public Brush HeaderForeground
        {
            get
            {
                return GetValue(HeaderForegroundProperty) as Brush;
            }
            set
            {
                SetValue(HeaderForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderSelectedForegroundProperty = DependencyProperty.Register("HeaderSelectedForeground", typeof(Brush), typeof(MegadropMenuItem));
        public Brush HeaderSelectedForeground
        {
            get
            {
                return GetValue(HeaderSelectedForegroundProperty) as Brush;
            }
            set
            {
                SetValue(HeaderSelectedForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsPanelVisibilityProperty = DependencyProperty.Register("ItemsPanelVisibility", typeof(Visibility), typeof(MegadropMenuItem), new PropertyMetadata(Visibility.Visible));
        public Visibility ItemsPanelVisibility
        {
            get
            {
                return (Visibility)GetValue(ItemsPanelVisibilityProperty);
            }
            set
            {
                SetValue(ItemsPanelVisibilityProperty, value);
            }
        }

        //2 단 메뉴 Hide
        public static readonly DependencyProperty HeaderPanelVisibilityProperty = DependencyProperty.Register("HeaderPanelVisibility", typeof(Visibility), typeof(MegadropMenuItem), new PropertyMetadata(Visibility.Visible));

        public Visibility HeaderPanelVisibility
        {
            get
            {
                return (Visibility)GetValue(HeaderPanelVisibilityProperty);
            }
            set
            {
                SetValue(HeaderPanelVisibilityProperty, value);
            }
        }
        //2 단 메뉴 Hide

        static MegadropMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MegadropMenuItem), new FrameworkPropertyMetadata(typeof(MegadropMenuItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LayoutRoot = GetTemplateChild("LayoutRoot") as Grid;

            attachMenuItemClickHandler();

            this.MouseLeftButtonDown += MegadropMenuItem_MouseLeftButtonDown;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (LayoutRoot != null)
                VisualStateManager.GoToElementState(LayoutRoot, "Selected", false);
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (LayoutRoot != null)
                VisualStateManager.GoToElementState(LayoutRoot, "MouseOver", false);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (LayoutRoot != null)
                VisualStateManager.GoToElementState(LayoutRoot, "Normal", false);
        }

        internal void gotoUnselectState()
        {
            if (LayoutRoot != null)
                VisualStateManager.GoToElementState(LayoutRoot, "Unselected", false);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            attachMenuItemClickHandler();
        }

        private void attachMenuItemClickHandler()
        {
            foreach (object item in this.Items)
            {
                if (item is MegadropMenuItem)
                {
                    MegadropMenuItem menuItem = item as MegadropMenuItem;

                    menuItem.MenuItemMouseLeftButtonDown -= menuItem_MenuItemMouseLeftButtonDown;
                    menuItem.MenuItemMouseLeftButtonDown += menuItem_MenuItemMouseLeftButtonDown;
                }
            }
        }

        void menuItem_MenuItemMouseLeftButtonDown(object sender, MegadropMenuItemEventArg e)
        {
            if (MenuItemMouseLeftButtonDown != null)
            {
                e.MenuPath.Insert(0, this);
                MenuItemMouseLeftButtonDown(sender, e);
            }
        }

        void MegadropMenuItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (MenuItemMouseLeftButtonDown != null)
            {
                MegadropMenuItemEventArg arg = new MegadropMenuItemEventArg();
                arg.MenuPath.Add(this);
                MenuItemMouseLeftButtonDown(this, arg);
            }
        }
    }
}
