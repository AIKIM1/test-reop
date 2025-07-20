/*************************************************************************************
 Created Date : 2021.04.06
      Creator : 조영대
   Decription : 추가 라벨 발행
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.06  조영대 : Initial Created
  2023.01.31  조영대 : Validation 추가
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
    public partial class FCS001_105_LABEL_PRINT : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private readonly Util _util = new Util();

        private DataTable dtDefect;
        private DataTable dtRepair;

        bool isValueChanged = true;
        #endregion

        #region Initialize 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_105_LABEL_PRINT()
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

            C1.WPF.DataGrid.DataGridCell gridDefect = dgLabelList.GetCell(rowIndex, dgLabelList.Columns["BOX_OUTER_WH_DFCT_GR_CODE"].Index);
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

            C1.WPF.DataGrid.DataGridCell gridRepair = dgLabelList.GetCell(rowIndex, dgLabelList.Columns["BOX_OUTER_WH_REPAIR_GR_CODE"].Index);
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

        private void defectCombo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLabelList.CurrentRow == null || !isValueChanged) return;
            
            int rowIndex = ((DataGridCellPresenter)((sender as Control).Parent)).Row.Index;
            
            DataView dvRepair = dtRepair.DefaultView;
            if (Util.NVC(e.NewValue).Equals("ETC"))
            {
                dvRepair.RowFilter = "CBO_CODE = 'ETC' OR CBO_NAME = ''";
                dgLabelList.SetGridCellCombo(rowIndex, "BOX_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
            }
            else
            {
                dvRepair.RowFilter = "CBO_CODE <> 'ETC' OR CBO_NAME = ''";
                dgLabelList.SetGridCellCombo(rowIndex, "BOX_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
            }

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("OUTER_WH_BOX_ID", typeof(string));
                dt.Columns.Add("MODLID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));
                dt.Columns.Add("BOX_DATE", typeof(string));
                dt.Columns.Add("DFCT_CODE", typeof(string));
                dt.Columns.Add("DFCT_NAME", typeof(string));
                dt.Columns.Add("CNT", typeof(string));

                foreach (int row in dgLabelList.GetCheckedRowIndex("CHK"))
                {
                    DataRow dr = dgLabelList.GetDataRow(row);

                    DataRow drNew = dt.NewRow();
                    drNew["OUTER_WH_BOX_ID"] = dr["OUTER_WH_BOX_ID"];
                    drNew["MODLID"] = dr["MDLLOT_ID"];
                    drNew["PRJT_NAME"] = dr["PROD_LOTID"];
                    drNew["BOX_DATE"] = dr["BOX_PACK_DTTM"];
                    drNew["DFCT_CODE"] = null;

                    string defect = dgLabelList.GetText(row, "BOX_OUTER_WH_DFCT_GR_CODE");
                    string repair = dgLabelList.GetText(row, "BOX_OUTER_WH_REPAIR_GR_CODE");
                    if (!Util.IsNVC(defect) || !Util.IsNVC(repair))
                    {
                        drNew["DFCT_NAME"] = defect + "-" + repair;
                    }
                    else
                    {
                        drNew["DFCT_NAME"] = string.Empty;
                    }

                    drNew["CNT"] = dr["CELL_CNT"];
                    dt.Rows.Add(drNew);
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    Util.MessageValidation("8009");
                    return;
                }

                BoxBarCodeAutoPrint(dt);
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
                inTable.Columns.Add("OUTER_WH_BOX_ID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUTER_WH_BOX_ID"] = null;
                newRow["FROMDATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["AREAID"] = cboArea.GetBindValue();
                newRow["EQSGID"] = cboLine.GetBindValue();
                newRow["PROCID"] = cboProcess.GetBindValue();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUTER_WH_BOX_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                        dgLabelList.SetGridCellCombo(row, "BOX_OUTER_WH_DFCT_GR_CODE", dtDefect);
                        dgLabelList.SetValue(row, "BOX_OUTER_WH_DFCT_GR_CODE", searchResult.Rows[row]["BOX_OUTER_WH_DFCT_GR_CODE"]);

                        DataView dvRepair = dtRepair.DefaultView;
                        if (Util.NVC(searchResult.Rows[row]["BOX_OUTER_WH_DFCT_GR_CODE"]).Equals("ETC"))
                        {
                            dvRepair.RowFilter = "CBO_CODE = 'ETC' OR CBO_NAME = ''";
                            dgLabelList.SetGridCellCombo(row, "BOX_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
                        }
                        else
                        {
                            dvRepair.RowFilter = "CBO_CODE <> 'ETC' OR CBO_NAME = ''";
                            dgLabelList.SetGridCellCombo(row, "BOX_OUTER_WH_REPAIR_GR_CODE", dvRepair.ToTable());
                        }

                        dgLabelList.SetValue(row, "BOX_OUTER_WH_REPAIR_GR_CODE", searchResult.Rows[row]["BOX_OUTER_WH_REPAIR_GR_CODE"]);
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
            finally
            {
                isValueChanged = true;
            }

        }

        private void BoxBarCodeAutoPrint(DataTable dt)
        {
            DataTable dtLabelItem = new DataTable();
            //dtLabelItem.Locale = new System.Globalization.CultureInfo("zh-cn");

            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  // LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     // Model Code
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     // 생성일자
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     // BOX ID BCD
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     // BOX ID
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     // PJT
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     // 수량
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     // 박스/Pallet 보관 유형 명
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            foreach (DataRow dr in dt.Rows)
            {
                DataRow drNew = dtLabelItem.NewRow();
                drNew["LABEL_CODE"] = "LBL0283";
                drNew["ITEM001"] = dr["MODLID"].GetString();
                drNew["ITEM002"] = dr["BOX_DATE"].GetString();
                drNew["ITEM003"] = dr["OUTER_WH_BOX_ID"].GetString();
                drNew["ITEM004"] = dr["OUTER_WH_BOX_ID"].GetString();
                drNew["ITEM005"] = dr["PRJT_NAME"].GetString();
                drNew["ITEM006"] = dr["CNT"].GetString();
                drNew["ITEM007"] = dr["DFCT_NAME"].GetString();
                dtLabelItem.Rows.Add(drNew);
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                UpdateDefectRepair();
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
                DataRow newRow = inBoxTable.NewRow();
                newRow["OUTER_WH_BOX_ID"] = drSelect["OUTER_WH_BOX_ID"];
                newRow["OUTER_WH_REPAIR_GR_CODE"] = drSelect["BOX_OUTER_WH_DFCT_GR_CODE"];
                newRow["OUTER_WH_DFCT_GR_CODE"] = drSelect["BOX_OUTER_WH_REPAIR_GR_CODE"];
                inBoxTable.Rows.Add(newRow);
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
