/*************************************************************************************
 Created Date : 2021.01.26
      Creator : 조영대
   Decription : 반송요청현황 및 이력 - RTD 실행 로그 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.26  조영대 : Initial Created. 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_054_RTD_RUN_LOG : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

        private string logString;


		public MCS001_054_RTD_RUN_LOG()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                logString = Util.NVC(parameters[0]);
                
                ShowList();
            }

        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod
        
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ShowList()
        {

            DataTable dtLog = new DataTable();
            dtLog.Columns.Add("STEP_NO", typeof(string));
            dtLog.Columns.Add("LOGIC", typeof(string));
            dtLog.Columns.Add("RUN_TIME", typeof(string));
            dtLog.Columns.Add("RESULT_CNT", typeof(string));


            string[] rowData = logString.Split(',');
            foreach (string row in rowData)
            {
                try
                {
                    DataRow newRow = dtLog.NewRow();
                    string[] colData = row.Split('|');
                    for (int inx = 0; inx < 4; inx++)
                    {
                        newRow[inx] = colData[inx];
                    }
                    dtLog.Rows.Add(newRow);
                }
                catch
                {
                    // 오류시 그냥 통과.
                }
            }

            //DataView dvLog = dtLog.DefaultView;
            //dvLog.Sort = "STEP_NO";
            //dtLog = dvLog.ToTable();

            dgList.SetItemsSource(dtLog, FrameOperation, false);
        }
        #endregion
        
        
    }
}