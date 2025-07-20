/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

**************************************************************************************/

using C1.WPF;
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_101 : UserControl, IWorkArea
    {        

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        private bool pqc_Request_Ng_Chk = false;
        private bool pqc_Search_Flag = false;
        int dtRow_Qty = 0;

        public PACK001_101()
        {
            InitializeComponent();
            Loaded += PACK001_101_Loaded;
        }

        private void PACK001_101_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PACK001_101_Loaded;
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQARequest);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            SetComboBox();
            limit_Pack_qty();
            txtWorkerName.Text = LoginInfo.USERNAME;
        }
        
        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion
        private void SetComboBox()
        {
            //날짜 콤보 셋
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;


            dtpSearch_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpSearch_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            //dtpSearch_ShipDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            // Area 셋팅
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboPrjModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter, cbChild: cboAreaChild, sCase: "AREA_AREATYPE");

            //작업자 조회 라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboPrjModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");

            C1ComboBox[] cboPrjModelParent = { cboArea, cboEquipmentSegment };
            _combo.SetCombo(cboPrjModel, CommonCombo.ComboStatus.ALL, cbParent: cboPrjModelParent, sCase: "PRJ_MODEL");

            //포장상태 Combo Set.
            string[] sFilter1 = { "PRDT_CLSS_CODE_PACK", "CMA,BMA" };
            _combo.SetCombo(cboPrdtClss, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "PRDT_CLSS_CODE_PACK");



            //검사의뢰이력조회


            // Area 셋팅
            String[] sFilter_AreaSearch = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            C1ComboBox[] cboSearch_AreaChild = { cboSearch_EquipmentSegment, cboSearch_PrjModel };
            _combo.SetCombo(cboSearch_Area, CommonCombo.ComboStatus.NONE, sFilter: sFilter_AreaSearch, cbChild: cboSearch_AreaChild, sCase: "AREA_AREATYPE");

            //조회 라인
            C1ComboBox[] cboSearch_EquipmentSegmentParent = { cboSearch_Area };
            C1ComboBox[] cboSearch_EquipmentSegmentChild = { cboSearch_PrjModel };
            _combo.SetCombo(cboSearch_EquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboSearch_EquipmentSegmentParent, cbChild: cboSearch_EquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");


            C1ComboBox[] cboSearch_PrjModelParent = { cboSearch_Area, cboSearch_EquipmentSegment };
            _combo.SetCombo(cboSearch_PrjModel, CommonCombo.ComboStatus.ALL, cbParent: cboSearch_PrjModelParent, sCase: "PRJ_MODEL");

            //판졍결과 Combo Set.
            string[] sFilterJUDGE = { "QMS_INSP_JUDGE_VALUE" };
            _combo.SetCombo(cboSearch_Judge_Value, CommonCombo.ComboStatus.ALL, sFilter: sFilterJUDGE, sCase: "COMMCODE");
        }

        private void limit_Pack_qty()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "LIMITED_QTY_PACK";
            dr["CBO_CODE"] = "LIMIT_PQC_REQUEST_QTY";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

            int Limit_qty = Int32.Parse(dtResult.Rows[0]["ATTRIBUTE2"].ToString());

            txtLimitQty.Value = Limit_qty;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            smplChk.Visibility = Visibility.Collapsed;
            btnSelect_All.Visibility = Visibility.Collapsed;
            btnDelete_All.Visibility = Visibility.Collapsed;
            pqc_Search_Flag = true;

            DateTime fromTime = dtpDateFrom.SelectedDateTime.Date;
            DateTime toTime = dtpDateTo.SelectedDateTime.Date.AddDays(1);
            int diff = (toTime - fromTime).Days;

            if (diff > 30)
            {
                Util.MessageInfo("SFU4466");
                return;              
            }

            GetSearchInfo(null,pqc_Search_Flag);

            txtSmplLotID.Text = "";

            Util.gridClear(dgPqcRequest);

            txtSelLotQty.Value = 0;
        }

        private void txtSmplLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    smplChk.Visibility = Visibility.Visible;
                    btnSelect_All.Visibility = Visibility.Visible;
                    btnDelete_All.Visibility = Visibility.Visible;
                    if (pqc_Search_Flag == true)
                    {
                        pqcRequest_gridClear();
                    }
                        pqcRequest_SearchInterLock(txtSmplLotID.Text);
                    if (pqc_Request_Ng_Chk == false)
                    {                                           
                        pqc_Search_Flag = false;                        
                        GetSearchInfo(txtSmplLotID.Text, pqc_Search_Flag);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void txtSmplLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
        //    {
        //        try
        //        {
        //            string[] stringSeparators = new string[] { "\r\n" };
        //            string sPasteString = Clipboard.GetText();
        //            string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
        //            int LotListCount = sPasteStrings.Length;

        //            smplChk.Visibility = Visibility.Visible;
        //            btnSelect_All.Visibility = Visibility.Visible;
        //            btnDelete_All.Visibility = Visibility.Visible;

        //            if (sPasteStrings.Count() >= 20)
        //            {
        //                Util.MessageValidation("FM_ME_0463");   //최대 수량은 20개 입니다.
        //                return;
        //            }

        //            string lotList = string.Empty;
        //            for (int i = 0; i < LotListCount-1; i++)
        //            {
                        
        //               pqcRequest_SearchInterLock(sPasteStrings[i]);
        //               if (pqc_Request_Ng_Chk == true)
        //               {
        //                   LotListCount = 0;
        //                    lotList = "";
        //                   return;
        //               }
        //                    if (i == 0)
        //               {
        //                   lotList = sPasteStrings[i];
        //               }
        //               else
        //               {
        //                   lotList = lotList + "," + sPasteStrings[i];
        //               }
        //               System.Windows.Forms.Application.DoEvents();
        //            }
        //            if (!lotList.ToString().Equals(""))
        //            {
        //                GetSearchInfo(lotList);
        //            }
                                                                            
        //        }
        //        catch (Exception ex)
        //        {
        //            Util.MessageException(ex);
        //            return;
        //        }

        //        e.Handled = true;
        //    }

        //}

        private void pqcRequest_SearchInterLock(string lotid)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(lotid))
                {
                    //LOT ID를 스캔해주세요.
                    Util.MessageValidation("SFU2814");
                    pqc_Request_Ng_Chk = true;
                    pqcRequest_gridClear();
                    txtSmplLotID.Focus();
                    return;
                }
           
                if (dgPqcInfo.ItemsSource != null)
                {
                    dtRow_Qty = ((DataView)dgPqcInfo.ItemsSource).ToTable().Rows.Count;

                    if (!string.IsNullOrEmpty(lotid))
                {
                        if (txtLimitQty.Value <= dtRow_Qty)
                        {
                            Util.MessageValidation("SFU5950");   //입력 가능한 수량을 초과하였습니다.
                    pqc_Request_Ng_Chk = true;
                    return;
                }
                    }

                    if (((DataView)dgPqcInfo.ItemsSource).ToTable().Select("LOTID = '" + lotid + "'").Length > 0)   //중복조건 체크
                {
                        Util.MessageValidation("SFU1376", lotid); //중복 스캔되었습니다.
                        lotid = string.Empty;
                        pqc_Request_Ng_Chk = true;
                        txtSmplLotID.Focus();
                        return;
                    }
                }                

                DataSet DATASET = new DataSet();

                DataTable RQSTDT = DATASET.Tables.Add("INDATA");                                                
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataTable RQSTDT2 = DATASET.Tables.Add("SCAN_CHK_INDATA");                
                RQSTDT2.Columns.Add("PRODID", typeof(string));
                RQSTDT2.Columns.Add("EQSGID", typeof(string));                

                DataRow INDATA = RQSTDT.NewRow();
                DataRow SCAN_CHK_INDATA = RQSTDT2.NewRow();

                if (dtRow_Qty > 0)
                {                                        
                    SCAN_CHK_INDATA["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgPqcInfo.Rows[0].DataItem, "PRODID"));
                    SCAN_CHK_INDATA["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgPqcInfo.Rows[0].DataItem, "EQSGID"));                    
                }
                else
                {                                        
                    SCAN_CHK_INDATA["PRODID"] = null;
                    SCAN_CHK_INDATA["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                }
                DATASET.Tables["SCAN_CHK_INDATA"].Rows.Add(SCAN_CHK_INDATA);
                INDATA["LOTID"] = lotid;

                DATASET.Tables["INDATA"].Rows.Add(INDATA);

                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SMPL_LOT_INSP_PQC_PACK", "INDATA, SCAN_CHK_INDATA", "RSLTDT", DATASET);

                dtRow_Qty = 0;
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SMPL_LOT_INSP_PQC_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (result.Tables.Count > 0)
                {
                    pqc_Request_Ng_Chk = false;
                }
                else
                {
                    pqc_Request_Ng_Chk = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                pqc_Request_Ng_Chk = true;                
            }            
        }
        #region Event - [1탭] PQC 제품검사 대상 조회
        private void GetSearchInfo(string Lotid,bool search_flag)
        {
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MODELID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = !string.IsNullOrWhiteSpace(Lotid) ? null : dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TO_DTTM"] = !string.IsNullOrWhiteSpace(Lotid) ? null : dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["AREAID"] = Util.NVC(cboArea.SelectedValue) == "" ? null : Util.NVC(cboArea.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["MODELID"] = Util.NVC(cboPrjModel.SelectedValue) == "" ? null : Util.NVC(cboPrjModel.SelectedValue);
                dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClss.SelectedValue) == "" ? null : Util.NVC(cboPrdtClss.SelectedValue);
                dr["LOTID"] = string.IsNullOrWhiteSpace(Lotid) ? null : Lotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_SEL_PQC_LOT_PACK", "INDATA", "OUTDATA", RQSTDT);
                if (1 > dtResult.Rows.Count && !string.IsNullOrEmpty(Lotid))
                {
                    Util.MessageInfo("SFU7000", Lotid);
                    return;
                }

                if (!string.IsNullOrEmpty(Lotid) && dgPqcInfo.Rows.Count >= 1)
                {                    
                    dgPqcInfo.IsReadOnly = false;
                    dgPqcInfo.BeginNewRow();
                    dgPqcInfo.EndNewRow(true);
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "CHK", dtResult.Rows[0]["CHK"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "LOTID", dtResult.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "UPDDTTM", dtResult.Rows[0]["UPDDTTM"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "PRODID", dtResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "PRJT_NAME", dtResult.Rows[0]["PRJT_NAME"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "MODLID", dtResult.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "AREAID", dtResult.Rows[0]["AREAID"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "EQSGNAME", dtResult.Rows[0]["EQSGNAME"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "EQSGID", dtResult.Rows[0]["EQSGID"].ToString());
                    DataTableConverter.SetValue(dgPqcInfo.CurrentRow.DataItem, "LOT_COUNT", dtResult.Rows[0]["LOT_COUNT"].ToString());
                    dgPqcInfo.IsReadOnly = true;                    
                }
                else
                {
                    Util.GridSetData(dgPqcInfo, dtResult, FrameOperation, true);
                    //dgPqcInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                }

                //하단 text박스 count 셋
                int LotCount = dgPqcInfo.Rows.Count;
                txtLotQty.Value = LotCount;

            }
            catch (Exception ex)
            {                
                Util.MessageException(ex);
            }
        }
        #endregion
      

        private void btnSelect_All_Click(object sender, RoutedEventArgs e)
        {
            string sLotid = string.Empty;
            try
            {
                DataTable dtdgPqcInfo = ((DataView)dgPqcInfo.ItemsSource).ToTable();
                dtdgPqcInfo.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                txtSelLotQty.Value = txtLotQty.Value;
                Util.GridSetData(dgPqcRequest, dtdgPqcInfo, FrameOperation, true);
                Util.GridSetData(dgPqcInfo, dtdgPqcInfo, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }
        private void chk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;

                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(true))//체크되는 경우
                {
                    DataTable dtTo = DataTableConverter.Convert(dgPqcRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("UPDDTTM", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRJT_NAME", typeof(string));
                        dtTo.Columns.Add("MODLID", typeof(string));
                        dtTo.Columns.Add("AREAID", typeof(string));                        
                        dtTo.Columns.Add("EQSGID", typeof(string));                        
                    }

                    if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0) //중복조건 체크
                    {
                        return;
                    }

                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    txtSelLotQty.Value = txtSelLotQty.Value + 1;
                    dtTo.Rows.Add(dr);
                    dgPqcRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else//체크 풀릴때
                {
                    DataTable dtTo = DataTableConverter.Convert(dgPqcRequest.ItemsSource);

                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);
                    txtSelLotQty.Value = txtSelLotQty.Value - 1;
                    dgPqcRequest.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGrid1CheckBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if(this.dgQAReqHis.CurrentColumn == null || this.dgQAReqHis.CurrentRow == null)
                {
                    return;
                }
                if (!this.dgQAReqHis.CurrentColumn.Name.ToUpper().Equals("CHK"))
                {
                    return;
                }
                if(this.dgQAReqHis.GetRowCount() <= 0)
                {
                    return;
                }
                int currentRowIndex = this.dgQAReqHis.CurrentRow.Index;

                //체크값이 True인 경우 다른 Row의 있는 내용은 False
                for(int i = 0; i < this.dgQAReqHis.GetRowCount(); i++)
                {
                    if (!i.Equals(currentRowIndex))
                    {
                        DataTableConverter.SetValue(this.dgQAReqHis.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //CHECKED 사용했을때 해본것
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                CheckBox cb = sender as CheckBox;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).DataGrid;
                object objRowIdx = dgQAReqHis.Rows[idx].DataItem;

                if (objRowIdx != null)
                {
                    if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals(true))
                    {
                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            if (i != idx)
                            {
                                DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                            }
                        }
                    }                
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 전송정보 삭제버튼 클릭시 동작하는 이벤트
        /// </summary>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            string sLotid = Util.NVC(dgPqcRequest.GetCell(iRow, dgPqcRequest.Columns["LOTID"].Index).Value);
            //삭제시 LotID 조회 시트상 해당 LotID 체크박스 해제
            for (int i = 0; i < dgPqcInfo.GetRowCount(); i++)
            {
                if (sLotid == Util.NVC(dgPqcInfo.GetCell(i, dgPqcInfo.Columns["LOTID"].Index).Value))
                {
                    DataTableConverter.SetValue(dgPqcInfo.Rows[i].DataItem, "CHK", false);
                }
            }
            // 선택된 행 삭제
            dgPqcRequest.IsReadOnly = false;
            dgPqcRequest.RemoveRow(iRow);
            txtSelLotQty.Value = txtSelLotQty.Value - 1;
            dgPqcRequest.IsReadOnly = true;

            //하단 text박스 count 셋
            DataTable dtResult = DataTableConverter.Convert(dgPqcRequest.ItemsSource);

        }

        private void btnDelete_All_Click(object sender, RoutedEventArgs e)
        {
            for (int iRow = dgPqcRequest.Rows.Count - 1; iRow > -1; iRow--)
            {
                //System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

                string sLotid = Util.NVC(dgPqcRequest.GetCell(iRow, dgPqcRequest.Columns["LOTID"].Index).Value);
                //삭제시 Pallet 조회 시트상 해당 PalletID 체크박스 해제
                for (int i = 0; i < dgPqcInfo.GetRowCount(); i++)
                {
                    if (sLotid == Util.NVC(dgPqcInfo.GetCell(i, dgPqcInfo.Columns["LOTID"].Index).Value))
                    {
                        DataTableConverter.SetValue(dgPqcInfo.Rows[i].DataItem, "CHK", false);
                        break;
                    }
                }
                // 선택된 행 삭제
                dgPqcRequest.IsReadOnly = false;
                txtSelLotQty.Value = 0;
                dgPqcRequest.RemoveRow(iRow);
                dgPqcRequest.IsReadOnly = true;
            }

            //하단 text박스 count 셋
            DataTable dtResult = DataTableConverter.Convert(dgPqcRequest.ItemsSource);

        }

        private void pqcRequest_gridClear()
        {
            txtSelLotQty.Value = 0;
            txtLotQty.Value = 0;
            Util.gridClear(dgPqcInfo);
            Util.gridClear(dgPqcRequest);
        }


        /// <summary>
        /// QA 검사의뢰
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQARequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgPqcRequest.GetRowCount() <= 0)
            {
                ms.AlertWarning("SFU4154"); //의뢰할 데이터가 없습니다.
                return;
            }

            QARequestSend();
        }

        private void QARequestSend()
        {
            try
            {
                DataTable dtQaTagetData = DataTableConverter.Convert(dgPqcRequest.ItemsSource);
                DataSet DATASET = new DataSet();
                DataTable RQSTDT = DATASET.Tables.Add("INDATA");
                
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow INDATA = RQSTDT.NewRow();                

                INDATA["LANGID"] = LoginInfo.LANGID;
                INDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                INDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                INDATA["USERID"] = LoginInfo.USERID;

                DATASET.Tables["INDATA"].Rows.Add(INDATA);

                DataTable RQSTDT2 = DATASET.Tables.Add("IN_LOT");
                RQSTDT2.Columns.Add("LOTID", typeof(string));
                RQSTDT2.Columns.Add("AREAID", typeof(string));
                RQSTDT2.Columns.Add("EQSGID", typeof(string));
                RQSTDT2.Columns.Add("PRODID", typeof(string));
                RQSTDT2.Columns.Add("MODLID", typeof(string));
                
                for (int i = 0; i < dtQaTagetData.Rows.Count; i++)
                {
                    DataRow IN_LOT = RQSTDT2.NewRow();
                    IN_LOT["LOTID"]  = Util.NVC(dtQaTagetData.Rows[i]["LOTID"]);
                    IN_LOT["AREAID"] = Util.NVC(dtQaTagetData.Rows[i]["AREAID"]);
                    IN_LOT["EQSGID"] = Util.NVC(dtQaTagetData.Rows[i]["EQSGID"]);
                    IN_LOT["PRODID"] = Util.NVC(dtQaTagetData.Rows[i]["PRODID"]);
                    IN_LOT["MODLID"] = Util.NVC(dtQaTagetData.Rows[i]["MODLID"]);

                    DATASET.Tables["IN_LOT"].Rows.Add(IN_LOT);
                                      
                }
                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SMPL_LOT_INSP_REQUEST_PQC_PACK", "INDATA,IN_LOT", null, DATASET);
                Util.MessageInfo("FM_ME_0186");
                pqcRequest_gridClear();
            }
            catch (Exception ex)
            {                
                Util.MessageException(ex);
            }
        }

        

        private void btnShtInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pqcRequest_gridClear();
                }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }        

        private void btnQASearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                DateTime his_fromTime = dtpSearch_DateFrom.SelectedDateTime.Date;
                DateTime his_toTime = dtpSearch_DateTo.SelectedDateTime.Date.AddDays(1);
                int diff = (his_toTime - his_fromTime).Days;

                if (diff > 90 && string.IsNullOrWhiteSpace(txtSearch_LotID.Text.Trim()))
                {
                    Util.MessageInfo("SFU5033", "3");
                    return;
        }

                GetQAReqHis();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }


        }
        //검사이력
        private void txtSearch_LotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtSearch_LotID.Text))
                    {
                        GetQAReqHis();
                    }else
                    {
                        Util.MessageInfo("SFU1813");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }
        }

        //검사이력 요청취소 [GQMS 요청으로 기능 제외 현업 박진우 선임님 협의함]
        //private void dgQAReqHis_Chk_Click(object sender, RoutedEventArgs e)
        //{
        //    List<DataRow> drList = DataTableConverter.Convert(dgQAReqHis.ItemsSource).Select("CHK = '1'").ToList();
        //    if(drList.Count >= 20)
        //    {
        //        Util.MessageValidation("FM_ME_0463");   //최대 수량은 20개 입니다.
        //        DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;
        //        DataTableConverter.SetValue(dgQAReqHis.Rows[dataRow].DataItem, "CHK", false);
        //        return;
        //    }                                   
        //}

        private void GetQAReqHis()
        {
            try
            {                
                string sLotID = txtSearch_LotID.Text.Trim();
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRJT_MODEL", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));                                
                RQSTDT.Columns.Add("JUDG_FLAG", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"]     = LoginInfo.LANGID;
                dr["AREAID"]     = Util.NVC(cboSearch_Area.SelectedValue);
                dr["EQSGID"]     = Util.NVC(cboSearch_EquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboSearch_EquipmentSegment.SelectedValue);
                dr["PRJT_MODEL"] = Util.NVC(cboSearch_PrjModel.SelectedValue) == "" ? null : Util.NVC(cboSearch_PrjModel.SelectedValue);
                dr["FROM_DATE"]  = !string.IsNullOrWhiteSpace(sLotID) ? null : dtpSearch_DateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"]    = !string.IsNullOrWhiteSpace(sLotID) ? null : dtpSearch_DateTo.SelectedDateTime.ToString("yyyyMMdd");                                
                dr["JUDG_FLAG"]  = Util.NVC(cboSearch_Judge_Value.SelectedValue) == "" ? null : Util.NVC(cboSearch_Judge_Value.SelectedValue);
                dr["LOTID"]      = string.IsNullOrWhiteSpace(sLotID) ? null : sLotID;
                
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_GET_SEL_PQC_LOT_PACK_HIST", "INDATA", "OUTDATA", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;                    
                    Util.GridSetData(dgQAReqHis, dtResult, FrameOperation, true);                              

                    if (ex != null)
                    {                        
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //검사이력 요청취소 [GQMS 요청으로 기능 제외 현업 박진우 선임님 협의함]
        private void btnQACancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //SFU1168 작업을 취소하시겠습니까? 
                Util.MessageConfirm("SFU1168", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        if (_util.GetDataGridCheckCnt(dgQAReqHis, "CHK") < 1)
                        {
                            Util.MessageValidation("SFU1636"); //선택된 대상이 없습니다.
                            return;
                        }
                        
                        List<DataRow> drList = DataTableConverter.Convert(dgQAReqHis.ItemsSource).Select("CHK = 'True'").ToList();
                        if (drList.Count <= 0)
                        {
                            Util.MessageValidation("SFU1939"); //취소 할 수 있는 상태가 아닙니다.	
                            return;
                        }

                        DataTable dtInfo = drList.CopyToDataTable();
                        List<string> sIdList = dtInfo.AsEnumerable().Select(c => c.Field<string>("INSP_REQ_ID")).Distinct().ToList();

                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("LANGID", typeof(string));
                        dtIndata.Columns.Add("SRCTYPE", typeof(string));
                        dtIndata.Columns.Add("SHOPID", typeof(string));
                        dtIndata.Columns.Add("INSP_REQ_ID", typeof(string));
                        dtIndata.Columns.Add("USERID", typeof(string));

                        foreach (string req_id in sIdList)
                        {
                            DataRow drIndata = dtIndata.NewRow();
                            drIndata["LANGID"] = LoginInfo.LANGID;
                            drIndata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            drIndata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            drIndata["INSP_REQ_ID"] = req_id;
                            drIndata["USERID"] = LoginInfo.USERID;
                            dtIndata.Rows.Add(drIndata);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_PQC_LOT_PACK", "INDATA", null, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            else
                            {
                                Util.MessageInfo("SFU1747");// 요청되었습니다.	
                                GetQAReqHis();
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }


        //2018-01-30 Start ============================================================================


        private void SetText_SearchResultCount(C1NumericBox txtLotCount, DataTable dtResult)
        {
            try
            {
                int lSumLot = 0;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {

                    object oLotQty = dtResult.Compute("Sum(LOT_COUNT)", "");
                    int.TryParse(oLotQty.ToString(), out lSumLot);
                }


                txtLotCount.Value = lSumLot;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
