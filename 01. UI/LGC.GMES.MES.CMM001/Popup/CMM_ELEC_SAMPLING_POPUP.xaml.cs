using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ELEC_RP_SAMPLING_POPUP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SAMPLING_POPUP : C1Window, IWorkArea
    {
        public DataTable PRID = new DataTable();
        DataSet indataSet = new DataSet();
        #region Initialize 
        #endregion
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ELEC_SAMPLING_POPUP()
        {
            InitializeComponent();
        }
        #region 테이블    

        #endregion



        #region EVENT
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {


                object[] tmps = C1WindowExtension.GetParameters(this);   
                            
                String PRID2 = tmps[0] as string;
                DataTable PRID = new DataTable();

                DataTable indata = indataSet.Tables.Add("INDATA");
                indata.Columns.Add("PRODID", typeof(string));
                indata.Columns.Add("LANGID", typeof(string));

                DataRow row = null;
                row = indata.NewRow();
                row["PRODID"] = PRID2;
                row["LANGID"] = LoginInfo.LANGID;
                indata.Rows.Add(row);

                DataTable outDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLING_POPUP", "RQSTDT", "RSLTDT", indata);

                dgSamplingPopup.ItemsSource = null; 
                //결과값 초기화 


                //데이터값이 0 이아닐경우에만 데이터를 불러옴
                if (outDt != null && outDt.Rows.Count > 0)
                {
                    Header = outDt.Rows[0]["PJTNAME"].ToString()   +   outDt.Rows[0]["CMCDNAME"].ToString().Substring(0,2)  +  ObjectDic.Instance.GetObjectName("QMS 대기 현황");
                    //Util.GridSetData(dgSamplingPopup, outDt, FrameOperation, true);

                    dgSamplingPopup.ItemsSource = DataTableConverter.Convert(outDt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region 함수

        #endregion     

        public CMM_ELEC_SAMPLING owner { get; internal set; }

        private void btnClose_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}