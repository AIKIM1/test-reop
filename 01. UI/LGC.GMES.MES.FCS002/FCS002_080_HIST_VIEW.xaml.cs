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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_080_HIST_VIEW : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sDate;
        private string _sEqp;
        private string _sWorkType;

        public string DATE
        {
            set { this._sDate = value; }
        }
        public string EQP
        {
            set { this._sEqp = value; }
        }
        public string WORK_TYPE
        {
            set { this._sWorkType = value; }
        }

        public FCS002_080_HIST_VIEW()
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
            _sDate = tmps[0] as string;
            _sEqp = tmps[1] as string;
            _sWorkType = tmps[2] as string;
            
            GetList();
        }

        #endregion

        #region Mehod
        #region [조회]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("WRK_DTTM", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_DTTM"] = _sDate;
                dr["EQPTID"] = _sEqp;
                dr["SELF_INSP_TYPE_CODE"] = _sWorkType;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WORKINSP_HISTORY", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgHistory, dtRslt, this.FrameOperation, true);
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
