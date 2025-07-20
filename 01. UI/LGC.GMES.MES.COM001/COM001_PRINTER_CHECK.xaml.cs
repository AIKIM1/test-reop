using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using QRCoder;
using System.Drawing;
using System.Collections;
using System.Text;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_PRINTER_CHECK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_PRINTER_CHECK : UserControl, IWorkArea
    {
        private string _LOTID = "XDC7B021";

        public IFrameOperation FrameOperation { get; set; }

        public COM001_PRINTER_CHECK()
        {
            InitializeComponent();
            //label.Content = ObjectDic.Instance.GetObjectName("바코드");
            //label1.Content = ObjectDic.Instance.GetObjectName("이력카드");
            //label2.Content = ObjectDic.Instance.GetObjectName("감열지");
            //btnBarcode.Content = ObjectDic.Instance.GetObjectName("인쇄");
            //btnHistoryCard.Content = ObjectDic.Instance.GetObjectName("인쇄");
            //btnHeat.Content = ObjectDic.Instance.GetObjectName("인쇄");
            //lblSBCAD.Content = ObjectDic.Instance.GetObjectName("SBC AD");
            //lblLGChemAD.Content = ObjectDic.Instance.GetObjectName("화학 AD");
            //lblFile.Content = ObjectDic.Instance.GetObjectName("노칭바코드");
            //btnSBCAuth.Content = ObjectDic.Instance.GetObjectName("인증");
            //btnLGChemAuth.Content = ObjectDic.Instance.GetObjectName("인증");
            //btnFilePrint.Content = ObjectDic.Instance.GetObjectName("다운로드");
            //label3.Content = ObjectDic.Instance.GetObjectName("바코드");
            //btnBarcode2.Content = ObjectDic.Instance.GetObjectName("인쇄");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsPersonByAuth(LoginInfo.USERID) == true)
            {
                grdMain.RowDefinitions[1].Height = new GridLength(35);
                grdMain.RowDefinitions[9].Height = new GridLength(35);
                txtBarcode.IsEnabled = true;
                txtBarcode2.IsEnabled = true;
            }
        }

        private void btnBarcode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBarcode.Text.Trim()) && txtBarcode.IsEnabled == true)
            {
                Util.MessageValidation("SFU1361");  //LOT ID 가 없습니다.
                txtBarcode.Focus();
                txtBarcode.SelectAll();
                return;
            }

            PrintLabel(txtBarcode.IsEnabled == true ? txtBarcode.Text.Trim() : _LOTID);
        }

        private void btnBarcode1_Click(object sender, RoutedEventArgs e)
        {
            int iBigQRSize = 880;
            string sZplCode;
            sZplCode =createQRCODE(txtBarcode2.Text.Trim(), iBigQRSize);
            Util.MessageInfo(sZplCode);

            CMM_ZPL_VIEWER2 wndPopup;
            wndPopup = new CMM_ZPL_VIEWER2(sZplCode);
            wndPopup.Show();

            }

        private void btnHistoryCard_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2 wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _LOTID; //LOT ID
                Parameters[1] = "E2000"; //PROCESS ID

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        void PrintLabel(string sLotID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("I_LBCD", typeof(string)); //라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string)); //프린터기종
                inTable.Columns.Add("I_RESO", typeof(string)); //해상도
                inTable.Columns.Add("I_PRCN", typeof(string)); //출력매수
                inTable.Columns.Add("I_MARH", typeof(string)); //시작위치H
                inTable.Columns.Add("I_MARV", typeof(string)); //시작위치V  

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LOTID"] = sLotID;
                        indata["I_LBCD"] = LoginInfo.CFG_LABEL_TYPE;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                    throw new Exception(MessageDic.Instance.GetMessage("SFU3030"));

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_SAMPLE", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["I_ATTVAL"].ToString();

                    foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()))
                        {
                            FrameOperation.PrintFrameMessage(string.Empty);
                            bool brtndefault = Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME]).Equals("USB") ? FrameOperation.Barcode_ZPL_USB_Print(zpl) : FrameOperation.Barcode_ZPL_Print(dr, zpl);

                            if (brtndefault == false)
                            {
                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); //Barcode Print 실패
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnFilePrint_Click(object sender, RoutedEventArgs e)
        {
            FilePrintLabel(_LOTID);
        }

        void FilePrintLabel(string sLotID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("I_LBCD", typeof(string)); //라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string)); //프린터기종
                inTable.Columns.Add("I_RESO", typeof(string)); //해상도
                inTable.Columns.Add("I_PRCN", typeof(string)); //출력매수
                inTable.Columns.Add("I_MARH", typeof(string)); //시작위치H
                inTable.Columns.Add("I_MARV", typeof(string)); //시작위치V  

                DataRow indata = inTable.NewRow();
                indata["LOTID"] = sLotID;
                indata["PROCID"] = "E2000";
                indata["I_LBCD"] = LoginInfo.CFG_LABEL_TYPE;
                indata["I_PRMK"] = "Z";
                indata["I_RESO"] = "300";
                indata["I_PRCN"] = LoginInfo.CFG_LABEL_COPIES;
                indata["I_MARH"] = "0";
                indata["I_MARV"] = "0";
                inTable.Rows.Add(indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_CT", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["I_ATTVAL"].ToString();

                    Util.SendZplBarcode(sLotID, zpl);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnHeatPrint_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PALLETID", _LOTID);
            dicParam.Add("MTGRNAME", "TEST1");
            dicParam.Add("MTRLDESC", "TEST2");
            dicParam.Add("MLOTID", "TEST3");
            dicParam.Add("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd"));

            LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT print = new LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT(dicParam);
            print.FrameOperation = FrameOperation;

            this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));
        }

        private void btnADAuth_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
            authConfirm.FrameOperation = FrameOperation;
            if (authConfirm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = "ELEC_MANA";    // 관리권한

                C1WindowExtension.SetParameters(authConfirm, Parameters);

                authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
            }
        }

        private void btnLGADAuth_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
            authConfirm.FrameOperation = FrameOperation;
            if (authConfirm != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = "ELEC_MANA";    // 관리권한
                Parameters[1] = "lgchem.com";

                C1WindowExtension.SetParameters(authConfirm, Parameters);

                authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
            }
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("인증성공");
            }
        }
        
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


        
        private bool IsPersonByAuth(string sUserID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = "ELEC_LABEL_OPER";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

    }
}