/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 강동희
   Decription : Aging S/C 호기별 예정 수량
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  DEVELOPER : Initial Created.

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
    public partial class FCS002_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_007()
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
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboAgingTypeChild = { cboEqp };
            string[] sFilter = { "FORM_AGING_TYPE_CODE" };
            _combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "AREA_COMMON_CODE", sFilter: sFilter, cbChild: cboAgingTypeChild);

            C1ComboBox[] cboEqpParent = { cboAgingType };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "SCEQPID", cbParent: cboEqpParent);
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgEqpStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            dgEqpStatus.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Column.Name.Equals("CST_CNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                string s = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "AGING_ISS_SCHD_DTTM"));
                if (!string.IsNullOrEmpty(s))
                {
                    DateTime dt = Convert.ToDateTime(s);
                    DateTime today = DateTime.Today;
                    if (today > dt)
                    {
                        if (e.Cell.Column.Index >= 1)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
            }
          ));
        }

        private void dgEqpStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    FCS002_005_02 TrayList = new FCS002_005_02();
                    TrayList.FrameOperation = FrameOperation;

                    object[] Parameters = new object[19];
                    Parameters[0] = string.Empty; //sOPER
                    Parameters[1] = ""; //sOPER_NAME
                    Parameters[2] = ""; //sLINE_ID
                    Parameters[3] = ""; //sLINE_NAME
                    Parameters[4] = ""; //sROUTE_ID
                    Parameters[5] = ""; //sROUTE_NAME
                    Parameters[6] = ""; //sMODEL_ID
                    Parameters[7] = ""; //sMODEL_NAME
                    Parameters[8] = "S"; //sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_TRAY"); //sStatusName
                    Parameters[10] = ""; //sROUTE_TYPE_DG
                    Parameters[11] = ""; //sROUTE_TYPE_DG_NAME
                    Parameters[12] = ""; //sLotID
                    Parameters[13] = ""; //sSPECIAL_YN
                    Parameters[14] = DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "EQPTID").ToString(); //_sAgingEqpID
                    Parameters[15] = DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "EQPTNAME").ToString(); //_sAgingEqpName

                    string _sDate = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "AGING_ISS_SCHD_DTTM"));
                    if (!string.IsNullOrEmpty(_sDate))
                    {
                        Parameters[16] = _sDate; //_sOpPlanTime
                    }
                    else Parameters[16] = "";

                    Parameters[17] = "";
                    Parameters[18] = "";
                    this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpStatus_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }
        }

        private void dgEqpStatus_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));   // S70 : EQPT_GR_TYPE_CODE
                dtRqst.Columns.Add("S71", typeof(string));   // S71 : LANE_ID
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(Util.NVC(cboAgingType.SelectedValue)))
                {
                    string code = SetTag(Util.NVC(cboAgingType.SelectedValue));
                    dr["S70"] = code.Split('_')[0];
                    dr["S71"] = code.Split('_')[1];
                }
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_UNLOAD_TRAY_CNT_BY_SC_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgEqpStatus, dtRslt, this.FrameOperation,true);

                string[] sColumnName = new string[] { "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgEqpStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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

        private string SetTag(string _code)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
            dr["COM_CODE"] = cboAgingType.SelectedValue;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_TYPE_CODE_ATTR", "RQSTDT", "RSLTDT", dtRqst);

            return dtRslt.Rows[0]["ATTR1"] + "_" + dtRslt.Rows[0]["ATTR2"];
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
