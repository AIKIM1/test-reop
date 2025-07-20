/*************************************************************************************
 Created Date : 2017.05.08
      Creator : JMK
   Decription : UI Event Log
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.04  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_074 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_074()
        {
            InitializeComponent();

            Initialize();
        }
        
        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpFrom.SelectedDateTime = DateTime.Now;
            dtpTo.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }


        private void dtpFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime > dtpTo.SelectedDateTime)
            {
                dtpTo.SelectedDateTime = dtpFrom.SelectedDateTime;
            }
        }

        private void dtpTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpTo.SelectedDateTime < dtpFrom.SelectedDateTime)
            {
                dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
            }
        }
                

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        
        #endregion


        #region Mehod


        /// <summary>
        /// 조회
        /// </summary>
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("START_DTTM", typeof(string));
                dtRqst.Columns.Add("END_DTTM", typeof(string));
                dtRqst.Columns.Add("MENUID", typeof(string));
                dtRqst.Columns.Add("PC_NAME", typeof(string));
                dtRqst.Columns.Add("EVENT_CNTT", typeof(string));
                dtRqst.Columns.Add("WORK_USER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["START_DTTM"] = dtpFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["END_DTTM"] = dtpTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["MENUID"] = txtFormId.GetBindValue();
                dr["PC_NAME"] = txtPcId.GetBindValue();
                dr["EVENT_CNTT"] = txtEventCntt.GetBindValue();
                dr["WORK_USER"] = txtWorkerId.GetBindValue();
                dtRqst.Rows.Add(dr);

                btnSearchHold.IsEnabled = false;

                // Background 처리 완료시 dgList_ExecuteDataCompleted 이벤트 호출
                dgList.ExecuteService("DA_SEL_TB_SFC_UI_EVENT_HIST", "RQSTDT", "RSLTDT", dtRqst);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            btnSearchHold.IsEnabled = true;
        }

        #endregion

    }
}
