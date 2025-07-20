/*************************************************************************************
 Created Date : 2020.10.16
      Creator : 신광희
   Decription : 조립 공정진척(CNB 2동) - 상단 버튼 영역 UserControl
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.10.21  김지은 : 무지부 방향설정 기능 추가
 2021.10.27  조영대 : 권취 방향 변경 팝업 추가
 2022.11.16  오화백 : 물류반송조건설정 버튼 추가
 2023.02.24  김용군 : ESHM 증설 - AZS_ECUTTER, AZS_STACKING 대응
 2024.01.08  남재현 : STK 특별 Tray 관리 동 추가.(PKG 특별 생산 설정)
 2025.05.20  천진수 : ESHG 증설 조립공정진척 DNC공정추가 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY005.Controls
{
    /// <summary>
    /// UcAssemblyCommand.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyCommand : UserControl
    {
        #region Properties

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string ProcessCode { get; set; }

        private bool isManageSlittingSide = false;
        public bool IsManageSlittingSide
        {
            get { return isManageSlittingSide; }
            set { isManageSlittingSide = value; }
        }

        public enum ButtonVisibilityType
        {
            NotchingProductLot,
            NotchingProductionResult,
            CommonProductLot,
            CommonProductionResult
        }

        #endregion Properties

        #region Constructor
        public UcAssemblyCommand()
        {
            InitializeComponent();
        }
        #endregion Constructor

        #region Events
        private void btnExtra_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }
        #endregion Events

        #region Methods

        public void SetButtonVisibility(ButtonVisibilityType displayType)
        {
            // ButtonArea 그리드 이하 버튼 숨김 처리
            foreach (Button button in Util.FindVisualChildren<Button>(ButtonArea))
            {
                button.Visibility = Visibility.Collapsed;
            }

            if (displayType == ButtonVisibilityType.NotchingProductLot)
            {
                btnExtra.Visibility = Visibility.Visible;
                btnQualityInput.Visibility = Visibility.Visible;
                btnEqptIssue.Visibility = Visibility.Visible;
                btnPrint.Visibility = Visibility.Visible;
            }
            else if (displayType == ButtonVisibilityType.NotchingProductionResult)
            {
                btnExtra.Visibility = Visibility.Visible;
                btnProductLot.Visibility = Visibility.Visible;
                //btnRunComplete.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;
                btnPrint.Visibility = Visibility.Visible;
                btnQualityInput.Visibility = Visibility.Visible;
            }
            else if (displayType == ButtonVisibilityType.CommonProductLot)
            {
                btnExtra.Visibility = Visibility.Visible;
                btnQualityInput.Visibility = Visibility.Visible;
                btnEqptIssue.Visibility = Visibility.Visible;
                btnEqptCondMain.Visibility = Visibility.Visible;
            }
            else if (displayType == ButtonVisibilityType.CommonProductionResult)
            {
                btnExtra.Visibility = Visibility.Visible;
                btnProductLot.Visibility = Visibility.Visible;
                //btnRunComplete.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;
                btnQualityInput.Visibility = Visibility.Visible;
            }

            //C1DropDownButton 하위의 버튼 Collapsed 처리
            foreach (Button button in Util.FindVisualChildren<Button>(grdDropdown))
            {
                if(button != null) button.Visibility = Visibility.Collapsed;
            }

            // TODO 추가기능 버튼 공정별 정리 필요.
            if (btnExtra.Visibility == Visibility.Visible)
            {
                btnReturnCondition.Visibility = Visibility.Collapsed;                // 물류반송조건설정

                //if (IsUpdateLaneNoVisibility(ProcessCode) == true)
                //{
                //    btnUpdateLaneNo.Visibility = Visibility.Visible;                   // 슬리팅 레인 번호 변경
                //}

                // 무지부 방향 설정
                isManageSlittingSide = IsAreaCommonCodeUse("MNG_SLITTING_SIDE_AREA", ProcessCode);
                if (isManageSlittingSide)
                {
                    btnWorkHalfSlitSide.Visibility = Visibility.Visible;

                    // 권취 방향 변경
                    if (IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
                    {
                        btnEmSectionRollDirctn.Visibility = Visibility.Visible;
                    }
                }

                if (ProcessCode == Process.NOTCHING)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnRemarkHistory.Visibility = Visibility.Visible;
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnEqptCond.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;

                    btnRunCancel.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;
                    btnRunCompleteCancel.Visibility = Visibility.Visible;

                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                
                    }

                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.NotchingProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible; // 2021.08.12 : 시생산샘플설정/해제 기능 추가

                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;   // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                    }
                }
                else if (ProcessCode == Process.VD_LMN)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnRework.Visibility = Visibility.Visible;
                   
                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else if (ProcessCode == Process.DNC || ProcessCode == Process.LAMINATION)   // 20250428 ESHG DNC공정신설
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    //btnEqptCond.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.CommonProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible; // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                        btnSpclProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;   // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                        btnSpclProdMode.Visibility = Visibility.Collapsed;
                    }
                }
                else if (ProcessCode == Process.STACKING_FOLDING)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    //btnEqptCond.Visibility = Visibility.Visible;
                    btnMerge.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnChgCarrier.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;
                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.CommonProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible; // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;   // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                    }


                }
                else if (ProcessCode == Process.PACKAGING)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnMerge.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnTrayInfo.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;

                    // 2024.01.08 : STK 특별 Tray 관리 동 추가.(PKG 특별 생산 설정)
                    if (IsCommonCodeUse("STK_BOX_SPCL_MNG_AREA", LoginInfo.CFG_AREA_ID))
                    {
                        btnSpclProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnSpclProdMode.Visibility = Visibility.Collapsed;
                    }

                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                 
                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.CommonProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible; // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;   // 2021.08.12 : 시생산샘플설정/해제 기능 추가
                    }

                }

                // 김용군 AZS_ECUTTER, AZS_STACKING 대응
                else if (ProcessCode == Process.AZS_ECUTTER)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnEqptCondSearch.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.CommonProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible;
                        btnSpclProdMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                        btnSpclProdMode.Visibility = Visibility.Collapsed;
                    }
                }
                else if (ProcessCode == Process.AZS_STACKING)
                {
                    btnWaitLot.Visibility = Visibility.Visible;
                    btnMerge.Visibility = Visibility.Visible;
                    btnWipNote.Visibility = Visibility.Visible;
                    btnChgCarrier.Visibility = Visibility.Visible;
                    btnCancelConfirm.Visibility = Visibility.Visible;
                    btnRunComplete.Visibility = Visibility.Visible;
                    // 물류반송조건설정 버튼 사용여부
                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                    // 생산실적 화면에서 조회 후 시생산 설정 및 해제 시 문제가 발생할 여지가 있어 시생산설정/해제는 Product Lot 영역에만 보이게 처리 함.
                    if (displayType == ButtonVisibilityType.CommonProductLot)
                    {
                        btnRunStart.Visibility = Visibility.Visible;
                        btnPilotProdMode.Visibility = Visibility.Visible;
                        btnPilotProdSPMode.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        btnRunStart.Visibility = Visibility.Collapsed;
                        btnPilotProdMode.Visibility = Visibility.Collapsed;
                        btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                    }
                }

                if (displayType == ButtonVisibilityType.NotchingProductLot || displayType == ButtonVisibilityType.CommonProductLot)
                {
                    btnRunComplete.Visibility = Visibility.Collapsed;
                }
            }
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


        /// <summary>
        /// 물류반송 조건설정 버튼
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        private bool IsReturnConditionVisibility(string ProcessID)
        {

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "LOGIS_COND_BY_PROC";
                dr["COM_CODE"] = ProcessID;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

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
                Util.MessageException(ex);
                return false;
            }
        }

        ///// <summary>
        ///// 슬리팅 레인 번호 변경 버튼
        ///// </summary>
        //private bool IsUpdateLaneNoVisibility(string sProcessID)
        //{
        //    const string sBizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
        //    try
        //    {
        //        DataTable dtInTable = new DataTable("INDATA");
        //        dtInTable.Columns.Add("LANGID", typeof(string));
        //        dtInTable.Columns.Add("AREAID", typeof(string));
        //        dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
        //        dtInTable.Columns.Add("COM_CODE", typeof(string));
        //        dtInTable.Columns.Add("USE_FLAG", typeof(string));
        //        DataRow dr = dtInTable.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        dr["COM_TYPE_CODE"] = "LANE_NO_CHG_BY_PROC";
        //        dr["COM_CODE"] = sProcessID;
        //        dr["USE_FLAG"] = "Y";
        //        dtInTable.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "INDATA", "OUTDATA", dtInTable);

        //        if (CommonVerify.HasTableRow(dtResult))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return false;
        //    }
        //}

        #endregion Methods

    }
}