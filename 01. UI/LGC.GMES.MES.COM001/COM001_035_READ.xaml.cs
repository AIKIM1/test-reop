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
    public partial class COM001_035_READ : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqRslt = string.Empty;

        public COM001_035_READ()
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
                _reqRslt = Util.NVC(tmps[1]);
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
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                Util.GridSetData(dgRequest, dsRslt.Tables["OUTLOT"], FrameOperation, true);
                //dgRequest.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTLOT"]);

                Util.gridClear(dgGrator);
                Util.GridSetData(dgGrator, dsRslt.Tables["OUTPROG"], FrameOperation, true);
                //dgGrator.ItemsSource = DataTableConverter.Convert(dsRslt.Tables["OUTPROG"]);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();


                if (_reqRslt.Equals("DEL")) {
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
