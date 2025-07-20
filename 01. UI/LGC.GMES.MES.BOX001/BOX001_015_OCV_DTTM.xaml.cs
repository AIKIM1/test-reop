/*************************************************************************************
 Created Date : 2017.12.08
      Creator : 이제섭S
   Decription : 선택한 Pallet의 MAX OCV 측정일시 표기.
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  INS 이제섭S : Initial Created.


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
    public partial class BOX001_015_OCV_DTTM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        string sPALLETIDs = string.Empty;


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_015_OCV_DTTM()
        {
            InitializeComponent();

            Loaded += BOX001_015_OCV_DTTM_Loaded;
        }

        private void BOX001_015_OCV_DTTM_Loaded(object sender, RoutedEventArgs e)
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            sPALLETIDs = tmps[0] as string;

            SearchOCVDate();



        }


        #endregion

        #region Initialize

        #endregion


        #region Event


        #endregion


        #region Mehod

        private void SearchOCVDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                if(!string.IsNullOrWhiteSpace(sPALLETIDs)) dr["PALLETID"] = sPALLETIDs;

                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OCV2_LOT_INFO_FOR_PERIOD", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgOCV, Result, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    
 
        #endregion


    }
}
