/*************************************************************************************
 Created Date : 2018.10.08
      Creator : 
   Decription : 라미대기 창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.08  오화백 : Initial Created.


 
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
using LGC.GMES.MES.MCS001.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        // 모니터링용 타이머 관련
        private System.Windows.Threading.DispatcherTimer monitorTimer = new System.Windows.Threading.DispatcherTimer();
        private string refeshAfter = "{0}";
        private bool bSetAutoSelTime = false;
        private DataTable DTOUT = null;
        //Rack 기본 셋팅 1호기 양극
        private static int MAX_ROW_C1 = 3;
        private static int MAX_COL_C1 = 20;
        //Rack 기본 셋팅 1호기 음극
        private static int MAX_ROW_A1 = 3;
        private static int MAX_COL_A1 = 20;
        //Rack 기본 셋팅 2호기 양극
        private static int MAX_ROW_C2 = 3;
        private static int MAX_COL_C2 = 20;
        //Rack 기본 셋팅 2호기 음극
        private static int MAX_ROW_A2 = 3;
        private static int MAX_COL_A2 = 20;
        //UserControl 
        private AssmLamiRack_CheckBox AssmRack = new AssmLamiRack_CheckBox();
        //PORT ID OR RACK ID
        private static string sPortOrRack = string.Empty;
        // Type : PORT 인지 RACK 인지
        private static string sType = string.Empty;
        //유저 컨트롤 버튼에 대한 정보 셋팅
        Button btnClick = new Button();
    
        //설비
        private static string sW1ANPW101 = "W1ANPW101";
        private static string sW1ANPW102 = "W1ANPW102";

        //컨베이어설비
        private static string sW1ACNV101 = "W1ACNV101";
        private static string sW1ACNV102 = "W1ACNV102";


        public MCS001_006()
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
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReJobLot);
            listAuth.Add(btnOrderLot);
            listAuth.Add(btnReturn);
            listAuth.Add(btnChangeCNV);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            InitRemakr();
            InitCombo();
            Refresh(true);
            TimerSetting(); //Timer
            sPortOrRack = string.Empty;
            sType = string.Empty;
            this.Loaded -= UserControl_Loaded;
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            // 자동 조회 시간 Combo
            String[] sFilter = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboReset, CommonCombo.ComboStatus.NA, sFilter: sFilter, sCase: "COMMCODE");
            if (cboReset != null && cboReset.Items != null && cboReset.Items.Count > 0)
                cboReset.SelectedIndex = 0;

            //창고재고정보 Combo
            _combo.SetCombo(cboWhInfo, CommonCombo.ComboStatus.ALL, sCase: "WH_QA");
            
            #region QA Combo 조회
            // QA Combo
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQGRID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQGRID"] = "NPW";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PRJT_FOR_MCS_COMBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboQA.DisplayMemberPath = "CBO_NAME";
            cboQA.SelectedValuePath = "CBO_CODE";
            cboQA.ItemsSource = dtResult.Copy().AsDataView();
            cboQA.SelectedIndex = 0;

            #endregion

            #region 입/출고 정보 조회

            //입/출고 정보 조회
            DataTable RQSTDT2 = new DataTable();
            RQSTDT2.TableName = "RQSTDT";
            RQSTDT2.Columns.Add("LANGID", typeof(string));
            RQSTDT2.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT2.Columns.Add("ATTRIBUTE2", typeof(string));

            DataRow dr2 = RQSTDT2.NewRow();
            dr2["LANGID"] = LoginInfo.LANGID;
            dr2["CMCDTYPE"] = "MCS_RACK_STAT_CODE";
            dr2["ATTRIBUTE2"] = "Y";
            RQSTDT2.Rows.Add(dr2);

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT2);
            cboRackState.DisplayMemberPath = "CBO_NAME";
            cboRackState.SelectedValuePath = "CBO_CODE";

            DataRow dataRow = dtResult2.NewRow();
            dataRow["CBO_CODE"] = string.Empty;
            dataRow["CBO_NAME"] = "-ALL-";
            dtResult2.Rows.InsertAt(dataRow, 0);

            cboRackState.ItemsSource = dtResult2.Copy().AsDataView();
            cboRackState.SelectedIndex = 0;
            #endregion

            #region 범례 정보 조회
            //Remark
            cboRemark.Items.Clear();

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboRemark.Items.Add(cbItemTitle);

            // DB에서 가져오기

            DataTable RQDT = new DataTable("RQSTDT");
            RQDT.Columns.Add("LANGID", typeof(string));

            DataRow drColor = RQDT.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;

            RQDT.Rows.Add(drColor);

            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COLOR_LEGEND", "RQSTDT", "RSLTDT", RQDT);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem();
                cbItem1.Content = row["KEYNAME"].ToString();
                cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;

                cboRemark.Items.Add(cbItem1);
            }
            cboRemark.SelectedIndex = 0;

            //cboColor.Height = 300;
            cboRemark.DropDownHeight = 400;
            #endregion

            #region 출고포트 조회
            //DataTable RQSTDT_PT = new DataTable();
            //RQSTDT_PT.TableName = "RQSTDT";
            //RQSTDT_PT.Columns.Add("LANGID", typeof(string));

            //DataRow dr_PT = RQSTDT_PT.NewRow();
            //dr_PT["LANGID"] = LoginInfo.LANGID;
            //RQSTDT_PT.Rows.Add(dr_PT);

            //DataTable dtResult_PT = null;

            //dtResult_PT = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_OUTPUT_PORT", "RQSTDT", "RSLTDT", RQSTDT_PT);

            //DataRow drSel = dtResult_PT.NewRow();

            //drSel["CBO_CODE"] = "SELECT";
            //drSel["CBO_NAME"] = "-SELECT-";
            //dtResult_PT.Rows.InsertAt(drSel, 0);

            //cboOutPort.DisplayMemberPath = "CBO_NAME";
            //cboOutPort.SelectedValuePath = "CBO_CODE";
            //cboOutPort.ItemsSource = dtResult_PT.Copy().AsDataView();

            //cboOutPort.SelectedValue = "SELECT";


            //// cboPort.MinHeight = 1000;
            ////cboPort.
            //cboOutPort.MaxDropDownHeight = 1000;


            //출고포트 조회
            String[] sFilter2 = { "", "NPW" };
            _combo.SetCombo(cboOutPort, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "CWALAMIPORT");
            #endregion

        }

        private void InitRemakr()
        {
            txtEqptID_1.Text = "Dryer";//EqptName(sW1ANPW101);
            txtEqptID_2.Text = "Dryer";// EqptName(sW1ANPW102);

            rdo_OneEqpt.Content = EqptName(sW1ANPW101);
            rdo_TwoEqpt.Content = EqptName(sW1ANPW102);
        }

        #endregion

        #region Event

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //조회조건에 LOT이 있으면 LOT을 조회해서 선택
            //아니면 전체 초기화
            if (txtLotS.Text == string.Empty)
            {
                Refresh(true);
            }
            else
            {
                SelectLOT();
            }
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region 출고대상조회 버튼 : btnSearch_OutPut_Click()
        /// <summary>
        /// 출고대상 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_OutPut_Click(object sender, RoutedEventArgs e)
        {
            OutputBind();
        }
        #endregion

        #region 창고정보조회(콤보박스)  : cboWhInfo_SelectedValueChanged()
        /// <summary>
        /// 창고정보 조회 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboWhInfo_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            WareHouseBind();
        }
        #endregion

        #region 창고정보조회(스프레드 머지) : dgWhereHose_MergingCells()
        /// <summary>
        /// 창고정보조회  스프레드 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWhereHose_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            if (dgWhereHose.GetRowCount() == 0)
                return;

            for (int i = 0; i <= dgWhereHose.GetRowCount(); i++)
            {
                if ((Util.NVC(DataTableConverter.GetValue(dgWhereHose.Rows[i + 1].DataItem, "CODE"))) == "N")
                {
                    e.Merge(new DataGridCellsRange(dgWhereHose.GetCell(i + 1, 1), dgWhereHose.GetCell(i + 1, 4)));
                }
            }
        }


        #endregion

        #region  창고정보조회(마우스더블클릭: 입고LOT 조회팝업) : dgWhereHose_MouseDoubleClick()  
        /// <summary>
        /// 입고LOT 조회 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWhereHose_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWhereHose.GetCellFromPoint(pnt);
                string POLARITY = string.Empty;
                if (cell != null)
                {
                    if (cell.Column.Name == "PRJT_NAME" || Util.NVC(DataTableConverter.GetValue(dgWhereHose.Rows[cell.Row.Index].DataItem, "CODE")) == "N")
                        return;
                    if (cell.Column.Name == "LOT_CNT_C" || cell.Column.Name == "TOTAL_QTY_C")
                    {
                        POLARITY = "C";
                    }
                    else
                    {
                        POLARITY = "A";
                    }
                }
                MCS001_006_INPUT_LOT popupInputLot = new MCS001_006_INPUT_LOT();
                popupInputLot.FrameOperation = this.FrameOperation;
                object[] parameters = new object[3];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgWhereHose.Rows[cell.Row.Index].DataItem, "PRJT_NAME")) == "TOTAL" ? string.Empty : Util.NVC(DataTableConverter.GetValue(dgWhereHose.Rows[cell.Row.Index].DataItem, "PRJT_NAME")); // 프로젝트명
                parameters[1] = POLARITY; //극성
                parameters[2] = string.Empty; //구분
                C1WindowExtension.SetParameters(popupInputLot, parameters);
                popupInputLot.Closed += new EventHandler(popupInputLot_Closed);
                grdMain.Children.Add(popupInputLot);
                popupInputLot.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        #endregion

        #region 창고정보조회(글자색) : dgWhereHose_LoadedCellPresenter()
        /// <summary>
        /// 글자색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWhereHose_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgWhereHose.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("LOT_CNT_C") || e.Cell.Column.Name.Equals("TOTAL_QTY_C") || e.Cell.Column.Name.Equals("LOT_CNT_A") || e.Cell.Column.Name.Equals("TOTAL_QTY_A"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CODE")).Equals("N"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Brown);

                    }
                }

            }));
        }

        #endregion

        #region QA검사(콤보박스) : cboQASelectedIndexChanged()
        private void cboQASelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            ReloadQA();
        }

        #endregion

        #region QA 검사(글자색) : dgQA_LoadedCellPresenter()
        /// <summary>
        /// QA 검사(글자색)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQA_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgQA.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("CC") || e.Cell.Column.Name.Equals("AC"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }

            }));
        }
        #endregion

        #region QA 검사(마우스더블클릭 : 입고LOT 조회팝업) : dgQA_MouseDoubleClick()
        /// <summary>
        /// QA 검사(마우스더블클릭 : 입고LOT 조회팝업)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgQA.GetCellFromPoint(pnt);
                string POLARITY = string.Empty;
                if (cell != null)
                {
                    //특성을 알수 없어서 클릭되는 컬럼에 따라 극성을 구분함
                    if (cell.Column.Name == "ELTR_TYPE_CODE")
                        return;
                    if (cell.Column.Name == "CC")
                    {
                        POLARITY = "C";
                    }
                    else
                    {
                        POLARITY = "A";
                    }
                }
                MCS001_006_INPUT_LOT popupInputLot = new MCS001_006_INPUT_LOT();
                popupInputLot.FrameOperation = this.FrameOperation;
                object[] parameters = new object[3];
                parameters[0] = string.Empty;
                parameters[1] = POLARITY; //극성
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgQA.Rows[cell.Row.Index].DataItem, "CODE")); //구분
                C1WindowExtension.SetParameters(popupInputLot, parameters);
                popupInputLot.Closed += new EventHandler(popupInputLot_Closed);
                grdMain.Children.Add(popupInputLot);
                popupInputLot.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        #endregion

        #region  LOT 조회조건 Key Down : txtLotS_KeyDown()
        /// <summary>
        /// LOT Text box KeyDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLotS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectLOT();
            }
        }
        #endregion

        #region Timer 관리 : cboReset_SelectedValueChanged(), OnUnloaded(), OnMonitorTimer()
        /// <summary>
        /// Timer 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboReset_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (monitorTimer != null)
                {
                    monitorTimer.Stop();

                    int iSec = 0;

                    if (cboReset != null && cboReset.SelectedValue != null && !cboReset.SelectedValue.ToString().Equals(""))
                    {
                        iSec = int.Parse(cboReset.SelectedValue.ToString());
                        bSetAutoSelTime = true;
                    }
                    else
                    {
                        bSetAutoSelTime = false;
                    }


                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        Util.MessageValidation("SFU1606");
                        return;
                    }
                    monitorTimer.Interval = new TimeSpan(0, 0, iSec);
                    monitorTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //  SKID BUFFER 모니터링 자동조회  %1초로 변경 되었습니다.                                     
                        Util.MessageInfo("SFU4537", cboReset.SelectedValue.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// Form Unload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            monitorTimer.Stop();
        }


        private void OnMonitorTimer(object sender, EventArgs e)
        {
            try
            {
                this.Refresh(false);
            }
            catch (Exception ex)
            {
                Util.Alert("Timer_Err" + ex.ToString());
            }
        }

        #endregion

        #region 1호기 창고 선택 : rdo_OneEqpt_Click()
        /// <summary>
        /// 1호기 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdo_OneEqpt_Click(object sender, RoutedEventArgs e)
        {
            //1호기가 선택시
            if (rdo_OneEqpt.IsChecked == true)
            {
                //기존 선택된 RACK정보, LOT정보 초기화
                Util.gridClear(dgLotInfo);
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C02))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A02))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                grdOneEqpt.Visibility = Visibility.Visible;
                grdTowEqpt.Visibility = Visibility.Collapsed;
            }
            else
            {
                Util.gridClear(dgLotInfo);
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C01))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A01))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                grdOneEqpt.Visibility = Visibility.Collapsed;
                grdTowEqpt.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 2호기 창고 선택 : rdo_TwoEqpt_Click()
        /// <summary>
        /// 2호기 선택시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdo_TwoEqpt_Click(object sender, RoutedEventArgs e)
        {
            //2호기가 선택시
            if (rdo_OneEqpt.IsChecked == true)
            {
                //기존 선택된 RACK정보, LOT정보 초기화
                Util.gridClear(dgLotInfo);
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C02))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A02))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                grdOneEqpt.Visibility = Visibility.Visible;
                grdTowEqpt.Visibility = Visibility.Collapsed;
            }
            else
            {
                Util.gridClear(dgLotInfo);
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C01))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A01))
                {
                    if (control != null)
                    {
                        control.IsChecked = false;
                    }
                }
                grdOneEqpt.Visibility = Visibility.Collapsed;
                grdTowEqpt.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region RACK 선택 : assmRack_Checked()
        /// <summary>
        /// RACK 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assmRack_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                //Thread.Sleep(2000);
                AssmLamiRack_CheckBox assmRack = sender as AssmLamiRack_CheckBox;
                if (assmRack == null) return;

                if (assmRack.IsChecked)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("RACKID", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["RACKID"] = assmRack.Name.ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dtRqst.Rows.Add(dr);
                    //RACK으로 LOT정보 조회
                    new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (result.Rows.Count > 0)
                        {
                            if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우 LOT ADD
                            {
                                try
                                {
                                    DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                                    DataRow newRow = null;
                                    for (int i = 0; i < result.Rows.Count; i++)
                                    {
                                        newRow = dtSource.NewRow();
                                        newRow["RACK_ID_2"] = result.Rows[i]["RACK_ID_2"];
                                        newRow["CHK"] = 0;
                                        newRow["RACK_ID"] = result.Rows[i]["RACK_ID"];
                                        newRow["PRJT_NAME"] = result.Rows[i]["PRJT_NAME"];
                                        newRow["PRODID"] = result.Rows[i]["PRODID"];
                                        newRow["WH_RCV_DTTM"] = result.Rows[i]["WH_RCV_DTTM"];
                                        newRow["WIPDTTM_ED"] = result.Rows[i]["WIPDTTM_ED"];
                                        newRow["LOTID"] = result.Rows[i]["LOTID"];
                                        newRow["WIP_QTY"] = result.Rows[i]["WIP_QTY"];
                                        newRow["JUDG_VALUE"] = result.Rows[i]["JUDG_VALUE"];
                                        newRow["VLD_DATE"] = result.Rows[i]["VLD_DATE"];
                                        newRow["SPCL_FLAG"] = result.Rows[i]["SPCL_FLAG"];
                                        newRow["TMP1"] = result.Rows[i]["TMP1"];
                                        newRow["SPCL_RSNCODE"] = result.Rows[i]["SPCL_RSNCODE"];
                                        newRow["TMP2"] = result.Rows[i]["TMP2"];
                                        newRow["WIP_REMARKS"] = result.Rows[i]["WIP_REMARKS"];
                                        newRow["TMP3"] = result.Rows[i]["TMP3"];
                                        newRow["WIPHOLD"] = result.Rows[i]["WIPHOLD"];
                                        newRow["RACK_STAT_CODE"] = result.Rows[i]["RACK_STAT_CODE"];
                                        newRow["EQPTID"] = result.Rows[i]["EQPTID"];
                                        newRow["EQPTNAME"] = result.Rows[i]["EQPTNAME"];
                                        newRow["X_PSTN"] = result.Rows[i]["X_PSTN"];
                                        newRow["Y_PSTN"] = result.Rows[i]["Y_PSTN"];
                                        newRow["Z_PSTN"] = result.Rows[i]["Z_PSTN"];
                                        newRow["ID_SEQ"] = result.Rows[i]["ID_SEQ"];
                                        newRow["WOID"] = result.Rows[i]["WOID"];
                                        newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                                        newRow["MODLID"] = result.Rows[i]["MODLID"];
                                        newRow["MCS_CST_ID"] = result.Rows[i]["MCS_CST_ID"];
                                        newRow["PRODDESC"] = result.Rows[i]["PRODDESC"];
                                        newRow["SAVE_YN"] = result.Rows[i]["SAVE_YN"];
                                        newRow["EQSGID"] = result.Rows[i]["EQSGID"];
                                        newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                                        newRow["TMP4"] = result.Rows[i]["TMP4"];
                                        newRow["TMP5"] = result.Rows[i]["TMP5"];
                                        newRow["TMP6"] = result.Rows[i]["TMP6"];
                                        newRow["TMP7"] = result.Rows[i]["TMP7"];
                                        newRow["TMP8"] = result.Rows[i]["TMP8"];
                                        newRow["TMP9"] = result.Rows[i]["TMP9"];
                                        dtSource.Rows.Add(newRow);
                                    }
                                    Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);
                                }
                                catch (Exception ex1)
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex1), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                }

                            }
                            else
                            {
                                Util.GridSetData(dgLotInfo, result, FrameOperation, false);

                                //축소
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);

                                btnBottomExpandFrame.IsChecked = true;

                            }
                        }


                        loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    DataTable _dtRackInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + assmRack.RackId + "'");

                    foreach (DataRow row in selectedRow)
                    {
                        _dtRackInfo.Rows.Remove(row);
                    }
                    Util.GridSetData(dgLotInfo, _dtRackInfo, FrameOperation, false);

                    if(dgLotInfo.Rows.Count == 0)
                    {
                        //확장
                        Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                        Monitoring.RowDefinitions[1].Height = new GridLength(8);
                        Monitoring.RowDefinitions[2].Height = new GridLength(8.3, GridUnitType.Star);
                        Monitoring.RowDefinitions[3].Height = new GridLength(8);
                        Monitoring.RowDefinitions[4].Height = new GridLength(31);

                        btnBottomExpandFrame.IsChecked = false;
                    }



                    loadingIndicator.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 출고대상Pancke정보(USING상태일경우만 수동출고가능) : dgLotInfo_BeginningEdit()
        private void dgLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (dgLotInfo.CurrentCell != null && dgLotInfo.SelectedIndex > -1)
            {
                if (dgLotInfo.CurrentCell.Column.Name == "CHK")
                {
                    string sStatCode = Util.NVC(dgLotInfo.GetCell(dgLotInfo.CurrentRow.Index, dgLotInfo.Columns["RACK_STAT_CODE"].Index).Value);
                    if (sStatCode == "USING")
                    {
                        e.Cancel = false;   // Editing 가능

                    }
                    else
                    {
                        e.Cancel = true;    // Editing 불가능
                    }
                }
            }
        }

        #endregion

        #region 수동출고예약 : btn_OutPut_Click()
        /// <summary>
        /// 수동출고예약
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OutPut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationReturn())
                return;
            string port = (string)cboOutPort.SelectedValue;


            //선택된 값 수 만큼 BIZ 호출(설계서상 요구사항임)
            Util.MessageConfirm("SFU4539", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sRackId = String.Empty;
                    string sLotId = String.Empty;
                    int DubRack = 0;
                    DataTable dtRack = new DataTable();
                    dtRack.Columns.Add("RACK_ID", typeof(string));
                    foreach (DataRow row in ((System.Data.DataView)dgLotInfo.ItemsSource).Table.Rows)
                    {
                        if (row["CHK"].ToString() == "1")
                        {
                            DubRack = 0;
                            if (dtRack.Rows.Count == 0)
                            {
                                DataRow dr = dtRack.NewRow();
                                dr["RACK_ID"] = row["RACK_ID"].ToString();
                                dtRack.Rows.Add(dr);
                            }
                            else
                            {
                                for (int i = 0; i < dtRack.Rows.Count; i++)
                                {
                                    if (dtRack.Rows[i]["RACK_ID"].ToString() == row["RACK_ID"].ToString())
                                    {
                                        DubRack = 1;
                                    }
                                }

                                if (DubRack == 0)
                                {
                                    DataRow dr = dtRack.NewRow();
                                    dr["RACK_ID"] = row["RACK_ID"].ToString();
                                    dtRack.Rows.Add(dr);
                                }
                            }
                        }
                    }
                    foreach (DataRow row in dtRack.Rows)
                    {

                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("FROM_ID", typeof(string));
                        inTable.Columns.Add("FROM_TYPE", typeof(string));
                        inTable.Columns.Add("TO_ID", typeof(string));
                        inTable.Columns.Add("TO_TYPE", typeof(string));
                        inTable.Columns.Add("LOGIS_CMD_PRIORITY_NO", typeof(Int32));
                        inTable.Columns.Add("USERID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));

                        DataRow newRow = inTable.NewRow();
                        newRow["FROM_ID"] = row["RACK_ID"].ToString();
                        newRow["FROM_TYPE"] = "RACK";
                        newRow["TO_ID"] = cboOutPort.SelectedValue.ToString();
                        newRow["TO_TYPE"] = "PORT";
                        newRow["LOGIS_CMD_PRIORITY_NO"] = 30;
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);

                        DataTable _dtRackInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                        DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + row["RACK_ID"].ToString() + "'");

                        foreach (DataRow Lotrow in selectedRow)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = Lotrow["LOTID"];
                            inLot.Rows.Add(newRow);
                        }
                        new ClientProxy().ExecuteService_Multi("BR_MCS_REG_LOGIS_CMD_NSP_NSR_MGV", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.AlertInfo("SFU1275");
                                this.Refresh(false);
                            }
                            catch (Exception ex)
                            {

                                Util.MessageException(ex);

                            }
                        }, inDataSet);
                    }
                }
            });
        }


        #endregion
     
        #region =============== 팝업 및 화면 이동 ==============

        #region 수동반송지시 팝업 : btnReturn_Click(), popupReturn_Closed()  === 사용안함
        /// <summary>
        /// 수동반송지시 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Visible;
            //try
            //{

            //    if (sType == "PORT")
            //        return;


            //    if (sPortOrRack.ToString() == string.Empty)
            //    {
            //        // %1(을)를 선택하세요.
            //        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("RACK"));
            //        return;
            //    }
            //    if (btnClick.Name.ToString().Substring(btnClick.Name.IndexOf("_") + 1, (btnClick.Name.Length - 1) - btnClick.Name.IndexOf("_")) == "USABLE")
            //    {
            //        // 선택한 렉에 펜케익이 없습니다..
            //        Util.MessageValidation("SFU5051");
            //        return;
            //    }
            //    MCS001_002_RETURN popupReturn = new MCS001_002_RETURN();
            //    popupReturn.FrameOperation = this.FrameOperation;


            //    object[] parameters = new object[4];
            //    parameters[0] = btnClick.Tag.ToString().Substring(btnClick.Tag.ToString().IndexOf("_") + 1, btnClick.Tag.ToString().Length - (btnClick.Tag.ToString().IndexOf("_") + 1)).Replace("\n", ",");// LOTID
            //    parameters[1] = sPortOrRack; //ID
            //    parameters[2] = sType; //Type
            //    parameters[3] = "NPW"; //EQGRID
            //    C1WindowExtension.SetParameters(popupReturn, parameters);

            //    popupReturn.Closed += new EventHandler(popupReturn_Closed);
            //    grdMain.Children.Add(popupReturn);
            //    popupReturn.BringToFront();
            //    loadingIndicator.Visibility = Visibility.Collapsed;

            //}
            //catch (Exception ex)
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //finally
            //{
            //    loadingIndicator.Visibility = Visibility.Collapsed;
            //}
        }

        /// <summary>
        /// 수동반송지시 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupReturn_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_002_RETURN popupReturn = sender as MCS001_002_RETURN;
            if (popupReturn.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                Refresh(true);
            }
            this.grdMain.Children.Remove(popupReturn);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 컨베어방향전환 팝업 : btnChangeCNV_Click(), popupChangeCnv_Closed()

        /// <summary>
        /// 컨베어방향전환
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangeCNV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_006_CHANGE_CNV popupChangeCnv = new MCS001_006_CHANGE_CNV();
                popupChangeCnv.FrameOperation = this.FrameOperation;
                popupChangeCnv.Closed += new EventHandler(popupChangeCnv_Closed);
                grdMain.Children.Add(popupChangeCnv);
                popupChangeCnv.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }

        }
        /// <summary>
        /// 컨베어방향전환 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupChangeCnv_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_006_CHANGE_CNV popupChangeCnv = sender as MCS001_006_CHANGE_CNV;
            if (popupChangeCnv.DialogResult == MessageBoxResult.OK)
            {

                this.grdMain.Children.Remove(popupChangeCnv);
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                Refresh(false);
            }
            else
            {
                this.grdMain.Children.Remove(popupChangeCnv);
            }
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 라미창고 입출고 이력조회 : btnInputOutputHist_Click()
        /// <summary>
        /// 라미창고 입출고 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputOutputHist_Click(object sender, RoutedEventArgs e)
        {
            //라미창고 입출고 이력조회
            loadingIndicator.Visibility = Visibility.Visible;

            object[] parameters = new object[1];
            parameters[0] = string.Empty;
            this.FrameOperation.OpenMenu("SFU010180070", true, parameters);

            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 라미 투입LOT 조회 : btnInputLot_Click(), popupInputLot_Closed()
        /// <summary>
        /// 라미 투입LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_006_INPUT_LOT popupInputLot = new MCS001_006_INPUT_LOT();
                popupInputLot.FrameOperation = this.FrameOperation;
                object[] parameters = new object[3];
                parameters[0] = string.Empty; //프로젝트명
                parameters[1] = string.Empty; //극성
                parameters[2] = string.Empty; //구분
                C1WindowExtension.SetParameters(popupInputLot, parameters);
                popupInputLot.Closed += new EventHandler(popupInputLot_Closed);
                grdMain.Children.Add(popupInputLot);
                popupInputLot.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        /// <summary>
        /// 라미 투입LOT 조회 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupInputLot_Closed(object sender, EventArgs e)
        {
            MCS001_006_INPUT_LOT popupInputLot = sender as MCS001_006_INPUT_LOT;
            if (popupInputLot.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popupInputLot);
        }
        #endregion

        #region 라미재작업LOT 조회 : btnReJobLot_Click(),popupReworkLot_Closed()
        /// <summary>
        /// 라미 재작업 LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReJobLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_006_REWORKLOT popupReworkLot = new MCS001_006_REWORKLOT();
                popupReworkLot.FrameOperation = this.FrameOperation;
                popupReworkLot.Closed += new EventHandler(popupReworkLot_Closed);
                grdMain.Children.Add(popupReworkLot);
                popupReworkLot.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void popupReworkLot_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_006_REWORKLOT popupReworkLot = sender as MCS001_006_REWORKLOT;
            if (popupReworkLot.DialogResult == MessageBoxResult.OK)
            {
                Refresh(false);
            }

            this.grdMain.Children.Remove(popupReworkLot);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 라미특이작업LOT 조회 : btnOrderLot_Click(),popupUnusualLot_Closed()
        /// <summary>
        /// 라미 특이작업LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOrderLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_006_UNUSUAL_LOT popupUnusualLot = new MCS001_006_UNUSUAL_LOT();
                popupUnusualLot.FrameOperation = this.FrameOperation;
                popupUnusualLot.Closed += new EventHandler(popupUnusualLot_Closed);
                grdMain.Children.Add(popupUnusualLot);
                popupUnusualLot.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void popupUnusualLot_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_006_UNUSUAL_LOT popupUnusualLot = sender as MCS001_006_UNUSUAL_LOT;
            if (popupUnusualLot.DialogResult == MessageBoxResult.OK)
            {
                Refresh(false);
            }

            this.grdMain.Children.Remove(popupUnusualLot);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }



        #endregion

        #region  Rack정보조회  : BtnUserControl_PreviewMouseDown(),assmRack_DoubleClick()
        private void BtnUserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount > 1)
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    try
                    {
                        if (sPortOrRack.ToString() == string.Empty)
                        {
                            // %1(을)를 선택하세요.
                            Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("RACK혹은PORT"));
                            return;
                        }

                        MCS001_006_LOT_INFO popupLotInfo = new MCS001_006_LOT_INFO();
                        popupLotInfo.FrameOperation = this.FrameOperation;

                        object[] parameters = new object[3];
                        parameters[0] = sPortOrRack;
                        parameters[1] = sType;


                        C1WindowExtension.SetParameters(popupLotInfo, parameters);

                        popupLotInfo.Closed += new EventHandler(popupLotInfo_Closed);
                        grdMain.Children.Add(popupLotInfo);
                        popupLotInfo.BringToFront();
                        loadingIndicator.Visibility = Visibility.Collapsed;

                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    Button btn = sender as Button;
                    if (btn.Tag == null) return;
                    if (string.IsNullOrWhiteSpace(btnClick.Name.ToString()))
                    {
                        btnClick = btn;
                    }
                    //전역변수 셋팅
                    sPortOrRack = btn.Tag.ToString().Substring(0, btnClick.Tag.ToString().IndexOf("_"));
                    sType = "PORT";
               
                }
            }
        }
        /// <summary>
        /// RACK 더블클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assmRack_DoubleClick(object sender, RoutedEventArgs e)
        {
           loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                AssmLamiRack_CheckBox assmRack = sender as AssmLamiRack_CheckBox;
                if (assmRack.Name.ToString() == string.Empty)
                {
                    // %1(을)를 선택하세요.
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("RACK혹은PORT"));
                    return;
                }

                MCS001_006_LOT_INFO popupLotInfo = new MCS001_006_LOT_INFO();
                popupLotInfo.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];
                parameters[0] = assmRack.Name;
                parameters[1] = assmRack.Check;


                C1WindowExtension.SetParameters(popupLotInfo, parameters);

                popupLotInfo.Closed += new EventHandler(popupLotInfo_Closed);
                grdMain.Children.Add(popupLotInfo);
                popupLotInfo.BringToFront();
                loadingIndicator.Visibility = Visibility.Collapsed;

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///  Rack정보조회 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupLotInfo_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_006_LOT_INFO popupLotInfo = sender as MCS001_006_LOT_INFO;
            if (popupLotInfo.DialogResult == MessageBoxResult.OK)
            {
                Refresh(false);
            }
            this.grdMain.Children.Remove(popupLotInfo);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #region 포트설정팝업 : btnPortSetting_Click(), popupPortSetting_Closed()
        /// <summary>
        /// 포트설정 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPortSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_006_PORT_SETTING popupPortSetting = new MCS001_006_PORT_SETTING();
                popupPortSetting.FrameOperation = this.FrameOperation;
                popupPortSetting.Closed += new EventHandler(popupPortSetting_Closed);
                grdMain.Children.Add(popupPortSetting);
                popupPortSetting.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        /// <summary>
        ///  포트설정 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupPortSetting_Closed(object sender, EventArgs e)
        {
            MCS001_006_PORT_SETTING popupPortSetting = sender as MCS001_006_PORT_SETTING;
            if (popupPortSetting.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popupPortSetting);
        }
        #endregion


        #endregion

        #region ============= 확장/축소 ===============

        #region 좌측축소 : btnLeftExpandFrame_Checked()
        /// <summary>
        ///  좌측축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            WhereHose.ColumnDefinitions[0].Width = new GridLength(2.7, GridUnitType.Star);
            WhereHose.ColumnDefinitions[1].Width = new GridLength(8);
            WhereHose.ColumnDefinitions[2].Width = new GridLength(7.3, GridUnitType.Star);
        }
        #endregion

        #region 좌측확장 : btnLeftExpandFrame_Unchecked()
        /// <summary>
        /// 좌측확장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLeftExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            // 확장
            WhereHose.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            WhereHose.ColumnDefinitions[1].Width = new GridLength(0);
            WhereHose.ColumnDefinitions[2].Width = new GridLength(10.0, GridUnitType.Star);

            //this.btnExpandFrame2.IsChecked = false;
            //RightArea.RowDefinitions[3].Height = new GridLength(30);
        }

        #endregion
        
        #region 하단축소 : btnBottomExpandFrame_Checked()
        /// <summary>
        /// 하단 축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBottomExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            //축소
            Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
            Monitoring.RowDefinitions[1].Height = new GridLength(8);
            Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
            Monitoring.RowDefinitions[3].Height = new GridLength(8);
            Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
       }
        #endregion

        #region 하단확장 : btnBottomExpandFrame_Unchecked()
        /// <summary>
        /// 하단 확장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBottomExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            //확장
            Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
            Monitoring.RowDefinitions[1].Height = new GridLength(8);
            Monitoring.RowDefinitions[2].Height = new GridLength(8.3, GridUnitType.Star);
            Monitoring.RowDefinitions[3].Height = new GridLength(8);
            Monitoring.RowDefinitions[4].Height = new GridLength(31);

        }
        #endregion

        #endregion
        
        #endregion

        #region Method

        #region MAX POSITION : MAX_POSITION()
        /// <summary>
        /// MAX POSITION
        /// </summary>
        /// <returns></returns>
        private DataTable MAX_POSITION(string Eqpt, string Zone)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = Eqpt;
            dr["ZONE_ID"] = Zone;
            dtRqst.Rows.Add(dr);
            return new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MAX_XYPOSITION_LAMI", "INDATA", "OUTDATA", dtRqst);
        }
        #endregion

        #region 재조회  : Refresh()

        private void Refresh(bool bButton)
        {
            try
            {
                monitorTimer.Stop();


                //버튼 조회시
                if (bButton == true)
                {
                    //loadingIndicator.Visibility = Visibility.Visible;
                    //초기화
                    Clear();
                    PrepareLayoutNoScroll_C1();
                    PrepareLayoutNoScroll_TopC1();
                    PrepareLayoutNoScroll_LeftC1();
                    PrepareLayoutNoScroll_A1();
                    PrepareLayoutNoScroll_TopA1();
                    PrepareLayoutNoScroll_LeftA1();
                    PrepareLayoutNoScroll_C2();
                    PrepareLayoutNoScroll_TopC2();
                    PrepareLayoutNoScroll_LeftC2();
                    PrepareLayoutNoScroll_A2();
                    PrepareLayoutNoScroll_TopA2();
                    PrepareLayoutNoScroll_LeftA2();
                    WareHouseBind(); //창고재고 정보
                    ReloadQA();
                    OutputBind();  //출고대상 정보
                    PortBind();    // PORT정보 
                    CNVBind();     // 컨베어 방향 조회 
                    DataBind_C1(bButton); //1호기 양극
                    DataBind_A1(bButton); //1호기 음극
                    DataBind_C2(bButton); //2호기 양극
                    DataBind_A2(bButton); //2호기 음극
                                   //loadingIndicator.Visibility = Visibility.Collapsed;


                }
                else // timer로 인한 조회시
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    DTOUT = new DataTable();
                    DTOUT = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    Util.gridClear(dgLotInfo);
                    PrepareLayoutNoScroll_C1();
                    PrepareLayoutNoScroll_TopC1();
                    PrepareLayoutNoScroll_LeftC1();
                    PrepareLayoutNoScroll_A1();
                    PrepareLayoutNoScroll_TopA1();
                    PrepareLayoutNoScroll_LeftA1();
                    PrepareLayoutNoScroll_C2();
                    PrepareLayoutNoScroll_TopC2();
                    PrepareLayoutNoScroll_LeftC2();
                    PrepareLayoutNoScroll_A2();
                    PrepareLayoutNoScroll_TopA2();
                    PrepareLayoutNoScroll_LeftA2();
                    WareHouseBind(); //창고재고 정보
                    ReloadQA();
                    PortBind();    // PORT정보 
                    CNVBind();     // 컨베어 방향 조회 
                    OutputBind();  //출고대상 정보
                    DataBind_C1(bButton); //1호기 양극
                    DataBind_A1(bButton); //1호기 음극
                    DataBind_C2(bButton); //2호기 양극
                    DataBind_A2(bButton); //2호기 음극
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }

            }
            finally
            {
                // 새로 고침 시간 update
                //refeshTo = refeshInterval;
                if (bSetAutoSelTime)
                {
                    monitorTimer.Start();
                }

                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Time 셋팅 : TimerSetting()
        private void TimerSetting()
        {
            //monitorTimer.Interval = new TimeSpan( 0, 0, 0, 60 );    // 60초에 한번
            monitorTimer.Interval = new TimeSpan(0, 0, 0, 1);
            monitorTimer.Tick += new EventHandler(OnMonitorTimer);
            refeshAfter = ObjectDic.Instance.GetObjectName("{0}초 후");
        }
        #endregion

        #region QA 검사 :ReloadQA()

        private void ReloadQA()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataRowView prjt_name = (DataRowView)cboQA.SelectedItem;
                string PRJT_NAME = prjt_name.Row["CBO_CODE"].ToString();


                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQGRID", typeof(string));
                if (PRJT_NAME != "0")
                {
                    inTable.Columns.Add("PRJT_NAME", typeof(string));
                }

                DataRow newRow = inTable.NewRow();
                newRow["EQGRID"] = "NPW";
                if (PRJT_NAME != "0")
                {
                    newRow["PRJT_NAME"] = PRJT_NAME;
                }

                inTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_MCS_SEL_QA_MEB", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        DataTable GridData = new DataTable();

                        GridData.Columns.Add("ELTR_TYPE_CODE", typeof(string)); // 
                        GridData.Columns.Add("CC", typeof(int)); // 양극
                        GridData.Columns.Add("AC", typeof(int)); // 음극
                        GridData.Columns.Add("CODE", typeof(string));

                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("라미대기"), 0, 0, 'Y' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("검사중"), 0, 0, 'I' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("재작업"), 0, 0, 'R' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("대기"), 0, 0, 'W' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("완공"), 0, 0, 'E' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("HOLD"), 0, 0, 'N' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("노칭재공종료"), 0, 0, 'V' });
                        GridData.Rows.Add(new object[] { ObjectDic.Instance.GetObjectName("라미재공종료"), 0, 0, 'F' });

                        foreach (DataRow dr in searchResult.Rows)
                        {
                            int idx = 0;
                            if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                            {
                                // 양극
                                idx = 1;
                            }
                            else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                            {
                                // 음극
                                idx = 2;
                            }
                            if (idx > 0)
                            {
                                GridData.Rows[0][idx] = dr["QA_Y"];
                                GridData.Rows[1][idx] = dr["QA_I"];
                                GridData.Rows[2][idx] = dr["QA_F"];
                                GridData.Rows[3][idx] = dr["QA_R"];
                                GridData.Rows[4][idx] = dr["QA_N"];
                                GridData.Rows[5][idx] = dr["QA_W"];
                                GridData.Rows[6][idx] = dr["QA_E"];
                                GridData.Rows[7][idx] = dr["QA_V"];
                            }
                        }
                        Util.GridSetData(this.dgQA, GridData, null, true);
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }
            );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 설비명조회 : EqptName()
        private string EqptName(string EqptID)
        {
            string ReturnEqptName = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID.ToString();
            dr["EQPTID"] = EqptID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_EQPTNAME", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult.Rows.Count > 0)
            {
                ReturnEqptName = dtResult.Rows[0]["EQPTNAME"].ToString();
            }
            return ReturnEqptName;

        }
        #endregion

        #region  초기화 :  Clear()
        /// <summary>
        /// 초기화
        /// </summary>
        private void Clear()
        {

            //상단 Clear
            Util.gridClear(dgLotInfo);
            //좌측 Clear
            Util.gridClear(dgWhereHose);
            //하단 Clear
            Util.gridClear(dgOutputInfo);
            sPortOrRack = string.Empty;
            sType = string.Empty;
            btnClick.Content = null;
        }

        #endregion

        #region ========   RACK, 연, 단 셋팅  ============

        #region  1호기 C_ZONE RACK 셋팅 : PrepareLayoutNoScroll_C1()
        /// <summary>
        /// 1호기 양극 RACK 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_C1()
        {
            stair_C01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_C01.ColumnDefinitions.Count > 0)
            {
                stair_C01.ColumnDefinitions.Clear();
            }
            if (stair_C01.RowDefinitions.Count > 0)
            {
                stair_C01.RowDefinitions.Clear();
            }

            DataTable dtMaxPosition = MAX_POSITION(sW1ANPW101, "C");
            if (dtMaxPosition.Rows[0]["X_LOCATION"].ToString() == string.Empty || dtMaxPosition.Rows[0]["Y_LOCATION"].ToString() == string.Empty)
                return;

            // X축 값 설정
            MAX_ROW_C1 = Convert.ToInt16(dtMaxPosition.Rows[0]["X_LOCATION"].ToString());
            // Y축 값 설정
            MAX_COL_C1 = Convert.ToInt16(dtMaxPosition.Rows[0]["Y_LOCATION"].ToString());

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_C1; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_C01.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_C1; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(65);
                stair_C01.RowDefinitions.Add(rowdef);
            }
            // BOARDER 추가
            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < MAX_ROW_C1; i++)
            {
                Border border = new Border();
                if (i == MAX_ROW_C1 - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_C1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_C1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_C01.Children.Add(border);
            }
            for (int i = 0; i < MAX_COL_C1; i++)
            {
                Border border = new Border();
                if (i == MAX_COL_C1 - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_C1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_C1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_C01.Children.Add(border);
            }
        }
        #endregion

        #region  1호기 C_ZONE 연 셋팅 : PrepareLayoutNoScroll_TopC1()
        /// <summary>
        /// 1호기 양극 연 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_TopC1()
        {
            stair_TopC01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_TopC01.ColumnDefinitions.Count > 0)
            {
                stair_TopC01.ColumnDefinitions.Clear();
            }
            if (stair_TopC01.RowDefinitions.Count > 0)
            {
                stair_TopC01.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_C1; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_TopC01.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(15);
            stair_TopC01.RowDefinitions.Add(rowdef);

            for (int i = 0; i < MAX_COL_C1; i++)
            {
                var tbC_TopC01 = new TextBlock() { Text = i + 1 + ObjectDic.Instance.GetObjectName("연"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                //tbC_TopC01.SetValue(Grid.RowProperty, 3);
                tbC_TopC01.SetValue(Grid.ColumnProperty, i);
                tbC_TopC01.HorizontalAlignment = HorizontalAlignment.Center;
                stair_TopC01.Children.Add(tbC_TopC01);

            }
        }
        #endregion

        #region  1호기 C_ZONE 단 셋팅 : PrepareLayoutNoScroll_LeftC1()
        /// <summary>
        /// 1호기 양극 단 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_LeftC1()
        {
            stair_LeftC01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_LeftC01.ColumnDefinitions.Count > 0)
            {
                stair_LeftC01.ColumnDefinitions.Clear();
            }
            if (stair_LeftC01.RowDefinitions.Count > 0)
            {
                stair_LeftC01.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(29.5);
            stair_LeftC01.ColumnDefinitions.Add(coldef);

            // Row 값 정의
            RowDefinition rowdef = null;

            //for (int i = 0; i < MAX_ROW_C1; i++)
            //{
            //    rowdef = new RowDefinition();
            //    rowdef.Height = new GridLength(29.5);
            //    stair_LeftC01.RowDefinitions.Add(rowdef);
            //}
            for (int i = 0; i < MAX_ROW_C1; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(55);
                stair_LeftC01.RowDefinitions.Add(rowdef);

                var tbC_LeftC01 = new TextBlock() { Text = MAX_ROW_C1 - i + ObjectDic.Instance.GetObjectName("단"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_LeftC01.SetValue(Grid.RowProperty, i);
                tbC_LeftC01.HorizontalAlignment = HorizontalAlignment.Center;
                stair_LeftC01.Children.Add(tbC_LeftC01);
            }
        }
        #endregion
        
        #region  1호기 A_ZONE RACK 셋팅 : PrepareLayoutNoScroll_A1()
        /// <summary>
        /// 1호기 음극 RACK 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_A1()
        {
            stair_A01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_A01.ColumnDefinitions.Count > 0)
            {
                stair_A01.ColumnDefinitions.Clear();
            }
            if (stair_A01.RowDefinitions.Count > 0)
            {
                stair_A01.RowDefinitions.Clear();
            }

            DataTable dtMaxPosition = MAX_POSITION(sW1ANPW101, "A");
            if (dtMaxPosition.Rows[0]["X_LOCATION"].ToString() == string.Empty || dtMaxPosition.Rows[0]["Y_LOCATION"].ToString() == string.Empty)
                return;

            // X축 값 설정
            MAX_ROW_A1 = Convert.ToInt16(dtMaxPosition.Rows[0]["X_LOCATION"].ToString());
            // Y축 값 설정
            MAX_COL_A1 = Convert.ToInt16(dtMaxPosition.Rows[0]["Y_LOCATION"].ToString());

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_A1; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_A01.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_A1; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(65);
                stair_A01.RowDefinitions.Add(rowdef);
            }
            // BOARDER 추가
            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < MAX_ROW_A1; i++)
            {
                Border border = new Border();
                if (i == MAX_ROW_A1 - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_A1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_A1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_A01.Children.Add(border);
            }
            for (int i = 0; i < MAX_COL_A1; i++)
            {
                Border border = new Border();
                if (i == MAX_COL_A1 - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_A1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_A1);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_A01.Children.Add(border);
            }
        }
        #endregion

        #region  1호기 A_ZONE 연 셋팅 : PrepareLayoutNoScroll_TopA1()
        /// <summary>
        /// 1호기 음극 연 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_TopA1()
        {
            stair_TopA01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_TopA01.ColumnDefinitions.Count > 0)
            {
                stair_TopA01.ColumnDefinitions.Clear();
            }
            if (stair_TopA01.RowDefinitions.Count > 0)
            {
                stair_TopA01.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_A1; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_TopA01.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(15);
            stair_TopA01.RowDefinitions.Add(rowdef);

            for (int i = 0; i < MAX_COL_A1; i++)
            {
                var tbC_TopA01 = new TextBlock() { Text = i + 1 + ObjectDic.Instance.GetObjectName("연"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_TopA01.SetValue(Grid.ColumnProperty, i);
                tbC_TopA01.HorizontalAlignment = HorizontalAlignment.Center;
                stair_TopA01.Children.Add(tbC_TopA01);
            }
        }
        #endregion

        #region  1호기 A_ZONE 단 셋팅 : PrepareLayoutNoScroll_LeftA1()
        /// <summary>
        /// 1호기 음극 단 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_LeftA1()
        {
            stair_LeftA01.Children.Clear();
            // 행/열 전체 삭제
            if (stair_LeftA01.ColumnDefinitions.Count > 0)
            {
                stair_LeftA01.ColumnDefinitions.Clear();
            }
            if (stair_LeftA01.RowDefinitions.Count > 0)
            {
                stair_LeftA01.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(29.5);
            stair_LeftA01.ColumnDefinitions.Add(coldef);

            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_A1; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(55);
                stair_LeftA01.RowDefinitions.Add(rowdef);
            }
            for (int i = 0; i < MAX_ROW_A1; i++)
            {
                var tbC_LeftA01 = new TextBlock() { Text = MAX_ROW_A1 - i + ObjectDic.Instance.GetObjectName("단"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_LeftA01.SetValue(Grid.RowProperty, i);
                tbC_LeftA01.HorizontalAlignment = HorizontalAlignment.Center;
                stair_LeftA01.Children.Add(tbC_LeftA01);
            }
        }
        #endregion
        
        #region  2호기 C_ZONE RACK 셋팅 : PrepareLayoutNoScroll_C2()
        /// <summary>
        /// 2호기 양극 RACK 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_C2()
        {
            stair_C02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_C02.ColumnDefinitions.Count > 0)
            {
                stair_C02.ColumnDefinitions.Clear();
            }
            if (stair_C02.RowDefinitions.Count > 0)
            {
                stair_C02.RowDefinitions.Clear();
            }

            DataTable dtMaxPosition = MAX_POSITION(sW1ANPW102, "C");
            if (dtMaxPosition.Rows[0]["X_LOCATION"].ToString() == string.Empty || dtMaxPosition.Rows[0]["Y_LOCATION"].ToString() == string.Empty)
                return;

            // X축 값 설정
            MAX_ROW_C2 = Convert.ToInt16(dtMaxPosition.Rows[0]["X_LOCATION"].ToString());
            // Y축 값 설정
            MAX_COL_C2 = Convert.ToInt16(dtMaxPosition.Rows[0]["Y_LOCATION"].ToString());

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_C2; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_C02.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_C2; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(65);
                stair_C02.RowDefinitions.Add(rowdef);
            }
            // BOARDER 추가
            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < MAX_ROW_C2; i++)
            {
                Border border = new Border();
                if (i == MAX_ROW_C2 - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_C2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_C2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_C02.Children.Add(border);
            }
            for (int i = 0; i < MAX_COL_C2; i++)
            {
                Border border = new Border();
                if (i == MAX_COL_C2 - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_C2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_C2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_C02.Children.Add(border);
            }
        }
        #endregion

        #region  2호기 C_ZONE 연 셋팅 : PrepareLayoutNoScroll_TopC2()
        /// <summary>
        /// 2호기 양극 연 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_TopC2()
        {
            stair_TopC02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_TopC02.ColumnDefinitions.Count > 0)
            {
                stair_TopC02.ColumnDefinitions.Clear();
            }
            if (stair_TopC02.RowDefinitions.Count > 0)
            {
                stair_TopC02.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_C2; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_TopC02.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(15);
            stair_TopC02.RowDefinitions.Add(rowdef);

            for (int i = 0; i < MAX_COL_C2; i++)
            {
                var tbC_TopC02 = new TextBlock() { Text = MAX_COL_C2 - i + ObjectDic.Instance.GetObjectName("연"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                //tbC_TopC01.SetValue(Grid.RowProperty, 3);
                tbC_TopC02.SetValue(Grid.ColumnProperty, i);
                tbC_TopC02.HorizontalAlignment = HorizontalAlignment.Center;
                stair_TopC02.Children.Add(tbC_TopC02);

            }
        }
        #endregion

        #region  2호기 C_ZONE 단 셋팅 : PrepareLayoutNoScroll_LeftC2()
        /// <summary>
        /// 2호기 양극 단 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_LeftC2()
        {
            stair_LeftC02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_LeftC02.ColumnDefinitions.Count > 0)
            {
                stair_LeftC02.ColumnDefinitions.Clear();
            }
            if (stair_LeftC02.RowDefinitions.Count > 0)
            {
                stair_LeftC02.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(29.5);
            stair_LeftC02.ColumnDefinitions.Add(coldef);

            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_C2; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(55);
                stair_LeftC02.RowDefinitions.Add(rowdef);
            }
            for (int i = 0; i < MAX_ROW_C2; i++)
            {
                var tbC_LeftC02 = new TextBlock() { Text = MAX_ROW_C1 - i + ObjectDic.Instance.GetObjectName("단"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_LeftC02.SetValue(Grid.RowProperty, i);
                tbC_LeftC02.HorizontalAlignment = HorizontalAlignment.Center;
                stair_LeftC02.Children.Add(tbC_LeftC02);
            }
        }
        #endregion

        #region  2호기 A_ZONE RACK 셋팅 : PrepareLayoutNoScroll_A2()
        /// <summary>
        /// 2호기 음극 RACK 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_A2()
        {
            stair_A02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_A02.ColumnDefinitions.Count > 0)
            {
                stair_A02.ColumnDefinitions.Clear();
            }
            if (stair_A02.RowDefinitions.Count > 0)
            {
                stair_A02.RowDefinitions.Clear();
            }

            DataTable dtMaxPosition = MAX_POSITION(sW1ANPW102, "A");
            if (dtMaxPosition.Rows[0]["X_LOCATION"].ToString() == string.Empty || dtMaxPosition.Rows[0]["Y_LOCATION"].ToString() == string.Empty)
                return;

            // X축 값 설정
            MAX_ROW_A2 = Convert.ToInt16(dtMaxPosition.Rows[0]["X_LOCATION"].ToString());
            // Y축 값 설정
            MAX_COL_A2 = Convert.ToInt16(dtMaxPosition.Rows[0]["Y_LOCATION"].ToString());

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_A2; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_A02.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_A2; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(65);
                stair_A02.RowDefinitions.Add(rowdef);
            }
            // BOARDER 추가
            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < MAX_ROW_A2; i++)
            {
                Border border = new Border();
                if (i == MAX_ROW_A2 - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_A2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, MAX_COL_A2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_A02.Children.Add(border);
            }
            for (int i = 0; i < MAX_COL_A2; i++)
            {
                Border border = new Border();
                if (i == MAX_COL_A2 - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_A2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, MAX_ROW_A2);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                stair_A02.Children.Add(border);
            }
        }
        #endregion

        #region  2호기 A_ZONE 연 셋팅 : PrepareLayoutNoScroll_TopA2()
        /// <summary>
        /// 2호기 음극 연 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_TopA2()
        {
            stair_TopA02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_TopA02.ColumnDefinitions.Count > 0)
            {
                stair_TopA02.ColumnDefinitions.Clear();
            }
            if (stair_TopA02.RowDefinitions.Count > 0)
            {
                stair_TopA02.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            for (int i = 0; i < MAX_COL_A2; i++)
            {
                coldef = new ColumnDefinition();
                coldef.Width = new GridLength(45);
                stair_TopA02.ColumnDefinitions.Add(coldef);
            }
            // Row 값 정의
            RowDefinition rowdef = null;
            rowdef = new RowDefinition();
            rowdef.Height = new GridLength(15);
            stair_TopA02.RowDefinitions.Add(rowdef);

            for (int i = 0; i < MAX_COL_A2; i++)
            {
                var tbC_TopA02 = new TextBlock() { Text = MAX_COL_A2 - i + ObjectDic.Instance.GetObjectName("연"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_TopA02.SetValue(Grid.ColumnProperty, i);
                tbC_TopA02.HorizontalAlignment = HorizontalAlignment.Center;
                stair_TopA02.Children.Add(tbC_TopA02);
            }
        }
        #endregion

        #region  2호기 A_ZONE 단 셋팅 : PrepareLayoutNoScroll_LeftA2()
        /// <summary>
        /// 2호기 음극 단 셋팅
        /// </summary>
        private void PrepareLayoutNoScroll_LeftA2()
        {
            stair_LeftA02.Children.Clear();
            // 행/열 전체 삭제
            if (stair_LeftA02.ColumnDefinitions.Count > 0)
            {
                stair_LeftA02.ColumnDefinitions.Clear();
            }
            if (stair_LeftA02.RowDefinitions.Count > 0)
            {
                stair_LeftA02.RowDefinitions.Clear();
            }

            // column 정의
            ColumnDefinition coldef = null;
            coldef = new ColumnDefinition();
            coldef.Width = new GridLength(29.5);
            stair_LeftA02.ColumnDefinitions.Add(coldef);

            // Row 값 정의
            RowDefinition rowdef = null;

            for (int i = 0; i < MAX_ROW_A2; i++)
            {
                rowdef = new RowDefinition();
                rowdef.Height = new GridLength(55);
                stair_LeftA02.RowDefinitions.Add(rowdef);
            }
            for (int i = 0; i < MAX_ROW_A2; i++)
            {
                var tbC_LeftA02 = new TextBlock() { Text = MAX_ROW_A2 - i + ObjectDic.Instance.GetObjectName("단"), Foreground = new SolidColorBrush(Colors.Black), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, FontFamily = new FontFamily("Malgun Gothic"), FontSize = 11, FontWeight = FontWeights.Bold };
                tbC_LeftA02.SetValue(Grid.RowProperty, i);
                tbC_LeftA02.HorizontalAlignment = HorizontalAlignment.Center;
                stair_LeftA02.Children.Add(tbC_LeftA02);
            }
        }
        #endregion

        #endregion

        #region ======== LOT 정보, PORT 정보, 반송정보, 창고정보, 출고대상정보 BIND ==========
        
        #region 1호기 C_ZONE LOT 정보 Bind : DataBind_C1()
        /// <summary>
        /// 1호기 양극 LOT 정보 Bind
        /// </summary>
        private void DataBind_C1(bool button)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = sW1ANPW101;
            dr["ZONE_ID"] = "C";
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);
            //시간체크떄문에
            //Stopwatch sw = new Stopwatch();
            new ClientProxy().ExecuteService("DA_MCS_SEL_LOT_POSITION_LAMI", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {
                try
                {
                    //sw.Start();
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (stair_C01.ColumnDefinitions.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            AssmLamiRack_CheckBox assmRack = new AssmLamiRack_CheckBox();
                            assmRack.Name = result.Rows[i]["RACK_ID"].ToString();
                            assmRack.Check = result.Rows[i]["DIV"].ToString(); //RACK인지 PORT인지
                            assmRack.RackId = result.Rows[i]["RACK_ID"].ToString(); //RACK_ID
                            assmRack.ProjectName = result.Rows[i]["PRJT_NAME"].ToString();  //프로젝트명 
                            assmRack.LotId = result.Rows[i]["LOTID"].ToString();  //LOTID
                            assmRack.JUDG_VALUE = result.Rows[i]["JUDG_VALUE"].ToString(); //QA검사결과
                            assmRack.Wip_Remarks = result.Rows[i]["WIP_REMARKS"].ToString(); //WIP REMAKRKS
                            assmRack.ElapseDay = Convert.ToInt32(result.Rows[i]["ELAPSE"]); //입고경과일
                            assmRack.SkidID = result.Rows[i]["MCS_CST_ID"].ToString(); //PORT여부
                            assmRack.PRDT_CLSS_CODE = result.Rows[i]["ELTR_TYPE_CODE"].ToString(); //극성
                            assmRack.Spcl_Flag = result.Rows[i]["SPCL_FLAG"].ToString();//특별관리
                            assmRack.WipHold = result.Rows[i]["WIPHOLD"].ToString();//HOLD관리
                            assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                            assmRack.Checked += assmRack_Checked; //체크박스 체크
                            Grid.SetRow(assmRack, MAX_ROW_C1 - Convert.ToInt16(result.Rows[i]["X_LOCATION"].ToString()));
                            Grid.SetColumn(assmRack, Convert.ToInt16(result.Rows[i]["Y_LOCATION"].ToString()) - 1);
                            stair_C01.Children.Add(assmRack);
                        }
                        //sw.Stop();
                        //ControlsLibrary.MessageBox.Show(sw.Elapsed.ToString());

                        if(button == false)
                        {
                            //DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                            if (DTOUT.Rows.Count > 0)
                            {
                              for(int i=0; i< DTOUT.Rows.Count; i++)
                                {
                                    //1호기 양극
                                    foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C01))
                                    {
                                        if (control != null)
                                        {
                                            if (control.Name.ToString() == DTOUT.Rows[i]["RACK_ID"].ToString())
                                            {
                                                control.IsChecked = true;
                                                ReCheckLotInf(DTOUT.Rows[i]["LOTID"].ToString());
                                            }
                                        }
                                    }
                                }
                              
                            }
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                catch (Exception)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                }
            });
        }
        #endregion

        #region 1호기 A_ZONE LOT 정보 Bind : DataBind_A1()
        /// <summary>
        /// 1호기 음극 LOT 정보 Bind
        /// </summary>
        private void DataBind_A1(bool button)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = sW1ANPW101;
            dr["ZONE_ID"] = "A";
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LOT_POSITION_LAMI", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (stair_A01.ColumnDefinitions.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        AssmLamiRack_CheckBox assmRack = new AssmLamiRack_CheckBox();
                        //if (result.Rows[i]["DIV"].ToString() == "RACK")
                        //{
                        assmRack.Name = result.Rows[i]["RACK_ID"].ToString();
                        assmRack.RackId = result.Rows[i]["RACK_ID"].ToString(); //RACK_ID
                        assmRack.Check = result.Rows[i]["DIV"].ToString(); //RACK인지 PORT인지
                        assmRack.ProjectName = result.Rows[i]["PRJT_NAME"].ToString();  //프로젝트명 
                        assmRack.LotId = result.Rows[i]["LOTID"].ToString();  //LOTID
                        assmRack.JUDG_VALUE = result.Rows[i]["JUDG_VALUE"].ToString(); //QA검사결과
                        assmRack.Wip_Remarks = result.Rows[i]["WIP_REMARKS"].ToString(); //WIP REMAKRKS
                        assmRack.ElapseDay = Convert.ToInt32(result.Rows[i]["ELAPSE"]); //입고경과일
                        assmRack.SkidID = result.Rows[i]["MCS_CST_ID"].ToString(); //PORT여부
                        assmRack.PRDT_CLSS_CODE = result.Rows[i]["ELTR_TYPE_CODE"].ToString(); //극성
                        assmRack.Spcl_Flag = result.Rows[i]["SPCL_FLAG"].ToString();//특별관리
                        assmRack.WipHold = result.Rows[i]["WIPHOLD"].ToString();//HOLD관리
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        assmRack.Checked += assmRack_Checked; //체크박스 체크

                        Grid.SetRow(assmRack, MAX_ROW_A1 - Convert.ToInt16(result.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, Convert.ToInt16(result.Rows[i]["Y_LOCATION"].ToString()) - 1);
                 
                        stair_A01.Children.Add(assmRack);
                    }
                }
                if (button == false)
                {
                    //DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    if (DTOUT.Rows.Count > 0)
                    {
                        for (int i = 0; i < DTOUT.Rows.Count; i++)
                        {
                            foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A01))
                            {
                                if (control != null)
                                {
                                    if (control.Name.ToString() == DTOUT.Rows[i]["RACK_ID"].ToString())
                                    {
                                        control.IsChecked = true;
                                        ReCheckLotInf(DTOUT.Rows[i]["LOTID"].ToString());
                                    }
                                }
                            }
                        }
                       
                    }
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });

        }
        #endregion

        #region 2호기 C_ZONE LOT 정보 Bind : DataBind_C2()
        /// <summary>
        /// 2호기 양극 LOT 정보 Bind
        /// </summary>
        private void DataBind_C2(bool button)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = sW1ANPW102;
            dr["ZONE_ID"] = "C";
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LOT_POSITION_LAMI", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (stair_C02.ColumnDefinitions.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        AssmLamiRack_CheckBox assmRack = new AssmLamiRack_CheckBox();
                        assmRack.Name = result.Rows[i]["RACK_ID"].ToString();
                        assmRack.RackId = result.Rows[i]["RACK_ID"].ToString(); //RACK_ID
                        assmRack.Check = result.Rows[i]["DIV"].ToString(); //RACK인지 PORT인지
                        assmRack.ProjectName = result.Rows[i]["PRJT_NAME"].ToString();  //프로젝트명 
                        assmRack.LotId = result.Rows[i]["LOTID"].ToString();  //LOTID
                        assmRack.JUDG_VALUE = result.Rows[i]["JUDG_VALUE"].ToString(); //QA검사결과
                        assmRack.Wip_Remarks = result.Rows[i]["WIP_REMARKS"].ToString(); //WIP REMAKRKS
                        assmRack.ElapseDay = Convert.ToInt32(result.Rows[i]["ELAPSE"]); //입고경과일
                        assmRack.SkidID = result.Rows[i]["MCS_CST_ID"].ToString(); //PORT여부
                        assmRack.PRDT_CLSS_CODE = result.Rows[i]["ELTR_TYPE_CODE"].ToString(); //극성
                        assmRack.Spcl_Flag = result.Rows[i]["SPCL_FLAG"].ToString();//특별관리
                        assmRack.WipHold = result.Rows[i]["WIPHOLD"].ToString();//HOLD관리
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        assmRack.Checked += assmRack_Checked; //체크박스 체크
                        Grid.SetRow(assmRack, MAX_ROW_C2 - Convert.ToInt16(result.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, MAX_COL_C2 - (Convert.ToInt16(result.Rows[i]["Y_LOCATION"].ToString())));
                        stair_C02.Children.Add(assmRack);
                    }
                }
                //타이머로 조회시 현상유지
                if (button == false)
                {
                    //DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    if (DTOUT.Rows.Count > 0)
                    {
                        for (int i = 0; i < DTOUT.Rows.Count; i++)
                        {
                            foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C02))
                            {
                                if (control != null)
                                {
                                    if (control.Name.ToString() == DTOUT.Rows[i]["RACK_ID"].ToString())
                                    {
                                        control.IsChecked = true;
                                        ReCheckLotInf(DTOUT.Rows[i]["LOTID"].ToString());
                                    }
                                }
                            }
                        }
                    
                    }
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region 2호기 A_ZONE LOT 정보 Bind : DataBind_A2()
        /// <summary>
        /// 1호기 음극 LOT 정보 Bind
        /// </summary>
        private void DataBind_A2(bool button)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ZONE_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["EQPTID"] = sW1ANPW102;
            dr["ZONE_ID"] = "A";
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LOT_POSITION_LAMI", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (stair_A02.ColumnDefinitions.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        AssmLamiRack_CheckBox assmRack = new AssmLamiRack_CheckBox();
                        assmRack.Name = result.Rows[i]["RACK_ID"].ToString();
                        assmRack.RackId = result.Rows[i]["RACK_ID"].ToString(); //RACK_ID
                        assmRack.Check = result.Rows[i]["DIV"].ToString(); //RACK인지 PORT인지
                        assmRack.ProjectName = result.Rows[i]["PRJT_NAME"].ToString();  //프로젝트명 
                        assmRack.LotId = result.Rows[i]["LOTID"].ToString();  //LOTID
                        assmRack.JUDG_VALUE = result.Rows[i]["JUDG_VALUE"].ToString(); //QA검사결과
                        assmRack.Wip_Remarks = result.Rows[i]["WIP_REMARKS"].ToString(); //WIP REMAKRKS
                        assmRack.ElapseDay = Convert.ToInt32(result.Rows[i]["ELAPSE"]); //입고경과일
                        assmRack.SkidID = result.Rows[i]["MCS_CST_ID"].ToString(); //PORT여부
                        assmRack.PRDT_CLSS_CODE = result.Rows[i]["ELTR_TYPE_CODE"].ToString(); //극성
                        assmRack.Spcl_Flag = result.Rows[i]["SPCL_FLAG"].ToString();//특별관리
                        assmRack.WipHold = result.Rows[i]["WIPHOLD"].ToString();//HOLD관리
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        assmRack.Checked += assmRack_Checked; //체크박스 체크

                        Grid.SetRow(assmRack, MAX_ROW_A2 - Convert.ToInt16(result.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, MAX_COL_A2 - (Convert.ToInt16(result.Rows[i]["Y_LOCATION"].ToString())));
                        stair_A02.Children.Add(assmRack);
                    }
                }
                //타이머로 조회시 현상유지
                if (button == false)
                {
                    //DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                    if (DTOUT.Rows.Count > 0)
                    {
                        for (int i = 0; i < DTOUT.Rows.Count; i++)
                        {
                            foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A02))
                            {
                                if (control != null)
                                {
                                    if (control.Name.ToString() == DTOUT.Rows[i]["RACK_ID"].ToString())
                                    {
                                        control.IsChecked = true;
                                        ReCheckLotInf(DTOUT.Rows[i]["LOTID"].ToString());
                                    }
                                }
                            }
                        }
                     }
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region PORT 정보 셋팅 : PortBind()
        /// <summary>
        /// PORT 정보 셋팅
        /// </summary>
        private void PortBind()
        {
            //Port에 물려있는 LOT 조회
            DataTable dtPortLot = PortLotInfo();

            if (dtPortLot.Rows.Count > 0)
            {
                for (int i = 0; i < dtPortLot.Rows.Count; i++)
                {
                    #region ===========  1호기 A_ZONE PORT =====================
                    //===========  1호기 양극 PORT =====================
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S171")
                    {
                        PORTA6S171.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S171.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S171, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S170")
                    {
                        PORTA6S170.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S170.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S170, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S169")
                    {
                        PORTA6S169.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S169.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S169, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S168")
                    {
                        PORTA6S168.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S168.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S168, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S167")
                    {
                        PORTA6S167.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S167.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S167, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S166")
                    {
                        PORTA6S166.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S166.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S166, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S165")
                    {
                        PORTA6S165.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S165.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S165, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S164")
                    {
                        PORTA6S164.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S164.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S164, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S163")
                    {
                        PORTA6S163.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S163.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S163, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    #endregion

                    #region ===========  1호기 음극 PORT =====================
                    //===========  1호기 음극 PORT =====================
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S152")
                    {
                        PORTA6S152.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S152.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S152, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S151")
                    {
                        PORTA6S151.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S151.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S151, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S150")
                    {
                        PORTA6S150.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S150.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S150, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S149")
                    {
                        PORTA6S149.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S149.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S149, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S148")
                    {
                        PORTA6S148.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S148.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S148, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S147")
                    {
                        PORTA6S147.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S147.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S147, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    #endregion

                    #region ===========  2호기 양극 PORT =====================
                    //===========  2호기 양극 PORT =====================

                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S146")
                    {
                        PORTA6S146.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S146.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S146, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S145")
                    {
                        PORTA6S145.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S145.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S145, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S144")
                    {
                        PORTA6S144.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S144.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S144, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S143")
                    {
                        PORTA6S143.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S143.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S143, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S142")
                    {
                        PORTA6S142.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S142.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S142, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S141")
                    {
                        PORTA6S141.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S141.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S141, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    #endregion

                    #region ===========  2호기 음극 PORT =====================
                    //===========  2호기 음극 PORT =====================
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S162")
                    {
                        PORTA6S162.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S162.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S162, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S161")
                    {
                        PORTA6S161.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S161.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S161, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S160")
                    {
                        PORTA6S160.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S160.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S160, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S159")
                    {
                        PORTA6S159.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S159.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S159, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S158")
                    {
                        PORTA6S158.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S158.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S158, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S157")
                    {
                        PORTA6S157.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S157.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S157, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S156")
                    {
                        PORTA6S156.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S156.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S156, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S155")
                    {
                        PORTA6S155.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S155.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S155, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S154")
                    {
                        PORTA6S154.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S154.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S154, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }
                    if (dtPortLot.Rows[i]["PORT_ID"].ToString() == "A6S153")
                    {
                        PORTA6S153.Content = dtPortLot.Rows[i]["JUDG_VALUE"].ToString() != string.Empty ? dtPortLot.Rows[i]["JUDG_VALUE"].ToString() : dtPortLot.Rows[i]["INOUT_TYPE_CODE"].ToString();
                        PORTA6S153.Tag = dtPortLot.Rows[i]["PORT_ID"].ToString() + "_" + dtPortLot.Rows[i]["LOTID"].ToString();
                        ButtonPort_Color(PORTA6S153, dtPortLot.Rows[i]["WIP_REMARKS"].ToString(), dtPortLot.Rows[i]["PRJT_NAME"].ToString());
                    }

                    #endregion

                }

            }
        }

        #endregion

        #region PORT별 LOT 정보 : PortLotInfo()
        /// <summary>
        /// PORT별 LOT 정보
        /// </summary>
        private DataTable PortLotInfo()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PORTID", typeof(string));
            #region ===========  1호기 A_ZONE PORT =====================
            // 1호기 양극 PORT
            DataRow dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S171";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S170";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S169";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S168";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S167";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S166";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S165";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S164";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S163";
            dtRqst.Rows.Add(dr);

            #endregion

            #region ===========  1호기 C_ZONE PORT =====================
            // 1호기 음극 PORT
            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S152";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S151";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S150";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S149";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S148";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S147";
            dtRqst.Rows.Add(dr);



            #endregion

            #region ===========  2호기 C_ZONE PORT =====================
            // 2호기 양극 PORT
            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S146";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S145";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S144";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S143";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S142";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S141";
            dtRqst.Rows.Add(dr);

            #endregion

            #region ===========  2호기 A_ZONE PORT =====================
            // 2호기 음극 PORT
            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S162";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S161";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S160";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S159";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S158";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S157";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S156";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S155";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S154";
            dtRqst.Rows.Add(dr);

            dr = dtRqst.NewRow();
            dr["PORTID"] = "A6S153";
            dtRqst.Rows.Add(dr);

            #endregion

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LOT_PORT", "INDATA", "OUTDATA", dtRqst);
            if (dtResult.Rows.Count > 0)
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["LOTID"] != null)
                    {
                        dtResult.Rows[i]["LOTID"] = dtResult.Rows[i]["LOTID"].ToString().Replace(",", "\n");
                    }
                }
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["JUDG_VALUE"] != null)
                    {
                        dtResult.Rows[i]["JUDG_VALUE"] = dtResult.Rows[i]["JUDG_VALUE"].ToString().Replace(",", "\n");
                    }
                }
            }
            return dtResult;
        }

        //return 
        #endregion

        #region 창고 정보 BIND : WareHouseBind()
        /// <summary>
        /// 창고정보 Bind
        /// </summary>
        private void WareHouseBind()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            if (cboWhInfo.SelectedValue.ToString() == string.Empty)
            {
                dr["PRODID"] = null;
            }
            else
            {
                dr["PRODID"] = cboWhInfo.SelectedValue.ToString();
            }
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_WAREHOUSE_INFO_LAMI", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                Util.GridSetData(dgWhereHose, result, FrameOperation, true);
                loadingIndicator.Visibility = Visibility.Collapsed;

            });
        }
        #endregion

        #region 입고/출고 예약 정보 Bind : OutputBind()
        /// <summary>
        /// 입고/출고 예약 정보 Bind
        /// </summary>
        private void OutputBind()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            string BizName = string.Empty;
            if (cboRackState.SelectedValue.ToString() == "RCV_RESERVE")
            {
                BizName = "DA_MCS_SEL_LOGIS_LAMI_IN";
            }
            else if (cboRackState.SelectedValue.ToString() == "ISS_RESERVE")
            {
                BizName = "DA_MCS_SEL_LOGIS_LAMI_OUT";
            }
            else
            {
                BizName = "DA_MCS_SEL_LOGIS_LAMI_OUT_ALL";
            }


            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("LOGIS_CMD_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));


            DataRow dr = dtRqst.NewRow();
            dr["PRODID"] = txtProdID_OutPut.Text == string.Empty ? null : txtProdID_OutPut.Text;
            dr["LOTID"] = txtLot_OutPut.Text == string.Empty ? null : txtLot_OutPut.Text;
            dr["LOGIS_CMD_ID"] = txtReqID_OutPut.Text == string.Empty ? null : txtReqID_OutPut.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService(BizName, "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgOutputInfo, result, FrameOperation, false);
                }
                else
                {
                    Util.gridClear(dgOutputInfo);
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });

        }
        #endregion

        #region CNV 정보 셋팅 : CNVBind()
        /// <summary>
        /// 컨베어 방향 BIND
        /// </summary>
        private void CNVBind()
        {
            //초기화
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LANG", typeof(string));
            dtRqst.Columns.Add("MAIN_CHECK", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["LANG"] = LoginInfo.LANGID;
            dr["MAIN_CHECK"] = "Y";
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_CNV_DIRECTION", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        if (result.Rows[i]["EQPTID"].ToString() == sW1ACNV101)
                        {
                            if (result.Rows[i]["EQPT_WRK_MODE"].ToString() == "F")
                            {
                                imgIN.Visibility = Visibility.Visible;
                                imgIN_B.Visibility = Visibility.Collapsed;
                            }
                            else if (result.Rows[i]["EQPT_WRK_MODE"].ToString() == "B")
                            {
                                imgIN.Visibility = Visibility.Collapsed;
                                imgIN_B.Visibility = Visibility.Visible;

                            }
                       }
                        if (result.Rows[i]["EQPTID"].ToString() == sW1ACNV102)
                        {
                            if (result.Rows[i]["EQPT_WRK_MODE"].ToString() == "F")
                            {
                                imgOUT.Visibility = Visibility.Collapsed;
                                imgOUT_B.Visibility = Visibility.Visible;

                            }
                            else if (result.Rows[i]["EQPT_WRK_MODE"].ToString() == "B")
                            {
                                imgOUT.Visibility = Visibility.Visible;
                                imgOUT_B.Visibility = Visibility.Collapsed;
                            }
                          
                        }
                    }
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            });
        }

        #endregion

        #endregion

        #region  LOT을 조회조건으로 Rack 조회 : DataBind_S()
        /// <summary>
        /// LOT을 조회조건으로 Rack 조회
        /// </summary>
        private DataTable DataBind_S(string LotID)
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LOTID"] = LotID;
            if (rdo_OneEqpt.IsChecked == true)
            {
                dr["EQPTID"] = sW1ANPW101;
            }
            else if(rdo_TwoEqpt.IsChecked == true)
            {
                dr["EQPTID"] =sW1ANPW102;
            }
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LOT_POSITION_LAMI", "INDATA", "OUTDATA", dtRqst);
            if (dtResult.Rows.Count > 0)
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["LOTID"] != null)
                    {
                        dtResult.Rows[i]["LOTID"] = dtResult.Rows[i]["LOTID"].ToString().Replace(",", "\n");
                    }
                }
            }
        


            return dtResult;

        }

        #endregion

        #region LOT을 조회조건으로 RACK의 체크박스 선택 : SelectLOT()
        /// <summary>
        /// LOT을 조회조건으로 재조회
        /// </summary>
        private void SelectLOT()
        {
            if (txtLotS.Text != string.Empty)
            {
               //LOT으로 Rack정보 및 LOT정보 조회
                DataTable dtRackLot = DataBind_S(txtLotS.Text);
                #region Rack 정보
                //Rack 정보
                if (dtRackLot.Rows.Count > 0)
                {
                    //1호기 양극
                    foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C01))
                    {
                        if (control != null)
                        {
                            if (control.Name.ToString() == dtRackLot.Rows[0]["RACK_ID"].ToString())
                            {
                                //Rack에 체크박스 체크
                                control.IsChecked = true;
                                OutPutLotBind(control);
                                //확장
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
                            }
                        }
                    }
                    //1호기 음극
                    foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A01))
                    {
                        if (control != null)
                        {
                            if (control.Name.ToString() == dtRackLot.Rows[0]["RACK_ID"].ToString())
                            {
                                //Rack에 체크박스 체크
                                control.IsChecked = true;
                                OutPutLotBind(control);
                                //확장
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
                            }
                        }
                    }
                    //2호기 양극
                    foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_C02))
                    {
                        if (control != null)
                        {
                            if (control.Name.ToString() == dtRackLot.Rows[0]["RACK_ID"].ToString())
                            {
                                //Rack에 체크박스 체크
                                control.IsChecked = true;
                                OutPutLotBind(control);
                                //확장
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
                            }

                        }
                    }
                    //2호기 음극
                    foreach (AssmLamiRack_CheckBox control in Util.FindVisualChildren<AssmLamiRack_CheckBox>(stair_A02))
                    {
                        if (control != null)
                        {
                            if (control.Name.ToString() == dtRackLot.Rows[0]["RACK_ID"].ToString())
                            {
                                //Rack에 체크박스 체크
                                control.IsChecked = true;
                                OutPutLotBind(control);
                                //확장
                                Monitoring.RowDefinitions[0].Height = new GridLength(1.7, GridUnitType.Star);
                                Monitoring.RowDefinitions[1].Height = new GridLength(8);
                                Monitoring.RowDefinitions[2].Height = new GridLength(6.0, GridUnitType.Star);
                                Monitoring.RowDefinitions[3].Height = new GridLength(8);
                                Monitoring.RowDefinitions[4].Height = new GridLength(2.0, GridUnitType.Star);
                            }

                        }
                    }
                    txtLotS.Text = string.Empty;
                    txtLotS.Focus();
                }
                else
                {
                    if(rdo_OneEqpt.IsChecked == true)
                    {
                        // Noteched Pancake 창고 x 호기에 존재하지 않는 LOT입니다
                        Util.MessageValidation("SFU5091", ObjectDic.Instance.GetObjectName("1"));
                    }
                    else
                    {
                        // Noteched Pancake 창고 x 호기에 존재하지 않는 LOT입니다
                        Util.MessageValidation("SFU5091", ObjectDic.Instance.GetObjectName("2"));
                    }
                   
                   

                    txtLotS.Text = string.Empty;
                    txtLotS.Focus();
                }
                #endregion

              
            }

        }

        #endregion

        #region LOT을 조회조건으로 출고대상 Pacncake 스프레드 Bind  : OutPutLotBind()

        /// <summary>
        /// LOT으로 조회시 LOT정보 출고대상 Pacncake 스프레드
        /// </summary>
        private void OutPutLotBind(AssmLamiRack_CheckBox Control)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("RACKID", typeof(string));
                dtRqst.Columns.Add("PORTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["RACKID"] = Control.Name.ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우
                        {

                            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                            {
                                if (DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "RACK_ID").ToString() == Control.Name)
                                {
                                    return;
                                }
                            }
                            DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                            DataRow newRow = null;
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                newRow = dtSource.NewRow();
                                newRow["RACK_ID_2"] = result.Rows[i]["RACK_ID_2"];
                                newRow["CHK"] = 0;
                                newRow["RACK_ID"] = result.Rows[i]["RACK_ID"];
                                newRow["PRJT_NAME"] = result.Rows[i]["PRJT_NAME"];
                                newRow["PRODID"] = result.Rows[i]["PRODID"];
                                newRow["WH_RCV_DTTM"] = result.Rows[i]["WH_RCV_DTTM"];
                                newRow["WIPDTTM_ED"] = result.Rows[i]["WIPDTTM_ED"];
                                newRow["LOTID"] = result.Rows[i]["LOTID"];
                                newRow["WIP_QTY"] = result.Rows[i]["WIP_QTY"];
                                newRow["JUDG_VALUE"] = result.Rows[i]["JUDG_VALUE"];
                                newRow["VLD_DATE"] = result.Rows[i]["VLD_DATE"];
                                newRow["SPCL_FLAG"] = result.Rows[i]["SPCL_FLAG"];
                                newRow["TMP1"] = result.Rows[i]["TMP1"];
                                newRow["SPCL_RSNCODE"] = result.Rows[i]["SPCL_RSNCODE"];
                                newRow["TMP2"] = result.Rows[i]["TMP2"];
                                newRow["WIP_REMARKS"] = result.Rows[i]["WIP_REMARKS"];
                                newRow["TMP3"] = result.Rows[i]["TMP3"];
                                newRow["WIPHOLD"] = result.Rows[i]["WIPHOLD"];
                                newRow["RACK_STAT_CODE"] = result.Rows[i]["RACK_STAT_CODE"];
                                newRow["EQPTID"] = result.Rows[i]["EQPTID"];
                                newRow["EQPTNAME"] = result.Rows[i]["EQPTNAME"];
                                newRow["X_PSTN"] = result.Rows[i]["X_PSTN"];
                                newRow["Y_PSTN"] = result.Rows[i]["Y_PSTN"];
                                newRow["Z_PSTN"] = result.Rows[i]["Z_PSTN"];
                                newRow["ID_SEQ"] = result.Rows[i]["ID_SEQ"];
                                newRow["WOID"] = result.Rows[i]["WOID"];
                                newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                                newRow["MODLID"] = result.Rows[i]["MODLID"];
                                newRow["MCS_CST_ID"] = result.Rows[i]["MCS_CST_ID"];
                                newRow["PRODDESC"] = result.Rows[i]["PRODDESC"];
                                newRow["SAVE_YN"] = result.Rows[i]["SAVE_YN"];
                                newRow["EQSGID"] = result.Rows[i]["EQSGID"];
                                newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                                newRow["TMP4"] = result.Rows[i]["TMP4"];
                                newRow["TMP5"] = result.Rows[i]["TMP5"];
                                newRow["TMP6"] = result.Rows[i]["TMP6"];
                                newRow["TMP7"] = result.Rows[i]["TMP7"];
                                newRow["TMP8"] = result.Rows[i]["TMP8"];
                                newRow["TMP9"] = result.Rows[i]["TMP9"];
                                dtSource.Rows.Add(newRow);
                            }
                            Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);
                        }
                        else
                        {
                            Util.GridSetData(dgLotInfo, result, FrameOperation, false);
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 수동출고예약 Valldation :  ValidationReturn()
        /// <summary>
        /// 수동출고예약 Valldation
        /// </summary>
        /// <returns></returns>
        private bool ValidationReturn()
        {

            if (dgLotInfo.Rows.Count == 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고할 RACK"));
                return false;
            }

            if (cboOutPort.SelectedIndex <= 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고포트"));
                return false;

            }
            int checkcount = 0;
            foreach (DataRow row in ((System.Data.DataView)dgLotInfo.ItemsSource).Table.Rows)
            {
                if (row["CHK"].ToString() == "1")
                { checkcount++; }
            }

            if (checkcount < 1)
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고대상 Pancake정보"));
                return false;
            }

            return true;
        }

        #endregion

        #region 타이머로 재조회시 기존 선택된 LOT 정보 갱신해서 출고대상 Pancake 스프레드 Bind : ReCheckLotInf()
        /// <summary>
        /// 타이머로 재조회시 선택된 LOT 정보 조회
        /// </summary>
        private void ReCheckLotInf(string _lotid)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _lotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_LAMI_INPUT_LOT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        if (dgLotInfo.Rows.Count > 0) //기존값이 있을경우
                        {

                            try
                            {
                                DataTable dtSource = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                                DataRow newRow = null;
                                for (int i = 0; i < result.Rows.Count; i++)
                                {
                                    newRow = dtSource.NewRow();
                                    newRow["RACK_ID_2"] = result.Rows[i]["RACK_ID_2"];
                                    newRow["CHK"] = 0;
                                    newRow["RACK_ID"] = result.Rows[i]["RACK_ID"];
                                    newRow["PRJT_NAME"] = result.Rows[i]["PRJT_NAME"];
                                    newRow["PRODID"] = result.Rows[i]["PRODID"];
                                    newRow["WH_RCV_DTTM"] = result.Rows[i]["WH_RCV_DTTM"];
                                    newRow["WIPDTTM_ED"] = result.Rows[i]["WIPDTTM_ED"];
                                    newRow["LOTID"] = result.Rows[i]["LOTID"];
                                    newRow["WIP_QTY"] = result.Rows[i]["WIP_QTY"];
                                    newRow["JUDG_VALUE"] = result.Rows[i]["JUDG_VALUE"];
                                    newRow["VLD_DATE"] = result.Rows[i]["VLD_DATE"];
                                    newRow["SPCL_FLAG"] = result.Rows[i]["SPCL_FLAG"];
                                    newRow["TMP1"] = result.Rows[i]["TMP1"];
                                    newRow["SPCL_RSNCODE"] = result.Rows[i]["SPCL_RSNCODE"];
                                    newRow["TMP2"] = result.Rows[i]["TMP2"];
                                    newRow["WIP_REMARKS"] = result.Rows[i]["WIP_REMARKS"];
                                    newRow["TMP3"] = result.Rows[i]["TMP3"];
                                    newRow["WIPHOLD"] = result.Rows[i]["WIPHOLD"];
                                    newRow["RACK_STAT_CODE"] = result.Rows[i]["RACK_STAT_CODE"];
                                    newRow["EQPTID"] = result.Rows[i]["EQPTID"];
                                    newRow["EQPTNAME"] = result.Rows[i]["EQPTNAME"];
                                    newRow["X_PSTN"] = result.Rows[i]["X_PSTN"];
                                    newRow["Y_PSTN"] = result.Rows[i]["Y_PSTN"];
                                    newRow["Z_PSTN"] = result.Rows[i]["Z_PSTN"];
                                    newRow["ID_SEQ"] = result.Rows[i]["ID_SEQ"];
                                    newRow["WOID"] = result.Rows[i]["WOID"];
                                    newRow["PRODNAME"] = result.Rows[i]["PRODNAME"];
                                    newRow["MODLID"] = result.Rows[i]["MODLID"];
                                    newRow["MCS_CST_ID"] = result.Rows[i]["MCS_CST_ID"];
                                    newRow["PRODDESC"] = result.Rows[i]["PRODDESC"];
                                    newRow["SAVE_YN"] = result.Rows[i]["SAVE_YN"];
                                    newRow["EQSGID"] = result.Rows[i]["EQSGID"];
                                    newRow["EQSGNAME"] = result.Rows[i]["EQSGNAME"];
                                    newRow["TMP4"] = result.Rows[i]["TMP4"];
                                    newRow["TMP5"] = result.Rows[i]["TMP5"];
                                    newRow["TMP6"] = result.Rows[i]["TMP6"];
                                    newRow["TMP7"] = result.Rows[i]["TMP7"];
                                    newRow["TMP8"] = result.Rows[i]["TMP8"];
                                    newRow["TMP9"] = result.Rows[i]["TMP9"];
                                    dtSource.Rows.Add(newRow);
                                }
                                Util.GridSetData(dgLotInfo, dtSource, FrameOperation, false);

                            }
                            catch (Exception ex1)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex1), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }

                        }
                        else
                        {
                            Util.GridSetData(dgLotInfo, result, FrameOperation, false);
                        }
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Rack 하단 PORT 배경색 설정 : ButtonPort_Color()
        /// <summary>
        /// Rack 하단 PORT 배경색 설정
        /// </summary>
        private void ButtonPort_Color(Button Button, string remark, string project)
        {
            if (remark == "Y")
            {
                Button.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC3D69B"));
            }
            else
            {
                Button.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD99694"));
                this.Foreground = new SolidColorBrush(Colors.Black);

                if (project == "T" || project == "U" || project == "F")
                {
                    ColorAnimation da = new ColorAnimation();

                    if (remark == "Y")
                    {
                        da.From = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC3D69B");
                    }
                    else
                    {
                        da.From = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD99694");
                    }

                    da.To = Colors.White;
                    da.Duration = new Duration(TimeSpan.FromSeconds(1));
                    da.AutoReverse = true;
                    da.RepeatBehavior = RepeatBehavior.Forever;
                    Button.Background.BeginAnimation(SolidColorBrush.ColorProperty, da);
                }
                else
                {
                    try
                    {
                        Button.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion

        #endregion

       
    }

}
