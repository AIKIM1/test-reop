using System;
using System.Collections.Generic;
using System.Data;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.MainFrame.Controls;

namespace LGC.GMES.MES.MainFrame
{
    class FrameOperation : IFrameOperation
    {
        private MainWindow window;
        private MainTabItemLayout tabItemLayout;
        private List<PrinterControl> printerList;
        private List<ScannerControl> scannerList;
        private DataRow drselectprint;

        internal FrameOperation(MainWindow window, MainTabItemLayout tabItemLayout, List<PrinterControl> printerList)
        {
            this.window = window;
            this.tabItemLayout = tabItemLayout;
            this.printerList = printerList;
        }

        internal FrameOperation(MainWindow window, MainTabItemLayout tabItemLayout, List<PrinterControl> printerList, List<ScannerControl> scannerList)
        {
            this.window = window;
            this.tabItemLayout = tabItemLayout;
            this.printerList = printerList;
            this.scannerList = scannerList;
        }
        
        public void PrintMessage(string msg)
        {
            tabItemLayout.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabItemLayout.Message = msg;
            }));
        }

        public void PrintFrameMessage(string msg, bool isUrgent = false)
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.SetMessage(msg, isUrgent);
                //window.SetMessage("[" + DataTableConverter.GetValue(tabItemLayout.DataContext, "MENUNAME") + "] " + msg, isUrgent);
            }));
        }

        public bool CheckScannerState()
        {
            bool hasPrinter = false;
            bool isAllSuccess = true;

            foreach (ScannerControl scanner in scannerList)
            {
                hasPrinter = true;

                if (!scanner.StatusCheck())
                    isAllSuccess = false;
            }
            return hasPrinter && isAllSuccess;
        }

        public bool SendScanData(string data)
        {
            bool hasPrinter = false;
            bool isAllSuccess = true;

            foreach (ScannerControl scanner in scannerList)
            {
                hasPrinter = true;
                
                if (!scanner.Send(data))
                    isAllSuccess = false;

            }
            return hasPrinter && isAllSuccess;
        }

        internal void ReadScanData(string portName, string data)
        {
            if (ReceiveScannerData != null)
                window.Dispatcher.BeginInvoke(new Action(() => ReceiveScannerData(portName, data)));
        }

        public event ReceiveScannerDataHandler ReceiveScannerData;

        public bool Barcode_ZPL_Print(DataRow drselectprint, string zpl)
        {
            this.drselectprint = drselectprint;

            bool hasPrinter = false;
            bool isAllSuccess = true;

            foreach (PrinterControl printer in printerList)
            {
                if (Convert.ToString(this.drselectprint[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME]) == printer.PortName)
                {
                    hasPrinter = true;

                    if (!printer.Barcode_ZPL_Print(zpl))
                        isAllSuccess = false;
                }
            }

            return hasPrinter && isAllSuccess;
        }

        public bool Barcode_ZPL_USB_Print(string zplCode)
        {
            PrintDialog pd = new PrintDialog();

            if (LoginInfo.CFG_SERIAL_PRINT != null && LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
            {
                DataRow[] drInfo = LoginInfo.CFG_SERIAL_PRINT.Select("DEFAULT = 'True'");

                if (drInfo != null && !string.IsNullOrWhiteSpace(drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string))
                    pd.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string);
            }

            PrinterControl.SendStringToPrinter(pd.PrintQueue.FullName, zplCode);

            return true;
        }

        /// <summary>
        /// 설비별 바코드 출력
        /// </summary>
        /// <param name="zplCode">ZPL Code</param>
        /// <param name="equipmentId">설비 ID</param>
        /// <returns></returns>
        public bool PrintUsbBarcodeEquipment(string zplCode, string equipmentId)
        {
            PrintDialog pd = new PrintDialog();

            if (LoginInfo.CFG_SERIAL_PRINT != null && LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
            {
                DataRow[] drInfo = LoginInfo.CFG_SERIAL_PRINT.Select("EQUIPMENT = '" + equipmentId.Trim() + "'");

                if (drInfo != null && !string.IsNullOrWhiteSpace(drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string))
                    pd.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string);
            }

            PrinterControl.SendStringToPrinter(pd.PrintQueue.FullName, zplCode);

            return true;
        }

        public bool PrintUsbBarCodeLabelByUniversalTransformationFormat8(string zplCode)
        {
            try
            {
                PrintDialog pd = new PrintDialog();

                if (LoginInfo.CFG_SERIAL_PRINT != null && LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
                {
                    DataRow[] drInfo = LoginInfo.CFG_SERIAL_PRINT.Select("DEFAULT = 'True'");

                    if (!string.IsNullOrWhiteSpace(drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string))
                        pd.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string);
                }

                PrinterControl.SendStringToPrinterByUniversalTransformationFormat8(pd.PrintQueue.FullName, zplCode);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FrameOperation : 200] PrintUsbBarCodeLabel Error ", "Exception :" + ex);
                return false;
            }
        }

        public bool PrintUsbBarCodeLabel(string zplCode, string labelCode)
        {
            
            try
            {
                PrintDialog pd = new PrintDialog();

                if (LoginInfo.CFG_SERIAL_PRINT != null && LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
                {
                    DataRow[] drInfo = LoginInfo.CFG_SERIAL_PRINT.Select("LABELID = '" + labelCode.Trim() + "'");

                    if (!string.IsNullOrWhiteSpace(drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string))
                        pd.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), (string) drInfo[0][CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME]);
                }

                // 중문 폰트 출력을 위한 Encoding Default -> UTF8
                PrinterControl.SendStringToPrinterByUniversalTransformationFormat8(pd.PrintQueue.FullName, zplCode);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteLine("[FrameOperation : 175] PrintUsbBarCodeLabel Error ", "Exception :" + ex);
                return false;
            }
        }

        public bool Barcode_ZPL_LPT_Print(DataRow drSelectPrint, string zpl)
        {
            this.drselectprint = drSelectPrint;

            bool hasPrinter = false;
            bool isAllSuccess = true;

            foreach (PrinterControl printer in printerList)
            {
                if (Convert.ToString(this.drselectprint[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME]) == printer.PortName)
                {
                    hasPrinter = true;

                    if (!Barcode_ZPL_PrintToLPT.Print(printer.PortName, zpl))
                        isAllSuccess = false;
                }
            }

            return hasPrinter && isAllSuccess;
        }

        //public bool PrintZPL(string labelType, string zpl)
        //{
        //    bool hasPrinter = false;
        //    bool isAllSuccess = true;

        //    foreach (PrinterControl printer in printerList)
        //    {
        //        if (printer.LabelType.Equals(labelType))
        //        {
        //            hasPrinter = true;

        //            if (!printer.PrintZPL(zpl))
        //                isAllSuccess = false;
        //        }
        //    }

        //    return hasPrinter && isAllSuccess;
        //}

        //public bool PrintZPL(string labelType, string shopid, string modelsuffix, DataSet ds, int copies = -1)
        //{
        //    bool hasPrinter = false;
        //    bool isAllSuccess = true;

        //    foreach (PrinterControl printer in printerList)
        //    {
        //        if (printer.LabelType.Equals(labelType))
        //        {
        //            hasPrinter = true;
        //            int targetCopies = copies;

        //            if(targetCopies == -1)
        //                targetCopies = printer.DefaultCopies;

        //            int printed = printer.PrintZPL(shopid, modelsuffix, ds, targetCopies);

        //            if (targetCopies != printed)
        //                isAllSuccess = false;
        //        }
        //    }

        //    return hasPrinter && isAllSuccess;
        //}

        public string MENUID { get; set; }

        public string AUTHORITY { get; set; }

        public bool IsCurrupted { get; set; }

        public object[] Parameters { get; set; }

        public void OpenMenu(string menuID, bool reopen = false, params object[] param)
        {
            window.OpenMenu(menuID, reopen, param);
        }

        public void OpenMenuFORM(string sMenuID, string sFormID, string sNameSpace, string sMenuName, bool reopen = false, params object[] param)
        {
            window.OpenMenuFORM(sMenuID, sFormID, sNameSpace, sMenuName, reopen, param);
        }

        public void OpenDummyMenu(object menuID, bool reopen = false, params object[] param)
        {
            window.OpenDumyMenu(menuID, reopen, param);
        }

        public void ClearFrameMessage()
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.ClearMessage();
            }));
        }

        public void PageFixed(bool isfixed)
        {
            if (isfixed == true)
            {
                window.munMainMenu.IsEnabled = false;
                window.mnMainMenu.IsEnabled = false;
                window.mnMainMenu.megaDropBox.IsOpen = false;
                window.tblAllMenu.IsEnabled = false;
                tabItemLayout.btnClose.IsEnabled = false;

                window.ddFavorite.IsEnabled = false;
                window.tblSetting.IsEnabled = false;
                window.tblHelp.IsEnabled = false;
                window.btnAllMenu.IsEnabled = false;
                window.btnLeftNext.IsEnabled = false;
                window.btnRightNext.IsEnabled = false;

                C1.WPF.C1TabItem selitem = window.tcMainContentTabControl.SelectedItem as C1.WPF.C1TabItem;

                foreach (Button btn in FindVisualChildren<Button>(selitem))
                    if (btn.Name.ToString() == "CloseButton")
                        btn.IsEnabled = false;

                foreach (C1.WPF.C1TabItem tabItem in window.tcMainContentTabControl.Items)
                    if (selitem == tabItem)
                        tabItem.IsEnabled = true;
                    else
                        tabItem.IsEnabled = false;
            }
            else
            {
                window.munMainMenu.IsEnabled = true;
                window.mnMainMenu.IsEnabled = true;
                window.mnMainMenu.megaDropBox.IsOpen = false;
                window.tblAllMenu.IsEnabled = true;
                tabItemLayout.btnClose.IsEnabled = true;

                window.ddFavorite.IsEnabled = true;
                window.tblSetting.IsEnabled = true;
                window.tblHelp.IsEnabled = true;
                window.btnAllMenu.IsEnabled = true;
                window.btnLeftNext.IsEnabled = true;
                window.btnRightNext.IsEnabled = true;

                C1.WPF.C1TabItem selitem = window.tcMainContentTabControl.SelectedItem as C1.WPF.C1TabItem;

                foreach (Button btn in FindVisualChildren<Button>(selitem))
                    if (btn.Name.ToString() == "btnClose")
                        btn.IsEnabled = true;

                foreach (C1.WPF.C1TabItem tabItem in window.tcMainContentTabControl.Items)
                    tabItem.IsEnabled = true;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
}