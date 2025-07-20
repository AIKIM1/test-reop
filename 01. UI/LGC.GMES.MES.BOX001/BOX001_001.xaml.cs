/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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


using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Reflection;

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.BOX001
{

    /// <summary>
    /// BOX001_001.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class BOX001_001 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        double sum = 0;
        double totalqty2 = 0;
        bool fifo_flag = false;
        #region 

    
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_001()
        {
            InitializeComponent();
        }

        #endregion

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            tbPalletID.Visibility = Visibility.Visible;
            txtReturnPalletID.Visibility = Visibility.Visible;

            tbPancakeID.Visibility = Visibility.Hidden;
            txtReturnPancakeID.Visibility = Visibility.Hidden;

        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();

        }
        private void btnProductRefresh_Click(object sender, RoutedEventArgs e)
        {
            CommonCombo combo = new CommonCombo();
            string[] sFilter = { "SR",  LoginInfo.CFG_EQSG_ID };
            combo.SetCombo(cboProductSRSPack, CommonCombo.ComboStatus.ALL, sCase: "cboMaterialCode", sFilter:sFilter);
        }

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

       

        #region[SRS포장출고]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgPalletList);
            ClearDataGrid(dgPancakeList);

            txtPalletQty.Text = 0.ToString();

            SearchData();
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
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            
            ClearDataGrid(dgPancakeList);

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[i].DataItem, "CHK", true);
                    GetPancakeList(i);
                }
            }

            SumTotalQty();
            txtPalletQty.Text = Convert.ToString(sum);
        }
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ClearDataGrid(dgPancakeList);
            for (int i = 0; i < dgPalletList.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPalletList.Rows[i].DataItem, "CHK", false);
            }

            SumTotalQty();
            txtPalletQty.Text = Convert.ToString(sum);

        }
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
                            GetPancakeList(index);
                        }
                        else if (cb.IsChecked == false)
                        {
                            string _BOXID = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[index].DataItem, "BOXID"));
                            DataTable dt = DataTableConverter.Convert(dgPancakeList.ItemsSource);
                            if (dt.Rows.Count!=0)
                            {
                                dgPancakeList.ItemsSource = dt.Select("BOXID <> '" + _BOXID + "'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select("BOXID <> '" + _BOXID + "'").CopyToDataTable());
                            }
                        }
                        SumTotalQty();
                        txtPalletQty.Text = String.Format("{0:#,##0}", sum);//Convert.ToString(sum);
                        txtPacakeQty.Text = String.Format("{0:#,##0}", dgPancakeList.GetRowCount());  //dgPancakeList.GetRowCount().ToString();

                    }

                }
                catch (Exception ex)
                {
                    Util.AlertInfo(ex.ToString());
                }

            }));
        }
        private void dgPancakeList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void btnPalletInit_Click(object sender, RoutedEventArgs e)
        {
            //해당 Pallet를 초기화 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2800"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        InitPallet();
                    }
                    catch (Exception ex)
                    {
                        Util.AlertInfo(ex.ToString());
                    }

                }
            });
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //해당 Pancake을 삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2801"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    DeletePancake(index);
                }
            });
        }
        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation(true))
            {
                return;
            }
            //포장출고를 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(fifo_flag == true ? "[%1]에 출고안 된 Pallet가 있습니다. \r\n [%2]로 포장출고 하시겠습니까?" :  " [%2]로 포장출고 하시겠습니까?", fifo_flag == true ? Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "PRJT_NAME")) : null,cboOutLine.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_SHOPID", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_EQSGID", typeof(string));
                        inData.Columns.Add("FROM_PROCID", typeof(string));
                        inData.Columns.Add("FROM_PCSGID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY2", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[0].DataItem, "EQSGID")); //LoginInfo.CFG_EQSG_ID;
                        row["FROM_PROCID"] = Process.SRS_BOXING;
                        row["FROM_PCSGID"] = "E";

                        string[] toInfo = Convert.ToString(cboOutLine.SelectedValue).Split('|');
                        row["TO_SHOPID"] = toInfo[0];
                        row["TO_AREAID"] = toInfo[1];
                        row["TO_SLOC_ID"] = toInfo[2];
                        row["MOVE_ORD_QTY"] = Decimal.Parse(txtPalletQty.Text);
                        row["MOVE_ORD_QTY2"] = Decimal.Parse(Convert.ToString(totalqty2));
                        row["NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INBOX");
                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("SHIPTO_ID", typeof(string));

                        for (int j = 0; j < dgPalletList.GetRowCount(); j++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "CHK")).Equals("True"))
                            {
                                row = inLot.NewRow();
                                row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[j].DataItem, "BOXID"));
                                row["SHIPTO_ID"] = toInfo[3];
                                inLot.Rows.Add(row);
                            }

                        }


                       
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_MOVE_RAW_MATERIAL", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_SEND_MOVE_RAW_MATERIAL", bizException.Message, bizException.ToString());
                                    return;
                                }




                                Util.AlertInfo("SFU1931"); //출고완료
                                txtPalletQty.Text = "0";
                                rtxRemark.Document.Blocks.Clear();
                                SearchData();

                              //  initCombo();
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
                        Util.AlertInfo(ex.ToString());
                    }

                }

                fifo_flag = false;

            });
        }

        #endregion
        #region[팔레트구성대기]
        private void btnSearchPack_Click(object sender, RoutedEventArgs e)
        {
            GetPackWiatSearch();
        }
        private void txtModelID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string[] sFilter = { "SR", txtModelID.Text };
                combo.SetCombo(cboModelSRS, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            }
           
        }
        #endregion

        #region[포장 출고 현황]
        private void btnOutSearch_Click(object sender, RoutedEventArgs e)
        {
            GetOutPackSearch();
        }
       
        private void dgPackOut_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked_PackOut);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked_PackOut);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked_PackOut);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked_PackOut);
                    }
                }
            }));
        }
        private void checkAll_Checked_PackOut(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPackOut.Rows[i].DataItem, "CHK", true);
                    GetOutPackSearch();
                }
            }

        }
        private void checkAll_Unchecked_PackOut(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPackOut.Rows[i].DataItem, "CHK", false);
                    GetOutPackSearch();
                }
            }

        }
        #endregion


        #endregion

        #region[Method]

        private void initCombo()
        {

            CommonCombo combo = new CommonCombo();

            string[] sFilter = { "SR", LoginInfo.CFG_EQSG_ID };
            string[] sFilter0 = { "SR", txtModelID.Text };
            combo.SetCombo(cboProductSRSPack, CommonCombo.ComboStatus.ALL, sCase: "cboMaterialCode", sFilter: sFilter);

            string[] sFilter1 = { "SRS" };
            combo.SetCombo(cboOutLine, CommonCombo.ComboStatus.NONE, sFilter:sFilter1);

            string[] sFilter2 = { Process.SRS_BOXING };

            //Pallet 구성대기
            combo.SetCombo(cboAreaSRS, CommonCombo.ComboStatus.NONE, sFilter:sFilter2);
            combo.SetCombo(cboEquipmentSegmentSRS, CommonCombo.ComboStatus.NONE, sFilter: sFilter2);

            combo.SetCombo(cboModelSRS, CommonCombo.ComboStatus.ALL, sFilter: sFilter0);

            string[] sFilter3 = { Convert.ToString(cboEquipmentSegmentSRS.SelectedValue) };
            combo.SetCombo(cboEquipmentSlitter, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);

            //포장현황
   
            combo.SetCombo(cboOutArea, CommonCombo.ComboStatus.NONE, sCase: "cboAreaSRS", sFilter:sFilter2);
            combo.SetCombo(cboOutReportLine, CommonCombo.ComboStatus.ALL, sCase: "cboOutLine", sFilter: sFilter1);


            string[] sFilter5 = { "S4000"};
            combo.SetCombo(cboOutEquipmentSegment, CommonCombo.ComboStatus.NONE, sCase: "cboEquipmentSegmentSRS", sFilter: sFilter5);
            combo.SetCombo(cboOutProductSRS, CommonCombo.ComboStatus.ALL, sFilter: sFilter0, sCase: "cboOutModelSRS"); //출고된 모델만 나오게

            string[] sFilterOut = { Convert.ToString(cboOutEquipmentSegment.SelectedValue) };
            combo.SetCombo(cboOutEquipmentSlitter, CommonCombo.ComboStatus.ALL, sCase: "cboEquipmentSlitter", sFilter: sFilterOut);

            //반품이력조회
            combo.SetCombo(cboReturnLocationSRS, CommonCombo.ComboStatus.ALL, sCase: "cboOutLine", sFilter:sFilter1);


        }
        #region[포장출고]
        private void SearchData()
        {

            try
            {
                string _AllProdID = string.Empty;
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                if (Convert.ToString(cboProductSRSPack.SelectedValue).Equals(""))
                {
                    
                    DataTable tmpdt =  DataTableConverter.Convert(cboProductSRSPack.ItemsSource);
                    for (int i = 1; i < tmpdt.Rows.Count; i++)
                    {
                        if (i == tmpdt.Rows.Count - 1)
                        {
                            _AllProdID += Convert.ToString(tmpdt.Rows[i]["CBO_CODE"]);
                            break;
                        }
                        _AllProdID += Convert.ToString(tmpdt.Rows[i]["CBO_CODE"]) + ",";
                    }


                    row["PRODID"] = _AllProdID;
                }
                else
                {
                    row["PRODID"] = Convert.ToString(cboProductSRSPack.SelectedValue);
                }

                dt.Rows.Add(row);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_SRS_PACKED", "RQSTDT", "RSLTDT", dt);

                Util.GridSetData(dgPalletList, result, FrameOperation);

                dgPalletList.BottomRows[0].Visibility = Visibility.Visible;
                ClearDataGrid(dgPancakeList);


                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgPalletList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTALQTY"], dac);

                txtLotID.Text = "";


             

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
            }


        }
        private void GetPancakeList(int index)
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

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_PANCAKE_SRS", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count <= 0)
                {
                    return;
                }

                DataTable _PancakeList = DataTableConverter.Convert(dgPancakeList.ItemsSource);

                if (dgPancakeList.ItemsSource == null)
                {
                    _PancakeList.Columns.Add("BOXID", typeof(string));
                    _PancakeList.Columns.Add("LOTID", typeof(string));
                    _PancakeList.Columns.Add("PRJT_NAME", typeof(string));
                    _PancakeList.Columns.Add("WIPQTY", typeof(decimal));
                    _PancakeList.Columns.Add("WIPQTY2", typeof(string));
                    _PancakeList.Columns.Add("MODLID", typeof(string));
                    _PancakeList.Columns.Add("PRODID", typeof(string));
                    _PancakeList.Columns.Add("WIPHOLD", typeof(string));
                    _PancakeList.Columns.Add("VLD_DATE", typeof(string));
                    _PancakeList.Columns.Add("PROD_VER_CODE", typeof(string));
                    _PancakeList.Columns.Add("MODLNAME", typeof(string));
                    _PancakeList.Columns.Add("UNIT_CODE", typeof(string));
                    _PancakeList.Columns.Add("EQSGID", typeof(string));
                }

                //PancakeList에 있는지 확인
                for (int i = 0; i < dgPancakeList.GetRowCount(); i++)
                {
                    if (_BOXID.Equals(Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "BOXID"))))
                    {
                        Util.AlertInfo("SFU1774"); //이미 구성 PANCAKE에 존재합니다.
                        return;
                    }
                }

                DataRow _Row = null;
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    _Row = _PancakeList.NewRow();
                    _Row["BOXID"] = result.Rows[i]["BOXID"].ToString();
                    _Row["LOTID"] = result.Rows[i]["LOTID"].ToString();
                    _Row["PRJT_NAME"] = result.Rows[i]["PRJT_NAME"].ToString();
                    _Row["WIPQTY"] = decimal.Parse(result.Rows[i]["WIPQTY"].ToString());
                    _Row["WIPQTY2"] = result.Rows[i]["WIPQTY2"].ToString();
                    _Row["MODLID"] = result.Rows[i]["MODLID"].ToString();
                    _Row["PRODID"] = result.Rows[i]["PRODID"].ToString();
                    _Row["WIPHOLD"] = result.Rows[i]["WIPHOLD"].ToString();
                    _Row["VLD_DATE"] = result.Rows[i]["VLD_DATE"].ToString();
                    _Row["PROD_VER_CODE"] = result.Rows[i]["PROD_VER_CODE"].ToString();
                    _Row["MODLNAME"] = result.Rows[i]["MODLNAME"].ToString();
                    _Row["UNIT_CODE"] = result.Rows[i]["UNIT_CODE"].ToString();
                    _Row["EQSGID"] = result.Rows[i]["EQSGID"].ToString();

                    _PancakeList.Rows.Add(_Row);
                }

                //dgPancakeList.ItemsSource = DataTableConverter.Convert(_PancakeList);
                Util.GridSetData(dgPancakeList, _PancakeList, FrameOperation, true);
                dgPancakeList.BottomRows[0].Visibility = Visibility.Visible;

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgPalletList.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgPancakeList.Columns["WIPQTY"], dac);


            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
            }
        }
        private void SumTotalQty()
        {
            
            sum = 0;
            totalqty2 = 0;

            for (int i = 0; i < dgPalletList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    sum += Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "TOTALQTY")).Equals("") ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "TOTALQTY")));
                    totalqty2 += Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "TOTALQTY2")).Equals("") ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "TOTALQTY2")));
                }
            }

            txtPalletQty.Text = String.Format("{0:#,##0}", sum); //Convert.ToString(sum);
            txtPacakeQty.Text = dgPancakeList == null ? 0.ToString() : String.Format("{0:#,##0}", dgPancakeList.GetRowCount());//dgPancakeList.GetRowCount().ToString();
            //  return sum;
        }

        private void DeletePancake(int index)
        {
            try
            {
                string _BOXID = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "BOXID"));
                string _PANCAKEID = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "LOTID"));
                string _PRODID = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "PRODID"));


                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("BOXID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                inData.Columns.Add("UNPACK_QTY", typeof(string));
                inData.Columns.Add("UNPACK_QTY2", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["BOXID"] = _BOXID;
                row["PRODID"] = _PRODID;//cboMaterialCode.SelectedValue.ToString();
                row["PACK_LOT_TYPE_CODE"] = "LOT";
                row["UNPACK_QTY"] = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "WIPQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "WIPQTY")));//Decimal.Parse(txtPalletQty.Text); // TotalQty - UnPackQty <=0 --> Pack 수량이 
                row["UNPACK_QTY2"] = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "WIPQTY2")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "WIPQTY2")));
                row["USERID"] = LoginInfo.USERID;
                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));


                row = inLot.NewRow();
                row["LOTID"] = _PANCAKEID;
                indataSet.Tables["INLOT"].Rows.Add(row);

                DataTable dt = DataTableConverter.Convert(dgPancakeList.ItemsSource);
                if (dt == null)
                {
                    return;
                }

                if (dt.Select("BOXID = '" + _BOXID + "'").Count() == 1)
                {
                    //해당 BOXID의 팬케익이 한 개 남은 경우
                    //팔레트 초기화
                    //해당 Pallet의 Pancake이 1개 남았습니다. Pallet을 초기화 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3066"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            string boxid = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[index].DataItem, "BOXID"));

                            try
                            {
                                ShowLoadingIndicator();

                                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_SRS", "INDATA,INLOT", null, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.AlertByBiz("BR_PRD_REG_UNPACK_LOT_FOR_SRS", bizException.Message, bizException.ToString());
                                            return;
                                        }




                                        txtPalletQty.Text = 0.ToString();
                                        Util.AlertInfo("SFU1413");//Pallet이 초기화 되었습니다.
                                       // initCombo();

                                        //pacake데이터 그리드에서 해당 pancakeid 지움
                                        DataTable dtp = DataTableConverter.Convert(dgPancakeList.ItemsSource);
                                        dtp.Rows[index].Delete();
                                        dgPancakeList.ItemsSource = DataTableConverter.Convert(dtp);

                                        //pallet데이터 그리드에서 해당 palletid 지움
                                        dtp = DataTableConverter.Convert(dgPalletList.ItemsSource);
                                        dgPalletList.ItemsSource = dtp.Select("BOXID <> '" + boxid + "'").Count() == 0 ? null : DataTableConverter.Convert(dtp.Select("BOXID <> '" + boxid + "'").CopyToDataTable());
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                    finally
                                    {
                                        HiddenLoadingIndicator();
                                    }
                                }, indataSet);

                            }
                            catch (Exception ex)
                            {
                                //Pallet초기화 에러
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2803"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                Util.MessageException(ex);
                            }
                        }

                  


                });
            

                }
                else // Pancake삭제
                {

                    try
                    {
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_SRS_PARTIAL", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    //Util.AlertByBiz("BR_PRD_REG_UNPACK_LOT_FOR_SRS_PARTIAL", bizException.Message, bizException.ToString());
                                    return;
                                }



                                Util.AlertInfo("SFU1417"); //Pancake이 삭제 되었습니다.

                                SearchData();
                                for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")).Equals(_BOXID))
                                    {
                                        DataTableConverter.SetValue(dgPalletList.Rows[i].DataItem, "CHK", 1);
                                        GetPancakeList(i);
                                    }
                                }
                                SumTotalQty();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Pancake 삭제 에러
                    }

                    
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
            }

        }
        private void InitPallet()
        {
            if (!Validation(false))
            {
                return;
            }

            for (int i = 0; i < dgPalletList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("BOXID", typeof(string));
                    inData.Columns.Add("PRODID", typeof(string));
                    inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                    inData.Columns.Add("UNPACK_QTY", typeof(string));
                    inData.Columns.Add("UNPACK_QTY2", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID"));
                    row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "PRODID"));
                    row["PACK_LOT_TYPE_CODE"] = "LOT";
                    row["UNPACK_QTY"] = Decimal.Parse(txtPalletQty.Text);
                    row["UNPACK_QTY2"] = totalqty2;
                    row["USERID"] = LoginInfo.USERID;
                    indataSet.Tables["INDATA"].Rows.Add(row);


                    DataTable inLot = indataSet.Tables.Add("INLOT");
                    inLot.Columns.Add("LOTID", typeof(string));

                    for (int j = 0; j < dgPancakeList.GetRowCount(); j++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[j].DataItem, "BOXID")).Equals(Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID"))))
                        {
                            row = inLot.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[j].DataItem, "LOTID"));
                            indataSet.Tables["INLOT"].Rows.Add(row);
                        }

                    }

                    try
                    {
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_SRS", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_UNPACK_LOT_FOR_SRS", bizException.Message, bizException.ToString());
                                    return;
                                }



                                Util.MessageInfo("SFU1280");

                                SearchData();
                                txtPalletQty.Text = 0.ToString();
                                initCombo();
                                rtxRemark.Document.Blocks.Clear();

                                ClearDataGrid(dgPancakeList);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                               // LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Pallet초기화 에러
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2803"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }

               

                }

            }

          

        }
        private void btnAddPancake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK") == -1)
                {
                    Util.MessageValidation("SFU3425"); //선택된 Pallet가 없습니다.
                    txtAddPancakeId.Text = "";
                    return;
                }
                if (DataTableConverter.Convert(dgPalletList.ItemsSource).Select("CHK = 1").Count() > 1)
                {
                    Util.MessageValidation("SFU3426"); //추가 할 Pallet 한 개만 선택해주세요
                    txtAddPancakeId.Text = "";
                    return;
                }
                if (txtAddPancakeId.Text.Equals(""))
                {
                    Util.MessageValidation("SFU3427"); //추가 할 PancakeID를 입력해주세요
                    txtAddPancakeId.Text = "";
                    return;
                }
                for (int i = 0; i < dgPancakeList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "LOTID")).Equals(txtAddPancakeId.Text))
                    {
                        Util.MessageValidation("SFU3428"); //추가하려는 PancakeID가 이미 있습니다.
                        txtAddPancakeId.Text = "";
                        return;
                    }
                }


                DataSet ds = new DataSet();
                DataTable indata = ds.Tables.Add("INDATA");
                indata.Columns.Add("SRCTYPE", typeof(string));
                indata.Columns.Add("BOXID", typeof(string));
                indata.Columns.Add("LOTID", typeof(string));
                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("USERID", typeof(string));

                DataRow row = indata.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "BOXID"));
                row["LOTID"] = txtAddPancakeId.Text;
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "PRODID"));
                row["USERID"] = LoginInfo.USERID;
                indata.Rows.Add(row);

                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PACK_LOT_FOR_SRS_PANCAKE_ADD", "INDATA", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_PACK_LOT_FOR_SRS_PANCAKE_ADD", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.MessageInfo("PSS9072");//작업이 완료되었습니다.
                            txtAddPancakeId.Text = "";

                            int index = _Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                            SearchData();
                            DataTableConverter.SetValue(dgPalletList.Rows[index].DataItem, "CHK", 1);
                            GetPancakeList(index);

                            SumTotalQty();
                           // txtPalletQty.Text = Convert.ToString(sum);
                           // txtPacakeQty.Text = dgPancakeList.GetRowCount().ToString();





                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }, ds);

                }
                catch (Exception ex)
                {
                    //출고취소에러
                    Util.MessageException(ex);

                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgPalletList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void btnPackCancel_Click(object sender, RoutedEventArgs e)
        {
            //포장출고를 취소하시겠습니까?

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2805"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "CHK")).Equals("1"))
                            {

                                DataSet indataSet = new DataSet();
                                DataTable inData = indataSet.Tables.Add("INDATA");
                                inData.Columns.Add("SRCTYPE", typeof(string));
                                inData.Columns.Add("RCV_ISS_ID", typeof(string));
                                inData.Columns.Add("AREAID", typeof(string));
                                inData.Columns.Add("CNCL_QTY", typeof(string));
                                inData.Columns.Add("USERID", typeof(string));
                                inData.Columns.Add("SHIP_TO_ID", typeof(string));

                                DataRow row = inData.NewRow();
                                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                row["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "RCV_ISS_ID"));
                                row["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "FROM_AREAID"));
                                row["CNCL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTALQTY"));
                                row["USERID"] = LoginInfo.USERID;
                                row["SHIP_TO_ID"] = LoginInfo.CFG_AREA_ID;//선택한거
                                indataSet.Tables["INDATA"].Rows.Add(row);

                                DataTable inpallet = indataSet.Tables.Add("INPALLET");
                                inpallet.Columns.Add("BOXID", typeof(string));

                                row = inpallet.NewRow();
                                row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID"));
                                indataSet.Tables["INPALLET"].Rows.Add(row);

                                try
                                {
                                    ShowLoadingIndicator();

                                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_PRODUCT_CANCEL_FOR_PACKING", "INDATA,INLOT", null, (bizResult, bizException) =>
                                    {
                                        try
                                        {
                                            if (bizException != null)
                                            {
                                                Util.AlertByBiz("BR_PRD_REG_SHIP_PRODUCT_CANCEL_FOR_PACKING", bizException.Message, bizException.ToString());
                                                return;
                                            }

                                            Util.AlertInfo("SFU1930");//출고 취소 완료
                                            initCombo();


                                        }
                                        catch (Exception ex)
                                        {
                                            Util.MessageException(ex);
                                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                        }
                                        finally
                                        {
                                            HiddenLoadingIndicator();
                                        }
                                    }, indataSet);

                                }
                                catch (Exception ex)
                                {
                                    //출고취소에러
                                    Util.MessageException(ex);

                                }



                            }
                        }
                  



                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Util.AlertInfo(ex.ToString());
                    }

                }
            });
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
            dg.BottomRows[0].Visibility = Visibility.Collapsed;
        }
        #endregion
        #region[팔레트구성대기]
        private void GetPackWiatSearch()
        {
            try
            {
                dgPaackWait.ItemsSource = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("MODLID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("PROCID_ST", typeof(string));
                dt.Columns.Add("PROCID_EW", typeof(string));
                dt.Columns.Add("PRODUCT_LEVEL", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboEquipmentSegmentSRS.SelectedValue);
                row["MODLID"] = Convert.ToString(cboModelSRS.SelectedValue).Equals("") ? null : Convert.ToString(cboModelSRS.SelectedValue);
                row["EQPTID"] = Convert.ToString(cboEquipmentSlitter.SelectedValue).Equals("") ? null : Convert.ToString(cboEquipmentSlitter.SelectedValue);
                row["WIPSTAT"] = Wip_State.WAIT;
                row["PROCID_ST"] = Process.SRS_SLITTING;
                row["PROCID_EW"] = Process.SRS_BOXING;
                row["PRODUCT_LEVEL"] = "SR";
                dt.Rows.Add(row);

                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_PACK_WAIT", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("DA_PRD_SEL_WIP_PACK_WAIT", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.GridSetData(dgPaackWait, bizResult, FrameOperation);



                            dgPaackWait.BottomRows[0].Visibility = Visibility.Visible;

                            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                            DataGridAggregateSum dagsum = new DataGridAggregateSum();
                            dagsum.ResultTemplate = dgPaackWait.Resources["ResultTemplate"] as DataTemplate;
                            dac.Add(dagsum);
                            DataGridAggregate.SetAggregateFunctions(dgPaackWait.Columns["WIPQTY"], dac);

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

                }
                catch (Exception ex)
                {
                    //조회 오류

                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2807"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }





            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
            }
           
            
        }
        #endregion
        #region[출고이력조회]
        private void dgPackOut_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void GetOutPackSearch()
        {
            try
            {
                dgPackOut.ItemsSource = null;
                string[] str = Convert.ToString(cboOutReportLine.SelectedValue).Equals("") ? null : Convert.ToString(cboOutReportLine.SelectedValue).Split('|');

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("FROMDATE", typeof(string));
                dt.Columns.Add("TODATE", typeof(string));
                dt.Columns.Add("SHIPTO_ID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                if (txtLotID.Text.Equals(""))
                {
                    row["EQSGID"] = Convert.ToString(cboOutEquipmentSegment.SelectedValue);
                    row["PRODID"] = Convert.ToString(cboOutProductSRS.SelectedValue).Equals("") ? null : Convert.ToString(cboOutProductSRS.SelectedValue);
                    row["EQPTID"] = Convert.ToString(cboOutEquipmentSlitter.SelectedValue).Equals("") ? null : Convert.ToString(cboOutEquipmentSlitter.SelectedValue);
                    row["LOTID"] = null;
                    row["FROMDATE"] = ldpDatePickerFrom.SelectedDateTime.ToShortDateString();
                    row["TODATE"] = ldpDatePickerTo.SelectedDateTime.ToShortDateString();
                    row["SHIPTO_ID"] = str == null ? null : str[3];
                }
                else
                {
                    row["EQSGID"] = null;
                    row["PRODID"] = null;
                    row["EQPTID"] = null;
                    row["LOTID"] = txtLotID.Text.Equals("") ? null : txtLotID.Text;
                    row["FROMDATE"] = null;
                    row["TODATE"] = null;
                    row["SHIPTO_ID"] = null;
                }
                
                dt.Rows.Add(row);


                try
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_SHIPED_SRS", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                               // Util.AlertByBiz("DA_PRD_SEL_LOT_SHIPED_SRS", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.GridSetData(dgPackOut, bizResult, FrameOperation, true);

                            dgPackOut.EndEdit();
                            dgPackOut.MergingCells -= dgPackOut_MergingCells;
                            dgPackOut.MergingCells += dgPackOut_MergingCells;

                            dgPackOut.BottomRows[0].Visibility = Visibility.Visible;

                            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                            DataGridAggregateSum dagsum = new DataGridAggregateSum();
                            dagsum.ResultTemplate = dgPackOut.Resources["ResultTemplate"] as DataTemplate;
                            dac.Add(dagsum);
                            DataGridAggregate.SetAggregateFunctions(dgPackOut.Columns["WIPQTY"], dac);

                            txtLotID.Text = "";

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

                }
                catch (Exception ex)
                {
                    //조회 오류
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.AlertInfo(ex.ToString());
            }
        }

        private void dgPackOut_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgPackOut.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgPackOut.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPackOut.GetCell(x, dgPackOut.Columns["BOXID"].Index).Value) == Util.NVC(dgPackOut.GetCell(i, dgPackOut.Columns["BOXID"].Index).Value))
                    {
                        x1 = i;
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dgPackOut.GetCell((int)x, (int)0), dgPackOut.GetCell((int)x1, (int)0)));
                        
                        x = x1 + 1;
                        i = x1;
                    }
                }
                e.Merge(new DataGridCellsRange(dgPackOut.GetCell((int)x, (int)0), dgPackOut.GetCell((int)x1, (int)0)));
               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

      


        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetOutPackSearch();
            }
            
        }
        #endregion


        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
           

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private bool Validation(bool outBtn)
        {
            //팔레트 초기화, 출고 버튼 선택시 validation

            int firstIndex = _Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
            if (firstIndex == -1)
            {
                Util.AlertInfo("SFU1632");//"선택된 LOT이 없습니다.
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
                            Util.MessageInfo("SFU1896");//제품코드가 같은 Pallet만 선택해주세요"
                            return false;
                        }
                    }
                   
                }
            }

            if (outBtn)
            {
                DataSet ds = new DataSet();

                DataTable inBox = ds.Tables.Add("INBOX");
                inBox.Columns.Add("BOXID", typeof(string));

                for (int i = 0; i < dgPalletList.GetRowCount(); i++)
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow row = inBox.NewRow();
                        row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID"));
                        inBox.Rows.Add(row);

                    }

                }

                try
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_VALID_OUT", "INBOX", null, ds);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);

                    return false;
                }

                if (DataTableConverter.Convert(dgPalletList.ItemsSource).Select("PRJT_NAME = '" + Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "PRJT_NAME")) + "' and PACKDTTM < '" + Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "PACKDTTM")) + "'").Count() > 0)
                {
                    fifo_flag = true;
                   // Util.MessageInfo("[%1] 해당모델에 출고 안 된 Pallet가 있습니다. ", Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK")].DataItem, "PRJT_NAME")));
                }
            }
            

           return true;

        }


        //private bool ValidInOut(int i)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("BOXID", typeof(string));
        //        dt.Columns.Add("ACTID", typeof(string));

        //        DataRow row = dt.NewRow();
        //        row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID"));
        //        row["ACTID"] = "STOCK_IN_STORAGE,LOCATE_OUT_STORAGE";
        //        dt.Rows.Add(row);

        //        DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPACTHISTORY_INOUT_CHECK", "RQSTDT", "RSLTDT", dt);
        //        if (result.Rows.Count == 0)
        //        {
        //            //팔레트{%1}가 창고 입고/출고 처리 되지 않았습니다.
        //            Util.MessageInfo("SFU3238", Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")));
        //            //Util.Alert("팔레트 [" + Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")) + " ]가 창고 입고/출고 처리 되지 않았습니다.");
        //            return false;
        //        }
        //        if (result.Select("ACTID = 'STOCK_IN_STORAGE'").Count() == 0 && result.Select("ACTID = 'LOCATE_IN_STORAGE'").Count() == 0)
        //        {
        //            //팔레트{%1}가 창고 입고처리 되지 않았습니다.
        //            Util.MessageInfo("SFU3240", Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")));
        //            //Util.Alert("팔레트 [" + Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")) + " ]가 창고 입고처리 되지 않았습니다.");
        //            return false;
        //        }

        //        if (result.Select("ACTID = 'LOCATE_OUT_STORAGE'").Count() == 0 && result.Select("ACTID = 'STOCK_OUT_STORAGE'").Count() == 0)
        //        {
        //            //팔레트{%1}가 창고 출고처리 되지 않았습니다.
        //            Util.MessageInfo("SFU3239", Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")));
        //            //Util.Alert("팔레트 [" + Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID")) + " ]가 창고 출고처리 되지 않았습니다.");
        //            return false;
        //        }

        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //private bool ValidRack(int i)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("LOTID", typeof(string));

        //        DataRow row = dt.NewRow();
        //        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "LOTID"));
        //        dt.Rows.Add(row);

        //        DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPATTR_VALID_RACK_INOUT", "RQSTDT", "RSLTDT", dt);
        //        if (result.Rows.Count == 0)
        //        {
        //            Util.MessageValidation("SFU1444" , Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "LOTID"))); // %1가 없습니다.
        //            return false;
        //        }
        //        if (!Convert.ToString(result.Rows[0]["RACK_ID"]).Equals(""))
        //        {
        //            Util.MessageValidation("SFU3583", Util.NVC(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "LOTID"))); //[%1]이 RACK에 입고 되어있는 상태입니다.
        //            return false;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        private void btnReturnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (rdPallet.IsChecked == true)
            {
                if (txtReturnPalletID.Text.Equals(""))
                {
                    Util.MessageValidation("SFU1411"); //PALLETID를 입력해주세요
                    return;
                }
            }
            if (rdPancake.IsChecked == true)
            { 
             if (txtReturnPancakeID.Text.Equals(""))
                {
                    Util.MessageValidation("SFU1414"); //PANCAKE ID를 입력해주세요
                    return;
                }
            }
            GetReturnInfo();
        }
        private void GetReturnInfo()
        {
            try
            {
                

                DataTable dt = null;
                DataRow row = null;
                DataTable result = null;

                if (rdPancake.IsChecked == true)
                {
                    dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    row = dt.NewRow();
                    row["LOTID"] = txtReturnPancakeID.Text;
                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SRS_RETURN_VAL", "RQSTDT", "RSLTDT", dt);
                    if (int.Parse(Convert.ToString(result.Rows[0]["COUNT"]).Equals("") ? 0.ToString() : Convert.ToString(result.Rows[0]["COUNT"])) == 0)
                    {
                        Util.MessageValidation("SFU1550");//반품 가능한 ID가 아닙니다.
                        return;
                    }
                }


                DataTable returnTable = DataTableConverter.Convert(dgReturnLot.ItemsSource);
                DataRow returnRow = null;

                if (dgReturnLot.ItemsSource == null)
                {
                    returnTable.Columns.Add("CHK", typeof(string));
                    returnTable.Columns.Add("PALLETID", typeof(string));
                    returnTable.Columns.Add("SHIPTO_NAME", typeof(string));
                    returnTable.Columns.Add("LOTID", typeof(string));
                    returnTable.Columns.Add("PRODID", typeof(string));
                    returnTable.Columns.Add("PRODNAME", typeof(string));
                    returnTable.Columns.Add("FROM_SHOPID", typeof(string));
                    returnTable.Columns.Add("FROM_AREAID", typeof(string));
                    returnTable.Columns.Add("FROM_EQSGID", typeof(string));
                    returnTable.Columns.Add("FROM_PROCID", typeof(string));
                    returnTable.Columns.Add("TO_SHOPID", typeof(string));
                    returnTable.Columns.Add("TO_AREAID", typeof(string));
                    returnTable.Columns.Add("TO_SLOC_ID", typeof(string));
                    returnTable.Columns.Add("TO_PCSGID", typeof(string));
                    returnTable.Columns.Add("MOVE_ORD_QTY", typeof(string));
                    returnTable.Columns.Add("MOVE_ORD_QTY2", typeof(string));
                    returnTable.Columns.Add("SLOC_NAME", typeof(string));
                    returnTable.Columns.Add("INSUSER", typeof(string));
                    returnTable.Columns.Add("REMARK", typeof(string));

                }

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("BOXID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                if (rdPallet.IsChecked == true)
                {
                    row["BOXID"] = txtReturnPalletID.Text;
                    row["LOTID"] = null;

                }
                else
                {
                    row["BOXID"] = null;
                    row["LOTID"] = txtReturnPancakeID.Text;
                }
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RETURN_SRS", "RQSTDT", "RSLTDT", dt);
                if (result == null)
                {
                    return;
                }
                if (result.Rows.Count == 0)
                {
                    FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "]" + MessageDic.Instance.GetMessage("SFU1905")); ////조회된 Data가 없습니다.
                    return;
                }

                if (rdPallet.IsChecked == true)
                {
                    if (returnTable.Select("PALLETID = '" + Convert.ToString(result.Rows[0]["PALLETID"]) + "'").Count() != 0)
                    {
                        Util.MessageValidation("SFU2011");
                       // Util.AlertInfo("SFU2011"); //해당 BOX는 이미 존재합니다.
                        return;
                    }
                   

                }
                else
                {
                    if (returnTable.Select("LOTID = '" + Convert.ToString(result.Rows[0]["LOTID"]) + "'").Count() != 0)
                    {
                        Util.AlertInfo("SFU1271"); //해당 PANCAKE은 이미 존재합니다.
                        return;
                    }
                }


                for (int i = 0; i < result.Rows.Count; i++)
                {
                    returnRow = returnTable.NewRow();
                    returnRow["CHK"] = Convert.ToString(result.Rows[i]["CHK"]);
                    returnRow["PALLETID"] = Convert.ToString(result.Rows[i]["PALLETID"]);
                    returnRow["SHIPTO_NAME"] = Convert.ToString(result.Rows[i]["SHIPTO_NAME"]);
                    returnRow["LOTID"] = Convert.ToString(result.Rows[i]["LOTID"]);
                    returnRow["PRODID"] = Convert.ToString(result.Rows[i]["PRODID"]);
                    returnRow["PRODNAME"] = Convert.ToString(result.Rows[i]["PRODNAME"]);
                    returnRow["FROM_SHOPID"] = Convert.ToString(result.Rows[i]["FROM_SHOPID"]);
                    returnRow["FROM_AREAID"] = Convert.ToString(result.Rows[i]["FROM_AREAID"]);
                    returnRow["FROM_EQSGID"] = Convert.ToString(result.Rows[i]["FROM_EQSGID"]);
                    returnRow["FROM_PROCID"] = Convert.ToString(result.Rows[i]["FROM_PROCID"]);
                    returnRow["TO_SHOPID"] = Convert.ToString(result.Rows[i]["TO_SHOPID"]);
                    returnRow["TO_AREAID"] = Convert.ToString(result.Rows[i]["TO_AREAID"]);
                    returnRow["TO_SLOC_ID"] = Convert.ToString(result.Rows[i]["TO_SLOC_ID"]);
                    returnRow["TO_PCSGID"] = Convert.ToString(result.Rows[i]["TO_PCSGID"]);
                    returnRow["MOVE_ORD_QTY"] = Convert.ToString(result.Rows[i]["MOVE_ORD_QTY"]);
                    returnRow["MOVE_ORD_QTY2"] = Convert.ToString(result.Rows[i]["MOVE_ORD_QTY2"]);
                    returnRow["SLOC_NAME"] = Convert.ToString(result.Rows[i]["SLOC_NAME"]);
                    returnRow["INSUSER"] = Convert.ToString(result.Rows[i]["INSUSER"]);

                    returnTable.Rows.Add(returnRow);
                }


                Util.GridSetData(dgReturnLot, returnTable, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
            }
            finally
            {
                txtReturnPalletID.Text = "";
                txtReturnPancakeID.Text = "";
            }

        }

    
        
        private void rdPallet_Click(object sender, RoutedEventArgs e)
        {
            if (rdPallet.IsChecked == true)
            {
                tbPalletID.Visibility = Visibility.Visible;
                txtReturnPalletID.Visibility = Visibility.Visible;

                tbPancakeID.Visibility = Visibility.Hidden;
                txtReturnPancakeID.Visibility = Visibility.Hidden;
            }
            else
            {
                tbPalletID.Visibility = Visibility.Hidden;
                txtReturnPalletID.Visibility = Visibility.Hidden;

                tbPancakeID.Visibility = Visibility.Visible;
                txtReturnPancakeID.Visibility = Visibility.Visible;
            }

            dgReturnLot.ItemsSource = null;
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReturnLot, "CHK") == -1)
            {
                Util.AlertInfo("SFU1632"); //선택된 LOT이 없습니다.
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
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_SHOPID", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_EQSGID", typeof(string));
                        inData.Columns.Add("FROM_PROCID", typeof(string));
                        inData.Columns.Add("FROM_PCSGID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_PCSGID", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY", typeof(string));
                        inData.Columns.Add("MOVE_ORD_QTY2", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = null;

                        for (int i = 0; i < dgReturnLot.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                

                                row = inData.NewRow();
                                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                row["FROM_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "FROM_SHOPID"));
                                row["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "FROM_AREAID"));
                                row["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "FROM_EQSGID"));
                                row["FROM_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "FROM_PROCID"));
                                row["FROM_PCSGID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "TO_SHOPID"));
                                row["TO_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "TO_SHOPID"));
                                row["TO_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "TO_AREAID"));
                                row["TO_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "TO_SLOC_ID"));
                                row["TO_PCSGID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "TO_PCSGID"));
                                row["MOVE_ORD_QTY"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "MOVE_ORD_QTY")));
                                row["MOVE_ORD_QTY2"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "MOVE_ORD_QTY2")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "MOVE_ORD_QTY2")));
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "LOTID"));
                                row["USERID"] = LoginInfo.USERID;
                                row["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgReturnLot.Rows[i].DataItem, "REMARK"));/* new TextRange(rtxReturnRemark.Document.ContentStart, rtxReturnRemark.Document.ContentEnd).Text;*/
                                indataSet.Tables["INDATA"].Rows.Add(row);



                            }
                        }
                      



                        try
                        {
                            ShowLoadingIndicator();

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_RETURN_RAW_MATERIAL", "INDATA", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.AlertByBiz("BR_PRD_REG_RECEIVE_RETURN_RAW_MATERIAL", bizException.Message, bizException.ToString());
                                        return;
                                    }

                                    Util.AlertInfo("SFU1552"); //반품 입고 처리 되었습니다.

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
                        Util.AlertInfo(ex.ToString());
                    }
                }
            });
        }
        private void txtReturnPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtReturnPalletID.Text.Equals(""))
                {
                    Util.AlertInfo("SFU1411"); //PALLETID를 입력해주세요
                    return;
                }


                GetReturnInfo();
            }
        }

        private void txtReturnPancakeID_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Enter)
            {
                if (txtReturnPancakeID.Text.Equals(""))
                {
                    Util.AlertInfo("SFU1414"); // PANCAKE ID를 입력해주세요
                    return;
                }

                GetReturnInfo();
            }
        }
        private void btnLotDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            DataTable dt = DataTableConverter.Convert(dgReturnLot.ItemsSource);
            dt.Rows[index].Delete();

            dgReturnLot.ItemsSource = DataTableConverter.Convert(dt);
        }
        private void btnOutCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (rdPallet.IsChecked == false)
                //{
                //    Util.MessageValidation("SFU3429"); //Pallet만 출고 취소가 가능합니다.
                //    return;
                //}
                if (_Util.GetDataGridCheckFirstRowIndex(dgPackOut, "CHK") == -1)
                {
                    Util.MessageValidation("PSS9073"); // 선택된 LOT이 없습니다."
                    return;
                }

                //출고취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3430"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("BOXID", typeof(string));
                            inData.Columns.Add("FROM_SHOPID", typeof(string));
                            inData.Columns.Add("FROM_AREAID", typeof(string));
                            inData.Columns.Add("FROM_EQSGID", typeof(string));
                            inData.Columns.Add("FROM_PROCID", typeof(string));
                            inData.Columns.Add("FROM_PCSGID", typeof(string));
                            inData.Columns.Add("TO_SHOPID", typeof(string));
                            inData.Columns.Add("TO_AREAID", typeof(string));
                            inData.Columns.Add("TO_SLOC_ID", typeof(string));
                            inData.Columns.Add("TO_PCSGID", typeof(string));
                            inData.Columns.Add("MOVE_ORD_QTY", typeof(string));
                            inData.Columns.Add("MOVE_ORD_QTY2", typeof(string));
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("NOTE", typeof(string));

                            DataRow row = null;

                            for (int i = 0; i < dgPackOut.Rows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "CHK")).Equals("True"))
                                {


                                    row = inData.NewRow();
                                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                    row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID"));
                                    row["FROM_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "FROM_SHOPID"));
                                    row["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "FROM_AREAID"));
                                    row["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "FROM_EQSGID"));
                                    row["FROM_PROCID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "FROM_PROCID"));
                                    row["FROM_PCSGID"] = "";
                                    row["TO_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TO_SHOPID"));
                                    row["TO_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TO_AREAID"));
                                    row["TO_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TO_SLOC_ID"));
                                    row["TO_PCSGID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TO_PCSGID"));
                                    row["MOVE_ORD_QTY"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "MOVE_ORD_QTY")));
                                    row["MOVE_ORD_QTY2"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "MOVE_ORD_QTY2")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "MOVE_ORD_QTY2")));
                                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "LOTID"));
                                    row["USERID"] = LoginInfo.USERID;
                                    row["NOTE"] = "";// Util.NVC(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "REMARK"));/* new TextRange(rtxReturnRemark.Document.ContentStart, rtxReturnRemark.Document.ContentEnd).Text;*/
                                    indataSet.Tables["INDATA"].Rows.Add(row);



                                }
                            }




                            try
                            {
                                ShowLoadingIndicator();

                                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SEND_RAW_MATERIAL", "INDATA", null, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.AlertByBiz("BR_PRD_REG_CANCEL_SEND_RAW_MATERIAL", bizException.Message, bizException.ToString());
                                            return;
                                        }

                                        Util.MessageInfo("SFU3431"); //출고 취소 되었습니다.

                                        GetOutPackSearch();

                                        //DataTable dt = DataTableConverter.Convert(dgReturnLot.ItemsSource).Select("CHK <> 'True'").Count() == 0 ? null : DataTableConverter.Convert(dgReturnLot.ItemsSource).Select("CHK <> 'True'").CopyToDataTable();
                                        //dgReturnLot.ItemsSource = dt == null ? null : DataTableConverter.Convert(dt);


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
                                Util.MessageException(ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }                                                                                    
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region[반품이력조회]
        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender == null) return;
            if (e.Key == Key.Enter)
            {
                GetReturnInfoDetl();
            }

        }
        private void btnReturnInfoSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetReturnInfoDetl();
                //string returnLoc = Convert.ToString(cboReturnLocationSRS.SelectedValue);

                //DataTable dt = new DataTable();
                //dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("FROM", typeof(string));
                //dt.Columns.Add("TO", typeof(string));
                //dt.Columns.Add("SHIPTO_ID", typeof(string));
                //dt.Columns.Add("TO_PROCID", typeof(string));
                //dt.Columns.Add("PROCID", typeof(string));

                //DataRow row = dt.NewRow();
                //row["LANGID"] = LoginInfo.LANGID;
                //row["FROM"] = ldpDatePickerReturnFrom.SelectedDateTime.ToShortDateString();
                //row["TO"] = ldpDatePickerReturnTo.SelectedDateTime.ToShortDateString();
                //row["TO_PROCID"] = Process.SRS_BOXING;
                //row["PROCID"] = Process.SRS_BOXING;

                //if (returnLoc.Equals(""))
                //{
                //    row["SHIPTO_ID"] = null;
                //}
                //else
                //{
                //    string[] arr = returnLoc.Split('|');
                //    row["SHIPTO_ID"] = arr[3];

                //}
                //dt.Rows.Add(row);


                //try
                //{
                //    ShowLoadingIndicator();

                //    new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_RETURN_INFO_SRS", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                //    {
                //        try
                //        {
                //            if (bizException != null)
                //            {
                //                Util.AlertByBiz("DA_PRD_SEL_BOX_RETURN_INFO_SRS", bizException.Message, bizException.ToString());
                //                return;
                //            }

                //            Util.GridSetData(dgReturnInfo, bizResult, FrameOperation);

                //        }
                //        catch (Exception ex)
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        }
                //        finally
                //        {
                //            HiddenLoadingIndicator();
                //        }
                //    });

                //}
                //catch (Exception ex)
                //{
                //    //조회 오류
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2807"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //}



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }

        private void GetReturnInfoDetl()
        {
            string returnLoc = Convert.ToString(cboReturnLocationSRS.SelectedValue);

            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("FROM", typeof(string));
            dt.Columns.Add("TO", typeof(string));
            dt.Columns.Add("SHIPTO_ID", typeof(string));
            dt.Columns.Add("TO_PROCID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["FROM"] = ldpDatePickerReturnFrom.SelectedDateTime.ToShortDateString();
            row["TO"] = ldpDatePickerReturnTo.SelectedDateTime.ToShortDateString();
            row["TO_PROCID"] = Process.SRS_BOXING;
            row["PROCID"] = Process.SRS_BOXING;
            row["PRODID"] = string.IsNullOrEmpty(txtProdID.Text) ? null : txtProdID.Text;

            if (returnLoc.Equals(""))
            {
                row["SHIPTO_ID"] = null;
            }
            else
            {
                string[] arr = returnLoc.Split('|');
                row["SHIPTO_ID"] = arr[3];

            }
            dt.Rows.Add(row);


            try
            {
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_RETURN_INFO_SRS", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_BOX_RETURN_INFO_SRS", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.GridSetData(dgReturnInfo, bizResult, FrameOperation);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        txtProdID.Text = "";
                    }
                });

            }
            catch (Exception ex)
            {
                //조회 오류
                Util.MessageException(ex);
            }
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


        #region [Pallet 이력 카드 발행]
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK") < 0)
                {
                    Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgPalletList, "CHK") >= 2)
                {
                    Util.MessageValidation("PSS9118");  //다중 선택시 작업 완료할 수 없습니다.  
                    return;
                }

                //발행하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2873"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataRow[] drChk = Util.gridGetChecked(ref dgPalletList, "CHK");
                            string sPallet_ID = drChk[0]["BOXID"].ToString();
                            //double dQty = Convert.ToDouble(drChk[0][" WIPQTY"].ToString());
                            string sQty = drChk[0]["TOTALQTY"].ToString();

                            decimal dQty = Math.Round(Convert.ToDecimal(drChk[0]["TOTALQTY"].ToString()), 2);

                            string sPallet_date = drChk[0]["PACKDTTM"].ToString();
                            string pack_note = drChk[0]["PACK_NOTE"].ToString();



                            //string sVld = sPallet_date.ToString().Substring(0, 4) + "-" + sPallet_date.ToString().Substring(4, 2) + "-" + sPallet_date.ToString().Substring(6, 2);

                            DataTable dtPackingCard = new DataTable();

                            //dtPackingCard.Columns.Add("TITLE", typeof(string));
                            dtPackingCard.Columns.Add("PALLETID", typeof(string));
                            dtPackingCard.Columns.Add("BARCODE", typeof(string));
                            dtPackingCard.Columns.Add("TOTAL_QTY", typeof(string));
                            dtPackingCard.Columns.Add("TOTALQTY", typeof(string));
                            dtPackingCard.Columns.Add("PALLET_DATE", typeof(string));
                            dtPackingCard.Columns.Add("PALLETDATE", typeof(string));
                            dtPackingCard.Columns.Add("NOTE", typeof(string));
                            dtPackingCard.Columns.Add("NOTE01", typeof(string));
                            dtPackingCard.Columns.Add("PRINT_DATE", typeof(string));
                            dtPackingCard.Columns.Add("PRINTDATE", typeof(string));
                            dtPackingCard.Columns.Add("LOT_ID", typeof(string));
                            dtPackingCard.Columns.Add("T_01", typeof(string));
                            dtPackingCard.Columns.Add("T_02", typeof(string));
                            dtPackingCard.Columns.Add("T_03", typeof(string));
                            dtPackingCard.Columns.Add("T_04", typeof(string));
                            dtPackingCard.Columns.Add("T_05", typeof(string));
                            dtPackingCard.Columns.Add("CNT", typeof(string));
                            dtPackingCard.Columns.Add("ROLL_CNT", typeof(string));

                            DataRow drCrad = null;

                            drCrad = dtPackingCard.NewRow();

                            drCrad.ItemArray = new object[] {
                                                  //ObjectDic.Instance.GetObjectName("PALLETID");,
                                                  sPallet_ID,
                                                  sPallet_ID,
                                                  ObjectDic.Instance.GetObjectName("총수량"),
                                                  String.Format("{0:#,##0}", (dQty)),
                                                  ObjectDic.Instance.GetObjectName("PALLET구성일시"),
                                                  sPallet_date,
                                                  ObjectDic.Instance.GetObjectName("특이사항"),
                                                  pack_note,
                                                  ObjectDic.Instance.GetObjectName("발행일시"),
                                                  System.DateTime.Now,
                                                  "Pancake ID",
                                                  ObjectDic.Instance.GetObjectName("모델명"),
                                                  ObjectDic.Instance.GetObjectName("버전"),
                                                  ObjectDic.Instance.GetObjectName("단위"),
                                                  ObjectDic.Instance.GetObjectName("재공"),
                                                  ObjectDic.Instance.GetObjectName("유효기간"),
                                                  ObjectDic.Instance.GetObjectName("총 롤 수"),
                                                  dgPancakeList.GetRowCount()
                                                };

                            dtPackingCard.Rows.Add(drCrad);

                            DataTable dtLotInfo = new DataTable();
                            dtLotInfo.Columns.Add("NO", typeof(string));
                            dtLotInfo.Columns.Add("LOTID", typeof(string));
                            dtLotInfo.Columns.Add("MODLID", typeof(string));
                            dtLotInfo.Columns.Add("PROD_VER_CODE", typeof(string));
                            dtLotInfo.Columns.Add("UNIT", typeof(string));
                            dtLotInfo.Columns.Add("WIPQTY", typeof(string));
                            dtLotInfo.Columns.Add("VLD_DATE", typeof(string));
                            dtLotInfo.Columns.Add("HOLD", typeof(string));
                 

                            for (int i = 0; i < dgPancakeList.GetRowCount(); i++)
                            {
                                DataRow drLotlist = dtLotInfo.NewRow();

                                drLotlist["NO"] = i + 1;
                                drLotlist["LOTID"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "LOTID");
                                drLotlist["MODLID"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "MODLID");
                                drLotlist["PROD_VER_CODE"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "PROD_VER_CODE");
                                drLotlist["UNIT"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "UNIT_CODE");
                                //drLotlist["WIPQTY"] = Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "WIPQTY")), 2);
                                drLotlist["WIPQTY"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "WIPQTY")), 2)));
                                drLotlist["VLD_DATE"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "VLD_DATE");
                                drLotlist["HOLD"] = DataTableConverter.GetValue(dgPancakeList.Rows[i].DataItem, "WIPHOLD");
                                dtLotInfo.Rows.Add(drLotlist);
                            }

                            LGC.GMES.MES.BOX001.Report_SRS_Tag rs = new LGC.GMES.MES.BOX001.Report_SRS_Tag();
                            rs.FrameOperation = this.FrameOperation;

                            if (rs != null)
                            {
                                // 태그 발행 창 화면에 띄움.
                                object[] Parameters = new object[3];
                                Parameters[0] = "Report_SRS_Tag";
                                Parameters[1] = dtPackingCard;
                                Parameters[2] = dtLotInfo;
                                
                                C1WindowExtension.SetParameters(rs, Parameters);

                                rs.Closed += new EventHandler(Print_Result);
                                // 팝업 화면 숨겨지는 문제 수정.
                                //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                                grdMain.Children.Add(rs);
                                rs.BringToFront();
                            }

                        }
                        catch (Exception ex)
                        {
                            Util.AlertInfo(ex.ToString());
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
                return;
            }
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_SRS_Tag wndPopup = sender as LGC.GMES.MES.BOX001.Report_SRS_Tag;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        private void dgPackOutChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgPackOut.SelectedIndex = idx;            
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgPackOut, "CHK") < 0)
                {
                    Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                if (_Util.GetDataGridCheckCnt(dgPackOut, "CHK") >= 2)
                {
                    Util.MessageValidation("PSS9118");  //다중 선택시 작업 완료할 수 없습니다.  
                    return;
                }

                //발행하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2873"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataRow[] drChk = Util.gridGetChecked(ref dgPackOut, "CHK");
                            string sPallet_ID = drChk[0]["BOXID"].ToString();
                            string sPallet_date = string.Empty;
                            string sNote = drChk[0]["PACK_NOTE"].ToString();

                            decimal dQty = 0;
                            decimal dSum = 0;
                            int iCnt = 0;

                            DataTable dtLotInfo = new DataTable();
                            dtLotInfo.Columns.Add("NO", typeof(string));
                            dtLotInfo.Columns.Add("LOTID", typeof(string));
                            dtLotInfo.Columns.Add("MODLID", typeof(string));
                            dtLotInfo.Columns.Add("PROD_VER_CODE", typeof(string));
                            dtLotInfo.Columns.Add("UNIT", typeof(string));
                            dtLotInfo.Columns.Add("WIPQTY", typeof(string));
                            dtLotInfo.Columns.Add("VLD_DATE", typeof(string));
                            dtLotInfo.Columns.Add("HOLD", typeof(string));

                            for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                            {
                                if (sPallet_ID == DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString())
                                {
                                    DataRow drLotlist = dtLotInfo.NewRow();

                                    iCnt = iCnt + 1;

                                    drLotlist["NO"] = iCnt;
                                    drLotlist["LOTID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "LOTID");
                                    drLotlist["MODLID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "MODLID");
                                    drLotlist["PROD_VER_CODE"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "PROD_VER_CODE");
                                    drLotlist["UNIT"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "UNIT_CODE");
                                    //drLotlist["WIPQTY"] = Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "MOVE_ORD_QTY")), 2);
                                    drLotlist["WIPQTY"] = drLotlist["WIPQTY"] = String.Format("{0:#,##0}", (Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "WIPQTY")), 2)));
                                    dQty = Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "WIPQTY")), 2);
                                    drLotlist["VLD_DATE"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "VLD_DATE");
                                    drLotlist["HOLD"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "WIPHOLD");

                                    sPallet_date = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "PACKDTTM").ToString();

                                    dtLotInfo.Rows.Add(drLotlist);

                                    dSum = dQty + dSum;
                                }
                            }


                            DataTable dtPackingCard = new DataTable();

                            //dtPackingCard.Columns.Add("TITLE", typeof(string));
                            dtPackingCard.Columns.Add("PALLETID", typeof(string));
                            dtPackingCard.Columns.Add("BARCODE", typeof(string));
                            dtPackingCard.Columns.Add("TOTAL_QTY", typeof(string));
                            dtPackingCard.Columns.Add("TOTALQTY", typeof(string));
                            dtPackingCard.Columns.Add("PALLET_DATE", typeof(string));
                            dtPackingCard.Columns.Add("PALLETDATE", typeof(string));
                            dtPackingCard.Columns.Add("NOTE", typeof(string));
                            dtPackingCard.Columns.Add("NOTE01", typeof(string));
                            dtPackingCard.Columns.Add("PRINT_DATE", typeof(string));
                            dtPackingCard.Columns.Add("PRINTDATE", typeof(string));
                            dtPackingCard.Columns.Add("LOT_ID", typeof(string));
                            dtPackingCard.Columns.Add("T_01", typeof(string));
                            dtPackingCard.Columns.Add("T_02", typeof(string));
                            dtPackingCard.Columns.Add("T_03", typeof(string));
                            dtPackingCard.Columns.Add("T_04", typeof(string));
                            dtPackingCard.Columns.Add("T_05", typeof(string));
                            dtPackingCard.Columns.Add("CNT", typeof(string));
                            dtPackingCard.Columns.Add("ROLL_CNT", typeof(string));

                            DataRow drCrad = null;

                            drCrad = dtPackingCard.NewRow();

                            drCrad.ItemArray = new object[] {
                                                  //ObjectDic.Instance.GetObjectName("PALLETID");,
                                                  sPallet_ID,
                                                  sPallet_ID,
                                                  ObjectDic.Instance.GetObjectName("총수량"),
                                                  //Convert.ToString(dSum),
                                                  String.Format("{0:#,##0}", (dSum)),
                                                  ObjectDic.Instance.GetObjectName("PALLET구성일시"),
                                                  sPallet_date,
                                                  ObjectDic.Instance.GetObjectName("특이사항"),
                                                  sNote,
                                                  ObjectDic.Instance.GetObjectName("발행일시"),
                                                  System.DateTime.Now,
                                                  "Pancake ID",
                                                  ObjectDic.Instance.GetObjectName("모델명"),
                                                  ObjectDic.Instance.GetObjectName("버전"),
                                                  ObjectDic.Instance.GetObjectName("단위"),
                                                  ObjectDic.Instance.GetObjectName("재공"),
                                                  ObjectDic.Instance.GetObjectName("유효기간"),
                                                  ObjectDic.Instance.GetObjectName("총 롤 수"),
                                                  iCnt
                                                };

                            dtPackingCard.Rows.Add(drCrad);





                            LGC.GMES.MES.BOX001.Report_SRS_Tag rs = new LGC.GMES.MES.BOX001.Report_SRS_Tag();
                            rs.FrameOperation = this.FrameOperation;

                            if (rs != null)
                            {
                                // 태그 발행 창 화면에 띄움.
                                object[] Parameters = new object[3];
                                Parameters[0] = "Report_SRS_Tag";
                                Parameters[1] = dtPackingCard;
                                Parameters[2] = dtLotInfo;

                                C1WindowExtension.SetParameters(rs, Parameters);

                                rs.Closed += new EventHandler(Print_Result);
                                // 팝업 화면 숨겨지는 문제 수정.
                                //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                                grdMain.Children.Add(rs);
                                rs.BringToFront();
                            }

                        }
                        catch (Exception ex)
                        {
                            Util.AlertInfo(ex.ToString());
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.ToString());
                return;
            }
        }

     

        private void cboOutProductSRS_Loaded(object sender, RoutedEventArgs e)
        {
            string[] sFilter0 = { "SR", txtModelID.Text };
            combo.SetCombo(cboOutProductSRS, CommonCombo.ComboStatus.ALL, sFilter: sFilter0, sCase: "cboOutModelSRS"); //출고된 모델만 나오게
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("PACK_NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["PACK_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inBox = indataSet.Tables.Add("INBOX");
                        inBox.Columns.Add("BOXID", typeof(string));

                        List<int> idxs = new List<int>();

                        for (int i = 0; i < dgPalletList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                row = inBox.NewRow();
                                row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[i].DataItem, "BOXID"));
                                inBox.Rows.Add(row);

                                idxs.Add(i);

                            }
                        }
                       
                            ShowLoadingIndicator();

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PACK_NOTE_FOR_SRS", "INDATA,INBOX", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    Util.MessageInfo("SFU1270"); //저장되었습니다.
                                    rtxRemark.Document.Blocks.Clear();

                                    SearchData();
                                    for (int i = 0; i < idxs.Count; i++)
                                    {
                                        DataTableConverter.SetValue(dgPalletList.Rows[idxs[i]].DataItem, "CHK", 1);
                                        GetPancakeList(idxs[i]);

                                    }
                                    


                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);

                                }
                                finally
                                {
                                    HiddenLoadingIndicator();
                                }
                            }, indataSet);

                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
            

        }

     

    }
}
