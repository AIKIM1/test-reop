using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_098.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_098 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_098()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            try
            {
                DataTable dtTemp = new DataTable();
                dtTemp.Columns.Add("NAME", typeof(string));
                dtTemp.Columns.Add("CODE", typeof(string));

                // Line Combo
                DataRow newRow = dtTemp.NewRow();

                newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { "EGL #2(M7G02)", "M7G02" };
                dtTemp.Rows.Add(newRow);

                newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { "EGL #1(M7G01)", "M7G01" };
                dtTemp.Rows.Add(newRow);

                cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();
                cboEquipmentSegment.DisplayMemberPath = "NAME";
                cboEquipmentSegment.SelectedValuePath = "CODE";
                cboEquipmentSegment.SelectedIndex = 1;

                dtTemp.Clear();

                // Model Combo
                //newRow = dtTemp.NewRow();

                //newRow = dtTemp.NewRow();
                //newRow.ItemArray = new object[] { "MCF40A760A10 [X1133]", "MCF40A760A10" };
                //dtTemp.Rows.Add(newRow);

                //newRow = dtTemp.NewRow();
                //newRow.ItemArray = new object[] { "MCF409858A10 [X1132]", "MCF409858A10" };
                //dtTemp.Rows.Add(newRow);

                //cboModel.ItemsSource = dtTemp.Copy().AsDataView();
                //cboModel.DisplayMemberPath = "NAME";
                //cboModel.SelectedValuePath = "CODE";
                //cboModel.SelectedIndex = 0;

                //dtTemp.Clear();

                SetModelCombo(cboModel);

                // Lami Cell Type Combo
                newRow = dtTemp.NewRow();

                newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { "M-Mono (MC)", "MC" };
                dtTemp.Rows.Add(newRow);

                newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { "L-Mono (ML)", "ML" };
                dtTemp.Rows.Add(newRow);

                newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { "R-Type BiCell (RT)", "RT" };
                dtTemp.Rows.Add(newRow);

                cboCellType.ItemsSource = dtTemp.Copy().AsDataView();
                cboCellType.DisplayMemberPath = "NAME";
                cboCellType.SelectedValuePath = "CODE";
                cboCellType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetModelCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_EGL_MODEL_CBO_TEMP";
            string[] arrColumn = { "EQSGID" };
            string[] arrCondition = { cboEquipmentSegment.SelectedValue.ToString()};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {            
            try
            {
                InitCombo();

                DataTable dt = new DataTable();
                dt.Columns.Add("LINE", typeof(string));
                dt.Columns.Add("MODEL", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("CELL_TYPE", typeof(string));
                dt.Columns.Add("QTY", typeof(decimal));

                dgList.ItemsSource = DataTableConverter.Convert(dt);

                //cboModel.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                //if (cboEquipmentSegment == null || cboEquipmentSegment.SelectedValue == null)
                //    return;


                //if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals("M7P02"))
                //{
                //    if (cboModel != null && cboModel.Items.Count > 0)
                //    {
                //        cboModel.SelectedValue = "MCF40A760A10";
                //    }
                //}
                //else if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals("M7P01"))
                //{
                //    if (cboModel != null && cboModel.Items.Count > 0)
                //    {
                //        cboModel.SelectedValue = "MCF409858A10";
                //    }
                //}

                if (cboEquipmentSegment == null || cboEquipmentSegment.SelectedValue == null)
                    return;

                SetModelCombo(cboModel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboModel_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtQty.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboElectType_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

        }

        private void txtQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtQty.Text, 1))
                {
                    txtQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (!CanAdd())
                        return;

                    DataTable dtTemp = DataTableConverter.Convert(dgList.ItemsSource);

                    DataRow newRow = dtTemp.NewRow();
                    newRow.ItemArray = new object[] { cboEquipmentSegment.SelectedValue, cboModel.SelectedValue, txtLotID.Text.Trim(), cboCellType.SelectedValue, txtQty.Text };
                    dtTemp.Rows.Add(newRow);

                    dtTemp.AcceptChanges();
                    dgList.ItemsSource = DataTableConverter.Convert(dtTemp);

                    txtLotID.Text = "";
                    txtQty.Text = "";

                    txtLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanAdd())
                    return;

                DataTable dtTemp = DataTableConverter.Convert(dgList.ItemsSource);

                DataRow newRow = dtTemp.NewRow();
                newRow.ItemArray = new object[] { cboEquipmentSegment.SelectedValue, cboModel.SelectedValue, txtLotID.Text.Trim(), cboCellType.SelectedValue, txtQty.Text };
                dtTemp.Rows.Add(newRow);

                dtTemp.AcceptChanges();
                dgList.ItemsSource = DataTableConverter.Convert(dtTemp);

                txtLotID.Text = "";
                txtQty.Text = "";

                txtLotID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDupDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgList == null || dgList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                            DataRow[] dr = dt.Select("LOTID = '" + Util.NVC(dtRow["LOTID"]) + "'");

                            if (dr.Length > 0)
                            {
                                dt.Rows.Remove(dr[0]);
                            }
                            dt.AcceptChanges();
                            dgList.ItemsSource = DataTableConverter.Convert(dt);
                        }
                    }
                });
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
                if (!CanSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveProcess();
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
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private bool CanAdd()
        {
            bool bRet = false;

            if (cboEquipmentSegment == null || cboEquipmentSegment.SelectedValue == null || Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
            {
                Util.MessageValidation("SFU1223");  // 라인을 선택하세요.
                return bRet;
            }

            if (cboModel == null || cboModel.SelectedValue == null || Util.NVC(cboModel.SelectedValue).Equals("") || Util.NVC(cboModel.SelectedValue).Equals("SELECT"))
            {
                Util.MessageValidation("SFU1225");  // 모델을 선택하세요.
                return bRet;
            }

            if (cboCellType == null || cboCellType.SelectedValue == null || Util.NVC(cboCellType.SelectedValue).Equals(""))
            {
                Util.MessageValidation("SFU1314");  // Bi-Cell Type를 선택 하세요.
                return bRet;
            }

            if (txtLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");  // LOT을 입력해주세요.
                return bRet;
            }

            if (txtQty.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1684");  // 수량을 입력하세요.
                return bRet;
            }

            //if (txtLotID.Text.Length != 10)
            //{
            //    Util.MessageValidation("90113");  // 입력된 생산LOT의 ID의 길이가 상이합니다.
            //    return bRet;
            //}

            if (dgList.Rows.Count > 0)
            {
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                DataRow[] dr1 = dt.Select("LOTID = '" + txtLotID.Text.Trim() + "'");

                if (dr1.Length > 0)
                {
                    Util.MessageValidation("SFU1196");  // Lot이 이미 추가되었습니다.

                    txtLotID.Text = "";
                    txtQty.Text = "";

                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (dgList == null || dgList.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1498");  // 데이터가 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void SaveProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MODLID", typeof(string));
                inTable.Columns.Add("CELL_TYPE", typeof(string));
                inTable.Columns.Add("WIPQTY", typeof(decimal));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                DataRow newRow;

                for (int i = 0; i < dgList.Rows.Count; i++)
                {
                    newRow = inTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                    newRow["MODLID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "MODEL"));
                    newRow["CELL_TYPE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CELL_TYPE"));
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "QTY")));
                    newRow["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LINE"));
                    newRow["USERID"] = LoginInfo.USERID;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_CREATE_SRC_WAIT_LOT_TEMP", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                        Util.gridClear(dgList);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
    }
}
