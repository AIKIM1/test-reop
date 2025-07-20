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
    public partial class FCS002_221_DFCT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string[] temp = new string[6];
        public FCS002_221_DFCT()
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


            Getlist();

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
             Getlist();
        }
        
        //private void dgAbList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        //{
        //    if (e.Row.HeaderPresenter == null)
        //    {
        //        return;
        //    }

        //    e.Row.HeaderPresenter.Content = null;

        //    TextBlock tb = new TextBlock();

        //    tb.Text = (e.Row.Index + 1 - dgAbList.TopRows.Count).ToString();
        //    tb.VerticalAlignment = VerticalAlignment.Center;
        //    tb.HorizontalAlignment = HorizontalAlignment.Center;
        //    e.Row.HeaderPresenter.Content = tb;
        //}


        #endregion

        #region Mehod


        private void Getlist()
        {
           try
            {
                dgDfctList.Refresh();


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOT_GRD_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = temp[1];
                dr["TO_DATE"] = temp[2];
                dr["ROUTID"] = temp[3];
                dr["PROD_LOTID"] = temp[4];
                dr["SUBLOT_GRD_CODE"] = temp[5];
                dtRqst.Rows.Add(dr);


                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_JUDG_BAD_FACTOR_MB", "RQSTDT", "RSLTDT", dtRqst);

                //dgDfctList.ItemsSource = DataTableConverter.Convert(dtResult);

                Util.GridSetData(dgDfctList, dtResult, FrameOperation, true);
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
