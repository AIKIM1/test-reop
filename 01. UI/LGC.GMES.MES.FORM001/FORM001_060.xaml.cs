/*************************************************************************************
 Created Date : 2020.07.06
      Creator : INS 김동일K
   Decription : Off-Line 보관 장기재고 관리 [C20200603-000041]
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.21  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_060.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_060 : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        public FORM001_060()
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
                ApplyPermissions();

                InitDetailInfo();

                //제품 
                SearchProduct();

                this.dtpDateFrom.SelectedDateTime = this.dtpDateTo.SelectedDateTime.AddMonths(-1);

                this.Loaded -= UserControl_Loaded;
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
                if (Util.NVC(cboShop.SelectedValue).Equals("") || Util.NVC(cboShop.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU3561");  // 공장을 입력하세요.
                    return;
                }

                InitDetailInfo();

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InitDetailInfo();

                cboNewShop.SelectedValue = cboShop.SelectedValue;
                cboNewArea.IsEnabled = true;
                cboNewLocation.IsEnabled = true;
                popSearchProdID.IsEnabled = true;

                txtNewPjtName.Text = "";
                txtAvailableQty.Value = 0;
                txtAvailableQty_Prv.Value = 0;
                txtHoldQty.Value = 0;
                txtHoldQty_Prv.Value = 0;
                txtSumQty.Value = 0;
                txtRemark.Text = "";
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
                #region Save Validation
                if (Util.NVC(cboNewShop.SelectedValue).Equals("") || Util.NVC(cboNewShop.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU3561");  // 공장을 입력하세요.
                    return;
                }

                if (Util.NVC(cboNewArea.SelectedValue).Equals("") || Util.NVC(cboNewArea.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU1499");  // 동을 선택하세요.
                    return;
                }
                
                if (Util.NVC(cboNewLocation.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    Util.MessageValidation("SFU4136");  // 저장위치를 선택해 주세요.
                    return;
                }
                else
                {
                    string sLocID = string.Empty;

                    sLocID = Util.NVC(cboNewLocation.SelectedValue);

                    if (sLocID.Equals(""))
                    {
                        int idx = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                        if (idx >= 0)
                            sLocID = Util.NVC(DataTableConverter.GetValue(dgList.Rows[idx].DataItem, "SLOC_ID"));
                    }

                    if (!ChkOffLineSLoc(sLocID))
                    {
                        Util.MessageValidation("SFU3758", sLocID);  // 저장위치 [%1]는 Off-Line 장기보관으로 설정된 위치가 아닙니다.
                        return;
                    }
                }

                if (Util.NVC(popSearchProdID.SelectedValue).Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1895");  // 제품을 선택하세요.
                    return;
                }
                
                if (txtAvailableQty.Value.ToString() == double.NaN.ToString())
                {
                    Util.MessageValidation("SFU3756");  // 가용 수량을 입력해 주세요.
                    return;
                }

                if (txtHoldQty.Value.ToString() == double.NaN.ToString())
                {
                    Util.MessageValidation("SFU3757");  // 보류 수량을 입력해 주세요.
                    return;
                }

                if (Util.NVC(txtRemark.Text).Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1590");  // 비고를 입력 하세요.
                    return;
                }

                
                #endregion


                double dAvQty, dHoQty, dAvQtyPrv, dHoQtyPrv;
                if (txtAvailableQty.Value.ToString() == double.NaN.ToString())
                    dAvQty = 0;
                else
                    dAvQty = txtAvailableQty.Value;

                if (txtAvailableQty_Prv.Value.ToString() == double.NaN.ToString())
                    dAvQtyPrv = 0;
                else
                    dAvQtyPrv = txtAvailableQty_Prv.Value;

                if (txtHoldQty.Value.ToString() == double.NaN.ToString())
                    dHoQty = 0;
                else
                    dHoQty = txtHoldQty.Value;

                if (txtHoldQty_Prv.Value.ToString() == double.NaN.ToString())
                    dHoQtyPrv = 0;
                else
                    dHoQtyPrv = txtHoldQty_Prv.Value;

                // 저장 하시겠습니까? ( 가용 : [%1] -> [%2], 보류 : [%3] -> [%4] )
                Util.MessageConfirm("SFU3755", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                }, dAvQtyPrv, dAvQty, dHoQtyPrv, dHoQty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    Util.MessageValidation("SFU2042", "31");    //기간은 {0}일 이내 입니다.
                //    return;
                //}

                GetHistInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ChkOffLineSLoc(string sLocID)
        {
            try
            {
                bool bRet = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(cboNewShop.SelectedValue);
                dr["AREAID"] = Util.NVC(cboNewArea.SelectedValue);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_OFF_LINE_USE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0)
                {
                    DataRow[] selRow = dtResult.Select("CBO_CODE = '" + sLocID + "'");

                    if (selRow?.Length > 0)
                        bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SearchProduct()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_PRODUCT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HideLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("SLOC_ID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("AVAIL_QTY", typeof(decimal));
                inDataTable.Columns.Add("HOLD_QTY", typeof(decimal));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SHOPID"] = Util.NVC(cboNewShop.SelectedValue);
                newRow["AREAID"] = Util.NVC(cboNewArea.SelectedValue);
                newRow["SLOC_ID"] = Util.NVC(cboNewLocation.SelectedValue);
                newRow["PRODID"] = Util.NVC(popSearchProdID.SelectedValue);
                newRow["AVAIL_QTY"] = txtAvailableQty.Value.ToString() == double.NaN.ToString() ? 0 : txtAvailableQty.Value;
                newRow["HOLD_QTY"] = txtHoldQty.Value.ToString() == double.NaN.ToString() ? 0 : txtHoldQty.Value;
                newRow["NOTE"] = txtRemark.Text;
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OFF_LINE_STCK_INFO", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //저장 되었습니다.
                        Util.MessageInfo("SFU1270");

                        InitDetailInfo();

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                        this.Dispatcher.BeginInvoke(new Action(() => btnSearchHist_Click(null, null)));
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
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetHistInfo()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = Util.NVC(cboShop.SelectedValue);
                newRow["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                newRow["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OFF_LINE_STCK_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInfoHist, searchResult, FrameOperation, true);
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

        private void GetList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("SLOC_ID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = Util.NVC(cboShop.SelectedValue);
                newRow["AREAID"] = Util.NVC(cboArea.SelectedValue).Equals("") ? null : Util.NVC(cboArea.SelectedValue);
                newRow["SLOC_ID"] = Util.NVC(cboLocation.SelectedValue).Equals("") ? null : Util.NVC(cboLocation.SelectedValue);
                newRow["PRODID"] = txtProductId.Text.Trim().Equals("") ? null : txtProductId.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OFF_LINE_STCK_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgList, searchResult, FrameOperation, true);
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

        private void ShowLoadingIndicator()
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnNew);
            listAuth.Add(btnSave);
            listAuth.Add(btnSearch);
            listAuth.Add(btnSearchHist);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "A" };
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");
            
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboLocation };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbParent: cboAreaParent, cbChild: cboAreaChild);
            
            C1ComboBox[] cboLocationParent = { cboShop, cboArea };
            _combo.SetCombo(cboLocation, CommonCombo.ComboStatus.ALL, cbParent: cboLocationParent, sCase: "OFF_LINE_USE_SLOC");


            
            C1ComboBox[] cboNewShopChild = { cboNewArea };
            _combo.SetCombo(cboNewShop, CommonCombo.ComboStatus.SELECT, cbChild: cboNewShopChild, sFilter: sFilter, sCase: "SHOP_AREATYPE");

            C1ComboBox[] cboNewAreaParent = { cboNewShop };
            C1ComboBox[] cboNewAreaChild = { cboNewLocation };
            _combo.SetCombo(cboNewArea, CommonCombo.ComboStatus.SELECT, cbParent: cboNewAreaParent, cbChild: cboNewAreaChild, sCase: "cboArea");

            C1ComboBox[] cboNewLocationParent = { cboNewShop, cboNewArea };
            _combo.SetCombo(cboNewLocation, CommonCombo.ComboStatus.SELECT, cbParent: cboNewLocationParent, sCase: "OFF_LINE_USE_SLOC");
        }

        private void InitDetailInfo()
        {
            cboShop.IsEnabled = false;

            cboNewShop.SelectedIndex = 0;
            cboNewShop.IsEnabled = false;

            cboNewArea.SelectedIndex = 0;
            cboNewArea.IsEnabled = false;

            cboNewLocation.SelectedIndex = 0;
            cboNewLocation.IsEnabled = false;

            popSearchProdID.SelectedText = "";
            popSearchProdID.SelectedValue = "";
            popSearchProdID.IsEnabled = false;

            txtNewPjtName.Text = "";
            txtNewPjtName.IsEnabled = false;

            txtAvailableQty.Value = double.NaN;
            txtHoldQty.Value = double.NaN;
            txtSumQty.Value = double.NaN;
            txtSumQty.IsEnabled = false;
            txtRemark.Text = "";
        }

        private void txtAvailableQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                double dAvaQty, dHoldQty;
                if (txtAvailableQty.Value.ToString() == double.NaN.ToString())
                    dAvaQty = 0;
                else
                    dAvaQty = txtAvailableQty.Value;

                if (txtHoldQty.Value.ToString() == double.NaN.ToString())
                    dHoldQty = 0;
                else
                    dHoldQty = txtHoldQty.Value;

                txtSumQty.Value = dAvaQty + dHoldQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHoldQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                double dAvaQty, dHoldQty;
                if (txtAvailableQty.Value.ToString() == double.NaN.ToString())
                    dAvaQty = 0;
                else
                    dAvaQty = txtAvailableQty.Value;

                if (txtHoldQty.Value.ToString() == double.NaN.ToString())
                    dHoldQty = 0;
                else
                    dHoldQty = txtHoldQty.Value;

                txtSumQty.Value = dAvaQty + dHoldQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                        else
                            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                    }

                    //row 색 바꾸기
                    dgList.SelectedIndex = idx;

                    // 상세 정보 조회
                    ListClicked(idx);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ListClicked(int iRow)
        {
            if (iRow < 0)
                return;

            InitDetailInfo();

            if (!_Util.GetDataGridCheckValue(dgList, "CHK", iRow))
                return;

            double dAvQty = 0;
            double dHoQty = 0;
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "AVAIL_QTY")), out dAvQty);
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "HOLD_QTY")), out dHoQty);

            cboNewShop.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "SHOPID"));
            cboNewArea.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "AREAID"));
            cboNewLocation.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "SLOC_ID"));
            popSearchProdID.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRODID"));
            popSearchProdID.SelectedText = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRODID")) + " : " + Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRODNAME"));
            txtNewPjtName.Text = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRJT_NAME"));
            txtAvailableQty.Value = dAvQty;
            txtAvailableQty_Prv.Value = dAvQty;
            txtHoldQty.Value = dHoQty;
            txtHoldQty_Prv.Value = dHoQty;
            txtSumQty.Value = txtAvailableQty.Value + txtHoldQty.Value;
            txtRemark.Text = Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "NOTE"));
        }
    }
}
