/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_018_BOX_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sSHOPID = string.Empty;
        string sPALLETIDs = string.Empty;
        string sAreaID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_018_BOX_HIST()
        {
            InitializeComponent();
            Loaded += BOX001_018_BOX_HIST_Loaded;
        }

        private void BOX001_018_BOX_HIST_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);            
            sPALLETIDs = tmps[0] as string;

            GetBoxHist();
        }


        #endregion

        #region Initialize

        #endregion


        #region Event


        #endregion


        #region Mehod

        private void GetBoxHist()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPALLETIDs;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_HIST_CP", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgBoxHist.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgBoxHist, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


    }
}
