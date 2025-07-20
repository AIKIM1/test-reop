/*************************************************************************************
 Created Date : 2021-11-23
      Creator : 
   Decription : Pack 물류포장 포장정보 레포트 발행
--------------------------------------------------------------------------------------
 [Change History]
  2021-11-23  김길용 : Initial Created.
  2022.01.19  김길용 :  팩3동 다국어 대응을 위한 수정
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
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pallet_CST_Tag : Window, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        string tmp = string.Empty;
        object[] tmps;
        string[] reports_name;
        string window_name = string.Empty;
        string palletID = string.Empty;
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

        public Pallet_CST_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);
            window_name = tmps[0] as string;
            palletID = tmps[1] as string; //palletID

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

        private void setReport(string gubun = "CMAEV2020")
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
                case "CMAEV2020":
                    param_c1DocumentViewer = c1DocumentViewer_CMAEV2020;
                    break;
                case "CMA_BASIC":
                    param_c1DocumentViewer = c1DocumentViewer_CMA_BASIC;
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
                    for (int fields_idx = 0; fields_idx < cr.Fields.Count; fields_idx++)
                    {
                        if (cr.Fields[fields_idx].Text != null)
                        {
                            switch (gubun)
                            {
                                case "CMAEV2020":
                                    if (cr.Fields[fields_idx].Name == null || cr.Fields[fields_idx].Name == "1" || cr.Fields[fields_idx].Name == "11" ||
                                        cr.Fields[fields_idx].Name == "11" || cr.Fields[fields_idx].Name == "12" || cr.Fields[fields_idx].Name == "13" ||
                                        cr.Fields[fields_idx].Name == "14" || cr.Fields[fields_idx].Name == "15" || cr.Fields[fields_idx].Name == "16" ||
                                        cr.Fields[fields_idx].Name == "18" || cr.Fields[fields_idx].Name == "19" || cr.Fields[fields_idx].Name == "111" ||
                                        cr.Fields[fields_idx].Name == "112" || cr.Fields[fields_idx].Name == "114" || cr.Fields[fields_idx].Name == "115" ||
                                        cr.Fields[fields_idx].Name == "118" || cr.Fields[fields_idx].Name == "17" || cr.Fields[fields_idx].Name == "110" || cr.Fields[fields_idx].Name == "124" || cr.Fields[fields_idx].Name == "125"
                                        )
                                    {
                                        if (cr.Fields[fields_idx].Name == "1")
                                        {
                                            string[] temp = cr.Fields[fields_idx].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                                            if (temp.Length <= 1)
                                            {
                                                cr.Fields[fields_idx].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                cr.Fields[fields_idx].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            cr.Fields[fields_idx].Text = ObjectDic.Instance.GetObjectName(cr.Fields[fields_idx].Text);
                                        }
                                    }
                                    break;
                            }
                        }
                        cr.Fields[fields_idx].Font.Name = "돋움체";
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
                DataSet dsIndata = new DataSet();
                DataTable dtIndata = new DataTable("INDATA");
                dtIndata.Columns.Add("PALLETID", typeof(string));
                dtIndata.Columns.Add("PLT_TYPE", typeof(string));
                dtIndata.Rows.Add(new object[] { palletID, reportname });
                dsIndata.Tables.Add(dtIndata);

                dtPltData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PLT_TAG_V2", "INDATA", "OUTDATA", dsIndata);
                dtPallet_history = dtPltData.Tables.Contains("OUTDATA") ? dtPltData.Tables["OUTDATA"] : null;
                //reportname = string.IsNullOrWhiteSpace(reportname) ? "CELLCST" : reportname;

                //switch (reportname)
                //{
                //    case "CELLCST":
                //        dtPallet_history = dtPltData.Rows.Count > 0 ? dtPltData : null;
                //        break;
                //}
                #endregion
                SetReportData();
                
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SetReportData()
        {
            #region Reportname 별 Binding Data Setting
            dtBindData = setBindData_CMAEV2020();
            #endregion
        }

        #region setBinData
        #region setBindData_CELLCST
        private DataTable setBindData_CMAEV2020()
        {
            try
            {
                dtBindData = null;
                int boxcnt = dtPallet_history.Rows.Count;
                int index = 0;
                string boxid = "ITEM";

                dtBindData = new DataTable();
                dtBindData.TableName = "INDATA";
                for (int i=1; i<51; i++)
                {
                    dtBindData.Columns.Add(boxid + i.ToString(), typeof(string));
                }
                //for (int j = 1; j <= boxcnt; j++)
                //{
                //    dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                //}

                DataRow drBindData = dtBindData.NewRow();
                for(int k=1; k< dtBindData.Columns.Count; k++)
                {
                    //if (!string.IsNullOrEmpty(dtPallet_history.Rows[0][boxid + k.ToString()].ToString()))
                    //{
                    if (k > 20 && index == 0)
                    {
                        for (int f = 1; f < dtPallet_history.Rows.Count; f++)
                        {
                            index = 1;
                            drBindData[boxid + k.ToString()] = (dtPallet_history.Rows[f]).ItemArray[19].ToString();
                            k++;
                        }
                    }else
                    {
                        if (index != 1)
                            drBindData[boxid + k.ToString()] = string.IsNullOrEmpty(dtPallet_history.Rows[0][boxid + k.ToString()].ToString()) ? null : dtPallet_history.Rows[0][boxid + k.ToString()].ToString();
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
                case "CMAEV2020":
                    seleted_c1DocumentViewer = c1DocumentViewer_CMAEV2020;
                    break;
                case "CMA_BASIC":
                    seleted_c1DocumentViewer = c1DocumentViewer_CMA_BASIC;
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
                reportname = dtPltData.Tables.Contains(reportname.Trim().ToUpper()) ? reportname.Trim().ToUpper() : "CMAEV2020";
                dtPallet_history = dtPltData.Tables[reportname];
                setDataTable();
                //SetReportData();
                setReport(gubun);
            }
        }
    }
}
