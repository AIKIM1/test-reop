/*************************************************************************************
 Created Date : 2021.10.21
      Creator : 곽란영
   Decription : VD 대기 창고 입고 LOT 조회
--------------------------------------------------------------------------------------
 [Change History]
    2021.10.21  곽란영 선임 : 신규 생성
**************************************************************************************/

using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_073 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 


        public MCS001_073()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        private void InitializeCombo()
        {
            SetStockerCombo(cboStocker);
            
        }


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSelectJumboRollWareHousing()) return;

                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_WAREHOUSING_NJ";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? null : txtLotId.Text.Trim();
                inTable.Rows.Add(dr);

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
                        Util.GridSetData(dgLotList,result, FrameOperation, true);
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
        #endregion

        #region Mehod

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

            ////const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            ////string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            ////string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "VWW", LoginInfo.CFG_AREA_ID };
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            //string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }


        private bool ValidationSelectJumboRollWareHousing()
        {
            //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            //{
            //    Util.MessageValidation("SFU2042", "31");
            //    return false;
            //}

            return true;
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