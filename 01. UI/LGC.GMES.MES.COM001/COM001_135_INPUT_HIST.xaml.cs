/*************************************************************************************
 Created Date : 2020.04.27
      Creator : INS 김동일K
   Decription : 수불정보 이상 Data - 투입이력 팝업 [C20200406-000377]
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.27  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_135_INPUT_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_135_INPUT_HIST : C1Window, IWorkArea
    {
        private string _LOTID = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _PROCID = string.Empty;

        public COM001_135_INPUT_HIST()
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length >= 3)
                {
                    _LOTID = Util.NVC(tmps[0]);
                    _WIPSEQ = Util.NVC(tmps[1]);
                    _PROCID = Util.NVC(tmps[2]);
                }

                if (_PROCID.Equals(Process.NOTCHING))
                {
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                }
                else if (_PROCID.Equals(Process.LAMINATION))
                {
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                }
                else if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                }
                else if (_PROCID.Equals(Process.PACKAGING))
                {
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                }

                SetInputHistGridFormat(_PROCID);

                GetInputHistory();

                this.Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetInputHistory()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = _WIPSEQ;
                inTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST_END_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInputHist, searchResult, null, true);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetInputHistGridFormat(string ProcID)
        {
            string sFormat = string.Empty;

            if (ProcID.Equals(Process.NOTCHING))
            {
                sFormat = "###,##0.##";
            }
            else
            {
                sFormat = "###,##0";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgInputHist.Columns["RMN_QTY"])).Format = sFormat;
        }
    }
}
