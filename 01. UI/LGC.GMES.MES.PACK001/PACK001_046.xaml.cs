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
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_045.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_046 : UserControl
    {        
        DataTable dtMain = new DataTable();      
        private bool _manualCommit = false;
        private DataTable isCreateTable = new DataTable();
        string sBeforeUse_flag = null;

        public PACK001_046()
        {
            InitializeComponent();
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }


        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        //조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();          
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;

            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return;
            }

            DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
            DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

            if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
            {
                //종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return;
            }
            
            dtMain=GetCellInfo(dtStartTime, dtEndTime);

            Util.GridSetData(dgLotList, dtMain, FrameOperation);
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        public DataTable GetCellInfo(DateTime dtStartTime, DateTime dtEndTime)
        {            
            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("STDTTM", typeof(string));
                inTable.Columns.Add("EDDTTM", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));

                DataRow dr = inTable.NewRow();
                if (dtpDateFrom != null && dtpDateTo != null)
                {
                    dr["STDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString() ;
                    dr["EDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString() ;
                    dr["LOTID"] = !string.IsNullOrWhiteSpace(txtLotId.Text.ToUpper()) ? txtLotId.Text.ToUpper() : null;
                    dr["PALLETID"] = !string.IsNullOrWhiteSpace(txtPlltId.Text.ToUpper()) ? txtPlltId.Text.ToUpper() : null;
                }

                if (!string.IsNullOrWhiteSpace(txtLotId.Text)) {
                    dr["STDTTM"] = null;
                    dr["EDDTTM"] = null;
                    dr["LOTID"] = txtLotId.Text.ToUpper();
                }

                if (!string.IsNullOrWhiteSpace(txtPlltId.Text))
                {
                    dr["STDTTM"] = null;
                    dr["EDDTTM"] = null;
                    dr["PALLETID"] = txtPlltId.Text.ToUpper();
                }

                inTable.Rows.Add(dr);                      

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_MST_SMPL_DATA_CLCT_ANODETAPPING", "RQSTDT", "RSLTDT", inTable);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_MST_SMPL_DATA_CLCT_ANODETAPPING", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_MST_SMPL_DATA_CLCT_ANODETAPPING", Logger.MESSAGE_OPERATION_END);
            }
            return result;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
