/*************************************************************************************
 Created Date : 2021.01.12
      Creator : 
   Decription : 작성이력보기
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.12  DEVELOPER : Initial Created.





 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_112.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_054_HISTORY : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sDate;
        private string _sLine;
        private string _sShift;
        private string _sEqp;
        private string _sWcType;
        private string _sModel;
        public FCS001_054_HISTORY()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _sDate = Util.NVC(tmps[0]);
                _sLine = Util.NVC(tmps[1]);
                _sShift = Util.NVC(tmps[2]);
                _sEqp = Util.NVC(tmps[3]);
                _sWcType = Util.NVC(tmps[4]);
                _sModel = Util.NVC(tmps[5]);
            }
            GetList();
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("WRK_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_DATE"] = _sDate;
                dr["EQSGID"] = _sLine;
                dr["MDLLOT_ID"] = _sModel;
                dr["SHFT_ID"] = _sShift;
                dr["WRKLOG_TYPE_CODE"] = _sWcType;
                dr["EQPTID"] = _sEqp;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WORKSHEET_WORKLOG_HISTORY", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgHistory, dtRslt, this.FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [Event]
        #endregion

    }
}
