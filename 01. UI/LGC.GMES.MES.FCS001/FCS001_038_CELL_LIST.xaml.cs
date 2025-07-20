/*************************************************************************************
 Created Date : 2021.01.13
      Creator : kang Dong Hee
   Decription : 상대판정 Cell List
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.13  NAME : Initial Created
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

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_038_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sLotId;
        private string sRouteId;
        private string sJudgGrade;

        public string LOTID
        {
            set { sLotId = value; }
            get { return sLotId; }
        }

        public string ROUTEID
        {
            set { sRouteId = value; }
            get { return sRouteId; }
        }

        public string JUDGGRADE
        {
            set { sJudgGrade = value; }
            get { return sJudgGrade; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        public FCS001_038_CELL_LIST()
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
                sRouteId = Util.NVC(tmps[1]);
                sJudgGrade = Util.NVC(tmps[2]);
            }

            //TextBox Setting
            InitText();
            //조회함수
            GetList();
        }

        private void InitText()
        {
            txtLotId.Text = LOTID;
            txtRoute.Text = ROUTEID;
            txtJudgGrd.Text = JUDGGRADE;
        }
        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("FINL_JUDG_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DAY_GR_LOTID"] = LOTID;
                dr["ROUTID"] = ROUTEID;
                dr["FINL_JUDG_CODE"] = JUDGGRADE;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_RJUDG_CELL_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgRjudgCell, dtRslt, FrameOperation, true);
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
