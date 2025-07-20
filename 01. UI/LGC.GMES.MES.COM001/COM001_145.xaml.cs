using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
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


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_141.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_145 : UserControl, IWorkArea
    {
        string sEqsg_ID = string.Empty;
        string sRownum_ID = string.Empty;
        string sBiz_Type = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_145()
        {
            InitializeComponent();

            Loaded += UserControl_Loaded;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // InitOpenSearch();
            dtpFDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1 - DateTime.Now.Day);
            InitCombo();
            Loaded -= UserControl_Loaded;
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");
        }

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { LoginInfo.CFG_AREA_ID };
                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
                cboArea.SelectedIndex = 0;
                //라인
                C1ComboBox[] cboLineParent = { cboArea };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);

                String[] sFilterType = { LoginInfo.LANGID, "RTLS_SCAN_YN" };
                _combo.SetCombo(cboScanYn, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilterType);
                cboScanYn.SelectedIndex = 0;

                sFilterType[1] = "RTLS_STOCK_PERIOD";
                _combo.SetCombo(cboPeriod, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilterType);
                cboScanYn.SelectedIndex = 0;

                fn_Init_RtlsMultiCmb();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void fn_Init_RtlsMultiCmb()
        {
            Init_cboEmType(cboEmType);
            Init_cboStatcode(cboStatcode);
            Init_cboJudgResult(cboJudgResult);
        }



        private void Init_cboEmType(MultiSelectionBox cbo)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_LINEOUT_TYPE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            //cbo.CheckAll();
        }

        private void Init_cboStatcode(MultiSelectionBox cbo)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_MMD_EM_STAT_CODE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            //cbo.CheckAll();
        }
        private void Init_cboJudgResult(MultiSelectionBox cbo)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "LOT_USG_TYPE_CODE";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            //cbo.CheckAll();
        }

        //private void InitOpenSearch()
        //{

        //    if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
        //    {

        //        Array ary = FrameOperation.Parameters;
        //        sEqsg_ID = ary.GetValue(0).ToString();
        //        sRownum_ID = ary.GetValue(1).ToString();
        //        sBiz_Type = ary.GetValue(2).ToString();

        //        //btnSearch_Click(null, null);
        //     }
        //}


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string[] Period;
            string F_Period = string.Empty;
            string T_Period = string.Empty;


            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("EM_DTTM_ST", typeof(DateTime));
            RQSTDT.Columns.Add("EM_DTTM_ED", typeof(DateTime));
            RQSTDT.Columns.Add("EM_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string)); // 카테고리 PORT_OUT, OVER_TAKE...
            RQSTDT.Columns.Add("PROCID_CAUSE", typeof(string)); //불량 발생 공정
            RQSTDT.Columns.Add("TYPE", typeof(string)); //제품타입
            RQSTDT.Columns.Add("SCAN_FLAG", typeof(string)); //스캔유무
            RQSTDT.Columns.Add("JUDG_RSLT", typeof(string)); //판정구분
            RQSTDT.Columns.Add("F_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("T_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품코드

            if (Util.NVC(cboPeriod.SelectedValue.ToString()) != "")
            {
                Period = Util.NVC(cboPeriod.SelectedValue.ToString()).Split('~');
                if (Period[0] == "")
                {
                    //if(Period[1] == "15")
                    //{

                    F_Period = "0";

                    T_Period = "15";
                    //}
                    //else
                    //{
                    //    F_Period = "90";
                    //    T_Period = "99999";
                    //}
                }
                else if (Period[1] == "")
                {

                    F_Period = "90";
                    T_Period = "99999";
                }
                else
                {
                    F_Period = Period[0];
                    T_Period = Period[1];
                }

            }


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            dr["EM_DTTM_ST"] = dtpFDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
            dr["EM_DTTM_ED"] = dtpTDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
            dr["EM_TYPE_CODE"] = Util.NVC(cboEmType.SelectedItemsToString) == "" ? null : cboEmType.SelectedItemsToString;
            dr["EM_STAT_CODE"] = cboStatcode.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboStatcode.SelectedItemsToString) == "" ? null : cboStatcode.SelectedItemsToString;
            dr["PROCID_CAUSE"] = Util.NVC(cboProcess.SelectedItemsToString) == "" ? null : cboProcess.SelectedItemsToString;
            dr["TYPE"] = Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : cboPrdtClass.SelectedItemsToString;
            dr["SCAN_FLAG"] = Util.NVC(cboScanYn.SelectedValue.ToString()) == "" ? null : cboScanYn.SelectedValue.ToString();
            dr["JUDG_RSLT"] = cboJudgResult.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked ||  Util.NVC(cboJudgResult.SelectedItemsToString) == "" ? null : cboJudgResult.SelectedItemsToString;
            dr["F_PERIOD"] = Util.NVC(cboPeriod.SelectedValue.ToString()) == "" ? null : F_Period;
            dr["T_PERIOD"] = Util.NVC(cboPeriod.SelectedValue.ToString()) == "" ? null : T_Period;
            dr["PRODID"] = Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString;




            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_SEL_EM_LOT_MNGT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.GridSetData(dgPalletInfo, dt, FrameOperation, true);
                    Util.Alert("SFU1905");
                    return;
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dt.Rows.Count));

                Util.GridSetData(dgPalletInfo, dt, FrameOperation);

            });
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgPalletInfo);
        }


        private void dgPalletInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //dgPalletInfo.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }
            //    if (e.Cell.Row.Index == 2)
            //    {
            //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
            //    }
            //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DELAY_YN")) == "Y")
            //    {
            //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
            //        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
            //    }

            //}));
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgPalletInfo);
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboProcess.isAllUsed = true;
                    SetcboProcess();
                    SetcboPrdtClass();
                    SetcboProduct();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboPrdtClass.isAllUsed = true;
                    SetcboPrdtClass();
                    SetcboProduct();


                }));
            }
            catch
            {
            }
        }
        private void cboPrdtClass_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboProduct.isAllUsed = true;
                    SetcboProduct();


                }));
            }
            catch
            {
            }
        }


        private void SetcboProcess()
        {
            try
            {
                this.cboProcess.SelectionChanged -= new System.EventHandler(this.cboProcess_SelectionChanged);
                string sSelectedValue = cboProcess.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboProcess.CheckAll();
                //cboProcess.Check(0);

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboProcess.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //    else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_PROC_ID)
                //    {
                //        cboProcess.Check(i);
                //        break;
                //    }
                //}
                this.cboProcess.SelectionChanged += new System.EventHandler(this.cboProcess_SelectionChanged);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SetcboPrdtClass()
        {
            try
            {

                this.cboPrdtClass.SelectionChanged -= new System.EventHandler(this.cboPrdtClass_SelectionChanged);
                string sSelectedValue = cboPrdtClass.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboPrdtClass.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboPrdtClass.CheckAll();
                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboPrdtClass.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //}
                this.cboPrdtClass.SelectionChanged += new System.EventHandler(this.cboPrdtClass_SelectionChanged);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboProduct()
        {
            try
            {
                string sSelectedValue = cboProduct.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = cboPrdtClass.SelectedItemsToString == "" ? null : cboPrdtClass.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);
                //cboProduct.CheckAll();

                //for (int i = 0; i < dtResult.Rows.Count; i++)
                //{
                //    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                //    {
                //        for (int j = 0; j < sSelectedList.Length; j++)
                //        {
                //            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                //            {
                //                cboProduct.Check(i);
                //                break;
                //            }
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


    }

}
