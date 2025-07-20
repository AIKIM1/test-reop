/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_222 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();

        public DataTable dtPackingCard;
        public DataTable dtBasicInfo;
        public DataTable dtSel01;
        public DataTable dtSel02;

        public bool bCancel = false;
        private string _APPRV_PASS_NO = string.Empty;

        public BOX001_222()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCancel);
            listAuth.Add(btnOut);
            listAuth.Add(btnCancel2);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "NJ_SHIPMENT_AREA" };
            String[] sFilter2 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };

            //C1ComboBox[] cboLocChild = { cboTransLoc };
            //_combo.SetCombo(cboFromShop, CommonCombo.ComboStatus.NONE, cbChild: cboLocChild, sFilter: sFilter1, sCase: "COMMCODE");

            //C1ComboBox[] cboLocParent = { cboFromShop };
            //_combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.NONE, cbParent: cboLocParent, sCase: "TRANSLOC");

            _combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");
            _combo.SetCombo(cboTransLoc_Hist, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "TRANSLOC");
            //C1ComboBox[] cboLocChild2 = { cboTransLoc2 };
            //_combo.SetCombo(cboFromShop2, CommonCombo.ComboStatus.NONE, cbChild: cboLocChild2, sFilter: sFilter1, sCase: "COMMCODE");

            //C1ComboBox[] cboLocParent2 = { cboFromShop2 };
            //_combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.NONE, cbParent: cboLocParent2, sCase: "TRANSLOC");


            _combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "TRANSLOC");
            _combo.SetCombo(cboStatus2, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            _combo.SetCombo(cboTransLoc6, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");
            _combo.SetCombo(cboStatus6, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;
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

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }

        #endregion

        #region N5 출고
        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid.Text != "")
                {
                    Search_N5();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    Search_N5();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_N5();
        }

        private void Search_N5()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                Util.gridClear(dgPacked_N5);
                Util.gridClear(dgPacked_N5_Hist);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXSTAT", typeof(String));
                RQSTDT.Columns.Add("BOXTYPE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXSTAT"] = "PACKED";
                dr["BOXTYPE"] = "PLT";
                dr["FROM_DATE"] = txtBoxid.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid.Text.Trim() != "" ? null : sEnd_date;
                dr["BOXID"] = txtBoxid.Text.Trim() == "" ? null : txtBoxid.Text;
                //dr["SHIPTO_ID"] = Util.NVC(cboTransLoc.SelectedValue) == "" ? null : Util.NVC(cboTransLoc.SelectedValue);
                dr["SHIPTO_ID"] = txtBoxid.Text.Trim() != "" ? null : Util.NVC(cboTransLoc.SelectedValue);
                dr["PRODID"] = txtProd_ID.Text.Trim() == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgPacked_N5.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgPacked_N5, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private String GetAreaTypeCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));

                DataRow row = dt.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_PCSGID", "RQSTDT", "RSLTDT", dt);
                if (result.Rows.Count == 0)
                {
                    return "A";
                }

                return Convert.ToString(result.Rows[0]["PCSGID"]);
            }
            catch (Exception ex)
            {
                return "A";
            }

         
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPacked_N5_Hist.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPacked_N5, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                // 유효일자 체크
                DataTable validDt = GetValidDate(Util.NVC(drChk[0]["BOXID"]));
                foreach (DataRow row in validDt.Rows)
                {
                    if (string.Equals(row["DIFF"], "N"))
                    {
                        Util.MessageValidation("SFU4277", Util.NVC(row["LOTID"]));  //LOT [%1]의 유효기간이 초과되어서 출고 할 수 없습니다.
                        return;
                    }
                }

                

                //선입선출 체크 , 2018-05-28 품질검사 오류로 출고안되서 해당 로직 제거
