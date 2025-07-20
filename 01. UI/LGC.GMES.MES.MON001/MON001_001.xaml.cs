/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public MON001_001()
        {
            InitializeComponent();

            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtpDateTo.SelectedDateTime = DateTime.Now;

            DateTime ToDay = DateTime.Now.Date;
            var month_first_day = ToDay.AddDays(1 - ToDay.Day);

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(1 - dtpDateTo.SelectedDateTime.Day);

        }



        private void dgResult1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult1.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("DTTMFROM", typeof(string));
                    dtRqst.Columns.Add("DTTMTO", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgResult1.Rows[cell.Row.Index].DataItem, "AREAID"));

                    dr["DTTMFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["DTTMTO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TEST_MODE_EQPT_LIST", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgResult2, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void dgResult2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult2.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("DTTMFROM", typeof(string));
                    dtRqst.Columns.Add("DTTMTO", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgResult2.Rows[cell.Row.Index].DataItem, "EQPTID"));

                    dr["DTTMFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["DTTMTO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TEST_MODE_EQPT_LIST_DETAIL", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgResult3, dtRslt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTMFROM", typeof(string));
                dtRqst.Columns.Add("DTTMTO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);

                dr["DTTMFROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DTTMTO"] = dtpDateTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TEST_MODE_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgResult1, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult1);
            Util.gridClear(dgResult2);
        }
        #endregion

        #endregion

    }
}
