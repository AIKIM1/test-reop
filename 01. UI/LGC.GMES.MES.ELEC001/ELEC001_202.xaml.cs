/*************************************************************************************
 Created Date : 2023.02.23
      Creator : 정재홍
   Decription : 재와인딩 공정진척 (설비Online) - ([E20230207-000434] - Rewinder Result Confirm Improve)
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.14  정재홍 : Initial Created. [ELEC001_115 COPY] 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
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
using System.Windows.Documents;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using System.Threading;

namespace LGC.GMES.MES.ELEC001
{

    public partial class ELEC001_202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        CurrentLotInfo _CurrentLot = new CurrentLotInfo();
        string _Lane = string.Empty;
        string _Lane_Ptn_qty = string.Empty;
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
        string _LotProcId = string.Empty;
        string _Wipstat = string.Empty;
        string _labelPassHoldFlag = string.Empty;

        bool defectFlag = false;
        bool remarkFlag = false;
        bool isDupplicatePopup = false;

        decimal inputOverrate;

        private string WIPSTATUS { get; set; }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_202()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;

            chkWait.Checked += OnCheckBoxChecked;
            chkWait.Unchecked += OnCheckBoxChecked;
            chkRun.Checked += OnCheckBoxChecked;
            chkRun.Unchecked += OnCheckBoxChecked;
            chkEqpEnd.Checked += OnCheckBoxChecked;
            chkEqpEnd.Unchecked += OnCheckBoxChecked;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            initcombo();
            ApplyPermissions();
            GetRewinderLot();
            SetVisible();

            string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));

            if (IsAreaCommonCodeUse("RESN_COUNT_USE_YN", Process.SLIT_REWINDING))
            {
                // C20210928-000539 재와인딩 NG TAG수 칼럼 추가
                if (LoginInfo.CFG_SHOP_ID == "G452" || LoginInfo.CFG_SHOP_ID == "G183")
                    dgWipReason.Columns["COUNTQTY"].Visibility = Visibility.Collapsed;
                else
                    dgWipReason.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                dgWipReason.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
            }
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    GetRewinderLot();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            GetRewinderLot();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            _Util.SetDataGridUncheck(dgWipInfo, "CHK", checkIndex);

            if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") < 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }


            if (!Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[checkIndex].DataItem, "PROCID")).Equals(Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK")].DataItem, "PROCID"))))
            {
                Util.MessageValidation("SFU1446");  //같은 공정이 아닙니다.
                DataTableConverter.SetValue(dgWipInfo.Rows[checkIndex].DataItem, "CHK", false);
                return;
            }
            
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

           _Util.SetDataGridUncheck(dgRewinderInfo, "CHK", checkIndex);
           if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                Util.gridClear(dgWipReason);
                
                ClearData();
                return;
            }
         
            GetProcessDetail(checkIndex);
        }

        private void btnMoveStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK") < 0)
                {
                    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                    return;
                }

                //재와인더로 이동하시겠습니까?
                Util.MessageConfirm("SFU1872", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MoveProcess();
                    }
                });

              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMoveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
                {
                    Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                    return;
                }

                if (!_CurrentLot.STATUS.Equals(Wip_State.WAIT))
                {
                    Util.MessageValidation("SFU1220");  //대기LOT이 아닙니다.
                    return;
                }

                //기존공정으로 이동하시겠습니까?
                Util.MessageConfirm("SFU1470", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MoveCancelProcess();
                    }
                });

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStart()) return;

            ELEC001_202_LOTSTART popStartRW = new ELEC001_202_LOTSTART();
            popStartRW.FrameOperation = FrameOperation;

            object[] parameters = new object[7];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = cboEquipmentSegment.SelectedValue;
            parameters[2] = cboEquipment.SelectedValue;
            parameters[3] = cboEquipment.Text;
            parameters[4] = _CurrentLot.LOTID;
            parameters[5] = _Lane;
            parameters[6] = _Lane_Ptn_qty;

            popStartRW.dtLot = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            C1WindowExtension.SetParameters(popStartRW, parameters);

            popStartRW.Closed += new EventHandler(popStartRW_Closed);
            Dispatcher.BeginInvoke(new Action(() => popStartRW.ShowModal()));
            popStartRW.CenterOnScreen();
        }

        private void popStartRW_Closed(object sender, EventArgs e)
        {
            ELEC001_202_LOTSTART popup = sender as ELEC001_202_LOTSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                chkRun.IsChecked = true;

                btnSearch_Click(btnSearch, null);
            }
        }

        private void btnStartCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStartCancel()) return;

            // 선택된 LOT을 작업 취소하시겠습니까? 
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    StartCancelProcess();
                }
            });
        }

        private void btnEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEnd()) return;

            // 장비완료 하시겠습니까? 
            Util.MessageConfirm("SFU1865", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqptEndProcess();
                }
            });
        }

        private void btnEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEndCancel()) return;

            // 선택된 LOT을 작업 취소하시겠습니까?
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqptEndCancelProcess();
                }
            });
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirm()) return;

            PreConfirmProcess();
        }

        private void btnSaveDefect_Click(object sender, RoutedEventArgs e)
        {
            //불량정보를 저장하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
              {
                  if (result == MessageBoxResult.OK)
                  {
                      SetDefect(dgWipReason);

                      if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", cboProcess.SelectedValue.ToString()))
                      {
                          Util.gridClear(dgRemark);
                          DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;
                          BindingWipNote(dt);
                      }
                  }
              });
        }

        private void btnPrintBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.Rows.Count < 3)
            {
                return;
            }

            //바코드를 발행하시겠습니까?
            Util.MessageConfirm("SFU1540", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                PrintLabel(_CurrentLot.LOTID);
                }
            });


        }

        private void btnCard_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.Rows.Count < 3)
            {
                return;
            }

            //이력카드를 발행하시겠습니까?
            Util.MessageConfirm("SFU1772", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = _CurrentLot.LOTID; //LOT ID
                        Parameters[1] = Util.NVC(cboProcess.SelectedValue); //PROCESS ID

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                        // RTLS (Only 자동차 2동)
                        if (LoginInfo.CFG_AREA_ID == "E6")
                            RunRTLS(txtEndLotID.Text, Util.NVC(cboProcess.SelectedValue));
                    }
                }
            });
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
                Parameters[3] = Util.NVC(cboProcess.SelectedValue);
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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            defectFlag = true;
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (GetDefectSum() == false)
            {
                if (dataGrid != null)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", 0);
                }
            }
            else
            {
                if (e.Cell.Column.Name.Equals("DFCT_TAG_QTY"))
                {
                    string sTagQty = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_TAG_QTY"));
                    string sTagRate = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TAG_CONV_RATE"));

                    double dTagQty = 0;
                    double dTagRate = 0;

                    double.TryParse(sTagQty, out dTagQty);
                    double.TryParse(sTagRate, out dTagRate);
                    DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RESNQTY", dTagQty * dTagRate);
                    dataGrid.UpdateLayout();
                }
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
        #endregion

        #region Mehod
        private void initcombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            String[] sFilter2 = { "REWINDING_PROCID" };
            C1ComboBox[] cboEquipmentChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentChild,  sFilter: sFilter2, sCase: "COMMCODE");

            C1ComboBox[] cboProcessParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);
        }

        private void SetStatus()
        {
            var status = new List<string>();
            if (chkWait.IsChecked == true)
                status.Add(Wip_State.WAIT);

            if (chkRun.IsChecked == true)
                status.Add(Wip_State.PROC);

            if (chkEqpEnd.IsChecked == true)
                status.Add(Wip_State.EQPT_END);

            WIPSTATUS = string.Join(",", status);
        }

        private void GetRewinderLot()
        {
            try
            {
                ClearData();
                SetStatus();

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                row["WIPSTAT"] = WIPSTATUS;
                row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_GET_REWINDER_LOT_ONLINE", "INDATA", "OUTDATA", dt);
                if (result.Rows.Count == 0)
                {
                    Util.gridClear(dgRewinderInfo);
                    return;
                }

                Util.GridSetData(dgRewinderInfo, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void SearchData()
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }

                if(cboProcess.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU3207");  //공정을 선택하세요.
                    return;
                }

                if (cboEquipment.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                string lotid = txtLOTID.Text;
                if (txtLOTID.Text.Equals(""))
                {
                    Util.MessageValidation("SFU1366");  //LOT ID를 입력해주세요.
                    return;
                }

                DataTable dt = GetLotInfo(lotid);

                if (!ValidationWipInfo(dt))
                {
                    return;
                }

                AddRow(dgWipInfo, dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtLOTID.Text = "";
            }

        }

        private bool ValidationWipInfo(DataTable dt)
        {
            if (dt == null)
            {
                return false;
            }

            if (!_Wipstat.Equals(Wip_State.WAIT))
            {
                Util.MessageValidation("SFU1220");  //대기LOT이 아닙니다.
                return false;
            }

            for (int i = 0; i < dgWipInfo.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "LOTID")).Equals(dt.Rows[0]["LOTID"]))
                {
                    Util.MessageValidation("SFU2014");  //해당 LOT이 이미 존재합니다.
                    return false;
                }
            }

            if (_Hold.Equals("Y"))
            {
                if (!GetHoldPassArea())
                {
                    Util.MessageValidation("SFU1761", new object[] { txtLOTID.Text });    //{0}이 HOLD상태 입니다.
                    txtLOTID.Text = "";
                    return false;
                }
            }

            if ( string.Equals("E2100", dt.Rows[0]["PROCID"]) || string.Equals("E4100", dt.Rows[0]["PROCID"]))
            {
                Util.MessageValidation("SFU3200");//재와인더 공정으로 이동된 LOT입니다.
                txtLOTID.Text = "";
                return false;
            }
            return true;
        }

        bool GetHoldPassArea()
        {
            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = txtLOTID.Text;
            indata["CMCDTYPE"] = "REWINDING_HOLD_PASS_AREA";
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTAREA_CHECK", "INDATA", "RSLTDT", inTable);

            if (_dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

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
            preTable.Rows.Add(row);
            
            Util.GridSetData(datagrid, preTable, FrameOperation, true);        
        }

        private void MoveProcess()
        {
            try
            {
                try
                {
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("PROCID_FR", typeof(string));
                    inData.Columns.Add("PROCID_TO", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("NOTE", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    row["IFMODE"] = IFMODE.IFMODE_OFF;
                    row["USERID"] = LoginInfo.USERID;
                    row["PROCID_FR"] = Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWipInfo, "CHK")].DataItem, "PROCID"));
                    row["PROCID_TO"] = Util.NVC(cboProcess.SelectedValue);
                    row["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    row["NOTE"] = "";
                    inData.Rows.Add(row);

                    DataTable inLot = indataSet.Tables.Add("IN_LOT");
                    inLot.Columns.Add("LOTID", typeof(string));

                    for (int i = 0; i < dgWipInfo.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            row = inLot.NewRow();
                            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWipInfo.Rows[i].DataItem, "LOTID"));
                            indataSet.Tables["IN_LOT"].Rows.Add(row);
                        }
                    }

                    loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVE_RW", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1766");

                            //DataTable dt = DataTableConverter.Convert(dgWipInfo.ItemsSource);
                            //dt = dt.Select("CHK = 'true'").Count() == 0 ? null : dt.Select("CHK = 'true'").CopyToDataTable();
                            //dt = GetLotInfo(Convert.ToString(dt.Rows[0]["LOTID"]));
                            //AddRow(dgRewinderInfo, dt);

                            if (chkWait.IsChecked == true)
                            {
                                btnSearch_Click(btnSearch, null);
                            }
                            else
                            {
                                chkWait.IsChecked = true;
                            }

                            remove(dgWipInfo, "CHK");
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MoveCancelProcess()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROCID_FR", typeof(string));
                inData.Columns.Add("NOTE", typeof(string));

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["PROCID_FR"] = Util.NVC(cboProcess.SelectedValue);
                row["NOTE"] = "";
                inData.Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgRewinderInfo.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "LOTID"));
                        indataSet.Tables["IN_LOT"].Rows.Add(row);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVECANCEL_RW", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.AlertInfo("SFU1766");  //이동완료
                        DataTable dt = DataTableConverter.Convert(dgRewinderInfo.ItemsSource);
                        dt = dt.Select("CHK = 'True'") == null ? null : dt.Select("CHK = 'True'").CopyToDataTable();
                        dt = GetLotInfo(Convert.ToString(dt.Rows[0]["LOTID"]));
                        AddRow(dgWipInfo, dt);

                        remove(dgRewinderInfo, "CHK");
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

        private void ConfirmProcessReWinding()
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
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                row["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                row["PROD_VER_CODE"] = txtVersion.Text;
                row["SHIFT"] = txtShift.Tag;
                row["WRK_USER_NAME"] = txtWorker.Text;
                row["WIPNOTE"] = DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK");

                row["LANE_QTY"] = _Lane;
                row["LANE_PTN_QTY"] = _Lane_Ptn_qty;
                inData.Rows.Add(row);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("INPUTQTY", typeof(decimal));
                inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                inLot.Columns.Add("RESNQTY", typeof(decimal));

                row = inLot.NewRow();
                row["LOTID"] = _CurrentLot.LOTID;
                row["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
                row["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                row["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY"));
                inLot.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RW_ONLINE", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.AlertInfo("SFU1889");  //착공완료 / 실적확정

                        GetRewinderLot();
                        ClearData();
                        SetLabelLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetSelRewindingHist(string sLotid, string sWipSeq)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
                inTable.Columns.Add("EQPT_END_APPLY_FLAG", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLotid;
                newRow["WIPSEQ"] = sWipSeq;

                if (_CurrentLot.STATUS == Wip_State.EQPT_END)
                {
                    newRow["INPUT_LOT_STAT_CODE"] = Wip_State.EQPT_END;
                    newRow["EQPT_END_APPLY_FLAG"] = "Y";
                }
                else
                {
                    newRow["INPUT_LOT_STAT_CODE"] = "PROC,OUT";
                }
                inTable.Rows.Add(newRow);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REWINDING_WRK_HIST_UPDATE", "INDATA", "OUTDATA", inTable);

                if (Result == null)
                {
                    return null;
                }
                
                return Result;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void PreConfirmProcess(bool bRealWorkerSelFlag = false)
        {
            #region 작업자 실명관리 기능 추가
            if (!bRealWorkerSelFlag && CheckRealWorkerCheckFlag())
            {
                CMM001.CMM_COM_INPUT_USER wndRealWorker = new CMM001.CMM_COM_INPUT_USER();

                wndRealWorker.FrameOperation = FrameOperation;
                object[] Parameters2 = new object[0];

                C1WindowExtension.SetParameters(wndRealWorker, Parameters2);

                wndRealWorker.Closed -= new EventHandler(wndRealWorker_Closed);
                wndRealWorker.Closed += new EventHandler(wndRealWorker_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndRealWorker.ShowModal()));

                return;
            }
            #endregion

            //실적 확정 하시겠습니까?
            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcessReWinding();
                }
            });
        }

        private void SetLabelLot()
        {
            txtEndLotID.Text = _CurrentLot.LOTID;

            if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
                PrintLabel(_CurrentLot.LOTID);
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
                        indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
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

                // 바코드 설정값이  존재하지 않는 경우 Validation 추가 (CNJ 요청, 메세지는 공용메세지보다 Detail하게 변경) [2018-08-21]
                if (inTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU5006"); // 현재 바코드 설정값이 존재하지 않습니다.(Setting -> 프린터 -> 바코드 설정을 확인하세요)
                    return;
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_RW", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["I_ATTVAL"].ToString();

                    Util.PrintLabel(FrameOperation, loadingIndicator, Util.NVC(dtMain.Rows[0]["I_ATTVAL"]));
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
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataRow["EQPTID"] = Convert.ToString(cboEquipment.SelectedValue);
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
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
                    if (datagrid.Columns["DFCT_TAG_QTY"].Visibility == Visibility.Visible)
                    {
                        inDataRow["DFCT_TAG_QTY"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "DFCT_TAG_QTY")).Equals("") ? 0.ToString() : Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "DFCT_TAG_QTY")); ;
                    }
                    else
                    {
                        inDataRow["DFCT_TAG_QTY"] = 0;
                    }
                    inDataRow["LANE_QTY"] = txtLane.Text;
                    inDataRow["LANE_PTN_QTY"] = _Lane_Ptn_qty;
                    inDataRow["COST_CNTR_ID"] =  Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "COSTCENTERID"));
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
                            return;
                        }

                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                        GetDefectList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
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
                _Lane = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY"));
                _Lane_Ptn_qty = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_PTN_QTY"));
                _CurrentLot.INPUTQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.GOODQTY = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPQTY"));
                _CurrentLot.WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPSEQ"));
                _CurrentLot.STATUS = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "WIPSTAT"));

                _LotProcId = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "PROCID"));

                txtVersion.Text = _CurrentLot.VERSION;
                txtLane.Text = _Lane;
                txtUnit.Text = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "UNIT"));

                // LOT GRID
                DataTable _dtLotInfo = new DataTable();
                _dtLotInfo.Columns.Add("LOTID", typeof(string));
                _dtLotInfo.Columns.Add("WIPSEQ", typeof(Int32));
                _dtLotInfo.Columns.Add("INPUTQTY", typeof(double));
                _dtLotInfo.Columns.Add("GOODQTY", typeof(double));
                _dtLotInfo.Columns.Add("GOODPTNQTY", typeof(double));
                _dtLotInfo.Columns.Add("LOSSQTY", typeof(double));
                _dtLotInfo.Columns.Add("DTL_DEFECT", typeof(double));
                _dtLotInfo.Columns.Add("DTL_LOSS", typeof(double));
                _dtLotInfo.Columns.Add("DTL_CHARGEPRD", typeof(double));

                DataRow dRow = _dtLotInfo.NewRow();
                dRow["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                dRow["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);
                dRow["INPUTQTY"] = Util.NVC(_CurrentLot.INPUTQTY);
                dRow["GOODQTY"] = Util.NVC(_CurrentLot.GOODQTY);
                dRow["GOODPTNQTY"] = Convert.ToDouble(Convert.ToDouble(_CurrentLot.GOODQTY) * Convert.ToDouble(_Lane));
                _dtLotInfo.Rows.Add(dRow);

                Util.GridSetData(dgLotInfo, _dtLotInfo, FrameOperation);

                GetDefectList();
                GetQualityList();

                DataTable dtCopy = _dtLotInfo.Copy();
                BindingWipNote(dtCopy);
            }
            catch (Exception ex)
            {
                DataTableConverter.SetValue(dgRewinderInfo.Rows[index].DataItem, "CHK", true);
                Util.MessageException(ex);
            }
        }

        private void GetDefectList()
        {
            try
            {
                Util.gridClear(dgWipReason);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("RESNPOSITION", typeof(string));
                

                DataRow Indata = inDataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["LOTID"] = _CurrentLot.LOTID;
                Indata["RESNPOSITION"] = null;

                inDataTable.Rows.Clear();
                inDataTable.Rows.Add(Indata);
                
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_ACTIVITYREASON_ELEC", "INDATA", "RSLTDT", inDataTable);

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
            decimal totalResnQty = 0;

            int laneqty = int.Parse(txtLane.Text);

            SumDefectTotalQty(ref ValueToDefect, ref ValueToLoss, ref ValueToCharge, ref ValueToExceedLength);
            totalResnQty = Convert.ToDecimal(ValueToDefect) + Convert.ToDecimal(ValueToLoss) + Convert.ToDecimal(ValueToCharge);

            // 투입량의 제한률 이상 초과하면 입력 금지
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"));
            decimal inputRateQty = Util.NVC_Decimal(inputQty * inputOverrate);

            if (inputRateQty > 0 && Util.NVC_Decimal(ValueToExceedLength) > inputRateQty)
            {
                Util.MessageValidation("SFU3195", new object[] { Util.NVC(inputOverrate * 100) + "%" });    //투입량의 %1를 초과하여 입력 불가 [생산량 재 입력 후 진행]
                return false;
            }

            // 투입량 대비 제한이 없어서 해당 Validation 추가
            if ((inputQty + Convert.ToDecimal(ValueToExceedLength)) < totalResnQty)
            {
                Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                return false;
            }

            // SET LOT GRID
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", ValueToDefect + ValueToLoss + ValueToCharge);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", ValueToDefect);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_LOSS", ValueToLoss);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_CHARGEPRD", ValueToCharge);
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY", ((double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "INPUTQTY"))) + ValueToExceedLength) - (ValueToDefect + ValueToLoss + ValueToCharge)) *laneqty);
            GetDefectSum_INCR();

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
                    if ((!string.IsNullOrEmpty(dr["RESNQTY"].ToString())) && (!string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "OUT_LOT_QTY_INCR")))
                    {
                        if (!string.Equals(Util.NVC(dr["RSLT_EXCL_FLAG"]), "Y"))
                        {
                            if (string.Equals(Util.NVC( dr["ACTID"]), "DEFECT_LOT"))
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

        private void GetDefectSum_INCR()
        {
            double ValueToDefectTag = 0F;
            double ValueTagConvertRate = 0F;
            double ValueToDefectBefTag = 0F;

            int laneqty = int.Parse(txtLane.Text);

            SumDefectTotalQty_INCR(ref ValueToDefectTag, ref ValueTagConvertRate);

            ValueToDefectBefTag = double.Parse(BeforeProcDefectSum().ToString());
            txtBefTag.Text = ValueToDefectBefTag.ToString();

            if (ValueToDefectTag == 0) return;

            // SET LOT GRID
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "LOSSQTY"))) + (ValueToDefectTag * ValueTagConvertRate)));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "DTL_DEFECT"))) + (ValueToDefectTag * ValueTagConvertRate)));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY", (double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY")))) + ((ValueToDefectBefTag - ValueToDefectTag) * ValueTagConvertRate));
            DataTableConverter.SetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY", ((double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODPTNQTY"))) + ((ValueToDefectBefTag - ValueToDefectTag) * ValueTagConvertRate)) * laneqty));
        }

        private void SumDefectTotalQty_INCR(ref double DefectTagSum, ref double TagConvertRate)
        {
            DefectTagSum = 0;

            if (dgWipReason.ItemsSource != null)
            {
                DataTable defectDt = ((DataView)dgWipReason.ItemsSource).Table;

                foreach (DataRow dr in defectDt.Rows)
                {
                    if ((!string.IsNullOrEmpty(dr["DFCT_TAG_QTY"].ToString())) && (string.Equals(Util.NVC(dr["PRCS_ITEM_CODE"]), "OUT_LOT_QTY_INCR")))
                    {
                        if (string.Equals(Util.NVC(dr["ACTID"]), "DEFECT_LOT"))
                        {
                            DefectTagSum += Convert.ToDouble(dr["DFCT_TAG_QTY"]);
                            TagConvertRate = Convert.ToDouble(dr["TAG_CONV_RATE"]);
                        }                          
                    }
                }
            }

        }

        public decimal BeforeProcDefectSum()
        {
            DataTable _dt = new DataTable();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("ACTID", typeof(string));
            
            DataRow indata = inTable.NewRow();
            indata["AREAID"] = LoginInfo.CFG_AREA_ID;

            for (int i = 0; i < dgRewinderInfo.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
                    indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[i].DataItem, "LOTID"));
            }

            indata["ACTID"] = "DEFECT_LOT";
            inTable.Rows.Add(indata);

            _dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRE_PROC_DFCT_TAG_QTY", "INDATA", "RSLTDT", inTable);

            return Convert.ToDecimal(_dt.Rows[0][0].ToString());
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
            indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
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
            Util.gridClear(dgQuality);
            Util.gridClear(dgWipReason);
            Util.gridClear(dgRemark);
            Util.gridClear(dgLotInfo);

            isDupplicatePopup = false;
            txtVersion.Text = "";
            txtLane.Text = "";

            txtUnit.Text = string.Empty;

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text) && txtShiftEndTime.Text.Length == 19)
            {
                // 현재시간보다 근무종료 시간이 작으면 클리어
                string sShiftTime = System.DateTime.Now.ToString("yyyy-MM-dd") + " " + txtShiftEndTime.Text.Substring(txtShiftEndTime.Text.IndexOf(' ') + 1, 8);
                
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
            listAuth.Add(btnMoveStart);
            listAuth.Add(btnMoveCancel);
            listAuth.Add(btnConfirm);
            listAuth.Add(btnSaveDefect);
            listAuth.Add(btnSaveRemark);
            listAuth.Add(btnShift);
            listAuth.Add(btnStart);
            listAuth.Add(btnStartCancel);
            listAuth.Add(btnEqptEnd);
            listAuth.Add(btnEqptEndCancel);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private DataTable GetLotInfo(string lotid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["LOTID"] = lotid;
            row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
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

        private void SetWipNote()
        {
            try
            {
                if (dgRemark.GetRowCount() < 1) return;

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
            if (!IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", cboProcess.SelectedValue.ToString()))
                if (dgRemark.GetRowCount() > 0) return;

            DataTable dtRemark = new DataTable();
            dtRemark.Columns.Add("LOTID", typeof(String));
            dtRemark.Columns.Add("NG_TAG1", typeof(String));
            dtRemark.Columns.Add("NG_TAG2", typeof(String));
            dtRemark.Columns.Add("REMARK", typeof(String));

            DataRow inDataRow = null;
            inDataRow = dtRemark.NewRow();
            inDataRow["LOTID"] = "ALL";
            dtRemark.Rows.Add(inDataRow);

            foreach (DataRow _row in dt.Rows)
            {
                inDataRow = dtRemark.NewRow();
                inDataRow["LOTID"] = Util.NVC(_row["LOTID"]);
                
                if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", Process.SLIT_REWINDING))
                {
                    inDataRow["NG_TAG1"] = GetNgTagQty(Util.NVC(_CurrentLot.LOTID), Util.NVC(_CurrentLot.WIPSEQ), Process.SLIT_REWINDING);
                    inDataRow["NG_TAG2"] = GetNgTagQty(Util.NVC(_CurrentLot.LOTID), Util.NVC(_CurrentLot.WIPSEQ), null);
                }
                else
                {
                    inDataRow["NG_TAG1"] = "0";
                    inDataRow["NG_TAG2"] = "0";
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        #region NG TAG 수량 조회
        private string GetNgTagQty(string sLotID, string sWipSeq, string sProcid)
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["LOTID"] = sLotID;
            indata["WIPSEQ"] = sWipSeq;
            indata["PROCID"] = sProcid;
            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_NG_TAG_QTY", "RQSTDT", "RSLTDT", inTable);

            if (dt.Rows.Count > 0)
            {
                return Util.NVC(dt.Rows[0]["DFCT_TAG_QTY"]);
            }
            else
            {
                return "0";
            }
        }
        #endregion

        private static bool IsEquipmentAttr(string equipmentCode)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(equipmentCode).Select();
                if (dr.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private void StartCancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["LOTID"] = _CurrentLot.LOTID;
                newRow["WIPSEQ"] = _CurrentLot.WIPSEQ;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_RW_ONLINE", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    btnSearch_Click(btnSearch, null);
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void EqptEndProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                InInput.Columns.Add("EQPT_END_QTY", typeof(decimal));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = InInput.NewRow();
                newRow["INPUT_LOTID"] = _CurrentLot.LOTID;
                newRow["EQPT_END_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgLotInfo.Rows[dgLotInfo.TopRows.Count].DataItem, "GOODQTY"));
                InInput.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_RW_ONLINE", "IN_EQP,IN_INPUT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    btnSearch_Click(btnSearch, null);

                    //// 생산 Lot 재조회
                    //SelectProductLot(_dvProductLot["LOTID"].ToString());
                    //SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void EqptEndCancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Decimal));
                inTable.Columns.Add("USERID", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["LOTID"] = _CurrentLot.LOTID;
                newRow["WIPSEQ"] = _CurrentLot.WIPSEQ;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_RW_ONLINE", "INDATA,INLOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.
                    btnSearch_Click(btnSearch, null);

                    // 생산 Lot 재조회
                    //SelectProductLot(_dvProductLot["LOTID"].ToString());
                    //SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ValidationStart()
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");     // 라인을 선택하세요.
                return false;
            }

            if (cboProcess.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");     // 설비를 선택 하세요.
                return false;
            }

            if (dgRewinderInfo.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1381");    //Lot을 선택하세요.
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return false;
            }

            if (!_CurrentLot.STATUS.Equals(Wip_State.WAIT))
            {
                Util.MessageValidation("SFU1220");  //대기LOT이 아닙니다.
                return false;
            }

            return true;
        }

        private bool ValidationStartCancel()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_CurrentLot.STATUS.Equals(Wip_State.WAIT))
            {
                Util.MessageValidation("SFU3172");  //설비 완공 LOT은 취소할 수 없습니다.
                return false;
            }

            if (_CurrentLot.STATUS.Equals(Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU1671");  //설비 완공 LOT은 취소할 수 없습니다.
                return false; 
            }
            
            return true;
        }

        private bool ValidationEqptEnd()
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1153");  //설비를 선택하세요.
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (!_CurrentLot.STATUS.Equals(Wip_State.PROC))
            {
                Util.MessageValidation("SFU2957");     // 진행중인 작업을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptEndCancel()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_CurrentLot.STATUS.Equals(Wip_State.WAIT))
            {
                Util.MessageValidation("SFU1939");     // 취소 할 수 있는 상태가 아닙니다.
                return false;
            }

            if (_CurrentLot.STATUS.Equals(Wip_State.PROC))
            {
                Util.MessageValidation("SFU3464");     // 진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                return false;
            }
            return true;
        }

        private bool ValidationConfirm()
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                // "선택된 작업대상이 없습니다."
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (!_CurrentLot.STATUS.Equals(Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (defectFlag == true)
            {
                Util.MessageValidation("SFU2900");  //불량/Loss/물청 정보를 저장하세요.
                return false;
            }

            if (remarkFlag == true)
            {
                Util.MessageValidation("SFU2977"); //특이사항 정보를 저장하세요.
                return false; 
            }

            if (string.IsNullOrEmpty(txtShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }
            return true;
        }

        private bool ValidLabelPass()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID");
            inTable.Columns.Add("EQPTID");

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = _CurrentLot.LOTID;
            newRow["EQPTID"] = cboEquipment.SelectedValue;
            inTable.Rows.Add(newRow);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DATA_ECLCT", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(rslt))
                return false;
            else
                return true;
        }

        private void CheckLabelPassHold(Action callback)
        {
            try
            {
                //라벨링 패스 기능은 기존에도 활성화 되어 있었음
                _labelPassHoldFlag = "Y";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("CMCDTYPE");
                inTable.Columns.Add("CMCODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "LABEL_PASS_HOLD_CHECK";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(rslt))
                {
                    if (!ValidLabelPass())
                    {
                        // Labeling Pass를 진행한 이력이 있습니다. 홀드하시겠습니까?
                        Util.MessageConfirm("SFU8218", result =>
                        {
                            //홀드
                            //전극의 모든 실적확정 로직에 플래그를 넣어서 Y면 자주검사스펙 홀드를 통과하도록하고 N이면 넘긴다
                            //MMD 동별 자주검사에서 자동홀드여부도 Y로 바꿔야 한다.
                            //또한 홀드를 걸 항목에 대해서만 LSL USL을 등록한다.
                            if (result == MessageBoxResult.OK)
                            {
                                _labelPassHoldFlag = "Y";
                            }
                            else
                            {
                                _labelPassHoldFlag = "N";
                            }
                            callback();
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception) { }
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
        

        private void dgRemark_Click(object sender, RoutedEventArgs e)
        {
            SetWipNote();
        }

        private void btnPrintLabel_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }

            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            // BARCODE_PRINT_PWD를 통해 동과 공정 관리
            if (LoginInfo.CFG_LABEL_AUTO == "Y" && IsAreaCommonCodeUse("BARCODE_PRINT_PWD", Util.NVC(cboProcess.SelectedValue)))
            {
                CMM_ELEC_BARCODE_AUTH authConfirm = new CMM_ELEC_BARCODE_AUTH();
                authConfirm.FrameOperation = FrameOperation;
                if (authConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Util.NVC(cboProcess.SelectedValue);

                    C1WindowExtension.SetParameters(authConfirm, Parameters);
                    authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Delete);

                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(authConfirm);
                            authConfirm.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboProcess.SelectedValue.ToString(); //재와인더
                    Parameters[2] = cboEquipment.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void OnCloseAuthConfirm_Delete(object sender, EventArgs e)
        {
            CMM_ELEC_BARCODE_AUTH window = sender as CMM_ELEC_BARCODE_AUTH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                CMM_ELEC_BARCODE wndPopup = new CMM_ELEC_BARCODE();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboProcess.SelectedValue.ToString(); //재와인더
                    Parameters[2] = cboEquipment.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }
                
        private void SetVisible()
        {
            if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", Process.SLIT_REWINDING))
            {
                dgRemark.Columns["NG_TAG1"].Visibility = Visibility.Visible;
                dgRemark.Columns["NG_TAG2"].Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region 작업자 실명관리 기능 추가
        private bool CheckRealWorkerCheckFlag()
        {
            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("REAL_WRKR_CHK_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["REAL_WRKR_CHK_FLAG"]).Equals("Y"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void wndRealWorker_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM001.CMM_COM_INPUT_USER window = sender as CMM001.CMM_COM_INPUT_USER;

                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SaveRealWorker(window.USER_NAME);

                    PreConfirmProcess(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _CurrentLot.LOTID;
                //newRow["WIPSEQ"] = null;
                newRow["WORKER_NAME"] = sWrokerName;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                if (inTable.Rows.Count < 1) return;

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        #endregion

        #region  20200701 불량수량 변경가능여부에 따른 수정색깔 설정
        //20200701 오화백 불량수량변경가능여부에 따른  수정색깔 설정
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                // 수정 가능여부에 따른 칼럼 처리
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    if (string.Equals(e.Cell.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                    }
                }
            }));
        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("DFCT_TAG_QTY") || e.Column.Name.Equals("COUNTQTY") || e.Column.Name.Equals("RESNQTY"))
            {
                if (!string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").GetString()) && DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG").Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

            if (string.Equals(e.Column.Name, "COUNTQTY") && !string.Equals(DataTableConverter.GetValue(e.Row.DataItem, "WRK_COUNT_MNGT_FLAG"), "Y"))
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region  20200701 품질정보탭 추가

        //품질항목 조회
        private void GetQualityList()
        {
            try
            {

                if (_CurrentLot.LOTID == string.Empty)
                    return;

                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("VER_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                List<C1DataGrid> lst = new List<C1DataGrid> { dgQuality };
                foreach (C1DataGrid dg in lst)
                {
                    IndataTable.Rows.Clear();

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _LotProcId;
                    Indata["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                    Indata["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);

                    if (!string.IsNullOrEmpty(txtVersion.Text))
                    {
                        Indata["VER_CODE"] = txtVersion.Text;
                        Indata["LANEQTY"] = txtLane.Text
                            ;
                    }
                    IndataTable.Rows.Add(Indata);

                    DataTable dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (dt.Rows.Count == 0)
                        dt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (dg.Visibility == Visibility.Visible)
                    {
                        Util.GridSetData(dg, dt, FrameOperation, true);
                        _Util.SetDataGridMergeExtensionCol(dg, new string[] { "CLSS_NAME1", "CLSS_NAME2" }, DataGridMergeMode.VERTICALHIERARCHI);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 품질항목 저장버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_Lane == string.Empty)
                return;
            try
            {
                SaveQuality(dgQuality);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 품질항목 저장
        /// </summary>
        /// <param name="dg"></param>
        private void SaveQuality(C1DataGrid dg)
        {
            try
            {
                if (dg.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU2002");  //품질정보가 없습니다.
                    return;
                }

                DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

                new ClientProxy().ExecuteServiceSync("BR_QCA_REG_WIP_DATA_CLCT", "INDATA", null, inEDCLot);
                dg.EndEdit(true);

                Util.MessageInfo("SFU1270");    //저장되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 품질항목 저장 데이터 테이블
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
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
                inData["LOTID"] = Util.NVC(_CurrentLot.LOTID);
                inData["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                decimal tmp;
                if (Decimal.TryParse(_iRow["CLCTVAL01"].ToString().ToString(CultureInfo.InvariantCulture.NumberFormat), out tmp))
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Decimal.Parse(Util.NVC(_iRow["CLCTVAL01"]).Trim()).ToString(CultureInfo.InvariantCulture.NumberFormat);
                else
                    inData["CLCTVAL01"] = Util.NVC(_iRow["CLCTVAL01"]) == Double.NaN.ToString() ? "" : Util.NVC(_iRow["CLCTVAL01"]).Trim().ToString();

                inData["WIPSEQ"] = Util.NVC(_CurrentLot.WIPSEQ);
                inData["CLCTSEQ"] = 1;
                IndataTable.Rows.Add(inData);
            }
            return IndataTable;
        }

        /// <summary>
        /// 품질항목 스프레드 수정시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid caller = sender as C1DataGrid;

            string sValue = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "CLCTVAL01"));
            string sUSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "USL"));
            string sLSL = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "LSL"));

            string sCLCTVALUE = string.Empty;
            string sCLCNAME = String.Empty;
            string sCLCITEM = String.Empty;

            string sCode = Util.NVC(DataTableConverter.GetValue(caller.Rows[caller.CurrentRow.Index].DataItem, "INSP_VALUE_TYPE_CODE"));

            if (!sValue.Equals("") && e.Cell.Presenter != null)
            {
                if (sCode.Equals("NUM"))
                {
                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
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

                    // 자주검사 USL, LSL 체크
                    DataTable dataCollect = DataTableConverter.Convert(caller.ItemsSource);
                    int iHGCount1 = 0;  // H/G
                    int iHGCount2 = 0;  // M/S
                    int iHGCount3 = 0;  // 1차 H/G
                    int iHGCount4 = 0;  // 1차 M/S
                    decimal sumValue1 = 0;
                    decimal sumValue2 = 0;
                    decimal sumValue3 = 0;
                    decimal sumValue4 = 0;
                    foreach (DataRow row in dataCollect.Rows)
                    {
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount1++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount2++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount3++;
                            }
                        }
                        else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                            {
                                sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                iHGCount4++;
                            }
                        }
                    }

                    if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue1 / iHGCount1)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue2 / iHGCount2)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 4)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                    else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue3 / iHGCount3)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                    else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(e.Cell.Value) - (sumValue4 / iHGCount4)) > 2)
                        Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);

                }
            }
        }

        /// <summary>
        /// 품질항목 스프레드 PreviewKeyDown 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null && dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    dataGrid.EndEdit(true);
                }
                else if (e.Key == Key.Delete)
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false)
                    {
                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, 0);
                        dataGrid.BeginEdit(dataGrid.CurrentCell);
                        dataGrid.EndEdit(true);

                        DataTableConverter.SetValue(dataGrid.CurrentCell.Row.DataItem, dataGrid.CurrentCell.Column.Name, DBNull.Value);

                        if (dataGrid.CurrentCell != null && dataGrid.CurrentCell.Presenter != null)
                        {
                            dataGrid.CurrentCell.Presenter.Background = new SolidColorBrush(Colors.White);
                            dataGrid.CurrentCell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            dataGrid.CurrentCell.Presenter.FontWeight = FontWeights.Normal;
                        }
                    }
                }
                else if (!char.IsControl((char)e.Key) && !char.IsDigit((char)e.Key))
                {
                    if (dataGrid.CurrentCell.Column.IsReadOnly == false && dataGrid.CurrentCell.IsEditable == false)
                        dataGrid.BeginEdit(dataGrid.CurrentCell.Row.Index, dataGrid.CurrentCell.Column.Index);
                }
            }
        }
        
        /// <summary>
        /// 품질항목 스프레드 LoadedCellPresenter 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                                C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                                C1.WPF.DataGrid.C1DataGrid grid;
                                grid = p.DataGrid;

                                string sCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "INSP_VALUE_TYPE_CODE"));
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));
                                string sCSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSL"));
                                string sLSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL_LIMIT"));
                                string sUSL_Limit = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL_LIMIT"));

                                if (panel != null)
                                {
                                    if (string.Equals(sCode, "NUM"))
                                    {
                                        C1NumericBox numeric = panel.Children[0] as C1NumericBox;

                                        // 재설정
                                        if (string.Equals(txtUnit.Text, "EA"))
                                            numeric.Format = "F1";
                                        else
                                            numeric.Format = GetUnitFormatted();

                                        // SRS요청으로 동별 LIMIT값 설정 [2017-11-30]
                                        if (!string.IsNullOrEmpty(sLSL_Limit) && Util.NVC_Decimal(sLSL_Limit) > 0)
                                            numeric.Minimum = Convert.ToDouble(sLSL_Limit);
                                        else
                                            numeric.Minimum = Double.NegativeInfinity;

                                        if (!string.IsNullOrEmpty(sUSL_Limit) && Util.NVC_Decimal(sUSL_Limit) > 0)
                                            numeric.Maximum = Convert.ToDouble(sUSL_Limit);
                                        else
                                            numeric.Maximum = Double.PositiveInfinity;

                                        if (numeric != null && !string.IsNullOrWhiteSpace(Util.NVC(numeric.Value)) && !string.Equals(Util.NVC(numeric.Value), Double.NaN.ToString()))
                                        {
                                            // 프레임버그로 값 재 설정 [2017-12-06]
                                            // 액셀 붙여넣기 기능으로 빈칸이 입력될 경우 Convert클래스 이용 시 오류 발생 문제로 체크용 Function 교체 [2019-01-28]
                                            if (!string.IsNullOrWhiteSpace(sValue) && !string.Equals(sValue, "NaN"))
                                            {
                                                //소수점Separator에 따라 분기(우크라이나 언어)
                                                if (sValue.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.CurrentCulture.NumberFormat);
                                                else
                                                    numeric.Value = Convert.ToDouble(sValue, CultureInfo.InvariantCulture.NumberFormat);
                                            }

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
                                        numeric.IsKeyboardFocusWithinChanged -= OnDataCollectGridFocusChanged;
                                        numeric.IsKeyboardFocusWithinChanged += OnDataCollectGridFocusChanged;
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

                        if (e.Cell.Row.Type == DataGridRowType.Bottom)
                        {
                            StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                            ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                            if (e.Cell.Column.Index == dataGrid.Columns["CLSS_NAME1"].Index)
                            {
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                    presenter.Content = ObjectDic.Instance.GetObjectName("평균");
                            }
                            else if (e.Cell.Column.Index == dataGrid.Columns["CLCTVAL01"].Index) // 측정값
                            {
                                if (presenter.HorizontalAlignment != HorizontalAlignment.Right)
                                    presenter.HorizontalAlignment = HorizontalAlignment.Right;

                                decimal sumValue = 0;
                                if (e.Cell.Row.Index == (dataGrid.Rows.Count - 1))
                                {
                                    if (presenter.Content.ToString().Equals("NaN") || presenter.Content.ToString().Equals("非?字"))
                                    {
                                        foreach (C1.WPF.DataGrid.DataGridRow row in dataGrid.Rows)
                                            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01")), Double.NaN.ToString()))
                                                sumValue += string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"))) ? 0 : Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "CLCTVAL01"));


                                        if (sumValue == 0)
                                            presenter.Content = 0;
                                        else
                                            presenter.Content = Util.NVC_Decimal(GetUnitFormatted(sumValue / (dataGrid.Rows.Count - dataGrid.BottomRows.Count), "EA"));
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// 품질항목 스프레드 UnloadedCellPresenter 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgQuality_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e != null && e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            e.Cell.Presenter.Background = null;
                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridFocusChange 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (Convert.ToBoolean(e.NewValue) == true)
                {
                    C1NumericBox item = sender as C1NumericBox;
                    StackPanel panel = item.Parent as StackPanel;
                    C1.WPF.DataGrid.DataGridCellPresenter p = panel.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                    int iRowIdx = p.Cell.Row.Index;
                    int iColIdx = p.Cell.Column.Index;
                    C1.WPF.DataGrid.C1DataGrid grid = p.DataGrid;

                    if (grid.CurrentCell.Column.Index != iColIdx)
                        grid.CurrentCell = grid.GetCell(iRowIdx, iColIdx);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridPreviewItmeKeyDown 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridPreviewItmeKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 품질항목 스프레드 OnDataCollectGridGotKeyboardLost 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridGotKeyboardLost(object sender, KeyboardFocusChangedEventArgs e)
        {
            try
            {
                if (isDupplicatePopup == true)
                {
                    e.Handled = false;
                    return;
                }

                isDupplicatePopup = true;
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
                    string sCSL = Util.NVC(DataTableConverter.GetValue(currentCell.Row.DataItem, "CSL"));
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
                     }

                    if (!string.IsNullOrEmpty(Util.NVC(item.Value)) && !string.Equals(Util.NVC(item.Value), Double.NaN.ToString()))
                    {
                        DataTable dataCollect = DataTableConverter.Convert(grid.ItemsSource);
                        int iHGCount1 = 0;  // H/G
                        int iHGCount2 = 0;  // M/S
                        int iHGCount3 = 0;  // 1차 H/G
                        int iHGCount4 = 0;  // 1차 M/S
                        decimal sumValue1 = 0;
                        decimal sumValue2 = 0;
                        decimal sumValue3 = 0;
                        decimal sumValue4 = 0;
                        foreach (DataRow row in dataCollect.Rows)
                        {
                            //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                            if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "E3000-0001") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue1 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount1++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI022") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue2 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount2++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue3 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount3++;
                                    }
                                }
                            }
                            else if (string.Equals(row["INSP_ITEM_ID"], "SI516") && !Util.NVC(row["CLSS_NAME1"]).Contains("HG"))
                            {
                                if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])))
                                {
                                    if (!string.Equals(Util.NVC(row["CLCTVAL01"]), Double.NaN.ToString()))
                                    {
                                        sumValue4 += Util.NVC_Decimal(row["CLCTVAL01"]);
                                        iHGCount4++;
                                    }
                                }
                            }
                        }
                        //[E20240430-000729] 자주검사 코드 임시 하드코팅 처리
                        if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "E3000-0001") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount1 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount1 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue1 / iHGCount1)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount2 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI022") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount2 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue2 / iHGCount2)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }
                        else if (iHGCount3 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 4);
                            else if (iHGCount3 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue3 / iHGCount3)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께") + "(HG)", 2);
                        }
                        else if (iHGCount4 > 0 && string.Equals(Util.NVC(dataCollect.Rows[iRowIdx]["INSP_ITEM_ID"]), "SI516") && !Util.NVC(dataCollect.Rows[iRowIdx]["CLSS_NAME1"]).Contains("HG"))
                        {
                            if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 4)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 4);
                            else if (iHGCount4 > 0 && Math.Abs(Util.NVC_Decimal(item.Value) - (sumValue4 / iHGCount4)) > 2)
                                Util.MessageValidation("SFU3519", ObjectDic.Instance.GetObjectName("두께"), 2);
                        }

                        if (grid.BottomRows.Count > 0)
                            grid.BottomRows[0].Refresh(false);
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

                }
                else
                    return;

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                isDupplicatePopup = false;
            }
        }

        private string GetUnitFormatted()
        {
            string sFormatted = "0";
            if (!string.IsNullOrEmpty(txtUnit.Text))
            {
                switch (txtUnit.Text)
                {
                    case "KG":
                        sFormatted = "F3";
                        break;

                    case "M":
                        sFormatted = "F2";
                        break;

                    case "EA":
                    default:
                        sFormatted = "F0";
                        break;
                }
            }
            return sFormatted;
        }

        private string GetUnitFormatted(object obj, string pattern)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (pattern)
            {
                case "KG":
                    sFormatted = "{0:###0.000}";
                    break;

                case "M":
                    sFormatted = "{0:###0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:###0.0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        protected virtual void OnCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            switch (cb.Name)
            {
                case "chkWait":
                    if (cb.IsChecked == true)
                    {
                        chkRun.IsChecked = false;
                        chkEqpEnd.IsChecked = false;
                    }
                    else
                    {
                        chkRun.IsChecked = true;
                        chkEqpEnd.IsChecked = true;
                    }
                    break;

                case "chkRun":
                case "chkEqpEnd":
                    if (cb.IsChecked == true)
                    {
                        chkWait.IsChecked = false;
                        chkRun.IsChecked = true;
                        chkEqpEnd.IsChecked = true;
                    }
                    else
                    {
                        chkWait.IsChecked = true;
                        chkRun.IsChecked = false;
                        chkEqpEnd.IsChecked = false;
                    }
                    break;
            }

            if (string.Equals(cb.Name, "chkEqpEnd")) // PROC, EQPT_END가 동시에 체크되기 떄문에 하나만 체크하도록 변경
                return;

            btnSearch_Click(btnSearch, null); //조회 실행
        }

        private DataTable GetEqptMountPstn(string _sEQPTID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow tmprow = dt.NewRow();
                tmprow["EQPTID"] = _sEQPTID;
                dt.Rows.Add(tmprow);

                DataTable dtEqptMount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", dt);

                if (!CommonVerify.HasTableRow(dtEqptMount))
                {
                    Util.MessageValidation("SFU2019");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                    return null;
                }

                return dtEqptMount;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion
    }
}
