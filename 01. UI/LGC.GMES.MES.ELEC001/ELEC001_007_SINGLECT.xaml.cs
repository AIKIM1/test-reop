/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : Single Coater 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  
**************************************************************************************/

using C1.WPF;
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
    public partial class ELEC001_007_SINGLECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _WOID = string.Empty;

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
        public ELEC001_007_SINGLECT()
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
            _WOID = Util.NVC(tmps[1]);

            GetLotList();
        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
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
                //DataRow dtRow = (rb.DataContext as DataRowView).Row;

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
            }
        }
        #endregion

        #region Mehod
        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = _WOID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SINGLE_CT_WIP", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
