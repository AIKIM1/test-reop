/*****************************************
 Created Date : 2016.10.14
      Creator : 
   Decription : 단면코터 공정진척
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

   1.설비종료 없음 : 주석처리 
   2.fn_QualitydataTotalAvg() : 기능적용
******************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_008 : IWorkArea
    {
        #region Declaration & Constructor 
        DataSet inDataSet = null;
        DataSet _EDCDataSet = null;
        #region
        private string _ValueWOID = string.Empty;
        private string _LargeLOTID = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _WIPSTAT = string.Empty;
        private string _INPUTQTY = string.Empty;
        private string _OUTPUTQTY = string.Empty;
        private string _CTRLQTY = string.Empty;
        private string _GOODQTY = string.Empty;
        private string _LOSSQTY = string.Empty;
        private string _WORKORDER = string.Empty;
        private string _WORKDATE = string.Empty;
        private string _VERSION = string.Empty;
        private string _PRODID = string.Empty;
        private string _WIPDTTM_ST = string.Empty;
        private string _WIPDTTM_ED = string.Empty;
        private string _REMARK = string.Empty;
        private string _CONFIRMUSER = string.Empty;
        private string _FINALCUT = string.Empty;
        private string _WIPSTAT_NAME = string.Empty;
        private string Listdata = string.Empty;
        private bool gDefectChangeFlag = false;
        private bool final_reform = false;
        private bool _TopOper = false;
        private bool _BackOper = false;
        private bool _RwOper = false;
        #endregion
        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        UcBaseElec _baseForm;

        public ELEC001_008()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.SINGLECOATER = true;
            _baseForm.PROCID = Process.COATING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }

        public IFrameOperation FrameOperation { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
            //ApplyPermissions();
        }
        #endregion

        #region Event

        #region 실적확인
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
#if false
            if (ValidateConfirm())
                ConfirmProcess();
#endif
            ConfirmProcess();
        }
        #endregion

        #endregion

        #region Mehod

        #region Init
        private void ApplyPermissions()
        {
            _baseForm.FrameOperation = FrameOperation;

            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Lot Info
        private void SetDeleteMLot(string LOTID_LARGE)
        {
            //대LOT을 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1489", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LOTID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LOTID"] = LOTID_LARGE;
                    IndataTable.Rows.Add(Indata);
                    // MUST_BIZ_APPLY
                    string _BizRule = "대LOT 삭제";

                    new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.AlertByBiz(_BizRule, ex.Message, ex.ToString());
                            return;
                        }

                        Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                    });
                }
            });
        }

        private void SetFinalCut(string LotID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_LARGE", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = LotID;
                string _BizRule = "DA_PRD_SEL_MAXLOT_CT";
                DataTable _DT = new ClientProxy().ExecuteServiceSync(_BizRule, "INDATA", "RSLTDT", inTable);

                string maxlotid = _DT.Rows[0]["MAXLOTID"].ToString();

                if (LotID != maxlotid)
                {
                    Util.MessageValidation("SFU1445");  //가장 큰 LOTID를 선택해주세요
                    return;
                }

                //LOT ID로 CUT 체크 함 MUST_MODIFY
                int cutNo = int.Parse(maxlotid.Substring(7, 1));

                // Final Cut PopUP 호출(Coater 전용 )
                string fcut = ""; // MLBUtil.PopupCoaterFinalChk(GridUtil.GetValue(spdLotID, rowidx, "WIPSTAT").ToString(), cutNo);

                if (_WIPSTAT == "PROC" || _WIPSTAT == "WAIT")
                {
                    Util.MessageValidation("SFU1269");  //장비완료 상태가 아닙니다.
                    return;
                }

                CMM_COATERFCUT wndFCut = new CMM_COATERFCUT();
                wndFCut.FrameOperation = FrameOperation;

                if (wndFCut != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = _WIPSTAT;
                    Parameters[1] = cutNo;
                    C1WindowExtension.SetParameters(wndFCut, Parameters);

                    wndFCut.Closed += new EventHandler(wndFCut_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndFCut.ShowModal()));
                }

                if (fcut.Equals(""))
                    return;

                //MUST_BIZ_APPLY
                //Final Cut 상태를 변경 하시겠습니까?
                Util.MessageConfirm("SFU1210", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        DataTable IndataTable = new DataTable();
                        IndataTable.Columns.Add("LOTID", typeof(string));
                        IndataTable.Columns.Add("EQPTID", typeof(string));
                        IndataTable.Columns.Add("PROCID", typeof(string));
                        IndataTable.Columns.Add("FINALCUT", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["LOTID"] = LotID;
                        Indata["EQPTID"] = _EQPTID;
                        Indata["PROCID"] = Process.TOP_COATING;
                        Indata["FINALCUT"] = fcut;
                        Indata["USERID"] = LoginInfo.USERID;

                        IndataTable.Rows.Add(Indata);
                        // MUST_BIZ_APPLY
                        _BizRule = "대LOT 삭제";

                        new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz(_BizRule, ex.Message, ex.ToString());
                                return;
                            }

                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region LOT Start / End / Cancel
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //if (GetSelectWorkOrderInfo() == null)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업지시를 선택 후 작업하십시오."), null, "Warning", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            if (!WorkOrder_chk())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("EQPTID", Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue.ToString()));
            dicParam.Add("EQSGID", Util.NVC(_baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue));
            dicParam.Add("LARGELOT", Util.NVC(_baseForm.LARGELOTID));
            dicParam.Add("WODETIL", Util.NVC(_baseForm.WO_DETL_ID));
            dicParam.Add("COATSIDE", Util.NVC(_baseForm.COATTYPE_COMBO.SelectedValue));
            dicParam.Add("SINGL", "true");
            if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
            {
                dicParam.Add("LOTID_PR", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
            }
            else
            {
                dicParam.Add("LOTID_PR", "");
            }
            ELEC001_007_LOTSTART _LotStart = new ELEC001_007_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;
            _LotStart.IsSingleCoater = true;
            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(_baseForm.PRODLOT_GRID, "CHK") == 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }

            ELEC001_008_LOTEND _LotEnd = new ELEC001_008_LOTEND();
            _LotEnd.FrameOperation = FrameOperation;

            if (_LotEnd != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _PRODID;
                Parameters[1] = _WORKDATE;
                Parameters[2] = _LOTID;
                Parameters[3] = _WIPSTAT;
                Parameters[4] = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue.ToString());
                C1WindowExtension.SetParameters(_LotEnd, Parameters);

                _LotEnd.Closed += new EventHandler(LotEnd_Closed);
                _LotEnd.ShowModal();
                _LotEnd.CenterOnScreen();
            }
        }

        //Func<string, bool> isStart = x => x == Wip_State.EQPT_END;

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_007_LOTSTART window = sender as ELEC001_007_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.REFRESH = true;
        }

        private void LotEnd_Closed(object sender, EventArgs e)
        {
            ELEC001_008_LOTEND window = sender as ELEC001_008_LOTEND;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot(GetSelectWorkOrderInfo());
                //GetProductLot();
            }
        }
        #endregion

        #region LOT Confirm
        private void ConfirmProcess()
        {
            try
            {
                string _ValueToMessage = string.Empty;

                //믹서 버전 Check
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VERSION_CT", "INDATA", "RSLTDT", inTable);
#if false
                if (dt.Rows.Count > 0)
                {
                    _ValueToMessage = dt.Rows[0]["BATCHID"].ToString() + " : 코터랑 버전이 다른 믹서입니다! 그대로 진행 하시겠습니까";
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(_ValueToMessage), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.No)
                        {
                            return;
                        }
                    });
                }
#endif


#if false
                //실적확정 FIFO : 이전 Cut 장비완료 Validation 
                inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("QR_GET_EQPENDCHK", "INDATA", "RSLTDT", inTable);

                if (dt.Rows[0]["LOTID"].ToString().Equals(_LOTID))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(dt.Rows[0]["LOTID"].ToString() + " 확정부터 진행하세요!"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None);
                    return;
                }
#endif

                //if (chkFinalCut.IsChecked.Value)
                //{
                    inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));

                    indata = inTable.NewRow();
                    indata["LOTID"] = _LOTID;
                    inTable.Rows.Add(indata);

                    dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", inTable);

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; ++i)
                            if (i == 0)
                                Listdata = string.Concat(Listdata, " ", dt.Rows[i]["LOTID"].ToString());
                            else
                                Listdata = string.Concat(Listdata, ", ", dt.Rows[i]["LOTID"].ToString());
                    }

                    _ValueToMessage = MessageDic.Instance.GetMessage("SFU2043", new object[] { _LOTID });   //확정 하려는{0} LOT은 FINAL CUT 입니다.\r\n확정 하시면 대랏은 종결처리되며, LOT 추가/삭제는 불가합니다.\r\n저장 하시겠습니까?
                //}
                //else
                //{
                //    _ValueToMessage = "SFU1241";
                //}

                Util.MessageConfirm(_ValueToMessage, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
#if false
                        inDataSet = new DataSet();

                        inTable = inDataSet.Tables.Add("INDATA");

                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("INPUTQTY", typeof(string));
                        inTable.Columns.Add("OUTPUTQTY", typeof(string));
                        inTable.Columns.Add("RESNQTY", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("LAST_FLAG", typeof(string));

                        indata = inTable.NewRow();
                        indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        indata["LANGID"] = LoginInfo.LANGID;
                        indata["IFMODE"] = IFMODE.IFMODE_OFF;
                        indata["EQPTID"] = _EQPTID;
                        indata["LOTID"] = _LOTID;
                        indata["INPUTQTY"] = Util.NVC(txtInputqty.Text);
                        indata["OUTPUTQTY"] = Util.NVC_DecimalStr(txtGoodqty.Value);
                        indata["RESNQTY"] = Util.NVC(txtLossqty.Text) == "" ? "0" : Util.NVC(txtLossqty.Text);
                        indata["PROD_VER_CODE"] = Util.NVC(txtVersion.Text);
                        indata["SHIFT"] = "A"; // Util.NVC(txtShift.Text);               // 작업조 적용
                        indata["WIPDTTM_ED"] = Util.StringToDateTime(dtpEndDateTime.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));
                        indata["WIPNOTE"] = Util.NVC(txtRemark.Text);
                        indata["LAST_FLAG"] = chkFinalCut.IsChecked.Value == true ? "Y" : "N";
                        if (LoginInfo.CFG_AREA_ID == "E6")  // 2공장
                        {
                            indata["USERID"] = "ADMIN"; // Util.NVC(txtTopOper.Text.ToString() + "," + txtBackOper.Text.ToString() + "," + txtRwOper.Text.ToString());
                        }
                        else
                        {
                            indata["USERID"] = "ADMIN"; // Util.NVC(txtTopOper.Text.ToString());  // 작업자 적용
                        }
                        inTable.Rows.Add(indata);

                        DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));

                        DataRow inMtrlDataRow = null;

                        inMtrlDataRow = InMtrldataTable.NewRow();
                        inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_MOUNT_PSTN01";
                        inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        inMtrlDataRow["INPUT_LOTID"] = "ASLCA0183A";

                        InMtrldataTable.Rows.Add(inMtrlDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CT", "INDATA,IN_INPUT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                            ClearControls();
                            GetWorkOrder();
                            GetProductLot();

                        }, inDataSet);

#else
                        //실적확정
                        inTable = new DataTable();
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("INPUTQTY", typeof(string));
                        inTable.Columns.Add("OUTPUTQTY", typeof(string));
                        inTable.Columns.Add("RESNQTY", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("LAST_FLAG", typeof(string));

                        indata = inTable.NewRow();
                        indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        indata["LANGID"] = LoginInfo.LANGID;
                        indata["IFMODE"] = IFMODE.IFMODE_OFF;
                        indata["EQPTID"] = _EQPTID;
                        indata["LOTID"] = _LOTID;
                        //indata["INPUTQTY"] = Util.NVC(txtInputqty.Text);
                        //indata["OUTPUTQTY"] = Util.NVC_DecimalStr(txtGoodqty.Value);
                        //indata["RESNQTY"] = Util.NVC(txtLossqty.Text) == "" ? "0" : Util.NVC(txtLossqty.Text);
                        //indata["PROD_VER_CODE"] = Util.NVC(txtVersion.Text);
                        indata["SHIFT"] = "A"; // Util.NVC(txtShift.Text);               // 작업조 적용
                        //indata["WIPDTTM_ED"] = Util.StringToDateTime(dtpEndDateTime.DateTime.Value.ToString("yyyy-MM-dd HH:mm"));
                        //indata["WIPNOTE"] = Util.NVC(txtRemark.Text);
                        //indata["LAST_FLAG"] = chkFinalCut.IsChecked.Value == true ? "Y" : "N";

                        if (LoginInfo.CFG_AREA_ID == "E6")  // 2공장
                        {
                            indata["USERID"] = "ADMIN"; // Util.NVC(txtTopOper.Text.ToString() + "," + txtBackOper.Text.ToString() + "," + txtRwOper.Text.ToString());
                        }
                        else
                        {
                            indata["USERID"] = "ADMIN"; // Util.NVC(txtTopOper.Text.ToString());  // 작업자 적용
                        }

                        inTable.Rows.Add(indata);

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_LOT_CT", "INDATA", null, inTable, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.AlertByBiz("BR_PRD_REG_END_LOT_CT", ex.Message, ex.ToString());
                                return;
                            }

                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.

                            //ClearControls();
                            //GetWorkOrder();
                            //GetProductLot();
                        });
                        //작업자 세분화 미적용
                        //모델변환율 미적용

                        //////////////GetLargeLot();
#endif


                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmLotList(string msg)
        {
            ELEC001_CONFIRM _LotConfirm = new ELEC001_CONFIRM();
            _LotConfirm.FrameOperation = FrameOperation;

            if (_LotConfirm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _LOTID;
                Parameters[1] = msg;
                C1WindowExtension.SetParameters(_LotConfirm, Parameters);

                _LotConfirm.Closed += new EventHandler(LotConfirm_Closed);
                _LotConfirm.ShowModal();
                _LotConfirm.CenterOnScreen();
            }
        }

        private void LotConfirm_Closed(object sender, EventArgs e)
        {
            ELEC001_CONFIRM window = sender as ELEC001_CONFIRM;

            if (window.DialogResult == MessageBoxResult.OK)
                final_reform = true;
        }
        #endregion

        #region Function
        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sWOID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["WOID"] = sWOID;
            IndataTable.Rows.Add(Indata);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_TB_SFC_WO_MTRL", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
        }
        private void wndFCut_Closed(object sender, EventArgs e)
        {
            CMM_COATERFCUT window = sender as CMM_COATERFCUT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (window.FINALCUT.Equals(""))
                    return;
            }
        }

        // 선택한 W/O 와 선택되어있는 W/O 다를때 메시지 추가 요청
        public bool WorkOrder_chk()
        {
            bool _Woder = true;
            if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.WORKORDER_GRID, "CHK") != -1)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(_baseForm.WORKORDER_GRID, "CHK");

                if (Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[0].DataItem, "EIO_WO_SEL_STAT")) == "Y")
                {
                    if (Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[0].DataItem, "WOID")) != Util.NVC(DataTableConverter.GetValue(_baseForm.WORKORDER_GRID.Rows[idx].DataItem, "WOID")))
                    {
                        Util.MessageValidation("SFU1436");
                        _Woder = false;
                    }
                    else
                    {
                        _Woder = true;
                    }
                }
            }
            return _Woder;
        }
        #endregion

        #endregion
    }
}