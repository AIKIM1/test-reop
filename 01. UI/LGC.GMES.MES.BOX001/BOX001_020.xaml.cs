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
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_020 : UserControl
    {

        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        public BOX001_020()
        {
            InitializeComponent();
            Loaded += BOX001_020_Loaded;
        }

        private void BOX001_020_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_020_Loaded;
            InitSet();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion


        #region Initialize

        private void InitSet()
        {
            dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString();
            dtpDateTo.Text = DateTime.Now.ToString();

            //타입 Combo Set.
            string[] sFilter5 = { "PACK_WRK_TYPE_CODE" };
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");
        }

        #endregion


        #region Event

        /// <summary>
        /// 교체 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 조회 전 이전 데이터 초기화
            dgReturnData.ItemsSource = null;

            string from = Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
            string to = Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["FROM_DTTM"] = from;
                dr["TO_DTTM"] = to;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("???", "RQSTDT", "RSLTDT", RQSTDT);
                dgReturnData.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion


        #region Mehod

        #endregion

    }
}
