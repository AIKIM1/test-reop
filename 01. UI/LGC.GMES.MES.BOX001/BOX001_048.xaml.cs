/*************************************************************************************
 Created Date : 2020.04.03
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.03  이현호  C20200214-000297 소형전지 물류 자동포장 설비 인박스 라벨 출력 형태 변경의 건 (INBOX 자동포장 관련하여 신규팝업 생성)
                                       추가 수정화면{BOX001_038(수동인쇄2차),BOX011_042(TTI 라벨발행)
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
    public partial class BOX001_048 : C1Window, IWorkArea
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
        public Socket _plc;

        private string Prev_Bit = "";
        private byte[] _readbuffer = new byte[1024];
        private byte[] _sendbuffer = { 80, 0, 0, 255, 255, 3, 0, 12, 0, 40, 0, 1, 4, 0, 0, 16, 0, 0, 160, 1, 0 }; //B0~16 Read Command 
        private byte[] _offbuffer = { 80, 0, 0, 255, 255, 3, 0, 13, 0, 16, 0, 1, 20, 1, 0, 17, 0, 0, 160, 1, 0, 0 }; //B1 OFF Command
        private byte[] _onbuffer = { 80, 0, 0, 255, 255, 3, 0, 13, 0, 16, 0, 1, 20, 1, 0, 17, 0, 0, 160, 1, 0, 16 }; //B1 On Command
        //16,17 00 = A0
        //15번째 메모리주소
        //18번재 메모리영역 160 = B영역
        //예시 15번째부터 0,0,0,160 = B영역A0바이너리에 0번째주소 B0
        //예시 15번째부터 1,0,0,160 = B영역A0바이너리에 1번째주소 B1
        string sLINEID = string.Empty;


        DataSet dInDataSet = null;
        Decimal nPrintcnt = 1;
        string sTitle = string.Empty;
        Decimal nTotalcellqty = 1;

        //private System.Threading.Mutex SocketMutex;

        private int _port;
        private int _interval;

        private bool _skip = false;
        private bool Lastprint = false;

        string _sPGM_ID = "BOX001_048";

        #region Declaration & Constructor 



        public BOX001_048()
        {
            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC") // 어떻게 처리할지 .. PAGE만 종료?
            {
                Util.MessageValidation("SFU4070"); // 로컬 환경에서 실행하십시오.
                return;
            }

            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (dInDataSet != null)
            {
                dInDataSet = null;
            }

            if (tmps.Length == 3)
            {
                sTitle = Util.NVC(tmps[0]);
                dInDataSet = tmps[1] as DataSet;
                nPrintcnt = (Decimal)tmps[2];
                nTotalcellqty = (Decimal)tmps[2];
            }

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            txttile.Text = sTitle;

            lblCurrentcnt.Text = "0";
            lblTotalcnt.Text = Math.Ceiling(nPrintcnt / 100).ToString();

            Initialize();

        }

        #endregion
        Configuration appConfig = null;
        #region Initialize
        private void Initialize()
        {
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
                    string s = Convert.ToString(_readbuffer[11], 2).PadLeft(8, '0'); // 01000010

                    if (Prev_Bit != s)
                    {
                        Prev_Bit = s;

                        Logger.Instance.WriteLine("ReceiveData Prev", Prev_Bit, LogCategory.UI);

                        Logger.Instance.WriteLine("ReceiveData Curr", s, LogCategory.UI);
                    }

                    SetLabelRequest(s.Substring(1, 1));
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

        private static void DisconnectCallback(IAsyncResult iar)
        {
            // Complete the disconnect request.
            Socket plc = (Socket)iar.AsyncState;
            plc.EndDisconnect(iar);
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
                    Logger.Instance.WriteLine("Disconnected PLC", _port.ToString());
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
                    if (_skip == false)
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
        #endregion

        #region Methods

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

        #region Events

        private void PrintLabel(bool bSkip)
        {
            string zplCode = string.Empty;
            string lblCode = string.Empty;

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;

            // 팔레트 구성 여부

            DataRow drPrtInfo = null;

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {

                PrintLabel(zplCode, drPrtInfo);

                LabelPrintComplted();
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private string _trigger = string.Empty;
        private void txtLabelRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // 0 => 1로 변경되었을 때만 실행
                if (_trigger == "0" && txtLabelRequest.Text == "1")
                {
                    if (nTotalcellqty % 100 != 0)
                    {
                        Lastprint = true;
                    }

                    Logger.Instance.WriteLine("LabelRequest On", string.Format("Prev {0}, Curr {1}", _trigger, txtLabelRequest.Text), LogCategory.UI);
                    if (Convert.ToInt16(lblCurrentcnt.Text) < Convert.ToInt16(lblTotalcnt.Text))
                    {
                        if (txttile.Text == "HP/SMP")
                        {
                            HPSMPPrint();
                        }
                        else if (txttile.Text == "SBD")
                        {
                            SBDPrint();
                        }
                        else if (txttile.Text == "TTI")
                        {
                            TTIPrint();
                        }
                    }
                }
                else if (_trigger == "1" && txtLabelRequest.Text == "0")
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

        #endregion

        private void SBDPrint()
        {
            string zplCode = "";

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (dInDataSet != null)
            {

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                string sBizRule = "BR_PRD_GET_INBOX_LABEL_FM";

                dInDataSet.Tables["INDATA"].Rows[0]["PGM_ID"] = _sPGM_ID;
                dInDataSet.Tables["INDATA"].Rows[0]["BZRULE_ID"] = sBizRule;

                //DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_LABEL_FM", "INDATA,INPRINT,INITEM", "OUTDATA", dInDataSet);
                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT,INITEM", "OUTDATA", dInDataSet);

                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < resultDS.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        zplCode = resultDS.Tables["OUTDATA"].Rows[i]["ZPLCODE"].GetString();
                        //for (int i = 0; i < prtQty; i++)
                        //{
                        PrintLabel(zplCode, drPrtInfo);
                    }
                    lblCurrentcnt.Text = (Convert.ToInt16(lblCurrentcnt.Text) + 1).ToString();
                    LabelPrintComplted();
                }
            }
            else
            {
                Util.MessageValidation("SFU4079"); //라벨 정보가 없습니다.
                return;
            }
        }

        private void HPSMPPrint()
        {
            string zplCode = "";

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (dInDataSet != null)
            {

                if (Lastprint == true && Convert.ToInt16(lblCurrentcnt.Text) == (Convert.ToInt16(lblTotalcnt.Text) - 1))
                {
                    dInDataSet.Tables["INDATA"].Rows[0]["QTY"] = nTotalcellqty;
                }

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                string sBizRule = "BR_PRD_GET_INBOX_ADD_LABEL";

                dInDataSet.Tables["INDATA"].Rows[0]["PGM_ID"] = _sPGM_ID;
                dInDataSet.Tables["INDATA"].Rows[0]["BZRULE_ID"] = sBizRule;

                //DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_ADD_LABEL", "INDATA,INPRINT", "OUTDATA", dInDataSet);
                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", dInDataSet);

                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < resultDS.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        zplCode = resultDS.Tables["OUTDATA"].Rows[i]["ZPLCODE"].GetString();
                        //for (int i = 0; i < prtQty; i++)
                        //{
                        PrintLabel(zplCode, drPrtInfo);
                    }

                    lblCurrentcnt.Text = (Convert.ToInt16(lblCurrentcnt.Text) + 1).ToString();
                    LabelPrintComplted();

                    nTotalcellqty = nTotalcellqty - 100;
                    if (nTotalcellqty < 0)
                    {
                        nTotalcellqty = 0;
                    }
                }
            }
            else
            {
                Util.MessageValidation("SFU4079"); //라벨 정보가 없습니다.
                return;
            }
        }

        private void TTIPrint()
        {
            string lblCode = "";
            string zplCode = "";

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (dInDataSet != null)
            {

                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                    return;

                string sBizRule = "BR_PRD_GET_LABEL_ZPL_FM";

                dInDataSet.Tables["INDATA"].Rows[0]["PGM_ID"] = _sPGM_ID;
                dInDataSet.Tables["INDATA"].Rows[0]["BZRULE_ID"] = sBizRule;

                //DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_LABEL_ZPL_FM", "INDATA,INPRINT,INITEM", "OUTDATA", dInDataSet);
                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INPRINT,INITEM", "OUTDATA", dInDataSet);

                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    for (int i = 0; i < resultDS.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        zplCode = resultDS.Tables["OUTDATA"].Rows[0]["ZPLCODE"].GetString();

                        zplCode = zplCode.Substring(2);

                        PrintLabel(zplCode, drPrtInfo);
                    }
                    lblCurrentcnt.Text = (Convert.ToInt16(lblCurrentcnt.Text) + 1).ToString();
                    LabelPrintComplted();
                }
            }
            else
            {
                Util.MessageValidation("SFU4079"); //라벨 정보가 없습니다.
                return;
            }
        }

        private void btnInBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt16(lblCurrentcnt.Text) < Convert.ToInt16(lblTotalcnt.Text))
            {
                if (txttile.Text == "HP/SMP")
                {
                    HPSMPPrint();
                }
                else if (txttile.Text == "SBD")
                {
                    SBDPrint();
                }
                else if (txttile.Text == "TTI")
                {
                    TTIPrint();
                }
            }
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_plc != null)
            {
                _timer.Stop();
                _plc.Disconnect(false);
                //_plc.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), _plc);
                _plc.Close();
                _plc = null;
                Logger.Instance.WriteLine("Disconnected PLC", "Disconnected");
            }
        }

    }
}
