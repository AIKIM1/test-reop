/*************************************************************************************
 Created Date : 2017.03.21
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 실적확정 모랏 처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.21  INS 김동일K : Initial Created.
  2018.10.29  INS 김동일K : 확정 시 샘플바코드발행 체크 기능 추가(체크 시 라벨 2장 발행 되도록 수정) 및 이력에 발행 횟수 컬럼 추가
  2023.11.13  문혜림 : E20230826-001656 사용자별 라디오 버튼 기본값 default 값으로 설정 기능 추가
**************************************************************************************/


using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_001_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _SelType = string.Empty;
        private string _Remain_EA = string.Empty;
        private string _Remain_M = string.Empty;
        private bool _bChecked = false;
        private string _Note = string.Empty;

        private Util _Util = new Util();

        public string SELECT_TYPE
        {
            get { return _SelType; }
        }
        public string REMAIN_EA
        {
            get { return _Remain_EA; }
        }
        public string REMAIN_M
        {
            get { return _Remain_M; }
        }
        public bool CHECKED
        {
            get { return _bChecked; }
        }

        public string NOTE
        {
            get { return _Note; }
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
        public ASSY001_001_CONFIRM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 2)
                {
                    _Remain_EA = Util.NVC(tmps[0]);
                    _Remain_M = Util.NVC(tmps[1]);
                }
                else
                {
                    _Remain_EA = "";
                    _Remain_M = "";
                }

                ApplyPermissions();

                txtLable.Text = MessageDic.Instance.GetMessage("SFU1858") + " (" + _Remain_M + " M / " + _Remain_EA + " EA)"; // 잔량이 남았습니다.

                // 2023.11.13 E20230826-001656 문혜림 추가 
                GetRdoUserConf();   
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (rdoWait.IsChecked.HasValue && (bool)rdoWait.IsChecked)
            {
                _SelType = "W";
            }
            else if (rdoLoss.IsChecked.HasValue && (bool)rdoLoss.IsChecked)
            {
                _SelType = "L";
            }

            if ((bool)chkBox.IsChecked)
                _bChecked = true;
            else
                _bChecked = false;

            if (txtRemainNote.Text.Length <= 2000) //WIPACTHISTORY.WIPNOTE의 데이터타입은 NVARCHAR(4000) = 최대 한글 2천자 
            {
                _Note = txtRemainNote.Text;
            }
            else
            {
                Util.MessageValidation("SFU1145"); //입력값이 최대값을 초과하였습니다.
                return;
            }

            // 2023.11.13 E20230826-001656 문혜림 추가
            if (SetRdoUserConf()) this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        #endregion

        #region [Validation]

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            //listAuth.Add(btnLossDfctSave);
            //listAuth.Add(btnPrdChgDfctSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        // 2023.11.13 E20230826-001656 문혜림 사용자가 선택한 라디오 버튼 default 값으로 저장
        private bool SetRdoUserConf()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));
            dtRqst.Columns.Add("USER_CONF01", typeof(string));
            dtRqst.Columns.Add("USER_CONF02", typeof(string));
            dtRqst.Columns.Add("USER_CONF03", typeof(string));
            dtRqst.Columns.Add("USER_CONF04", typeof(string));
            dtRqst.Columns.Add("USER_CONF05", typeof(string));
            dtRqst.Columns.Add("USER_CONF06", typeof(string));
            dtRqst.Columns.Add("USER_CONF07", typeof(string));
            dtRqst.Columns.Add("USER_CONF08", typeof(string));
            dtRqst.Columns.Add("USER_CONF09", typeof(string));
            dtRqst.Columns.Add("USER_CONF10", typeof(string));

            DataRow drNew = dtRqst.NewRow();
            drNew["WRK_TYPE"] = "SAVE";
            drNew["USERID"] = LoginInfo.USERID;
            drNew["CONF_TYPE"] = "USER_CONFIG_RDO";
            drNew["CONF_KEY1"] = this.ToString(); // LGC.CMES.MES.ASSY001.ASSY001_001_CONFIRM
            drNew["CONF_KEY2"] = rdoWait.GroupName.ToString(); // rdoRemain
            drNew["CONF_KEY3"] = LoginInfo.USERID;
            drNew["USER_CONF02"] = string.Empty;
            drNew["USER_CONF03"] = string.Empty;
            drNew["USER_CONF04"] = string.Empty;
            drNew["USER_CONF05"] = string.Empty;
            drNew["USER_CONF06"] = string.Empty;
            drNew["USER_CONF07"] = string.Empty;
            drNew["USER_CONF08"] = string.Empty;
            drNew["USER_CONF09"] = string.Empty;
            drNew["USER_CONF10"] = string.Empty;

            // 사용자가 선택한 라디오 버튼 값 저장
            if (rdoWait.IsChecked.HasValue && (bool)rdoWait.IsChecked) drNew["USER_CONF01"] = "rdoWait";   
            else if (rdoLoss.IsChecked.HasValue && (bool)rdoLoss.IsChecked) drNew["USER_CONF01"] = "rdoLoss";

            dtRqst.Rows.Add(drNew);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
            if (dtResult != null && dtResult.Rows.Count > 0) return true;

            return false;
        }

        // 2023.11.13 E20230826-001656 문혜림 사용자가 설정한 라디오 버튼 값 load
        private void GetRdoUserConf()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["WRK_TYPE"] = "SELECT";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_RDO";
            dr["CONF_KEY1"] = this.ToString(); // LGC.CMES.MES.ASSY001.ASSY001_001_CONFIRM
            dr["CONF_KEY2"] = rdoWait.GroupName.ToString(); // rdoRemain 
            dr["CONF_KEY3"] = LoginInfo.USERID;
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow drConf in dtResult.Rows)
                {
                    // 사용자의 라디오 버튼 설정 값에 따라 default 값 다르게 표시
                    if (drConf["USER_CONF01"].Equals("rdoWait")) rdoWait.IsChecked = true;
                    else if (drConf["USER_CONF01"].Equals("rdoLoss")) rdoLoss.IsChecked = true;
                }
            }
        }
        #endregion

        #endregion
    }
}
