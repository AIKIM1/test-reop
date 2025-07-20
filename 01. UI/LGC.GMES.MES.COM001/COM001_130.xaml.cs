/*************************************************************************************
 Created Date : 2018.07.20
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 조립공정 선별일지
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.20  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_130.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_130 : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        public COM001_130()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                DataRow dr = dt.NewRow();
                //if (dt.Columns.Contains("CHK"))
                //{
                //    dr["CHK"] = true;
                //}
                dt.Rows.Add(dr);

                Util.GridSetData(dgList, dt, FrameOperation);
                dgList.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_130_EDIT wndEdit = new COM001_130_EDIT();
                wndEdit.FrameOperation = FrameOperation;
                object[] Parameters = new object[4];


                Parameters[0] = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgList, "CHK")].DataItem, "HIST_SEQNO"));
                Parameters[1] = "";
                Parameters[2] = "";               

                C1WindowExtension.SetParameters(wndEdit, Parameters);

                wndEdit.Closed += new EventHandler(wndEdit_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndEdit.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeletePickWrkList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
                InitControls();

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("SHFT_ID") ||
                    e.Column.Name.Equals("PICK_TRGT_CODE") ||
                    e.Column.Name.Equals("PICK_UNIT") ||
                    e.Column.Name.Equals("DFCT_TRGT_NAME") ||
                    e.Column.Name.Equals("DFCT_UNIT"))
                {
                    var combo = e.EditingElement as C1ComboBox;
                    
                    string sCmcdType = string.Empty;
                    if (e.Column.Name.Equals("SHFT_ID"))
                        sCmcdType = "SELECTION_SHFT_TYPE_CODE";
                    else if (e.Column.Name.Equals("PICK_TRGT_CODE") || e.Column.Name.Equals("DFCT_TRGT_NAME"))
                        sCmcdType = "SELECTION_TARGET_CODE";
                    else if (e.Column.Name.Equals("PICK_UNIT") || e.Column.Name.Equals("DFCT_UNIT"))
                        sCmcdType = "SELECTION_UNIT_CODE";


                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("CMCDTYPE", typeof(string));

                    DataRow tmpDataRow = inDataTable.NewRow();
                    tmpDataRow["LANGID"] = LoginInfo.LANGID;
                    tmpDataRow["CMCDTYPE"] = sCmcdType;

                    inDataTable.Rows.Add(tmpDataRow);
                    // 작업조
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "INDATA", "RSLTDT", inDataTable);
                  
                    combo.ItemsSource = DataTableConverter.Convert(dtRslt.Copy());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (!e.Column.Name.Equals("CHK"))
                {
                    //if (Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).ToUpper().Equals("FALSE") ||
                    //    Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).Equals("0") ||
                    //    Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).Equals(""))
                        e.Cancel = true;
                }

                if (e.Column.Name.Equals("LOTID") ||
                    e.Column.Name.Equals("PRJT_NAME"))
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
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

                                    if (dg.CurrentCell != null)
                                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                                    else if (dg.Rows.Count > 0)
                                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);                          
                                    break;
                            }
                        }
                        else if (e.Cell.Column.Index != dg.Columns.Count - 1)
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

                        //if (dg.CurrentCell != null)
                        //    dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        //else if (dg.Rows.Count > 0)
                        //    dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sFilter: sFilter);

        }

        private void InitControls()
        {
            try
            {
                DataTable dt = new DataTable();
                for (int i = 0; i < dgList.Columns.Count; i++)
                {
                    dt.Columns.Add(dgList.Columns[i].Name);
                }

                //DataRow dr = dt.NewRow();
                //dt.Rows.Add(dr);
                Util.GridSetData(dgList, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("FRDT", typeof(string));
                dtRqst.Columns.Add("TODT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FRDT"] = Util.GetCondition(dtpDateFrom);
                dr["TODT"] = Util.GetCondition(dtpDateTo);

                if (!Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                }

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SELECTION_DIARY_LIST", "INDATA", "OUTDATA", dtRqst, (result, bizEx) => 
                {
                    try
                    {
                        if (bizEx != null)
                        {
                            Util.MessageException(bizEx);
                            return;
                        }

                        dgList.CurrentCellChanged -= dgList_CurrentCellChanged;
                        Util.GridSetData(dgList, result, FrameOperation, true);
                        dgList.CurrentCellChanged += dgList_CurrentCellChanged;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void wndEdit_Closed(object sender, EventArgs e)
        {
            COM001_130_EDIT window = sender as COM001_130_EDIT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
        }

        public void DeletePickWrkList()
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
                {
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";

                RQSTDT.Columns.Add("HIST_SEQNO", typeof(String));
                RQSTDT.Columns.Add("DEL_FLAG", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));

                DataTable dTmp = DataTableConverter.Convert(dgList.ItemsSource);

                DataRow[] drTmpLst = dTmp?.Select("CHK='1'");

                foreach (DataRow drTmp in drTmpLst)
                {
                    DataRow dr = RQSTDT.NewRow();

                    dr["HIST_SEQNO"] = Util.NVC(drTmp["HIST_SEQNO"]);
                    dr["DEL_FLAG"] = "Y";
                    dr["USERID"] = LoginInfo.USERID;

                    RQSTDT.Rows.Add(dr);
                }

                if (RQSTDT.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService("DA_PRD_UPD_PICK_WRK_HIST", "INDATA", "", RQSTDT, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU3544");
                        GetList();
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
