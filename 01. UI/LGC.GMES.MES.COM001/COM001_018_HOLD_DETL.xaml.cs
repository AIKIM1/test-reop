/*************************************************************************************
 Created Date : 2018.11.01
      Creator : 고현영
   Decription : 전지 GMES 고도화 - 재공현황 - HOLD LOTID 더블클릭시 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.01  CNS 고현영 : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_216_CST_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_018_HOLD_DETL : C1Window, IWorkArea
    {
        public string lotId;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_018_HOLD_DETL()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= C1Window_Loaded;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                lotId = tmps[0].ToString();
            }
            else
            {
                lotId = "";
            }

            GetHoldHistory(lotId);

            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void GetHoldHistory(string pLotId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pLotId)) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = pLotId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPHOLDHISTORY", "INDATA", "OUTDATA", inTable, (searchRslt, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgHoldHistList, searchRslt, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
    }
}
