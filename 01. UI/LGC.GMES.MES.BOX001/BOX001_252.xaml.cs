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
  2023.07.07  이병윤    : E20230614-000843 오창 2공장 GMES Tesla 라벨 발행 기능 추가
  2023.11.17  오수현    : E20230901-001599 남경 소형조립 - TESLA Label print time fix로 달력 숨김. System date로 대체.  
  2024.03.12  오수현    : E20240227-000766 남경 소형조립 -  LOTCODE 아래 Check box 추가. LOTCODE 대문자로 변환(전 법인). PART NAME /MEASURE / WEIGHT UNIT - textbox 수정 불가.
                                            - Tesla Part Number 값은 Check Box 체크 시에는 LOTCODE Top 3-Bit 및 MMD 기반 Auto mapping화 하고 체크 해제시에는 수동 선택 가능.
                                            - Checkbox 미체크 시 부품 이름, 단위1, 단위2는 수정이 허용됨
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_252 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 


        string strZPL = string.Empty;
        string zpl = string.Empty;


        string siteCode = "";
        string strSupplierNo = "";
        string strSupplierName = "";

        public BOX001_252()
        {

            InitializeComponent();
            initCombo();
            initText();

            this.Loaded += BOX001_252_Loaded;
        }

        private void initText()
        {
            try
            {
                #region PART COUNTRY_OF_ORIGIN
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
                #endregion


                txtLotCode.CharacterCasing = CharacterCasing.Upper;

                #region Visibility
                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                {
                    txtPrintDate.Visibility = Visibility.Collapsed;
                    dtpPrintDate.Visibility = Visibility.Collapsed;

                    chkAuto.Visibility = Visibility.Visible;
                }
                else
                {
                    txtPrintDate.Visibility = Visibility.Visible;
                    dtpPrintDate.Visibility = Visibility.Visible;

                    chkAuto.Visibility = Visibility.Collapsed;
                }
                #endregion
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
                #region PART NUMBER Combo
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

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dataTable.Rows.Add(new string[] { dtResult.Rows[i]["ATTRIBUTE1"].ToString(), dtResult.Rows[i]["CMCODE"].ToString() });
                }

                cboTeslaPartNum.ItemsSource = dataTable.DefaultView;
                cboTeslaPartNum.DisplayMemberPath = "NAME";
                cboTeslaPartNum.SelectedValuePath = "VALUE";
                #endregion


                #region SUPPLIER Combo
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
                #endregion

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void BOX001_252_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= BOX001_252_Loaded;


            Initialize();
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

                if (int.Parse(txtTotalNum.Text) > maxPrtQty)
                {
                    Util.MessageValidation("PSS9157", maxPrtQty); // 프린트 수량은 %1을 초과할 수 없습니다.
                    return;
                }

                if (string.IsNullOrEmpty(cboSupplier.Text))
                {
                    Util.MessageValidation("SFU8481"); // SUPPLIER NAME을 선택하세요.
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

        private void btnLotCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lotCode = txtLotCode.Text.ToString();

                if (string.IsNullOrEmpty(lotCode))
                {
                    Util.MessageValidation("SFU8249"); // LotCode를 1자리 이상 입력해주세요.
                    return;
                }

                BOX001_252_LOT_SEARCH wndSearch = new BOX001_252_LOT_SEARCH(txtLotCode.Text.ToString());
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
            BOX001_252_LOT_SEARCH window = sender as BOX001_252_LOT_SEARCH;
            if (window.DialogResult != MessageBoxResult.Cancel)
            {
                txtLotCode.Text = window.lotCode;

                // E20240227-000766
                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182"))
                {
                    if (chkAuto.IsChecked.ToString() == "True")
                        SetPartNumber();
                }
            }
        }

        #endregion

        private DateTime GetDBDateTime()
        {
            try
            {
                DateTime tmpDttm = DateTime.Now;
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", null, "OUTDATA", null);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tmpDttm = (DateTime)dtRslt.Rows[0]["DATE"];
                }

                return tmpDttm;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return DateTime.Now;
            }
        }

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
                // dic.Add("CONTENT_LABEL_ID", labelID);
                dic.Add("QRcode1", Lot2D);
                //dic.Add("SUPPLIER", "          " + cboSupplier.SelectedValue.ToString());

                //////////if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                //////////{
                //////////    dic.Add("SUPPLIER", "          " + cboSupplier.SelectedValue.ToString());
                //////////}
                //////////else if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                //////////{
                dic.Add("SUPPLIER", "          " + strSupplierNo + "\r\n" + cboSupplier.Text.ToString());
                //////////}
                //dic.Add("PRINT_DATE", dtpPrintDate.SelectedDateTime.Year + "-" + dtpPrintDate.SelectedDateTime.Month + "-" + dtpPrintDate.SelectedDateTime.Day); 2022.06.20 날짜 표현 방식 변경

                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                {
                    DateTime dt = DateTime.Now;
                    DataTable dtResultGetDate = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_GETDATE", "", "RSLTDT", null);

                    if (dtResultGetDate.Rows.Count > 0)
                    {
                        string sDate = dtResultGetDate.Rows[0]["DATE_YYYYMMDD"].ToString();
                        dic.Add("PRINT_DATE", sDate.Substring(4, 2) + "/" + sDate.Substring(6, 2) + "/" + sDate.Substring(0, 4));
                    }
                }
                else
                {
                    dic.Add("PRINT_DATE", dtpPrintDate.SelectedDateTime.Month + "/" + dtpPrintDate.SelectedDateTime.Day + "/" + dtpPrintDate.SelectedDateTime.Year);

                }
                dic.Add("EXP_DATE", txtExpDate.Text.ToString());
                dic.Add("SERIAL_NUM", txtSerialNum.Text.ToString());
                dic.Add("LOT_CODE", txtLotCode.Text.ToString());
                dic.Add("PART_NUM", cboTeslaPartNum.SelectedValue.ToString());
                dic.Add("SITE_CODE", siteCode);

                int prtQTY = 0;

                if (!string.IsNullOrEmpty(txtTotalNum.Text.ToString()))
                {
                    prtQTY = Int32.Parse(txtTotalNum.Text.ToString());
                }

                BOX001_252_PRINT wndPrint = new BOX001_252_PRINT(dic, prtQTY);
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
        private void wndPrint_Closed(object sender, EventArgs e)
        {
            btnPrint.IsEnabled = true;
        }

        #region 부품번호 및 MMD 기반으로 한 Auto mapping
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                
                if (cboTeslaPartNum.SelectedIndex == -1) // E20240227-000766
                    return;

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

                txtPartName.Text    = dtResult.Rows[0]["CMCDNAME"].ToString(); // Part Number
                txtMeasure.Text     = dtResult.Rows[0]["ATTRIBUTE1"].ToString(); // Unit1
                txtWeightUnit.Text  = dtResult.Rows[0]["ATTRIBUTE2"].ToString(); // Unit2
                txtQuantity.Text    = dtResult.Rows[0]["ATTRIBUTE4"].ToString();
                txtGrossWeight.Text = dtResult.Rows[0]["ATTRIBUTE5"].ToString();

                //if (!LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립이 아닌경우
                //{
                //    siteCode = dtResult.Rows[0]["ATTRIBUTE3"].ToString(); //남경인경우 별도 아래에서 가져온다.
                //}

                //if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182")) //남경 소형조립
                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("A010")) //오창 소형조립
                {
                    siteCode = dtResult.Rows[0]["ATTRIBUTE3"].ToString();
                }
                else
                {
                    #region MMD TESLA SUPPLIER 정보 조회
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
                    #endregion 

                    #region SUPPLIER NAME 콤보 박스 구성
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
                    #endregion 

                }//남경 소형조립

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

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

        private void chkAuto_Checked(object sender, RoutedEventArgs e)
        {
            // E20240227-000766 콤보 활성화 여부
            if (chkAuto.IsChecked.ToString() == "True")
            {
                cboTeslaPartNum.IsEnabled = false;

                txtPartName.IsReadOnly    = true;
                txtMeasure.IsReadOnly     = true;
                txtWeightUnit.IsReadOnly  = true;

                SetPartNumber();
            }
            else
            {
                cboTeslaPartNum.IsEnabled = true;

                txtPartName.IsReadOnly    = false;
                txtMeasure.IsReadOnly     = false;
                txtWeightUnit.IsReadOnly  = false;
            }
        }

        private void txtLotCode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // E20240227-000766 - 남경 소형조립
            if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182"))
            {
                if (chkAuto.IsChecked.ToString() == "True")
                    SetPartNumber();
            }
        }

        #region E20240227-000766 Part Number auto mapping
        private void SetPartNumber()
        {
            try
            {
                if (txtLotCode.Text.Length < 3)
                    return;

                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstDt.Columns.Add("CMCODE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "TESLA_LOTCODE_NJ";
                drnewrow["CMCODE"] = Util.NVC(txtLotCode.Text.ToUpper().Substring(0, 3)); // LOT ID top 3-bit
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dtRqstDt);

                string mmdPartNumber = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    mmdPartNumber = dtResult.Rows[0]["ATTRIBUTE1"].ToString();

                    if (cboTeslaPartNum != null && cboTeslaPartNum.Items != null && cboTeslaPartNum.Items.Count > 0 && cboTeslaPartNum.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboTeslaPartNum.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("VALUE"))
                        {
                            bool bFnd = false;
                            // ComboBox 항목 수 만큼 돌면서 mmdPartNumber 변수와 같은 값을 가진 항목을 검색
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["VALUE"]).Equals(mmdPartNumber))
                                {
                                    cboTeslaPartNum.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboTeslaPartNum.Items.Count > 0)
                            {
                                InitAutoMapping();
                            }
                        }
                    }
                }
                else
                {
                    InitAutoMapping();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion


        private void InitAutoMapping()
        {
            cboTeslaPartNum.SelectedIndex = -1;

            txtPartName.Text = string.Empty;
            txtMeasure.Text = string.Empty;
            txtWeightUnit.Text = string.Empty;
            txtQuantity.Text = string.Empty;
            txtGrossWeight.Text = string.Empty;
        }
    }
}
