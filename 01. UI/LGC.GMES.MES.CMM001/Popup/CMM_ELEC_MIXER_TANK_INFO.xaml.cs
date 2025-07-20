/*************************************************************************************
 Created Date : 2018.06.01
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 코터 공정진척 Slurry 장착 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.01  INS 김동일K : Initial Created.
  2022.02.23      정재홍  : [C20211216-000385] - 코터 장착된 Slurry Lot 양품량과 코터 생산실적 총량을 비교하여 팝업 실행 여부
**************************************************************************************/

using System;
using System.Linq;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_MIXER_TANK_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_MIXER_TANK_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRDT_CLSS_CODE = string.Empty;
        private string _EQSGID = string.Empty;
        private string _WOID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _SLURRYID = string.Empty;
        private string _MTRLID = string.Empty;

        private int _POSITION = -1;

        private bool isSingleCoater = false;
        private bool isAllConfirm = false;
        private bool isSlurryTerm = false;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        public string _ReturnCLSSCODE
        {
            get { return _PRDT_CLSS_CODE; }
        }
        public string _ReturnPRODID
        {
            get { return _PRODID; }
        }
        public int _ReturnPosition
        {
            get { return _POSITION; }
        }
        public bool _IsAllConfirm
        {
            get { return isAllConfirm; }
        }

        public bool _IsSlurryTerm
        {
            get { return isSlurryTerm; }
        }

        public string _ReturnMTRLID
        {
            get { return _MTRLID; }
        }
        #endregion

        #region Initialize
        public CMM_ELEC_MIXER_TANK_INFO()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps == null)
                {
                    return;
                }

                _EQSGID = Util.NVC(tmps[0]);
                _EQPTID = Util.NVC(tmps[1]);
                _PROCID = Util.NVC(tmps[2]);
                _LOTID = Util.NVC(tmps[3]);
                //CSR : [C20211216-000385]
                _PRODID = Util.NVC(tmps[5]);
                _MTRLID = Util.NVC(tmps[6]);

                InitCombo();

                //CSR : [C20211216-000385]
                if (!IsCommoncodeUse("COATER_SLURRY_POPUP"))
                {
                    tabSlurryInputComp.Visibility = Visibility.Visible;
                    tabSlurryTankMoveInfo.Visibility = Visibility.Visible;
                    tabSlurryInputInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tabSlurryInputComp.Visibility = Visibility.Collapsed;
                    tabSlurryTankMoveInfo.Visibility = Visibility.Collapsed;
                    tabSlurryInputInfo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    //DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgLotInfo.SelectedIndex = idx;

                    GetDetailInfo(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "EQPT_MOUNT_PSTN_ID")),
                                  Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "INPUT_LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboState_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                if (e.OldValue < 0)
                    return;

                Util.gridClear(dgLotInfo);
                GetMixerSlurryInfo();
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
                GetMixerSlurryInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void btnDetlSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetDetailInfoByPeriod();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
        private void GetMixerSlurryInfo()
        {
            try
            {
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 30)
                //{
                //    // 기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "30");
                //    return;
                //}

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DATEFROM", typeof(string));       // 일자
                IndataTable.Columns.Add("DATETO", typeof(string));
                IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                //newRow["DATEFROM"] = dtpDateFrom.SelectedDateTime.ToShortDateString().ToString();
                //newRow["DATETO"] = dtpDateTo.SelectedDateTime.ToShortDateString().ToString();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboEqptPstnID.SelectedValue);

                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_INPUT_LIST", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgLotInfo, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDetailInfo(string sPstnID, string sInputLot)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));       // 일자
                IndataTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["INPUT_LOTID"] = sInputLot;

                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_INPUT_HIST", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgLotInfoDetl, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDetailInfoByPeriod()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string)); 
                IndataTable.Columns.Add("FRDT", typeof(string));
                IndataTable.Columns.Add("TODT", typeof(string)); 

                DataRow newRow = IndataTable.NewRow();
                newRow["EQPTID"] = _EQPTID;
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(cboEqptPstnID_Hist.SelectedValue);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FRDT"] = dtpDateFrom.SelectedDateTime.ToShortDateString().ToString();
                newRow["TODT"] = dtpDateTo.SelectedDateTime.ToShortDateString().ToString();

                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_INPUT_HIST_DETAIL", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgLotInfoDetl_Priod, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 자재 투입위치 코드
            String[] sFilter1 = { _EQPTID, "PROD" };
            String[] sFilter2 = { _EQPTID, null }; // 자재,제품 전체
            String[] sFilter3 = { "BICELL_TYPE_FD" };

            _combo.SetCombo(cboEqptPstnID, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
            _combo.SetCombo(cboEqptPstnID_Hist, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

        }
        #endregion

        private void btnSearch_Hst_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetOutLotList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
                    //DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    dgLotList.SelectedIndex = idx;

                    GetInputHistUI(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID")));

                    GetInputHistEQ(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[idx].DataItem, "LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutLotList()
        {
            try
            {
                if ((dtpDateTo_Hst.SelectedDateTime - dtpDateFrom_Hst.SelectedDateTime).TotalDays > 31)
                {
                    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["EQPTID"] = _EQPTID;
                Indata["FROM_DATE"] = Util.GetCondition(dtpDateFrom_Hst);
                Indata["TO_DATE"] = Util.GetCondition(dtpDateTo_Hst);
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

        private void GetInputHistUI(string sLotID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["LOTID"] = sLotID;
                newRow["INPUT_LOT_TYPE_CODE"] = "PROD";
                newRow["LANGID"] = LoginInfo.LANGID;

                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_BY_LOTID_LOT_TYPE_CODE", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgInputHistUI, dtMain, FrameOperation, true);
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

        private void GetInputHistEQ(string sLotID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("OUTPUT_LOTID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = IndataTable.NewRow();
                newRow["OUTPUT_LOTID"] = sLotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                IndataTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_WIP_INPUT_MTRL_HIST_TMP", "INDATA", "RSLTDT", IndataTable);
                Util.GridSetData(dgInputHistEQ, dtMain, FrameOperation, true);
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

        private void dgSlurryLot_Checked(object sender, RoutedEventArgs e)
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

                    dgSlurrySumInfo.SelectedIndex = idx;

                    string sLOTID = Util.NVC(DataTableConverter.GetValue(dgSlurrySumInfo.Rows[idx].DataItem, "LOTID"));

                    GetSlurryInputSumInfoDetail(_EQPTID, sLOTID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool IsCommoncodeUse(string sCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnSearchSlurry_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Util.gridClear(dgSlurrySumInfo);
                Util.gridClear(dgSlurryLotDetl);

                GetSlurryInputSumInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSlurryInputSumInfo()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MTRLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["MTRLID"] = _MTRLID;
                Indata["PRODID"] = _PRODID;
                Indata["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_INPUT_SUM", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSlurrySumInfo, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSlurryInputSumInfoDetail(string sEQPTID, string sLOTID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("INPUT_LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["INPUT_LOTID"] = (sLOTID == null) ? null : Util.NVC(sLOTID);
                Indata["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_INPUT_SUM_DETAIL", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSlurryLotDetl, dtMain, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
