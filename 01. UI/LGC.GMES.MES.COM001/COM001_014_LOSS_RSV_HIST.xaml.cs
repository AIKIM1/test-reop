/*************************************************************************************
 Created Date : 2023.09.13
      Creator : 김대현
   Decription : 설비 Loss 사전등록 이력 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.08 김대현 CSR ID E20230811-001215 설비 Loss 사전등록 
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
    /// COM001_014_CHG_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_014_LOSS_RSV_HIST : C1Window, IWorkArea
    {
        private string sEqptid = string.Empty;
        private string sRsvSeq = string.Empty;

        public COM001_014_LOSS_RSV_HIST()
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

            if (tmps != null)
            {
                sRsvSeq = Util.NVC(tmps[0]);
            }
            GetHistory();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("RSV_SEQNO", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; 
                newRow["RSV_SEQNO"] = sRsvSeq;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_RSV_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgHist, searchResult, null, true);

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
