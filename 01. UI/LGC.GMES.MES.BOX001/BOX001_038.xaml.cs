/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2018.03.28  이상훈   C20180123_90089 아웃박스 라벨 발행시 SOC 는 선택값이 아니라 INPALLET TAG 에 반영된 시스템 정보를 가져오는것으로 변경 요청
  2019.03.14  이상훈   C20190131_13476 GMES Inbox Label 출력화면 개선 요청의 건 (소형전지/물류)
  2020.04.06  이현호   C20200214-000297 소형전지 물류 자동포장 설비 인박스 라벨 출력 형태 변경의 건 (HP/SMP 라벨 INBOX 자동발행 기능 추가)
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
using C1.WPF.DataGrid.Summaries;
using System.Configuration;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_038 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        DataTable _dtInPallet; //소멸시점?
        private string _inPltID = string.Empty;
        //private int _printQty = 1;
        // private int _adjustedCellQty = 0;
        private int _selIndex = 0; //선택된 Row
        private string _currentOutBox = string.Empty;
        private int _defaultCellQty = 0;

        // C20180829_77493 반복 조회 처리 기능 방지
        private string _sInpalletID = "";
        private string _sLotID = "";

        private bool isSearchButtonClicked = false;
        private bool isManualSelect = true; //발행 후 재조회에 의한 선택 : false, 사용자선택 :true

        private bool isResidaulCell = false;

        string _sPGM_ID = "BOX001_038";

        #region Declaration & Constructor 



        public BOX001_038()
        {
            InitializeComponent();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                this.btnAutoPrintAdd_Inbox.Visibility = Visibility.Collapsed;
                this.btnAutoPrint_Inbox.Visibility = Visibility.Collapsed;
            }
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                this.btnPrint_Inbox.Visibility = Visibility.Collapsed;
                this.btnPrintAdd_Inbox.Visibility = Visibility.Collapsed;
                this.inBoxPrintQty.IsReadOnly = true;
                this.txtCellQty.IsReadOnly = true;
            }
            SetResidualCellVisibility();

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
        }

        #endregion




        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            InitializeCombo();
            // txtPalletId.Text = "CAGD87623-001"; //테스트용
            //GetEqptWrkInfo();
            //   dtpDateFrom.SelectedDateTime = DateTime.Now;
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

            String[] sFilter1 = { "OWMS_BOX_TYPE_CODE" };
            _combo.SetCombo(cboShippingMethod, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboVender, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "SHIPTO_CP");
            //_combo.SetCombo(cboVender_Inbox, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "SHIPTO_CP");
            setVendor_InboxCombo();

            string[] sFilter3 = { LoginInfo.LANGID, "MOBILE_INBOX_ADD_LABEL_OC" };
            _combo.SetCombo(cboCustomer, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODES");
        }
        #endregion

        #region Methods
        private void GetInPalletList(bool isSearchButtonClicked)
        {
            //Validation 추가 - 작업자/조 선택
            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return;
            }

            try
            {
                ClearPalletInfo();
                _selIndex = dgPrintList.SelectedIndex; //dgPrintList 재조회전 선택되어있는 RowIndex
                DataSet inDataSet = new DataSet();

                DataTable RQSTDT = inDataSet.Tables.Add("INDATA");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("OWMS_BOX_TYPE_CODE");
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("USERID");

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BOXID"] = txtPalletId.Text;
                dr["OWMS_BOX_TYPE_CODE"] = Util.NVC(cboShippingMethod.SelectedValue);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;

                RQSTDT.Rows.Add(dr);

                _inPltID = txtPalletId.Text;
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_PROC_SHIP_PALLET_FM", "INDATA", "OUTDATA,OUTBOX", (RSLTDS, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    dgPrintList.ItemsSource = DataTableConverter.Convert(RSLTDS.Tables["OUTDATA"]);
                    dgPrintList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);

                    dgOutBoxList.ItemsSource = DataTableConverter.Convert(RSLTDS.Tables["OUTBOX"]);
                    dgOutBoxList.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Star);
                    txtTotalBoxQty.Text = "박스 수량 : " + dgOutBoxList.Rows.Count.ToString();




                    if (RSLTDS.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        _defaultCellQty = int.Parse(RSLTDS.Tables["OUTDATA"].Rows[0].GetValue("OUTBOX_CELL_QTY").GetString());
                        if (chkAdjustQty.IsChecked == false)
                        {
                            txtAdjustQty.Value = _defaultCellQty;
                        }
                        _dtInPallet = RSLTDS.Tables["OUTDATA"].Copy();
                        string prodID = Util.NVC(RSLTDS.Tables["OUTDATA"].Rows[0]["PRODID"]);
                        //SetPalletInfo();
                        if (isSearchButtonClicked)
                        {
                            setSOCCombo(cboSOC, CommonCombo.ComboStatus.SELECT, prodID);
                            if (!string.IsNullOrEmpty(RSLTDS.Tables["OUTDATA"].Rows[0].GetValue("SOC").ToString()))
                            {
                                cboSOC.Text = RSLTDS.Tables["OUTDATA"].Rows[0].GetValue("SOC").ToString();
                                //[C20180123_90089] 값이 세팅 될 경우 사용자 선택 할 수 없도록 개선
                                cboSOC.IsEnabled = false ;

                            }
                            else
                            {
                                //[C20180123_90089] 값이 세팅 될 경우 사용자 선택 할 수 없도록 개선
                                cboSOC.IsEnabled = true;
                            }

                            if (!RSLTDS.Tables["OUTDATA"].Rows[0].GetValue("PRODID").GetString().StartsWith("MCS"))
                            {
                                blckCustomer.Visibility = Visibility.Collapsed;
                                cboMcsCustomer.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                blckCustomer.Visibility = Visibility.Visible;
                                cboMcsCustomer.Visibility = Visibility.Visible;

                                CommonCombo _combo = new CommonCombo();
                                string[] sFilter4 = { LoginInfo.CFG_SHOP_ID, RSLTDS.Tables["OUTDATA"].Rows[0].GetValue("PRODID").GetString() };
                                _combo.SetCombo(cboMcsCustomer, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "CUSTID");
                            }

                            isManualSelect = true;
                        }
                        else
                        {

                            dgPrintList.SelectedIndex = _selIndex;
                            if (_selIndex > -1)
                                DataTableConverter.SetValue(dgPrintList.Rows[_selIndex].DataItem, "CHK", true);
                        }
                    }
                    else
                    {
                        Util.MessageValidation("100150", new string[] { txtPalletId.Text }); // 입력하신 Pallet ID[%1]는 존재하지 않는 Pallet ID 입니다. 확인 바랍니다.

                    }
                    if (dgPrintList.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["TOTAL_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["END_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["TOTAL_BOX_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["END_BOX_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["REST_BOX_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["REST_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPrintList.Columns["LAST_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }
                }, inDataSet);
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
        private void SetPalletInfoByOutboxID(string outboxID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("BOXID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["BOXID"] = outboxID;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                DataTable rsDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTBOX_LABEL_INFO_FM", "RQSTDT", "RSLTDT", dt);
                if (rsDt != null && rsDt.Rows.Count > 0)
                {

                    txtInfo01.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM12").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM01").ToString();
                    txtInfo02.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM05").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM02").ToString();
                    txtInfo03.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM02").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM03").ToString();
                    txtInfo04.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM03").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM04").ToString();
                    txtInfo05.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM04").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM05").ToString();
                    txtInfo06.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM01").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM06").ToString();

                    txtInfo11.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM08").ToString();
                    txtInfo12.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM06").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM09").ToString();
                    txtInfo13.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM07").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM10").ToString();
                    txtInfo14.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM08").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM11").ToString();
                    txtInfo15.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM12").ToString();
                    txtInfo16.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM13").ToString();

                    txtInfo21.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM14").ToString();
                    txtInfo22.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM16").ToString();
                    txtInfo23.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM18").ToString();
                    txtInfo24.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM20").ToString();
                    txtInfo25.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM22").ToString();
                    txtInfo26.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM24").ToString();

                    txtInfo31.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM15").ToString();
                    txtInfo32.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM17").ToString();
                    txtInfo33.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM19").ToString();
                    txtInfo34.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM21").ToString();
                    txtInfo35.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM23").ToString();
                    txtInfo36.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? "" : rsDt.Rows[0].GetValue("PRT_ITEM25").ToString();

                    bcBoxID.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM09").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM26").ToString();
                    txtInfoBoxID.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString().StartsWith("MCS") ? rsDt.Rows[0].GetValue("PRT_ITEM10").ToString() : rsDt.Rows[0].GetValue("PRT_ITEM27").ToString();
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private bool CreateOutBox(int vQty, string sInBox1 = null, string sInBox2 = null)
        {
            try
            {
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
                    return false;

                string sBizRule = "BR_PRD_REG_OUTBOX_FM";

                DataSet inDataSet = new DataSet();

                DataTable inPalletTable = inDataSet.Tables.Add("INPALLET");
                inPalletTable.Columns.Add("AREAID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("PRODID");
                inPalletTable.Columns.Add("LOTID");
                inPalletTable.Columns.Add("PRDT_GRD_CODE");
                inPalletTable.Columns.Add("SHIPTO_ID");
                inPalletTable.Columns.Add("SOC");
                inPalletTable.Columns.Add("USERID");
                inPalletTable.Columns.Add("EQPTID");
                inPalletTable.Columns.Add("OWMS_BOX_TYPE_CODE");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("ACTUSER");
                inPalletTable.Columns.Add("CUSTOMER_ID");
                inPalletTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inPalletTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inOutBoxTable = inDataSet.Tables.Add("INOUTBOX");
                inOutBoxTable.Columns.Add("PROJECT");
                inOutBoxTable.Columns.Add("OUTBOX_CELL_QTY");
                inOutBoxTable.Columns.Add("BOX_WEIGHT");
                inOutBoxTable.Columns.Add("CAPA");
                inOutBoxTable.Columns.Add("VLTG_VALUE");
                inOutBoxTable.Columns.Add("REMARK");
                inOutBoxTable.Columns.Add("REWORK_CODE");
                inOutBoxTable.Columns.Add("MODEL_NAME");
                inOutBoxTable.Columns.Add("INBOXID1");
                inOutBoxTable.Columns.Add("INBOXID2");

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow inPalletDr = inPalletTable.NewRow();
                inPalletDr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inPalletDr["BOXID"] = dgPrintList.SelectedItem.GetValue("BOXID");
                inPalletDr["PRODID"] = dgPrintList.SelectedItem.GetValue("PRODID");
                inPalletDr["LOTID"] = dgPrintList.SelectedItem.GetValue("LOTID");
                inPalletDr["PRDT_GRD_CODE"] = dgPrintList.SelectedItem.GetValue("PRDT_GRD_CODE");
                inPalletDr["SHIPTO_ID"] = Util.NVC(cboVender.SelectedValue);
                inPalletDr["SOC"] = Util.NVC(cboSOC.Text);

                inPalletDr["USERID"] = LoginInfo.USERID;
                inPalletDr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                inPalletDr["OWMS_BOX_TYPE_CODE"] = Util.NVC(cboShippingMethod.SelectedValue);
                inPalletDr["LANGID"] = LoginInfo.LANGID;
                inPalletDr["ACTUSER"] = txtWorker.Tag;
                inPalletDr["CUSTOMER_ID"] = Util.NVC(cboMcsCustomer.SelectedValue);
                inPalletDr["PGM_ID"] = _sPGM_ID;
                inPalletDr["BZRULE_ID"] = sBizRule;

                inPalletTable.Rows.Add(inPalletDr);

                DataRow inOutBoxDr = inOutBoxTable.NewRow();
                inOutBoxDr["PROJECT"] = dgPrintList.SelectedItem.GetValue("PROJECT");
                inOutBoxDr["OUTBOX_CELL_QTY"] = vQty == 0 ? (dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY")) : vQty;
                inOutBoxDr["BOX_WEIGHT"] = vQty == 0 ? string.Format("{0:N2}", dgPrintList.SelectedItem.GetValue("BOX_WEIGHT")) : Convert.ToString((vQty * dgPrintList.SelectedItem.GetValue("CELL_NET_WEIGHT").SafeToDouble()));
                inOutBoxDr["CAPA"] = string.Format("{0:N0}", dgPrintList.SelectedItem.GetValue("CAPA"));
                inOutBoxDr["VLTG_VALUE"] = string.Format("{0:N2}", decimal.Parse(cboSOC.SelectedValue.ToString()));
                inOutBoxDr["REMARK"] = txtWorker.Tag;
                inOutBoxDr["REWORK_CODE"] = dgPrintList.SelectedItem.GetValue("JOB_COUNT");
                inOutBoxDr["MODEL_NAME"] = dgPrintList.SelectedItem.GetValue("MODEL_NAME");
                if (!string.IsNullOrEmpty(sInBox1))
                {
                    inOutBoxDr["INBOXID1"] = sInBox1;
                }
                if (!string.IsNullOrEmpty(sInBox2))
                {
                    inOutBoxDr["INBOXID2"] = sInBox2;
                }

                inOutBoxTable.Rows.Add(inOutBoxDr);

                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet ds = new DataSet();
                ds = inDataSet;
                string xmltxt = ds.GetXml();

                // GetInPalletList();
                //DataSet RSLTDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTBOX_FM", "INPALLET,INOUTBOX,INPRINT", "OUTBOX,OUTZPL", inDataSet, null); // 동기호출로 변경
                DataSet RSLTDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INPALLET,INOUTBOX,INPRINT", "OUTBOX,OUTZPL", inDataSet, null); // 동기호출로 변경

                if (RSLTDS != null && RSLTDS.Tables.Count > 0)
                {
                    if ((RSLTDS.Tables.IndexOf("OUTBOX") > -1) && RSLTDS.Tables["OUTBOX"].Rows.Count > 0)
                    {
                        outBoxID = RSLTDS.Tables["OUTBOX"].Rows[0]["BOXID"].GetString();
                    }
                    if ((RSLTDS.Tables.IndexOf("OUTZPL") > -1) && RSLTDS.Tables["OUTZPL"].Rows.Count > 0)
                    {
                        lblCode = RSLTDS.Tables["OUTZPL"].Rows[0]["LABEL_CODE"].GetString();
                        zplCode = RSLTDS.Tables["OUTZPL"].Rows[0]["ZPLCODE"].GetString();

                        if (zplCode.Split(',')[0].Equals("1"))
                        {
                            ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            return false;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                        }
                        if (sPrt.Equals("D")) // 20171123 : 소형1동 Datamax 프린터에서 ZPL 인쇄안되서 DPL로 인쇄할 수 있도록 수정
                        {
                            zplCode = zplCode.Insert(zplCode.IndexOf('q'), "");
                            zplCode = zplCode.Insert(zplCode.IndexOf('L'), "");
                            zplCode = (char)0x02 + zplCode + (char)0x03;
                        }
                    }
                }
                _currentOutBox = RSLTDS.Tables["OUTBOX"].Rows[0]["BOXID"].ToString();
                isManualSelect = false;
                btnSearch_Click(null, null);
                PrintLabel(zplCode, drPrtInfo);
                SetPalletInfoByOutboxID(_currentOutBox);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetInboxScanPopUp()
        {
            BOX001_038_RESIDUAL_CELL popupResidual = new BOX001_038_RESIDUAL_CELL { FrameOperation = FrameOperation };

            popupResidual.FrameOperation = this.FrameOperation;

            object[] parameters = new object[2];

            parameters[0] = dgPrintList.SelectedItem.GetValue("BOXID").ToString();
            parameters[1] = dgPrintList.SelectedItem.GetValue("REST_CELL_QTY").ToString();

            C1WindowExtension.SetParameters(popupResidual, parameters);
            popupResidual.Closed += new EventHandler(popupResidual_Closed);

            grdMain.Children.Add(popupResidual);
            popupResidual.BringToFront();
        }

        private void popupResidual_Closed(object sender, EventArgs e)
        {
            try
            {
                BOX001_038_RESIDUAL_CELL popup = sender as BOX001_038_RESIDUAL_CELL;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    CreateOutBox(popup.vQty, popup.inbox1, popup.inbox2);
                    //InboxMapping(popup.inbox1, popup.inbox2);
                }
                grdMain.Children.Remove(popup);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void InboxMapping(string sBox1, string sBox2)
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "INDATA";
                dtRqstDt.Columns.Add("INBOXID1", typeof(string));
                dtRqstDt.Columns.Add("INBOXID2", typeof(string));
                dtRqstDt.Columns.Add("OUTBOXID", typeof(string));
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = dtRqstDt.NewRow();
                inDataRow["INBOXID1"] = sBox1;
                inDataRow["INBOXID2"] = sBox2;
                inDataRow["OUTBOXID"] = _currentOutBox;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["USERID"] = LoginInfo.USERID;

                dtRqstDt.Rows.Add(inDataRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_MAPPING_INBOX_OUTBOX", "RQSTDT", "RSLTDT", dtRqstDt);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void SetResidualCellVisibility()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "INBOX_SCAN_FOR_CELL_TRACE";
                dr["CMCODE"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    btnPrintAll.Visibility = Visibility.Collapsed;
                    btnMergeSplit.Visibility = Visibility.Collapsed;
                    isResidaulCell = true;
                    return;
                }            

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// [C20180123_90089] 발행하려는 inbox의 SOC 값이 Pallet 구성 중인 SOC 값과 동일한지 비교
        /// </summary>
        /// <returns></returns>
        private Boolean GetInBoxSOCvalueChk()
        {

            if (cboSOC.SelectedValue == null || cboSOC.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4071"); //SOC를 선택하세요. 
                return false;
            }

            string sSOC = Util.NVC(cboSOC.Text);

            foreach (C1.WPF.DataGrid.DataGridRow row in dgOutBoxList.Rows)
            {
                if (!sSOC.Equals(DataTableConverter.GetValue(row.DataItem, "SOC_VALUE").ToString ()))
                {
                    return false;
                }
            }
            return true;
        }

        private void CreatePallet()
        {

            try
            {
                if(dgOutBoxList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2058");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;
                btnAddPallet.IsEnabled = false;

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("ACTUSER");

                DataTable inOutBoxTable = inDataSet.Tables.Add("INOUTBOX");
                inOutBoxTable.Columns.Add("BOXID");
                inOutBoxTable.Columns.Add("BOXSEQ");

                DataRow inDatadr = inDataTable.NewRow();
                inDatadr["USERID"] = LoginInfo.USERID;
                inDatadr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDatadr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDatadr["ACTUSER"] = txtWorker.Tag;
                inDataTable.Rows.Add(inDatadr);

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOutBoxList.Rows)
                {
                    DataRow inOutBoxdr = inOutBoxTable.NewRow();
                    inOutBoxdr["BOXID"] = DataTableConverter.GetValue(row.DataItem, "BOXID");
                    inOutBoxdr["BOXSEQ"] = DataTableConverter.GetValue(row.DataItem, "BOXSEQ");
                    inOutBoxTable.Rows.Add(inOutBoxdr);
                }

                //DataSet ds = new DataSet();
                //ds = inDataSet;
                // string xmltxt = ds.GetXml();

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PACKING_OUTBOX_FM", "INDATA,INOUTBOX", "OUTPALLET", (RSLTDS, ex) =>
                //{
                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //    if ((RSLTDS.Tables.IndexOf("OUTPALLET") > -1) && RSLTDS.Tables["OUTPALLET"].Rows.Count > 0)
                //    {
                //        string outPallet = RSLTDS.Tables["OUTPALLET"].Rows[0]["BOXID"].GetString();
                //        btnSearch_Click(null, null);
                //        Util.MessageValidation("SFU4073", outPallet);
                //    }
                //}, inDataSet);

                string outPallet = string.Empty;
                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACKING_OUTBOX_FM", "INDATA,INOUTBOX", "OUTPALLET", inDataSet);

                if ((resultDs.Tables.IndexOf("OUTPALLET") > -1) && resultDs.Tables["OUTPALLET"].Rows.Count > 0)
                {
                    outPallet = resultDs.Tables["OUTPALLET"].Rows[0]["BOXID"].GetString();
                    btnSearch_Click(null, null);
                    //System.Windows.MessageBox.Show("팔레트 구성이 완료되었습니다. Pallet ID :" + outPallet);
                    Util.MessageValidation("SFU4073", outPallet);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnAddPallet.IsEnabled = true;
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
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
        private void setSOCCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string prodID)
        {
            const string bizRuleName = "DA_BAS_SEL_PRDT_SOC_VLTG_COMBO";
            string[] arrColumn = { "SHOPID", "PRODID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }

        private void setVendor_InboxCombo()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add(cboVender_Inbox.SelectedValuePath);
            dt.Columns.Add(cboVender_Inbox.DisplayMemberPath);

            // SBD Vendor
            DataRow dr = dt.NewRow();
            dr[cboVender_Inbox.SelectedValuePath] = "SELECT";
            dr[cboVender_Inbox.DisplayMemberPath] = "-SELECT-";
            dt.Rows.Add(dr);

            //dr = dt.NewRow();
            //dr[cboVender_Inbox.SelectedValuePath] = "315209";
            //dr[cboVender_Inbox.DisplayMemberPath] = "STANLEY BLACK & DECKER, INC";
            //dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[cboVender_Inbox.SelectedValuePath] = "CN003256";
            dr[cboVender_Inbox.DisplayMemberPath] = "STANLEY BLACK&DECKER(SHENZHEN)";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[cboVender_Inbox.SelectedValuePath] = "MO000007";
            dr[cboVender_Inbox.DisplayMemberPath] = "BLACK&DECKER MACAO";
            dt.Rows.Add(dr);

            cboVender_Inbox.ItemsSource = dt.Copy().AsDataView(); ;
            cboVender_Inbox.SelectedIndex = 0;

        }

        private bool CanCreateOutBox()
        {
            if (dgPrintList.SelectedItem == null)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return false;
            }
            if (string.IsNullOrEmpty(dgPrintList.SelectedItem.GetValue("MODEL_NAME").ToString()))
            {
                Util.MessageValidation("SFU4077"); //Model 정보가 없으므로 발행 불가합니다.
                return false;
            }
            if (cboSOC.SelectedValue.ToString().Equals("SELECT"))
            {
                // TODO: 메세지처리
                Util.MessageValidation("SFU4071"); //SOC를 선택하세요

                return false;
            }

            if (cboMcsCustomer.Visibility == Visibility.Visible)
            {
                if (cboMcsCustomer.SelectedValue.ToString().Equals("SELECT"))
                {
                    // TODO: 메세지처리
                    Util.MessageValidation("SFU8264"); //고객사 ID를 선택해주세요

                    return false;
                }
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return false;
            }

            //C20180123_90089 출력 대상관 pallet 대상 SOC 값 비교 
            if (dgOutBoxList.Rows.Count > 0)
            {
                string sSOC = dgOutBoxList.Rows[0].DataItem.GetValue("SOC_VALUE").ToString();
                if (!cboSOC.Text.Equals(sSOC))
                {
                    Util.MessageValidation("SFU4059");// 동일한 SOC 값이 아닙니다.
                    return false;
                }
            }

            //if(수량마이너스)
            // return false;
            return true;
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
                                //  SetPalletInfo();
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
        private void SetPalletInfo()
        {
            if (dgPrintList.SelectedItem == null)
                return;
            try
            {
                txtInfo01.Text = dgPrintList.SelectedItem.GetValue("MODEL_NAME").ToString();
                txtInfo02.Text = dgPrintList.SelectedItem.GetValue("PRDT_GRD_CODE").ToString();
                txtInfo03.Text = chkAdjustQty.IsChecked == true ? txtAdjustQty.Value.ToString() : dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY").ToString();
                txtInfo04.Text = chkAdjustQty.IsChecked == true ? Convert.ToString((txtAdjustQty.Value * dgPrintList.SelectedItem.GetValue("CELL_NET_WEIGHT").SafeToDouble())) + "Kg" : string.Format("{0:N2}", dgPrintList.SelectedItem.GetValue("BOX_NET_WEIGHT")) + "Kg";
                txtInfo05.Text = dgPrintList.SelectedItem.GetValue("CAPA").ToString() + "mAh";
                txtInfo06.Text = dgPrintList.SelectedItem.GetValue("PRODID").ToString();

                txtInfo11.Text = cboSOC.Text.Equals("-SELECT-") ? "" : cboSOC.Text + "%";
                txtInfo12.Text = dgPrintList.SelectedItem.GetValue("LOTID").ToString().Substring(4, 3);
                txtInfo13.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtInfo14.Text = cboSOC.SelectedValue.ToString().Equals("SELECT") ? "" : string.Format("{0:N2}", decimal.Parse(cboSOC.SelectedValue.ToString())) + "V";
                txtInfo15.Text = dgPrintList.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString();
                txtInfo16.Text = dgPrintList.SelectedItem.GetValue("PRODWEEK").ToString();


                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("CUSTOMERID");
                dt.Columns.Add("MODLID");
                dt.Columns.Add("GRD_TYPE_CODE");
                dt.Columns.Add("USE_FLAG");
                dt.Columns.Add("SHOPID");

                DataRow dr = dt.NewRow();
                dr["CUSTOMERID"] = null;
                dr["MODLID"] = dgPrintList.SelectedItem.GetValue("PRODID").ToString();
                dr["GRD_TYPE_CODE"] = "A";
                dr["USE_FLAG"] = "Y";
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dt.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                string xml = ds.GetXml();

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHOP_GRD_CUST_PRDT", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        TextBlock tb = FindName("txtInfo2" + (i + 1).ToString()) as TextBlock;
                        tb.Text = result.Rows[i]["CUSTOMERID"].ToString() + " P/N:";
                    }
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        TextBlock tb = FindName("txtInfo3" + (i + 1).ToString()) as TextBlock;
                        tb.Text = result.Rows[i]["CUSTPRODID"].ToString();
                    }

                }
                // bcBoxID.Text = _currentOutBox;
                //txtInfoBoxID.Text = _currentOutBox;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ClearPalletInfo()
        {
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    TextBlock tb = FindName("txtInfo0" + (i + 1).ToString()) as TextBlock;
                    tb.Text = string.Empty;

                    TextBlock tb2 = FindName("txtInfo1" + (i + 1).ToString()) as TextBlock;
                    tb2.Text = string.Empty;

                    TextBlock tb3 = FindName("txtInfo2" + (i + 1).ToString()) as TextBlock;
                    tb3.Text = string.Empty;

                    TextBlock tb4 = FindName("txtInfo3" + (i + 1).ToString()) as TextBlock;
                    tb4.Text = string.Empty;
                }
                txtInfoBoxID.Text = string.Empty;
                bcBoxID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetCustProdId(string prodId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CUSTOMERID");
            dt.Columns.Add("MODLID");
            dt.Columns.Add("SHOPID");

            DataRow dr = dt.NewRow();
            dr["CUSTOMERID"] = cboVender_Inbox.SelectedValue.ToString();
            dr["MODLID"] = prodId;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

            dt.Rows.Add(dr);

            DataTable rsDt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHOP_GRD_CUST_PRDT", "RQSTDT", "RSLTDT", dt);
            if (rsDt != null && rsDt.Rows.Count > 0)
            {
                return rsDt.Rows[0]["CUSTPRODID"].ToString();
            }

            return string.Empty;
        }

        private DataTable getCustProdInfo()
        {
            try
            {
                // 고객사 등급별 제품코드 기준정보 테이블 전환
                // TB_MMD_GRD_CUST_PRDT => TB_MMD_SHOP_GRD_CUST_PRDT 테이블 변경으로
                // SHOPID PK 추가되었기 때문에 Input 파라미터 추가
                // 2018.04.26 yhcha 
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("CUSTOMERID");
                dt.Columns.Add("PRODID");
                dt.Columns.Add("SOC");
                dt.Columns.Add("SHOPID");
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CUSTOMERID"] = "";
                dr["PRODID"] = dgInbox.SelectedItem.GetValue("PRODID").ToString();
                dr["SOC"] = cboSOC.Text;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dt.Rows.Add(dr);

                DataSet rsDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_CUST_PRODID_FOR_SBD_FM", "INDATA", "OUTDATA", ds);

                //2020-02-03 최상민 수정 Tables[1] -> Tables["OUTDATA"]
                if (rsDs != null && rsDs.Tables.Count > 0 && rsDs.Tables["OUTDATA"] != null && rsDs.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable resultDt = rsDs.Tables["OUTDATA"].Copy();
                    return resultDt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
                return null;
            }
        }
        private void PrintInbox(int prtQty)
        {
            string zplCode = string.Empty;
            string lblCode = string.Empty;

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;
            DataTable dtCustProd = new DataTable();
            //if (getCustProdInfo() != null)
            //{
            //    dtCustProd = getCustProdInfo();
            //}

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {
                string sBizRule = "BR_PRD_GET_INBOX_LABEL_FM";

                string custProdId = GetCustProdId(dgInbox.SelectedItem.GetValue("PRODID").ToString());
                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("PRODID");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("LABEL_TYPE_CODE");
                inData.Columns.Add("USERID");
                inData.Columns.Add("LOTID");
                inData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow inDataRow = inData.NewRow();
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                inDataRow["PRODID"] = dgInbox.SelectedItem.GetValue("PRODID").ToString();
                inDataRow["SHIPTO_ID"] = "M9999"; // cboVender_Inbox.SelectedValue;
                inDataRow["LABEL_TYPE_CODE"] = "INBOX";
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["LOTID"] = dgInbox.SelectedItem.GetValue("PKG_LOTID").ToString();
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = sBizRule;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = prtQty;// sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataTable inItemTable = inDataSet.Tables.Add("INITEM");
                inItemTable.Columns.Add("MODEL");
                inItemTable.Columns.Add("LINE");
                inItemTable.Columns.Add("CAPACITY");
                inItemTable.Columns.Add("GRADE");
                inItemTable.Columns.Add("LOT");
                inItemTable.Columns.Add("DATE");
                inItemTable.Columns.Add("VOLTAGE");
                inItemTable.Columns.Add("PN");
                inItemTable.Columns.Add("RN");
                //inItemTable.Columns.Add("CUSTOMERID01");
                //inItemTable.Columns.Add("CUSTOMERID02");
                //inItemTable.Columns.Add("CUSTOMERID03");
                //inItemTable.Columns.Add("CUSTOMERID04");
                //inItemTable.Columns.Add("CUSTOMERID05");
                //inItemTable.Columns.Add("CUSTOMERID06");
                //inItemTable.Columns.Add("CUSTOMERID07");
                //inItemTable.Columns.Add("CUSTOMERID08");
                //inItemTable.Columns.Add("CUSTPRODID01");
                //inItemTable.Columns.Add("CUSTPRODID02");
                //inItemTable.Columns.Add("CUSTPRODID03");
                //inItemTable.Columns.Add("CUSTPRODID04");
                //inItemTable.Columns.Add("CUSTPRODID05");
                //inItemTable.Columns.Add("CUSTPRODID06");
                //inItemTable.Columns.Add("CUSTPRODID07");
                //inItemTable.Columns.Add("CUSTPRODID08");
                DataRow inItemDr = inItemTable.NewRow();
                inItemDr["MODEL"] = dgInbox.SelectedItem.GetValue("MODEL_NAME").ToString();
                inItemDr["LINE"] = dgInbox.SelectedItem.GetValue("LINE_NO").ToString();
                inItemDr["CAPACITY"] = String.Format("{0:N0}", Convert.ToDecimal(dgInbox.SelectedItem.GetValue("CAPA"))) == "0" ? "" :String.Format("{0:N0}", Convert.ToDecimal(dgInbox.SelectedItem.GetValue("CAPA"))) + "mAh";
                inItemDr["GRADE"] = dgInbox.SelectedItem.GetValue("PRDT_GRD_CODE").ToString();
                inItemDr["LOT"] = dgInbox.SelectedItem.GetValue("LOTID_PRODDATE").ToString();
                inItemDr["DATE"] = DateTime.Now.ToString("yyyy.MM.dd");
                inItemDr["VOLTAGE"] = string.Format("{0:F2}", Convert.ToDecimal(cboSOC_Inbox.SelectedValue)) == "0.00" ? "" :string.Format("{0:F2}", Convert.ToDecimal(cboSOC_Inbox.SelectedValue)) + "V";
                inItemDr["PN"] = custProdId;
                inItemDr["RN"] = dgInbox.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString();
                //foreach (DataRow dr in dtCustProd.AsEnumerable())
                //{
                //    //INITEM[0]["CUSTOMERID0" + (i + 1).ToString()] = VAR_CUST_PRDT[i]["CUSTOMERID"].ToString() + " P/N:";
                //    //INITEM[0]["CUSTPRODID0" + (i + 1).ToString()] = VAR_CUST_PRDT[i]["CUSTPRODID"].ToString();

                //}
                //for (int i = 0; i < dtCustProd.Rows.Count; i++)
                //{
                //    inItemDr["CUSTOMERID0" + (i + 1).ToString()] = dtCustProd.Rows[i]["CUSTOMERID"].ToString() + "P/N:";
                //    inItemDr["CUSTPRODID0" + (i + 1).ToString()] = dtCustProd.Rows[i]["CUSTPRODID"].ToString();
                //}
                inItemTable.Rows.Add(inItemDr);

                //DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_LABEL_FM", "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet);
                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT,INITEM", "OUTDATA", inDataSet);

                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < resultDS.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        lblCode = resultDS.Tables["OUTDATA"].Rows[i]["LABEL_CODE"].GetString();
                        zplCode = resultDS.Tables["OUTDATA"].Rows[i]["ZPLCODE"].GetString();
                        //for (int i = 0; i < prtQty; i++)
                        //{
                        PrintLabel(zplCode, drPrtInfo);

                        System.Threading.Thread.Sleep(500);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    //}
                }
                else
                {
                    Util.MessageValidation("SFU4079"); //라벨 정보가 없습니다.
                    return;
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private DataSet AutoPrintInbox()
        {
            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;
            DataTable dtCustProd = new DataTable();

            DataSet indataSet = new DataSet();

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
            {
                indataSet = null;
                return indataSet;
            }
            try
            {
                string custProdId = GetCustProdId(dgInbox.SelectedItem.GetValue("PRODID").ToString());

                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID");
                inData.Columns.Add("PRODID");
                inData.Columns.Add("SHIPTO_ID");
                inData.Columns.Add("LABEL_TYPE_CODE");
                inData.Columns.Add("USERID");
                inData.Columns.Add("LOTID");
                inData.Columns.Add("PGM_ID");
                inData.Columns.Add("BZRULE_ID");

                DataRow inDataRow = inData.NewRow();
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                inDataRow["PRODID"] = dgInbox.SelectedItem.GetValue("PRODID").ToString();
                inDataRow["SHIPTO_ID"] = "M9999"; // cboVender_Inbox.SelectedValue;
                inDataRow["LABEL_TYPE_CODE"] = "INBOX";
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["LOTID"] = dgInbox.SelectedItem.GetValue("PKG_LOTID").ToString();
                inDataRow["PGM_ID"] = _sPGM_ID;
                inDataRow["BZRULE_ID"] = _sPGM_ID;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = 1;// sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataTable inItemTable = indataSet.Tables.Add("INITEM");
                inItemTable.Columns.Add("MODEL");
                inItemTable.Columns.Add("LINE");
                inItemTable.Columns.Add("CAPACITY");
                inItemTable.Columns.Add("GRADE");
                inItemTable.Columns.Add("LOT");
                inItemTable.Columns.Add("DATE");
                inItemTable.Columns.Add("VOLTAGE");
                inItemTable.Columns.Add("PN");
                inItemTable.Columns.Add("RN");

                DataRow inItemDr = inItemTable.NewRow();
                inItemDr["MODEL"] = dgInbox.SelectedItem.GetValue("MODEL_NAME").ToString();
                inItemDr["LINE"] = dgInbox.SelectedItem.GetValue("LINE_NO").ToString();
                inItemDr["CAPACITY"] = String.Format("{0:N0}", Convert.ToDecimal(dgInbox.SelectedItem.GetValue("CAPA"))) == "0" ? "" : String.Format("{0:N0}", Convert.ToDecimal(dgInbox.SelectedItem.GetValue("CAPA"))) + "mAh";
                inItemDr["GRADE"] = dgInbox.SelectedItem.GetValue("PRDT_GRD_CODE").ToString();
                inItemDr["LOT"] = dgInbox.SelectedItem.GetValue("LOTID_PRODDATE").ToString();
                inItemDr["DATE"] = DateTime.Now.ToString("yyyy.MM.dd");
                inItemDr["VOLTAGE"] = string.Format("{0:F2}", Convert.ToDecimal(cboSOC_Inbox.SelectedValue)) == "0.00" ? "" : string.Format("{0:F2}", Convert.ToDecimal(cboSOC_Inbox.SelectedValue)) + "V";
                inItemDr["PN"] = custProdId;
                inItemDr["RN"] = dgInbox.SelectedItem.GetValue("PRINT_MODEL_NAME").ToString();
                inItemTable.Rows.Add(inItemDr);

                return indataSet;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                indataSet = null;
                return indataSet;
            }
        }

        private void GetInPalletForInbox()
        {
            try
            {

                //C20190131_13476 lotid,pallet id 필수 입력 하도록 개선
                if (string.IsNullOrEmpty(txtLotId.Text) || txtLotId.Text.Length < 8)
                {
                    Util.MessageValidation("SFU4478");
                    return;
                }

                if (string.IsNullOrEmpty(txtInPalletId.Text) || txtInPalletId.Text.Length < 14)
                {
                    Util.MessageInfo("SFU5020"); // Pallet ID 정보가 없을 경우 In Box 라벨은 출력 할 수 없습니다.
                    return;
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("FROM_PACKDTTM");
                inDataTable.Columns.Add("TO_PACKDTTM");
                inDataTable.Columns.Add("PKG_LOTID");
                inDataTable.Columns.Add("PRDT_GRD_CODE");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PALLETID");

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["AREAID"] = cboArea.SelectedValue;
                inDataRow["PKG_LOTID"] = string.IsNullOrEmpty(txtLotId.Text) ? null : txtLotId.Text;
                inDataRow["PRDT_GRD_CODE"] = string.IsNullOrEmpty(txtGrade.Text) ? null : txtGrade.Text;
                inDataRow["PALLETID"] = string.IsNullOrEmpty(txtInPalletId.Text) ? null : txtInPalletId.Text;

                if (string.IsNullOrEmpty(inDataRow["PKG_LOTID"]?.ToString()))
                {
                    inDataRow["FROM_PACKDTTM"] = Convert.ToDateTime(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd"));
                    inDataRow["TO_PACKDTTM"] = Convert.ToDateTime(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59");
                }
                else
                {
                    inDataRow["FROM_PACKDTTM"] = null;
                    inDataRow["TO_PACKDTTM"] = null;
                }
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xmltxt = ds.GetXml();
                new ClientProxy().ExecuteService("BR_PRD_GET_PKG_LOT_FOR_INBOX_FM", "INDATA", "OUTDATA", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    Util.GridSetData(dgInbox, result, FrameOperation);
                    if (dgInbox.Rows.Count < 1 || dgInbox.ItemsSource == null)
                    {
                        Util.MessageInfo("SFU2816"); // 조회 결과가 없습니다
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// C20180829_77493
        /// </summary>
        private void GetInPalletPrintList(string sLotID, string sBoxID)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("PRT_TYPE");

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LOTID"] = sLotID;
                inDataRow["BOXID"] = sBoxID;
                inDataRow["PRT_TYPE"] = "TTI_INBOX";

                inDataTable.Rows.Add(inDataRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                new ClientProxy().ExecuteService("DA_PRD_SEL_LABEL_HIST_BOXID", "INDATA", "OUTDATA", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }

                    Util.GridSetData(dgTTIInBoxList, result, FrameOperation);
                    //DataTable dt = result;
                    if (dgTTIInBoxList.Rows.Count > 0 && dgTTIInBoxList.ItemsSource != null)
                    {
                        Util.MessageValidation("SFU5019"); // 메시지 변경 대상
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationAddInbox()
        {
            if (dgInbox.ItemsSource == null || dgInbox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgInbox.ItemsSource).Select("CHK = '1' or CHK = 'True'");

            if (dr.Length == 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return false;
            }

            if (cboSOC_Inbox.SelectedValue == null || cboSOC_Inbox.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4071"); //SOC를 선택하세요. 
                return false;
            }

            return true;
        }

        private bool ValidationInbox(bool skip)
        {
            if (dgInbox.ItemsSource == null || dgInbox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            if(!skip)
            {
                if (inBoxPrintQty.Value < 1)
                {
                    Util.MessageValidation("SFU4085");//발행 수량을 확인하세요.
                    return false;
                }
            }

            //if (cboVender_Inbox.SelectedValue == null || cboVender_Inbox.SelectedValue.ToString() == "SELECT")
            //{
            //    Util.MessageValidation("SFU4096"); //출하처를 선택하세요.
            //    return false;
            //}

            if (cboSOC_Inbox.SelectedValue == null || cboSOC_Inbox.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4071"); //SOC를 선택하세요. 
                return false;
            }
            if (dgInbox.Rows.Count < 1 || dgInbox.SelectedItem == null)
            {
                Util.Alert("10008"); //선택된 데이터가 없습니다.
                return false;
            }

            return true;
        }

        private void SetInboxSearchInfo()
        {
            //조회조건 세팅
            txtLotId.Text = dgPrintList.SelectedItem.GetValue("LOTID").ToString();
            txtGrade.Text = dgPrintList.SelectedItem.GetValue("PRDT_GRD_CODE").ToString();

            //DateTime dt;
            //DateTime.TryParse("2017-07-04",out dt);
            // dtpDateFrom.SelectedDateTime = dt;
            //txtLotId.Text = "DDMQF10S";
        }

        private void DeleteBox(string boxID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("AREAID");
                dt.Columns.Add("BOXID");
                dt.Columns.Add("USERID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BOXID"] = boxID;
                dr["USERID"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_OUTBOX_FM", "INBOX", null, dt);
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

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


        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private bool ValidationPrint(bool skip)
        {

            if (dgInbox.ItemsSource == null || dgInbox.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgInbox.ItemsSource).Select("CHK = '1' or CHK = 'True'");

            if (dr.Length == 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                return false;
            }

            if (cboSOC_Inbox.SelectedValue == null || cboSOC_Inbox.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4071"); //SOC를 선택하세요. 
                return false;
            }

            if (cboCustomer.SelectedIndex < 0 || cboCustomer.SelectedValue.GetString().Equals("SELECT"))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("고객사"));
                return false;
            }

            if(!skip)
            {
                if (txtCellQty.Value < 1)
                {
                    // Cell 정보 수량을 입력하세요
                    Util.MessageValidation("SFU4484");
                    return false;
                }

                if (inBoxPrintQty.Value == 0)
                {
                    // 발행 수량을 확인하세요.
                    Util.MessageValidation("SFU4085");
                    return false;
                }
            }

            return true;
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private void PrintProcessManual(string LotID)
        {
            try
            {
                string sPrt = string.Empty;
                string sRes = string.Empty;
                string sCopy = string.Empty;
                string sXpos = string.Empty;
                string sYpos = string.Empty;
                string sDark = string.Empty;
                DataRow drPrtInfo = null;

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

                string sBizRule = "BR_PRD_GET_INBOX_ADD_LABEL";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("CUSTOMERID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SOC");
                inDataTable.Columns.Add("QTY");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = LotID;
                newRow["CUSTOMERID"] = Util.NVC(cboCustomer.SelectedValue);
                newRow["USERID"] = txtWorker.Tag;
                newRow["SOC"] = cboSOC_Inbox.Text;
                newRow["QTY"] = txtCellQty.Value;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PRODID"] = dgInbox.SelectedItem.GetValue("PRODID").ToString().Trim();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_ADD_LABEL", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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
                        PrintLabel(zplCode, drPrtInfo);

                        ////   Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
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
            }
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private DataSet AutoPrintProcessManual(string LotID)
        {
            DataSet indataSet = new DataSet();
            try
            {
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
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("CUSTOMERID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SOC");
                inDataTable.Columns.Add("QTY");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("PGM_ID");
                inDataTable.Columns.Add("BZRULE_ID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = LotID;
                newRow["CUSTOMERID"] = Util.NVC(cboCustomer.SelectedValue);
                newRow["USERID"] = txtWorker.Tag;
                newRow["SOC"] = cboSOC_Inbox.Text;
                newRow["QTY"] = 100;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PRODID"] = dgInbox.SelectedItem.GetValue("PRODID").ToString().Trim();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = _sPGM_ID;
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);


                return indataSet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                indataSet = null;
                return indataSet;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
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
        //private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LGCDatePicker dtPik = sender as LGCDatePicker;
        //    if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
        //    {
        //        dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
        //        return;
        //    }
        //}
        //private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    LGCDatePicker dtPik = sender as LGCDatePicker;
        //    if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
        //    {
        //        dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
        //        return;
        //    }
        //}
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
            //Validation 추가
            if (sender != null)
            {
                isSearchButtonClicked = true;
            }
            GetInPalletList(isSearchButtonClicked);
            isSearchButtonClicked = false;
            chkAdjustQty.IsChecked = false;
        }
        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreateOutBox())
                    return;
                if (chkAdjustQty.IsChecked == true)
                {
                    Util.MessageValidation("SFU4080"); //조정된 수량으로 일괄 발행할 수 없습니다.
                    return;
                }

                int printQty = (int)dgPrintList.SelectedItem.GetValue("REST_BOX_QTY") - 1;
                int vQty = (int)dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY");
                int lastPrintQty = (int)dgPrintList.SelectedItem.GetValue("LAST_QTY");

                 
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4078"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (!CreateOutBox(vQty))
                            return;

                        for (int i = 0; i < printQty - 1; i++)
                        {
                            CreateOutBox(vQty); // cell수량 몇개인지 설정
                            System.Threading.Thread.Sleep(1000);
                        }

                        CreateOutBox(lastPrintQty);
                    }
                });
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateOutBox())
                return;

            if (isResidaulCell)
            {
                GetInboxScanPopUp();
                return;
            }

            string msg = string.Empty;
            string[] msgparams = new string[2];
            int adjustedCellQty = int.Parse(txtAdjustQty.Value.ToString());
            int printQty = int.Parse(txtPrintQty.Value.ToString()); //발행수량, forloop
            int totalCellQty = (int)dgPrintList.SelectedItem.GetValue("TOTAL_CELL_QTY");
            int endCellQty = (int)dgPrintList.SelectedItem.GetValue("END_CELL_QTY");
            int restCellQty = (int)dgPrintList.SelectedItem.GetValue("REST_CELL_QTY");
            int vQty = (int)dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY"); // cell수량 -> 수량 설정시 설정한 수량으로(잔여수량<수동설정수량 이면 총수량-완료수량으로)
            int lastPrintQty = (int)dgPrintList.SelectedItem.GetValue("LAST_QTY");
            bool isLastPrint = false; // 잔량박스 인쇄인지
                                      // + 잔량박스 무게 처리 부분 비즈에 있는지 확인해야함.
                                      // 잔량박스 인쇄후 잔량 0이면 아웃박스 발행 불가.

            if (chkAdjustQty.IsChecked == true)
            {
                if (txtAdjustQty.Value < 1)
                {
                    Util.MessageValidation("SFU4081");//Cell 수량 0으로 발행할 수 없습니다.
                    chkAdjustQty.IsChecked = false;
                    return;
                }


                if (restCellQty < adjustedCellQty) //잔량box인지
                {
                    printQty = 1;
                    vQty = totalCellQty - endCellQty;
                    //isLastPrint = true;
                }
                else
                {
                    vQty = adjustedCellQty;
                    if (restCellQty < vQty * printQty) //잔여 cell수량 <총 인쇄 cell수량 이면 한장 덜 인쇄.
                    {
                        printQty = (int)Math.Floor((decimal)((restCellQty) / vQty));

                        if (restCellQty % vQty == 0)
                            isLastPrint = false;
                        else
                            isLastPrint = true;
                    }
                    else
                    {
                        isLastPrint = false;
                    }
                    // isLastPrint = false;
                }
                //msg = "조정된 Cell 수량(" + vQty + ") 으로 OUTBOX " + (printQty + (isLastPrint ? 1 : 0)) + "회 발행하시겠습니까?"; //메시지 처리 필요
                msg = "SFU4082";
                msgparams[0] = vQty.ToString();
                msgparams[1] = (printQty + (isLastPrint ? 1 : 0)).ToString();
            }
            else
            {
                if (restCellQty < vQty * printQty) //잔여 cell수량 <총 인쇄 cell수량 이면 한장 덜 인쇄.
                {
                    printQty = (int)Math.Floor((decimal)((restCellQty) / vQty));
                    if (restCellQty % vQty == 0)
                        isLastPrint = false;
                    else
                        isLastPrint = true;
                }
                else
                {
                    isLastPrint = false;
                }
                //msg = "Cell 수량(" + vQty + ") 으로 OUTBOX " + (printQty + (isLastPrint ? 1 : 0)) + "회 발행하시겠습니까?"; //메시지 처리 필요
                msg = "SFU4083";
                msgparams[0] = vQty.ToString();
                msgparams[1] = (printQty + (isLastPrint ? 1 : 0)).ToString();
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg, msgparams), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
             {
                 if (result == MessageBoxResult.OK)
                 {
                     for (int i = 0; i < printQty; i++)
                     {
                         CreateOutBox(vQty); // cell수량 몇개인지 설정
                         System.Windows.Forms.Application.DoEvents();
                     }

                     if (isLastPrint) // 잔량박스인쇄
                     {
                         CreateOutBox(lastPrintQty);
                     }
                     chkAdjustQty.IsChecked = false;

                     txtPrintQty.Value = 0;
                 }
             });

        }

        private void btnResidualCell_Click(object sender, RoutedEventArgs e)
        {
            GetInboxScanPopUp();
        }

        private void btnAddPallet_Click(object sender, RoutedEventArgs e)
        {
            //validation
            CreatePallet();
        }
        //private void txtPrintQty_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    _printQty = (int)txtPrintQty.Value;
        //}
        //private void txtAdjustQty_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    _adjustedCellQty = (int)txtAdjustQty.Value;
        //}
        //private void txtPrintQty_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    _printQty = (int)txtPrintQty.Value;
        //}
        //private void txtAdjustQty_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    _adjustedCellQty = (int)txtAdjustQty.Value;
        //}
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                /*
                 * GMES-R0142 
                 * 2018-03-09
                 * 작업자 정보 저장안하도록 수정
                 */
                //GetEqptWrkInfo();

                txtShift.Text = Util.NVC(shiftPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(shiftPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(shiftPopup.USERNAME);
                txtWorker.Tag = Util.NVC(shiftPopup.USERID);
            }
            this.grdMain.Children.Remove(shiftPopup);
        }
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                /*
                * GMES-R0142 
                * 2018-03-09
                * 작업자 정보 저장안하도록 수정
                */

                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] =  LoginInfo.CFG_EQPT_ID;
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void cboSOC_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgPrintList.SelectedItem == null)
                return;
            SetPalletInfo();
        }

        private void cboVender_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgPrintList.SelectedItem == null)
                return;
            SetPalletInfo();
        }
        private void txtWorker_TextChanged(object sender, TextChangedEventArgs e)
        {
            // SetPalletInfo();
        }
        private void cboShippingMethod_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgPrintList.SelectedItem == null)
                return;
            GetInPalletList(false);
        }
        private void btnMergeSplit_Click(object sender, RoutedEventArgs e)
        {
            if (txtWorker.Tag == null || string.IsNullOrEmpty(txtWorker.Tag.ToString()))
            {
                Util.MessageValidation("SFU1842"); //작업자를 선택하세요.
                return;
            }

            BOX001_038_OUTBOX_SPLIT_MERGE popUp = new BOX001_038_OUTBOX_SPLIT_MERGE { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtWorker.Tag;
                //Parameters[1] = cboLine.SelectedValue;
                //Parameters[2] = cboEqpt.SelectedValue; // popup창에서 validation 함
                //Parameters[3] = txtShift_Main.Tag; // 작업조id
                //Parameters[4] = txtShift_Main.Text; // 작업조name
                //Parameters[5] = txtWorker_Main.Tag; // 작업자id
                //Parameters[6] = txtWorker_Main.Text; // 작업자name
                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
                //popUp.Closed += new EventHandler(runStart_Closed);
            }
        }
        private void btnSearch_Inbox_Click(object sender, RoutedEventArgs e)
        {
            //validation
            GetInPalletForInbox();
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private void btnPrintAdd_Inbox_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint(false))
                return;

            for (int cnt = 0; cnt < inBoxPrintQty.Value; cnt++)
            {
                PrintProcessManual(dgInbox.SelectedItem.GetValue("PKG_LOTID").ToString().Trim());
            }

        }


        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private void btnAutoPrintAdd_Inbox_Click(object sender, RoutedEventArgs e)
        {
            DataSet indataSet = new DataSet();
            indataSet = null;

            if (!ValidationPrint(true))
                return;

            indataSet = AutoPrintProcessManual(dgInbox.SelectedItem.GetValue("PKG_LOTID").ToString().Trim());


            Decimal Printcnt = Convert.ToDecimal(dgInbox.SelectedItem.GetValue("TOTAL_CELL_QTY").ToString()) - Convert.ToDecimal(dgInbox.SelectedItem.GetValue("END_CELL_QTY").ToString());
            if (indataSet != null)
            {
                BOX001_048 popupAutoLabelAdd = new BOX001_048 { FrameOperation = FrameOperation };
                if (ValidationGridAdd(popupAutoLabelAdd.Name) == false)
                    return;

                object[] parameters = new object[3];
                parameters[0] = "HP/SMP";
                parameters[1] = indataSet;
                parameters[2] = Printcnt;

                C1WindowExtension.SetParameters(popupAutoLabelAdd, parameters);

                Logger.Instance.WriteLine("Windows Popup", "Open");
                popupAutoLabelAdd.Closed += popupAutoLabelAdd_Closed;
                popupAutoLabelAdd.ShowModal();
                popupAutoLabelAdd.BringToFront();
            }
        }

        private void popupAutoLabelAdd_Closed(object sender, EventArgs e)
        {
            BOX001_048 popup = sender as BOX001_048;
            if (popup != null)
            {
                Logger.Instance.WriteLine("Popup Close", "Popup Close");
            }
            grdMain.Children.Remove(popup);

        }

        private void popupLabelAdd_Closed(object sender, EventArgs e)
        {
            BOX001_038_INBOX_ADD_LABEL popup = sender as BOX001_038_INBOX_ADD_LABEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        private void btnPrint_Inbox_Click(object sender, RoutedEventArgs e)
        {
            int printQty = (int)inBoxPrintQty.Value;
            if (!ValidationInbox(false))
                return;
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4084", printQty), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
             {
                 if (result == MessageBoxResult.OK)
                 {
                     PrintInbox(printQty);
                 }
             });
        }

        //2020.04.01  이현호   GMES Inbox 출력화면 중 자동출력 기능 추가 및 HP/SMP 라벨 팝업로직 화면으로 이동처리.
        private void btnAutoPrint_Inbox_Click(object sender, RoutedEventArgs e)
        {
            DataSet indataSet = new DataSet();
            indataSet = null;
            if (!ValidationInbox(true))
                return;

            indataSet = AutoPrintInbox();

            Decimal Printcnt = Convert.ToDecimal(dgInbox.SelectedItem.GetValue("TOTAL_CELL_QTY").ToString()) - Convert.ToDecimal(dgInbox.SelectedItem.GetValue("END_CELL_QTY").ToString());

            if (indataSet != null)
            {
                BOX001_048 popupAutoLabelAdd = new BOX001_048 { FrameOperation = FrameOperation };
                if (ValidationGridAdd(popupAutoLabelAdd.Name) == false)
                    return;

                object[] parameters = new object[3];
                parameters[0] = "SBD";
                parameters[1] = indataSet;
                parameters[2] = Printcnt;

                C1WindowExtension.SetParameters(popupAutoLabelAdd, parameters);

                Logger.Instance.WriteLine("Windows Popup", "Open");
                popupAutoLabelAdd.Closed += popupAutoLabelAdd_Closed;
                popupAutoLabelAdd.ShowModal();
                popupAutoLabelAdd.BringToFront();
            }
        }

        private void dgInboxChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null)
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    string prodID = rb.DataContext.GetValue("PRODID").ToString();
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    setSOCCombo(cboSOC_Inbox, CommonCombo.ComboStatus.SELECT, prodID);
                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgInbox.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgInbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
            {
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                }
                return;
            }
            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dg.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
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

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }

                    dgPrintList.SelectedIndex = idx;
                    //  dgPrintList.SelectedItem = dgPrintList.Rows[idx];

                    _inPltID = (string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "BOXID");

                    if (isManualSelect)
                    {
                        SetPalletInfo();
                    }
                    else
                    {
                        SetPalletInfoByOutboxID(_currentOutBox);
                    }
                    if (dgPrintList.SelectedItem != null && (int)dgPrintList.SelectedItem.GetValue("REST_CELL_QTY") <= 0)
                    {
                        btnPrint.IsEnabled = false;
                        btnPrintAll.IsEnabled = false;
                    }
                    else
                    {
                        btnPrint.IsEnabled = true;
                        btnPrintAll.IsEnabled = true;
                    }

                    //[C20180123_90089] 값이 세팅 될 경우 사용자 선택 할 수 없도록 개선
                    String sSocValue = "";
                    sSocValue = (string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "SOC");

                    if (!string.IsNullOrEmpty(sSocValue))
                    {
                        cboSOC.Text = sSocValue;
                        cboSOC.IsEnabled = false;

                    }
                    else
                    {
                        cboSOC.Text = "-SELECT-";
                        cboSOC.IsEnabled = true;
                    }

                    //
                    if (!_sInpalletID.Equals((string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "LOTID"))
                        && !_sLotID.Equals((string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "BOXID")))
                    {
                        _sInpalletID = ((string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "LOTID"));
                        _sLotID = ((string)DataTableConverter.GetValue(dgPrintList.SelectedItem, "BOXID"));
                        GetInPalletPrintList(_sInpalletID, _sLotID);
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void dgPrintList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null || dg.CurrentCell.Row.Index == dg.Rows.Count - 1)
            {
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                }
                return;
            }
            int idx = dg.CurrentCell.Row.Index;

            if (idx < 0)
                return;
            dgPrintList.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);

        }
        private void btnInbox_Click(object sender, RoutedEventArgs e)
        {
            inBoxTab.IsSelected = true;
            SetInboxSearchInfo();
            GetInPalletForInbox();
            if (dgInbox.Rows.Count > 0)
            {
                dgInbox.SelectedIndex = 0;
                if (cboSOC.SelectedItem != null && cboSOC.SelectedValue.ToString() != "SELECT")
                    cboSOC_Inbox.SelectedValue = cboSOC.SelectedValue;
            }
        }
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            BOX001_038_REPRINT popUp = new BOX001_038_REPRINT { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            string boxID = dgOutBoxList.Rows[index].DataItem.GetValue("BOXID").ToString();

            Util.MessageConfirm("SFU3260", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID");
                    dt.Columns.Add("BOXID");
                    dt.Columns.Add("USERID");
                    dt.Columns.Add("LANGID");
                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["BOXID"] = boxID;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dt.Rows.Add(dr);
                    new ClientProxy().ExecuteService("BR_PRD_DEL_OUTBOX_FM", "INBOX", null, dt, (rsdt, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                        }
                        Util.MessageInfo("SFU3544"); // 삭제되었습니다.
                                                     // --GetOutBoxList();
                        btnSearch_Click(null, null);
                    });

                }
            }, new string[] { boxID });
        }
        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInPalletList(true);
            }
        }
        private void chkAdjustQty_Checked(object sender, RoutedEventArgs e)
        {
            txtAdjustQty.IsEnabled = true;
        }
        private void chkAdjustQty_Unchecked(object sender, RoutedEventArgs e)
        {
            txtAdjustQty.IsEnabled = false;
            txtAdjustQty.Value = _defaultCellQty;

        }
        private void dgOutBoxList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgOutBoxList.SelectedItem != null)
            {
                string outboxID = dgOutBoxList.SelectedItem.GetValue("BOXID").ToString();
                SetPalletInfoByOutboxID(outboxID);
            }
        }
        private void txtPrintQty_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }
        private void txtAdjustQty_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }
        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("SFU4109", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //for (int i = dgOutBoxList.Rows.Count-1; i >= 0; i--)
                    //{
                    //    string boxID = dgOutBoxList.Rows[i].DataItem.GetValue("BOXID").ToString();
                    //    DeleteBox(boxID);
                    //    System.Windows.Forms.Application.DoEvents();
                    //}
                    foreach (DataRow dr in ((DataView)dgOutBoxList.ItemsSource).ToTable().AsEnumerable())
                    {
                        string boxID = dr["BOXID"].ToString();
                        DeleteBox(boxID);
                        btnSearch_Click(null, null);
                        System.Windows.Forms.Application.DoEvents();
                    }
                    Util.MessageInfo("SFU1889");
                }

            });
        }

        /// <summary>
        /// C20190131_13476 BOX001_042 open 처리 기능
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTTIInboxOpen_Click(object sender, RoutedEventArgs e)
        {
            BOX001_042 wndEqpComment = new BOX001_042();
            wndEqpComment.FrameOperation = FrameOperation;

            //if (wndEqpComment != null)
            //{
            object[] Parameters = new object[0];
            this.FrameOperation.OpenMenu("SFU010070110", true, null);
        }

        #endregion

    }

}
