/*************************************************************************************
 Created Date : 2022.12.27
      Creator : 정문교C
   Decription : 보빈 상태 변경
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.27  정문교 : Initial Created.
  2023.01.05  신광희 : 세정여부, 권취여부 콤보박스 멀티 선택 기능, 권취여부 전후/ 세정여부 전후 값 비교 하이라이트 표시, 세정여부 권취여부 콤보박스 설정값에 따른 상호 값 설정
  2023.01.12  신광희 : 보빈ID 영역 멀티 선택 붙여넣기 시 조회 처리 수정
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using C1.WPF.DataGrid;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_MCS_CHANGE_BOBIN_STATUS.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_MCS_CHANGE_BOBIN_STATUS : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        public bool IsUpdated;

        private string _AreaID;
        private string _GR_AreaID;
        private string _CstType;
        private readonly Util _util = new Util();

        public CMM_MCS_CHANGE_BOBIN_STATUS()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    

        private void Initialize()
        {
            ApplyPermissions();
            InitializeControls();

            SetCstCleanFlagCombo(cboCstCleanFlag);
            SetCstCleanFlagComboUpdate(cboCstCleanFlagUpdate);
            SetCarrierCleanFlagComboItem();
            SetPetWindingCmplFlagCombo(cboPetWindingCmplFlag);
            SetPetWindingCmplFlagComboUpdate(cboPetWindingCmplFlagUpdate);
            SetWindingCompleteFlagComboItem();

            cboCstCleanFlagUpdate.SelectedValueChanged += cboCstCleanFlagUpdate_SelectedValueChanged;
            cboPetWindingCmplFlagUpdate.SelectedValueChanged += cboPetWindingCmplFlagUpdate_SelectedValueChanged;
        }

        private void InitializeControls()
        {
        }

        private static void SetCstCleanFlagCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "CST_CLEAN_FLAG" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetCstCleanFlagComboUpdate(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "CST_CLEAN_FLAG", "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetPetWindingCmplFlagCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "PET_WINDING_CMPL_FLAG" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetPetWindingCmplFlagComboUpdate(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "PET_WINDING_CMPL_FLAG", "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetCarrierCleanFlagComboItem()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";

            DataSet ds = new DataSet();
            DataTable inTable = ds.Tables.Add("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("ATTRIBUTE1", typeof(string));
            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["CMCDTYPE"] = "CST_CLEAN_FLAG";
            newRow["ATTRIBUTE1"] = "Y";
            inTable.Rows.Add(newRow);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            var dataGridComboBoxColumn = dgBobin.Columns["CBO_CLEAN_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (dataGridComboBoxColumn != null)
            {
                DataRow dr = searchResult.NewRow();
                dr["CBO_CODE"] = string.Empty;
                dr["CBO_NAME"] = string.Empty;
                searchResult.Rows.InsertAt(dr, 0);
            }
            dataGridComboBoxColumn.ItemsSource = searchResult.Copy().AsDataView();
        }

        private void SetWindingCompleteFlagComboItem()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";

            DataSet ds = new DataSet();
            DataTable inTable = ds.Tables.Add("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("ATTRIBUTE1", typeof(string));
            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["CMCDTYPE"] = "PET_WINDING_CMPL_FLAG";
            newRow["ATTRIBUTE1"] = "Y";
            inTable.Rows.Add(newRow);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            var dataGridComboBoxColumn = dgBobin.Columns["CBO_WINDING_COMPLETE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (dataGridComboBoxColumn != null)
            {
                DataRow dr = searchResult.NewRow();
                dr["CBO_CODE"] = string.Empty;
                dr["CBO_NAME"] = string.Empty;
                searchResult.Rows.InsertAt(dr, 0);
            }
            dataGridComboBoxColumn.ItemsSource = searchResult.Copy().AsDataView();
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Initialize();

                object[] tmps = C1WindowExtension.GetParameters(this);
                _AreaID = tmps[0] as string;
                _GR_AreaID = tmps[1] as string;
                _CstType = tmps[2] as string;

                Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBobinId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                SelectList(txtBobinId.Text);
            }

        }

        private void txtBobinId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { Environment.NewLine };
                    string pasteString = Clipboard.GetText();
                    string[] pasteStrings = pasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (pasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }


                    DataTable dt = DataTableConverter.Convert(dgBobin.ItemsSource);

                    if(CommonVerify.HasTableRow(dt))
                    {
                        for (int i = 0; i < pasteStrings.Length; i++)
                        {
                            DataRow[] dr = dt.Select("BOBBIN_ID = '" + pasteStrings[i] + "'");
                            if (dr.Length > 0)
                            {
                                Util.MessageValidation("SFU8535", pasteStrings[i]);
                                return;
                            }
                        }
                    }

                    string pasteText = string.Join(";", pasteStrings);

                    if (pasteStrings.Length > 0)
                    {
                        SelectList(pasteText);
                        e.Handled = true;
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtBobinId.Text = string.Empty;
                    txtBobinId.Focus();
                }
            }
        }


        private void dgBobin_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("DELETE"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "KEYIN")) == "Y")
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ((ContentControl)e.Cell.Presenter.Content).Visibility = Visibility.Collapsed;
                        }
                    }

                    if (e.Cell.Column.Name.Equals("CBO_CLEAN_FLAG"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").GetString() == "True" || DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").GetString() == "1")
                        {
                            if(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CLEAN_FLAG_AFTER")).GetString()))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CLEAN_FLAG")).GetString() !=
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CLEAN_FLAG_AFTER")).GetString())
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                                }
                            }
                        }
                    }
                    else if(e.Cell.Column.Name.Equals("CBO_WINDING_COMPLETE_FLAG"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").GetString() == "True" || DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").GetString() == "1")
                        {
                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CLEAN_FLAG_AFTER")).GetString()))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PET_WINDING_CMPL_FLAG")).GetString() !=
                                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PET_WINDING_CMPL_FLAG_AFTER")).GetString())
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                                }
                            }
                        }
                    }
                }
            }));
        }

        private void dgBobin_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e?.Row?.DataItem == null || e.Column == null)
                return;

            if (e.Column.Name == "DELETE")
            {
                if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[e.Row.Index].DataItem, "KEYIN")) != "Y")
                {
                    e.Cancel = true;
                    return;
                }
            }
            /*
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv["CHK"].GetString() != "True" && e.Column != this.dgBobin.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }
                
            if (e.Column != dgBobin.Columns["CHK"])
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
            */
        }

        private void dgBobin_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
        }

        private void dgBobin_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataRowView drv = e.Cell.Row.DataItem as DataRowView;

            int rowIndex = e.Cell.Row.Index;
            int colIndex = e.Cell.Column.Index;

            int cleanFlagIndex = dgBobin.Columns["CBO_CLEAN_FLAG"].Index;
            int windingCompleteFlagIndex = dgBobin.Columns["CBO_WINDING_COMPLETE_FLAG"].Index;

            if (e.Cell.Column.Name == "CHK")
            {
                if (DataTableConverter.GetValue(drv, "CHK").GetString() == "1" || DataTableConverter.GetValue(drv, "CHK").GetString() == "True")
                {
                    #region [체크박스 선택 시 캐리어 세정여부 영역]
                    string cleanFlag = cboCstCleanFlagUpdate.SelectedValue.Equals("SELECT") ? null : cboCstCleanFlagUpdate.SelectedValue.GetString();
                    DataTableConverter.SetValue(drv, "CST_CLEAN_FLAG_AFTER", cleanFlag);

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(drv, "CST_CLEAN_FLAG_AFTER")).GetString()))
                    {
                        if (dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter != null)
                            dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(drv, "CST_CLEAN_FLAG")).GetString() !=
                            Util.NVC(DataTableConverter.GetValue(drv, "CST_CLEAN_FLAG_AFTER")).GetString())
                        {
                            if (dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter != null)
                                dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            if (dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter != null)
                                dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                    }
                    #endregion


                    #region [체크박스 선택 시 PET 권취여부 영역]
                    string windingCompleteFlag = cboPetWindingCmplFlagUpdate.SelectedValue.Equals("SELECT") ? null : cboPetWindingCmplFlagUpdate.SelectedValue.GetString();
                    DataTableConverter.SetValue(drv, "PET_WINDING_CMPL_FLAG_AFTER", windingCompleteFlag);

                    if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(drv, "PET_WINDING_CMPL_FLAG_AFTER")).GetString()))
                    {
                        if (dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter != null)
                            dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(drv, "PET_WINDING_CMPL_FLAG")).GetString() !=
                            Util.NVC(DataTableConverter.GetValue(drv, "PET_WINDING_CMPL_FLAG_AFTER")).GetString())
                        {
                            if (dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter != null)
                                dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            if (dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter != null)
                                dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                    }
                    #endregion

                }
                else
                {
                    DataTableConverter.SetValue(drv, "CST_CLEAN_FLAG_AFTER", string.Empty);
                    DataTableConverter.SetValue(drv, "PET_WINDING_CMPL_FLAG_AFTER", string.Empty);
                    if (dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter != null)
                        dgBobin.GetCell(rowIndex, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    if (dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter != null)
                        dgBobin.GetCell(rowIndex, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            dgBobin.EndEdit();
            dgBobin.EndEditRow(true);
        }
        private void btnColumnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

                if (Util.NVC(dgBobin.GetCell(iRow, dgBobin.Columns["CHK"].Index).Value) == "1" ||
                    Util.NVC(dgBobin.GetCell(iRow, dgBobin.Columns["CHK"].Index).Value) == "True")
                {
                    if (Util.NVC(dgBobin.GetCell(iRow, dgBobin.Columns["KEYIN"].Index).Value) == "Y")
                    {
                        string sCell = Util.NVC(dgBobin.GetCell(iRow, dgBobin.Columns["BOBBIN_ID"].Index).Value);

                        DataTable dtDelete = DataTableConverter.Convert(dgBobin.ItemsSource);

                        dtDelete.Select("BOBBIN_ID = '" + sCell + "'").ToList<DataRow>().ForEach(row => row.Delete());
                        dtDelete.AcceptChanges();

                        Util.GridSetData(dgBobin, dtDelete, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch())
                    return;

                SelectList(txtBobinId.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgBobin.EndEdit();
                dgBobin.EndEditRow(true);

                if (!ValidationSave())
                    return;

                Save();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        private void cboCstCleanFlagUpdate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            string value = cboCstCleanFlagUpdate.SelectedValue.Equals("SELECT") ? null : cboCstCleanFlagUpdate.SelectedValue.GetString();
            int cleanFlagIndex = dgBobin.Columns["CBO_CLEAN_FLAG"].Index;
            int windingCompleteFlagIndex = dgBobin.Columns["CBO_WINDING_COMPLETE_FLAG"].Index;

            cboPetWindingCmplFlagUpdate.SelectedValueChanged -= cboPetWindingCmplFlagUpdate_SelectedValueChanged;

            //Carrier 세정여부 선택에 따른 PET 권취 여부 설정
            //Carrier 세정여부(Q) 선택 시 PET 권취 여부 null , 세정여부(Y) 선택 시 PET 권취 여부(Q)
            string windingCompleteFlagvalue;
            if(string.IsNullOrEmpty(value))
            {
                windingCompleteFlagvalue = null;
                cboPetWindingCmplFlagUpdate.SelectedValue = "SELECT";
            }
            else
            {
                windingCompleteFlagvalue = value.Equals("Q") ? null : "Q";
                cboPetWindingCmplFlagUpdate.SelectedValue = value.Equals("Q") ? "SELECT" : "Q";
            }

            if (CommonVerify.HasDataGridRow(dgBobin))
            {
                for (int j = 0; j < dgBobin.Rows.Count; j++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CHK")).GetString() == "1" ||
                        Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CHK")).GetString() == "True")
                    {
                        DataTableConverter.SetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER", value);
                        DataTableConverter.SetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER", windingCompleteFlagvalue);

                        if(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER")).GetString()))
                        {
                            if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG")).GetString() !=
                               Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER")).GetString())
                            {
                                if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                            else
                            {
                                if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }

                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER")).GetString()))
                        {
                            if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG")).GetString() !=
                               Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER")).GetString())
                            {
                                if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                            else
                            {
                                if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }
                    }
                }
            }
            cboPetWindingCmplFlagUpdate.SelectedValueChanged += cboPetWindingCmplFlagUpdate_SelectedValueChanged;
        }

        private void cboPetWindingCmplFlagUpdate_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string value = cboPetWindingCmplFlagUpdate.SelectedValue.Equals("SELECT") ? null : cboPetWindingCmplFlagUpdate.SelectedValue.GetString();
            int windingCompleteFlagIndex = dgBobin.Columns["CBO_WINDING_COMPLETE_FLAG"].Index;
            int cleanFlagIndex = dgBobin.Columns["CBO_CLEAN_FLAG"].Index;

            cboCstCleanFlagUpdate.SelectedValueChanged -= cboCstCleanFlagUpdate_SelectedValueChanged;

            //PET 권취여부 선택에 따른 Carrier 세정 여부 설정
            //PET 권취여부(Q) 선택 시 Carrier 세정 여부(Y) , 권취여부(Y) 선택 시 Carrier 세정 여부(Y)
            string cleanFlagvalue;

            if (string.IsNullOrEmpty(value))
            {
                cleanFlagvalue = string.Empty;
                cboCstCleanFlagUpdate.SelectedValue = "SELECT";
            }
            else
            {   // 권취여부(Q) 선택 시, 권취여부(Y) 선택 시 모두 Y
                cleanFlagvalue = "Y";
                cboCstCleanFlagUpdate.SelectedValue = "Y";
            }

            if (CommonVerify.HasDataGridRow(dgBobin))
            {
                for (int j = 0; j < dgBobin.Rows.Count; j++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CHK")).GetString() == "1" ||
                        Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CHK")).GetString() == "True")
                    {
                        DataTableConverter.SetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER", value);
                        DataTableConverter.SetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER", cleanFlagvalue);

                        if(string.IsNullOrEmpty(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER").GetString()))
                        {
                            if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG")).GetString() !=
                               Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "PET_WINDING_CMPL_FLAG_AFTER")).GetString())
                            {
                                if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                            else
                            {
                                if (dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, windingCompleteFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }

                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER").GetString()))
                        {
                            if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG")).GetString() !=
                               Util.NVC(DataTableConverter.GetValue(dgBobin.Rows[j].DataItem, "CST_CLEAN_FLAG_AFTER")).GetString())
                            {
                                if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            }
                            else
                            {
                                if (dgBobin.GetCell(j, cleanFlagIndex).Presenter != null)
                                    dgBobin.GetCell(j, cleanFlagIndex).Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }
                    }
                }
            }
            cboCstCleanFlagUpdate.SelectedValueChanged += cboCstCleanFlagUpdate_SelectedValueChanged;
        }

        #endregion

        #region Mehod

        private void SelectList(string bobinIdText)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_MCS_GET_CHANGE_BOBBIN_STATUS";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("GR_AREAID", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                inTable.Columns.Add("PET_WINDING_CMPL_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AreaID;
                newRow["GR_AREAID"] = _GR_AreaID;
                newRow["CSTTYPE"] = _CstType;

                if (string.IsNullOrWhiteSpace(bobinIdText))
                {
                    newRow["CST_CLEAN_FLAG"] = cboCstCleanFlag.SelectedValue.ToString() == "SELECT" ? null : cboCstCleanFlag.SelectedValue.ToString();
                    newRow["PET_WINDING_CMPL_FLAG"] = cboPetWindingCmplFlag.SelectedValue.ToString() == "SELECT" ? null : cboPetWindingCmplFlag.SelectedValue.ToString();
                }
                else
                {
                    newRow["CSTID"] = bobinIdText;
                }
                inTable.Rows.Add(newRow);


                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUT_INFO", inTable, (bizResult, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("CST_CLEAN_FLAG_AFTER", typeof(string));
                    bizResult.Columns.Add("PET_WINDING_CMPL_FLAG_AFTER", typeof(string));
                    DataTable dt = bizResult;

                    DataColumn dc = new DataColumn("KEYIN");

                    if (string.IsNullOrWhiteSpace(bobinIdText))
                    {
                        dc.DefaultValue = "N";
                        dt.Columns.Add(dc);

                        Util.GridSetData(dgBobin, dt, FrameOperation,true);
                    }
                    else
                    {
                        if (dt.Rows.Count == 0)
                        {
                            Util.MessageInfo("SFU1905");    // 조회된 Data가 없습니다.
                            txtBobinId.Text = string.Empty;
                            return;
                        }

                        dc.DefaultValue = "Y";

                        foreach (DataRow row in dt.Rows)
                        {
                            row["CHK"] = true;
                            row["CST_CLEAN_FLAG_AFTER"] = cboCstCleanFlagUpdate.SelectedValue?.GetString() == "SELECT" ? null : cboCstCleanFlagUpdate.SelectedValue.GetString();
                            row["PET_WINDING_CMPL_FLAG_AFTER"] = cboPetWindingCmplFlagUpdate.SelectedValue?.GetString() == "SELECT" ? null : cboPetWindingCmplFlagUpdate.SelectedValue.GetString();
                        }
                        dt.AcceptChanges();
                        dt.Columns.Add(dc);

                        DataTable dtBefore = DataTableConverter.Convert(dgBobin.ItemsSource);
                        dtBefore.Merge(dt);

                        Util.GridSetData(dgBobin, dtBefore, FrameOperation, true);
                    }

                    txtBobinId.Text = string.Empty;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void Save()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_MCS_REG_CHANGE_BOBBIN_STATUS";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
                inTable.Columns.Add("PET_WINDING_CMPL_FLAG", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("OLD_CST_CLEAN_FLAG", typeof(string));
                inTable.Columns.Add("OLD_PET_WINDING_CMPL_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();

                foreach (C1.WPF.DataGrid.DataGridRow row in dgBobin.Rows)
                {
                    if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                             Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                    {
                        newRow = inTable.NewRow();
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "BOBBIN_ID").GetString();
                        newRow["OLD_CST_CLEAN_FLAG"] = DataTableConverter.GetValue(row.DataItem, "CST_CLEAN_FLAG").GetString();
                        newRow["OLD_PET_WINDING_CMPL_FLAG"] = DataTableConverter.GetValue(row.DataItem, "PET_WINDING_CMPL_FLAG").GetString();
                        newRow["AREAID"] = _AreaID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["CST_CLEAN_FLAG"] = DataTableConverter.GetValue(row.DataItem, "CST_CLEAN_FLAG_AFTER").GetString();
                        // Carrier 세정여부 'Q' 선택 시 PET 권취여부 [NULL] 로 저장
                        newRow["PET_WINDING_CMPL_FLAG"] = DataTableConverter.GetValue(row.DataItem, "CST_CLEAN_FLAG_AFTER").GetString().Equals("Q") ? null : DataTableConverter.GetValue(row.DataItem, "PET_WINDING_CMPL_FLAG_AFTER").GetString();

                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    // 정상처리되었습니다.
                        Util.gridClear(dgBobin);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button> { btnSearch, btnSave };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region [Func]
        private bool ValidationSearch()
        {
            if (string.IsNullOrWhiteSpace(txtBobinId.Text) && cboCstCleanFlag.SelectedValue.ToString() == "SELECT" && cboPetWindingCmplFlag.SelectedValue.ToString() == "SELECT")
            {
                // 조회조건 입력 후 조회해야합니다.
                Util.MessageValidation("SFU4494");
                Util.gridClear(dgBobin);
                return false;
            }

            if (txtBobinId.Text.Length > 0)
            {
                DataTable dt = DataTableConverter.Convert(dgBobin.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    DataRow[] dr = dt.Select("BOBBIN_ID = '" + txtBobinId.Text + "'");

                    if (dr.Length > 0)
                    {
                        // 해당 ID는 이미 존재합니다.
                        Util.MessageValidation("SFU2879");
                        txtBobinId.Text = string.Empty;

                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationSave()
        {
            if (_util.GetDataGridRowCountByCheck(dgBobin, "CHK", true) < 1 &&
                _util.GetDataGridRowCountByCheck(dgBobin, "CHK") < 1)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dgBobin.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                         Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                {
                    if(string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "CST_CLEAN_FLAG_AFTER").GetString()))
                    {
                        object[] parameters = new object[2];
                        parameters[0] = DataTableConverter.GetValue(row.DataItem, "BOBBIN_ID").GetString();
                        parameters[1] = ObjectDic.Instance.GetObjectName("CST_CLEAN_FLAG");
                        //보빈ID [%1] 의 [%2]를 확인하세요.
                        Util.MessageValidation("SFU8536", parameters);
                        return false;
                    }

                }
            }


            return true;
        }



        #endregion


    }
}