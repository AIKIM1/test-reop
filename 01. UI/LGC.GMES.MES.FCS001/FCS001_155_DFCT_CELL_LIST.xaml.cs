/*************************************************************************************
 Created Date : 2023.10.06
      Creator : LGY
   Decription : CT 작업 실적 불량 CELL 팝업창
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.30  DEVELOPER : Initial Created.



 

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Data;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS001
{  
    public partial class FCS001_155_DFCT_CELL_LIST : C1Window, IWorkArea
    {

        private string LOTID = string.Empty;
        public FCS001_155_DFCT_CELL_LIST()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null) return;
            else
            {
                LOTID = Util.NVC(tmps[0]);        
            }

            GetList(); 
        }
        #endregion

        
        private void GetList()
        {
            try
            {
     
                DataTable dtRqst = new DataTable();
                if (!string.IsNullOrEmpty(LOTID)) dtRqst.Columns.Add("LOTID"); 

                DataRow dr = dtRqst.NewRow();
                if (!string.IsNullOrEmpty(LOTID)) dr["LOTID"] = LOTID;

                dtRqst.Rows.Add(dr);///H

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CT_DFCT_CELLID", "INDATA", "OUTDATA", dtRqst); //K등급 CELL 조회.       
                Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    
    }
}

