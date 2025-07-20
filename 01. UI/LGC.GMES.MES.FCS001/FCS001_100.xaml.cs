/*************************************************************************************
 Created Date : 2021.01.13
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - 활성화 : 재고 실사
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  조영대 : Initial Created.
  2024.05.24  윤지해 : NERP 대응 프로젝트      차수마감취소(1차만) 버튼 추가 및 차수 추가(2차 이후) 시 ERP 실적 마감 FLAG 확인 후 생성 가능하도록 변경
                       (NB 활성화 2동에서만 데이터가 존재하나 대부분 1차수에서 끝나고 2022년도 이후 3회만 2차수 생성. 차수 생성에 대한 동일한 로직 적용하나, ERP로 데이터 전송하지는 않도록 함)
**************************************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_100 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS001_100()
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
            winFCS001_101.FrameOperation = FrameOperation;
            winFCS001_102.FrameOperation = FrameOperation;
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
