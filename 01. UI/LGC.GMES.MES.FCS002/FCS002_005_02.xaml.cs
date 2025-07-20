/*************************************************************************************
 Created Date : 2022.12.14
      Creator : 강동희
   Decription : Tray List
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.14  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.Configuration;
using System.IO;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_005_02 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        private string _sOPER = string.Empty;
        private string _sOPER_NAME = string.Empty;
        private string _sLINE_ID = string.Empty;
        private string _sLINE_NAME = string.Empty;
        private string _sROUTE_ID = string.Empty;
        private string _sROUTE_NAME = string.Empty;
        private string _sMODEL_ID = string.Empty;
        private string _sMODEL_NAME = string.Empty;
        private string _sROUTE_TYPE_DG = string.Empty;
        private string _sROUTE_TYPE_DG_NAME = string.Empty;
        private string _sStatus = string.Empty;
        private string _sStatusName = string.Empty;
        private string _sSPECIAL_YN = string.Empty;
        private string _sSpecialName = string.Empty;
        private string _sLOT_ID = string.Empty;
        private string _sAgingEqpID = string.Empty;
        private string _sAgingEqpName = string.Empty; //추가 02.17 
        private string _sOpPlanTime = string.Empty;
        private string _sLotType = string.Empty;
        private string _sLotTypeName = string.Empty;
        private bool checkEvent = true;
        private bool isNoSampleOut = false;
        private string _sProcGrCode = string.Empty;
        bool bUseFlag = false; //20221018_Rack간 이동/이동해제 기능 추가

        DataTable dtBoxList = new DataTable();

        public FCS002_005_02()
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

            dtBoxList.Columns.Add("EQP_ID", typeof(string));
            dtBoxList.Columns.Add("COLOR_FLAG", typeof(string));

            chkGroup.IsChecked = true;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);

            object[] tmps = this.FrameOperation.Parameters;

            _sOPER = tmps[0] as string;
            _sOPER_NAME = tmps[1] as string;
            _sLINE_ID = tmps[2] as string;
            _sLINE_NAME = tmps[3] as string;
            _sROUTE_ID = tmps[4] as string;
            _sROUTE_NAME = tmps[5] as string;
            _sMODEL_ID = tmps[6] as string;
            _sMODEL_NAME = tmps[7] as string;
            _sStatus = tmps[8] as string;
            _sStatusName = tmps[9] as string;
            _sROUTE_TYPE_DG = tmps[10] as string;
            _sROUTE_TYPE_DG_NAME = tmps[11] as string;
            _sLOT_ID = tmps[12] as string;
            _sSPECIAL_YN = tmps[13] as string;
            _sAgingEqpID = tmps[14] as string; //추가 02-17
            if (tmps.Length > 15) //추가 02-17
            {
                _sAgingEqpName = tmps[15] as string;
                _sOpPlanTime = tmps[16] as string;
            }
            _sLotType = tmps[17] as string;
            _sLotTypeName = tmps[18] as string;

            InitText();
            GetList();
            
            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_UnChecked;

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Index == 0)
                        return;
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        ////////////////////////////////////////////  default 색상 및 Cursor
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        ///////////////////////////////////////////////////////////////////////////////////

                        

                        if ((e.Cell.Column.Name.Equals("CSTID") && e.Cell.Row.Index < dataGrid.Rows.Count - 1))
                        {
                            //int row = e.Cell.Row.Index;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue); // CSTID 색상 변경
                        }
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DUMMY_FLAG")).ToString().Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE")).ToString().Equals("Y"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_CODE")).ToString().Equals("H"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Magenta);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                        if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).ToString().Equals("SAMPLE")) ||
                            (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_TYPE")).ToString().Equals("SAMPLE_SHIP")))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(System.Windows.Media.Colors.DarkViolet);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }





                        if (e.Cell.Column.Name.Equals("PROD_LOTID"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("합계")))
                            {
                                e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                                dgTrayList.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index - 1)).Presenter.Visibility = Visibility.Collapsed;
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("PERCENT_VAL")))
                            {
                                e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                                dgTrayList.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index - 1)).Presenter.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            }
                        }

                        if (e.Cell.Column.Name.Equals("EQP_ID"))
                        {
                            if (dtBoxList.Rows.Count > 0)
                            {
                                string sBoxID = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQP_ID")).ToString();
                                if (!string.IsNullOrEmpty(sBoxID))
                                {
                                    DataRow[] drSelect = dtBoxList.Select("EQP_ID = '" + sBoxID + "' AND COLOR_FLAG = 'Y'");
                                    if (drSelect.Length > 0)
                                    {
                                        e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                    }
                                }
                            }
                        }

                        //Column Width Set
                        if (e.Cell.Column.Name.Equals("A_GRD_QTY") || e.Cell.Column.Name.Equals("B_GRD_QTY") || e.Cell.Column.Name.Equals("C_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("D_GRD_QTY") || e.Cell.Column.Name.Equals("E_GRD_QTY") || e.Cell.Column.Name.Equals("F_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("G_GRD_QTY") || e.Cell.Column.Name.Equals("H_GRD_QTY") || e.Cell.Column.Name.Equals("I_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("J_GRD_QTY") || e.Cell.Column.Name.Equals("K_GRD_QTY") || e.Cell.Column.Name.Equals("L_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("M_GRD_QTY") || e.Cell.Column.Name.Equals("N_GRD_QTY") || e.Cell.Column.Name.Equals("O_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("P_GRD_QTY") || e.Cell.Column.Name.Equals("Q_GRD_QTY") || e.Cell.Column.Name.Equals("R_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("S_GRD_QTY") || e.Cell.Column.Name.Equals("T_GRD_QTY") || e.Cell.Column.Name.Equals("U_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("Y_GRD_QTY") || e.Cell.Column.Name.Equals("V_GRD_QTY") || e.Cell.Column.Name.Equals("W_GRD_QTY") ||
                            e.Cell.Column.Name.Equals("X_GRD_QTY") || e.Cell.Column.Name.Equals("Z_GRD_QTY") || e.Cell.Column.Name.Equals("GRD_1_QTY") ||
                            e.Cell.Column.Name.Equals("GRD_2_QTY") || e.Cell.Column.Name.Equals("WIP_QTY")   || e.Cell.Column.Name.Equals("INPUT_SUBLOT_QTY"))
                        {
                            e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45);

                        }
                        //Column Size  넓어지는 현상 임시조치
                        string LangID = LoginInfo.LANGID;

                        if (LangID.Equals("ko-KR") || LangID.Equals("zh-CN"))
                        {
                            if (e.Cell.Column.Name.Equals("WIPDTTM_ED") || e.Cell.Column.Name.Equals("WIPDTTM_ST") || e.Cell.Column.Name.Equals("OP_PLAN_TIME") || e.Cell.Column.Name.Equals("ED_PLAN_TIME") ||
                                e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("OP_NAME") || e.Cell.Column.Name.Equals("PROCID") || e.Cell.Column.Name.Equals("NEXT_OP_NAME"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                            }

                            if (e.Cell.Column.Name.Equals("PROD_LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                            }

                            if (e.Cell.Column.Name.Equals("ROUTID") || e.Cell.Column.Name.Equals("OUT_TYPE"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(75);
                            }

                            if (e.Cell.Column.Name.Equals("DUMMY_FLAG") || e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("LOT_TYPE"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(85);
                            }

                            else
                            {

                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                            }
                        }

                        else
                        {
                            if (e.Cell.Column.Name.Equals("WIPDTTM_ED") || e.Cell.Column.Name.Equals("WIPDTTM_ST") || e.Cell.Column.Name.Equals("OP_PLAN_TIME") || e.Cell.Column.Name.Equals("ED_PLAN_TIME") ||
                               e.Cell.Column.Name.Equals("LOT_TYPE") || e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("DUMMY_FLAG") ||
                               e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("OP_NAME") || e.Cell.Column.Name.Equals("PROCID") || e.Cell.Column.Name.Equals("NEXT_OP_NAME"))

                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                            }

                            if (e.Cell.Column.Name.Equals("PROD_LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                            }

                            if (e.Cell.Column.Name.Equals("ROUTID"))
                            {
                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(120);
                            }

                            else
                            {

                                e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                            }

                        }


                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 30)
            {
                //tbTime.Visibility = Visibility.Visible;
                _timer.Stop();
            }
        }
        private void dgTrayList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null)
                    return;

                DataTable dt = DataTableConverter.Convert(dgTrayList.ItemsSource);

                if (e.Row.Index == 0)
                {
                    e.Row.DataGrid.RowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0C0C0")); //GRAY
                }
                else
                {
                    if (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["EQP_ID"]).Equals(dt.Rows[e.Row.Index - 1]["EQP_ID"].ToString()))
                    {
                        e.Row.DataGrid.RowBackground = e.Row.DataGrid.Rows[e.Row.Index - 1].Presenter.Background;
                    }
                    else
                    {
                        if (e.Row.DataGrid.Rows[e.Row.Index - 1].Presenter.Background == new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0C0C0")))
                        {
                            e.Row.DataGrid.RowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C0C0C0")); //GRAY
                        }
                        else
                        {
                            e.Row.DataGrid.RowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF")); //WHITE
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgTrayList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            //AddRowSumnPerQty(dgTrayList);

            gdConditionArea.IsEnabled = true;
        }

        private void dgTrayList_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;

            // 기본 체크값이 없어 처리함
            if (dtResult != null)
            {
                dtResult.Columns.Add("CHK", typeof(bool));
                dtResult.Select().ToList<DataRow>().ForEach(row => row["CHK"] = false);

                string PreBoxID = string.Empty;
                string BoxID = string.Empty;
                bool bColor_Flag = false;
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    BoxID = dtResult.Rows[i]["EQP_ID"].ToString();

                    if (!string.IsNullOrEmpty(BoxID))
                    {


                        if (PreBoxID == string.Empty)
                            PreBoxID = BoxID;

                        if (PreBoxID != BoxID)
                        {
                            bColor_Flag = !bColor_Flag;

                            DataRow d = dtBoxList.NewRow();
                            d["EQP_ID"] = BoxID;
                            d["COLOR_FLAG"] = bColor_Flag == true ? "Y" : "N";

                            dtBoxList.Rows.Add(d);
                        }

                        PreBoxID = BoxID;

                    }
                }
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";


                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("CSTID", typeof(string));
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // CST ID;
                            if (sheet.GetCell(rowInx, 0) == null)
                                continue;

                            string CSTID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            DataRow dataRow = dataTable.NewRow();
                            dataRow["CSTID"] = CSTID;
                            dataTable.Rows.Add(dataRow);
                        }

                        if (dataTable.Rows.Count > 0)
                            dataTable = dataTable.DefaultView.ToTable(true);

                        foreach (DataRow dr in dataTable.Rows)
                        {
                            string cstid = dr["CSTID"].ToString();
                            for (int i = 0; i < dgTrayList.Rows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).Equals(cstid))
                                {
                                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                                    continue;
                                }
                            }
                        }

                        Util.Alert("FM_ME_0239");  //처리가 완료되었습니다.

                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnLowVolt_Click(object sender, RoutedEventArgs e)
        {
            try
            {

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

                if (dgTrayList.Rows.Count == 0) return;

                DataTable dtRslt = new DataTable();
                dtRslt = GetLowVolt();

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).Equals(dtRslt.Rows[j][0].ToString()))
                        {
                            DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                            dgTrayList.Rows[i].Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0000FF"));
                        }
                    }
                }

                lblMsg.Text = ObjectDic.Instance.GetObjectName("선택한 저전압 팔레트 수량 : ") + dtRslt.Rows.Count.ToString();
                lblMsg.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
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

            string sTrayList = string.Empty;


            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    sTrayList += DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID") + "|";
                }
            }

            if (sTrayList.Length == 0)
            {
                //Tray를 선택해주세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0081"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    { }
                });
                return;
            }

            LGC.GMES.MES.FCS002.FCS002_021_SPECIAL_MANAGEMENT wndPopup = new LGC.GMES.MES.FCS002.FCS002_021_SPECIAL_MANAGEMENT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];

                Parameters[0] = sTrayList.Substring(0, sTrayList.Length - 1);
                Parameters[1] = null;

                C1WindowExtension.SetParameters(wndPopup, Parameters);
                wndPopup.Closed += new EventHandler(wndPopup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                wndPopup.BringToFront();
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_021_SPECIAL_MANAGEMENT window = sender as FCS002_021_SPECIAL_MANAGEMENT;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void OnCloseRegDefectLane(object sender, EventArgs e)
        {
            LGC.GMES.MES.FCS002.FCS002_021_SPECIAL_MANAGEMENT window = sender as LGC.GMES.MES.FCS002.FCS002_021_SPECIAL_MANAGEMENT;
            if (window.DialogResult == MessageBoxResult.OK)
                GetList();
        }

        private void btnRoute_Click(object sender, RoutedEventArgs e)
        {
            int iChkCnt = 0;
            try
            {
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

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        iChkCnt++;
                    }
                }

                if (iChkCnt == 0)
                {
                    //선택된 데이터가 없습니다.
                    Util.MessageInfo("FM_ME_0165");
                    return;
                }

                ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                         Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                    {
                        alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                    }
                }

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("BF_ROUTE_ID", typeof(string));
                dtRqst.Columns.Add("MDF_ID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                    // 일괄선택시 합계행 제외
                    if (string.IsNullOrEmpty(sLotid))
                        continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (!((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3")) ||
                              (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("4")) ||
                              (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7")) ||
                              (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9"))))
                        {
                            //Aging 공정이 아닌 Tray가 있습니다.
                            Util.MessageInfo("FM_ME_0015");
                            return;
                        }

                        DataRow dr = dtRqst.NewRow();
                        dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")).ToString();
                        dr["BF_ROUTE_ID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "ROUTID")).ToString();
                        dr["MDF_ID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }
                }

                FCS002_005_ROUTE_CHG wndConfirm = new FCS002_005_ROUTE_CHG();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    wndConfirm.TrayList = dtRqst;

                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void btnForceOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                         Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                    {
                        alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                    }
                }

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                    }
                }

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LANGID", typeof(string));
                dtInData.Columns.Add("AREAID", typeof(string));

                DataRow drIn = dtInData.NewRow();
                drIn["USERID"] = LoginInfo.USERID;
                drIn["LANGID"] = LoginInfo.LANGID;
                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;

                dtInData.Rows.Add(drIn);

                DataTable dtInLot = ds.Tables.Add("INLOT");
                dtInLot.Columns.Add("LOTID", typeof(string));


                string sMessage = "";
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "AGING_ISS_PRIORITY_NO")).Equals("5"))
                        {
                            Util.Alert("FM_ME_0535");
                            return;
                        }
                        string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        // 일괄선택시 합계행 제외
                        if (string.IsNullOrEmpty(sLotid))
                            continue;

                        DataRow dr = dtInLot.NewRow();
                        dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtInLot.Rows.Add(dr);

                        if ( string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME"))))
                        {
                            // 출고예정시간이 없는경우 강제출고가 불가합니다.
                            Util.Alert("FM_ME_0506");
                            return;
                        }

                        //else if (!(string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")))) && (Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME")).ToString()) - DateTime.Now).TotalSeconds > 0)
                        //{
                        //    //출고예정시간이 미달인 경우 강제출고가 불가합니다.
                        //    Util.Alert("FM_ME_0507");
                        //    return;
                        //}
                        else
                        { 
                            // 강제출고요청을 하시겠습니까?
                            sMessage = "FM_ME_0094";
                        }
                    }
                    
                }
                    if (dtInLot.Rows.Count == 0)
                    {
                        //선택된 데이터가 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            { }
                        });
                        return;
                    }

                    Util.MessageConfirm(sMessage, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_FORCE_OUT_MULTI_MB", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    //강제출고 요청을 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0092");
                                    GetList();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }
                            }, ds);

                        }
                    });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSampleOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                         Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                    {
                        alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                    }
                }

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                    }
                }

                isNoSampleOut = false;

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));

                //DataRow drIn = dtInData.NewRow();
                //drIn["USERID"] = LoginInfo.USERID;


                //DataTable dtInLot = ds.Tables.Add("INLOT");
                //dtInLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "AGING_ISS_PRIORITY_NO")).Equals("5"))
                        {
                            Util.Alert("FM_ME_0535"); // 이미 출고요청된 tray가 있습니다.
                            return;
                        }
                        string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        // 일괄선택시 합계행 제외
                        if (string.IsNullOrEmpty(sLotid))
                            continue;

                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtInData.Rows.Add(drIn);

                        if (txtStatus.Text.Equals("WAIT") &&
                            (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("7") ||
                             Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("9")))
                        {
                            isNoSampleOut = true;
                        }
                    }                    
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }

                if (isNoSampleOut)
                {
                    //Aging 공정 대기 상태인 Tray는 Sample 출고가 불가합니다. 
                    Util.Alert("FM_ME_0445");
                    return;
                }

                //Sample 출고 하시겠습니까?
                Util.MessageConfirm("FM_ME_0323", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_OUT_MB", "INDATA", "OUTDATA,OUT_SAMPLE_PORT", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "0")
                                {
                                    //Sample 출고 지시를 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0065");
                                    GetList();
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString() == "-1")
                                {
                                    //Sample 출고는 Aging 공정에서만 가능합니다. 
                                    Util.Alert("FM_ME_0444"); 
                                }

                                if (bizResult.Tables["OUT_SAMPLE_PORT"].Rows.Count > 0)
                                {
                                    //POPUP
                                    FCS002_005_03 wndRunStart = new FCS002_005_03();
                                    wndRunStart.FrameOperation = FrameOperation;

                                    if (wndRunStart != null)
                                    {
                                        object[] Parameters = new object[1];
                                        Parameters[0] = result;

                                        C1WindowExtension.SetParameters(wndRunStart, Parameters);
                                    }
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
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSampleRel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                         Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                    {
                        alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                    }
                }

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                    }
                }

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID", typeof(string));
                dtInData.Columns.Add("LOTID", typeof(string));

                //DataRow drIn = dtInData.NewRow();
                //drIn["USERID"] = LoginInfo.USERID;

                string TrayList = string.Empty;

                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "AGING_ISS_PRIORITY_NO")).Equals("7"))
                        {
                            Util.Alert("FM_ME_0548"); // Sample 출고 요청상태가 아닌 tray가 있습니다.
                            return;
                        }
                        string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        // 일괄선택시 합계행 제외
                        if (string.IsNullOrEmpty(sLotid))
                            continue;


                        if(!TrayList.IsNullOrEmpty())
                        {
                            TrayList += "," + Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID"));
                        }
                        else
                        {
                            TrayList = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID"));
                        }

                        DataRow drIn = dtInData.NewRow();
                        drIn["USERID"] = LoginInfo.USERID;
                        drIn["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                        dtInData.Rows.Add(drIn);

                    }
                }

                if (dtInData.Rows.Count == 0)
                {
                    //선택된 데이터가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }


                if (!TrayList.IsNullOrEmpty())
                {
                    DataTable dtInData1 = new DataTable();
                    dtInData1.Columns.Add("CSTID", typeof(string));


                    DataRow drIn1 = dtInData1.NewRow();
                    drIn1["CSTID"] = TrayList;

                    dtInData1.Rows.Add(drIn1);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SAMPLE_OUT_CHK_MB", "RQSTDT", "RSLTDT", dtInData1);

                    if (dtRslt.Rows.Count > 0)
                    {
                        Util.Alert("FM_ME_0549"); // Sample 출고 취소가 불가능한 Tray가 있습니다.
                        return;
                    }

                }
              







                //Sample 해제하시겠습니까?
                Util.MessageConfirm("FM_ME_0324", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_SAMPLE_IN_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //Sample 해제를 완료하였습니다.
                                Util.MessageInfo("FM_ME_0067");
                                GetList();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtTrayID.Text.Trim() == string.Empty)
                    return;

                CheckTray(txtTrayID.Text.Trim());
                txtTrayID.SelectAll();
                txtTrayID.Focus();
            }
        }

        private void txtTrayID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string _ValueToFind = string.Empty;
                    bool bFlag = false;

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        for (int j = 0; j < dgTrayList.Rows.Count; j++)
                        {
                            if (sPasteStrings[i] == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CSTID")))
                            {
                                DataTableConverter.SetValue(dgTrayList.Rows[j].DataItem, "CHK", true);
                                bFlag = true;
                            }
                        }
                    }

                    if (!bFlag)
                    {
                        //해당 Tray ID가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0260"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTrayID.SelectAll();
                                txtTrayID.Focus();
                            }
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["G_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["O_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["R_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["Q_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["I_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["L_GRD_QTY"].Visibility = Visibility.Visible;
            dgTrayList.Columns["X_GRD_QTY"].Visibility = Visibility.Visible;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            dgTrayList.Columns["G_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["O_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["R_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["Q_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["I_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["L_GRD_QTY"].Visibility = Visibility.Collapsed;
            dgTrayList.Columns["X_GRD_QTY"].Visibility = Visibility.Collapsed;
        }

        private void chkEWGrade_Checked(object sender, RoutedEventArgs e)
        {
            DataTable newdt = DataTableConverter.Convert(dgTrayList.ItemsSource);
            DataRow[] dr = newdt.Select("E_W_GRD_QTY > 1");

            if (dr.Length == 0)
            {
                Util.gridClear(dgTrayList);
                return;
            }
            Util.GridSetData(dgTrayList, dr.CopyToDataTable(), null);
            // DataTable dt = newdt.Clone();


            // for(int i=0;i<dr.Length; i++)
            // {
            //     // DataRow newrow = dr[i];
            //     // dt.Rows.Add(newrow);
            //     dt.ImportRow(dr[i]);
            // }
            //// Util.GridSetData(dgTrayList, dt, null);
            // Util.GridSetData(dgTrayList, dt, FrameOperation, true);


           // AddRowSumnPerQty(dgTrayList);

            gdConditionArea.IsEnabled = true;
        }

        private void chkEWGrade_Unchecked(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgTrayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayList == null || dgTrayList.CurrentRow == null || !dgTrayList.Columns.Contains("CSTID") || !dgTrayList.Columns.Contains("ROUTID"))
                {
                    return;
                }
                
            //    if (dgTrayList.CurrentColumn.Name.Equals("CSTID"))
            //    {
                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters); //Tray 정보조회(FORM - 소형)
                //}
                //else if (dgTrayList.CurrentColumn.Name.Equals("ROUTID"))
                //{
                //    DataTable dtRqst = new DataTable();
                //    dtRqst.TableName = "RQSTDT";
                //    dtRqst.Columns.Add("TRAY_NO", typeof(string));

                //    DataRow dr = dtRqst.NewRow();
                //    dr["TRAY_NO"] = DataTableConverter.GetValue(dgTrayList.CurrentRow.DataItem, "LOTID").ToString();
                //    dtRqst.Rows.Add(dr);

                //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LINE_MODEL_ROUTE_FROM_TRAY_MB", "RQSTDT", "RSLTDT", dtRqst);

                //    //Route 관리
                //    //Util.OpenRouteForm(dtRslt.Rows[0]["LINE_ID"].ToString(), dtRslt.Rows[0]["MODEL_ID"].ToString(), dtRslt.Rows[0]["ROUTE_ID"].ToString());
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotID.Text.Trim() == string.Empty)
                        return;

                    if (chkCellLot.IsChecked == true)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                        dtRqst.Columns.Add("PROCID", typeof(string));
                        dtRqst.Columns.Add("EQSGID", typeof(string));
                        dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                        dtRqst.Columns.Add("ROUTID", typeof(string));
                        dtRqst.Columns.Add("ROUTE_TYPE_DG", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["PROD_LOTID"] = Util.GetCondition(txtLotID);
                        dr["PROCID"] = (string.IsNullOrEmpty(_sOPER) ? null : _sOPER);
                        dr["EQSGID"] = (string.IsNullOrEmpty(_sLINE_ID) ? null : _sLINE_ID);
                        dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sMODEL_ID) ? null : _sMODEL_ID);
                        dr["ROUTID"] = (string.IsNullOrEmpty(_sROUTE_ID) ? null : _sROUTE_ID);
                        dr["ROUTE_TYPE_DG"] = (string.IsNullOrEmpty(_sROUTE_TYPE_DG) ? null : _sROUTE_TYPE_DG);
                        dtRqst.Rows.Add(dr);

                        ShowLoadingIndicator();
                        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_BY_CELL_LOT_MB", "RQSTDT", "RSLTDT", dtRqst);

                        for (int i = 0; i < SearchResult.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgTrayList.Rows.Count; j++)
                            {
                                if (SearchResult.Rows[i]["CSTID"].ToString() == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[j].DataItem, "CSTID")))
                                {
                                    DataTableConverter.SetValue(dgTrayList.Rows[j].DataItem, "CHK", true);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dgTrayList.Rows.Count; i++)
                        {
                            if (txtLotID.Text.Trim() == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROD_LOTID")))
                            {
                                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                            }
                        }
                    }
                }

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

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                // 일괄선택시 합계행 제외
                if (string.IsNullOrEmpty(sLotid))
                    continue;

                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
            }
        }

        private void chkAll_UnChecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", false);
            }
        }

        private void dgTrayList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                if (!Util.NVC(dgTrayList.GetCell(e.Row.Index, dgTrayList.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계")))
                    && !Util.NVC(dgTrayList.GetCell(e.Row.Index, dgTrayList.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("PERCENT_VAL"))))
                {
                    tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }

            }

        }

        private void dgTrayList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            //C1DataGrid grid = sender as C1DataGrid;
            //if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 3)
            //{
            //    var _mergeList = new List<DataGridCellsRange>();

            //    _mergeList.Add(new DataGridCellsRange(grid.GetCell(grid.Rows.Count - 2, 1), grid.GetCell(grid.Rows.Count - 2, 8)));
            //    _mergeList.Add(new DataGridCellsRange(grid.GetCell(grid.Rows.Count - 1, 1), grid.GetCell(grid.Rows.Count - 1, 8)));

            //    foreach (var range in _mergeList)
            //    {
            //        e.Merge(range);
            //    }
            //}
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")).Equals(string.Empty)) return;

            if (checkEvent.Equals(true))
            {
                checkEvent = false;
                if (chkEWGrade.IsChecked == false && (chkGroup.IsChecked == true || 
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "PROC_GR_CODE")).Equals("4"))) // 고온 aging 개별 선택 불가
                {
                    SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")), true);
                }
                checkEvent = true;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int rowIndex = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (dgTrayList.Rows[rowIndex].DataItem == null ||
                Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")).Equals(string.Empty)) return;

            if (checkEvent.Equals(true))
            {
                checkEvent = false;
                if (chkEWGrade.IsChecked == false && (chkGroup.IsChecked == true ||
                    Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "PROC_GR_CODE")).Equals("4"))) // 고온 aging 개별 선택 불가
                {
                    SetBoxIdCheck(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[rowIndex].DataItem, "EQP_ID")), false);
                }
                checkEvent = true;
            }
        }


        #endregion

        #region Method

        private void CheckTray(string sTray)
        {
            bool bFlag = false;
            try
            {
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                    if (sTray == Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")))
                    {
                        DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                        bFlag = true;
                    }
                }

                if (!bFlag)
                {
                    //해당 Tray ID가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0260"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayID.SelectAll();
                            txtTrayID.Focus();
                        }
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetList()
        {
            try
            {
                chkAll.IsChecked = false;
                chkCellLot.IsChecked = false;
                chkEWGrade.IsChecked = false;
                chkDetail.IsChecked = false;

                dtBoxList.Rows.Clear();

                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
                    _timer.Tick += new EventHandler(timer_Tick);
                }

                sec = 0;
                _timer.Start();

                dgTrayList.ClearRows();

                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROD_LOTID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("ROUTID", typeof(string));
                dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));
                dt.Columns.Add("MDLLOT_ID", typeof(string));
                dt.Columns.Add("SPECIAL_YN", typeof(string));
                dt.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                dt.Columns.Add("AGING_EQP_ID", typeof(string));
                dt.Columns.Add("OP_PLAN_TIME", typeof(string));

                //dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dt.Columns.Add("ABNORM_FLAG", typeof(string));
                dt.Columns.Add("LOTTYPE", typeof(string));


                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (string.IsNullOrEmpty(txtLotID.Text))
                    dr["PROD_LOTID"] = (string.IsNullOrEmpty(_sLOT_ID) ? null : "%" + _sLOT_ID + "%");
                else
                    dr["PROD_LOTID"] =  "%" + txtLotID.Text + "%";
                dr["PROCID"] = (string.IsNullOrEmpty(_sOPER) ? null : _sOPER);
                dr["EQSGID"] = (string.IsNullOrEmpty(_sLINE_ID) ? null : _sLINE_ID);
                dr["ROUTID"] = (string.IsNullOrEmpty(_sROUTE_ID) ? null : _sROUTE_ID);
                dr["TRAY_OP_STATUS_CD"] = (string.IsNullOrEmpty(_sStatus) ? null : _sStatus);
                dr["MDLLOT_ID"] = (string.IsNullOrEmpty(_sMODEL_ID) ? null : _sMODEL_ID);
                dr["SPECIAL_YN"] = (string.IsNullOrEmpty(_sSPECIAL_YN) ? null : _sSPECIAL_YN);
                dr["ROUTE_TYPE_DG"] = (string.IsNullOrEmpty(_sROUTE_TYPE_DG) ? null : _sROUTE_TYPE_DG);
                dr["AGING_EQP_ID"] = (string.IsNullOrEmpty(_sAgingEqpID) ? null : _sAgingEqpID);
                dr["OP_PLAN_TIME"] = (string.IsNullOrEmpty(_sOpPlanTime) ? null : _sOpPlanTime);
                dr["LOTTYPE"] = (string.IsNullOrEmpty(_sLotType) ? null : _sLotType);

                dt.Rows.Add(dr);

                gdConditionArea.IsEnabled = false;

                // Background 처리 완료시 dgTrayList_ExecuteDataCompleted 이벤트 호출
                dgTrayList.ExecuteService("BR_GET_TRAY_LIST_MB", "INDATA", "OUTDATA", dt, true);

                btnForceOut.Visibility = Visibility.Visible; // 임시처리
                //btnSet();
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

        private void SetBoxIdCheck(string boxId, bool isCheck)
        {
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).Equals(boxId))
                {
                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", isCheck);
                }
            }
        }

        private void btnSet()
        {
            Visibility vAging = Visibility.Collapsed; 
            for(int i = 0; i<dgTrayList.Rows.Count;i++)
            {
                // 상온 aging 일때만 강제출고 버튼 표시
                if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "PROC_GR_CODE")).Equals("3"))
                    vAging = Visibility.Visible;

            }

            btnForceOut.Visibility = vAging;
        }

        private DataTable GetLowVolt()
        {
            DataTable dtRslt = new DataTable();

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = _sLOT_ID == string.Empty ? null : _sLOT_ID;
                dr["PROCID"] = _sOPER == string.Empty ? null : _sOPER;
                dr["EQSGID"] = _sLINE_ID == string.Empty ? null : _sLINE_ID;
                dr["MDLLOT_ID"] = _sMODEL_ID == string.Empty ? null : _sMODEL_ID;
                dr["ROUTID"] = _sROUTE_ID == string.Empty ? null : _sROUTE_ID;
                dr["SPCL_FLAG"] = _sSPECIAL_YN == string.Empty ? null : _sSPECIAL_YN;

                dtRqst.Rows.Add(dr);
                ShowLoadingIndicator();

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LIST_LOW_VOLT_GRADE_MB", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();

            }
            return dtRslt;
        }

        private void InitText()
        {
            txtLine.Text = _sLINE_NAME;
            txtModel.Text = _sMODEL_NAME;
            txtOp.Text = _sOPER_NAME;
            txtRoute.Text = _sROUTE_NAME;
            txtSpecialYN.Text = _sSPECIAL_YN;
            txtStatus.Text = _sStatusName;
            txtAgingEqpName.Text = _sAgingEqpID;
            txtLotID.Text = _sLOT_ID;
            txtLotType.Text = _sLotTypeName;
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

        private void AddRowSumnPerQty(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dtTemp = ConvertToDataTable(datagrid);

            int iInputCol = dtTemp.Columns["INPUT_SUBLOT_QTY"].Ordinal;
            int iWipCol = dtTemp.Columns["WIP_QTY"].Ordinal;
            int iSumFromCol = dtTemp.Columns["A_GRD_QTY"].Ordinal;
            int iSumToCol = dtTemp.Columns["Z_GRD_QTY"].Ordinal;
            int iSumFromCol2 = dtTemp.Columns["GRD_1_QTY"].Ordinal;
            int iSumToCol2 = dtTemp.Columns["GRD_2_QTY"].Ordinal;
            int iSumInputQty = 0;
            int iTotalSumQty = 0;
            int iSumQty = 0;
            double ColValue = 0;
            string ColName = string.Empty;

            DataRow row_sum = dtTemp.NewRow();
            row_sum["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");
            for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
            {
                ColName = dtTemp.Columns[iCol].ColumnName;

                if (iCol.Equals(iInputCol) || iCol.Equals(iWipCol) || (iCol >= iSumFromCol && iCol <= iSumToCol) || (iCol >= iSumFromCol2 && iCol <= iSumToCol2))
                {
                    for (int iRow = 0; iRow < dtTemp.Rows.Count; iRow++)
                    {
                        iSumQty = Convert.ToInt32(Util.NVC(dtTemp.Rows[iRow][ColName]));
                        iTotalSumQty = iTotalSumQty + iSumQty;
                    }

                    row_sum[ColName] = Convert.ToString(iTotalSumQty);

                    if (iCol.Equals(iInputCol))
                    {
                        iSumInputQty = iTotalSumQty;
                    }
                }
                iTotalSumQty = 0;
                iSumQty = 0;
            }

            DataRow row_pre = dtTemp.NewRow();
            row_pre["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("PERCENT_VAL");
            for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
            {
                ColName = dtTemp.Columns[iCol].ColumnName;

                if (iCol.Equals(iWipCol) || (iCol >= iSumFromCol && iCol <= iSumToCol) || (iCol >= iSumFromCol2 && iCol <= iSumToCol2))
                {
                    for (int iRow = 0; iRow < dtTemp.Rows.Count; iRow++)
                    {
                        iSumQty = Convert.ToInt32(dtTemp.Rows[iRow][ColName]);
                        iTotalSumQty = iTotalSumQty + iSumQty;
                    }

                    ColValue = Convert.ToDouble(iTotalSumQty) / Convert.ToDouble(iSumInputQty) * 100;

                    if (ColValue == 0)
                    {
                        row_pre[ColName] = "0.00";
                    }
                    else
                    {
                        row_pre[ColName] = ColValue.ToString("#,#.00");
                    }
                }
                iTotalSumQty = 0;
                iSumQty = 0;
            }

            dtTemp.Rows.Add(row_sum);
            dtTemp.Rows.Add(row_pre);

            Util.GridSetData(datagrid, dtTemp, FrameOperation, true);

            btnSet();
        }

        public static DataTable ConvertToDataTable(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    dt = ((DataView)dg.ItemsSource).Table;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        #endregion

        private void btnManualOut_Click(object sender, RoutedEventArgs e)
        {
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

            FCS002_026_MANUAL_OUT manualOut = new FCS002_026_MANUAL_OUT();
            manualOut.FrameOperation = FrameOperation;
            if (manualOut != null)
            {
                string sTrayList = string.Empty;
                for (int i = 0; i < dgTrayList.GetRowCount(); i++)
                {
                    string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                    // 일괄선택시 합계행 제외
                    if (string.IsNullOrEmpty(sLotid))
                        continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                                  sTrayList += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")) + "|";
                    }
                }

                if (string.IsNullOrEmpty(sTrayList))
                {
                    Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                    return;
                }

                object[] Parameters = new object[2];
                Parameters[0] = sTrayList;
                Parameters[1] = string.Empty;

                C1WindowExtension.SetParameters(manualOut, Parameters);

                manualOut.Closed += new EventHandler(ManualOut_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => manualOut.ShowModal()));
            }
        }

        private void ManualOut_Closed(object sender, EventArgs e)
        {
            FCS002_026_MANUAL_OUT window = sender as FCS002_026_MANUAL_OUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetList();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnAgingManualOut_Click(object sender, RoutedEventArgs e)
        {
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

            ArrayList alEqp = new ArrayList(); //선택된 모든 설비를 저장하면서 aging op 체크
            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if ((Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("True") ||
                     Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1")) && (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")))))
                {
                    alEqp.Add(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID")).ToString();
                }
            }

            for (int i = 0; i < dgTrayList.Rows.Count; i++)
            {
                if (alEqp.Contains(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "EQP_ID"))))
                {
                    DataTableConverter.SetValue(dgTrayList.Rows[i].DataItem, "CHK", true);
                }
            }

            FCS002_026_MANUAL_OUT manualOut = new FCS002_026_MANUAL_OUT();
            manualOut.FrameOperation = FrameOperation;
            if (manualOut != null)
            {
                string sTrayList = string.Empty;
                for (int i = 0; i < dgTrayList.Rows.Count; i++)
                {
                   
                    string sLotid = Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "LOTID"));
                    // 일괄선택시 합계행 제외
                    if (string.IsNullOrEmpty(sLotid))
                        continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE") ||
                        Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "AGING_ISS_PRIORITY_NO")).Equals("5"))
                        {
                            Util.Alert("FM_ME_0535");
                            return;
                        }


                        if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "OP_PLAN_TIME"))))
                        {
                            // 출고예정시간이 없는경우 수동출고가 불가합니다.
                            Util.Alert("FM_ME_0547");
                            return;
                        }


                        sTrayList += Util.NVC(DataTableConverter.GetValue(dgTrayList.Rows[i].DataItem, "CSTID")) + "|";
                    }
                }

                if (string.IsNullOrEmpty(sTrayList))
                {
                    Util.MessageInfo("FM_ME_0081");  //Tray를 선택해주세요.
                    return;
                }

                object[] Parameters = new object[2];
                Parameters[0] = sTrayList;
                Parameters[1] = "Y";

                C1WindowExtension.SetParameters(manualOut, Parameters);

                manualOut.Closed += new EventHandler(ManualOut_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => manualOut.ShowModal()));
            }
        }
    }

}
