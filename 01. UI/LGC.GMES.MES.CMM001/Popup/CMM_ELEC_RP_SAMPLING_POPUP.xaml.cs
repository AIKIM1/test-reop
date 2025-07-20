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
    public partial class CMM_ELEC_RP_SAMPLING_POPUP : C1Window, IWorkArea
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
        public CMM_ELEC_RP_SAMPLING_POPUP()
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
                




               // if (item != null)                

                    
                    DataTable indata = indataSet.Tables.Add("INDATA");
                    indata.Columns.Add("PRODID", typeof(string));
                    indata.Columns.Add("LANGID", typeof(string));


                    
                    DataRow row = null;

                    
                   
                    row = indata.NewRow();

                    row["PRODID"] = PRID2;
                    row["LANGID"] = LoginInfo.LANGID;


                    indata.Rows.Add(row);


                            //   if (item != null)
                        //    {
                                //DataTable dt = new DataTable();
                                //dt.Columns.Add("PRODID");
                                //dt.Columns.Add("LANGID");
                                //DataRow dr = dt.NewRow();
                                //dr["PRODID"] = _prodID;
                                //dr["LANGID"] = LoginInfo.LANGID;
                                //dt.Rows.Add(dr);

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


                                

                //    Util.GridSetData(SamplingPopup, PRID, FrameOperation, false);

                //DataSet drpopup = new DataSet();
                //DataTable inData = indataSet.Tables.Add("INDATA");
                //dtPOPUP.Columns.Add("LOTID", typeof(string));
                //dtPOPUP.Columns.Add("PRODID", typeof(string));
                //dtPOPUP.Columns.Add("PJTNAME", typeof(string));
                //dtPOPUP.Columns.Add("PROCNAME", typeof(string));
                //dtPOPUP.Columns.Add("WIPHOLD", typeof(string));
                //dtPOPUP.Columns.Add("CMCDNAME", typeof(string));

                //DataTable dt = ((DataView)SamplingPopup.ItemsSource).Table;
                //DataRow row = null;

                //foreach (DataRow inRow in dt.Rows)
                //{
                //    if (Convert.ToBoolean(inRow["CHK"]))
                //    {
                //        row = inData.NewRow();

                //        row["LOTID"] = Util.NVC(inRow["LOTID"]).ToUpper();
                //        row["PRODID"] = Util.NVC(inRow["PRODID"]).ToUpper();
                //        row["PJTNAME"] = Util.NVC(inRow["PJTNAME"]).ToUpper();
                //        row["PROCNAME"] = Util.NVC(inRow["PROCNAME"]).ToUpper();
                //        row["WIPHOLD"] = Util.NVC(inRow["WIPHOLD"]).ToUpper();
                //        row["CMCDNAME"] = Util.NVC(inRow["CMCDNAME"]).ToUpper();

                //        indataSet.Tables["INDATA"].Rows.Add(row);
                //    }



                //  }
             
                // [pjtname] [CMCDNAME] 재고 현황
                //  }
            }




            catch (Exception ex)
            {
                throw ex;
            }
        }

        



        private void dgSampling_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {

        }
        #endregion

        #region 함수

        #endregion     


        public CMM_ELEC_RP_SAMPLING owner { get; internal set; }

        private void btnClose_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {

            this.Close();

        }
    }



}