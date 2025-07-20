/*************************************************************************************
 Created Date : 2023.01.10
      Creator : 조영대
   Decription : 생산실적 레포트 수량비교 - Lot별 상세 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.09  조영대 : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_069_DETAILS_BY_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtMaster = null;
        string CALDATE_F = string.Empty;
        string CALDATE_T = string.Empty;
        string EQSGID = string.Empty;
        string MDLLOT_ID = string.Empty;
        string PROCID = string.Empty;
        string ROUT_RSLT_GR_CODE = string.Empty;
        string LOTID = string.Empty;
        string INQUIRY_MODE = string.Empty;
        string INQUIRY_ALL = string.Empty;

        public FCS001_069_DETAILS_BY_LOT()
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
            try
            {
                object[] args = C1WindowExtension.GetParameters(this);
                dtMaster = args[0] as DataTable;
                CALDATE_F = Util.NVC(args[1]);
                CALDATE_T = Util.NVC(args[2]);
                EQSGID = Util.NVC(args[3]);
                MDLLOT_ID = Util.NVC(args[4]);
                PROCID = Util.NVC(args[5]);
                ROUT_RSLT_GR_CODE = Util.NVC(args[6]);
                LOTID = Util.NVC(args[7]);
                INQUIRY_MODE = Util.NVC(args[8]);
                INQUIRY_ALL = Util.NVC(args[9]);

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaster_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void dgDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (sender == null) return;
                    if (e.Cell.Presenter == null) return;

                    switch (e.Cell.Row.Type)
                    {
                        case DataGridRowType.Item:
                            if (e.Cell.Column.Name.ToString() == "SUBLOTID")
                            {
                                e.Cell.Presenter.Foreground = Brushes.Blue;
                            }
                            else
                            {
                                if (Util.NVC(dg.GetValue(e.Cell.Row.Index, "DIFF_INPUT_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "DIFF_DFCT_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "EXCEPT_DAY_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "NON_RPT_RSLT_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "LQC_REQ_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "PQC_REQ_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "DAY_DFCT_YN")).Equals("Y") ||
                                    Util.NVC(dg.GetValue(e.Cell.Row.Index, "RPT_DFCT_YN")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Foreground = Brushes.Red;
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = Brushes.Black;
                                    e.Cell.Presenter.FontWeight = FontWeights.Regular;
                                }
                            }
                            break;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDetail_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Column.Name.Equals("SUBLOTID"))
                {
                    //Open Cell Form
                    string sCellId = cell.Text;
                    FCS001_022 fcs022 = new FCS001_022();
                    fcs022.FrameOperation = FrameOperation;

                    object[] parameters = new object[2];
                    parameters[0] = Util.NVC(sCellId);
                    parameters[1] = "Y";

                    this.FrameOperation.OpenMenuFORM("SFU010710020", "FCS001_022", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Cell 정보조회"), true, parameters);
                }
            }
        }

        #endregion

        #region Mehod


        private void GetList()
        {
            try
            {
                dgMaster.ClearRows();
                dgDetail.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CALDATE_F", typeof(string));         //생산일자 FROM
                dtRqst.Columns.Add("CALDATE_T", typeof(string));         //조회기간 TO
                dtRqst.Columns.Add("EQSGID", typeof(string));            //생산라인
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));         //모델
                dtRqst.Columns.Add("SHFT_ID", typeof(string));           //작업조
                dtRqst.Columns.Add("PROCID", typeof(string));            //공정ID
                dtRqst.Columns.Add("LOTTYPE", typeof(string));           //LOT유형
                dtRqst.Columns.Add("ROUT_GR_CODE", typeof(string));      //라우트 그룹코드
                dtRqst.Columns.Add("ROUT_RSLT_GR_CODE", typeof(string)); //라우트 실적그룹 코드 (N 제외)
                dtRqst.Columns.Add("LOTID", typeof(string));             //TRAY LOT ID
                dtRqst.Columns.Add("CELLID", typeof(string));            //CELL ID
                dtRqst.Columns.Add("INQUIRY_MODE", typeof(string));      //조회범위 (LOT / CELL - 팝업 조회시 CELL)
                dtRqst.Columns.Add("INQUIRY_ALL", typeof(string));       //전체조회여부(Y: 전체, N: 차이나는데이터만)
                
                DataRow dr = dtRqst.NewRow();
                dr["CALDATE_F"] = CALDATE_F;
                dr["CALDATE_T"] = CALDATE_T;
                dr["EQSGID"] = EQSGID;
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["PROCID"] = PROCID;
                dr["ROUT_RSLT_GR_CODE"] = ROUT_RSLT_GR_CODE;
                dr["LOTID"] = LOTID;
                dr["INQUIRY_MODE"] = INQUIRY_MODE;
                dr["INQUIRY_ALL"] = INQUIRY_ALL;
                dtRqst.Rows.Add(dr);

                dgMaster.SetItemsSource(dtMaster, FrameOperation, true);

                dgDetail.ExecuteService("BR_GET_PROD_RSLT_RPT_DIFF", "INDATA", "OUTDATA", dtRqst, true);

                if (PROCID.Equals("FFJ101") && ROUT_RSLT_GR_CODE.Equals("R")) // 1차충전 - 재작업
                {
                    dgMaster.Columns["LOTID"].Visibility = Visibility.Collapsed;
                }

                if (PROCID.Equals("FFJ101") || PROCID.Equals("FF1101") || PROCID.Equals("FF6102")) // 1차충전 - 직행, 2차충전, 저전압
                {
                    dgMaster.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["LQC_REQ_YN"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["PQC_REQ_YN"].Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals("FFD101")) // DEGAS
                {
                    dgMaster.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                    dgDetail.Columns["PQC_REQ_YN"].Visibility = Visibility.Collapsed;
                }
                else if (PROCID.Equals("FF5101")) // EOL
                {
                    dgMaster.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
         
    }
}
