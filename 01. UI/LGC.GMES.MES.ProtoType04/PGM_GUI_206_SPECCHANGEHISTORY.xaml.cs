/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System.Collections.Generic;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_206_SPECCHANGEHISTORY : C1Window, IWorkArea
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

        public PGM_GUI_206_SPECCHANGEHISTORY()
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

            if (tmmp02 != null && tmmp02.Rows.Count > 0)
            {
                txtSearchSpec.Text = tmmp02.Rows[0][0].ToString();
            }
        }

        private void getSpecdata()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품           

            DataRow dr = RQSTDT.NewRow();
            dr["PRODID"] = txtSearchSpec.Text;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPEC_HISTORY", "RQSTDT", "RSLTDT", RQSTDT);

            SetBinding(dgChangeHistory, dtResult);

        }

        private void testGridData()
        {
            dtSearchResult = new DataTable();

            dtSearchResult.Columns.Add("NO", typeof(string));
            dtSearchResult.Columns.Add("PRODID", typeof(string));
            dtSearchResult.Columns.Add("ROUTID", typeof(string));
            dtSearchResult.Columns.Add("FLOWID", typeof(string));
            dtSearchResult.Columns.Add("MA", typeof(string));
            dtSearchResult.Columns.Add("SUBGRP_SIZE", typeof(string));
            dtSearchResult.Columns.Add("OOCRULEID", typeof(string));
            dtSearchResult.Columns.Add("REAL_SPC", typeof(string));
            dtSearchResult.Columns.Add("LCL_P", typeof(string));
            dtSearchResult.Columns.Add("CCL_P", typeof(string));
            dtSearchResult.Columns.Add("UCL_P", typeof(string));
            dtSearchResult.Columns.Add("SAMPLE_SIZE", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_TYPE", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_USER", typeof(string));
            dtSearchResult.Columns.Add("CHANGE_DATE", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "1", "MBHV0501AP", "CELL01", "DF01", "600", "30", "1", "Y", "0.0000", "0.0000", "0.0000", "1", "기존자료", "admin", "2014-09-22 오전 10:32:23" });
            menulist.Add(new object[] { "2", "MBHV0501AP", "CELL01", "DF01", "0", "1", "3", "N", "0", "0", "0", "1", "수정", "oksoks", "2014-09-22 오전 11:33:45" });
            menulist.Add(new object[] { "3", "MBHV0501AP", "CELL01", "DF01", "100", "10", "1", "Y", "0.0000", "0.0000", "0.0000", "1", "기존자료", "partner", "2014-09-23 오전 09:30:11" });

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