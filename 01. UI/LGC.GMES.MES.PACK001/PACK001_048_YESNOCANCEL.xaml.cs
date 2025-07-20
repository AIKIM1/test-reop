/*************************************************************************************
 Created Date : 2019.08.07
      Creator : 염규범
  Description :
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.07  염규범    Initialize
  2021.12.24  정용석    부모 Form에서 넘어온 데이터 가지고 Grid Data 표출 (기존 순서도 호출 제외시킴)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_048_YESNOCANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_048_YESNOCANCEL()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function List
        
        #endregion

        #region Event
        /// <summary>
        /// name         : C1Window_Loaded
        /// desc         : C1Window_Loaded
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] objParam = C1WindowExtension.GetParameters(this);
            if (objParam == null || objParam.Length <= 0)
            {
                return;
            }

            this.RtxtContent.AppendText(objParam[0].ToString());
        }

        /// <summary>
        /// name         : btnClose_Click
        /// desc         : btnClose_Click
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// name         : btnClose_Click
        /// desc         : btnClose_Click
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Yes;
        }

        /// <summary>
        /// name         : btnClose_Click
        /// desc         : btnClose_Click
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.No;
        }

        #endregion
    }
}