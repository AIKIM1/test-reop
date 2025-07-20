/*****************************************
 Created Date : 2016.10.14
      Creator : 金亨根
   Decription : 하프슬리터 공정진척
------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
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
    public partial class ELEC001_015 : IWorkArea
    {
        #region Declaration & Constructor 

        #region
        private string _ValueWOID = string.Empty;
        private string _LargeLOTID = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _LOTIDPR = string.Empty;
        private string _CUTID = string.Empty;
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
        private string _LANEQTY = string.Empty;
        private string sEQPTID = string.Empty;
        string cut = string.Empty;
        #endregion

        DataTable dtMain = new DataTable();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        UcBaseElec _baseForm;

        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_015()
        {
            InitializeComponent();
            InitInheritance();
        }

        void InitInheritance()
        {
            _baseForm = new UcBaseElec();
            grdMain.Children.Add(_baseForm);
            _baseForm.PROCID = Process.HALF_SLITTING;
            _baseForm.STARTBUTTON.Click += btnStart_Click;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            _baseForm.FrameOperation = FrameOperation;
            _baseForm.SetApplyPermissions();
            //ApplyPermissions();
        }
        #endregion

        #region Event
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

        #region Transaction
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!WorkOrder_chk())
                return;

            if (ValidateRun())
            {
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("PROCID", Process.HALF_SLITTING);
                dicParam.Add("EQPTID", Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue));
                dicParam.Add("EQSGID", Util.NVC(_baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue));
                if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
                    dicParam.Add("RUNLOT", Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR")));
                dicParam.Add("COAT_SIDE_TYPE", "");

                sEQPTID = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue);  // 작업시작용

                ELEC001_LOTSTART _LotStart = new ELEC001_LOTSTART(dicParam);
                _LotStart.FrameOperation = FrameOperation;

                if (_LotStart != null)
                {
                    _LotStart.Closed += new EventHandler(LotStart_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => _LotStart.ShowModal()));
                    _LotStart.CenterOnScreen();
                }

                //ELEC001_LOTSTART _LotStart = new ELEC001_LOTSTART();
                //_LotStart.FrameOperation = FrameOperation;

                //if (_LotStart != null)
                //{
                //    object[] Parameters = new object[5];
                //    Parameters[0] = Process.HALF_SLITTING;
                //    Parameters[1] = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue);
                //    Parameters[2] = Util.NVC(_baseForm.EQUIPMENTSEGMENT_COMBO.SelectedValue);
                //    if (new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK") != -1)
                //    {
                //        Parameters[3] = Util.NVC(DataTableConverter.GetValue(_baseForm.PRODLOT_GRID.Rows[new Util().GetDataGridCheckFirstRowIndex(_baseForm.PRODLOT_GRID, "CHK")].DataItem, "LOTID_PR"));
                //    }
                //    Parameters[4] = "";
                //    sEQPTID = Util.NVC(_baseForm.EQUIPMENT_COMBO.SelectedValue);  // 작업시작용

                //    C1WindowExtension.SetParameters(_LotStart, Parameters);

                //    _LotStart.Closed += new EventHandler(LotStart_Closed);
                //    _LotStart.ShowModal();
                //    _LotStart.CenterOnScreen();
                //}
            }
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            //장비완료 UI에서 없음
            ELEC001_014_LOTEND _LotEnd = new ELEC001_014_LOTEND();
            _LotEnd.FrameOperation = FrameOperation;

            if (_LotEnd != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = _PRODID;
                Parameters[1] = _WORKDATE;
                Parameters[2] = _LOTID;
                Parameters[3] = _WIPSTAT;
                //Parameters[4] = Util.NVC(_baseForm.EquipmentCombo.SelectedValue.ToString());

                C1WindowExtension.SetParameters(_LotEnd, Parameters);

                _LotEnd.Closed += new EventHandler(LotEnd_Closed);
                _LotEnd.ShowModal();
                _LotEnd.CenterOnScreen();
            }
        }

        private void LotStart_Closed(object sender, EventArgs e)
        {
            ELEC001_LOTSTART _LotStart = sender as ELEC001_LOTSTART;

            if (_LotStart.DialogResult == MessageBoxResult.OK)
                _baseForm.RunProcess(_LotStart._ReturnLotID);
        }

        private void LotEnd_Closed(object sender, EventArgs e)
        {
            ELEC001_014_LOTEND window = sender as ELEC001_014_LOTEND;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                //GetWorkOrder();
                GetProductLot();
            }
        }

        private void btnFinalCut_Click(object sender, RoutedEventArgs e)
        {
            //if (_Util.GetDataGridCheckCnt(_baseForm.ProdLotGrid, "CHK") < 0)
            //    return;

            //int _iRow = _Util.GetDataGridCheckFirstRowIndex(_baseForm.ProdLotGrid, "CHK");
            //string lotid = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["LOTID"].ToString()));
            //string lotid_pr = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["PARENTLOT"].ToString()));
            //string childseq = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["CHILD_GR_SEQNO"].ToString()));
            //string status = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["WIPSTAT"].ToString()));

            //if (status != "EQPT_END")
            //{
            //    Util.Alert("SFU1864");  //장비완료 상태의 LOT이 아닙니다.
            //    return;
            //}

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID_PR", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = inTable.NewRow();
                //indata["LOTID_PR"] = lotid_pr;
                indata["PROCID"] = Process.HALF_SLITTING;
                string _BizRule = "QR_ROLLPRESS_MAXSEQ";
                DataTable _DT = new ClientProxy().ExecuteServiceSync(_BizRule, "INDATA", "RSLTDT", inTable);

                string maxseq = DataTableConverter.GetValue(_DT, "MAXSEQ").ToString();

                //if (childseq != maxseq)
                //{
                //    Util.Alert("SFU1256");  //마지막에 생성된 LOTID를 선택 해 주세요
                //    return;
                //}

                CMM_FCUT wndFCut = new CMM_FCUT();
                wndFCut.FrameOperation = FrameOperation;

                if (wndFCut != null)
                {
                    object[] Parameters = new object[3];
                    //Parameters[0] = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["FINALCUT"].ToString()));
                    //Parameters[1] = Util.NVC(DataTableConverter.GetValue(_baseForm.ProdLotGrid.Rows[_iRow].DataItem, _baseForm.ProdLotGrid.Columns["CHILD_GR_SEQNO"].ToString()));
                    C1WindowExtension.SetParameters(wndFCut, Parameters);

                    wndFCut.Closed += new EventHandler(wndFCut_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndFCut.ShowModal()));
                }

                if (wndFCut.FINALCUT == "")
                    return;

                //MUST_BIZ_APPLY\
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
                        //Indata["LOTID"] = lotid;
                        Indata["EQPTID"] = _EQPTID;
                        Indata["PROCID"] = Process.HALF_SLITTING;
                        Indata["FINALCUT"] = wndFCut.FINALCUT;
                        Indata["USERID"] = LoginInfo.USERID;

                        IndataTable.Rows.Add(Indata);
                        // MUST_BIZ_APPLY
                        _BizRule = "ECOM_CHANGE_LOTCUTINFO_V01";

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

        #region Mehod

        #region Lot Info
        private void GetProductLot(DataRow drWDInfo)
        {
            try
            {
                if (drWDInfo == null)
                    return;

                Util.gridClear(_baseForm.PRODLOT_GRID);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["WOID"] = _ValueWOID;
                Indata["PROCID"] = Process.HALF_SLITTING;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_HS", "INDATA", "RSLTDT", IndataTable);

                _baseForm.PRODLOT_GRID.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetProductLot()
        {
        }

        private void GetLotInfo(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            _LOTID = rowview["LOTID"].ToString();
            _INPUTQTY = rowview["WIPQTY"].ToString();
            _WIPSTAT = rowview["WIPSTAT"].ToString();
            _WIPDTTM_ST = rowview["WIPDTTM_ST"].ToString();
            _WIPDTTM_ED = rowview["WIPDTTM_ED"].ToString();
            _REMARK = rowview["REMARK"].ToString();
            _PRODID = rowview["PRODID"].ToString();
            _VERSION = rowview["MIXER"].ToString();
            _EQPTID = rowview["EQPTID"].ToString();
            _WIPSTAT_NAME = rowview["WIPSNAME"].ToString();
            _LOTIDPR = rowview["LOTID_PR"].ToString();
            _CUTID = rowview["CUT_ID"].ToString();

            if (_WIPSTAT != "WAIT")
            {
                //txtVersion.Text = _VERSION;
                //txtStartTime.Text = _WIPDTTM_ST;
                //dtpEndDateTime.DateTime = Util.StringToDateTime(_WIPDTTM_ED);

                _baseForm.WIPSTATUS = _WIPSTAT;
                _baseForm.LOTID = _WIPSTAT == "EQPT_END" ? _LOTIDPR : _LOTID;
            }
        }
        #endregion

        #region LOT Start / Cancel / End
        private bool ValidateRun()
        {
            return true;
        }

        private bool ValidateCancel()
        {
            return true;
        }

        private bool ValidateEqpEnd()
        {
            return true;
        }
        #endregion

        private void wndFCut_Closed(object sender, EventArgs e)
        {
            CMM_COATERFCUT window = sender as CMM_COATERFCUT;

            if (window.DialogResult == MessageBoxResult.OK)
                if (window.FINALCUT.Equals(""))
                    return;
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