/*************************************************************************************
 Created Date : 2016.08.29
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - CUT 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.29  INS 김동일K : Initial Created.
  
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

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_FCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_FCUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _FinalCut = string.Empty;
        private string _ChildSeq = string.Empty;
        private string _SelFianlCut = string.Empty;

        public string FINALCUT
        {
            get { return _SelFianlCut; }
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

        public CMM_FCUT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _FinalCut = Util.NVC(tmps[0]);
                _ChildSeq = Util.NVC(tmps[1]);
            }
            else
            {
                _FinalCut = "";
                _ChildSeq = "";
            }

            if (_ChildSeq.Equals("1"))
            {
                rdoDelCut.Visibility = Visibility.Hidden;
            }

            rdoChangeCut.IsChecked = true;
        }

        private void rdoChangeCut_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void rdoDelCut_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (rdoChangeCut.IsChecked.HasValue && (bool)rdoChangeCut.IsChecked)
            {
                if (_FinalCut.Equals("Y"))
                    _SelFianlCut = "N";
                else
                    _SelFianlCut = "Y";
            }
            else if (rdoDelCut.IsChecked.HasValue && (bool)rdoDelCut.IsChecked)
            {
                _SelFianlCut = "D";
            }
            else
            {
                _SelFianlCut = "";
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _SelFianlCut = "";
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        #endregion


    }
}
