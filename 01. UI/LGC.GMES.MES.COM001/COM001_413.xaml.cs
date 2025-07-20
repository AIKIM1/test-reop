/*************************************************************************************
 Created Date : 2025.07.17
      Creator : 이원용
   Decription : 재공정보현황SNAP
--------------------------------------------------------------------------------------
 [Change History]
  2025.07.17  DEVELOPER : Initial Created.
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_413 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DispatcherTimer _timer = new DispatcherTimer();
        DispatcherTimer _Auto_timer = new DispatcherTimer();
        private int sec = 0;
        private int sec2 = 0;
        bool bUseFlag = false; //2023.08.14 NA1동 경과일 및 상 하층 레인 추가
        bool bComboCheckFlag = false; // 2023.11.17 콤보박스 변화 체크     
        int lineTotalCnt = 0;         // 2023.11.17 Line 총 개수
        int modelTotalCnt = 0;        // 2024.07.23 Juwita N, Total Count Model
        DataTable dtResult_1;         // 2023.11.17 Line MultiSelectionBox 데이터 담기
        DataTable dtResult_2;         // 2024.07.23 Juwita N, Model MultiSelectionBox Data

        public COM001_413()
        {
            InitializeComponent();
            InitCombo();

            //_timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            //_timer.Tick += new EventHandler(timer_Tick);
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

            //C1ComboBox[] cboLineChild = { cboModel };
            //_combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            //C1ComboBox[] cboModelChild = { cboRoute };
            //C1ComboBox[] cboModelParent = { cboLine };
            //_combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            string[] sFilter = { "ROUT_TYPE_GR_CODE" };
            //C1ComboBox[] cboroutesetchild = { cboRoute };
            //_combo.SetCombo(cboRouteDG, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboroutesetchild);
            _combo.SetCombo(cboRouteDG, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter);

            // 2021-05-13 Parent Combobox Parameter 매핑 오류로 인하여 null Combobox 추가
            //C1ComboBox ncbo = new C1ComboBox();
            //C1ComboBox[] cborouteParent = { cboLine, cboModel, ncbo, ncbo, cboRouteDG };
            //_combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", cbParent: cborouteParent);

            string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가

            /// 상 / 하층레인 검색조건 추가
            ///2023.08.14 

            string[] sFilter2 = { "FLOOR_CODE" };
            //_combo.SetCombo(cboFloor, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter2);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetLineCombo(cboLine);
            SetLineModel(cboModel);
            SetFormRoute(cboRoute);
            bComboCheckFlag = true;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);

            _Auto_timer = new DispatcherTimer();

            ///2023.08.14 
            /// ToolTip 추가 (작업 시작일자로부터 입력한 경과일이 지난 데이터를 보여줍니다.)

            //bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "COM001_413"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            //if (bUseFlag)
            //{

            //    GridOverTime.Visibility = Visibility.Visible;
            //    GridFloor.Visibility = System.Windows.Visibility.Visible;
            //    tbOverTime.SetValidation(MessageDic.Instance.GetMessage("SFU9024", txtOverTime.Text));

            //}
            //else
            //{
            //    GridOverTime.Visibility = Visibility.Collapsed;
            //    GridFloor.Visibility = System.Windows.Visibility.Collapsed;

            //}



            this.Loaded -= UserControl_Loaded;
        }
        private void auto_Timer_Tick(object sender, EventArgs e)
        {
            sec2++;
            if (sec2 >= 30)
            {
                btnSearch_Click(null, null);
                sec2 = 0;
            }
        }

        private void SetLineCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("AREAID", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqstA.Rows.Add(drA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", dtRqstA);

                dtResult_1 = dtResult;
                lineTotalCnt = dtResult.Rows.Count;

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
                Util.MessageException(ex);
            }
        }

        private void SetLineModel(MultiSelectionBox cbo)
        {
            try
            {
                // 2024.07.23 Fauzul A, Removing unused column (AREAID) & row, and using new DA (DA_BAS_SEL_COMBO_LINE_MULTI_MODEL_IN_WIP)
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = cboLine.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MULTI_MODEL_IN_WIP", "RQSTDT", "RSLTDT", RQSTDT);

                dtResult_2 = dtResult;
                modelTotalCnt = dtResult.Rows.Count;

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                //2024.07.23 Juwita N, Storing the data to the multiselectionbox
                // cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                // if (cbo.Items.Count > 0)
                //     cbo.SelectedIndex = 0;

                if (dtResult.Rows.Count != 0)
                {
                    cbo.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                        cbo.Check(-1);
                    }
                    else
                    {
                        cbo.isAllUsed = true;
                        cbo.ItemsSource = DataTableConverter.Convert(dtResult);
                        cbo.CheckAll();
                    }
                }
                else
                {
                    //2024.07.23 Fauzul A, Storing Null data to prevent error popup
                    //cbo.ItemsSource = null;
                    cbo.ItemsSource = dtResult.Copy().AsDataView();
                    cbo.CheckAll();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetFormRoute(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(cboLine.SelectedItemsToString)) ? cboLine.SelectedItemsToString : null;   //cboline.getbindvalue()

                //2023.07.23, Fauzul A, Setting model to the row
                //dr["MDLLOT_ID"] = cboModel.GetBindValue();
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(cboModel.SelectedItemsToString)) ? cboModel.SelectedItemsToString : null;
                dr["ROUTE_TYPE_DG"] = cboRouteDG.GetBindValue();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_MULTI_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void cboLine_Loaded(object sender, RoutedEventArgs e)
        {
            if (cboLine.ItemsSource == null) SetLineModel(cboModel);
        }

        private void cboLine_SelectionChanged(object sender, EventArgs e)
        {
            if (cboLine.SelectedItems.Count == 0)
            {
                cboLine.CheckAll();
            }
        }

        private void cboLine_DropDownClosed(object sender)
        {
            if (sender == null) return;
            SetLineModel(cboModel);
            SetFormRoute(cboRoute);
        }

        private void cboRouteDg_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null) return;
            if (bComboCheckFlag) SetFormRoute(cboRoute);
        }

        //2024.07.23 Juwita N, Commenting due to changes and unused. The new one is cboModel_SelectionChanged 
        //private void cboModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    if (sender == null) return;
        //    if (bComboCheckFlag) SetFormRoute(cboRoute);
        //}

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyOper_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (sender == null) return;

            //try
            //{
            //    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            //    if (sec >= 30)
            //    {
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0352"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            //        {
            //            if (result == MessageBoxResult.OK)
            //            {
            //                btnSearch.Focus();
            //            }
            //        });
            //        return;
            //    }

            //    if (dgWipbyOper.CurrentRow != null && (dgWipbyOper.CurrentColumn.Name.Equals("WAITTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WAITCELL") ||
            //        dgWipbyOper.CurrentColumn.Name.Equals("WORKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WORKCELL") ||
            //        dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDCELL") ||
            //        dgWipbyOper.CurrentColumn.Name.Equals("TROUBLETRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TROUBLECELL") ||
            //        dgWipbyOper.CurrentColumn.Name.Equals("RECHECKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("RECHECKCELL") ||
            //        dgWipbyOper.CurrentColumn.Name.Equals("TOTALTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALINPUTCELL")))
            //    {
            //    }
            //    else
            //    {
            //        return;
            //    }

            //    if (Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dgWipbyOper.CurrentColumn.Name.ToString())).Equals("0")) return;
            //    string sOPER = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID"));
            //    string sOPER_NAME = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCNAME"));
            //    string sMAX_PROCID_PROC_GR_CODE = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MAX_PROCID_PROC_GR_CODE"));
            //    //if (fpsWipbyOper.ActiveSheet.Rows[e.Row].Tag.ToString().Equals("sum"))
            //    //{
            //    //    sOPER = string.Empty;
            //    //    sOPER_NAME = "-ALL-";
            //    //}
            //    string sLINE_ID = cboLine.SelectedItemsToString;
            //    string sLINE_NAME = "";

            //    List<string> LcboLine = new List<string>();
            //    int arr_cnt = sLINE_ID.Split(',').Length;

            //    if (lineTotalCnt == arr_cnt)
            //    {
            //        sLINE_NAME = "All";
            //    }
            //    else
            //    {
            //        foreach (DataRow item in dtResult_1.Rows)
            //        {
            //            for (int i = 0; i < arr_cnt; i++)
            //            {
            //                if (item["CBO_CODE"].ToString() == sLINE_ID.Split(',')[i].ToString())
            //                {
            //                    LcboLine.Add(item["CBO_NAME"].ToString());
            //                    break;
            //                }
            //            }
            //        }
            //        sLINE_NAME = String.Join(",", LcboLine);
            //    }

            //    string sROUTE_ID = Util.GetCondition(cboRoute);
            //    string sROUTE_NAME = cboRoute.Text;

            //    //2024.07.23 Juwita N, The selected item can directly be stored
            //    //string sMODEL_ID = Util.GetCondition(cboModel);
            //    //string sMODEL_NAME = cboModel.Text;
            //    //if (chkModel.IsChecked.Equals(true))
            //    //{
            //    //    sMODEL_ID = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MDLLOT_ID"));
            //    //    sMODEL_NAME = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MODEL_NAME"));
            //    //}
            //    string sMODEL_ID = cboModel.SelectedItemsToString;
            //    string sMODEL_NAME = "";

            //    List<string> LcboModel = new List<string>();
            //    int arr_cntModel = sMODEL_ID.Split(',').Length;

            //    if (modelTotalCnt == arr_cntModel)
            //    {
            //        sMODEL_NAME = "All";
            //    }
            //    else
            //    {
            //        foreach (DataRow item in dtResult_2.Rows)
            //        {
            //            for (int i = 0; i < arr_cntModel; i++)
            //            {
            //                if (item["CBO_CODE"].ToString() == sMODEL_ID.Split(',')[i].ToString())
            //                {
            //                    LcboModel.Add(item["CBO_NAME"].ToString());
            //                    break;
            //                }
            //            }
            //        }
            //        sMODEL_NAME = String.Join(",", LcboModel);
            //    }
            //    // 2024.08.14 Ahmad F, Storing only selected Model name
            //    if (chkModel.IsChecked.Equals(true))
            //    {
            //        sMODEL_ID = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MDLLOT_ID"));
            //        sMODEL_NAME = Util.NVC(DataTableConverter.GetValue(dgWipbyOper.CurrentRow.DataItem, "MODEL_NAME"));
            //    }

            //    string sStatus = null;
            //    string sStatusName = null;
            //    string sLotID = Util.GetCondition(txtLotId);
            //    string sSpecial = Util.GetCondition(cboSpecial);
            //    string sSpecialName = cboSpecial.Text;
            //    string sRouteTypeDG = Util.GetCondition(cboRouteDG);
            //    string sRouteTypeDGName = cboRouteDG.Text;
            //    string sLotType = Util.GetCondition(cboLotType);
            //    string sLotTypeName = cboLotType.Text;
            //    //string sExceptHold = chkHold.IsChecked == true ? "Y" : "N";

            //    ///2023.08.14 
            //    string sOverTime = string.IsNullOrEmpty(tbOverTime.Text) ? null : (Int32.Parse(tbOverTime.Text) * 24 * 60).ToString();
            //    string sFloorLane = string.Empty;// Util.GetCondition(cboFloor);

            //    if (dgWipbyOper.CurrentRow != null && (dgWipbyOper.CurrentColumn.Name.Equals("WORKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WORKCELL")))
            //    {
            //        sStatus = "S";
            //        sStatusName = ObjectDic.Instance.GetObjectName("WORK_TRAY");  //작업Tray

            //        if (sOPER.Length > 1)
            //        {
            //            if (sMAX_PROCID_PROC_GR_CODE == "3"
            //                || sMAX_PROCID_PROC_GR_CODE == "4"
            //                || sMAX_PROCID_PROC_GR_CODE == "7"
            //                || sMAX_PROCID_PROC_GR_CODE == "9")
            //            {
            //                //Load_COM001_413_01(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName, sLotType, sLotTypeName, sExceptHold, sOverTime, sFloorLane);
            //                return;
            //            }
            //        }
            //    }
            //    else if (dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("AGINGENDCELL"))
            //    {
            //        sStatus = "P";
            //        sStatusName = ObjectDic.Instance.GetObjectName("AGING_END_WAIT"); //Aging 종료대기
            //    }
            //    else if (dgWipbyOper.CurrentColumn.Name.Equals("WAITTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("WAITCELL"))
            //    {
            //        sStatus = "E";
            //        sStatusName = ObjectDic.Instance.GetObjectName("WAIT"); //대기
            //    }
            //    else if (dgWipbyOper.CurrentColumn.Name.Equals("TROUBLETRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TROUBLECELL"))
            //    {
            //        sStatus = "T";
            //        sStatusName = ObjectDic.Instance.GetObjectName("WORK_ERR"); //작업이상
            //    }
            //    else if (dgWipbyOper.CurrentColumn.Name.Equals("RECHECKTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("RECHECKCELL"))
            //    {
            //        sStatus = "R";
            //        sStatusName = ObjectDic.Instance.GetObjectName("RECHECK"); //RECHECK
            //    }
            //    else if (dgWipbyOper.CurrentColumn.Name.Equals("TOTALTRAY") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALINPUTCELL") || dgWipbyOper.CurrentColumn.Name.Equals("TOTALCURRCELL"))
            //    {
            //        sStatus = "A";
            //        sStatusName = ObjectDic.Instance.GetObjectName("TOTAL"); //토탈
            //    }

            //   // Load_COM001_413_02(sOPER, sOPER_NAME, sLINE_ID, sLINE_NAME, sROUTE_ID, sROUTE_NAME, sMODEL_ID, sMODEL_NAME, sStatus, sStatusName, sLotID, sSpecial, sSpecialName, sRouteTypeDG, sRouteTypeDGName, sLotType, sLotTypeName, sExceptHold, sOverTime, sFloorLane);


            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
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
                if (e.Cell.Row.Type != DataGridRowType.Item) return;

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

                    //20220119_모델별 합계 Row 추가 START
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROCNAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROCNAME")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                    //20220119_모델별 합계 Row 추가 END
                }
            }));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //sec++;
            if (sec >= 30)
            {
                //tbTime.Visibility = Visibility.Visible;
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

        private void chkTimer_Checked(object sender, RoutedEventArgs e)
        {
            _Auto_timer.Interval = TimeSpan.FromTicks(10000000);
            _Auto_timer.Tick += new EventHandler(auto_Timer_Tick);
            _Auto_timer.Start();
        }

        private void chkTimer_Unchecked(object sender, RoutedEventArgs e)
        {
            _Auto_timer.Tick -= new EventHandler(auto_Timer_Tick);
            _Auto_timer.Stop();
        }

        ///2023.08.14 
        ///2023-08-14 엔터키 조회 이벤트 추가 1
        private void tbOverTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }
        //2023-06-15 경과일 입력 문자열 체크
        private bool GetDataTypeNumStr(string sInput)
        {
            int iCnt = 0;
            var sChar = sInput.ToCharArray();

            for (int i = 0; i < sChar.Length; i++)
            {
                if (char.IsNumber(sChar[i]) == false)
                {
                    iCnt = iCnt + 1;
                }
            }

            if (iCnt > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                ///2023.08.14 
                ///경과일 입력 문자열 체크
                if (GetDataTypeNumStr(tbOverTime.Text) == false)
                {
                    Util.Alert("SFU2877");  //숫자만 입력 가능합니다.
                    tbOverTime.Text = "";
                    return;
                }


                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                    _timer.Tick += new EventHandler(timer_Tick);
                }

                //tbTime.Visibility = Visibility.Collapsed;
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
                dtRqst.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BY_MODEL", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string)); // 2021.08.19 Lot 유형 검색조건 추가
                dtRqst.Columns.Add("SUM_TYPE", typeof(string));
                dtRqst.Columns.Add("SUM_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = (!string.IsNullOrEmpty(cboLine.SelectedItemsToString)) ? cboLine.SelectedItemsToString : null;

                //2024.07.23, Fauzul A, Setting the model to the row
                //dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["MDLLOT_ID"] = (!string.IsNullOrEmpty(cboModel.SelectedItemsToString)) ? cboModel.SelectedItemsToString : null;

                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);

                if (!string.IsNullOrEmpty(txtLotId.Text))
                    dr["LOTID"] = Util.GetCondition(txtLotId, bAllNull: true);

                dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["ROUT_TYPE_CODE"] = Util.GetCondition(cboRouteDG, bAllNull: true);
                dr["BY_MODEL"] = chkModel.IsChecked == true ? "Y" : "N";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true); // 2021.08.19 Lot 유형 검색조건 추가

                if ((bool)rdoCurrent.IsChecked)
                {
                    dr["SUM_TYPE"] = Util.NVC(rdoCurrent.Tag);
                    dr["SUM_DATE"] = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                }
                if ((bool)rdoDay.IsChecked)
                {
                    dr["SUM_TYPE"] = Util.NVC(rdoDay.Tag);
                    dr["SUM_DATE"] = dtpDate.SelectedDateTime.AddDays(-1).ToString("yyyyMMdd");
                }
                if ((bool)rdoMonth.IsChecked)
                {
                    dr["SUM_TYPE"] = Util.NVC(rdoMonth.Tag);
                    DateTime selectedDate = dtpMonth.SelectedDateTime;
                    DateTime lastDayOfPreviousMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1).AddDays(-1);
                    dr["SUM_DATE"] = lastDayOfPreviousMonth.ToString("yyyyMMdd");
                }



                dtRqst.Rows.Add(dr);

                btnSearch.IsEnabled = false;

                // 백그라운드 실행 실행, 완료 후 dgWipbyOper_ExecuteDataCompleted 이벤트 실행
                dgWipbyOper.ExecuteService("DA_PRD_SEL_STOCK_SNAP", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWipbyOper_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = true;

                DataTable dtRslt = e.ResultData as DataTable;

                if (chkModel.IsChecked == true)
                {
                    dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                    dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Visible;

                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "MDLLOT_ID", "MODEL_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgWipbyOper, sColumnName, DataGridMergeMode.VERTICAL);
                }
                else
                {
                    dgWipbyOper.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                    dgWipbyOper.Columns["MODEL_NAME"].Visibility = Visibility.Collapsed;
                }

                if (!dgWipbyOper.IsUserConfigUsing)
                {
                    dgWipbyOper.Columns.Where(w => !w.Width.IsStar).ToList()
                        .ForEach(x => x.Width = x is DataGridNumericColumn ? new C1.WPF.DataGrid.DataGridLength(100) : new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Load_COM001_413_01(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLOT_ID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName,
                                         string sExceptHold,
                                         string sOverTime, string sFloorLane)
        {
            //일별 출고 예정
            //COM001_413_01 DayIssueList = new COM001_413_01();
            //DayIssueList.FrameOperation = FrameOperation;

            object[] Parameters = new object[20];
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

            Parameters[17] = sExceptHold;
            ///2023.08.14 
            Parameters[18] = sOverTime;
            Parameters[19] = sFloorLane;
            //this.FrameOperation.OpenMenu("SFU010705051", true, Parameters);
            this.FrameOperation.OpenMenuFORM("SFU010705051", "COM001_413_01", "LGC.GMES.MES.COM001", ObjectDic.Instance.GetObjectName("일별 출고 현황"), true, Parameters);

            #region OLD
            //STM_211 stm211 = new STM_211();

            //stm211.OPER = sOPER;
            //stm211.OPERNAME = sOPER_NAME;

            //stm211.LINEID = sLINE_ID;
            //stm211.LINENAME = sLINE_NAME;

            //stm211.ROUTEID = sROUTE_ID;
            //stm211.ROUTENAME = sROUTE_NAME;

            //stm211.MODELID = sMODEL_ID;
            //stm211.MODELNAME = sMODEL_NAME;

            //stm211.ROUTETYPEDG = sROUTE_TYPE_DG;
            //stm211.ROUTETYPEDGNAME = sROUTE_TYPE_DG_NAME;

            //stm211.TRAYSTATUS = sStatus;
            //stm211.TRAYSTATUSNAME = sStatusName;

            //stm211.SPECIALYN = sSPECIAL_YN;
            //stm211.SPECIALYNNAME = sSpecialName;

            //stm211.LOTID = sLOT_ID;

            //FrmMain.ShowNewTab(stm211); 
            #endregion
        }

        private void Load_COM001_413_02(string sOPER, string sOPER_NAME,
                                         string sLINE_ID, string sLINE_NAME,
                                         string sROUTE_ID, string sROUTE_NAME,
                                         string sMODEL_ID, string sMODEL_NAME,
                                         string sStatus, string sStatusName,
                                         string sLotID, string sSPECIAL_YN,
                                         string sSpecialName,
                                         string sROUTE_TYPE_DG, string sROUTE_TYPE_DG_NAME,
                                         string sLotType, string sLotTypeName, string sExceptHold,
                                         string sOverTime, string sFloorLane)
        {
            //Tray List
            //COM001_413_02 TrayList = new COM001_413_02();
            //TrayList.FrameOperation = FrameOperation;

            object[] Parameters = new object[23];
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
            Parameters[19] = "";
            Parameters[20] = sExceptHold;
            ///2023.08.14 
            Parameters[21] = sOverTime;
            Parameters[22] = sFloorLane;
            //this.FrameOperation.OpenMenu("SFU010705052", true, Parameters);
            this.FrameOperation.OpenMenuFORM("SFU010705052", "COM001_413_02", "LGC.GMES.MES.COM001", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);

            #region OLD
            //stm212.OPER = sOPER;
            //stm212.OPERNAME = sOPER_NAME;

            //stm212.LINEID = sLINE_ID;
            //stm212.LINENAME = sLINE_NAME;

            //stm212.ROUTEID = sROUTE_ID;
            //stm212.ROUTENAME = sROUTE_NAME;

            //stm212.MODELID = sMODEL_ID;
            //stm212.MODELNAME = sMODEL_NAME;

            //stm212.TRAYSTATUS = sStatus;
            //stm212.TRAYSTATUSNAME = sStatusName;

            //stm212.ROUTETYPEDG = sROUTE_TYPE_DG;
            //stm212.ROUTETYPEDGNAME = sROUTE_TYPE_DG_NAME;

            //stm212.LOTID = sLotID;
            //stm212.SPECIALYN = sSPECIAL_YN; 
            #endregion
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

            NewTable = dtRslt.Copy();
            NewTable.Clear();

            int WaitTray = 0;
            int WaitCell = 0;
            int WorkTray = 0;
            int WorkCell = 0;
            int AgingEndTray = 0;
            int AgingEndCell = 0;
            int TroubleTray = 0;
            int TroubleCell = 0;
            int RecheckTray = 0;
            int RecheckCell = 0;
            int TotalTray = 0;
            int TotalInputCell = 0;
            int TotalCurrCell = 0;

            for (int iRow = 0; iRow < dtRslt.Rows.Count; iRow++)
            {
                WaitTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WAITTRAY"])));
                WaitCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WAITCELL"])));
                WorkTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WORKTRAY"])));
                WorkCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["WORKCELL"])));
                AgingEndTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["AGINGENDTRAY"])));
                AgingEndCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["AGINGENDCELL"])));
                TroubleTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["TROUBLETRAY"])));
                TroubleCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["TROUBLECELL"])));
                RecheckTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["RECHECKTRAY"])));
                RecheckCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["RECHECKCELL"])));
                TotalTray += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["TOTALTRAY"])));
                TotalInputCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["TOTALINPUTCELL"])));
                TotalCurrCell += Convert.ToInt32(Util.NVC(Convert.ToString(dtRslt.Rows[iRow]["TOTALCURRCELL"])));
            }

            DataRow newRow = NewTable.NewRow();
            newRow["MDLLOT_ID"] = string.Empty;
            newRow["MODEL_NAME"] = string.Empty;
            newRow["PROCNAME"] = ObjectDic.Instance.GetObjectName("ALL_SUM");
            newRow["WAITTRAY"] = WaitTray;
            newRow["WAITCELL"] = WaitCell;
            newRow["WORKTRAY"] = WorkTray;
            newRow["WORKCELL"] = WorkCell;
            newRow["AGINGENDTRAY"] = AgingEndTray;
            newRow["AGINGENDCELL"] = AgingEndCell;
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

            dtRslt = NewTable.Copy();

            return dtRslt;
        }
        //20220119_모델별 합계 Row 추가 END

        #endregion


        //2021-05-06 엔터키 조회 이벤트 추가
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void chkModel_HoldChecked(object sender, RoutedEventArgs e)
        {
            //chkHold:홀드제외 ,  chkOnlyHold:홀드만
            string objNm = ((CheckBox)sender).Name; //클릭한 녀석 이름

            //if (objNm.Equals("chkHold"))
            //{
            //    if (chkHold.IsChecked == true)     //chkHold:홀드제외 체크일 경우
            //    {
            //        chkOnlyHold.IsChecked = false;
            //    }
            //}
            //else
            //{
            //    if (chkOnlyHold.IsChecked == true) // chkOnlyHold:홀드만 체크일 경우
            //    {
            //        chkHold.IsChecked = false;
            //    }
            //}
        }

        //2024.07.23 Juwita N, adding new method for onchange the selection 
        private void cboModel_SelectionChanged(object sender, EventArgs e)
        {
            if (cboModel.SelectedItems.Count == 0)
            {
                cboModel.CheckAll();
            }
        }

        //2024.07.23 Juwita N, new method for dropdown closed 
        private void cboModel_DropDownClosed(object sender)
        {
            if (sender == null) return;
            SetFormRoute(cboRoute);
        }
        private void fnForceUpperCase_TextChanged(object sender, TextChangedEventArgs e)
        {

            dynamic textBox = sender; // UcBaseTextBox, TextBox 모두 적용
            try
            {
                //현재 커서 위치 저장
                int caretIndex = textBox.CaretIndex;

                //대문자로 변환
                string upper = textBox.Text.ToUpper();


                //변경된 내용이 있으면 적용
                if (textBox.Text != upper)
                {
                    textBox.Text = upper;
                    textBox.CaretIndex = caretIndex;
                }
            }
            catch
            {
                // 예외 발생 무시(Text 속성 없는 다른 컨트롤이 연결된 경우 등)
            }
        }
        private void rdoCurrent_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = false;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoDay_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = true;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoMonth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = false;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
