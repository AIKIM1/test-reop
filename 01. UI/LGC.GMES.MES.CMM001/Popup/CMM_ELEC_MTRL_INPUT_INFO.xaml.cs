/*************************************************************************************
 Created Date : 2021.08.09
         Creator : Jeong
      Decription : RollMap Pilot3동 자재사용현황 메뉴 추가
--------------------------------------------------------------------------------------
 [Change History]
  2021.08.09 정종원 : 최초생성
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.ControlsLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_MTRL_INPUT_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MTRL_INPUT_INFO : C1Window, IWorkArea
    {
        #region Declare Variable
        private string _EQSGID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;

        public CMM_ELEC_MTRL_INPUT_INFO()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Init Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps == null)
                    return;

                _EQSGID = Util.NVC(tmps[0]);
                _EQPTID = Util.NVC(tmps[1]);
                _PROCID = Util.NVC(tmps[2]);
                _LOTID = Util.NVC(tmps[3]);

                Init(_LOTID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Init(string sLotID)
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);

            GetOutLotList(sLotID);
        }
        #endregion
        #region Control Event
        private void dgLotListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgLotList.SelectedIndex = idx;

                    SetMaterialInput(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgInputMtrlHist_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            try
            {
                if (dgInputMtrl.CurrentRow == null || dgInputMtrl.CurrentRow.Index < 0 || dgInputMtrl.SelectedIndex < 0)
                    return;

                if (dgInputMtrl.GetRowCount() > 0)
                    SetMaterialInputHist(Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[dgInputMtrl.SelectedIndex].DataItem, "INPUT_LOTID"))
                        , Util.NVC(DataTableConverter.GetValue(dgInputMtrl.Rows[dgInputMtrl.SelectedIndex].DataItem, "EQPT_MOUNT_PSTN_ID")));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetOutLotList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #region User Method
        private void GetOutLotList(string sLotID = "")
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["EQPTID"] = _EQPTID;
                Indata["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                Indata["TO_DATE"] = Util.GetCondition(dtpDateTo);
                Indata["AREATYPE"] = "E";

                dtRqst.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_LIST", "INDATA", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgLotList, bizResult, FrameOperation, true);

                        if (!string.IsNullOrWhiteSpace(sLotID))
                        {
                            for (int i = 0; i < dgLotList.GetRowCount(); i++)
                            {
                                if (string.Equals(sLotID, DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID")))
                                {
                                    DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "CHK", true);
                                    SetMaterialInput(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "LOTID").GetString());
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetMaterialInput(string sLotID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgInputMtrl);                
                Util.gridClear(dgInputMtrlHist);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = sLotID;
                
                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_BY_LOTID_LOT_TYPE_CODE", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    List<string> inner = new List<string>();
                    inner.Add("'START'");
                    inner.Add("'END'");
                    inner.Add("'CONSUME'");

                    DataRow[] rows = dtMain.Select("INPUT_LOT_STAT_CODE IN (" + string.Join(",", inner.ToArray()) + ")",
                       "INPUT_LOT_TYPE_CODE, INPUT_LOTID ASC");

                    if (rows != null && rows.Length > 0)
                        Util.GridSetData(dgInputMtrl, rows.CopyToDataTable(), FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetMaterialInputHist(string sInputLotID, string sEqptPstnID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgInputMtrlHist);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("INPUT_LOTID", typeof(string));
                IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["INPUT_LOTID"] = sInputLotID;
                newRow["EQPT_MOUNT_PSTN_ID"] = sEqptPstnID;

                IndataTable.Rows.Add(newRow);

                DataTable dtHistory = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_HIST_RM", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgInputMtrlHist, dtHistory, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
