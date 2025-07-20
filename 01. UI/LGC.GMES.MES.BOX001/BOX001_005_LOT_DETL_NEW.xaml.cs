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
    public partial class BOX001_005_LOT_DETL_NEW : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sWorker = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_005_LOT_DETL_NEW()
        {
            InitializeComponent();
            Loaded += BOX001_005_LOT_DETL_NEW_Loaded;
        }

        private void BOX001_005_LOT_DETL_NEW_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtPalletid.Text = tmps[0] as string;
            DataTable dtResult = tmps[1] as DataTable;
            sSHOPID = tmps[2] as string;
            sAREAID = tmps[3] as string;
            sWorker = tmps[4] as string;

            //dgLOTInfo.ItemsSource = DataTableConverter.Convert(dtResult);
            Util.GridSetData(dgLOTInfo, dtResult, FrameOperation);

        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnReConfirm_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                for (int i = 0; i < dgLOTInfo.GetRowCount(); i++)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));                    
                    RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = Util.NVC(dgLOTInfo.GetCell(i, dgLOTInfo.Columns["SUBLOTID"].Index).Value);
                    dr["SHOPID"] = sSHOPID;
                    dr["AREAID"] = Util.NVC(dgLOTInfo.GetCell(i, dgLOTInfo.Columns["AREAID"].Index).Value);
                    dr["EQSGID"] = Util.NVC(dgLOTInfo.GetCell(i, dgLOTInfo.Columns["EQSGID"].Index).Value);
                    dr["MDLLOT_ID"] = Util.NVC(dgLOTInfo.GetCell(i, dgLOTInfo.Columns["MDLLOT_ID"].Index).Value);
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "Y";
                    dr["INSP_SKIP_FLAG"] = "N";
                    dr["2D_BCR_SKIP_FLAG"] = "Y";
                    dr["USERID"] = sWorker;
                    RQSTDT.Rows.Add(dr);


                    // ClientProxy2007
                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", RQSTDT);
                    
                }

                // 정상적으로 처리되었으면 확인 메세지 출력
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3169"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);

                this.DialogResult = MessageBoxResult.OK;
                Close();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }


        #endregion

        #region Mehod


        #endregion


    }
}
