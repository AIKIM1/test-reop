/*************************************************************************************
 Created Date : 2020.03.18
      Creator : 
   Decription : 믹서 투입현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.29  김태우 책임 : NFF 분기(OHT 관련 추가 정보 추가)
  2024.02.20  김태균 : OHT 컬럼 추가
  2024.05.24  배현우 : OHT Cutting Try 컬럼 사용 안함 처리
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_036 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo _combo = new CommonCombo();
        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_036()
        {
            InitializeComponent();     
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SetEvent();
            InitDisplay();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            string[] Filter1 = new string[] { LoginInfo.CFG_EQSG_ID };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, sFilter: Filter1, sCase: "ProcessCWA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
        }
        #endregion

        #region InitDisplay
        private void InitDisplay()
        {
            if (IsMixerOHTInfo())
            {
                dgResult.Columns["OHT_STRT_DTTM"].Visibility = Visibility.Visible;
                dgResult.Columns["OHT_EMPTY_BAG_RCY_END_DTTM"].Visibility = Visibility.Visible;
                dgResult.Columns["OHT_CUT_DTTM"].Visibility = Visibility.Visible;
                dgResult.Columns["OHT_END_DTTM"].Visibility = Visibility.Visible;
                dgResult.Columns["MTRL_INPUT_TIME"].Visibility = Visibility.Visible;
                dgResult.Columns["BAG_CLOSE_TIME"].Visibility = Visibility.Visible;
                dgResult.Columns["BAG_LOAD_TIME"].Visibility = Visibility.Visible;
                dgResult.Columns["TOTL_CYCL_TIME"].Visibility = Visibility.Visible;
                dgResult.Columns["BAG_INI_WEIGHT"].Visibility = Visibility.Visible;
                dgResult.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                dgResult.Columns["CUT_COUNT"].Visibility = Visibility.Collapsed; //2024-05-24 해당 컬럼 미사용
                dgResult.Columns["BAG_CUT_FLAG"].Visibility = Visibility.Visible;
            }
            else
            {
                dgResult.Columns["OHT_STRT_DTTM"].Visibility = Visibility.Collapsed;
                dgResult.Columns["OHT_EMPTY_BAG_RCY_END_DTTM"].Visibility = Visibility.Collapsed;
                dgResult.Columns["OHT_CUT_DTTM"].Visibility = Visibility.Collapsed;
                dgResult.Columns["OHT_END_DTTM"].Visibility = Visibility.Collapsed;
                dgResult.Columns["MTRL_INPUT_TIME"].Visibility = Visibility.Collapsed;
                dgResult.Columns["BAG_CLOSE_TIME"].Visibility = Visibility.Collapsed;
                dgResult.Columns["BAG_LOAD_TIME"].Visibility = Visibility.Collapsed;
                dgResult.Columns["TOTL_CYCL_TIME"].Visibility = Visibility.Collapsed;
                dgResult.Columns["BAG_INI_WEIGHT"].Visibility = Visibility.Collapsed;
                dgResult.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                dgResult.Columns["CUT_COUNT"].Visibility = Visibility.Collapsed;
                dgResult.Columns["BAG_CUT_FLAG"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Event
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31) {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }

            if (cboProcess.SelectedValue.ToString().Equals("SELECT")) {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요
                return;
            }

            SearchData();
        }
        #endregion

        #region Mehod

        private void SearchData()
        {

            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgResult);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Util.GetCondition(cboProcess, bAllNull:true);
                Indata["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                Indata["LOTID"] = Util.GetCondition(txtLOTID).Equals("") ? null : Util.GetCondition(txtLOTID);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_CONSUME_LIST", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgResult, dtMain, FrameOperation,true);
                    string[] sColumnName = new string[] { "LOTID", "REQ_ID", "EQPTID", "EQPTNAME" };
                    _Util.SetDataGridMergeExtensionCol(dgResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }
        private bool IsMixerOHTInfo()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "MIXER_OHT_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDIUSE"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }
        #endregion
    }
}
