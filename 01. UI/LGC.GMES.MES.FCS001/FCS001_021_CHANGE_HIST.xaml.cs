/*************************************************************************************
 Created Date : 2022.08.18
      Creator : 이정미
   Decription : Tray 수동 변경 이력 
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.18  이정미 : Initial Created
  2023.04.11  권혜정 : [E20230410-000093] 변경 이력 팝업 사이즈 변경

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_021_CHANGE_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private string _sTrayNo;
  
        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_021_CHANGE_HIST()
        {
            InitializeComponent();
        }
        #endregion

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _sTrayNo = tmps[0] as string;
                GetList();
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
        
        private void GetList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _sTrayNo; 

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CHG_HIST_UI", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgCstHist, dtRslt, FrameOperation, false);


                HiddenLoadingIndicator();
               
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }
        
        #endregion
    }
}
