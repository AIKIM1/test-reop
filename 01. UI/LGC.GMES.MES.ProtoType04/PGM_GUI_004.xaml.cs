/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public PGM_GUI_004()
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

        //화면내 combo 셋팅
        private void InitCombo()
        {
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboProcess };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent);

            //공정
            C1ComboBox[] cbProcessParent = { cboLine };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null , cbParent: cbProcessParent);
        }

        private void InitGrid()
        {

            Util.gridClear(dgList);
            Util.gridClear(dgListMaterial);
        }
        #endregion

        #region Event
        private void chkdgList_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.CurrentRow.DataItem == null)
                return;

            int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (rowIndex != i)
                    if (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter != null
                        && (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                        (dgList.GetCell(i, dgList.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
            }


            GetWorkorderMaterial(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.CurrentRow.DataItem == null)
                return;

            DataRowView _dRow = dgList.CurrentRow.DataItem as DataRowView;

            SaveData(_dRow[2].ToString());

            
        }

        #endregion

        #region Mehod
        private void SearchData()
        {
            InitGrid();

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("AREA", typeof(string));
            IndataTable.Columns.Add("LINE", typeof(string));
            IndataTable.Columns.Add("PROCESS", typeof(string));
            IndataTable.Columns.Add("PLANDATE", typeof(string));

            string _ValueToMonth = string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime);
            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["AREA"] = cboArea.SelectedValue.ToString();
            Indata["LINE"] = cboLine.SelectedValue.ToString();
            Indata["PROCESS"] = cboProcess.SelectedValue.ToString();
            Indata["PLANDATE"] = _ValueToMonth;
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORDER", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    return;
                }

                dgList.ItemsSource = DataTableConverter.Convert(result);
            });

        }

        private void GetWorkorderMaterial(object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;

            if (rowview == null)
                return;

            Util.gridClear(dgList);

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("WOID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["WOID"] = rowview[2].ToString();
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORDER_MTRL", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                if (ex != null)
                {
                    return;
                }

                dgListMaterial.ItemsSource = DataTableConverter.Convert(result);
            });
        }

        private void SaveData(string ValueToSend)
        {

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("WOID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["WOID"] = ValueToSend;
                    IndataTable.Rows.Add(Indata);

                    string _BizRule = "DA_PRD_REG_CLOSE_WORKORDER"; // "BR_PRD_REG_CLOSE_WORKORDER"; 

                    //DataTable dtResult = new ClientProxy().ExecuteServiceSync(_BizRule, "INDATA", "RSLTDT", IndataTable);

                    new ClientProxy().ExecuteService(_BizRule, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    });


                }
            });
            SearchData();
        }
        #endregion


    }
}
