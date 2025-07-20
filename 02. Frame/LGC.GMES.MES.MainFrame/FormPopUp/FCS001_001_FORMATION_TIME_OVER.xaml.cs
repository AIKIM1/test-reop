/*************************************************************************************
 Created Date : 2023.09.25
      Creator : 손동혁
   Decription : 충방전기 작업중 Tray 시간경과 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.09.25  손동혁 : Initial Created.





 
**************************************************************************************/

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

namespace LGC.GMES.MES.MainFrame
{
    public partial class FCS001_001_FORMATION_TIME_OVER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable _dt = new DataTable();


        public FCS001_001_FORMATION_TIME_OVER()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion


        #region Initialize


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _dt = tmps[0] as DataTable;
            }

            troublerist.ItemsSource = DataTableConverter.Convert(_dt);
        }

        #endregion



        #region Mehod

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


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
