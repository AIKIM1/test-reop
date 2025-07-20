/*************************************************************************************
 Created Date : 2022.02.11
      Creator : INS 김동일
   Decription : CT공정 - 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2022.02.11  INS 김동일 : Initial Created.

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
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_011_CREATE_OUTLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_011_CREATE_OUTLOT : C1Window, IWorkArea
    {
        private bool bSave = false;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _prodLotID = string.Empty;
        private string _prodWipSeq = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;


        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        
        public ASSY001_011_CREATE_OUTLOT()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LineID = Util.NVC(tmps[0]);
            _EqptID = Util.NVC(tmps[1]);
            _prodLotID = Util.NVC(tmps[2]);
            _prodWipSeq = Util.NVC(tmps[3]);
            _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[4]);
            _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[5]);
            
            ApplyPermissions();

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") ||
                _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                lblCST.Visibility = Visibility.Visible;
                txtOutCa.Visibility = Visibility.Visible;
            }
            else
            {
                lblCST.Visibility = Visibility.Collapsed;
                txtOutCa.Visibility = Visibility.Collapsed;
            }

            GetInputTermLot();
        }

        private void txtOutBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutBoxQty.Text, 0))
                {
                    txtOutBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutCa.Visibility == Visibility.Visible)
                        txtOutCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    System.Threading.Thread.Sleep(300);

                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutCa.Text = "";
                            txtOutCa.Focus();
                        });

                        return;
                    }

                    //2020-01-16 오화백 영문과 숫자만 들어가도록 로직 추가
                    bool outCa = Regex.IsMatch(txtOutCa.Text, @"^[a-zA-Z0-9]+$");

                    if (outCa == false)
                    {
                        Util.MessageValidation("SFU3674", (action) =>
                        {
                            txtOutCa.Text = "";
                            txtOutCa.Focus();
                        });

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnbtnCreate(object sender, RoutedEventArgs e)
        {
            if (!CanCreateBox())
                return;

            //"생산 수량 %1 개로 생성 하시겠습니까?"
            Util.MessageConfirm("SFU4888", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutBox();
                }
            }, txtOutBoxQty.Text);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool CanCreateBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgdLotInfo, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if ((Convert.ToDecimal(txtOutBoxQty.Text) % 1) > 0)
            {
                //Util.Alert("소수점 입력은 불가능 합니다. 수량을 확인해 주세요.");
                Util.MessageValidation("SFU2342");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if (txtOutCa.Visibility == Visibility.Visible && !CheckedUseCassette())  //Cassette 중복여부 체크.
            {
                txtOutCa.SelectAll();
                txtOutCa.Focus();
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void GetInputTermLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("PROD_LOTID");
                dt.Columns.Add("PROD_WIPSEQ");
                dt.Columns.Add("EQPTID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = _prodLotID;
                dr["PROD_WIPSEQ"] = _prodWipSeq;
                dr["EQPTID"] = _EqptID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_TERM_LOT_LIST_CI", "INDATA", "OUTDATA", dt, (resultDt, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.GridSetData(dgdLotInfo, resultDt, FrameOperation, false);
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
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void CreateOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgdLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgdLotInfo, "CHK")].DataItem, "LOTID")); ;
                newRow["PROD_LOTID"] = _prodLotID;
                newRow["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_CI", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException, (msgResult) =>
                            {
                                if (txtOutCa.Visibility == Visibility.Visible)
                                {
                                    txtOutCa.Text = "";
                                    txtOutCa.Focus();
                                }
                            });
                            return;
                        }
                        
                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfoAutoClosing("SFU1275");

                        GetInputTermLot();

                        bSave = true;
                        txtOutCa.Text = "";
                        
                        if (txtOutCa.Visibility == Visibility.Visible)
                            txtOutCa.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable dtIN_EQP = IndataSet.Tables.Add("IN_EQP");
                dtIN_EQP.Columns.Add("SRCTYPE", typeof(string));
                dtIN_EQP.Columns.Add("IFMODE", typeof(string));
                dtIN_EQP.Columns.Add("CSTID", typeof(string));
                dtIN_EQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow dr = dtIN_EQP.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                dr["WIP_TYPE_CODE"] = "OUT";
                dtIN_EQP.Rows.Add(dr);


                DataTable dtIN_INPUT = IndataSet.Tables.Add("IN_INPUT");
                dtIN_INPUT.Columns.Add("LANGID", typeof(string));
                dtIN_INPUT.Columns.Add("PROCID", typeof(string));
                dtIN_INPUT.Columns.Add("EQSGID", typeof(string));

                dr = dtIN_INPUT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CT_INSP;
                dr["EQSGID"] = _LineID;
                dtIN_INPUT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

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

                //row 색 바꾸기
                dgdLotInfo.SelectedIndex = idx;
            }
        }
    }
}
