/*************************************************************************************
 Created Date : 2022.10.28
      Creator : 이태규
   Decription : 자재 재고실사(Box)-차수관리팝업
--------------------------------------------------------------------------------------
 [Change History]
  2022.10.28  이태규 : Initial Created.
***************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_037_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREAID = "";
        string _STCK_CNT_YM = "";
        string _STCK_CNT_SEQNO = "";
        private string sAreaID = string.Empty;
        const string _STCK_CNT_TYPE_CURR = "CURR";
        const string _STCK_CNT_TYPE_DTTM = "DTTM";

        DataTable _dtBeforeSet = new DataTable();
        public PACK003_037_POPUP()
        {
            InitializeComponent();
            InitCombo();
            if (LoginInfo.CFG_SHOP_ID.Equals("G481")) chkStockerFlag.Visibility = Visibility.Visible;

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
            //object[] tmps = C1WindowExtension.GetParameters(this);

            //if (tmps != null && tmps.Length >= 1)
            //{
            //    _AREAID = Util.NVC(tmps[0]);
            //    _STCK_CNT_YM = Util.NVC(tmps[1]);
            //    _STCK_CNT_SEQNO = Util.NVC(tmps[2]);

            //    cboAreaShot.SelectedValue = _AREAID;
            //    ldpMonthShot.SelectedDateTime = Convert.ToDateTime(_STCK_CNT_YM);
            //}
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            //차수를 추가하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2959"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //{
            //    if (result.ToString().Equals("OK"))
            //    {
            //        DegreeAdd_Chk();
            //    }
            //}
            //);
        }

        #region Mehod
        private DateTime GetCurrentTime()
        {
            //try
            //{
            //    DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
            //    return (DateTime)dt.Rows[0]["SYSTIME"];
            //}
            //catch (Exception ex) { }

            return DateTime.Now;
        }

        private void DegreeAdd_Chk()
        {
            //try
            //{
            //    DataTable INDATA = new DataTable();
            //    INDATA.TableName = "INDATA";
            //    INDATA.Columns.Add("AREAID", typeof(string));
            //    INDATA.Columns.Add("STCK_CNT_YM", typeof(string));
            //    INDATA.Columns.Add("STOCKCNT_TIME", typeof(DateTime));

            //    DataRow dr = INDATA.NewRow();
            //    dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
            //    dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
            //    dr["STOCKCNT_TIME"] = ldpMonthShot.SelectedDateTime.ToString("yyyy-MM") + "-01 06:00:00";

            //    if (dr["AREAID"].Equals("")) return;

            //    INDATA.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCKCNT_CHK", "INDATA", "OUTDATA", INDATA);

            //    string sSEQNO_CHANGE_FLAG = dtRslt.Rows[0]["RESULT"].ToString();
            //    if (dtRslt.Rows.Count > 0 && sSEQNO_CHANGE_FLAG.Equals("N"))
            //    {
            //        //월중 차월 차수 추가시는 기존 1차 전산재고를 변경하시겠습니까?
            //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4237"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //        {
            //            if (result.ToString().Equals("OK"))
            //            {
            //                DegreeAdd(sSEQNO_CHANGE_FLAG);
            //            }
            //        }
            //        );
            //    }
            //    else
            //    {
            //        DegreeAdd("Y");
            //        RegStockCountDiff("Y");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void DegreeAdd(string sSEQNO_CHANGE_FLAG)
        {
            //try
            //{
            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_NOTE", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_TYPE", typeof(string));
            //    RQSTDT.Columns.Add("SUM_DATE", typeof(string));                
            //    RQSTDT.Columns.Add("USERID", typeof(string));
            //    RQSTDT.Columns.Add("SEQNO_CHANGE_FLAG", typeof(string));
            //    RQSTDT.Columns.Add("STCK_CNT_RSLT_CREATION_FLAG", typeof(string));
            //    DataRow dr = RQSTDT.NewRow();
            //    dr["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
            //    dr["STCK_CNT_NOTE"] = Util.GetCondition(txtNoteShot, "SFU1404"); //Note를 입력하세요.
            //    dr["STCK_CNT_TYPE"] = rdoCurrent.IsChecked == true ? _STCK_CNT_TYPE_CURR : _STCK_CNT_TYPE_DTTM;
            //    dr["SUM_DATE"] = Util.GetCondition(ldpSumDate);
            //    dr["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); //동은필수입니다.
            //    dr["USERID"] = LoginInfo.USERID;
            //    dr["SEQNO_CHANGE_FLAG"] = sSEQNO_CHANGE_FLAG;
            //    if((bool)chkStockerFlag.IsChecked)dr["STCK_CNT_RSLT_CREATION_FLAG"]='Y'; // 체크할때만 정보 저장
            //    if (dr["AREAID"].Equals("")) return;
            //    if (dr["STCK_CNT_NOTE"].Equals("")) return;

            //    RQSTDT.Rows.Add(dr);
                
            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOCKCNT_START_SP", "INDATA", null, RQSTDT);
            //    sAreaID = cboAreaShot.SelectedValue.ToString();


            //    this.DialogResult = MessageBoxResult.OK;
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        // 전산재고 실사재고 비교 저장
        private void RegStockCountDiff(string stockCountCompleteFlag)
        {

            //string bizRuleName = "BR_PRD_REG_STCK_CNT_DIFF";

            //DataSet dsINDATA = new DataSet();
            //DataSet dsOUTDATA = new DataSet();

            //try
            //{
            //    DataTable dtINDATA = new DataTable("INDATA");
            //    dtINDATA.Columns.Add("LANGID", typeof(string));
            //    dtINDATA.Columns.Add("STCK_CNT_YM", typeof(string));
            //    dtINDATA.Columns.Add("AREAID", typeof(string));
            //    dtINDATA.Columns.Add("PRODID", typeof(string));
            //    dtINDATA.Columns.Add("STCK_CNT_SEQNO", typeof(string));
            //    dtINDATA.Columns.Add("COM_STCK_DIFF_QTY", typeof(int));
            //    dtINDATA.Columns.Add("RLTH_STCK_DIFF_QTY", typeof(int));
            //    dtINDATA.Columns.Add("COM_STCK_QTY", typeof(int));
            //    dtINDATA.Columns.Add("RLTH_STCK_QTY", typeof(int));
            //    dtINDATA.Columns.Add("NEXT_PROC_INPUT_QTY", typeof(int));
            //    dtINDATA.Columns.Add("NOTE", typeof(string));
            //    dtINDATA.Columns.Add("REGUSER", typeof(string));
            //    dtINDATA.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));

            //    DataRow drINDATA = dtINDATA.NewRow();
            //    drINDATA["LANGID"] = LoginInfo.LANGID;
            //    drINDATA["STCK_CNT_YM"] = Util.GetCondition(ldpMonthShot);
            //    drINDATA["STCK_CNT_SEQNO"] = this._STCK_CNT_SEQNO;
            //    drINDATA["AREAID"] = Util.GetCondition(cboAreaShot, "SFU3203"); // 동은필수입니다.
            //    drINDATA["COM_STCK_DIFF_QTY"] = -1;
            //    drINDATA["RLTH_STCK_DIFF_QTY"] = -1;
            //    drINDATA["COM_STCK_QTY"] = -1;
            //    drINDATA["RLTH_STCK_QTY"] = -1;
            //    drINDATA["NEXT_PROC_INPUT_QTY"] = -1;
            //    drINDATA["REGUSER"] = LoginInfo.USERID;
            //    drINDATA["NOTE"] = string.Empty;
            //    drINDATA["STCK_CNT_CMPL_FLAG"] = stockCountCompleteFlag;           // 차수마감시에는 Y, 실사차사유입력시에는 N
            //    dtINDATA.Rows.Add(drINDATA);
            //    dsINDATA.Tables.Add(dtINDATA);

            //    string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
            //    dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }
        #endregion
    }
}
