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
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.MainFrame.ConfigWindows.Common_sub;
using LGC.GMES.MES.MainFrame.Security;
using System.Windows.Threading;

namespace LGC.GMES.MES.MainFrame.ConfigWindows
{
    /// <summary>
    /// CommonConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommonConfigWindow : UserControl, ICustomConfig
    {
        private string selectedSite = string.Empty;
        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        private string selectedPCSG = null;
        private string selectedEQSG = null;
        private string selectedProcess = null;
        private List<string> selectedOrg = new List<string>();
        private string selectedMenu = string.Empty;

        //private string addTitle = string.Empty;
        //private bool allowResize = false;
        private bool autoLogin = false;
        //private bool msgClear = false;
        //private double interval = 1;

        private bool initialized = false;

        public CommonConfigWindow()
        {
            InitializeComponent();
        }

        public string ConfigName
        {
            get { return "Common"; }
        }

        public DataTable[] GetCustomConfigs()
        {
            DataTable commonConfigTable = new DataTable(CustomConfig.CONFIGTABLE_COMMON);
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_SITE, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_SHOP, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_AREA, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_PCSG, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_EQSG, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_PROCESS, typeof(string));
            commonConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_COMMON_DEFAULTMENU, typeof(string));
            DataRow commonConfig = commonConfigTable.NewRow();
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_SITE] = selectedSite;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_SHOP] = selectedShop;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_AREA] = selectedArea;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_PCSG] = selectedPCSG;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_EQSG] = selectedEQSG;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_PROCESS] = selectedProcess;
            commonConfig[CustomConfig.CONFIGTABLE_COMMON_DEFAULTMENU] = cboMenu.SelectedValue;
            commonConfigTable.Rows.Add(commonConfig);
            commonConfigTable.AcceptChanges();

            DataTable orgConfigTable = new DataTable(CustomConfig.CONFIGTABLE_ORG);
            orgConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_ORG_CODE, typeof(string));
            if (selectedOrg.Count > 0)
            {
                foreach (string orgCode in selectedOrg)
                {
                    DataRow orgConfig = orgConfigTable.NewRow();
                    orgConfig[CustomConfig.CONFIGTABLE_ORG_CODE] = orgCode;
                    orgConfigTable.Rows.Add(orgConfig);
                }
            }
            else
            {
                foreach (string orgCode in mcboOrg.SelectedItems)
                {
                    DataRow orgConfig = orgConfigTable.NewRow();
                    orgConfig[CustomConfig.CONFIGTABLE_ORG_CODE] = orgCode;
                    orgConfigTable.Rows.Add(orgConfig);
                }
            }

            DataTable applicationConfigTable = new DataTable(CustomConfig.CONFIGTABLE_APPLICATION);
            applicationConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_APPLICATION_AUTOLOGIN, typeof(bool));
            DataRow applicationConfig = applicationConfigTable.NewRow();
            applicationConfig[CustomConfig.CONFIGTABLE_APPLICATION_AUTOLOGIN] = chkAutoLogin.IsChecked == true;;
            applicationConfigTable.Rows.Add(applicationConfig);
            applicationConfigTable.AcceptChanges();

            DataTable monitoringConfigTable = new DataTable(CustomConfig.CONFIGTABLE_MONITORING);
            monitoringConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_MONITORING_FULLMONITORING, typeof(bool));
            DataRow monitoringConfig = monitoringConfigTable.NewRow();
            monitoringConfig[CustomConfig.CONFIGTABLE_MONITORING_FULLMONITORING] = chkMonitoring.IsChecked == true;
            monitoringConfigTable.Rows.Add(monitoringConfig);
            monitoringConfigTable.AcceptChanges();

            DataTable loggingConfigTable = new DataTable(CustomConfig.CONFIGTABLE_LOGGING);
            loggingConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_UI, typeof(bool));
            loggingConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_MONITORING, typeof(bool));
            loggingConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_FRAME, typeof(bool));
            loggingConfigTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_BIZRULE, typeof(bool));
            DataRow loggingConfig = loggingConfigTable.NewRow();
            loggingConfig[CustomConfig.CONFIGTABLE_LOGGING_UI] = chkUILog.IsChecked == true;
            loggingConfig[CustomConfig.CONFIGTABLE_LOGGING_MONITORING] = chkMonitoringLog.IsChecked == true;
            loggingConfig[CustomConfig.CONFIGTABLE_LOGGING_FRAME] = chkFrameLog.IsChecked == true;
            loggingConfig[CustomConfig.CONFIGTABLE_LOGGING_BIZRULE] = chkBizRuleLog.IsChecked == true;
            loggingConfigTable.Rows.Add(loggingConfig);
            loggingConfigTable.AcceptChanges();

            return new DataTable[] { commonConfigTable, orgConfigTable, applicationConfigTable, monitoringConfigTable, loggingConfigTable };
        }

        public void SetCustomConfigs(DataSet configSet)
        {
            if (!initialized)
            {
                initialized = true;

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_COMMON) && configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows.Count > 0)
                    {
                        selectedSite = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_SITE].ToString();
                        selectedShop = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_SHOP].ToString();
                        selectedArea = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_AREA].ToString();
                        selectedPCSG = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_PCSG].ToString();
                        selectedEQSG = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_EQSG].ToString();
                        selectedProcess = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_PROCESS].ToString();
                        if (configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Columns.Contains(CustomConfig.CONFIGTABLE_COMMON_DEFAULTMENU))
                            selectedMenu = configSet.Tables[CustomConfig.CONFIGTABLE_COMMON].Rows[0][CustomConfig.CONFIGTABLE_COMMON_DEFAULTMENU].ToString();
                    }

                    selectedOrg.Clear();
                    if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_ORG))
                    {
                        foreach (DataRow orgRow in configSet.Tables[CustomConfig.CONFIGTABLE_ORG].Rows)
                        {
                            selectedOrg.Add(orgRow[CustomConfig.CONFIGTABLE_ORG_CODE].ToString());
                        }
                    }

                    if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_APPLICATION) && configSet.Tables[CustomConfig.CONFIGTABLE_APPLICATION].Rows.Count > 0)
                    {
                        autoLogin = new SecureLoginInfo().AUTOLOGIN;
                    }

                    try
                    {
                        if (configSet.Tables.Contains(CustomConfig.CONFIGTABLE_MONITORING) && configSet.Tables[CustomConfig.CONFIGTABLE_MONITORING].Rows.Count > 0)
                        {
                            chkMonitoring.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_MONITORING].Rows[0][CustomConfig.CONFIGTABLE_MONITORING_FULLMONITORING];
                        }
                        else
                        {
                            chkMonitoring.IsChecked = true;
                        }
                    }
                    catch (Exception chkMonitoringException)
                    {
                        chkMonitoring.IsChecked = true;
                    }

                    try
                    {
                        chkUILog.IsChecked = false;
                        chkMonitoringLog.IsChecked = false;
                        chkFrameLog.IsChecked = false;
                        chkBizRuleLog.IsChecked = false;

                        chkUILog.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_LOGGING].Rows[0][CustomConfig.CONFIGTABLE_LOGGING_UI];
                        chkMonitoringLog.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_LOGGING].Rows[0][CustomConfig.CONFIGTABLE_LOGGING_MONITORING];
                        chkFrameLog.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_LOGGING].Rows[0][CustomConfig.CONFIGTABLE_LOGGING_FRAME];
                        chkBizRuleLog.IsChecked = (bool)configSet.Tables[CustomConfig.CONFIGTABLE_LOGGING].Rows[0][CustomConfig.CONFIGTABLE_LOGGING_BIZRULE];
                    }
                    catch (Exception chkLoggingException)
                    {
                    }

                    refreshOrganization();
                    refreshApplication();
                }));
            }
        }

        public bool CanSave()
        {
            if (string.IsNullOrEmpty(selectedSite)
                || string.IsNullOrEmpty(selectedShop)
                || string.IsNullOrEmpty(selectedArea)
                || selectedPCSG == null
                || selectedEQSG == null
                || selectedProcess == null
                || mcboOrg.SelectedItems.Count == 0
                || cboMenu.SelectedIndex < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void refreshApplication()
        {
            chkAutoLogin.Checked -= new RoutedEventHandler(chkAutoLogin_Checked);

            chkAutoLogin.IsChecked = autoLogin;

            chkAutoLogin.Checked += new RoutedEventHandler(chkAutoLogin_Checked);
        }

        private void refreshOrganization()
        {
            DataTable siteIndataTable = new DataTable();
            siteIndataTable.Columns.Add("LANGID", typeof(string));
            DataRow siteIndata = siteIndataTable.NewRow();
            siteIndata["LANGID"] = LoginInfo.LANGID;
            siteIndataTable.Rows.Add(siteIndata);
            new ClientProxy().ExecuteService("COR_SEL_SITE_CBO", "INDATA", "OUTDATA", siteIndataTable, (siteResult, siteException) =>
            {
                if (siteException != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(siteException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cboSite.ItemsSource = DataTableConverter.Convert(siteResult);

                if ((from DataRow site in siteResult.Rows where site["SITEID"].Equals(selectedSite) select site).Count() > 0)
                    cboSite.SelectedValue = selectedSite;
                else if (siteResult.Rows.Count > 0)
                    cboSite.SelectedIndex = 0;
                else
                    cboSite_SelectionChanged(cboSite, null);
            }
            );

            DataTable pcsgIndataTable = new DataTable();
            pcsgIndataTable.Columns.Add("LANGID", typeof(string));
            pcsgIndataTable.Columns.Add("SUPPLIERID", typeof(string));
            DataRow pcsgIndata = pcsgIndataTable.NewRow();
            pcsgIndata["LANGID"] = LoginInfo.LANGID;
            pcsgIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
            pcsgIndataTable.Rows.Add(pcsgIndata);
            new ClientProxy().ExecuteService("COR_SEL_PROCESSSEGMENT_BY_SUPPLIERID_G", "INDATA", "OUTDATA", pcsgIndataTable, (pcsgResult, pcsgException) =>
            {
                if (pcsgException != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(pcsgException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (pcsgResult.Rows.Count > 0)
                {
                    DataRow row = pcsgResult.NewRow();
                    row["PCSGID"] = string.Empty;
                    row["PCSGNAME"] = "All";

                    pcsgResult.Rows.InsertAt(row, 0);
                }

                cboPCSG.ItemsSource = DataTableConverter.Convert(pcsgResult);

                if ((from DataRow pcsg in pcsgResult.Rows where pcsg["PCSGID"].Equals(selectedPCSG) select pcsg).Count() > 0)
                    cboPCSG.SelectedValue = selectedPCSG;
                else if (pcsgResult.Rows.Count > 0)
                    cboPCSG.SelectedIndex = 0;
                else
                    cboPCSG_SelectionChanged(cboPCSG, null);
            }
            );
        }

        private void cboSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboSite.SelectedIndex > -1)
                    {
                        selectedSite = cboSite.SelectedValue.ToString();

                        DataTable shopIndataTable = new DataTable();
                        shopIndataTable.Columns.Add("LANGID", typeof(string));
                        shopIndataTable.Columns.Add("SUPPLIERID", typeof(string));
                        shopIndataTable.Columns.Add("SITEID", typeof(string));
                        DataRow shopIndata = shopIndataTable.NewRow();
                        shopIndata["LANGID"] = LoginInfo.LANGID;
                        shopIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
                        shopIndata["SITEID"] = cboSite.SelectedValue;
                        shopIndataTable.Rows.Add(shopIndata);
                        new ClientProxy().ExecuteService("COR_SEL_SHOP_BY_SUPPLIERID_G", "INDATA", "OUTDATA", shopIndataTable, (shopResult, shopException) =>
                        {
                            if (shopException != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(shopException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            cboShop.ItemsSource = DataTableConverter.Convert(shopResult);

                            if ((from DataRow shop in shopResult.Rows where shop["SHOPID"].Equals(selectedShop) select shop).Count() > 0)
                                cboShop.SelectedValue = selectedShop;
                            else if (shopResult.Rows.Count > 0)
                                cboShop.SelectedIndex = 0;
                            else
                                cboShop_SelectionChanged(sender, null);
                        }
                        );
                    }
                    else
                    {
                        selectedSite = string.Empty;
                    }
                }));
        }

        private void cboShop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboShop.SelectedIndex > -1)
                    {
                        selectedShop = cboShop.SelectedValue.ToString();

                        DataTable areaIndataTable = new DataTable();
                        areaIndataTable.Columns.Add("LANGID", typeof(string));
                        areaIndataTable.Columns.Add("SUPPLIERID", typeof(string));
                        areaIndataTable.Columns.Add("SHOPID", typeof(string));
                        DataRow areaIndata = areaIndataTable.NewRow();
                        areaIndata["LANGID"] = LoginInfo.LANGID;
                        areaIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
                        areaIndata["SHOPID"] = cboShop.SelectedValue;
                        areaIndataTable.Rows.Add(areaIndata);
                        new ClientProxy().ExecuteService("COR_SEL_AREA_BY_SUPPLIERID_G", "INDATA", "OUTDATA", areaIndataTable, (areaResult, areaException) =>
                        {
                            if (areaException != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(areaException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            cboArea.ItemsSource = DataTableConverter.Convert(areaResult);

                            if ((from DataRow area in areaResult.Rows where area["AREAID"].Equals(selectedArea) select area).Count() > 0)
                                cboArea.SelectedValue = selectedArea;
                            else if (areaResult.Rows.Count > 0)
                                cboArea.SelectedIndex = 0;
                            else
                                cboArea_SelectionChanged(sender, null);
                        }
                        );

                        DataTable orgIndataTable = new DataTable();
                        orgIndataTable.Columns.Add("LANGID", typeof(string));
                        orgIndataTable.Columns.Add("SHOPID", typeof(string));
                        DataRow orgIndata = orgIndataTable.NewRow();
                        orgIndata["LANGID"] = LoginInfo.LANGID;
                        orgIndata["SHOPID"] = selectedShop;
                        orgIndataTable.Rows.Add(orgIndata);
                        new ClientProxy().ExecuteService("COR_SEL_ORG_BY_SHOPID_G", "INDATA", "OUTDATA", orgIndataTable, (orgResult, orgException) =>
                            {
                                if (orgException != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(orgException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                mcboOrg.ItemsSource = DataTableConverter.Convert(orgResult);

                                if (selectedOrg.Count > 0)
                                {
                                    foreach (string orgCode in selectedOrg)
                                    {
                                        mcboOrg.Check(orgCode);
                                    }

                                    selectedOrg.Clear();
                                }
                            }
                        );

                        DataTable defaultMenuTable = new DataTable();
                        defaultMenuTable.Columns.Add("LANGID", typeof(string));
                        defaultMenuTable.Columns.Add("USERID", typeof(string));
                        defaultMenuTable.Columns.Add("PROGRAMTYPE", typeof(string));
                        defaultMenuTable.Columns.Add("SHOPID", typeof(string));
                        DataRow defaultMenu = defaultMenuTable.NewRow();
                        defaultMenu["LANGID"] = LoginInfo.LANGID;
                        defaultMenu["USERID"] = LoginInfo.USERID;
                        defaultMenu["PROGRAMTYPE"] = "SUI";
                        defaultMenu["SHOPID"] = cboShop.SelectedValue.ToString();
                        defaultMenuTable.Rows.Add(defaultMenu);
                        new ClientProxy().ExecuteService("COR_SEL_MENU_BY_CONFIG_G", "INDATA", "OUTDATA", defaultMenuTable, (defaultMenuResult, defaultMenuException) =>
                        {
                            if (defaultMenuException != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(defaultMenuException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            cboMenu.ItemsSource = DataTableConverter.Convert(defaultMenuResult);

                            if (!string.IsNullOrEmpty(selectedMenu) && defaultMenuResult.Rows.Count > 0)
                            {
                                cboMenu.SelectedValue = selectedMenu;
                            }
                            else
                            {
                                cboMenu.SelectedIndex = -1;
                            }
                        }
                        );
                    }
                    else
                    {
                        selectedShop = string.Empty;
                    }
                }
            ));
        }

        private void cboArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboArea.SelectedIndex > -1)
                    {
                        selectedArea = cboArea.SelectedValue.ToString();
                    }
                    else
                    {
                        selectedArea = string.Empty;
                    }

                    cboPCSG_SelectionChanged(sender, e);
                }
            ));
        }

        private void cboPCSG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboPCSG.SelectedIndex > -1)
                    {
                        selectedPCSG = cboPCSG.SelectedValue.ToString();
                    }
                    else
                    {
                        selectedPCSG = null;
                    }

                    if (cboArea.SelectedIndex > -1 && cboPCSG.SelectedIndex > -1)
                    {
                        DataTable eqsgIndataTable = new DataTable();
                        eqsgIndataTable.Columns.Add("LANGID", typeof(string));
                        eqsgIndataTable.Columns.Add("SUPPLIERID", typeof(string));
                        if (cboPCSG.SelectedIndex != 0)
                            eqsgIndataTable.Columns.Add("PCSGID", typeof(string));
                        eqsgIndataTable.Columns.Add("AREAID", typeof(string));
                        DataRow eqsgIndata = eqsgIndataTable.NewRow();
                        eqsgIndata["LANGID"] = LoginInfo.LANGID;
                        eqsgIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
                        if (cboPCSG.SelectedIndex != 0)
                            eqsgIndata["PCSGID"] = cboPCSG.SelectedValue;
                        eqsgIndata["AREAID"] = cboArea.SelectedValue;
                        eqsgIndataTable.Rows.Add(eqsgIndata);
                        new ClientProxy().ExecuteService("COR_SEL_EQUIPMENTSEGMENT_BY_SUPPLIERID_G", "INDATA", "OUTDATA", eqsgIndataTable, (eqsgResult, eqsgException) =>
                        {
                            if (eqsgException != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(eqsgException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            if (eqsgResult.Rows.Count > 0)
                            {
                                DataRow row = eqsgResult.NewRow();
                                row["EQSGID"] = string.Empty;
                                row["EQSGNAME"] = "All";
                                eqsgResult.Rows.InsertAt(row, 0);
                            }

                            cboEQSG.ItemsSource = DataTableConverter.Convert(eqsgResult);

                            if ((from DataRow eqsg in eqsgResult.Rows where eqsg["EQSGID"].Equals(selectedEQSG) select eqsg).Count() > 0)
                                cboEQSG.SelectedValue = selectedEQSG;
                            else if (eqsgResult.Rows.Count > 0)
                                cboEQSG.SelectedIndex = 0;
                            else
                                cboEQSG_SelectionChanged(sender, e);
                        }
                        );
                    }
                }
            ));
        }

        private void cboEQSG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboEQSG.SelectedIndex > -1)
                    {
                        selectedEQSG = cboEQSG.SelectedValue.ToString();
                    }
                    else
                    {
                        selectedEQSG = null;
                    }

                    if (cboPCSG.SelectedIndex > -1 && cboEQSG.SelectedIndex > -1)
                    {
                        DataTable processIndataTable = new DataTable();
                        processIndataTable.Columns.Add("LANGID", typeof(string));
                        processIndataTable.Columns.Add("SUPPLIERID", typeof(string));
                        if (cboPCSG.SelectedIndex != 0)
                            processIndataTable.Columns.Add("PCSGID", typeof(string));
                        if (cboEQSG.SelectedIndex != 0)
                            processIndataTable.Columns.Add("EQSGID", typeof(string));
                        DataRow processIndata = processIndataTable.NewRow();
                        processIndata["LANGID"] = LoginInfo.LANGID;
                        processIndata["SUPPLIERID"] = string.IsNullOrEmpty(LoginInfo.SUPPLIERID.ToString()) ? null : LoginInfo.SUPPLIERID;
                        if (cboPCSG.SelectedIndex != 0)
                            processIndata["PCSGID"] = cboPCSG.SelectedValue;
                        if (cboEQSG.SelectedIndex != 0)
                            processIndata["EQSGID"] = cboEQSG.SelectedValue;
                        processIndataTable.Rows.Add(processIndata);
                        new ClientProxy().ExecuteService("COR_SEL_PROCESS_BY_SUPPLIERID_G", "INDATA", "OUTDATA", processIndataTable, (processResult, processException) =>
                        {
                            if (processException != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(processException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            if (processResult.Rows.Count > 0)
                            {
                                DataRow row = processResult.NewRow();
                                row["PROCID"] = string.Empty;
                                row["PROCNAME"] = "All";
                                processResult.Rows.InsertAt(row, 0);
                            }

                            cboProcess.ItemsSource = DataTableConverter.Convert(processResult);

                            if ((from DataRow process in processResult.Rows where process["PROCID"].Equals(selectedProcess) select process).Count() > 0)
                                cboProcess.SelectedValue = selectedProcess;
                            else if (processResult.Rows.Count > 0)
                                cboProcess.SelectedIndex = 0;
                            else
                                cboProcess_SelectionChanged(sender, e);
                        }
                        );
                    }
                    else
                    {
                        cboProcess.ItemsSource = new object[] { };
                    }
                }
            ));
        }

        private void cboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (cboProcess.SelectedIndex > -1)
                    {
                        selectedProcess = cboProcess.SelectedValue.ToString();
                    }
                    else
                    {
                        selectedProcess = null;
                    }
                }
            ));
        }

        private void chkAutoLogin_Checked(object sender, RoutedEventArgs e)
        {
            UserCheck userCheck = new UserCheck();
            userCheck.Closed += (s, arg) =>
                {
                    if (userCheck.DialogResult != true)
                    {
                        chkAutoLogin.IsChecked = false;
                    }
                };
            userCheck.ShowDialog();
        }
    }
}
