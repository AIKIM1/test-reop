/*************************************************************************************
 Created Date : 2023.05.01
      Creator : 
   Decription : 예약 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.01  DEVELOPER : Initial Created.
  2023.05.15  이지은      폴란드 GMES 역전개 프로젝트
  2023.08.14  이지은      경과 시간에 따른 컬럼 색 변경 범례 추가, 정렬조건 변경
  2024.02.02  권순범      예약명칭(MultiSelectionBox) 조건 추가
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
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_157 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;
        private DataTable dtColor = new DataTable();
        Hashtable hash_loss_color = new Hashtable();

        Util _Util = new Util();

        public FCS001_157()
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            SetAreaComCode(cboRsvType);

            SetRcvName(cboRsvName); //MultiSelectionBox

            GeColorLegend();

        }

        private void SetAreaComCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORMLGS_RSV_TYPE";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetRcvName(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORMLGS_RSV_NAME";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = drRslt["CBO_NAME"].ToString() + " (" + drRslt["CBO_CODE"].ToString() + ")";
                }
            }
            return dt;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                _timer.Tick += new EventHandler(timer_Tick);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 10)
            {
                btnSearch_Click(null, null);
                sec = 0;
            }
        }

        private void chkTimer_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void chkTimer_Unchecked(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

        private void dgRsvStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    int PreMinDiff = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRE_MIN_DIFF"));
                    int MinSysDiff = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MIN_SYS_DIFF"));

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "EXEC_DTTM" && PreMinDiff >= 30)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow); 
                    }
                    if (e.Cell.Column.Name.ToString() == "EXEC_DTTM" && MinSysDiff >= 30)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red); 
                    }
                    if (e.Cell.Column.Name.ToString() == "EXEC_PGM_NAME" && MinSysDiff >= 30)
                    {
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgRsvStatus_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgRsvStatus.CurrentRow.DataItem, "EXEC_PGM_NAME_ORG")))) return;

                if (datagrid.CurrentColumn.Name == "EXEC_PGM_NAME" )
                {
                    FCS001_158 wndTRAY = new FCS001_158();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgRsvStatus.CurrentRow.DataItem, "EXEC_PGM_NAME_ORG")); // PGM_NAME
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgRsvStatus.CurrentRow.DataItem, "EXEC_TYPE")); //REV_TYPE
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgRsvStatus.CurrentRow.DataItem, "EXEC_DTTM")); // EXEC_DTTM

                    this.FrameOperation.OpenMenu("SFU010181013", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboRsvName_SelectionChanged(object sender, EventArgs e)
        {
            if (cboRsvName.SelectedItems.Count == 0)
            {
                cboRsvName.CheckAll();
            }
        }

        private void cboRsvName_DropDownClosed(object sender)
        {
            if (sender == null) return;
        }

        #endregion

        #region Method
        private void GetList()
        {
           try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EXEC_TYPE", typeof(string));
                inDataTable.Columns.Add("EXEC_PGM_NAME", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EXEC_TYPE"] = Util.GetCondition(cboRsvType, bAllNull: true);
                newRow["EXEC_PGM_NAME"] = cboRsvName.SelectedItemsToString;
     
                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_FORMLGS_RSV_EXEC_HIST_UI", "INDATA", "OUTDATA", inDataTable);

                Util.GridSetData(dgRsvStatus, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 범례
        /// </summary>
        private void GeColorLegend()
        {
            try
            {
                dtColor = null;

                C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("- LEGEND -") };
                cboColorLegend.Items.Add(cbItemTiTle);

                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                //DataRow dr = dtRqst.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["CMCDTYPE"] = "FORM_AGINGSTATUS";

                //dtRqst.Rows.Add(dr);

                //dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqst);

                //hash_loss_color = DataTableConverter.ToHash(dtColor);

                //foreach (DataRow drRslt in dtColor.Rows)
                //{
                //    C1ComboBoxItem cbItem = new C1ComboBoxItem();
                //    cbItem.Content = drRslt["CMCDNAME"];
                //    cbItem.Foreground = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE2"].ToString()));
                //    cbItem.Background = ColorToBrush(System.Drawing.Color.FromName(drRslt["ATTRIBUTE1"].ToString()));
                //    cboColorLegend.Items.Add(cbItem);
                //}

                C1ComboBoxItem cbItem = new C1ComboBoxItem();

                cbItem.Content = ObjectDic.Instance.GetObjectName("LEGEND_RSV_1"); // "마지막 성공 시간 대비 최신 성공 시간이 30분 이상 차이나는 경우";
                cbItem.Foreground = ColorToBrush(System.Drawing.Color.FromName("Black"));
                cbItem.Background = ColorToBrush(System.Drawing.Color.FromName("yellow"));
                cboColorLegend.Items.Add(cbItem);

                C1ComboBoxItem cbItem2 = new C1ComboBoxItem();

                cbItem2.Content = ObjectDic.Instance.GetObjectName("LEGEND_RSV_2"); //"최신 예약 시간이 현재 시간 대비 30분 이상 차이나는 경우";
                cbItem2.Foreground = ColorToBrush(System.Drawing.Color.FromName("Black"));
                cbItem2.Background = ColorToBrush(System.Drawing.Color.FromName("Red"));
                cboColorLegend.Items.Add(cbItem2);

                cboColorLegend.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Brush ColorToBrush(System.Drawing.Color C)
        {
            return new SolidColorBrush(Color.FromArgb(C.A, C.R, C.G, C.B));
        }

        #endregion

        
    }
}
