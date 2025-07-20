/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.15  LEEHJ     : 소형활성화 MES 복사
  2023.07.04  조영대    : FCS002_311_READ => FCS001_311_READ 복사
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_311_READ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRsltCode = string.Empty;
        private string _reqApprBizCode = string.Empty;

        public FCS001_311_READ()
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
                _reqNo = Util.NVC(tmps[0]);
                _reqRsltCode = Util.NVC(tmps[1]);
                _reqApprBizCode = Util.NVC(tmps[2]);

                this.Header = Util.NVC(tmps[3]);
            }
            SetRead();
        }

        #endregion

        #region Mehod

        #region [조회]
        public void SetRead()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_TYPE", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("IS_ALL", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_TYPE"] = _reqApprBizCode;
                dr["REQ_NO"] = _reqNo;
                if ((_reqApprBizCode.Equals("REQUEST_BIZWF_LOT") && !_reqRsltCode.Equals("DEL")) ||
                    (_reqApprBizCode.Equals("REQUEST_CANCEL_BIZWF_LOT") && _reqRsltCode.Equals("DEL")))
                {
                    dr["IS_ALL"] = "NONE";
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT,OUTCELL", inData);

                Util.gridClear(dgRequest);
                Util.GridSetData(dgRequest, dsRslt.Tables["OUTLOT"], FrameOperation, true);

                Util.gridClear(dgGrator);
                Util.GridSetData(dgGrator, dsRslt.Tables["OUTPROG"], FrameOperation, true);

                Util.gridClear(dgCellList);
                Util.GridSetData(dgCellList, dsRslt.Tables["OUTCELL"], FrameOperation, true);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();


                if (_reqRsltCode.Equals("DEL"))
                {
                    grApp.Visibility = Visibility.Collapsed;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion


    }
}
