/*************************************************************************************
 Created Date : 2019.03.21
      Creator : 신광희 차장
   Decription : 전극 설비 포트상태 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.03.21  신광희 차장 : Initial Created.  
**************************************************************************************/
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_024.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_024 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        #endregion

        public MCS001_024()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(!CommonVerify.HasDataGridRow(dgElectrodePortList) ) btnSearch_Click(btnSearch, null);

            Loaded -= UserControl_Loaded;
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                double height = expandGrid.RowDefinitions[2].ActualHeight;
                double gridHeight = expandGrid.RowDefinitions[4].ActualHeight;

                expandGrid.RowDefinitions[2].Height = new GridLength(0);
                expandGrid.RowDefinitions[3].Height = new GridLength(0);
                expandGrid.RowDefinitions[4].Height = new GridLength(gridHeight + height + 8);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                expandGrid.RowDefinitions[2].Height = new GridLength(7.4, GridUnitType.Star);
                expandGrid.RowDefinitions[3].Height = new GridLength(8);
                expandGrid.RowDefinitions[4].Height = new GridLength(2.6, GridUnitType.Star);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_PORT_ELTR_MONITORING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("PORTID1", typeof(string));
                inTable.Columns.Add("PORTID2", typeof(string));
                inTable.Columns.Add("PORTID3", typeof(string));
                inTable.Columns.Add("PORTID4", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PORTID1"] = !string.IsNullOrEmpty(txtPortID1.Text) ? txtPortID1.Text : null;
                dr["PORTID2"] = !string.IsNullOrEmpty(txtPortID2.Text) ? txtPortID2.Text : null;
                dr["PORTID3"] = !string.IsNullOrEmpty(txtPortID3.Text) ? txtPortID3.Text : null;
                dr["PORTID4"] = !string.IsNullOrEmpty(txtPortID4.Text) ? txtPortID4.Text : null;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        var dtBinding = result.Copy();
                        dtBinding.Columns.Add(new DataColumn() { ColumnName = "SEQ", DataType = typeof(int) });
                        int rowIndex = 1;

                        foreach (DataRow row in dtBinding.Rows)
                        {
                            row["SEQ"] = rowIndex;
                            rowIndex++;
                        }
                        dtBinding.AcceptChanges();

                        Util.GridSetData(dgElectrodePortList, dtBinding, null, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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



        #endregion


    }
}
