/*************************************************************************************
 Created Date : 2020.11.17
      Creator : 
   Decription : 전용OCV 호기별 출고 예정 수량
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.17  DEVELOPER : CREATED


**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

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
    public partial class FCS002_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_008()
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

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            string[] sFilter = { "8", null, "M" };
            _combo.SetCombo(cboEqp, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "EQPID", sFilter: sFilter);

            string[] sFilter1 = { null, "8", null, "81", null };
            _combo.SetCombo(cboOpId, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP", sFilter: sFilter1);
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
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.GetCondition(cboEqp, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOpId, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_UNLOAD_TRAY_CNT_BY_OCV_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgEqpStatus, dtRslt, FrameOperation, true);
                string[] sColumnName = new string[] { "EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgEqpStatus, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
            
        }

        private void dgEqpStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (cell.Text != datagrid.CurrentColumn.Header.ToString() &&datagrid.CurrentRow!=null && datagrid.CurrentColumn.Name.Equals("TRAY_CNT"))
                {

                    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

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
                    Parameters[14] = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "EQPTID").ToString()); //_sAgingEqpID
                    Parameters[15] = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "EQPTNAME")); //_sAgingEqpName

                    string _sDate = Util.NVC(DataTableConverter.GetValue(dgEqpStatus.CurrentRow.DataItem, "AGING_ISS_SCHD_DTTM"));
                    if (!string.IsNullOrEmpty(_sDate))
                        Parameters[16] = _sDate; //_sOpPlanTime
                    else Parameters[16] = "";

                    Parameters[17] = "";
                    Parameters[18] = "";

                    this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("TRAY_CNT"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
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
    }
}
