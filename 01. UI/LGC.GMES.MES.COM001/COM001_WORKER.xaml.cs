/*************************************************************************************
 Created Date : 2019.09. 25
      Creator : LG CNS 김대근
   Decription : 작업자 조회 화면 신규 생성
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.25  LG CNS 김대근 : 작업자 조회 화면 신규 생성
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_WORKER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_WORKER : C1Window, IWorkArea
    {
        #region [Variables & Constructor]
        private string _searchName = string.Empty;
        private string _areaID = string.Empty;
        private string _procID = string.Empty;
        private string _selectedUserID = string.Empty;
        private string _selectedUserName = string.Empty;

        public string SelectedUserID
        {
            get { return _selectedUserID; }
        }

        public string SelectedUserName
        {
            get { return _selectedUserName; }
        }

        public COM001_WORKER()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitValueFromParent();
            GetWorkerList();
        }

        private void rbWorkerList_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;
            if (rb == null || rb.DataContext == null)
                return;

            DataGridCellPresenter parentCell = rb.Parent as DataGridCellPresenter;
            if (parentCell == null || parentCell.Row == null || parentCell.DataGrid == null)
                return;

            if (rb.IsChecked.HasValue && rb.IsChecked.Value)
            {
                int idx = parentCell.Row.Index;

                _selectedUserID = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "USERID"));
                _selectedUserName = Util.NVC(DataTableConverter.GetValue(parentCell.DataGrid.Rows[idx].DataItem, "USERNAME"));
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region [Init Method]
        private void InitValueFromParent()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            int paramLen = 3;

            if (parameters == null || parameters.Length < paramLen)
                return;

            _searchName = Util.NVC(parameters[0]);
            _areaID = Util.NVC(parameters[1]);
            _procID = Util.NVC(parameters[2]);
        }
        #endregion

        #region [BizCall]
        private void GetWorkerList()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("USERNAME", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["USERNAME"] = string.IsNullOrEmpty(_searchName) ? null : _searchName;
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_BAS_SEL_USER_BY_AREA", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgWorkerList);
                        Util.GridSetData(dgWorkerList, result, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Util Method]
        #endregion
    }
}
