/*************************************************************************************
 Created Date : 2020.01.22
      Creator : 
   Decription : 전공정LOSS 워크오더 Assign
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.22  DEVELOPER : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_REQUEST_YIELD_ASSIGN : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private object[] tmps = null;
        private string _WOID = string.Empty;

        public string WOID
        {
            get { return _WOID; }
        }

        public COM001_035_REQUEST_YIELD_ASSIGN()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            SearchData();
        }
        #endregion

        #region Event

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgSeletedWOList, "CHK");
            if (idx < 0)
            {
                Util.MessageInfo("SFU1651");    //선택된 항목이 없습니다.
                return;
            }

            _WOID = Util.NVC(DataTableConverter.GetValue(dgSeletedWOList.Rows[idx].DataItem, "WOID"));

            this.DialogResult = MessageBoxResult.OK;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }
                dgSeletedWOList.SelectedIndex = idx;
            }
        }
        #endregion

        #region Mehod

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgSeletedWOList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = Util.NVC(tmps[0]);
                Indata["LOTID"] = Util.NVC(tmps[1]);
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_HOLD_WORKORDER_LIST", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        return;
                    }

                    Util.GridSetData(dgSeletedWOList, result, FrameOperation, true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        #endregion
    }
}
