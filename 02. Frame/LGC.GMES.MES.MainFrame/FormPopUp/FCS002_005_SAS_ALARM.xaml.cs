/*************************************************************************************
 Created Date : 2024.08.26
      Creator : 최성필
   Decription : SAS 송수신 오류 알람
--------------------------------------------------------------------------------------
 [Change History]
  2028.08.26  DEVELOPER :  Initial Created.

  
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
    public partial class FCS002_005_SAS_ALARM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable _dt = new DataTable();


        public FCS002_005_SAS_ALARM()
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

            dgSASList.ItemsSource = DataTableConverter.Convert(_dt);

            if(_dt.Rows.Count > 0)
            {
                for (int i = 0; i< _dt.Rows.Count; i++)
                {    // 이력테이블에 미등록 된 알람인 경우 테이블에 저장
                    if(_dt.Rows[i]["HIST_FLAG"].ToString().Equals("N"))
                    {
                        DataTable dt = new DataTable();
                        dt.TableName = "RQSTDT";
                        dt.Columns.Add("ROUTID", typeof(string));
                        dt.Columns.Add("DAY_GR_LOTID", typeof(string));
                        dt.Columns.Add("ACTDTTM", typeof(string));
                        dt.Columns.Add("MES_CALC_CNT", typeof(string));
                        dt.Columns.Add("TOTL_CNT", typeof(string));
                        dt.Columns.Add("ALARM_PCT", typeof(string));
                        dt.Columns.Add("MMD_SET_PCT", typeof(string));
                        dt.Columns.Add("REL_FLAG", typeof(string));
                        dt.Columns.Add("INSUSER", typeof(string));
                        dt.Columns.Add("INSDTTM", typeof(string));
                        dt.Columns.Add("UPDUSER", typeof(string));
                        dt.Columns.Add("UPDDTTM", typeof(string));

                        DateTime dttm = DateTime.Now;

                        DataRow dr = dt.NewRow();
                        dr["ROUTID"] = _dt.Rows[i]["ROUTID"].ToString();
                        dr["DAY_GR_LOTID"] = _dt.Rows[i]["DAY_GR_LOTID"].ToString();
                        dr["ACTDTTM"] = dttm;
                        dr["MES_CALC_CNT"] = _dt.Rows[i]["CELL_CNT"].ToString();
                        dr["TOTL_CNT"] = _dt.Rows[i]["TOTAL_CNT"].ToString();
                        dr["ALARM_PCT"] = _dt.Rows[i]["PCT"].ToString();
                        dr["MMD_SET_PCT"] = _dt.Rows[i]["MMD_SET_PCT"].ToString();
                        dr["REL_FLAG"] ="N";
                        dr["INSUSER"] = "MES";
                        dr["INSDTTM"] = dttm;
                        dr["UPDUSER"] = "MES";
                        dr["UPDDTTM"] = dttm;
                        dt.Rows.Add(dr);


                        new ClientProxy().ExecuteService("DA_INS_TB_SFC_FORM_SAS_ALARM_HIST_MB", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        });
                    }
                }
            }
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
