/*************************************************************************************
 Created Date : 2017.09.28
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Tray, Cell  정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017. 09. 28  Lee. D. R : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_REWORK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_015_CELLID : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY003_015_CELLID()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtCell.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod


        #endregion

        #region Button Event
        private void txtCell_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clear_Form();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = txtCell.Text;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_SUBLOT", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    txtCellID.Text = Util.NVC(SearchResult.Rows[0]["SUBLOTID"]);
                    txtEqpt.Text = Util.NVC(SearchResult.Rows[0]["EQPTNAME"]);
                    txtPjt.Text = Util.NVC(SearchResult.Rows[0]["PRJT_NAME"]);
                    txtLotId.Text = Util.NVC(SearchResult.Rows[0]["PROD_LOTID"]);
                    txtCreateDttm.Text = Util.NVC(SearchResult.Rows[0]["INSDTTM"]);
                    txtTrayID.Text = Util.NVC(SearchResult.Rows[0]["SUB_CSTID"]);
                    txtSlotNo.Text = Util.NVC(SearchResult.Rows[0]["CSTSLOT"]);
                    txtELFiling.Text = Util.NVC(SearchResult.Rows[0]["EL_WEIGHT"]);
                    txtConfirmYN.Text = Util.NVC(SearchResult.Rows[0]["FORM_MOVE_STAT_CODE_NAME"]);
                    txtBefore.Text = Util.NVC(SearchResult.Rows[0]["EL_PRE_WEIGHT"]);
                    txtHeader.Text = Util.NVC(SearchResult.Rows[0]["EL_PSTN"]);
                    txtAfter.Text = Util.NVC(SearchResult.Rows[0]["EL_AFTER_WEIGHT"]);
                    txtJudge.Text = Util.NVC(SearchResult.Rows[0]["EL_JUDG_VALUE"]);
                    txtWorkType.Text = Util.NVC(SearchResult.Rows[0]["WIP_WRK_TYPE_CODE_DESC"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Clear_Form()
        {
            txtCellID.Text = "";
            txtEqpt.Text = "";
            txtPjt.Text = "";
            txtLotId.Text = "";
            txtCreateDttm.Text = "";
            txtTrayID.Text = "";
            txtSlotNo.Text = "";
            txtELFiling.Text = "";
            txtConfirmYN.Text = "";
            txtBefore.Text = "";
            txtHeader.Text = "";
            txtAfter.Text = "";
            txtJudge.Text = "";
        }
        #endregion

        


    }
}
