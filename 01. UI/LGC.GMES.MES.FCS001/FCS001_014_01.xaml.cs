/*************************************************************************************
 Created Date : 2020.11.03
      Creator : 김태균
   Decription : Aging 한계시간 초과 현황(Model, Route별)
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.03  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Reflection;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_014_01 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable _dt = new DataTable();


        public FCS001_014_01()
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

        
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _dt = GetDataTableFromObjects(tmps);
            }

            dgfpsList.ItemsSource = DataTableConverter.Convert(_dt);
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

    }
}
