/*************************************************************************************
 Created Date : 2017.05.30
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.30  DEVELOPER : Initial Created.
  2023.04.03  이홍주      소형활성화MES 복사





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_332_STOCKCNT_START : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREAID = "";
        string _STCK_CNT_YM = "";

        const string _STCK_CNT_TYPE_CURR = "CURR";
        const string _STCK_CNT_TYPE_DTTM = "DTTM";

        DataTable _dtBeforeSet = new DataTable();
        public FCS002_332_STOCKCNT_START()
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
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            _combo.SetCombo(cboAreaShot, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
        }

        #endregion



        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AREAID = Util.NVC(tmps[0]);
                _STCK_CNT_YM = Util.NVC(tmps[1]);

                cboAreaShot.SelectedValue = _AREAID;
                ldpMonthShot.SelectedDateTime = Convert.ToDateTime(_STCK_CNT_YM);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            //차수를 추가하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2959"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeAdd_Chk();
                }
            }
            );
        }

        #region Mehod
        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private void DegreeAdd_Chk()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("STCK_CNT_YM", typeof(string));
                INDATA.Columns.Add("STOCKCNT_TIME", typeof(DateTime));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["STOCKCNT_TIME"] = ldpMonthShot.SelectedDateTime.ToString("yyyy-MM") + "-01 06:00:00";

                if (dr["AREAID"].Equals("")) return;

                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCKCNT_CHK", "INDATA", "OUTDATA", INDATA);

                string sSEQNO_CHANGE_FLAG = dtRslt.Rows[0]["RESULT"].ToString();
                if (dtRslt.Rows.Count > 0 && sSEQNO_CHANGE_FLAG.Equals("N"))
                {
                    //월중 차월 차수 추가시는 기존 1차 전산재고를 변경하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4237"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result.ToString().Equals("OK"))
                        {
                            DegreeAdd(sSEQNO_CHANGE_FLAG);
                        }
                    }
                    );
                }
                else
                {
                    DegreeAdd("Y");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DegreeAdd(string sSEQNO_CHANGE_FLAG)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_NOTE", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_TYPE", typeof(string));
                RQSTDT.Columns.Add("SUM_DATE", typeof(string));                
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SEQNO_CHANGE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
                dr["STCK_CNT_NOTE"] = Util.GetCondition(txtNoteShot, "SFU1404"); //Note를 입력하세요.
                dr["STCK_CNT_TYPE"] = rdoCurrent.IsChecked == true ? _STCK_CNT_TYPE_CURR : _STCK_CNT_TYPE_DTTM;
                dr["SUM_DATE"] = Util.GetCondition(ldpSumDate);
                dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;
                dr["SEQNO_CHANGE_FLAG"] = sSEQNO_CHANGE_FLAG;

                if (dr["AREAID"].Equals("")) return;
                if (dr["STCK_CNT_NOTE"].Equals("")) return;

                RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_STOCKCNT", "RQSTDT", "RSLTDT", RQSTDT);
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCKCNT_START", "INDATA", null, RQSTDT);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCKCNT_START_SP", "INDATA", null, RQSTDT);
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
