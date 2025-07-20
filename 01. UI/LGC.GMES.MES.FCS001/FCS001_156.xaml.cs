/*************************************************************************************
 Created Date : 2023.4.06
      Creator : 
   Decription : CT기 작업 실적
--------------------------------------------------------------------------------------
 [Change History]
  2023.04.06  DEVELOPER : Initial Created.
  
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_156.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_156 : UserControl, IWorkArea
    {

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        //기본 설정시간 parameter 추가
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        public FCS001_156()
        {
            InitializeComponent();

        }
        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            SetWorkResetTime();
            InitControl();
            InitGridCombo();
            Loaded -= UserControl_Loaded;
        }
        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();

            // 시간 제거
            dtpFromDate.SelectedDateTime = new DateTime(dtpFromDate.SelectedDateTime.Year, dtpFromDate.SelectedDateTime.Month, dtpFromDate.SelectedDateTime.Day);
            dtpToDate.SelectedDateTime = new DateTime(dtpToDate.SelectedDateTime.Year, dtpToDate.SelectedDateTime.Month, dtpToDate.SelectedDateTime.Day);

        }
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild,sFilter: sFilter);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "FORM_CT_INSP_MTHD_CODE" }, CommonCombo.ComboStatus.NONE, dgCtInsp.Columns["RECHK_FLAG"], "CBO_CODE", "CBO_NAME");
            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_COMMCODE_CBO", new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "FORM_CT_INSP_MTHD_CODE" }, CommonCombo.ComboStatus.NONE, dgCtInsp.Columns["RECHK_FLAG"], "CBO_CODE", "CBO_NAME");
        }
        private void InitGridCombo()
        {
            DataTable dt = Get_Grid_Combo_CommonCode("YORN");
            (dgCtInsp.Columns["RECHK_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);

            dt = Get_Grid_Combo_CommonCode("FORM_CT_INSP_MTHD_CODE");
            (dgCtInsp.Columns["RECHK_TYPE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }
        private DataTable Get_Grid_Combo_CommonCode(string cmcdType)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = cmcdType;
            IndataTable.Rows.Add(Indata);

            DataTable dt_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", IndataTable);
            return dt_Result;
        }

        private DataTable Get_GridCombo_Data(string sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
            return dtResult;
        }
        #endregion
        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            try
            {
                int iListIndex = Util.gridFindDataRow(ref dgCtInsp, "CHK", "True", false);
                if(iListIndex < 0) { Util.MessageInfo("SFU1636"); return; }

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable RQSTDT = new DataTable("RQSTDT");
                        RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                        RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                        RQSTDT.Columns.Add("EQSGID", typeof(string));
                        RQSTDT.Columns.Add("RECHK_FLAG", typeof(string));
                        RQSTDT.Columns.Add("RECHK_TYPE", typeof(string));
                        RQSTDT.Columns.Add("UPDUSER", typeof(string));

                        for (int _iRow = 0; _iRow < dgCtInsp.Rows.Count; _iRow++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "CHK")).Equals("True"))
                            {
                                if (!fn_EmptyString_Chk(_iRow)) return;
                                DataRow Indata = RQSTDT.NewRow();
                                Indata["MDLLOT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "MDLLOT_ID"));
                                Indata["PKG_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "PKG_LOTID"));
                                Indata["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "EQSGID"));
                                Indata["RECHK_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "RECHK_FLAG"));
                                Indata["RECHK_TYPE"] = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "RECHK_TYPE"));
                                Indata["UPDUSER"] = LoginInfo.USERID;
                                RQSTDT.Rows.Add(Indata);
                            }
                        }

                        loadingIndicator.Visibility = Visibility.Visible;
                        new ClientProxy().ExecuteService("DA_BAS_UPD_CT_MDL", "RQSTDT", "", RQSTDT, (bizResult, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            Util.MessageInfo("SFU1518");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgCtInsp_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dg = sender as C1DataGrid;

            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "RECHK_TYPE")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[e.Cell.Row.Index].DataItem, "RECHK_FLAG")).ToString() == "Y")
                        {
                            e.Cell.Presenter.IsEnabled = true;
                        }
                        else
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                    }
                }
            }));
        }
        private void dgCtInsp_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgCtInsp.CurrentColumn.Name == "RECHK_FLAG")
            {
                C1.WPF.DataGrid.DataGridCell cell = dgCtInsp.GetCell(dgCtInsp.CurrentRow.Index, dgCtInsp.CurrentColumn.Index);
                if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[dgCtInsp.CurrentRow.Index].DataItem, "RECHK_FLAG")).ToString() == "Y")
                {
                    cell.Presenter.IsEnabled = true;
                }
                else
                {
                    cell.Presenter.IsEnabled = false;
                }
            }
        }


        private void dgCtInsp_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "RECHK_FLAG")
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[e.Cell.Row.Index].DataItem, "RECHK_FLAG")).ToString() == "Y")
                {
                    dgCtInsp.GetCell(e.Cell.Row.Index, dgCtInsp.Columns["RECHK_TYPE"].Index).Presenter.IsEnabled = true; ;

                    //e.Cell.Presenter.IsEnabled = true;
                }
                else
                {
                    dgCtInsp.GetCell(e.Cell.Row.Index, dgCtInsp.Columns["RECHK_TYPE"].Index).Presenter.IsEnabled = false; ;
                }
            }
        }

        #endregion
        #region Method
        private void GetList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("PKG_LOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["PKG_LOT_ID"] = string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : txtPkgLotID.Text;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_BAS_SEL_CT_MDL", "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if(result.Rows.Count != 0)
                    {
                        result.Columns.Add("CHK", typeof(bool));
                    }
                    Util.GridSetData(dgCtInsp, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private bool fn_EmptyString_Chk(int _iRow)
        {
            bool bReturn = true;
            string mdllot_ID = Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "MDLLOT_ID"));
            //if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "RECHK_FLAG")) == "N")
            //{
            //    //재확인 여부를 선택하십시오.
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(string.Format("[{0}]{1}", mdllot_ID, MessageDic.Instance.GetMessage("SFU9016")), "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    bReturn = false;
            //}
            if (Util.NVC(DataTableConverter.GetValue(dgCtInsp.Rows[_iRow].DataItem, "RECHK_TYPE")) == "")
            {
                //재확인 유형을 선택하십시오.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(string.Format("[{0}]{1}", mdllot_ID, MessageDic.Instance.GetMessage("SFU9017")), "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
            }
            return bReturn;
        }
        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }
        #endregion
    }
}
