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
            // ���� Combo
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

            // Validation ����1:Check�� ���� 2���� ó������
            if (_Util.GetDataGridCheckCnt(dgBoxMapping, "CHK") > 2)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("�ִ� 2�� LOT���� ���ð����մϴ�."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            // ��ȸ������� Check�� ������ ó���ϱ� ���� ����
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

            // Validation ����2:������ ���� ��� ���ܵǵ��� Validation
            if (iCheckCount == 2)
            {
                if (!sTempProdName_1.Equals(sTempProdName_2))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("������ ���� �ƴմϴ�.��Ȯ�� ���ּ���."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validation ����3:ó���� ������ ������ ���, PackingNo �Է¿��� Ȯ��
            // �ʿ� ���� ���̴µ�....
            //if (iCheckCount ==0)
            //{
                //GetPackingCardSubCheckQuery
            //}

            if (iCheckCount == 1)
            {
                //GetPackingCardSubCheckQuery
                // LOT ID sTempLot_1 �� �̹� ó���� ������ �����մϴ�. 
            }
            else if (iCheckCount == 2)
            {
                //GetPackingCardSubCheckMergeQuery
                // LOT ID sTempLot_1 �� �̹� ó���� ������ �����մϴ�. 
                // LOT ID sTempLot_2 �� �̹� ó���� ������ �����մϴ�. 


                //�ߺ� Merge ���� �ȵǵ��� Check: �Է�LOT split �̷� Ȯ��
            }

            // ����ī�� �����ϱ� ��, Lot Merger���� �ƴ��� Ȯ��
            //    ����:LOT 1�� ����, LOT 2�� ���ÿ� ���� ����ī�� ó������ �б���
            //        :iCheckCount 0�̸� ����ī�� ����ȸ
            //        :iCheckCount 1�̸� �Ѱ� LOTó��
            //        :iCheckCount 2�̸� �ΰ��� LOTó��




            // ����ī�� ���� ���� Body ����
            //     ����:imsiCheck = 1 �̸�, LOT1�� ó��
            //     ����:imsiCheck = 2 �̸�, LOT2�� ó��(����ī�� ���� ��, Merge�� ������ �Ǵ���)






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
