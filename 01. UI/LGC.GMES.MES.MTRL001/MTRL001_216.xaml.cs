/*************************************************************************************
 Created Date : 2024.09.12
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.12  DEVELOPER : Initial Created.
  2025.01.15  조범모     멀티선택 기능 추가 및 EWM으로 자재 반품 요청 처리
  2025.02.19  조범모     입고창고를 무조건 선택할 수 있도록 수정 (As-Is: 결재모드에서만 사용)
  2025.03.05  박승민     '2025.02.19' 기준 ROLLBACK
  2025.04.10  조범모     소포장 그룹 LOT 일괄반품 처리 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_216 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;
        public MTRL001_216()
        {
            InitializeComponent();
            InitCombo();

            dtpEndT2.SelectedDateTime = DateTime.Today;
            dtpStartT2.SelectedDateTime = DateTime.Today.AddDays(-31);

            
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            try
            {
                //자재그룹
                SetRawMaterialGroupCombo(cboMaterialGroup);
                SetRawMaterialGroupCombo(cboMaterialGroup2);

                //자재조회
                SearchMtrl_Group();

                //입고창고
                SetInputArgWHCombo(cboInputArgWH);

                //출고창고
                SetOutputArgWHCombo(cboOutputArgWH);

                //자재 유형
                SetMtrlType(cboMaterialType);
                SetMtrlType(cboMaterialType2);

                //반퓸 유형 - 결재 여부 체크
                {
                    //동별공통코드 : INV_MTRL_REQ_LOC/EWM/ATTRIBUTE3가 "Y"이면 반품승인모드로 작동하고, 값이 없거나 "N"이면 기존 LT31T전송하는 로직으로 작동
                    SetIssuType(cboIssuType);

                    if (cboIssuType.GetBindValue() != null)
                    {
                        if (cboIssuType.SelectedItem.GetValue("ATTRIBUTE3").ToString().Equals("Y"))
                        {
                            cboReturnText.Visibility = Visibility.Visible;
                            cboReturnType.Visibility = Visibility.Visible;
                            SetReturnType(cboReturnType);

                            HISTORY.Visibility = Visibility.Visible;
                        }
                    }
                }

                //반퓸 상태
                SetReturnStatus(cboReturnStatus);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        #region 자재 리스트 버튼 조회 : btnSearch()
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            GetMtrlList();
        }
        private void btnSearch2(object sender, RoutedEventArgs e)
        {
            GetMtrlList2();
        }
        #endregion

        #region 반품입고 처리 버튼 : btnSearch()
        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            //반품입고 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU2808"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Process();
                        }
                    });
        }

        #endregion

        #region 자재ID 조회 : popSearchMtrl_ValueChanged()

        /// <summary>
        /// 자재 정보 조회
        /// </summary>
        private void popSearchMtrl_ValueChanged(object sender, EventArgs e)
        {
            PopupFindControl popSearchMtrl = sender as PopupFindControl;

            try
            {
                if (popSearchMtrl.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrl.SelectedValue.ToString()))
                {
                    popSearchMtrl.SelectedValue = null;
                    popSearchMtrl.SelectedText = null;
                    popSearchMtrl.ItemsSource = null;

                }
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                inDataTable.Columns.Add("MTGRID", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                newRow["MTGRID"] = cboMaterialGroup.GetBindValue();
                newRow["MTRLID"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);

                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        { }

        //반품입고 요청취소 처리
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Button rb = sender as Button;
            if (DataTableConverter.GetValue(rb.DataContext, "RTN_CANCEL").Equals("Visible"))
            {
                //반품취소하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3258"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                , (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            const string bizRuleName = "BR_INV_REG_RETURN_STATE_CHANGE";

                            DataSet inDataSet = new DataSet();

                            #region INDATA
                            DataTable inTable = new DataTable("INDATA");
                            {
                                inTable.Columns.Add("LANGID", typeof(string));
                                inTable.Columns.Add("USERID", typeof(string));
                                inTable.Columns.Add("CANCEL_REASON_DETL", typeof(string));
                                inTable.Columns.Add("TO_STAT_CODE", typeof(string));

                                DataRow newRow = inTable.NewRow();
                                newRow["LANGID"] = LoginInfo.LANGID;
                                newRow["USERID"] = LoginInfo.USERID;
                                newRow["CANCEL_REASON_DETL"] = "";
                                newRow["TO_STAT_CODE"] = "C";

                                inTable.Rows.Add(newRow);
                            }
                            inDataSet.Tables.Add(inTable);
                            #endregion

                            #region REQUEST_LIST
                            DataTable inTable2 = new DataTable("REQUEST_LIST");
                            {
                                inTable2.Columns.Add("REQ_ID", typeof(string));
                                {
                                    DataRow newRow = inTable2.NewRow();
                                    newRow["REQ_ID"] = DataTableConverter.GetValue(rb.DataContext, "REQ_ID").GetString();
                                    inTable2.AddDataRow(newRow);
                                }
                            }
                            inDataSet.Tables.Add(inTable2);
                            #endregion

                            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,REQUEST_LIST", null, (result2, bizException) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    GetMtrlList2();

                                    //Util.AlertInfo("정상 처리 되었습니다.");
                                    Util.MessageInfo("SFU1275");
                                }
                                catch (Exception ex)
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    Util.MessageException(ex);
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

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMtrlList();
            }
        }
        private void txtCarrierId2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetMtrlList2();
            }
        }
        private void dgMtrlList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgMtrlList.TopRows.Count; i < dgMtrlList.Rows.Count; i++)
                {

                    if (dgMtrlList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgMtrlList.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idxS, dgMtrlList.Columns["CHK"].Index), dgMtrlList.GetCell(idxE, dgMtrlList.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idxS, dgMtrlList.Columns["REP_PROCESSING_GROUP_ID"].Index), dgMtrlList.GetCell(idxE, dgMtrlList.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idxS, dgMtrlList.Columns["CHK"].Index), dgMtrlList.GetCell(idxE, dgMtrlList.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList.GetCell(idxS, dgMtrlList.Columns["REP_PROCESSING_GROUP_ID"].Index), dgMtrlList.GetCell(idxE, dgMtrlList.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgMtrlList.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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
        private void dgMtrlList2_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgMtrlList2.TopRows.Count; i < dgMtrlList2.Rows.Count; i++)
                {
                    if (dgMtrlList2.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgMtrlList2.Rows[i].DataItem, "REQ_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgMtrlList2.Rows[i].DataItem, "REQ_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgMtrlList2.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REQ_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REQ_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REQ_SLOC_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REQ_SLOC_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["TO_SLOC_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["TO_SLOC_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["CREATION_DATE"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["CREATION_DATE"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REASON_DETAIL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REASON_DETAIL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["STATUS_NM"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["STATUS_NM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["RTN_CANCEL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["RTN_CANCEL"].Index)));
                                    e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["PRINT_LABEL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["PRINT_LABEL"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REQ_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REQ_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REQ_SLOC_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REQ_SLOC_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["TO_SLOC_ID"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["TO_SLOC_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["CREATION_DATE"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["CREATION_DATE"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["REASON_DETAIL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["REASON_DETAIL"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["STATUS_NM"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["STATUS_NM"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["RTN_CANCEL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["RTN_CANCEL"].Index)));
                                e.Merge(new DataGridCellsRange(dgMtrlList2.GetCell(idxS, dgMtrlList2.Columns["PRINT_LABEL"].Index), dgMtrlList2.GetCell(idxE, dgMtrlList2.Columns["PRINT_LABEL"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgMtrlList2.Rows[i].DataItem, "REQ_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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

        //라벨발행
        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
            Button rb = sender as Button;
            if (DataTableConverter.GetValue(rb.DataContext, "PRINT_LABEL").Equals("Visible"))
            {
                //바코드 발행하시겠습니까?
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1540"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                , (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {

                            DataTable dt = new DataTable();
                            

                            // DATA Table
                            DataSet inDataSet = new DataSet();
                            DataTable inTable = inDataSet.Tables.Add("INDATA");
                            inTable.Columns.Add("LANGID", typeof(string));
                            inTable.Columns.Add("REQ_ID", typeof(string));
                            inTable.Columns.Add("USERID", typeof(string));


                            DataRow newRow = inTable.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["REQ_ID"] = DataTableConverter.GetValue(rb.DataContext, "REQ_ID").GetString();
                            newRow["USERID"] = LoginInfo.USERID;

                            inTable.Rows.Add(newRow);

                           
                            new ClientProxy().ExecuteService_Multi("BR_INV_REG_MTRL_RETURN_LABEL", "INDATA", "OUTDATA,RETURN_INFO", (bizResult, bizException) =>
                            {


                                try
                                {
                                    if (bizException != null)
                                    {
                                        SetLabelHistory(DataTableConverter.GetValue(rb.DataContext, "REQ_ID").GetString(), "N", Convert.ToInt32(bizResult.Tables["RETURN_INFO"].Rows[0]["PRT_SEQ"].ToString()), bizException.ToString());
                                        return;
                                    }

                                    // ZPL 코드 생성
                                    if (bizResult.Tables["RETURN_INFO"].Rows.Count > 0)
                                    {
                                        if (PrintZPL(bizResult.Tables["RETURN_INFO"].Rows[0]["RET_LABEL_ZPL_CNTT"].ToString(), drPrtInfo))
                                        {
                                            //바코드 발행 이력저장
                                            SetLabelHistory(DataTableConverter.GetValue(rb.DataContext, "REQ_ID").GetString(), "Y", Convert.ToInt32(bizResult.Tables["RETURN_INFO"].Rows[0]["PRT_SEQ"].ToString()), string.Empty);
                                        }

                                    }

                                }
                                catch (Exception ex)
                                {

                                    Util.MessageException(ex);
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


        #endregion

        #region Mehod

        #region 자재그룹
        private void SetRawMaterialGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_MTGRID_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void cboMaterialGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SearchMtrl_Group();
        }
        #endregion

        #region 입고 처리시 사용 창고 설정 : SetInputWHCombo()
        private void SetInputArgWHCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE9" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_WH_LOCATE", "Y", "IN" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetOutputArgWHCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE9" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_WH_LOCATE", "Y", "OUT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region 자재/반품요청 리스트 조회 : GetMtrlList()/GetMtrlList2()

        /// <summary>
        /// 자재 리스트 조회
        /// </summary>
        private void GetMtrlList()
        {
            try
            {
                Util.gridClear(dgMtrlList);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("MTRL_CD", typeof(string));
                inDataTable.Columns.Add("REP_PROCESSING_GROUP_ID", typeof(string));
                inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));
                inDataTable.Columns.Add("MATERIAL_GROUP_CODE", typeof(string));
                inDataTable.Columns.Add("MTRL_TYPE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["MTRL_CD"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.Nvc();
                newRow["REP_PROCESSING_GROUP_ID"] = txtCarrierId.GetBindValue();
                newRow["MATERIAL_LOCATION"] = cboOutputArgWH.GetBindValue();
                newRow["MATERIAL_GROUP_CODE"] = cboMaterialGroup.GetBindValue();
                newRow["MTRL_TYPE"] = cboMaterialType.GetBindValue();

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_INV_SEL_MTRL_PALLET_TRAY_LIST", "RQSTDT", "RSLTD", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgMtrlList, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetMtrlList2()
        {
            try
            {
                Util.gridClear(dgMtrlList2);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("MTRL_TYPE", typeof(string));
                inDataTable.Columns.Add("MTRL_CD", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                inDataTable.Columns.Add("STAT_CODE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = dtpStartT2.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpEndT2.SelectedDateTime.ToString("yyyyMMdd");
                newRow["MTRL_TYPE"] = cboMaterialType2.GetBindValue();
                newRow["MTRL_CD"] = popSearchMtrl2.SelectedValue == null ? null : popSearchMtrl2.SelectedValue.Nvc();
                newRow["CARRIER_ID"] = txtCarrierId2.GetBindValue();
                newRow["STAT_CODE"] = cboReturnStatus.GetBindValue() == null ? null : cboReturnStatus.GetBindValue().ToString().Replace("RETURN_", "").Replace("_RETURN", "");

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_RETURN_REQ_HISTORY", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgMtrlList2, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion


        #region 반품입고 Validation : Validation()
        /// <summary>
        /// HOLD Validation
        /// </summary>
        /// <returns></returns>
        private bool Validation()
        {
            if (!CommonVerify.HasDataGridRow(dgMtrlList))
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (_Util.GetDataGridRowCountByCheck(dgMtrlList, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }
            else
            {
                

                //string selectedMTRL_CD = _Util.GetDataGridFirstRowBycheck(dgMtrlList, "CHK").Field<string>("MATERIAL_CD").GetString();
                //string selectedPLLT = _Util.GetDataGridFirstRowBycheck(dgMtrlList, "CHK").Field<string>("IWMS_2D_BCD_STR").GetString();
                //foreach (C1.WPF.DataGrid.DataGridRow dr in (from C1.WPF.DataGrid.DataGridRow rows in dgMtrlList.Rows
                //                                            where rows.DataItem != null
                //                                                  && rows.Visibility == Visibility.Visible
                //                                                  && rows.Type == DataGridRowType.Item
                //                                                  && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                //                                            select rows))
                //{
                //    if (DataTableConverter.GetValue(dr.DataItem, "IWMS_2D_BCD_STR").GetString().Equals(selectedPLLT) == false)
                //    {
                //        Util.MessageValidation("INV_0082"); // 다수선택시 동일한 자재코드만 가능합니다.
                //        return false;
                //    }

                //    if (DataTableConverter.GetValue(dr.DataItem, "MATERIAL_CD").GetString().Equals(selectedMTRL_CD) == false)
                //    {
                //        Util.MessageValidation("INV_0082"); // 다수선택시 동일한 자재코드만 가능합니다.
                //        return false;
                //    }
                //}
            }

            if (cboOutputArgWH.GetBindValue() == null)
            {
                Util.MessageValidation("SFU2068"); //출고창고를 선택하세요.
                return false;
            }

            if (cboInputArgWH.GetBindValue() == null)
            {
                Util.MessageValidation("SFU2069"); //입고창고를 선택하세요.
                return false;
            }

            //동별공통코드 : INV_MTRL_REQ_LOC/EWM/ATTRIBUTE3가 "Y"이면 반품승인모드로 작동하고, 값이 없거나 "N"이면 기존 LT31T전송하는 로직으로 작동
            if (cboIssuType.GetBindValue() != null)
            {
                if (cboIssuType.SelectedItem.GetValue("ATTRIBUTE3").ToString().Equals("Y"))
                {
                    if (cboReturnType.GetBindValue() == null)
                    {
                        Util.MessageValidation("SFU3640");   //반품유형을 선택해 주세요.
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region 반품입고 요청 처리 : Process()
        private void Process()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_RETURN_INPUT";
                string indata = "INDATA,DURABLE_LIST,INRETURN_INFO";
                DataSet inDataSet = new DataSet();

                #region INDATA
                DataTable inTable = new DataTable("INDATA");
                {
                    inTable.Columns.Add("USER_ID", typeof(string));
                    inTable.Columns.Add("RESERVATION_HOLD_FLAG", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["USER_ID"] = LoginInfo.USERID;
                    if (chkHoldIssue.IsChecked == true)
                    {
                        newRow["RESERVATION_HOLD_FLAG"] = "Y";
                    }
                    inTable.Rows.Add(newRow);
                }
                inDataSet.Tables.Add(inTable);
                #endregion

                #region DURABLE_LIST
                DataTable inTable2 = new DataTable("DURABLE_LIST");
                {
                    inTable2.Columns.Add("CARRIERID", typeof(string));

                    //체크된 row 찾기 -> dr
                    foreach (C1.WPF.DataGrid.DataGridRow dr in (from C1.WPF.DataGrid.DataGridRow rows in dgMtrlList.Rows
                                                                where rows.DataItem != null
                                                                      && rows.Visibility == Visibility.Visible
                                                                      && rows.Type == DataGridRowType.Item
                                                                      && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                                                                select rows))
                    {
                        //체크된 row와 같은 대표자재LOTID row 찾기 -> dr2
                        foreach (C1.WPF.DataGrid.DataGridRow dr2 in (from C1.WPF.DataGrid.DataGridRow rows in dgMtrlList.Rows
                                                                    where rows.DataItem != null
                                                                          && rows.Visibility == Visibility.Visible
                                                                          && rows.Type == DataGridRowType.Item
                                                                          && DataTableConverter.GetValue(rows.DataItem, "REP_PROCESSING_GROUP_ID").GetString() 
                                                                             == DataTableConverter.GetValue(dr.DataItem, "REP_PROCESSING_GROUP_ID").GetString()
                                                                     select rows))
                        {
                            DataRow newRow = inTable2.NewRow();
                            newRow["CARRIERID"] = DataTableConverter.GetValue(dr2.DataItem, "PROCESSING_GROUP_ID").GetString();
                            inTable2.AddDataRow(newRow);
                        }

                        //DURABLE_LIST 에 담기
                        inDataSet.Tables.Add(inTable2);
                    }
                }
                #endregion

                #region INRETURN_INFO
                DataTable inTable3 = new DataTable("INRETURN_INFO");
                {
                    inTable3.Columns.Add("REASON", typeof(string));
                    inTable3.Columns.Add("REASON_DETAIL", typeof(string));
                    inTable3.Columns.Add("TO_LOCATE", typeof(string));

                    DataRow newRow = inTable3.NewRow();
                    newRow["TO_LOCATE"] = cboInputArgWH.GetBindValue();                            //입고창고
                    if (cboReturnType.GetBindValue() != null)
                    {
                        newRow["REASON"] = cboReturnType.GetBindValue();                           //사유코드 B(품질반품)/R(잔량반품)
                        newRow["REASON_DETAIL"] = cboReturnType.SelectedItem.GetValue("ATTR2");    //사유내용 품질반품(B)/잔량반품(R)
                    }

                    inTable3.AddDataRow(newRow);
                }
                inDataSet.Tables.Add(inTable3);
                #endregion

                //동별공통코드 : INV_MTRL_REQ_LOC/EWM/ATTRIBUTE3가 "Y"이면 반품승인모드로 작동하고, 값이 없거나 "N"이면 기존 LT31T전송하는 로직으로 작동
                if (cboIssuType.GetBindValue() != null)
                {
                    if (cboIssuType.SelectedItem.GetValue("ATTRIBUTE3").ToString().Equals("Y"))
                    {
                        indata += ",LT38T_OPTION";

                        #region LT38T_OPTION
                        DataTable inTable4 = new DataTable("LT38T_OPTION");
                        {
                            inTable4.Columns.Add("HOLD_FLAG", typeof(string));
                            inTable4.Columns.Add("HOLD_TYPE", typeof(string));
                            inTable4.Columns.Add("HOLD_CODE", typeof(string));

                            DataRow newRow = inTable4.NewRow();
                            newRow["HOLD_FLAG"] = "Y";
                            newRow["HOLD_TYPE"] = "MANUAL";
                            newRow["HOLD_CODE"] = cboReturnType.SelectedItem.GetValue("ATTR1");  //Hold 사유코드 : INV_HOLD_CODE 참조 01(품질)/05(기타)

                            inTable4.AddDataRow(newRow);
                        }
                        inDataSet.Tables.Add(inTable4);
                        #endregion
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, indata, null, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetMtrlList();

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1275");
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 자재 정보 조회 : SearchMtrl()

        /// <summary>
        /// 기본 자재 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SearchMtrl_Group()
        {
            try
            {

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                inDataTable.Columns.Add("MTGRID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                newRow["MTGRID"] = cboMaterialGroup.GetBindValue();
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrl2.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        #endregion

        #region 자재유형 : SetMtrlType()
        private static void SetMtrlType(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE11" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_MATERIAL_CATEGORY", "Y", "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion

        #region 반품유형 : SetReturnType()
        private void SetReturnType(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "RETURN_TYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

            if (arrColumn != null)
            {
                // 동적 컬럼 생성 및 Row 추가
                foreach (string col in arrColumn)
                    inDataTable.Columns.Add(col, typeof(string));

                DataRow dr = inDataTable.NewRow();

                for (int i = 0; i < inDataTable.Columns.Count; i++)
                    dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                inDataTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            DataRow dr2 = dtResult.NewRow();
            dr2[selectedValueText] = "SELECT";
            dr2[displayMemberText] = "-SELECT-";
            dtResult.Rows.InsertAt(dr2, 0);

            cbo.DisplayMemberPath = displayMemberText;
            cbo.SelectedValuePath = selectedValueText;
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
        #endregion

        #region 반품상태 : SetReturnStatus()
        private void SetReturnStatus(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "RTN_STAT_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

            if (arrColumn != null)
            {
                // 동적 컬럼 생성 및 Row 추가
                foreach (string col in arrColumn)
                    inDataTable.Columns.Add(col, typeof(string));

                DataRow dr = inDataTable.NewRow();

                for (int i = 0; i < inDataTable.Columns.Count; i++)
                    dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                inDataTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            DataRow dr2 = dtResult.NewRow();
            dr2[selectedValueText] = null;
            dr2[displayMemberText] = "-ALL-";
            dtResult.Rows.InsertAt(dr2, 0);

            cbo.DisplayMemberPath = displayMemberText;
            cbo.SelectedValuePath = selectedValueText;
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }

        #endregion

        #region 창고속성 : SetIssuType()
        private static void SetIssuType(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "BUSINESS_USAGE_CODE_ID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_MTRL_REQ_LOC", "EWM", "Y" };
            string selectedValueText = "BUSINESS_USAGE_CODE_ID";
            string displayMemberText = "BUSINESS_USAGE_CODE_ID";

            DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

            if (arrColumn != null)
            {
                // 동적 컬럼 생성 및 Row 추가
                foreach (string col in arrColumn)
                    inDataTable.Columns.Add(col, typeof(string));

                DataRow dr = inDataTable.NewRow();

                for (int i = 0; i < inDataTable.Columns.Count; i++)
                    dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                inDataTable.Rows.Add(dr);
            }

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            cbo.DisplayMemberPath = displayMemberText;
            cbo.SelectedValuePath = selectedValueText;
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
        #endregion

        #region [바코드 프린터 발행용]
        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        #endregion


        #region 반품이력 : SetLabelHistory()
        private void SetLabelHistory(string ReqID, string Prt_Cmpl, int Prt_seq, string Errmsg)
        {
            //반품 라벨 요청 승인시 BCD 출력 후 이력 생성
            DataSet dsHistory = new DataSet();
            DataTable dtHistory = dsHistory.Tables.Add("INDATA");
            dtHistory.Columns.Add("LANGID");
            dtHistory.Columns.Add("REQ_ID");
            dtHistory.Columns.Add("USERID");
            dtHistory.Columns.Add("PRT_CMPL_YN");
            dtHistory.Columns.Add("PRT_ERR_MSG");
            dtHistory.Columns.Add("PRT_SEQ");
            DataRow dr = dtHistory.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["REQ_ID"] = ReqID;
            dr["USERID"] = LoginInfo.USERID;
            dr["PRT_CMPL_YN"] = Prt_Cmpl;
            dr["PRT_ERR_MSG"] = Errmsg;
            dr["PRT_SEQ"] = Prt_seq;
            dtHistory.Rows.Add(dr);
            new ClientProxy().ExecuteService_Multi("BR_INV_UPD_MTRL_RETURN_LABEL", "INDATA", "OUTDATA", (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
                if (result == null || result.Tables.Count < 1 || result.Tables["OUTDATA"].Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1646");
                    return;
                }
          
            }, dsHistory);
        }
        #endregion
        #endregion
    }
}
