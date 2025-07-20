/*************************************************************************************
 Created Date : 2022.04.8
      Creator : 신광희 ELEC003_CONFIRM_USER COPY 하여 사용
   Decription : 실적확정
--------------------------------------------------------------------------------------
 [Change History]
 2024.04.09  양영재 : 실적확정 시 기본값으로 작업자 이름 추가
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_CONFIRM_USER : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _equipmentSegmentCode = string.Empty;
        private string _processCode = string.Empty;
        private string _processName = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _lotID = string.Empty;
        private string _shiftId = string.Empty;
        private string _wrkStrtDttm = string.Empty;
        private string _wrkEndDttm = string.Empty;
        private string _wrkUserId = string.Empty;
        private string _wrkUserName = string.Empty;

        private string _confirmUserId = string.Empty;
        private string _confirmkUserName = string.Empty;

        public string ShiftID
        {
            get { return _shiftId; }
        }
        public string ConfirmUserID
        {
            get { return _confirmUserId; }
        }
        public string ConfirmkUserName
        {
            get { return _confirmkUserName; }
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

        public ELEC002_CONFIRM_USER()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetControl();
            SetControlVisibility();

            this.Loaded -= C1Window_Loaded;
        }

        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _equipmentSegmentCode = tmps[0] as string;
            _processCode = tmps[1] as string;
            _processName = tmps[2] as string;
            _equipmentCode = tmps[3] as string;
            _equipmentName = tmps[4] as string;
            _lotID = tmps[5] as string;
            _shiftId = tmps[6] as string;
            _wrkStrtDttm = tmps[7] as string;
            _wrkEndDttm = tmps[8] as string;
            _wrkUserId = tmps[9] as string;
            _wrkUserName = tmps[10] as string;

            txtProcess.Text = _processName;
            txtEquipment.Text = _equipmentName;
            txtLotID.Text = _lotID;
            txtShift.Text = _shiftId;
            txtWorker.Text = _wrkUserName;

            if (string.IsNullOrWhiteSpace(_wrkUserId))
            {
                SelectAutoInputWorkUser();
            }
            else
            {
                txtConfirmUserID.Text = _wrkUserId.Split(',')[0].Trim();
                txtConfirmUser.Text = _wrkUserName.Split(',')[0].Trim();
            }
        }
    


        private void SetControlVisibility()
        {
        }

        /// <summary>
        /// 실적확정 작업자
        /// </summary>
        private void txtConfirmUser_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PopupUser();
            }
        }

        private void btnConfirmUser_Click(object sender, RoutedEventArgs e)
        {
            PopupUser();
        }

        /// <summary>
        /// 선택 
        /// </summary>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelectConfirm()) return;

            SsttWorkUser();
            this.DialogResult = MessageBoxResult.OK;
        }

        /// <summary>
        /// 닫기
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 작업자 조회
        /// </summary>
        private void SelectWorkUser(string UserID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHFT_ID", typeof(string));
                inTable.Columns.Add("WRK_STRT_DTTM", typeof(string));
                inTable.Columns.Add("WRK_END_DTTM", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["SHFT_ID"] = _shiftId;
                newRow["WRK_STRT_DTTM"] = _wrkStrtDttm;
                newRow["WRK_END_DTTM"] = _wrkEndDttm;
                newRow["WRK_USERID"] = UserID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE_EQSG_PROC_WRKR_DRB", "INDATA", "RSLTDT", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    txtConfirmUser.Text = dtResult.Rows[0]["WRK_USERNAME"].ToString();
                    txtConfirmUserID.Text = dtResult.Rows[0]["WRK_USERID"].ToString();
                }
                else
                {
                    Util.MessageValidation("SFU7351"); // 월력에 등록된 작업자가 아닙니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region[[Validation]
        private bool ValidationSelectConfirm()
        {
            if (string.IsNullOrWhiteSpace(txtConfirmUserID.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void SsttWorkUser()
        {
            _confirmUserId = txtConfirmUserID.Text;
            _confirmkUserName = txtConfirmUser.Text;
        }

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

        #region [팝업]

        private void PopupUser()
        {
            CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
            popUser.FrameOperation = FrameOperation;

            object[] Parameters = new object[1];
            Parameters[0] = txtConfirmUser.Text;
            C1WindowExtension.SetParameters(popUser, Parameters);

            popUser.Closed += new EventHandler(popUser_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //월력 등록자 체크 제외
                //SelectWorkUser(popup.USERID); 

                txtConfirmUser.Text = popup.USERNAME;
                txtConfirmUserID.Text = popup.USERID;
            }
        }

        #endregion

        private void SelectAutoInputWorkUser()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQSG_PROC_WRKR_DRB", "INDATA", "RSLTDT", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    txtConfirmUser.Text = dtResult.Rows[0]["REAL_WRKR_NAME"].ToString();
                    txtConfirmUserID.Text = dtResult.Rows[0]["USERID"].ToString();
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
