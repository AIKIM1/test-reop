using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MainFrame.ConfigWindows
{
    /// <summary>
    /// PrinterConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PrinterConfigWindow : UserControl, ICustomConfig
    {
        private bool initialized = false;
        private DataTable initialTable = null;
        private string SelectedShop = string.Empty;
        private string SelectedLabelType = string.Empty;

        public PrinterConfigWindow()
        {
            InitializeComponent();
        }

        public string ConfigName
        {
            get { return "Printer"; }
        }

        public DataTable[] GetCustomConfigs()
        {
            List<DataTable> tableList = new List<DataTable>();

            DataTable scantypeTable = new DataTable(CustomConfig.CONFIGTABLE_SCANTYPE);
            scantypeTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANTYPE_SHOPID, typeof(string));
            scantypeTable.Columns.Add(CustomConfig.CONFIGTABLE_SCANTYPE_LABELTYPE, typeof(string));
            DataRow scantypeRow = scantypeTable.NewRow();
            scantypeRow[CustomConfig.CONFIGTABLE_SCANTYPE_SHOPID] = SelectedShop;
            scantypeRow[CustomConfig.CONFIGTABLE_SCANTYPE_LABELTYPE] = SelectedLabelType;
            scantypeTable.Rows.Add(scantypeRow);
            tableList.Add(scantypeTable);

            if (dgComPort.Rows.Count > 0)
            {
                DataTable serialPrinterTable = DataTableConverter.Convert(dgComPort.GetCurrentItems());
                serialPrinterTable.TableName = CustomConfig.CONFIGTABLE_SERIALPRINTER;
                tableList.Add(serialPrinterTable);
            }

            return tableList.ToArray();
        }

        public void SetCustomConfigs(DataSet configSet)
        {
            if (!initialized)
            {
                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER))
                {
                    initialTable = configSet.Tables[CustomConfig.CONFIGTABLE_SERIALPRINTER];

                    if (!initialTable.Columns.Contains(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS))
                    {
                        initialTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, typeof(bool));

                        foreach (DataRow row in initialTable.Rows)
                            row[CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS] = false;
                    }
                }
                else
                {
                    initialTable = createEmptySerialPrinterTable();
                }

                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_SCANTYPE) && configSet.Tables[CustomConfig.CONFIGTABLE_SCANTYPE].Rows.Count > 0)
                {
                    SelectedShop = configSet.Tables[CustomConfig.CONFIGTABLE_SCANTYPE].Rows[0][CustomConfig.CONFIGTABLE_SCANTYPE_SHOPID].ToString();
                    SelectedLabelType = configSet.Tables[CustomConfig.CONFIGTABLE_SCANTYPE].Rows[0][CustomConfig.CONFIGTABLE_SCANTYPE_LABELTYPE].ToString();
                }
            }

            bool shopChanged = false;

            if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_COMMON) && configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows.Count > 0)
            {
                string currentShop = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_SHOP].ToString();

                if (!SelectedShop.Equals(currentShop))
                {
                    SelectedShop = currentShop;
                    SelectedLabelType = string.Empty;

                    shopChanged = true;
                }
            }

            if (!initialized || shopChanged)
            {
                DataTable labelTypeConditionTable = new DataTable();
                labelTypeConditionTable.Columns.Add("SHOPID", typeof(string));
                DataRow labelTypeCondition = labelTypeConditionTable.NewRow();
                labelTypeCondition["SHOPID"] = SelectedShop;
                labelTypeConditionTable.Rows.Add(labelTypeCondition);

                new ClientProxy().ExecuteService("COR_SEL_SFU_LABEL_TYPE_G", "INDATA", "OUTDATA", labelTypeConditionTable, (labelTypeResult, labelTypeException) =>
                {
                    if (labelTypeException != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(labelTypeException));
                        return;
                    }

                    cboLabelScanType.ItemsSource = DataTableConverter.Convert(labelTypeResult);

                    if (labelTypeResult.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(SelectedLabelType))
                        {
                            cboLabelScanType.SelectedIndex = (from DataRow r in labelTypeResult.Rows select r["LABEL_TYPE"]).ToList().IndexOf(SelectedLabelType);
                        }
                        else
                        {
                            if (cboLabelScanType.SelectedIndex == 0)
                                cboLabelScanType_SelectionChanged(null, null);
                            else
                                cboLabelScanType.SelectedIndex = 0;
                        }
                    }
                });

                initialized = true;
            }
        }

        private DataTable createEmptySerialPrinterTable()
        {
            DataTable emptySerialPrinterTable = new DataTable();
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELTYPE, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_BAUDRATE, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_PARITYBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_DATABIT, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_STOPBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_X, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_Y, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, typeof(int));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT, typeof(string));
            return emptySerialPrinterTable;
        }

        public bool CanSave()
        {
            return true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            dgComPort.BeginningNewRow += new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewSubRow);
            dgComPort.BeginNewRow();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            dgComPort.BeginningNewRow += new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewRow);
            dgComPort.BeginNewRow();
        }

        void dgComPort_BeginningNewRow(object sender, C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs e)
        {
            dgComPort.BeginningNewRow -= new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewRow);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY, Guid.NewGuid().ToString());
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE, true);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_X, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_Y, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, 1);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, false);
        }

        void dgComPort_BeginningNewSubRow(object sender, C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs e)
        {
            dgComPort.BeginningNewRow -= new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewSubRow);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY, Guid.NewGuid().ToString());
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE, false);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY, DataTableConverter.GetValue(dgComPort.SelectedItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY));
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELTYPE, DataTableConverter.GetValue(dgComPort.SelectedItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELTYPE));
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME, string.Empty);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_X, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_Y, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, DataTableConverter.GetValue(dgComPort.SelectedItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES));
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, DataTableConverter.GetValue(dgComPort.SelectedItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS));

            dgComPort.CommittedNewRow += new EventHandler<C1.WPF.DataGrid.DataGridRowEventArgs>(dgComPort_CommittedNewSubRow);

            dgComPort.EndNewRow(true);
        }

        void dgComPort_CommittedNewSubRow(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            dgComPort.CommittedNewRow -= new EventHandler<C1.WPF.DataGrid.DataGridRowEventArgs>(dgComPort_CommittedNewSubRow);

            DataTable originalTable = DataTableConverter.Convert(dgComPort.GetCurrentItems());
            DataRow addedRow = originalTable.Rows[originalTable.Rows.Count - 1];
            DataRow copiedRow = originalTable.NewRow();
            copiedRow.ItemArray = addedRow.ItemArray;

            IEnumerable<DataRow> regacyList = (from DataRow r in originalTable.Rows
                                               where !addedRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY].Equals(r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY])
                                                    && (addedRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY].Equals(r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY])
                                                        || addedRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY].Equals(r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY]))
                                               orderby string.Empty.Equals(r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY]) ? "0" : r[CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY].ToString()
                                               select r);

            originalTable.Rows.InsertAt(copiedRow, originalTable.Rows.IndexOf(regacyList.Last()) + 1);
            originalTable.Rows.Remove(addedRow);

            dgComPort.ItemsSource = DataTableConverter.Convert(originalTable);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgComPort.SelectedItem != null)
                dgComPort.RemoveRow(dgComPort.SelectedIndex);
        }

        private void dgComPort_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            dgComPort.EndNewRow(true);
            dgComPort.EndEditRow(true);

            if (dgComPort.SelectedIndex < 0)
            {
                e.Handled = true;
                dgComPort.ContextMenu.IsOpen = false;
            }
            else
            {
                if (DataTableConverter.GetValue(dgComPort.SelectedItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE).Equals(false))
                {
                    e.Handled = true;
                    dgComPort.ContextMenu.IsOpen = false;
                }
            }
        }

        private void cboLabelScanType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLabelScanType.SelectedIndex < 0)
            {
                SelectedLabelType = string.Empty;

                tbLabelScanType.Text = SelectedLabelType;

                dgComPort.ItemsSource = DataTableConverter.Convert(createEmptySerialPrinterTable());
            }
            else
            {
                tbLabelScanType.Text = DataTableConverter.GetValue(cboLabelScanType.SelectedItem, "LABEL_GROUP").ToString();

                (dgComPort.Columns[3] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = (from object r in cboLabelScanType.ItemsSource select DataTableConverter.GetValue(r, "LABEL_TYPE"));

                if (!SelectedLabelType.Equals(cboLabelScanType.SelectedValue))
                {
                    SelectedLabelType = cboLabelScanType.SelectedValue.ToString();

                    if (dgComPort.ItemsSource == null)
                        dgComPort.ItemsSource = DataTableConverter.Convert(createEmptySerialPrinterTable());
                }
                else
                {
                    dgComPort.ItemsSource = DataTableConverter.Convert(initialTable);
                }
            }
        }

        private void dgComPort_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Equals(colLabelType))
                if (DataTableConverter.GetValue(e.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE).Equals(false))
                    e.Cancel = true;

            if (e.Column.Equals(colCopies))
                if (DataTableConverter.GetValue(e.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE).Equals(false))
                    e.Cancel = true;
        }

        private void dgComPort_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Equals(colCopies))
                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE).Equals(true))
                    foreach (object standbyPrinter in (from r in dgComPort.Rows
                                                       where DataTableConverter.GetValue(r.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY) != null
                                                       && DataTableConverter.GetValue(r.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY).Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY))
                                                       select r.DataItem))
                        DataTableConverter.SetValue(standbyPrinter, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, DataTableConverter.GetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES));
        }
    }
}