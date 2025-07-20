/*************************************************************************************
 Created Date : 2016.09.20
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - 이상보고 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.20  INS 김동일K : Initial Created.
  
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

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007_ABNORMAL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007_ABNORMAL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
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

        public ASSY001_007_ABNORMAL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #endregion
    }
}
