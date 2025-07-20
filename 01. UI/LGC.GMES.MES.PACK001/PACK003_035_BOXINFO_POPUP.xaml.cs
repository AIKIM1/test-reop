/*************************************************************************************
 Created Date : 2022.12.13
      Creator : 김진수
   Decription : 원자재 조회 화면의 Box 정보 조회 기능
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2022.12.13     김진수       SI         Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

using LGC.GMES.MES.PACK001.Class;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_035_BOXINFO_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrfCode = string.Empty;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_035_BOXINFO_POPUP()
        {
            InitializeComponent();


        }

        Util util = new Util();

        
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                txtMtrlPortID.Text = (string)tmps[0];
                txtMtrlID.Text = (string)tmps[1];
                txtStatcode.Text = (string)tmps[2];

                GetBoxHistoryData();

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        
        #endregion

        #region Mehod

        private void GetBoxHistoryData()
        {
            string bizRuleName = "DA_MTRL_SEL_TB_SFC_PROD_RACK_MTRL_BOX_STCK_OPT";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("MTRL_PORT_ID", typeof(string));
                dtRQSTDT.Columns.Add("MTRLID", typeof(string));
                dtRQSTDT.Columns.Add("REQ_STAT_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["MTRL_PORT_ID"] = txtMtrlPortID.Text;
                drRQSTDT["MTRLID"] = txtMtrlID.Text;
                drRQSTDT["REQ_STAT_CODE"] = txtStatcode.Text;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    Util.GridSetData(this.dgSearchResultList, dtRSLTDT, FrameOperation);
                    //grdMainTp2.FrozenColumnCount = 5;
                    PackCommon.SearchRowCount(ref this.tbSearchListCount, this.dgSearchResultList.GetRowCount());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


    }
}
