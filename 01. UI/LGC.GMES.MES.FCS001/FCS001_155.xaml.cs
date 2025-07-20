/*************************************************************************************
 Created Date : 2023.4.06
      Creator : 
   Decription : CT기 작업 실적
--------------------------------------------------------------------------------------
 [Change History]
  2023.04.06  DEVELOPER : Initial Created.
  2023.07.13  배준호    : 투입,불량 수량 합계 표시
  2023.10.31  임근영    : 불량수량 팝업창 추가,특별 관리 TRAY 색인 추가,TRAYID 더블클릭시 TRAY 정보조회 화면으로 이동. 
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
    /// FCS001_154.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_155 : UserControl, IWorkArea
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

        public FCS001_155()
        {
            InitializeComponent();
        }
        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            SetWorkResetTime();
            InitControl();

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
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild, sFilter: sFilter);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);
        }
        #endregion
        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
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
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["PKG_LOT_ID"] = string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : txtPkgLotID.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_BAS_SEL_CT_DATA", "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    SetBottomRow(result);

                    Util.GridSetData(dg_Info, result, FrameOperation, true);

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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
        /// <summary>
        /// 하단 Summary Row
        /// </summary>
        /// <param name="dt"></param>
        private void SetBottomRow(DataTable dt)
        {

            DataRow dr = dt.NewRow();

            //모델명 - "합계"
            dr["INSP_MTHD_NAME"] = ObjectDic.Instance.GetObjectName("합계");

            int colIdx = dt.Columns.IndexOf("INPUT_QTY");
            for (int i = colIdx; i < colIdx + 2; i++)
            {
                    int sum = 0;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        sum += Util.NVC_Int(dt.Rows[j][i]);
                    }
                    dr[i] = sum;
            }

            //Total 양품율
            decimal input = Convert.ToDecimal(dr["INPUT_QTY"].ToString());
            decimal dfct = Convert.ToDecimal(dr["DFCT_QTY"].ToString());

            if (input == 0) dr["INPUT_QTY"] = 0;
            else dr["INPUT_QTY"] = input;

            if (dfct == 0) dr["DFCT_QTY"] = 0;
            else dr["DFCT_QTY"] = dfct;


            dt.Rows.Add(dr);
        }
        #endregion

        private void dg_Info_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Index == dg.Rows.Count - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                }

                // LOTID 색 부분 추가 
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Cursor = Cursors.Arrow;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    //특별관리 TRAY 일시 
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("Y"))   //임시로.  DA 수정 필요. 
                    {
                        //e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    //END 

                    if (e.Cell.Column.Name.Equals("LOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                }
                //END

            }));
        }

        private void dg_Info_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e.Row.HeaderPresenter == null)
                {
                    return;
                }

                e.Row.HeaderPresenter.Content = null;

                TextBlock tb = new TextBlock();

                int num = e.Row.Index + 1 - dg_Info.TopRows.Count;
                if (num > 0)
                {
                    tb.Text = num.ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
       
        private void dg_Info_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg_Info.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    
                    if (!cell.Column.Name.Equals("DFCT_QTY") && !cell.Column.Name.Equals("LOTID"))
                    {
                        return; 
                    }
                    
                    if (cell.Column.Name.Equals("LOTID"))
                    {
                        string sTrayId = Util.NVC(DataTableConverter.GetValue(dg_Info.Rows[cell.Row.Index].DataItem, "CSTID")).ToString();
                        string sTrayNo = Util.NVC(DataTableConverter.GetValue(dg_Info.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();

                        object[] parameters = new object[6];
                        parameters[0] = sTrayId;
                        parameters[1] = sTrayNo;
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = "Y";
                        this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
                    }

                    if (cell.Column.Name.Equals("DFCT_QTY"))  // 불량 수량 클릭시 CELLID 조회.
                    {
                        if (string.IsNullOrEmpty(Util.NVC(cell.Text)) || Util.NVC(cell.Text).Equals("0")) return;

                        FCS001_155_DFCT_CELL_LIST wndPopup = new FCS001_155_DFCT_CELL_LIST();   
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            int row = cell.Row.Index;
                            
                            object[] Parameters = new object[1];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dg_Info.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();


                            C1WindowExtension.SetParameters(wndPopup, Parameters);
                            wndPopup.Closed += new EventHandler(wndPopup_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));


                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        
        private void dgProdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);
            if (cell == null)
            {
                if (datagrid.CurrentCell == null || datagrid.CurrentRow == null || datagrid.CurrentColumn == null) return;
                if (datagrid.CurrentRow.Type != DataGridRowType.Bottom) return;
                cell = datagrid.CurrentCell;
            }

            else
            {
                if (cell.Row.Type != DataGridRowType.Item) return;
            }

            if (string.IsNullOrEmpty(Util.NVC(cell.Text)) || Util.NVC(cell.Text).Equals("0")) return;

            FCS001_155_DFCT_CELL_LIST wndPopup = new FCS001_155_DFCT_CELL_LIST();  //임시 
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                int row = cell.Row.Index;
                object[] Parameters = new object[3];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dg_Info.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();
                Parameters[1] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00"); //FROM_DATE
                Parameters[2] = !string.IsNullOrWhiteSpace(txtPkgLotID.Text) ? null : dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00"); //TO_DATE

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_155_DFCT_CELL_LIST window = sender as FCS001_155_DFCT_CELL_LIST;
        }




    }
}
