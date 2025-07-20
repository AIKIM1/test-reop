using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF.DataGrid;
using System.Threading;
using System.ComponentModel;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_050_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_RUNSTART : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _EqsgID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProdID = string.Empty;
        BizDataSet _Biz = new BizDataSet();
        Util _Util = new Util();

        public ASSY004_050_RUNSTART()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            grdMsg.Visibility = Visibility.Collapsed;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 3)
            {
                _EqsgID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProdID = Util.NVC(tmps[2]);
            }
            else
            {
                _EqsgID = "";
                _EqptID = "";
                _ProdID = "";
            }

            GetInputMountInfo();
            GetWaitMagazinesByType(dgCType, "MC");
            GetWaitMagazinesByType(dgAType, "HC");

            GetModelList();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("MTRLID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PRODID"] = string.IsNullOrEmpty(_ProdID) ? null : _ProdID;
                inData.Rows.Add(newRow);

                for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID"));
                        newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "PRODID"));
                        newRow["ACTQTY"] = Util.NVC_Int(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "WIPQTY"));

                        inInput.Rows.Add(newRow);
                    }
                    else
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                        {
                            newRow = inInput.NewRow();
                            newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                            newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID"));
                            newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "PRODID"));
                            newRow["ACTQTY"] = Util.NVC_Int(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "WIPQTY"));

                            inInput.Rows.Add(newRow);
                        }
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_PROD_LOT_RWK_ST_L", "INDATA,IN_INPUT", "OUTDATA", indataSet);

                string lotID = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["LOTID"]);

                HideLoadingIndicator();

                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", lotID); // [%1] LOT이 생성 되었습니다.

                grdMsg.Visibility = Visibility.Visible;

                AsynchronousClose();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void dgCType_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    int iPutRow = -1;

                                    if (CanAddMagin(dg, e.Cell.Row.Index, "MC", out iPutRow))
                                    {

                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        AddInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row, "MC", iPutRow);
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    RemoveInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row, "MC");
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void dgInput_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                            (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void dgAType_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    int iPutRow = -1;

                                    if (CanAddMagin(dg, e.Cell.Row.Index, "HC", out iPutRow))
                                    {

                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        AddInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row, "HC", iPutRow);
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    RemoveInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row, "HC");
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void popSearchProdID_ValueChanged(object sender, EventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(popSearchProdID.ItemsSource);
            string prjt_name = string.Empty;
            foreach (DataRowView drv in dt.DefaultView)
            {
                if (Util.NVC(DataTableConverter.GetValue(drv, "PRODID")).Equals(popSearchProdID.SelectedValue as string))
                {
                    prjt_name = Util.NVC(DataTableConverter.GetValue(drv, "PRJT_NAME"));
                    break;
                }
            }
            _ProdID = Util.NVC(popSearchProdID.SelectedValue);
            txtPjtName.Text = prjt_name;

            GetWaitMagazinesByType(dgCType, "MC");
            GetWaitMagazinesByType(dgAType, "HC");

            //clear한 경우
            if (string.IsNullOrEmpty(_ProdID))
            {
                foreach(DataRowView drv in dgInput.ItemsSource)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(drv["SEL_LOTID"])))
                    {
                        DataTableConverter.SetValue(drv, "SEL_LOTID", "");
                        DataTableConverter.SetValue(drv, "PRDT_CLSS_CODE", "");
                        DataTableConverter.SetValue(drv, "WIPQTY", 0);
                        DataTableConverter.SetValue(drv, "PRODID", "");
                        DataTableConverter.SetValue(drv, "PRODNAME", "");
                    }
                }
            }
        }
        #endregion

        #region [Method]
        private void GetInputMountInfo()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO_RWK", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInput.CurrentCellChanged -= dgInput_CurrentCellChanged;
                        //dgInput.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInput, searchResult, null, true);
                        dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;

                        txtSelA.Text = searchResult.Select("PRDT_CLSS_CODE = 'HC'") == null ? "0" : searchResult.Select("PRDT_CLSS_CODE = 'HC'").Length.ToString();
                        txtSelC.Text = searchResult.Select("PRDT_CLSS_CODE = 'MC'") == null ? "0" : searchResult.Select("PRDT_CLSS_CODE = 'MC'").Length.ToString();

                        if (dgInput.CurrentCell != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                        else if (dgInput.Rows.Count > 0 && dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1) != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetWaitMagazinesByType(C1DataGrid datagrid, string sType)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("PRODID");
                inTable.Columns.Add("PRDT_LEVEL2_CODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["EQSGID"] = _EqsgID;
                newRow["PRODID"] = popSearchProdID.SelectedValue as string;
                newRow["PRDT_LEVEL2_CODE"] = sType;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_WAIT_MAG_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (datagrid.Name.Equals("dgAtype"))
                            datagrid.CurrentCellChanged -= dgAType_CurrentCellChanged;
                        else if (datagrid.Name.Equals("dgCType"))
                            datagrid.CurrentCellChanged -= dgCType_CurrentCellChanged;

                        datagrid.ItemsSource = DataTableConverter.Convert(searchResult);


                        if (datagrid.Name.Equals("dgAtype"))
                            datagrid.CurrentCellChanged += dgAType_CurrentCellChanged;
                        else if (datagrid.Name.Equals("dgCType"))
                            datagrid.CurrentCellChanged += dgCType_CurrentCellChanged;


                        if (datagrid.CurrentCell != null)
                            datagrid.CurrentCell = datagrid.GetCell(datagrid.CurrentCell.Row.Index, datagrid.Columns.Count - 1);
                        else if (datagrid.Rows.Count > 0 && datagrid.GetCell(datagrid.Rows.Count, datagrid.Columns.Count - 1) != null)
                            datagrid.CurrentCell = datagrid.GetCell(datagrid.Rows.Count, datagrid.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AddInMaz(DataRow addRow, string sType, int iInputRow)
        {
            try
            {
                if (iInputRow < 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                if (!dtTmp.Columns.Contains("PRDT_CLSS_CODE"))
                    dtTmp.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                if (!dtTmp.Columns.Contains("MTRL_CLSS_CODE"))
                    dtTmp.Columns.Add("MTRL_CLSS_CODE", typeof(string));
                if (!dtTmp.Columns.Contains("PRODID"))
                    dtTmp.Columns.Add("PRODID", typeof(string));
                if (!dtTmp.Columns.Contains("PRODNAME"))
                    dtTmp.Columns.Add("PRODNAME", typeof(string));
                if (!dtTmp.Columns.Contains("WIPQTY"))
                    dtTmp.Columns.Add("WIPQTY", typeof(int));

                for (int i = 0; i < dtTmp.Columns.Count; i++)
                {
                    for (int j = 0; j < addRow.Table.Columns.Count; j++)
                    {
                        if (dtTmp.Columns[i].ColumnName.Equals(addRow.Table.Columns[j].ColumnName))
                        {
                            if (addRow[j].GetType() == typeof(string))
                            {
                                dtTmp.Rows[iInputRow][i] = Util.NVC(addRow[j]);
                            }
                            else if (addRow.Table.Columns[j].ColumnName.Equals("CHK"))
                            {
                                dtTmp.Rows[iInputRow][i] = false;
                            }
                            else
                            {
                                dtTmp.Rows[iInputRow][i] = addRow[j];
                            }
                        }
                        else if (dtTmp.Columns[i].ColumnName.Equals("SEL_LOTID") && addRow.Table.Columns[j].ColumnName.Equals("LOTID"))
                        {
                            dtTmp.Rows[iInputRow][i] = Util.NVC(addRow[j]);
                        }

                        if (dtTmp.Columns[i].ColumnName.Equals("MAG_TYPE"))
                        {
                            dtTmp.Rows[iInputRow][i] = sType;
                        }
                    }
                }

                dtTmp.AcceptChanges();
                dgInput.BeginEdit();
                dgInput.ItemsSource = DataTableConverter.Convert(dtTmp);
                dgInput.EndEdit();

                if (sType.Equals("HC"))
                    txtSelA.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();
                else
                    txtSelC.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();

                if (dgInput.CurrentCell != null)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                else if (dgInput.Rows.Count > 0)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RemoveInMaz(DataRow removeRow, string sType)
        {
            try
            {
                int idx = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", Util.NVC(removeRow["LOTID"]));
                if (idx < 0)
                    return;

                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "SEL_LOTID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRDT_CLSS_CODE", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "WIPQTY", 0);
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODNAME", "");

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                if (dtTmp == null || dtTmp.Rows.Count <= 0)
                    return;

                if (sType.Equals("HC"))
                    txtSelA.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();
                else
                    txtSelC.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();

                if (dgInput.CurrentCell != null)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                else if (dgInput.Rows.Count > 0)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetModelList()
        {
            try
            {

                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("CLASS_LEVEL3_CODE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                //제품코드 Stacked Cell
                newRow["CLASS_LEVEL3_CODE"] = "SC";

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_FP_EQSG_PROC_PRDT_BY_PROC", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [Util & Valid Method]
        private bool CanAddMagin(C1.WPF.DataGrid.C1DataGrid dgReady, int iRedSelRow, string sType, out int iPutRow)
        {
            bool bRet = false;

            iPutRow = -1;

            string sTmpLot = Util.NVC(DataTableConverter.GetValue(dgReady.Rows[iRedSelRow].DataItem, "LOTID"));
            string sTmpType = Util.NVC(DataTableConverter.GetValue(dgReady.Rows[iRedSelRow].DataItem, "PRODUCT_LEVEL2_CODE"));
            // 투입LOT 중복 체크
            for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(sTmpLot))
                {
                    //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1967");
                    return bRet;
                }
                else if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(sTmpLot))
                {
                    //Util.Alert("선택한 LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1657");
                    return bRet;
                }

                // 투입LOT이 없고 선택 Lot이 없는 Row.
                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD") && Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "MTRL_CLSS_CODE")).Equals(sTmpType))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals("") &&
                    Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        if (iPutRow < 0) iPutRow = i;
                    }
                }


            }

            if (iPutRow < 0)
            {
                //Util.Alert("더이상 투입할 수 없습니다.");
                dgReady.SelectedIndex = -1;
                Util.MessageValidation("SFU1222");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion
    }
}
