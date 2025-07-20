/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_200 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        #region Declaration & Constructor 
        public PGM_GUI_200()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            #region Combo Setting
            // 구분 Combo
            //SetCombo_Division();

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "WH_DIVISION" };
            _combo.SetCombo(cboDivision, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { "WH_SHIPMENT" };
            _combo.SetCombo(cboShipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "COMMCODE");

            String[] sFilter3 = { "WH_TYPE" };
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

            #endregion

        }
        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            string sTempLot_1 = string.Empty;
            string sTempLot_2 = string.Empty;
            string sTempProdName_1 = string.Empty;
            string sTempProdName_2 = string.Empty;

            int iCheckCount = 0;
            int imsiCheck = 0;

            // Validation 로직1:Check된 개수 2개만 처리가능
            if (_Util.GetDataGridCheckCnt(dgBoxMapping, "CHK") > 2)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("최대 2개 LOT까지 선택가능합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            // 조회결과에서 Check된 실적만 처리하기 위해 선별
            for (int i = 0; 1 < dgBoxMapping.Rows.Count; i++)
            {
                //(dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                if ((dgBoxMapping.GetCell(i, dgBoxMapping.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked == true)
                {
                    iCheckCount = iCheckCount + 1;
                    if (iCheckCount == 1)
                    {
                        //sTempLot_1 
                        //sTempProdName_1
                    }
                    else if (iCheckCount == 2)
                    {
                        //sTempLot_2 
                        //sTempProdName_2
                    }
                }
            }

            // Validation 로직2:동일한 모델일 경우 제외되도록 Validation
            if (iCheckCount == 2)
            {
                if (!sTempProdName_1.Equals(sTempProdName_2))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동일한 모델이 아닙니다.재확인 해주세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validation 로직3:처리된 실적이 존재할 경우, PackingNo 입력여부 확인
            // 필요 없어 보이는뎅....
            //if (iCheckCount ==0)
            //{
                //GetPackingCardSubCheckQuery
            //}

            if (iCheckCount == 1)
            {
                //GetPackingCardSubCheckQuery
                // LOT ID sTempLot_1 는 이미 처리된 실적이 존재합니다. 
            }
            else if (iCheckCount == 2)
            {
                //GetPackingCardSubCheckMergeQuery
                // LOT ID sTempLot_1 는 이미 처리된 실적이 존재합니다. 
                // LOT ID sTempLot_2 는 이미 처리된 실적이 존재합니다. 


                //중복 Merge 구성 안되도록 Check: 입력LOT split 이력 확인
            }

            // 포장카드 발행하기 전, Lot Merger인지 아닌지 확인
            //    설명:LOT 1개 선택, LOT 2개 선택에 따라서 포장카드 처리로직 분기함
            //        :iCheckCount 0이면 포장카드 재조회
            //        :iCheckCount 1이면 한개 LOT처리
            //        :iCheckCount 2이면 두개개 LOT처리




            // 포장카드 발행 로직 Body 시작
            //     설명:imsiCheck = 1 이면, LOT1개 처리
            //     설명:imsiCheck = 2 이면, LOT2개 처리(포장카드 구성 시, Merge된 것으로 판단함)






        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
