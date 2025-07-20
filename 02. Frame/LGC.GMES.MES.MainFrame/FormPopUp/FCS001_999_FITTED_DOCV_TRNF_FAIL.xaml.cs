/*************************************************************************************
 Created Date  : 2024.06.27
      Creator  : 권용섭 (cnsyongsub)
   Decription  : 보정 △OCV 전송실패
   Constraints : DB Table (TB_SFC_LOT_FITTED_DOCV_TRNF), BizRule (DA_SEL_LOT_FITTED_DOCV_TRNF_FAIL_LIST)
--------------------------------------------------------------------------------------
 [Change History]
  2024.06.27   권용섭    : [E20240620-001371] Initialization Create (보정 dOCV 송/수신 실패시 팝업 알림)
**************************************************************************************/
#region Import Library
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
#endregion

namespace LGC.GMES.MES.MainFrame
{
    public partial class FCS001_999_FITTED_DOCV_TRNF_FAIL : C1Window, IWorkArea
    {
        #region 00. Member Variable
        /// <summary> DataTable </summary>
        DataTable dtRQSTDT = new DataTable();
        #endregion 00. Member Variable
        
        #region 01. All Window Event
        #region 01-01. Component Constructor
        /// <summary>
        /// Component Constructor
        /// </summary>
        public FCS001_999_FITTED_DOCV_TRNF_FAIL()
        {
            InitializeComponent();
        }
        #endregion

        #region 01-02. Frame과 상호작용하기 위한 객체
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region 01-03. ComponentOne Window Loaded Event
        /// <summary>
        /// ComponentOne Window Loaded Event
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                dtRQSTDT = tmps[0] as DataTable;
            }
            dgfpsList.ItemsSource = DataTableConverter.Convert(dtRQSTDT);

            // Set DataGrid Column Width Auto Size
            SetDataGridColumnWidthAutoSize();
        }
        #endregion
        #endregion 01. All Window Event

        #region 99. User Defined Method
        #region 99-01. Get DataTable From Objects
        /// <summary>
        /// Get DataTable From Objects
        /// </summary>
        /// <param name="objects">object[]</param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTableFromObjects(object[] objects)
        {
            if (objects != null && objects.Length > 0)
            {
                Type t = objects[0].GetType();
                DataTable dt = new DataTable(t.Name);

                foreach (PropertyInfo pi in t.GetProperties())
                {
                    dt.Columns.Add(new DataColumn(pi.Name));
                }

                foreach (var o in objects)
                {
                    DataRow dr = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        dr[dc.ColumnName] = o.GetType().GetProperty(dc.ColumnName).GetValue(o, null);
                    }
                    dt.Rows.Add(dr);
                }

                return dt;
            }
            return null;
        }
        #endregion

        #region 99-02. Set DataGrid Column Width Auto Size
        /// <summary>
        /// Set DataGrid Column Width Auto Size
        /// </summary>
        private void SetDataGridColumnWidthAutoSize()
        {
            try
            {
                if (dgfpsList != null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn dgColumn in dgfpsList.Columns)
                    {
                        dgfpsList.Columns[dgColumn.Index].Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    dgfpsList.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #endregion 99. User Defined Method
    }
}