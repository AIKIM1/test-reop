/*************************************************************************************
 Created Date : 2020-11-30
      Creator : 
   Decription : Pack 물류포장 정보 레포트 발행
--------------------------------------------------------------------------------------
 [Change History]
  2020-11-30  김길용 : Initial Created.
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using QRCoder;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Printing;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LOGIS_CST_Tag : Window, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        string tmp = string.Empty;
        object[] tmps;
        string[] reports_name;
        string window_name = string.Empty;
        string bPID = string.Empty;
        C1.WPF.C1Report.C1DocumentViewer param_c1DocumentViewer; //전달 받은 내용
        C1.WPF.C1Report.C1DocumentViewer seleted_c1DocumentViewer; //선택한 tab 내용
        string eqsgid = string.Empty;
        string model = string.Empty;
        //임시 DPI
        string strDpi = string.Empty;

        DataSet dtPltData;
        DataTable dtPallet_history;
        DataTable dtBindData;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public LOGIS_CST_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            reports_name = new string[] { "CELLCST" };

            tmps = C1WindowExtension.GetParameters(this);
            window_name = tmps[0] as string;
            bPID = tmps[1] as string; //cstID
            //eqsgid = tmps[2] as string; //eqsgid

            this.Loaded -= Window_Loaded;

            setDataTable();
            setReport();

            //cr.Render();
            //http://our.componentone.com/groups/topic/need-help-getting-c1reports-to-work/
            //임시 DPI
            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
            {
                if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                {

                    strDpi = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();

                }
            }
        }

        private void setReport(string gubun = "CELLCST")
        {
            C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

            string filename = string.Empty;
            string reportname = string.Empty;

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            param_c1DocumentViewer = new C1.WPF.C1Report.C1DocumentViewer();

            reportname = "Pallet_Tag_" + gubun;


            if (dtBindData == null || dtBindData.Rows.Count == 0)
            {
                return;
            }

            switch (gubun)
            {
                case "CELLCST":
                    param_c1DocumentViewer = c1DocumentViewer_CELLCST;
                    if (Convert.ToInt32(dtPallet_history.Rows.Count) > 390)
                    {
                        reportname = "Pallet_Tag_" + gubun + "_ADDROW";
                    }
                    break;
            }

            filename = reportname + ".xml";

            #region Report 별 Data Binding
            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + filename))
            {
                if (stream != null)
                {
                    cr.Load(stream, reportname);

                    //레포트에 Value Binding
                    for (int col = 0; col < dtBindData.Columns.Count; col++)
                    {
                        string strColName = dtBindData.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName))
                        {
                            if (strColName.Contains("EOLDATABCR"))
                            {
                                System.Drawing.ImageConverter imgcvt = new System.Drawing.ImageConverter();
                                Image img = (Image)imgcvt.ConvertFrom(dtBindData.Rows[0][strColName]);

                                cr.Fields[strColName].Picture = img == null ? (Image)imgcvt.ConvertFrom("") : img;
                            }
                            else
                            {
                                cr.Fields[strColName].Text = dtBindData.Rows[0][strColName] == null ? "" : dtBindData.Rows[0][strColName].ToString();
                            }
                        }
                    }

                    //Language Binding : 다국어 처리
                    for (int fields_iex = 0; fields_iex < cr.Fields.Count; fields_iex++)
                    {
                        if (cr.Fields[fields_iex].Text != null)
                        {
                            switch (gubun)
                            {
                                case "CELLCST":
                                    if (cr.Fields[fields_iex].Name == "TITLE" || cr.Fields[fields_iex].Name == null || cr.Fields[fields_iex].Name == "21" || cr.Fields[fields_iex].Name == "22" ||
                                        cr.Fields[fields_iex].Name == "23" || cr.Fields[fields_iex].Name == "24")
                                    {
                                        if (cr.Fields[fields_iex].Name == "TITLE")
                                        {
                                            cr.Fields[fields_iex].Text = "CARRIER-PALLET CELL Packing Sheet";
                                        }
                                        if (cr.Fields[fields_iex].Name == "1")
                                        {
                                            string[] temp = cr.Fields[fields_iex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                                            if (temp.Length <= 1)
                                            {
                                                cr.Fields[fields_iex].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                cr.Fields[fields_iex].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            cr.Fields[fields_iex].Text = ObjectDic.Instance.GetObjectName(cr.Fields[fields_iex].Text);
                                        }

                                    }
                                    break;
                            }
                            //cr.Fields[fields_iex].Text = ObjectDic.Instance.GetObjectName(cr.Fields[fields_iex].Text);
                        }

                        cr.Fields[fields_iex].Font.Name = "돋움체";
                    }

                    setDocumentView(param_c1DocumentViewer, cr);
                }
            }
            #endregion
        //}
        }

        private void setDataTable(string reportname = null)
        {
            try
            {
                #region Reportname 별 DATA 받아오기
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = bPID;
                RQSTDT.Rows.Add(dr);

                DataTable dtPltData = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CARRIER_COUNT_HISTORY", "RQSTDT", "RSLTDT", RQSTDT);
                
                reportname = string.IsNullOrWhiteSpace(reportname) ? "CELLCST" : reportname;

                switch (reportname)
                {
                    case "CELLCST":
                        dtPallet_history = dtPltData.Rows.Count > 0 ? dtPltData : null;
                        break;
                }

                #region 이전 개별 호출 소스
                /* 이전 개별 호출 소스
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));                

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;                        

                RQSTDT.Rows.Add(dr);

                switch (reportname)
                {
                    case "CMA":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY", "INDATA", "OUTDATA", RQSTDT);
                        //if (dtPallet_history.Rows.Count > 20)
                        //{
                        //    Util.AlertInfo("출력 DATA BOX 수량 제한 : 20개까지만 출력됩니다.\n                        현재 BOX수량(" + dtPallet_history.Rows.Count.ToString() + ")"); //
                        //    return;
                        //}
                        break;

                    case "X09CMA":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        //if (dtPallet_history.Rows.Count > 63)
                        //{
                        //    Util.AlertInfo("출력 DATA LOT 수량 제한 : 63개까지만 출력됩니다.\n                        현재 LOT수량(" + dtPallet_history.Rows.Count.ToString() + ")"); //
                        //    return;
                        //}
                        break;

                    case "YFCMA":
                        //2019.02.26
                        //dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    case "B10CMA":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        //if (dtPallet_history.Rows.Count > 10)
                        //{
                        //    Util.AlertInfo("출력 DATA LOT 수량 제한 : 24개까지만 출력됩니다.\n                        현재 LOT수량(" + dtPallet_history.Rows.Count.ToString() + ")"); //
                        //    return;
                        //}
                        break;

                    case "313HBMA":
                        //2019.02.26
                        //dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    case "315HCMA":
                        //2019.02.26
                        //dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    case "PL65":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    //2018.04.18
                    //case "BMW_PORCHE":
                    case "Porsche12V":
                        //2018.09.27
                        //dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_X09CMA", "INDATA", "OUTDATA", RQSTDT);
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_PORSCHE", "INDATA", "OUTDATA", RQSTDT);

                        model = dtPallet_history.Rows[0]["MODEL"].ToString();
                        break;

                    //2018.04.18
                    case "BMW12V":
                        //2018.11.08
                        //dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY", "INDATA", "OUTDATA", RQSTDT);
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_BMW", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    //2019.02.26
                    case "Ford48V":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_FORD48V", "INDATA", "OUTDATA", RQSTDT);
                        break;

                    //2020.05.27 KIM MIN SEOK
                    case "C727EOL":
                        dtPallet_history = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_COUNT_HISTORY_C727EOL", "INDATA", "OUTDATA", RQSTDT);
                        break;
                }
                */
                #endregion
                #endregion

                SetReportData();

                #region Reportname 별 Binding Data Setting
                /*
                if (dtPallet_history == null || dtPallet_history.Rows.Count == 0)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("선택한 PALLETID가 없거나 포장해제된 PALLET입니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                   ms.AlertWarning("SFU3637");//선택한 PALLETID가 없거나 포장해제된 PALLET입니다.
                    return ;
                }
                else
                {
                    switch (reportname)
                    {
                        case "CMA":
                            dtBindData = setBindData_CMA();
                            break;

                        case "X09CMA": //MOKA
                            //2019.12.05
                            //if(eqsgid == "P1Q02")
                            if (eqsgid == "P2Q02")
                            {
                                dtBindData = setBindData_X09CMA();
                            }
                            break;

                        case "YFCMA":
                            //dtBindData = setBindData_YFCMA();
                            break;

                        case "B10CMA":
                           if(eqsgid == "P2Q04")
                            {
                                dtBindData = setBindData_B10CMA();
                            }                            
                            break;

                        case "313HBMA":
                            if(eqsgid == "P6Q07")
                            {
                                dtBindData = setBindData_313HBMA();
                            }                            
                            break;

                        case "315HCMA":
                            //dtBindData = setBindData_315HCMA();
                            break;

                        case "PL65":
                            if(eqsgid == "P6Q09")
                            {
                                //dtBindData = setBindData_PL65();
                            }                          
                            break;

                        //2018.04.18
                        //case "BMW_PORCHE":
                        case "Porsche12V":
                            if (eqsgid == "P2Q11")
                            {
                                dtBindData = setBindData_BMW_PORCHE();
                            }
                            break;

                        //2018.04.18
                        case "BMW12V":
                            if (eqsgid == "P2Q11")
                            {
                                dtBindData = setBindData_BMW12V();
                            }
                            break;

                        //2019.02.26
                        case "Ford48V":
                            if (eqsgid == "P6Q12")
                            {
                                dtBindData = setBindData_Ford48V();
                            }
                            break;

                        case "C727EOL":
                            if(eqsgid == "P8Q21" || eqsgid == "P8Q30" || eqsgid == "P8Q31")
                            {
                                dtBindData = setBindData_C727EOL();
                            }
                            break;
                    }
                }
                */
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SetReportData(string reportname = "CELLCST")
        {
            #region Reportname 별 Binding Data Setting
            if (dtPallet_history == null || dtPallet_history.Rows.Count == 0)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show("선택한 PALLETID가 없거나 포장해제된 PALLET입니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                ms.AlertWarning("SFU3637");//선택한 PALLETID가 없거나 포장해제된 PALLET입니다.
                return;
            }
            else
            {
                switch (reportname)
                {
                    case "CELLCST":
                        dtBindData = setBindData_CELLCST();
                        break;

                }
            }
            #endregion
        }

        #region setBinData
        #region setBindData_CELLCST
        private DataTable setBindData_CELLCST()
        {
            try
            {
                dtBindData = null;
                int boxcnt = dtPallet_history.Rows.Count;
                int index = 2;
                string boxid = "LOTID";



                dtBindData = new DataTable();
                dtBindData.TableName = "RQSTDT";
                dtBindData.Columns.Add("BARCODE", typeof(string));
                dtBindData.Columns.Add("CSTID", typeof(string));
                dtBindData.Columns.Add("CSTID1", typeof(string));
                dtBindData.Columns.Add("BOXID", typeof(string));
                dtBindData.Columns.Add("BOXID1", typeof(string));                
                dtBindData.Columns.Add("DATE", typeof(string)); //Pallet 구성 일자
                dtBindData.Columns.Add("BOXCNT", typeof(string)); //Pallet 구성 Cell 갯수
                dtBindData.Columns.Add("USER", typeof(string));
                dtBindData.Columns.Add("PRODID", typeof(string));

                for (int j = 1; j <= boxcnt; j++)
                {
                    dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));

                }

                DataRow drBindData = dtBindData.NewRow();
                drBindData["BARCODE"] = dtPallet_history.Rows[0]["BOXID"].ToString();
                drBindData["CSTID"] = dtPallet_history.Rows[0]["CSTID"].ToString();
                drBindData["CSTID1"] = dtPallet_history.Rows[0]["CSTID"].ToString();
                drBindData["BOXID"] = dtPallet_history.Rows[0]["BOXID"].ToString();
                drBindData["BOXID1"] = dtPallet_history.Rows[0]["BOXID"].ToString();
                drBindData["DATE"] = dtPallet_history.Rows[0]["DATE"].ToString();
                drBindData["BOXCNT"] = dtPallet_history.Rows.Count;
                drBindData["USER"] = dtPallet_history.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = dtPallet_history.Rows[0]["PRODID"].ToString();

                drBindData["LOTID1"] = dtPallet_history.Rows[0]["LOTID"].ToString();
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[boxid + index.ToString()] = dtPallet_history.Rows[i]["LOTID"].ToString();

                        index++;
                    }
                }

                dtBindData.Rows.Add(drBindData);

                return dtBindData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }
        }
        #endregion
        #endregion

        private byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);

            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(100, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
         }

        private void setDocumentView(C1.WPF.C1Report.C1DocumentViewer c1DocumentViewer, C1.C1Report.C1Report cr)
        {
            c1DocumentViewer.Document = cr.FixedDocumentSequence;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            C1.WPF.C1TabItem seleted_item = tcMain.SelectedItem as C1.WPF.C1TabItem;

            string item_header = seleted_item.Header.ToString();

            switch (item_header)
            {
                case "CELLCST":
                    seleted_c1DocumentViewer = c1DocumentViewer_CELLCST;
                    break;

            }

            seleted_c1DocumentViewer.Print();

            if (this.DialogResult == null)
            {
                return;
            }

            this.DialogResult = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
            this.Close();
        }

        private void tcMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dtPltData != null)
            {
                string reportname = ((C1.WPF.C1TabItem)tcMain.SelectedItem).Header.ToString();
                string gubun = reportname;
                reportname = dtPltData.Tables.Contains(reportname.Trim().ToUpper()) ? reportname.Trim().ToUpper() : "CELLCST";
                dtPallet_history = dtPltData.Tables[reportname];
                SetReportData(reportname);
                setReport(gubun);
            }
        }
    }
}
