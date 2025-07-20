/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
              DEVELOPER : Initial Created.
  2023.06.28  정재홍    : [E20230616-001580] - electrode MES UI time set
  2023.09.07  양영재    : [E20230905-000417] -> Skid와 Box에 대해서도 Excel 업로드 기능 추가
 
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

using System.IO;
using C1.WPF.Excel;
using System.Globalization;
using System.Configuration;
using System.Collections.Specialized;

using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_032.xaml에 대한 상호 작용 논리
    /// 조용수 사원  [요청번호]C20180601_03986 
    /// </summary>
    public partial class ELEC001_032 : UserControl, IWorkArea
    {
        string _SCANTYPE = "";
        bool ExcelUp_YN = true;

        TextBox _SCANID = new TextBox();
        TextBox _SCANIDCANCEL = new TextBox();
        C1ComboBox _CBED = new C1ComboBox();
        C1ComboBox _CBHISTYPE = new C1ComboBox();
        Util _Util = new Util();
        public ELEC001_032()
        {
            InitializeComponent();
            InitCombo();
            _SCANID = txtScanId;
            _CBED = cboed;
            _CBHISTYPE = cboChangeMkt;
            _SCANIDCANCEL = txtScanIdCancel;

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        #region [ 콤보박스 만들기 ]
        private void InitCombo()
        {

            if (_SCANTYPE.Equals("LOT"))
            {
                if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
                    dgEdlist.Columns["LOTID"].Visibility = Visibility.Visible;
                if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
                    dgEdlist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
                    dgEdlist.Columns["BOXID"].Visibility = Visibility.Collapsed;

                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
                    dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Visible;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
                    dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Collapsed;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
                    dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Collapsed;
            }

            if (_SCANTYPE.Equals("SKID"))
            {
                if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
                    dgEdlist.Columns["CSTID"].Visibility = Visibility.Visible;
                if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
                    dgEdlist.Columns["LOTID"].Visibility = Visibility.Collapsed;
                if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
                    dgEdlist.Columns["BOXID"].Visibility = Visibility.Collapsed;

                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
                    dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Visible;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
                    dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Collapsed;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
                    dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Collapsed;
            }

            if (_SCANTYPE.Equals("BOX"))
            {
                if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
                    dgEdlist.Columns["BOXID"].Visibility = Visibility.Visible;
                if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
                    dgEdlist.Columns["LOTID"].Visibility = Visibility.Collapsed;
                if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
                    dgEdlist.Columns["CSTID"].Visibility = Visibility.Collapsed;

                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
                    dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Visible;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
                    dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Collapsed;
                if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
                    dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //내/외수 선택

            String[] sFilter1 = { "MKT_TYPE_CODE"};
            //string[] sFilter2 = { "ALL" };
            _combo.SetCombo(cboed, CommonCombo.ComboStatus.SELECT,sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { "MKT_HIST_TYPE" };
            _combo.SetCombo(cboChangeMkt, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            ////SHOP
            //C1ComboBox[] cboFromAreaChild = { cboAreaFrom };
            //_combo.SetCombo(cboShopFrom, CommonCombo.ComboStatus.NONE, cbChild: cboFromAreaChild, sCase: "FROMSHOP");

            ////동
            //C1ComboBox[] cboFromAreaParent = { cboShopFrom };
            //_combo.SetCombo(cboAreaFrom, CommonCombo.ComboStatus.NONE, cbParent: cboFromAreaParent, sCase: "AREA_NO_AUTH", sFilter: sFilter2);

        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion
        //Lot 유형스캔
        private void txtScanId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int msgCount = 0;
                    int msgCount2 = 0;
                    
                    _Util.SetDataGridMergeExtensionCol(dgEdlist, new string[] { "BOXID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("SCANFLAG", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = txtScanId.Text;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["SCANFLAG"] = _SCANTYPE;

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MKT_CHANGE_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows[0]["MKT_TYPE_CODE"].ToString() == "E")
                    {
                        cboed.SelectedValue = "D";
                    }
                    if (dtResult.Rows[0]["MKT_TYPE_CODE"].ToString() == "D")
                    {
                        cboed.SelectedValue = "E";
                    }

                    DataTable dtData = new DataTable();
                    if (dgEdlist.ItemsSource != null)
                    {
                        dtData = DataTableConverter.Convert(dgEdlist.ItemsSource);
                    }

                    for (int k = 0; k < dtData.Rows.Count; k++)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (dtData.Rows.Count > 0)
                            {
                                if (dtData.Rows[k]["LOTID"].ToString() == dtResult.Rows[i]["LOTID"].ToString())
                                {
                                    msgCount2++;
                                    if (msgCount2 == 1)
                                    {
                                        Util.MessageValidation("SFU2014");
                                        if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() == "E")
                                        {
                                            cboed.SelectedValue = "D";
                                        }
                                        if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() == "D")
                                        {
                                            cboed.SelectedValue = "E";
                                        }
                                        return;
                                    }
                                }

                                if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() != dtResult.Rows[i]["MKT_TYPE_CODE"].ToString())
                                {
                                    msgCount++;
                                    if (msgCount == 1)
                                    {
                                        Util.MessageValidation("SFU4271");
                                        if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() == "E")
                                        {
                                            cboed.SelectedValue = "D";
                                        }
                                        if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() == "D")
                                        {
                                            cboed.SelectedValue = "E";
                                        }
                                        return;
                                    }

                                }
                            }
                        }
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgEdlist.ItemsSource);
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgEdlist, dtInfo, FrameOperation);
                    msgCount = 0;
                    msgCount2 = 0;
                    _SCANID.Text = "";
                    _SCANID.Focus();

                    DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                    DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
                    DataGridAggregatesCollection dat = new DataGridAggregatesCollection();
                    DataGridAggregateSum dagsum = new DataGridAggregateSum();
                    DataGridAggregateCount dgcount = new DataGridAggregateCount();
                    DataGridAggregateDistinct dgdiscount = new DataGridAggregateDistinct();
                    dagsum.ResultTemplate = dgEdlist.Resources["ResultTemplate"] as DataTemplate;
                    dac.Add(dagsum);
                    daq.Add(dgcount);
                    dat.Add(dgdiscount);

                    if (_SCANTYPE.Equals("BOX"))
                    {
                        DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                        DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["BOXID"], dat);
                    }

                    if (_SCANTYPE.Equals("SKID"))
                    {
                        DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                        DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["CSTID"], dat);
                    }

                    if (_SCANTYPE.Equals("LOT"))
                    {
                        DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //초기화 버튼 클릭
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgEdlist);
            txtScanId.Clear();
            _CBED.SelectedIndex = 0;
        }
        //저장 버튼 클릭
        private void btnSaveMarketType_Click(object sender, RoutedEventArgs e)
        {
            try
            { 

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow drdate = RQSTDT.NewRow();
                drdate["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(drdate);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE_DATETIME", "RQSTDT", "RSLTDT", RQSTDT);
                
                //시장유형 없을 경우
                if(_CBED.SelectedIndex == 0)
                {
                    Util.MessageValidation("SFU4371"); //시장 유형을 선택하세요
                    return;
                }
                
                if (DataTableConverter.GetValue(dgEdlist.Rows[0].DataItem, "MKT_TYPE_CODE").ToString() == _CBED.SelectedValue.ToString())
                {
                    Util.MessageValidation("SFU4929"); //등록된 시장유형과 변경하려는 시장유형이 같습니다.                    
                    return;
                }
                
                DataSet indataSet = new DataSet();
                DataTable indata = indataSet.Tables.Add("INDATA");
                indata.Columns.Add("SRCTYPE", typeof(string));
                indata.Columns.Add("USERID", typeof(string));
                indata.Columns.Add("MKT_TYPE_CODE", typeof(string));
                indata.Columns.Add("CALDATE", typeof(string));

                DataRow row = indata.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["MKT_TYPE_CODE"] = cboed.SelectedValue.ToString();
                //row["MKT_TYPE_CODE"] = DataTableConverter.GetValue(dgEdlist.Rows[0].DataItem, "MKT_TYPE_CODE").ToString();
                row["CALDATE"] = dtResult.Rows[0]["CALDATE"].ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable lot = indataSet.Tables.Add("INLOT");
                lot.Columns.Add("LOTID", typeof(String));

                    for (int i = 0; i < dgEdlist.Rows.Count -1; i++)
                    {
                      
                        row = lot.NewRow();
                        row["LOTID"] = DataTableConverter.GetValue(dgEdlist.Rows[i].DataItem, "LOTID").ToString();
                        indataSet.Tables["INLOT"].Rows.Add(row);
                    
                    }
                
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MKT_CHANGE_LOT", "INDATA,INLOT ", null, indataSet);
                Util.MessageValidation("SFU1166");
                Util.gridClear(dgEdlist);
                _SCANID.Text = "";
                _SCANID.Focus();
                _CBED.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);                
            }
        }

        private void rdoLot_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "LOT";
            Util.gridClear(dgEdlist);
            _SCANID.Text = "";
            _SCANID.Focus();
            _CBED.SelectedIndex = 0;

            if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
            {
                dgEdlist.Columns["LOTID"].Visibility = Visibility.Visible;
                btnExcelUpload.IsEnabled = true;
            }

            if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
                dgEdlist.Columns["CSTID"].Visibility = Visibility.Collapsed;
            if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
                dgEdlist.Columns["BOXID"].Visibility = Visibility.Collapsed;
        }

        private void rdoSkid_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "SKID";
            Util.gridClear(dgEdlist);
            _SCANID.Text = "";
            _SCANID.Focus();
            _CBED.SelectedIndex = 0;

            if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
                dgEdlist.Columns["LOTID"].Visibility = Visibility.Visible;
            if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
                dgEdlist.Columns["CSTID"].Visibility = Visibility.Visible;
            if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
            {
                dgEdlist.Columns["BOXID"].Visibility = Visibility.Collapsed;
                btnExcelUpload.IsEnabled = true;
            }
        }

        private void rdoBoxid_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "BOX";
            Util.gridClear(dgEdlist);
            _SCANID.Text = "";
            _SCANID.Focus();
            _CBED.SelectedIndex = 0;

            if (dgEdlist != null && dgEdlist.Columns.Contains("LOTID"))
                dgEdlist.Columns["LOTID"].Visibility = Visibility.Visible;
            if (dgEdlist != null && dgEdlist.Columns.Contains("CSTID"))
            {
                dgEdlist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                btnExcelUpload.IsEnabled = true;
            }
            if (dgEdlist != null && dgEdlist.Columns.Contains("BOXID"))
                dgEdlist.Columns["BOXID"].Visibility = Visibility.Visible;
        }

        private void rdoLot2_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "LOT";
            Util.gridClear(dgEdlistCancel);
            _SCANIDCANCEL.Text = "";
            _SCANIDCANCEL.Focus();

            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
            {
                dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Visible;
                btnExcelUpload2.IsEnabled = true;
            }
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
                dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Collapsed;
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
                dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Collapsed;
        }

        private void rdoSkid2_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "SKID";
            Util.gridClear(dgEdlistCancel);
            _SCANIDCANCEL.Text = "";
            _SCANIDCANCEL.Focus();

            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
                dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Visible;
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
                dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Visible;
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
            {
                dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Collapsed;
                btnExcelUpload2.IsEnabled = false;
            }
        }

        private void rdoBoxid2_Checked(object sender, RoutedEventArgs e)
        {
            _SCANTYPE = "BOX";
            Util.gridClear(dgEdlistCancel);
            _SCANIDCANCEL.Text = "";
            _SCANIDCANCEL.Focus();

            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("LOTID"))
                dgEdlistCancel.Columns["LOTID"].Visibility = Visibility.Visible;
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("CSTID"))
            {
                dgEdlistCancel.Columns["CSTID"].Visibility = Visibility.Collapsed;
                btnExcelUpload2.IsEnabled = false;
            }
            if (dgEdlistCancel != null && dgEdlistCancel.Columns.Contains("BOXID"))
                dgEdlistCancel.Columns["BOXID"].Visibility = Visibility.Visible;
        }

        // 변경 이력 조회 탭 Click이벤트
        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgEdHistlist);
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("MKT_HIST_TYPE", typeof(string));

                // CSR : [E20230616-001580]
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("NEXT_DAY_YN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                if (txtProdid.Text != "")
                {
                    dr["PRODID"] = txtProdid.Text;
                }
                if (txtLotid.Text != "")
                {
                    dr["LOTID"] = txtLotid.Text;
                }
                if (txtCstid.Text != "")
                {
                    dr["CSTID"] = txtCstid.Text;
                }
                if (txtBoxid.Text != "")
                {
                    dr["BOXID"] = txtBoxid.Text;
                }

                if (cboChangeMkt.SelectedValue.Equals(""))
                {
                    dr["MKT_HIST_TYPE"] = "CHANGE_MKT_TYPE_CODE, CANCEL_CHANGE_MKT_TYPE_CODE";
                }
                if (cboChangeMkt.SelectedValue.Equals("CANCEL"))
                {
                    dr["MKT_HIST_TYPE"]  = "CANCEL_CHANGE_MKT_TYPE_CODE";
                }
                if (cboChangeMkt.SelectedValue.Equals("CHANGE"))
                {
                    dr["MKT_HIST_TYPE"] = "CHANGE_MKT_TYPE_CODE";
                }

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["NEXT_DAY_YN"] = (chkDate.IsChecked == true) ? "Y" : "N";

                //if (txtPjt.Text != "")
                //{
                //    dr["PRJT_NAME"] = txtPjt.Text;
                //}
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_MKT_CHANGE_LOT_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                //DataTable dtInfo = DataTableConverter.Convert(dgEdHistlist.ItemsSource);
                //dtInfo.Merge(dtResult);
                Util.GridSetData(dgEdHistlist, dtResult, FrameOperation, false);
                _Util.SetDataGridMergeExtensionCol(dgEdHistlist, new string[] { "CSTID", "BOXID" }, DataGridMergeMode.VERTICALHIERARCHI);

                //Util.GridSetData(dgEdHistlist, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //시장유형 변경 취소 조회 쿼리
        private void txtScanIdCancel_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    //if (txtLotid.Text == "")
                    //{
                    //    Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                    //    return;
                    //}
                    //_Util.SetDataGridMergeExtensionCol(dgEdlistCancel, new string[] { "BOXID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("CSTID", typeof(string));
                    RQSTDT.Columns.Add("BOXID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    //RQSTDT.Columns.Add("SCANFLAG", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    if (_SCANTYPE.Equals("LOT"))
                    {
                        dr["LOTID"] = txtScanIdCancel.Text;
                    }
                    if (_SCANTYPE.Equals("SKID"))
                    {
                        dr["CSTID"] = txtScanIdCancel.Text;
                    }
                    if (_SCANTYPE.Equals("BOX"))
                    {
                        dr["BOXID"] = txtScanIdCancel.Text;
                    }
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    //dr["SCANFLAG"] = _SCANTYPE;

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_MKT_CHANGE_LOT_CANCELL", "RQSTDT", "RSLTDT", RQSTDT);

                    DataTable dtInfo = DataTableConverter.Convert(dgEdlistCancel.ItemsSource);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(dtResult.Rows[i]["CALDATE"].ToString()))
                        {
                            // 내/외수 전환된 LOT이 아니라 취소가 불가 합니다.
                            Util.MessageValidation("SFU4994");
                            return;

                        }
                        if (dtResult.Rows[i]["ERP_CLOSE"].Equals("CLOSE"))
                        {
                            // ERP 생산실적이 마감 되었습니다.
                            Util.MessageValidation("SFU3494");
                            return;
                        }
                    }
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgEdlistCancel, dtInfo, FrameOperation);
                    _Util.SetDataGridMergeExtensionCol(dgEdlistCancel, new string[] { "CSTID", "BOXID" }, DataGridMergeMode.VERTICALHIERARCHI);

                    _SCANIDCANCEL.Text = "";
                    _SCANIDCANCEL.Focus();

                    if (dtResult.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU2060");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancelMarketType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow drdate = RQSTDT.NewRow();
                drdate["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(drdate);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE_DATETIME", "RQSTDT", "RSLTDT", RQSTDT);


                DataSet indataSet = new DataSet();
                DataTable indata = indataSet.Tables.Add("INDATA");
                indata.Columns.Add("SRCTYPE", typeof(string));
                indata.Columns.Add("USERID", typeof(string));
                indata.Columns.Add("CALDATE", typeof(string));

                DataRow row = indata.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["CALDATE"] = dtResult.Rows[0]["CALDATE"].ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable lot = indataSet.Tables.Add("INLOT");
                lot.Columns.Add("LOTID", typeof(String));

                for (int i = 0; i < dgEdlistCancel.Rows.Count; i++)
                {
                    row = lot.NewRow();
                    row["LOTID"] = DataTableConverter.GetValue(dgEdlistCancel.Rows[i].DataItem, "LOTID").ToString();
                    indataSet.Tables["INLOT"].Rows.Add(row);
                }
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MKT_CHANGE_LOT_CANCEL", "INDATA,INLOT ", null, indataSet);
                Util.MessageValidation("SFU1166");
                Util.gridClear(dgEdlistCancel);
                _SCANIDCANCEL.Text = "";
                _SCANIDCANCEL.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReset2_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgEdlistCancel);
            txtScanIdCancel.Clear();
        }

        private void Tablnx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tablnx.SelectedIndex == 0)
                Util.gridClear(dgEdlistCancel);
            else if (Tablnx.SelectedIndex == 1)
                Util.gridClear(dgEdlist);
        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        private void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);

                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);

                XLSheet sheet = book.Sheets[sheetNo];
                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                //Excel Loop시 Break 처리(WIPHOLD, 기타등..)
                ExcelUp_YN = true;


                if (sheet.Rows.Count > 100 && _SCANTYPE =="BOX")
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("SFU9900", new String[] { _SCANTYPE, "100" });
                    return;
                }

                if (sheet.Rows.Count > 200 && _SCANTYPE == "SKID")
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("SFU9900", new String[] { _SCANTYPE, "200" });
                    return;
                }

                

                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (ExcelUp_YN == false)
                        break;

                    //Excel Upload LOT Grid 추가
                    if (sheet.GetCell(rowInx, 0) != null && !sheet.GetCell(rowInx, 0).Text.Trim().Equals(""))
                    {
                        GetLotCheck(Util.NVC(sheet.GetCell(rowInx, 0).Text));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void GetLotCheck(string LotId)
        {
            try
            {
                int msgCount = 0;
                int msgCount2 = 0;

                _Util.SetDataGridMergeExtensionCol(dgEdlist, new string[] { "BOXID", "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("SCANFLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotId;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SCANFLAG"] = _SCANTYPE;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MKT_CHANGE_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows[0]["MKT_TYPE_CODE"].ToString() == "E")
                {
                    cboed.SelectedValue = "D";
                }
                if (dtResult.Rows[0]["MKT_TYPE_CODE"].ToString() == "D")
                {
                    cboed.SelectedValue = "E";
                }

                DataTable dtData = new DataTable();
                if (dgEdlist.ItemsSource != null)
                {
                    dtData = DataTableConverter.Convert(dgEdlist.ItemsSource);
                }

                for (int k = 0; k < dtData.Rows.Count; k++)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtData.Rows.Count > 0)
                        {
                            if (dtData.Rows[k]["LOTID"].ToString() == dtResult.Rows[i]["LOTID"].ToString())
                            {
                                msgCount2++;
                                if (msgCount2 == 1)
                                {
                                    //Excel 중복 LOT 메시지 
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1376", new object[] { LotId }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        Util.gridClear(dgEdlist);
                                        _CBED.SelectedIndex = 0;
                                    });
                                    ExcelUp_YN = false;
                                    return;
                                }
                            }

                            if (dtData.Rows[0]["MKT_TYPE_CODE"].ToString() != dtResult.Rows[i]["MKT_TYPE_CODE"].ToString())
                            {
                                msgCount++;
                                if (msgCount == 1)
                                {
                                    //Excel 파일에 내수/수출이 동시에 있을시 메시지 
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8332", new object[] { LotId }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                    {
                                        Util.gridClear(dgEdlist);
                                        _CBED.SelectedIndex = 0;
                                    });
                                    ExcelUp_YN = false;
                                    return;
                                }
                            }
                        }
                    }
                }

                DataTable dtInfo = DataTableConverter.Convert(dgEdlist.ItemsSource);
                dtInfo.Merge(dtResult);
                Util.GridSetData(dgEdlist, dtInfo, FrameOperation);
                msgCount = 0;
                msgCount2 = 0;
                _SCANID.Text = "";
                _SCANID.Focus();

                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
                DataGridAggregatesCollection dat = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                DataGridAggregateCount dgcount = new DataGridAggregateCount();
                DataGridAggregateDistinct dgdiscount = new DataGridAggregateDistinct();
                dagsum.ResultTemplate = dgEdlist.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                daq.Add(dgcount);
                dat.Add(dgdiscount);

                if (_SCANTYPE.Equals("BOX"))
                {
                    DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                    DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["BOXID"], dat);
                }

                if (_SCANTYPE.Equals("SKID"))
                {
                    DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                    DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["CSTID"], dat);
                }

                if (_SCANTYPE.Equals("LOT"))
                {
                    DataGridAggregate.SetAggregateFunctions(dgEdlist.Columns["LOTID"], daq);
                }
            }
            catch (Exception ex)
            {
                ExcelUp_YN = false;
                Util.gridClear(dgEdlist);
                _CBED.SelectedIndex = 0;

                Util.MessageException(ex);
            }
        }

        private void ExcelUpload_Click2(object sender, RoutedEventArgs e)
        {
            GetExcelCancel();
        }

        private void GetExcelCancel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcelCancel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcelCancel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);

                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);

                XLSheet sheet = book.Sheets[sheetNo];
                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                ExcelUp_YN = true;
                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (ExcelUp_YN == false)
                        break;

                    //Excel Upload LOT Grid 추가
                    if (sheet.GetCell(rowInx, 0) != null && !sheet.GetCell(rowInx, 0).Text.Trim().Equals(""))
                    {
                        GetLotCancel(Util.NVC(sheet.GetCell(rowInx, 0).Text));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetLotCancel(string LotId)
        {
            try
            {
                DataTable dtCancel = new DataTable();
                if (dgEdlistCancel.ItemsSource != null)
                {
                    dtCancel = DataTableConverter.Convert(dgEdlistCancel.ItemsSource);
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("SCANFLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LotId;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_MKT_CHANGE_LOT_CANCELL", "RQSTDT", "RSLTDT", RQSTDT);

                //내외수 변경 취소 DATA 없을시
                if (dtResult.Rows.Count < 1)
                {
                    ExcelUp_YN = false;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8334", new object[] { LotId }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        Util.gridClear(dgEdlistCancel);
                        ExcelUp_YN = false;
                        return;
                    });
                }

                //Excel 중복 LOT 메시지 
                for (int k = 0; k < dtCancel.Rows.Count; k++)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtCancel.Rows[k]["LOTID"].ToString() == dtResult.Rows[i]["LOTID"].ToString())
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1376", new object[] { LotId }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                Util.gridClear(dgEdlistCancel);
                            });
                            ExcelUp_YN = false;
                            return;
                        }
                    }
                }

                //Grid 추가 
                DataTable dtInfo = DataTableConverter.Convert(dgEdlistCancel.ItemsSource);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(dtResult.Rows[i]["CALDATE"].ToString()))
                    {
                        // 내/외수 전환된 LOT이 아니라 취소가 불가 합니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8334", new object[] { LotId }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            Util.gridClear(dgEdlistCancel);
                        });
                        ExcelUp_YN = false;
                        return;
                    }

                    if (dtResult.Rows[i]["ERP_CLOSE"].Equals("CLOSE"))
                    {
                        // ERP 생산실적이 마감 되었습니다.
                        Util.MessageValidation("SFU3494");
                        ExcelUp_YN = false;
                        return;
                    }
                }

                dtInfo.Merge(dtResult);
                Util.GridSetData(dgEdlistCancel, dtInfo, FrameOperation);
                _Util.SetDataGridMergeExtensionCol(dgEdlistCancel, new string[] { "CSTID", "BOXID" }, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //
    }
}
