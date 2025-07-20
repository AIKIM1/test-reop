/*************************************************************************************
 Created Date : 2018.01.30
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면 - 부분폐기 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.30  INS 김동일K : Initial Created.
  
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
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_001_WAITLOT_PART_SCRAP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_001_WAITLOT_PART_SCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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
        public ASSY003_001_WAITLOT_PART_SCRAP()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }
            ApplyPermissions();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanScrap())
                    return;

                Util.MessageConfirm("SFU3533", (result) => 
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PartScrap();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancelScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCancelScrap())
                    return;

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        PartScrapCancel();
                    }
                });
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
                dgScrap.IsReadOnly = true;

                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                string lotId = Util.NVC(DataTableConverter.GetValue(dgScrap.Rows[index].DataItem, "LOTID"));
                DataTable dt = DataTableConverter.Convert(dgScrap.ItemsSource);
                dt.Rows.RemoveAt(index);

                //dgScrap.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dgScrap, dt, FrameOperation, true);

                foreach (C1.WPF.DataGrid.DataGridRow boxListRow in dgScrap.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(boxListRow.DataItem, "LOTID")).Equals(lotId))
                    {
                        DataTableConverter.SetValue(boxListRow.DataItem, "CHK", 0);

                        break;
                    }
                }

                dgScrap.IsReadOnly = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["LOTID"] = txtWaitPancakeLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_BY_LOTID_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        txtWaitPancakeLot.Text = "";

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (dgScrap.GetRowCount() == 0)
                        {
                            dgScrap.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgScrap.ItemsSource);
                            dtSource.Merge(searchResult);

                            Util.gridClear(dgScrap);
                            dgScrap.ItemsSource = DataTableConverter.Convert(dtSource);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgCancelScrap);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PART_SCRAP_LIST_NT", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        Util.GridSetData(dgCancelScrap, bizResult, FrameOperation, true);
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

        private void PartScrap()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                DataTable inLotTable = inDataSet.Tables.Add("IN_LOT");

                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("REG_YN", typeof(string));
                
                DataRow dtRow = null;
                dtRow = inTable.NewRow();

                dtRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dtRow["IFMODE"] = IFMODE.IFMODE_OFF;
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["NOTE"] = "";

                inTable.Rows.Add(dtRow);
                dtRow = null;

                for (int i = 0; i < dgScrap.Rows.Count; i++)
                {
                    dtRow = inLotTable.NewRow();

                    dtRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgScrap.Rows[i].DataItem, "LOTID"));
                    dtRow["REG_YN"] = "Y";

                    inLotTable.Rows.Add(dtRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DFCT_LANE_NT", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        
                        Util.MessageInfo("SFU1275");

                        Util.gridClear(dgScrap);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
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

        private void PartScrapCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                DataTable inLotTable = inDataSet.Tables.Add("IN_LOT");

                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("REG_YN", typeof(string));

                DataRow dtRow = null;
                dtRow = inTable.NewRow();

                dtRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dtRow["IFMODE"] = IFMODE.IFMODE_OFF;
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["NOTE"] = "";

                inTable.Rows.Add(dtRow);
                dtRow = null;

                for (int i = 0; i < dgCancelScrap.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgCancelScrap, "CHK", i)) continue;

                    dtRow = inLotTable.NewRow();

                    dtRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCancelScrap.Rows[i].DataItem, "LOTID"));
                    dtRow["REG_YN"] = "N";

                    inLotTable.Rows.Add(dtRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DFCT_LANE_NT", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
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
            listAuth.Add(btnScrap);
            listAuth.Add(btnCancelScrap);

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

        #endregion

        #region [Validation]
        private bool CanScrap()
        {
            bool bRet = false;
            
            if (dgScrap.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1636");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanCancelScrap()
        {
            bool bRet = false;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgCancelScrap, "CHK");
            if (iRow < 0)
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
        
    }
}
