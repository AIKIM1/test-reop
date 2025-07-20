/*************************************************************************************
 Created Date : 2020.12.02
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 투입요청서 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.04  조영대 : Initial Created.
  2021.09.28  김지은 : 투입 단위 중량 기준 추가
  2023.04.20  강성묵 : 원자재 출고 요청시 진행중인 자재 존재 여부 체크
  2023.10.25  장영철 : 자재투입요청서 QR 추가 (GM2 Pjt)
  2024.05.17  배현우 : 오창 하도급 적용 (권한별 승인,반려 버튼추가 및 조회쿼리 추가)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.ELEC003.Controls;
using System.ComponentModel;
using LGC.GMES.MES.ELEC001;
using System.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ELEC003
{
    public partial class ELEC003_002_MONITORING : UserControl, IWorkArea
    {
        #region Declaration

        public event MinusButtonClickEventHandler MinusButtonClick;
        public delegate void MinusButtonClickEventHandler(object sender, string equipmentSegment, string process, string equipment);

        public delegate void HopperDoubleClickEventHandler(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId);
        public event HopperDoubleClickEventHandler HopperDoubleClick;

        private SoundPlayer player;
        private System.Windows.Threading.DispatcherTimer tmInputRequest = new System.Windows.Threading.DispatcherTimer();

        private Util util = new Util();

        private System.Windows.Threading.DispatcherTimer refreshTimer = null;

        private bool uses_PNTR_WRKR_TYPE = false;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string ProcessCode { get; set; }

        // 2023.04.20  강성묵: 원자재 출고 요청시 진행중인 자재 존재 여부 체크
        private bool bChkRmtrlShip = false;

        #endregion

        #region Initialize
        public ELEC003_002_MONITORING()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            SetComboBox();

            InitializeUserControls();

            if (IsRemoveAreaHardCode("VIEW_STATION_ID")) {
                dgInputRequest.Columns["STATION_ID"].Visibility = Visibility.Visible;
            }
        }

        private void SetComboBox()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            CommonCombo combo = new CommonCombo();

            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cbProcessParent, sCase: "ProcessCWA");

            // 설비
            SetEquipmentCombo(cboEquipment);

            // 극성
            String[] sFilter1 = { "ELTR_TYPE_CODE" };
            combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // 자동조회
            String[] sFilter3 = { "DRB_REFRESH_TERM_LONG" };
            combo.SetCombo(cboAutoSearch, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");
        }

        private void InitializeUserControls()
        {
            if (IsCommonCode("SUBCONT_SHOP", LoginInfo.CFG_SHOP_ID))
                uses_PNTR_WRKR_TYPE = true;

            if (uses_PNTR_WRKR_TYPE)
            {
                dgInputRequest.Columns["APPR_REQ_RSLT_NAME"].Visibility = Visibility.Visible;
                if (LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))
                {
                    btnAccept.Visibility = Visibility.Visible;
                    btnReject.Visibility = Visibility.Visible;
                    btnDelete.Visibility = Visibility.Collapsed;
                }
            }
                tmInputRequest.Interval = TimeSpan.FromSeconds(10);
        }
        
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            Initialize();

            this.Loaded -= UserControl_Loaded;

            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    if (ProcessCode != null && !ProcessCode.Equals(string.Empty))
                    {
                        cboProcess.SelectedValue = ProcessCode;

                        this.Refresh(Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboProcess.SelectedValue), Util.NVC(cboEquipment.SelectedItemsToString));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void tmInputRequest_Tick(object sender, EventArgs e)
        {
            if (!chkRequstAlarm.IsChecked.Value)
            {
                tmInputRequest.Stop();
                tmInputRequest.Tick -= tmInputRequest_Tick;
                return;
            }
            
            CheckRequest();
        }

        private void Detail_Closed(object sender, EventArgs e)
        {
            ELEC001_002_DETAIL_CWA window = sender as ELEC001_002_DETAIL_CWA;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
                tmInputRequest.IsEnabled = true;
                this.player.Stop();
            }
        }

        private void chkRequstAlarm_Checked(object sender, RoutedEventArgs e)
        {
            tmInputRequest.Tick += tmInputRequest_Tick;
            tmInputRequest.Interval = TimeSpan.FromSeconds(10);
            tmInputRequest.Start();

            CheckRequest();
        }

        private void chkRequstAlarm_Unchecked(object sender, RoutedEventArgs e)
        {
            tmInputRequest.Stop();
            tmInputRequest.Tick -= tmInputRequest_Tick;
        }
        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            MinusButtonClick?.Invoke(btnMinus, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboProcess.SelectedValue), Util.NVC(cboEquipment.SelectedItemsToString));
        }
        

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {            
            SetEquipmentCombo(cboEquipment);
        }

        private void cboPolar_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentCombo(cboEquipment);
        }
        

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh(Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboProcess.SelectedValue), Util.NVC(cboEquipment.SelectedItemsToString));
        }
        
        private void btnSearchRequest_Click(object sender, RoutedEventArgs e)
        {
            SearchRequestList();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> indexes = dgInputRequest.GetCheckedRowIndex("CHK");
                if (indexes != null && indexes.Count.Equals(0))
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }
           
                DataTable dtColumnLang = new DataTable();
                dtColumnLang.Columns.Add("믹서투입요청서", typeof(string));
                dtColumnLang.Columns.Add("담당자", typeof(string));
                dtColumnLang.Columns.Add("요청서번호", typeof(string));
                dtColumnLang.Columns.Add("요청이력", typeof(string));
                dtColumnLang.Columns.Add("자재상세정보", typeof(string));
                dtColumnLang.Columns.Add("요청일자", typeof(string));
                dtColumnLang.Columns.Add("프로젝트명", typeof(string));
                dtColumnLang.Columns.Add("요청작업자", typeof(string));
                dtColumnLang.Columns.Add("요청장비", typeof(string));
                dtColumnLang.Columns.Add("특이사항", typeof(string));
                dtColumnLang.Columns.Add("자재", typeof(string));
                dtColumnLang.Columns.Add("자재규격", typeof(string));
                dtColumnLang.Columns.Add("요청중량", typeof(string));
                dtColumnLang.Columns.Add("REQ_BAG_QTY", typeof(string));
                dtColumnLang.Columns.Add("호퍼", typeof(string));

                DataTable ReqPrintTag = new DataTable();
                ReqPrintTag.Columns.Add("REQ_DTTM", typeof(string));
                ReqPrintTag.Columns.Add("REQ_ID", typeof(string));
                ReqPrintTag.Columns.Add("REQ_ID1", typeof(string));

                ReqPrintTag.Columns.Add("QR_REQ_ID", typeof(string));

                ReqPrintTag.Columns.Add("USERNAME", typeof(string));
                ReqPrintTag.Columns.Add("NOTE", typeof(string));
                ReqPrintTag.Columns.Add("EPQTID", typeof(string));
                ReqPrintTag.Columns.Add("PRJT_NAME", typeof(string));

                DataTable ReqPrintTagDetl = new DataTable();
                ReqPrintTagDetl.Columns.Add("MTRLID_0", typeof(string));
                ReqPrintTagDetl.Columns.Add("MTRLDESC_0", typeof(string));
                ReqPrintTagDetl.Columns.Add("QTY_0", typeof(decimal));
                ReqPrintTagDetl.Columns.Add("BAG_QTY_0", typeof(int));
                ReqPrintTagDetl.Columns.Add("HOPPER_0", typeof(string));

                foreach (int rowIndex in indexes)
                {


                    DataRow drCrad = dtColumnLang.NewRow();

                    drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("믹서투입요청서"),
                                                  ObjectDic.Instance.GetObjectName("담당자"),
                                                  ObjectDic.Instance.GetObjectName("요청서번호"),
                                                  ObjectDic.Instance.GetObjectName("요청이력"),
                                                  ObjectDic.Instance.GetObjectName("자재상세정보"),
                                                  ObjectDic.Instance.GetObjectName("요청일자"),
                                                  ObjectDic.Instance.GetObjectName("프로젝트명"),
                                                  ObjectDic.Instance.GetObjectName("요청작업자"),
                                                  ObjectDic.Instance.GetObjectName("요청장비"),
                                                  ObjectDic.Instance.GetObjectName("특이사항"),
                                                  ObjectDic.Instance.GetObjectName("자재"),
                                                  ObjectDic.Instance.GetObjectName("자재규격"),
                                                  ObjectDic.Instance.GetObjectName("요청중량"),
                                                  ObjectDic.Instance.GetObjectName("REQ_BAG_QTY"),
                                                  ObjectDic.Instance.GetObjectName("호퍼")
                                               };

                    dtColumnLang.Rows.Add(drCrad);

                    DataRowView drSelect = dgInputRequest.Rows[rowIndex].DataItem as DataRowView;
                    
                    DataRow drReqNew = ReqPrintTag.NewRow();
                    drReqNew["REQ_DTTM"] = Util.NVC(drSelect["REQ_DATE"]);
                    drReqNew["REQ_ID"] = Util.NVC(drSelect["REQ_ID"]);
                    drReqNew["REQ_ID1"] = Util.NVC(drSelect["REQ_ID"]);

                    drReqNew["QR_REQ_ID"] = Util.NVC(drSelect["REQ_ID"]);

                    drReqNew["USERNAME"] = Util.NVC(drSelect["REQ_USER_NAME"]);
                    drReqNew["NOTE"] = Util.NVC(drSelect["NOTE"]);
                    drReqNew["EPQTID"] = Util.NVC(drSelect["EQPTNAME"]);
                    drReqNew["PRJT_NAME"] = Util.NVC(drSelect["PRJT_NAME"]);
                    ReqPrintTag.Rows.Add(drReqNew);
                    
                    DataRow drReqDetlNew = ReqPrintTagDetl.NewRow();
                    drReqDetlNew["MTRLID_0"] = Util.NVC(drSelect["MTRLID"]);
                    drReqDetlNew["MTRLDESC_0"] = Util.NVC(drSelect["MTRLDESC"]);
                    drReqDetlNew["QTY_0"] = System.Math.Round(Convert.ToDecimal(drSelect["REQ_QTY"]), 3);
                    drReqDetlNew["BAG_QTY_0"] = Convert.ToInt32(Util.NVC_Int(drSelect["MTRL_BAG_QTY"]));
                    drReqDetlNew["HOPPER_0"] = Util.NVC(drSelect["HOPPER_NAME"]) + "[" + Util.NVC(drSelect["HOPPER_ID"]) + "]";
                    ReqPrintTagDetl.Rows.Add(drReqDetlNew);
                }

                Report_Multi rs = new LGC.GMES.MES.ELEC001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[4];
                    Parameters[0] = "MReqList_Print";
                    Parameters[1] = ReqPrintTag;
                    Parameters[2] = ReqPrintTagDetl;
                    Parameters[3] = dtColumnLang;

                    C1WindowExtension.SetParameters(rs, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }

        private void UcHopperList_HopperDoubleClick(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId)
        {
            HopperDoubleClick?.Invoke(sender, equipmentSegment, process, equipment, materialId, hopperId);        
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> indexes = dgInputRequest.GetCheckedRowIndex("CHK");
                if (indexes != null && indexes.Count.Equals(0))
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }
                

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                foreach (int idx in indexes)
                {
                    DataRow Indata = IndataTable.NewRow();
                    Indata["REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[idx].DataItem, "REQ_ID")); ;
                    Indata["USERID"] = LoginInfo.USERID;
                    IndataTable.Rows.Add(Indata);
                }

                //삭제처리 하시겠습니까?
                Util.MessageConfirm("SFU1259", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_RMTRL_INPUT_REQ_PROC", "INDATA", null, IndataTable);

                            SearchRequestList();
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> indexes = dgInputRequest.GetCheckedRowIndex("CHK");
                if (indexes != null && indexes.Count.Equals(0))
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                // 2023.04.20  강성묵: 원자재 출고 요청시 진행중인 자재 존재 여부 체크
                if (bChkRmtrlShip == true)
                {
                    // 원자재 요청 실행시 false로 변경해줘야 다음 Event에서 CHK_RMTRL_SHIP 체크 가능함.
                    bChkRmtrlShip = false;
                }
                else
                {
                    if (IsRemoveAreaHardCode("CHK_RMTRL_SHIP") == true)
                    {
                        try
                        {
                            DataTable dtRQSTDT = new DataTable();
                            dtRQSTDT.TableName = "RQSTDT";
                            dtRQSTDT.Columns.Add("LANGID", typeof(string));
                            dtRQSTDT.Columns.Add("EQPTID", typeof(string));
                            dtRQSTDT.Columns.Add("MTRLID", typeof(string));

                            foreach (int iIdx in indexes)
                            {
                                string sChkEqptId = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[iIdx].DataItem, "EQPTID"));
                                string sChkMtrlId = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[iIdx].DataItem, "MTRLID"));

                                // 동일 장비 & 자제 제외
                                string sSelectFilter = "";
                                sSelectFilter += "     EQPTID=" + "'" + sChkEqptId + "'";
                                sSelectFilter += " AND MTRLID=" + "'" + sChkMtrlId + "'";

                                if (dtRQSTDT.Rows.Count > 0)
                                {
                                    if (dtRQSTDT.Select(sSelectFilter).Count() > 0)
                                    {
                                        continue;
                                    }
                                }

                                DataRow drRqstDt = dtRQSTDT.NewRow();
                                drRqstDt["LANGID"] = LoginInfo.LANGID;
                                drRqstDt["EQPTID"] = sChkEqptId;
                                drRqstDt["MTRLID"] = sChkMtrlId;

                                dtRQSTDT.Rows.Add(drRqstDt);
                            }

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RMTRL_REQ_STAT", "RQSTDT", "RSLTDT", dtRQSTDT);

                            if (CommonVerify.HasTableRow(dtResult))
                            {
                                ELEC003_RMTRL_SHIP_CHK popupRmtrlShipChk = new ELEC003_RMTRL_SHIP_CHK();

                                if (popupRmtrlShipChk != null)
                                {
                                    popupRmtrlShipChk.FrameOperation = FrameOperation;

                                    object[] Parameters = new object[1];
                                    Parameters[0] = dtResult;

                                    C1WindowExtension.SetParameters(popupRmtrlShipChk, Parameters);

                                    popupRmtrlShipChk.Closed += new EventHandler(popupRmtrlShipChk_Closed);

                                    this.Dispatcher.BeginInvoke(new Action(() => popupRmtrlShipChk.Show()));

                                    // popupRmtrlShipChk_Closed 에서 팝업 화면 결과에 따라 btnRequest_Click 함수를 재호출 함.
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));

                foreach (int idx in indexes)
                {
                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[idx].DataItem, "REQ_ID"));
                    IndataTable.Rows.Add(Indata);
                }



                //출고요청 하시겠습니까?
                Util.MessageConfirm("SFU2086", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService("BR_MTR_REG_MATERIAL_REL_REQUEST_INPUT", "INDATA", "OUTDATA", IndataTable, (result, ex) =>
                        {
                            HiddenLoadingIndicator();

                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("Y"))
                            {
                                // 요청되었습니다.
                                Util.MessageInfo("SFU1747");

                                // 재조회
                                SearchRequestList();
                            }
                            else if (result.Rows.Count != 0 && result.Rows[0]["PROC_FLAG"].ToString().Equals("N"))
                            {
                                Util.MessageValidation(result.Rows[0]["ERR_MSG"].ToString());
                            }
                            else
                            {
                                // 요청 실패하였습니다.
                                Util.MessageValidation("FM_ME_0185");
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }
        private void popupRmtrlShipChk_Closed(object sender, EventArgs e)
        {
            ELEC003_RMTRL_SHIP_CHK popup = sender as ELEC003_RMTRL_SHIP_CHK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 원자재 요청 실행시 true로 변경해줘야 CHK_RMTRL_SHIP 체크하지 않고 원자재 요청 가능함..
                bChkRmtrlShip = true;
                btnRequest_Click(null, null);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            List<Button> listAuth = new List<Button>
            {

            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                dr["ELTR_TYPE_CODE"] = Util.IsNVC(cboElecType.SelectedValue) ? null : Util.NVC(cboElecType.SelectedValue);     

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_WITH_CODE_DRB", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PlaySound()
        {
            player = new System.Media.SoundPlayer(LGC.GMES.MES.ELEC003.Properties.Resources.InputBois);
            this.player.Play();
        }

        private string GetRequestCount()
        {
            try
            {
                string _ValueToFind = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_POPUP", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    _ValueToFind = dtMain.Rows[0]["REQ_ID"].ToString();
                }
                return _ValueToFind;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void CheckRequest()
        {
            if (!string.IsNullOrEmpty(GetRequestCount()))
            {
                tmInputRequest.IsEnabled = false;

                this.PlaySound();


                ELEC001_002_DETAIL_CWA _ReqDetail = new ELEC001_002_DETAIL_CWA();
                _ReqDetail.FrameOperation = FrameOperation;

                if (_ReqDetail != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = GetRequestCount();
                    //Parameters[1] = _EQPTID;
                    C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                    _ReqDetail.Closed += new EventHandler(Detail_Closed);
                    _ReqDetail.ShowModal();
                    _ReqDetail.CenterOnScreen();

                    this.PlaySound();
                }
            }
        }

        public void Refresh(string line, string process, string equipment)
        {
            gdHopperList.Children.Clear();
            gdHopperList.RowDefinitions.Clear();

            Util.gridClear(dgInputRequest);

            if (Util.IsNVC(cboEquipmentSegment.SelectedValue) || Util.NVC(cboEquipmentSegment.SelectedValue).Equals("SELECT"))
            {
                //라인을 선택하세요.
                Util.MessageValidation("SFU1223");  
                return;
            }

            if (Util.IsNVC(cboProcess.SelectedValue) || Util.NVC(cboProcess.SelectedValue).Equals("SELECT"))
            {
                //공정을 선택하세요.
                Util.MessageValidation("SFU1459");  
                return;
            }

            cboEquipmentSegment.SelectedValue = line;
            cboProcess.SelectedValue = process;

            if (cboEquipment.ItemsSource != null)
            {
                cboEquipment.UncheckAll();
                string[] eqpList = equipment.Split(',');
                foreach (string item in eqpList)
                {
                    cboEquipment.Check(item);
                }
            }

            SearchHopperInEqpt(line, process, Util.NVC(cboElecType.SelectedValue), equipment);
        }

        private void SearchHopperInEqpt(string line, string process, string elecType, string equipment)
        {
            try
            {
                ShowLoadingIndicator();

                GC.Collect();

                Util.gridClear(dgInputRequest);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("EQPT_LIST", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = line;
                Indata["PROCID"] = process;
                Indata["ELTR_TYPE_CODE"] = Util.IsNVC(cboElecType.SelectedValue) ? null : Util.NVC(cboElecType.SelectedValue);
                if (equipment.IndexOf(",") > -1)
                {
                    string[] equipList = equipment.Split(',');
                    Indata["EQPT_LIST"] = string.Join(",", equipList);
                }
                else
                {
                    Indata["EQPTID"] = Util.IsNVC(equipment) ? null : Util.NVC(equipment);
                }
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_HOPPER_MONITER_DRB", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();

                        Util.MessageException(ex);
                        return;
                    }
             
                    gdHopperList.Children.Clear();
                    gdHopperList.RowDefinitions.Clear();
                    
                    var eqptList = result.AsEnumerable()
                    .GroupBy(g => new {
                        EQPTID = g.Field<string>("EQPTID"),
                        EQPTNAME = g.Field<string>("EQPTNAME"),
                        EIOIFMODE = g.Field<string>("EIOIFMODE")
                    })
                    .Select(f => new {
                        EqptId = f.Key.EQPTID,
                        EqptName = f.Key.EQPTNAME,
                        EqptOnOff = f.Key.EIOIFMODE
                    })
                    .OrderBy(o => o.EqptId).ToList();

                    int rowIndex = 0;
                    
                    foreach (var eqptItem in eqptList)
                    {
                        DataRow eqpt = result.AsEnumerable()
                        .Where(g => Util.NVC(g.Field<string>("EQPTID")).Equals(eqptItem.EqptId) &&
                                    !Util.NVC(g.Field<string>("WOID")).Equals(string.Empty))
                        .FirstOrDefault();


                        RowDefinition newRow = new RowDefinition { Height = new GridLength(80) };
                        gdHopperList.RowDefinitions.Add(newRow);

                        UcHopperInEqptList ucHopperList = new UcHopperInEqptList();
                        ucHopperList.Width = ucHopperList.Height = double.NaN;

                        ucHopperList.EqptId = eqptItem.EqptId;
                        ucHopperList.EqptName = eqptItem.EqptName;
                        ucHopperList.EqptOnOff = eqptItem.EqptOnOff.Equals("ON");
                        ucHopperList.UseAlarm = true;
                        ucHopperList.UseGradient = true;

                        if (eqpt != null)
                        {
                            ucHopperList.ProjectName = Util.NVC(eqpt["PRJT_NAME"]);
                            ucHopperList.Version = Util.NVC(eqpt["PROD_VER_CODE"]);
                            ucHopperList.WorkOrderId = Util.NVC(eqpt["WOID"]);
                            ucHopperList.ProductId = Util.NVC(eqpt["PRODID"]);
                        }

                        Grid.SetRow(ucHopperList, rowIndex);
                        Grid.SetColumn(ucHopperList, 0);

                        DataRow[] drhopper = result.AsEnumerable()
                        .Where(f => Util.NVC(f.Field<string>("EQPTID")).Equals(eqptItem.EqptId)).ToArray();

                        ucHopperList.SetHopperData(drhopper);
                        ucHopperList.HopperDoubleClick += UcHopperList_HopperDoubleClick;
                        gdHopperList.Children.Add(ucHopperList);

                        rowIndex++;
                        
                        
                    }

                    eqptList.Clear();
                    result.Dispose();

                    this.InvalidateVisual();
                    
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return;
            }
        }
        
        private void SearchRequestList()
        {
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(dgInputRequest);

                string sBizrule = string.Empty;

                if (uses_PNTR_WRKR_TYPE)
                {
                    if (LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_DRB_DLGT";
                    }
                    else if (LoginInfo.PNTR_WRKR_TYPE.Equals("WRKR"))
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_DRB_WRKR";
                    }
                    else
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_DRB_SUBCONT";
                    }
                }
                else
                {
                    sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_DRB";
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("REQ_DATE", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPT_LIST", typeof(string));
                IndataTable.Columns.Add("CMPL_FLAG", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow drNew = IndataTable.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNew["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString();
                drNew["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                drNew["REQ_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                drNew["ELTR_TYPE_CODE"] = Util.IsNVC(cboElecType.SelectedValue) ? null : Util.NVC(cboElecType.SelectedValue);
                drNew["USERID"] = LoginInfo.USERID;

                if (cboEquipment.SelectedItemsToString.IndexOf(",") > -1)
                {
                    string[] equipList = cboEquipment.SelectedItemsToString.Split(',');
                    drNew["EQPT_LIST"] = string.Join(",", equipList);
                }
                else
                {
                    drNew["EQPTID"] = Util.IsNVC(cboEquipment.SelectedItemsToString) ? null : Util.NVC(cboEquipment.SelectedItemsToString);
                }
                drNew["CMPL_FLAG"] = chkConfirm.IsChecked.Equals(true) ? "Y" : "N";
                IndataTable.Rows.Add(drNew);

                new ClientProxy().ExecuteService(sBizrule, "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {                     
                        Util.MessageException(ex);
                        return;
                    }

                    Util.GridSetData(dgInputRequest, result, FrameOperation, true);
                });   
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                Util.MessageException(ex);
                return;
            }
        }

        private void TimerSetting()
        {
            if (Util.IsNVC(cboAutoSearch.SelectedValue))
            {
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer = null;
                }
                return;
            }

            if (refreshTimer == null)
            {
                refreshTimer = new System.Windows.Threading.DispatcherTimer();

                int interval = Convert.ToInt32(cboAutoSearch.SelectedValue);

                refreshTimer.Interval = TimeSpan.FromSeconds(interval);
                refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            }
            
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Refresh(Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboProcess.SelectedValue), Util.NVC(cboEquipment.SelectedItemsToString));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 동별 공통코드 체크 : 동별 기능 분기 적용 (2023.04.20  강성묵)
        /// </summary>
        private bool IsRemoveAreaHardCode(string sComCode)
        {
            const string sBizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable dtInTable = new DataTable("INDATA");
                dtInTable.Columns.Add("LANGID", typeof(string));
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("COM_CODE", typeof(string));
                dtInTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = dtInTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EXEC_YN_FOR_REMOVE_AREA_HARD_CODE";
                dr["COM_CODE"] = sComCode;
                dr["USE_FLAG"] = "Y";
                dtInTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "INDATA", "OUTDATA", dtInTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU2878", (sresult) => //승인하시겠습니까?
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {

                        List<int> indexes = dgInputRequest.GetCheckedRowIndex("CHK");
                        if (indexes != null && indexes.Count.Equals(0))
                        {
                            Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                            return;
                        }


                        DataTable dtRQSTDT = new DataTable();
                        dtRQSTDT.TableName = "RQSTDT";
                        dtRQSTDT.Columns.Add("REQ_NO", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_USERID", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_RSLT_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_NOTE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("LANGID", typeof(string));

                      

                        foreach (int idx in indexes)
                        {
                            DataRow drnewrow = dtRQSTDT.NewRow();
                            drnewrow["REQ_NO"] = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[idx].DataItem, "REQ_ID")); ;
                            drnewrow["APPR_USERID"] = LoginInfo.USERID;
                            drnewrow["APPR_RSLT_CODE"] = "APP";
                            drnewrow["APPR_NOTE"] = String.Empty;
                            drnewrow["APPR_BIZ_CODE"] = "RMTRL_INPUT";
                            drnewrow["LANGID"] = LoginInfo.LANGID;
                            dtRQSTDT.Rows.Add(drnewrow);
                        }
                        

                        new ClientProxy().ExecuteService("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.AlertByBiz("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", Exception.Message, Exception.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            SearchRequestList();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU2866", (sresult) => //반려하시겠습니까?
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {

                        List<int> indexes = dgInputRequest.GetCheckedRowIndex("CHK");
                        if (indexes != null && indexes.Count.Equals(0))
                        {
                            Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                            return;
                        }

                        DataTable dtRQSTDT = new DataTable();
                        dtRQSTDT.TableName = "RQSTDT";
                        dtRQSTDT.Columns.Add("REQ_NO", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_USERID", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_RSLT_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_NOTE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("LANGID", typeof(string));

                     
                        foreach (int idx in indexes)
                        {
                            DataRow drnewrow = dtRQSTDT.NewRow();
                            drnewrow["REQ_NO"] = Util.NVC(DataTableConverter.GetValue(dgInputRequest.Rows[idx].DataItem, "REQ_ID")); ;
                            drnewrow["APPR_USERID"] = LoginInfo.USERID;
                            drnewrow["APPR_RSLT_CODE"] = "REJ";
                            drnewrow["APPR_NOTE"] = String.Empty;
                            drnewrow["APPR_BIZ_CODE"] = "RMTRL_INPUT";
                            drnewrow["LANGID"] = LoginInfo.LANGID;
                            dtRQSTDT.Rows.Add(drnewrow);
                        }

                        new ClientProxy().ExecuteService("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                Util.AlertByBiz("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", Exception.Message, Exception.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            SearchRequestList();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        SearchRequestList();
                    }
                }
            });
        }
        #endregion
    }
}
