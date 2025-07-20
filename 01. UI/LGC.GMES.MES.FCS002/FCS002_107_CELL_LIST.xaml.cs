/*************************************************************************************
 Created Date : 2021.03.23
      Creator : kang Dong Hee
   Decription : Cell ID 상세
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.23  NAME : Initial Created
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_107_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sLotId;
        private string sWipSeq;
        private string sActDtTm;

        public string LOTID
        {
            set { sLotId = value; }
            get { return sLotId; }
        }

        public string WIPSEQ
        {
            set { sWipSeq = value; }
            get { return sWipSeq; }
        }

        public string ACTDTTM
        {
            set { sActDtTm = value; }
            get { return sActDtTm; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        public FCS002_107_CELL_LIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                sLotId = Util.NVC(tmps[0]);
                sWipSeq = Util.NVC(tmps[1]);
                sActDtTm = Util.NVC(tmps[2]);
            }

            //조회함수
            GetList();
        }

        private void dgCellDetall_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgCellDetall.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));
                dtRqst.Columns.Add("ACTDTTM", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = LOTID;
                dr["WIPSEQ"] = WIPSEQ;
                dr["ACTDTTM"] = ACTDTTM;
                dr["ACTID"] = "TRANSFER_SUBLOT";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_ACTHISTORY_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgCellDetall, dtRslt, FrameOperation, true);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

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
