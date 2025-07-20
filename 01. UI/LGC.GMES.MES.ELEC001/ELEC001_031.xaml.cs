using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;



namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_031.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC001_031 : UserControl
    {
        CommonCombo _combo = new CommonCombo();
        private System.Windows.Threading.DispatcherTimer timer;
        string _AREATYPE = "";
        string _LOTID = "";
        int msgCount = 0;
        bool timerChk = false;
        bool searchChk = false;
        Grid[] dgSearch = new Grid[6];
        Grid[] dgBottom = new Grid[6];
        GridSplitter[] dgSplitter = new GridSplitter[6];
        C1DataGrid[] dgQuality = new C1DataGrid[6];
        TextBox[] txtLotid = new TextBox[6];
        TextBox[] txtPjt = new TextBox[6];
        TextBox[] txtPolarity = new TextBox[6];
        TextBox[] txtWipseq = new TextBox[6];
        TextBox[] txtCbo_code = new TextBox[6];
        string[] btnQuSave = new string[6];
        Util _Util = new Util();


        public ELEC001_031()
        {
            InitializeComponent();
            InitCombo();
            InitGrid();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // Timer 설정
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(txtCycle.Value);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Tick += timer_Start;
            timer.Start();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Tick -= timer_Start;
        }

        #region Timer
        private void timer_Start(object sender, EventArgs e)
        {
            GriddgQualityData();
            timerChk = true;
        }
        #endregion

        #region [ 콤보박스 만들기 ]
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);

            //공정

            String[] sFilter1 = { "THICK_PROCESS", Process.ROLL_PRESSING };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "THICK_PROCESS");

        }
        #endregion

        #region Grid속성
        private void InitGrid()
        {           
            dgSearch[0] = dgSearch1;
            dgSearch[1] = dgSearch2;
            dgSearch[2] = dgSearch3;
            dgSearch[3] = dgSearch4;
            dgSearch[4] = dgSearch5;
            dgSearch[5] = dgSearch6;

            dgBottom[0] = dgBottom1;
            dgBottom[1] = dgBottom2;
            dgBottom[2] = dgBottom3;
            dgBottom[3] = dgBottom4;
            dgBottom[4] = dgBottom5;
            dgBottom[5] = dgBottom6;
            
            dgQuality[0] = dgQuality1;
            dgQuality[1] = dgQuality2;
            dgQuality[2] = dgQuality3;
            dgQuality[3] = dgQuality4;
            dgQuality[4] = dgQuality5;
            dgQuality[5] = dgQuality6;

            dgSplitter[0] = dgSplitter1;
            dgSplitter[1] = dgSplitter2;
            dgSplitter[2] = dgSplitter3;
            dgSplitter[3] = dgSplitter4;
            dgSplitter[4] = dgSplitter5;
            dgSplitter[5] = dgSplitter6;

            txtLotid[0] = Lotid1;
            txtLotid[1] = Lotid2;
            txtLotid[2] = Lotid3;
            txtLotid[3] = Lotid4;
            txtLotid[4] = Lotid5;
            txtLotid[5] = Lotid6;

            txtPjt[0] = Pjt1;
            txtPjt[1] = Pjt2;
            txtPjt[2] = Pjt3;
            txtPjt[3] = Pjt4;
            txtPjt[4] = Pjt5;
            txtPjt[5] = Pjt6;

            txtPolarity[0] = Polarity1;
            txtPolarity[1] = Polarity2;
            txtPolarity[2] = Polarity3;
            txtPolarity[3] = Polarity4;
            txtPolarity[4] = Polarity5;
            txtPolarity[5] = Polarity6;

            txtWipseq[0] = Wipseq1;
            txtWipseq[1] = Wipseq2;
            txtWipseq[2] = Wipseq3;
            txtWipseq[3] = Wipseq4;
            txtWipseq[4] = Wipseq5;
            txtWipseq[5] = Wipseq6;

            txtCbo_code[0] = Cbo_code1;
            txtCbo_code[1] = Cbo_code2;
            txtCbo_code[2] = Cbo_code3;
            txtCbo_code[3] = Cbo_code4;
            txtCbo_code[4] = Cbo_code5;
            txtCbo_code[5] = Cbo_code6;
            
            
        }

        #endregion
        #region [ 타이머 설정 ]
        private void btnCycle_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(txtCycle.Value);

            Util.MessageInfoAutoClosing("SFU1166");    // 변경 되었습니다.
        }
        #endregion

        #region [ 조회버튼 ]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            timerChk = false;
            searchChk = true;
            GriddgQualityData();
        }
        #endregion


        public void GriddgQualityData()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue;
            dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
            dr["EQPTID"] = cboEquipment.SelectedItemsToString;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            DataTable _qualDT = new DataTable();

            if (timerChk == false)
            {
                if (searchChk == true)
                {
                    for (int j = 0; j < dgQuality.Length; j++)
                    {
                        Util.gridClear(dgQuality[j]);
                        dgSearch[j].Visibility = Visibility.Collapsed;
                        dgQuality[j].Visibility = Visibility.Collapsed;
                        dgBottom[j].Visibility = Visibility.Collapsed;
                        dgSplitter[j].Visibility = Visibility.Collapsed;
                    }
                }
            }

            for (int i = 0; i < dtResult.Rows.Count; i++)
            {

                _LOTID = txtLotid[i].Text;

                if (timerChk == true)
                {
                    if(searchChk == true) { 
                        if (btnQuSave[i] == "Y")
                            {
                            if (_LOTID != dtResult.Rows[i]["LOTID"].ToString())
                            {
                                msgCount++;
                                if (msgCount == 1)
                                {
                                    Util.MessageInfo("SFU1199");
                                }
                            }
                            btnQuSave[i] = "N";
                            }
                            else
                            {
                                //if (_LOTID != dtResult.Rows[i]["LOTID"].ToString())
                                //{
                                //    msgCount++;
                                //    if (msgCount == 1)
                                //    {
                                //        Util.MessageInfo("SFU1199");
                                //    }
                                //}                         
                        }
                    }
                }
                // 조회클릭 
                else
                {
                        dgSearch[i].Visibility = Visibility.Visible;
                        dgQuality[i].Visibility = Visibility.Visible;
                        dgBottom[i].Visibility = Visibility.Visible;
                        dgSplitter[i].Visibility = Visibility.Visible;
                        txtPolarity[i].Text = dtResult.Rows[i]["CBO_NAME"].ToString();
                        txtLotid[i].Text = dtResult.Rows[i]["LOTID"].ToString();
                        txtPjt[i].Text = dtResult.Rows[i]["PJT"].ToString();
                        txtWipseq[i].Text = dtResult.Rows[i]["WIPSEQ"].ToString();
                        txtCbo_code[i].Text = dtResult.Rows[i]["CBO_CODE"].ToString();
                        
                        dtDataCollectdQuality(dtResult);

                }
            }
            msgCount = 0;
        }

        private void dtDataCollectdQuality(DataTable dtResult)
        {
            for (int i = 0; i < dtResult.Rows.Count; i++)
            {
                if (dtResult.Rows[i]["LOTID"].ToString() != "")
                {
                    _Util.SetDataGridMergeExtensionCol(dgQuality[i], new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                    DataTable IndataTable = new DataTable();
                    DataTable _qualDT = new DataTable();
                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("AREAID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                    IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    IndataTable.Columns.Add("VER_CODE", typeof(string));
                    IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = dtResult.Rows[i]["AREAID"].ToString();
                    Indata["PROCID"] = dtResult.Rows[i]["PROCID"].ToString();
                    Indata["LOTID"] = dtResult.Rows[i]["LOTID"].ToString();
                    Indata["WIPSEQ"] = dtResult.Rows[i]["WIPSEQ"].ToString();
                    Indata["VER_CODE"] = dtResult.Rows[i]["VER_CODE"].ToString();
                    if (dtResult.Rows[i]["LANEQTY"].ToString() != null && dtResult.Rows[i]["LANEQTY"].ToString() != "")
                    {
                        Indata["LANEQTY"] = dtResult.Rows[i]["LANEQTY"].ToString();
                    }
                    IndataTable.Rows.Add(Indata);

                    _qualDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);
                    if (_qualDT.Rows.Count == 0)
                    {
                        _qualDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);
                        Util.GridSetData(dgQuality[i], _qualDT, FrameOperation, true);
                    }
                    else
                    {
                        Util.GridSetData(dgQuality[i], _qualDT, FrameOperation, true);
                    }
                }else
                {

                }
                //
            }

        }


        private void btnSaveQuality1_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[0] = "Y";
            SaveQuality(dgQuality1);            
        }

        private void btnSaveQuality2_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[1] = "Y";
            SaveQuality(dgQuality2);

        }
        private void btnSaveQuality3_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[2] = "Y";
            SaveQuality(dgQuality3);

        }
        private void btnSaveQuality4_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[3] = "Y";
            SaveQuality(dgQuality4);

        }
        private void btnSaveQuality5_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[4] = "Y";
            SaveQuality(dgQuality5);

        }
        private void btnSaveQuality6_Click(object sender, RoutedEventArgs e)
        {

            btnQuSave[5] = "Y";
            SaveQuality(dgQuality6);

        }
        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;


                foreach (DataRow _iRow in dt.Rows)
                {
                    inData = IndataTable.NewRow();

                    inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (dg == dgQuality1)
                {
                    inData["LOTID"] = Lotid1.Text.ToString();
                    inData["EQPTID"] = Cbo_code1.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq1.Text.ToString()) ? null : Wipseq1.Text.ToString();
                }


                if (dg == dgQuality2)
                {
                    inData["LOTID"] = Lotid2.Text.ToString();
                    inData["EQPTID"] = Cbo_code2.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq2.Text.ToString()) ? null : Wipseq2.Text.ToString();
                }

                if (dg == dgQuality3)
                {
                    inData["LOTID"] = Lotid3.Text.ToString();
                    inData["EQPTID"] = Cbo_code3.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq3.Text.ToString()) ? null : Wipseq3.Text.ToString();
                }
                if (dg == dgQuality4)
                {
                    inData["LOTID"] = Lotid4.Text.ToString();
                    inData["EQPTID"] = Cbo_code4.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq4.Text.ToString()) ? null : Wipseq4.Text.ToString();
                }
                if (dg == dgQuality5)
                {
                    inData["LOTID"] = Lotid5.Text.ToString();
                    inData["EQPTID"] = Cbo_code5.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq5.Text.ToString()) ? null : Wipseq5.Text.ToString();
                }
                if (dg == dgQuality6)
                {
                    inData["LOTID"] = Lotid6.Text.ToString();
                    inData["EQPTID"] = Cbo_code6.Text.ToString();
                    inData["WIPSEQ"] = string.IsNullOrEmpty(Wipseq6.Text.ToString()) ? null : Wipseq6.Text.ToString();
                }
                inData["USERID"] = LoginInfo.USERID;
                    inData["CLCTITEM"] = _iRow["CLCTITEM"];
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim();                    
                    inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            }
            
            return IndataTable;
        }


        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                return;
            }

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);
            
            new ClientProxy().ExecuteService("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    //Util.AlertByBiz("BR_QCA_REG_WIP_DATA_CLCT", ex.Message, ex.ToString());
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1998");    //품질 정보가 저장되었습니다.

            });
        }


        private void SetcboEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);                

                cboEquipment.ItemsSource = DataTableConverter.Convert(dtResult);


                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboEquipment.Check(i);

                    //if (Util.NVC(cboEquipmentSegment.SelectedValue).Equals(LoginInfo.CFG_EQSG_ID))
                    //{
                    //    cboProcess.Check(i);
                    //    break;
                    //}
                    //else
                    //    cboProcess.Check(i);
                }

            }
            catch (Exception ex)
            {
                
            }

        }


        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetcboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        //품질정보 키 이벤트
        private void LoadedQualityInfoCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {

                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            //else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01") || string.Equals(e.Cell.Column.Name, "CLCTVAL02"))
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));

                                if (panel != null)
                                {

                                    C1NumericBox numeric = panel.Children[0] as C1NumericBox;
                                    


                                    if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && numeric.Value != 0 && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                    {
                                        // 프레임버그로 값 재 설정 [2017-12-06]
                                        // 액셀 붙여넣기 기능으로 빈칸이 입력될 경우 Convert클래스 이용 시 오류 발생 문제로 체크용 Function 교체 [2019-01-28]
                                        if (!string.IsNullOrWhiteSpace(sValue) && !string.Equals(sValue, "NaN"))
                                            numeric.Value = Convert.ToDouble(sValue);

                                        if (sLSL != "" && Util.NVC_Decimal(numeric.Value) < Util.NVC_Decimal(sLSL))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        }

                                        else if (sUSL != "" && Util.NVC_Decimal(numeric.Value) > Util.NVC_Decimal(sUSL))
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                        }
                                        else
                                        {
                                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                        }
                                    }
                                    
                                    numeric.PreviewKeyDown -= OnDataCollectGridPreviewItmeKeyDown;
                                    numeric.PreviewKeyDown += OnDataCollectGridPreviewItmeKeyDown;
                                    numeric.LostKeyboardFocus -= OnDataCollectGridGotKeyboardLost;
                                    numeric.LostKeyboardFocus += OnDataCollectGridGotKeyboardLost;

                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }

                }));
            }
        }

        protected virtual void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                //timer.Stop();
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Enter)
                {
                    int iRowIdx = 0;
                    int iColIdx = 0;
                    C1.WPF.DataGrid.C1DataGrid grid;

                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                       
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                    }
                    else
                        return;

                    if (e.Key == Key.Down || e.Key == Key.Enter)
                    {
                        if ((iRowIdx + 1) < (grid.GetRowCount() - 1))
                            grid.ScrollIntoView(iRowIdx + 2, grid.Columns["CLCTVAL01"].Index);

                        if (grid.GetRowCount() > ++iRowIdx)
                        {
                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    else if (e.Key == Key.Up)
                    {
                        if (grid.GetRowCount() > --iRowIdx)
                        {
                            if (iRowIdx > 0)
                                grid.ScrollIntoView(iRowIdx - 1, grid.Columns["CLCTVAL01"].Index);

                            if (iRowIdx < 0)
                            {
                                e.Handled = true;
                                return;
                            }

                            grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                            C1.WPF.DataGrid.DataGridCellPresenter p = grid.GetCell(iRowIdx, iColIdx).Presenter;

                            if (p != null)
                            {
                                StackPanel panel = p.Content as StackPanel;

                                for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                                {
                                    if (panel.Children[cnt].Visibility == Visibility.Visible)
                                        panel.Children[cnt].Focus();
                                }
                            }
                        }
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Delete)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        int iColIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        iColIdx = p.Cell.Column.Index;
                        grid = p.DataGrid;
                        item.Value = double.NaN;

                        C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                        currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                        currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        currentCell.Presenter.FontWeight = FontWeights.Normal;
                    }

                    else if (sender.GetType().Name == "ComboBox")
                    {
                        ComboBox item = sender as ComboBox;
                        item.Text = string.Empty;
                        item.SelectedIndex = -1;
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (sender.GetType().Name == "C1NumericBox")
                    {
                        int iRowIdx = 0;
                        C1.WPF.DataGrid.C1DataGrid grid;

                        C1NumericBox item = sender as C1NumericBox;
                        StackPanel panel = item.Parent as StackPanel;
                        C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                        iRowIdx = p.Cell.Row.Index;
                        grid = p.DataGrid;

                        // 액셀파일 PASTE시 공란PASS없이 전체 붙여넣기 추가 [2019-01-28]
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None);

                        foreach (string line in lines)
                        {
                            if (iRowIdx < grid.GetRowCount())
                                if (string.Equals(DataTableConverter.GetValue(grid.Rows[iRowIdx].DataItem, "INSP_VALUE_TYPE_CODE"), "NUM"))
                                    DataTableConverter.SetValue(grid.Rows[iRowIdx].DataItem, "CLCTVAL01", line.Trim());

                            iRowIdx++;
                        }
                    }
                    dgQuality1.Refresh();
                    dgQuality2.Refresh();
                    dgQuality3.Refresh();
                    dgQuality4.Refresh();
                    dgQuality5.Refresh();
                    dgQuality6.Refresh();
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        //


        //상한치 하한치 색 이벤트

        protected virtual void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
       

                int iRowIdx = 0;
                int iColIdx = 0;
                C1.WPF.DataGrid.C1DataGrid grid;

                if (sender.GetType().Name == "C1NumericBox")
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    C1.WPF.DataGrid.DataGridCell currentCell = grid.GetCell(iRowIdx, iColIdx);
                    string sLSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "LSL"));
                    string sUSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "USL"));

                    if (item != null && !string.IsNullOrEmpty(Util.NVC(item.Value)) && item.Value != 0 && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        if (sLSL != "" && Util.NVC_Decimal(item.Value) < Util.NVC_Decimal(sLSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        else if (sUSL != "" && Util.NVC_Decimal(item.Value) > Util.NVC_Decimal(sUSL))
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            currentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            currentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            currentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                        //isChangeQuality = true;
                    }


                }

                else if (sender.GetType().Name == "ComboBox")
                {
                    ComboBox item = sender as ComboBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    iRowIdx = p.Cell.Row.Index;
                    iColIdx = p.Cell.Column.Index;
                    grid = p.DataGrid;

                    //isChangeQuality = true;
                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
            finally
            {
                //isDupplicatePopup = false;
            }
        }

        //                         
        

        //불확실
        #region [### 공정의 AreaType ###]
        public void GetAreaType(string sProcID)
        {
            try
            {

                _AREATYPE = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = sProcID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    _AREATYPE = dtRslt.Rows[0]["PCSGID"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }






        #endregion


    }
}
