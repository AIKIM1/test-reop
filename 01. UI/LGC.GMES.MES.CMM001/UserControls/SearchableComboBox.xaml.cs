using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// SearchableComboBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SearchableComboBox : UserControl
    {
        string[] Names;

        public SearchableComboBox()
        {
            InitializeComponent();
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }

        private void PreviewTextInput_EnhanceComboSearch(object sender, TextCompositionEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;

            cmb.IsDropDownOpen = true;

            if (!string.IsNullOrEmpty(cmb.Text))
            {
                string fullText = cmb.Text.Insert(GetChildOfType<TextBox>(cmb).CaretIndex, e.Text);
                cmb.ItemsSource = Names.Where(s => s.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
            }
            else if (!string.IsNullOrEmpty(e.Text))
            {
                cmb.ItemsSource = Names.Where(s => s.IndexOf(e.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
            }
            else
            {
                cmb.ItemsSource = Names;
            }
        }

        private void Pasting_EnhanceComboSearch(object sender, DataObjectPastingEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;

            cmb.IsDropDownOpen = true;

            string pastedText = (string)e.DataObject.GetData(typeof(string));
            string fullText = cmb.Text.Insert(GetChildOfType<TextBox>(cmb).CaretIndex, pastedText);

            if (!string.IsNullOrEmpty(fullText))
                cmb.ItemsSource = Names.Where(s => s.IndexOf(fullText, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
            else
                cmb.ItemsSource = Names;
        }

        private void PreviewKeyUp_EnhanceComboSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                ComboBox cmb = (ComboBox)sender;

                cmb.IsDropDownOpen = true;

                Debug.WriteLine($" # cmb.Text po usunięciu: {cmb.Text}");

                if (!string.IsNullOrEmpty(cmb.Text))
                    cmb.ItemsSource = Names.Where(s => s.IndexOf(cmb.Text, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
                else
                    cmb.ItemsSource = Names;//DropdownNames
            }
        }
    }
}