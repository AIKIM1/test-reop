/*************************************************************************************
 Created Date : 2024.09.12
      Creator : 오화백
   Decription : Init
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.12 DEVELOPER : Initial Created.
  2025.07.15 김선준   SI         PACK일경우 분기
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_215 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();
        DataTable dtMain = new DataTable();
        Util _Util = new Util();

        public MTRL001_215()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void InitCombo()
        {
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P")) //PACK
            {
                initcombo();
            }
            else
            {
                // 창고유형 콤보박스
                SetStockerTypeCombo(cboStokerType);
                // Stocker 콤보박스
                SetStockerCombo(cboStoker);
            }
        }

        #region 창고유형 콤보 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고 유형  콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStackerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P")) return; //PACK
            SetStockerCombo(cboStoker);
        }
        #endregion

        #region 조회버튼 이벤트 : btnSearch_Click()
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #endregion

        #region Mehod

        #region 창고유형 콤보박스 조회 : SetStockerTypeCombo()
        /// <summary>
        /// 창고 유형 콤보박스 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, "AREA_EQUIPMENT_MTRL_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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

            stockerType = string.IsNullOrEmpty(cboStokerType.SelectedValue.GetString()) ? null : cboStokerType.SelectedValue.GetString();

            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }
        #endregion

        #region [Combo Box Set] initcombo()
        private void initcombo()
        {
            //창고유형
            C1ComboBox[] cbCild = { cboStoker };
            string[] sFilter = new string[1];
            sFilter[0] = "WH_DIVS_CODE";
            combo.SetCombo(cboStokerType, CommonCombo.ComboStatus.NONE, cbChild: cbCild, sFilter: sFilter, sCase: "COMMCODE");

            //창고 
            C1ComboBox[] cboParent = { cboStokerType };
            combo.SetCombo(cboStoker, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboParent, sCase: "AUTO_MANUAL_WHID");

            cboStokerType.IsEnabled = false;
        }
        #endregion

        #region 창고상태 리스트 조회 : GetTargetHoldList()

        /// <summary>
        /// HOLD대상 리스트
        /// </summary>
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                inDataTable.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgList);

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQUIPMENT_ID"] = string.IsNullOrEmpty(cboStoker.SelectedValue.ToString()) ? null : cboStoker.SelectedValue.ToString();
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("P"))
                {
                    newRow["EQPT_GR_TYPE_CODE"] = null;
                }
                else
                {
                    newRow["EQPT_GR_TYPE_CODE"] = string.IsNullOrEmpty(cboStokerType.SelectedValue.ToString()) ? null : cboStokerType.SelectedValue.ToString();
                }

                inDataTable.Rows.Add(newRow);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_INV_SEL_WAREHOUSE_STATUS", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgList, searchResult, FrameOperation, true);
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

        #endregion
    }
}
