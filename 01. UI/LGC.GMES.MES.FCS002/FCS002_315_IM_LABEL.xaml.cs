/*************************************************************************************
 Created Date : 2023.12.08
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.08  이홍주    : IM NFF BOX라벨 재발행POPUP 
  2024.03.21  이홍주    : NFF BOX라벨 재발행시 기존 ZPLCODE 사용 
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_315_IM_LABEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        
        private String sPage = null;
        private int _PageQty = 1;
        private string _ReportFormat = string.Empty;

        C1.C1Report.C1Report[] crList = null;
        
        public List<string> teslaSeqNoList;

        private Dictionary<string, string> _dic = null;
        Util _Util = new Util();
        string strZPL = string.Empty;
        string zpl = string.Empty;
        string sOutBox = string.Empty;

        string siteCode = "";
        string strSupplierNo = "";
                
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();

        public FCS002_315_IM_LABEL()
        {
            InitializeComponent();
        }
        private void FCS002_315_IM_LABEL_Loaded(object sender, RoutedEventArgs e)
        {
          
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        //public FCS002_315_IM_LABEL()
        //{
        //    ///sOutBox = pOutBox;
        //}

        #endregion

        #region Initialize
        private void Initialize()
        {
            
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtOutBoxID.Text = tmps[0] as string;
            //_UserId = tmps[1] as string;
            //_PalltID = tmps[2] as string;
            //_AommGrade = tmps[3] as string;
        
        }
        #endregion

        #region Event
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //2022.04.27 필수 입력 사항 체크
                //PART NUMBER
                if (string.IsNullOrEmpty(txtOutBoxID.Text))
                {
                    Util.MessageValidation("SFU4391"); // BoxID를 입력하세요.
                    return;
                }
                
                //OUTBOX 3S Contents라벨 재발행
                if (rdo3s.IsChecked == true)
                {                    
                    RePrintProcess_3J();
                }
                else if (rdo1d.IsChecked == true)
                {
                    RePrintProcess_1D();
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
           
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        
        //Tesla OutBox Label 재발행
        private void RePrintProcess_3J()
        {
            try
            {
                string sOutBoxId = (txtOutBoxID.Text).Trim();
                
                DataTable dtRqstDt = new DataTable();

                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("BOXID", typeof(string));
                
                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["BOXID"] = sOutBoxId;

                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRqstDt);

                if (dtResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("101241", new string[] { txtOutBoxID.Text });  //라벨[%1]의 발행 이력이 존재하지 않습니다.

                    return;
                }

                //dtResult.Rows.Count == 0||



                /*  RSLTDT Comment
                    BOX_LABEL_ID
                    LABEL_PRT_SEQNO :  PK, 출력순번
                    LABEL_CODE      :  LBL0322 등의 라벨 코드
                    PRT_DTTM        : 출력일자 
                    REPRT_FLAG       
                    LABEL_ZPL_CNTT  : ZPL CODE
                    NOTE                 
                    LABEL_PRT_COUNT :
                    PRT_ITEM01      : CONTRY_OF_ORIGIN(KOREA)
                    PRT_ITEM02      : Quantity(64 EA)
                    PRT_ITEM03      : Gross Weight(111 LB)
                    PRT_ITEM04      : Part Name(4680LB1)
                    PRT_ITEM05
                    PRT_ITEM06      : BOXID(3SD1Q0000640081X4183756100A)
                    PRT_ITEM07      : Supplier(178119)
                    PRT_ITEM08      : Supplier Name(LG Energy Soluction LTD)
                    PRT_ITEM09      
                    PRT_ITEM10
                    PRT_ITEM11      : LOTCODE 
                    PRT_ITEM12      : PartNo?(1837561-00-A)
                    PRT_ITEM13      : LOTID 2D(P1837561-00-A:1TQ62CC21P:15D)
                    PRT_ITEM14                    PRT_ITEM15                    PRT_ITEM16                    PRT_ITEM17                    PRT_ITEM18
                    PRT_ITEM19                    PRT_ITEM20                    PRT_ITEM21                    PRT_ITEM22                    PRT_ITEM23
                    PRT_ITEM24                    PRT_ITEM25                    PRT_ITEM26                    PRT_ITEM27                    PRT_ITEM28
                    PRT_ITEM29                    PRT_ITEM30                    INSUSER                       INSDTTM                       UPDUSER                    UPDDTTM
                    LOTID                         PRT_ITEM31                    PRT_ITEM32                    PRT_ITEM33                    PRT_ITEM34
                    PRT_ITEM35                    PRT_ITEM36                    PRT_ITEM37                    PRT_ITEM38                    PRT_ITEM39
                    PRT_ITEM40                    PRT_ITEM41                    PRT_ITEM42                    PRT_ITEM43                    PRT_ITEM44
                    PRT_ITEM45                    PRT_ITEM46                    PRT_ITEM47                    PRT_ITEM48                    PRT_ITEM49
                    PRT_ITEM50                    BOXID                         PRT_TYPE                      PGM_ID                        BZRULE_ID
                    */

                string[] vQuantity = dtResult.Rows[0]["PRT_ITEM02"].ToString().Split(' ');
                string  sWeight = dtResult.Rows[0]["PRT_ITEM03"].ToString();
                               

                //drnewrow["ATTRIBUTE3"] = Util.NVC(cboSupplier.Text.ToString());

                btnPrint.IsEnabled = false;

                Dictionary<string, string> dic = new Dictionary<string, string>();

                dic.Add("ZPLCODE", dtResult.Rows[0]["LABEL_ZPL_CNTT"].ToString());
                dic.Add("CONTRY_OF_ORIGIN", dtResult.Rows[0]["PRT_ITEM01"].ToString());
                dic.Add("QTY", vQuantity[0]);
                dic.Add("QTY_UNIT", vQuantity[1]);
                dic.Add("WEIGHT", sWeight);
                dic.Add("PART_NAME", dtResult.Rows[0]["PRT_ITEM04"].ToString());
                dic.Add("QRcode1", dtResult.Rows[0]["PRT_ITEM13"].ToString());
                dic.Add("SUPPLIER", "          " + dtResult.Rows[0]["PRT_ITEM07"].ToString() + "\r\n" + dtResult.Rows[0]["PRT_ITEM08"].ToString()); 
                dic.Add("PRINT_DATE", dtResult.Rows[0]["PRT_ITEM10"].ToString());
                ///dic.Add("EXP_DATE", txtExpDate.Text.ToString());
                ///dic.Add("SERIAL_NUM", txtSerialNum.Text.ToString());
                dic.Add("LOT_CODE", dtResult.Rows[0]["PRT_ITEM11"].ToString());  //LOTID
                dic.Add("PART_NUM", dtResult.Rows[0]["PRT_ITEM12"].ToString());//.Replace("-", ""));
                dic.Add("SITE_CODE", SiteSetting(dtResult.Rows[0]["PRT_ITEM12"].ToString(), dtResult.Rows[0]["PRT_ITEM08"].ToString())); 
                dic.Add("REPRINT_YN", "Y");
                dic.Add("BOXID", sOutBoxId);
                
                int prtQTY = 0;
                //재발행은 1로 고정
                prtQTY = 1;

                FCS002_305_PRINT wndPrint = new FCS002_305_PRINT(dic, prtQTY, "LGC.GMES.MES.FCS002.Report.Tesla_Report.xml");
                wndPrint.FrameOperation = FrameOperation;

                if (wndPrint != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = 1;
                    C1WindowExtension.SetParameters(wndPrint, Parameters);
                    wndPrint.Closed += new EventHandler(wndPrint_Closed);

                    if (wndPrint.teslaSeqNoList.Count != 0)
                    {
                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
                        //  grdMain.Children.Add(wndPrint);
                        //  wndPrint.BringToFront();
                    }
                    else
                    {
                        btnPrint.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void RePrintProcess_1D()
        {
            try
            {
                string sOutBoxId = (txtOutBoxID.Text).Trim();

                DataTable dtRqstDt = new DataTable();

                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("BOXID", typeof(string));
                dtRqstDt.Columns.Add("OUTBOX_1D_LABEL_ID", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["BOXID"] = sOutBoxId;
                //drnewrow["OUTBOX_1D_LABEL_ID"] = sOutBoxId;

                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_BOX_ATTR_F_MB", "RQSTDT", "RSLTDT", dtRqstDt);

                if (dtResult.Rows.Count <= 0)
                {
                    Util.MessageValidation("101241", new string[] { txtOutBoxID.Text });  //라벨[%1]의 발행 이력이 존재하지 않습니다.

                    return;
                }
                
                /*  RSLTDT Comment
                    OUTBOX_1D_LABEL_ID : OUTBOX 1D Label Id
                    DSNT_ID         :  방습제 ID
                    INSUSER         :  
                    INSDTTM         :  
                    UPDUSER         :  
                    UPDDTTM         :  
                    
                    */

                string  sOutBox1dLabel = dtResult.Rows[0]["OUTBOX_1D_LABEL_ID"].ToString();

                RePrint_1DBox_ZPL(sOutBox1dLabel);

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        
        private string SiteSetting(string sTeslaPartNum, string sSupplierName)
        {
            //사이트 코드 정보 셋팅


            string sSiteCode = "";

            try
            {
                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")
                    || LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) //남경 소형조립, 오창2산단 소형조립
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                    dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));
                    dtRqst.Columns.Add("CMCDNAME", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;

                    if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                    {
                        dr["CMCDTYPE"] = "TESLA_SUPPLIER_NJ";
                    }
                    else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) // 오창2산단 소형조립
                    {
                        dr["CMCDTYPE"] = "TESLA_SUPPLIER_OC2";
                    }
                    dr["ATTRIBUTE1"] = sTeslaPartNum;
                    dr["CMCDNAME"] = sSupplierName;
                    dtRqst.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_CMCDNAME", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtResult.Rows.Count > 0)
                        sSiteCode = dtResult.Rows[0]["ATTRIBUTE5"].ToString();
                }

                return sSiteCode;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return sSiteCode;
            }
        }

        private void wndPrint_Closed(object sender, EventArgs e)
        {
            btnPrint.IsEnabled = true;
        }
            
        private void RePrint_1DBox_ZPL(string sOutBox1dLabel)
        {
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

                //if (cboEquipment.SelectedValue.ToString().Equals(string.Empty) || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
                //{
                //    // 설비를 선택하세요.
                //    Util.MessageInfo("SFU1153");
                //    return;
                //}

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("PRINTQTY");
                inDataTable.Columns.Add("LABELTYPE");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("OUTBOX_1D_LABEL_ID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PRINTQTY"] = 2; //1D라벨은 2장씩 출력
                newRow["LABELTYPE"] = "TESLA_DISP_OUTBOX"; // TESLA 1D OUTBOX : 사용안함. 
                newRow["USERID"] = LoginInfo.USERID;
                newRow["OUTBOX_1D_LABEL_ID"] = sOutBox1dLabel;
                

                inDataTable.Rows.Add(newRow);
                
                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                //OUTDATA 2건 생성?
                //BOXID
                //ZPLCODE

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_OUTBOX_1D_LABEL_FOR_TESLA_MB", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        string zplCode = string.Empty;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        foreach (DataRow dr in bizResult.Tables["OUTDATA"].Rows)
                        {
                            zplCode += dr["ZPLCODE"].ToString();
                        }

                        if (PrintLabel(zplCode, drPrtInfo))
                        {
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        }

                        //this.DialogResult = MessageBoxResult.OK;
                        //this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //txtBoxID.Text = string.Empty;
            }
        }

        private byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);

            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            //System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(400, 400, null, IntPtr.Zero);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(600, 600, null, IntPtr.Zero);
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

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
      
        #endregion


    }
}
