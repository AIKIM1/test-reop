using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.Common;
using Microsoft.Win32.SafeHandles;

namespace LGC.GMES.MES.MainFrame.Controls
{
    public enum PrinterStatus { Error, Connected, LabelNotTaken };
    public class Printer : IDisposable
    {
        private static readonly string PRINTER_STATUS_CONNECTED = "Connected";
        private static readonly string PRINTER_STATUS_DISCONNECTED = "Can not connect to this printer";
        private static readonly string PRINTER_STATUS_NOLABEL = "This printer does not have any label";
        private static readonly string PRINTER_STATUS_PAUSED = "This printer is paused";
        private static readonly string PRINTER_STATUS_BUFFERFULL = "The buffer of this printer is full";
        private static readonly string PRINTER_STATUS_TEMPERATURELOW = "The header temperature of this printer is too low";
        private static readonly string PRINTER_STATUS_TEMPERATUREHIGH = "The header temperature of this printer is too high";
        private static readonly string PRINTER_STATUS_HEADOPENED = "The head lever of this printer is opend";
        private static readonly string PRINTER_STATUS_NORIBBON = "This printer does not have a ribbon";
        private static readonly string PRINTER_STATUS_INCORRECT = "Can not check status of this printer";
        private static readonly string PRINTER_STATUS_LABELNOTTAKEN = "There is a printed label on this printer";
        private static readonly int PRINTER_READ_TIMEOUT = 2 * 1000;
        private static readonly int PRINTER_WRITE_DELAY = 1 * 1000;
        private SerialPort _serialPort;
        private object _lockObject = new object();
        private string portName;
        private int baudRate;
        private Parity parity;
        private int dataBits;
        private StopBits stopBits;
        private int x;
        private int y;
        private bool connectionless;

        public string PortName
        {
            get
            {
                if (_serialPort == null)
                    return string.Empty;

                return _serialPort.PortName;
            }
        }

        public PrinterStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public int X { get { return x; } }
        public int Y { get { return y; } }

        public bool Connectionless { get { return connectionless; } }

        internal Printer(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int x, int y, bool connectionless)
        {
            try
            {
                this.portName = portName;
                this.baudRate = baudRate;
                this.parity = parity;
                this.dataBits = dataBits;
                this.stopBits = stopBits;
                this.x = x;
                this.y = y;
                this.connectionless = connectionless;

                if (!connectionless)
                {
                    _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                    //_serialPort.Open();
                }

                Status = PrinterStatus.Connected;
                StatusMessage = PRINTER_STATUS_CONNECTED;
            }
            catch (Exception ex)
            {
                Status = PrinterStatus.Error;
                StatusMessage = ex.Message;
            }
        }

        internal void checkPrinterStatus()
        {
            lock (_lockObject)
            {
                if (_serialPort == null)
                {
                    try
                    {
                        _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                        //string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Open";
                        //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                        _serialPort.Open();
                        Status = PrinterStatus.Connected;
                        StatusMessage = PRINTER_STATUS_CONNECTED;
                    }
                    catch (Exception ex)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = ex.Message;
                    }
                }

                if (_serialPort == null)
                {
                    return;
                }

                if (!_serialPort.IsOpen)
                {
                    try
                    {
                        //string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Open";
                        //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                        _serialPort.Open();
                    }
                    catch (Exception ex)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = ex.Message;
                        return;
                    }
                }

                Status = PrinterStatus.Connected;
                StatusMessage = PRINTER_STATUS_CONNECTED;

                string statusCheckMsg = "~HS";

                _serialPort.DiscardInBuffer();


                // 상태 체크 무시
                // 제브라 프린터만 상태체크가 가능함으로 
                // 향후 변경이 필요함

                return;

                writeToSerialPort(statusCheckMsg);

                Thread.Sleep(PRINTER_WRITE_DELAY);

                DateTime readDttm = DateTime.Now;
                string received = string.Empty;

