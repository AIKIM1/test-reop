/*************************************************************************************
 Created Date : 2016.10.01
      Creator : 김광호C
   Decription : 대차 모니터링 - Location 상세 창
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
    public partial class COM001_032_LOCPOPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Type = string.Empty;
        private string _EmptyYn = string.Empty;
        private string _CartType = string.Empty;
        private string _CartNo = string.Empty;
        private string _LocationCode = string.Empty;
        private string _LotID = string.Empty;
        private string _LocationName = string.Empty;
        private string _ProdDttmSt = string.Empty;
        private string _ProdDttmEt = string.Empty;

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
        public COM001_032_LOCPOPUP()
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
                _EmptyYn = Util.NVC(tmps[1]);
                _CartType = Util.NVC(tmps[2]);
                _CartNo = Util.NVC(tmps[3]);
                _LocationCode = Util.NVC(tmps[4]);
                _LocationName = Util.NVC(tmps[5]);
                _LotID = Util.NVC(tmps[6]);
                _ProdDttmSt = Util.NVC(tmps[7]);
                _ProdDttmEt = Util.NVC(tmps[8]);

                if (_CartType == "")
                    _CartType = null;
            }
            else
            {
                _Type = "CT";
                _EmptyYn = "";
                _CartType = "";
                _CartNo = "";
                _LocationCode = "";
                _LocationName = "";
                _LotID = "";
                _ProdDttmSt = "";
                _ProdDttmEt = "";
            }

            txtLocID.Text = _LocationCode;
            txtLocName.Text = _LocationName;

            if (_Type == "CT")
            {
                dgLocationCartList.Visibility = Visibility.Visible;
                dgLocationLottList.Visibility = Visibility.Hidden;
                dgLocationLottDetail.Visibility = Visibility.Hidden;
                this.Height = 600;
                this.Width = 750;
            }
            else
            {
                dgLocationLottList.Visibility = Visibility.Visible;
                dgLocationLottDetail.Visibility = Visibility.Visible;
                dgLocationCartList.Visibility = Visibility.Hidden;
                this.Height = 800;
                this.Width = 700;
            }

            getDataFromRTLSDB(_Type, "");

            if (_Type == "LT")
                getLotDetail();

        }

        // Detail 조회
        private void dgLocationLottList_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            getLotDetail();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void getDataFromRTLSDB(string sType, string sProdWeek)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RFID_TYPE", typeof(string));
                RQSTDT.Columns.Add("CART_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("CART_NO", typeof(string));
                RQSTDT.Columns.Add("EMPTY_CART_FLAG", typeof(string));
                RQSTDT.Columns.Add("LOCATION_ID", typeof(string));
                RQSTDT.Columns.Add("LOT_ID", typeof(string));
                RQSTDT.Columns.Add("AREA_ABBR_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODTYPE", typeof(string));
                RQSTDT.Columns.Add("PROC_ABBR_CODE", typeof(string));
                RQSTDT.Columns.Add("PROD_DTTM_ST", typeof(DateTime));
                RQSTDT.Columns.Add("PROD_DTTM_ET", typeof(DateTime));
                RQSTDT.Columns.Add("PROD_WEEK", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_TYPE"] = sType;
                dr["CART_TYPE_CODE"] = _CartType;
                dr["CART_NO"] = _CartNo;
                dr["EMPTY_CART_FLAG"] = _EmptyYn;
                dr["LOCATION_ID"] = _LocationCode;
                dr["LOT_ID"] = _LotID;

                if (_ProdDttmSt != null && _ProdDttmSt.Length > 0)
                    dr["PROD_DTTM_ST"] = Convert.ToDateTime(_ProdDttmSt);

                if (_ProdDttmEt != null && _ProdDttmEt.Length > 0)
                    dr["PROD_DTTM_ET"] = Convert.ToDateTime(_ProdDttmEt);

                dr["PROD_WEEK"] = sProdWeek;

                RQSTDT.Rows.Add(dr);

                // OLD/NEW
                //DataTable dTResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_LOCATION_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dTResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_ZONE_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dTResult != null && dTResult.Rows.Count > 0)
                {
                    if (sType == "CT")
                    {
                        dgLocationCartList.ItemsSource = DataTableConverter.Convert(dTResult);
                    }
                    else if (sType == "LT")
                    {
                        dgLocationLottList.ItemsSource = DataTableConverter.Convert(dTResult);
                    }
                    else if (sType == "LTD")
                    {
                        dgLocationLottDetail.ItemsSource = DataTableConverter.Convert(dTResult);
                    }
                }

                return;                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getLotDetail()
        {
            string sProd_week = "";

            if (dgLocationLottList.Rows.Count < 1)
                return;

            DataRowView _dRow = dgLocationLottList.CurrentRow.DataItem as DataRowView;

            if (_dRow == null)
                return;

            sProd_week = _dRow["YYYYMMWEEK"].ToString();

            if (sProd_week == "TOTAL")
                sProd_week = null;

            getDataFromRTLSDB("LTD", sProd_week);
        }

        #endregion

        
    }
}
