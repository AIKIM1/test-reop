using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
    public class DataGridMultiLangColumn : C1.WPF.DataGrid.DataGridBoundColumn
    {
        public override object GetCellContentRecyclingKey(C1.WPF.DataGrid.DataGridRow row)
        {
            return typeof(MultiLangTextPresenter);
        }

        public override FrameworkElement CreateCellContent(C1.WPF.DataGrid.DataGridRow row)
        {
            return new MultiLangTextPresenter() { BorderThickness = new Thickness(0), Background = new SolidColorBrush(Colors.Transparent) };
        }

        public override void BindCellContent(FrameworkElement cellContent, C1.WPF.DataGrid.DataGridRow row)
        {
            MultiLangTextPresenter presenter = cellContent as MultiLangTextPresenter;
            if (Binding != null)
            {
                Binding newBinding = CopyBinding(Binding);
                newBinding.Source = row.DataItem;
                presenter.SetBinding(MultiLangTextPresenter.TextProperty, newBinding);
            }
            presenter.HorizontalAlignment = this.HorizontalAlignment;
            presenter.VerticalAlignment = this.VerticalAlignment;
        }

        public override FrameworkElement GetCellEditingContent(C1.WPF.DataGrid.DataGridRow row)
        {
            MultiLangEditor editor = new MultiLangEditor();
            if (Binding != null)
            {
                Binding newBinding = CopyBinding(Binding);
                newBinding.Mode = BindingMode.OneTime;
                newBinding.Source = row.DataItem;
                editor.SetBinding(MultiLangEditor.TextProperty, newBinding);
            }
            return editor;
        }

        public override void EndEdit(FrameworkElement editingElement)
        {
            MultiLangEditor editor = editingElement as MultiLangEditor;
            editor.EndEdit(true);

            base.EndEdit(editingElement);

            if (Binding != null)
            {
                if (editor.DataContext != null)
                {
                    DataTableConverter.SetValue(editor.DataContext, Binding.Path.Path, editor.Text);
                }
                else
                {
                    C1.WPF.DataGrid.DataGridCellPresenter pre = editor.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    if (pre != null)
                    {
                        DataTableConverter.SetValue(pre.Cell.Row.DataItem, Binding.Path.Path, editor.Text);
                    }
                }

            }
        }
    }
}
