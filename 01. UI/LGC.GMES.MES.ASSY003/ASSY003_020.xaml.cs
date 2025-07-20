/*************************************************************************************
 Created Date : 2017.11.25
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - VD (STP 후) 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.25  INS 김동일K : Initial Created.
  2022.04.06  장희만      : C20220410-000011 - VD 예약 화면에 V/D(STD후) Process 추가, 재공이동, DISPATCH 버튼 비활성화
  2023.06.25  조영대      : 설비 Loss Level 2 Code 사용 체크 및 변환
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_020.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_020 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool bchkWait = true;
        private bool bchkRun = true;
        private bool bchkEqpEnd = true;
        private bool bchkEnd = false;

        private bool bLoaded = true;

        private bool bTestMode = false;
        private string sTestModeType = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        string inputLot = string.Empty;
        DataTable dtMain = new DataTable();
        CurrentLotInfo cLot = new CurrentLotInfo();

        private UC_WORKORDER_LINE winWo = new UC_WORKORDER_LINE();

        #region CurrentLotInfo
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _txtShift = string.Empty;
        private string _txtWorker = string.Empty;
        private string _predver = string.Empty;
        private string _wipdttm = string.Empty;
        private string _wipnote = string.Empty;
        private string _resonqty = "0";
        private string _inputqty = string.Empty;
        private string _wipqty = string.Empty;
        private string _txtissue = string.Empty;
        private string cancelflag = string.Empty;
        private string _REMARK = string.Empty;
        private string _VERSION = string.Empty;
        #endregion
        
        public ASSY003_020()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            //재공이동, DISPATCH 버튼 비활성화, 재공이동은 VD예약 화면으로 이동
            btnMoveWip.Visibility = Visibility.Collapsed;
            btnDispatch.Visibility = Visibility.Collapsed;

        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        private void InitCombo()
        {
            String[] sFilter = { Process.VD_LMN_AFTER_STP, LoginInfo.CFG_AREA_ID };
            //C1ComboBox[] cboLineChild = { cboVDEquipment };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
            
            ////설비
            //String[] sFilter2 = { Process.VD_LMN_AFTER_STP, cboVDEquipmentSegment.SelectedValue.ToString() };
            //C1ComboBox[] cboEquipmentParent = { cboVDEquipmentSegment };
            //combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        
        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }
        
        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":

                                chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);

                                // 동일 배치 전체 선택
                                if (!chk.IsChecked.Value)
                                {
                                    chkAll.IsChecked = false;
                                    ClearData();


                                    // 진행중인 경우..
                                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("PROC") ||
                                        Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("EQPT_END"))
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_BTCH_WRK_NO")).Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO"))))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    // 진행중인 경우..
                                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("PROC") ||
                                        Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("EQPT_END"))
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_BTCH_WRK_NO")).Equals(Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO"))))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
                                            }
                                            else
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "WIPSTAT")).Equals("END"))
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "WIPSTAT")).Equals("END"))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                        //int preValue = (int)e.Cell.Value;

                                        //Util.DataGridCheckAllUnChecked(dg);

                                        //if (preValue > 0) e.Cell.Value = true;
                                        //else e.Cell.Value = false;
                                    }
                                    else
                                    {
                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO")).Equals(""))
                                            {
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }

                                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                                if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                                {
                                    chkAll.IsChecked = true;
                                }

                                chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            checkAllProcess();
            if ((bool)chkRun.IsChecked)
            {
                ProcessDetail();
            }
        }

        private void checkAllProcess()
        {
            if (dgLotInfo == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
                // C1.WPF.DataGrid.DataGridRow row = dgLotInfo.Rows[idx];
                // DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {

            uncheckProcess();

        }

        private void uncheckProcess()
        {
            if (dgLotInfo == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            ClearData();
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //Util.Alert("라인을 선택 하세요.");
                    Util.MessageValidation("SFU1223");
                    HiddenLoadingIndicator();
                    return;
                }

                if (cboVDEquipment.SelectedIndex < 0 || cboVDEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //Util.Alert("설비를 선택 하세요.");
                    Util.MessageValidation("SFU1673");
                    HiddenLoadingIndicator();
                    return;
                }

                if (bLoaded == false)
                {
                    if (!(bool)chkRun.IsChecked && !(bool)chkWait.IsChecked && !(bool)chkEnd.IsChecked)
                    //    if (!(bool)chkRun.IsChecked && !(bool)chkEqpEnd.IsChecked && !(bool)chkWait.IsChecked)
                    {
                        //Util.Alert("LOT 상태 선택 조건을 하나 이상 선택하세요.");
                        Util.MessageValidation("SFU1370");
                        HiddenLoadingIndicator();
                        return;
                    }
                }

                GetProductLot();
                GetWorkOrder();
                ClearData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void dg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    if (cell == null || cell.Presenter == null || cell.Presenter.Content == null) return;
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CHK")).Equals("1"))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
                        {
                            ProcessDetail();
                        }

                    }

                }
            }));
        }
        #endregion
        
        #region MainWindow
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                //Util.Alert("선택 된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    // WAIT 상태에서 RESERVE 상태로 변경 되었기때문에 수정
                    //if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("WAIT"))
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("RESERVE"))
                    {
                        //Util.MessageValidation("SFU1494"); // 대기 LOT이 아닙니다.  대기 LOT을 선택해 주세요.
                        Util.MessageValidation("SFU8490"); // 예약 LOT이 아닙니다.  대기 LOT을 선택해 주세요.
                        return;
                    }
                }
            }

            Util.MessageConfirm("SFU1435", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("IN_EQP");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                                                
                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;

                        inData.Rows.Add(row);

                        DataTable inMtrl = indataSet.Tables.Add("IN_LOT");
                        inMtrl.Columns.Add("LOTID", typeof(string));
                        inMtrl.Columns.Add("CSTID", typeof(string));

                        DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                        foreach (DataRow _iRow in dtProductLot.Rows)
                        {
                            if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                            {
                                row = inMtrl.NewRow();

                                row["LOTID"] = _iRow["LOTID"];
                                inMtrl.Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_STPVD_S", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

                                _LOTID = string.Empty;
                                //chkRun.IsChecked = true;
                                //chkWait.IsChecked = false;
                                //chkEqpEnd.IsChecked = false;
                                GetProductLot();
                                chkAll.IsChecked = false;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        HiddenLoadingIndicator();
                    }
                }
            });

        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                //Util.Alert("선택된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }

            string sTmp = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                    {
                        Util.MessageValidation("SFU3172");  // 진행중인 Lot만 취소 할 수 있습니다.
                        return;
                    }

                    //if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    //{
                    //    Util.MessageValidation("SFU4288", Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));  // 선택한 Lot[%]은 동일한 작업배치번호가 아닙니다. 같은 작업배치번호 단위로 시작취소할 수 있습니다.
                    //    return;
                    //}
                }
                //else
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                //    {
                //        Util.MessageValidation("SFU4289");  // 동일한 작업배치번호를 모두 선택 후 시작취소하세요.
                //        return;
                //    }
                //}
            }

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataTable dt = new DataTable();
                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("EQPTID", typeof(string));
                        dt.Columns.Add("LOTID", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));
                        dt.Columns.Add("IFMODE", typeof(string));

                        DataRow row = dt.NewRow();
                        DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                        foreach (DataRow _iRow in dtProductLot.Rows)
                        {
                            if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                            {
                                row = dt.NewRow();
                                row["LOTID"] = _iRow["LOTID"];
                                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                row["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                                row["USERID"] = LoginInfo.USERID;
                                row["IFMODE"] = IFMODE.IFMODE_OFF;
                                dt.Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_START_LOT_VD_NJ", "RQSTDT", null, dt);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업 시작 취소"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                        Util.MessageInfo("SFU1839");
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
            });
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
           
            ApplyPermissions();

            SetWorkOrderWindow();

            if (chkWait != null && chkWait.IsChecked.HasValue)
                chkWait.IsChecked = true;
            if (chkRun != null && chkRun.IsChecked.HasValue)
                chkRun.IsChecked = true;
            if (chkEnd != null && chkEnd.IsChecked.HasValue)
                chkEnd.IsChecked = false;
            //if (chkEqpEnd != null && chkEqpEnd.IsChecked.HasValue)
            //    chkEqpEnd.IsChecked = true;

            chkPrint.IsChecked = true;

            bLoaded = false;
        }
        
        #endregion


        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnConfirm);
            listAuth.Add(btnMoveWip);
            listAuth.Add(btnRunCancel);
            listAuth.Add(btnRunStart);
            listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {

            if (grdWorkOrder.Children.Count == 0)
            {
                winWo.FrameOperation = FrameOperation;
                winWo._UCParent = this;
                winWo.PROCID = Process.VD_LMN_AFTER_STP;
                grdWorkOrder.Children.Add(winWo);

            }

        }

        private void GetWorkOrder()
        {
            if (winWo == null)
                return;

            winWo.EQPTSEGMENT = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
            winWo.EQPTID = Convert.ToString(cboVDEquipment.SelectedValue);
            winWo.PROCID = Process.VD_LMN_AFTER_STP;

            winWo.GetWorkOrder();
        }

        private void GetProductLot()
        {
            try
            {
                string sInQuery = string.Empty;

                if (chkWait.IsChecked.HasValue && (bool)chkWait.IsChecked)
                {
                    //sInQuery = "WAIT"; 예약 상태로 변경
                    sInQuery = "RESERVE";

                    //if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                    //    dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Collapsed;
                }

                if (chkRun.IsChecked.HasValue && (bool)chkRun.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "PROC";
                    else
                        sInQuery = sInQuery + ",PROC";

                    //if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                    //    dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
                }

                if (chkEnd.IsChecked.HasValue && (bool)chkEnd.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "END";
                    else
                        sInQuery = sInQuery + ",END";
                }

                //if (chkEqpEnd.IsChecked.HasValue && (bool)chkEqpEnd.IsChecked)
                //{
                //    if (sInQuery.Equals(""))
                //        sInQuery = "EQPT_END";
                //    else
                //        sInQuery = sInQuery + ",EQPT_END";

                //    if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                //        dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
                //}
                //sInQuery = "PROC,EQPT_END";
                if (bLoaded == true)
                {
                    //sInQuery = "WAIT,PROC,EQPT_END,END";대기에서 예약 상태로 변경
                    sInQuery = "RESERVE,PROC,EQPT_END,END";
                }
                


DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);
                newRow["PROCID"] = Process.VD_LMN_AFTER_STP;
                newRow["EQSGID"] = Util.NVC(cboVDEquipmentSegment.SelectedValue);
                newRow["WIPSTAT"] = sInQuery;

                txtRemark.Text = _REMARK;

                inTable.Rows.Add(newRow);

                dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VDSTP", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgLotInfo, dtMain, FrameOperation);

                if (dtMain?.Select("WIPSTAT IN ('PROC', 'EQPT_END')").Length > 0)
                {
                    if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgLotInfo.Columns.Contains("EQPT_BTCH_WRK_NO"))
                        dgLotInfo.Columns["EQPT_BTCH_WRK_NO"].Visibility = Visibility.Collapsed;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }
        
        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        
        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            if (!_Util.GetDataGridCheckValue(dgLotInfo, "CHK", iRow))
            {
                return;
            }

        }

        private void ProcessDetail()
        {
            if (dgLotInfo == null) { return; }
            if (dgLotInfo.Rows.Count == 0) { return; }
            cLot.ENDTIME_CHAR = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            int idx = _Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1261"); //선택된LOT이 없습니다
                return;
            }

            cLot.LOTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID"));
            cLot.INPUTQTY = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPQTY"));
            cLot.PRODID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID"));
            cLot.EQPTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID"));
            cLot.STARTTIME_CHAR = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "STARTTIME"));
            cLot.STATUS = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "WIPSTAT"));
            _predver = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PROD_VER_CODE"));
            _VERSION = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PROD_VER_CODE"));

            if (cLot.STATUS.Equals("EQPT_END"))
            {
                cLot.ENDTIME_CHAR = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "ENDTIME"));
            }

            cLot.EQPTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "EQPTID"));
            txtWorkdate.Text = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CALDATE"));

            txtStartTime.Text = cLot.STARTTIME_CHAR;



        }

        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //설비
            String[] sFilter2 = { Process.VD_LMN_AFTER_STP, cboVDEquipmentSegment.SelectedValue.ToString() };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidConfirm()) return;

            Util.MessageConfirm("SFU1744", (result) => //완공처리 하시겠습니까?
            {

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet indataSet = new DataSet();
                        DataTable ineqp = indataSet.Tables.Add("IN_EQP");
                        ineqp.Columns.Add("SRCTYPE", typeof(string));
                        ineqp.Columns.Add("IFMODE", typeof(string));
                        ineqp.Columns.Add("EQPTID", typeof(string));
                        ineqp.Columns.Add("USERID", typeof(string));
                        
                        DataRow row = null;

                        row = ineqp.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);                        
                        row["USERID"] = LoginInfo.USERID;

                        ineqp.Rows.Add(row);

                        DataTable INLOT = indataSet.Tables.Add("IN_LOT");
                        INLOT.Columns.Add("LOTID", typeof(string));
                        
                        DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                        foreach (DataRow _iRow in dtProductLot.Rows)
                        {
                            if (_iRow["CHK"].ToString().Equals("1") || _iRow["CHK"].ToString().Equals("True"))
                            {
                                row = INLOT.NewRow();
                                row["LOTID"] = _iRow["LOTID"];

                                INLOT.Rows.Add(row);
                            }
                        }
 
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_STPVD_S", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                                GetProductLot();
                                ClearData();                                
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        HiddenLoadingIndicator();
                    }
                }

            });
        }

        private bool ValidConfirm()
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1661"); //선택 된 LOT이 없습니다.
                return false;

            }
            
            string sTmp = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("PROC") ||
                          Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("EQPT_END")))
                    {
                        Util.MessageValidation("SFU2045"); //확정 할 수 있는 LOT상태가 아닙니다.
                        return false;
                    }

                    //if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                    //{
                    //    Util.MessageValidation("SFU4288", Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));  // 선택한 Lot[%1]은 동일한 작업배치번호가 아닙니다. 같은 작업배치번호 단위로 진행할 수 있습니다.
                    //    return false;
                    //}
                }
                //else
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "EQPT_BTCH_WRK_NO")).Equals(sTmp))
                //    {
                //        Util.MessageValidation("SFU4289");  // 동일한 작업배치번호를 모두 선택 후 진행 하세요.
                //        return false;
                //    }
                //}
            }

            return true;

        }

        private void btnMoveWip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidMoveWip()) return;

                Util.MessageConfirm("SFU1763", (result) => //이동 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            ShowLoadingIndicator();

                            DataSet indataSet = new DataSet();
                            DataTable ineqp = indataSet.Tables.Add("INDATA");
                            ineqp.Columns.Add("SRCTYPE", typeof(string));
                            ineqp.Columns.Add("IFMODE", typeof(string));
                            ineqp.Columns.Add("PROCID", typeof(string));
                            ineqp.Columns.Add("EQPTID", typeof(string));                            
                            ineqp.Columns.Add("USERID", typeof(string));
                            ineqp.Columns.Add("PROCID_TO", typeof(string));
                            ineqp.Columns.Add("EQSGID_TO", typeof(string));
                            ineqp.Columns.Add("FLOWNORM", typeof(string));

                            DataRow row = null;

                            row = ineqp.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["PROCID"] = Process.VD_LMN_AFTER_STP; // 조회된 LOT의 공정 (from 공정)
                            row["EQPTID"] = Util.NVC(cboVDEquipment.SelectedValue);                            
                            row["USERID"] = LoginInfo.USERID;
                            row["PROCID_TO"] = null;
                            row["EQSGID_TO"] = null;
                            row["FLOWNORM"] = "Y";  // 정상 흐름 여부

                            ineqp.Rows.Add(row); 

                            DataTable INLOT = indataSet.Tables.Add("INLOT");
                            INLOT.Columns.Add("LOTID", typeof(string));
                            INLOT.Columns.Add("WIPNOTE", typeof(string));

                            DataTable dtProductLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                            foreach (DataRow _iRow in dtProductLot.Rows)
                            {
                                if (_iRow["CHK"].ToString().Equals("1") || _iRow["CHK"].ToString().Equals("True"))
                                {
                                    row = INLOT.NewRow();
                                    row["LOTID"] = _iRow["LOTID"];
                                    
                                    INLOT.Rows.Add(row);
                                }
                            }

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_S", "INDATA,INLOT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    
                                    GetProductLot();
                                    ClearData();
                                    
                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    HiddenLoadingIndicator();
                                }
                            }, indataSet);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                            HiddenLoadingIndicator();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidMoveWip()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1661"); //선택 된 LOT이 없습니다.
                return false;

            }
            
            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    // V/D예약 단계 추가로 WAIT:대기에서 RESERVE:예약으로 변경
                    //if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("WAIT"))
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("RESERVE"))
                    {
                        Util.MessageValidation("SFU1869"); //재공 상태가 이동가능한 상태가 아닙니다.
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgLotInfo);
                ClearData();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        private void ClearData()
        {
            _LOTID = "";
            _EQPTID = "";
            _WIPSTAT = "";
            _txtShift = "";
            _txtWorker = "";
            _predver = "";
            _wipdttm = "";
            _wipnote = "";
            _inputqty = "";
            _wipqty = "";
            _txtissue = "";
            cancelflag = "";
            _REMARK = "";
            _VERSION = "";
            txtWorkdate.Text = "";
            txtRemark.Text = "";
            txtStartTime.Text = "";
            txtWorkdate.Text = "";
            txtWorkMinute.Text = "";
            chkAll.IsChecked = false;

        }
        
        private void cboVDEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                if (winWo != null)
                {
                    winWo.EQPTSEGMENT = cboVDEquipmentSegment.SelectedValue != null ? Convert.ToString(cboVDEquipmentSegment.SelectedValue) : "";
                    winWo.EQPTID = cboVDEquipment.SelectedValue != null ? Convert.ToString(cboVDEquipment.SelectedValue) : "";
                    winWo.PROCID = Process.VD_LMN_AFTER_STP;

                    winWo.ClearWorkOrderInfo();
                }

                // 설비 선택 시 자동 조회 처리
                if (cboVDEquipment.SelectedIndex > 0 && cboVDEquipment.Items.Count > cboVDEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        ShowLoadingIndicator();

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void chkShowSkid_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if ((sender as CheckBox).IsChecked.HasValue && (bool)(sender as CheckBox).IsChecked)
                {
                    dgcSKIDID.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkShowSkid_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if ((sender as CheckBox).IsChecked.HasValue && !(bool)(sender as CheckBox).IsChecked)
                {
                    dgcSKIDID.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Run_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed &&
            //    (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
            //   //(Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
            //   //(Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
            //   )
            //{
            //    if (chkShowSkid != null && chkShowSkid.Visibility == Visibility.Collapsed)
            //    {
            //        chkShowSkid.Visibility = Visibility.Visible;
            //    }
            //    else if (chkShowSkid != null && chkShowSkid.Visibility == Visibility.Visible)
            //    {
            //        chkShowSkid.Visibility = Visibility.Collapsed;
            //    }
            //}
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.VD_LMN_AFTER_STP ;
                sEqsg = cboVDEquipmentSegment.SelectedIndex >= 0 ? cboVDEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboVDEquipment.SelectedIndex >= 0 ? cboVDEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }
        }


        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgLotInfo);
                    ClearData();
                    
                    //chkEqpEnd.IsChecked = false;


                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkWait == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkWait_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkWait = false;
            }
        }

        private void chkRun_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgLotInfo);
                    ClearData();
                    //chkEqpEnd.IsChecked = false;

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkRun == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkRun_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkRun = false;
            }
        }

        private void chkEqpEnd_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgLotInfo);
                    ClearData();
                    //chkWait.IsChecked = false;
                    //chkRun.IsChecked = false;

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkEqpEnd == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkEqpEnd_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkEqpEnd = false;
            }
        }

        private void btnDispatch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            Util.MessageConfirm("SFU4314", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Print();                    
                }
            });
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgLotInfo, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "WIPSTAT")).Equals("END"))
                    {
                        Util.MessageValidation("SFU4321"); // 완공 LOT이 아닙니다. 완공 LOT을 선택해 주세요.
                        return bRet;
                    }
                }
            }

            bRet = true;

            return bRet;
        }

        private void SetDispatch(string sBoxID, decimal dQty)
        {
            try
            {
                DataSet indataSet = _Biz.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                newRow["EQSGID"] = cboVDEquipmentSegment.SelectedValue.ToString();
                newRow["REWORK"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                newRow = inDataTable.NewRow();
                newRow["LOTID"] = sBoxID;
                newRow["ACTQTY"] = dQty;
                newRow["ACTUQTY"] = 0;
                newRow["WIPNOTE"] = "";

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Print()
        {
            try
            {
                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnDispatch.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                DataTable dtRslt = new DataTable();

                for (int i = 0; i < dgLotInfo.Rows.Count - dgLotInfo.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgLotInfo, "CHK", i)) continue;

                    dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "LOTID")));

                    if (dtRslt == null || dtRslt.Rows.Count < 1) continue;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩
                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    //dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    {
                        dicParam.Add("MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(winWo.dgWorkOrder.Rows[_Util.GetDataGridCheckFirstRowIndex(winWo.dgWorkOrder, "CHK")].DataItem, "MKT_TYPE_CODE")));
                        dicParam.Add("CSTID", Util.NVC(dtRslt.Rows[0]["CSTID"]));
                    }

                    dicList.Add(dicParam);

                    //if (chkPrint.IsChecked.HasValue && (bool)chkPrint.IsChecked)
                    if (!(bool)chkPrint.IsChecked)
                    {
                        SetDispatch(Util.NVC(dtRslt.Rows[0]["LOTID"]), Convert.ToDecimal(Util.NVC(dtRslt.Rows[0]["WIPQTY"])));
                    }
                }

                if (chkPrint.IsChecked.HasValue && (bool)chkPrint.IsChecked)
                {
                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD();
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STP;
                        Parameters[2] = cboVDEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboVDEquipment.SelectedValue.ToString();
                        //Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
                }
                else
                {
                    btnSearch_Click(null, null);
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnDispatch.IsEnabled = true;
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            btnSearch_Click(null, null);
        }

        private void chkEnd_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgLotInfo);
                    ClearData();
                    //chkEqpEnd.IsChecked = false;

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkEnd == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }));
                    }
                }
            }
        }

        private void chkEnd_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboVDEquipment.Items[cboVDEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkEnd = false;
            }
        }
        
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (txtLotID.Text.Trim().Equals("")) return;

                    SetInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetInfo()
        {
            try
            {
                int iFind = 0;
                bool bFind = false;

                DataTable dtTmp = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                if (dtTmp == null) return;

                for (int i = 0; i < dtTmp.Rows.Count; i++)
                {
                    if (Util.NVC(dtTmp.Rows[i]["LOTID"]).Equals(txtLotID.Text) ||
                        Util.NVC(dtTmp.Rows[i]["CSTID"]).Equals(txtLotID.Text))
                    {
                        dtTmp.Rows[i]["CHK"] = 1;
                        //DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHK", true);
                        iFind = i;
                        
                        bFind = true;
                    }

                    // 대기가 아니면 Uncheck
                    if (!Util.NVC(dtTmp.Rows[i]["WIPSTAT"]).Equals("WAIT"))
                    {
                        dtTmp.Rows[i]["CHK"] = 0;
                    }
                }

                if (!bFind)
                    Util.MessageValidation("SFU1886"); //정보가 없습니다.

                txtLotID.Text = "";
                
                Util.GridSetData(dgLotInfo, dtTmp, FrameOperation, false);

                dgLotInfo.ScrollIntoView(iFind, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        #region 계획정지, 테스트 Run
        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void btnScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanScheduledShutdownMode()) return;

                if (bTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // 계획정지를 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanTestMode()
        {
            bool bRet = false;

            if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboVDEquipment.SelectedIndex < 0 || cboVDEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bScheduledShutdown)
            //{
            //    Util.MessageValidation("SFU4464"); // 계획정지중 입니다. 계획정지를 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CanScheduledShutdownMode()
        {
            bool bRet = false;

            if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboVDEquipment.SelectedIndex < 0 || cboVDEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (bTestMode)
            //{
            //    Util.MessageValidation("SFU4465"); // 테스트 Run 중 입니다. 테스트 Run 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void HideScheduledShutdown()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool bTest)
        {
            try
            {
                string sname = string.Empty;
                if (bTest)
                {
                    recTestMode.Fill = redBrush;
                    sname = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    sname = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
                opacityAnimation.AutoReverse = true;
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(opacityAnimation, sname);
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowTestMode()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(true);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(true);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        private bool SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string sBizName = string.Empty;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // 계획정지 loss 코드.
                }
                else
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE";

                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = cboVDEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(sBizName, "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboVDEquipment == null || cboVDEquipment.SelectedValue == null) return;
                if (Util.NVC(cboVDEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboVDEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    sTestModeType = Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }
    }
}
