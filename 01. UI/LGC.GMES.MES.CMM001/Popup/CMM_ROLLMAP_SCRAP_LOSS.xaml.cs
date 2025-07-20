/*************************************************************************************
 Created Date : 2021.07.01
      Creator : 신광희
   Decription : ROLLMAP DATA (파단(전극간연결) Loss량 표시)
--------------------------------------------------------------------------------------
 [Change History]
  2021.07.01  : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_ROLLMAP_SCRAP_LOSS : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _equipmentMeasurementCode;
        private string _startPosition;
        private string _endPosition;
        private string _scrapQty;

        public CMM_ROLLMAP_SCRAP_LOSS()
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

        #endregion

        #region Initialize

        private void InitData()
        {

        }


        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitializeControl()
        {

        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _equipmentMeasurementCode = Util.NVC(parameters[0]);
                _startPosition = Util.NVC(parameters[1]);
                _endPosition = Util.NVC(parameters[2]);
                _scrapQty = Util.NVC(parameters[3]);

                txtStart.Value = _startPosition.GetDouble();
                txtScrapQty.Value = _scrapQty.GetDouble();
            }

            InitializeControl();
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw;
            }
        }

        #endregion

        #region Mehod



        #endregion


    }
}
