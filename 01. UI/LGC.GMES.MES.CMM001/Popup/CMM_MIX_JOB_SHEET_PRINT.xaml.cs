/*************************************************************************************
 Created Date : 2019.09.06
      Creator : 오화백
   Decription : MIX 작업일지 출력
--------------------------------------------------------------------------------------
 [Change History]
   2019.09.24  DEVELOPER : 러시아로 번역시 에러나는 부분 수정.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Globalization;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_MIX_JOB_SHEET_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        DataTable dtDocument = null;     //배치정보
        DataTable dtMtrlGrd = null;      //원재료계량
        DataTable dtMix = null;          // 믹싱
        DataTable dtSelfInsp = null;     //자주검사
        DataTable dtChkScale = null;     //체크스케일
        DataTable dtInputQty = null;     //총투입량
        DataTable dtMtrlGrdReady = null; //원재료 준비
        DataTable dtChkScaleOut = null;  //체크스케일 배출
        DataTable dtSlurry = null;       //슬러리 이송

        string _Remark = string.Empty;
        private string _paperSize = string.Empty;
        private string _SettingPrintName = string.Empty;

        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public CMM_MIX_JOB_SHEET_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            dtDocument     = tmps[0] as DataTable;            //배치정보
            dtMtrlGrd      = tmps[1] as DataTable;            //원재료계량
            dtMix          = tmps[2] as DataTable;            // 믹싱
            dtSelfInsp     = tmps[3] as DataTable;            //자주검사
            dtChkScale     = tmps[4] as DataTable;            //체크스케일
            dtInputQty     = tmps[5] as DataTable;            //총투입량
            dtMtrlGrdReady = tmps[6] as DataTable;            //원재료 준비
            dtChkScaleOut  = tmps[7] as DataTable;            //체크스케일 배출
            dtSlurry       = tmps[8] as DataTable;            //슬러리 이송
            _Remark        = tmps[9] as string;               //특이사항

            if (LoginInfo.CFG_GENERAL_PRINTER != null
                && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0
                && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null
                && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
            {
                _SettingPrintName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
            }

            if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
            {
                _paperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Form Load
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Window_Loaded;

            SetControl();
            PrintView();
        }

        /// <summary>
        /// 출력 
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintProcess();
        }

        /// <summary>
        /// 닫기 
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// SizeChanged(
        /// </summary>
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();
        }
        #endregion

        #region Method


        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintView()
        {
            try
            {
                string ReportNamePath = "LGC.GMES.MES.CMM001.Report.Mix_JOB_SHEET.xml";
                string ReportName = "Mix_JOB_SHEET";

                cr = new C1.C1Report.C1Report();
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream(ReportNamePath))
                {
                    cr.Load(stream, ReportName);

                    // 다국어 처리 및 Clear
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        // Title
                        if (cr.Fields[cnt].Name.IndexOf("txTitle", StringComparison.Ordinal) > -1)
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }
                    }
              
                    // 배치정보 Data Binding
                    int Doc_index = 1;
                    for (int row = 0; row < dtDocument.Rows.Count; row++)
                    {
                        dtDocument.Rows[row]["OUT_QTY"] = String.Format(nfi,"{0:#,##0.000}", Convert.ToDouble(dtDocument.Rows[row]["OUT_QTY"], CultureInfo.InvariantCulture));
                        dtDocument.Rows[row]["WIPDTTM_ED"] = string.IsNullOrEmpty(dtDocument.Rows[row]["WIPDTTM_ED"].ToString()) ? (object)DBNull.Value :String.Format("{0:yyyy-MM-dd hh:mm}", Convert.ToDateTime(dtDocument.Rows[row]["WIPDTTM_ED"]));
                    
                    }

                    for (int row = 0; row < dtDocument.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtDocument.Columns.Count; col++)
                        {
                            cr.Fields["DOC_ITEM_" + Doc_index].Text = dtDocument.Rows[row][col].ToString();
                            Doc_index = Doc_index + 1;
                        }
                    }
                    //원재료계량 Data Binding
                    int Mtrl_index = 1;
                    string Chk_WipStat = string.Empty;
                    DataTable dtMtrlGrd_Print = new DataTable();
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE07", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE08", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE09", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE10", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE11", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE12", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE13", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE14", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE15", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE16", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE17", typeof(string));
                    dtMtrlGrd_Print.Columns.Add("RPT_ITEM_VALUE18", typeof(string));
                    for(int i=0; i< dtMtrlGrd.Rows.Count; i++)
                    {
                            DataRow drMtrlGrd = dtMtrlGrd_Print.NewRow();
                            drMtrlGrd["RPT_ITEM_VALUE02"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE02"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE03"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE03"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE04"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE04"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE05"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE05"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE05"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE06"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE06"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE07"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE07"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE07"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE08"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE08"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE08"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE09"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE09"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE09"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE10"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE10"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE11"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE11"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE12"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE12"].ToString();
                            drMtrlGrd["RPT_ITEM_VALUE13"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE13"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE13"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE14"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE14"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE14"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE15"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE15"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE15"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE16"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE16"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE16"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE17"] = string.IsNullOrEmpty(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE17"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE17"], CultureInfo.InvariantCulture));
                            drMtrlGrd["RPT_ITEM_VALUE18"] = dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE18"].ToString();
                            dtMtrlGrd_Print.Rows.Add(drMtrlGrd);
                      
                           if (dtMtrlGrd.Rows[i]["RPT_ITEM_VALUE02"].ToString() == ObjectDic.Instance.GetObjectName("수동투입"))
                           {
                            Chk_WipStat = "Y";
                           }
                         
                    }
                    
                    if(Chk_WipStat == "Y")  //재공상태가 PROC 인지 아닌지..  PROC이면 수동투입이 없음
                    {
                        for (int row = 0; row < dtMtrlGrd_Print.Rows.Count - 2; row++)
                        {
                            for (int col = 0; col < dtMtrlGrd_Print.Columns.Count; col++)
                            {
                                cr.Fields["RAW_ITEM_" + Mtrl_index].Text = dtMtrlGrd_Print.Rows[row][col].ToString();
                                Mtrl_index = Mtrl_index + 1;
                            }
                        }

                        cr.Fields["RAW_ITEM_171"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 2]["RPT_ITEM_VALUE02"].ToString();
                        cr.Fields["RAW_ITEM_177"].Text = string.IsNullOrEmpty(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 2]["RPT_ITEM_VALUE08"].ToString()) ? null : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 2]["RPT_ITEM_VALUE08"], CultureInfo.InvariantCulture));
                        cr.Fields["RAW_ITEM_188"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE02"].ToString();
                        cr.Fields["RAW_ITEM_192"].Text = string.IsNullOrEmpty(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE06"].ToString()) ? null : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));
                        cr.Fields["RAW_ITEM_194"].Text = string.IsNullOrEmpty(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE08"].ToString()) ? null : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE08"], CultureInfo.InvariantCulture));
                        cr.Fields["RAW_ITEM_198"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE12"].ToString();
                        cr.Fields["RAW_ITEM_204"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE18"].ToString();
                    }
                   else
                    {
                        for (int row = 0; row < dtMtrlGrd_Print.Rows.Count - 1; row++)
                        {
                            for (int col = 0; col < dtMtrlGrd_Print.Columns.Count; col++)
                            {
                                cr.Fields["RAW_ITEM_" + Mtrl_index].Text = dtMtrlGrd_Print.Rows[row][col].ToString();
                                Mtrl_index = Mtrl_index + 1;
                            }
                        }

                        cr.Fields["RAW_ITEM_188"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE02"].ToString();
                        cr.Fields["RAW_ITEM_192"].Text = string.IsNullOrEmpty(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE06"].ToString()) ? null : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));
                        cr.Fields["RAW_ITEM_194"].Text = string.IsNullOrEmpty(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE08"].ToString()) ? null : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtMtrlGrd.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE08"], CultureInfo.InvariantCulture));
                        cr.Fields["RAW_ITEM_198"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE12"].ToString();
                        cr.Fields["RAW_ITEM_204"].Text = dtMtrlGrd_Print.Rows[dtMtrlGrd_Print.Rows.Count - 1]["RPT_ITEM_VALUE18"].ToString();
                    }
       

                    //믹싱 Data Binding
                    int Mix_index = 1;
                    DataTable dtMix_Print = new DataTable();
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE07", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE08", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE09", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE10", typeof(string));
                    dtMix_Print.Columns.Add("RPT_ITEM_VALUE11", typeof(string));
            
                    for (int i = 0; i < dtMix.Rows.Count; i++)
                    {
                        DataRow drMix = dtMix_Print.NewRow();
                        drMix["RPT_ITEM_VALUE02"] = dtMix.Rows[i]["RPT_ITEM_VALUE02"].ToString();
                        drMix["RPT_ITEM_VALUE03"] =  string.IsNullOrEmpty(dtMix.Rows[i]["RPT_ITEM_VALUE03"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0}",   Convert.ToDouble(dtMix.Rows[i]["RPT_ITEM_VALUE03"], CultureInfo.InvariantCulture));
                        drMix["RPT_ITEM_VALUE04"] =  string.IsNullOrEmpty(dtMix.Rows[i]["RPT_ITEM_VALUE04"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMix.Rows[i]["RPT_ITEM_VALUE04"], CultureInfo.InvariantCulture));
                        drMix["RPT_ITEM_VALUE05"] =  string.IsNullOrEmpty(dtMix.Rows[i]["RPT_ITEM_VALUE05"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0}",   Convert.ToDouble(dtMix.Rows[i]["RPT_ITEM_VALUE05"], CultureInfo.InvariantCulture));
                        drMix["RPT_ITEM_VALUE06"] =  string.IsNullOrEmpty(dtMix.Rows[i]["RPT_ITEM_VALUE06"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMix.Rows[i]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));
                        drMix["RPT_ITEM_VALUE07"] =  string.IsNullOrEmpty(dtMix.Rows[i]["RPT_ITEM_VALUE07"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.0}", Convert.ToDouble(dtMix.Rows[i]["RPT_ITEM_VALUE07"], CultureInfo.InvariantCulture)); 
                        drMix["RPT_ITEM_VALUE08"] = dtMix.Rows[i]["RPT_ITEM_VALUE08"].ToString();
                        drMix["RPT_ITEM_VALUE09"] = dtMix.Rows[i]["RPT_ITEM_VALUE09"].ToString();
                        drMix["RPT_ITEM_VALUE10"] = dtMix.Rows[i]["RPT_ITEM_VALUE10"].ToString();
                        drMix["RPT_ITEM_VALUE11"] = dtMix.Rows[i]["RPT_ITEM_VALUE11"].ToString();
                    
                        dtMix_Print.Rows.Add(drMix);
                    }
                    for (int row = 0; row < dtMix_Print.Rows.Count - 1; row++)
                    {
                        for (int col = 0; col < dtMix_Print.Columns.Count; col++)
                        {
                            cr.Fields["MIX_ITEM_" + Mix_index].Text = dtMix_Print.Rows[row][col].ToString();
                            Mix_index = Mix_index + 1;
                        }
                    }
                    cr.Fields["MIX_ITEM_83"].Text = dtMix_Print.Rows[dtMix_Print.Rows.Count - 1]["RPT_ITEM_VALUE02"].ToString();
                    cr.Fields["MIX_ITEM_81"].Text = dtMix_Print.Rows[dtMix_Print.Rows.Count - 1]["RPT_ITEM_VALUE10"].ToString();
                    cr.Fields["MIX_ITEM_82"].Text = dtMix_Print.Rows[dtMix_Print.Rows.Count - 1]["RPT_ITEM_VALUE11"].ToString();
                  
                    //자주검사 Data Binding
                    int Self_index = 1;

                    DataTable dtSelfInsp_Print = new DataTable();
                    dtSelfInsp_Print.Columns.Add("CLSS_NAME1", typeof(string));
                    dtSelfInsp_Print.Columns.Add("CLSS_NAME2", typeof(string));
                    dtSelfInsp_Print.Columns.Add("UNIT_CODE", typeof(string));
                    dtSelfInsp_Print.Columns.Add("USL", typeof(string));
                    dtSelfInsp_Print.Columns.Add("LSL", typeof(string));
                    dtSelfInsp_Print.Columns.Add("CLCTVAL01", typeof(string));
             
                    for (int i = 0; i < dtSelfInsp.Rows.Count; i++)
                    {
                        DataRow drSelfInsp = dtSelfInsp_Print.NewRow();
                        drSelfInsp["CLSS_NAME1"] = dtSelfInsp.Rows[i]["CLSS_NAME1"].ToString();
                        drSelfInsp["CLSS_NAME2"] = dtSelfInsp.Rows[i]["CLSS_NAME2"].ToString();
                        drSelfInsp["UNIT_CODE"] = dtSelfInsp.Rows[i]["UNIT_CODE"].ToString();
                        drSelfInsp["USL"] = dtSelfInsp.Rows[i]["USL"].ToString();
                        drSelfInsp["LSL"] = dtSelfInsp.Rows[i]["LSL"].ToString();
                        drSelfInsp["CLCTVAL01"] = string.IsNullOrEmpty(dtSelfInsp.Rows[i]["CLCTVAL01"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.###}", Convert.ToDouble(dtSelfInsp.Rows[i]["CLCTVAL01"], CultureInfo.InvariantCulture));  
                  
                        dtSelfInsp_Print.Rows.Add(drSelfInsp);
                    }
                    for (int row = 0; row < dtSelfInsp_Print.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtSelfInsp_Print.Columns.Count; col++)
                        {
                            cr.Fields["SELF_ITEM_" + Self_index].Text = dtSelfInsp_Print.Rows[row][col].ToString();
                            Self_index = Self_index + 1;
                        }
                    }
               
                    //체크스케일 Data Binding
                    int Chk_index = 1;
                    DataTable dtChkScale_Print = new DataTable();
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                    //dtChkScale_Print.Columns.Add("RPT_ITEM_VALUE07", typeof(string));
              
                    for (int i = 0; i < dtChkScale.Rows.Count; i++)
                    {
                        DataRow drChkScale = dtChkScale_Print.NewRow();
                        drChkScale["RPT_ITEM_VALUE01"] = dtChkScale.Rows[i]["RPT_ITEM_VALUE01"].ToString();
                        drChkScale["RPT_ITEM_VALUE02"] = dtChkScale.Rows[i]["RPT_ITEM_VALUE02"].ToString();
                        drChkScale["RPT_ITEM_VALUE03"] = string.IsNullOrEmpty(dtChkScale.Rows[i]["RPT_ITEM_VALUE03"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtChkScale.Rows[i]["RPT_ITEM_VALUE03"], CultureInfo.InvariantCulture));
                        drChkScale["RPT_ITEM_VALUE04"] = string.IsNullOrEmpty(dtChkScale.Rows[i]["RPT_ITEM_VALUE04"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:#,##0.000}", Convert.ToDouble(dtChkScale.Rows[i]["RPT_ITEM_VALUE04"], CultureInfo.InvariantCulture)); 
                        drChkScale["RPT_ITEM_VALUE05"] = dtChkScale.Rows[i]["RPT_ITEM_VALUE05"].ToString();
                        drChkScale["RPT_ITEM_VALUE06"] = dtChkScale.Rows[i]["RPT_ITEM_VALUE06"].ToString();
                        //drChkScale["RPT_ITEM_VALUE07"] = dtChkScale.Rows[i]["RPT_ITEM_VALUE07"].ToString();
                        dtChkScale_Print.Rows.Add(drChkScale);
                    }
                    for (int row = 0; row < dtChkScale_Print.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtChkScale_Print.Columns.Count; col++)
                        {
                            cr.Fields["CHK_ITEM_" + Chk_index].Text = dtChkScale_Print.Rows[row][col].ToString();
                            Chk_index = Chk_index + 1;
                        }
                    }
                   //총투입량 Data Binding
                    int All_index = 1;
                    for (int row = 0; row < dtInputQty.Rows.Count; row++)
                    {
                        dtInputQty.Rows[row]["INPUT_QTY"] = string.IsNullOrEmpty(dtInputQty.Rows[row]["INPUT_QTY"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0}", Convert.ToDouble(dtInputQty.Rows[row]["INPUT_QTY"], CultureInfo.InvariantCulture));
                    }
                   for (int row = 0; row < dtInputQty.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtInputQty.Columns.Count; col++)
                        {
                            cr.Fields["ALL_ITEM_" + All_index].Text = dtInputQty.Rows[row][col].ToString();
                            All_index = All_index + 1;
                        }
                    }
                   //원재료 준비 Data Binding
                    int Ready_index = 1;

                    DataTable dtMtrlGrdReady_Print = new DataTable();
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                    dtMtrlGrdReady_Print.Columns.Add("RPT_ITEM_VALUE07", typeof(string));

                    for (int i = 0; i < dtMtrlGrdReady.Rows.Count; i++)
                    {
                        DataRow drMtrlGrdReady = dtMtrlGrdReady_Print.NewRow();
                        drMtrlGrdReady["RPT_ITEM_VALUE01"] = dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE01"].ToString();
                        drMtrlGrdReady["RPT_ITEM_VALUE02"] = dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE02"].ToString();
                        drMtrlGrdReady["RPT_ITEM_VALUE03"] = string.IsNullOrEmpty(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE03"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,###}", Convert.ToDouble(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE03"], CultureInfo.InvariantCulture)); 
                        drMtrlGrdReady["RPT_ITEM_VALUE04"] = string.IsNullOrEmpty(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE04"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,###}", Convert.ToDouble(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE04"], CultureInfo.InvariantCulture)); 
                        drMtrlGrdReady["RPT_ITEM_VALUE05"] = string.IsNullOrEmpty(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE05"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,###}", Convert.ToDouble(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE05"], CultureInfo.InvariantCulture)); 
                        drMtrlGrdReady["RPT_ITEM_VALUE06"] = string.IsNullOrEmpty(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE06"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,###}", Convert.ToDouble(dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture)); 
                        drMtrlGrdReady["RPT_ITEM_VALUE07"] = dtMtrlGrdReady.Rows[i]["RPT_ITEM_VALUE07"].ToString();
                        dtMtrlGrdReady_Print.Rows.Add(drMtrlGrdReady);
                    }
                    for (int row = 0; row < dtMtrlGrdReady_Print.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtMtrlGrdReady_Print.Columns.Count; col++)
                        {
                            cr.Fields["READY_ITEM_" + Ready_index].Text = dtMtrlGrdReady_Print.Rows[row][col].ToString();
                            Ready_index = Ready_index + 1;
                        }
                    }
            
                    //체크스케일 배출 Data Binding
                    int Out_index = 1;

                    DataTable dtChkScaleOut_Print = new DataTable();
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE07", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE08", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE09", typeof(string));
                    dtChkScaleOut_Print.Columns.Add("RPT_ITEM_VALUE10", typeof(string));

                    for (int i = 0; i < dtChkScaleOut.Rows.Count; i++)
                    {
                        DataRow drChkScaleOut = dtChkScaleOut_Print.NewRow();
                        drChkScaleOut["RPT_ITEM_VALUE01"] = dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE01"].ToString();
                        drChkScaleOut["RPT_ITEM_VALUE02"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE02"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE02"], CultureInfo.InvariantCulture));  
                        drChkScaleOut["RPT_ITEM_VALUE03"] = dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE03"].ToString();
                        drChkScaleOut["RPT_ITEM_VALUE04"] = dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE04"].ToString();
                        drChkScaleOut["RPT_ITEM_VALUE05"] = dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE05"].ToString();
                        drChkScaleOut["RPT_ITEM_VALUE06"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE06"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));  
                        drChkScaleOut["RPT_ITEM_VALUE07"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE07"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE07"], CultureInfo.InvariantCulture));  
                        drChkScaleOut["RPT_ITEM_VALUE08"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE08"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE08"], CultureInfo.InvariantCulture));  
                        drChkScaleOut["RPT_ITEM_VALUE09"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE09"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE09"], CultureInfo.InvariantCulture));  
                        drChkScaleOut["RPT_ITEM_VALUE10"] = string.IsNullOrEmpty(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE10"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.000}", Convert.ToDouble(dtChkScaleOut.Rows[i]["RPT_ITEM_VALUE10"], CultureInfo.InvariantCulture));  
                        dtChkScaleOut_Print.Rows.Add(drChkScaleOut);
                    }
                    for (int row = 0; row < dtChkScaleOut_Print.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtChkScaleOut_Print.Columns.Count; col++)
                        {
                            cr.Fields["OUT_ITEM_" + Out_index].Text = dtChkScaleOut_Print.Rows[row][col].ToString();
                            Out_index = Out_index + 1;
                        }
                    }
              
                    //Slurry 이송  Data Binding
                    int Move_index = 1;

                    DataTable dtSlurry_Print = new DataTable();
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE01", typeof(string));
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE02", typeof(string));
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE03", typeof(string));
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE04", typeof(string));
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE05", typeof(string));
                    dtSlurry_Print.Columns.Add("RPT_ITEM_VALUE06", typeof(string));
               
                    for (int i = 0; i < dtSlurry.Rows.Count; i++)
                    {
                        DataRow drSlurry = dtSlurry_Print.NewRow();
                        drSlurry["RPT_ITEM_VALUE01"] = dtSlurry.Rows[i]["RPT_ITEM_VALUE01"].ToString();
                        drSlurry["RPT_ITEM_VALUE02"] = dtSlurry.Rows[i]["RPT_ITEM_VALUE02"].ToString();
                        drSlurry["RPT_ITEM_VALUE03"] = dtSlurry.Rows[i]["RPT_ITEM_VALUE03"].ToString();
                        drSlurry["RPT_ITEM_VALUE04"] = dtSlurry.Rows[i]["RPT_ITEM_VALUE04"].ToString();
                        drSlurry["RPT_ITEM_VALUE05"] = string.IsNullOrEmpty(dtSlurry.Rows[i]["RPT_ITEM_VALUE05"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.0}", Convert.ToDouble(dtSlurry.Rows[i]["RPT_ITEM_VALUE05"], CultureInfo.InvariantCulture)); 
                        drSlurry["RPT_ITEM_VALUE06"] = string.IsNullOrEmpty(dtSlurry.Rows[i]["RPT_ITEM_VALUE06"].ToString()) ? (object)DBNull.Value : String.Format(nfi, "{0:###,###,##0.0}", Convert.ToDouble(dtSlurry.Rows[i]["RPT_ITEM_VALUE06"], CultureInfo.InvariantCulture));          
                        dtSlurry_Print.Rows.Add(drSlurry);
                    }
                    for (int row = 0; row < dtSlurry_Print.Rows.Count; row++)
                    {
                        for (int col = 0; col < dtSlurry_Print.Columns.Count; col++)
                        {
                            if (dtSlurry_Print.Rows[row][col].ToString() != string.Empty)
                            {
                                cr.Fields["MOVE_ITEM_" + Move_index].Text = dtSlurry_Print.Rows[row][col].ToString();
                                Move_index = Move_index + 1;
                            }
                       }
                    }

                   //특이사항
                    cr.Fields["RMK_ITEM_1"].Text = _Remark;
                }
                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                c1DocumentViewer.FitToWidth();
                c1DocumentViewer.FitToHeight();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 출력
        /// </summary>
        private void PrintProcess()
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            pm.Print(ps, ps.DefaultPageSettings);
        }


        #endregion



    }
}
