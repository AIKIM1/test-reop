using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MainFrame.Controls
{
    /// <summary>
    /// ScannerControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScannerControl : UserControl, IDisposable
    {
        private SerialPort _serialPort;
        private static readonly string SCANNER_STATUS_CONNECTED = "Connected";
        private static readonly string SCANNER_STATUS_DISCONNECTED = "Can not connect to this printer";

        private ReceiveScannerDataHandler dataReceivedHandler;

        public enum ScannerStatus { Error, Connected};

        public ScannerStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public ScannerControl()
        {
            InitializeComponent();
        }

        public string PortName
        {
            get
            {
                if (_serialPort == null)
                    return string.Empty;

                return _serialPort.PortName;
            }
        }

        public void Open(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, ReceiveScannerDataHandler dataReceivedHandler)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.dataReceivedHandler = dataReceivedHandler;

                    try
                    {
                        _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                        _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                        _serialPort.Open();

                        Status = ScannerStatus.Connected;
                        StatusMessage = SCANNER_STATUS_CONNECTED;
                    }
                    catch (Exception ex)
                    {
                        Status = ScannerStatus.Error;
                        StatusMessage = SCANNER_STATUS_DISCONNECTED;
                    }
                }));
        }
        internal void sendToSerialPort(string text)
        {
            byte[] data = System.Text.ASCIIEncoding.Default.GetBytes(text);
            _serialPort.Write(data, 0, data.Length);
        }

        public bool Send(string text)
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();

            if (_serialPort.IsOpen)
            {
                sendToSerialPort(text);
                
                return true;
            }
            else
            {
                _serialPort.Close();              
                return false;
            }
        }

        public bool StatusCheck()
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return false;

            return true;
        }


        const char STX = (char)0x02;
        const char LF = (char)0x0A;

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);

            //bool bComplete = false;
            //int iTryCount = 0;

            //string message = string.Empty;

            //do
            //{
            string value = _serialPort.ReadExisting();
            //    message += value;
            //    if (value.IndexOf(LF) > 0)
            //    {
            //        if (message[0] == STX)
            //        {
            //            bComplete = true;
            //        }
            //    }

            //    iTryCount += 1;
            //    System.Threading.Thread.Sleep(10);
            //}
            //while (!bComplete || iTryCount > 10);
            
            if (dataReceivedHandler != null)
                dataReceivedHandler(_serialPort.PortName, value);
        }

        public void Dispose()
        {
            if (_serialPort != null)
                _serialPort.Close();
        }
    }
}
