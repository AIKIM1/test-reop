using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Data;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_REPORT2 : C1Window, IWorkArea
    {
        string _LOTID = "";
        string _PROCID = "";
        string _CUTID = "";
        string _SKIDID = "";
        string _ENDLOTFLAG = "";
        int _iMixerCnt = 0;
        int _iReportCnt = 0;

        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_REPORT2()
        {
            InitializeComponent();
            ClearTextBlocks(grReport);

            this.Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Input,(System.Threading.ThreadStart)(() =>
                {
                    if (string.Equals(LoginInfo.CFG_CARD_AUTO, "Y") && string.Equals(_ENDLOTFLAG, "Y"))
                    {
                        DataTable skidInfo = GetMulitSkidInfo();

                        if (skidInfo.Rows.Count > 0)
                        {
                            for (int i = 0; i < skidInfo.Rows.Count; i++)
                            {
                                _iReportCnt = 0;
                                _iMixerCnt = 0;
                                _SKIDID = Util.NVC(skidInfo.Rows[i]["CSTID"]);
                                GetValue();                              

                                if (i == (skidInfo.Rows.Count - 1))
                                    SetHistoryCardPrint();
                                else
                                    SetHistoryCardAutoPrint();
                            }
                        }
                        else
                        {
                            SetHistoryCardPrint();
                        }
                    }
                }
            ));
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _LOTID = tmps[0].ToString();
            _PROCID = tmps[1].ToString();

            if (tmps.Length > 2)
                _SKIDID = tmps[2].ToString();

            if (tmps.Length > 3)
                _ENDLOTFLAG = tmps[3].ToString();

            GetValue();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

            if (dialog.ShowDialog() != true)
                return;

            dialog.PrintVisual(grReport, "GMES PRINT");
        }

        private void ClearTextBlocks(DependencyObject obj)
        {
            try
            {
                TextBlock tb = obj as TextBlock;

                if (tb != null)
                    if (!tb.Name.Equals(""))
                        tb.Text = "";

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                    ClearTextBlocks(VisualTreeHelper.GetChild(obj, i));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetValue()
        {
            ClearTextBlocks(grReport);

            grMixer02.Visibility = Visibility.Collapsed;

            for (int i = 1; i < 10; i++)
            {
                Grid gr = (Grid)grReport.FindName("gr0" + i);
                gr.Visibility = Visibility.Collapsed;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_HIERARCHY_V01", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (dtRslt.Rows[i]["PROCID"].ToString().Equals(Process.PRE_MIXING))
                        continue;

                    if (dtRslt.Rows[i]["PROCID"].ToString().Equals(Process.SRS_MIXING))
                        grMixer01.Visibility = Visibility.Collapsed;

                    grMixer02.Visibility = Visibility.Collapsed;

                    if (string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["MAX_ACTDTTM"])))
                        continue;

                    switch (Util.NVC(dtRslt.Rows[i]["PROCID"]))
                    {
                        case "E1000":
                            SetMixer(Util.NVC(dtRslt.Rows[i]["LOTID"]), Util.NVC(dtRslt.Rows[i]["PROCID"]), Util.NVC(dtRslt.Rows[i]["WIPSEQ"]));
                            break;

                        default:
                            SetReport(Util.NVC(dtRslt.Rows[i]["LOTID"]), Util.NVC(dtRslt.Rows[i]["PROCID"]), Util.NVC(dtRslt.Rows[i]["WIPSEQ"]));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void buttonGet_Click(object sender, RoutedEventArgs e)
        {
            ClearTextBlocks(grReport);

            _LOTID = txtLot.Text;
            _PROCID = txtProc.Text;

            grMixer02.Visibility = Visibility.Collapsed;

            for (int i = 1; i < 10; i++)
            {
                Grid gr = (Grid)grReport.FindName("gr0" + i);
                gr.Visibility = Visibility.Collapsed;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LOTID"] = txtLot.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_HIERARCHY_V01", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[i]["MAX_ACTDTTM"])))
                        continue;

                    switch (Util.NVC(dtRslt.Rows[i]["PROCID"]))
                    {
                        case "E1000":
                            SetMixer(Util.NVC(dtRslt.Rows[i]["LOTID"]), Util.NVC(dtRslt.Rows[i]["PROCID"]), Util.NVC(dtRslt.Rows[i]["WIPSEQ"]));
                            break;

                        default:
                            SetReport(Util.NVC(dtRslt.Rows[i]["LOTID"]), Util.NVC(dtRslt.Rows[i]["PROCID"]), Util.NVC(dtRslt.Rows[i]["WIPSEQ"]));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMixer(string sLot, string sProc, string sWipSeq)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProc;
                dr["WIPSEQ"] = sWipSeq;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_V01", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    _iMixerCnt++;

                    Grid gr = (Grid)grReport.FindName("grMixer0" + _iMixerCnt);
                    if (gr != null)
                    {
                        gr.Visibility = Visibility.Visible;

                        ((TextBlock)grReport.FindName("EQPT_MIXER0" + _iMixerCnt)).Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                        ((TextBlock)grReport.FindName("LOTID_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["LOTID"].ToString();
                        ((TextBlock)grReport.FindName("PRODID_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["PRODID"].ToString();
                        ((TextBlock)grReport.FindName("OUPUT_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["OUTPUT_QTY"].ToString();// + dtRslt.Rows[0]["UNIT_CODE"].ToString();
                        ((TextBlock)grReport.FindName("CALDATE_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["CALDATE"].ToString();
                        ((TextBlock)grReport.FindName("WORKER_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["WORKER"].ToString();
                        //((TextBlock)grReport.FindName("REMARK_MIXER0" + _iMixerCnt)).Text = dtRslt.Rows[0]["WIP_NOTE"].ToString();
                        ((TextBlock)grReport.FindName("REMARK_MIXER0" + _iMixerCnt)).Text = GetConvertRemark(dtRslt.Rows[0]["WIP_NOTE"].ToString());
                    }
                    //헤더
                    //((TextBlock)grReport.FindName("BATCH_NO0" + _iMixerCnt)).Text = dtRslt.Rows[0]["LOTID"].ToString();
                    //((TextBlock)grReport.FindName("BATCH_NO0" + _iMixerCnt)).Visibility = Visibility.Visible;
                    //헤더
                }

                if (sLot.Equals(_LOTID) && "E1000".Equals(_PROCID))
                    SetHeader(dtRslt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetReport(string sLot, string sProc, string sWipSeq)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProc;
                dr["WIPSEQ"] = sWipSeq;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_V01", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    /////////////////////////////////////////////////////////
                    //C20210712-000111 비고 Tag 수량 표기
                    //동별 공통코드에 공정 대상 여부 확인
                    DataTable dtAreaCom = new DataTable();
                    dtAreaCom.Columns.Add("LANGID", typeof(string));
                    dtAreaCom.Columns.Add("AREAID", typeof(string));
                    dtAreaCom.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dtAreaCom.Columns.Add("COM_CODE", typeof(string));

                    DataRow drCom = dtAreaCom.NewRow();

                    drCom["LANGID"] = LoginInfo.LANGID;
                    drCom["AREAID"] = dtRslt.Rows[0]["AREAID"].ToString();
                    drCom["COM_TYPE_CODE"] = "DFCT_TAG_QTY_CARD_PRINT";
                    drCom["COM_CODE"] = sProc;

                    dtAreaCom.Rows.Add(drCom);

                    DataTable dtArea = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtAreaCom);

                    //동별 공통코드 존재하면 NG Tag 수량 조회
                    DataTable dtTagReslt = new DataTable();
                    dtTagReslt = null; 

                    if (dtArea != null && dtArea.Rows.Count > 0)
                    {
                        DataTable dtTag = new DataTable();
                        dtTag.Columns.Add("LOTID", typeof(string));
                        dtTag.Columns.Add("WIPSEQ", typeof(string));
                        dtTag.Columns.Add("PROCID", typeof(string));
                        dtTag.Columns.Add("SKIDID", typeof(string));

                        DataRow drTag = dtTag.NewRow();

                        drTag["LOTID"] = sLot;
                        drTag["WIPSEQ"] = sWipSeq;
                        drTag["PROCID"] = sProc;
                        if (!string.IsNullOrEmpty(_SKIDID))
                            drTag["SKIDID"] = _SKIDID;

                        dtTag.Rows.Add(drTag);

                        dtTagReslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_NG_TAG_QTY", "RQSTST", "RSLTDT", dtTag);
                    }
                    /////////////////////////////////////////////////////////

                    //if (!dtRslt.Rows[0]["OUTPUT_QTY"].ToString().Equals("0"))
                    //{
                    String _Procd = string.Empty;
                    _iReportCnt++;
                    Grid gr = (Grid)grReport.FindName("gr0" + _iReportCnt);

                    gr.Visibility = Visibility.Visible;

                    _Procd = dtRslt.Rows[0]["PROCNAME"].ToString(); 
                    if (!dtRslt.Rows[0]["COATING_SIDE_TYPE"].ToString().Equals(""))
                    {
                        if (!LoginInfo.CFG_SHOP_ID.Equals("G383") || !LoginInfo.CFG_SHOP_ID.Equals("G382"))
                            _Procd += "(" + dtRslt.Rows[0]["COATING_SIDE_TYPE"].ToString() + ")";
                    }
                    if (_Procd.Length > 12 && _Procd.IndexOf("(") != -1)
                    {
                        ((TextBlock)grReport.FindName("PROC0" + _iReportCnt)).Text = _Procd.Substring(0, _Procd.IndexOf("(")) + "\n" + _Procd.Substring(_Procd.IndexOf("("), _Procd.Length - _Procd.IndexOf("("));
                        ((TextBlock)grReport.FindName("PROC0" + _iReportCnt)).Margin = new Thickness(0, -20, 0, 0);
                        ((TextBlock)grReport.FindName("EQPT0" + _iReportCnt)).Margin = new Thickness(0, 30, 0, 0);
                    }
                    else
                    {
                        ((TextBlock)grReport.FindName("PROC0" + _iReportCnt)).Text = _Procd;
                    }
                        

                    if (!dtRslt.Rows[0]["EQPTSHORTNAME"].ToString().Equals(""))
                    {
                        ((TextBlock)grReport.FindName("EQPT0" + _iReportCnt)).Text = "(" + dtRslt.Rows[0]["EQPTSHORTNAME"].ToString() + ")";
                    }
                    else
                    {
                        ((TextBlock)grReport.FindName("EQPT0" + _iReportCnt)).Text = "(" + dtRslt.Rows[0]["EQPTNAME"].ToString() + ")";
                    }
                    ((TextBlock)grReport.FindName("LOTID0" + _iReportCnt)).Text = dtRslt.Rows[0]["LOTID"].ToString();
                    ((TextBlock)grReport.FindName("OUTPUT_CR0" + _iReportCnt)).Text = dtRslt.Rows[0]["OUTPUT_QTY"].ToString();// + dtRslt.Rows[0]["UNIT_CODE"].ToString();
                    ((TextBlock)grReport.FindName("OUTPUT_SR0" + _iReportCnt)).Text = dtRslt.Rows[0]["OUTPUT_QTY2"].ToString();// + dtRslt.Rows[0]["UNIT_CODE"].ToString();
                    ((TextBlock)grReport.FindName("CALDATE0" + _iReportCnt)).Text = dtRslt.Rows[0]["CALDATE"].ToString();
                    ((TextBlock)grReport.FindName("WORKER0" + _iReportCnt)).Text = dtRslt.Rows[0]["WORKER"].ToString();
                    //((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = dtRslt.Rows[0]["WIP_NOTE"].ToString();
                    ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = GetConvertRemark(dtRslt.Rows[0]["WIP_NOTE"].ToString());
                    ((TextBlock)grReport.FindName("LOTNO0" + _iReportCnt)).Text = "LOT ID";
                    ((TextBlock)grReport.FindName("PROC_VERH")).Text =  ObjectDic.Instance.GetObjectName("전극버전");
                    ((TextBlock)grReport.FindName("SITE_LABEL")).Text = ObjectDic.Instance.GetObjectName("코팅생산지");
                    ((TextBlock)grReport.FindName("AMTRL_RATE_LABEL")).Text = ObjectDic.Instance.GetObjectName("활물질구분");
                    ((TextBlock)grReport.FindName("TITLE")).Text = ObjectDic.Instance.GetObjectName("전극이력카드");
                        
                    if (LoginInfo.LANGID.Equals("en-US"))
                    {
                        ((TextBlock)grReport.FindName("PROC_VERH")).FontSize = 12;
                        ((TextBlock)grReport.FindName("SITE_LABEL")).FontSize = 12;
                        ((TextBlock)grReport.FindName("AMTRL_RATE_LABEL")).FontSize = 10;
                        ((TextBlock)grReport.FindName("TITLE")).FontSize = 20;
                    }
                    if (sLot.Equals(_LOTID) && sProc.Equals(_PROCID))
                        SetHeader(dtRslt);

                    if (sProc.Equals(Process.COATING))
                        SetCoaterHeader(dtRslt);

                    //롤프레스일경우 태그정보 가져오기
                    if (sProc.Equals(Process.ROLL_PRESSING))
                    {
                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.Columns.Add("LOTID", typeof(string));
                        dtRqst1.Columns.Add("LANGID", typeof(string));
                        dtRqst1.Columns.Add("AREAID", typeof(string));

                        DataRow dr1 = dtRqst1.NewRow();

                        dr1["LANGID"] = LoginInfo.LANGID;
                        dr1["LOTID"] = sLot;
                        dr1["AREAID"] = dtRslt.Rows[0]["AREAID"].ToString();
                        dtRqst1.Rows.Add(dr1);

                        DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "OUTDATA", dtRqst1);

                        string sTag = "";

                        // CST ID 비고에 추가
                        if (dtRslt.Rows[0]["CSTID"].ToString() != "")
                            sTag = "[ " + dtRslt.Rows[0]["CSTID"].ToString() + " ] ";

                        //if ((string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6")) &&
                        //    string.Equals(dtRslt.Rows[0]["QA_INSP_TRGT_FLAG"], "Y") && string.Equals(dtRslt.Rows[0]["ELEC_TYPE"], "C"))
                        //    sTag += ObjectDic.Instance.GetObjectName("슬리터에서 샘플링 필요") + "\n";

                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"])) && Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]).Contains("|"))
                        {
                            sTag += GetConvertRemark(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]));
                        }
                        else
                        {
                            if (dtRslt1.Rows.Count > 0)
                            {
                                foreach (DataRow drRslt in dtRslt1.Rows)
                                    sTag += drRslt["CLCTNAME"].ToString() + ":" + drRslt["CLCTVAL01"].ToString() + " ";

                                sTag += "\n";
                            }

                            DataTable dtRqst2 = new DataTable();
                            dtRqst2.Columns.Add("LOTID", typeof(string));
                            dtRqst2.Columns.Add("LANGID", typeof(string));

                            DataRow dr2 = dtRqst2.NewRow();

                            dr2["LANGID"] = LoginInfo.LANGID;
                            dr2["LOTID"] = sLot;
                            dtRqst2.Rows.Add(dr2);

                            /*
                            DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_ROLL_REMARK", "INDATA", "OUTDATA", dtRqst2);

                            sTag += "압연회수: WIP Note " + Util.NVC(dtRslt2.Rows[0]["REMARK"]);
                            */

                            sTag += ObjectDic.Instance.GetObjectName("압연회수") + " : WIP Note  " + dtRslt.Rows[0]["WIP_NOTE"].ToString();
                        }

                        // Auto Stop 표시 추가 [2018-09-19]
                        if (string.Equals(Util.NVC(dtRslt.Rows[0]["AUTO_STOP_FLAG"]), "Y"))
                        {
                            if (!string.IsNullOrEmpty(sTag) && !string.Equals(sTag.Substring(sTag.Length - 1, 1), "\n"))
                                sTag += "\n" +  ObjectDic.Instance.GetObjectName("AUTOSTOP");
                            else
                                sTag += ObjectDic.Instance.GetObjectName("AUTOSTOP");
                        }

                        //NG Tag 수량 표기[C20210712-000111]
                        if (dtTagReslt != null && dtTagReslt.Rows.Count > 0)
                        {
                            sTag += "TAG QTY : " + dtTagReslt.Rows[0]["DFCT_TAG_QTY"].ToString();
                        }

                        ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sTag;
                    }
                    //코터 : Top/Back 구분 공정 적용???
                    else if (sProc.Equals(Process.COATING) || sProc.Equals(Process.INS_COATING))
                    {

                        string sTag = "";

                        // CST ID 비고에 추가
                        if (dtRslt.Rows[0]["CSTID"].ToString() != "")
                            sTag = "[ " + dtRslt.Rows[0]["CSTID"].ToString() + " ] ";

                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"])) && Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]).Contains("|"))
                        {
                            sTag += GetConvertRemark(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]));
                        }
                        else
                        {
                            DataTable dtRqst1 = new DataTable();
                            dtRqst1.Columns.Add("LOTID", typeof(string));
                            dtRqst1.Columns.Add("LANGID", typeof(string));

                            DataRow dr1 = dtRqst1.NewRow();

                            dr1["LANGID"] = LoginInfo.LANGID;
                            dr1["LOTID"] = sLot;
                            dtRqst1.Rows.Add(dr1);

                            DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_COATER_REMARK", "INDATA", "OUTDATA", dtRqst1);

                            sTag += Util.NVC(dtRslt1.Rows[0]["REMARK"]);
                        }

                        //설비불량코드에 해당하는 수량을 보여줌
                        DataTable dtDefect = new DataTable();
                        dtDefect.Columns.Add("LANGID", typeof(string));
                        dtDefect.Columns.Add("LOTID", typeof(string));
                        dtDefect.Columns.Add("PROCID", typeof(string));
                        dtDefect.Columns.Add("WIPSEQ", typeof(string));

                        DataRow drDefect = dtDefect.NewRow();
                        drDefect["LANGID"] = LoginInfo.LANGID;
                        drDefect["LOTID"] = sLot;
                        drDefect["PROCID"] = sProc;
                        drDefect["WIPSEQ"] = sWipSeq;
                        dtDefect.Rows.Add(drDefect);

                        DataTable dtDefectRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_DFCT_CLCT_BY_LOTID", "INDATA", "OUTDATA", dtDefect);
                        if(dtDefectRslt != null && dtDefectRslt.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(sTag) && sTag.Length > 0 && !sTag.Substring(sTag.Length - 1, 1).Equals("\n"))
                            {
                                sTag += "\n";
                            }
                            sTag += Util.NVC(dtDefectRslt.Rows[0]["RESULT"]);
                        }

                        //NG Tag 수량 표기[C20210712-000111]
                        if (dtTagReslt != null && dtTagReslt.Rows.Count > 0)
                        {
                            sTag += "TAG QTY : " + dtTagReslt.Rows[0]["DFCT_TAG_QTY"].ToString();
                        }

                        ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sTag;
                        //((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text += "\n" + sTag;
                    }
                    else if (sProc.Equals(Process.SLITTING) || sProc.Equals(Process.SRS_SLITTING) || sProc.Equals(Process.SLIT_REWINDING) || sProc.Equals(Process.HEAT_TREATMENT))
                    {
                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.Columns.Add("LOTID", typeof(string));
                        dtRqst1.Columns.Add("PROCID", typeof(string));
                        dtRqst1.Columns.Add("SKIDID", typeof(string));
                        string sTag = string.Empty;

                        DataRow dr1 = dtRqst1.NewRow();
                        dr1["LOTID"] = sLot;
                        dr1["PROCID"] = sProc;

                        if (!string.IsNullOrEmpty(_SKIDID))
                            dr1["SKIDID"] = _SKIDID;

                        dtRqst1.Rows.Add(dr1);

                        DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_SLITER_REMARK", "INDATA", "OUTDATA", dtRqst1);

                        if(dtRslt1 != null && dtRslt1.Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"])) && Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]).Contains("|"))
                            {
                                sTag = GetConvertMultiRemark(Util.NVC(dtRslt1.Rows[0]["SL_REMARK"]), Util.NVC(dtRslt1.Rows[0]["SL_LANE"]));
                            }
                            else
                            {
                                if (!Util.NVC(dtRslt1.Rows[0]["REMARK"]).Equals(""))
                                    sTag = "Lane QTY " + Util.NVC(dtRslt1.Rows[0]["REMARK"]);
                                if (!Util.NVC(dtRslt1.Rows[0]["WIPNOTE"]).Equals(""))
                                    sTag += "\n" + "Lane Note " + Util.NVC(dtRslt1.Rows[0]["WIPNOTE"]);
                            }

                            _CUTID = Util.NVC(dtRslt1.Rows[0]["CUT_ID"]);

                            //NG Tag 수량 표기[C20210712-000111]
                            if (dtTagReslt != null && dtTagReslt.Rows.Count > 0)
                            {
                                string sDfct = string.Empty;
                                for (int k = 0; k < dtTagReslt.Rows.Count; k++)
                                {
                                    if (k == 0)
                                        sDfct = dtTagReslt.Rows[k]["CHILD_GR_SEQNO"].ToString() + ":(" + dtTagReslt.Rows[k]["DFCT_TAG_QTY"].ToString() + ")";
                                    else
                                        sDfct = sDfct + "," + dtTagReslt.Rows[k]["CHILD_GR_SEQNO"].ToString() + ":(" + dtTagReslt.Rows[k]["DFCT_TAG_QTY"].ToString() + ")";
                                }
                                sTag += "TAG QTY : " + sDfct;
                            }

                        ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sTag;
                            ((TextBlock)grReport.FindName("LOTNO0" + _iReportCnt)).Text = "SKID ID";
                            ((TextBlock)grReport.FindName("LOTID0" + _iReportCnt)).Text = Util.NVC(dtRslt1.Rows[0]["CUT_ID"]);
                            ((TextBlock)grReport.FindName("OUTPUT_CR0" + _iReportCnt)).Text = dtRslt1.Rows[0]["OUTPUT_QTY"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                            ((TextBlock)grReport.FindName("OUTPUT_SR0" + _iReportCnt)).Text = dtRslt1.Rows[0]["OUTPUT_QTY2"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";

                            OUTPUT_CR.Text = dtRslt1.Rows[0]["OUTPUT_QTY"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                            LANE_QTY.Text = dtRslt1.Rows[0]["LANE_QTY"].ToString();
                            OUTPUT_SR.Text = dtRslt1.Rows[0]["OUTPUT_QTY2"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                            CUTID.Text = "SKID ID";
                            LOT_NO.Text = _CUTID;
                            LOT_NO_BAR.Text = "*" + _CUTID + "*";
                        }

                        #region MyRegion
                        //if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["WIP_NOTE"])) && Util.NVC(dtRslt.Rows[0]["WIP_NOTE"]).Contains("|"))
                        //{
                        //    sTag = GetConvertMultiRemark(Util.NVC(dtRslt1.Rows[0]["SL_REMARK"]), Util.NVC(dtRslt1.Rows[0]["SL_LANE"]));
                        //}
                        //else
                        //{
                        //    if (!Util.NVC(dtRslt1.Rows[0]["REMARK"]).Equals(""))
                        //        sTag = "Lane QTY " + Util.NVC(dtRslt1.Rows[0]["REMARK"]);
                        //    if (!Util.NVC(dtRslt1.Rows[0]["WIPNOTE"]).Equals(""))
                        //        sTag += "\n" + "Lane Note " + Util.NVC(dtRslt1.Rows[0]["WIPNOTE"]);
                        //}

                        //_CUTID = Util.NVC(dtRslt1.Rows[0]["CUT_ID"]);

                        ////NG Tag 수량 표기[C20210712-000111]
                        //if (dtTagReslt != null && dtTagReslt.Rows.Count > 0)
                        //{
                        //    string sDfct = string.Empty;
                        //    for (int k = 0; k < dtTagReslt.Rows.Count; k++)
                        //    {
                        //        if (k == 0)
                        //            sDfct = dtTagReslt.Rows[k]["CHILD_GR_SEQNO"].ToString() + ":(" + dtTagReslt.Rows[k]["DFCT_TAG_QTY"].ToString() + ")";
                        //        else
                        //            sDfct = sDfct + "," + dtTagReslt.Rows[k]["CHILD_GR_SEQNO"].ToString() + ":(" + dtTagReslt.Rows[k]["DFCT_TAG_QTY"].ToString() + ")";
                        //    }
                        //    sTag += "TAG QTY : " + sDfct;
                        //}

                        //((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sTag;
                        //((TextBlock)grReport.FindName("LOTNO0" + _iReportCnt)).Text = "SKID ID";
                        //((TextBlock)grReport.FindName("LOTID0" + _iReportCnt)).Text = Util.NVC(dtRslt1.Rows[0]["CUT_ID"]);
                        //((TextBlock)grReport.FindName("OUTPUT_CR0" + _iReportCnt)).Text = dtRslt1.Rows[0]["OUTPUT_QTY"].ToString() +"("+ dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                        //((TextBlock)grReport.FindName("OUTPUT_SR0" + _iReportCnt)).Text = dtRslt1.Rows[0]["OUTPUT_QTY2"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";

                        //OUTPUT_CR.Text = dtRslt1.Rows[0]["OUTPUT_QTY"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                        //LANE_QTY.Text = dtRslt1.Rows[0]["LANE_QTY"].ToString();
                        //OUTPUT_SR.Text = dtRslt1.Rows[0]["OUTPUT_QTY2"].ToString() + "(" + dtRslt1.Rows[0]["UNIT"].ToString() + ")";
                        //CUTID.Text = "SKID ID";
                        //LOT_NO.Text = _CUTID;
                        //LOT_NO_BAR.Text = "*" + _CUTID + "*"; 
                        #endregion
                    }

                    // 오창 소형, 남경 소형 전극 흡습포장 문구 추가 요청 [2018-11-14]
                    if ((string.Equals(LoginInfo.CFG_SHOP_ID, "A011") || string.Equals(LoginInfo.CFG_SHOP_ID, "G183")) &&
                        (string.Equals(sProc, Process.COATING) || string.Equals(sProc, Process.ROLL_PRESSING) || string.Equals(sProc, Process.TAPING) || string.Equals(sProc, Process.SLITTING)))
                    {
                        //2023-01-09 오화백  EP동은 Plant 정보가 소형전극이지만  물리적으로는 자동차전극 동에 있으므로 EP동일 경우는 해당문구 제외
                        if (LoginInfo.CFG_AREA_ID != "EP")
                        {
                            string sAddContents = string.Empty;
                            sAddContents = ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text;

                            if (!string.IsNullOrEmpty(sAddContents) && !string.Equals(sAddContents.Substring(sAddContents.Length - 1, 1), "\n"))
                                sAddContents += "\n" + MessageDic.Instance.GetMessage("SFU5056");
                            else
                                sAddContents += MessageDic.Instance.GetMessage("SFU5056");

                            ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sAddContents;
                        }
                    }

                    // 동별 자주검사 항목 MMD기준정보 기준으로 항목 표시 추가 [2019-02-08] , C20190129_10541
                    DataTable collectDt = GetDataCollectHistory(sLot, Util.NVC_Int(sWipSeq));                  
                    if ( collectDt != null && collectDt.Rows.Count > 0)
                    {
                        string sAddContents = string.Empty;
                        sAddContents = ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text;

                        foreach( DataRow collectRow in collectDt.Rows)
                        {
                            if (!string.IsNullOrEmpty(sAddContents) && !string.Equals(sAddContents.Substring(sAddContents.Length - 1, 1), "\n"))
                                sAddContents += "\n" + Util.NVC(collectRow[0]);
                            else
                                sAddContents += Util.NVC(collectRow[0]);
                        }
                        ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sAddContents;
                    }

                    //롤맵이면서 코터일경우 코터 TAG 불량정보 항목추가 _ 2022.03.25, 
                    if((string.Equals(sProc, Process.COATING) && string.Equals(dtRslt.Rows[0]["ROLLMAP_APPLY_FLAG"].ToString(), "Y")))
                    {
                        string sAddContents = string.Empty;
                        sAddContents = ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text;

                        DataTable dtRqst1 = new DataTable();
                        dtRqst1.Columns.Add("LOTID", typeof(string));
                        dtRqst1.Columns.Add("LANGID", typeof(string));

                        DataRow dr1 = dtRqst1.NewRow();
                        dr1["LOTID"] = sLot;
                        dr1["LANGID"] = LoginInfo.LANGID;


                        dtRqst1.Rows.Add(dr1);

                        DataTable dtDefectRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_RP_TAG_SECTION_RM", "INDATA", "OUTDATA", dtRqst1);
                        double missMax = 0;
                        double missMin = 0;

                        double etcMax = 0;
                        double etcMin = 0;


                        //NG Tag 수량 표기[C20210712-000111]
                        if (dtDefectRslt != null && dtDefectRslt.Rows.Count > 0)
                        {
                            string misMatch = ObjectDic.Instance.GetObjectName("미스매치");
                            string etcDefect = ObjectDic.Instance.GetObjectName("기타불량");

                            //NG NAME 데이터에 "미스매치" 단어가 들어가면 미스매치로 그룹바이.
                            //StartPosition 은 최소값을 EndPosition 은 최대값을 표현
                            var missQuery = dtDefectRslt.AsEnumerable().Where(a => a.Field<string>("TAG_NG_NAME").IndexOf(misMatch) > 0)
                                            .GroupBy(g => g.Field<string>("TAG_NG_NAME").IndexOf(misMatch) > 0)
                               .Select(y => new
                               {
                                   tagStartPosition = y.Min(z => z.GetValue("TAG_START_POSITION").GetDecimal()),
                                   tagEndPosition = y.Max(z => z.GetValue("TAG_END_POSITION").GetDecimal()),
                               }).FirstOrDefault();

                            if (missQuery != null)
                            {
                                missMin = missQuery.tagStartPosition.GetDouble();
                                missMax = missQuery.tagEndPosition.GetDouble();
                            }

                            //NG NAME 데이터에 "미스매치" 단어가 없으면 기타임. 없는 것으로 그룹바이
                            //StartPosition 은 최소값을 EndPosition 은 최대값을 표현
                            var etcQuery = dtDefectRslt.AsEnumerable().Where(a => a.Field<string>("TAG_NG_NAME").IndexOf(misMatch) <= 0)
                                             .GroupBy(g => g.Field<string>("TAG_NG_NAME").IndexOf(misMatch) <= 0)
                                .Select(s => new
                                {
                                    tagStartPosition = s.Min(z => z.GetValue("TAG_START_POSITION").GetDecimal()),
                                    tagEndPosition = s.Max(z => z.GetValue("TAG_END_POSITION").GetDecimal())
                                }).FirstOrDefault();

                            if (etcQuery != null)
                            {
                                etcMin = etcQuery.tagStartPosition.GetDouble();
                                etcMax = etcQuery.tagEndPosition.GetDouble();
                            }
                            sAddContents += etcDefect + "(" + etcMin + " ~ " + etcMax + ") ,"+ misMatch + "(" + missMin + " ~ " + missMax + ")";
                        }

                        ((TextBlock)grReport.FindName("REMARK0" + _iReportCnt)).Text = sAddContents;
                    }
                } 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetHistoryCardPrintCount()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));

            DataRow inDetailDataRow = inDataTable.NewRow();
            inDetailDataRow["LOTID"] = _LOTID;
            inDetailDataRow["PROCID"] = _PROCID;
            inDataTable.Rows.Add(inDetailDataRow);

            new ClientProxy().ExecuteService("DA_PRD_UPD_PROCESS_LOT_HIST_PRINT_CNT", "INDATA", null, inDataTable, (result, returnEx) =>
            {
                try
                {
                    if (returnEx != null)
                        return;
                }
                catch (Exception ex) { }
            });
        }

        private DataTable GetPrintCount()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _LOTID;
                Indata["PROCID"] = _PROCID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetMulitSkidInfo()
        {
            if (!string.Equals(_PROCID, Process.SLITTING))
                return new DataTable();

            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _LOTID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_SKID_INFO", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetDataCollectHistory(string sLotID, int iWipSeq)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotID;
                Indata["WIPSEQ"] = iWipSeq;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_CARD_QA_REMARK", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetHeader(DataTable dtRslt)
        {
            CUTID.Text = "LOT ID";
            LOT_NO.Text = dtRslt.Rows[0]["LOTID"].ToString();
            LOT_NO_BAR.Text = "*" + dtRslt.Rows[0]["LOTID"].ToString() + "*";
            OUTPUT_CR.Text = dtRslt.Rows[0]["OUTPUT_QTY"].ToString();// + dtRslt.Rows[0]["UNIT_CODE"].ToString();
            LANE_QTY.Text = dtRslt.Rows[0]["LANE_QTY"].ToString();
            OUTPUT_SR.Text = dtRslt.Rows[0]["OUTPUT_QTY2"].ToString();// + dtRslt.Rows[0]["UNIT_CODE"].ToString();
            PRODID.Text = dtRslt.Rows[0]["PRODID"].ToString();
            MODEL.Text = dtRslt.Rows[0]["MODLID"].ToString();
            PRJT_NAME.Text = dtRslt.Rows[0]["PRJT_NAME"].ToString();
            PROC_VER.Text = dtRslt.Rows[0]["PROD_VER_CODE"].ToString();
            ELE_TYPE.Text = dtRslt.Rows[0]["ELEC_TYPE_NAME"].ToString();
            LIMIT_DATE.Text = dtRslt.Rows[0]["VLD_DATE"].ToString();
            AREANAME.Text = dtRslt.Rows[0]["AREANAME"].ToString();

            if (string.IsNullOrEmpty(SITE_NAME.Text))
                SITE_NAME.Text = dtRslt.Rows[0]["SITE_NAME"].ToString();

            if (string.IsNullOrEmpty(AMTRL_RATE.Text))
                AMTRL_RATE.Text = dtRslt.Rows[0]["AMTRL_RATE_INFO"].ToString();

            PRINT_DATE.Text = "PRINT DATE : " + dtRslt.Rows[0]["PRINT_DATE"].ToString();
            numCardCopies.Value = LoginInfo.CFG_CARD_COPIES;
        }

        private void SetCoaterHeader(DataTable dtRslt)
        {
            SITE_NAME.Text = dtRslt.Rows[0]["SITE_NAME"].ToString();
        }

        private void SetHistoryCardPrint()
        {
            double scale = 1000 / grReport.ActualHeight;

            this.Width = 760;

            // Y Scale 설정값으로도 조정 가능하게 변경 [2017-08-01]
            if (LoginInfo.CFG_ETC.Rows.Count > 0 && LoginInfo.CFG_ETC.Rows[0].ItemArray.Length >= 4 && !string.IsNullOrEmpty(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) &&
                Util.NVC_Int(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) < 100)
                grReport.LayoutTransform = new ScaleTransform(1, Convert.ToDouble(Util.NVC_Decimal(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) / 100));
            else if (scale < 1)
                grReport.LayoutTransform = new ScaleTransform(1, scale);

            grMain.Children.Remove(grReport);

            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            dialog.PrintTicket.CopyCount = Convert.ToInt32(numCardCopies.Value);

            if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                dialog.PrintQueue = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());

            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);

            FixedPage page1 = new FixedPage();
            page1.Width = document.DocumentPaginator.PageSize.Width;
            page1.Height = document.DocumentPaginator.PageSize.Height;

            page1.Children.Add(grReport);

            PageContent page1Content = new PageContent();
            ((IAddChild)page1Content).AddChild(page1);
            document.Pages.Add(page1Content);

            try
            {
                dialog.PrintDocument(document.DocumentPaginator, "GMES PRINT");

                // 이력 회수 관리
                SetHistoryCardPrintCount();

                if (string.Equals(LoginInfo.CFG_CARD_AUTO, "Y") && string.Equals(_ENDLOTFLAG, "Y"))
                {
                    this.Close();
                }
                else
                {
                    Util.MessageInfo("SFU1236", (result) =>
                    {
                        this.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo("SFU1933", (result) =>
                {
                    this.Close();
                });
            }
        }

        private void SetHistoryCardAutoPrint()
        {
            double scale = 1000 / grReport.ActualHeight;

            this.Width = 760;

            // Y Scale 설정값으로도 조정 가능하게 변경 [2017-08-01]
            if (LoginInfo.CFG_ETC.Rows.Count > 0 && LoginInfo.CFG_ETC.Rows[0].ItemArray.Length >= 4 && !string.IsNullOrEmpty(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) &&
                Util.NVC_Int(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) < 100)
                grReport.LayoutTransform = new ScaleTransform(1, Convert.ToDouble(Util.NVC_Decimal(LoginInfo.CFG_ETC.Rows[0]?[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString()) / 100));
            else if (scale < 1)
                grReport.LayoutTransform = new ScaleTransform(1, scale);

            string gridClone = XamlWriter.Save(grReport);
            System.IO.StringReader stringReader = new System.IO.StringReader(gridClone);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            Grid newGrid = (Grid)XamlReader.Load(xmlReader);

            PrintDialog dialog = new PrintDialog();

            dialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            dialog.PrintTicket.CopyCount = Convert.ToInt32(numCardCopies.Value);

            if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                dialog.PrintQueue = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());

            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);

            FixedPage page1 = new FixedPage();
            page1.Width = document.DocumentPaginator.PageSize.Width;
            page1.Height = document.DocumentPaginator.PageSize.Height;

            page1.Children.Add(newGrid);

            PageContent page1Content = new PageContent();
            ((IAddChild)page1Content).AddChild(page1);
            document.Pages.Add(page1Content);

            try
            {
                dialog.PrintDocument(document.DocumentPaginator, "GMES PRINT");
            }
            catch (Exception ex) {}
        }

        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (numCardCopies.Value == 0)
                return;

            // 이력카드 PRINT
            DataTable printDT = GetPrintCount();

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT2"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        SetHistoryCardPrint();
                    }
                });
            }
            else
            {
                SetHistoryCardPrint();
            }
        }

        private string GetConvertRemark(string sRemark)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Clear();
            string[] wipNotes = sRemark.Split('|');

            for (int i = 0; i < wipNotes.Length; i++)
            {
                if (!string.IsNullOrEmpty(wipNotes[i]))
                {
                    if (i == 0)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + wipNotes[i]);
                    else if (i == 1)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[i]);
                    else if (i == 2)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[i]);
                    else if (i == 3)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[i]);
                    else if (i == 4)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[i]);
                    else if (i == 5)
                        strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[i]);
                    strBuilder.Append("\n");
                }
            }
            return strBuilder.ToString();
        }

        private string GetConvertMultiRemark(string sRemark, string sLane)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Clear();
            string[] lanes = sRemark.Split('^');
            string[] wipNoteLanes = sLane.Split('|');

            string sWord = string.Empty;
            for (int i = 0; i < lanes.Length; i++)
            {
                string[] wipNotes = lanes[i].Split('|');
                for (int j = 0; j < wipNotes.Length; j++)
                {
                    if (!string.IsNullOrEmpty(wipNotes[j]))
                    {
                        if (j == 0)
                            sWord += wipNotes[j] + "(" + wipNoteLanes[i] + ")" + ",";

                        if ( i == (lanes.Length - 1))
                        {
                            if (!string.IsNullOrEmpty(sWord) && string.Equals(sWord.Substring(sWord.Length - 1, 1), ","))
                                sWord = sWord.Substring(0, sWord.Length - 1);

                            if (j == 0)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("특이사항") + " : " + sWord);
                            else if (j == 1)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("공통특이사항") + " : " + wipNotes[j]);
                            else if (j == 2)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("조정횟수") + " : " + wipNotes[j]);
                            else if (j == 3)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("압연횟수") + " : " + wipNotes[j]);
                            else if (j == 4)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("색지정보") + " : " + wipNotes[j]);
                            else if (j == 5)
                                strBuilder.Append(ObjectDic.Instance.GetObjectName("합권이력") + " : " + wipNotes[j]);
                            strBuilder.Append("\n");
                        }
                    }
                }
            }
            return strBuilder.ToString();
        }
    }
}