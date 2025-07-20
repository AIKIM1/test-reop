/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_021_CONTROLCHANGEHISTORY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtSearchResult;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        public COM001_021_CONTROLCHANGEHISTORY()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            SetParam();

            //아직 테이블이 완성되지 않아서 실제 data 가져올수 없어서 주석처리
            //getControldata();

            //TABLE이 구성되지 않아 TEST롤 데이터 뿌려줌...
            testGridData();
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }



        #endregion

        #region Mehod
        private void SetParam()
        {
            string tmp = C1WindowExtension.GetParameter(this);
            object[] tmps = C1WindowExtension.GetParameters(this);
            string tmmp01 = tmps[0] as string;
            DataTable tmmp02 = tmps[1] as DataTable;

            if(tmmp02 != null && tmmp02.Rows.Count > 0)
            {
                txtSearchFlow.Text = tmmp02.Rows[0][0].ToString();
            }
        }

        private void getControldata()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품           

            DataRow dr = RQSTDT.NewRow();
            dr["PRODID"] = txtSearchFlow.Text;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONTROL_HISTORY", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgChangeHistory, dtResult);
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("NO", typeof(string));
            dtSearchResult.Columns.Add("PROCID", typeof(string));
            dtSearchResult.Columns.Add("CTRLITEM", typeof(string));
            dtSearchResult.Columns.Add("CTRLNAME", typeof(string));
            dtSearchResult.Columns.Add("CNT", typeof(string));
            dtSearchResult.Columns.Add("MIN", typeof(string));
            dtSearchResult.Columns.Add("MAX", typeof(string));
            dtSearchResult.Columns.Add("UNIT", typeof(string));
            dtSearchResult.Columns.Add("USE_YN", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_TYPE", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_USER", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_DATE", typeof(string));           

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "1", "BT001", "BTC018", "CELL 전압 하한", "5", "1", "10", "999", "Y", "기존자료", "admin",   "2014-09-22 오전 10:32:23" });
            menulist.Add(new object[] { "2", "BT001", "BTC018", "CELL 전압 하한", "2", "1", "10", "999", "N", "기존자료", "oksoks",  "2014-09-22 오전 11:33:45" });
            menulist.Add(new object[] { "3", "BT001", "BTC018", "CELL 전압 하한", "4", "0", "8",  "999", "Y", "기존자료", "partner", "2014-09-23 오전 09:30:11" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtSearchResult.NewRow();
                newRow.ItemArray = item;
                dtSearchResult.Rows.Add(newRow);
            }

            SetBinding(dgChangeHistory, dtSearchResult);
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
    }
}
