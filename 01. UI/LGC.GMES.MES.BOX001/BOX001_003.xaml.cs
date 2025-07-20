/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_003 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public BOX001_003()
        {
            InitializeComponent();

            Initialize();
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

            SetEvent();

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo combo = new CommonCombo();

            String[] sFilter = { "NISSAN_STAT" };
            combo.SetCombo(cboType, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

        }
        #endregion


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            try
            {
                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sLotid = txtLotID.Text.ToString().Trim();
                string sType = cboType.SelectedValue.ToString();

                if (sLotid == "" || sLotid == null)
                {
                    sLotid = null;
                }

                if (sType == "")
                {
                    sType = null;
                }
                else if (sType == "PRINTED")
                {
                    sType = "PACKED";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(string));
                RQSTDT.Columns.Add("ENDDATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sLotid != "" ? null : sStartdate;
                dr["ENDDATE"] = sLotid != "" ? null : sEnddate;
                dr["LOTID"] = sLotid;
                dr["LABEL_CODE"] = "LBL0003";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXSTAT"] = sType;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RANID_PRINTHIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgPrintHist);
                //dgPrintHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgPrintHist, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

        }

    }
}
