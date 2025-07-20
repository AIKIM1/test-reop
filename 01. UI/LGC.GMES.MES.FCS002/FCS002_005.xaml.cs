/*************************************************************************************
 Created Date : 2023.01.16
      Creator : Dooly
   Decription : 재공정보현황(공정별)
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.16  DEVELOPER : Initial Created.
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
using System.Linq; //20220119_모델별 합계 Row 추가
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        public FCS002_005()
        {
            InitializeComponent();
            InitCombo();
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

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox nCbo = new C1ComboBox();
            C1ComboBox[] cboRouteParent = { cboLine, cboModel, nCbo, nCbo };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter1 = { "FORM_SPCL_FLAG_MCC" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가

            string[] sFilter2 = { "ORDER_TYPE_CODE" };
            _combo.SetCombo(cboSort, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: sFilter2);
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                if (sec >= 30)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0352"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            btnSearch.Focus();
                        }
                    });
                    return;
                }

                if (dgWipbyOper.CurrentRow != null && (dgWipbyOper.CurrentColumn.Name.Equals("WAITTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WAITCELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("WORKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WORKCELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDCELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER1TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER1CELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER2TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER2CELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER3TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER3CELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("TROUBLETRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TROUBLECELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("RECHECKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("RECHECKCELL") ||
                    dgWipbyOper.CurrentColumn.Name.Equals("TOTALTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALINPUTCELL")))
                {
                }
                else
                {
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dgWipbyOper.CurrentColumn.Name.ToString())).Equals("0")) return;
                string sOPER = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID"));
                string sOPER_NAME = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCNAME"));
                string sMAX_PROCID_PROC_GR_CODE = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID_PROC_GR_CODE"));
                string sLINE_ID = Util.GetCondition(cboLine);
                string sLINE_NAME = cboLine.Text;
                string sROUTE_ID = Util.GetCondition(cboRoute);
                string sROUTE_NAME = cboRoute.Text;
                string sMODEL_ID = Util.GetCondition(cboModel);
                string sMODEL_NAME = cboModel.Text;
                if (chkModel.IsChecked.Equals(true))
                {
                    sMODEL_ID = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MDLLOT_ID"));
                    sMODEL_NAME = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MODEL_NAME"));
                }

                string sStatus = null;
                string sStatusName = null;
                string sLotID = Util.GetCondition(txtLotId);
                //string sSpecial = Util.GetCondition(cboSpecial);
                string sSpecial = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "SPCL_TYPE_CODE"));
                //if (!string.IsNullOrEmpty(sSpecial)) //2025.05.29 특별관리 Flag 변경으로 주석처리
                //{
                //    sSpecial = sSpecial.Equals("N") ? "N" : "Y";
                //}
                string sSpecialName = cboSpecial.Text;
                string sRouteTypeDG = string.Empty;
                string sRouteTypeDGName = string.Empty;
                string sLotType = Util.GetCondition(cboLotType);
                string sLotTypeName = cboLotType.Text;

                if (dgWipbyOper.CurrentRow != null && (dgWipbyOper.CurrentColumn.Name.Equals("WORKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WORKCELL")))
                {
                    sStatus = "S";
                    sStatusName = ObjectDic.Instance.GetObjectName("WORK_TRAY");  //작업Tray

                    if (sOPER.Length > 1)
                    {
                        if (sMAX_PROCID_PROC_GR_CODE == "3"
                            || sMAX_PROCID_PROC_GR_CODE == "4"
                            || sMAX_PROCID_PROC_GR_CODE == "7"
                            || sMAX_PROCID_PROC_GR_CODE == "9")
                        {
                            Load_FCS002_005_01(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName, sLotType, sLotTypeName);
                            return;
                        }
                    }
                }
                else if (dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDCELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER1TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER1CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER2TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER2CELL")
                    || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER3TRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGOVER3CELL"))
                {
                    sStatus = "P";
                    sStatusName = ObjectDic.Instance.GetObjectName("AGING_END_WAIT"); //Aging 종료대기
                }
                else if (dgWipbyOper.CurrentColumn.Name.Equals("WAITTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WAITCELL"))
                {
                    sStatus = "E";
                    sStatusName = ObjectDic.Instance.GetObjectName("WAIT"); //대기
                }
                else if (dgWipbyOper.CurrentColumn.Name.Equals("TROUBLETRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TROUBLECELL"))
                {
                    sStatus = "T";
                    sStatusName = ObjectDic.Instance.GetObjectName("WORK_ERR"); //작업이상
                }
                else if (dgWipbyOper.CurrentColumn.Name.Equals("RECHECKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("RECHECKCELL"))
                {
                    sStatus = "R";
                    sStatusName = ObjectDic.Instance.GetObjectName("RECHECK"); //RECHECK
                }
                else if (dgWipbyOper.CurrentColumn.Name.Equals("TOTALTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALINPUTCELL") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALCURRCELL"))
                {
                    sStatus = "A";
                    sStatusName = ObjectDic.Instance.GetObjectName("TOTAL"); //토탈
                }

                Load_FCS002_005_02(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName, sLotType, sLotTypeName);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgWipbyOper_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (string.IsNullOrEmpty(e.Column.Name) == false)
            //    {
            //        if (e.Column.Name.Equals("WORKTRAY") || e.Column.Name.Equals("WORKCELL") || e.Column.Name.Equals("AGINGENDTRAY") || e.Column.Name.Equals("AGINGENDCELL") ||
            //            e.Column.Name.Equals("TROUBLETRAY") || e.Column.Name.Equals("TROUBLECELL") || e.Column.Name.Equals("WAITCELL") || e.Column.Name.Equals("WAITTRAY"))
            //        {
            //            e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
            //            e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
            //        }
            //    }
            //}));
        }

        private void dgWipbyOper_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                {
                    if (e.Cell.Column.Name.Equals("WORKTRAY") || e.Cell.Column.Name.Equals("WORKCELL") || e.Cell.Column.Name.Equals("AGINGENDTRAY") || e.Cell.Column.Name.Equals("AGINGENDCELL") ||
                        e.Cell.Column.Name.Equals("WAITCELL") || e.Cell.Column.Name.Equals("WAITTRAY") || e.Cell.Column.Name.Equals("RECHECKTRAY") || e.Cell.Column.Name.Equals("RECHECKCELL") ||
                        e.Cell.Column.Name.Equals("TOTALTRAY") || e.Cell.Column.Name.Equals("TOTALINPUTCELL"))
                    {
                        if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                    if (cboSpecial.SelectedValue.ToString() == "Y")
                    {
                        if (e.Cell.Column.Name.Equals("WORKTRAY") || e.Cell.Column.Name.Equals("WORKCELL") || e.Cell.Column.Name.Equals("AGINGENDTRAY") || e.Cell.Column.Name.Equals("AGINGENDCELL") ||
                            e.Cell.Column.Name.Equals("WAITCELL") || e.Cell.Column.Name.Equals("WAITTRAY") || e.Cell.Column.Name.Equals("TOTALTRAY") || e.Cell.Column.Name.Equals("TOTALINPUTCELL"))
                        {
                            if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE"))) &&
                        !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE")).ToString().Equals("N"))
                    {
                        if (!e.Cell.Column.Name.Equals("MDLLOT_ID") && !e.Cell.Column.Name.Equals("MODEL_NAME") && !e.Cell.Column.Name.Equals("PROCNAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (e.Cell.Column.Name.Equals("TROUBLETRAY") || e.Cell.Column.Name.Equals("TROUBLECELL"))
                    {
                        if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkViolet);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }


                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROCNAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROCNAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                }
            }));
        }

        private void dgWipbyOper_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                if (!Util.NVC(dgWipbyOper.GetCell(e.Row.Index, dgWipbyOper.Columns["PROCNAME"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계")))
                    && !Util.NVC(dgWipbyOper.GetCell(e.Row.Index, dgWipbyOper.Columns["PROCNAME"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                {
                    tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 30)
            {
                tbTime.Visibility = Visibility.Visible;
                _timer.Stop();
            }
        }

        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Visible;
            GetList();
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Collapsed;
            GetList();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            dgWipbyOper.Columns["AGINGOVER1TRAY"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["AGINGOVER1CELL"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["AGINGOVER2TRAY"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["AGINGOVER2CELL"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["AGINGOVER3TRAY"].Visibility = Visibility.Visible;
            dgWipbyOper.Columns["AGINGOVER3CELL"].Visibility = Visibility.Visible;

        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            dgWipbyOper.Columns["AGINGOVER1TRAY"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["AGINGOVER1CELL"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["AGINGOVER2TRAY"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["AGINGOVER2CELL"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["AGINGOVER3TRAY"].Visibility = Visibility.Collapsed;
            dgWipbyOper.Columns["AGINGOVER3CELL"].Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                    _timer.Tick += new EventHandler(timer_Tick);
                }

                tbTime.Visibility = Visibility.Collapsed;
                sec = 0;
                _timer.Start();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BY_MODEL", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("SORT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);

                if (!string.IsNullOrEmpty(txtLotId.Text))
                    dr["LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);

                dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["BY_MODEL"] = chkModel.IsChecked == true ? "Y" : "N";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가
                dr["SORT"] = Util.GetCondition(cboSort, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WIP_RETRIEVE_INFO_SPECIAL_TOT_ROUTE_MODEL_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (chkModel.IsChecked == true)
                {
                    dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                    dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Visible;

                    DataTable NewTable = new DataTable();
                    if (dtRslt.Rows.Count > 0)
                    {
                        NewTable = gridSumRowAddByModel(dtRslt);
                        NewTable.Merge(gridSumRowAddALL(dtRslt));

                        Util _Util = new Util();
                        string[] sColumnName = new string[] { "MDLLOT_ID", "MODEL_NAME", "PROCNAME" };
                        _Util.SetDataGridMergeExtensionCol(dgWipbyOper, sColumnName, DataGridMergeMode.VERTICAL);
                    }
                    Util.GridSetData(dgWipbyOper, NewTable, FrameOperation, true);
                }
                else
                {
                    dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                    dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Collapsed;

                    DataTable NewTable = new DataTable();
                    if (dtRslt.Rows.Count > 0)
                    {
                        NewTable = dtRslt.Copy();
                        NewTable.Merge(gridSumRowAddALL(dtRslt));

                        Util _Util = new Util();
                        string[] sColumnName = new string[] { "PROCNAME" };
                        _Util.SetDataGridMergeExtensionCol(dgWipbyOper, sColumnName, DataGridMergeMode.VERTICAL);
                    }
                    Util.GridSetData(dgWipbyOper, NewTable, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void Load_FCS002_005_01(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLOT_ID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName)
        {
            //일별 출고 예정
            FCS002_005_01 DayIssueList = new FCS002_005_01();
            DayIssueList.FrameOperation = FrameOperation;

            object[] Parameters = new object[17];
            Parameters[0] = sOPER; //sOPER
            Parameters[1] = sOPER_NAME; //sOPER_NAME
            Parameters[2] = sLINE_ID; //sLINE_ID
            Parameters[3] = sLINE_NAME; //sLINE_NAME
            Parameters[4] = sROUTE_ID; //sROUTE_ID
            Parameters[5] = sROUTE_NAME; //sROUTE_NAME
            Parameters[6] = sMODEL_ID; //sMODEL_ID
            Parameters[7] = sMODEL_NAME; //sMODEL_NAME
            Parameters[8] = sROUTE_TYPE_DG; //sROUTE_TYPE_DG
            Parameters[9] = sROUTE_TYPE_DG_NAME; //sROUTE_TYPE_DG_NAME
            Parameters[10] = sStatus; //sStatus
            Parameters[11] = sStatusName; //sStatusName
            Parameters[12] = sSPECIAL_YN; //sSPECIAL_YN
            Parameters[13] = sSpecialName; //sSpecialName
            Parameters[14] = sLOT_ID; //sLOT_ID
            //LotType 추가
            Parameters[15] = sLotType;
            Parameters[16] = sLotTypeName;

            this.FrameOperation.OpenMenuFORM("FCS002_005_01", "FCS002_005_01", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("일별 출고 현황"), true, Parameters);
        }

        private void Load_FCS002_005_02(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLotID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName)
        {
            //Tray List
            FCS002_005_02 TrayList = new FCS002_005_02();
            TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[19];
            Parameters[0] = sOPER; //sOPER
            Parameters[1] = sOPER_NAME; //sOPER_NAME
            Parameters[2] = sLINE_ID; //sLINE_ID
            Parameters[3] = sLINE_NAME; //sLINE_NAME
            Parameters[4] = sROUTE_ID; //sROUTE_ID
            Parameters[5] = sROUTE_NAME; //sROUTE_NAME
            Parameters[6] = sMODEL_ID; //sMODEL_ID
            Parameters[7] = sMODEL_NAME; //sMODEL_NAME
            Parameters[8] = sStatus; //sStatus
            Parameters[9] = sStatusName; //sStatusName
            Parameters[10] = sROUTE_TYPE_DG; //sROUTE_TYPE_DG
            Parameters[11] = sROUTE_TYPE_DG_NAME; //sROUTE_TYPE_DG_NAME
            Parameters[12] = sLotID; //sLotID
            Parameters[13] = sSPECIAL_YN; //sSPECIAL_YN
            Parameters[14] = "";
            Parameters[15] = "";
            Parameters[16] = "";
            Parameters[17] = sLotType;
            Parameters[18] = sLotTypeName;

            this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
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

        //20220119_모델별 합계 Row 추가 START
        private DataTable gridSumRowAddByModel(DataTable dtRslt)
        {
            DataTable NewTable = new DataTable();
            DataTable TmpTable = new DataTable();

            var modelList = dtRslt.AsEnumerable()
            .GroupBy(g => new
            {
                MODEL_ID = g.Field<string>("MDLLOT_ID"),
                MODEL_NAME = g.Field<string>("MODEL_NAME")
            })
            .Select(f => new
            {
                modelID = f.Key.MODEL_ID,
                modelName = f.Key.MODEL_NAME
            })
            .OrderBy(o => o.modelID).ToList();

            foreach (var modelItem in modelList)
            {
                TmpTable = dtRslt.Select("MDLLOT_ID = '" + Util.NVC(modelItem.modelID) + "'").CopyToDataTable();

                int WaitTray = 0;
                int WaitCell = 0;
                int WorkTray = 0;
                int WorkCell = 0;
                int AgingEndTray = 0;
                int AgingEndCell = 0;

                int AgingOver1Tray = 0;
                int AgingOver1Cell = 0;
                int AgingOver2Tray = 0;
                int AgingOver2Cell = 0;
                int AgingOver3Tray = 0;
                int AgingOver3Cell = 0;

                int TroubleTray = 0;
                int TroubleCell = 0;
                int RecheckTray = 0;
                int RecheckCell = 0;
                int TotalTray = 0;
                int TotalInputCell = 0;
                int TotalCurrCell = 0;

                for (int iRow = 0; iRow < TmpTable.Rows.Count; iRow++)
                {
                    WaitTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITTRAY"])));
                    WaitCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITCELL"])));
                    WorkTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKTRAY"])));
                    WorkCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKCELL"])));
                    AgingEndTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDTRAY"])));
                    AgingEndCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDCELL"])));

                    AgingOver1Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER1TRAY"])));
                    AgingOver1Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER1CELL"])));
                    AgingOver2Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER2TRAY"])));
                    AgingOver2Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER2CELL"])));
                    AgingOver3Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER3TRAY"])));
                    AgingOver3Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER3CELL"])));
                    
                    TroubleTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLETRAY"])));
                    TroubleCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLECELL"])));
                    RecheckTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKTRAY"])));
                    RecheckCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKCELL"])));
                    TotalTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALTRAY"])));
                    TotalInputCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALINPUTCELL"])));
                    TotalCurrCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALCURRCELL"])));
                }

                DataRow newRow = TmpTable.NewRow();
                newRow["MDLLOT_ID"] = Util.NVC(modelItem.modelID);
                newRow["MODEL_NAME"] = Util.NVC(modelItem.modelName);
                newRow["PROCNAME"] = ObjectDic.Instance.GetObjectName("합계");
                newRow["WAITTRAY"] = WaitTray;
                newRow["WAITCELL"] = WaitCell;
                newRow["WORKTRAY"] = WorkTray;
                newRow["WORKCELL"] = WorkCell;
                newRow["AGINGENDTRAY"] = AgingEndTray;
                newRow["AGINGENDCELL"] = AgingEndCell;

                newRow["AGINGOVER1TRAY"] = AgingOver1Tray;
                newRow["AGINGOVER1CELL"] = AgingOver1Cell;
                newRow["AGINGOVER2TRAY"] = AgingOver2Tray;
                newRow["AGINGOVER2CELL"] = AgingOver2Cell;
                newRow["AGINGOVER3TRAY"] = AgingOver3Tray;
                newRow["AGINGOVER3CELL"] = AgingOver3Cell;
                
                newRow["TROUBLETRAY"] = TroubleTray;
                newRow["TROUBLECELL"] = TroubleCell;
                newRow["RECHECKTRAY"] = RecheckTray;
                newRow["RECHECKCELL"] = RecheckCell;
                newRow["TOTALTRAY"] = TotalTray;
                newRow["TOTALINPUTCELL"] = TotalInputCell;
                newRow["TOTALCURRCELL"] = TotalCurrCell;
                newRow["MAX_PROCID"] = string.Empty;
                newRow["MAX_PROCID_PROC_GR_CODE"] = string.Empty;
                TmpTable.Rows.Add(newRow);

                NewTable.Merge(TmpTable);
            }
            dtRslt = NewTable.Copy();

            return dtRslt;
        }
        //20220119_모델별 합계 Row 추가 END

        //20220119_모델별 합계 Row 추가 START
        private DataTable gridSumRowAddALL(DataTable dtRslt)
        {
            DataTable NewTable = new DataTable();
            DataTable TmpTable = new DataTable();

            var typeList = dtRslt.AsEnumerable()
            .GroupBy(g => new
            {
                SPCL_TYPE = g.Field<string>("SPCL_TYPE_CODE"),
                SPCLL_NAME = g.Field<string>("SPECIAL_NAME")
            })
            .Select(f => new
            {
                spclType = f.Key.SPCL_TYPE,
                spclName = f.Key.SPCLL_NAME
            })
            .OrderBy(o => o.spclType).ToList();

            NewTable = dtRslt.Copy();
            NewTable.Clear();

            foreach (var typeItem in typeList)
            {
                TmpTable = dtRslt.Select("SPCL_TYPE_CODE = '" + Util.NVC(typeItem.spclType) + "'").CopyToDataTable();

                int WaitTray = 0;
                int WaitCell = 0;
                int WorkTray = 0;
                int WorkCell = 0;
                int AgingEndTray = 0;
                int AgingEndCell = 0;

                int AgingOver1Tray = 0;
                int AgingOver1Cell = 0;
                int AgingOver2Tray = 0;
                int AgingOver2Cell = 0;
                int AgingOver3Tray = 0;
                int AgingOver3Cell = 0;

                int TroubleTray = 0;
                int TroubleCell = 0;
                int RecheckTray = 0;
                int RecheckCell = 0;
                int TotalTray = 0;
                int TotalInputCell = 0;
                int TotalCurrCell = 0;

                for (int iRow = 0; iRow < TmpTable.Rows.Count; iRow++)
                {
                    WaitTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITTRAY"])));
                    WaitCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITCELL"])));
                    WorkTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKTRAY"])));
                    WorkCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKCELL"])));
                    AgingEndTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDTRAY"])));
                    AgingEndCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDCELL"])));

                    AgingOver1Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER1TRAY"])));
                    AgingOver1Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER1CELL"])));
                    AgingOver2Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER2TRAY"])));
                    AgingOver2Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER2CELL"])));
                    AgingOver3Tray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER3TRAY"])));
                    AgingOver3Cell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGOVER3CELL"])));

                    TroubleTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLETRAY"])));
                    TroubleCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLECELL"])));
                    RecheckTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKTRAY"])));
                    RecheckCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKCELL"])));
                    TotalTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALTRAY"])));
                    TotalInputCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALINPUTCELL"])));
                    TotalCurrCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALCURRCELL"])));
                }

                DataRow newRow = NewTable.NewRow();
                newRow["MDLLOT_ID"] = string.Empty;
                newRow["MODEL_NAME"] = string.Empty;
                newRow["PROCNAME"] = ObjectDic.Instance.GetObjectName("ALL_SUM");
                newRow["SPCL_TYPE_CODE"] = Util.NVC(typeItem.spclType);
                newRow["SPECIAL_NAME"] = Util.NVC(typeItem.spclName);
                newRow["WAITTRAY"] = WaitTray;
                newRow["WAITCELL"] = WaitCell;
                newRow["WORKTRAY"] = WorkTray;
                newRow["WORKCELL"] = WorkCell;
                newRow["AGINGENDTRAY"] = AgingEndTray;
                newRow["AGINGENDCELL"] = AgingEndCell;

                newRow["AGINGOVER1TRAY"] = AgingOver1Tray;
                newRow["AGINGOVER1CELL"] = AgingOver1Cell;
                newRow["AGINGOVER2TRAY"] = AgingOver2Tray;
                newRow["AGINGOVER2CELL"] = AgingOver2Cell;
                newRow["AGINGOVER3TRAY"] = AgingOver3Tray;
                newRow["AGINGOVER3CELL"] = AgingOver3Cell;

                newRow["TROUBLETRAY"] = TroubleTray;
                newRow["TROUBLECELL"] = TroubleCell;
                newRow["RECHECKTRAY"] = RecheckTray;
                newRow["RECHECKCELL"] = RecheckCell;
                newRow["TOTALTRAY"] = TotalTray;
                newRow["TOTALINPUTCELL"] = TotalInputCell;
                newRow["TOTALCURRCELL"] = TotalCurrCell;
                newRow["MAX_PROCID"] = string.Empty;
                newRow["MAX_PROCID_PROC_GR_CODE"] = string.Empty;
                NewTable.Rows.Add(newRow);

                //NewTable.Merge(TmpTable);
            }
            dtRslt = NewTable.Copy();

            return dtRslt;
        }
        //20220119_모델별 합계 Row 추가 END

        #endregion

    }
}
