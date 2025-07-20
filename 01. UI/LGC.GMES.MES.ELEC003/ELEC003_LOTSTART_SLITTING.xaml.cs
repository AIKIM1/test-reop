/*************************************************************************************
 Created Date : 2021.07.08
      Creator : 조영대
   Decription : 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
 2021.07.08  조영대 : Initial Created.
 2025.07.13  양직일   LOT START 시 BOBBIN ID를 받아 LOT을 착공한다.
                      착공후 OUT LOT은 LANE 번호에 맞추어 BOBBIN이 연결되게 된다.     
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// ELEC001_LOTSTART
    /// </summary>
    public partial class ELEC003_LOTSTART_SLITTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _LANEQTY = string.Empty;
        private string _RUNLOT = string.Empty;  // 작업시작 Lot
        private string _COAT_SIDE_TYPE = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _ELEC_CSTID = string.Empty;   //전극 CST ID

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }

        public string _ReturnProdID
        {
            get { return _PRODID; }
        }
        public string _ReturnLaneQty
        {
            get { return _LANEQTY; }
        }

        private enum TYPE
        {
            MAPPING = 0,
            EMPTY = 1,
            LOCK = 2
        }

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }
        public ELEC003_LOTSTART_SLITTING()
        {
            InitializeComponent();
            ApplyPermissions();
        }

        public ELEC003_LOTSTART_SLITTING(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("RUNLOT")) _RUNLOT = dicParam["RUNLOT"];
                if (dicParam.ContainsKey("COAT_SIDE_TYPE")) _COAT_SIDE_TYPE = dicParam["COAT_SIDE_TYPE"];

                SetIdentInfo();
                dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                }

                string strUseYN = getCarrierUseYN();
                if (strUseYN == "Y")
                {
                    txtBobbinT.IsEnabled = true; 
                    txtBobbinB.IsEnabled = true;
                }
                else
                {
                    txtBobbinT.IsEnabled = false;
                    txtBobbinB.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!GetLotInfo())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(_PROCID) || string.IsNullOrEmpty(_EQSGID))
                {
                    _LDR_LOT_IDENT_BAS_CODE = "";
                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = _PROCID;
                row["EQSGID"] = _EQSGID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); };
        }



          private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }

            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();

                    #region MESSAGE SET
                    DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");
                    inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inLotDataTable.Columns.Add("IFMODE", typeof(string));
                    inLotDataTable.Columns.Add("EQPTID", typeof(string));
                    inLotDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inLotDataRow = null;
                    inLotDataRow = inLotDataTable.NewRow();
                    inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    inLotDataRow["EQPTID"] = _EQPTID;
                    inLotDataRow["USERID"] = LoginInfo.USERID;
                    inLotDataTable.Rows.Add(inLotDataRow);

                    DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                    InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                    InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                    DataRow inMtrlDataRow = null;
                    inMtrlDataRow = InMtrldataTable.NewRow();
                    inMtrlDataRow["INPUT_LOTID"] = _LOTID;
                    inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(_EQPTID);
                    inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                    InMtrldataTable.Rows.Add(inMtrlDataRow);
                    #endregion



                    string strUseYN = getCarrierUseYN();
                    if (strUseYN == "Y")
                    {

                        fnSlitterLotStartBobbinBind();

                    }
                    else
                    {

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_SL", "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                        {
                            #region 공정UI LOGIC : BR_PRD_REG_START_LOT_SL
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            #region QC 샘플 PopUp
                            DataTable dtCutTable = result.Tables["RSLTDT"];

                            string strCutid = dtCutTable.Rows[0]["CUT_ID"].ToString();

                            //2020.12.23 Slitter QC 샘플 화면 POPUP 
                            //PROCESSEQUIPMENTSEGMENT.GQMS_SMPLG_POPUP_APPLY_FLAG 값이 'Y'이면 실행
                            DataTable InTable = new DataTable();
                            InTable.Columns.Add("PROCID", typeof(string));
                            InTable.Columns.Add("EQSGID", typeof(string));

                            DataRow dtRow = InTable.NewRow();
                            dtRow["PROCID"] = Process.SLITTING;
                            dtRow["EQSGID"] = _EQPTID;
                            InTable.Rows.Add(dtRow);

                            DataTable dtProcEqst = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", InTable);

                            if (dtProcEqst != null && dtProcEqst.Rows.Count > 0 &&
                                Convert.ToString(dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"]) == "Y")
                            {
                                DataSet inDataSet2 = new DataSet();

                                //Slitter QC 샘플 POPUP 실행 비즈 호출
                                DataTable InEqpTable = inDataSet2.Tables.Add("IN_EQP");
                                InEqpTable.Columns.Add("SRCTYPE", typeof(string));
                                InEqpTable.Columns.Add("IFMODE", typeof(string));
                                InEqpTable.Columns.Add("EQPTID", typeof(string));
                                InEqpTable.Columns.Add("USERID", typeof(string));
                                InEqpTable.Columns.Add("SMPL_REG_YN", typeof(string));

                                DataRow drEqp = null;
                                drEqp = InEqpTable.NewRow();
                                drEqp["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                drEqp["IFMODE"] = IFMODE.IFMODE_OFF;
                                drEqp["EQPTID"] = _EQPTID;
                                drEqp["USERID"] = LoginInfo.USERID;
                                drEqp["SMPL_REG_YN"] = dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"].ToString();
                                InEqpTable.Rows.Add(drEqp);

                                DataTable dtLotTable = inDataSet2.Tables.Add("IN_LOT");
                                dtLotTable.Columns.Add("LOTID", typeof(string));

                                DataRow drLot = null;
                                drLot = dtLotTable.NewRow();
                                drLot["LOTID"] = strCutid;

                                dtLotTable.Rows.Add(drLot);

                                DataSet dsInfoRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHK_LOT_SAMPLING_SL", "IN_EQP,IN_LOT", "OUTDATA", inDataSet2);

                                DataTable dtInfo = dsInfoRslt.Tables["OUTDATA"];

                                if (dtInfo.Rows[0]["MSGTYPE"].Equals("INFO"))
                                {
                                    //Message 등록 여부 확인
                                    DataTable dtCode = new DataTable();
                                    dtCode.Columns.Add("LANGID", typeof(string));
                                    dtCode.Columns.Add("MSGID", typeof(string));

                                    DataRow dtRowMsg = dtCode.NewRow();
                                    dtRowMsg["LANGID"] = LoginInfo.LANGID;
                                    dtRowMsg["MSGID"] = "1" + dtInfo.Rows[0]["MSGCODE"].ToString(); //기존 메시지 변경 불가로 신규 메시지로 처리

                                    dtCode.Rows.Add(dtRowMsg);

                                    DataTable dtMessage = new ClientProxy().ExecuteServiceSync("DA_SEL_MESSAGE_LOT_START_RP", "INDATA", "RSLTDT", dtCode);

                                    if (dtMessage == null || dtMessage.Rows.Count == 0)
                                    {
                                        Util.MessageValidation("SFU1392");  //MESSAGE 테이블에 메세지를 등록해주세요
                                        return;
                                    }

                                    if (string.Equals(dtInfo.Rows[0]["MSGCODE"], "95000")) // 해당BIZ에서 95000번을 샘플링으로 리턴
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2960", new object[] { Convert.ToString(dtMessage.Rows[0]["MSGNAME"]) })
                                            , true, true, ObjectDic.Instance.GetObjectName("바코드발행"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Barresult, isBarCode) =>
                                            {
                                                if (isBarCode == true)
                                                {
                                                    if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                                                    {
                                                        Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                                                    return;
                                                    }

                                                #region [샘플링 출하거래처 추가]
                                                // SAMPLING용 발행 매수 추가
                                                int iSamplingCount;

                                                    DataTable LabelDT = new DataTable();
                                                    LabelDT = (dgLotInfo.ItemsSource as DataView).Table;

                                                    DataTable sampleDT = new DataTable();
                                                    sampleDT.Columns.Add("CUT_ID", typeof(string));
                                                    sampleDT.Columns.Add("LOTID", typeof(string));
                                                    sampleDT.Columns.Add("COMPANY", typeof(string));
                                                    DataRow dRow = null;

                                                    foreach (DataRow _iRow in LabelDT.Rows)
                                                    {
                                                        iSamplingCount = 0;
                                                        string[] sCompany = null;
                                                        foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                                        {
                                                            iSamplingCount = Util.NVC_Int(items.Key);
                                                            sCompany = Util.NVC(items.Value).Split(',');
                                                        }
                                                        for (int i = 0; i < iSamplingCount; i++)
                                                        {
                                                            dRow = sampleDT.NewRow();
                                                            dRow["CUT_ID"] = _iRow["CUT_ID"];
                                                            dRow["LOTID"] = _iRow["LOTID"];
                                                            dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                                                            sampleDT.Rows.Add(dRow);
                                                        }
                                                    }
                                                    var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();
                                                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                                        for (int i = 0; i < sortdt.Rows.Count; i++)
                                                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), Process.SLITTING, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                                                #endregion
                                            }
                                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                        });
                                    }
                                    else
                                    {
                                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                    }
                                }
                                else
                                {
                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                }
                            }
                            else
                            {
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            }
                            #endregion

                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (mResult) =>
                            {
                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            });
                            #endregion
                        }, inDataSet);


                    }
                }
            });
        }

        private void fnSlitterLotStartBobbinBind()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                #region MESSAGE SET

                #region IN_EQP :     BR_PRD_REG_START_LOT_SL_USING_CST 사용하려 작성
                DataTable IN_EQP = inDataSet.Tables.Add("IN_EQP");
                IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                IN_EQP.Columns.Add("IFMODE", typeof(string));
                IN_EQP.Columns.Add("EQPTID", typeof(string));
                IN_EQP.Columns.Add("LANE_QTY", typeof(Int32));
                IN_EQP.Columns.Add("USERID", typeof(string));

                DataRow inLotDataRow = null;
                inLotDataRow = IN_EQP.NewRow();
                inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inLotDataRow["EQPTID"] = _EQPTID;
                inLotDataRow["LANE_QTY"] = Convert.ToInt32(_LANEQTY);
                inLotDataRow["USERID"] = LoginInfo.USERID;

                IN_EQP.Rows.Add(inLotDataRow);
                #endregion

                #region  IN_INPUT :     BR_PRD_REG_START_LOT_SL_USING_CST 사용하려 작성
                DataTable IN_INPUT = inDataSet.Tables.Add("IN_INPUT");
                IN_INPUT.Columns.Add("CSTID", typeof(string));
                IN_INPUT.Columns.Add("MTRLID", typeof(string));
                IN_INPUT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                IN_INPUT.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                DataRow inMtrlDataRow = null;
                inMtrlDataRow = IN_INPUT.NewRow();
                inMtrlDataRow["CSTID"] = _ELEC_CSTID;
                inMtrlDataRow["MTRLID"] = _LOTID;
                inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = GetEqptCurrentMtrl(_EQPTID);
                inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                IN_INPUT.Rows.Add(inMtrlDataRow);
                #endregion

                #region  IN_OUT :     BR_PRD_REG_START_LOT_SL_USING_CST 사용하려 작성
                DataTable IN_OUT = inDataSet.Tables.Add("IN_OUT");
                IN_OUT.Columns.Add("LANE_NO", typeof(string));
                IN_OUT.Columns.Add("OUT_CSTID", typeof(string));

                DataRow IN_OUT_Carrier_Top_DataRow = null;
                IN_OUT_Carrier_Top_DataRow = IN_OUT.NewRow();
                IN_OUT_Carrier_Top_DataRow["LANE_NO"] = "1";
                IN_OUT_Carrier_Top_DataRow["OUT_CSTID"] = txtBobbinT.Text.Trim();

                IN_OUT.Rows.Add(IN_OUT_Carrier_Top_DataRow);


                DataRow IN_OUT_Carrier_BOTTOM_DataRow = null;
                IN_OUT_Carrier_BOTTOM_DataRow = IN_OUT.NewRow();
                IN_OUT_Carrier_BOTTOM_DataRow["LANE_NO"] = "2";
                IN_OUT_Carrier_BOTTOM_DataRow["OUT_CSTID"] = txtBobbinB.Text.Trim();

                IN_OUT.Rows.Add(IN_OUT_Carrier_BOTTOM_DataRow);
                #endregion

                #region INDATA :     BR_PRD_REG_START_LOT_SL_USING_CST 사용하려 작성
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                //INDATA.Columns.Add("CUT_ID", typeof(string));
                INDATA.Columns.Add("CHILD_GR_SEQNO", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;
                inDataRow = INDATA.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["LOTID"] = _LOTID;
                //inDataRow["CUT_ID"] =CUT_ID;
                inDataRow["CHILD_GR_SEQNO"] = _LANEQTY;
                inDataRow["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(inDataRow);
                #endregion

                #endregion



                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_SL_USING_CST", "IN_EQP,IN_INPUT,IN_OUT,INDATA", "OUT_LOT", (result, ex) =>
                {
                    #region 공정UI LOGIC : BR_PRD_REG_START_LOT_SL
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    #region QC 샘플 PopUp
                    DataTable dtCutTable = result.Tables["OUT_LOT"];

                    string strCutid = dtCutTable.Rows[0]["CUT_ID"].ToString();

                    //2020.12.23 Slitter QC 샘플 화면 POPUP 
                    //PROCESSEQUIPMENTSEGMENT.GQMS_SMPLG_POPUP_APPLY_FLAG 값이 'Y'이면 실행
                    DataTable InTable = new DataTable();
                    InTable.Columns.Add("PROCID", typeof(string));
                    InTable.Columns.Add("EQSGID", typeof(string));

                    DataRow dtRow = InTable.NewRow();
                    dtRow["PROCID"] = Process.SLITTING;
                    dtRow["EQSGID"] = _EQPTID;
                    InTable.Rows.Add(dtRow);

                    DataTable dtProcEqst = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", InTable);

                    if (dtProcEqst != null && dtProcEqst.Rows.Count > 0 &&
                        Convert.ToString(dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"]) == "Y")
                    {
                        DataSet inDataSet2 = new DataSet();

                        //Slitter QC 샘플 POPUP 실행 비즈 호출
                        DataTable InEqpTable = inDataSet2.Tables.Add("IN_EQP");
                        InEqpTable.Columns.Add("SRCTYPE", typeof(string));
                        InEqpTable.Columns.Add("IFMODE", typeof(string));
                        InEqpTable.Columns.Add("EQPTID", typeof(string));
                        InEqpTable.Columns.Add("USERID", typeof(string));
                        InEqpTable.Columns.Add("SMPL_REG_YN", typeof(string));

                        DataRow drEqp = null;
                        drEqp = InEqpTable.NewRow();
                        drEqp["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        drEqp["IFMODE"] = IFMODE.IFMODE_OFF;
                        drEqp["EQPTID"] = _EQPTID;
                        drEqp["USERID"] = LoginInfo.USERID;
                        drEqp["SMPL_REG_YN"] = dtProcEqst.Rows[0]["GQMS_SMPLG_POPUP_APPLY_FLAG"].ToString();
                        InEqpTable.Rows.Add(drEqp);

                        DataTable dtLotTable = inDataSet2.Tables.Add("IN_LOT");
                        dtLotTable.Columns.Add("LOTID", typeof(string));

                        DataRow drLot = null;
                        drLot = dtLotTable.NewRow();
                        drLot["LOTID"] = strCutid;

                        dtLotTable.Rows.Add(drLot);

                        DataSet dsInfoRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHK_LOT_SAMPLING_SL", "IN_EQP,IN_LOT", "OUTDATA", inDataSet2);

                        DataTable dtInfo = dsInfoRslt.Tables["OUTDATA"];

                        if (dtInfo.Rows[0]["MSGTYPE"].Equals("INFO"))
                        {
                            //Message 등록 여부 확인
                            DataTable dtCode = new DataTable();
                            dtCode.Columns.Add("LANGID", typeof(string));
                            dtCode.Columns.Add("MSGID", typeof(string));

                            DataRow dtRowMsg = dtCode.NewRow();
                            dtRowMsg["LANGID"] = LoginInfo.LANGID;
                            dtRowMsg["MSGID"] = "1" + dtInfo.Rows[0]["MSGCODE"].ToString(); //기존 메시지 변경 불가로 신규 메시지로 처리

                            dtCode.Rows.Add(dtRowMsg);

                            DataTable dtMessage = new ClientProxy().ExecuteServiceSync("DA_SEL_MESSAGE_LOT_START_RP", "INDATA", "RSLTDT", dtCode);

                            if (dtMessage == null || dtMessage.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU1392");  //MESSAGE 테이블에 메세지를 등록해주세요
                                return;
                            }

                            if (string.Equals(dtInfo.Rows[0]["MSGCODE"], "95000")) // 해당BIZ에서 95000번을 샘플링으로 리턴
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2960", new object[] { Convert.ToString(dtMessage.Rows[0]["MSGNAME"]) })
                                    , true, true, ObjectDic.Instance.GetObjectName("바코드발행"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Barresult, isBarCode) =>
                                    {
                                        if (isBarCode == true)
                                        {
                                            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                                            {
                                                Util.MessageValidation("SFU2003"); // 프린트 환경 설정값이 없습니다.
                                                        return;
                                            }

                                                    #region [샘플링 출하거래처 추가]
                                                    // SAMPLING용 발행 매수 추가
                                                    int iSamplingCount;

                                            DataTable LabelDT = new DataTable();
                                            LabelDT = (dgLotInfo.ItemsSource as DataView).Table;

                                            DataTable sampleDT = new DataTable();
                                            sampleDT.Columns.Add("CUT_ID", typeof(string));
                                            sampleDT.Columns.Add("LOTID", typeof(string));
                                            sampleDT.Columns.Add("COMPANY", typeof(string));
                                            DataRow dRow = null;

                                            foreach (DataRow _iRow in LabelDT.Rows)
                                            {
                                                iSamplingCount = 0;
                                                string[] sCompany = null;
                                                foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                                {
                                                    iSamplingCount = Util.NVC_Int(items.Key);
                                                    sCompany = Util.NVC(items.Value).Split(',');
                                                }
                                                for (int i = 0; i < iSamplingCount; i++)
                                                {
                                                    dRow = sampleDT.NewRow();
                                                    dRow["CUT_ID"] = _iRow["CUT_ID"];
                                                    dRow["LOTID"] = _iRow["LOTID"];
                                                    dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                                                    sampleDT.Rows.Add(dRow);
                                                }
                                            }
                                            var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();
                                            for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                                for (int i = 0; i < sortdt.Rows.Count; i++)
                                                    Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), Process.SLITTING, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                                                    #endregion
                                                }
                                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                            });
                            }
                            else
                            {
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            }
                        }
                        else
                        {
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        }
                    }
                    else
                    {
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    #endregion

                    //정상처리되었습니다.
                    Util.MessageInfo("SFU1275", (mResult) =>
                    {
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    });
                    #endregion
                }, inDataSet);



            }

            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }



        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;

            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                if (dg != null)
                {

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LANE_QTY").ToString();
                _ELEC_CSTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CSTID").ToString();
                txtLOTID.Text = _LOTID;
            }
        }

        private void txtLOTID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //LotStart(txtLOTID.Text);
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLOTID.Text) || dgLotInfo.GetRowCount() == 0)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                LotSelect();
            }
        }

        private void dgLotInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgLotInfo.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool GetLotInfo()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtEquipment.Text = dtMain.Rows[0]["EQPTNAME"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                    txtWOID.Text = dtMain.Rows[0]["WOID"].ToString();
                    GetLotList();
                    rslt = true;
                }
                else
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요
                    rslt = false;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                IndataTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = Util.NVC(txtWorkorder.Text);
                Indata["COATING_SIDE_TYPE_CODE"] = _COAT_SIDE_TYPE.ToString().Equals("") == true ? null : _COAT_SIDE_TYPE;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count <= 0 || dtMain == null)
                {
                    dgLotInfo.ItemsSource = null;
                    return;
                }

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);

                txtLOTID.Text = _RUNLOT;
                LotSelect();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LotSelect()
        {
            // 그리드에 일치하는 lot 자동선택
            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLOTID.Text + "'").FirstOrDefault());
            int cIdx = dgLotInfo.Columns["CHK"].Index;

            if (rIdx >= 0)
            {
                dt.Rows[rIdx][cIdx] = true;
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                dgLotInfo.SelectedIndex = rIdx;
                dgLotInfo.UpdateLayout();
                dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LANE_QTY").ToString();
                _ELEC_CSTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "CSTID").ToString();
            }
        }

        string GetEqptCurrentMtrl(string sEqptID)
        {
            try
            {
                string MountMTRL = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                    return "";

                MountMTRL = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                return MountMTRL;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        // 샘플링 라벨발행 수량 / 출하처(자동차 롤프레스,슬리터)
        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            if (string.Equals(getQAInspectFlag(sLotID), "Y"))
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PROCID"] = Process.SLITTING;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };
            }

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private string getQAInspectFlag(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(result.Rows[0]["SLIT_QA_INSP_TRGT_FLAG"]);
            }
            catch (Exception ex) { }

            return "";
        }
        #endregion





        private void txtBobbinT_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBobbinT.Text) )
            {
                return;
            }

            #region txtBobbinT 와 txtBobbinB 비교
            if (txtBobbinT.Text.Length >= 1)
            {
                if (txtBobbinT.Text.Trim() == txtBobbinB.Text.Trim())
                {
                    object[] parameters = new object[1];
                    parameters[0] = "Carrier(B)";
                    MessageValidationWithCallBack("MMD0357", (result) =>
                    {
                        txtBobbinT.Focus();
                    }, parameters); //%1 와 값이 같습니다 다른 값을 입력하세요. }
                    return;
                }
            }
            #endregion


            if (e.Key == Key.Enter)
            {
                CheckCarrierInfo(txtBobbinT, txtBobbinB, TYPE.MAPPING);
            }
        }

        private void txtBobbinB_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBobbinB.Text))
            {
                return;
            }

            #region txtBobbinB 와 txtBobbinT 비교
            if (txtBobbinB.Text.Length >= 1)
            {
                if (txtBobbinB.Text.Trim() == txtBobbinT.Text.Trim())
                {
                    object[] parameters = new object[1];
                    parameters[0] = "Carrier(T)";
                    MessageValidationWithCallBack("MMD0357", (result) =>
                    {
                        txtBobbinB.Focus();
                    }, parameters); //%1 와 값이 같습니다 다른 값을 입력하세요. }
                    return;
                }
            }
            #endregion


            if (e.Key == Key.Enter)
            {
                CheckCarrierInfo(txtBobbinB, txtBobbinT, TYPE.MAPPING);
            }
        }




        private void txtBobbinT_TextChanged(object sender, TextChangedEventArgs e)
        {
            //
        }

        private void txtBobbinB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //
        }

        private string getCarrierUseYN()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLT_BOBBIN_BIND", "RQSTDT", "RSLTDT", inTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(result.Rows[0]["USE_YN"]);
            }
            catch (Exception ex)
            {
                return "";
            }

            return "";
        }


        // 2025-07-13 양직일
        // 조회시 화면에 인디게이터 출력
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }


        // 2025-07-13 양직일
        // 조회시 화면에 인디게이터 숨기기
        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        //
        // 2025-07-13 양직일
        // 메시지 생성 및 출력
        //
        public static void MessageValidationWithCallBack(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }


        //  2025-07-12 양직일
        //  Carrier 투입가능여부를 확인하고 투입 불가한 경우 메시지를 출력한다.
        //

        private void CheckCarrierInfo(TextBox txtBobT, TextBox txtBobB, TYPE _type)
        {
            try
            {
                ShowLoadingIndicator();
  
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = txtBobT.Text.Trim();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //캐리어 존재여부
                        if (searchResult.Rows.Count == 0)
                        {
                            object[] parameters = new object[1];
                            parameters[0] = txtBobT.Text.Trim();
                            MessageValidationWithCallBack("SFU7001", (result) =>
                            {
                                txtBobT.Focus();
                            }, parameters); //CSTID[%1]에 해당하는 CST가 없습니다.

                            return;
                        }


                        if (Util.NVC(searchResult.Rows[0]["CSTTYPE"]) != "BB" )
                        {
                            object[] parameters = new object[2];
                            parameters[0] = "CSTTYPE ";
                            parameters[1] = "PANCAKE BOBBIN";
                            MessageValidationWithCallBack("617", (result) =>
                            {
                                txtBobT.Focus();
                            }, parameters); //해당 Carrier는 정상입니다.

                            return;
                        }
                        //캐리어 상태
                        switch (_type)
                        {
                            case TYPE.MAPPING:
                                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("U"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU7002", (result) =>
                                    {
                                        txtBobT.Focus();
                                    }, parameters); //CSTID[%1] 이 상태가 %2 입니다.

                                    return;
                                }
                                break;

                            case TYPE.EMPTY:
                                if (Util.NVC(searchResult.Rows[0]["CSTSTAT"]).Equals("E"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU7002", (result) =>
                                    {
                                        txtBobT.Focus();
                                    }, parameters); //CSTID[%1] 이 상태가 %2 입니다.

                                    return;
                                }
                                break;

                            //C20211002-000028
                            case TYPE.LOCK:
                                if (!Util.NVC(searchResult.Rows[0]["MAPP_ERR_OCCR_FLAG"]).Equals("Y"))
                                {
                                    object[] parameters = new object[2];
                                    parameters[0] = Util.NVC(searchResult.Rows[0]["CSTID"]);
                                    parameters[1] = Util.NVC(searchResult.Rows[0]["CSTSNAME"]);
                                    MessageValidationWithCallBack("SFU8433", (result) =>
                                    {
                                        txtBobT.Focus();

                                    }, parameters); //해당 Carrier는 정상입니다.

                                    return;
                                }
                                break;
                        }

 
                        txtBobB.Focus();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
    }
}