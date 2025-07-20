using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_043_POPUP : C1Window, IWorkArea
    {
        DataTable dtMain = new DataTable();
        string sEQSGID = string.Empty;
        string sPROCID = string.Empty;
        string sEQPTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PACK001_043_POPUP()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    sEQSGID = Util.NVC(tmps[0]);
                    sPROCID = Util.NVC(tmps[1]);
                    sEQPTID = Util.NVC(tmps[2]);
                    btnSearch_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        public DataTable GetPrintInfoHistory(string sEQSGID, string sPROCID, string sEQPTID)
        {
            ShowLoadingIndicator();
            loadingIndicator.Visibility = System.Windows.Visibility.Visible;
            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = sEQSGID;
                newRow["PROCID"] = sPROCID;
                newRow["EQPTID"] = sEQPTID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL_CHG_HIST", "RQSTDT", "RSLTDT", inTable);

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQPT_LABEL_CHG_HIST", ex);
                return result;
            }
            finally
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                Logger.Instance.WriteLine(Logger.OPERATION_C + "DA_PRD_SEL_TB_SFC_EQPT_LABEL_CHG_HIST", Logger.MESSAGE_OPERATION_END);
                HiddenLoadingIndicator();
            }
            return result;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            DoEvents();
            dtMain = GetPrintInfoHistory(sEQSGID, sPROCID, sEQPTID);
            if (dtMain.Rows.Count>0) { 
                eqptname.Text = dtMain.Rows[0][0].ToString() + " > " + dtMain.Rows[0][1].ToString() + " > " + dtMain.Rows[0][2].ToString();
                Util.GridSetData(dgPrintList, dtMain, FrameOperation);
            }
            loadingIndicator.Visibility = Visibility.Collapsed;
            HiddenLoadingIndicator();
        }

        private void dgPrintList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            e.Cancel = false;
        }
    }
}
