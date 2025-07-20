/*****************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 절연코터 
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.

   절연코터 공정코드 확인 : J/R, Pancake
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
    public partial class ELEC001_009 : IWorkArea
    {
        #region Declaration & Constructor 

        #region
        private string _ValueWOID = string.Empty;  // MUST_MODIFY
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

        #endregion

        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        UcBaseElec _baseForm;

        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_009()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.INS_COATING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #region Init
        #endregion

        #region Lot Info
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
                {
                    return;
                }

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
                        Indata["PROCID"] = Process.COATING;
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

        #region LOT Start / End
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            if (!WorkOrder_chk())
                return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();
            dicParam.Add("PROCID", Util.NVC(_baseForm.PROCID));
            dicParam.Add("EQPTID", Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue));
            dicParam.Add("EQSGID", Util.NVC(_baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue));
            dicParam.Add("COAT_SIDE_TYPE", Util.NVC(_baseForm.COATTYPE_COMBO.SelectedValue));
            if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
            {
                dicParam.Add("RUNLOT", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
            }
            else
            {
                dicParam.Add("RUNLOT", Util.NVC(_baseForm.LOTID));
            }

            ELEC001_LOTSTART _LotStart = new ELEC001_LOTSTART(dicParam);
            _LotStart.FrameOperation = FrameOperation;

            if (_LotStart != null)
            {
                _LotStart.Closed += new EventHandler(LotStart_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                _LotStart.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_LOTSTART window = sender as ELEC001_LOTSTART;

            if (window.DialogResult == MessageBoxResult.OK)
                _baseForm.RunProcess(window._ReturnLotID);
        }

        private void LotEnd_Closed(object sender, EventArgs e)
        {
            ELEC001_007_LOTEND window = sender as ELEC001_007_LOTEND;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot(GetSelectWorkOrderInfo());
            }
        }
        #endregion

        #region LOT Confirm
        private bool CheckMtrl()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHKMTRL_CT", "INDATA", "RSLTDT", inTable);
                if (DataTableConverter.GetValue(dt, "CNT").ToString() == "0")
                {
                    Util.MessageValidation("SFU1514");  //등록된 슬러리 데이터가 없습니다.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;
        }

        private void ConfirmProcess()
        {
            try
            {
                string messagefirm = "";
                //믹서 버전 Check
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VERSION_CT", "INDATA", "RSLTDT", inTable);

                if (dt.Rows.Count > 0)
                {
                    //{0} : 코터와 버전이 틀린 믹서입니다. 그대로 진행 하시겠습니까?
                    messagefirm = MessageDic.Instance.GetMessage("SFU2884", new object[] { dt.Rows[0]["BATCHID"].ToString() });
                }

                Util.MessageConfirm(messagefirm, (sresult) =>
                {
                    if (sresult == MessageBoxResult.No)
                    {
                        return;
                    }
                });

                //실적확정 FIFO
                inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("QR_GET_EQPENDCHK", "INDATA", "RSLTDT", inTable);

                if (dt.Rows[0]["LOTID"].ToString().Equals(_LOTID))
                {
                    Util.MessageValidation("SFU2046", new object[] { dt.Rows[0]["LOTID"].ToString() }); //{0} 확정부터 진행하세요.
                    return;
                }
                if (_FINALCUT == "Y")
                {
                    inTable = new DataTable();
                    inTable.Columns.Add("LOTID", typeof(string));

                    indata = inTable.NewRow();
                    indata["LOTID"] = _LOTID;
                    inTable.Rows.Add(indata);

                    dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_LIST", "INDATA", "RSLTDT", inTable);

                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; ++i)
                        {
                            if (i == 0)
                            {
                                Listdata = string.Concat(Listdata, " ", dt.Rows[i]["LOTID"].ToString());
                            }
                            else
                            {
                                Listdata = string.Concat(Listdata, ", ", dt.Rows[i]["LOTID"].ToString());
                            }
                        }
                    }
                    messagefirm = MessageDic.Instance.GetMessage("SFU2043", new object[] { _LOTID });   //확정 하려는{0} LOT은 FINAL CUT 입니다.\r\n확정 하시면 대랏은 종결처리되며, LOT 추가/삭제는 불가합니다.\r\n저장 하시겠습니까?
                    ConfirmLotList(messagefirm);
                    if (!final_reform) { return; }
                }
                else
                {
                    messagefirm = MessageDic.Instance.GetMessage("SFU1241");    //저장하시겠습니까?
                }
                //실적확정
                inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUTQTY", typeof(string));
                //inTable.Columns.Add("CTRLQTY", typeof(string));
                inTable.Columns.Add("LOSSQTY", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("COATVER", typeof(string));
                inTable.Columns.Add("FINAL_CHECK", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));

                indata = inTable.NewRow();
                indata["LOTID"] = _LOTID;
                indata["PROCID"] = Process.COATING;
                indata["EQPTID"] = _EQPTID;
                //indata["INPUTQTY"] = Util.NVC(txtGoodqty.Text);
                //indata["CTRLQTY"] = Util.NVC(txt..);
                //indata["LOSSQTY"] = Util.NVC(txtLossqty.Text);
                //indata["WIPNOTE"] = Util.NVC(txtRemark.Text);
                //indata["COATVER"] = Util.NVC(txtVersion.Text);
                indata["FINAL_CHECK"] = _FINALCUT;
                //indata["USERID"] = Util.NVC(txtOper.Text.ToString());
                //indata["SHIFT"] = Util.NVC(cboShift.SelectedValue.ToString());
                inTable.Rows.Add(indata);
                //MUST_BIZ_APPLY
                dt = new ClientProxy().ExecuteServiceSync("ECOM_END_LOT", "INDATA", "RSLTDT", inTable);
                Util.AlertInfo("SFU1275");  //정상처리되었습니다.
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
            {
                final_reform = true;
            }
        }
        #endregion

        #region Function
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