/*************************************************************************************
 Created Date : 2021.01.13
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 활성화 : 재고 실사
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  조영대 : Initial Created.
  2024.05.24  윤지해 : NERP 대응 프로젝트      차수마감취소(1차만) 버튼 추가 및 차수 추가(2차 이후) 시 ERP 실적 마감 FLAG 확인 후 생성 가능하도록 변경
                       (MB동에서만 사용하며, 사실상 데이터가 생성되지 않아 사용하지 않는 것으로 보임. 차수 추가에 대한 로직은 반영하나, ERP로 데이터 전송하지 않도록 함)
**************************************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_100 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS002_100()
        {
            InitializeComponent();       
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void InitControls()
        {
            winFCS002_101.FrameOperation = FrameOperation;
            winFCS002_102.FrameOperation = FrameOperation;
            winCOM001_077.FrameOperation = FrameOperation;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControls();
            
            ApplyPermissions();
           
        }
        
        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        #endregion
        
    }
}
