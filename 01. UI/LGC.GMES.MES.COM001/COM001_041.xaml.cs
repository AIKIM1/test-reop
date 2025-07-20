/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 라인이동
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.11.29  백광영    : LOTID Copy & Paste 기능 추가
  2018.03.06  신광희    : BOX ID 조회 조건 추가 및 그리드 컬럼 추가 
  2021.08.19  김지은    : 반품 시 LOT유형의 시험생산구분코드 Validation
  2023.11.08  오수현    : E20230621-000829 이력조회 탭 인수처 라인 멀티콤보로 변경
  2024.08.21  안유수 E20240705-001701 라인간이동 > 이력 조회 탭에서 인계처 콤보 데이터에 따라 인수처 콤보 값 변경되도록 수정
  2025.04.25  이주원    : E20240905-001442(CAT_UP_0589) 작업자가 대상LOT을 체크하지 않고 '이동'버튼을 누를 시 지정에러메세지 표시
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_041 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        string ValueToLot = string.Empty;

        List<string> LotList = new List<string>();

        public COM001_041()
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
            Initialize();

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOut);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();

            txtLotID_Out.Focus();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo combo = new CommonCombo();

            //라인
            string[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboFromEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT");

            combo.SetCombo(cboToEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT");

            //라인
            //combo.SetCombo(cboHistToEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT_EXCEPT");
            SetCboHistToEquipmentSegment();

            //라인
            combo.SetCombo(cboHistFromEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT");

            string[] sFilter = { "MOVE_ORD_STAT_CODE" };
            combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            #region [상세 인계 이력]
            dtpDateFrom2.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo2.SelectedDateTime = (DateTime)System.DateTime.Now;

            combo.SetCombo(cboHistFromEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT");

            combo.SetCombo(cboStat2, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            combo.SetCombo(cboHistToEquipmentSegment2, CommonCombo.ComboStatus.ALL, cbChild: null, sFilter: sFilter1, sCase: "EQUIPMENTSEGMENT_EXCEPT");

            #endregion

            txtLotID_Out.Focus();
            txtLotID_Out.SelectAll();            

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

        #region Event
        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgMoveList.GetRowCount() == 0 || dgMoveList.GetCheckedDataRow("CHK").Count <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }

                #region # 특별관리 Lot - 인계가능 라인 체크
                string sLine = Util.GetCondition(cboToEquipmentSegment);

                if (CommonVerify.HasDataGridRow(dgMoveList))
                {
                    DataTable dt = ((DataView)dgMoveList.ItemsSource).Table;
                    foreach (DataRow dRow in dt.Rows)
                    {
                        if (string.Equals(Util.NVC(dRow["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(dRow["RSV_EQSGID_LIST"])))
                        {
                            if (!Util.NVC(dRow["RSV_EQSGID_LIST"]).Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(dRow["LOTID"]), Util.NVC(dRow["RSV_EQSGID_LIST"]) });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return;
                            }
                        }
                    }
                }
                #endregion

                string sToshop = string.Empty;
                string sToArea = string.Empty;
                string sToEqsg = string.Empty;
              

                sToArea = LoginInfo.CFG_AREA_ID;
                sToEqsg = Util.GetCondition(cboToEquipmentSegment, "SFU1223");    //라인을 선택하세요.
                if (sToEqsg.Equals("")) return;

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("AREAID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["AREAID"] = sToArea;

                RQSTDT1.Rows.Add(dr1);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_SHIOPID", "RQSTDT", "RSLTDT", RQSTDT1);

                sToshop = Result.Rows[0]["SHOPID"].ToString();

                //double dSum = 0;
                //double dSum2 = 0;
                //double dTotal = 0;
                //double dTotal2 = 0;

                //for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                //{
                //    dSum = Convert.ToDouble(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY").ToString());
                //    dSum2 = Convert.ToDouble(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY2").ToString());

                //    dTotal = dTotal + dSum;
                //    dTotal2 = dTotal2 + dSum2;
                //}

                decimal dSum = 0;
                decimal dSum2 = 0;
                decimal dTotal = 0;
                decimal dTotal2 = 0;

                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {                    
                    dSum = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY")));
                    dSum2 = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY2")));

                    dTotal = dTotal + dSum;
                    dTotal2 = dTotal2 + dSum2;
                }


                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("FROM_SHOPID", typeof(string));
                inData.Columns.Add("FROM_AREAID", typeof(string));
                inData.Columns.Add("FROM_EQSGID", typeof(string));
                inData.Columns.Add("FROM_PROCID", typeof(string));
                inData.Columns.Add("TO_SHOPID", typeof(string));
                inData.Columns.Add("TO_AREAID", typeof(string));
                inData.Columns.Add("TO_EQSGID", typeof(string));
                inData.Columns.Add("TO_PROCID", typeof(string));
                inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["FROM_SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                row["FROM_EQSGID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "EQSGID").ToString();
                row["FROM_PROCID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString();
                row["TO_SHOPID"] = sToshop;
                row["TO_AREAID"] = sToArea;
                row["TO_EQSGID"] = sToEqsg;
                row["TO_PROCID"] = DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString();
                row["MOVE_ORD_QTY"] = dTotal;
                row["MOVE_ORD_QTY2"] = dTotal2;
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("WIPQTY2", typeof(decimal));

                for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        DataRow row2 = inLot.NewRow();
                        row2["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID"));
                        row2["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY"));
                        row2["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "WIPQTY2"));

                        indataSet.Tables["INLOT"].Rows.Add(row2);
                    }
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_LINE", "INDATA,INLOT", null, indataSet);

                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                Util.gridClear(dgMoveList);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;             
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_data();
        }

        private void Search_data()
        {
            try
            {
                Util.gridClear(dgMove_Detail);
                Util.gridClear(dgMove_Master);

                string sStart_date = Util.GetCondition(dtpDateFrom);
                string sEnd_date = Util.GetCondition(dtpDateTo); string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sArea = LoginInfo.CFG_AREA_ID;
                string sEqsgFrom = Util.GetCondition(cboHistFromEquipmentSegment, bAllNull: true);
                //string sEqsg = Util.GetCondition(cboHistToEquipmentSegment, bAllNull: true);
                string sEqsgTo = Util.ConvertEmptyToNull(cboHistToEquipmentSegment.SelectedItemsToString);


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_EQSGID"] = sEqsgFrom;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_EQSGID"] = sEqsgTo;
                dr["MOVE_TYPE_CODE"] = "MOVE_LINE";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat, bAllNull: true);
                dr["PRODID"] = txtProd_ID.Text == "" ? null : txtProd_ID.Text;
                RQSTDT.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(RQSTDT);
                //string xml = ds.GetXml();

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove_Master, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

      
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {     
            Util.gridClear(dgMoveList);
        }


        /// <summary>
        /// 인계취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMoveOrderID = string.Empty;

                DataRow[] drChk = Util.gridGetChecked(ref dgMove_Master, "CHK");

                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                    return;
                }
                else
                {
                    sMoveOrderID = drChk[0]["MOVE_ORD_ID"].ToString();
                }

                if (!drChk[0]["MOVE_ORD_STAT_CODE"].ToString().Equals("MOVING"))
                {
                    Util.AlertInfo("SFU1939");  //취소할 수 있는 상태가 아닙니다.
                    return;
                }                

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("MOVE_ORD_ID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = "UI";
                row["MOVE_ORD_ID"] = sMoveOrderID;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgMove_Detail.GetRowCount(); i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "CHK")).Equals("True"))
                    //{
                        if (DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "MOVE_STAT_CODE").ToString() == "MOVING")
                        {
                            DataRow row2 = inLot.NewRow();
                            row2["LOTID"] = DataTableConverter.GetValue(dgMove_Detail.Rows[i].DataItem, "LOTID").ToString();

                            indataSet.Tables["INLOT"].Rows.Add(row2);
                        }
                    //}
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_PACKLOT_AREA", "INDATA,INLOT", null, indataSet);

                Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                Util.gridClear(dgMove_Detail);
                Util.gridClear(dgMove_Master);

                Search_data();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLotID_Out_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList(txtLotID_Out.Text.Trim(), true);
            }
        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    const string bizRuleName = "DA_BAS_SEL_WIP_WITH_ATTR_MOVE";

                    //DoEvents();
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("EQSGID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = txtBoxId.Text.Trim();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["EQSGID"] = Util.GetCondition(cboFromEquipmentSegment, "SFU1223");    //라인을 선택하세요.
                    inTable.Rows.Add(dr);
                    

                    DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                    if (searchResult.Rows.Count < 1)
                    {
                        Util.MessageValidation("SFU1870");
                        return;
                    }
                    else
                    {
                        // NND End시에도 처리 가능하도록 변경
                        if (Util.NVC(searchResult.Rows[0]["PROCID"]).Equals(Process.NOTCHING))
                        {
                            if (!(searchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT") || searchResult.Rows[0]["WIPSTAT"].ToString().Equals(Wip_State.END)))
                            {
                                Util.MessageValidation("SFU1869");
                                //Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                return;
                            }
                        }
                        else
                        {
                            if (!searchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                            {
                                Util.MessageValidation("SFU1869");
                                //Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                return;
                            }
                        }

                        if (searchResult.Rows[0]["EQSGID"].ToString().Equals(Util.GetCondition(cboToEquipmentSegment)))
                        {
                            Util.MessageValidation("SFU1653");
                            //Util.Alert("SFU1653");  //선택된 라인의 재공입니다.
                            return;
                        }

                        //HOLD 재공 이동 처리 가능하도록 수정 요청 (폴란드3동 요구 사항)
                        //if (searchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                        //{
                        //    Util.MessageValidation("SFU1340");
                        //    //Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                        //    return;
                        //}

                        #region # 특별관리 Lot - 인계가능 라인 체크
                        if (string.Equals(Util.NVC(searchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["RSV_EQSGID_LIST"])))
                        {
                            string SpclLine = Util.NVC(searchResult.Rows[0]["RSV_EQSGID_LIST"]);
                            string sLine = Util.GetCondition(cboToEquipmentSegment);

                            if (!SpclLine.Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(searchResult.Rows[0]["LOTID"]), SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return;
                            }
                        }
                        #endregion

                        if (dgMoveList.GetRowCount() > 1)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString() != searchResult.Rows[0]["PROCID"].ToString())
                            {
                                Util.MessageValidation("SFU1789");
                                //Util.Alert("SFU1789");  //이전에 스캔한 LOT의 공정과 다릅니다.
                                return;
                            }
                            #region # 특별관리 Lot - 혼입 체크 
                            if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "SPCL_FLAG")) != Util.NVC(searchResult.Rows[0]["SPCL_FLAG"]))
                            {
                                if ((Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "SPCL_FLAG")) == "Y") || (Util.NVC(searchResult.Rows[0]["SPCL_FLAG"]) == "Y"))
                                {
                                    Util.Alert("SFU8214");  // 특별관리 Lot과 일반 Lot을 같이 이동할 수 없습니다.
                                    return;
                                }
                            }
                            #endregion
                        }

                        if (dgMoveList.GetRowCount() == 0)
                        {
                            dgMoveList.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == searchResult.Rows[0]["PRODID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(searchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                                    dtSource.Merge(searchResult);

                                    Util.gridClear(dgMoveList);
                                    dgMoveList.ItemsSource = DataTableConverter.Convert(dtSource);
                                }
                                else
                                {
                                    Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    return;
                                }
                            }
                            else
                            {
                                Util.MessageValidation("SFU1893");
                                //Util.Alert("SFU1893");      //제품ID가 같지 않습니다.
                                return;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtBoxId.Text = string.Empty;
                    txtBoxId.Focus();
                }
            }
        }

        private void txtLotID_Out_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string _ValueToFind = string.Empty;

                    LotList.Clear();

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList(sPasteStrings[i], false) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        _ValueToFind = string.Join(",", LotList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }
                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLotID_Out.Text = "";
                    txtLotID_Out.Focus();

                    //HiddenLoadingIndicator();
                }
            }
        }
        #endregion

        #region Mehod

        private void SetCboHistToEquipmentSegment()
        {
            try
            {
                //라인
                string[] sFilter1 = { LoginInfo.CFG_AREA_ID };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sFilter1[0];
                dr["EQSGID"] = Convert.ToString(cboHistFromEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQSG_EXCEPT_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboHistToEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);

                cboHistToEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboHistToEquipmentSegment.SelectedValuePath = "CBO_CODE";
            }
            catch //(Exception ex)
            {
                //Util.MessageException(ex);
            }
        }


        private void SearchMoveLOTList(int idx)
        {
            string sMoveOrderID = string.Empty;

            sMoveOrderID = DataTableConverter.GetValue(dgMove_Master.Rows[idx].DataItem, "MOVE_ORD_ID").ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("MOVE_ORD_ID", typeof(String));
            RQSTDT.Columns.Add("LANGID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["MOVE_ORD_ID"] = sMoveOrderID;
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_DETAIL_LINE", "RQSTDT", "RSLTDT", RQSTDT);

            //Util.gridClear(dgMove_Detail);
            //dgMove_Detail.ItemsSource = DataTableConverter.Convert(DetailResult);

            Util.GridSetData(dgMove_Detail, DetailResult, FrameOperation);
        }

        private bool GetLotList(string sLotId, bool vOnlyOne)
        {
            try
            {
                DoEvents();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotId;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboFromEquipmentSegment, "SFU1223");    //라인을 선택하세요.;

                RQSTDT.Rows.Add(dr);
                
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR_MOVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (vOnlyOne)  // 1건 / Multi 구분
                {
                    if (SearchResult.Rows.Count < 1)
                    {
                        Util.Alert("SFU1870");      //재공 정보가 없습니다.
                        return false;
                    }
                    else
                    {
                        // NND End시에도 처리 가능하도록 변경
                        if (Util.NVC(SearchResult.Rows[0]["PROCID"]).Equals(Process.NOTCHING))
                        {
                            if (!(SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT") || SearchResult.Rows[0]["WIPSTAT"].ToString().Equals(Wip_State.END)))
                            {
                                Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                return false;
                            }
                        }
                        else
                        {
                            if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                            {
                                Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                return false;
                            }
                        }

                        if (SearchResult.Rows[0]["EQSGID"].ToString().Equals(Util.GetCondition(cboToEquipmentSegment)))
                        {
                            Util.Alert("SFU1653");  //선택된 라인의 재공입니다.
                            return false;
                        }

                        //HOLD 재공 이동 처리 가능하도록 수정 요청 (폴란드3동 요구 사항)
                        //if (SearchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                        //{
                        //    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                        //    return false;
                        //}

                        #region # 특별관리 Lot - 인계가능 라인 체크
                        if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                        {
                            string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                            string sLine = Util.GetCondition(cboToEquipmentSegment);

                            if (!SpclLine.Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]), SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return false;
                            }
                        }
                        #endregion

                        for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString() == sLotId)
                            {
                                Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                                return false;
                            }
                            #region # 특별관리 Lot - 혼입 체크 
                            if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) != Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]))
                            {
                                if ((Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) == "Y") || (Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]) == "Y"))
                                {
                                    Util.Alert("SFU8214");  // 특별관리 Lot과 일반 Lot을 같이 이동할 수 없습니다.
                                    return false;
                                }
                            }
                            #endregion
                        }

                        if (dgMoveList.GetRowCount() > 1)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                            {
                                Util.Alert("SFU1789");  //이전에 스캔한 LOT의 공정과 다릅니다.
                                return false;
                            }
                        }

                        if (dgMoveList.GetRowCount() == 0)
                        {
                            dgMoveList.ItemsSource = DataTableConverter.Convert(SearchResult);
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                                    dtSource.Merge(SearchResult);

                                    Util.gridClear(dgMoveList);
                                    dgMoveList.ItemsSource = DataTableConverter.Convert(dtSource);
                                }
                                else
                                {
                                    Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    return false;
                                }
                            }
                            else
                            {
                                Util.Alert("SFU1893");      //제품ID가 같지 않습니다.
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    if (SearchResult.Rows.Count < 1)
                    {
                        LotList.Add(sLotId);
                    }
                    else
                    {
                        // NND End시에도 처리 가능하도록 변경
                        if (Util.NVC(SearchResult.Rows[0]["PROCID"]).Equals(Process.NOTCHING))
                        {
                            if (!(SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT") || SearchResult.Rows[0]["WIPSTAT"].ToString().Equals(Wip_State.END)))
                            {
                                //Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                LotList.Add(sLotId);
                                return true;
                            }
                        }
                        else
                        {
                            if (!SearchResult.Rows[0]["WIPSTAT"].ToString().Equals("WAIT"))
                            {
                                //Util.Alert("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                                LotList.Add(sLotId);
                                return true;
                            }
                        }
                        
                        if (SearchResult.Rows[0]["EQSGID"].ToString().Equals(Util.GetCondition(cboToEquipmentSegment)))
                        {
                            //Util.Alert("SFU1653");  //선택된 라인의 재공입니다.
                            LotList.Add(sLotId);
                            return true;
                        }

                        //HOLD 재공 이동 처리 가능하도록 수정 요청 (폴란드3동 요구 사항)
                        //if (SearchResult.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                        //{
                        //    ////Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                        //    LotList.Add(sLotId);
                        //    return true;
                        //}

                        #region # 특별관리 Lot - 인계가능 라인 체크
                        if (string.Equals(Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]), "Y") && !string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"])))
                        {
                            string SpclLine = Util.NVC(SearchResult.Rows[0]["RSV_EQSGID_LIST"]);
                            string sLine = Util.GetCondition(cboToEquipmentSegment);

                            if (!SpclLine.Contains(sLine))
                            {
                                Util.MessageInfo("SFU8215", new string[] { Util.NVC(SearchResult.Rows[0]["LOTID"]), SpclLine });  // Lot [%1]의 지정된 라인[%2]을 선택하세요.
                                return false;
                            }
                        }
                        #endregion

                        for (int i = 0; i < dgMoveList.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "LOTID").ToString() == sLotId)
                            {
                                //Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                                LotList.Add(sLotId);
                                return true;
                            }

                            #region # 특별관리 Lot - 혼입 체크 
                            if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) != Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]))
                            {
                                if ((Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[i].DataItem, "SPCL_FLAG")) == "Y") || (Util.NVC(SearchResult.Rows[0]["SPCL_FLAG"]) == "Y"))
                                {
                                    Util.Alert("SFU8214");  // 특별관리 Lot과 일반 Lot을 같이 이동할 수 없습니다.
                                    return false;
                                }
                            }
                            #endregion
                        }

                        if (dgMoveList.GetRowCount() > 1)
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PROCID").ToString() != SearchResult.Rows[0]["PROCID"].ToString())
                            {
                                //Util.Alert("SFU1789");  //이전에 스캔한 LOT의 공정과 다릅니다.
                                LotList.Add(sLotId);
                                return true;
                            }
                        }

                        if (dgMoveList.GetRowCount() == 0)
                        {
                            dgMoveList.ItemsSource = DataTableConverter.Convert(SearchResult);
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PRODID").ToString() == SearchResult.Rows[0]["PRODID"].ToString())
                            {
                                //2021.08.19 : LOT유형의 시험생산구분코드(PILOT_PROD_DIVS_CODE)Validation
                                if (Util.NVC(DataTableConverter.GetValue(dgMoveList.Rows[0].DataItem, "PILOT_PROD_DIVS_CODE")).Equals(SearchResult.Rows[0]["PILOT_PROD_DIVS_CODE"].ToString()))
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgMoveList.ItemsSource);
                                    dtSource.Merge(SearchResult);

                                    Util.gridClear(dgMoveList);
                                    dgMoveList.ItemsSource = DataTableConverter.Convert(dtSource);
                                }
                                else
                                {
                                    //Util.MessageValidation("SFU8187");  //Lot유형의 시생산구분코드가 동일한 제품만 함께 이동할 수 있습니다.
                                    LotList.Add(sLotId);
                                }
                            }
                            else
                            {
                                //Util.Alert("SFU1893");      //제품ID가 같지 않습니다.
                                LotList.Add(sLotId);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                txtLotID_Out.Text = "";
                txtLotID_Out.Focus();
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        private void dgMove_MasterChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgMove_Master.BeginEdit();
                //    dgMove_Master.ItemsSource = DataTableConverter.Convert(dt);
                //    dgMove_Master.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgMove_Master.SelectedIndex = idx;

                SearchMoveLOTList(idx);
            }
            //else
            //{
            //    Util.gridClear(dgMove_Detail);
            //}
        }

        private void txtProd_ID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID.Text != "")
                {
                    Search_data();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        #region [상세이력조회]        
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Detail_Hist();
        }

        private void txtProd_ID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtProd_ID2.Text != "")
                {
                    Detail_Hist();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtLotid.Text != "")
                {
                    Detail_Hist();
                }
                else
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }
            }
        }

        private void Detail_Hist()
        {
            try
            {
                Util.gridClear(dgMove_Line);

                string sStart_date = Util.GetCondition(dtpDateFrom2);
                string sEnd_date = Util.GetCondition(dtpDateTo2); string.Format("{0:yyyyMMdd}", dtpDateTo2.SelectedDateTime);
                string sArea = LoginInfo.CFG_AREA_ID;
                string sEqsg = Util.GetCondition(cboHistToEquipmentSegment2, bAllNull: true);
                string sEqsgFrom = Util.GetCondition(cboHistFromEquipmentSegment2, bAllNull: true);


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_EQSGID", typeof(String));
                RQSTDT.Columns.Add("TO_AREAID", typeof(String));
                RQSTDT.Columns.Add("TO_EQSGID", typeof(String));
                RQSTDT.Columns.Add("MOVE_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("MOVE_ORD_STAT_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_EQSGID"] = sEqsgFrom;
                dr["TO_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["TO_EQSGID"] = sEqsg;
                dr["MOVE_TYPE_CODE"] = "MOVE_LINE";
                dr["MOVE_ORD_STAT_CODE"] = Util.GetCondition(cboStat2, bAllNull: true);
                dr["PRODID"] = txtProd_ID2.Text == "" ? null : txtProd_ID2.Text;
                dr["LOTID"] = txtLotid.Text == "" ? null : txtLotid.Text;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_HIST_MASTER_LINE_DTL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgMove_Line, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        private void cboHistFromEquipmentSegment_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            SetCboHistToEquipmentSegment();
        }
    }
}

