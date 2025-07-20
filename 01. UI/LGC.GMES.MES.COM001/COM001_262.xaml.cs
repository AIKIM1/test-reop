/*************************************************************************************
 Created Date : 2020.06.12
      Creator : 
   Decription : VD 전극검사 Loss 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.12  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_262 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        string CSTStatus = string.Empty;

        public COM001_262()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SetEvent();
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            // 라인
            String[] sFilter = { LoginInfo.CFG_SHOP_ID, null, Process.VD_ELEC, null };
            C1ComboBox[] cboLineChild = { cboVDEquipment };
            combo.SetCombo(cboVDEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "LINEBYSHOP");
        }

        #endregion

        #region Event

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        private void cboVDEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

            // 설비
            String[] sFilter1 = { Process.VD_ELEC, Util.NVC(cboVDEquipmentSegment.SelectedValue) };
            C1ComboBox[] cboEquipmentParent = { cboVDEquipmentSegment };
            combo.SetCombo(cboVDEquipment, CommonCombo.ComboStatus.ALL, sFilter: sFilter1);
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }
            if (cboVDEquipmentSegment.SelectedIndex < 0 || cboVDEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageInfo("라인을 선택하세요."); //라인을 선택하세요.
                return;
            }

            SearchData();
        }

        #endregion

        #region Funct
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void SearchData()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgSearch);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FROM_DATE", typeof(string));
                IndataTable.Columns.Add("TO_DATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("PJT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQSGID"] = Util.GetCondition(cboVDEquipmentSegment, bAllNull: true);
                Indata["PROCID"] = Process.VD_ELEC;
                Indata["EQPTID"] = Util.GetCondition(cboVDEquipment, bAllNull: true);
                Indata["LOTID"] = Util.NVC(txtLotID.Text).Equals(string.Empty) ? null : Util.NVC(txtLotID.Text);
                Indata["MODLID"] = Util.NVC(txtModel.Text).Equals(string.Empty) ? null : Util.NVC(txtModel.Text);
                Indata["PJT"] = Util.NVC(txtPJT.Text).Equals(string.Empty) ? null : Util.NVC(txtPJT.Text);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_LOSS_LIST", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgSearch, dtMain, FrameOperation);
                    string[] sColumnName = new string[] { "PANCAKE_GR_ID" };
                    _Util.SetDataGridMergeExtensionCol(dgSearch, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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
        #endregion
    }
}

