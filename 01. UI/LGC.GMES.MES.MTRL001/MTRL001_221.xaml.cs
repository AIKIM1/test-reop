/*************************************************************************************
 Created Date : 2024.11.06
      Creator : 
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.05  Initial Created.
  2025.03.21  이홍주          모델명 (PRJT_NAME) 칼럼 추가
 
 
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
using System.Windows.Media;

namespace LGC.GMES.MES.MTRL001
{

    public partial class MTRL001_221 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private readonly string GROUND_PALLET = "GROUND_PALLET";
        private Util _util = new Util();

        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MTRL001_221()
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

                dtpEnd.SelectedDateTime = DateTime.Today;
                dtpStart.SelectedDateTime = DateTime.Today.AddDays(-31);

                if (tabMain.SelectedItem.Equals(tabT2))
                {
                    this.Loaded -= UserControl_Loaded;
                }
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
            // 공정
            cboProcCodeT1.SetCommonCode("STO_PROCESS", CommonCombo.ComboStatus.ALL, true);
            cboProcCodeT2.SetCommonCode("STO_PROCESS", CommonCombo.ComboStatus.ALL, true);

            //상태
            cboStatusT1.SetCommonCode("SHIP_RCV_ISS_STAT_CODE", CommonCombo.ComboStatus.ALL, true);
            cboStatusT2.SetCommonCode("SHIP_RCV_ISS_STAT_CODE", CommonCombo.ComboStatus.ALL, true);
        }

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, "Y", "AREA_EQUIPMENT_MTRL_GROUP" };

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME", string.Empty);
        }
        #endregion

        #region [Method]
        private void GetStockerListT1()
        {
            try
            {
                Util.gridClear(dgListT1);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("STAT_CODE", typeof(string));
                inDataTable.Columns.Add("PalletID", typeof(string));
                inDataTable.Columns.Add("PLANT_ID", typeof(string));
                inDataTable.Columns.Add("AND_TYPE1", typeof(string));
                inDataTable.Columns.Add("AND_TYPE2", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboStockerT1.GetBindValue();
                newRow["PROCID"] = cboProcCodeT1.SelectedValue == null ? null : cboProcCodeT1.SelectedValue.ToString();
                newRow["STAT_CODE"] = cboStatusT1.SelectedValue == null ? null : cboStatusT1.SelectedValue.ToString();
                newRow["PALLETID"] = txtPalletIDT1.GetBindValue();
                newRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;

                if (cboStockerT1.GetBindValue() == null)
                {
                    newRow["AND_TYPE1"] = DBNull.Value;
                    newRow["AND_TYPE2"] = DBNull.Value;
                }
                else
                {
                    if (cboStockerT1.GetBindValue().ToString().Equals(GROUND_PALLET))
                    {
                        newRow["AND_TYPE1"] = DBNull.Value;
                        newRow["AND_TYPE2"] = "1";
                    }
                    else
                    {
                        newRow["AND_TYPE1"] = "1";
                        newRow["AND_TYPE2"] = DBNull.Value;
                    }
                }
                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_GDS_PRCS_INFO", "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
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

        private void GetStockerListT2()
        {
            try
            {
                Util.gridClear(dgListT2);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("STAT_CODE", typeof(string));
                inDataTable.Columns.Add("PalletID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("PLANT_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = cboProcCodeT2.SelectedValue == null ? null : cboProcCodeT2.SelectedValue.ToString();
                newRow["STAT_CODE"] = cboStatusT2.SelectedValue == null ? null : cboStatusT2.SelectedValue.ToString();
                newRow["PALLETID"] = txtPalletIDT2.GetBindValue();
                newRow["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpEnd.SelectedDateTime.ToString("yyyyMMdd");
                newRow["PLANT_ID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_GDS_PRCS_INFO_HIST", "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgListT2, searchResult, FrameOperation, true);
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

        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.Nvc()) ? null : cboStockerTypeT1.SelectedValue.Nvc();
            string electrodeType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.Nvc()) ? null : cboStockerTypeT1.SelectedValue.Nvc();

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
            dtResult.Rows.InsertAt(drIns2, dtResult.Rows.Count );
            cbo.ItemsSource = dtResult.Copy().AsDataView();

            cbo.SelectedIndex = 0;
        }
        #endregion

        #region [Event]
        private void cboStockerTypeT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetStockerCombo(cboStockerT1);
        }

        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            GetStockerListT1();
        }

        private void btnSearchT2_Click(object sender, RoutedEventArgs e)
        {
            GetStockerListT2();

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

        private void dgListT2_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

        private void dgListT1_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                dgListT1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    //link 색변경
                    if (e.Cell.Column.Name.Equals("TOP_PROCESSING_GROUP_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListT1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgListT1.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "TOP_PROCESSING_GROUP_ID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            tabMain.SelectedItem = tabT2;
                            txtPalletIDT2.Text = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[cell.Row.Index].DataItem, cell.Column.Name));
                            btnSearchT2.PerformClick();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
