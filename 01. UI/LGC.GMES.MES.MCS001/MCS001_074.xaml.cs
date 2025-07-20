/*************************************************************************************
 Created Date : 2021.10.21
      Creator : 곽란영
   Decription : VD 대기 창고 입출고 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
    2021.10.21  곽란영 선임 : 신규 생성
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_074.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_074 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        #endregion

        public MCS001_074()
        {
            InitializeComponent();
            InitializeCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSelectPancakeInout()) return;

                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_INOUT_LIST_NJ";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MCS_CST_ID", typeof(string));
                inTable.Columns.Add("PORTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["PRJT_NAME"] = !string.IsNullOrEmpty(txtProjectName.Text.Trim()) ? txtProjectName.Text : null;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["MCS_CST_ID"] = !string.IsNullOrEmpty(txtSkidId.Text.Trim()) ? txtSkidId.Text : null;
                dr["PORTID"] = cboPortId.SelectedValue;
                dr["LOTID"] = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                inTable.Rows.Add(dr);

                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

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

                        Util.GridSetData(dgLotList, result, FrameOperation, true);  // 프레임 하단 조회건수 표시 방식으로 변경
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
            SetPortCombo(cboPortId);
        }

        #endregion

        #region Mehod

        private void InitializeCombo()
        {
            SetStockerCombo(cboStocker);
            SetElectrodeTypeCombo(cboElectrodeType);
            SetPortCombo(cboPortId);
        }

        private bool ValidationSelectPancakeInout()
        {
            //if (cboStocker?.SelectedValue == null || cboStocker.SelectedValue.GetString() == "SELECT")
            //{
            //    return false;
            //}

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

            ////const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            ////string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            ////string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "VWW", LoginInfo.CFG_AREA_ID };
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            //string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            //string selectedValueText = cbo.SelectedValuePath;
            //string displayMemberText = cbo.DisplayMemberPath;

            //CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetInoutTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "MCS_CMD_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
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

        private void SetPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PORT_BY_EQPTID";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboStocker.SelectedValue.GetString() };
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
