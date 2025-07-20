/*************************************************************************************
 Created Date : 2021.03.17
      Creator : 김길용
   Decription : 물류관리 > Cell 반품승인요청 상세POPUP
--------------------------------------------------------------------------------------
 [Change History]
    Date         Author      CSR         Description...
  2021.03.17   김길용        SI           Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_008_RETURN_PALLETINFO : C1Window, IWorkArea
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

        public PACK003_008_RETURN_PALLETINFO()
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
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    string sCarrierId = Util.NVC(tmps[0]);
                    string sPalletId = Util.NVC(tmps[1]);
                    sTrfCode = Util.NVC(tmps[2]);
                    txtPallet.Text = sPalletId;
                    if (txtPallet.Text.Length > 0)
                    {
                        getPalletInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //private void btnEcel_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}

        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //if (txtPallet.Text.Length > 0)
        //        //{
        //        //    getPalletInfo();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}
        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    if (txtPallet.Text.Length > 0)
                    {
                        
                        getPalletInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod
        
        private void getPalletInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("CSTID", typeof(string));
                RQSTDT.Columns.Add("PLTID", typeof(string));
                RQSTDT.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["CSTID"] = txtCarrier.Text;
                dr["PLTID"] = txtPallet.Text;
                dr["REQ_NO"] = sTrfCode.ToString() == "" ? null : sTrfCode.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOGIS_APPR_REQ_SUMLIST", "RQSTDT", "RSLTDT", RQSTDT);
                Clear();
                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    Util.Alert("101471");  // 조회된 결과가 없습니다.
                    return;
                }
                
                dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
                txtCarrier.Text = dtResult.Rows[0]["CSTID"].ToString();
                txtPallet.Text = dtResult.Rows[0]["PLTID"].ToString();
                txtStat.Text = dtResult.Rows[0]["CMCODE"].ToString() + " : " + dtResult.Rows[0]["CMCNAME"].ToString();
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void Clear()
        {
            this.txtPallet.Text = string.Empty;
            this.txtCarrier.Text = string.Empty;
            this.txtStat.Text = string.Empty;
            this.sTrfCode = string.Empty;
            Util.gridClear(this.dgSearchResultList);
            // 건수표시
            this.tbSearchListCount.Text = "[ 0 건 ]";
        }
        #endregion


    }
}
