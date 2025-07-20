/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2023.03.15  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_311_REQUEST_BIZWFLOT_SPLIT_SEARCH : C1Window, IWorkArea
    {
        Util _util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

        }
        #region Initialize
        public FCS002_311_REQUEST_BIZWFLOT_SPLIT_SEARCH()
        {
            InitializeComponent();
        }
        #endregion



        #region [EVENT]


        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion


        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            string sLOTID = txtLOTID.Text;

            DataSet inData = new DataSet();

            DataTable dtRqst = inData.Tables.Add("INDATA");

            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("LOTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = sLOTID;

            dtRqst.Rows.Add(dr);
           
            try
            {
                //Pallet선별처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_BIZWF_LOT", "INDATA", "OUTDATA", (Result, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    DataTable dtRslt = Result.Tables["OUTDATA"];

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1905");
                    }
                    else
                    {
                        Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                    }

                }, inData);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                txtLOTID.Text = "";
            }

        }

    }
}
