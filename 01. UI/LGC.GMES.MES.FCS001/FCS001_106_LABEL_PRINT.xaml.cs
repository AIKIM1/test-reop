/*************************************************************************************
 Created Date : 2021.03.29
      Creator : 조영대
   Decription : 활성화 외부 보관 Pallet 구성
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.29  조영대 : Initial Created

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_106_LABEL_PRINT : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private DataTable dtDefect;
        private DataTable dtRepair;

        bool isValueChanged = true;
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public FCS001_106_LABEL_PRINT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();

                GetComboData();             

                object[] parameter = C1WindowExtension.GetParameters(this);
                if (parameter.Length == 6)
                {
                    cboArea.SelectedValue = parameter[0];
                    cboLine.SelectedValue = parameter[1];
                    cboProcGroup.SelectedValue = parameter[2];
                    cboProcess.SelectedValue = parameter[3];
                    dtpFromDate.SelectedDateTime = (DateTime)parameter[4];
                    dtpToDate.SelectedDateTime = (DateTime)parameter[5];
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLabelList_CheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox chkCurrent = sender as CheckBox;
            if (!chkCurrent.IsVisible) return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)chkCurrent.Parent).Row.Index;

            C1.WPF.DataGrid.DataGridCell gridDefect = dgLabelList.GetCell(rowIndex, dgLabelList.Columns["PLLT_OUTER_WH_DFCT_GR_CODE"].Index);
            if (gridDefect != null && gridDefect.Presenter != null && gridDefect.Presenter.Content != null)
            {
                C1ComboBox combo = gridDefect.Presenter.Content as C1ComboBox;
                if (combo != null)
                {
                    if (chkCurrent.IsChecked.Equals(true))
                    {
                        combo.IsEnabled = true;
                    }
                    else
                    {
                        combo.IsEnabled = false;
                    }
                }
            }

            C1.WPF.DataGrid.DataGridCell gridRepair = dgLabelList.GetCell(rowIndex, dgLabelList.Columns["PLLT_OUTER_WH_REPAIR_GR_CODE"].Index);
            if (gridRepair != null && gridRepair.Presenter != null && gridRepair.Presenter.Content != null)
            {
                C1ComboBox combo = gridRepair.Presenter.Content as C1ComboBox;
                if (combo != null)
                {
                    if (chkCurrent.IsChecked.Equals(true))
                    {
                        combo.IsEnabled = true;
                    }
                    else
                    {
                        combo.IsEnabled = false;
                    }
                }
            }
        }

        private void dgLabelList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Presenter != null && e.Cell.Presenter.Content is C1ComboBox)
            {
                C1ComboBox combo = e.Cell.Presenter.Content as C1ComboBox;
                if (dgLabelList.IsCheckedRow("CHK", e.Cell.Row.Index))
                {
                    combo.IsEnabled = true;
                }
                else
                {
                    combo.IsEnabled = false;
                }
            }
        }
        private void DefectCombo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLabelList.CurrentRow == null || !isValueChanged) return;

            int rowIndex = ((DataGridCellPresenter)((sender as Control).Parent)).Row.Index;

            DataView dvRepair = dtRepair.DefaultView;
            if (Util.NVC(e.NewValue).Equals("ETC"))
            {
                dvRepair.RowFilter = "CBO_CODE = 'ETC' OR CBO_NAME = ''";
                dgLabelList.SetGridCellCombo(rowIndex, "PLLT_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
            }
            else
            {
                dvRepair.RowFilter = "CBO_CODE <> 'ETC' OR CBO_NAME = ''";
                dgLabelList.SetGridCellCombo(rowIndex, "PLLT_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PalletLabelPrint();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgLabelList);
        }

        private void cboProcGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboProcess);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLabelList();
        }
        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPrint);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void GetComboData()
        {
            try
            {
                CommonCombo_Form combo = new CommonCombo_Form();

                // 동
                combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "AREA");

                // 라인
                SetEquipmentSegmentCombo(cboLine);

                // 공정 그룹
                cboProcGroup.SetCommonCode("PROC_GR_CODE", CommonCombo.ComboStatus.ALL, false);

                // 공정
                SetProcessCombo(cboProcess);


                // Grid Combo
                string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "OUTER_WH_DFCT_GR_CODE";
                RQSTDT.Rows.Add(dr);

                dtDefect = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                DataRow newRow = dtDefect.NewRow();
                newRow["CBO_CODE"] = null;
                newRow["CBO_NAME"] = string.Empty;
                dtDefect.Rows.InsertAt(newRow, 0);

                RQSTDT.Rows.Clear();
                dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "OUTER_WH_REP_GR_CODE";
                RQSTDT.Rows.Add(dr);
                dtRepair = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                newRow = dtRepair.NewRow();
                newRow["CBO_CODE"] = null;
                newRow["CBO_NAME"] = string.Empty;
                dtRepair.Rows.InsertAt(newRow, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null,
                cboProcGroup.GetStringValue().IsNullOrEmpty() ? null : cboProcGroup.GetStringValue() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void GetLabelList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("OUTER_WH_PLLT_ID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUTER_WH_PLLT_ID"] = null;
                newRow["FROMDATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["AREAID"] = cboArea.GetBindValue();
                newRow["EQSGID"] = cboLine.GetBindValue();
                newRow["PROCID"] = cboProcess.GetBindValue();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUTER_WH_PLLT_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    if (searchException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(searchException);
                        return;
                    }

                    if (!searchResult.Columns.Contains("CHK"))
                    {
                        searchResult.Columns.Add("CHK", typeof(bool));
                        searchResult.Select().ToList<DataRow>().ForEach(r => r["CHK"] = false);
                    }
                    
                    dgLabelList.SetItemsSource(searchResult, FrameOperation, true);

                    isValueChanged = false;
                    for (int row = 0; row < searchResult.Rows.Count; row++)
                    {
                        dgLabelList.SetGridCellCombo(row, "PLLT_OUTER_WH_DFCT_GR_CODE", dtDefect);
                        dgLabelList.SetValue(row, "PLLT_OUTER_WH_DFCT_GR_CODE", searchResult.Rows[row]["PLLT_OUTER_WH_DFCT_GR_CODE"]);

                        DataView dvRepair = dtRepair.DefaultView;
                        if (Util.NVC(searchResult.Rows[row]["PLLT_OUTER_WH_DFCT_GR_CODE"]).Equals("ETC"))
                        {
                            dvRepair.RowFilter = "CBO_CODE = 'ETC' OR CBO_NAME = ''";
                            dgLabelList.SetGridCellCombo(row, "PLLT_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
                        }
                        else
                        {
                            dvRepair.RowFilter = "CBO_CODE <> 'ETC' OR CBO_NAME = ''";
                            dgLabelList.SetGridCellCombo(row, "PLLT_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
                        }

                        dgLabelList.SetValue(row, "PLLT_OUTER_WH_REPAIR_GR_CODE", searchResult.Rows[row]["PLLT_OUTER_WH_REPAIR_GR_CODE"]);
                    }
                    isValueChanged = true;

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        
        private void PalletLabelPrint()
        {
            try
            {
                if (!dgLabelList.IsCheckedRow("CHK"))
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("BARCODE", typeof(string));
                dt.Columns.Add("BARCODE_TXT", typeof(string));
                dt.Columns.Add("MODEL", typeof(string));
                dt.Columns.Add("LINE", typeof(string));
                dt.Columns.Add("BAD_CONTENT", typeof(string));
                dt.Columns.Add("TOTAL_COUNT", typeof(string));
                dt.Columns.Add("PKG_LOT", typeof(string));
                dt.Columns.Add("REG_USER_NAME", typeof(string));
                dt.Columns.Add("REG_DATE", typeof(string));
                dt.Columns.Add("PRC_USER_NAME", typeof(string));
                dt.Columns.Add("PRC_DATE", typeof(string));
                dt.Columns.Add("DETAIL_CONTENT", typeof(string));
                                
                foreach (int row in dgLabelList.GetCheckedRowIndex("CHK"))
                {
                    DataRow dr = dgLabelList.GetDataRow(row);

                    DataRow drNew = dt.NewRow();
                    drNew["BARCODE"] = Util.NVC(dr["OUTER_WH_PLLT_ID"]);
                    drNew["BARCODE_TXT"] = Util.NVC(dr["OUTER_WH_PLLT_ID"]);
                    drNew["MODEL"] = Util.NVC(dr["MDLLOT_ID"]);
                    drNew["LINE"] = Util.NVC(dr["EQSGNAME"]);

                    string defect = dgLabelList.GetText(row, "PLLT_OUTER_WH_DFCT_GR_CODE");
                    string repair = dgLabelList.GetText(row, "PLLT_OUTER_WH_REPAIR_GR_CODE");
                    if (!Util.IsNVC(defect) || !Util.IsNVC(repair))
                    {
                        drNew["BAD_CONTENT"] = defect + "-" + repair;
                    }
                    else
                    {
                        drNew["BAD_CONTENT"] = string.Empty;
                    }

                    drNew["TOTAL_COUNT"] = Util.NVC(dr["CELL_CNT"]);
                    drNew["PKG_LOT"] = Util.NVC(dr["PROD_LOTID"]);
                    drNew["REG_USER_NAME"] = Util.NVC(dr["PACK_USER"]);
                    drNew["REG_DATE"] = Util.NVC(dr["PLLT_PACK_DTTM"]);
                    drNew["PRC_USER_NAME"] = string.Empty;
                    drNew["PRC_DATE"] = string.Empty;
                    drNew["DETAIL_CONTENT"] = string.Empty;
                    dt.Rows.Add(drNew);
                }

                string targetFile = "LGC.GMES.MES.CMM001.Report.OUT_PALLET_LABEL.xml";
                string targetName = "OUT_PALLET_LABEL";

                Util.PrintNoPreview(dt, targetFile, targetName);

                UpdateDefectRepair();

                Util.MessageInfo("SFU1236");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UpdateDefectRepair()
        {
            const string bizRuleName = "BR_PRD_REG_FORM_WH_OUTER_LABEL_REPRINT";

            DataSet ds = new DataSet();

            DataTable inTable = ds.Tables.Add("INDATA");
            inTable.Columns.Add("USERID", typeof(string));

            DataRow row = inTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(row);

            DataTable inBoxTable = ds.Tables.Add("INBOX");
            inBoxTable.Columns.Add("OUTER_WH_BOX_ID", typeof(string));
            inBoxTable.Columns.Add("OUTER_WH_REPAIR_GR_CODE", typeof(string));
            inBoxTable.Columns.Add("OUTER_WH_DFCT_GR_CODE", typeof(string));

            DataTable inPlltTable = ds.Tables.Add("INPLLT");
            inPlltTable.Columns.Add("OUTER_WH_PLLT_ID", typeof(string));
            inPlltTable.Columns.Add("PLLT_OUTER_WH_DFCT_GR_CODE", typeof(string));
            inPlltTable.Columns.Add("PLLT_OUTER_WH_REPAIR_GR_CODE", typeof(string));

            foreach (DataRow drSelect in dgLabelList.GetCheckedDataRow("CHK"))
            {
                DataRow newRow = inPlltTable.NewRow();
                newRow["OUTER_WH_PLLT_ID"] = drSelect["OUTER_WH_PLLT_ID"];
                newRow["PLLT_OUTER_WH_DFCT_GR_CODE"] = drSelect["PLLT_OUTER_WH_DFCT_GR_CODE"];
                newRow["PLLT_OUTER_WH_REPAIR_GR_CODE"] = drSelect["PLLT_OUTER_WH_REPAIR_GR_CODE"];
                inPlltTable.Rows.Add(newRow);
            }

            string xml = ds.GetXml();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INBOX,INPLLT", null, (result, bizException) =>
            {
                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }

            }, ds);

        }


        #endregion

    }
}
