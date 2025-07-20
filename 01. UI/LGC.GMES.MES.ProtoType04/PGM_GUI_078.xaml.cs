/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_078 : UserControl, IWorkArea 
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private PGM_GUI_075_PROCESSINFO window_PROCESSINFO = new PGM_GUI_075_PROCESSINFO();

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        public PGM_GUI_078()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void btnProcessSelect_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.ProtoType04";
            string MAINFORMNAME = "PGM_GUI_075_SELECTPROCESS";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }
        void popup_Closed(object sender, EventArgs e)
        {
            PGM_GUI_075_SELECTPROCESS popup = sender as PGM_GUI_075_SELECTPROCESS;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataRow drSelectedProcess = popup.DrSelectedProcessInfo;

                if (drSelectedProcess != null)
                {
                    string sSelectedProcessID = drSelectedProcess["PROCNAME"].ToString();//drSelectedProcess["PROCID"].ToString();
                    string sSelectedLineID = drSelectedProcess["LINEID"].ToString();

                    window_PROCESSINFO.setProcess(sSelectedLineID, sSelectedProcessID);

                    tbTitle.Text = popup.SSelectedProcessTitle;
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            setProcessInfo();
            TimerSetting();
            setCboAutoSearch();
        }

        private void chkPageFixed_Checked(object sender, RoutedEventArgs e)
        {
            TimerStatus(true);
        }
        private void chkPageFixed_Unchecked(object sender, RoutedEventArgs e)
        {
            TimerStatus(false);
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            setDgProductLot();
        }

        private void btnProdutLotSearch_Click(object sender, RoutedEventArgs e)
        {
            setDgProductLot();
        }

        private void cboAutoSearch_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboAutoSearch.SelectedIndex == 0)
            {
                int iInterval = (int)(cboAutoSearchTime.Value) > 2 ? (int)(cboAutoSearchTime.Value) : 3;
                // Timer Start
                timer.Interval = new TimeSpan(0, 0, 0, iInterval);
                TimerStatus(true);
            }
            else
            {
                if ((bool)chkPageFixed.IsChecked) cboAutoSearch.SelectedIndex = 0;
                else TimerStatus(false);
            }
        }

        private void dgProductLot_SelectionChanged(object sender, C1.WPF.DataGrid.DataGridSelectionChangedEventArgs e)
        {
            
            //MessageBox.Show(Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem, "LOTID")) + "["+ dgProductLot.CurrentCell.Column.DisplayIndex.ToString());

            

            //dgProductLot.sel
            //C1.WPF.DataGrid.DataGridSelectedItemsCollection<C1.WPF.DataGrid.DataGridColumn> test = dgProductLot.Selection.SelectedColumns;
            //test.

            //DataGridCellInfo cellInfo = dgProductLot.
            //DataGridColumn column = cellInfo.Column;
        }

        private void dgProductLot_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (dgProductLot.SelectedItem != null && dgProductLot.CurrentCell != null)
            {
                MessageBox.Show(Util.NVC(DataTableConverter.GetValue(dgProductLot.SelectedItem, "LOTID")) + "[" + dgProductLot.CurrentCell.Column.DisplayIndex.ToString());
            }
        }
        #endregion

        #region Mehod

        private void setProcessInfo()
        {
            if (dgWorkInfo.Children.Count == 0)
            {
                window_PROCESSINFO.PGM_GUI_078 = this;
                dgWorkInfo.Children.Add(window_PROCESSINFO);
            }
        }

        public void TimerStatus(bool bOn)
        {
            if (bOn)
            {
                chkPageFixed.IsChecked = true;
                FrameOperation.PageFixed(true);
                btnProdutLotSearch.IsEnabled = false;
                timer.IsEnabled = true;
                cboAutoSearch.IsEnabled = true;
                cboAutoSearch.SelectedIndex = 0;
            }
            else
            {
                chkPageFixed.IsChecked = false;
                FrameOperation.PageFixed(false);
                btnProdutLotSearch.IsEnabled = true;
                timer.IsEnabled = false;
                cboAutoSearch.IsEnabled = false;
                cboAutoSearch.SelectedIndex = 1;
            }
        }

        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);
        }

        private void setCboAutoSearch()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("CODE", typeof(string));
            
            dt.Rows.Add("Y", "Y");
            dt.Rows.Add("N", "N");
            cboAutoSearch.ItemsSource = dt.Copy().AsDataView();
        }

        private void setDgProductLot()
        {
            DataTable dtProductLotList = new DataTable();
            dtProductLotList.Columns.Add("LOTID", typeof(string));
            dtProductLotList.Columns.Add("KEYPARTLOT", typeof(string));
            dtProductLotList.Columns.Add("PRODNAME", typeof(string));
            dtProductLotList.Columns.Add("CREATEDATE", typeof(DateTime));
            dtProductLotList.Columns.Add("PROCNAME", typeof(string));
            dtProductLotList.Columns.Add("WIPSTATE", typeof(string));

            dtProductLotList.Rows.Add("295B92190RTG7EA40164", "G6R0425L", "B10 LREV_E63_CMA", DateTime.Today, "4호기 CMA 조립" , "작업중");
            dtProductLotList.Rows.Add("295B92190RTG7EA40135", "G6R0399L", "B10 LREV_E63_CMA", DateTime.Today, "4호기 CMA 검사", "작업중");
            dtProductLotList.Rows.Add("295B92190RTG7EA40078", "G6R0247L", "B10 LREV_E63_CMA", DateTime.Today, "수리/재작업 – 통전불량", "작업중");
            dtProductLotList.Rows.Add("295B92190RTG7EA40075", "G5R0241L", "B10 LREV_E63_CMA", DateTime.Today, "4호기 CMA 검사", "작업중");
            
            dgProductLot.ItemsSource = DataTableConverter.Convert(dtProductLotList);

            //dgProductLot.Focus();
            
        }




        #endregion
        
    }
}
