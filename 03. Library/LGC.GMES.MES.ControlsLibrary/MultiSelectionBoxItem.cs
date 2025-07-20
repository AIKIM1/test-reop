using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MultiSelectionBoxItem : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void firePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CheckBox chk;

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked",
                                                                                                  typeof(bool),
                                                                                                  typeof(MultiSelectionBoxItem),
                                                                                                  new PropertyMetadata(false,
                                                                                                                       (d, arg) =>
                                                                                                                       {
                                                                                                                           MultiSelectionBoxItem multiSelectionBoxItem = d as MultiSelectionBoxItem;
                                                                                                                           if (arg.OldValue != arg.NewValue)
                                                                                                                               multiSelectionBoxItem.firePropertyChanged("IsChecked");
                                                                                                                       }));
        public bool IsChecked
        {
            get
            {
                return (bool)this.GetValue(IsCheckedProperty);
            }
            set
            {
                this.SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsAllProperty = DependencyProperty.Register("IsAll", typeof(bool), typeof(MultiSelectionBoxItem), new PropertyMetadata(false));
        public bool IsAll
        {
            get
            {
                return (bool)this.GetValue(IsAllProperty);
            }
            set
            {
                this.SetValue(IsAllProperty, value);
            }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(object), typeof(MultiSelectionBoxItem), new PropertyMetadata(null));
        public object Item
        {
            get
            {
                return this.GetValue(ItemProperty);
            }
            set
            {
                this.SetValue(ItemProperty, value);
            }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(MultiSelectionBoxItem), new PropertyMetadata(null));
        private ContentPresenter cp;
        public DataTemplate ItemTemplate
        {
            get
            {
                return this.GetValue(ItemTemplateProperty) as DataTemplate;
            }
            set
            {
                this.SetValue(ItemTemplateProperty, value);
            }
        }

        static MultiSelectionBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectionBoxItem), new FrameworkPropertyMetadata(typeof(MultiSelectionBoxItem)));
        }

        public MultiSelectionBoxItem()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            chk = GetTemplateChild("chk") as CheckBox;
        }

        internal Size MeasureActualSize()
        {
            if (chk != null)
            {
                chk.Measure(new Size(1024, 768));

                return chk.DesiredSize;
            }
            else
            {
                return new Size(0, 0);
            }
        }
    }
}
