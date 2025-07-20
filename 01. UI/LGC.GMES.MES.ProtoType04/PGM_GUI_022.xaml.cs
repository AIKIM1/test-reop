/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_022 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_022()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string sStartdate = dtpDateFrom.ToString();
            string sEnddate = dtpDateTo.ToString();
            string sLotid = txtLotID.Text.ToString().Trim();
            string sType = cboType.SelectedValue.ToString();

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("STARTDATE", typeof(DateTime));
            RQSTDT.Columns.Add("ENDDATE", typeof(DateTime));
            RQSTDT.Columns.Add("FLAG", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["STARTDATE"] = sStartdate;
            dr["ENDDATE"] = sEnddate;
            dr["FLAG"] = sType;
            dr["LOTID"] = sLotid;
            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_PRINTHIST", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                dgPrintHist.ItemsSource = DataTableConverter.Convert(result);

            });

        }

    }
}
