/*************************************************************************************
 Created Date : 2021.09.09
      Creator : INS 김동일K
   Decription : C20210817-000221
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.09  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_012_BOX_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_012_BOX_HIST : C1Window, IWorkArea
    {
        public BOX001_012_BOX_HIST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            string sRcvIssID = string.Empty;
            string sPalletID = string.Empty;

            if (tmps.Length >= 2)
            {
                sRcvIssID = Util.NVC(tmps[0]);
                sPalletID = Util.NVC(tmps[1]);
            }

            GetInfo(sRcvIssID, sPalletID);
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetInfo(string sRcvIssID, string sPalletID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sRcvIssID) || string.IsNullOrWhiteSpace(sPalletID)) return;
                
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("RCV_ISS_ID", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));
                
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["RCV_ISS_ID"] = sRcvIssID;
                newRow["BOXID"] = sPalletID;
                
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_PALLET_BOX_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgList, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}
