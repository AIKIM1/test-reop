/*************************************************************************************
 Created Date : 2019.07.17
      Creator : INS 김동일K
   Decription : CWA3, CNB 설비 자동HOLD 전극 확인 및 사유 입력
--------------------------------------------------------------------------------------
 [Change History]
  2019.07.17  INS 김동일K : Initial Created.

**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_133.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_133 : UserControl, IWorkArea
    {
        private Util _util = new Util();

        public COM001_133()
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
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearch);
                listAuth.Add(btnSave);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                this.Loaded -= UserControl_Loaded;

                InitCombo();

                chkHoldLevel.IsChecked = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다

                    return;
                }
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.

                    return;
                }
            }));
        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    GetAutoHoldList();
                    txtLotId.Text = "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetAutoHoldList();
        }

        private void dgHoldLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void txtPerson_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtPerson.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst, (dtRslt, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (dtRslt.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU1592");  //사용자 정보가 없습니다.
                            }
                            else if (dtRslt.Rows.Count == 1)
                            {
                                txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                                txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                                txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                            }
                            else
                            {
                                dgPersonSelect.Visibility = Visibility.Visible;

                                Util.gridClear(dgPersonSelect);

                                dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                                this.Focusable = true;
                                this.Focus();
                                this.Focusable = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            HideLoadingIndicator();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    HideLoadingIndicator();
                }
            }
        }

        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveHoldResn();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboProcess };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정        
            C1ComboBox[] cboProcParent = { cboArea };
            C1ComboBox[] cboProcChild = { cboEquipmentSegment, cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcChild, sCase: "PROCESS_BY_AREAID_PCSG", cbParent: cboProcParent);

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //라인
            C1ComboBox[] cboEqsgParent = { cboArea, cboProcess };
            C1ComboBox[] cboEqsgChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgChild, cbParent: cboEqsgParent, sCase: "PROCESSEQUIPMENTSEGMENT");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //HOLD 사유
            string[] sFilter2 = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter2);

            //확인여부
            string[] sFilter3 = { "EQPT_HOLD_VERIFICATION" };
            _combo.SetCombo(cboCnfYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboCnfYN != null)
                cboCnfYN.SelectedValue = "N";

            string[] sFilter4 = { "EQPT_HOLD_TYPE_CODE" };
            _combo.SetCombo(cboEqptHoldType, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "COMMCODE");


        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private bool CanSave()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            //if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "EQPT_HOLD_CNFM_FLAG")).Equals("Y"))
            //{
            //    Util.MessageValidation("PSS9138");
            //    return bRet;
            //}

            if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("HOLD 사유를 선택 하세요.");
                Util.MessageValidation("SFU1342");
                return bRet;
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                //Util.Alert("LOT HOLD 에 대한 설명을 입력해 주세요.");
                Util.MessageValidation("SFU1214");
                return bRet;
            }

            if (txtPersonId.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU4011");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void Clear()
        {
            Util.gridClear(dgList);
            txtLotId.Text = "";
            txtPerson.Text = "";
            txtPersonDept.Text = "";
            txtPersonId.Text = "";
            txtRemark.Text = "";
        }

        private void SaveHoldResn()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInputLot = indataSet.Tables.Add("IN_LOT");
                inInputLot.Columns.Add("LOTID", typeof(string));
                inInputLot.Columns.Add("WIPSEQ", typeof(int));
                inInputLot.Columns.Add("HOLD_DTTM", typeof(DateTime));
                inInputLot.Columns.Add("HOLD_CODE", typeof(string));
                inInputLot.Columns.Add("HOLD_NOTE", typeof(string));
                inInputLot.Columns.Add("HOLD_USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.GetCondition(cboEquipment);
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dgList.Rows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    newRow = inInputLot.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = int.Parse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "WIPSEQ")));
                    newRow["HOLD_DTTM"] = (DateTime)DataTableConverter.GetValue(dgList.Rows[i].DataItem, "HOLD_DTTM_ORG");
                    newRow["HOLD_CODE"] = cboHoldReason.SelectedValue;
                    newRow["HOLD_NOTE"] = txtRemark.Text;
                    newRow["HOLD_USERID"] = txtPersonId.Text;

                    inInputLot.Rows.Add(newRow);
                }

                if (inInputLot.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    HideLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT_EQPT_HOLD_TYPE_CODE", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1275");

                        Clear();
                        GetAutoHoldList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void GetAutoHoldList()
        {
            try
            {
                if (txtLotId.Text.Trim().Length < 1 && (Util.NVC(cboProcess?.SelectedValue).Equals("") || Util.NVC(cboProcess?.SelectedValue).IndexOf("SELECT") >= 0))
                {
                    Util.MessageValidation("SFU1459");
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = null;

                //BizDataSet에 메소드를 추가하지 않고 직접 칼럼을 작성을 했습니다.
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("STDT", typeof(string));
                inDataTable.Columns.Add("EDDT", typeof(string));
                inDataTable.Columns.Add("HOLD_TYPE", typeof(string));
                inDataTable.Columns.Add("VERIFY_YN", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                if (txtLotId.Text.Trim().Length > 0)
                {
                    newRow["LOTID"] = txtLotId.Text;
                }
                else
                {
                    newRow["PROCID"] = Util.GetCondition(cboProcess);
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    newRow["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["HOLD_TYPE"] = Util.GetCondition(cboEqptHoldType, bAllNull: true);
                    newRow["VERIFY_YN"] = Util.GetCondition(cboCnfYN, bAllNull: true);
                }

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_AUTO_HOLD_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgList, searchResult, FrameOperation, false);

                        txtLotId.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void chkHoldLevel_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;

                if (cboHoldReason.Items.Count > 1)
                    cboHoldReason.SelectedIndex = 0;

                if (checkBox.IsChecked == true)
                {
                    GetAreaDefectCode();
                    cboHoldReason.IsEnabled = false;
                }
                else
                {
                    Util.gridClear(dgHoldGroup1);
                    Util.gridClear(dgHoldGroup2);
                    cboHoldReason.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton radioButton = sender as RadioButton;

                if (radioButton.DataContext == null)
                    return;

                if ((bool)radioButton.IsChecked)
                {
                    C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = (C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent;
                    if (cellPresenter != null)
                    {
                        C1.WPF.DataGrid.C1DataGrid dataGrid = cellPresenter.DataGrid;
                        int rowIdx = cellPresenter.Row.Index;
                        //dataGrid.SelectedIndex = rowIdx;

                        if (string.Equals(radioButton.GroupName, "radHoldGroup1"))
                        {
                            if (cboHoldReason.Items.Count > 1)
                                cboHoldReason.SelectedIndex = 0;

                            GetAreaDefectDetailCode(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "DFCT_CODE")));
                        }
                        else if (string.Equals(radioButton.GroupName, "radHoldGroup2"))
                        {
                            cboHoldReason.SelectedValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "RESNCODE"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAreaDefectCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dataRow["ACTID"] = "HOLD_LOT";

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup1, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetAreaDefectDetailCode(string sDefectCode)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = Util.NVC(cboArea.SelectedValue);
                dataRow["ACTID"] = "HOLD_LOT";
                dataRow["DFCT_CODE"] = sDefectCode;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_DETL_CODE", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgHoldGroup2, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }
    }
}
