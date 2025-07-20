/*************************************************************************************
 Created Date : 2017.08.10
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Folding 공정진척 화면 - C생산 인계 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.10  INS 김동일K : Initial Created.
  2017.12.10  CNS 고현영S : 패키지 팝업화면 추가 및 이력관리
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
    /// WND_CPROD_TRANSFER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WND_CPROD_TRANSFER : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _EqptName = string.Empty;
        private string _ProcId = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _EqgrId;


        CMM_PERSON wndPerson = null;

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

        public WND_CPROD_TRANSFER()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            DateTime dtNowTime = System.DateTime.Now;
            if (dtpDateFromCrt != null)
                dtpDateFromCrt.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            if (dtpDateToCrt != null)
                dtpDateToCrt.SelectedDateTime = dtNowTime;

            if (dtpDateFromHist != null)
                dtpDateFromHist.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-1);
            if (dtpDateToHist != null)
                dtpDateToHist.SelectedDateTime = dtNowTime;
        }

        private void LoadCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { _LineID, _ProcId, _EqgrId};
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "EQUIPMENT_BY_EQSGID");

            //String[] sFilter2 = { "", _EqptID };
            //_combo.SetCombo(cboTransfer, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "C_PRODUCT_TRANSFER");

            String[] sFilter2 = { LoginInfo.CFG_AREA_ID, null, Process.CPROD };
            _combo.SetCombo(cboTransfer, CommonCombo.ComboStatus.SELECT, sCase: "cboEquipmentSegmentAssy", sFilter: sFilter2);

            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sCase: "LOTTYPE");

        }
        #endregion

        #region Event       

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                _LineID = Util.NVC(tmps[0]);
                _LineName = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
                _ProcId = Util.NVC(tmps[3]);
                _EqgrId = Util.NVC(tmps[4]);
                _EqptName = Util.NVC(tmps[5]);
            }
            else
            {
                _LineID = "";
                _LineName = "";
                _EqptID = "";
                _ProcId = "";
                _EqptName = "";
            }

            if (wndPerson != null)
                wndPerson.BringToFront();

            ApplyPermissions();

            InitializeControls();

            ClearControls();

            LoadCombo();

            SetControls();
        }

        private void SetControls()
        {
            txtEquipmentSegment.Text = _LineName;
            cboEquipment.SelectedValue = _EqptID;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1TabControl c1TabControl = sender as C1TabControl;
            if (c1TabControl.IsLoaded)
            {
                if (c1tabCreate.IsSelected)
                {
                    btnCreate.Visibility = Visibility.Visible;
                }

                if (c1tabHist.IsSelected)
                {
                    btnCreate.Visibility = Visibility.Collapsed;
                }
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

                    txtEquipment.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "EQPTNAME"));
                    txtPjt.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "PRJT_NAME"));
                    txtProd.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "PRODID"));
                    //txtWrkEnd.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "WIPDTTM_ED"));
                    txtMarketType.Text = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "MKT_TYPE_NAME"));
                    cboLotType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[idx].DataItem, "LOTTYPE"));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                inDataTable.Columns.Add("LOTTYPE", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("IN_LOT");
                inLotTable.Columns.Add("WIPQTY", typeof(decimal));
                inLotTable.Columns.Add("CSTID", typeof(string));
                

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROCID"] = _ProcId;
                newRow["WOID"] = "";// Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProdList, "CHK")].DataItem, "WOID"));
                newRow["WO_DETL_ID"] = "";// Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProdList, "CHK")].DataItem, "WO_DETL_ID"));
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProdList, "CHK")].DataItem, "MKT_TYPE_CODE"));
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProdList, "CHK")].DataItem, "PRODID"));
                newRow["TO_EQSGID"] = Util.NVC(cboTransfer.SelectedValue);
                newRow["TO_PROCID"] = Process.CPROD;
                newRow["MOVE_USERID"] = Util.NVC(txtUserName.Tag);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOTTYPE"] = Util.NVC(cboLotType.SelectedValue);

                inDataTable.Rows.Add(newRow);

                newRow = null;
                newRow = inLotTable.NewRow();

                decimal dQty = 0;
                decimal.TryParse(txtQty.Text, out dQty);
                newRow["WIPQTY"] = dQty;
                newRow["CSTID"] = "";

                inLotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_CPROD_RWK_OUT_LOT", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        string sLOTID = searchResult.Tables["OUTDATA"].Rows[0]["LOTID"].ToString();

                        CProdSendPrint(sLOTID, dQty);

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

        private void CProdSendPrint(string sLotid, decimal dQty)
        {
            try
            {
                // 발행..
                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("LOTID", sLotid);
                //dicParam.Add("QTY", Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[i].DataItem, "SEND_QTY"))).ToString());
                dicParam.Add("QTY", Convert.ToDouble(txtQty.Text).ToString());
                dicParam.Add("EQSGNAME", _LineName);
                dicParam.Add("EQPTNAME", _EqptName);

                dicParam.Add("PRINTQTY", "1");  // 발행 수                
                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                dicList.Add(dicParam);
                

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = _LineID;
                    Parameters[3] = _EqptID;
                    Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                    Parameters[5] = "CREATE_CPRODUCT";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void dtpDateFromCrt_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (sender == null)
            //        return;

            //    LGCDatePicker dtPik = (sender as LGCDatePicker);

            //    if (dtpDateToCrt.SelectedDateTime.Subtract(dtPik.SelectedDateTime).Days > 7)
            //    {
            //        //dtPik.SelectedDateTime = dtpDateToCrt.SelectedDateTime.AddDays(-7);
            //        dtpDateToCrt.SelectedDateTime = dtPik.SelectedDateTime.AddDays(+7);
            //        // 조회 기간은 7일을 초과할 수 없습니다.
            //        Util.MessageValidation("SFU3567");
            //        return;
            //    }

            //    if (Convert.ToDecimal(dtpDateToCrt.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            //    {
            //        dtPik.SelectedDateTime = dtpDateToCrt.SelectedDateTime;
            //        Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
            //        //e.Handled = false;
            //        return;
            //    }

            //}));
        }

        private void dtpDateToCrt_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (sender == null)
            //        return;

            //    LGCDatePicker dtPik = (sender as LGCDatePicker);

            //    if (dtPik.SelectedDateTime.Subtract(dtpDateFromCrt.SelectedDateTime).Days > 7)
            //    {
            //        //dtPik.SelectedDateTime = dtpDateFromCrt.SelectedDateTime;
            //        dtpDateFromCrt.SelectedDateTime = dtPik.SelectedDateTime.AddDays(-7);
            //        // 조회 기간은 7일을 초과할 수 없습니다.
            //        Util.MessageValidation("SFU3567");
            //        return;
            //    }

            //    if (Convert.ToDecimal(dtpDateFromCrt.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            //    {
            //        dtPik.SelectedDateTime = dtpDateFromCrt.SelectedDateTime;
            //        //시작일자 이전 날짜는 선택할 수 없습니다.
            //        Util.MessageValidation("SFU1698");
            //        //e.Handled = false;
            //        return;
            //    }


            //    btnSearchProd_Click(null, null);
            //}));
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetCProdLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanPrint())
                    return;

                CProdThermalPrint();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgHistList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":

                                SetCHKBoxControls(e);

                                break;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                btnSearchProd_Click(sender, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetProdList()
        {
            try
            {
                string sBizName = string.Empty;

                if (_ProcId.Equals(Process.PACKAGING))
                    sBizName = "DA_PRD_SEL_CPROD_PROD_LIST_CL";
                else
                    sBizName = "DA_PRD_SEL_CPROD_PROD_LIST_FD";

                ShowLoadingIndicator();
                Util.gridClear(dgProdList);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(DateTime));
                inTable.Columns.Add("TODT", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcId;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = string.IsNullOrEmpty(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue;
                newRow["FRDT"] = dtpDateFromCrt.SelectedDateTime;
                newRow["TODT"] = dtpDateToCrt.SelectedDateTime;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
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

        private void CreateProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SEND_QTY", typeof(int));
                inTable.Columns.Add("CPROD_WRK_PSTN_ID", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _ProcId;
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = txtProd.Text;
                newRow["SEND_QTY"] = txtQty.Text;
                newRow["CPROD_WRK_PSTN_ID"] = cboTransfer.SelectedValue;
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgProdList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProdList, "CHK")].DataItem, "MKT_TYPE_CODE"));
                newRow["USERID"] = "";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_CPROD_LOT", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            AutoThermalPrint(Util.NVC(bizResult.Rows[0]["CPROD_LOTID"]), txtEquipment.Text, txtQty.Text);
                        }

                        ClearControls();

                        Util.DataGridCheckAllUnChecked(dgProdList);
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

        private void GetCProdLotList()
        {
            try
            {
                Util.gridClear(dgHistList);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(string));
                inTable.Columns.Add("TODT", typeof(string));
                inTable.Columns.Add("CPROD_RWK_LOT_EQSGID", typeof(string));
                inTable.Columns.Add("CPROD_RWK_LOT_EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FRDT"] = dtpDateFromHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpDateToHist.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CPROD_RWK_LOT_EQSGID"] = _LineID;
                newRow["CPROD_RWK_LOT_EQPTID"] = _EqptID;
                newRow["LOTID"] = String.IsNullOrEmpty(txtcProdLotHist.Text) ? null : txtcProdLotHist.Text;
                newRow["PRJT_NAME"] = String.IsNullOrEmpty(txtPjtNameHist.Text) ? null : txtPjtNameHist.Text;
                newRow["TO_PROCID"] = Process.CPROD;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_SEND_HIST", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgHistList, bizResult, null, false);
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

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);
            listAuth.Add(btnSearchProd);
            listAuth.Add(btnSearchHist);
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

        private void ClearControls()
        {
            try
            {
                txtEquipment.Text = "";
                txtProd.Text = "";
                txtQty.Text = "";
                //txtWrkEnd.Text = "";
                txtMarketType.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetCHKBoxControls(DataGridCellEventArgs e)
        {
            int selectedIndex = _Util.GetDataGridCheckFirstRowIndex(dgHistList, "CHK");
            int preValue = (int)e.Cell.Value;

            Util.DataGridCheckAllUnChecked(dgHistList);

            if (preValue > 0) e.Cell.Value = true;
            else e.Cell.Value = false;
        }

        private void AutoThermalPrint(string sLotid, string sEqutName, string sQty)
        {
            try
            {
                // 발행..
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("LOTID", sLotid);
                dicParam.Add("QTY", sQty);
                dicParam.Add("EQSGNAME", Util.NVC(_LineName.Split(new string[] { ":" }, StringSplitOptions.None)[1]));
                dicParam.Add("EQPTNAME", Util.NVC(txtEquipment.Text));

                dicParam.Add("PRINTQTY", "1");  // 발행 수                
                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT(dicParam);
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = null;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = _LineID;
                    Parameters[3] = _EqptID;
                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "M";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT;

            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void CProdThermalPrint()
        {
            try
            {
                // 발행..
                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                for (int i = 0; i < dgHistList.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgHistList, "CHK", i)) continue;


                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    dicParam.Add("LOTID", Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[i].DataItem, "LOTID")));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[i].DataItem, "SEND_QTY"))).ToString());
                    dicParam.Add("EQSGNAME", _LineName);
                    dicParam.Add("EQPTNAME", _EqptName);

                    dicParam.Add("PRINTQTY", "1");  // 발행 수                
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.

                    dicList.Add(dicParam);
                }

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_CPRODUCT();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = _LineID;
                    Parameters[3] = _EqptID;
                    Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                    Parameters[5] = "CREATE_CPRODUCT";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanCreate()
        {
            bool bRet = false;

            if (txtQty.Text.Length < 1)
            {
                Util.MessageValidation("SFU1684");  // 수량을 입력하세요.
                return bRet;
            }

            if (cboTransfer.SelectedIndex < 0 || cboTransfer.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("인계작업장을 선택하세요.");
                Util.MessageValidation("SFU3706");
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

            if (cboLotType.SelectedIndex < 0 || cboLotType.SelectedValue.ToString().Trim().Equals("SELECT") || cboLotType.SelectedValue == null)
            {
                Util.MessageValidation("SFU4068");  //LOT 유형을 선택하세요.
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgHistList, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #endregion
             
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanDelete())
                    return;

                Util.MessageConfirm("SFU4398", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteCProdLot();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteCProdLot()
        {
            try
            {
                ShowLoadingIndicator();

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgHistList, "CHK");
                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("MOVE_ORD_ID", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                DataTable dtLot = ds.Tables.Add("IN_LOT");
                dtLot.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _EqptID;
                dr["MOVE_ORD_ID"] = Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[idx].DataItem, "MOVE_ORD_ID"));
                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;

                dt.Rows.Add(dr);
                
                for (int i = 0; i < dgHistList.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgHistList, "CHK", i)) continue;

                    dr = dtLot.NewRow();
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgHistList.Rows[i].DataItem, "LOTID"));

                    dtLot.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SEND_CPROD_LOT", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1937");

                        GetCProdLotList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                },
                ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanDelete()
        {
            bool bRet = false;

            int index = _Util.GetDataGridCheckFirstRowIndex(dgHistList, "CHK");

            if (index < 0)
            {
                //선택된 LOT이 없습니다.
                Util.MessageValidation("SFU3529");
                return bRet;
            }

            //if (!DataTableConverter.GetValue(dgHistList.Rows[index].DataItem, "CPROD_LOT_STAT").Equals("WAIT"))
            //{
            //    //해당LOT은 삭제할 수 없는 상태입니다.
            //    Util.MessageValidation("SFU4298");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
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
    }
}
