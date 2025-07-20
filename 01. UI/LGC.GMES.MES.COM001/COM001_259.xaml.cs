/*************************************************************************************
 Created Date : 2019.05.20
      Creator : 
   Decription : Pack 출하현황 
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Data;
//using Microsoft.VisualBasic;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_259 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtNote = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_259()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void txtPrjName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion

        #region Mehod

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHIP_DATE", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["SHIP_DATE"] = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                Indata["PRODID"] = Util.NVC(txtProdID.Text.Trim());
                Indata["PRJT_NAME"] = Util.NVC(txtModel.Text.Trim());

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_SHIP_MNT", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, result, FrameOperation, true);
                    string[] sColumnName = new string[] { "SHIP_DATE", "PRODID", "PRJT_NAME", "PRODID", "PRJT_NAME", "PLAN_QTY", "AVAILABLE_QTY", "HOLD_QTY", "SUM_ISS_QTY", "REMAIN_QTY", "DIFF_QTY" };
                    _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
