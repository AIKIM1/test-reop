/*************************************************************************************
 Created Date : 2018.09.03
      Creator : 홍진수
   Decription : 부서 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.03  홍진수 : Initial Created.
  
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
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_DEPARTMENT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _DeptID = string.Empty;
        private string _DeptName = string.Empty;

        Util _Util = new Util();

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string DEPTID
        {
            get { return _DeptID; }
        }
        public string DEPTNAME
        {
            get { return _DeptName; }
        }

        public CMM_DEPARTMENT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
          
            if (tmps != null && tmps.Length >= 1)
            {
                _DeptName = Util.NVC(tmps[0]);
            }

            txtDeptName.Text = _DeptName;

            if (txtDeptName.Text.Trim().Length > 0)
                btnSearch_Click(null, null);
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDepartmentList();
        }
        #endregion

        #region [부서명]
        private void txtDeptName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetDepartmentList();
            }
        }
        #endregion

        #region [부서 그리드 선택]
        private void dgDeptChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTableConverter.SetValue(dgDept.Rows[idx].DataItem, "CHK", true);
                //row 색 바꾸기
                dgDept.SelectedIndex = idx;

                _DeptID = DataTableConverter.GetValue(dgDept.Rows[idx].DataItem, "CBO_CODE").ToString();
                _DeptName = DataTableConverter.GetValue(dgDept.Rows[idx].DataItem, "CBO_NAME").ToString();
            }

        }
        #endregion

        #region [선택]
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (dgDept.SelectedIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void GetDepartmentList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DEPTNAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DEPTNAME"] = txtDeptName.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DEPARTMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgDept, dtResult, FrameOperation);

                if (dtResult != null && dtResult.Rows.Count == 1)
                {
                    _DeptID = DataTableConverter.GetValue(dgDept.Rows[0].DataItem, "CBO_CODE").ToString();
                    _DeptName = DataTableConverter.GetValue(dgDept.Rows[0].DataItem, "CBO_NAME").ToString();

                    this.DialogResult = MessageBoxResult.OK;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion


    }
}