/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_PILOT_DETL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_PILOT_DETL()
        {
            InitializeComponent();
        }

        private void BOX001_PILOT_DETL_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }

        #endregion

        #region Initialize
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            DataTable pilotInfo = (DataTable)tmps[0]; //시생산
            DataTable npilotInfo = (DataTable)tmps[1]; //시생산외

            if (npilotInfo.Rows.Count < 1)  //시생산외 데이터가 없을 경우 Hidden 처리
            {
                nPilot.Visibility = Visibility.Hidden;
            }
            if (pilotInfo.Columns.Contains("POSITION"))  // NO는 위치정보.Parameter로 넘겨받은 테이블에 NO가 있는 경우 column visible처리
            {
                dgPILOT.Columns["POSITION"].Visibility = Visibility.Visible;
                dgNPILOT.Columns["POSITION"].Visibility = Visibility.Visible;
            }
            if (pilotInfo.Columns.Contains("BOXID"))  // Parameter로 넘겨받은 테이블에 BOXID가 있는 경우 column visible처리
            {
                dgPILOT.Columns["BOXID"].Visibility = Visibility.Visible;
                dgNPILOT.Columns["BOXID"].Visibility = Visibility.Visible;
            }

            Util.GridSetData(dgPILOT, pilotInfo, this.FrameOperation);
            Util.GridSetData(dgNPILOT, npilotInfo, this.FrameOperation);
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        
    }
}
