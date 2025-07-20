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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_039_VDQA : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string cstid;

        public COM001_039_VDQA()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                cstid = Convert.ToString(tmps[0]);
            }

            SearchData();
        }

        #endregion

        #region Mehod
        #region [조회]

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = cstid;
                dt.Rows.Add(dr);

                DataTable result= new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_BTCH_WRK_VD_RESULT", "INDATA", "OUTDATA", dt);

                Util.GridSetData( dgVDQA, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }



        }
       
        #endregion
        #endregion


    }
}
