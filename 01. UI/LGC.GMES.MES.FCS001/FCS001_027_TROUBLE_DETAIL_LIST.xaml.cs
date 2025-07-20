/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : Trouble 상세 List
--------------------------------------------------------------------------------------
 [Change History]
  2020.        DEVELOPER  : Initial Created.
  2022.07.13   조영대 : Truble Code Combo 수정   
  2022.08.18   조영대 : Truble Code Combo 조회 조건이 다를때 조회   
  2023.05.02   최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.06.12   임근영 : 설비알람레벨 Combobox,컬럼 추가. 
                        Trouble 분석 화면에서 더블클릭시 조회되는 조건에 설비알람레벨 추가 
  2023.08.24   홍석원 : Main/Sub알람 구분 조회 기능 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_027_TROUBLE_DETAIL_LIST : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _sLANE_ID = string.Empty;
        private string _sS70 = string.Empty;
        private string _sTROUBLE_CD = string.Empty;

        private string _sFROM_DATE = string.Empty;
        private string _sTO_DATE = string.Empty;

        private string saveLANE_ID = string.Empty;
        private string saveS70 = string.Empty;
        private string saveFROM_DATE = string.Empty;
        private string saveTO_DATE = string.Empty;
        private string _sALARM_LEVEL = string.Empty;

        private bool _bMainAlarmFlag = false;
        private bool _bSubAlarmFlag = false;


        public FCS001_027_TROUBLE_DETAIL_LIST()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE",sFilter: sFilter);

            //C1ComboBox[] cboEqpKindChild = { cboTrouble };
            //_combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE", cbChild: cboEqpKindChild);
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE");

            //C1ComboBox[] cboTroubleParent = { cboEqp };
            //_combo.SetCombo(cboTrouble, CommonCombo_Form.ComboStatus.ALL, sCase: "TROUBLE",cbParent:cboTroubleParent);

            //알람레벨콤보박스추가///
            _combo.SetCombo(cboLevel, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_ALARM_LEVEL_CODE");

            GetTroubleCombo();

        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;

            //if (string.IsNullOrEmpty(_sLANE_ID) || string.IsNullOrEmpty(_sS70) || string.IsNullOrEmpty(_sTROUBLE_NAME) || string.IsNullOrEmpty(_sTROUBLE_CD))
            //    return;

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = this.FrameOperation.Parameters;

            _sLANE_ID = Util.NVC(tmps[0]);
            _sS70 = Util.NVC(tmps[1]);
            _sTROUBLE_CD = Util.NVC(tmps[2]);
            _sALARM_LEVEL = Util.NVC(tmps[7]);   ///
            _bMainAlarmFlag = tmps[8] == null ? false : (bool)tmps[8];
            _bSubAlarmFlag = tmps[9] == null ? false : (bool)tmps[9];

            InitCombo();
            InitControl();

            dtpFromDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[3]));
            dtpFromTime.DateTime = Util.StringToDateTime(dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[4]), "yyyyMMdd HHmmss");
            dtpToDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(tmps[5]));
            dtpToTime.DateTime = Util.StringToDateTime(dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + " " + Util.NVC(tmps[6]), "yyyyMMdd HHmmss");

            cboLane.SelectedValue = _sLANE_ID;
            cboEqp.SelectedValue = _sS70;
            cboTrouble.SelectedValue = _sTROUBLE_CD;
            cboLevel.SelectedValue = _sALARM_LEVEL;   ////

            chkMainAlarm.IsChecked = _bMainAlarmFlag;
            chkSubAlarm.IsChecked = _bSubAlarmFlag;

            GetList();

            //  SetEvent();

            this.Loaded -= UserControl_Loaded;
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dtpDate_DataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTroubleCombo();
        }

        private void dtpDate_DataTimeChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<DateTime> e)
        {
            GetTroubleCombo();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            GetTroubleCombo();
        }

        private void cboEqp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            GetTroubleCombo();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANE_ID", typeof(string));
                inDataTable.Columns.Add("EQP_KIND_CD", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TO_DATE", typeof(DateTime));
                inDataTable.Columns.Add("TROUBLE_CD", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("ALARM_LEVEL", typeof(string));   //

                inDataTable.Columns.Add("MAIN_ALARM_FLAG", typeof(string));
                inDataTable.Columns.Add("SUB_ALARM_FLAG", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANE_ID"] = cboLane.GetBindValue();
                newRow["EQP_KIND_CD"] = cboEqp.GetBindValue();
                newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:59");
                newRow["TROUBLE_CD"] = cboTrouble.GetBindValue();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["ALARM_LEVEL"] = cboLevel.GetBindValue();  ///////

                // Main/Sub Alarm Flag가 모두 체크되거나 해제된 경우에는 WHERE 조건을 제외
                if (chkMainAlarm.IsChecked.Value && !chkSubAlarm.IsChecked.Value) // MainAlarm만 조회
                {
                    newRow["MAIN_ALARM_FLAG"] = "Y";
                }
                else if (!chkMainAlarm.IsChecked.Value && chkSubAlarm.IsChecked.Value) // SubAlarm만 조회
                {
                    newRow["SUB_ALARM_FLAG"] = "Y";
                }

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_SEL_LOAD_TROUBLE_LIST", "INDATA", "OUTDATA", inDataTable, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgTroubleDetailList, dtResult, FrameOperation, true);
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetTroubleCombo()
        {
            if (dtpFromDate.SelectedDateTime == null || dtpFromTime.DateTime == null) return;
            if (dtpToDate.SelectedDateTime == null || dtpToTime.DateTime == null) return;

            string fromDate = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
            string toDate = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:00");

            // 다르면 실행
            if (saveLANE_ID == cboLane.GetStringValue() &&
                saveS70 == cboEqp.GetStringValue() &&
                saveFROM_DATE == fromDate &&
                saveTO_DATE == toDate) return;

            string bizRuleName = "DA_BAS_SEL_COMBO_TROUBLE";
            string[] arrColumn = { "LANGID", "LANE_ID", "S70", "FROM_DATE", "TO_DATE" };
            string[] arrCondition = { LoginInfo.LANGID,
                                      cboLane.GetStringValue() == "" ? null : cboLane.GetStringValue(),
                                      cboEqp.GetStringValue() == "" ? null : cboEqp.GetStringValue(),
                                      fromDate.Replace('.', ':'), toDate.Replace('.', ':') };

            cboTrouble.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);

            saveLANE_ID = cboLane.GetStringValue();
            saveS70 = cboEqp.GetStringValue();
            saveFROM_DATE = fromDate;
            saveTO_DATE = toDate;
        }


        #endregion

        
    }
}
