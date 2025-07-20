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
    public partial class COM001_021_LOTEHISTORY : C1Window, IWorkArea
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



        public COM001_021_LOTEHISTORY()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            SetParam();

            //아직 테이블이 완성되지 않아서 실제 data 가져올수 없어서 주석처리
            //getSpecdata();

            //TABLE이 구성되지 않아 TEST롤 데이터 뿌려줌...
            testGridData();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

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
                txtSearchLot.Text = tmmp02.Rows[0][0].ToString();
            }
        }

        private void getSpecdata()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품           

            DataRow dr = RQSTDT.NewRow();
            dr["LOTID"] = txtSearchLot.Text;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PILOTLOT_HISTORY", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgChangeHistory, dtResult);
        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("PARENT_LOTID", typeof(string));
            dtSearchResult.Columns.Add("PREV_LOTID", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_LOTID", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_USER", typeof(string));
            dtSearchResult.Columns.Add("USEYN", typeof(string));
            dtSearchResult.Columns.Add("CHANGEDATE", typeof(string));
            
            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "MBHV0501AP", "MBHV0501AP", "MBHV0501AP", "ADMIN", "Y", "2014-09-22 오전 10:32:23" });
            menulist.Add(new object[] { "MBHV0501AP", "MBHV0501AP", "MBHV0501AP", "ADMIN", "Y", "2014-09-22 오전 11:33:45" });
            menulist.Add(new object[] { "MBHV0501AP", "MBHV0501AP", "MBHV0501AP", "ADMIN", "N", "2014-09-23 오전 09:30:11" });

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

        
    }
}