                do
                {
                    try
                    {
                        received = readFromSerialPort();

                        if (DateTime.Now.Subtract(readDttm).TotalMilliseconds > PRINTER_READ_TIMEOUT)
                            break;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                while (received.Length < 0x52);

                if (string.IsNullOrEmpty(received))
                {
                    Status = PrinterStatus.Error;
                    StatusMessage = PRINTER_STATUS_DISCONNECTED;
                }
                else if (received.Length >= 0x52)
                {
                    int num5 = 0;
                    int num6 = 0;

                    try
                    {
                        num5 = int.Parse(received.Substring(0x2b, 1));
                        num6 = int.Parse(received.Substring(0x2d, 1));
                    }
                    catch (Exception ex)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_INCORRECT;

                        return;
                    }

                    if (int.Parse(received.Substring(5, 1)) == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_NOLABEL;
                    }
                    else if (int.Parse(received.Substring(7, 1)) == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_PAUSED;
                    }
                    else if (int.Parse(received.Substring(0x12, 1)) == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_BUFFERFULL;
                    }
                    else if (int.Parse(received.Substring(30, 1)) == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_TEMPERATURELOW;
                    }
                    else if (int.Parse(received.Substring(0x20, 1)) == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_TEMPERATUREHIGH;
                    }
                    else if (num5 == 1)
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_HEADOPENED;
                    }
                    else if ((num5 == 0) && (num6 == 1))
                    {
                        Status = PrinterStatus.Error;
                        StatusMessage = PRINTER_STATUS_NORIBBON;
                    }
                }
                else
                {
                    Status = PrinterStatus.Error;
                    StatusMessage = PRINTER_STATUS_INCORRECT;
                }

                if (Status == PrinterStatus.Connected)
                {
                    if (checkLabelStatus(received))
                    {
                        Status = PrinterStatus.LabelNotTaken;
                        StatusMessage = PRINTER_STATUS_LABELNOTTAKEN;
                    }
                }

