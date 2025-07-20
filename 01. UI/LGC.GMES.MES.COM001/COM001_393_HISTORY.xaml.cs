/*************************************************************************************
 Created Date : 2023.12.26
      Creator : 조성근

   Decription : 슬러리 이력 정보조회
--------------------------------------------------------------------------------------
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_393_HISTORY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _LOTID = string.Empty;
        string _OUTPUT_LOTID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_393_HISTORY()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;
            _LOTID = Util.NVC(tmps[0]);
            _OUTPUT_LOTID = Util.NVC(tmps[1]);

            getHistory();

        }
        
      
        #endregion

        #region Validation
        

        private void getHistory()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";                
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("INPUT_LOTID", typeof(string));
                RQSTDT.Columns.Add("OUTPUT_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if(_LOTID != "")  dr["INPUT_LOTID"] = _LOTID;
                if (_OUTPUT_LOTID != "") dr["OUTPUT_LOTID"] = _OUTPUT_LOTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_MIXER_TANK_MOUNT_MTRL_HIST_RM", "RQSTDT", "OUTDATA", RQSTDT);
                Util.GridSetData(dgList, dtResult, FrameOperation, true);
                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

       

        #region 닫기 버튼 이벤트       

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

       
       
    }
}
