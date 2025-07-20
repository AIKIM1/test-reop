/*************************************************************************************
 Created Date : 2019.01.21
      Creator : 
   Decription : GMES 고도화 - 포장대기 전극 전산화 (C20181115_44802)
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.21  INS 김동일K : Initial Created.

**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_225_STANDBY_INVENTORY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_225_STANDBY_INVENTORY : C1Window, IWorkArea
    {
        bool bChanged = false;
        private string MONTH_SEQ = "";
        private string DAY_SEQ = "";         

        private DataTable _dtShft = new DataTable();

        private Util _Util = new Util();

        public delegate void PackingOutDataHandler(object obj, string sTransLoc, string sPackWay);

        public event PackingOutDataHandler PackingOutEvent;
        
        public BOX001_225_STANDBY_INVENTORY()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 2)
                {
                    //_LineID = Util.NVC(tmps[0]);
                    //_EqptID = Util.NVC(tmps[1]);                    
                }
                else
                {
                    //_LineID = "";
                    //_EqptID = "";                   
                }

                ApplyPermissions();

                this.Loaded -= C1Window_Loaded;

                Initialize();

                InitPopupControls();

                loadingIndicator.Visibility = Visibility.Collapsed;
                loadingIndicator2.Visibility = Visibility.Collapsed;

                btnPackOut.IsEnabled = true;
                btnSearch.IsEnabled = true;
                btnModify.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboTransLoc_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MONTH_SEQ = "";
                DAY_SEQ = "";

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (iRow >= 0)
                {
                    this.txtPopDate.Text = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PACK_DATE"));

                    // 팝업 파라미터 설정.
                    DataTable dtSrc = DataTableConverter.Convert(dgList.ItemsSource);
                    if (!dtSrc.Columns.Contains("NEW_YN"))
                        dtSrc.Columns.Add("NEW_YN", typeof(string));

                    DataTable dtTgt = dtSrc.Clone();

                    for(int i = 0; i< dtSrc.Rows.Count; i++)
                    {
                        if(dtSrc.Rows[i]["CHK"].ToString().Equals("1"))
                        {
                            if(!string.IsNullOrEmpty(dtSrc.Rows[i]["SHIPTO_ID"].ToString())) {
                                cboTransLoc.SelectedValue = dtSrc.Rows[i]["SHIPTO_ID"].ToString();
                            }
                        }
                    }
                    foreach (DataRow drSrc in dtSrc.Rows)
                    {
                        if (drSrc["CHK"].ToString().Equals("1") || drSrc["CHK"].ToString().ToUpper().Equals("TRUE"))
                        {                            
                            dtTgt.ImportRow(drSrc);
                        }
                    }

                    dgModifyList.MergingCells -= MergingCells;

                    if (dtTgt.Rows.Count > 0)
                    {
                        Util.GridSetData(dgModifyList, dtTgt, FrameOperation, false);

                        // Shift 콤보
                        (dgModifyList.Columns["SHFT_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtShft.Copy());
                    }
                    
                    dgModifyList.MergingCells += MergingCells;
                }
                else
                {
                    this.txtPopDate.Text = this.dtpdate.SelectedDateTime.ToString("yyyy-MM-dd");

                    GetSeqInfo();
                }

                //this.dtpPopdate.Text = this.dtpdate.Text;
                //this.dtpPopdate.SelectedDateTime = this.dtpdate.SelectedDateTime;
                                
                this.grdModify.Visibility = Visibility.Visible;
                this.txtPopLotID.Focus();

                //this.dtpPopdate.SelectedDataTimeChanged -= dtpPopdate_SelectedDataTimeChanged;
                //this.dtpPopdate.SelectedDataTimeChanged += dtpPopdate_SelectedDataTimeChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 출고처 Validation
                bool bVldPass = true;
                string sPrvLotID = string.Empty;
                string sCstID = string.Empty;
                string sValueToKey = string.Empty;
                string sValueToFind = string.Empty;

                for (int i = 0; i < dgList.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    // 중복 Lot 확인
                    if (!sPrvLotID.Equals("") && Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID")).Equals(sPrvLotID))
                    {
                        Util.Alert("SFU1914");   //중복 스캔되었습니다.
                        return;
                    }
                    sPrvLotID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));


                    // 재공정보 조회
                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("LOTID", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["LOTID"] = sPrvLotID;

                    RQSTDT2.Rows.Add(dr2);

                    DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_PANCAKE", "RQSTDT", "RSLTDT", RQSTDT2);

                    if (Lot_Result.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");   //재공 정보가 없습니다.
                        return;
                    }

                    string ValueToKey = string.Empty;
                    string ValueToFind = string.Empty;

                    Dictionary<string, string> ValueToCompany = getShipCompany(Util.NVC(Lot_Result.Rows[0]["PRODID"]));
                    foreach (KeyValuePair<string, string> items in ValueToCompany)
                    {
                        ValueToKey = items.Key;
                        ValueToFind = items.Value;
                    }

                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        bVldPass = false;

                        sValueToKey = ValueToKey;
                        sValueToFind = ValueToFind;                        
                    }
                }

                if (bVldPass)
                {
                    Util.MessageConfirm("SFU6014", (result) =>  // 포장구성 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ProcessPackOut();

                            btnPackOut.IsEnabled = true;
                            btnSearch.IsEnabled = true;
                            btnModify.IsEnabled = true;

                            loadingIndicator.Visibility = Visibility.Collapsed;

                            //loadingIndicator.Visibility = Visibility.Visible;

                            //btnPackOut.IsEnabled = false;
                            //btnSearch.IsEnabled = false;
                            //btnModify.IsEnabled = false;

                            //int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                            //if (iRow < 0)
                            //{
                            //    Util.MessageValidation("SFU1663"); //스캔한 데이터가 없습니다.
                            //    return;
                            //}

                            //DataTable dtSrc = DataTableConverter.Convert(dgList.ItemsSource);
                            //DataTable dtTgt = dtSrc.Clone();
                            //foreach (DataRow drSrc in dtSrc.Rows)
                            //{
                            //    if (drSrc["CHK"].ToString().Equals("1") || drSrc["CHK"].ToString().ToUpper().Equals("TRUE"))
                            //    {
                            //        dtTgt.ImportRow(drSrc);
                            //    }
                            //}

                            //this.PackingOutEvent(dtTgt, cboTransLoc?.SelectedValue?.ToString(), cboPackWay?.SelectedValue?.ToString());

                            //this.DialogResult = MessageBoxResult.OK;
                        }
                    });
                }
                else
                {
                    Util.MessageConfirm("SFU5048", (result) =>
                    {
                        if (result == MessageBoxResult.Cancel)
                        {
                            return;
                        }

                        ProcessPackOut();

                        btnPackOut.IsEnabled = true;
                        btnSearch.IsEnabled = true;
                        btnModify.IsEnabled = true;

                        loadingIndicator.Visibility = Visibility.Collapsed;

                    }, new object[] { sValueToFind, sValueToKey });
                }
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PACK_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MODLID"] = txtModel.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PACK_DATE"] = Util.GetCondition(dtpdate);

                dtRqst.Rows.Add(dr);

                //new ClientProxy().ExecuteService("BR_PRD_SEL_PACKING_STNB_LIST", "INDATA", "OUTDATA", dtRqst, (result, bizEx) =>
                new ClientProxy().ExecuteService("DA_PRD_SEL_PACKING_STANDBY_LIST_V2", "INDATA", "OUTDATA", dtRqst, (result, bizEx) =>
                {
                    try
                    {
                        if (bizEx != null)
                        {
                            Util.MessageException(bizEx);
                            return;
                        }

                        dgList.MergingCells -= MergingCells;

                        Util.GridSetData(dgList, result, FrameOperation, true);

                        //dgList.MergingCells -= MergingCells;
                        dgList.MergingCells += MergingCells;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {                        
                        _Util.SetDataGridMergeExtensionCol(dgList, new string[] { "SHIPTO_ID" }, DataGridMergeMode.VERTICALHIERARCHI);
                        _Util.SetDataGridMergeExtensionCol(dgList, new string[] { "BOXCHK" }, DataGridMergeMode.VERTICALHIERARCHI);
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

        private void cboPackWay_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;

                                        SetCheckSameSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                                    }
                                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                    {
                                        

                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        if (!SetCheckSameSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                            return;
                                    }
                                    break;
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //this.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    C1DataGrid dg = sender as C1DataGrid;
                //    if (e.Cell != null &&
                //        e.Cell.Presenter != null &&
                //        e.Cell.Presenter.Content != null)
                //    {
                //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                //        if (chk != null)
                //        {
                //            switch (Convert.ToString(e.Cell.Column.Name))
                //            {
                //                case "CHK":
                //                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                //                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                //                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                //                        chk.IsChecked = true;

                //                        if (!SetCheckSameSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                //                            return;

                //                        //row 색 바꾸기
                //                        //dgList.SelectedIndex = e.Cell.Row.Index;
                //                    }
                //                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                //                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                //                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                //                        chk.IsChecked = false;

                //                        SetCheckSameSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                //                    }
                //                    break;
                //            }
                //        }
                //    }
                //}));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }
                        
                        //Grid Data Binding 이용한 Background 색 변경
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e.Cell.Column.Name.Equals("ELECTYPE_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELECTYPE")).Equals("A"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if ((e.Cell.Column.Name.Equals("OQCPASS_NAME") || e.Cell.Column.Name.Equals("WIPHOLD")) && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OQCPASS")).Equals("P") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OQCPASS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if ((e.Cell.Column.Name.Equals("OQCPASS_NAME") || e.Cell.Column.Name.Equals("WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Initialize()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
                String[] sFilter2 = { "WH_DIVISION" };
                String[] sFilter3 = { "WH_TYPE2" };

                _combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "TRANSLOC");
                _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "PACKWAY");

                Util.gridClear(dgList);


                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CMCDTYPE", typeof(string));

                DataRow tmpDataRow = inDataTable.NewRow();
                tmpDataRow["LANGID"] = LoginInfo.LANGID;
                tmpDataRow["CMCDTYPE"] = "SELECTION_SHFT_TYPE_CODE";

                inDataTable.Rows.Add(tmpDataRow);
                // 작업조
                _dtShft = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "INDATA", "RSLTDT", inDataTable);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnModify);
            //listAuth.Add(btnSave);
            listAuth.Add(btnPackOut);
            listAuth.Add(btnSearch);

            //listAuth.Add(btnPopAdd);
            listAuth.Add(btnPopRemove);
            listAuth.Add(btnPopSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool SetCheckSameSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;
            
            string sSeq = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "MONTHSEQ"));

            // 모두 Uncheck 처리 및 동일 자랏의 경우는 Check 처리.
            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (dataitem.Index != i)
                {
                    if (sSeq == Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MONTHSEQ")))
                    {
                        if (bUncheckAll)
                        {
                            if (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter != null &&
                            dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                            {
                                (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                            }
                            DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
                        }
                        else
                        {
                            if (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter != null &&
                            dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                            {
                                (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                            }
                            DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                        }

                    }
                    else
                    {
                        if (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter != null &&
                            dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                        {
                            (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                        }
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
                    }
                }
            }

            return true;
        }

        private void txtPopLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = string.Empty;
                    string sCstID = string.Empty;
                    string sCstID2 = string.Empty;

                    try
                    {
                        if (sender.GetType() == typeof(string))
                        {
                            sLotid = sender.ToString();
                        }
                        else
                        {
                            sLotid = txtPopLotID.Text.ToString().Trim();
                        }

                        if (sLotid == "")
                        {
                            Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                            return;
                        }

                        // 출고 이력 조회
                        DataTable RQSTDT0 = new DataTable();
                        RQSTDT0.TableName = "RQSTDT";
                        RQSTDT0.Columns.Add("CSTID", typeof(String));
                        RQSTDT0.Columns.Add("AREAID", typeof(String));

                        DataRow dr0 = RQSTDT0.NewRow();
                        dr0["CSTID"] = sLotid;
                        dr0["AREAID"] = LoginInfo.CFG_AREA_ID;
                        RQSTDT0.Rows.Add(dr0);

                        DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                        int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                        if (iCnt <= 0)
                        {
                            //출고 대상이 없습니다.
                            Util.MessageValidation("SFU3017", (result) =>
                            {
                                txtPopLotID.SelectAll();
                                txtPopLotID.Focus();
                            });

                            return;
                        }

                        // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        RQSTDT1.Columns.Add("PROCID", typeof(String));
                        RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = sLotid;
                        dr1["PROCID"] = "E7000";
                        dr1["WIPSTAT"] = "WAIT";
                        RQSTDT1.Rows.Add(dr1);

                        DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_2", "RQSTDT", "RSLTDT", RQSTDT1);
                        if (Prod_Result.Rows.Count == 0)
                        {
                            //재공 정보가 없습니다.
                            Util.MessageValidation("SFU1870", (result) =>
                            {
                                txtPopLotID.SelectAll();
                                txtPopLotID.Focus();
                            });

                            return;
                        }

                        //Binding.

                        bChanged = true;

                        DataTable dt = DataTableConverter.Convert(dgModifyList.ItemsSource);
                        #region [Scan Validation]
                        if (dt.Rows.Count > 0)
                        {
                            // 동일 Model.
                            if (!dt.Rows[0]["MODLID"].ToString().Equals(Prod_Result.Rows[0]["MODLID"].ToString()))
                            {
                                //동일한 제품만 작업 가능합니다.
                                Util.MessageValidation("SFU4338", (result) =>
                                {
                                    txtPopLotID.SelectAll();
                                    txtPopLotID.Focus();
                                });

                                return;
                            }
                            // 동일 제품
                            if (!dt.Rows[0]["PRODID"].ToString().Equals(Prod_Result.Rows[0]["PRODID"].ToString()))
                            {
                                //동일한 제품만 작업 가능합니다.
                                Util.MessageValidation("SFU4338", (result) =>
                                {
                                    txtPopLotID.SelectAll();
                                    txtPopLotID.Focus();
                                });

                                return;
                            }
                            // 동일 극성.
                            if (!dt.Rows[0]["ELECTYPE"].ToString().Equals(Prod_Result.Rows[0]["ELECTYPE"].ToString()))
                            {
                                //극성 정보가 다릅니다.
                                Util.MessageValidation("SFU2057", (result) =>
                                {
                                    txtPopLotID.SelectAll();
                                    txtPopLotID.Focus();
                                });
                                
                                return;
                            }
                        }

                        // 동일 Lot
                        DataRow[] dtRows = dt.Select("LOTID = '" + sLotid + "'");
                        if (dtRows?.Length > 0)
                        {
                            //동일한 LOT[%1]이 스캔되었습니다.
                            Util.MessageValidation("SFU4951", (result) =>
                            {
                                txtPopLotID.SelectAll();
                                txtPopLotID.Focus();
                            });
                            
                            return;
                        }
                        #endregion
                        DataRow dr = dt.NewRow();
                        if (dt.Columns.Contains("CHK"))
                            dr["CHK"] = true;
                        if (dt.Columns.Contains("LOTID"))
                            dr["LOTID"] = Prod_Result.Columns.Contains("LOTID") ? Util.NVC(Prod_Result.Rows[0]["LOTID"]) : "";
                        if (dt.Columns.Contains("ELECTYPE"))
                            dr["ELECTYPE"] = Prod_Result.Columns.Contains("ELECTYPE") ? Util.NVC(Prod_Result.Rows[0]["ELECTYPE"]) : "";
                        if (dt.Columns.Contains("ELECTYPE_NAME"))
                            dr["ELECTYPE_NAME"] = Prod_Result.Columns.Contains("ELECTYPE_NAME") ? Util.NVC(Prod_Result.Rows[0]["ELECTYPE_NAME"]) : "";
                        if (dt.Columns.Contains("MODLID"))
                            dr["MODLID"] = Prod_Result.Columns.Contains("MODLID") ? Util.NVC(Prod_Result.Rows[0]["MODLID"]) : "";
                        if (dt.Columns.Contains("PRODID"))
                            dr["PRODID"] = Prod_Result.Columns.Contains("PRODID") ? Util.NVC(Prod_Result.Rows[0]["PRODID"]) : "";
                        if (dt.Columns.Contains("PRJT_NAME"))
                            dr["PRJT_NAME"] = Prod_Result.Columns.Contains("PRJT_NAME") ? Util.NVC(Prod_Result.Rows[0]["PRJT_NAME"]) : "";
                        if (dt.Columns.Contains("WIPHOLD"))
                            dr["WIPHOLD"] = Prod_Result.Columns.Contains("WIPHOLD") ? Util.NVC(Prod_Result.Rows[0]["WIPHOLD"]) : "";
                        //if (dt.Columns.Contains("SHFT_ID"))
                        //    dr["SHFT_ID"] = "";
                        if (dt.Columns.Contains("NEW_YN"))
                            dr["NEW_YN"] = "Y";
                        
                        if (dt.Rows.Count < 1)
                        {
                            dr["MONTHSEQ"] = MONTH_SEQ;
                            dr["PACK_DAILY_SEQS"] = DAY_SEQ;

                            if (dt.Columns.Contains("PACK_DATE"))
                                dr["PACK_DATE"] = this.dtpdate.SelectedDateTime.ToString("yyyyMMdd");
                        }
                        else
                        {
                            if (dt.Columns.Contains("PACK_DATE"))
                                dr["PACK_DATE"] = Util.NVC(dt.Rows[dt.Rows.Count - 1]["PACK_DATE"]);
                            if (dt.Columns.Contains("MONTHSEQ"))
                                dr["MONTHSEQ"] = Util.NVC(dt.Rows[dt.Rows.Count - 1]["MONTHSEQ"]);
                            if (dt.Columns.Contains("PACK_DAILY_SEQS"))
                                dr["PACK_DAILY_SEQS"] = Util.NVC(dt.Rows[dt.Rows.Count - 1]["PACK_DAILY_SEQS"]);
                            if (dt.Columns.Contains("SHFT_ID"))
                                dr["SHFT_ID"] = Util.NVC(dt.Rows[dt.Rows.Count - 1]["SHFT_ID"]);
                        }

                        dt.Rows.Add(dr);

                        Util.GridSetData(dgModifyList, dt, FrameOperation);
                        
                        // Shift 콤보
                        (dgModifyList.Columns["SHFT_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtShft.Copy());
                        
                        dgModifyList.ScrollIntoView(dt.Rows.Count - 1, 0);

                        txtPopLotID.Text = "";
                        txtPopLotID.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex, (result) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            txtPopLotID.SelectAll();
                            txtPopLotID.Focus();
                        });

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        txtPopLotID.SelectAll();
                        txtPopLotID.Focus();
                    });
            }
        }

        private void txtPopLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    try
                    {
                        //loadingIndicator2.Visibility = Visibility.Visible;

                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                        foreach (string item in sPasteStrings)
                        {
                            KeyEventArgs args = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Enter);
                            this.Dispatcher.BeginInvoke(new Action(() => txtPopLotID_KeyDown(item, args)));                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //loadingIndicator2.Visibility = Visibility.Collapsed;
                        return;
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPopAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bChanged = true;

                DataTable dt = DataTableConverter.Convert(dgModifyList.ItemsSource);
                DataRow dr = dt.NewRow();
                if (dt.Columns.Contains("CHK"))
                    dr["CHK"] = true;

                dt.Rows.Add(dr);

                Util.GridSetData(dgModifyList, dt, FrameOperation, true);

                // Shift 콤보
                DataTable dtShift = new DataTable();
                dtShift.Columns.Add("CODE");
                dtShift.Columns.Add("NAME");

                dtShift.Rows.Add("N", "N");
                dtShift.Rows.Add("Y", "Y");

                (dgModifyList.Columns["SHFT_ID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtShift.Copy());

                dgModifyList.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPopRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        bChanged = true;
                        PopupSave(true);
                        
                        DataTable dt = DataTableConverter.Convert(dgModifyList.ItemsSource);
                        
                        for (int i = dgModifyList.Rows.Count - 1; i >= 0; i--)
                        {
                            if (!_Util.GetDataGridCheckValue(dgModifyList, "CHK", i)) continue;

                            dt.Rows.RemoveAt(i);
                            
                        }

                        Util.GridSetData(dgModifyList, dt, FrameOperation);
                    }
                });

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPopSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        bChanged = true;
                        PopupSave();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModifyList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                txtPopLotID.SelectAll();
                txtPopLotID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModifyList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        //Grid Data Binding 이용한 Background 색 변경
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e.Cell.Column.Name.Equals("ELECTYPE_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELECTYPE")).Equals("A"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            if ((e.Cell.Column.Name.Equals("OQCPASS_NAME") || e.Cell.Column.Name.Equals("WIPHOLD")) && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OQCPASS")).Equals("P") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OQCPASS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NEW_YN")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C9FFC3"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                            }

                            if ((e.Cell.Column.Name.Equals("OQCPASS_NAME") || e.Cell.Column.Name.Equals("WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NEW_YN")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C9FFC3"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                            }

                            if (!(e.Cell.Column.Name.Equals("OQCPASS_NAME") || e.Cell.Column.Name.Equals("WIPHOLD")) && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "NEW_YN")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C9FFC3"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModifyList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModifyList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgModifyList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPopClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.grdModify.Visibility = Visibility.Collapsed;

                if (bChanged)
                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                Util.gridClear(dgModifyList);
                bChanged = false;
            }
        }

        private void InitPopupControls()
        {
            try
            {
                DataTable dt = new DataTable();
                for (int i = 0; i < dgModifyList.Columns.Count; i++)
                {
                    dt.Columns.Add(dgModifyList.Columns[i].Name);
                }

                //DataRow dr = dt.NewRow();
                //dt.Rows.Add(dr);
                Util.GridSetData(dgModifyList, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                int iStdx = 0;
                int iEndx = 0;
                string sTmpKey = string.Empty;
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        iStdx = i;
                        sTmpKey = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MONTHSEQ"));

                        continue;
                    }

                    if (sTmpKey.Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MONTHSEQ"))))
                    {
                        iEndx = i;
                    }
                    else
                    {
                        if (iStdx < iEndx)
                        {
                            if (dg.Name.Equals("dgList"))
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PACK_DAILY_SEQS"].Index), dg.GetCell(iEndx, dg.Columns["PACK_DAILY_SEQS"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MONTHSEQ"].Index), dg.GetCell(iEndx, dg.Columns["MONTHSEQ"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["ELECTYPE_NAME"].Index), dg.GetCell(iEndx, dg.Columns["ELECTYPE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MODLID"].Index), dg.GetCell(iEndx, dg.Columns["MODLID"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["PRJT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["SHFT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["SHFT_NAME"].Index)));
                            }
                            else
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PACK_DAILY_SEQS"].Index), dg.GetCell(iEndx, dg.Columns["PACK_DAILY_SEQS"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MONTHSEQ"].Index), dg.GetCell(iEndx, dg.Columns["MONTHSEQ"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["ELECTYPE_NAME"].Index), dg.GetCell(iEndx, dg.Columns["ELECTYPE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MODLID"].Index), dg.GetCell(iEndx, dg.Columns["MODLID"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["PRJT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["SHFT_ID"].Index), dg.GetCell(iEndx, dg.Columns["SHFT_ID"].Index)));
                            }
                        }

                        iStdx = i;
                        sTmpKey = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "MONTHSEQ"));
                    }
                }

                if (iStdx < iEndx)
                {
                    if (dg.Name.Equals("dgList"))
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PACK_DAILY_SEQS"].Index), dg.GetCell(iEndx, dg.Columns["PACK_DAILY_SEQS"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MONTHSEQ"].Index), dg.GetCell(iEndx, dg.Columns["MONTHSEQ"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["ELECTYPE_NAME"].Index), dg.GetCell(iEndx, dg.Columns["ELECTYPE_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MODLID"].Index), dg.GetCell(iEndx, dg.Columns["MODLID"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["PRJT_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["SHFT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["SHFT_NAME"].Index)));
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PACK_DAILY_SEQS"].Index), dg.GetCell(iEndx, dg.Columns["PACK_DAILY_SEQS"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MONTHSEQ"].Index), dg.GetCell(iEndx, dg.Columns["MONTHSEQ"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["ELECTYPE_NAME"].Index), dg.GetCell(iEndx, dg.Columns["ELECTYPE_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["MODLID"].Index), dg.GetCell(iEndx, dg.Columns["MODLID"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["PRJT_NAME"].Index), dg.GetCell(iEndx, dg.Columns["PRJT_NAME"].Index)));
                        e.Merge(new DataGridCellsRange(dg.GetCell(iStdx, dg.Columns["SHFT_ID"].Index), dg.GetCell(iEndx, dg.Columns["SHFT_ID"].Index)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSeqInfo()
        {
            try
            {
                string sTmp = this.txtPopDate.Text.Replace("-", "");

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("AREAID", typeof(String));
                RQSTDT1.Columns.Add("PACK_DATE", typeof(String));
                RQSTDT1.Columns.Add("PACK_MONTH", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr1["PACK_DATE"] = sTmp;// this.dtpPopdate.SelectedDateTime.ToString("yyyyMMdd");
                dr1["PACK_MONTH"] = sTmp.Length > 2 ? sTmp.Substring(0, sTmp.Length - 2) : ""; //this.dtpPopdate.SelectedDateTime.ToString("yyyyMM");
                RQSTDT1.Rows.Add(dr1);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_WAIT_SEQNO", "RQSTDT", "RSLTDT", RQSTDT1);
                if (dtRslt.Rows.Count > 0)
                {
                    this.MONTH_SEQ = dtRslt.Rows[0]["MONTHSEQ"].ToString();
                    this.DAY_SEQ = dtRslt.Rows[0]["PACK_DAILY_SEQS"].ToString();
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);            
            }
        }

        private void PopupSave(bool bDelete = false)
        {
            try
            {
                loadingIndicator2.Visibility = Visibility.Visible;

                dgModifyList.EndEdit();
                
                DataTable inTable = new DataTable();
                
                //inTable.Columns.Add("SRCTYPE", typeof(string));
                //inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PACK_DATE", typeof(string));
                inTable.Columns.Add("PACK_DAILY_SEQS", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("SHFT_ID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIPTO_ID", typeof(string));

                DataRow newRow = null;
                string sShft = string.Empty;

                for (int i = 0; i < dgModifyList.Rows.Count; i++)
                {
                    if (i == 0)
                        sShft = Util.NVC(DataTableConverter.GetValue(dgModifyList.Rows[i].DataItem, "SHFT_ID"));

                    if (!_Util.GetDataGridCheckValue(dgModifyList, "CHK", i)) continue;

                    newRow = inTable.NewRow();
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["PACK_DATE"] = Util.NVC(DataTableConverter.GetValue(dgModifyList.Rows[i].DataItem, "PACK_DATE")); //this.dtpPopdate.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["PACK_DAILY_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgModifyList.Rows[i].DataItem, "PACK_DAILY_SEQS"));
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgModifyList.Rows[i].DataItem, "LOTID"));
                    newRow["SHFT_ID"] = sShft;
                    newRow["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgModifyList.Rows[i].DataItem, "NOTE"));
                    newRow["DEL_FLAG"] = bDelete ? "Y" : "N";
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["SHIPTO_ID"] = cboTransLoc.SelectedValue.ToString();
                    
                    inTable.Rows.Add(newRow);
                }
                
                new ClientProxy().ExecuteServiceSync("DA_PRD_MERGE_TB_SFC_PACK_WAIT_ELTR_CONF", "INDATA", "", inTable);

                if (!bDelete)
                {
                    Util.MessageInfo("SFU1275");

                    this.Dispatcher.BeginInvoke(new Action(() => btnPopClose_Click(null, null)));            
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator2.Visibility = Visibility.Hidden;
            }
        }

        private void dtpPopdate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetSeqInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ProcessPackOut()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                btnPackOut.IsEnabled = false;
                btnSearch.IsEnabled = false;
                btnModify.IsEnabled = false;

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1663"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dtSrc = DataTableConverter.Convert(dgList.ItemsSource);
                DataTable dtTgt = dtSrc.Clone();
                foreach (DataRow drSrc in dtSrc.Rows)
                {
                    if (drSrc["CHK"].ToString().Equals("1") || drSrc["CHK"].ToString().ToUpper().Equals("TRUE"))
                    {
                        dtTgt.ImportRow(drSrc);
                    }
                }
                if (string.IsNullOrEmpty(dtTgt.Rows[0]["SHIPTO_ID"].ToString()))
                {
                    Util.MessageInfo("SFU4999");
                    return;
                }
                //this.PackingOutEvent(dtTgt, cboTransLoc?.SelectedValue?.ToString(), cboPackWay?.SelectedValue?.ToString());
                this.PackingOutEvent(dtTgt, dtTgt.Rows[0]["SHIPTO_ID"].ToString(), cboPackWay?.SelectedValue?.ToString());
                
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private Dictionary<string, string> getShipCompany(string sProdID)
        {
            try
            {
                Dictionary<string, string> sCompany = new Dictionary<string, string>();

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRODID"] = sProdID;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SMPLG_SHIP_COMPANY", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return new Dictionary<string, string> { { string.Empty, string.Empty } };

                DataTable ShipTo = new DataTable("INDATA");
                ShipTo.Columns.Add("SHIPTO_ID", typeof(string));

                DataRow ShipToIndata = ShipTo.NewRow();
                ShipToIndata["SHIPTO_ID"] = cboTransLoc.SelectedValue.ToString();

                ShipTo.Rows.Add(ShipToIndata);

                DataTable dtShipTo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", ShipTo);

                if (dtShipTo == null || dtShipTo.Rows.Count == 0)
                    return new Dictionary<string, string> { { string.Empty, string.Empty } };

                DataRow[] dr = dtResult.Select("COMPANY_CODE = '" + dtShipTo.Rows[0]["COMPANY_CODE"].ToString() + "'");

                if (dr.Length == 0 || dr == null)
                {
                    var ShipCompany = new List<string>();
                    foreach (DataRow dRow in dtResult.Rows)
                    {
                        ShipCompany.Add(Util.NVC(dRow["COMPANY_CODE"]));
                    }
                    sCompany.Add(dtShipTo.Rows[0]["COMPANY_CODE"].ToString(), string.Join(",", ShipCompany));
                }
                return sCompany;
            }
            catch (Exception ex) { }

            return new Dictionary<string, string> { { string.Empty, string.Empty } };
        }
    }
}
