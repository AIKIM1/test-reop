/*************************************************************************************
 Created Date : 2023.05.26
      Creator : 최성필
   Decription : 수동 예약 현황
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
    public partial class FCS002_001_RESERVE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string[] temp = new string[2];
        public FCS002_001_RESERVE()
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

      
       
        private void dgRcvList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgRsvList.TopRows.Count).ToString();
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
                dgRsvList.Refresh();
                

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANEID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANEID"] = temp[1];
                dtRqst.Rows.Add(dr);


                ShowLoadingIndicator();

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_BOX_RSV_TRAY_MB", "RQSTDT", "RSLTDT", dtRqst);

                dgRsvList.ItemsSource = DataTableConverter.Convert(dtResult);

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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //예약 취소하시겠습니까?
                Util.MessageConfirm("FM_ME_0313", (resultMessage) =>
                {
                    if (resultMessage == MessageBoxResult.OK)
                    {

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "IN_DATA";

                        dtRqst.Columns.Add("EQUIPMENT_ID", typeof(string));
                        dtRqst.Columns.Add("PORT_ID", typeof(string));
                        dtRqst.Columns.Add("DURABLE_ID", typeof(string));
                        dtRqst.Columns.Add("USER_ID", typeof(string));
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));


                        for (int i = 0; i < dgRsvList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRsvList.Rows[i].DataItem, "CHK")).Equals("True") ||
                            Util.NVC(DataTableConverter.GetValue(dgRsvList.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                DataRow drIn = dtRqst.NewRow();
                                drIn["USER_ID"] = LoginInfo.USERID;
                                drIn["EQUIPMENT_ID"] = Util.NVC(DataTableConverter.GetValue(dgRsvList.Rows[i].DataItem, "EQPTID"));
                                drIn["DURABLE_ID"] = Util.NVC(DataTableConverter.GetValue(dgRsvList.Rows[i].DataItem, "RSV_CSTID"));
                                drIn["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgRsvList.Rows[i].DataItem, "PORT_ID"));
                                drIn["SRCTYPE"] = "UI";
                                dtRqst.Rows.Add(drIn);


                            }
                        }

                        if (dtRqst.Rows.Count == 0)
                        {
                            //선택된 데이터가 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0165"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                { }
                            });
                            return;
                        }


                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_ACT_PORT_CANCEL_RESERVE_TRANSFER_CST", "IN_DATA", null, dtRqst);

                        // 취소되었습니다.
                        Util.MessageInfo("SFU1937");
                        Getlist();

                    }
                });

             
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

















        }
    }
}
