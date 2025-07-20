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
        private string m_Status = string.Empty; //현재상태m_Status (출고대기N ,출고 O, 출하구성L ,실물확인C , 출하S)
        private string m_TwinFlag = "N"; //1 Cut 2가대일 경우 Y
        private Boolean m_bDataBinding = false; //데이터 바인딩여부. 포장카드 생성여부
        private Boolean m_ReprintFlag;
        private string m_Prod_Create_Seq = string.Empty;
        private string m_Prod_Code = string.Empty;
        private double m_dPattern_Conv = 0; //변환율
        private Boolean m_SelectedDataChange = true;
        private string m_ToLoc = string.Empty;

        #region Declaration & Constructor 
        public PGM_GUI_200_PACKINGCARD_MANUAL()
        {
            InitializeComponent();

            Initialize();
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
            //포장카드 구성 버튼 클릭시 Validation






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



                
                //WMS로 전송되지 않은 예약 정보가 있는지 확인
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
                
                
                
                // 기존 : 1. 실적 정보를 수량 체크 / 0 => 실적 정보가 없는 LOT 입니다.
                //        2. Status 체크 / IC => WMS 입고 완료되어 취소할수 없습니다. WMS 에서 입고취소 후 취소 가능합니다.
                //        3. P_LOT_PACKING_CANCEL_NEW 호출


                // 포장 실적정보 확인
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
