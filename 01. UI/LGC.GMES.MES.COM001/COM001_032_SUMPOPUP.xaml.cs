/*************************************************************************************
 Created Date : 2016.10.26
      Creator : 김광호C
   Decription : 대차 모니터링 - 대차/LOT SUMMARY 조회
--------------------------------------------------------------------------------------
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// ASSY001_004_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_032_SUMPOPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Type = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
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
        public COM001_032_SUMPOPUP()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _Type = Util.NVC(tmps[0]);
            }
            else
            {
                _Type = "CT";
            }

            if (_Type == "CT")
            {
                getDataCartSummary();
                this.Height = 340;
                this.Header = "대차 유형별 현황";
            }
            else
            {
                getDataLotSummary("LOC", null);
                this.Height = 700;
                this.Header = "재공 현황";
            }
        }

        private void dgLocationList_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            string sLocationCode = "", sLocationName = "";
            DataRowView _dRow = dgLocationList.CurrentRow.DataItem as DataRowView;

            if (_dRow == null)
                return;

            sLocationCode = _dRow["LOCATION_CODE"].ToString();
            sLocationName = _dRow["LOCATION_NAME"].ToString();

            if (sLocationName == "TOTAL")
            {
                getDataLotSummary("WEEK", null);
            }
            else
            {
                getDataLotSummary("WEEK", sLocationCode);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void getDataCartSummary()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                // OLD/NEW
                DataTable dTResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_CART_SUM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dTResult != null && dTResult.Rows.Count > 0)
                {
                    dgCartTypeList.ItemsSource = DataTableConverter.Convert(dTResult);
                }
                dgCartTypeList.Visibility = Visibility.Visible;
                dgLocationList.Visibility = Visibility.Hidden;
                dgLocationLottList.Visibility = Visibility.Hidden;

                return;                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getDataLotSummary(string sType, string sLocationCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STYPE", typeof(string));
                RQSTDT.Columns.Add("LOCATION_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STYPE"] = sType;
                dr["LOCATION_CODE"] = sLocationCode;

                RQSTDT.Rows.Add(dr);

                // OLD/NEW
                DataTable dTResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_LOT_SUM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dTResult != null && dTResult.Rows.Count > 0)
                {
                    if (sType == "LOC")
                    {
                        dgLocationList.ItemsSource = DataTableConverter.Convert(dTResult);
                    }
                    else

                    {
                        dgLocationLottList.ItemsSource = DataTableConverter.Convert(dTResult);
                    }
                }
                dgLocationList.Visibility = Visibility.Visible;
                dgLocationLottList.Visibility = Visibility.Visible;
                dgCartTypeList.Visibility = Visibility.Hidden;

                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion

        
    }
}
