/*************************************************************************************
 Created Date : 2017.09.28
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Tray, Cell  정보 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017. 09. 28  Lee. D. R : Initial Created.
  
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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_REWORK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_015_TRAYID : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY003_015_TRAYID()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtTrayID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod


        #endregion

        #region Button Event
        private void txtTrayID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgTray);
                Util.gridClear(dgCell);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = txtTrayID.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_OUTLOT_LIST", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgTray, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Grid Click Event
        private void dgTrayChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                //row 색 바꾸기
                dgTray.SelectedIndex = idx;

                Search_Tray_List(DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "CSTID").ToString(), DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "PROD_LOTID").ToString(), DataTableConverter.GetValue(dgTray.Rows[idx].DataItem, "LOTID").ToString());
            }
        }

        private void Search_Tray_List(string sTrayID, string sProdLotID, string sOutLotID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("CSTID", typeof(String));                
                RQSTDT.Columns.Add("OUT_LOTID", typeof(String));
                RQSTDT.Columns.Add("PROD_LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = sTrayID;                
                dr["OUT_LOTID"] = sOutLotID;
                dr["PROD_LOTID"] = sProdLotID;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PKG_INFO_SUBLOT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgCell, DetailResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
