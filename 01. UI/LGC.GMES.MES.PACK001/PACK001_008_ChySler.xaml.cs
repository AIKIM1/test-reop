/*************************************************************************************
 Created Date : 2017.03.29
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.29  DEVELOPER : Initial Created.
  2022.10.06  : CSR- 
  2024.05.28  김민석 ESMI 중복 ID 입력 에러 메시지 팝업 무조건 발생하여 해당 현상 해소를 위한 LOADING INDICATOR, DOEVENT 주석 처리 [요청번호] E20240514-000398 



 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_008_ChySler : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtModelResult;
        DataTable dtMasterResult;
        DataTable dtChrySlerProd;
        DataTable dtChrySlerMatrial;
        DataTable dtTextResult;
        System.ComponentModel.BackgroundWorker bkWorker;

        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = "LBL0020";
        string label_code_Master = "LBL0060"; //Master
        string label_code_ChrslerBMA = "LBL0086"; //ChrslerProd BMA Barcode
        string label_code_ChrslerProd = "LBL0061"; //ChrslerProd
        string label_code_ChrslerMaterial = "LBL0062"; //ChrslerMaterial
        string zpl = string.Empty;
        bool print_all = false;

        string seleted_lot = string.Empty;
        string seleted_eqsgid = string.Empty;
        string test_zpl_Crsler = "";
        bool multiPrint = false;

        public PACK001_008_ChySler()
        {
            InitializeComponent();

            this.Loaded += PACK001_008_ChySler_Loaded;
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
            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            string labelCode = string.Empty;
            DataTable dt = new DataTable();
            C1.WPF.C1ComboBox cb = new C1.WPF.C1ComboBox();

            dtpDate_Mp.SelectedDataTimeChanged += dtpDate_Mp_SelectedDataTimeChanged;

            dtpDate_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom_Mp.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;

            tbPalletHistory_cnt_Mp.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            labelCode = label_code_ChrslerProd;
            dt = dtChrySlerProd;
            cb = cboProduct_Mp;

            clearTextBox_CrySler_Prod();

            setCombo_Chrysler_Tab(0, labelCode, dt, cb);

            //*woosuck 초기 라벨 아이템값 바인딩 하지 않음
            //getValueSetting_Mp();

            //dtpDate_Mp.SelectedDataTimeChanged += dtpDate_Mp_SelectedDataTimeChanged;

            txtPrintIDInput.Focus();
        }

        private void dtpDate_Mp_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = dtpDate_Mp.SelectedDateTime;

            //DateTime dt = DateTime.Now.AddMonths(8); //두자리 테스트

            //tbDateMfg_Mp.Text = dt.ToString("ddMMMyyyy"); //월 약어 이름...지원안해줘서....아래로 바꿈

            string dd = dt.ToString("dd");
            string MM = dt.ToString("MMM").ToUpper();

            if (MM != "" && MM.Length < 3)
            {
                MM = chage_date(MM);
            }

            string yyyy = dt.ToString("yyyy");


            tbDateMfg_Mp.Text = dd + MM + yyyy;

            //tbDateMfg_Mp.Text = dt.ToString("ddMMMyyyy");


        }

        private string chage_date(string MM)
        {
            string return_MM = string.Empty;

            switch (MM)
            {
                case "1":
                    return_MM = "JAN";
                    break;
                case "2":
                    return_MM = "FEB";
                    break;
                case "3":
                    return_MM = "MAR";
                    break;
                case "4":
                    return_MM = "APR";
                    break;
                case "5":
                    return_MM = "MAY";
                    break;
                case "6":
                    return_MM = "JUN";
                    break;
                case "7":
                    return_MM = "JUL";
                    break;
                case "8":
                    return_MM = "AUG";
                    break;
                case "9":
                    return_MM = "SEP";
                    break;
                case "10":
                    return_MM = "OCT";
                    break;
                case "11":
                    return_MM = "NOV";
                    break;
                case "12":
                    return_MM = "DEC";
                    break;

            }

            return return_MM;
        }

        private string ToMonthName(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }



        #endregion

        private void setCombo_Chrysler_Tab(int tab_idx, string labelCode, DataTable dt, C1.WPF.C1ComboBox cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = labelCode; //
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; //

                RQSTDT.Rows.Add(dr);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROJECTNAME_PRODID_COMBO", "INDATA", "OUTDATA", RQSTDT);

                //*woosuck 제품 코드 콤보의 -- Select Item -- 항목 추가
                //DataRow drBlank = dt.NewRow();
                //drBlank.ItemArray = new object[] { "-- Select Item --", "", "", "" };
                //dt.Rows.InsertAt(drBlank, 0);

                cb.DisplayMemberPath = "CBO_NAME";
                cb.SelectedValuePath = "CBO_CODE";
                cb.ItemsSource = DataTableConverter.Convert(dt); //AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                //ComboStatus cs 
                //CommonCombo.AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        //*woosuck Scan Lot - Mapping Lot으로 찾음, 처음 투입 Lot으로 인쇄 정보 추출
        private string getScanLotInfor(string sLotid, out string sProdId)
        {
            string sRtnLotid = string.Empty;
            sProdId = string.Empty;

            try
            {
                string sPartNo = tbPartCustP_Mp.Text.Trim();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LOTID"] = sLotid;
                INDATA.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "INDATA", "OUTDATA", INDATA);

                if (dt != null && dt.Rows.Count > 0)
                {
                    sRtnLotid = dt.Rows[0]["LOTID"].ToString().Trim();
                    sProdId = dt.Rows[0]["PRODID"].ToString().Trim();
                }
            }
            catch
            {
                sRtnLotid = string.Empty;
            }

            return sRtnLotid;
        }

        //*woosuck Commoncode정보에서 RU Partnumber 정보 추출 (동별 공통코드 : ESMI_PACK_RU_PARTNO)
        private string getPartnumber_desc(string sPartnumber)
        {
            string sPartnumberDesc = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ESMI_PACK_RU_PARTNO";
                dr["COM_CODE"] = sPartnumber.Trim();
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt != null && dt.Rows.Count > 0)
                {
                    sPartnumberDesc = dt.Rows[0]["COM_CODE_NAME"].ToString();
                }
                else
                {
                    sPartnumberDesc = sPartnumber;
                }


            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

            return sPartnumberDesc;
        }

        //*woosuck LOT의 마지막 발행 정보와 현 발행 Part No 비교
        private bool chkLastPartnumberOfLot(string stLotID, out string strErrMsg, string strPN = null)
        {
            bool bResult = false;
            strErrMsg = string.Empty;

            try
            {
                string sPartNo = tbPartCustP_Mp.Text.Trim();
                string strLastPN = string.Empty;

                strLastPN = string.IsNullOrWhiteSpace(strPN) ? getLastPartnumberOfLot(stLotID) : strPN;

              
                if (string.IsNullOrEmpty(strLastPN))
                {
                    strErrMsg = "SFU8523";
                }
                else if (sPartNo.Equals(strLastPN) || (string.IsNullOrEmpty(sPartNo) && !string.IsNullOrEmpty(strLastPN)))
                {
                    bResult = true;
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

            return bResult;
        }

        //*woosuck 발행 정보 추출 DA_PRD_SEL_TB_SFC_LABEL_PRT_HIST_LAST
        private string getLastPartnumberOfLot(string stLotID)
        {
            string sLastPN = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = stLotID;
                dr["LABEL_CODE"] = label_code_ChrslerBMA;
                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_LABEL_PRT_HIST_LAST", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt != null && dt.Rows.Count > 0)
                {
                    sLastPN = dt.Rows[0]["PRT_ITEM02"].ToString();
                }
            }
            catch { }

            return sLastPN;
        }
        //*woosuck 제품ID로 라벨 정보 불러오기
        private void getValueSetting_Chrysler(string sProdID, string sPartNo)
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
                dr["PRODID"] = sProdID;
                dr["LABEL"] = label_code_ChrslerProd; //dtpDateTo.SelectedDateTime.ToString();
                dr["SHIPTO_ID"] = null; //dtpDateTo.SelectedDateTime.ToString();F
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtChrySlerProd = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtChrySlerProd == null || dtChrySlerProd.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox_CrySler_Prod(sPartNo);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Event
        private void PACK001_008_ChySler_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_ChySler_Loaded;
        }

        private DataRow[] selectText(DataTable dt, string item_code)
        {
            DataRow[] drs;

            drs = dt.Select("ITEM_CODE = '" + item_code + "'");
            return drs;
        }

        private string returnString(DataTable dt, string item_code)
        {
            return selectText(dt, item_code).Length > 0 ? Util.NVC(selectText(dt, item_code)[0]["ITEM_VALUE"]) : "";
        }

        #endregion

        #region Mehod

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

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct_Mp);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom_Mp);
                dr["TODATE"] = Util.GetCondition(dtpDateTo_Mp);

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgResult_Mp.ItemsSource = null;
                tbPalletHistory_cnt_Mp.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgResult_Mp, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbPalletHistory_cnt_Mp, Util.NVC(dtResult.Rows.Count));
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
                CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
                int tab_idx = 0;
                string error_prod = string.Empty;
                TextBlock tbprint_cnt = new TextBlock();
                C1NumericBox nb = new C1NumericBox();

                if (print_all)
                {
                    for (int i = 0; i < tcMain.Items.Count; i++)
                    {
                        tab_idx = i;

                        if (tab_idx == 0) //chrysler(Prod)
                        {
                            btn = btnPrint_Mp;
                            tbprint_cnt = tbPrint2_cnt_Mp;
                            nb = nbPrintCnt_Mp;
                            labelCode = label_code_ChrslerProd;
                            error_prod = "chrysler(Prod)";
                        }
                        else if (tab_idx == 1) //Master
                        {
                            btn = btnPrint_M;
                            tbprint_cnt = tbPrint2_cnt_M;
                            nb = nbPrintCnt_M;
                            labelCode = label_code_Master;
                            error_prod = "Master";

                        }
                        else if (tab_idx == 2) //Chrysler(Material)
                        {
                            btn = btnPrint_Mt;
                            tbprint_cnt = tbPrint2_cnt_Mt;
                            nb = nbPrintCnt_Mt;
                            labelCode = label_code_ChrslerMaterial;
                            error_prod = "Chrysler(Material)";
                        }


                        I_ATTVAL = labelItemsGet(labelCode, tab_idx);

                        if (I_ATTVAL == "")
                        {
                            ms.AlertWarning(error_prod + " : NO PROD"); //출력 data를 가져오지 못했습니다.
                            return;
                        }

                        getZpl(I_ATTVAL, labelCode, tab_idx);

                        if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                        {
                            //zpl = new TextRange(txtTextMaster.Document.ContentStart, txtTextMaster.Document.ContentEnd).Text;

                            wndPopup = new CMM_ZPL_VIEWER2(zpl);
                            wndPopup.Show();
                        }


                        for (int j = 0; j < nb.Value; j++)
                        {
                            if (blPrintStop) break;

                            Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                            tbprint_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (j + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    tab_idx = tcMain.SelectedIndex;

                    if (tab_idx == 0) //chrysler(Prod)
                    {
                        btn = btnPrint_Mp;
                        tbprint_cnt = tbPrint2_cnt_Mp;
                        nb = nbPrintCnt_Mp;
                        labelCode = label_code_ChrslerProd;
                    }
                    else if (tab_idx == 1) //Master
                    {
                        btn = btnPrint_M;
                        tbprint_cnt = tbPrint2_cnt_M;
                        nb = nbPrintCnt_M;
                        labelCode = label_code_Master;

                    }
                    else if (tab_idx == 2) //Chrysler(Material)
                    {
                        btn = btnPrint_Mt;
                        tbprint_cnt = tbPrint2_cnt_Mt;
                        nb = nbPrintCnt_Mt;
                        labelCode = label_code_ChrslerMaterial;
                    }

                    if (multiPrint)
                    {
                        //Lot List를 가져옴
                        TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                        string sText = textRange.Text;
                        string pre_prodid = string.Empty;
                        DataTable temp = new DataTable();

                        sText = sText.Replace("\r\n", "/").Replace(" ", "");

                        if (sText == "/")
                        {
                            return;
                        }
                        string[] lots = sText.Split('/');

                        for (int i = 0; i < lots.Length; i++)
                        {
                            if (lots[i].ToString() == "")
                            {
                                lots[i] = null;
                            }
                        }

                        for (int i = 0; i < lots.Length; i++)
                        {
                            if (lots[i] != null)
                            {
                                setUiParameters(lots[i].ToString()); //lot의 정보를 가지고 UI LAYOUT 세팅

                                I_ATTVAL = labelItemsGet(labelCode, tab_idx);

                                if (I_ATTVAL == "")
                                {
                                    ms.AlertWarning("NO PROD"); //출력 data를 가져오지 못했습니다.
                                    continue;
                                }

                                getZpl(I_ATTVAL, labelCode, tab_idx);

                                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                                if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                                {
                                    wndPopup = new CMM_ZPL_VIEWER2(zpl);
                                    //wndPopup.Show();
                                }
                            }
                        }
                    }
                    else
                    {
                        I_ATTVAL = labelItemsGet(labelCode, tab_idx);

                        if (I_ATTVAL == "")
                        {
                            ms.AlertWarning("NO PROD"); //출력 data를 가져오지 못했습니다.
                            return;
                        }

                        getZpl(I_ATTVAL, labelCode, tab_idx);

                        for (int j = 0; j < nb.Value; j++)
                        {
                            if (blPrintStop) break;

                            Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                            tbprint_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (j + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                            System.Threading.Thread.Sleep(1000);

                            if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                            {
                                //if (tab_idx == 1)
                                //{
                                //    txtTextMaster.Document.Blocks.Add(new Paragraph(new Run(zpl)));
                                //}
                                //else if (tab_idx == 2)
                                //{
                                //    txtTextProd.Document.Blocks.Add(new Paragraph(new Run(zpl)));
                                //}

                                //zpl = new TextRange(txtTextMaster.Document.ContentStart, txtTextMaster.Document.ContentEnd).Text;

                                wndPopup = new CMM_ZPL_VIEWER2(zpl);
                                wndPopup.Show();
                            }
                        }
                    }
                }
                ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 

                multiPrint = false;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void setUiParameters(string lotId)
        {
            //ChySler LotID 기준 : P05193143AO-TESIG0757C0031
            //LOTID 가공
            //seleted_lot = dt.Rows[currentRow]["LOTID"].ToString();
            //seleted_eqsgid = DataTableConverter.GetValue(dgResult_Mp.Rows[currentRow].DataItem, "LOTID").ToString();
            //seleted_eqsgid = dt.Rows[currentRow]["EQSGID"].ToString();


            //PART# CUST(P), PART# CUST(1P)
            /*
            tbPartCustP_Mp.Text = seleted_lot.Substring(0, seleted_lot.IndexOf('-')); //P05193143AO
            bcPartCustP_Mp.Text = tbPartCustP_Mp.Text;
            tbPartCust1P_Mp.Text = tbPartCustP_Mp.Text;
            bcPartCust1P_Mp.Text = tbPartCustP_Mp.Text;
            */

            //SERIAL# (38)
            tbSerial_Mp.Text = lotId.Substring(lotId.IndexOf('-') + 6);//0757C0031
            bcSerial_Mp.Text = tbSerial_Mp.Text;

            //CHANGE LETTER
            //tbChange_Mp.Text = tbPartCustP_Mp.Text.Substring(tbPartCustP_Mp.Text.Length-2);

            //2D BARCODE
            bcTotalId_Mp.Text = tbPartCustP_Mp.Text + "," + txtQty_Mp.Text + "," + tbSuplr_Mp.Text + "," + tbSerial_Mp.Text + "," + tbPartCust1P_Mp.Text;

            //DATE MFG : 날짜 선택하면 적용되는 것으로 변경
            //tbDateMfg_Mp.Text = DateTime.Now.ToString("ddMMMyyyy");   // 원래 DateTime.Now.ToString("DDMMMYYYY");   인데 윈7에서 월name을 지원안해줌
        }

        private void getZpl(string I_ATTVAL, string LabelCode, int tab_idx)
        {
            try
            {
                if (tab_idx == 0)
                {
                    DataTable dtResult = Util.getDirectZpl(sLBCD: LabelCode, sATTVAL: I_ATTVAL);

                    if (dtResult == null || dtResult.Rows.Count > 0)
                    {
                        zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                    }



                    //string sRESO = string.Empty; //DPI(해상도)
                    //string sMARH = string.Empty; //LEFT
                    //string sMARV = string.Empty; //TOP
                    //string sDARK = string.Empty; //DARKNESS


                    //foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    //{
                    //    if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    //    {
                    //        sRESO = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                    //        break;
                    //    }
                    //}

                    //foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    //{
                    //    if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    //    {
                    //        sMARH = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                    //        break;
                    //    }
                    //}

                    //foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    //{
                    //    if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    //    {
                    //        sMARV = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                    //        break;
                    //    }
                    //}

                    //foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    //{
                    //    if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    //    {
                    //        sDARK = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                    //        break;
                    //    }
                    //}


                    //DataSet RQSTDS= new DataSet();
                    //DataTable INDATA = new DataTable();
                    //DataTable IN_OPTION = new DataTable();
                    //DataRow drINDATA = INDATA.NewRow();
                    //DataRow drIN_OPTION = IN_OPTION.NewRow();

                    //INDATA.Columns.Add("SRCTYPE", typeof(string));
                    //INDATA.Columns.Add("LANGID", typeof(string));
                    //INDATA.Columns.Add("LOTID", typeof(string));
                    //INDATA.Columns.Add("PROCID", typeof(string));
                    //INDATA.Columns.Add("EQPTID", typeof(string));
                    //INDATA.Columns.Add("EQSGID", typeof(string));
                    //INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                    //INDATA.Columns.Add("LABEL_CODE", typeof(string));
                    //INDATA.Columns.Add("PRN_QTY", typeof(string));
                    //INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                    //INDATA.Columns.Add("PRODID", typeof(string));
                    //INDATA.Columns.Add("DPI", typeof(string));

                    //drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    //drINDATA["LANGID"] = LoginInfo.LANGID;
                    //drINDATA["LOTID"] = seleted_lot;
                    //drINDATA["PROCID"] = "";
                    //drINDATA["EQPTID"] = "";
                    //drINDATA["EQSGID"] = seleted_eqsgid;
                    //drINDATA["LABEL_TYPE"] = "PACK_INBOX";
                    //drINDATA["LABEL_CODE"] = LabelCode;
                    //drINDATA["PRN_QTY"] = nbPrintCnt_Mp.Value.ToString();
                    //drINDATA["SAMPLE_FLAG"] = "N";
                    //drINDATA["PRODID"] = Util.GetCondition(cboProduct_Mp);
                    //drINDATA["DPI"] = sRESO;   

                    //INDATA.Rows.Add(drINDATA);

                    //IN_OPTION.Columns.Add("LEFT", typeof(string));
                    //IN_OPTION.Columns.Add("TOP", typeof(string));
                    //IN_OPTION.Columns.Add("DARKNESS", typeof(string));

                    //drIN_OPTION["LEFT"] = sMARH;
                    //drIN_OPTION["TOP"] = sMARV;
                    //drIN_OPTION["DARKNESS"] = sDARK;

                    //IN_OPTION.Rows.Add(drIN_OPTION);

                    //RQSTDS.Tables.Add(INDATA);
                    //RQSTDS.Tables.Add(IN_OPTION);

                    //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL", "INDATA,IN_OPTION", "OUTDATA", RQSTDS);

                    //if(dsResult != null && dsResult.Tables.Count > 0)
                    //{
                    //    zpl = dsResult.Tables["OUTDATA"].Rows[0]["ZPLSTRING"].ToString();
                    //}

                }
                else
                {
                    DataTable dtResult = Util.getDirectZpl(sLBCD: LabelCode, sATTVAL: I_ATTVAL);

                    if (dtResult == null || dtResult.Rows.Count > 0)
                    {
                        zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet(string labelCode, int tab_idx)
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
                dr["LABEL_CODE"] = labelCode;

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData(tab_idx);

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

        private DataTable getInputData(int tab_Index)
        {
            DataTable dt = new DataTable();

            if (tab_Index == 0) //Chrsler(Prod)
            {
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
                dt.Columns.Add("ITEM022", typeof(string));
                dt.Columns.Add("ITEM023", typeof(string));
                dt.Columns.Add("ITEM024", typeof(string));
                dt.Columns.Add("ITEM025", typeof(string));
                dt.Columns.Add("ITEM026", typeof(string));
                dt.Columns.Add("ITEM027", typeof(string));

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = tbPartCustP_Mp.Text; //05193144AA
                dr["ITEM002"] = tbPartCustP_Mp.Text; //05193144AA 
                dr["ITEM003"] = tbShipFrom1_Mp.Text; //LG CMI
                dr["ITEM004"] = tbShipFrom2_Mp.Text; //1 LG WAY
                dr["ITEM005"] = tbShipFrom3_Mp.Text; //HOLLAND, MI, 49423                
                dr["ITEM006"] = tbShipTo1_Mp.Text; //SYNCREON
                dr["ITEM007"] = tbShipTo2_Mp.Text; //2935 Pillette Rd
                dr["ITEM008"] = tbShipTo3_Mp.Text; //Windsor, ON N8T 0A7
                dr["ITEM009"] = tbDestination_Mp.Text; //09103AJ
                dr["ITEM010"] = bcTotalId_Mp.Text;
                dr["ITEM011"] = tbPartCust1P_Mp.Text; //05193144AA
                dr["ITEM012"] = tbPartCust1P_Mp.Text; //05193144AA
                dr["ITEM013"] = tbPartNumber_Mp1.Text; //MSD Plug
                dr["ITEM014"] = tbPartNumber_Mp2.Text; //
                dr["ITEM015"] = tbGross1_Mp.Text; //12.20
                dr["ITEM016"] = tbGross2_Mp.Text; //5.533
                dr["ITEM017"] = txtQty_Mp.Text; //
                dr["ITEM018"] = txtQty_Mp.Text; //                         
                dr["ITEM019"] = tbChange_Mp.Text; //AA
                dr["ITEM020"] = tbDateMfg_Mp.Text; //                         
                dr["ITEM021"] = tbSuplr_Mp.Text; //22547
                dr["ITEM022"] = tbSuplr_Mp.Text; //22547
                dr["ITEM023"] = tbSerial_Mp.Text; //
                dr["ITEM024"] = tbSerial_Mp.Text; //                         
                dr["ITEM025"] = tbDockLoc_Mp.Text; //EX
                dr["ITEM026"] = tbDropZone_Mp.Text; //F3381L 
                dr["ITEM027"] = tbPackaging_Mp.Text; //EXP051505
                dt.Rows.Add(dr);
            }
            else if (tab_Index == 1) //Master
            {
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

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = tbShipFrom1_M.Text; //LG CMI 
                dr["ITEM002"] = tbShipFrom2_M.Text; //1 LGWAY 
                dr["ITEM003"] = tbShipFrom3_M.Text; //HOLLAND, MI, 49423
                dr["ITEM004"] = tbShipTo1_M.Text; //CTC-East Dock(C.Stanos)
                dr["ITEM005"] = tbShipTo2_M.Text; //800 Chrysler Dr.
                dr["ITEM006"] = tbShipTo3_M.Text; //Auburn Hiils MI, 48326
                dr["ITEM007"] = tbPartP_M.Text; //05139144AA
                dr["ITEM008"] = tbPartP_M.Text; //05139144AA
                dr["ITEM009"] = tbPart1P_M.Text; //31491834
                dr["ITEM010"] = tbPart1P_M.Text; //31491834
                dr["ITEM011"] = txtQty_M.Text; //
                dr["ITEM012"] = txtQty_M.Text; //
                dr["ITEM013"] = tbSUPLR_M.Text; //22547
                dr["ITEM014"] = tbSUPLR_M.Text; //22547              
                dr["ITEM015"] = tbPkg_M.Text; //EXP0151505
                dr["ITEM016"] = tbPartNumber_M.Text; //MSG Plug
                dt.Rows.Add(dr);
            }
            else if (tab_Index == 2) //Chrsler(Material)
            {
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
                dt.Columns.Add("ITEM022", typeof(string));
                dt.Columns.Add("ITEM023", typeof(string));
                dt.Columns.Add("ITEM024", typeof(string));
                dt.Columns.Add("ITEM025", typeof(string));
                dt.Columns.Add("ITEM026", typeof(string));
                dt.Columns.Add("ITEM027", typeof(string));

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = tbPartCustP_Mt.Text; //05193144AA
                dr["ITEM002"] = tbPartCustP_Mt.Text; //05193144AA 
                dr["ITEM003"] = tbShipFrom1_Mt.Text; //LG CMI
                dr["ITEM004"] = tbShipFrom2_Mt.Text; //1 LG WAY
                dr["ITEM005"] = tbShipFrom3_Mt.Text; //HOLLAND, MI, 49423                
                dr["ITEM006"] = tbShipTo1_Mt.Text; //SYNCREON
                dr["ITEM007"] = tbShipTo2_Mt.Text; //2935 Pillette Rd
                dr["ITEM008"] = tbShipTo3_Mt.Text; //Windsor, ON N8T 0A7
                dr["ITEM009"] = tbDestination_Mt.Text; //09103AJ
                dr["ITEM010"] = bcTotalId_M.Text;
                dr["ITEM011"] = tbPartCust1P_Mt.Text; //05193144AA
                dr["ITEM012"] = tbPartCust1P_Mt.Text; //05193144AA
                dr["ITEM013"] = tbPartNumber_Mt1.Text; //MSD Plug
                dr["ITEM014"] = tbPartNumber_Mt2.Text; //
                dr["ITEM015"] = tbGross1_Mt.Text; //12.20
                dr["ITEM016"] = tbGross2_Mt.Text; //5.533
                dr["ITEM017"] = txtQty_Mt.Text; //
                dr["ITEM018"] = txtQty_Mt.Text; //                         
                dr["ITEM019"] = tbChange_Mt.Text; //AA
                dr["ITEM020"] = tbDateMfg_Mt.Text; //                         
                dr["ITEM021"] = tbSuplr_Mt.Text; //22547
                dr["ITEM022"] = tbSuplr_Mt.Text; //22547
                dr["ITEM023"] = tbSerial_Mt.Text; //
                dr["ITEM024"] = tbSerial_Mt.Text; //                         
                dr["ITEM025"] = tbDockLoc_Mt.Text; //EX
                dr["ITEM026"] = tbDropZone_Mt.Text; //F3381L 
                dr["ITEM027"] = tbPackaging_Mt.Text; //EXP051505
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button btn = new Button();

            if (tcMain.SelectedIndex == 0)
            {
                btn = btnPrint_Mp;
            }
            else if (tcMain.SelectedIndex == 1)
            {
                btn = btnPrint_M;
            }
            else if (tcMain.SelectedIndex == 2)
            {
                btn = btnPrint_Mt;
            }

            if (print_all)
            {
                btnPrint_Mp.Content = ObjectDic.Instance.GetObjectName("출력");
                btnPrint_M.Content = ObjectDic.Instance.GetObjectName("출력");
                btnPrint_Mt.Content = ObjectDic.Instance.GetObjectName("출력");

                btnPrint_Mp.Foreground = Brushes.White;
                btnPrint_M.Foreground = Brushes.White;
                btnPrint_Mt.Foreground = Brushes.White;
            }
            else
            {
                btn.Content = ObjectDic.Instance.GetObjectName("출력");
            }

            blPrintStop = true;
            btn.Foreground = Brushes.White;

            print_all = false;
        }
        #endregion       

        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tab_idx = tcMain.SelectedIndex;

            if (tab_idx < 0)
            {
                return;
            }

            setChryslerTab(tab_idx);
        }

        private void setChryslerTab(int tab_idx)
        {
            try
            {
                string labelCode = string.Empty;
                DataTable dt = new DataTable();
                C1.WPF.C1ComboBox cb = new C1.WPF.C1ComboBox();

                if (tab_idx == 0) //Chrysler(Prod)
                {
                    dtpDate_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;

                    dtpDateFrom_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;
                    dtpDateFrom_Mp.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    dtpDateTo_Mp.SelectedDateTime = (DateTime)System.DateTime.Now;

                    tbPalletHistory_cnt_Mp.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                    labelCode = label_code_ChrslerProd;
                    dt = dtChrySlerProd;
                    cb = cboProduct_Mp;

                    txtPrintIDInput.Focus();
                }
                else if (tab_idx == 1) //Master
                {
                    dtpDate_M.SelectedDateTime = (DateTime)System.DateTime.Now;

                    //dtpDateFrom_M.SelectedDateTime = (DateTime)System.DateTime.Now;
                    //dtpDateFrom_M.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    //dtpDateTo_M.SelectedDateTime = (DateTime)System.DateTime.Now;

                    //tbPalletHistory_cnt_M.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                    labelCode = label_code_Master;
                    dt = dtMasterResult;
                    cb = cboProduct_M;
                }
                else if (tab_idx == 2) //Chrysler(Material)
                {
                    dtpDate_Mt.SelectedDateTime = (DateTime)System.DateTime.Now;

                    //dtpDateFrom_Mt.SelectedDateTime = (DateTime)System.DateTime.Now;
                    //dtpDateFrom_Mt.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    //dtpDateTo_Mt.SelectedDateTime = (DateTime)System.DateTime.Now;

                    //tbPalletHistory_cnt_Mt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                    labelCode = label_code_ChrslerMaterial;
                    dt = dtChrySlerMatrial;
                    cb = cboProduct_Mt;

                    if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                    {
                        ADMIN.Visibility = Visibility.Visible;
                    }
                }

                setCombo_Chrysler_Tab(tab_idx, labelCode, dt, cb);

                if (tab_idx == 1) // master는 조회로 데이터를 가지고 오지 않고 바로 정보조회후 세팅해줌.
                {
                    getValueSetting_M();
                }
                else if (tab_idx == 2) // material은 조회로 데이터를 가지고 오지 않고 바로 정보조회후 세팅해줌.
                {
                    getValueSetting_Mt();

                    //2D BARCODE
                    bcTotalId_M.Text = tbPartCustP_Mt.Text + "," + txtQty_Mt.Text + "," + tbSuplr_Mt.Text + "," + tbPartCust1P_Mt.Text;


                    dtpDate_Mt.SelectedDataTimeChanged += dtpDate_Mt_SelectedDataTimeChanged;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDate_Mt_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = dtpDate_Mt.SelectedDateTime;

            string dd = dt.ToString("dd");
            string MM = dt.ToString("MMM").ToUpper();

            if (MM.Length != 3)
            {
                MM = chage_date(MM);
            }

            string yyyy = dt.ToString("yyyy");

            tbDateMfg_Mt.Text = dd + MM + yyyy;
        }

        private void btnSearch_M_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting_M();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void getValueSetting_M()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct_M); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = label_code_Master; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtMasterResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtMasterResult == null || dtMasterResult.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox_Master();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getValueSetting_Mt()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LABEL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboProduct_Mt); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = label_code_ChrslerMaterial; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtChrySlerMatrial = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtChrySlerMatrial == null || dtChrySlerMatrial.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox_CrySler_Matrial();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getValueSetting_Mp()
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
                dr["PRODID"] = Util.GetCondition(cboProduct_Mp); //dtpDateTo.SelectedDateTime.ToString
                dr["LABEL"] = label_code_ChrslerProd; //dtpDateTo.SelectedDateTime.ToString();
                dr["SHIPTO_ID"] = null; //dtpDateTo.SelectedDateTime.ToString();F
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID; //dtpDateTo.SelectedDateTime.ToString();

                RQSTDT.Rows.Add(dr);

                dtChrySlerProd = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtChrySlerProd == null || dtChrySlerProd.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBox_CrySler_Prod();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setTextBox_Master()
        {
            if (dtMasterResult == null || dtMasterResult.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtMasterResult;

            tbShipFrom1_M.Text = returnString(dt, "ITEM001");        //LG CMI         
            tbShipFrom2_M.Text = returnString(dt, "ITEM002");      //1 LGWAY    
            tbShipFrom3_M.Text = returnString(dt, "ITEM003");     //HOLLAND, MI, 49423
            tbShipTo1_M.Text = returnString(dt, "ITEM004");          //CTC-East Dock(C.Stanos)
            tbShipTo2_M.Text = returnString(dt, "ITEM005");           //800 Chrysler Dr.
            tbShipTo3_M.Text = returnString(dt, "ITEM006");             //Auburn Hiils MI, 48326
            tbPartP_M.Text = returnString(dt, "ITEM007");         //05139144AA
            bcPartCustP_M.Text = returnString(dt, "ITEM008");            //05139144AA
            tbPart1P_M.Text = returnString(dt, "ITEM009");                  //05139144AA
            bcPartCust1P_M.Text = returnString(dt, "ITEM010");         //05139144AA
            txtQty_M.Text = returnString(dt, "ITEM011");       //
            bcQty_M.Text = returnString(dt, "ITEM012");           //
            tbSUPLR_M.Text = returnString(dt, "ITEM013"); //22547
            bcSUPLR_M.Text = returnString(dt, "ITEM014"); //22547
            tbPkg_M.Text = returnString(dt, "ITEM015");            //EXP0151505
            tbPartNumber_M.Text = returnString(dt, "ITEM016");     //MSG Plug
        }

        private void setTextBox_CrySler_Prod(string sPartNo = "")
        {
            if (dtChrySlerProd == null || dtChrySlerProd.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtChrySlerProd;

            // "Result Grid Click" : Default가 쿼리임 : Search후 grid click하면 적용됨
            tbPartCustP_Mp.Text = string.IsNullOrEmpty(sPartNo) ? returnString(dt, "ITEM001") : sPartNo;//"- Grid Click -"; //returnString(dt, "ITEM001");      //05193144AA      
            bcPartCustP_Mp.Text = string.IsNullOrEmpty(sPartNo) ? returnString(dt, "ITEM002") : sPartNo;//"- Grid Click -"; // returnString(dt, "ITEM002");     //05193144AA  
            tbShipFrom1_Mp.Text = returnString(dt, "ITEM003");     //LG CMI
            tbShipFrom2_Mp.Text = returnString(dt, "ITEM004");     //1 LG WAY
            tbShipFrom3_Mp.Text = returnString(dt, "ITEM005");     //HOLLAND, MI, 49423
            tbShipTo1_Mp.Text = returnString(dt, "ITEM006");        //SYNCREON
            tbShipTo2_Mp.Text = returnString(dt, "ITEM007");        //2935 Pillette Rd
            tbShipTo3_Mp.Text = returnString(dt, "ITEM008");        //Windsor, ON N8T 0A7
            tbDestination_Mp.Text = returnString(dt, "ITEM009");    //Windsor, ON N8T 0A7
            //bcTotalId_Mp.Text = returnString(dt, "ITEM010");      //2D 바코드 영역
            tbPartCust1P_Mp.Text = string.IsNullOrEmpty(sPartNo) ? returnString(dt, "ITEM011") : sPartNo;//"- Grid Click -"; //    //05193144AA
            bcPartCust1P_Mp.Text = string.IsNullOrEmpty(sPartNo) ? returnString(dt, "ITEM012") : sPartNo;//"- Grid Click -"; //     //05193144AA
            tbPartNumber_Mt1.Text = returnString(dt, "ITEM013"); //MSD Plug
            tbPartNumber_Mt2.Text = returnString(dt, "ITEM014"); //
            tbGross1_Mp.Text = returnString(dt, "ITEM015");            //EXP0151505
            tbGross2_Mp.Text = returnString(dt, "ITEM016");     //MSG Plug
            txtQty_Mp.Text = returnString(dt, "ITEM017");
            bcQty_Mp.Text = returnString(dt, "ITEM018");
            tbChange_Mp.Text = returnString(dt, "ITEM019"); //"-Grid Click-"; // //AA
            //tbDateMfg_Mp.Text = returnString(dt, "ITEM020"); //날짜는 사용자가 직접 선택
            tbSuplr_Mp.Text = returnString(dt, "ITEM021");//22547
            bcPartCust1PSuplr_Mp.Text = returnString(dt, "ITEM022");//22547
            tbSerial_Mp.Text = "- Grid Click -"; //
            bcSerial_Mp.Text = "- Grid Click -"; //
            tbDockLoc_Mp.Text = returnString(dt, "ITEM025");//EX
            tbDropZone_Mp.Text = returnString(dt, "ITEM026");//F3381L 
            tbPackaging_Mp.Text = returnString(dt, "ITEM027");//EXP051505

            //*woosuck Part number 정보 표시
            //txtPartDesc.Text = getPartnumber_desc("P" + tbPartCustP_Mp.Text);
        }

        //*woosuck 라벨 아이템 초기화
        private void clearTextBox_CrySler_Prod()
        {
            tbPartCustP_Mp.Text = string.Empty;
            bcPartCustP_Mp.Text = string.Empty;
            tbShipFrom1_Mp.Text = string.Empty;
            tbShipFrom2_Mp.Text = string.Empty;
            tbShipFrom3_Mp.Text = string.Empty;
            tbShipTo1_Mp.Text = string.Empty;
            tbShipTo2_Mp.Text = string.Empty;
            tbShipTo3_Mp.Text = string.Empty;
            tbDestination_Mp.Text = string.Empty;
            tbPartCust1P_Mp.Text = string.Empty;
            bcPartCust1P_Mp.Text = string.Empty;
            tbPartNumber_Mt1.Text = string.Empty;
            tbPartNumber_Mt2.Text = string.Empty;
            tbGross1_Mp.Text = string.Empty;
            tbGross2_Mp.Text = string.Empty;
            txtQty_Mp.Text = string.Empty;
            bcQty_Mp.Text = string.Empty;
            tbChange_Mp.Text = string.Empty;
            tbSuplr_Mp.Text = string.Empty;
            bcPartCust1PSuplr_Mp.Text = string.Empty;
            tbSerial_Mp.Text = "- Grid Click -"; //
            bcSerial_Mp.Text = "- Grid Click -"; //
            tbDockLoc_Mp.Text = string.Empty;
            tbDropZone_Mp.Text = string.Empty;
            tbPackaging_Mp.Text = string.Empty;

            //*woosuck Part number 정보 초기화
            txtPartDesc.Text = string.Empty;
            txtLabelProdID.Text = string.Empty;
            txtPrintIDInput.Text = string.Empty;

            TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
            textRange.Text = string.Empty;
            seleted_lot = string.Empty;
        }

        private void setTextBox_CrySler_Matrial()
        {
            if (dtMasterResult == null || dtMasterResult.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtChrySlerMatrial;

            tbPartCustP_Mt.Text = returnString(dt, "ITEM001");      //05193144AA       
            bcPartCustP_Mt.Text = returnString(dt, "ITEM002");     //05193144AA  
            tbShipFrom1_Mt.Text = returnString(dt, "ITEM003");     //LG CMI
            tbShipFrom2_Mt.Text = returnString(dt, "ITEM004");     //1 LG WAY
            tbShipFrom3_Mt.Text = returnString(dt, "ITEM005");     //HOLLAND, MI, 49423
            tbShipTo1_Mt.Text = returnString(dt, "ITEM006");        //SYNCREON
            tbShipTo2_Mt.Text = returnString(dt, "ITEM007");        //2935 Pillette Rd
            tbShipTo3_Mt.Text = returnString(dt, "ITEM008");        //Windsor, ON N8T 0A7
            tbDestination_Mt.Text = returnString(dt, "ITEM009");    //Windsor, ON N8T 0A7
            //bcTotalId_Mt.Text = returnString(dt, "ITEM010");      //2D 바코드 영역
            tbPartCust1P_Mt.Text = returnString(dt, "ITEM011");     //05193144AA
            bcPartCust1P_Mt.Text = returnString(dt, "ITEM012");     //05193144AA
            tbPartNumber_Mt1.Text = returnString(dt, "ITEM013"); //MSD Plug
            tbPartNumber_Mt2.Text = returnString(dt, "ITEM014"); //
            tbGross1_Mt.Text = returnString(dt, "ITEM015");            //EXP0151505
            tbGross2_Mt.Text = returnString(dt, "ITEM016");     //MSG Plug
            txtQty_Mt.Text = returnString(dt, "ITEM017");
            bcQty_Mt.Text = returnString(dt, "ITEM018");
            tbChange_Mt.Text = returnString(dt, "ITEM019"); //AA
            //tbDateMfg_Mt.Text = returnString(dt, "ITEM020"); //날짜는 사용자가 직접 선택
            tbSuplr_Mt.Text = returnString(dt, "ITEM021");//22547
            bcPartCust1PSuplr_Mt.Text = returnString(dt, "ITEM022");//22547
            tbSerial_Mt.Text = returnString(dt, "ITEM023");
            bcSerial_Mt.Text = returnString(dt, "ITEM024");
            tbDockLoc_Mt.Text = returnString(dt, "ITEM025");//EX
            tbDropZone_Mt.Text = returnString(dt, "ITEM026");//F3381L 
            tbPackaging_Mt.Text = returnString(dt, "ITEM027");//EXP051505

        }



        private void btnPrint_M_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPrint_M.Content.ToString().Replace("#", "") == ObjectDic.Instance.GetObjectName("취소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint_M.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                PrintProcess(btnPrint_M);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnPrint_Mt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnPrint_Mt.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint_Mt.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                PrintProcess(btnPrint_Mt);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnSearch_Mp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting_Mp();

                Get_Product_Lot();

            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //*woosuck 프린트 이벤트시 이력 검사
        private void btnPrint_Mp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strErrMsg = string.Empty;

                if (string.IsNullOrWhiteSpace(seleted_lot))
                {
                    ms.AlertInfo("SFU1261");
                    return;
                }

                if (chkLastPartnumberOfLot(seleted_lot, out strErrMsg))
                {
                    Mp_Print();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(strErrMsg))
                    {
                        ms.AlertWarning("SFU3283"); //입력오류 : 포장중인 BOX의 제품과 입력한 LOT의 제품이 다릅니다.
                    }
                    else
                    {
                        ms.AlertWarning(strErrMsg, seleted_lot);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void Mp_Print()
        {
            try
            {
                if (btnPrint_Mp.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint_Mp.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                if (Convert.ToBoolean(chkPrintAll.IsChecked))
                {
                    //Master, prod, material 라벨 동시 출력
                    initMasterMaterial();

                    allPrint();

                    print_all = true;
                }

                PrintProcess(btnPrint_Mp);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void initMasterMaterial()
        {
            //value setting.
            tcMain.SelectedIndex = 1;

            tcMain.SelectedIndex = 2;
        }

        private void allPrint()
        {

        }

        private void dgResult_Mp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgResult_Mp.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                DataTable dt = DataTableConverter.Convert(dgResult_Mp.ItemsSource);
                //ChySler LotID 기준 : P05193143AO-TESIG0757C0031
                //LOTID 가공
                seleted_lot = dt.Rows[currentRow]["LOTID"].ToString();
                //seleted_eqsgid = DataTableConverter.GetValue(dgResult_Mp.Rows[currentRow].DataItem, "LOTID").ToString();
                seleted_eqsgid = dt.Rows[currentRow]["EQSGID"].ToString();


                //PART# CUST(P), PART# CUST(1P)
                /*
                tbPartCustP_Mp.Text = seleted_lot.Substring(0, seleted_lot.IndexOf('-')); //P05193143AO
                bcPartCustP_Mp.Text = tbPartCustP_Mp.Text;
                tbPartCust1P_Mp.Text = tbPartCustP_Mp.Text;
                bcPartCust1P_Mp.Text = tbPartCustP_Mp.Text;
                */

                //SERIAL# (38)
                tbSerial_Mp.Text = seleted_lot.Substring(seleted_lot.IndexOf('-') + 6);//0757C0031
                bcSerial_Mp.Text = tbSerial_Mp.Text;

                //CHANGE LETTER
                //tbChange_Mp.Text = tbPartCustP_Mp.Text.Substring(tbPartCustP_Mp.Text.Length-2);

                //2D BARCODE
                bcTotalId_Mp.Text = tbPartCustP_Mp.Text + "," + txtQty_Mp.Text + "," + tbSuplr_Mp.Text + "," + tbSerial_Mp.Text + "," + tbPartCust1P_Mp.Text;

                //DATE MFG : 날짜 선택하면 적용되는 것으로 변경
                //tbDateMfg_Mp.Text = DateTime.Now.ToString("ddMMMyyyy");   // 원래 DateTime.Now.ToString("DDMMMYYYY");   인데 윈7에서 월name을 지원안해줌
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void txtQty_Mp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgResult_Mp != null && dgResult_Mp.Rows.Count > 0)
                {
                    if (txtQty_Mp.Text.Length > 0)
                    {
                        //2D BARCODE
                        bcTotalId_Mp.Text = tbPartCustP_Mp.Text + "," + txtQty_Mp.Text + "," + tbSuplr_Mp.Text + "," + tbSerial_Mp.Text + "," + tbPartCust1P_Mp.Text;
                        bcQty_Mp.Text = txtQty_Mp.Text;
                    }
                }
            }
        }

        private void txtQty_M_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                if (txtQty_M.Text.Length > 0)
                {
                    bcQty_M.Text = txtQty_M.Text;
                }
            }
        }

        private void txtQty_Mt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtQty_Mt.Text.Length > 0)
                {
                    bcTotalId_M.Text = tbPartCustP_Mt.Text + "," + txtQty_Mt.Text + "," + tbSuplr_Mt.Text + "," + tbPartCust1P_Mt.Text;
                    bcQty_Mt.Text = txtQty_Mt.Text;
                }
            }
        }

        private void btnPrintList_Clear_Click(object sender, RoutedEventArgs e)
        {
            //TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
            //textRange.Text = "";
            //txtLabelProdID.Text = "";

            clearTextBox_CrySler_Prod();

            DoEvents();

            txtPrintIDInput.Focus();
            //cboLabelCode.ItemsSource = null;
            //cboLabelCode.SelectedValue = null;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        //*woosuck Scan한 ID 정보로 출력정보 및 Validation
        //2024.05.27 ESMI 바코드 SCAN 시 중복 팝업 발생으로 LOADING INDICATOR, DOEVENT 부분 주석 처리 - KIM MIN SEOK
        private void txtPrintIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    //ShowLoadingIndicator();
                    //DoEvents();

                    if (txtPrintIDInput.Text.Length > 0)
                    {
                        string strErrMsg = string.Empty;

                        TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                        string sLotList = textRange.Text.Replace("\r\n", "").Trim();
                        string sProdID = string.Empty;
                        //mapping lot, prodid 추출
                        string sScanLot = getScanLotInfor(txtPrintIDInput.Text.Trim().ToUpper(), out sProdID);
                        //마지막 인쇄 part number 추출
                        string sPartNo = getLastPartnumberOfLot(sScanLot);


                        if (string.IsNullOrEmpty(sScanLot))
                        {


                            //HiddenLoadingIndicator();

                            Util.MessageInfo("SFU1195", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPrintIDInput.Focus();
                                    txtPrintIDInput.SelectAll();
                                }
                            });

                            //ms.AlertInfo("SFU1195"); //Lot 정보가 없습니다.
                            
                            return;
                        }

                        if (sLotList.Contains(sScanLot))
                        {
                            //HiddenLoadingIndicator();
                            //LOT ID는 중복 입력할 수 없습니다.
                            Util.MessageInfo("SFU1376", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPrintIDInput.Focus();
                                    txtPrintIDInput.SelectAll();
                                }
                            }, sScanLot);

                            //ms.AlertInfo("SFU1376",sScanLot);
                            //txtPrintIDInput.SelectAll();

                            return;
                        }


                        if (!chkLastPartnumberOfLot(sScanLot.Trim(), out strErrMsg, sPartNo))
                        {
                            //HiddenLoadingIndicator();

                            if (string.IsNullOrWhiteSpace(strErrMsg))
                            {
                                Util.MessageInfo("SFU3283", (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtPrintIDInput.Focus();
                                        txtPrintIDInput.SelectAll();
                                    }
                                });
                                //ms.AlertInfo("SFU3283"); //입력오류 : 포장중인 BOX의 제품과 입력한 LOT의 제품이 다릅니다.
                            }
                            else
                            {
                                //HiddenLoadingIndicator();
                                Util.MessageInfo(strErrMsg, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtPrintIDInput.Focus();
                                        txtPrintIDInput.SelectAll();
                                    }
                                }, sScanLot);
                                //ms.AlertInfo(strErrMsg, sScanLot);
                            }
                            return;
                        }

                        //처음 입력된 ID로 출력정보 설정
                        if (string.IsNullOrWhiteSpace(textRange.Text))
                        {
                            //라밸 초기정보 설정
                            getValueSetting_Chrysler(sProdID, sPartNo);
                            //Part number sub title desc
                            txtPartDesc.Text = getPartnumber_desc("P" + sPartNo);
                        }

                        if (string.IsNullOrWhiteSpace(textRange.Text))
                        {
                            txtNote_PrintIDList.AppendText(sScanLot + "\r\n");
                            textRange.Text = textRange.Text.Replace("\r\n\r\n", "\r\n");
                        }
                        else
                        {
                            txtNote_PrintIDList.AppendText("\r\n" + sScanLot + "\r\n");

                            textRange.Text = textRange.Text.Replace("\r\n\r\n", "\r\n");
                        }

                    }
                    else
                    {
                        ms.AlertWarning("SFU1009");//입력된 LOT ID 데이터가 없습니다.
                        return;
                    }
                    //HiddenLoadingIndicator();
                    txtPrintIDInput.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //*woosuck 투입Lot이 없으면 return
                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                if (textRange.Text.Trim().Equals("")) return;

                multiPrint = true;
                Mp_Print();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void txtNote_PrintIDList_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                string sText = textRange.Text;
                string pre_prodid = string.Empty;
                DataTable temp = new DataTable();

                sText = sText.Replace("\r\n", "/").Replace(" ", "");

                if (sText == "/")
                {
                    return;
                }
                string[] lots = sText.Split('/');

                for (int i = 0; i < lots.Length; i++)
                {
                    if (lots[i].ToString() == "")
                    {
                        lots[i] = null;
                    }
                }

                if (lots[0] != null) //첫번째 lot 체크및 prod 담아두기
                {
                    pre_prodid = LotFind(lots[0]);//제품 ID 가져옴

                    if (pre_prodid != null)
                    {
                        txtLabelProdID.Text = pre_prodid;
                    }
                    else
                    {
                        ms.AlertWarning("1032", lots[0].ToString()); //LOT [%1]의 재공(WIP) 정보가 존재하지 않습니다.
                    }
                }

                for (int i = 1; i < lots.Length; i++) //두번째 lot부터 체크  첫번째 lot의 prod와 비교
                {
                    if (lots[i] != null)
                    {
                        string prodid = LotFind(lots[i]); //제품 ID 가져옴

                        if (prodid != null)
                        {
                            if (pre_prodid != prodid)
                            {
                                ms.AlertWarning("SFU1893"); //제품ID가 다릅니다.
                            }
                        }
                        else
                        {
                            ms.AlertWarning("1032", lots[0].ToString()); //LOT [%1]의 재공(WIP) 정보가 존재하지 않습니다.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string LotFind(string lotId)
        {
            //입력된 boxid 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = lotId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTCHECK_PROD", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["WIP_PROD"].ToString();
                }
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //*woosuck 콤보 변경시 내용 초기화
        private void cboProduct_Mp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            clearTextBox_CrySler_Prod();
        }

        //*woosuck Ctrl+V Lot ID 입력 - 필요 없음.-사용 안함
        private void txtPrintIDInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key != Key.V)
            {
                return;
            }

            try
            {
                string strErrMsg = string.Empty;

                string[] stringSeparators = new string[] { "\r\n", ",", "\t" };
                var lstClipboardData = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None).ToList<string>();
                string[] lstLOTIDList = lstClipboardData.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                // Validation Check...
                if (lstLOTIDList.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   // 최대 100개 까지 가능합니다.
                    return;
                }


                if (lstLOTIDList.Count() > 0)
                {
                    string errLotid = string.Empty;
                    string strLists = string.Empty;

                    txtNote_PrintIDList.Document.Blocks.Clear();

                    foreach (string item in lstLOTIDList)
                    {
                        if (!chkLastPartnumberOfLot(item.Trim(),out strErrMsg))
                        {
                            if (string.IsNullOrWhiteSpace(strErrMsg))
                            {
                                ms.AlertInfo("SFU3283"); //[%1] 투입유형이 다릅니다.
                            }
                            else
                            {
                                ms.AlertInfo(strErrMsg, item); 
                            }

                            break;
                        }

                        strLists += item.Trim() + Environment.NewLine;

                    }

                    txtNote_PrintIDList.AppendText(strLists);
                    //txtPrintIDInput.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            e.Handled = true;
        }
    }
}

