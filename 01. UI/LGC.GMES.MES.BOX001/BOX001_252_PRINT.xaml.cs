/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.04.05  JHM       : C20220329-000427 - Tesla Contents Label 양식 변경
  2023.01.05  김린겸    : C20221212-000579 Tesla label ID quantity check
  2023.01.18  이병윤    : E20230113-000080
  2023.07.07  이병윤    : E20230614-000843 오창 2공장 GMES Tesla 라벨 발행 기능 추가 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QRCoder;
using System.Drawing;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_252_PRINT : C1Window, IWorkArea
    {
        private Dictionary<string, string> _dic = null;
        private String sPage = null;
        private int _PageQty = 1;
        private string _ReportFormat = string.Empty;
        string _siteCode = "";
        C1.C1Report.C1Report[] crList = null;

        public List<string> teslaSeqNoList;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_252_PRINT()
        {
            InitializeComponent();
        }


        public BOX001_252_PRINT(Dictionary<string, string> dicParam, int PrtQty)
        {
            try
            {
                InitializeComponent();

                _dic = dicParam;
                _PageQty = PrtQty;
                //_Page = _List[0]["PAGE"];
                sPage = "Tesla_Report";

                teslaSeqNoList = new List<string>();

                //C1.C1Report.C1Report[] crList = new C1.C1Report.C1Report[(_List.Count + 1) / 2];
                crList = new C1.C1Report.C1Report[PrtQty];

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[crList.Length];

                _ReportFormat = "LGC.GMES.MES.BOX001.Report.Tesla_Report.xml";

                try
                {
                    Get_Seq_No(); //라벨 SEQ 채번 및 DB INSERT

                    if (teslaSeqNoList.Count == 0)
                    {
                        return;
                    }
                }
                catch(Exception ex)
                {
                    Util.MessageValidation("SFU8252"); // 잠시 후 다시 시도해주세요.
                    return;
                }

                using (Stream stream = a.GetManifestResourceStream(_ReportFormat))
                {
                    crList[0] = new C1.C1Report.C1Report();
                    crList[0].Load(stream, sPage); //cr.Load(@"C:\c1report.xml", "test");
                                                   //crList[0].Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    for (int i = 0; i < dicParam.Count; i++)
                    {
                        string key = dicParam.Keys.ToList()[i].ToString();

                        if (key.Equals("QTY_UNIT"))
                            continue;
                        if (key.Equals("SITE_CODE"))
                            continue;

                        crList[0].Fields[key].Text = dicParam[key];

                    }

                    for (int i = 0; i < crList.Length; i++)
                    {
                        if (i == 0)
                            continue;

                        crList[i] = new C1.C1Report.C1Report();
                        crList[i].CopyFrom(crList[0]);
                    }

                    //Save_Label_Hist();
            
                    for (int i = 0; i < crList.Length; i++)
                    {
                        //cr.Fields["CONTENT_LABEL_ID"].Text = "1";

                        crList[i].Fields["CONTENT_LABEL_ID"].Text = teslaSeqNoList[i];

                        crList[i].Fields["QTY"].Text = dicParam["QTY"] + " " + dicParam["QTY_UNIT"];

                        ImageConverter imgcvt = new ImageConverter();
                        System.Drawing.Image img = (System.Drawing.Image)imgcvt.ConvertFrom(GetQRCode(teslaSeqNoList[i]));
                        crList[i].Fields["QRCode"].Picture = img == null ? (System.Drawing.Image)imgcvt.ConvertFrom("") : img;

                        //LOT ID 2D 바코드: 사양서대로 정사이즈 추가 시 위에 거 참조해서 사이즈를 맞춘다.
                        System.Drawing.Image img1 = (System.Drawing.Image)imgcvt.ConvertFrom(GetQRCode1(dicParam["QRcode1"]));
                        crList[i].Fields["QRCode1"].Picture = img1 == null ? (System.Drawing.Image)imgcvt.ConvertFrom("") : img1;

                        crList[i].Render();
                        crList[0].C1Document.Body.Children.Add(new C1.C1Preview.RenderImage(crList[i].GetPageImage(0)));
                        crList[0].C1Document.Reflow();
                    }
                }

                c1DocumentViewer.Document = crList[0].FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);

            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(400, 400, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
        }

        private byte[] GetQRCode1(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);

            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(74, 74, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Pdf Files|*.pdf";
                saveFileDialog.Title = "Save an Pdf File";
                saveFileDialog.FileName = "*";

                if (System.Configuration.ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    saveFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    Logger.Instance.WriteLine("PDF LOCAL PATH", saveFileDialog.FileName);

                    C1.C1Preview.Export.PdfExporter ex = new C1.C1Preview.Export.PdfExporter();
                    //ex.Document = crList[0].Document;
                    ex.Document = crList[0].C1Document;
                    ex.Export(saveFileDialog.FileName);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void Get_Seq_No()
        {
            string bizrule = "";
            /*
             * E20230113-000080 시퀀스 증가 기준 변경
             * 발행일에서 해당기준으로(Print Date/Part Number/Supplier NO)
             */
            if (LoginInfo.CFG_SHOP_ID.Equals("A010") || LoginInfo.CFG_SHOP_ID.Equals("F030")) // 오창 소형조립, 오창2산단 소형조립
            {
                /* E20230113-000080 BR 변경
                 * BR_PRD_GET_TESLA_LABEL_SEQ_NO_FM -> BR_PRD_GET_TESLA_SEQNO_LABEL_FM
                 */
                bizrule = "BR_PRD_GET_TESLA_SEQNO_LABEL_FM";
            }
            else if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                /* E20230113-000080 BR 변경
                 * BR_PRD_GET_TESLA_LABEL_SEQ_NO -> BR_PRD_GET_TESLA_SEQNO_LABEL
                 */
                bizrule = "BR_PRD_GET_TESLA_SEQNO_LABEL";
            }


            DataTable dtRqstDt = new DataTable();
            dtRqstDt.TableName = "RQSTDT";
            dtRqstDt.Columns.Add("PRT_DATE", typeof(string));
            dtRqstDt.Columns.Add("INSUSER", typeof(string));
            dtRqstDt.Columns.Add("PRT_QTY", typeof(int));
            dtRqstDt.Columns.Add("QTY", typeof(int));
            dtRqstDt.Columns.Add("PART_NUM", typeof(string));
            dtRqstDt.Columns.Add("LOT_CODE", typeof(string));
            dtRqstDt.Columns.Add("SITE_CODE", typeof(string));

            DataRow drnewrow = dtRqstDt.NewRow();
            drnewrow["PRT_DATE"] = DateTime.Parse(_dic["PRINT_DATE"]).ToString("yyyyMMdd");
            drnewrow["INSUSER"] = LoginInfo.USERID;
            drnewrow["PRT_QTY"] = _PageQty;
            drnewrow["QTY"] = _dic["QTY"].ToString();
            drnewrow["PART_NUM"] = _dic["PART_NUM"].ToString().Replace("-", "");
            drnewrow["LOT_CODE"] = _dic["LOT_CODE"].ToString();
            drnewrow["SITE_CODE"] = _dic["SITE_CODE"].ToString();

            dtRqstDt.Rows.Add(drnewrow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizrule, "RQSTDT", "RSLTDT", dtRqstDt);

            int nREMAINING_QUANTITY = Int32.Parse(dtResult.Rows[0]["REMAINING_QUANTITY"].ToString());
            if (nREMAINING_QUANTITY == -999)
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    teslaSeqNoList.Add(Util.NVC(dtResult.Rows[i]["TESLA_SEQNO"].ToString()));
                }
            }
            else if (nREMAINING_QUANTITY < 0)
            {
                Util.MessageValidation("SFU8892", 0);
            }
            else if (nREMAINING_QUANTITY >= 0)
            {
                Util.MessageValidation("SFU8892", nREMAINING_QUANTITY);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
