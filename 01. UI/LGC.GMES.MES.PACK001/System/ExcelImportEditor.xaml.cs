/*************************************************************************************
 Created Date : 2020.01.27
      Creator : 염규범 S
   Decription : 엑셀 Upload 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.27  염규범  : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.PACK001
{
    public partial class ExcelImportEditor : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public delegate void EventFormClosed();
        public event EventFormClosed FormClosed;

        DataTable DATA_GRID;
        
        bool GENERATE_UI = false;

        #endregion

        #region Initialize
        public ExcelImportEditor(DataTable dataGrid, bool generateUI = false)
        {
            InitializeComponent();

            this.DATA_GRID = dataGrid;
            this.GENERATE_UI = generateUI;
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitDataGrid();
        }

        #endregion

        #region Event

        void InitDataGrid()
        {
            DataTable dtSource = this.DATA_GRID.Clone();
            //DataTable dtSource = Util.MakeDataTable(this.DATA_GRID, false);

            if(dtSource.Columns.Count == 0)
            {
                Util.MessageInfo("UpLoad 형태가 없는상태입니다. 개발자에게, 확인 부탁드립니다.");
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
                return;
            }
            

            foreach (DataColumn dColumn in dtSource.Columns)
            {
                Util.SetGridColumnText(dataGrid, null, null, dColumn.ToString(), false, false, false, false, new C1.WPF.DataGrid.DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto), HorizontalAlignment.Center, Visibility.Visible);
            }


            /*
            C1DataGrid sourceGrid = this.DATA_GRID;
            DataView sourceView = sourceGrid.ItemsSource as DataView;
            /*
            if (sourceView == null)
            {
                foreach (DataRow dRow in sourceGrid.Rows)
                {
                    Util.SetGridColumnText(dataGrid, dRow["LOTID"].ToString(), null, dRow["LOTID"].ToString(), false, false, false, false, 100, HorizontalAlignment.Left, Visibility.Visible);
                }

                /*
                MessageBox.Show("ItemsSource has not initialized!");
                return;
               
        }
    
            DataTable cloneSource = sourceView.Table.Clone();

            if (cloneSource.Columns.Contains("CHK"))
            {
                cloneSource.Columns.Remove(cloneSource.Columns["CHK"]);
            }


            this.dataGrid1.Columns.Clear();
            this.dataGrid1.ItemsSource = null;
            this.dataGrid1.FrozenColumnCount = 0;
            this.dataGrid1.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.MultiRange;
                 */

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.FormClosed != null) FormClosed();
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*new LGC.GMES.MMD.Common.ExcelExporter().*/
                new ExcelExporter().Export(dataGrid);
                //Export(this.dataGrid);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void Export(C1DataGrid dataGrid, string defaultFileName = null)
        {
           
        }

        #endregion

        #region Mehod

        #endregion

    }
}
