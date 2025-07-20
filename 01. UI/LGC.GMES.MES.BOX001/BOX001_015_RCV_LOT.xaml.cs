/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.05.22  최도훈    : 인도네시아 조회시 오류나는 현상 수정




 
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
    public partial class BOX001_015_RCV_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sFROM_DTTM = string.Empty;
        string sTO_DTTM = string.Empty;
        string sAREAID = string.Empty;
        string sEQSGID = string.Empty;
        string sPRODID = string.Empty;
        string sEQPTID = string.Empty;
        string sPACK_WRK_TYPE_CODE = string.Empty;
       

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public BOX001_015_RCV_LOT()
        {
            InitializeComponent();
            Loaded += BOX001_015_RCV_LOT_Loaded;
        }

        private void BOX001_015_RCV_LOT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sFROM_DTTM = tmps[0] as string;
            sTO_DTTM = tmps[1] as string;
            sAREAID = tmps[2] as string;
            sEQSGID = tmps[3] as string;
            sPRODID = tmps[4] as string;
            sEQPTID = tmps[5] as string;
            sPACK_WRK_TYPE_CODE = tmps[6] as string;

            SearchData();
        }


        #endregion

        #region Initialize

        #endregion


        #region Event


        #endregion


        #region Mehod
        
        private void SearchData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrWhiteSpace(sFROM_DTTM)) dr["FROM_DTTM"] = sFROM_DTTM;
                if(!string.IsNullOrWhiteSpace(sTO_DTTM)) dr["TO_DTTM"] = sTO_DTTM;
                if(!string.IsNullOrWhiteSpace(sAREAID)) dr["AREAID"] = sAREAID;
                if(!string.IsNullOrWhiteSpace(sEQSGID)) dr["EQSGID"] = sEQSGID;
                if(!string.IsNullOrWhiteSpace(sPRODID)) dr["PRODID"] = sPRODID;
                if(!string.IsNullOrWhiteSpace(sEQPTID)) dr["EQPTID"] = sEQPTID;
                if(!string.IsNullOrWhiteSpace(sPACK_WRK_TYPE_CODE)) dr["PACK_WRK_TYPE_CODE"] = sPACK_WRK_TYPE_CODE;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RCV_LOT_INFO_FOR_PERIOD", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgResult, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
