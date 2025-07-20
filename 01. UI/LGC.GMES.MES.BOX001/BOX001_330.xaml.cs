/*************************************************************************************
 Created Date : 2024.01.10
      Creator :  
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.10  김도형    : Initial Created. [E20231122-001031] [NA PI]전극MES시스템의 포장화면 개선건 
  2024.03.06  김도형    : [E20240220-000692] Skid화면 이력조회 개선
  2024.05.17  김도형    : [E20240408-000359] 电极包装card改善 전극포장card improvement
**************************************************************************************/

using C1.WPF.DataGrid;
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
using System.Windows.Media; 

using C1.WPF;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_330 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private bool IsMobileAssy_SkidCfg = false;
        Util _Util = new Util();
        public DataTable dtPackingCard_SkidCfg;
        int checkIndex_SkidCfg;

        // 전극포장
        private System.Windows.Threading.DispatcherTimer timer_Pack;
        private LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING_OQC_RP sampling_Pack;
        private DataTable OriginSamplingData_Pack;
        private bool IsSamplingCheck_Pack = false;

        public string sTempLot_1_Pack = string.Empty;
        public string sTempLot_2_Pack = string.Empty;
        public string sNote2_Pack = string.Empty;
        private string _APPRV_PASS_NO_Pac = string.Empty;

        public Boolean bReprint_Pack = true;

        private BOX001_330_PACKINGCARD window01_Pack = new BOX001_330_PACKINGCARD();
        private BOX001_330_PACKINGCARD_MERGE window02_Pack = new BOX001_330_PACKINGCARD_MERGE();

        public bool bNew_Load_Pack = false;
        public DataTable dtPackingCard_Pack;

        public bool bCancel_Pack = false;

        public BOX001_330()
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
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            ApplyPermissions();
            SetMobileAssy_SkidCfg();
            
            //전극포장
            #region Quality Check [자동차 1,2동만 적용] 
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                // SampleData 
                SetActSamplingData_Pack();

                timer_Pack.Tick -= timer_Start_Pack;
                timer_Pack.Tick += timer_Start_Pack;
                timer_Pack.Start();
            }
            #endregion
        }

        // 전극포장
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                timer_Pack.Stop();
                timer_Pack.Tick -= timer_Start_Pack;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave_SkidCfg);
            listAuth.Add(btnNewSkid_SkidCfg);
            listAuth.Add(btnSkidSearch_SkidCfg);
             
            listAuth.Add(btnLotidSearch_SkidCfg);
            listAuth.Add(btnAdd_SkidCfg);

            //전극포장
            listAuth.Add(btnPackOut_Pack);  // 전극포장 : 포장구성

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            // 전극 포장
            
            #region Combo Setting
            CommonCombo _combo_Pack = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "WH_DIVISION" };
            String[] sFilter2 = { "WH_SHIPMENT" };
            String[] sFilter3 = { "WH_TYPE" };
            String[] sFilter4 = { "WH_STATUS" };
            String[] sFilter5 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };

             
            _combo_Pack.SetCombo(cboTransLoc2_Pack, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");
            _combo_Pack.SetCombo(cboTransLoc_Hist_Pack, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "TRANSLOC");

            _combo_Pack.SetCombo(cboType_Pack, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE"); 

            // CNJ,CNA 라인 조회조건 추가 [C20180808_60878]
            _combo_Pack.SetCombo(cboEqsg_Hist_Pack, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT");


            #endregion

            // PANCAKE 고정요청 [2017-09-04]
            cboType_Pack.SelectedValue = "PANCAKE";

            #region Quality Check [자동차 1,2동만 적용] 
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                // Visible
                stQuality_Pack.Visibility = Visibility.Visible;

                // SampleData 
                //SetActSamplingData();

                // Timer
                timer_Pack = new System.Windows.Threading.DispatcherTimer();
                timer_Pack.Interval = TimeSpan.FromMinutes(1d); // 1분 간격으로 설정
                //timer.Tick -= timer_Start;
                //timer.Tick += timer_Start;
                //timer.Start();  
            }
            #endregion

            SetEvent_Pack();
             

            // SKID 구성
            SKID_ID_SkidCfg.Focus();
        }

        private void SetEvent_Pack()
        {
            //this.Loaded -= UserControl_Loaded; 

            dtpDateFrom_Box_Pack.SelectedDataTimeChanged += dtpDateFrom_Box_Pack_SelectedDataTimeChanged;
            dtpDateTo_Box_Pack.SelectedDataTimeChanged += dtpDateTo_Box_Pack_SelectedDataTimeChanged;
             
        }

        #endregion

        /*****************************************************************************************/
        /* Skid 구성관련  함수
       /*****************************************************************************************/
        #region Skid 구성관련  함수

        #region
        private void btnNewSkid_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSkidList_SkidCfg);
            Util.gridClear(dgLotList_SkidCfg);
            SKID_ID_SkidCfg.Text = "";
            SkidLotID_SkidCfg.Text = "";
        }

        private void btnSave_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int dgCheck = 0;
                int dgCheck2 = 0;

                if (dgSkidList_SkidCfg.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU3672"); //Skid를 구성할 Lot이 존재하지 않습니다.
                    return;
                }

                if (dgSkidList_SkidCfg.Rows.Count > 1)
                {
                    for (int i = 0; i < dgSkidList_SkidCfg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgSkidList_SkidCfg.Rows[0].DataItem, "AREAID").ToString() != DataTableConverter.GetValue(dgSkidList_SkidCfg.Rows[i].DataItem, "AREAID").ToString())
                        {
                            Util.MessageValidation("SFU4998", DataTableConverter.GetValue(dgSkidList_SkidCfg.Rows[0].DataItem, "LOTID").ToString(), DataTableConverter.GetValue(dgSkidList_SkidCfg.Rows[i].DataItem, "LOTID").ToString());
                            return;
                        }
                    }
                }


                // 2022-03-22 장기훈 C20210727-000344 : Skid 구성 저장 시 Line 체크 여부 --> 기준정보에 등록이 되어 있으면 같은 라인만 저장이 가능함

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "SKID_LINE_CHK_AREA";
                dr["LANGID"] = "ko-KR";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);
                
                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMONCODE_SKID_LINE_CHK_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                if (Result.Rows.Count > 0)
                {
                    DataTable dt1 = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;
                    DataTable dt2 = new DataTable();   // 선택된 Recodeset
                    dt2.Columns.Add("EQSGID", typeof(String));
                    dt2.Clear();

                    if (dgSkidList_SkidCfg.Rows.Count >= 1)
                    {                        
                        foreach (DataRow row in dt1.Rows)
                        {                            
                            if (Convert.ToBoolean(row["CHK"]))
                            {
                                dt2.Rows.Add(row["EQSGID"].ToString());                                                          
                            }                         
                        }
                    }

                    if (dt2.Rows.Count > 1)
                    {
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            if (dt2.Rows[0]["EQSGID"].ToString() != dt2.Rows[i]["EQSGID"].ToString())
                            {
                                Util.MessageValidation("SFU8275", LoginInfo.CFG_AREA_ID);
                                return;
                            }
                        }
                    }
                }
                //----------------------------------------->

                DataTable dt6 = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;

                if (dgSkidList_SkidCfg.Rows.Count >= 1)
                {
                    foreach (DataRow row in dt6.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]) && !Util.NVC(row["LOCATION"]).Equals(""))
                            dgCheck++;
                    }
                }


                var query = from rows in dt6.AsEnumerable()
                            group rows by rows.Field<string>("PRODID") into g
                            select new { prodCount = g.Count() };

                if (query.Count() > 1)
                {
                    Util.MessageValidation("SFU4205"); // 같은 제품만 SKID구성 할 수 있습니다.
                    return;
                }

                var query_mkt = from rows in dt6.AsEnumerable()
                                group rows by rows.Field<string>("MKT_TYPE_CODE") into g
                                select new { prodCount = g.Count() };

                if (query_mkt.Count() > 1)
                {
                    Util.MessageValidation("SFU4455"); // 내수용과 수출품은 동일한 SKID로 구성이 불가능합니다.
                    return;
                }



                if (dgLotList_SkidCfg.ItemsSource != null)
                {
                    DataTable dt7 = ((DataView)dgLotList_SkidCfg.ItemsSource).Table;
                    if (dgLotList_SkidCfg.Rows.Count >= 1)
                    {
                        foreach (DataRow row in dt7.Rows)
                        {
                            if (Convert.ToBoolean(row["CHK"]) && !Util.NVC(row["LOCATION"]).Equals(""))
                                dgCheck2++;
                        }
                    }
                }
                else
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    //SKID_ID_SkidCfg.Text = "";
                    return;
                }

                if (dgCheck + dgCheck2 == 0)
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    //SKID_ID_SkidCfg.Text = "";
                    return;
                }

                if (dgCheck >= 1)
                {
                    //Skid가 저장 됩니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU3219", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                            SetSkid_SkidCfg("A");
                    });
                }

                if (dgCheck2 >= 1)
                {
                    //추출한 데이터가 삭제됩니다. 계속 하시겠습니까?
                    Util.MessageConfirm("SFU3220", (sResult) =>
                    {
                        if (sResult == MessageBoxResult.OK)
                            SetSkid_SkidCfg("D");
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetSkid_SkidCfg(string PROC_TYPE)
        {
            try
            {
                if (PROC_TYPE.Equals("A"))
                {
                    DataSet inDataSet = new DataSet();
                    DataTable dt = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                    inDataTable.Columns.Add("SKID_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inDataRow["USERID"] = LoginInfo.USERID;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {
                                if (Util.NVC(row["LOCATION"]).Equals("LO"))
                                {
                                    if (row["SKID_ID"].Equals(""))
                                    {
                                        inDataRow["ISSUE_FLAG"] = "Y";
                                        inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                        break;
                                    }
                                    else
                                    {
                                        inDataRow["ISSUE_FLAG"] = "N";
                                        inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                    DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                    DataRow inSkidDataRow = null;
                    InSkidDataTable.Columns.Add("LOTID", typeof(string));
                    InSkidDataTable.Columns.Add("PRODID", typeof(string));
                    InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                    InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {

                                inSkidDataRow = InSkidDataTable.NewRow();
                                inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                                inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                                inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                                inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                                inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);

                            }
                        }
                    }
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID_SkidCfg.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                            Print_SkidCfg(SKID_ID_SkidCfg.Text.ToString());
                        }
                        else
                        {
                            //SKID_ID_SkidCfg.Text = "";
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.

                            return;
                        }


                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                        //SKID_ID_SkidCfg.Text = "";

                        return;
                    }
                }

                if (PROC_TYPE.Equals("D"))
                {
                    DataSet inDataSet = new DataSet();
                    DataTable dt = ((DataView)dgLotList_SkidCfg.ItemsSource).Table;

                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                    inDataTable.Columns.Add("SKID_ID", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inDataRow = null;

                    inDataRow = inDataTable.NewRow();
                    inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inDataRow["USERID"] = LoginInfo.USERID;

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {
                                if (Util.NVC(row["LOCATION"]).Equals("HI"))
                                {
                                    inDataRow["ISSUE_FLAG"] = "N";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                            }
                        }
                    }

                    inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                    DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                    DataRow inSkidDataRow = null;
                    InSkidDataTable.Columns.Add("LOTID", typeof(string));
                    InSkidDataTable.Columns.Add("PRODID", typeof(string));
                    InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                    InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["LOCATION"]).Equals(""))
                            {

                                inSkidDataRow = InSkidDataTable.NewRow();
                                inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                                inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                                inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                                inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                                inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);

                            }
                        }
                    }
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID_SkidCfg.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                            //btnSkidSearchs_SkidCfg();
                            Print_SkidCfg(SKID_ID_SkidCfg.Text.ToString());
                            //SKID_ID_SkidCfg.Text = "";

                        }
                        else
                        {
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                            //SKID_ID_SkidCfg.Text = "";

                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //SKID_ID_SkidCfg.Text = "";

                        return;
                    }


                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Print_SkidCfg(string sSkid_ID)
        {
            try
            {
                dtPackingCard_SkidCfg = new DataTable();

                dtPackingCard_SkidCfg.Columns.Add("Title", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("SKID_ID", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("PROJECT", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("PRODID", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("VER", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("QTY", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("UNIT", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("PRODDATE", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("VLDDATE", typeof(string));
                dtPackingCard_SkidCfg.Columns.Add("TOTAL_QTY", typeof(string));

                DataRow drCrad = null;

                drCrad = dtPackingCard_SkidCfg.NewRow();

                drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극SKID구성카드"),
                                                  sSkid_ID,
                                                  sSkid_ID,
                                                  ObjectDic.Instance.GetObjectName("PJT"),
                                                  ObjectDic.Instance.GetObjectName("반제품"),
                                                  ObjectDic.Instance.GetObjectName("버전"),
                                                  ObjectDic.Instance.GetObjectName("수량"),
                                                  ObjectDic.Instance.GetObjectName("단위"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  ObjectDic.Instance.GetObjectName("유효기간"),
                                                  ObjectDic.Instance.GetObjectName("총 수량")
                                               };

                dtPackingCard_SkidCfg.Rows.Add(drCrad);

                LGC.GMES.MES.COM001.Report_SKID rs = new LGC.GMES.MES.COM001.Report_SKID();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "Report_SKID";
                    Parameters[1] = sSkid_ID;
                    Parameters[2] = dtPackingCard_SkidCfg;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result_SkidCfg);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void Print_Result_SkidCfg(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.Report_SKID wndPopup = sender as LGC.GMES.MES.COM001.Report_SKID;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    btnSkidSearchs_SkidCfg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSkidOut_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            //아래로 이동
            if (dgSkidList_SkidCfg.Rows.Count <= 0)
                return;

            DataTable dt6 = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;

            DataTable dt7 = new DataTable();

            if (dgLotList_SkidCfg.Rows.Count >= 1)
            {
                dt7 = ((DataView)dgLotList_SkidCfg.ItemsSource).Table;
            }
            else
            {
                dt7.TableName = "dgLotList";
                dt7.Columns.Add("CHK", typeof(String));
                dt7.Columns.Add("LOTID", typeof(String));
                dt7.Columns.Add("PRODID", typeof(String));
                dt7.Columns.Add("PRODNAME", typeof(String));
                dt7.Columns.Add("MODLID", typeof(String));
                dt7.Columns.Add("AVL_DAYS", typeof(String));
                dt7.Columns.Add("PRE_VLD_DATE", typeof(String));
                dt7.Columns.Add("VLD_DATE", typeof(String));
                dt7.Columns.Add("AREAID", typeof(String));
                dt7.Columns.Add("EQSGID", typeof(String));
                dt7.Columns.Add("PROCID", typeof(String));
                dt7.Columns.Add("WIPQTY", typeof(String));
                dt7.Columns.Add("CUT_ID", typeof(String));
                dt7.Columns.Add("SKID_ID", typeof(String));
                dt7.Columns.Add("INPUTFLAG", typeof(String));
                dt7.Columns.Add("LOCATION", typeof(String));
            }
            foreach (DataRow row in dt6.Rows)
            {
                if (Convert.ToBoolean(row["CHK"]))
                {
                    if (!Util.NVC(row["LOTID"]).Equals(""))
                    {
                        DataRow dr = dt7.NewRow();
                        dr["CHK"] = true;
                        dr["LOTID"] = Util.NVC(row["LOTID"]);
                        dr["PRODID"] = Util.NVC(row["PRODID"]);
                        dr["PRODNAME"] = Util.NVC(row["PRODNAME"]);
                        dr["MODLID"] = Util.NVC(row["MODLID"]);
                        dr["AVL_DAYS"] = Util.NVC(row["AVL_DAYS"]);
                        dr["PRE_VLD_DATE"] = Util.NVC(row["PRE_VLD_DATE"]);
                        dr["VLD_DATE"] = Util.NVC(row["VLD_DATE"]);
                        dr["AREAID"] = Util.NVC(row["AREAID"]);
                        dr["EQSGID"] = Util.NVC(row["EQSGID"]);
                        dr["PROCID"] = Util.NVC(row["PROCID"]);
                        dr["WIPQTY"] = Util.NVC(row["WIPQTY"]);
                        dr["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                        dr["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                        if (Util.NVC(row["INPUTFLAG"]).Equals("A"))
                        {
                            dr["INPUTFLAG"] = "";
                        }
                        else
                        {
                            dr["INPUTFLAG"] = "D";
                        }
                        if (Util.NVC(row["LOCATION"]).Equals("LO"))
                        {
                            dr["LOCATION"] = "";
                        }
                        else
                        {
                            dr["LOCATION"] = "HI";
                        }
                        dt7.Rows.Add(dr);
                    }
                }
            }
            Util.GridSetData(dgLotList_SkidCfg, dt7, null, false);
            dgLotList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);

            for (int i = dgSkidList_SkidCfg.GetRowCount(); i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(dgSkidList_SkidCfg.ItemsSource);
                if (_Util.GetDataGridCheckValue(dgSkidList_SkidCfg, "CHK", i) == true)
                {
                    dt.Rows[i].Delete();
                }
                dgSkidList_SkidCfg.ItemsSource = DataTableConverter.Convert(dt);
            }
        }

        private void btnAdd_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            
                //위로 이동
                if (dgLotList_SkidCfg.Rows.Count <= 0)
                return;
            //추가2
            SKID_ID_SkidCfg.Clear();
            DataTable dt7 = ((DataView)dgLotList_SkidCfg.ItemsSource).Table;

            DataTable dt6 = new DataTable();

            if (dgSkidList_SkidCfg.Rows.Count >= 1)
            {
                dt6 = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;
            }
            else
            {
                dt6.TableName = "dgSkidList";
                dt6.Columns.Add("CHK", typeof(String));
                dt6.Columns.Add("SKID_ID", typeof(String));
                dt6.Columns.Add("LOTID", typeof(String));
                dt6.Columns.Add("PRODID", typeof(String));
                dt6.Columns.Add("PRODNAME", typeof(String));
                dt6.Columns.Add("MODLID", typeof(String));
                dt6.Columns.Add("AVL_DAYS", typeof(String));
                dt6.Columns.Add("PRE_VLD_DATE", typeof(String));
                dt6.Columns.Add("VLD_DATE", typeof(String));
                dt6.Columns.Add("AREAID", typeof(String));
                dt6.Columns.Add("EQSGID", typeof(String));
                dt6.Columns.Add("PROCID", typeof(String));
                dt6.Columns.Add("WIPQTY", typeof(String));
                dt6.Columns.Add("CUT_ID", typeof(String));
                dt6.Columns.Add("INPUTFLAG", typeof(String));
                dt6.Columns.Add("LOCATION", typeof(String));
                dt6.Columns.Add("MKT_TYPE_CODE", typeof(String));
                dt6.Columns.Add("MKT_TYPE_NAME", typeof(String));
            }
            foreach (DataRow row2 in dt7.Rows)
            {
                if (Convert.ToBoolean(row2["CHK"]))
                {
                    if (!Util.NVC(row2["LOTID"]).Equals(""))
                    {
                        if (!dt7.Columns.Contains("MKT_TYPE_CODE"))
                        {
                            Util.MessageValidation("SFU9210");  // 시장유형 이 존재하지 않습니다.(취출 내역일 경우)
                            return;
                        }
                        DataRow dr = dt6.NewRow();
                        dr["CHK"] = true;
                        dr["SKID_ID"] = SKID_ID_SkidCfg.Text;
                        dr["LOTID"] = Util.NVC(row2["LOTID"]);
                        dr["PRODID"] = Util.NVC(row2["PRODID"]);
                        dr["PRODNAME"] = Util.NVC(row2["PRODNAME"]);
                        dr["MODLID"] = Util.NVC(row2["MODLID"]);
                        dr["AVL_DAYS"] = Util.NVC(row2["AVL_DAYS"]);
                        dr["PRE_VLD_DATE"] = Util.NVC(row2["PRE_VLD_DATE"]);
                        dr["VLD_DATE"] = Util.NVC(row2["VLD_DATE"]);
                        dr["AREAID"] = Util.NVC(row2["AREAID"]);
                        dr["EQSGID"] = Util.NVC(row2["EQSGID"]);
                        dr["PROCID"] = Util.NVC(row2["PROCID"]);
                        dr["WIPQTY"] = Util.NVC(row2["WIPQTY"]);
                        dr["CUT_ID"] = Util.NVC(row2["CUT_ID"]);
                        dr["MKT_TYPE_CODE"] = Util.NVC(row2["MKT_TYPE_CODE"]);
                        dr["MKT_TYPE_NAME"] = Util.NVC(row2["MKT_TYPE_NAME"]);
                        if (Util.NVC(row2["INPUTFLAG"]).Equals("D"))
                        {
                            dr["INPUTFLAG"] = "";
                        }
                        else
                        {
                            dr["INPUTFLAG"] = "A";
                        }
                        if (Util.NVC(row2["LOCATION"]).Equals("HI"))
                        {
                            dr["LOCATION"] = "";
                        }
                        else
                        {
                            dr["LOCATION"] = "LO";
                        }

                        dt6.Rows.Add(dr);
                    }
                }
            }

            Util.GridSetData(dgSkidList_SkidCfg, dt6, null, false);
            dgSkidList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);

            for (int i = dgLotList_SkidCfg.GetRowCount(); i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(dgLotList_SkidCfg.ItemsSource);
                if (_Util.GetDataGridCheckValue(dgLotList_SkidCfg, "CHK", i) == true)
                {
                    dt.Rows[i].Delete();
                }
                dgLotList_SkidCfg.ItemsSource = DataTableConverter.Convert(dt);
            }
            SkidLotID_SkidCfg.Text = "";
        }

        private void SKID_ID_SkidCfg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSkidSearch_SkidCfg_Click(sender, e);
            }
        }

        private void SkidLotID_SkidCfg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLotidSearch_SkidCfg_Click(sender, e);
            }
        }

        private void btnLotidSearch_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            btnLotSearchs_SkidCfg();           
        }

        private void GridSet_Checked_SkidCfg()
        {
            if (dgLotList_SkidCfg.Rows.Count > 0)
            {
                for (int i = 0; i < dgLotList_SkidCfg.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgLotList_SkidCfg.Rows[i].DataItem, "CHK", true);
                    dgLotList_SkidCfg.ScrollIntoView(i, dgLotList_SkidCfg.Columns["CHK"].Index);
                }
            }
        }

        private void btnSkidSearch_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            btnSkidSearchs_SkidCfg();  
        }

        private bool NJ_AreaChk_SkidCfg()
        {
            bool area_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACKING_VERSION_CHECK_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0 && LoginInfo.CFG_AREA_ID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    area_chk =  true;
                }
                else
                {
                    area_chk =  false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return area_chk;
        }

        private bool lotVerChk_SkidCfg()
        {
            bool ver_chk = true;

            return ver_chk;
        }

        private void btnSkidSearchs_SkidCfg()
        {
            try
            {
                //if (SKID_ID.Text.ToString() == "")
                //{
                //    Util.MessageValidation("SFU2934");  //입력한 Skid ID 가 없습니다.
                //    return;
                //}

                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("SKID_ID", typeof(String));
                RQSTDT.Columns.Add("DateFrom", typeof(String));
                RQSTDT.Columns.Add("DateTo", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                if (SKID_ID_SkidCfg.Text.ToString().Trim().Equals(""))
                {

                }
                else
                {
                    dr["SKID_ID"] = SKID_ID_SkidCfg.Text.ToString().Trim();
                }
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["DateFrom"] = Util.GetCondition(dtpDateFrom);
                //dr["DateTo"] = Util.GetCondition(dtpDateTo);

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                    //SKID_ID_SkidCfg.Text = "";
                    return;
                }
                else
                {
                    for (int i = 0; i < Result.Rows.Count; i++)
                    {
                        if (!string.Equals(Result.Rows[0]["BOXID"].ToString(), ""))
                        {
                            Util.MessageValidation("SFU2978"); //포장완료한 이력이 있습니다.
                            return;
                        }
                    }
                    Util.gridClear(dgSkidList_SkidCfg);
                    Util.GridSetData(dgSkidList_SkidCfg, Result, null, false);
                    dgSkidList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    //SKID_ID_SkidCfg.Text = "";                
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //SKID_ID_SkidCfg.Text = "";
                return;
            }
        }

        private void btnLotSearchs_SkidCfg()
        {
            try
            {
                if (SkidLotID_SkidCfg.Text.ToString() == "")
                {
                    Util.MessageValidation("SFU1366");  //LOT ID를 입력해주세요
                    return;
                }

                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = SkidLotID_SkidCfg.Text.ToString().Trim();

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1195");  //Lot 정보가 없습니다.
                    SkidLotID_SkidCfg.Text = "";
                    return;
                }
                else
                {
                    for (int i = 0; i < Result.Rows.Count; i++)
                    {
                        if (!string.Equals(Result.Rows[0]["BOXID"].ToString(), ""))
                        {
                            Util.MessageValidation("SFU2978"); //포장완료한 이력이 있습니다.
                            return;
                        }
                    }

                    if (IsMobileAssy_SkidCfg == true)
                    {
                        string sReturnLine = GetReturnLine_SkidCfg(SkidLotID_SkidCfg.Text.ToString().Trim());

                        Result.Columns.Add("RTLOCATION");
                        Result.Rows[0]["RTLOCATION"] = sReturnLine;
                    }

                    if (dgLotList_SkidCfg.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgLotList_SkidCfg, Result, FrameOperation);
                    }
                    else
                    {
                        for (int i = 0; i < dgLotList_SkidCfg.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgLotList_SkidCfg.Rows[i].DataItem, "LOTID").ToString() == SkidLotID_SkidCfg.Text.ToString())
                            {
                                Util.Alert("SFU1914");   //중복 스캔되었습니다.
                                return;
                            }

                            if (Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgLotList_SkidCfg.Rows[i].DataItem, "MKT_TYPE_CODE").ToString())
                            {
                                Util.Alert("SFU4455");   //내수용과 수출품은 동일한 SKID로 구성이 불가능합니다.
                                return;
                            }

                            if (Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgLotList_SkidCfg.Rows[i].DataItem, "PRODID").ToString())
                            {
                                Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                                return;
                            }

                            if (IsMobileAssy_SkidCfg == true)
                            {
                                if (!string.Equals(Result.Rows[0]["RTLOCATION"], DataTableConverter.GetValue(dgLotList_SkidCfg.Rows[i].DataItem, "RTLOCATION")))
                                {
                                    Util.Alert("SFU4039");   //반품동이 동일한 LOT이 아닙니다.
                                    return;
                                }
                            }

                            if (NJ_AreaChk_SkidCfg())
                            {
                                if (DataTableConverter.GetValue(dgLotList_SkidCfg.Rows[i].DataItem, "PROD_VER_CODE").ToString() != Result.Rows[0]["PROD_VER_CODE"].ToString())
                                {
                                    Util.Alert("SFU1501");   //동일 버전이 아닙니다.
                                    return;
                                }
                            }
                        }

                        dgLotList_SkidCfg.IsReadOnly = false;
                        dgLotList_SkidCfg.BeginNewRow();
                        dgLotList_SkidCfg.EndNewRow(true);
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "CHK", Result.Rows[0]["CHK"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "LOTID", Result.Rows[0]["LOTID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PRODID", Result.Rows[0]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PRODNAME", Result.Rows[0]["PRODNAME"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "MODLID", Result.Rows[0]["MODLID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "AVL_DAYS", Result.Rows[0]["AVL_DAYS"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PRE_VLD_DATE", Result.Rows[0]["PRE_VLD_DATE"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "VLD_DATE", Result.Rows[0]["VLD_DATE"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "AREAID", Result.Rows[0]["AREAID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PRODID", Result.Rows[0]["PRODID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PRODNAME", Result.Rows[0]["PRODNAME"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "MODLID", Result.Rows[0]["MODLID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "EQSGID", Result.Rows[0]["EQSGID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PROCID", Result.Rows[0]["PROCID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "WIPQTY", Result.Rows[0]["WIPQTY"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "CUT_ID", Result.Rows[0]["CUT_ID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "SKID_ID", Result.Rows[0]["SKID_ID"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "INPUTFLAG", Result.Rows[0]["INPUTFLAG"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "LOCATION", Result.Rows[0]["LOCATION"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "MKT_TYPE_CODE", Result.Rows[0]["MKT_TYPE_CODE"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "MKT_TYPE_NAME", Result.Rows[0]["MKT_TYPE_NAME"].ToString());
                        DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "PROD_VER_CODE", Result.Rows[0]["PROD_VER_CODE"].ToString());

                        if (IsMobileAssy_SkidCfg == true)
                            DataTableConverter.SetValue(dgLotList_SkidCfg.CurrentRow.DataItem, "RTLOCATION", Result.Rows[0]["RTLOCATION"].ToString());

                        dgLotList_SkidCfg.IsReadOnly = true;
                    }
                    GridSet_Checked_SkidCfg();
                    SkidLotID_SkidCfg.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion
        // 체크박스 관련
        #region
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre_SkidCfg = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll_SkidCfg = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        private void dgSkidList_SkidCfg_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgSkidList_SkidCfg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                } 
            }
        }

        private void dgLotList_SkidCfg_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgLotList_SkidCfg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgSkidList_SkidCfg_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgSkidList_SkidCfg.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void dgLotList_SkidCfg_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgLotList_SkidCfg.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }
        #endregion

        private void btnReprint_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            if (SKID_ID_SkidCfg.Text.ToString() == "")
            {
                Util.MessageValidation("SFU2934");  //입력한 Skid ID 가 없습니다.
                return;
            }

            if (sKidValidation_SkidCfg(SKID_ID_SkidCfg.Text.ToString().Trim()))
            {
                Print_SkidCfg(SKID_ID_SkidCfg.Text.ToString());
            }

        }

        private bool sKidValidation_SkidCfg(string Skid_id)
        {
            // SKID Grid Data 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SKID_ID", typeof(String));

            DataRow dr = RQSTDT.NewRow();

            dr["SKID_ID"] = Skid_id;

            RQSTDT.Rows.Add(dr);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

            if (Result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                return false;
            }
            return true;
        }

        private void btnCutSkid_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            if (dgSkidList_SkidCfg.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2934");  //입력한 Skid ID 가 없습니다.
                return;
            }

            if (SKID_ID_SkidCfg.Text.ToString() == "")
            {
                int chk = 0;
                DataSet inDataSet = new DataSet();
                DataTable dt = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("ISSUE_FLAG", typeof(string));
                inDataTable.Columns.Add("SKID_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["USERID"] = LoginInfo.USERID;

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOCATION"]).Equals(""))
                        {
                            if (Util.NVC(row["LOCATION"]).Equals("LO"))
                            {
                                chk++;
                                if (row["SKID_ID"].Equals(""))
                                {
                                    inDataRow["ISSUE_FLAG"] = "Y";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                                else
                                {
                                    inDataRow["ISSUE_FLAG"] = "N";
                                    inDataRow["SKID_ID"] = Util.NVC(row["SKID_ID"]);
                                    break;
                                }
                            }
                        }
                    }
                }

                inDataSet.Tables["INDATA"].Rows.Add(inDataRow);
                DataTable InSkidDataTable = inDataSet.Tables.Add("IN_LOT");
                DataRow inSkidDataRow = null;
                InSkidDataTable.Columns.Add("LOTID", typeof(string));
                InSkidDataTable.Columns.Add("PRODID", typeof(string));
                InSkidDataTable.Columns.Add("CUT_ID", typeof(string));
                InSkidDataTable.Columns.Add("INPUT_TYPE", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        if (!Util.NVC(row["LOCATION"]).Equals(""))
                        {

                            inSkidDataRow = InSkidDataTable.NewRow();
                            inSkidDataRow["LOTID"] = Util.NVC(row["LOTID"]);
                            inSkidDataRow["PRODID"] = Util.NVC(row["PRODID"]);
                            inSkidDataRow["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                            inSkidDataRow["INPUT_TYPE"] = Util.NVC(row["INPUTFLAG"]);

                            inDataSet.Tables["IN_LOT"].Rows.Add(inSkidDataRow);
                        }
                    }
                }

                if (chk != 0)
                {
                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet);

                        if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            SKID_ID_SkidCfg.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                        }
                        else
                        {
                            Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                            return;
                        }

                        // SKID Grid Data 조회
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("SKID_ID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["SKID_ID"] = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();

                        RQSTDT.Rows.Add(dr);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_LOTID", "RQSTDT", "RSLTDT", RQSTDT);

                        if (Result.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            Util.gridClear(dgSkidList_SkidCfg);
                            Util.GridSetData(dgSkidList_SkidCfg, Result, null, false);
                            dgSkidList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                            checkAll_SkidCfg_Checked(null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                }
            }

            // Skid ID 이미 재발행된 상태에서 Skid 분리를 사용하면 0 + Alphabet으로 표현되어 Naming Rule에서 SEQ를 가져올 수 없어 오류가 발생함
            // 그래서 신규 Skid ID가 발행된 상태에서 Skid 분리 버튼 사용을 금지시킴 [2018-08-21]
            if( !string.IsNullOrEmpty(SKID_ID_SkidCfg.Text.ToString()) && SKID_ID_SkidCfg.Text.ToString().Length == 9)
            {
                if (string.Equals(SKID_ID_SkidCfg.Text.ToString().Substring(7, 1), "0" ))
                {
                    Util.MessageValidation("SFU5007");  //이미 재발행된 Skid에서는 Skid분리를 사용할 수 없습니다.(분리하실 Pancake를 추가하여 저장하시기 바랍니다.)
                    return;
                }
            }

            Util.MessageConfirm("SFU3582", (sResult) => // Skid ID를 분리 하시겠습니까?
            {
                if (sResult == MessageBoxResult.OK)
                    CutSkid_SkidCfg(SKID_ID_SkidCfg.Text.ToString());
            });
        }

        private void CutSkid_SkidCfg(string sSkidID)
        {

            if (SKID_ID_SkidCfg.Text.ToString() == "")
            {
                Util.MessageValidation("SFU2934");  //입력한 Skid ID 가 없습니다.
                return;
            }

            try
            {
                string sSkidID2 = sSkidID.Substring(0, 8).ToString() + Convert.ToChar((Convert.ToInt64(Util.NVC(sSkidID.Substring(8, 1)), 16)) + 65).ToString();
                int dtchk = 0;
                DataSet inDataSet2 = new DataSet();
                DataTable dt2 = ((DataView)dgSkidList_SkidCfg.ItemsSource).Table;

                DataTable inDataTable2 = inDataSet2.Tables.Add("INDATA");

                inDataTable2.Columns.Add("SRCTYPE", typeof(string));
                inDataTable2.Columns.Add("AREAID", typeof(string));
                inDataTable2.Columns.Add("ISSUE_FLAG", typeof(string));
                inDataTable2.Columns.Add("SKID_ID", typeof(string));
                inDataTable2.Columns.Add("USERID", typeof(string));

                DataRow inDataRow2 = null;

                inDataRow2 = inDataTable2.NewRow();
                inDataRow2["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow2["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow2["USERID"] = LoginInfo.USERID;
                inDataRow2["ISSUE_FLAG"] = "N";
                inDataRow2["SKID_ID"] = sSkidID2;

                inDataSet2.Tables["INDATA"].Rows.Add(inDataRow2);
                DataTable InSkidDataTable2 = inDataSet2.Tables.Add("IN_LOT");
                DataRow inSkidDataRow2 = null;
                InSkidDataTable2.Columns.Add("LOTID", typeof(string));
                InSkidDataTable2.Columns.Add("PRODID", typeof(string));
                InSkidDataTable2.Columns.Add("CUT_ID", typeof(string));
                InSkidDataTable2.Columns.Add("INPUT_TYPE", typeof(string));

                foreach (DataRow row in dt2.Rows)
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        long i = 0;
                        char[] code = Util.NVC(row["LOTID"]).Substring(9, 1).ToCharArray(0, 1);
                        if (code[0] > 64)
                        {
                            i = Convert.ToInt64(Util.NVC(code[0] - 48)) - 1;
                        }
                        else
                        {
                            i = Convert.ToInt64(Util.NVC(code[0] - 48));
                        }
                        if ((Convert.ToInt64(i % 2) == 0))
                        {
                            inSkidDataRow2 = InSkidDataTable2.NewRow();
                            inSkidDataRow2["LOTID"] = Util.NVC(row["LOTID"]);
                            inSkidDataRow2["PRODID"] = Util.NVC(row["PRODID"]);
                            inSkidDataRow2["CUT_ID"] = Util.NVC(row["CUT_ID"]);
                            inSkidDataRow2["INPUT_TYPE"] = "A";

                            inDataSet2.Tables["IN_LOT"].Rows.Add(inSkidDataRow2);
                            dtchk++;
                        }
                    }
                }
                if (dtchk == 0)
                {
                    Util.MessageValidation("SFU2906"); //선택된 정보나 변경된 정보가 없습니다.
                    return;
                }
                try
                {
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_SKID", "INDATA,IN_LOT", "OUTDATA", inDataSet2);

                    if (dsRslt != null && dsRslt.Tables.Contains("OUTDATA") && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        //SKID_ID.Text = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SKID_ID"]).ToString();
                        checkAll_SkidCfg_Unchecked(null, null);
                        Print_SkidCfg(sSkidID);
                        Print_SkidCfg(sSkidID2);                      
                    }
                    else
                    {
                        Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                        return;
                    }
                }
                catch (Exception ex)
                {

                    Util.MessageException(ex);
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSkidList_SkidCfg_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre_SkidCfg.Content = chkAll_SkidCfg;
                        //pre.Content = new HorizontalContentAlignment = "Center";
                        e.Column.HeaderPresenter.Content = pre_SkidCfg;

                        chkAll_SkidCfg.Checked -= new RoutedEventHandler(checkAll_SkidCfg_Checked);
                        chkAll_SkidCfg.Unchecked -= new RoutedEventHandler(checkAll_SkidCfg_Unchecked);
                        chkAll_SkidCfg.Checked += new RoutedEventHandler(checkAll_SkidCfg_Checked);
                        chkAll_SkidCfg.Unchecked += new RoutedEventHandler(checkAll_SkidCfg_Unchecked);
                    }
                }
            }));
        }

        void checkAll_SkidCfg_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSkidList_SkidCfg.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgSkidList_SkidCfg.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_SkidCfg_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgSkidList_SkidCfg.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgSkidList_SkidCfg.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex_SkidCfg = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }


        private void btnCSearch_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "31");
                //    return;
                //}

                // SKID Grid Data 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("DATEFROM", typeof(String));
                RQSTDT.Columns.Add("DATETO", typeof(String));
                RQSTDT.Columns.Add("EIF_ACTUSER_EXCEPT", typeof(String));  // [E20240220-000692] Skid화면 이력조회 개선
            
                DataRow dr = RQSTDT.NewRow();
                if (SKID_ID2_SkidCfg.Text.ToString().Trim().Equals(""))
                {
                    dr["DATEFROM"] = Util.GetCondition(dtpDateFrom_SkidCfg);
                    dr["DATETO"] = Util.GetCondition(dtpDateTo_SkidCfg);
                }
                else
                {
                    dr["CSTID"] = SKID_ID2_SkidCfg.Text.ToString().Trim();
                }
                                
                dr["EIF_ACTUSER_EXCEPT"] = (chkEifActuserExcept_SkidCfg.IsChecked == true) ? "Y" : "N";  // [E20240220-000692] Skid화면 이력조회 개선

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_CONF_HIST_V02", "RQSTDT", "RSLTDT", RQSTDT); // [E20240220-000692] Skid화면 이력조회 개선

                if (Result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                    return;
                }
                else
                {
                    Util.gridClear(dgHistList_SkidCfg);
                    Util.gridClear(dgHistList2_SkidCfg);
                    Util.GridSetData(dgHistList_SkidCfg, Result, null, false);
                    dgHistList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //SKID_ID.Text = "";
                return;
            }
        }

        private void btnPackCofing_SkidCfg_Click(object sender, RoutedEventArgs e)
        {
            // SFU6014 포장구성 하시겠습니까?

            int dtchk = 0;
            string sSKIDID = "";

            if (dgHistList_SkidCfg.GetRowCount() == 0)
            {
                Util.MessageValidation("SFU2844"); //SKID 정보가 없습니다.
                return;
            }

            DataTable dtHistList = ((DataView)dgHistList_SkidCfg.ItemsSource).Table;
            foreach (DataRow row in dtHistList.Rows)
            {
                if (Util.NVC(row["CHK"]).Equals("True"))
                {
                    sSKIDID = Util.NVC(row["CSTID"]).ToString();
                    dtchk++;
                }
            }
             
            if (dtchk == 0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return;
            }

            if (!sSKIDID.Equals(""))
            {

                this.Tab_Packing.IsSelected = true;
                if (btnPackOut_Pack.IsEnabled)
                {
                    txtLotID_Pack.Text = sSKIDID;
                    Search_SkidID_LotID_Pack();
                }
                else
                {
                    Util.MessageValidation("SFU9211"); // 이전 포장 작업이 완료 되지 않았습니다.초기화 후 진행하시기 바랍니다.
                    return;
                }
            }

            return;
        }

        private void CheckBox_SkidCfg_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    return;
                }
                if (dgHistList_SkidCfg.GetRowCount() < 0)
                {
                    return;
                }

                checkIndex_SkidCfg = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;
                DataTableConverter.SetValue(dgHistList_SkidCfg.Rows[checkIndex_SkidCfg].DataItem, "CHK", true);
                dgHistList_SkidCfg.SelectedIndex = checkIndex_SkidCfg;

                for (int i = 0; i < dgHistList_SkidCfg.Rows.Count; i++)
                {
                    if (i != checkIndex_SkidCfg)
                    {
                        DataTableConverter.SetValue(dgHistList_SkidCfg.Rows[i].DataItem, "CHK", false);
                    }
                    else
                    {
                        // SKID 상세 Data 조회
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("CSTID", typeof(String));
                        RQSTDT.Columns.Add("ACTDTTM", typeof(DateTime));

                        DataRow dr = RQSTDT.NewRow();
                        dr["CSTID"] = DataTableConverter.GetValue(dgHistList_SkidCfg.Rows[checkIndex_SkidCfg].DataItem, "CSTID").ToString().Trim();
                        dr["ACTDTTM"] = DataTableConverter.GetValue(dgHistList_SkidCfg.Rows[checkIndex_SkidCfg].DataItem, "ACTDTTM").ToString().Trim();

                        RQSTDT.Rows.Add(dr);

                        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_CONF_HIST_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                        if (Result.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2844");   //SKID 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            Util.gridClear(dgHistList2_SkidCfg);
                            Util.GridSetData(dgHistList2_SkidCfg, Result, null, false);
                            dgHistList2_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                        }

                    }

                }
                dgHistList_SkidCfg.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SKID_ID2_SkidCfg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnCSearch_SkidCfg_Click(sender, e);
            }
        }

        private void SetMobileAssy_SkidCfg()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ASSY_RETURN_USELINE"; 
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    if (string.Equals(row["CBO_CODE"], LoginInfo.CFG_AREA_ID))
                    {
                        IsMobileAssy_SkidCfg = true;
                        break;
                    }
                }
            }
            catch (Exception ex) { }
        }

        private string GetReturnLine_SkidCfg(string sLotID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LOTID"] = sLotID;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_AREA", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["AREAID"]);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return "";
        }
        #endregion Skid구성 End
        /*********************************************** Skid구성 End ****************************/


        /*****************************************************************************************/
        /* 전극포장관련 함수
        /*****************************************************************************************/
        #region 전극포장관련 함수

        #region 자동차동 SAMPLING 전용 FUNCTION
        private void btnQuality_Pack_Click(object sender, RoutedEventArgs e)
        {
            sampling_Pack = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
            sampling_Pack.FrameOperation = FrameOperation;

            if (sampling_Pack != null)
            {
                C1WindowExtension.SetParameters(sampling_Pack, null);

                sampling_Pack.Closed -= new EventHandler(OnCloseSampling_Pack);
                sampling_Pack.Closed += new EventHandler(OnCloseSampling_Pack);
                this.Dispatcher.BeginInvoke(new Action(() => sampling_Pack.ShowModal()));
            }
        }

        private void timer_Start_Pack(object sender, EventArgs e)
        {
            if (sampling_Pack == null && IsSamplingCheck_Pack == false)
            {
                //LinearGradientBrush btnGradient = btnQuality.Background as LinearGradientBrush;

                //System.Windows.Media.Animation.ColorAnimation animation = new System.Windows.Media.Animation.ColorAnimation();
                //animation.From = System.Windows.Media.Colors.Blue;
                //animation.To = System.Windows.Media.Colors.Orange;
                //animation.Duration = TimeSpan.FromSeconds(1);
                //animation.AutoReverse = true;
                //animation.RepeatBehavior = RepeatBehavior.Forever;

                //Storyboard.SetTarget(animation, btnGradient);
                //Storyboard.SetTargetProperty(animation, new PropertyPath(SolidColorBrush.ColorProperty));

                //Storyboard sb = new Storyboard();
                //sb.Children.Add(animation);
                //sb.Begin();

                SetActSamplingData_Pack();
            }
        }

        private void OnCloseSampling_Pack(object sender, EventArgs e)
        {
            CMM001.CMM_ELEC_SAMPLING_OQC_RP window = sender as CMM001.CMM_ELEC_SAMPLING_OQC_RP;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (IsSamplingCheck_Pack)
                {
                    IsSamplingCheck_Pack = false;
                    Storyboard board = (Storyboard)this.Resources["storyBoard"];
                    if (board != null)
                        board.Stop();
                }
            }
            sampling_Pack.Close();
            sampling_Pack = null;
            GC.Collect();
        }


        private void SetActSamplingData_Pack()
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.ROLL_PRESSING;
                IndataTable.Rows.Add(Indata);

                //DA_PRD_SEL_LOT_SAMPLE_CNA_QA - > DA_PRD_SEL_LOT_SAMPLE_QA 변경
                //사용 UI화면과 DA가 다름 동일하게 처리하기 위하여 변경 작업
                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_SAMPLE_QA", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                            throw searchException;

                        if (OriginSamplingData_Pack == null)
                            OriginSamplingData_Pack = result;
                        else
                            IsDiffSamplingData(OriginSamplingData_Pack, result);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { }
        }

        private void IsDiffSamplingData(DataTable oldData, DataTable newData)
        {
            bool IsChangeSampling = false;
            foreach (DataRow oldRow in oldData.Rows)
            {
                foreach (DataRow newRow in newData.Rows)
                {
                    if (string.Equals(oldRow["LOTID"], newRow["LOTID"]))
                    {
                        // 변경된 데이터 검증
                        if (!string.Equals(oldRow["JUDG_FLAG"], newRow["JUDG_FLAG"]))
                            IsChangeSampling = true;

                        // 미검사 OR 불합격 판정 -> 합격 변경 시
                        if ((string.IsNullOrEmpty(Util.NVC(oldRow["JUDG_FLAG"])) || string.Equals(oldRow["JUDG_FLAG"], "F")) && string.Equals(newRow["JUDG_FLAG"], "Y") && IsSamplingCheck_Pack == false)
                        {
                            IsSamplingCheck_Pack = true;
                            Storyboard board = (Storyboard)this.Resources["storyBoard"];
                            if (board != null)
                                board.Begin();

                            // 팝업 자동 생성
                            if (sampling_Pack == null && chkQuality_Pack.IsChecked == true)
                            {
                                sampling_Pack = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
                                sampling_Pack.FrameOperation = FrameOperation;

                                if (sampling_Pack != null)
                                {
                                    C1WindowExtension.SetParameters(sampling_Pack, null);

                                    sampling_Pack.Closed -= new EventHandler(OnCloseSampling_Pack);
                                    sampling_Pack.Closed += new EventHandler(OnCloseSampling_Pack);
                                    this.Dispatcher.BeginInvoke(new Action(() => sampling_Pack.ShowModal()));
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // 갱신이 존재하면 신규 데이터로 변경 요청
            if (IsChangeSampling == false || (oldData.Rows.Count != newData.Rows.Count))
                IsChangeSampling = true;

            if (IsChangeSampling == true)
            {
                OriginSamplingData_Pack.Clear();
                OriginSamplingData_Pack = newData;
            }
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre_Pack = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();

        CheckBox chkAll_Pack = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };


        private Dictionary<string, string> getShipCompany_Pack(string sProdID)
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
                ShipToIndata["SHIPTO_ID"] = cboTransLoc2_Pack.SelectedValue.ToString();

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

        #endregion  자동차동 SAMPLING 전용 FUNCTION
        

        private void dtpDateFrom_Box_Pack_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom_Box_Pack.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo_Box_Pack.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_Box_Pack_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo_Box_Pack.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom_Box_Pack.SelectedDateTime;
                return;
            }
        }

        private void cboType_Pack_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLot_Type = cboType_Pack.SelectedValue.ToString();

            if (sLot_Type == "JUMBO_ROLL")
            { 
                btnPackOut_Pack.Content = ObjectDic.Instance.GetObjectName("출고");

                dgOut_Pack.Columns["M_WIPQTY"].Header = "S/ROLL";
                dgOut_Pack.Columns["CELL_WIPQTY"].Header = "N/ROLL";
            }
            else
            {
                btnPackOut_Pack.Content = ObjectDic.Instance.GetObjectName("포장구성");

                dgOut_Pack.Columns["M_WIPQTY"].Header = "C/ROLL";
                dgOut_Pack.Columns["CELL_WIPQTY"].Header = "S/ROLL";
            }
        }

        private void txtLotID_Pack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search_SkidID_LotID_Pack();
            }
        }

        private void Search_SkidID_LotID_Pack()
        { 

            try
            {
                string sLotid = string.Empty;
                string sLot_Type = string.Empty;

                if (txtLotID_Pack.Text.ToString() == "")
                {
                    Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                    return;
                }

                if (cboType_Pack.SelectedIndex < 0 || cboType_Pack.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.Alert("SFU1895");   //제품을 선택하세요
                    return;
                }
                else
                {
                    sLot_Type = cboType_Pack.SelectedValue.ToString();
                }


                if (dgOut_Pack.GetRowCount() >= 2)
                {
                    Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                    return;
                }

                if (sLot_Type == "PANCAKE")
                {
                    sLotid = txtLotID_Pack.Text.ToString().Trim();

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
                    if (OutResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                        return;
                    }
                    else
                    {
                        int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                        if (iCnt <= 0)
                        {
                            Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                            return;
                        }
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

                    DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT1);

                    if (Prod_Result.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");   //재공 정보가 없습니다.
                        return;
                    }

                    for (int i = 0; i < Prod_Result.Rows.Count; i++)
                    {
                        if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                        {
                            Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                            return;
                        }
                    }
                    #region # 시생산 Lot (CNJ,CNA,CNB 제외)
                    if (!_Util.IsCommonCodeUse("PILOT_EXCEPT_ISSUE_PLANT", LoginInfo.CFG_SHOP_ID))
                    {
                        DataRow[] dRow = Prod_Result.Select("LOTTYPE = 'X'");
                        if (dRow.Length > 0)
                        {
                            Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다
                            return;
                        }
                    }
                    #endregion
                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dt.Columns.Add("COM_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                    dr["COM_CODE"] = "QMS_NOCHECK_PACKING";

                    dt.Rows.Add(dr);

                    //QMS 체크 제외 AREA
                    DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                    if (AreaResult.Rows.Count == 0)
                    {
                        // [E20240408-000359] 电极包装card改善 전극포장card improvement
                        if (cboTransLoc2_Pack.SelectedIndex < 0 || string.IsNullOrEmpty(cboTransLoc2_Pack.SelectedValue.ToString()) || cboTransLoc2_Pack.Text.ToString().Equals("-SELECT-"))
                        {
                            Util.MessageInfo("SFU4335"); // 출고처를 선택해 주세요
                            return;
                        }

                        DataTable dtChk = new DataTable();
                        dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                        DataRow drChk = dtChk.NewRow();
                        drChk["SHIPTO_ID"] = cboTransLoc2_Pack.SelectedValue.ToString();

                        dtChk.Rows.Add(drChk);

                        DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);
                        if (Chk_Result.Rows.Count != 0)
                        {
                            if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                            {
                                // 품질결과 검사 체크
                                DataSet indataSet = new DataSet();

                                DataTable inData = indataSet.Tables.Add("INDATA");
                                inData.Columns.Add("LOTID", typeof(string));
                                inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                                inData.Columns.Add("BR_TYPE", typeof(string));

                                DataRow row = inData.NewRow();
                                row["LOTID"] = sLotid;
                                row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                                row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                                indataSet.Tables["INDATA"].Rows.Add(row);

                                //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                                //신규 HOLD 적용을 위해 변경 작업
                                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                                {
                                    if (Exception != null)
                                    {
                                        Util.MessageException(Exception);
                                        return;
                                    }
                                    else
                                    {
                                        Search_Pancake_Pack(sLotid);

                                        txtLotID_Pack.SelectAll();
                                        txtLotID_Pack.Focus();
                                    }

                                }, indataSet);
                            }
                            else
                            {
                                // 품질결과 Skip
                                Search_Pancake_Pack(sLotid);

                                txtLotID_Pack.SelectAll();
                                txtLotID_Pack.Focus();
                            }
                        }
                    }
                    else
                    {
                        Search_QMS_Validation_Pack(sLotid);
                        // 품질결과 Skip
                        Search_Pancake_Pack(sLotid);

                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }
                }
                else if (sLot_Type == "JUMBO_ROLL")
                {
                    sLotid = txtLotID_Pack.Text.ToString().Trim().ToUpper();

                    // 출고 이력 조회
                    DataTable RQSTDT0 = new DataTable();
                    RQSTDT0.TableName = "RQSTDT";
                    RQSTDT0.Columns.Add("LOT_ID", typeof(String));

                    DataRow dr0 = RQSTDT0.NewRow();
                    dr0["LOT_ID"] = sLotid;

                    RQSTDT0.Rows.Add(dr0);

                    DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT_JB", "RQSTDT", "RSLTDT", RQSTDT0);

                    if (OutResult.Rows.Count != 0)
                    {
                        if (OutResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString() == "SHIPPED" || OutResult.Rows[0]["RCV_ISS_STAT_CODE"].ToString() == "SHIPPING")
                        {
                            Util.MessageValidation("SFU3018"); //출고 이력이 존재합니다.
                            return;
                        }
                    }

                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dt.Columns.Add("COM_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                    dr["COM_CODE"] = "QMS_NOCHECK_PACKING";

                    dt.Rows.Add(dr);

                    DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);

                    if (AreaResult.Rows.Count == 0)
                    {
                        // 품질결과 검사 체크
                        DataSet indataSet = new DataSet();

                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                        inData.Columns.Add("BR_TYPE", typeof(string));

                        DataRow row = inData.NewRow();
                        row["LOTID"] = sLotid;
                        row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                        row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                        //신규 HOLD 적용을 위해 변경 작업
                        new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                return;
                            }

                            Search_Roll_Pack(sLotid);

                            txtLotID_Pack.SelectAll();
                            txtLotID_Pack.Focus();

                        }, indataSet);
                    }
                    else
                    {
                        Search_QMS_Validation_Pack(sLotid);
                        // 품질결과 Skip
                        Search_Roll_Pack(sLotid);
                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            } 
        }
        private void Search_QMS_Validation_Pack(string sLotid)
        {
            //WIP HOLD 체크
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));

            DataRow dr2 = dt2.NewRow();
            dr2["LOTID"] = sLotid;

            dt2.Rows.Add(dr2);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_FOR_HOLD_CHECK", "RQSTDT", "RSLTDT", dt2);
            if (Result.Rows.Count != 0)
            {
                if (Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                                             //return;
                }
                //QMS HOLD 체크 -DA_PRD_SEL_QMS_INFO  //  출하 단계에서 검사결과없으면 출하불능
                DataTable dt3 = new DataTable();
                dt3.Columns.Add("LOTID", typeof(string));

                DataRow dr3 = dt3.NewRow();
                dr3["LOTID"] = sLotid;

                dt3.Rows.Add(dr3);

                DataTable Result2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_INFO", "RQSTDT", "RSLTDT", dt3);

                // 포장카드에는“ OQC 검사 결과 없음”포장작업은 정상처리
                if (Result2.Rows.Count == 0)
                {
                    Util.Alert("SFU3492");   // 품질검사 결과가 없어서 출하가 불가합니다. 공정사에게 보고하세요.
                    sNote2_Pack = ObjectDic.Instance.GetObjectName("OQC 검사 결과 없음");

                    // 품질검서 없을시 등록된 인원들에 대해 BIZ 내에서 메일을 보냄
                    DataTable dt4 = new DataTable();
                    dt4.TableName = "INDATA";
                    dt4.Columns.Add("LANGID", typeof(string));
                    dt4.Columns.Add("SKIDID", typeof(string));

                    DataRow dr4 = dt4.NewRow();
                    dr4["LANGID"] = LoginInfo.LANGID;
                    dr4["SKIDID"] = sLotid;

                    dt4.Rows.Add(dr4);
                    new ClientProxy().ExecuteService("BR_PRD_CHK_QMS_FOR_MAILING", "INDATA", null, dt4, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                    });
                }
            }
        }

        private void Search_Pancake_Pack(string sLotid)
        {
            try
            {
                // 재공정보 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(String));
                RQSTDT2.Columns.Add("LANGID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = sLotid;
                dr2["LANGID"] = LoginInfo.LANGID;

                RQSTDT2.Rows.Add(dr2);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_CUTID", "RQSTDT", "RSLTDT", RQSTDT2);

                if (Lot_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }
                string ValueToKey = string.Empty;
                string ValueToFind = string.Empty;

                Dictionary<string, string> ValueToCompany = getShipCompany_Pack(Util.NVC(Lot_Result.Rows[0]["PRODID"]));
                foreach (KeyValuePair<string, string> items in ValueToCompany)
                {
                    ValueToKey = items.Key;
                    ValueToFind = items.Value;
                }

                if (dgOut_Pack.GetRowCount() == 0)
                {
                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                txtLotID_Pack.SelectAll();
                                txtLotID_Pack.Focus();
                                return;
                            }
                            Util.GridSetData(dgOut_Pack, Lot_Result, FrameOperation);
                            txtLotID_Pack.SelectAll();
                            txtLotID_Pack.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        Util.GridSetData(dgOut_Pack, Lot_Result, FrameOperation);
                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }
                }
                else
                {
                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Lot_Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "MKT_TYPE_CODE").ToString())
                    {
                        Util.Alert("SFU4454");   //내수용과 수출용은 같이 포장할 수 없습니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    if (NJ_AreaChk_Pack())
                    {
                        if (DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "PROD_VER_CODE").ToString() != Lot_Result.Rows[0]["PROD_VER_CODE"].ToString())
                        {
                            Util.Alert("SFU1501");   //동일 버전이 아닙니다.
                            return;
                        }
                    }

                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                txtLotID_Pack.SelectAll();
                                txtLotID_Pack.Focus();
                                return;
                            }
                            BindingPancake_Pack(Lot_Result);
                            txtLotID_Pack.SelectAll();
                            txtLotID_Pack.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        BindingPancake_Pack(Lot_Result);
                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

     
        private void BindingPancake_Pack(DataTable dt)
        {
            dgOut_Pack.IsReadOnly = false;
            dgOut_Pack.BeginNewRow();
            dgOut_Pack.EndNewRow(true);
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "LANE_QTY", dt.Rows[0]["LANE_QTY"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PROJECTNAME", dt.Rows[0]["PROJECTNAME"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PRDT_CLSS_CODE", dt.Rows[0]["PRDT_CLSS_CODE"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PRDT_CLSS_NAME", dt.Rows[0]["PRDT_CLSS_NAME"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PROCID", dt.Rows[0]["PROCID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "MKT_TYPE_CODE", dt.Rows[0]["MKT_TYPE_CODE"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PROD_VER_CODE", dt.Rows[0]["PROD_VER_CODE"].ToString());
            dgOut_Pack.IsReadOnly = true;
        }

        private bool NJ_AreaChk_Pack()
        {
            bool area_chk = true;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACKING_VERSION_CHECK_AREA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && LoginInfo.CFG_AREA_ID.Equals(dtResult.Rows[0]["CBO_CODE"]))
                {
                    area_chk = true;
                }
                else
                {
                    area_chk = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return area_chk;
        }

        private void Search_Roll_Pack(string sLotid)
        {
            try
            {
                // 재공정보 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotid;
                //dr1["PROCID"] = "E7000";  //<= 확인 필요
                dr1["WIPSTAT"] = "WAIT";

                RQSTDT1.Rows.Add(dr1);

                DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_JB", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Prod_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (Prod_Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                    return;
                }

                string ValueToKey = string.Empty;
                string ValueToFind = string.Empty;

                Dictionary<string, string> ValueToCompany = getShipCompany_Pack(Util.NVC(Prod_Result.Rows[0]["PRODID"]));
                foreach (KeyValuePair<string, string> items in ValueToCompany)
                {
                    ValueToKey = items.Key;
                    ValueToFind = items.Value;
                }

                if (dgOut_Pack.GetRowCount() == 0)
                {
                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                            dgOut_Pack.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                            Util.GridSetData(dgOut_Pack, Prod_Result, FrameOperation);
                            txtLotID_Pack.SelectAll();
                            txtLotID_Pack.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        dgOut_Pack.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                        Util.GridSetData(dgOut_Pack, Prod_Result, FrameOperation);
                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }
                }
                else
                {
                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Prod_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    //dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                            BindingRoll_Pack(Prod_Result);
                            txtLotID_Pack.SelectAll();
                            txtLotID_Pack.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        BindingRoll_Pack(Prod_Result);
                        txtLotID_Pack.SelectAll();
                        txtLotID_Pack.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void BindingRoll_Pack(DataTable dt)
        {
            dgOut_Pack.IsReadOnly = false;
            dgOut_Pack.BeginNewRow();
            dgOut_Pack.EndNewRow(true);
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut_Pack.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            dgOut_Pack.IsReadOnly = true;
        }

        private void btnRefresh_Pack_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOut_Pack);
            dgSub_Pack.Children.Clear();

            dgOut_Pack.IsEnabled = true;
            txtLotID_Pack.IsReadOnly = false;
            btnPackOut_Pack.IsEnabled = true;
            txtLotID_Pack.Text = "";
            sNote2_Pack = string.Empty;
            txtLotID_Pack.Focus();

            txtComment_Pack.Text = "";
        }

        private void btnPackOut_Pack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOut_Pack.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                string sLot_Type = cboType_Pack.SelectedValue.ToString();

                txtLotID_Pack.IsReadOnly = true;
                btnPackOut_Pack.IsEnabled = false;

                //dgOut.IsReadOnly = true;
                dgOut_Pack.IsEnabled = false;

                if (sLot_Type == "PANCAKE")
                {
                    string sTempProdName_1 = string.Empty;
                    string sTempProdName_2 = string.Empty;
                    string sPackingLotType1 = string.Empty;
                    string sPackingLotType2 = string.Empty;

                    bReprint_Pack = false;

                    int imsiCheck = 0;                // 설명:처리해야할 LOT 개수 판단
                    int iCheckCount = 0;

                    sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                    // Validation 로직1:Check된 개수 2개만 처리가능
                    if (dgOut_Pack.GetRowCount() > 2)
                    {
                        Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                        return;
                    }

                    // 조회결과에서 Check된 실적만 처리하기 위해 선별
                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        iCheckCount = iCheckCount + 1;
                        if (iCheckCount == 1)
                        {
                            imsiCheck = 1;
                            sTempLot_1_Pack = Util.NVC(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "LOTID"));
                        }
                        else if (iCheckCount == 2)
                        {
                            imsiCheck = 2;
                            sTempLot_2_Pack = Util.NVC(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "LOTID"));
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Visible;
                    if (imsiCheck == 1)
                    {
                        Get_Sub();
                    }
                    else if (imsiCheck == 2)
                    {
                        Get_Sub_Merge();
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    txtLotID_Pack.Text = "";

                }

                // JUMBO-ROLL
                else
                {
                    string sSHIPTO_ID = string.Empty;
                    sSHIPTO_ID = cboTransLoc2_Pack.SelectedValue.ToString();  //출고이력조회탭의 출고처(cboTransLoc)에서 포장출고의 출고처(cboTransLoc2_Pack)로 변경 2024.01.08

                    string sTO_SLOC_ID = string.Empty;
                    string sFROM_SLOC_ID = string.Empty;
                    string sShopID = string.Empty;

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                    RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["SHIPTO_ID"] = sSHIPTO_ID;

                    RQSTDT.Rows.Add(dr);

                    DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_SLOC_ID", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SlocIDResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU2071"); //To_Location 기준정보가 존재하지 않습니다.
                        return;
                    }
                    else
                    {
                        sTO_SLOC_ID = SlocIDResult.Rows[0]["TO_SLOC_ID"].ToString();
                        sShopID = SlocIDResult.Rows[0]["SHOPID"].ToString();
                    }


                    DataTable RQSTDT2 = new DataTable();
                    RQSTDT2.TableName = "RQSTDT";
                    RQSTDT2.Columns.Add("LOTID", typeof(String));

                    DataRow dr2 = RQSTDT2.NewRow();
                    dr2["LOTID"] = DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "LOTID").ToString();

                    RQSTDT2.Rows.Add(dr2);

                    DataTable FromResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FROM_SLOC_ID", "RQSTDT", "RSLTDT", RQSTDT2);

                    if (FromResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU2072"); //From_Location 기준정보가 존재하지 않습니다.
                        return;
                    }
                    else
                    {
                        sFROM_SLOC_ID = FromResult.Rows[0]["FROM_SLOC_ID"].ToString();
                    }

                    double dQty = 0;
                    double dQty2 = 0;
                    double dTotal = 0;
                    double dTotal2 = 0;

                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        dQty = Convert.ToDouble(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "M_WIPQTY").ToString());
                        dQty2 = Convert.ToDouble(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "CELL_WIPQTY").ToString());

                        dTotal = dTotal + dQty;
                        dTotal2 = dTotal2 + dQty2;
                    }


                    DataSet indataSet = new DataSet();

                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("FROM_AREAID", typeof(string));
                    inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                    inData.Columns.Add("TO_AREAID", typeof(string));
                    inData.Columns.Add("TO_SLOC_ID", typeof(string));
                    inData.Columns.Add("OUTBOXID", typeof(string));
                    inData.Columns.Add("PRODID", typeof(string));
                    inData.Columns.Add("BOXTYPE", typeof(string));
                    inData.Columns.Add("BOXLAYER", typeof(string));
                    inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                    inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                    inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                    inData.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                    inData.Columns.Add("SHIPTO_ID", typeof(string));
                    inData.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                    inData.Columns.Add("ISS_NOTE", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("PROCID", typeof(string));
                    inData.Columns.Add("TO_SHOPID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                    row["FROM_SLOC_ID"] = sFROM_SLOC_ID;
                    row["TO_AREAID"] = "";
                    row["TO_SLOC_ID"] = sTO_SLOC_ID;
                    row["OUTBOXID"] = "";
                    row["PRODID"] = DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "PRODID");
                    row["BOXTYPE"] = "PLT";
                    row["BOXLAYER"] = "0";
                    row["PACK_LOT_TYPE_CODE"] = "LOT";
                    row["TOTAL_QTY"] = dTotal;
                    row["TOTAL_QTY2"] = dTotal2;
                    row["EXP_DOM_TYPE_CODE"] = "E";
                    row["SHIPTO_ID"] = sSHIPTO_ID;
                    row["OWMS_BOX_TYPE_CODE"] = "";
                    row["ISS_NOTE"] = "";
                    row["NOTE"] = "";
                    row["USERID"] = LoginInfo.USERID;
                    row["PROCID"] = DataTableConverter.GetValue(dgOut_Pack.Rows[0].DataItem, "PROCID");
                    row["TO_SHOPID"] = sShopID;

                    indataSet.Tables["INDATA"].Rows.Add(row);

                    DataTable inBox = indataSet.Tables.Add("BOXID");

                    inBox.Columns.Add("INBOXID", typeof(string));
                    inBox.Columns.Add("PRODID", typeof(string));
                    inBox.Columns.Add("BOXTYPE", typeof(string));
                    inBox.Columns.Add("BOXLAYER", typeof(string));
                    inBox.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                    inBox.Columns.Add("TOTAL_QTY", typeof(decimal));
                    inBox.Columns.Add("TOTAL_QTY2", typeof(decimal));
                    inBox.Columns.Add("EXP_DOM_TYPE_CODE", typeof(string));
                    inBox.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        DataRow row2 = inBox.NewRow();

                        row2["INBOXID"] = "";
                        row2["PRODID"] = DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "PRODID").ToString();
                        row2["BOXTYPE"] = "";
                        row2["BOXLAYER"] = "0";
                        row2["PACK_LOT_TYPE_CODE"] = "LOT";
                        row2["TOTAL_QTY"] = Convert.ToDecimal(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "M_WIPQTY"));
                        row2["TOTAL_QTY2"] = Convert.ToDecimal(DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "CELL_WIPQTY"));
                        row2["EXP_DOM_TYPE_CODE"] = "E";
                        row2["OWMS_BOX_TYPE_CODE"] = "";

                        indataSet.Tables["BOXID"].Rows.Add(row2);
                    }


                    DataTable inLot = indataSet.Tables.Add("INLOT");

                    inLot.Columns.Add("INBOXID", typeof(string));
                    inLot.Columns.Add("LOTID", typeof(string));
                    inLot.Columns.Add("LOTQTY", typeof(string));
                    inLot.Columns.Add("LOTQTY2", typeof(string));

                    for (int i = 0; i < dgOut_Pack.GetRowCount(); i++)
                    {
                        DataRow row3 = inLot.NewRow();

                        row3["INBOXID"] = "";
                        row3["LOTID"] = DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "LOTID");
                        row3["LOTQTY"] = DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "M_WIPQTY");
                        row3["LOTQTY2"] = DataTableConverter.GetValue(dgOut_Pack.Rows[i].DataItem, "CELL_WIPQTY");

                        indataSet.Tables["INLOT"].Rows.Add(row3);
                    }

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_LOT_FOR_PACKING", "INDATA,BOXID,INLOT", null, indataSet);

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    txtLotID_Pack.Text = "";

                    Util.gridClear(dgOut_Pack);
                    dgSub_Pack.Children.Clear();

                    dgOut_Pack.IsEnabled = true;
                    txtLotID_Pack.IsReadOnly = false;
                    btnPackOut_Pack.IsEnabled = true;
                    txtLotID_Pack.Text = "";
                    txtLotID_Pack.Focus();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                dgOut_Pack.IsEnabled = true;
                txtLotID_Pack.IsReadOnly = false;
                btnPackOut_Pack.IsEnabled = true;
                txtLotID_Pack.Text = "";
                txtLotID_Pack.Focus();
                return;
            }
        }


        private void Get_Sub()
        {
            if (dgSub_Pack.Children.Count == 0)
            {
                bNew_Load_Pack = true;
                window01_Pack.BOX001_330 = this;
                window01_Pack.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub_Pack.Children.Add(window01_Pack);
            }
        }

        private void Get_Sub_Merge()
        {
            if (dgSub_Pack.Children.Count == 0)
            {
                bNew_Load_Pack = true;
                window02_Pack.BOX001_330 = this;
                window02_Pack.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub_Pack.Children.Add(window02_Pack);
            }
        }

        #region 포장 이력 조회 - 남경
        private void txtBoxID_Hist_Pack_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtBoxID_Hist_Pack.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist_Pack();
                        loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void txtLotid_Hist_Pack_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotid_Hist_Pack.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist_Pack();
                        loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void txtSkid_Hist_Pack_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSkid_Hist_Pack.Text != "")
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        Serach_Box_Hist_Pack();
                        loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void btnSearch_Box_Pack_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Serach_Box_Hist_Pack();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void Serach_Box_Hist_Pack()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom_Box_Pack.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo_Box_Pack.SelectedDateTime);

                if (txtLotid_Hist_Pack.Text != "" || txtBoxID_Hist_Pack.Text != "" || txtSkid_Hist_Pack.Text != "")
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
                // CNJ, CNA 라이 조회조건 추가 [C20180808_60878]
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXSTAT"] = "PACKED";
                dr["LOTID"] = txtLotid_Hist_Pack.Text.Trim() == "" ? null : txtLotid_Hist_Pack.Text;
                dr["BOXID"] = txtBoxID_Hist_Pack.Text.Trim() == "" ? null : txtBoxID_Hist_Pack.Text;
                dr["CSTID"] = txtSkid_Hist_Pack.Text.Trim() == "" ? null : txtSkid_Hist_Pack.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc_Hist_Pack, bAllNull: true);
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["PRODID"] = txtProd_ID_Hist_Pack.Text.Trim() == "" ? null : txtProd_ID_Hist_Pack.Text;
                // CNJ, CNA 라이 조회조건 추가 [C20180808_60878]
                dr["EQSGID"] = Util.GetCondition(cboEqsg_Hist_Pack, bAllNull: true);

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NJ_PACK_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgBox_Hist_Pack);
                Util.GridSetData(dgBox_Hist_Pack, SearchResult, FrameOperation);

                string[] sColumnName = new string[] { "OUTER_BOXID", "CSTID" };
                _Util.SetDataGridMergeExtensionCol(dgBox_Hist_Pack, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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
            dagsum.ResultTemplate = dgBox_Hist_Pack.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            daq.Add(dgcount);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist_Pack.Columns["LOTID"], daq);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist_Pack.Columns["TOTAL_QTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgBox_Hist_Pack.Columns["WIPQTY2"], dac);
        }

        private void txtProd_ID_Hist_Pack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID_Hist_Pack.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Serach_Box_Hist_Pack();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.MessageValidation("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        #endregion 포장 이력 조회 - 남경

        private void btnDelete_Pack_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgOut_Pack.IsReadOnly = false;
                    dgOut_Pack.RemoveRow(index);
                    dgOut_Pack.IsReadOnly = true;
                }
            });
        }

        #endregion  전극포장 End
        /*********************************************** 전극포장 End ****************************/

    }
}
