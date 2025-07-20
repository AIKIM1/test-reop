/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2020.03.03  이현호   C20200302-000124 H30A DMC 라벨 출력시 오기입 건 수정 (21700 Model 라벨출력 오류수정)
  2020.04.06  이현호   C20200214-000297 소형전지 물류 자동포장 설비 인박스 라벨 출력 형태 변경의 건 (TTI 라벨 INBOX 자동발행 기능 추가)
  2020.07.10  최상민   C20200710-000143 + 79832 + GMES DMC 라벨 발행 화면 개선 요청의 건
                       -> 조립 생산일 : 날짜 형식 오류 수정 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using System.Configuration;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_042 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        DataTable _dtItem = null; // inDataTable 담을 변수
        DataTable _dtItem2 = null; // inDataTable 담을 변수
        DataSet _dsResult = null; // biz Result DataSet Copy 담을 변수
        DataTable _dtPlt = null; // Pallet 정보
        bool _stopPrint = false;
        private readonly Dictionary<string, string> _pageNum1 = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _pageNum2 = new Dictionary<string, string>();

        string _sPGM_ID = "BOX001_042";

        #region Declaration & Constructor 



        public BOX001_042()
        {
            InitializeComponent();

            Initialize();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                this.btnInBoxAutoPrint.Visibility = Visibility.Collapsed;
            }
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                this.nbPrtQty_Box.IsReadOnly = true;
                this.btnInBoxPrint.Visibility = Visibility.Collapsed;
            }
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            Initialize();
            SetEvent();

            if (LoginInfo.CFG_SHOP_ID == "A010")
            {
                soShip.Text = "Ship No";
            }else
            {
                soShip.Text = "SO No";
            }
            
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            cboPageNum1.SelectedValueChanged -= cboPageNum1_SelectedValueChanged;
            cboPageNum2.SelectedValueChanged -= cboPageNum2_SelectedValueChanged;

            nbPrtQty_Pallet.Value = 1;
            InitializePageNumCombo();

            cboPageNum1.SelectedValueChanged += cboPageNum1_SelectedValueChanged;
            cboPageNum2.SelectedValueChanged += cboPageNum2_SelectedValueChanged;
            InitializeCombo();
            InitializeDataTable();
            GetSalesOrderInfo();
           // GetEqptWrkInfo();
        }

        private void InitializeCombo()
        {

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
        }
        #endregion

        #region Methods
        private void InitializeDataTable()
        {
            _dtItem = new DataTable("INITEM");
            _dtItem.Columns.Add("MNGT_ITEM_NAME");
            _dtItem.Columns.Add("MNGT_ITEM_VALUE");

            _dtItem2 = new DataTable("INITEM");
            _dtItem2.Columns.Add("MNGT_ITEM_NAME");
            _dtItem2.Columns.Add("MNGT_ITEM_VALUE");
            _dtPlt = new DataTable();
            _dtPlt.Columns.Add("Customer");
            _dtPlt.Columns.Add("Supplier");
            _dtPlt.Columns.Add("PONo");
            _dtPlt.Columns.Add("Model");
            _dtPlt.Columns.Add("Qty_Box");
            _dtPlt.Columns.Add("Qty_Cell");
            _dtPlt.Columns.Add("PartNo");
            _dtPlt.Columns.Add("Version");
            _dtPlt.Columns.Add("Pallet_No");
            _dtPlt.Columns.Add("Weight");
            _dtPlt.Columns.Add("Origin");
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
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
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
        private void GetInPalletList(string prodID =null)
        {
            try
            {
                if (string.IsNullOrEmpty(txtLotID.Text) || txtLotID.Text.Length < 8 )
                {
                    Util.MessageValidation("SFU4478");
                    return;
                }

                if (string.IsNullOrEmpty(txtPalletID.Text) || txtPalletID.Text.Length < 14)
                {
                    Util.MessageInfo("SFU5020"); // Pallet ID 정보가 없을 경우 In Box 라벨은 출력 할 수 없습니다.
                    btnInBoxPrint.IsEnabled = false;
                }
                else
                {
                    btnInBoxPrint.IsEnabled = true;
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("PKG_LOTID");
                inDataTable.Columns.Add("FROM_PACKDTTM");
                inDataTable.Columns.Add("TO_PACKDTTM");
                inDataTable.Columns.Add("CUSTOMERID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("INPALLETID");
                DataRow inDataRow = inDataTable.NewRow();

                inDataRow["AREAID"] = cboArea.SelectedValue;
                inDataRow["PKG_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                inDataRow["INPALLETID"] = string.IsNullOrEmpty(txtPalletID.Text) ? null : txtPalletID.Text;
                
                //inDataRow["FROM_PACKDTTM"] = Convert.ToDateTime(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"));
                //inDataRow["TO_PACKDTTM"] = Convert.ToDateTime(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59");

                inDataRow["FROM_PACKDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                inDataRow["TO_PACKDTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

                inDataRow["CUSTOMERID"] = "US001047";
                inDataRow["PRODID"] = prodID;
                inDataTable.Rows.Add(inDataRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xmltxt = ds.GetXml();
                new ClientProxy().ExecuteService("BR_PRD_GET_INPALLET_FOR_INBOX_FM", "INDATA", "OUTDATA", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    Util.GridSetData(dgInpallet, result, FrameOperation);
                    ClearPreview(null);
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private void GetSalesOrderInfo()
        {
            try
            {
                //string prvSalesOrder = string.Empty;
                //int idx = -1;
                //if (dgSalesOrder.ItemsSource != null && dgSalesOrder.Rows.Count > 0 && dgSalesOrder.SelectedIndex > -1)
                //{
                //    prvSalesOrder = dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO").ToString();
                //    idx = dgSalesOrder.SelectedIndex;
                //}                
                string sBizName = "";

                if (LoginInfo.CFG_SHOP_ID == "A010")
                {
                    sBizName = "DA_PRD_SEL_SHIPMENT_INFO";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_SO_INFO";
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHIPMENT_NO");
                RQSTDT.Columns.Add("PO_NO");
                RQSTDT.Columns.Add("SHOPID");
                RQSTDT.Columns.Add("CUSTOMERID");
                DataRow inDataRow = RQSTDT.NewRow();
                inDataRow["SHIPMENT_NO"] = txtShipNo.Text;
                inDataRow["PO_NO"] = null; //뭐 던져야하는지 확인
                inDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataRow["CUSTOMERID"] = "US001047";
                RQSTDT.Rows.Add(inDataRow);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_SHIPMENT_INFO", "RQSTDT", "RSLTDT", RQSTDT, (RSLTDT, ex) =>
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", RQSTDT, (RSLTDT, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }


                    Util.GridSetData(dgSalesOrder, RSLTDT, FrameOperation, false);
                    //if(idx>-1&& dgSalesOrder.ItemsSource != null && dgSalesOrder.Rows.Count > 0 )
                    //{
                    //    if (dgSalesOrder.Rows[idx].DataItem.GetValue("SHIPMENT_NO").ToString().Equals(prvSalesOrder))
                    //    {
                    //        dgSalesOrder.SelectedIndex = idx;
                    //        DataTableConverter.SetValue(dgSalesOrder.Rows[idx].DataItem, "CHK", true);
                    //    }
                    //}
                    //ClearPreview(null);
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private void SetCapaGrdCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_TTI_CAPA_GRD_OC";
            string[] arrColumn = { "PRODID" };
            string[] arrCondition = { prodID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }

        private void PrintBoxLabel(int prtQty, object sender)
        {
            if(string.IsNullOrEmpty(dgInpallet.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString()))
            {
                Util.MessageValidation("SFU4077"); //Model 정보가 없으므로 발행 불가합니다.
                return;
            }
            try
            {
                string shiptoID = "";
                _stopPrint = false;
                Button btn = sender as Button;
                string outBoxID = string.Empty;
                string lblCode = string.Empty;
                string zplCode = string.Empty;

                //PRINTER SETTING 변수
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;
                DataRow drPrtInfo = null;

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                string sBizRule = "BR_PRD_GET_LABEL_ZPL_FM";

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("LABEL_TYPE_CODE");
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("PRT_LABEL_TYPE_CODE");
                inDataTable.Columns.Add("PALLETID");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                _dtItem.Merge(_dtItem2);
                if(cbOutCapa.Visibility==Visibility.Visible)
                {
                    foreach (DataRow dr in _dtItem.AsEnumerable())
                    {
                        if(dr["MNGT_ITEM_NAME"].ToString().Equals("CAPA"))
                        {
                            dr["MNGT_ITEM_VALUE"] = cbOutCapa.Text.Split(':')[1].Trim();
                            break;
                        }
                    }
                }
                inDataSet.Tables.Add(_dtItem.Copy());

                if (LoginInfo.CFG_SHOP_ID.Equals("A010"))
                {
                    shiptoID = "M1009";
                }
                else
                {
                    shiptoID = "M3171";
                }
                DataRow inDataDr = inDataTable.NewRow();
                inDataDr["LANGID"] = LoginInfo.LANGID;
                inDataDr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataDr["LABEL_TYPE_CODE"] = btn.Name.Equals("btnOutBoxPrint") ? "OUTBOX" : "INBOX";
                inDataDr["PRODID"] = dgInpallet.SelectedItem.GetValue("PRODID").ToString(); //TODO: INPALLET PRODID로 변경
                inDataDr["LOTID"] = dgInpallet.SelectedItem.GetValue("PKG_LOTID").ToString() + "_" + dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO").ToString(); //라벨 발행 키값 - LOTID(포장실적)_SHIPPINGNOTE(S/O정보)
                inDataDr["SHIPTO_ID"] = btn.Name.Equals("btnOutBoxPrint") ? dgSalesOrder.SelectedItem.GetValue("SHIPTO_ID").ToString() : shiptoID;
                inDataDr["USERID"] = txtWorker.Tag;
                //C20180829_77493 추가
                inDataDr["PRT_LABEL_TYPE_CODE"] = btn.Name.Equals("btnOutBoxPrint") ? "" : "TTI_INBOX";
                inDataDr["PALLETID"] = dgInpallet.SelectedItem.GetValue("BOXID").ToString();
                inDataDr["PGM_ID"] = _sPGM_ID;
                inDataDr["BZRULE_ID"] = sBizRule;
                inDataTable.Rows.Add(inDataDr);

                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;//prtQty;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet ds = new DataSet();
                ds = inDataSet;
                string xmltxt = ds.GetXml();

                //DataSet RSLTDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_LABEL_ZPL_FM", "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet, null); // 동기호출로 변경
                DataSet RSLTDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet, null); // 동기호출로 변경

                if (RSLTDS != null && RSLTDS.Tables.Count > 0)
                {

                    if ((RSLTDS.Tables.IndexOf("OUTDATA") > -1) && RSLTDS.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        lblCode = RSLTDS.Tables["OUTDATA"].Rows[0]["LABEL_CODE"].GetString();
                        zplCode = RSLTDS.Tables["OUTDATA"].Rows[0]["ZPLCODE"].GetString();
                        if (zplCode.Split(',')[0].Equals("1"))
                        {
                            ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                        }
                    }
                    if(string.IsNullOrEmpty(lblCode)||string.IsNullOrEmpty(zplCode))
                    {
                        Util.MessageValidation("SFU4089"); //해당 제품에 대한 라벨 기준정보가 없습니다.
                        return;
                    }
                    for (int i = 0; i < prtQty; i++)
                    {
                        if (!_stopPrint)
                        {
                            PrintLabel(zplCode, drPrtInfo);
                            System.Threading.Thread.Sleep(500);
                            System.Windows.Forms.Application.DoEvents();
                        }
                    }
                    _stopPrint = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetPalletDataTable()
        {

            //validation
            try
            {
                _dtPlt.Clear();
                DataRow dr = _dtPlt.NewRow();
                dr["Customer"] = txtPltCustomer.Text;
                dr["Supplier"] = txtPltSupplier.Text;
                dr["PONo"] = tbPltPONo.Text;
                dr["Model"] = tbPltModel.Text;
                dr["Qty_Box"] = (int)txtPltBoxQty.Value;
                dr["Qty_Cell"] = (int)txtPltCellQty.Value;
                dr["PartNo"] = tbPltPartNo.Text;
                dr["Version"] = tbPltVer.Text;
                dr["Pallet_No"] = txtPltPalltNo.Text;
                dr["Weight"] = txtPltWeight.Text + "KG";
                dr["Origin"] = txtPltOrigin.Text;
                _dtPlt.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void SetPreview()
        {
            if (cbOutCapa.Items.Count > 1)
            {
                cbOutCapa.Visibility = Visibility.Visible;
                tbOutCapa.Visibility = Visibility.Collapsed;
            }
            else
            {
                cbOutCapa.Visibility = Visibility.Collapsed;
                tbOutCapa.Visibility = Visibility.Visible;
            }

            DataRowView drInPallet = (DataRowView)dgInpallet.SelectedItem;
            DataRowView drSalesOrder = (DataRowView)dgSalesOrder.SelectedItem;

            bool isNulldrInPallet = drInPallet == null ? true : false;
            bool isNulldrSalesOrder = drSalesOrder == null ? true : false;
            
            string capaGrdCode = (cbOutCapa.Visibility == Visibility.Visible)&&!(cbOutCapa.SelectedValue.Equals("SELECT")) ? cbOutCapa.Text.Split(':')[0].Trim() : string.Empty;
            
            if (!isNulldrInPallet && drInPallet.GetValue("CHK").SafeToBoolean() == true)
            {
                tbOutModel.Text = drInPallet.GetValue("PRINT_MODEL_NAME").ToString();
                tbOutGrade.Text = drInPallet.GetValue("PRDT_GRD_CODE").ToString();
                tbOutLine.Text = drInPallet.GetValue("LINE_NO").ToString();
                tbOutLot.Text = drInPallet.GetValue("LOTID_PRODDATE").ToString();
                tbOutDate.Text = drInPallet.GetValue("PRINTDATE").ToString();
                tbOutCapa.Text = string.Format("{0:G}", drInPallet.GetValue("CAPA").SafeToDouble()) + "mAh";
                tbOutVer.Text = tbPltVer.Text = drInPallet.GetValue("PACK_VER").ToString();
                bcBoxID.Text = "LG" + drInPallet["PROJECT"].ToString().Replace("18650", "").Replace("20650", "").Replace("21700", "").Replace(" ", "") + drInPallet["LOTID_PRODDATE"] + drInPallet["PRINTDATE"].ToString() + capaGrdCode + drInPallet["PRDT_GRD_CODE"];
                tbOutBcd.Text = "LG" + drInPallet["PROJECT"].ToString().Replace("18650", "").Replace("20650", "").Replace("21700", "").Replace(" ", "") + drInPallet["LOTID_PRODDATE"] + drInPallet["PRINTDATE"].ToString() + capaGrdCode + drInPallet["PRDT_GRD_CODE"];
                tbOutCapaCode.Text = capaGrdCode;
                _dtItem.Clear();
                for (int i = 0; i < drInPallet.Row.Table.Columns.Count; i++)
                {
                    DataRow dr = _dtItem.NewRow();
                    if (drInPallet.Row.Table.Columns[i].ColumnName.Equals("QUANTITY"))
                    {
                        dr["MNGT_ITEM_NAME"] = drInPallet.Row.Table.Columns[i].ColumnName;
                        dr["MNGT_ITEM_VALUE"] = ((int)tbOutQty.Value).ToString() + "pcs";
                        _dtItem.Rows.Add(dr);
                        continue;
                    }

                    if (drInPallet.Row.Table.Columns[i].ColumnName.Equals("CAPA"))
                    {
                        dr["MNGT_ITEM_NAME"] = drInPallet.Row.Table.Columns[i].ColumnName;
                        dr["MNGT_ITEM_VALUE"] = tbOutCapa.Text;
                        _dtItem.Rows.Add(dr);
                        continue;
                    }

                    if (drInPallet.Row.Table.Columns[i].ColumnName.Equals("BOXID_BCD")|| drInPallet.Row.Table.Columns[i].ColumnName.Equals("BOXID_TEXT"))
                    {
                        dr["MNGT_ITEM_NAME"] = drInPallet.Row.Table.Columns[i].ColumnName;
                        //string capaGrdCode = cbOutCapa.Visibility == Visibility.Visible ? cbOutCapa.Text.Split(':')[0].Trim() : string.Empty;
                        dr["MNGT_ITEM_VALUE"] = bcBoxID.Text;
                        _dtItem.Rows.Add(dr);
                        continue;
                    }

                    //if (drInPallet.Row.Table.Columns[i].ColumnName.Equals("CAPA"))
                    //{
                    //    dr["MNGT_ITEM_NAME"] = drInPallet.Row.Table.Columns[i].ColumnName;
                    //    dr["MNGT_ITEM_VALUE"] = tbOutCapa.Text;
                    //    _dtItem.Rows.Add(dr);
                    //    continue;
                    //}

                    dr["MNGT_ITEM_NAME"] = drInPallet.Row.Table.Columns[i].ColumnName;
                    dr["MNGT_ITEM_VALUE"] = drInPallet.GetValue(dr["MNGT_ITEM_NAME"].ToString()).ToString();
                    _dtItem.Rows.Add(dr);
                }
            }
            if (!isNulldrSalesOrder && drSalesOrder.GetValue("CHK").SafeToBoolean() == true)
            {
                tbOutPN.Text = tbPltPartNo.Text = drSalesOrder.GetValue("CUST_MTRLID").ToString();
                tbOutPONo.Text = tbPltPONo.Text = drSalesOrder.GetValue("PO_NO").ToString();
                tbPltModel.Text = drSalesOrder.GetValue("CUST_MTRLNAME").ToString();
                _dtItem2.Clear();

                for (int i = 0; i < drSalesOrder.Row.Table.Columns.Count; i++)
                {
                    DataRow dr = _dtItem2.NewRow();

                    if (drSalesOrder.Row.Table.Columns[i].ColumnName.Equals("PRINT_MODEL_NAME"))
                    {
                        continue;
                    }

                    dr["MNGT_ITEM_NAME"] = drSalesOrder.Row.Table.Columns[i].ColumnName;
                    dr["MNGT_ITEM_VALUE"] = drSalesOrder.GetValue(dr["MNGT_ITEM_NAME"].ToString()).ToString();
                    _dtItem2.Rows.Add(dr);
                }
            }
        }


        /// <summary>
        /// C20180829_77493 s/o 정보 변경 시 하위 오브젝트 초기화 처리
        /// </summary>
        private void setInPalletInfoClear()
        {
            Util.gridClear(dgInpallet);
            txtPalletID.Text = "";
            txtLotID.Text = "";
            nbPrtQty_Box.Value = 1;
            tbOutQty.Value = 0;
        }


        private void ClearPreview(object sender)
        {
            Button btn = sender as Button;
            if (sender == null)
            {
                if (dgInpallet.Rows.Count < 1 || dgInpallet.SelectedIndex < 0)
                {
                    cbOutCapa.Visibility = Visibility.Collapsed;
                    cbOutCapa.ItemsSource = null;
                    tbOutCapa.Visibility = Visibility.Visible;
                    tbOutModel.Text = tbOutGrade.Text = tbOutLine.Text = tbOutLot.Text = tbOutDate.Text = tbOutCapa.Text = tbOutVer.Text = tbPltVer.Text = string.Empty;
                }
                else if (dgSalesOrder.Rows.Count < 1 || dgSalesOrder.SelectedIndex < 0)
                {
                    tbOutPN.Text = tbOutPONo.Text = tbPltPartNo.Text = tbPltPONo.Text = tbPltModel.Text = string.Empty;
                }
            }
            else
            {
                if (btn.Name.Equals("btnSearch"))
                {
                    cbOutCapa.Visibility = Visibility.Collapsed;
                    cbOutCapa.ItemsSource = null;
                    tbOutCapa.Visibility = Visibility.Visible;
                    tbOutModel.Text = tbOutGrade.Text = tbOutLine.Text = tbOutLot.Text = tbOutDate.Text = tbOutCapa.Text = tbOutVer.Text = tbPltVer.Text = string.Empty;
                }
                else if (btn.Name.Equals("btnSearch_SO"))
                    tbOutPN.Text = tbOutPONo.Text = tbPltPartNo.Text = tbPltPONo.Text = tbPltModel.Text = string.Empty;
            }
        }
        #endregion

        #region Events
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //Validation
            string prodID = null;
            ClearPreview(sender);
            if (dgSalesOrder.Rows.Count>0 && dgSalesOrder.SelectedIndex>-1)
            {
                prodID = dgSalesOrder.SelectedItem.GetValue("PRODID").ToString();
            }
            GetInPalletList(prodID);
        }
        private void btnSearch_SO_Click(object sender, RoutedEventArgs e)
        {
            //Validation
            ClearPreview(sender);
            GetSalesOrderInfo();
            setInPalletInfoClear();
        }
        private void dgPrintListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                //if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                if (drv != null)
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    string prodID = dtRow["PRODID"]?.ToString();
                    SetCapaGrdCombo(cbOutCapa, CommonCombo.ComboStatus.SELECT, prodID);
                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgInpallet.SelectedIndex = idx;
                    SetPreview();
                    //GetSalesOrderInfo(prodID);
                    ClearPreview(null);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgInpallet_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            //dgInpallet.SelectedIndex = idx;
            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);


        }
        private void dgSalesOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearPreview(null);

                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                //if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                if (drv != null)
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    string prodID = dtRow["PRODID"]?.ToString();

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgSalesOrder.SelectedIndex = idx;
                    //GetInPalletList(prodID);
                    SetPreview();
                }
                //선택 시 inpallet 대상 리스트 초기화 처리
                setInPalletInfoClear();
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSalesOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            //dgInpallet.SelectedIndex = idx;
            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }
        private void btnOutBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            //validation
            if (dgInpallet.Rows.Count < 1 || dgInpallet.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU4090"); //실적 정보를 선택하세요.
                return;

            }
            if (dgInpallet.Rows.Count < 1 || dgSalesOrder.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU4091"); //S/O 정보를 선택하세요
                return;

            }
            if (string.IsNullOrEmpty(tbOutCapaCode.Text))
            {
                Util.MessageValidation("SFU4092"); //용량 등급을 선택하세요
                return;
            }

            if (string.IsNullOrEmpty(tbOutQty.Value.ToString()) || (int)tbOutQty.Value <= 0)
            {
                Util.MessageValidation("SFU1683"); //수량은 0 보다 커야 합니다.
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }
            if (!dgSalesOrder.SelectedItem.GetValue("PRODID").ToString().Equals(dgInpallet.SelectedItem.GetValue("PRODID").ToString()))
            {
                Util.MessageValidation("SFU4128");
                return;
            }
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4093",(int)nbPrtQty_Box.Value), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            { //Outbox라벨 " + (int)nbPrtQty_Box.Value + " 장 발행하시겠습니까?
                if (result == MessageBoxResult.OK)
                {
                    PrintBoxLabel((int)nbPrtQty_Box.Value, sender);
                    tbOutQty.Value = 0; //C20180424_70933 초기화 값을 200 -> 0으로 변경
                }
                
            });

        }
        private void btnInBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            //validation
            if (dgInpallet.Rows.Count < 1 || dgInpallet.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU4090"); // 실적 정보를 선택하세요.
                return;

            }
            if (string.IsNullOrEmpty(tbOutCapaCode.Text))
            {
                Util.MessageValidation("SFU4092"); //용량 등급을 선택하세요
                return;
            }
                      
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4084", (int)nbPrtQty_Box.Value), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {//Inbox라벨 " + (int)nbPrtQty_Box.Value + " 장 발행하시겠습니까?
                if (result == MessageBoxResult.OK)
                {
                    PrintBoxLabel((int)nbPrtQty_Box.Value, sender);
                }
            });
        }
        private void btnPrint_Plt_Click(object sender, RoutedEventArgs e)
        {

            if (dgSalesOrder.SelectedItem == null)
            {
                Util.MessageValidation("SFU4090"); //S/O 정보를 선택하세요
                return;
            }
            if (cboPageNum1.SelectedItem == null)
            {
                Util.MessageValidation("SFU4095"); //발행 페이지를 선택하세요
                return;
            }
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }
            if(cboPageNum1.SelectedIndex > cboPageNum2.SelectedIndex)
            {
                Util.MessageValidation("SFU4094"); //시작 페이지가 끝 페이지보다 클 수 없습니다.
                return;
            }
            SetPalletDataTable();
            BOX001_042_PALLET popUp = new BOX001_042_PALLET { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _dtPlt.Copy();
                Parameters[1] = nbPrtQty_Pallet.Value.ToString();
                Parameters[2] = cboPageNum1.SelectedValue?.ToString();
                Parameters[3] = cboPageNum2.SelectedValue?.ToString();

                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                /*
                 * 2018-09-05 오화백
                 * 작업자 정보 저장안하도록 수정
                 */
                txtShift.Text = Util.NVC(shiftPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(shiftPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(shiftPopup.USERNAME);
                txtWorker.Tag = Util.NVC(shiftPopup.USERID);


                //GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(shiftPopup);
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                //2018-09-05 오화백 작업자 저장 안되도록 수정
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }

        private void cbOutCapa_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetPreview();
        }

        private void tbOutQty_LostFocus(object sender, RoutedEventArgs e)
        {
            SetPreview();
        }

        private void InitializePageNumCombo()
        {
            _pageNum1.Clear();
            _pageNum2.Clear();

            cboPageNum1.SelectedValuePath = cboPageNum2.SelectedValuePath  = "Key";
            cboPageNum1.DisplayMemberPath = cboPageNum2.DisplayMemberPath = "Value";

            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                //_pageNum1.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum1.Add(i.ToString(), "Page " + i.ToString());
                }

                cboPageNum2.ItemsSource = null;
                cboPageNum2.ItemsSource = _pageNum1;
                cboPageNum2.SelectedIndex = _pageNum1.Count-1;

                cboPageNum1.ItemsSource = null;
                cboPageNum1.ItemsSource = _pageNum1;
                cboPageNum1.SelectedIndex = 0;

            }


        }

        private void cboPageNum1_IsDropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (cboPageNum1.IsDropDownOpen == false)
                return;

            if (cboPageNum1 == null)
                return;

            _pageNum1.Clear();
            cboPageNum1.SelectedValuePath = "Key";
            cboPageNum1.DisplayMemberPath = "Value";

            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                   // _pageNum.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum1.Add(i.ToString(), "Page " + i.ToString());
                }
               
                cboPageNum1.ItemsSource = null;
                cboPageNum1.ItemsSource = _pageNum1;
                cboPageNum1.SelectedIndex = 0;
            }
            else
                return;
        }
        private void cboPageNum2_IsDropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (cboPageNum2.IsDropDownOpen == false)
                return;

            if (cboPageNum2 == null)
                return;

            _pageNum2.Clear();
            cboPageNum2.SelectedValuePath = "Key";
            cboPageNum2.DisplayMemberPath = "Value";

            //int startPageNum = cboPageNum1.SelectedValue != null ? int.Parse(cboPageNum1.SelectedValue.ToString()) : 1;
            int pageNum = int.Parse(nbPrtQty_Pallet.Value.ToString());
            if (pageNum > 0)
            {

                // _pageNum.Add("ALL", "ALL");

                for (int i = 1; i <= pageNum; i++)
                {
                    _pageNum2.Add(i.ToString(), "Page " + i.ToString());
                }

                cboPageNum2.ItemsSource = null;
                cboPageNum2.ItemsSource = _pageNum2;
                //cboPageNum2.SelectedIndex = 0;
            }
            else
                return;
        }

        private void nbPrtQty_Pallet_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            InitializePageNumCombo();
            txtPltPalltNo.Text = cboPageNum1.SelectedValue.ToString() + "/" + cboPageNum2.SelectedValue.ToString();
        }



        #endregion

        private void cboPageNum1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int startPageNum = int.Parse(cboPageNum1.SelectedValue.ToString());
            int endPageNum = int.Parse(cboPageNum2.SelectedValue.ToString());

            txtPltPalltNo.Text = startPageNum.ToString() + "/" + nbPrtQty_Pallet.Value.ToString();
        }

        private void cboPageNum2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            int startPageNum = int.Parse(cboPageNum1.SelectedValue.ToString());
            int endPageNum = int.Parse(cboPageNum2.SelectedValue.ToString());
            if (startPageNum > endPageNum)
            {
                cboPageNum2.SelectedIndex = cboPageNum1.SelectedIndex;
                return;
            }
        }

        private void txtShipNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button btn = FindName("btnSearch") as Button;
                //Validation
                ClearPreview(btn);
                GetSalesOrderInfo();
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Validation
                Button btn = FindName("btnSearch_SO") as Button;

                string prodID = null;
                ClearPreview(btn);
                if (dgSalesOrder.Rows.Count > 0 && dgSalesOrder.SelectedIndex > -1)
                {
                    prodID = dgSalesOrder.SelectedItem.GetValue("PRODID").ToString();
                }
                //GetInPalletList(prodID);  //C20180829_77493 조회 기능 제거
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Validation
                Button btn = FindName("btnSearch_SO") as Button;

                string prodID = null;
                ClearPreview(btn);
                if (dgSalesOrder.Rows.Count > 0 && dgSalesOrder.SelectedIndex > -1)
                {
                    prodID = dgSalesOrder.SelectedItem.GetValue("PRODID").ToString();
                }
                //GetInPalletList(prodID); //C20180829_77493 조회 기능 제거
            }
        }

        private void btnCancelPrint_Click(object sender, RoutedEventArgs e)
        {
            _stopPrint = true;
            Util.MessageInfo("SFU1937"); //취소되었습니다.
            
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 TTI 라벨 팝업로직 화면으로 이동처리.
        private void btnInBoxAutoPrint_Click(object sender, RoutedEventArgs e)
        {
            DataSet indataSet = new DataSet();
            indataSet = null;
            //validation
            if (dgInpallet.Rows.Count < 1 || dgInpallet.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU4090"); // 실적 정보를 선택하세요.
                return;

            }
            if (string.IsNullOrEmpty(tbOutCapaCode.Text))
            {
                Util.MessageValidation("SFU4092"); //용량 등급을 선택하세요
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }

            Decimal Printcnt = Convert.ToDecimal(dgInpallet.SelectedItem.GetValue("TOTAL_CELL_QTY").ToString());

            indataSet = AutoPrintBoxLabel(sender);

            if (indataSet != null)
            {
                BOX001_048 popupAutoLabelAdd = new BOX001_048 { FrameOperation = FrameOperation };
                if (ValidationGridAdd(popupAutoLabelAdd.Name) == false)
                    return;

                object[] parameters = new object[3];
                parameters[0] = "TTI";
                parameters[1] = indataSet;
                parameters[2] = Printcnt;

                C1WindowExtension.SetParameters(popupAutoLabelAdd, parameters);

                Logger.Instance.WriteLine("Windows Popup", "Open");
                popupAutoLabelAdd.Closed += popupAutoLabelAdd_Closed;
                popupAutoLabelAdd.ShowModal();
                popupAutoLabelAdd.BringToFront();
            }
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 TTI 라벨 팝업로직 화면으로 이동처리.
        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 TTI 라벨 팝업로직 화면으로 이동처리.
        private void popupAutoLabelAdd_Closed(object sender, EventArgs e)
        {
            BOX001_048 popup = sender as BOX001_048;
            if (popup != null)
            {
                Logger.Instance.WriteLine("Popup Close", "Popup Close");
            }
            grdMain.Children.Remove(popup);
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 TTI 라벨 팝업로직 화면으로 이동처리.
        private DataSet AutoPrintBoxLabel(object sender)
        {
            DataSet indataSet = new DataSet();
            

            if (string.IsNullOrEmpty(dgInpallet.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString()))
            {
                Util.MessageValidation("SFU4077"); //Model 정보가 없으므로 발행 불가합니다.
                indataSet = null;
                return indataSet;
            }
            try
            {
                _stopPrint = false;
                Button btn = sender as Button;
                string outBoxID = string.Empty;
                string lblCode = string.Empty;
                string zplCode = string.Empty;

                //PRINTER SETTING 변수
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;
                DataRow drPrtInfo = null;

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    indataSet = null;
                    return indataSet;
                }

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("LABEL_TYPE_CODE");
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("PRT_LABEL_TYPE_CODE");
                inDataTable.Columns.Add("PALLETID");
                inDataTable.Columns.Add("PGM_ID");
                inDataTable.Columns.Add("BZRULE_ID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                _dtItem.Merge(_dtItem2);
                if (cbOutCapa.Visibility == Visibility.Visible)
                {
                    foreach (DataRow dr in _dtItem.AsEnumerable())
                    {
                        if (dr["MNGT_ITEM_NAME"].ToString().Equals("CAPA"))
                        {
                            dr["MNGT_ITEM_VALUE"] = cbOutCapa.Text.Split(':')[1].Trim();
                            break;
                        }
                    }
                }
                indataSet.Tables.Add(_dtItem.Copy());

                DataRow inDataDr = inDataTable.NewRow();
                inDataDr["LANGID"] = LoginInfo.LANGID;
                inDataDr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataDr["LABEL_TYPE_CODE"] = btn.Name.Equals("btnOutBoxPrint") ? "OUTBOX" : "INBOX";
                inDataDr["PRODID"] = dgInpallet.SelectedItem.GetValue("PRODID").ToString(); //TODO: INPALLET PRODID로 변경
                inDataDr["LOTID"] = dgInpallet.SelectedItem.GetValue("PKG_LOTID").ToString() + "_" + dgSalesOrder.SelectedItem.GetValue("SHIPMENT_NO").ToString(); //라벨 발행 키값 - LOTID(포장실적)_SHIPPINGNOTE(S/O정보)
                inDataDr["SHIPTO_ID"] = btn.Name.Equals("btnOutBoxPrint") ? dgSalesOrder.SelectedItem.GetValue("SHIPTO_ID").ToString() : "M1009";
                inDataDr["USERID"] = txtWorker.Tag;
                //C20180829_77493 추가
                inDataDr["PRT_LABEL_TYPE_CODE"] = btn.Name.Equals("btnOutBoxPrint") ? "" : "TTI_INBOX";
                inDataDr["PALLETID"] = dgInpallet.SelectedItem.GetValue("BOXID").ToString();
                inDataDr["PGM_ID"] = _sPGM_ID;
                inDataDr["BZRULE_ID"] = _sPGM_ID;

                inDataTable.Rows.Add(inDataDr);

                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;//prtQty;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                return indataSet;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                indataSet = null;
                return indataSet;
            }
        }
    }
    
}
