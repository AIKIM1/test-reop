using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MainFrame
{
    public class MegadropMenu : ItemsControl
    {
        public event EventHandler<MegadropMenuItemEventArg> MenuItemClick;

        public Button btnLeftNext;
        public Button btnRightNext;
        public Border bdmegaDropBox;

        public Popup megaDropBox;

        public static readonly DependencyProperty SelectedMenuItemProperty = DependencyProperty.Register("SelectedMenuItem", typeof(MegadropMenuItem), typeof(MegadropMenu), new PropertyMetadata(null));
        public MegadropMenuItem SelectedMenuItem
        {
            get
            {
                return GetValue(SelectedMenuItemProperty) as MegadropMenuItem;
            }
            set
            {
                SetValue(SelectedMenuItemProperty, value);
            }
        }

        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register("ContentPadding", typeof(Thickness), typeof(MegadropMenu), new PropertyMetadata(new Thickness(0)));
        public Thickness ContentPadding
        {
            get
            {
                return (Thickness)GetValue(ContentPaddingProperty);
            }
            set
            {
                SetValue(ContentPaddingProperty, value);
            }
        }

        public static readonly DependencyProperty DropPanelBorderThicknessProperty = DependencyProperty.Register("DropPanelBorderThickness", typeof(Thickness), typeof(MegadropMenu), new PropertyMetadata(new Thickness(0)));
        public Thickness DropPanelBorderThickness
        {
            get
            {
                return (Thickness)GetValue(DropPanelBorderThicknessProperty);
            }
            set
            {
                SetValue(DropPanelBorderThicknessProperty, value);
            }
        }

        public static readonly DependencyProperty DropPanelBorderBrushProperty = DependencyProperty.Register("DropPanelBorderBrush", typeof(Brush), typeof(MegadropMenu), new PropertyMetadata(null));
        public Brush DropPanelBorderBrush
        {
            get
            {
                return GetValue(DropPanelBorderBrushProperty) as Brush;
            }
            set
            {
                SetValue(DropPanelBorderBrushProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalDropPanelAlignmentProperty = DependencyProperty.Register("HorizontalDropPanelAlignment", typeof(HorizontalAlignment), typeof(MegadropMenu), new PropertyMetadata(HorizontalAlignment.Center));
        public HorizontalAlignment HorizontalDropPanelAlignment
        {
            get
            {
                return (HorizontalAlignment)GetValue(HorizontalDropPanelAlignmentProperty);
            }
            set
            {
                SetValue(HorizontalDropPanelAlignmentProperty, value);
            }
        }

        public static readonly DependencyProperty VerticalDropPanelAlignmentProperty = DependencyProperty.Register("VerticalDropPanelAlignment", typeof(VerticalAlignment), typeof(MegadropMenu), new PropertyMetadata(VerticalAlignment.Top));
        public VerticalAlignment VerticalDropPanelAlignment
        {
            get
            {
                return (VerticalAlignment)GetValue(VerticalDropPanelAlignmentProperty);
            }
            set
            {
                SetValue(VerticalDropPanelAlignmentProperty, value);
            }
        }

        public static readonly DependencyProperty DropPanelPaddingProperty = DependencyProperty.Register("DropPanelPadding", typeof(Thickness), typeof(MegadropMenu), new PropertyMetadata(new Thickness(0)));
        public Thickness DropPanelPadding
        {
            get
            {
                return (Thickness)GetValue(DropPanelPaddingProperty);
            }
            set
            {
                SetValue(DropPanelPaddingProperty, value);
            }
        }

        static MegadropMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MegadropMenu), new FrameworkPropertyMetadata(typeof(MegadropMenu)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            btnLeftNext = GetTemplateChild("btnLeftNext") as Button;
            btnLeftNext.Click -= btnLeftNext_Click;
            btnLeftNext.Click += btnLeftNext_Click;
            btnRightNext = GetTemplateChild("btnRightNext") as Button;
            btnRightNext.Click -= btnRightNext_Click;
            btnRightNext.Click += btnRightNext_Click;
            bdmegaDropBox = GetTemplateChild("bdmegaDropBox") as Border;

            megaDropBox = GetTemplateChild("megaDropBox") as Popup;
            megaDropBox.Opened += megaDropBox_Opened;
            megaDropBox.Closed += megaDropBox_Closed;

            this.MouseLeave += MegadropMenu_MouseLeave;

            this.MouseLeftButtonDown += (sender, arg) =>
            {
                arg.Handled = true;
            };

            attachMenuItemClickHandler();
        }

        void MegadropMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            megaDropBox.IsOpen = false;
        }

        void megaDropBox_Opened(object sender, EventArgs e)
        {
            System.Windows.Controls.ItemsControl dropBox = bdmegaDropBox.Child as System.Windows.Controls.ItemsControl;
            double totWidth = 0;

            foreach (LGC.GMES.MES.MainFrame.MegadropMenuItem item in dropBox.Items)
                totWidth += item.ActualWidth;

            if (this.ActualWidth < totWidth + ((dropBox.Items.Count - 1) * 60))
            {
                btnLeftNext.Visibility = Visibility.Visible;
                btnRightNext.Visibility = Visibility.Visible;
            }
            else
            {
                btnLeftNext.Visibility = Visibility.Collapsed;
                btnRightNext.Visibility = Visibility.Collapsed;
                bdmegaDropBox.Margin = new Thickness(0);
            }
        }

        void megaDropBox_Closed(object sender, EventArgs e)
        {
            unselectMenuItem(this, null);

            megaDropBox.StaysOpen = true;
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
            MegadropMenuItem menuItem = sender as MegadropMenuItem;

            unselectMenuItem(this, e.MenuPath);

            if (this.Items.Contains(menuItem))
            {
                SelectedMenuItem = menuItem;
                megaDropBox.IsOpen = true;
            }

            if (MenuItemClick != null)
                MenuItemClick(sender, e);
        }

        internal void unselectMenuItem(ItemsControl parent, List<MegadropMenuItem> exceptList)
        {
            foreach (object item in parent.Items)
            {
                if (item is MegadropMenuItem)
                {
                    MegadropMenuItem otherMenuItem = item as MegadropMenuItem;

                    if (exceptList == null || !exceptList.Contains(item))
                        otherMenuItem.gotoUnselectState();

                    unselectMenuItem(otherMenuItem, exceptList);
                }
            }
        }

        public void CloseMenu()
        {
            //megaDropBox.IsOpen = false;
        }

        private void btnLeftNext_Click(object sender, RoutedEventArgs e)
        {
            Thickness newthick = new Thickness();
            Thickness oldthick = bdmegaDropBox.Margin;
            System.Windows.Controls.ItemsControl item = bdmegaDropBox.Child as System.Windows.Controls.ItemsControl;
            newthick.Left = oldthick.Left + (item.ActualWidth / 10);

            if (newthick.Left > 0)
            {
                newthick.Left = 0;
                bdmegaDropBox.Margin = newthick;
            }
            else
            {
                bdmegaDropBox.Margin = newthick;
            }

            bdmegaDropBox.Margin = newthick;
        }

        private void btnRightNext_Click(object sender, RoutedEventArgs e)
        {
            Thickness newthick = new Thickness();
            Thickness oldthick = bdmegaDropBox.Margin;
            System.Windows.Controls.ItemsControl item = bdmegaDropBox.Child as System.Windows.Controls.ItemsControl;
            MegadropMenuItem itemoffset = item.Items[item.Items.Count - 1] as MegadropMenuItem;

            newthick.Left = oldthick.Left - itemoffset.ActualWidth;
            bdmegaDropBox.Margin = newthick;
            double totWidth = 0;

            foreach (MegadropMenuItem itemmenu in item.Items)
                totWidth += itemmenu.ActualWidth;

            if (newthick.Left < 0)
            {
                if (totWidth + 200 < Math.Abs(newthick.Left))
                    bdmegaDropBox.Margin = oldthick;
                else
                    bdmegaDropBox.Margin = newthick;
            }
            else
            {
                bdmegaDropBox.Margin = newthick;
            }
        }
    }
}