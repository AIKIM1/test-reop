/*************************************************************************************
 Created Date : 2023.05.26
      Creator : 최성필
   Decription : 이상 Rack 목록
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.26  DEVELOPER : Initial Created.





 
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
using System.Reflection;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_012_ABNORMAL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string[] temp = new string[4];
        public FCS002_012_ABNORMAL()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter = { "FORM_AGINGSTATUS_MCC", null, null, "Y", null, null };
            _combo.SetCombo(cboAbnormal, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN_WITH_OPTION", sFilter: sFilter);
        }


        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                for (int i = 0; i < tmps.Length; i++)
                    temp[i] = tmps[i].ToString();
            }
            

            InitCombo();

          
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
             Getlist();
        }
        
        private void dgAbList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgAbList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }


        #endregion

        #region Mehod


        private void Getlist()
        {
           try
            {
                dgAbList.Refresh();
                

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("ABNORMAL_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = "MBUPPER";
                dr["CMCDTYPE"] = "FORM_AGINGSTATUS_MCC";
                dr["ABNORMAL_CD"] = Util.GetCondition(cboAbnormal, bAllNull: true); ;
                dtRqst.Rows.Add(dr);


                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_RACK_ABNORMAL_MB", "RQSTDT", "RSLTDT", dtRqst);

                dgAbList.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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
