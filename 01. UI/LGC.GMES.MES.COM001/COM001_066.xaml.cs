/*************************************************************************************
 Created Date : 2017.01.21
      Creator : 이진선
   Decription : 선분산출고
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{

    /// <summary>
    /// BOX001_001.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class COM001_066 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        double sum;
        double totalqty2;

        #region 

    
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_066()
        {
           InitializeComponent();
            this.Loaded += UserControl_Loaded;

        }

        #endregion

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            initCombo();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void initCombo()
        {
            //포장출고
            string[] sFilter = { "PD", LoginInfo.CFG_EQSG_ID };
            combo.SetCombo(cboPreMixerProduct, CommonCombo.ComboStatus.ALL, sCase: "cboMaterialCode", sFilter: sFilter);

            string[] sFilter2 = { "", "PRDMIX_OUT_LINE" };
            combo.SetCombo(cboOutLine, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODES");

            //출고이력조회
            string[] sFilter3 = { Process.PRE_MIXING_PACK };
            combo.SetCombo(cboOutArea, CommonCombo.ComboStatus.NONE, sCase: "cboAreaSRS", sFilter: sFilter3);
            combo.SetCombo(cboOutEquipmentSegment, CommonCombo.ComboStatus.NONE, sCase: "cboEquipmentSegmentSRS", sFilter:sFilter3);

          //  string[] sFilter5 = { "PD", null };
            combo.SetCombo(cboOutModel, CommonCombo.ComboStatus.ALL, sCase: "cboPrdMixProdid"); //출고된 모델만 나오게

            //반품입고
            combo.SetCombo(cboReturnArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "cboOutLine");

            //반품이력조회
            combo.SetCombo(cboReturnLocation, CommonCombo.ComboStatus.ALL, sCase: "cboOutLine", sFilter:sFilter2);


        }
        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
            dg.BottomRows[0].Visibility = Visibility.Collapsed;
        }
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        #endregion



        #region 선분산 출고처리
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        private void btnProductRefresh_Click(object sender, RoutedEventArgs e)
        {
            string[] sFilter = { "PD", LoginInfo.CFG_EQSG_ID };
            combo.SetCombo(cboPreMixerProduct, CommonCombo.ComboStatus.ALL, sCase: "cboMaterialCode", sFilter: sFilter);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgPalletList);
            dgBatchList.ItemsSource = null;
            SearchData();
        }
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void dgPalletList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        private void dgPalletList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgPalletList.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg.GetRowCount() == 0)
                {
                    if (e.Row.Type == DataGridRowType.Bottom)
                    {
                        e.Row.Visibility = Visibility.Collapsed;
                    }
                }
            }));
        }
        //private void dgPalletList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        //{
        //    C1DataGrid dg = sender as C1DataGrid;
        //    if (dg.GetRowCount() == 0)
        //    {
        //        if (e.Row.Type == DataGridRowType.Bottom)
        //        {
        //            e.Row.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //}
        private void dgPalletList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            dgPalletList.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    if (cb.IsChecked == true)
                    {
                        GetBacthIDList(index);
                    }
                    else if (cb.IsChecked == false)
                    {
                        string _BOXID = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[index].DataItem, "BOXID"));
                        DataTable dt = DataTableConverter.Convert(dgBatchList.ItemsSource);
                        if (dt.Rows.Count != 0)
                        {
                            dgBatchList.ItemsSource = dt.Select("BOXID <> '" + _BOXID + "'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select("BOXID <> '" + _BOXID + "'").CopyToDataTable());
                        }
                    }

                        SumTotalQty();

                    }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

            }));
        }
        private void btnPreMixerOut_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }

            //포장출고하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2802"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string[] toInfo = Convert.ToString(cboOutLine.SelectedValue).Split('|');

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("ACTDTTM", typeof(DateTime));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("EQSGID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["ACTDTTM"] = PackOutDate.SelectedDateTime;
                        row["AREAID"] = toInfo[0];
                        row["EQSGID"] = toInfo[1];
                        row["NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("ACTQTY", typeof(decimal));

                        for (int j = 0; j < dgPalletList.GetRowCount(); j++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "CHK")).Equals("True"))
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "LOTID"));
                                row["ACTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "WIPQTY"));
                                inLot.Rows.Add(row);
                            }

                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_TANKLOT_PM", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_SEND_TANKLOT_PM", bizException.Message, bizException.ToString());
                                    return;
                                }

                                Util.AlertInfo("SFU1931");  //출고완료
                                rtxRemark.Document.Blocks.Clear();
                                SearchData();

                                initCombo();
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);

                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }

                }
            });
        }

        #region[Method]

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();

                row["WIPSTAT"] = Wip_State.WAIT;
                row["PROCID"] = Process.PRE_MIXING_PACK;//"E0700"; // 선분산 믹서 공정에 추가 
                row["PRODID"] = (Convert.ToString(cboPreMixerProduct.SelectedValue).Equals("") || !txtOutLOTID.Text.Equals("")) ? null : Convert.ToString(cboPreMixerProduct.SelectedValue);
                row["LOTID"] = txtOutLOTID.Text.Equals("") ? null : txtOutLOTID.Text;
                dt.Rows.Add(row);


                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_PRE_MIXER", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_WIP_PRE_MIXER", searchException.Message, searchException.ToString());
                            return;
                        }

                        Util.GridSetData(dgPalletList, searchResult, FrameOperation);
                        dgPalletList.BottomRows[0].Visibility = Visibility.Visible;

                        dgBatchList.ItemsSource = null;

                        DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                        DataGridAggregateSum dagsum = new DataGridAggregateSum();
                        dagsum.ResultTemplate = dgPalletList.Resources["ResultTemplate"] as DataTemplate;
                        dac.Add(dagsum);
                        DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["WIPQTY"], dac);
                        txtOutLOTID.Text = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
            );

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            
        }
        private void GetBacthIDList(int index)
        {
            try
            {
                string _BOXID = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[index].DataItem, "BOXID"));

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("BOXID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["BOXID"] = _BOXID;

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTRACE_BATCHID", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count == 0)
                {
                    return;
                }

                DataTable BatchListDT = DataTableConverter.Convert(dgBatchList.ItemsSource);

                if (dgBatchList.ItemsSource == null)
                {
                    BatchListDT.Columns.Add("BOXID", typeof(string));
                    BatchListDT.Columns.Add("FROM_LOTID", typeof(string));
                }

                //PancakeList에 있는지 확인
                for (int i = 0; i < dgBatchList.GetRowCount(); i++)
                {
                    if (_BOXID.Equals(Util.NVC(DataTableConverter.GetValue(dgBatchList.Rows[i].DataItem, "BOXID"))))
                    {
                        Util.Alert("SFU2809");  //배치가 이미 존재합니다.
                        return;
                    }
                }

                DataRow batchRow = null;
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    batchRow = BatchListDT.NewRow();
                    batchRow["BOXID"] = result.Rows[i]["BOXID"].ToString();
                    batchRow["FROM_LOTID"] = result.Rows[i]["FROM_LOTID"].ToString();

                    BatchListDT.Rows.Add(batchRow);
                }

                Util.GridSetData(dgBatchList, BatchListDT, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {

            dgBatchList.ItemsSource = null;

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[i].DataItem, "CHK", true);
                    GetBacthIDList(i);
                }
            }

            SumTotalQty();
        }
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[i].DataItem, "CHK", false);
                }

                //ClearDataGrid(dgBatchList);
                dgBatchList.ItemsSource = null;
            }
        }
        void SumTotalQty()
        {
            sum = 0;
            totalqty2 = 0;

            for (int i = 0; i < dgPalletList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    sum += Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "WIPQTY")));
                    totalqty2 += Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "WIPQTY2")).Equals("") ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "WIPQTY2")));
                }
            }
        }
        private bool Validation()
        {
            int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
            if (firstIndex == -1)
            {
                Util.Alert("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            for (int i = 0; i < dgPalletList.GetRowCount(); i++)
            {
                if (firstIndex != i)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[firstIndex].DataItem, "PRODID")).Equals(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "PRODID"))))
                        {
                            Util.Alert("SFU1896");  //제품코드가 같은 Pallet만 선택해주세요
                            return false;
                        }
                    }

                }
            }

            return true;
        }
        #endregion

       

     

       

        #endregion


        #region 출고 이력 조회

        private void btnOutSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            GetOutData();
        }
        private void txtOutLotID_KeyDown(object sender, KeyEventArgs e)
        {

            if (sender == null) return;
            if (e.Key == Key.Enter)
            {
                GetOutData();
            }
        }

        private void GetOutData()
        {
            try
            {
                dgBoxOut.ItemsSource = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("FROM_EQSGID", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                if (txtOutLotID.Text.Equals(""))
                {
                    row["FROM_EQSGID"] = Convert.ToString(cboOutEquipmentSegment.SelectedValue);
                    row["LOTID"] = null;
                    row["FROM_DATE"] = ldpDatePickerFrom.SelectedDateTime.ToShortDateString();
                    row["TO_DATE"] = ldpDatePickerTo.SelectedDateTime.ToShortDateString();
                    row["PRODID"] = cboOutModel.SelectedValue.ToString().Equals("") ? null : cboOutModel.SelectedValue.ToString();
                }
                else
                {
                    row["FROM_EQSGID"] = null;
                    row["LOTID"] = txtOutLotID.Text.Equals("") ? null : txtOutLotID.Text;
                    row["FROM_DATE"] = null;
                    row["TO_DATE"] = null;
                    row["PRODID"] = null;
                }

                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_PREMIXER_SEND_HIST", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_BOX_PREMIXER_SEND_HIST", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.GridSetData(dgBoxOut, bizResult, FrameOperation, false);

                        txtOutLotID.Text = "";

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

               // DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_PREMIXER_OUT_V2", "RQSTDT", "RSLTDT", dt);
               // Util.GridSetData(dgBoxOut, result, FrameOperation, false);

                //txtOutLotID.Text = "";

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgBoxOut_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            if (dgBoxOut.ItemsSource == null) return;
            if (dgBoxOut.GetRowCount() == 0) return;

            string MOVE_ORD_ID = Util.NVC(DataTableConverter.GetValue(dgBoxOut.Rows[0].DataItem, "MOVE_ORD_ID"));

            if (MOVE_ORD_ID.Equals("")) return;

            int idx = 0;

            for (int i = 0; i < dgBoxOut.GetRowCount(); i++)
            {
                if (i >= idx)
                {
                    if (!MOVE_ORD_ID.Equals(Util.NVC(DataTableConverter.GetValue(dgBoxOut.Rows[i].DataItem, "MOVE_ORD_ID"))))
                    {
                        MOVE_ORD_ID = Util.NVC(DataTableConverter.GetValue(dgBoxOut.Rows[i].DataItem, "MOVE_ORD_ID"));

                        for (int j = 1; j < 7; j++)
                        {
                            e.Merge(new DataGridCellsRange(dgBoxOut.GetCell(idx, j), dgBoxOut.GetCell(i - 1, j)));
                        }

                        idx = i;
                    }
                }


            }
        }

        #endregion


        #region 반품
        private void txtReturnLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender == null) return;
            if (e.Key == Key.Enter)
            {
                if (txtReturnLotID.Text.Equals(""))
                {
                    Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                    return;
                }

                GetReturnInfo();
            }
        }
        private void cboReturnArea_SelectedIndexChanged(object sender, C1.WPF.PropertyChangedEventArgs<int> e)
        {
            cboReturnArea.SelectedValue = "A011|E2|210A|A0075";
            cboReturnArea.IsEnabled = false;
        }
        private void btnReturnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (txtReturnLotID.Text.Equals(""))
            {
                Util.Alert("SFU1366");  //LOT ID를 입력해주세요
                return;
            }

            GetReturnInfo();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReturnLot, "CHK") == -1)
            {
                Util.Alert("SFU1661");  //선택 된 LOT이 없습니다.
                return;
            }
            if (dgReturnLot.ItemsSource == null)
            {
                return;
            }

            //반품 입고 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2808"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string[] returnArea = Convert.ToString(cboReturnArea.SelectedValue).Split('|');

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("ACTDTTM", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["ACTDTTM"] = null;
                        row["AREAID"] = returnArea[1];
                        row["NOTE"] = new TextRange(rtxReturnRemark.Document.ContentStart, rtxReturnRemark.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;
                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("ACTQTY", typeof(decimal));

                        for (int i = 0; i < dgReturnLot.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "LOTID"));
                                row["ACTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "WIPQTY"));
                                indataSet.Tables["INLOT"].Rows.Add(row);
                            }
                        }

                                try
                                {
                                    ShowLoadingIndicator();

                                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_TANKLOT_PM", "INDATA,INLOT", null, (bizResult, bizException) =>
                                    {
                                        try
                                        {
                                            if (bizException != null)
                                            {
                                                Util.AlertByBiz("BR_PRD_REG_RETURN_TANKLOT_PM", bizException.Message, bizException.ToString());
                                                return;
                                            }

                                            Util.AlertInfo("SFU1552");  //반품 입고 처리 되었습니다.

                                            DataTable dt = DataTableConverter.Convert(dgReturnLot.ItemsSource).Select("CHK <> 'True'").Count() == 0 ? null : DataTableConverter.Convert(dgReturnLot.ItemsSource).Select("CHK <> 'True'").CopyToDataTable();
                                            dgReturnLot.ItemsSource = dt == null ? null : DataTableConverter.Convert(dt);


                                        }
                                        catch (Exception ex)
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                        }
                                        finally
                                        {
                                            HiddenLoadingIndicator();
                                        }
                                    }, indataSet);

                                }
                                catch (Exception ex)
                                {
                                    //반품입고
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2867"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                }
                            
                        
                        
                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }
                }
            });
        }
        private void btnLotDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            DataTable dt = DataTableConverter.Convert(dgReturnLot.ItemsSource);
            dt.Rows[index].Delete();

            dgReturnLot.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void GetReturnInfo()
        {
            try
            {
                DataTable dt = null;
                DataRow row = null;

                DataTable returnTable = DataTableConverter.Convert(dgReturnLot.ItemsSource);
                DataRow returnRow = null;

                

                if (dgReturnLot.ItemsSource == null)
                {
                    returnTable.Columns.Add("CHK", typeof(bool));
                    returnTable.Columns.Add("PALLETID", typeof(string));
                    returnTable.Columns.Add("AREANAME", typeof(string));
                    returnTable.Columns.Add("LOTID", typeof(string));
                    returnTable.Columns.Add("PRODID", typeof(string));
                    returnTable.Columns.Add("PRODNAME", typeof(string));
                    returnTable.Columns.Add("FROM_SHOPID", typeof(string));
                    returnTable.Columns.Add("FROM_AREAID", typeof(string));
                    returnTable.Columns.Add("FROM_EQSGID", typeof(string));
                    returnTable.Columns.Add("FROM_PROCID", typeof(string));
                    returnTable.Columns.Add("TO_SHOPID", typeof(string));
                    returnTable.Columns.Add("TO_AREAID", typeof(string));
                    returnTable.Columns.Add("TO_PROCID", typeof(string));
                    returnTable.Columns.Add("TO_EQSGID", typeof(string));
                    returnTable.Columns.Add("TO_SLOC_ID", typeof(string));
                    returnTable.Columns.Add("MOVE_STRT_DTTM", typeof(DateTime));
                    returnTable.Columns.Add("MOVE_END_DTTM", typeof(DateTime));
                    returnTable.Columns.Add("WIPQTY", typeof(decimal));
                    returnTable.Columns.Add("WIPHOLD", typeof(string));
                    returnTable.Columns.Add("INSUSER", typeof(string));
                }

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["LOTID"] = txtReturnLotID.Text;
                dt.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_CHK_RETURN_TANKLOT_PM", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.AlertByBiz("BR_PRD_CHK_RETURN_TANKLOT_PM", searchException.Message, searchException.ToString());
                            return;
                        }

                        if (searchResult?.Rows.Count == 0)
                        {
                            FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); //조회된 Data가 없습니다.
                            return;
                        }

                        if (returnTable.Select("LOTID = '" + Convert.ToString(searchResult.Rows[0]["LOTID"]) + "'").Count() != 0)
                        {
                            Util.Alert("SFU2014");  //해당 LOT이 이미 존재합니다.
                            return;
                        }

                        for (int i = 0; i < searchResult.Rows.Count; i++)
                        {
                            returnRow = returnTable.NewRow();
                            returnRow["CHK"] = searchResult.Rows[i]["CHK"];
                            returnRow["PALLETID"] = Convert.ToString(searchResult.Rows[i]["PALLETID"]);
                            returnRow["AREANAME"] = Convert.ToString(searchResult.Rows[i]["AREANAME"]);
                            returnRow["LOTID"] = Convert.ToString(searchResult.Rows[i]["LOTID"]);
                            returnRow["PRODID"] = Convert.ToString(searchResult.Rows[i]["PRODID"]);
                            returnRow["PRODNAME"] = Convert.ToString(searchResult.Rows[i]["PRODNAME"]);
                            returnRow["FROM_SHOPID"] = Convert.ToString(searchResult.Rows[i]["FROM_SHOPID"]);
                            returnRow["FROM_AREAID"] = Convert.ToString(searchResult.Rows[i]["FROM_AREAID"]);
                            returnRow["FROM_EQSGID"] = Convert.ToString(searchResult.Rows[i]["FROM_EQSGID"]);
                            returnRow["FROM_PROCID"] = Convert.ToString(searchResult.Rows[i]["FROM_PROCID"]);
                            returnRow["TO_SHOPID"] = Convert.ToString(searchResult.Rows[i]["TO_SHOPID"]);
                            returnRow["TO_AREAID"] = Convert.ToString(searchResult.Rows[i]["TO_AREAID"]);
                            returnRow["TO_PROCID"] = Convert.ToString(searchResult.Rows[i]["TO_PROCID"]);
                            returnRow["TO_EQSGID"] = Convert.ToString(searchResult.Rows[i]["TO_EQSGID"]);
                            returnRow["TO_SLOC_ID"] = Convert.ToString(searchResult.Rows[i]["TO_SLOC_ID"]);
                            returnRow["MOVE_STRT_DTTM"] = Util.DatetoString(searchResult.Rows[i]["MOVE_STRT_DTTM"]);
                            returnRow["MOVE_END_DTTM"] = Util.DatetoString(searchResult.Rows[i]["MOVE_END_DTTM"]);
                            returnRow["WIPQTY"] = Convert.ToString(searchResult.Rows[i]["WIPQTY"]);
                            returnRow["WIPHOLD"] = Convert.ToString(searchResult.Rows[i]["WIPHOLD"]);
                            returnRow["INSUSER"] = Convert.ToString(searchResult.Rows[i]["INSUSER"]);

                            returnTable.Rows.Add(returnRow);
                        }

                        for(int i = 0; i < returnTable.Rows.Count; i++)
                        {
                            if(returnTable.Rows[0]["PRODID"].ToString() != returnTable.Rows[i]["PRODID"].ToString())
                            {
                                Util.MessageInfo("SFU1893");
                                return;
                            }
                        }

                        Util.GridSetData(dgReturnLot, returnTable, FrameOperation);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        txtReturnLotID.Text = string.Empty;
                    }
                }
        );






            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #endregion


        #region 반품 이력 조회

        private void btnReturnInfoSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] returnLoc = Convert.ToString(cboReturnLocation.SelectedValue).Equals("") ? null : Convert.ToString(cboReturnLocation.SelectedValue).Split('|');
         

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("TO_EQSGID", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));


                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["TO_EQSGID"] = returnLoc == null ? null : returnLoc[1];
                row["FROM_DATE"] = ldpDatePickerReturnFrom.SelectedDateTime.ToShortDateString();
                row["TO_DATE"] = ldpDatePickerReturnTo.SelectedDateTime.ToShortDateString();


                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_PREMIXER_RETURN_HIST", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.AlertByBiz(" DA_PRD_SEL_BOX_PREMIXER_RETURN_HIST", searchException.Message, searchException.ToString());
                            return;
                        }

                        Util.GridSetData(dgReturnInfo, searchResult, FrameOperation);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        txtReturnLotID.Text = string.Empty;
                    }
                }
        );


            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }







        #endregion

     
    }
}