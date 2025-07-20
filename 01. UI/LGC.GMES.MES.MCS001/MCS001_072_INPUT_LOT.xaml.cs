/*************************************************************************************
 Created Date : 2021.11.08
      Creator : 곽란영 대리
   Decription : 입고LOT 조회 [팝업]
--------------------------------------------------------------------------------------
 [Change History]
    0000.00.00  홍길동 차장 : 수정 내용 작성.
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_072_INPUT_LOT : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

		public MCS001_072_INPUT_LOT()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitializeCombo();
            object[] parameters = C1WindowExtension.GetParameters( this );
			
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            SetStockerCombo(cboStocker);
            SetElectrodeTypeCombo(cboElectrodeType);
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSelectPancakeWareHousing()) return;

                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_WAREHOUSING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MCS_CST_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["PRJT_NAME"] = !string.IsNullOrEmpty(txtProjectName.Text) ? txtProjectName.Text : null;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["MCS_CST_ID"] = !string.IsNullOrEmpty(txtSkidId.Text) ? txtSkidId.Text : null;
                dr["LOTID"] = !string.IsNullOrEmpty(txtLotId.Text) ? txtLotId.Text : null;
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //dgLotList.ItemsSource = DataTableConverter.Convert(result);
                        Util.GridSetData(dgLotList, result, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod

        private bool ValidationSelectPancakeWareHousing()
        {

            return true;
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            /// 6동 모니터링 화면을 타동에서 같이 사용
            /// STK 설비 구조가 변경되어 조회하는 BIZ가 분리
            /// 동별 호출 BIZ를 동별 공통코드 관리를 통해 처리
            /// COM_TYPE_CODE : VD_STK_CBO_BIZ

            //동별 공통코드
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "VD_STK_CBO_BIZ";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

            string scboBiz = dtResult.Rows[0]["COM_CODE"].ToString();

            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(scboBiz, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);


            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            //string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
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



        #endregion


    }
}