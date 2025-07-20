/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_200_PACKINGCARD_MANUAL : C1Window, IWorkArea
    {
        private string m_Mode = string.Empty;
        private string m_WC_CODE = string.Empty;
        private string m_LotID = string.Empty;
        private string m_ProdType = string.Empty;
        private string m_PackingNo1 = string.Empty;
        private string m_PackingNo2 = string.Empty;
        private string m_TrasferLoc = string.Empty;
        private string m_PkgWay = string.Empty;
        private double m_iLane = 0;
        private double m_dCutMAvg = 0;
        private double m_dCellAvg = 0;
        private double m_iFrameLane1 = 0;
        private double m_iFrameLane2 = 0;
        private double m_dCutM1 = 0;
        private double m_dCutM2 = 0;
        private double m_dCell1 = 0;
        private double m_dCell2 = 0;
        private string m_PackingRemark = string.Empty;
        private string m_ProdFlag = string.Empty;
        private string m_Status = string.Empty; //�������m_Status (�����N ,��� O, ���ϱ���L ,�ǹ�Ȯ��C , ����S)
        private string m_TwinFlag = "N"; //1 Cut 2������ ��� Y
        private Boolean m_bDataBinding = false; //������ ���ε�����. ����ī�� ��������
        private Boolean m_ReprintFlag;
        private string m_Prod_Create_Seq = string.Empty;
        private string m_Prod_Code = string.Empty;
        private double m_dPattern_Conv = 0; //��ȯ��
        private Boolean m_SelectedDataChange = true;
        private string m_ToLoc = string.Empty;

        #region Declaration & Constructor 
        public PGM_GUI_200_PACKINGCARD_MANUAL()
        {
            InitializeComponent();

            Initialize();
        }

        /// <summary>
        ///  Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        #endregion

        #region Initialize
        private void Initialize()
        {

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter3 = { "WH_TYPE" };
            _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
        }
        
        #endregion

        #region Event
        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            //����ī�� ���� ��ư Ŭ���� Validation






        }

        private void cboPackWay_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrintCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnWMSCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string slitter_lot_id1 = string.Empty;
                string slitter_lot_id2 = string.Empty;
                string packing_no1 = string.Empty;
                string packing_no2 = string.Empty;



                
                //WMS�� ���۵��� ���� ���� ������ �ִ��� Ȯ��
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));
                 
                DataRow dr = RQSTDT.NewRow();
                dr[""] = packing_no1;
                dr[""] = packing_no2;


                DataTable TransResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                if (TransResult.Rows.Count > 0)
                {

                }


                RQSTDT.Clear();
                
                
                
                // ���� : 1. ���� ������ ���� üũ / 0 => ���� ������ ���� LOT �Դϴ�.
                //        2. Status üũ / IC => WMS �԰� �Ϸ�Ǿ� ����Ҽ� �����ϴ�. WMS ���� �԰���� �� ��� �����մϴ�.
                //        3. P_LOT_PACKING_CANCEL_NEW ȣ��


                // ���� �������� Ȯ��
                //DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));

                dr = RQSTDT.NewRow();
                //DataRow dr = RQSTDT.NewRow();
                dr[""] = packing_no1;
                dr[""] = packing_no2;

                DataTable PackInfoResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);






            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnChkDel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAllDel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
