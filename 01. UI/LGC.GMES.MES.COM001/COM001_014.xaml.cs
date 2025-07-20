/*************************************************************************************
 Created Date : 2016.08.18
      Creator : SCPARK
   Decription : 설비 LOSS 관리
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.12.24 손우석 CSR ID 3852388 GMES UI Eqpt.loss 조회 오류 수정 요청의 건 [요청번호]C20181123_52388
  2019.04.29 염규범 CSR ID C20190423_79897 오창 자동차1,2 동 소형 1동 CWA 2동 포함 처리
  2019.07.19 김도형 CSR ID C20190717_43980 CMI 포함 처리
  2020.05.21 김준겸 CSR ID C20200507-000004 작업자 칼럼 추가 요청 건
  2020.05.21 김준겸 CSR ID C20200427-000204 '비고'사이즈 조정시 다음줄로 줄바꿈 처리.
  2020.06.30 김준겸 CSR ID C20200427-000204 '원인,해결조치' 하단 Grid 출력안되는 원인 해결.
  2020.09.02 김준겸 CSR ID C20200728-000321 원인설비 -SELECT- 초기화 설정 (PACK만 적용)
  2022.01.13 안유수 CSR ID C20220113-000221 설비 Loss 등록 화면에서 조회 조건 중 작업 조 선택이 안되는 현상 수정
  2022.08.23 윤지해 CSR ID C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
  2022.08.23 정용석 CSR ID C20220512-000432 [PEE Technology Team] GMES ESWA PACK - Additional pop-up and query to register EQPT Loss details
  2022.12.16 김린겸 CSR ID C20220929-000399 원통형 후공정 설비 loss 기능 개선 요청 //COM001_014 : TabControl 용으로 사용, Tab_Main & Tab_Unit 파일 새로 추가후 기존 코드 복사함
  2023.03.10 김린겸 E20230126-000178 [생산PI] 설비 Loss 등록 시 다중 설비 일괄조회/등록 기능 추가 요청 건
  2023.03.14 김린겸 E20230126-000178 [생산PI] 설비 Loss 등록 시 다중 설비 일괄조회/등록 기능 추가 요청 건, 설비 Loss 등록 (멀티) Tab 소형조립만 적용
  2023.12.19 김대현 E20231208-001776 설비 Loss 등록, 수정 화면 통합
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using System.Globalization;
using System.Windows.Documents;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.COM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private bool _isLoaded = false;

        public COM001_014()
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

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded == false)
            {
                ShowLoadingIndicator();
                DoEvents();

                LGC.GMES.MES.COM001.COM001_014_Tab_Main tab_Main = new LGC.GMES.MES.COM001.COM001_014_Tab_Main();
                tab_Main.FrameOperation = FrameOperation;
                Tab_Main.Content = tab_Main;

                //if ((LoginInfo.CFG_SHOP_ID.ToString().Equals("A010") || LoginInfo.CFG_SHOP_ID.ToString().Equals("F030") || LoginInfo.CFG_SHOP_ID.ToString().Equals("G182"))
                //    && !(LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("P") || LoginInfo.CFG_AREA_ID.ToString().Equals("MA"))
                //    )
                if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("M"))                    
                {
                    LGC.GMES.MES.COM001.COM001_014_Tab_Main_Multi tab_Main_Multi = new LGC.GMES.MES.COM001.COM001_014_Tab_Main_Multi();
                    tab_Main_Multi.FrameOperation = FrameOperation;
                    Tab_Main_Multi.Content = tab_Main_Multi;
                }
                else
                {
                    TabControl.Items.Remove(Tab_Main_Multi);
                }


                if (LoginInfo.CFG_SHOP_ID.ToString().Equals("G182"))
                {
                    LGC.GMES.MES.COM001.COM001_014_Tab_Unit tab_Unit = new LGC.GMES.MES.COM001.COM001_014_Tab_Unit();
                    tab_Unit.FrameOperation = FrameOperation;
                    Tab_Unit.Content = tab_Unit;
                }
                else
                {
                    //Tab_Unit.Visibility = Visibility.Collapsed;
                    TabControl.Items.Remove(Tab_Unit);
                }

                LGC.GMES.MES.COM001.COM001_014_Tab_Reserve tab_Reserve = new LGC.GMES.MES.COM001.COM001_014_Tab_Reserve();
                tab_Reserve.FrameOperation = FrameOperation;
                Tab_Reserve.Content = tab_Reserve;

                COM001_014_Tab_Req_Hist tab_Req_Hist = new COM001_014_Tab_Req_Hist();
                tab_Req_Hist.FrameOperation = FrameOperation;
                Tab_Req_Hist.Content = tab_Req_Hist;

                HiddenLoadingIndicator();
            }

            _isLoaded = true;
            ////this.Loaded -= UserControl_Loaded;
        }

        private void Tab_Main_GotFocus(object sender, RoutedEventArgs e)
        {
            Tab_Main.GotFocus -= Tab_Main_GotFocus;
            //Tab_Unit.GotFocus += Tab_Unit_GotFocus;
            //ClearAll(txtCSTID, txtLOTID, dgMapping);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tabIdx = ((sender as C1TabControl)).SelectedIndex;
            //if (e.Source is C1TabControl) // This is a soultion of those problem.
            //{
            switch (tabIdx)
            {
                case 0:
                    //Util.AlertInfo("설비 LOSS 등록");
                    break;
                case 1:
                    //Util.AlertInfo("설비 LOSS 등록 (멀티)");
                    break;
                case 2:
                    //Util.AlertInfo("설비 LOSS 등록 (호기)");
                    break;
                default:
                    //Util.AlertInfo("Tab: {0}", tabIdx);
                    break;
            }
            //Util.AlertInfo("Tab: {0}", tabIdx);
            //}
        }

        #endregion

        #region Mehod

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

    }
}
