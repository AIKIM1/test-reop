using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_326_REQUEST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_326_SORTING_REQUEST : C1Window, IWorkArea
    {
        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        public BOX001_326_SORTING_REQUEST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
            }
            SetRead();
        }
        #region [조회]
        public void SetRead()
        {
            try
            {
                txtReqNote.Text = string.Empty;    //요청명
                txtReqNo.Text = string.Empty;      //요청번호
                txtPrcsDivs.Text = string.Empty;   //처리구분
                txtReqDttm.Text = string.Empty;    //요청일
                txtReqCmpl.Text = string.Empty;  //요청완료일
                txtRefUser.Text = string.Empty;    //담당자

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATE");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_TB_SFC_APPR_REQ_SUBLOT", "RQSTDT", "RSLTDT", inData);
                DataSet dsApprRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_SEL_TB_SFC_APPR_PROG", "RQSTDT", "RSLTDT", inData);

                txtReqNote.Text = dsRslt.Tables["RSLTDT"].Rows[0]["REQ_NOTE"].ToString();    //요청명
                txtReqNo.Text = dsRslt.Tables["RSLTDT"].Rows[0]["REQ_NO"].ToString(); ;      //요청번호
                txtPrcsDivs.Text = dsRslt.Tables["RSLTDT"].Rows[0]["PROCESS_CATEGORY"].ToString();   //처리구분
                txtReqDttm.Text = dsRslt.Tables["RSLTDT"].Rows[0]["REQ_DTTM"].ToString();    //요청일
                txtReqCmpl.Text = dsRslt.Tables["RSLTDT"].Rows[0]["REQ_CMPL_DATE"].ToString();    //요청완료일
                txtRefUser.Text = dsRslt.Tables["RSLTDT"].Rows[0]["REF_USER"].ToString();    //담당자

                Util.gridClear(dgInLot);
                Util.GridSetData(dgInLot, dsRslt.Tables["RSLTDT"], FrameOperation);
                
                Util.gridClear(dgApprList);
                Util.GridSetData(dgApprList, dsApprRslt.Tables["RSLTDT"], FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }

}
