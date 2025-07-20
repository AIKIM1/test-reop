/*************************************************************************************
 Created Date : 2019.03.05
      Creator : INS 김동일K
   Decription : 설비 Loss 이력 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.03.05  INS 김동일K : Initial Created.
  
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
    public partial class COM001_014_CHG_HIST : C1Window, IWorkArea
    {
        private string sEqptid = string.Empty;
        private string sPreLossSeq = string.Empty;

        public COM001_014_CHG_HIST()
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

            if (tmps != null && tmps.Length >= 2)
            {
                sEqptid = Util.NVC(tmps[0]);
                sPreLossSeq = Util.NVC(tmps[1]);
            }

            ApplyPermissions();
            GetHistory();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetHistory()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRE_LOSS_SEQNO", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; 
                newRow["EQPTID"] = sEqptid;
                newRow["PRE_LOSS_SEQNO"] = sPreLossSeq;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_CHG_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
