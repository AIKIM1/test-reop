/*************************************************************************************
 Created Date : 2020.12.16
      Creator : Kang Dong Hee
   Decription : JIG 불량 셀 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.16  NAME : Initial Created
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_060 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        #endregion

        #region [Initialize]
        public FCS002_060()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting            
            InitCombo();
            //Control Setting
            InitControl();
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE");

            string[] sFilter = { null, null, null, null, null };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE", sFilter: sFilter);

            //DataTable dtOp = new DataTable();

            //dtOp.Columns.Add("CBO_NAME", typeof(string));
            //dtOp.Columns.Add("CBO_CODE", typeof(string));

            //DataRow dr3 = dtOp.NewRow();
            //dr3["CBO_CODE"] = "J99";
            //dr3["CBO_NAME"] = "JIG End";
            //dtOp.Rows.InsertAt(dr3, 0);

            //DataRow dr2 = dtOp.NewRow();
            //dr2["CBO_CODE"] = "J00";
            //dr2["CBO_NAME"] = "JIG Start";
            //dtOp.Rows.InsertAt(dr2, 0);

            //DataRow dr1 = dtOp.NewRow();
            //dr1["CBO_CODE"] = string.Empty;
            //dr1["CBO_NAME"] = "-ALL-";
            //dtOp.Rows.InsertAt(dr1, 0);

            //cboOp.DisplayMember = "CBO_NAME";
            //cboOp.ValueMember = "CBO_CODE";

            //cboOp.DataSource = dtOp.Copy().AsDataView();
            //cboOp.SelectedIndex = 0;

            string[] sFilterRework = { "CJG" };
            ComCombo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterRework, sCase: "CMN");

            cboLane.SelectedIndexChanged += cbo_SelectedIndexChanged;
            cboRoute.SelectedIndexChanged += cbo_SelectedIndexChanged;
            cboOp.SelectedIndexChanged += cbo_SelectedIndexChanged;
            cboGrade.SelectedIndexChanged += cbo_SelectedIndexChanged;


        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgJigDefectCell);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("OP_ID", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));
                dtRqst.Columns.Add("CELL_ID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("LOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd000000");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd235959");
                dr["OP_ID"] = Util.GetCondition(cboOp, bAllNull: true);
                dr["GRADE_CD"] = Util.GetCondition(cboGrade, bAllNull: true);
                dr["CELL_ID"] = Util.GetCondition(txtCellId, bAllNull: true);
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                dr["LOT_ID"] = Util.GetCondition(txtLotId, bAllNull: true);
                dr["ROUTE_ID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("SEL_JIG_FORMATION_BAD_CELL_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgJigDefectCell, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void dgJigDefectCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgJigDefectCell.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("CELL_ID") || !cell.Column.Name.Equals("FROM_TRAY_NO"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("CELL_ID"))
                    {
                        //Open Cell Form
                        string sCellId = cell.Text;
                        object[] parameters = new object[2];
                        parameters[0] = Util.NVC(sCellId);
                        parameters[1] = "Y"; //_sActYN

                        this.FrameOperation.OpenMenu("SFU010710310", true, parameters);
                    }
                    else if (cell.Column.Name.Equals("FROM_TRAY_NO"))
                    {
                        //Tray 정보조회 화면 연계
                        object[] parameters = new object[6];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgJigDefectCell.Rows[cell.Row.Index].DataItem, "CSTID"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgJigDefectCell.Rows[cell.Row.Index].DataItem, "LOTID"));
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = string.Empty;
                        this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgJigDefectCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString() == "CELL_ID" || e.Cell.Column.Name.ToString() == "FROM_TRAY_NO")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void cbo_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSearch_Click(null, null);
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCellId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        #endregion

    }
}
