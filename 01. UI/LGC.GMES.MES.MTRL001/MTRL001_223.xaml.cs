/*************************************************************************************
 Created Date : 2025.01.14
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.12  DEVELOPER : Initial Created.
  2025.01.14  유재기    : Data List 파라메터 추가
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
    public partial class MTRL001_223 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public MTRL001_223()
        {
            InitializeComponent();
            InitCombo();
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

                //자재조회
                SearchMtrl_Group();

                //입고창고
                SetInputArgWHCombo(cboInputArgWH);

                //출고창고
                SetOutputArgWHCombo(cboOutputArgWH);
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
        #endregion

        #region 리스트 선택 : Status_Checked()
        private void Status_Checked(object sender, RoutedEventArgs e)
        {
            dgMtrlList.Selection.Clear();

            CheckBox rb = sender as CheckBox;

            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(true) ||
                DataTableConverter.GetValue(rb.DataContext, "CHK").Nvc().Equals("1"))
            {
                if (_Util.GetDataGridRowCountByCheck(dgMtrlList, "CHK") > 1)
                {
                    string selectedDGSU_YN = _Util.GetDataGridFirstRowBycheck(dgMtrlList, "CHK").Field<string>("DGSU_YN").GetString();
                    if (DataTableConverter.GetValue(rb.DataContext, "DGSU_YN").Equals(selectedDGSU_YN) == false)
                    {
                        //일반 자재와 위험물 자재는 함께 선택할 수 없습니다.
                        Util.MessageValidation("SUF9040", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                rb.IsChecked = false;
                            }
                        });
                    }
                }
            }
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

        private void popSearchMtrl_ValueChanged(object sender, EventArgs e)
        {
            SearchMtrl();
        }

        #endregion

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

        #region 자재 리스트 조회 : GetMtrlList()

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


                DataRow newRow = inDataTable.NewRow();
                newRow["MTRL_CD"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.Nvc();
                newRow["REP_PROCESSING_GROUP_ID"] = txtCarrierId.GetBindValue();
                newRow["MATERIAL_LOCATION"] = cboOutputArgWH.GetBindValue();
                newRow["MATERIAL_GROUP_CODE"] = cboMaterialGroup.GetBindValue();

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_PALLET_TRAY_LIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
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
                string selectedDGSU_YN = _Util.GetDataGridFirstRowBycheck(dgMtrlList, "CHK").Field<string>("DGSU_YN").GetString();
                foreach (C1.WPF.DataGrid.DataGridRow dr in (from C1.WPF.DataGrid.DataGridRow rows in dgMtrlList.Rows
                                                            where rows.DataItem != null
                                                                  && rows.Visibility == Visibility.Visible
                                                                  && rows.Type == DataGridRowType.Item
                                                                  && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                                                            select rows))
                {
                    if (DataTableConverter.GetValue(dr.DataItem, "DGSU_YN").GetString().Equals(selectedDGSU_YN) == false)
                    {
                        Util.MessageValidation("SUF9040"); //일반 자재와 위험물 자재는 함께 선택할 수 없습니다.
                        return false;
                    }
                }
            }

            if (cboOutputArgWH.GetBindValue() == null)
            {
                Util.MessageValidation("SFU2068"); //출고창고를 선택하세요.
                return false;
            }

            //if (cboInputArgWH.GetBindValue() == null)
            //{
            //    Util.MessageValidation("SFU2069"); //입고창고를 선택하세요.
            //    return false;
            //}

            return true;
        }

#endregion

#region 반품입고 처리 : Process()
        private void Process()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_RETURN_INPUT";

                DataSet inDataSet = new DataSet();

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

                DataTable inTable2 = new DataTable("DURABLE_LIST");
                {
                    inTable2.Columns.Add("CARRIERID", typeof(string));

                    foreach (C1.WPF.DataGrid.DataGridRow dr in (from C1.WPF.DataGrid.DataGridRow rows in dgMtrlList.Rows
                                                                 where rows.DataItem != null
                                                                       && rows.Visibility == Visibility.Visible
                                                                       && rows.Type == DataGridRowType.Item
                                                                       && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                                                                 select rows))
                    {
                        DataRow newRow = inTable2.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(dr.DataItem, "PROCESSING_GROUP_ID").GetString();
                        inTable2.AddDataRow(newRow);
                    }
                }
                inDataSet.Tables.Add(inTable2);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,DURABLE_LIST", null, (result, bizException) =>
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


        /// <summary>
        /// 자재 정보 조회
        /// </summary>

        private void SearchMtrl()
        {
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

#endregion

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
