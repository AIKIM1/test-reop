/*************************************************************************************
 Created Date : 2022.09.15
      Creator : 김호선
   Decription : PACK RTLS CMA,BMA 판정 등록
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.15  김호선 : Initial Created.
  
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_149_JUDG_Hist : C1Window, IWorkArea
    {

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_149_JUDG_Hist()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                string lotID = tmps[0].ToString();
                txtSearchLotId.Text = lotID;
                Lot_Hist_Search();
            }
            else
            {
                this.Close();
            }
        }

        #endregion


        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void Lot_Hist_Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtSearchLotId.Text;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_SEL_EM_LOT_JUDG_HIST", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        throw ex;
                    }

                    if (dt.Rows.Count < 1)
                    {
                        Util.Alert("SFU1905");
                        return;
                    }

                    Util.GridSetData(dgJudgHist, dt, FrameOperation);

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}