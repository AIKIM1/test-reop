/*************************************************************************************
 Created Date : 2018.11.06
      Creator : 
   Decription : MEB노칭대기창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.06  오화백 : Initial Created.
  2018.06.18  오화백  새로운 요청으로 전체 리뉴얼
  2020.05.18  김동일K [C20191120-000373] 재고현황 정보에 음/양극 합계 컬럼 추가 및 불필요 하드코딩 제거, 글자색 변경 오류 수정.
 
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

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        // 모니터링용 타이머 관련
        private System.Windows.Threading.DispatcherTimer monitorTimer = new System.Windows.Threading.DispatcherTimer();
        private string refeshAfter = "{0}";
        private bool bSetAutoSelTime = false;
     


        public MCS001_002()
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
            //listAuth.Add(btnReturn);
            //listAuth.Add(btnPortStat);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            InitRemakr();
            InitCombo();
            Refresh(true);
            TimerSetting();
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

            // QA Combo
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQGRID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQGRID"] = "PCB";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PRJT_FOR_MCS_COMBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboQA.DisplayMemberPath = "CBO_NAME";
            cboQA.SelectedValuePath = "CBO_CODE";
            cboQA.ItemsSource = dtResult.Copy().AsDataView();
            cboQA.SelectedIndex = 0;
        }
        private void InitRemakr()
        {
            //Remark
            cboRemark.Items.Clear();

            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem();
            cbItemTitle.Content = ObjectDic.Instance.GetObjectName("범례");
            cboRemark.Items.Add(cbItemTitle);

            // DataTable
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("KEYNAME", typeof(string));
            IndataTable.Columns.Add("KEYVALUE", typeof(string));


            DataRow Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = ObjectDic.Instance.GetObjectName("노칭대기");
            Indata["KEYVALUE"] = "#FFFFFFFF";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = ObjectDic.Instance.GetObjectName("검사대기");
            Indata["KEYVALUE"] = "#FFF000";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = ObjectDic.Instance.GetObjectName("검사중");
            Indata["KEYVALUE"] = "#e0c24c";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = ObjectDic.Instance.GetObjectName("특별관리");
            Indata["KEYVALUE"] = "#000000";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = "HOLD";
            Indata["KEYVALUE"] = "#fb0000";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = "TERM";
            Indata["KEYVALUE"] = "#fb0000";
            IndataTable.Rows.Add(Indata);

            Indata = IndataTable.NewRow();
            Indata["KEYNAME"] = ObjectDic.Instance.GetObjectName("포트상태");
            Indata["KEYVALUE"] = "#FFFFFFFF";
            IndataTable.Rows.Add(Indata);


            foreach (DataRow row in IndataTable.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem();

                if (row["KEYNAME"].ToString() == ObjectDic.Instance.GetObjectName("특별관리"))
                {
                    cbItem1.Content = row["KEYNAME"].ToString();
                    cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;
                    cbItem1.Foreground = new BrushConverter().ConvertFromString("#FFFFFFFF") as SolidColorBrush;
                    cbItem1.FontWeight = FontWeights.Bold;
                }
                else if (row["KEYNAME"].ToString() == "HOLD")
                {
                    cbItem1.Content = row["KEYNAME"].ToString();
                    cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;
                    cbItem1.Foreground = new BrushConverter().ConvertFromString("#FFFFFFFF") as SolidColorBrush;
                    cbItem1.FontWeight = FontWeights.Bold;
                }
                else if (row["KEYNAME"].ToString() == "TERM")
                {
                    cbItem1.Content = row["KEYNAME"].ToString();
                    cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;
                    cbItem1.Foreground = new BrushConverter().ConvertFromString("#FFF000") as SolidColorBrush;
                    cbItem1.FontWeight = FontWeights.Bold;
                }
                else if (row["KEYNAME"].ToString() == ObjectDic.Instance.GetObjectName("포트상태"))
                {
                    cbItem1.Content = "UR,Y";
                    cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;
                    cbItem1.Foreground = new BrushConverter().ConvertFromString("#fb0000") as SolidColorBrush;
                    cbItem1.FontWeight = FontWeights.Bold;
                }
                else
                {
                    cbItem1.Content = row["KEYNAME"].ToString();
                    cbItem1.Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush;
                    cbItem1.FontWeight = FontWeights.Bold;
                }


                cboRemark.Items.Add(cbItem1);
            }


            cboRemark.SelectedIndex = 0;

            //cboColor.Height = 300;
            cboRemark.DropDownHeight = 400;
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
            Refresh(true);
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region  사용자 컨트롤 클릭 : assmRack_DoubleClick(), popupLotInfo_Closed()
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
                MEBRack assmRack = sender as MEBRack;
                if (assmRack.Name.ToString() == string.Empty)
                {
                    // %1(을)를 선택하세요.
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("RACK혹은PORT"));
                    return;
                }

                MCS001_004_PORT_INFO popupLotInfo = new MCS001_004_PORT_INFO();
                popupLotInfo.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = assmRack.Name;

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
            MCS001_004_PORT_INFO popupLotInfo = sender as MCS001_004_PORT_INFO;
            if (popupLotInfo.DialogResult == MessageBoxResult.OK)
            {
                Refresh(false);
            }
            this.grdMain.Children.Remove(popupLotInfo);
            loadingIndicator.Visibility = Visibility.Collapsed;
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
            MEB_Noching.RowDefinitions[2].Height = new GridLength(8, GridUnitType.Star);
            MEB_Noching.RowDefinitions[4].Height = new GridLength(2, GridUnitType.Star);
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
            MEB_Noching.RowDefinitions[2].Height = new GridLength(10.0, GridUnitType.Star);
            MEB_Noching.RowDefinitions[4].Height = new GridLength(34);

        }
        #endregion

        #region MEB 노칭대기 창고 입출고 이력조회 : btnInputOutputHist_Click()
        /// <summary>
        /// MEB 노칭대기 창고 입출고 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputOutputHist_Click(object sender, RoutedEventArgs e)
        {
            // MEB 노칭대기 창고 입출고 이력조회
            loadingIndicator.Visibility = Visibility.Visible;

            object[] parameters = new object[1];
            parameters[0] = string.Empty;
            this.FrameOperation.OpenMenu("SFU010180050", true, parameters);

            loadingIndicator.Visibility = Visibility.Collapsed;
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

        #region QA검사(콤보박스) : cboQASelectedIndexChanged()
        private void cboQASelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            ReloadQA();
        }

        #endregion
     
        #region 프로젝트ID로 재공정보 조회 : txtPjt_PreviewKeyDown()
        private void txtPjt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReloadQA();
            }

        }
        #endregion

        #region 수동출고가능리스트 : btnOutPutList_Click(), popupOutputList_Closed()
        /// <summary>
        /// 수동출고가능리스트 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutPutList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_002_OUTPUT_LIST popupOutputList = new MCS001_002_OUTPUT_LIST();
                popupOutputList.FrameOperation = this.FrameOperation;
                popupOutputList.Closed += new EventHandler(popupOutputList_Closed);
                grdMain.Children.Add(popupOutputList);
                popupOutputList.BringToFront();

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
        /// 수동출고가능리스트 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupOutputList_Closed(object sender, EventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            MCS001_002_OUTPUT_LIST popupOutputList = sender as MCS001_002_OUTPUT_LIST;
            if (popupOutputList != null && popupOutputList.IsUpdated)
            {

                this.grdMain.Children.Remove(popupOutputList);
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                Refresh(false);
            }
            else
            {
                this.grdMain.Children.Remove(popupOutputList);
            }
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 입고LOT 조회 :btnInputLott_Click()
        /// <summary>
        /// 입고 LOT 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MCS001_002_INPUT_LOT popupInputLot = new MCS001_002_INPUT_LOT();
                popupInputLot.FrameOperation = this.FrameOperation;
                object[] parameters = new object[4];
                parameters[0] = string.Empty; //프로젝트명
                parameters[1] = string.Empty; //극성
                parameters[2] = string.Empty; 
                parameters[3] = "PCB"; //설비그룹
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
        /// 입고 LOT  조회 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupInputLot_Closed(object sender, EventArgs e)
        {
            MCS001_002_INPUT_LOT popupInputLot = sender as MCS001_002_INPUT_LOT;
            if (popupInputLot.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popupInputLot);
        }

        #endregion

        #region 재공정보조회(스프레드 클릭시 입고 LOT 팝업 호출) : dgQA_MouseDoubleClick()

        /// <summary>
        /// 입고 LOT 팝업
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
                    if (cell.Column.Name == "PRJT_NAME" || cell.Column.Name == "ELTR_TYPE_CODE")
                        return;
                    if (cell.Column.Name == "CC" || cell.Column.Name == "CC_WIP_QTY")
                    {
                        POLARITY = "C";
                    }
                    else if (cell.Column.Name == "AC" || cell.Column.Name == "AC_WIP_QTY")
                    {
                        POLARITY = "A";
                    }
                }
                MCS001_002_INPUT_LOT popupInputLot = new MCS001_002_INPUT_LOT();
                popupInputLot.FrameOperation = this.FrameOperation;
                object[] parameters = new object[4];
                parameters[0] =  Util.NVC(DataTableConverter.GetValue(dgQA.Rows[cell.Row.Index].DataItem, "PRJT_NAME")); // 프로젝트명
                parameters[1] = POLARITY; //극성
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgQA.Rows[cell.Row.Index].DataItem, "ELTR_TYPE_CODE")) == "Total" ? string.Empty : Util.NVC(DataTableConverter.GetValue(dgQA.Rows[cell.Row.Index].DataItem, "CODE"));
                parameters[3] = "PCB";
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

        #region 재공정보조회(글자색) : dgWhereHose_LoadedCellPresenter()
        /// <summary>
        /// 글자색
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
                    if (e.Cell.Column.Name.Equals("CC") || e.Cell.Column.Name.Equals("AC") || e.Cell.Column.Name.Equals("ALL_SUM") || e.Cell.Column.Name.Equals("WIP_QTY") || e.Cell.Column.Name.Equals("CC_WIP_QTY") || e.Cell.Column.Name.Equals("AC_WIP_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }

                }

            }));
        }

        #endregion


        #endregion

        #region Method

        #region 자재창고 보관버퍼 Bind : MaterialsDataBind()
        /// <summary>
        /// 자재창고 보관버퍼 Bind
        /// </summary>
        private void MaterialsDataBind()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQGRID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQGRID"] = "PCB";
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = "W1APCB101";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LOT_MEB_POSITION", "INDATA", "OUTDATA", dtRqst);

            grdMaterials.Children.Clear();

            if (grdMaterials.ColumnDefinitions.Count > 0)
            {
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (dtRslt.Rows[i]["X_LOCATION"].ToString() != string.Empty || dtRslt.Rows[i]["Y_LOCATION"].ToString() != string.Empty)
                    {
                        MEBRack assmRack = new MEBRack();
                        assmRack.Name = dtRslt.Rows[i]["PORT_ID"].ToString();

                        //assmRack.ButtonUsControl.Click += ButtonUsControl_Click;
                        assmRack.PORT_ID = dtRslt.Rows[i]["PORT_ID"].ToString();
                        assmRack.LotId = dtRslt.Rows[i]["LOTID"].ToString();
                        assmRack.ProdId = dtRslt.Rows[i]["PRODID"].ToString();
                        assmRack.ProcId = dtRslt.Rows[i]["PROCID"].ToString();
                        assmRack.Wipstate = dtRslt.Rows[i]["WIPSTAT"].ToString();
                        assmRack.Wiphold = dtRslt.Rows[i]["WIPHOLD"].ToString();
                        assmRack.Spcl_Flag = dtRslt.Rows[i]["SPCL_FLAG"].ToString();
                        assmRack.Mtrlexistflag = dtRslt.Rows[i]["MTRL_EXIST_FLAG"].ToString();
                        assmRack.PortStat = dtRslt.Rows[i]["PORT_STAT_CODE"].ToString();
                        assmRack.QA = dtRslt.Rows[i]["QA"].ToString();
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        Grid.SetRow(assmRack, Convert.ToInt16(dtRslt.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, Convert.ToInt16(dtRslt.Rows[i]["Y_LOCATION"].ToString()));


                        grdMaterials.Children.Add(assmRack);
                    }

                }

                //PortBind();
            }
        }
        #endregion

        #region 양극 데이터 Bind : CathodDataBind()
        /// <summary>
        /// 양극 데이터 Bind
        /// </summary>
        private void CathodDataBind()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQGRID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQGRID"] = "PCB";
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = "W1APCB102";
            dr["ELTR_TYPE_CODE"] = "C";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LOT_MEB_POSITION", "INDATA", "OUTDATA", dtRqst);

            grdCathode.Children.Clear();

            if (grdCathode.ColumnDefinitions.Count > 0)
            {
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (dtRslt.Rows[i]["X_LOCATION"].ToString() != string.Empty || dtRslt.Rows[i]["Y_LOCATION"].ToString() != string.Empty)
                    {
                        MEBRack assmRack = new MEBRack();
                        assmRack.Name = dtRslt.Rows[i]["PORT_ID"].ToString();

                        //assmRack.ButtonUsControl.Click += ButtonUsControl_Click;
                        assmRack.PORT_ID = dtRslt.Rows[i]["PORT_ID"].ToString();
                        assmRack.LotId = dtRslt.Rows[i]["LOTID"].ToString();
                        assmRack.ProdId = dtRslt.Rows[i]["PRODID"].ToString();
                        assmRack.ProcId = dtRslt.Rows[i]["PROCID"].ToString();
                        assmRack.Wipstate = dtRslt.Rows[i]["WIPSTAT"].ToString();
                        assmRack.Wiphold = dtRslt.Rows[i]["WIPHOLD"].ToString();
                        assmRack.Spcl_Flag = dtRslt.Rows[i]["SPCL_FLAG"].ToString();
                        assmRack.Mtrlexistflag = dtRslt.Rows[i]["MTRL_EXIST_FLAG"].ToString();
                        assmRack.PortStat = dtRslt.Rows[i]["PORT_STAT_CODE"].ToString();
                        assmRack.QA = dtRslt.Rows[i]["QA"].ToString();
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        Grid.SetRow(assmRack, Convert.ToInt16(dtRslt.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, Convert.ToInt16(dtRslt.Rows[i]["Y_LOCATION"].ToString()));


                        grdCathode.Children.Add(assmRack);
                    }

                }

                //PortBind();
            }
        }
        #endregion

        #region 음극 데이터 Bind : AnodeDataBind()
        /// <summary>
        /// 음극 데이터 Bind
        /// </summary>
        private void AnodeDataBind()
        {

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("EQGRID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("EQPTID", typeof(string));
            dtRqst.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            DataRow dr = dtRqst.NewRow();
            dr["EQGRID"] = "PCB";
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = "W1APCB102";
            dr["ELTR_TYPE_CODE"] = "A";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_LOT_MEB_POSITION", "INDATA", "OUTDATA", dtRqst);

            grdAnode.Children.Clear();

            if (grdAnode.ColumnDefinitions.Count > 0)
            {
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (dtRslt.Rows[i]["X_LOCATION"].ToString() != string.Empty || dtRslt.Rows[i]["Y_LOCATION"].ToString() != string.Empty)
                    {
                        MEBRack assmRack = new MEBRack();
                        assmRack.Name = dtRslt.Rows[i]["PORT_ID"].ToString();

                        //assmRack.ButtonUsControl.Click += ButtonUsControl_Click;
                        assmRack.PORT_ID = dtRslt.Rows[i]["PORT_ID"].ToString();
                        assmRack.LotId = dtRslt.Rows[i]["LOTID"].ToString();
                        assmRack.ProdId = dtRslt.Rows[i]["PRODID"].ToString();
                        assmRack.ProcId = dtRslt.Rows[i]["PROCID"].ToString();
                        assmRack.Wipstate = dtRslt.Rows[i]["WIPSTAT"].ToString();
                        assmRack.Wiphold = dtRslt.Rows[i]["WIPHOLD"].ToString();
                        assmRack.Spcl_Flag = dtRslt.Rows[i]["SPCL_FLAG"].ToString();
                        assmRack.Mtrlexistflag = dtRslt.Rows[i]["MTRL_EXIST_FLAG"].ToString();
                        assmRack.PortStat = dtRslt.Rows[i]["PORT_STAT_CODE"].ToString();
                        assmRack.QA = dtRslt.Rows[i]["QA"].ToString();
                        assmRack.DoubleClick += assmRack_DoubleClick; //더블클릭
                        Grid.SetRow(assmRack, Convert.ToInt16(dtRslt.Rows[i]["X_LOCATION"].ToString()));
                        Grid.SetColumn(assmRack, Convert.ToInt16(dtRslt.Rows[i]["Y_LOCATION"].ToString()));


                        grdAnode.Children.Add(assmRack);
                    }

                }

                //PortBind();
            }
        }
        #endregion

        #region 생산설비포트현황 BIND : EqptPortBind()
        /// <summary>
        /// 생산설비포트현황 BIND
        /// </summary>
        private void EqptPortBind()
        {

            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("LANG", typeof(string));
            dtRqst.Columns.Add("NOTCHING_CHK", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANG"] = LoginInfo.LANGID;
            dr["NOTCHING_CHK"] = "Y";
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_PRODUCT_EQPT", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
            {

                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
              

                Util.GridSetData(dgEqptPort, result, FrameOperation, true);
                loadingIndicator.Visibility = Visibility.Collapsed;

            });

        }
        #endregion

        #region 출고대상 정보 Bind : OutputBind()
        /// <summary>
        /// 출고대상 정보 Bind
        /// </summary>
        private void OutputBind()
        {

            loadingIndicator.Visibility = Visibility.Visible;
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));
            dtRqst.Columns.Add("LOGIS_CMD_ID", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("EQGRID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["PRODID"] = null; //txtProdID_OutPut.Text == string.Empty ? null : txtProdID_OutPut.Text;
            dr["LOTID"] = null;  //txtLot_OutPut.Text == string.Empty ? null : txtLot_OutPut.Text;
            dr["LOGIS_CMD_ID"] = null; // txtReqID_OutPut.Text == string.Empty ? null : txtReqID_OutPut.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQGRID"] = "PCB";
            dtRqst.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_LOGIS_MEB", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
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

        #region 반송 정보 : ReturnInfoBind()

        ///// <summary>
        ///// 반송 정보
        ///// </summary>
        //private void ReturnInfoBind(string Lotid)
        //{

        //    DataTable dtRqst = new DataTable();
        //    dtRqst.Columns.Add("LOTID", typeof(string));
        //    dtRqst.Columns.Add("LANGID", typeof(string));

        //    DataRow dr = dtRqst.NewRow();
        //    dr["LOTID"] = Lotid;
        //    dr["LANGID"] = LoginInfo.LANGID;
        //    dtRqst.Rows.Add(dr);

        //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_RETURN_INFO", "INDATA", "OUTDATA", dtRqst);
        //    if (dtRslt.Rows.Count > 0)
        //    {
        //        txtEquipment.Text = dtRslt.Rows[0]["EQPTNAME"].ToString();
        //        txtEquipment.Tag = dtRslt.Rows[0]["EQPTID"].ToString();
        //        txtFrom.Text = dtRslt.Rows[0]["FROM_RACK_ID"].ToString();
        //        txtTo.Text = dtRslt.Rows[0]["TO_RACK_ID"].ToString();
        //        txtReturnTime.Text = dtRslt.Rows[0]["LOGIS_CMD_GNRT_DTTM"].ToString();
        //    }
        //    else
        //    {
        //        txtEquipment.Text = string.Empty;
        //        txtEquipment.Tag = null;
        //        txtFrom.Text = string.Empty;
        //        txtTo.Text = string.Empty;
        //        txtReturnTime.Text = string.Empty;
        //    }
        //}

        #endregion
      
        #region  초기화 :  Clear()
        /// <summary>
        /// 초기화
        /// </summary>
        private void Clear()
        {

            //좌측 Clear
            Util.gridClear(dgEqptPort);
            //하단 Clear
            Util.gridClear(dgOutputInfo);


        }


        #endregion

        #region 재조회  : Refresh()

        private void Refresh(bool bButton)
        {
            try
            {
                monitorTimer.Stop();

                loadingIndicator.Visibility = Visibility.Visible;
                //버튼 조회시
                if (bButton == true)
                {
                    //초기화
                    Clear();
                    MaterialsDataBind();  //자재창고보관버퍼 조회 
                    CathodDataBind();  // 양극 데이터 조회
                    AnodeDataBind(); //음극 데이터 조회
                    EqptPortBind(); //생산설비포트현황
                    ReloadQA();
                    OutputBind();  //출고대상 정보
                }
                else // timer로 인한 조회시
                {
                    MaterialsDataBind();  //자재창고보관버퍼 조회 
                    CathodDataBind();  // 양극 데이터 조회
                    AnodeDataBind(); //음극 데이터 조회
                    EqptPortBind();  //생산설비포트현황
                    ReloadQA();
                    OutputBind();  //출고대상 정보
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

                loadingIndicator.Visibility = Visibility.Collapsed;
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
                string PRJT_NAME = txtPjt.Text;


                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQGRID"] = "PCB";
                if (PRJT_NAME == string.Empty)
                {
                    newRow["PRJT_NAME"] = null;
                }
                else
                {
                    newRow["PRJT_NAME"] = PRJT_NAME;
                }


                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_MCS_SEL_QA_MEB_LAMI_NH", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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

                        GridData.Columns.Add("PRJT_NAME", typeof(string)); // 
                        GridData.Columns.Add("ELTR_TYPE_CODE", typeof(string)); // 
                        GridData.Columns.Add("CC", typeof(int)); // 양극
                        GridData.Columns.Add("CC_WIP_QTY", typeof(int)); // 양극 총 수량
                        GridData.Columns.Add("AC", typeof(int)); // 음극
                        GridData.Columns.Add("AC_WIP_QTY", typeof(int)); // 음극 총 수량
                        GridData.Columns.Add("ALL_SUM", typeof(decimal));
                        GridData.Columns.Add("WIP_QTY", typeof(decimal));
                        GridData.Columns.Add("CODE", typeof(string));

                        string PRJTNAME = string.Empty;
                        if (searchResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {

                                if (searchResult.Rows[i]["PRJT_NAME"].ToString() != PRJTNAME)
                                {
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("라미대기"), 0, 0, 0, 0, 0, 0, 'Y' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("검사중"), 0, 0, 0, 0, 0, 0, 'I' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("불합격"), 0, 0, 0, 0, 0, 0, 'F' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("재작업"), 0, 0, 0, 0, 0, 0, 'R' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("대기"), 0, 0, 0, 0, 0, 0, 'W' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("HOLD"), 0, 0, 0, 0, 0, 0, 'E' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("특별관리"), 0, 0, 0, 0, 0, 0, 'V' });
                                    GridData.Rows.Add(new object[] { searchResult.Rows[i]["PRJT_NAME"].ToString(), ObjectDic.Instance.GetObjectName("TOTAL"), 0, 0, 0, 0, 0, 0, 'A' });
                                    foreach (DataRow dr in searchResult.Rows)
                                    {
                                        if (searchResult.Rows[i]["PRJT_NAME"].ToString() == dr["PRJT_NAME"].ToString())
                                        {
                                            int idx = 0;
                                            if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                            {
                                                // 양극
                                                idx = dgQA.Columns["CC"].Index;
                                            }
                                            else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                            {
                                                // 음극
                                                idx = dgQA.Columns["AC"].Index;
                                            }
                                            if (idx > 0)
                                            {
                                                int Row = 0;
                                                for (int j = 0; j < GridData.Rows.Count; j++)
                                                {
                                                    if (searchResult.Rows[i]["PRJT_NAME"].ToString() == GridData.Rows[j]["PRJT_NAME"].ToString())
                                                    {
                                                        Row = j;
                                                        break;
                                                    }

                                                }
                                                GridData.Rows[Row][idx] = dr["QA_Y"];
                                                GridData.Rows[Row]["WIP_QTY"] = Convert.ToDecimal(dr["QA_Y_QTY"]) + Convert.ToDecimal(GridData.Rows[Row]["WIP_QTY"]);
                                                GridData.Rows[Row]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row][idx]) + Convert.ToInt32(GridData.Rows[Row]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_Y_QTY"]) + Convert.ToDecimal(GridData.Rows[Row]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_Y_QTY"]) + Convert.ToDecimal(GridData.Rows[Row]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 1][idx] = dr["QA_I"];
                                                GridData.Rows[Row + 1]["WIP_QTY"] = Convert.ToDecimal(dr["QA_I_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["WIP_QTY"]);
                                                GridData.Rows[Row + 1]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 1][idx]) + Convert.ToInt32(GridData.Rows[Row + 1]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 1]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_I_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 1]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_I_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 2][idx] = dr["QA_F"];
                                                GridData.Rows[Row + 2]["WIP_QTY"] = Convert.ToDecimal(dr["QA_F_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["WIP_QTY"]);
                                                GridData.Rows[Row + 2]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 2][idx]) + Convert.ToInt32(GridData.Rows[Row + 2]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 2]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_F_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 2]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_F_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 3][idx] = dr["QA_R"];
                                                GridData.Rows[Row + 3]["WIP_QTY"] = Convert.ToDecimal(dr["QA_R_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["WIP_QTY"]);
                                                GridData.Rows[Row + 3]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 3][idx]) + Convert.ToInt32(GridData.Rows[Row + 3]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 3]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_R_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 3]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_R_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 4][idx] = dr["QA_W"];
                                                GridData.Rows[Row + 4]["WIP_QTY"] = Convert.ToDecimal(dr["QA_W_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["WIP_QTY"]);
                                                GridData.Rows[Row + 4]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 4][idx]) + Convert.ToInt32(GridData.Rows[Row + 4]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 4]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_W_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 4]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_W_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 5][idx] = dr["QA_E"];
                                                GridData.Rows[Row + 5]["WIP_QTY"] = Convert.ToDecimal(dr["QA_E_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["WIP_QTY"]);
                                                GridData.Rows[Row + 5]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 5][idx]) + Convert.ToInt32(GridData.Rows[Row + 5]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 5]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_E_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 5]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_E_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 6][idx] = dr["QA_V"];
                                                GridData.Rows[Row + 6]["WIP_QTY"] = Convert.ToDecimal(dr["QA_V_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["WIP_QTY"]);
                                                GridData.Rows[Row + 6]["ALL_SUM"] = Convert.ToInt32(GridData.Rows[Row + 6][idx]) + Convert.ToInt32(GridData.Rows[Row + 6]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 6]["CC_WIP_QTY"] = Convert.ToDecimal(dr["QA_V_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 6]["AC_WIP_QTY"] = Convert.ToDecimal(dr["QA_V_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["AC_WIP_QTY"]);


                                                GridData.Rows[Row + 7][idx] = Convert.ToDecimal(GridData.Rows[Row][idx]) + Convert.ToDecimal(GridData.Rows[Row + 1][idx]) + Convert.ToDecimal(GridData.Rows[Row + 2][idx]) + Convert.ToDecimal(GridData.Rows[Row + 3][idx]) + Convert.ToDecimal(GridData.Rows[Row + 4][idx]) + Convert.ToDecimal(GridData.Rows[Row + 5][idx]) + Convert.ToDecimal(GridData.Rows[Row + 6][idx]);
                                                GridData.Rows[Row + 7]["WIP_QTY"] = Convert.ToDecimal(GridData.Rows[Row]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["WIP_QTY"]);
                                                GridData.Rows[Row + 7]["ALL_SUM"] = Convert.ToDecimal(GridData.Rows[Row]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["ALL_SUM"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["ALL_SUM"]);
                                                if (dr["ELTR_TYPE_CODE"].ToString().Equals("C"))
                                                    GridData.Rows[Row + 7]["CC_WIP_QTY"] = Convert.ToDecimal(GridData.Rows[Row]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["CC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["CC_WIP_QTY"]);
                                                else if (dr["ELTR_TYPE_CODE"].ToString().Equals("A"))
                                                    GridData.Rows[Row + 7]["AC_WIP_QTY"] = Convert.ToDecimal(GridData.Rows[Row]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 1]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 2]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 3]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 4]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 5]["AC_WIP_QTY"]) + Convert.ToDecimal(GridData.Rows[Row + 6]["AC_WIP_QTY"]);
                                            }
                                        }
                                    }
                                }

                                PRJTNAME = searchResult.Rows[i]["PRJT_NAME"].ToString();
                            }

                            Util.GridSetData(this.dgQA, GridData, null, true);
                        }



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

        #endregion

   
    }

}
