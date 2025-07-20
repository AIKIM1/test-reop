/*************************************************************************************
 Created Date : 2023.05.31
      Creator : 
   Decription : Aging Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.31  DEVELOPER : 강동희

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_012_AGING_TRAY_LIST : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        private string _sAGING_TYPE = string.Empty;
        private string _sSCLINE = string.Empty;
        private string _sROW = string.Empty;

        public FCS002_012_AGING_TRAY_LIST()
        {
            InitializeComponent();
        }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;

            _sAGING_TYPE = tmps[0] as string;
            _sSCLINE = tmps[1] as string;
            _sROW = tmps[2] as string;

            InitCombo();

            cboAgingType.SelectedValue = _sAGING_TYPE;
            cboSCLine.SelectedValue = _sSCLINE;
            cboRow.SelectedValue = _sROW;
            cboOrder.SelectedValue = "EQPTID"; //EQPTID

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter = { "FORM_AGING_TYPE_CODE", string.Empty };
            _combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SYSTEM_AREA_COMMON_CODE", sFilter: sFilter);

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox nCbo = new C1ComboBox();
            C1ComboBox[] cboRouteParent = { cboLine, cboModel, nCbo, nCbo };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter1 = { "FORM_COMBO_STATE_COND" };
            _combo.SetCombo(cboStatus, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter3 = { "COMBO_PROC_INFO_BY_DATE_ORDER_CONDITION", "PROD_LOTID,EQPTID,WRK_STRT_DTTM" }; //E08
            _combo.SetCombo(cboOrder, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "COMMCODE", sFilter: sFilter3);
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SYSTEM_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);
                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void cboAgingType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            GetCommonCode();

            cboSCLine.Text = string.Empty;
            string[] sFilter = { EQPT_GR_TYPE_CODE, LANE_ID };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "SCLINE", sFilter: sFilter); //20210331 S/C 호기 필수 값으로 변경
        }

        private void cboSCLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            cboRow.Text = string.Empty;
            object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, Util.GetCondition(cboSCLine) };
            _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_ROW", objParent: objParent);
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayList == null || dgTrayList.CurrentRow == null || !dgTrayList.Columns.Contains("CSTID"))
                {
                    return;
                }

                if (dgTrayList.CurrentRow != null && (dgTrayList.CurrentColumn.Name.Equals("CSTID")))
                {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG"))))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("P"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }

                    if (e.Cell.Column.Name.ToString() == "CSTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("EQP_ROW_LOC", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("WIP_STATUS", typeof(string));
                dtRqst.Columns.Add("SORT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANE_ID"] = LANE_ID;
                dr["EQPT_GR_TYPE_CODE"] = EQPT_GR_TYPE_CODE;
                dr["EQPTID"] = cboSCLine.SelectedValue;
                dr["EQP_ROW_LOC"] = Util.GetCondition(cboRow, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text))
                {
                    dr["PROD_LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);
                }
                dr["WIP_STATUS"] = Util.GetCondition(cboStatus, bAllNull: true);
                dr["SORT"] = Util.GetCondition(cboOrder, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_TRAY_LIST_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTrayList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
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
    }
}
