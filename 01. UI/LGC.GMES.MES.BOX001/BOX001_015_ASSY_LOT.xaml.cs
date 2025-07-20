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
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_015_ASSY_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sSHOPID = string.Empty;
        string sPALLETIDs = string.Empty;
        string sRCVISSID = string.Empty;
        string sAreaID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_015_ASSY_LOT()
        {
            InitializeComponent();
            Loaded += BOX001_015_ASSY_LOT_Loaded;
        }

        private void BOX001_015_ASSY_LOT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sSHOPID = tmps[0] as string;
            sPALLETIDs = tmps[1] as string;
            sAreaID = tmps[2] as string;
            if(tmps.Count() > 3)
                sRCVISSID = tmps[3] as string;
            GetAssyInfo();
            GetAssyLotSum();
        }


        #endregion

        #region Initialize

        #endregion


        #region Event


        #endregion


        #region Mehod

        private void GetAssyInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if(string.IsNullOrWhiteSpace(sRCVISSID))
                    dr["PALLETID"] = sPALLETIDs;
                else
                    dr["RCV_ISS_ID"] = sRCVISSID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);
                //  dgAssyLot.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgAssyLot, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAssyLotSum()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if (string.IsNullOrWhiteSpace(sRCVISSID))
                    dr["PALLETID"] = sPALLETIDs;
                else
                    dr["RCV_ISS_ID"] = sRCVISSID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SUM_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);
               // dgLotSum.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgLotSum, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
