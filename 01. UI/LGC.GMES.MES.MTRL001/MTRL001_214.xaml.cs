/*************************************************************************************
 Created Date : 2024.09.11
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.11  오화백 : Initial Created.
  2025.02.24  이홍주 : 평치재고 조회 추가 
  2025.03.21  이홍주   ERP 저장위치 칼럼 추가
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MTRL001
{
    /// <summary>
    /// COM001_134.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MTRL001_214 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        private readonly string GROUND_PALLET = "GROUND_PALLET";
        private Util _util = new Util();

        #endregion
               

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_214()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearchT1);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                dtpDate.SelectedDateTime = DateTime.Today;

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void InitCombo()
        {
            // 창고유형
            SetStockerTypeCombo(cboStockerTypeT1);
            SetStockerTypeCombo(cboStockerTypeT2);
        }

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, "Y", "AREA_EQUIPMENT_MTRL_GROUP" };

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME", string.Empty);
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.Nvc()) ? null : cboStockerTypeT1.SelectedValue.Nvc();
            //string electrodeType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.Nvc()) ? null : cboStockerTypeT1.SelectedValue.Nvc();

            //const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            //string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME", LoginInfo.CFG_EQPT_ID);
            
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQGRID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQGRID"] = stockerType;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            DataRow drIns = dtResult.NewRow();
            drIns["CBO_NAME"] = "-ALL-";
            drIns["CBO_CODE"] = "";
            dtResult.Rows.InsertAt(drIns, 0);

            DataRow drIns2 = dtResult.NewRow();
            drIns2["CBO_NAME"] = ObjectDic.Instance.GetObjectName(GROUND_PALLET);
            drIns2["CBO_CODE"] = GROUND_PALLET;
            dtResult.Rows.InsertAt(drIns2, dtResult.Rows.Count);
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;


        }

        private void SearchMtrl_Group(PopupFindControl pfControl)
        {
            try
            {

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_PROD_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        if (pfControl.Equals(popSearchMtrlT1))
                        {
                            popSearchMtrlT1.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        else if (pfControl.Equals(popSearchMtrlT2))
                        {
                            popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                        }

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

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender.Equals(cboStockerTypeT1))
            {
                SetStockerCombo(cboStockerT1);
            }
            else if (sender.Equals(cboStockerTypeT2))
            {
                SetStockerCombo(cboStockerT2);
            }
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender.Equals(cboStockerT1))
            {
                SearchMtrl_Group(popSearchMtrlT1);
            }
            else if (sender.Equals(cboStockerT2))
            {
                SearchMtrl_Group(popSearchMtrlT2);
            }
        }

        private void popSearchMtrl_ValueChanged(object sender, EventArgs e)
        {
            SearchMtrl(sender);
        }

        private void SearchMtrl(object sender)
        {
            try
            {
                if (!(sender is PopupFindControl)) return;

                PopupFindControl searchMtrlPopup = (PopupFindControl)sender;

                if (searchMtrlPopup.SelectedValue == null || string.IsNullOrWhiteSpace(searchMtrlPopup.SelectedValue.ToString()))
                {
                    searchMtrlPopup.SelectedValue = null;
                    searchMtrlPopup.SelectedText = null;
                    searchMtrlPopup.ItemsSource = null;
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                newRow["PRODID"] = searchMtrlPopup.SelectedValue == null ? null : searchMtrlPopup.SelectedValue.ToString();
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_PROD_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        searchMtrlPopup.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            GetStockerListT1();
        }

        private void GetStockerListT1()
        {
            try
            {
                Util.gridClear(dgListT1);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SNAP_DATE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MTRL_CD", typeof(string));
                inDataTable.Columns.Add("PALLETID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SNAP_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EQPTID"] = cboStockerT1.GetBindValue();
                newRow["MTRL_CD"] = popSearchMtrlT1.SelectedValue == null ? null : popSearchMtrlT1.SelectedValue.Nvc();
                newRow["PALLETID"] = txtPalletIDT1.GetBindValue();
                               

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_STATUS_VIEW", "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgListT1, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

         private void dgListT1_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

    }
}
