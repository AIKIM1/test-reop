/*************************************************************************************
 Created Date :
      Creator : PSM
   Decription : Trouble 분석
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  PSM : Initial Created
  2020.12.07  AKB : 
  2023.05.02  최도훈 : 인도네시아 조회시 오류나는 현상 수정
  2023.05.26  임근영 : [E20230525-001172] 설비레벨 콤보박스 추가
  2023.06.12  임근영 : Trouble 상세 List 화면에 넘겨주는 parameter 추가(ALARM_LEVEL)
  2023.08.24  홍석원 : Main/Sub알람 구분 조회 기능 추가
  2025.07.02  전상진 : 화면 최초 Load 후 조회시 조회된 데이터 Row가 정상적으로 공통함수 이벤트에 전달이 되지않아 Merge가 동작안해 별도로 소스내 로직 구현으로 처리
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
    public partial class FCS001_027 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_027()
        {
            InitializeComponent();
            InitCombo();
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

            //C1ComboBox[] cboLaneChild = { cboLane };
            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilter);

            //C1ComboBox[] cboEqpKindParent = { cboEqp };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE");

            _combo.SetCombo(cboLevel, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_ALARM_LEVEL_CODE");  ////

            dtpFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LANE_ID", typeof(string));
                inDataTable.Columns.Add("S70", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("EQPT_ALARM_LEVEL_CODE", typeof(string)); ///

                inDataTable.Columns.Add("MAIN_ALARM_FLAG", typeof(string));
                inDataTable.Columns.Add("SUB_ALARM_FLAG", typeof(string));


                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                newRow["S70"] = Util.GetCondition(cboEqp, bAllNull: true);
                newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                newRow["EQPT_ALARM_LEVEL_CODE"] = Util.GetCondition(cboLevel, bAllNull: true); ///

                // Main/Sub Alarm Flag가 모두 체크되거나 해제된 경우에는 WHERE 조건을 제외
                if(chkMainAlarm.IsChecked.Value && !chkSubAlarm.IsChecked.Value) // MainAlarm만 조회
                {
                    newRow["MAIN_ALARM_FLAG"] = "Y";
                } else if (!chkMainAlarm.IsChecked.Value && chkSubAlarm.IsChecked.Value) // SubAlarm만 조회
                {
                    newRow["SUB_ALARM_FLAG"] = "Y";
                }

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_SEL_LOAD_TROUBLE", "INDATA", "OUTDATA", inDataTable, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgTroubleList, dtResult, FrameOperation, true);

                    string[] sColumnName = new string[] { "LANE_NAME", "EQPTKIND", "EQPTNAME" };
                    SetDataGridMergeExtensionCol_MIL(dgTroubleList, dtResult.Rows.Count, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                });

                //string[] sColumnName = new string[] { "LANE_NAME", "EQPTKIND", "EQPTNAME" };
                //_Util.SetDataGridMergeExtensionCol(dgTroubleList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgTroubleList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("EQPT_ALARM_NAME") || e.Cell.Column.Name.Equals("EQPT_ALARM_CODE"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgTroubleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            if (!(dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_NAME") || dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_CODE"))) return;

            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                string sLANE_ID = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "LANE_ID")));
                string sS70 = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPTKINDCD")));
                string sTROUBLE_CD = "";
                ////설비알람 레벨 여기 추가.
                string sALARM_LEVEL = (Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPT_ALARM_LEVEL")));////////추가

                bool bMainAlarmFlag = chkMainAlarm.IsChecked.Value;
                bool bSubAlarmFlag = chkSubAlarm.IsChecked.Value;


                DateTime sFROM_DATE = dtpFromDate.SelectedDateTime;
                string sFROM_TIME = dtpFromTime.DateTime.Value.ToString("HHmm00");
                DateTime sTO_DATE = dtpToDate.SelectedDateTime;
                string sTO_TIME = dtpToTime.DateTime.Value.ToString("HHmm59");

                if (dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_CODE") || dgTroubleList.CurrentColumn.Name.Equals("EQPT_ALARM_NAME"))
                {
                    sTROUBLE_CD = Util.NVC(DataTableConverter.GetValue(dgTroubleList.CurrentRow.DataItem, "EQPT_ALARM_CODE"));
                }

                Load_FCS001_027_TROUBLE_DETAIL_LIST(sLANE_ID, sS70, sTROUBLE_CD, sFROM_DATE, sFROM_TIME, sTO_DATE,sTO_TIME,sALARM_LEVEL, bMainAlarmFlag, bSubAlarmFlag); //////////설비알람레벨 파라미터 추가 

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Load_FCS001_027_TROUBLE_DETAIL_LIST(string sLANE_ID, string sS70,
                                         string sTROUBLE_CD, DateTime sFROM_DATE, string sFROM_TIME, DateTime sTO_DATE, string sTO_TIME,string sALARM_LEVEL, //////////설비알람레벨 파라미터 추가 
                                         bool bMainAlarmFlag, bool bSubAlarmFlag)  
        {
            FCS001_027_TROUBLE_DETAIL_LIST TroubleDetailList = new FCS001_027_TROUBLE_DETAIL_LIST();
            TroubleDetailList.FrameOperation = FrameOperation;

            if (TroubleDetailList != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = sLANE_ID; //sLANE_ID
                Parameters[1] = sS70; //sS70
                Parameters[2] = sTROUBLE_CD; //sTROUBLE_CD
                Parameters[3] = sFROM_DATE; //sFROM_DATE
                Parameters[4] = sFROM_TIME; //sFROM_TIME
                Parameters[5] = sTO_DATE; //sTO_DATE
                Parameters[6] = sTO_TIME; //sTO_TIME
                //설비 알람 레벨 파라미터 추가 
                Parameters[7] = sALARM_LEVEL; //sALARM_LEVEL

                Parameters[8] = bMainAlarmFlag; //bMainAlarmFlag
                Parameters[9] = bSubAlarmFlag; //bSubAlarmFlag

                this.FrameOperation.OpenMenuFORM("SFU010710071", "FCS001_027_TROUBLE_DETAIL_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Trouble 상세 List"), true, Parameters);
            }

        }

        private void btnTrouble_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            try
            {
                Load_FCS001_027_EQPT_TROUBLE_LIST();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void Load_FCS001_027_EQPT_TROUBLE_LIST()
        {
            FCS001_027_EQPT_TROUBLE_LIST EqptTroubleList = new FCS001_027_EQPT_TROUBLE_LIST();
            EqptTroubleList.FrameOperation = FrameOperation;

            if (EqptTroubleList != null)
            {
                this.FrameOperation.OpenMenuFORM("SFU010710072", "FCS001_027_EQPT_TROUBLE_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("설비 Trouble List"), true, null);
            }

        }
        public void SetDataGridMergeExtensionCol_MIL(C1.WPF.DataGrid.C1DataGrid dataGrid, int rowcount, string[] sColumnName, DataGridMergeMode eMergeMode)
        {
            if (rowcount > 0)
            {
                for (int i = 0; i < sColumnName.Length; i++)
                {
                    DataGridMergeExtension.SetMergeMode(dataGrid.Columns[sColumnName[i].ToString()], eMergeMode); //DataGridMergeMode.VERTICALHIERARCHI);
                }
                dataGrid.ReMerge();
            }
        }
    }
}
