/*************************************************************************************
 Created Date : 2024.03.07
      Creator : 홍석원
   Decription : Cell 반품 확정 처리 후 처리결과 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.07  홍석원 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_310_RESULT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_310_RESULT()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataTable dtResult = (DataTable)tmps[0];

            Util.GridSetData(dgReturnResult, dtResult, this.FrameOperation); 
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion

        private void BOX001_310_RESULT_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }
    }
}
