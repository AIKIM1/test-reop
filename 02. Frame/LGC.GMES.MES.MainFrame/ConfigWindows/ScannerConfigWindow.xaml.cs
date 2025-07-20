using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainFrame.ConfigWindows
{
    /// <summary>
    /// ScannerConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScannerConfigWindow : UserControl, ICustomConfig
    {
        private bool initialized = false;

        public ScannerConfigWindow()
        {
            InitializeComponent();
        }

        public string ConfigName
        {
            get { return "Scanner"; }
        }

        public DataTable[] GetCustomConfigs()
        {
            //dgComPort.EndNewRow(true);
            //dgComPort.EndEditRow(true);

            DataTable scanItemTable = DataTableConverter.Convert(dgScanItem.GetCurrentItems());
            scanItemTable.TableName = CustomConfig.CONFIGTABLE_SCANITEM;

            DataTable soundTable = new DataTable(CustomConfig.CONFIGTABLE_SOUND);
            soundTable.Columns.Add(CustomConfig.CONFIGTABLE_SOUND_USEYN, typeof(bool));
            soundTable.Columns.Add(CustomConfig.CONFIGTABLE_SOUND_OK, typeof(string));
            soundTable.Columns.Add(CustomConfig.CONFIGTABLE_SOUND_NG, typeof(string));
            DataRow sound = soundTable.NewRow();
            sound[CustomConfig.CONFIGTABLE_SOUND_USEYN] = chkUseSound.IsChecked.Value;
            sound[CustomConfig.CONFIGTABLE_SOUND_OK] = fpOKFile.SelectedFile != null ? fpOKFile.SelectedFile.FullName : string.Empty;
            sound[CustomConfig.CONFIGTABLE_SOUND_NG] = fpNGFile.SelectedFile != null ? fpNGFile.SelectedFile.FullName : string.Empty;
            soundTable.Rows.Add(sound);
            soundTable.AcceptChanges();

            DataTable serialPortTable = DataTableConverter.Convert(dgComPort.GetCurrentItems());
            serialPortTable.TableName = CustomConfig.CONFIGTABLE_SERIALPORT;

            return new DataTable[] { scanItemTable, soundTable, serialPortTable };
        }

        public void SetCustomConfigs(DataSet configSet)
        {
            if (!initialized)
            {
                initialized = true;

                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_SCANITEM))
                {
                    dgScanItem.ItemsSource = DataTableConverter.Convert(configSet.Tables[CustomConfig.CONFIGTABLE_SCANITEM]);
                }
                else
                {
                    DataTable emptyScanItemTable = new DataTable();
                    emptyScanItemTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANITEM_PARTKEY, typeof(string));
                    emptyScanItemTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANITEM_PARTNAME, typeof(string));
                    emptyScanItemTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANITEM_VALIDATION, typeof(bool));
                    emptyScanItemTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANITEM_USEYN, typeof(bool));

                    dgScanItem.ItemsSource = DataTableConverter.Convert(emptyScanItemTable);
                }

                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_SOUND) && configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows.Count > 0)
                {
                    chkUseSound.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows[0][CustomConfig.CONFIGTABLE_SOUND_USEYN];

                    if (!string.IsNullOrEmpty(configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows[0][CustomConfig.CONFIGTABLE_SOUND_OK].ToString()))
                        fpOKFile.SelectedFile = new FileInfo(configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows[0][CustomConfig.CONFIGTABLE_SOUND_OK].ToString());

                    if (!string.IsNullOrEmpty(configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows[0][CustomConfig.CONFIGTABLE_SOUND_NG].ToString()))
                        fpNGFile.SelectedFile = new FileInfo(configSet.Tables[CustomConfig.CONFIGTABLE_SOUND].Rows[0][CustomConfig.CONFIGTABLE_SOUND_NG].ToString());
                }

                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_SERIALPORT) && configSet.Tables[CustomConfig.CONFIGTABLE_SERIALPORT].Rows.Count > 0)
                {
                    dgComPort.ItemsSource = DataTableConverter.Convert(configSet.Tables[CustomConfig.CONFIGTABLE_SERIALPORT]);
                }
                else
                {
                    DataTable emptySerialPortTable = new DataTable();
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME, typeof(string));
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_BAUDRATE, typeof(int));
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_PARITYBIT, typeof(string));
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_DATABIT, typeof(int));
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_STOPBIT, typeof(string));
                    emptySerialPortTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPORT_EQUIPMENT_SEGMENT, typeof(string));

                    dgComPort.ItemsSource = DataTableConverter.Convert(emptySerialPortTable);
                }
            }
        }

        public bool CanSave()
        {
            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            dgComPort.BeginNewRow();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgComPort.SelectedItem != null)
                dgComPort.RemoveRow(dgComPort.SelectedIndex);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    class PortNames : List<string>
    {
        public PortNames() : base(SerialPort.GetPortNames())
        {
            if (CustomConfig.Instance.ConfigSet.Tables.Contains(CustomConfig.CONFIGTABLE_SERIALPORT))
                foreach (DataRow row in CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_SERIALPORT].Rows)
                    if (!base.Contains(row[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME].ToString()))
                        base.Add(row[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME].ToString());
        }
    }

    class BaudRates : List<int>
    {
        public BaudRates() : base()
        {
            base.Add(115200);
            base.Add(57600);
            base.Add(38400);
            base.Add(19200);
            base.Add(9600);
            base.Add(4800);
            base.Add(2400);
        }
    }

    class ParityBits : List<string>
    {
        public ParityBits() : base()
        {
            foreach (Parity parity in Enum.GetValues(typeof(Parity)))
                base.Add(parity.ToString());
        }
    }

    class DataBits : List<int>
    {
        public DataBits() : base()
        {
            base.Add(8);
        }
    }

    class StopBits : List<string>
    {
        public StopBits() : base()
        {
            foreach (System.IO.Ports.StopBits stopBits in Enum.GetValues(typeof(System.IO.Ports.StopBits)))
                base.Add(stopBits.ToString());
        }
    }

    class DPIs : List<int>
    {
        public DPIs() : base()
        {
            base.Add(203);
            base.Add(300);
            base.Add(600);
        }
    }

    class Darknesses : List<int>
    {
        public Darknesses() : base()
        {
            base.Add(0);
            base.Add(1);
            base.Add(2);
            base.Add(3);
            base.Add(4);
            base.Add(5);
            base.Add(6);
            base.Add(7);
            base.Add(8);
            base.Add(9);
            base.Add(10);
            base.Add(11);
            base.Add(12);
            base.Add(13);
            base.Add(14);
            base.Add(15);
            base.Add(16);
            base.Add(17);
            base.Add(18);
            base.Add(19);
            base.Add(20);
            base.Add(21);
            base.Add(22);
            base.Add(23);
            base.Add(24);
            base.Add(25);
            base.Add(26);
            base.Add(27);
            base.Add(28);
            base.Add(29);
            base.Add(30);
        }
    }

    class EquipmentSegments : List<string>
    {
        public EquipmentSegments() : base()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                //cbo.ItemsSource = DataTableConverter.Convert(result);

                //if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                //    cbo.SelectedValue = selectedEquipmentSegmant;
                //else if (result.Rows.Count > 0)
                //    cbo.SelectedIndex = 0;
                //else if (result.Rows.Count == 0)
                //    cbo.SelectedItem = null;

                foreach (DataRow dr in result.Rows)
                    base.Add(dr["CBO_CODE"] as string);
            });
        }
    }
}