/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.10.03  손우석 CSR ID 3499448 Pack10호 호라이즌 포장박스 라벨 출력 변경 요청 건 요청번호 C20171003_99448
  2017.10.27  손우석 CSR ID 3514652 GMES 포장박스 라벨 출력 개선 요청 건  요청번호 C20171026_14652
  2018.10.08  손우석 같은 제품 다른 라인/라른 사이트 인경우 라인 정보 파라미터 추가
  2018.10.29  손우석 파라미터 명칭 오류 수정
  2018.11.09  손우석 CSR ID 3837429 폴란드 Volvo 517H 포장 라벨 변경 요청 요청번호 C20181107_37429
  2018.11.30  손우석 CSR ID 3847117 517H(Volvo) Box 라벨 발행 화면 개선 요청 건 요청번호 C20181119_47117
  2019.04.07  손우석 519H 제품 추가로 인한 조건 추가
  2020.06.24  손우석 서비스번호 73520 [생산PI팀]Box 라벨 프린터 화면 개선 [요청번호] C20200620-000040
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtModelResult;       
        DataTable dtTextResult;
        DataTable dtLabelCodes;
        System.ComponentModel.BackgroundWorker bkWorker;

        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = string.Empty; 
        string zpl = string.Empty;

        string reference_txt = "^A0B,56,56^FO750,408^FDITEM_REF^FS"; //^A0B,56,56^FO750,708^FDtesttest^FS
        string reference_bar = "^BY4,2.0^FO800,190^B3B,N,96,N,N^FDITEM_REF^FS";  //^BY4,2.0^FO800,340^B3B,N,96,N,N^FDtesttest^FS
        string ITEM_REF = string.Empty;
        string MSD_Plug = string.Empty;

        public PACK001_008()
        {            
            InitializeComponent();

            this.Loaded += PACK001_008_Loaded;   
        }
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //2017.10.27
            //if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
            //{
            if (LoginInfo.CFG_SHOP_ID == "A040")
            { 
                //txtSeletedLabel.Visibility = Visibility.Visible;
                cboLabel.Visibility = Visibility.Visible;
            }
            //}

            //2018.11.30
            if (LoginInfo.CFG_SHOP_ID == "G481")
            {
                lblLotd.Visibility = Visibility.Visible;
                txtSearchLotId.Visibility = Visibility.Visible;
                radL.Visibility = Visibility.Visible;
                radP.Visibility = Visibility.Visible;
            }

            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now ;
            //2017.10.27
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;         

            txtDate.Text = PrintOutDate(DateTime.Now);  //txtZpl018.Text = PrintOutDate(DateTime.Now);
            //dtpDate_SelectedDateChanged(null, null); //dtp312HDay_ValueChanged(null, null); dtpDate

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            getLabelCode();  // COMMONCODE TABLE에서 해당 화면에서 발행할 LABEL_CODE 가져오기 (CMCDTYPE = "PACK_LABEL_CODE")
            setCombo_PROD(); // LABEL CODE로 제품 정보 가져오기
            setCombo_Out();  // 제품정보로 출하처 정보 가져오기
            //2017.10.27
            setCombo_Label();

            dtpDate.SelectedDataTimeChanged += dtpDate_SelectedDateChanged;            
        }
        #endregion Initialize

        #region Event
        private void PACK001_008_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_Loaded;
        }

        private void setTextBox()
        {
            if (dtTextResult == null || dtTextResult.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtTextResult;

            txtNetWeight.Text = returnString(dt, "ITEM006");        //Net Weight(kg) : 115           
            txtGrossWeight.Text = returnString(dt, "ITEM007");      //Gross Weight(kg) : 160           
            txtDescription2.Text = returnString(dt, "ITEM014");     //Description 2 : 355V,25.9A, 6,500Wh(Usable)
            txtReceive.Text = returnString(dt, "ITEM001");          //Receiver : VOLVO TORSLANDA MONTERING           
            txtDock.Text = returnString(dt, "ITEM002");             //DOCK GATE : TCV
            txtSupplierAddress.Text = returnString(dt, "ITEM005");  //Supplier Address : LG Chem, Ltd
            txtBoxes.Text = returnString(dt, "ITEM008");            //No of Boxes : 1        

            if (tabItem.Header.ToString() == "HORI")
            {
                //2017.10.03
                //txtLogistic1.Text = "1E0001683AB";
                //bcLogistic1.Text = "1E0001683AB";
                //bcLogistic1.Visibility = Visibility.Visible;
                txtLogistic1.Visibility = Visibility.Hidden;
                bcLogistic1.Visibility = Visibility.Hidden;
                txtLogistic2.Visibility = Visibility.Hidden;

                //ITEM_REF = txtLogistic1.Text;
            }
            else
            {
                bcLogistic1.Visibility = Visibility.Hidden;
                txtLogistic2.Visibility = Visibility.Visible;

                txtLogistic1.Text = "logic";
                txtLogistic2.Text = "reference";
            }

            txtDescription1.Text = returnString(dt, "ITEM013");     //Description : BATTERY PACK

            //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H")
            //{               
            //    txtAdvice.Text = "N" + returnString(dt, "ITEM003");           //ADVICE NOTE NO : N61606292
            //    txtpartNumber.Text = "P" + returnString(dt, "ITEM009");       //Part No : P31491834
            //    txtquantity.Text = "Q" + returnString(dt, "ITEM011");         //Quantity : Q1
            //    txtSupplierID.Text = "V" + returnString(dt, "ITEM015");       //Supplier ID : VGE2PB
            //    txtSerial.Text = "S" + returnString(dt, "ITEM018");           //Serial No : S12031601
            //    txtBatch.Text = "H" + returnString(dt, "ITEM020");            //Batch No : H312031601
            //}
            //else
            //{
            txtAdvice.Text = returnString(dt, "ITEM003");           //ADVICE NOTE NO : 61606292
            txtpartNumber.Text = returnString(dt, "ITEM009"); //"31407014";                    //Part No : 31491834
            txtquantity.Text = returnString(dt, "ITEM011");         //Quantity : 1
            txtSupplierID.Text = returnString(dt, "ITEM015");       //Supplier ID : GE2PB

            //2017.10.03
            if (tabItem.Header.ToString() != "HORI")
            {
                txtSerial.Text = returnString(dt, "ITEM018");           //Serial No : 312031601
            }

            txtBatch.Text = returnString(dt, "ITEM020");            //Batch No : 312031601

            //2019.04.07
            if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "519H")
            //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H")
            //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
            {
                bcAdvice.Text = "N" + txtAdvice.Text;           //ADVICE NOTE NO : N61606292
                bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P31491834
                bcquantity.Text = "Q" + txtquantity.Text;       //Quantity : Q1
                bcSupplierID.Text = "V" + txtSupplierID.Text;   //Supplier ID : VGE2PB
                bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S12031601
                bcBatch.Text = "H" + txtBatch.Text;            //Batch No : H312031601
            }
            //2017.10.03
            else if (tabItem.Header.ToString() == "HORI")
            {
                bcAdvice.Text = "N" + txtAdvice.Text;           //ADVICE NOTE NO : N61606292
                //bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P31491834
                bcquantity.Text = "Q" + txtquantity.Text;       //Quantity : Q1
                bcSupplierID.Text = "V" + txtSupplierID.Text;   //Supplier ID : VGE2PB

                //20171009 LOT 발번 체계 바뀌면서 빠짐.
                txtLogistic1.Text = "";
                bcLogistic1.Text = "";
                txtLogistic2.Text = "";
                //bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S12031601
                //bcBatch.Text = "H" + txtBatch.Text;            //Batch No : H312031601
            }
            else
            {
                bcAdvice.Text = txtAdvice.Text;           //ADVICE NOTE NO : N61606292
                bcpartNumber.Text = txtpartNumber.Text;   //Part No : P31491834
                bcquantity.Text = txtquantity.Text;       //Quantity : Q1
                bcSupplierID.Text = txtSupplierID.Text;   //Supplier ID : VGE2PB
                bcSerial.Text = txtSerial.Text;           //Serial No : S12031601
                bcBatch.Text = txtBatch.Text;            //Batch No : H312031601
            }
        }

        private string returnString(DataTable dt, string item_code)
        {
            return selectText(dt, item_code).Length > 0 ? Util.NVC(selectText(dt, item_code)[0]["ITEM_VALUE"]) : "";
        }

        #region Button
        private void btnAdvice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Label Layout의 Advice Note No 부분 출력
                string strZPLString = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH10,10^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                if ((bool)chkAutoPrint.IsChecked)
                {
                    strZPLString += string.Format("^A0N,18,20^FO5,0^CI0^FDAdvice Note No (N)^FS");
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                }
                else
                {                    
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtAdvice.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", txtAdvice.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 

                    return;
                }
            }
            catch(Exception ex)
            {
                Util.Alert(ex.Message);
            }            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting();

                dtpDate_SelectedDateChanged(null, null);

                Get_Product_Lot();
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPrint.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                if (txtSerial.Text != txtBatch.Text)
                {
                    ms.AlertWarning("SFU3376"); //Serial No과 Batch No가 일치 하지 않습니다
                    return;
                }

                PrintProcess(btnPrint);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        #endregion Button

        #region Dataset / DataTable / DataRow
        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();                        

                string strLotId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();
                string strRefNo = strLotId.Substring(strLotId.IndexOf('T')+1);

                //2017.10.03
                if (tabItem.Header.ToString() == "HORI")
                {
                    // 1234567890123456789012345678
                    // S17402001#P1E0001683AB#CELEC
                    txtpartNumber.Text = strLotId.ToString().Substring(11, 11).ToString();

                    bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P1E0001683AB   
                    txtSerial.Text = strLotId.ToString().Substring(1, 8).ToString();  //17402001
                    txtBatch.Text = strLotId.ToString().Substring(1, 8).ToString();   //17402001

                    bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S17402001                    
                    bcBatch.Text = "H" + txtBatch.Text;             //Batch No  : H17402001
                }
                else
                {
                    //31499073 T 817131605  - partnumber T serial
                    //2297504 Volvo 313H, 517H (Pack 3호기) Barcode 체계 변경
                    strLotId = strLotId.Contains("T") ? strLotId.Substring(0, strLotId.IndexOf('T')) : "";

                    txtpartNumber.Text = strLotId;
                    txtSerial.Text = strRefNo;
                    txtBatch.Text = strRefNo;

                    //모델 313H, 517H(Pack 3호기) Barcode 체계 변경 20170516
                    //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
                    //2019.04.07
                    //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H")
                    if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "519H")
                    {
                        bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P31491834     
                        bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S12031601
                        bcBatch.Text = "H" + txtBatch.Text;            //Batch No : H312031601
                    }
                }

                //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H")
                //{
                //    txtpartNumber.Text = "P" + strLotId;
                //    txtSerial.Text = "S" + strRefNo;
                //    txtBatch.Text = "H" + strRefNo;
                //}                    

                /*
                string strDateCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
                string strDateCodeMonth = "ABCDEFGHJKLM";

                if (strLotId.Length == 16)
                {
                    string strYY = strLotId.Substring(10, 1);
                    string strMM = strLotId.Substring(11, 1);

                    strLotId = string.Format("3{0:00}{1:00}{2:0000}", strDateCodeYear.IndexOf(strYY) + 1, strDateCodeMonth.IndexOf(strMM) + 1, strLotId.Substring(12, strLotId.Length - 12));

                txtpartNumber.Text = strRefNo;                       

                }
                //2189498 3호기_312H 포장 라벨 변경요청

                //2297504 Volvo 312H (Pack 3호기) Barcode 체계 변경
                if (strLotId.Length == 18)
                {
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);
                    strLotId = strLotId.Substring(11);
                    txtpartNumber.Text = strRefNo; 
                }

                //to-be
                if (strLotId.Length > 18)
                {
                    //strLotId = strLotId.Substring(strLotId.IndexOf('T') + 1);
                    strLotId = strLotId.Substring(11);
                    txtpartNumber.Text = strRefNo;
                }

                txtSerial.Text = strLotId;
                txtBatch.Text = strLotId;

                //2534222 3호기_313H 포장박스 라벨 Dock Gate 수정 기능 구현
                txtDock.Text = "NODATE";
                    //Clipboard.SetText(MakeZPLString());
 */
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dtpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dtpDate == null || dtpDate.SelectedDateTime == null)
                {
                    return;
                }

                if (cboProduct == null)
                {
                    return;
                }

                //날짜 선택시 Advice Note No, Date 정보를 가져와서 Text 박스에 세팅    

                txtDate.Text = "D" + dtpDate.SelectedDateTime.ToString("yyMMdd");
                //txtDate.Text = dtpDate.SelectedDateTime.ToString("yyMMdd");

                string line_no = getLine();

                txtAdvice.Text = line_no + dtpDate.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();

                //2019.04.07
                //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
                if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI" || tabItem.Header.ToString() == "519H")
                {
                    bcAdvice.Text = "N" + txtAdvice.Text;
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void dtpDate_SelectedDateChanged1(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpDate_SelectedDateChanged(null, null);
        }

        private DataRow[] selectText(DataTable dt, string item_code)
        {
            DataRow[] drs;

            drs = dt.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        #endregion Dataset / DataTable / DataRow

        #region Text
        private void txtAdvice_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtpartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtquantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }

        private void txtBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            setBacode(sender);
        }
        private void txtLogistic1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tabItem.Header.ToString() == "HORI")
            {
                //2017.10.03
                //ITEM_REF = txtLogistic1.Text;
                setBacode(sender);
            }
        }

        #endregion Text

        #region Combo
        private void cboProduct_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dtModelResult != null || dtModelResult.Rows.Count > 0)
            {
                DataRow[] dr = dtModelResult.Select("CBO_CODE = '" + Util.GetCondition(cboProduct) + "'");
                tabItem.Header = Util.NVC(dr[0]["MODEL"]);

                //2017.10.27
                //txtSeletedLabel.Text = Util.NVC(dr[0]["LABEL_CODE"]);
                //label_code = Util.NVC(dr[0]["LABEL_CODE"]);
                if (dtLabelCodes != null && dtLabelCodes.Rows.Count > 0)
                {
                    txtSeletedLabel.Text = dtLabelCodes.Rows[0]["LABEL_CODE1"].ToString();
                }

                setCombo_Out();
                //getValueSetting();              
            }
        }

        //2017.10.27
        private void cboLabel_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //2018.11.08
            //txtSeletedLabel.Text = cboLabel.SelectedValue.ToString();

            if (LoginInfo.CFG_SHOP_ID != "G481")
            {
                txtSeletedLabel.Text = cboLabel.SelectedValue.ToString();
                label_code = cboLabel.SelectedValue.ToString();
            }
            else
            {
                label_code = txtSeletedLabel.Text;
            }

            //label_code = cboLabel.SelectedValue.ToString();
        }

        private void cboProduct_Out_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //2017.10.27
            setCombo_Label();
        }
        #endregion Combo

        #endregion Event

        #region Mehod
        private void getLabelCode()
        {
            try
            {
                //테스트 후에 삭제
                string contry = string.Empty;

                if (LoginInfo.CFG_SHOP_ID == "A040")
                {
                    contry = "KOR";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G451")
                {
                    contry = "CNA";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G382")
                {
                    contry = "CMI";
                }
                else if (LoginInfo.CFG_SHOP_ID == "G481")
                {
                    contry = "POL";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = contry; // "KOR"; 테스트 후 변경

                RQSTDT.Rows.Add(dr);

                dtLabelCodes = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void setCombo_PROD()
        {
            try
            {
                string label_codes = string.Empty;

                if (dtLabelCodes != null && dtLabelCodes.Rows.Count > 0)
                {
                    label_codes = dtLabelCodes.Rows[0]["LABEL_CODE1"].ToString();

                    if (dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString();
                    }
                }
                else
                {
                    label_codes = "LBL0020";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_CODE"] = label_codes;

                RQSTDT.Rows.Add(dr);

                dtModelResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROJECTNAME_PRODID_COMBO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct.DisplayMemberPath = "CBO_NAME";
                cboProduct.SelectedValuePath = "CBO_CODE";
                cboProduct.ItemsSource = DataTableConverter.Convert(dtModelResult); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //2018-11-09
                txtSeletedLabel.Text = label_codes;

                //ComboStatus cs 
                //CommonCombo.AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void setCombo_Out()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["LABEL_CODE"] = Util.GetCondition(txtSeletedLabel);
                dr["PRODID"] = Util.GetCondition(cboProduct);

                RQSTDT.Rows.Add(dr);

                DataTable dtShipto = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABEL_SHIPTO", "INDATA", "OUTDATA", RQSTDT);

                cboProduct_Out.DisplayMemberPath = "CBO_NAME";
                cboProduct_Out.SelectedValuePath = "CBO_CODE";
                cboProduct_Out.ItemsSource = DataTableConverter.Convert(dtShipto); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //ComboStatus cs 
                //CommonCombo.AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboProduct_Out.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2017.10.27
        private void setCombo_Label()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["SHIPTO"] = Util.GetCondition(cboProduct_Out);

                RQSTDT.Rows.Add(dr);

                DataTable dtShipto = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_SHIPTO", "INDATA", "OUTDATA", RQSTDT);

                cboLabel.DisplayMemberPath = "CBO_NAME";
                cboLabel.SelectedValuePath = "CBO_CODE";
                cboLabel.ItemsSource = DataTableConverter.Convert(dtShipto);

                cboLabel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Get_Product_Lot()
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                //2018.11.30
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();                                 
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom); 
                dr["TODATE"] = Util.GetCondition(dtpDateTo);
                //2018.11.30
                dr["LOTID"] = (bool)radL.IsChecked ? txtSearchLotId.Text : null;
                dr["PALLETID"] = (bool)radP.IsChecked ? txtSearchLotId.Text : null;


                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgResult.ItemsSource = null;
                tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                Util.MessageException(ex);
            }
        }

        private string getLine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //2018.10.08
                RQSTDT.Columns.Add("EQSGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct); //dtpDateTo.SelectedDateTime.ToString();
                //2018.10.08
                //dr["PRODID"] = LoginInfo.CFG_EQSG_ID.ToString();
                //2018.10.29
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_WITH_PRODID_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return "8";
                }
                else
                {
                    return dtResult.Rows[0]["LINE_NO"].ToString();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getValueSetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = Util.GetCondition(txtSeletedLabel);// label_code; //dtpDateTo.SelectedDateTime.ToString();
                dr["SHIPTO_ID"] = Util.GetCondition(cboProduct_Out); //dtpDateTo.SelectedDateTime.ToString();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtTextResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtTextResult == null || dtTextResult.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox();

                    dtpDate_SelectedDateChanged(null, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getZpl(string I_ATTVAL, string LabelCode)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = label_code;
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                ////ITEM001=TEST1^ITEM002=TEST2

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: LabelCode
                                                     , sATTVAL: I_ATTVAL
                                                     );

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();

                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(testZpl);
                    //wndPopup.Show();

                    //Util.PrintLabel(FrameOperation, loadingIndicator, testZpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet(string labelCode)
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;
            string I_ATTVAL_MSD = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = labelCode;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData();

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region sample value 뿌림
                        /*
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();
                        */
                        #endregion

                        #region 화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }

                    if (tabItem.Header.ToString() == "HORI")
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {

                            #region 화면에서 입력된 값 뿌림                        
                            item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                            item_value = dtInput.Rows[0][item_code].ToString();
                            #endregion

                            if (item_code == "ITEM001" || item_code == "ITEM002" || item_code == "ITEM009" || item_code == "ITEM010" ||
                                item_code == "ITEM013" || item_code == "ITEM015" || item_code == "ITEM016")
                            {
                                if (item_code == "ITEM013")
                                {
                                    I_ATTVAL_MSD += item_code + "=" + "MSD Plug";
                                }
                                else if (item_code == "ITEM009")
                                {
                                    I_ATTVAL_MSD += item_code + "=" + "ITEM_REF";
                                }
                                else if (item_code == "ITEM010")
                                {
                                    I_ATTVAL_MSD += item_code + "=" + "PITEM_REF";
                                }
                                else
                                {
                                    I_ATTVAL_MSD += item_code + "=" + item_value;
                                }

                                //if(item_code != "ITEM016")
                                //{
                                //    I_ATTVAL_MSD += "^";
                                //}
                            }
                            else
                            {
                                I_ATTVAL_MSD += item_code + "=";
                            }

                            if (i < dtResult.Rows.Count - 1)
                            {
                                I_ATTVAL_MSD += "^";
                            }
                        }

                        MSD_Plug = I_ATTVAL_MSD;
                    }

                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion Method

        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        private void PrintProcess(Button btn)
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                
                btn.Content = ObjectDic.Instance.GetObjectName("취소");
                btn.Foreground = Brushes.White;
            }
            else
            {
                blPrintStop = true;                
                btn.Content = ObjectDic.Instance.GetObjectName("출력");
                btn.Foreground = Brushes.Red;
            }
        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    PrintAcess();
                }));
            }
            catch (Exception ex)
            {
                bkWorker.CancelAsync();
                blPrintStop = true;

                Util.AlertInfo(ex.Message);
            }
        }

        private void PrintAcess()
        {
            try
            {
                string I_ATTVAL = string.Empty;
                //2020.06.24
                //CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
                int tab_idx = tcMain.SelectedIndex;               
              
                btn = btnPrint;
                tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + "]";
                labelCode = label_code; 

                I_ATTVAL = labelItemsGet(labelCode);

                getZpl(I_ATTVAL, labelCode);

                if (tabItem.Header.ToString() == "HORI")
                {
                    zpl = SetReference(zpl);
                }

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (i + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }
                
                if(tabItem.Header.ToString() == "HORI")
                {
                    getZpl(MSD_Plug, labelCode);

                    zpl = SetReference_1(zpl);

                    if ((LoginInfo.USERID.Trim() == "ogong") || (LoginInfo.USERID.Trim() == "cnszhftm15") || (LoginInfo.USERID.Trim() == "everystreet"))
                    {
                        //2020.06.24
                        //wndPopup = new CMM_ZPL_VIEWER2(zpl);
                        //wndPopup.Show();

                        Preview popup = new Preview(labelCode, zpl);
                        popup.Show();
                    }

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }

                //ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private string SetReference(string _zpl)
        {
            string zpl_ref = string.Empty;
            string text = string.Empty;
            string bar = string.Empty;

            text = reference_txt.Replace("ITEM_REF", txtLogistic1.Text);
            //2017.10.09
            //bar = reference_bar.Replace("ITEM_REF", txtLogistic1.Text);

            //int test = _zpl.IndexOf("BATTERY PACK^FS");

            zpl_ref = _zpl.Insert(_zpl.IndexOf("^A0B,64,52^FO80,960^FDLondon Taxi Company^FS"), text + bar);
            zpl_ref = zpl_ref.Replace("^BY9,2.0^FO725,1190^B3B,N,96,N,N^FD^FS", "");

            zpl_ref = zpl_ref.Replace("^BY5,2.0^FO253,982^B3B,N,96,N,N^FD^FS", "");

            zpl_ref = zpl_ref.Replace("^BY4,2.0^FO1068,1057^B3B,N,96,N,N^FD^FS", "");
            zpl_ref = zpl_ref.Replace("^BY4,2.0^FO1068,240^B3B,N,96,N,N^FD^FS", "");

            return zpl_ref;
        }

        private string SetReference_1(string _zpl)
        {
            string zpl_ref = string.Empty;
            string text = string.Empty;
            string bar = string.Empty;

            text = reference_txt.Replace("ITEM_REF", "1E0002971AA");
            bar = reference_bar.Replace("ITEM_REF", "1E0002971AA");

            zpl_ref = _zpl.Replace("ITEM_REF", "1E0002971AA");

             //int test = _zpl.IndexOf("BATTERY PACK^FS");

            //zpl_ref = _zpl.Insert(_zpl.IndexOf("^A0B,64,52^FO80,960^FDLondon Taxi Company^FS"), text + bar);
            zpl_ref = zpl_ref.Replace("^BY9,2.0^FO725,1149^B3B,N,96,N,N^FD^FS", "");

            zpl_ref = zpl_ref.Replace("^BY5,2.0^FO253,941^B3B,N,96,N,N^FD^FS", "");

            zpl_ref = zpl_ref.Replace("^BY4,2.0^FO1068,1016^B3B,N,96,N,N^FD^FS", "");
            zpl_ref = zpl_ref.Replace("^BY4,2.0^FO1068,240^B3B,N,96,N,N^FD^FS", "");

            zpl_ref = zpl_ref.Replace("^BY5,2.0^FO496,934^B3B,N,96,N,N^FDPITEM_REF^FS",""); //^BY5,2.0^FO496,934^B3B,N,96,N,N^FDP1E0002971AA^FS

            zpl_ref = zpl_ref.Replace("^BY5,2.0^FO496,934^B3B,N,96,N,N^FD", "^BY5,2.0^FO496,750^B3B,N,96,N,N^FD");

            return zpl_ref;
        }


        private DataTable getInputData()
        {
            DataTable dt = new DataTable();
          
            dt.TableName = "INPUTDATA";
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));
            dt.Columns.Add("ITEM003", typeof(string));
            dt.Columns.Add("ITEM004", typeof(string));
            dt.Columns.Add("ITEM005", typeof(string));
            dt.Columns.Add("ITEM006", typeof(string));
            dt.Columns.Add("ITEM007", typeof(string));
            dt.Columns.Add("ITEM008", typeof(string));
            dt.Columns.Add("ITEM009", typeof(string));
            dt.Columns.Add("ITEM010", typeof(string));
            dt.Columns.Add("ITEM011", typeof(string));
            dt.Columns.Add("ITEM012", typeof(string));
            dt.Columns.Add("ITEM013", typeof(string));
            dt.Columns.Add("ITEM014", typeof(string));
            dt.Columns.Add("ITEM015", typeof(string));
            dt.Columns.Add("ITEM016", typeof(string));
            dt.Columns.Add("ITEM017", typeof(string));
            dt.Columns.Add("ITEM018", typeof(string));
            dt.Columns.Add("ITEM019", typeof(string));
            dt.Columns.Add("ITEM020", typeof(string));
            dt.Columns.Add("ITEM021", typeof(string));

            DataRow dr = dt.NewRow();
            dr["ITEM001"] = Util.GetCondition(txtReceive); //VOLVO TORSLANDA MONTERING
            dr["ITEM002"] = Util.GetCondition(txtDock); //TCV
            dr["ITEM003"] = Util.GetCondition(txtAdvice); //61606292
            dr["ITEM004"] = bcAdvice.Text; //61606292
            dr["ITEM005"] = Util.GetCondition(txtSupplierAddress); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition(txtNetWeight); //115
            dr["ITEM007"] = Util.GetCondition(txtGrossWeight); //160
            dr["ITEM008"] = Util.GetCondition(txtBoxes); //1
            dr["ITEM009"] = Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM010"] = bcpartNumber.Text; //31491834
            dr["ITEM011"] = Util.GetCondition(txtquantity); //1
            dr["ITEM012"] = bcquantity.Text; //1
            dr["ITEM013"] = Util.GetCondition(txtDescription1); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition(txtDescription2); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM016"] = bcSupplierID.Text; //GE2PB
            dr["ITEM017"] = Util.GetCondition(txtDate); //D160629
            dr["ITEM018"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM019"] = bcSerial.Text; //616242017
            dr["ITEM020"] = Util.GetCondition(txtBatch); //616242017
            dr["ITEM021"] = bcBatch.Text; //616242017
            dt.Rows.Add(dr);           
           
            return dt;            
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button btn = new Button();
            btn = btnPrint;

            btn.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btn.Foreground = Brushes.White;         
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                //2020.06.24
                //if ((LoginInfo.USERID.Trim() == "cnswkdakscjf") || (LoginInfo.USERID.Trim() == "ogong"))
                if ((LoginInfo.USERID.Trim() == "ogong") || (LoginInfo.USERID.Trim() == "cnszhftm15") || (LoginInfo.USERID.Trim() == "everystreet"))
                {
                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZpl);
                    //wndPopup.Show();

                    Preview popup = new Preview("", zpl);
                    popup.Show();

                }
                //wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*
                private string MakeZPLString()
                {
                    try
                    {
                        string strResult = string.Empty;
                        string strBackRollCheck = "B";

                        strResult = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH0,0^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                        strResult += "^POI";

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            //2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가
                            strResult += string.Format("^A0R,70,55^FO1200,30^CI0^FD{0}^FS", txtReceive.Text); //VOLVO TORSLANDA MONTERING
                            strResult += string.Format("^A0R,70,60^FO1090,230^CI0^FD{0}^FS", txtAdvice.Text); //TTLC1601
                            strResult += string.Format("^BY4,2.8^FO990,50^B3R,N,104,N,N^FDN{0}^FS", txtAdvice.Text); //TTLC1601(N)
                            strResult += string.Format("^A0R,138,0^FO820,230^CI0^FD{0}^FS", txtpartNumber.Text); //50634477
                            strResult += string.Format("^BY4,2.8^FO740,50^B3R,N,104,N,N^FDP{0}^FS", txtpartNumber.Text); //50634477(P)
                            strResult += string.Format("^A0R,138,0^FO578,230^CI0^FD{0}^FS", txtquantity.Text); //1
                            strResult += string.Format("^BY5,2.8^FO505,50^B3R,N,104,N,N^FDQ{0}^FS", txtquantity.Text); //1(Q)
                            strResult += string.Format("^A0R,50,0^FO440,230^CI0^FD{0}^FS", txtSupplierID.Text); //GE2PB
                            strResult += string.Format("^BY4,2.8^FO345,50^B3R,N,104,N,N^FDV{0}^FS", txtSupplierID.Text); //GE2PB(V)
                            strResult += string.Format("^A0R,50,0^FO270,230^CI0^FD{0}^FS", txtSerial.Text); //312031601
                            strResult += string.Format("^BY3,2.8^FO170,50^B3R,N,104,N,N^FDS{0}^FS", txtSerial.Text); //312031601(S)


                            strResult += string.Format("^A0R,138,138^FO1160,1100^CI0^FD{0}^FS", txtDock.Text); //TVV
                            strResult += string.Format("^A0R,50,0^FO1080,808^CI0^FD{0}^FS", txtSupplierAddress.Text); //LG Chem, Ltd.
                            strResult += string.Format("^A0R,70,0^FO960,880^CI0^FD{0}^FS", txtNetWeight.Text); //150
                            strResult += string.Format("^A0R,70,0^FO960,1180^CI0^FD{0}^FS", txtGrossWeight.Text); //180
                            strResult += string.Format("^A0R,70,0^FO960,1480^CI0^FD{0}^FS", txtBoxes.Text); //1
                            strResult += string.Format("^A0R,70,0^FO623,802^CI0^FD{0}^FS", txtDescription1.Text); //BATTERY PACK

                            if ((bool)rdb515H.IsChecked)
                                strResult += string.Format("^A0R,35,30^FO640,1280^CI0^FD{0}^FS", txtDescription2.Text); //355V,25.9A, 6,500Wh(Usable)
                            else
                                strResult += string.Format("^A0R,50,40^FO640,1280^CI0^FD{0}^FS", txtDescription2.Text); //375V, 30Ah, 11,250Wh

                            //2203316 312H 포장박스라벨 수정요청
                            strResult += string.Format("^A0R,50,0^FO523,880^CI0^FD{0}^FS", txtLogistic1.Text); //Master Label Number
                            strResult += string.Format("^A0R,50,0^FO473,880^CI0^FD{0}^FS", txtLogistic2.Text); //TTLC1601
                                                                                                               //2203316 312H 포장박스라벨 수정요청

                            strResult += string.Format("^A0R,70,0^FO330,850^CI0^FD{0}^FS", txtDate.Text); //D120323
                            strResult += string.Format("^A0R,50,0^FO270,980^CI0^FD{0}^FS", txtBatch.Text);  //312031601
                            strResult += string.Format("^BY3,2.8^FO170,850^B3R,N,104,N,N^FDH{0}^FS", txtBatch.Text); //312031601(H)
                                                                                                                     //2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가           
                        }));

                        //strResult += "^POI";
                        ////2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가
                        //strResult += string.Format("^A0R,70,55^FO1200,30^CI0^FD{0}^FS", txt312H01.Text); //VOLVO TORSLANDA MONTERING
                        //strResult += string.Format("^A0R,70,60^FO1090,230^CI0^FD{0}^FS", txt312H03.Text); //TTLC1601
                        //strResult += string.Format("^BY4,2.8^FO990,50^B3R,N,104,N,N^FDN{0}^FS", txt312H03.Text); //TTLC1601(N)
                        //strResult += string.Format("^A0R,138,0^FO820,230^CI0^FD{0}^FS", txt312H08.Text); //50634477
                        //strResult += string.Format("^BY4,2.8^FO740,50^B3R,N,104,N,N^FDP{0}^FS", txt312H08.Text); //50634477(P)
                        //strResult += string.Format("^A0R,138,0^FO578,230^CI0^FD{0}^FS", txt312H09.Text); //1
                        //strResult += string.Format("^BY5,2.8^FO505,50^B3R,N,104,N,N^FDQ{0}^FS", txt312H09.Text); //1(Q)
                        //strResult += string.Format("^A0R,50,0^FO440,230^CI0^FD{0}^FS", txt312H12.Text); //GE2PB
                        //strResult += string.Format("^BY4,2.8^FO345,50^B3R,N,104,N,N^FDV{0}^FS", txt312H12.Text); //GE2PB(V)
                        //strResult += string.Format("^A0R,50,0^FO270,230^CI0^FD{0}^FS", txt312H14.Text); //312031601
                        //strResult += string.Format("^BY3,2.8^FO170,50^B3R,N,104,N,N^FDS{0}^FS", txt312H14.Text); //312031601(S)


                        //strResult += string.Format("^A0R,138,138^FO1160,1100^CI0^FD{0}^FS", txt312H02.Text); //TVV
                        //strResult += string.Format("^A0R,50,0^FO1080,808^CI0^FD{0}^FS", txt312H04.Text); //LG Chem, Ltd.
                        //strResult += string.Format("^A0R,70,0^FO960,880^CI0^FD{0}^FS", txt312H05.Text); //150
                        //strResult += string.Format("^A0R,70,0^FO960,1180^CI0^FD{0}^FS", txt312H06.Text); //180
                        //strResult += string.Format("^A0R,70,0^FO960,1480^CI0^FD{0}^FS", txt312H07.Text); //1
                        //strResult += string.Format("^A0R,70,0^FO623,802^CI0^FD{0}^FS", txt312H10.Text); //BATTERY PACK
                        //if ((bool)rdb515H.IsChecked)
                        //    strResult += string.Format("^A0R,35,30^FO640,1280^CI0^FD{0}^FS", txt312H11.Text); //355V,25.9A, 6,500Wh(Usable)
                        //else
                        //    strResult += string.Format("^A0R,50,40^FO640,1280^CI0^FD{0}^FS", txt312H11.Text); //375V, 30Ah, 11,250Wh

                        ////2203316 312H 포장박스라벨 수정요청
                        //strResult += string.Format("^A0R,50,0^FO523,880^CI0^FD{0}^FS", txtLogisticRef1.Text); //Master Label Number
                        //strResult += string.Format("^A0R,50,0^FO473,880^CI0^FD{0}^FS", txtLogisticRef2.Text); //TTLC1601
                        ////2203316 312H 포장박스라벨 수정요청

                        //strResult += string.Format("^A0R,70,0^FO330,850^CI0^FD{0}^FS", txt312H13.Text); //D120323
                        //strResult += string.Format("^A0R,50,0^FO270,980^CI0^FD{0}^FS", txt312H15.Text);  //312031601
                        //strResult += string.Format("^BY3,2.8^FO170,850^B3R,N,104,N,N^FDH{0}^FS", txt312H15.Text); //312031601(H)
                        ////2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가

                        strResult += "^PQ1,0,1,Y^XZ";

                        //Clipboard.SetText(strResult);
                        //File.WriteAllText("312H" + "_BOX.txt", strResult.Replace("^FS", "^FS" + Environment.NewLine));
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            Util.AlertInfo(strResult.Replace("^FS", "^FS"));
                        }));

                        return strResult;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
        */

        private void setBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

                //2019.04.07
                //if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
                if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI" || tabItem.Header.ToString() == "519H")
                {
                    switch (tbBox.Name)
                    {
                        case "txtAdvice":
                            bcAdvice.Text = "N" + tbBox.Text;
                            break;
                        case "txtpartNumber":
                            bcpartNumber.Text = "P" + tbBox.Text;
                            break;
                        case "txtquantity":
                            bcquantity.Text = "Q" + tbBox.Text;
                            break;
                        case "txtSupplierID":
                            bcSupplierID.Text = "V" + tbBox.Text;
                            break;
                        case "txtSerial":
                            bcSerial.Text = "S" + tbBox.Text;
                            break;
                        case "txtBatch":
                            bcBatch.Text = "H" + tbBox.Text;
                            break;
                        case "txtLogistic1":
                            if (tabItem.Header.ToString() == "HORI")
                            {
                                bcLogistic1.Text =txtLogistic1.Text;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (tbBox.Name)
                    {
                        case "txtAdvice":
                            bcAdvice.Text = tbBox.Text;
                            break;
                        case "txtpartNumber":
                            bcpartNumber.Text = tbBox.Text;
                            break;
                        case "txtquantity":
                            bcquantity.Text = tbBox.Text;
                            break;
                        case "txtSupplierID":
                            bcSupplierID.Text = tbBox.Text;
                            break;
                        case "txtSerial":
                            bcSerial.Text = tbBox.Text;
                            break;
                        case "txtBatch":
                            bcBatch.Text = tbBox.Text;
                            break;
                        default:
                            break;
                    }
                }                
            }
            catch (Exception ex)
            {

                Util.AlertInfo(ex.Message);
            }
        }

        private void setTexBox()
        {
            try
            {
                txtReceive.Text = "VOLVO TORSLANDA MONTERING";
                txtAdvice.Text = "31607151";
                txtDock.Text = " TVV ";
                txtSupplierAddress.Text = "LG Chem, Ltd";
                txtNetWeight.Text = "150";
                txtGrossWeight.Text = "180";
                txtBoxes.Text = "1";
                txtpartNumber.Text = "31407014";
                txtquantity.Text = "1";
                txtSupplierID.Text = "GE2PB";
                txtSerial.Text = "312031601";
                if (tabItem.Header.ToString() == "HORI")
                {
                    txtLogistic1.Text = "logic";
                    txtLogistic2.Text = "reference";
                }
                else
                {
                    txtLogistic1.Text = "1E0001683AB";
                    bcLogistic1.Text = "1E0001683AB";
                    bcLogistic1.Visibility = Visibility.Visible;
                    txtLogistic2.Visibility = Visibility.Hidden;
                }              
                txtDate.Text = "D160629";
                txtBatch.Text = "312031601";
                txtDescription1.Text = "BATTERY PACK";
                txtDescription2.Text = "375V, 30Ah, 11,250Wh";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}