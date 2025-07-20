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
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.MainFrame.ConfigWindows
{
    /// <summary>
    /// ValidationConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ValidationConfigWindow : UserControl, ICustomConfig
    {
        private string processValidationTarget = string.Empty;

        private bool initialized = false;

        public ValidationConfigWindow()
        {
            InitializeComponent();
        }

        public string ConfigName
        {
            get { return "Validation"; }
        }

        public DataTable[] GetCustomConfigs()
        {
            DataTable commonValidationTable = DataTableConverter.Convert(dgCommonValidation.GetCurrentItems());
            commonValidationTable.TableName = CustomConfig.CONFIGTABLE_COMMONVALIDATION;

            DataTable processTable = new DataTable(CustomConfig.CONFIGTABLE_PROCESSVALIDATIONTARGET);
            processTable.Columns.Add(CustomConfig.CONFIGTABLE_PROCESSVALIDATIONTARGET_PROCESS, typeof(string));
            DataRow process = processTable.NewRow();
            process[CustomConfig.CONFIGTABLE_PROCESSVALIDATIONTARGET_PROCESS] = processValidationTarget;
            processTable.Rows.Add(process);
            processTable.AcceptChanges();

            DataTable processValidationTable = DataTableConverter.Convert(dgProcessValidation.GetCurrentItems());
            processValidationTable.TableName = CustomConfig.CONFIGTABLE_PROCESSVALIDATION;


            return new DataTable[] { commonValidationTable, processTable, processValidationTable };
        }

        public void SetCustomConfigs(DataSet configSet)
        {
            if (!initialized)
            {
                initialized = true;

                try
                {
                    DataTable commonValidationItemConditionTable = new DataTable();
                    commonValidationItemConditionTable.Columns.Add("LANGID", typeof(string));
                    DataRow commonValidationItemConditionRow = commonValidationItemConditionTable.NewRow();
                    commonValidationItemConditionRow["LANGID"] = LoginInfo.LANGID;
                    commonValidationItemConditionTable.Rows.Add(commonValidationItemConditionRow);

                    new ClientProxy().ExecuteService("COR_SEL_COMMONCODE_CBO_CONFIG", "INDATA", "OUTDATA", commonValidationItemConditionTable, (commonValidationItemResult, commonValidationItemException) =>
                        {
                            try
                            {
                                if (commonValidationItemException != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(commonValidationItemException));
                                    return;
                                }

                                DataTable resultTable = createEmptyCommonValidationTable();
                                foreach (DataRow row in commonValidationItemResult.Rows)
                                {
                                    DataRow resultRow = resultTable.NewRow();
                                    resultRow[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY] = row[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY];
                                    resultRow[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONNAME] = row[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONNAME];
                                    resultRow[CustomConfig.CONFIGTABLE_COMMONVALIDATION_USEYN] = false;
                                    resultTable.Rows.Add(resultRow);
                                }

                                if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_COMMONVALIDATION))
                                {
                                    foreach (DataRow row in configSet.Tables[CustomConfig.CONFIGTABLE_COMMONVALIDATION].Rows)
                                    {
                                        string validationKey = row[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY].ToString();
                                        IEnumerable<DataRow> resultRowList = (from DataRow r in resultTable.Rows where r[CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY].Equals(validationKey) select r);
                                        if (resultRowList.Count() > 0)
                                        {
                                            resultRowList.First()[CustomConfig.CONFIGTABLE_COMMONVALIDATION_USEYN] = row[CustomConfig.CONFIGTABLE_COMMONVALIDATION_USEYN];
                                        }
                                    }
                                }

                                dgCommonValidation.ItemsSource = DataTableConverter.Convert(resultTable);
                            }
                            catch (Exception ex)
                            {
                                dgCommonValidation.ItemsSource = DataTableConverter.Convert(createEmptyCommonValidationTable());
                            }
                        }
                    );
                }
                catch (Exception ex)
                {
                    dgCommonValidation.ItemsSource = DataTableConverter.Convert(createEmptyCommonValidationTable());
                }
            }


            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    string newProcessID = string.Empty;
                    if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_COMMON) && configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows.Count > 0)
                    {
                        newProcessID = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_PROCESS].ToString();
                    }

                    if (!newProcessID.Equals(processValidationTarget))
                    {
                        processValidationTarget = newProcessID;

                        DataTable procIndataTable = new DataTable();
                        procIndataTable.Columns.Add("PROCID", typeof(string));
                        procIndataTable.Columns.Add("LANGID", typeof(string));
                        DataRow procIndata = procIndataTable.NewRow();
                        procIndata["PROCID"] = processValidationTarget;
                        procIndata["LANGID"] = LoginInfo.LANGID;
                        procIndataTable.Rows.Add(procIndata);
                        new ClientProxy().ExecuteService("COR_SEL_PROCESS_TBL", "INDATA", "OUTDATA", procIndataTable, (procResult, procException) =>
                            {
                                if (procException != null)
                                {
                                    tbProcess.Text = string.Empty;
                                }
                                else
                                {
                                    tbProcess.Text = procResult.Rows.Count > 0 ? procResult.Rows[0]["PROCNAME_L"].ToString() : string.Empty;
                                }
                            }
                        );
                    }

                    dgProcessValidation.ItemsSource = DataTableConverter.Convert(createEmptyProcessValidationTable());
                }
            ));
        }

        private static DataTable createEmptyProcessValidationTable()
        {
            DataTable dgProcessValidationTable = new DataTable();
            dgProcessValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_PROCESSVALIDATION_VALIDATIONKEY, typeof(string));
            dgProcessValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_PROCESSVALIDATION_VALIDATIONNAME, typeof(string));
            dgProcessValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_PROCESSVALIDATION_USEYN, typeof(bool));
            return dgProcessValidationTable;
        }

        private DataTable createEmptyCommonValidationTable()
        {
            DataTable dgCommonValidationTable = new DataTable();
            dgCommonValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY, typeof(string));
            dgCommonValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMONVALIDATION_VALIDATIONNAME, typeof(string));
            dgCommonValidationTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMONVALIDATION_USEYN, typeof(bool));
            return dgCommonValidationTable;
        }

        public bool CanSave()
        {
            return true;
        }
    }
}
