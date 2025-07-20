/*************************************************************************************
 Created Date : 2024.11.19
      Creator : Adira S
   Decription : 반송지시현황 조회 List(pack용)
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_047_TRF_CMD_STATUS_LIST : C1Window, IWorkArea
    {

        #region Declaration & Constructor 

        private string _portid = string.Empty;

        public PACK003_047_TRF_CMD_STATUS_LIST()
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _portid = Util.NVC(tmps[0]);

                SeachData();
            }
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }   
        #endregion

        #region Method
        private void SeachData()
        {
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("PORT_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["PORT_ID"] = _portid;

            RQSTDT.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_SEL_TRF_CMD_BY_TO_PORT", "INDATA", "OUTDATA", RQSTDT);

            if (dtRslt.Rows.Count != 0)
            {
                Util.GridSetData(dgList, dtRslt, FrameOperation);
            }
        }
        #endregion
    }
}