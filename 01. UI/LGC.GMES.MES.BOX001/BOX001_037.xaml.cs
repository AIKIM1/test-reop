/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2018.03.28  이상훈   C20180123_90089 아웃박스 라벨 발행시 SOC 는 선택값이 아니라 INPALLET TAG 에 반영된 시스템 정보를 가져오는것으로 변경 요청
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using System.Net.Sockets;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using System.Threading;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_037 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        DataTable _dtInPallet; //소멸시점?
        private string _inPltID = string.Empty;
        private int _printQty = 1;
       // private int _adjustedCellQty = 0;
        private int _selIndex = 0; //선택된 Row
        private string _currentOutBox = string.Empty;
        private bool isSearchButtonClicked = false;
        private bool printedFlag = false;
        private bool isManualSelect = true; //발행 후 재조회에 의한 선택 : false, 사용자선택 :true
        private int _defaultCellQty = 0;
        private System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();
        private Socket _plc;
        private byte[] _readbuffer = new byte[1024];
        private byte[] _sendbuffer = { 80, 0, 0, 255, 255, 3, 0, 12, 0, 40, 0, 1, 4, 0, 0, 0, 0, 0, 160, 1, 0 }; //B0~16 Read Command 
        private byte[] _offbuffer = { 80, 0, 0, 255, 255, 3, 0, 13, 0, 16, 0, 1, 20, 1, 0, 1, 0, 0, 160, 1, 0, 0 }; //B1 OFF Command
        private byte[] _onbuffer = { 80, 0, 0, 255, 255, 3, 0, 13, 0, 16, 0, 1, 20, 1, 0, 1, 0, 0, 160, 1, 0, 16 }; //B1 On Command

        // C20180829_77493 반복 조회 처리 기능 방지
        private string _sInpalletID = "";
        private string _sLotID = "";


        //private System.Threading.Mutex SocketMutex;

        private int _port;
        private int _interval;

        private bool _skip = false;

        private bool isResidaulCell = false;

        string _sPGM_ID = "BOX001_037";

        #region Declaration & Constructor 



        public BOX001_037()
        {
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                Util.MessageValidation("SFU4070"); // 로컬 환경에서 실행하십시오.
                return;
            }

            InitializeComponent();

            Initialize();

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

        }

        #endregion
        Configuration appConfig = null;
        #region Initialize
        private void Initialize()
        {
            InitializeCombo();
            //GetEqptWrkInfo();


            //appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location + ".config");

            string customConfigFile = Assembly.GetExecutingAssembly().Location + ".config";

            if (System.IO.File.Exists(customConfigFile))
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = customConfigFile;
                appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);               
            }
            else
            {
                System.Windows.MessageBox.Show("셋업 파일이 없습니다.");
                return;
            }

            _port = int.Parse(appConfig.AppSettings.Settings["MinPort"].Value);//Properties.Settings.Default.MinPort;
            _interval = int.Parse(appConfig.AppSettings.Settings["Interval"].Value);
            txtLabelComplete.Text = _port.ToString();
            _timer.Interval = TimeSpan.FromMilliseconds(_interval);//TimeSpan.FromSeconds(Properties.Settings.Default.Interval);
            _timer.Tick += _timer_Tick;
            if (!_timer.IsEnabled)
                _timer.Start();
        }


        private void Connected(IAsyncResult iar)
        {
            Socket plc = (Socket)iar.AsyncState;

            try
            {
                plc.EndConnect(iar);
            }
            catch (SocketException)
            {
            }
        }

        private void SendData(IAsyncResult iar)
        {
            try
            {
                Socket plc = (Socket)iar.AsyncState;
                int sent = plc.EndSend(iar);
                plc.BeginReceive(_readbuffer, 0, 1024, SocketFlags.None, new AsyncCallback(ReceiveData), plc);
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void ReceiveData(IAsyncResult iar)
        {
            try
            {
                Socket plc = (Socket)iar.AsyncState;
                int recv = plc.EndReceive(iar);
                if (recv > 11)
                {
                    string s = Convert.ToString(_readbuffer[11], 2).PadLeft(8, '0'); // 00010001

                    Logger.Instance.WriteLine("ReceiveData", s, LogCategory.UI);

                    SetLabelRequest(s.Substring(7, 1));
                    //SetLabelComplete(s.Substring(6, 1));
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }

        delegate void SetResponseText(string bResponse);

        private void SetLabelRequest(string bResponse)
        {
            if (!txtLabelRequest.Dispatcher.CheckAccess())
            {
                txtLabelRequest.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetResponseText(SetLabelRequest), bResponse);
            }
            else
            {
                txtLabelRequest.Text = bResponse;
            }
        }

        private void SetLabelComplete(string bResponse)
        {
            if (!txtLabelComplete.Dispatcher.CheckAccess())
            {
                txtLabelComplete.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new SetResponseText(SetLabelComplete), bResponse);
            }
            else
            {
                txtLabelComplete.Text = bResponse;
            }
        }

        private void LabelPrintComplted()
        {
            if (_plc != null && _plc.Connected)
            {
                _plc.BeginSend(_onbuffer, 0, _onbuffer.Length, SocketFlags.None, new AsyncCallback(SendData), _plc);
                Logger.Instance.WriteLine("LabelPrintComplted", "Send Label OK", LogCategory.UI);
                Thread.Sleep(1000);
                _skip = false;
            }
            else
            {
                Logger.Instance.WriteLine("LabelPrintComplted", "Error Send Label OK", LogCategory.UI);
            }
        }

        private void DoPrintLabel()
        {
            btnPrint_Click(null, null);            
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_plc == null)
                    _plc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (_plc.Connected == false)
                {
                    //_timer.Stop();
                    btnConnect.Background = Brushes.Red;
                    btnConnect.Content = "Disconnected";
                    int.Parse(appConfig.AppSettings.Settings["MinPort"].Value);
                    //IPEndPoint iep = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.IPAddress), _port);
                    IPEndPoint iep = new IPEndPoint(IPAddress.Parse(appConfig.AppSettings.Settings["IPAddress"].Value), _port);
                    var result = _plc.BeginConnect(iep, null, null);
                    
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), true);

                    if (success)
                    {
                        _plc.EndConnect(result);
                    }
                    else
                    {
                        _plc.Close();
                        Logger.Instance.WriteLine("Disconnected PLC", "Connection Fail!");
                        throw new SocketException(10060); // Connection timed out. 
                    }
                    //_plc.Connect(iep);
                }

                if (_plc.Connected)
                {
                    btnConnect.Background = Brushes.Blue;
                    btnConnect.Content = "Connected";
                    if(_skip == false)
                        _plc.BeginSend(_sendbuffer, 0, _sendbuffer.Length, SocketFlags.None, new AsyncCallback(SendData), _plc);
                }
                else
                {
                    btnConnect.Background = Brushes.Red;
                    btnConnect.Content = "Disconnected";
                    ResetPort();
                }
            }
            catch (Exception ex)
            {
                if (_plc.Connected)
                    _plc.Disconnect(false);
                _plc = null;
                ResetPort();
            }
            finally
            {
                //if (!_timer.IsEnabled)
                //{
                //    _timer.Start();
                //}
            }
        }

        private void ResetPort()
        {
            if (_port > int.Parse(appConfig.AppSettings.Settings["MaxPort"].Value)) //Properties.Settings.Default.MaxPort)
                _port = int.Parse(appConfig.AppSettings.Settings["MinPort"].Value);//Properties.Settings.Default.MinPort;
            else
                _port += 1;

            txtLabelComplete.Text = _port.ToString();
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "OWMS_BOX_TYPE_CODE" };
            _combo.SetCombo(cboShippingMethod, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE");
            string[] sFilter2 = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboVender, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "SHIPTO_CP");
        }
        #endregion

        #region Methods
        private void GetInPalletList(bool isSearchButtonClicked)
        {
            //Validation 추가

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
                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PROC_SHIP_PALLET_FM", "INDATA", "OUTDATA,OUTBOX", inDataSet);
                Util.GridSetData(dgPrintList, resultDS.Tables["OUTDATA"], FrameOperation, true);
                Util.GridSetData(dgOutBoxList, resultDS.Tables["OUTBOX"], FrameOperation, true);
                _dtInPallet = resultDS.Tables["OUTDATA"].Copy();
                if (resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    _defaultCellQty = int.Parse(resultDS.Tables["OUTDATA"].Rows[0].GetValue("OUTBOX_CELL_QTY").GetString());
                    if (chkAdjustQty.IsChecked == false)
                    {
                        txtAdjustQty.Value = _defaultCellQty;
                    }
                    string prodID = resultDS.Tables["OUTDATA"].Rows[0].GetValue("PRODID").GetString();
                    if (isSearchButtonClicked)
                    {
                        setSOCCombo(cboSOC, CommonCombo.ComboStatus.SELECT, prodID);
                        if (!string.IsNullOrEmpty(resultDS.Tables["OUTDATA"].Rows[0].GetValue("SOC").ToString()))
                        {
                            cboSOC.Text = resultDS.Tables["OUTDATA"].Rows[0].GetValue("SOC").ToString();
                            //[C20180123_90089] 값이 세팅 될 경우 사용자 선택 할 수 없도록 개선
                            cboSOC.IsEnabled = false;

                        }
                        else
                        {
                            //[C20180123_90089] 값이 세팅 될 경우 사용자 선택 할 수 없도록 개선
                            cboSOC.IsEnabled = true;
                        }
                        isManualSelect = true;
                    }
                    else
                    {
                        dgPrintList.SelectedIndex = _selIndex;
                        if(_selIndex>-1)
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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (cboSOC.SelectedValue==null || cboSOC.SelectedValue.ToString().Equals("SELECT"))
                {
                    // TODO: 메세지처리
                    Util.MessageValidation("SFU4071"); // SOC를 선택하세요.
                }
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    Util.MessageValidation("SFU1843"); // 작업자 선택하시오
                }
            }
        }
        private void CreateOutBox(int vQty, string sInBox1 = null, string sInBox2 = null)
        {
            try
            {
                if (cboSOC.SelectedValue.Equals("SELECT"))
                {
                    Util.MessageValidation("SFU4072"); //SOC가 선택되지 않았습니다.
                    return;
                }
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
                inPalletDr["SHIPTO_ID"] = Util.NVC(cboVender.SelectedValue);  //출하처 콤보 세팅 후 주석 제거 =>TEST
                inPalletDr["SOC"] = Util.NVC(cboSOC.Text);

                inPalletDr["USERID"] = LoginInfo.USERID;
                inPalletDr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                inPalletDr["OWMS_BOX_TYPE_CODE"] = Util.NVC(cboShippingMethod.SelectedValue);
                inPalletDr["LANGID"] = LoginInfo.LANGID;
                inPalletDr["ACTUSER"] = txtWorker.Tag;
                inPalletDr["PGM_ID"] = _sPGM_ID;
                inPalletDr["BZRULE_ID"] = sBizRule;
                inPalletTable.Rows.Add(inPalletDr);

                DataRow inOutBoxDr = inOutBoxTable.NewRow();
                inOutBoxDr["PROJECT"] = dgPrintList.SelectedItem.GetValue("PROJECT");
                inOutBoxDr["OUTBOX_CELL_QTY"] = vQty == 0 ? (dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY")) : vQty;
                inOutBoxDr["BOX_WEIGHT"] = vQty == 0 ? string.Format("{0:N1}", dgPrintList.SelectedItem.GetValue("BOX_WEIGHT")) : Convert.ToString((vQty * dgPrintList.SelectedItem.GetValue("CELL_NET_WEIGHT").SafeToDouble())); //무게 처리부분 로직 확인필요
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
                inPrintDr["PRMK"] = appConfig.AppSettings.Settings["PrtModel"].Value;
                //inPrintDr["PRMK"] = "Z";  //테스트
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet ds = new DataSet();
                ds = inDataSet;
                string xmltxt = ds.GetXml();

                loadingIndicator.Visibility = Visibility.Visible;

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
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else
                        {
                            zplCode = zplCode.Substring(2);
                        }
                       if (appConfig.AppSettings.Settings["PrtModel"].Value.Equals("D"))
                        //if (false)    //테스트
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
                printedFlag = true;
                chkAdjustQty.IsChecked = false;
                
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void CreatePallet()
        {

            try
            {
                if (dgOutBoxList.Rows.Count <= 0)
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
                //string xmltxt = ds.GetXml();
                string outPallet = string.Empty;
                DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACKING_OUTBOX_FM", "INDATA,INOUTBOX", "OUTPALLET", inDataSet);

                if ((resultDs.Tables.IndexOf("OUTPALLET") > -1) && resultDs.Tables["OUTPALLET"].Rows.Count > 0)
                {
                    outPallet = resultDs.Tables["OUTPALLET"].Rows[0]["BOXID"].GetString();
                    btnSearch_Click(null, null);
                    System.Windows.MessageBox.Show("팔레트 구성이 완료되었습니다. Pallet ID :" + outPallet);
                    //Util.MessageValidation("SFU4073", outPallet);
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
        private void setSOCCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string prodID)
        {
            const string bizRuleName = "DA_BAS_SEL_PRDT_SOC_VLTG_COMBO";
            string[] arrColumn = { "SHOPID", "PRODID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, null);
        }

        private void Reprint()
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

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("BOXID");
                inData.Columns.Add("LANGID");
                DataRow inDataRow = inData.NewRow();
                inDataRow["BOXID"] = dgOutBoxList.SelectedItem.GetValue("BOXID").ToString();
                inDataRow["LANGID"] = LoginInfo.LANGID;
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
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRINT_FM", "INDATA,INPRINT", "OUTDATA", inDataSet);
                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    zplCode = resultDS.Tables["OUTDATA"].Rows[0]["ZPLCODE"].GetString();
                    if (zplCode.Split(',')[0].Equals("1"))
                    {
                        ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);

                    }
                    else
                    {
                        zplCode = zplCode.Substring(2);
                    }
                    if (appConfig.AppSettings.Settings["PrtModel"].Value.Equals("D"))
                    {
                        zplCode = zplCode.Insert(zplCode.IndexOf('q'), "");
                        zplCode = zplCode.Insert(zplCode.IndexOf('L'), "");
                        zplCode = (char)0x02 + zplCode + (char)0x03;
                    }
                    PrintLabel(zplCode, drPrtInfo);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private bool CanCreateOutBox()
        {
            //if (chkAutoPallet.IsChecked == true && txtAdjustPalletQty.Value > 0)  //자동pallet 구성 설정 시
            //{
            //    if (dgOutBoxList.GetRowCount() == txtAdjustPalletQty.Value)
            //    {
            //        CreatePallet();
            //        return true;
            //    }
            //}


            if (dgPrintList.SelectedItem == null||dgPrintList.SelectedIndex<0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return false;
            }

            if (string.IsNullOrEmpty(dgPrintList.SelectedItem.GetValue("MODEL_NAME").ToString()))
            {
                Util.MessageInfo("SFU4077"); //Model 정보가 없으므로 발행 불가합니다
                return false;
            }

            if (int.Parse(dgPrintList.SelectedItem.GetValue("REST_CELL_QTY").ToString()) <= 0)
            {
                System.Windows.MessageBox.Show("잔량이 부족하여 OUTBOX를 생성할 수 없습니다.");
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
                                //SetPalletInfo();
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
                txtInfo04.Text = chkAdjustQty.IsChecked == true ? Convert.ToString((txtAdjustQty.Value * dgPrintList.SelectedItem.GetValue("CELL_NET_WEIGHT").SafeToDouble())) + "Kg" : string.Format("{0:N1}", dgPrintList.SelectedItem.GetValue("BOX_NET_WEIGHT")) + "Kg";
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

                bcBoxID.Text = _currentOutBox;
                txtInfoBoxID.Text = _currentOutBox;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                if (rsDt != null || rsDt.Rows.Count > 0)
                {
                    txtInfo01.Text = rsDt.Rows[0].GetValue("PRT_ITEM01").ToString();
                    txtInfo02.Text = rsDt.Rows[0].GetValue("PRT_ITEM02").ToString();
                    txtInfo03.Text = rsDt.Rows[0].GetValue("PRT_ITEM03").ToString();
                    txtInfo04.Text = rsDt.Rows[0].GetValue("PRT_ITEM04").ToString();
                    txtInfo05.Text = rsDt.Rows[0].GetValue("PRT_ITEM05").ToString();
                    txtInfo06.Text = rsDt.Rows[0].GetValue("PRT_ITEM06").ToString();

                    txtInfo11.Text = rsDt.Rows[0].GetValue("PRT_ITEM08").ToString();
                    txtInfo12.Text = rsDt.Rows[0].GetValue("PRT_ITEM09").ToString();
                    txtInfo13.Text = rsDt.Rows[0].GetValue("PRT_ITEM10").ToString();
                    txtInfo14.Text = rsDt.Rows[0].GetValue("PRT_ITEM11").ToString();
                    txtInfo15.Text = rsDt.Rows[0].GetValue("PRT_ITEM12").ToString();
                    txtInfo16.Text = rsDt.Rows[0].GetValue("PRT_ITEM13").ToString();

                    txtInfo21.Text = rsDt.Rows[0].GetValue("PRT_ITEM14").ToString();
                    txtInfo22.Text = rsDt.Rows[0].GetValue("PRT_ITEM16").ToString();
                    txtInfo23.Text = rsDt.Rows[0].GetValue("PRT_ITEM18").ToString();
                    txtInfo24.Text = rsDt.Rows[0].GetValue("PRT_ITEM20").ToString();
                    txtInfo25.Text = rsDt.Rows[0].GetValue("PRT_ITEM22").ToString();
                    txtInfo26.Text = rsDt.Rows[0].GetValue("PRT_ITEM24").ToString();

                    txtInfo31.Text = rsDt.Rows[0].GetValue("PRT_ITEM15").ToString();
                    txtInfo32.Text = rsDt.Rows[0].GetValue("PRT_ITEM17").ToString();
                    txtInfo33.Text = rsDt.Rows[0].GetValue("PRT_ITEM19").ToString();
                    txtInfo34.Text = rsDt.Rows[0].GetValue("PRT_ITEM21").ToString();
                    txtInfo35.Text = rsDt.Rows[0].GetValue("PRT_ITEM23").ToString();
                    txtInfo36.Text = rsDt.Rows[0].GetValue("PRT_ITEM25").ToString();

                    bcBoxID.Text = rsDt.Rows[0].GetValue("PRT_ITEM26").ToString();
                    txtInfoBoxID.Text = rsDt.Rows[0].GetValue("PRT_ITEM27").ToString();
                }
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
        #endregion

        #region Events
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

                if (dtResult.Rows.Count > 0)
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
            //txtAdjustQty.Value = int.Parse(dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY").ToString());
        }
        private void btnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateOutBox())
                return;

            int printQty = (int)dgPrintList.SelectedItem.GetValue("REST_BOX_QTY") - 1;
            int vQty = (int)dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY");
            int lastPrintQty = (int)dgPrintList.SelectedItem.GetValue("LAST_QTY");


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4078"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    for (int i = 0; i < printQty; i++)
                    {
                        CreateOutBox(vQty); // cell수량 몇개인지 설정
                    }

                    CreateOutBox(lastPrintQty);
                }
            });
        }

        private void PrintLabel(bool bSkip)
        {          
            if (CanCreateOutBox())
            {
                if (isResidaulCell)
                {
                    GetInboxScanPopUp();                   
                    return;
                }

                string msg = string.Empty;
                int printQty; //발행수량, forloop
                              //int totalCellQty = (int)dgPrintList.SelectedItem.GetValue("TOTAL_CELL_QTY");
                              //int endCellQty = (int)dgPrintList.SelectedItem.GetValue("END_CELL_QTY");
                int adjustedCellQty = int.Parse(txtAdjustQty.Value.ToString());
                int restCellQty = (int)dgPrintList.SelectedItem.GetValue("REST_CELL_QTY");
                int vQty = (int)dgPrintList.SelectedItem.GetValue("OUTBOX_CELL_QTY"); // cell수량 -> 수량 설정시 설정한 수량으로(잔여수량<수동설정수량 이면 총수량-완료수량으로)
                int lastPrintQty = (int)dgPrintList.SelectedItem.GetValue("LAST_QTY");
                bool isLastPrint = false; // 잔량박스 인쇄인지
                                          // + 잔량박스 무게 처리 부분 비즈에 있는지 확인해야함.
                                          // 잔량박스 인쇄후 잔량 0이면 아웃박스 발행 불가.

                if (chkAdjustQty.IsChecked == true && adjustedCellQty > 0) // cell수량 조정 체크시 발행수량은 1장, vQty 설정
                {
                    printQty = 1;
                    if (restCellQty < adjustedCellQty) //잔량box인지
                    {
                        // vQty = totalCellQty - endCellQty;
                        isLastPrint = true;
                    }
                    else
                    {
                        vQty = adjustedCellQty;
                        isLastPrint = false;
                    }
                    msg = "조정된 Cell 수량(" + vQty + ") 으로 OUTBOX 발행하시겠습니까?"; //메시지 처리 필요
                }
                else
                {
                    if (restCellQty < vQty * _printQty) //잔여 cell수량 <총 인쇄 cell수량 이면 한장 덜 인쇄.
                    {
                        printQty = (int)Math.Floor((decimal)((restCellQty) / vQty));
                        isLastPrint = true;
                    }
                    else
                    {
                        printQty = _printQty;
                        isLastPrint = false;
                    }
                    msg = "Cell 수량(" + vQty + ") 으로 OUTBOX " + (printQty + (isLastPrint ? 1 : 0)) + "회 발행하시겠습니까?"; //메시지 처리 필요
                }

                for (int i = 0; i < printQty; i++)
                {
                    CreateOutBox(vQty); // cell수량 몇개인지 설정
                }

                if (isLastPrint) // 잔량박스인쇄
                {
                    CreateOutBox(lastPrintQty);
                }

                // 팔레트 구성 여부

                if (chkAutoPallet.IsChecked == true && txtAdjustPalletQty.Value > 0)  //자동pallet 구성 설정 시
                {
                    if (dgOutBoxList.GetRowCount() == txtAdjustPalletQty.Value)
                    {
                        _skip = true;
                        CreatePallet();
                        chkAutoPallet.IsChecked = false;
                        txtAdjustPalletQty.Value = 0;
                    }
                }

                LabelPrintComplted();
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
                    if (chkAutoPallet.IsChecked == true && txtAdjustPalletQty.Value > 0)  //자동pallet 구성 설정 시
                    {
                        if (dgOutBoxList.GetRowCount() == txtAdjustPalletQty.Value)
                        {
                            _skip = true;
                            CreatePallet();
                            chkAutoPallet.IsChecked = false;
                            txtAdjustPalletQty.Value = 0;
                        }
                    }
                    LabelPrintComplted();
                }
                grdMain.Children.Remove(popup);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            // 수동 라벨 발행시 
            PrintLabel(true);
        }
       
        private void btnAddPallet_Click(object sender, RoutedEventArgs e)
        {
            //validation
            CreatePallet();
        }

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
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                Parameters[7] = "N";  // 저장 플로그 "Y" 일때만 저장.
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
            //SetPalletInfo();
        }
        private void cboShippingMethod_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgPrintList.SelectedItem == null)
                return;
            GetInPalletList(false);

        }
        private void dgPrintList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null )
            {
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);
                }
                dg.SelectedIndex = -1;
                dg.SelectedItem = null;
                return;
            }
            if( dg.CurrentCell.Row.Index == dg.Rows.Count - 1)
            {
                for (int i = 0; i < dg.Rows.Count; i++)
                {
                    if (dg.Rows[i].DataItem.GetValue("CHK").Equals(true))
                        dg.SelectedItem = dg.Rows[i].DataItem;
                }
                return;
            }
            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
            dgPrintList.SelectedIndex = idx;
            
            dgPrintList.SelectedItem = dgPrintList.Rows[idx].DataItem;
            dgPrintList.EndEdit();
            dgPrintList.EndEditRow(true);

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

                if (drv != null)
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }

                    dgPrintList.SelectedIndex = idx;
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

        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
                    string boxID = dgOutBoxList.Rows[index].DataItem.GetValue("BOXID").ToString();
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
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                Util.MessageException(ex);
                            }
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageInfo("SFU4104", boxID);//Outbox ID : %1가 삭제되었습니다.
                            btnSearch_Click(null, null);
                        });

                }
            });
        }

        private string _trigger = string.Empty;
        private void txtLabelRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // 0 => 1로 변경되었을 때만 실행
                if (_trigger == "0" && txtLabelRequest.Text == "1")
                {
                    Logger.Instance.WriteLine("LabelRequest On", string.Format("Prev {0}, Curr {1}", _trigger, txtLabelRequest.Text), LogCategory.UI);
                    //btnPrint_Click(null, null);
                    PrintLabel(false);
                }
                else if(_trigger == "1" && txtLabelRequest.Text == "0")
                {
                    Logger.Instance.WriteLine("LabelRequest Off", string.Format("Prev {0}, Curr {1}", _trigger, txtLabelRequest.Text), LogCategory.UI);
                }
                
            }
            catch
            {

            }
            finally
            {
                _trigger = txtLabelRequest.Text;
            }
        }

        private void txtLabelComplete_TextChanged(object sender, TextChangedEventArgs e)
        {

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

        private void chkAutoPallet_Checked(object sender, RoutedEventArgs e)
        {
            txtAdjustPalletQty.IsEnabled = true;
        }

        private void chkAutoPallet_Unchecked(object sender, RoutedEventArgs e)
        {
            txtAdjustPalletQty.IsEnabled = false;
        }
        private void dgOutBoxList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgOutBoxList.SelectedItem != null)
            {
                string outboxID = dgOutBoxList.SelectedItem.GetValue("BOXID").ToString();
                SetPalletInfoByOutboxID(outboxID);
            }
        }

        private void txtAdjustQty_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void txtAdjustPalletQty_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetInPalletList(true);
            }
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            if(dgOutBoxList.Rows.Count<1||dgOutBoxList.SelectedItem==null||dgOutBoxList.SelectedIndex<0)
            {
                Util.MessageValidation("SFU1651"); //선택된 항목이 없습니다.
                return;
            }
            Reprint();
        }
    }
}

//adjustQty => 디비 조회시 qty 정보 가져다가 바인딩 시켜놓기..
