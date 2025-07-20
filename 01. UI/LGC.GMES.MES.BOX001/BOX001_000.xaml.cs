/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Reflection;

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System.Management;

namespace LGC.GMES.MES.BOX001
{

    /// <summary>
    /// BOX001_000.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class BOX001_000 : UserControl, IWorkArea
    {
        #region 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_000()
        {
            InitializeComponent();
        }

        #endregion
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");
            //var portQuery = new ManagementObjectSearcher("Select * from Win32_TCPIPPrinterPort");

            //foreach (var printer in printerQuery.Get())
            //{
            //    txtNote.AppendText(printer.GetPropertyValue("Attributes").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Caption").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Default").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("DefaultPriority").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("DeviceID").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Direct").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("DoCompleteFirst").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("DriverName").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("EnableBIDI").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("EnableDevQueryPrint").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Hidden").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Local").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Name").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Network").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("PortName").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("PrinterState").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("PrinterStatus").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("Status").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(printer.GetPropertyValue("SystemName").ToString());
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(System.Environment.NewLine);
            //    txtNote.AppendText(System.Environment.NewLine);
                //var status = printer.GetPropertyValue("Status");
                //var isDefault = printer.GetPropertyValue("Default");
                //var isNetworkPrinter = printer.GetPropertyValue("Network");

                //DataRow newRow = dtResult.NewRow();
                //newRow.ItemArray = new object[] { name, name };
                //dtResult.Rows.Add(newRow);
            //}

            //cboPrinter.ItemsSource = DataTableConverter.Convert(dtResult);



            // rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {


        }

        private void btnLazer_Click(object sender, RoutedEventArgs e)
        {
            Util.LazerPlayer();

        }

        private void btnWarning_Click(object sender, RoutedEventArgs e)
        {
            Util.WarningPlayer();
        }

        private void btnDingdong_Click(object sender, RoutedEventArgs e)
        {
            Util.DingPlayer();
        }
    }
}
