/*************************************************************************************
 Created Date : 2016.11.20
      Creator : JEONG JONGWON
   Decription : 원각형 조립 Collect BaseForm
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyOutProduct.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyOutProduct
    {
        public C1DataGrid DgOut { get; set; }

        //public struct PreviewValuesstruct
        //{
        //    public string PreviewTray;

        //    public PreviewValuesstruct(string trayCode)
        //    {
        //        PreviewTray = trayCode;
        //    }
        //}

        //public PreviewValuesstruct PreviewValues = new PreviewValuesstruct("");

        public string PreviewValues { get; set; }

        public UcAssyOutProduct()
        {
            InitializeComponent();
            SetControl();
            PreviewValues = string.Empty;
        }

        private void SetControl()
        {
            DgOut = dgOut;
        }

        private void dgOut_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgOut_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            if (dg != null)
                            {
                                var box = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (box != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                    box.IsChecked.HasValue &&
                                                    !(bool)box.IsChecked))
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        // 이전 값 저장.
                                        PreviewValues = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));
                                        SetOutTrayButtonEnable(e.Cell.Row);

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var checkBox = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (checkBox !=null)
                                                        checkBox.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             (bool)checkBox.IsChecked))
                                    {
                                        if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                            PreviewValues = string.Empty;
                                            // 확정 시 저장, 삭제 버튼 비활성화
                                            SetOutTrayButtonEnable(null);
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    if (dgOut.CurrentCell != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    else if (dgOut.Rows.Count > 0)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);

                }
            }));
        }

        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgOut_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        public void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                /*
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutCreate.IsEnabled = true;
                    btnOutDel.IsEnabled = true;
                    btnOutConfirmCancel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = true;
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
