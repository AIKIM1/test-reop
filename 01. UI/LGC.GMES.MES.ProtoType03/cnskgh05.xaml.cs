/*************************************************************************************
 Created Date : 2016.10.01
      Creator : 김광호C
   Decription : 대차 모니터링 - Location 상세 창
--------------------------------------------------------------------------------------
  
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType03
{
    /// <summary>
    /// ASSY001_004_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnskgh05 : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _Type = string.Empty;
        private string _LocationCode = string.Empty;
        private string _LocationName = string.Empty;

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
        public cnskgh05()
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
                _LocationCode = Util.NVC(tmps[1]);
                _LocationName = Util.NVC(tmps[2]);
            }
            else
            {
                _Type = "CRT";
                _LocationCode = "";
                _LocationName = "";
            }

            txtLocID.Text = _LocationCode;
            txtLocName.Text = _LocationName;

            getDataFromRTLSDB();
        }

        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [BizCall]
        private void getDataFromRTLSDB()
        {
            try
            {
                string sCartType = _Type;
                string sLocationID = _LocationCode;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RFID_TYPE", typeof(string));
                RQSTDT.Columns.Add("LOCATION_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_TYPE"] = sCartType;
                dr["LOCATION_ID"] = sLocationID;
                RQSTDT.Rows.Add(dr);

                DataTable dTResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_TB_RTLS_ZONE_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dTResult != null && dTResult.Rows.Count > 0)
                {
                    if (_Type == "CRT")
                    {
                        dgLocationCartList.ItemsSource = DataTableConverter.Convert(dTResult);
                        dgLocationCartList.Visibility = Visibility.Visible;
                        dgLocationLottList.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        dgLocationLottList.ItemsSource = DataTableConverter.Convert(dTResult);
                        dgLocationLottList.Visibility = Visibility.Visible;
                        dgLocationCartList.Visibility = Visibility.Hidden;
                    }
                }

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
