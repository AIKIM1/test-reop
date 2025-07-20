/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2020.07.15  오화백K : DA_PRD_SEL_WAIT_WIP을 BR_PRD_SEL_WAIT_WIP로 변경


  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_LOTSTART
    /// </summary>
    public partial class ELEC001_001_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _MTRLID = string.Empty;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ELEC001_001_LOTSTART()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _PROCID = Util.NVC(tmps[0]);
            _EQPTID = Util.NVC(tmps[1]);

            GetLotInfo();
        }
        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
            }
        }
        #endregion

        #region Mehod
        private void GetLotInfo()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtEquipment.Text = dtMain.Rows[0]["EQPTNAME"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();

                    GetLotList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                //2020-07-15 오화백 변경
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = _PROCID;
                //2020-07-15 오화백 변경
                Indata["EQPTID"] = _EQPTID;

                IndataTable.Rows.Add(Indata);

                //2020-07-15 오화백 변경
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgLotInfo_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgLotInfo.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }
    }
}
