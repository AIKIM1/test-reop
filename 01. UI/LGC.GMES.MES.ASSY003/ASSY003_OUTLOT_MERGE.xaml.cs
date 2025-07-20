/*************************************************************************************
 Created Date : 2017.08.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - LOT ID MERGE 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.14  INS 김동일K : Initial Created.
   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_OUTLOT_MERGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_OUTLOT_MERGE : C1Window, IWorkArea
    {   
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _Procid = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
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

        public ASSY003_OUTLOT_MERGE()
        {
            InitializeComponent();
        }
        
        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);
                _LineName = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _Procid = "";
                _LineName = "";
            }
            txtLotID.Focus();
            ApplyPermissions();                        
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataTable dtTmp = new DataTable();
                dtTmp.Columns.Add("CHK", typeof(Boolean));
                dtTmp.Columns.Add("LOTID", typeof(string));
                dtTmp.Columns.Add("WIPQTY", typeof(string));
                dtTmp.Columns.Add("PRODID", typeof(string));
                dtTmp.Columns.Add("PRODNAME", typeof(string));
                dtTmp.Columns.Add("PROCID", typeof(string));
                dtTmp.Columns.Add("PROCNAME", typeof(string));
                dtTmp.Columns.Add("WIPSTAT", typeof(string));
                dtTmp.Columns.Add("WIPSNAME", typeof(string));
                dtTmp.Columns.Add("EQSGID", typeof(string));
                dtTmp.Columns.Add("EQSGNAME", typeof(string));

                dgMerge.ItemsSource = DataTableConverter.Convert(dtTmp);
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetLotInfo();
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
                if (!CanSave()) return;

                Util.MessageConfirm("SFU2876", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MergeProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMergeChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
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
                    dgMerge.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgMerge == null || dgMerge.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            DataTable dt = DataTableConverter.Convert(dgMerge.ItemsSource);

                            DataRow[] dr = dt.Select("LOTID = '" + Util.NVC(dtRow["LOTID"]) + "'");

                            if (dr.Length > 0)
                            {
                                dt.Rows.Remove(dr[0]);
                            }
                            dt.AcceptChanges();

                            Util.GridSetData(dgMerge, dt, FrameOperation, false);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                List<double> listQty = dt.AsEnumerable().Select(row => double.Parse(Util.NVC(row["WIPQTY"]))).ToList();
                                txtTotQty.Text = listQty.Sum().ToString("#,###");
                            }
                            else
                            {
                                txtTotQty.Text = "0";
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtLotID.Text;

                if (Process.CPROD.Equals(_Procid))
                    newRow["WIPSTAT"] = null;
                else
                    newRow["WIPSTAT"] = "WAIT";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MERGE_LOT_INFO", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        txtLotID.Text = "";

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        AddLotList(bizResult);
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

        private void MergeProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");                

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inFrom = inDataSet.Tables.Add("IN_FROMLOT");

                inFrom.Columns.Add("FROM_LOTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[_util.GetDataGridCheckFirstRowIndex(dgMerge, "CHK")].DataItem, "LOTID"));
                newRow["NOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgMerge.Rows.Count; i++)
                {
                    if (_util.GetDataGridCheckValue(dgMerge, "CHK", i)) continue;

                    newRow = null;

                    newRow = inFrom.NewRow();

                    newRow["FROM_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[i].DataItem, "LOTID"));

                    inFrom.Rows.Add(newRow);
                }
                 

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_INOUT_LOT_ASSY", "INDATA,IN_FROMLOT", null, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        
                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        Util.gridClear(dgMerge);

                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, inDataSet);
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
            listAuth.Add(btnAdd);
            listAuth.Add(btnSave);
            
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

        private void AddLotList(DataTable dtRslt)
        {
            try
            {
                if (dtRslt == null) return;

                if (dtRslt.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1386");   // LOT정보가 없습니다.
                    return;
                }

                DataTable dtList = DataTableConverter.Convert(dgMerge.ItemsSource);

                if (dtList != null)
                {
                    DataRow[] dtRows = dtList.Select("LOTID = '" + Util.NVC(dtRslt.Rows[0]["LOTID"]) + "'");

                    if (dtRows.Length > 0)
                    {
                        Util.MessageInfo("SFU1508");   // 동일한 LOT ID가 있습니다.
                        return;
                    }

                    double dQty = 0;
                    double.TryParse(Util.NVC(dtRslt.Rows[0]["WIPQTY"]), out dQty);

                    DataRow dtRow = dtList.NewRow();
                    dtRow["CHK"] = 0;
                    dtRow["LOTID"] = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    dtRow["WIPQTY"] = dQty;
                    dtRow["PRODID"] = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                    dtRow["PRODNAME"] = Util.NVC(dtRslt.Rows[0]["PRODNAME"]);
                    dtRow["PROCID"] = Util.NVC(dtRslt.Rows[0]["PROCID"]);
                    dtRow["PROCNAME"] = Util.NVC(dtRslt.Rows[0]["PROCNAME"]);
                    dtRow["WIPSTAT"] = Util.NVC(dtRslt.Rows[0]["WIPSTAT"]);
                    dtRow["WIPSNAME"] = Util.NVC(dtRslt.Rows[0]["WIPSNAME"]);
                    dtRow["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);
                    dtRow["EQSGNAME"] = Util.NVC(dtRslt.Rows[0]["EQSGNAME"]);

                    dtList.Rows.Add(dtRow);

                    dtList.AcceptChanges();
                    
                    Util.GridSetData(dgMerge, dtList, FrameOperation, false);

                    if (dtList != null && dtList.Rows.Count > 0)
                    {
                        List<double> listQty = dtList.AsEnumerable().Select(row => double.Parse(Util.NVC(row["WIPQTY"]))).ToList();
                        txtTotQty.Text = listQty.Sum().ToString("#,###");
                    }
                    else
                    {
                        txtTotQty.Text = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanAdd()
        {
            bool bRet = false;

            //if (txtQty.Text.Length < 1)
            //{
            //    Util.MessageValidation("SFU1684");  // 수량을 입력하세요.
            //    return bRet;
            //}

            //if (cboTransfer.SelectedIndex < 0 || cboTransfer.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("인계작업장을 선택하세요.");
            //    Util.MessageValidation("SFU3706");
            //    return bRet;
            //}

            //if (txtProd.Text.Trim().Equals(""))
            //{
            //    // 제품을 선택하세요.
            //    Util.MessageValidation("SFU1895");
            //    return bRet;
            //}

            bRet = true;

            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;
                        
            DataTable dtTmp = DataTableConverter.Convert(dgMerge.ItemsSource);

            if (dtTmp == null || dtTmp.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3708");  // Merge할 Lot이 없습니다.
                return bRet;
            }
            else if (dtTmp.Rows.Count == 1)
            {
                Util.MessageValidation("SFU3709");  // 최소 Lot이 2개 있어야 Merge할 수 있습니다.
                return bRet;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMerge, "CHK") < 0)
            {
                Util.MessageValidation("SFU3707");  // Merge Lot을 선택하세요.
                return bRet;
            }

            // 동일 공정 체크
            List<string> list = dtTmp.AsEnumerable().Select(row => Util.NVC(row["PROCID"])).Distinct().ToList();
            if (list != null && list.Count > 1)
            {
                Util.MessageValidation("SFU3600");  // 동일 공정이 아닙니다.
                return bRet;
            }

            // 동일 제품 체크
            List<string> list2 = dtTmp.AsEnumerable().Select(row => Util.NVC(row["PRODID"])).Distinct().ToList();
            if (list2 != null && list2.Count > 1)
            {
                Util.MessageValidation("SFU1502");  // 동일 제품이 아닙니다.
                return bRet;
            }

            // 대기 상태 체크
            if (!_Procid.Equals(Process.CPROD))
            {
                DataRow[] dtRow = dtTmp.Select("WIPSTAT <> 'WAIT'");
                if (dtRow != null && dtRow.Length > 0)
                {
                    Util.MessageValidation("SFU3710");  // 대기 상태의 Lot만 Merge 가능 합니다.
                    return bRet;
                }
            }

            // 동일 라인 체크
            DataRow[] dtRow2 = dtTmp.Select("EQSGID <> '" + _LineID + "'");
            if (dtRow2 != null && dtRow2.Length > 0)
            {
                Util.MessageValidation("SFU3711", _LineName);  // %1 Line의 LOT만 Merge 가능 합니다.
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #endregion
    }
}
