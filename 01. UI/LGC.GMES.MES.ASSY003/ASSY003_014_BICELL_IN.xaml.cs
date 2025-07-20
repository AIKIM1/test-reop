/*************************************************************************************
 Created Date : 2017.12.26
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - C생산 관리 - BI CELL 입고 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.26  INS 김동일K : Initial Created.

**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_BICELL_IN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_BICELL_IN : C1Window, IWorkArea
    {
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _ProcId = string.Empty;
        private string _txtUserName = string.Empty;
        private string _txtUserID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _EqgrId;


        CMM_PERSON wndPerson = null;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_014_BICELL_IN()
        {
            InitializeComponent();
        }

        private void LoadCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string[] sFilter2 = { "BICELL_TYPE_FD" };
            _combo.SetCombo(cboCellType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE");
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _LineName = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
                _txtUserName = Util.NVC(tmps[3]);
                _txtUserID = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _LineName = "";
                _EqptID = "";
                _txtUserName = "";
                _txtUserID = "";
            }
            
            if (wndPerson != null)
                wndPerson.BringToFront();

            if (txtUserName != null)
            {
                txtUserName.Text = _txtUserName;
                txtUserName.Tag = _txtUserID;
            }

            ApplyPermissions();
            
            ClearControls();

            LoadCombo();
        }

        private void cboCellType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnSearchProd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearControls();

                GetProdList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
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
                    dgProdList.SelectedIndex = idx;
                                        
                    txtPjt.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "PRJT_NAME"));
                    txtProd.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "PRODID"));
                    txtMarketType.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "MKT_TYPE_NAME"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (wndPerson != null)
                    wndPerson = null;

                wndPerson = new CMM_PERSON();
                wndPerson.FrameOperation = this.FrameOperation;

                if (wndPerson != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = txtUserName.Text;

                    C1WindowExtension.SetParameters(wndPerson, Parameters);

                    wndPerson.Closed += new EventHandler(wndPerson_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        wndPerson.ShowModal();
                        wndPerson.BringToFront();
                    }));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreate())
                    return;

                // 인계처리 하시겠습니까?
                Util.MessageConfirm("SFU2931", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateCProd();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);
            listAuth.Add(btnSearchProd);            

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

        private void ClearControls()
        {
            try
            {
                txtProd.Text = "";
                txtQty.Text = "";
                txtMarketType.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetProdList()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgProdList);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("CELLTYPE", typeof(string));
   
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PRODID"] = _ProcId;
                newRow["CELLTYPE"] = cboCellType.SelectedValue.Equals("") || cboCellType.SelectedValue.Equals("SELECT") ? null : cboCellType.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgProdList, bizResult, null, false);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
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

        private void wndPerson_Closed(object sender, EventArgs e)
        {
            wndPerson = null;

            CMM_PERSON wndPopup = sender as CMM_PERSON;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPopup.USERNAME;
                txtUserName.Tag = wndPopup.USERID;
            }
        }

        private bool CanCreate()
        {
            bool bRet = false;

            if (txtQty.Text.Length < 1)
            {
                Util.MessageValidation("SFU1684");  // 수량을 입력하세요.
                return bRet;
            }

            if (txtProd.Text.Trim().Equals(""))
            {
                // 제품을 선택하세요.
                Util.MessageValidation("SFU1895");
                return bRet;
            }

            if (Util.NVC(txtUserName.Text).Trim().Equals(""))
            {
                Util.MessageValidation("SFU1842");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void CreateCProd()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("TO_EQSGID", typeof(string));
                inDataTable.Columns.Add("TO_PROCID", typeof(string));
                inDataTable.Columns.Add("MOVE_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("IN_LOT");
                inLotTable.Columns.Add("WIPQTY", typeof(decimal));
                inLotTable.Columns.Add("CSTID", typeof(string));


                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROCID"] = _ProcId;
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                newRow = null;
                newRow = inLotTable.NewRow();

                decimal dQty = 0;
                decimal.TryParse(txtQty.Text, out dQty);
                newRow["WIPQTY"] = dQty;
                newRow["CSTID"] = "";

                inLotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
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
    }
}
