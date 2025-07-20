using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_144.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_144 : UserControl, IWorkArea
    {
        string _boxID = string.Empty;

        int _idx = 99;
        int _NodataCnt = 0;
        int _NoinputCnt = 0;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        bool _Chk = false;

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public MessageBoxResult DialogResult { get; private set; }

        public COM001_144()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCmb();
        }
        private void InitCmb()
        {
            CommonCombo _combo = new CommonCombo();
            // 동
            //C1ComboBox[] cboInputAreaChild = { cboLine };
            String[] sFiltercboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_AREATYPE");


            //라인
            //C1ComboBox[] cboInputEquipmentSegmentParent = { cboArea };
            //_combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbParent: cboInputEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");
            SetCboEQSG();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment.SelectedItemsToString == "")
                {
                    Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }

                Serarch_List();
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed; 
            }
           
        }

        private void Serarch_List()
        {
            //rdoALL
            string sType = null;

             if (rdoALL_MY.IsChecked == true)
            {
                sType = "I";
            }
            else if(rdoALL_MN.IsChecked == true)
            {
                sType = "N";
            }

            loadingIndicator.Visibility = Visibility.Visible;

            Util.gridClear(dgNodate);
            Util.gridClear(dgnoInputLot);

            DataSet dsResult = null;

            DataSet dsInput = new DataSet();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("OCCR_DTTM_FROM", typeof(DateTime));
            RQSTDT.Columns.Add("OCCR_DTTM_TO", typeof(DateTime));
            RQSTDT.Columns.Add("NODATA_STAT_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = cboEquipmentSegment.SelectedItemsToString; // //"P7Q04";
            dr["OCCR_DTTM_FROM"] = dtpFDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
            dr["OCCR_DTTM_TO"] = dtpTDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
            dr["NODATA_STAT_CODE"] = sType;

            RQSTDT.Rows.Add(dr);

            dsInput.Tables.Add(RQSTDT);

            dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_GET_NODATA_INPUT_LIST", "RQSTDT", "RSLT_BOX_LIST,RSLT_LOT_LIST", dsInput, null);

            loadingIndicator.Visibility = Visibility.Collapsed;

            if (dsResult != null && dsResult.Tables.Count > 0)
            {
                if ((dsResult.Tables.IndexOf("RSLT_LOT_LIST") > -1) && dsResult.Tables["RSLT_LOT_LIST"].Rows.Count > 0)
                {
                    Util.GridSetData(dgnoInputLot, dsResult.Tables["RSLT_LOT_LIST"], FrameOperation, false);
                    _NoinputCnt = dsResult.Tables["RSLT_LOT_LIST"].Rows.Count;
                    Util.SetTextBlockText_DataGridRowCount(tbistCount, Util.NVC(_NoinputCnt));
                       
                }

                if ((dsResult.Tables.IndexOf("RSLT_BOX_LIST") > -1) && dsResult.Tables["RSLT_BOX_LIST"].Rows.Count > 0)
                {
                    Util.GridSetData(dgNodate, dsResult.Tables["RSLT_BOX_LIST"], FrameOperation, false);
                     _NodataCnt = dsResult.Tables["RSLT_BOX_LIST"].Rows.Count;
                    Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(_NodataCnt));
                }

            }

            if(_NodataCnt != _NoinputCnt)
            {
                btnAllCheck.Visibility = Visibility.Collapsed;
                _Chk = false;
            }
            else
            {
                btnAllCheck.Visibility = Visibility.Visible;
            }
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
         {
            if (_Chk)
            {
                return;
            }

            if (sender == null)
                return;
            CheckBox ckBox = sender as CheckBox;

            DataRow[] drInfo = GetChecked(ref dgNodate, "CHK");

            _NodataCnt = drInfo.Count();

            //if (_NodataCnt != (_NoinputCnt + 1))
            //{
            //    ckBox.IsChecked = false;
            //    return;
            //}

            if ((bool)ckBox.IsChecked)
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)ckBox.Parent).Row.Index;
                fn_Search_Box(idx);

                _idx = idx;
            }
            
        }

        private void fn_Search_Box(int idx)
        {
           string BoxID =  Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[idx].DataItem, "BOXID"));
           //if(dgnoInputLot.ItemsSource != null)
           if(dgnoInputLot.Rows.Count > 0)
            {
                DataTable dt = DataTableConverter.Convert(dgnoInputLot.ItemsSource);

                DataTable sortDt = dt.AsEnumerable().OrderBy(x => x["BOXID"].ToString() == BoxID ? 0 : 1).ThenBy(x => x["BOXID"]).CopyToDataTable();
                Util.GridSetData(dgnoInputLot, sortDt, FrameOperation);

                _boxID = BoxID;
            }
        }

        private void fn_Search_Lot(int idx)
        {
            string LotID = Util.NVC(DataTableConverter.GetValue(dgnoInputLot.Rows[idx].DataItem, "SUBLOTID"));
            // if(dgnoInputLot.ItemsSource != null)
            for (int i = 0; i < dgNodate.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "INFO_MAPP_LOTID")).ToString() == LotID)
                {
                   //Util.Alert("SFU1223");  //라인을 선택하세요.
                    return;
                }
            }

            if (dgNodate.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[_idx].DataItem, "INFO_MAPP_LOTID")).ToString() == "")
                {
                    DataTableConverter.SetValue(dgNodate.Rows[_idx].DataItem, "INFO_MAPP_LOTID", LotID);
                }
                else
                {
                    string MppingLot = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[_idx].DataItem, "INFO_MAPP_LOTID"));

                    DataTableConverter.SetValue(dgNodate.Rows[_idx].DataItem, "INFO_MAPP_LOTID", LotID);

                    for (int i = 0; i < dgnoInputLot.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgnoInputLot.Rows[i].DataItem, "SUBLOTID")).ToString() == MppingLot)
                        {
                            DataTableConverter.SetValue(dgnoInputLot.Rows[i].DataItem, "CHK", false);
                        }
                    }

                }
                //DataRow[] drInfo2 = GetChecked(ref dgnoInputLot, "CHK");
                //_NoinputCnt = drInfo2.Count();
            }
        }


        private void dgnoInputLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgnoInputLot.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXID")) == _boxID)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                }
                else
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }


            }));
        }

        private void btnChoiceMapping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SetMapping();
                        }
                    });
 

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void SetMapping()
        {
            try
            {
                //int NoDataIndex = Util.gridFindDataRow(ref dgNodate, "CHK", "True", false);
                //int NoInputIndex = Util.gridFindDataRow(ref dgnoInputLot, "CHK", "True", false);

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("NODATA_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("INFO_MAPP_LOTID", typeof(string));
                RQSTDT.Columns.Add("NODATA_PRCS_CODE", typeof(string));

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LOTID", typeof(string));
                //RQSTDT.Columns.Add("NODATA_STAT_CODE", typeof(string));
                ////RQSTDT.Columns.Add("NODATA_STAT_UPDDTTM", typeof(string));
                //RQSTDT.Columns.Add("INFO_MAPP_LOTID", typeof(string));
                ////RQSTDT.Columns.Add("MOVE_AREAID", typeof(string));
                //RQSTDT.Columns.Add("NODATA_PRCS_CODE", typeof(string));
                ////RQSTDT.Columns.Add("NODATA_PRCS_UPDDTTM", typeof(string));

                for (int i = 0; i < dgNodate.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "INFO_MAPP_LOTID"))!="")
                        {
                            // DataRow dr = RQSTDT.NewRow();
                            DataRow row = RQSTDT.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "LOTID"));
                            row["NODATA_STAT_CODE"] = "I";
                            //dr["NODATA_STAT_UPDDTTM"] = 
                            row["INFO_MAPP_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "INFO_MAPP_LOTID"));
                            //dr["MOVE_AREAID"] = "";
                            row["NODATA_PRCS_CODE"] = "C";
                            //dr["NODATA_PRCS_UPDDTTM"] = 
                            indataSet.Tables["RQSTDT"].Rows.Add(row);
                        }
                    }
                }
                        
                //  DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_RTLS_UPD_NODATA_STAT", "RQSTDT", "", RQSTDT);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_RTLS_UPD_NODATA_STAT", "RQSTDT", "", (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");

                    this.DialogResult = MessageBoxResult.OK;

                }, indataSet);

                Serarch_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnchoiseCancel_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    ReleaseMapping();
                }
            });
        }

        private void ReleaseMapping()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("NODATA_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("NODATA_PRCS_CODE", typeof(string));

                for (int i = 0; i < dgNodate.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow row = RQSTDT.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[i].DataItem, "LOTID"));
                        row["NODATA_STAT_CODE"] = "N";
                        row["NODATA_PRCS_CODE"] = "D";
                        indataSet.Tables["RQSTDT"].Rows.Add(row);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_RTLS_UPD_NODATA_STAT", "RQSTDT", "", (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");

                    this.DialogResult = MessageBoxResult.OK;

                }, indataSet);

                Serarch_List();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            //try
            //{
            //    int NoDataIndex = Util.gridFindDataRow(ref dgNodate, "CHK", "True", false);

            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LOTID", typeof(string));
            //    RQSTDT.Columns.Add("NODATA_STAT_CODE", typeof(string));
            //    //RQSTDT.Columns.Add("NODATA_STAT_UPDDTTM", typeof(string));
            //    //RQSTDT.Columns.Add("INFO_MAPP_LOTID", typeof(string));
            //    //RQSTDT.Columns.Add("MOVE_AREAID", typeof(string));
            //    RQSTDT.Columns.Add("NODATA_PRCS_CODE", typeof(string));
            //    //RQSTDT.Columns.Add("NODATA_PRCS_UPDDTTM", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[NoDataIndex].DataItem, "LOTID"));
            //    dr["NODATA_STAT_CODE"] = "N";
            //    //dr["NODATA_STAT_UPDDTTM"] = 
            //    //dr["INFO_MAPP_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgnoInputLot.Rows[NoInputIndex].DataItem, "SUBLOTID"));
            //    //dr["MOVE_AREAID"] = "";
            //    dr["NODATA_PRCS_CODE"] = "D";
            //    //dr["NODATA_PRCS_UPDDTTM"] = 
            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_RTLS_UPD_NODATA_STAT", "RQSTDT", "", RQSTDT);

            //    //cboLine.ItemsSource = DataTableConverter.Convert(dtResult);

            //    Util.MessageInfo("SFU3532");

            //    Serarch_List();
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        public static DataRow[] GetChecked(ref C1DataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = 'True'");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (_Chk)
            {
                return;
            }
            if (_idx.ToString() == "99")
            {
                return;
            }

           if (sender == null)
                return;
            CheckBox ckBox = sender as CheckBox;

            DataRow[] drInfo = GetChecked(ref dgnoInputLot, "CHK");

           //if(_NoinputCnt != drInfo.Count())
           // {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)ckBox.Parent).Row.Index;
                fn_Search_Lot(idx);
            //}

           //if(_NodataCnt == drInfo.Count())
           // {
           //     int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)ckBox.Parent).Row.Index;
           //     fn_Search_Lot(idx);
           //     _NoinputCnt = drInfo.Count();
           // }

        }

        private void dgNodate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgNodate.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INFO_MAPP_LOTID")) == "")
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
                else
                {
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                }


            }));
        }

        private void SetCboEQSG()
        {
            try
            {
                string sSelectedValue = cboEquipmentSegment.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboEquipmentSegment.Check(i);
                                break;
                            }
                        }
                    }
                    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                    {
                        cboEquipmentSegment.Check(i);
                        break;
                    }
                }
              }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetCboEQSG();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_Chk)
            {
                return;
            }

            if (sender == null)
                return;
            CheckBox ckBox = sender as CheckBox;

            DataRow[] drInfo = GetChecked(ref dgNodate, "CHK");

            _NodataCnt = drInfo.Count();

            if (!(bool)ckBox.IsChecked)
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)ckBox.Parent).Row.Index;
                string MppingLot = Util.NVC(DataTableConverter.GetValue(dgNodate.Rows[idx].DataItem, "INFO_MAPP_LOTID"));

                DataTableConverter.SetValue(dgNodate.Rows[idx].DataItem, "INFO_MAPP_LOTID", "");

                for ( int i = 0; i < dgnoInputLot.Rows.Count; i++)
                {
                    if(Util.NVC(DataTableConverter.GetValue(dgnoInputLot.Rows[i].DataItem, "SUBLOTID")).ToString() == MppingLot)
                    {
                        DataTableConverter.SetValue(dgnoInputLot.Rows[i].DataItem, "CHK", false);
                    }
                }

                //DataRow[] drInfo2 = GetChecked(ref dgnoInputLot, "CHK");
                //_NoinputCnt = drInfo2.Count();

            }

        }

        //private void dgNodatet_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            if (string.IsNullOrEmpty(e.Column.Name) == false)
        //            {
        //                if (e.Column.Name.Equals("CHK"))
        //                {
        //                    pre.Content = chkAll;
        //                    e.Column.HeaderPresenter.Content = pre;
        //                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                    chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
        //                    //chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                    //chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //                }
        //            }
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return;
        //    }
        //}

        #region  전체 선택 이벤트     
        private void CheckAll(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            for (int i = 0; i < dataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "CHK", true);
            }

        }
        private void UnCheckAll(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            for (int i = 0; i < dataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, "CHK", false);
            }
        }

        #endregion

        private void btnAllCheck_Click(object sender, RoutedEventArgs e)
        {

            if(_Chk)
            {
                UnCheckAll(dgNodate);
                UnCheckAll(dgnoInputLot);
                _Chk = false;

                for (int i = 0; i < dgNodate.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgNodate.Rows[i].DataItem, "INFO_MAPP_LOTID", "");
                }

            }
            else
            {
                _Chk = true;
                CheckAll(dgNodate);
                CheckAll(dgnoInputLot);

                for (int i = 0; i < dgNodate.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgNodate.Rows[i].DataItem, "INFO_MAPP_LOTID", Util.NVC(DataTableConverter.GetValue(dgnoInputLot.Rows[i].DataItem, "SUBLOTID")).ToString());
                }

            }

        }

    }
}
