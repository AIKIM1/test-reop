/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.10.08  손우석 같은 제품 다른 라인/라른 사이트 인경우 라인 정보 파라미터 추가
  2018.10.29  손우석 파라미터 명칭 오류 수정
  2019.11.26  손우석 CSRID 6383 [긴급]Box 라벨 발행（312,313,515,517,CMA EV) 추가 요청 [요청번호] C20191120-000136
  2020.06.24  손우석 서비스번호 73520 [생산PI팀]Box 라벨 프린터 화면 개선 [요청번호] C20200620-000040
  2022.03.14  염규범 라벨 출력시 ESNA의 EV 의 탭의 경우에는 신규 로직으로 처리 
                     라벨 출력에 따라서, 이력 테이블 TB_SFC_LABEL_PRT_HIST 에 저장 및 추후 WMS 출고 처리할때, I/F 테이블에 해당 내용으로 검색하여서,
                     요청사항의 ITEM 추가 진행
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
    public partial class PACK001_008_CNA : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtResult;
        DataTable dtModelResult;
        DataTable dtTextResult;
        DataTable dtLabelCodes;
        //2019.11.26
        DataTable dtLabelCodesEV;
        DataTable dtTextResultEV;
        System.ComponentModel.BackgroundWorker bkWorker;
        //2019.11.26
        System.ComponentModel.BackgroundWorker bkEVWorker;

        private string sComScanerMsg = string.Empty;
        private bool blPrintStop = true;
        string label_code = string.Empty;
        string zpl = string.Empty;

        //2019.11.26
        string strLotIdEV = string.Empty;
        string srePalletIdEV = string.Empty;
        string strQuantity = string.Empty;

        public PACK001_008_CNA()
        {
            InitializeComponent();

            this.Loaded += PACK001_008_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion Declaration & Constructor 

        #region Initialize
        private void Initialize()
        {
            if (LoginInfo.USERID.Trim() == "ogong")
            {
                txtSeletedLabel.Visibility = Visibility.Visible;
            }

            dtpDate.SelectedDateTime = (DateTime)System.DateTime.Now;
            //2019.11.26
            dtpEVDate.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //2019.11.26
            dtpEVDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpEVDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpEVDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            txtDate.Text = PrintOutDate(DateTime.Now);  //txtZpl018.Text = PrintOutDate(DateTime.Now);
            //dtpDate_SelectedDateChanged(null, null); //dtp312HDay_ValueChanged(null, null); dtpDate

            //2019.11.26
            txtEVDate.Text = PrintOutDate(DateTime.Now);

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            //2019.11.26
            bkEVWorker = new System.ComponentModel.BackgroundWorker();
            bkEVWorker.WorkerSupportsCancellation = true;
            bkEVWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkEVWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkEVWorker_RunWorkerCompleted);

            tbPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
            //2019.11.26
            tbEVPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

            getLabelCode();  // COMMONCODE TABLE에서 해당 화면에서 발행할 LABEL_CODE 가져오기 (CMCDTYPE = "PACK_LABEL_CODE")
            setCombo_PROD(); // LABEL CODE로 제품 정보 가져오기
            setCombo_Out();  // 제품정보로 출하처 정보 가져오기
            //2019.11.26
            getLabelCodeEV();
            setCombo_EVPROD();
            setCombo_EVOut();
            setCombo_Label();

            dtpDate.SelectedDataTimeChanged += dtpDate_SelectedDateChanged;
            //2019.11.26
            dtpEVDate.SelectedDataTimeChanged += dtpEVDate_SelectedDateChanged;
        }

        #endregion Initialize


        private void PACK001_008_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_008_Loaded;
        }

        #region Event

        private void rdb515H_CheckedChanged(object sender, RoutedEventArgs e)
        {
            //dtpDate_SelectedDateChanged(null, null);
        }

        #region Event - Datagrid

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
                //string strPalletId = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "PALLETID").ToString();
                string strRefNo = strLotId.Substring(strLotId.IndexOf('T') + 1);

                //31499073 T 817131605  - partnumber T serial
                //2297504 Volvo 313H, 517H (Pack 3호기) Barcode 체계 변경
                strLotId = strLotId.Contains("T") ? strLotId.Substring(0, strLotId.IndexOf('T')) : "";

                txtpartNumber.Text = strLotId;
                //txtPalletId.Text = strPalletId;

                txtSerial.Text = strRefNo;
                txtBatch.Text = strRefNo;
                

                //모델 313H, 517H(Pack 3호기) Barcode 체계 변경 20170516
                if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
                {

                    bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P31491834     
                    bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S12031601
                    bcBatch.Text = "H" + txtBatch.Text;            //Batch No : H312031601
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
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void dgEVResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgEVResult.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int currentRow = cell.Row.Index;
                int _col = cell.Column.Index;
                string value = cell.Value.ToString();

                strLotIdEV = DataTableConverter.GetValue(dgEVResult.Rows[currentRow].DataItem, "LOTID").ToString();
                srePalletIdEV = Util.NVC(DataTableConverter.GetValue(dgEVResult.Rows[currentRow].DataItem, "PALLETID"));

                txtPalletId.Text = srePalletIdEV;
                bcPalletId.Text = srePalletIdEV;

                //2020.06.24
                //txtEVSerial.Text = strLotIdEV;
                //bcEVSerial.Text = "S" + strLotIdEV;
                //txtEVquantity.Text = Convert_LabelItems(strQuantity.Replace("@LOTID", strLotIdEV));

                if (dtTextResultEV == null || dtTextResultEV.Rows.Count == 0)
                {
                    return;
                }

                DataTable dt = dtTextResultEV;

                txtEVquantity.Text = Convert_LabelItems(returnString(dt, "ITEM007").Replace("@LOTID", srePalletIdEV));
                bcEVquantity.Text = "Q" + txtEVquantity.Text;

                txtEVSerial.Text = Convert_LabelItems(returnString(dt, "ITEM011").Replace("@LOTID", srePalletIdEV)); ;
                bcEVSerial.Text = "S" + txtEVSerial.Text;

                txtEVDate.Text = "D" + dtpEVDate.SelectedDateTime.ToString("yyyyMMdd");

                txtEVAdvice.Text = dtpEVDate.SelectedDateTime.ToString("yyyyMMdd");
                bcEVAdvice.Text = "N" + txtEVAdvice.Text;
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }
        
        #endregion Event - Datagrid

        #region Event - Button

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
            catch (Exception ex)
            {
                //2020.06.24
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
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

                //2019.11.26
                //PrintProcess(btnPrint);
                PrintProcess(btnPrint, 0);
            }
            catch (Exception ex)
            {
                //2020.06.04
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getValueSetting2();

                dtpEVDate_SelectedDateChanged(null, null);

                Get_Product_Lot_EV();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void btnEVPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnEVPrint.Content.ToString() == ObjectDic.Instance.GetObjectName("취 소"))
                {
                    bkEVWorker.Dispose();
                    blPrintStop = true;
                    btnEVPrint.Content = ObjectDic.Instance.GetObjectName("출 력");
                    return;
                }

                PrintProcess(btnEVPrint, 1);
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void btnEVAdvice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Label Layout의 Advice Note No 부분 출력
                string strZPLString = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH10,10^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                if ((bool)chkAutoPrint.IsChecked)
                {
                    strZPLString += string.Format("^A0N,18,20^FO5,0^CI0^FDAdvice Note No (N)^FS");
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtEVAdvice.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", bcEVAdvice.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbEVPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                }
                else
                {
                    strZPLString += string.Format("^A0N,70,60^FO210,0^CI0^FD{0}^FS", txtEVAdvice.Text); //txt312H03
                    strZPLString += string.Format("^BY4,2.8^FO30,60^B3N,N,104,N,N^FDN{0}^FS", bcEVAdvice.Text); //txt312H03
                    strZPLString += string.Format("^PQ{0},0,1,Y^XZ", nbEVPrintCnt_R.Value);  //nuAdviceNoteNo
                    PrintBoxLabel(strZPLString);

                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 

                    return;
                }
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        #endregion Event - Button

        #region Event - Date

        private void dtpDate_SelectedDateChanged1(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpDate_SelectedDateChanged(null, null);
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

                if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
                {
                    bcAdvice.Text = "N" + txtAdvice.Text;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void dtpDate_SelectedDateChanged2(object sender, PropertyChangedEventArgs<double> e)
        {
            dtpEVDate_SelectedDateChanged(null, null);
        }

        //2019.11.26
        private void dtpEVDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dtpEVDate == null || dtpEVDate.SelectedDateTime == null)
                {
                    return;
                }

                if (cboEVProduct == null)
                {
                    return;
                }

                //날짜 선택시 Advice Note No, Date 정보를 가져와서 Text 박스에 세팅
                txtEVDate.Text = "D" + dtpEVDate.SelectedDateTime.ToString("yyyyMMdd");

                //2020.06.24
                //string line_no = getLineEV();

                //txtEVAdvice.Text = line_no + dtpEVDate.SelectedDateTime.ToString("yyMMdd") + nbEVProductBox.Value.ToString();
                txtEVAdvice.Text = dtpEVDate.SelectedDateTime.ToString("yyyyMMdd");
                bcEVAdvice.Text = "N" + txtEVAdvice.Text;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Event - Date

        #region Event - Text

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

        //2019.11.26
        private void txtEVAdvice_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVpartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVquantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVSupplierID_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVSerial_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            setEVBacode(sender);
        }

        //2019.11.26
        private void txtEVSeletedLabel_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion Event - Text

        #region Event - Combo

        private void cboProduct_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dtModelResult != null || dtModelResult.Rows.Count > 0)
            {
                DataRow[] dr = dtModelResult.Select("CBO_CODE = '" + Util.GetCondition(cboProduct) + "'");

                tabItem.Header = Util.NVC(dr[0]["MODEL"]);
                txtSeletedLabel.Text = Util.NVC(dr[0]["LABEL_CODE"]);
                label_code = Util.NVC(dr[0]["LABEL_CODE"]);

                setCombo_Out();

                //getValueSetting();              
            }
        }

        //2019.11.26
        private void cboLabel_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtEVSeletedLabel.Text = cboLabel.SelectedValue.ToString();
            label_code = cboLabel.SelectedValue.ToString();
        }

        //2019.11.26
        private void cboEVProduct_seletedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dtModelResult != null || dtModelResult.Rows.Count > 0)
            {
                DataRow[] dr = dtModelResult.Select("CBO_CODE = '" + Util.GetCondition(cboEVProduct) + "'");

                //tabItem.Header = Util.NVC(dr[0]["MODEL"]);
                txtEVSeletedLabel.Text = Util.NVC(dr[0]["LABEL_CODE"]);
                label_code = Util.NVC(dr[0]["LABEL_CODE"]);

                setCombo_EVOut();
            }
        }

        #endregion Event - Combp

        #endregion Event

        #region Method

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
                    label_codes = "LBL0020,LBL0067";
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

                //ComboStatus cs 
                //CommonCombo.AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.AlertInfo(ex.Message);
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
                    return "6";
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
                dr["PRODID"] = Util.GetCondition(cboProduct);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpDateTo);

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

        private void getLabelCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = "CNA";

                RQSTDT.Rows.Add(dr);

                dtLabelCodes = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
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


        private void labelItemsGetEV(string labelCode, int tab_idx)
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

                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";

                DataRow drIndata = null;
                drIndata = INDATA.NewRow();

                //2022.03.14 염규범 선임
                // TB_SFC_LABEL_PRT_HIST 테이블에 BOXID 의 INDEX가 없고, 데이터 양이 문제 및 일정으로 인하여서 LOTID에 PALLETID를 입력 하는 방식으로 처리
                // 추후에, BOXID에 INDEX를 넣을 경우에는, LOTID 를 BOXID에 입력 처리로 변경하고, 출고할때, WMS I/F 테이블에, 
                // 조회하는 부분을 LOTID에서 BOXID로 처리하는 방향으로 변경해서 진행처리가 필요합니다.
                INDATA.Columns.Add("LOTID", typeof(string));
                //INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("REPRT_FLAG", typeof(string));
                INDATA.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                INDATA.Columns.Add("INSUSER", typeof(string));

                drIndata["LOTID"] = txtPalletId.Text.ToString();
                //drIndata["BOXID"] = srePalletIdEV;
                drIndata["LABEL_CODE"] = labelCode;
                drIndata["NOTE"] = "EV_BOX_LABEL";
                drIndata["REPRT_FLAG"] = "N";
                drIndata["LABEL_PRT_COUNT"] = "1";
                drIndata["INSUSER"] = LoginInfo.USERID;


                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    dtInput = getInputData(tab_idx);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        #region 화면에서 입력된 값 뿌림                        
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtInput.Rows[0][item_code].ToString();
                        #endregion

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            if(string.Format("PRT_ITEM{0:D2}", i + 1).Equals("PRT_ITEM23") || string.Format("PRT_ITEM{0:D2}", i + 1).Equals("PRT_ITEM24"))
                            {

                            }
                            else
                            {
                                I_ATTVAL += "^";
                            }
                        }

                        INDATA.Columns.Add(string.Format("PRT_ITEM{0:D2}", i + 1), typeof(string));
                        drIndata[string.Format("PRT_ITEM{0:D2}", i + 1)] = item_value;
                    }

                    // 2022.03.16 염규범 선임
                    // ESNA 의 개발건, 라벨 디자인 부재로 인하여서, 임시 테스트용
                    //INDATA.Columns.Add("PRT_ITEM23", typeof(string));
                    //drIndata["PRT_ITEM23"] = dtInput.Rows[0]["ITEM023"].ToString();
                    //INDATA.Columns.Add("PRT_ITEM24", typeof(string));
                    //drIndata["PRT_ITEM24"] = dtInput.Rows[0]["ITEM024"].ToString(); 

                    INDATA.Rows.Add(drIndata);

                    
                }

                getZpl(I_ATTVAL, labelCode);

                //drIndata["LABEL_ZPL_CNTT"] = zpl;

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    tbPrint2_cnt2.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (i + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                    //if (!zpl.Contains("not exist !"))
                    //{                                 
                        new ClientProxy().ExecuteServiceSync("DA_PRD_INS_PACKLABEL_HIST", "INDATA", "", INDATA);
                    //}
                 
                }

            }
            catch (Exception ex)
            {
                throw ex;
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
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void setCombo_EVPROD()
        {
            try
            {
                string label_codes = string.Empty;

                if (dtLabelCodesEV != null && dtLabelCodesEV.Rows.Count > 0)
                {
                    label_codes = dtLabelCodesEV.Rows[0]["LABEL_CODE1"].ToString();

                    if (dtLabelCodes.Rows[0]["LABEL_CODE2"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodesEV.Rows[0]["LABEL_CODE2"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE3"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodesEV.Rows[0]["LABEL_CODE3"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE4"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodesEV.Rows[0]["LABEL_CODE4"].ToString();
                    }

                    if (dtLabelCodes.Rows[0]["LABEL_CODE5"].ToString() != "")
                    {
                        label_codes += "," + dtLabelCodesEV.Rows[0]["LABEL_CODE5"].ToString();
                    }
                }
                else
                {
                    label_codes = "LBL0020,LBL0067";
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

                cboEVProduct.DisplayMemberPath = "CBO_NAME";
                cboEVProduct.SelectedValuePath = "CBO_CODE";
                cboEVProduct.ItemsSource = DataTableConverter.Convert(dtModelResult);

                cboEVProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private string getLineEV()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = Util.GetCondition(cboEVProduct);
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID.ToString();

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LINE_WITH_PRODID_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return "6";
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

        //2019.11.26
        private void getValueSetting2()
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
                dr["PRODID"] = Util.GetCondition(cboEVProduct);
                dr["LABEL"] = Util.GetCondition(txtEVSeletedLabel);
                dr["SHIPTO_ID"] = Util.GetCondition(cboEVProduct_Out);
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                dtTextResultEV = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRINTVALUE_FIND", "INDATA", "OUTDATA", RQSTDT);


                if (dtTextResultEV == null || dtTextResultEV.Rows.Count == 0)
                {
                    //ms.AlertWarning("추가"); //MMD 기준정보 누락 : 출하처 인쇄항목 관리
                    return;
                }
                else
                {
                    setTextBoxEV();

                    dtpEVDate_SelectedDateChanged(null, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2019.11.26
        private void Get_Product_Lot_EV()
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
                dr["PRODID"] = Util.GetCondition(cboEVProduct);
                dr["FROMDATE"] = Util.GetCondition(dtpEVDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpEVDateTo);

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_COUNT_VOLVOBMA", "INDATA", "OUTDATA", RQSTDT);

                dgEVResult.ItemsSource = null;
                tbEVPalletHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                Util.GridSetData(dgEVResult, dtResult, FrameOperation);

                Util.SetTextBlockText_DataGridRowCount(tbEVPalletHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
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
                dr["PRODID"] = Util.GetCondition(cboEVProduct);
                dr["SHIPTO"] = Util.GetCondition(cboEVProduct_Out);

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

        //2019.11.26
        private void getLabelCodeEV()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CONTRY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "PACK_LABEL_CODE";
                dr["CONTRY"] = "CNA1";

                RQSTDT.Rows.Add(dr);

                dtLabelCodesEV = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_CODE_FIND", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void setCombo_EVOut()
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
                dr["LABEL_CODE"] = Util.GetCondition(txtEVSeletedLabel);
                dr["PRODID"] = Util.GetCondition(cboEVProduct);

                RQSTDT.Rows.Add(dr);

                DataTable dtShipto = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LABEL_SHIPTO", "INDATA", "OUTDATA", RQSTDT);

                cboEVProduct_Out.DisplayMemberPath = "CBO_NAME";
                cboEVProduct_Out.SelectedValuePath = "CBO_CODE";
                cboEVProduct_Out.ItemsSource = DataTableConverter.Convert(dtShipto);

                cboEVProduct_Out.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private string Convert_LabelItems(string strQuery)
        {
            string strRetrun = string.Empty;

            try
            {
                //BR_PRD_SEL_DYNAMIC_SQL
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STRQUERY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STRQUERY"] = strQuery;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_DYNAMIC_SQL", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    strRetrun = dtResult.Rows[0]["RESULT"].ToString();
                }

                return strRetrun;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Method

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

        private DataTable getInputData(int tab_Index)
        {
            DataTable dt = new DataTable();
            //2019.11.26
            //dt.TableName = "INPUTDATA";
            //dt.Columns.Add("ITEM001", typeof(string));
            //dt.Columns.Add("ITEM002", typeof(string));
            //dt.Columns.Add("ITEM003", typeof(string));
            //dt.Columns.Add("ITEM004", typeof(string));
            //dt.Columns.Add("ITEM005", typeof(string));
            //dt.Columns.Add("ITEM006", typeof(string));
            //dt.Columns.Add("ITEM007", typeof(string));
            //dt.Columns.Add("ITEM008", typeof(string));
            //dt.Columns.Add("ITEM009", typeof(string));
            //dt.Columns.Add("ITEM010", typeof(string));
            //dt.Columns.Add("ITEM011", typeof(string));
            //dt.Columns.Add("ITEM012", typeof(string));
            //dt.Columns.Add("ITEM013", typeof(string));
            //dt.Columns.Add("ITEM014", typeof(string));
            //dt.Columns.Add("ITEM015", typeof(string));
            //dt.Columns.Add("ITEM016", typeof(string));
            //dt.Columns.Add("ITEM017", typeof(string));
            //dt.Columns.Add("ITEM018", typeof(string));
            //dt.Columns.Add("ITEM019", typeof(string));
            //dt.Columns.Add("ITEM020", typeof(string));
            //dt.Columns.Add("ITEM021", typeof(string));
            //dt.Columns.Add("ITEM022", typeof(string));

            //DataRow dr = dt.NewRow();
            //dr["ITEM001"] = Util.GetCondition(txtReceive); //VOLVO TORSLANDA
            //dr["ITEM002"] = Util.GetCondition(txtDock); //TCV
            //dr["ITEM003"] = Util.GetCondition(txtAdvice); //61606292
            //dr["ITEM004"] = bcAdvice.Text; //61606292
            //dr["ITEM005"] = Util.GetCondition(txtSupplierAddress); // LG Chem, Ltd
            //dr["ITEM006"] = Util.GetCondition(txtNetWeight); //115
            //dr["ITEM007"] = Util.GetCondition(txtGrossWeight); //160
            //dr["ITEM008"] = Util.GetCondition(txtBoxes); //1
            //dr["ITEM009"] = Util.GetCondition(txtpartNumber); //31491834
            //dr["ITEM010"] = bcpartNumber.Text; //31491834
            //dr["ITEM011"] = Util.GetCondition(txtquantity); //1
            //dr["ITEM012"] = bcquantity.Text; //1
            //dr["ITEM013"] = Util.GetCondition(txtDescription1); //BATTERY PACK
            //dr["ITEM014"] = Util.GetCondition(txtDescription2); //355V,25.9A,6500Wh(Usable)
            //dr["ITEM015"] = Util.GetCondition(txtSupplierID); //GE2PB
            //dr["ITEM016"] = bcSupplierID.Text; //GE2PB
            //dr["ITEM017"] = Util.GetCondition(txtDate); //D160629
            //dr["ITEM018"] = Util.GetCondition(txtSerial); //616242017
            //dr["ITEM019"] = bcSerial.Text; //616242017
            //dr["ITEM020"] = Util.GetCondition(txtBatch); //616242017
            //dr["ITEM021"] = bcBatch.Text; //616242017
            //dr["ITEM022"] = Util.GetCondition(txtReceive2); //MONTERING
            //dt.Rows.Add(dr);

            if (tab_Index == 0)
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
                

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = Util.GetCondition(txtReceive); //VOLVO TORSLANDA
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
                dr["ITEM022"] = Util.GetCondition(txtReceive2); //MONTERING
                dt.Rows.Add(dr);
            }
            else if (tab_Index == 1)
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

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = Util.GetCondition(txtEVReceive);
                dr["ITEM002"] = Util.GetCondition(txtEVReceive2);
                dr["ITEM003"] = Util.GetCondition(txtEVAdvice);         // Advice Note No   신규 인터페이스 ( IWMS ) : (PRT_ITEM03)ITEM003 ->  
                //2020.06.24
                //dr["ITEM004"] = bcAdvice.Text;
                dr["ITEM004"] = bcEVAdvice.Text;
                dr["ITEM005"] = Util.GetCondition(txtEVpartNumber);     // PART NUMBER      신규 인터페이스 ( IWMS ) : (PRT_ITEM05)ITEM005 -> 
                dr["ITEM006"] = bcEVpartNumber.Text;
                dr["ITEM007"] = Util.GetCondition(txtEVquantity);       // Quantity         신규 인터페이스 ( IWMS ) : (PRT_ITEM07)ITEM007 -> 
                dr["ITEM008"] = bcEVquantity.Text;
                dr["ITEM009"] = Util.GetCondition(txtEVSupplierID);     // Supplier ID      신규 인터페이스 ( IWMS ) : (PRT_ITEM09)ITEM009 -> 
                dr["ITEM010"] = bcEVSupplierID.Text;
                dr["ITEM011"] = Util.GetCondition(txtEVSerial);         // Serial ID        신규 인터페이스 ( IWMS ) : (PRT_ITEM11)ITEM011 -> 
                dr["ITEM012"] = bcEVSerial.Text;
                dr["ITEM013"] = Util.GetCondition(txtEVDock);
                dr["ITEM014"] = Util.GetCondition(txtEVSupplierAddress);
                dr["ITEM015"] = Util.GetCondition(txtEVNetWeight);
                dr["ITEM016"] = Util.GetCondition(txtEVGrossWeight);
                dr["ITEM017"] = Util.GetCondition(txtEVBoxes);
                dr["ITEM018"] = Util.GetCondition(txtEVDescription1);
                dr["ITEM019"] = Util.GetCondition(txtEVLogistic1);
                dr["ITEM020"] = Util.GetCondition(txtOrigin);
                dr["ITEM021"] = Util.GetCondition(txtEVDate);
                dr["ITEM022"] = Util.GetCondition(txtCharge);
                dr["ITEM023"] = Util.GetCondition(txtPalletId);
                dr["ITEM024"] = Util.GetCondition(txtPalletId);
                dt.Rows.Add(dr);
            }

            else
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

                DataRow dr = dt.NewRow();
                dr["ITEM001"] = Util.GetCondition(txtEVReceive);
                dr["ITEM002"] = Util.GetCondition(txtEVReceive2);
                dr["ITEM003"] = Util.GetCondition(txtEVAdvice);
                //2020.06.24
                //dr["ITEM004"] = bcAdvice.Text;
                dr["ITEM004"] = bcEVAdvice.Text;
                dr["ITEM005"] = Util.GetCondition(txtEVpartNumber);
                dr["ITEM006"] = bcEVpartNumber.Text;
                dr["ITEM007"] = Util.GetCondition(txtEVquantity);
                dr["ITEM008"] = bcEVquantity.Text;
                dr["ITEM009"] = Util.GetCondition(txtEVSupplierID);
                dr["ITEM010"] = bcEVSupplierID.Text;
                dr["ITEM011"] = Util.GetCondition(txtEVSerial);
                dr["ITEM012"] = bcEVSerial.Text;
                dr["ITEM013"] = Util.GetCondition(txtEVDock);
                dr["ITEM014"] = Util.GetCondition(txtEVSupplierAddress);
                dr["ITEM015"] = Util.GetCondition(txtEVNetWeight);
                dr["ITEM016"] = Util.GetCondition(txtEVGrossWeight);
                dr["ITEM017"] = Util.GetCondition(txtEVBoxes);
                dr["ITEM018"] = Util.GetCondition(txtEVDescription1);
                dr["ITEM019"] = Util.GetCondition(txtEVLogistic1);
                dr["ITEM020"] = Util.GetCondition(txtOrigin);
                dr["ITEM021"] = Util.GetCondition(txtEVDate);
                dr["ITEM022"] = Util.GetCondition(txtCharge);
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string PrintOutDate(DateTime dt)
        {
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US", true);
            return dt.ToString("dd") + dt.ToString("MMMM", format).Substring(0, 3).ToUpper() + dt.ToString("yyyy");
        }

        //2019.11.26
        //private void PrintProcess(Button btn
        private void PrintProcess(Button btn, int tab_idx)
        {
            //2019.11.26
            //if (!bkWorker.IsBusy)
            //{
            //    blPrintStop = false;
            //    bkWorker.RunWorkerAsync();

            //    btn.Content = ObjectDic.Instance.GetObjectName("취소");
            //    btn.Foreground = Brushes.White;
            //}
            //else
            //{
            //    blPrintStop = true;
            //    btn.Content = ObjectDic.Instance.GetObjectName("출력");
            //    btn.Foreground = Brushes.Red;
            //}

            if (tab_idx == 0)
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
            else
            {
                if (!bkEVWorker.IsBusy)
                {
                    blPrintStop = false;
                    bkEVWorker.RunWorkerAsync();

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

                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void bkEVWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    PrintAcessEV();
                    //PrintAcess();
                }));
            }
            catch (Exception ex)
            {
                bkEVWorker.CancelAsync();
                blPrintStop = true;

                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button btn = new Button();

            btn = btnPrint;

            btn.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btn.Foreground = Brushes.White;
        }

        //2019.11.26
        private void bkEVWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Button btn = new Button();

            btn = btnEVPrint;

            btn.Content = ObjectDic.Instance.GetObjectName("출력");
            blPrintStop = true;
            btn.Foreground = Brushes.White;
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

                I_ATTVAL = labelItemsGet(labelCode, tab_idx);

                getZpl(I_ATTVAL, labelCode);

                //if (LoginInfo.USERID.Trim() == "cnswkdakscjf")
                //{
                //    wndPopup = new CMM_ZPL_VIEWER2(zpl);
                //    wndPopup.Show();
                //}

                for (int i = 0; i < nbPrintCnt.Value; i++)
                {
                    if (blPrintStop) break;

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                    tbPrint2_cnt.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : " + (i + 1).ToString() + ObjectDic.Instance.GetObjectName("건") + "]";
                    System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);
                }

                //2020.06.24
                if ((LoginInfo.USERID.Trim() == "ogong"))
                {
                    Preview popup = new Preview(labelCode, zpl);
                    popup.Show();
                }

                //ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
            }
        }

        //2019.11.26
        private void PrintAcessEV()
        {
            try
            {
                string I_ATTVAL = string.Empty;
                CMM_ZPL_VIEWER2 wndPopup;
                Button btn = null;
                string labelCode = string.Empty;
                int tab_idx = tcMain.SelectedIndex;

                btn = btnEVPrint;
                tbPrint2_cnt2.Text = "[" + ObjectDic.Instance.GetObjectName("인쇄장수") + " : 0 " + ObjectDic.Instance.GetObjectName("건") + "]";
                labelCode = label_code;

                labelItemsGetEV(labelCode, tab_idx);

               

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintBoxLabel(string sZpl)
        {
            try
            {
                Util.PrintLabel(FrameOperation, loadingIndicator, sZpl);
                System.Threading.Thread.Sleep((int)nbDelay.Value * 1000);

                if (LoginInfo.USERID.Trim() == "ogong")
                {
                    //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZpl);
                    //wndPopup.Show();
                    Preview popup = new Preview("", sZpl);
                    popup.Show();
                }

                //wndPopup.Show();
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

                if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
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
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
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
            txtReceive.Text = returnString(dt, "ITEM001");          //Receiver : VOLVO TORSLANDA  
            txtReceive2.Text = returnString(dt, "ITEM022");          //Receiver : MONTERING           
            txtDock.Text = returnString(dt, "ITEM002");             //DOCK GATE : TCV
            txtSupplierAddress.Text = returnString(dt, "ITEM005");  //Supplier Address : LG Chem, Ltd
            txtBoxes.Text = returnString(dt, "ITEM008");            //No of Boxes : 1            
            txtLogistic1.Text = "logic";
            txtLogistic2.Text = "reference";
            txtDescription1.Text = returnString(dt, "ITEM013");     //Description : BATTERY PACK

            txtAdvice.Text = returnString(dt, "ITEM003");           //ADVICE NOTE NO : 61606292
            txtpartNumber.Text = returnString(dt, "ITEM009"); //"31407014";                    //Part No : 31491834
            txtquantity.Text = returnString(dt, "ITEM011");         //Quantity : 1
            txtSupplierID.Text = returnString(dt, "ITEM015");       //Supplier ID : GE2PB
            txtSerial.Text = returnString(dt, "ITEM018");           //Serial No : 312031601
            txtBatch.Text = returnString(dt, "ITEM020");            //Batch No : 312031601

            if (tabItem.Header.ToString() == "313H" || tabItem.Header.ToString() == "517H" || tabItem.Header.ToString() == "HORI")
            {
                bcAdvice.Text = "N" + txtAdvice.Text;           //ADVICE NOTE NO : N61606292
                bcpartNumber.Text = "P" + txtpartNumber.Text;   //Part No : P31491834
                bcquantity.Text = "Q" + txtquantity.Text;       //Quantity : Q1
                bcSupplierID.Text = "V" + txtSupplierID.Text;   //Supplier ID : VGE2PB
                bcSerial.Text = "S" + txtSerial.Text;           //Serial No : S12031601
                bcBatch.Text = "H" + txtBatch.Text;            //Batch No : H312031601
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

        //2019.11.26
        private void setTextBoxEV()
        {
            if (dtTextResultEV == null || dtTextResultEV.Rows.Count == 0)
            {
                return;
            }

            DataTable dt = dtTextResultEV;

            //Receiver
            txtEVReceive.Text = returnString(dt, "ITEM001");   
            //2020.06.24
            //txtEVReceive2.Text = returnString(dt, "ITEM022");
            txtEVReceive2.Text = returnString(dt, "ITEM002");

            //ADVICE NOTE NO
            //2020.06.24
            //txtEVAdvice.Text = returnString(dt, "ITEM003");
            txtEVAdvice.Text = Convert_LabelItems(returnString(dt, "ITEM003").Replace("@LOTID", strLotIdEV));
            bcEVAdvice.Text = "N" + txtEVAdvice.Text;

            //Part No
            txtEVpartNumber.Text = returnString(dt, "ITEM005");
            bcEVpartNumber.Text = "P" + txtEVpartNumber.Text;

            //Quantity
            //txtEVquantity.Text = returnString(dt, "ITEM007");
            strQuantity = returnString(dt, "ITEM007");
            txtEVquantity.Text = Convert_LabelItems(returnString(dt, "ITEM007").Replace("@LOTID", srePalletIdEV));
            bcEVquantity.Text = "Q" + txtEVquantity.Text;

            //Supplier ID
            txtEVSupplierID.Text = returnString(dt, "ITEM009");
            bcEVSupplierID.Text = "V" + txtEVSupplierID.Text;

            //Serial No
            //txtEVSerial.Text = returnString(dt, "ITEM011");           
            txtEVSerial.Text = Convert_LabelItems(returnString(dt, "ITEM011").Replace("@LOTID", strLotIdEV));
            bcEVSerial.Text = "S" + txtEVSerial.Text;

            //DOCK GATE
            txtEVDock.Text = returnString(dt, "ITEM013");

            //Supplier Address : Nanjing LG Chem New Energy Battery
            txtEVSupplierAddress.Text = returnString(dt, "ITEM014");

            //Net Weight(kg)
            txtEVNetWeight.Text = returnString(dt, "ITEM015");
           
            //Gross Weight(kg)
            txtEVGrossWeight.Text = returnString(dt, "ITEM016");
            
            //No of Boxes
            txtEVBoxes.Text = returnString(dt, "ITEM017");

            //Description
            txtEVDescription1.Text = returnString(dt, "ITEM018");     

            txtEVLogistic1.Text = returnString(dt, "ITEM019");

            // Country of Origin
            txtOrigin.Text = returnString(dt, "ITEM020");

            //Eng Change
            txtCharge.Text = returnString(dt, "ITEM022");     
        }

        //2019.11.26
        private void setEVBacode(object sender)
        {
            try
            {
                TextBox tbBox = (TextBox)sender;

                switch (tbBox.Name)
                {
                    case "txtEVAdvice":
                        bcEVAdvice.Text = "N" + tbBox.Text;
                        break;
                    case "txtEVpartNumber":
                        bcEVpartNumber.Text = "P" + tbBox.Text;
                        break;
                    case "txtEVquantity":
                        bcEVquantity.Text = "Q" + tbBox.Text;
                        break;
                    case "txtEVSupplierID":
                        bcEVSupplierID.Text = "V" + tbBox.Text;
                        break;
                    case "txtEVSerial":
                        bcEVSerial.Text = "S" + tbBox.Text;
                        break;
                    case "txtEVBatch":
                        bcEVBatch.Text = "H" + tbBox.Text;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                //2020.06.24
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        #region 사용안함

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


        private string MakeZPLString()
        {
            try
            {
                string strResult = string.Empty;
                //string strBackRollCheck = "B";

                //strResult = "^XA^MCY^XZ^XA^LRN^FWN^CFD,24^LH0,0^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^XZ^XA";

                //strResult += "^POI";

                //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    //2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가
                //    strResult += string.Format("^A0R,70,55^FO1200,30^CI0^FD{0}^FS", txtReceive.Text); //VOLVO TORSLANDA MONTERING
                //    strResult += string.Format("^A0R,70,60^FO1090,230^CI0^FD{0}^FS", txtAdvice.Text); //TTLC1601
                //    strResult += string.Format("^BY4,2.8^FO990,50^B3R,N,104,N,N^FDN{0}^FS", txtAdvice.Text); //TTLC1601(N)
                //    strResult += string.Format("^A0R,138,0^FO820,230^CI0^FD{0}^FS", txtpartNumber.Text); //50634477
                //    strResult += string.Format("^BY4,2.8^FO740,50^B3R,N,104,N,N^FDP{0}^FS", txtpartNumber.Text); //50634477(P)
                //    strResult += string.Format("^A0R,138,0^FO578,230^CI0^FD{0}^FS", txtquantity.Text); //1
                //    strResult += string.Format("^BY5,2.8^FO505,50^B3R,N,104,N,N^FDQ{0}^FS", txtquantity.Text); //1(Q)
                //    strResult += string.Format("^A0R,50,0^FO440,230^CI0^FD{0}^FS", txtSupplierID.Text); //GE2PB
                //    strResult += string.Format("^BY4,2.8^FO345,50^B3R,N,104,N,N^FDV{0}^FS", txtSupplierID.Text); //GE2PB(V)
                //    strResult += string.Format("^A0R,50,0^FO270,230^CI0^FD{0}^FS", txtSerial.Text); //312031601
                //    strResult += string.Format("^BY3,2.8^FO170,50^B3R,N,104,N,N^FDS{0}^FS", txtSerial.Text); //312031601(S)


                //    strResult += string.Format("^A0R,138,138^FO1160,1100^CI0^FD{0}^FS", txtDock.Text); //TVV
                //    strResult += string.Format("^A0R,50,0^FO1080,808^CI0^FD{0}^FS", txtSupplierAddress.Text); //LG Chem, Ltd.
                //    strResult += string.Format("^A0R,70,0^FO960,880^CI0^FD{0}^FS", txtNetWeight.Text); //150
                //    strResult += string.Format("^A0R,70,0^FO960,1180^CI0^FD{0}^FS", txtGrossWeight.Text); //180
                //    strResult += string.Format("^A0R,70,0^FO960,1480^CI0^FD{0}^FS", txtBoxes.Text); //1
                //    strResult += string.Format("^A0R,70,0^FO623,802^CI0^FD{0}^FS", txtDescription1.Text); //BATTERY PACK

                //    if ((bool)rdb515H.IsChecked)
                //        strResult += string.Format("^A0R,35,30^FO640,1280^CI0^FD{0}^FS", txtDescription2.Text); //355V,25.9A, 6,500Wh(Usable)
                //    else
                //        strResult += string.Format("^A0R,50,40^FO640,1280^CI0^FD{0}^FS", txtDescription2.Text); //375V, 30Ah, 11,250Wh

                //    //2203316 312H 포장박스라벨 수정요청
                //    strResult += string.Format("^A0R,50,0^FO523,880^CI0^FD{0}^FS", txtLogistic1.Text); //Master Label Number
                //    strResult += string.Format("^A0R,50,0^FO473,880^CI0^FD{0}^FS", txtLogistic2.Text); //TTLC1601
                //                                                                                        //2203316 312H 포장박스라벨 수정요청

                //    strResult += string.Format("^A0R,70,0^FO330,850^CI0^FD{0}^FS", txtDate.Text); //D120323
                //    strResult += string.Format("^A0R,50,0^FO270,980^CI0^FD{0}^FS", txtBatch.Text);  //312031601
                //    strResult += string.Format("^BY3,2.8^FO170,850^B3R,N,104,N,N^FDH{0}^FS", txtBatch.Text); //312031601(H)
                //                                                                                                //2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가           
                //}));

                ////strResult += "^POI";
                //////2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가
                ////strResult += string.Format("^A0R,70,55^FO1200,30^CI0^FD{0}^FS", txt312H01.Text); //VOLVO TORSLANDA MONTERING
                ////strResult += string.Format("^A0R,70,60^FO1090,230^CI0^FD{0}^FS", txt312H03.Text); //TTLC1601
                ////strResult += string.Format("^BY4,2.8^FO990,50^B3R,N,104,N,N^FDN{0}^FS", txt312H03.Text); //TTLC1601(N)
                ////strResult += string.Format("^A0R,138,0^FO820,230^CI0^FD{0}^FS", txt312H08.Text); //50634477
                ////strResult += string.Format("^BY4,2.8^FO740,50^B3R,N,104,N,N^FDP{0}^FS", txt312H08.Text); //50634477(P)
                ////strResult += string.Format("^A0R,138,0^FO578,230^CI0^FD{0}^FS", txt312H09.Text); //1
                ////strResult += string.Format("^BY5,2.8^FO505,50^B3R,N,104,N,N^FDQ{0}^FS", txt312H09.Text); //1(Q)
                ////strResult += string.Format("^A0R,50,0^FO440,230^CI0^FD{0}^FS", txt312H12.Text); //GE2PB
                ////strResult += string.Format("^BY4,2.8^FO345,50^B3R,N,104,N,N^FDV{0}^FS", txt312H12.Text); //GE2PB(V)
                ////strResult += string.Format("^A0R,50,0^FO270,230^CI0^FD{0}^FS", txt312H14.Text); //312031601
                ////strResult += string.Format("^BY3,2.8^FO170,50^B3R,N,104,N,N^FDS{0}^FS", txt312H14.Text); //312031601(S)


                ////strResult += string.Format("^A0R,138,138^FO1160,1100^CI0^FD{0}^FS", txt312H02.Text); //TVV
                ////strResult += string.Format("^A0R,50,0^FO1080,808^CI0^FD{0}^FS", txt312H04.Text); //LG Chem, Ltd.
                ////strResult += string.Format("^A0R,70,0^FO960,880^CI0^FD{0}^FS", txt312H05.Text); //150
                ////strResult += string.Format("^A0R,70,0^FO960,1180^CI0^FD{0}^FS", txt312H06.Text); //180
                ////strResult += string.Format("^A0R,70,0^FO960,1480^CI0^FD{0}^FS", txt312H07.Text); //1
                ////strResult += string.Format("^A0R,70,0^FO623,802^CI0^FD{0}^FS", txt312H10.Text); //BATTERY PACK
                ////if ((bool)rdb515H.IsChecked)
                ////    strResult += string.Format("^A0R,35,30^FO640,1280^CI0^FD{0}^FS", txt312H11.Text); //355V,25.9A, 6,500Wh(Usable)
                ////else
                ////    strResult += string.Format("^A0R,50,40^FO640,1280^CI0^FD{0}^FS", txt312H11.Text); //375V, 30Ah, 11,250Wh

                //////2203316 312H 포장박스라벨 수정요청
                ////strResult += string.Format("^A0R,50,0^FO523,880^CI0^FD{0}^FS", txtLogisticRef1.Text); //Master Label Number
                ////strResult += string.Format("^A0R,50,0^FO473,880^CI0^FD{0}^FS", txtLogisticRef2.Text); //TTLC1601
                //////2203316 312H 포장박스라벨 수정요청

                ////strResult += string.Format("^A0R,70,0^FO330,850^CI0^FD{0}^FS", txt312H13.Text); //D120323
                ////strResult += string.Format("^A0R,50,0^FO270,980^CI0^FD{0}^FS", txt312H15.Text);  //312031601
                ////strResult += string.Format("^BY3,2.8^FO170,850^B3R,N,104,N,N^FDH{0}^FS", txt312H15.Text); //312031601(H)
                //////2178985 312H Volvo Box 라벨 출력 시 바코드 구분자 추가

                //strResult += "^PQ1,0,1,Y^XZ";

                ////Clipboard.SetText(strResult);
                ////File.WriteAllText("312H" + "_BOX.txt", strResult.Replace("^FS", "^FS" + Environment.NewLine));
                //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                //{
                //    Util.AlertInfo(strResult.Replace("^FS", "^FS"));
                //}));

                return strResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion 사용안함
    }
}