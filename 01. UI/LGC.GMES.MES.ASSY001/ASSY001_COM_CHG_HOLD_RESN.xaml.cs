/*************************************************************************************
 Created Date : 2019.06.18
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 자동 홀드 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.22  INS 김동일K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_COM_CHG_HOLD_RESN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_COM_CHG_HOLD_RESN : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Eqptid = string.Empty;
        private string _Eqsgid = string.Empty;
        private string _Procid = string.Empty;

        private Util _util = new Util();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY001_COM_CHG_HOLD_RESN()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                _Eqsgid = Util.NVC(tmps[0]);
                _Eqptid = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);

                ApplyPermissions();
                
                CommonCombo _combo = new CommonCombo();

                //HOLD 사유
                string[] sFilter = { "HOLD_LOT" };
                _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

                //확인여부
                string[] sFilter1 = { "EQPT_HOLD_VERIFICATION" };
                _combo.SetCombo(cboCnfYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

                string[] sFilter2 = { "EQPT_HOLD_TYPE_CODE" };
                _combo.SetCombo(cboEqptHoldType, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

                GetAutoHoldList();
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetAutoHoldList();
        }

        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            //{
            //    dtExpected.SelectedDateTime = DateTime.Now;

            //    //Util.AlertInfo("오늘이후날짜만지정가능합니다.");
            //    Util.MessageValidation("SFU1740");                
            //}
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        #endregion

        #region Mehod

        #region [BizCall]

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
                newRow["EQPTID"] = _Eqptid;
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

                        this.DialogResult = MessageBoxResult.OK;
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
                loadingIndicator.Visibility = Visibility.Visible;

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

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _Procid;
                newRow["EQSGID"] = _Eqsgid;
                newRow["EQPTID"] = _Eqptid;
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                newRow["HOLD_TYPE"] = Util.GetCondition(cboEqptHoldType, bAllNull: true);
                newRow["VERIFY_YN"] = Util.GetCondition(cboCnfYN, bAllNull: true);

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

                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, true);
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

        #endregion

        #region [Validation]
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

            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "EQPT_HOLD_CNFM_FLAG")).Equals("Y"))
            {
                Util.MessageValidation("PSS9138");
                return bRet;
            }

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
        #endregion

        #region [Func]

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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #endregion
        
    }
}
