/*************************************************************************************
 Created Date : 2017.11.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching Rewinding 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.22  INS 김동일K : Initial Created.
  2023.06.25  조영대      : 설비 Loss Level 2 Code 사용 체크 및 변환
  2024.08.09  김대현      : LOTID입력하고 조회시 해당 LOT만 조회되도록 수정
  2025.02.13  이민형      : 김광희 책임님 요청으로 추가기능 버튼 제거 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
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
    /// ASSY003_024.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_024 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private bool bTestMode = false;
        private string sTestModeType = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        CurrentLotInfo _CurrentLot = new CurrentLotInfo();
        //string _Lane = string.Empty;
        //string _Lane_Ptn_qty = string.Empty;
        string _InputQty = string.Empty;
        string _GoodQty_PC = string.Empty;
        string _DefectQty = string.Empty;
        string _LossQty = String.Empty;
        string _Shift = string.Empty;
        string _StartTime = string.Empty;
        string _EndTime = string.Empty;
        string _OperTime = string.Empty;
        string _Remark = string.Empty;
        string _Hold = string.Empty;
        bool defectFlag = false;
        bool lossFlag = false;
        bool chargeFlag = false;
        bool remarkFlag = false;

        string _LotProcId = string.Empty;
        string _Wipstat = string.Empty;
        //string procid = string.Empty;

        decimal inputOverrate;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public ASSY003_024()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize


        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, Process.NOTCHING_REWINDING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");

            String[] sFilter1 = { Process.NOTCHING_REWINDING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_MAIN_LEVEL");

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
            GetRewinderLot();

            //string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            //inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));

        }
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //SearchData();
                GetRewinderLot();
            }

        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                loadingIndicator.Visibility = Visibility.Collapsed;
                return;
            }

            //if (cboOperation.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU3207");  //공정을 선택하세요.
            //    return;
            //}

            GetRewinderLot();
        }
        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    //재와인더 이동
        //    if (sender == null) return;

        //    int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        //    _Util.SetDataGridUncheck(dgWipInfo, "CHK", checkIndex);

        //    if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") == -1)
        //    {
        //        Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
        //        return;
        //    }


        //    if (!Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[checkIndex].DataItem, "PROCID")).Equals(Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK")].DataItem, "PROCID"))))
        //    {
        //        Util.MessageValidation("SFU1446");  //같은 공정이 아닙니다.
        //        DataTableConverter.SetValue(dgWipInfo.Rows[checkIndex].DataItem, "CHK", false);
        //        return;
        //    }

        //}
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            _Util.SetDataGridUncheck(dgRewinderInfo, "CHK", checkIndex);
            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
            {
                Util.gridClear(dgWipReason);

                ClearData();
                return;
            }

            GetProcessDetail(checkIndex);
        }

        //private void btnMoveStart_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (sender == null) return;

        //        if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") == -1)
        //        {
        //            Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
        //            return;
        //        }

        //        //재와인더로 이동하시겠습니까?
        //        Util.MessageConfirm("SFU1872", (result) =>
        //        {
        //            if (result == MessageBoxResult.OK)
        //            {
        //                MoveProcess();
        //            }
        //        });


        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }

        //}

        //private void btnMoveCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (sender == null) return;

        //        if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
        //        {
        //            Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
        //            return;
        //        }

        //        //기존공정으로 이동하시겠습니까?
        //        Util.MessageConfirm("SFU1470", (result) =>
        //        {
        //            if (result == MessageBoxResult.OK)
        //            {
        //                MoveCancelProcess();
        //            }
        //        });


        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            defectFlag = true;
            if (GetDefectSum() == false)
            {
                C1DataGrid dataGrid = sender as C1DataGrid;
                if (dataGrid != null)
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            //if (cboOperation.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU3207");  //공정을 선택하세요.
            //    return;
            //}

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            if (defectFlag == true)
            {
                Util.MessageValidation("SFU2900");  //불량/Loss/물청 정보를 저장하세요.
                return;
            }

            if (remarkFlag == true)
            {
                Util.MessageValidation("SFU2977"); //특이사항 정보를 저장하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }

            //실적 확정 하시겠습니까?
            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }

        private void btnSaveDefect_Click(object sender, RoutedEventArgs e)
        {
            //불량정보를 저장하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect(dgWipReason);
                    //  SetDefect(dgDefect);
                }
            });
            //  SetDefect(_CurrentLot.LOTID, dgDefect);
        }

        private void btnPrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            //if (dgLotInfo.Rows.Count < 3)
            //{
            //    return;
            //}

            ////바코드를 발행하시겠습니까?
            //Util.MessageConfirm("SFU1540", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        PrintLabel(_CurrentLot.LOTID);
            //    }
            //});


        }
        private void btnCard_Click(object sender, RoutedEventArgs e)
        {
            //if (dgLotInfo.Rows.Count < 3)
            //{
            //    return;
            //}

            ////이력카드를 발행하시겠습니까?
            //Util.MessageConfirm("SFU1772", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            //        wndPopup.FrameOperation = FrameOperation;

            //        if (wndPopup != null)
            //        {
            //            object[] Parameters = new object[2];
            //            Parameters[0] = _CurrentLot.LOTID; //LOT ID
            //            Parameters[1] = Util.NVC(cboOperation.SelectedValue); //PROCESS ID

            //            C1WindowExtension.SetParameters(wndPopup, Parameters);

            //            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

            //            // RTLS (Only 자동차 2동)
            //            if (LoginInfo.CFG_AREA_ID == "E6")
            //                RunRTLS(txtEndLotID.Text, Util.NVC(cboOperation.SelectedValue));
            //        }
            //    }
            //});
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.NOTCHING_REWINDING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void OnCloseShift(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }
        #endregion

        #region Mehod
        //private void initcombo()
        //{
        //    CommonCombo _combo = new CommonCombo();
        //    String[] sFilter = { LoginInfo.CFG_AREA_ID };
        //    _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

        //    String[] sFilter2 = { "REWINDING_PROCID" };
        //    C1ComboBox[] cboEquipmentChild = { cboEquipment };
        //    _combo.SetCombo(cboOperation, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild, sFilter: sFilter2, sCase: "COMMCODE");

        //    C1ComboBox[] cboOperationParent = { cboEquipmentSegment, cboOperation };
        //    _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboOperationParent);
        //}
        private void GetRewinderLot()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                //dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.NOTCHING_REWINDING;
                row["WIPSTAT"] = Wip_State.WAIT;

                if (!string.IsNullOrEmpty(txtLOTID.Text))
                {
                    row["LOTID"] = txtLOTID.Text;
                }
                //row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_GET_REWINDER_LOT_NEW", "INDATA", "OUTDATA", dt);
                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_GET_REWINDER_LOT", "INDATA", "OUTDATA", dt);
                if (result.Rows.Count == 0)
                {
                    Util.gridClear(dgRewinderInfo);
                    return;
                }

                Util.GridSetData(dgRewinderInfo, result, FrameOperation, true);
                //dgRewinderInfo.ItemsSource = DataTableConverter.Convert(result);

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

        private string GetCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return Util.NVC(row["ATTRIBUTE1"]);
            }
            catch (Exception ex) { }

            return "";
        }
        private void SearchData()//-> 변경필요
        {
            //try
            //{
            //    if (cboEquipmentSegment.SelectedIndex < 1)
            //    {
            //        Util.MessageValidation("SFU1223");  //라인을 선택하세요.
            //        return;
            //    }

            //    //if (cboOperation.SelectedIndex < 1)
            //    //{
            //    //    Util.MessageValidation("SFU3207");  //공정을 선택하세요.
            //    //    return;
            //    //}

            //    if (cboEquipment.SelectedIndex < 1)
            //    {
            //        Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //        return;
            //    }

            //    string lotid = txtLOTID.Text;
            //    if (txtLOTID.Text.Equals(""))
            //    {
            //        Util.MessageValidation("SFU1366");  //LOT ID를 입력해주세요.
            //        return;
            //    }

            //    DataTable dt = GetLotInfo(lotid);

            //    if (!ValidationWipInfo(dt))
            //    {
            //        return;
            //    }

            //    AddRow(dgWipInfo, dt);

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    txtLOTID.Text = "";
            //}

        }
        //private bool ValidationWipInfo(DataTable dt)
        //{
        //    if (dt == null)
        //    {
        //        return false;
        //    }
        //    if (!_Wipstat.Equals(Wip_State.WAIT))
        //    {
        //        Util.MessageValidation("SFU1220");  //대기LOT이 아닙니다.
        //        return false;
        //    }

        //    for (int i = 0; i < dgWipInfo.GetRowCount(); i++)
        //    {
        //        if (Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "LOTID")).Equals(dt.Rows[0]["LOTID"]))
        //        {
        //            Util.MessageValidation("SFU2014");  //해당 LOT이 이미 존재합니다.
        //            return false;
        //        }
        //    }

        //    if (_Hold.Equals("Y"))
        //    {
        //        Util.MessageValidation("SFU1761", new object[] { txtLOTID.Text });    //{0}이 HOLD상태 입니다.
        //        txtLOTID.Text = "";
        //        return false;
        //    }

        //    if (string.Equals(Process.NOTCHING_REWINDING, dt.Rows[0]["PROCID"]))
        //    {
        //        Util.MessageValidation("SFU3200");//재와인더 공정으로 이동된 LOT입니다.
        //        txtLOTID.Text = "";
        //        return false;
        //    }

        //    // 이동 가능 공정 체크.
        //    if (!GetMoveToProcChk(Util.NVC(dt.Rows[0]["LOTID"])))
        //    {
        //        Util.MessageValidation("SFU2925");  // 이동가능한 LOT이 아닙니다.
        //        return false;
        //    }

        //    //if (!string.Equals(Process.NOTCHING_REWINDING, dt.Rows[0]["PROCID"]))
        //    //{
        //    //    Util.MessageValidation(""); // 노칭 재와인더 작업을 할 수 없는 LOT 입니다.
        //    //    return false;
        //    //}

        //    return true;
        //}

        private void AddRow(C1.WPF.DataGrid.C1DataGrid datagrid, DataTable dt)
        {
            DataTable preTable = DataTableConverter.Convert(datagrid.ItemsSource);

            if (preTable.Columns.Count == 0)
            {
                preTable = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
                {
                    preTable.Columns.Add(Convert.ToString(col.Name));
                }
            }

            DataRow row = preTable.NewRow();
            row["CHK"] = Convert.ToBoolean(false);
            row["LOTID"] = Convert.ToString(dt.Rows[0]["LOTID"]);
            row["PROCID"] = Convert.ToString(dt.Rows[0]["PROCID"]);
            row["PROCNAME"] = Convert.ToString(dt.Rows[0]["PROCNAME"]);
            row["WIPSTAT"] = Convert.ToString(dt.Rows[0]["WIPSTAT"]);
            row["WIPSNAME"] = Convert.ToString(dt.Rows[0]["WIPSNAME"]);
            row["PRJT_NAME"] = Convert.ToString(dt.Rows[0]["PRJT_NAME"]);
            row["PRODID"] = Convert.ToString(dt.Rows[0]["PRODID"]);
            row["PRODNAME"] = Convert.ToString(dt.Rows[0]["PRODNAME"]);
            row["PROD_VER_CODE"] = Convert.ToString(dt.Rows[0]["PROD_VER_CODE"]);
            row["LANE_QTY"] = Convert.ToString(dt.Rows[0]["LANE_QTY"]);
            row["WIPQTY"] = Convert.ToString(dt.Rows[0]["WIPQTY"]);
            row["WIPQTY2"] = Convert.ToString(dt.Rows[0]["WIPQTY2"]);
            row["UNIT"] = Convert.ToString(dt.Rows[0]["UNIT"]);
            row["LANE_PTN_QTY"] = Convert.ToString(dt.Rows[0]["LANE_PTN_QTY"]);
            row["WIPSEQ"] = Convert.ToString(dt.Rows[0]["WIPSEQ"]);
            //row["WIPDTTM_ST"] = Convert.ToString(dt.Rows[0]["WIPDTTM_ST"]);
            //row["WIPDTTM_ED"] = Convert.ToString(dt.Rows[0]["WIPDTTM_ED"]);
            //row["WORKDATE"] = Convert.ToString(dt.Rows[0]["WORKDATE"]);

            preTable.Rows.Add(row);

            //datagrid.ItemsSource = DataTableConverter.Convert(preTable);
            Util.GridSetData(datagrid, preTable, FrameOperation, true);
        }



        #region[BUTTON PROCESS]
        //private void MoveProcess()
        //{
        //    try
        //    {
        //        try
        //        {
        //            DataSet indataSet = new DataSet();
        //            DataTable inData = indataSet.Tables.Add("INDATA");
        //            inData.Columns.Add("SRCTYPE", typeof(string));
        //            inData.Columns.Add("IFMODE", typeof(string));
        //            inData.Columns.Add("PROCID", typeof(string));
        //            inData.Columns.Add("EQPTID", typeof(string));
        //            inData.Columns.Add("USERID", typeof(string));
        //            inData.Columns.Add("PROCID_TO", typeof(string));
        //            inData.Columns.Add("EQSGID_TO", typeof(string));
        //            inData.Columns.Add("FLOWNORM", typeof(string));

        //            DataRow row = inData.NewRow();
        //            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        //            row["IFMODE"] = IFMODE.IFMODE_OFF;
        //            row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK")].DataItem, "PROCID")); // 조회된 LOT의 공정 (from 공정)
        //            row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
        //            row["USERID"] = LoginInfo.USERID;
        //            row["PROCID_TO"] = Process.NOTCHING_REWINDING;
        //            row["EQSGID_TO"] = Util.NVC(cboEquipmentSegment.SelectedValue); ->공정선택
        //            row["FLOWNORM"] = "N";  // 정상 흐름 여부

        //            inData.Rows.Add(row);

        //            DataTable inLot = indataSet.Tables.Add("INLOT");
        //            inLot.Columns.Add("LOTID", typeof(string));

        //            for (int i = 0; i < dgWipInfo.Rows.Count; i++)
        //            {
        //                if (Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "CHK")).Equals("True"))
        //                {
        //                    row = inLot.NewRow();
        //                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "LOTID"));
        //                    inLot.Rows.Add(row);
        //                }
        //            }

        //            loadingIndicator.Visibility = Visibility.Visible;

        //            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_S", "INDATA,INLOT", null, (bizResult, bizException) =>
        //            {
        //                try
        //                {
        //                    if (bizException != null)
        //                    {
        //                        Util.MessageException(bizException);
        //                        //Util.AlertByBiz("BR_PRD_REG_LOCATE_LOT_MOVE_RW", bizException.Message, bizException.ToString());
        //                        return;
        //                    }

        //                    Util.MessageInfo("SFU1766");
        //                    //Util.AlertInfo("SFU1766");  //이동완료

        //                    DataTable dt = DataTableConverter.Convert(dgWipInfo.ItemsSource);
        //                    dt = dt.Select("CHK = 'true'").Count() == 0 ? null : dt.Select("CHK = 'true'").CopyToDataTable();
        //                    dt = GetLotInfo(Convert.ToString(dt.Rows[0]["LOTID"]));

        //                    AddRow(dgRewinderInfo, dt);

        //                    remove(dgWipInfo, "CHK");


        //                }
        //                catch (Exception ex)
        //                {
        //                    Util.MessageException(ex);
        //                }
        //                finally
        //                {
        //                    loadingIndicator.Visibility = Visibility.Collapsed;
        //                }
        //            }, indataSet);

        //        }
        //        catch (Exception ex)
        //        {
        //            Util.MessageException(ex);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }

        //}
        //private void MoveCancelProcess()
        //{
        //    try
        //    {
        //        DataSet indataSet = new DataSet();
        //        DataTable inData = indataSet.Tables.Add("INDATA");
        //        inData.Columns.Add("SRCTYPE", typeof(string));
        //        inData.Columns.Add("IFMODE", typeof(string));
        //        inData.Columns.Add("USERID", typeof(string));
        //        inData.Columns.Add("PROCID_FR", typeof(string));
        //        inData.Columns.Add("NOTE", typeof(string));

        //        DataRow row = inData.NewRow();
        //        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
        //        row["IFMODE"] = "OFF";
        //        row["USERID"] = LoginInfo.USERID;
        //        row["PROCID_FR"] = Process.NOTCHING_REWINDING;
        //        row["NOTE"] = "";
        //        inData.Rows.Add(row);

        //        DataTable inLot = indataSet.Tables.Add("IN_LOT");
        //        inLot.Columns.Add("LOTID", typeof(string));

        //        for (int i = 0; i < dgRewinderInfo.Rows.Count; i++)
        //        {
        //            if (Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
        //            {
        //                row = inLot.NewRow();
        //                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "LOTID"));
        //                indataSet.Tables["IN_LOT"].Rows.Add(row);
        //            }

        //        }

        //        loadingIndicator.Visibility = Visibility.Visible;

        //        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVECANCEL_RW", "INDATA,IN_LOT", null, (bizResult, bizException) =>
        //        {
        //            try
        //            {
        //                if (bizException != null)
        //                {
        //                    Util.MessageException(bizException);
        //                    //Util.AlertByBiz("BR_PRD_REG_LOCATE_LOT_MOVECANCEL_RW", bizException.Message, bizException.ToString());
        //                    return;
        //                }

        //                Util.AlertInfo("SFU1766");  //이동완료

        //                DataTable dt = DataTableConverter.Convert(dgRewinderInfo.ItemsSource);
        //                dt = dt.Select("CHK = 'True'") == null ? null : dt.Select("CHK = 'True'").CopyToDataTable();
        //                dt = GetLotInfo(Convert.ToString(dt.Rows[0]["LOTID"]));

        //                AddRow(dgWipInfo, dt);

        //                remove(dgRewinderInfo, "CHK");
        //                ClearData();


        //            }
        //            catch (Exception ex)
        //            {
        //                Util.MessageException(ex);
        //            }
        //            finally
        //            {
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //            }
        //        }, indataSet);

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }

        //}
        private void ConfirmProcess()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("IN_EQP");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROD_VER_CODE", typeof(string));
                inData.Columns.Add("SHIFT", typeof(string));
                inData.Columns.Add("WRK_USER_NAME", typeof(string));
                inData.Columns.Add("WIPNOTE", typeof(string));
                inData.Columns.Add("LANE_QTY", typeof(string));
                inData.Columns.Add("LANE_PTN_QTY", typeof(string));


                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = "OFF";
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                row["PROCID"] = Process.NOTCHING_REWINDING;
                row["USERID"] = LoginInfo.USERID;
                row["PROD_VER_CODE"] = "";// txtVersion.Text;
                row["SHIFT"] = txtShift.Tag;
                row["WRK_USER_NAME"] = txtWorker.Text;
                row["WIPNOTE"] = DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK");

                row["LANE_QTY"] = 1;
                row["LANE_PTN_QTY"] = 1;
                inData.Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("INPUTQTY", typeof(string));
                inLot.Columns.Add("OUTPUTQTY", typeof(string));
                inLot.Columns.Add("RESNQTY", typeof(string));

                row = inLot.NewRow();
                row["LOTID"] = _CurrentLot.LOTID;
                row["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
                row["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                row["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY"));
                inLot.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_END_LOT_RW_ASSY", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            //Util.AlertByBiz("BR_PRD_REG_START_END_LOT_RW", bizException.Message, bizException.ToString());
                            return;
                        }
                        Util.AlertInfo("SFU1924");  //착공완료 / 실적확정

                        GetRewinderLot();
                        ClearData();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void PrintLabel(string sLotID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("I_LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("I_RESO", typeof(string));   // 해상도
                inTable.Columns.Add("I_PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("I_MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("I_MARV", typeof(string));   // 시작위치V  

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LOTID"] = sLotID;
                        indata["PROCID"] = Process.NOTCHING_REWINDING;
                        indata["I_LBCD"] = LoginInfo.CFG_LABEL_TYPE;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_RW", "INDATA", "RSLTDT", inTable);
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_COM", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["I_ATTVAL"].ToString();

                    Util.PrintLabel(FrameOperation, loadingIndicator, Util.NVC(dtMain.Rows[0]["I_ATTVAL"]));
                    // Modify : 2016.12.15, 환경설정 Default Print 출력 *************************************************************************
                    //foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    //{
                    //    if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    //    {
                    //        FrameOperation.PrintFrameMessage(string.Empty);
                    //        bool brtndefault = Util.PrintZpl_COM_LTP_USB(FrameOperation, dr, Util.NVC(dr["ZPL"]), Util.NVC(dr["PORTNAME"]));
                    //        if (brtndefault == false)
                    //        {
                    //            loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                    //            FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));    //Barcode Print 실패
                    //            return;
                    //        }
                    //    }
                    //}
                    //****************************************************************************************************************************
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetDefect(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            try
            {
                defectFlag = false;
                if (datagrid.Rows.Count < 0) return;


                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataRow["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inDataRow);

                DataTable IndataTable = inDataSet.Tables.Add("INRESN");
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("ACTID", typeof(string));
                IndataTable.Columns.Add("RESNCODE", typeof(string));
                IndataTable.Columns.Add("RESNQTY", typeof(decimal));
                IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
                IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
                IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
                IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

                for (int i = 0; i < datagrid.GetRowCount(); i++)
                {

                    inDataRow = IndataTable.NewRow();

                    inDataRow["LOTID"] = _CurrentLot.LOTID;
                    inDataRow["WIPSEQ"] = _CurrentLot.WIPSEQ;
                    inDataRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "ACTID"));
                    inDataRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNCODE"));
                    inDataRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNQTY"));
                    inDataRow["DFCT_TAG_QTY"] = 0;
                    inDataRow["LANE_QTY"] = 1;
                    inDataRow["LANE_PTN_QTY"] = 1;
                    inDataRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "COST_CNTR_ID"));
                    IndataTable.Rows.Add(inDataRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(bizException.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                            return;
                        }

                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                        GetDefectList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void remove(C1.WPF.DataGrid.C1DataGrid datagrid, string sCol)
        {
            DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
            datagrid.ItemsSource = dt.Select(sCol + "= 'false'").Count() == 0 ? null : DataTableConverter.Convert(dt.Select(sCol + "= 'false'").CopyToDataTable());
            Util.GridSetData(datagrid, DataTableConverter.Convert(datagrid.ItemsSource), FrameOperation, true);
        }

        private void GetProcessDetail(int index)
        {
            try
            {
                _CurrentLot.LOTID = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LOTID"));
                _CurrentLot.VERSION = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "PROD_VER_CODE"));
                //_Lane = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY"));
                //_Lane_Ptn_qty = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY"));
                _CurrentLot.INPUTQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.GOODQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPSEQ"));

                //txtVersion.Text = _CurrentLot.VERSION;
                //txtLane.Text = _Lane;
                txtUnit.Text = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "UNIT"));

                // LOT GRID
                DataTable _dtLotInfo = new DataTable();
                _dtLotInfo.Columns.Add("LOTID", typeof(string));
                _dtLotInfo.Columns.Add("WIPSEQ", typeof(Int32));
                _dtLotInfo.Columns.Add("INPUTQTY", typeof(double));
                _dtLotInfo.Columns.Add("GOODQTY", typeof(double));
                //_dtLotInfo.Columns.Add("GOODPTNQTY", typeof(double));
                _dtLotInfo.Columns.Add("LOSSQTY", typeof(double));
                _dtLotInfo.Columns.Add("DTL_DEFECT", typeof(double));
                _dtLotInfo.Columns.Add("DTL_LOSS", typeof(double));
                _dtLotInfo.Columns.Add("DTL_CHARGEPRD", typeof(double));

                DataRow dRow = _dtLotInfo.NewRow();
                dRow["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                dRow["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);
                dRow["INPUTQTY"] = Util.NVC(_CurrentLot.INPUTQTY);
                dRow["GOODQTY"] = Util.NVC(_CurrentLot.GOODQTY);
                //dRow["GOODPTNQTY"] = Convert.ToDouble(Convert.ToDouble(_CurrentLot.GOODQTY) * Convert.ToDouble(_Lane));

                _dtLotInfo.Rows.Add(dRow);

                Util.GridSetData(dgLotInfo, _dtLotInfo, FrameOperation);

                GetDefectList();

                DataTable dtCopy = _dtLotInfo.Copy();
                BindingWipNote(dtCopy);
            }
            catch (Exception ex)
            {
                DataTableConverter.SetValue(dgRewinderInfo.Rows[index].DataItem, "CHK", false);
                Util.MessageException(ex);
            }

        }

        private bool GetMoveToProcChk(string sLotID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID_TO", typeof(string));
                inTable.Columns.Add("PATHTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID_TO"] = Process.NOTCHING_REWINDING;
                newRow["PATHTYPE"] = "BEFORE";
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MOVE_TO_PROC_CHK", "INDATA", "RSLTDT", inTable);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetDefectList()
        {
            try
            {
                Util.gridClear(dgWipReason);

                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING_REWINDING;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["LOTID"] = _CurrentLot.LOTID;
                newRow["WIPSEQ"] = _CurrentLot.WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", inTable);

                Util.GridSetData(dgWipReason, dt, FrameOperation);

                GetDefectSum();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool GetDefectSum()
        {
            double ValueToDefect = 0F;
            double ValueToLoss = 0F;
            double ValueToCharge = 0F;
            double ValueToExceedLength = 0F; //길이초과수량

            //int laneqty = int.Parse(txtLane.Text);

            SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);

            //// 투입량의 제한률 이상 초과하면 입력 금지
            //decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
            //decimal inputRateQty = Util.NVC_Decimal(inputQty * inputOverrate);

            //if (inputRateQty > 0 && Util.NVC_Decimal(ValueToExceedLength) > inputRateQty)
            //{
            //    Util.MessageValidation("SFU3195", new object[] { Util.NVC(inputOverrate * 100) + "%" });    //투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
            //    return false;
            //}

            // SET LOT GRID
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", ValueToDefect + ValueToLoss + ValueToCharge);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", ValueToDefect);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_LOSS", ValueToLoss);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_CHARGEPRD", ValueToCharge);

            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge));
            //DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY", ((double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge)) * laneqty);

            return true;
        }

        private void SumDefectTotalQty(ref double DefectSum, ref double LossSum, ref double ChargeSum, ref double ExceedLength)
        {
            DefectSum = 0;
            LossSum = 0;
            ChargeSum = 0;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable defectDt = ((DataView)dgWipReason.ItemsSource).Table;

                foreach (DataRow dr in defectDt.Rows)
                {
                    if (!string.IsNullOrEmpty(dr["RESNQTY"].ToString()))
                    {
                        if (!string.Equals(Util.NVC(dr["RSLT_EXCL_FLAG"]), "Y"))
                        {
                            if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                                DefectSum += Convert.ToDouble(dr["RESNQTY"]);
                            else if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                LossSum += Convert.ToDouble(dr["RESNQTY"]);
                            else if (string.Equals(Util.NVC(dr["ACTID"]), "CHARGE_PROD_LOT"))
                                ChargeSum += Convert.ToDouble(dr["RESNQTY"]);
                        }
                        else
                        {
                            if (string.Equals(Util.NVC(dr["ACTID"]), "LOSS_LOT"))
                                ExceedLength = Convert.ToDouble(dr["RESNQTY"]);
                        }
                    }
                }
            }

        }
        bool IsExceptionDefectResult(string actId, string resnCode)
        {
            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("ACTID", typeof(string));
            inTable.Columns.Add("RESNCODE", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            indata["PROCID"] = Process.NOTCHING_REWINDING;
            indata["ACTID"] = actId;
            indata["RESNCODE"] = resnCode;
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_ELEC_DEFECT_EXCEPTION", "INDATA", "RSLTDT", inTable);

            if (_dt.Rows[0][0].ToString().Equals("1"))
                return true;
            else
                return false;
        }

        private void ClearData()
        {
            //  Util.gridClear(dgDefect);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgRemark);
            Util.gridClear(dgLotInfo);

            //txtVersion.Text = "";
            //txtLane.Text = "";

            txtUnit.Text = string.Empty;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);

                //if (Convert.ToDateTime(txtShiftEndTime.Text) < System.DateTime.Now)
                if (Convert.ToDateTime(sShiftTime) < System.DateTime.Now)
                {
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                }
            }
        }


        private void RunRTLS(string sLotID, string sProcID)
        {
            /*
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = sProcID;
                IndataTable.Rows.Add(Indata);

                DataTable dtRTLS = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RTLS_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dtRTLS.Rows.Count == 0)
                    return;

                DataTable iTable = new DataTable("INDATA");
                iTable.Columns.Add("CONDITION", typeof(string));
                iTable.Columns.Add("CART_NO", typeof(string));
                iTable.Columns.Add("LOTID", typeof(string));
                iTable.Columns.Add("USERID", typeof(string));
                iTable.Columns.Add("ZONE_ID", typeof(string));
                iTable.Columns.Add("EQPTID", typeof(string));

                foreach (DataRow dr in dtRTLS.Rows)
                {
                    DataRow rtlsdata = iTable.NewRow();
                    rtlsdata["CONDITION"] = "EQPT_LOT_MAP";
                    rtlsdata["CART_NO"] = "";
                    rtlsdata["LOTID"] = Util.NVC(dr["LOTID"]);
                    rtlsdata["USERID"] = "MES";
                    rtlsdata["ZONE_ID"] = "";
                    rtlsdata["EQPTID"] = Util.NVC(dr["EQPTID"]);
                    iTable.Rows.Add(rtlsdata);
                }
                new ClientProxy().ExecuteService("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, iTable, (result, searchException) =>
                {
                    if (searchException != null)
                        throw searchException;
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            */
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Tag = window.USERID;
                txtWorker.Text = window.USERNAME;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnMoveStart);
            //listAuth.Add(btnMoveCancel);
            listAuth.Add(btnConfirm);

            listAuth.Add(btnSaveDefect);
            listAuth.Add(btnSaveRemark);
            listAuth.Add(btnShift);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        private DataTable GetLotInfo(string lotid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["LOTID"] = lotid;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_RW_INFO", "INDATA", "OUTDATA", dt);

            if (result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2025");  //해당하는 LOT정보가 없습니다.
                return null;
            }
            _LotProcId = Convert.ToString(result.Rows[0]["PROCID"]);
            _Wipstat = Convert.ToString(result.Rows[0]["WIPSTAT"]);
            _Hold = Convert.ToString(result.Rows[0]["WIPHOLD"]);
            return result;

        }
        private void initGridTable(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in datagrid.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            datagrid.BeginEdit();
            datagrid.ItemsSource = DataTableConverter.Convert(dt);
            datagrid.EndEdit();
        }
        #endregion
        private void SetWipNote()
        {
            try
            {
                if (dgRemark.GetRowCount() < 1)
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable dt = ((DataView)dgRemark.ItemsSource).Table;
                DataRow inData = null;
                for (int i = 1; i < dt.Rows.Count; i++)
                {
                    inData = inTable.NewRow();

                    inData["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                    inData["WIP_NOTE"] = Util.NVC(dt.Rows[i]["REMARK"]);
                    inData["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(inData);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        //Util.AlertByBiz("BR_PRD_REG_WIPHISTORY_NOTE", ex.Message, ex.ToString());
                        return;
                    }

                    Util.MessageInfo("SFU1275");        //정상 처리 되었습니다.
                    remarkFlag = false;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BindingWipNote(DataTable dt)
        {
            if (dgRemark.GetRowCount() > 0) return;

            DataTable dtRemark = new DataTable();

            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));
            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = "ALL";
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                inDataRow["REMARK"] = GetWIPNOTE(Util.NVC(_row["LOTID"]));
                dtRemark.Rows.Add(inDataRow);
            }
            Util.GridSetData(dgRemark, dtRemark, FrameOperation);
        }
        private string GetWIPNOTE(string sLotID)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHISTORYATTR_WIPNOTE", "INDATA", "RSLTDT", inTable);
            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["WIP_NOTE"]);
            }
            else
            {
                return "";
            }
        }

        private void dgRemart_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row != null && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, dgRemark.Columns[e.Cell.Column.Index].Name)).Equals(""))
            {
                remarkFlag = true;
            }

            if (dgRemark.Rows.Count < 1) return;
            if (e.Cell.Row.Index != 0) return;

            string strAll = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[e.Cell.Row.Index].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));
            string strTmp = "";
            for (int i = 1; i < dgRemark.Rows.Count; i++)
            {
                strTmp = Util.NVC(DataTableConverter.GetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name));

                if (!string.IsNullOrEmpty(strTmp))
                    strTmp += " " + strAll;
                else
                    strTmp = strAll;

                DataTableConverter.SetValue(dgRemark.Rows[i].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, strTmp);
            }
            DataTableConverter.SetValue(dgRemark.Rows[0].DataItem, dgRemark.Columns[e.Cell.Column.Index].Name, "");
        }
        private void dgRemark_Click(object sender, RoutedEventArgs e)
        {
            SetWipNote();
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipmentSegment.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
            //    return;
            //}

            //if (cboEquipment.SelectedIndex < 1)
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return;
            //}

            //CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
            //wndPopup.FrameOperation = FrameOperation;

            //if (wndPopup != null)
            //{
            //    //(grdCommand.Children[0] as UcCommand).btnExtra.IsDropDownOpen = false;
            //    object[] Parameters = new object[3];
            //    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
            //    Parameters[1] = cboOperation.SelectedValue.ToString(); //재와인더
            //    Parameters[2] = cboEquipment.SelectedValue.ToString();

            //    C1WindowExtension.SetParameters(wndPopup, Parameters);

            //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            //}
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                ClearData();

                string sEqsgID = string.Empty;
                string sEqptID = string.Empty;

                if (cboEquipmentSegment.SelectedValue != null)
                    sEqsgID = cboEquipmentSegment.SelectedValue.ToString();
                else
                    sEqsgID = "";

                if (cboEquipment.SelectedValue != null)
                    sEqptID = cboEquipment.SelectedValue.ToString();
                else
                    sEqptID = "";

                txtWorker.Text = "";
                txtWorker.Tag = "";
                txtShift.Text = "";
                txtShift.Tag = "";
                txtShiftStartTime.Text = "";
                txtShiftEndTime.Text = "";
                txtShiftDateTime.Text = "";
                txtLossCnt.Text = "";
                txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);

                // 설비 선택 시 자동 조회 처리
                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        loadingIndicator.Visibility = Visibility.Visible;

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
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

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
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
                loadingIndicator.Visibility = Visibility.Visible;

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

                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
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
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedValue == null) return;
                if (Util.NVC(cboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

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

        //private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    btnExtra.IsDropDownOpen = false;
        //}
    }
}
#endregion