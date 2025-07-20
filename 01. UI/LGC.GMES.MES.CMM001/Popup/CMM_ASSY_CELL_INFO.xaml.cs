/*************************************************************************************
 Created Date : 2018.05.08
      Creator : 정문교
   Decription : Cell 정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.08  정문교 : Initial Created.
  2019.06.28  정문교 : 공정 및 Cell Map에서 사용하게 수정
  2022.03.29  김광오    C20211115-000251    PI T: STK 공정 단계 추가 BOX mapping cell 정보 완료  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_SCRAP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CELL_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public string FormCellMap { get; set; }             // 공정에서 호출시는 Null, Cell Map에서 호출시는 "Y"

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_CELL_INFO()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetControl();
            SearchProcess();
            Loaded -= C1Window_Loaded;

        }

        private void InitializeControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (FormCellMap != null && FormCellMap.Equals("Y"))
            {
                // Cell Map 화면에서 호출시
                tbLotID.Text = ObjectDic.Instance.GetObjectName("생산 Lot");
                tbCarrierID.Text = ObjectDic.Instance.GetObjectName("Cell ID");
                dgList.Columns["REP_CELL_ID"].Visibility = Visibility.Collapsed;
                dgList.Columns["BOX_SLOT_NO"].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgList.Columns["CELL_ID"].Visibility = Visibility.Collapsed;
            }

            txtLotID.Text = tmps[0].ToString();
            txtCarrierID.Text = tmps[1].ToString();
        }

        private void SetControl()
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        /// <summary>
        /// Cell 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = string.Empty;

                DataTable dtRqst = new DataTable();
                if (FormCellMap != null && FormCellMap.Equals("Y"))
                {
                    bizRuleName = "DA_PRD_SEL_CELLMAP_CELL_INFO_L";

                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("PROD_LOTID", typeof(String));
                    dtRqst.Columns.Add("REP_CELL_ID", typeof(String));
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CELLMAP_CELL_LIST_L";

                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("LOTID", typeof(String));
                    dtRqst.Columns.Add("CSTID", typeof(String));
                }

                DataRow dr = dtRqst.NewRow();
                if (FormCellMap != null && FormCellMap.Equals("Y"))
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROD_LOTID"] = txtLotID.Text;
                    dr["REP_CELL_ID"] = txtCarrierID.Text.Equals("") ? null : txtCarrierID.Text;
                    dtRqst.Rows.Add(dr);
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = txtLotID.Text;
                    dr["CSTID"] = txtCarrierID.Text.Equals("") ? null : txtCarrierID.Text;
                    dtRqst.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}