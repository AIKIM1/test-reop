/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.06.27   권용섭    : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림
  2024.08.26   최성필    : FDS 발열셀/ SAS 송수신 오류 알림 추가
  2025.04.22   이지은    : 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부 소형 리빌딩 반영건
**************************************************************************************/
#region Import Library
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
#endregion

namespace LGC.GMES.MES.MainFrame
{
    /// <summary>
    /// ConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConfigWindow : C1Window
    {
        #region Declaration & Constructor
        string selectedShop = string.Empty;
        string selectedArea = string.Empty;
        string selectedEquipmentSegmant = null;
        string selectedProcess = null;
        string selectedEquipment = null;
        string labelType = string.Empty;
        int labelCopies = 1;
        string cutLabel = string.Empty;
        string labelAuto = string.Empty;
        int cardCopies = 1;
        string cardAuto = string.Empty;
        string cardPopup = string.Empty;
        int thermalCopies = 2;
        //C20210415-000402 대LOT별 첫번째 컷 바코드 라벨 수량
        int firstCutCopies = 0;

        DataTable initialTable = null;

        public ConfigWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ConfigWindow_Loaded);
        }

        void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        #endregion

        #region Initialize
        public void Initialize()
        {
            try
            {
                CustomConfig.Instance.Reload(ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"]);

                selectedShop = LoginInfo.CFG_SHOP_ID;
                selectedArea = LoginInfo.CFG_AREA_ID;
                selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
                selectedProcess = LoginInfo.CFG_PROC_ID;
                selectedEquipment = LoginInfo.CFG_EQPT_ID;

                labelType = LoginInfo.CFG_LABEL_TYPE;
                labelCopies = LoginInfo.CFG_LABEL_COPIES;
                cutLabel = LoginInfo.CFG_CUT_LABEL;
                labelAuto = LoginInfo.CFG_LABEL_AUTO;
                cardCopies = LoginInfo.CFG_CARD_COPIES;
                cardAuto = LoginInfo.CFG_CARD_AUTO;
                cardPopup = LoginInfo.CFG_CARD_POPUP;
                //C20210415-000402 대LOT별 첫번째 컷 바코드 라벨 수량
                firstCutCopies = LoginInfo.CFG_LABEL_FIRST_CUT_COPIES;


                thermalCopies = LoginInfo.CFG_THERMAL_COPIES;

                Set_Combo_LabelType(cboLabelType);
                numLabelCopies.Value = labelCopies;
                Set_Combo_CutLabel(cboCutLabel);
                Set_Combo_LableAuto(cboLabelAuto);
                numCardCopies.Value = cardCopies;
                Set_Combo_CardAuto(cboCardAuto);
                Set_Combo_CardPopup(cboCardPopup);
                numThermalCopies.Value = thermalCopies;
                numFirstCutCopies.Value = firstCutCopies;

                Set_Combo_Shop(cboShop);
                Set_Combo_Lang(cboLang);
                Set_Combo_DefaultMenu(cboDefaultMenu);
                Set_Combo_Wipstat(cboWipstat);
                SetNumericCombo(cboSampleCopies, 1, 5);
                SetNumericCombo(cboHistYScale, 1, 100);
                SetPaperSizeCombo(cboPaperSize);

                SetLocalPrinterComboBox(cboGeneralPrinter);
                SetLocalPrinterComboBox(cboThermalPrinter);

                dgBarcodePrinter.ItemsSource = DataTableConverter.Convert(LoginInfo.CFG_SERIAL_PRINT);

                if (LoginInfo.CFG_THERMAL_PRINT.Rows.Count > 0)
                {
                    numLeftMargin.Value = Convert.ToInt32(LoginInfo.CFG_THERMAL_PRINT.Rows[0]["X"]);
                    numTopMargin.Value = Convert.ToInt32(LoginInfo.CFG_THERMAL_PRINT.Rows[0]["Y"]);
                }

                chkUILog.IsChecked = Convert.ToBoolean(LoginInfo.CFG_LOGGING.Rows[0][CustomConfig.CONFIGTABLE_LOGGING_UI]);
                chkFrameLog.IsChecked = Convert.ToBoolean(LoginInfo.CFG_LOGGING.Rows[0][CustomConfig.CONFIGTABLE_LOGGING_FRAME]);
                chkBizRuleLog.IsChecked = Convert.ToBoolean(LoginInfo.CFG_LOGGING.Rows[0][CustomConfig.CONFIGTABLE_LOGGING_BIZRULE]);

                // 공지사항팝업 자동 여부 추가 [2018-01-19]
                chkNoticeUseType.IsChecked = (LoginInfo.CFG_ETC.Rows.Count > 0 && LoginInfo.CFG_ETC.Columns["NOTICE"] != null) ? Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_NOTICEUSE]) : false;

                chkSmallType.IsChecked = LoginInfo.CFG_ETC.Rows.Count > 0 ? Convert.ToBoolean(LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_SMALLTYPE]) : false;

                //Set_Combo_LabelType(cboLabelType);
                //Set_Combo_LableAuto(cboLabelAuto);
                //Set_Combo_CardAuto(cboCardAuto);
                //Set_Combo_CardPopup(cboCardPopup);

                C1.WPF.DataGrid.DataGridComboBoxColumn portTypeColumn = dgBarcodePrinter.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (portTypeColumn != null)
                    portTypeColumn.ItemsSource = DataTableConverter.Convert(dtLocalPort());

                C1.WPF.DataGrid.DataGridComboBoxColumn localPrinterColumn = dgBarcodePrinter.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (localPrinterColumn != null)
                    localPrinterColumn.ItemsSource = DataTableConverter.Convert(dtLocalPrinter());

                C1.WPF.DataGrid.DataGridComboBoxColumn zplTypeColumn = dgBarcodePrinter.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (zplTypeColumn != null)
                    zplTypeColumn.ItemsSource = DataTableConverter.Convert(dtZplType());

                C1.WPF.DataGrid.DataGridComboBoxColumn equipmentColumn = dgBarcodePrinter.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (equipmentColumn != null)
                    equipmentColumn.ItemsSource = DataTableConverter.Convert(dtEquipment());

                C1.WPF.DataGrid.DataGridComboBoxColumn labelInfoColumn = dgBarcodePrinter.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID] as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (labelInfoColumn != null)
                    labelInfoColumn.ItemsSource = DataTableConverter.Convert(GetLabelInfo());

                //활성화 알람 관련 추가 2021-03-08
                chkEqpStatus.IsChecked = LoginInfo.CFG_EQP_STATUS;
                chkWLot.IsChecked = LoginInfo.CFG_W_LOT;
                //2023.01.17 : 공정대기시간초과, aging시간초과 팝업 flag 추가 - leeyj
                chkFormProcWaitLimitTimeOver.IsChecked = LoginInfo.CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER;
                chkFormAgingLimitTimeOver.IsChecked = LoginInfo.CFG_FORM_AGING_LIMIT_TIME_OVER;
                chkFormAgingOutputTimeOver.IsChecked = LoginInfo.CFG_FORM_AGING_OUTPUT_TIME_OVER; // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
                //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                chkFormFittedDOCVTrnfFail.IsChecked = LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL;
                //2024.08.20 / 최성필(cso59463) / FDS 발열셀 알람 팝업 사용 여부
                chkFormFDSAlarm.IsChecked = LoginInfo.CFG_FORM_FDS_ALARM;
                //2024.08.26 / 최성필(cso59463) / SAS 송수신 오류 알람 팝업 사용 여부
                chkFormSASAlarm.IsChecked = LoginInfo.CFG_FORM_SAS_ALARM;

                //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부
                chkFormHighAgingAbnormTmprAlarm.IsChecked = LoginInfo.CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("SYSTEM_ID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SYSTEM_CHK", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count > 0)
            {
                LoginInfo.CFG_SYSTEM_TYPE_CODE = dtResult.Rows[0]["SYSTEM_TYPE_CODE"].ToString();

                //활성화는 라인,공정 선택하지 않음. - 2020.11.13 
                if (dtResult.Rows[0]["SYSTEM_TYPE_CODE"].ToString().Equals("F"))
                {
                    if (!Check_Mandatory(new List<DependencyObject> { cboShop, cboArea }))
                        return;
                }
                else
                {
                    if (!Check_Mandatory(new List<DependencyObject> { cboShop, cboArea, cboEquipmentSegment, cboProcess }))
                        return;
                }
            }
            else
            {
                if (!Check_Mandatory(new List<DependencyObject> { cboShop, cboArea, cboEquipmentSegment, cboProcess }))
                    return;
            }

            //if (!Check_Mandatory(new List<DependencyObject> { cboShop, cboArea, cboEquipmentSegment, cboProcess }))
            //    return;

            //if (Convert.ToString(cboArea.SelectedValue).Substring(0, 1) == "P")
            //{
            //    //Pack 공장은 설비 등록이 필수 아님
            //    if (Check_Mandatory(new List<DependencyObject> { cboShop, cboArea, cboEquipmentSegmant, cboProcess }) == false)
            //    {
            //        return;
            //    }
            //}
            //else
            //{
            //    if (Check_Mandatory(new List<DependencyObject> { cboShop, cboArea, cboEquipmentSegmant, cboProcess, cboEquipment }) == false)
            //    {
            //        return;
            //    }
            //}

            LoginInfo.CFG_SHOP_ID = Convert.ToString(cboShop.SelectedValue);
            LoginInfo.CFG_AREA_ID = Convert.ToString(cboArea.SelectedValue);
            LoginInfo.CFG_EQSG_ID = Convert.ToString(cboEquipmentSegment.SelectedValue);
            LoginInfo.CFG_PROC_ID = Convert.ToString(cboProcess.SelectedValue);
            LoginInfo.CFG_EQPT_ID = Convert.ToString(cboEquipment.SelectedValue);

            LoginInfo.CFG_SHOP_NAME = Convert.ToString(DataTableConverter.GetValue(cboShop.SelectedItem, "CBO_NAME"));
            LoginInfo.CFG_AREA_NAME = Convert.ToString(DataTableConverter.GetValue(cboArea.SelectedItem, "CBO_NAME"));
            LoginInfo.CFG_EQSG_NAME = Convert.ToString(DataTableConverter.GetValue(cboEquipmentSegment.SelectedItem, "CBO_NAME"));
            LoginInfo.CFG_PROC_NAME = Convert.ToString(DataTableConverter.GetValue(cboProcess.SelectedItem, "CBO_NAME"));
            LoginInfo.CFG_EQPT_NAME = Convert.ToString(DataTableConverter.GetValue(cboEquipment.SelectedItem, "CBO_NAME"));

            LoginInfo.CFG_LABEL_TYPE = Convert.ToString(cboLabelType.SelectedValue);
            LoginInfo.CFG_LABEL_COPIES = Convert.ToInt32(numLabelCopies.Value);
            LoginInfo.CFG_CUT_LABEL = Convert.ToString(cboCutLabel.SelectedValue);
            LoginInfo.CFG_LABEL_AUTO = cboLabelAuto.SelectedValue as string;
            LoginInfo.CFG_CARD_COPIES = Convert.ToInt32(numCardCopies.Value);
            LoginInfo.CFG_CARD_AUTO = Convert.ToString(cboCardAuto.SelectedValue);
            LoginInfo.CFG_CARD_POPUP = Convert.ToString(cboCardPopup.SelectedValue);
            LoginInfo.CFG_LABEL_FIRST_CUT_COPIES = Convert.ToInt32(numFirstCutCopies.Value); 

            LoginInfo.CFG_THERMAL_COPIES = Convert.ToInt32(numThermalCopies.Value);

            LoginInfo.LANGID = Convert.ToString(cboLang.SelectedValue);
            LoginInfo.CFG_INI_MENUID = Convert.ToString(cboDefaultMenu.SelectedValue);

            #region 활성화 알림 팝업

            //활성화 알림 팝업
            if ((bool)chkEqpStatus.IsChecked) LoginInfo.CFG_EQP_STATUS = true;
            else LoginInfo.CFG_EQP_STATUS = false;

            if ((bool)chkWLot.IsChecked) LoginInfo.CFG_W_LOT = true;
            else LoginInfo.CFG_W_LOT = false;

            //2023.01.17 : 공정 대기 시간 초과 팝업 관련 Setting Flag 추가 - leeyj
            if ((bool)chkFormProcWaitLimitTimeOver.IsChecked) LoginInfo.CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER = true;
            else LoginInfo.CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER = false;

            //2023.01.17 : Aging 시간 초과 팝업 관련 Setting Flag 추가 - leeyj
            if ((bool)chkFormAgingLimitTimeOver.IsChecked) LoginInfo.CFG_FORM_AGING_LIMIT_TIME_OVER = true;
            else LoginInfo.CFG_FORM_AGING_LIMIT_TIME_OVER = false;

            // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
            if ((bool)chkFormAgingOutputTimeOver.IsChecked) LoginInfo.CFG_FORM_AGING_OUTPUT_TIME_OVER = true;
            else LoginInfo.CFG_FORM_AGING_OUTPUT_TIME_OVER = false;

            //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
            if ((bool)chkFormFittedDOCVTrnfFail.IsChecked) LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL = true;
            else LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL = false;

            //2024.08.20 / 최성필(cso59463) / FDS 발열셀 알람 팝업 사용 여부
            if ((bool)chkFormFDSAlarm.IsChecked) LoginInfo.CFG_FORM_FDS_ALARM = true;
            else LoginInfo.CFG_FORM_FDS_ALARM = false;
            //2024.08.26 / 최성필(cso59463) / SAS 송수신 알람 팝업 사용 여부
            if ((bool)chkFormSASAlarm.IsChecked) LoginInfo.CFG_FORM_SAS_ALARM = true;
            else LoginInfo.CFG_FORM_SAS_ALARM = false;

            //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부
            if ((bool)chkFormHighAgingAbnormTmprAlarm.IsChecked) LoginInfo.CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM = true;
            else LoginInfo.CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM = false;

            #endregion 활성화 알림 팝업

            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("USERID", typeof(string));
            dtRQSTDT.Columns.Add("SYSTEM_TYPE_CODE", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));
            dtRQSTDT.Columns.Add("EQPTID", typeof(string));
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("INI_MENUID", typeof(string));
            dtRQSTDT.Columns.Add("INSUSER", typeof(string));
            dtRQSTDT.Columns.Add("USEFLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["SYSTEM_TYPE_CODE"] = LGC.GMES.MES.Common.LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
            drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            drnewrow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            drnewrow["PROCID"] = LoginInfo.CFG_PROC_ID;
            drnewrow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["INI_MENUID"] = LoginInfo.CFG_INI_MENUID;
            drnewrow["INSUSER"] = LoginInfo.USERID;
            drnewrow["USEFLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("BR_REG_TB_SOM_USER_SCRN_SET_MST", "RQSTDT", null, dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoginInfo.CFG_GENERAL_PRINTER = CreateEmptyGeneralPrinterTable();
                DataRow drGeneral = LoginInfo.CFG_GENERAL_PRINTER.NewRow();
                drGeneral[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME] = cboGeneralPrinter.SelectedValue;
                LoginInfo.CFG_GENERAL_PRINTER.Rows.Add(drGeneral);

                LoginInfo.CFG_SERIAL_PRINT = DataTableConverter.Convert(dgBarcodePrinter.ItemsSource);

                LoginInfo.CFG_THERMAL_PRINT = CreateEmptyThermalPrintTable();
                DataRow drThermal = LoginInfo.CFG_THERMAL_PRINT.NewRow();
                drThermal[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME] = cboThermalPrinter.SelectedValue;
                drThermal["X"] = numLeftMargin.Value;
                drThermal["Y"] = numTopMargin.Value;
                LoginInfo.CFG_THERMAL_PRINT.Rows.Add(drThermal);

                LoginInfo.CFG_LABEL = CreateEmptyLabelTable();
                DataRow drlabel = LoginInfo.CFG_LABEL.NewRow();
                drlabel[CustomConfig.CONFIG_LABEL_TYPE] = Convert.ToString(cboLabelType.SelectedValue);
                drlabel[CustomConfig.CONFIG_LABEL_COPIES] = Convert.ToInt32(numLabelCopies.Value);
                drlabel[CustomConfig.CONFIG_CUT_LABEL] = cboCutLabel.SelectedValue as string;
                drlabel[CustomConfig.CONFIG_LABEL_AUTO] = Convert.ToString(cboLabelAuto.SelectedValue);
                drlabel[CustomConfig.CONFIG_CARD_COPIES] = Convert.ToInt32(numCardCopies.Value);
                drlabel[CustomConfig.CONFIG_CARD_AUTO] = Convert.ToString(cboCardAuto.SelectedValue);
                drlabel[CustomConfig.CONFIG_CARD_POPUP] = Convert.ToString(cboCardPopup.SelectedValue);
                drlabel[CustomConfig.CONFIG_THERMAL_COPIES] = Convert.ToInt32(numThermalCopies.Value);
                drlabel[CustomConfig.CONFIG_LABEL_FIRST_CUT_COPIES] = Convert.ToInt32(numFirstCutCopies.Value);
                LoginInfo.CFG_LABEL.Rows.Add(drlabel);

                LoginInfo.CFG_LOGGING = CreateEmptyLoggingTable();
                DataRow drlogging = LoginInfo.CFG_LOGGING.NewRow();
                drlogging[CustomConfig.CONFIGTABLE_LOGGING_UI] = chkUILog.IsChecked;
                drlogging[CustomConfig.CONFIGTABLE_LOGGING_FRAME] = chkFrameLog.IsChecked;
                drlogging[CustomConfig.CONFIGTABLE_LOGGING_BIZRULE] = chkBizRuleLog.IsChecked;
                LoginInfo.CFG_LOGGING.Rows.Add(drlogging);

                LoginInfo.CFG_ETC = CreateEmptyEtcTable();
                DataRow drEtc = LoginInfo.CFG_ETC.NewRow();
                drEtc[CustomConfig.CONFIGTABLE_ETC_NOTICEUSE] = chkNoticeUseType.IsChecked;
                drEtc[CustomConfig.CONFIGTABLE_ETC_SMALLTYPE] = chkSmallType.IsChecked;
                drEtc[CustomConfig.CONFIGTABLE_ETC_WIPSTAT] = Convert.ToString(cboWipstat.SelectedValue);
                drEtc[CustomConfig.CONFIGTABLE_ETC_SAMPLE_COPIES] = Convert.ToString(cboSampleCopies.SelectedValue);
                drEtc[CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE] = Convert.ToString(cboHistYScale.SelectedValue);
                drEtc[CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE] = Convert.ToString(cboPaperSize.SelectedValue);
                LoginInfo.CFG_ETC.Rows.Add(drEtc);

                DataSet dsConfig = new DataSet();
                dsConfig.Tables.Add(LoginInfo.CFG_GENERAL_PRINTER);
                dsConfig.Tables.Add(LoginInfo.CFG_SERIAL_PRINT);
                dsConfig.Tables.Add(LoginInfo.CFG_THERMAL_PRINT);
                dsConfig.Tables.Add(LoginInfo.CFG_LOGGING);
                dsConfig.Tables.Add(LoginInfo.CFG_ETC);
                dsConfig.Tables.Add(LoginInfo.CFG_LABEL);

                if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                {
                    LoginInfo.CFG_FORM = CreateEmptyFormTable();
                    DataRow drForm = LoginInfo.CFG_FORM.NewRow();
                    drForm["EQP_STATUS"] = chkEqpStatus.IsChecked;
                    drForm["W_LOT"] = chkWLot.IsChecked;
                    drForm["PROC_WAIT_LIMIT_TIME_OVER"] = chkFormProcWaitLimitTimeOver.IsChecked;
                    drForm["AGING_LIMIT_TIME_OVER"] = chkFormAgingLimitTimeOver.IsChecked;
                    drForm["AGING_OUTPUT_TIME_OVER"] = chkFormAgingOutputTimeOver.IsChecked; // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
                    drForm["FITTED_DOCV_TRNF_FAIL"] = chkFormFittedDOCVTrnfFail.IsChecked;   // 2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                    drForm["FORM_FDS_ALARM"] = chkFormFDSAlarm.IsChecked;   // 2024.08.20 / 최성필(csp59463) / FDS Alarm 사용여부
                    drForm["FORM_SAS_ALARM"] = chkFormSASAlarm.IsChecked;   // 2024.08.26 / 최성필(csp59463) / SAS 송수신 오류 사용여부
                    drForm["FORM_HIGH_AGING_ABNORM_TMPR_ALARM"] = chkFormHighAgingAbnormTmprAlarm.IsChecked;   //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부
                    LoginInfo.CFG_FORM.Rows.Add(drForm);
                    dsConfig.Tables.Add(LoginInfo.CFG_FORM);
                }

                string customConfigPath = string.Empty;
                string settingFileName = string.Empty;

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\";
                    settingFileName = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;

                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];

                        if (string.IsNullOrEmpty(current))
                            current = directoryName;
                        else
                            current += "\\" + directoryName;

                        DirectoryInfo directoryInfo = new DirectoryInfo(current);

                        if (!directoryInfo.Exists)
                            directoryInfo.Create();
                    }

                    dsConfig.WriteXml(customConfigPath + settingFileName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];
                    settingFileName = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;

                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];

                        if (string.IsNullOrEmpty(current))
                            current = directoryName;
                        else
                            current += @"\" + directoryName;

                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);

                        if (!directoryInfo.Exists)
                            directoryInfo.Create();
                    }

                    FileInfo info = new FileInfo(current + @"\" + settingFileName);

                    if (info.Exists)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(current + @"\" + settingFileName, null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None);
                        File.Delete(current + @"\" + settingFileName);
                    }

                    dsConfig.WriteXml(current + @"\" + settingFileName, XmlWriteMode.WriteSchema);
                }

                CustomConfig.Instance.Reload(ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"]);

                this.DialogResult = MessageBoxResult.OK;

            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboShop.SelectedIndex > -1)
                {
                    selectedShop = Convert.ToString(cboShop.SelectedValue);
                    Set_Combo_Area(cboArea);
                }
                else
                {
                    selectedShop = string.Empty;
                }
            }));
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegment(cboEquipmentSegment);
                    Set_Combo_Process(cboProcess);
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    Set_Combo_Process(cboProcess);
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipment.SelectedIndex > -1)
                    selectedEquipment = Convert.ToString(cboEquipment.SelectedValue);
                else
                    selectedEquipment = string.Empty;
            }));
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void cboLang_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboLang.SelectedIndex > -1)
                    LoginInfo.LANGID = Convert.ToString(cboLang.SelectedValue);
                else
                    LoginInfo.LANGID = string.Empty;
            }));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            dgBarcodePrinter.BeginningNewRow += new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewRow);
            dgBarcodePrinter.BeginNewRow();
            dgBarcodePrinter.EndNewRow(true);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgBarcodePrinter.SelectedItem != null)
                dgBarcodePrinter.RemoveRow(dgBarcodePrinter.SelectedIndex);
        }

        private void dgComPort_BeginningNewRow(object sender, C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs e)
        {
            int cnt = (from C1.WPF.DataGrid.DataGridRow r in dgBarcodePrinter.Rows
                       where r.DataItem != null &&
                             DataTableConverter.GetValue(r.DataItem, "DEFAULT") != null &&
                             Convert.ToString(DataTableConverter.GetValue(r.DataItem, "DEFAULT").ToString()).Equals("True")
                       select r.DataItem).Count();

            dgBarcodePrinter.BeginningNewRow -= new EventHandler<C1.WPF.DataGrid.DataGridBeginningNewRowEventArgs>(dgComPort_BeginningNewRow);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERKEY, Guid.NewGuid().ToString());
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_ISACTIVE, true);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE, "Z");
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_BAUDRATE, "9600");
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_PARITYBIT, "None");
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_DATABIT, "8");
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_STOPBIT, "One");
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_X, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_Y, 0);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI, 203);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES, 1);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS, 15);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, false);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT, string.Empty);
            DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID, string.Empty);

            if (cnt == 0)
                DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT, true);
            else
                DataTableConverter.SetValue(e.Item, CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT, false);
        }

        private void dgComPort_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "DEFAULT":
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT, true);
                                chk.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;

                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT, false);
                                    }
                                }
                                break;
                        }
                    }

                    if (e.Cell.Column.Name.Equals(CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME) != null)
                        {
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PORTNAME).ToString().Contains("USB"))
                            {
                                dg.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME].IsReadOnly = false;
                            }
                            else
                            {
                                dg.Columns[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME].IsReadOnly = true;
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERNAME, string.Empty);
                            }
                        }
                    }
                }
            }));
        }
        #endregion

        #region Mehod
        private void Set_Combo_Shop(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("USERID", typeof(string));
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_SHOP_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedShop) select dr).Count() > 0)
                    cbo.SelectedValue = selectedShop;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;

                cboShop_SelectedItemChanged(cbo, null);
                cboShop.SelectedItemChanged -= cboShop_SelectedItemChanged;
                cboShop.SelectedItemChanged += cboShop_SelectedItemChanged;
            });
        }

        private void Set_Combo_Area(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("SHOPID", typeof(string));
            dtRQSTDT.Columns.Add("USERID", typeof(string));
            dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["SHOPID"] = selectedShop;
            drnewrow["USERID"] = LoginInfo.USERID;
            drnewrow["USE_FLAG"] = "Y";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedArea) select dr).Count() > 0)
                    cbo.SelectedValue = selectedArea;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;

                cboArea_SelectedItemChanged(cbo, null);
                cboArea.SelectedItemChanged -= cboArea_SelectedItemChanged;
                cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
            });
        }

        private void Set_Combo_EquipmentSegment(C1ComboBox cbo)
        {
            string sBizName = string.Empty;

            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["AREAID"] = selectedArea;
            dtRQSTDT.Rows.Add(drnewrow);

            //기존에는 활성화 라인은 빼고 가져오게 되어 있음. CNB2동 활성화 오픈으로 활성화와 기존의 DA를 분리함 2021-04-02
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
                sBizName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FORM";
            else
                sBizName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            
            new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                    cbo.SelectedValue = selectedEquipmentSegmant;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;

                cboEquipmentSegment_SelectedItemChanged(cbo, null);
                cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
                cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            });
        }

        private void Set_Combo_Process(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["EQSGID"] = selectedEquipmentSegmant;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                    cbo.SelectedValue = selectedProcess;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;

                cboProcess_SelectedItemChanged(cbo, null);
                cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
                cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
            });
        }

        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("EQSGID", typeof(string));
            dtRQSTDT.Columns.Add("PROCID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["EQSGID"] = selectedEquipmentSegmant;
            drnewrow["PROCID"] = selectedProcess;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                    cbo.SelectedValue = selectedEquipment;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;

                cboEquipment.SelectedItemChanged -= cboEquipment_SelectedItemChanged;
                cboEquipment.SelectedItemChanged += cboEquipment_SelectedItemChanged;
            });
        }

        private void Set_Combo_Lang(C1ComboBox cbo)
        {
            new ClientProxy().ExecuteService("COR_SEL_LANGUAGE", null, "RSLTDT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if (!string.IsNullOrEmpty(LoginInfo.LANGID))
                    cbo.SelectedValue = LoginInfo.LANGID;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
            });
        }

        private void Set_Combo_Wipstat(C1ComboBox cbo)
        {
            string selectedWipStat = string.Empty;

            if (LoginInfo.CFG_ETC.Rows.Count > 0)
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_WIPSTAT))
                    selectedWipStat = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_WIPSTAT].ToString();

            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));

            DataRow drNewRow = dtRQSTDT.NewRow();
            drNewRow["LANGID"] = LoginInfo.LANGID;
            dtRQSTDT.Rows.Add(drNewRow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_WIP_STAT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow drSelect = result.NewRow();
                drSelect["CBO_NAME"] = "-SELECT-";
                drSelect["CBO_CODE"] = null;
                result.Rows.InsertAt(drSelect, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedWipStat) select dr).Count() > 0)
                    cbo.SelectedValue = selectedWipStat;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;
            });
        }

        private void SetNumericCombo(C1ComboBox cbo, int iStart, int iEnd)
        {
            string savedValue = string.Empty;

            if (LoginInfo.CFG_ETC.Rows.Count > 0)
                if (LoginInfo.CFG_ETC.Columns.Contains(cbo.Name.Equals("cboSampleCopies") ? CustomConfig.CONFIGTABLE_ETC_SAMPLE_COPIES : CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE))
                    savedValue = LoginInfo.CFG_ETC.Rows[0][cbo.Name.Equals("cboSampleCopies") ? CustomConfig.CONFIGTABLE_ETC_SAMPLE_COPIES : CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE].ToString();

            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CODE", typeof(string));
            dtResult.Columns.Add("NAME", typeof(string));

            DataRow newRow = dtResult.NewRow();

            for (int i = iStart; i <= iEnd; i++)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { i, i };
                dtResult.Rows.Add(newRow);
            }

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if ((from DataRow dr in dtResult.Rows where dr["CODE"].Equals(savedValue) select dr).Count() > 0)
                cbo.SelectedValue = savedValue;
            else if (dtResult.Rows.Count > 0)
                cbo.SelectedIndex = cbo.Name.Equals("cboSampleCopies") ? 2 : cbo.Items.Count - 1;
            else if (dtResult.Rows.Count == 0)
                cbo.SelectedItem = null;
        }

        private void SetPaperSizeCombo(C1ComboBox cbo)
        {
            try
            {

                string selectedPaperSize = string.Empty;

                if (LoginInfo.CFG_ETC.Rows.Count > 0)
                    if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
                        selectedPaperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                var dr = dt.NewRow();
                dr["CBO_CODE"] = "A4";
                dr["CBO_NAME"] = "A4";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["CBO_CODE"] = "A5";
                dr["CBO_NAME"] = "A5";
                dt.Rows.Add(dr);

                DataRow drSelect = dt.NewRow();
                drSelect["CBO_CODE"] = null;
                drSelect["CBO_NAME"] = "-SELECT-";
                dt.Rows.InsertAt(drSelect, 0);
                cbo.ItemsSource = DataTableConverter.Convert(dt);

                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == selectedPaperSize
                             select t).FirstOrDefault();

                if (query != null)
                {
                    cbo.SelectedValue = selectedPaperSize;
                }
                else
                {
                    if (dt.Rows.Count > 0)
                        cbo.SelectedIndex = 0;
                    else if (dt.Rows.Count == 0)
                        cbo.SelectedItem = null;
                }

            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void SetLocalPrinterComboBox(C1ComboBox cbo)
        {
            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CODE", typeof(string));
            dtResult.Columns.Add("NAME", typeof(string));

            DataRow naRow = dtResult.NewRow();
            naRow.ItemArray = new object[] { null, "-SELECT-" };
            dtResult.Rows.InsertAt(naRow, 0);

            

            var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");

            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { name, name };
                dtResult.Rows.Add(newRow);
            }

            cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            if (cbo.Name.Equals("cboGeneralPrinter"))
                if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0)
                    if (LoginInfo.CFG_GENERAL_PRINTER.Columns.Contains(CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME))
                        if (!string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0][CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                            cbo.SelectedValue = LoginInfo.CFG_GENERAL_PRINTER.Rows[0][CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

            if (cbo.Name.Equals("cboThermalPrinter"))
                if (LoginInfo.CFG_THERMAL_PRINT.Rows.Count > 0)
                    if (LoginInfo.CFG_THERMAL_PRINT.Columns.Contains(CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME))
                        if (!string.IsNullOrEmpty(LoginInfo.CFG_THERMAL_PRINT.Rows[0][CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString()))
                            cbo.SelectedValue = LoginInfo.CFG_THERMAL_PRINT.Rows[0][CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString();
        }

        DataTable dtLocalPrinter()
        {
            DataTable dtResult = new DataTable();

            dtResult.Columns.Add("CODE", typeof(string));
            dtResult.Columns.Add("NAME", typeof(string));

            var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");

            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                //var status = printer.GetPropertyValue("Status");
                //var isDefault = printer.GetPropertyValue("Default");
                //var isNetworkPrinter = printer.GetPropertyValue("Network");

                DataRow newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { name, name };
                dtResult.Rows.Add(newRow);
            }

            return dtResult;
        }

        class LocalPortInfo
        {
            public LocalPortInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }

            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
        }

        static List<LocalPortInfo> GetLocalPortList()
        {
            List<LocalPortInfo> devices = new List<LocalPortInfo>();

            ManagementObjectCollection collection;

            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
                devices.Add(new LocalPortInfo((string)device.GetPropertyValue("DeviceID"), (string)device.GetPropertyValue("PNPDeviceID"), (string)device.GetPropertyValue("Description")));

            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_ParallelPort"))
                collection = searcher.Get();

            if (collection != null)
                foreach (var device in collection)
                    devices.Add(new LocalPortInfo((string)device.GetPropertyValue("DeviceID"), (string)device.GetPropertyValue("PNPDeviceID"), (string)device.GetPropertyValue("Description")));

            collection.Dispose();
            return devices;
        }

        DataTable dtLocalPort()
        {
            DataTable dtResult = new DataTable();
            DataRow newRow;

            dtResult.Columns.Add("CODE", typeof(string));
            dtResult.Columns.Add("NAME", typeof(string));

            string[] ports = SerialPort.GetPortNames();

            foreach (var port in ports)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { port, port };
                dtResult.Rows.Add(newRow);
            }

            var localPorts = GetLocalPortList();

            if (localPorts.Count > 0)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "USB", "USB" };
                dtResult.Rows.Add(newRow);

                foreach (LocalPortInfo port in localPorts)
                {
                    if (port.DeviceID.Contains("LPT"))
                    {
                        newRow = dtResult.NewRow();
                        newRow.ItemArray = new object[] { port.DeviceID.Substring(0, 4), port.DeviceID.Substring(0, 4) };
                        dtResult.Rows.Add(newRow);
                    }
                }
            }

            return dtResult;
        }

        DataTable dtZplType()
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRqstDt.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "PRINTER_MODEL";
                dtRqstDt.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRqstDt);

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        DataTable dtEquipment()
        {
            try
            {
                DataTable dtRqstDt = new DataTable();
                dtRqstDt.TableName = "RQSTDT";
                dtRqstDt.Columns.Add("LANGID", typeof(string));
                dtRqstDt.Columns.Add("EQSGID", typeof(string));
                dtRqstDt.Columns.Add("PROCID", typeof(string));

                DataRow drNewRow = dtRqstDt.NewRow();
                drNewRow["LANGID"] = LoginInfo.LANGID;
                drNewRow["EQSGID"] = selectedEquipmentSegmant;
                drNewRow["PROCID"] = selectedProcess;
                dtRqstDt.Rows.Add(drNewRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRqstDt);

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        DataTable GetLabelInfo()
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LABELINFO_CBO";

                DataTable dtRqstDt = new DataTable("RQSTDT");
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dtRqstDt);
                return dtResult;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void Set_Combo_DefaultMenu(C1ComboBox cbo)
        {
            DataTable dtIndata = new DataTable();
            dtIndata.Columns.Add("USERID", typeof(string));
            dtIndata.Columns.Add("PROGRAMTYPE", typeof(string));
            dtIndata.Columns.Add("MENUIUSE", typeof(string));
            dtIndata.Columns.Add("LANGID", typeof(string));
            dtIndata.Columns.Add("MENULEVEL", typeof(string));
            dtIndata.Columns.Add("SYSTEM_ID", typeof(string));
            dtIndata.Columns.Add("AREAID", typeof(string));

            DataRow menuIndata = dtIndata.NewRow();
            menuIndata["USERID"] = LoginInfo.USERID;
            menuIndata["PROGRAMTYPE"] = LGC.GMES.MES.Common.Common.APP_System;
            menuIndata["MENUIUSE"] = "Y";
            menuIndata["LANGID"] = LoginInfo.LANGID;
            menuIndata["MENULEVEL"] = "3";
            menuIndata["SYSTEM_ID"] = LoginInfo.SYSID;
            menuIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtIndata.Rows.Add(menuIndata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_MENU_WITH_BOOKMARK", "INDATA", "OUTDATA", dtIndata, (result, ex) =>
            {
                if (ex != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow dr = result.NewRow();
                dr["FULLMENUNAME"] = "-SELECT-";
                dr["MENUID"] = null;
                result.Rows.InsertAt(dr, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if (!string.IsNullOrEmpty(LoginInfo.CFG_INI_MENUID))
                    cbo.SelectedValue = LoginInfo.CFG_INI_MENUID;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
            });
        }

        private DataTable CreateEmptyGeneralPrinterTable()
        {
            DataTable emptyGeneralPrinterTable = new DataTable();
            emptyGeneralPrinterTable.TableName = CustomConfig.CONFIGTABLE_GENERALPRINTER;
            emptyGeneralPrinterTable.Columns.Add(CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME, typeof(string));

            return emptyGeneralPrinterTable;
        }

        private DataTable CreateEmptyThermalPrintTable()
        {
            DataTable emptyThermalPrintTable = new DataTable();
            emptyThermalPrintTable.TableName = CustomConfig.CONFIGTABLE_THERMALPRINTER;
            emptyThermalPrintTable.Columns.Add(CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME, typeof(string));
            emptyThermalPrintTable.Columns.Add(CustomConfig.CONFIGTABLE_THERMALPRINTER_X, typeof(int));
            emptyThermalPrintTable.Columns.Add(CustomConfig.CONFIGTABLE_THERMALPRINTER_Y, typeof(int));

            return emptyThermalPrintTable;
        }

        private DataTable CreateEmptyLoggingTable()
        {
            DataTable emptyLoggingTable = new DataTable();
            emptyLoggingTable.TableName = CustomConfig.CONFIGTABLE_LOGGING;
            emptyLoggingTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_UI, typeof(bool));
            emptyLoggingTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_MONITORING, typeof(bool));
            emptyLoggingTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_FRAME, typeof(bool));
            emptyLoggingTable.Columns.Add(CustomConfig.CONFIGTABLE_LOGGING_BIZRULE, typeof(bool));

            return emptyLoggingTable;
        }

        private DataTable CreateEmptyLabelTable()
        {
            DataTable emptyLabelTable = new DataTable();
            emptyLabelTable.TableName = CustomConfig.CONFIGTABLE_LABEL;
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_LABEL_TYPE, typeof(string));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_LABEL_COPIES, typeof(Int32));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_CUT_LABEL, typeof(string));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_LABEL_AUTO, typeof(string));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_CARD_COPIES, typeof(Int32));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_CARD_AUTO, typeof(string));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_CARD_POPUP, typeof(string));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_THERMAL_COPIES, typeof(Int32));
            emptyLabelTable.Columns.Add(CustomConfig.CONFIG_LABEL_FIRST_CUT_COPIES, typeof(Int32));
            return emptyLabelTable;
        }

        private DataTable CreateEmptyEtcTable()
        {
            DataTable emptyEtcTable = new DataTable();
            emptyEtcTable.TableName = CustomConfig.CONFIGTABLE_ETC;
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_NOTICEUSE, typeof(bool));
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_SMALLTYPE, typeof(bool));
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_WIPSTAT, typeof(string));
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_SAMPLE_COPIES, typeof(int));
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_HISTCARD_SCALE, typeof(int));
            emptyEtcTable.Columns.Add(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE, typeof(string));

            return emptyEtcTable;
        }

        private DataTable CreateEmptyFormTable()
        {
            DataTable emptyFormTable = new DataTable();
            emptyFormTable.TableName = "FORM";
            emptyFormTable.Columns.Add("EQP_STATUS", typeof(bool));
            emptyFormTable.Columns.Add("W_LOT", typeof(bool));
            emptyFormTable.Columns.Add("PROC_WAIT_LIMIT_TIME_OVER", typeof(bool));
            emptyFormTable.Columns.Add("AGING_LIMIT_TIME_OVER", typeof(bool));
            emptyFormTable.Columns.Add("AGING_OUTPUT_TIME_OVER", typeof(bool)); // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
            emptyFormTable.Columns.Add("FITTED_DOCV_TRNF_FAIL", typeof(bool));  // 2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
            emptyFormTable.Columns.Add("FORM_FDS_ALARM", typeof(bool));  // 2024.08.20 / 최성필(csp59463) / FDS Alarm 사용여부            
            emptyFormTable.Columns.Add("FORM_SAS_ALARM", typeof(bool));  // 2024.08.20 / 최성필(csp59463) / SAS 송수신 오류 Alarm 사용여부         
            emptyFormTable.Columns.Add("FORM_HIGH_AGING_ABNORM_TMPR_ALARM", typeof(bool));  //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부   

            return emptyFormTable;
        }

        public bool Check_Mandatory(List<DependencyObject> contList)
        {
            foreach (var item in contList)
            {
                if (Convert.ToString((item as C1ComboBox).SelectedValue) == string.Empty)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("필수입력항목을 모두 입력하십시오."), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void Set_Combo_LabelType(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["CMCDTYPE"] = "PROC_LABEL_CODE";

            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                cbo.ItemsSource = DataTableConverter.Convert(result);

                if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(labelType) select dr).Count() > 0)
                    cbo.SelectedValue = labelType;
                else if (result.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (result.Rows.Count == 0)
                    cbo.SelectedItem = null;
            });
        }

        private void Set_Combo_CutLabel(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "Y";
                row["CBO_NAME"] = "Y";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "N";
                row["CBO_NAME"] = "N";
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if ((from DataRow dr in dt.Rows where dr["CBO_CODE"].Equals(cutLabel) select dr).Count() > 0)
                    cbo.SelectedValue = cutLabel;
                else if (dt.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (dt.Rows.Count == 0)
                    cbo.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }

        private void Set_Combo_LableAuto(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "Y";
                row["CBO_NAME"] = "Y";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "N";
                row["CBO_NAME"] = "N";
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if ((from DataRow dr in dt.Rows where dr["CBO_CODE"].Equals(labelAuto) select dr).Count() > 0)
                    cbo.SelectedValue = labelAuto;
                else if (dt.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (dt.Rows.Count == 0)
                    cbo.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }

        private void Set_Combo_CardAuto(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "Y";
                row["CBO_NAME"] = "Y";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "N";
                row["CBO_NAME"] = "N";
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if ((from DataRow dr in dt.Rows where dr["CBO_CODE"].Equals(cardAuto) select dr).Count() > 0)
                    cbo.SelectedValue = cardAuto;
                else if (dt.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (dt.Rows.Count == 0)
                    cbo.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }

        private void Set_Combo_CardPopup(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "Y";
                row["CBO_NAME"] = "Y";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "N";
                row["CBO_NAME"] = "N";
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if ((from DataRow dr in dt.Rows where dr["CBO_CODE"].Equals(cardPopup) select dr).Count() > 0)
                    cbo.SelectedValue = cardPopup;
                else if (dt.Rows.Count > 0)
                    cbo.SelectedIndex = 0;
                else if (dt.Rows.Count == 0)
                    cbo.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion
    }
}