/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.13  김도형    : [E20230306-000128]전극수 소수점 개선
 

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media.Animation;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_045 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();

        private System.Windows.Threading.DispatcherTimer timer;
        private LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING_OQC_RP sampling;
        private DataTable OriginSamplingData;
        private bool IsSamplingCheck = false;

        public string sTempLot_1 = string.Empty;
        public string sTempLot_2 = string.Empty;
        public string sTempLot_3 = string.Empty;
        public string sNote2 = string.Empty;
        private string _APPRV_PASS_NO = string.Empty;

        public Boolean bReprint = true;

        private BOX001_045_PACKINGCARD_MERGE window02 = new BOX001_045_PACKINGCARD_MERGE();

        public bool bNew_Load = false;
        public DataTable dtPackingCard;

        public bool bCancel = false;


        public BOX001_045()
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
            listAuth.Add(btnPackOut);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //SetEvent();

            //txtLotID.Focus();

            #region Quality Check [자동차 1,2동만 적용] 
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                // SampleData 
                SetActSamplingData();

                timer.Tick -= timer_Start;
                timer.Tick += timer_Start;
                timer.Start();
            }
            #endregion
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                timer.Stop();
                timer.Tick -= timer_Start;
            }
        }


        #endregion

        #region Initialize
        private void Initialize()
        {

            #region Combo Setting
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "WH_DIVISION" };
            String[] sFilter2 = { "WH_SHIPMENT" };
            String[] sFilter3 = { "WH_TYPE" };
            String[] sFilter4 = { "WH_STATUS" };
            String[] sFilter5 = { "SHIP_BOX_RCV_ISS_STAT_CODE" };

            _combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE");

            _combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            #endregion

            // PANCAKE 고정요청 [2017-09-04]
            cboType.SelectedValue = "PANCAKE";

            #region Quality Check [자동차 1,2동만 적용] 
            //2022-12-29 오화백  동 :EP 추가
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                // Visible
                stQuality.Visibility = Visibility.Visible;

                // SampleData 
                //SetActSamplingData();

                // Timer
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMinutes(1d); // 1분 간격으로 설정
                //timer.Tick -= timer_Start;
                //timer.Tick += timer_Start;
                //timer.Start();  
            }
            #endregion

            txtLotID.Focus();
        }

        #endregion

        #region Event

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOut);
            dgSub.Children.Clear();

            dgOut.IsEnabled = true;
            txtLotID.IsReadOnly = false;
            btnPackOut.IsEnabled = true;
            txtLotID.Text = "";
            sNote2 = string.Empty;
            txtLotID.Focus();

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOut.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }
                if (dgOut.GetRowCount() > 3)
                {
                    Util.MessageValidation("SFU4310"); //3개 SKID까지 포장가능합니다.
                    return;
                }

                string sLot_Type = cboType.SelectedValue.ToString();

                txtLotID.IsReadOnly = true;
                btnPackOut.IsEnabled = false;

                //dgOut.IsReadOnly = true;
                dgOut.IsEnabled = false;

                if (sLot_Type == "PANCAKE")
                {
                    string sTempProdName_1 = string.Empty;
                    string sTempProdName_2 = string.Empty;
                    string sTempProdName_3 = string.Empty;
                    string sPackingLotType1 = string.Empty;
                    string sPackingLotType2 = string.Empty;
                    string sPackingLotType3 = string.Empty;

                    bReprint = false;

                    int imsiCheck = 0;                // 설명:처리해야할 LOT 개수 판단
                    int iCheckCount = 0;

                    sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType3 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                    // Validation 로직1:Check된 개수 3개만 처리가능
                    if (dgOut.GetRowCount() > 3)
                    {
                        Util.MessageValidation("SFU4310"); //3개 SKID까지 포장가능합니다.
                        return;
                    }

                    // 조회결과에서 Check된 실적만 처리하기 위해 선별
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        iCheckCount = iCheckCount + 1;
                        if (iCheckCount == 1)
                        {
                            imsiCheck = 1;
                            sTempLot_1 = "";
                            sTempLot_2 = "";
                            sTempLot_3 = "";

                            sTempLot_1 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        }
                        else if (iCheckCount == 2)
                        {
                            imsiCheck = 2;
                            sTempLot_2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        }
                        else if (iCheckCount == 3)
                        {
                            imsiCheck = 3;
                            sTempLot_3 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Visible;

                    Get_Sub_Merge();

                    loadingIndicator.Visibility = Visibility.Collapsed;
                    txtLotID.Text = "";

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                dgOut.IsEnabled = true;
                txtLotID.IsReadOnly = false;
                btnPackOut.IsEnabled = true;
                txtLotID.Text = "";
                txtLotID.Focus();
                return;
            }
        }

        private void Get_Sub_Merge()
        {
            if (dgSub.Children.Count == 0)
            {
                bNew_Load = true;
                window02.BOX001_045 = this;
                window02.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub.Children.Add(window02);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sLotid = string.Empty;
                    string sLot_Type = string.Empty;

                    if (txtLotID.Text.ToString() == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    if (cboType.SelectedIndex < 0 || cboType.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.Alert("SFU1895");   //제품을 선택하세요
                        return;
                    }
                    else
                    {
                        sLot_Type = cboType.SelectedValue.ToString();
                    }


                    if (dgOut.GetRowCount() >= 3)
                    {
                        Util.MessageValidation("SFU4310"); // 3개 SKID까지 포장가능합니다.
                        return;
                    }

                    if (sLot_Type == "PANCAKE")
                    {
                        sLotid = txtLotID.Text.ToString().Trim();

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
                        #region # 시생산 Lot 
                        DataRow[] dRow = Prod_Result.Select("LOTTYPE = 'X'");
                        if (dRow.Length > 0)
                        {
                            Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다
                            return;
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
                            DataTable dtChk = new DataTable();
                            dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                            DataRow drChk = dtChk.NewRow();
                            drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

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
                                            Search_Pancake(sLotid);

                                            txtLotID.SelectAll();
                                            txtLotID.Focus();
                                        }

                                    }, indataSet);
                                }
                                else
                                {
                                    // 품질결과 Skip
                                    Search_Pancake(sLotid);

                                    txtLotID.SelectAll();
                                    txtLotID.Focus();
                                }
                            }
                        }
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_Pancake(sLotid);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else if (sLot_Type == "JUMBO_ROLL")
                    {
                        sLotid = txtLotID.Text.ToString().Trim().ToUpper();

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

                                Search_Roll(sLotid);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_Roll(sLotid);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void Search_QMS_Validation(string sLotid)
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
                    sNote2 = ObjectDic.Instance.GetObjectName("OQC 검사 결과 없음");

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

        private void Search_Pancake(string sLotid)
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

                Dictionary<string, string> ValueToCompany = getShipCompany(Util.NVC(Lot_Result.Rows[0]["PRODID"]));
                foreach (KeyValuePair<string, string> items in ValueToCompany)
                {
                    ValueToKey = items.Key;
                    ValueToFind = items.Value;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                txtLotID.SelectAll();
                                txtLotID.Focus();
                                return;
                            }
                            Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }

                    // 특이사항이 등록된 LOT인지 조회 (2022.04.26. pbdebug)
                    CheckLotRemark();
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Lot_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    if (!string.Equals(ValueToKey, string.Empty))
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                txtLotID.SelectAll();
                                txtLotID.Focus();
                                return;
                            }
                            BindingPancake(Lot_Result);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        BindingPancake(Lot_Result);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }
        private void BindingPancake(DataTable dt)
        {
            dgOut.IsReadOnly = false;
            dgOut.BeginNewRow();
            dgOut.EndNewRow(true);
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LANE_QTY", dt.Rows[0]["LANE_QTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROJECTNAME", dt.Rows[0]["PROJECTNAME"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRDT_CLSS_CODE", dt.Rows[0]["PRDT_CLSS_CODE"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRDT_CLSS_NAME", dt.Rows[0]["PRDT_CLSS_NAME"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROCID", dt.Rows[0]["PROCID"].ToString());

            dgOut.IsReadOnly = true;
        }

        private void Search_Roll(string sLotid)
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

                Dictionary<string, string> ValueToCompany = getShipCompany(Util.NVC(Prod_Result.Rows[0]["PRODID"]));
                foreach (KeyValuePair<string, string> items in ValueToCompany)
                {
                    ValueToKey = items.Key;
                    ValueToFind = items.Value;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    if (ValueToCompany.Count > 0)
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                            dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                            Util.GridSetData(dgOut, Prod_Result, FrameOperation);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                        Util.GridSetData(dgOut, Prod_Result, FrameOperation);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Prod_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    //dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                    if (ValueToCompany.Count > 0)
                    {
                        Util.MessageConfirm("SFU5048", (result) =>
                        {
                            if (result == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                            BindingRoll(Prod_Result);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }, new object[] { ValueToFind, ValueToKey });
                    }
                    else
                    {
                        BindingRoll(Prod_Result);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void BindingRoll(DataTable dt)
        {
            dgOut.IsReadOnly = false;
            dgOut.BeginNewRow();
            dgOut.EndNewRow(true);
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            dgOut.IsReadOnly = true; ;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgOut.IsReadOnly = false;
                    dgOut.RemoveRow(index);
                    dgOut.IsReadOnly = true;
                }
            });
        }
        private void cboType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLot_Type = cboType.SelectedValue.ToString();

            if (sLot_Type == "JUMBO_ROLL")
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("출고");

                dgOut.Columns["M_WIPQTY"].Header = "S/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "N/ROLL";
            }
            else
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("포장구성");

                dgOut.Columns["M_WIPQTY"].Header = "C/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "S/ROLL";
            }
        }

        #endregion


        #region 자동차동 SAMPLING 전용 FUNCTION
        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
            sampling.FrameOperation = FrameOperation;

            if (sampling != null)
            {
                C1WindowExtension.SetParameters(sampling, null);

                sampling.Closed -= new EventHandler(OnCloseSampling);
                sampling.Closed += new EventHandler(OnCloseSampling);
                this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
            }
        }

        private void timer_Start(object sender, EventArgs e)
        {
            if (sampling == null && IsSamplingCheck == false)
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

                SetActSamplingData();
            }
        }

        private void OnCloseSampling(object sender, EventArgs e)
        {
            CMM001.CMM_ELEC_SAMPLING_OQC_RP window = sender as CMM001.CMM_ELEC_SAMPLING_OQC_RP;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (IsSamplingCheck)
                {
                    IsSamplingCheck = false;
                    Storyboard board = (Storyboard)this.Resources["storyBoard"];
                    if (board != null)
                        board.Stop();
                }
            }
            sampling.Close();
            sampling = null;
            GC.Collect();
        }

        private void SetActSamplingData()
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

                        if (OriginSamplingData == null)
                            OriginSamplingData = result;
                        else
                            IsDiffSamplingData(OriginSamplingData, result);
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
                        if ((string.IsNullOrEmpty(Util.NVC(oldRow["JUDG_FLAG"])) || string.Equals(oldRow["JUDG_FLAG"], "F")) && string.Equals(newRow["JUDG_FLAG"], "Y") && IsSamplingCheck == false)
                        {
                            IsSamplingCheck = true;
                            Storyboard board = (Storyboard)this.Resources["storyBoard"];
                            if (board != null)
                                board.Begin();

                            // 팝업 자동 생성
                            if (sampling == null && chkQuality.IsChecked == true)
                            {
                                sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
                                sampling.FrameOperation = FrameOperation;

                                if (sampling != null)
                                {
                                    C1WindowExtension.SetParameters(sampling, null);

                                    sampling.Closed -= new EventHandler(OnCloseSampling);
                                    sampling.Closed += new EventHandler(OnCloseSampling);
                                    this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
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
                OriginSamplingData.Clear();
                OriginSamplingData = newData;
            }
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

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
                ShipToIndata["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

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

        #endregion

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOutHIst.GetRowCount() == 0 || dgOutDetail.GetRowCount() == 0)
                {
                    Util.Alert("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("10008");   //선택된 데이터가 없습니다.
                    return;
                }

                SteelRackReprint();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SteelRackReprint()
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();
                string sOuter_Boxid = drChk[0]["OUTER_BOXID"].ToString();
                string procRemark = string.Empty;
                string pan_gr_id = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRcv_Iss_Id;
                dr["CMCDTYPE"] = "PRDT_ABBR_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable ReprintResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT", "RQSTDT", "RSLTDT", RQSTDT);

                if (ReprintResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LANGID", typeof(String));
                RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["RCV_ISS_ID"] = sRcv_Iss_Id;
                RQSTDT1.Rows.Add(dr1);

                DataTable CutidResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_CUTID", "RQSTDT", "RSLTDT", RQSTDT1);

                if (CutidResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                    return;
                }

                /// Line 정보 ///
                DataTable RQSTLotList = new DataTable();
                RQSTLotList.TableName = "RQSTDT";
                RQSTLotList.Columns.Add("BOXID", typeof(String));

                DataRow drLotList = RQSTLotList.NewRow();
                drLotList["BOXID"] = Util.NVC(drChk[0]["Outer_BoxID"]);
                RQSTLotList.Rows.Add(drLotList);

                DataTable dtLotList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STEELBOX_CONV_QTY_LOT", "RQSTDT", "RSLTDT", RQSTLotList, RowSequenceNo: true);

                for (int i = 0; i < dtLotList.Rows.Count; i++)
                {
                    pan_gr_id += dtLotList.Rows[i]["SKID_ID"].ToString() + ",";
                }

                //열처리 체크

                DataTable result = new DataTable();
                result.TableName = "RQSTDT";
                result.Columns.Add("LANGID", typeof(String));
                result.Columns.Add("CSTID", typeof(String));
                result.Columns.Add("AREAID", typeof(String));

                DataRow dr4 = result.NewRow();

                dr4["LANGID"] = LoginInfo.LANGID;
                dr4["CSTID"] = pan_gr_id;
                dr4["AREAID"] = LoginInfo.CFG_AREA_ID;

                result.Rows.Add(dr4);

                DataTable chkResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_PACK_PROC_ROUTE_CHK", "RQSTDT", "RSLTDT", result);

                if (!chkResult.Rows[0]["PROC_REMARK"].ToString().Equals(""))
                {
                    procRemark = ObjectDic.Instance.GetObjectName(chkResult.Rows[0]["PROC_REMARK"].ToString());
                }
                else
                {
                    procRemark = "";
                }

                /// Header 정보 ///                
                // 유효일자 //
                DataRow[] Rows = dtLotList.Select("", "VLD_DATE ASC");
                string ValidateDate = Rows[0]["VLD_DATE"].ToString();

                // 생산일자 //
                Rows = dtLotList.Select("", "REG_DATE ASC");
                string WipDate = Rows[0]["REG_DATE"].ToString();

                // 합계 //
                string TotalLane = dtLotList.Rows.Count.ToString();
                decimal TotalCRoll = Util.NVC_Decimal(dtLotList.Compute("SUM(CROLL)", ""));
                decimal TotalSRoll = Util.NVC_Decimal(dtLotList.Compute("SUM(SROLL)", ""));
                decimal TotalM = Util.NVC_Decimal(dtLotList.Compute("SUM(M)", ""));

                DataTable dtReprint = new DataTable();
                dtReprint.Columns.Add("Title", typeof(string));
                dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                dtReprint.Columns.Add("PACK_NO", typeof(string));
                dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                dtReprint.Columns.Add("Transfer", typeof(string));
                dtReprint.Columns.Add("Total_M", typeof(string));
                dtReprint.Columns.Add("Total_Cell", typeof(string));
                dtReprint.Columns.Add("VLD_DATE", typeof(string));
                dtReprint.Columns.Add("REG_DATE", typeof(string));
                dtReprint.Columns.Add("REMARK", typeof(string));
                dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                dtReprint.Columns.Add("V_DATE", typeof(string));
                dtReprint.Columns.Add("P_DATE", typeof(string));
                dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                dtReprint.Columns.Add("PRODID", typeof(string));
                dtReprint.Columns.Add("PROC_REMARK", typeof(string));

                DataRow drCrad = dtReprint.NewRow();

                if (Util.NVC(GetEltrValueDecimalApplyFlag()).Equals("Y"))  ////[E20230306-000128]电极数量小数点改善(전극수 소수점 개선)
                {
                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드 SteelRack"),
                                                  ReprintResult.Rows[0]["MODLID"].ToString(),
                                                  ReprintResult.Rows[0]["OUTER_BOXID"].ToString(),
                                                  "*" + ReprintResult.Rows[0]["OUTER_BOXID"].ToString() + "*",
                                                  ReprintResult.Rows[0]["FROM_AREA"].ToString() + " -> " + ReprintResult.Rows[0]["SHIPTO_NAME"].ToString(),
                                                  string.Format("{0:#,##0.#}",(decimal)(Math.Truncate( TotalCRoll * 10) / 10) ) +"/"+ string.Format("{0:#,##0.#}",(decimal)(Math.Truncate( TotalM * 10) / 10) )+"M",
                                                  string.Format("{0:#,##0.#}",(decimal)(Math.Truncate( TotalSRoll * 10) / 10) ) +"/"+ string.Format("{0:#,##0.#}",(decimal)(Math.Truncate( TotalM * 10) / 10) )+"M",
                                                  ValidateDate,
                                                  WipDate,
                                                  Util.NVC(ReprintResult.Rows[0]["ISS_NOTE"]),
                                                  ReprintResult.Rows[0]["UNIT_CODE"].ToString(),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  Util.NVC(ReprintResult.Rows[0]["OFFER_SHEET_DESCRIPTION"]),
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ "
                                                     + ReprintResult.Rows[0]["PRDT_ABBR_CODE"].ToString() + " ]",
                                                  procRemark
                                               };
                }
                else
                {
                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드 SteelRack"),
                                                  ReprintResult.Rows[0]["MODLID"].ToString(),
                                                  ReprintResult.Rows[0]["OUTER_BOXID"].ToString(),
                                                  "*" + ReprintResult.Rows[0]["OUTER_BOXID"].ToString() + "*",
                                                  ReprintResult.Rows[0]["FROM_AREA"].ToString() + " -> " + ReprintResult.Rows[0]["SHIPTO_NAME"].ToString(),
                                                  string.Format("{0:#,##0}",TotalCRoll) +"/"+ string.Format("{0:#,##0}",TotalM)+"M",
                                                  string.Format("{0:#,##0}",TotalSRoll)+"/"+ string.Format("{0:#,##0}",TotalM)+"M",
                                                  ValidateDate,
                                                  WipDate,
                                                  Util.NVC(ReprintResult.Rows[0]["ISS_NOTE"]),
                                                  ReprintResult.Rows[0]["UNIT_CODE"].ToString(),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  Util.NVC(ReprintResult.Rows[0]["OFFER_SHEET_DESCRIPTION"]),
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ "
                                                     + ReprintResult.Rows[0]["PRDT_ABBR_CODE"].ToString() + " ]",
                                                  procRemark
                                               };
                }
 
                dtReprint.Rows.Add(drCrad);

                LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    //태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "PackingCard_New_ReThree";
                    Parameters[1] = dtReprint;
                    Parameters[2] = dtLotList;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result);
                    // 팝업 화면 숨겨지는 문제 수정.
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal iTotal_Qty = 0;
                decimal iTotal_Qty2 = 0;

                if (dgOutDetail.GetRowCount() <= 0)
                {
                    Util.AlertInfo("SFU1636");  //선택된 대상이 없습니다.
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.AlertInfo("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                if (!drChk[0]["BOX_RCV_ISS_STAT_CODE"].ToString().Equals("SHIPPING"))
                {
                    Util.AlertInfo("SFU1939");   //취소할수있는상태가아닙니다.
                    return;
                }

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
                    Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
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
                row["OUTBOXID"] = drChk[0]["OUTER_BOXID"].ToString();
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
                    //row2["UNPACK_QTY"] = BoxResult.Rows[i]["WIPQTY"].ToString();
                    //row2["UNPACK_QTY2"] = BoxResult.Rows[i]["WIPQTY2"].ToString();
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_LOT_FOR_PACKING", "INDATA,INNERBOX,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        Util.gridClear(dgOutDetail);
                        Boxmapping_Master();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Boxmapping_Master();
            loadingIndicator.Visibility = Visibility.Collapsed;
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

        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Boxmapping_Master();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
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
                    loadingIndicator.Visibility = Visibility.Visible;
                    Boxmapping_Master();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void Boxmapping_Master()
        {
            try
            {
                string sShipToID = string.Empty;
                string sStatus = string.Empty;
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                Util.gridClear(dgOutHIst);
                Util.gridClear(dgOutDetail);

                if (cboStatus.SelectedIndex < 0 || cboStatus.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
                }
                else
                {
                    sStatus = cboStatus.SelectedValue.ToString();
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
                dr["OUTER_BOXID"] = txtBoxid.Text.Trim() == "" ? null : txtBoxid.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID.Text.Trim() == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_STEEL_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutHIst);
                Util.GridSetData(dgOutHIst, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void CheckLotRemark()
        {
            if (dgOut.Rows.Count == 0)
                return;

            string sMent = string.Empty;
            string sBtchNote = string.Empty;
            string sLotIDS = string.Empty;

            DataTable RQSTDT = new DataTable("INDATA");
            RQSTDT.Columns.Add("SKIDID", typeof(string));

            for (int i = 0; i < dgOut.Rows.Count; i++)
            {
                DataRow dr = RQSTDT.NewRow();
                dr["SKIDID"] = DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID");
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ELTR_VISUAL_MNGT_LOT_RMK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                    {
                        if (sBtchNote.Length == 0)
                        {
                            sBtchNote = "[" + Util.NVC(dtRslt.Rows[j]["BTCH_NOTE"]) + "]";
                        }

                        sLotIDS = sLotIDS + "[" + Util.NVC(dtRslt.Rows[j]["LOTID"]) + "]\n";
                    }
                }

                RQSTDT.Rows.Clear();
            }

            sMent = "\n\n특이사항: " + sBtchNote + "\n\nLOTID: \n" + sLotIDS;

            if (sLotIDS.Length > 0)
                Util.MessageValidation("SFU9997", sMent); // 특이사항이 등록된 LOT이 있습니다.  %1

        }

        //[E20230306-000128]전극수 소수점 개선
        private string GetEltrValueDecimalApplyFlag()
        {

            string sEltrValueDecimalApplyFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_VALUE_DECIMAL_APPLY_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrValueDecimalApplyFlag = Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());

                }
                else
                {
                    sEltrValueDecimalApplyFlag = "N";
                }

                return sEltrValueDecimalApplyFlag;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return sEltrValueDecimalApplyFlag;
            }
        }

    }
}

