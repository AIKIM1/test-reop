/*************************************************************************************
 Created Date : 2024.01.05
      Creator : 김용식
   Decription : Aging 출하 예약 후 기준시간 초과 현황, 비정상 트레이조회에 팝업조회버튼으로 추가
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.05  DEVELOPER : Initial Created.
  
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
            GetAgingOutputOverTime();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetAgingOutputOverTime();
        }
        #endregion

        #region Mehod

        private void GetAgingOutputOverTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_AGING_OUTPUT_TIME_OVER_TRAY_CNT", "RQSTDT", "RSLTDT", dt, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (result.Rows.Count > 0)
                    {
                        dgfpsList.ItemsSource = DataTableConverter.Convert(result);
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
    }
}
