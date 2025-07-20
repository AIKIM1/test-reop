/*************************************************************************************
 Created Date : 2019.12.05
      Creator : 신광희
   Decription : Carrier 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.05  신광희 차장 : Initial Created.    
  2023.02.15  조영대      : 활성화에서 오픈시 Carrier 명칭을 Tray 로 변경.
  2023.12.05  김태우      : 오창 2산단 NFF 는 대표LOT 을 사용하여 쿼리 변경. 
  2023.12.07  김태우      : 컬럼명 carrierid/대표랏 변경
  2024.01.23  배현우      : 대표LOT 과 CARRIER 동시 사용하므로 비즈 변경
  2024.05.09  양강주      : Tray 세척/사용 이력 조회 기능 추가
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_035.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_035 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private decimal _searchCount = 100;

        public MCS001_035()
        {
            InitializeComponent();
            RepLotUseForm();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            
            InitializeControl();
            rdo100.IsChecked = true;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            ClearControl();

            //SelecCarrierHistoryList();
            //SelectCarrierInformation();

            ShowLoadingIndicator();
            SelecCarrierHistoryList((table, ex) =>
            {
                if (ex != null)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
                else
                {
                    SelectCarrierInformation();
                    Util.GridSetData(dgCarrierHistory, table, null, true);

                    //HiddenLoadingIndicator();
                }
            });
        }

        private void txtCarrierId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtCarrierId.Text.Trim()))
                        btnSearch_Click(btnSearch, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCarrierInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

            }));
        }

        private void dgCarrierInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
        }

        private void rdoCount_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;

            dtpDateFrom.IsEnabled = false;
            dtpDateTo.IsEnabled = false;

            switch (radioButton.Name)
            {
                case "rdo100":
                    _searchCount = 100;

                    break;
                case "rdo300":
                    _searchCount = 300;
                    break;
                case "rdo500":
                    _searchCount = 500;
                    break;
                case "rdoDate":
                    _searchCount = 0;
                    dtpDateFrom.IsEnabled = true;
                    dtpDateTo.IsEnabled = true;
                    break;
            }

        }

        private void dgCarrierInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgCarrierHistory_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

        }

        #endregion

        #region Method

        private void SelecCarrierHistoryList(Action<DataTable, Exception> actionCompleted = null)
        {
            string bizRuleName = "";
            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", LoginInfo.CFG_AREA_ID.ToString()))  //대표LOT 사용하는지
                bizRuleName = "BR_MCS_SEL_CARRIER_HISTORY_TRACE"; //DA ->BR 변경 공정마다 CARRIER, 대표LOT 둘 다 사용 2024-01-23 배현우
            else
                bizRuleName = "DA_MCS_SEL_CARRIER_HISTORY_TRACE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("LOTID", typeof(string));
				inTable.Columns.Add("COUNT", typeof(decimal));

                string fromDate, toDate;

                if((rdoDate.IsChecked != null && (bool)rdoDate.IsChecked))
                {
                    fromDate = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    toDate = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                }else
                {
                    fromDate = null;
                    toDate = null;
                }

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = fromDate; //dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = toDate; //dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
				dr["LOTID"] = !string.IsNullOrEmpty(txtLotId.Text) ? txtLotId.Text : null;
				dr["COUNT"] = _searchCount;
                inTable.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    actionCompleted?.Invoke(bizResult, bizException);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectCarrierInformation()
        {
            string bizRuleName = "";
            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", LoginInfo.CFG_AREA_ID.ToString()))  //대표LOT 사용하는지
                bizRuleName = "BR_MCS_SEL_CARRIER_DETAIL_INFO"; //DA ->BR 변경 공정마다 CARRIER, 대표LOT 둘 다 사용 2024-01-23 배현우
            else
                bizRuleName = "DA_MCS_SEL_CARRIER_DETAIL_INFO";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("LOTID", typeof(string));

				DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = !string.IsNullOrEmpty(txtCarrierId.Text) ? txtCarrierId.Text : null;
				dr["LOTID"] = !string.IsNullOrEmpty(txtLotId.Text) ? txtLotId.Text : null;
				inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    HiddenLoadingIndicator();
                    DataTable dtCarrierInfo = GetCarrierDetailInfo(bizResult);
                    Util.GridSetData(dgCarrierInfo, dtCarrierInfo, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetCarrierDetailInfo(DataTable dt)
        {
            DataTable dtCarrierInfo = new DataTable();
            dtCarrierInfo.Columns.Add("ITEM", typeof(string));
            dtCarrierInfo.Columns.Add("DATA", typeof(string));

			string strCstType = string.Empty;

            if (CommonVerify.HasTableRow(dt))
            {
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("Type"), dt.Rows[0]["CSTTYPE_NAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("사용자재"), dt.Rows[0]["CSTPROD_NAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("상태"), dt.Rows[0]["CSTSTAT_NAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("LOTID"), dt.Rows[0]["LOTID"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("설비"), dt.Rows[0]["EQPTNAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("공정"), dt.Rows[0]["PROCNAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("Port"), dt.Rows[0]["PORT_NAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("RACK"), dt.Rows[0]["RACK_NAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("Outer Carrier ID"), dt.Rows[0]["OUTER_CSTID"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("동"), dt.Rows[0]["AREANAME"]);
                dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("Defect"), dt.Rows[0]["CST_DFCT_FLAG"]);
				dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("비정상"), dt.Rows[0]["ABNORM_TRF_RSN_CODE_NAME"]);
				dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("Carrier 관리상태"), dt.Rows[0]["CST_MNGT_STAT_CODE_NAME"]);

				strCstType = dt.Rows[0]["CSTTYPE"].ToString();
			}

            if (!string.IsNullOrEmpty(txtCarrierId.Text) && !strCstType.Equals("PT") && !strCstType.Equals(""))
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["CSTID"] = txtCarrierId.Text;
                inTable.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_FINAL_LOT_CST_ACT_HIST", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("마지막 LOT ID"), dtResult.Rows[0]["LOTID"]);
                    dtCarrierInfo.Rows.Add(ObjectDic.Instance.GetObjectName("마지막 BOBBIN ID"), dtResult.Rows[0]["CSTID"]);
                }
            }

            return dtCarrierInfo;
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

        private void InitializeControl()
        {
            dtpDateTo.SelectedDateTime = GetSystemTime();
            dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddMonths(-1);

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
            {
                tbCarrierId.Text = ObjectDic.Instance.GetObjectName("Tray ID");
                tbCurrentCarrierInfo.Text = ObjectDic.Instance.GetObjectName("CURR_TRAY_INFO");
                dgCarrierHistory.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Tray ID");

            }

            #region 활성화 공정 화면 추가 제어
            {
                
                Visibility setVisible = (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F") == true && _util.IsAreaCommoncodeAttrUse("CARRIER_CLEAN_USE_COUNT_MNGT_PROC", "FORM_TRAY", new string[] { "" }) == true) ? Visibility.Visible : Visibility.Hidden;

                // Tray 세척 이력 조회 기능 숨김 처리
                dgCarrierHistory.Columns["USE_COUNT_LIMIT"].Visibility = setVisible;
                dgCarrierHistory.Columns["USE_COUNT"].Visibility = setVisible;
                dgCarrierHistory.Columns["ACCU_USE_COUNT"].Visibility = setVisible;
                dgCarrierHistory.Columns["CST_CLEAN_NAME"].Visibility = setVisible;
            }
            #endregion

        }

        private void ClearControl()
        {
            Util.gridClear(dgCarrierInfo);
            Util.gridClear(dgCarrierHistory);
        }

        private bool ValidationSearch()
        {
			if (string.IsNullOrEmpty(txtCarrierId.Text) && string.IsNullOrEmpty(txtLotId.Text))
            {
				Util.MessageValidation("SFU8275", tbCarrierId.Text);
				return false;
            }

            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


		#endregion

		private void txtLotId_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Enter)
				{
					if (!string.IsNullOrEmpty(txtLotId.Text.Trim()))
						btnSearch_Click(btnSearch, null);
				}
			}
			catch (Exception ex)
			{
				Util.MessageException(ex);
			}
		}
        private void RepLotUseForm()
        {

            if (_util.IsCommonCodeUseAttr("REP_LOT_USE_AREA", LoginInfo.CFG_AREA_ID))  //NFF 추가
            {
                dgCarrierHistory.Columns["CSTID"].Header = "CARRIER_REP_LOTID";

            }
            else
            {
                dgCarrierHistory.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("캐리어ID");

            }
        }
    }
}
