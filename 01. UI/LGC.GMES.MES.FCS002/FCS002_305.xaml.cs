/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.04.05  JHM       : C20220329-000427 - Tesla Contents Label 양식 변경
  2022.04.27  JHM       : C20220426-000440 - Tesla Contents Label 발행 시 필수 항목 체크
  2022.05.24  JHM       : C20220515-000013 - Tesla Contents Label 원산지 표시 변경
  2022.06.20  JHM       : C20220620-000070 - Tesla Contents Label 양식 변경 2
  2023.01.05  김린겸    : C20221212-000579 Tesla label ID quantity check
  2023.03.13  이홍주    : 소형활성화 MES 복사
  2023.11.14  이홍주    : 3S TESLA 라벨을 PDF 및 ZPL로 출력하게 수정
  2024.03.14  이홍주    : INBOX 라벨 신규 및 재발 발행 기능추가
  2024.03.21  이홍주    : 3S 라벨 재발행시 기존 ZPLCODE 활용하게 변경
  2024.05.03  이홍주    : INBOX 신규 발행시 발행수를 선택 할수 있게 변경
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_305 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        string strZPL = string.Empty;
        string zpl = string.Empty;


        string siteCode = "";
        string strSupplierNo = "";
                
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;

        int iInboxSeq = 9999;

        DataRow drPrtInfo = null;


        public FCS002_305()
        {

            InitializeComponent();
            initCombo();
            initText();
            
            this.Loaded += FCS002_305_Loaded;
        }

        private void initText()
        {
            try
            {
                //COUNTRY_OF_ORIGIN
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstDt.Columns.Add("CMCODE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "TESLA_LABEL_COUNTRY_OF_ORIGIN";

                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    drnewrow["CMCODE"] = "A010";
                }
                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) //오창2산단 소형조립
                {
                    drnewrow["CMCODE"] = "F030";
                }

                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                {
                    drnewrow["CMCODE"] = "G182";
                }
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

                DataTable dataTable = new DataTable();

               txtCountry.Text = dtResult.Rows[0]["ATTRIBUTE1"].ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        private void initCombo()
        {
            try
            {
                //PART NUM
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_PART_NUM";
                }
                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) //오창2산단 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_PART_NUM_OC2";
                }
                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_PART_NUM_NJ";
                }
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("VALUE", typeof(string));
                dataTable.Columns.Add("NAME", typeof(string));

                for(int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dataTable.Rows.Add(new string[] { dtResult.Rows[i]["ATTRIBUTE1"].ToString(), dtResult.Rows[i]["CMCODE"].ToString() });
                }

                cboTeslaPartNum.ItemsSource = dataTable.DefaultView;
                cboTeslaPartNum.DisplayMemberPath = "NAME";
                cboTeslaPartNum.SelectedValuePath = "VALUE";

                //SUPPLIER
                dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));

                drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER";
            
                dtRqstDt.Rows.Add(drnewrow);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

                dataTable = new DataTable();

                dataTable.Columns.Add("VALUE", typeof(string));
                dataTable.Columns.Add("NAME", typeof(string));

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dataTable.Rows.Add(new string[] { dtResult.Rows[i]["ATTRIBUTE1"].ToString(), dtResult.Rows[i]["CMCODE"].ToString() });
                }

                cboSupplier.ItemsSource = dataTable.DefaultView;
                cboSupplier.DisplayMemberPath = "NAME";
                cboSupplier.SelectedValuePath = "VALUE";



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void FCS002_305_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= FCS002_305_Loaded;
            
            Initialize();
            GetInBoxDaySeq();
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
           
        }
        #endregion

        #region Event

        //출력 수량 변경시
        private void numCnt_ValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<double> e)
        {


            if (numCnt.Value > 100)
            {
                numCnt.Value = 100;
            }

            GetInBoxDaySeq();
        }

        private void chkPrint_Click(object sender, RoutedEventArgs e)
        {
            //연속발행 checkbox 체크시에만 활성화시킴.

        }

        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2022.04.27 필수 입력 사항 체크
                //PART NUMBER
                if (string.IsNullOrEmpty(cboTeslaPartNum.Text))
                {
                    Util.MessageValidation("PSS9158"); // PART NUMBER를 선택하세요.
                    return;
                }
                //PART NAME
                if (string.IsNullOrEmpty(txtPartName.Text))
                {
                    Util.MessageValidation("PSS9159"); // PART NAME를 입력하세요.
                    return;
                }
                //QUANTITY
                if (string.IsNullOrEmpty(txtQuantity.Text))
                {
                    Util.MessageValidation("SFU1154"); // 수량을 입력하세요.
                    return;
                }
                //PO UNIT OF MEASURE
                if (string.IsNullOrEmpty(txtMeasure.Text))
                {
                    Util.MessageValidation("PSS9160"); // PO UNIT OF MEASURE를 입력하세요.
                    return;
                }
                //GROSS WEIGHT
                if (string.IsNullOrEmpty(txtGrossWeight.Text))
                {
                    Util.MessageValidation("PSS9161"); // GROSS WEIGHT를 입력하세요.
                    return;
                }
                //WEIGHT UNIT
                if (string.IsNullOrEmpty(txtWeightUnit.Text))
                {
                    Util.MessageValidation("PSS9162"); // WEIGHT UNIT을 입력하세요.
                    return;
                }
                //프린트 수량
                if (string.IsNullOrEmpty(txtTotalNum.Text))
                {
                    Util.MessageValidation("PSS9154"); // 프린트 수량을 입력하세요.
                    return;
                }

                int maxPrtQty = 300;

                if(int.Parse(txtTotalNum.Text) > maxPrtQty)
                {
                    Util.MessageValidation("PSS9157", maxPrtQty); // 프린트 수량은 %1을 초과할 수 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(cboSupplier.Text))
                {
                    Util.MessageValidation("SFU8481"); // SUPPLIER NAME을 선택하세요.
                    return;
                }


                string lotCode = txtLotCode.Text.ToString();

                if (string.IsNullOrEmpty(lotCode))
                {
                    Util.MessageValidation("SFU1379"); // LOT을 입력해주세요
                    return;
                }

            
                //해당 site Setting
                SiteSetting();
                PrintProcess();
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        //OUTBOX REPRINT
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
                if (rdo3S.IsChecked == true)
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


        //INBOX PRINT
        private void btnInboxPrint_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                //INBOX 재발행이면
                if (rdoReprint.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(txtInBoxID.Text))
                    {
                           //SFU4517 Inbox ID를 입력 하세요.
                           Util.MessageValidation("SFU4517"); 
                        return;
                    }
                }

                //선택된 방향이 없으면..
                if (chkA.IsChecked == false && chkB.IsChecked == false && chkC.IsChecked == false && chkD.IsChecked == false)
                {
                    //SFU4636 선택된 INBOX 정보가 없습니다.
                    Util.MessageValidation("SFU4636");
                    return;
                }
                PrintProcess_Inbox();
                

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }



        private void txtOutBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnRePrint_Click(null,null);
            }
        }


        #region txtInBoxID_KeyDown : 재발행시
        private void txtInBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sInboxId = (txtInBoxID.Text).Trim();

                if (sInboxId.Length > 21)  //NFF기준 뱡향포함 22자리
                {
                    string sDirection = sInboxId.Substring(21, 1);
                    if (sDirection == "A" || sDirection == "B" || sDirection == "C" || sDirection == "D")
                    {
                        sInboxId = sInboxId.Substring(0, 21);
                        txtInBoxID.Text = sInboxId;
                    }
                }

                btnInboxPrint_Click(null, null);
               
            }
        }
        #endregion


        private void btnLotCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lotCode = txtLotCode.Text.ToString();

                if (string.IsNullOrEmpty(lotCode)){
                    Util.MessageValidation("SFU8249"); // LotCode를 1자리 이상 입력해주세요.
                    return;
                }

                FCS002_305_LOT_SEARCH wndSearch = new FCS002_305_LOT_SEARCH(txtLotCode.Text.ToString());
                wndSearch.FrameOperation = FrameOperation;

                if (wndSearch != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = 1;
                    C1WindowExtension.SetParameters(wndSearch, Parameters);

                     wndSearch.Closed += new EventHandler(wndSearch_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSearch.ShowModal()));
                    //  grdMain.Children.Add(wndPrint);
                    //  wndPrint.BringToFront(); 
                }

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void wndSearch_Closed(object sender, EventArgs e)
        {
            FCS002_305_LOT_SEARCH window = sender as FCS002_305_LOT_SEARCH;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                txtLotCode.Text = window.lotCode;
            }
        }

        private void RePrint_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtOutBoxID.Focus();
        }

        private void rdo3S_Checked(object sender, RoutedEventArgs e)
        {
            //txtOutBoxID.Focus();
        }

        private void txtTotalNum_changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtCntLabel.Text = txtTotalNum.Text;
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }


        private void txtGrossWeight_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9 .]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txt_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        private void rdo3S_Click(object sender, RoutedEventArgs e)
        {
            txtOutBoxID.Focus();
        }



        private void rdoNewInbox_Click(object sender, RoutedEventArgs e)
        {
            txtInBoxID.Text = "";
            txtInBoxID.IsEnabled = false;

            chkA.IsChecked = true;
            chkB.IsChecked = true;
            chkC.IsChecked = true;
            chkD.IsChecked = true;
            numCnt.IsEnabled = true;

        }

        private void rdoReInbox_Click(object sender, RoutedEventArgs e)
        {
            txtInBoxID.IsEnabled = true;

            numCnt.IsEnabled = false;
            numCnt.Value = 1;
        }
        


        #endregion

        #region Mehod
        private void PrintProcess()
        {
            try
            {
                //string labelID = "3SADU" + string.Format("{0:000000}", Int32.Parse(txtQuantity.Text.ToString())) + "000" + "000"
                //                  + cboTeslaPartNum.SelectedValue.ToString().Replace("-","");

                //string Lot2D = P(Part Number 줄임말) 1626983-00-A (Part Number) :1T(Lot Code 의미함)H05AK03C(실제 Lot ID):15D(Expiration Date 의미함, 데이터 없음)
                //LOTID 2D 바코드 추가
                string Lot2D = "P" + cboTeslaPartNum.SelectedValue.ToString() + ":1T" + txtLotCode.Text.ToString() + ":15D";

                DataTable dtRqstDt = new DataTable();

                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstDt.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRqstDt.Columns.Add("ATTRIBUTE3", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();

                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER";
                }
                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) // 오창2산단 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER_OC2";
                }
                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                {
                    drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER_NJ";
                }

                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    drnewrow["ATTRIBUTE1"] = cboSupplier.SelectedValue.ToString();
                }

                else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")
                   || LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) //오창2산단 소형조립, 남경 소형조립
                {
                    drnewrow["ATTRIBUTE1"] = Util.NVC(cboTeslaPartNum.SelectedValue);
                }

                drnewrow["ATTRIBUTE3"] = Util.NVC(cboSupplier.Text.ToString());

                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", dtRqstDt);

                strSupplierNo = dtResult.Rows[0]["ATTRIBUTE4"].ToString();

                btnPrint.IsEnabled = false;

                Dictionary<string, string> dic = new Dictionary<string, string>();

                dic.Add("MFG_Part_Num", txtMfgPartNum.Text.ToString());
                dic.Add("CONTRY_OF_ORIGIN", txtCountry.Text.ToString());
                dic.Add("QTY", txtQuantity.Text.ToString());
                dic.Add("QTY_UNIT", txtMeasure.Text.ToString());
                dic.Add("WEIGHT", txtGrossWeight.Text.ToString() + " " + txtWeightUnit.Text.ToString());
                dic.Add("PART_NAME", txtPartName.Text.ToString());
               
                dic.Add("QRcode1", Lot2D);
               
                dic.Add("SUPPLIER", "          " + strSupplierNo + "\r\n" + cboSupplier.Text.ToString());
                dic.Add("PRINT_DATE", dtpPrintDate.SelectedDateTime.Month + "/" + dtpPrintDate.SelectedDateTime.Day + "/" + dtpPrintDate.SelectedDateTime.Year);
                dic.Add("EXP_DATE", txtExpDate.Text.ToString());
                dic.Add("SERIAL_NUM", txtSerialNum.Text.ToString());
                dic.Add("LOT_CODE", txtLotCode.Text.ToString());
                dic.Add("PART_NUM", cboTeslaPartNum.SelectedValue.ToString());
                dic.Add("SITE_CODE", siteCode);
                dic.Add("REPRINT_YN", "N");

                int prtQTY = 0;

                if (!string.IsNullOrEmpty(txtTotalNum.Text.ToString())){
                    prtQTY = Int32.Parse(txtTotalNum.Text.ToString());
                }

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
                        this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
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

        //해당일자의 INBOX라벨(LBL0352) 출력 SEQ 구하기
        private void GetInBoxDaySeq()
        {
            try
            {
                DataTable dtRqstDt = new DataTable();

                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LABEL_CODE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LABEL_CODE"] = "LBL0352";

                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LABEL_PRT_SEQ_MB", "RQSTDT", "RSLTDT", dtRqstDt);

                if (dtResult.Rows.Count > 0)
                {
                    iInboxSeq = dtResult.Rows[0]["SEQ_BY_DAY"].SafeToInt32();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (txtSeqFromTo != null && numCnt.Value.SafeToInt32() > 0 && numCnt.Value.SafeToInt32() < 999)
                {
                    txtSeqFromTo.Text = iInboxSeq.ToString() + " ~ " + (iInboxSeq - (numCnt.Value.SafeToInt32() - 1)).ToString();
                }
            }
            
       }


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

                btnPrint.IsEnabled = false;

                Dictionary<string, string> dic = new Dictionary<string, string>();

                ///dic.Add("MFG_Part_Num", txtMfgPartNum.Text.ToString());
                ///

                
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
                        this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
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
        #region PrintProcess_Inbox : INBOX(재)출력
        private void PrintProcess_Inbox()
        {
            try
            {
                // MMD0117 - 라벨을 발행 하시겠습니까?                
                Util.MessageConfirm("MMD0117", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sInboxId = (txtInBoxID.Text).Trim();

                        if (sInboxId.Length > 21)  //NFF기준 뱡향포함 22자리
                        {
                            string sDirection = sInboxId.Substring(21, 1);
                            if (sDirection == "A" || sDirection == "B" || sDirection == "C" || sDirection == "D")
                            {
                                sInboxId = sInboxId.Substring(0, 21);
                                txtInBoxID.Text = sInboxId;
                            }
                        }

                        Print_InBox_ZPL(sInboxId);
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion
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

        private void SiteSetting()
        {
            //사이트 코드 정보 셋팅
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
                    dr["ATTRIBUTE1"] = Util.NVC(cboTeslaPartNum.SelectedValue);
                    dr["CMCDNAME"] = Util.NVC(DataTableConverter.Convert(cboSupplier.ItemsSource).Rows[cboSupplier.SelectedIndex]["NAME"].ToString());
                    //dr["CMCDNAME"] = Util.NVC(cboSupplier.Text.ToString());
                    dtRqst.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_CMCDNAME", "RQSTDT", "RSLTDT", dtRqst);

                    if (dtResult.Rows.Count > 0)
                        siteCode = dtResult.Rows[0]["ATTRIBUTE5"].ToString();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string SiteSetting(string sTeslaPartNum, string sSupplierName)
        {
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

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstDt.Columns.Add("CMCODE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;


                drnewrow["CMCDTYPE"] = "TESLA_LABEL_CONTENT";

                drnewrow["CMCODE"] = Util.NVC(cboTeslaPartNum.SelectedValue);
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

                if (dtResult.Rows.Count > 0)
                {
                    txtPartName.Text = dtResult.Rows[0]["CMCDNAME"].ToString();
                    txtMeasure.Text = dtResult.Rows[0]["ATTRIBUTE1"].ToString();
                    txtWeightUnit.Text = dtResult.Rows[0]["ATTRIBUTE2"].ToString();
                    txtQuantity.Text = dtResult.Rows[0]["ATTRIBUTE4"].ToString();
                    txtGrossWeight.Text = dtResult.Rows[0]["ATTRIBUTE5"].ToString();
                }
                else
                {
                    txtPartName.Text = "";
                    txtMeasure.Text = "";
                    txtWeightUnit.Text = "";
                    txtQuantity.Text = "";
                    txtGrossWeight.Text = "";
                }

                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    siteCode = dtResult.Rows[0]["ATTRIBUTE3"].ToString();
                }
                else
                {
                    //SUPPLIER
                    dtRqstDt = new DataTable();
                    dtRqstDt.TableName = "RQSTDT";
                    dtRqstDt.Columns.Add("LANGID", typeof(string));
                    dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));

                    drnewrow = dtRqstDt.NewRow();
                    drnewrow["LANGID"] = LoginInfo.LANGID;
                    if (LoginInfo.CFG_SHOP_ID.ToString().Equals("F030")) // 오창2산단 소형조립
                    {
                        drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER_OC2";
                    }
                    else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                    {
                        drnewrow["CMCDTYPE"] = "TESLA_SUPPLIER_NJ";
                    }

                    dtRqstDt.Rows.Add(drnewrow);

                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);
                    //임시 추가
                    DataTable dataTable = new DataTable();

                    dataTable = new DataTable();

                    dataTable.Columns.Add("VALUE", typeof(string));
                    dataTable.Columns.Add("NAME", typeof(string));

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        //선택된 PartNumber의 해당되는 Supplier 
                        if (cboTeslaPartNum.SelectedValue.ToString() == dtResult.Rows[i]["ATTRIBUTE1"].ToString())
                        {
                            dataTable.Rows.Add(new string[] { dtResult.Rows[i]["ATTRIBUTE1"].ToString(), dtResult.Rows[i]["CMCDNAME"].ToString() });
                        }
                    }

                    cboSupplier.ItemsSource = dataTable.DefaultView;
                    cboSupplier.DisplayMemberPath = "NAME";
                    cboSupplier.SelectedValuePath = "VALUE";
                }//남경 소형조립

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void RePrint_1DBox_ZPL(string sOutBox1dLabel)
        {
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

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
                newRow["PRMK"] = sPrt; // "Z" : ZEBRA; Print type
                newRow["RESO"] = sRes; // "300": DPI
                newRow["PRCN"] = sCopy; // "1": Print Count
                newRow["MARH"] = sXpos; // "0": Horizone pos
                newRow["MARV"] = sYpos; // "0": Vertical pos
                newRow["DARK"] = sDark; // "15" : darkness
                inPrintTable.Rows.Add(newRow);


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
            }
        }

        #region Print_InBox_ZPL : Inbox 라벨출력
        private void Print_InBox_ZPL(string sInBoxId)
        {
            try
            {




                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    //SFU2003 프린트 환경 설정값이 없습니다.
                    Util.MessageInfo("SFU2003"); 
                    return;
                    
                }
                

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("LABELTYPE"); //사용안함
                inDataTable.Columns.Add("USERID");  //필수
                inDataTable.Columns.Add("INBOXID"); //재발행시 필수 (방향제외)
                inDataTable.Columns.Add("PRINT_QTY"); //신규발행시 필수 
                inDataTable.Columns.Add("NEW_LABEL_FLAG"); //신규 여부 (Y,N)
                inDataTable.Columns.Add("INBOX_A_FLAG"); //INBOX라벨 A 방향 출력여부 (Y,N)
                inDataTable.Columns.Add("INBOX_B_FLAG"); //INBOX라벨 B 방향 출력여부 (Y,N)
                inDataTable.Columns.Add("INBOX_C_FLAG"); //INBOX라벨 C 방향 출력여부 (Y,N)
                inDataTable.Columns.Add("INBOX_D_FLAG"); //INBOX라벨 D 방향 출력여부 (Y,N)
                
                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");    //Z : zebra
                inPrintTable.Columns.Add("RESO");    // "300"; DPI
                inPrintTable.Columns.Add("PRCN");    // "1"; Print Count
                inPrintTable.Columns.Add("MARH");    // "0"; Horizone pos
                inPrintTable.Columns.Add("MARV");    // "0"; Vertical pos
                inPrintTable.Columns.Add("DARK");    // darkness
                
                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LABELTYPE"] = ""; // TESLA INBOX : 사용안함. 
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INBOXID"] = sInBoxId;
                newRow["PRINT_QTY"] = numCnt.Value; //신규 라벨 발행수 1~100
                newRow["NEW_LABEL_FLAG"] = rdoNew.IsChecked == true ? "Y" : "N";
                newRow["INBOX_A_FLAG"]  = chkA.IsChecked == true ? "Y" : "N";
                newRow["INBOX_B_FLAG"]  = chkB.IsChecked == true ? "Y" : "N";
                newRow["INBOX_C_FLAG"]  = chkC.IsChecked == true ? "Y" : "N";
                newRow["INBOX_D_FLAG"]  = chkD.IsChecked == true ? "Y" : "N";


                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "Z"; Print type (Zebra)
                newRow["RESO"] = sRes; // "300"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_LABEL_FOR_TESLA_UI_MB", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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
            }
        }
        #endregion


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
