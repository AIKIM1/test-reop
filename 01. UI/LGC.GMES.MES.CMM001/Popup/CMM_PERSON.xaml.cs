/*************************************************************************************
 Created Date : 2017.03.20
      Creator : 정문교
   Decription : 사용자 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.20  정문교 : Initial Created.
  
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
    public partial class CMM_PERSON : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _UserID = string.Empty;
        private string _UserName = string.Empty;
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

        public string USERID
        {
            get { return _UserID; }
        }
        public string USERNAME
        {
            get { return _UserName; }
        }

        public string DEPTID
        {
            get { return _DeptID; }
        }
        public string DEPTNAME
        {
            get { return _DeptName; }
        }

        public CMM_PERSON()
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
                _UserName = Util.NVC(tmps[0]);
            }

            txtUserName.Text = _UserName;

            if (txtUserName.Text.Trim().Length > 0)
                btnSearch_Click(null, null);
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetUserList();
        }
        #endregion

        #region [요청자명]
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserList();
            }
        }
        #endregion

        #region [요청자 그리드 선택]
        private void dgUserChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") ||
                (rb.DataContext as DataRowView).Row["CHK"].Equals(false)))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTableConverter.SetValue(dgUser.Rows[idx].DataItem, "CHK", true);
                //row 색 바꾸기
                dgUser.SelectedIndex = idx;

                _UserID = DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "USERID").ToString();
                _UserName = DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "USERNAME").ToString();
                _DeptID = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "DEPTID"));
                _DeptName = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[idx].DataItem, "DEPTNAME"));
            }

        }
        #endregion

        #region [선택]
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (dgUser.SelectedIndex < 0)
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

        private void GetUserList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERNAME"] = txtUserName.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgUser, dtResult, FrameOperation);

                if (dtResult != null && dtResult.Rows.Count == 1)
                {
                    _UserID = DataTableConverter.GetValue(dgUser.Rows[0].DataItem, "USERID").ToString();
                    _UserName = DataTableConverter.GetValue(dgUser.Rows[0].DataItem, "USERNAME").ToString();
                    _DeptID = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[0].DataItem, "DEPTID"));
                    _DeptName = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[0].DataItem, "DEPTNAME"));

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