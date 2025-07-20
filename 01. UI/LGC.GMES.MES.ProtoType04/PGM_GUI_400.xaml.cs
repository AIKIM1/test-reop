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
    public partial class PGM_GUI_400 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        private PGM_GUI_200_PACKINGCARD_MERGE window01 = new PGM_GUI_200_PACKINGCARD_MERGE();

        #region Declaration & Constructor 
        public PGM_GUI_400()
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

            dtpDateFrom3.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            dtpDateTo3.SelectedDateTime = (DateTime)System.DateTime.Now;

            #region Combo Setting
            // 구분 Combo
            //SetCombo_Division();

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter1 = { "WH_DIVISION" };
            //_combo.SetCombo(cboDivision, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter2 = { "WH_SHIPMENT" };
            _combo.SetCombo(cboShipment, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");
            _combo.SetCombo(cboShipment2, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");
            _combo.SetCombo(cboShipment3, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");

            String[] sFilter3 = { "WH_TYPE" };
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");
            _combo.SetCombo(cboType2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");
            _combo.SetCombo(cboType3, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            String[] sFilter4 = { "WH_STATUS" };
            _combo.SetCombo(cboStatus2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "COMMCODE");
            _combo.SetCombo(cboStatus3, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "COMMCODE");

            #endregion

        }


        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtLotID.Text == "")
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("LOT ID를 입력해주세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lot 정보 조회
            // 1. 품질 검사 정보 확인



        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOut);
            dgSub.Children.Clear();
        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sTempLot_1 = string.Empty;
                string sTempLot_2 = string.Empty;
                string sTempProdName_1 = string.Empty;
                string sTempProdName_2 = string.Empty;

                int imsiCheck = 0;                // 설명:처리해야할 LOT 개수 판단
                int iCheckCount = 0;

                string sPackingLotType1 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT
                string sPackingLotType2 = "P";    // 포장 LOT 타입. M 이면 수작업 LOT

                // Validation 로직1:Check된 개수 2개만 처리가능
                //if (_Util.GetDataGridCheckCnt(dgOut, "CHK") > 2)
                if (dgOut.Rows.Count > 2)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("최대 2개 LOT까지 선택가능합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (dgOut.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("스캔 데이터가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 조회결과에서 Check된 실적만 처리하기 위해 선별
                for (int i = 0; 1 < dgOut.Rows.Count; i++)
                {
                    //if ((dgOut.GetCell(i, dgOut.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked == true)
                    //{
                    iCheckCount = iCheckCount + 1;
                    if (iCheckCount == 1)
                    {
                        sTempLot_1 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        //sTempProdName_1
                    }
                    else if (iCheckCount == 2)
                    {
                        sTempLot_2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        //sTempProdName_2
                    }
                    //}
                }

                // Validation 로직:동일한 모델일 경우 제외되도록 Validation
                if (sTempProdName_1.CompareTo(sTempProdName_2) == 1)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동일한 모델이 아닙니다. 재확인 해주세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validation 로직 : 처리된 실적이 존재할 경우, PackingNo 입력여부 확인
                // 삭제 처리 - 무조건 스캔 데이터 존재한 후 포장카드 발행 가능


                // Validation 로직 : 처리된 실적이 존재할 경우, 알람창 발생
                if (iCheckCount == 1)
                {


                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이미 처리된 실적입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (iCheckCount == 2)
                {

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이미 처리된 실적입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                #region 09-12 / 이전 버전...
                //string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
                ////string MAINFORMNAME = "PGM_GUI_200_PACKING_CARD";
                //string MAINFORMNAME = "PGM_GUI_200_PACKINGCARD_MERGE";

                //Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
                //Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
                //object obj = Activator.CreateInstance(targetType);

                //IWorkArea workArea = obj as IWorkArea;
                //workArea.FrameOperation = FrameOperation;

                //C1Window popup = obj as C1Window;
                //if (popup != null)
                //{
                //    popup.Closed -= popup_Closed;
                //    popup.Closed += popup_Closed;
                //    popup.ShowModal();
                //    popup.CenterOnScreen();
                //}
                #endregion

                Get_Sub();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Get_Sub()
        {
            if (dgSub.Children.Count == 0)
            {
                window01.PGM_GUI_400 = this;
                dgSub.Children.Add(window01);
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void popup_Closed(object sender, EventArgs e)
        {

        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReturn3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}