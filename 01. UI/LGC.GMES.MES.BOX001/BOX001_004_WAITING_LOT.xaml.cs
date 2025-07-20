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
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_004_WAITING_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 


        public BOX001_004_WAITING_LOT()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter2 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

        }

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 대기 Lot 조회
            Get_WaitLotList();

        }

        private void Get_WaitLotList()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                string sElect = string.Empty;

                if (cboElecType.SelectedIndex < 0 || cboElecType.SelectedValue.ToString().Trim().Equals(""))
                {
                    sElect = null;
                }
                else
                {
                    sElect = cboElecType.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("ELECRODE", typeof(String));
                RQSTDT.Columns.Add("WIPSTAT", typeof(String));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["PROCID"] = "E9100";
                dr["ELECRODE"] = sElect;
                dr["WIPSTAT"] = "WAIT";

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NISSAN_WAITLIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgWaitList);
                //dgWaitList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgWaitList, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
    }
}
