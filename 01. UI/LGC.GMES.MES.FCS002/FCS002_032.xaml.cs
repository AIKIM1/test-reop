/*************************************************************************************
 Created Date : 2020.10.15
      Creator : Dooly
   Decription : Lane 별 출고가능 수량설정
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  DEVELOPER : Initial Created.
  2022.05.18  이정미 : NB1동 전용으로 변경되어 전체적인 수정 
  2022.07.02  이정미 : 저장 이벤트 수정 - 최대수량 변경 가능하도록 수정
                       저장 이벤트 오류 수정 - 모든 데이터가 업데이트되는 오류 수정 
  2022.08.19  조영대 : 행번호 추가, 블럭설정
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_032 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_032()
        {
            InitializeComponent();

        }

        #endregion Declaration & Constructor 


        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            //동
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
            C1ComboBox[] cboPlantChild = { cboEquipmentSegment };
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "PLANT", cbChild: cboPlantChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            ComCombo.SetCombo(cboEquipmentSegment, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE_SHOPID", cbParent: cboLineParent);


        }
        #endregion Initialize

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgShipCutLane_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    txtSelLane.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_ID"));
                    txtFromOp.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CURR_OP"));
                    txtToOp.Text = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NEXT_OP"));

                    GetDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LANE_ID")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "CURR_PROC_GR_CODE")),
                                 Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "NEXT_PROC_GR_CODE")));
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgShipCutLane.GetRowCount() == 0)
            {

                Util.Alert("SFU3552");  //저장 할 DATA가 없습니다.
                return;
            }

            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    /* DataTable RQSTDT = new DataTable();
                     RQSTDT.TableName = "RQSTDT";
                     RQSTDT.Columns.Add("PORT_ID", typeof(String));
                     RQSTDT.Columns.Add("MAX_TRF_QTY", typeof(String));
                     RQSTDT.Columns.Add("USERID", typeof(String));

                     DataTable dt = DataTableConverter.Convert(dgShipCutLane.ItemsSource);

                     for (int i = 0; i < dt.Rows.Count; i++)
                     {
                         DataRow dr = RQSTDT.NewRow();
                         dr["PORT_ID"] = dt.Rows[i]["PORT_ID"].ToString();
                         dr["MAX_TRF_QTY"] = dt.Rows[i]["MAX_TRF_QTY"].ToString();
                         dr["USERID"] = LoginInfo.USERID;

                         RQSTDT.Rows.Add(dr);
                     }*/

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("MAX_BUF_QTY", typeof(String));
                    RQSTDT.Columns.Add("USERID", typeof(String));
                    RQSTDT.Columns.Add("LANE_ID", typeof(String));
                    RQSTDT.Columns.Add("CURR_PROC_GR_CODE", typeof(String));
                    RQSTDT.Columns.Add("NEXT_PROC_GR_CODE", typeof(String));

                    DataTable dt = DataTableConverter.Convert(dgShipCutLane.ItemsSource);

                    for (int i = 0; i < dgShipCutLane.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "FLAG")).Equals("Y"))
                        {  // string[] sTagList = dgShipCutLane.GetCell(i, 0).ToString().Split('_');
                            DataRow dr = RQSTDT.NewRow();
                            dr["MAX_BUF_QTY"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "MAX_BUF_QTY")); //dgShipCutLane.GetCell(i, 4).ToString();
                                                                                                                                      //Convert.ToUInt16(dgShipCutLane.GetCell(i, 1).Text);
                            dr["USERID"] = LoginInfo.USERID;
                            dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "LANE_ID"));
                            dr["CURR_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "CURR_PROC_GR_CODE"));
                            //agList[1]; //NEXT_OP
                            dr["NEXT_PROC_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgShipCutLane.Rows[i].DataItem, "NEXT_PROC_GR_CODE"));//sTagList[2];
                            RQSTDT.Rows.Add(dr);
                        }
                    }

                    ShowLoadingIndicator();
                    //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_UPD_LANE_AGING_OUT_CNT_NEW", "RQSTDT", "RSLTDT", RQSTDT);
                    new ClientProxy().ExecuteService("DA_UPD_LANE_AGING_OUT_CNT_NEW", "RQSTDT", "RSLTDT", RQSTDT, (results, ex) =>
                    {
                        if (ex != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                            return;
                        }

                        Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                        GetList();

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                    HiddenLoadingIndicator();
                }
            });
        }

        private void dgLaneTryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgLaneTryList.ItemsSource == null)
                    return;

                if (sender == null)
                    return;

                if (dgLaneTryList.CurrentRow != null && dgLaneTryList.CurrentColumn.Name.Equals("CSTID"))
                {
                    //Tray 조회
                    object[] parameters = new object[6];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgLaneTryList.CurrentRow.DataItem, "CSTID"));   //TRAYID
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(dgLaneTryList.CurrentRow.DataItem, "LOTID"));   //TRAYNO

                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgLaneTryList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        private void dgShipCutLane_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CURR_CNT"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        private void dgLaneTryList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        private void dgShipCutLane_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgLaneTryList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null) return;

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgLaneTryList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion Event


        #region Method
        private void Clear()
        {
            Util.gridClear(dgShipCutLane);
            Util.gridClear(dgLaneTryList);

            txtSelLane.Text = string.Empty;
            txtFromOp.Text = string.Empty;
            txtToOp.Text = string.Empty;
        }

        private void GetList()
        {
            try
            {
                Clear();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("COND", typeof(string));

                DataRow dr = dtRqst.NewRow();

                DataTable dtRslt = new DataTable();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COND"] = Util.ConvertEmptyToNull((string)cboEquipmentSegment.SelectedValue);

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LANE_OUT_ABLE_CNT_MB", "INDATA", "OUTDATA", dtRqst);
                //dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LANE_OUT_ABLE_CNT", "INDATA", "OUTDATA", dtRqst);

                dtRslt.Columns.Add("FLAG", typeof(string));

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["FLAG"] = "N";
                }

                Util.GridSetData(dgShipCutLane, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "LANE_ID", "CURR_OP", "NEXT_OP" };
                _Util.SetDataGridMergeExtensionCol(dgShipCutLane, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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

        private void GetDetail(string sLineID, string sBF_PROC, string sCR_PROC)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CURR_PROCID", typeof(string));
                dtRqst.Columns.Add("NEXT_PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = sLineID;
                dr["CURR_PROCID"] = sBF_PROC;
                dr["NEXT_PROCID"] = sCR_PROC;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LANE_BUFFER_TRAY_LIST_MB", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLaneTryList, dtRslt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
        #endregion Method

        
    }
}
