/*************************************************************************************
 Created Date : 2018.10.30
      Creator : INS 김동일K
   Decription : GMES 고도화 - 노칭 공정진척 확정 시 메시지 팝업 추가
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.30  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_001_CONFIRM_MSG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_CONFIRM_MSG : C1Window, IWorkArea
    {
        private bool _bChecked = false;
        private double _dRemain = 0;

        public bool CHECKED
        {
            get { return _bChecked; }
        }

        public double REMAINQTY
        {
            get { return _dRemain; }
        }

        public ASSY001_001_CONFIRM_MSG()
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

                if (tmps != null && tmps.Length >= 4)
                {
                    txtLable.Text = Util.NVC(tmps[0]);
                    if ((bool)tmps[1])
                    {
                        chkBox.Visibility = Visibility.Visible;
                        chkBox.Content = Util.NVC(tmps[2]);
                    }
                    else
                    {
                        chkBox.Visibility = Visibility.Collapsed;
                    }

                    _dRemain = (double)tmps[3];
                }
                else
                {
                    btnOK.Visibility = Visibility.Collapsed;
                }

                ApplyPermissions();                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOK);
            //listAuth.Add(btnLossDfctSave);
            //listAuth.Add(btnPrdChgDfctSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOK();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DoOK()
        {
            if ((bool)chkBox.IsChecked) {
                _bChecked = true;
            }
            else
            {
                _bChecked = false;
            }

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnCancel_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    DoOK();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
