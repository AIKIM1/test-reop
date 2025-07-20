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
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_025 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        private string ACTID = "LOCATE_OUT_STORAGE";
        private string RESNGRID = "ELEC_WH_OUT_REASON";
        private string LOTID = string.Empty;
        private string HoldFlag = string.Empty;
        string HoldScanID = string.Empty;
        DataSet inDataSet = null; //INLOT
        decimal _WipQty = 0;
        string _RoutID = string.Empty;
        string _ProcID = string.Empty;
        string _FlowID = string.Empty;

        public ELEC001_025()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
            //SetcboReasonCode();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMovePC);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo combo = new CommonCombo();
            String[] sFilter1 = { LoginInfo.CFG_EQSG_ID };
            string[] sFilter2 = { "ALL" };
            //SHOP
            C1ComboBox[] cboShopChild = { cboArea, cboEquipmentSegment };
            combo.SetCombo(cboShop, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sCase: "SHOPRELEATION");

            //동
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbParent: cboAreaParent, cbChild: cboAreaChild, sCase: "AREA_NO_AUTH", sFilter: sFilter2);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent);

            //극성
            String[] sFilterElectype = { "ELEC_TYPE" };
            combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODE");

            //프로젝트 명
            combo.SetCombo(cboPrjtName, CommonCombo.ComboStatus.ALL);

            SetEvent();
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            txtPancakeID.Focus();
        }

        #endregion

        #region Mehod
        //BizRule 사용
        private void CheckPancakeID_Biz(string PancakeID)
        {
            try
            {
                if (PancakeID == "")
                {
                    //스캔한 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtPancakeID.Focus();
                    });
                    return;
                }
                //Grid 생성
                DataTable dtData = new DataTable();
                dtData.Columns.Add("CHK", typeof(string));
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("PRODID", typeof(string));
                dtData.Columns.Add("WIPQTY", typeof(string));
                dtData.Columns.Add("WIPQTY2", typeof(string));
                dtData.Columns.Add("PROCID", typeof(string));
                dtData.Columns.Add("WIPHOLD", typeof(string));
                dtData.Columns.Add("EQSGID", typeof(string));
                dtData.AcceptChanges();

                for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString() == PancakeID)
                    {
                        //동일한 LOT이 스캔되었습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1504"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            this.txtPancakeID.Clear();
                            this.txtPancakeID.Focus();
                        });
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("RESNFLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = PancakeID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["RESNFLAG"] = "";// (bool)this.chkemergencyMove.IsChecked ? "Y" : "";

                RQSTDT.Rows.Add(dr);
                //DA_BAS_SEL_WIP_WITH_ATTR_MOVE
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_MOVE_NOTCHING_ELEC_LOT", "RQSTDT", "RSLTDT", RQSTDT); //DA_BAS_SEL_WIP_WITH_ATTR_MOVE

                if (SearchResult.Rows.Count < 1)
                {
                    //{0}의 재공정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2883", new object[] { PancakeID }), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtPancakeID.Clear();
                        this.txtPancakeID.Focus();
                    });
                    return;
                }
                else
                {
                    if (dgOutputPC.ItemsSource != null && dgOutputPC.Rows.Count > 0)
                    {
                        if (DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            //제품ID가 같지 않습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1893"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.txtPancakeID.Clear();
                                this.txtPancakeID.Focus();
                            });
                            return;
                        }

                        if (DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                        {
                            //같은공정의 LOT이 아닙니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2853"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.txtPancakeID.Clear();
                                this.txtPancakeID.Focus();
                            });
                            return;
                        }
                        else
                        {
                            dtData = DataTableConverter.Convert(dgOutputPC.ItemsSource);
                        }
                    }

                    //선입선출 위배여부 [Y: 정상, N:위배]
                    //if (SearchResult.Rows[0]["CHECK_FLAG"].ToString().Equals("N"))
                    //{
                    //    //선입선출 위반입니다. 긴급공정투입하시겠습니까?
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3626", null), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Mresult) =>
                    //    {
                    //        if (Mresult == MessageBoxResult.OK)
                    //        {
                    //            chkemergencyMove.IsChecked = true;

                    //            CheckLotID_Sub(SearchResult, dtData);
                    //        }
                    //        else
                    //        {
                    //            this.txtPancakeID.Clear();
                    //            this.txtPancakeID.Focus();
                    //        }
                    //    });
                    //}
                    //else
                    //{
                        CheckLotID_Sub(SearchResult, dtData);
                    //}
                }
            }
            catch (Exception ex)
            {
                /*LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtPancakeID.Clear();
                    this.txtPancakeID.Focus();
                });
                */
                Util.MessageException(ex);
                
                    this.txtPancakeID.Clear();
                    this.txtPancakeID.Focus();
               
                return;
            }
        }


        private void CheckLotID_Sub(DataTable SearchResult, DataTable dtData)
        {
            if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
            {
                //이동가능한 LOT이 아닙니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2925"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtPancakeID.Clear();
                    this.txtPancakeID.Focus();
                });
                return;
            }
            if (SearchResult.Rows[0]["WH_ID"].ToString() == "")
            {
                //창고에 입고되지않은 LOT입니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2962"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtPancakeID.Clear();
                    this.txtPancakeID.Focus();
                });
                return;
            }

            DataRow newRow = null;
            newRow = dtData.NewRow();
            newRow["CHK"] = SearchResult.Rows[0]["CHK"].ToString();
            newRow["LOTID"] = SearchResult.Rows[0]["LOTID"].ToString();
            newRow["PRODID"] = SearchResult.Rows[0]["PRODID"].ToString();
            newRow["WIPQTY"] = SearchResult.Rows[0]["WIPQTY"].ToString();
            newRow["WIPQTY2"] = SearchResult.Rows[0]["WIPQTY2"].ToString();
            newRow["PROCID"] = SearchResult.Rows[0]["PROCID"].ToString();
            newRow["WIPHOLD"] = SearchResult.Rows[0]["WIPHOLD"].ToString();
            newRow["EQSGID"] = SearchResult.Rows[0]["EQSGID"].ToString();
            _WipQty = (decimal)SearchResult.Rows[0]["WIPQTY"];
            dtData.Rows.Add(newRow);
            dgOutputPC.ItemsSource = DataTableConverter.Convert(dtData);

            _RoutID = SearchResult.Rows[0]["ROUTID"].ToString();
            _ProcID = SearchResult.Rows[0]["PROCID"].ToString();
            _FlowID = SearchResult.Rows[0]["FLOWID"].ToString();

            this.txtPancakeID.Clear();
        }

        //INLOT
        private DataTable _InLot()
        {
            DataTable dt = null;

            dt = ((DataView)dgOutputPC.ItemsSource).Table;
            DataTable IndataTable = inDataSet.Tables.Add("INLOT");
            IndataTable.Columns.Add("LOTID", typeof(string));
            foreach (DataRow dr in dt.Rows)
            {
                dr["LOTID"] = dr["LOTID"];
                IndataTable.ImportRow(dr);
            }
            return IndataTable;
        }
        //private DataTable _InResn()
        //{
        //    DataTable dt = null;
        //    DataTable IndataTable = inDataSet.Tables.Add("INRESN");
        //    IndataTable.Columns.Add("RESNCODE", typeof(string));
        //    IndataTable.Columns.Add("RESNQTY", typeof(decimal));
        //    IndataTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
        //    IndataTable.Columns.Add("PROCID_CAUSE", typeof(string));
        //    IndataTable.Columns.Add("RESNNOTE", typeof(string));
        //    DataRow dr = IndataTable.NewRow();

        //    dt = ((DataView)dgOutputPC.ItemsSource).Table;
        //    dr["RESNCODE"] = cboReasonCodeMove.SelectedValue.ToString().Trim();
        //    dr["RESNQTY"] = _WipQty;
        //    dr["RESNCODE_CAUSE"] = null;
        //    dr["PROCID_CAUSE"] = null;
        //    dr["RESNNOTE"] = null;
        //    IndataTable.Rows.Add(dr);
        //    return IndataTable;
        //}

        private void Save_RTLS_Mapping_Condition()
        {
            try
            {
                string sCondition = "";
                string sZoneID = "";

                // 해체
                sCondition = "RACK_OUT";

                DataSet indataSet = new DataSet();

                DataTable inData = indataSet.Tables.Add("INDATA");

                inData.Columns.Add("CONDITION", typeof(string));
                inData.Columns.Add("CART_NO", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                {
                    DataRow row = inData.NewRow();
                    row["CONDITION"] = sCondition;
                    row["CART_NO"] = sZoneID; //DataTableConverter.GetValue(dgData.Rows[i].DataItem, "CART_NO").ToString(); //수정필요
                    row["LOTID"] = DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString();
                    row["USERID"] = LoginInfo.USERID;

                    indataSet.Tables["INDATA"].Rows.Add(row);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, indataSet);
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private bool WipHold()
        {           
            for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
            {
                HoldFlag = DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPHOLD").ToString();
                if (HoldFlag == "Y")
                {
                    HoldScanID += DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID").ToString() + ", ";
                }
            }

            if (HoldScanID != "")
            {
                HoldScanID = HoldScanID.Remove(HoldScanID.Length - 2);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Event
        //사유코드 콤보 셋팅
        //private void SetcboReasonCode()
        //{
        //    try
        //    {
        //        DataTable INDATA = new DataTable();
        //        INDATA.TableName = "INDATA";
        //        INDATA.Columns.Add("LANGID", typeof(string));
        //        INDATA.Columns.Add("ACTID", typeof(string));
        //        INDATA.Columns.Add("RESNGRID", typeof(string));

        //        DataRow dr = INDATA.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["ACTID"] = ACTID;
        //        dr["RESNGRID"] = RESNGRID;
        //        INDATA.Rows.Add(dr);

        //        new ClientProxy().ExecuteService("COR_SEL_ACTIVITYREASON_CBO", "INDATA", "OUTDATA", INDATA, (dtResult, Exception) =>
        //        {
        //            if (Exception != null)
        //            {
        //                ControlsLibrary.MessageBox.Show(Exception.Message, null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
        //                return;
        //            }
        //            cboReasonCodeMove.DisplayMemberPath = "RESNNAME";
        //            cboReasonCodeMove.SelectedValuePath = "RESNCODE";
        //            cboReasonCodeMove.ItemsSource = DataTableConverter.Convert(dtResult);
        //            cboReasonCodeMove.SelectedIndex = 0;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return;
        //    }
        //}

        private void SetDataGridReport()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(Util.GetCondition(cboPrjtName)); //PrjName
                dr["PRDT_CLSS_CODE"] = Util.ConvertEmptyToNull(Util.GetCondition(cboElecType)); //ElecType
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_STOCK_REPORT", "INDATA", "OUTDATA", INDATA, (dtResult, Exception) =>
                {
                    if (Exception != null)
                    {

                        ControlsLibrary.MessageBox.Show(Exception.Message, null, "Info", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                        return;
                    }

                    dgDataReport.ItemsSource = DataTableConverter.Convert(dtResult);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtPancakeID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    e.Handled = true;
                    CheckPancakeID_Biz(txtPancakeID.Text.Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void btnMovePC_Click(object sender, RoutedEventArgs e)//노칭이동
        {
            if (dgOutputPC.Rows.Count <= 0)
            {
                //출고처리할 LOT 정보가 존재하지 않습니다.
                Util.AlertInfo("SFU2967");
                return;
            }

            // CMI요청으로 WIPHOLD시 Interlock 추가 [2019-07-31]
            if(IsCommonCode("ELTR_SHIP_TO_NTC_HOLD_CHK_AREA", LoginInfo.CFG_AREA_ID) == true)
            {
                bool isHold = false;
                string sLotID = string.Empty; 

                for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
                {
                    if(string.Equals(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPHOLD"), "Y"))
                    {
                        isHold = true;
                        sLotID = Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "LOTID"));
                        break;
                    }
                }

                if ( isHold == true)
                {
                    Util.MessageValidation("SFU5115", new object[] { sLotID });
                    return;
                }
            }

            string sToShop = string.Empty;
            string sToArea = string.Empty;
            string sToEqsg = string.Empty;

            sToShop = Util.GetCondition(cboShop, MessageDic.Instance.GetMessage("SFU1424"));    //Shop을 선택하세요.
            sToArea = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));    //동을 선택하세요.
            sToEqsg = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));    //라인을 선택하세요.

            double dSum = 0;
            double dSum2 = 0;
            double dTotal = 0;
            double dTotal2 = 0;

            for (int i = 0; i < dgOutputPC.GetRowCount(); i++)
            {
                dSum = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY")));
                dSum2 = Double.Parse(Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY2")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(dgOutputPC.Rows[i].DataItem, "WIPQTY2")));

                dTotal = dTotal + dSum;
                dTotal2 = dTotal2 + dSum2;
            }

            #region INDATA
            inDataSet = new DataSet();

            DataTable INDATA = inDataSet.Tables.Add("INDATA"); // new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("PRODID", typeof(string));
            INDATA.Columns.Add("FROM_SHOPID", typeof(string));
            INDATA.Columns.Add("FROM_AREAID", typeof(string));
            INDATA.Columns.Add("FROM_EQSGID", typeof(string));
            INDATA.Columns.Add("FROM_PROCID", typeof(string));
            INDATA.Columns.Add("FROM_PCSGID", typeof(string));
            INDATA.Columns.Add("TO_SHOPID", typeof(string));
            INDATA.Columns.Add("TO_AREAID", typeof(string));
            INDATA.Columns.Add("TO_EQSGID", typeof(string));
            INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
            INDATA.Columns.Add("MOVE_ORD_QTY", typeof(string));
            INDATA.Columns.Add("MOVE_ORD_QTY2", typeof(string));
            INDATA.Columns.Add("INTRANSIT_FLAG", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["PRODID"] = DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PRODID").ToString();
            dr["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["FROM_EQSGID"] = DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "EQSGID").ToString();
            dr["FROM_PROCID"] = DataTableConverter.GetValue(dgOutputPC.Rows[0].DataItem, "PROCID").ToString();
            dr["FROM_PCSGID"] = "E";
            dr["TO_SHOPID"] = sToShop;
            dr["TO_AREAID"] = sToArea;
            dr["TO_EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["TO_SLOC_ID"] = null;
            dr["MOVE_ORD_QTY"] = dTotal;
            dr["MOVE_ORD_QTY2"] = dTotal2;
            dr["INTRANSIT_FLAG"] = "Y";
            dr["NOTE"] = null;
            dr["USERID"] = LoginInfo.USERID;
            INDATA.Rows.Add(dr);
            #endregion

            #region INLOT
            DataTable InLot = _InLot();
            #endregion

            #region INRESN
            //if ((bool)chkemergencyMove.IsChecked)
            //{
            //    DataTable InResn = _InResn();
            //}

            #endregion

            if (!WipHold())//Hold Lot이 있을 경우
            {
                //Hold상태 반제품입니다. 출고하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3132", new object[] { Util.NVC(HoldScanID) }), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Mresult) =>
                {
                    if (Mresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            //BR_PRD_REG_SEND_PACKLOT_SHOP -> BR_PRD_REG_STOCK_PANCAKE_OUT
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_PANCAKE_OUT", "INDATA,INLOT", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    {
                                        this.txtPancakeID.Focus();
                                    });
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다. 
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                        this.SetDataGridReport();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                    else
                    {
                        HoldScanID = string.Empty;
                        return;
                    }
                });
            }
            else
            {
                //출고 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3121"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            //BR_PRD_REG_SEND_PACKLOT_SHOP -> BR_PRD_REG_STOCK_PANCAKE_OUT
                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_STOCK_PANCAKE_OUT", "INDATA,INLOT,INRESN", null, (Bizresult, Exception) =>
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    {
                                        this.txtPancakeID.Focus();
                                    });
                                    return;
                                }
                                else
                                {
                                    //정상처리되었습니다. 
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                                    {
                                        //this.Save_RTLS_Mapping_Condition();//RTLS 대차매핑 처리 //20170328 주석처리
                                        this.Initialize_dgReceive();
                                    });
                                }
                            }, inDataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }

                    }
                });
            }

        }
        private void Initialize_dgReceive()
        {            
            Util.gridClear(dgOutputPC);
            txtPancakeID.Text = null;
            txtPancakeID.Focus();
            HoldScanID = string.Empty;
            //chkemergencyMove.IsChecked = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    
                    dgOutputPC.IsReadOnly = false;
                    dgOutputPC.RemoveRow(index);
                    dgOutputPC.IsReadOnly = true;
                }
            });
        }

        //private void chkemergency_Checked(object sender, RoutedEventArgs e)
        //{
        //    this.cboReasonCodeMove.IsEnabled = true;
        //}

        //private void chkemergency_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    this.cboReasonCodeMove.IsEnabled = false;
        //}

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.txtPancakeID.Focus();
        }
        #endregion

        private void btnSearchShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //프로젝트명을 재검색 후 기존 프로젝트명 선택 
                string sPrjtName = (string)cboPrjtName.SelectedValue;
                combo.SetCombo(cboPrjtName, CommonCombo.ComboStatus.ALL);

                if (!string.IsNullOrEmpty(sPrjtName))
                    cboPrjtName.SelectedValue = sPrjtName;

                this.SetDataGridReport();
            }
            catch (Exception ex)
            { }
        }

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
                    CheckPancakeID_Biz(dtLot.Rows[0]["LOTID"].ToString());
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private bool IsCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

    }
}
