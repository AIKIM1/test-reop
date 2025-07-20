/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.06.29  정재홍    : C20220513-000276 - GMES 시스템의 소형2동 9, 10호 라인 남경 역수입 전극 공급을 위한 시스템 신규 개발 요청 건
  2025.02.24  이민형    : 날짜 함수 Util.GetConfition 으로 형변환 함수 변경
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_022 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        string pcsgid = string.Empty;
        private string sEmpty_Lot = string.Empty;



        #region Declaration & Constructor 
        public BOX001_022()
        {
            InitializeComponent();            
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private C1ComboBox cboOperation = new C1ComboBox(); // LOT

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
            SetEvent();
            SetRadioButton();
            SetInitGrid();
            SearchAreaReturnConfirm();
            if (pcsgid.Equals("E"))
            {
                Tab_TestHist.Visibility = Visibility;
            }

        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReceive);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //공정
            cboOperation.DisplayMemberPath = "PROCID";
            cboOperation.SelectedValuePath = "PROCID";

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboOperation };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: cboEquipmentSegmentParent, sCase: "PROCESSEQUIPMENTSEGMENT");

            //공정
            combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboEquipmentSegmentParent);


            combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "AREA");

            //// ComboBox 추가 필요
            //string[] sFilters = { LoginInfo.CFG_SHOP_ID, "OUTSD_ELTR_TYPE_CODE" };
            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilters);
            //combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "AREA", sFilter: sFilters);

            //string[] sFilters1 = { Convert.ToString(cboArea.SelectedValue) };
            //combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, sFilter: sFilters1);

            //반품 Tab
            combo.SetCombo(cboReturnArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            string[] sFilter = { LoginInfo.LANGID, "IWMS_RCV_ISS_TYPE_CODE" };
            combo.SetCombo(cboReturnResn, CommonCombo.ComboStatus.SELECT, sFilter:sFilter, sCase: "COMMCODES");

            combo.SetCombo(cboReturnHistArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            rdoPancake.IsChecked = true;
            rdoRoll_Click(null, null);  

            //txtLotid.Focus();
            txtPalletId.Focus();

            string[] sFilter2 = {"", "PANCAKE_STAT_CODE" };
            combo.SetCombo(cboPancakeStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter:sFilter2);

            cboArea.IsEnabled = false;

            //입고 가능 자재LOT 탭
            combo.SetCombo(cboPrjtName, CommonCombo.ComboStatus.ALL, sCase : "cboPrjtNameMtrlRecieve");

            //검사이력조회 탭
            combo.SetCombo(cboSpjtName, CommonCombo.ComboStatus.ALL, sCase: "cboPrjtNameMtrlRecieve");
            string[] sFilter3 = { "", "ELEC_TYPE" };
            combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilter3);

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
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
        private void SetRadioButton()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID ;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRCESSSEGMENT", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count == 0)
                {
                    return;
                }

                string roll = Convert.ToString(result.Rows[0]["ROLL"]);
                string pancake = Convert.ToString(result.Rows[0]["PANCAKE"]);
                string nocthed_pancake = Convert.ToString(result.Rows[0]["NOTCHED_PANCAKE"]);
                pcsgid = Convert.ToString(result.Rows[0]["PCSGID"]);

                if (roll.Equals("Y"))
                {
                    rdoRoll.Visibility = Visibility.Visible;
                    rdoRoll2.Visibility = Visibility.Visible;
                    rdoReturnRoll.Visibility = Visibility.Visible;
                }

                if (pancake.Equals("Y"))
                {
                    rdoPancake.Visibility = Visibility.Visible;
                    rodPancake2.Visibility = Visibility.Visible;
                    rdoReturnPancake.Visibility = Visibility.Visible;
                }

                if (nocthed_pancake.Equals("Y"))
                {
                    rdoNochedPancake.Visibility = Visibility.Visible;
                    rdoNochedPancake2.Visibility = Visibility.Visible;
                    rdoReturnNochedPancake.Visibility = Visibility.Visible;
                }


                if (pcsgid.Equals("A"))
                {
                    //rdoRoll.Visibility = Visibility.Collapsed;
                    //rdoRoll2.Visibility = Visibility.Collapsed;
                    //rdoReturnRoll.Visibility = Visibility.Collapsed;

                    rdoPancake.IsChecked = true;
                    rodPancake2.IsChecked = true;
                    rdoReturnPancake.IsChecked = true;
                    cboProcid.IsEnabled = false;
                }
                else if (pcsgid.Equals("E"))
                {
                    //rdoPancake.Visibility = Visibility.Collapsed;
                    //rdoNochedPancake.Visibility = Visibility.Collapsed;

                    //rodPancake2.Visibility = Visibility.Collapsed;
                    //rdoNochedPancake2.Visibility = Visibility.Collapsed;

                    //rdoReturnPancake.Visibility = Visibility.Collapsed;
                    //rdoReturnNochedPancake.Visibility = Visibility.Collapsed;

                    rdoRoll.IsChecked = true;
                    rdoRoll2.IsChecked = true;
                    rdoReturnRoll.IsChecked = true;
                    cboProcid.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Mehod

        #endregion

        #region Event

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                try
                {
                    string sLotid = string.Empty;
                    sLotid = txtLotid.Text.Trim();

                    if (sLotid == "")
                    {
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < dgReceive.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID")).Equals(sLotid))
                        {
                            Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("LOTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = sLotid;

                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("BR_PRD_SEL_RECEIVE_MTRL_IWMS", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            if (searchResult.Rows.Count != 0)
                            {
                                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRODID")).Equals(searchResult.Rows[0]["PRODID"].ToString()))
                                    {
                                        Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");
                                        return;
                                    }
                                }
                            }


                            if (dgReceive.GetRowCount() == 0)
                            {
                                 dgReceive.ItemsSource = DataTableConverter.Convert(searchResult);
                                
                                Util.GridSetData(dgReceive, searchResult, FrameOperation);

                                if (rdoNochedPancake.IsChecked != true)
                                {
                                    // 공정SEGMENT 추가 [2017-10-11]
                                    if (searchResult != null && searchResult.Rows.Count > 0)
                                    {
                                        DataView view = new DataView(searchResult);
                                        DataTable distinctValues = view.ToTable(true, "PROCID");

                                        cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                                        cboOperation.SelectedIndex = 0;

                                        string sOriginalArea = Util.NVC(cboArea.SelectedValue);
                                        string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                                        cboArea.SelectedIndex = 0;
                                        cboArea.SelectedValue = sOriginalArea;
                                        cboEquipmentSegment.SelectedValue = sOriginalLine;
                                    }
                                }
                                Init();
                                
                            }
                            else
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgReceive.ItemsSource);
                                dtSource.Merge(searchResult);

                                Util.gridClear(dgReceive);
                                dgReceive.ItemsSource = DataTableConverter.Convert(dtSource);
                            }

                            // CSR : C20220513-000276
                            getVdMessagePopUp();
                            
                            txtLotid.SelectAll();
                            txtLotid.Focus();
                            txtLotid.Text = "";

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
                    return;
                }
            }
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtPalletId.Text.Trim()))
                {
                    Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }

                // Pallet 중복 체크 확인
                if (CommonVerify.HasDataGridRow(dgReceive))
                {
                    DataTable dt = ((DataView)dgReceive.ItemsSource).Table;
                    var queryEdit = (from t in dt.AsEnumerable()
                        where t.Field<string>("PLLT_ID") == txtPalletId.Text
                                     select t).ToList();

                    if (queryEdit.Any())
                    {
                        Util.MessageValidation("SFU1781");   //이미추가된팔레트입니다.
                        return;
                    }
                }
                

                const string bizRuleName = "BR_PRD_SEL_RECEIVE_MTRL_IWMS_PLLT";

                DataTable inTable = new DataTable {TableName = "INDATA"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PLLT_ID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PLLT_ID"] = txtPalletId.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count != 0)
                        {
                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRODID")).Equals(searchResult.Rows[0]["PRODID"].ToString()))
                                {
                                    Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");
                                    return;
                                }
                            }
                        }


                        if (dgReceive.GetRowCount() > 0)
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgReceive.ItemsSource);
                            dtSource.Merge(searchResult);
                            Util.gridClear(dgReceive);
                            dgReceive.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                        else
                        {
                            dgReceive.ItemsSource = DataTableConverter.Convert(searchResult);
                            Util.GridSetData(dgReceive, searchResult, FrameOperation);

                            if (CommonVerify.HasTableRow(searchResult))
                            {
                                DataView view = new DataView(searchResult);
                                DataTable distinctValues = view.ToTable(true, "PROCID");

                                cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                                cboOperation.SelectedIndex = 0;

                                string sOriginalArea = Util.NVC(cboArea.SelectedValue);
                                string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                                cboArea.SelectedIndex = 0;
                                cboArea.SelectedValue = sOriginalArea;
                                cboEquipmentSegment.SelectedValue = sOriginalLine;
                            }
                        }

                        // CSR : C20220513-000276
                        getVdMessagePopUp();
                        
                        txtPalletId.Text = string.Empty;
                        txtPalletId.Focus();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
        }

        private void Init()
        {
            txtLotid.Text = "";
            txtLotid.Focus();
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (dgReceive.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }

                if (cboArea.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1799");   //입고 할 동을 선택해주세요.
                    return;
                }

          
                if (rdoRoll.IsChecked == true || rdoNochedPancake.IsChecked == true)
                {
                    if (cboProcid.Text.Equals("-SELECT-"))
                    {
                        Util.MessageValidation("SFU1795");   //입고 공정을 선택해주세요.
                        return;
                    }
                }

                //입고 하시겠습니까?
               Util.MessageConfirm("SFU2073", (result) =>
               {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {

                            decimal dSum = 0;
                            decimal dSum2 = 0;
                            decimal dTotal = 0;
                            decimal dTotal2 = 0;

                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                dSum = Util.NVC_Decimal(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY"));
                                dSum2 = Util.NVC_Decimal(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY2"));

                                dTotal = dTotal + dSum;
                                dTotal2 = dTotal2 + dSum2;
                            }

                            DataSet indataSet = new DataSet();
                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("SRCTYPE", typeof(string));
                            inData.Columns.Add("WO_DETL_ID", typeof(string));
                            inData.Columns.Add("PRODID", typeof(string));
                            inData.Columns.Add("BOMREV", typeof(string));
                            inData.Columns.Add("PROCID", typeof(string));
                            inData.Columns.Add("PCSGID", typeof(string));
                            inData.Columns.Add("EQSGID", typeof(string));
                            inData.Columns.Add("EQPTID", typeof(string));
                            inData.Columns.Add("RECIPEID", typeof(string));
                            //inData.Columns.Add("PROD_VER_CODE", typeof(string));
                            inData.Columns.Add("IFMODE", typeof(string));
                            inData.Columns.Add("TRSF_POST_FLAG", typeof(string));
                            inData.Columns.Add("AREAID", typeof(string));
                            inData.Columns.Add("SLOC_ID", typeof(string));
                            inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                            inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                            inData.Columns.Add("USERID", typeof(string));
                            inData.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                            inData.Columns.Add("NOTE", typeof(string));
                            inData.Columns.Add("PANROLL_GUBUN", typeof(string));
                            inData.Columns.Add("ROUTID", typeof(string));
                            inData.Columns.Add("FLOWID", typeof(string));

                           string[] str = Convert.ToString(cboProcid.SelectedValue).Split('|');

                            DataRow row = inData.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["WO_DETL_ID"] = "";//없음
                            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "PRODID"));
                            row["BOMREV"] = "";//없음
                            row["PROCID"] = rdoRoll.IsChecked == true ? str[1] : rdoNochedPancake.IsChecked == true ? str[1] : null;//Convert.ToString(cboProcid.SelectedValue);
                            row["PCSGID"] = pcsgid;
                            row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                            row["EQPTID"] = ""; //없음
                            row["RECIPEID"] = ""; //없음
                            //row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "PROD_VER_CODE"));
                            row["IFMODE"] = "";//없음
                            row["TRSF_POST_FLAG"] = "N";
                            row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                            row["SLOC_ID"] = "";//없음
                            row["TOTAL_QTY"] = dTotal;
                            row["TOTAL_QTY2"] = dTotal2;
                            row["USERID"] = LoginInfo.USERID;
                            row["PRDT_CLSS_CODE"] = "";
                            row["NOTE"] = txtRemark.Text.ToString();
                            row["PANROLL_GUBUN"] = rdoRoll.IsChecked == true ? "R" : rdoPancake.IsChecked == true ? "P" : "NP";
                            row["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "ROUTID"));
                            row["FLOWID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[0].DataItem, "FLOWID"));

                            inData.Rows.Add(row);


                            DataTable inLot = indataSet.Tables.Add("INLOT");
                            inLot.Columns.Add("PLLT_ID", typeof(string)); //추가
                            inLot.Columns.Add("LOTID", typeof(string));
                            inLot.Columns.Add("IWMS_RCV_ID", typeof(string)); //추가
                            inLot.Columns.Add("LOTTYPE", typeof(string));
                            inLot.Columns.Add("LOTID_RT", typeof(string));
                            inLot.Columns.Add("ACTQTY", typeof(decimal));
                            inLot.Columns.Add("ACTQTY2", typeof(decimal));
                            inLot.Columns.Add("ACTUNITQTY", typeof(decimal));
                            inLot.Columns.Add("PR_LOTID", typeof(string));
                            inLot.Columns.Add("WIPNOTE", typeof(string));
                            inLot.Columns.Add("WIP_TYPE_CODE", typeof(string));
                            inLot.Columns.Add("HOTFLAG", typeof(string));
                            inLot.Columns.Add("PROD_LOTID", typeof(string));
                            inLot.Columns.Add("PRJT_NAME", typeof(string));
                            inLot.Columns.Add("SLIT_CUT_ID", typeof(string));
                            inLot.Columns.Add("SLIT_DATE", typeof(string));
                            inLot.Columns.Add("LANE_QTY", typeof(string));
                            inLot.Columns.Add("RT_LOT_CR_DTTM", typeof(string));
                            inLot.Columns.Add("ROLLPRESS_DATE", typeof(string));
                            inLot.Columns.Add("FROM_SHOPID", typeof(string));
                            inLot.Columns.Add("FROM_AREAID", typeof(string));
                            inLot.Columns.Add("VLD_DATE", typeof(string));
                            inLot.Columns.Add("ROUTID", typeof(string));
                            inLot.Columns.Add("FLOWID", typeof(string));
                            inLot.Columns.Add("PROD_VER_CODE", typeof(string));
                            inLot.Columns.Add("CSTID", typeof(string));

                           for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                row = inLot.NewRow();
                                row["PLLT_ID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PLLT_ID"));
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID"));
                                row["IWMS_RCV_ID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "IWMS_RCV_ISS_ID"));
                               //row["LOTTYPE"] = "P";
                                row["LOTTYPE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTTYPE"));
                                row["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LOTID_RT"));
                                row["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY"));
                                row["ACTQTY2"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY2")).Equals("") ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "WIPQTY2"));
                                row["ACTUNITQTY"] = 0;//없고
                                row["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PR_LOTID"));
                                row["WIPNOTE"] = txtRemark.Text.ToString();
                                row["WIP_TYPE_CODE"] = "IN";
                                row["HOTFLAG"] = "";//없고
                                row["PROD_LOTID"] = "";//없고
                                row["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRJT_NAME"));
                                row["SLIT_CUT_ID"] = "";
                                row["SLIT_DATE"] = "";
                                row["LANE_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "LANE_PTN_QTY"));
                                row["RT_LOT_CR_DTTM"] = "";
                                row["ROLLPRESS_DATE"] = "";
                                row["FROM_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "SHOPID"));
                                row["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "AREAID"));
                                row["VLD_DATE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "VLD_DATE"));
                                row["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "ROUTID"));
                                row["FLOWID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "FLOWID"));
                                row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PROD_VER_CODE"));
                                row["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CSTID"));
                               inLot.Rows.Add(row);
                            }

                            //string xml = indataSet.GetXml();

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_MTRL_IWMS", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1798");   //입고 처리 되었습니다.
                                Initialize_dgReceive();

                            }, indataSet);
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
                return;
            }
        }

        private void Initialize_dgReceive()
        {
            Util.gridClear(dgReceive);
            txtLotid.Text = string.Empty;
            txtRemark.Text = string.Empty;
            txtPalletId.Text = string.Empty;
            txtLotid.Focus();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sAreaID = cboArea2.SelectedValue.ToString();

                DataTable dt = null;
                DataRow row = null;

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PANCAKE_STAT_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["WIPSTAT"] = Wip_State.WAIT;
                row["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                row["TO_DATE"] = Util.GetCondition(dtpDateTo);
                row["AREAID"] = sAreaID == "" ? null : sAreaID;
                row["PANCAKE_STAT_CODE"] = Convert.ToString(cboPancakeStat.SelectedValue).Equals("") ? null : Convert.ToString(cboPancakeStat.SelectedValue);
                row["LOTID"] = txtSearchLotid.Text.Trim() == "" ? null : txtSearchLotid.Text.Trim();
                dt.Rows.Add(row);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_HIST_IWMS", "RQSTDT", "RSLTDT", dt);

                Util.gridClear(dgReceive_Hist);
                Util.GridSetData(dgReceive_Hist, SearchResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgReceive_Hist);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
           //삭제 하시겠습니까?
           Util.MessageConfirm("SFU1230", (result) =>
           {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgReceive.IsReadOnly = false;
                    dgReceive.RemoveRow(index);
                    dgReceive.IsReadOnly = true;
                }
            });
        }

        private void rdoRoll_Click(object sender, RoutedEventArgs e)
        {
            if (rdoRoll.IsChecked == true)
            {
                cboProcid.IsEnabled = true;
            }
        }
        private void rdoPancake_Click(object sender, RoutedEventArgs e)
        {
            if (rdoPancake.IsChecked == true)
            {
                cboProcid.IsEnabled = false;
                cboProcid.SelectedIndex = 0;
            }
        }

        private void rdoNochedPancake_Click(object sender, RoutedEventArgs e)
        {
            if (rdoNochedPancake.IsChecked == true)
            {
                cboProcid.IsEnabled = true;
            }
        }

        #region[반품]
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkReturnAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        private void dgReturn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkReturnAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkReturnAll.Checked -= new RoutedEventHandler(checkReturnAll_Checked);
                        chkReturnAll.Unchecked -= new RoutedEventHandler(checkReturnAll_Unchecked);
                        chkReturnAll.Checked += new RoutedEventHandler(checkReturnAll_Checked);
                        chkReturnAll.Unchecked += new RoutedEventHandler(checkReturnAll_Unchecked);
                    }
                }
            }));
        }
        private void txtReturnLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddReturnRow((sender as TextBox).Name);
            }
        }

        private void txtReturnSKIDid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtReturnSKIDid.Text.Trim()))
                {
                    Util.MessageValidation("SFU2934");
                    return;
                }

                // SKID Validation 처리 로직 ?

                const string bizRuleName = "BR_PRD_SEL_RETURN_MTRL_IWMS_FOR_SKID";

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["CSTID"] = txtReturnSKIDid.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (result, searchException) =>
                {

                    try
                    {
                        //HiddenLoadingIndicator();

                        if (searchException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(searchException);
                            txtReturnPalletID.Text = string.Empty;
                            txtReturnLotid.Text = string.Empty;
                            txtReturnSKIDid.Text = string.Empty;
                            return;
                        }

                        for (int j = 0; j < result.Rows.Count; j++)
                        {
                            if (result.Rows[j]["WIPHOLD"].Equals("Y"))
                            {
                                HiddenLoadingIndicator();
                                Util.MessageInfo("SFU6016");
                                return;
                            }
                        }
                        
                        if (dgReturn.GetRowCount() > 0)
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgReturn.ItemsSource);
                            Util.gridClear(dgReturn);
                            DataTable dtTarget = Util.CheckBoxColumnAddTable(result, true);
                            DataTable dtUnion = dtSource.AsEnumerable().Union(dtTarget.AsEnumerable()).Distinct(DataRowComparer.Default).CopyToDataTable<DataRow>();
                            Util.GridSetData(dgReturn, dtUnion, FrameOperation, true);
                        }
                        else
                        {
                            Util.GridSetData(dgReturn, Util.CheckBoxColumnAddTable(result, true), FrameOperation, true);
                        }
                        HiddenLoadingIndicator();

                        dgReturnChk.IsReadOnly = false;
                        dgReturnPlltID.IsReadOnly = true;
                        dgReturnLOTID.IsReadOnly = true;
                        dgReturnISSID.IsReadOnly = true;
                        dgReturnPJTID.IsReadOnly = true;
                        dgReturnModel.IsReadOnly = true;
                        dgReturnProdID.IsReadOnly = true;
                        dgReturnProdName.IsReadOnly = true;
                        dgReturnVersion.IsReadOnly = true;
                        dgReturnValidate.IsReadOnly = true;
                        dgReturnLot_rt.IsReadOnly = true;
                        dgReturnPTN.IsReadOnly = true;
                        dgReturnQty.IsReadOnly = false;
                        dgReturnQty2.IsReadOnly = true;
                        dgReturnshopid.IsReadOnly = true;
                        dgReturnAreaid.IsReadOnly = true;
                        dgReturnit.IsReadOnly = true;
                        dgReturnMktTypeCode.IsReadOnly = true;

                        txtReturnSKIDid.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);

                    }
                });

            }
        }

        private void checkReturnAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgReturn.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", true);
            }
        }

        private void checkReturnAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgReturn.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", false);
            }
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReturn, "CHK") == -1)
            {
                Util.MessageValidation("SFU1381"); //LOT을 선택하세요.
                return;
            }

            if (cboReturnResn.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1554"); //"반품사유를 입력하세요"
                return;
            }

           //반품처리 하시겠습니까?
           Util.MessageConfirm("SFU2868", (result) =>
           {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("IWMS_RCV_ISS_TYPE_CODE", typeof(string));
                        inData.Columns.Add("ELTR_MTRL_TYPE_CODE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row =  inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["AREAID"] = Convert.ToString(cboReturnArea.SelectedValue);
                        row["IWMS_RCV_ISS_TYPE_CODE"] = Convert.ToString(cboReturnResn.SelectedValue);
                        row["ELTR_MTRL_TYPE_CODE"] = rdoReturnRoll.IsChecked == true ? "R" : "P";//없음
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string)); //추가
                        inLot.Columns.Add("RTN_QTY", typeof(decimal));
                        inLot.Columns.Add("RTN_QTY2", typeof(decimal)); //추가

                        row = null;

                        for (int i = 0; i < dgReturn.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                row = inLot.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID"));
                                row["RTN_QTY"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "WIPQTY"));
                                row["RTN_QTY2"] = Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "WIPQTY"));
                                inLot.Rows.Add(row);
                            }

                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_MTRL_IWMS", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1557");   //반품처리 되었습니다.

                            DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);
                            dt = dt.Select("CHK = True").Count() == 0 ? null :  dt.Select("CHK = True").CopyToDataTable();

                            DataTable dtResult = DataTableConverter.Convert(dgReturn.ItemsSource);

                            try
                            {
                                if (dtResult.Select("CHK <> True").Length == 0)
                                {
                                    Util.gridClear(dgReturn);
                                }
                                else
                                {
                                    dtResult = dtResult.Select("CHK  <> True").CopyToDataTable();
                                    Util.GridSetData(dgReturn, dtResult, FrameOperation, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                dgReturn.ItemsSource = null;
                            }
                            

                            DataTable dt2 = new DataTable();
                            dt2.Columns.Add("ReturnID", typeof(string));
                            dt2.Columns.Add("FROM_AREA", typeof(string));
                            dt2.Columns.Add("RETURN_RESN", typeof(string));
                            dt2.Columns.Add("NOTE01", typeof(string));
                            dt2.Columns.Add("dateTime", typeof(string));
                            dt2.Columns.Add("BARCODE_RETURNID", typeof(string));

                            DataRow row2 = dt2.NewRow();
                            row2["ReturnID"] = bizResult.Tables["OUTDATA"].Rows[0]["IWMS_RCV_ISS_ID"];
                            row2["BARCODE_RETURNID"] = bizResult.Tables["OUTDATA"].Rows[0]["IWMS_RCV_ISS_ID"];
                            row2["FROM_AREA"] = Convert.ToString(cboReturnArea.Text);
                            row2["RETURN_RESN"] = Convert.ToString(cboReturnResn.Text);//dateTime
                            row2["NOTE01"] = txtReturnRemark.Text.Equals("") ? " " : txtReturnRemark.Text;
                            row2["dateTime"] = DateTime.Now.ToString("yyyy-MM-dd");
                            dt2.Rows.Add(row2);

                            //반품표발행
                            LGC.GMES.MES.BOX001.Report_Return_Tag rs = new LGC.GMES.MES.BOX001.Report_Return_Tag();
                            rs.FrameOperation = this.FrameOperation;

                            if (rs != null)
                            {
                                // 태그 발행 창 화면에 띄움.
                                object[] Parameters = new object[4];
                                Parameters[0] = "Report_Return_Tag";
                                Parameters[1] = GetPrintTitle();
                                Parameters[2] = dt2; //공통내용
                                Parameters[3] = dt; //팬케익내용

                                C1WindowExtension.SetParameters(rs, Parameters);

                                rs.Closed += new EventHandler(Print_Result);
                                // 팝업 화면 숨겨지는 문제 수정.
                                grdMain.Children.Add(rs);
                                rs.BringToFront();
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
        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Return_Tag wndPopup = sender as LGC.GMES.MES.BOX001.Report_Return_Tag;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnReturnSearch_Click(object sender, RoutedEventArgs e)
        {
            //AddReturnRow();
        }
        private void btnReturnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    //dgReturn.IsReadOnly = false;
                    //dgReturn.RemoveRow(index);
                    //dgReturn.IsReadOnly = true;
                    
                    DataTable dt = DataTableConverter.Convert(dgReturn.ItemsSource);
                    dt.Rows.RemoveAt(index);
                    Util.GridSetData(dgReturn, dt, FrameOperation);                    
                }
            });
        }

        private void SetInitGrid()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgReturn.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgReturn.BeginEdit();
            dgReturn.ItemsSource = DataTableConverter.Convert(dt);
            dgReturn.EndEdit();
        }
        private DataTable GetPrintTitle()
        {
            try
            {
                string Title = ObjectDic.Instance.GetObjectName("반품ID");
                string From_Area = ObjectDic.Instance.GetObjectName("반품지");
                string From_Resn = ObjectDic.Instance.GetObjectName("반품사유");
                string Note = ObjectDic.Instance.GetObjectName("특이사항");
                string title_date = ObjectDic.Instance.GetObjectName("발행일시");
                string title_lotid = ObjectDic.Instance.GetObjectName("LOTID");
                string title_pj = ObjectDic.Instance.GetObjectName("프로젝트명");
                string title_version = ObjectDic.Instance.GetObjectName("버전");
                string title_unit = ObjectDic.Instance.GetObjectName("단위");
                string title_wipqty = ObjectDic.Instance.GetObjectName("재공") + (rdoReturnRoll.IsChecked == true ? "(Roll)" : "(Pancake)");
                string title_valid_date = ObjectDic.Instance.GetObjectName("유효기간");

                DataTable dt1 = new DataTable();
                dt1.Columns.Add("TITLE", typeof(string));
                dt1.Columns.Add("FROM", typeof(string));
                dt1.Columns.Add("RESN", typeof(string));
                dt1.Columns.Add("NOTE", typeof(string));
                dt1.Columns.Add("TITLE_DATE", typeof(string));
                dt1.Columns.Add("TITLE_LOTID", typeof(string));
                dt1.Columns.Add("T_01", typeof(string));
                dt1.Columns.Add("T_02", typeof(string));
                dt1.Columns.Add("T_03", typeof(string));
                dt1.Columns.Add("T_04", typeof(string));
                dt1.Columns.Add("T_05", typeof(string));

                DataRow row1 = dt1.NewRow();
                row1["TITLE"] = Title;
                row1["FROM"] = From_Area;
                row1["RESN"] = From_Resn;
                row1["NOTE"] = Note;
                row1["TITLE_DATE"] = title_date;
                row1["TITLE_LOTID"] = title_lotid;
                row1["T_01"] = title_pj;
                row1["T_02"] = title_version;
                row1["T_03"] = title_unit;
                row1["T_04"] = title_wipqty;
                row1["T_05"] = title_valid_date;

                dt1.Rows.Add(row1);

                return dt1;

            } catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region[반품이력]

        private void txtReturnHistLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReturnCancelSearch();
            }
        }

        private void btnReturnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgReturnHist, "CHK") == -1)
            {
                //LOT을 선택하세요.
                Util.MessageValidation("SFU1381");
                return;
            }

            for(int i = 0; i < dgReturnHist.GetRowCount(); i++)
            {
                if(DataTableConverter.GetValue(dgReturnHist.Rows[i].DataItem, "CHK").ToString() == "1" )
                {
                    if (DataTableConverter.GetValue(dgReturnHist.Rows[i].DataItem, "PANCAKE_STAT_NAME").ToString() == ObjectDic.Instance.GetObjectName("반품확정") ||
                        DataTableConverter.GetValue(dgReturnHist.Rows[i].DataItem, "PANCAKE_STAT_CODE").ToString() == "RETURN_CONFIRM")
                    {
                        Util.MessageValidation("SFU4303"); //반품확정된 제품입니다.
                        return;
                    }
                }
            }
            

            //반품취소하시겠습니까?
            Util.MessageConfirm("SFU3258", (result) =>
           {              
                if (result == MessageBoxResult.OK)
                {
                    try
                    {

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IWMS_RCV_ID", typeof(string));
                        inData.Columns.Add("PANCAKE_STAT_CODE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IWMS_RCV_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturnHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReturnHist, "CHK")].DataItem, "IWMS_RCV_ID"));
                        row["PANCAKE_STAT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReturnHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgReturnHist, "CHK")].DataItem, "PANCAKE_STAT_CODE"));
                        row["AREAID"] = cboReturnHistArea.SelectedValue.ToString();
                        row["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(row);

                        DataTable inLot = indataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("PLLT_ID", typeof(string)); //추가
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("RTN_QTY", typeof(decimal));
                        inLot.Columns.Add("RTN_QTY2", typeof(decimal));

                        row = null;

                        for (int i = 0; i < dgReturnHistDetail.GetRowCount(); i++)
                        {
                            row = inLot.NewRow();
                            row["PLLT_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PLLT_ID"));
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PANCAKE_ID"));
                            row["RTN_QTY"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PANCAKE_QTY"))); 
                            row["RTN_QTY2"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PANCAKE_QTY")));
                            inLot.Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_CANCEL_MTRL_IWMS", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //반품취소하였습니다.
                            Util.MessageInfo("SFU3259");

                            ReturnCancelSearch();
                            dgReturnHistDetail.ItemsSource = null;


                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                       Util.MessageException(ex);
                    }

                }
            });
        }

        private void btnReturnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            ReturnCancelSearch();
        }

        private void ReturnCancelSearch()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["LOTID"] = txtReturnHistLotid.Text.Equals("") ? null : txtReturnHistLotid.Text;
                row["FROM_DATE"] = Util.GetCondition(dtpReturnHistDateFrom);
                row["TO_DATE"] = Util.GetCondition(dtpReturnHistDateTo);
                row["AREAID"] = Convert.ToString(cboReturnHistArea.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_CANCEL_MTRL_IWMS", "RQST", "RSLT", dt);

                dgReturnHistDetail.ItemsSource = null;

                Util.GridSetData(dgReturnHist, result, FrameOperation, true);
            
                /*
                if (!txtReturnHistLotid.Text.Equals(""))
                {
                    DataTableConverter.SetValue(dgReturnHist.Rows[0].DataItem, "CHK", true);
                    GetDetailList(0);

                }
                */

                txtReturnHistLotid.Text = "";                       

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void AddReturnRow(string controlName)
        {
            try
            {
                if (controlName.Equals("txtReturnLotid"))
                {
                    if (txtReturnLotid.Text.Equals(""))
                    {
                        //LOTID를 입력해주세요
                        Util.MessageValidation("SFU1366");
                        return;
                    }


                    for (int i = 0; i < dgReturn.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "LOTID")).Equals(txtReturnLotid.Text))
                        {
                            Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
                            return;
                        }

                    }
                }
                else
                {
                    if (txtReturnPalletID.Text.Equals(""))
                    {
                        Util.MessageValidation("SFU1411"); //PalletID를 입력해주세요
                        return;
                    }

                    for (int i = 0; i < dgReturn.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PLLT_ID")).Equals(txtReturnPalletID.Text))
                        {
                            Util.MessageValidation("SFU1781"); //"이미추가된팔레트입니다."
                            return;
                        }
                    }
                }

                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));

                if (controlName.Equals("txtReturnLotid"))
                {
                    dt.Columns.Add("LOTID", typeof(string));
                }
                else
                {
                    dt.Columns.Add("PLLT_ID", typeof(string));
                }
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                if (controlName.Equals("txtReturnLotid"))
                {
                    row["LOTID"] = txtReturnLotid.Text;
                }
                else
                {
                    row["PLLT_ID"] = txtReturnPalletID.Text;
                }
                row["AREAID"] = Convert.ToString(cboReturnArea.SelectedValue);
                dt.Rows.Add(row);

                new ClientProxy().ExecuteService( (controlName.Equals("txtReturnLotid") ? "BR_PRD_SEL_RETURN_MTRL_IWMS" : "BR_PRD_SEL_RETURN_MTRL_IWMS_FOR_PLLT"), "INDATA", "OUTDATA", dt, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        txtReturnPalletID.Text = "";
                        txtReturnLotid.Text = "";
                        return;
                    }
                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {
                        if (bizResult.Rows[i]["WIPHOLD"].Equals("Y"))
                        {
                            Util.MessageInfo("SFU6016");
                            return;
                        }
                    }

                    if (!bizResult.Columns.Contains("CHK"))
                    {
                        bizResult.Columns.Add("CHK", typeof(bool));
                        if (bizResult.Rows.Count > 0)
                            bizResult.Rows[0]["CHK"] = true;
                    }

                    if (dgReturn.GetRowCount() == 0)
                    {
                        dgReturn.ItemsSource = DataTableConverter.Convert(bizResult);

                        Util.GridSetData(dgReturn, bizResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgReturn.ItemsSource);
                        dtSource.Merge(bizResult);

                        Util.gridClear(dgReturn);
                        dgReturn.ItemsSource = DataTableConverter.Convert(dtSource);
                    }

                    dgReturn.IsReadOnly = false;
                    //dgReturn.BeginNewRow();
                    //dgReturn.EndNewRow(true);
                    

                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "CHK", true);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PLLT_ID", bizResult.Rows[0]["PLLT_ID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID", bizResult.Rows[0]["LOTID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "IWMS_RCV_ISS_ID", bizResult.Rows[0]["IWMS_RCV_ISS_ID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRJT_NAME", bizResult.Rows[0]["PRJT_NAME"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MODLID", bizResult.Rows[0]["MODLID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODID", bizResult.Rows[0]["PRODID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODNAME", bizResult.Rows[0]["PRODNAME"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PROD_VER_CODE", bizResult.Rows[0]["PROD_VER_CODE"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "VLD_DATE", bizResult.Rows[0]["VLD_DATE"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID_RT", bizResult.Rows[0]["LOTID_RT"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LANE_PTN_QTY", bizResult.Rows[0]["LANE_PTN_QTY"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY", Util.NVC_Decimal(bizResult.Rows[0]["WIPQTY"]));
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY2", Util.NVC_Decimal(bizResult.Rows[0]["WIPQTY2"]));
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "SHOPID", bizResult.Rows[0]["SHOPID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "AREAID", bizResult.Rows[0]["AREAID"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "UNIT", bizResult.Rows[0]["UNIT"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MKT_TYPE_CODE", bizResult.Rows[0]["MKT_TYPE_CODE"]);
                    //DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPHOLD", bizResult.Rows[0]["WIPHOLD"]);



                    dgReturnChk.IsReadOnly = false;
                    dgReturnPlltID.IsReadOnly = true;
                    dgReturnLOTID.IsReadOnly = true;
                    dgReturnISSID.IsReadOnly = true;
                    dgReturnPJTID.IsReadOnly = true;
                    dgReturnModel.IsReadOnly = true;
                    dgReturnProdID.IsReadOnly = true;
                    dgReturnProdName.IsReadOnly = true;
                    dgReturnVersion.IsReadOnly = true;
                    dgReturnValidate.IsReadOnly = true;
                    dgReturnLot_rt.IsReadOnly = true;
                    dgReturnPTN.IsReadOnly = true;
                    dgReturnQty.IsReadOnly = false;
                    dgReturnQty2.IsReadOnly = true;
                    dgReturnshopid.IsReadOnly = true;
                    dgReturnAreaid.IsReadOnly = true;
                    dgReturnit.IsReadOnly = true;
                    dgReturnMktTypeCode.IsReadOnly = true;

                    txtReturnLotid.Text = string.Empty;

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRePublish_Click(object sender, RoutedEventArgs e)
        {
            int index = _Util.GetDataGridCheckFirstRowIndex(dgReturnHist, "CHK");
            if (index == -1)
            {
                Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                return;
            }

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("ReturnID", typeof(string));
            dt2.Columns.Add("FROM_AREA", typeof(string));
            dt2.Columns.Add("RETURN_RESN", typeof(string));
            dt2.Columns.Add("NOTE01", typeof(string));
            dt2.Columns.Add("dateTime", typeof(string));
            dt2.Columns.Add("BARCODE_RETURNID", typeof(string));
            //newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));
            DataRow row = dt2.NewRow();
            row["ReturnID"] = DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "IWMS_RCV_ID");
            row["BARCODE_RETURNID"] = DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "IWMS_RCV_ID");
            row["FROM_AREA"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[0].DataItem, "AREANAME")).Equals("") ? "" : Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[0].DataItem, "AREANAME"));
            row["RETURN_RESN"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[0].DataItem, "CMCDNAME")).Equals("") ? "" : Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[0].DataItem, "CMCDNAME"));
            row["NOTE01"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[0].DataItem, "NOTE"));
            row["dateTime"] = DateTime.Now.ToString("yyyy-MM-dd");
            dt2.Rows.Add(row);

            DataTable ReturnDetail = new DataTable();
            ReturnDetail.Columns.Add("LOTID", typeof(string));
            ReturnDetail.Columns.Add("PRJT_NAME", typeof(string));
            ReturnDetail.Columns.Add("PROD_VER_CODE", typeof(string));
            ReturnDetail.Columns.Add("UNIT", typeof(string));
            ReturnDetail.Columns.Add("WIPQTY", typeof(string));
            ReturnDetail.Columns.Add("VLD_DATE", typeof(string));

            for (int i = 0; i < dgReturnHistDetail.GetRowCount(); i++)
            {
                row = ReturnDetail.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PANCAKE_ID"));
                row["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "PRJT_NAME"));
                row["PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PROD_VER_CODE"));
                row["UNIT"] = Util.NVC(DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "UNIT_CODE"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "PANCAKE_QTY"));
                row["VLD_DATE"] = Util.NVC(DataTableConverter.GetValue(dgReturnHistDetail.Rows[i].DataItem, "VLD_DATE"));
                ReturnDetail.Rows.Add(row);
            }

            LGC.GMES.MES.BOX001.Report_Return_Tag rs = new LGC.GMES.MES.BOX001.Report_Return_Tag();
            rs.FrameOperation = this.FrameOperation;

            if (rs != null)
            {
                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[4];
                Parameters[0] = "Report_Return_Tag";
                Parameters[1] = GetPrintTitle();
                Parameters[2] = dt2;
                Parameters[3] = ReturnDetail;

                C1WindowExtension.SetParameters(rs, Parameters);

                rs.Closed += new EventHandler(Print_Result);
                grdMain.Children.Add(rs);
                rs.BringToFront();
            }
        }
         
        private void dgReturnHist_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int index = e.Row.Index;

                //if (DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "PANCAKE_STAT_NAME").Equals("반품 확정"))
                //    {                    
                //        Util.MessageValidation("반품확정된 제품입니다.");                    
                //        return;                               
                //    }                    
                 
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    //int index = e.Row.Index;


                    DataTable dtReturnHistDetail = DataTableConverter.Convert(dgReturnHistDetail.ItemsSource);
                    DataTable dtReturnHist = DataTableConverter.Convert(dgReturnHist.ItemsSource);


                    if(DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "CHK").ToString() == "1")//if(dtReturnHist.Rows[index]["CHK"].ToString() == "1")
                    {

                        GetDetailList(index);
                    }
                    else
                    {
                        string tmp1 = dtReturnHist.Rows[index]["IWMS_RCV_ID"].ToString();
                        

                        DataRow[] drs =  dtReturnHistDetail.Select("IWMS_RCV_ISS_ID = '" + dtReturnHist.Rows[index]["IWMS_RCV_ID"].ToString() + "'");

                        foreach(DataRow dr in drs)
                        {
                            //if(dr != null) dtReturnHistDetail.Rows.Add(dr);

                            dtReturnHistDetail.Rows.Remove(dr);

                        }
                        Util.gridClear(dgReturnHistDetail);
                        Util.GridSetData(dgReturnHistDetail, dtReturnHistDetail, FrameOperation, true);
                    }
                }
            }));
        }

        private void GetDetailList(int index)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("IWMS_RCV_ISS_ID", typeof(string));

                DataRow row = dt.NewRow();
                row["IWMS_RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "IWMS_RCV_ID"));
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_IWMS_RCV_ISS_HIST_RETURN", "RQST", "RSLT", dt);

                if(dgReturnHistDetail.GetRowCount() > 0 )
                {
                    DataTable dtSource = DataTableConverter.Convert(dgReturnHistDetail.ItemsSource);

                    dtSource.Merge(result);

                    Util.GridSetData(dgReturnHistDetail, dtSource, FrameOperation, true);

                }
                else
                {
                    Util.GridSetData(dgReturnHistDetail, result, FrameOperation, true);
                }               

                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtReturnPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddReturnRow((sender as TextBox).Name);
            }
        }

        private void SearchAreaReturnConfirm()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "IWMS_NONE_AREAID";
                dr["CMCDIUSE"] = "Y";

                RQSTDT.Rows.Add(dr);
                DataTable dtArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_JUDGE_HIST_CHK", "RQSTDT", "RSLTDT", RQSTDT);
                for (int i = 0; i < dtArea.Rows.Count; i++)
                {
                    if (dtArea.Rows[i]["CMCODE"].ToString().Equals(LoginInfo.CFG_AREA_ID))
                    {
                        Tab_Return.Visibility = Visibility.Collapsed;
                        Tab_Return2.Visibility = Visibility.Collapsed;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #endregion

        private void dgReturnHist_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            int index = e.Row.Index;
            if (DataTableConverter.GetValue(dgReturnHist.Rows[index].DataItem, "PANCAKE_STAT_NAME").Equals(ObjectDic.Instance.GetObjectName("반품확정")))
            {
                DataTableConverter.SetValue(dgReturnHist.Rows[index].DataItem, "CHK", true);
            }
        }

        private void btnSearch_Roll_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            GetReceiveRoll();

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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void txtRollLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetReceiveRoll();
            }
        }


        private void GetReceiveRoll()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (txtRollLotid.Text.Equals(""))
                {
                    dr["LOTID"] = null;
                    dr["PRJT_NAME"] = Convert.ToString(cboPrjtName.SelectedValue).Equals("") ? null : Convert.ToString(cboPrjtName.SelectedValue);
                }
                else
                {
                    dr["LOTID"] = txtRollLotid.Text;
                    dr["PRJT_NAME"] = null;
                }
            
                dt.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_IWMS_PANCAKE_INFO_HIST", "INDATA", "OUTDATA", dt, (result, searchException) =>
                {

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                    }

                    Util.GridSetData(dgReceiveRollInfo, result, FrameOperation, true);

                   
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void txtLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Create(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }


                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LOTID"] = sLotid;

                RQSTDT.Rows.Add(dr);


                new ClientProxy().ExecuteService("BR_PRD_SEL_RECEIVE_MTRL_IWMS", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count != 0)
                        {

                            for (int i = 0; i < dgReceive.GetRowCount(); i++)
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRODID")).Equals(searchResult.Rows[0]["PRODID"].ToString()))
                                {
                                    Util.MessageValidation("SFU1656");//선택한 LOTID의 제품코드가 다릅니다.
                                    return;
                                }
                            }
                            //추가
                            if (sEmpty_Lot == "")
                                sEmpty_Lot += sLotid;
                            else
                                sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                            
                        }

                        if (dgReceive.GetRowCount() == 0)
                        {
                            dgReceive.ItemsSource = DataTableConverter.Convert(searchResult);

                            Util.GridSetData(dgReceive, searchResult, FrameOperation);

                            if (rdoNochedPancake.IsChecked != true)
                            {
                                // 공정SEGMENT 추가 [2017-10-11]
                                if (searchResult != null && searchResult.Rows.Count > 0)
                                {
                                    DataView view = new DataView(searchResult);
                                    DataTable distinctValues = view.ToTable(true, "PROCID");

                                    cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                                    cboOperation.SelectedIndex = 0;

                                    string sOriginalArea = Util.NVC(cboArea.SelectedValue);
                                    string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                                    cboArea.SelectedIndex = 0;
                                    cboArea.SelectedValue = sOriginalArea;
                                    cboEquipmentSegment.SelectedValue = sOriginalLine;
                                }
                            }
                            Init();

                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgReceive.ItemsSource);
                            dtSource.Merge(searchResult);

                            Util.gridClear(dgReceive);
                            dgReceive.ItemsSource = DataTableConverter.Convert(dtSource);
                        }

                        // CSR : C20220513-000276
                        getVdMessagePopUp();
                       
                        txtLotid.SelectAll();
                        txtLotid.Focus();
                        txtLotid.Text = "";

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
                                      

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        //추가


        //private void txtReturnLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
        //    {

        //        try
        //        {
        //            ShowLoadingIndicator();

        //            if (txtReturnLotid.ToString().Equals(""))
        //            {
        //                Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
        //                return;
        //            }


        //            string[] stringSeparators = new string[] { "\r\n" };
        //            string sPasteString = Clipboard.GetText();
        //            string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
        //            string sPasteStringLot = "";
        //            if (sPasteStrings.Count() > 100)
        //            {
        //                Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
        //                return;
        //            }
        //            for (int j = 0; j < dgReturn.Rows.Count; j++)
        //            {
        //                for (int i = 0; i < sPasteStrings.Length; i++)
        //                {
        //                    if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[j].DataItem, "LOTID")).Equals(sPasteStrings[i]))
        //                    {
        //                        Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
        //                        return;
        //                    }
        //                }
        //            }
        //            for (int i = 0; i < sPasteStrings.Length; i++)
        //            {

        //                sPasteStringLot += sPasteStrings[i] + ",";
        //                //ReturnMulti_Create(sPasteStringLot);                                                                    
        //                //ReturnMulti_Create(sPasteStrings[i]);
        //            }

        //            ReturnMulti_Create(sPasteStringLot);
        //        }
        //        catch (Exception ex)
        //        {
        //            Util.MessageException(ex);
        //            return;
        //        }
        //        finally
        //        {
        //            HiddenLoadingIndicator();
        //        }

        //        e.Handled = true;
        //    }
        //}

        //bool ReturnMulti_Create(string sLotid)
        //{
        //    try
        //    {

        //        DoEvents();



        //        DataTable dt = new DataTable();
        //        dt.TableName = "INDATA";
        //        dt.Columns.Add("LANGID", typeof(string));


        //        dt.Columns.Add("LOTID", typeof(string));

        //        dt.Columns.Add("AREAID", typeof(string));

        //        DataRow row = dt.NewRow();
        //        row["LANGID"] = LoginInfo.LANGID;

        //        row["LOTID"] = sLotid;

        //        row["AREAID"] = Convert.ToString(cboReturnArea.SelectedValue);
        //        dt.Rows.Add(row);

        //        new ClientProxy().ExecuteService("BR_PRD_SEL_RETURN_MTRL_IWMS_INLOT", "INDATA", "OUTDATA", dt, (bizResult, bizException) =>
        //    {

        //            //if (sEmpty_Lot == "")
        //            //    sEmpty_Lot += sLotid;
        //            //else
        //            //sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
        //            try
        //        {
        //            if (bizException != null)
        //            {

        //                Util.MessageException(bizException);

        //                txtReturnPalletID.Text = "";
        //                txtReturnLotid.Text = "";
        //                return;

        //            }

        //            if (bizResult.Rows.Count != 0)
        //            {
        //                for (int i = 0; i < dgReturn.GetRowCount(); i++)
        //                {
        //                    if (!Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PRODID")).Equals(bizResult.Rows[0]["PRODID"].ToString()))
        //                    {
        //                        Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");
        //                        return;
        //                    }
        //                }
        //            }

        //            if (dgReturn.GetRowCount() == 0)
        //            {
        //                dgReturn.ItemsSource = DataTableConverter.Convert(bizResult);

        //                Util.GridSetData(dgReturn, bizResult, FrameOperation);

        //                txtReturnPalletID.Text = "";
        //                txtReturnLotid.Text = "";

        //            }
        //            else
        //            {
        //                DataTable dtSource = DataTableConverter.Convert(dgReturn.ItemsSource);
        //                dtSource.Merge(bizResult);

        //                Util.gridClear(dgReturn);
        //                dgReturn.ItemsSource = DataTableConverter.Convert(dtSource);
        //            }


        //            txtReturnLotid.SelectAll();
        //            txtReturnLotid.Focus();
        //            txtReturnLotid.Text = "";
        //            dgReturn.IsReadOnly = false;
        //            dgReturn.BeginNewRow();
        //            dgReturn.EndNewRow(true);

        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "CHK", true);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PLLT_ID", bizResult.Rows[0]["PLLT_ID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID", bizResult.Rows[0]["LOTID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "IWMS_RCV_ISS_ID", bizResult.Rows[0]["IWMS_RCV_ISS_ID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRJT_NAME", bizResult.Rows[0]["PRJT_NAME"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MODLID", bizResult.Rows[0]["MODLID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODID", bizResult.Rows[0]["PRODID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODNAME", bizResult.Rows[0]["PRODNAME"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PROD_VER_CODE", bizResult.Rows[0]["PROD_VER_CODE"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "VLD_DATE", bizResult.Rows[0]["VLD_DATE"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID_RT", bizResult.Rows[0]["LOTID_RT"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LANE_PTN_QTY", bizResult.Rows[0]["LANE_PTN_QTY"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY", bizResult.Rows[0]["WIPQTY"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY2", bizResult.Rows[0]["WIPQTY2"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "SHOPID", bizResult.Rows[0]["SHOPID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "AREAID", bizResult.Rows[0]["AREAID"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "UNIT", bizResult.Rows[0]["UNIT"]);
        //            DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MKT_TYPE_CODE", bizResult.Rows[0]["MKT_TYPE_CODE"]);

        //            dgReturnChk.IsReadOnly = false;
        //            dgReturnLOTID.IsReadOnly = true;
        //            dgReturnISSID.IsReadOnly = true;
        //            dgReturnPJTID.IsReadOnly = true;
        //            dgReturnModel.IsReadOnly = true;
        //            dgReturnProdID.IsReadOnly = true;
        //            dgReturnProdName.IsReadOnly = true;
        //            dgReturnVersion.IsReadOnly = true;
        //            dgReturnValidate.IsReadOnly = true;
        //            dgReturnLot_rt.IsReadOnly = true;
        //            dgReturnPTN.IsReadOnly = true;
        //            dgReturnQty.IsReadOnly = false;
        //            dgReturnQty2.IsReadOnly = true;
        //            dgReturnshopid.IsReadOnly = true;
        //            dgReturnAreaid.IsReadOnly = true;
        //            dgReturnit.IsReadOnly = true;
        //            dgReturnMktTypeCode.IsReadOnly = true;

        //            txtReturnLotid.Text = string.Empty;
        //        }
        //        catch (Exception ex)
        //        {
        //            Util.MessageException(ex);
        //        }
        //    });


        //        return true;
        //    }

        //    catch (Exception ex)
        //    {
        //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        Util.MessageException(ex);
        //        return true;
        //    }
        //}

        private void txtSkidId_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                try
                {

                    //Util.MessageValidation("해당 기능은 다시 수정하여 배포날짜의 올리겠습니다.");   //스캔한 데이터가 없습니다.
                    //return;

                    string sSkidid = string.Empty;
                    sSkidid = txtSkidId.Text.Trim();

                    if (sSkidid == "")
                    {
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < dgReceive.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "CSTID")).Equals(sSkidid))
                        {
                            Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("LANGID", typeof(String));
                    RQSTDT.Columns.Add("AREAID", typeof(String));
                    RQSTDT.Columns.Add("CSTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["CSTID"] = sSkidid;

                    RQSTDT.Rows.Add(dr);

                    new ClientProxy().ExecuteService("BR_PRD_SEL_RECEIVE_MTRL_IWMS_SKID", "INDATA", "OUTDATA", RQSTDT, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);

                                return;
                            }

                            if (searchResult.Rows.Count != 0)
                            {

                                for (int i = 0; i < dgReceive.GetRowCount(); i++)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(dgReceive.Rows[i].DataItem, "PRODID")).Equals(searchResult.Rows[0]["PRODID"].ToString()))
                                    {
                                        Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");
                                        return;
                                    }
                                }
                            }

                            if (dgReceive.GetRowCount() == 0)
                            {
                                dgReceive.ItemsSource = DataTableConverter.Convert(searchResult);

                                Util.GridSetData(dgReceive, searchResult, FrameOperation);

                                // 공정SEGMENT 추가 [2017-10-11]
                                if (searchResult != null && searchResult.Rows.Count > 0)
                                {
                                    DataView view = new DataView(searchResult);
                                    DataTable distinctValues = view.ToTable(true, "PROCID");

                                    cboOperation.ItemsSource = DataTableConverter.Convert(distinctValues);
                                    cboOperation.SelectedIndex = 0;

                                    string sOriginalArea = Util.NVC(cboArea.SelectedValue);
                                    string sOriginalLine = Util.NVC(cboEquipmentSegment.SelectedValue);

                                    cboArea.SelectedIndex = 0;
                                    cboArea.SelectedValue = sOriginalArea;
                                    cboEquipmentSegment.SelectedValue = sOriginalLine;
                                }
                                Init();

                            }
                            else
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgReceive.ItemsSource);
                                dtSource.Merge(searchResult);

                                Util.gridClear(dgReceive);
                                dgReceive.ItemsSource = DataTableConverter.Convert(dtSource);
                            }

                            // CSR : C20220513-000276
                            getVdMessagePopUp();
                            
                            txtSkidId.SelectAll();
                            txtSkidId.Focus();
                            txtSkidId.Text = "";

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
                    return;
                }
            }
        }

        private void txtReturnLotid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                try
                {
                    ShowLoadingIndicator();

                    if (txtReturnLotid.ToString().Equals(""))
                    {
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }


                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string sPasteStringLot = "";
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }
                    for (int j = 0; j < dgReturn.Rows.Count; j++)
                    {

                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(sPasteStrings[i]))
                            {
                                Util.MessageValidation("SFU1362");

                                return;
                            }

                            if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[j].DataItem, "LOTID")).Equals(sPasteStrings[i]))
                            {
                                Util.MessageValidation("SFU2014");   //해당 LOT이 이미 존재합니다.
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {

                        sPasteStringLot += sPasteStrings[i] + ",";
                        //ReturnMulti_Create(sPasteStringLot);                                                                    
                        //ReturnMulti_Create(sPasteStrings[i]);
                    }

                    ReturnMulti_Create(sPasteStringLot);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }
        bool ReturnMulti_Create(string sLotid)
        {
            try
            {
                DoEvents();


                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));


                dt.Columns.Add("LOTID", typeof(string));

                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;

                row["LOTID"] = sLotid;

                row["AREAID"] = Convert.ToString(cboReturnArea.SelectedValue);
                dt.Rows.Add(row);

                new ClientProxy().ExecuteService("BR_PRD_SEL_RETURN_MTRL_IWMS_INLOT" , "INDATA", "OUTDATA", dt, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        txtReturnPalletID.Text = "";
                        txtReturnLotid.Text = "";
                        return;
                    }
                    if (bizResult.Rows.Count != 0)
                    {
                        for (int j = 0; j < bizResult.Rows.Count; j++)
                        {
                            if (bizResult.Rows[j]["WIPHOLD"].Equals("Y"))
                            {
                                Util.MessageInfo("SFU6016");
                                return;
                            }
                        }
                        for (int i = 0; i < dgReturn.GetRowCount(); i++)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "PRODID")).Equals(bizResult.Rows[0]["PRODID"].ToString()))
                            {
                                Util.MessageValidation("해당 LOT의 제품코드와 다릅니다.");
                                return;
                            }
                        }
                    }

                    //for (int i = 0; i < bizResult.Rows.Count; i++)
                    //{
                    //    dgReturn.IsReadOnly = false;
                    //    dgReturn.BeginNewRow();
                    //    dgReturn.EndNewRow(true);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "CHK", true);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PLLT_ID", bizResult.Rows[i]["PLLT_ID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID", bizResult.Rows[i]["LOTID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "IWMS_RCV_ISS_ID", bizResult.Rows[i]["IWMS_RCV_ISS_ID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRJT_NAME", bizResult.Rows[i]["PRJT_NAME"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MODLID", bizResult.Rows[i]["MODLID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODID", bizResult.Rows[i]["PRODID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PRODNAME", bizResult.Rows[i]["PRODNAME"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "PROD_VER_CODE", bizResult.Rows[i]["PROD_VER_CODE"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "VLD_DATE", bizResult.Rows[i]["VLD_DATE"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LOTID_RT", bizResult.Rows[i]["LOTID_RT"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "LANE_PTN_QTY", bizResult.Rows[i]["LANE_PTN_QTY"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY", Util.NVC_Decimal(bizResult.Rows[i]["WIPQTY"]));
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPQTY2", Util.NVC_Decimal(bizResult.Rows[i]["WIPQTY2"]));
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "SHOPID", bizResult.Rows[i]["SHOPID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "AREAID", bizResult.Rows[i]["AREAID"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "UNIT", bizResult.Rows[i]["UNIT"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "MKT_TYPE_CODE", bizResult.Rows[i]["MKT_TYPE_CODE"]);
                    //    DataTableConverter.SetValue(dgReturn.CurrentRow.DataItem, "WIPHOLD", bizResult.Rows[i]["WIPHOLD"]);
                    //}

                    if (!bizResult.Columns.Contains("CHK"))
                    {
                        bizResult.Columns.Add("CHK", typeof(bool));
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                            bizResult.Rows[i]["CHK"] = true;
                    }

                    if (dgReturn.GetRowCount() == 0)
                    {
                        dgReturn.ItemsSource = DataTableConverter.Convert(bizResult);

                        Util.GridSetData(dgReturn, bizResult, FrameOperation);

                        txtReturnPalletID.Text = "";
                        txtReturnLotid.Text = "";

                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgReturn.ItemsSource);
                        dtSource.Merge(bizResult);

                        Util.gridClear(dgReturn);
                        dgReturn.ItemsSource = DataTableConverter.Convert(dtSource);
                    }
                    dgReturn.IsReadOnly = false;
                    dgReturnChk.IsReadOnly = false;
                    dgReturnPlltID.IsReadOnly = true;
                    dgReturnLOTID.IsReadOnly = true;
                    dgReturnISSID.IsReadOnly = true;
                    dgReturnPJTID.IsReadOnly = true;
                    dgReturnModel.IsReadOnly = true;
                    dgReturnProdID.IsReadOnly = true;
                    dgReturnProdName.IsReadOnly = true;
                    dgReturnVersion.IsReadOnly = true;
                    dgReturnValidate.IsReadOnly = true;
                    dgReturnLot_rt.IsReadOnly = true;
                    dgReturnPTN.IsReadOnly = true;
                    dgReturnQty.IsReadOnly = false;
                    dgReturnQty2.IsReadOnly = true;
                    dgReturnshopid.IsReadOnly = true;
                    dgReturnAreaid.IsReadOnly = true;
                    dgReturnit.IsReadOnly = true;
                    dgReturnMktTypeCode.IsReadOnly = true;

                    txtReturnLotid.Text = string.Empty;

                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;
        }

        private void btnSerarchCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("INSP_CLSS_CODE", typeof(string));                
                dt.Columns.Add("PRJT_NAME", typeof(string));                
                dt.Columns.Add("ELEC_TYPE", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INSP_CLSS_CODE"] = "LQCM027";
                dr["PRJT_NAME"] = Convert.ToString(cboSpjtName.SelectedValue).Equals("") ? null : Convert.ToString(cboSpjtName.SelectedValue);
                dr["ELEC_TYPE"] = Convert.ToString(cboElecType.SelectedValue).Equals("") ? null : Convert.ToString(cboElecType.SelectedValue);
                dr["PRODID"] = txtSprodId.Text.Trim().Equals("") ? null : Convert.ToString(txtSprodId.Text.Trim());


                dt.Rows.Add(dr);
               

                new ClientProxy().ExecuteService("DA_PRD_SEL_ELEC_QA_RESULT", "INDATA", "OUTDATA", dt, (result, searchException) =>
                {

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                    }

                    Util.GridSetData(dgSearchTestHistInfo, result, FrameOperation, true);


                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btnPrint = sender as Button;
                if (btnPrint != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                    string sLotID = string.Empty;
                    sLotID = Util.NVC(dataRow.Row["LOTID"]);

                    if (string.Equals(btnPrint.Name, "btnBarcode"))
                    {
                        if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                            return;
                        }

                       Util.PrintLabel_OtherElec(FrameOperation, loadingIndicator, sLotID, Process.SLITTING);
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            
        }

        private void txtSearchLotid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtSearchLotid.Text.Trim()))
                        return;

                    SearchLot(txtSearchLotid.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchLot(string sLotID)
        {
            try
            {
                string sAreaID = cboArea2.SelectedValue.ToString();

                DataTable dt = null;
                DataRow row = null;

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("FROM_DATE", typeof(string));
                dt.Columns.Add("TO_DATE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PANCAKE_STAT_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["WIPSTAT"] = Wip_State.WAIT;
                row["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                row["TO_DATE"] = Util.GetCondition(dtpDateTo);
                row["AREAID"] = sAreaID == "" ? null : sAreaID;
                row["PANCAKE_STAT_CODE"] = Convert.ToString(cboPancakeStat.SelectedValue).Equals("") ? null : Convert.ToString(cboPancakeStat.SelectedValue);
                row["LOTID"] = sLotID == "" ? null : sLotID;
                dt.Rows.Add(row);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_HIST_IWMS", "RQSTDT", "RSLTDT", dt);

                Util.gridClear(dgReceive_Hist);
                Util.GridSetData(dgReceive_Hist, SearchResult, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getVdMessagePopUp()
        {
            try
            {
                // CSR : C20220513-000276
                DataTable dtVdSmp = DataTableConverter.Convert(dgReceive.ItemsSource);

                if (dtVdSmp != null && dtVdSmp.Rows.Count > 0)
                {
                    int iVdSmpCnt = 0;
                    string sVdLotid = string.Empty;

                    if (dtVdSmp.Select("VD_QA_INSP_SMPLG_TRGT_FLAG = 'Y' ").Count() > 0)
                    {
                        DataTable dtCoyp = dtVdSmp.Select("VD_QA_INSP_SMPLG_TRGT_FLAG = 'Y' ").CopyToDataTable();

                        iVdSmpCnt = dtCoyp.Rows.Count;
                        sVdLotid = dtCoyp.Rows[0]["LOTID"].ToString();

                        if (iVdSmpCnt > 0)
                        {
                            Util.MessageValidation("SFU8504", iVdSmpCnt);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
