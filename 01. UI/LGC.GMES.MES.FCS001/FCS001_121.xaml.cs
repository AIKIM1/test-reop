/******************************************************************************************************************
 Created Date : 2023.10.05
      Creator : 이정미
   Decription : Tray 세척기 현황 
------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.10.05  DEVELOPER : Initial Created


******************************************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;




namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_121 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        public FCS001_121()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboBdlgCd, CommonCombo_Form.ComboStatus.ALL, sCase: "BLDG");
        }
  
        #endregion
       
        #region Method
        private void GetList()
        {
            try
            {
                Util.gridClear(dgEqp);
                Util.gridClear(dgTray);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("BLDG_CD", typeof(string));
  
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BLDG_CD"] = Util.GetCondition(cboBdlgCd, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CLEANER_LIST_UI", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgEqp, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayList(string sEqtpId) 
        {
            try
            {
                Util.gridClear(dgTray);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQPTID"] = sEqtpId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CLEAN_HISTORY_BY_EQP_UI", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayLocHist()
        {
            try
            {
                Util.gridClear(dgTrayHist);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = txtTrayID.Text;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_CLEAN_HISTORY_UI", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgTrayHist, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgEqp_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgEqp.GetCellFromPoint(pnt);

            if (cell != null)
            {
                string sEqtpId = Util.NVC(DataTableConverter.GetValue(dgEqp.Rows[cell.Row.Index].DataItem, "EQPTID"));
                GetTrayList(sEqtpId);
            }
        }

        private void btnTrayHistSearch_Click(object sender, RoutedEventArgs e)
        {
            GetTrayLocHist();
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayID.Text)) && (e.Key == Key.Enter))
            {
                GetTrayLocHist();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }
        #endregion
    }
}