/*
                for (int i = 0; i < drChk.Length; i++)
                {
                    fifoCheck(Util.NVC(drChk[i]["BOXID"]));
                }
*/
                //출고처리 하시겠습니까?
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3121"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3121", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        #region OLD Version

                        string sSHIPTO_ID = cboTransLoc.SelectedValue.ToString();
                        string sTO_SLOC_ID = string.Empty;
                        string sShopID = string.Empty;
                        string sPkgWay = string.Empty;
                        string sBoxid = string.Empty;
                        string sOUTER_BOXID = string.Empty;
                        string sOWMS_Code = string.Empty;

                        DataTable RQSTDT01 = new DataTable();
                        RQSTDT01.TableName = "RQSTDT";
                        RQSTDT01.Columns.Add("FROM_AREAID", typeof(String));
                        RQSTDT01.Columns.Add("SHIPTO_ID", typeof(String));

                        DataRow dr01 = RQSTDT01.NewRow();
                        dr01["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr01["SHIPTO_ID"] = sSHIPTO_ID;

                        RQSTDT01.Rows.Add(dr01);

                        DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_SLOC_ID", "RQSTDT", "RSLTDT", RQSTDT01);

                        if (SlocIDResult.Rows.Count == 0)
                        {
                            Util.MessageInfo("SFU2071");
                            return;
                        }
                        else
                        {
                            sTO_SLOC_ID = SlocIDResult.Rows[0]["TO_SLOC_ID"].ToString();
                            sShopID = SlocIDResult.Rows[0]["SHOPID"].ToString();
                        }

                        sBoxid = drChk[0]["BOXID"].ToString();

                        sPkgWay = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "BOXTYPE"));

                        if (sPkgWay == "CRT")
                        {
                            sOWMS_Code = "EG";

                        }
                        else if (sPkgWay == "BOX")
                        {
                            sOWMS_Code = "EB";
                        }

                        /*
                        decimal dSum = 0;
                        decimal dSum2 = 0;
                        decimal dTotal = 0;
                        decimal dTotal2 = 0;

                        for (int i = 0; i < dgPacked_N5_Hist.GetRowCount(); i++)
                        {
                            dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY")));
                            dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY2")));

                            dTotal = dTotal + dSum;
                            dTotal2 = dTotal2 + dSum2;
                        }
                        */
                        decimal dTotal = Convert.ToDecimal(Util.NVC(drChk[0]["TOTAL_QTY"]));
                        decimal dTotal2 = Convert.ToDecimal(Util.NVC(drChk[0]["TOTAL_QTY2"]));

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_SHOPID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("ISS_QTY", typeof(decimal));
                        inData.Columns.Add("ISS_QTY2", typeof(decimal));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("AREA_TYPE_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["FROM_AREAID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "FROM_AREAID");
                        row["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "FROM_SLOC_ID");
                        row["TO_SHOPID"] = sShopID;
                        row["TO_AREAID"] = "";
                        row["TO_SLOC_ID"] = sTO_SLOC_ID;
                        row["ISS_QTY"] = dTotal;
                        row["ISS_QTY2"] = dTotal2;
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = sSHIPTO_ID;
                        row["NOTE"] = "";
                        row["USERID"] = LoginInfo.USERID;
                        row["AREA_TYPE_CODE"] = "E";//GetAreaTypeCode();//"E";

                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inBox = indataSet.Tables.Add("INPALLET");

                        inBox.Columns.Add("BOXID", typeof(string));
                        inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        DataRow row2 = inBox.NewRow();

                        row2["BOXID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "OUTER_BOXID");
                        row2["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;

                        indataSet.Tables["INPALLET"].Rows.Add(row2);


                        DataTable inLot = indataSet.Tables.Add("INBOX");

                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("LOTQTY", typeof(string));
                        inLot.Columns.Add("LOTQTY2", typeof(string));
                        inLot.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        for (int i = 0; i < dgPacked_N5_Hist.GetRowCount(); i++)
                        {
                            DataRow row3 = inLot.NewRow();

                            row3["BOXID"] = sBoxid;
                            row3["LOTID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "LOTID");
                            row3["LOTQTY"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY");
                            row3["LOTQTY2"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY2");
                            row3["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;

                            indataSet.Tables["INBOX"].Rows.Add(row3);
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", "INDATA,INPALLET,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", bizException.Message, bizException.ToString());
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                                txtBoxid.Text = null;
                                txtBoxid.Focus();
                                Search_N5();
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

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PRODUCT_FOR_PACKING", "INDATA,INPALLET,INBOX", null, indataSet);

                        //Util.AlertInfo("SFU1275"); //정상 처리 되었습니다.
                        //Search_N5();

                        #endregion

                    }
                });

                #region 수정...

                //string PkgWayTemp = string.Empty;
                //string sPackageWay = string.Empty;
                //string sOWMS_Code = string.Empty;
                //string m_TrasferLoc = string.Empty;
                //string m_PkgWay = string.Empty;
                //string sBoxid = string.Empty;

                //if (dgPacked_N5.GetRowCount() <= 0)
                //{
                //    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 항목이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    Util.Alert("10008");   //선택된 데이터가 없습니다.
                //    return;
                //}

                //DataRow[] drChk = Util.gridGetChecked(ref dgPacked_N5, "CHK");

                //if (drChk.Length <= 0)
                //{
                //    //Util.AlertInfo("선택된 항목이 없습니다.");
                //    Util.Alert("10008");   //선택된 데이터가 없습니다.
                //    return;
                //}
                //else
                //{
                //    PkgWayTemp = drChk[0]["BOXSTAT"].ToString();
                //    sBoxid = drChk[0]["BOXID"].ToString();
                //}

                //m_TrasferLoc = cboTransLoc.Text;

                //sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드");

                //m_PkgWay = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "BOXTYPE"));

                //if (m_PkgWay == "CRT")
                //{
                //    sPackageWay = sPackageWay + " " + ObjectDic.Instance.GetObjectName("가대");
                //    sOWMS_Code = "EG";

                //}
                //else if (m_PkgWay == "BOX")
                //{
                //    sPackageWay = sPackageWay + " " +  "BOX";
                //    sOWMS_Code = "EB";
                //}

                //if ("CNA".Equals(m_TrasferLoc))
                //{
                //    sPackageWay = sPackageWay + "CNA";
                //}

                //string sUnitCode = string.Empty;

                //sUnitCode = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "UNIT_CODE"));


                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                //DataRow dr = RQSTDT.NewRow();
                //dr["OUTER_BOXID"] = sBoxid;

                //RQSTDT.Rows.Add(dr);

                //DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                //if (OutResult.Rows.Count <= 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장이력 조회중 에러가 발생하였습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    return;
                //}

                //string sVld = string.Empty;
                //string sProdDate = string.Empty;
                //string sVld2 = string.Empty;
                //string sProdDate2 = string.Empty;
                //string sPackingNo = string.Empty;
                //string sModlid = string.Empty;
                //string sFrom = string.Empty;
                //string sTo = string.Empty;
                //double iSum = 0;
                //double iSum2 = 0;
                //double iSum3 = 0;
                //double iSum4 = 0;
                //string sLotid = string.Empty;
                //string sLotid2 = string.Empty;
                //string sVer = string.Empty;
                //string sVer2 = string.Empty;
                //string sLane = string.Empty;
                //string sLane2 = string.Empty;

                //string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                //string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                //sFrom = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "EQSGNAME"));

                //for (int i = 0; i < OutResult.Rows.Count; i++)
                //{
                //    if (i == 0)
                //    {
                //        if (OutResult.Rows[i]["VLD_DATE"].ToString() == "")
                //        {
                //            sVld = null;
                //        }
                //        else
                //        {
                //            string sVld_date = OutResult.Rows[i]["VLD_DATE"].ToString();
                //            sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                //        }

                //        DateTime ProdDate = Convert.ToDateTime(OutResult.Rows[i]["WIPSDTTM"].ToString());
                //        sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                //        sPackingNo = OutResult.Rows[i]["OUTER_BOXID"].ToString();
                //        sModlid = OutResult.Rows[i]["MODLID"].ToString();
                //        //sFrom = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "UNIT_CODE"));
                //        //sTo = Util.NVC(DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "UNIT_CODE"));
                //        iSum = Convert.ToDouble(OutResult.Rows[i]["TOTALQTY"].ToString());
                //        iSum3 = Convert.ToDouble(OutResult.Rows[i]["TOTALQTY2"].ToString());
                //        sVer = OutResult.Rows[i]["PROD_VER_CODE"].ToString();
                //        sLane = OutResult.Rows[i]["LANE"].ToString();

                //        sLotid = OutResult.Rows[i]["CUT_ID"].ToString();
                //        sUnitCode = OutResult.Rows[i]["UNIT_CODE"].ToString();

                //    }
                //    else
                //    {
                //        if (OutResult.Rows[i]["VLD_DATE"].ToString() == "")
                //        {
                //            sVld2 = null;
                //        }
                //        else
                //        {
                //            string sVld_date = OutResult.Rows[i]["VLD_DATE"].ToString();
                //            sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                //        }

                //        DateTime ProdDate = Convert.ToDateTime(OutResult.Rows[i]["WIPSDTTM"].ToString());
                //        sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                //        iSum2 = Convert.ToDouble(OutResult.Rows[i]["TOTALQTY"].ToString());
                //        iSum4 = Convert.ToDouble(OutResult.Rows[i]["TOTALQTY2"].ToString());
                //        sVer2 = OutResult.Rows[i]["PROD_VER_CODE"].ToString();
                //        sLane2 = OutResult.Rows[i]["LANE"].ToString();

                //        sLotid2 = OutResult.Rows[i]["CUT_ID"].ToString();
                //    }
                //}


                //string sTotal_M = string.Empty;
                //string sTotal_C = string.Empty;
                //string sM1 = string.Empty;
                //string sC1 = string.Empty;
                //string sM2 = string.Empty;
                //string sC2 = string.Empty;
                //string sNo1 = string.Empty;
                //string sNo2 = string.Empty;

                //if (OutResult.Rows.Count == 1)
                //{
                //    double total_M = iSum;
                //    sTotal_M = Convert.ToString(total_M);

                //    double total_C = iSum3;
                //    sTotal_C = Convert.ToString(total_C);

                //    sM1 = sTotal_M;
                //    sM2 = null;
                //    sC1 = sTotal_C;
                //    sC2 = null;
                //    sNo1 = "1";

                //}
                //else
                //{
                //    double total_M = iSum + iSum2;
                //    sTotal_M = Convert.ToString(total_M);

                //    double total_C = iSum3 + iSum4;
                //    sTotal_C = Convert.ToString(total_C);


                //    sM1 = Convert.ToString(iSum);
                //    sM2 = Convert.ToString(iSum2);
                //    sC1 = Convert.ToString(iSum3);
                //    sC2 = Convert.ToString(iSum4);
                //    sNo1 = "1";
                //    sNo2 = "2";
                //}

                //dtPackingCard = new DataTable();

                //dtPackingCard.Columns.Add("Title", typeof(string));
                //dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                //dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                //dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                //dtPackingCard.Columns.Add("Transfer", typeof(string));
                //dtPackingCard.Columns.Add("Total_M", typeof(string));
                //dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                //dtPackingCard.Columns.Add("No1", typeof(string));
                //dtPackingCard.Columns.Add("No2", typeof(string));
                //dtPackingCard.Columns.Add("Lot1", typeof(string));
                //dtPackingCard.Columns.Add("Lot2", typeof(string));
                //dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                //dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                //dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                //dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                //dtPackingCard.Columns.Add("V1", typeof(string));
                //dtPackingCard.Columns.Add("V2", typeof(string));
                //dtPackingCard.Columns.Add("L1", typeof(string));
                //dtPackingCard.Columns.Add("L2", typeof(string));
                //dtPackingCard.Columns.Add("M1", typeof(string));
                //dtPackingCard.Columns.Add("M2", typeof(string));
                //dtPackingCard.Columns.Add("C1", typeof(string));
                //dtPackingCard.Columns.Add("C2", typeof(string));
                //dtPackingCard.Columns.Add("REMARK", typeof(string));
                //dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                //dtPackingCard.Columns.Add("V_DATE", typeof(string));
                //dtPackingCard.Columns.Add("P_DATE", typeof(string));

                //DataRow drCrad = null;

                //drCrad = dtPackingCard.NewRow();

                //drCrad.ItemArray = new object[] { sPackageWay,
                //                                  sModlid,
                //                                  sPackingNo,
                //                                  sPackingNo,
                //                                  sFrom + " -> " + m_TrasferLoc,
                //                                  sTotal_M,
                //                                  sTotal_C,
                //                                  sNo1,
                //                                  sNo2,
                //                                  sLotid,
                //                                  sLotid2,
                //                                  sVld,
                //                                  sVld2,
                //                                  sProdDate,
                //                                  sProdDate2,
                //                                  sVer,
                //                                  sVer2,
                //                                  sLane,
                //                                  sLane2,
                //                                  sM1,
                //                                  sM2,
                //                                  sC1,
                //                                  sC2,
                //                                  "",
                //                                  sUnitCode,
                //                                  sV_DATE,
                //                                  sP_DATE
                //                                };

                //dtPackingCard.Rows.Add(drCrad);


                //string sSHIPTO_ID = cboTransLoc.SelectedValue.ToString();
                //string sTO_SLOC_ID = string.Empty;
                //string sShopID = string.Empty;

                //DataTable RQSTDT01 = new DataTable();
                //RQSTDT01.TableName = "RQSTDT";
                //RQSTDT01.Columns.Add("FROM_AREAID", typeof(String));
                //RQSTDT01.Columns.Add("SHIPTO_ID", typeof(String));

                //DataRow dr01 = RQSTDT01.NewRow();
                //dr01["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr01["SHIPTO_ID"] = sSHIPTO_ID;

                //RQSTDT01.Rows.Add(dr01);

                //DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_SLOC_ID", "RQSTDT", "RSLTDT", RQSTDT01);

                //if (SlocIDResult.Rows.Count == 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("To_Location 기준정보가 존재하지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                //else
                //{
                //    sTO_SLOC_ID = SlocIDResult.Rows[0]["TO_SLOC_ID"].ToString();
                //    sShopID = SlocIDResult.Rows[0]["SHOPID"].ToString();
                //}



                //// INDATA 정보 DataTable
                //dtBasicInfo = new DataTable();
                //dtBasicInfo.TableName = "dtBasicInfo";
                //dtBasicInfo.Columns.Add("FROM_AREAID", typeof(string));
                //dtBasicInfo.Columns.Add("FROM_SLOC_ID", typeof(string));
                //dtBasicInfo.Columns.Add("TO_SHOPID", typeof(string));
                //dtBasicInfo.Columns.Add("TO_SLOC_ID", typeof(string));
                //dtBasicInfo.Columns.Add("ISS_QTY", typeof(Decimal));
                //dtBasicInfo.Columns.Add("ISS_QTY2", typeof(Decimal));
                //dtBasicInfo.Columns.Add("ISS_NOTE", typeof(string));
                //dtBasicInfo.Columns.Add("SHIPTO_ID", typeof(string));
                //dtBasicInfo.Columns.Add("NOTE", typeof(string));
                //dtBasicInfo.Columns.Add("USERID", typeof(string));
                //dtBasicInfo.Columns.Add("TYPE", typeof(string));

                //DataRow drInfo = dtBasicInfo.NewRow();
                //drInfo["FROM_AREAID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "FROM_AREAID");
                //drInfo["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "FROM_SLOC_ID");
                //drInfo["TO_SHOPID"] = sShopID;
                //drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                //drInfo["ISS_QTY"] = iSum + iSum2;
                //drInfo["ISS_QTY2"] = iSum3 + iSum4;
                //drInfo["ISS_NOTE"] = "";
                //drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                //drInfo["NOTE"] = "";
                //drInfo["USERID"] = LoginInfo.USERID;
                //drInfo["TYPE"] = "PACK";

                //dtBasicInfo.Rows.Add(drInfo);


                //// INPALLET 정보 DataTable
                //dtSel01 = new DataTable();
                //dtSel01.TableName = "dtSel01";
                //dtSel01.Columns.Add("OUTER_BOXID", typeof(string));
                //dtSel01.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                //DataRow drSel01 = dtSel01.NewRow();

                //drSel01["OUTER_BOXID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[0].DataItem, "OUTER_BOXID");
                //drSel01["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;

                //dtSel01.Rows.Add(drSel01);


                //// INBOX 정보 DataTable
                //dtSel02 = new DataTable();
                //dtSel02.TableName = "dtSel02";
                //dtSel02.Columns.Add("BOXID", typeof(string));
                //dtSel02.Columns.Add("LOTID", typeof(string));
                //dtSel02.Columns.Add("LOTQTY", typeof(string));
                //dtSel02.Columns.Add("LOTQTY2", typeof(string));
                //dtSel02.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                //for (int i = 0; i < dgPacked_N5_Hist.GetRowCount(); i++)
                //{
                //    DataRow drSel02 = dtSel02.NewRow();

                //    drSel02["BOXID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "OUTER_BOXID");
                //    drSel02["LOTID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "LOTID");
                //    drSel02["LOTQTY"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY");
                //    drSel02["LOTQTY2"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "WIPQTY2");
                //    drSel02["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;

                //    dtSel02.Rows.Add(drSel02);
                //}

                //LGC.GMES.MES.BOX001.Report_Packing rs = new LGC.GMES.MES.BOX001.Report_Packing();
                //rs.FrameOperation = this.FrameOperation;

                //if (rs != null)
                //{
                //    // 태그 발행 창 화면에 띄움.
                //    object[] Parameters = new object[5];
                //    Parameters[0] = "PackingCard_New";
                //    Parameters[1] = dtPackingCard;
                //    Parameters[2] = dtBasicInfo;
                //    Parameters[3] = dtSel01;
                //    Parameters[4] = dtSel02;

                //    C1WindowExtension.SetParameters(rs, Parameters);

                //    rs.Closed += new EventHandler(Print_Result);
                //    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                //}
                #endregion

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void fifoCheck(string boxID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = boxID;

                RQSTDT.Rows.Add(dr);
                
               new ClientProxy().ExecuteServiceSync("BR_PRD_CHECK_BOX_FIFO_NJ", "RQSTDT", "", RQSTDT);               
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPacked_N5_Hist.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPacked_N5, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                //포장취소 하시겠습니까?
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3135"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3135", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        decimal iTotal_Qty = 0;
                        decimal iTotal_Qty2 = 0;

                        string sOUTER_BOXID = drChk[0]["BOXID"].ToString();

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["OUTER_BOXID"] = sOUTER_BOXID;

                        RQSTDT.Rows.Add(dr);

                        //DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_NJ_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_NJ_HIST_INNER_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                        if (BoxResult.Rows.Count <= 0)
                        {
                            Util.MessageInfo("SFU3022");    //포장이력을 조회할수 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();

                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("OUTBOXID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("UNPACK_QTY", typeof(decimal));
                        inData.Columns.Add("UNPACK_QTY2", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["OUTBOXID"] = drChk[0]["BOXID"].ToString();
                        row["PRODID"] = drChk[0]["PRODID"].ToString();
                        row["UNPACK_QTY"] = drChk[0]["TOTAL_QTY"];
                        row["UNPACK_QTY2"] = drChk[0]["TOTAL_QTY2"];
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = "";

                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inBox = indataSet.Tables.Add("INNERBOX");
                        inBox.Columns.Add("BOXID", typeof(string));
                        inBox.Columns.Add("PRODID", typeof(string));
                        inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("UNPACK_QTY", typeof(string));
                        inBox.Columns.Add("UNPACK_QTY2", typeof(string));

                        for (int i = 0; i < BoxResult.Rows.Count; i++)
                        {
                            DataRow row2 = inBox.NewRow();

                            row2["BOXID"] = BoxResult.Rows[i]["BOXID"].ToString();
                            row2["PRODID"] = BoxResult.Rows[i]["PRODID"].ToString();
                            row2["PACK_LOT_TYPE_CODE"] = "LOT";
                            row2["UNPACK_QTY"] = BoxResult.Rows[i]["TOTAL_QTY"].ToString();
                            row2["UNPACK_QTY2"] = BoxResult.Rows[i]["TOTAL_QTY2"].ToString();

                            iTotal_Qty += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY"].ToString());
                            iTotal_Qty2 += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY2"].ToString());

                            indataSet.Tables["INNERBOX"].Rows.Add(row2);

                        }

                        DataTable inLot = indataSet.Tables.Add("INLOT");

                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgPacked_N5_Hist.GetRowCount(); i++)
                        {
                            DataRow row3 = inLot.NewRow();

                            row3["BOXID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "BOXID").ToString();
                            row3["LOTID"] = DataTableConverter.GetValue(dgPacked_N5_Hist.Rows[i].DataItem, "LOTID").ToString();

                            indataSet.Tables["INLOT"].Rows.Add(row3);
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_PACKING_NJ", "INDATA,INNERBOX,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_UNPACK_LOT_FOR_PACKING_NJ", searchException.Message, searchException.ToString());
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                Util.gridClear(dgPacked_N5);
                                Util.gridClear(dgPacked_N5_Hist);
                                txtBoxid.Text = null;
                                txtBoxid.Focus();
                                Search_N5();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet
                        );
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgPacked_N5Choice_Checked(object sender, RoutedEventArgs e)
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
                dgPacked_N5.SelectedIndex = idx;

                SearchBox_Detail(idx);
            }
        }

        private void SearchBox_Detail(int idx)
        {
            try
            { 
                string sOUTER_BOXID = string.Empty;

                sOUTER_BOXID = DataTableConverter.GetValue(dgPacked_N5.Rows[idx].DataItem, "BOXID").ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID"] = sOUTER_BOXID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgPacked_N5_Hist);
                Util.GridSetData(dgPacked_N5_Hist, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region N5 출고 이력 조회
        private void txtBoxid2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxid2.Text != "")
                    {
                        Boxmapping_Master();
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Boxmapping_Master();
        }

        private void Boxmapping_Master()
        {
            try
            {
                string sShipToID = string.Empty;
                string sStatus = string.Empty;
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom2.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);

                Util.gridClear(dgOutHIst);
                Util.gridClear(dgOutDetail);

                if (cboStatus2.SelectedIndex < 0 || cboStatus2.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
                }
                else
                {
                    sStatus = cboStatus2.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["OUTER_BOXID"] = txtBoxid2.Text.Trim() == "" ? null : txtBoxid2.Text;
                //dr["SHIPTO_ID"] = Util.NVC(cboTransLoc2.SelectedValue) == "" ? null : Util.NVC(cboTransLoc2.SelectedValue);
                //dr["SHIPTO_ID"] = txtBoxid2.Text.Trim() != "" ? null : Util.NVC(cboTransLoc2.SelectedValue);
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc2, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid2.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid2.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID2.Text.Trim() == "" ? null : txtProd_ID2.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutHIst);
                //dgOutHIst.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgOutHIst, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnCancel2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOutDetail.GetRowCount() <= 0)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                if (drChk.Length <= 0 )
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                if (!drChk[0]["BOX_RCV_ISS_STAT_CODE"].ToString().Equals("SHIPPING"))
                {
                    Util.MessageValidation("SFU1939");   //취소할수있는상태가아닙니다.
                    return;
                }

                //출고취소 하시겠습니까?
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3136"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3136", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        decimal iTotal_Qty = 0;
                        decimal iTotal_Qty2 = 0;

                        string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();


                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["RCV_ISS_ID"] = sRcv_Iss_Id;

                        RQSTDT.Rows.Add(dr);

                        //DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST", "RQSTDT", "RSLTDT", RQSTDT);
                        DataTable BoxResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_INNER_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                        if (BoxResult.Rows.Count <= 0)
                        {
                            Util.MessageInfo("SFU3023");    //출고이력을 조회할수 없습니다.
                            return;
                        }

                        DataSet indataSet = new DataSet();

                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("AREAID", typeof(string));
                        inData.Columns.Add("OUTBOXID", typeof(string));
                        inData.Columns.Add("PRODID", typeof(string));
                        inData.Columns.Add("UNPACK_QTY", typeof(decimal));
                        inData.Columns.Add("UNPACK_QTY2", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("AREA_TYPE_CODE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = "UI";
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["OUTBOXID"] = drChk[0]["OUTER_BOXID"].ToString();
                        row["PRODID"] = drChk[0]["PRODID"].ToString();
                        row["UNPACK_QTY"] = drChk[0]["TOTAL_QTY"];
                        row["UNPACK_QTY2"] = drChk[0]["TOTAL_QTY2"];
                        row["USERID"] = LoginInfo.USERID;
                        row["NOTE"] = "";
                        row["AREA_TYPE_CODE"] = "E";

                        indataSet.Tables["INDATA"].Rows.Add(row);


                        DataTable inBox = indataSet.Tables.Add("INNERBOX");
                        inBox.Columns.Add("BOXID", typeof(string));
                        inBox.Columns.Add("PRODID", typeof(string));
                        inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                        inBox.Columns.Add("UNPACK_QTY", typeof(string));
                        inBox.Columns.Add("UNPACK_QTY2", typeof(string));

                        for (int i = 0; i < BoxResult.Rows.Count; i++)
                        {
                            DataRow row2 = inBox.NewRow();

                            row2["BOXID"] = BoxResult.Rows[i]["BOXID"].ToString();
                            row2["PRODID"] = BoxResult.Rows[i]["PRODID"].ToString();
                            row2["PACK_LOT_TYPE_CODE"] = "LOT";
                            row2["UNPACK_QTY"] = BoxResult.Rows[i]["TOTAL_QTY"].ToString();
                            row2["UNPACK_QTY2"] = BoxResult.Rows[i]["TOTAL_QTY2"].ToString();

                            iTotal_Qty += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY"].ToString());
                            iTotal_Qty2 += Convert.ToDecimal(BoxResult.Rows[i]["TOTAL_QTY2"].ToString());

                            indataSet.Tables["INNERBOX"].Rows.Add(row2);

                        }

                        DataTable inLot = indataSet.Tables.Add("INLOT");

                        inLot.Columns.Add("BOXID", typeof(string));
                        inLot.Columns.Add("LOTID", typeof(string));

                        for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                        {
                            DataRow row3 = inLot.NewRow();

                            row3["BOXID"] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "BOXID").ToString();
                            row3["LOTID"] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "LOTID").ToString();

                            indataSet.Tables["INLOT"].Rows.Add(row3);
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_PACKING", "INDATA,INNERBOX,INLOT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    //Util.AlertByBiz("BR_PRD_REG_UNPACK_LOT_FOR_PACKING", searchException.Message, searchException.ToString());
                                    Util.MessageException(searchException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                                Util.gridClear(dgOutDetail);
                                Util.gridClear(dgOutHIst);
                                txtBoxid2.Text = null;
                                txtBoxid2.Focus();
                                Boxmapping_Master();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPacked_N5.GetRowCount() == 0)
                {
                    Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPacked_N5, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.MessageValidation("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                string sType = string.Empty;
                string sBoxid = drChk[0]["BOXID"].ToString();

                //
                //1. BOXID의 재공정보 확인
                //
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID"] = sBoxid;

                RQSTDT.Rows.Add(dr);

                DataTable Reprint_Main = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_PRINT", "RQSTDT", "RSLTDT", RQSTDT);

                if (Reprint_Main.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                //
                //2.BOX의 재공정보 확인 및 SUB_BOX 정보 및 포함된 LANE 수량 정보
                //
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["OUTER_BOXID"] = sBoxid;

                RQSTDT1.Rows.Add(dr1);

                DataTable Reprint_Sub = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_PRINT_SUB_CELL", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Reprint_Sub.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                //
                //3.BOX에 포장된 PANCAKE의 CSTID 정보
                //
                //DataTable RQSTDT2 = new DataTable();
                //RQSTDT2.TableName = "RQSTDT";
                //RQSTDT2.Columns.Add("OUTER_BOXID", typeof(String));

                //DataRow dr2 = RQSTDT2.NewRow();
                //dr2["OUTER_BOXID"] = sBoxid;

                //RQSTDT2.Rows.Add(dr2);

                //DataTable Result_Skid = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_SKID", "RQSTDT", "RSLTDT", RQSTDT2);

                //if (Result_Skid.Rows.Count == 1 && Result_Skid.Rows[0]["CSTID"].ToString().Length > 0)
                //{
                    //3.1 BOX에 포함된 SUB_BOX정보(갯수)
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("OUTER_BOXID", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["OUTER_BOXID"] = sBoxid;

                    RQSTDT3.Rows.Add(dr3);

                    DataTable dtCnt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTER_BOXID_CNT", "RQSTDT", "RSLTDT", RQSTDT3);

                    if (dtCnt.Rows.Count > 1)
                    {
                        sType = "TWO";
                    }
                    else
                    {
                        sType = "ONE";
                    }
                //}
                //else
                //{
                //    sType = "ONE";
                //}

                string sBoxtype = string.Empty;

                if (Reprint_Main.Rows[0]["BOXTYPE"].ToString() == "CRT")
                {
                    sBoxtype = ObjectDic.Instance.GetObjectName("가대") + "#" + Reprint_Sub.Rows.Count;
                }
                else
                {
                    sBoxtype = "BOX";
                }

                string sPackageWay = ObjectDic.Instance.GetObjectName("조립포장카드") + " " + sBoxtype;
                string sVld = string.Empty;
                string sProdDate = string.Empty;
                string sVld2 = string.Empty;
                string sProdDate2 = string.Empty;
                string sPackingNo = string.Empty;
                string sPackingNo2 = string.Empty;
                string sModlid = string.Empty;
                string sFrom = string.Empty;
                string sTo = string.Empty;
                double iSum = 0;
                double iSum2 = 0;
                double iSum3 = 0;
                double iSum4 = 0;
                string sLotid = string.Empty;
                string sLotid2 = string.Empty;
                string sVer = string.Empty;
                string sVer2 = string.Empty;
                string sLane = string.Empty;
                string sLane2 = string.Empty;
                string sUnitCode = string.Empty;
                string sNote = string.Empty;
                string sOper_Desc = string.Empty;
                string sD1 = string.Empty;
                string sD2 = string.Empty;
                string sAbbrCode = string.Empty;

                // 환산자 처리
                double iConvSum = 0;
                double iConvSum2 = 0;
                double iConvSum3 = 0;
                double iConvSum4 = 0;

                string sTotal_M = string.Empty;
                string sTotal_C = string.Empty;
                string sTotal_M2 = string.Empty;
                string sTotal_C2 = string.Empty;

                string sM1 = string.Empty;
                string sC1 = string.Empty;
                string sM2 = string.Empty;
                string sC2 = string.Empty;
                string sNo1 = string.Empty;
                string sNo2 = string.Empty;

                string sPrjt_name = string.Empty;
                string sElec = string.Empty;

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

               

                if (sType == "ONE")
                {
                    for (int i = 0; i < Reprint_Sub.Rows.Count; i++) //SUB_BOX의 갯수만큼 루핑 : 조립은 SUB_BOX가 한개임.
                    {
                        if (i == 0)
                        {
                            if (Reprint_Sub.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = Reprint_Sub.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate = Convert.ToDateTime(Reprint_Sub.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                            //BOXID 정보 변수에 담아두기
                            sPackingNo = Reprint_Main.Rows[i]["OUTER_BOXID"].ToString();
                            sModlid    = Reprint_Main.Rows[i]["MODLID"].ToString();
                            sFrom      = Reprint_Main.Rows[i]["EQSGNAME"].ToString();
                            sTo        = Reprint_Main.Rows[i]["SHIPTO_NAME"].ToString();
                            sUnitCode  = Reprint_Main.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode  = Reprint_Main.Rows[i]["PRDT_ABBR_CODE"].ToString();
                            sNote      = Reprint_Main.Rows[i]["PACK_NOTE"].ToString();
                            sOper_Desc = Reprint_Main.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            sPrjt_name = Reprint_Main.Rows[i]["PRJT_NAME"].ToString();
                            sElec = Reprint_Main.Rows[i]["ELEC"].ToString();

                            //SUB_BOX 정보 변수에 담아두기
                            iSum   = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY"].ToString());
                            iSum3  = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY2"].ToString());
                            sVer   = Reprint_Sub.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane  = Reprint_Sub.Rows[i]["LANE"].ToString();
                            sLotid = Reprint_Sub.Rows[i]["BOXID"].ToString();    

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(Reprint_Sub.Rows[i]["PRODID"])));

                                iConvSum  = Convert.ToDouble(Util.NVC_Decimal(iSum)  * sConvLength) / 1000;
                                iConvSum3 = Convert.ToDouble(Util.NVC_Decimal(iSum3) * sConvLength) / 1000;
                            }
                        }
                        else
                        {
                            //sPackingNo2 = Reprint_Main.Rows[i]["OUTER_BOXID"].ToString();

                            if (Reprint_Sub.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld2 = null;
                            }
                            else
                            {
                                string sVld_date = Reprint_Sub.Rows[i]["VLD_DATE"].ToString();
                                sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate = Convert.ToDateTime(Reprint_Sub.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                            iSum2 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY"].ToString());
                            iSum4 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY2"].ToString());
                            sVer2 = Reprint_Sub.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane2 = Reprint_Sub.Rows[i]["LANE"].ToString();

                            sLotid2 = Reprint_Sub.Rows[i]["CSTID"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                iConvSum2 = Convert.ToDouble(Util.NVC_Decimal(iSum2) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Reprint_Sub.Rows[i]["PRODID"]))) / 1000);
                                iConvSum4 = Convert.ToDouble(Util.NVC_Decimal(iSum4) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Reprint_Sub.Rows[i]["PRODID"]))) / 1000);
                            }
                        }
                    } //FOR 마지막



                    if (Reprint_Sub.Rows.Count == 1)
                    {
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            double total_M = iSum;
                            sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", iConvSum) + "M";

                            double total_C = iSum3;
                            sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", iConvSum3) + "M";
                        }
                        else
                        {
                            double total_M = iSum;
                            sTotal_M = String.Format("{0:#,##0}", total_M);

                            double total_C = iSum3;
                            sTotal_C = String.Format("{0:#,##0}", total_C);
                        }

                        sNo1 = "1";
                        sNo2 = null;
                        sM1 = String.Format("{0:#,##0}", iSum);
                        sM2 = "";
                        sC1 = String.Format("{0:#,##0}", iSum3);
                        sC2 = "";

                        if (string.Equals(sUnitCode, "EA"))
                            sD1 = String.Format("{0:#,##0}", iConvSum3);
                        else
                            sD1 = String.Format("{0:#,##0}", iSum3);

                        sD2 = "";
                    }
                    else
                    {
                        if (string.Equals(sUnitCode, "EA"))
                        {
                            double total_M = iSum + iSum2;
                            sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", (iConvSum + iConvSum2)) + "M";

                            double total_C = iSum3 + iSum4;
                            sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", (iConvSum3 + iConvSum4)) + "M";
                        }
                        else
                        {
                            double total_M = iSum + iSum2;
                            sTotal_M = String.Format("{0:#,##0}", total_M);

                            double total_C = iSum3 + iSum4;
                            sTotal_C = String.Format("{0:#,##0}", total_C);
                        }

                        sNo1 = "1";
                        sNo2 = "2";
                        sM1 = String.Format("{0:#,##0}", iSum);
                        sM2 = String.Format("{0:#,##0}", iSum2);
                        sC1 = String.Format("{0:#,##0}", iSum3);
                        sC2 = String.Format("{0:#,##0}", iSum4);

                        if (string.Equals(sUnitCode, "EA"))
                        {
                            sD1 = String.Format("{0:#,##0}", iConvSum3);
                            sD2 = String.Format("{0:#,##0}", iConvSum4);
                        }
                        else
                        {
                            sD1 = String.Format("{0:#,##0}", iSum3);
                            sD2 = String.Format("{0:#,##0}", iSum4);
                        }
                    }

                    DataTable dtReprint = new DataTable();
                    dtReprint.Columns.Add("Title", typeof(string));
                    dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                    dtReprint.Columns.Add("PRJT_NAME", typeof(string));
                    dtReprint.Columns.Add("PACK_NO", typeof(string));
                    dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                    dtReprint.Columns.Add("Transfer", typeof(string));

                    dtReprint.Columns.Add("Total_M", typeof(string));
                    dtReprint.Columns.Add("Total_Cell", typeof(string));
                    dtReprint.Columns.Add("No1", typeof(string));
                    dtReprint.Columns.Add("No2", typeof(string));
                    dtReprint.Columns.Add("Lot1", typeof(string));

                    dtReprint.Columns.Add("Lot2", typeof(string));
                    dtReprint.Columns.Add("VLD_DATE1", typeof(string));
                    dtReprint.Columns.Add("VLD_DATE2", typeof(string));
                    dtReprint.Columns.Add("REG_DATE1", typeof(string));
                    dtReprint.Columns.Add("REG_DATE2", typeof(string));

                    dtReprint.Columns.Add("V1", typeof(string));
                    dtReprint.Columns.Add("V2", typeof(string));
                    dtReprint.Columns.Add("L1", typeof(string));
                    dtReprint.Columns.Add("L2", typeof(string));
                    dtReprint.Columns.Add("M1", typeof(string));

                    dtReprint.Columns.Add("M2", typeof(string));
                    dtReprint.Columns.Add("C1", typeof(string));
                    dtReprint.Columns.Add("C2", typeof(string));
                    dtReprint.Columns.Add("D1", typeof(string));
                    dtReprint.Columns.Add("D2", typeof(string));

                    dtReprint.Columns.Add("REMARK", typeof(string));
                    dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                    dtReprint.Columns.Add("V_DATE", typeof(string));
                    dtReprint.Columns.Add("P_DATE", typeof(string));
                    dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                    dtReprint.Columns.Add("ELEC", typeof(string));
                    dtReprint.Columns.Add("PRODID", typeof(string));

                    DataRow drCrad = null;

                    drCrad = dtReprint.NewRow();

                    drCrad.ItemArray = new object[] { sPackageWay,
                                                  sModlid,
                                                  "/" + sPrjt_name,
                                                  sPackingNo,
                                                  "*" + sPackingNo + "*",
                                                  sFrom + " -> " + sTo,
                                                  sTotal_M,
                                                  sTotal_C,
                                                  sNo1,
                                                  sNo2,
                                                  sLotid,
                                                  sLotid2,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  sVer,
                                                  sVer2,
                                                  sLane,
                                                  sLane2,
                                                  sM1,
                                                  sM2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  sElec,
                                                  Util.NVC(DataTableConverter.GetValue(dgPacked_N5.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                };

                    dtReprint.Rows.Add(drCrad);

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        Parameters[0] = "PackingCard_Assy";
                        Parameters[1] = dtReprint;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        // this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                        grdMain.Children.Add(rs);
                        rs.BringToFront();

                    }
                }
                else //2가대일 경우
                {

                    for (int i = 0; i < Reprint_Sub.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (Reprint_Sub.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = Reprint_Sub.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate = Convert.ToDateTime(Reprint_Sub.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                            sPackingNo = Reprint_Main.Rows[i]["OUTER_BOXID"].ToString();
                            sModlid = Reprint_Main.Rows[i]["MODLID"].ToString();
                            sFrom = Reprint_Main.Rows[i]["EQSGNAME"].ToString();
                            sTo = Reprint_Main.Rows[i]["SHIPTO_NAME"].ToString();
                            sUnitCode = Reprint_Main.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = Reprint_Main.Rows[i]["PRDT_ABBR_CODE"].ToString();
                            sNote = Reprint_Main.Rows[i]["PACK_NOTE"].ToString();
                            sOper_Desc = Reprint_Main.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            sPrjt_name = Reprint_Main.Rows[i]["PRJT_NAME"].ToString();
                            sElec = Reprint_Main.Rows[i]["ELEC"].ToString();

                            iSum = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY2"].ToString());
                            sVer = Reprint_Sub.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = Reprint_Sub.Rows[i]["LANE"].ToString();
                            sLotid = Reprint_Sub.Rows[i]["BOXID"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(Reprint_Sub.Rows[i]["PRODID"])));

                                iConvSum = Convert.ToDouble(Util.NVC_Decimal(iSum) * sConvLength) / 1000;
                                iConvSum3 = Convert.ToDouble(Util.NVC_Decimal(iSum3) * sConvLength) / 1000;
                            }
                        }
                        else
                        {
                            sPackingNo2 = Reprint_Sub.Rows[i]["BOXID"].ToString();

                            if (Reprint_Sub.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld2 = null;
                            }
                            else
                            {
                                string sVld_date = Reprint_Sub.Rows[i]["VLD_DATE"].ToString();
                                sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate = Convert.ToDateTime(Reprint_Sub.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                            iSum2 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY"].ToString());
                            iSum4 = Convert.ToDouble(Reprint_Sub.Rows[i]["TOTALQTY2"].ToString());
                            sVer2 = Reprint_Sub.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane2 = Reprint_Sub.Rows[i]["LANE"].ToString();
                            sLotid2 = Reprint_Sub.Rows[i]["BOXID"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                decimal sConvLength = Util.NVC_Decimal(GetPatternLength(Util.NVC(Reprint_Sub.Rows[i]["PRODID"])));

                                iConvSum2 = Convert.ToDouble(Util.NVC_Decimal(iSum2) * sConvLength) / 1000;
                                iConvSum4 = Convert.ToDouble(Util.NVC_Decimal(iSum4) * sConvLength) / 1000;
                            }
                        }
                    }

                    if (string.Equals(sUnitCode, "EA"))
                    {
                        double total_M = iSum;
                        sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", iConvSum) + "M";

                        double total_C = iSum3;
                        sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", iConvSum3) + "M";

                        double total_M2 = iSum2;
                        sTotal_M2 = String.Format("{0:#,##0}", total_M2) + "/" + String.Format("{0:#,##0}", (iConvSum2)) + "M";

                        double total_C2 = iSum4;
                        sTotal_C2 = String.Format("{0:#,##0}", total_C2) + "/" + String.Format("{0:#,##0}", (iConvSum4)) + "M";
                    }
                    else
                    {
                        double total_M = iSum;
                        sTotal_M = String.Format("{0:#,##0}", total_M);

                        double total_C = iSum3;
                        sTotal_C = String.Format("{0:#,##0}", total_C);

                        double total_M2 = iSum2;
                        sTotal_M2 = String.Format("{0:#,##0}", total_M2);

                        double total_C2 = iSum4;
                        sTotal_C2 = String.Format("{0:#,##0}", total_C2);
                    }

                    sM1 = String.Format("{0:#,##0}", iSum);
                    sM2 = String.Format("{0:#,##0}", iSum2);
                    sC1 = String.Format("{0:#,##0}", iSum3);
                    sC2 = String.Format("{0:#,##0}", iSum4);

                    if (string.Equals(sUnitCode, "EA"))
                    {
                        sD1 = String.Format("{0:#,##0}", iConvSum3);
                        sD2 = String.Format("{0:#,##0}", iConvSum4);
                    }
                    else
                    {
                        sD1 = String.Format("{0:#,##0}", iSum3);
                        sD2 = String.Format("{0:#,##0}", iSum4);
                    }

                    dtPackingCard = new DataTable();

                    dtPackingCard.Columns.Add("Title", typeof(string));
                    dtPackingCard.Columns.Add("Title1", typeof(string));
                    dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                    dtPackingCard.Columns.Add("MODEL_NAME1", typeof(string));
                    dtPackingCard.Columns.Add("PRJT_NAME", typeof(string));
                    dtPackingCard.Columns.Add("PRJT_NAME1", typeof(string));
                    dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                    dtPackingCard.Columns.Add("PACK_NO1", typeof(string));
                    dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                    dtPackingCard.Columns.Add("HEAD_BARCODE1", typeof(string));
                    dtPackingCard.Columns.Add("Transfer", typeof(string));
                    dtPackingCard.Columns.Add("Transfer1", typeof(string));
                    dtPackingCard.Columns.Add("Total_M", typeof(string));
                    dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                    dtPackingCard.Columns.Add("Total_M1", typeof(string));
                    dtPackingCard.Columns.Add("Total_Cell1", typeof(string));
                    dtPackingCard.Columns.Add("No1", typeof(string));
                    dtPackingCard.Columns.Add("No2", typeof(string));
                    dtPackingCard.Columns.Add("Lot1", typeof(string));
                    dtPackingCard.Columns.Add("Lot2", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                    dtPackingCard.Columns.Add("V1", typeof(string));
                    dtPackingCard.Columns.Add("V2", typeof(string));
                    dtPackingCard.Columns.Add("L1", typeof(string));
                    dtPackingCard.Columns.Add("L2", typeof(string));
                    dtPackingCard.Columns.Add("M1", typeof(string));
                    dtPackingCard.Columns.Add("M2", typeof(string));
                    dtPackingCard.Columns.Add("C1", typeof(string));
                    dtPackingCard.Columns.Add("C2", typeof(string));
                    dtPackingCard.Columns.Add("D1", typeof(string));
                    dtPackingCard.Columns.Add("D2", typeof(string));
                    dtPackingCard.Columns.Add("REMARK", typeof(string));
                    dtPackingCard.Columns.Add("REMARK1", typeof(string));
                    dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                    dtPackingCard.Columns.Add("UNIT_CODE1", typeof(string));
                    dtPackingCard.Columns.Add("V_DATE", typeof(string));
                    dtPackingCard.Columns.Add("P_DATE", typeof(string));
                    dtPackingCard.Columns.Add("V_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("P_DATE1", typeof(string));
                    dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                    dtPackingCard.Columns.Add("OFFER_DESC1", typeof(string));
                    dtPackingCard.Columns.Add("ELEC", typeof(string));
                    dtPackingCard.Columns.Add("ELEC1", typeof(string));
                    dtPackingCard.Columns.Add("PRODID", typeof(string));
                    dtPackingCard.Columns.Add("PRODID1", typeof(string));

                    DataRow drCrad = null;

                    drCrad = dtPackingCard.NewRow();

                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("조립포장카드") + " " + ObjectDic.Instance.GetObjectName("1가대"),
                                                   ObjectDic.Instance.GetObjectName("조립포장카드") + " " + ObjectDic.Instance.GetObjectName("2가대"),
                                                  sModlid,
                                                  sModlid,
                                                  "/" + sPrjt_name,
                                                  "/" + sPrjt_name,
                                                  //sPackingNo,
                                                  //sPackingNo2,
                                                  //sPackingNo,
                                                  //sPackingNo2,
                                                  sPackingNo,
                                                  sPackingNo,
                                                  "*" + sPackingNo + "*",
                                                  "*" + sPackingNo + "*",
                                                  sFrom + " -> " + sTo,
                                                  sFrom + " -> " + sTo,
                                                  sTotal_M,
                                                  sTotal_C,
                                                  sTotal_M2,
                                                  sTotal_C2,
                                                  "1",
                                                  "1",
                                                  sLotid,
                                                  sLotid2,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  sVer,
                                                  sVer2,
                                                  sLane,
                                                  sLane2,
                                                  sM1,
                                                  sM2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sNote,
                                                  sUnitCode,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  sOper_Desc,
                                                  sElec,
                                                  sElec,
                                                  Util.NVC(DataTableConverter.GetValue(dgPacked_N5.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgPacked_N5.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                               };

                    dtPackingCard.Rows.Add(drCrad);

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        Parameters[0] = "PackingCard_2CRT_Assy";
                        Parameters[1] = dtPackingCard;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        // this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {   
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRODID"] = prodID;

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_MBOM_FOR_PRODID", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["MTRL_QTY"])))
                        return Util.NVC_Decimal(result.Rows[0]["MTRL_QTY"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        #region Old Version
        //private void btnReprint_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (dgOutHIst.GetRowCount() == 0)
        //        {
        //            Util.Alert("10008");   //선택된 데이터가 없습니다.
        //            return;
        //        }

        //        DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

        //        if (drChk.Length <= 0)
        //        {
        //            Util.Alert("10008");   //선택된 데이터가 없습니다.
        //            return;
        //        }

        //        string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(String));
        //        RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["RCV_ISS_ID"] = sRcv_Iss_Id;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable ReprintResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (ReprintResult.Rows.Count <= 0)
        //        {
        //            Util.Alert("SFU1870");   //재공 정보가 없습니다.
        //            return;
        //        }

        //        string sBoxtype = string.Empty;

        //        if (ReprintResult.Rows[0]["BOXTYPE"].ToString() == "CRT")
        //        {
        //            sBoxtype = ObjectDic.Instance.GetObjectName("가대") + "#" + ReprintResult.Rows.Count;
        //        }
        //        else
        //        {
        //            sBoxtype = "BOX";
        //        }


        //        DataTable RQSTDT1 = new DataTable();
        //        RQSTDT1.TableName = "RQSTDT";
        //        RQSTDT1.Columns.Add("LANGID", typeof(String));
        //        RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));

        //        DataRow dr1 = RQSTDT1.NewRow();
        //        dr1["LANGID"] = LoginInfo.LANGID;
        //        dr1["RCV_ISS_ID"] = sRcv_Iss_Id;

        //        RQSTDT1.Rows.Add(dr1);

        //        DataTable CutidResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_CUTID", "RQSTDT", "RSLTDT", RQSTDT1);

        //        if (CutidResult.Rows.Count <= 0)
        //        {
        //            Util.Alert("SFU1870");   //재공 정보가 없습니다.
        //            return;
        //        }


        //        //string sPackageWay = "전극포장카드";
        //        string sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드") + " " + sBoxtype;
        //        string sVld = string.Empty;
        //        string sProdDate = string.Empty;
        //        string sVld2 = string.Empty;
        //        string sProdDate2 = string.Empty;
        //        string sPackingNo = string.Empty;
        //        string sModlid = string.Empty;
        //        string sFrom = string.Empty;
        //        string sTo = string.Empty;
        //        double iSum = 0;
        //        double iSum2 = 0;
        //        double iSum3 = 0;
        //        double iSum4 = 0;
        //        string sLotid = string.Empty;
        //        string sLotid2 = string.Empty;
        //        string sVer = string.Empty;
        //        string sVer2 = string.Empty;
        //        string sLane = string.Empty;
        //        string sLane2 = string.Empty;
        //        string sUnitCode = string.Empty;
        //        string sNote = string.Empty;

        //        string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
        //        string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

        //        for (int i = 0; i < ReprintResult.Rows.Count; i++)
        //        {
        //            if (i == 0)
        //            {
        //                if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
        //                {
        //                    sVld = null;
        //                }
        //                else
        //                {
        //                    string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
        //                    sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
        //                }

        //                DateTime ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
        //                sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

        //                sPackingNo = ReprintResult.Rows[i]["OUTER_BOXID"].ToString();
        //                sModlid = ReprintResult.Rows[i]["MODLID"].ToString();
        //                sFrom = ReprintResult.Rows[i]["FROM_AREA"].ToString();
        //                sTo = ReprintResult.Rows[i]["SHIPTO_NAME"].ToString();
        //                iSum = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
        //                iSum3 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
        //                sVer = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
        //                sLane = ReprintResult.Rows[i]["LANE"].ToString();

        //                sLotid = CutidResult.Rows[i]["CSTID"].ToString();
        //                sUnitCode = ReprintResult.Rows[i]["UNIT_CODE"].ToString();

        //                if (drChk[0]["PACK_NOTE"].ToString() == "")
        //                {
        //                    sNote = null;
        //                }
        //                else
        //                {
        //                    sNote = drChk[0]["PACK_NOTE"].ToString();
        //                }


        //                //if (ReprintResult.Rows[i]["ISS_NOTE"].ToString() == "")
        //                //{
        //                //    sNote = null;
        //                //}
        //                //else
        //                //{
        //                //    sNote = ReprintResult.Rows[i]["ISS_NOTE"].ToString();
        //                //}

        //            }
        //            else
        //            {
        //                if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
        //                {
        //                    sVld2 = null;
        //                }
        //                else
        //                {
        //                    string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
        //                    sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
        //                }

        //                DateTime ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
        //                sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

        //                iSum2 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY"].ToString());
        //                iSum4 = Convert.ToDouble(ReprintResult.Rows[i]["TOTALQTY2"].ToString());
        //                sVer2 = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
        //                sLane2 = ReprintResult.Rows[i]["LANE"].ToString();

        //                sLotid2 = CutidResult.Rows[i]["CUT_ID"].ToString();
        //            }
        //        }


        //        string sTotal_M = string.Empty;
        //        string sTotal_C = string.Empty;
        //        string sM1 = string.Empty;
        //        string sC1 = string.Empty;
        //        string sM2 = string.Empty;
        //        string sC2 = string.Empty;

        //        if (ReprintResult.Rows.Count == 1)
        //        {
        //            double total_M = iSum;
        //            sTotal_M = Convert.ToString(total_M);

        //            double total_C = iSum3;
        //            sTotal_C = Convert.ToString(total_C);

        //            sM1 = sTotal_M;
        //            sM2 = "";
        //            sC1 = sTotal_C;
        //            sC2 = "";

        //        }
        //        else
        //        {
        //            double total_M = iSum + iSum2;
        //            sTotal_M = Convert.ToString(total_M);

        //            double total_C = iSum3 + iSum4;
        //            sTotal_C = Convert.ToString(total_C);


        //            sM1 = Convert.ToString(iSum);
        //            sM2 = Convert.ToString(iSum2);
        //            sC1 = Convert.ToString(iSum3);
        //            sC2 = Convert.ToString(iSum4);
        //        }



        //        //double dTotal = Convert.ToDouble(sSum) + Convert.ToDouble(sSum2);
        //        //int dTotal = Convert.ToInt32(sSum) + Convert.ToInt32(sSum2);


        //        DataTable dtReprint = new DataTable();
        //        dtReprint.Columns.Add("Title", typeof(string));
        //        dtReprint.Columns.Add("MODEL_NAME", typeof(string));
        //        dtReprint.Columns.Add("PACK_NO", typeof(string));
        //        dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
        //        dtReprint.Columns.Add("Transfer", typeof(string));
        //        dtReprint.Columns.Add("Total_M", typeof(string));
        //        dtReprint.Columns.Add("Total_Cell", typeof(string));
        //        dtReprint.Columns.Add("No1", typeof(string));
        //        dtReprint.Columns.Add("No2", typeof(string));
        //        dtReprint.Columns.Add("Lot1", typeof(string));
        //        dtReprint.Columns.Add("Lot2", typeof(string));
        //        dtReprint.Columns.Add("VLD_DATE1", typeof(string));
        //        dtReprint.Columns.Add("VLD_DATE2", typeof(string));
        //        dtReprint.Columns.Add("REG_DATE1", typeof(string));
        //        dtReprint.Columns.Add("REG_DATE2", typeof(string));
        //        dtReprint.Columns.Add("V1", typeof(string));
        //        dtReprint.Columns.Add("V2", typeof(string));
        //        dtReprint.Columns.Add("L1", typeof(string));
        //        dtReprint.Columns.Add("L2", typeof(string));
        //        dtReprint.Columns.Add("M1", typeof(string));
        //        dtReprint.Columns.Add("M2", typeof(string));
        //        dtReprint.Columns.Add("C1", typeof(string));
        //        dtReprint.Columns.Add("C2", typeof(string));
        //        dtReprint.Columns.Add("REMARK", typeof(string));
        //        dtReprint.Columns.Add("UNIT_CODE", typeof(string));
        //        dtReprint.Columns.Add("V_DATE", typeof(string));
        //        dtReprint.Columns.Add("P_DATE", typeof(string));

        //        DataRow drCrad = null;

        //        drCrad = dtReprint.NewRow();

        //        drCrad.ItemArray = new object[] { sPackageWay,
        //                                          sModlid,
        //                                          sPackingNo,
        //                                          sPackingNo,
        //                                          sFrom + " -> " + sTo,
        //                                          sTotal_M,
        //                                          sTotal_C,
        //                                          "1",
        //                                          "2",
        //                                          sLotid,
        //                                          sLotid2,
        //                                          sVld,
        //                                          sVld2,
        //                                          sProdDate,
        //                                          sProdDate2,
        //                                          sVer,
        //                                          sVer2,
        //                                          sLane,
        //                                          sLane2,
        //                                          sM1,
        //                                          sM2,
        //                                          sC1,
        //                                          sC2,
        //                                          sNote,
        //                                          sUnitCode,
        //                                          sV_DATE,
        //                                          sP_DATE
        //                                        };

        //        dtReprint.Rows.Add(drCrad);

        //        //object[] Parameters = new object[2];
        //        //Parameters[0] = "PackingCard_New";
        //        //Parameters[1] = dtReprint;

        //        //LGC.GMES.MES.BOX001.Report rs = new LGC.GMES.MES.BOX001.Report();
        //        //C1WindowExtension.SetParameters(rs, Parameters);
        //        //rs.Show();

        //        LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
        //        rs.FrameOperation = this.FrameOperation;

        //        if (rs != null)
        //        {
        //            //태그 발행 창 화면에 띄움.
        //            object[] Parameters = new object[2];
        //            Parameters[0] = "PackingCard_New";
        //            Parameters[1] = dtReprint;

        //            C1WindowExtension.SetParameters(rs, Parameters);

        //            rs.Closed += new EventHandler(Print_Result);
        //            // 팝업 화면 숨겨지는 문제 수정.
        //            // this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
        //            grdMain.Children.Add(rs);
        //            rs.BringToFront();

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //        return;
        //    }
        //}

        #endregion

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Common wndPopup = sender as LGC.GMES.MES.BOX001.Report_Common;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgOutHIstChoice_Checked(object sender, RoutedEventArgs e)
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
                dgOutHIst.SelectedIndex = idx;

                Boxmapping_Detail(idx);

            }
        }

        private void Boxmapping_Detail(int idx)
        {
            try
            {
                string sRCV_ISS_ID = string.Empty;
                string sOUTER_BOXID = string.Empty;

                sRCV_ISS_ID = DataTableConverter.GetValue(dgOutHIst.Rows[idx].DataItem, "RCV_ISS_ID").ToString();
                sOUTER_BOXID = DataTableConverter.GetValue(dgOutHIst.Rows[idx].DataItem, "OUTER_BOXID").ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["OUTER_BOXID"] = sOUTER_BOXID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutDetail);
                Util.GridSetData(dgOutDetail, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region 통문증 발행
        private void btnSearch6_Click(object sender, RoutedEventArgs e)
        {
            OutHist();
        }

        private void txtBoxid6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid6.Text != "")
                {
                    OutHist();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void OutHist()
        {
            try
            {
                string sShipToID = string.Empty;
                string sStatus = string.Empty;
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFromForPrint.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateToForPrint.SelectedDateTime);

                if (cboStatus6.SelectedIndex < 0 || cboStatus6.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
                }
                else
                {
                    sStatus = cboStatus6.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["OUTER_BOXID"] = txtBoxid6.Text.Trim() == "" ? null : txtBoxid6.Text;
                //dr["SHIPTO_ID"] = Util.NVC(cboTransLoc6.SelectedValue) == "" ? null : Util.NVC(cboTransLoc6.SelectedValue);
                dr["SHIPTO_ID"] = txtBoxid6.Text.Trim() != "" ? null : Util.NVC(cboTransLoc6.SelectedValue);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid6.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid6.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID6.Text.Trim() == "" ? null : txtProd_ID6.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgGate);
                Util.GridSetData(dgGate, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnDeleteForPrint6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> list = _Util.GetDataGridCheckRowIndex(dgGate, "CHK");
                if (list.Count <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                DataTable dtRQSTDT = new DataTable("RQSTDT");
                dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRQSTDT.Columns.Add("APPRV_PASS_NO", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                foreach (int row in list)
                {
                    DataRow ordRow = dtRQSTDT.NewRow();
                    ordRow["RCV_ISS_ID"] = DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "RCV_ISS_ID");
                    ordRow["APPRV_PASS_NO"] = null;
                    ordRow["USERID"] = LoginInfo.USERID;
                    dtRQSTDT.Rows.Add(ordRow);
                }

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_APPRV_PASS_NO_FOR_SHIP", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (bCancel == false)
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                OutHist();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnPrint6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> list = _Util.GetDataGridCheckRowIndex(dgGate, "CHK");
                if (list.Count <= 0)
                {
                    Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                Dictionary<string, string> valueList = new Dictionary<string, string>();
                string OUT_LIST = string.Empty;

                foreach (int row in list)
                {
                    string strValue = Util.NVC(DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "APPRV_PASS_NO"));
                    if (!valueList.ContainsKey(strValue)) valueList.Add(strValue, row.ToString());

                    OUT_LIST = OUT_LIST + Util.NVC(DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "RCV_ISS_ID"));
                    if (list.Last() != row) OUT_LIST = OUT_LIST + ",";

                    string sErp_err_code = Util.NVC(DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "ERP_ERR_CODE"));
                    if (sErp_err_code != "SUCCESS")
                    {
                        Util.MessageValidation("SFU3237", Util.NVC(DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "RCV_ISS_ID")));     // %1은 아직 전표번호가 생성되지 않았습니다. 잠시후에 처리하세요.
                        return;
                    }
                }

                if (valueList.Count() > 1)
                {
                    if (valueList.ContainsKey(string.Empty))
                    {
                        Util.MessageInfo("SFU2928");     //이미 발행된 항목이 포함되어있습니다.
                        return;
                    }
                    else
                    {
                        Util.MessageInfo("SFU2902");    //상이한 통문이력을 선택하였습니다.\r\n삭제후 진행해주세요.
                        return;
                    }
                }

                Print(valueList.ContainsKey(string.Empty) ? true : false, Util.NVC(valueList.Keys.First()), OUT_LIST);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print(bool bNew, string sAPPRV_PASS_NO, string sOUT_LIST)
        {
            try
            {
                Report_Gate_Tag rs = new Report_Gate_Tag();
                rs.FrameOperation = this.FrameOperation;

                _APPRV_PASS_NO = sAPPRV_PASS_NO;

                if (rs != null)
                {
                    DataSet dtData = new DataSet();
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = bNew;
                    Parameters[1] = sAPPRV_PASS_NO;
                    Parameters[2] = "Report_Tag";
                    Parameters[3] = "Report_MTRL";
                    Parameters[4] = sOUT_LIST;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(wndPrint_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    //grdMain.Children.Add(rs);
                    //rs.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void wndPrint_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Gate_Tag wndPopup = sender as LGC.GMES.MES.BOX001.Report_Gate_Tag;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    txtBoxid6.Text = null;
                    txtBoxid6.Focus();
                    OutHist();
                }
                else
                {
                    bCancel = true;
                    APPRV_PASS_NO_Init();
                    bCancel = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void APPRV_PASS_NO_Init()
        {
            try
            {
                if (_APPRV_PASS_NO == null ||  _APPRV_PASS_NO == "")
                {
                    List<int> list = _Util.GetDataGridCheckRowIndex(dgGate, "CHK");
                    if (list.Count <= 0)
                    {
                        Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                        return;
                    }

                    DataTable dtRQSTDT = new DataTable("RQSTDT");
                    dtRQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                    dtRQSTDT.Columns.Add("APPRV_PASS_NO", typeof(string));
                    dtRQSTDT.Columns.Add("USERID", typeof(string));

                    foreach (int row in list)
                    {
                        DataRow ordRow = dtRQSTDT.NewRow();
                        ordRow["RCV_ISS_ID"] = DataTableConverter.GetValue(dgGate.Rows[row].DataItem, "RCV_ISS_ID");
                        ordRow["APPRV_PASS_NO"] = null;
                        ordRow["USERID"] = LoginInfo.USERID;
                        dtRQSTDT.Rows.Add(ordRow);
                    }

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_APPRV_PASS_NO_FOR_SHIP", "RQSTDT", "RSLTDT", dtRQSTDT);

                    if (bCancel == false)
                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                }
                OutHist();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
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

        #region 출고이력조회
        private void txtBoxID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxID_Hist.Text != "")
                    {
                        Serach_Out_Hist();
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtSkid_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSkid_Hist.Text != "")
                    {
                        Serach_Out_Hist();
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtLotid_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotid_Hist.Text != "")
                    {
                        Serach_Out_Hist();
                    }
                    else
                    {
                        Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Box_Click(object sender, RoutedEventArgs e)
        {
            Serach_Out_Hist();
        }

        private void Serach_Out_Hist()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom_Box.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo_Box.SelectedDateTime);

                if (txtLotid_Hist.Text != "" || txtBoxID_Hist.Text != "" || txtSkid_Hist.Text != "")
                {
                    sStart_date = null;
                    sEnd_date = null;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXSTAT", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PJTNAME", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXSTAT"] = "PACKED";
                dr["LOTID"] = txtLotid_Hist.Text.Trim() == "" ? null : txtLotid_Hist.Text;
                dr["BOXID"] = txtBoxID_Hist.Text.Trim() == "" ? null : txtBoxID_Hist.Text;
                dr["CSTID"] = txtSkid_Hist.Text.Trim() == "" ? null : txtSkid_Hist.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc_Hist, bAllNull: true);
                // dr["PJTNAME"] = Util.GetCondition(dgOut_Hist);                
                dr["PJTNAME"] = Util.NVC(dr["PJTNAME"]).ToString();
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_OUT_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOut_Hist);
                Util.GridSetData(dgOut_Hist, SearchResult, FrameOperation);

                string[] sColumnName = new string[] { "OUTER_BOXID", "CSTID"  , "PJTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgOut_Hist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregatesCollection daq = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            DataGridAggregateCount dgcount = new DataGridAggregateCount();
            dagsum.ResultTemplate = dgOut_Hist.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            daq.Add(dgcount);
            DataGridAggregate.SetAggregateFunctions(dgOut_Hist.Columns["LOTID"], daq);
            DataGridAggregate.SetAggregateFunctions(dgOut_Hist.Columns["WIPQTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgOut_Hist.Columns["WIPQTY2"], dac);

        }

        #endregion

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
                {
                    Boxmapping_Master();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtProd_ID6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID6.Text != "")
                {
                    OutHist();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtProd_ID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID_Hist.Text != "")
                {
                    Serach_Out_Hist();
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private DataTable GetValidDate(string sBoxID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("BOXID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["BOXID"] = sBoxID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_VLD_DATE_FOR_BOX", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }
    }
}
