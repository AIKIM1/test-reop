/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - 실적확정 모랏 처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_001_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_001_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _SelType = string.Empty;
        private string _Remain_EA = string.Empty;
        private string _Remain_M = string.Empty;

        private string _PopDoubleChk = string.Empty;
        private long Firsttime = 0;

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
        public string POPDOUBLECHK
        {
            get { return _PopDoubleChk; }
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
        public ASSY003_001_CONFIRM()
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
            
            if (One_Click()) // 한번만 터치되도록 한다. ( 이중 터치 실행 방지 )
            {                
                if (rdoWait.IsChecked.HasValue && (bool)rdoWait.IsChecked)
                    _SelType = "W";
                else if (rdoLoss.IsChecked.HasValue && (bool)rdoLoss.IsChecked)
                    _SelType = "L";

                _PopDoubleChk = "YES";
                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                _PopDoubleChk = "NO";
                this.DialogResult = MessageBoxResult.No;
            }
            
        }
        #endregion        
        private bool One_Click()
        {
            long CurrentTime = DateTime.Now.Ticks;
            if (CurrentTime - Firsttime < 1000000) // 0.4초 ( MS에서는 더블클릭 평균 시간을 0.4초로 보는거 같다.)
            {
                Firsttime = CurrentTime;   // 더블클릭 또는 2회(2회, 3회 4회...)클릭 시 실행되지 않도록 함
                return false;   // 더블클릭 됨
            }
            else
            {
                Firsttime = CurrentTime;   // 1번만 실행되도록 함
                return true;   // 더블클릭 아님
            }
        }

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

        #endregion

        #endregion
    }
}
