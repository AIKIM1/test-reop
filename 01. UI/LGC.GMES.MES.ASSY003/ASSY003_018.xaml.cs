/*************************************************************************************
 Created Date : 2017.11.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Taping 파우치 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.15  INS 김동일K : Initial Created.
  2023.06.25  조영대      : 설비 Loss Level 2 Code 사용 체크 및 변환
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_018.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_018 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private bool bTestMode = false;
        private string sTestModeType = string.Empty;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

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
        bool defectFlag = false;
        bool remarkFlag = false;

        string _LotProcId = string.Empty;
        string _Wipstat = string.Empty;
        //string procid = string.Empty;

        decimal inputOverrate;

        #region Popup 처리 로직 변경

        CMM_SHIFT_USER2 wndShiftUser;

        
        #endregion
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
        public ASSY003_018()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            
            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, Process.TAPING_POUCH };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");

            String[] sFilter1 = { Process.TAPING_POUCH };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_MAIN_LEVEL");

        }
        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            #region Popup 처리 로직 변경

            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

            #endregion
            
            ApplyPermissions();

            GetRewinderLot();

            //string sConvRate = GetCommonCode("INPUTQTY_OVER_RATE", "ELEC_OVER_RATE");
            //inputOverrate = string.IsNullOrEmpty(sConvRate) ? -1 : (Util.NVC_Decimal(sConvRate) * Util.NVC_Decimal(0.01));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HideLoadingIndicator();
                return;
            }

            ClearControls();

            GetRewinderLot();

            GetEqptWrkInfo();
        }
        
        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                ClearControls();

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
        
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (wndShiftUser != null)
                wndShiftUser = null;

            wndShiftUser = new CMM_SHIFT_USER2();
            wndShiftUser.FrameOperation = this.FrameOperation;

            if (wndShiftUser != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.TAPING_POUCH;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShift_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));
            }
        }

        #endregion

        #region [작업대상]

        #endregion

        #region [투입]

        #endregion

        #region [생산]

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]        
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

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.TAPING_POUCH;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
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
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

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
                inData.Columns.Add("SHIFT", typeof(string));
                inData.Columns.Add("WRK_USERID", typeof(string));
                inData.Columns.Add("WRK_USER_NAME", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("WIPNOTE", typeof(string));
                

                DataRow row = inData.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                row["PROCID"] = Process.TAPING_POUCH;
                row["SHIFT"] = txtShift.Tag;
                row["WRK_USERID"] = txtWorker.Tag;
                row["WRK_USER_NAME"] = txtWorker.Text;
                row["WIPNOTE"] = DataTableConverter.GetValue(dgRemark.Rows[1].DataItem, "REMARK");
                row["USERID"] = LoginInfo.USERID;
                
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

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_END_LOT_TP", "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            //Util.AlertByBiz("BR_PRD_REG_START_END_LOT_RW", bizException.Message, bizException.ToString());
                            return;
                        }
                                                
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        
                        //System.Threading.Thread.Sleep(500);

                        //// Winding 이력카드 출력
                        //PrintHistoryCard(_CurrentLot.LOTID);

                        ClearControls();
                        GetRewinderLot();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetRewinderLot()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.TAPING_POUCH;
                row["WIPSTAT"] = Wip_State.WAIT;
                row["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                dt.Rows.Add(row);

                ShowLoadingIndicator();

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_GET_REWINDER_LOT", "INDATA", "OUTDATA", dt);
                if (result.Rows.Count == 0)
                {
                    Util.gridClear(dgRewinderInfo);
                    return;
                }

                Util.GridSetData(dgRewinderInfo, result, FrameOperation, false);
                //dgRewinderInfo.ItemsSource = DataTableConverter.Convert(result);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private bool GetExcptTime(out double dHour)
        {
            dHour = 0;

            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = Process.TAPING_POUCH;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_WORK_EXCP_TIME", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EXCP_TIME"))
                {
                    if (!double.TryParse(Util.NVC(dtRslt.Rows[0]["EXCP_TIME"]), out dHour))
                        dHour = 0;

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
        #endregion

        #region [Validation]

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("설비를 선택 하세요.");
            //    Util.MessageValidation("SFU1673");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CanConfirm()
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

            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }

            if (defectFlag == true)
            {
                Util.MessageValidation("SFU2900");  //불량/Loss/물청 정보를 저장하세요.
                return bRet;
            }

            if (remarkFlag == true)
            {
                Util.MessageValidation("SFU2977"); //특이사항 정보를 저장하세요.
                return bRet;
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                //Util.Alert("근무조를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무조"));
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                //Util.Alert("근무자를 입력해 주세요.");
                Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("근무자"));
                return bRet;
            }

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();

                // 공통코드에 등록된 시간 내의 경우에는 초기화 하지 않도록..
                double dHour = 0;
                if (GetExcptTime(out dHour))
                {
                    if (dHour > 0)
                    {
                        shiftStartDateTime = shiftStartDateTime.AddHours(-dHour);
                        shiftEndDateTime = shiftEndDateTime.AddHours(-dHour);
                    }
                }

                int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                if (prevCheck < 0 || nextCheck > 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [PopUp Event]
        private void wndShift_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                //txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                //txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                //txtWorker.Tag = Util.NVC(wndPopup.USERID);
                //txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                //txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                //txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);                

                GetEqptWrkInfo();
            }
        }
        
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            listAuth.Add(btnSaveDefect);
            listAuth.Add(btnSaveRemark);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        /// <summary>
        /// Main Windows Data Clear 처리
        /// UC_WORKORDER 컨트롤에서 Main Window Data Clear
        /// </summary>
        /// <returns>Clear 완료 여부</returns>
        public void ClearControls(bool bClearRewinderInfo = true)
        {
            try
            {
                if (bClearRewinderInfo)
                    Util.gridClear(dgRewinderInfo);

                Util.gridClear(dgLotInfo);
                Util.gridClear(dgWipReason);
                Util.gridClear(dgRemark);

                //if (txtVersion != null)
                //    txtVersion.Text = "";
                //if (txtLane != null)
                //    txtLane.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        
        #endregion

        #endregion

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            //실적 확정 하시겠습니까?
            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }

        private void btnCard_Click(object sender, RoutedEventArgs e)
        {
            //if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return;
            //}

            ////이력카드를 발행하시겠습니까?
            //Util.MessageConfirm("SFU1772", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        PrintHistoryCard(Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK")].DataItem, "LOTID")));
            //    }
            //});

            if (!ValidationHistoryCard()) return;

            CMM_ASSY_HISTORYCARD popupHistoryCard = new CMM_ASSY_HISTORYCARD { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = DataTableConverter.Convert(cboEquipment.ItemsSource);
            parameters[1] = Util.NVC(cboEquipment.SelectedIndex);
            parameters[2] = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            parameters[3] = Util.NVC(cboEquipmentSegment.SelectedIndex);
            parameters[4] = Process.TAPING_POUCH;
            parameters[5] = false;

            C1WindowExtension.SetParameters(popupHistoryCard, parameters);
            popupHistoryCard.Closed += new EventHandler(popupHistoryCard_Closed);
            grdMain.Children.Add(popupHistoryCard);
            popupHistoryCard.BringToFront();
        }

        private void popupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_HISTORYCARD popup = sender as CMM_ASSY_HISTORYCARD;
            // 이력 팝업 종료후 처리
            this.grdMain.Children.Remove(popup);
        }
        
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            _Util.SetDataGridUncheck(dgRewinderInfo, "CHK", checkIndex);
            if (_Util.GetDataGridCheckFirstRowIndex(dgRewinderInfo, "CHK") == -1)
            {
                ClearControls(false);
                return;
            }

            ClearControls(false);

            GetProcessDetail(checkIndex);
        }

        private void btnSaveDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(dgWipReason))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return;
            }
            if (_CurrentLot.LOTID.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

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

        private void dgRemark_Click(object sender, RoutedEventArgs e)
        {
            if (dgRemark.Rows.Count < 1)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            SetWipNote();
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

        private void GetProcessDetail(int index)
        {
            try
            {
                _CurrentLot.LOTID = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LOTID"));
                _CurrentLot.VERSION = Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "PROD_VER_CODE"));
                _Lane = "1";// Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY")).Equals("") ? 1.ToString() : Util.NVC(DataTableConverter.GetValue(dgRewinderInfo.Rows[index].DataItem, "LANE_QTY"));
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
                DataTableConverter.SetValue(dgRewinderInfo.Rows[index].DataItem, "CHK", true);
                Util.MessageException(ex);
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
                newRow["PROCID"] = Process.TAPING_POUCH;
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

            int laneqty = 1;// int.Parse(txtLane.Text);

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
                    inDataRow["LANE_QTY"] = 1;// txtLane.Text;
                    inDataRow["LANE_PTN_QTY"] = 1;// _Lane_Ptn_qty;
                    inDataRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "COST_CNTR_ID"));
                    IndataTable.Rows.Add(inDataRow);
                }

                ShowLoadingIndicator();

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

                        Util.MessageInfo("SFU1275"); //정상처리되었습니다.

                        GetDefectList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintHistoryCard(string lotId)
        {
            try
            {
                //string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN";
                string bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WN_V01_WP";
                string outPutData = "OUT_DATA,OUT_ELEC";

                DataSet ds = _Biz.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable indataTable = ds.Tables["IN_DATA"];
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = lotId;
                indataTable.Rows.Add(indata);
                //ds.Tables.Add(indataTable);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", outPutData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        CMM_ASSY_WINDERCARD_PRINT poopupHistoryCard = new CMM_ASSY_WINDERCARD_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[5];
                        parameters[0] = result;
                        parameters[1] = false;
                        parameters[2] = lotId;
                        parameters[3] = Process.TAPING_POUCH;
                        parameters[4] = true;
                        C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                        poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                        //grdMain.Children.Add(poopupHistoryCard);
                        //poopupHistoryCard.BringToFront();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDERCARD_PRINT popup = sender as CMM_ASSY_WINDERCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(popup);
        }

        private bool ValidationHistoryCard()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

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
                HideLoadingIndicator();
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
    }
}
