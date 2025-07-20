/*************************************************************************************
 Created Date : 2023.12.14
      Creator : 김용식
   Decription : Aging 출하 예약 후 기준시간 초과 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.14  DEVELOPER : Initial Created.
  2024.01.05 김용식          : MainPopup 조회 조건 추가
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
    public partial class FCS001_164_AGING_OUTPUT_TIME_OVER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable _dt = new DataTable();


        public FCS001_164_AGING_OUTPUT_TIME_OVER()
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
                _dt = tmps[0] as DataTable;
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("MAINPOPYN", typeof(string)); // 2024.01.05 Main Popup Flag Add

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["MAINPOPYN"] = "Y"; // 2024.01.05 Main Popup Flag Add
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_OUTPUT_TIME_OVER_TRAY_CNT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    dgfpsList.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
