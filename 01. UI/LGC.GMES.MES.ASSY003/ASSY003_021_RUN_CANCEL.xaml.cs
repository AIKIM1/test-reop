/*************************************************************************************
 Created Date : 2017.12.09
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - 특이작업 - C생산 공정진척 - 작업시작취소
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.09  CNS 고현영S : 생성
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_021_RUN_CANCEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_021_RUN_CANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string _wrkPstnId = "";
        string _wrkPstnName = "";
        string _lotId = "";

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        Report_CProd_Out rptCProdOut;

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

        public ASSY003_021_RUN_CANCEL()
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
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length == 3 )
                {
                    _wrkPstnId = tmps[0].ToString();
                    _wrkPstnName = tmps[1].ToString();
                    _lotId = tmps[2].ToString();
                }
                else
                {
                    _wrkPstnId = "";
                    _wrkPstnName = "";
                    _lotId = "";
                }

                LoadTextBox();

                ApplyPermissions();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();



                this.DialogResult = MessageBoxResult.OK;
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

      
        private void LoadTextBox()
        {
            tbxWrkPstnName.Text = _wrkPstnName;
        }

        #endregion

        #region Method

        #region [BizCall]

        #endregion

        #region [Validation]

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
