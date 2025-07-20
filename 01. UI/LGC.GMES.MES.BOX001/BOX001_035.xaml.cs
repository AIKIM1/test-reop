/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.05.17  김도형    : [E20240408-000359] 电极包装card改善 전극포장card improvement




  
 
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
    public partial class BOX001_035 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        Util _Util = new Util();

        public string sTempLot_1 = string.Empty;
        public string sTempLot_2 = string.Empty;
        public string sTempSkid_1 = string.Empty;
        public string sTempSkid_2 = string.Empty;
        public string sNote2 = string.Empty;
        private string _APPRV_PASS_NO = string.Empty;

        public Boolean bReprint = true;

        private BOX001_035_PACKINGCARD window01 = new BOX001_035_PACKINGCARD();
        private BOX001_035_PACKINGCARD_MERGE window02 = new BOX001_035_PACKINGCARD_MERGE();

        public bool bNew_Load = false;
        public DataTable dtPackingCard;

        public bool bCancel = false;


        public BOX001_035()
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

            _combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "TRANSLOC");

            #endregion
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
            sTempSkid_1 = string.Empty;
            sTempSkid_2 = string.Empty;
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

                string sLot_Type = "PANCAKE";

                txtLotID.IsReadOnly = true;
                txtCARRIERID.IsReadOnly = true;
                btnPackOut.IsEnabled = false;

                dgOut.IsEnabled = false;               

                if (sLot_Type == "PANCAKE")
                {
                    string sTempProdName_1 = string.Empty;
                    string sTempProdName_2 = string.Empty;
                    string sPackingLotType1 = string.Empty;
                    string sPackingLotType2 = string.Empty;
                    string sCstID = string.Empty;
                    string sCstID2 = string.Empty;

                    bReprint = false;

                    int imsiCheck = 0;                // 설명:처리해야할 LOT 개수 판단
                    int iCheckCounts = 1;

                    sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                    sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                    sCstID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "CSTID"));
                    sTempSkid_1 = sCstID;
                    imsiCheck = 1;
                    for (int i = 1; i < dgOut.GetRowCount(); i++)
                    {
                        if (!sCstID.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                        {
                            if (!sCstID2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                            {
                                sCstID2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));
                                iCheckCounts++;
                            }
                        }
                        if(iCheckCounts == 2)
                        {
                            if (sCstID2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID")))||sCstID.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                            {
                                if(sCstID2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                                    sTempSkid_2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));
                            }
                            else
                                iCheckCounts++;
                        }
                    }

                    if (iCheckCounts > 2)
                    {
                        Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                        return;
                    }

                    int t1 = 0;
                    int t2 = 0;
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (sTempSkid_1.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                        {
                            if(t1 == 0)
                            {
                                sTempLot_1 += Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                                t1++;
                            }
                            else
                                sTempLot_1 += "," + Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        }
                        if (sTempSkid_2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                        {
                            if (t2 == 0)
                            {
                                sTempLot_2 += Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                                t2++;
                            }
                            else
                                sTempLot_2 += "," + Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                           
                            imsiCheck = 2;
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

        private void Get_Sub()
        {
            if (dgSub.Children.Count == 0)
            {
                bNew_Load = true;
                window01.BOX001_035 = this;
                window01.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                dgSub.Children.Add(window01);
            }
        }

        private void Get_Sub_Merge()
        {
            if (dgSub.Children.Count == 0)
            {
                bNew_Load = true;
                window02.BOX001_035 = this;
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
                    string sCstID = string.Empty;
                    string sCstID2 = string.Empty;
                    sLot_Type = "PANCAKE";
                    
                    if (txtLotID.Text.ToString() == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
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

                        DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);
                        if (AreaResult.Rows.Count == 0)
                        {
                            // [E20240408-000359] 电极包装card改善 전극포장card improvement
                            if (cboTransLoc2.SelectedIndex < 0 || string.IsNullOrEmpty(cboTransLoc2.SelectedValue.ToString()) || cboTransLoc2.Text.ToString().Equals("-SELECT-"))
                            {
                                Util.MessageInfo("SFU4335"); // 출고처를 선택해 주세요
                                return;
                            }

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
                                row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                                indataSet.Tables["INDATA"].Rows.Add(row);
                                loadingIndicator.Visibility = Visibility.Visible;

                                //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                                //신규 HOLD 적용을 위해 변경 작업
                                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    if (Exception != null)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                        return;
                                    }

                                    Search_Pancake(sLotid);

                                    txtLotID.SelectAll();
                                    txtLotID.Focus();

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
                        else
                        {
                            Search_QMS_Validation(sLotid);
                            // 품질결과 Skip
                            Search_Pancake(sLotid);

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

        private void Search_Pancake(string sLotid)
        {
            try
            {
                string sCstID = string.Empty;
                int iCheckCounts = 1;

                // 재공정보 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = sLotid;

                RQSTDT2.Rows.Add(dr2);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_PANCAKE", "RQSTDT", "RSLTDT", RQSTDT2);

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
                        if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE")) != Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE")).Equals("X") || Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]).Equals("X"))
                            {
                                Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다.
                                return;
                            }
                        }
                        #endregion
                    }

                    string sCstID1 = string.Empty;
                    string sCstID2 = string.Empty;

                    if (dgOut.Rows.Count > 0)
                        sCstID1 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "CSTID")).ToString();
                    for (int i = 1; i < dgOut.GetRowCount(); i++)
                    {
                        if (!sCstID1.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                        {
                            if (!sCstID2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                            {
                                sCstID2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));
                                iCheckCounts++;
                            }
                        }
                        if (iCheckCounts == 2)
                        {
                            if (!sCstID2.Equals(Lot_Result.Rows[0]["CSTID"].ToString()) && !sCstID1.Equals(Lot_Result.Rows[0]["CSTID"].ToString()))
                                iCheckCounts++;
                        }
                    }

                    if (iCheckCounts > 2)
                    {
                        Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "MKT_TYPE_CODE").ToString())
                    {
                        Util.Alert("SFU4454");   //내수용과 수출용은 같이 포장할 수 없습니다.
                        return;
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
                loadingIndicator.Visibility = Visibility.Collapsed;
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
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CSTID", dt.Rows[0]["CSTID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LANE_QTY", dt.Rows[0]["LANE_QTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MKT_TYPE_CODE", dt.Rows[0]["MKT_TYPE_CODE"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTTYPE", dt.Rows[0]["LOTTYPE"].ToString());
            dgOut.IsReadOnly = true;
        }

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
                string sCstID = string.Empty;
                string sCstID2 = string.Empty;
                sLot_Type = "PANCAKE";

                
                if (sLot_Type == "PANCAKE")
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
                    RQSTDT1.Columns.Add("PROCID", typeof(String));
                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LOTID"] = sLotID;
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
                    DataTable vTable = Prod_Result.DefaultView.ToTable(true, "LOTTYPE");
                    DataRow[] dRow = vTable.Select("LOTTYPE = 'X'");
                    if (vTable.Rows.Count > 1 && dRow.Length != 0)
                    {
                        Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다.
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
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);
                            loadingIndicator.Visibility = Visibility.Visible;

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                    return;
                                }

                                Search_Pancake(sLotID);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            // 품질결과 Skip
                            Search_Pancake(sLotID);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else
                    {
                        Search_QMS_Validation(sLotID);
                        // 품질결과 Skip
                        Search_Pancake(sLotID);

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
}

