/*************************************************************************************
 Created Date : 2018.01.23
      Creator : 정규환
  Description : 승인자 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.23  정규환 : Initial Created.
  2022.02.13  정용석 : 승인자/참조자 조회로 변경 (관련 Parent Form ID (FCS002_311_REQUEST1_PACK, COM001_REQUEST_YIELD_PACK)
  2023.03.15  LEEHJ  : 소형활성화 MES 복사


**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_311_PACK_PERSON : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private string searchGubun = string.Empty;
        private string userID = string.Empty;
        private string userName = string.Empty;
        private string departmentID = string.Empty;
        private string departmentName = string.Empty;
        #endregion

        #region Constructor
        public FCS002_311_PACK_PERSON()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties...
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string GUBUN
        {
            get { return this.searchGubun; }
        }

        public string USERID
        {
            get { return this.userID; }
        }

        public string USERNAME
        {
            get { return this.userName; }
        }

        public string DEPTID
        {
            get { return this.departmentID; }
        }

        public string DEPTNAME
        {
            get { return this.departmentName; }
        }
        #endregion

        #region Member Function Lists...
        // 로딩 그림 나오게끔 해주는거
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        // Initialize
        private void Initialize()
        {
            object[] arrObject = C1WindowExtension.GetParameters(this);

            if (arrObject != null && arrObject.Length >= 1)
            {
                this.userName = Util.NVC(arrObject[0]);
                this.searchGubun = Util.NVC(arrObject[1]);
            }

            switch (this.searchGubun)
            {
                case "APPROVER":
                    this.txtGubun.Text = ObjectDic.Instance.GetObjectName("승인자");
                    break;
                case "REFERRER":
                    this.txtGubun.Text = ObjectDic.Instance.GetObjectName("참조자");
                    break;
                default:
                    this.txtGubun.Text = ObjectDic.Instance.GetObjectName("사용자");
                    break;
            }

            this.txtUserName.Text = this.userName;

            if (!string.IsNullOrEmpty(this.txtUserName.Text))
            {
                this.SearchProcess();
            }
        }

        // 조회
        private void SearchProcess()
        {
            // Declaration
            DataTable dt = new DataTable();

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                this.DoEvents();
                // 인간 조회
                switch (this.searchGubun)
                {
                    case "APPROVER":
                        dt = this.GetPersonPackApproverUser(this.txtUserName.Text);
                        break;
                    case "REFERRER":
                        dt = this.GetPersonUser(this.txtUserName.Text);
                        break;
                    default:
                        dt = this.GetPersonUser(this.txtUserName.Text);
                        break;
                }

                if (!CommonVerify.HasTableRow(dt))  // 조회된 인간이 없다면
                {
                    Util.MessageValidation("SFU1592");  // 사용자 정보가 없습니다.
                }
                else if (dt.Rows.Count == 1)        // 조회된 인간이 1마리
                {
                    this.userID = dt.AsEnumerable().Select(x => x.Field<string>("USERID")).FirstOrDefault();
                    this.userName = dt.AsEnumerable().Select(x => x.Field<string>("USERNAME")).FirstOrDefault();
                    this.departmentID = dt.AsEnumerable().Select(x => x.Field<string>("DEPTID")).FirstOrDefault();
                    this.departmentName = dt.AsEnumerable().Select(x => x.Field<string>("DEPTNAME")).FirstOrDefault();
                    this.DialogResult = MessageBoxResult.OK;
                }
                else                                // 조회된 인간이 두마리 이상
                {
                    Util.GridSetData(this.dgPerson, dt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 순서도 호출 - 승인자 조회
        private DataTable GetPersonPackApproverUser(string userName)
        {
            string bizRuleName = "DA_PRD_SEL_HOLD_RELEASE_PERSON";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["USERNAME"] = userName;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 참조자 조회
        private DataTable GetPersonUser(string userName)
        {
            string bizRuleName = "DA_BAS_SEL_PERSON_BY_NAME";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("USERNAME", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["USERNAME"] = userName;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchProcess();
            }
        }

        private void dgUserChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            RadioButton radioButton = sender as RadioButton;

            if (radioButton.DataContext == null)
            {
                return;
            }

            if ((bool)radioButton.IsChecked && (radioButton.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int selectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent).Row.Index;

                DataTableConverter.SetValue(this.dgPerson.Rows[selectedIndex].DataItem, "CHK", true);
                this.dgPerson.SelectedIndex = selectedIndex;
                this.userID = DataTableConverter.GetValue(dgPerson.Rows[selectedIndex].DataItem, "USERID").ToString();
                this.userName = DataTableConverter.GetValue(dgPerson.Rows[selectedIndex].DataItem, "USERNAME").ToString();
                this.departmentID = Util.NVC(DataTableConverter.GetValue(dgPerson.Rows[selectedIndex].DataItem, "DEPTID"));
                this.departmentName = Util.NVC(DataTableConverter.GetValue(dgPerson.Rows[selectedIndex].DataItem, "DEPTNAME"));
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgPerson.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return;
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
    }
}