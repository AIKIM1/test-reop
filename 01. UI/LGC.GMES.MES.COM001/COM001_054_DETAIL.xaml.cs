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
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_054_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public DataTable dtAdd;

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        string tmmp02 = string.Empty;
        string tmmp03 = string.Empty;

        public Boolean bCheck = false;

        public COM001_054_DETAIL()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as string;
            tmmp03 = tmps[2] as string;

            this.Loaded -= Window_Loaded;

            Initialize();

        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            Search_Lot();
        }
        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Search_Lot()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = tmmp01;
                dr["PRODID"] = tmmp02;
                dr["AREAID"] = tmmp03;

                RQSTDT.Rows.Add(dr);

                DataTable Search_Lot = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_STOCK_LOT_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLot_Info, Search_Lot, FrameOperation);

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("PRODID", typeof(String));
                RQSTDT1.Columns.Add("AREAID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = tmmp01;
                dr1["PRODID"] = tmmp02;
                dr1["AREAID"] = tmmp03;

                RQSTDT1.Rows.Add(dr1);

                DataTable Search_Cell = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FCS_STOCK_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                Util.GridSetData(dgCell_Info, Search_Cell, FrameOperation);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        #endregion

        #region Mehod


        #endregion


    }
}
