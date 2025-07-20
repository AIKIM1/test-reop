using System;
using System.Collections.Generic;
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


using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Reflection;

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ELEC001
{

    public partial class PGM_GUI_303 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util util = new Util();

        int MAXMERGECOUNT = 3;
        int cnt = 0;
        int checkIndex; //합권취소에서 그리드 클릭시 index저장
        string temp = string.Empty;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_303()
        {
            InitializeComponent();
           // ldpDatePickerFrom.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }



        #endregion


     
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (rdStock.IsChecked == true)
            //{
            //    btnStock.Visibility = Visibility.Visible;
            //    btnReturn.Visibility = Visibility.Hidden;
            //}
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (rdStock.IsChecked == true)
            {
                btnStock.Visibility = Visibility.Visible;
                btnReturn.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStock.Visibility = Visibility.Hidden;
                btnReturn.Visibility = Visibility.Visible;
            }

            initGrid();
        }


        #region[TAB1]
        #region[Event]

        private void rdStock_Click(object sender, RoutedEventArgs e)
        {
            if (rdStock.IsChecked == true)
            {
                btnStock.Visibility = Visibility.Visible;
                btnReturn.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStock.Visibility = Visibility.Hidden;
                btnReturn.Visibility = Visibility.Visible;
            }

        }
        private void rdReturn_Click(object sender, RoutedEventArgs e)
        {
            if (rdStock.IsChecked == true)
            {
                btnStock.Visibility = Visibility.Visible;
                btnReturn.Visibility = Visibility.Hidden;
            }
            else
            {
                btnStock.Visibility = Visibility.Hidden;
                btnReturn.Visibility = Visibility.Visible;
            }
        }
        private void txtLOTID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string lotid = txtLOTID.Text;
                if (lotid.Equals(""))
                {
                    Util.MessageValidation("SFU1366");
                    return;
                }
                SearchData(lotid);
            }
        }


        #endregion

        #region[Method]
        private void SearchData(string lotid)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void initGrid()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgLotInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }

            dgLotInfo.BeginEdit();
            dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            dgLotInfo.EndEdit();
        }
        #endregion

        #endregion

        #region[TAB2]

        #endregion




     

    

    
    }
}
