/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.08.05   김도형   : [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_036 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        public string sTempLot_1 = string.Empty;
        public string sTempLot_2 = string.Empty;
        public string sNote2 = string.Empty;
        private string _APPRV_PASS_NO = string.Empty;

        public Boolean bReprint = true;

        private BOX001_036_PACKINGCARD_MERGE window01 = new BOX001_036_PACKINGCARD_MERGE();

        public bool bNew_Load = false;
        public bool bCancel = false;
        public DataTable dtPackingCard;

        private string _TabOut_Hist_View_Yn = string.Empty; // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
        private string _Tab_detail_View_Yn = string.Empty; //  [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)

        public BOX001_036()
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

            SetEvent();

            txtLotID.Focus();
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

            // 포장출고탭 
            _combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");
            // 출고이력탭   // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
            _combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.ALL, sFilter: sFilter);
            _combo.SetCombo(cboStatus2, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");

            // 상세조회탭
            _combo.SetCombo(cboTransLoc7, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "TRANSLOC");
            _combo.SetCombo(cboStatus7, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");
            #endregion


            // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
            SetElecRollPackingHistTapViewYn();  // ELEC_ROLL_PACKING_HIST_TAP_VIEW_YN 전극 ROLL Packing 이력탭 표시여부

            if (!_TabOut_Hist_View_Yn.Equals("Y"))   // 출고이력탭 
            {
                TabOUT_Hist.Visibility = Visibility.Collapsed;  
            }
            if (!_Tab_detail_View_Yn.Equals("Y"))  // 상세이력
            { 
                Tab_Detail.Visibility = Visibility.Collapsed;
            }

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Event

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            sTempLot_1 = string.Empty;
            sTempLot_2 = string.Empty;
            sNote2 = string.Empty;
            Util.gridClear(dgOut);
            dgSub.Children.Clear();

            dgOut.IsEnabled = true;
            txtLotID.IsReadOnly = false;
            btnPackOut.IsEnabled = true;
            txtLotID.Text = "";

            txtCARRIERID.IsReadOnly = false;
            txtCARRIERID.Text = string.Empty;

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

                string sLot_Type = "ROLL";

                txtLotID.IsReadOnly = true;
                txtCARRIERID.IsReadOnly = true;
                btnPackOut.IsEnabled = false;

                dgOut.IsEnabled = false;               

                if (sLot_Type == "ROLL")
                {
                    string sTempProdName_1 = string.Empty;
                    string sTempProdName_2 = string.Empty;
                    string sPackingLotType1 = string.Empty;
                    string sPackingLotType2 = string.Empty;
                    string sLotID = string.Empty;
                    string sLotID2 = string.Empty;

                    bReprint = false;

                    sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                    sLotID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "LOTID"));

                    if (dgOut.GetRowCount() > 2)
                    {
                        Util.MessageValidation("SFU3570"); //최대 2개 LOT까지 포장가능합니다.
                        return;
                    }
                        sTempLot_1 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "LOTID"));
                    if(dgOut.GetRowCount() == 2)
                        sTempLot_2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[1].DataItem, "LOTID"));

                    loadingIndicator.Visibility = Visibility.Visible;
                    Get_Sub_Merge();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    txtLotID.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                window01.BOX001_036 = this;
                window01.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub.Children.Add(window01);
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
                    string sLotID = string.Empty;
                    string sLotID2 = string.Empty;
                    sLot_Type = "ROLL";
                    
                    if (txtLotID.Text.ToString() == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }
                    if (sLot_Type == "ROLL")
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

                        int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());

                        if (iCnt <= 0)
                        {
                            Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                            return;
                        }

                        // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("LOTID", typeof(String));
                        RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["LOTID"] = sLotid;
                        dr1["WIPSTAT"] = "WAIT";

                        RQSTDT1.Rows.Add(dr1);

                        DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_ROLL", "RQSTDT", "RSLTDT", RQSTDT1);
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
                            DataTable dtChk = new DataTable();
                            dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                            DataRow drChk = dtChk.NewRow();
                            drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

                            dtChk.Rows.Add(drChk);

                            DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);

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
                                row["BR_TYPE"] = "P_ROLL";                  //OLD BR Search 변수

                                indataSet.Tables["INDATA"].Rows.Add(row);
                                loadingIndicator.Visibility = Visibility.Visible;

                                //BR_PRD_CHK_QMS_FOR_PACKING_DETAIL_ROLL -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                                //신규 HOLD 적용을 위해 변경 작업
                                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    if (Exception != null)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                        return;
                                    }

                                    Search_LotID(sLotid);

                                    txtLotID.SelectAll();
                                    txtLotID.Focus();

                                }, indataSet);
                            }
                            else
                            {
                                // 품질결과 Skip
                                Search_LotID(sLotid);

                                txtLotID.SelectAll();
                                txtLotID.Focus();
                            }
                        }
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_LotID(sLotid);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
            }
        }

        private void Search_LotID(string sLotid)
        {
            try
            {
                string sLotID = string.Empty;

                // 재공정보 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = sLotid;

                RQSTDT2.Rows.Add(dr2);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LIST_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT2);

                if (Lot_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    Util.GridSetData(dgOut, Lot_Result, FrameOperation);
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
                        #region # 시생산 Lot 
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE").ToString() != Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]))
                        {
                            if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE").ToString().Equals("X") || Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]).Equals("X"))
                            {
                                Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다.  
                                return;
                            }
                        }
                        #endregion
                    }

                    if (dgOut.Rows.Count >= 2)
                    {
                        Util.MessageValidation("SFU3570"); //최대 2개 LOT까지 포장가능합니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["PROCID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PROCID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    dgOut.IsReadOnly = false;
                    dgOut.BeginNewRow();
                    dgOut.EndNewRow(true);
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", Lot_Result.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", Lot_Result.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LANE_QTY", Lot_Result.Rows[0]["LANE_QTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", Lot_Result.Rows[0]["M_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", Lot_Result.Rows[0]["CELL_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", Lot_Result.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PROCID", Lot_Result.Rows[0]["PROCID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTTYPE", Lot_Result.Rows[0]["LOTTYPE"].ToString());
                    dgOut.IsReadOnly = true;
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
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
        #endregion

        private void txtCARRIERID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearhCarrierID(txtCARRIERID.Text);

                txtCARRIERID.Focus();
                txtCARRIERID.SelectAll();
            }
        }

        private void SearhCarrierID(string sCarrierID)
        {
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }
                else
                {
                    SearchLotID(dtLot.Rows[0]["LOTID"].ToString());
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchLotID(string sLotID)
        {
            try
            {
                string sLot_Type = string.Empty;
                sLot_Type = "ROLL";
                
                if (sLot_Type == "ROLL")
                {
                    // 출고 이력 조회
                    DataTable RQSTDT0 = new DataTable();
                    RQSTDT0.TableName = "RQSTDT";
                    RQSTDT0.Columns.Add("CSTID", typeof(String));
                    RQSTDT0.Columns.Add("AREAID", typeof(String));

                    DataRow dr0 = RQSTDT0.NewRow();
                    dr0["CSTID"] = sLotID;
                    dr0["AREAID"] = LoginInfo.CFG_AREA_ID;

                    RQSTDT0.Rows.Add(dr0);

                    DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                    int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());

                    if (iCnt <= 0)
                    {
                        Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                        return;
                    }

                    // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("LOTID", typeof(String));
                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LOTID"] = sLotID;
                    dr1["WIPSTAT"] = "WAIT";

                    RQSTDT1.Rows.Add(dr1);

                    DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_ROLL", "RQSTDT", "RSLTDT", RQSTDT1);
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
                        DataTable dtChk = new DataTable();
                        dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                        DataRow drChk = dtChk.NewRow();
                        drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

                        dtChk.Rows.Add(drChk);

                        DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);

                        if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                        {
                            // 품질결과 검사 체크
                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotID;
                            row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_ROLL";                  //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);
                            loadingIndicator.Visibility = Visibility.Visible;

                            //BR_PRD_CHK_QMS_FOR_PACKING_DETAIL_ROLL -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                    return;
                                }

                                Search_LotID(sLotID);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            // 품질결과 Skip
                            Search_LotID(sLotID);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else
                    {
                        Search_QMS_Validation(sLotID);
                        // 품질결과 Skip
                        Search_LotID(sLotID);

                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return;
            }
        }         

        // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
        private void SetElecRollPackingHistTapViewYn()
        {

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELEC_ROLL_PACKING_HIST_TAP_VIEW_YN";// 전극 ROLL Packing 이력탭 표시여부
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
                    _TabOut_Hist_View_Yn = Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());  // 출고이력탭 보기여부
                    _Tab_detail_View_Yn = Util.NVC(dtResult.Rows[0]["ATTR2"].ToString());   // 상세이력탭 보기여부
                }
                else
                {
                    _TabOut_Hist_View_Yn = "N";
                    _Tab_detail_View_Yn = "N";
                }

                return;
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
                _TabOut_Hist_View_Yn = "N";
                _Tab_detail_View_Yn = "N";
                return;
            }
        }
         
        // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
        #region 출고이력탭 (TabOUT_Hist) Start

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

        // 포장카드 재발행
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

                /* 발행로직 백업
                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();

                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["RCV_ISS_ID"] = sRcv_Iss_Id;

                RQSTDT3.Rows.Add(dr3);

                DataTable ReprintResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_INNER_BOX", "RQSTDT", "RSLTDT", RQSTDT3);

                if (ReprintResult2.Rows.Count == 3)
                    SteelCaseReprint();
                else
                    NormalReprint();
                */

                if (string.Equals(drChk[0]["SHIPTO_ID"], "A0081"))
                {
                    SteelCaseReprint2();
                }
                else
                {
                    NormalReprint();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // 출고취소
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

        // 조회
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
                dr["OUTER_BOXID"] = txtBoxid.Text.Trim() == "" ? null : txtBoxid.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID.Text.Trim() == "" ? null : txtProd_ID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_MASTER", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SteelCaseReprint2()
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();
                string sOuter_Boxid = drChk[0]["OUTER_BOXID"].ToString();

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

                string sEltrValueDecimalApplyFlag = string.Empty;
                sEltrValueDecimalApplyFlag = GetEltrValueDecimalApplyFlag(); //[E20230306-000128]전극수 소수점 개선 

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

                DataTable dtLotList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CONV_QTY_LOT", "RQSTDT", "RSLTDT", RQSTLotList, RowSequenceNo: true);

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
                dtReprint.Columns.Add("V", typeof(string));
                dtReprint.Columns.Add("L", typeof(string));
                dtReprint.Columns.Add("M", typeof(string));
                dtReprint.Columns.Add("C", typeof(string));
                dtReprint.Columns.Add("D", typeof(string));
                dtReprint.Columns.Add("REMARK", typeof(string));
                dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                dtReprint.Columns.Add("V_DATE1", typeof(string));
                dtReprint.Columns.Add("P_DATE1", typeof(string));
                dtReprint.Columns.Add("V_DATE2", typeof(string));
                dtReprint.Columns.Add("P_DATE2", typeof(string));
                dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                dtReprint.Columns.Add("PRODID", typeof(string));

                DataRow drCrad = dtReprint.NewRow();

                if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드"),
                                                  ReprintResult.Rows[0]["MODLID"].ToString(),
                                                  ReprintResult.Rows[0]["OUTER_BOXID"].ToString(),
                                                  "*" + ReprintResult.Rows[0]["OUTER_BOXID"].ToString() + "*",
                                                  ReprintResult.Rows[0]["FROM_AREA"].ToString() + " -> " + ReprintResult.Rows[0]["SHIPTO_NAME"].ToString(),
                                                  (decimal)(Math.Truncate( TotalCRoll * 10) / 10) ,
                                                  (decimal)(Math.Truncate( TotalSRoll * 10) / 10) ,
                                                  ValidateDate,
                                                  WipDate,
                                                  ReprintResult.Rows[0]["PROD_VER_CODE"].ToString(),
                                                  TotalLane,
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalCRoll * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalSRoll * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalM * 10) / 10) ),
                                                  Util.NVC(ReprintResult.Rows[0]["ISS_NOTE"]),
                                                  ReprintResult.Rows[0]["UNIT_CODE"].ToString(),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  Util.NVC(ReprintResult.Rows[0]["OFFER_SHEET_DESCRIPTION"]),
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ "
                                                     + ReprintResult.Rows[0]["PRDT_ABBR_CODE"].ToString() + " ]",
                                               };
                }
                else
                {
                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드"),
                                                  ReprintResult.Rows[0]["MODLID"].ToString(),
                                                  ReprintResult.Rows[0]["OUTER_BOXID"].ToString(),
                                                  "*" + ReprintResult.Rows[0]["OUTER_BOXID"].ToString() + "*",
                                                  ReprintResult.Rows[0]["FROM_AREA"].ToString() + " -> " + ReprintResult.Rows[0]["SHIPTO_NAME"].ToString(),
                                                  TotalCRoll,
                                                  TotalSRoll,
                                                  ValidateDate,
                                                  WipDate,
                                                  ReprintResult.Rows[0]["PROD_VER_CODE"].ToString(),
                                                  TotalLane,
                                                  String.Format("{0:#,##0}", TotalCRoll),
                                                  String.Format("{0:#,##0}", TotalSRoll),
                                                  String.Format("{0:#,##0}", TotalM),
                                                  Util.NVC(ReprintResult.Rows[0]["ISS_NOTE"]),
                                                  ReprintResult.Rows[0]["UNIT_CODE"].ToString(),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  ObjectDic.Instance.GetObjectName("유효일자"),
                                                  ObjectDic.Instance.GetObjectName("생산일자"),
                                                  Util.NVC(ReprintResult.Rows[0]["OFFER_SHEET_DESCRIPTION"]),
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ "
                                                     + ReprintResult.Rows[0]["PRDT_ABBR_CODE"].ToString() + " ]",
                                               };
                }
                dtReprint.Rows.Add(drCrad);

                LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    //태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[3];
                    Parameters[0] = "PackingCard_New_Four";
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

        private void NormalReprint()
        {
            try
            {
                string sType = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgOutHIst, "CHK");

                string sRcv_Iss_Id = drChk[0]["RCV_ISS_ID"].ToString();
                string sOuter_Boxid = drChk[0]["OUTER_BOXID"].ToString();
                string sOwms_box_type = drChk[0]["OWMS_BOX_TYPE_CODE"].ToString();

                if (sOwms_box_type.Equals("ER") && LoginInfo.CFG_SHOP_ID.Equals("A011"))
                {
                    Util.MessageValidation("SFU8021"); //SteelRack 포장카드는 전극 포장 및 출고(Steel Rack) 출고이력에서 재발행 해주세요.
                    return;
                }

                string sEltrValueDecimalApplyFlag = string.Empty;
                sEltrValueDecimalApplyFlag = GetEltrValueDecimalApplyFlag(); //[E20230306-000128]전극수 소수점 개선 

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

                // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
                // DA_PRD_SEL_PACKCARD_REPRINT -> DA_PRD_SEL_PACKCARD_REPRINT_NEW 변경 
                // 개발 서버에 DA 중복 존재 NEW로 신규 개발
                DataTable ReprintResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_NEW", "RQSTDT", "RSLTDT", RQSTDT);

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
                else if (CutidResult.Rows.Count == 1)
                {
                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("OUTER_BOXID", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["OUTER_BOXID"] = sOuter_Boxid;

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
                }
                else
                {
                    sType = "ONE";
                }

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
                string sAbbrCode = string.Empty;
                string sLotid3 = string.Empty;
                string sLotid4 = string.Empty;

                // 환산자 처리
                decimal iConvSum = 0;
                decimal iConvSum2 = 0;
                decimal iConvSum3 = 0;
                decimal iConvSum4 = 0;

                string sTotal_M = string.Empty;
                string sTotal_C = string.Empty;
                string sTotal_M2 = string.Empty;
                string sTotal_C2 = string.Empty;
                string sTotal_C3 = string.Empty;
                string sM1 = string.Empty;
                string sC1 = string.Empty;
                string sM2 = string.Empty;
                string sC2 = string.Empty;

                string sD1 = string.Empty;
                string sD2 = string.Empty;

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");
                string sBoxtype = string.Empty;
                string sPKG_DATE = string.Empty;

                DateTime ProdDate;
                if (ReprintResult.Rows[0]["BOXTYPE"].ToString() == "CRT")
                {
                    sBoxtype = ReprintResult.Rows.Count + ObjectDic.Instance.GetObjectName("가대");
                }
                else
                {
                    sBoxtype = "BOX";
                }

                string sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드") + " " + sBoxtype;
                String pan_gr_id = string.Empty;
               
                for (int i = 0; i < CutidResult.Rows.Count; i++)
                {
                    if (i == 0)
                        sLotid = CutidResult.Rows[i]["CSTID"].ToString();
                    if (!sLotid.Equals(CutidResult.Rows[i]["CSTID"].ToString()))
                    {
                        sLotid2 = CutidResult.Rows[i]["CSTID"].ToString();
                    }
                    pan_gr_id += CutidResult.Rows[i]["CSTID"].ToString() + ","; 
                }
                String procRemark = string.Empty;

                String chk_lotid = string.Empty;

                for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                {
                    chk_lotid += Util.NVC(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "LOTID")) + ",";
                }

                string sHLotHeatAreaRouteTargetCheck = string.Empty;
                sHLotHeatAreaRouteTargetCheck = GetLotHeatAreaRouteTargetCheck(chk_lotid);  // 열처리 체크 대상여부 (LOTID 기준)

                if (sHLotHeatAreaRouteTargetCheck.Equals("Y"))
                {
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
                }
                else
                {
                    procRemark = "";
                }
                if (sLotid.Equals(""))
                {

                    DataTable RQSTDT3 = new DataTable();
                    RQSTDT3.TableName = "RQSTDT";
                    RQSTDT3.Columns.Add("LANGID", typeof(String));
                    RQSTDT3.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));

                    DataRow dr3 = RQSTDT3.NewRow();
                    dr3["LANGID"] = LoginInfo.LANGID;
                    dr3["RCV_ISS_ID"] = sRcv_Iss_Id;
                    dr3["CMCDTYPE"] = "PRDT_ABBR_CODE";

                    RQSTDT3.Rows.Add(dr3);

                    DataTable ReprintResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_ROLL", "RQSTDT", "RSLTDT", RQSTDT3);

                    if (ReprintResult2.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                        return;
                    }

                    for (int i = 0; i < ReprintResult2.Rows.Count; i++)
                    {
                        if (!ReprintResult2.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                        {
                            ProdDate = Convert.ToDateTime(ReprintResult2.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                        }
                        if (i == 0)
                            sLotid3 = ReprintResult2.Rows[i]["LOTID"].ToString();
                        if (!sLotid3.Equals(ReprintResult2.Rows[i]["LOTID"].ToString()))
                        {
                            sLotid4 = ReprintResult2.Rows[i]["LOTID"].ToString();
                            if (!ReprintResult2.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                            {
                                ProdDate = Convert.ToDateTime(ReprintResult2.Rows[i]["WIPSDTTM"].ToString());
                                sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                            }
                        }
                    }
                }

                if (sType == "ONE")
                {
                    for (int i = 0; i < ReprintResult.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }
                            if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                            {
                                ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                                sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                            }

                            sPackingNo = ReprintResult.Rows[i]["OUTER_BOXID"].ToString();
                            sModlid = ReprintResult.Rows[i]["MODLID"].ToString();
                            sFrom = ReprintResult.Rows[i]["FROM_AREA"].ToString();
                            sTo = ReprintResult.Rows[i]["SHIPTO_NAME"].ToString();
                            iSum = Convert.ToDouble(CutidResult.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(CutidResult.Rows[i]["TOTALQTY2"].ToString());
                            sVer = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = ReprintResult.Rows[i]["LANE"].ToString();

                            //sLotid3 = ReprintResult.Rows[i]["LOTID"].ToString();
                            //sLotid4 = ReprintResult.Rows[i]["LOTID"].ToString();
                            sUnitCode = ReprintResult.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = ReprintResult.Rows[i]["PRDT_ABBR_CODE"].ToString();
                            sPKG_DATE = Convert.ToDateTime(ReprintResult.Rows[i]["PACKDTTM"]).ToString("yyyy-MM-dd");

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                                {
                                    iConvSum = Util.NVC_Decimal(iSum) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer));
                                    iConvSum3 = Util.NVC_Decimal(iSum3) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer));
                                }
                                else
                                {
                                    iConvSum = Math.Floor(Util.NVC_Decimal(iSum) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer)));
                                    iConvSum3 = Math.Floor(Util.NVC_Decimal(iSum3) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer)));
                                    //GetPatternLengthVer(Util.NVC(CutidResult.Rows[i]["PRODID"]), Util.NVC(CutidResult.Rows[i]["PROD_VER_CODE"])) // 추후 적용
                                }
                            }

                            if (ReprintResult.Rows[i]["ISS_NOTE"].ToString() == "")
                            {
                                sNote = null;
                            }
                            else
                            {
                                sNote = ReprintResult.Rows[i]["ISS_NOTE"].ToString();
                            }

                            if (ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                            {
                                sOper_Desc = null;
                            }
                            else
                            {
                                sOper_Desc = ReprintResult.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            }
                        }
                        else
                        {
                            if (CutidResult.Rows.Count == 2)
                            {
                                if (ReprintResult.Rows[i]["VLD_DATE"].ToString() == "")
                                {
                                    sVld2 = null;
                                }
                                else
                                {
                                    string sVld_date = ReprintResult.Rows[i]["VLD_DATE"].ToString();
                                    sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                                }

                                if (!ReprintResult.Rows[i]["WIPSDTTM"].ToString().Equals(""))
                                {
                                    ProdDate = Convert.ToDateTime(ReprintResult.Rows[i]["WIPSDTTM"].ToString());
                                    sProdDate2 = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                                }

                                iSum2 = Convert.ToDouble(CutidResult.Rows[i]["TOTALQTY"].ToString());
                                iSum4 = Convert.ToDouble(CutidResult.Rows[i]["TOTALQTY2"].ToString());
                                sVer2 = ReprintResult.Rows[i]["PROD_VER_CODE"].ToString();
                                sLane2 = ReprintResult.Rows[i]["LANE"].ToString();
                                //sLotid2 = CutidResult.Rows[1]["CSTID"].ToString();
                                if (string.Equals(sUnitCode, "EA"))
                                {
                                    if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                                    {
                                        iConvSum2 = Util.NVC_Decimal(iSum2) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer2));
                                        iConvSum4 = Util.NVC_Decimal(iSum4) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer2));
                                    }
                                    else
                                    {
                                        iConvSum2 = Math.Floor(Util.NVC_Decimal(iSum2) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer2)));
                                        iConvSum4 = Math.Floor(Util.NVC_Decimal(iSum4) * Util.NVC_Decimal(GetPatternLength(Util.NVC(CutidResult.Rows[i]["PRODID"]), sVer2)));
                                        //GetPatternLengthVer(Util.NVC(CutidResult.Rows[i]["PRODID"]), Util.NVC(CutidResult.Rows[i]["PROD_VER_CODE"]))
                                    }
                                }
                            }
                        }
                    }



                    if (CutidResult.Rows.Count == 1)
                    {
                        if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                        {
                            if (string.Equals(sUnitCode, "EA"))
                            {
                                double total_M = iSum;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum * 10) / 10)) + "M";

                                double total_C = iSum3;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum3 * 10) / 10)) + "M";
                            }
                            else
                            {
                                double total_M = iSum;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10));

                                double total_C = iSum3;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10));
                            }

                            sM1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum * 10) / 10));
                            sM2 = "";
                            sC1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));
                            sC2 = "";

                            if (string.Equals(sUnitCode, "EA"))
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum3 * 10) / 10));
                            else
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));

                            sD2 = "";
                        }
                        else
                        {
                            if (string.Equals(sUnitCode, "EA"))
                            {
                                double total_M = Math.Floor(iSum);
                                sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", iConvSum) + "M";

                                double total_C = Math.Floor(iSum3);
                                sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", iConvSum3) + "M";
                            }
                            else
                            {
                                double total_M = Math.Floor(iSum);
                                sTotal_M = String.Format("{0:#,##0}", total_M);

                                double total_C = Math.Floor(iSum3);
                                sTotal_C = String.Format("{0:#,##0}", total_C);
                            }

                            sM1 = String.Format("{0:#,##0}", Math.Floor(iSum));
                            sM2 = "";
                            sC1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                            sC2 = "";

                            if (string.Equals(sUnitCode, "EA"))
                                sD1 = String.Format("{0:#,##0}", iConvSum3);
                            else
                                sD1 = String.Format("{0:#,##0}", Math.Floor(iSum3));

                            sD2 = "";
                        }
                    }
                    else
                    {
                        if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                        {
                            if (string.Equals(sUnitCode, "EA"))
                            {
                                double total_M = iSum + iSum2;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((iConvSum + iConvSum2) * 10) / 10)) + "M";

                                double total_C = iSum3 + iSum4;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((iConvSum3 + iConvSum4) * 10) / 10)) + "M";
                            }
                            else
                            {
                                double total_M = iSum + iSum2;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10));

                                double total_C = iSum3 + iSum4;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10));
                            }

                            sM1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum * 10) / 10));
                            sM2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum2 * 10) / 10));
                            sC1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));
                            sC2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum4 * 10) / 10));

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum3 * 10) / 10));
                                sD2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum4 * 10) / 10));
                            }
                            else
                            {
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));
                                sD2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum4 * 10) / 10));
                            }
                        }
                        else
                        {
                            if (string.Equals(sUnitCode, "EA"))
                            {
                                double total_M = Math.Floor(iSum) + Math.Floor(iSum2);
                                sTotal_M = String.Format("{0:#,##0}", total_M) + "/" + String.Format("{0:#,##0}", (iConvSum + iConvSum2)) + "M";

                                double total_C = Math.Floor(iSum3) + Math.Floor(iSum4);
                                sTotal_C = String.Format("{0:#,##0}", total_C) + "/" + String.Format("{0:#,##0}", (iConvSum3 + iConvSum4)) + "M";
                            }
                            else
                            {
                                double total_M = Math.Floor(iSum) + Math.Floor(iSum2);
                                sTotal_M = String.Format("{0:#,##0}", total_M);

                                double total_C = Math.Floor(iSum3) + Math.Floor(iSum4);
                                sTotal_C = String.Format("{0:#,##0}", total_C);
                            }

                            sM1 = String.Format("{0:#,##0}", Math.Floor(iSum));
                            sM2 = String.Format("{0:#,##0}", Math.Floor(iSum2));
                            sC1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                            sC2 = String.Format("{0:#,##0}", Math.Floor(iSum4));

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                sD1 = String.Format("{0:#,##0}", iConvSum3);
                                sD2 = String.Format("{0:#,##0}", iConvSum4);
                            }
                            else
                            {
                                sD1 = String.Format("{0:#,##0}", Math.Floor(iSum3));
                                sD2 = String.Format("{0:#,##0}", Math.Floor(iSum4));
                            }
                        }
                    }

                    DataTable dtReprint = new DataTable();
                    DataRow drCrad = null;

                    if (sLotid.Equals(""))
                    {

                        dtReprint.Columns.Add("Title", typeof(string));
                        dtReprint.Columns.Add("MODEL_NAME", typeof(string));
                        dtReprint.Columns.Add("PACK_NO", typeof(string));
                        dtReprint.Columns.Add("HEAD_BARCODE", typeof(string));
                        dtReprint.Columns.Add("Transfer", typeof(string));
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
                        dtReprint.Columns.Add("C1", typeof(string));
                        dtReprint.Columns.Add("C2", typeof(string));
                        dtReprint.Columns.Add("D1", typeof(string));
                        dtReprint.Columns.Add("D2", typeof(string));
                        dtReprint.Columns.Add("REMARK", typeof(string));
                        dtReprint.Columns.Add("UNIT_CODE", typeof(string));
                        dtReprint.Columns.Add("V_DATE", typeof(string));
                        dtReprint.Columns.Add("P_DATE", typeof(string));
                        dtReprint.Columns.Add("OFFER_DESC", typeof(string));
                        dtReprint.Columns.Add("PRODID", typeof(string));
                        dtReprint.Columns.Add("Text5", typeof(string));
                        dtReprint.Columns.Add("Text13", typeof(string));
                        dtReprint.Columns.Add("Text7", typeof(string)); // LOT ID

                        drCrad = dtReprint.NewRow();

                        drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("1가대"),
                                                  sModlid,
                                                  sPackingNo,
                                                  "*" + sPackingNo + "*",
                                                  sFrom + " -> " + sTo,
                                                  sTotal_C,
                                                  "1",
                                                  "",
                                                  sLotid3,
                                                  sLotid4,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  sVer,
                                                  sVer2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString() + " :"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString()
                                                  , ObjectDic.Instance.GetObjectName("LOT ID").ToString()
                                               };
                    }
                    else
                    {

                        dtReprint.Columns.Add("Title", typeof(string));
                        dtReprint.Columns.Add("MODEL_NAME", typeof(string));
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
                        dtReprint.Columns.Add("PRODID", typeof(string));
                        dtReprint.Columns.Add("EQSG_NAME", typeof(string));
                        dtReprint.Columns.Add("PROC_REMARK", typeof(string));
                        dtReprint.Columns.Add("PKG_DATE", typeof(string));
                        drCrad = dtReprint.NewRow();

                        drCrad.ItemArray = new object[] { sPackageWay,
                                                      sModlid,
                                                      sPackingNo,
                                                      "*" + sPackingNo + "*",
                                                      sFrom + " -> " + sTo,
                                                      sTotal_M,
                                                      sTotal_C,
                                                      "1",
                                                      "2",
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
                                                      Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                      Util.NVC(DataTableConverter.GetValue(dgOutDetail.Rows[0].DataItem, "EQSG_NAME")),
                                                      procRemark,
                                                      sPKG_DATE
                                                    };
                    }
                    dtReprint.Rows.Add(drCrad);

                    // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
                    string sReportCardName = string.Empty;

                    if (PackingReportMode())
                    {
                        sReportCardName = "PackingCard_New_NJ";
                    }
                    else
                    {
                        sReportCardName = "PackingCard_New";
                    }
                    ///////////////////////////////////////////////////////////////

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        if (sLotid.Equals(""))
                        {
                            Parameters[0] = "PackingCard_Roll";
                        }
                        else
                        {
                            // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
                            //Parameters[0] = "PackingCard_New";
                            Parameters[0] = sReportCardName;
                        }
                        Parameters[1] = dtReprint;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                }
                else if (sType == "TWO")
                {
                    DataTable RQSTDT6 = new DataTable();
                    RQSTDT6.TableName = "RQSTDT";
                    RQSTDT6.Columns.Add("LANGID", typeof(String));
                    RQSTDT6.Columns.Add("RCV_ISS_ID", typeof(String));

                    DataRow dr6 = RQSTDT6.NewRow();
                    dr6["LANGID"] = LoginInfo.LANGID;
                    dr6["RCV_ISS_ID"] = sRcv_Iss_Id;

                    RQSTDT6.Rows.Add(dr6);

                    DataTable Result_Add = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKCARD_REPRINT_ADD", "RQSTDT", "RSLTDT", RQSTDT6);

                    if (Result_Add.Rows.Count <= 0)
                    {
                        Util.MessageValidation("SFU3016"); //출고이력 조회중 에러가 발생하였습니다.
                        return;
                    }

                    for (int i = 0; i < Result_Add.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (Result_Add.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld = null;
                            }
                            else
                            {
                                string sVld_date = Result_Add.Rows[i]["VLD_DATE"].ToString();
                                sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            DateTime ProdDate2 = Convert.ToDateTime(Result_Add.Rows[i]["WIPSDTTM"].ToString());
                            sProdDate = ProdDate2.Year.ToString() + "-" + ProdDate2.Month.ToString("00") + "-" + ProdDate2.Day.ToString("00");

                            sPackingNo = Result_Add.Rows[i]["BOXID"].ToString();
                            sModlid = Result_Add.Rows[i]["MODLID"].ToString();
                            sFrom = Result_Add.Rows[i]["FROM_AREA"].ToString();
                            sTo = Result_Add.Rows[i]["SHIPTO_NAME"].ToString();
                            iSum = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY"].ToString());
                            iSum3 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY2"].ToString());
                            sVer = Result_Add.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane = Result_Add.Rows[i]["LANE"].ToString();

                            sLotid = Result_Add.Rows[i]["CSTID"].ToString();
                            sUnitCode = Result_Add.Rows[i]["UNIT_CODE"].ToString();
                            sAbbrCode = ReprintResult.Rows[i]["PRDT_ABBR_CODE"].ToString();

                            sPKG_DATE = Convert.ToDateTime(ReprintResult.Rows[i]["PACKDTTM"]).ToString("yyyy-MM-dd");

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                                {
                                    iConvSum = Util.NVC_Decimal(iSum) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer));
                                    iConvSum3 = Util.NVC_Decimal(iSum3) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer));
                                }
                                else
                                {
                                    iConvSum = Math.Floor(Util.NVC_Decimal(iSum) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer)));
                                    iConvSum3 = Math.Floor(Util.NVC_Decimal(iSum3) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer)));
                                    //GetPatternLengthVer(Util.NVC(CutidResult.Rows[i]["PRODID"]), Util.NVC(CutidResult.Rows[i]["PROD_VER_CODE"]))
                                }
                            }

                            if (Result_Add.Rows[i]["ISS_NOTE"].ToString() == "")
                            {
                                sNote = null;
                            }
                            else
                            {
                                sNote = Result_Add.Rows[i]["ISS_NOTE"].ToString();
                            }

                            if (Result_Add.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                            {
                                sOper_Desc = null;
                            }
                            else
                            {
                                sOper_Desc = Result_Add.Rows[i]["OFFER_SHEET_DESCRIPTION"].ToString();
                            }
                        }
                        else
                        {
                            sPackingNo2 = Result_Add.Rows[i]["BOXID"].ToString();
                            sLane2 = Result_Add.Rows[i]["LANE"].ToString();

                            if (Result_Add.Rows[i]["VLD_DATE"].ToString() == "")
                            {
                                sVld2 = null;
                            }
                            else
                            {
                                string sVld_date = Result_Add.Rows[i]["VLD_DATE"].ToString();
                                sVld2 = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                            }

                            iSum2 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY"].ToString());
                            iSum4 = Convert.ToDouble(Result_Add.Rows[i]["TOTALQTY2"].ToString());
                            sVer2 = Result_Add.Rows[i]["PROD_VER_CODE"].ToString();
                            sLane2 = Result_Add.Rows[i]["LANE"].ToString();

                            sLotid2 = Result_Add.Rows[i]["CSTID"].ToString();

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                                {
                                    iConvSum2 = Util.NVC_Decimal(iSum2) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer2));
                                    iConvSum4 = Util.NVC_Decimal(iSum4) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer2));
                                }
                                else
                                {
                                    iConvSum2 = Math.Floor(Util.NVC_Decimal(iSum2) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer2)));
                                    iConvSum4 = Math.Floor(Util.NVC_Decimal(iSum4) * Util.NVC_Decimal(GetPatternLength(Util.NVC(Result_Add.Rows[i]["PRODID"]), sVer2)));
                                    //GetPatternLengthVer(Util.NVC(CutidResult.Rows[i]["PRODID"]), Util.NVC(CutidResult.Rows[i]["PROD_VER_CODE"]))
                                }
                            }

                        }
                        if (Util.NVC(sEltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                        {
                            if (string.Equals(sUnitCode, "EA"))
                            {
                                double total_M = iSum;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum * 10) / 10)) + "M";

                                double total_C = iSum3;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum3 * 10) / 10)) + "M";

                                double total_M2 = iSum2;
                                sTotal_M2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M2 * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((iConvSum2) * 10) / 10)) + "M";

                                double total_C2 = iSum4;
                                sTotal_C2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C2 * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((iConvSum4) * 10) / 10)) + "M";

                                sTotal_C3 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((total_C + total_C2) * 10) / 10)) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((iConvSum3 + iConvSum4) * 10) / 10)) + "M";
                            }
                            else
                            {
                                double total_M = iSum;
                                sTotal_M = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M * 10) / 10));

                                double total_C = iSum3;
                                sTotal_C = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_C * 10) / 10));

                                double total_M2 = iSum2;
                                sTotal_M2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(total_M2 * 10) / 10));

                                double total_C2 = iSum4;
                                sTotal_C2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((total_C + total_C2) * 10) / 10));

                                sTotal_C3 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate((total_C + total_C2) * 10) / 10));
                            }

                            sM1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum * 10) / 10));
                            sM2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum2 * 10) / 10));
                            sC1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));
                            sC2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum4 * 10) / 10));

                            if (string.Equals(sUnitCode, "EA"))
                            {
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum3 * 10) / 10));
                                sD2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iConvSum4 * 10) / 10));
                            }
                            else
                            {
                                sD1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum3 * 10) / 10));
                                sD2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate(iSum4 * 10) / 10));
                            }
                        }
                        else
                        {
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

                                sTotal_C3 = String.Format("{0:#,##0}", total_C + total_C2) + "/" + String.Format("{0:#,##0}", iConvSum3 + iConvSum4) + "M";
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
                                sTotal_C2 = String.Format("{0:#,##0}", total_C + total_C2);

                                sTotal_C3 = String.Format("{0:#,##0}", total_C + total_C2);
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
                        }
                    }
                    dtPackingCard = new DataTable();
                    DataRow drCrad = null;

                    if (sLotid.Equals(""))
                    {
                        dtPackingCard.Columns.Add("Title", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                        dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                        dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                        dtPackingCard.Columns.Add("Transfer", typeof(string));
                        dtPackingCard.Columns.Add("Total_Cell", typeof(string));
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
                        dtPackingCard.Columns.Add("C1", typeof(string));
                        dtPackingCard.Columns.Add("C2", typeof(string));
                        dtPackingCard.Columns.Add("D1", typeof(string));
                        dtPackingCard.Columns.Add("D2", typeof(string));
                        dtPackingCard.Columns.Add("REMARK", typeof(string));
                        dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                        dtPackingCard.Columns.Add("V_DATE", typeof(string));
                        dtPackingCard.Columns.Add("P_DATE", typeof(string));
                        dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                        dtPackingCard.Columns.Add("PRODID", typeof(string));
                        dtPackingCard.Columns.Add("Text5", typeof(string));
                        dtPackingCard.Columns.Add("Text13", typeof(string));
                        dtPackingCard.Columns.Add("Text7", typeof(string)); // LOT ID


                        drCrad = dtPackingCard.NewRow();

                        drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("2가대"),
                                                  sModlid,
                                                  sOuter_Boxid,
                                                  "*" + sOuter_Boxid + "*",
                                                  sFrom + " -> " + sTo,
                                                  sTotal_C3,
                                                  "1",
                                                  "2",
                                                  sLotid3,
                                                  sLotid4,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  sVer,
                                                  sVer2,
                                                  sC1,
                                                  sC2,
                                                  sD1,
                                                  sD2,
                                                  sNote,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString() + " :"
                                                  , ObjectDic.Instance.GetObjectName("수량").ToString()
                                                  , ObjectDic.Instance.GetObjectName("LOT ID").ToString()
                                               };
                    }
                    else
                    {
                        dtPackingCard.Columns.Add("Title", typeof(string));
                        dtPackingCard.Columns.Add("Title1", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                        dtPackingCard.Columns.Add("MODEL_NAME1", typeof(string));
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
                        dtPackingCard.Columns.Add("PRODID", typeof(string));
                        dtPackingCard.Columns.Add("PRODID1", typeof(string));
                        dtPackingCard.Columns.Add("EQSG_NAME", typeof(string));
                        dtPackingCard.Columns.Add("EQSG_NAME1", typeof(string));
                        dtPackingCard.Columns.Add("PKG_DATE", typeof(string));
                        dtPackingCard.Columns.Add("PKG_DATE1", typeof(string));
                        drCrad = null;

                        drCrad = dtPackingCard.NewRow();

                        drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("1가대"),
                                                   ObjectDic.Instance.GetObjectName("전극포장카드") + " " + ObjectDic.Instance.GetObjectName("2가대"),
                                                  sModlid,
                                                  sModlid,
                                                  sLotid + "0",
                                                  sLotid + "1",
                                                  "*" + sLotid + "0"+ "*",
                                                  "*" + sLotid + "1"+ "*",
                                                  sFrom + " -> " + sTo,
                                                  sFrom + " -> " + sTo,
                                                  sTotal_M,
                                                  sTotal_C,
                                                  sTotal_M2,
                                                  sTotal_C2,
                                                  "1",
                                                  "1",
                                                  sLotid,
                                                  sLotid,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate,
                                                  sVer,
                                                  sVer,
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
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgOutHIst.Rows[dgOutHIst.SelectedIndex].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgOutDetail.Rows[0].DataItem, "EQSG_NAME")),
                                                  Util.NVC(DataTableConverter.GetValue(dgOutDetail.Rows[0].DataItem, "EQSG_NAME")),
                                                  sPKG_DATE,
                                                  sPKG_DATE
                                               };
                    }
                    dtPackingCard.Rows.Add(drCrad);

                    // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
                    string sReportCardName = string.Empty;

                    if (PackingReportMode())
                    {
                        sReportCardName = "PackingCard_2CRT_NJ";
                    }
                    else
                    {
                        sReportCardName = "PackingCard_2CRT";
                    }
                    ///////////////////////////////////////////////////////////////

                    LGC.GMES.MES.BOX001.Report_Common rs = new LGC.GMES.MES.BOX001.Report_Common();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        //태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        if (sLotid.Equals(""))
                        {
                            Parameters[0] = "PackingCard_Roll";
                        }
                        else
                        {
                            // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
                            //Parameters[0] = "PackingCard_2CRT";
                            Parameters[0] = sReportCardName;
                        }
                        Parameters[1] = dtPackingCard;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(Print_Result);
                        // 팝업 화면 숨겨지는 문제 수정.
                        grdMain.Children.Add(rs);
                        rs.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private decimal GetPatternLength(string prodID, string prodVerCode)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PRODID"] = prodID;
                Indata["PROD_VER_CODE"] = prodVerCode;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_PTN", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
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

        private bool PackingReportMode()
        {
            // C20210826-000420 [LGESNJ 生产PI]电极ElectrodePack'g Card 改善
            DataTable RQSTDT3 = new DataTable();
            RQSTDT3.TableName = "RQSTDT";
            RQSTDT3.Columns.Add("LANGID", typeof(string));
            RQSTDT3.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT3.Columns.Add("CMCODE", typeof(string));

            DataRow dr3 = RQSTDT3.NewRow();
            dr3["LANGID"] = LoginInfo.LANGID;
            dr3["CMCDTYPE"] = "ELEC_PACKING_CARD";
            dr3["CMCODE"] = LoginInfo.CFG_SHOP_ID;
            RQSTDT3.Rows.Add(dr3);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT3);

            if (dtResult.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetLotHeatAreaRouteTargetCheck(string sLotid)
        {
            string sHLotHeatAreaRouteTargetCheck = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT"; 
                RQSTDT.Columns.Add("LOTID", typeof(String));


                DataRow dr = RQSTDT.NewRow(); 
                dr["LOTID"] = sLotid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_HEAT_AREA_ROUTE_TARGET_CHECK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {

                    sHLotHeatAreaRouteTargetCheck = Util.NVC(dtResult.Rows[0]["HEAT_AREA_ROUTE_CHECK"].ToString());  // HEAT_AREA_ROUTE 대상여부 CHECK


                }
                else
                {
                    sHLotHeatAreaRouteTargetCheck = "N"; 
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return sHLotHeatAreaRouteTargetCheck;
        }


        #endregion 출고이력탭 (TabOUT_Hist) End

        // [E20240717-000849] OC4동 라벨System변경 요청 件(포장카드)
        #region 상세이력탭 (Tab_Detail) Start

        private void txtBoxid7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtBoxid7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Search_Out_Hist();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }
        private void txtProd_ID7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID7.Text != "")
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    Search_Out_Hist();
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void btnSearch7_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            Search_Out_Hist();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void Search_Out_Hist()
        {
            try
            {

                if ((dtpDateTo_Hist.SelectedDateTime - dtpDateFrom_Hist.SelectedDateTime).TotalDays > 7)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "7");
                    return;
                }

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom_Hist.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo_Hist.SelectedDateTime);

                //string area_HMS = GetAreaHMS();
                //sStart_date = sStart_date + area_HMS;
                //sEnd_date = sEnd_date + area_HMS;

                string sStatus = cboStatus7.SelectedValue.ToString();

                Util.gridClear(dgOutHIst);
                Util.gridClear(dgOutDetail);

                if (cboStatus7.SelectedIndex < 0 || cboStatus7.SelectedValue.ToString().Trim().Equals(""))
                {
                    sStatus = null;
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
                dr["OUTER_BOXID"] = txtBoxid7.Text.Trim() == "" ? null : txtBoxid7.Text;
                dr["SHIPTO_ID"] = Util.GetCondition(cboTransLoc7, bAllNull: true);
                dr["BOX_RCV_ISS_STAT_CODE"] = sStatus;
                dr["FROM_DATE"] = txtBoxid7.Text.Trim() != "" ? null : sStart_date;
                dr["TO_DATE"] = txtBoxid7.Text.Trim() != "" ? null : sEnd_date;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProd_ID7.Text.Trim() == "" ? null : txtProd_ID7.Text;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_HIST_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOut_Hist);
                Util.GridSetData(dgOut_Hist, SearchResult, FrameOperation);

                string[] sColumnName = new string[] { "OUTER_BOXID" };
                _Util.SetDataGridMergeExtensionCol(dgOut_Hist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        private string GetAreaHMS()
        {
            string hms = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(String));
            RQSTDT.Columns.Add("AREAID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_HMS", "RQSTDT", "RSLTDT", RQSTDT);

            if (SearchResult.Rows.Count > 0)
            {
                string temp = SearchResult.Rows[0]["DAY_CLOSE_HMS"].ToString(); //060000
                string temp1 = temp.Substring(0, 2);

                hms = " " + temp1 + ":00";
            }
            else
            {
                hms = " 06:00";
            }

            return hms;
        }

        #endregion 상세이력탭 (Tab_Detail) End
    }
}

