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
    public partial class BOX001_300_LOT_DETL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_300_LOT_DETL()
        {
            InitializeComponent();
            Loaded += BOX001_300_LOT_DETL_Loaded;
        }

        private void BOX001_300_LOT_DETL_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtPalletid.Text = tmps[0] as string;
            txtLotid.Text = tmps[1] as string;

            getLotInfo(txtPalletid.Text.Trim(), txtLotid.Text.Trim());
        }
        
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Lot 상세 조회
        /// </summary>
        /// <param name="dataItem"></param>
        private void getLotInfo(string sPalletID, string sLotid)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_DETL_CP", "RQSTDT", "RSLTDT", RQSTDT);

                dgLOTInfo.ItemsSource = DataTableConverter.Convert(dtResult);

                string[] sColumnName = new string[] { "LOTID", "TRAYID" };
                _Util.SetDataGridMergeExtensionCol(dgLOTInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
        
    }
}
