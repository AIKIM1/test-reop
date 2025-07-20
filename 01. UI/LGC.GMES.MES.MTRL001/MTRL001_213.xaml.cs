/*************************************************************************************
 Created Date : 2024.09.11
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.11  오화백 : Initial Created.
  2025.04.21  조범모 : iWMS / EWM 구분에 따른 Biz 변경 (DA_INV_SEL_WAREHOUSE_MTRL -> BR_INV_SEL_WAREHOUSE_MTRL
  2025.04.28  오화백 : Foil 자재 HOLD 기능 추가
 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;



namespace LGC.GMES.MES.MTRL001
{
    /// <summary>
    /// COM001_134.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MTRL001_213 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 


        private Util _util = new Util();

        private bool isProductionStandbyWarehouseT1 = false; // 생산대기창고여부
        private bool isProductionStandbyWarehouseT2 = false; // 생산대기창고여부

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        /// <summary>
        /// 생성자
        /// </summary>
        public MTRL001_213()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 화면로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearchT1);
                listAuth.Add(btnSearchT2);
                listAuth.Add(btnSearchT3);
                listAuth.Add(btnSaveT1);
                listAuth.Add(btnSaveT2);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                InitCombo();
       

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 콤보박스 정보 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
           
        }

        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {
            //출고창고
            SetOutputWHCombo();

            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerTypeT1);
            SetStockerTypeCombo(cboStockerTypeT2);
            SetStockerTypeCombo(cboStockerTypeT3);
            SetStockerTypeCombo(cboStockerTypeT4);

            // Stocker 콤보박스
            SetStockerCombo(cboStockerT1);
            SetStockerCombo(cboStockerT2);
            SetStockerCombo(cboStockerT3);
            SetStockerCombo(cboStockerT4);
            // 자재 그룹 정보
            SetMtgrCombo(cboMtrlGroupT1);
            SetMtgrCombo(cboMtrlGroupT2);
            SetMtgrCombo(cboMtrlGroupT3);
            SetMtgrCombo(cboMtrlGroupT4);
            //사유코드
            SetHoldCodeCombo(cboReasonCodeT1);
            SetHoldCodeCombo(cboReasonCodeT2);
            SetHoldCodeCombo(cboReasonCodeT3);
            SetHoldCodeCombo(cboReasonCodeT4);
            //HOLDTYPE
            SetHoldTypeCombo(cboHoldType);

            //HOLD/RELESE
            SetHoldReleseCombo(cboHoldRelese);

            SearchMtrl_Group();

        }


        #endregion

        #region Event

        #region STK내 HOLD대상 리스트 버튼 이벤트 : btnSearchT1_Click()
        /// <summary>
        /// HOLD 대상 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            if (cboOutputWHT1.GetBindValue() == null && cboStockerTypeT1.SelectedValue.GetString() == "MWW")
            {
                // 출고창고를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고창고"));                
                return;
            }
            Util.gridClear(dgListT1);
            GetTargetHoldList();
        }
        #endregion

        #region STK내 Relese 대상 리스트 버튼 이벤트 : btnSearchT2_Click()

        /// <summary>
        ///  Relese 대상 HOLD 자재조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchT2_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListT2);
            if (cboOutputWHT2.GetBindValue() == null && cboStockerTypeT2.SelectedValue.GetString() == "MWW")
            {
                // 출고창고를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("출고창고"));
                return;
            }
            if(cboStockerTypeT2.SelectedValue.GetString() == "MWW")
            {
                GetTargetReleseList();
            }
            else
            {
                GetTargetReleseListFoil();
            }
           
        }

        #endregion

        #region STK내 사유변경 대상 리스트 버튼 이벤트 : btnSearchT3_Click()

        /// <summary>
        /// HOLD 사유 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchT3_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListT3);
            if(cboStockerTypeT3.SelectedValue.GetString() == "MWW")
            {
                GetTargetChangeNoteList();
             
            }
            else
            {
                GetTargetChangeNoteListFoil();
               
            }

           
        }


        #endregion

        #region STK내 HOLD 및 Relese 이력 리스트 버튼 이벤트 : btnSearchT4_Click()
        /// <summary>
        /// STK내 HOLD 및 Relese 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchT4_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListT4);
            if(cboStockerTypeT4.SelectedValue.GetString() == "MWW")
            {
                GettHoldHistoryList();
              
            }
            else
            {
                GettHoldHistoryListFoil();
             
            }
           
        }
        #endregion

        #region 창고유형 콤보 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고 유형  콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            if (TabHold.IsSelected == true)
            {
                SetStockerCombo(cboStockerT1);

                if (cboStockerTypeT1.SelectedValue.GetString() == "FWW")
                {
                    OutPutWH.IsEnabled = false;
                    OutPutWHT2.IsEnabled = false;
                    dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                   

                }
                else
                {
                    OutPutWH.IsEnabled = true;
                    OutPutWHT2.IsEnabled = true;
                    dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                   
                }

            }
            else if (TabRelese.IsSelected == true)
            {
                SetStockerCombo(cboStockerT2);

                if (cboStockerTypeT2.SelectedValue.GetString() == "FWW")
                {
                    OutPutWH.IsEnabled = false;
                    OutPutWHT2.IsEnabled = false;
                    dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                  
                }
                else
                {
                    OutPutWH.IsEnabled = true;
                    OutPutWHT2.IsEnabled = true;
                    dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                   
                }
            }
            else if (TabNoteChange.IsSelected == true)
            {
                SetStockerCombo(cboStockerT3);
                if (cboStockerTypeT2.SelectedValue.GetString() == "FWW")
                {
                    OutPutWH.IsEnabled = false;
                    OutPutWHT2.IsEnabled = false;
                    dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;                   
                }
                else
                {
                    OutPutWH.IsEnabled = true;
                    OutPutWHT2.IsEnabled = true;                 
                    dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;                 
                }
            }
            else
            {
                SetStockerCombo(cboStockerT4);
                if (cboStockerTypeT2.SelectedValue.GetString() == "FWW")
                {
                    OutPutWH.IsEnabled = false;
                    OutPutWHT2.IsEnabled = false;                 
                    dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    OutPutWH.IsEnabled = true;
                    OutPutWHT2.IsEnabled = true;                 
                    dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                }
            }

        }
        #endregion

        #region Hold 처리 버튼 이벤트 : btnSaveT1_Click()
        /// <summary>
        /// HOLD 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveT1_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationHold()) return;
            //HOLD 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU1345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if(cboStockerTypeT1.SelectedValue.GetString() == "MWW")
                            {
                                ProcessHold();
                            }
                            else
                            {
                                ProcessHoldFoil();
                            }
                           
                        }
                    });
        }
        #endregion

        #region Release 버튼 이벤트 : btnSaveT2_Click()
        /// <summary>
        /// HOLD 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveT2_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRelease()) return;
            //HOLD 해제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU4046"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if(cboStockerTypeT2.SelectedValue.GetString() == "MWW")
                            {
                                ProcessRelease();
                            }
                            else
                            {
                                ProcessReleaseFoil();
                            }
                           
                        }
                    });
        }
        #endregion

        #region 사유변경 버튼 이벤트 : btnSaveT3_Click()

        /// <summary>
        /// 사유변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveT3_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeNote()) return;
            //사유변경하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    "SFU3804"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if(cboStockerTypeT3.SelectedValue.GetString() == "MWW")
                            {
                                ProcessChangeNote();
                            }
                            else
                            {
                                ProcessChangeNoteFoil();
                            }

                            
                        }
                    });
        }

        #endregion

        private void cboOutputWHT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboOutputWHT1.SelectedValue.Equals(GetStandbyWarehouseCode()))
            {
                cboStockerTypeT1.IsEnabled = false;
                cboStockerT1.IsEnabled = false;
            }
            else
            {
                cboStockerTypeT1.IsEnabled = true;
                cboStockerT1.IsEnabled = true;
            }

            popSearchMtrl.SelectedValue = null;
            popSearchMtrl.SelectedText = null;
            popSearchMtrl.ItemsSource = null;

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            newRow["MTRLID"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();
            newRow["MATERIAL_LOCATION"] = cboOutputWHT1.GetBindValue();
            inDataTable.Rows.Add(newRow);
            new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        private void cboOutputWHT2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboOutputWHT2.SelectedValue.Equals(GetStandbyWarehouseCode()))
            {
                cboStockerTypeT2.IsEnabled = false;
                cboStockerT2.IsEnabled = false;
            }
            else
            {
                cboStockerTypeT2.IsEnabled = true;
                cboStockerT2.IsEnabled = true;
            }

            popSearchMtrlT2.SelectedValue = null;
            popSearchMtrlT2.SelectedText = null;
            popSearchMtrlT2.ItemsSource = null;

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
            inDataTable.Columns.Add("MTRLID", typeof(string));
            inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            newRow["MTRLID"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();
            newRow["MATERIAL_LOCATION"] = cboOutputWHT2.GetBindValue();
            inDataTable.Rows.Add(newRow);
            new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        #endregion

        #region Method

        private void SetOutputWHCombo()
        {            
            const string bizRuleName = "DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO";
            string[] arrColumn = { "LANGID", "FACILITY_CODE", "BUSINESS_USAGE_TYPE_CODE", "USE_FLAG", "ATTRIBUTE8" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "INV_WH_LOCATE", "Y", "Y" };            
            string selectedValueText = cboOutputWHT1.SelectedValuePath;
            string displayMemberText = cboOutputWHT1.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboOutputWHT1, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
            CommonCombo.CommonBaseCombo(bizRuleName, cboOutputWHT2, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private string GetStandbyWarehouseCode()
        {
            
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
            inDataTable.Columns.Add("BUSINESS_USAGE_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));
            inDataTable.Columns.Add("ATTRIBUTE14", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
            newRow["BUSINESS_USAGE_TYPE_CODE"] = "INV_WH_LOCATE";
            newRow["USE_FLAG"] = "Y";
            newRow["ATTRIBUTE14"] = "Y";

            inDataTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INV_SEL_TB_EGN_CODE_ITEM_INFO_CBO", "INDATA", "OUTDATA", inDataTable);

            string ProductionStandbyWarehouseCode = string.Empty;
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                ProductionStandbyWarehouseCode = Util.NVC(dtResult.Rows[0]["CBO_CODE"]);
            }

            return ProductionStandbyWarehouseCode;

        }

        #region 창고유형 콤보박스 조회 : SetStockerTypeCombo()

        /// <summary>
        /// 창고 유형 콤보박스 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "ATTR4", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, null,"Y", "AREA_EQUIPMENT_MTRL_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);

            if(cbo.SelectedValue.GetString() == "FWW" )
            {
                OutPutWH.IsEnabled = false;
                OutPutWHT2.IsEnabled = false;
                dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
                dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Collapsed;
            }
            else
            {
                OutPutWH.IsEnabled = true;
                OutPutWHT2.IsEnabled = true;
                dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
                dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Visibility = Visibility.Visible;
            }

        }

        #endregion

        #region 창고 콤보박스 조회 : SetStockerCombo()

        /// <summary>
        /// 창고 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.Empty;

            if (TabHold.IsSelected == true)
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.GetString()) ? null : cboStockerTypeT1.SelectedValue.GetString();

            }
            else if (TabRelese.IsSelected == true)
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT2.SelectedValue.GetString()) ? null : cboStockerTypeT2.SelectedValue.GetString();
            }
            else if (TabNoteChange.IsSelected == true)
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT3.SelectedValue.GetString()) ? null : cboStockerTypeT3.SelectedValue.GetString();
            }
            else
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT4.SelectedValue.GetString()) ? null : cboStockerTypeT4.SelectedValue.GetString();
            }

            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

         
        }

        #endregion

        #region 자재그룹정보 조회 : SetMtgrCombo()

        /// <summary>
        /// 자재그룹 정보
        /// </summary>
        /// <param name="cbo"></param>
        private void SetMtgrCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_INV_SEL_MTGRID_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region 자재ID 조회 : popSearchMtrl_ValueChanged()

        private void popSearchMtrl_ValueChanged(object sender, EventArgs e)
        {
            SearchMtrl();
        }

        #endregion

        #region 사유코드 : SetHoldCodeCombo()
        private void SetHoldCodeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "INV_HOLD_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            string ComboStatus = string.Empty;
            if (cbo.Name == "cboReasonCodeT1"|| cbo.Name == "cboReasonCodeT4")
            {
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
            }
            else
            {
                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
            }

          
        }
        #endregion

        #region HOLD유형 : SetHoldTypeCombo()
        private void SetHoldTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "INV_HOLD_TYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region HOLD/RELESE : SetHoldReleseCombo()
        private void SetHoldReleseCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "INV_HOLD_STATUS" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region 자재 정보 조회 : SearchMtrl()

        /// <summary>
        /// 기본 자재 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SearchMtrl_Group()
        {
            try
            {
               
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                 DataRow newRow = inDataTable.NewRow();
                newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT3.ItemsSource = DataTableConverter.Convert(searchResult);
                        popSearchMtrlT4.ItemsSource = DataTableConverter.Convert(searchResult);
                      
                    }
                    catch (Exception ex)
                    {

                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자재 정보 조회
        /// </summary>        
        private void SearchMtrl()
        {
            try
            {
                if (TabHold.IsSelected == true)
                {
                    if (popSearchMtrl.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrl.SelectedValue.ToString()))
                    {
                        popSearchMtrl.SelectedValue = null;
                        popSearchMtrl.SelectedText = null;
                        popSearchMtrl.ItemsSource = null;
                      
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrl.SelectedValue == null? null: popSearchMtrl.SelectedValue.ToString();
                    newRow["MATERIAL_LOCATION"] = cboOutputWHT1.GetBindValue();
                    inDataTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrl.ItemsSource = DataTableConverter.Convert(searchResult);
                          
                        }
                        catch (Exception ex)
                        {

                            Util.MessageException(ex);
                        }
                    });
                }
                else if (TabRelese.IsSelected == true)
                {
                    if (popSearchMtrlT2.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT2.SelectedValue.ToString()))
                    {
                        popSearchMtrlT2.SelectedValue = null;
                        popSearchMtrlT2.SelectedText = null;
                        popSearchMtrlT2.ItemsSource = null;
                       
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrlT2.SelectedValue.ToString();
                    newRow["MATERIAL_LOCATION"] = cboOutputWHT2.GetBindValue();
                    inDataTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT2.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {

                            Util.MessageException(ex);
                        }
                    });
                }
                else if (TabNoteChange.IsSelected == true)
                {
                    if (popSearchMtrlT3.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT3.SelectedValue.ToString()))
                    {
                        popSearchMtrlT3.SelectedValue = null;
                        popSearchMtrlT3.SelectedText = null;
                        popSearchMtrlT3.ItemsSource = null;
                        return;
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT3.SelectedValue == null ? null : popSearchMtrlT3.SelectedValue.ToString();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT3.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {

                            Util.MessageException(ex);
                        }
                    });
                }
                else
                {
                    if (popSearchMtrlT4.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchMtrlT4.SelectedValue.ToString()))
                    {
                        popSearchMtrlT4.SelectedValue = null;
                        popSearchMtrlT4.SelectedText = null;
                        popSearchMtrlT4.ItemsSource = null;
                        return;
                    }
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inDataTable.Columns.Add("MTRLID", typeof(string));
                    DataRow newRow = inDataTable.NewRow();
                    newRow["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                    newRow["MTRLID"] = popSearchMtrlT4.SelectedValue == null ? null : popSearchMtrlT4.SelectedValue.ToString();
                    inDataTable.Rows.Add(newRow);
                    new ClientProxy().ExecuteService("DA_INV_SEL_MTRL_ID", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                    {
                        try
                        {

                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            popSearchMtrlT4.ItemsSource = DataTableConverter.Convert(searchResult);
                        }
                        catch (Exception ex)
                        {

                            Util.MessageException(ex);
                        }
                    });
                }
                
               
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK내 HOLD대상 리스트 조회 : GetTargetHoldList()

        /// <summary>
        /// HOLD대상 리스트
        /// </summary>
        private void GetTargetHoldList()
        {
            try
            {
                Util.gridClear(dgListT1);

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                if(cboStockerTypeT1.SelectedValue.GetString() == "MWW")
                {
                    newRow["MATERIAL_LOCATION"] = cboOutputWHT1.GetBindValue();
                }
               if (cboOutputWHT1.SelectedValue.Equals(GetStandbyWarehouseCode()))
                {                    
                    isProductionStandbyWarehouseT1 = true;
                }
                else
                {
                    newRow["WH_TYPE"] = cboStockerTypeT1.SelectedValue.ToString();
                    newRow["STK_ID"] = cboStockerT1.SelectedValue == null ? null : cboStockerT1.SelectedValue.ToString();
                    isProductionStandbyWarehouseT1 = false;
                }
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrl.SelectedValue == null ? null : popSearchMtrl.SelectedValue.ToString();
                newRow["CARRIER_ID"] = txtPalletIDT1.GetBindValue();
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_INV_SEL_WAREHOUSE_MTRL", "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT1, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        #endregion

        #region STK내 Relese대상 리스트 조회 : GetTargetReleseList()

        /// <summary>
        /// Relese 대상 조회
        /// </summary>
        private void GetTargetReleseList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_REASON_CODE", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("MATERIAL_LOCATION", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_LOC", typeof(string));
                inDataTable.Columns.Add("PROC_LOC", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT2);

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MATERIAL_LOCATION"] = cboOutputWHT2.GetBindValue();

                if (cboOutputWHT2.SelectedValue.Equals(GetStandbyWarehouseCode()))
                {
                    newRow["MTRL_LOC"] = "Y";
                    isProductionStandbyWarehouseT2 = true;
                   
                }
                else
                {
                    newRow["PROC_LOC"] = "Y";
                    newRow["WH_TYPE"] = cboStockerTypeT2.SelectedValue.ToString();
                    newRow["STK_ID"] = cboStockerT2.SelectedValue == null ? null : cboStockerT2.SelectedValue.ToString();
                    isProductionStandbyWarehouseT2 = false;
                }
                
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrlT2.SelectedValue.ToString();
                newRow["HOLD_REASON_CODE"] = cboReasonCodeT2.SelectedValue == null ? null : cboReasonCodeT2.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT2.SelectedDateTime.ToString("yyyyMMdd");  
                newRow["END_DATE"] = dtpDateToT2.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT2.GetBindValue();
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_WAREHOUSE_HOLD_MTRL", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT2, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK내 Foil 자재 Relese대상 리스트 조회 : GetTargetReleseListFoil()

        /// <summary>
        /// Foil 자재 Relese 대상 조회
        /// </summary>
        private void GetTargetReleseListFoil()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_REASON_CODE", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_LOC", typeof(string));
                inDataTable.Columns.Add("PROC_LOC", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT2);

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROC_LOC"] = "Y";
                newRow["WH_TYPE"] = cboStockerTypeT2.SelectedValue.ToString();
                newRow["STK_ID"] = cboStockerT2.SelectedValue == null ? null : cboStockerT2.SelectedValue.ToString();
                isProductionStandbyWarehouseT2 = false;

                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT2.SelectedValue == null ? null : popSearchMtrlT2.SelectedValue.ToString();
                newRow["HOLD_REASON_CODE"] = cboReasonCodeT2.SelectedValue == null ? null : cboReasonCodeT2.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT2.SelectedDateTime.ToString("yyyyMMdd");
                newRow["END_DATE"] = dtpDateToT2.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT2.GetBindValue();
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_WAREHOUSE_HOLD_MTRL_FOIL", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT2, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK내 HOLD 사유변경 대상 리스트 : GetTargetChangeNoteList()
        /// <summary>
        /// HOLD 사유변경 대상 조회
        /// </summary>
        private void GetTargetChangeNoteList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_REASON_CODE", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_LOC", typeof(string));
                inDataTable.Columns.Add("PROC_LOC", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT3);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WH_TYPE"] = cboStockerTypeT3.SelectedValue.ToString();
                newRow["STK_ID"] = cboStockerT3.SelectedValue == null ? null : cboStockerT3.SelectedValue.ToString();
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT3.SelectedValue == null ? null : popSearchMtrlT3.SelectedValue.ToString();
                newRow["HOLD_REASON_CODE"] = cboReasonCodeT3.SelectedValue == null ? null : cboReasonCodeT3.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT3.SelectedDateTime.ToString("yyyyMMdd");
                newRow["END_DATE"] = dtpDateToT3.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT3.GetBindValue();
                newRow["PROC_LOC"] = "Y";
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_WAREHOUSE_HOLD_MTRL", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT3, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK내 Foil 자재 HOLD 사유변경 대상 리스트 : GetTargetChangeNoteListFoil()
        /// <summary>
        /// Foil 자재 HOLD 사유변경 대상 조회
        /// </summary>
        private void GetTargetChangeNoteListFoil()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_REASON_CODE", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_LOC", typeof(string));
                inDataTable.Columns.Add("PROC_LOC", typeof(string));
                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT3);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WH_TYPE"] = cboStockerTypeT3.SelectedValue.ToString();
                newRow["STK_ID"] = cboStockerT3.SelectedValue == null ? null : cboStockerT3.SelectedValue.ToString();
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT3.SelectedValue == null ? null : popSearchMtrlT3.SelectedValue.ToString();
                newRow["HOLD_REASON_CODE"] = cboReasonCodeT3.SelectedValue == null ? null : cboReasonCodeT3.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT3.SelectedDateTime.ToString("yyyyMMdd");
                newRow["END_DATE"] = dtpDateToT3.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT3.GetBindValue();
                newRow["PROC_LOC"] = "Y";
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_WAREHOUSE_HOLD_MTRL_FOIL", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT3, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK내 HOLD 이력 리스트 조회 : GettHoldHistoryList()

        /// <summary>
        /// HOLD 이력 리스트 조회
        /// </summary>
        private void GettHoldHistoryList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_STATUS", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));
                
                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT4);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WH_TYPE"] = cboStockerTypeT4.SelectedValue.ToString();
                newRow["STK_ID"] = cboStockerT4.SelectedValue == null ? null : cboStockerT4.SelectedValue.ToString();
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT4.SelectedValue == null ? null : popSearchMtrlT4.SelectedValue.ToString();
                newRow["HOLD_STATUS"] = cboHoldRelese.SelectedValue == null ? null : cboHoldRelese.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT4.SelectedDateTime.ToString("yyyyMMdd");
                newRow["END_DATE"] = dtpDateToT4.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT4.GetBindValue();
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_HOLD_MTRL_HISTORY", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT4, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        #endregion

        #region STK내 Foil 자재 HOLD 이력 리스트 조회 : GettHoldHistoryListFoil()

        /// <summary>
        /// Foil HOLD 이력 리스트 조회
        /// </summary>
        private void GettHoldHistoryListFoil()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("WH_TYPE", typeof(string));
                inDataTable.Columns.Add("STK_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_NAME_KEYWORD", typeof(string));
                inDataTable.Columns.Add("HOLD_STATUS", typeof(string));
                inDataTable.Columns.Add("SART_DATE", typeof(string));
                inDataTable.Columns.Add("END_DATE", typeof(string));
                inDataTable.Columns.Add("CARRIER_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT4);
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["WH_TYPE"] = cboStockerTypeT4.SelectedValue.ToString();
                newRow["STK_ID"] = cboStockerT4.SelectedValue == null ? null : cboStockerT4.SelectedValue.ToString();
                newRow["MTRL_NAME_KEYWORD"] = popSearchMtrlT4.SelectedValue == null ? null : popSearchMtrlT4.SelectedValue.ToString();
                newRow["HOLD_STATUS"] = cboHoldRelese.SelectedValue == null ? null : cboHoldRelese.SelectedValue.ToString();
                newRow["SART_DATE"] = dtpDateFromT4.SelectedDateTime.ToString("yyyyMMdd");
                newRow["END_DATE"] = dtpDateToT4.SelectedDateTime.ToString("yyyyMMdd");
                newRow["CARRIER_ID"] = txtPalletIDT4.GetBindValue();
                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_HOLD_MTRL_HISTORY_FOIL", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgListT4, searchResult, FrameOperation, true);
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        #endregion

        #region HOLD 처리 : ProcessHold()
        private void ProcessHold()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("RESERVATION_HOLD_FLAG", typeof(string));


                DataTable dtSelect = new DataTable();

                DataTable dtCopy = DataTableConverter.Convert(dgListT1.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i=0; i< dtCopy.Rows.Count; i++)
                {
                    if(dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for(int j=0; j< dtTo.Rows.Count; j++)
                        {
                            if(dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }

                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = "MANUAL";
                        if (isProductionStandbyWarehouseT1)
                        {
                            newRow["HOLD_YN"] = "N";
                        }
                        else
                        {
                            newRow["HOLD_YN"] = "Y";
                        }
                       
                        newRow["HOLD_CODE"] = cboReasonCodeT1.SelectedValue.ToString();
                        newRow["HOLD_NOTE"] = txtRemarkT1.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        if (isProductionStandbyWarehouseT1)
                        {
                            newRow["RESERVATION_HOLD_FLAG"] = "Y";
                        }
                        inTable.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetHoldList();
                        cboReasonCodeT1.SelectedIndex = 0;
                        txtRemarkT1.Text = string.Empty;
                        Util.MessageInfo("SFU1267"); //HOLD 등록이 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Foil자재 HOLD 처리 : ProcessHoldFoil()
        private void ProcessHoldFoil()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD_FOIL";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("RESERVATION_HOLD_FLAG", typeof(string));


                DataTable dtSelect = new DataTable();

                DataTable dtCopy = DataTableConverter.Convert(dgListT1.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i = 0; i < dtCopy.Rows.Count; i++)
                {
                    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }

                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = "MANUAL";
                        newRow["HOLD_YN"] = "Y";

                        newRow["HOLD_CODE"] = cboReasonCodeT1.SelectedValue.ToString();
                        newRow["HOLD_NOTE"] = txtRemarkT1.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["RESERVATION_HOLD_FLAG"] = null;
                        inTable.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetHoldList();
                        cboReasonCodeT1.SelectedIndex = 0;
                        txtRemarkT1.Text = string.Empty;
                        Util.MessageInfo("SFU1267"); //HOLD 등록이 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region HOLD 처리 Validation : ValidationHold()
        /// <summary>
        /// HOLD Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationHold()
        {
            if (!CommonVerify.HasDataGridRow(dgListT1))
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgListT1, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (cboReasonCodeT1.SelectedIndex == 0)
            {
                Util.MessageValidation("Hold 사유코드를 선택하세요");
                return false;
            }
            if (txtRemarkT1.Text == string.Empty)
            {
                Util.MessageInfo("SFU4300");// HOLD 사유를 입력하세요
                return false;
            }

            return true;
        }
        #endregion

        #region Release 처리 : ProcessRelease()
        private void ProcessRelease()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("RESERVATION_HOLD_FLAG", typeof(string));

                DataTable dtCopy = DataTableConverter.Convert(dgListT2.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i = 0; i < dtCopy.Rows.Count; i++)
                {
                    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }
                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = "MANUAL";
                        newRow["HOLD_YN"] = "N";
                        newRow["HOLD_CODE"] = dtTo.Rows[i]["HOLD_CODE"].GetString(); 
                        newRow["HOLD_NOTE"] = txtRemarkT2.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        if (isProductionStandbyWarehouseT2)
                        {
                            newRow["RESERVATION_HOLD_FLAG"] = "N";
                        }
                        inTable.Rows.Add(newRow);
                    }
                }
              

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetReleseList();
                        txtRemarkT2.Text = string.Empty;
                        Util.MessageInfo("SFU1268"); //HOLD 해제 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Foil 원자재 Release 처리 : ProcessReleaseFoil()
        private void ProcessReleaseFoil()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD_FOIL";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
               

                DataTable dtCopy = DataTableConverter.Convert(dgListT2.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i = 0; i < dtCopy.Rows.Count; i++)
                {
                    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }
                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = "MANUAL";
                        newRow["HOLD_YN"] = "N";
                        newRow["HOLD_CODE"] = dtTo.Rows[i]["HOLD_CODE"].GetString();
                        newRow["HOLD_NOTE"] = txtRemarkT2.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;
                      
                        inTable.Rows.Add(newRow);
                    }
                }


                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetReleseListFoil();
                        txtRemarkT2.Text = string.Empty;
                        Util.MessageInfo("SFU1268"); //HOLD 해제 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Release 처리 Validation : ValidationRelease()
        /// <summary>
        /// HOLD Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationRelease()
        {
            if (!CommonVerify.HasDataGridRow(dgListT2))
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgListT2, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (txtRemarkT2.Text == string.Empty)
            {
                Util.MessageInfo("SFU4301");// HOLD 해제 사유를 입력하세요
                return false;
            }

            return true;
        }
        #endregion

        #region 사유변경 처리 : ProcessChangeNote()
        private void ProcessChangeNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataTable dtCopy = DataTableConverter.Convert(dgListT3.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i = 0; i < dtCopy.Rows.Count; i++)
                {
                    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }

                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = dtTo.Rows[i]["HOLD_TYPE"].GetString(); 
                        newRow["HOLD_YN"] = "Y";
                        newRow["HOLD_CODE"] = cboReasonCodeT4.SelectedValue.ToString();
                        newRow["HOLD_NOTE"] = txtRemarkT3.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;

                        inTable.Rows.Add(newRow);
                    }
                }

              
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetChangeNoteList();
                        cboReasonCodeT4.SelectedIndex = 0;
                        txtRemarkT3.Text = string.Empty;
                        Util.MessageInfo("SFU1166"); //변경되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Foil 자재 사유변경 처리 : ProcessChangeNoteFoil()
        private void ProcessChangeNoteFoil()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "BR_INV_REG_MTRL_HOLD_FOIL";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("PALLET_ID", typeof(string));
                inTable.Columns.Add("HOLD_TYPE", typeof(string));
                inTable.Columns.Add("HOLD_YN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("HOLD_NOTE", typeof(string));
                inTable.Columns.Add("USER_ID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataTable dtCopy = DataTableConverter.Convert(dgListT3.ItemsSource);
                DataTable dtTo = dtCopy.Copy();

                for (int i = 0; i < dtCopy.Rows.Count; i++)
                {
                    if (dtCopy.Rows[i]["CHK"].ToString() == "1")
                    {
                        for (int j = 0; j < dtTo.Rows.Count; j++)
                        {
                            if (dtCopy.Rows[i]["REP_PROCESSING_GROUP_ID"].ToString() == dtTo.Rows[j]["REP_PROCESSING_GROUP_ID"].ToString())
                            {
                                dtTo.Rows[j]["CHK"] = 1;
                            }
                        }
                    }
                }

                for (int i = 0; i < dtTo.Rows.Count; i++)
                {
                    if (dtTo.Rows[i]["CHK"].ToString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["PALLET_ID"] = dtTo.Rows[i]["PALLET_ID"].GetString();
                        newRow["HOLD_TYPE"] = dtTo.Rows[i]["HOLD_TYPE"].GetString();
                        newRow["HOLD_YN"] = "Y";
                        newRow["HOLD_CODE"] = cboReasonCodeT4.SelectedValue.ToString();
                        newRow["HOLD_NOTE"] = txtRemarkT3.Text;
                        newRow["USER_ID"] = LoginInfo.USERID;
                        newRow["LANGID"] = LoginInfo.LANGID;

                        inTable.Rows.Add(newRow);
                    }
                }


                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        GetTargetChangeNoteListFoil();
                        cboReasonCodeT4.SelectedIndex = 0;
                        txtRemarkT3.Text = string.Empty;
                        Util.MessageInfo("SFU1166"); //변경되었습니다.
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        #region 사유변경 처리 Validation : ValidationChangeNote()
        /// <summary>
        /// HOLD Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationChangeNote()
        {
            if (!CommonVerify.HasDataGridRow(dgListT3))
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgListT3, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636"); //선택된 데이터가 없습니다.
                return false;
            }

            if (cboReasonCodeT4.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1342"); //Hold 사유를 선택하세요
                return false;
            }


            return true;
        }




        #endregion

        private void dgListT1_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgListT1.TopRows.Count; i < dgListT1.Rows.Count; i++)
                {

                    if (dgListT1.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgListT1.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgListT1.GetCell(idxS, dgListT1.Columns["CHK"].Index), dgListT1.GetCell(idxE, dgListT1.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgListT1.GetCell(idxS, dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT1.GetCell(idxE, dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgListT1.GetCell(idxS, dgListT1.Columns["CHK"].Index), dgListT1.GetCell(idxE, dgListT1.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgListT1.GetCell(idxS, dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT1.GetCell(idxE, dgListT1.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListT2_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgListT2.TopRows.Count; i < dgListT2.Rows.Count; i++)
                {

                    if (dgListT2.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT2.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListT2.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgListT2.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgListT2.GetCell(idxS, dgListT2.Columns["CHK"].Index), dgListT2.GetCell(idxE, dgListT2.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgListT2.GetCell(idxS, dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT2.GetCell(idxE, dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgListT2.GetCell(idxS, dgListT2.Columns["CHK"].Index), dgListT2.GetCell(idxE, dgListT2.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgListT2.GetCell(idxS, dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT2.GetCell(idxE, dgListT2.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT2.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListT3_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgListT3.TopRows.Count; i < dgListT3.Rows.Count; i++)
                {

                    if (dgListT3.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT3.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListT3.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgListT3.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgListT3.GetCell(idxS, dgListT3.Columns["CHK"].Index), dgListT3.GetCell(idxE, dgListT3.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgListT3.GetCell(idxS, dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT3.GetCell(idxE, dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgListT3.GetCell(idxS, dgListT3.Columns["CHK"].Index), dgListT3.GetCell(idxE, dgListT3.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgListT3.GetCell(idxS, dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT3.GetCell(idxE, dgListT3.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT3.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListT4_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgListT4.TopRows.Count; i < dgListT4.Rows.Count; i++)
                {

                    if (dgListT4.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")) + Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "HOLD_STATUS_DESC"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")) + Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "HOLD_STATUS_DESC"))  == sTmpLvCd)
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgListT4.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                      e.Merge(new DataGridCellsRange(dgListT4.GetCell(idxS, dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT4.GetCell(idxE, dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgListT4.GetCell(idxS, dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Index), dgListT4.GetCell(idxE, dgListT4.Columns["REP_PROCESSING_GROUP_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "REP_PROCESSING_GROUP_ID")) + Util.NVC(DataTableConverter.GetValue(dgListT4.Rows[i].DataItem, "HOLD_STATUS_DESC")) ;
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
