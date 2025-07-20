/*************************************************************************************
 Created Date : 2020.10.20
      Creator : Kang Dong Hee
   Decription : Tray반송이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  NAME   : Initial Created
  2021.04.13  KDH    : 화면간 이동 시 초기화 현상 제거
  2022.12.07  이정미 : 비고란에 모든 내용 보이도록 수정
  2024.01.10  조영대 : 적재정보 컬럼 추가
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_025 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        #endregion

        #region [Initialize]
        public FCS001_025()
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
            SetWorkResetTime();
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                this.ClearValidation();
                if (string.IsNullOrEmpty(txtTrayID.Text))
                {
                    //Tray ID를 입력해주세요.
                    txtTrayID.SetValidation("FM_ME_0070");
                    return;
                }

                if ((dtpToDate.SelectedDateTime.Date - dtpFromDate.SelectedDateTime.Date).Days >= 7)
                {
                    Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                    return;
                }

                Util.gridClear(dgTrayLocHist);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["CMCDTYPE"] = "CSTSTAT";
                dr["CSTID"] = txtTrayID.Text.Trim();
                dr["USE_FLAG"] = "Y";
                dtRqst.Rows.Add(dr);

                // 백그라운드로 실행. 실행 후 dgTrayLocHist_ExecuteDataCompleted 처리
                dgTrayLocHist.ExecuteService("DA_SEL_CV_TRAY_POSITION_INFO", "RQSTDT", "RSLTDT", dtRqst, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 공통함수로 뺄지 확인 필요 START
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        // 공통함수로 뺄지 확인 필요 END

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void dgTrayLocHist_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            // 쿼리 완료.
        }

        #endregion


    }
}
