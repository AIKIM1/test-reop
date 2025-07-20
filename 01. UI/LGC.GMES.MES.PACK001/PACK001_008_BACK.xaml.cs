/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_008_BACK : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        System.ComponentModel.BackgroundWorker bkWorker;

        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = "LBL0020";
        string zpl = string.Empty;

        public PACK001_008_BACK()
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
            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now ;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;         

            txtDate.Text = PrintOutDate(DateTime.Now);  //txtZpl018.Text = PrintOutDate(DateTime.Now);
            //dtpDate_SelectedDateChanged(null, null); //dtp312HDay_ValueChanged(null, null); dtpDate

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            setTexBox();
        }
        #endregion

        #region Event
        private void PACK001_008_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_Loaded;
        }

        private void btnAdvice_Click(object sender, RoutedEventArgs e)
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
            }
            else
            {
                return;
            }
        }

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

                //2189498 3호기_312H 포장 라벨 변경요청                   

                string strLotId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();

                //2297504 Volvo 312H (Pack 3호기) Barcode 체계 변경
                //string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, strLotId.IndexOf('T')) : "";
                string strRefNo = strLotId.Contains("T") ? strLotId.Substring(0, 11) : "";


                txtpartNumber.Text = strRefNo;
                txtSerial.Text = strLotId;
                txtBatch.Text = strRefNo;

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
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void dtpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dtpDate == null || dtpDate.SelectedDateTime == null)
            {
                return;
            }

            //날짜 선택시 Advice Note No, Date 정보를 가져와서 Text 박스에 세팅            
            txtDate.Text = "D" + dtpDate.SelectedDateTime.ToString("yyyyMMdd");           

            if (rdb312H == null || rdb313H == null || rdb515H == null)
            {
                return;
            }

            if ((bool)rdb515H.IsChecked)
                txtAdvice.Text = "6" + dtpDate.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();
            else if ((bool)rdb313H.IsChecked)
                txtAdvice.Text = "7" + dtpDate.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();
            else
                txtAdvice.Text = "3" + dtpDate.SelectedDateTime.ToString("yyMMdd") + nbProductBox.Value.ToString();
        }

        private void dtpDate_SelectedDateChanged1(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpDate_SelectedDateChanged(null, null);
        }

        

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //조회 조건들에 해당하는 LOT_ID와 PALLET_ID 가져와서 Grid에 바인딩
                if ((bool)this.rdb312H.IsChecked == false && (bool)this.rdb313H.IsChecked == false && (bool)this.rdb515H.IsChecked == false)
                {                    
                    ms.AlertWarning("추가"); //312H,313H,515H 중 하나를 선택하여 주세요"
                    return;
                }
                else
                {
                    string _Model = string.Empty;
                    if ((bool)this.rdb312H.IsChecked)
                    {
                        _Model = rdb312H.Content.ToString();
                        txtNetWeight.Text = "150";
                        txtGrossWeight.Text = "180";
                        txtDescription2.Text = "375V, 30Ah, 11,250Wh";
                        //txt312H05.Text = "150";
                        //txt312H06.Text = "180";
                        //txt312H11.Text = "375V, 30Ah, 11,250Wh";
                    }
                    else if ((bool)this.rdb313H.IsChecked)
                    {
                        _Model = rdb313H.Content.ToString();
                        txtNetWeight.Text = "150";
                        txtGrossWeight.Text = "180";
                        txtDescription2.Text = "375V, 30Ah, 11,250Wh";
                    }
                    //2745485 Pack6호_515H BMA 포장박스 라벨 출력 기능 구현
                    else if ((bool)this.rdb515H.IsChecked)
                    {
                        _Model = rdb515H.Content.ToString();
                        txtNetWeight.Text = "115";
                        txtGrossWeight.Text = "160";
                        txtDescription2.Text = "355V,25.9A, 6,500Wh(Usable)";
                    }

                    dtpDate_SelectedDateChanged(null, null);
                    Get_Product_Lot(_Model);
                }
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
            
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintProcess();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }           
        }

        private void rdb515H_CheckedChanged(object sender, RoutedEventArgs e)
        {
            dtpDate_SelectedDateChanged(null, null);
        }
       
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
        #endregion

        #region Mehod

        private void Get_Product_Lot(string Model)
        {
            DataTable dtResult;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                switch (Model)
                {
                    case "312H":
                        //2534222 3호기_313H 포장박스 라벨 Dock Gate 수정 기능 구현                    
                        dr["PRODID"] = "AMDAU0068A"; // Model;
                        break;
                    case "313H":
                        //2534222 3호기_313H 포장박스 라벨 Dock Gate 수정 기능 구현                       
                        dr["PRODID"] = null; //Model;
                        break;
                    //2745485 Pack6호_515H BMA 포장박스 라벨 출력 기능 구현
                    case "515H":
                        //0000000 6호기_515H 포장박스 라벨                        
                        dr["PRODID"] = "APRSAKANG-A1"; //Model;
                        break;
                }

                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);  //dtpDateFrom.SelectedDateTime.ToString();
                dr["TODATE"] = Util.GetCondition(dtpDateTo); //dtpDateTo.SelectedDateTime.ToString();

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
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }


        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        private void PrintProcess()
        {
            if (!bkWorker.IsBusy)
            {
                blPrintStop = false;
                bkWorker.RunWorkerAsync();
                btnPrint.Content = ObjectDic.Instance.GetObjectName("취소");
                btnPrint.Foreground = Brushes.White;
            }
            else
            {
                btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
                blPrintStop = true;
                btnPrint.Foreground = Brushes.Red;
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
                tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + "]";
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;

                I_ATTVAL = labelItemsGet();

                getZpl(I_ATTVAL);

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (i + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }
                //wndPopup = new CMM_ZPL_VIEWER2(zpl);
                //wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getZpl(string I_ATTVAL)
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
                                                      sLBCD: label_code
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

        private string labelItemsGet()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            DataTable dtInput;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = label_code;

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
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
            dr["ITEM004"] = Util.GetCondition(txtAdvice); //61606292
            dr["ITEM005"] = Util.GetCondition(txtSupplierAddress); // LG Chem, Ltd
            dr["ITEM006"] = Util.GetCondition(txtNetWeight); //115
            dr["ITEM007"] = Util.GetCondition(txtGrossWeight); //160
            dr["ITEM008"] = Util.GetCondition(txtBoxes); //1
            dr["ITEM009"] = Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM010"] = Util.GetCondition(txtpartNumber); //31491834
            dr["ITEM011"] = Util.GetCondition(txtquantity); //1
            dr["ITEM012"] = Util.GetCondition(txtquantity); //1

            dr["ITEM013"] = Util.GetCondition(txtDescription1); //BATTERY PACK
            dr["ITEM014"] = Util.GetCondition(txtDescription2); //355V,25.9A,6500Wh(Usable)
            dr["ITEM015"] = Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM016"] = Util.GetCondition(txtSupplierID); //GE2PB
            dr["ITEM017"] = Util.GetCondition(txtDate); //D160629
            dr["ITEM018"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM019"] = Util.GetCondition(txtSerial); //616242017
            dr["ITEM020"] = Util.GetCondition(txtBatch); //616242017
            dr["ITEM021"] = Util.GetCondition(txtBatch); //616242017
            dt.Rows.Add(dr);

            return dt;
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btnPrint.Foreground = Brushes.White;
           
            ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                CMM_ZPL_VIEWER2 wndPopup;

                wndPopup = new CMM_ZPL_VIEWER2(sZpl);

                Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                //wndPopup.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        private void setBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

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
                txtLogistic1.Text = "logic";
                txtLogistic2.Text = "reference";
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
        #endregion

    }
}

