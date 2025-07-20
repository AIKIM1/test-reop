/*************************************************************************************
 Created Date : 2021.12.06
      Creator : 신광희
   Decription : 소형 조립 공정진척(NFF) - 상단 버튼 영역 UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.06  신광희 : Initial Created.
 2023.10.25  김용군 : 오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
 2023.12.05  배현우 : 무지부 권취 방향 설정 버튼 추가
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY006.Controls
{
    /// <summary>
    /// UcAssemblyCommand.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyCommand : UserControl
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation { get; set; }

        public string ProcessCode { get; set; }

        public enum ButtonVisibilityType
        {
            ProductLot,
            ProductionResult
        }

        private ButtonVisibilityType _buttonVisibilityType;
        private bool isManageSlittingSide = false;

        public UcAssemblyCommand()
        {
            InitializeComponent();
        }
        #endregion Declaration & Constructor 

        #region Events
        private void btnExtra_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }    
        #endregion Events

        #region Methods

        public void SetButtonVisibility(ButtonVisibilityType displayType)
        {
            _buttonVisibilityType = displayType;
            //GetButtonPermissionGroup();

            // ButtonArea 그리드 이하 버튼 숨김 처리
            foreach (Button button in Util.FindVisualChildren<Button>(ButtonArea))
            {
                button.Visibility = Visibility.Collapsed;
            }

            btnExtra.Visibility = Visibility.Visible;
            btnQualityInput.Visibility = Visibility.Visible;

            if (displayType == ButtonVisibilityType.ProductLot)
            {
                btnEqptCondMain.Visibility = Visibility.Visible;
                btnEqptIssue.Visibility = Visibility.Visible;
                btnHistoryCard.Visibility = Visibility.Visible;
            }
            else if (displayType == ButtonVisibilityType.ProductionResult)
            {
                btnProductLot.Visibility = Visibility.Visible;
                btnEqptIssue.Visibility = Visibility.Collapsed;
                btnConfirm.Visibility = Visibility.Visible;
                btnHistoryCard.Visibility = Visibility.Visible;
            }

            //C1DropDownButton 하위의 버튼 Collapsed 처리
            foreach (Button button in Util.FindVisualChildren<Button>(grdDropdown))
            {
                if(button != null) button.Visibility = Visibility.Collapsed;
            }

            // TODO 추가기능 버튼 공정별 정리 필요.
            if (btnExtra.Visibility == Visibility.Visible)
            {

                btnAutoRsltCnfm.Visibility = Visibility.Collapsed;
                if (string.Equals(ProcessCode, Process.WINDING))
                {
                    isManageSlittingSide = IsAreaCommonCodeUse("MNG_SLITTING_SIDE_AREA", ProcessCode);
                    if (isManageSlittingSide)
                    {
                        btnWorkHalfSlitSide.Visibility = Visibility.Visible;

                        
                    }

                    btnWaitPancake.Visibility = Visibility.Visible;
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnRunStart.Visibility = Visibility.Visible;
                    //btnCancelConfirm.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnEqptEndCancel.Visibility = Visibility.Visible;
                    if (LoginInfo.CFG_EQSG_ID.Equals("MCC01"))
                    {
                        btnAutoRsltCnfm.Visibility = Visibility.Visible;
                    }
                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnCancelTerm.Visibility = Visibility.Visible;
                        btnCancelTermSepa.Visibility = Visibility.Visible;
                        btnEditEqptQty.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnEqptEnd.Visibility = Visibility.Visible;
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnCancel.Visibility = Visibility.Collapsed;
                        btnEditEqptQty.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                    }
                }
                else if (string.Equals(ProcessCode, Process.ASSEMBLY))
                {
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnRunStart.Visibility = Visibility.Visible;
                    //btnCancelConfirm.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnEqptEndCancel.Visibility = Visibility.Visible;
                    btnTrayReconf.Visibility = Visibility.Collapsed; //   
                    btnHistoryCard.Visibility = Visibility.Collapsed;

                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnWindingLot.Visibility = Visibility.Visible;
                        btnCancelTerm.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnEqptEnd.Visibility = Visibility.Visible;
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnCancel.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnTrayReconf.Visibility = Visibility.Collapsed;
                    }
                }
                else if (string.Equals(ProcessCode, Process.ZZS))
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnQualitySearch.Visibility = Visibility.Visible;
                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                }
                else if (string.Equals(ProcessCode, Process.PACKAGING))
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnQualitySearch.Visibility = Visibility.Visible;
                    btnCProduct.Visibility = Visibility.Visible;
                    btnBoxIn.Visibility = Visibility.Visible;
                    btnCancelTerm.Visibility = Visibility.Visible;
                    btnMerge.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                }
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                else if (string.Equals(ProcessCode, Process.ZTZ))
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnRunStart.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnEqptEnd.Visibility = Visibility.Visible;
                    btnEqptEndCancel.Visibility = Visibility.Visible;
                    btnHistoryCard.Visibility = Visibility.Collapsed;
                    btnPrint.Visibility = Visibility.Visible;
                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnRunStart.Visibility = Visibility.Visible;
                    //btnCancelConfirm.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnEqptEndCancel.Visibility = Visibility.Visible;
                    btnHistoryCard.Visibility = Visibility.Collapsed;

                    if (displayType == ButtonVisibilityType.ProductLot)
                    {
                        btnBoxPrint.Visibility = Visibility.Visible;
                        btnTrayLotChange.Visibility = Visibility.Visible;
                        btnLastCellNo.Visibility = Visibility.Visible;
                        btnCellDetailInfo.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnCancel.Visibility = Visibility.Collapsed;
                        btnEqptEnd.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                    }
                }
            }
            GetButtonPermissionGroup();
        }

        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            foreach (Button button in Util.FindVisualChildren<Button>(grdDropdown))
            {
                if (button != null)
                {
                    listAuth.Add(button);
                }
            }
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetButtonPermissionGroup()
        {
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) return;

            InitializeButtonPermissionGroup();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["USERID"] = LoginInfo.USERID;
            dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            //dtRow["AREAID"] = "M9"; //TODO 테스트 이후 삭제 예정
            dtRow["PROCID"] = ProcessCode;
            dtRow["EQGRID"] = null;
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Columns.Contains("BTN_PMS_GRP_CODE"))
                {
                    foreach (DataRow dr in dtResult.Rows)
                    {
                        if (dr == null) continue;

                        switch (Util.NVC(dr["BTN_PMS_GRP_CODE"]))
                        {
                            case "LOTSTART_W": // 작업시작 사용 권한

                                if (_buttonVisibilityType == ButtonVisibilityType.ProductLot)
                                {
                                    btnRunStart.Visibility = Visibility.Visible;
                                }
                                break;

                            case "EQPT_END_W":  // 장비완료 사용 권한
                                if (_buttonVisibilityType == ButtonVisibilityType.ProductionResult)
                                {
                                    btnEqptEnd.Visibility = Visibility.Visible;
                                }
                                break;

                            case "CONFIRM_W":   // 실적확정
                                if (_buttonVisibilityType == ButtonVisibilityType.ProductionResult)
                                {
                                    btnConfirm.Visibility = Visibility.Visible;
                                }
                                break;

                            case "CANCEL_LOTSTART_W":   // 작업시작 취소
                                if (_buttonVisibilityType == ButtonVisibilityType.ProductLot)
                                {
                                    btnCancel.Visibility = Visibility.Visible;
                                }
                                break;

                            case "CANCEL_EQPT_END_W":   // 장비완료 취소
                                btnEqptEndCancel.Visibility = Visibility.Visible;
                                break;

                            case "CANCEL_TERM_W":       // 투입LOT 종료 취소 권한
                                if (!string.Equals(ProcessCode, Process.WASHING) && _buttonVisibilityType == ButtonVisibilityType.ProductLot)
                                {
                                    btnCancelTerm.Visibility = Visibility.Visible;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                //btnRunStart.Visibility = Visibility.Collapsed;
                //btnEqptEnd.Visibility = Visibility.Collapsed;
                //btnConfirm.Visibility = Visibility.Collapsed;
                //btnCancel.Visibility = Visibility.Collapsed;
                //btnEqptEndCancel.Visibility = Visibility.Collapsed;
                //btnCancelTerm.Visibility = Visibility.Collapsed;


                if (btnRunStart.Visibility == Visibility.Visible) btnRunStart.Visibility = Visibility.Collapsed;
                if (btnEqptEnd.Visibility == Visibility.Visible) btnEqptEnd.Visibility = Visibility.Collapsed;
                if (btnConfirm.Visibility == Visibility.Visible) btnConfirm.Visibility = Visibility.Collapsed;
                if (btnCancel.Visibility == Visibility.Visible) btnCancel.Visibility = Visibility.Collapsed;
                if (btnEqptEndCancel.Visibility == Visibility.Visible) btnEqptEndCancel.Visibility = Visibility.Collapsed;
                if (btnCancelTerm.Visibility == Visibility.Visible) btnCancelTerm.Visibility = Visibility.Collapsed;

                /*
                if(grdProductionResult.Visibility == Visibility.Visible)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;        //작업시작
                    UcAssemblyCommand.btnCancel.Visibility = Visibility.Collapsed;          //시작취소
                    UcAssemblyCommand.btnEqptEndCancel.Visibility = Visibility.Collapsed;   //장비완료 취소
                    //UcAssemblyCommand.btnConfirm.Visibility = Visibility.Collapsed;         //실적확정

                    if (!string.Equals(_processCode, Process.WASHING))
                        UcAssemblyCommand.btnCancelTerm.Visibility = Visibility.Collapsed;  //투입LOT종료취소
                }
                else
                {
                    UcAssemblyCommand.btnEqptEnd.Visibility = Visibility.Collapsed;         //장비완료
                    UcAssemblyCommand.btnEqptEndCancel.Visibility = Visibility.Collapsed;   //장비완료 취소
                }

                //TODO 공정별 구분이 필요 할수 있음
                UcAssemblyEquipment.IsInputUseAuthority = false;
                UcAssemblyEquipment.IsWaitUseAuthority = false;
                UcAssemblyEquipment.IsInputHistoryAuthority = false;
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private bool IsCommonCodeUse(string sCmcdType, string sAreaid)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCmcdType;
                dr["CMCODE"] = sAreaid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion Methods

    }
}