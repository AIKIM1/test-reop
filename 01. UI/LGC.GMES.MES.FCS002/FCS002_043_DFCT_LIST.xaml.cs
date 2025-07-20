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
    public partial class FCS002_043_DFCT_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string[] temp = new string[5];
        public FCS002_043_DFCT_LIST()
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
                    if(tmps[i] != null)
                        temp[i] = tmps[i].ToString();
            }
        }

     
        

        #endregion

        #region Mehod


        private void Getlist()
        {
           try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("JUDGGRD", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("HIST_YN", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["FROM_TIME"] = Util.NVC(temp[0]); 
                dr["TO_TIME"] = Util.NVC(temp[1]);
                dr["JUDGGRD"] = Util.NVC(temp[2]);
                dr["PROCID"] = Util.NVC(temp[3]);
              
                if (!string.IsNullOrEmpty(temp[4]))
                    dr["HIST_YN"] = Util.NVC(temp[4]);

                dtRqst.Rows.Add(dr);


                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SELL_ROUTE_INFO_CONDITION_DFCT_MB", "RQSTDT", "RSLTDT", dtRqst);

                dgList.ItemsSource = DataTableConverter.Convert(dtResult);

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
