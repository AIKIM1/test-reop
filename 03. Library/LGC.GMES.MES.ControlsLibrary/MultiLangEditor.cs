using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MultiLangEditor : Control
    {
        private C1DataGrid dg;

        public event EventHandler TextChanged;

        private bool reevaluate = false;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MultiLangEditor), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d as MultiLangEditor).reevaluate && (d as MultiLangEditor).dg != null)
            {
                string value = string.Empty;
                if (e.NewValue != null)
                {
                    value = (string)e.NewValue;
                }
                else
                {
                    value = string.Empty;
                }

                Dictionary<string, string> textParts = new Dictionary<string, string>();
                try
                {
                    string[] partArray = value.Split('|');
                    foreach (string part in partArray)
                    {
                        if (part == null)
                            continue;

                        string[] partDetail = part.Split('\\');
                        textParts.Add(partDetail[0], partDetail[1]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    textParts.Clear();
                }

                System.Data.DataTable table = new System.Data.DataTable();

                table.Columns.Add("LANGID", typeof(string));
                table.Columns.Add("LANGNAME", typeof(string));
                table.Columns.Add("VALUE", typeof(string));

                System.Data.DataRow newRow = null;

                foreach (object langInfo in LoginInfo.SUPPORTEDLANGINFOLIST)
                {
                    newRow = table.NewRow();
                    newRow["LANGID"] = DataTableConverter.GetValue(langInfo, "LANGID");
                    newRow["LANGNAME"] = DataTableConverter.GetValue(langInfo, "LANGNAME");
                    if (textParts.ContainsKey((string)DataTableConverter.GetValue(langInfo, "LANGID")))
                    {
                        newRow["VALUE"] = textParts[(string)DataTableConverter.GetValue(langInfo, "LANGID")];
                    }
                    else
                    {
                        newRow["VALUE"] = string.Empty;
                    }
                    table.Rows.Add(newRow);
                }
                (d as MultiLangEditor).dg.ItemsSource = DataTableConverter.Convert(table);

                //DataTable table = new DataTable();
                //table.Columns.Add(new DataColumn() { ColumnName = "LANGID", DataType = typeof(string) });
                //table.Columns.Add(new DataColumn() { ColumnName = "LANGNAME", DataType = typeof(string) });
                //table.Columns.Add(new DataColumn() { ColumnName = "VALUE", DataType = typeof(string) });
                //foreach (object langInfo in LoginInfo.SUPPORTEDLANGINFOLIST)
                //{
                //    DataRow row = new DataRow();
                //    row.Add("LANGID", DataTableConverter.GetValue(langInfo, "LANGID"));
                //    row.Add("LANGNAME", DataTableConverter.GetValue(langInfo, "LANGNAME"));
                //    if (textParts.ContainsKey((string)DataTableConverter.GetValue(langInfo, "LANGID")))
                //    {
                //        row.Add("VALUE", textParts[(string)DataTableConverter.GetValue(langInfo, "LANGID")]);
                //    }
                //    else
                //    {
                //        row.Add("VALUE", string.Empty);
                //    }
                //    table.Rows.Add(row);
                //}

                //(d as MultiLangEditor).dg.ItemsSource = table;
            }
        }

        public MultiLangEditor()
        {
            this.DefaultStyleKey = typeof(MultiLangEditor);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            dg = GetTemplateChild("dg") as C1DataGrid;

            string value = this.Text;

            Dictionary<string, string> textParts = new Dictionary<string, string>();
            try
            {
                string[] partArray = value.Split('|');
                foreach (string part in partArray)
                {
                    string[] partDetail = part.Split('\\');
                    textParts.Add(partDetail[0], partDetail[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                textParts.Clear();
            }

            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("LANGID", typeof(string));
            table.Columns.Add("LANGNAME", typeof(string));
            table.Columns.Add("VALUE", typeof(string));
            System.Data.DataRow newRow = null;
            foreach (object langInfo in LoginInfo.SUPPORTEDLANGINFOLIST)
            {
                newRow = table.NewRow();
                newRow["LANGID"] = DataTableConverter.GetValue(langInfo, "LANGID");
                newRow["LANGNAME"] = DataTableConverter.GetValue(langInfo, "LANGNAME");
                if (textParts.ContainsKey((string)DataTableConverter.GetValue(langInfo, "LANGID")))
                {
                    newRow["VALUE"] = textParts[(string)DataTableConverter.GetValue(langInfo, "LANGID")];
                }
                else
                {
                    newRow["VALUE"] = string.Empty;
                }
                table.Rows.Add(newRow);
            }
            dg.ItemsSource = DataTableConverter.Convert(table);
            dg.CommittingEdit += dg_CommittingEdit;
            dg.LostFocus += EditingElement_LostFocus;
            dg.Loaded += dg_Loaded;

            //DataTable table = new DataTable();
            //table.Columns.Add(new DataColumn() { ColumnName = "LANGID", DataType = typeof(string) });
            //table.Columns.Add(new DataColumn() { ColumnName = "LANGNAME", DataType = typeof(string) });
            //table.Columns.Add(new DataColumn() { ColumnName = "VALUE", DataType = typeof(string) });
            //foreach (object langInfo in LoginInfo.SUPPORTEDLANGINFOLIST)
            //{
            //    DataRow row = new DataRow();
            //    row.Add("LANGID", DataTableConverter.GetValue(langInfo, "LANGID"));
            //    row.Add("LANGNAME", DataTableConverter.GetValue(langInfo, "LANGNAME"));
            //    if (textParts.ContainsKey((string)DataTableConverter.GetValue(langInfo, "LANGID")))
            //    {
            //        row.Add("VALUE", textParts[(string)DataTableConverter.GetValue(langInfo, "LANGID")]);
            //    }
            //    else
            //    {
            //        row.Add("VALUE", string.Empty);
            //    }
            //    table.Rows.Add(row);
            //}
            ////dg.ItemsSource = DataTableConverter.Convert(table);
            //dg.CommittingEdit += dg_CommittingEdit;
            //this.LostFocus += EditingElement_LostFocus;
        }

        public DependencyObject FindChild(DependencyObject o, Type childType)
        {
            DependencyObject foundChild = null;
            if (o != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(o);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(o, i);
                    if (child.GetType() != childType)
                    {
                        foundChild = FindChild(child, childType);
                        break;
                    }
                    else
                    {
                        foundChild = child;
                        break;
                    }
                }
            }
            return foundChild;

        }

        public bool findDescendent(DependencyObject root, object targetChild)
        {
            if (root == targetChild)
            {
                return true;
            }
            else
            {
                int count = VisualTreeHelper.GetChildrenCount(root);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(root, i);
                    if (findDescendent(child, targetChild))
                        return true;
                }
            }

            return false;
        }

        void EditingElement_LostFocus(object sender, RoutedEventArgs e)
        {
            object obj = FocusManager.GetFocusedElement(Application.Current.MainWindow);

            if (obj != null)
            {
                DependencyObject child = FindChild(this, obj.GetType());
                if (child == null)
                {
                    if (obj.GetType() == typeof(C1.WPF.C1TextBoxBase))
                    {
                        C1.WPF.C1TextBoxBase txt = obj as C1.WPF.C1TextBoxBase;
                        txt.Focus();
                        txt.SelectAll();
                    }
                    else
                    {
                        dg_CommittingEdit(null, null);
                        if (this.TemplatedParent != null)
                        {
                            VisualStateManager.GoToState(this.TemplatedParent as MultiLangTextBox, "Unfocused", false);
                        }
                    }
                }
                if (obj.GetType() == typeof(C1.WPF.DataGrid.C1DataGrid))
                {
                    C1.WPF.DataGrid.C1DataGrid dg = obj as C1.WPF.DataGrid.C1DataGrid;
                    dg.EndEdit(true);
                }
            }

            //if (this.TemplatedParent != null)
            //{
            //    VisualStateManager.GoToState(this.TemplatedParent as MultiLangTextBox, "Unfocused", false);
            //}
            //object obj = FocusManager.GetFocusedElement(this);
            ////object obj = FocusManager.GetFocusedElement();
            //if (obj == null || !findDescendent(this, obj))
            //{
            //    dg.EndEdit(true);
            //}
        }

        internal void EndEdit(bool commitChanges)
        {
            dg.EndEdit(commitChanges);
        }

        void dg_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                sb.Append(DataTableConverter.GetValue(row.DataItem, "LANGID"));
                sb.Append("\\");
                sb.Append(DataTableConverter.GetValue(row.DataItem, "VALUE"));
                sb.Append("|");
            }
            string result = sb.ToString();
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }

            reevaluate = true;

            this.Text = result;

            reevaluate = false;

            if (TextChanged != null)
            {
                TextChanged(this, null);
            }
        }

        private void dg_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (Border bd in FindVisualChildren<Border>(dg))
                {
                    if (bd.Name.ToString() == "GridBorder")
                    {
                        bd.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3C3C3"));
                        bd.BorderThickness = new Thickness(1);
                    }
                }
            }));
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

    }
}