                if (_serialPort.IsOpen == true)
                {
                    //string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Close";
                    //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                    _serialPort.Close();
                }
            }
        }

        internal void writeToSerialPort(string text)
        {
            //byte[] buffer = Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text));
            //_serialPort.Write(buffer, 0, buffer.Length);
            byte[] buffer = Encoding.Default.GetBytes(text);
            _serialPort.Write(buffer, 0, buffer.Length);
        }

        public bool Print(string zpl)
        {
            lock (_lockObject)
            {
                checkPrinterStatus();

                //string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Open";
                //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                if(!_serialPort.IsOpen)
                    _serialPort.Open();

                if (_serialPort.IsOpen || Status != PrinterStatus.Error)
                {
                    writeToSerialPort(zpl);

                    if (Connectionless)
                        this.Dispose();

                    if (!_serialPort.IsOpen)
                    {
                        //string msg01 = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Close";
                        //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg01, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                        _serialPort.Close();
                    }

                    return true;
                }
                else
                {
                    if (Connectionless)
                    {
                        this.Dispose();
                    }
                    if (!_serialPort.IsOpen)
                    {
                        //string msg02 = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Close";
                        //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg02, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                        _serialPort.Close();
                    }
                    return false;
                }
            }
        }

        private string readFromSerialPort()
        {
            return Encoding.Default.GetString(Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(_serialPort.ReadExisting())));
        }

        private bool checkLabelStatus(string received)
        {
            if (received.Length >= 0x52)
            {
                if (received.Substring(0x35, 1) == "1")
                {
                    return true;
                }
                else if (received.Substring(0x35, 1) == "0")
                {
                    if (int.Parse(received.Substring(0x37, 8)) == 0)
                        return false;
                    else
                        return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_serialPort == null)
                return;

            lock (_lockObject)
            {
                if (_serialPort.IsOpen)
                {
                    //string msg = new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Close";
                    //LGC.GMES.MES.Common.Logger.Instance.WriteLine(msg, LGC.GMES.MES.Common.Logger.MESSAGE_OPERATION_START);
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
        }
    }

    public class PrinterManager
    {
        public static readonly PrinterManager Instance = new PrinterManager();

        private static readonly int CHECKING_PERIOD = 1 * 1000;

        private Dictionary<string, Printer> printerList = new Dictionary<string, Printer>();
        private Thread checkingThread;
        private object _lockObject = new object();
        private object _checkingCompleteMonitorObject = new object();
        private bool checkingStop = false;

        public object CheckingCompleteMonitorObject { get { return _checkingCompleteMonitorObject; } }

        private PrinterManager()
        {
        }

        public Printer CreateOrGetPrinter(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int x, int y, bool connectionless)
        {
            if (printerList.ContainsKey(portName))
            {
                return printerList[portName];
            }
            else
            {
                Printer newPrinter = new Printer(portName, baudRate, parity, dataBits, stopBits, x, y, connectionless);
                lock (_lockObject)
                {
                    printerList.Add(portName, newPrinter);
                }
                return newPrinter;
            }
        }

        public void StartPrinterChecking()
        {
            checkingStop = false;
            checkingThread = new Thread(() =>
                {
                    DateTime startTime = DateTime.Now;

                    while (true)
                    {
                        startTime = DateTime.Now;

                        Printer[] printerArray = null;

                        lock (_lockObject)
                        {
                            printerArray = printerList.Values.ToArray();
                        }

                        List<Thread> threadList = new List<Thread>();
                        foreach (Printer printer in printerArray)
                        {
                            if (printer.Connectionless)
                                continue;

                            Thread thread = new Thread((param) =>
                                {
                                    Printer p = param as Printer;
                                    p.checkPrinterStatus();
                                }
                            );
                            thread.Start(printer);
                            threadList.Add(thread);
                        }

                        foreach (Thread thread in threadList)
                        {
                            thread.Join();
                        }

                        checkingStop = true;

                        lock (_checkingCompleteMonitorObject)
                        {
                            try
                            {
                                if (checkingStop)
                                {
                                    return;
                                }
                                else
                                {
                                    DateTime endTime = DateTime.Now;
                                    TimeSpan diff = endTime.Subtract(startTime);
                                    int sleepTime = CHECKING_PERIOD - (int)diff.TotalMilliseconds;
                                    if (sleepTime > 0)
                                        Thread.Sleep(sleepTime);
                                    else
                                        Thread.Sleep(1);
                                }
                            }
                            finally
                            {
                                Monitor.PulseAll(_checkingCompleteMonitorObject);
                            }
                        }
                    }
                }
            );
            checkingThread.Start();
        }

        public void Clear()
        {
            lock (_checkingCompleteMonitorObject)
            {
                checkingStop = true;
            }

            if (checkingThread != null)
                checkingThread.Join();

            checkingThread = null;

            foreach (Printer printer in printerList.Values)
            {
                printer.Dispose();
            }
            printerList.Clear();
        }
    }

    /// <summary>
    /// Printer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PrinterControl : UserControl, IDisposable
    {
        private List<Printer> printerList = new List<Printer>();

        private Thread checkPrinterThread;
        private object threadLock = new object();
        private bool stopThread = false;

        private string _portName = null;
        public string PortName
        {
            get
            {
                return _portName;
            }
        }

        private string _labelType = null;
        public string LabelType
        {
            get
            {
                return _labelType;
            }
        }

        private int _defaultCopies = 1;
        public int DefaultCopies
        {
            get
            {
                return _defaultCopies;
            }
        }

        public PrinterControl(string labelType, int defaultCopies)
        {
            InitializeComponent();
            _labelType = labelType;
            _defaultCopies = defaultCopies;

            checkPrinterThread = new Thread(checkPrinter);
            checkPrinterThread.Start();
        }

        public void checkPrinter()
        {
            //while (true)
            //{
            lock (PrinterManager.Instance.CheckingCompleteMonitorObject)
            {
                Monitor.Wait(PrinterManager.Instance.CheckingCompleteMonitorObject);
            }

            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("Label Type : " + LabelType);
            foreach (Printer printer in printerList)
            {
                sb.Append(" " + printer.PortName + " : " + printer.StatusMessage + " ");
                //sb.AppendLine("\t" + printer.PortName + " : " + printer.StatusMessage);
            }
            string tooltip = sb.ToString();

            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    PrinterStatus currentStatus = PrinterStatus.Error;
                    foreach (Printer printer in printerList)
                    {
                        if (printer.Status == PrinterStatus.Connected)
                        {
                            currentStatus = PrinterStatus.Connected;
                            break;
                        }
                        else if (printer.Status == PrinterStatus.LabelNotTaken)
                        {
                            currentStatus = PrinterStatus.LabelNotTaken;
                        }
                    }

                    if (currentStatus == PrinterStatus.Connected)
                    {
                        imgGreen.Visibility = Visibility.Visible;
                        imgRed.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        imgGreen.Visibility = Visibility.Collapsed;
                        imgRed.Visibility = Visibility.Visible;
                    }

                    ToolTipService.SetToolTip(this, tooltip);
                }));

            lock (threadLock)
            {
                if (stopThread)
                {
                    return;
                }
            }
            //}
        }

        public void Open(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int x, int y, bool connectionless)
        {
            try
            {
                _portName = portName;
                printerList.Add(PrinterManager.Instance.CreateOrGetPrinter(portName, baudRate, parity, dataBits, stopBits, x, y, connectionless));
            }
            catch (Exception ex)
            {
            }
        }

        public bool Barcode_ZPL_Print(string zpl)
        {
            foreach (Printer printer in printerList)
            {
                if (printer.Print(zpl))
                    return true;
            }

            return false;
        }

        public bool PrintZPL(string zpl)
        {
            foreach (Printer printer in printerList)
            {
                if (printer.Print(zpl))
                    return true;
            }

            return false;
        }

        public int PrintZPL(string shopid, string modelsuffix, DataSet ds, int copies)
        {
            int printed = 0;

            //foreach (Printer printer in printerList)
            //{
            //    if (printed >= copies)
            //        break;

            //    string addzpl = "^LH" + printer.X + "," + printer.Y + "^FS";
            //    string zpl = LGIT.GLABEL.ZPL.FuncZPL.GetZPLScriptList(Common.Common.GLABELAccess, shopid, LabelType, modelsuffix, ds, addzpl);

            //    int tryCopies = copies - printed;
            //    for (int copy = 0; copy < tryCopies; copy++)
            //    {
            //        bool result = printer.Print(zpl);
            //        if (result)
            //            printed++;
            //        else
            //            break;
            //    }
            //}

            return printed;
        }

        public void Dispose()
        {
            lock (threadLock)
            {
                stopThread = true;
            }

            checkPrinterThread.Join();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            #region printer test
            //            if (e.ClickCount < 2)
            //                return;

            //            string zpl = @"^XA
            //^LH100,50^FS
            //^SEE:UHANGUL.DAT^FS
            //^CW1,E:KFONT3.FNT^CI26^FS
            //^FO0,0^GB1120,1120,4^FS
            //^FO2,80^GB1116,0,3^FS
            //^FO2,160^GB1116,0,3^FS
            //^FO2,240^GB1116,0,3^FS
            //^FO2,320^GB1116,0,3^FS
            //^FO2,400^GB1116,0,3^FS
            //^FO2,480^GB1116,0,3^FS
            //^FO2,560^GB1116,0,3^FS
            //^FO2,640^GB1116,0,3^FS
            //^FO2,720^GB1116,0,3^FS
            //^FO2,800^GB1116,0,3^FS
            //^FO2,860^GB1116,0,3^FS
            //^FO230,2^GB0,800,3^FS
            //^FO788,2^GB0,80,3^FS
            //^FO558,2^GB0,80,3^FS
            //^FO558,160^GB0,640,3^FS
            //^FO788,160^GB0,640,3^FS
            //^FO18,20^A0N,50,50^FDLOT ID^FS
            //^FO18,100^A0N,50,50^FDPlateNo^FS
            //^FO18,180^A0N,50,50^FDModel^FS
            //^FO18,260^A0N,50,50^FDThickness^FS
            //^FO18,340^A0N,50,50^FDBTIR^FS
            //^FO18,420^A0N,50,50^FDLGD-F1^FS
            //^FO18,500^A0N,50,50^FDUFM-F1^FS
            //^FO18,580^A0N,50,50^FDUFM-F2^FS
            //^FO18,660^A1N,40,40^FD제조사^FS
            //^FO18,740^A1N,40,40^FD현재공정^FS
            //^FO570,20^A1N,35,40^FD자재LOT^FS
            //^FO570,180^A0N,50,50^FDSize X,Y^FS
            //^FO570,260^A0N,50,50^FDFTIR^FS
            //^FO570,340^A0N,50,50^FDTTV^FS
            //^FO570,420^A1N,35,40^FDLGD 판정^FS
            //^FO570,500^A1N,40,35^FDUFM1 판정^FS
            //^FO570,580^A1N,35,40^FDSize X,Y^FS
            //^FO570,660^A1N,35,40^FD출하일자^FS
            //^FO570,740^A1N,35,40^FD출하담당자^FS
            //^FO500,810^A0N,50,50^FDRemark^FS
            //^FO248,20^A0N,50,50^FDN15A001^FS
            //^FO248,180^A0N,50,50^FDQ85120F20L^FS
            //^FO815,15^A0N,50,50^FDPP15A001^FS
            //^XZ";
            //            PrintZPL(zpl);

            //            e.Handled = true;

            //            //DataSet ds = new DataSet();
            //            //DataTable table = ds.Tables.Add("LABELINFO");
            //            //table.Columns.Add("KEYDATA", typeof(string));
            //            //table.Columns.Add("VALUEDATA", typeof(string));
            //            //DataRow row = table.NewRow();
            //            //row["KEYDATA"] = "LOTID";
            //            //row["VALUEDATA"] = "LOTID_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "GOODSNAME";
            //            //row["VALUEDATA"] = "GOODSNAME_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "PLATENO";
            //            //row["VALUEDATA"] = "PLATENO_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "THICKNESS";
            //            //row["VALUEDATA"] = "THICKNESS_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "RECYCLE";
            //            //row["VALUEDATA"] = "RECYCLE_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "SIDETRANS";
            //            //row["VALUEDATA"] = "SIDETRANS_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "TTV";
            //            //row["VALUEDATA"] = "TTV_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "FTIR";
            //            //row["VALUEDATA"] = "FTIR_D";
            //            //table.Rows.Add(row);

            //            //table = ds.Tables.Add("LABELINFO2");
            //            //table.Columns.Add("KEYDATA", typeof(string));
            //            //table.Columns.Add("VALUEDATA", typeof(string));
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "LOTID";
            //            //row["VALUEDATA"] = "LOTID_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "GOODSNAME";
            //            //row["VALUEDATA"] = "GOODSNAME_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "PLATENO";
            //            //row["VALUEDATA"] = "PLATENO_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "THICKNESS";
            //            //row["VALUEDATA"] = "THICKNESS_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "RECYCLE";
            //            //row["VALUEDATA"] = "RECYCLE_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "SIDETRANS";
            //            //row["VALUEDATA"] = "SIDETRANS_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "TTV";
            //            //row["VALUEDATA"] = "TTV_D";
            //            //table.Rows.Add(row);
            //            //row = table.NewRow();
            //            //row["KEYDATA"] = "FTIR";
            //            //row["VALUEDATA"] = "FTIR_D";
            //            //table.Rows.Add(row);

            //            //string zpl = LGIT.GLABEL.ZPL.FuncZPL.GetZPLScriptList("D", "PM1", "PM1_MATERIAL_LABEL_200", "COMMON", ds, "");
            //            //bool b = PrintZPL(zpl);
            #endregion
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess,
        uint dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition,
        uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.
            di.pDocName = "LG CHEM";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }

                    EndDocPrinter(hPrinter);
                }

                ClosePrinter(hPrinter);
            }

            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (!bSuccess)
                dwError = Marshal.GetLastWin32Error();

            return bSuccess;
        }

        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            Int32 dwCount;
            byte[] btASCIIBytes = Encoding.Default.GetBytes(szString);
            dwCount = btASCIIBytes.Length;
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(dwCount);
            Marshal.Copy(btASCIIBytes, 0, pUnmanagedBytes, dwCount);
            SendBytesToPrinter(szPrinterName, pUnmanagedBytes, dwCount);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return true;
        }

        public static bool SendStringToPrinterByUniversalTransformationFormat8(string szPrinterName, string szString)
        {
            Int32 dwCount;
            byte[] btASCIIBytes = Encoding.UTF8.GetBytes(szString);
            dwCount = btASCIIBytes.Length;
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(dwCount);
            Marshal.Copy(btASCIIBytes, 0, pUnmanagedBytes, dwCount);
            SendBytesToPrinter(szPrinterName, pUnmanagedBytes, dwCount);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return true;
        }
    }

    public static class Barcode_ZPL_PrintToLPT
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public static bool Print(string lptport, string sZpl)
        {
            string nl = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
            bool IsConnected = false;

            try
            {
                Byte[] buffer = new byte[sZpl.Length];
                buffer = System.Text.Encoding.ASCII.GetBytes(sZpl);

                SafeFileHandle fh = CreateFile(lptport + ":", FileAccess.Write, 0, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);

                if (!fh.IsInvalid)
                {
                    IsConnected = true;
                    FileStream lpt1 = new FileStream(fh, FileAccess.Write);
                    lpt1.Write(buffer, 0, buffer.Length);

                    lpt1.Close();
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
            }

            return IsConnected;
        }
    }
}