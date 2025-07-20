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
  2023.03.13  이홍주    : 소형활성화 MES 복사
  2023.11.14  이홍주    : 3S TESLA 라벨을 PDF 및 ZPL로 출력하게 수정
  2024.03.21  이홍주    : 3S TESLA 라벨을 QRCODE를 BIGSIZE 이미지로 변환
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
using System.Collections;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_305_PRINT : C1Window, IWorkArea
    {
        private Dictionary<string, string> _dic = null;
        private String sPage = null;
        private int _PageQty = 1;
        private string _ReportFormat = string.Empty;
        
        C1.C1Report.C1Report[] crList = null;

        Util _util = new Util();

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        DataTable dtResult = new DataTable();
        DataRow _drPrtInfo = null;


        public List<string> teslaSeqNoList;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_305_PRINT()
        {
            InitializeComponent();
        }

        public FCS002_305_PRINT(Dictionary<string, string> dicParam, int PrtQty, string sReportFormat)
        {
            try
            {
                InitializeComponent();

                _dic = dicParam;
                _PageQty = PrtQty;
                sPage = "Tesla_Report";

               
                teslaSeqNoList = new List<string>();    //OUTBOXID
                crList = new C1.C1Report.C1Report[PrtQty];

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[crList.Length];

                //_ReportFormat = "LGC.GMES.MES.FCS002.Report.Tesla_Report.xml";
                _ReportFormat = sReportFormat;


                #region Tesla_Report : 3J Content Label
                if (_ReportFormat == "LGC.GMES.MES.FCS002.Report.Tesla_Report.xml")
                {
                    try
                    {

                        if (_dic.Count > 0 && (_dic["REPRINT_YN"].ToString() == "Y"))  //재발행
                        {
                            teslaSeqNoList.Add(_dic["BOXID"].ToString());
                        }
                        else
                        {
                            Get_Seq_No(); //라벨 SEQ 채번 및 DB INSERT

                        }

                        if (teslaSeqNoList.Count == 0)
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageValidation("SFU8252"); // 잠시 후 다시 시도해주세요.
                        return;
                    }

                    using (Stream stream = a.GetManifestResourceStream(_ReportFormat))
                    {
                        crList[0] = new C1.C1Report.C1Report();

                        //TEST
                        //crList[0].Load(@"C:\Tesla_Report.xml", sPage);
                        //crList[0].Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                        crList[0].Load(stream, sPage);


                        for (int i = 0; i < dicParam.Count; i++)
                        {
                            string key = dicParam.Keys.ToList()[i].ToString();

                            if (key.Equals("QTY_UNIT"))
                                continue;
                            else if (key.Equals("SITE_CODE"))
                                continue;
                            else if (key.Equals("REPRINT_YN"))
                                continue;
                            else if (key.Equals("BOXID"))
                                continue;
                            else if (key.Equals("ZPLCODE"))
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
                #endregion

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
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

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if ((bool)rdoPdf.IsChecked)
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
                #region ZPL Print
                else
                {
                    // 발행 가능 여부 체크
                    if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    {
                        return;
                    }

                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("LANGID");
                    inData.Columns.Add("EQPTID");
                    inData.Columns.Add("BOXID");

                    DataRow newRow = inData.NewRow();

                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                    newRow["BOXID"] = "PALLET_ID";

                    inData.Rows.Add(newRow);

                    DataTable inPrint = indataSet.Tables.Add("INPRINT");
                    inPrint.Columns.Add("PRMK");
                    inPrint.Columns.Add("RESO");
                    inPrint.Columns.Add("PRCN");
                    inPrint.Columns.Add("MARH");
                    inPrint.Columns.Add("MARV");
                    inPrint.Columns.Add("DARK");

                    newRow = inPrint.NewRow();
                    newRow["PRMK"] = _sPrt;
                    newRow["RESO"] = _sRes;
                    newRow["PRCN"] = _sCopy;
                    newRow["MARH"] = _sXpos;
                    newRow["MARV"] = _sYpos;
                    newRow["DARK"] = _sDark;
                    inPrint.Rows.Add(newRow);

                    /////
                    try
                    {
                        //신규
                        if (dtResult.Rows.Count > 0 && teslaSeqNoList.Count > 0)
                        {
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {   
                                string sZplCode = dtResult.Rows[i]["ZPL_CODE"].ToString();

                                string sBoxID = teslaSeqNoList[0];
                                sZplCode = sZplCode.Trim().Substring(2);
                                int iBigQRSize = 880;
                                sZplCode = sZplCode.Replace("QRCODE_BIG", createQRCODE(sBoxID, iBigQRSize));

                                if(PrintLabel(sZplCode, _drPrtInfo))
                                { 
                                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                }

                            }
                        }
                        else//재발행
                        {
                            string sZplCode = _dic["ZPLCODE"].ToString();

                            string sBoxID = teslaSeqNoList[0];
                            sZplCode = sZplCode.Trim().Substring(2);

                            int iBigQRSize = 880;
                            sZplCode = sZplCode.Replace("QRCODE_BIG", createQRCODE(sBoxID, iBigQRSize));

                            
                            if (PrintLabel(sZplCode, _drPrtInfo))
                            {
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            }
                        }


                        

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }
                #endregion ZPL

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }


        }




        /// <summary> QR코드를 ZPL코드로 변환 </summary>
        /// <param name="QR">QR코드를 만들고자 하는 문자열</param>
        /// /// <param name="size">만들고자하는 크기(픽셀단위)</param>
        private String createQRCODE(String qr, int size)
        {
            int widthByte = size / 8;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrcodedata = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
            QRCode code = new QRCode(qrcodedata);

            Bitmap nbit = code.GetGraphic(20, System.Drawing.Color.Black, System.Drawing.Color.White, false);
            Bitmap resized = new Bitmap(nbit, size, size);
            String cuerpo = createBody(resized);
            String QRCODE = encodeHexAscii(cuerpo, widthByte);

            return QRCODE;
        }
        
        /// <summary>
        /// QR코드를 각 픽셀마다 RGB값을 더해 16진수 값으로 변환
        /// </summary>
        /// <param name="bitmap">QR코드를 bitmap으로 변형한 값.</param>
        private String createBody(Bitmap bitmap)
        {
            StringBuilder sb = new StringBuilder();
            int width = bitmap.Width;
            int height = bitmap.Height;
            int index = 0;
            char[] auxBinaryChar = { '0', '0', '0', '0', '0', '0', '0', '0' };
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    System.Drawing.Color c = bitmap.GetPixel(w, h);
                    char auxChar = '1';
                    int totalColor = c.R + c.G + c.B;
                    if (totalColor > 384)
                    {
                        auxChar = '0';
                    }
                    auxBinaryChar[index] = auxChar;
                    index++;
                    if (index == 8)
                    {
                        sb.Append((fourByteBinary(new String(auxBinaryChar))));
                        auxBinaryChar = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
                        index = 0;
                    }
                }
                /* (오른쪽 90도 회전)
                for (int h = height - 1; h > -1; h--)
                {
                    Color c = bitmap.GetPixel(w, h);
                    char auxChar = '1';
                    int totalColor = c.R + c.G + c.B;
                    if (totalColor > 384)
                    {
                        auxChar = '0';
                    }
                    auxBinaryChar[index] = auxChar;
                    index++;
                    if (index == 8)
                    {
                        sb.Append((fourByteBinary(new String(auxBinaryChar))));
                        auxBinaryChar = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
                        index = 0;
                    }
                }
                */
                sb.Append("\n");

            }
            return sb.ToString();
        }

        /// <summary>
        /// 8비트 String을 16진수로 바꾸는 함수
        /// </summary>
        /// <param name="binaryStr"></param>
        /// <returns></returns>
        private String fourByteBinary(String binaryStr)
        {
            int int2 = System.Convert.ToInt32(binaryStr, 2);
            String int16 = System.Convert.ToString(int2, 16);
            if (int16.Length == 1)
            {
                int16 = '0' + int16;
            }
            return int16.ToUpper();
        }

        /// <summary>
        /// 16진수를 아스키코드로 변환하는 함수
        /// </summary>
        /// <param name="code"></param>
        /// <param name="widthBytes"></param>
        /// <returns></returns>
        private String encodeHexAscii(String code, int widthBytes)
        {
            Hashtable mapCode = new Hashtable();
            mapCode.Add(1, "G");
            mapCode.Add(2, "H");
            mapCode.Add(3, "I");
            mapCode.Add(4, "J");
            mapCode.Add(5, "K");
            mapCode.Add(6, "L");
            mapCode.Add(7, "M");
            mapCode.Add(8, "N");
            mapCode.Add(9, "O");
            mapCode.Add(10, "P");
            mapCode.Add(11, "Q");
            mapCode.Add(12, "R");
            mapCode.Add(13, "S");
            mapCode.Add(14, "T");
            mapCode.Add(15, "U");
            mapCode.Add(16, "V");
            mapCode.Add(17, "W");
            mapCode.Add(18, "X");
            mapCode.Add(19, "Y");
            mapCode.Add(20, "g");
            mapCode.Add(40, "h");
            mapCode.Add(60, "i");
            mapCode.Add(80, "j");
            mapCode.Add(100, "k");
            mapCode.Add(120, "l");
            mapCode.Add(140, "m");
            mapCode.Add(160, "n");
            mapCode.Add(180, "o");
            mapCode.Add(200, "p");
            mapCode.Add(220, "q");
            mapCode.Add(240, "r");
            mapCode.Add(260, "s");
            mapCode.Add(280, "t");
            mapCode.Add(300, "u");
            mapCode.Add(320, "v");
            mapCode.Add(340, "w");
            mapCode.Add(360, "x");
            mapCode.Add(380, "y");
            mapCode.Add(400, "z");

            int maxlinea = widthBytes * 2;
            StringBuilder sbCode = new StringBuilder();
            StringBuilder sbLinea = new StringBuilder();
            String previousLine = null;
            int counter = 1;
            char aux = code[0];
            Boolean firstChar = false;
            for (int i = 1; i < code.Length; i++)
            {
                if (firstChar)
                {
                    aux = code[i];
                    firstChar = false;
                    continue;
                }
                if (code[i] == '\n')
                {
                    if (counter >= maxlinea && aux == '0')
                    {
                        sbLinea.Append(",");
                    }
                    else if (counter >= maxlinea && aux == 'F')
                    {
                        sbLinea.Append("!");
                    }
                    else if (counter > 20)
                    {
                        int multi20 = (counter / 20) * 20;
                        int resto20 = (counter % 20);
                        sbLinea.Append(mapCode[multi20]);
                        if (resto20 != 0)
                        {
                            sbLinea.Append(mapCode[resto20] + aux.ToString());
                        }
                        else
                        {
                            sbLinea.Append(aux);
                        }
                    }
                    else
                    {
                        sbLinea.Append(mapCode[counter] + aux.ToString());
                        if (mapCode[counter] == null)
                        {
                        }
                    }
                    counter = 1;
                    firstChar = true;
                    if (sbLinea.ToString().Equals(previousLine))
                    {
                        sbCode.Append(":");
                    }
                    else
                    {
                        sbCode.Append(sbLinea.ToString());
                    }
                    previousLine = sbLinea.ToString();
                    sbLinea.Length = 0;
                    continue;
                }
                if (aux == code[i])
                {
                    counter++;
                }
                else
                {
                    if (counter > 20)
                    {
                        int multi20 = (counter / 20) * 20;
                        int resto20 = (counter % 20);
                        sbLinea.Append(mapCode[multi20]);
                        if (resto20 != 0)
                        {
                            sbLinea.Append(mapCode[resto20] + aux.ToString());
                        }
                        else
                        {
                            sbLinea.Append(aux);
                        }
                    }
                    else
                    {
                        sbLinea.Append(mapCode[counter] + aux.ToString());
                    }
                    counter = 1;
                    aux = code[i];
                }
            }
            return sbCode.ToString();
        }

        private void Get_Seq_No()
        {
            string bizrule = "";

            dtResult.Clear();
            
       
            if (LoginInfo.CFG_SHOP_ID.Equals("A010") || LoginInfo.CFG_SHOP_ID.Equals("F030")) // 오창 소형조립, 오창2산단 소형조립
            {  
              bizrule = "BR_GET_OUTBOX_UI_ZPL_LABEL_INFO_MB"; 
            }
            else if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
            {
                
            }

            DataSet indataSet = new DataSet();

            DataTable dtINDATA = indataSet.Tables.Add("INDATA");
            dtINDATA.TableName = "INDATA";
            dtINDATA.Columns.Add("SRCTYPE", typeof(string));
            dtINDATA.Columns.Add("IFMODE", typeof(string));
            dtINDATA.Columns.Add("EQPTID", typeof(string));
            dtINDATA.Columns.Add("TESLA_PART_NUM", typeof(string));
            dtINDATA.Columns.Add("SITE_CODE", typeof(string));
            dtINDATA.Columns.Add("PRINT_TYPE", typeof(string));
            dtINDATA.Columns.Add("SHOPID", typeof(string));
            dtINDATA.Columns.Add("LOTID", typeof(string));
            dtINDATA.Columns.Add("GROSS_WEIGHT", typeof(string));
            dtINDATA.Columns.Add("CELLQTY", typeof(string));
            dtINDATA.Columns.Add("PRT_QTY", typeof(int));
            dtINDATA.Columns.Add("USERID", typeof(string));
            
            DataRow drnewrow = dtINDATA.NewRow();
            //drnewrow["PRT_DATE"] = DateTime.Parse(_dic["PRINT_DATE"]).ToString("yyyyMMdd");
            drnewrow["SRCTYPE"] = "UI";
            drnewrow["IFMODE"] = "OFF";
            drnewrow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
            drnewrow["TESLA_PART_NUM"] = _dic["PART_NUM"].ToString();//.Replace("-", ""); 
            drnewrow["SITE_CODE"] = _dic["SITE_CODE"].ToString();
            drnewrow["PRINT_TYPE"] = "1"; //1: ZPL 2:USB
            drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drnewrow["LOTID"] = _dic["LOT_CODE"].ToString();
            drnewrow["GROSS_WEIGHT"] = _dic["WEIGHT"].ToString();
            drnewrow["CELLQTY"] = _dic["QTY"].ToString();
            drnewrow["PRT_QTY"] = _PageQty;
            drnewrow["USERID"] = LoginInfo.USERID;

            dtINDATA.Rows.Add(drnewrow);

            DataSet ds = new ClientProxy().ExecuteServiceSync_Multi(bizrule, "INDATA", "OUTDATA", indataSet);
            

            if (ds.Tables.Contains("OUTDATA") && ds.Tables["OUTDATA"].Rows.Count > 0)
            {
                dtResult = ds.Tables["OUTDATA"];
            }
            else
            {
                return;
            }

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

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
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
