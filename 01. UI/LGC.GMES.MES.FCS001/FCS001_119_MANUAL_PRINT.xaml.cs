/*************************************************************************************
 Created Date : 2024.05.22
      Creator : Kwon Yongsub (cnsyongsub)
   Decription : Pallet ID 수동발행(2D)
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.22   권용섭    : [] Initialization Create (ESGM2 팔렛트 아이디 수동발행 화면 추가 요청)
**************************************************************************************/
#region Import Library
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
#endregion

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_119_MANUAL_PRINT : UserControl, IWorkArea
    {
        #region 00. Member Variable
        private string strLabelID = string.Empty;       // 2D Label ID <- LBL0294
        private string strLabelNM = string.Empty;       // 2D Label Name <- Ultium Cells Pallet
        private string strLabelProd = string.Empty;     // T30 : E101
        private string strCstProd = string.Empty;       // T30
        #endregion 00. Member Variable

        #region 01. All Window Event
        #region 01-01. Component Constructor
        /// <summary>
        /// Component Constructor
        /// </summary>
        public FCS001_119_MANUAL_PRINT()
        {
            InitializeComponent();
        }
        #endregion

        #region 01-02. Frame과 상호작용하기 위한 객체
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region 01-03. UserControl Loaded Event
        /// <summary>
        /// UserControl Loaded Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Loaded -= UserControl_Loaded;
                InitUserControl();                  // Initialization User Control
                InitSerialPrint();                  // Initialization Serial Print
                InitFactoryLabelID();               // Initialization Factory Label Code
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion 01. All Window Event
        
        #region 03. Window Button Event
        #region 03-01. Search Button Event
        /// <summary>
        /// Search Button Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion
        
        #region 03-03. Refresh Button Event
        /// <summary>
        /// Refresh Button Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            InitUserControl();
        }
        #endregion

        #region 03-04. Radio Button Event
        /// <summary>
        /// Radio Button Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoPublish_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.PerformClick();
        }
        #endregion
        #endregion 03. Window Button Event
        
        #region 95. ComboBox & TextBox & Spread Event
        #region 95-01. Pallet ID KeyDown Event
        /// <summary>
        /// Pallet ID KeyDown Event
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">KeyEventArgs</param>
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(this.txtPalletID.Text.Trim()))
                    {
                        btnSearch.PerformClick();
                    }
                }
                else
                {
                    var objRegex = new Regex(@"^[a-zA-Z0-9]+$");   // Only Alphanumeric
                    if (!objRegex.IsMatch(e.Key.ToString()))
                    {
                        e.Handled = true;
                    }
                    objRegex = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 95-02. Pallet ID PreviewKeyDown Event
        /// <summary>
        /// Pallet ID PreviewKeyDown Event
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">KeyEventArgs</param>
        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (!string.IsNullOrEmpty(this.txtPalletID.Text.Trim()))
                    {
                        btnSearch.PerformClick();
                    }
                }
                else if (e.Key == Key.Space || e.Key == Key.ImeProcessed)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 95-03. Pallet ID PreviewTextInput Event
        /// <summary>
        /// Pallet ID PreviewTextInput Event
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">TextCompositionEventArgs</param>
        private void txtPalletID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                var objRegex = new Regex(@"^[a-zA-Z0-9]+$");   // Only Alphanumeric
                if (!objRegex.IsMatch(e.Text))
                {
                    e.Handled = true;
                }
                objRegex = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 95-04. Pallet ID ClipboardPasted Event
        /// <summary>
        /// Pallet ID ClipboardPasted Event
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">DataObjectPastingEventArgs</param>
        /// <param name="text">string</param>
        private void txtPalletID_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            try
            {
                if (!String.IsNullOrEmpty(text))
                {
                    string strPastedText = text.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");
                    var objRegex = new Regex(@"^[a-zA-Z0-9]+$");   // Only Alphanumeric
                    if (!objRegex.IsMatch(strPastedText))
                    {
                        /// <summary> 숫자와 영문대문자만 입력가능합니다. </summary>
                        Util.MessageInfo("SFU3674", (rsltConfirm) =>
                        {
                            if (rsltConfirm == MessageBoxResult.OK)
                            {
                                InitUserControl();
                            }
                        });
                    }
                    objRegex = null;
                    this.txtPalletID.Text = strPastedText;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion 95. ComboBox & TextBox & Spread Event

        #region 99. User Defined Method
        #region 99-01. Show Loading Indicator
        /// <summary>
        /// Show Loading Indicator
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region 99-02. Hidden Loading Indicator
        /// <summary>
        /// Hidden Loading Indicator
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 99-03. Check for Reissue
        /// <summary>
        /// Check for Reissue
        /// </summary>
        private void GetList()
        {
            try
            {
                #region 99-03-01. Validation Condition
                /// <summary> 입력 또는 스캔된 팔렛트 아이디 확인 </summary>
                if (String.IsNullOrEmpty(Util.NVC(txtPalletID.Text))) 
                {
                    Util.MessageValidation("SFU8010");  //Pallet정보를 스캔하세요.
                    return;
                }
                this.txtScanPalletID.Text = Util.NVC(this.txtPalletID.Text);

                /// <summary> 라벨 발행 매수 확인 </summary>
                if (numPrintQty.Value < 1 || numPrintQty.Value > 4)
                {
                    Util.MessageValidation("SFU4085");  //발행 수량을 확인하세요.
                    return;
                }
                #endregion

                #region 99-03-02. Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                
                /// <summary> DataRow </summary>
                DataRow drIssue = dtRQSTDT.NewRow();
                drIssue["CSTID"] = txtScanPalletID.Text;
                dtRQSTDT.Rows.Add(drIssue);
                #endregion
                
                #region 99-03-03. Excute Data Access
                DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    #region 99-03-03-01. Defined DataTable and DataRow Variable
                    /// <summary> DataTable </summary>
                    DataTable dtRQSTDT2 = new DataTable();
                    dtRQSTDT2.TableName = "RQSTDT";
                    dtRQSTDT2.Columns.Add("LANGID", typeof(string));
                    dtRQSTDT2.Columns.Add("CSTID", typeof(string));

                    /// <summary> DataRow </summary>
                    DataRow drIssue2 = dtRQSTDT2.NewRow();
                    drIssue2["LANGID"] = LoginInfo.LANGID;
                    drIssue2["CSTID"] = txtScanPalletID.Text;
                    dtRQSTDT2.Rows.Add(drIssue2);
                    #endregion

                    #region 99-03-03-02. Excute Data Access
                    DataTable dtRSLTDT2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WITH_WIP", "RQSTDT", "RSLTDT", dtRQSTDT2);
                    if (CommonVerify.HasTableRow(dtRSLTDT2))
                    {
                        foreach (DataRow drRSLTDT2 in dtRSLTDT2.Rows)
                        {
                            strLabelProd = drRSLTDT2["CSTPROD_NAME"].ToString().Split(':')[1].Trim();
                            strCstProd = drRSLTDT2["CSTPROD"].ToString();
                            this.txtCstProdName.Text = drRSLTDT2["CSTPROD_NAME"].ToString();
                        }
                    }
                    else
                    {
                        /// <summary> Pallet 정보가 없습니다. </summary>
                        Util.MessageInfo("SFU4245", (rsltExist) =>
                        {
                            if (rsltExist == MessageBoxResult.OK)
                            {
                                InitUserControl();
                            }
                        });
                    }
                    #endregion

                    #region 99-03-03-03. Releases all resources used
                    drIssue2 = null;
                    dtRSLTDT2.Dispose();
                    dtRSLTDT2 = null;
                    dtRQSTDT2.Dispose();
                    dtRQSTDT2 = null;
                    #endregion
                }
                else
                {
                    /// <summary> 라벨[%1] 발행 이력이 존재하지 않습니다. </summary>
                    Util.MessageInfo("101241", (rsltConfirm) =>
                    {
                        if (rsltConfirm == MessageBoxResult.OK)
                        {
                            InitUserControl();
                        }
                    }, new string[] { txtPalletID.Text });
                }
                #endregion

                #region 99-03-99. Releases all resources used
                drIssue = null;
                dtRSLTDT.Dispose();
                dtRSLTDT = null;
                dtRQSTDT.Dispose();
                dtRQSTDT = null;
                #endregion

                #region 99-03-99. Excute Print Label
                if (!String.IsNullOrEmpty(strCstProd) && !String.IsNullOrEmpty(strLabelProd))
                {
                    ExecutePrint(Util.NVC(txtScanPalletID.Text), Util.NVC(txtCstProdName.Text), Convert.ToInt16(numPrintQty.Value));
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 99-04. Execute for Pallet Label
        /// <summary>
        /// Execute for Pallet Label
        /// </summary>
        private void ExecutePrint(string strPrintID, string strProdName, int intPrintQty = 1)
        {
            try
            {
                #region 99-04-01. Validation Condition
                /// <summary> 입력 또는 스캔된 팔렛트 아이디 확인 </summary>
                if (String.IsNullOrEmpty(strPrintID) || String.IsNullOrEmpty(strProdName))
                {
                    Util.MessageValidation("SFU8010");  //Pallet정보를 스캔하세요.
                    return;
                }

                /// <summary> 프린트 환경설정 정보 확인 </summary>
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU2003");  //프린트 환경 설정값이 없습니다.
                    return;
                }

                /// <summary> 해당 법인의 Pallet ID (2D) 정보 확인 </summary>
                if (String.IsNullOrEmpty(strLabelID))
                {
                    Util.MessageValidation("SFU8266");  //출력할 라벨이 존재하지 않습니다.
                    return;
                }

                /// <summary> 프린터 환경설정에 라벨정보 항목이 없습니다. </summary>
                // 2024.05.22 / cnsyongsub / 라벨정보 콤보박스 설정(DA_BAS_SEL_LABELINFO_CBO) 하드코딩으로 LBL0106, LBL0107 되어 있어서 제외함.
                //var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                //             where t.Field<string>("LABELID") == strLabelID
                //             select t).ToList();
                //if (!query.Any())
                //{
                //    Util.MessageValidation("SFU4339");  //프린터 환경설정에 라벨정보 항목이 없습니다.
                //    return;
                //}

                /// <summary> 프린터 환경설정에 라벨정보 항목이 없습니다. </summary>
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
                {
                    Util.MessageValidation("SFU4339");  //프린터 환경설정에 라벨정보 항목이 없습니다.
                    return;
                }
                #endregion

                #region 99-04-02. 2D Label Design Item Variable
                // 2024.05.22 : 운영 MMD 라벨 디자인 정보
                /// LABEL_CODE	DSGN_VER_NO	PRTR_MDL_ID	PRTR_RESOL_CODE	DSGN_CNTT
                /// LBL0294	3	Z	203	^XA  ^A0N,79,80^FO80,80^FDITEM001^FS  ^A0N,79,80^FO80,178^FDITEM002^FS  ^FO80,337^BQN,2,8^FH^FDLA,ITEM003^FS  ^FS  ^FO313,337^BQN,2,8^FH^FDLA,ITEM003^FS  ^FS  ^FO545,337^BQN,2,8^FH^FDLA,ITEM003^FS  ^FS  ^PQ1,0,1,Y  ^XZ
                /// LBL0294	3	Z	300	^XA  ^A0N,117,118^FO118,118^FDITEM001^FS  ^A0N,117,118^FO118,263^FDITEM002^FS  ^FO118,498^BQN,2,10^FH^FDLA,ITEM003^FS  ^FS  ^FO462,498^BQN,2,10^FH^FDLA,ITEM003^FS  ^FS  ^FO806,498^BQN,2,10^FH^FDLA,ITEM003^FS  ^FS  ^PQ1,0,1,Y  ^XZ
                const string strDBItem001 = "ITEM001";
                const string strDBItem002 = "ITEM002";
                const string strDBItem003 = "ITEM003";
                #endregion

                #region 99-04-03. Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                /// <summary> DataRow </summary>
                DataRow drIssue = dtRQSTDT.NewRow();
                drIssue["LABEL_CODE"] = strLabelID;
                dtRQSTDT.Rows.Add(drIssue);
                #endregion

                #region 99-04-04. Excute Data Access
                new ClientProxy().ExecuteService("DA_SEL_LABEL_PRINT_BY_TRAYID", "RQSTDT", "RSLTDT", dtRQSTDT, (result, bizException) =>
                {
                    #region 99-04-04-01. Exception Occurrence
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    #endregion

                    #region 99-04-04-02. 2D Label Design Information
                    if (CommonVerify.HasTableRow(result))
                    {
                        /// <summary> 2D Label Design Information exist </summary>
                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            /// <summary> 2D Label Design Information exist </summary>
                            if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()))
                            {
                                /// <summary> ZPL Text </summary>
                                var varZPLText = (from t in result.AsEnumerable()
                                                 where t.Field<string>("PRTR_RESOL_CODE") == dr["DPI"].GetString()
                                                    && t.Field<string>("PRTR_MDL_ID") == dr["PRINTERTYPE"].GetString()
                                                 select new { strZPLCode = t.Field<string>("DSGN_CNTT") }).FirstOrDefault();
                                if (varZPLText != null)
                                {
                                    string strZPLCode = string.Empty;
                                    strZPLCode = varZPLText.strZPLCode.Replace(strDBItem001, strPrintID).Replace(strDBItem002, strLabelProd).Replace(strDBItem003, strPrintID);

                                    for (int intQty = 0; intQty < intPrintQty; intQty++)
                                    {
                                        bool blnZPLPrint = dr["PORTNAME"].GetString().ToUpper().Equals("USB") ? FrameOperation.Barcode_ZPL_USB_Print(strZPLCode) : FrameOperation.Barcode_ZPL_Print(dr, strZPLCode);
                                        if (blnZPLPrint == false)
                                        {
                                            FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                            return;
                                        }
                                        System.Threading.Thread.Sleep(500);
                                    }

                                    // 발행이력 추가 - 성공한 프린트일 경우에만 이력을 저장한다.
                                    InsertLabelIssueHist(strPrintID, strProdName,  (rdoPublishNo.IsChecked.Equals(true) ? "I" : "R"), intPrintQty);
                                }
                            }

                            /// <summary> 라벨 발행을 완료하였습니다. </summary>
                            Util.MessageInfo("FM_ME_0126", (rsltConfirm) =>
                            {
                                if (rsltConfirm == MessageBoxResult.OK)
                                {
                                    InitUserControl();
                                }
                            });
                        }
                    }
                    else
                    {
                        /// <summary> 해당 제품에 대한 라벨 기준정보가 없습니다. </summary>
                        Util.MessageInfo("SFU4089", (rsltConfirm) =>
                        {
                            if (rsltConfirm == MessageBoxResult.OK)
                            {
                                InitUserControl();
                            }
                        });
                    }
                    #endregion 99-04-04-02. 2D Label Design Information
                }
                );
                #endregion 99-04-04. Excute Data Access

                #region 99-04-99. Releases all resources used
                drIssue = null;
                dtRQSTDT.Dispose();
                dtRQSTDT = null;
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 99-05. Initialization User Control
        /// <summary>
        /// Initialization User Control
        /// </summary>
        private void InitUserControl()            
        {
            try
            {
                // Initialization of Input/Scan Pallet Condition
                this.txtPalletID.Text = "";             // 입력 or 스캔 팔렛트 아이디
                this.numPrintQty.Value = 1;             // 발행 매수

                // Initialization of Issue Mode
                this.rdoPublishNo.IsChecked = false;    // 신규발행 UnChecked
                this.rdoPublishNo.IsEnabled = false;    // 신규발행 UnEnabled
                this.rdoPublishYes.IsChecked = true;    // 재발행 Checked
                this.rdoPublishYes.IsEnabled = true;    // 재발행 Enabled

                // Initialization of Target Condition
                this.txtScanPalletID.Text = "";         // 대상 팔렛트 아이디
                this.txtCstProdName.Text = "";          // 대상 제품명
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtPalletID.Focus();
            }
        }
        #endregion

        #region 99-06. Initialization Serial Print
        /// <summary>
        /// Initialization Serial Print
        /// </summary>
        private void InitSerialPrint()
        {
            try
            {
                /// <summary> 프린트 환경설정 정보 확인 </summary>
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU2003");  //프린트 환경 설정값이 없습니다.
                    return;
                }

                /// <summary> 프린터 환경설정에 라벨정보 항목이 없습니다. </summary>
                // 2024.05.22 / cnsyongsub / 라벨정보 콤보박스 설정(DA_BAS_SEL_LABELINFO_CBO) 하드코딩으로 LBL0106, LBL0107 되어 있어서 제외함.
                //var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                //             where t.Field<string>("LABELID") == strLabelID
                //             select t).ToList();
                //if (!query.Any())
                //{
                //    Util.MessageValidation("SFU4339");  //프린터 환경설정에 라벨정보 항목이 없습니다.
                //    return;
                //}

                /// <summary> 프린터 환경설정에 라벨정보 항목이 없습니다. </summary>
                if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
                {
                    Util.MessageValidation("SFU4339");  //프린터 환경설정에 라벨정보 항목이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 99-07. Initialization Factory Label Code
        /// <summary>
        /// Initialization Factory Label Code
        /// </summary>
        private void InitFactoryLabelID()
        {
            try
            {
                #region 99-07-01. Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("COM_CODE", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));
                
                /// <summary> DataRow </summary>
                DataRow drIssue = dtRQSTDT.NewRow();
                drIssue["LANGID"] = LoginInfo.LANGID;
                drIssue["AREAID"] = LoginInfo.CFG_AREA_ID;
                drIssue["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                drIssue["COM_CODE"] = "FORM_PLT_2D_LABEL_CODE";
                drIssue["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drIssue);
                #endregion

                #region 99-07-02. Excute Data Access
                DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    foreach (DataRow drRSLTDT in dtRSLTDT.Rows)
                    {
                        // 동별 공통코드는 존재하고, 속성1=팔렛트 2D 라벨코드 값이 정상적일때 설정
                        if (!string.IsNullOrEmpty(drRSLTDT["ATTR1"].ToString()) && !drRSLTDT["ATTR1"].Equals("NaN") && !drRSLTDT["ATTR1"].Equals(System.DBNull.Value))
                        {
                            strLabelID = drRSLTDT["ATTR1"].ToString();      // LBL0294
                        }
                        // 동별 공통코드는 존재하고, 속성2=팔렛트 2D 라벨명 값이 정상적일때 설정
                        if (!string.IsNullOrEmpty(drRSLTDT["ATTR2"].ToString()) && !drRSLTDT["ATTR2"].Equals("NaN") && !drRSLTDT["ATTR2"].Equals(System.DBNull.Value))
                        {
                            strLabelNM = drRSLTDT["ATTR2"].ToString();      // Ultium Cell Pallet
                        }
                    }
                }
                else
                {
                    /// <summary>
                    /// LABEL ID : Only 2D Barcode
                    /// LBL0189 = Tray ID (1D), LBL0290 = Tray ID (2D), LBL0294 = Pallet ID (2D), LBL0106 = 양품 태그 라벨, LBL0107 = 불량 태그 라벨
                    /// 해당 기능은 동별 공통코드 등록된 공장 [G671 (GM1_OH/AB), G673 (GM2_TN/AE), G675 (GM3_LS/AH)] 대상으로만 적용한다.
                    /// </summary>
                    switch (LoginInfo.CFG_AREA_ID.ToUpper().ToString())
                    {
                        case "AB":  // G671 (GM1_OH, AB)
                        case "AE":  // G673 (GM2_TN, AE)
                        case "AH":  // G675 (GM3_LS, AH)
                            strLabelID = "LBL0294";    // Ultium Cell Pallet
                            break;
                    }
                }
                #endregion

                #region 99-07-03. Releases all resources used
                drIssue = null;
                dtRSLTDT.Dispose();
                dtRSLTDT = null;
                dtRQSTDT.Dispose();
                dtRQSTDT = null;
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 99-08. Insert label issuance history
        /// <summary>
        /// Insert label issuance history
        /// </summary>
        private void InsertLabelIssueHist(string strPrintID, string strPrintProdName, string strIssueFlag, int intPrintQty = 1)
        {
            try
            {
                #region 99-08-01. Defined DataTable and DataRow Variable
                /// <summary> DataTable </summary>
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("CSTID", typeof(string));
                dtRQSTDT.Columns.Add("LABEL_CODE", typeof(string));
                dtRQSTDT.Columns.Add("PRT_QTY", typeof(int));
                dtRQSTDT.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));

                /// <summary> DataRow </summary>
                DataRow drIssue = dtRQSTDT.NewRow();
                drIssue["CSTID"] = strPrintID;
                drIssue["LABEL_CODE"] = strIssueFlag;
                drIssue["PRT_QTY"] = intPrintQty;
                drIssue["REMARKS_CNTT"] = String.Format("{0} / {1} / {2} ea",  strLabelID, strPrintProdName, intPrintQty.ToString());
                drIssue["USERID"] = LoginInfo.USERID;
                dtRQSTDT.Rows.Add(drIssue);
                #endregion

                #region 99-08-02. Excute Data Access
                DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_CST_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    // 인서트 성공시 처리가 필요한 경우
                }
                #endregion

                #region 99-08-03. Releases all resources used
                drIssue = null;
                dtRSLTDT.Dispose();
                dtRSLTDT = null;
                dtRQSTDT.Dispose();
                dtRQSTDT = null;
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion 99. User Defined Method
    }
}
