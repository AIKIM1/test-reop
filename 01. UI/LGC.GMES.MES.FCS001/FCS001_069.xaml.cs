/*************************************************************************************
 Created Date : 2022.12.19
      Creator : 조영대
   Decription : 생산실적 레포트 수량비교
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.19   조영대 : Initial Created
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_069 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        private RadioButton checkedRadio = null;
        private string searchOption = string.Empty;
        #endregion

        #region [Initialize]
        public FCS001_069()
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
            InitCombo();

            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "LINEMODEL", cbParent: cboModelParent);

            cboRouteSet.SetCommonCode("WRKLOG_TYPE_CODE", CommonCombo.ComboStatus.NONE, true);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Today;
            dtpToDate.SelectedDateTime = DateTime.Today;
        }
        #endregion

        #region [Method]

        private void GetList()
        {
            try
            {
                searchOption = string.Empty;

                dgList.ClearRows();

                searchOption = cboRouteSet.GetStringValue() + cboPerfGrp.GetStringValue();

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
                dr["CALDATE_F"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["CALDATE_T"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["EQSGID"] = cboLine.GetBindValue();
                dr["MDLLOT_ID"] = cboModel.GetBindValue();
                dr["PROCID"] = cboRouteSet.GetBindValue("ATTR1");
                dr["ROUT_RSLT_GR_CODE"] = cboPerfGrp.GetBindValue();

                if (searchOption.Equals("AR")) // 1차충전 - 재작업
                {
                    dr["LOTID"] = txtPkgLotId.GetBindValue();
                }
                else
                {
                    dr["LOTID"] = txtTrayLotId.GetBindValue();
                }

                dr["INQUIRY_MODE"] = "LOT";
                dr["INQUIRY_ALL"] = rdoAll.IsChecked.Equals(true) ? "Y" : "N";
                dtRqst.Rows.Add(dr);
                               
                btnSearch.IsEnabled = false;

                dgList.ExecuteService("BR_GET_PROD_RSLT_RPT_DIFF", "INDATA", "OUTDATA", dtRqst, true);

                if (searchOption.Equals("AR")) // 1차충전 - 재작업
                {
                    dgList.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    dgList.Columns["INPUT_QTY_DAY_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["INPUT_QTY_RPT_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DIFF_INPUT_QTY_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["GOOD_QTY_DAY_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["GOOD_QTY_RPT_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DIFF_GOOD_QTY_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DFCT_QTY_DAY_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DFCT_QTY_RPT_LOT"].Visibility = Visibility.Collapsed;
                    dgList.Columns["DIFF_DFCT_QTY_LOT"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgList.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgList.Columns["INPUT_QTY_DAY_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["INPUT_QTY_RPT_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["DIFF_INPUT_QTY_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["GOOD_QTY_DAY_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["GOOD_QTY_RPT_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["DIFF_GOOD_QTY_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["DFCT_QTY_DAY_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["DFCT_QTY_RPT_LOT"].Visibility = Visibility.Visible;
                    dgList.Columns["DIFF_DFCT_QTY_LOT"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]

        private void dtpFromDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            dtpToDate.SelectedDateTime = dtpFromDate.SelectedDateTime;
        }

        private void cboRouteSet_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            switch (cboRouteSet.GetStringValue())
            {
                case "D":
                case "Q":
                    cboPerfGrp.SetCommonCode("ROUT_RSLT_GR_CODE", "CBO_CODE = 'ALL 만선택가능'", CommonCombo.ComboStatus.ALL, true);
                    break;
                default:
                    cboPerfGrp.SetCommonCode("ROUT_RSLT_GR_CODE", "CBO_CODE IN ('0', 'R')", CommonCombo.ComboStatus.NONE, true);
                    break;
            }
            
        }

        private void cboPerfGrp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboRouteSet.GetStringValue().Equals("A") && cboPerfGrp.GetStringValue().Equals("R")) // 1차충전 - 재작업
            {
                tbPkgLotId.Visibility = txtPkgLotId.Visibility = Visibility.Visible;
                tbTrayLotId.Visibility = txtTrayLotId.Visibility = Visibility.Collapsed;
                txtTrayLotId.Text = string.Empty;
            }
            else
            {
                tbPkgLotId.Visibility = txtPkgLotId.Visibility = Visibility.Collapsed;
                tbTrayLotId.Visibility = txtTrayLotId.Visibility = Visibility.Visible;
                txtPkgLotId.Text = string.Empty;
            }

            dgList.ClearRows();
        }

        private void txtTrayLotId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTrayLotId.Text.Trim().Equals(string.Empty))
            {
                if (checkedRadio != null)
                {
                    checkedRadio.IsChecked = true;
                    checkedRadio = null;
                }

                rdoAll.IsEnabled = true;
                rdoTermExists.IsEnabled = true;
            }
            else
            {
                if (checkedRadio == null)
                {
                    if (rdoAll.IsChecked.Equals(true))
                    {
                        checkedRadio = rdoAll;
                    }
                    else
                    {
                        checkedRadio = rdoTermExists;
                    }
                    rdoAll.IsChecked = true;
                }
                rdoAll.IsEnabled = false;
                rdoTermExists.IsEnabled = false;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();           
        }


        private void dgList_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;

            if (dtResult != null)
            {
                dtResult.Columns.Add(new DataColumn("SORT1", typeof(string)));
                dtResult.AsEnumerable().ToList().ForEach(r => r["SORT1"] = Util.NVC(r["CALDATE"]) + Util.NVC(r["EQSGID"]) + Util.NVC(r["MDLLOT_ID"]) + Util.NVC(r["PROCID"]) + Util.NVC(r["PROD_LOTID"]));
                dtResult.Columns.Add(new DataColumn("SORT2", typeof(int)));
                dtResult.AsEnumerable().ToList().ForEach(r => r["SORT2"] = 1);

                foreach (DataRow row in dtResult.Rows)
                {
                    foreach (DataColumn col in dtResult.Columns)
                    {
                        if (!col.DataType.Name.Equals("Decimal")) continue;
                        if (row[col].Equals(DBNull.Value)) row[col] = 0;
                    }
                }

                var subSumData = from subSumRow in dtResult.Copy().AsEnumerable()
                                 group subSumRow by new
                                 {
                                     CALDATE = subSumRow.Field<string>("CALDATE"),
                                     EQSGID = subSumRow.Field<string>("EQSGID"),
                                     MDLLOT_ID = subSumRow.Field<string>("MDLLOT_ID"),
                                     PROCID = subSumRow.Field<string>("PROCID"),
                                     PROD_LOTID = subSumRow.Field<string>("PROD_LOTID")
                                 } into grp
                                 select new
                                 {
                                     ALL = "ALL",
                                     CALDATE = grp.Key.CALDATE,
                                     EQSGID = grp.Key.EQSGID,
                                     MDLLOT_ID = grp.Key.MDLLOT_ID,
                                     PROCID = grp.Key.PROCID,
                                     PROD_LOTID = grp.Key.PROD_LOTID,

                                     PKG_INPUT_QTY_DAY_SUM = grp.Sum(r => r.Field<decimal>("PKG_INPUT_QTY_DAY_SUM")) / grp.Count(),
                                     PKG_INPUT_QTY_RPT_SUM = grp.Sum(r => r.Field<decimal>("PKG_INPUT_QTY_RPT_SUM")) / grp.Count(),
                                     DIFF_PKG_INPUT_QTY_SUM = grp.Sum(r => r.Field<decimal>("DIFF_PKG_INPUT_QTY_SUM")) / grp.Count(),

                                     PKG_GOOD_QTY_DAY_SUM = grp.Sum(r => r.Field<decimal>("PKG_GOOD_QTY_DAY_SUM")) / grp.Count(),
                                     PKG_GOOD_QTY_RPT_SUM = grp.Sum(r => r.Field<decimal>("PKG_GOOD_QTY_RPT_SUM")) / grp.Count(),
                                     DIFF_PKG_GOOD_QTY_SUM = grp.Sum(r => r.Field<decimal>("DIFF_PKG_GOOD_QTY_SUM")) / grp.Count(),

                                     PKG_DFCT_QTY_DAY_SUM = grp.Sum(r => r.Field<decimal>("PKG_DFCT_QTY_DAY_SUM")) / grp.Count(),
                                     PKG_DFCT_QTY_RPT_SUM = grp.Sum(r => r.Field<decimal>("PKG_DFCT_QTY_RPT_SUM")) / grp.Count(),
                                     DIFF_PKG_DFCT_QTY_SUM = grp.Sum(r => r.Field<decimal>("DIFF_PKG_DFCT_QTY_SUM")) / grp.Count(),

                                     INPUT_QTY_DAY_LOT = grp.Sum(r => r.Field<decimal>("INPUT_QTY_DAY_LOT")),
                                     INPUT_QTY_RPT_LOT = grp.Sum(r => r.Field<decimal>("INPUT_QTY_RPT_LOT")),
                                     DIFF_INPUT_QTY_LOT = grp.Sum(r => r.Field<decimal>("DIFF_INPUT_QTY_LOT")),

                                     GOOD_QTY_DAY_LOT = grp.Sum(r => r.Field<decimal>("GOOD_QTY_DAY_LOT")),
                                     GOOD_QTY_RPT_LOT = grp.Sum(r => r.Field<decimal>("GOOD_QTY_RPT_LOT")),
                                     DIFF_GOOD_QTY_LOT = grp.Sum(r => r.Field<decimal>("DIFF_GOOD_QTY_LOT")),

                                     DFCT_QTY_DAY_LOT = grp.Sum(r => r.Field<decimal>("DFCT_QTY_DAY_LOT")),
                                     DFCT_QTY_RPT_LOT = grp.Sum(r => r.Field<decimal>("DFCT_QTY_RPT_LOT")),
                                     DIFF_DFCT_QTY_LOT = grp.Sum(r => r.Field<decimal>("DIFF_DFCT_QTY_LOT"))
                                 };

                if (!searchOption.Equals("AR"))
                {
                    foreach (var data in subSumData)
                    {
                        DataRow newRow = dtResult.NewRow();
                        newRow["CALDATE"] = string.Empty;
                        newRow["EQSGID"] = string.Empty;
                        newRow["MDLLOT_ID"] = string.Empty;
                        newRow["PROCID"] = string.Empty;
                        newRow["PROD_LOTID"] = string.Empty;
                        newRow["SORT1"] = data.CALDATE + data.EQSGID + data.MDLLOT_ID + data.PROCID + data.PROD_LOTID;
                        newRow["SORT2"] = 2;
                        newRow["LOTID"] = ObjectDic.Instance.GetObjectName("소계");

                        newRow["INPUT_QTY_DAY_LOT"] = data.INPUT_QTY_DAY_LOT;
                        newRow["INPUT_QTY_RPT_LOT"] = data.INPUT_QTY_RPT_LOT;
                        newRow["DIFF_INPUT_QTY_LOT"] = data.DIFF_INPUT_QTY_LOT;

                        newRow["GOOD_QTY_DAY_LOT"] = data.GOOD_QTY_DAY_LOT;
                        newRow["GOOD_QTY_RPT_LOT"] = data.GOOD_QTY_RPT_LOT;
                        newRow["DIFF_GOOD_QTY_LOT"] = data.DIFF_GOOD_QTY_LOT;

                        newRow["DFCT_QTY_DAY_LOT"] = data.DFCT_QTY_DAY_LOT;
                        newRow["DFCT_QTY_RPT_LOT"] = data.DFCT_QTY_RPT_LOT;
                        newRow["DIFF_DFCT_QTY_LOT"] = data.DIFF_DFCT_QTY_LOT;
                        dtResult.Rows.Add(newRow);
                    }
                }

                var sumData = from sumRow in subSumData.AsEnumerable()
                              group sumRow by new
                              {
                                  ALL = sumRow.ALL
                              } into grp
                              select new
                              {
                                  PKG_INPUT_QTY_DAY_SUM = grp.Sum(r => r.PKG_INPUT_QTY_DAY_SUM),
                                  PKG_INPUT_QTY_RPT_SUM = grp.Sum(r => r.PKG_INPUT_QTY_RPT_SUM),
                                  DIFF_PKG_INPUT_QTY_SUM = grp.Sum(r => r.DIFF_PKG_INPUT_QTY_SUM),

                                  PKG_GOOD_QTY_DAY_SUM = grp.Sum(r => r.PKG_GOOD_QTY_DAY_SUM),
                                  PKG_GOOD_QTY_RPT_SUM = grp.Sum(r => r.PKG_GOOD_QTY_RPT_SUM),
                                  DIFF_PKG_GOOD_QTY_SUM = grp.Sum(r => r.DIFF_PKG_GOOD_QTY_SUM),

                                  PKG_DFCT_QTY_DAY_SUM = grp.Sum(r => r.PKG_DFCT_QTY_DAY_SUM),
                                  PKG_DFCT_QTY_RPT_SUM = grp.Sum(r => r.PKG_DFCT_QTY_RPT_SUM),
                                  DIFF_PKG_DFCT_QTY_SUM = grp.Sum(r => r.DIFF_PKG_DFCT_QTY_SUM),

                                  INPUT_QTY_DAY_LOT = grp.Sum(r => r.INPUT_QTY_DAY_LOT),
                                  INPUT_QTY_RPT_LOT = grp.Sum(r => r.INPUT_QTY_RPT_LOT),
                                  DIFF_INPUT_QTY_LOT = grp.Sum(r => r.DIFF_INPUT_QTY_LOT),

                                  GOOD_QTY_DAY_LOT = grp.Sum(r => r.GOOD_QTY_DAY_LOT),
                                  GOOD_QTY_RPT_LOT = grp.Sum(r => r.GOOD_QTY_RPT_LOT),
                                  DIFF_GOOD_QTY_LOT = grp.Sum(r => r.DIFF_GOOD_QTY_LOT),

                                  DFCT_QTY_DAY_LOT = grp.Sum(r => r.DFCT_QTY_DAY_LOT),
                                  DFCT_QTY_RPT_LOT = grp.Sum(r => r.DFCT_QTY_RPT_LOT),
                                  DIFF_DFCT_QTY_LOT = grp.Sum(r => r.DIFF_DFCT_QTY_LOT)
                              };


                foreach (var data in sumData)
                {
                    DataRow newRow = dtResult.NewRow();
                    newRow["CALDATE"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["EQSGID"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["MDLLOT_ID"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["PROCID"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["SORT1"] = "Z";
                    newRow["SORT2"] = 3;

                    newRow["PKG_INPUT_QTY_DAY_SUM"] = data.PKG_INPUT_QTY_DAY_SUM;
                    newRow["PKG_INPUT_QTY_RPT_SUM"] = data.PKG_INPUT_QTY_RPT_SUM;
                    newRow["DIFF_PKG_INPUT_QTY_SUM"] = data.DIFF_PKG_INPUT_QTY_SUM;

                    newRow["PKG_GOOD_QTY_DAY_SUM"] = data.PKG_GOOD_QTY_DAY_SUM;
                    newRow["PKG_GOOD_QTY_RPT_SUM"] = data.PKG_GOOD_QTY_RPT_SUM;
                    newRow["DIFF_PKG_GOOD_QTY_SUM"] = data.DIFF_PKG_GOOD_QTY_SUM;

                    newRow["PKG_DFCT_QTY_DAY_SUM"] = data.PKG_DFCT_QTY_DAY_SUM;
                    newRow["PKG_DFCT_QTY_RPT_SUM"] = data.PKG_DFCT_QTY_RPT_SUM;
                    newRow["DIFF_PKG_DFCT_QTY_SUM"] = data.DIFF_PKG_DFCT_QTY_SUM;

                    newRow["INPUT_QTY_DAY_LOT"] = data.INPUT_QTY_DAY_LOT;
                    newRow["INPUT_QTY_RPT_LOT"] = data.INPUT_QTY_RPT_LOT;
                    newRow["DIFF_INPUT_QTY_LOT"] = data.DIFF_INPUT_QTY_LOT;

                    newRow["GOOD_QTY_DAY_LOT"] = data.GOOD_QTY_DAY_LOT;
                    newRow["GOOD_QTY_RPT_LOT"] = data.GOOD_QTY_RPT_LOT;
                    newRow["DIFF_GOOD_QTY_LOT"] = data.DIFF_GOOD_QTY_LOT;

                    newRow["DFCT_QTY_DAY_LOT"] = data.DFCT_QTY_DAY_LOT;
                    newRow["DFCT_QTY_RPT_LOT"] = data.DFCT_QTY_RPT_LOT;
                    newRow["DIFF_DFCT_QTY_LOT"] = data.DIFF_DFCT_QTY_LOT;
                    dtResult.Rows.Add(newRow);
                }

                DataView dvResult = dtResult.DefaultView;
                dvResult.Sort = "SORT1, SORT2";
                dtResult = dvResult.ToTable();

            }
        }

        private void dgList_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                Util _Util = new Util();
                string[] sColumnName = new string[] { "PKG_INPUT_QTY_DAY_SUM", "PKG_INPUT_QTY_RPT_SUM", "DIFF_PKG_INPUT_QTY_SUM", "PKG_GOOD_QTY_DAY_SUM",
                    "PKG_GOOD_QTY_RPT_SUM", "DIFF_PKG_GOOD_QTY_SUM", "PKG_DFCT_QTY_DAY_SUM", "PKG_DFCT_QTY_RPT_SUM", "DIFF_PKG_DFCT_QTY_SUM"};

                if (!searchOption.Equals("AR"))
                {
                    _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICAL);
                }
                else
                {
                    _Util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.NONE);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCellFromPoint(pnt);
                if (cell == null || cell.Column == null || Util.NVC(cell.Value).Equals(string.Empty)) return;
                if (Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE")).Equals(string.Empty)) return;

                if (searchOption.Equals("AR") && cell.Column.Name.Equals("PROD_LOTID"))
                {
                    FCS001_069_DETAILS_BY_LOT wndDetail = new FCS001_069_DETAILS_BY_LOT();
                    wndDetail.FrameOperation = FrameOperation;

                    if (wndDetail != null)
                    {
                        DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                        object[] parameters = new object[10];
                        if (Util.NVC(dgList.GetValue(cell.Row.Index, "LOTID")).Equals(string.Empty))
                        {
                            parameters[0] = dt.AsEnumerable().Where(r =>
                                !DBNull.Value.Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE")).Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID")).Equals(r.Field<string>("EQSGID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID")).Equals(r.Field<string>("MDLLOT_ID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID")).Equals(r.Field<string>("PROCID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROD_LOTID")).Equals(r.Field<string>("PROD_LOTID"))
                            ).CopyToDataTable();
                        }
                        else
                        {
                            parameters[0] = dt.AsEnumerable().Where(r =>
                                !DBNull.Value.Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE")).Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID")).Equals(r.Field<string>("EQSGID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID")).Equals(r.Field<string>("MDLLOT_ID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID")).Equals(r.Field<string>("PROCID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROD_LOTID")).Equals(r.Field<string>("PROD_LOTID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "LOTID")).Equals(r.Field<string>("LOTID"))
                            ).CopyToDataTable();
                        }
                        parameters[1] = Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE"));
                        parameters[2] = Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE"));
                        parameters[3] = Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID"));
                        parameters[4] = Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID"));
                        parameters[5] = Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID"));
                        parameters[6] = cboPerfGrp.GetBindValue();
                        parameters[7] = Util.NVC(dgList.GetValue(cell.Row.Index, "PROD_LOTID"));
                        parameters[8] = "CELL";
                        parameters[9] = rdoAll.IsChecked.Equals(true) ? "Y" : "N";

                        C1WindowExtension.SetParameters(wndDetail, parameters);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndDetail.ShowModal()));
                    }
                }
                else if (cell.Column.Name.Equals("LOTID"))
                {
                    FCS001_069_DETAILS_BY_LOT wndDetail = new FCS001_069_DETAILS_BY_LOT();
                    wndDetail.FrameOperation = FrameOperation;

                    if (wndDetail != null)
                    {
                        DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                        
                        object[] parameters = new object[10];
                        if (Util.NVC(dgList.GetValue(cell.Row.Index, "LOTID")).Equals(string.Empty))
                        {
                            parameters[0] = dt.AsEnumerable().Where(r =>
                                !DBNull.Value.Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE")).Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID")).Equals(r.Field<string>("EQSGID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID")).Equals(r.Field<string>("MDLLOT_ID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID")).Equals(r.Field<string>("PROCID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROD_LOTID")).Equals(r.Field<string>("PROD_LOTID"))
                            ).CopyToDataTable();                            
                        }
                        else
                        {
                            parameters[0] = dt.AsEnumerable().Where(r =>
                                !DBNull.Value.Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE")).Equals(r.Field<string>("CALDATE")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID")).Equals(r.Field<string>("EQSGID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID")).Equals(r.Field<string>("MDLLOT_ID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID")).Equals(r.Field<string>("PROCID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "PROD_LOTID")).Equals(r.Field<string>("PROD_LOTID")) &&
                                Util.NVC(dgList.GetValue(cell.Row.Index, "LOTID")).Equals(r.Field<string>("LOTID"))
                            ).CopyToDataTable();
                        }
                        parameters[1] = Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE"));
                        parameters[2] = Util.NVC(dgList.GetValue(cell.Row.Index, "CALDATE"));
                        parameters[3] = Util.NVC(dgList.GetValue(cell.Row.Index, "EQSGID"));
                        parameters[4] = Util.NVC(dgList.GetValue(cell.Row.Index, "MDLLOT_ID"));
                        parameters[5] = Util.NVC(dgList.GetValue(cell.Row.Index, "PROCID"));
                        parameters[6] = cboPerfGrp.GetBindValue();
                        parameters[7] = Util.NVC(dgList.GetValue(cell.Row.Index, "LOTID"));
                        parameters[8] = "CELL";
                        parameters[9] = rdoAll.IsChecked.Equals(true) ? "Y" : "N";

                        C1WindowExtension.SetParameters(wndDetail, parameters);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndDetail.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                            e.Cell.Presenter.Foreground = Brushes.Black; ;
                            e.Cell.Presenter.Background = null;

                            if (searchOption.Equals("AR"))
                            {
                                if (e.Cell.Column.Name.ToString() == "PROD_LOTID" && dg.GetValue(e.Cell.Row.Index, "SORT2").Equals(1))
                                {
                                    e.Cell.Presenter.Foreground = Brushes.Blue;
                                }
                            }
                            else
                            {
                                if (e.Cell.Column.Name.ToString() == "LOTID" && dg.GetValue(e.Cell.Row.Index, "SORT2").Equals(1))
                                {
                                    e.Cell.Presenter.Foreground = Brushes.Blue;
                                }
                            }
                          

                            if (dg.GetValue(e.Cell.Row.Index, "SORT2").Equals(2) && e.Cell.Column.Index > 13)
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(100, 247, 233, 213));
                            }

                            if (dg.GetValue(e.Cell.Row.Index, "SORT2").Equals(3))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(255, 247, 233, 213));
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

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = sender as C1DataGrid;
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = Brushes.Black;
                    }
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
